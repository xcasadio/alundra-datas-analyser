# Prompt de reprise — Phase 3 du portage de LOADER.EXE

> Copier tout ce qui suit comme premier message de la nouvelle session.

---

Je continue la transliteration de `LOADER.EXE` (Alundra PSX, SLES-01198 FR) en C#.

**Lis d'abord `docs/loader-exe-transliteration-plan.md` en entier.** Il contient l'état
d'avancement, toutes les adresses Ghidra, les cartographies VRAM déjà fermées et les pièges
rencontrés. Ne re-dérive pas ce qui y est déjà consigné et vérifié.

## Objectif de cette session

Terminer la Phase 3, dans cet ordre — chaque étape débloque la suivante :

### 1. Rasteriseur de glyphes (le verrou actuel)

Localiser puis porter la fonction qui écrit le texte dans les tampons de tuiles.

Ce que je sais déjà, ne le recherche pas :

- `SetTextLayer` (0x800221d4) **n'est pas** le rasteriseur : c'est un initialisateur qui réemploie
  la structure `UIBox` de 36 octets comme descripteur de couche de texte. Voir §6.3 du plan pour la
  correspondance des champs.
- ⚠️ Le champ à +4 du descripteur contient un **pointeur** vers la `UIBox` d'affichage liée, pas un
  `otIndex`. Il faut donc un type `TextLayer` **distinct** de `UiBox` — ne réemploie pas la classe
  existante comme le fait l'original.
- `g_tileBuffer1` (0x8014a8b8) n'a **qu'une seule référence** dans tout le binaire, celle de
  `InitializeTileMap` dans `InitializeSelectionScreenGraphics`. Le rasteriseur passe donc par la
  structure `TileMap`, pas par le tampon directement.
- **Piste à suivre** : les appelants de `ClearTile`, et les fonctions prenant un `TileMap *`.
- La fonte est `g_loadRoomFontTim` — ressource ETC `TIM` #4, image #3 de `loader.idx`, 256×256
  en 4 bpp.

Vérification attendue : rendre une chaîne connue dans `g_tileMap1`, puis contrôler le contenu du
rectangle VRAM (0x2C0, 0x180) de taille 0x100×0x30.

### 2. Écran de sélection de sauvegarde

La cartographie VRAM est **déjà fermée** au §6.3 (4 couches de tuiles, 10 `UIBox` avec leurs
`packedU`/`packedV`, tailles, offsets écran et CLUT). Câble-la sur `LoaderUiRenderer`.

Puis la machine à états `RunLoaderMainSequence` (0x80024e28) : `InitSelectionMenu` →
`ShowSelectionScreen` → `GetUserInput` / `ValidateSelection` → `ProcessSelection`.

Les sauvegardes viennent du backend desktop qui **existe déjà** :
`{BaseDirectory}/saves/card{0|1}/{titre}-{NN}.json`, chargées par `SaveData.LoadFromJson`.
Ne réimplémente aucun accès carte mémoire. Le loader gère 4 emplacements de 0x76C octets, drapeau
d'occupation à +8.

### 3. Décor et fondus

`UpdateBackgroundState` (0x800231c4, avec la caméra `g_cameraStartX/Y`),
`UpdateMenuGraphics` (0x80023b14), `UpdateFadeState` (0x800243d4),
`InitSelectionBackdropTiles` (0x80023d94).

### 4. Écran de chargement de boot

`LoaderEngine.SkipBootLoadingScreen` vaut `true` parce que la scène réelle (`RunLoadingScreenIntro`,
0x80024fa8) a besoin de la couche de tuiles de la *load room*. Une fois l'étape 1 faite, repasse-le
à `false` et porte la vraie scène.

## Règles de fidélité

Annotation obligatoire sur chaque fonction et global porté :

```csharp
// GHIDRA: <nomOriginal> @ 0x<adresse>
// SOURCE: Ghidra (ReVa get-decompilation) | PCSX-Redux runtime | lecture directe
// JUSTIFICATION: PSX hardware adaptation only | C# language bridge only
// RELATION: <ce que faisait l'original et pourquoi l'adaptation est équivalente>
```

