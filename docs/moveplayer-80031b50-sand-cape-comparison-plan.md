# MovePlayer 0x80031B50 Sand Cape Comparison Plan

## Objective

Comparer la version C# de `PlayerManager.MovePlayer()` avec la version originale PSX uniquement sur le comportement lie a la cape de sable, pour expliquer pourquoi le joueur ne peut plus bouger apres son utilisation.

Fonction cible:

- C# : `AlundraTools/AlundraEngine/Gameplay/PlayerManager.cs`
- Original : `MovePlayer @ 0x80031B50`

## Focus Restriction

La fonction est tres grosse. On ne decompile pas toute la fonction au premier passage.

Le premier perimetre autorise est strictement:

- le chemin qui entre dans le switch sur `PlayerEntity.TargetAnimationId`
- les cas sable `0x20..0x24`
- les labels de sortie immediats de cette zone
- les appels directs effectues depuis cette zone

On n'elargit a d'autres branches de `MovePlayer` que si la comparaison locale ne ferme pas la cause du gel.

## Current C# Anchors

### MovePlayer anchors

- `MovePlayer()` commence au debut du fichier `PlayerManager.cs`
- la sous-branche sable est visible autour des cas:
  - `EnterSand = 0x20`
  - `ExitSand = 0x21`
  - `InSand = 0x22`
  - `InSandMoving = 0x23`
  - `InSandDash = 0x24`

### Current C# sand branch summary

#### Case `EnterSand` / `InSandDash`

- appelle `TryUseItem()`
- si `TryUseItem() == 0`, la branche sort immediatement
- si `g_warpLockTimer == 0x20`, la branche sort immediatement
- sinon elle tombe sur le label commun qui force un retour vers `Jump` ou `Idle`

#### Case `ExitSand`

- appelle `TryUseItem()`
- sort immediatement

#### Case `InSand` / `InSandMoving`

- ecrit `PlayerEntity.TargetDirection = dir`
- appelle `TryUseItem()`
- si `TryUseItem() == 0`, la branche sort immediatement
- si `g_warpLockTimer == 0x20`, la logique ne fait que choisir entre:
  - `InSand`
  - `InSandMoving`
  - `InSandDash`
- sinon elle retombe sur le label commun qui force `Jump` ou `Idle`

### Related helper already closed

#### `TryUseSandCape()`

Etat deja ferme cote C# et precedent travail PSX:

- avec `g_warpLockTimer == 0`, l'objet ne marche que si:
  - `Slope_18c == 3`
  - `TargetAnimationId < 2`
- succes: `TargetAnimationId = EnterSand`
- avec `g_warpLockTimer != 0`, la sortie ne marche que depuis `InSand`
- la sortie exige aussi:
  - `FinalForceX == 0`
  - `FinalForceY == 0`
  - `IsOnGround != 0`
  - aucune entite bloquante en overlap
- succes: `TargetAnimationId = ExitSand`

## Local Hypothesis

Le gel de mouvement vient probablement d'un ecart dans la sous-branche sable de `MovePlayer`, pas dans `TryUseSandCape()`.

Hypothese precise:

- soit la logique originale des cas `0x20..0x24` fait plus que changer l'animation, par exemple une mise a jour de forces, de direction, ou un saut vers un chemin de mouvement commun
- soit la condition `g_warpLockTimer == 0x20` n'est pas utilisee pareil dans l'original
- soit la version C# quitte trop tot la branche sable et court-circuite un chemin de locomotion encore actif dans l'original

## Cheapest Discriminating Check

Avant toute decompilation large, comparer seulement la zone originale correspondant aux cas `0x20..0x24` avec la zone C# actuelle.

Le check doit repondre a une question binaire:

- l'original execute-t-il un chemin de mouvement ou de mise a jour de vitesse pendant `InSand` / `InSandMoving` / `InSandDash` que la version C# ne reproduit pas ?

Si oui, la cause du gel est locale et la correction pourra rester locale.

## Decompilation Plan

### Phase 1: Freeze current C# behavior

Pour chaque cas sable, relever dans le memo:

- preconditions d'entree
- appels effectues
- ecritures sur:
  - `TargetAnimationId`
  - `TargetDirection`
  - `g_warpLockTimer`
  - `FinalForceX/Y/Z`
  - `ForceX/Y/Z`
  - `Flags`
