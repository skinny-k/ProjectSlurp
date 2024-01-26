using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// duplicates the Spring Arm component from Unreal Engine using the Cinemachine system
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
        InitializeCameraSettings();
    }

    void Start()
    {
        InitializeCameraSettings();
    }

    void Update()
    {
        // follows the desired object, if it is set
        if (FollowObject != null)
        {
            transform.position = Vector3.Lerp(transform.position, FollowObject.transform.position, _followLerpAlpha);
        }

        // rotates toward the desired rotation
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(new Vector3(roll, yaw, 0)), _slerpAlpha);
    }

    void InitializeCameraSettings()
    {
        // finds the primary camera and sets the desired spring arm length
        if (_camera == null)
        {
            _camera = GetComponentInChildren<CinemachineVirtualCamera>(true);
        }
        if (_camera != null)
        {
            _camera.transform.localPosition = new Vector3(_camera.transform.localPosition.x, _camera.transform.localPosition.y, -_targetArmLength);
        }

        // sets the initial rotation of the spring arm
        roll = Mathf.Clamp(transform.localRotation.eulerAngles.x, _minRoll, _maxRoll);
        yaw = transform.localRotation.eulerAngles.y;
        transform.localRotation = Quaternion.Euler(new Vector3(roll, yaw, 0));
    }

    public void ApplyRoll(float degrees)
    {
        // adds degrees to the desired roll
        roll = Mathf.Clamp(roll + degrees, _minRoll, _maxRoll) % 360;
    }

    public void SetRoll(float degrees, bool blend = false)
    {
        // sets the desired roll to the input degrees
        roll = degrees % 360;

        if (!blend)
        {
            // immediately sets the camera's rotation to the desired roll without blending
            transform.localRotation = Quaternion.Euler(new Vector3(roll, yaw, 0));
        }
    }

    public void ApplyYaw(float degrees)
    {
        // adds degrees to the desired yaw
        yaw += degrees;
        yaw %= 360;
    }

    public void SetYaw(float degrees, bool blend = false)
    {
        // sets the desired yaw to the input degrees
        yaw = degrees % 360;

        if (!blend)
        {
            // immediately sets the camera's rotation to the desired yaw without blending
            transform.localRotation = Quaternion.Euler(new Vector3(roll, yaw, 0));
        }
    }
}
