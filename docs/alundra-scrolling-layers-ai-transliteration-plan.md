# Plan agent IA - translitterer les layers de scrolling Alundra

Ce document est un plan d'execution pour un agent IA peu performant. Il doit avancer par petites preuves, sans inventer d'architecture, afin de porter le rendu des layers de scrolling du jeu original vers le runtime C#.

## Objectif

Porter en C# le rendu des layers de scrolling Alundra, en partant de la fonction originale `RenderAllTileLayers @ 0x8005B670`, du portage C dans `Alundraportage/src/Loader/level.h` et `Alundraportage/src/Loader/level.cpp`, et des donnees deja chargees dans `AlundraTools/AlundraEngine/DatasBin/GameMap.cs` via `GameMap.ScrollParameters`.

Le premier jalon visible est la scene chargee par le savestate 1 de PCSX-Redux: elle contient un scrolling foreground de nuages. Le rendu C# doit reproduire ce scrolling au moins pour ce cas avant de generaliser.

## Regles non negociables pour l'agent

1. Ne pas refaire un moteur de rendu moderne.
2. Ne pas remplacer les donnees originales par des `List<>`, `Dictionary<>`, LINQ ou un manager invente dans le coeur du portage.
3. Ne pas renommer les inconnues avec des noms metier speculatifs.
4. Ne pas faire confiance au port C quand il contredit Ghidra ou les donnees en memoire.
5. Toujours fermer les tailles de structures et les offsets par Ghidra, par dump memoire, ou par comparaison binaire.
6. Ajouter un commentaire `GHIDRA:` au-dessus de chaque fonction ou globale translitteree depuis l'original.
7. Ajouter un commentaire `JUSTIFICATION:` au-dessus de chaque helper C# qui n'existe pas dans l'original.
8. Si un point n'est pas prouve, laisser le nom brut et ajouter `PARTIAL:` ou `BLOCKED:`.
9. Toujours verifier les chemins d'echec et les early exits, pas seulement le cas ou les nuages apparaissent.
10. Avancer en commits ou changements monotoniques, sans refactor esthetique.

## Fichiers a connaitre

### Portage C de reference

- `Alundraportage/src/Loader/level.h`
- `Alundraportage/src/Loader/level.cpp`

Classes et fonctions utiles:

- `LiningHeader`
- `LiningInfos`
- `LayerInfos`
- `Scrollar`
- `Cellular`
- `Cell`
- `Overlay`
- `OverlayExt`
- `Lining::Init`
- `Lining::InitScrollar`
- `Lining::SetLining`
- `Lining::SetScrollar`
- `Lining::SetCellular`
- `Lining::SetOverlay`
- `Level::DrawScene`

Le port C est une aide importante, mais il ne remplace pas Ghidra. Utiliser le C pour comprendre le comportement attendu, puis verifier les offsets, les tailles et le controle de flux dans Ghidra.

### Runtime C# actuel

- `AlundraTools/AlundraEngine/DatasBin/GameMap.cs`
- `AlundraTools/AlundraEngine/DatasBin/ScrollScreen.cs`
- `AlundraTools/AlundraEngine/DatasBin/GameMapHeader.cs`
- `AlundraTools/AlundraEngine/Graphics/GraphicManager.cs`
- `AlundraTools/AlundraEngine/Graphics/Renderer.cs`
- `AlundraTools/AlundraEngine/Graphics/IRenderer.cs`
- `AlundraTools/AlundraEngine/Graphics/SpriteDepth.cs`
- `AlundraTools/AlundraEngine/StaticVariables.cs`
- `AlundraTools/AlundraGameRuntimeMcpServer/RuntimeHelperTools.cs`
- `docs/pcsx-redux-mcp-tools.md`

Fonctions et champs deja presents:

- `GameMap.ScrollParameters`
- `ScrollParameters.TileSheetBitmap`
- `GraphicManager.RenderAllTileLayers`
- `GraphicManager.UpdateScrollingTileAnimation`
- `GraphicManager.RenderLayerToBuffer`
- `GraphicManager.RenderTileOverlayLayer`
- `StaticVariables.g_numberOfLayersDrawn`
- `StaticVariables.g_tileAnimationType`
- `StaticVariables.g_tileAnimationMode`
- `StaticVariables.g_renderingBufferIndex`
- `StaticVariables.INT_800c48c4`
- `StaticVariables.g_cameraScrollingX`
- `StaticVariables.g_cameraScrollingY`

## Etat actuel observe dans le code C#

