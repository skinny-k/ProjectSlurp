using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : Character
{
    protected NavMeshAgent _nav;
    protected Health _health;
    
    void OnEnable()
    {
        _nav = GetComponent<NavMeshAgent>();
        _health = GetComponent<Health>();
    }
}
