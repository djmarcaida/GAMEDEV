using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor window & menu action that builds a complete Resident Evil Laser Corridor
/// in the active scene with 1 click.
/// </summary>
public class ResidentEvilCorridorBuilder : EditorWindow
{
    private float corridorLength = 75f;
    private float corridorWidth = 8f;
    private float corridorHeight = 6f;

    [MenuItem("Tools/Resident Evil/Open Corridor Builder Window")]
    public static void ShowWindow()
    {
        GetWindow<ResidentEvilCorridorBuilder>("RE Corridor Builder");
    }

    [MenuItem("Tools/Resident Evil/Generate Complete Laser Corridor Scene (1-Click)")]
    public static void GenerateCorridorQuick()
    {
        ResidentEvilCorridorBuilder builder = CreateInstance<ResidentEvilCorridorBuilder>();
        builder.BuildCorridor();
        DestroyImmediate(builder);
    }

    private void OnGUI()
    {
        GUILayout.Label("Resident Evil Laser Corridor Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        corridorLength = EditorGUILayout.FloatField("Corridor Length (m)", corridorLength);
        corridorWidth = EditorGUILayout.FloatField("Corridor Width (m)", corridorWidth);
        corridorHeight = EditorGUILayout.FloatField("Corridor Height (m)", corridorHeight);

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Generate Laser Corridor in Scene", GUILayout.Height(36)))
        {
            BuildCorridor();
        }
    }

    public void BuildCorridor()
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Build Resident Evil Corridor");
        int groupIndex = Undo.GetCurrentGroup();

        // 1. Root Object
        GameObject root = new GameObject("--- RESIDENT EVIL LASER CORRIDOR ---");
        Undo.RegisterCreatedObjectUndo(root, "Create Corridor Root");

        // 2. Materials
        Material floorMat = CreateDarkMaterial("Corridor_FloorMat", new Color(0.12f, 0.13f, 0.15f), 0.7f);
        Material wallMat = CreateDarkMaterial("Corridor_WallMat", new Color(0.2f, 0.22f, 0.25f), 0.3f);
        Material laserMat = CreateLaserMaterial();

        // 3. Corridor Geometry
        float halfL = corridorLength * 0.5f;

        // Floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(root.transform);
        floor.transform.position = new Vector3(0, -0.5f, 0);
        floor.transform.localScale = new Vector3(corridorWidth, 1f, corridorLength);
        floor.GetComponent<Renderer>().sharedMaterial = floorMat;
        int groundLayerIdx = LayerMask.NameToLayer("Ground");
        if (groundLayerIdx != -1) floor.layer = groundLayerIdx;
        Undo.RegisterCreatedObjectUndo(floor, "Create Floor");

        // Ceiling
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(root.transform);
        ceiling.transform.position = new Vector3(0, corridorHeight + 0.5f, 0);
        ceiling.transform.localScale = new Vector3(corridorWidth, 1f, corridorLength);
        ceiling.GetComponent<Renderer>().sharedMaterial = wallMat;
        Undo.RegisterCreatedObjectUndo(ceiling, "Create Ceiling");

        // Left Wall
        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "LeftWall";
        leftWall.transform.SetParent(root.transform);
        leftWall.transform.position = new Vector3(-corridorWidth * 0.5f - 0.5f, corridorHeight * 0.5f, 0);
        leftWall.transform.localScale = new Vector3(1f, corridorHeight, corridorLength);
        leftWall.GetComponent<Renderer>().sharedMaterial = wallMat;
        Undo.RegisterCreatedObjectUndo(leftWall, "Create Left Wall");

        // Right Wall
        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "RightWall";
        rightWall.transform.SetParent(root.transform);
        rightWall.transform.position = new Vector3(corridorWidth * 0.5f + 0.5f, corridorHeight * 0.5f, 0);
        rightWall.transform.localScale = new Vector3(1f, corridorHeight, corridorLength);
        rightWall.GetComponent<Renderer>().sharedMaterial = wallMat;
        Undo.RegisterCreatedObjectUndo(rightWall, "Create Right Wall");

        // Back Wall (Entrance end)
        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "EntranceWall";
        backWall.transform.SetParent(root.transform);
        backWall.transform.position = new Vector3(0, corridorHeight * 0.5f, -halfL - 0.5f);
        backWall.transform.localScale = new Vector3(corridorWidth, corridorHeight, 1f);
        backWall.GetComponent<Renderer>().sharedMaterial = wallMat;
        Undo.RegisterCreatedObjectUndo(backWall, "Create Back Wall");

        // Far End Wall (Exit / Spawner end)
        GameObject exitWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exitWall.name = "FarExitWall";
        exitWall.transform.SetParent(root.transform);
        exitWall.transform.position = new Vector3(0, corridorHeight * 0.5f, halfL + 0.5f);
        exitWall.transform.localScale = new Vector3(corridorWidth, corridorHeight, 1f);
        exitWall.GetComponent<Renderer>().sharedMaterial = wallMat;
        Undo.RegisterCreatedObjectUndo(exitWall, "Create Exit Wall");

        // 4. Lighting
        GameObject lightingParent = new GameObject("Corridor_Lighting");
        lightingParent.transform.SetParent(root.transform);
        int lightCount = 6;
        for (int i = 0; i < lightCount; i++)
        {
            float zPos = Mathf.Lerp(-halfL + 5f, halfL - 5f, (float)i / (lightCount - 1));
            GameObject lightObj = new GameObject($"Ceiling_Light_{i + 1}");
            lightObj.transform.SetParent(lightingParent.transform);
            lightObj.transform.position = new Vector3(0, corridorHeight - 0.3f, zPos);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 18f;
            l.intensity = 2.2f;
            l.color = new Color(0.9f, 0.95f, 1.0f);
        }

        // 5. Start Point & Player
        GameObject startPoint = new GameObject("PlayerStartPoint");
        startPoint.transform.SetParent(root.transform);
        startPoint.transform.position = new Vector3(0, 0.05f, -halfL + 3.5f);
        startPoint.transform.rotation = Quaternion.identity;
        Undo.RegisterCreatedObjectUndo(startPoint, "Create StartPoint");

        // Find or setup player
        Player existingPlayer = Object.FindFirstObjectByType<Player>();
        GameObject playerObj;
        if (existingPlayer != null)
        {
            playerObj = existingPlayer.gameObject;
            playerObj.transform.position = startPoint.transform.position + Vector3.up * 1f;
            playerObj.transform.rotation = Quaternion.identity;
        }
        else
        {
            // Create default player capsule
            playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObj.name = "Player";
            playerObj.tag = "Player";
            playerObj.transform.position = startPoint.transform.position + Vector3.up * 1f;
            Rigidbody rb = playerObj.AddComponent<Rigidbody>();
            rb.freezeRotation = true;
            playerObj.AddComponent<Player>();
            playerObj.AddComponent<PlayerHealth>();
            Undo.RegisterCreatedObjectUndo(playerObj, "Create Player");
        }

        // 6. Laser Spawner at far end
        GameObject spawnerObj = new GameObject("LaserSpawner");
        spawnerObj.transform.SetParent(root.transform);
        spawnerObj.transform.position = new Vector3(0, 0, halfL - 2.5f);
        LaserSpawner spawner = spawnerObj.AddComponent<LaserSpawner>();
        // Wire fields using reflection or defaults
        SetPrivateField(spawner, "corridorWidth", corridorWidth - 0.2f);
        SetPrivateField(spawner, "corridorHeight", corridorHeight);
        SetPrivateField(spawner, "despawnZ", -halfL - 6f);
        SetPrivateField(spawner, "laserMaterial", laserMat);
        SetPrivateField(spawner, "spawnInterval", 1.8f);
        SetPrivateField(spawner, "initialDelay", 1.5f);
        SetPrivateField(spawner, "randomizePatterns", true);
        SetPrivateField(spawner, "avoidImmediateRepeat", true);
        Undo.RegisterCreatedObjectUndo(spawnerObj, "Create Spawner");

        // 7. Corridor Goal at end
        GameObject goalObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        goalObj.name = "CorridorGoal_Trigger";
        goalObj.transform.SetParent(root.transform);
        goalObj.transform.position = new Vector3(0, corridorHeight * 0.5f, halfL - 1.5f);
        goalObj.transform.localScale = new Vector3(corridorWidth, corridorHeight, 2f);
        goalObj.GetComponent<Collider>().isTrigger = true;
        // Make trigger mesh invisible
        goalObj.GetComponent<Renderer>().enabled = false;
        CorridorGoal goal = goalObj.AddComponent<CorridorGoal>();
        Undo.RegisterCreatedObjectUndo(goalObj, "Create Goal");

        // 8. Bonus Floor Panels along the corridor
        GameObject bonusParent = new GameObject("Bonus_Floor_Panels");
        bonusParent.transform.SetParent(root.transform);

        CreateBonusPanel(bonusParent.transform, new Vector3(0, 0.02f, -halfL * 0.45f), BonusFloorPanel.BonusType.SpeedBoost, "SpeedPad (Yellow)");
        CreateBonusPanel(bonusParent.transform, new Vector3(0, 0.02f, 0f), BonusFloorPanel.BonusType.HealthRestore, "HealthPad (Green)");
        CreateBonusPanel(bonusParent.transform, new Vector3(0, 0.02f, halfL * 0.45f), BonusFloorPanel.BonusType.EnergyShield, "ShieldPad (Cyan)");

        // 9. Laser Corridor Game Manager & HUD
        GameObject managerObj = new GameObject("LaserCorridor_GameManager");
        managerObj.transform.SetParent(root.transform);
        LaserCorridorGameManager manager = managerObj.AddComponent<LaserCorridorGameManager>();
        managerObj.AddComponent<CorridorHUD>();

        SetPrivateField(manager, "playerObject", playerObj);
        SetPrivateField(manager, "startPoint", startPoint.transform);
        SetPrivateField(manager, "laserSpawner", spawner);
        SetPrivateField(manager, "corridorGoal", goal);
        Undo.RegisterCreatedObjectUndo(managerObj, "Create Manager");

        Undo.CollapseUndoOperations(groupIndex);
        Selection.activeGameObject = root;

        Debug.Log("<b>[Resident Evil Laser Corridor]</b> Successfully generated corridor and configured game systems! Press Play to test.");
    }

