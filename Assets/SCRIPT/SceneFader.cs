using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Persistent singleton that handles animated transitions between scenes.
/// All timing and style parameters are configurable from the Inspector.
/// Place this GameObject in the "Debut" scene alongside GameManager.
/// </summary>
public class SceneFader : MonoBehaviour
{
    public enum TransitionStyle
    {
        Fade,
        SlideLeft,
        SlideRight,
        SlideUp,
        SlideDown
    }

    public static SceneFader Instance { get; private set; }

    [Header("Style")]
    [SerializeField] private TransitionStyle _style = TransitionStyle.Fade;
    [SerializeField] private Color _transitionColor = Color.black;

    [Header("Timings")]
    [SerializeField] private float _fadeOutDuration = 0.5f;
    [SerializeField] private float _holdDuration    = 0.1f;
    [SerializeField] private float _fadeInDuration  = 0.5f;
    [SerializeField] private float _fadeInDelay     = 0.0f;

    [Header("Easing")]
    [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private CanvasGroup   _canvasGroup;
    private RectTransform _panelRect;

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

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeInSequence());
    }

    /// <summary>Triggers the out-transition then loads the target scene.</summary>
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    // ── Coroutines ────────────────────────────────────────────────────────────

    private IEnumerator FadeInSequence()
    {
        SetProgress(1f);
        if (_fadeInDelay > 0f)
            yield return new WaitForSecondsRealtime(_fadeInDelay);
        yield return AnimateTransition(1f, 0f, _fadeInDuration);
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        SetProgress(0f);
        yield return AnimateTransition(0f, 1f, _fadeOutDuration);
        if (_holdDuration > 0f)
            yield return new WaitForSecondsRealtime(_holdDuration);
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Animates progress from <paramref name="from"/> to <paramref name="to"/>
    /// over <paramref name="duration"/> seconds using the configured curve and style.
    /// Progress 0 = hidden, 1 = fully covering the screen.
    /// </summary>
    private IEnumerator AnimateTransition(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            SetProgress(to);
            yield break;
        }

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float linear   = t / duration;
            float eased    = _curve.Evaluate(Mathf.Clamp01(linear));
            float progress = Mathf.Lerp(from, to, eased);
            SetProgress(progress);
            yield return null;
        }

        SetProgress(to);
    }

    /// <summary>
    /// Applies the transition progress [0 = hidden, 1 = covering] according to the chosen style.
    /// </summary>
    private void SetProgress(float progress)
    {
        switch (_style)
        {
            case TransitionStyle.Fade:
                _canvasGroup.alpha = progress;
                _panelRect.anchoredPosition = Vector2.zero;
                break;

            case TransitionStyle.SlideLeft:
                _canvasGroup.alpha = 1f;
                _panelRect.anchoredPosition = new Vector2(
                    Mathf.Lerp(Screen.width, 0f, progress), 0f);
                break;

            case TransitionStyle.SlideRight:
                _canvasGroup.alpha = 1f;
                _panelRect.anchoredPosition = new Vector2(
                    Mathf.Lerp(-Screen.width, 0f, progress), 0f);
                break;

            case TransitionStyle.SlideUp:
                _canvasGroup.alpha = 1f;
                _panelRect.anchoredPosition = new Vector2(
                    0f, Mathf.Lerp(-Screen.height, 0f, progress));
                break;

            case TransitionStyle.SlideDown:
                _canvasGroup.alpha = 1f;
                _panelRect.anchoredPosition = new Vector2(
                    0f, Mathf.Lerp(Screen.height, 0f, progress));
                break;
        }
    }

    // ── UI Construction ───────────────────────────────────────────────────────

    /// <summary>Builds a fullscreen overlay at runtime — no prefab needed.</summary>
    private void BuildOverlay()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("TransitionPanel");
        panel.transform.SetParent(transform, false);

        Image image = panel.AddComponent<Image>();
        image.color = _transitionColor;

        _panelRect = panel.GetComponent<RectTransform>();
        _panelRect.anchorMin        = Vector2.zero;
        _panelRect.anchorMax        = Vector2.one;
        _panelRect.offsetMin        = Vector2.zero;
        _panelRect.offsetMax        = Vector2.zero;
        _panelRect.anchoredPosition = Vector2.zero;

        _canvasGroup = panel.AddComponent<CanvasGroup>();
        _canvasGroup.alpha         = 1f;
        _canvasGroup.blocksRaycasts = false;
    }
}
