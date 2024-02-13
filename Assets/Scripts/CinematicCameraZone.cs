using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CinematicCameraZone : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _cam;

    Collider collider;

    void OnValidate()
    {
        collider = GetComponent<Collider>();
        collider.isTrigger = true;
        if (_cam != null)
        {
            _cam.Priority = 0;
        }
    }

    void OnStart()
    {
        collider = GetComponent<Collider>();
        collider.isTrigger = true;
        if (_cam != null)
        {
            _cam.Priority = 0;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() != null)
        {
            _cam.Priority = 11;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player>() != null)
        {
            _cam.Priority = 0;
        }
    }
}
