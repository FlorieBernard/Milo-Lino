using UnityEngine;

/// <summary>
/// The firefly the cats must catch to open the passage.
/// Only one cat can see it (configured in Inspector).
/// The cat that can see it is the only one that can catch it.
/// On catch: destroys the obstacle and unblocks Lino.
/// </summary>
public class Firefly : MonoBehaviour
{
    public enum CatTarget { Milo, Lino }

    [Header("Visibility")]
    [SerializeField] private CatTarget _visibleTo = CatTarget.Milo;
    [SerializeField] private CharacterSwitcher _characterSwitcher;

    [Header("Float Movement")]
    [SerializeField] private float _floatSpeed = 1.5f;
    [SerializeField] private float _floatAmplitude = 0.25f;

    [Header("On Catch")]
    [SerializeField] private GameObject _obstacleToDestroy;
    [SerializeField] private LinoBlocker _linoBlocker;

    private SpriteRenderer _renderer;
    private Vector3 _startPosition;
    private bool _caught = false;

    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _startPosition = transform.position;
    }

    private void Update()
    {
        if (_caught) return;

        // Floating animation
        float yOffset = Mathf.Sin(Time.time * _floatSpeed) * _floatAmplitude;
        transform.position = _startPosition + new Vector3(0f, yOffset, 0f);

        // Show only to the correct cat
        if (_renderer != null && _characterSwitcher != null)
        {
            bool shouldBeVisible = _visibleTo == CatTarget.Milo
                ? _characterSwitcher.IsPlayingMilo
                : !_characterSwitcher.IsPlayingMilo;
            _renderer.enabled = shouldBeVisible;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_caught) return;

        bool miloTouched = other.CompareTag("Milo");
        bool linoTouched = other.CompareTag("Lino");

        bool canCatch = (_visibleTo == CatTarget.Milo && miloTouched)
                     || (_visibleTo == CatTarget.Lino && linoTouched);

        if (!canCatch) return;

        Catch();
    }

    private void Catch()
    {
        _caught = true;

        AudioManager.Instance?.Play("FireflyCatch");

        if (_obstacleToDestroy != null)
            Destroy(_obstacleToDestroy);

        if (_linoBlocker != null)
            _linoBlocker.Unblock();

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
