using UnityEngine;

public class SignTrigger : MonoBehaviour
{
    [SerializeField] private TutorialBox tutorialBox;
    [SerializeField] private bool triggerOnce = true;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip signSound;

    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (triggerOnce && hasTriggered) 
                return;

            hasTriggered = true;

            if (audioSource != null && signSound != null)
            {
                audioSource.PlayOneShot(signSound);
            }

            tutorialBox.Show();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            tutorialBox.Hide();
        }
    }
}