using System.Collections.Generic;
using UnityEngine;

public class LaserSpawner : MonoBehaviour
{
    public enum LaserPatternType
    {
        LowHurdle,
        HighBeam,
        LeftPassage,
        RightPassage,
        CenterDoorway,
        SideFlanks,
        GridCrouchHatch,
        MidAirWindow,
        StaggeredChoice,
        DoubleWindowGrid
    }

    [SerializeField] private bool autoSpawn = true;
    [SerializeField] private float initialDelay = 1.5f;
    [SerializeField] private float spawnInterval = 1.8f;
    [SerializeField] private float laserSpeed = 6.5f;
    [SerializeField] private int laserDamage = 35;

    [SerializeField] private bool randomizePatterns = true;
    [SerializeField] private bool avoidImmediateRepeat = true;
    [SerializeField] private bool useSpecificPattern;
    [SerializeField] private LaserPatternType testPattern = LaserPatternType.LowHurdle;

    [SerializeField] private float corridorWidth = 7.8f;
    [SerializeField] private float corridorHeight = 6.0f;
    [SerializeField] private float despawnZ = -43f;

    [SerializeField] private Material laserMaterial;

    private float timer;
    private bool isSpawning;
    private int patternIndex;
    private int lastPatternIndex = -1;
    private readonly List<GameObject> activeLasers = new();

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

    private void Start()
    {
        if (laserMaterial == null)
            laserMaterial = CreateDefaultLaserMaterial();

        if (autoSpawn)
            StartSpawning();
    }

