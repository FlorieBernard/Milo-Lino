# Unity Scene Setup — Milo & Lino

Guide de configuration des scènes pour reproduire le projet à partir des scripts existants.
À compléter au fur et à mesure des sessions.

---

## Table des matières

1. [Structure des scènes](#1-structure-des-scènes)
2. [SceneFader — Transitions entre scènes](#2-scenefader--transitions-entre-scènes)
3. [CharacterSwitcher — Changement de personnage](#3-characterswitcher--changement-de-personnage)
4. [DialogueZone — Zones de dialogue](#4-dialoguezone--zones-de-dialogue)
5. [SpriteAnimator — Animations décoratives](#5-spriteanimator--animations-décoratives)
6. [PlayerMovementBase — Coyote Time](#6-playermovementbase--coyote-time)
7. [ExitZone — Passage au niveau suivant](#7-exitzone--passage-au-niveau-suivant)
8. [MenuPause — Menu pause](#8-menupause--menu-pause)
9. [OptionsMenu — Menu options](#9-optionsmenu--menu-options)
10. [LocalizationManager — Système de langue](#10-localizationmanager--système-de-langue)
11. [LocalizedText — Texte UI localisé](#11-localizedtext--texte-ui-localisé)

---

## 1. Structure des scènes

Le projet contient plusieurs scènes. La scène **Debut** est la scène de démarrage et contient les singletons persistants.

```
Debut       ← scène de lancement (GameManager, SceneFader, AudioManager, LocalizationManager)
Level1
Level2
Level3
Fin
```

> Les GameObjects avec `DontDestroyOnLoad` doivent être placés **uniquement dans Debut**, jamais dans les niveaux.

### GameManager — ordre des scènes

Le `GameManager` dans la scène Debut a un tableau `_sceneOrder` configurable dans l'Inspector :

```
Debut → Level1 → Level2 → Level3 → Fin
```

Pour ajouter un niveau : ajouter son nom dans le tableau. L'`ExitZone` de chaque niveau appelle automatiquement `LoadNextScene()`.

---

## 2. SceneFader — Transitions entre scènes

**Script :** `Assets/SCRIPT/SceneFader.cs`
**Placement :** GameObject vide dans la scène **Debut**, nommé par exemple `SceneFader`.

### Setup

1. Créer un **GameObject vide** dans la scène Debut.
2. Lui attacher le script `SceneFader`.
3. Aucun Canvas ou Image à créer manuellement — le script les génère à l'exécution.

### Paramètres Inspector

| Champ | Description | Valeur par défaut |
|---|---|---|
| **Style** | Type de transition (Fade, SlideLeft, SlideRight, SlideUp, SlideDown) | Fade |
| **Transition Color** | Couleur du panneau de transition | Noir |
| **Fade Out Duration** | Durée de la sortie (avant chargement) | 0.5s |
| **Hold Duration** | Pause écran plein avant le fade-in | 0.1s |
| **Fade In Duration** | Durée d'apparition de la scène | 0.5s |
| **Fade In Delay** | Délai avant le fade-in | 0s |
| **Curve** | Courbe d'easing (AnimationCurve) | EaseInOut |

### Utilisation dans un script

```csharp
// Depuis n'importe quel script
SceneFader.Instance.FadeToScene("Level2");
```

### Notes

- Le fade-in se déclenche **automatiquement** à chaque chargement de scène via `SceneManager.sceneLoaded`.
- Le `Canvas` créé a un `sortingOrder` de **999** — il s'affiche toujours au-dessus.
- Le panneau bloque les raycasts : `blocksRaycasts = false` (pas d'interférence avec l'UI du jeu).

---

## 3. CharacterSwitcher — Changement de personnage

**Script :** `Assets/SCRIPT/CharacterSwitcher.cs`
**Placement :** GameObject dans chaque scène de jeu (ou sur un manager persistant).

### Setup

1. Créer un **GameObject vide** nommé `CharacterSwitcher` dans la scène.
2. Attacher le script `CharacterSwitcher`.
3. Remplir les références Inspector :

| Champ | Ce qu'il faut glisser |
|---|---|
| **Milo Movement** | Component `PlayerMovementMilo` du GameObject Milo |
| **Lino Movement** | Component `PlayerMovementLino` du GameObject Lino |
| **Milo Collider** | `Collider2D` du GameObject Milo |
| **Lino Collider** | `Collider2D` du GameObject Lino |
| **Milo Sprite** | `SpriteRenderer` du GameObject Milo |
| **Lino Sprite** | `SpriteRenderer` du GameObject Lino |
| **Main Camera** | La caméra principale de la scène |
| **Milo Transform** | Transform du GameObject Milo (auto-détecté si non renseigné) |

### Paramètres visuels

| Champ | Description |
|---|---|
| **Milo Sky Color** | Couleur du fond caméra quand Milo est actif |
| **Lino Active Color** | Couleur du sprite de Lino quand il est actif (jaune) |
| **Lino Sky Color** | Couleur du fond caméra quand Lino est actif (gris) |
| **World Grey Tint** | Teinte appliquée à tous les sprites du monde quand Lino joue |

### Lino Only Objects

- **Lino Only Objects** : liste de GameObjects visibles uniquement quand Lino joue ou que Milo s'en approche.
- **Detection Distance** : distance (en unités Unity) à partir de laquelle l'objet s'active même si Milo joue.

### Comportement

- **Tab** : alterne entre Milo et Lino.
- Quand **Lino est actif** : tout le monde est grisé sauf Lino, Milo, et les Fireflies (composant `Firefly`).
- Le personnage **inactif** a son Rigidbody2D gelé (`FreezeAll`) pour ne pas glisser sur les pentes.

### Appel depuis un script

```csharp
// Forcer le retour sur Milo (ex : fin d'une séquence Lino)
CharacterSwitcher switcher = FindObjectOfType<CharacterSwitcher>();
switcher.ForceMilo();
```

---

## 4. DialogueZone — Zones de dialogue

**Script :** `Assets/SCRIPT/DialogueZone.cs`
**Placement :** Sur un GameObject avec un **Collider2D en mode Trigger**.

### Setup

1. Créer un GameObject (ex: `DialogueTrigger_Intro`).
2. Ajouter un `Collider2D` (Box ou Circle) et cocher **Is Trigger**.
3. Attacher le script `DialogueZone`.
4. Créer un **Canvas UI** dans la scène avec :
   - Un `Panel` (dialoguePanel)
   - Un `Image` pour le portrait
   - Deux `TextMeshProUGUI` : nom du personnage + texte du dialogue
   - *(Optionnel)* Un indicateur "continuer" (flèche, icône...)

### Références Inspector

| Champ | Ce qu'il faut glisser |
|---|---|
| **Dialogue Panel** | Le Panel UI racine du dialogue |
| **Portrait** | Le composant `Image` du portrait |
| **Name Text** | Le `TextMeshProUGUI` du nom |
| **Dialogue Text** | Le `TextMeshProUGUI` du texte |
| **Continue Indicator** | *(Optionnel)* Un GameObject affiché en attente d'input |
| **Milo Portrait** | Sprite portrait de Milo |
| **Lino Portrait** | Sprite portrait de Lino |

### Configuration du dialogue

| Champ | Description |
|---|---|
| **Trigger Target** | Qui peut déclencher (Milo / Lino / Both) |
| **Repeatable** | Si coché, le dialogue se rejoue à chaque entrée |
| **Lines** | Tableau des lignes de texte (fallback sans localisation) |
| **Line Keys** | *(Optionnel)* Clés CSV pour le système de localisation — remplace `Lines` si `LocalizationManager` est présent |
| **Speaker Names** | Tableau des noms (doit correspondre 1:1 avec Lines/LineKeys) |
| **Letter Delay** | Vitesse de l'effet machine à écrire (secondes/lettre) |
| **Line Pause** | Pause entre les lignes si Wait For Input est désactivé |
| **Wait For Input** | Le joueur appuie sur Space/Jump pour passer |
| **Skip Typing On Input** | Appuyer pendant la frappe affiche la ligne entière instantanément |

> **Rétrocompatibilité :** Les DialogueZones existantes avec `Lines` rempli et sans `Line Keys` fonctionnent sans aucune modification.

### Tags requis

Les personnages doivent avoir les tags Unity suivants :
- Milo → tag **`Milo`**
- Lino → tag **`Lino`**

---

## 5. SpriteAnimator — Animations décoratives

**Script :** `Assets/SCRIPT/SpriteAnimator.cs`
**Placement :** Sur n'importe quel GameObject avec un `SpriteRenderer` (plantes, arbres, décos...).

### Setup

1. Sélectionner le GameObject décoratif.
2. Attacher le script `SpriteAnimator` (le `SpriteRenderer` est requis automatiquement).
3. Dans **Frames**, glisser les sprites dans l'ordre d'animation.

### Paramètres

| Champ | Description |
|---|---|
| **Frames** | Tableau de sprites à animer |
| **FPS** | Vitesse de l'animation (images par seconde) |
| **Play Mode** | Loop / PingPong / Once |
| **Random Offset** | Démarre à une frame aléatoire (évite la synchronisation visuelle entre objets similaires) |
| **Play On Start** | Démarre automatiquement |

### API (depuis un script)

```csharp
SpriteAnimator anim = GetComponent<SpriteAnimator>();
anim.Play();    // démarre
anim.Pause();   // met en pause
anim.Stop();    // arrête et remet à la frame 0
```

---

## 6. PlayerMovementBase — Coyote Time

**Script :** `Assets/SCRIPT/PlayerMovementBase.cs`
Ce paramètre est hérité par `PlayerMovementMilo` et `PlayerMovementLino`.

### Concept

Le **coyote time** permet au joueur de sauter pendant une courte fenêtre après avoir quitté une plateforme (comme Wile E. Coyote dans les cartoons). Cela rend les sauts en bord de plateforme plus indulgents.

### Paramètre Inspector

| Champ | Description | Valeur par défaut |
|---|---|---|
| **Coyote Time** | Durée (secondes) pendant laquelle on peut encore sauter après avoir quitté le sol | 0.15s |

> Ce champ apparaît sur les composants `PlayerMovementMilo` et `PlayerMovementLino` dans l'Inspector, car ils héritent de `PlayerMovementBase`.

### Notes

- Le timer se réinitialise à chaque saut pour éviter les double-sauts en l'air.
- Réduire à `0` désactive complètement le coyote time.

---

---

## 7. ExitZone — Passage au niveau suivant

**Script :** `Assets/SCRIPT/ExitZone.cs`
**Placement :** Sur un GameObject avec un **Collider2D en mode Trigger**, à placer librement dans le niveau.

### Setup

1. Créer un GameObject (ex: `ExitZone`).
2. Ajouter un `Collider2D` (Box ou Circle) et cocher **Is Trigger**.
3. Attacher le script `ExitZone`.
4. Positionner la zone à l'endroit voulu dans le niveau (sortie, porte, bord...).

### Comportement

- Quand **Milo** entre dans la zone → `GameManager.LoadNextScene()` est appelé.
- Le `GameManager` charge la scène suivante dans son tableau `_sceneOrder`.
- La transition est animée via `SceneFader` si présent.

> Seul Milo peut déclencher la zone (Lino suit toujours Milo).

---

## 8. MenuPause — Menu pause

**Script :** `Assets/SCRIPT/MenuPause.cs`
**Placement :** Sur un GameObject dans chaque scène de jeu (ou sur un objet persistant).

### Setup

1. Créer un **Canvas UI** avec un Panel enfant (le menu pause).
2. Dans le Panel, ajouter les boutons : **Resume**, **Options**, **Main Menu**, **Quit**.
3. Créer un GameObject vide `MenuPause` et attacher le script.
4. Dans l'Inspector, assigner :
   - `_container` → le Panel racine du menu pause
   - `_optionsMenu` → le composant `OptionsMenu` *(optionnel)*

### Wiring des boutons

| Bouton | Méthode à appeler |
|---|---|
| Resume | `MenuPause.ResumeButton()` |
| Options | `MenuPause.OptionsButton()` |
| Main Menu | `MenuPause.MainMenuButton()` |
| Quit | `MenuPause.QuitGame()` |

### Comportement

- **Escape** : alterne entre ouverture et fermeture du menu.
- À l'ouverture : `Time.timeScale = 0` (jeu figé).
- À la fermeture : `Time.timeScale = 1` (jeu repris).

> **Prérequis :** Un **EventSystem** doit être présent dans la scène pour que les boutons fonctionnent. Ajouter via `GameObject > UI > Event System`.

---

## 9. OptionsMenu — Menu options

**Script :** `Assets/SCRIPT/OptionsMenu.cs`
**Placement :** Sur le GameObject racine du panel Options (enfant du Canvas).

### Setup UI

Créer dans le Canvas un Panel `OptionsPanel` contenant :
- Un **Slider** (0→1) pour la musique
- Un **Slider** (0→1) pour les SFX
- Un **TMP_Dropdown** pour la langue
- Un bouton **Retour** qui appelle `OptionsMenu.Hide()`

Attacher `OptionsMenu` au panel root. Assigner dans l'Inspector :

| Champ | Ce qu'il faut glisser |
|---|---|
| **Music Slider** | Le Slider de volume musique |
| **SFX Slider** | Le Slider de volume SFX |
| **Language Dropdown** | Le TMP_Dropdown de langue |
| **Language Codes** | Tableau de codes correspondant aux options du Dropdown (ex: `fr`, `en`) |
| **Panel** | Ce même GameObject (le root du panel) |

### Wiring des callbacks

| Élément | Event | Méthode |
|---|---|---|
| Music Slider | `OnValueChanged` | `OptionsMenu.OnMusicVolumeChanged` |
| SFX Slider | `OnValueChanged` | `OptionsMenu.OnSfxVolumeChanged` |
| Language Dropdown | `OnValueChanged` | `OptionsMenu.OnLanguageChanged` |

### Notes

- Les sliders se positionnent automatiquement aux valeurs sauvegardées à l'ouverture.
- Les codes langue dans `_languageCodes` doivent correspondre **exactement** aux noms de colonnes dans le CSV (ex: `fr`, `en`).

---

## 10. LocalizationManager — Système de langue

**Script :** `Assets/SCRIPT/LocalizationManager.cs`
**Placement :** GameObject vide dans la scène **Debut**, nommé `LocalizationManager`.

### Setup

1. Créer un GameObject vide dans Debut et attacher `LocalizationManager`.
2. Créer les fichiers CSV dans `Assets/Resources/Localization/`.
3. Dans l'Inspector, référencer les fichiers CSV (sans extension, sans `Assets/Resources/`) :

```
Localization/dialogues
```

### Format CSV

```
key,fr,en
intro_01,Bonjour Milo !,Hello Milo!
intro_02,"C'est l'heure, partons !","It's time, let's go!"
```

- Première ligne = en-tête avec les codes de langue
- Première colonne = clé unique
- Champs contenant des virgules ou apostrophes → entourer de guillemets `"`
- Encodage : **UTF-8**

### Ajouter une langue

Ajouter simplement une colonne dans le CSV :

```
key,fr,en,es
intro_01,Bonjour !,Hello!,¡Hola!
```

Puis ajouter `"es"` dans `_languageCodes` de l'`OptionsMenu` et une option dans le Dropdown.

### API

```csharp
LocalizationManager.Instance.Get("intro_01");           // texte courant
LocalizationManager.Instance.SetLanguage("en");          // changer de langue
LocalizationManager.Instance.CurrentLanguage;            // "fr" / "en" / ...
```

---

## 11. LocalizedText — Texte UI localisé

**Script :** `Assets/SCRIPT/LocalizedText.cs`
**Placement :** Sur n'importe quel GameObject avec un `TextMeshProUGUI`.

### Setup

1. Sélectionner le GameObject du texte UI (bouton, label...).
2. Attacher `LocalizedText`.
3. Dans l'Inspector, saisir la **clé CSV** dans le champ `_key`.

Le texte se met à jour automatiquement au chargement et à chaque changement de langue.

> Si `LocalizationManager` est absent de la scène, le texte reste inchangé (pas d'erreur).

---

---

## Checklist Unity Editor — configuration manuelle requise

Les éléments suivants ne peuvent pas être scriptés et doivent être faits directement dans l'éditeur Unity.

### 1. Scène Debut — ajouter LocalizationManager

- Créer un **GameObject vide** nommé `LocalizationManager`
- Attacher le script `LocalizationManager`
- Dans `_csvFiles` : vérifier que `Localization/dialogues` est bien présent

### 2. ExitZone — dans chaque niveau

- Créer un **GameObject** à l'endroit de sortie du niveau
- Ajouter un `Collider2D` → cocher **Is Trigger**
- Attacher le script `ExitZone`
- Répéter pour Level1, Level2, Level3

### 3. MenuPause — ajouter le bouton Options

- Dans le Canvas du menu pause, ajouter un bouton **Options**
- Le wirer sur `MenuPause.OptionsButton()`
- Dans l'Inspector du `MenuPause`, assigner `_optionsMenu` → le composant `OptionsMenu`

### 4. OptionsMenu — créer le Panel UI

Créer dans le Canvas un Panel `OptionsPanel` contenant :

| Élément | Type Unity | Configuration |
|---|---|---|
| Slider musique | `Slider` | Min=0, Max=1 — `OnValueChanged` → `OptionsMenu.OnMusicVolumeChanged` |
| Slider SFX | `Slider` | Min=0, Max=1 — `OnValueChanged` → `OptionsMenu.OnSfxVolumeChanged` |
| Dropdown langue | `TMP_Dropdown` | Options : "Français", "English" — `OnValueChanged` → `OptionsMenu.OnLanguageChanged` |
| Bouton Retour | `Button` | `OnClick` → `OptionsMenu.Hide()` |

Attacher `OptionsMenu` au root du Panel. Assigner dans l'Inspector :
- `_musicSlider`, `_sfxSlider`, `_languageDropdown`, `_panel`
- `_languageCodes` : `fr`, `en` (dans le même ordre que les options du Dropdown)

### 5. CSV dialogues — remplir les vraies clés

Éditer `Assets/Resources/Localization/dialogues.csv` avec les textes du jeu :

```
key,fr,en
nom_de_la_cle,Texte en français,"Texte en anglais"
```

Puis dans chaque `DialogueZone`, remplir le tableau `_lineKeys` avec les clés correspondantes.

*Document mis à jour — 2026-05-28.*
