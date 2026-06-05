using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attacher sur Lino. Lino suit la position que Milo occupait il y a _followDelay secondes.
/// Activable via SetActive(bool) ou par trigger tagué "Corridor".
/// </summary>
public class LinoFollower : MonoBehaviour
{
    [Header("Référence")]
    [SerializeField] private Transform _milo;

    [Header("Paramètres")]
    [SerializeField] private float _followDelay = 0.5f;
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _minDistance = 0.5f;

    public bool IsActive { get; private set; }

    private Rigidbody2D _linoRb;
    private Rigidbody2D _miloRb;
    private readonly List<(Vector3 pos, float time)> _history = new();

    private void Start()
    {
        _linoRb = GetComponent<Rigidbody2D>();
        if (_milo != null)
            _miloRb = _milo.GetComponent<Rigidbody2D>();
    }

    /// <summary>Active ou désactive le suivi. La désactivation vide l'historique.</summary>
    public void SetActive(bool active)
    {
        IsActive = active;
        if (!active) _history.Clear();
    }

    private void FixedUpdate()
    {
        if (_milo == null) return;

        // Enregistrer chaque frame (buffer max = _followDelay + 1s)
        _history.Add((_milo.position, Time.time));
        while (_history.Count > 0 && Time.time - _history[0].time > _followDelay + 1f)
            _history.RemoveAt(0);

        if (!IsActive || _history.Count == 0) return;

        // Trouver la position de Milo il y a ~_followDelay secondes
        Vector3 targetPos = _history[0].pos;
        for (int i = _history.Count - 1; i >= 0; i--)
        {
            if (Time.time - _history[i].time >= _followDelay)
            {
                targetPos = _history[i].pos;
                break;
            }
        }

        // Garder le Y de Lino quand Milo est au sol (Lino gère sa propre gravité)
        bool miloAirborne = _miloRb != null && Mathf.Abs(_miloRb.linearVelocity.y) > 0.1f;
        if (!miloAirborne)
            targetPos.y = transform.position.y;
        targetPos.z = transform.position.z;

        float dist = Vector2.Distance(transform.position, targetPos);
        if (dist <= _minDistance) return;

        Vector2 newPos = Vector2.MoveTowards(transform.position, targetPos, _speed * Time.fixedDeltaTime);
        _linoRb.MovePosition(newPos);

        float scaleX = targetPos.x < transform.position.x
            ? -Mathf.Abs(transform.localScale.x)
            : Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(scaleX, transform.localScale.y, 1f);
    }

    // Compatibilité avec l'ancien système Corridor
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Corridor")) SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Corridor")) SetActive(false);
    }
}
