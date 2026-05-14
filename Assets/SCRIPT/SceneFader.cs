using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Persistent singleton that handles fade-to-black transitions between scenes.
/// Place this GameObject in the "Debut" scene alongside GameManager and AudioManager.
/// No prefab or UI setup needed — the overlay is built automatically in code.
/// </summary>
public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private Color _fadeColor = Color.black;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlay();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeIn());
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeIn()
    {
        yield return Fade(from: 1f, to: 0f);
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        yield return Fade(from: 0f, to: 1f);
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator Fade(float from, float to)
    {
        _canvasGroup.alpha = from;
        for (float t = 0f; t < _fadeDuration; t += Time.unscaledDeltaTime)
        {
            _canvasGroup.alpha = Mathf.Lerp(from, to, t / _fadeDuration);
            yield return null;
        }
        _canvasGroup.alpha = to;
    }

    /// <summary>Builds a fullscreen black overlay at runtime — no prefab needed.</summary>
    private void BuildOverlay()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Always on top of everything

        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("FadePanel");
        panel.transform.SetParent(transform, false);

        Image image = panel.AddComponent<Image>();
        image.color = _fadeColor;

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        _canvasGroup = panel.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 1f;          // Start black on first scene
        _canvasGroup.blocksRaycasts = false;
    }
}
