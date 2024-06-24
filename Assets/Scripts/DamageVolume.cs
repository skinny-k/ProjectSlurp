using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkinnyUtils;

// A volume that can damage a destructible object
[RequireComponent(typeof(Collider))]
public class DamageVolume : MonoBehaviour
{
    [Tooltip("Which objects this volume should be able to damage.\nPlayer: This volume will damage the player and any allied characters\nEnemy: This volume will damage enemy characters\nNon-Character: This volume will damage any destructible objects that are not characters")]
    [SerializeField][EnumFlagAttribute] TeamAffiliation _hitsTeams = 0;
    [Tooltip("How much damage the damage volume inflicts.")]
    [SerializeField] int _damageOnHit = 1;
    
    void OnTriggerEnter(Collider other)
    {
        IDamageable hit = other.GetComponent<IDamageable>();
        if (hit != null && _hitsTeams.HasFlag(hit.GetTeam()))
        {
            hit.TakeDamage(_damageOnHit);
        }
    }
}
