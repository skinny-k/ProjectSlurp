using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

[RequireComponent(typeof(Rigidbody))]
public class PlayerWeapon : MonoBehaviour, IThrowable, IReturnable
{
    [Header("Visual Settings")]
    [SerializeField] Vector3 _baseRotation = new Vector3(-70, 0, 0);
    [SerializeField] Vector3 _aimedRotation = new Vector3(-20, 0, 0);
    [SerializeField] Vector3 _holdPosition = new Vector3(1.4f, 0.6f, 0);

    [Header("Other")]
    [Tooltip("The time in seconds that it should take for the weapon to return to the owning player.")]
    [SerializeField] float _returnTime = 0.25f;
    [Tooltip("The distance from the player's hand the weapon should consider to be returned.")]
    [SerializeField] float _returnThreshold = 0.15f;

    // TODO: enable/disable hurt box based on weapon context
    private DamageVolume _hurtbox;
    private ParentConstraint _constraint;
    private Rigidbody _rb;
    private Quaternion _throwRotation;
    private Vector3 _lastThrownFrom;
    private float _returnSpeed;

    public PlayerActions Owner;
    public Transform TravelNode { get; private set; }
    public float ThrowSpeed { get; private set; } = 4f;
    public float MaxThrowDistance { get; private set; } = 30f;
    public float ReturnTime => _returnTime;
    public bool IsAiming { get; private set; } = false;
    public bool IsHeld { get; private set; } = true;
    public bool IsThrown { get; private set; } = false;
    public bool IsReturning { get; private set; } = false;

    void Start()
    {
        // initialize weapon settings
        _rb = GetComponent<Rigidbody>();
        _hurtbox = transform.Find("Hurt Box").GetComponent<DamageVolume>();
        _constraint = GetComponent<ParentConstraint>();
        TravelNode = transform.Find("Travel Node");

        transform.localRotation = Quaternion.Euler(_baseRotation);
    }

    void Update()
    {
        // if the weapon is being held, not thrown...
        // NOTE: IsHeld and IsThrown should never be true at the same time, so checking one should be sufficient to
        //       know the other, but it can't hurt to check both just in case
        if (IsHeld && !IsThrown)
        {
            Vector3 targetRot = Vector3.zero;
            if (IsAiming)
            {
                targetRot = _aimedRotation;
            }
            else
            {
                targetRot = _baseRotation;
            }

            // rotate toward the target rotation as necessary
            if (_constraint.GetRotationOffset(0) != targetRot)
            {
                _constraint.SetRotationOffset(0, Vector3.Slerp(_constraint.GetRotationOffset(0), targetRot, 0.02f));
            }
        }
        else if (!IsHeld && IsThrown)
        {
            // rotate toward the throw rotation
            transform.localRotation = Quaternion.Slerp(transform.localRotation, _throwRotation, 0.75f);

            // if the weapon has reached its maximum distance, boomerang back to the weapon's owner
            if (Vector3.Distance(transform.position, _lastThrownFrom) > MaxThrowDistance)
            {
                ReturnTo(Owner);
            }
        }
        
        if (IsReturning)
        {
            // return to the owner's hand, not the owner itself
            // NOTE: this must be repeated on update because the player could move after the weapon is recalled
            Vector3 returnPos = Owner.transform.position + _holdPosition;
            // point towards the return target
            Vector3 lookDir = (transform.position - returnPos).normalized;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.LookRotation(lookDir), 0.75f);
            // accelerate towards the return target
            _rb.velocity = (returnPos - transform.position).normalized * _returnSpeed;

            if (Vector3.Distance(returnPos, transform.position) <= _returnThreshold)
            {
                ReparentTo(Owner.GetComponent<Player>());
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsHeld)
        {
            // if the weapon was on its way out when it collided...
            if (IsThrown && !IsReturning)
            {
                // TODO: Still having weird stuff with this for rotated objects
                // stop and parent the weapon to the hit object
                _rb.velocity = Vector3.zero;

                if (_constraint.sourceCount > 0)
                {
                    _constraint.RemoveSource(0);
                }
                ConstraintSource source = new ConstraintSource();
                source.sourceTransform = collision.transform; source.weight = 1;
                _constraint.AddSource(source);
                _constraint.SetTranslationOffset(0, transform.position - collision.transform.position);
                _constraint.SetRotationOffset(0, transform.rotation.eulerAngles - collision.transform.rotation.eulerAngles);

                IsThrown = false;
            }
            else if (collision.gameObject.GetComponent<Player>() != null) // if the weapon touched the player...
            {
                Player player = collision.gameObject.GetComponent<Player>();
                ReparentTo(player);
            }
        }
    }
    
    public void Aim(bool state)
    {
        IsAiming = state;
    }
    
    // NOTE: speed is inherited from IThrowable, but is not used in this implementation
    public void Throw(Vector3 dir, float speed = 0)
    {
        if (_constraint.sourceCount > 0)
        {
            _constraint.RemoveSource(0);
        }
        
        // track where the weapon was thrown from
        // this is useful for the boomerang effect that the weapon has
        _lastThrownFrom = transform.position;
        
        // enable collision
        GetComponent<Collider>().enabled = true;
        _rb.isKinematic = false;

        // rotate towards the direction of the throw and accelerate in that direction
        transform.parent = null;
        _throwRotation = Quaternion.LookRotation(dir, Vector3.Cross(dir, Vector3.right));
        _rb.velocity = dir * ThrowSpeed;
        IsHeld = false;
        IsThrown = true;
    }

    public void ReturnTo(PlayerActions player)
    {
        if (Owner != player)
        {
            Owner = player;
        }

        if (_constraint.sourceCount > 0)
        {
            _constraint.RemoveSource(0);
        }

        // return to the player
        // NOTE: speed is non-constant to ensure that the weapon always returns in a predictable amount of time
        IsReturning = true;
        _rb.isKinematic = false;
        _returnSpeed = Vector3.Distance(transform.position, player.transform.position) / _returnTime;
    }

    public bool CanTravel()
    {
        return !IsHeld && !IsReturning & !IsThrown;
    }

    private void ReparentTo(Player player)
    {
        player.GetComponent<PlayerActions>().CollectWeapon();

        GetComponent<Collider>().enabled = false;

        if (_constraint.sourceCount > 0)
        {
            _constraint.RemoveSource(0);
        }
        ConstraintSource source = new ConstraintSource();
        source.sourceTransform = Owner.transform; source.weight = 1;
        _constraint.AddSource(source);
        _constraint.SetTranslationOffset(0, _holdPosition);
        _constraint.SetRotationOffset(0, _constraint.rotationAtRest);
        
        _rb.velocity = Vector3.zero;
        _rb.isKinematic = true;

        IsHeld = true;
        IsThrown = false;
        IsReturning = false;
    }
}
