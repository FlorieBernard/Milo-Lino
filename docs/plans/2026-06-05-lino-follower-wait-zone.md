# Lino Follower + Wait Zone Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Lino suit Milo avec un décalage temporel (activable par zone), une WaitForLinoZone bloque Milo en fin de niveau jusqu'à ce que Lino arrive, et CatPawTrail traverse toujours de bord à bord en passant près du centre.

**Architecture:** `LinoFollower.cs` (existant) est réécrit pour utiliser un buffer de positions horodatées — il expose `SetActive(bool)` pour contrôle externe. `WaitForLinoZone.cs` (nouveau) est un trigger 2D qui gère la séquence freeze/switch/unfreeze. `CharacterSwitcher` reçoit `ForceLino()`. `CatPawTrail.GetRandomTrailPath()` est remplacé pour partir du bord gauche/droit uniquement.

**Tech Stack:** Unity 2D, C#, Rigidbody2D, Trigger Collider2D, Queue/List buffer

---

### Task 1 : Réécrire LinoFollower.cs

**Files:**
- Modify: `Assets/SCRIPT/LinoFollower.cs`

**Ce que le script existant fait déjà :**
- Buffer de positions de Milo
- MoveTowards pour suivre
- Tag "Corridor" pour activer

**Ce qui change :**
- Buffer horodaté (enregistrer `(Vector3 pos, float time)` chaque FixedUpdate)
- Lire la position de Milo il y a exactement `_followDelay` secondes
- `SetActive(bool)` pour contrôle externe (garde la compatibilité Corridor)
- Enregistrement permanent (même inactif) pour avoir l'historique prêt à l'activation

**Step 1 : Remplacer le contenu du fichier**

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attacher sur Lino. Lino suit la position que Milo occupait il y a _followDelay secondes.
/// Activable via SetActive(bool) ou par trigger tagué "Corridor".
/// </summary>
public class LinoFollower : MonoBehaviour
{
    [Header("Référence")]
    [SerializeField] private Transform _milo;

    [Header("Paramètres")]
    [SerializeField] private float _followDelay = 0.5f;
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _minDistance = 0.5f;

    public bool IsActive { get; private set; }

    private Rigidbody2D _linoRb;
    private Rigidbody2D _miloRb;
    private readonly List<(Vector3 pos, float time)> _history = new();

    private void Start()
    {
        _linoRb = GetComponent<Rigidbody2D>();
        if (_milo != null)
            _miloRb = _milo.GetComponent<Rigidbody2D>();
    }

    /// <summary>Active ou désactive le suivi. La désactivation vide l'historique.</summary>
    public void SetActive(bool active)
    {
        IsActive = active;
        if (!active) _history.Clear();
    }

    private void FixedUpdate()
    {
        if (_milo == null) return;

        // Enregistrer chaque frame (buffer max = _followDelay + 1s)
        _history.Add((_milo.position, Time.time));
        while (_history.Count > 0 && Time.time - _history[0].time > _followDelay + 1f)
            _history.RemoveAt(0);

        if (!IsActive || _history.Count == 0) return;

        // Trouver la position de Milo il y a ~_followDelay secondes
        Vector3 targetPos = _history[0].pos;
        for (int i = _history.Count - 1; i >= 0; i--)
        {
            if (Time.time - _history[i].time >= _followDelay)
            {
                targetPos = _history[i].pos;
                break;
            }
        }

        // Garder le Y de Lino quand Milo est au sol (Lino gère sa propre gravité)
        bool miloAirborne = _miloRb != null && Mathf.Abs(_miloRb.linearVelocity.y) > 0.1f;
        if (!miloAirborne)
            targetPos.y = transform.position.y;
        targetPos.z = transform.position.z;

        float dist = Vector2.Distance(transform.position, targetPos);
        if (dist <= _minDistance) return;

        Vector2 newPos = Vector2.MoveTowards(transform.position, targetPos, _speed * Time.fixedDeltaTime);
        _linoRb.MovePosition(newPos);

        float scaleX = targetPos.x < transform.position.x
            ? -Mathf.Abs(transform.localScale.x)
            : Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(scaleX, transform.localScale.y, 1f);
    }