- labels de sortie utilises
- conditions qui font retomber vers `Jump` / `Idle`

### Phase 2: Targeted PSX capture

Ne pas demander `pcsx_analyze_function` sur toute la fonction au debut.

Faire plutot des captures ciblees pour retrouver:

1. l'entree du switch base sur `TargetAnimationId`
2. la zone des comparaisons `0x20..0x24`
3. les appels immediats de cette zone
4. les labels de sortie locaux de cette zone

Sources prioritaires:

- PCSX web API disassembly sur petites fenetres
- Ghidra uniquement pour recoller les labels et les branches quand une fenetre n'est pas suffisante

### Phase 3: Side-by-side comparison table

Pour chaque case sable, remplir:

| Case | C# conditions | Original conditions | C# writes | Original writes | Missing call/write? |
| --- | --- | --- | --- | --- | --- |
| `0x20 EnterSand` |  |  |  |  |  |
| `0x21 ExitSand` |  |  |  |  |  |
| `0x22 InSand` |  |  |  |  |  |
| `0x23 InSandMoving` |  |  |  |  |  |
| `0x24 InSandDash` |  |  |  |  |  |

### Phase 4: Expand only if needed

Elargir la decompilation seulement si l'un des points suivants reste ouvert:

- la branche sable saute vers un label partage hors fenetre locale
- un helper appele depuis la branche sable porte en realite la locomotion utile
- la mise a jour des forces vient d'un bloc juste avant ou juste apres la zone sable

## Things That Must Be Compared

Verifier pour chaque chemin de sortie, pas seulement le cas nominal:

- `TargetAnimationId`
- `TargetDirection`
- `g_warpLockTimer`
- `g_playerWarpTimer`
- `g_playerEffectTransitionCooldown`
- appel ou non a `TryUseItem()`
- appel ou non a `UpdatePlayerCarriedEntity(...)`
- appel ou non a `MaybeStartWarpAnimation()`
- appel ou non a `AnimateWarpEffect()`
- retombee vers `Jump` / `Idle`
- ecritures eventuelles sur forces ou vitesse

## Likely Comparison Boundaries

Les limites les plus probables a fermer en premier sont:

1. la transition `EnterSand -> InSand`
2. la logique de locomotion pendant `InSand` / `InSandMoving`
3. la condition de dash pendant `InSandDash`
4. la sortie locale quand `g_warpLockTimer != 0x20`

## Stop Conditions

On s'arrete et on documente si:

- la fenetre ASM locale ne permet pas de fermer les labels de sortie
- un helper appele depuis la zone sable devient le vrai controleur du mouvement
- la comparaison impose de comprendre une zone beaucoup plus large de `MovePlayer`

Dans ce cas, le prochain pas minimal doit etre note avant d'elargir.

## Initial Working Notes

- La logique C# actuelle donne une forte suspicion sur le test `g_warpLockTimer == 0x20`.
- Si l'original n'utilise pas exactement ce garde-fou, ou s'il execute encore un chemin de mouvement commun ensuite, le gel est explique.
- Le correctif devra etre local au slice sable sauf preuve contraire.

## Original Comparison Notes

### Closed: input setup before the animation switch

Original ASM confirme le meme preambule que le C# avant le switch d'animation:

- `0x80031CD4..0x80031CDC` : lecture du pad, puis `buttonsHold = ButtonsHold >> 12`
- `0x80031CE0..0x80031CE8` : lookup dans la table de directions
- `0x80031CF0..0x80031CF8` : si le resultat vaut `-1`, repli sur `PlayerEntity.TargetDirection`

Conclusion:

- le calcul de `buttonsHold` et `dir` utilise par la branche sable est coherent avec le C# actuel

### Closed: `InSand` / `InSandMoving`

Bloc original ferme a `0x800324B8`.

Sequence originale:

- delay slot du `jal 0x8002ED64` : ecriture de `TargetDirection = dir`
- appel `TryUseItem()`
- si retour `0`, sortie immediate vers la fin de `MovePlayer`
- lecture de `g_warpLockTimer`
- si `g_warpLockTimer != 0x20`, saut vers le label commun `0x8003253C`
- sinon:
  - si `ButtonsJustPressed & 0xD0` != 0 : `TargetAnimationId = 0x24`
  - sinon si `buttonsHold != 0` : `TargetAnimationId = 0x23`
  - sinon : `TargetAnimationId = 0x22`

