using UnityEngine;

/// <summary>
/// Controls an individual laser beam or grid moving down the corridor toward the player.
/// Deals damage on contact and self-destructs after reaching the end of its travel path.
/// </summary>
public class LaserBeam : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement direction in world space (typically towards the player start position)")]
    [SerializeField] private Vector3 moveDirection = Vector3.back;
    [SerializeField] private float speed = 7f;
    [SerializeField] private float lifetime = 15f;
    [SerializeField] private float despawnZThreshold = -25f; // Despawns if it travels past this Z coord

    [Header("Damage Settings")]
    [SerializeField] private int damage = 35;

    [Header("Pattern Oscillation (Optional)")]
    [SerializeField] private bool oscillateVertical = false;
    [SerializeField] private float verticalAmplitude = 0.8f;
    [SerializeField] private float verticalFrequency = 2f;
    
    [SerializeField] private bool oscillateHorizontal = false;
    [SerializeField] private float horizontalAmplitude = 1.2f;
    [SerializeField] private float horizontalFrequency = 1.5f;

    private float age = 0f;
    private Vector3 basePosition;
    private float currentSpeedMultiplier = 1f;

    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    public int Damage
    {
        get => damage;
        set => damage = value;
    }

    public Vector3 MoveDirection
    {
        get => moveDirection;
        set => moveDirection = value.normalized;
    }

    public float DespawnZThreshold
    {
        get => despawnZThreshold;
        set => despawnZThreshold = value;
    }

    public bool OscillateVertical
    {
        get => oscillateVertical;
        set => oscillateVertical = value;
    }

    public float VerticalAmplitude
    {
        get => verticalAmplitude;
        set => verticalAmplitude = value;
    }

    public float VerticalFrequency
    {
        get => verticalFrequency;
        set => verticalFrequency = value;
    }

    private void Start()
    {
        basePosition = transform.position;
        // Ensure collider is set to trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        age += Time.deltaTime;

        // Move forward along path
        Vector3 forwardStep = moveDirection * (speed * currentSpeedMultiplier * Time.deltaTime);
        basePosition += forwardStep;

        Vector3 nextPos = basePosition;

        // Add oscillation offsets if enabled
        if (oscillateVertical)
        {
            nextPos.y += Mathf.Sin(age * verticalFrequency * Mathf.PI * 2f) * verticalAmplitude;
        }
        if (oscillateHorizontal)
        {
            nextPos.x += Mathf.Sin(age * horizontalFrequency * Mathf.PI * 2f) * horizontalAmplitude;
        }

        transform.position = nextPos;

        // Check lifetime or threshold
        if (age >= lifetime || transform.position.z <= despawnZThreshold)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Try to find PlayerHealth
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null)
        {
            health = other.GetComponentInParent<PlayerHealth>();
        }

        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }

    /// <summary>
    /// Temporarily slows down this beam (e.g. from a slow bonus panel).
    /// </summary>
    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = Mathf.Clamp(multiplier, 0.1f, 3f);
    }
}
