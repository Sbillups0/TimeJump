using UnityEngine;

public class SignTrigger : MonoBehaviour
{
    [SerializeField] private TutorialBox tutorialBox;
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (triggerOnce && hasTriggered) return;
            hasTriggered = true;
            tutorialBox.Show();
        }
    }
}