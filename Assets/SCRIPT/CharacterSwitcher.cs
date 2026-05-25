using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    [SerializeField] private PlayerMovementMilo _miloMovement;
    [SerializeField] private PlayerMovementLino _linoMovement;
    [SerializeField] private Collider2D _miloCollider;
    [SerializeField] private Collider2D _linoCollider;

    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer _miloSprite;
    [SerializeField] private SpriteRenderer _linoSprite;
    [SerializeField] private Camera _mainCamera;

    [Header("Lino Exclusive Objects")]
    [SerializeField] private GameObject[] _linoOnlyObjects;
    [SerializeField] private Transform _miloTransform;
    [SerializeField] private float _detectionDistance = 3f;

    [Header("Controls")]
    [SerializeField] private bool _switchingEnabled = true;

    private static readonly Color SkyBlue = new Color(0.53f, 0.81f, 0.92f);
    private static readonly Color DimGray = new Color(0.5f, 0.5f, 0.5f);

    private bool _isPlayingMilo = true;
    private Rigidbody2D _miloRb;
    private Rigidbody2D _linoRb;

    public bool IsPlayingMilo => _isPlayingMilo;

    private void Start()
    {
        if (_miloMovement != null)
        {
            _miloMovement.enabled = true;
            _miloRb = _miloMovement.GetComponent<Rigidbody2D>();
        }
        if (_linoMovement != null)
        {
            _linoMovement.enabled = false;
            _linoRb = _linoMovement.GetComponent<Rigidbody2D>();
        }
        if (_miloCollider != null && _linoCollider != null)
            Physics2D.IgnoreCollision(_miloCollider, _linoCollider, true);

        if (_miloTransform == null && _miloMovement != null)
            _miloTransform = _miloMovement.transform;

        UpdateColors();
        UpdateLinoObjects();
    }

    private void Update()
    {
        if (_switchingEnabled && Input.GetKeyDown(KeyCode.Tab))
            SwitchCharacter();

        UpdateLinoObjects();
    }

    public void ForceMilo()
    {
        if (_isPlayingMilo) return;
        SwitchCharacter();
    }

    private void SwitchCharacter()
    {
        if (_miloMovement == null || _linoMovement == null) return;

        _miloMovement.enabled = !_miloMovement.enabled;
        _linoMovement.enabled = !_linoMovement.enabled;

        if (_miloRb != null) _miloRb.linearVelocity = new Vector2(0, _miloRb.linearVelocity.y);
        if (_linoRb != null) _linoRb.linearVelocity = new Vector2(0, _linoRb.linearVelocity.y);

        _isPlayingMilo = !_isPlayingMilo;

        UpdateColors();
    }

    private void UpdateColors()
    {
        if (_miloSprite != null) _miloSprite.color = _isPlayingMilo ? Color.white : Color.gray;
        if (_linoSprite != null) _linoSprite.color = _isPlayingMilo ? Color.gray : Color.white;
        if (_mainCamera != null) _mainCamera.backgroundColor = _isPlayingMilo ? SkyBlue : DimGray;
    }

    private void UpdateLinoObjects()// only object visible
    {
        if (_miloTransform == null) return;
        foreach (GameObject obj in _linoOnlyObjects)
        {
            if (obj == null) continue;
            float distance = Vector3.Distance(_miloTransform.position, obj.transform.position); 
            obj.SetActive(!_isPlayingMilo || distance <= _detectionDistance);
        }
    }
}
