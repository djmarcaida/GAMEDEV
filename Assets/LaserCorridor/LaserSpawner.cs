using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns laser beams/patterns at the end of the corridor and sends them towards the player.
/// Supports both pre-made prefabs and procedural beams (works without setup!).
/// </summary>
public class LaserSpawner : MonoBehaviour
{
    public enum LaserPatternType
    {
        LowHurdle = 0,        // Single low beam - jump over
        HighBeam = 1,         // Overhead beams - crouch under
        LeftPassage = 2,      // Right side blocked - strafe left to pass
        RightPassage = 3,     // Left side blocked - strafe right to pass
        CenterDoorway = 4,    // Left & right walls blocked - run down center gate
        SideFlanks = 5,       // Center pillar blocked - pass via left or right wall lane
        GridCrouchHatch = 6,  // Full RE grid with floor crawlway escape hatch
        MidAirWindow = 7,     // Bottom hurdle + top ceiling blocked - jump through mid window
        StaggeredChoice = 8,  // Low hurdle on left, high beam on right
        DoubleWindowGrid = 9  // Grid with dual escape doors (left & right)
    }

    [Header("Spawn Settings")]
    [SerializeField] private bool autoSpawn = true;
    [SerializeField] private float initialDelay = 1.5f;
    [SerializeField] private float spawnInterval = 1.8f; // Laser shoots out frequently
    [SerializeField] private float laserSpeed = 6.5f;
    [SerializeField] private int laserDamage = 35;
    [Header("Pattern Sequencing")]
    [SerializeField] private bool randomizePatterns = true;
    [SerializeField] private bool avoidImmediateRepeat = true;
    [SerializeField] private bool useSpecificPattern = false;
    [SerializeField] private LaserPatternType testPattern = LaserPatternType.LowHurdle;

    public bool RandomizePatterns
    {
        get => randomizePatterns;
        set => randomizePatterns = value;
    }

    public float SpawnInterval
    {
        get => spawnInterval;
        set => spawnInterval = Mathf.Max(0.2f, value);
    }

    public float InitialDelay
    {
        get => initialDelay;
        set => initialDelay = Mathf.Max(0f, value);
    }

    public float LaserSpeed
    {
        get => laserSpeed;
        set => laserSpeed = value;
    }

    public int TotalPatterns => System.Enum.GetValues(typeof(LaserPatternType)).Length;

    [Header("Corridor Dimensions")]
    [SerializeField] private float corridorWidth = 7.8f;
    [SerializeField] private float corridorHeight = 6.0f;
    [SerializeField] private float despawnZ = -43f;

    [Header("Material")]
    [SerializeField] private Material laserMaterial; // Red glowing emissive material

    private float timer = 0f;
    private bool isSpawningActive = false;
    private int patternIndex = 0;
    private int lastPatternIndex = -1;
    private readonly List<GameObject> activeLasers = new List<GameObject>();
    private readonly List<int> patternBag = new List<int>();

    private void Start()
    {
        if (laserMaterial == null)
        {
            laserMaterial = CreateDefaultLaserMaterial();
        }

        if (autoSpawn)
        {
            StartSpawning();
        }
    }

