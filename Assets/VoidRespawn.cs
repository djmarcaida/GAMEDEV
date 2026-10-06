using System.Collections;
using UnityEngine;

public class VoidRespawn : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Void hit by: " + other.gameObject.name);

        // Find the player object
        GameObject playerObj = null;

        if (other.CompareTag("Player"))
        {
            playerObj = other.gameObject;
        }
        else if (other.transform.root.CompareTag("Player"))
        {
            playerObj = other.transform.root.gameObject;
        }
        else if (other.GetComponentInParent<CharacterController>() != null)
        {
            playerObj = other.GetComponentInParent<CharacterController>().gameObject;
        }
        else if (other.GetComponentInParent<Rigidbody>() != null)
        {
            playerObj = other.GetComponentInParent<Rigidbody>().gameObject;
        }

        if (playerObj != null)
        {
            StartCoroutine(DoTeleport(playerObj));
        }
    }

    private IEnumerator DoTeleport(GameObject target)
    {
        if (respawnPoint == null)
        {
            Debug.LogError("Respawn Point is not assigned in the Inspector!");
            yield break;
        }

        // 1. Handle CharacterController
        CharacterController cc = target.GetComponent<CharacterController>();
        if (cc == null) cc = target.GetComponentInChildren<CharacterController>();

        // 2. Handle Rigidbody
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb == null) rb = target.GetComponentInChildren<Rigidbody>();

        // Disable movement controllers during move
        if (cc != null) cc.enabled = false;
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Move the target position
        target.transform.position = respawnPoint.position;
        target.transform.rotation = respawnPoint.rotation;

        // Wait one physics step to ensure Unity registers the new coordinates
        yield return new WaitForFixedUpdate();

        // Restore velocity / state
        if (rb != null)
        {
            rb.position = respawnPoint.position;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }

        if (cc != null)
        {
            cc.enabled = true;
        }

        Debug.Log("Teleport finished to: " + respawnPoint.position);
    }
}