1. `GameMap.Load` instancie `ScrollParameters` quand `Header.ScrollingScreenOffset != -1`.
2. `ScrollParameters` lit deja l'en-tete type `LiningHeader` et cree `TileSheetBitmap`.
3. `ScrollParameters` ne conserve pas encore assez de donnees pour rendre les layers: `LiningInfos` n'est pas expose, les `LayerInfos` ne sont pas stockes, les scrollars/cellular/cells ne sont pas stockes.
4. `GameMap.ScrollScreen` existe encore comme ancien champ nullable, mais il n'est pas alimente par le chargement actuel.
5. `GraphicManager.RenderAllTileLayers @ 0x8005B670` teste `CurrentMap.ScrollScreen`, donc le rendu des layers ne peut probablement pas partir de `CurrentMap.ScrollParameters` aujourd'hui.
6. `RenderLayerToBuffer @ 0x8005B848` et `RenderTileOverlayLayer @ 0x8005BA40` sont des stubs avec `NotImplementedException`.
7. Le parseur `ScrollScreen` lit les champs `Scrollar` en `int32`, alors que le port C declare `Scrollar` en huit `int8_t`. Ce point doit etre verifie dans Ghidra et corrige seulement apres preuve.
8. Le parseur `ScrollParameters` semble comparer `Layers[layerID]` a `1` ou `2`, alors que le port C choisit le mode par `LiningInfos.ModeLayer[layerID]`. Verifier dans Ghidra avant de corriger.
9. `TileSheetBitmap` est cree avec une seule palette. Or les entrees de map scroll ont un `palDex`. Pour reproduire le rendu, il faudra probablement conserver les palettes et produire un bitmap par palette, ou extraire des tuiles palettees a la demande. C'est une adaptation backend, pas une logique metier.

## Source de verite et niveau de preuve

Classer chaque decision:

- Closed: confirme par Ghidra, dump PCSX, ou donnees binaires.
- Partial: controle de flux compris, semantique partielle.
- Blocked: manque de preuve.

Sources a utiliser dans cet ordre:

1. Ghidra sur `SLUS_006.62`.
2. PCSX-Redux avec savestate 1.
3. Dumps memoire autour de la map chargee et du bloc scrolling.
4. Port C `Lining`.
5. C# existant.
6. Docs du repo.

## Phase 0 - Preparation

### Etape 0.1 - Verifier le depot

Commands conseillees:

```powershell
rtk git status
rtk git diff -- AlundraTools/AlundraEngine/DatasBin/ScrollScreen.cs AlundraTools/AlundraEngine/DatasBin/GameMap.cs AlundraTools/AlundraEngine/Graphics/GraphicManager.cs
```

Si `rtk` est indisponible, utiliser `git status` et `git diff`.

Ne jamais revert des changements existants sans demande explicite.

### Etape 0.2 - Verifier la solution cible

Solution principale:

```powershell
AlundraTools/Alundra.sln
```

Build cible minimal:

```powershell
dotnet build .\AlundraTools\AlundraGame\AlundraGame.csproj
```

Ne pas lancer une grosse correction globale si le build echoue sur un probleme sans rapport. Noter le probleme et continuer seulement si le blocage touche le scrolling.

### Etape 0.3 - Charger le savestate de reference

Avec PCSX-Redux Web Server actif:

1. Charger le savestate 1.
2. Capturer une screenshot originale.
3. Noter le map id, la camera X/Y, et si possible les modes de layers.
4. Ne pas avancer sur le rendu C# avant d'avoir une image originale de reference.

Outils PCSX utiles, selon disponibilite MCP:

- `pcsx_savestate_load` avec slot `1`.
- `pcsx_screenshot` pour l'image originale.
- `pcsx_read_memory` pour lire les blocs en RAM.
- `pcsx_analyze_function` pour obtenir un listing MIPS si Ghidra/ReVa n'est pas disponible.

Lire `docs/pcsx-redux-mcp-tools.md` avant d'appeler les outils PCSX.

## Phase 1 - Analyse Ghidra obligatoire

### Etape 1.1 - Extraire la fonction principale

Dans Ghidra, ouvrir:

```text
RenderAllTileLayers @ 0x8005B670
```

Sauver la decompilation dans:

```text
obj/render_all_tile_layers_8005b670_decomp.txt
```

Ne pas modifier le C# a cette etape.

### Etape 1.2 - Construire la table des fonctions liees

Analyser au minimum:

| Adresse | Nom C# actuel | Action |
|---|---|---|
| `0x8005B670` | `RenderAllTileLayers` | Fonction principale, controle de flux. |
| `0x8005B7A0` | `UpdateScrollingTileAnimation` | Animation des tuiles de scrolling. |
| `0x8005B848` | `RenderLayerToBuffer` | Rendu d'un layer. Stub C# actuel. |
| `0x8005BA40` | `RenderTileOverlayLayer` | Overlay/effet additionnel. Stub C# actuel. |

