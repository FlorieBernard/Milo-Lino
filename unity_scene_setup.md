# Unity Scene Setup Guide — Milo-Lino

## Nombre de scènes : 8

| # | Nom de scène | Type |
|---|---|---|
| 1 | `Menu` | Menu principal |
| 2 | `Debut` | Texte d'intro |
| 3 | `Level1` | Niveau 1 |
| 4 | `Corridor1` | Couloir entre N1 et N2 |
| 5 | `Level2` | Niveau 2 |
| 6 | `Corridor2` | Couloir entre N2 et N3 |
| 7 | `Level3` | Niveau 3 |
| 8 | `Fin` | Texte d'outro |

---

## Avant de commencer — Réglages globaux Unity

### Build Settings (`File > Build Settings`)
- [ ] Ajouter les 8 scènes dans cet ordre exact : `Menu`, `Debut`, `Level1`, `Corridor1`, `Level2`, `Corridor2`, `Level3`, `Fin`

### Tags (`Edit > Project Settings > Tags and Layers`)
- [ ] Créer le tag `Milo`
- [ ] Créer le tag `Lino`
- [ ] Créer le tag `Corridor` (renommer/supprimer l'ancien `Couloir`)
- [ ] Créer le tag `Ice`

---

## Scène : `Menu`

### GameObjects nécessaires
- [ ] **Canvas UI** avec boutons "Jouer" et "Quitter"
- [ ] **GameObject vide** avec `Menu.cs`
  - Bouton "Jouer" → appelle `Menu.PlayGame()`
  - Bouton "Quitter" → appelle `Menu.QuitGame()`
- [ ] **GameObject vide** avec `SceneMusic.cs`
  - `_musicClip` : musique du menu principal

> Pas de GameManager ni AudioManager ici. Ils sont créés dans la scène `Debut`.

---

## Scène : `Debut`

### GameObjects nécessaires
- [ ] **GameManager** — GameObject vide avec `GameManager.cs`
  - `_sceneOrder` : `Debut`, `Level1`, `Corridor1`, `Level2`, `Corridor2`, `Level3`, `Fin`, `Menu`

- [ ] **AudioManager** — GameObject vide avec `AudioManager.cs` *(persistant comme GameManager)*
  - `_sounds` : ajouter les SFX au fur et à mesure (noms suggérés : `Jump`, `Land`, `FireflyCatch`)
  - `_musicVolume` : ex. `0.8`
  - `_fadeDuration` : ex. `1` *(durée du fondu entre deux musiques)*

- [ ] **SceneFader** — GameObject vide avec `SceneFader.cs` *(persistant)*
  - `_fadeDuration` : ex. `0.5` *(durée du fondu au noir entre scènes)*
  - `_fadeColor` : noir par défaut
  - > Aucun autre réglage — l'overlay est créé automatiquement en code

- [ ] **SceneMusic** — GameObject vide avec `SceneMusic.cs`
  - `_musicClip` : musique de la scène Debut (optionnel)

- [ ] **Canvas UI** avec un `TextMeshProUGUI`
- [ ] **TypingEffect** — GameObject avec `TypingEffect.cs`
  - `_phrases` : remplir avec le texte d'intro
  - `_textDisplay` : assigner le TextMeshProUGUI
  - `_loadNextSceneOnComplete` : ✅ coché
  - `_sceneLoadDelay` : ex. `2`

---

## Scènes : `Level1`, `Level2`, `Level3` *(identique pour chaque niveau)*

### Milo
- [ ] **GameObject "Milo"** — tag : `Milo`
  - `Rigidbody2D` — Gravity Scale: 3, Freeze Rotation Z ✅
  - `CapsuleCollider2D`
  - `SpriteRenderer`
  - `PlayerMovementMilo.cs`
    - `_rb` : Rigidbody2D de Milo
    - `_groundCheck` : Transform enfant positionné sous les pieds
    - `_groundLayer` : sélectionner le layer "Ground"
    - `_runVFX` : assigner un ParticleSystem enfant (optionnel)
    - `_jumpVFX` : assigner un ParticleSystem enfant (optionnel)
    - `smokePrefab` : assigner le prefab de fumée (optionnel)
  - `RespawnOnFall.cs`
    - `_deathHeight` : ex. `-15`

### Lino
- [ ] **GameObject "Lino"** — tag : `Lino`
  - `Rigidbody2D` — Gravity Scale: 3, Freeze Rotation Z ✅
  - `CapsuleCollider2D`
  - `SpriteRenderer`
  - `PlayerMovementLino.cs`
    - `_rb`, `_groundCheck`, `_groundLayer` : même logique que Milo
    - `_runVFX`, `_jumpVFX` : assigner des ParticleSystem enfants (optionnel)
  - `LinoBlocker.cs` *(Lino démarre bloqué)*
  - `RespawnOnFall.cs`

### CharacterSwitcher
- [ ] **GameObject vide** avec `CharacterSwitcher.cs`
  - `_miloMovement` : PlayerMovementMilo de Milo
  - `_linoMovement` : PlayerMovementLino de Lino
  - `_miloCollider` : Collider de Milo
  - `_linoCollider` : Collider de Lino
  - `_miloSprite` : SpriteRenderer de Milo
  - `_linoSprite` : SpriteRenderer de Lino
  - `_mainCamera` : Camera principale
  - `_miloTransform` : Transform de Milo
  - `_switchingEnabled` : ✅ coché
  - `_linoOnlyObjects` : objets visibles uniquement via Lino (si applicable)

### Luciole (autant que nécessaire)
- [ ] **GameObject "Firefly"** pour chaque luciole
  - `SpriteRenderer`
  - `CircleCollider2D` — **Is Trigger** : ✅
  - `Firefly.cs`
    - `_visibleTo` : `Milo` ou `Lino`
    - `_characterSwitcher` : assigner CharacterSwitcher
    - `_obstacleToDestroy` : l'objet qui bloque le passage
    - `_linoBlocker` : LinoBlocker de Lino

### Obstacle bloquant le passage
- [ ] **GameObject "Obstacle"** (plateforme ou mur)
  - `SpriteRenderer` + `Collider2D`
  - *(Référencé dans Firefly._obstacleToDestroy)*

### Zone de sortie
- [ ] **GameObject vide "ExitZone"** en fin de niveau
  - `BoxCollider2D` — **Is Trigger** : ✅
  - `ExitZone.cs`

### Caméra
- [ ] **Camera** avec `CameraFollow.cs`
  - `_milo` : Transform de Milo
  - `_lino` : Transform de Lino
  - `_isGreatRoom` : ❌ décoché

### Musique
- [ ] **GameObject vide** avec `SceneMusic.cs`
  - `_musicClip` : musique du niveau *(si vide, la musique précédente s'arrête)*
  - > Si les 3 niveaux ont la même musique → assigner le même clip, il ne sera pas redémarré

### Optionnel
- [ ] **Plateformes mobiles** : `MovingPlatform.cs`
- [ ] **Surfaces glissantes** : tag `Ice` sur la plateforme *(aucun script, juste le tag)*
- [ ] **Parallaxe** : `Parallax.cs` sur les calques de fond — régler `_parallaxEffect` entre 0 et 1
- [ ] **Zones de dialogue** : `DialogueZone.cs` + trigger + UI
- [ ] **Menu pause** : `MenuPause.cs` + container UI assigné à `_container`

---

## Scènes : `Corridor1`, `Corridor2` *(identique pour chaque couloir)*

### Milo
- [ ] Même configuration que dans les niveaux

### Lino
- [ ] Même configuration que dans les niveaux **SANS** `LinoBlocker.cs`
- [ ] **Ajouter** `LinoFollower.cs`
  - `_milo` : Transform de Milo
  - `_speed`, `_minDistance`, `_followDelay` : ajuster au goût

### Zone Corridor (trigger de suivi)
- [ ] **GameObject vide** couvrant toute la zone jouable
  - `BoxCollider2D` — **Is Trigger** : ✅, tag : `Corridor`
  - `CorridorZone.cs`
    - `_characterSwitcher` : assigner CharacterSwitcher

### CharacterSwitcher
- [ ] Même configuration que dans les niveaux, **SAUF** :
  - `_switchingEnabled` : ❌ décoché *(pas de switch au Tab dans les couloirs)*

### Zone de sortie
- [ ] `ExitZone.cs` en fin de couloir

### Musique
- [ ] **GameObject vide** avec `SceneMusic.cs`
  - `_musicClip` : musique du couloir *(peut être la même que le niveau)*

---

## Scène : `Fin`

- [ ] **Canvas UI** avec `TextMeshProUGUI`
- [ ] **TypingEffect** avec `TypingEffect.cs`
  - `_phrases` : texte de fin
  - `_loadNextSceneOnComplete` : ✅ coché *(chargera `Menu` si c'est le dernier dans `_sceneOrder`)*
- [ ] **GameObject vide** avec `SceneMusic.cs`
  - `_musicClip` : musique de fin *(ou vide pour silence)*

---

## Récapitulatif des scripts par scène

| Script | Menu | Debut | Level | Corridor | Fin |
|---|:---:|:---:|:---:|:---:|:---:|
| `GameManager` | | ✅ | | | |
| `AudioManager` | | ✅ | | | |
| `SceneFader` | | ✅ | | | |
| `SceneMusic` | ✅ | ✅ | ✅ | ✅ | ✅ |
| `Menu` | ✅ | | | | |
| `MenuPause` | | | ✅ | ✅ | |
| `TypingEffect` | | ✅ | | | ✅ |
| `PlayerMovementMilo` | | | ✅ | ✅ | |
| `PlayerMovementLino` | | | ✅ | ✅ | |
| `CharacterSwitcher` | | | ✅ | ✅ | |
| `LinoBlocker` | | | ✅ | | |
| `LinoFollower` | | | | ✅ | |
| `CorridorZone` | | | | ✅ | |
| `Firefly` | | | ✅ | | |
| `ExitZone` | | | ✅ | ✅ | |
| `RespawnOnFall` | | | ✅ | ✅ | |
| `CameraFollow` | | | ✅ | ✅ | |
| `MovingPlatform` | | | ✅ | | |
| `Parallax` | | | ✅ | ✅ | |
| `DialogueZone` | | | ✅ | | |
