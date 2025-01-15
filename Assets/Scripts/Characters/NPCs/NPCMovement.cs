using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCMovement : CharacterMovement
{
    protected NavMeshAgent _nav;

    void OnValidate()
    {
        InitializeNavMeshAgent();
    }

    override protected void Start()
    {
        base.Start();

        InitializeNavMeshAgent();
        ClearDestination();
    }

    protected void InitializeNavMeshAgent()
    {
        _nav = GetComponent<NavMeshAgent>();
        
        _nav.speed = _moveSpeed;
        _nav.angularSpeed = _turnSpeed;
    }

    override protected void ApplyMovement()
    {
        //
    }

    virtual public bool SetDestination(Vector3 destination)
    {
        return _nav.SetDestination(destination);
    }

    virtual public bool MoveFromCurrentPosition(Vector3 offset)
    {
        return SetDestination(transform.position + offset);
    }

    virtual public bool ClearDestination()
    {
        return SetDestination(transform.position);
    }
}
