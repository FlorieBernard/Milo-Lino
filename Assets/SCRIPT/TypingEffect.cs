using System.Collections;
using TMPro;
using UnityEngine;

public class TypingEffect : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string[] _phrases;
    [SerializeField] private TextMeshProUGUI _textDisplay;

    [Tooltip("Delay between each letter (seconds)")]
    [SerializeField] private float _letterDelay = 0.05f;

    [Tooltip("Delay between each phrase (seconds)")]
    [SerializeField] private float _phraseDelay = 1f;

    [Tooltip("Load the next scene automatically when all phrases are shown")]
    [SerializeField] private bool _loadNextSceneOnComplete = false;

    [Tooltip("Delay before loading the next scene (seconds)")]
    [SerializeField] private float _sceneLoadDelay = 1f;

    private void Start()
    {
        if (_textDisplay == null)
            _textDisplay = GetComponent<TextMeshProUGUI>();

        StartCoroutine(ShowPhrases());
    }

    private IEnumerator ShowPhrases()
    {
        foreach (string phrase in _phrases)
        {
            _textDisplay.text = string.Empty;
            yield return StartCoroutine(ShowLetterByLetter(phrase));
            yield return new WaitForSeconds(_phraseDelay);
        }

        if (_loadNextSceneOnComplete)
        {
            yield return new WaitForSeconds(_sceneLoadDelay);
            GameManager.Instance?.LoadNextScene();
        }
    }

    private IEnumerator ShowLetterByLetter(string phrase)
    {
        for (int i = 0; i <= phrase.Length; i++)
        {
            _textDisplay.text = phrase.Substring(0, i);
            yield return new WaitForSeconds(_letterDelay);
        }
    }
}