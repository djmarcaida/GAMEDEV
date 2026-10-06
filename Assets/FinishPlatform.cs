using UnityEngine;
using TMPro;

public class FinishPlatform : MonoBehaviour
{
    public TextMeshProUGUI finishText;

    private void OnTriggerEnter(Collider other)
    {
        if (finishText != null)
        {
            finishText.gameObject.SetActive(true);
        }
    }
}