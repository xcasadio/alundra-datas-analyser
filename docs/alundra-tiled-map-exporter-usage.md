# Exporter les cartes Alundra vers Tiled

L'export Tiled est produit par `AlundraDataExtractor` en meme temps que les JSON et PNG existants des cartes. La V1 est uniquement a sens unique : elle exporte les donnees Alundra vers Tiled, elle ne reimporte pas les modifications Tiled vers le format du jeu.

## Executer l'extracteur

Depuis la racine du depot, lancer l'extracteur depuis `AlundraTools/AlundraTools` afin que `EntityNames.csv` soit resolu par le helper existant :

```powershell
Push-Location AlundraTools\AlundraTools
dotnet ..\AlundraDataExtractor\bin\Debug\net9.0-windows\AlundraDataExtractor.dll "D:\development\repo\Alundra Remake\Alundra (France)\Alundra (France)_extracted" "D:\development\repo\Alundra Remake\remaster-data-extracted"
Pop-Location
```

Les deux arguments sont :

- le dossier des donnees extraites du disque original ;
- le dossier de sortie de l'extracteur.

Si le binaire Debug standard est verrouille par une session de validation, construire dans un dossier de sortie temporaire puis utiliser le DLL produit :

```powershell
dotnet build AlundraTools/AlundraDataExtractor/AlundraDataExtractor.csproj -p:BaseOutputPath=obj\copilot-build\manual\
Push-Location AlundraTools\AlundraTools
dotnet ..\AlundraDataExtractor\obj\copilot-build\manual\Debug\net9.0-windows\AlundraDataExtractor.dll "D:\development\repo\Alundra Remake\Alundra (France)\Alundra (France)_extracted" "D:\development\repo\Alundra Remake\remaster-data-extracted"
Pop-Location
```

## Fichiers generes

Pour chaque carte `map_N`, les fichiers Tiled sont ecrits dans `<sortie>\data\tiled` :

- `map_N.tmj` : carte Tiled JSON, avec les couches visibles `Render_*`, les couches brutes masquees `Ground`/`Walls_*`, puis `Portals`, `MapEvents` et `Entities`.
- `map_N_tileset.tsj` : tileset Tiled JSON externe, avec les proprietes brutes `TileId`, `Palette`, `Tile` et les animations de tiles quand elles sont disponibles.
- `map_N_tileset.png` : tileset compact genere pour Tiled.
- `map_N.alundra.json` : compagnon brut Alundra pour les donnees qui ne rentrent pas proprement dans les couches natives Tiled.

Le compagnon brut conserve notamment les donnees par cellule (`Walkability`, `GroundProperty`, `Slope`, `Height`, `WallTilesOffset`, `TileId`, `Palette`, `Tile`, `Flags`) et les piles de murs (`Offset`, `Count`, ids de tiles bruts, positions renderer calculees). Les couches visibles `Render_*` suivent l'ordre de rendu du jeu, ligne source par ligne source : sol de la ligne, puis piles de murs de cette ligne. Les couches `Ground` et `Walls_*` restent presentes mais masquees pour conserver les donnees brutes et les positions renderer calculees.

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