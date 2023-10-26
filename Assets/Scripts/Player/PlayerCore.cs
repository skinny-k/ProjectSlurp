using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCamera))]
[RequireComponent(typeof(PlayerActions))]
public class PlayerCore : MonoBehaviour
{
    protected InputManager input;
    protected PlayerMovement movement;
    protected PlayerCamera camera;
    protected PlayerActions actions;

    protected bool _isBlocking;
    protected bool _isAiming;
    
    protected void OnEnable()
    {
        input = GetComponent<InputManager>();
        movement = GetComponent<PlayerMovement>();
        camera = GetComponent<PlayerCamera>();
        actions = GetComponent<PlayerActions>();

        SubscribeToInput();
    }

    protected void OnDisable()
    {
        UnsubscribeToInput();
    }

    protected void SubscribeToInput()
    {
        input.OnMove += Move;
        input.OnCameraMove += Move;
        input.OnJump += Jump;
        input.OnSlowFall += SlowFall;
        input.OnHighJump += HighJump;
        input.OnAttack += Attack;
        input.OnBlock += Block;
        input.OnDash += Dash;
        input.OnAim += Aim;
        input.OnThrow += Throw;
        input.OnTravel += Travel;
    }

    protected void UnsubscribeToInput()
    {
        input.OnMove -= Move;
        input.OnCameraMove -= Move;
        input.OnJump -= Jump;
        input.OnSlowFall -= SlowFall;
        input.OnHighJump -= HighJump;
        input.OnAttack -= Attack;
        input.OnBlock -= Block;
        input.OnDash -= Dash;
        input.OnAim -= Aim;
        input.OnThrow -= Throw;
        input.OnTravel -= Travel;
    }

    protected void Move(Vector2 value)
    {
        movement.Move(value);
    }

    protected void MoveCamera(Vector2 value)
    {
        camera.Move(value);
    }

    protected void Jump()
    {
        movement.Jump();
    }

    protected void SlowFall()
    {
        movement.SlowFall();
    }

    protected void HighJump()
    {
        movement.HighJump();
    }

    protected void Attack()
    {
        actions.Attack();
    }

    protected void Block()
    {
        actions.Block();
    }

    protected void Dash()
    {
        actions.Dash();
    }

    protected void Aim()
    {
        actions.Aim();
    }

    protected void Throw()
    {
        actions.Throw();
    }

    protected void Travel()
    {
        actions.Travel();
    }
}
