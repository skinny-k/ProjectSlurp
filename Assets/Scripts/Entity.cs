using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a game object that should be affected by gravity and physics
[RequireComponent(typeof(Rigidbody))]
public class Entity : MonoBehaviour
{
    public Rigidbody Rb { get; private set; } = null;
    
    protected void OnValidate()
    {
        Rb = GetComponent<Rigidbody>();
    }
}
