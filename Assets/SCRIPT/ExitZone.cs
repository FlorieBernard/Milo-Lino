using UnityEngine;

/// <summary>
/// Place this on a trigger zone at the exit of each level or corridor.
/// When Milo reaches it, the next scene is loaded.
/// </summary>
public class ExitZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Milo"))
            GameManager.Instance?.LoadNextScene();
    }
}
