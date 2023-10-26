using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool IsSlowFalling { get; private set; } = false;
    
    public void Move(Vector2 input)
    {
        // Debug.Log(input);
    }

    public void Jump()
    {
        Debug.Log("Jump");
    }

    public void SlowFall()
    {
        IsSlowFalling = !IsSlowFalling;
        string msg = "Start ";
        if (!IsSlowFalling)
            msg = "End ";
        Debug.Log(msg + "Slow Fall");
    }

    public void HighJump()
    {
        Debug.Log("High Jump");
    }
}
