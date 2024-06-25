using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// duplicates the Spring Arm component from Unreal Engine using the Cinemachine system
public class SpringArm : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _camera;
    [Tooltip("Minimum spring arm length as a fraction of target arm offset magnitude.")]
    [SerializeField] float _minimumArmLength = 1f;
    [SerializeField] float _targetArmLength = 10f;
    [SerializeField] Vector3 _armOffsetProportions = new Vector3(0, 0, 1f);

    [Header("Collision Settings")]
    [SerializeField] float _collisionOffset = 0.1f;
    [SerializeField] LayerMask layers;

    [Header("Follow Settings")]
    [SerializeField] public GameObject FollowObject;
    [SerializeField] float _followLerpAlpha = 0.5f;

    [Header("Rotation Settings")]
    [SerializeField] float _slerpAlpha = 0.1f;
    [SerializeField] float _maxRoll = 90f;
    [SerializeField] float _minRoll = -90f;

    private float _roll = 0f;
    private float _yaw = 0f;
    private float _currentArmLength = 10f;

    public float CurrentArmLength => _currentArmLength;
    public float TargetArmLength => _targetArmLength;
    public Vector3 ArmOffsetProportions => _armOffsetProportions;
    
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
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(new Vector3(_roll, _yaw, 0)), _slerpAlpha);

        // lerp towards maximum possible arm length
        Vector3 armPos = UpdateArmLength();
        _camera.transform.localPosition = Vector3.Slerp(_camera.transform.localPosition, armPos, _slerpAlpha);
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
            _camera.transform.localPosition = _armOffsetProportions.normalized * _targetArmLength;
        }

        // sets the initial rotation of the spring arm
        _roll = Mathf.Clamp(transform.localRotation.eulerAngles.x, _minRoll, _maxRoll);
        _yaw = transform.localRotation.eulerAngles.y;
        transform.localRotation = Quaternion.Euler(new Vector3(_roll, _yaw, 0));
    }

    private Vector3 UpdateArmLength()
    {
        RaycastHit hit;
        if (Physics.Raycast(FollowObject.transform.position, _camera.transform.position - FollowObject.transform.position, out hit, _targetArmLength, layers))
        {
            return _armOffsetProportions.normalized * Mathf.Clamp(hit.distance - _collisionOffset, _targetArmLength * _minimumArmLength, _targetArmLength);
        }
        else
        {
            return _armOffsetProportions.normalized * _targetArmLength;
        }
    }

    public void SetTargetArmLength(float length)
    {
        _targetArmLength = length;
    }

    public void ApplyRoll(float degrees)
    {
        // adds degrees to the desired roll
        _roll = Mathf.Clamp(_roll + degrees, _minRoll, _maxRoll) % 360;
    }

    public void SetRoll(float degrees, bool blend = false)
    {
        // sets the desired roll to the input degrees
        _roll = degrees % 360;

        if (!blend)
        {
            // immediately sets the camera's rotation to the desired roll without blending
            transform.localRotation = Quaternion.Euler(new Vector3(_roll, _yaw, 0));
        }
    }

    public void ApplyYaw(float degrees)
    {
        // adds degrees to the desired yaw
        _yaw += degrees;
        _yaw %= 360;
    }

    public void SetYaw(float degrees, bool blend = false)
    {
        // sets the desired yaw to the input degrees
        _yaw = degrees % 360;

        if (!blend)
        {
            // immediately sets the camera's rotation to the desired yaw without blending
            transform.localRotation = Quaternion.Euler(new Vector3(_roll, _yaw, 0));
        }
    }
}
