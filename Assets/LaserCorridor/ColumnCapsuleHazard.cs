using UnityEngine;

/// <summary>
/// Patrolling laser column that uses Physics.CheckCapsule for detection.
/// The translucent shell shows the exact sensor radius around the bright core.
/// </summary>
public class ColumnCapsuleHazard : StealthHazardBase
{
    [Header("Capsule Sensor Settings")]
    [SerializeField] private float capsuleRadius = 0.9f;
    [SerializeField] private Vector3 capsuleTopOffset = new Vector3(0f, 4.0f, 0f);
    [SerializeField] private Vector3 capsuleBottomOffset = new Vector3(0f, 0.9f, 0f);

    private Material coreMaterial;
    private Material shellMaterial;
    private Light columnLight;

    protected override bool FacesMoveDirection => false;

    protected override void BuildVisuals()
    {
        // Capsule sensor spans [bottom - radius, top + radius]; Unity's cylinder primitive is 2 units tall.
        float span = Vector3.Distance(capsuleTopOffset, capsuleBottomOffset) + capsuleRadius * 2f;
        Vector3 midPoint = (capsuleTopOffset + capsuleBottomOffset) * 0.5f;

        coreMaterial = StealthVisuals.CreateLit("Column_CoreMat", calmColor);
        StealthVisuals.CreatePrimitive(PrimitiveType.Cylinder, "Column_Core", transform, midPoint,
            new Vector3(0.18f, span * 0.5f, 0.18f), coreMaterial, false);

        shellMaterial = StealthVisuals.CreateTransparent("Column_ShellMat", calmColor);
        StealthVisuals.CreatePrimitive(PrimitiveType.Capsule, "Column_SensorShell", transform, midPoint,
            new Vector3(capsuleRadius * 2f, span * 0.5f, capsuleRadius * 2f), shellMaterial, false);

        columnLight = StealthVisuals.CreateLight("Column_Light", transform, midPoint, LightType.Point, capsuleRadius * 4f, 2f);
    }

    protected override bool PerformPhysicsDetection()
    {
        Vector3 top = transform.TransformPoint(capsuleTopOffset);
        Vector3 bottom = transform.TransformPoint(capsuleBottomOffset);
        return Physics.CheckCapsule(top, bottom, capsuleRadius, playerLayerMask, QueryTriggerInteraction.Ignore);
    }

    protected override void ApplyStateColors(Color color)
    {
        coreMaterial.color = color;
        StealthVisuals.SetEmission(coreMaterial, color * (2f + Mathf.Sin(Time.time * 8f) * 0.3f));

        Color shell = color;
        shell.a = 0.15f;
        shellMaterial.color = shell;

        columnLight.color = color;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = calmColor;
        Vector3 top = transform.TransformPoint(capsuleTopOffset);
        Vector3 bottom = transform.TransformPoint(capsuleBottomOffset);
        Gizmos.DrawWireSphere(top, capsuleRadius);
        Gizmos.DrawWireSphere(bottom, capsuleRadius);
        Gizmos.DrawLine(top, bottom);
    }
}
