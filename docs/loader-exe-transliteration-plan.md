# LOADER.EXE — Plan de transliteration et état d'avancement

> **Cible** : `AlundraTools/AlundraEngine/Loader/` + `AlundraTools/PsxSdk/` — porter `LOADER.EXE`
> (Alundra PSX, SLES-01198 FR) en C#, sur le modèle de `AlundraEngine/Closing/ClosingEngine.cs`.

---

## 0. État d'avancement

| Phase | Contenu | État |
|---|---|---|
| 0 | `LoaderExeInspector` — 16 ressources TIM de LOADER.EXE | ✅ fait |
| 1 | Lecture vidéo PSX (STR / MDEC) dans `PsxSdk` | ✅ fait, validé contre jPSXdec |
| 2 | Intégration `LoaderEngine` : séquence de boot + attract | ✅ fait |
| 3 | Écran-titre, menu, texte, sélection de slot, écran de boot | ✅ fait |

**Deux exigences ont modifié le plan initial :**

1. **`lastFrame` est corrigé** (exception assumée à la transliteration) — voir §4.7.
2. **Le code vidéo et les fonctions du SDK PSX sont dans un projet séparé** `PsxSdk`, sans aucune
   dépendance à Alundra, réutilisable pour d'autres jeux PSX — voir §3.

**Déviations assumées** (annotées `DELIBERATE DEVIATION` dans le code) : §4.7 uniquement.
`SkipBootLoadingScreen` est repassé à `false` — la scène réelle est portée, voir §6.7.

---

## 1. Contexte

`LOADER.EXE` est le premier exécutable lancé par `SYSTEM.CNF`. Il enchaîne :

1. écran de chargement (image TIM interne à l'EXE) ;
2. vidéo de présentation de l'éditeur (`\MOVIE\EURO_OP.MOV`) ;
3. écran-titre + menu **Démarrer / Continuer** ;
4. si inactivité (30 s) : vidéo d'intro (`\MOVIE\ARAN_OP.MOV`) puis retour au menu — boucle d'attract ;
5. sur validation : `LoadExec("cdrom:\ALUN_CD.EXE;1")` → le jeu.

---

## 2. Faits établis (preuves — ne pas re-dériver)

### 2.1 Adressage EXE

En-tête PS-EXE : `t_addr = 0x80020000`, `t_size = 0x0012B000`, `pc0 = 0x800360F4`.

```
File Offset = RAM Address - 0x8001F800
```

**Identique au delta de `CLOSING.EXE`.**

### 2.2 Graphe d'appel du flux principal

```
main (0x80025668)
├── InitializePsx (0x80025238)
├── RunLoadingScreenIntro (0x80024fa8)
└── boucle infinie : LaunchGame(mode)   mode = 0x840 puis -1
    └── LaunchGame (0x800255f0)
        ├── PlayMovie(0, 0x28, &g_EURO_OP_MOV_fileInfo, 0xaf4, mode, 0x136)
        ├── MainLoop(1)                        ← écran-titre + menu
        ├── PlayMovie(8, 0x10, &g_ARAN_OP_MOV_fileInfo, 0xeab, mode, 0)
        └── MainLoop(1)
```

`MainLoop` (0x8002538c) → `PromptNewGameOrContinue(0x708, bgm)` → si `-1` (timeout 1800 frames)
retour à `LaunchGame` ; si `0` → nouvelle partie ; si `1` → `RunLoaderMainSequence`
(sélection de slot) ; puis `LoadExec("cdrom:\\ALUN_CD.EXE;1", ...)`.

**Masques de boutons** (bits conformes à `AlundraEngine.Gameplay.PadState`) :
`0x1000` Haut, `0x4000` Bas, `0x0800` Start, `0x0040` Croix.
Donc `0x840` = Start|Croix — le masque de saut de vidéo du premier passage.

### 2.3 Fichiers vidéo (format STR brut, secteurs de 2048 octets)

| Fichier | Dimensions | Secteurs | Vidéo | Audio XA | Frames | Version |
|---|---|---|---|---|---|---|
| `EURO_OP.MOV` | 320×160 | 28 200 | 24 675 | 3 525 | 2 820 | 3 |
| `ARAN_OP.MOV` | 304×224 | 37 999 | 32 981 | 5 018 | 3 765 | 3 |
| `MATRIX.MOV` | 320×240 | 2 039 | 1 779 | 260 | 203 | 2 |
| `ARAN_END.MOV` | 320×160 | 66 215 | 57 834 | 8 381 | 6 602 | 3 |

Entrelacement : 1 secteur audio tous les 8. `ARAN_OP.MOV` se termine par **307 secteurs de
bourrage** et `ARAN_END.MOV` par **120** — toute recherche du dernier secteur vidéo doit en tenir
compte (une fenêtre de balayage trop courte échoue silencieusement sur ces deux fichiers).

**Cadence : 15 fps** sur les quatre fichiers.

⚠️ Piège coûteux : la vitesse simple d'un CD-ROM est **75 secteurs/s** (150 Ko/s de données
utilisateur de 2048 octets), donc la double vitesse — celle que sélectionne `CdRead2(0x1c0)` via
son bit `CdlModeSpeed` — vaut **150 secteurs/s**, pas 300. Avec ≈ 10 secteurs par frame cela donne
15 fps. Une première version du port utilisait 300 et jouait tout deux fois trop vite.

L'audio XA verrouille ce chiffre **sans dépendre d'aucune hypothèse sur le lecteur** : il est en
37800 Hz stéréo 4 bits, donc un secteur Form 2 porte 2016 trames d'échantillons par canal =
53,33 ms, soit 18,75 secteurs audio/s ; le multiplexeur en place un tous les 8, ce qui impose
150 secteurs/s. Diviser le nombre de frames de chaque film par la durée totale de son audio XA
donne 15,00 / 14,86 / 14,96 / 14,99 fps pour EURO_OP / ARAN_OP / ARAN_END / MATRIX.

> Méthode réutilisable : pour dater un flux STR, **ne pas partir de la vitesse supposée du lecteur** ;
> partir de l'audio entrelacé, dont la cadence est imposée par sa fréquence d'échantillonnage.

En-tête STR (32 octets, en tête de chaque secteur vidéo), little-endian :

| Offset | Type | Champ |
|---|---|---|
| 0x00 | u16 | `magic` = `0x0160` |
| 0x02 | u16 | `type` = `0x8001` |
| 0x04 | u16 | `chunkNo` |
| 0x06 | u16 | `chunksTotal` |
| 0x08 | u32 | `frameNo` (base 1) |
| 0x0C | u32 | `demuxSizeBytes` |
| 0x10 | u16 | `width` |
| 0x12 | u16 | `height` |
| 0x14 | u16 | `runLengthCodeCount` (mots 32 bits de données MDEC, arrondi à un multiple de 32) |
| 0x16 | u16 | `magic` = `0x3800` |
| 0x18 | u16 | `quantScale` |
| 0x1A | u16 | `version` (2 ou 3) |
| 0x1C | u32 | inutilisé |

**Point clé** : la charge utile (octets 32..2047) commence par une copie exacte des 8 octets
0x14..0x1B. `FUN_80027738` s'en sert comme test de validité de frame.

**Audio — ré-extraction obligatoire.** Les `.MOV` extraits ont été écrits à 2048 octets par secteur
uniformément. Les secteurs XA sont des Mode 2 **Form 2** (2324 octets utiles) : ils y sont tronqués
à 16 des 18 groupes ADPCM, soit 11 % des échantillons perdus toutes les 53 ms. Ces fichiers ne
peuvent donc pas produire de son correct, quelle que soit la qualité du décodeur.

La ré-extraction se fait depuis l'image CD brute en conservant des secteurs entiers :

```bash
dotnet run --project AlundraTools/AlundraDataExtractor -- --extract-movies "<image.bin>" "<...>/MOVIE"
```

Elle produit des `.STR` (2352 octets par secteur, sous-en-têtes conservés) à côté des `.MOV`.
`LoaderEngine` prend le `.STR` s'il existe et retombe sinon sur le `.MOV` (vidéo seule, avec un
message dans la trace).

### 2.4 Ressources TIM internes à `LOADER.EXE` — table close

La colonne « Start Offset » des `.idx` jPSXdec **est** exploitable, contrairement à ce qu'affirme
le commentaire d'en-tête de `Closing/ClosingExeInspector.cs`. Formule générale :

```
fileOffset = (idx.SectorStart - fichier.PremierSecteur) * 2048 + idx.StartOffset
```

