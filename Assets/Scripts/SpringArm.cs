using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringArm : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] float _targetArmLength = 10f;

    [Header("Follow Settings")]
    [SerializeField] public GameObject _followObject;
    [SerializeField] float _followLerpAlpha = 0.5f;

    [Header("RotationSettings")]
    [SerializeField] float _slerpAlpha = 0.1f;
    [SerializeField] float _maxRoll = 90f;
    [SerializeField] float _minRoll = -90f;

    private float roll = 0f;
    private float yaw = 0f;
    
    void OnValidate()
    {
        if (_camera == null)
        {
            _camera = GetComponentInChildren<Camera>(true);
        }
        _camera.transform.localPosition = new Vector3(0, 0, -_targetArmLength);
        roll = Mathf.Clamp(transform.rotation.eulerAngles.x, _minRoll, _maxRoll);
        yaw = transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(new Vector3(roll, yaw, 0));
    }

    void Update()
    {
        if (_followObject != null)
        {
            transform.position = Vector3.Lerp(transform.position, _followObject.transform.position, _followLerpAlpha);
        }

        Vector3 targetRot = new Vector3(roll, yaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(targetRot), _slerpAlpha);
    }

    public void ApplyRoll(float degrees)
    {
        roll = Mathf.Clamp(roll + degrees, _minRoll, _maxRoll) % 360;
    }

    public void ApplyYaw(float degrees)
    {
        yaw += degrees;
        yaw %= 360;
    }
}
