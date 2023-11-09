using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Basic Movement Settings")]
    [SerializeField] float _moveSpeed = 5f;
    [SerializeField] float _turnSpeed = 360f;

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
    private Rigidbody _rb;

    private List<float> _speedModifiers = new List<float>();
    private float _netSpeedModifier = 1f;
    
    private int _currentJumps = 0;

    private Vector3 _targetRot;
    private Vector3 _moveDir = Vector3.zero;
    
    public bool IsGrounded { get; private set; } = true;
    public bool IsHighJumping { get; private set; } = false;
    public bool IsSlowFalling { get; private set; } = false;
    public bool IsDashing { get; private set; } = false;
    public bool IsTraveling { get; private set; } = false;

    public bool CanMove => !IsDashing && !IsTraveling;
    public bool IsInActiveAerial => IsHighJumping && IsSlowFalling;

    void Start()
    {
        _input = GetComponent<InputManager>();
        _player = GetComponent<Player>();
        _actions = GetComponent<PlayerActions>();
        _rb = GetComponent<Rigidbody>();

        _moveDir = transform.forward;
    }

    void Update()
    {
        if (_actions.IsAiming)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(_targetRot), _turnSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (IsHighJumping && _rb.velocity.y > 0)
        {
            RecalculateNetSpeedModifier();
            _netSpeedModifier *= Mathf.Clamp((8f - _rb.velocity.y) / 12f, 0, 1);
        }
        else if (IsHighJumping && _rb.velocity.y < 0)
        {
            IsHighJumping = false;
        }
        
        if (IsDashing)
        {
            _rb.velocity = Vector3.MoveTowards(_rb.velocity, transform.forward * _dashSpeed, _dashAcceleration * Time.deltaTime);
        }
        else if (!IsTraveling)
        {
            _rb.velocity = Vector3.MoveTowards(_rb.velocity, new Vector3(0, _rb.velocity.y, 0), _dashAcceleration * Time.deltaTime);
        }

        if (IsTraveling)
        {
            Vector3 dir = (_actions.PlayerWeapon.TravelNode.position - transform.position).normalized;
            _rb.velocity = Vector3.MoveTowards(_rb.velocity, dir * _travelSpeed, _travelAcceleration * Time.deltaTime);
        }
        else if (!IsDashing)
        {
            _rb.velocity = Vector3.MoveTowards(_rb.velocity, new Vector3(0, _rb.velocity.y, 0), _travelAcceleration * Time.deltaTime);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Walkable" && collision.transform.position.y < transform.position.y)
        {
            _currentJumps = 0;
            IsGrounded = true;
            IsHighJumping = false;
            SlowFall(false);
        }
    }

    public void AddRotation(Vector2 input, float sensitivity)
    {
        _targetRot += new Vector3(0, input.x * sensitivity * Time.deltaTime, 0);
    }

    public void RotateTo(Vector3 rotation)
    {
        _targetRot = rotation;
    }
    
    public void Move(Vector2 input)
    {
        if (CanMove)
        {
            input = input.normalized;
            float speedThisFrame = _moveSpeed * _netSpeedModifier;
            if (!_player.Camera.IsMovingToDefault)
            {
                _moveDir = _player.Camera.transform.TransformVector(new Vector3(input.x, 0, input.y));
            }
            _moveDir = (new Vector3(_moveDir.x, 0, _moveDir.z)).normalized;
            transform.position += _moveDir * speedThisFrame * Time.deltaTime;

            if (!_actions.IsAiming)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(_moveDir, Vector3.up), _turnSpeed * Time.deltaTime);
            }
        }
    }

    public void Jump()
    {
        if (!IsDashing && !IsTraveling && _currentJumps < _maxJumps)
        {
            if (!IsGrounded)
            {
                _currentJumps = _maxJumps;
            }
            else 
            {
                if (_airSpeedModifier != 1f)
                {
                    AddSpeedModifier(_airSpeedModifier);
                }
                _currentJumps++;
            }
            
            _rb.velocity = Vector3.up * _jumpForce;
            IsGrounded = false;

            Debug.Log("Jump");
        }
    }

    public void SlowFall(bool isSlowFallHeld)
    {
        if (!_actions.IsAiming && IsSlowFalling != isSlowFallHeld && !IsTraveling)
        {
            IsSlowFalling = isSlowFallHeld;

            if (IsSlowFalling)
            {
                _rb.useGravity = false;
                _rb.velocity = new Vector3(_rb.velocity.x, -1 * _slowFallVelocityY, _rb.velocity.z);

                if (_slowFallAirSpeedModifier != 1f)
                {
                    RemoveSpeedModifier(_airSpeedModifier);
                    AddSpeedModifier(_slowFallAirSpeedModifier);
                }
            }
            else
            {
                _rb.useGravity = true;
                
                RemoveSpeedModifier(_slowFallAirSpeedModifier);
                if (_airSpeedModifier != 1f && !IsGrounded && !_speedModifiers.Contains(_airSpeedModifier))
                    AddSpeedModifier(_airSpeedModifier);
            }

            string msg = "Start ";
            if (!IsSlowFalling)
                msg = "End ";
            Debug.Log(msg + "Slow Fall");
        }
    }

    public void HighJump()
    {
        if (!_actions.IsAiming && IsGrounded && !IsDashing && !IsTraveling && _input.GetInputValueAsVector2("Move") == Vector2.zero)
        {
            if (_airSpeedModifier != 1f)
            {
                AddSpeedModifier(_airSpeedModifier);
            }
            
            _rb.velocity = Vector3.up * _highJumpForce;
            _currentJumps = _maxJumps;
            IsGrounded = false;
            IsHighJumping = true;

            Debug.Log("High Jump");
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
        if (!IsTraveling)
        {
            IsTraveling = true;
            Debug.Log("Start Travel");
        }
    }

    public void EndTravel()
    {
        _rb.velocity = Vector3.zero;
        IsTraveling = false;
        Debug.Log("End Travel");
    }

    public void AddSpeedModifier(float modifier)
    {
        _speedModifiers.Add(modifier);
        RecalculateNetSpeedModifier();
    }

    public void AddSpeedModifierWithDuration(float modifier, float duration)
    {
        _speedModifiers.Add(modifier);
        RecalculateNetSpeedModifier();
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
}
