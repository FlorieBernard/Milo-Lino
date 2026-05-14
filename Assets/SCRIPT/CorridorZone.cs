using UnityEngine;

/// <summary>
/// Place this on a trigger zone at the entrance of each corridor.
/// Ensures Milo is always the controlled cat when entering a corridor.
/// </summary>
public class CorridorZone : MonoBehaviour
{
    [SerializeField] private CharacterSwitcher _characterSwitcher;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Milo"))
            _characterSwitcher?.ForceMilo();
    }
}
