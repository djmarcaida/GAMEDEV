using UnityEngine;

/// <summary>
/// Builds furniture and fixtures from textured parts at real-world dimensions (metres).
/// Convention: a prop's front faces local -Z and its back faces +Z, so against the left wall use yaw -90,
/// against the right wall yaw 90. Solid props get a BoxCollider so they block movement, hazard sight and the flashlight.
/// </summary>
public static class FacilityProps
{
    private static readonly Color DeskFrame = new Color(0.2f, 0.21f, 0.23f);
    private static readonly Color CabinetBeige = new Color(0.62f, 0.6f, 0.55f);
    private static readonly Color LockerBlue = new Color(0.27f, 0.35f, 0.43f);
    private static readonly Color RackBlue = new Color(0.1f, 0.22f, 0.48f);
    private static readonly Color RackOrange = new Color(0.85f, 0.36f, 0.07f);
    private static readonly Color ServerBlack = new Color(0.07f, 0.07f, 0.08f);
    private static readonly Color VendingRed = new Color(0.5f, 0.06f, 0.05f);
    private static readonly Color DarkPlastic = new Color(0.1f, 0.1f, 0.11f);
    private static readonly Color WhitePlastic = new Color(0.85f, 0.85f, 0.83f);
    private static readonly Color FireRed = new Color(0.7f, 0.05f, 0.04f);

    // ---------------------------------------------------------------- Helpers

