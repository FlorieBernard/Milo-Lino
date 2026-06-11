using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Triggers a dialogue sequence when a character enters the zone.
/// Drop on any GameObject with a Trigger Collider2D.
/// </summary>
public class DialogueZone : MonoBehaviour
{
    public enum TriggerTarget { Milo, Lino, Both }
    public enum AdvanceMode { InputOnly, TimerOnly, InputOrTimer }

    [Header("Trigger")]
    [Tooltip("Which character(s) can start this dialogue.")]
    [SerializeField] private TriggerTarget _triggerTarget = TriggerTarget.Both;
    [Tooltip("Can this dialogue play more than once?")]
    [SerializeField] private bool _repeatable = false;

    [Header("Dialogue Content")]
    [SerializeField] private Sprite _miloPortrait;
    [SerializeField] private Sprite _linoPortrait;
    [TextArea(2, 5)]
    [SerializeField] private string[] _lines;
    [Tooltip("Optional. CSV keys for localized text. If set and LocalizationManager is present, overrides _lines.")]
    [SerializeField] private string[] _lineKeys;
    [SerializeField] private string[] _speakerNames;
    [Tooltip("Optional. Emotion sprite per line; empty entry = character's default portrait.")]
    [SerializeField] private Sprite[] _lineEmojis;

    [Header("Name Colors")]
    [SerializeField] private Color _miloNameColor = new Color(0.20f, 0.55f, 0.85f);
    [SerializeField] private Color _linoNameColor = new Color(0.90f, 0.70f, 0.05f);

    [Header("UI References")]
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private Image _portrait;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [Tooltip("Optional. Dedicated Image for per-line emojis (hidden when the line has none).")]
    [SerializeField] private Image _emojiImage;
    [Tooltip("Optional — shown while waiting for player input.")]
    [SerializeField] private GameObject _continueIndicator;

    [Header("Timings")]
    [SerializeField] private float _letterDelay = 0.04f;
    [Tooltip("Delay before auto-advance (TimerOnly / InputOrTimer modes).")]
    [SerializeField] private float _linePause = 2f;

    [Header("Interaction")]
    [Tooltip("How lines advance: input only, timer only, or whichever comes first.")]
    [SerializeField] private AdvanceMode _advanceMode = AdvanceMode.InputOrTimer;
    [Tooltip("Skip typing and show full line instantly on input.")]
    [SerializeField] private bool _skipTypingOnInput = true;

    private bool _isRunning  = false;
    private bool _hasPlayed  = false;
    private bool _inputPressed = false;
    private bool _isTyping   = false;

    private static int _activeDialogues = 0;

    /// <summary>True while any dialogue is running. Used to block player actions (e.g. jump).</summary>
    public static bool IsDialogueRunning => _activeDialogues > 0;

    private void Update()
    {
        if (_isRunning && Input.GetButtonDown("Jump"))
            _inputPressed = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Le trigger marche");
        if (_isRunning) return;
        if (!_repeatable && _hasPlayed) return;
        if (!CanTrigger(other)) return;

        StartCoroutine(RunDialogue());
    }

    private bool CanTrigger(Collider2D other)
    {
        return _triggerTarget switch
        {
            TriggerTarget.Milo => other.CompareTag("Milo"),
            TriggerTarget.Lino => other.CompareTag("Lino"),
            TriggerTarget.Both => other.CompareTag("Milo") || other.CompareTag("Lino"),
            _                  => false
        };
    }

    private IEnumerator RunDialogue()
    {
        Debug.Log("Dialogue is running");
        _isRunning = true;
        _hasPlayed = true;
        _inputPressed = false;
        _activeDialogues++;

        if (_dialoguePanel != null)
        {
            Debug.Log("panel is not null");
            _dialoguePanel.SetActive(true);
        }
        if (_continueIndicator != null) _continueIndicator.SetActive(false);

        // The name text may live outside the panel hierarchy — drive it explicitly.
        if (_nameText != null) _nameText.gameObject.SetActive(true);

        int lineCount = Mathf.Min(
            Mathf.Max(_lines.Length, _lineKeys?.Length ?? 0),
            _speakerNames.Length
        );

        for (int i = 0; i < lineCount; i++)
        {
            _nameText.text    = _speakerNames[i];
            _nameText.color   = PickNameColor(_speakerNames[i]);
            _portrait.sprite  = PickPortrait(_speakerNames[i]);
            UpdateEmoji(i);
            _dialogueText.text = string.Empty;
            _inputPressed = false;

            yield return StartCoroutine(TypeLine(GetLine(i)));

            // Line finished — wait for input or auto-pause
            if (_continueIndicator != null) _continueIndicator.SetActive(true);

            yield return StartCoroutine(WaitForAdvance());

            if (_continueIndicator != null) _continueIndicator.SetActive(false);
        }

        if (_emojiImage != null) _emojiImage.gameObject.SetActive(false);
        if (_nameText != null) _nameText.gameObject.SetActive(false);
        if (_dialoguePanel != null) _dialoguePanel.SetActive(false);
        _isRunning = false;
        _activeDialogues--;
    }

    // Safety net: if the zone is disabled mid-dialogue, the coroutine dies —
    // release the global lock so the player is never stuck unable to jump.
    private void OnDisable()
    {
        if (_isRunning)
        {
            _isRunning = false;
            _activeDialogues = Mathf.Max(0, _activeDialogues - 1);
        }
    }

    private IEnumerator TypeLine(string line)
    {
        _isTyping = true;

        foreach (char c in line)
        {
            // Skip typing instantly if player presses input while typing
            if (_skipTypingOnInput && _inputPressed)
            {
                _dialogueText.text = line;
                _inputPressed = false;
                break;
            }

            _dialogueText.text += c;
            yield return new WaitForSeconds(_letterDelay);
        }

        _dialogueText.text = line; // ensure full line is displayed
        _isTyping = false;
    }

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

    /// <summary>
    /// Returns the text for a line. Uses localization key if LocalizationManager is present
    /// and a key is defined, otherwise falls back to the raw _lines entry.
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

    private Sprite PickPortrait(string speakerName)
    {
        if (speakerName == "Milo") return _miloPortrait;
        if (speakerName == "Lino") return _linoPortrait;
        return _miloPortrait; // fallback
    }

    /// <summary>
    /// Shows the line's emoji in the dedicated Image, or hides it when the
    /// line has no emoji (or no Image is assigned).
    /// </summary>
    private void UpdateEmoji(int lineIndex)
    {
        if (_emojiImage == null) return;

        Sprite emoji = (_lineEmojis != null && lineIndex < _lineEmojis.Length)
            ? _lineEmojis[lineIndex]
            : null;

        _emojiImage.sprite = emoji;
        _emojiImage.gameObject.SetActive(emoji != null);
    }

    /// <summary>Returns the name color for the current speaker.</summary>
    private Color PickNameColor(string speakerName)
    {
        return speakerName == "Lino" ? _linoNameColor : _miloNameColor;
    }
}