Conclusion:

- la branche C# actuelle pour `InSand` / `InSandMoving` est structurellement conforme a l'original sur ce slice

### Closed: `EnterSand` / `InSandDash`

Bloc original ferme a `0x8003251C`.

Sequence originale:

- appel `TryUseItem()`
- si retour `0`, sortie immediate
- lecture de `g_warpLockTimer`
- si `g_warpLockTimer == 0x20`, sortie immediate
- sinon saut vers le label commun `0x8003253C`

Conclusion:

- la branche C# actuelle pour `EnterSand` / `InSandDash` est structurellement conforme a l'original sur ce slice

### Closed: shared sand exit label

Label original ferme a `0x8003253C`.

Sequence originale:

- lecture de `PlayerEntity.IsOnGround`
- si `IsOnGround == 0` : `TargetAnimationId = 0x2D` (`Jump`)
- sinon : `TargetAnimationId = 0x00` (`Idle`)

Conclusion:

- le chemin C# commun `Jump` / `Idle` apres la logique sable correspond a l'original

### Closed: `ProcessSandCapeUseSequence()`

Helper original ferme a `0x80035260`.

Sequence originale:

- si `TargetAnimationId - 0x20 < 5`
- et `IsOnGround != 0`
- et `Slope_18c != 4`
- alors retour `0`
- sinon appel `AnimateWarpEffect()` puis retour `1`

Conclusion:

- le helper C# actuel est conforme a l'original sur le controle de `g_warpLockTimer == 0x20`

## Current Conclusion

Le slice sable compare dans `MovePlayer` ne montre pas de divergence locale claire entre C# et original.

La cause la plus probable n'est donc plus:

- ni le calcul de direction avant le switch
- ni les cas `EnterSand` / `InSand` / `InSandMoving` / `InSandDash`
- ni `ProcessSandCapeUseSequence()`

Le prochain suspect local le plus probable est la progression d'etat qui fait sortir `EnterSand` vers `InSand` puis vers une locomotion effective, ou une autre fonction qui applique le mouvement reel en fonction de l'animation sable.

## Movement Application Follow-up

### Closed: runtime update order for effective movement

Le pipeline C# qui transforme l'animation joueur en deplacement est:

1. `EntityManager.UpdateEntitiesEvents()` appelle `PlayerManager.MovePlayer()`
2. `EntityManager.UpdateEntitiesAnimation()` appelle `UpdateAnimation(entity)`
3. `PhysicsEngine.UpdateEntitiesPhysics()` appelle `UpdateEntitiesForces()` puis `MoveEntity()`

Conclusion:

- `MovePlayer()` choisit l'etat
- `UpdateAnimation()` charge l'`AnimationSet`
- `PhysicsEngine` applique ensuite les forces et le deplacement reels

### Closed: `UpdateEntityPhysics()` original vs C#

Original compare:

- `0x80036614..0x800366F4`

Comportement original ferme:

- retour immediat si `Speed`, `CurrentDirection` et `Acceleration low nibble` ne changent pas
- sinon:
  - `CurrentDirection = TargetDirection`
  - `Speed = AnimationSet.Speed`
  - `Acceleration = AnimationSet.Acceleration & 0x0F`
  - `TargetForceX = g_offsetXList[TargetDirection] * Speed`
  - `TargetForceY = g_offsetYList[TargetDirection] * Speed`
  - `ForceStepX = abs(TargetForceX - ForceX) >> Acceleration`
  - `ForceStepY = abs(TargetForceY - ForceY) >> Acceleration`

Conclusion:

- le C# `PhysicsEngine.UpdateEntityPhysics()` est structurellement conforme a l'original sur ce calcul

### Closed: `IncrementForce()` original vs C#

Original compare:

- `0x800367E4..0x80036824`

Comportement original ferme:

- pas de clamp special pour forcer un pas minimal
- la fonction ajoute ou retire `step`, puis clamp seulement si le resultat depasse la cible

Conclusion:

- le C# `PhysicsEngine.IncrementForce()` est conforme a l'original
- le gel sable n'est pas explique par un oubli C# de pas minimal dans `IncrementForce()`