Pour chaque fonction, remplir cette fiche:

```text
Fonction: 
Adresse Ghidra: 
Callers: 
Callees: 
Parametres observes: 
Retour observe: 
Globales lues: 
Globales ecrites: 
Structures lues: 
Primitives PSX creees: 
Correspondance C Lining: 
Correspondance C# actuelle: 
Etat: Closed / Partial / Blocked
```

### Etape 1.3 - Identifier les globales originales

Chercher toutes les globales lues/ecrites par `RenderAllTileLayers` et ses callees. Inclure au minimum les candidates deja presentes en C#:

| Globale C# | Adresse connue | Usage attendu |
|---|---:|---|
| `g_numberOfLayersDrawn` | `0x800DC084` | Retour/compteur de rendu. |
| `g_cameraScrollingX` | `0x800E4328` | Camera X passee au rendu. |
| `g_cameraScrollingY` | `0x800E432C` | Camera Y passee au rendu. |
| `g_tileAnimationType` | `0x80181BE4` | Gate animation. |
| `g_tileAnimationMode` | `0x8018678C` | Bits d'activation layer 0/1. |
| `g_renderingBufferIndex` | `0x8018CF5C` | Double buffer. |
| `INT_800c48c4` | `0x800C48C4` | Compteur/tick inconnu. |

Si une adresse est incertaine, ne pas ajouter de commentaire `GHIDRA:` dans le code. Marquer `BLOCKED:` dans les notes.

### Etape 1.4 - Fermer le layout du bloc scrolling

Comparer Ghidra, dumps RAM et port C. Produire une table finale avant de coder.

Layout attendu par le port C:

| Offset depuis bloc scrolling | Structure | Taille C | Champs |
|---:|---|---:|---|
| `0x00` | `LiningHeader` | `0x1C` | `Graphics`, `Layers[2]`, `ScriptTable`, `Overlay`, `OverlayExt`, `WaveLUT`. |
| `0x1C` | `LiningInfos` | `0x08` | `Enabled`, `AnimNum`, `ModeLayer[2]`, `BGColor`. |
| `Layers[i] + 0x00` | `LayerInfos` | `0x04` | `Unused`, `AnimTimer`, `BlendMode`, `Ground`. |
| `Layers[i] + 0x04` | `Scrollar` | `0x08` | 8 champs signes 8-bit. |
| `Layers[i] + 0x04` | `Cellular` | `0x08` | 8 champs 8-bit. |
| `Layers[i] + 0x0C` | `Cell[]` | `0x14` par cell | Cellules cellular. |
| `Overlay` | `Overlay[]` | `0x04` par frame | RGB + hold. |
| `OverlayExt` | `OverlayExt[]` | `0x10` par frame | 4 coins RGB + hold + padding. |

Points a verifier dans Ghidra:

1. Les offsets `Graphics`, `Layers`, `Overlay`, `OverlayExt`, `WaveLUT` sont-ils relatifs au debut du bloc scrolling ?
2. `Scrollar` est-il vraiment huit `sbyte`, ou le C# actuel a-t-il une raison de lire huit `int32` ?
3. `ModeLayer` est-il le selecteur `0=off`, `1=scrollar`, `2=cellular` ?
4. `LayerInfos.Ground` choisit-il background vs foreground ?
5. Le tile map de scroll est-il a `Graphics + 0x8100` ?
6. Le second tile map est-il a `Graphics + 0x8100 + 0x960` quand les deux layers sont scrollar ?
7. La grille logique scrollar est-elle `0x28` colonnes par `0x1E` lignes ?
8. L'ecran logique utilise-t-il `320x240` ou la hauteur C# `236` ? Ne pas supposer.
9. Le flux de decompression du graphics block est-il equivalent a `Texture::TexFromData` du port C ?

### Etape 1.5 - Comparer avec le savestate 1

Dans PCSX savestate 1:

1. Lire les valeurs de `g_currentMap`, `g_cameraScrollingX`, `g_cameraScrollingY`.
2. Trouver le bloc scrolling de la map chargee.
3. Dumper au moins `0x100` bytes a partir du debut du bloc scrolling.
4. Dumper le ou les blocs pointes par `Layers[0]` et `Layers[1]`.
5. Verifier `LiningInfos.Enabled`, `AnimNum`, `ModeLayer[0]`, `ModeLayer[1]`.
6. Identifier quel layer est le foreground de nuages via `LayerInfos.Ground`.
7. Noter les valeurs exactes dans `obj/scrolling_savestate1_notes.md`.

