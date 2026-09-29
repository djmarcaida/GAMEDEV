using UnityEngine;
using TMPro;

public class FinishPlatform : MonoBehaviour
{
    public TextMeshProUGUI finishText;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(">>> TOUCHED BY: " + other.gameObject.name);

        if (finishText != null)
        {
            finishText.gameObject.SetActive(true);
            Debug.Log(">>> TEXT ACTIVATED!");
        }
        else
        {
            Debug.LogError(">>> ERROR: finishText slot is EMPTY in Inspector!");
        }
    }
}