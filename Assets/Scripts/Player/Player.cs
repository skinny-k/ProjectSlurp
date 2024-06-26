using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Coordinates responses by player component classes to input events from Input Manager
[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerActions))]
public class Player : Entity
{
    [SerializeField] PlayerCamera _camera;
    [SerializeField] PlayerHapticsData _hapticsSettings;
    
    protected InputManager _input;
    protected PlayerMovement _movement;
    protected PlayerActions _actions;

    public PlayerCamera Camera => _camera;
    public PlayerHapticsData HapticsSettings => _hapticsSettings;
    
    protected void OnEnable()
    {
        _input = GetComponent<InputManager>();
        _movement = GetComponent<PlayerMovement>();
        _actions = GetComponent<PlayerActions>();

        SubscribeToInput();
    }

    protected void OnDisable()
    {
        UnsubscribeToInput();
    }

    void Start()
    {
        if (_camera != null)
        {
            _camera.SetPlayer(this);
        }
    }

    protected void SubscribeToInput()
    {
        _input.OnMove += Move;
        _input.OnCameraMove += MoveCamera;
        _input.OnJump += Jump;
        _input.OnSlowFall += SlowFall;
        _input.OnHighJump += HighJump;
        _input.OnAttack += Attack;
        _input.OnBlock += Block;
        _input.OnDash += Dash;
        _input.OnAim += Aim;
        _input.OnThrow += Throw;
        _input.OnTravel += Travel;
        _input.OnShoulderSwitch += ShoulderSwitch;
    }

    protected void UnsubscribeToInput()
    {
        _input.OnMove -= Move;
        _input.OnCameraMove -= MoveCamera;
        _input.OnJump -= Jump;
        _input.OnSlowFall -= SlowFall;
        _input.OnHighJump -= HighJump;
        _input.OnAttack -= Attack;
        _input.OnBlock -= Block;
        _input.OnDash -= Dash;
        _input.OnAim -= Aim;
        _input.OnThrow -= Throw;
        _input.OnTravel -= Travel;
        _input.OnShoulderSwitch -= ShoulderSwitch;
    }

    // allows the player to be rotated by force
    // useful for rotating the player while aiming
    public void Rotate(Vector2 value, float sensitivity)
    {
        _movement.AddRotation(value, sensitivity);
    }

    // helper functions that coordinate responses to input from the player component classes
    protected void Move(Vector2 value)
    {
        _movement.Move(value);
    }

    protected void MoveCamera(Vector2 value)
    {
        _camera.Move(value);
        // _lastCameraMove = 0;
    }

    protected void Jump()
    {
        _movement.Jump();
    }

    protected void SlowFall(bool isSlowFallHeld)
    {
        _movement.SlowFall(isSlowFallHeld);
    }

    protected void HighJump()
    {
        _movement.HighJump();
    }

    protected void Attack()
    {
        _actions.Attack();
    }

    protected void Block()
    {
        _actions.Block();
    }

    protected void Dash()
    {
        _movement.Dash();
    }

    protected void Aim()
    {
        _camera.Aim();
        _actions.Aim();
    }

    protected void Throw()
    {
        _actions.Throw();
    }

    protected void Travel()
    {
        _movement.Travel();
    }

    protected void ShoulderSwitch()
    {
        _camera.ShoulderSwitch();
    }
}
