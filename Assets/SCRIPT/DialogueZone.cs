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

    [Header("UI References")]
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private Image _portrait;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [Tooltip("Optional — shown while waiting for player input.")]
    [SerializeField] private GameObject _continueIndicator;

    [Header("Timings")]
    [SerializeField] private float _letterDelay = 0.04f;
    [Tooltip("If Wait For Input is false, pause between lines (seconds).")]
    [SerializeField] private float _linePause = 2f;

    [Header("Interaction")]
    [Tooltip("Player presses Space/Enter to advance instead of auto-timer.")]
    [SerializeField] private bool _waitForInput = true;
    [Tooltip("Skip typing and show full line instantly on input.")]
    [SerializeField] private bool _skipTypingOnInput = true;

    private bool _isRunning  = false;
    private bool _hasPlayed  = false;
    private bool _inputPressed = false;
    private bool _isTyping   = false;

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

        if (_dialoguePanel != null)
        {
            Debug.Log("panel is not null");
            _dialoguePanel.SetActive(true);
        }
        if (_continueIndicator != null) _continueIndicator.SetActive(false);

        int lineCount = Mathf.Min(
            Mathf.Max(_lines.Length, _lineKeys?.Length ?? 0),
            _speakerNames.Length
        );

        for (int i = 0; i < lineCount; i++)
        {
            _nameText.text    = _speakerNames[i];
            _portrait.sprite  = PickPortrait(_speakerNames[i]);
            _dialogueText.text = string.Empty;
            _inputPressed = false;

            yield return StartCoroutine(TypeLine(GetLine(i)));

            // Line finished — wait for input or auto-pause
            if (_continueIndicator != null) _continueIndicator.SetActive(true);

            if (_waitForInput)
            {
                yield return new WaitUntil(() => _inputPressed);
                _inputPressed = false;
            }
            else
            {
                yield return new WaitForSeconds(_linePause);
            }

            if (_continueIndicator != null) _continueIndicator.SetActive(false);
        }

        //if (_dialoguePanel!=null)_dialoguePanel.SetActive(false);
        _isRunning = false;
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
}