    // Compatibilité avec l'ancien système Corridor
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Corridor")) SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Corridor")) SetActive(false);
    }
}
```

**Step 2 : Tester en Play Mode (Corridor existant)**

Si une scène utilise déjà le tag Corridor : vérifier que Lino suit toujours Milo dans la zone.

**Step 3 : Commit**

```bash
git add Assets/SCRIPT/LinoFollower.cs
git commit -m "refactor: LinoFollower - horodated buffer + SetActive API"
```

---

### Task 2 : Ajouter ForceLino() dans CharacterSwitcher

**Files:**
- Modify: `Assets/SCRIPT/CharacterSwitcher.cs`

**Contexte :** `ForceMilo()` existe déjà (ligne 80). `SwitchCharacter()` est privée. `ForceLino()` est symétrique.

**Step 1 : Ajouter la méthode après ForceMilo()**

Ouvrir `Assets/SCRIPT/CharacterSwitcher.cs`, repérer le bloc `ForceMilo()` et ajouter juste après :

```csharp
/// <summary>Forces a switch to Lino if Milo is currently active.</summary>
public void ForceLino()
{
    if (!_isPlayingMilo) return;
    SwitchCharacter();
}
```

**Step 2 : Tester manuellement en Play Mode**

Ajouter un appel temporaire dans un script de test ou via la console Unity pour vérifier que Tab et ForceLino() donnent le même résultat.

**Step 3 : Commit**

```bash
git add Assets/SCRIPT/CharacterSwitcher.cs
git commit -m "feat: add ForceLino() to CharacterSwitcher"
```

---

### Task 3 : Créer WaitForLinoZone.cs

**Files:**
- Create: `Assets/SCRIPT/WaitForLinoZone.cs`

**Comportement :**
- **Milo entre (phase IDLE)** → freeze Milo, LinoFollower off, switch à Lino, phase = MILO_WAITING
- **Lino entre (phase MILO_WAITING)** → unfreeze Milo, LinoFollower on, switch à Milo, zone désactivée

**Step 1 : Créer le script**

```csharp
using UnityEngine;

/// <summary>
/// Zone trigger de fin de niveau : bloque Milo jusqu'à ce que Lino arrive.
///
/// Setup dans l'Inspector :
///   - Assigner CharacterSwitcher (le GameObject CharacterSwitcher de la scène)
///   - Assigner LinoFollower (le composant sur Lino)
///   - Assigner MiloRb (le Rigidbody2D de Milo)
///   - Mettre un Collider2D en mode Trigger sur ce GameObject
///   - Les tags "Milo" et "Lino" doivent être corrects sur les personnages
/// </summary>
public class WaitForLinoZone : MonoBehaviour
{
    [SerializeField] private CharacterSwitcher _switcher;
    [SerializeField] private LinoFollower _linoFollower;
    [SerializeField] private Rigidbody2D _miloRb;

    private bool _miloWaiting = false;
    private RigidbodyConstraints2D _miloOriginalConstraints;

