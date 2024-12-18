using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// interface for objects that can be damaged
public interface IDamageable
{
    public int TakeDamage(int amount);
    public TeamAffiliation GetTeam();
}
