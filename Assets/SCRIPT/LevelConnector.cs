using UnityEngine;

/// <summary>
/// Place this component in each scene to define which scene comes next.
/// GameManager.LoadNextScene() reads this value at runtime.
/// </summary>
public class LevelConnector : MonoBehaviour
{
    [SerializeField] private string _nextScene;

    /// <summary>The name of the scene to load after this one.</summary>
    public string NextScene => _nextScene;
}
