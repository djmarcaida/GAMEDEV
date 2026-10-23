using UnityEngine;

/// <summary>
/// Collectible that is invisible unless the player's flashlight is on and the object is inside its light cone.
/// Collection ([E] while looking at it) is handled by <see cref="StealthManager"/>.
/// </summary>
public class HiddenStealthObject : MonoBehaviour
{
    public enum HiddenObjectType
    {
        ClassifiedIntel,
        SecurityOverrideKeycard,
        NaniteMedkit,
        EmpDevice
    }

    [Header("Identity & Reward")]
    [SerializeField] private string objectName = "Classified Schematics";
    [SerializeField] private HiddenObjectType objectType = HiddenObjectType.ClassifiedIntel;
    [SerializeField] private float targetRadius = 0.3f;
    [SerializeField] private float interactDistance = 3.0f;

    [Header("Visual Feedback")]
    [SerializeField] private Color revealColor = new Color(0.2f, 0.95f, 1.0f);
    [SerializeField] private float revealFadeSpeed = 5.0f;

    private Renderer objectRenderer;
    private Material objectMat;
    private Light revealLight;
    private bool isIlluminated;
    private float visibility;

    public string ObjectName => objectName;
    public bool IsCollected { get; private set; }

    /// <summary>Fully faded in and therefore collectable.</summary>
    public bool IsRevealed => isIlluminated && visibility > 0.5f;

    public float InteractDistance => interactDistance;

    public void Configure(string displayName, HiddenObjectType type)
    {
        objectName = displayName;
        objectType = type;
    }

    private void Start()
    {
        objectMat = StealthVisuals.CreateTransparent("HiddenObject_Mat", revealColor, true);
        objectRenderer = StealthVisuals.CreatePrimitive(PrimitiveType.Cube, "HiddenMesh", transform, Vector3.zero,
            new Vector3(0.45f, 0.35f, 0.25f), objectMat, false);

        revealLight = StealthVisuals.CreateLight("RevealGlowLight", transform, Vector3.up * 0.2f, LightType.Point, 2.5f, 0f);
        revealLight.color = revealColor;

        ApplyVisibility();
    }

    private void Update()
    {
        var flashlight = PlayerFlashlight.Instance;
        bool lit = flashlight != null && flashlight.IsInLightCone(transform.position, targetRadius);

        isIlluminated = lit;

        visibility = Mathf.MoveTowards(visibility, lit ? 1f : 0f, Time.deltaTime * revealFadeSpeed);
        ApplyVisibility();

        if (visibility > 0f)
            transform.Rotate(Vector3.up, 45f * visibility * Time.deltaTime, Space.World);
    }

    private void ApplyVisibility()
    {
        if (objectRenderer == null) return;

        objectRenderer.enabled = visibility > 0.01f;
        Color c = revealColor;
        c.a = visibility;
        objectMat.color = c;
        StealthVisuals.SetEmission(objectMat, revealColor * (visibility * 2.5f * (1f + Mathf.Sin(Time.time * 6f) * 0.25f)));

        revealLight.enabled = visibility > 0.05f;
        revealLight.intensity = visibility * 1.8f;
    }

    public void Collect(PlayerHealth playerHealth)
    {
        if (IsCollected) return;

        IsCollected = true;
        if (StealthAudio.Instance != null)
            StealthAudio.Instance.PlaySecretCollected();

        ApplyReward(playerHealth);
        gameObject.SetActive(false);
    }

    private void ApplyReward(PlayerHealth playerHealth)
    {
        var manager = StealthManager.Instance;
        switch (objectType)
        {
            case HiddenObjectType.ClassifiedIntel:
                if (playerHealth != null) playerHealth.Heal(20);
                break;

            case HiddenObjectType.SecurityOverrideKeycard:
                if (manager != null) manager.ApplyTemporaryHazardSlowdown(0.4f, 8f);
                break;

            case HiddenObjectType.NaniteMedkit:
                if (playerHealth != null) playerHealth.Heal(50);
                break;

            case HiddenObjectType.EmpDevice:
                if (StealthAudio.Instance != null) StealthAudio.Instance.PlayEmpDischarge();
                if (manager != null) manager.TriggerEmpBlast(6f);
                break;
        }
    }

    public void ResetState()
    {
        IsCollected = false;
        isIlluminated = false;
        visibility = 0f;
        gameObject.SetActive(true);
        ApplyVisibility();
    }
}