* `fichier.PremierSecteur` = secteur de la ligne `Type:File` du même `.idx` — **0** pour
  `loader.idx` (produit depuis l'EXE autonome), **7159** pour `closing.exe.idx` (produit depuis
  l'image disque).
* Le pas reste **2048** même si le `.idx` annonce `Sector size:2352`.

Vérifié : **16/16** sur `LOADER.EXE`, **13/13** sur `CLOSING.EXE`. Les adresses RAM correspondantes
portent des symboles Ghidra déjà nommés :

| # | Offset fichier | Adresse RAM | Symbole Ghidra | Dim. | Bpp |
|---|---|---|---|---|---|
| 0 | `0x024C38` | `0x80044438` | `g_loadRoomBackgroundTimBuffer` | 320×384 | 8 |
| 1 | `0x042E60` | `0x80062660` | `g_loadRoomCloudsTim` | 320×128 | 4 |
| 2 | `0x047EA8` | `0x800676A8` | `g_loadRoomBackgroundMessageTim` | 256×256 | 4 |
| 3 | `0x04FEF0` | `0x8006F6F0` | `g_loadRoomFontTim` | 256×256 | 4 |
| 4 | `0x057F38` | `0x80077738` | `g_loadRoomSpriteSheetTim` | 256×256 | 8 |
| 5 | `0x068160` | `0x80087960` | `g_loadingScreenTim` | 320×240 | 16 |
| 6 | `0x08D97C` | `0x800AD17C` | `g_licenceScreenTim` | 256×256 | 4 |
| 7–14 | `0x0959C4` … `0x0EE0DC` | `0x800B51C4` … `0x8010D8DC` | `g_TitleFrame0` … `g_TitleFrame7` | 320×160 | 8 |
| 15 | `0x0FAB0C` | `0x8011A30C` | `g_TitleFull` | 320×240 | 8 |

> `g_TitleFull` contient déjà les libellés « Démarrer » / « Continuer » gravés dans l'image ; seul
> le surlignage de l'option sélectionnée passe par la couche UIBox (Phase 3).
>
> ⚠️ Table établie sur la version **France**. `LoaderExeInspector` valide chaque offset au
> chargement et lève une exception explicite si la forme du TIM ne correspond pas, ce qui est le
> symptôme attendu sur une autre version régionale.

---

## 3. Architecture réalisée

```
AlundraTools/PsxSdk/                     ← net9.0, AUCUNE dépendance Alundra / System.Drawing / MonoGame
├── Cd/CdSector.cs                       géométrie des secteurs CD
├── Cd/CdImageReader.cs                  image BIN/ISO + arborescence ISO 9660 + extraction brute
├── Audio/XaAdpcmDecoder.cs              CD-XA ADPCM → PCM 16 bits
├── Mdec/MdecTables.cs                   zigzag, table de quantification, code books VLC
├── Mdec/MdecBitReader.cs                lecture binaire mots 16 bits LE, MSB d'abord
├── Mdec/MdecVlcDecoder.cs               équivalent DecDCTvlc → flux de codes MDEC
├── Mdec/MdecImageDecoder.cs             équivalent du MDEC matériel (déquant + IDCT + YUV→RGB24)
├── Streaming/StrFrameHeader.cs          en-tête STR 32 octets
├── Streaming/StrSectorReader.cs         démultiplexeur (remplace le ring buffer CD)
├── Video/MoviePlaybackOptions.cs        les 6 paramètres de PlayMovie
└── Video/StrMoviePlayer.cs              machine à états de PlayMovie

AlundraTools/AlundraEngine/Loader/       ← spécifique Alundra
├── LoaderExeInspector.cs                les 16 TIM, conteneur ETC, tables son/fonte/hotspots
├── LoaderState.cs                       états de la séquence de boot
├── MovieFrameBitmap.cs                  RGB24 → Bitmap pour IRenderer
├── UiBox.cs                             struct UIBox de 36 octets
├── LoaderUiRenderer.cs                  RenderUIBox (sprite + sprite pivoté) et POLY_F4
├── LoaderTileMap.cs                     struct TileMap de 44 octets + pixels/blit/scroll
├── LoaderTextLayer.cs                   descripteur de couche de texte + fonte + typewriter
├── LoaderEtcStrings.cs                  DATA/ETC_RES.R
├── LoaderSaveSlots.cs                   4 enregistrements de 0x76C octets ← backend desktop
├── LoaderSelectionScreen.cs             RunLoaderMainSequence et tout ce qu'elle pilote
└── LoaderEngine.cs                      main + LaunchGame + MainLoop + écran de boot
```

`PsxSdk` n'expose que des `byte[]`, `Stream` et structures simples : c'est l'hôte qui décide
comment afficher une frame. **Aucun type Alundra, aucun chemin de fichier Alundra, aucune table
extraite de l'exécutable d'Alundra n'y figure.**

---

## 4. PHASE 1 — Lecture vidéo PSX (STR / MDEC) ✅

### 4.1 Frontière logiciel / matériel

* **`DecDCTvlc` @ `0x8002ba84`** — 832 octets, aucun appelé : le décodeur VLC/RLE **logiciel** de
  `libpress`, piloté par quatre tables pré-aplaties résidant dans l'exécutable :
  `0x80132a88` (DC luma), `0x80132e88` (DC chroma), `0x80133288` (AC primaire, 8192 × 8 octets),
  `0x80143288` (AC secondaire/échappement).

  **Décision d'architecture** : ces tables ne sont *pas* extraites de LOADER.EXE. Ce sont des
  aplatissements d'accélération des code books **standard** (tables VLC MPEG-1 réutilisées par le
  MDEC). Les recopier lierait le SDK à l'exécutable d'Alundra, ce qui contredit l'exigence de
  réutilisabilité. `MdecTables` exprime donc le code book sous sa forme canonique documentée et
  `MdecVlcDecoder` en construit une table de correspondance équivalente à l'initialisation. La
  sémantique (en-tête, prédiction DC par type de bloc, format des mots de sortie, marqueur de fin
  de frame) est conservée à l'identique.

* **`DecDCTin` @ `0x8002b4ac` / `DecDCTout` @ `0x8002b528`** — simples amorces DMA vers le MDEC.
  Aucune logique de décodage. → remplacés par `MdecImageDecoder`.

* **`StSetRing` / `StGetNext` / `StFreeRing` / `StCdInterrupt` / `CdRead2`** — pilote CD-ROM temps
  réel (registres, DMA, ISR). → remplacés par `StrSectorReader`, qui ne conserve que la sémantique
  de `StGetNext`.

### 4.2 Table de quantification — correction importante

`MDEC_reset` (@ `0x8002b5d0`) téléverse la table de quantification depuis `0x80132944` avec la
commande `0x40000001` (128 octets = luma + chroma, les deux copies identiques).

Lue dans l'exécutable et comparée entrée par entrée : c'est **la matrice intra par défaut MPEG-1
en ordre zigzag, avec exactement une différence — l'entrée 0 vaut `2` et non `8`**.

Ce facteur 4 n'est pas cosmétique : le bitstream STR stocke le DC **multiplié par 4** dans le champ
DC 10 bits du MDEC (explicitement en version 3, où `DecDCTvlc` émet `(dc & 0xff) << 2` ; et de fait
en version 2 pour le DC brut). Avec la valeur MPEG-1 de 8, tous les DC ressortent 4× trop grands :
la luminance sature et la chrominance devient violemment sursaturée. C'était le dernier bug du
décodeur, identifié en mesurant qu'un DC de 480 doit produire le pixel 248 (luma 120 = 480/4).

La seconde table (`0x801329c8`, commande `0x60000000`) est la table d'échelle IDCT en Q15
(`0x5A82` = 0,7071) — confirme un IDCT standard ; l'implémentation flottante est équivalente.

### 4.3 Fonctions portées

| Adresse | Nom Ghidra | Emplacement C# | Règle |
|---|---|---|---|
| `0x80027ff4` | `PlayMovie` | `StrMoviePlayer` | 1:1 → machine à états |
| `0x80027c10` | `FUN_80027c10` | `StrMoviePlayer` ctor / `StrSectorReader` | 1:1 sauf couche CD |
| `0x80027888` | `FUN_80027888` | `StrMoviePlayer.DecodeNextFrame` | 1:1 |
| `0x80027738` | `FUN_80027738` | `StrSectorReader.TryGetNextFrame` | 1:1 |
| `0x800279b4` | `FUN_800279b4` | fusionné (décodage synchrone) | adaptation |
| `0x80027a4c` | `FUN_80027a4c` | `MdecImageDecoder` | adaptation |
| `0x8002790c` | `FUN_8002790c` | `MdecImageDecoder` | adaptation |
| `0x80027d6c` | `FUN_80027d6c` | `LoaderEngine.UpdateMovie` | adaptation |
| `0x80027f10` | `FUN_80027f10` | `StrMoviePlayer.Volume` | adaptation |
| `0x8002ba84` | `DecDCTvlc` | `MdecVlcDecoder` | sémantique 1:1, code book canonique |
| `0x8002b340` | `DecDCTReset` | `MdecTables` | données lues dans l'EXE |

### 4.4 Découpage en bandes — non porté, et pourquoi

L'original décode par bandes verticales de 24 pixels VRAM (= 16 pixels réels en 24 bpp) pour
recouvrir le travail du MDEC avec le DMA. Ce recouvrement n'est **pas observable dans la sortie** et
n'a aucun intérêt sans DMA. `MdecImageDecoder` décode donc la frame entière, mais **conserve
l'ordre des macroblocs colonne par colonne** — c'est précisément cet ordre qui rendait le découpage
en bandes possible, et il est imposé par le bitstream.

La branche « première bande partielle » (`vramRect.w % 0x18 != 0`) n'est de toute façon jamais prise
pour les quatre vidéos du jeu (480 % 24 = 0, 456 % 24 = 0).

### 4.5 Ordre des blocs dans un macrobloc

`Cr, Cb, Y0 (haut-gauche), Y1 (haut-droite), Y2 (bas-gauche), Y3 (bas-droite)`.

> Piège rencontré : Cr et Cb occupant les indices 0 et 1, les quatre quadrants luma commencent à
> l'indice **2**. Oublier ce décalage fait lire la chrominance comme luminance sur la moitié haute
> de chaque macrobloc — l'image reste structurée mais devient un empilement de bandes colorées.

### 4.6 Sémantique de `PlayMovie` conservée

* Saut : `curFrame > SkipAfterFrame` **et** boutons pressés ⊆ masque **et** intersection non vide.
* Fondu sonore sur les 15 dernières frames, arrêt quand le volume atteint 0.
* Chien de garde : arrêt si le numéro de frame recule.

### 4.7 `lastFrame` — DÉVIATION ASSUMÉE

L'original passe une limite de frames codée en dur, **inférieure** au contenu réel des fichiers :

| Vidéo | `param_4` | Frames réelles | Coupées |
|---|---|---|---|
| `EURO_OP.MOV` | `0xAF4` = 2804 | 2820 | 16 |
| `ARAN_OP.MOV` | `0xEAB` = 3755 | 3765 | 10 |

`MoviePlaybackOptions.StopAtLastFrame` vaut `false` par défaut : les vidéos sont jouées jusqu'à leur
fin naturelle. Mettre la propriété à `true` restaure la troncature d'origine à l'identique.
Le fondu sonore se déclenche alors 15 frames avant la vraie fin plutôt qu'avant la limite.

### 4.8 Vérification — résultats

Décodeur comparé à **jPSXdec v2.0** (`-quality psx -up NearestNeighbor`), qui sert de référence.
Attention : jPSXdec indexe les frames à partir de 0, les numéros d'en-tête STR commencent à 1.

| Test | Résultat |
|---|---|
| `MATRIX.MOV` frame 150 (version 2) | 91,15 % des canaux **bit-exacts**, écart moyen 0,12, max 9 |
| `EURO_OP.MOV` frame 2001 (version 3) | écart moyen **0,299**, max 6, **99,64 % à ±2** |
| Décodage intégral des 4 vidéos | 13 390 frames, aucune exception, aucune troncature |
| Débit | ≈ 900 frames/s en Release (30 fps requis) |

