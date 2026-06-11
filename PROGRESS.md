# PROGRESS

## État actuel

### Features terminées (code) — 2026-06-11
- **DialogueZone — AdvanceMode** : enum `InputOnly / TimerOnly / InputOrTimer` (défaut). Les lignes avancent à l'input ou après `_linePause`, le premier des deux.
- **DialogueZone — emoji par ligne** : tableau `_lineEmojis` parallèle à `_lines`; entrée vide = portrait par défaut du personnage.
- **DialogueZone — couleur du nom** : `_miloNameColor` / `_linoNameColor` appliqués à `_nameText` selon le locuteur.
- **LevelConnector — sons par personnage** : `_commonSounds` / `_miloSounds` / `_linoSounds` (AudioSource[]), pattern identique aux VFX.
- **CharacterSwitcher — SwapSounds** : toutes les sources jouent dès le Start, mute/unmute au switch (boucles synchronisées, pas de redémarrage).
- **Muffle** : c'est Lino qui entend étouffé (`SetMuffled(!_isPlayingMilo)`) — le code d'origine était correct, seuls les commentaires/docs étaient inversés et ont été alignés.
- **SceneMusic — warnings** : log explicite si `AudioManager.Instance` est null (scène testée sans la scène "Debut") ou si aucun clip n'est assigné.

### Setup Unity restant (à faire dans l'éditeur)
- **DialogueZone** : rien à changer — toutes les zones sont en `InputOrTimer` (timer + espace) par défaut. Remplir `_lineEmojis` et les deux couleurs de nom dans l'Inspector.
- **LevelConnector** : assigner les AudioSources d'ambiance dans `Common/Milo/Lino Sounds` (sources en `loop`, placées dans la scène).
- **SceneMusic** : vérifier qu'un clip est bien assigné dans chaque scène; tester depuis la scène "Debut" (sinon pas d'AudioManager → warning console).
- **MenuButtonHover** : ajouter Image enfant "Patte" sur chaque bouton du menu, assigner dans l'Inspector
- **CatPawTrail** : créer GameObject vide dans Canvas, attacher `CatPawTrail`, assigner `PawSprite` + `Canvas`
- **WaitForLinoZone** : créer GameObject vide en fin de niveau avec BoxCollider2D (Is Trigger), attacher `WaitForLinoZone`, assigner `CharacterSwitcher`, `LinoFollower`, `MiloRb` / `LinoRb`, `MessageObject`

### Docs
- Design : `docs/plans/2026-06-11-dialogue-audio-design.md`
- Plan d'implémentation : `docs/plans/2026-06-11-dialogue-audio-implementation.md`
- Changelog : `docs/changelog/patch_note_V0_1.md`
