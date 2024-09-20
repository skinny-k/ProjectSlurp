using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Handles player movement and movement abilities
public class PlayerMovement : MonoBehaviour
{
    [Header("Basic Movement Settings")]
    [SerializeField] float _moveSpeed = 5f;
    [SerializeField] float _turnSpeed = 360f;
    [Tooltip("The steepest slope the player can walk up.")]
    [SerializeField] float _slopeTolerance = 45f;
    [SerializeField] float _groundCheckRadius = 0.85f;
    [SerializeField] float _groundCheckPadding = 0.05f;

    [Header("Gravity")]
    [Tooltip("The force of the custom gravity. It is recommended you use the same value listed in the Physics section of Project Settings.")]
    [SerializeField] Vector3 _gravity = new Vector3(0, -9.81f, 0);

    [Header("Aim Movement Settings")]
    [SerializeField] public float AimSpeedModifier = 0.5f;

    [Header("Air Settings")]
    [Tooltip("The speed reduction applied to the player while in the air.")]
    [SerializeField] float _airSpeedModifier = 1f;
    [SerializeField] float _slowFallVelocityY = 1f;
    [Tooltip("The speed reduction applied to the player while slow falling. Compounds with Air Speed Modifier.")]
    [SerializeField] float _slowFallAirSpeedModifier = 0.5f;

    [Header("Jump Settings")]
    [Tooltip("The number of jumps the player can make.")]
    [SerializeField] int _maxJumps = 2;
    [Tooltip("If the player jumps in the air after falling off a ledge, the number of additional jumps to penalize them by.")]
    [SerializeField] int _airJumpPenalty = 1;
    [SerializeField] float _jumpVelocity = 10f;
    [SerializeField] float _highJumpVelocity = 20f;
    [Tooltip("The amount of grace time the player has after walking off a ledge to make their first jump without penalty.")]
    [SerializeField] float _coyoteTime = 0.15f;

    [Header("Dash Settings")]
    [SerializeField] float _dashDuration = 0.5f;
    [SerializeField] float _dashSpeed = 15f;
    [SerializeField] float _dashAcceleration = 20f;

    [Header("Travel Settings")]
    [SerializeField] float _travelSpeed = 25f;
    [SerializeField] float _travelAcceleration = 35f;

    private InputManager _input;
    private Player _player;
    private PlayerActions _actions;

    private Rigidbody _rb;
    private CapsuleCollider _col;

    private Dictionary<string, float> _speedModifiers = new Dictionary<string, float>();
    private float _netSpeedModifier = 1f;
    private string _airSModKey;
    private string _highJumpSModKey;
    private string _slowFallSModKey;
    
    private int _currentJumps = 0;
    private float _currentAirTime = 0f;

    private Vector3 _targetRot;
    private Vector2 _moveInput;
    private Vector3 _moveDir = Vector3.zero;

    private GroundInfo _ground;
    private RaycastHit _groundHit;
    private float _groundCheckDistance;
    private float _halfHeight;

    private HapticsManager.HapticEventInfo _slowFallHaptics;
    private HapticsManager.HapticEventInfo _travelHaptics;
    
    public bool IsGrounded { get; private set; } = true;
    public bool IsHighJumping { get; private set; } = false;
    public bool IsSlowFalling { get; private set; } = false;
    public bool IsDashing { get; private set; } = false;
    public bool IsTraveling { get; private set; } = false;

    public bool IsInPrivilegedMove => IsDashing || IsTraveling;
    public bool IsInActiveAerial => IsHighJumping || IsSlowFalling;
    
    void Start()
    {
        _input = GetComponent<InputManager>();
        _player = GetComponent<Player>();
        _actions = GetComponent<PlayerActions>();

        _rb = _player.Rb;
        _col = GetComponent<CapsuleCollider>();

        _moveDir = transform.forward;

        _halfHeight = (_col.height / 2) * transform.localScale.y;
        _groundCheckDistance = _halfHeight - _groundCheckRadius + _groundCheckPadding;

        _airSModKey = GetInstanceID() + "_air";
        _highJumpSModKey = GetInstanceID() + "_hij";
        _slowFallSModKey = GetInstanceID() + "_slf";
    }

