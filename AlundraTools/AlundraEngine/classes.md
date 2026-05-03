# AlundraEngine — Classification des classes

> **Asset Loading** : classe utilisée pour lire/parser les données binaires depuis les fichiers (datas.bin, balance.bin, etc.bin, sound.bin, TIM, …)
> **Runtime** : classe utilisée pendant l'exécution du jeu (logique de jeu, rendu, input, UI, …)
> **Both** : classe qui fait les deux (chargement + utilisation runtime)

---

## Racine

| Fichier | Classe | Statut | Description |
|---------|--------|--------|-------------|
| Breakpoint.cs | `Breakpoint` | **Runtime** | Utilitaire de debug pour déclencher des breakpoints |
| CdManager.cs | `CdManager` | **Runtime** | Gestion du streaming CD-ROM pendant le gameplay |
| EntityNames.cs | `EntityNames` | **Asset Loading** | Charge les noms d'entités/sprites depuis un CSV |
| GameEngine.cs | `GameEngine` | **Both** | Classe moteur central : initialise les assets chargés et exécute la boucle de jeu |
| GameInitializer.cs | `GameInitializer` | **Runtime** | Initialise le système d'affichage, palettes, tables CLUT, contrôleurs |
| LogManager.cs | `LogManager` | **Runtime** | Système de logging catégorisé pour le debug runtime |
| MemoryCardDataBlob.cs | `MemoryCardDataBlob` | **Both** | Structure de données pour les sauvegardes carte mémoire PSX (sérialisation/désérialisation) |
| PadManager.cs | `PadManager` | **Runtime** | Lecture et traitement des entrées manette à chaque frame |
| PhysicsEngine.cs | `PhysicsEngine` | **Runtime** | Physique des entités : forces, collisions, mouvement, attributs de tuiles |
| Random.cs | `Random` | **Runtime** | Générateur de nombres pseudo-aléatoires déterministe |
| SaveData.cs | `SaveData` | **Both** | Données de sauvegarde avec sérialisation/désérialisation JSON |
| ScrollingParameters.cs | `ScrollingParameters` | **Runtime** | Conteneur de données pour la configuration du scrolling de map |
| SoundManager.cs | `SoundManager` | **Runtime** | Gestion du système sonore : initialisation, lecture, état SPU |
| StaticVariables.cs | `StaticVariables` | **Runtime** | Conteneur massif pour toutes les variables d'état du jeu, constantes et tables |

## Balance/

| Fichier | Classe | Statut | Description |
|---------|--------|--------|-------------|
| BalanceAnimValRef.cs | `BalanceAnimValRef` | **Asset Loading** | Lit une référence de valeur d'animation balance depuis BinaryReader |
| BalanceBin.cs | `BalanceBin` | **Both** | Charge balance.bin et fournit la recherche runtime des records par index sprite/item |
| BalanceRecord.cs | `BalanceRecord` | **Asset Loading** | Lit un record balance en liste chaînée (HP, valeurs, anim vals) depuis BinaryReader |
| BalanceRecordData.cs | `BalanceRecordData` | **Runtime** | Copie mutable d'un BalanceRecord utilisée au runtime pour les stats d'entités |
| ItemBalanceRecord.cs | `ItemBalanceRecord` | **Runtime** | Conteneur simple associant un ID d'item à son balance record |

## DatasBin/

