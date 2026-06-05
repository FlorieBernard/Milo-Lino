using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Options menu controller. Attach to the root of your Options panel.
///
/// Inspector wiring required:
///   _musicSlider       → Slider (Min 0, Max 1), OnValueChanged → OnMusicVolumeChanged
///   _sfxSlider         → Slider (Min 0, Max 1), OnValueChanged → OnSfxVolumeChanged
///   _languageDropdown  → TMP_Dropdown, OnValueChanged → OnLanguageChanged
///                        Options must match _languageCodes order (e.g. "Français", "English")
///   _panel             → this panel's root GameObject
///
/// _languageCodes must match CSV column names exactly (e.g. "fr", "en").
/// </summary>
public class OptionsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("Language")]
    [SerializeField] private TMP_Dropdown _languageDropdown;
    [Tooltip("Must match CSV header column names, in the same order as Dropdown options.")]
    [SerializeField] private string[] _languageCodes = { "fr", "en" };

    [Header("Panel")]
    [SerializeField] private GameObject _panel;

    private void OnEnable()
    {
        if (AudioManager.Instance != null)
        {
            if (_musicSlider != null) _musicSlider.value = AudioManager.Instance.MusicVolume;
            if (_sfxSlider != null) _sfxSlider.value = AudioManager.Instance.SfxVolume;
        }

        if (LocalizationManager.Instance != null && _languageDropdown != null)
        {
            int idx = Array.IndexOf(_languageCodes, LocalizationManager.Instance.CurrentLanguage);
            if (idx >= 0) _languageDropdown.value = idx;
        }
    }

    public void Show()
    {
        if (_panel != null) _panel.SetActive(true);
    }

    public void Hide()
    {
        if (_panel != null) _panel.SetActive(false);
    }

    // ── Inspector callbacks ───────────────────────────────────────────────────

    public void OnMusicVolumeChanged(float value)
        => AudioManager.Instance?.SetMusicVolume(value);

    public void OnSfxVolumeChanged(float value)
        => AudioManager.Instance?.SetSfxVolume(value);

    public void OnLanguageChanged(int index)
    {
        if (index < 0 || index >= _languageCodes.Length) return;
        LocalizationManager.Instance?.SetLanguage(_languageCodes[index]);
    }
}
