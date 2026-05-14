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
        AudioManager.Instance?.PlayMusic(_musicClip);
    }
}
