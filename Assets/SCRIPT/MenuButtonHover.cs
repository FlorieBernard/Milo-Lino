using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Affiche une patte de chat à gauche du bouton au survol de la souris.
/// Attacher sur chaque Button du menu. Référencer l'Image enfant "Patte".
/// </summary>
public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image pawImage;
    [SerializeField] private float fadeDuration = 0.2f;

    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        if (pawImage == null)
        {
            Debug.LogWarning($"[MenuButtonHover] Paw Image non assignée sur {gameObject.name}. Assigne l'Image enfant 'Patte' dans l'Inspector.", this);
            return;
        }
        pawImage.canvasRenderer.SetAlpha(0f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartFade(1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartFade(0f);
    }

    private void StartFade(float targetAlpha)
    {
        if (pawImage == null) return;
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeTo(targetAlpha));
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = pawImage.canvasRenderer.GetAlpha();
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            pawImage.canvasRenderer.SetAlpha(alpha);
            yield return null;
        }

        pawImage.canvasRenderer.SetAlpha(targetAlpha);
        _fadeCoroutine = null;
    }
}
