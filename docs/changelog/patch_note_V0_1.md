# Patch notes — v0.1

## v0.1.0 — 2026-06-11

### Features
- **Dialogue** : avancement automatique des lignes (input, timer, ou les deux — `AdvanceMode`).
- **Dialogue** : sprite d'émotion configurable par ligne (`_lineEmojis`), fallback sur le portrait du personnage.
- **Dialogue** : couleur du nom du locuteur configurable par personnage (Milo / Lino).
- **Audio** : sons d'ambiance par personnage sur `LevelConnector` (`Common/Milo/Lino Sounds`), mute/unmute au switch sans redémarrage des boucles.

### Fixes
- **Audio** : inversion du filtre sourd corrigée — c'est Milo qui entend étouffé, pas Lino.
- **Audio** : `SceneMusic` logge un warning explicite quand l'AudioManager est absent ou qu'aucun clip n'est assigné (au lieu d'échouer en silence).
