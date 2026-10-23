using UnityEngine;

/// <summary>
/// First-person flashlight held slightly below and to the right of the camera.
/// The beam lags the view a little like a hand-held torch, uses a reflector-pattern cookie, and casts soft shadows.
/// Exposes a light-cone test used to reveal hidden objects (and to draw hazards' attention).
/// </summary>
[RequireComponent(typeof(Camera))]
public class PlayerFlashlight : MonoBehaviour
{
    public static PlayerFlashlight Instance { get; private set; }

    [Header("Controls")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F;
    [SerializeField] private KeyCode alternateToggleKey = KeyCode.Mouse1;

    [Header("Light Parameters")]
    [SerializeField] private bool startsOn = true;
    [SerializeField] private float range = 38f;
    [SerializeField] private float spotAngle = 96f;   // 2x the beam radius of the old 58° cone (radius scales with tan(angle / 2))
    [SerializeField] private float innerSpotAngle = 50f;
    [SerializeField] private float targetIntensity = 20f; // raised so the wider beam keeps its brightness
    [SerializeField] private Color lightColor = new Color(1f, 0.96f, 0.88f);
    [SerializeField] private float smoothSpeed = 16f;

    [Header("Hand-held Feel")]
    [Tooltip("Beam position relative to the camera. Offsetting it from the eye makes shadows visible.")]
    [SerializeField] private Vector3 holdOffset = new Vector3(0.22f, -0.2f, 0.1f);
    [Tooltip("How quickly the beam catches up with the view direction.")]
    [SerializeField] private float followSharpness = 14f;

    [Tooltip("Layers that block the beam. Defaults to everything except the Player layer.")]
    [SerializeField] private LayerMask obstacleMask;

    private Transform beam;
    private Light spotLight;
    private bool isOn;
    private float currentIntensity;

    public bool IsOn => isOn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        if (obstacleMask.value == 0)
        {
            obstacleMask = Physics.DefaultRaycastLayers;
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1) obstacleMask &= ~(1 << playerLayer);
        }

        beam = new GameObject("FlashlightBeam").transform;
        beam.SetParent(transform, false);
        beam.localPosition = holdOffset;

        spotLight = beam.gameObject.AddComponent<Light>();
        spotLight.type = LightType.Spot;
        spotLight.range = range;
        spotLight.spotAngle = spotAngle;
        spotLight.innerSpotAngle = innerSpotAngle;
        spotLight.color = lightColor;
        spotLight.cookie = ProceduralTextures.FlashlightCookie();
        spotLight.shadows = LightShadows.Soft;
        spotLight.shadowStrength = 0.9f;
        spotLight.shadowNearPlane = 0.1f;

        isOn = startsOn;
        currentIntensity = isOn ? targetIntensity : 0f;
        ApplyIntensity();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(alternateToggleKey))
            SetOn(!isOn);

        currentIntensity = Mathf.Lerp(currentIntensity, isOn ? targetIntensity : 0f, Time.deltaTime * smoothSpeed);
        ApplyIntensity();
    }

    private void LateUpdate()
    {
        // Runs after the camera has been rotated this frame; the beam eases toward where the player looks.
        float t = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
        beam.rotation = Quaternion.Slerp(beam.rotation, transform.rotation, t);
    }

    private void ApplyIntensity()
    {
        spotLight.intensity = currentIntensity;
        spotLight.enabled = currentIntensity > 0.01f;
    }

    public void SetOn(bool on)
    {
        if (isOn == on) return;

        isOn = on;
        if (StealthAudio.Instance != null)
            StealthAudio.Instance.PlayFlashlightToggle(isOn);
    }

    /// <summary>
    /// True if the flashlight is on and <paramref name="targetPosition"/> is within range, inside the
    /// spot cone, and not occluded by level geometry.
    /// </summary>
    public bool IsInLightCone(Vector3 targetPosition, float targetRadius)
    {
        if (!isOn) return false;

        Vector3 origin = beam.position;
        Vector3 toTarget = targetPosition - origin;
        float dist = toTarget.magnitude;

        if (dist > range || dist < 0.001f) return false;
        // Only the bright part of the cookie reveals things, not the faint outer spill.
        if (Vector3.Angle(beam.forward, toTarget) > spotAngle * 0.4f) return false;

        // Anything solid noticeably in front of the target blocks the beam.
        return !Physics.Raycast(origin, toTarget / dist, Mathf.Max(0f, dist - targetRadius), obstacleMask, QueryTriggerInteraction.Ignore);
    }
}
