using UnityEngine;
using UnityEngine.UI;

public class StarCollector : MonoBehaviour
{
    public Text gameOverText;

    private bool collected = false;

    void Start()
    {
        if (gameOverText != null)
        {
            GameMessageUI.RegisterText(gameOverText);
        }

        if (GameSessionManager.Instance != null && !GameSessionManager.Instance.IsGameplayActive)
        {
            GameMessageUI.ClearMessage();
        }
    }

    public static void ResetStarCounter()
    {
        GameMessageUI.ClearMessage();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
        {
            return;
        }

        if (other.CompareTag("Player") || other.transform.root.name == "Robot")
        {
            collected = true;

            if (GameSessionManager.Instance != null)
            {
                GameSessionManager.Instance.RegisterStarCollected();
            }

            Destroy(gameObject);
        }
    }
}