    void Update()
    {
        // forces the player to look in the direction of the aim camera while aiming
        if (_actions.IsAiming)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(_targetRot), _turnSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        CheckGrounded();
        ApplyMovement();
        ApplyGravity();
        
        if (IsHighJumping)
            UpdateHighJump();
        // UpdatePrivilegedMovement();

        // // dash & travel handling
        // if (IsDashing)
        // {
        //     // accelerates into a dash as necessary
        //     _rb.velocity = Vector3.MoveTowards(_rb.velocity, transform.forward * _dashSpeed, _dashAcceleration);
        // }
        // else if (IsTraveling)
        // {
        //     // accelerates into a travel as necessary
        //     Vector3 dir = (_actions.Weapon.TravelNode.position - transform.position).normalized;
        //     _rb.velocity = Vector3.MoveTowards(_rb.velocity, dir * _travelSpeed, _travelAcceleration);
        // }
    }

    void ApplyMovement()
    {
        // get movement input
        _moveInput = _player.GetMove();

        // if the player is providing movement input and
        // is not in a movement type that prevents other movement
        if (_moveInput.magnitude >= 0.05f && !IsInPrivilegedMove)
        {
            
            // calculate movement input in relation to camera
            Vector2 input = _player.GetMove();
            if (!_player.Camera.IsMovingToDefault)
            {
                Vector3 inputDir = new Vector3(input.x, 0, input.y);
                if (Mathf.Approximately(Mathf.Abs(_player.Camera.GetForward().y), 1.0f))
                {
                    inputDir = new Vector3(input.x, input.y, 0);
                }

                _moveDir = _player.Camera.transform.TransformDirection(inputDir);
            }
            _moveDir.y = 0f; _moveDir.Normalize();

            // rotate player towards the movement direction if they are not aiming
            if (!_actions.IsAiming)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(_moveDir, Vector3.up), _turnSpeed * Time.deltaTime);
            }

