using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    [SerializeField] float _rotationalSpeed = 180f;
    [Tooltip("If the absolute value of the vertical camera input is at least this value, horizontal input will be discarded.")]
    [SerializeField] float _verticalMaxout = 0.8f;
    [Tooltip("If the absolute value of the horizontal camera input is at least this value, vertical input will be discarded.")]
    [SerializeField] float _horizontalMaxout = 0.8f;
    
    [Header("Aim Settings")]
    [SerializeField] float _aimSensitivity = 90f;
    [Tooltip("The roll, in degrees, that the camera should return to when the player stops aiming")]
    [SerializeField] float _aimReturnRoll = 20f;

    [Header("Cameras")]
    [SerializeField] CinemachineVirtualCamera _followCamera;
    [SerializeField] SpringArm _followCameraSpringArm;
    [SerializeField] CinemachineVirtualCamera _aimCamera;
    [SerializeField] SpringArm _aimCameraSpringArm;

    public bool IsAiming { get; private set; } = false;

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
    }

    void Update()
    {
        if (IsAiming)
        {
            _followCameraSpringArm.SetYaw(_followCameraSpringArm.FollowObject.transform.rotation.eulerAngles.y);
        }
    }

    public void SetPlayer(Player player)
    {
        _player = player;
    }

    public void Move(Vector2 input)
    {
        input = FixInput(input);
        if (!IsAiming)
        {
            _followCameraSpringArm.ApplyYaw(input.x * _rotationalSpeed * Time.deltaTime);
            _followCameraSpringArm.ApplyRoll(input.y * _rotationalSpeed * Time.deltaTime);
        }
        else
        {
            _aimCameraSpringArm.ApplyRoll(-input.y * _aimSensitivity * Time.deltaTime);
            _player.Rotate(input, _aimSensitivity);
        }
    }

    public void Aim()
    {
        IsAiming = !IsAiming;

        string msg = "Start ";
        if (IsAiming)
        {
            _followCameraSpringArm.SetRoll(_aimReturnRoll);
            _aimCamera.Priority = 11;
        }
        else
        {
            _aimCamera.Priority = 0;
            _aimCameraSpringArm.SetRoll(0);
            msg = "End ";
        }
        Debug.Log(msg + "Aim");
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