### Closed: player sand animation movement data in C# assets

Probe C# execute sur `DATAS.BIN` via l'assembly `AlundraEngine.dll`, sprite joueur `SpriteRecords[0]`:

- `0x20 EnterSand`: `Speed = 0`, `Acceleration raw = 208`, `Acceleration low nibble = 0`
- `0x21 ExitSand`: `Speed = 0`, `Acceleration raw = 208`, `Acceleration low nibble = 0`
- `0x22 InSand`: `Speed = 0`, `Acceleration raw = 212`, `Acceleration low nibble = 4`
- `0x23 InSandMoving`: `Speed = 128`, `Acceleration raw = 209`, `Acceleration low nibble = 1`
- `0x24 InSandDash`: `Speed = 288`, `Acceleration raw = 208`, `Acceleration low nibble = 0`

Conclusion:

- les donnees C# chargees pour les anims sable de mouvement ne sont pas nulles
- `InSandMoving` et `InSandDash` peuvent produire une force de deplacement via le pipeline physique normal
- si le joueur ne bouge pas, le probleme n'est probablement pas un `AnimationSet.Speed` nul pour `0x23` / `0x24`

### Current conclusion after movement follow-up

Le suivi du deplacement effectif deplace le soupcon vers l'amont:

- soit le joueur ne progresse pas reellement jusque `InSandMoving` / `InSandDash`
- soit les forces sont ensuite annulees plus tard par collision / obstacle / etat runtime

Le prochain check discriminant le plus rentable est runtime:

- verifier si le joueur entre vraiment en `0x23` / `0x24` quand une direction est tenue en sable
- verifier simultanement `TargetForceX/Y`, `ForceX/Y`, `FinalForceX/Y` et `ForceAdjusted`

## Collision Follow-up

### Closed: `GetCollisionFlagsWithPlayer()` original vs C#

Original compare:

- `0x80037488..0x800375DC`

Comportement original ferme:

- le bypass collision debug ne retourne `0` que si:
  - le bit de signe de `g_debugState` est actif
  - et `g_debugFlags & 0x80000000` est non nul
- le masque joueur local vaut:
  - `0x41` si `PlayerEntity.Flags & 0x8`
  - sinon `0x40`
  - puis `|= 0x1000` si `PlayerEntity.Flags & 0x1`
- pour chacun des 4 coins:
  - collision si `(Walkability | (GroundProperty << 8)) & flag` est non nul
  - ou si `MapHeights[i] >= ModdedPosZ`
- collision supplementaire si `g_gravityFlag < 2` et `(tileFlags & 0x0E00) == 0x0800`
- collision supplementaire si:
  - `g_warpLockTimer == 0x20`
  - `ModdedPosZ != MapHeights[i] + 1`
  - ou `(tileFlags & 0x0E00) != 0x0600`

Semantique fermee:

- quand le joueur est en etat sable actif (`g_warpLockTimer == 0x20`), une tuile sable `0x0600` au niveau exact `MapHeights[i] + 1` reste traversable
- le helper original ne force donc pas une collision sur le sable actif lui-meme
- il force une collision seulement si le joueur essaye d'aller ailleurs que sur ce cas autorise

Note locale fermee:

- le dernier test C# lit `MapTile.Flags & 0x0E00`
- cela reste equivalent ici, car `MapTile.Flags` repacke `Walkability | (GroundProperty << 8)` dans les 16 bits bas, comme le `lhu` original

Conclusion:

- la logique C# n'etait pas conforme a l'original sur cette branche sable precise
- la version C# avait inverse ce test et forcait une collision sur les tuiles sable actives elles-memes
- ce point explique directement le blocage observe en `InSandMoving`
- le bypass debug en tete de fonction restait aussi une divergence fermee, mais il n'est pas la cause du blocage sable

### Current collision conclusion

La cause locale du blocage sable est maintenant fermee dans le corps de `GetCollisionFlagsWithPlayer()`.

Le prochain hop local utile n'est plus de chercher une autre cause, mais de verifier le comportement live apres correction:

- confirmer que l'entree sable via `Rond` mene bien ensuite a un commit de `PosX` sur `cape de sable.json`
- conserver `player-xy-move` comme probe discriminant si une autre divergence reapparait

