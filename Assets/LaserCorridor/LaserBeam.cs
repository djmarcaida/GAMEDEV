using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [SerializeField] private Vector3 moveDirection = Vector3.back;
    [SerializeField] private float speed = 7f;
    [SerializeField] private float lifetime = 15f;
    [SerializeField] private float despawnZThreshold = -25f;
    [SerializeField] private int damage = 35;

    [SerializeField] private bool oscillateVertical;
    [SerializeField] private float verticalAmplitude = 0.8f;
    [SerializeField] private float verticalFrequency = 2f;

    [SerializeField] private bool oscillateHorizontal;
    [SerializeField] private float horizontalAmplitude = 1.2f;
    [SerializeField] private float horizontalFrequency = 1.5f;

    private float age;
    private Vector3 basePosition;
    private float speedMultiplier = 1f;

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

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Clamp(multiplier, 0.1f, 3f);
    }

    private void Start()
    {
        basePosition = transform.position;

        if (TryGetComponent<Collider>(out var col))
        {
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        age += Time.deltaTime;

        basePosition += moveDirection * (speed * speedMultiplier * Time.deltaTime);
        Vector3 pos = basePosition;

        if (oscillateVertical)
            pos.y += Mathf.Sin(age * verticalFrequency * Mathf.PI * 2f) * verticalAmplitude;

        if (oscillateHorizontal)
            pos.x += Mathf.Sin(age * horizontalFrequency * Mathf.PI * 2f) * horizontalAmplitude;

        transform.position = pos;

        if (age >= lifetime || transform.position.z <= despawnZThreshold)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerHealth>(out var health) ||
            (other.transform.parent != null && other.transform.parent.TryGetComponent(out health)))
        {
            health.TakeDamage(damage);
        }
    }
}
