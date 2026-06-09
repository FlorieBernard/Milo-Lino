# Design — Footstep Sounds par surface

**Date:** 2026-06-09

## Contexte

Chaque plateforme/zone peut définir ses propres sons de pas via un composant `FootstepSurface`. Le player pioche un clip aléatoire à intervalle régulier quand il marche au sol. Milo et Lino partagent le même système.

## Composants

### FootstepSurface (nouveau)
Composant à placer sur chaque plateforme/zone.
- `AudioClip[] _clips` — pool de clips, pioche aléatoire à chaque pas
- `AudioClip GetRandom()` — retourne `_clips[Random.Range(...)]`
- Si la plateforme n'a pas ce composant → silencieux (pas d'erreur)

### AudioManager — ajout PlayOneShot
- Nouvelle méthode `PlayOneShot(AudioClip clip)` sur AudioManager
- Utilise un `AudioSource` dédié en one-shot (respecte le volume SFX global)
- Permet de jouer n'importe quel clip dynamique sans l'enregistrer dans `_sounds`

### PlayerMovementBase — HandleFootsteps()
- `[SerializeField] float _footstepInterval = 0.35f` (configurable Inspector)
- `float _footstepTimer` (privé)
- Appelé dans `Update()` après `HandleRunVFX()`
- Logique :
  - Si `IsGrounded() && Mathf.Abs(Horizontal) > 0.1f` → décrémenter timer
  - Sinon → reset timer à `_footstepInterval` (redémarre proprement au prochain pas)
  - Quand timer ≤ 0 : `OverlapCircle` → `GetComponentInParent<FootstepSurface>()` → `AudioManager.Instance?.PlayOneShot(surface.GetRandom())`
  - Reset timer

## Workflow Unity
1. Placer `FootstepSurface` sur chaque plateforme, assigner les `AudioClip[]`
2. `_footstepInterval` ajustable par Inspector sur le player (défaut 0.35s)
3. Aucun son à enregistrer dans `AudioManager._sounds`
