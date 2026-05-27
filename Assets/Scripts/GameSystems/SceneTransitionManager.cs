using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    private bool isTransitioning;

    public bool IsTransitioning => isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("SceneTransitionManager is missing a Fade Canvas Group reference.");
            return;
        }

        Image fadeImage = fadeCanvasGroup.GetComponent<Image>();

        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;
        }

        fadeCanvasGroup.alpha = 1f;
        StartCoroutine(FadeFromBlack());
    }

    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning)
            return;

        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;

        yield return FadeToBlack();

        SceneManager.LoadScene(sceneName);
    }

    public IEnumerator FadeToBlack()
    {
        if (fadeCanvasGroup == null)
            yield break;

        fadeCanvasGroup.blocksRaycasts = true;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    public IEnumerator FadeFromBlack()
    {
        if (fadeCanvasGroup == null)
            yield break;

        fadeCanvasGroup.blocksRaycasts = true;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }
}