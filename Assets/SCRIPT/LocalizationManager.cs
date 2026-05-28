using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent singleton that loads and serves localized strings from CSV files.
///
/// CSV format (one file, all languages):
///   key,fr,en
///   intro_01,Bonjour !,Hello!
///
/// Place this GameObject in the "Debut" scene alongside GameManager.
/// CSV files go in: Assets/Resources/Localization/
///
/// Usage:
///   LocalizationManager.Instance.Get("my_key")   → current language string
///   LocalizationManager.Instance.SetLanguage("en")
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    /// <summary>Fired whenever the language changes. Subscribe to refresh UI text.</summary>
    public static event Action OnLanguageChanged;

    [SerializeField] private string _defaultLanguage = "fr";
    [Tooltip("Paths relative to Resources/, without extension. E.g. 'Localization/dialogues'")]
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

    /// <summary>Switches the active language and reloads all CSV files.</summary>
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

    // ── CSV parsing ───────────────────────────────────────────────────────────

    private void LoadCsv(string resourcePath)
    {
        TextAsset asset = Resources.Load<TextAsset>(resourcePath);
        if (asset == null)
        {
            Debug.LogWarning($"[LocalizationManager] CSV not found at Resources/{resourcePath}.csv");
            return;
        }

        string[] lines = asset.text.Split('\n');
        if (lines.Length < 2) return;

        string[] header = SplitCsvLine(lines[0]);
        int langIndex = Array.IndexOf(header, _currentLanguage.Trim());
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

    /// <summary>Splits a CSV line, respecting double-quoted fields that may contain commas.</summary>
    private static string[] SplitCsvLine(string line)
    {
        var result  = new List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        foreach (char c in line)
        {
            if (c == '"')                   inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes) { result.Add(current.ToString()); current.Clear(); }
            else                            current.Append(c);
        }
        result.Add(current.ToString());
        return result.ToArray();
    }
}
