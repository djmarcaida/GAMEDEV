using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coordinates the stealth game mode: builds the stealth layout, aggregates hazard suspicion for the HUD,
/// handles collecting revealed hidden objects, and applies EMP / slowdown effects.
/// </summary>
public class StealthManager : MonoBehaviour
{
    public static StealthManager Instance { get; private set; }

    [Header("Stealth Configuration")]
    [SerializeField] private bool autoPopulateStealthLevel = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [Tooltip("How far off the crosshair (degrees) a revealed object can be and still be collected.")]
    [SerializeField] private float interactAimAngle = 12f;

    private readonly List<StealthHazardBase> hazards = new();
    private readonly List<HiddenStealthObject> secrets = new();
    private Transform cameraTransform;
    private PlayerHealth playerHealth;
    private float slowdownTimer;
    private string notification = "";
    private float notificationTimer;

    public float HighestSuspicion { get; private set; }
    public StealthHazardBase.HazardAlertState HighestState { get; private set; }
    public int SecretsCollected { get; private set; }
    public int TotalSecrets => secrets.Count;
    public HiddenStealthObject CurrentInteractable { get; private set; }
    public string StatusNotification => notificationTimer > 0f ? notification : "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        if (StealthAudio.Instance == null)
            gameObject.AddComponent<StealthAudio>();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        var player = FindAnyObjectByType<Player>();
        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        var cam = Camera.main;
        if (cam == null && player != null)
            cam = player.GetComponentInChildren<Camera>();
        if (cam != null)
        {
            cameraTransform = cam.transform;
            if (cam.GetComponent<PlayerFlashlight>() == null)
                cam.gameObject.AddComponent<PlayerFlashlight>();
        }

        if (autoPopulateStealthLevel && FindAnyObjectByType<StealthHazardBase>() == null)
            BuildStealthLevel();

