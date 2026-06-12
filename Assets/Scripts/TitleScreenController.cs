using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Beanball Tutorial";

    public void OnStartPressed()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}