using System.Collections;
using UnityEngine;

public class BonusFloorPanel : MonoBehaviour
{
    public enum BonusType
    {
        HealthRestore,
        SpeedBoost,
        EnergyShield,
        LaserSlowdown
    }

    [SerializeField] private BonusType bonusType = BonusType.HealthRestore;
    [SerializeField] private int healthAmount = 35;
    [SerializeField] private float speedMultiplier = 1.6f;
    [SerializeField] private float buffDuration = 5.0f;

    [SerializeField] private bool singleUsePerRound = true;
    [SerializeField] private float cooldownTime = 10.0f;

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
            panelRenderer = GetComponent<Renderer>();

        if (panelRenderer != null)
            panelMat = panelRenderer.material;

        ConfigureColors();
        SetVisuals(true);

        if (TryGetComponent<Collider>(out var col))
            col.isTrigger = true;
    }

    private void ConfigureColors()
    {
        activeColor = bonusType switch
        {
            BonusType.HealthRestore => new Color(0.1f, 0.9f, 0.2f),
            BonusType.SpeedBoost => new Color(1.0f, 0.85f, 0.1f),
            BonusType.EnergyShield => new Color(0.1f, 0.7f, 1.0f),
            BonusType.LaserSlowdown => new Color(0.8f, 0.2f, 1.0f),
            _ => Color.white
        };
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isAvailable) return;

        if (other.TryGetComponent<PlayerHealth>(out var health) ||
            (other.transform.parent != null && other.transform.parent.TryGetComponent(out health)))
        {
            ApplyBonus(health);
        }
    }

    private void ApplyBonus(PlayerHealth playerHealth)
    {
        isAvailable = false;
        SetVisuals(false);

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
            StartCoroutine(CooldownRoutine());
    }

    private void SlowDownLasers()
    {
        var lasers = FindObjectsByType<LaserBeam>(FindObjectsSortMode.None);
        foreach (var laser in lasers)
        {
            if (laser != null)
                laser.SetSpeedMultiplier(0.5f);
        }
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldownTime);
        ResetPanel();
    }

    public void ResetPanel()
    {
        isAvailable = true;
        SetVisuals(true);
    }

    private void SetVisuals(bool active)
    {
        Color color = active ? activeColor : inactiveColor;

        if (panelMat != null)
        {
            panelMat.color = color;
            if (panelMat.HasProperty("_EmissionColor"))
            {
                panelMat.EnableKeyword("_EMISSION");
                panelMat.SetColor("_EmissionColor", active ? color * 1.8f : Color.black);
            }
        }

        if (panelLight != null)
        {
            panelLight.color = color;
            panelLight.intensity = active ? 2.5f : 0.2f;
        }
    }
}