## ComputeXYPosition Follow-up

### Closed: `ComputeXYPosition()` obstacle gate original vs C#

Original compare:

- `0x80037730..0x80037A4C`

Chemin original ferme autour du point suspect:

- apres le deplacement speculatif et le recalcul de terrain, l'original appelle `FindEntityCollisionCandidate()`
- si une entite bloque, saut immediat vers `0x80037938`
- sinon appel indirect de la fonction collision choisie plus tot:
  - joueur: `0x80037488`
  - autres entites: `0x800373E4`
- `0x80037930`: si le retour vaut `0`, saut vers `0x80037D68` (`NO_OBSTACLE_PATH`)
- sinon chute directe vers `0x80037938`
- `0x80037938`: restauration de `PosX`, `PosY`, `PosZ`
- ensuite l'original ne fige pas encore l'entite:
  - il coupe `dx` et `dy` par 2
  - retente un essai
  - puis peut encore passer dans les ajustements directionnels avant le vrai `FINAL_OBSTACLE`

Conclusion:

- le comportement C# actuel est conforme a l'original sur le point exact souleve:
  - tout `flags != 0` provoque bien un rollback vers la position precedente
- `flags == 1` n'a pas de semantique speciale ici:
  - c'est seulement le OR des 4 slots `collisionFlags[]`
  - tout retour non nul prend la meme branche obstacle
- le rollback a `OBSTACLE_PATH` n'est donc pas une divergence C#
- si Alundra reste immobile, cela veut dire que les retries et/ou les ajustements directionnels rencontrent encore un blocage ensuite

### Closed: `ComputeEntityGroundHeight()` core height fill original vs C#

Original compare:

- `0x800370C4..0x80037360`

Points fermes:

- meme collecte des 4 coins du hitbox
- meme clamp des tuiles X/Y en bord de map
- meme alimentation de `MapTiles[i]`
- meme calcul de hauteur de base `tile.Height << 20`
- meme traitement des pentes `Slope & 3`
- meme ecriture de `MapHeights[i]`
- meme selection du `highest`

Conclusion:

- le calcul central de `MapHeights` juste avant le test de collision ne montre pas d'ecart structurel clair sur ce slice

### Current conclusion after ComputeXYPosition follow-up

Le gel sable ne s'explique pas par un ecart de controle de flux evident dans:

- `GetCollisionFlagsWithPlayer()`
- `ComputeXYPosition()` sur la branche `CHECK_ENTITY_COLLISION -> OBSTACLE_PATH`
- `ComputeEntityGroundHeight()` sur le remplissage principal de `MapTiles/MapHeights`

Le prochain check discriminant utile devient plutot runtime ou tres local sur les donnees:

- verifier quelles cases de `collisionFlags[0..3]` montent pendant `InSandMoving` / `InSandDash`
- verifier quelles valeurs exactes de `MapHeights[i]`, `ModdedPosZ` et `MapTiles[i].GroundProperty` alimentent ce non-zero

## Runtime Probe Follow-up

### Closed: live repro save-state now really equips a usable sand cape

Verification locale fermee sur `72 - bonaire's dream sand cape repro.json` puis dans le runtime C#:

- `PlayerStats.ItemId = 32`
- `NumberOfItems[65] = 1`
- la formule de `TryUseItem()` confirme bien que le compteur utile pour l'item `0x20` est `NumberOfItems[itemId * 2 + 1]`, donc `65`

Conclusion:

- le repro live charge maintenant bien une cape de sable utilisable

### Closed: the new `player-collision` runtime snapshot builds and responds

Validation effectuee:

- build `AlundraEngine.csproj` vert apres correction du DTO runtime (`GravityFlag` devait rester en `uint`)
- build `AlundraGame.csproj` vert
- requete `player-collision` repond dans le runtime live

### Closed: current repro spawn is not yet on sand

Snapshots live obtenus apres chargement du save-state corrige:

- checkpoint: `AlundraGame.Update`
- `collisionFlagsOr = 0`
- `collisionFlags = [0, 0, 0, 0]`
- `tileAttributes = 0`
- `Slope_18c = 0`
- pour les 4 coins:
  - `GroundProperty = 0`
  - `Slope = 4`
  - `Height = 14`

Consequence locale fermee:

