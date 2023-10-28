using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    [SerializeField] float _rotationalSpeed = 180f;
    [Tooltip("If the absolute value of the vertical camera input is at least this value, horizontal input will be discarded.")]
    [SerializeField] float _verticalMaxout = 0.8f;
    [Tooltip("If the absolute value of the horizontal camera input is at least this value, vertical input will be discarded.")]
    [SerializeField] float _horizontalMaxout = 0.8f;

    public SpringArm Spring { get; private set; }

    void OnValidate()
    {
        Spring = GetComponentInParent<SpringArm>();
    }

    public void Move(Vector2 input)
    {
        input = input.normalized;
        if (Mathf.Abs(input.x) >= _horizontalMaxout && Mathf.Abs(input.y) < _verticalMaxout)
        {
            input = new Vector2(Mathf.Sign(input.x), 0);
        }
        else if (Mathf.Abs(input.y) >= _verticalMaxout && Mathf.Abs(input.x) < _horizontalMaxout)
        {
            input = new Vector2(0, Mathf.Sign(input.y));
        }
        Spring.ApplyYaw(input.x * _rotationalSpeed * Time.deltaTime);
        Spring.ApplyRoll(input.y * _rotationalSpeed * Time.deltaTime);
    }
}
