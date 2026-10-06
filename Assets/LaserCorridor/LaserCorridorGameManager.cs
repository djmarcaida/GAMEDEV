using System.Collections;
using UnityEngine;

/// <summary>
/// Central manager for the Resident Evil Laser Corridor experience.
/// Coordinates game loop, respawns, laser spawning, and level completion.
/// </summary>
public class LaserCorridorGameManager : MonoBehaviour
{
    public static LaserCorridorGameManager Instance { get; private set; }

    public enum GameState
    {
        Ready,
        Playing,
        GameOver,
        Victory
    }

    [Header("Core References")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Transform startPoint;
    [SerializeField] private LaserSpawner laserSpawner;
    [SerializeField] private CorridorGoal corridorGoal;

    [Header("Respawn & Delay Settings")]
    [SerializeField] private float gameOverRespawnDelay = 2.0f;

    private GameState currentState = GameState.Ready;
    private PlayerHealth playerHealth;
    private BonusFloorPanel[] bonusPanels;

    public GameState State => currentState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Auto-discover references if unassigned
        if (playerObject == null)
        {
            Player p = FindFirstObjectByType<Player>();
            if (p != null) playerObject = p.gameObject;
        }

        if (playerObject != null)
        {
            playerHealth = playerObject.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = playerObject.AddComponent<PlayerHealth>();
            }

            // Hook death event
            playerHealth.OnDeath += HandlePlayerDeath;
        }

        if (laserSpawner == null)
        {
            laserSpawner = FindFirstObjectByType<LaserSpawner>();
        }

        if (corridorGoal == null)
        {
            corridorGoal = FindFirstObjectByType<CorridorGoal>();
        }

        // Cache all floor bonus panels
        bonusPanels = FindObjectsByType<BonusFloorPanel>(FindObjectsSortMode.None);

        // Record start point if not set
        if (startPoint == null && playerObject != null)
        {
            GameObject spObj = new GameObject("StartPoint_Auto");
            spObj.transform.position = playerObject.transform.position;
            spObj.transform.rotation = playerObject.transform.rotation;
            startPoint = spObj.transform;
        }

        StartRound();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandlePlayerDeath;
        }
    }

    public void StartRound()
    {
        currentState = GameState.Playing;

        if (laserSpawner != null)
        {
            laserSpawner.ClearAllActiveLasers();
            laserSpawner.StartSpawning();
        }

        if (corridorGoal != null)
        {
            corridorGoal.ResetGoal();
        }

        ResetAllBonusPanels();
    }

    private void HandlePlayerDeath()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.GameOver;
        Debug.Log("[GameManager] GAME OVER! Player eliminated. Preparing reset...");

        if (laserSpawner != null)
        {
            laserSpawner.StopSpawning();
        }

        StartCoroutine(ResetPlayerRoutine());
    }

    public void OnGoalReached()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Victory;
        Debug.Log("[GameManager] VICTORY! Corridor Cleared!");

        if (laserSpawner != null)
        {
            laserSpawner.StopSpawning();
            laserSpawner.ClearAllActiveLasers();
        }
    }

    private IEnumerator ResetPlayerRoutine()
    {
        yield return new WaitForSeconds(gameOverRespawnDelay);

        // Teleport player back to start point safely
        if (playerObject != null && startPoint != null)
        {
            Rigidbody rb = playerObject.GetComponent<Rigidbody>();
            CharacterController cc = playerObject.GetComponent<CharacterController>();

            if (cc != null) cc.enabled = false;
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            playerObject.transform.position = startPoint.position;
            playerObject.transform.rotation = startPoint.rotation;

            yield return new WaitForFixedUpdate();

            if (rb != null)
            {
                rb.position = startPoint.position;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = false;
            }

            if (cc != null) cc.enabled = true;
        }

        // Reset player health and buffs
        if (playerHealth != null)
        {
            playerHealth.ResetState();
        }

        // Reset corridor lasers & floor panels
        StartRound();
    }

    private void ResetAllBonusPanels()
    {
        if (bonusPanels == null || bonusPanels.Length == 0)
        {
            bonusPanels = FindObjectsByType<BonusFloorPanel>(FindObjectsSortMode.None);
        }

        foreach (var panel in bonusPanels)
        {
            if (panel != null)
            {
                panel.ResetPanel();
            }
        }
    }
}
