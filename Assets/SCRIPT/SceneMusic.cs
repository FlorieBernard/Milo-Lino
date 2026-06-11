using UnityEngine;

/// <summary>
/// Place this in any scene to define which music should play there.
/// The AudioManager handles the crossfade automatically.
/// If no clip is assigned, the current music will stop.
/// </summary>
public class SceneMusic : MonoBehaviour
{
    [SerializeField] private AudioClip _musicClip;

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[SceneMusic] No AudioManager in scene — start from the 'Debut' scene to hear music.", this);
            return;
        }

        if (_musicClip == null)
            Debug.LogWarning("[SceneMusic] No clip assigned — current music will stop.", this);

        AudioManager.Instance.PlayMusic(_musicClip);
    }
}
