using System.Collections;
using UnityEngine;

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

    [SerializeField] private GameObject playerObject;
    [SerializeField] private Transform startPoint;
    [SerializeField] private LaserSpawner laserSpawner;
    [SerializeField] private CorridorGoal corridorGoal;
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
        if (playerObject == null)
        {
            var p = FindFirstObjectByType<Player>();
            if (p != null) playerObject = p.gameObject;
        }

        if (playerObject != null)
        {
            playerHealth = playerObject.GetComponent<PlayerHealth>() ?? playerObject.AddComponent<PlayerHealth>();
            playerHealth.OnDeath += OnPlayerDied;
        }

        if (laserSpawner == null)
            laserSpawner = FindFirstObjectByType<LaserSpawner>();

        if (corridorGoal == null)
            corridorGoal = FindFirstObjectByType<CorridorGoal>();

        bonusPanels = FindObjectsByType<BonusFloorPanel>(FindObjectsSortMode.None);

        if (startPoint == null && playerObject != null)
        {
            var sp = new GameObject("StartPoint_Auto");
            sp.transform.position = playerObject.transform.position;
            sp.transform.rotation = playerObject.transform.rotation;
            startPoint = sp.transform;
        }

        StartRound();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDeath -= OnPlayerDied;
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
            corridorGoal.ResetGoal();

        ResetBonusPanels();
    }

    private void OnPlayerDied()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.GameOver;

        if (laserSpawner != null)
            laserSpawner.StopSpawning();

        StartCoroutine(RespawnRoutine());
    }

    public void OnGoalReached()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Victory;

        if (laserSpawner != null)
        {
            laserSpawner.StopSpawning();
            laserSpawner.ClearAllActiveLasers();
        }
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(gameOverRespawnDelay);

        if (playerObject != null && startPoint != null)
        {
            var rb = playerObject.GetComponent<Rigidbody>();
            var cc = playerObject.GetComponent<CharacterController>();

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

        if (playerHealth != null)
            playerHealth.ResetState();

        StartRound();
    }

    private void ResetBonusPanels()
    {
        if (bonusPanels == null || bonusPanels.Length == 0)
            bonusPanels = FindObjectsByType<BonusFloorPanel>(FindObjectsSortMode.None);

        foreach (var panel in bonusPanels)
        {
            if (panel != null)
                panel.ResetPanel();
        }
    }
}
