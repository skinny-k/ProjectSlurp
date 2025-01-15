using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Handles player movement and movement abilities
public class PlayerMovement : CharacterMovement
{ 
    [Header("Player Air Settings")]
    [Tooltip("The force of the custom gravity. It is recommended you use the same value listed in the Physics section of Project Settings.")]
    [SerializeField] float _highJumpGravityModifier = 0.5f;
    [SerializeField] float _diveGravityModifier = 4f;
    [SerializeField] float _slowFallVelocityY = 1f;
    [Tooltip("The speed reduction applied to the player while slow falling. Compounds with Air Speed Modifier.")]
    [SerializeField][Range(0f, 1f)] float _slowFallAirSpeedModifier = 0.5f;

    [Header("Aim Movement Settings")]
    [SerializeField][Range(0f, 1f)] public float AimSpeedModifier = 0.5f;

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
    [SerializeField] float _dashVelocity = 15f;
    [SerializeField] AnimationCurve _dashCurve;
    [SerializeField] float _dashDuration = 0.5f;

    [Header("Travel Settings")]
    [SerializeField] float _travelSpeed = 25f;
    [SerializeField] float _travelAcceleration = 35f;
    [SerializeField][Range(0f, 1f)] float _travelSpeedConservation = 0.5f;

    private InputManager _input;
    private Player _player;
    private PlayerActions _actions;

    private string _highJumpSModKey;
    private string _slowFallSModKey;
    
    private int _currentJumps = 0;
    private float _currentAirTime = 0f;

    private Vector2 _moveInput;
    private Vector3 _moveDir = Vector3.zero;

    private bool _canDash = true;
    private Vector3 _dashDir;
    private float _dashTimer = 0f;

    private HapticsManager.HapticEventInfo _slowFallHaptics;
    private HapticsManager.HapticEventInfo _travelHaptics;
    
    public bool IsHighJumping { get; private set; } = false;
    public bool IsSlowFalling { get; private set; } = false;
    public bool IsDashing { get; private set; } = false;
    public bool IsTraveling { get; private set; } = false;
    public bool IsDiving { get; private set; } = false;

    public bool IsInPrivilegedMove => IsDashing || IsTraveling;
    public bool IsInActiveAerial => IsHighJumping || IsSlowFalling;
    
    override protected void Start()
    {
        base.Start();
        
        _input = GetComponent<InputManager>();
        _player = GetComponent<Player>();
        _actions = GetComponent<PlayerActions>();

        _moveDir = transform.forward;

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

    override protected void FixedUpdate()
    {
        CheckGrounded();

        if (IsInPrivilegedMove)
        {
            UpdatePrivilegedMove();
        }
        else
        {
            ApplyMovement();
            ApplyGravity();
            
            if (IsHighJumping)
            {
                UpdateHighJump();
            }
        }
    }

    override protected void ApplyMovement()
    {
        // get movement input
        _moveInput = _player.GetMove();

        // if the player is providing movement input and
        // is not in a movement type that prevents other movement
        if (_moveInput.magnitude >= 0.05f)
        {
            // calculate movement input in relation to camera
            _moveDir = _player.Camera.InputToCameraDirection(_moveInput, _moveDir);

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

    override protected void ApplyGravity()
    {
        if (!IsGrounded)
        {
            if (IsSlowFalling)
            {
                _rb.velocity = new Vector3(_rb.velocity.x, _slowFallVelocityY, _rb.velocity.z);
            }
            else
            {
                _rb.AddForce(_gravity * (IsDiving ? _diveGravityModifier : 1) * (IsHighJumping ? _highJumpGravityModifier : 1) * _rb.drag, ForceMode.Acceleration);
            }
        }
    }

    override protected bool CheckGrounded(bool forced = false, float ySpeed = 0f)
    {
        if (!base.CheckGrounded(forced, ySpeed))
        {
            _currentAirTime += Time.fixedDeltaTime;
        }
        return IsGrounded;
    }

    override protected void HitGround(float ySpeed)
    {
        _currentJumps = 0;
        IsHighJumping = false;
        SlowFall(false);

        _canDash = true;

        // play haptics
        if (ySpeed > _player.HapticsSettings.D_threshold.x)
        {
            float str = Mathf.Clamp((ySpeed - _player.HapticsSettings.D_threshold.x) / (_player.HapticsSettings.D_threshold.y - _player.HapticsSettings.D_threshold.x), 0, 1);
            HapticsManager.TimedRumble(_player.HapticsSettings.D_strength * str, _player.HapticsSettings.D_duration);
        }

        base.HitGround(ySpeed);
    }

    override protected void OnCollisionEnter(Collision collision)
    {
        // TODO: Add haptic feedback for collision?
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            CheckGrounded(true, Mathf.Abs(collision.relativeVelocity.y));
            
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

    void UpdatePrivilegedMove()
    {
        if (IsDashing)
        {
            UpdateDash();
        }
        else if (IsTraveling)
        {
            UpdateTravel();
        }
    }

    public void Dash()
    {
        if (_canDash && !_actions.IsAiming && !IsHighJumping && !IsTraveling)
        {
            // dash in the direction of input. if no input, dash forward instead
            if (_player.GetMove().magnitude >= 0.05f)
            {
                _dashDir = _player.Camera.InputToCameraDirection(_player.GetMove(), transform.forward);
            }
            else
            {
                _dashDir = transform.forward;
                _dashDir.y = 0;
                _dashDir.Normalize();
            }
            
            StartCoroutine(DoDash());
            Debug.Log("Dash");
        }
    }

    private IEnumerator DoDash()
    {
        SlowFall(false);
        _dashTimer = 0f;
        IsDashing = true;

        yield return new WaitForSeconds(_dashDuration);

        IsDashing = false;
        _canDash = IsGrounded;
    }

    void UpdateDash()
    {
        _dashTimer += Time.fixedDeltaTime;
        _rb.velocity = _dashDir * _dashVelocity * _dashCurve.Evaluate(_dashTimer / _dashDuration);
    }

    public void Travel()
    {
        if (!IsTraveling && _actions.Weapon.CanTravel())
        {
            IsTraveling = true;
            Debug.Log("Start Travel");

            _travelHaptics = HapticsManager.StartRumble(_player.HapticsSettings.I_strength);
        }
    }

    public void EndTravel()
    {
        if (IsTraveling)
        {
            _rb.velocity *= Mathf.Clamp(_travelSpeedConservation, 0f, 1f);
            IsTraveling = false;
            Debug.Log("End Travel");

            HapticsManager.StopRumble(_travelHaptics);
            HapticsManager.TimedRumble(_player.HapticsSettings.I_impact, _player.HapticsSettings.I_duration);
        }
    }

    void UpdateTravel()
    {
        Vector3 dir = (_actions.Weapon.TravelNode.position - transform.position).normalized;
        _rb.velocity = Vector3.MoveTowards(_rb.velocity, dir * _travelSpeed, _travelAcceleration * Time.deltaTime);
    }

    // helper function for diving
    // most dive behavior is in PlayerActions, but since diving is also movement, some info is stored here for cohesion
    public void SetDiving(bool state)
    {
        IsDiving = state;
    }
}