        hazards.AddRange(FindObjectsByType<StealthHazardBase>());
        secrets.AddRange(FindObjectsByType<HiddenStealthObject>());
    }

    private void Update()
    {
        if (notificationTimer > 0f)
            notificationTimer -= Time.deltaTime;

        if (slowdownTimer > 0f)
        {
            slowdownTimer -= Time.deltaTime;
            if (slowdownTimer <= 0f)
                SetHazardSpeed(1f);
        }

        UpdateAggregateSuspicion();
        UpdateInteraction();
    }

    private void UpdateAggregateSuspicion()
    {
        HighestSuspicion = 0f;
        HighestState = StealthHazardBase.HazardAlertState.Calm;

        foreach (var hazard in hazards)
        {
            if (hazard == null || !hazard.isActiveAndEnabled) continue;
            HighestSuspicion = Mathf.Max(HighestSuspicion, hazard.Suspicion);
            if (hazard.State > HighestState)
                HighestState = hazard.State;
        }
    }

    /// <summary>Picks the revealed object closest to the crosshair within reach and collects it on [E].</summary>
    private void UpdateInteraction()
    {
        CurrentInteractable = null;
        if (cameraTransform == null) return;

        float bestAngle = interactAimAngle;
        foreach (var secret in secrets)
        {
            if (secret == null || secret.IsCollected || !secret.IsRevealed) continue;

            Vector3 toSecret = secret.transform.position - cameraTransform.position;
            if (toSecret.magnitude > secret.InteractDistance) continue;

            float angle = Vector3.Angle(cameraTransform.forward, toSecret);
            if (angle <= bestAngle)
            {
                bestAngle = angle;
                CurrentInteractable = secret;
            }
        }

        if (CurrentInteractable != null && Input.GetKeyDown(interactKey))
        {
            var collected = CurrentInteractable;
            CurrentInteractable = null;
            collected.Collect(playerHealth);
            SecretsCollected++;
            ShowNotification($"Picked up {collected.ObjectName}  ({SecretsCollected}/{secrets.Count})");
        }
    }

    private void ShowNotification(string message, float duration = 3.5f)
    {
        notification = message;
        notificationTimer = duration;
    }

    public void TriggerEmpBlast(float duration)
    {
        ShowNotification($"Security systems offline for {duration:F0}s");
        foreach (var hazard in hazards)
            if (hazard != null) hazard.ApplyEmp(duration);
    }

    public void ApplyTemporaryHazardSlowdown(float speedMultiplier, float duration)
    {
        slowdownTimer = duration;
        ShowNotification("Security override: hazards slowed");
        SetHazardSpeed(speedMultiplier);
    }

    private void SetHazardSpeed(float multiplier)
    {
        foreach (var hazard in hazards)
            if (hazard != null) hazard.SetSpeedMultiplier(multiplier);
    }

    /// <summary>Called by the game manager at the start of every round (including after death).</summary>
    public void ResetRound()
    {
        slowdownTimer = 0f;
        SecretsCollected = 0;
        CurrentInteractable = null;

        foreach (var hazard in hazards)
            if (hazard != null) hazard.ResetHazard();

        foreach (var secret in secrets)
            if (secret != null) secret.ResetState();
    }

    /// <summary>
    /// Builds cover, hazards and hidden collectibles to match the zones laid out by <see cref="FacilityBuilder"/>
    /// (x: -6..6, floor y = 0, z: -60..60, player starts at z = -56 heading +z).
    /// </summary>
    private void BuildStealthLevel()
    {
        var root = new GameObject("Stealth_Level_Elements").transform;
        var barrier = new Vector3(2.4f, 1.15f, 0.7f);

        // Offices: drone sweeping between the desks; barriers and desks block its line of sight.
        CreateBarricade(root, new Vector3(-2.2f, 0f, -44f), barrier);
        CreateBarricade(root, new Vector3(2.4f, 0f, -37.5f), barrier);
        CreateHazard<DroneSphereHazard>(root, "Hazard_PatrolDrone_CheckSphere", new Vector3(-4.2f, 1.6f, -40f), new Vector3(4.2f, 1.6f, -40f), 3f);

        // Storage: laser curtain spanning the aisle between the shelving racks; crouch to pass under it.
        var scanner = CreateHazard<ScannerCubeHazard>(root, "Hazard_LaserScanner_CheckBox", new Vector3(0f, 0f, -31f), new Vector3(0f, 0f, -17f), 2.4f);
        scanner.BoxSize = new Vector3(10.7f, 2.4f, 1.0f);

        // Security atrium: ceiling camera facing the approach; move between the barriers while it pans away.
        CreateBarricade(root, new Vector3(-2.5f, 0f, -8f), barrier);
        CreateBarricade(root, new Vector3(3f, 0f, -1f), barrier);
        CreateBarricade(root, new Vector3(-3f, 0f, 5f), barrier);
        var turret = CreateHazard<TurretRaycastHazard>(root, "Hazard_Turret_Raycast", new Vector3(0f, 5.7f, 12f), new Vector3(0f, 5.7f, 12f), 0f);
        turret.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        // Server room: two laser columns criss-crossing the aisle between the racks.
        CreateHazard<ColumnCapsuleHazard>(root, "Hazard_SecurityColumn_A_CheckCapsule", new Vector3(-3.8f, 0f, 18f), new Vector3(3.8f, 0f, 26f), 2.6f);
        CreateHazard<ColumnCapsuleHazard>(root, "Hazard_SecurityColumn_B_CheckCapsule", new Vector3(3.8f, 0f, 30f), new Vector3(-3.8f, 0f, 38f), 3f);

        // Loading bay: a faster drone guarding the exit; crates give cover.
        CreateHazard<DroneSphereHazard>(root, "Hazard_PatrolDrone_B_CheckSphere", new Vector3(4f, 1.6f, 46f), new Vector3(-4f, 1.6f, 46f), 3.4f);

        // Hidden objects: only visible inside the flashlight cone.
        CreateHiddenObject(root, new Vector3(-5.75f, 2.03f, -52.5f), "Access Codes",
            HiddenStealthObject.HiddenObjectType.ClassifiedIntel);
        CreateHiddenObject(root, new Vector3(5.3f, 0.94f, -41.7f), "Personnel File",
            HiddenStealthObject.HiddenObjectType.ClassifiedIntel);
        CreateHiddenObject(root, new Vector3(-5.3f, 1.58f, -26.5f), "Override Keycard",
            HiddenStealthObject.HiddenObjectType.SecurityOverrideKeycard);
        CreateHiddenObject(root, new Vector3(-2.5f, 0.4f, -7.3f), "Nanite Health Infusion",
            HiddenStealthObject.HiddenObjectType.NaniteMedkit);
        CreateHiddenObject(root, new Vector3(5.6f, 0.4f, 28f), "Tactical EMP Disruptor",
            HiddenStealthObject.HiddenObjectType.EmpDevice);
    }

    private static T CreateHazard<T>(Transform parent, string name, Vector3 a, Vector3 b, float speed) where T : StealthHazardBase
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.position = a;
        var hazard = obj.AddComponent<T>();
        hazard.Configure(a, b, speed);
        return hazard;
    }

    private static void CreateBarricade(Transform parent, Vector3 pos, Vector3 size)
    {
        var obj = new GameObject("Cover_Barricade");
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        obj.AddComponent<StealthCoverBarricade>().Size = size;
    }

    private static void CreateHiddenObject(Transform parent, Vector3 pos, string displayName, HiddenStealthObject.HiddenObjectType type)
    {
        var obj = new GameObject($"Hidden_{displayName.Replace(' ', '_')}");
        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        obj.AddComponent<HiddenStealthObject>().Configure(displayName, type);
    }
}