    private void Update()
    {
        if (!isSpawningActive) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnNextPattern();
        }
    }

    public void StartSpawning()
    {
        isSpawningActive = true;
        timer = spawnInterval - initialDelay; // Starts after initialDelay
    }

    public void StopSpawning()
    {
        isSpawningActive = false;
    }

    /// <summary>
    /// Spawns the next pattern according to sequential rotation, test override, or randomized bag.
    /// </summary>
    public void SpawnNextPattern()
    {
        if (useSpecificPattern)
        {
            SpawnPattern(testPattern);
            return;
        }

        int count = TotalPatterns;
        LaserPatternType chosenPattern;

        if (randomizePatterns)
        {
            int nextIdx;
            if (avoidImmediateRepeat && count > 1)
            {
                do
                {
                    nextIdx = Random.Range(0, count);
                } while (nextIdx == lastPatternIndex);
            }
            else
            {
                nextIdx = Random.Range(0, count);
            }
            lastPatternIndex = nextIdx;
            chosenPattern = (LaserPatternType)nextIdx;
        }
        else
        {
            chosenPattern = (LaserPatternType)(patternIndex % count);
            patternIndex++;
        }

        SpawnPattern(chosenPattern);
    }

    public void SpawnPattern(LaserPatternType pattern)
    {
        switch (pattern)
        {
            case LaserPatternType.LowHurdle:
                SpawnLowHurdle();
                break;
            case LaserPatternType.HighBeam:
                SpawnHighBeam();
                break;
            case LaserPatternType.LeftPassage:
                SpawnLeftPassage();
                break;
            case LaserPatternType.RightPassage:
                SpawnRightPassage();
                break;
            case LaserPatternType.CenterDoorway:
                SpawnCenterDoorway();
                break;
            case LaserPatternType.SideFlanks:
                SpawnSideFlanks();
                break;
            case LaserPatternType.GridCrouchHatch:
                SpawnGridCrouchHatch();
                break;
            case LaserPatternType.MidAirWindow:
                SpawnMidAirWindow();
                break;
            case LaserPatternType.StaggeredChoice:
                SpawnStaggeredChoice();
                break;
            case LaserPatternType.DoubleWindowGrid:
                SpawnDoubleWindowGrid();
                break;
        }
    }

    // -------------------------------------------------------------
    // PATTERN IMPLEMENTATIONS (All guaranteed physically passable)
    // -------------------------------------------------------------

    /// <summary>
    /// 1. Low Hurdle: Full-width laser beam at ~0.38m. Player jumps over.
    /// </summary>
    private void SpawnLowHurdle()
    {
        float halfW = corridorWidth * 0.5f;
        GameObject root = CreatePatternRoot("Laser_LowHurdle", laserSpeed);
        AddHorizontalBeam(root, -halfW, halfW, 0.38f, 0.14f);
    }

    /// <summary>
    /// 2. High Beam: Overhead laser beams starting at 1.55m. Player ducks / crouches under (player crouch is 1.0m).
    /// </summary>
    private void SpawnHighBeam()
    {
        float halfW = corridorWidth * 0.5f;
        GameObject root = CreatePatternRoot("Laser_HighBeam", laserSpeed);
        AddHorizontalBeam(root, -halfW, halfW, 1.55f, 0.14f);
        AddHorizontalBeam(root, -halfW, halfW, 2.7f, 0.12f);
        AddHorizontalBeam(root, -halfW, halfW, 3.9f, 0.12f);
        AddHorizontalBeam(root, -halfW, halfW, 5.1f, 0.12f);
    }

    /// <summary>
    /// 3. Left Passage: Laser wall blocks right ~60% of corridor. Clear ~3m safe passage on the LEFT.
    /// </summary>
    private void SpawnLeftPassage()
    {
        float halfW = corridorWidth * 0.5f;
        float splitX = -1.0f; // Safe gap from -halfW to -1.0f (~2.9m wide)
        GameObject root = CreatePatternRoot("Laser_LeftPassage", laserSpeed);

        AddHorizontalBeam(root, splitX, halfW, 0.6f);
        AddHorizontalBeam(root, splitX, halfW, 1.7f);
        AddHorizontalBeam(root, splitX, halfW, 2.8f);
        AddHorizontalBeam(root, splitX, halfW, 3.9f);
        AddHorizontalBeam(root, splitX, halfW, 5.0f);
        AddVerticalBeam(root, splitX, 0.1f, corridorHeight - 0.2f);
        AddVerticalBeam(root, (splitX + halfW) * 0.5f, 0.1f, corridorHeight - 0.2f);
    }

    /// <summary>
    /// 4. Right Passage: Laser wall blocks left ~60% of corridor. Clear ~3m safe passage on the RIGHT.
    /// </summary>
    private void SpawnRightPassage()
    {
        float halfW = corridorWidth * 0.5f;
        float splitX = 1.0f; // Safe gap from 1.0f to halfW (~2.9m wide)
        GameObject root = CreatePatternRoot("Laser_RightPassage", laserSpeed);

        AddHorizontalBeam(root, -halfW, splitX, 0.6f);
        AddHorizontalBeam(root, -halfW, splitX, 1.7f);
        AddHorizontalBeam(root, -halfW, splitX, 2.8f);
        AddHorizontalBeam(root, -halfW, splitX, 3.9f);
        AddHorizontalBeam(root, -halfW, splitX, 5.0f);
        AddVerticalBeam(root, splitX, 0.1f, corridorHeight - 0.2f);
        AddVerticalBeam(root, (-halfW + splitX) * 0.5f, 0.1f, corridorHeight - 0.2f);
    }

    /// <summary>
    /// 5. Center Doorway: Both left and right walls blocked by lasers, leaving a 2.3m wide center open door.
    /// </summary>
    private void SpawnCenterDoorway()
    {
        float halfW = corridorWidth * 0.5f;
        float doorHalf = 1.15f; // Door opening is 2.3m wide (from -1.15 to +1.15)
        GameObject root = CreatePatternRoot("Laser_CenterDoorway", laserSpeed);

        // Left wing
        AddHorizontalBeam(root, -halfW, -doorHalf, 0.6f);
        AddHorizontalBeam(root, -halfW, -doorHalf, 1.7f);
        AddHorizontalBeam(root, -halfW, -doorHalf, 2.8f);
        AddHorizontalBeam(root, -halfW, -doorHalf, 3.9f);
        AddHorizontalBeam(root, -halfW, -doorHalf, 5.0f);
        AddVerticalBeam(root, -doorHalf, 0.1f, corridorHeight - 0.2f);

        // Right wing
        AddHorizontalBeam(root, doorHalf, halfW, 0.6f);
        AddHorizontalBeam(root, doorHalf, halfW, 1.7f);
        AddHorizontalBeam(root, doorHalf, halfW, 2.8f);
        AddHorizontalBeam(root, doorHalf, halfW, 3.9f);
        AddHorizontalBeam(root, doorHalf, halfW, 5.0f);
        AddVerticalBeam(root, doorHalf, 0.1f, corridorHeight - 0.2f);

        // Overhead door lintel & upper filler
        AddHorizontalBeam(root, -doorHalf, doorHalf, 2.8f);
        AddHorizontalBeam(root, -doorHalf, doorHalf, 4.2f);
        AddHorizontalBeam(root, -doorHalf, doorHalf, 5.4f);
    }

    /// <summary>
    /// 6. Side Flanks: Dense center laser pillar. Player runs through either the left or right wall lane.
    /// </summary>
    private void SpawnSideFlanks()
    {
        float pillarHalf = 1.1f; // Pillar is 2.2m wide in center. Leaves 2.8m open lane on both flanks!
        GameObject root = CreatePatternRoot("Laser_SideFlanks", laserSpeed);

        AddHorizontalBeam(root, -pillarHalf, pillarHalf, 0.5f);
        AddHorizontalBeam(root, -pillarHalf, pillarHalf, 1.5f);
        AddHorizontalBeam(root, -pillarHalf, pillarHalf, 2.6f);
        AddHorizontalBeam(root, -pillarHalf, pillarHalf, 3.7f);
        AddHorizontalBeam(root, -pillarHalf, pillarHalf, 4.8f);
        AddHorizontalBeam(root, -pillarHalf, pillarHalf, 5.6f);
        AddVerticalBeam(root, -pillarHalf, 0.1f, corridorHeight - 0.2f);
        AddVerticalBeam(root, pillarHalf, 0.1f, corridorHeight - 0.2f);
    }

    /// <summary>
    /// 7. Grid Crouch Hatch: Iconic Resident Evil cross-grid with an illuminated 2.3m x 1.35m crawlway hatch at floor level.
    /// </summary>
    private void SpawnGridCrouchHatch()
    {
        float halfW = corridorWidth * 0.5f;
        float hatchHalf = 1.15f; // 2.3m wide crawlway hatch in center
        float hatchHeight = 1.35f; // Player crouching is 1.0m tall, fits with 0.35m clearance
        GameObject root = CreatePatternRoot("Laser_GridCrouchHatch", laserSpeed * 0.85f); // Slightly slower for readability

        // Horizontal bars across upper half
        AddHorizontalBeam(root, -halfW, halfW, 1.4f);
        AddHorizontalBeam(root, -halfW, halfW, 2.5f);
        AddHorizontalBeam(root, -halfW, halfW, 3.6f);
        AddHorizontalBeam(root, -halfW, halfW, 4.7f);
        AddHorizontalBeam(root, -halfW, halfW, 5.6f);

        // Lower bars on left and right outside the hatch
        AddHorizontalBeam(root, -halfW, -hatchHalf, 0.65f);
        AddHorizontalBeam(root, hatchHalf, halfW, 0.65f);

        // Vertical boundary bars
        AddVerticalBeam(root, -hatchHalf, 0.1f, corridorHeight - 0.2f);
        AddVerticalBeam(root, hatchHalf, 0.1f, corridorHeight - 0.2f);
        AddVerticalBeam(root, -halfW * 0.6f, 0.1f, corridorHeight - 0.2f);
        AddVerticalBeam(root, halfW * 0.6f, 0.1f, corridorHeight - 0.2f);
    }

    /// <summary>
    /// 8. Mid-Air Window: Low hurdle barrier (0.45m) and top ceiling barrier. Center window is open from 0.5m to 3.2m.
    /// Player leaps forward through the window!
    /// </summary>
    private void SpawnMidAirWindow()
    {
        float halfW = corridorWidth * 0.5f;
        float windowHalf = 1.4f; // 2.8m wide mid window
        GameObject root = CreatePatternRoot("Laser_MidAirWindow", laserSpeed);

        // Bottom hurdle
        AddHorizontalBeam(root, -halfW, halfW, 0.45f);

        // Top ceiling barrier
        AddHorizontalBeam(root, -halfW, halfW, 3.2f);
        AddHorizontalBeam(root, -halfW, halfW, 4.5f);
        AddHorizontalBeam(root, -halfW, halfW, 5.6f);

        // Left & right wing fillers between hurdle and ceiling
        AddVerticalBeam(root, -windowHalf, 0.45f, 3.2f);
        AddVerticalBeam(root, windowHalf, 0.45f, 3.2f);
        AddHorizontalBeam(root, -halfW, -windowHalf, 1.8f);
        AddHorizontalBeam(root, windowHalf, halfW, 1.8f);
    }

    /// <summary>
    /// 9. Staggered Choice: Low hurdle on left (jump), high beam on right (crouch). Player chooses their dodge style!
    /// </summary>
    private void SpawnStaggeredChoice()
    {
        float halfW = corridorWidth * 0.5f;
        GameObject root = CreatePatternRoot("Laser_StaggeredChoice", laserSpeed);

        // Left half: Low hurdle (jump)
        AddHorizontalBeam(root, -halfW, 0f, 0.38f, 0.14f);

        // Right half: High beam (duck)
        AddHorizontalBeam(root, 0f, halfW, 1.55f, 0.14f);
        AddHorizontalBeam(root, 0f, halfW, 2.8f, 0.12f);
        AddHorizontalBeam(root, 0f, halfW, 4.1f, 0.12f);
        AddHorizontalBeam(root, 0f, halfW, 5.3f, 0.12f);

        // Central divider post
        AddVerticalBeam(root, 0f, 0.1f, corridorHeight - 0.2f, 0.14f);
    }

    /// <summary>
    /// 10. Double Window Grid: Two distinct open doorways in the grid (one left, one right).
    /// </summary>
    private void SpawnDoubleWindowGrid()
    {
        float halfW = corridorWidth * 0.5f;
        GameObject root = CreatePatternRoot("Laser_DoubleWindowGrid", laserSpeed * 0.88f);

        // Center pillar
        float centerHalf = 0.65f;
        AddVerticalBeam(root, -centerHalf, 0.1f, corridorHeight - 0.2f);
        AddVerticalBeam(root, centerHalf, 0.1f, corridorHeight - 0.2f);
        AddHorizontalBeam(root, -centerHalf, centerHalf, 0.8f);
        AddHorizontalBeam(root, -centerHalf, centerHalf, 2.0f);
        AddHorizontalBeam(root, -centerHalf, centerHalf, 3.2f);
        AddHorizontalBeam(root, -centerHalf, centerHalf, 4.4f);
        AddHorizontalBeam(root, -centerHalf, centerHalf, 5.5f);

        // Overhead headers across doorways
        AddHorizontalBeam(root, -halfW, -centerHalf, 3.0f);
        AddHorizontalBeam(root, centerHalf, halfW, 3.0f);
        AddHorizontalBeam(root, -halfW, -centerHalf, 4.5f);
        AddHorizontalBeam(root, centerHalf, halfW, 4.5f);
        AddHorizontalBeam(root, -halfW, -centerHalf, 5.6f);
        AddHorizontalBeam(root, centerHalf, halfW, 5.6f);

        // Outer edge vertical framing
        AddVerticalBeam(root, -halfW + 0.3f, 0.1f, corridorHeight - 0.2f);
        AddVerticalBeam(root, halfW - 0.3f, 0.1f, corridorHeight - 0.2f);
    }

    // -------------------------------------------------------------
    // LASER BUILDER HELPERS
    // -------------------------------------------------------------

    private GameObject CreatePatternRoot(string patternName, float speed)
    {
        GameObject root = new GameObject(patternName);
        root.transform.position = transform.position;

        LaserBeam beamScript = root.AddComponent<LaserBeam>();
        beamScript.Speed = speed;
        beamScript.Damage = laserDamage;
        beamScript.MoveDirection = Vector3.back;
        beamScript.DespawnZThreshold = despawnZ;

        activeLasers.Add(root);
        return root;
    }

    private void AddHorizontalBeam(GameObject parent, float xMin, float xMax, float y, float thickness = 0.12f)
    {
        float width = Mathf.Abs(xMax - xMin);
        float xCenter = (xMin + xMax) * 0.5f;
        CreateChildBeam(parent, new Vector3(xCenter, y, 0), new Vector3(width, thickness, thickness), "Laser_Bar_H");
    }

    private void AddVerticalBeam(GameObject parent, float x, float yMin, float yMax, float thickness = 0.12f)
    {
        float height = Mathf.Abs(yMax - yMin);
        float yCenter = (yMin + yMax) * 0.5f;
        CreateChildBeam(parent, new Vector3(x, yCenter, 0), new Vector3(thickness, height, thickness), "Laser_Bar_V");
    }

    private void CreateChildBeam(GameObject parent, Vector3 localPos, Vector3 localScale, string barName = "Laser_Bar")
    {
        GameObject child = GameObject.CreatePrimitive(PrimitiveType.Cube);
        child.name = barName;
        child.transform.SetParent(parent.transform);
        child.transform.localPosition = localPos;
        child.transform.localScale = localScale;

        Renderer rend = child.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material = laserMaterial;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        Collider col = child.GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Forward trigger events to parent
        LaserBeamTriggerForwarder forwarder = child.AddComponent<LaserBeamTriggerForwarder>();
        forwarder.Setup(parent.GetComponent<LaserBeam>());
    }

    /// <summary>
    /// Destroys all active lasers currently in transit down the corridor.
    /// </summary>
    public void ClearAllActiveLasers()
    {
        for (int i = activeLasers.Count - 1; i >= 0; i--)
        {
            if (activeLasers[i] != null)
            {
                Destroy(activeLasers[i]);
            }
        }
        activeLasers.Clear();
        timer = 0f;
    }

    private Material CreateDefaultLaserMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        mat.name = "CorridorLaser_Material";
        Color laserRed = new Color(1.0f, 0.05f, 0.05f, 1.0f);
        mat.color = laserRed;

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", laserRed * 2.5f);
        }

        return mat;
    }
}

/// <summary>
/// Helper for compound laser grids to pass trigger hits to the parent LaserBeam.
/// </summary>
public class LaserBeamTriggerForwarder : MonoBehaviour
{
    private LaserBeam parentBeam;

    public void Setup(LaserBeam parent)
    {
        parentBeam = parent;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (health != null && parentBeam != null)
        {
            health.TakeDamage(parentBeam.Damage);
        }
    }
}