| Fichier | Classe | Statut | Description |
|---------|--------|--------|-------------|
| AnimationSet.cs | `AnimationSet` | **Asset Loading** | Lit les offsets, vitesse et flags d'un set d'animation sprite |
| DataBinHeader.cs | `DataBinHeader` | **Asset Loading** | Lit le header du fichier datas.bin (offsets vers les sprite records, maps, etc.) |
| DatasBin.cs | `DatasBin` | **Asset Loading** | Chargeur principal de datas.bin : parse le header, toutes les game maps, écran de chargement |
| FrameCollisionData.cs | `FrameCollisionData` | **Asset Loading** | Lit les données de boîte de collision par frame |
| GameMap.cs | `GameMap` | **Asset Loading** | Charge une map complète : info, tuiles, sprites, palettes, strings, données de scroll |
| GameMapHeader.cs | `GameMapHeader` | **Asset Loading** | Lit les offsets/tailles des sections de game map |
| GameMapInfo.cs | `GameMapInfo` | **Asset Loading** | Lit les métadonnées de map : ID, gravité, palettes, portails, entrées sprite map |
| Map.cs | `Map` | **Asset Loading** | Lit la grille de map : dimensions, copies, tuiles et murs |
| MapCopy.cs | `MapCopy` | **Asset Loading** | Lit une définition de région de copie de map |
| MapEffectRecord.cs | `MapEffectRecord` | **Asset Loading** | Lit un record d'effet de map (position, flags, ID d'effet) |
| MapTile.cs | `MapTile` | **Both** | Lit les données de tuile depuis le binaire ; utilisé aussi au runtime pour les vérifications de marchabilité/hauteur |
| Portal.cs | `Portal` | **Asset Loading** | Lit une définition de portail/warp |
| ScrollScreen.cs | `ScrollParameters` | **Asset Loading** | Lit la config des couches de scroll, palettes et tilesheets |
| SiAnimation.cs | `SiAnimation` | **Asset Loading** | Lit la séquence de frames d'animation sprite |
| SiAnimDir.cs | `SiAnimDir` | **Runtime** | Enum définissant les constantes de direction d'animation (Down/Up/Left/Right) |
| SiCommand.cs | `SiCommand` | **Both** | Structure de données de commande script parsée, utilisée au chargement et par le debugger |
| SiEffectAnimation.cs | `SiEffectAnimation` | **Asset Loading** | Lit les frames d'animation d'effet sprite |
| SiEffectFrame.cs | `SiEffectFrame` | **Asset Loading** | Lit une frame d'animation d'effet (délai, image set) |
| SiEntityRecord.cs | `SiEntityRecord` | **Asset Loading** | Lit un record de définition d'entité (position, event codes, index sprite) |
| SiFrame.cs | `SiFrame` | **Asset Loading** | Lit une frame d'animation sprite (délai, collision, image set) |
| SiImage.cs | `SiImage` | **Asset Loading** | Lit une définition d'image sprite (sheet, palette, coordonnées UV) |
| SiImageSet.cs | `SiImageSet` | **Asset Loading** | Lit un set d'images sprite (tri de profondeur, tableau d'images) |
| SiMapEventRecord.cs | `SiMapEventRecord` | **Asset Loading** | Lit une région de déclenchement d'événement de map |
| SpriteEffectRecord.cs | `SpriteEffectRecord` | **Asset Loading** | Lit un record d'effet sprite et précharge toutes ses animations |
| SpriteInfo.cs | `SpriteInfo` | **Asset Loading** | Chargeur maître des données sprite d'une map : tables, effets, palettes, event codes, entités |
| SpriteInfoEntities.cs | `SpriteInfoEntities` | **Asset Loading** | Lit le tableau de records d'entités |
| SpriteInfoEventCodes.cs | `SpriteInfoEventCodes` | **Asset Loading** | Lit les 6 tables d'event codes (A-F) et le tableau de bytecode |
| SpriteInfoHeader.cs | `SpriteInfoHeader` | **Asset Loading** | Lit les pointeurs et tailles de la section sprite info |
| SpriteInfoMapEvents.cs | `SpriteInfoMapEvents` | **Asset Loading** | Lit le tableau de records d'événements de map |
| SpriteMapEntry.cs | `SpriteMapEntry` | **Runtime** | Conteneur de données pour l'état de tuile animée utilisé lors du rendu |
| SpriteRecord.cs | `SpriteRecord` | **Asset Loading** | Lit le record de table sprite, sets d'animation, et précharge toutes les animations directionnelles |
| SpriteTableHeader.cs | `SpriteTableHeader` | **Asset Loading** | Lit le header d'entrée de table sprite (pointeurs, flags, dimensions) |
| TileAnimDescriptor.cs | `TileAnimDescriptor` | **Runtime** | Conteneur simple pour l'état d'animation de tuile |
| WallTiles.cs | `WallTiles` | **Asset Loading** | Lit les données de couche de tuiles de mur (offset, count, IDs de tuiles) |

## Etc/

| Fichier | Classe | Statut | Description |
|---------|--------|--------|-------------|
| EtcRes.cs | `EtcRes` | **Asset Loading** | Classe de base abstraite pour le chargement des ressources etc.bin (strings, items) |
| EtcResR.cs | `EtcResR` | **Asset Loading** | Charge etc.bin européen (PAL) : tables d'index, strings, noms/descriptions d'items |
| EtcResUsa.cs | `EtcResUsa` | **Asset Loading** | Charge etc.bin USA : idem EtcResR avec gestion des offsets nuls |

## Gameplay/

