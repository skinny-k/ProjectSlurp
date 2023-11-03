using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerActions : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] Weapon _weapon;
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
        if (!HasWeapon && Vector3.Distance(transform.position, _weapon.transform.position) > _maxThrowDistance)
        {
            ReturnWeapon();
        }

        // if (IsAiming)
        // {
        //     Vector3 targetRot = _player.Camera.transform.rotation.eulerAngles;
        //     targetRot.z = transform.rotation.eulerAngles.z;
        //     transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(targetRot), 0.5f);
        // }
    }
    
    public void Attack()
    {
        Debug.Log("Attack");
    }
    
    public void Block()
    {
        IsBlocking = !IsBlocking;
        string msg = "Start ";
        if (!IsBlocking)
            msg = "End ";
        Debug.Log(msg + "Block");
    }

    public void Aim()
    {
        IsAiming = !IsAiming;

        Vector3 targetRot = Quaternion.LookRotation(_player.Camera.transform.forward).eulerAngles;
        targetRot.x = transform.rotation.x;
        targetRot.z = transform.rotation.z;
        GetComponent<PlayerMovement>().RotateTo(targetRot);

        if (!IsAiming || HasWeapon)
            _weapon.Aim(IsAiming);

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

    public void Throw()
    {
        Vector3 target = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, _maxThrowDistance));
        
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit, _maxThrowDistance))
        {
            target = hit.point;
        }
        _weapon.Throw(target - _weapon.transform.position, _throwSpeed);
        HasWeapon = false;
        Debug.Log("Throw");
    }

    public void Travel()
    {
        Debug.Log("Travel");
    }

    public void ReturnWeapon()
    {
        _weapon.Return(_player);
        HasWeapon = true;
    }
}
