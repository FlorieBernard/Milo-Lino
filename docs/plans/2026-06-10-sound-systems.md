# Sound Systems Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Ajouter le son d'approche luciole en loop dans `Firefly.cs` (les 3 autres sons — Jump, Land, FireflyCatch — sont déjà câblés en code et n'attendent que leurs AudioClips dans l'Inspector Unity).

**Architecture:** `Firefly.cs` utilise `Physics2D.OverlapCircle` dans `Update()` pour détecter si un joueur est dans le rayon de proximité — même pattern que `IsGrounded()` dans `PlayerMovementBase`. Un bool `_isPlayerNearby` évite d'appeler `Play`/`Stop` à chaque frame. Le son "FireflyProximity" est stopé dans `Catch()` pour éviter qu'il continue après ramassage.

**Tech Stack:** Unity 2D, C#, AudioManager singleton (pattern `Play(string)` / `Stop(string)`), `Physics2D.OverlapCircle`

---

### Task 1 : Modifier `Firefly.cs` — détection de proximité et son loop

**Files:**
- Modify: `Assets/SCRIPT/Firefly.cs`

**Step 1 : Ajouter les champs sérialisés dans `Firefly.cs`**

Après le `[Header("On Catch")]` existant (ligne ~22), ajouter :

```csharp
[Header("Proximity Sound")]
[SerializeField] private float _proximityRadius = 3f;
[SerializeField] private LayerMask _playerLayer;
```

Ajouter le champ privé après `private bool _caught = false;` :

```csharp
private bool _isPlayerNearby = false;
```

**Step 2 : Ajouter la méthode `HandleProximitySound()`**

Ajouter avant `Catch()` :

```csharp
private void HandleProximitySound()
{
    bool nearby = Physics2D.OverlapCircle(transform.position, _proximityRadius, _playerLayer) != null;
    if (nearby == _isPlayerNearby) return;
    _isPlayerNearby = nearby;
    if (nearby)
        AudioManager.Instance?.Play("FireflyProximity");
    else
        AudioManager.Instance?.Stop("FireflyProximity");
}
```

**Step 3 : Appeler `HandleProximitySound()` dans `Update()`**

Dans `Update()`, juste avant la gestion de la visibilité (ou en fin de méthode), ajouter :

```csharp
if (!_caught) HandleProximitySound();
```

La condition `if (_caught) return;` existante en haut de `Update()` gère déjà le cas post-ramassage — mais on appelle explicitement avant le return pour être sûr. En réalité, `if (_caught) return;` est déjà là, donc `HandleProximitySound()` n'est jamais appelé après catch. C'est correct.

**Step 4 : Stopper le son dans `Catch()`**

Dans `Catch()`, avant `gameObject.SetActive(false)`, ajouter :

```csharp
AudioManager.Instance?.Stop("FireflyProximity");
_isPlayerNearby = false;
```

**Step 5 : Vérifier le fichier final**

Le `Update()` doit ressembler à :

```csharp
private void Update()
{
    if (_caught) return;

    // Floating animation
    float yOffset = Mathf.Sin(Time.time * _floatSpeed) * _floatAmplitude;
    transform.position = _startPosition + new Vector3(0f, yOffset, 0f);

    // Show only to the correct cat
    if (_renderer != null && _characterSwitcher != null)
    {
        bool shouldBeVisible = _visibleTo == CatTarget.Milo
            ? _characterSwitcher.IsPlayingMilo
            : !_characterSwitcher.IsPlayingMilo;
        _renderer.enabled = shouldBeVisible;
    }

    HandleProximitySound();
}
```

Et `Catch()` :

```csharp
private void Catch()
{
    _caught = true;

    AudioManager.Instance?.Stop("FireflyProximity");
    _isPlayerNearby = false;
    AudioManager.Instance?.Play("FireflyCatch");

    if (_obstacleToDestroy != null)
        Destroy(_obstacleToDestroy);

    if (_linoBlocker != null)
        _linoBlocker.Unblock();

    gameObject.SetActive(false);
    Destroy(gameObject);
}
```

**Step 6 : Commit**

```bash
git add Assets/SCRIPT/Firefly.cs
git commit -m "feat: add firefly proximity loop sound"
```

---

### Task 2 : Setup Inspector Unity (manuel — hors scope code)

> Ces étapes sont à faire dans l'éditeur Unity. Elles ne nécessitent aucune modification de code.

**Sur le GameObject AudioManager (scène "Debut") :**

Ajouter 4 entrées dans `_sounds[]` :

| Name | AudioClip | Volume | Pitch | Loop |
|------|-----------|--------|-------|------|
| `Jump` | *(clip saut chat)* | 1.0 | 1.0 | false |
| `Land` | *(clip atterrissage)* | 1.0 | 1.0 | false |
| `FireflyCatch` | *(clip ramassage luciole)* | 1.0 | 1.0 | false |
| `FireflyProximity` | *(clip ambiance luciole)* | 0.6 | 1.0 | **true** |

**Sur chaque prefab/GameObject Firefly :**

- Assigner `Player Layer` dans le champ `_playerLayer` (layer des chats joueurs)
- Ajuster `_proximityRadius` si besoin (défaut : 3 unités)

---

### Récapitulatif des sons et leur état

| Son | Câblé en code | AudioClip à assigner |
|-----|:---:|:---:|
| Saut (`Jump`) | ✅ `PlayerMovementBase:165` | ⬜ Inspector |
| Atterrissage (`Land`) | ✅ `PlayerMovementBase:229` | ⬜ Inspector |
| Ramassage luciole (`FireflyCatch`) | ✅ `Firefly:72` | ⬜ Inspector |
| Approche luciole (`FireflyProximity`) | ⬜ Task 1 | ⬜ Inspector |
