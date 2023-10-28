using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 5f;

    private List<float> _speedModifiers;
    private Player player;
    private float _netSpeedModifier = 1f;
    
    public bool IsSlowFalling { get; private set; } = false;

    void OnValidate()
    {
        player = GetComponent<Player>();
    }
    
    public void Move(Vector2 input)
    {
        input = input.normalized;
        float speedThisFrame = _moveSpeed * _netSpeedModifier;
        // Vector3 dir = new Vector3(input.x, 0, input.y);
        Vector3 dir = player.Camera.transform.TransformVector(new Vector3(input.x, 0, input.y));
        dir = (new Vector3(dir.x, 0, dir.z)).normalized;
        transform.position += dir * _moveSpeed * Time.deltaTime;
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

    public void AddSpeedModifier(float modifier)
    {
        _speedModifiers.Add(modifier);
        RecalculateNetSpeedModifier();
    }

    public void AddSpeedModifierWithDuration(float modifier, float duration)
    {
        _speedModifiers.Add(modifier);
        RecalculateNetSpeedModifier();
    }

    public void RemoveSpeedModifier(float modifier)
    {
        _speedModifiers.Remove(modifier);
        RecalculateNetSpeedModifier();
    }

    private void RecalculateNetSpeedModifier()
    {
        _netSpeedModifier = 1f;
        foreach (float modifier in _speedModifiers)
        {
            _netSpeedModifier *= modifier;
            if (_netSpeedModifier == 0)
            {
                break;
            }
        }
    }
}
