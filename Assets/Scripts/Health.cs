using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a character or anotehr object with health
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] int _maxHealth = 10;

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

    public void TakeDamage(int amount)
    {
        ModifyHealth(-amount);
    }

    public void TakeDamageByPercentage(float p)
    {
        if (p >= 0f && p <= 1f)
        {
            TakeDamage((int) Mathf.Round(_maxHealth * p));
        }
    }

    public void GainHealth(int amount)
    {
        ModifyHealth(amount);
    }

    public void GainHealthByPercentage(float p)
    {
        if (p >= 0f && p <= 1f)
        {
            GainHealth((int) Mathf.Round(_maxHealth * p));
        }
    }

    protected void ModifyHealth(int mod)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + mod, 0, _maxHealth);
    }
}
