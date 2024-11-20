using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeamAffiliation { Player, Enemy, Character, NonCharacter }

[RequireComponent(typeof(Health))]
public class Character : Entity
{
    [SerializeField] TeamAffiliation _team;

    public TeamAffiliation Team { get; private set; }
}