    private static Transform Root(Transform parent, string name, Vector3 pos, float yaw)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        t.localPosition = pos;
        t.localRotation = Quaternion.Euler(0f, yaw, 0f);
        return t;
    }

    private static GameObject Box(Transform parent, string name, Vector3 pos, Vector3 size, Material mat, bool collider = false, float yaw = 0f)
    {
        return StealthVisuals.CreateBox(name, parent, pos, size, mat, collider, yaw);
    }

    /// <summary>Upright cylinder; <paramref name="height"/> is the full height.</summary>
    private static Transform Cylinder(Transform parent, string name, Vector3 pos, float diameter, float height, Material mat)
    {
        return StealthVisuals.CreatePrimitive(PrimitiveType.Cylinder, name, parent, pos, new Vector3(diameter, height * 0.5f, diameter), mat).transform;
    }

    private static void Collider(Transform root, Vector3 center, Vector3 size)
    {
        var col = root.gameObject.AddComponent<BoxCollider>();
        col.center = center;
        col.size = size;
    }

    // ---------------------------------------------------------------- Office

    public static Transform Desk(Transform parent, Vector3 pos, float yaw, bool withMonitor = true)
    {
        var root = Root(parent, "Desk", pos, yaw);
        var frame = FacilityMaterials.PaintedMetal(DeskFrame);

        Box(root, "Top", new Vector3(0f, 0.74f, 0f), new Vector3(1.6f, 0.035f, 0.8f), FacilityMaterials.Wood);
        foreach (var x in new[] { -0.75f, 0.75f })
            foreach (var z in new[] { -0.35f, 0.35f })
                Box(root, "Leg", new Vector3(x, 0.36f, z), new Vector3(0.05f, 0.72f, 0.05f), frame);
        Box(root, "ModestyPanel", new Vector3(0f, 0.5f, 0.36f), new Vector3(1.45f, 0.4f, 0.02f), frame);

        // Drawer pedestal
        var drawerMat = FacilityMaterials.PaintedMetal(CabinetBeige);
        Box(root, "Pedestal", new Vector3(0.52f, 0.33f, 0f), new Vector3(0.42f, 0.62f, 0.7f), drawerMat);
        for (int i = 0; i < 3; i++)
        {
            float y = 0.12f + i * 0.2f;
            Box(root, "DrawerFront", new Vector3(0.52f, y, -0.352f), new Vector3(0.39f, 0.18f, 0.006f), drawerMat);
            Box(root, "Handle", new Vector3(0.52f, y + 0.05f, -0.362f), new Vector3(0.12f, 0.015f, 0.015f), FacilityMaterials.BrushedMetal);
        }

        // Desk clutter
        Box(root, "Keyboard", new Vector3(-0.1f, 0.765f, -0.15f), new Vector3(0.44f, 0.02f, 0.14f), FacilityMaterials.Plastic(DarkPlastic));
        Box(root, "Papers", new Vector3(-0.58f, 0.762f, -0.1f), new Vector3(0.21f, 0.01f, 0.3f), FacilityMaterials.Plastic(WhitePlastic), false, 12f);
        Cylinder(root, "Mug", new Vector3(0.45f, 0.8f, -0.2f), 0.08f, 0.1f, FacilityMaterials.Plastic(new Color(0.7f, 0.2f, 0.15f)));

        if (withMonitor)
            Monitor(root, new Vector3(-0.1f, 0.76f, 0.18f));

        Collider(root, new Vector3(0f, 0.38f, 0f), new Vector3(1.6f, 0.76f, 0.8f));
        return root;
    }

    /// <summary>Flat screen showing a terminal, with a faint glow on the desk.</summary>
    public static void Monitor(Transform parent, Vector3 basePos)
    {
        var plastic = FacilityMaterials.Plastic(DarkPlastic);
        Box(parent, "MonitorBase", basePos + new Vector3(0f, 0.008f, 0f), new Vector3(0.22f, 0.015f, 0.18f), plastic);
        Box(parent, "MonitorNeck", basePos + new Vector3(0f, 0.17f, 0.03f), new Vector3(0.04f, 0.32f, 0.03f), plastic);
        Box(parent, "MonitorBezel", basePos + new Vector3(0f, 0.36f, 0f), new Vector3(0.56f, 0.34f, 0.03f), plastic);
        Box(parent, "MonitorScreen", basePos + new Vector3(0f, 0.365f, -0.016f), new Vector3(0.53f, 0.3f, 0.002f), FacilityMaterials.Screen);

        var light = StealthVisuals.CreateLight("ScreenGlow", parent, basePos + new Vector3(0f, 0.35f, -0.25f), LightType.Point, 1.8f, 0.35f);
        light.color = new Color(0.45f, 0.75f, 1f);
    }

    public static Transform OfficeChair(Transform parent, Vector3 pos, float yaw)
    {
        var root = Root(parent, "OfficeChair", pos, yaw);
        var fabric = FacilityMaterials.Plastic(new Color(0.13f, 0.14f, 0.16f));
        var plastic = FacilityMaterials.Plastic(DarkPlastic);

        for (int i = 0; i < 5; i++)
            Box(root, "BaseSpoke", Vector3.zero, new Vector3(0.05f, 0.04f, 0.32f), plastic, false, i * 72f).transform.localPosition =
                Quaternion.Euler(0f, i * 72f, 0f) * new Vector3(0f, 0.07f, 0.16f);
        Cylinder(root, "GasLift", new Vector3(0f, 0.27f, 0f), 0.05f, 0.36f, FacilityMaterials.BrushedMetal);
        Box(root, "Seat", new Vector3(0f, 0.48f, 0f), new Vector3(0.48f, 0.08f, 0.46f), fabric);
        var back = Box(root, "Backrest", new Vector3(0f, 0.82f, 0.24f), new Vector3(0.46f, 0.52f, 0.06f), fabric);
        back.transform.localRotation = Quaternion.Euler(-8f, 0f, 0f);
        foreach (var x in new[] { -0.26f, 0.26f })
            Box(root, "Armrest", new Vector3(x, 0.66f, 0.02f), new Vector3(0.05f, 0.04f, 0.3f), plastic);

        Collider(root, new Vector3(0f, 0.55f, 0f), new Vector3(0.55f, 1.1f, 0.55f));
        return root;
    }

    public static Transform FilingCabinet(Transform parent, Vector3 pos, float yaw)
    {
        var root = Root(parent, "FilingCabinet", pos, yaw);
        var paint = FacilityMaterials.PaintedMetal(CabinetBeige);
        Box(root, "Body", new Vector3(0f, 0.66f, 0f), new Vector3(0.47f, 1.32f, 0.62f), paint);
        for (int i = 0; i < 4; i++)
        {
            float y = 0.18f + i * 0.32f;
            Box(root, "Drawer", new Vector3(0f, y, -0.313f), new Vector3(0.43f, 0.29f, 0.008f), paint);
            Box(root, "Handle", new Vector3(0f, y + 0.07f, -0.325f), new Vector3(0.14f, 0.02f, 0.02f), FacilityMaterials.BrushedMetal);
            Box(root, "LabelSlot", new Vector3(0f, y + 0.11f, -0.319f), new Vector3(0.08f, 0.03f, 0.004f), FacilityMaterials.Plastic(WhitePlastic));
        }
        Collider(root, new Vector3(0f, 0.66f, 0f), new Vector3(0.47f, 1.32f, 0.62f));
        return root;
    }

    public static Transform LockerRow(Transform parent, Vector3 pos, float yaw, int count)
    {
        var root = Root(parent, "LockerRow", pos, yaw);
        var paint = FacilityMaterials.PaintedMetal(LockerBlue);
        var slot = FacilityMaterials.Plastic(new Color(0.03f, 0.03f, 0.03f));
        float width = 0.4f, total = width * count;

        for (int i = 0; i < count; i++)
        {
            float x = -total * 0.5f + width * (i + 0.5f);
            Box(root, "LockerBody", new Vector3(x, 0.94f, 0f), new Vector3(width - 0.006f, 1.8f, 0.5f), paint);
            Box(root, "LockerDoor", new Vector3(x, 0.95f, -0.253f), new Vector3(width - 0.03f, 1.7f, 0.008f), paint);
            for (int v = 0; v < 4; v++)
                Box(root, "Vent", new Vector3(x, 1.62f + v * 0.04f, -0.258f), new Vector3(0.22f, 0.012f, 0.004f), slot);
            Box(root, "Handle", new Vector3(x + 0.13f, 0.95f, -0.265f), new Vector3(0.02f, 0.12f, 0.02f), FacilityMaterials.BrushedMetal);
        }
        Box(root, "Plinth", new Vector3(0f, 0.02f, 0f), new Vector3(total, 0.04f, 0.48f), FacilityMaterials.Plastic(DarkPlastic));
        Collider(root, new Vector3(0f, 0.92f, 0f), new Vector3(total, 1.84f, 0.5f));
        return root;
    }

    public static Transform ReceptionDesk(Transform parent, Vector3 pos, float yaw)
    {
        var root = Root(parent, "ReceptionDesk", pos, yaw);
        var panel = FacilityMaterials.PaintedMetal(new Color(0.25f, 0.27f, 0.3f));
        Box(root, "FrontPanel", new Vector3(0f, 0.55f, -0.3f), new Vector3(2.6f, 1.1f, 0.06f), FacilityMaterials.Wood);
        Box(root, "Counter", new Vector3(0f, 1.11f, -0.2f), new Vector3(2.7f, 0.04f, 0.42f), FacilityMaterials.BrushedMetal);
        Box(root, "WorkTop", new Vector3(0f, 0.74f, 0.1f), new Vector3(2.5f, 0.035f, 0.7f), FacilityMaterials.Wood);
        foreach (var x in new[] { -1.27f, 1.27f })
            Box(root, "SidePanel", new Vector3(x, 0.55f, 0f), new Vector3(0.05f, 1.1f, 0.7f), panel);
        Monitor(root, new Vector3(-0.5f, 0.76f, 0.25f));
        Monitor(root, new Vector3(0.6f, 0.76f, 0.25f));
        Collider(root, new Vector3(0f, 0.57f, -0.05f), new Vector3(2.7f, 1.14f, 0.8f));
        return root;
    }

    public static Transform Bench(Transform parent, Vector3 pos, float yaw)
    {
        var root = Root(parent, "Bench", pos, yaw);
        for (int i = 0; i < 3; i++)
            Box(root, "Slat", new Vector3(0f, 0.45f, -0.14f + i * 0.14f), new Vector3(1.6f, 0.04f, 0.12f), FacilityMaterials.Wood);
        var frame = FacilityMaterials.PaintedMetal(DeskFrame);
        foreach (var x in new[] { -0.7f, 0.7f })
        {
            Box(root, "LegFront", new Vector3(x, 0.22f, -0.15f), new Vector3(0.05f, 0.44f, 0.05f), frame);
            Box(root, "LegBack", new Vector3(x, 0.22f, 0.15f), new Vector3(0.05f, 0.44f, 0.05f), frame);
            Box(root, "Rail", new Vector3(x, 0.41f, 0f), new Vector3(0.05f, 0.03f, 0.4f), frame);
        }
        Collider(root, new Vector3(0f, 0.24f, 0f), new Vector3(1.6f, 0.48f, 0.42f));
        return root;
    }

    // ---------------------------------------------------------------- Storage

    public static Transform ShelvingRack(Transform parent, Vector3 pos, float yaw, int seed)
    {
        var root = Root(parent, "ShelvingRack", pos, yaw);
        const float length = 2.4f, depth = 0.6f, height = 2.2f;
        var upright = FacilityMaterials.PaintedMetal(RackBlue);
        var beam = FacilityMaterials.PaintedMetal(RackOrange);

        foreach (var x in new[] { -length * 0.5f + 0.03f, length * 0.5f - 0.03f })
            foreach (var z in new[] { -depth * 0.5f + 0.03f, depth * 0.5f - 0.03f })
                Box(root, "Upright", new Vector3(x, height * 0.5f, z), new Vector3(0.06f, height, 0.06f), upright);

        float[] levels = { 0.12f, 0.75f, 1.35f, 1.95f };
        var rng = new System.Random(seed);
        foreach (var y in levels)
        {
            Box(root, "BeamFront", new Vector3(0f, y, -depth * 0.5f + 0.03f), new Vector3(length, 0.08f, 0.04f), beam);
            Box(root, "BeamBack", new Vector3(0f, y, depth * 0.5f - 0.03f), new Vector3(length, 0.08f, 0.04f), beam);
            Box(root, "Deck", new Vector3(0f, y + 0.045f, 0f), new Vector3(length - 0.08f, 0.015f, depth - 0.04f), FacilityMaterials.BrushedMetal);

            // Boxes and crates sitting on the deck.
            float x = -length * 0.5f + 0.1f;
            while (x < length * 0.5f - 0.35f)
            {
                float w = 0.3f + (float)rng.NextDouble() * 0.35f;
                if (x + w > length * 0.5f - 0.08f) break;
                if (rng.NextDouble() > 0.2)
                {
                    float h = 0.2f + (float)rng.NextDouble() * Mathf.Min(0.38f, y < 1.9f ? 0.38f : 0.25f);
                    float d = 0.3f + (float)rng.NextDouble() * 0.2f;
                    var mat = rng.NextDouble() > 0.3 ? FacilityMaterials.Cardboard : FacilityMaterials.CrateWood;
                    Box(root, "StoredBox", new Vector3(x + w * 0.5f, y + 0.053f + h * 0.5f, 0.05f), new Vector3(w, h, d), mat, false, (float)(rng.NextDouble() - 0.5) * 10f);
                }
                x += w + 0.04f;
            }
        }

        Collider(root, new Vector3(0f, height * 0.5f, 0f), new Vector3(length, height, depth));
        return root;
    }

    /// <summary>Wooden shipping crate with a reinforcing frame.</summary>
    public static Transform Crate(Transform parent, Vector3 pos, float yaw, Vector3 size)
    {
        var root = Root(parent, "Crate", pos, yaw);
        var frame = FacilityMaterials.Wood;
        Box(root, "Body", new Vector3(0f, size.y * 0.5f, 0f), size - Vector3.one * 0.02f, FacilityMaterials.CrateWood);
        const float t = 0.06f;
        foreach (var sx in new[] { -1f, 1f })
            foreach (var sz in new[] { -1f, 1f })
                Box(root, "Corner", new Vector3(sx * (size.x - t) * 0.5f, size.y * 0.5f, sz * (size.z - t) * 0.5f), new Vector3(t, size.y, t), frame);
        foreach (var y in new[] { t * 0.5f, size.y - t * 0.5f })
        {
            Box(root, "RailX", new Vector3(0f, y, -(size.z - t) * 0.5f), new Vector3(size.x, t, t), frame);
            Box(root, "RailX", new Vector3(0f, y, (size.z - t) * 0.5f), new Vector3(size.x, t, t), frame);
            Box(root, "RailZ", new Vector3(-(size.x - t) * 0.5f, y, 0f), new Vector3(t, t, size.z), frame);
            Box(root, "RailZ", new Vector3((size.x - t) * 0.5f, y, 0f), new Vector3(t, t, size.z), frame);
        }
        Collider(root, new Vector3(0f, size.y * 0.5f, 0f), size);
        return root;
    }

    public static Transform CardboardBox(Transform parent, Vector3 pos, float yaw, Vector3 size)
    {
        var root = Root(parent, "CardboardBox", pos, yaw);
        Box(root, "Body", new Vector3(0f, size.y * 0.5f, 0f), size, FacilityMaterials.Cardboard);
        Box(root, "Tape", new Vector3(0f, size.y + 0.001f, 0f), new Vector3(0.05f, 0.002f, size.z + 0.002f), FacilityMaterials.Plastic(new Color(0.75f, 0.6f, 0.4f)));
        Collider(root, new Vector3(0f, size.y * 0.5f, 0f), size);
        return root;
    }

    public static Transform Pallet(Transform parent, Vector3 pos, float yaw)
    {
        var root = Root(parent, "Pallet", pos, yaw);
        foreach (var x in new[] { -0.5f, 0f, 0.5f })
            Box(root, "Runner", new Vector3(x, 0.05f, 0f), new Vector3(0.1f, 0.1f, 1f), FacilityMaterials.CrateWood);
        for (int i = 0; i < 5; i++)
            Box(root, "Board", new Vector3(0f, 0.12f, -0.42f + i * 0.21f), new Vector3(1.2f, 0.03f, 0.14f), FacilityMaterials.CrateWood);
        Collider(root, new Vector3(0f, 0.07f, 0f), new Vector3(1.2f, 0.14f, 1f));
        return root;
    }

    /// <summary>Pallet loaded with a crate and boxes; useful as cover.</summary>
    public static Transform LoadedPallet(Transform parent, Vector3 pos, float yaw, int seed)
    {
        var root = Pallet(parent, pos, yaw);
        var rng = new System.Random(seed);
        if (rng.NextDouble() > 0.5)
        {
            Crate(root, new Vector3(0f, 0.135f, 0f), 0f, new Vector3(1.1f, 0.8f, 0.9f));
        }
        else
        {
            CardboardBox(root, new Vector3(-0.28f, 0.135f, -0.22f), 4f, new Vector3(0.55f, 0.45f, 0.45f));
            CardboardBox(root, new Vector3(0.3f, 0.135f, -0.2f), -3f, new Vector3(0.5f, 0.45f, 0.5f));
            CardboardBox(root, new Vector3(0f, 0.135f, 0.25f), 2f, new Vector3(1.0f, 0.5f, 0.45f));
            CardboardBox(root, new Vector3(-0.1f, 0.635f, 0.05f), 8f, new Vector3(0.6f, 0.4f, 0.5f));
        }
        return root;
    }

    // ---------------------------------------------------------------- Technical

    public static Transform ServerRack(Transform parent, Vector3 pos, float yaw, int seed)
    {
        var root = Root(parent, "ServerRack", pos, yaw);
        var body = FacilityMaterials.PaintedMetal(ServerBlack);
        Box(root, "Cabinet", new Vector3(0f, 1.0f, 0f), new Vector3(0.6f, 2.0f, 1.0f), body);

        var unitMat = FacilityMaterials.PaintedMetal(new Color(0.16f, 0.16f, 0.18f));
        Material[] leds =
        {
            FacilityMaterials.Emissive(new Color(0.1f, 1f, 0.3f), 4f),
            FacilityMaterials.Emissive(new Color(1f, 0.6f, 0.05f), 4f),
            FacilityMaterials.Emissive(new Color(0.2f, 0.5f, 1f), 4f),
        };
        var rng = new System.Random(seed);
        for (int i = 0; i < 14; i++)
        {
            float y = 0.2f + i * 0.125f;
            Box(root, "ServerUnit", new Vector3(0f, y, -0.505f), new Vector3(0.5f, 0.1f, 0.01f), unitMat);
            int ledCount = rng.Next(1, 4);
            for (int l = 0; l < ledCount; l++)
                Box(root, "LED", new Vector3(-0.2f + l * 0.035f, y + 0.02f, -0.512f), new Vector3(0.012f, 0.012f, 0.004f), leds[rng.Next(leds.Length)]);
        }
        Box(root, "DoorGlass", new Vector3(0f, 1.0f, -0.53f), new Vector3(0.56f, 1.9f, 0.01f), FacilityMaterials.Glass(new Color(0.05f, 0.08f, 0.1f, 0.35f)));
        Collider(root, new Vector3(0f, 1.0f, -0.02f), new Vector3(0.6f, 2.0f, 1.04f));
        return root;
    }

    public static Transform VendingMachine(Transform parent, Vector3 pos, float yaw)
    {
        var root = Root(parent, "VendingMachine", pos, yaw);
        Box(root, "Body", new Vector3(0f, 0.93f, 0f), new Vector3(0.9f, 1.86f, 0.8f), FacilityMaterials.PaintedMetal(VendingRed));
        Box(root, "Display", new Vector3(-0.1f, 1.1f, -0.402f), new Vector3(0.6f, 1.2f, 0.005f), FacilityMaterials.Emissive(new Color(1f, 0.9f, 0.75f), 1.6f));
        for (int r = 0; r < 5; r++)
            Box(root, "Shelf", new Vector3(-0.1f, 0.6f + r * 0.24f, -0.405f), new Vector3(0.6f, 0.015f, 0.004f), FacilityMaterials.BrushedMetal);
        Box(root, "Glass", new Vector3(-0.1f, 1.1f, -0.41f), new Vector3(0.62f, 1.22f, 0.01f), FacilityMaterials.Glass(new Color(0.6f, 0.7f, 0.8f, 0.15f)));
        Box(root, "Keypad", new Vector3(0.33f, 1.15f, -0.41f), new Vector3(0.15f, 0.25f, 0.02f), FacilityMaterials.BrushedMetal);
        Box(root, "Tray", new Vector3(-0.1f, 0.3f, -0.41f), new Vector3(0.55f, 0.18f, 0.02f), FacilityMaterials.Plastic(DarkPlastic));

        var glow = StealthVisuals.CreateLight("VendingGlow", root, new Vector3(-0.1f, 1.1f, -0.9f), LightType.Point, 3.5f, 0.9f);
        glow.color = new Color(1f, 0.85f, 0.65f);
        Collider(root, new Vector3(0f, 0.93f, 0f), new Vector3(0.9f, 1.86f, 0.8f));
        return root;
    }

    public static Transform WaterCooler(Transform parent, Vector3 pos, float yaw)
    {
        var root = Root(parent, "WaterCooler", pos, yaw);
        Box(root, "Cabinet", new Vector3(0f, 0.5f, 0f), new Vector3(0.32f, 1.0f, 0.32f), FacilityMaterials.Plastic(WhitePlastic));
        Box(root, "Tap", new Vector3(0f, 0.85f, -0.17f), new Vector3(0.05f, 0.05f, 0.04f), FacilityMaterials.Plastic(new Color(0.2f, 0.4f, 0.9f)));
        StealthVisuals.CreatePrimitive(PrimitiveType.Cylinder, "Bottle", root, new Vector3(0f, 1.23f, 0f), new Vector3(0.27f, 0.23f, 0.27f),
            FacilityMaterials.Glass(new Color(0.4f, 0.65f, 0.9f, 0.45f)), false);
        Collider(root, new Vector3(0f, 0.68f, 0f), new Vector3(0.32f, 1.36f, 0.32f));
        return root;
    }

    public static Transform TrashBin(Transform parent, Vector3 pos)
    {
        var root = Root(parent, "TrashBin", pos, 0f);
        Cylinder(root, "Bin", new Vector3(0f, 0.3f, 0f), 0.36f, 0.6f, FacilityMaterials.PaintedMetal(new Color(0.2f, 0.25f, 0.22f)));
        Cylinder(root, "Rim", new Vector3(0f, 0.6f, 0f), 0.38f, 0.03f, FacilityMaterials.BrushedMetal);
        Collider(root, new Vector3(0f, 0.31f, 0f), new Vector3(0.38f, 0.62f, 0.38f));
        return root;
    }

    // ---------------------------------------------------------------- Wall fixtures (no colliders; they sit flush on the wall)

    /// <summary>Door set into a wall. <paramref name="pos"/> is on the wall surface at floor level.</summary>
    public static Transform Door(Transform parent, Vector3 pos, float yaw, Color leafColor)
    {
        var root = Root(parent, "Door", pos, yaw);
        var frame = FacilityMaterials.BrushedMetal;
        Box(root, "JambL", new Vector3(-0.58f, 1.1f, -0.04f), new Vector3(0.08f, 2.2f, 0.1f), frame);
        Box(root, "JambR", new Vector3(0.58f, 1.1f, -0.04f), new Vector3(0.08f, 2.2f, 0.1f), frame);
        Box(root, "Header", new Vector3(0f, 2.24f, -0.04f), new Vector3(1.24f, 0.08f, 0.1f), frame);
        Box(root, "Leaf", new Vector3(0f, 1.06f, -0.025f), new Vector3(1.08f, 2.1f, 0.05f), FacilityMaterials.PaintedMetal(leafColor));
        Box(root, "Window", new Vector3(0f, 1.55f, -0.052f), new Vector3(0.24f, 0.55f, 0.006f), FacilityMaterials.Glass(new Color(0.08f, 0.1f, 0.12f, 0.8f)));
        Box(root, "KickPlate", new Vector3(0f, 0.15f, -0.052f), new Vector3(1.0f, 0.25f, 0.004f), frame);
        Box(root, "LeverBase", new Vector3(0.42f, 1.0f, -0.055f), new Vector3(0.05f, 0.12f, 0.01f), frame);
        Box(root, "Lever", new Vector3(0.36f, 1.03f, -0.08f), new Vector3(0.14f, 0.025f, 0.025f), frame);
        Box(root, "SignPlate", new Vector3(0f, 2.45f, -0.01f), new Vector3(0.5f, 0.14f, 0.01f), FacilityMaterials.Plastic(new Color(0.12f, 0.3f, 0.55f)));
        return root;
    }

    public static Transform FireExtinguisher(Transform parent, Vector3 pos, float yaw)
    {
        var root = Root(parent, "FireExtinguisher", pos, yaw);
        Cylinder(root, "Cylinder", new Vector3(0f, 0.95f, -0.1f), 0.15f, 0.5f, FacilityMaterials.PaintedMetal(FireRed));
        Box(root, "Valve", new Vector3(0f, 1.24f, -0.1f), new Vector3(0.05f, 0.08f, 0.05f), FacilityMaterials.Plastic(DarkPlastic));
        Box(root, "Bracket", new Vector3(0f, 1.0f, -0.015f), new Vector3(0.12f, 0.2f, 0.03f), FacilityMaterials.PaintedMetal(DeskFrame));
        Box(root, "Sign", new Vector3(0f, 1.55f, -0.005f), new Vector3(0.2f, 0.28f, 0.01f), FacilityMaterials.PaintedMetal(FireRed));
        return root;
    }

    public static Transform NoticeBoard(Transform parent, Vector3 pos, float yaw, int seed)
    {
        var root = Root(parent, "NoticeBoard", pos, yaw);
        Box(root, "Frame", new Vector3(0f, 0f, -0.015f), new Vector3(1.24f, 0.84f, 0.03f), FacilityMaterials.BrushedMetal);
        Box(root, "Cork", new Vector3(0f, 0f, -0.032f), new Vector3(1.16f, 0.76f, 0.006f), FacilityMaterials.Plastic(new Color(0.55f, 0.36f, 0.2f)));
        var rng = new System.Random(seed);
        Color[] papers = { WhitePlastic, new Color(0.9f, 0.85f, 0.45f), new Color(0.75f, 0.85f, 0.95f) };
        for (int i = 0; i < 6; i++)
        {
            var size = new Vector3(0.18f + (float)rng.NextDouble() * 0.1f, 0.24f + (float)rng.NextDouble() * 0.08f, 0.002f);
            var p = new Vector3(-0.45f + (i % 3) * 0.42f + (float)(rng.NextDouble() - 0.5) * 0.08f, i < 3 ? 0.16f : -0.18f, -0.037f);
            var paper = Box(root, "Paper", p, size, FacilityMaterials.Plastic(papers[rng.Next(papers.Length)]));
            paper.transform.localRotation = Quaternion.Euler(0f, 0f, (float)(rng.NextDouble() - 0.5) * 8f);
        }
        return root;
    }

    public static Transform WallVent(Transform parent, Vector3 pos, float yaw)
    {
        var root = Root(parent, "WallVent", pos, yaw);
        var metal = FacilityMaterials.PaintedMetal(new Color(0.55f, 0.57f, 0.58f));
        Box(root, "Frame", new Vector3(0f, 0f, -0.01f), new Vector3(0.7f, 0.4f, 0.02f), metal);
        Box(root, "Cavity", new Vector3(0f, 0f, -0.021f), new Vector3(0.62f, 0.32f, 0.002f), FacilityMaterials.Plastic(new Color(0.02f, 0.02f, 0.02f)));
        for (int i = 0; i < 6; i++)
        {
            var slat = Box(root, "Slat", new Vector3(0f, -0.13f + i * 0.052f, -0.03f), new Vector3(0.62f, 0.008f, 0.04f), metal);
            slat.transform.localRotation = Quaternion.Euler(35f, 0f, 0f);
        }
        return root;
    }

    // ---------------------------------------------------------------- Lighting fixtures

    public enum FixtureState { Off, On, Faulty }

    /// <summary>Suspended fluorescent troffer. Lit fixtures cast a pool of light straight down.</summary>
    public static Light CeilingFixture(Transform parent, Vector3 pos, FixtureState state, Color lightColor)
    {
        var root = Root(parent, "CeilingFixture", pos, 0f);
        var housing = FacilityMaterials.PaintedMetal(new Color(0.8f, 0.8f, 0.78f));
        Box(root, "Housing", Vector3.zero, new Vector3(0.36f, 0.1f, 1.3f), housing);
        foreach (var z in new[] { -0.5f, 0.5f })
            Cylinder(root, "Hanger", new Vector3(0f, 0.3f, z), 0.01f, 0.5f, FacilityMaterials.BrushedMetal);

        Material diffuser = state == FixtureState.Off
            ? FacilityMaterials.Plastic(new Color(0.45f, 0.45f, 0.44f))
            : FacilityMaterials.CreateEmissive(lightColor, 3f);
        Box(root, "Diffuser", new Vector3(0f, -0.052f, 0f), new Vector3(0.32f, 0.006f, 1.25f), diffuser);

        if (state == FixtureState.Off) return null;

        var light = StealthVisuals.CreateLight("FixtureLight", root, new Vector3(0f, -0.1f, 0f), LightType.Spot, 9f, 3.4f);
        light.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        light.spotAngle = 125f;
        light.innerSpotAngle = 50f;
        light.color = lightColor;

        if (state == FixtureState.Faulty)
            root.gameObject.AddComponent<FlickeringLight>().Setup(light, diffuser, lightColor * 3f, FlickeringLight.Mode.Faulty);
        return light;
    }

    /// <summary>Small wall-mounted emergency beacon with a slow red pulse.</summary>
    public static void EmergencyLight(Transform parent, Vector3 pos, float yaw)
    {
        var root = Root(parent, "EmergencyLight", pos, yaw);
        Box(root, "Base", new Vector3(0f, 0f, -0.03f), new Vector3(0.22f, 0.12f, 0.06f), FacilityMaterials.Plastic(DarkPlastic));
        var lens = FacilityMaterials.CreateEmissive(new Color(1f, 0.12f, 0.05f), 4f);
        Box(root, "Lens", new Vector3(0f, 0f, -0.07f), new Vector3(0.18f, 0.08f, 0.03f), lens);
        var light = StealthVisuals.CreateLight("EmergencyGlow", root, new Vector3(0f, -0.1f, -0.4f), LightType.Point, 4.5f, 0.9f);
        light.color = new Color(1f, 0.15f, 0.08f);
        root.gameObject.AddComponent<FlickeringLight>().Setup(light, lens, new Color(1f, 0.12f, 0.05f) * 4f, FlickeringLight.Mode.Pulse);
    }

    /// <summary>Illuminated green EXIT sign.</summary>
    public static void ExitSign(Transform parent, Vector3 pos, float yaw)
    {
        var root = Root(parent, "ExitSign", pos, yaw);
        Box(root, "Housing", new Vector3(0f, 0f, -0.04f), new Vector3(0.7f, 0.26f, 0.08f), FacilityMaterials.Plastic(WhitePlastic));
        Box(root, "Face", new Vector3(0f, 0f, -0.081f), new Vector3(0.64f, 0.2f, 0.002f), FacilityMaterials.Emissive(new Color(0.1f, 0.9f, 0.3f), 3f));

        var textObj = new GameObject("ExitText");
        textObj.transform.SetParent(root, false);
        textObj.transform.localPosition = new Vector3(0f, 0f, -0.085f);
        var text = textObj.AddComponent<TMPro.TextMeshPro>();
        text.text = "EXIT";
        text.enableAutoSizing = true;
        text.fontSizeMin = 0.2f;
        text.fontSizeMax = 3f;
        text.fontStyle = TMPro.FontStyles.Bold;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.color = new Color(0.95f, 1f, 0.95f);
        text.rectTransform.sizeDelta = new Vector2(0.64f, 0.2f);

        var light = StealthVisuals.CreateLight("ExitGlow", root, new Vector3(0f, -0.2f, -0.5f), LightType.Point, 4f, 0.8f);
        light.color = new Color(0.2f, 1f, 0.4f);
    }
}
