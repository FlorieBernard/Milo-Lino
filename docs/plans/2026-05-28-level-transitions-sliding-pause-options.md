# Level Transitions / Anti-Slide / Pause / Options — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Fix level transitions (remove corridors), stop cats sliding on slopes, fix pause menu toggle, add Options menu with volume + language.

**Architecture:** All changes are isolated script-level. New singletons (LocalizationManager) follow the existing DontDestroyOnLoad pattern from GameManager/AudioManager. DialogueZone gets a fallback: if no LocalizationManager, use raw `_lines` text; if present, look up by key.

**Tech Stack:** Unity 2D, C#, TextMeshPro, PlayerPrefs, Resources folder (CSV), UnityEngine.UI (Slider, Dropdown)

---

## Task 1: Fix level transitions — remove corridors

**Files:**
- Modify: `Assets/SCRIPT/GameManager.cs`
- Delete: `Assets/SCRIPT/CorridorZone.cs` + `Assets/SCRIPT/CorridorZone.cs.meta`

**Step 1: Update GameManager default scene order**

In `GameManager.cs`, replace the `_sceneOrder` default value:

```csharp
[SerializeField] private string[] _sceneOrder =
{
    "Debut",
    "Level1",
    "Level2",
    "Level3",
    "Fin"
};
```

**Step 2: Delete CorridorZone**

```bash
rm Assets/SCRIPT/CorridorZone.cs
rm Assets/SCRIPT/CorridorZone.cs.meta
```

**Step 3: Verify in Unity Editor**

- Open Unity. No compile errors expected.
- In the Debut scene, select the GameManager GameObject → Inspector should show the updated scene order (without corridors).
- If the Inspector still shows old values, clear the array and re-enter manually.

**Step 4: Commit**

```bash
git add Assets/SCRIPT/GameManager.cs
git add -u Assets/SCRIPT/CorridorZone.cs Assets/SCRIPT/CorridorZone.cs.meta
git commit -m "fix: remove corridor scenes from level progression"
```

---

## Task 2: Fix cat sliding on inclined surfaces

**Files:**
- Modify: `Assets/SCRIPT/PlayerMovementBase.cs`

**Context:** The Rigidbody2D has no friction on the character collider, so Unity physics causes the cat to slide down slopes. The fix: when grounded, not moving, and not on ice → force `velocity.x = 0` to counteract slope gravity.

**Step 1: Edit FixedUpdate in PlayerMovementBase.cs**

Find the `FixedUpdate` method (currently around line 50) and replace it:

```csharp
protected virtual void FixedUpdate()
{
    float targetSpeed = Horizontal * speed;

    if (_isOnIce)
    {
        _currentHorizontalSpeed = Mathf.Lerp(_currentHorizontalSpeed, targetSpeed, _iceAcceleration * Time.fixedDeltaTime);
    }
    else
    {
        _currentHorizontalSpeed = targetSpeed;

        // Prevent sliding on inclined surfaces when the player is not moving.
        if (IsGrounded() && Horizontal == 0f)
            _currentHorizontalSpeed = 0f;
    }

    _rb.linearVelocity = new Vector2(_currentHorizontalSpeed, _rb.linearVelocity.y);
}
```

Note: the `if (IsGrounded() && Horizontal == 0f)` block is redundant with `_currentHorizontalSpeed = targetSpeed` when `Horizontal == 0`, but it makes intent explicit and ensures no floating-point drift.

Actually, simplify to:

```csharp
protected virtual void FixedUpdate()
{
    float targetSpeed = Horizontal * speed;

    if (_isOnIce)
        _currentHorizontalSpeed = Mathf.Lerp(_currentHorizontalSpeed, targetSpeed, _iceAcceleration * Time.fixedDeltaTime);
    else
        _currentHorizontalSpeed = targetSpeed;

    _rb.linearVelocity = new Vector2(_currentHorizontalSpeed, _rb.linearVelocity.y);
}
```

Wait — this already sets `velocity.x = 0` when `Horizontal == 0`. The actual sliding issue is that the **Rigidbody2D itself** applies physics on the slope (Y axis), which causes the character to slide DOWN in the Y direction. Unity physics resolves this through friction between the collider and the surface.

**Real fix:** The character collider needs `angularDrag` and a **PhysicsMaterial2D** with `friction > 0`, OR we constrain the rotation and use a `PhysicsMaterial2D` with high friction on the character only — BUT this conflicts with ice.

**Correct code fix:** Override slope sliding by projecting velocity when grounded on a slope. Simplest approach: freeze Y velocity when grounded and not jumping.

Replace the full `FixedUpdate` with:

