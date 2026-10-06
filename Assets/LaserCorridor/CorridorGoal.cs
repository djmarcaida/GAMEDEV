using UnityEngine;

public class CorridorGoal : MonoBehaviour
{
    private bool hasTriggered;

    private void Start()
    {
        if (TryGetComponent<Collider>(out var col))
            col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.TryGetComponent<PlayerHealth>(out _) ||
            (other.transform.parent != null && other.transform.parent.TryGetComponent<PlayerHealth>(out _)))
        {
            hasTriggered = true;
            if (LaserCorridorGameManager.Instance != null)
                LaserCorridorGameManager.Instance.OnGoalReached();
        }
    }

    public void ResetGoal()
    {
        hasTriggered = false;
    }
}
