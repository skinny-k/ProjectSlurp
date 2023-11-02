using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerActions))]
public class Player : MonoBehaviour
{
    [SerializeField] PlayerCamera _camera;
    
    protected InputManager _input;
    protected PlayerMovement _movement;
    protected PlayerActions _actions;
    protected bool _isBlocking;
    protected bool _isAiming;

    public PlayerCamera Camera => _camera;
    
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
    }

    public void Rotate(Vector2 value, float sensitivity)
    {
        _movement.Rotate(value, sensitivity);
    }

    protected void Move(Vector2 value)
    {
        _movement.Move(value);
    }

    protected void MoveCamera(Vector2 value)
    {
        _camera.Move(value);
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
        _actions.Travel();
    }
}