```csharp
protected virtual void FixedUpdate()
{
    float targetSpeed = Horizontal * speed;

    if (_isOnIce)
        _currentHorizontalSpeed = Mathf.Lerp(_currentHorizontalSpeed, targetSpeed, _iceAcceleration * Time.fixedDeltaTime);
    else
        _currentHorizontalSpeed = targetSpeed;

    // On non-ice ground with no input: prevent slope sliding by zeroing Y velocity
    float verticalVelocity = _rb.linearVelocity.y;
    if (!_isOnIce && IsGrounded() && Horizontal == 0f && verticalVelocity < 0f)
        verticalVelocity = 0f;

    _rb.linearVelocity = new Vector2(_currentHorizontalSpeed, verticalVelocity);
}
```

This only zeroes downward Y velocity (`< 0f`) when grounded + stationary + not on ice. Upward velocity (jumps) is unaffected.

**Step 2: Test in Unity Editor**

- Run Play mode.
- Walk Milo onto a tree trunk or inclined platform, release movement.
- Cat should stay in place instead of sliding.
- Walk onto an ice tile (tagged "Ice") → sliding should still work.

**Step 3: Commit**

```bash
git add Assets/SCRIPT/PlayerMovementBase.cs
git commit -m "fix: prevent cat sliding on inclined non-ice surfaces"
```

---

## Task 3: Fix pause menu toggle (Escape closes AND opens)

**Files:**
- Modify: `Assets/SCRIPT/MenuPause.cs`

**Step 1: Replace MenuPause.cs content**

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pause menu controller. Attach to a persistent GameObject in each game scene.
/// Requires: _container assigned in Inspector (the pause panel root).
/// Buttons in the Canvas must call ResumeButton(), MainMenuButton(), QuitGame().
/// </summary>
public class MenuPause : MonoBehaviour
{
    [SerializeField] private GameObject _container;

    private bool _isPaused = false;

