using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyPerception : MonoBehaviour
{
    [Tooltip("The maximum distance at which an enemy can see a player.")]
    [SerializeField] float _maxSightDistance = 25f;
    [Tooltip("Within this distance, the enemy will use their Close FOV")]
    [SerializeField] float _minSightDistance = 5f;
    [Tooltip("The maximum difference in Y for which an enemy can see a player.")]
    [SerializeField] float _maxLookUp = 7.5f;
    [Tooltip("The degrees difference from forward at which an enemy can see a player. For instance, a FOV of 90 will detect any player directly left, directly right or anywhere in front of the enemy.")]
    [SerializeField] float _normalFOV = 45f;
    [Tooltip("The FOV to use within the Minimum Sight Distance.")]
    [SerializeField] float _closeFOV = 75f;

    Enemy _enemy;

    SphereCollider _maxSightCol;
    Player _playerInSight = null;
    
    void OnValidate()
    {
        InitializeSight();
    }
    
    void Start()
    {
        _enemy = GetComponentInParent<Enemy>();

        InitializeSight();
    }
    
    private void InitializeSight()
    {
        _maxSightCol = GetComponent<SphereCollider>();
        _maxSightCol.radius = _maxSightDistance;
        _maxSightCol.center = Vector3.zero;
        _maxSightCol.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        _playerInSight = other.GetComponent<Player>();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Player>() != null)
        {
            _playerInSight = null;
        }
    }

    private bool IsInSight(Player player)
    {
        // player exists and is within the max alloweed vertical distance and is in at least one FOV
        return player != null && Mathf.Abs(transform.position.y - player.transform.position.y) <= _maxLookUp &&
               (IsInMaxSight(player) || IsInMinSight(player));
    }

    private bool IsInMinSight(Player player)
    {
        // distance is less than min sight and angle is less than close FOV
        return Vector3.Distance(transform.position, player.transform.position) <= _minSightDistance &&
               Vector3.Angle(transform.forward, player.transform.position - transform.position) <= _closeFOV;
    }
    
    private bool IsInMaxSight(Player player)
    {
        // angle is less than normal FOV
        return Vector3.Angle(transform.forward, player.transform.position - transform.position) <= _normalFOV;
    }
}
