using UnityEngine;

public class AttachPlayer : MonoBehaviour {

    public GameObject Player;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered by: " + other.gameObject.name);
        if (other.gameObject == Player)
        {
            Debug.Log("Player matched!");
            Player.transform.parent = transform;
        }
    }

    private void OnTriggerExit (Collider other)
    {
        if (other.gameObject == Player)
        {
            Player.transform.parent = null;
        }
    }
}
