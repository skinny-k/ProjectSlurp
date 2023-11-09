using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    [SerializeField] float _rotationalSpeed = 180f;
    [Tooltip("The roll in degrees that the camera should return to when the player is not providing camera input while moving or is not aiming.")]
    [SerializeField] float _defaultRoll = 15f;
    [Tooltip("The time in seconds that the camera should wait before returning to its default.")]
    [SerializeField] public float ResetDelay = 2f;
    [SerializeField] float _resetLerpAlpha = 0.05f;
    [Tooltip("If the absolute value of the vertical camera input is at least this value, horizontal input will be discarded.")]
    [SerializeField] float _verticalMaxout = 0.8f;
    [Tooltip("If the absolute value of the horizontal camera input is at least this value, vertical input will be discarded.")]
    [SerializeField] float _horizontalMaxout = 0.8f;
    
    [Header("Aim Settings")]
    [SerializeField] float _aimSensitivity = 90f;

    [Header("Cameras")]
    [SerializeField] CinemachineVirtualCamera _followCamera;
    [SerializeField] SpringArm _followCameraSpringArm;
    [SerializeField] CinemachineVirtualCamera _aimCamera;
    [SerializeField] SpringArm _aimCameraSpringArm;

    public bool IsAiming { get; private set; } = false;
    public bool IsMovingToDefault { get; private set; } = false;

    private Player _player;

    void OnValidate()
    {
        if (_followCamera != null)
        {
            _followCameraSpringArm = _followCamera.GetComponentInParent<SpringArm>();
        }
        else if (_followCameraSpringArm != null)
        {
            _followCamera = _followCameraSpringArm.GetComponentInChildren<CinemachineVirtualCamera>();
        }

        if (_aimCamera != null)
        {
            _aimCameraSpringArm = _aimCamera.GetComponentInParent<SpringArm>();
        }
        else if (_aimCameraSpringArm != null)
        {
            _aimCamera = _aimCameraSpringArm.GetComponentInChildren<CinemachineVirtualCamera>();
        }

        if (_followCameraSpringArm != null)
        {
            _followCameraSpringArm.transform.rotation = Quaternion.Euler(_defaultRoll, transform.position.y, 0);
        }
    }

    void Update()
    {
        if (IsMovingToDefault)
        {
            float targetYaw = Mathf.Lerp(transform.rotation.eulerAngles.y, _player.transform.rotation.eulerAngles.y, _resetLerpAlpha);
            if (Mathf.Abs(transform.rotation.eulerAngles.y - _player.transform.rotation.eulerAngles.y) > 180f)
            {
                targetYaw = Mathf.Lerp(transform.rotation.eulerAngles.y + 360f, _player.transform.rotation.eulerAngles.y, _resetLerpAlpha);
            }
            float targetRoll = Mathf.Lerp(transform.rotation.eulerAngles.x, _defaultRoll, _resetLerpAlpha);
            
            _followCameraSpringArm.SetYaw(targetYaw);
            _followCameraSpringArm.SetRoll(targetRoll);

            if (targetYaw == _player.transform.rotation.eulerAngles.y && targetRoll == _defaultRoll)
            {
                IsMovingToDefault = false;
            }
        }
    }

    public void SetPlayer(Player player)
    {
        _player = player;
    }

    public void Move(Vector2 input)
    {
        IsMovingToDefault = false;
        input = FixInput(input);
        if (IsAiming)
        {
            _aimCameraSpringArm.ApplyYaw(input.x * _aimSensitivity * Time.deltaTime);
            _aimCameraSpringArm.ApplyRoll(-input.y * _aimSensitivity * Time.deltaTime);
            _player.Rotate(input, _aimSensitivity);
        }
        else
        {
            _followCameraSpringArm.ApplyYaw(input.x * _rotationalSpeed * Time.deltaTime);
            _followCameraSpringArm.ApplyRoll(input.y * _rotationalSpeed * Time.deltaTime);
        }
    }

    public void MoveToDefault()
    {
        IsMovingToDefault = true;
    }

    public void Aim()
    {
        IsAiming = !IsAiming;

        if (IsAiming)
        {
            _aimCameraSpringArm.SetYaw(_followCameraSpringArm.transform.rotation.eulerAngles.y);
            _aimCameraSpringArm.SetRoll(0);
            _aimCamera.Priority = 11;
        }
        else
        {
            _followCameraSpringArm.SetYaw(_aimCameraSpringArm.transform.rotation.eulerAngles.y);
            _followCameraSpringArm.SetRoll(_defaultRoll);
            _aimCamera.Priority = 0;
        }
    }

    private Vector2 FixInput(Vector2 input)
    {
        Vector2 normalized = input.normalized;
        if (Mathf.Abs(normalized.x) >= _horizontalMaxout && Mathf.Abs(normalized.y) < _verticalMaxout)
        {
            input = new Vector2(input.x, 0);
        }
        else if (Mathf.Abs(normalized.y) >= _verticalMaxout && Mathf.Abs(normalized.x) < _horizontalMaxout)
        {
            input = new Vector2(0, input.y);
        }
        return input;
    }
}
