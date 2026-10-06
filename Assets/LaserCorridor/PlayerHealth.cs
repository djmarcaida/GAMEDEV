using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player health, damage taking, invulnerability frames,
/// status bonuses (Speed boost, Shield, Health recovery), and death notification.
/// Compatible with Unity 6.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private float invulnerabilityDuration = 0.8f; // i-frames after taking damage

    [Header("Status Effects")]
    [SerializeField] private bool hasShield = false;
    [SerializeField] private float shieldDurationRemaining = 0f;
    [SerializeField] private float speedBoostRemaining = 0f;
    [SerializeField] private float activeSpeedMultiplier = 1f;

    // Events for UI or external managers
    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action OnDeath;
    public event Action<string> OnBonusAcquired; // (bonus name)
    public event Action OnDamageTaken;

    private bool isInvulnerable = false;
    private Player playerController;
    private float originalMoveSpeed = 5f;
    private Coroutine speedBoostCoroutine;
    private Coroutine shieldCoroutine;

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
        {
            originalMoveSpeed = playerController.MoveSpeed;
        }
        currentHealth = maxHealth;
    }

    private void Start()
    {
        NotifyHealthChanged();
    }

    private void Update()
    {
        // Update shield timer
        if (hasShield && shieldDurationRemaining > 0f)
        {
            shieldDurationRemaining -= Time.deltaTime;
            if (shieldDurationRemaining <= 0f)
            {
                hasShield = false;
                shieldDurationRemaining = 0f;
                Debug.Log("[PlayerHealth] Shield expired.");
            }
        }

        // Update speed boost timer
        if (speedBoostRemaining > 0f)
        {
            speedBoostRemaining -= Time.deltaTime;
            if (speedBoostRemaining <= 0f)
            {
                ResetSpeedBoost();
            }
        }
    }

    /// <summary>
    /// Applies damage to the player unless shielded or invulnerable.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        // If shielded, absorb the hit completely
        if (hasShield)
        {
            Debug.Log("[PlayerHealth] Shield absorbed laser damage!");
            hasShield = false;
            shieldDurationRemaining = 0f;
            StartCoroutine(InvulnerabilityRoutine(0.4f));
            OnDamageTaken?.Invoke();
            return;
        }

        // Check i-frames
        if (isInvulnerable) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"[PlayerHealth] Took {damage} damage! Remaining HP: {currentHealth}/{maxHealth}");
        NotifyHealthChanged();
        OnDamageTaken?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityRoutine(invulnerabilityDuration));
        }
    }

    /// <summary>
    /// Heals the player by a specified amount (clamped to maxHealth).
    /// </summary>
    public void Heal(int amount)
    {
        if (currentHealth <= 0) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"[PlayerHealth] Restored {amount} HP! Current HP: {currentHealth}/{maxHealth}");
        NotifyHealthChanged();
        OnBonusAcquired?.Invoke($"+{amount} HP Restored");
    }

    /// <summary>
    /// Applies a temporary speed boost multiplier.
    /// </summary>
    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if (playerController == null)
        {
            playerController = GetComponent<Player>();
        }

        if (playerController != null)
        {
            activeSpeedMultiplier = multiplier;
            speedBoostRemaining = duration;
            playerController.MoveSpeed = originalMoveSpeed * multiplier;
            Debug.Log($"[PlayerHealth] Speed Boost activated! ({multiplier}x for {duration}s)");
            OnBonusAcquired?.Invoke($"Speed Boost x{multiplier:F1}");
        }
    }

    private void ResetSpeedBoost()
    {
        speedBoostRemaining = 0f;
        activeSpeedMultiplier = 1f;
        if (playerController != null)
        {
            playerController.MoveSpeed = originalMoveSpeed;
            Debug.Log("[PlayerHealth] Speed Boost ended.");
        }
    }

    /// <summary>
    /// Grants an energy shield that absorbs laser damage for a set duration.
    /// </summary>
    public void ApplyShield(float duration)
    {
        hasShield = true;
        shieldDurationRemaining = duration;
        Debug.Log($"[PlayerHealth] Energy Shield activated for {duration}s!");
        OnBonusAcquired?.Invoke($"Shield Active ({duration:F0}s)");
    }

    private IEnumerator InvulnerabilityRoutine(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }

    private void Die()
    {
        Debug.Log("[PlayerHealth] Player eliminated by corridor laser!");
        OnDeath?.Invoke();
    }

    /// <summary>
    /// Resets all stats when respawned by the GameManager.
    /// </summary>
    public void ResetState()
    {
        currentHealth = maxHealth;
        isInvulnerable = false;
        hasShield = false;
        shieldDurationRemaining = 0f;
        ResetSpeedBoost();
        NotifyHealthChanged();
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
