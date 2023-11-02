using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerActions : MonoBehaviour
{
    [SerializeField] PlayerCamera _camera;

    private Player _player;
    private PlayerMovement _movement;
    private Rigidbody _rb;
    
    public bool IsBlocking { get; private set; } = false;
    public bool IsAiming { get; private set; } = false;

    void Start()
    {
        _player = GetComponent<Player>();
        _movement = GetComponent<PlayerMovement>();
        _rb = GetComponent<Rigidbody>();
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
        Debug.Log("Throw");
    }

    public void Travel()
    {
        Debug.Log("Travel");
    }
}
