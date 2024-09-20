using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Player Haptics Data")]
public class PlayerHapticsData : ScriptableObject
{
    [Header("Attack Haptics")]
    [SerializeField] float _attackStrength = 0.5f;
    [SerializeField] float _attackDuration = 0.15f;

    [Header("Block Haptics")]
    [SerializeField] float _blockStrength = 0.6f;
    [SerializeField] float _blockDuration = 0.15f;

    [Header("Throw Haptics")]
    [SerializeField] float _throwStrength = 0.3f;
    [SerializeField] float _throwDuration = 0.25f;

    [Header("Land Haptics")]
    [Tooltip("Maximum allowed strength of the land haptic event.")]
    [SerializeField] float _landStrength = 0.6f;
    [Tooltip("Minimum and maximum velocity to scale the haptic strength to. If velocity is <= the minimum value, the effective strength will be 0. If velocity is >= the maximum value, the effective strength will be Land Strength.")]
    [SerializeField] Vector2 _landThresholds = Vector2.zero;
    [SerializeField] float _landDuration = 0.2f;

    [Header("High Jump Haptics")]
    [SerializeField] float _highJumpStrength = 0.6f;
    [SerializeField] float _highJumpDuration = 0.5f;

    [Header("Slow Fall Haptics")]
    [SerializeField] float _slowFallStrength = 0.4f;

    [Header("Dash Haptics")]
    [SerializeField] float _dashStrength = 0.3f;
    [SerializeField] float _dashDuration = 0.15f;

    [Header("Travel Haptics")]
    [SerializeField] float _travelStrength = 0.75f;
    [SerializeField] float _travelImpactStrength = 1f;
    [SerializeField] float _travelImpactDuration = 0.25f;

    [Header("Collect Weapon Haptics")]
    [SerializeField] float _collectStrength = 0.15f;
    [SerializeField] float _collectDuration = 0.1f;

    public float A_strength => _attackStrength;
    public float A_duration => _attackDuration;

    public float B_strength => _blockStrength;
    public float B_duration => _blockDuration;

    public float C_strength => _throwStrength;
    public float C_duration => _throwDuration;

    public float D_strength => _landStrength;
    public Vector2 D_threshold => _landThresholds;
    public float D_duration => _landDuration;
    
    public float F_strength => _highJumpStrength;
    public float F_duration => _highJumpDuration;

    public float G_strength => _slowFallStrength;

    public float H_strength => _dashStrength;
    public float H_duration => _dashDuration;

    public float I_strength => _travelStrength;
    public float I_impact => _travelImpactStrength;
    public float I_duration => _travelImpactDuration;

    public float J_strength => _collectStrength;
    public float J_duration => _collectDuration;
}
