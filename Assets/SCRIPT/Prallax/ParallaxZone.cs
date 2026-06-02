using UnityEngine;

/// <summary>
/// Place two of these at the boundaries of a parallax section:
///   • Start zone → enables parallax camera + disables respawn
///   • End   zone → disables parallax camera + re-enables respawn
///
/// Setup per zone:
///   1. GameObject vide avec Collider2D → Is Trigger ✅, taille = largeur du passage
///   2. Attacher ce script, choisir le mode (Start / End)
///
/// Note: no deaths in parallax zones — RespawnOnFall is disabled while inside.
/// </summary>
public class ParallaxZone : MonoBehaviour
{
    //public enum ZoneMode { Start, End }

    //[Tooltip("Start = enter parallax area. End = leave parallax area.")]
    //[SerializeField] private ZoneMode _mode = ZoneMode.Start;

    //private void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (!IsPlayer(other)) return;

    //    bool entering = _mode == ZoneMode.Start;
    //    CameraManager.Instance?.SetParallaxMode(entering);
    //    SetRespawnEnabled(!entering);
    //}

    //private static bool IsPlayer(Collider2D col)
    //    => col.CompareTag("Milo") || col.CompareTag("Lino");

    //private static void SetRespawnEnabled(bool enabled)
    //{
    //    foreach (var r in FindObjectsByType<RespawnOnFall>(FindObjectsSortMode.None))
    //        r.enabled = enabled;
    //}
}
