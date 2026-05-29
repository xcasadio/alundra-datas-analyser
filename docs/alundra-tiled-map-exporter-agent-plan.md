# Plan d’exporteur de cartes Tiled pour AlundraDataExtractor

Objectif : ajouter un convertisseur/exporteur dans `AlundraTools/AlundraDataExtractor` qui produit des fichiers de carte compatibles avec Tiled à partir des données de carte Alundra déjà extraites. La V1 cible uniquement la visualisation et une édition confortable. Elle ne doit pas essayer de réimporter vers le format binaire original d’Alundra.

Fichiers sources principaux déjà présents :

- `AlundraTools/AlundraDataExtractor/Program.cs` : charge chaque `GameMap`, écrit `map_{id}.json`, `map_{id}_tilesheet.png` et `map_{id}_spritesheet.png`.
- `AlundraTools/AlundraDataExtractor/GameMapHelper.cs` : dessine les tile sheets de carte à partir des tiles de sol et des tiles de mur.
- `AlundraTools/AlundraEngine/DatasBin/Map.cs` : expose `Width`, `Height`, `MapTiles` et le chargement des données de tiles de mur.
- `AlundraTools/AlundraEngine/DatasBin/MapTile.cs` : expose `Walkability`, `GroundProperty`, `Slope`, `Height`, `TileId`, `Palette`, `Tile`, `WallTilesOffset` et les `WallTiles` optionnels.
- `AlundraTools/AlundraEngine/DatasBin/GameMapInfo.cs` : expose les données de niveau carte `MapId`, `Gravity`, `ZViscosity`, `SlideEffectId`, `BalanceLevel`, les octets bruts `C` à `_11`, `SpriteMapEntries` et `Portals`.
- `AlundraTools/AlundraEngine/DatasBin/Portal.cs` : expose le rectangle du portail, la destination, le niveau z et les flags.
- `AlundraTools/AlundraEngine/DatasBin/SiMapEventRecord.cs` : expose le rectangle d’événement de carte et les octets d’événement bruts.
- `AlundraTools/AlundraEngine/DatasBin/SiEntityRecord.cs` : expose les enregistrements d’entités de carte et leurs indices de scripts bruts.
- `AlundraTools/AlundraEngine/Graphics/GraphicManager.cs` : placement runtime actuel des tiles de sol et de mur.
- `AlundraTools/AlundraTools/GameControls/frmAlundra.cs` : visualisation éditeur/debug existante pour les tiles, portails, entités et tables d’événements de carte.

## Icônes de statut

- ⬜ Non commencé
- 🔄 En cours
- ✅ Terminé
- ⛔ Bloqué

Chaque tâche d’agent IA doit suivre ce protocole :

- Changer exactement une icône de tâche de `⬜` à `🔄` avant de modifier le code.
- Travailler uniquement dans le périmètre de la tâche.
- Exécuter la validation de tâche listée dans la tâche.
- Changer la même icône de tâche de `🔄` à `✅` seulement après la réussite de la validation.
- Commit immédiatement après la tâche.
- Laisser les icônes des tâches suivantes inchangées.
- Si la tâche ne peut pas avancer sans nouvelles informations, changer l’icône en `⛔`, documenter le blocage sous cette tâche, et commit le statut bloqué.

Les messages de commit doivent rester petits et monotoniques. Ne pas combiner plusieurs tâches dans un seul commit.

## Critique de l’ordre proposé

L’ordre proposé est globalement cohérent pour une V1, car il commence par le plus petit résultat visible : une couche de sol avec un tileset, puis ajoute la décoration verticale, puis les métadonnées et les couches d’objets.

Le code actuel permet cette progression incrémentale. `SaveMap` dispose déjà d’un `GameMap` chargé et écrit déjà les assets JSON et PNG sources. `GameMapHelper.SaveTileSheet` parcourt déjà `gameMap.Map.MapTiles`, inclut `TileId` et inclut chaque entrée `WallTiles.Tiles` lorsqu’elle est présente.

