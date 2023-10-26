using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    public bool IsBlocking { get; private set; } = false;
    public bool IsAiming { get; private set; } = false;
    
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

    public void Dash()
    {
        Debug.Log("Dash");
    }

    public void Aim()
    {
        IsAiming = !IsAiming;
        string msg = "Start ";
        if (!IsAiming)
            msg = "End ";
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
