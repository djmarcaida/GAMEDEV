using System.Collections;
using UnityEngine;

public class VoidRespawn : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
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
            yield break;

        var cc = target.GetComponent<CharacterController>() ?? target.GetComponentInChildren<CharacterController>();
        var rb = target.GetComponent<Rigidbody>() ?? target.GetComponentInChildren<Rigidbody>();

        if (cc != null) cc.enabled = false;
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        target.transform.position = respawnPoint.position;
        target.transform.rotation = respawnPoint.rotation;

        yield return new WaitForFixedUpdate();

        if (rb != null)
        {
            rb.position = respawnPoint.position;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }

        if (cc != null)
            cc.enabled = true;
    }
}