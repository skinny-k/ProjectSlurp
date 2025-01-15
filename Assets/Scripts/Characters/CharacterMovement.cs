using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterMovement : MonoBehaviour
{
    [Header("Basic Movement Settings")]
    [SerializeField] protected float _moveSpeed = 5f;
    [SerializeField] protected float _turnSpeed = 360f;
    [Tooltip("The steepest slope the character can walk up.")]
    [SerializeField][Range(0f, 90f)] protected float _slopeTolerance = 45f;
    [SerializeField] protected float _groundCheckRadius = 0.85f;
    [SerializeField] protected float _groundCheckPadding = 0.05f;

    [Header("Gravity")]
    [Tooltip("The force of the custom gravity. It is recommended you use the same value listed in the Physics section of Project Settings.")]
    [SerializeField] protected Vector3 _gravity = new Vector3(0, -9.81f, 0);

    [Header("Air Settings")]
    [Tooltip("The speed reduction applied to the character while in the air.")]
    [SerializeField][Range(0f, 1f)] protected float _airSpeedModifier = 1f;

    public event Action OnHitGround;
    
    protected Rigidbody _rb;
    protected CapsuleCollider _col;
    
    protected Dictionary<string, float> _speedModifiers = new Dictionary<string, float>();
    protected float _netSpeedModifier = 1f;
    protected string _airSModKey;

    protected Vector3 _targetRot;
    
    protected GroundInfo _ground;
    protected RaycastHit _groundHit;
    protected float _groundCheckDistance;
    protected float _halfHeight;

    public bool IsGrounded { get; protected set; } = true;
    
    // while most other functions will use largely the same functionality,
    // movement will be applied in almost completely different manners between NPCs and players
    abstract protected void ApplyMovement();
    
    virtual protected void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<CapsuleCollider>();
        
        _halfHeight = (_col.height / 2) * transform.localScale.y;
        _groundCheckDistance = _halfHeight - _groundCheckRadius + _groundCheckPadding;

        _airSModKey = GetInstanceID() + "_air";
    }

    virtual protected void FixedUpdate()
    {
        CheckGrounded();
        
        ApplyMovement();
        ApplyGravity();
    }

    virtual protected void ApplyGravity()
    {
        if (!IsGrounded)
        {
            _rb.AddForce(_gravity * _rb.drag, ForceMode.Acceleration);
        }
    }

    virtual protected bool CheckGrounded(bool forced = false, float ySpeed = 0f)
    {
        bool r = Physics.SphereCast(transform.position, _groundCheckRadius, Vector3.down, out _groundHit, _groundCheckDistance, ~LayerMask.NameToLayer("Environment"), QueryTriggerInteraction.Ignore);
        _ground.UpdateFromRaycastHit(_groundHit);

        // if character hit the ground this physics step
        if (!IsGrounded && r && !_ground.Steeper(_slopeTolerance))
        {
            HitGround(forced ? ySpeed : Mathf.Abs(_rb.velocity.y));
        }
        // if character left the ground
        else if (IsGrounded && !r)
        {
            AddSpeedModifier(_airSModKey, _airSpeedModifier);
        }
        IsGrounded = r && !_ground.Steeper(_slopeTolerance);
        return IsGrounded;
    }

    virtual protected void HitGround(float ySpeed)
    {
        RemoveSpeedModifier(_airSModKey);

        // prevent any bouncing from happening
        _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);

        Debug.Log("Hit the Ground!");
        OnHitGround?.Invoke();
    }

    virtual protected void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.isTrigger && collision.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            CheckGrounded(true, Mathf.Abs(collision.relativeVelocity.y));
        }
    }

    // helper functions to forcibly adjust the rotation the character should be facing in
    // rotation is not actually implemenm
    public void AddRotation(Vector2 input, float sensitivity)
    {
        _targetRot += new Vector3(0, input.x * sensitivity * Time.deltaTime, 0);
    }

    public void RotateTo(Vector3 rotation)
    {
        _targetRot = rotation;
    }

    // helper functions to apply and remove multiple speed modifiers more easily
    public bool AddSpeedModifier(string key, float modifier)
    {
        try
        {
            _speedModifiers.Add(key, modifier);
        }
        // modifier with key already exists
        catch (ArgumentException ex)
        {
            Debug.LogWarning("Speed modifier with key '" + key + "' already exists. Logging warning:\n" + ex.Message);
            return false;
        }
        RecalculateNetSpeedModifier();
        return true;
    }

    public IEnumerator AddSpeedModifierWithDuration(string key, float modifier, float duration)
    {
        if (AddSpeedModifier(key, modifier))
        {
            yield return new WaitForSeconds(duration);

            RemoveSpeedModifier(key);
        }
        yield return null;
    }

    public float SetSpeedModifier(string key, float modifier)
    {
        _speedModifiers[key] = modifier;
        RecalculateNetSpeedModifier();
        return _speedModifiers[key];
    }

    public void RemoveSpeedModifier(string key)
    {
        _speedModifiers.Remove(key);
        RecalculateNetSpeedModifier();
    }

    virtual protected void RecalculateNetSpeedModifier()
    {
        _netSpeedModifier = 1f;
        foreach (float modifier in _speedModifiers.Values)
        {
            // don't bother calculating if the modifier is one
            if (modifier == 1f)
            {
                continue;
            }

            _netSpeedModifier *= modifier;
            // if _netSpeedModifier is ever 0, it won't ever increase, so end the calculation
            if (_netSpeedModifier == 0)
            {
                return;
            }
        }
    }

    protected struct GroundInfo
    {
        public bool hit;
        public Collider collider;
        public Vector3 point;
        public Vector3 normal;
        public float angle;

        public void UpdateFromRaycastHit(RaycastHit hit)
        {
            this.hit = hit.transform != null;
            this.collider = hit.collider;
            this.point = hit.point;
            normal = hit.normal;
            angle = Vector3.Angle(normal, Vector3.up);
        }

        public bool Steeper(float angle)
        {
            return hit && this.angle > angle;
        }
    }
}
