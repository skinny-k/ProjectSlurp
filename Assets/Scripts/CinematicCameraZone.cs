using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A zone that will take control of the player camera to give a cinematic view
[RequireComponent(typeof(Collider))]
public class CinematicCameraZone : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _cam;

    Collider _collider;

    void OnValidate()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
        if (_cam != null)
        {
            _cam.Priority = 0;
        }
    }

    void OnStart()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
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