- `TryUseSandCape()` ne peut pas encore reussir a cette position, car son pre-requis ferme est `Slope_18c == 3`
- l'absence de reaction lors de l'appui sur `I` a cet emplacement est donc coherente avec le code C# actuel

### Closed: Win32 input injection reaches the live MonoGame window

Probe live:

- maintien `Right Arrow` via `keybd_event`
- snapshot runtime pendant l'appui:
  - `CurrentAnimationId = 1`
  - `TargetAnimationId = 1`
  - `FinalForceX = 159744`
  - `ForceAdjusted = 1`
  - `collisionFlagsOr = 0`
  - `PosX` a bien augmente

Conclusion:

- l'inspection runtime et l'injection clavier permettent maintenant de tester le bug en live sans instrumentation gameplay supplementaire

### Closed: immediate left/right neighborhood stayed non-sand during the probe

Probe live court autour du point de repro:

- deplacement vers la droite: mouvement observe, mais `Slope_18c` reste `0`
- deplacement vers la gauche: mouvement observe, `FinalForceX = -159744`, mais `Slope_18c` reste `0`
- sur ces probes, `collisionFlagsOr` reste `0`

Conclusion:

- le point de repro charge n'est pas encore exactement sur une zone sable activable
- le prochain pas runtime utile n'est plus de reverifier `ComputeXYPosition()`, mais de trouver une case voisine ou `Slope_18c` passe a `3`, puis de reprendre le snapshot `player-collision` pendant `EnterSand` / `InSandMoving`

## Original Live Sand-Movement Capture

### Closed: runtime player base and active sand state in the original

Probe PCSX ferme sur le slot joueur original:

- base joueur: `g_entitySlots[0] @ 0x80127D30`
- offsets fermes suivis pendant le probe:
  - `TargetAnimationId @ +0x88`
  - `CurrentAnimationId @ +0x90`
  - `TargetDirection @ +0x8C`
  - `CurrentDirection @ +0x94`
  - `TargetForceX/Y @ +0xBC/+0xC0`
  - `ForceX/Y @ +0xC4/+0xC8`
  - `ForceStepX/Y @ +0xD4/+0xD8`
  - `FinalForceX/Y @ +0xE4/+0xE8`
  - `Acceleration @ +0xF0`
  - `Speed @ +0xF4`
  - `Slope_18c @ +0x18C`

Etat observe au repos dans le sable actif:

- `TargetAnimationId = 0x22`
- `CurrentAnimationId = 0x22`
- `TargetDirection = 0x08`
- `CurrentDirection = 0x08`
- `Speed = 0`
- `TargetForceX = 0`
- `ForceX = 0`
- `FinalForceX = 0`
- `Slope_18c = 3`
- `IsOnGround = 1`

Conclusion:

- l'original est bien dans l'etat `InSand` actif et pret a bouger

### Closed: live transition `InSand -> InSandMoving` in the original

Probe PCSX avec maintien `right` ferme:

- avant appui:
  - `TargetAnimationId = 0x22`
  - `CurrentAnimationId = 0x22`
  - `Speed = 0`
  - `TargetForceX = 0`
  - `ForceX = 0`
  - `FinalForceX = 0`
- pendant l'appui:
  - `TargetAnimationId = 0x23`
  - `CurrentAnimationId = 0x23`
  - `TargetDirection = 0x18`
  - `CurrentDirection = 0x18`
  - `Acceleration = 1`
  - `Speed = 128`
  - `TargetForceX = 98304` (`0x18000`)
  - `ForceStepX = 49152` (`0xC000`)
  - `ForceX = 49152` (`0xC000`)
  - `FinalForceX = 49152` (`0xC000`)
  - `TargetForceY = 0`
  - `ForceY = 0`
  - `FinalForceY = 0`
  - `Slope_18c = 3`
  - `ForceAdjusted = 1`
- apres relachement:
  - `TargetAnimationId = 0x22`
  - `CurrentAnimationId = 0x22`
  - `Speed = 0`
  - `TargetForceX = 0`
  - `ForceX = 0`
  - `FinalForceX = 0`

Conclusion:

- l'original ne passe pas par un helper cache special au moment ou le joueur commence a bouger dans le sable
- le chemin utile est bien:
  - `InSand (0x22)` au repos
  - `InSandMoving (0x23)` quand une direction est tenue
  - retour `InSand (0x22)` quand la direction est relachee

