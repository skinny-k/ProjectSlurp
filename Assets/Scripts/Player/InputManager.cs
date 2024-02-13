using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// As of Unity 2021.3.21f, polling for input is the only way to receive input from the new Input System. This class
// instead wraps input from the Player Input component into an event-based system
[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    PlayerInput _input;

    private bool _isSlowFallHeld = false;

    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnCameraMove;
    public event Action OnJump;
    public event Action<bool> OnSlowFall;
    public event Action OnHighJump;
    public event Action OnAttack;
    public event Action OnBlock;
    public event Action OnDash;
    public event Action OnAim;
    public event Action OnThrow;
    public event Action OnTravel;
    public event Action OnShoulderSwitch;

    void OnValidate()
    {
        _input = GetComponent<PlayerInput>();
    }

    void Update()
    {
        // polls each input action for state changes and fires a corresponding event if a state change has occurred
        
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
        // Slow Fall event will fire both when the button is pressed and released, including the current slow fall
        // state when it does so
        if (_input.actions["Slow Fall"].triggered || (_isSlowFallHeld && _input.actions["Slow Fall"].ReadValue<float>() < 0.5f))
        {
            _isSlowFallHeld = !_isSlowFallHeld;
            OnSlowFall?.Invoke(_isSlowFallHeld);
        }
        if (_input.actions["High Jump"].triggered)
        {
            OnHighJump?.Invoke();
        }

        // Combat
        // Attack action will not trigger if Aim action is active
        // NOTE: The Throw action can trigger when left trigger is held down (i.e. the Aim action is active) and
        //       either the right trigger or north face button is pressed. Since the north face button also
        //       activates the Attack action, this prevents both actions from triggering on the same press of the
        //       north button.
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

        // Shoulder Switch
        if (_input.actions["Shoulder Switch"].triggered)
        {
            OnShoulderSwitch?.Invoke();
        }
    }

    // helper functions to make controller values more easily callable from other scripts
    public Vector2 GetInputValueAsVector2(String value)
    {
        return _input.actions[value].ReadValue<Vector2>();
    }

    public float GetInputValueAsFloat(String value)
    {
        return _input.actions[value].ReadValue<float>();
    }

    public bool GetInputValueAsBoolean(String value)
    {
        return _input.actions[value].ReadValue<bool>();
    }
}
