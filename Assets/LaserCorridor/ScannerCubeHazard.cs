using UnityEngine;

/// <summary>
/// Sweeping laser curtain that uses Physics.CheckBox for volume detection.
/// The curtain's bottom edge sits above crouch height, so a crouching player can crawl underneath.
/// </summary>
public class ScannerCubeHazard : StealthHazardBase
{
    [Header("Box Sensor Settings")]
    [SerializeField] private Vector3 boxSize = new Vector3(7.6f, 2.4f, 1.0f);
    [Tooltip("Local centre of the box. The default leaves a 1.3m crawlspace under the curtain.")]
    [SerializeField] private Vector3 boxOffset = new Vector3(0f, 2.5f, 0f);

    private Material gridMaterial;
    private Material gantryMaterial;
    private Light scannerLight;

    /// <summary>Sensor box size; set before Start (e.g. right after AddComponent).</summary>
    public Vector3 BoxSize
    {
        get => boxSize;
        set => boxSize = value;
    }

    // The curtain keeps its orientation across the corridor while sliding back and forth.
    protected override bool FacesMoveDirection => false;

    protected override void BuildVisuals()
    {
        gantryMaterial = StealthVisuals.CreateLit("Scanner_GantryMat", calmColor);
        StealthVisuals.CreatePrimitive(PrimitiveType.Cube, "Scanner_Gantry", transform,
            boxOffset + Vector3.up * (boxSize.y * 0.5f + 0.15f), new Vector3(boxSize.x + 0.4f, 0.3f, 0.5f), gantryMaterial);

        gridMaterial = StealthVisuals.CreateTransparent("Scanner_GridMat", calmColor);
        StealthVisuals.CreatePrimitive(PrimitiveType.Cube, "Scanner_Grid", transform, boxOffset, boxSize, gridMaterial, false);

        // Bright edge along the bottom of the curtain so the crawlspace is easy to read.
        StealthVisuals.CreatePrimitive(PrimitiveType.Cube, "Scanner_BottomEdge", transform,
            boxOffset - Vector3.up * (boxSize.y * 0.5f), new Vector3(boxSize.x, 0.04f, boxSize.z), gantryMaterial, false);

        scannerLight = StealthVisuals.CreateLight("Scanner_Light", transform, boxOffset, LightType.Spot, boxSize.y * 2f, 2.5f);
        scannerLight.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        scannerLight.spotAngle = 80f;
    }

    protected override bool PerformPhysicsDetection()
    {
        Vector3 center = transform.TransformPoint(boxOffset);
        return Physics.CheckBox(center, boxSize * 0.5f, transform.rotation, playerLayerMask, QueryTriggerInteraction.Ignore);
    }

    protected override void ApplyStateColors(Color color)
    {
        gantryMaterial.color = color * 0.8f;
        StealthVisuals.SetEmission(gantryMaterial, color * 1.5f);

        Color gridColor = color;
        gridColor.a = 0.18f + Mathf.Sin(Time.time * 6f) * 0.06f;
        gridMaterial.color = gridColor;

        scannerLight.color = color;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = calmColor;
        Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(boxOffset), transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}
