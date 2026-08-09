# Exporter les cartes Alundra vers Tiled

L'export Tiled est produit par `AlundraDataExtractor` en meme temps que les JSON et PNG existants des cartes. La V1 est uniquement a sens unique : elle exporte les donnees Alundra vers Tiled, elle ne reimporte pas les modifications Tiled vers le format du jeu.

## Executer l'extracteur

Depuis la racine du depot, lancer l'extracteur depuis `AlundraTools/AlundraTools` afin que `EntityNames.csv` soit resolu par le helper existant :

```powershell
Push-Location AlundraTools\AlundraTools
dotnet ..\AlundraDataExtractor\bin\Debug\net9.0-windows\AlundraDataExtractor.dll "D:\development\repo\Alundra Remake\Alundra (France)\Alundra (France)_extracted" "D:\development\repo\Alundra Remake\remaster-data-extracted"
Pop-Location
```

Les deux arguments positionnels sont :

- le dossier des donnees extraites du disque original ;
- le dossier de sortie de l'extracteur.

Si le binaire Debug standard est verrouille par une session de validation, construire dans un dossier de sortie temporaire puis utiliser le DLL produit :

```powershell
dotnet build AlundraTools/AlundraDataExtractor/AlundraDataExtractor.csproj -p:BaseOutputPath=obj\copilot-build\manual\
Push-Location AlundraTools\AlundraTools
dotnet ..\AlundraDataExtractor\obj\copilot-build\manual\Debug\net9.0-windows\AlundraDataExtractor.dll "D:\development\repo\Alundra Remake\Alundra (France)\Alundra (France)_extracted" "D:\development\repo\Alundra Remake\remaster-data-extracted"
Pop-Location
```

## Options de layout

Deux sorties image ont un layout configurable, avec des defauts differents : le tileset Tiled est `compact`, le spritesheet reste `original`.

| Option | Sortie concernee | Defaut | Modes |
| --- | --- | --- | --- |
| `--tiled-tileset-layout original\|compact` | `data\tiled\map_N_tileset.png` | `compact` | `compact` packe uniquement les tiles utilisees, une cellule par `rawTileId` ; `original` reprend le layout historique du tilesheet |
| `--spritesheet-layout original\|compact` | `data\map_N_spritesheet.png` et `data\map_alundra_spritesheet.png` | `original` | `original` empile les pages VRAM natives ; `compact` packe uniquement les quads utilises, une cellule par couple (region VRAM, palette) |

```powershell
Push-Location AlundraTools\AlundraTools
dotnet ..\AlundraDataExtractor\bin\Debug\net9.0-windows\AlundraDataExtractor.dll "D:\development\repo\Alundra Remake\Alundra (France)\Alundra (France)_extracted" "D:\development\repo\Alundra Remake\remaster-data-extracted" --tiled-tileset-layout original --spritesheet-layout compact
Pop-Location
```

Le layout de tileset par defaut est `compact` : il packe uniquement les tiles utilisees et reste le mode de rendu Tiled fiable quand plusieurs `rawTileId` reutilisent le meme slot historique avec des palettes differentes. Le mode `original` reprend le layout historique de `map_N_tilesheet.png`, trous inutilises compris ; quand un slot historique est deja pris par une autre variante de palette du meme index, ou qu'il n'est pas modelise par ce layout, la tile est ajoutee a la suite de la zone authentique (l'image grandit vers le bas) plutot que de faire echouer l'export.

Le mode `original` du spritesheet empile les huit pages VRAM natives de 256x256, chaque quad etant dessine a la fenetre VRAM qu'il echantillonne : le PNG mesure toujours 256x2048, les trous sont conserves, et les champs `AtlasX`/`AtlasY` des `SiImage` serialises valent exactement `(SourceX, page * 256 + SourceY)`. Ces coordonnees ne sont pas exemptes de collisions : une meme region VRAM reutilisee sous plusieurs palettes (scintillement a cycle de couleurs, par exemple) tombe sur une seule cellule, donc le dernier quad dessine gagne et les autres decoupent la mauvaise couleur. Sur `map_10`, 499 signatures distinctes se ramenent ainsi a 333 cellules. Le mode `compact` donne une cellule par signature, donc chaque quad decoupe bien la couleur prevue.

## Fichiers generes

Pour chaque carte `map_N`, les fichiers Tiled sont ecrits dans `<sortie>\data\tiled` :

- `map_N.tmj` : carte Tiled JSON, avec les couches visibles `Render_*`, puis `Portals`, `MapEvents` et `Entities`.
- `map_N_tileset.tsj` : tileset Tiled JSON externe, avec les proprietes brutes `TileId`, `Palette`, `Tile` et les animations de tiles quand elles sont disponibles.
- `map_N_tileset.png` : tileset Tiled. Par defaut il repacke uniquement les tiles utilisees ; le mode optionnel `original` reprend exactement le layout historique du tilesheet de carte, avec ses trous, quand la carte ne reutilise pas le meme slot pour plusieurs variants bruts.
- `map_N.alundra.json` : compagnon brut Alundra pour les donnees qui ne rentrent pas proprement dans les couches natives Tiled.

Le compagnon brut conserve notamment les donnees par cellule (`Walkability`, `GroundProperty`, `Slope`, `Height`, `WallTilesOffset`, `TileId`, `Palette`, `Tile`, `Flags`) et les piles de murs (`Offset`, `Count`, ids de tiles bruts, positions renderer calculees). Les couches visibles `Render_*` sont un packing minimal de l'ordre de rendu du jeu. Une couche supplementaire n'est creee que lorsque plusieurs tiles ciblent la meme cellule Tiled ; chaque couche expose un `Z`/`RenderPlane` custom. Les donnees brutes `Ground` et `Walls_*` ne sont plus exportees comme couches Tiled et restent uniquement dans le fichier compagnon. Le tileset Tiled peut suivre soit le layout historique `original`, soit le layout `compact`; dans les deux cas, le mapping `TileId` brut vers gid Tiled reste explicite dans le `.tsj`.

Les animations de tiles Tiled utilisent des durees en millisecondes. La duree brute Alundra `FrameDuration` est conservee dans `AnimationFrameDurationPsxFrames`, puis convertie avec la frequence PSX de l'extraction (`50 Hz` pour les donnees PAL, `60 Hz` pour les donnees USA). Par exemple, une duree brute de `8` frames PAL devient `160` ms dans les entrees `animation[].duration`.

## Ouvrir dans Tiled

Ouvrir directement `map_N.tmj` dans Tiled. Le fichier reference le tileset externe `map_N_tileset.tsj`, qui reference lui-meme `map_N_tileset.png` dans le meme dossier.

## Valider une exportation

Le validateur structurel ne lance pas le runtime du jeu. Il lit les fichiers exportes et verifie les dimensions, les plages de GID, l'image de tileset, les couches d'objets, le compagnon brut et les animations Tiled :

```powershell
& .\scripts\validate-tiled-export.ps1 -DataPath "D:\development\repo\Alundra Remake\remaster-data-extracted\data" -MapName map_0
```

Pour couvrir aussi les animations de tiles, utiliser une carte animee telle que `map_10` dans l'extraction de reference actuelle :

```powershell
& .\scripts\validate-tiled-export.ps1 -DataPath "D:\development\repo\Alundra Remake\remaster-data-extracted\data" -MapName map_10
```