## Phase 2 - Analyse du port C Lining

### Etape 2.1 - Lire `Lining::Init`

Points a extraire:

1. `_infos = (LiningInfos*)(_data + 28)`.
2. Si `Enabled == 0`, retour immediat.
3. `_hasGraphx = 256 < _dataSize - _header->Graphics`.
4. Palette et graphics sont charges depuis `_data + _header->Graphics`.
5. Les etats runtime par layer sont remis a zero.
6. Le mode de layer vient de `_infos->ModeLayer[layerID]`.
7. Overlay initialise seulement si `BGColor.Alpha != 0`.

### Etape 2.2 - Lire `Lining::SetLining`

Flux a conserver:

1. `primCount = 0`.
2. Si disabled, retourner 0.
3. Si graphics presents, boucle `layerID = 1` puis `0`.
4. `ModeLayer == 1`: appeler `SetScrollar`.
5. `ModeLayer == 2`: appeler `SetCellular`.
6. Si overlay actif, appeler `SetOverlay`.
7. Retourner `primCount`.

Ne pas changer l'ordre des layers sans preuve Ghidra.

### Etape 2.3 - Lire `Lining::SetScrollar`

Transcrire les operations en pseudo-code litteral:

```text
if FactorXDenom == 0 or FactorYDenom == 0: return 0
parallaxOffsetX = cameraX * FactorXNum / FactorXDenom
parallaxOffsetY = cameraY * FactorYNum / FactorYDenom
animNum = AnimNum <= 0 ? 1 : AnimNum
anim timer/counter
update timerX, offsetX, periodX
update timerY, offsetY, periodY
if renderer not ready: return 0
screenPosX = offsetX + parallaxOffsetX
screenPosY = offsetY + parallaxOffsetY
wrap X in 0..639
wrap Y in 0..479
tileX = screenPosX >> 4
tileY = screenPosY >> 4
subX = screenPosX & 15
subY = screenPosY & 15
baseMap = data + Graphics + 0x8100 + optional second offset
visible grid rows/cols
for each visible tile:
    tx/ty wrap in 0x28/0x1E
    entry = baseMap + ty * 0x50 + tx * 2
    tileVal = entry[0]
    palDex = entry[1]
    skip tileVal == 0
    u0 = (tileVal & 0x0F) << 4
    v0 = ((tileVal & 0xF0) + vAnim) & 0xFF
    destination 16x16
    enqueue background or foreground selon LayerInfos.Ground
```

### Etape 2.4 - Lire `Level::DrawScene`

Important pour l'ordre:

1. `SetLining` est appele avant le test `drawer->IsReady()`.
2. Le background scroll est dessine avant les tiles de la map.
3. Les tiles et walls sont dessines.
4. L'overlay est dessine.
5. Le foreground scroll est dessine apres overlay dans le port C.

Comparer avec l'ordre original Ghidra. Si Ghidra differe, suivre Ghidra.

## Phase 3 - Corriger et completer les donnees C#

Ne pas commencer cette phase tant que la phase 1 n'a pas une table de layout au moins `Partial`.

### Etape 3.1 - Stabiliser `GameMap.Load`

Dans `GameMap.Load`, avant de creer `ScrollParameters`, positionner explicitement le stream:

```csharp
br.BaseStream.Position = Offset + Header.ScrollingScreenOffset;
ScrollParameters = new ScrollParameters(br, Header.StringTableOffset - Header.ScrollingScreenOffset);
```

Ajouter ce changement seulement si le diff montre que cela ne casse pas un autre chemin. C'est un garde-fou de lecture, pas une nouvelle logique de jeu.

### Etape 3.2 - Faire de `ScrollParameters` le port de `Lining`

Dans `ScrollScreen.cs`, completer `ScrollParameters` pour contenir les donnees minimales, sans manager invente.

Champs candidats a ajouter apres preuve:

```csharp
// GHIDRA: LiningInfos equivalent in scrolling block @ offset 0x1C
public LiningInfos Infos;

public bool HasGraphics;
public Color[][] Palettes;
public Bitmap[] TileSheetBitmapsByPalette;
public LayerInfos[] LayerInfos;
public ScrollScreen[] Scrollars;
public Cellular[] Cellulars;
public Cell[][] Cells;
```

Attention: le commentaire `GHIDRA:` obligatoire concerne les globales et fonctions translitterees. Pour des champs de structure sans adresse globale, utiliser plutot des commentaires de layout si necessaire, par exemple `// Layout: LiningInfos @ block + 0x1C`.

### Etape 3.3 - Corriger les types de structure

