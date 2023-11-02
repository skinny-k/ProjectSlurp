using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private InputManager _input;
    private Player _player;
    private PlayerActions _actions;
    private Rigidbody _rb;

    private List<float> _speedModifiers = new List<float>();
    private float _netSpeedModifier = 1f;

    private int _currentJumps = 0;
    
    public bool IsGrounded { get; private set; } = true;
    public bool IsHighJumping { get; private set; } = false;
    public bool IsSlowFalling { get; private set; } = false;
    public bool IsDashing { get; private set; } = false;

    void Start()
    {
        _input = GetComponent<InputManager>();
        _player = GetComponent<Player>();
        _actions = GetComponent<PlayerActions>();
        _rb = GetComponent<Rigidbody>();
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
        else
        {
            _rb.velocity = Vector3.MoveTowards(_rb.velocity, new Vector3(0, _rb.velocity.y, 0), _dashAcceleration * Time.deltaTime);
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
    
    public void Move(Vector2 input)
    {
        if (!IsDashing)
        {
            input = input.normalized;
            float speedThisFrame = _moveSpeed * _netSpeedModifier;
            Vector3 dir = _player.Camera.transform.TransformVector(new Vector3(input.x, 0, input.y));
            dir = (new Vector3(dir.x, 0, dir.z)).normalized;
            transform.position += dir * speedThisFrame * Time.deltaTime;

            if (!_actions.IsAiming)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir, Vector3.up), _turnSpeed * Time.deltaTime);
            }
        }
    }

    public void Rotate(Vector2 input, float sensitivity)
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, input.x * sensitivity, 0)), sensitivity * Time.deltaTime);
    }

    public void Jump()
    {
        if (!IsDashing && _currentJumps < _maxJumps)
        {
            if (_airSpeedModifier != 1f)
            {
                AddSpeedModifier(_airSpeedModifier);
            }
            
            _rb.velocity = Vector3.up * _jumpForce;
            _currentJumps++;
            IsGrounded = false;

            Debug.Log("Jump");
        }
    }

    public void SlowFall(bool isSlowFallHeld)
    {
        if (!_actions.IsAiming && IsSlowFalling != isSlowFallHeld)
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
        if (!_actions.IsAiming && IsGrounded && _input.GetInputValueAsVector2("Move") == Vector2.zero)
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
        if (!_actions.IsAiming && !IsHighJumping)
        {
            StartCoroutine(DoDash());
            Debug.Log("Dash");
        }
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
