using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringArm : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _camera;
    [SerializeField] float _targetArmLength = 10f;

    [Header("Follow Settings")]
    [SerializeField] public GameObject FollowObject;
    [SerializeField] float _followLerpAlpha = 0.5f;

    [Header("Rotation Settings")]
    [SerializeField] float _slerpAlpha = 0.1f;
    [SerializeField] float _maxRoll = 90f;
    [SerializeField] float _minRoll = -90f;

    private float roll = 0f;
    private float yaw = 0f;
    
    void OnValidate()
    {
        if (_camera == null)
        {
            _camera = GetComponentInChildren<CinemachineVirtualCamera>(true);
        }
        if (_camera != null)
        {
            _camera.transform.localPosition = new Vector3(_camera.transform.localPosition.x, _camera.transform.localPosition.y, -_targetArmLength);
        }

        roll = Mathf.Clamp(transform.localRotation.eulerAngles.x, _minRoll, _maxRoll);
        yaw = transform.localRotation.eulerAngles.y;
        transform.localRotation = Quaternion.Euler(new Vector3(roll, yaw, 0));
    }

    void Start()
    {
        if (_camera == null)
        {
            _camera = GetComponentInChildren<CinemachineVirtualCamera>(true);
        }
        _camera.transform.localPosition = new Vector3(_camera.transform.localPosition.x, _camera.transform.localPosition.y, -_targetArmLength);
        roll = Mathf.Clamp(transform.localRotation.eulerAngles.x, _minRoll, _maxRoll);
        yaw = transform.localRotation.eulerAngles.y;
        transform.localRotation = Quaternion.Euler(new Vector3(roll, yaw, 0));
    }

    void Update()
    {
        if (FollowObject != null)
        {
            transform.position = Vector3.Lerp(transform.position, FollowObject.transform.position, _followLerpAlpha);
        }

        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(new Vector3(roll, yaw, 0)), _slerpAlpha);
    }

    public void ApplyRoll(float degrees)
    {
        roll = Mathf.Clamp(roll + degrees, _minRoll, _maxRoll) % 360;
    }

    public void SetRoll(float degrees, bool blend = false)
    {
        roll = degrees % 360;

        if (!blend)
        {
            transform.localRotation = Quaternion.Euler(new Vector3(roll, yaw, 0));
        }
    }

    public void ApplyYaw(float degrees)
    {
        yaw += degrees;
        yaw %= 360;
    }

    public void SetYaw(float degrees, bool blend = false)
    {
        yaw = degrees % 360;

        if (!blend)
        {
            transform.localRotation = Quaternion.Euler(new Vector3(roll, yaw, 0));
        }
    }
}