    private GameObject CreateBonusPanel(Transform parent, Vector3 localPos, BonusFloorPanel.BonusType type, string name)
    {
        GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.name = $"BonusPanel_{name}";
        pad.transform.SetParent(parent);
        pad.transform.position = localPos;
        pad.transform.localScale = new Vector3(2.2f, 0.08f, 2.2f);

        Collider col = pad.GetComponent<Collider>();
        col.isTrigger = true;

        BonusFloorPanel panelScript = pad.AddComponent<BonusFloorPanel>();
        SetPrivateField(panelScript, "bonusType", type);

        // Add subtle indicator point light
        GameObject lightObj = new GameObject("Panel_Glow");
        lightObj.transform.SetParent(pad.transform);
        lightObj.transform.localPosition = new Vector3(0, 0.5f, 0);
        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Point;
        l.range = 3f;
        l.intensity = 2f;
        SetPrivateField(panelScript, "panelLight", l);

        return pad;
    }

    private Material CreateDarkMaterial(string name, Color albedo, float metallic)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material mat = new Material(shader) { name = name };
        mat.color = albedo;
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.5f);
        return mat;
    }

    private Material CreateLaserMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
        Material mat = new Material(shader) { name = "RE_Laser_Emissive" };
        Color red = new Color(1.0f, 0.05f, 0.05f, 1f);
        mat.color = red;
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", red * 3.5f);
        }
        return mat;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }
}