Si Ghidra confirme le port C:

1. `ScrollScreen` doit lire des `sbyte`, pas des `int32`.
2. `LayerInfos` doit lire 4 bytes.
3. `Cellular` doit lire 8 bytes.
4. `Cell` doit lire exactement 20 bytes.
5. `LiningInfos.BGColor` doit garder l'ordre exact des bytes. Le C declare `Color BGColor`; verifier l'ordre `r,g,b,alpha` dans `entities.h` ou Ghidra.

Exemple attendu pour `Scrollar` si confirme:

```csharp
public sealed class ScrollScreen
{
    public readonly sbyte FactorXNum;
    public readonly sbyte FactorXDenom;
    public readonly sbyte FactorYNum;
    public readonly sbyte FactorYDenom;
    public readonly sbyte ScrollXSpeed;
    public readonly sbyte ScrollXPeriod;
    public readonly sbyte ScrollYSpeed;
    public readonly sbyte ScrollYPeriod;
}
```

Garder le nom `ScrollScreen` seulement si le projet l'utilise deja; sinon documenter qu'il correspond au `Scrollar` du C. Ne pas renommer massivement sans besoin.

### Etape 3.4 - Lire les offsets relativement au bloc scrolling

Regle simple pour l'agent:

```text
absolute = scrollingBlockBase + relativeOffset
```

Utiliser cette regle pour:

- `Graphics`
- `Layers[0]`
- `Layers[1]`
- `ScriptTable`
- `Overlay`
- `OverlayExt`
- `WaveLUT`

Ne jamais utiliser un offset comme position absolue dans `datas.bin`.

### Etape 3.5 - Conserver l'etat runtime necessaire

Le port C `Lining` melange donnees chargees et etat runtime. Pour une translitteration proche, ajouter les tableaux d'etat dans `ScrollParameters` ou dans l'emplacement C# qui correspond le mieux a l'etat original, sans creer de manager.

Etat minimal pour Scrollar:

```text
_animFrameTimer[2]
_animFrameCounter[2]
_parallaxOffsetX[2]
_parallaxOffsetY[2]
_offsetX[2]
_offsetY[2]
_timerX[2]
_timerY[2]
_scrollDirX[2]
_scrollDirY[2]
```

Etat pour Cellular plus tard:

```text
CELL_MAX = 200
_cellPosX[2][CELL_MAX]
_cellPosY[2][CELL_MAX]
_cellTickX[2][CELL_MAX]
_cellTickY[2][CELL_MAX]
_waveTick[2]
```

Etat pour Overlay plus tard:

```text
_ovrOff
_ovrTick
_ovrHold
```

### Etape 3.6 - Initialiser comme le C

Ajouter une methode equivalente a `Lining::Init` seulement si elle correspond a une fonction originale ou a un pont langage minimal.

Option preferee pour un agent peu performant:

1. Initialiser les donnees dans le constructeur `ScrollParameters`.
2. Initialiser les etats runtime dans une methode nommee proche de l'original, par exemple `InitScrollar`, deja presente.
3. Ne pas creer `ScrollLayerManager` ou `ScrollingRenderer`.

Commentaires obligatoires:

```csharp
// GHIDRA: InitScrollar equivalent, original address to close before final naming
```

Si l'adresse Ghidra de `InitScrollar` n'est pas connue, ne pas inventer. Utiliser un commentaire `PARTIAL:` dans les notes, pas un faux `GHIDRA:`.

## Phase 4 - Porter le rendu Scrollar en C#

Cette phase vise le foreground de nuages du savestate 1. Ne pas coder Cellular/Overlay tant que Scrollar n'est pas valide.

### Etape 4.1 - Remplacer le gate de `RenderAllTileLayers`

Etat actuel a remplacer:

```csharp
if (_gameEngine.CurrentMap.ScrollScreen != null && _gameEngine.CurrentMap.ScrollScreen.ScrollYSpeed != 0)
```

Gate attendu, a fermer par Ghidra:

```text
CurrentMap.ScrollParameters != null
ScrollParameters.Infos.Enabled != 0
ScrollParameters.HasGraphics == true
au moins un ModeLayer actif
```

Ne pas garder `ScrollYSpeed != 0` comme gate global si Ghidra ne le confirme pas. Un layer peut etre visible avec vitesse nulle mais parallax camera.

### Etape 4.2 - Garder le controle de flux original

`RenderAllTileLayers @ 0x8005B670` doit rester structurellement proche de Ghidra:

