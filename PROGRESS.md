# PROGRESS

## État actuel

### Features terminées (code)
- **Cat paw menu effects** : `MenuButtonHover.cs`, `CatPawTrail.cs` (traversée bord à bord près du centre)
- **Fix landing stuck in Fall state** : suppression de `HandleLanding()` / `_wasGrounded`
- **Nettoyage espaces d'alignement** : tous les scripts C# alignés
- **LinoFollower refactor** : buffer horodaté + `SetActive(bool)` + compatibilité Corridor
- **CharacterSwitcher** : `ForceLino()` ajouté
- **WaitForLinoZone** : nouveau script — attend atterrissage, affiche message, délai avant switch, Milo toujours final

### Setup Unity restant (à faire dans l'éditeur)
- **MenuButtonHover** : ajouter Image enfant "Patte" sur chaque bouton du menu, assigner dans l'Inspector
- **CatPawTrail** : créer GameObject vide dans Canvas, attacher `CatPawTrail`, assigner `PawSprite` + `Canvas`
- **WaitForLinoZone** : créer GameObject vide en fin de niveau avec BoxCollider2D (Is Trigger), attacher `WaitForLinoZone`, assigner :
  - `CharacterSwitcher`
  - `LinoFollower` (composant sur Lino)
  - `MiloRb` / `LinoRb` (Rigidbody2D des deux chats)
  - `MessageObject` (GameObject UI avec le texte, désactivé par défaut)
