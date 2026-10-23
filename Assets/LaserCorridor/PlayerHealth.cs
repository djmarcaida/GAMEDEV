using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float invulnerabilityDuration = 0.8f;

    public event Action OnDeath;
    public event Action<string> OnBonusAcquired;

    private bool isInvulnerable;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0 || isInvulnerable) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (currentHealth <= 0)
            OnDeath?.Invoke();
        else
            StartCoroutine(InvulnerableFrames(invulnerabilityDuration));
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnBonusAcquired?.Invoke($"+{amount} HP");
    }

    private IEnumerator InvulnerableFrames(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }

    public void ResetState()
    {
        StopAllCoroutines();
        currentHealth = maxHealth;
        isInvulnerable = false;
    }
}