1. Inverser `g_renderingBufferIndex` si l'original le fait.
2. Incrementer `INT_800c48c4` si l'original le fait.
3. Appeler `UpdateScrollingTileAnimation` si `g_tileAnimationType != 0`.
4. Tester les bits de `g_tileAnimationMode` seulement si Ghidra confirme qu'ils controlent les layers.
5. Appeler `RenderLayerToBuffer(0, ...)` puis `RenderLayerToBuffer(1, ...)` selon l'ordre original.
6. Appeler `RenderTileOverlayLayer` si la condition originale est vraie.
7. Retourner exactement le meme type de compteur que l'original.

Point suspect actuel: `additionalTiles` est calcule mais l'addition au compteur est commentee. Verifier Ghidra et corriger seulement apres preuve.

### Etape 4.3 - Implementer `RenderLayerToBuffer` pour Scrollar

Dans un premier jalon, `RenderLayerToBuffer` peut gerer seulement `ModeLayer == 1`. Les autres modes doivent retourner 0 ou etre marques `PARTIAL:` selon le flux original.

Pseudo-code cible pour Scrollar:

```text
scrollParameters = CurrentMap.ScrollParameters
infos = scrollParameters.Infos
layerInfos = scrollParameters.LayerInfos[layerId]
scrollar = scrollParameters.Scrollars[layerId]
if mode != 1: return 0
if FactorXDenom == 0 or FactorYDenom == 0: return 0
update parallax
update animation timer
update continuous offsets
update period offsets
wrap screenPosX in 0..639
wrap screenPosY in 0..479
compute tileX/tileY/subX/subY
compute baseMap offset
iterate visible grid
for each nonzero tile entry:
    compute source rect
    compute dest rect
    draw/enqueue sprite at correct depth
    primCount++
return primCount
```

### Etape 4.4 - Gerer le rendu palette

Le port C envoie `tileVal` et `palDex` au drawer. Le C# `Bitmap` a deja les couleurs appliquees. Donc le backend C# doit permettre un rendu par palette.

Option simple et acceptable comme adaptation backend:

1. Garder les donnees 4bpp brutes du graphics block.
2. Creer `Bitmap[] TileSheetBitmapsByPalette` pour les palettes disponibles.
3. Au rendu, choisir `TileSheetBitmapsByPalette[palDex]`.
4. Dessiner un sous-rectangle 16x16 du bitmap choisi.

Si `Renderer.AddSprite` ne supporte pas les rectangles source, ne pas changer tout le renderer. Ajouter un helper minimal documente:

```csharp
// JUSTIFICATION: backend GDI renderer adaptation only
```

Le helper doit seulement dessiner une region source vers une region destination. Il ne doit pas contenir de logique de scrolling.

Alternative si plus simple:

1. Extraire un bitmap 16x16 par cle `(tileVal, palDex, vAnim)`.
2. Mettre en cache avec une structure simple.
3. Le cache est backend, pas runtime original.

Ne pas utiliser ce cache pour changer le controle de flux ou ignorer des tuiles.

### Etape 4.5 - Choisir les depths sans inventer la logique

Le port C separe:

- queue background si `LayerInfos.Ground == 0`
- queue foreground si `LayerInfos.Ground != 0`

Dans C# actuel, le renderer trie par `Depth`. Faire une correspondance minimale:

- background scroll: depth sous les tiles de map.
- foreground scroll: depth au-dessus des tiles/entities, mais sous UI.

Verifier dans Ghidra les ordering tables passees a `RenderAllTileLayers`, `RenderLayerToBuffer` et `RenderTileOverlayLayer`. Si l'ordre original est clair, utiliser l'equivalent. Si l'ordre n'est pas ferme, marquer `PARTIAL:` et choisir une adaptation backend temporaire limitee au test savestate 1.

### Etape 4.6 - Respecter les dimensions originales

Verifier avant de coder:

- largeur ecran scroll: `320`
- hauteur ecran scroll: `240` ou `236`
- largeur wrap: `640`
- hauteur wrap: `480`
- taille tile scroll: `16x16`
- map scroll: `0x28 x 0x1E`
- stride row: `0x50` bytes
- deuxieme map scroll offset: `0x960`

Ne pas remplacer par `StaticVariables.MapTileWidth` ou `MapTileHeight`; ces constantes sont `24x16` pour la map principale, pas pour le layer de scrolling.

## Phase 5 - Porter Overlay et Cellular apres Scrollar

Ne pas commencer cette phase tant que le foreground de nuages est visible et stable.

### Etape 5.1 - Overlay

Analyser `RenderTileOverlayLayer @ 0x8005BA40` et comparer avec `Lining::SetOverlay`.

A fermer:

