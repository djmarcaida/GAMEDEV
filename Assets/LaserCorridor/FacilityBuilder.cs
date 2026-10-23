using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Turns the bare corridor into a dark research facility: re-textures the scene shell, adds structure
/// (pilasters, beams, pipes, ducts, trim), furnishes each zone, and sets up dark lighting, fog and post-processing
/// so the player's flashlight is the main light source.
///
/// Zones along the corridor (player enters at z = -56):
///   z -60..-48 lobby, -48..-32 offices, -32..-16 storage, -16..16 security atrium, 16..40 server room, 40..60 loading bay / exit.
/// </summary>
public class FacilityBuilder : MonoBehaviour
{
    private const float W = CorridorLayout.HalfWidth;
    private const float L = CorridorLayout.HalfLength;
    private const float H = CorridorLayout.Height;
    private const float WallTrim = 0.03f; // wainscot thickness

    // Left-wall props face +X, right-wall props face -X (see FacilityProps convention).
    private const float FaceRight = -90f;
    private const float FaceLeft = 90f;

    [Header("Atmosphere")]
    [SerializeField] private Color ambientColor = new Color(0.035f, 0.038f, 0.048f);
    [SerializeField] private Color fogColor = new Color(0.02f, 0.022f, 0.028f);
    [SerializeField] private float fogDensity = 0.022f;

    private Transform root;

    private void Awake()
    {
        root = new GameObject("Facility_Environment").transform;

        ApplyAtmosphere();
        ReskinShell();
        BuildStructure();
        BuildCeilingLights();
        BuildLobby();
        BuildOffices();
        BuildStorage();
        BuildAtrium();
        BuildServerRoom();
        BuildLoadingBay();

        // Nothing under the environment root moves, so batch it to keep draw calls down.
        StaticBatchingUtility.Combine(root.gameObject);
    }

    // ---------------------------------------------------------------- Lighting & post

    private void ApplyAtmosphere()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColor;
        var probe = new SphericalHarmonicsL2();
        probe.AddAmbientLight(ambientColor);
        RenderSettings.ambientProbe = probe;
        RenderSettings.reflectionIntensity = 0.08f;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;

