using TMPro;
using UnityEngine;

/// <summary>
/// Attach to any GameObject with a TextMeshProUGUI to auto-refresh its text when the language changes.
/// Set _key in the Inspector to the corresponding CSV key.
/// If LocalizationManager is not in the scene, the text remains unchanged.
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
