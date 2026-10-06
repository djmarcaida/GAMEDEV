using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float invulnerabilityDuration = 0.8f;

    [SerializeField] private bool hasShield;
    [SerializeField] private float shieldDurationRemaining;
    [SerializeField] private float speedBoostRemaining;
    [SerializeField] private float activeSpeedMultiplier = 1f;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;
    public event Action<string> OnBonusAcquired;
    public event Action OnDamageTaken;

    private bool isInvulnerable;
    private Player playerController;
    private float baseMoveSpeed = 5f;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool HasShield => hasShield;
    public bool IsInvulnerable => isInvulnerable;
    public float ShieldTimeRemaining => shieldDurationRemaining;
    public float SpeedBoostTimeRemaining => speedBoostRemaining;

    private void Awake()
    {
        playerController = GetComponent<Player>();
        if (playerController != null)
            baseMoveSpeed = playerController.MoveSpeed;

        currentHealth = maxHealth;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Update()
    {
        if (hasShield && shieldDurationRemaining > 0f)
        {
            shieldDurationRemaining -= Time.deltaTime;
            if (shieldDurationRemaining <= 0f)
            {
                hasShield = false;
                shieldDurationRemaining = 0f;
            }
        }

        if (speedBoostRemaining > 0f)
        {
            speedBoostRemaining -= Time.deltaTime;
            if (speedBoostRemaining <= 0f)
            {
                ResetSpeedBoost();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        if (hasShield)
        {
            hasShield = false;
            shieldDurationRemaining = 0f;
            StartCoroutine(InvulnerableFrames(0.4f));
            OnDamageTaken?.Invoke();
            return;
        }

        if (isInvulnerable) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamageTaken?.Invoke();

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
        else
        {
            StartCoroutine(InvulnerableFrames(invulnerabilityDuration));
        }
    }

    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnBonusAcquired?.Invoke($"+{amount} HP");
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if (playerController == null)
            playerController = GetComponent<Player>();

        if (playerController != null)
        {
            activeSpeedMultiplier = multiplier;
            speedBoostRemaining = duration;
            playerController.MoveSpeed = baseMoveSpeed * multiplier;
            OnBonusAcquired?.Invoke($"Speed Boost x{multiplier:F1}");
        }
    }

    private void ResetSpeedBoost()
    {
        speedBoostRemaining = 0f;
        activeSpeedMultiplier = 1f;

        if (playerController != null)
            playerController.MoveSpeed = baseMoveSpeed;
    }

    public void ApplyShield(float duration)
    {
        hasShield = true;
        shieldDurationRemaining = duration;
        OnBonusAcquired?.Invoke($"Shield Active ({duration:F0}s)");
    }

    private IEnumerator InvulnerableFrames(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }

    public void ResetState()
    {
        currentHealth = maxHealth;
        isInvulnerable = false;
        hasShield = false;
        shieldDurationRemaining = 0f;
        ResetSpeedBoost();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