    private void Awake()
    {
        if (_miloRb != null)
            _miloOriginalConstraints = _miloRb.constraints;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_miloWaiting && other.CompareTag("Milo"))
            OnMiloEntered();
        else if (_miloWaiting && other.CompareTag("Lino"))
            OnLinoEntered();
    }

    private void OnMiloEntered()
    {
        _miloWaiting = true;
        _miloRb.linearVelocity = Vector2.zero;
        _miloRb.constraints = RigidbodyConstraints2D.FreezeAll;
        _linoFollower?.SetActive(false);
        _switcher?.ForceLino();
    }

    private void OnLinoEntered()
    {
        _miloWaiting = false;
        _miloRb.constraints = _miloOriginalConstraints;
        _linoFollower?.SetActive(true);
        _switcher?.ForceMilo();
        gameObject.SetActive(false);
    }
}
```

**Step 2 : Setup dans la scène**

1. Créer un GameObject vide à la position voulue (fin de niveau)
2. Ajouter un `BoxCollider2D`, cocher `Is Trigger`, ajuster la taille
3. Attacher `WaitForLinoZone`
4. Assigner dans l'Inspector :
   - `CharacterSwitcher` : le GameObject qui porte ce script
   - `LinoFollower` : le composant sur Lino
   - `Milo Rb` : le Rigidbody2D de Milo

**Step 3 : Tester en Play Mode**

- Milo entre dans la zone → Milo freezé, switch à Lino automatique
- Lino entre dans la zone → Milo libéré, switch à Milo, zone désactivée
- Vérifier que le LinoFollower se réactive bien après

**Step 4 : Commit**

```bash
git add Assets/SCRIPT/WaitForLinoZone.cs
git commit -m "feat: add WaitForLinoZone - wait for Lino before continuing"
```

---

### Task 4 : Fix CatPawTrail - traversée bord à bord près du centre

**Files:**
- Modify: `Assets/SCRIPT/CatPawTrail.cs`

**Problème actuel :** `GetRandomTrailPath()` part d'un coin vers le coin opposé → trajectoire diagonale qui peut passer loin du centre et être coupée par les bords de l'écran.

**Fix :** Partir du bord gauche OU droit (Y aléatoire proche du centre ±30%), aller vers le bord opposé.

**Step 1 : Remplacer GetRandomTrailPath()**

Trouver la méthode `GetRandomTrailPath()` dans `CatPawTrail.cs` et la remplacer par :

```csharp
private (Vector2 start, Vector2 dir) GetRandomTrailPath()
{
    var rt = canvas.GetComponent<RectTransform>();
    float hw = rt.rect.width / 2f;
    float hh = rt.rect.height / 2f;

    // Toujours de gauche à droite ou de droite à gauche
    // Y de départ/arrivée proche du centre (±30% de la hauteur)
    float startY = Random.Range(-hh * 0.3f, hh * 0.3f);
    float endY = Random.Range(-hh * 0.3f, hh * 0.3f);

    bool leftToRight = Random.value > 0.5f;
    Vector2 start = new Vector2(leftToRight ? -hw : hw, startY);
    Vector2 end = new Vector2(leftToRight ? hw : -hw, endY);

    return (start, (end - start).normalized);
}
```

**Step 2 : Tester en Play Mode (Menu)**

- Lancer la scène Menu
- Attendre ~10s → une traversée apparaît de bord gauche/droit
- Vérifier qu'elle passe toujours près du centre
- Vérifier l'alternance gauche/droite des pattes et le flip de sprite

**Step 3 : Commit**

```bash
git add Assets/SCRIPT/CatPawTrail.cs
git commit -m "fix: CatPawTrail always crosses side-to-side near screen center"
```

---

### Task 5 : Mettre à jour PROGRESS.md

**Step 1 : Mettre à jour PROGRESS.md**

```markdown
## État actuel

### Features terminées
- Cat paw menu effects : scripts créés, setup Unity à faire
- Fix landing stuck in Fall state

### En cours
- LinoFollower amélioré (buffer horodaté + SetActive)
- WaitForLinoZone (nouveau)
- CatPawTrail fix traversée centre

### Setup Unity restant
- MenuButtonHover : assigner Paw Image sur chaque bouton
- CatPawTrail : assigner PawSprite + Canvas dans l'Inspector
- WaitForLinoZone : placer dans la scène, assigner les références
```

**Step 2 : Commit**

```bash
git add PROGRESS.md docs/plans/2026-06-05-lino-follower-wait-zone.md
git commit -m "docs: add lino follower + wait zone plan and update progress"
```
