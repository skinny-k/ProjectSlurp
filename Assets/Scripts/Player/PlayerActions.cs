using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerActions : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] public Weapon PlayerWeapon;
    [SerializeField] float _throwSpeed = 50f;
    [SerializeField] float _maxThrowDistance = 20f;

    private Player _player;
    private PlayerMovement _movement;
    private Rigidbody _rb;
    
    public bool IsBlocking { get; private set; } = false;
    public bool IsAiming { get; private set; } = false;
    public bool HasWeapon { get; private set; } = true;

    void Start()
    {
        _player = GetComponent<Player>();
        _movement = GetComponent<PlayerMovement>();
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!HasWeapon && Vector3.Distance(transform.position, PlayerWeapon.transform.position) > _maxThrowDistance)
        {
            ReturnWeapon();
        }
    }
    
    public void Attack()
    {
        if (_movement.CanMove)
        {
            Debug.Log("Attack");
        }
    }
    
    public void Block()
    {
        if (_movement.CanMove && !_movement.IsInActiveAerial && !IsAiming)
        {
            IsBlocking = !IsBlocking;
            string msg = "Start ";
            if (!IsBlocking)
            msg = "End ";
            Debug.Log(msg + "Block");
        }
    }

    public void Aim()
    {
        if (_movement.CanMove && !_movement.IsHighJumping)
        {
            IsAiming = !IsAiming;

            Vector3 targetRot = Quaternion.LookRotation(_player.Camera.transform.forward).eulerAngles;
            targetRot.x = transform.rotation.x;
            targetRot.z = transform.rotation.z;
            GetComponent<PlayerMovement>().RotateTo(targetRot);

            if (!IsAiming || HasWeapon)
                PlayerWeapon.Aim(IsAiming);

            string msg = "Start ";
            if (IsAiming)
            {
                _movement.AddSpeedModifier(_movement.AimSpeedModifier);
            }
            else
            {
                _movement.RemoveSpeedModifier(_movement.AimSpeedModifier);
                msg = "End ";
            }
            Debug.Log(msg + "Aim");
        }
    }

    public void Throw()
    {
        if (!PlayerWeapon.IsInTransit && _movement.CanMove)
        {
            ReturnWeapon();

            Vector3 target = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, _maxThrowDistance));
            
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit, _maxThrowDistance))
            {
                target = hit.point;
            }
            PlayerWeapon.Throw(target - PlayerWeapon.transform.position, _throwSpeed);
            HasWeapon = false;
            Debug.Log("Throw");
        }
    }

    public void ReturnWeapon()
    {
        PlayerWeapon.Return(_player);
        HasWeapon = true;
        if (_movement.IsTraveling)
        {
            _movement.EndTravel();
        }
    }
}