    private void Update()
    {
        if (!isSpawning) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnNextPattern();
        }
    }

    public void StartSpawning()
    {
        isSpawning = true;
        timer = spawnInterval - initialDelay;
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    public void SpawnNextPattern()
    {
        if (useSpecificPattern)
        {
            SpawnPattern(testPattern);
            return;
        }

        const int count = 10;
        LaserPatternType pattern;

        if (randomizePatterns)
        {
            int nextIdx = Random.Range(0, count);
            if (avoidImmediateRepeat && count > 1)
            {
                while (nextIdx == lastPatternIndex)
                    nextIdx = Random.Range(0, count);
            }
            lastPatternIndex = nextIdx;
            pattern = (LaserPatternType)nextIdx;
        }
        else
        {
            pattern = (LaserPatternType)(patternIndex % count);
            patternIndex++;
        }

        SpawnPattern(pattern);
    }

    public void SpawnPattern(LaserPatternType pattern)
    {
        float hw = corridorWidth * 0.5f;

        switch (pattern)
        {
            case LaserPatternType.LowHurdle:
                SpawnLowHurdle(hw);
                break;
            case LaserPatternType.HighBeam:
                SpawnHighBeam(hw);
                break;
            case LaserPatternType.LeftPassage:
                SpawnLeftPassage(hw);
                break;
            case LaserPatternType.RightPassage:
                SpawnRightPassage(hw);
                break;
            case LaserPatternType.CenterDoorway:
                SpawnCenterDoorway(hw);
                break;
            case LaserPatternType.SideFlanks:
                SpawnSideFlanks();
                break;
            case LaserPatternType.GridCrouchHatch:
                SpawnGridCrouchHatch(hw);
                break;
            case LaserPatternType.MidAirWindow:
                SpawnMidAirWindow(hw);
                break;
            case LaserPatternType.StaggeredChoice:
                SpawnStaggeredChoice(hw);
                break;
            case LaserPatternType.DoubleWindowGrid:
                SpawnDoubleWindowGrid(hw);
                break;
        }
    }

    private void SpawnLowHurdle(float hw)
    {
        var root = CreateRoot("Laser_LowHurdle", laserSpeed);
        AddHBeam(root, -hw, hw, 0.38f, 0.14f);
    }

    private void SpawnHighBeam(float hw)
    {
        var root = CreateRoot("Laser_HighBeam", laserSpeed);
        AddHBeam(root, -hw, hw, 1.55f, 0.14f);
        AddHBeam(root, -hw, hw, 2.7f, 0.12f);
        AddHBeam(root, -hw, hw, 3.9f, 0.12f);
        AddHBeam(root, -hw, hw, 5.1f, 0.12f);
    }

    private void SpawnLeftPassage(float hw)
    {
        const float splitX = -1.0f;
        var root = CreateRoot("Laser_LeftPassage", laserSpeed);
        AddHBeam(root, splitX, hw, 0.6f);
        AddHBeam(root, splitX, hw, 1.7f);
        AddHBeam(root, splitX, hw, 2.8f);
        AddHBeam(root, splitX, hw, 3.9f);
        AddHBeam(root, splitX, hw, 5.0f);
        AddVBeam(root, splitX, 0.1f, corridorHeight - 0.2f);
        AddVBeam(root, (splitX + hw) * 0.5f, 0.1f, corridorHeight - 0.2f);
    }

    private void SpawnRightPassage(float hw)
    {
        const float splitX = 1.0f;
        var root = CreateRoot("Laser_RightPassage", laserSpeed);
        AddHBeam(root, -hw, splitX, 0.6f);
        AddHBeam(root, -hw, splitX, 1.7f);
        AddHBeam(root, -hw, splitX, 2.8f);
        AddHBeam(root, -hw, splitX, 3.9f);
        AddHBeam(root, -hw, splitX, 5.0f);
        AddVBeam(root, splitX, 0.1f, corridorHeight - 0.2f);
        AddVBeam(root, (-hw + splitX) * 0.5f, 0.1f, corridorHeight - 0.2f);
    }

    private void SpawnCenterDoorway(float hw)
    {
        const float dh = 1.15f;
        var root = CreateRoot("Laser_CenterDoorway", laserSpeed);
        AddHBeam(root, -hw, -dh, 0.6f);
        AddHBeam(root, -hw, -dh, 1.7f);
        AddHBeam(root, -hw, -dh, 2.8f);
        AddHBeam(root, -hw, -dh, 3.9f);
        AddHBeam(root, -hw, -dh, 5.0f);
        AddVBeam(root, -dh, 0.1f, corridorHeight - 0.2f);

        AddHBeam(root, dh, hw, 0.6f);
        AddHBeam(root, dh, hw, 1.7f);
        AddHBeam(root, dh, hw, 2.8f);
        AddHBeam(root, dh, hw, 3.9f);
        AddHBeam(root, dh, hw, 5.0f);
        AddVBeam(root, dh, 0.1f, corridorHeight - 0.2f);

        AddHBeam(root, -dh, dh, 2.8f);
        AddHBeam(root, -dh, dh, 4.2f);
        AddHBeam(root, -dh, dh, 5.4f);
    }

    private void SpawnSideFlanks()
    {
        const float ph = 1.1f;
        var root = CreateRoot("Laser_SideFlanks", laserSpeed);
        AddHBeam(root, -ph, ph, 0.5f);
        AddHBeam(root, -ph, ph, 1.5f);
        AddHBeam(root, -ph, ph, 2.6f);
        AddHBeam(root, -ph, ph, 3.7f);
        AddHBeam(root, -ph, ph, 4.8f);
        AddHBeam(root, -ph, ph, 5.6f);
        AddVBeam(root, -ph, 0.1f, corridorHeight - 0.2f);
        AddVBeam(root, ph, 0.1f, corridorHeight - 0.2f);
    }

    private void SpawnGridCrouchHatch(float hw)
    {
        const float hh = 1.15f;
        var root = CreateRoot("Laser_GridCrouchHatch", laserSpeed * 0.85f);
        AddHBeam(root, -hw, hw, 1.4f);
        AddHBeam(root, -hw, hw, 2.5f);
        AddHBeam(root, -hw, hw, 3.6f);
        AddHBeam(root, -hw, hw, 4.7f);
        AddHBeam(root, -hw, hw, 5.6f);
        AddHBeam(root, -hw, -hh, 0.65f);
        AddHBeam(root, hh, hw, 0.65f);
        AddVBeam(root, -hh, 0.1f, corridorHeight - 0.2f);
        AddVBeam(root, hh, 0.1f, corridorHeight - 0.2f);
        AddVBeam(root, -hw * 0.6f, 0.1f, corridorHeight - 0.2f);
        AddVBeam(root, hw * 0.6f, 0.1f, corridorHeight - 0.2f);
    }

    private void SpawnMidAirWindow(float hw)
    {
        const float wh = 1.4f;
        var root = CreateRoot("Laser_MidAirWindow", laserSpeed);
        AddHBeam(root, -hw, hw, 0.45f);
        AddHBeam(root, -hw, hw, 3.2f);
        AddHBeam(root, -hw, hw, 4.5f);
        AddHBeam(root, -hw, hw, 5.6f);
        AddVBeam(root, -wh, 0.45f, 3.2f);
        AddVBeam(root, wh, 0.45f, 3.2f);
        AddHBeam(root, -hw, -wh, 1.8f);
        AddHBeam(root, wh, hw, 1.8f);
    }

    private void SpawnStaggeredChoice(float hw)
    {
        var root = CreateRoot("Laser_StaggeredChoice", laserSpeed);
        AddHBeam(root, -hw, 0f, 0.38f, 0.14f);
        AddHBeam(root, 0f, hw, 1.55f, 0.14f);
        AddHBeam(root, 0f, hw, 2.8f, 0.12f);
        AddHBeam(root, 0f, hw, 4.1f, 0.12f);
        AddHBeam(root, 0f, hw, 5.3f, 0.12f);
        AddVBeam(root, 0f, 0.1f, corridorHeight - 0.2f, 0.14f);
    }

    private void SpawnDoubleWindowGrid(float hw)
    {
        var root = CreateRoot("Laser_DoubleWindowGrid", laserSpeed * 0.88f);
        const float ch = 0.65f;
        AddVBeam(root, -ch, 0.1f, corridorHeight - 0.2f);
        AddVBeam(root, ch, 0.1f, corridorHeight - 0.2f);
        AddHBeam(root, -ch, ch, 0.8f);
        AddHBeam(root, -ch, ch, 2.0f);
        AddHBeam(root, -ch, ch, 3.2f);
        AddHBeam(root, -ch, ch, 4.4f);
        AddHBeam(root, -ch, ch, 5.5f);
        AddHBeam(root, -hw, -ch, 3.0f);
        AddHBeam(root, ch, hw, 3.0f);
        AddHBeam(root, -hw, -ch, 4.5f);
        AddHBeam(root, ch, hw, 4.5f);
        AddHBeam(root, -hw, -ch, 5.6f);
        AddHBeam(root, ch, hw, 5.6f);
        AddVBeam(root, -hw + 0.3f, 0.1f, corridorHeight - 0.2f);
        AddVBeam(root, hw - 0.3f, 0.1f, corridorHeight - 0.2f);
    }

    private GameObject CreateRoot(string name, float speed)
    {
        var root = new GameObject(name);
        root.transform.position = transform.position;

        var beam = root.AddComponent<LaserBeam>();
        beam.Speed = speed;
        beam.Damage = laserDamage;
        beam.MoveDirection = Vector3.back;
        beam.DespawnZThreshold = despawnZ;

        activeLasers.Add(root);
        return root;
    }

    private void AddHBeam(GameObject parent, float xMin, float xMax, float y, float thickness = 0.12f)
    {
        float width = Mathf.Abs(xMax - xMin);
        float xCenter = (xMin + xMax) * 0.5f;
        CreateBar(parent, new Vector3(xCenter, y, 0), new Vector3(width, thickness, thickness));
    }

    private void AddVBeam(GameObject parent, float x, float yMin, float yMax, float thickness = 0.12f)
    {
        float height = Mathf.Abs(yMax - yMin);
        float yCenter = (yMin + yMax) * 0.5f;
        CreateBar(parent, new Vector3(x, yCenter, 0), new Vector3(thickness, height, thickness));
    }

    private void CreateBar(GameObject parent, Vector3 localPos, Vector3 scale)
    {
        var child = GameObject.CreatePrimitive(PrimitiveType.Cube);
        child.name = "Laser_Bar";
        child.transform.SetParent(parent.transform);
        child.transform.localPosition = localPos;
        child.transform.localScale = scale;

        if (child.TryGetComponent<Renderer>(out var rend))
        {
            rend.material = laserMaterial;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        if (child.TryGetComponent<Collider>(out var col))
            col.isTrigger = true;

        child.AddComponent<LaserBeamTriggerForwarder>().Setup(parent.GetComponent<LaserBeam>());
    }

    public void ClearAllActiveLasers()
    {
        for (int i = activeLasers.Count - 1; i >= 0; i--)
        {
            if (activeLasers[i] != null)
                Destroy(activeLasers[i]);
        }
        activeLasers.Clear();
        timer = 0f;
    }

    private Material CreateDefaultLaserMaterial()
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Unlit/Color")
            ?? Shader.Find("Standard");

        var mat = new Material(shader) { name = "CorridorLaser_Material" };
        var red = new Color(1f, 0.05f, 0.05f, 1f);
        mat.color = red;

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", red * 2.5f);
        }

        return mat;
    }
}

public class LaserBeamTriggerForwarder : MonoBehaviour
{
    private LaserBeam parentBeam;

    public void Setup(LaserBeam parent) => parentBeam = parent;

    private void OnTriggerEnter(Collider other)
    {
        if (parentBeam == null) return;

        if (other.TryGetComponent<PlayerHealth>(out var health) ||
            (other.transform.parent != null && other.transform.parent.TryGetComponent(out health)))
        {
            health.TakeDamage(parentBeam.Damage);
        }
    }
}
