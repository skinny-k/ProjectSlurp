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
    [SerializeField] float _slopeTolerance = 45f;
    [SerializeField] float _acceleration = 5f;

    [Header("Aim Movement Settings")]
    [SerializeField] public float AimSpeedModifier = 0.5f;

    [Header("Air Settings")]
    [SerializeField] float _airSpeedModifier = 1f;
    [SerializeField] float _slowFallVelocityY = 1f;
    [SerializeField] float _slowFallAirSpeedModifier = 0.5f;

    [Header("Jump Settings")]
    [SerializeField] int _maxJumps = 2;
    [SerializeField] float _jumpForce = 10f;
    [SerializeField] float _highJumpForce = 20f;

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

    private List<float> _speedModifiers = new List<float>();
    private float _netSpeedModifier = 1f;
    
    private int _currentJumps = 0;

    private Vector3 _targetRot;
    private Vector2 _moveInput;
    private Vector3 _moveDir = Vector3.zero;
    private RaycastHit _moveHit;
    private GroundInfo _ground;
    private RaycastHit _groundHit;

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

        _moveDir = transform.forward;
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
        // high jump handling
        if (IsHighJumping && _player.Rb.velocity.y > 0)
        {
            // decreases the player's speed when they start a high jump
            // this speed returns as the player reaches the apex of their jump
            // NOTE: This speed modifier is not added the the list of speed modifiers so that it is easier to
            //       remove last frame's speed modifier. However, this means that the net speed modifier must
            //       be recalculated on each frame of a high jump as well. 1-frame changes in net speed
            //       modifier may occur, but should be negligible.
            RecalculateNetSpeedModifier();
            _netSpeedModifier *= Mathf.Clamp((8f - _player.Rb.velocity.y) / 12f, 0, 1);
        }
        else if (IsHighJumping && _player.Rb.velocity.y < 0)
        {
            // a high jump ends once a player reaches the apex of their high jump
            IsHighJumping = false;
        }

        // input and slope handling
        GetCurrentGround();
        // get movement input
        _moveInput = _player.GetMove();
        // if the player is providing movement input
        if (_moveInput.magnitude >= 0.05f)
        {
            // if the player is not dashing or traveling, which would prevent other types of movement...
            if (!IsInPrivilegedMove)
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
                if (!_ground.Steeper(_slopeTolerance))
                {
                    if (IsGrounded)
                    {
                        _player.Rb.velocity = Vector3.MoveTowards(_player.Rb.velocity, Vector3.ProjectOnPlane(_moveDir, _ground.normal) * _moveSpeed * _netSpeedModifier, _acceleration);
                    }
                    else
                    {
                        _player.Rb.velocity = Vector3.MoveTowards(_player.Rb.velocity, (new Vector3(_moveDir.x, 0, _moveDir.z) * _moveSpeed * _netSpeedModifier) + (Vector3.up * _player.Rb.velocity.y), _acceleration);
                    }
                }
            }
        }
        else
        {
            if (IsGrounded)
            {
                _player.Rb.velocity = Vector3.MoveTowards(_player.Rb.velocity, Vector3.zero, _acceleration);
            }
            else
            {
                _player.Rb.velocity = Vector3.MoveTowards(_player.Rb.velocity, Vector3.up * _player.Rb.velocity.y, _acceleration);
            }
        }

        // dash, travel & deceleration handling
        if (IsDashing)
        {
            // accelerates into a dash as necessary
            _player.Rb.velocity = Vector3.MoveTowards(_player.Rb.velocity, transform.forward * _dashSpeed, _dashAcceleration);
        }
        else if (IsTraveling)
        {
            // accelerates into a travel as necessary
            Vector3 dir = (_actions.PlayerWeapon.TravelNode.position - transform.position).normalized;
            _player.Rb.velocity = Vector3.MoveTowards(_player.Rb.velocity, dir * _travelSpeed, _travelAcceleration);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            // forces the player's weapon to return if they hit part of the environment while traveling
            // NOTE: This will also force the travel to end immediately
            if (IsTraveling)
            {
                _actions.ForceWeaponReturn();
            }
            
            // resets the player's jumps and aerial movement abilities if they touched the ground
            if (!IsGrounded && GetCurrentGround())
            {
                _currentJumps = 0;
                IsGrounded = true;
                IsHighJumping = false;
                SlowFall(false);

                _player.Rb.velocity = new Vector3(_player.Rb.velocity.x, 0, _player.Rb.velocity.z);

                float hapticStrength = collision.relativeVelocity.magnitude < _player.HapticsSettings.D_threshold ? _player.HapticsSettings.D_strength_1 : _player.HapticsSettings.D_strength_2;
                HapticsManager.TimedRumble(hapticStrength, _player.HapticsSettings.D_duration);
            }
        }
    }

    // forcibly rotates the player
    // useful when rotating the player while aiming
    public void AddRotation(Vector2 input, float sensitivity)
    {
        _targetRot += new Vector3(0, input.x * sensitivity * Time.deltaTime, 0);
    }

    public void RotateTo(Vector3 rotation)
    {
        // sets the rotation the player should be facing in
        // this helps rotational movement look a little more smooth
        _targetRot = rotation;
    }

    public void Jump()
    {
        // if the player is not dashing or traveling and has remaining jumps...
        if (!IsInPrivilegedMove && _currentJumps < _maxJumps)
        {
            // the player should lose one jump when they walk off an edge
            // TODO: rework this so it will work when jumps are increased
            if (!IsGrounded)
            {
                _currentJumps = _maxJumps;
            }
            else 
            {
                // reduce speed in air
                // TODO: rework this so that it will apply when the player walks off of an edge
                if (_airSpeedModifier != 1f)
                {
                    AddSpeedModifier(_airSpeedModifier);
                }
                _currentJumps++;
            }
            
            // add jump force
            _player.Rb.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
            IsGrounded = false;

            Debug.Log("Jump");
        }
    }

    public void SlowFall(bool isSlowFallHeld)
    {
        // if the player is not aiming, is not traveling and the new slow fall state would be a change
        if (!_actions.IsAiming && IsSlowFalling != isSlowFallHeld && !IsTraveling)
        {
            IsSlowFalling = isSlowFallHeld;

            // if the player is starting a slow fall...
            if (IsSlowFalling)
            {
                // disable gravity and apply a constant downward velocity
                _player.Rb.useGravity = false;
                _player.Rb.velocity = new Vector3(_player.Rb.velocity.x, -1 * _slowFallVelocityY, _player.Rb.velocity.z);

                // reduce speed in air, making sure that we're not doubling up first
                if (_slowFallAirSpeedModifier != 1f)
                {
                    RemoveSpeedModifier(_airSpeedModifier);
                    AddSpeedModifier(_slowFallAirSpeedModifier);
                }

                _slowFallHaptics = HapticsManager.StartRumble(_player.HapticsSettings.G_strength);
            }
            else // if the player is ending a slow fall...
            {
                // turn gravity back on
                // NOTE: slow fall speed should be less than terminal velocity, so we don't need to set vertical velocity
                _player.Rb.useGravity = true;
                
                // if the player is still in the air, make sure the normal air speed modifier is active
                RemoveSpeedModifier(_slowFallAirSpeedModifier);
                if (_airSpeedModifier != 1f && !IsGrounded && !_speedModifiers.Contains(_airSpeedModifier))
                    AddSpeedModifier(_airSpeedModifier);

                HapticsManager.StopRumble(_slowFallHaptics);
            }

            string msg = "Start ";
            if (!IsSlowFalling)
                msg = "End ";
            Debug.Log(msg + "Slow Fall");
        }
    }

    public void HighJump()
    {
        // if the player is not aiming and is stationary on the ground...
        if (!_actions.IsAiming && IsGrounded && !IsInPrivilegedMove && _input.GetInputValueAsVector2("Move") == Vector2.zero)
        {
            // TODO: consolidate air speed modifier applications as much as possible
            if (_airSpeedModifier != 1f)
            {
                AddSpeedModifier(_airSpeedModifier);
            }
            
            // apply jump force
            _player.Rb.AddForce(Vector3.up * _highJumpForce, ForceMode.VelocityChange);
            _currentJumps = _maxJumps;
            IsGrounded = false;
            IsHighJumping = true;

            Debug.Log("High Jump");
            HapticsManager.TimedRumble(_player.HapticsSettings.F_strength, _player.HapticsSettings.F_duration);
        }
    }

    public void Dash()
    {
        if (!_actions.IsAiming && !IsHighJumping && !IsTraveling)
        {
            StartCoroutine(DoDash());
            Debug.Log("Dash");
        }
    }

    public void Travel()
    {
        if (!IsTraveling && _actions.PlayerWeapon.CanTravel())
        {
            IsTraveling = true;
            IsGrounded = false;
            Debug.Log("Start Travel");

            _travelHaptics = HapticsManager.StartRumble(_player.HapticsSettings.I_strength);
        }
    }

    public void EndTravel()
    {
        if (IsTraveling)
        {
            _player.Rb.velocity = Vector3.zero;
            IsTraveling = false;
            IsGrounded = false;
            Debug.Log("End Travel");

            HapticsManager.StopRumble(_travelHaptics);
            HapticsManager.TimedRumble(_player.HapticsSettings.I_impact, _player.HapticsSettings.I_duration);
        }
    }

    private bool GetCurrentGround()
    {
        bool r = Physics.SphereCast(transform.position + Vector3.up * 2.7f, 0.9f, Vector3.down, out _groundHit, 2.7f, ~LayerMask.NameToLayer("Environment"));
        _ground.UpdateFromRaycastHit(_groundHit);
        if (IsGrounded && !r)
        {
            IsGrounded = false;
        }
        return r && _ground.angle <= _slopeTolerance;
    }

    // helper functions to apply and remove multiple speed modifiers more easily
    public void AddSpeedModifier(float modifier)
    {
        _speedModifiers.Add(modifier);
        RecalculateNetSpeedModifier();
    }

    public IEnumerator AddSpeedModifierWithDuration(float modifier, float duration)
    {
        AddSpeedModifier(modifier);

        yield return new WaitForSeconds(duration);

        RemoveSpeedModifier(modifier);
    }

    public void RemoveSpeedModifier(float modifier)
    {
        _speedModifiers.Remove(modifier);
        RecalculateNetSpeedModifier();
    }

    private void RecalculateNetSpeedModifier()
    {
        _netSpeedModifier = 1f;
        foreach (float modifier in _speedModifiers)
        {
            _netSpeedModifier *= modifier;
            if (_netSpeedModifier == 0)
            {
                break;
            }
        }
    }

    private IEnumerator DoDash()
    {
        SlowFall(false);
        IsDashing = true;

        yield return new WaitForSeconds(_dashDuration);

        IsDashing = false;
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
