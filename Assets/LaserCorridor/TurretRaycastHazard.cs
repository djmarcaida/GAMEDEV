using UnityEngine;

/// <summary>
/// Ceiling-mounted surveillance camera that uses Physics.Raycast for detection: line-of-sight rays
/// inside its searchlight cone, plus sweeping laser tripwires. Cover barricades block both.
/// </summary>
public class TurretRaycastHazard : StealthHazardBase
{
    [Header("Raycast Vision Settings")]
    [SerializeField] private float visionRange = 22f;
    [SerializeField] private float visionConeAngle = 48f;
    [SerializeField] private float visionPitch = 35f;
    [SerializeField] private Vector3 lensOffset = new Vector3(0f, -0.3f, 0.4f);

    [Header("Turret Sweep")]
    [SerializeField] private float panAngleRange = 55f;
    [SerializeField] private float panSpeed = 22f;

    [Header("Laser Tripwires")]
    [SerializeField] private int tripwireCount = 3;
    [SerializeField] private float tripwireSpreadAngle = 30f;
    [SerializeField] private float tripwirePitch = 38f;

    private Transform turretHead;
    private Transform lens;
    private Light searchLight;
    private LineRenderer[] tripwires;
    private Material turretMaterial;
    private Material laserMaterial;
    private float currentPanAngle;
    private bool panningRight = true;

    protected override bool FacesMoveDirection => false;

    protected override void BuildVisuals()
    {
        turretMaterial = StealthVisuals.CreateLit("Turret_Mat", calmColor);
        StealthVisuals.CreatePrimitive(PrimitiveType.Cylinder, "Turret_CeilingMount", transform, Vector3.up * 0.2f, new Vector3(0.8f, 0.15f, 0.8f), turretMaterial);

        turretHead = new GameObject("Turret_Head").transform;
        turretHead.SetParent(transform, false);
        StealthVisuals.CreatePrimitive(PrimitiveType.Cube, "Turret_HeadMesh", turretHead, Vector3.zero, new Vector3(0.55f, 0.45f, 0.75f), turretMaterial);
        lens = StealthVisuals.CreatePrimitive(PrimitiveType.Sphere, "Turret_Lens", turretHead, lensOffset, Vector3.one * 0.3f, turretMaterial).transform;
        lens.localRotation = Quaternion.Euler(visionPitch, 0f, 0f);

        searchLight = StealthVisuals.CreateLight("Turret_Searchlight", lens, Vector3.zero, LightType.Spot, visionRange, 3.5f);
        searchLight.spotAngle = visionConeAngle;
        searchLight.innerSpotAngle = visionConeAngle * 0.65f;
        searchLight.shadows = LightShadows.Soft;

        laserMaterial = StealthVisuals.CreateUnlit("Turret_LaserMat", calmColor);
        tripwires = new LineRenderer[tripwireCount];
        for (int i = 0; i < tripwireCount; i++)
        {
            var wireObj = new GameObject($"Tripwire_{i}");
            wireObj.transform.SetParent(turretHead, false);
            var lr = wireObj.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.widthMultiplier = 0.025f;
            lr.sharedMaterial = laserMaterial;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            tripwires[i] = lr;
        }
    }

    // Stationary mount: "patrolling" means panning the head left and right.
    protected override void UpdatePatrolMovement()
    {
        float delta = panSpeed * speedMultiplier * Time.deltaTime;
        currentPanAngle += panningRight ? delta : -delta;
        if (Mathf.Abs(currentPanAngle) >= panAngleRange)
        {
            currentPanAngle = Mathf.Clamp(currentPanAngle, -panAngleRange, panAngleRange);
            panningRight = !panningRight;
        }

        turretHead.localRotation = Quaternion.Euler(0f, currentPanAngle, 0f);
    }

    protected override bool PerformPhysicsDetection()
    {
        Vector3 lensPos = lens.position;

        // 1. Camera vision: raycasts to head/chest/feet that fall inside the searchlight cone.
        bool seen = HasLineOfSightToPlayer(lensPos, visionRange, lens.forward, visionConeAngle * 0.5f);

        // 2. Tripwires: one ray each, always updated so the beams track the head.
        bool tripped = false;
        float wireLength = visionRange * 1.2f;
        for (int i = 0; i < tripwires.Length; i++)
        {
            float yaw = tripwireCount > 1 ? ((float)i / (tripwireCount - 1) - 0.5f) * tripwireSpreadAngle : 0f;
            Vector3 dir = turretHead.rotation * Quaternion.Euler(tripwirePitch, yaw, 0f) * Vector3.forward;
            Vector3 end = lensPos + dir * wireLength;

            if (Physics.Raycast(lensPos, dir, out RaycastHit hit, wireLength, sightMask, QueryTriggerInteraction.Ignore))
            {
                end = hit.point;
                tripped |= IsPlayerCollider(hit.collider);
            }

            tripwires[i].SetPosition(0, lensPos);
            tripwires[i].SetPosition(1, end);
        }

        return seen || tripped;
    }

    protected override void ApplyStateColors(Color color)
    {
        turretMaterial.color = color;
        StealthVisuals.SetEmission(turretMaterial, color * 1.8f);
        searchLight.color = color;
        laserMaterial.color = color;
    }

    public override void ResetHazard()
    {
        base.ResetHazard();
        currentPanAngle = 0f;
        panningRight = true;
        if (turretHead != null) turretHead.localRotation = Quaternion.identity;
    }

    private void OnDrawGizmosSelected()
    {
        if (lens == null) return;
        Gizmos.color = calmColor;
        Gizmos.DrawRay(lens.position, lens.forward * visionRange);
    }
}
