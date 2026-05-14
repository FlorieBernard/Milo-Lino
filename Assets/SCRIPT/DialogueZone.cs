using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueZone : MonoBehaviour
{
    private const string MiloName = "Milo";

    [Header("Dialogue Content")]
    [SerializeField] private Sprite _miloPortrait;
    [SerializeField] private Sprite _linoPortrait;
    [TextArea(2, 4)]
    [SerializeField] private string[] _lines;
    [SerializeField] private string[] _speakerNames;

    [Header("UI References")]
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private Image _portrait;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _dialogueText;

    [Header("Timings")]
    [SerializeField] private float _letterDelay = 0.04f;
    [SerializeField] private float _linePause = 2f;

    private bool _isRunning = false;
    private bool _hasPlayed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(MiloName) || _hasPlayed || _isRunning) return;
        StartCoroutine(StartDialogue());
    }

    private IEnumerator StartDialogue()
    {
        _isRunning = true;
        _hasPlayed = true;
        _dialoguePanel.SetActive(true);

        int lineCount = Mathf.Min(_lines.Length, _speakerNames.Length);

        for (int i = 0; i < lineCount; i++)
        {
            _nameText.text = _speakerNames[i];
            _portrait.sprite = _speakerNames[i] == MiloName ? _miloPortrait : _linoPortrait;
            _dialogueText.text = string.Empty;

            foreach (char c in _lines[i])
            {
                _dialogueText.text += c;
                yield return new WaitForSeconds(_letterDelay);
            }

            yield return new WaitForSeconds(_linePause);
        }

        _dialoguePanel.SetActive(false);
        _isRunning = false;
    }
}