    private void Start()
    {
        _container.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused) Resume();
            else Pause();
        }
    }

    private void Pause()
    {
        _isPaused = true;
        _container.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Resume()
    {
        _isPaused = false;
        _container.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ResumeButton() => Resume();

    public void MainMenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame() => Application.Quit();
}
```

**Step 2: Test in Unity Editor**

- Press Escape → pause menu appears, game freezes.
- Press Escape again → menu disappears, game resumes.
- Click Resume button → same result.
- Click Main Menu → goes to Menu scene.

**Step 3: Commit**

```bash
git add Assets/SCRIPT/MenuPause.cs
git commit -m "fix: pause menu Escape toggle, extract Resume/Pause methods"
```

---

## Task 4: AudioManager — expose volume controls

**Files:**
- Modify: `Assets/SCRIPT/AudioManager.cs`

**Step 1: Add volume methods and PlayerPrefs loading**

Add the following to `AudioManager.cs` :

In `Awake()`, after setting up sources, load saved volumes:

```csharp
// Load saved volumes
float savedMusic = PlayerPrefs.GetFloat("MusicVolume", _musicVolume);
float savedSfx   = PlayerPrefs.GetFloat("SfxVolume", 1f);
SetMusicVolume(savedMusic);
SetSfxVolume(savedSfx);
```

Add these two public methods:

```csharp
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

/// <summary>Current SFX volume [0-1] (as set by SetSfxVolume).</summary>
public float SfxVolume => PlayerPrefs.GetFloat("SfxVolume", 1f);
```

**Step 2: Verify compilation in Unity**

No Play test needed — just confirm zero compile errors in the Console.

**Step 3: Commit**

```bash
git add Assets/SCRIPT/AudioManager.cs
git commit -m "feat: add SetMusicVolume/SetSfxVolume with PlayerPrefs persistence"
```

---

## Task 5: LocalizationManager — CSV-based localization singleton

**Files:**
- Create: `Assets/SCRIPT/LocalizationManager.cs`
- Create: `Assets/Resources/Localization/dialogues.csv` (sample file)

**Step 1: Create LocalizationManager.cs**

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent singleton that loads and serves localized strings from CSV files.
/// CSV format: first column = key, subsequent columns = language values.
/// Header row: key,fr,en  (add more languages by adding columns)
///
/// Place this GameObject in the "Debut" scene.
/// CSV files go in: Assets/Resources/Localization/{filename}.csv
///
/// Usage:
///   LocalizationManager.Instance.Get("my_key")  → returns string in current language
///   LocalizationManager.Instance.SetLanguage("en")
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    /// <summary>Fired whenever the language changes. UI components should subscribe.</summary>
    public static event Action OnLanguageChanged;

    [SerializeField] private string _defaultLanguage = "fr";
    [SerializeField] private string[] _csvFiles = { "Localization/dialogues" };

    private string _currentLanguage;
    private readonly Dictionary<string, string> _strings = new();

    public string CurrentLanguage => _currentLanguage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        string saved = PlayerPrefs.GetString("Language", _defaultLanguage);
        SetLanguage(saved);
    }

    /// <summary>Switches language and reloads all CSV files.</summary>
    public void SetLanguage(string langCode)
    {
        _currentLanguage = langCode;
        _strings.Clear();

        foreach (string file in _csvFiles)
            LoadCsv(file);

        PlayerPrefs.SetString("Language", langCode);
        OnLanguageChanged?.Invoke();
    }

    /// <summary>Returns the localized string for the given key. Falls back to the key itself if not found.</summary>
    public string Get(string key)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;
        return _strings.TryGetValue(key, out string value) ? value : key;
    }

    // ── CSV Parsing ───────────────────────────────────────────────────────────

    private void LoadCsv(string resourcePath)
    {
        TextAsset asset = Resources.Load<TextAsset>(resourcePath);
        if (asset == null)
        {
            Debug.LogWarning($"[LocalizationManager] CSV not found: Resources/{resourcePath}.csv");
            return;
        }

        string[] lines = asset.text.Split('\n');
        if (lines.Length < 2) return;

        // Parse header to find column index for current language
        string[] header = SplitCsvLine(lines[0]);
        int langIndex = Array.IndexOf(header, _currentLanguage);
        if (langIndex < 0)
        {
            Debug.LogWarning($"[LocalizationManager] Language '{_currentLanguage}' not found in {resourcePath}.csv");
            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = SplitCsvLine(line);
            if (cols.Length <= langIndex) continue;

            string key   = cols[0].Trim();
            string value = cols[langIndex].Trim();
            if (!string.IsNullOrEmpty(key))
                _strings[key] = value;
        }
    }

    /// <summary>Splits a CSV line, respecting quoted fields.</summary>
    private static string[] SplitCsvLine(string line)
    {
        var result = new System.Collections.Generic.List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        foreach (char c in line)
        {
            if (c == '"')       { inQuotes = !inQuotes; }
            else if (c == ',' && !inQuotes) { result.Add(current.ToString()); current.Clear(); }
            else                { current.Append(c); }
        }
        result.Add(current.ToString());
        return result.ToArray();
    }
}
```

**Step 2: Create sample CSV**

Create `Assets/Resources/Localization/dialogues.csv` :

```
key,fr,en
sample_hello,Bonjour !,Hello!
sample_intro,C'est l'heure de l'aventure !,Time for adventure!
```

**Step 3: Verify in Unity**

No Play test yet — just confirm no compile errors.

**Step 4: Commit**

```bash
git add Assets/SCRIPT/LocalizationManager.cs Assets/SCRIPT/LocalizationManager.cs.meta
git add Assets/Resources/ Assets/Resources.meta
git commit -m "feat: add LocalizationManager with CSV-based localization"
```

---

## Task 6: LocalizedText — auto-refresh UI text on language change

**Files:**
- Create: `Assets/SCRIPT/LocalizedText.cs`

**Step 1: Create LocalizedText.cs**

```csharp
using TMPro;
using UnityEngine;

/// <summary>
/// Attach to any GameObject with a TextMeshProUGUI.
/// Automatically updates text when language changes.
/// Set _key to the CSV key for this text.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string _key;

    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= Refresh;
    }

    private void Refresh()
    {
        if (LocalizationManager.Instance == null) return;
        _text.text = LocalizationManager.Instance.Get(_key);
    }
}
```

**Step 2: Verify in Unity**

No Play test yet — compile errors check only.

**Step 3: Commit**

```bash
git add Assets/SCRIPT/LocalizedText.cs Assets/SCRIPT/LocalizedText.cs.meta
git commit -m "feat: add LocalizedText component for auto-refreshing UI text"
```

---

## Task 7: DialogueZone — add localization support (backward-compatible)

**Files:**
- Modify: `Assets/SCRIPT/DialogueZone.cs`

**Step 1: Add _lineKeys field and fallback logic**

In `DialogueZone.cs`, in the `[Header("Dialogue Content")]` section, add after `_lines`:

```csharp
[Tooltip("Optional. If LocalizationManager is present, these keys override _lines.")]
[SerializeField] private string[] _lineKeys;
```

Add a private helper method:

```csharp
/// <summary>
/// Returns the text for a given line index.
/// Uses localization keys if LocalizationManager is available and _lineKeys is set,
/// otherwise falls back to the raw _lines array.
/// </summary>
private string GetLine(int index)
{
    if (LocalizationManager.Instance != null
        && _lineKeys != null
        && index < _lineKeys.Length
        && !string.IsNullOrEmpty(_lineKeys[index]))
    {
        return LocalizationManager.Instance.Get(_lineKeys[index]);
    }

    return (index < _lines.Length) ? _lines[index] : string.Empty;
}
```

In `RunDialogue()`, replace the line:
```csharp
int lineCount = Mathf.Min(_lines.Length, _speakerNames.Length);
```
with:
```csharp
int lineCount = Mathf.Min(
    Mathf.Max(_lines.Length, _lineKeys?.Length ?? 0),
    _speakerNames.Length
);
```

And replace:
```csharp
_dialogueText.text = string.Empty;
// ...
yield return StartCoroutine(TypeLine(_lines[i]));
```
with:
```csharp
_dialogueText.text = string.Empty;
// ...
yield return StartCoroutine(TypeLine(GetLine(i)));
```

**Step 2: Test in Unity Editor**

- Existing DialogueZones (with `_lines` filled, no `_lineKeys`) → work unchanged.
- A DialogueZone with `_lineKeys` → shows localized text when LocalizationManager is in scene.

**Step 3: Commit**

```bash
git add Assets/SCRIPT/DialogueZone.cs
git commit -m "feat: DialogueZone supports localization keys with backward-compatible fallback"
```

---

## Task 8: OptionsMenu — UI script

**Files:**
- Create: `Assets/SCRIPT/OptionsMenu.cs`

**Step 1: Create OptionsMenu.cs**

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Options menu controller.
/// Attach to the root of your Options panel.
///
/// Inspector wiring required:
///   _musicSlider    → Slider (0-1), OnValueChanged → OnMusicVolumeChanged
///   _sfxSlider      → Slider (0-1), OnValueChanged → OnSfxVolumeChanged
///   _languageDropdown → TMP_Dropdown, OnValueChanged → OnLanguageChanged
///   _panel          → this panel's root GameObject
///
/// Languages in the Dropdown must match exactly the column names in your CSV
/// (e.g. first option = "fr", second = "en").
/// </summary>
public class OptionsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("Language")]
    [SerializeField] private TMP_Dropdown _languageDropdown;
    [Tooltip("Must match CSV column names in the same order as dropdown options.")]
    [SerializeField] private string[] _languageCodes = { "fr", "en" };

    [Header("Panel")]
    [SerializeField] private GameObject _panel;

    private void OnEnable()
    {
        // Initialise sliders to current values
        if (AudioManager.Instance != null)
        {
            if (_musicSlider != null) _musicSlider.value = AudioManager.Instance.MusicVolume;
            if (_sfxSlider   != null) _sfxSlider.value   = AudioManager.Instance.SfxVolume;
        }

        // Initialise dropdown to current language
        if (LocalizationManager.Instance != null && _languageDropdown != null)
        {
            string current = LocalizationManager.Instance.CurrentLanguage;
            int idx = System.Array.IndexOf(_languageCodes, current);
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

    // ── Callbacks (wire in Inspector) ────────────────────────────────────────

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
```

**Step 2: Add Options button to MenuPause**

In `MenuPause.cs`, add a reference and a method:

```csharp
[SerializeField] private OptionsMenu _optionsMenu;

public void OptionsButton()
{
    _optionsMenu?.Show();
}
```

**Step 3: Verify in Unity**

Compile check only for now.

**Step 4: Commit**

```bash
git add Assets/SCRIPT/OptionsMenu.cs Assets/SCRIPT/OptionsMenu.cs.meta
git add Assets/SCRIPT/MenuPause.cs
git commit -m "feat: add OptionsMenu with volume sliders and language dropdown"
```

---

## Task 9: Update UNITY_SCENE_SETUP.md

**Files:**
- Modify: `docs/UNITY_SCENE_SETUP.md`

Add sections for:
- **ExitZone** — setup (trigger zone, Milo-only, calls LoadNextScene)
- **MenuPause** — setup + Escape toggle + button wiring
- **OptionsMenu** — UI setup, slider/dropdown wiring, language codes
- **LocalizationManager** — Debut scene placement, CSV format, Resources folder
- **LocalizedText** — component usage on TextMeshProUGUI
- Update **DialogueZone** section to mention `_lineKeys`
- Update **GameManager** section to remove mention of corridors

**No commit needed** — documentation commits are batched at the end.

---

## Task 10: Final documentation commit

```bash
git add docs/UNITY_SCENE_SETUP.md
git commit -m "docs: update scene setup guide with all new systems"
```
