using UnityEngine;

/// <summary>
/// Place on any platform or zone to define the footstep sounds
/// played when a character walks on it.
/// </summary>
public class FootstepSurface : MonoBehaviour
{
    [SerializeField] private AudioClip[] _clips;

    /// <summary>Returns a random clip from the pool. Returns null if the pool is empty.</summary>
    public AudioClip GetRandom()
    {
        if (_clips == null || _clips.Length == 0) return null;
        return _clips[Random.Range(0, _clips.Length)];
    }
}
