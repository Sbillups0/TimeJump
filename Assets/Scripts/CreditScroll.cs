using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScroll : MonoBehaviour
{
    [SerializeField] private RectTransform creditsText;
    [SerializeField] private float scrollSpeed = 50f;     // pixels per second
    [SerializeField] private float endPositionY = 2500f;  // stop/transition point
    [SerializeField] private string nextScene = "TitleScreen";

    void Update()
    {
        // Move the text upward
        creditsText.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        if (creditsText.anchoredPosition.y >= endPositionY)
        {
            SceneManager.LoadScene(nextScene);
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene(nextScene);
        }
    }
}