Correction importante : un « tileset compact » n’est pas seulement une optimisation. C’est un contrat de données. Les valeurs de sol et de mur d’Alundra sont des valeurs `ushort TileId` dont les bits bas sélectionnent une tile et les bits hauts de palette sont utilisés par `GameMap.GetTileBitmap`. Les cellules des couches Tiled ont besoin de gids stables. L’exporteur doit donc construire et persister un mapping déterministe de l’id de tile brut Alundra vers le gid Tiled. Un PNG visuellement compact sans ce mapping ne suffit pas.

La séparation entre les couches de tiles visuelles et les données brutes par cellule est correcte. Dans le modèle actuel, `Walkability`, `GroundProperty`, `Slope`, `Height` et `WallTilesOffset` vivent sur chaque `MapTile`, pas sur le bitmap de tile lui-même. Une couche de sol Tiled peut afficher le sol, mais elle ne peut pas remplacer en toute sécurité l’enregistrement brut de cellule Alundra en V1. Conserver un JSON compagnon ou une couche d’objets dédiée aux données brutes pour ces champs.

Les murs doivent venir après le sol, comme proposé, mais leur placement doit suivre les faits vérifiés dans le renderer existant. Le runtime dessine une tile de sol à `x * MapTileWidth`, `(y - tile.Height) * MapTileHeight` ; les tiles de mur sont décalées avec `WallTiles.Offset` puis empilées par `MapTileHeight`. Le plan ne doit pas traiter les couches de mur comme de simples overlays sur la même cellule, sauf si les coordonnées Tiled exportées reproduisent cette règle ou stockent explicitement le placement brut des murs séparément.

Les propriétés de carte sont peu coûteuses et utiles, donc les ajouter avant les couches d’objets est raisonnable. Les champs de niveau carte vérifiés sont `MapId`, `Gravity`, `ZViscosity`, `SlideEffectId`, `BalanceLevel`, `C`, `D`, `E`, `F`, `_10`, `_11`, ainsi que les tailles/offsets d’en-tête si l’agent choisit de les préserver.

Les portails constituent une bonne première couche d’objets, car `Portal` possède des champs de type rectangle : `X1`, `Y1`, `X2`, `Y2`, `DestMapId`, `DestTileX`, `DestTileY`, `ZLevel` et `Flags`. Le `frmAlundra` existant ignore les enregistrements de portail où `X2 == 0xff && Y2 == 0xff` ; l’exporteur doit vérifier et documenter si ce même filtre est utilisé.

Les événements de carte sont aussi adaptés à une couche d’objets, car `SiMapEventRecord` possède `X1`, `Y1`, `X2`, `Y2`, `EventCodesBIndex` et les champs bruts `Ub1..Ub3`. Conserver les octets bruts comme propriétés. Ne pas inventer de noms sémantiques pour `Ub1..Ub3`.

Les entités peuvent aussi être adaptées à une couche d’objets, mais elles sont plus complexes que les portails et les événements de carte. `SiEntityRecord` possède des limites d’activation, des octets de position, une direction/table de sprite, des indices de scripts, `_10` et `Contents`. Le `frmAlundra` existant affiche `XPos / 2`, `YPos / 2` et dessine en utilisant `Height / 2` pour le placement. L’exporteur doit préserver les octets bruts comme propriétés et documenter toute mise à l’échelle d’affichage appliquée.

Les animations Tiled doivent attendre que le catalogue raw-tile-id-to-gid soit stable. Le helper de tile sheet actuel peut dessiner toutes les frames animées en utilisant `TileAnimDescriptor` et `GameMapInfo.SpriteMapEntries`, mais les définitions d’animations Tiled doivent pointer vers des gids. Ajouter l’animation avant que le mapping de gids soit stable rendrait le résultat fragile.

L’import round-trip doit rester hors de la V1. L’extracteur actuel écrit le JSON complet de `GameMap`, et la sortie Tiled destinée à l’éditeur sera nécessairement une vue d’édition avec perte, sauf si chaque champ brut Alundra et chaque règle de mutation est préservé. Le plan V1 doit conserver le JSON brut comme donnée compagnon faisant autorité.

## Contraintes fortes

