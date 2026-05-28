using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent singleton that manages scene progression.
/// Place this GameObject in the "Debut" scene.
/// Scene order is configured in the Inspector.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private string[] _sceneOrder =
    {
        "Debut",
        "Level1",
        "Level2",
        "Level3",
        "Fin"
    };

    private int _currentIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadNextScene()
    {
        _currentIndex++;
        if (_currentIndex < _sceneOrder.Length)
            LoadScene(_sceneOrder[_currentIndex]);
    }

    public void ResetAndStart()
    {
        _currentIndex = 0;
        LoadScene(_sceneOrder[_currentIndex]);
    }

    private void LoadScene(string sceneName)
    {
        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeToScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }
}
