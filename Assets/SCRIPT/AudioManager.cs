using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;

    [HideInInspector] public AudioSource source;
}

/// <summary>
/// Persistent singleton that manages all game sounds and music.
/// Place this GameObject in the "Debut" scene alongside GameManager.
///
/// — SFX: Add entries to _sounds in the Inspector, then call AudioManager.Instance?.Play("SoundName").
/// — Music: Place a SceneMusic component in each scene with the desired AudioClip.
///
/// Suggested SFX names: "Jump", "Land", "FireflyCatch"
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sound Effects")]
    [SerializeField] private Sound[] _sounds;

    [Header("Music")]
    [Range(0f, 1f)]
    [SerializeField] public float _musicVolume = 0.1f;
    [SerializeField] public float _fadeDuration = 1f;

    [Header("Hearing (Lino = muffled)")]
    [Tooltip("Low-pass cutoff when Lino is active. Lower = more muffled. ~800 Hz recommended.")]
    [SerializeField] public float _muffledCutoff = 800f;

    private AudioSource _musicSource;
    private AudioSource _oneShotSource;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Set up SFX sources
        foreach (Sound s in _sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        // Set up one-shot source for dynamic clips (e.g. footsteps)
        _oneShotSource = gameObject.AddComponent<AudioSource>();

        // Set up dedicated music source
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.volume = 0f;

        // Restore saved volumes
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", _musicVolume));
        SetSfxVolume(PlayerPrefs.GetFloat("SfxVolume", 1f));
    }

    // ── Volume control ────────────────────────────────────────────────────────

    /// <summary>Sets music volume [0-1] and persists to PlayerPrefs.</summary>
    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        _musicSource.volume = _musicVolume;
        PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
    }

    /// <summary>Sets SFX volume [0-1] for all sound effects and persists to PlayerPrefs.</summary>
    public void SetSfxVolume(float volume)
    {
        float v = Mathf.Clamp01(volume);
        foreach (Sound s in _sounds)
        {
            if (s.source != null)
                s.source.volume = s.volume * v;
        }
        PlayerPrefs.SetFloat("SfxVolume", v);
    }

    /// <summary>Current music volume [0-1].</summary>
    public float MusicVolume => _musicVolume;

    /// <summary>Current SFX volume [0-1].</summary>
    public float SfxVolume => PlayerPrefs.GetFloat("SfxVolume", 1f);

    /// <summary>
    /// Applies or removes a low-pass filter on the AudioListener to simulate muffled hearing.
    /// Call with true when Lino is active, false when Milo is active.
    /// </summary>
    public void SetMuffled(bool muffled)
    {
        AudioListener listener = FindFirstObjectByType<AudioListener>();
        if (listener == null) return;

        var filter = listener.GetComponent<AudioLowPassFilter>()
            ?? listener.gameObject.AddComponent<AudioLowPassFilter>();
        filter.cutoffFrequency = muffled ? _muffledCutoff : 22000f;
    }

    // ── SFX ─────────────────────────────────────────────────────────────────

    public void Play(string soundName)
    {
        Sound s = Array.Find(_sounds, sound => sound.name == soundName);
        if (s?.source == null) return;
        s.source.Play();
    }

    public void Stop(string soundName)
    {
        Sound s = Array.Find(_sounds, sound => sound.name == soundName);
        if (s?.source == null) return;
        s.source.Stop();
    }

    public void StopAll()
    {
        foreach (Sound s in _sounds)
            s.source?.Stop();
    }

    /// <summary>
    /// Plays an arbitrary clip once without interrupting other sounds.
    /// Respects the current SFX volume setting.
    /// </summary>
    public void PlayOneShot(AudioClip clip)
    {
        if (clip == null || _oneShotSource == null) return;
        _oneShotSource.PlayOneShot(clip, SfxVolume);
    }

    // ── Music ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Plays a music clip with a fade-in. If the same clip is already playing, does nothing.
    /// If clip is null, stops the current music.
    /// </summary>
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            StopMusic();
            return;
        }

        // Already playing this clip — don't restart it
        if (_musicSource.clip == clip && _musicSource.isPlaying)
            return;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(CrossfadeToClip(clip));
    }

    public void StopMusic()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeOut());
    }

    private IEnumerator CrossfadeToClip(AudioClip clip)
    {
        // Fade out current track
        yield return StartCoroutine(FadeOut());

        // Switch and fade in new track
        _musicSource.clip = clip;
        _musicSource.Play();
        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator FadeOut()
    {
        if (!_musicSource.isPlaying)
        {
            _musicSource.volume = 0f;
            yield break;
        }

        float startVolume = _musicSource.volume;
        for (float t = 0; t < _fadeDuration; t += Time.unscaledDeltaTime)
        {
            _musicSource.volume = Mathf.Lerp(startVolume, 0f, t / _fadeDuration);
            yield return null;
        }
        _musicSource.volume = 0f;
        _musicSource.Stop();
    }

    private IEnumerator FadeIn()
    {
        for (float t = 0; t < _fadeDuration; t += Time.unscaledDeltaTime)
        {
            _musicSource.volume = Mathf.Lerp(0f, _musicVolume, t / _fadeDuration);
            yield return null;
        }
        _musicSource.volume = _musicVolume;
    }
}
