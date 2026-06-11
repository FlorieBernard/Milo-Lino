# Dialogue & Audio Improvements Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Auto-advancing dialogues with per-line emoji and speaker name colors; per-character ambient sounds via LevelConnector; fix SceneMusic (muffle inversion + silent failures).

**Architecture:** All changes extend existing components (DialogueZone, LevelConnector, CharacterSwitcher, SceneMusic) following patterns already in place (parallel arrays for dialogue data, VFX-style lists on LevelConnector). No new GameObjects, no new dependencies.

**Tech Stack:** Unity 2D (C#), TextMeshPro. No automated test framework in this project — each task ends with a manual Play-mode verification step instead (documented per task).

**Design doc:** `docs/plans/2026-06-11-dialogue-audio-design.md`

**Git rules (project):** only stage `.cs` files and `docs/` + `PROGRESS.md`, always with explicit paths. Never `git add .`. Scene/asset changes (inspector wiring) are committed by Yohan himself.

---

### Task 1: DialogueZone — AdvanceMode (input OR timer)

**Files:**
- Modify: `Assets/SCRIPT/DialogueZone.cs`

**Step 1: Replace the `_waitForInput` field with an enum**

Replace:

```csharp
    [Header("Interaction")]
    [Tooltip("Player presses Space/Enter to advance instead of auto-timer.")]
    [SerializeField] private bool _waitForInput = true;
```

with:

```csharp
    public enum AdvanceMode { InputOnly, TimerOnly, InputOrTimer }

    [Header("Interaction")]
    [Tooltip("How lines advance: input only, timer only, or whichever comes first.")]
    [SerializeField] private AdvanceMode _advanceMode = AdvanceMode.InputOrTimer;
```

(Place the enum declaration just below the existing `TriggerTarget` enum to keep enums grouped; keep the `[Header]` on the field.)

**Step 2: Replace the wait logic in `RunDialogue`**

Replace:

```csharp
            if (_waitForInput)
            {
                yield return new WaitUntil(() => _inputPressed);
                _inputPressed = false;
            }
            else
            {
                yield return new WaitForSeconds(_linePause);
            }
```

with:

```csharp
            yield return StartCoroutine(WaitForAdvance());
```

**Step 3: Add the `WaitForAdvance` coroutine** (below `TypeLine`):

```csharp
    /// <summary>
    /// Waits for the configured advance condition after a line is displayed:
    /// player input, timer, or whichever comes first.
    /// </summary>
    private IEnumerator WaitForAdvance()
    {
        switch (_advanceMode)
        {
            case AdvanceMode.InputOnly:
                yield return new WaitUntil(() => _inputPressed);
                break;
            case AdvanceMode.TimerOnly:
                yield return new WaitForSeconds(_linePause);
                break;
            case AdvanceMode.InputOrTimer:
                float deadline = Time.time + _linePause;
                yield return new WaitUntil(() => _inputPressed || Time.time >= deadline);
                break;
        }
        _inputPressed = false;
    }
```

**Step 4: Update the `_linePause` tooltip**

Replace `"If Wait For Input is false, pause between lines (seconds)."` with `"Delay before auto-advance (TimerOnly / InputOrTimer modes)."`

**Step 5: Verify (Unity)**

Open a scene with a DialogueZone, set Advance Mode = InputOrTimer, Play: a line must advance alone after `_linePause` seconds, AND immediately if Space is pressed. No console errors. ⚠️ Scenes that used `Wait For Input = false` now need Advance Mode = TimerOnly set in the inspector (serialized bool is dropped).

**Step 6: Commit**

```bash
rtk git add Assets/SCRIPT/DialogueZone.cs && rtk git commit -m "feat(dialogue): auto-advance lines via input, timer, or both"
```

---

### Task 2: DialogueZone — per-line emoji + speaker name color

**Files:**
- Modify: `Assets/SCRIPT/DialogueZone.cs`

**Step 1: Add serialized fields** (in "Dialogue Content", after `_speakerNames`):

```csharp
    [Tooltip("Optional. Emotion sprite per line; empty entry = character's default portrait.")]
    [SerializeField] private Sprite[] _lineEmojis;

    [Header("Name Colors")]
    [SerializeField] private Color _miloNameColor = Color.white;
    [SerializeField] private Color _linoNameColor = Color.white;
```

**Step 2: Use them in `RunDialogue`**

Replace:

```csharp
            _nameText.text    = _speakerNames[i];
            _portrait.sprite  = PickPortrait(_speakerNames[i]);
```

with:

```csharp
            _nameText.text    = _speakerNames[i];
            _nameText.color   = PickNameColor(_speakerNames[i]);
            _portrait.sprite  = PickSprite(i, _speakerNames[i]);
```

**Step 3: Add the helpers** (next to `PickPortrait`):

```csharp
    /// <summary>
    /// Returns the per-line emotion sprite if one is set, otherwise the
    /// speaker's default portrait.
    /// </summary>
    private Sprite PickSprite(int lineIndex, string speakerName)
    {
        if (_lineEmojis != null
            && lineIndex < _lineEmojis.Length
            && _lineEmojis[lineIndex] != null)
        {
            return _lineEmojis[lineIndex];
        }

        return PickPortrait(speakerName);
    }

    /// <summary>Returns the name color for the current speaker.</summary>
    private Color PickNameColor(string speakerName)
    {
        return speakerName == "Lino" ? _linoNameColor : _miloNameColor;
    }
```

**Step 4: Verify (Unity)**

In a DialogueZone, fill `_lineEmojis` for some lines only. Play: lines with a sprite show it, the others show the default portrait; the name switches color between Milo and Lino lines.

**Step 5: Commit**

```bash
rtk git add Assets/SCRIPT/DialogueZone.cs && rtk git commit -m "feat(dialogue): per-line emoji sprites and speaker name colors"
```

---

### Task 3: LevelConnector — per-character sound lists

**Files:**
- Modify: `Assets/SCRIPT/LevelConnector.cs`

**Step 1: Add fields** (after the VFX fields):

```csharp
    [Header("Ambient Sounds")]
    [SerializeField] private AudioSource[] _commonSounds = System.Array.Empty<AudioSource>();
    [SerializeField] private AudioSource[] _miloSounds   = System.Array.Empty<AudioSource>();
    [SerializeField] private AudioSource[] _linoSounds   = System.Array.Empty<AudioSource>();
```

**Step 2: Add properties** (after the VFX properties):

```csharp
    /// <summary>Sons toujours audibles, quel que soit le personnage.</summary>
    public IReadOnlyList<AudioSource> CommonSounds => _commonSounds;

    /// <summary>Sons audibles uniquement en mode Milo.</summary>
    public IReadOnlyList<AudioSource> MiloSounds => _miloSounds;

    /// <summary>Sons audibles uniquement en mode Lino.</summary>
    public IReadOnlyList<AudioSource> LinoSounds => _linoSounds;
```

**Step 3: Commit**

```bash
rtk git add Assets/SCRIPT/LevelConnector.cs && rtk git commit -m "feat(audio): per-character ambient sound lists on LevelConnector"
```

---

### Task 4: CharacterSwitcher — play/mute sounds on switch

**Files:**
- Modify: `Assets/SCRIPT/CharacterSwitcher.cs`

**Step 1: Start sounds in `Start()`** — after `StartCommonVfx();` add:

```csharp
        StartSounds();
```

**Step 2: Swap on switch** — in `SwitchCharacter()`, after `SwapVfx(_isPlayingMilo);` add:

```csharp
        SwapSounds(_isPlayingMilo);
```

**Step 3: Add the methods** (below `SwapVfx`):

```csharp
    /// <summary>
    /// Starts every ambient AudioSource once, then mutes the ones the current
    /// character cannot hear. Sources keep playing muted so loops stay in sync.
    /// </summary>
    private void StartSounds()
    {
        if (_levelConnector == null) return;

        foreach (AudioSource s in _levelConnector.CommonSounds) s?.Play();
        foreach (AudioSource s in _levelConnector.MiloSounds)   s?.Play();
        foreach (AudioSource s in _levelConnector.LinoSounds)   s?.Play();

        SwapSounds(_isPlayingMilo);
    }

    /// <summary>
    /// Mutes the sounds the current character cannot hear and unmutes his own.
    /// Common sounds are never touched here.
    /// </summary>
    private void SwapSounds(bool playingMilo)
    {
        if (_levelConnector == null) return;

        foreach (AudioSource s in _levelConnector.MiloSounds)
        {
            if (s != null) s.mute = !playingMilo;
        }
        foreach (AudioSource s in _levelConnector.LinoSounds)
        {
            if (s != null) s.mute = playingMilo;
        }
    }
```

**Step 4: Verify (Unity)**

In a scene's LevelConnector, assign one looping AudioSource to Milo Sounds and one to Lino Sounds. Play: only Milo's sound is audible; press Tab → it mutes and Lino's becomes audible, without restarting.

**Step 5: Commit**

```bash
rtk git add Assets/SCRIPT/CharacterSwitcher.cs && rtk git commit -m "feat(audio): mute/unmute per-character ambient sounds on switch"
```

---

### Task 5: Fix muffle inversion + SceneMusic warnings

**Files:**
- Modify: `Assets/SCRIPT/CharacterSwitcher.cs:166`
- Modify: `Assets/SCRIPT/SceneMusic.cs`

**Step 1: Fix the inversion** — in `UpdateColors()`, replace:

```csharp
        AudioManager.Instance?.SetMuffled(!_isPlayingMilo);
```

with:

```csharp
        AudioManager.Instance?.SetMuffled(_isPlayingMilo);
```

**Step 2: Make SceneMusic failures visible** — replace the body of `SceneMusic.Start()`:

```csharp
    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[SceneMusic] No AudioManager in scene — start from the 'Debut' scene to hear music.", this);
            return;
        }

        if (_musicClip == null)
            Debug.LogWarning("[SceneMusic] No clip assigned — current music will stop.", this);

        AudioManager.Instance.PlayMusic(_musicClip);
    }
```

**Step 3: Verify (Unity)**

- Play from "Debut": music plays clearly while Lino is active; switch to Milo (Tab) → everything sounds muffled (low-pass), music included.
- Play a scene directly without AudioManager → console shows the `[SceneMusic]` warning instead of silent nothing.

**Step 4: Commit**

```bash
rtk git add Assets/SCRIPT/CharacterSwitcher.cs Assets/SCRIPT/SceneMusic.cs && rtk git commit -m "fix(audio): muffle Milo (not Lino) and warn on SceneMusic misconfig"
```

---

### Task 6: Closing

**Files:**
- Modify: `PROGRESS.md` (rewrite with current state)
- Create/Modify: `docs/changelog/patch_note_V0_x.md` (next minor version — feature additions)

**Steps:** update PROGRESS.md and the patch note, then:

```bash
rtk git add PROGRESS.md docs/changelog/ && rtk git commit -m "docs: update progress and changelog"
```
