# Design — VFX swap & darkness progression au switch Lino

**Date:** 2026-06-09

## Contexte

Lorsqu'on passe en mode Lino, l'écran s'assombrit et le VFX ambiance Milo (ex. feuilles) doit être remplacé par la pluie. L'intensité de l'assombrissement diminue au fil des niveaux. La pluie disparaît à partir d'un niveau configurable (en assignant `null` à `linoVfx`).

## Composants modifiés

### LevelConnector
Ajout de 3 champs :
- `float _linoDarkness` — intensité du gris Lino (0 = aucun, 1 = gris complet). Défaut : 1.
- `ParticleSystem _miloVfx` — VFX ambiance actif quand on joue Milo (feuilles, etc.).
- `ParticleSystem _linoVfx` — VFX actif quand on joue Lino (pluie). Laisser vide = aucun VFX Lino.

### CharacterSwitcher
- Au `Start()` : récupère `LevelConnector` via `FindFirstObjectByType`, active `_miloVfx`.
- `UpdateColors()` : dérive `linoSkyColor` et `worldGreyTint` par `Color.Lerp` entre couleur Milo et couleur max, modulé par `linoDarkness`.
- Nouvelle méthode `SwapVfx(bool playingMilo)` appelée dans `SwitchCharacter()` :
  - → Milo : `linoVfx.Stop()`, `miloVfx.Play()`
  - → Lino : `miloVfx.Stop()`, `linoVfx.Play()` (si non null)
- Fallback si `LevelConnector` absent : comportement hardcodé actuel conservé.

## Exemple de configuration par niveau

| Niveau | `linoDarkness` | `miloVfx` | `linoVfx` |
|--------|---------------|-----------|-----------|
| Level1 | 1.0 | Feuilles | Pluie |
| Level2 | 0.7 | Autre VFX | Pluie |
| Level3 | 0.3 | Autre VFX | null |

## Edge cases
- `LevelConnector` absent → fallback couleurs hardcodées, pas de swap VFX.
- `linoVfx` null → pas de VFX Lino, seulement l'assombrissement.
- `linoDarkness = 0` → aucun gris, monde identique en mode Lino.