- Ne pas supprimer ni modifier les sorties existantes `map_{id}.json`, `map_{id}_tilesheet.png` ou `map_{id}_spritesheet.png`.
- Ne pas inventer de noms sémantiques pour les champs bruts ou inconnus. Préserver les noms tels que `C`, `D`, `E`, `F`, `_10`, `_11`, `Ub1`, `Ub2`, `Ub3` et `_10` sur les entités, sauf si un fichier source prouve déjà un meilleur nom.
- Préserver les valeurs brutes Alundra comme propriétés exportées, même lorsqu’une coordonnée d’affichage plus conviviale est aussi exportée.
- Garder la V1 à sens unique : données Alundra vers données Tiled uniquement.
- Garder les données brutes compagnons disponibles à côté de l’export Tiled, soit en liant le fichier `map_{id}.json` existant, soit en écrivant un JSON compagnon ciblé.
- Utiliser le projet C# existant et le style `System.Text.Json`, sauf si une tâche prouve explicitement le besoin d’une autre dépendance.
- Préserver le worktree sale. Au moment où ce plan a été écrit, Git affichait déjà des suppressions Ghidra sans rapport, un sous-module `MGUI` modifié, des fichiers JetBrainsMono non suivis, `Alundraportage/` non suivi, ainsi que des fichiers de verrouillage/base de données Ghidra.

## Sortie cible pour la V1

T01 a vérifié le contrat officiel du format JSON de Tiled. La V1 utilise donc JSON/TMJ pour la carte et JSON/TSJ pour le tileset externe, car l’extracteur écrit déjà des assets JSON et PNG et le format Tiled JSON stocke nativement les couches, les objets, les propriétés personnalisées et les animations de tiles.

Dossier de sortie proposé sous le dossier d’extraction `data` existant :

- `tiled/map_{id}.tmj` : carte Tiled JSON.
- `tiled/map_{id}_tileset.tsj` : tileset Tiled JSON externe.
- `tiled/map_{id}_tileset.png` : tileset visuel compact généré à partir des ids de tiles bruts Alundra.
- `tiled/map_{id}.alundra.json` : JSON compagnon ciblé pour les données brutes par cellule et les données murales que les couches de tiles Tiled ne peuvent pas représenter fidèlement seules.

## Tableau des tâches IA

### ✅ T01 - Vérifier le contrat de format Tiled

Périmètre : choisir le format de sortie Tiled concret et prouver le schéma minimal nécessaire à Tiled.

Étapes :

- Vérifier si Tiled JSON/TMJ est le format choisi pour cette tâche de repo.
- Créer une très petite carte et un tileset d’exemple écrits à la main hors du code de production uniquement si nécessaire pour la validation.
- Documenter les champs requis pour la taille de carte, la taille des tiles, les tilesets, les couches de tiles, les couches d’objets, les propriétés personnalisées et les animations de tiles.
- Mettre à jour `Target Output For V1` si l’extension choisie ou la séparation des fichiers change.

Validation :

- L’exemple s’ouvre dans Tiled, ou la tâche documente la commande/l’outil exact de validation Tiled utilisé.

Résultat T01 :

- Format choisi : Tiled JSON map `.tmj` avec tileset externe `.tsj`.
- Champs de carte minimaux vérifiés : `type = "map"`, `version`, `tiledversion`, `orientation`, `renderorder`, `width`, `height`, `tilewidth`, `tileheight`, `infinite`, `layers`, `tilesets`, `nextlayerid`, `nextobjectid`.
- Couches de tiles : objets `type = "tilelayer"` avec `id`, `name`, `width`, `height`, `x`, `y`, `opacity`, `visible` et tableau `data` de GIDs.
- Couches d’objets : objets `type = "objectgroup"` avec `draworder = "topdown"`, `objects`, `opacity`, `visible`, `x`, `y`, `width`, `height`; chaque objet utilise des coordonnées en pixels.
- Propriétés personnalisées : tableaux de `{ "name", "type", "value" }` sur carte, tileset, tiles, couches et objets.
- Tileset externe : `.tsj` avec `type = "tileset"`, `version`, `tiledversion`, `name`, `tilewidth`, `tileheight`, `spacing`, `margin`, `columns`, `tilecount`, `image`, `imagewidth`, `imageheight` et `tiles` optionnel.
- Animations : chaque tile animée est une entrée de `tiles` avec `id` et `animation`, où chaque frame contient `tileid` local et `duration` en millisecondes.
- Validation Tiled documentée : sur une machine avec Tiled installé, exécuter `tiled --export-map json data/tiled/map_0.tmj obj/tiled-validation/map_0.json`. Dans l’environnement actuel, `Get-Command tiled, tiled.exe, tmxrasterizer, tmxvalidator` ne trouve aucun outil Tiled local.