1. Condition d'activation: `BGColor.Alpha != 0` ou autre globale originale.
2. Mode simple vs extended: flag `< 0x65` ou equivalent.
3. Sequence `hold`, tick, loop quand `hold == 0`.
4. Couleurs par coin pour `OverlayExt`.
5. Blend mode PSX attendu.

Backend C# possible:

- utiliser `Renderer.AddQuadColor` si cela respecte les quatre coins.
- utiliser un helper `JUSTIFICATION: backend GDI renderer adaptation only` si necessaire.

### Etape 5.2 - Cellular

Analyser `ModeLayer == 2` et les fonctions C:

- `CellNormal`
- `CellScriptTrack`
- `CellFallRespawn`
- `CellWaveX`

Ordre conseille:

1. Parser `Cellular` et `Cell[]` correctement.
2. Initialiser `_cellPosX/Y` depuis `X0/Y0`.
3. Porter `CellNormal`.
4. Porter `CellFallRespawn`.
5. Porter `CellWaveX` avec `WaveLUT`.
6. Laisser `CellScriptTrack` a 0 si Ghidra et donnees confirment qu'il n'est jamais utilise. Sinon `BLOCKED:`.

Ne pas utiliser `System.Random` pour remplacer `std::rand()` sans preuve sur le RNG original. Si le RNG original est necessaire, utiliser la globale/fonction originale deja portee ou marquer `PARTIAL:`.

## Phase 6 - Validation

### Validation A - Donnees chargees

Pour une map avec scrolling:

1. `CurrentMap.ScrollParameters != null`.
2. `ScrollParameters.Infos.Enabled != 0`.
3. `ScrollParameters.HasGraphics == true`.
4. `ModeLayer[0]` et `ModeLayer[1]` correspondent au dump PCSX.
5. `LayerInfos[i].Ground` identifie correctement background/foreground.
6. `TileSheetBitmap` ou `TileSheetBitmapsByPalette` non null.
7. Les scrollars ont des valeurs plausibles en `sbyte`, pas des grands entiers dus a une lecture `int32`.

### Validation B - Build

Commande:

```powershell
dotnet build .\AlundraTools\AlundraGame\AlundraGame.csproj
```

Si possible, lancer aussi:

```powershell
dotnet build .\AlundraTools\AlundraGameRuntimeMcpServer\AlundraGameRuntimeMcpServer.csproj
```

### Validation C - Scene savestate 1

1. Charger savestate 1 dans PCSX-Redux.
2. Capturer screenshot originale.
3. Lancer C# sur la meme map ou charger le meme etat si le runtime le permet.
4. Verifier que les nuages foreground sont visibles.
5. Laisser tourner au moins 60 frames.
6. Verifier que les nuages bougent dans la meme direction que l'original.
7. Verifier que le foreground passe devant les elements qui doivent etre derriere lui.
8. Verifier qu'il n'y a pas de seam visible au wrap X/Y.
9. Verifier que la palette correspond a l'original.
10. Noter les ecarts dans le rapport final.

### Validation D - Comparaison compteur/etat

Sur 10 frames consecutives, comparer si possible:

| Frame | PCSX camera X | PCSX camera Y | C# camera X | C# camera Y | layer | offsetX | offsetY | primCount |
|---:|---:|---:|---:|---:|---:|---:|---:|---:|

Si les offsets divergent, inspecter dans cet ordre:

1. Signe des champs `sbyte`.
2. Division entiere et denominator.
3. Period sign et `scrollDirX/Y`.
4. Ordre des updates timer/offset.
5. Wrap 640/480.
6. Camera utilisee avant/apres update.

## Phase 7 - Documentation des preuves

Creer ou mettre a jour un fichier de notes, par exemple:

```text
obj/scrolling_layers_evidence.md
```

Contenu minimal:

```text
Savestate: 1
Map id:
Function RenderAllTileLayers: 0x8005B670
Function RenderLayerToBuffer: 0x8005B848
Function RenderTileOverlayLayer: 0x8005BA40
Scrolling block offset in datas.bin:
Scrolling block address in RAM:
ModeLayer[0]:
ModeLayer[1]:
Foreground layer id:
Graphics offset:
Layer[0] offset:
Layer[1] offset:
Tile map base:
Palette count:
Known deviations C#:
Blocked points:
```

## Pieges connus