Classification de preuve : `CERTAIN` / `PROBABLE` / `PARTIAL` / `INCONNU` / `BLOCKED` /
`CORRECTION`. Toute déviation volontaire doit être annotée `DELIBERATE DEVIATION` (il y en a deux à
ce jour, §4.7 et §5.3).

Interdits : redessiner l'architecture, introduire des abstractions modernes dans le cœur porté,
supprimer un chemin mort sans l'annoter, inventer une valeur non lue dans Ghidra, l'EXE ou
l'émulateur.

`PsxSdk` doit rester **indépendant d'Alundra** : aucun type du jeu, aucun chemin de fichier du jeu,
aucune table extraite de son exécutable. Le code spécifique va dans `AlundraEngine/Loader/`.

## Pièges déjà rencontrés — ne les refais pas

1. **Cache de textures figé.** `AlundraRenderer._textureCache` est indexé sur l'**instance** du
   `Bitmap`. Toute image reconstruite frame par frame doit appeler
   `IRenderer.InvalidateTexture(bitmap)`, sinon elle se fige silencieusement sur son premier
   contenu. Et ne détruis jamais un `Bitmap` que le renderer garde comme clé : remplis-le sur place
   (voir le compteur de génération VRAM dans `LoaderUiRenderer`).
2. **Deux VAB d'effets sonores distincts.** `DAT_8012d5d2` (chargé par `FUN_80028504`, message
   `Load Se`) contre `DAT_8012d5d4` (chargé par `FUN_80028650`, message `Load SeGroup`). Les deux
   sont des `VABp` valides : un contrôle de magie ne les distingue pas. Voir §6.2.
3. **Vitesse CD.** 1× = 75 secteurs/s, donc 2× = **150**, pas 300. Pour dater un flux STR, part de
   l'audio XA entrelacé, jamais de la vitesse supposée du lecteur.
4. **Table de quantification MDEC.** Entrée 0 = **2**, pas 8 comme MPEG-1.
5. **Nom trompeur corrigé.** `PlayTransitionAnimation` a été renommée `PlaySoundEffect` (0x80028b40)
   dans Ghidra, avec un commentaire de plaque portant les preuves.

## Méthode de vérification

Je ne peux ni voir ni entendre le résultat, donc chaque changement doit être vérifié par mesure :

- **Graphique** : piloter `LoaderEngine` avec le `Renderer` logiciel
  (`new Renderer(Graphics, Bitmap)`), capturer des frames en PNG et comparer numériquement
  (pixels différents, delta max) entre régions ou entre instants. Un contrôle « ça affiche quelque
  chose » ne suffit pas : compare aussi une région qui **ne doit pas** changer.
- **Audio** : rendre le PCM du mixeur SPU et faire un A/B entre deux exécutions identiques, avec et
  sans l'événement. Le séquenceur est déterministe. Suivre la **décroissance** par tranches, pas
  seulement la crête — c'est ce qui distingue un son correct d'un son qui boucle.
- **Référence externe** : jPSXdec v2.0 dans `D:\development\repo\Alundra Remake\jpsxdec_v2.0`
  (`-vidfmt bs` sort le bitstream démultiplexé, `-vidfmt mdec` le flux de codes MDEC, ce qui permet
  d'isoler l'étage fautif sans deviner). ⚠️ jPSXdec indexe les frames à partir de 0, les en-têtes
  STR à partir de 1.

Construis avec `dotnet build AlundraTools\AlundraGame\AlundraGame.csproj -c Release` — la solution
entière échoue tant que `AlundraGameRuntimeMcpServer.exe` tourne (fichier verrouillé), ce n'est pas
une régression.

Mets à jour `docs/loader-exe-transliteration-plan.md` au fur et à mesure, et corrige Ghidra quand tu
établis qu'un nom est faux.
