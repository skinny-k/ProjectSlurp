using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a character or another object with health
[RequireComponent(typeof(Collider))]
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] int _maxHealth = 10;

    public event Action OnDie;
    
    public int CurrentHealth { get; private set; }
    
    public int MaxHealth => _maxHealth;
    public float PercentHealth => ((float) CurrentHealth) / ((float) _maxHealth);
    public bool IsAlive => CurrentHealth > 0;

    protected Character _character;

    void OnEnable()
    {
        _character = GetComponent<Character>();

        CurrentHealth = _maxHealth;
    }

    public TeamAffiliation GetTeam()
    {
        return _character != null ? _character.Team : TeamAffiliation.NonCharacter;
    }

    // returns total health after damage
    public int TakeDamage(int amount)
    {
        if (ModifyHealth(-amount) <= 0)
        {
            HandleDeath();
            return 0;
        }
        else return CurrentHealth;
    }

    // returns total health after damage
    public int TakeDamageByPercentage(float p)
    {
        if (p >= 0f && p <= 1f)
        {
            return TakeDamage((int) Mathf.Round(_maxHealth * p));
        }
        else return CurrentHealth;
    }

    // returns total health after gain
    public int GainHealth(int amount)
    {
        return ModifyHealth(amount);
    }

    // returns total health after gain
    public int GainHealthByPercentage(float p)
    {
        if (p >= 0f && p <= 1f)
        {
            return GainHealth((int) Mathf.Round(_maxHealth * p));
        }
        else return CurrentHealth;
    }

    // returns total health after modification
    protected int ModifyHealth(int mod)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + mod, 0, _maxHealth);
        return CurrentHealth;
    }

    protected void HandleDeath()
    {
        OnDie?.Invoke();
    }
}
