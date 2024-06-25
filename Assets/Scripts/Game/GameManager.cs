using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

[RequireComponent(typeof(HapticsManager))]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);

            Random.InitState((int)DateTime.Now.Ticks);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
