using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent singleton that manages scene progression.
/// Place this GameObject in the "Debut" scene.
/// Each scene defines its successor via a <see cref="LevelConnector"/> component.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private string _firstScene = "Level1";

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

    /// <summary>Loads the next scene as defined by the current scene's LevelConnector.</summary>
    public void LoadNextScene()
    {
        LevelConnector connector = FindFirstObjectByType<LevelConnector>();
        if (connector == null)
        {
            Debug.LogWarning("GameManager: no LevelConnector found in current scene.");
            return;
        }
        LoadScene(connector.NextScene);
    }

    /// <summary>Resets progression and starts from the first scene.</summary>
    public void ResetAndStart()
    {
        LoadScene(_firstScene);
    }

    private void LoadScene(string sceneName)
    {
        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeToScene(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            LoadNextScene();
        }
    }
}
