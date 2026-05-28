# 🐱✨ Unity Scene Setup — Milo & Lino ✨🐱

> 🌸 **Guide de configuration des scènes** — tout ce qu'il faut savoir pour setup le projet depuis zéro !

---

## 📋 Table des matières

| # | Section |
|---|---|
| 1 | [🗂️ Structure des scènes](#1-️-structure-des-scènes) |
| 2 | [🎬 SceneFader — Transitions](#2--scenefader--transitions-entre-scènes) |
| 3 | [🔄 CharacterSwitcher — Changer de perso](#3--characterswitcher--changement-de-personnage) |
| 4 | [💬 DialogueZone — Dialogues](#4--dialoguezone--zones-de-dialogue) |
| 5 | [🌿 SpriteAnimator — Animations déco](#5--spriteanimator--animations-décoratives) |
| 6 | [🐾 PlayerMovement — Coyote Time](#6--playermovementbase--coyote-time) |
| 7 | [🚪 ExitZone — Niveau suivant](#7--exitzone--passage-au-niveau-suivant) |
| 8 | [⏸️ MenuPause — Menu pause](#8-️-menupause--menu-pause) |
| 9 | [⚙️ OptionsMenu — Options](#9-️-optionsmenu--menu-options) |
| 10 | [🌍 LocalizationManager — Langues](#10--localizationmanager--système-de-langue) |
| 11 | [🔤 LocalizedText — Texte UI localisé](#11--localizedtext--texte-ui-localisé) |
| 12 | [🔊 AudioManager — Sons](#12--audiomanager--gestion-du-son) |
| 13 | [🎵 SceneMusic — Musique par scène](#13--scenemusic--musique-par-scène) |
| 14 | [🎥 CameraFollow — Caméra](#14--camerafollow--caméra) |
| 15 | [✨ Firefly — Luciole](#15--firefly--luciole) |
| 16 | [🐾 LinoFollower — Lino suit Milo](#16--linofollower--lino-suit-milo) |
| 17 | [🚧 LinoBlocker — Bloquer Lino](#17--linoblocker--blocage-de-lino) |
| 18 | [💥 ObjectTrigger — Trigger d'objet](#18--objecttrigger--trigger-dobjet) |
| 19 | [🟫 MovingPlatform — Plateforme mobile](#19--movingplatform--plateforme-mobile) |
| 20 | [💀 RespawnOnFall — Respawn](#20--respawnонfall--respawn) |
| 21 | [⌨️ TypingEffect — Effet de frappe](#21-️-typingeffect--effet-de-frappe-introoutro) |
| 22 | [🏠 Menu — Écran titre](#22--menu--écran-titre) |
| 23 | [🌄 Parallax — Fond parallaxe](#23--parallax--fond-parallaxe) |
| 24 | [🌊 ParallaxZone — Zone de transition parallaxe](#24--parallaxzone--zone-de-transition-parallaxe) |
| ✅ | [Checklist Unity Editor](#-checklist-unity-editor--configuration-manuelle) |

---

## 1. 🗂️ Structure des scènes

### Scènes du projet

| Scène | Rôle |
|---|---|
| 🏠 **Menu** | Écran titre — boutons Jouer / Quitter |
| ⭐ **Debut** | Scène de lancement. Contient tous les singletons persistants. N'est jamais rechargée. |
| 🍃 **Level1** | Niveau 1 |
| ❄️ **Level2** | Niveau 2 |
| 🌸 **Level3** | Niveau 3 |
| 🎉 **Fin** | Écran de fin |
| 🧪 **R&D / TestLinoFollow** | Scènes de test — ne pas inclure dans le build final |

### 🔒 Singletons persistants (scène Debut uniquement)

> ⚠️ Ces GameObjects ont `DontDestroyOnLoad` — ils doivent exister **uniquement dans Debut**, jamais dans les niveaux !

| GameObject | Script |
|---|---|
| `GameManager` | `GameManager` |
| `SceneFader` | `SceneFader` |
| `AudioManager` | `AudioManager` |
| `LocalizationManager` | `LocalizationManager` *(optionnel)* |

### 🗺️ Ordre de progression

```
Menu → Debut → Level1 → Level2 → Level3 → Fin
```

> 💡 Pour ajouter un niveau : ajouter son nom dans le tableau `_sceneOrder` du GameManager.
> L'`ExitZone` de chaque niveau appelle automatiquement `LoadNextScene()`.

### 🏗️ Build Settings

Dans **File > Build Settings**, ajouter les scènes dans cet ordre :
1. Menu
2. Debut
3. Level1
4. Level2
5. Level3
6. Fin

> ❌ Ne pas inclure R&D et TestLinoFollow.

---

## 2. 🎬 SceneFader — Transitions entre scènes

**📄 Script :** `Assets/SCRIPT/SceneFader.cs`
**📍 Placement :** GameObject vide dans la scène **Debut**

### ⚙️ Setup

1. Créer un **GameObject vide** dans Debut → nommer `SceneFader`
2. Attacher le script `SceneFader`
3. ✅ Aucun Canvas à créer — il se génère tout seul à l'exécution !

### 🎛️ Paramètres Inspector

| Champ | Description | Défaut |
|---|---|---|
| **Style** | Fade / SlideLeft / SlideRight / SlideUp / SlideDown | Fade |
| **Transition Color** | Couleur du panneau | Noir |
| **Fade Out Duration** | Durée de sortie | 0.5s |
| **Hold Duration** | Pause écran plein | 0.1s |
| **Fade In Duration** | Durée d'apparition | 0.5s |
| **Fade In Delay** | Délai avant fade-in | 0s |
| **Curve** | Courbe d'easing | EaseInOut |

### 💻 Utilisation dans un script

```csharp
SceneFader.Instance.FadeToScene("Level2");
```

### 📝 Notes

- 🔁 Le fade-in se déclenche **automatiquement** à chaque chargement de scène
- 👆 Le Canvas a un `sortingOrder` de **999** — toujours au-dessus de tout
- 🖱️ `blocksRaycasts = false` — pas d'interférence avec l'UI du jeu

---

## 3. 🔄 CharacterSwitcher — Changement de personnage

**📄 Script :** `Assets/SCRIPT/CharacterSwitcher.cs`
**📍 Placement :** GameObject dans chaque scène de jeu

### ⚙️ Setup

1. Créer un **GameObject vide** nommé `CharacterSwitcher` dans la scène
2. Attacher le script `CharacterSwitcher`
3. Remplir les références :

| Champ | Ce qu'il faut glisser |
|---|---|
| **Milo Movement** | Component `PlayerMovementMilo` de Milo |
| **Lino Movement** | Component `PlayerMovementLino` de Lino |
| **Milo Collider** | `Collider2D` de Milo |
| **Lino Collider** | `Collider2D` de Lino |
| **Milo Sprite** | `SpriteRenderer` de Milo |
| **Lino Sprite** | `SpriteRenderer` de Lino |
| **Main Camera** | La caméra principale |
| **Milo Transform** | Transform de Milo *(auto-détecté si vide)* |

### 🎨 Paramètres visuels

| Champ | Description |
|---|---|
| **Milo Sky Color** | Couleur du fond quand Milo joue |
| **Lino Active Color** | Couleur de Lino quand il est actif (jaune 💛) |
| **Lino Sky Color** | Couleur du fond quand Lino joue (gris 🩶) |
| **World Grey Tint** | Teinte appliquée à tout le monde quand Lino joue |

### 👁️ Lino Only Objects

- **Lino Only Objects** : GameObjects visibles uniquement quand Lino joue ou que Milo s'approche
- **Detection Distance** : distance à partir de laquelle l'objet s'active même avec Milo

### 🎮 Comportement

- **Tab** : alterne entre Milo 🐱 et Lino 🐈
- Quand **Lino est actif** → tout le monde est grisé sauf Lino, Milo et les Fireflies ✨
- Le personnage **inactif** est gelé (`FreezeAll`) → ne glisse pas sur les pentes

### 💻 Appel depuis un script

```csharp
CharacterSwitcher switcher = FindObjectOfType<CharacterSwitcher>();
switcher.ForceMilo(); // forcer le retour sur Milo
```

---

## 4. 💬 DialogueZone — Zones de dialogue

**📄 Script :** `Assets/SCRIPT/DialogueZone.cs`
**📍 Placement :** Sur un GameObject avec un **Collider2D Trigger**

### ⚙️ Setup

1. Créer un GameObject (ex: `DialogueTrigger_Intro`)
2. Ajouter un `Collider2D` (Box ou Circle) → cocher **Is Trigger** ✅
3. Attacher `DialogueZone`
4. Créer un **Canvas UI** dans la scène avec :
   - Un `Panel` → le panneau de dialogue
   - Un `Image` → portrait du personnage
   - Deux `TextMeshProUGUI` → nom + texte
   - *(Optionnel)* Un indicateur "continuer" ▶️

### 🔗 Références Inspector

| Champ | Ce qu'il faut glisser |
|---|---|
| **Dialogue Panel** | Le Panel UI racine |
| **Portrait** | Le composant `Image` du portrait |
| **Name Text** | `TextMeshProUGUI` du nom |
| **Dialogue Text** | `TextMeshProUGUI` du texte |
| **Continue Indicator** | *(Optionnel)* Objet affiché en attente d'input |
| **Milo Portrait** | Sprite portrait de Milo |
| **Lino Portrait** | Sprite portrait de Lino |

### 🎛️ Configuration

| Champ | Description |
|---|---|
| **Trigger Target** | Qui déclenche : Milo / Lino / Both |
| **Repeatable** | ♻️ Si coché, se rejoue à chaque entrée |
| **Lines** | Tableau des lignes *(fallback sans localisation)* |
| **Line Keys** | 🌍 Clés CSV pour la localisation *(optionnel)* |
| **Speaker Names** | Noms des personnages *(1:1 avec Lines)* |
| **Letter Delay** | ⌨️ Vitesse de l'effet machine à écrire |
| **Line Pause** | Pause entre lignes *(si Wait For Input désactivé)* |
| **Wait For Input** | Space/Jump pour avancer |
| **Skip Typing On Input** | Affiche la ligne entière instantanément sur input |

> 💡 **Rétrocompatibilité :** Les DialogueZones existantes avec `Lines` rempli fonctionnent sans modification !

### 🏷️ Tags requis

- Milo → tag **`Milo`**
- Lino → tag **`Lino`**

---

## 5. 🌿 SpriteAnimator — Animations décoratives

**📄 Script :** `Assets/SCRIPT/SpriteAnimator.cs`
**📍 Placement :** Sur tout GameObject avec un `SpriteRenderer` (plantes, arbres, décos...)

### ⚙️ Setup

1. Sélectionner le GameObject décoratif
2. Attacher `SpriteAnimator` *(le `SpriteRenderer` est requis automatiquement)*
3. Glisser les sprites dans **Frames** dans l'ordre d'animation

### 🎛️ Paramètres

| Champ | Description |
|---|---|
| **Frames** | 🖼️ Tableau de sprites à animer |
| **FPS** | Vitesse de l'animation |
| **Play Mode** | Loop / PingPong / Once |
| **Random Offset** | 🎲 Démarre à une frame aléatoire *(évite la synchro visuelle)* |
| **Play On Start** | Démarre automatiquement |

### 💻 API

```csharp
SpriteAnimator anim = GetComponent<SpriteAnimator>();
anim.Play();    // ▶️ démarre
anim.Pause();   // ⏸️ met en pause
anim.Stop();    // ⏹️ arrête et remet à la frame 0
```

---

## 6. 🐾 PlayerMovementBase — Coyote Time

**📄 Script :** `Assets/SCRIPT/PlayerMovementBase.cs`
*(hérité par `PlayerMovementMilo` et `PlayerMovementLino`)*

### 🐱 C'est quoi le Coyote Time ?

Le **coyote time** permet de sauter encore un court instant après avoir quitté le bord d'une plateforme — comme Wile E. Coyote qui court dans le vide avant de tomber ! Ça rend les sauts beaucoup plus agréables. 🎮

### 🎛️ Paramètre Inspector

| Champ | Description | Défaut |
|---|---|---|
| **Coyote Time** | Fenêtre de saut après avoir quitté le sol (secondes) | 0.15s |

> 💡 Ce champ apparaît sur `PlayerMovementMilo` et `PlayerMovementLino` dans l'Inspector.

### 📝 Notes

- 🚫 Le timer se reset à chaque saut → pas de double saut en l'air
- Mettre à `0` pour désactiver complètement

---

## 7. 🚪 ExitZone — Passage au niveau suivant

**📄 Script :** `Assets/SCRIPT/ExitZone.cs`
**📍 Placement :** Trigger zone à placer librement dans le niveau

### ⚙️ Setup

1. Créer un GameObject (ex: `ExitZone`)
2. Ajouter un `Collider2D` → cocher **Is Trigger** ✅
3. Attacher `ExitZone`
4. 📍 Positionner où tu veux dans le niveau (sortie, porte, bord...)

### 🎮 Comportement

- Quand **Milo** entre dans la zone → `GameManager.LoadNextScene()` 🚀
- La transition est animée via `SceneFader` si présent
- Seul Milo peut déclencher *(Lino suit toujours)*

---

## 8. ⏸️ MenuPause — Menu pause

**📄 Script :** `Assets/SCRIPT/MenuPause.cs`
**📍 Placement :** GameObject dans chaque scène de jeu

### ⚙️ Setup

1. Créer un **Canvas UI** avec un Panel enfant (le menu pause)
2. Ajouter les boutons : **Resume**, **Options**, **Main Menu**, **Quit**
3. Créer un GameObject vide `MenuPause` → attacher le script
4. Assigner dans l'Inspector :
   - `_container` → le Panel racine du menu
   - `_optionsMenu` → le composant `OptionsMenu` *(optionnel)*

### 🔗 Wiring des boutons

| Bouton | Méthode |
|---|---|
| ▶️ Resume | `MenuPause.ResumeButton()` |
| ⚙️ Options | `MenuPause.OptionsButton()` |
| 🏠 Main Menu | `MenuPause.MainMenuButton()` |
| ❌ Quit | `MenuPause.QuitGame()` |

### 🎮 Comportement

- **Escape** : ouvre ET ferme le menu (toggle)
- À l'ouverture → `Time.timeScale = 0` ⏸️
- À la fermeture → `Time.timeScale = 1` ▶️

> ⚠️ **Prérequis :** Un **EventSystem** doit être dans la scène !
> Ajouter via `GameObject > UI > Event System`

---

## 9. ⚙️ OptionsMenu — Menu options

**📄 Script :** `Assets/SCRIPT/OptionsMenu.cs`
**📍 Placement :** Sur le root du panel Options (enfant du Canvas)

### ⚙️ Setup UI

Créer dans le Canvas un Panel `OptionsPanel` avec :
- 🎵 Un **Slider** (0→1) pour le volume musique
- 🔊 Un **Slider** (0→1) pour le volume SFX
- 🌍 Un **TMP_Dropdown** pour la langue
- ◀️ Un bouton **Retour** → `OptionsMenu.Hide()`

Assigner dans l'Inspector :

| Champ | Ce qu'il faut glisser |
|---|---|
| **Music Slider** | Slider volume musique |
| **SFX Slider** | Slider volume SFX |
| **Language Dropdown** | TMP_Dropdown de langue |
| **Language Codes** | `fr`, `en` *(dans le même ordre que le Dropdown)* |
| **Panel** | Ce même GameObject |

### 🔗 Wiring des callbacks

| Élément | Event | Méthode |
|---|---|---|
| Music Slider | `OnValueChanged` | `OptionsMenu.OnMusicVolumeChanged` |
| SFX Slider | `OnValueChanged` | `OptionsMenu.OnSfxVolumeChanged` |
| Language Dropdown | `OnValueChanged` | `OptionsMenu.OnLanguageChanged` |

> 💡 Les sliders se positionnent automatiquement aux valeurs sauvegardées à l'ouverture !

---

## 10. 🌍 LocalizationManager — Système de langue

**📄 Script :** `Assets/SCRIPT/LocalizationManager.cs`
**📍 Placement :** GameObject vide dans la scène **Debut**

### ⚙️ Setup

1. Créer un GameObject vide `LocalizationManager` dans Debut
2. Attacher le script `LocalizationManager`
3. Créer les fichiers CSV dans `Assets/Resources/Localization/`
4. Dans `_csvFiles` de l'Inspector → `Localization/dialogues`

### 📄 Format CSV

```
key,fr,en
intro_01,Bonjour Milo !,Hello Milo!
intro_02,"C'est l'heure, partons !","It's time, let's go!"
```

- 1ère ligne = en-tête avec les codes de langue
- 1ère colonne = clé unique
- Champs avec virgules/apostrophes → entourer de `"guillemets"`
- Encodage : **UTF-8** ✅

### ➕ Ajouter une langue

Ajouter une colonne dans le CSV :

```
key,fr,en,es
intro_01,Bonjour !,Hello!,¡Hola!
```

Puis ajouter `"es"` dans `_languageCodes` de l'OptionsMenu + une option dans le Dropdown.

### 💻 API

```csharp
LocalizationManager.Instance.Get("intro_01");       // texte courant
LocalizationManager.Instance.SetLanguage("en");      // changer de langue
LocalizationManager.Instance.CurrentLanguage;        // "fr" / "en" / ...
```

---

## 11. 🔤 LocalizedText — Texte UI localisé

**📄 Script :** `Assets/SCRIPT/LocalizedText.cs`
**📍 Placement :** Sur tout GameObject avec un `TextMeshProUGUI`

### ⚙️ Setup

1. Sélectionner le GameObject du texte UI
2. Attacher `LocalizedText`
3. Saisir la **clé CSV** dans le champ `_key` de l'Inspector

✅ Le texte se met à jour automatiquement au chargement et à chaque changement de langue !

> 💡 Si `LocalizationManager` est absent, le texte reste inchangé *(pas d'erreur)*

---

## 12. 🔊 AudioManager — Gestion du son

**📄 Script :** `Assets/SCRIPT/AudioManager.cs`
**📍 Placement :** GameObject vide dans la scène **Debut**

### ⚙️ Setup

1. Créer un GameObject vide `AudioManager` dans Debut
2. Attacher le script `AudioManager`
3. Dans `_sounds`, ajouter autant d'entrées que de SFX :

| Champ | Description |
|---|---|
| **Name** | Identifiant du son : `"Jump"`, `"Land"`, `"FireflyCatch"` |
| **Clip** | L'AudioClip à jouer |
| **Volume** | Volume de base [0-1] |
| **Pitch** | Hauteur [0.1-3] |
| **Loop** | ♻️ Cocher pour un son en boucle |

### 🎵 Paramètres musique

| Champ | Description | Défaut |
|---|---|---|
| **Music Volume** | Volume global musique [0-1] | 0.8 |
| **Fade Duration** | Durée du crossfade | 1s |

### 🦻 Son étouffé — Milo entend mal

Quand **Milo** est le personnage actif, un filtre passe-bas (`AudioLowPassFilter`) est automatiquement appliqué sur l'`AudioListener` de la caméra. Tout l'audio (musique + SFX) sonne alors étouffé, comme si on entendait mal. Quand on repasse sur **Lino**, le filtre est retiré et le son revient à la normale.

| Champ Inspector | Description | Défaut |
|---|---|---|
| **Muffled Cutoff** | Fréquence de coupure en Hz. Plus bas = plus étouffé | `800 Hz` |

> 💡 Valeurs de référence : `400 Hz` = très sourd · `800 Hz` = clairement étouffé · `2000 Hz` = légèrement voilé

Aucun setup manuel nécessaire — le filtre est créé et géré entièrement par le code.

### 💻 API

```csharp
AudioManager.Instance?.Play("Jump");          // ▶️ jouer un SFX
AudioManager.Instance?.Stop("Jump");          // ⏹️ arrêter un SFX
AudioManager.Instance?.StopAll();             // ⏹️ arrêter tous les SFX
AudioManager.Instance?.SetMusicVolume(0.5f);  // 🎵 volume musique
AudioManager.Instance?.SetSfxVolume(0.8f);   // 🔊 volume SFX
AudioManager.Instance?.SetMuffled(true);      // 🦻 activer le filtre étouffé manuellement
```

---

## 13. 🎵 SceneMusic — Musique par scène

**📄 Script :** `Assets/SCRIPT/SceneMusic.cs`
**📍 Placement :** Dans **chaque scène** qui a une musique

### ⚙️ Setup

1. Créer un GameObject vide `SceneMusic` dans la scène
2. Attacher `SceneMusic`
3. Glisser l'`AudioClip` dans `_musicClip` 🎶

### 🎮 Comportement

- 🎵 Au démarrage → appelle `AudioManager.PlayMusic()` avec crossfade automatique
- 🔇 Si `_musicClip` est vide → la musique s'arrête
- ♻️ Si la même musique joue déjà → elle ne repart pas du début

---

## 14. 🎥 CameraFollow — Caméra

**📄 Script :** `Assets/SCRIPT/CameraFollow.cs`
**📍 Placement :** Sur la **Main Camera** de chaque scène de jeu

### 🎛️ Paramètres Inspector

| Champ | Description |
|---|---|
| **Milo** | Transform de Milo |
| **Lino** | Transform de Lino |
| **Follow Speed** | Vitesse de lerp de la caméra |
| **Offset Z** | Décalage Z *(généralement -10)* |
| **Is Great Room** | ✅ Coché = caméra **fixe**, décoché = caméra qui suit |

### 🎮 Comportement

- 📌 **Is Great Room coché** → caméra statique
- 🎯 **Is Great Room décoché** → suit le **point médian** entre Milo et Lino

---

## 15. ✨ Firefly — Luciole

**📄 Script :** `Assets/SCRIPT/Firefly.cs`
**📍 Placement :** Sur le GameObject luciole, avec un `Collider2D` Trigger

### ⚙️ Setup

1. Créer le GameObject luciole avec `SpriteRenderer` + `Collider2D` (Trigger)
2. Attacher `Firefly`
3. Remplir les références :

| Champ | Description |
|---|---|
| **Visible To** | 🐱 Quel chat peut voir/attraper la luciole |
| **Character Switcher** | Le `CharacterSwitcher` de la scène |
| **Float Speed** | Vitesse de l'animation flottante |
| **Float Amplitude** | Amplitude du mouvement vertical |
| **Obstacle To Destroy** | *(Optionnel)* Objet détruit à la capture |
| **Lino Blocker** | *(Optionnel)* `LinoBlocker` débloqué à la capture |

### 🎮 Comportement

- 💫 Flotte verticalement en boucle
- 👁️ Visible **uniquement par le bon chat** (l'autre ne la voit pas !)
- 🎯 Quand le bon chat la touche → SFX `"FireflyCatch"` + obstacle détruit + Lino débloqué

> 🩶 La luciole est **exclue de l'effet gris** du CharacterSwitcher automatiquement

---

## 16. 🐾 LinoFollower — Lino suit Milo

**📄 Script :** `Assets/SCRIPT/LinoFollower.cs`
**📍 Placement :** Sur le GameObject **Lino**

### ⚙️ Setup

1. Attacher `LinoFollower` à Lino
2. Glisser le `Transform` de Milo dans `_milo`
3. Placer des **zones de suivi** dans le niveau *(voir ci-dessous)*

### 🎛️ Paramètres Inspector

| Champ | Description | Défaut |
|---|---|---|
| **Milo** | Transform de Milo | — |
| **Speed** | Vitesse de Lino vers Milo | 3 |
| **Min Distance** | Distance avant que Lino s'arrête | 0.5 |
| **Follow Delay** | Délai avec lequel Lino rejoue le chemin | 0.3s |

### 🏷️ Zones de suivi (tag `Corridor`)

Pour créer une zone où Lino suit Milo :

1. Créer un GameObject avec un `Collider2D` → **Is Trigger** ✅
2. Donner le tag **`Corridor`** à ce GameObject
3. Positionner dans le niveau

> 💡 Tant que Lino est dans une zone `Corridor`, il reproduit le chemin de Milo avec un léger retard.

---

## 17. 🚧 LinoBlocker — Blocage de Lino

**📄 Script :** `Assets/SCRIPT/LinoBlocker.cs`
**📍 Placement :** Sur le GameObject **Lino**

### 🎮 Comportement

- 🛑 Par défaut, Lino **ne peut pas avancer** (vélocité X = 0)
- ✅ Appeler `Unblock()` pour le libérer — via une `Firefly` ou un `ObjectTrigger`

### 💻 API

```csharp
linoBlocker.Unblock();            // libérer Lino
bool bloque = linoBlocker.IsBlocked;  // lire l'état
```

---

## 18. 💥 ObjectTrigger — Trigger d'objet

**📄 Script :** `Assets/SCRIPT/ObjectTrigger.cs`
**📍 Placement :** Sur un GameObject avec un `Collider2D` Trigger

### 🎛️ Paramètres Inspector

| Champ | Description |
|---|---|
| **Lino** | GameObject de Lino *(pour `LinoBlocker`)* |
| **Object To Disappear** | L'objet à désactiver/détruire quand Milo entre |
| **Milo Tag** | Tag du déclencheur *(défaut : `"Milo"`)* |
| **Destroy Instead Of Disable** | Détruire plutôt que désactiver |

### 🎮 Comportement

- Milo entre → objet cible disparaît + `LinoBlocker.Unblock()` 🔓
- 💥 Le trigger se détruit après déclenchement *(one-shot)*

---

## 19. 🟫 MovingPlatform — Plateforme mobile

**📄 Script :** `Assets/SCRIPT/MovingPlatform.cs`
**📍 Placement :** Sur le GameObject de la plateforme

### 🎛️ Paramètres Inspector

| Champ | Description | Défaut |
|---|---|---|
| **Direction** | ↔️ Horizontal ou ↕️ Vertical | Horizontal |
| **Distance** | Amplitude du mouvement (unités Unity) | 3 |
| **Speed** | Vitesse du mouvement | 2 |

### 🎮 Comportement

Mouvement sinusoïdal automatique — aucun Rigidbody requis !

> 💡 Pour que le chat reste sur la plateforme, elle doit avoir un `Collider2D` non-trigger avec le bon Layer pour le ground check.

---

## 20. 💀 RespawnOnFall — Respawn

**📄 Script :** `Assets/SCRIPT/RespawnOnFall.cs`
**📍 Placement :** Sur **Milo** ET **Lino**

### 🎛️ Paramètres Inspector

| Champ | Description | Défaut |
|---|---|---|
| **Death Height** | Hauteur Y en dessous de laquelle le respawn se déclenche | -10 |

### 🎮 Comportement

- 📍 Mémorise la position de départ au `Start()`
- 💀 Tombe en dessous de `_deathHeight` → téléportation au spawn + vélocité reset

> ⚠️ Ajuster `_deathHeight` selon la taille de chaque niveau !

---

## 21. ⌨️ TypingEffect — Effet de frappe (intro/outro)

**📄 Script :** `Assets/SCRIPT/TypingEffect.cs`
**📍 Placement :** Sur un GameObject UI dans une scène d'intro ou de fin

### ⚙️ Setup

1. Créer un GameObject avec un `TextMeshProUGUI`
2. Attacher `TypingEffect`
3. Glisser le `TextMeshProUGUI` dans `_textDisplay` *(auto-détecté si vide)*
4. Remplir le tableau `_phrases`

### 🎛️ Paramètres Inspector

| Champ | Description | Défaut |
|---|---|---|
| **Phrases** | Tableau de textes en séquence | — |
| **Text Display** | `TextMeshProUGUI` cible | auto |
| **Letter Delay** | ⌨️ Délai entre chaque lettre | 0.05s |
| **Phrase Delay** | ⏳ Pause entre chaque phrase | 1s |
| **Load Next Scene On Complete** | 🚀 Charge la scène suivante après la dernière phrase | false |
| **Scene Load Delay** | Délai avant le chargement | 1s |

---

## 22. 🏠 Menu — Écran titre

**📄 Script :** `Assets/SCRIPT/Menu.cs`
**📍 Placement :** Sur un GameObject dans la scène **Menu**

### 🔗 Wiring des boutons

| Bouton | Méthode |
|---|---|
| 🎮 Jouer / Play | `Menu.PlayGame()` |
| ❌ Quitter / Quit | `Menu.QuitGame()` |

### 🎮 Comportement

- `PlayGame()` → `GameManager.ResetAndStart()` *(repart depuis le début)*
- Si GameManager pas encore chargé → charge directement `"Debut"`

---

## 23. 🌄 Parallax — Fond parallaxe

**📄 Script :** `Assets/SCRIPT/Prallax/Parallax.cs`
**📍 Placement :** Sur chaque **couche de fond** (background layer)

Fait défiler un GameObject à une fraction de la vitesse de la caméra pour donner une illusion de profondeur.

### 🎛️ Paramètres Inspector

| Paramètre | Description |
|---|---|
| **Parallax Effect** | Facteur de défilement `[0–1]` |

### 🎨 Valeurs recommandées

| Couche | Valeur |
|---|---|
| 🌌 Ciel / très loin | `0.05 – 0.15` |
| ⛰️ Montagnes / plans lointains | `0.2 – 0.4` |
| 🌲 Arbres / plans intermédiaires | `0.4 – 0.6` |
| 🌿 Buissons / plans proches | `0.7 – 0.9` |

### ⚡ Tips importants

> 💡 **LateUpdate, pas FixedUpdate** — Cinemachine met la caméra à jour en LateUpdate. Si le parallax tourne en Update ou FixedUpdate, les fonds tremblent légèrement à chaque frame.

> 💡 **Offset relatif** — Le script mémorise la position X de la caméra au démarrage (`_startCamPosX`) et calcule un déplacement *relatif*. Avec l'ancienne formule `cam.x * factor`, si la caméra ne démarrait pas à x=0 les fonds sautaient au chargement.

> 💡 **Cache Camera.main** — `Camera.main` fait un `GetComponent` en interne à chaque appel. Le cacher dans `Awake()` évite de l'appeler 60× par seconde.

> 💡 **Parallax uniquement en X** — Le Y est intentionnellement ignoré. Un parallax vertical crée un effet désagréable et nauséeux sur les plateformers.

> 💡 **Activer seulement dans les zones prévues** — Utilise `ParallaxZone` (section 24) pour n'activer le parallax que là où c'est nécessaire.

---

## 24. 🌊 ParallaxZone — Zone de transition parallaxe

**📄 Script :** `Assets/SCRIPT/Prallax/ParallaxZone.cs`
**📍 Placement :** Sur 2 GameObjects invisibles à l'entrée et à la sortie de chaque zone parallaxe

### 💡 Principe

Place **deux zones** aux limites d'une section parallaxe :

```
[Zone Start] ──── zone parallax (caméra mobile, pas de mort) ──── [Zone End]
```

- **Start zone** → active la caméra follow (parallax visible) + désactive le respawn
- **End zone** → rétablit la caméra fixe + réactive le respawn

### 🎛️ Paramètre Inspector

| Paramètre | Description |
|---|---|
| **Mode** | `Start` = entrée zone · `End` = sortie zone |

### 🛠️ Setup dans Unity

1. Créer un **GameObject vide** à l'entrée de la zone parallaxe
2. Ajouter un `Collider2D` → **Is Trigger ✅** *(bande verticale couvrant toute la hauteur du passage)*
3. Attacher `ParallaxZone`, choisir **Mode = Start**
4. Dupliquer le GameObject, le placer à la sortie, **Mode = End**

### 🔗 Dépendances

- **CameraManager** doit être présent dans la scène avec `_cineFollowCam` et `_cineFixedCam` configurés
- **RespawnOnFall** sur les deux chats est automatiquement désactivé à l'entrée (pas de mort dans les zones parallax) et réactivé à la sortie

### 🐛 Debug (éditeur uniquement)

Les touches `I` / `O` permettent de basculer manuellement entre les caméras dans l'éditeur. Elles sont compilées uniquement en mode `UNITY_EDITOR` et n'apparaissent pas dans les builds.

---

## ✅ Checklist Unity Editor — configuration manuelle

> 🛠️ Ces étapes ne peuvent pas être scriptées — à faire directement dans Unity !

### 1️⃣ Scène Debut — ajouter LocalizationManager

- [ ] Créer un **GameObject vide** nommé `LocalizationManager`
- [ ] Attacher le script `LocalizationManager`
- [ ] Vérifier que `Localization/dialogues` est dans `_csvFiles`

### 2️⃣ ExitZone — dans chaque niveau

- [ ] Créer un GameObject à la sortie du niveau
- [ ] Ajouter `Collider2D` → **Is Trigger** ✅
- [ ] Attacher `ExitZone`
- [ ] Répéter pour Level1, Level2, Level3

### 3️⃣ MenuPause — bouton Options

- [ ] Ajouter un bouton **Options** dans le Canvas pause
- [ ] Wirer sur `MenuPause.OptionsButton()`
- [ ] Assigner `_optionsMenu` dans l'Inspector du `MenuPause`

### 4️⃣ OptionsMenu — créer le Panel UI

| Élément | Type | Configuration |
|---|---|---|
| 🎵 Slider musique | `Slider` | Min=0, Max=1 → `OnMusicVolumeChanged` |
| 🔊 Slider SFX | `Slider` | Min=0, Max=1 → `OnSfxVolumeChanged` |
| 🌍 Dropdown langue | `TMP_Dropdown` | "Français", "English" → `OnLanguageChanged` |
| ◀️ Bouton Retour | `Button` | → `OptionsMenu.Hide()` |

- [ ] Assigner `_musicSlider`, `_sfxSlider`, `_languageDropdown`, `_panel`
- [ ] `_languageCodes` : `fr`, `en` *(même ordre que le Dropdown)*

### 5️⃣ ParallaxZone — une paire par section parallaxe

- [ ] Créer un GameObject vide → `Collider2D` **Is Trigger ✅** → `ParallaxZone` → **Mode = Start**
- [ ] Dupliquer → placer à la sortie → **Mode = End**
- [ ] Vérifier que `CameraManager` a `_cineFollowCam` et `_cineFixedCam` assignés

### 6️⃣ CSV dialogues — remplir les vrais textes

Éditer `Assets/Resources/Localization/dialogues.csv` :

```
key,fr,en
ma_cle,Texte en français,"Texte en anglais"
```

- [ ] Remplir avec les textes du jeu
- [ ] Dans chaque `DialogueZone`, remplir `_lineKeys` avec les clés

---

*🐱 Document mis à jour — 2026-05-28 — sections 23 & 24 ajoutées (Parallax + ParallaxZone)*
