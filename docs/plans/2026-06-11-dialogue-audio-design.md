# Design — Dialogue improvements, per-character sounds, SceneMusic fix

Date: 2026-06-11
Status: validated

## 1. Dialogue auto-advance (DialogueZone.cs)

Replace `_waitForInput` (bool) with an enum:

```csharp
public enum AdvanceMode { InputOnly, TimerOnly, InputOrTimer }
```

- `InputOrTimer` (new default): after a line is fully typed, advance on Jump
  input **or** after `_linePause` seconds — whichever comes first.
- `InputOnly` / `TimerOnly` preserve the two previous behaviors.
- `_skipTypingOnInput` unchanged.

Note: replacing the serialized bool resets the field in scenes that used
`_waitForInput = false`; acceptable since the new default covers both.

## 2. Emoji per line (DialogueZone.cs)

- New parallel array `Sprite[] _lineEmojis` (same pattern as `_speakerNames`).
- Per line: if `_lineEmojis[i]` is set → use it as portrait sprite;
  otherwise fallback to `PickPortrait(speakerName)` (default Milo/Lino portrait).
- Array may be shorter than `_lines` — out-of-range = fallback.

## 3. Speaker name color (DialogueZone.cs)

- New inspector fields `Color _miloNameColor`, `Color _linoNameColor`.
- Applied to `_nameText.color` according to the speaker of the current line.
- Unknown speaker → keep `_miloNameColor` as fallback (mirrors PickPortrait).

## 4. Per-character sounds (LevelConnector.cs + CharacterSwitcher.cs)

Mirror of the existing VFX pattern:

- `LevelConnector`: `AudioSource[] _commonSounds`, `_miloSounds`, `_linoSounds`
  exposed as `IReadOnlyList<AudioSource>` properties.
- `CharacterSwitcher`:
  - `StartCommonSounds()` at Start — plays common sources.
  - `SwapSounds(bool playingMilo)` on switch — **mute/unmute** (not Stop/Play)
    so ambient loops stay time-synced with no audible restart.
  - All character sources start playing at Start (muted or not), then only
    `mute` is toggled.

## 5. SceneMusic fix

Root causes identified:

1. **Muffle inversion (main suspect)** — `CharacterSwitcher.cs:166` passes
   `!_isPlayingMilo` to `SetMuffled`, muffling all audio (music included,
   800 Hz low-pass on the AudioListener) while **Lino** is active.
   Confirmed intent: **Milo** hears muffled. Fix: `SetMuffled(_isPlayingMilo)`.
2. **Silent no-op** — `AudioManager.Instance?.PlayMusic(...)` does nothing when
   testing a scene directly (AudioManager only lives in "Debut" scene).
   Fix: `Debug.LogWarning` in `SceneMusic.Start` when Instance is null.
3. **Unassigned clip stops music** — intended behavior but confusing.
   Fix: `Debug.LogWarning` when `_musicClip` is null.

## Out of scope

- Refactor of parallel arrays into `DialogueLine[]` struct (would wipe
  serialized dialogue data in scenes).
- ScriptableObject-based dialogue assets (YAGNI).
