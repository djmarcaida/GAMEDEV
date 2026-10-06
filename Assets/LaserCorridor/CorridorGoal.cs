using UnityEngine;

/// <summary>
/// Trigger placed at the end of the corridor. Reaching this trigger completes the level.
/// </summary>
public class CorridorGoal : MonoBehaviour
{
    [SerializeField] private bool hasTriggered = false;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            hasTriggered = true;
            Debug.Log("[CorridorGoal] PLAYER REACHED THE END OF THE CORRIDOR!");

            if (LaserCorridorGameManager.Instance != null)
            {
                LaserCorridorGameManager.Instance.OnGoalReached();
            }
        }
    }

    public void ResetGoal()
    {
        hasTriggered = false;
    }
}
