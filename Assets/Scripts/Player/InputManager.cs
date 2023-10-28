using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    PlayerInput _input;

    private bool _isSlowFallHeld = false;

    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnCameraMove;
    public event Action OnJump;
    public event Action OnSlowFall;
    public event Action OnHighJump;
    public event Action OnAttack;
    public event Action OnBlock;
    public event Action OnDash;
    public event Action OnAim;
    public event Action OnThrow;
    public event Action OnTravel;

    void OnValidate()
    {
        _input = GetComponent<PlayerInput>();
    }

    void Update()
    {
        // Movement and Camera
        if (_input.actions["Camera"].ReadValue<Vector2>() != Vector2.zero)
        {
            OnCameraMove?.Invoke(_input.actions["Camera"].ReadValue<Vector2>());
        }
        if (_input.actions["Move"].ReadValue<Vector2>() != Vector2.zero)
        {
            OnMove?.Invoke(_input.actions["Move"].ReadValue<Vector2>());
        }

        // Jump & Aerial Maneuvers
        if (_input.actions["Jump"].triggered)
        {
            OnJump?.Invoke();
        }
        if (_input.actions["Slow Fall"].triggered || (_isSlowFallHeld && _input.actions["Slow Fall"].ReadValue<float>() < 0.5f))
        {
            _isSlowFallHeld = !_isSlowFallHeld;
            OnSlowFall?.Invoke();
        }
        if (_input.actions["High Jump"].triggered)
        {
            OnHighJump?.Invoke();
        }

        // Combat
        if (_input.actions["Attack"].triggered && _input.actions["Aim"].ReadValue<float>() <= 0.01f)
        {
            OnAttack?.Invoke();
        }
        if (_input.actions["Block"].triggered)
        {
            OnBlock?.Invoke();
        }
        if (_input.actions["Dash"].triggered)
        {
            OnDash?.Invoke();
        }

        // Weapon Throwing
        if (_input.actions["Aim"].triggered)
        {
            OnAim?.Invoke();
        }
        if (_input.actions["Throw"].triggered)
        {
            OnThrow?.Invoke();
        }
        if (_input.actions["Travel"].triggered)
        {
            OnTravel?.Invoke();
        }
    }
}
