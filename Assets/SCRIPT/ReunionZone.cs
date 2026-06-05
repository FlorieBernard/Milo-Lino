using System.Collections;
using UnityEngine;

/// <summary>
/// Zone trigger de fin de niveau : bloque le premier chat arrivé jusqu'à ce que l'autre arrive.
///
/// Comportement :
///   - Milo ou Lino peut arriver en premier (les deux cas sont gérés).
///   - Si le chat arrive en sautant, on attend qu'il atterrisse avant de continuer.
///   - Le message s'affiche et la caméra revient doucement à sa position initiale (blend Cinemachine).
///   - Une fois le blend terminé : switch sur la fixed cam, freeze le chat, switch sur l'autre.
///   - Quand le second chat entre, on libère le premier et Milo reprend toujours le contrôle.
///
/// Setup Inspector :
///   - CharacterSwitcher, LinoFollower, MiloRb, LinoRb
///   - MessageObject : GameObject UI avec le texte d'attente (désactivé par défaut)
///   - Collider2D en mode Trigger sur ce GameObject
///   - Tags "Milo" et "Lino" sur les personnages
///   - La durée du retour caméra se configure dans le CinemachineBrain (Default Blend Duration)
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
    [Tooltip("Durée d'affichage du message avant disparition automatique (secondes).")]
    [SerializeField] private float _messageHideDuration = 4f;

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

        // Afficher le message et lancer le retour caméra simultanément
        if (_messageObject != null)
        {
            _messageObject.SetActive(true);
            StartCoroutine(HideMessageAfterDelay());
        }
        CameraManager.Instance?.SetParallaxMode(true, false);

        // Attendre que le blend Cinemachine soit terminé
        yield return null; // une frame pour que IsBlending devienne true
        while (CameraManager.Instance != null && CameraManager.Instance.IsBlending)
            yield return null;

        // Figer le premier chat
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        // Désactiver LinoFollower pendant l'attente
        _linoFollower?.SetActive(false);

        // Switcher sur l'autre chat
        if (isMilo)
            _switcher?.ForceLino();
        else
            _switcher?.ForceMilo();
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(_messageHideDuration);
        if (_messageObject != null) _messageObject.SetActive(false);
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
