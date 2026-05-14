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
    [SerializeField] private float _musicVolume = 0.8f;
    [SerializeField] private float _fadeDuration = 1f;

    private AudioSource _musicSource;
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

        // Set up dedicated music source
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.volume = 0f;
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
