using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Manages character switching between Milo and Lino.
/// When Lino is active, the entire world is tinted grey except Lino himself and Fireflies.
/// </summary>
public class CharacterSwitcher : MonoBehaviour
{
    [SerializeField] private PlayerMovementMilo _miloMovement;
    [SerializeField] private PlayerMovementLino _linoMovement;
    [SerializeField] private Collider2D _miloCollider;
    [SerializeField] private Collider2D _linoCollider;

    [Header("Visual — Milo")]
    [SerializeField] private SpriteRenderer _miloSprite;
    [SerializeField] private Color _miloSkyColor = new Color(0.53f, 0.81f, 0.92f);

    [Header("Visual — Lino")]
    [SerializeField] private SpriteRenderer _linoSprite;
    [SerializeField] private Color _linoActiveColor = new Color(1f, 0.85f, 0.1f, 1f);
    [SerializeField] private Color _linoSkyColor = new Color(0.30f, 0.30f, 0.30f, 1f);
    [SerializeField] private Color _worldGreyTint = new Color(0.35f, 0.35f, 0.35f, 1f);

    [SerializeField] private Camera _mainCamera;
    [SerializeField] private CinemachineCamera _cineFollowCam;

    [Header("Lino Exclusive Objects")]
    [SerializeField] private GameObject[] _linoOnlyObjects;
    [SerializeField] private Transform _miloTransform;
    [SerializeField] private float _detectionDistance = 3f;

    [Header("Controls")]
    [SerializeField] private bool _switchingEnabled = true;

    private bool _isPlayingMilo = true;
    private Rigidbody2D _miloRb;
    private Rigidbody2D _linoRb;
    private RigidbodyConstraints2D _miloOriginalConstraints;
    private RigidbodyConstraints2D _linoOriginalConstraints;

    // Maps each greyed SpriteRenderer to its original color for restoration.
    private readonly Dictionary<SpriteRenderer, Color> _originalColors = new();

    public bool IsPlayingMilo => _isPlayingMilo;

    private void Start()
    {
        if (_miloMovement != null)
        {
            _miloMovement.enabled = true;
            _miloRb = _miloMovement.GetComponent<Rigidbody2D>();
            _miloOriginalConstraints = _miloRb.constraints;
        }
        if (_linoMovement != null)
        {
            _linoMovement.enabled = false;
            _linoRb = _linoMovement.GetComponent<Rigidbody2D>();
            _linoOriginalConstraints = _linoRb.constraints;
            _linoRb.constraints = RigidbodyConstraints2D.FreezeAll;
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

    /// <summary>Forces a switch to Milo if Lino is currently active.</summary>
    public void ForceMilo()
    {
        if (_isPlayingMilo) return;
        SwitchCharacter();
    }

    /// <summary>Forces a switch to Lino if Milo is currently active.</summary>
    public void ForceLino()
    {
        if (!_isPlayingMilo) return;
        SwitchCharacter();
    }

    private void SwitchCharacter()
    {
        Debug.Log("Swith miaou");

        if (_miloMovement == null || _linoMovement == null)
        {
            Debug.Log("Milo or Lino is null");
        }

        _miloMovement.enabled = !_miloMovement.enabled;
        _linoMovement.enabled = !_linoMovement.enabled;

        _isPlayingMilo = !_isPlayingMilo;

        // Mettre à jour la cible de la caméra follow
        if (_cineFollowCam != null)
            _cineFollowCam.Follow = _isPlayingMilo ? _miloTransform : _linoMovement.transform;

        // Freeze the inactive character so they don't slide on slopes.
        Rigidbody2D nowActive = _isPlayingMilo ? _miloRb : _linoRb;
        Rigidbody2D nowInactive = _isPlayingMilo ? _linoRb : _miloRb;
        RigidbodyConstraints2D activeConstraints = _isPlayingMilo
            ? _miloOriginalConstraints
            : _linoOriginalConstraints;

        if (nowInactive != null)
        {
            nowInactive.linearVelocity = Vector2.zero;
            nowInactive.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        if (nowActive != null)
        {
            nowActive.constraints = activeConstraints;
        }
        UpdateColors();
    }

    private void UpdateColors()
    {
        if (_isPlayingMilo)
        {
            RestoreWorldColors();
            if (_miloSprite != null) _miloSprite.color = Color.white;
            if (_linoSprite != null) _linoSprite.color = Color.gray;
            if (_mainCamera != null) _mainCamera.backgroundColor = _miloSkyColor;
        }
        else
        {
            ApplyGreyToWorld();
            if (_miloSprite != null) _miloSprite.color = _worldGreyTint;
            if (_linoSprite != null) _linoSprite.color = _linoActiveColor;
            if (_mainCamera != null) _mainCamera.backgroundColor = _linoSkyColor;
        }

        // Milo hears poorly — apply low-pass filter when he is active.
        AudioManager.Instance?.SetMuffled(_isPlayingMilo);
    }

    // ── World grey effect ─────────────────────────────────────────────────────

    /// <summary>
    /// Greys all active SpriteRenderers in the scene, excluding Lino and Fireflies.
    /// Stores original colors for later restoration.
    /// </summary>
    private void ApplyGreyToWorld()
    {
        _originalColors.Clear();
        foreach (SpriteRenderer sr in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (IsExcluded(sr)) continue;
            _originalColors[sr] = sr.color;
            sr.color = _worldGreyTint;
        }
    }

    /// <summary>Restores all SpriteRenderers to their original colors.</summary>
    private void RestoreWorldColors()
    {
        foreach (var kvp in _originalColors)
        {
            if (kvp.Key != null)
                kvp.Key.color = kvp.Value;
        }
        _originalColors.Clear();
    }

    /// <summary>
    /// Applies grey tint to a newly activated object's renderers while Lino is active.
    /// </summary>
    private void GreyObject(GameObject obj)
    {
        foreach (SpriteRenderer sr in obj.GetComponentsInChildren<SpriteRenderer>())
        {
            if (IsExcluded(sr)) continue;
            if (!_originalColors.ContainsKey(sr))
                _originalColors[sr] = sr.color;
            sr.color = _worldGreyTint;
        }
    }

    /// <summary>
    /// Returns true for renderers that must keep their original color:
    /// Lino, Milo, and any Firefly.
    /// </summary>
    private bool IsExcluded(SpriteRenderer sr)
    {
        return sr == _linoSprite
            || sr == _miloSprite
            || sr.GetComponent<Firefly>() != null;
    }

    // ── Lino-only objects ─────────────────────────────────────────────────────

    private void UpdateLinoObjects()
    {
        if (_miloTransform == null) return;
        foreach (GameObject obj in _linoOnlyObjects)
        {
            if (obj == null) continue;
            float distance = Vector3.Distance(_miloTransform.position, obj.transform.position);
            bool shouldBeActive = !_isPlayingMilo || distance <= _detectionDistance;
            bool wasActive = obj.activeSelf;

            obj.SetActive(shouldBeActive);

            // Grey a newly activated object if Lino is currently playing.
            if (!_isPlayingMilo && shouldBeActive && !wasActive)
                GreyObject(obj);
        }
    }
}