        // The original corridor lights flood the hall; the new fixtures replace them.
        foreach (var l in FindObjectsByType<Light>())
            if (l.name.StartsWith("Ceiling_Light"))
                l.enabled = false;

        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = fogColor;
        }

        var volumeObj = new GameObject("Facility_PostProcess");
        volumeObj.transform.SetParent(root, false);
        var volume = volumeObj.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 100f;
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        volume.sharedProfile = profile;

        var tone = profile.Add<Tonemapping>(true);
        tone.mode.Override(TonemappingMode.ACES);

        var color = profile.Add<ColorAdjustments>(true);
        color.postExposure.Override(0.8f);
        color.contrast.Override(18f);
        color.saturation.Override(-18f);

        var bloom = profile.Add<Bloom>(true);
        bloom.threshold.Override(0.85f);
        bloom.intensity.Override(0.8f);
        bloom.scatter.Override(0.7f);

        var vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.32f);
        vignette.smoothness.Override(0.5f);

        var grain = profile.Add<FilmGrain>(true);
        grain.type.Override(FilmGrainLookup.Medium3);
        grain.intensity.Override(0.3f);
        grain.response.Override(0.85f);

        var aberration = profile.Add<ChromaticAberration>(true);
        aberration.intensity.Override(0.12f);
    }

    // ---------------------------------------------------------------- Shell & structure

    /// <summary>Swaps the scene's stretched cubes for UV-correct meshes with facility materials.</summary>
    private static void ReskinShell()
    {
        var corridor = GameObject.Find("LaserCorridor");
        if (corridor == null) return;

        Reskin(corridor.transform.Find("Floor"), FacilityMaterials.Floor);
        Reskin(corridor.transform.Find("Ceiling"), FacilityMaterials.Ceiling);
        Reskin(corridor.transform.Find("LeftWall"), FacilityMaterials.Wall);
        Reskin(corridor.transform.Find("RightWall"), FacilityMaterials.Wall);
        Reskin(corridor.transform.Find("EntranceWall"), FacilityMaterials.Wall);
        Reskin(corridor.transform.Find("FarExitWall"), FacilityMaterials.Wall);
    }

    private static void Reskin(Transform t, Material mat)
    {
        if (t == null) return;

        Vector3 size = t.localScale;
        t.localScale = Vector3.one;
        if (t.TryGetComponent(out MeshFilter mf)) mf.sharedMesh = StealthVisuals.GetBoxMesh(size);
        if (t.TryGetComponent(out BoxCollider col)) col.size = size;
        if (t.TryGetComponent(out MeshRenderer mr)) mr.sharedMaterial = mat;
    }

    private void BuildStructure()
    {
        var concrete = FacilityMaterials.Concrete;
        var wainscot = FacilityMaterials.PaintedMetal(new Color(0.15f, 0.21f, 0.23f));
        var rubber = FacilityMaterials.Plastic(new Color(0.06f, 0.06f, 0.06f));
        var brushed = FacilityMaterials.BrushedMetal;
        float length = L * 2f;

        // Pilasters and ceiling beams frame each 8m bay.
        for (int i = 0; i < CorridorLayout.PilasterCount; i++)
        {
            float z = CorridorLayout.FirstPilasterZ + i * CorridorLayout.BaySize;
            foreach (var side in new[] { -1f, 1f })
                StealthVisuals.CreateBox("Pilaster", root, new Vector3(side * (W - 0.2f), H * 0.5f, z), new Vector3(0.4f, H, 0.6f), concrete, true);
            StealthVisuals.CreateBox("CeilingBeam", root, new Vector3(0f, H - 0.225f, z), new Vector3(W * 2f, 0.45f, 0.35f), concrete);
        }

        // Wall trim: dark painted wainscot, steel chair rail, rubber skirting.
        foreach (var side in new[] { -1f, 1f })
        {
            StealthVisuals.CreateBox("Wainscot", root, new Vector3(side * (W - WallTrim * 0.5f), 0.55f, 0f), new Vector3(WallTrim, 1.1f, length), wainscot);
            StealthVisuals.CreateBox("ChairRail", root, new Vector3(side * (W - 0.035f), 1.12f, 0f), new Vector3(0.04f, 0.05f, length), brushed);
            StealthVisuals.CreateBox("Skirting", root, new Vector3(side * (W - WallTrim - 0.012f), 0.06f, 0f), new Vector3(0.025f, 0.12f, length), rubber);
        }

        // Floor paint: yellow walkway lines and hazard bands at both ends of the laser-curtain zone.
        var yellowPaint = FacilityMaterials.Plastic(new Color(0.8f, 0.6f, 0.05f));
        foreach (var side in new[] { -1f, 1f })
            StealthVisuals.CreateBox("FloorLine", root, new Vector3(side * 4.7f, 0.0015f, 0f), new Vector3(0.08f, 0.003f, length - 2f), yellowPaint);
        foreach (var z in new[] { -31.8f, -16.2f })
            StealthVisuals.CreateBox("HazardBand", root, new Vector3(0f, 0.002f, z), new Vector3(W * 2f - 0.1f, 0.004f, 0.35f), FacilityMaterials.HazardStripes);

        // Service runs: three pipes on the left, a cable tray on the right, an air duct off-centre.
        var pipeGrey = FacilityMaterials.PaintedMetal(new Color(0.45f, 0.47f, 0.48f));
        var pipeRed = FacilityMaterials.PaintedMetal(new Color(0.6f, 0.06f, 0.05f));
        var pipeWhite = FacilityMaterials.PaintedMetal(new Color(0.8f, 0.8f, 0.76f));
        Pipe(new Vector3(-5.5f, 5.3f, 0f), 0.2f, length, pipeWhite);
        Pipe(new Vector3(-5.2f, 5.35f, 0f), 0.12f, length, pipeRed);
        Pipe(new Vector3(-4.98f, 5.38f, 0f), 0.08f, length, pipeGrey);
        for (float z = -58f; z <= 58f; z += 4f)
        {
            StealthVisuals.CreateBox("PipeHanger", root, new Vector3(-5.25f, 5.18f, z), new Vector3(0.75f, 0.04f, 0.05f), brushed);
            StealthVisuals.CreateBox("PipeRod", root, new Vector3(-4.9f, 5.58f, z), new Vector3(0.02f, 0.8f, 0.02f), brushed);
        }

        StealthVisuals.CreateBox("CableTray", root, new Vector3(5.45f, 5.1f, 0f), new Vector3(0.45f, 0.02f, length), brushed);
        foreach (var x in new[] { 5.23f, 5.67f })
            StealthVisuals.CreateBox("CableTrayLip", root, new Vector3(x, 5.15f, 0f), new Vector3(0.02f, 0.1f, length), brushed);
        var cable = FacilityMaterials.Plastic(new Color(0.05f, 0.05f, 0.05f));
        foreach (var x in new[] { 5.33f, 5.42f, 5.5f, 5.58f })
            Pipe(new Vector3(x, 5.13f, 0f), 0.035f, length, cable);

        var duct = FacilityMaterials.BrushedMetal;
        StealthVisuals.CreateBox("AirDuct", root, new Vector3(2.6f, 5.3f, 0f), new Vector3(0.8f, 0.5f, length), duct);
        for (float z = -L + 1f; z < L; z += 1.5f)
            StealthVisuals.CreateBox("DuctSeam", root, new Vector3(2.6f, 5.3f, z), new Vector3(0.84f, 0.54f, 0.04f), duct);

        // Wall vents high up on alternate bays.
        for (int i = 0; i < 14; i++)
        {
            float z = CorridorLayout.FirstPilasterZ + CorridorLayout.BaySize * (i + 0.5f);
            bool left = i % 2 == 0;
            FacilityProps.WallVent(root, new Vector3(left ? -W : W, 4.4f, z + 2.2f), left ? FaceRight : FaceLeft);
        }

        // Red emergency beacons on alternating pilasters.
        for (int i = 1; i < CorridorLayout.PilasterCount - 1; i += 2)
        {
            float z = CorridorLayout.FirstPilasterZ + i * CorridorLayout.BaySize;
            bool left = (i / 2) % 2 == 0;
            FacilityProps.EmergencyLight(root, new Vector3(left ? -(W - 0.4f) : W - 0.4f, 4.0f, z), left ? FaceRight : FaceLeft);
        }
    }

    private void Pipe(Vector3 center, float diameter, float length, Material mat)
    {
        var pipe = StealthVisuals.CreatePrimitive(PrimitiveType.Cylinder, "Pipe", root, center, new Vector3(diameter, length * 0.5f, diameter), mat);
        pipe.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private void BuildCeilingLights()
    {
        var cold = new Color(0.82f, 0.9f, 1f);
        var warm = new Color(1f, 0.9f, 0.75f);
        var sodium = new Color(1f, 0.62f, 0.3f);
        var blue = new Color(0.55f, 0.75f, 1f);

        // One fixture per bay; most are dead, a few work, some are failing.
        var plan = new (FacilityProps.FixtureState state, Color color)[]
        {
            (FacilityProps.FixtureState.On, warm),      // -52 lobby
            (FacilityProps.FixtureState.Faulty, cold),  // -44
            (FacilityProps.FixtureState.Faulty, cold),  // -36
            (FacilityProps.FixtureState.Off, cold),     // -28
            (FacilityProps.FixtureState.On, cold),      // -20
            (FacilityProps.FixtureState.Off, cold),     // -12
            (FacilityProps.FixtureState.Faulty, cold),  // -4
            (FacilityProps.FixtureState.On, cold),      // 4
            (FacilityProps.FixtureState.Off, cold),     // 12 (turret mount; skipped below)
            (FacilityProps.FixtureState.Off, blue),     // 20
            (FacilityProps.FixtureState.Faulty, blue),  // 28
            (FacilityProps.FixtureState.On, blue),      // 36
            (FacilityProps.FixtureState.On, sodium),    // 44
            (FacilityProps.FixtureState.Off, cold),     // 52
        };

        for (int i = 0; i < plan.Length; i++)
        {
            float z = CorridorLayout.FirstPilasterZ + CorridorLayout.BaySize * (i + 0.5f);
            if (Mathf.Approximately(z, 12f)) continue; // The security camera hangs here.
            FacilityProps.CeilingFixture(root, new Vector3(0f, 5.35f, z), plan[i].state, plan[i].color);
        }
    }

    // ---------------------------------------------------------------- Zones

    private static float LeftX(float depth) => -W + WallTrim + depth * 0.5f;
    private static float RightX(float depth) => W - WallTrim - depth * 0.5f;

    private void BuildLobby()
    {
        FacilityProps.Door(root, new Vector3(0f, 0f, -L), 180f, new Color(0.3f, 0.32f, 0.34f));
        FacilityProps.NoticeBoard(root, new Vector3(-W + WallTrim, 1.6f, -58.3f), FaceRight, 1);
        FacilityProps.LockerRow(root, new Vector3(LeftX(0.5f), 0f, -52.5f), FaceRight, 6);
        FacilityProps.Bench(root, new Vector3(-4.9f, 0f, -52.5f), FaceRight);
        FacilityProps.TrashBin(root, new Vector3(-5.6f, 0f, -54.3f));
        FacilityProps.Door(root, new Vector3(-W + WallTrim, 0f, -50f), FaceRight, new Color(0.25f, 0.35f, 0.45f));

        FacilityProps.ReceptionDesk(root, new Vector3(4.3f, 0f, -52f), FaceLeft);
        FacilityProps.OfficeChair(root, new Vector3(5.2f, 0f, -52.3f), FaceLeft + 15f);
        FacilityProps.WaterCooler(root, new Vector3(RightX(0.32f), 0f, -58.5f), FaceLeft);
        FacilityProps.FireExtinguisher(root, new Vector3(W - WallTrim, 0f, -55f), FaceLeft);
    }

    private void BuildOffices()
    {
        float[] leftDesks = { -46.2f, -42.8f, -37.6f, -34.4f };
        float[] rightDesks = { -46.4f, -42.2f, -37.8f, -34.2f };
        for (int i = 0; i < leftDesks.Length; i++)
        {
            FacilityProps.Desk(root, new Vector3(LeftX(0.8f), 0f, leftDesks[i]), FaceRight);
            FacilityProps.OfficeChair(root, new Vector3(-4.75f, 0f, leftDesks[i] + 0.1f), FaceLeft + (i * 23f % 40f - 20f));
        }
        for (int i = 0; i < rightDesks.Length; i++)
        {
            FacilityProps.Desk(root, new Vector3(RightX(0.8f), 0f, rightDesks[i]), FaceLeft);
            FacilityProps.OfficeChair(root, new Vector3(4.75f, 0f, rightDesks[i] - 0.1f), FaceRight + (i * 31f % 40f - 20f));
        }

        FacilityProps.FilingCabinet(root, new Vector3(LeftX(0.62f), 0f, -41.0f), FaceRight);
        FacilityProps.FilingCabinet(root, new Vector3(LeftX(0.62f), 0f, -39.0f), FaceRight);
        FacilityProps.FilingCabinet(root, new Vector3(RightX(0.62f), 0f, -36.25f), FaceLeft);
        FacilityProps.FilingCabinet(root, new Vector3(RightX(0.62f), 0f, -35.75f), FaceLeft);
        FacilityProps.Door(root, new Vector3(W - WallTrim, 0f, -44.3f), FaceLeft, new Color(0.45f, 0.4f, 0.3f));
        FacilityProps.TrashBin(root, new Vector3(-4.6f, 0f, -44.5f));
    }

    private void BuildStorage()
    {
        float[] racks = { -29.5f, -26.5f, -21.5f, -18.5f };
        for (int i = 0; i < racks.Length; i++)
        {
            FacilityProps.ShelvingRack(root, new Vector3(LeftX(0.6f), 0f, racks[i]), FaceRight, 10 + i);
            FacilityProps.ShelvingRack(root, new Vector3(RightX(0.6f), 0f, racks[i]), FaceLeft, 20 + i);
        }

        // Low pallets stay under the laser curtain's 1.3m crawlspace.
        FacilityProps.LoadedPallet(root, new Vector3(-2.6f, 0f, -23f), 8f, 1);
        FacilityProps.LoadedPallet(root, new Vector3(2.9f, 0f, -28.6f), -5f, 2);
        FacilityProps.CardboardBox(root, new Vector3(-4.95f, 0f, -24.6f), 12f, new Vector3(0.5f, 0.4f, 0.45f));
        FacilityProps.CardboardBox(root, new Vector3(4.9f, 0f, -20.1f), -7f, new Vector3(0.6f, 0.45f, 0.5f));
        FacilityProps.CardboardBox(root, new Vector3(4.95f, 0.45f, -20.1f), 4f, new Vector3(0.45f, 0.35f, 0.4f));
    }

    private void BuildAtrium()
    {
        FacilityProps.Door(root, new Vector3(-W + WallTrim, 0f, -12f), FaceRight, new Color(0.5f, 0.12f, 0.1f));
        FacilityProps.FireExtinguisher(root, new Vector3(-W + WallTrim, 0f, -9.6f), FaceRight);
        FacilityProps.Bench(root, new Vector3(-5.2f, 0f, -4f), FaceRight);
        FacilityProps.VendingMachine(root, new Vector3(LeftX(0.8f), 0f, 2.6f), FaceRight);
        FacilityProps.TrashBin(root, new Vector3(-5.6f, 0f, 4.6f));

        FacilityProps.WaterCooler(root, new Vector3(RightX(0.32f), 0f, -13.2f), FaceLeft);
        FacilityProps.TrashBin(root, new Vector3(5.6f, 0f, -12.4f));
        FacilityProps.NoticeBoard(root, new Vector3(W - WallTrim, 1.6f, -4f), FaceLeft, 2);
        foreach (var z in new[] { 3.0f, 3.5f, 4.0f })
            FacilityProps.FilingCabinet(root, new Vector3(RightX(0.62f), 0f, z), FaceLeft);
        FacilityProps.Door(root, new Vector3(W - WallTrim, 0f, 9f), FaceLeft, new Color(0.3f, 0.32f, 0.34f));
        FacilityProps.FireExtinguisher(root, new Vector3(W - WallTrim, 0f, 12.7f), FaceLeft);
    }

    private void BuildServerRoom()
    {
        float[] offsets = { -2.48f, -1.86f, -1.24f, -0.62f, 0.62f, 1.24f, 1.86f, 2.48f };
        int seed = 100;
        foreach (var bay in new[] { 20f, 28f, 36f })
        {
            foreach (var o in offsets)
            {
                FacilityProps.ServerRack(root, new Vector3(LeftX(1f), 0f, bay + o), FaceRight, seed++);
                FacilityProps.ServerRack(root, new Vector3(RightX(1f), 0f, bay + o), FaceLeft, seed++);
            }

            // Cold aisle glow from the rack LEDs.
            foreach (var side in new[] { -1f, 1f })
            {
                var glow = StealthVisuals.CreateLight("RackGlow", root, new Vector3(side * 4.6f, 1.2f, bay), LightType.Point, 3.2f, 0.5f);
                glow.color = new Color(0.3f, 0.6f, 1f);
            }
        }
    }

    private void BuildLoadingBay()
    {
        FacilityProps.Crate(root, new Vector3(-5.3f, 0f, 42.5f), 3f, new Vector3(1.2f, 1.0f, 1.0f));
        FacilityProps.Crate(root, new Vector3(-5.3f, 1.0f, 42.5f), -6f, new Vector3(1.0f, 0.8f, 0.9f));
        FacilityProps.Crate(root, new Vector3(-5.2f, 0f, 45f), 0f, new Vector3(1.1f, 0.9f, 1.1f));
        FacilityProps.LoadedPallet(root, new Vector3(-5.0f, 0f, 49f), 90f, 3);
        FacilityProps.LoadedPallet(root, new Vector3(5.0f, 0f, 43f), 90f, 4);
        FacilityProps.LoadedPallet(root, new Vector3(5.0f, 0f, 46.5f), 85f, 5);

        // Mid-floor cover.
        FacilityProps.Crate(root, new Vector3(-2.8f, 0f, 44f), 10f, new Vector3(1.2f, 1.1f, 1.0f));
        FacilityProps.LoadedPallet(root, new Vector3(2.6f, 0f, 50f), -12f, 6);

        // Exit: double doors with an illuminated sign.
        FacilityProps.Door(root, new Vector3(-0.6f, 0f, L), 0f, new Color(0.15f, 0.35f, 0.2f));
        FacilityProps.Door(root, new Vector3(0.6f, 0f, L), 0f, new Color(0.15f, 0.35f, 0.2f));
        // Not under the batched root: TextMeshPro rebuilds its mesh at runtime.
        FacilityProps.ExitSign(transform, new Vector3(0f, 2.8f, L), 0f);
        FacilityProps.FireExtinguisher(root, new Vector3(-W + WallTrim, 0f, 55f), FaceRight);
        FacilityProps.TrashBin(root, new Vector3(5.6f, 0f, 55f));
    }
}
