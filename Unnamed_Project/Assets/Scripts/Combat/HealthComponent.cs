using UnityEngine;
using System;

public class HealthComponent : MonoBehaviour
{
    public float maxHealth = 100f;

    float _health;

    public event Action OnDeath;
    public event Action OnDamageTaken;
    public event Action<float> OnHealthChanged;

    public float NormalizedHealth => _health / maxHealth;

    void Awake()
    {
        _health = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (_health <= 0f) return;
        _health = Mathf.Max(0f, _health - amount);
        OnHealthChanged?.Invoke(NormalizedHealth);
        OnDamageTaken?.Invoke();
        if (_health <= 0f)
            OnDeath?.Invoke();
    }
}
