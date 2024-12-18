using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags] public enum TeamAffiliation { Player = 1, Enemy = 2, Character = 4, NonCharacter = 8 }

[RequireComponent(typeof(Health))]
public class Character : Entity
{
    [SerializeField] TeamAffiliation _team;

    public TeamAffiliation Team => _team;
    public Health CharacterHealth { get; private set; }

    void Awake()
    {
        CharacterHealth = GetComponent<Health>();
        CharacterHealth.OnDie += HandleDeath;
    }

    protected virtual void HandleDeath()
    {
        // definitely replace later, just a dumb programmer thing to squash the character
        transform.localScale = new Vector3 (1f, 0.05f, 1f);
        transform.Find("Art").parent = null;
        Destroy(this.gameObject);
    }
}