Commit: `docs(tiled): verify tiled export format`

### ✅ T02 - Ajouter le point d’entrée de l’exporteur

Périmètre : ajouter le plus petit chemin de code pouvant être appelé depuis `SaveMap` sans modifier les sorties existantes.

Étapes :

- Ajouter une nouvelle classe d’exporteur sous `AlundraTools/AlundraDataExtractor`, par exemple `TiledMapExporter`.
- L’appeler depuis `SaveMap` après les exports JSON et PNG existants.
- Créer le dossier de sortie `data/tiled`.
- Garder `SaveAlundraMap` inchangé, sauf si T01 exige explicitement d’exporter aussi la carte spéciale Alundra.

Validation :

- `dotnet build AlundraTools/AlundraDataExtractor/AlundraDataExtractor.csproj` réussit.

Commit: `tools(tiled): add map exporter entry point`

### ✅ T03 - Construire un catalogue de tiles déterministe

Périmètre : collecter les ids de tiles bruts Alundra qui doivent apparaître dans le tileset Tiled.

Étapes :

- Parcourir chaque `MapTile.TileId` sauf `0xffff`.
- Parcourir chaque entrée non nulle `MapTile.WallTiles.Tiles` sauf `0xffff`.
- Construire un mapping déterministe `rawTileId -> tiledLocalTileId` et `rawTileId -> gid`.
- Préserver à la fois le `TileId` brut et les valeurs parsées `Palette`/`Tile` dans les propriétés de tile du tileset ou dans le JSON compagnon.
- Ne pas fusionner deux ids de tiles bruts qui se rendent visuellement de façon similaire mais possèdent des bits de palette différents.

Validation :

- Ajouter ou exécuter une vérification ciblée prouvant que la même carte produit le même ordre de mapping sur deux exports.
- Le build réussit.

Commit: `tools(tiled): build raw tile catalog`

### ✅ T04 - Générer le PNG de tileset Tiled compact

Périmètre : créer le PNG visuel utilisé par Tiled à partir du catalogue de tiles.

Étapes :

- Générer un PNG compact selon l’ordre du catalogue en utilisant `GameMap.GetTileBitmap(rawTileId)`.
- Utiliser les dimensions de tile vérifiées depuis `StaticVariables.MapTileWidth` et `StaticVariables.MapTileHeight`.
- Garder la gestion transparent/vide pour `0xffff` en dehors du catalogue.
- Écrire un fichier de tileset Tiled ou des métadonnées de tileset embarquées qui pointent vers le PNG compact.

Validation :

- Exporter une carte et vérifier que les dimensions du PNG compact correspondent au nombre d’entrées du catalogue et aux dimensions de tile.
- Le build réussit.

Commit: `tools(tiled): generate compact tileset image`

### ✅ T05 - Exporter la couche de tiles de sol

Périmètre : créer la première carte Tiled qui affiche le sol.

Étapes :

- Écrire une carte Tiled avec `width = gameMap.Map.Width`, `height = gameMap.Map.Height`, `tilewidth = StaticVariables.MapTileWidth` et `tileheight = StaticVariables.MapTileHeight`, sauf si T01 prouve qu’une autre représentation Tiled est nécessaire.
- Ajouter une couche de tiles `Ground` dont les gids de cellules proviennent du mapping du catalogue pour `MapTile.TileId`.
- Représenter `0xffff` comme cellule vide `0`.
- Ajouter des propriétés personnalisées de carte pour le nom de fichier source, l’index de carte et `GameMap.Info.MapId`.

Validation :

- Exporter `map_0` et l’ouvrir dans Tiled, ou valider le fichier avec l’outil de validation Tiled choisi.
- Le build réussit.

Commit: `tools(tiled): export ground layer`

### ✅ T06 - Préserver les données brutes par cellule de carte

Périmètre : garder les métadonnées de cellules Alundra disponibles sans prétendre qu’il s’agit de données natives de couche de tiles Tiled.

