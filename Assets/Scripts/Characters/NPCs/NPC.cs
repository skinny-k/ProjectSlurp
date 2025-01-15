using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPCMovement))]
public class NPC : Character
{
    protected NPCMovement _movement;

    void Start()
    {
        _movement = GetComponent<NPCMovement>();
    }
}
