using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(CinemachineBrain))]
public class PlayerCamera : MonoBehaviour
{
    [SerializeField] float _rotationalSpeed = 180f;
    [Tooltip("The roll in degrees that the camera should return to when the player is not providing camera input while moving or is not aiming.")]
    [SerializeField] float _defaultRoll = 15f;
    // [Tooltip("The time in seconds that the camera should wait before returning to its default.")]
    // [SerializeField] public float ResetDelay = 2f;
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
    private Vector2 _cameraInput;
    private CinemachineBrain _cm;

    void OnValidate()
    {
        // find follow camera & spring arm
        if (_followCamera != null)
        {
            _followCameraSpringArm = _followCamera.GetComponentInParent<SpringArm>();
        }
        else if (_followCameraSpringArm != null)
        {
            _followCamera = _followCameraSpringArm.GetComponentInChildren<CinemachineVirtualCamera>();
        }

        // find aim camera & spring arm
        if (_aimCamera != null)
        {
            _aimCameraSpringArm = _aimCamera.GetComponentInParent<SpringArm>();
        }
        else if (_aimCameraSpringArm != null)
        {
            _aimCamera = _aimCameraSpringArm.GetComponentInChildren<CinemachineVirtualCamera>();
        }

        // initialize follow camera spring arm with default settings
        if (_followCameraSpringArm != null)
        {
            _followCameraSpringArm.transform.rotation = Quaternion.Euler(_defaultRoll, transform.position.y, 0);
        }
    }

    void Awake()
    {
        _cm = GetComponent<CinemachineBrain>();
    }

    void Update()
    {
        // fix input as necessary
        _cameraInput = FixInput(_player.GetCamera());
        if (_cameraInput.magnitude >= 0.05f)
        {
            IsMovingToDefault = false;

            if (IsAiming)
            {
                // apply the input movement to the aim camera and rotate the player to match
                _aimCameraSpringArm.ApplyYaw(_cameraInput.x * _aimSensitivity * Time.deltaTime);
                _aimCameraSpringArm.ApplyRoll(-_cameraInput.y * _aimSensitivity * Time.deltaTime);
                _player.Rotate(_cameraInput, _aimSensitivity);
            }
            else
            {
                // apply the input movement to the follow camera
                _followCameraSpringArm.ApplyYaw(_cameraInput.x * _rotationalSpeed * Time.deltaTime);
                _followCameraSpringArm.ApplyRoll(_cameraInput.y * _rotationalSpeed * Time.deltaTime);
            }
        }
        
        // move the follow camera toward its default rotation as necessary
        if (IsMovingToDefault)
        {
            // lerp yaw and roll toward player's yaw and default roll
            float targetYaw = Mathf.Lerp(transform.rotation.eulerAngles.y, _player.transform.rotation.eulerAngles.y, _resetLerpAlpha);
            if (Mathf.Abs(transform.rotation.eulerAngles.y - _player.transform.rotation.eulerAngles.y) > 180f)
            {
                targetYaw = Mathf.Lerp(transform.rotation.eulerAngles.y + 360f, _player.transform.rotation.eulerAngles.y, _resetLerpAlpha);
            }
            float targetRoll = Mathf.Lerp(transform.rotation.eulerAngles.x, _defaultRoll, _resetLerpAlpha);
            
            _followCameraSpringArm.SetYaw(targetYaw);
            _followCameraSpringArm.SetRoll(targetRoll);

            // if yaw and roll equal player's yaw and default roll, respectively, stop trying to move to the default
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

    public void MoveToDefault()
    {
        IsMovingToDefault = true;
    }

    public void Aim()
    {
        IsAiming = !IsAiming;

        if (IsAiming)
        {
            // snap the aim camera's yaw and roll to the follow camera's yaw and 0, respectively, then set the aim
            // camera as the active camera
            _aimCameraSpringArm.SetYaw(_followCameraSpringArm.transform.rotation.eulerAngles.y);
            _aimCameraSpringArm.SetRoll(0);
            _aimCamera.Priority = 20;
        }
        else
        {
            // snap the follow camera's yaw and roll to the aim camera's yaw and the default roll, respectively,
            // then set the follow camera as the active camera
            _followCameraSpringArm.SetYaw(_aimCameraSpringArm.transform.rotation.eulerAngles.y);
            _followCameraSpringArm.SetRoll(_defaultRoll);
            _aimCamera.Priority = 0;
        }
    }

    public void ShoulderSwitch()
    {
        Vector3 newArm = _aimCameraSpringArm.ArmOffsetProportions;
        newArm.x *= -1;
    }

    // forces the input to be read as purely vertical or purely horizontal if it is close enough
    // this is helpful for camera input, as it allows the player to pan straight up, straight down or straight to
    // the side much more easily
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

    public Vector3 GetForward()
    {
        CinemachineBlend _blend = _cm.ActiveBlend;
        if (_blend == null)
        {
            return transform.forward;
        }
        else
        {
            Vector3 dirA = _blend.CamA.VirtualCameraGameObject.transform.forward;
            Vector3 dirB = _blend.CamB.VirtualCameraGameObject.transform.forward;
            return Vector3.Lerp(dirA, dirB, _blend.TimeInBlend / _blend.Duration).normalized;
        }
    }

    public Vector3 InputToCameraDirection(Vector2 input, Vector3 defaultVector)
    {
        if (!IsMovingToDefault)
        {
            Vector3 inputDir = new Vector3(input.x, 0, input.y);
            if (Mathf.Approximately(Mathf.Abs(GetForward().y), 1.0f))
            {
                inputDir.y = input.y;
                inputDir.z = 0;
            }

            Vector3 result = transform.TransformDirection(inputDir);
            result.y = 0f;
            result.Normalize();
            return result;
        }
        else
        {
            defaultVector.y = 0f;
            defaultVector.Normalize();
            return defaultVector;
        }
    }
}
