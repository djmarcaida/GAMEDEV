using System.Collections;
using UnityEngine;

/// <summary>
/// Floor panel that grants bonuses when the player steps on it.
/// Supports Health, Speed Boost, Energy Shield, or Laser Slowdown.
/// </summary>
public class BonusFloorPanel : MonoBehaviour
{
    public enum BonusType
    {
        HealthRestore,
        SpeedBoost,
        EnergyShield,
        LaserSlowdown
    }

    [Header("Bonus Configuration")]
    [SerializeField] private BonusType bonusType = BonusType.HealthRestore;
    [SerializeField] private int healthAmount = 35;
    [SerializeField] private float speedMultiplier = 1.6f;
    [SerializeField] private float buffDuration = 5.0f;

    [Header("Panel Behavior")]
    [SerializeField] private bool singleUsePerRound = true;
    [SerializeField] private float cooldownTime = 10.0f;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer panelRenderer;
    [SerializeField] private Light panelLight;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.gray;

    private bool isAvailable = true;
    private Material panelMat;

    public BonusType Type => bonusType;
    public bool IsAvailable => isAvailable;

    private void Start()
    {
        if (panelRenderer == null)
        {
            panelRenderer = GetComponent<Renderer>();
        }

        if (panelRenderer != null)
        {
            panelMat = panelRenderer.material;
        }

        // Configure default colors based on bonus type
        ApplyDefaultColors();
        SetVisualState(true);

        // Ensure collider is a trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void ApplyDefaultColors()
    {
        switch (bonusType)
        {
            case BonusType.HealthRestore:
                activeColor = new Color(0.1f, 0.9f, 0.2f); // Vibrant Green
                break;
            case BonusType.SpeedBoost:
                activeColor = new Color(1.0f, 0.85f, 0.1f); // Vibrant Yellow / Gold
                break;
            case BonusType.EnergyShield:
                activeColor = new Color(0.1f, 0.7f, 1.0f); // Sci-Fi Cyan
                break;
            case BonusType.LaserSlowdown:
                activeColor = new Color(0.8f, 0.2f, 1.0f); // Purple
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isAvailable) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            GrantBonus(playerHealth);
        }
    }

    private void GrantBonus(PlayerHealth playerHealth)
    {
        isAvailable = false;
        SetVisualState(false);

        switch (bonusType)
        {
            case BonusType.HealthRestore:
                playerHealth.Heal(healthAmount);
                break;

            case BonusType.SpeedBoost:
                playerHealth.ApplySpeedBoost(speedMultiplier, buffDuration);
                break;

            case BonusType.EnergyShield:
                playerHealth.ApplyShield(buffDuration);
                break;

            case BonusType.LaserSlowdown:
                SlowDownLasers();
                break;
        }

        if (!singleUsePerRound)
        {
            StartCoroutine(CooldownRoutine());
        }
    }

    private void SlowDownLasers()
    {
        LaserBeam[] lasers = FindObjectsByType<LaserBeam>(FindObjectsSortMode.None);
        foreach (var laser in lasers)
        {
            laser.SetSpeedMultiplier(0.5f);
        }
        Debug.Log("[BonusFloorPanel] Lasers slowed down!");
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldownTime);
        ResetPanel();
    }

    /// <summary>
    /// Resets the panel so it can be picked up again (called on game restart).
    /// </summary>
    public void ResetPanel()
    {
        isAvailable = true;
        SetVisualState(true);
    }

    private void SetVisualState(bool active)
    {
        Color targetColor = active ? activeColor : inactiveColor;

        if (panelMat != null)
        {
            panelMat.color = targetColor;
            if (panelMat.HasProperty("_EmissionColor"))
            {
                panelMat.EnableKeyword("_EMISSION");
                panelMat.SetColor("_EmissionColor", active ? targetColor * 1.8f : Color.black);
            }
        }

        if (panelLight != null)
        {
            panelLight.color = targetColor;
            panelLight.intensity = active ? 2.5f : 0.2f;
        }
    }
}
