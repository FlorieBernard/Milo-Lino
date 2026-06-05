# Menu Cat Paw Effects Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Ajouter une patte de chat au survol des boutons du menu, et une animation décorative de traversées de pattes sur l'écran du menu.

**Architecture:** Deux composants Unity UI indépendants : `MenuButtonHover.cs` (IPointerEnterHandler/IPointerExitHandler + fade coroutine sur une Image enfant) et `CatPawTrail.cs` (manager de pool qui spawne des traversées de pattes en diagonale à intervalle régulier). Tout en UI Canvas, aucune dépendance externe.

**Tech Stack:** Unity UI (uGUI), C#, CanvasGroup/Image, Coroutines, Object Pool manuel

---

### Task 1 : Créer le composant MenuButtonHover

**Files:**
- Create: `Assets/SCRIPT/MenuButtonHover.cs`

**Prérequis :** Avoir le sprite PNG de patte de chat importé dans Unity (peu importe le nom, on le référencera via l'Inspector).

**Step 1 : Créer le script**

```csharp
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
        if (pawImage != null)
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
```

**Step 2 : Setup dans la scène Menu**

Dans Unity, pour **chaque bouton** du menu :
1. Ajouter un child GameObject nommé `Patte`
2. Ajouter un composant `Image` sur ce child
3. Assigner le sprite de patte dans le champ `Source Image`
4. Dans le `Rect Transform` : ancre à gauche du bouton, position X ≈ -60, Y = 0, Width/Height selon la taille souhaitée (ex: 50x50)
5. Décocher `Raycast Target` sur l'Image pour ne pas bloquer les clics
6. Attacher `MenuButtonHover` sur le **bouton parent**
7. Glisser l'Image "Patte" dans le champ `Paw Image` du composant

**Step 3 : Tester en Play Mode**

- Lancer la scène Menu
- Passer la souris sur un bouton → la patte apparaît en fondu
- Retirer la souris → la patte disparaît en fondu
- Vérifier qu'on peut toujours cliquer le bouton normalement

**Step 4 : Commit**

```bash
git add Assets/SCRIPT/MenuButtonHover.cs Assets/SCRIPT/MenuButtonHover.cs.meta
git commit -m "feat: add cat paw hover effect on menu buttons"
```

---

### Task 2 : Créer le composant CatPawTrail

**Files:**
- Create: `Assets/SCRIPT/CatPawTrail.cs`

**Step 1 : Créer le script**

```csharp
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
    [SerializeField] private float lateralOffset = 30f;   // décalage gauche/droite pour alterner les pattes
    [SerializeField] private float angleVariation = 15f;  // degrés de variation aléatoire

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
        return null; // pool épuisé, on skip
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
        // Choisir un coin de départ aléatoire et la direction vers le coin opposé
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

            // Rotation dans le sens de la marche
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

        // 4 coins possibles
        Vector2[] corners = {
            new(-hw, -hh), // bas-gauche
            new( hw, -hh), // bas-droite
            new(-hw,  hh), // haut-gauche
            new( hw,  hh)  // haut-droite
        };

        int startIdx = Random.Range(0, 4);
        int endIdx = (startIdx + 2) % 4; // coin opposé en diagonale
        // Légère variation : parfois on prend un coin adjacent opposé
        if (Random.value > 0.5f)
            endIdx = 3 - startIdx;

        Vector2 start = corners[startIdx];
        Vector2 dir = (corners[endIdx] - corners[startIdx]).normalized;
        return (start, dir);
    }

    private IEnumerator PawLifecycle(Image paw)
    {
        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            paw.canvasRenderer.SetAlpha(elapsed / fadeInDuration);
            yield return null;
        }
        paw.canvasRenderer.SetAlpha(1f);

        // Visible
        yield return new WaitForSeconds(visibleDuration);

        // Fade out
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
```

**Step 2 : Setup dans la scène Menu**

1. Dans le Canvas du menu, créer un GameObject vide nommé `CatPawTrail`
2. Attacher le composant `CatPawTrail`
3. Assigner dans l'Inspector :
   - `Paw Sprite` : le sprite PNG de patte de chat
   - `Canvas` : le Canvas parent
4. Ajuster les valeurs selon le rendu voulu (spawn interval, nombre de pas, taille...)
5. Le mettre en bas de la hiérarchie du Canvas pour qu'il s'affiche derrière les boutons (ou au-dessus si souhaité)

**Step 3 : Tester en Play Mode**

- Lancer la scène Menu
- Attendre ~8 secondes → une traversée de 6 pattes apparaît en diagonale depuis un coin
- Vérifier l'alternance gauche/droite des pattes
- Vérifier que le fade in/out fonctionne
- Vérifier que les pattes ne bloquent pas les clics sur les boutons

**Step 4 : Commit**

```bash
git add Assets/SCRIPT/CatPawTrail.cs Assets/SCRIPT/CatPawTrail.cs.meta
git commit -m "feat: add decorative cat paw trail animation on menu"
```

---

### Task 3 : Écrire le design doc et mettre à jour PROGRESS.md

**Step 1 : Sauvegarder le design doc**

Fichier déjà présent : `docs/plans/2026-06-05-menu-cat-paw-effects.md` (ce fichier).

**Step 2 : Mettre à jour PROGRESS.md**

Ajouter les deux features comme "En cours" ou "Terminé" selon l'avancement.

**Step 3 : Commit final**

```bash
git add docs/plans/2026-06-05-menu-cat-paw-effects.md
git add PROGRESS.md
git commit -m "docs: add cat paw menu effects plan and update progress"
```
