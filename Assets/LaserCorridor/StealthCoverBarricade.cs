using UnityEngine;

/// <summary>
/// Waist-high concrete barrier used as cover. Its solid collider blocks hazard line-of-sight raycasts and
/// the flashlight beam, so a crouching player behind it is hidden from the turret and drones.
/// </summary>
public class StealthCoverBarricade : MonoBehaviour
{
    [SerializeField] private Vector3 size = new Vector3(2.4f, 1.15f, 0.7f);

    public Vector3 Size
    {
        get => size;
        set => size = value;
    }

    // Built in Start (not Awake) so a Size assigned right after AddComponent is respected.
    private void Start()
    {
        var boxCol = GetComponent<BoxCollider>();
        if (boxCol == null)
            boxCol = gameObject.AddComponent<BoxCollider>();

        boxCol.size = size;
        boxCol.center = Vector3.up * (size.y * 0.5f);
        boxCol.isTrigger = false;

        if (GetComponentInChildren<Renderer>() != null) return;

        // Jersey-barrier profile: wide foot, narrower body, worn hazard paint on the top edge.
        float footH = size.y * 0.3f, bodyH = size.y - footH - 0.1f;
        var concrete = FacilityMaterials.Concrete;
        StealthVisuals.CreateBox("Foot", transform, Vector3.up * (footH * 0.5f), new Vector3(size.x, footH, size.z), concrete);
        StealthVisuals.CreateBox("Body", transform, Vector3.up * (footH + bodyH * 0.5f), new Vector3(size.x, bodyH, size.z * 0.62f), concrete);
        StealthVisuals.CreateBox("HazardCap", transform, Vector3.up * (size.y - 0.05f), new Vector3(size.x, 0.1f, size.z * 0.6f), FacilityMaterials.HazardStripes);
        foreach (var x in new[] { -size.x * 0.35f, size.x * 0.35f })
            StealthVisuals.CreateBox("LiftSlot", transform, new Vector3(x, 0.06f, 0f), new Vector3(0.18f, 0.08f, size.z + 0.002f), FacilityMaterials.Plastic(new Color(0.03f, 0.03f, 0.03f)));
    }
}