| Fichier | Classe | Statut | Description |
|---------|--------|--------|-------------|
| Entity.cs | `Entity` | **Runtime** | Objet entité principal : position, animation, HP, état physique, flags |
| EntityGameplayManager.cs | `EntityGameplayManager` | **Runtime** | Logique gameplay : rotation d'entité, calcul de direction, comportement de vol |
| EntityManager.cs | `EntityManager` | **Runtime** | Cycle de vie des entités : allocation, initialisation, mises à jour d'animation, calcul HP |
| EntityStatus.cs | `EntityStatus` | **Runtime** | Enum d'états du cycle de vie d'entité (Destroyed, Loaded, Normal, etc.) |
| ItemDropProperties.cs | `ItemDropProperties` | **Runtime** | Conteneur de données pour les propriétés de drop d'items |
| MagicEarthParameters.cs | `MagicEarthParameters` | **Runtime** | Conteneur de données pour les paramètres du sort de magie terre |
| PadState.cs | `PadState` | **Runtime** | État des boutons manette avec suivi press/hold/release et logique de répétition |
| PlayerAnimation.cs | `PlayerAnimation` | **Runtime** | Enum définissant tous les IDs d'animation du joueur (Idle, Attack, Jump, etc.) |
| PlayerManager.cs | `PlayerManager` | **Runtime** | Mouvement du joueur, combat, gestion des warps, utilisation d'items, machine d'états d'animation |
| PlayerStats.cs | `PlayerStats` | **Runtime** | Conteneur de stats joueur (HP, MP, argent, arme/item équipé) |
| SpriteEffect.cs | `SpriteEffect` | **Runtime** | Instance d'effet sprite runtime : position, animation, statut, entité attachée |
| SpriteRef.cs | `SpriteRef` | **Runtime** | Référence runtime à des images sprite avec position et profondeur pour le rendu |
| Voice.cs | `Voice` | **Runtime** | Conteneur de données d'état de canal voix SPU |

## Gameplay/Scripts/

| Fichier | Classe | Statut | Description |
|---------|--------|--------|-------------|
| CutsceneChannel.cs | `CutsceneChannel` | **Runtime** | Conteneur de paramètres de canal d'animation de cinématique |
| EntityEventHandlers.cs | `EntityEventHandlers` | **Runtime** | Interpréteur de bytecode script : dispatche les commandes 0x00-0xFF vers les fonctions handler |
| EventCodeDebugger.cs | `EventCodeDebugger` | **Both** | Parse le bytecode event code en listes de SiCommand ; utilisé au chargement et debug runtime |
| EventProgramState.cs | `EventProgramState` | **Runtime** | État runtime d'un programme script en exécution (compteur programme, paramètres, résultat) |
| FunctionTypeA.cs | `FunctionTypeA` | **Runtime** | Handlers d'événements de chargement d'entité : set animations, spawn warps |
| FunctionTypeC.cs | `FunctionTypeC` | **Runtime** | Handlers d'IA tick d'entité : idle, patrouille, poursuite, vol, comportements de boss |
| FunctionTypeD.cs | `FunctionTypeD` | **Runtime** | Handlers d'événements de contact d'entité : réactions de dégâts, knockback |
| FunctionTypeE.cs | `FunctionTypeE` | **Runtime** | Handlers d'événements de désactivation : destruction, animations de mort |
| MapEvent.cs | `MapEvent` | **Runtime** | Instance d'événement de map runtime : ID, référence de record, entité attachée et état programme |
| ScriptHelper.cs | `ScriptHelper` | **Runtime** | Fonctions utilitaires de script : calcul de direction, extension de signe, position relative |
| SpriteEventHandlers.cs | `SpriteEventHandlers` | **Runtime** | Registre mappant les index de programme sprite aux fonctions handler d'IA |
| WarpSlotState.cs | `WarpSlotState` | **Runtime** | Conteneur de données pour l'état d'animation de transition warp |

## Gameplay/Scripts/Boss/

| Fichier | Classe | Statut | Description |
|---------|--------|--------|-------------|
| AI_Melzas2.cs | `AI_Melzas2` | **Runtime** | IA du boss final (Melzas phase 2) : spawn, transitions de phase, patterns d'attaque |
| AncientGuardian.cs | `AncientGuardian` | **Runtime** | IA du boss Ancient Guardian : gravité, machine d'états, patterns d'attaque |

## Graphics/

