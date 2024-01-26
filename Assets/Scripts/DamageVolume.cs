using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkinnyUtils;

public enum TeamAffiliation { Player, Enemy, NonCharacter }

// A volume that can damage a destructible object
public class DamageVolume : MonoBehaviour
{
    [Tooltip("Which objects this volume should be able to damage.\nPlayer: This volume will damage the player and any allied characters\nEnemy: This volume will damage enemy characters\nNon-Character: This volume will damage any destructible objects that are not characters")]
    [SerializeField][EnumFlagAttribute] TeamAffiliation hitsTeams = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