### Closed: original code path that produces the sand movement values

Disassembly PCSX fermee sur les blocs utiles:

- `0x800324B8`:
  - `TryUseItem()`
  - si `g_warpLockTimer == 0x20` et `buttonsHold != 0`
  - alors `TargetAnimationId = 0x23`
  - sinon `TargetAnimationId = 0x22`
  - si `ButtonsJustPressed & 0xD0` alors `TargetAnimationId = 0x24`
- `0x80036614..0x800366F4`:
  - lit `AnimationSet.Speed`
  - ecrit `CurrentDirection = TargetDirection`
  - calcule `TargetForceX = g_offsetXList[dir] * Speed`
  - calcule `TargetForceY = g_offsetYList[dir] * Speed`
  - ecrit `Acceleration = AnimationSet.Acceleration & 0x0F`
  - calcule `ForceStepX = abs(TargetForceX - ForceX) >> Acceleration`
  - calcule `ForceStepY = abs(TargetForceY - ForceY) >> Acceleration`
- `0x800367E4..0x80036824`:
  - incremente ou decremente `ForceX/Y` vers la cible par `step`
  - clamp seulement au depassement

### Closed: numeric comparison against the current C# port

Les valeurs runtime originales observees ferment exactement le calcul attendu du port C#:

- C# `MovePlayer()` met `TargetAnimationId = InSandMoving (0x23)` quand `buttonsHold != 0` dans l'etat sable
- les donnees d'anim deja fermees cote C# donnent pour `InSandMoving`:
  - `Speed = 128`
  - `Acceleration low nibble = 1`
- C# `g_offsetXList[0x18] = 0x300`
- donc cote C#:
  - `TargetForceX = 0x300 * 128 = 0x18000 = 98304`
  - `ForceStepX = abs(0x18000 - 0) >> 1 = 0xC000 = 49152`
  - `IncrementForce(0, 0x18000, 0xC000) = 0xC000 = 49152`

Conclusion:

- sur le chemin effectivement observe dans l'original, le port C# actuel suit la meme chaine de donnees et le meme controle de flux pour produire le mouvement sable nominal
- la cause du bug sable deja observe n'est donc pas un manque de locomotion de base dans `InSandMoving`
- si le C# se fige encore dans un autre repro, le prochain suspect redevient un etat runtime local du repro fautif, pas le chemin nominal `0x22 -> 0x23 -> UpdateEntityPhysics -> IncrementForce`

## Live C# Same-Moment Comparison

### Closed: aligned C# rest state matches the original active sand rest state

Probe runtime C# aligne sur le meme moment sable:

- checkpoint runtime disponible via la pipe `alundra-csharp-runtime`
- etat observe au repos:
  - `TargetAnimationId = 0x22`
  - `CurrentAnimationId = 0x22`
  - `TargetDirection = 0x08`
  - `CurrentDirection = 0x08`
  - `Acceleration = 4`
  - `Speed = 0`
  - `TargetForceX = 0`
  - `ForceX = 0`
  - `ForceStepX = 3072`
  - `FinalForceX = 0`
  - `Slope_18c = 3`
  - `ForceAdjusted = 0`
  - `warpLockTimer = 0x20`

Conclusion:

- le repos sable actif du C# est coherent avec l'original sur les donnees utiles

### Closed: aligned C# moving-sand force path matches the original numeric path

Probe runtime C# avec maintien `Right Arrow` sur la fenetre live:

- etat observe pendant `InSandMoving`:
  - `TargetAnimationId = 0x23`
  - `CurrentAnimationId = 0x23`
  - `TargetDirection = 0x18`
  - `CurrentDirection = 0x18`
  - `Acceleration = 1`
  - `Speed = 128`
  - `TargetForceX = 98304`
  - `ForceStepX = 49152`
  - `ForceX = 49152`
  - `FinalForceX = 49152`
  - `TargetForceY = 0`
  - `ForceY = 0`
  - `FinalForceY = 0`
  - `Slope_18c = 3`
  - `ForceAdjusted = 1`

Conclusion:

