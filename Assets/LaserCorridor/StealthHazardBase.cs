using UnityEngine;

/// <summary>
/// Base class for moving corridor hazards with stealth detection.
/// Handles patrol movement, suspicion build-up, alert state, damage and colour feedback.
/// Subclasses only decide whether the player is inside their sensor this frame.
/// </summary>
public abstract class StealthHazardBase : MonoBehaviour
{
    public enum HazardAlertState
    {
        Calm,
        Suspicious,
        Alerted
    }

    private const float SuspiciousThreshold = 0.15f;
    private const float AlertReleaseThreshold = 0.4f;

    [Header("Patrol Movement")]
    [SerializeField] protected Vector3 patrolPointA;
    [SerializeField] protected Vector3 patrolPointB;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float waypointWaitTime = 0.5f;

    [Header("Detection")]
    [Tooltip("Layers the Check* volume queries test against. Defaults to the Player layer.")]
    [SerializeField] protected LayerMask playerLayerMask;
    [Tooltip("Layers that block line-of-sight raycasts. Must include the Player layer.")]
    [SerializeField] protected LayerMask sightMask = Physics.DefaultRaycastLayers;
    [SerializeField] protected float detectionBuildRate = 1.6f;
    [SerializeField] protected float detectionDecayRate = 0.85f;
    [SerializeField] protected float standingMultiplier = 1.35f;
    [SerializeField] protected float crouchMultiplier = 0.55f;
    [SerializeField] protected float stationaryMultiplier = 0.6f;
    [Tooltip("Extra suspicion rate while the player's flashlight is shining on this hazard.")]
    [SerializeField] protected float flashlightMultiplier = 1.5f;
    [SerializeField] protected int damageOnAlert = 25;
    [SerializeField] protected float damageInterval = 1.0f;

    [Header("Visual Feedback Colors")]
    [SerializeField] protected Color calmColor = new Color(0.1f, 0.8f, 0.95f);
    [SerializeField] protected Color suspiciousColor = new Color(1.0f, 0.75f, 0.1f);
    [SerializeField] protected Color alertColor = new Color(1.0f, 0.1f, 0.1f);

    protected Player targetPlayer;
    protected Collider playerCollider;
    protected HazardAlertState alertState = HazardAlertState.Calm;
    protected float suspicionMeter;
    protected float empFreezeTimer;
    protected float speedMultiplier = 1f;

    private PlayerHealth targetHealth;
    private float pingTimer;
    private float damageTimer;
    private bool movingTowardsB = true;
    private float waitTimer;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool visualsBuilt;

    public float Suspicion => suspicionMeter;
    public HazardAlertState State => alertState;

    /// <summary>Whether the hazard turns to face the direction it is patrolling in.</summary>
    protected virtual bool FacesMoveDirection => true;

    protected virtual void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        if (patrolPointA == Vector3.zero && patrolPointB == Vector3.zero)
            patrolPointA = patrolPointB = transform.position;

