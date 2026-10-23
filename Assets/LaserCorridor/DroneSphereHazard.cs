using UnityEngine;

/// <summary>
/// Patrol drone that uses Physics.CheckSphere for its proximity radar, then confirms with
/// line-of-sight raycasts so walls and cover barricades hide the player.
/// </summary>
public class DroneSphereHazard : StealthHazardBase
{
    private const int RingSegments = 40;

    [Header("Sphere Sensor Settings")]
    [SerializeField] private float detectionRadius = 3.2f;
    [Tooltip("Radar radius multiplier while the player is crouching.")]
    [SerializeField] private float crouchRadiusFactor = 0.7f;

    private LineRenderer radarRing;
    private Material bodyMaterial;
    private Material ringMaterial;
    private Light scannerLight;

    private float CurrentRadius => targetPlayer != null && targetPlayer.IsCrouching
        ? detectionRadius * crouchRadiusFactor
        : detectionRadius;

    protected override void BuildVisuals()
    {
        bodyMaterial = StealthVisuals.CreateLit("Drone_BodyMat", calmColor);
        StealthVisuals.CreatePrimitive(PrimitiveType.Sphere, "Drone_Body", transform, Vector3.zero, new Vector3(0.9f, 0.65f, 0.9f), bodyMaterial);

        scannerLight = StealthVisuals.CreateLight("Drone_ScannerLight", transform, Vector3.zero, LightType.Point, detectionRadius * 1.5f, 2f);

        // Unit circle, scaled to the live radar radius each frame.
        var ringObj = new GameObject("Drone_RadarRing");
        ringObj.transform.SetParent(transform, false);
        radarRing = ringObj.AddComponent<LineRenderer>();
        radarRing.useWorldSpace = false;
        radarRing.loop = true;
        radarRing.widthMultiplier = 0.045f;
        radarRing.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        radarRing.positionCount = RingSegments;
        for (int i = 0; i < RingSegments; i++)
        {
            float rad = (float)i / RingSegments * Mathf.PI * 2f;
            radarRing.SetPosition(i, new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)));
        }
        ringMaterial = StealthVisuals.CreateUnlit("Drone_RingMat", calmColor);
        radarRing.sharedMaterial = ringMaterial;
    }

    protected override bool PerformPhysicsDetection()
    {
        float radius = CurrentRadius;
        Vector3 center = transform.position;

        // Broad phase: is any part of the player inside the radar bubble?
        if (!Physics.CheckSphere(center, radius, playerLayerMask, QueryTriggerInteraction.Ignore))
            return false;

        // Narrow phase: the radar can't see through walls or cover.
        return HasLineOfSightToPlayer(center, radius + 2f, transform.forward);
    }

    protected override void Update()
    {
        base.Update();

        float pulse = 1f + Mathf.Sin(Time.time * 5f) * 0.03f;
        radarRing.transform.localScale = Vector3.one * (CurrentRadius * pulse);
        radarRing.transform.Rotate(Vector3.up, 30f * Time.deltaTime, Space.Self);
    }

    protected override void ApplyStateColors(Color color)
    {
        bodyMaterial.color = color;
        StealthVisuals.SetEmission(bodyMaterial, color * 2f);
        scannerLight.color = color;
        ringMaterial.color = color;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = calmColor;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