Les écarts résiduels sont l'arrondi de l'IDCT (implémentations différentes), pas une divergence
d'algorithme.

---

## 5. PHASE 2 — Intégration `LoaderEngine` ✅

`LoaderEngine.MainLoop()` est appelé une fois par frame affichée par `AlundraGame.Draw`.
Séquence vérifiée sur 60 000 frames simulées :

```
PlayMovieEuro  11280 frames hôte = 188,0 s  (2820 frames vidéo à 15 fps — film complet)
TitleMenu       1800 frames hôte =  30,0 s  (0x708)
PlayMovieIntro 15060 frames hôte = 251,0 s  (3765 frames vidéo — film complet)
TitleMenu       1800 frames hôte              → puis alternance EURO / ARAN indéfiniment
```

(L'écran de chargement de boot est sauté par défaut, voir §5.2.)

Start ou Croix depuis le menu → `GameState.Game`.

Pièges rencontrés et corrigés :

* l'alternance de l'attract doit suivre **quelle vidéo a été jouée en dernier**, sinon la boucle se
  bloque sur `EURO_OP` après le premier cycle ;
* le compteur d'inactivité est réarmé par **n'importe quel bouton maintenu**
  (`if (buttons == 0) iVar6 = loopIndex;`), pas seulement par Haut/Bas.

### 5.1 Backend MonoGame — cache de textures et images mutables

`AlundraRenderer._textureCache` est un `Dictionary<Bitmap, Texture2D>` **indexé sur l'instance du
bitmap**. L'hypothèse implicite est qu'un bitmap ne change jamais après son premier upload : vraie
pour toutes les ressources statiques, fausse pour une frame de vidéo, qui est un seul bitmap
réécrit 30 fois par seconde.

Symptôme : **écran noir pendant toute la vidéo** — la texture uploadée à la première frame (noire,
les deux films démarrant sur un fondu depuis le noir) était renvoyée indéfiniment.

Correctif : `IRenderer.InvalidateTexture(Bitmap?)`. `AlundraRenderer` réuploade en place dans la
`Texture2D` existante (pas de réallocation par frame) ; le `Renderer` logiciel n'a rien à faire
puisqu'il relit le bitmap à chaque tracé. `MovieFrameBitmap` conserve **un bitmap stable par
taille** (320×160 et 304×224) au lieu de réallouer à chaque changement de film, sinon chaque
alternance de l'attract abandonnait une texture dans le cache.

> À retenir pour la Phase 3 et au-delà : toute image reconstruite frame par frame doit appeler
> `InvalidateTexture`, sinon elle se fige silencieusement sur son premier contenu.

### 5.2 Audio des vidéos

`XaAdpcmDecoder` décode les secteurs Form 2 en PCM 16 bits. Rien à transliterer : sur console le
contrôleur CD envoie les secteurs XA directement au décodeur ADPCM du SPU, aucun code du jeu ne
touche ces échantillons. La seule chose que le jeu pilote est le volume d'entrée CD
(`FUN_80027f10` @ `0x80027f10` → `SpuSetCommonAttr`), que `StrMoviePlayer.Volume` conserve et que
`IMovieAudioOutput.Volume` reproduit — la rampe de fin de film fonctionne donc telle quelle.

Points vérifiés :

* le filtre de prédiction doit utiliser un **décalage arithmétique**, pas une division :
  `(k0*prev1 + k1*prev2 + 32) >> 6`. Avec `/ 64` l'erreur d'arrondi sur les sommes négatives
  s'accumule dans l'IIR — mesuré contre jPSXdec sur MATRIX : décalage → écart max 16 / moyen 1,69 ;
  division → max 78 / moyen 17,3 ;
* la production audio est automatiquement à la bonne cadence, puisque l'audio et la vidéo sortent
  des mêmes secteurs : une frame vidéo consomme ≈ 1,25 secteur audio, soit 2520 trames à 15 fps
  = 37800 Hz. Mesuré : 187,95 s d'audio pour 187,99 s de vidéo sur EURO_OP, 251,25 / 251,28 sur
  ARAN_OP, 440,59 / 440,63 sur ARAN_END, zéro échantillon perdu.

`MonoGameMovieAudioOutput` est volontairement séparé de `MonoGameSoundPlaybackBackend` : ce dernier
possède le flux 44100 Hz du mixeur SPU pour la musique et les bruitages, alors que les vidéos du
loader tournent avant le démarrage du driver son et portent leur propre flux 37800 Hz.

### 5.3 Écran de chargement de boot — **activé**, voir §6.7

`LoaderEngine.SkipBootLoadingScreen` valait `true` tant que la scène n'était pas portée. Elle l'est
désormais et la constante est à `false`. ⚠️ Cette section supposait que l'image à afficher était
`g_loadingScreenTim` : c'est faux, c'est `g_licenceScreenTim`. Détail et mesures au §6.7.

---

## 6. PHASE 3 — Écran-titre, texte et sélection de sauvegarde ✅

### 6.1 Fait

* **VRAM émulée** — `PsxSdk/Graphics/PsxVram.cs` : framebuffer 1024×512 en mots de 16 bits,
  `LoadImage`, téléversement de TIM, échantillonnage de sprite (4/8/16 bpp, CLUT, couleur 15 bits).

  > Décision : le pipeline UI de la PSX ne dessine **pas** depuis des images décodées. Il téléverse
  > le bloc de pixels et la CLUT d'un TIM à des rectangles VRAM fixes, puis les sprites
  > échantillonnent une page de texture. Émuler cet adressage est à la fois plus fidèle et **plus
  > simple** que de deviner quelle image source un sprite visait : plusieurs éléments partagent une
  > page de texture et les téléversements ultérieurs en écrasent délibérément d'autres.

* **`UiBox`** (`Loader/UiBox.cs`) — structure de 36 octets + `InitializeUiBox` (0x80025d5c),
  `SetUiBoxOffset` (0x80025dc0), `SetUiBoxBaseAndRotation` (0x80025dcc).

  > `LOADER.EXE` embarque sa propre copie du moteur UI ; cette structure **ne correspond pas** à
  > `AlundraEngine/UI/UIBoxConfiguration`. Vérifié avant de porter — les deux exécutables divergent.

* **`LoaderUiRenderer`** — `RenderUIBox` (0x80025dfc, chemin sprite), `RenderUiBoxFlatQuad`
  (0x80026408), `InitializeUiBoxBasePosition` (0x80026320), téléversement TIM façon
  `SetTileLayerBounds` (0x800268ac).

* **Écran-titre** — `InitBootSequenceGraphics` (0x800213c4) pour la partie `g_TitleFull`, et
  `FUN_80021a1c` (0x80021a1c) pour le tracé : bande logo (5 boîtes 64×160), deux entrées de menu
  (128×16), bloc copyright (5 boîtes 64×48), boîte de message.

* **Surbrillance pulsée** — `couleur + (ccos(phase) >> 6)` autour de `0xA0` pour l'entrée
  sélectionnée, `0x40` pour l'autre, phase incrémentée de `0x58` par frame et repliée à `0xFFF`,
  restaurée après le tracé pour que la pulsation ne s'accumule pas. Mesuré : entrée non
  sélectionnée stable à 12,0, sélectionnée oscillant entre 18,1 et 23,9, inversion correcte sur
  Haut/Bas.

### 6.2 Son du loader — analyse close, portage à faire ⬜

Le loader embarque **sa propre copie du driver son**, mais elle utilise exactement le même format
que le jeu : SEQ + VAB extraits de `DATA/SOUND.BIN`. `SoundManager` côté C# expose déjà
`LoadSeq(byte[], short vabId)`, `PlaySoundEffect(uint)` et `AdvanceSoundFrame()` — et il **tourne
déjà pendant le loader**, puisque `AlundraGame` construit `_gameEngine` et démarre
`_soundBackend.StartTickDriver(...)` avant de créer `_loaderEngine`.

#### Table BGM — `DAT_8012d398` (RAM), offset fichier `0x10DB98`

Entrées de 12 octets (3 × u32). Les bornes s'enchaînent **d'une entrée à la suivante** :

```
seq      = [ E[n][0], E[n][1] )
vabHead  = [ E[n][1], E[n][2] )
vabBody  = [ E[n][2], E[n+1][0] )
```

Vérifié contre `SOUND.BIN` — les magies tombent juste :

| Piste | SEQ | taille | VAB head | taille | VAB body |
|---|---|---|---|---|---|
| 0 | `0x581800` | `0x13000` | `0x594800` `VABp` | `0x1000` | `0xF000` |
| **1 (titre)** | `0x5A4800` `SEQp` | `0x8800` | `0x5AD000` `VABp` | `0x3000` | `0x24800` |
| 2 | `0x5D4800` `SEQp` | `0x2000` | `0x5D6800` `VABp` | `0x2000` | `0x16000` |

`PromptNewGameOrContinue` reçoit `bgmTrackId = 1` depuis `MainLoop(1)` : **la musique de l'écran-titre
est la piste 1**. `ShowSelectionScreen` utilise la piste `0x29`.

Chaîne d'appels de `PlayBgmTrack` (0x80028dd8), noms PsyQ rétablis :
`SsVabOpenHeadSticky` → `SsVabTransBody` → `SsSeqOpen(0x801DB2A8, vab)` → `SsSeqSetVol(0x7f,0x7f)`
→ `SsSeqPlay(seq, SSPLAY_PLAY, 0)`.

#### Table SFX — `BYTE_ARRAY_8012d5e4` (RAM), offset fichier `0x10DDE4`

Entrées de 22 octets (`0x16`). Champs utilisés par `PlaySoundEffect` (0x80028b40) :

| Offset | Rôle |
|---|---|
| +0 | référence VAB ; `-2` = entrée désactivée, `-1` = déclenchement direct |
| +2 / +4 / +6 | programme / ton / note passés à `SsUtKeyOnV` |
| +8 | drapeaux ; bit 0 posé pendant la lecture |
| +10 | doit valoir `-1` pour que l'entrée soit jouable |
| +12 | lien vers l'entrée suivante |
| +16 | nombre maximal de voix simultanées |

> ✅ **Renommée dans Ghidra** : cette fonction s'appelait `PlayTransitionAnimation`, ce qui était
> faux — son message d'erreur est `SE ON %d Fialed :MaxOver` et elle finit sur `SsUtKeyOnV`. Elle
> est désormais `void PlaySoundEffect(int sfxId)` (la signature était aussi déclarée `(void)` alors
> que les 15 appelants passent un argument). Le renommage s'est propagé à tous les sites d'appel.

Décodé pour les deux sons du menu :

* **`PlaySoundEffect(1)`** — déplacement du curseur, appelé sur Haut et sur Bas :
  programme 0, ton 0, **note 60**, volumes `0x7f`/`0x7f`.
* **`PlaySoundEffect(2)`** — validation (Start ou Croix).

La banque VAB des effets est chargée par `FUN_80028650(0x25)` depuis `InitializeSoundDriver`
(0x80028338) et son identifiant atterrit dans `DAT_8012d5d2`.

#### Le VAB des effets — attention, il y a **deux** VAB distincts

`InitializeSoundDriver` initialise **trois** handles VAB (`g_bgmVabHandle`, `DAT_8012d5d2`,
`DAT_8012d5d4`) et en remplit deux :

| Handle | Chargé par | Source | Message d'erreur |
|---|---|---|---|
| `DAT_8012d5d2` | `FUN_80028504` (0x80028504) | **trois scalaires fixes** `DAT_8012d134/38/3c` | `Load Se ...` |
| `DAT_8012d5d4` | `FUN_80028650(0x25)` (0x80028650) | table de banques `DAT_8012d13c`, entrées de 8 octets | `Load SeGroup ...` |

**C'est `DAT_8012d5d2` qu'utilise la branche directe de `PlaySoundEffect`**, donc les deux sons du
menu. Il vaut : head `0x000800` (`VABp`, taille `0x2800`), body `0x003000` (taille `0x38800`).

> ⚠️ **Piège rencontré.** Ce port a d'abord chargé la banque 0x25 de la table de groupes
> (`VABp` à `0x2FF000`, body `0x5000`). C'est un VAB parfaitement valide, donc le contrôle de magie
> passait — mais les indices programme/ton adressaient des échantillons sans rapport. Symptômes :
> un « souffle » au lieu du son attendu, **et une boucle infinie**, parce que les drapeaux de
> bouclage du mauvais ton n'ont aucune raison de terminer. Les deux symptômes venaient de la même
> erreur.
>
> Les trois scalaires précèdent immédiatement la table de banques : l'ensemble forme une seule
> chaîne contiguë d'offsets u32, et `DAT_8012d13c` sert à la fois de fin de body pour ce VAB et de
> première entrée de la table de groupes. D'où la confusion.

La table de groupes (146 entrées cohérentes) reste lue par `ReadSfxVabBankTable` : elle servira à
la branche non portée de `PlaySoundEffect`, celle des enregistrements dont le `VabId` référence un
groupe.

#### Portage — fait ✅

`SoundManager` reçoit trois points d'entrée dédiés au loader, qui réutilisent son moteur existant
(`LoadVabFromSoundBinRange`, `LoadSeq`, `TryPlayDirectSoundEffectVoices`) mais sont alimentés par
les tables du **loader** au lieu de celles d'`ALUN_CD.EXE` :

* `PlayLoaderBgm(seqOffset, seqEnd, vabBodyOffset, vabBodyEnd)` / `StopLoaderBgm()`
* `LoadLoaderSfxVab(headerOffset, bodyOffset, bodyEnd)`
* `PlayLoaderSoundEffect(table, sfxId, sfxVabId)`

> Le `SoundManager` du jeu **tourne déjà pendant le loader** (`AlundraGame` construit `_gameEngine`
> et démarre le tick 60 Hz avant de créer `_loaderEngine`), donc aucun second driver son n'est
> nécessaire. La table du loader est **passée en paramètre** plutôt qu'installée par-dessus
> `g_soundEffectData`, que le jeu possède et repeuple depuis `SoundBin`.

`LoaderExeInspector` expose `ReadBgmTrackTable`, `ReadSfxVabBankTable` et `ReadSoundEffectTable`,
chacune validant ses entrées (bornes croissantes, dans les limites de `SOUND.BIN`) avant de
s'arrêter.

**Vérifié** en rendant le PCM du mixeur SPU :

| Contrôle | Résultat |
|---|---|
| Tables lues | 47 pistes BGM, 146 banques VAB, 962 enregistrements SFX |
| Enregistrement #1 | `vab=-1 prog=0 tone=0 note=60 seq=-1 maxVoices=2 toneCount=1` — conforme au décodage |
| BGM piste 1, 3 s | crête 24552 / 32767 — non silencieuse |
| Effet curseur, A/B | l'effet contribue bien au signal |
| Décroissance de l'effet | crête par 0,5 s : 8213 → 567 → 147 → 32 → 8 → 3 → 2 → 1 — extinction propre, **pas de boucle** |

> Le contrôle de décroissance est indispensable : c'est lui qui distingue un effet correct d'un
> effet qui boucle indéfiniment. Une simple mesure « l'effet produit du son » ne l'aurait pas vu.
> Méthode : rendre deux fois la même fenêtre de 5 s, avec et sans l'effet, et suivre le delta max
> par tranche de 0,5 s. La différence des deux signaux isole exactement l'effet, musique annulée.

> Le test A/B est la seule preuve valable pour l'effet : la crête globale est dominée par la
> musique. Le séquenceur étant déterministe, deux exécutions identiques ne divergent que si
> l'effet a réellement alloué des voix. La crête BGM est identique (24552) dans les deux passes.

`PARTIAL` assumé : seule la branche `VabId == -1` de `PlaySoundEffect` est portée. C'est
celle qu'empruntent les deux sons du menu ; la branche chaînée qui remonte `RefSfxId` à la
recherche d'un VAB déjà ouvert ne l'est pas.

### 6.2b Conteneur de ressources « ETC » — **fermé** ✅

C'était le point bloquant commun à l'animation du titre **et** au décor de l'écran de sélection.

`GetEtcResource` (0x800276bc) parcourt un conteneur embarqué dans `LOADER.EXE`, pointé par
`g_loadRoomBackgroundTimPtr`. Format :

```
entrée : [u32 packedId][u32 payloadSize][payload…]
packedId = tag[0] | tag[1]<<8 | tag[2]<<16 | index<<24
fin      : packedId == 0xFF444E45   ("END" + 0xFF)
```

Le conteneur commence **8 octets avant le premier TIM catalogué** (0x024C30). Parcouru et recoupé
contre `loader.idx` — les 15 offsets de payload tombent exactement sur les entrées 0 à 14 :

| Entrée | Offset payload | Correspondance loader.idx |
|---|---|---|
| `TIM` #1..#7 | `0x024C38` … `0x08D97C` | images 0 à 6 |
| **`ANM` #1..#8** | `0x0959C4` … `0x0EE0DC` | **images 7 à 14 = `g_TitleFrame0..7`** |
| `END` | `0x0FAAFC` | — |

Deux conséquences directes :

* les huit `ANM` **sont** les frames que `FUN_80021a1c` fait défiler en VRAM (0x180, 0) toutes les
  8 frames : c'est l'animation du logo, et `LoaderEngine.TitleAnimationAvailable` peut passer à
  `true` une fois le câblage fait ;
* `GetEtcResource(…, "TIM", 3)` — le décor de l'écran de sélection — est l'image #2,
  `g_loadRoomBackgroundMessageTim`, 256×256 en 4 bpp.

`g_TitleFull` est absent du conteneur, et c'est normal : il est situé après le sentinel et
`InitBootSequenceGraphics` le passe directement à `InitializeTileLayer`.

Porté : `LoaderExeInspector.ReadEtcResources()` / `FindEtcResource(name, index)`.

### 6.3 « Continuer » / carte mémoire — analyse close, portage à faire ⬜

#### Cartographie VRAM de l'écran de sélection — **fermée**

Relevée sur `InitializeSelectionScreenGraphics` (0x80022af4). Couches téléversées :

| Couche | Source | Image VRAM | CLUT |
|---|---|---|---|
| `g_transitionGraphics` | `GetEtcResource("TIM", 3)` | (0x3C0, 0) | (0x110, 0x1E0) |
| `g_tileMap1` | tampon dynamique 0x100×0x30 | (0x2C0, 0x180) | — |
| `g_tileMap2` | tampon dynamique 0x100×0x20 | (0x2C0, 0x1B0) | — |
| `g_tileMap3` | tampon dynamique 0x60×0x10 | (0x2C0, 0x1D0) | — |

`UIBox`, toutes en 4 bpp (`bppMode` 0), `otIndex` 100 :

| Boîte | packedU / packedV | Taille | Offset écran | CLUT |
|---|---|---|---|---|
| `g_uiBox0[0]` | 0x3C0 / 0x00 | 0xA0×0x40 | (0, 0) | (0x110, 0x1E0) |
| `g_uiBox0[1]` | 0x3C0 / 0x40 | 0xA0×0x40 | (0xA0, 0) | (0x110, 0x1E0) |
| `g_uiBox0[2]` | 0x3C0 / 0x80 | 0xA0×0x40 | (0, 0) | (0x110, 0x1E0) |
| `g_uiBox0[3]` | 0x3C0 / 0xC0 | 0xA0×0x40 | (0xA0, 0) | (0x110, 0x1E0) |
| `g_uiBox0[4]` | 0x3F0 / 0x00 | 0x40×0x20 | (0, 0) | (0x110, 0x1E0) |
| `g_uiBox0[5]` | 0x3F0 / 0x20 | 0x40×0x20 | (0x40, 0) | (0x110, 0x1E0) |
| `g_textBox1` | 0x2C0 / 0x180 | 0x100×0x30 | (0x20, 8) | (0x100, 0x1E1) |
| `g_textBox2` | 0x2C0 / 0x1B0 | 0x100×0x20 | (0x20, 0x10) | (0x100, 0x1E1) |
| `g_textBox3` | 0x2C0 / 0x1D0 | 0x60×0x10 | (0x10, 8) | (0x100, 0x1E1) |
| `g_saveSlotBox` | 0x2C0 / 0x100 | 0x10×0x10 | base (0xAF, 0x85), otIndex 0x78 | (0x100, 0x1E1) |

> Les trois `g_textBoxN` échantillonnent **exactement** les rectangles VRAM où `g_tileMapN` dépose
> son contenu : ce sont les couches de texte, rendues dynamiquement en VRAM puis affichées par les
> `UIBox`.
>
> ⚠️ **CORRECTION.** Cette section affirmait que les entrées ETC 0xCA et 0xCB fournissaient « les
> sprites du curseur via `SetSpriteImage` ». C'est faux sur les deux points : `SetSpriteImage` a été
> renommée `DrawTextToLayer` (elle dessine du **texte**, §6.5), et 0xCA / 0xCB sont deux **chaînes**
> — « OUI » et « NON » sur la version France — écrites côte à côte, espacées de 0x30, dans
> `g_tileMap3`. Le curseur qui les désigne est `g_saveSlotBox`, dont `ValidateSelection` pose
> `baseX = choix * 0x30 + 0xBF`.

Deux téléversements supplémentaires complètent la carte, relevés hors de
`InitializeSelectionScreenGraphics` :

| Couche | Source | Image VRAM | CLUT | Posée par |
|---|---|---|---|---|
| décor (salle) | `GetEtcResource("TIM", 1)` 320×384 8bpp | (0x300, 0) | (0, 0x1E0) | `InitSelectionBackdropTiles` |
| brume | `GetEtcResource("TIM", 2)` 320×128 4bpp | (0x300, 0x180) | (0x100, 0x1E0) | `InitSelectionBackdropTiles` |
| planche de sprites | `GetEtcResource("TIM", 5)` 256×256 8bpp | (0x280, 0) | (0, 0x1E1) | `InitSaveSlotSelectionUI` |
| fonte | `GetEtcResource("TIM", 4)` 256×256 4bpp | (0x2C0, 0x100) | (0x100, 0x1E1) | `InitFontTileMap` |

> Le rectangle de la fonte et ceux des trois `g_tileMapN` **se recouvrent volontairement** : la fonte
> occupe les lignes 0x100..0x1FF et les tampons de texte les lignes 0x180..0x1DF. La moitié basse de
> la planche de fonte est donc écrasée à chaque téléversement de texte, ce qui est sans effet
> puisque les glyphes utilisés sont dans la moitié haute. Aucune CLUT n'entre en collision avec une
> autre — vérifié rectangle par rectangle, écran-titre et écran de boot compris.

#### `SetTextLayer` (0x800221d4) — ce n'est **pas** le rasteriseur

Contrairement à ce que le nom laisse croire, la fonction ne dessine rien. C'est un simple
initialisateur d'état qui **réemploie la structure `UIBox` de 36 octets comme descripteur de couche
de texte** :

| Champ `UIBox` | Usage dans un descripteur de texte |
|---|---|
| `baseX` / `baseY` | les deux moitiés du `modeAndPalette` 32 bits |
| `otIndex` + `flatColorR/G` (les 4 octets à +4) | **pointeur** vers la `UIBox` d'affichage liée |
| tout le reste | mis à zéro, `bppMode = 2` |

Les trois appels passent `0x8014EC80`, `0x8014ECB0` et `0x8014ECE0` — espacés de 0x30, ce sont les
tampons de texte.

> ⚠️ Conséquence pour le portage : le champ à +4 contient un **pointeur**, pas un `otIndex`. La
> classe `UiBox` actuelle ne peut donc pas servir telle quelle de descripteur de texte ; il faudra
> un type distinct plutôt que de réemployer la structure comme le fait l'original.

Le rasteriseur lui-même est décrit au §6.5.


#### Flux exact

`RunLoaderMainSequence` (0x80024e28), appelée par `MainLoop` quand l'entrée 1 est validée :

```
InitSelectionMenu()          0x8002471c   lit la carte, joue SFX 4, attend, joue SFX 5
ShowSelectionScreen()        0x800247fc   BGM piste 0x29
UpdateSelectionCursor()      0x80024888
boucle:
    result = GetUserInput()  0x80024984   6 = annuler
    result = ValidateSelection(result)    0x80024b10
tant que result == -1
ProcessSelection(result)     0x80024a54
retourne l'index d'emplacement, ou -1 si annulé
```

`MainLoop` recopie ensuite `g_saveSlotRecords + index * 0x76C` dans `g_saveDataInRam`, pose
`SlotData = 1` et `LastMapId = index`, puis enchaîne sur `LoadExec("cdrom:\ALUN_CD.EXE;1")`.

#### Lecture de la carte

`InitSelectionMenu` → `HandleResourceLoadFailureOrFallback` (0x80024560) →
**`ParseAndLoadResourceScript` (0x80021ee8)** : c'est le lecteur de carte proprement dit. Ses codes
de retour se traduisent en messages ETC :

| Code | Message ETC | Sens probable |
|---|---|---|
| -1 | `0xC2` | pas de carte |
| -2 | `0xC3` | carte non formatée |
| -3 | `0xC5` | — |
| -4 / -5 | `0xC6` | — |
| succès, ≥1 emplacement occupé | `0xC1` | liste des sauvegardes |
| succès, aucun occupé | `0xC8` | aucune sauvegarde |

#### Format des emplacements — **fermé**

`g_saveSlotRecords` : **4 emplacements de 0x76C octets**, drapeau d'occupation à **+8**.
Déduit des quatre tests de `HandleResourceLoadFailureOrFallback` : offsets 8, 0x774, 0xEE0, 0x164C
— écarts constants de 0x76C. Chaque emplacement occupé allume aussi `g_slotError4..1`
(ordre inversé : l'emplacement 0 renseigne `g_slotError4`).

#### ⚠️ `CORRECTION` — le drapeau d'occupation ne peut pas être transliteré littéralement

Symptôme : « Continuer » répondait **« Aucune donnée Alundra enregistrée »** (entrée ETC `0xC8`) alors
que plusieurs sauvegardes existaient sur le disque.

Deux causes indépendantes, toutes deux dans l'énumération des emplacements :

1. **`CurrentFlagName` n'est jamais écrit par ce portage.** Le champ est déclaré dans `SaveData`,
   recopié par `CopyFrom` et lu par `MemoryCardManager` — mais **aucune ligne du dépôt ne lui affecte
   de valeur**. Il vaut donc `""` dans toute sauvegarde produite par le jeu. Le test d'occupation
   `g_saveSlotRecords[slot * 0x76C + 8] != 0`, transliteré au pied de la lettre, rejetait donc
   systématiquement les quatre emplacements.

   Ce que l'original demande réellement, c'est « ce bloc de 0x76C octets a-t-il été rempli ? » —
   `LoadSaveSlotsAndPickMessage` met 0x2000 octets à zéro avant la lecture, donc n'importe quel octet
   non nul signifie « la carte a écrit ici ». Sur desktop, l'équivalent fidèle est
   **« un fichier de sauvegarde s'est chargé »**, et c'est désormais le critère.

2. **Seule la carte 0 était lue.** `g_memorySlotId` vaut `0x270F` par défaut, que
   `MemoryCardManager.DesktopMemoryCardDirectory` ramène à **1** : par défaut le jeu écrit dans
   `saves/card1`. Les deux cartes sont maintenant parcourues, carte 0 puis carte 1.

Conséquence sur le nom de chapitre : les quatre chiffres ASCII en tête de `CurrentFlagName` sont un
index dans `ETC_RES.R` désignant le **chapitre** (entrée 0 « Un Nouveau Départ », 1 « Wendell
Succombe », 2 « Fuite vers le Manoir de Tarn »…). Le champ étant vide, l'index vaut `-1` et la ligne
de chapitre est laissée vide. `PARTIAL` assumé : retomber sur 0 étiquetterait **toutes** les
sauvegardes « Un Nouveau Départ », ce qui serait pire que de ne rien dire. Écrire ces quatre chiffres
au moment de la sauvegarde suffirait à rallumer la ligne.

Le résumé affiché reste, lui, correct : c'est `GameStateDescription` (+0x28), que le jeu remplit avec
sa ligne de HUD — « HP 13   TIME 00:03:11 ».

**Vérifié**, six dispositions de répertoires, après avoir d'abord **reproduit la panne** avec des
sauvegardes de forme réaliste (`CurrentFlagName` vide) :

| Disposition | Résultat | Occupés |
|---|---|---|
| aucune carte | -1 → « Insère une Carte Mémoire » | 0 |
| carte 0 seule, 2 sauvegardes | 0 | 2 |
| **carte 1 seule** (défaut de `g_memorySlotId`), 3 | 0 | 3 |
| les deux cartes, 2 + 2 | 0 | 4 |
| carte 0 absente, carte 1 avec 1 | 0 | 1 |
| 6 sauvegardes pour 4 emplacements | 0 | 4 (les 4 premières) |

Avant correctif, la deuxième ligne comme la cinquième donnaient `occupied = 0` et le message `0xC8`.

#### L'élément qui rend le portage faisable

`AlundraEngine/UI/MemoryCardManager.cs` possède **déjà un backend de carte mémoire desktop** :

* répertoire `{BaseDirectory}/saves/card{0|1}` ;
* fichiers `{titre}-{NN}.json`, 4 au maximum par carte ;
* `SaveData.LoadFromJson(...)` reconstruit l'objet de sauvegarde, `SlotData = 1` marquant l'occupation.

Le loader n'a donc pas à réimplémenter d'accès carte : il lui suffit d'énumérer ce répertoire pour
remplir ses 4 emplacements, et de poser l'objet choisi dans `g_saveDataInRam`. Les codes d'erreur
-1..-5 se réduisent alors à « répertoire absent » / « aucun fichier valide ».

#### Portage — fait ✅

`LoaderSelectionScreen` porte l'ensemble ; voir §6.6 pour la machine à états et les corrections que
le portage a imposées.

### 6.4 Historique graphique

1. ~~**Conteneur « ANM »**~~ — ✅ **fait**. Voir §6.2b pour le format du conteneur.
   `AdvanceTitleAnimation` reproduit `FUN_80021a1c` : maintien de 9 frames par couche
   (`INT_80042f68` compte jusqu'à 8), index qui **reboucle de 8 vers 3** et non vers 0 — le premier
   passage parcourt les huit frames en fioriture d'ouverture, puis l'animation s'installe sur une
   boucle de cinq. Téléversement en VRAM (0x180, 0), CLUT (0, 0x1E3).

   Vérifié sur le rendu réel : entre deux frames espacées d'une couche, **17 000 à 18 500 pixels
   changent dans la zone du logo (y < 160) et exactement 0 dans la zone menu + copyright
   (y ≥ 160)** — les frames font 320×160 et n'écrasent donc que le haut du rectangle de
   `g_TitleFull`, qui fait 320×240. Le sous-titre katakana, absent du rendu statique, apparaît.

   > Défaut corrigé au passage : `LoaderUiRenderer` détruisait ses `Bitmap` de sprite à chaque
   > changement de VRAM, alors que `AlundraRenderer` les conserve comme **clés** de son cache de
   > textures — il restait donc des entrées pointant sur des bitmaps détruits. Les sprites sont
   > désormais conservés et **remplis sur place**, avec un compteur de génération VRAM pour savoir
   > lesquels re-échantillonner. Sans l'animation, ce défaut serait resté invisible.
2. ~~**Sélection de slot**~~ — ✅ **fait**, §6.6.
3. ~~**Écran de sélection**~~ — ✅ **fait**, §6.6.
4. **Chemin sprite pivoté de `RenderUIBox`** — ✅ **fait**, et la raison invoquée pour le laisser
   `BLOCKED` était fausse : voir la `CORRECTION` du §6.6.
5. ~~**Écran de chargement de boot**~~ — ✅ **fait**, §6.7.

---

## 6.5 Le moteur de texte — **fermé** ✅

C'était le verrou de la phase. Le rasteriseur n'était pas caché : il était **mal nommé**.

### La chaîne complète

```
AdvanceTextLayerTypewriter (0x800227ac)   une frame : défilement, délai, ou un caractère
DrawTextToLayer            (0x80022518)   une chaîne entière d'un coup
   └── DrawGlyphToTileMap  (0x800223ec)   un glyphe
         ├── code < 0x100 : BlitTileMapTransparent (0x80026c10) depuis la planche de fonte
         └── code ≥ 0x100 : DrawKanjiGlyphToTileMap (0x800221fc)  ← BLOCKED, kanji ROM du BIOS
BlitTileMapTransparent → GetTileMapPixel (0x80026958) + SetTileMapPixel (0x800269e8)
```

### Cinq noms Ghidra corrigés

| Adresse | Ancien nom | Nouveau nom | Pourquoi l'ancien était faux |
|---|---|---|---|
| `0x800269e8` | `ClearTile` | `SetTileMapPixel` | écrit **un pixel de valeur arbitraire** ; « clear » venait des 3 appelants qui passent 0, les 8 autres passent 6, 3 ou une valeur lue ailleurs |
| `0x80022518` | `SetSpriteImage` | `DrawTextToLayer` | parcourt une chaîne octet par octet et n'appelle que le dessin de glyphe |
| `0x80022160` | `ResetGraphicsState` | `InitFontTileMap` | ne remet rien à zéro : charge `g_loadRoomFontTim` et le téléverse |
| `0x80022a20` | `SetupTransitionLayer` | `ClearTextLayer` | efface la surface d'une couche de texte, aucun rapport avec une transition |
| `0x80024560` | `HandleResourceLoadFailureOrFallback` | `LoadSaveSlotsAndPickMessage` | lit la carte et choisit le message ; l'échec n'est qu'une de ses branches |

Renommées aussi : `FUN_80026958` → `GetTileMapPixel`, `FUN_800223ec` → `DrawGlyphToTileMap`,
`FUN_800227ac` → `AdvanceTextLayerTypewriter`, `FUN_800221fc` → `DrawKanjiGlyphToTileMap`,
`FUN_80026c10` / `FUN_80026b0c` → `BlitTileMap{Transparent,Opaque}`,
`FUN_80026e20` → `ScrollTileMapUpOneLine`, `FUN_8002274c` → `ScrollTextLayerOneLine`,
`FUN_800239c4` → `MoveCameraAndTestHotspots`. Chacune porte un commentaire de plaque avec ses preuves.

### Deux champs de `TileMap` mal nommés

`SetTileMapPixel` les tranche : `imageHPixels` (+0x24) est la **largeur** en pixels — elle borne x
et sert de pas de ligne — et `imageWPixels` (+0x20) est le **pointeur de pixels**, pas une largeur.

### Le descripteur de couche de texte

`SetTextLayer` (0x800221d4) réemploie la structure `UIBox` ; le §6.3 en donnait deux champs, voici
les huit (le descripteur fait **0x20** octets, pas 0x24 — les trois sont à `g_uiSlot1`,
`g_uiSlot1 + 0x20` et `g_uiSlot3`) :

| Offset | Rôle réel | Champ `UIBox` réemployé |
|---|---|---|
| +0x00 | `TileMap *` de la surface à rastériser | `baseX` / `baseY` |
| +0x04 | `UIBox *` de la boîte qui l'affiche | `otIndex` + `r` + `g` |
| +0x08 | curseur X en pixels | `rotationZ` |
| +0x0C | curseur Y en pixels | `r`/`g`/`b`/`pad0F` |
| +0x10 | pointeur sur la chaîne en cours de frappe | `bppMode` / `abrOrMinus1` |
| +0x14 | valeur de rechargement du délai par caractère (0) | `packedU` / `packedV` |
| +0x18 | frames restantes avant le prochain caractère | `width` / `height` |
| +0x1C | lignes restantes à faire défiler | `clutX_raw` / `clutY` |

> ⚠️ Le §6.3 disait que le champ +0x04 pointe sur la `UIBox` liée. C'est exact, mais le champ **+0x00**
> — que l'on aurait pu lire comme deux coordonnées — est un **`TileMap *`** : les trois appels
> passent `0x8014EC80`, `0x8014ECB0`, `0x8014ECE0`, qui sont `&g_tileMap1/2/3` et non « les tampons
> de texte » comme le supposait la note d'origine.

### La fonte

`g_characterPositionInSpriteSheet` @ `0x80042f80` : 256 entrées de 20 octets. L'ordre des champs de
`FontCharacter` dans Ghidra ne suit pas l'ordre mémoire ; par offset :
**+0 largeur d'avance (= largeur de blit), +4 hauteur, +8 x source, +12 y source, +16 décalage y**.
La planche est une grille 16×16 : entrée `c` est en `(c % 16 * 16, c / 16 * 16)`, largeur variable.
Les entrées 0..15 sont les cellules 16×16 de la première ligne — c'est là que
`UpdateBackgroundState` prend les quatre frames du curseur animé.

`InitFontTileMap` force l'entrée 0 de la palette à `0x0000` pour la rendre transparente.

### Le balisage des chaînes

`\N` et `\A` retour à la ligne (curseur Y += 16), `\W` + chiffre hexa → glyphe `0x10`..`0x1F`,
`{` + octet → code `octet + 0x50`, `}` + octet → code `octet + 0x90`, octet ≥ 0x80 → code deux
octets gros-boutiste (chemin kanji), sinon l'octet lui-même. Tout autre `\` + octet est consommé et
ignoré.

C'est ainsi que le français passe dans une planche de 256 glyphes : `D}Ypart` = `D` + (`}` + `Y`
= 0x59 + 0x90 = 0xE9 = `é`) + `part`.

### Le chemin kanji — `BLOCKED`

`DrawKanjiGlyphToTileMap` appelle `Krom2RawAdd2`, l'aide PsyQ qui résout un code Shift-JIS en motif
1 bit 16×15 dans la **ROM kanji du BIOS de la console**. Cette ROM n'est ni dans `LOADER.EXE`, ni sur
le disque, ni accessible depuis l'émulateur ici. Le reste de la fonction est transliterée et
s'exécute dès qu'une source de motifs est fournie (`LoaderTextLayer.KanjiPatternProvider`) : le motif
est étendu en grille 16×16, puis chaque bit allumé écrit l'index 6 en (x, y) et l'index 3 sur
(x+1, y), (x, y+1) et (x+1, y+1) — une ombre portée d'un pixel, dans cet ordre pour que chaque pixel
de glyphe recouvre l'ombre posée par ses voisins gauche et haut.

**Ce chemin est inatteignable sur la version France** : mesuré sur les 1024 entrées de `ETC_RES.R`,
**0 octet ≥ 0x80** et 180 échappements `{` / `}`.

### Vérification

Chaîne `ETC` 0xC1 rendue dans `g_tileMap1` puis relue dans le rectangle VRAM (0x2C0, 0x180) de
0x100×0x30 :

| Contrôle | Résultat |
|---|---|
| Chaîne rendue | `Utilisation de la Carte\NM}Ymoire . . .` → « Utilisation de la Carte / Mémoire . . . » |
| Curseur final | X = 66, Y = 16 (le `\N` a bien avancé d'une ligne de 16) |
| Pixels posés | 841 / 12288 ; histogramme d'index `6:838, 2:3` |
| Mots VRAM changés | 389 / 3072 |
| **Région témoin** (fonte, lignes 0x100..0x17F) | **0 mot changé** |
| Typewriter vs rendu direct | 841 pixels dans les deux cas, en 37 frames |

---

## 6.6 Écran de sélection de sauvegarde — **fait** ✅

Ce n'est pas une liste d'emplacements : c'est une **petite salle qu'on parcourt**. Le décor est la
*load room* (une bibliothèque), les quatre sauvegardes sont quatre pupitres, et l'on y amène un
personnage.

### `MoveCameraAndTestHotspots` (0x800239c4) — le cœur

La fonction déplace une **copie** de la caméra (`g_cameraStartX/Y`, virgule fixe 20.12), teste cette
copie contre `g_selectionHotspots` @ `0x800443b0`, et **retourne le code sans valider le
déplacement** en cas de collision. C'est tout le système de collision.

Table lue (11 enregistrements de 5 `short` : code, x, y, w, h ; terminateur `-1`) :

| Code | Rectangle | Sens |
|---|---|---|
| 0..3 | 16×44 en x = 0x50 / 0x80 / 0xB0 / 0xE0, y = 0x78 | les quatre pupitres |
| 6 | 320×16 en y = 0xF0 | la sortie |
| 0x64..0x69 | six rectangles | des **murs** |

`ValidateSelection` ne retient que 0..3, donc heurter un mur ne fait que bloquer la marche.
Pas de 0x1800 par frame en vertical, 0x2000 en horizontal (1,5 et 2 pixels).

### Quatre globales mal nommées

Le nom d'origine induit en erreur sur chacune ; le portage les renomme et l'annotation `CORRECTION`
donne la correspondance :

| Ghidra | Rôle réel |
|---|---|
| `g_selectedSlot` | **niveau de fondu** de l'écran (positif = quad additif blanc, négatif = quad soustractif noir) |
| `g_transitionState` | drapeau **repos** du marcheur : 1 quand aucune direction n'est tenue, et c'est lui qui autorise le déplacement |
| `g_fadeInProgress` | **orientation** du marcheur (0 bas, 1 haut, 2 gauche, 3 droite) |
| `g_fadeType` | **code de hotspot** renvoyé au dernier test, -1 si aucun |

### `CORRECTION` — le chemin sprite pivoté n'était pas mort

Le §6.4 le déclarait `BLOCKED` au motif que « aucun élément du loader ne définit de rotation, tous
passent -1 ». C'est faux : `InitSaveSlotSelectionUI` donne la rotation 0 à `UIBox_ARRAY_8014f390`
(l'ombre du marcheur) et `UpdateMenuGraphics` fait tourner `UIBox_ARRAY_8014f548[1..3]` chaque frame
(pas de -0x16 et +0xB, repliés à 0x1000). Le chemin est donc porté : `POLY_FT4` dont les quatre coins
`(±(w-1)/2, ±(h-1)/2)` passent par une rotation Z en virgule fixe 1.12 (4096 = tour complet) puis
sont translatés au centre de la boîte, rendu via `IRenderer.DrawDeformedQuad`.

#### ⚠️ `CORRECTION` — les UV de `DrawDeformedQuad` sont **normalisées**

Symptômes : un trait parasite à gauche de l'ombre du marcheur, et les rayons arc-en-ciel autour du
globe du sage réduits à quelques points colorés au lieu de faisceaux.

Les deux venaient du même endroit. `IRenderer.DrawDeformedQuad` attend des coordonnées de texture
**normalisées entre 0 et 1** — les deux backends le prouvent : `GraphicManager` passe
`img.Swidth / (float)bitmap.Width` et `AlundraRenderer` les injecte directement dans
`Vector2(u, v)`. Le premier jet passait les **spans en pixels** (`w-1`, `h-1`), donc chaque élément
pivoté échantillonnait sa texture des dizaines de fois de suite : 23× pour l'ombre 24×16, 79× pour un
rayon 80×8.

L'étendue correcte est `(w-1)/w`, pas 1 : le `POLY_FT4` d'origine couvre `u..u+(w-1)` sur un quad
lui-même large de `w-1`, si bien que le texel *i* tombe exactement sur le pixel *i*.

> Pourquoi la vérification initiale ne l'a pas vu : elle comptait les pixels opaques du sprite
> (« l'ombre dessine 254 px »), ce qui reste vrai quand la géométrie part en vrille. Et le
> `Renderer` logiciel passe par `Graphics.DrawImage(bitmap, 3 points, srcRect)`, qui absorbe
> silencieusement un `srcRect` hors bornes.

**Invariant ajouté**, et c'est lui qu'il fallait mesurer dès le départ : **à `rotationZ = 0`, le
chemin pivoté doit dessiner au même endroit que le chemin sprite**, une rotation nulle étant
l'identité. La comparaison porte sur la boîte englobante et le centroïde pondéré, pas sur les pixels
— sinon on mesure le lissage bilinéaire de GDI+ et non la géométrie.

| Élément | Écart de bord | Dérive du centroïde |
|---|---|---|
| ombre 24×16 | 2 px | 0,70 px |
| rayon 80×8 | 2 px | 0,56 px |
| étoile 51×49 | 1 px | 0,04 px |

(Le pixel d'écart de bord est structurel : le `POLY_FT4` couvre `w-1` là où le `SPRT` couvre `w`.)

**Contrôle négatif** — avec les UV en pixels rétablies, le sprite pivoté s'effondre en boîte
englobante **0×0** et l'écart de bord monte à 22, 79 et 50 px. Le test échoue donc bien sur le défaut
qu'il est censé attraper.

### La machine à états

`RunLoaderMainSequence` (0x80024e28) est une pile de boucles bloquantes ; `LoaderSelectionScreen`
l'exprime en phases, une itération par frame hôte :

```
CardMessage      LoadSaveSlotsAndPickMessage + message ETC, attend Rond (0x20)
CardMessageHold  0x3C frames
FadeIn           ShowSelectionScreen : BGM 0x29, niveau -0xFF → 0 par pas de 8
Prompt           UpdateSelectionCursor : message ETC 199, attend Croix (0x40)
PromptHold       0x3C frames
Walking          GetUserInput → MoveCameraAndTestHotspots
Confirm          ValidateSelection : résumé de la sauvegarde, OUI / NON, attend Croix
FadeOut          ProcessSelection : niveau 0 → 0xFF par pas de 3
Settle           2 frames à vide, puis le résultat
```

`InitSelectionMenu` n'appelle **que** `UpdateBackgroundState` — ni `UpdateFadeState`, ni
`UpdateMenuGraphics` : le premier message s'affiche donc sur fond noir, décor éteint. C'est
conforme, et c'est ce que montre la capture de vérification.

### Passage de main au jeu — **branché** ✅

Sur console, `MainLoop` (0x8002538c) recopie l'enregistrement choisi dans `g_saveDataInRam`, pose
`SlotData = 1` et `LastMapId = index`, puis appelle `LoadExec("cdrom:\ALUN_CD.EXE;1")`. `LoadExec`
**n'efface pas** ce bloc : c'est tout le mécanisme. `ALUN_CD.EXE` démarre ensuite et son
`InitializeGameState` (0x80031700) le relit pour choisir entre nouvelle partie et partie chargée.

Ici les deux exécutables sont un seul processus, et `GameEngine.InitializeEngine` tourne **à la
construction** — avant le loader, parce que le pilote son doit être vivant pendant qu'il joue les
vidéos et la musique du titre. `InitializeGameState` s'exécutait donc *avant* le loader au lieu
d'après.

`GameEngine.ApplyLoaderSelection()` rétablit l'ordre d'origine pour **la seule moitié qui dépend de
la sauvegarde** : il relance `InitializeGameState()` puis repose
`g_currentMap = ~g_desiredMap` — l'affectation que `GameInitializer.Initialize` fait juste après le
même appel, et qui force le chargement de la carte à la première frame. Les chargements de
ressources qui l'encadrent (sprites, fichier de carte, fonte, son, moteur de défilement) ne lisent
pas la sauvegarde et ne sont **pas** rejoués.

`AlundraGame` fait la recopie puis l'appel au moment où le loader rend `GameState.Game`.

**Vérifié** sur un `GameEngine` réel piloté sans fenêtre :

| Contrôle | Résultat |
|---|---|
| Après `InitializeEngine` | `SlotData=0`, `desiredMap=389` (0x185), caméra de nouvelle partie, `time=0` |
| Après `ApplyLoaderSelection` (`SlotData=1`) | `desiredMap=416` = `InitialMapId` de la sauvegarde |
| Caméra | dérivée des tuiles de la sauvegarde, formule identique à l'originale |
| `g_gameplayTime` | repris de la sauvegarde (123456) |
| `g_currentMap == ~g_desiredMap` | vrai — le chargement de carte se déclenchera |
| Retour à `SlotData=0` puis rappel | revient **exactement** à l'état de nouvelle partie (carte et caméra) |

#### `--game-state-file` — court-circuit de mise au point

`StaticVariables.GameStateFileNameToLoad` l'emporte à l'intérieur de `InitializeGameState`. Ce n'est
plus un conflit : quand l'option est passée, `AlundraGame` démarre directement en
`GameState.InGame` et **ne lance jamais le loader**, donc les deux ne peuvent pas se contredire. Rien
d'autre n'est nécessaire — `GameEngine.InitializeEngine` a déjà chargé le fichier dans
`g_saveDataInRam` et forcé `SlotData = 1`, si bien que le monde est déjà debout sur cette sauvegarde.

C'est un point d'entrée de développement ; l'original n'a pas d'équivalent, `ALUN_CD.EXE` n'étant
jamais atteint autrement que par `LOADER.EXE`.

**Vérifié** avec un fichier de sauvegarde sur disque portant `SlotData = 0` volontairement :

| Contrôle | Résultat |
|---|---|
| Sauvegarde chargée depuis le fichier | OK (`GameStateDescription` retrouvée) |
| `SlotData` forcé à 1 | OK |
| `desiredMap` = `InitialMapId` du fichier (452) | OK |
| Caméra et `g_gameplayTime` issus du fichier | OK |
| `g_currentMap == ~g_desiredMap` | OK |
| Un passage de main du loader ne l'écrase pas | OK — d'où le court-circuit |

### Résumé d'une sauvegarde

`ValidateSelection` lit l'enregistrement de 0x76C octets, qui est le blob `SaveData` du jeu :
les quatre chiffres ASCII en tête de `CurrentFlagName` (+8) forment un **index dans `ETC_RES.R`**
donnant le nom du lieu, et `GameStateDescription` (+0x28, 0x20 octets) est le nom de la partie. Les
deux sont écrits dans `g_tileMap2`, l'un en y = 0, l'autre en y = 0x10.

### Marqueurs de pupitre

`g_slotError4..1` (déclarées à l'envers : l'indice 0 est l'emplacement 0) sont des **pointeurs de
chaîne** consommés un caractère par frame par `UpdateMenuGraphics`, chaque chiffre choisissant
`packedU = (c - 0x30) * 0xC + 0x280`. `null` = pas de sauvegarde, rien n'est dessiné.
`"0"` (`0x80044380`) = image fixe. La chaîne d'animation (`0x80044384`) est
`0123456` suivi de `3456` répété : une ouverture puis une boucle. Arrivé au terminateur, le curseur
**recule d'un caractère**, ce qui fige le marqueur sur sa dernière image — et c'est précisément ce
que fait `ValidateSelection` en le parquant à `+0x24`, l'octet nul de cette chaîne.

### `ETC_RES.R`

`GetEtcResourceEntry` (0x80025184) est une ligne : `base + ((u16 *)base)[index]`. Le fichier
`DATA/ETC_RES.R` commence donc par un index de 16 bits relatifs à lui-même — 0x800 octets, soit
**1024 entrées** sur la version France, la première pointant sur 0x0801.

> Le tampon est partagé par toutes les entrées plutôt que découpé par chaîne, **volontairement** :
> le bip de frappe est conditionné à la parité de l'**adresse** du caractère
> (`if (((uint)p & 1) != 0) PlaySoundEffect(0x4f)`), et découper changerait cette parité.

### Vérification

Écran piloté par le `Renderer` logiciel, 821 frames, entrées scriptées (Rond, Croix, Haut, Gauche) :

| Contrôle | Résultat |
|---|---|
| Enchaînement des phases | `CardMessage → …→ Confirm → FadeOut → Settle → Finished`, aux compteurs attendus (0x3C, 32, 85, 2) |
| Résultat | emplacement 1, `SaveData` « SLOT 1 TEST » |
| Effets sonores, dans l'ordre | 4, 5, 0x193, 0x194, 0xCC, 2 — plus 130 bips de frappe |
| BGM | 0x29 lancée, arrêtée à `ProcessSelection` |
| Marche : écran entier entre deux instants | 26 174 pixels diffèrent, delta max 223 |
| Marche : **région témoin** (40 lignes hautes du décor, ni brume ni marcheur) | **0 pixel** |
| Défilement de la brume | 10 pixels en 40 frames = 1 pixel toutes les 4 frames, conforme à `g_loaderState1 == 4` |

### Deux défauts trouvés par la mesure

1. **Débordement de profondeur.** `LoaderUiRenderer.DepthBase` valait `SpriteDepth.BackgroundUI`,
   c'est-à-dire `int.MaxValue - 4` : `DepthBase + otIndex` débordait dès l'indice 5 et ressortait en
   profondeur très négative, donc **derrière tout**. L'écran-titre n'utilise que les indices 0 et 1,
   ce qui a masqué le défaut ; l'écran de sélection va de 0 à 200 et son décor de premier plan, ses
   ornements, ses panneaux et son texte passaient tous sous le décor. Base ramenée à
   `BackgroundUI - 0x100`.
2. **Règle de transparence fausse.** `PsxVram.ReadSprite` testait l'**index** de CLUT contre 0. La
   vraie règle PSX est qu'un texel est transparent quand sa **couleur 15 bits** vaut `0x0000`, après
   la conversion par la CLUT. Le blit logiciel du jeu lui-même tranche :
   `BlitTileMapTransparent` fait `if (clut[index] != 0)`. Le décor de la *load room* code ses zones
   transparentes sur un index non nul dont l'entrée de palette est nulle : avec l'ancienne règle la
   bande de premier plan sortait entièrement opaque et barrait l'écran de noir.

---

## 6.7 Écran de chargement de boot — **fait** ✅

`SkipBootLoadingScreen` est repassé à `false`.

**`CORRECTION`** : le §5.3 supposait que la bonne image était `g_loadingScreenTim`. Elle ne l'est pas.
`RunLoadingScreenIntro` demande `GetEtcResource(g_loadRoomBackgroundTimPtr, "TIM", 7)`, et la
septième entrée `TIM` du conteneur est la charge utile **#6**, `g_licenceScreenTim` (256×256, 4 bpp) —
l'écran d'avertissement anti-piratage. Il part en VRAM (0x180, 0x100), CLUT (0, 0x1E5), et s'affiche
en (0x20, -8).

Le fondu est un `POLY_F4` plein écran en `abr = 2` (**soustractif**), dont la couleur est
`g_cursorFadeLevel` : 0xFF (écran noir) → 0 par pas de 2, maintien 500 ticks, puis retour à 0xFF.

> Un défaut de l'instrument de mesure est apparu ici : le `Renderer` logiciel ignorait
> `BlendMode.Additive` et `BlendMode.Subtractive` et dessinait le quad **opaque**, ce qui donnait un
> écran noir permanent. Le backend MonoGame les implémente correctement depuis toujours ; le
> renderer logiciel les compose maintenant pixel à pixel dans le tampon de trame. Sans ce correctif,
> aucune mesure de fondu — ni ici, ni sur l'écran de sélection — n'aurait dit la vérité.

**Vérifié** — luminance moyenne par frame : 0,0 → 0,7 (f20) → 4,8 (f60) → 18,6 (f130) → 18,6 (f400,
maintien) → 10,8 (f660) → 4,0 (f700) → 0,0 (f754). Profil 128 + 500 + 128 conforme.

L'écran-titre a été recontrôlé après le changement de règle de transparence : 76 800 / 76 800 texels
opaques, rendu identique.

---

## 6.8 END.EXE — la vidéo de fin, dans `ClosingEngine` ✅

La séquence de fin est **deux exécutables** sur console, chaînés par un `LoadExec` :
`END.EXE` joue `\MOVIE\ARAN_END.MOV`, puis passe la main à `CLOSING.EXE` qui déroule le générique.
`ClosingEngine` les réunit en une seule machine à états — il n'y a pas de `LoadExec` ici, la vidéo
est simplement l'état qui précède le générique.

`main` de END.EXE (0x80021304) tient en une ligne utile :

```c
MainLoop(0, 0x28, &g_ARAN_END_MOV_fileInfo, 0x19c5, 0x800, 0);
...
LoadExec("cdrom:\\CLOSING.EXE;1", &DAT_801fff00, 0);
```

Et `MainLoop` @ 0x80023ce8 **est** le lecteur de LOADER.EXE (`PlayMovie` @ 0x80027ff4) : mêmes six
paramètres, même test de saut
`(skipAfterFrame < curFrame) && ((buttons | mask) == mask) && ((buttons & mask) != 0)`, même rampe
sonore sur les 15 dernières frames, même chien de garde sur le recul du numéro de frame. `PsxSdk`
couvre donc la fonction telle quelle ; seuls les paramètres changent.

| Paramètre | Valeur | Sens |
|---|---|---|
| `param_1` / `param_2` | 0 / 0x28 | position écran — 320×160 centré verticalement, comme EURO_OP |
| `param_3` | `&DAT_8005b890` | `CdlFILE` rempli au démarrage depuis `"\MOVIE\ARAN_END.MOV;1"` (0x80020000) |
| `param_4` | `0x19C5` = 6597 | limite de frames, contre **6602** réelles → 5 coupées |
| `param_5` | `0x800` | masque de saut = **Start seul**, pas Croix |
| `param_6` | 0 | sautable dès la première frame, contrairement à EURO_OP (0x136) |

`DELIBERATE DEVIATION`, identique au §4.7 et pour la même raison : `StopAtLastFrame` reste `false`,
la vidéo va jusqu'à sa fin naturelle au lieu d'être coupée 5 frames trop tôt.

Le puits audio (`IMovieAudioOutput`) est celui du loader, réutilisé : les deux ne jouent jamais en
même temps, le loader étant terminé depuis longtemps quand le générique arrive. `MovieFrameBitmap`
et `IMovieAudioOutput` ne vivent sous `Loader/` que parce que le loader en a eu besoin le premier —
ils ne contiennent rien de spécifique au loader.

**Vérifié** sur 400 frames hôte, en observant la **liste de tracés** plutôt que l'état interne (le
lecteur dessine un unique sprite 320×160 en (0, 0x28), le générique dessine des quads déformés) :

| Contrôle | Résultat |
|---|---|
| Sans bouton | 396/400 frames dessinées, la première à l'indice 4 — l'écart est bien au démarrage (4 + 396 = 400), le flux étant à 15 fps contre 60 Hz |
| L'image bouge | 51 190 pixels changent entre la frame 100 et la 400 |
| **Croix maintenue** | 396/400 — identique à la référence, donc Croix **ne saute pas** ✓ masque 0x800 |
| **Start maintenu** | 125/400 — saut effectif, puis le générique |

Le contrôle Croix/Start est celui qui compte : c'est la seule chose qui distingue ce masque de celui
du loader, et une erreur y serait invisible sans le mesurer.

---

## 7. Règles de fidélité

Chaque fonction/global porté reçoit un bloc d'annotation :

```csharp
// GHIDRA: <nomOriginal> @ 0x<adresse>
// SOURCE: Ghidra (ReVa get-decompilation) | PCSX-Redux runtime | lecture directe
// JUSTIFICATION: PSX hardware adaptation only | C# language bridge only
// RELATION: <ce que faisait l'original et pourquoi l'adaptation est équivalente>
```

Classification de preuve : `CERTAIN` / `PROBABLE` / `PARTIAL` / `INCONNU` / `BLOCKED` / `CORRECTION`.

Interdits : redessiner l'architecture, introduire des abstractions modernes dans le cœur porté,
supprimer un chemin mort sans l'annoter, inventer une valeur non lue dans Ghidra, l'EXE ou
l'émulateur. Toute déviation volontaire (une seule à ce jour : §4.7) doit être annotée
`DELIBERATE DEVIATION` dans le code.

**Chemins encore `BLOCKED` ou `PARTIAL`, en un coup d'œil :**

| Quoi | État | Pourquoi |
|---|---|---|
| `DrawKanjiGlyphToTileMap` (0x800221fc) | `BLOCKED` | a besoin de la ROM kanji du BIOS ; inatteignable sur la version FR (§6.5) |
| Branche chaînée de `PlaySoundEffect` (`VabId != -1`) | `PARTIAL` | seule la branche directe est portée (§6.2) |
| Codes d'erreur carte -2..-5 | `PARTIAL` | le backend desktop ne peut produire que « répertoire absent » (§6.6) |
| Nom de chapitre d'une sauvegarde | `PARTIAL` | `SaveData.CurrentFlagName` n'est jamais écrit par le portage (§6.6) |
| `DisplayString2` | non porté | `printf` de mise au point |

---

## 8. Outils de vérification

**Ghidra / ReVa** — programme `/LOADER.EXE` (1017 fonctions, 5096 symboles).

**jPSXdec v2.0** — `D:\development\repo\Alundra Remake\jpsxdec_v2.0`, décodeur de référence :

```bash
java -jar jpsxdec.jar -f <movie.mov> -x index.idx
```

```bash
java -jar jpsxdec.jar -x index.idx -i 0 -vidfmt png -quality psx -up NearestNeighbor -start 149 -end 150 -dir out
```

`-vidfmt bs` sort le bitstream démultiplexé (ce que produit `StrSectorReader`) et `-vidfmt mdec`
le flux de codes MDEC (ce que produit `MdecVlcDecoder`) — les deux permettent d'isoler l'étage
fautif sans deviner.

**PCSX-Redux** — points d'arrêt utiles : `0x80027738` (en-tête STR par frame), `0x8002ba84`
(entrée/sortie VLC), `0x80027a4c` (progression des bandes), `0x80027ff4` (paramètres réels).