Étapes :

- Exporter `Walkability`, `GroundProperty`, `Slope`, `Height`, `WallTilesOffset`, le `TileId` brut, `Palette` parsé, `Tile` parsé et `Flags` par cellule.
- Utiliser soit `map_{id}.alundra.json`, soit une couche d’objets Tiled clairement nommée ; choisir la représentation la plus petite et la plus utilisable après vérification du comportement de Tiled.
- Lier le fichier compagnon depuis les propriétés personnalisées de la carte si un JSON compagnon est utilisé.
- Garder l’ordre du tableau traçable avec `y * Width + x`.

Validation :

- Une vérification ciblée prouve que le nombre de cellules exportées est égal à `Width * Height` et que des cellules sélectionnées correspondent aux valeurs source de `GameMap`.
- Le build réussit.

Commit: `tools(tiled): preserve raw cell metadata`

### ✅ T07 - Exporter les couches de tiles de mur

Périmètre : visualiser `WallTiles` sans perdre la structure brute des murs.

Étapes :

- Déterminer le maximum `WallTiles.Count` présent dans la carte.
- Ajouter des couches de tiles `Walls_0` à `Walls_N` pour les indices de pile de tiles de mur.
- Utiliser la règle de placement du renderer existant depuis `GraphicManager` : le sol utilise `(y - tile.Height) * tileheight`, les murs utilisent `WallTiles.Offset` et s’empilent par tileheight.
- Si les couches de tiles Tiled ne peuvent pas représenter fidèlement le décalage vertical, stocker les données brutes exactes des murs dans le JSON compagnon et documenter le compromis visuel dans les propriétés de carte.
- Préserver `WallTiles.Offset`, `WallTiles.Count` et chaque id de tile de mur brut.

Validation :

- Exporter une carte avec au moins une tile de mur et vérifier que les gids de couche de mur correspondent aux entrées source `WallTiles.Tiles`.
- Le build réussit.

Commit: `tools(tiled): export wall layers`

### ✅ T08 - Exporter les propriétés de carte

Périmètre : attacher les métadonnées vérifiées de niveau carte à la carte Tiled.

Étapes :

- Exporter `MapId`, `Gravity`, `ZViscosity`, `SlideEffectId`, `BalanceLevel`, `C`, `D`, `E`, `F`, `_10` et `_11` comme propriétés personnalisées de carte.
- Envisager d’exporter les offsets/tailles d’en-tête uniquement si c’est utile pour la traçabilité ; conserver leurs noms originaux depuis `GameMapHeader`.
- Ne pas renommer les champs inconnus.

Validation :

- Une vérification ciblée confirme que toutes les propriétés de carte listées existent pour une carte exportée.
- Le build réussit.

Commit: `tools(tiled): export map properties`

### ✅ T09 - Exporter la couche d’objets des portails

Périmètre : ajouter les rectangles de portails comme objets Tiled.

Étapes :

- Ajouter une couche d’objets nommée `Portals`.
- Exporter un objet par enregistrement `Portal` valide.
- Préserver `X1`, `Y1`, `X2`, `Y2`, `DestMapId`, `DestTileX`, `DestTileY`, `ZLevel` et `Flags` comme propriétés.
- Suivre la règle de placement des portails existante dans `frmAlundra` ou documenter toute différence.
- Vérifier si le filtre existant `X2 != 0xff && Y2 != 0xff` est le filtre de validité correct avant de l’appliquer.

Validation :

- Une vérification ciblée compare le nombre de portails exportés et leurs propriétés aux enregistrements source.
- Le build réussit.

Commit: `tools(tiled): export portal objects`

### ⬜ T10 - Exporter la couche d’objets des événements de carte

Périmètre : ajouter les rectangles d’événements de carte comme objets Tiled.

Étapes :

- Ajouter une couche d’objets nommée `MapEvents`.
- Exporter un objet par `SiMapEventRecord` non nul.
- Préserver `X1`, `Y1`, `X2`, `Y2`, `EventCodesBIndex`, `Ub1`, `Ub2` et `Ub3` comme propriétés.
- Ajouter optionnellement une propriété d’affichage seulement pour `EventCodesBIndex & 0x7f`, mais garder aussi l’octet brut.

