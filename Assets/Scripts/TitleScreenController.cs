using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";

    public void OnStartPressed()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}