1. Ne pas confondre `Layers[layerID]` avec `ModeLayer[layerID]`.
2. Ne pas lire `Scrollar` en `int32` si Ghidra confirme des bytes signes.
3. Ne pas utiliser les tiles `24x16` de la map principale pour les scroll layers `16x16`.
4. Ne pas utiliser `StaticVariables.ScreenHeight = 236` sans verifier que le layer original n'utilise pas `240`.
5. Ne pas ignorer `palDex`.
6. Ne pas oublier que le graphics block commence par les palettes avant les pixels.
7. Ne pas supposer que le C# `TileSheetBitmap` palette 0 suffit.
8. Ne pas sauter les frames de tick quand le renderer ne dessine pas. Le port C update les timers avant le gate `IsReady`.
9. Ne pas changer l'ordre background/map/overlay/foreground sans preuve.
10. Ne pas corriger des comportements etranges de l'original sous pretexte qu'ils semblent bizarres.
11. Ne pas masquer un `NotImplementedException` par un retour silencieux si la fonction originale devrait etre appelee.
12. Ne pas faire de gros refactors dans `Renderer` pour ce besoin.
13. Ne pas effacer `GameMap.ScrollScreen` sans verifier les usages existants. Preferer ne plus l'utiliser pour le rendu des layers et documenter son statut.
14. Ne pas oublier que `RenderAllTileLayers` retourne un compteur. Verifier si les deux layers doivent etre additionnes.
15. Ne pas oublier les chemins disabled: no scrolling block, `Enabled == 0`, no graphics, denominator zero, tileVal zero, out of bounds.

## Micro-plan de changement conseille

### Changement 1 - Evidence only

Livrable:

- `obj/render_all_tile_layers_8005b670_decomp.txt`
- `obj/scrolling_layers_evidence.md`

Aucun code modifie.

### Changement 2 - Parser les donnees scroll

Fichiers probables:

- `AlundraTools/AlundraEngine/DatasBin/GameMap.cs`
- `AlundraTools/AlundraEngine/DatasBin/ScrollScreen.cs`

Objectif:

- stream position explicite.
- `LiningInfos` conserve.
- `LayerInfos` conserve.
- `Scrollar` lu avec les tailles correctes.
- palettes conservees.
- bitmaps par palette ou donnees brutes conservees.

Validation:

- build.
- property grid Scroll screen affiche des valeurs coherentes.
- savestate/map de nuages montre les modes attendus.

### Changement 3 - Initialiser l'etat Scrollar

Fichier probable:

- `AlundraTools/AlundraEngine/DatasBin/ScrollScreen.cs`

Objectif:

- zero des tableaux d'etat.
- calcul initial `_scrollDirX/Y` par layer.

Validation:

- pas de rendu encore obligatoire.
- logs/debug montrent directions coherentes.

### Changement 4 - Rendu Scrollar minimal

Fichier probable:

- `AlundraTools/AlundraEngine/Graphics/GraphicManager.cs`
- possiblement `Renderer.cs` / `IRenderer.cs` pour un helper source-rect justifie.

Objectif:

- `RenderAllTileLayers` branche `ScrollParameters`.
- `RenderLayerToBuffer` rend `ModeLayer == 1`.
- foreground de nuages visible dans savestate 1.

Validation:

- build.
- screenshot C#.
- mouvement sur 60 frames.

### Changement 5 - Overlay

Objectif:

- porter `RenderTileOverlayLayer` / `SetOverlay` apres preuve.

Validation:

- scenes avec overlay simple et extended.

### Changement 6 - Cellular

Objectif:

- porter `ModeLayer == 2` apres preuve.

Validation:

- scenes connues avec effets cellular.

## Definition of done pour le premier jalon

Le premier jalon est termine quand:

1. Le savestate 1 original a une screenshot de reference.
2. Le C# charge des `ScrollParameters` coherents pour la meme scene.
3. `RenderAllTileLayers @ 0x8005B670` n'est plus bloque par `CurrentMap.ScrollScreen == null`.
4. `RenderLayerToBuffer @ 0x8005B848` rend au moins les layers `Scrollar`.
5. Les nuages foreground sont visibles.
6. Les nuages bougent dans la bonne direction.
7. Le foreground est dessine dans le bon ordre relatif.
8. Les palettes sont suffisamment proches pour comparaison visuelle.
9. Le build `AlundraGame.csproj` passe ou les echecs restants sont documentes comme hors scope.
10. Les points non fermes sont listes comme `PARTIAL` ou `BLOCKED`.

## Format de rapport final attendu par l'agent

Utiliser ce format:

```text
Closed:
- fonctions analysees:
- structures fermees:
- champs C# ajoutes:
- rendu observe:
- build:

Partial:
- semantiques encore partielles:
- choix backend temporaires:

Blocked:
- preuve manquante:
- plus petit pas suivant:

Fichiers modifies:
-
```

Ne pas conclure par une proposition vague. Donner le prochain pas concret si quelque chose reste bloque.