| Fichier | Classe | Statut | Description |
|---------|--------|--------|-------------|
| BlendMode.cs | `BlendMode` | **Runtime** | Enum des modes de blending GPU PSX (Average, Additive, Subtractive, AdditiveDim) |
| DR_TPAGE.cs | `DR_TPAGE` | **Runtime** | Structure de primitive de dessin de page de texture GPU PSX |
| EffectManager.cs | `EffectManager` | **Runtime** | Gestion des effets sprite runtime : spawn, initialisation, allocation de slots |
| GraphicManager.cs | `GraphicManager` | **Runtime** | Gestionnaire de rendu principal : tuiles, entités, couches, overlays debug, composition UI |
| ImageHelper.cs | `ImageHelper` | **Both** | Utilitaire d'image PSX : conversion de couleurs, décompression, génération bitmap |
| InventoryCursorAnimation.cs | `InventoryCursorAnimation` | **Runtime** | Conteneur de données pour l'état d'animation du curseur inventaire |
| IRenderer.cs | `IRenderer` | **Runtime** | Interface de rendu : commandes de dessin sprite/tuile/quad/ligne/texte |
| POLY_FT4.cs | `POLY_FT4` | **Runtime** | Structure de primitive GPU PSX quad texturé flat |
| POLY_G4.cs | `POLY_G4` | **Runtime** | Structure de primitive GPU PSX quad Gouraud-shaded |
| Renderer.cs | `Renderer` | **Runtime** | Implémentation concrète du renderer : tri de sprites, cache, dessin GDI+ |
| SpriteDepth.cs | `SpriteDepth` | **Runtime** | Constantes de valeurs de couche de tri de profondeur UI/effet |
| SPRT.cs | `SPRT` | **Runtime** | Structure de primitive sprite GPU PSX |
| TILE.cs | `TILE` | **Runtime** | Structure de primitive rectangle coloré GPU PSX |
| TimLoader.cs | `TimLoader` | **Asset Loading** | Charge et décode les fichiers image PSX TIM (4/8/16bpp avec palettes CLUT) |

## Sound/

| Fichier | Classe | Statut | Description |
|---------|--------|--------|-------------|
| SoundBin.cs | `SoundBin` | **Both** | Charge sound.bin (données SEQ, headers/bodies VAB) ; lecture SFX runtime avec décodage ADPCM |
| SoundFont.cs | `SoundFont` | **Asset Loading** | Convertit les données VAB en format SoundFont : décode les samples ADPCM, construit la structure SF2 |
| VoiceInfo.cs | `VoiceInfo` | **Runtime** | Conteneur de données pour les assignations de canaux voix SPU (IDs SFX, volumes, pans) |

## Text/

| Fichier | Classe | Statut | Description |
|---------|--------|--------|-------------|
| Font3.cs | `Font3` | **Both** | Charge les données de palette et d'image de police/HUD ; génère les sprites bitmap pour le rendu |
| FontCharInfo.cs | `FontCharInfo` | **Runtime** | Conteneur de données pour les métriques de caractère de police (largeur, hauteur, UV source, y-offset) |
| TextDecoder.cs | `TextDecoder` | **Runtime** | Décode les chaînes de texte du jeu : conversion CP850-Latin1 et tokens de caractères spéciaux |

## UI/

| Fichier | Classe | Statut | Description |
|---------|--------|--------|-------------|
| CallBackInfo.cs | `CallBackInfo` | **Runtime** | Record de callback UI : position, dimensions, délégués init/render |
| HudManager.cs | `HudManager` | **Runtime** | Initialisation et rendu du HUD : barres HP/MP, icônes de vie, sprites de nombres |
| MainInventoryManager.cs | `MainInventoryManager` | **Runtime** | UI inventaire principal : affichage armes/items, animation curseur, compteurs argent/clés |
| MemoryCardManager.cs | `MemoryCardManager` | **Runtime** | UI sauvegarde/chargement : processus carte mémoire, sérialisation données, gestion de slots |
| SubInventoryManager.cs | `SubInventoryManager` | **Runtime** | UI sous-inventaire : affichage armurerie/bottes, rendu description d'items |
| UiBoxAnimated.cs | `UiBoxAnimated` / `UIBoxConfiguration` / `TextToDisplay` / `UIMemoryFileBox` | **Runtime** | Conteneurs de données pour les layouts UI, état d'affichage de texte, couleurs de boîte mémoire |
| UIDebugManager.cs | `UIDebugManager` | **Runtime** | UI menu de flags debug : initialisation, rendu, gestion d'overlays |
| UiDrawCmd.cs | `UiDrawCmd` | **Runtime** | Conteneur de données pour une commande de dessin de tuile UI (UV, palette, taille) |
| UIHandler.cs | `UiHandler` | **Both** | Charge les fichiers palette/texture UI ; gère le rendu et l'animation des boîtes de dialogue au runtime |
| UIHelper.cs | `UiHelper` | **Runtime** | Génération bitmap UI à partir de commandes de dessin ; définitions statiques de données boîtes dialogue/inventaire |
| UiLerper.cs | `UiLerper` | **Runtime** | État d'interpolation linéaire pour les transitions animation d'éléments UI |
| UIManager.cs | `UIManager` | **Runtime** | Gestionnaire UI maître : boîtes de dialogue, noms de personnages, choix, rendu de texte |
| UIRecord.cs | `UiRecord` | **Runtime** | Record d'élément UI : statut, définition de boîte, pointeurs de fonctions setup/render |

---

## Résumé

| Classification | Nombre |
|----------------|--------|
| **Asset Loading** | 33 |
| **Runtime** | 62 |
| **Both** | 12 |
| **Total** | 107 |