- la transition C# `0x22 -> 0x23` et toute la chaine de donnees de force numeriques matchent l'original observe dans PCSX
- le bug sable C# n'est donc pas dans:
  - le choix de l'animation sable
  - le choix de direction
  - `UpdateEntityPhysics()`
  - `IncrementForce()`
  - la production de `FinalForceX`

### Closed: the C# divergence now appears after force generation, at movement commit time

Probe runtime C# sur plusieurs checkpoints successifs pendant le maintien droite:

- checkpoints observes:
  - `AlundraGame.Update`
  - `AlundraGame.Draw`
  - `GameEngine.Update`
- les checkpoints sont tous avant la logique frame suivante:
  - `AlundraGame.Update` est avant `_inputManager.Update()`
  - `AlundraGame.Draw` est avant `_gameEngine.MainLoop()`
  - `GameEngine.Update` checkpoint est au debut de `GameEngine.Update(...)`
- malgre cela, sur plusieurs frames successives en `0x23`:
  - `FinalForceX` reste a `49152`
  - `ForceAdjusted` reste a `1`
  - `CollisionFlagsOr` reste a `1`
  - `PosX` reste strictement inchange

Conclusion:

- le C# atteint bien le meme etat de locomotion sable que l'original jusqu'a `FinalForceX`
- mais le deplacement horizontal n'est pas committe d'une frame a l'autre
- la divergence locale la plus probable est maintenant en aval de la generation de force, dans:
  - `ApplyEntityForces()` / `MoveEntity()` / `ComputeXYPosition()`
  - ou dans les donnees collision utilisees au moment du commit
- le couple runtime discriminant ferme est maintenant:
  - original: `0x23` + `FinalForceX != 0` mene a un deplacement observable
  - C#: `0x23` + `FinalForceX != 0` persiste, mais `PosX` ne bouge pas

## Final Root Cause Closure

### Closed: player movement rollback came from an inverted active-sand collision clause

Preuve locale fermee:

- `ComputeXYPosition()` original a `0x80037730` choisit bien `GetCollisionFlagsWithPlayer()` pour le joueur, pas `GetCollisionFlags()`
- le helper original `GetCollisionFlagsWithPlayer()` a `0x80037488` ne bloque pas le sable actif lui-meme
- sous `g_warpLockTimer == 0x20`, il force une collision seulement si:
  - `ModdedPosZ != MapHeights[i] + 1`
  - ou `(tileFlags & 0x0E00) != 0x0600`

Consequence:

- le cas autorise en sable actif est exactement:
  - `ModdedPosZ == MapHeights[i] + 1`
  - et `(tileFlags & 0x0E00) == 0x0600`
- la version C# avait inverse cette logique et marquait ce cas autorise comme collision
- `ComputeXYPosition()` recevait alors `flags != 0`, faisait un rollback de `PosX`, puis epuisait ses retries sans commit horizontal

Correctif applique:

- restauration du choix original du helper collision joueur dans `ComputeXYPosition()`
- correction de la branche `g_warpLockTimer == 0x20` dans `GetCollisionFlagsWithPlayer()` pour suivre la semantique originale PSX

Etat final ferme cote code:

- le root cause est corrige a la source dans le helper collision translittere
- la validation live finale sur `cape de sable.json` est fermee

### Closed: live validation on `cape de sable.json`

Validation finale fermee sur l'instance live ouverte avec `--datas-bin`, apres activation manuelle de la cape via `Rond`:

- etat juste avant le maintien droite:
  - `TargetAnimationId = 0x22`
  - `CurrentAnimationId = 0x22`
  - `warpLockTimer = 0x20`
  - `Slope_18c = 3`
  - `PosX = 18087936`
- pendant le maintien droite apres correctif:
  - `TargetAnimationId = 0x23`
  - `CurrentAnimationId = 0x23`
  - `FinalForceX = 49152`
  - `collisionFlagsOr = 0`
  - `player-xy-move.exitPath = ReturnResult`
  - `player-xy-move.attemptedPosX == player-xy-move.exitPosX`
  - `PosX` progresse `18087936 -> 18137088 -> 18186240 -> 18235392 ... -> 19267584`

Conclusion:

- le rollback observe avant correctif a disparu
- `ComputeXYPosition()` ne recoit plus de collision forcee sur le sable actif
- le deplacement sable du port C# est a nouveau coherent avec le comportement original ferme