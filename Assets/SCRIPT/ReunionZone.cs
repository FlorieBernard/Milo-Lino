using System.Collections;
using UnityEngine;

/// <summary>
/// Zone trigger de fin de niveau : bloque le premier chat arrivé jusqu'à ce que l'autre arrive.
///
/// Comportement :
///   - Milo ou Lino peut arriver en premier (les deux cas sont gérés).
///   - Si le chat arrive en sautant, on attend qu'il atterrisse avant de le figer.
///   - Un message s'affiche à l'écran pour prévenir le joueur.
///   - Après _switchDelay secondes, le chat est figé et on switch sur l'autre.
///   - Quand le second chat entre, on libère le premier et Milo reprend toujours le contrôle.
///
/// Setup Inspector :
///   - CharacterSwitcher : le composant CharacterSwitcher de la scène
///   - LinoFollower : le composant LinoFollower sur Lino
///   - MiloRb / LinoRb : Rigidbody2D des deux chats
///   - MessageObject : GameObject UI avec le texte d'attente (désactivé par défaut)
///   - SwitchDelay : délai (secondes) après atterrissage avant de figer et switcher
///   - Collider2D en mode Trigger sur ce GameObject
///   - Tags "Milo" et "Lino" sur les personnages
/// </summary>
public class ReunionZone : MonoBehaviour
{
    [SerializeField] private CharacterSwitcher _switcher;
    [SerializeField] private LinoFollower _linoFollower;
    [SerializeField] private Rigidbody2D _miloRb;
    [SerializeField] private Rigidbody2D _linoRb;

    [Header("UI")]
    [Tooltip("GameObject contenant le texte d'attente. Désactivé par défaut dans la scène.")]
    [SerializeField] private GameObject _messageObject;

    [Header("Timing")]
    [Tooltip("Délai (secondes) après atterrissage avant de figer le chat et switcher.")]
    [SerializeField] private float _switchDelay = 1.5f;

    private enum Phase { Idle, MiloWaiting, LinoWaiting }
    private Phase _phase = Phase.Idle;

    private RigidbodyConstraints2D _miloOriginalConstraints;
    private RigidbodyConstraints2D _linoOriginalConstraints;

    private void Awake()
    {
        if (_miloRb != null) _miloOriginalConstraints = _miloRb.constraints;
        if (_linoRb != null) _linoOriginalConstraints = _linoRb.constraints;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_phase == Phase.Idle)
        {
            if (other.CompareTag("Milo"))
            {
                _phase = Phase.MiloWaiting;
                StartCoroutine(HandleFirstCat(_miloRb, isMilo: true));
            }
            else if (other.CompareTag("Lino"))
            {
                _phase = Phase.LinoWaiting;
                StartCoroutine(HandleFirstCat(_linoRb, isMilo: false));
            }
        }
        else if (_phase == Phase.MiloWaiting && other.CompareTag("Lino"))
            OnSecondCatEntered();
        else if (_phase == Phase.LinoWaiting && other.CompareTag("Milo"))
            OnSecondCatEntered();
    }

    private IEnumerator HandleFirstCat(Rigidbody2D rb, bool isMilo)
    {
        // Attendre l'atterrissage si le chat est en l'air
        while (rb != null && Mathf.Abs(rb.linearVelocity.y) > 0.1f)
            yield return null;

        // Afficher le message d'attente
        if (_messageObject != null) _messageObject.SetActive(true);

        // Délai avant de figer et switcher
        yield return new WaitForSeconds(_switchDelay);

        // Figer le premier chat
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // Désactiver LinoFollower pendant l'attente
        _linoFollower?.SetActive(false);

        // Switcher sur l'autre chat (symétrique : toujours basculer sur celui qui n'est pas figé)
        if (isMilo)
            _switcher?.ForceLino();
        else
            _switcher?.ForceMilo();
    }

    private void OnSecondCatEntered()
    {
        StopAllCoroutines();

        // Libérer le chat qui attendait
        if (_phase == Phase.MiloWaiting && _miloRb != null)
            _miloRb.constraints = _miloOriginalConstraints;
        else if (_phase == Phase.LinoWaiting && _linoRb != null)
            _linoRb.constraints = _linoOriginalConstraints;

        _linoFollower?.SetActive(true);
        _switcher?.ForceMilo();

        if (_messageObject != null) _messageObject.SetActive(false);

        gameObject.SetActive(false);
    }
}
