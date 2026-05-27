using UnityEngine;

public class LevelConnector : MonoBehaviour
{
    [Header("Scene Connection")]
    [SerializeField] private string nextSceneName;

    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player";

    private bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered)
            return;

        if (!other.CompareTag(playerTag))
            return;

        hasTriggered = true;

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(nextSceneName);
        }
        else
        {
            Debug.LogError("No SceneTransitionManager found in the scene.");
        }
    }
}