        if (playerLayerMask.value == 0)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer == -1)
                Debug.LogWarning($"{name}: no 'Player' layer defined; stealth volume checks will not detect the player.", this);
            else
                playerLayerMask = 1 << playerLayer;
        }
    }

    protected virtual void Start()
    {
        BuildVisuals();
        visualsBuilt = true;
        ApplyStateColors(calmColor);
    }

    /// <summary>Creates the hazard's meshes, lights and materials. Runs in Start so Configure* calls made right after AddComponent apply.</summary>
    protected abstract void BuildVisuals();

    /// <summary>Sets the patrol route at runtime. The hazard starts at <paramref name="a"/>.</summary>
    public void Configure(Vector3 a, Vector3 b, float speed)
    {
        patrolPointA = a;
        patrolPointB = b;
        moveSpeed = speed;
        transform.position = initialPosition = a;
    }

    protected virtual void Update()
    {
        if (empFreezeTimer > 0f)
        {
            empFreezeTimer -= Time.deltaTime;
            ApplyStateColors(Color.Lerp(calmColor, Color.gray, 0.6f));
            return;
        }

        if (!EnsurePlayer()) return;

        UpdatePatrolMovement();

        bool detected = PerformPhysicsDetection();
        UpdateSuspicion(detected);
        UpdateFeedback();

        if (alertState == HazardAlertState.Alerted && detected)
            HandleAlertDamage();
    }

    private bool EnsurePlayer()
    {
        if (targetPlayer != null) return true;

        targetPlayer = FindAnyObjectByType<Player>();
        if (targetPlayer == null) return false;

        playerCollider = targetPlayer.GetComponent<Collider>();
        targetHealth = targetPlayer.GetComponent<PlayerHealth>();
        return true;
    }

    /// <summary>
    /// Returns true when the player is inside this hazard's sensor, using a Unity physics query
    /// (CheckSphere, CheckBox, CheckCapsule or Raycast).
    /// </summary>
    protected abstract bool PerformPhysicsDetection();

    protected abstract void ApplyStateColors(Color color);

    protected virtual void UpdatePatrolMovement()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        Vector3 target = movingTowardsB ? patrolPointB : patrolPointA;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * speedMultiplier * Time.deltaTime);

        Vector3 moveDir = target - transform.position;
        if (FacesMoveDirection && moveDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * 6f);

        if (moveDir.sqrMagnitude < 0.0025f)
        {
            movingTowardsB = !movingTowardsB;
            waitTimer = waypointWaitTime;
        }
    }

    /// <summary>
    /// Raycasts from <paramref name="origin"/> to the player's head, chest and feet.
    /// Returns true if any ray reaches the player before hitting a wall or cover.
    /// </summary>
    protected bool HasLineOfSightToPlayer(Vector3 origin, float maxRange, Vector3 forward, float halfConeAngle = 180f)
    {
        if (playerCollider == null) return false;

        Bounds bounds = playerCollider.bounds;
        if ((bounds.ClosestPoint(origin) - origin).sqrMagnitude < 0.0025f)
            return true; // Sensor origin is inside the player; a ray would start inside the collider and miss it.

        for (int i = 0; i < 3; i++)
        {
            float y = Mathf.Lerp(bounds.max.y - 0.1f, bounds.min.y + 0.1f, i * 0.5f);
            Vector3 toPoint = new Vector3(bounds.center.x, y, bounds.center.z) - origin;
            float dist = toPoint.magnitude;

            if (dist > maxRange) continue;
            if (halfConeAngle < 180f && Vector3.Angle(forward, toPoint) > halfConeAngle) continue;

            if (Physics.Raycast(origin, toPoint / dist, out RaycastHit hit, dist, sightMask, QueryTriggerInteraction.Ignore)
                && IsPlayerCollider(hit.collider))
                return true;
        }

        return false;
    }

    protected bool IsPlayerCollider(Collider col)
    {
        return col != null && targetPlayer != null && col.transform.IsChildOf(targetPlayer.transform);
    }

    private void UpdateSuspicion(bool detected)
    {
        if (detected)
        {
            float rate = detectionBuildRate;
            rate *= targetPlayer.IsCrouching ? crouchMultiplier : standingMultiplier;
            if (!targetPlayer.IsMoving) rate *= stationaryMultiplier;

            var flashlight = PlayerFlashlight.Instance;
            if (flashlight != null && flashlight.IsInLightCone(transform.position, 0.5f))
                rate *= flashlightMultiplier;

            suspicionMeter = Mathf.Clamp01(suspicionMeter + rate * Time.deltaTime);
        }
        else
        {
            suspicionMeter = Mathf.Clamp01(suspicionMeter - detectionDecayRate * Time.deltaTime);
        }

        if (alertState == HazardAlertState.Alerted)
        {
            // Hysteresis: stay alarmed until suspicion has clearly dropped.
            if (suspicionMeter < AlertReleaseThreshold)
                alertState = HazardAlertState.Suspicious;
        }
        else if (suspicionMeter >= 1f)
        {
            alertState = HazardAlertState.Alerted;
            damageTimer = damageInterval; // First hit lands immediately.
            if (StealthAudio.Instance != null)
                StealthAudio.Instance.PlayAlarmSiren();
        }
        else
        {
            alertState = suspicionMeter > SuspiciousThreshold ? HazardAlertState.Suspicious : HazardAlertState.Calm;
        }
    }

    private void UpdateFeedback()
    {
        Color stateColor;
        switch (alertState)
        {
            case HazardAlertState.Alerted:
                stateColor = Color.Lerp(alertColor, Color.white, Mathf.PingPong(Time.time * 8f, 1f) * 0.4f);
                break;

            case HazardAlertState.Suspicious:
                stateColor = Color.Lerp(calmColor, suspiciousColor, suspicionMeter);
                pingTimer += Time.deltaTime;
                if (pingTimer >= Mathf.Lerp(0.8f, 0.25f, suspicionMeter))
                {
                    pingTimer = 0f;
                    if (StealthAudio.Instance != null)
                        StealthAudio.Instance.PlayDetectionPing(suspicionMeter);
                }
                break;

            default:
                stateColor = calmColor;
                pingTimer = 0f;
                break;
        }

        ApplyStateColors(stateColor);
    }

    private void HandleAlertDamage()
    {
        damageTimer += Time.deltaTime;
        if (damageTimer < damageInterval) return;

        damageTimer = 0f;
        if (targetHealth != null)
            targetHealth.TakeDamage(damageOnAlert);
    }

    public void ApplyEmp(float duration)
    {
        empFreezeTimer = duration;
        suspicionMeter = 0f;
        alertState = HazardAlertState.Calm;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Clamp(multiplier, 0.1f, 2.5f);
    }

    public virtual void ResetHazard()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        movingTowardsB = true;
        waitTimer = 0f;
        suspicionMeter = 0f;
        alertState = HazardAlertState.Calm;
        empFreezeTimer = 0f;
        speedMultiplier = 1f;
        damageTimer = 0f;
        pingTimer = 0f;
        if (visualsBuilt) ApplyStateColors(calmColor);
    }
}
