using UnityEngine;

/// <summary>
/// Animates a SpriteRenderer by cycling through a list of sprites.
/// Works on any 2D object: plants, trees, decorations, etc.
/// No Animator required — configure everything from the Inspector.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    public enum PlayMode { Loop, PingPong, Once }

    [Header("Frames")]
    [Tooltip("Sprites played in order (drag them from the Project window).")]
    [SerializeField] private Sprite[] _frames;

    [Header("Timing")]
    [Tooltip("Frames per second.")]
    [SerializeField] private float _fps = 8f;

    [Header("Playback")]
    [SerializeField] private PlayMode _playMode = PlayMode.Loop;
    [Tooltip("Start at a random frame so nearby plants don't all animate in sync.")]
    [SerializeField] private bool _randomOffset = true;
    [Tooltip("Play automatically on Start. Uncheck to trigger via script.")]
    [SerializeField] private bool _playOnStart = true;

    private SpriteRenderer _renderer;
    private float _timer;
    private int   _currentFrame;
    private int   _direction = 1;   // +1 forward, -1 backward (PingPong)
    private bool  _playing;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (_frames == null || _frames.Length == 0) return;

        if (_randomOffset)
            _currentFrame = Random.Range(0, _frames.Length);

        _renderer.sprite = _frames[_currentFrame];

        if (_playOnStart) Play();
    }

    private void Update()
    {
        if (!_playing || _frames == null || _frames.Length < 2) return;

        _timer += Time.deltaTime;
        float frameDuration = 1f / Mathf.Max(_fps, 0.01f);

        while (_timer >= frameDuration)
        {
            _timer -= frameDuration;
            AdvanceFrame();
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Starts or resumes playback.</summary>
    public void Play() => _playing = true;

    /// <summary>Pauses playback without resetting.</summary>
    public void Pause() => _playing = false;

    /// <summary>Stops playback and resets to the first frame.</summary>
    public void Stop()
    {
        _playing = false;
        _currentFrame = 0;
        _direction = 1;
        if (_frames != null && _frames.Length > 0)
            _renderer.sprite = _frames[0];
    }

    // ── Frame logic ───────────────────────────────────────────────────────────

    private void AdvanceFrame()
    {
        _currentFrame += _direction;

        switch (_playMode)
        {
            case PlayMode.Loop:
                if (_currentFrame >= _frames.Length)
                    _currentFrame = 0;
                break;

            case PlayMode.PingPong:
                if (_currentFrame >= _frames.Length - 1)
                {
                    _currentFrame = _frames.Length - 1;
                    _direction = -1;
                }
                else if (_currentFrame <= 0)
                {
                    _currentFrame = 0;
                    _direction = 1;
                }
                break;

            case PlayMode.Once:
                if (_currentFrame >= _frames.Length)
                {
                    _currentFrame = _frames.Length - 1;
                    _playing = false;
                }
                break;
        }

        _renderer.sprite = _frames[_currentFrame];
    }
}