            // finalize movement
            if (IsGrounded)
            {
                if (!_ground.Steeper(_slopeTolerance))
                {
                    _rb.AddForce(Vector3.ProjectOnPlane(_moveDir, _ground.normal).normalized * _moveSpeed * _netSpeedModifier * _rb.mass, ForceMode.Force);
                }
            }
            else
            {
                _rb.AddForce(_moveDir * _moveSpeed * _netSpeedModifier * _rb.mass, ForceMode.Force);
            }
        }
    }

    void ApplyGravity()
    {
        if (!IsGrounded)
        {
            if (IsSlowFalling)
            {
                _rb.velocity = new Vector3(_rb.velocity.x, _slowFallVelocityY, _rb.velocity.z);
            }
            else
            {
                _rb.AddForce(_gravity * (IsHighJumping ? 0.5f : 1) * _rb.drag, ForceMode.Acceleration);
            }
        }
    }

    private bool CheckGrounded()
    {
        bool r = Physics.SphereCast(transform.position, _groundCheckRadius, Vector3.down, out _groundHit, _groundCheckDistance, ~LayerMask.NameToLayer("Environment"));
        _ground.UpdateFromRaycastHit(_groundHit);

        // if player hit the ground this physics step
        if (!IsGrounded && r)
        {
            _currentAirTime = 0f;
            HitGround();
        }
        // if player left the ground
        else if (IsGrounded && !r)
        {
            _currentAirTime += Time.fixedDeltaTime;
            AddSpeedModifier(_airSModKey, _airSpeedModifier);
        }
        // if player is inthe air
        else if (!r)
        {
            _currentAirTime += Time.fixedDeltaTime;
        }
        IsGrounded = r;
        return IsGrounded;
    }

    void HitGround()
    {
        _currentJumps = 0;
        IsHighJumping = false;
        SlowFall(false);
        RemoveSpeedModifier(_airSModKey);

        // play haptics
        if (Mathf.Abs(_rb.velocity.y) > _player.HapticsSettings.D_threshold.x)
        {
            float str = Mathf.Clamp((Mathf.Abs(_rb.velocity.y) - _player.HapticsSettings.D_threshold.x) / (_player.HapticsSettings.D_threshold.y - _player.HapticsSettings.D_threshold.x), 0, 1);
            Debug.Log(Mathf.Abs(_rb.velocity.y) + " : " + str);
            HapticsManager.TimedRumble(_player.HapticsSettings.D_strength * str, _player.HapticsSettings.D_duration);
        }

        // prevent any bouncing from happening
        _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
    }

    void OnCollisionEnter(Collision collision)
    {
        // TODO: Add haptic feedback for collision?
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            // forces the player's weapon to return if they hit part of the environment while traveling
            // NOTE: This will also force the travel to end immediately
            if (IsTraveling)
            {
                _actions.ForceWeaponReturn();
            }
        }
    }

    public void Jump()
    {
        // if the player is not dashing or traveling and has jumps remaining
        if (!IsInPrivilegedMove && _currentJumps < _maxJumps)
        {
            // increment jumps -- logic should cover all cases
            // if the player is grounded or is not making their first jump or is making their first jump but has coyote time left
            if (IsGrounded || _currentJumps != 0 || _currentAirTime <= _coyoteTime)
            {
                _currentJumps++;
            }
            // if the player is making their first jump from the air without coyote time
            else if (!IsGrounded && _currentJumps == 0 && _currentAirTime > _coyoteTime)
            {
                // add the air jump penalty
                _currentJumps = _airJumpPenalty + 1;
            }

            _rb.velocity = new Vector3(_rb.velocity.x, _jumpVelocity, _rb.velocity.z);
            Debug.Log("Jump");
        }
    }

    public void HighJump()
    {
        // if the player is not aiming and is stationary on the ground...
        if (!_actions.IsAiming && IsGrounded && !IsInPrivilegedMove && _input.GetInputValueAsVector2("Move") == Vector2.zero)
        {
            // apply jump force
            _rb.velocity = Vector3.up * _highJumpVelocity;
            _currentJumps = _maxJumps;
            IsHighJumping = true;
            AddSpeedModifier(_highJumpSModKey, 0);

            Debug.Log("High Jump");
            HapticsManager.TimedRumble(_player.HapticsSettings.F_strength, _player.HapticsSettings.F_duration);
        }
    }

    private void UpdateHighJump()
    {
        // decreases the player's speed when they start a high jump
        // this speed returns as the player reaches the apex of their jump
        // if the resulting speed modifier is 1, the player is at the apex of the jump, so end high jump
        if (SetSpeedModifier(_highJumpSModKey, Mathf.Clamp((_highJumpVelocity - _rb.velocity.y) / _highJumpVelocity, 0, 1)) >= 1f)
        {
            RemoveSpeedModifier(_highJumpSModKey);
            IsHighJumping = false;
        }
    }

    public void SlowFall(bool isSlowFallHeld)
    {
        // if the player is not aiming or in a privileged move and the new slow fall state would be a change
        if (!_actions.IsAiming && !IsInPrivilegedMove && IsSlowFalling != isSlowFallHeld)
        {
            IsSlowFalling = isSlowFallHeld;

            // if the player is starting a slow fall
            if (IsSlowFalling)
            {
                // apply a constant downward velocity
                _rb.velocity = new Vector3(_rb.velocity.x, _slowFallVelocityY, _rb.velocity.z);

                // add slow fall speed modifier
                AddSpeedModifier(_slowFallSModKey, _slowFallAirSpeedModifier);
                _slowFallHaptics = HapticsManager.StartRumble(_player.HapticsSettings.G_strength);
                Debug.Log("Start Slow Fall");
            }
            // if the player is ending a slow fall
            else
            {
                RemoveSpeedModifier(_slowFallSModKey);
                HapticsManager.StopRumble(_slowFallHaptics);
                Debug.Log("End Slow Fall");
            }
        }
    }

    public void Dash()
    {
        // if (!_actions.IsAiming && !IsHighJumping && !IsTraveling)
        // {
        //     StartCoroutine(DoDash());
        //     Debug.Log("Dash");
        // }
    }

    // private IEnumerator DoDash()
    // {
    //     SlowFall(false);
    //     IsDashing = true;

    //     yield return new WaitForSeconds(_dashDuration);

    //     IsDashing = false;
    // }

    public void Travel()
    {
        // if (!IsTraveling && _actions.Weapon.CanTravel())
        // {
        //     IsTraveling = true;
        //     IsGrounded = false;
        //     Debug.Log("Start Travel");

        //     _travelHaptics = HapticsManager.StartRumble(_player.HapticsSettings.I_strength);
        // }
    }

    public void EndTravel()
    {
        // if (IsTraveling)
        // {
        //     _rb.velocity = Vector3.zero;
        //     IsTraveling = false;
        //     IsGrounded = false;
        //     Debug.Log("End Travel");

        //     HapticsManager.StopRumble(_travelHaptics);
        //     HapticsManager.TimedRumble(_player.HapticsSettings.I_impact, _player.HapticsSettings.I_duration);
        // }
    }

    public void UpdatePrivilegedMovement()
    {
        //
    }

    // helper functions to forcibly adjust the rotation the player should be facing in
    // used when rotating the player while aiming
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

    private void RecalculateNetSpeedModifier()
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

    internal struct GroundInfo
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
