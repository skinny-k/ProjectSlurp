using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeamAffiliation { Player, Enemy, NonCharacter }

// interface for objects that can be damaged
public interface IDamageable
{
    public void TakeDamage(int amount);
    public TeamAffiliation GetTeam();
}