Validation :

- Une vérification ciblée compare le nombre d’événements de carte exportés et leurs propriétés aux enregistrements source.
- Le build réussit.

Commit: `tools(tiled): export map event objects`

### ⬜ T11 - Exporter la couche d’objets des entités

Périmètre : ajouter les enregistrements d’entités de carte comme objets Tiled sans perdre les octets bruts.

Étapes :

- Ajouter une couche d’objets nommée `Entities`.
- Exporter un objet par `SiEntityRecord` non nul.
- Préserver les champs bruts : `XMin`, `YMin`, `XMax`, `YMax`, `IsEnabled`, `SpriteDirection`, `SpriteTableIndex`, `XPos`, `YPos`, `Height`, les six indices de codes d’événements, `_10` et `Contents`.
- Ajouter des coordonnées d’affichage uniquement si elles correspondent à la règle existante de `frmAlundra` : `XPos / 2`, `YPos / 2` et hauteur d’affichage depuis `Height / 2`.
- Ajouter un nom d’entité uniquement via le helper `EntityNames` existant ; ne pas inventer de noms.

Validation :

- Une vérification ciblée compare le nombre d’entités exportées et les propriétés brutes aux enregistrements source.
- Le build réussit.

Commit: `tools(tiled): export entity objects`

### ⬜ T12 - Exporter les animations de tiles

Périmètre : ajouter les métadonnées d’animation de tiles Tiled après stabilisation des tiles statiques et des gids.

Étapes :

- Utiliser `GameInitializer.CreateTileAnimDescriptors(0)` et `GameMap.Info.SpriteMapEntries`, en correspondant au chemin actuel de dessin des tiles animées de l’extracteur.
- Convertir uniquement les animations dont les ids de tiles de frames bruts existent dans le catalogue de tiles.
- Préserver la durée de frame depuis les champs source vérifiés.
- Si une frame animée requise est absente du catalogue compact, étendre le catalogue de manière déterministe au lieu d’écrire une animation cassée.

Validation :

- Exporter une carte avec au moins une `SpriteMapEntry` activée et vérifier que l’animation Tiled référence des ids de tiles valides.
- Le build réussit.

Commit: `tools(tiled): export tile animations`

### ⬜ T13 - Ajouter une commande ou un test de validation d’export

Périmètre : rendre les futurs travaux d’agent faciles à vérifier.

Étapes :

- Ajouter un test, script ou commande ciblé documenté dans ce fichier qui valide structurellement une carte exportée.
- Vérifier les dimensions des couches, les plages de gids, le chemin d’image du tileset, la présence des propriétés de couches d’objets et le nombre de cellules brutes compagnons.
- Garder la validation indépendante du runtime complet du jeu autant que possible.

Validation :

- La nouvelle commande de validation échoue sur un fichier exporté volontairement invalide ou contient des assertions qui détecteraient des couches/propriétés manquantes.
- La commande de build/test réussit.

Commit: `test(tiled): validate exported tiled map`

### ⬜ T14 - Documenter l’utilisation

Périmètre : documenter comment exécuter l’extracteur et ouvrir la sortie Tiled.

Étapes :

- Mettre à jour le README pertinent ou ajouter une courte doc près de ce plan.
- Inclure les arguments de ligne de commande pour `AlundraDataExtractor` tels qu’ils existent après implémentation.
- Lister les fichiers Tiled générés et clarifier que la V1 est uniquement à sens unique.
- Mentionner le JSON compagnon brut et les données qui restent hors des couches de tiles natives Tiled.

Validation :

- Suivre la commande documentée une fois sur un chemin d’extraction local ou documenter pourquoi elle ne peut pas être exécutée dans l’environnement actuel.

Commit: `docs(tiled): document tiled exporter usage`

## Travail différé

- Import round-trip depuis Tiled vers le format CasaEngine/Alundra.
- Améliorations UX d’édition pour les données de collision et de hauteur par cellule.
- Renommage sémantique des champs inconnus après preuve par le code ou les notes de reverse-engineering.
- Aperçus de sprites/objets pour les entités au-delà de simples rectangles d’objets.
- Réutilisation de tileset entre cartes, si cela s’avère utile plus tard.
