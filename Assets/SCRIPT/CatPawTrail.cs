using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Génère des traversées de pattes de chat en décor sur le menu.
/// Attacher sur un GameObject vide enfant du Canvas.
/// </summary>
public class CatPawTrail : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Sprite pawSprite;
    [SerializeField] private Canvas canvas;

    [Header("Pool")]
    [SerializeField] private int poolSize = 20;

    [Header("Paramètres traversée")]
    [SerializeField] private float spawnInterval = 8f;
    [SerializeField] private float stepDelay = 0.15f;
    [SerializeField] private int stepsPerTrail = 6;
    [SerializeField] private float pawSize = 60f;
    [SerializeField] private float lateralOffset = 30f;
    [SerializeField] private float angleVariation = 15f;

    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float visibleDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private readonly List<Image> _pool = new();

    private void Start()
    {
        BuildPool();
        StartCoroutine(TrailLoop());
    }

    private void BuildPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject("CatPaw", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(transform, false);

            var img = go.GetComponent<Image>();
            img.sprite = pawSprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.canvasRenderer.SetAlpha(0f);

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(pawSize, pawSize);

            go.SetActive(false);
            _pool.Add(img);
        }
    }

    private Image GetPooledPaw()
    {
        foreach (var img in _pool)
        {
            if (!img.gameObject.activeSelf)
                return img;
        }
        return null;
    }

    private IEnumerator TrailLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            StartCoroutine(SpawnTrail());
        }
    }

    private IEnumerator SpawnTrail()
    {
        var (startPos, direction) = GetRandomTrailPath();
        float angleRad = Mathf.Atan2(direction.y, direction.x);
        float angleRandOffset = Random.Range(-angleVariation, angleVariation) * Mathf.Deg2Rad;
        angleRad += angleRandOffset;

        Vector2 forward = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        Vector2 perpendicular = new Vector2(-forward.y, forward.x);

        float stepDistance = (canvas.GetComponent<RectTransform>().rect.width * 1.4f) / stepsPerTrail;

        for (int i = 0; i < stepsPerTrail; i++)
        {
            var paw = GetPooledPaw();
            if (paw == null) yield break;

            float side = (i % 2 == 0) ? 1f : -1f;
            Vector2 pos = startPos + forward * (stepDistance * i) + perpendicular * (lateralOffset * side);

            var rt = paw.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;

            float angleDeg = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
            rt.localRotation = Quaternion.Euler(0f, 0f, angleDeg - 90f);

            paw.gameObject.SetActive(true);
            StartCoroutine(PawLifecycle(paw));

            yield return new WaitForSeconds(stepDelay);
        }
    }

    private (Vector2 start, Vector2 dir) GetRandomTrailPath()
    {
        var rt = canvas.GetComponent<RectTransform>();
        float w = rt.rect.width;
        float h = rt.rect.height;
        float hw = w / 2f;
        float hh = h / 2f;

        Vector2[] corners = {
            new(-hw, -hh),
            new( hw, -hh),
            new(-hw,  hh),
            new( hw,  hh)
        };

        int startIdx = Random.Range(0, 4);
        int endIdx = (startIdx + 2) % 4;
        if (Random.value > 0.5f)
            endIdx = 3 - startIdx;

        Vector2 start = corners[startIdx];
        Vector2 dir = (corners[endIdx] - corners[startIdx]).normalized;
        return (start, dir);
    }

    private IEnumerator PawLifecycle(Image paw)
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            paw.canvasRenderer.SetAlpha(elapsed / fadeInDuration);
            yield return null;
        }
        paw.canvasRenderer.SetAlpha(1f);

        yield return new WaitForSeconds(visibleDuration);

        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            paw.canvasRenderer.SetAlpha(1f - elapsed / fadeOutDuration);
            yield return null;
        }
        paw.canvasRenderer.SetAlpha(0f);
        paw.gameObject.SetActive(false);
    }
}
