# Analyse audio Alundra: musiques SEQ/VAB et SFX C#

Date: 2026-05-18

## Portee

Cette note analyse:

- le lecteur musique present dans `Alundraportage`, qui sait lire des pistes depuis `SOUND.BIN` avec un mini menu console;
- le chemin de lecture SFX dans la version C# du jeu et de l'editeur;
- l'ecart avec le comportement sonore original, en particulier la fonction `FUN_80049794 @ 0x80049794`, qui applique un effet de tone/volume sur des voix deja lancees.

Validation effectuee:

- `dotnet build AlundraTools\AlundraEngine\AlundraEngine.csproj --no-restore`: OK, 655 avertissements existants;
- chargement reflectif de `SoundBin` depuis `D:\development\repo\Alundra Remake\Alundra (France)\Alundra (France)_extracted\DATA\SOUND.BIN`: OK;
- headers lus: 962 `SfxRecord`, VAB global `Ps=11`, `Vs=58`, VAB map 0 `Ps=1`, `Vs=10`;
- decode ADPCM sans lecture audio sur quelques VAG: OK pour les buffers non vides, boucles detectees.

Limite de validation: je n'ai pas lance l'UI interactive ni compare le rendu audio avec une capture PSX. Les conclusions ci-dessous sont donc un audit de code plus une validation de chargement/decode, pas une validation perceptive complete.

## Lecture des musiques dans le port C

Le point d'entree est `Alundra::AudioPlayer::Start`, lance dans un thread separe depuis `Alundraportage/src/main.cpp`.

Chemin de donnees:

1. `loadSOUND_BIN()` ouvre `test/SOUND.BIN`.
2. `GenerateSectionInfos()` scanne tout le fichier a la recherche des magics `pQES` et `pBAV`.
3. Les sections `pQES` sont stockees dans `sequences` avec leur taille dans `sequenceSizes`.
4. Les sections `pBAV` sont parsees par `VAB::setupVAB()` et stockees dans `vabs`.
5. Le menu console propose deux modes:
   - `M`: choisir manuellement un index SEQ et un index VAB;
   - `T`: choisir une piste predefinie.
6. La piste selectionnee appelle `AudioEngine::SetSEQ(sequences[seq], sequenceSizes[seq], loops, loops > 0 ? -1 : 0)`, puis `AudioEngine::SetSoundFont(vabs[vab])`, puis `AudioEngine::Play()`.

La table de pistes du mini menu mappe les musiques connues sur des couples SEQ/VAB. Les six premieres sequences sont commentees comme `SE`; la premiere vraie piste BGM exposee est `Title`, avec `seq=6`, `vab=78`. Les pistes suivantes continuent globalement en `seq=N`, `vab=N+72`, par exemple `Jess` `7/79`, `Village Inoa` `21/93`, `Game Over` `44/116`, etc. Certaines fanfares ont `loops=1`, ce qui change le comportement de fin/loop donne a `SEQPlayer`.

La lecture effective repose sur FluidSynth:

- `AudioEngine::Init()` cree les settings, le synth, le driver audio, puis initialise `SEQPlayer`;
- `AudioEngine::SetSoundFont()` convertit le VAB en `.sf2` via `SF2::SoundFont::FromVAB()` puis charge ce SoundFont dans FluidSynth;
- `AudioEngine::SetSEQ()` remet l'etat SEQ a zero et transmet le buffer au lecteur;
- un thread appelle `SEQPlayer::Exec()` environ 60 fois par seconde;
- `SEQPlayer` parse le format SEQ, les deltas, les messages MIDI-like, les control changes, les meta events de tempo et de fin de piste.

Etat: ce lecteur musique C est le meilleur point de depart existant pour porter les musiques en C#, car il ferme deja trois problemes importants: extraction des sections `SOUND.BIN`, association SEQ/VAB, et conversion VAB vers SoundFont.

Points partiels dans ce lecteur C:

- plusieurs commandes NRPN/control changes restent marquees non implementees;
- la couche SPU originale est remplacee par FluidSynth/SoundFont, donc ce n'est pas une reproduction SPU exacte;
- l'audio thread fait un `continue` actif quand `skip` est vrai, ce qui peut consommer inutilement du CPU;
- le chemin `SoundFont::FromVAB()` est une adaptation pratique, pas une translitteration stricte du driver son original.

## Etat des musiques dans la version C#

La note precedente n'est plus a jour: le runtime C# ne se limite plus a enumerer `SOUND.BIN`. Le pipeline BGM/SEQ/VAB est maintenant translittere en grande partie dans `SoundManager` et branche vers une sortie audio desktop.

Indices principaux:

- `SoundBin` scanne les sections `pQES/pBAV` et expose `SoundBinSectionOffsets/Sizes/Types`, `SequenceSectionOffsets/Sizes`, `VabSectionOffsets/Sizes`, `MusicSeqVabOffsets`, plus la lecture par plages via `ReadRange()`;
- `SoundManager.LoadBgm()`, `InitializeBgm()`, `FUN_8008f2e8()`, `LoadMapSequence()`, `LoadMapSequenceVab()`, `HandleMapSoundStreaming()`, `LoadSeq()`, `PlaySeq()`, `FUN_8008F808()`, `FUN_8008F760()` et `FUN_8008F690()` ont maintenant du controle de flux et des mutations runtime materialises;
- `Script_166_0A6` appelle `LoadBgm(variables[1])` et `Script_167_0A7` appelle `FUN_8004b114(variables[1], variables[2])`; ces deux chemins alimentent maintenant un vrai runtime sequence/VAB cote C#;
- `Script_168_0A8` ne retourne pas toujours `0`: `IsSoundLoading()` reflète directement `g_soundLoadState != 0`;
- le blocage CERTAIN observe pendant cet audit etait local: `HandleMapSoundStreaming()` n'etait plus appele dans la boucle normale. Ce tick a ete reactive dans `GameEngine`, donc le chargement asynchrone declenche par `Script_167_0A7` progresse maintenant hors transitions de warp;
- les notes de sequence finissent sur la sortie audio desktop: le chemin passe par `TryPlaySequenceNoteVoice`/`FUN_800934B8`, puis `SoundBin.PlayLoadedVabTone()`, `PlaySfxInner()`, `WriteWavFile()`, et enfin `PlayWave()` vers `MonoGameSoundPlaybackBackend` ou `System.Media.SoundPlayer`.

Conclusion: le runtime C# sait maintenant charger des VAB/SEQ et declencher une lecture sonore. Le blocage principal n'est plus l'absence d'un pipeline BGM, mais le fait que le backend desktop ne reproduit pas encore le contrat SPU par voix requis pour une lecture fidele.

Blocages CERTAINS restants pour les musiques:

- `ISoundPlaybackBackend` expose maintenant `UpdateVoiceStereoVolume(...)` et `UpdateVoicePitch(...)`: les dirty flags SPU volume gauche/droite et pitch sont consommes sur les voix MonoGame suivies via `FUN_8009311C` et les pushes immediats des helpers runtime;
- `MonoGameSoundPlaybackBackend` applique maintenant volume/pan et pitch par voix; en revanche il n'expose toujours ni controle de boucle par points de boucle, ni reverb equivalente au SPU;
- `PlaySfxInner()` transporte maintenant jusqu'au backend le sous-cas `repeat && loopStart == 0 && loopEnd == sampleCount - 1`: les loops couvrant tout l'echantillon peuvent boucler fidelement, mais les vraies boucles a sous-plage (intro + sustain) restent non reproduites;
- quand le backend MonoGame est actif, le WAV desktop est maintenant encode avec un sample rate neutre (`44100`) et le pitch initial est pousse vers `SoundEffectInstance.Pitch`; le fallback `SoundPlayer` reste, lui, sur le pitch bake dans le WAV;
- `FUN_800912B4` ecrit bien les etats bruts `g_spuVoiceAdsr1[]` / `g_spuVoiceAdsr2[]` et pose le dirty flag ADSR `0x30`, mais `FUN_8009311C` et le contrat `ISoundPlaybackBackend` ne propagent toujours vers le desktop que volume stereo et pitch: l'enveloppe ADSR PSX reste donc non reproduite sur les voix suivies;
- les helpers reverb (`FUN_800906A8`, `FUN_800906C8`, `FUN_80090728` et leurs ecritures associees) restent des no-op desktop ou de simples enregistrements d'etat brut;
- la table de callback `0x801F6D68` n'est pas encore portee dans `FUN_8008C918`: on sait maintenant localement que `FUN_8008CA40` peut armer `field_0x16 = 0x28`, que `FUN_8008C918` recoit ensuite l'octet de controle associe, et que le chemin normal non-callback range ce couple dans `field_0x14` / `field_0x2A` avant consommation par `FUN_8008CC70`; il manque donc toujours la dereference effective de `0x801F6D68` et la signature exacte du callback.

## Lecture des SFX dans le jeu C#

Le chemin runtime actuel est:

1. les scripts/UI/appels gameplay appellent `SoundManager.PlaySoundEffect(uint sfxId)`;
2. `SoundManager` filtre `g_soundEffectState`, `sfxId < 1`, et `sfxId > 0x3c2`;
3. l'appel descend directement vers `_gameEngine.SoundBin.PlaySoundEffect((int)sfxId)`;
4. `SoundBin` choisit un `SfxRecord` depuis `SfxRecordsData`;
5. si `record.VabId == -1`, il lit dans le VAB global;
6. sinon il suit la chaine `RefSfxId` jusqu'a trouver un record dont `VabId == _mapVabIndex`;
7. pour chaque tone, il appelle `PlaySfxInner()` sur le VAB global ou map;
8. `PlaySfxInner()` decode le VAG ADPCM en PCM, ecrit un WAV en memoire, puis le joue via `System.Media.SoundPlayer.Play()`.

Le VAB de map est charge par `SoundBin.OpenMap(mapId)`. Dans `GameEngine.LoadMap`, `GameMap.Loaded` est un champ readonly initialise a `false`, donc le bloc `if (!CurrentMap.Loaded)` s'execute et appelle bien `SoundBin.OpenMap(mapId)`. C'est maladroit, mais en pratique le VAB de map est ouvert.

Ce qui fonctionne:

- `SoundBin` charge le header/body du VAB global et du VAB de map;
- la table `VabIndexByMapId` est utilisee pour choisir le VAB de map;
- les records SFX globaux et map sont resolus;
- les VAG ADPCM peuvent etre decodes en PCM/WAV;
- les SFX simples peuvent produire un son via `SoundPlayer`;
- l'editeur et le jeu partagent le meme decodeur `SoundBin`.

Ce qui n'est pas fidele au runtime original:

- aucune allocation de voix SPU equivalente n'est maintenue;
- `g_voiceState`, `g_voiceToneVolume`, `g_voiceTonePan`, priorites, max voices, flags de lecture et etat des voix ne sont pas correctement pilotes;
- les SFX dont `SeqNum != -1` ne sont plus ignores: ils passent maintenant par `TryPlaySoundEffectSequence()`, `LoadSeq()` et `PlaySeq()`, mais restent soumis aux limites du backend sequence/voix decrit ci-dessus;
- `record.Flags & 2` est teste, mais les flags ne sont jamais vraiment mis a jour comme dans le runtime original;
- les effets ADSR/reverb SPU ne sont toujours pas reproduits fidelement dans le jeu C#: volume/pan/pitch sont maintenant pousses sur les voix MonoGame suivies, mais pas les autres effets hardware;
- `System.Media.SoundPlayer` ne donne toujours ni mixage multi-voix controle par l'etat SPU, ni callback equivalent; `MonoGameSoundPlaybackBackend` couvre maintenant volume/pan/pitch par voix suivie mais pas les callbacks ni les loops par points;
- les loops SFX decodees (`loopStart`, `loopEnd`, `repeat`) ne sont gerees que partiellement a la lecture runtime: le sous-cas boucle sur tout l'echantillon est maintenant transporte jusqu'au backend, mais pas les loops a sous-plage;
- `StopAllSound()` coupe maintenant explicitement les 24 voix runtime avant de relancer la sequence courante, mais ce redemarrage repose toujours sur le backend sequence/voix partiel decrit ci-dessus.

Conclusion: les SFX C# sont lus partiellement. Le chemin est utile pour ecouter/identifier des samples, mais il n'est pas encore un port fidele du systeme sonore du jeu. En particulier, seules les loops couvrant tout l'echantillon sont maintenant gerees; les loops a sous-plage restent absentes.

## Lecture des SFX dans l'editeur C#

L'editeur utilise `AlundraTools.GameControls.SoundboardControl`.

Fonctionnement:

- `Initialize(SoundBin)` remplit la liste des VAG globaux depuis `GlobalVabHeader.Header.Vs`;
- la liste principale affiche tous les `SfxRecord` avec `VabId`, `ProgramNumber`, `ToneNumber`, `Note`, `Flags`, `SeqNum`, `RefSfxId`, `MaxVoices`, `NumTones`;
- `ChangeMap(uint mapId)` appelle `SoundBin.OpenMap(mapId)` et remplit la liste des VAG de map;
- selectionner un `SfxRecord` appelle `SoundBin.PlaySoundEffect(sfxid, -1, -1, _is8Bit, ...)`;
- selectionner un VAG global ou map appelle directement `PlaySfx()` ou `PlayMapSfx()`;
- l'entree MIDI peut rejouer le SFX selectionne avec une note/velocite.

L'editeur est donc meilleur comme soundboard/inspecteur de samples que comme verification du runtime: il affiche les records, les boucles et la waveform, mais il contourne aussi la logique originale de voix SPU et ne teste pas les effets runtime.

Point technique observe: la waveform retournee par `DecodeAdpcm()` est stockee en ordre big-endian interne pour le 16-bit, puis `WriteWavFile()` inverse les octets lors de l'ecriture WAV. C'est coherent pour la lecture actuelle, mais ce n'est pas un format PCM little-endian directement reutilisable sans precaution.

## Fonction originale `LoadMapSounds @ 0x8004A09C`

La fonction originale a ete inspectee par disassemblage PCSX-Redux et translitteree dans le runtime C#.

Comportement ferme:

- entree: `a0` = `mapId`;
- elle appelle `GetMapSoundIndex(mapId) @ 0x80049D3C`;
- si l'index son est non nul et different de `g_currentMapSoundIndex @ 0x80173844`, elle recharge l'etat BGM/SFX;
- si `g_requestedSeqId @ 0x80165128` est positif ou nul, elle appelle `InitializeBgm(g_requestedSeqId) @ 0x8008F458`, `FUN_8008A718(0)`, puis `ResetSomethingSound(g_requestedSeqId) @ 0x8008DF04`;
- si l'index son recalcule n'est pas `0x2D`, elle appelle `LoadMapSequence(index, 0) @ 0x80049BE0`;
- `LoadMapSequence` appelle `LoadMapSequenceVab @ 0x8004A184`, qui lit le header VAB associe depuis `SOUND.BIN`, appelle `LoadVabHeaderCore @ 0x8008FB0C`, puis streame le body VAB via `UploadVabBodyChunk @ 0x8008FFC0`;
- apres cette branche, elle appelle toujours `FUN_8008F808(g_requestedSeqId, 0x7F, 10)`;
- elle lit le groupe sonore via `FUN_80049F00(mapId)`, qui retourne directement `DAT_800C6D28[mapId]`;
- si ce groupe differe de `g_currentSoundGroup @ 0x80173848`, elle appelle `FUN_800489C8(mapId)`;
- `LoadMapSoundGroup @ 0x800489C8` libere le VAB map courant via `FreeLoadedVab(g_mapSoundVabId) @ 0x8008F9A4`, recalcule le groupe avec `GetSoundGroupByMapId @ 0x80049F00`, stocke `g_currentSoundGroup`, puis appelle `LoadMapSoundVab @ 0x80048850` pour charger le VAB map du groupe courant;
- la fin appelle `FUN_8005AC90()`, puis `FUN_8004BE0C()`, et retourne `1`.

Etat C# apres translitteration:

- `GameEngine.LoadMapSounds(uint mapId)` porte maintenant le commentaire `GHIDRA: LoadMapSounds @ 0x8004A09C` et conserve la fin originale: appel du bloc son, appel `FUN_8005ac90()`, appel `InitializeHudPositionBeforeHide()`, retour `1`;
- `SoundManager.LoadMapSounds(uint mapId)` reste le split C# existant pour la partie son, avec une relation explicite vers `0x8004A09C`;
- `GetSoundGroupByMapId(uint mapId)` a ete ajoute et retourne la table `SoundBin.VabIndexByMapId`, correspondant a `DAT_800C6D28`;
- `LoadMapSoundGroup(uint mapId)` a ete ajoute: il libere `g_mapSoundVabId`, met a jour `g_currentSoundGroup`, puis recharge le VAB de map via `SoundBin.OpenMap(mapId)`;
- `FUN_8008a718(0)` et `FUN_8008f808(g_requestedSeqId, 0x7F, 10)` sont presents dans le flux mais restent bloques, car le contrat de driver son PSX/sequence fade n'est pas encore porte dans le backend C#;
- `MainInventoryManager.FUN_8005ac90()` est maintenant appelable depuis `LoadMapSounds`, mais son appel interne `SetCdReadPosition(iVar2)` reste partiel dans le runtime C# actuel;
- `HudManager.InitializeHudPositionBeforeHide()` est annote comme `FUN_8004BE0C @ 0x8004BE0C`.

Interpretation prudente: `LoadMapSounds` n'est pas seulement un chargement BGM. C'est aussi le point de synchronisation entre offset son de map, groupe VAB/SFX de map, et etat UI/HUD apres changement de map. La translitteration C# respecte maintenant ce controle de flux, mais le chargement audio reste partiel tant que `FUN_80048850`, `FUN_8008A718` et `FUN_8008F808` ne sont pas portes avec un backend sonore controle par voix/sequence.

## Fonction originale `FUN_80049794 @ 0x80049794`

La fonction originale a ete inspectee par disassemblage PCSX-Redux.

Comportement ferme a ce stade:

- entree: `a0` = id SFX, `a1` et `a2` = parametres de mix tone/volume utilises dans le calcul;
- elle lit `g_soundEffectData @ 0x800A82E8`, avec une taille de record de `0x16` octets;
- si le record est global (`VabId == -1`), elle utilise le VAB global `DAT_800A8246`;
- sinon elle resout le SFX de map via `FindSfxRecordForSoundGroup(sfxId, g_currentSoundGroup) @ 0x80048A14` puis utilise le VAB map `g_mapSoundVabId @ 0x800A8248`;
- elle appelle `CopyVabProgramAttributes(vabId, programNumber, stack+0x10) @ 0x800901A8` pour recuperer des parametres de programme VAB;
- elle boucle sur `ToneCount` (`record + 0x14`);
- pour chaque tone, elle cherche une voix deja active via `FindVoiceBySfxIdAndToneIndex(sfxId, toneNumber + toneIndex) @ 0x8004974C`;
- si aucune voix n'est trouvee, elle ne lance pas de son et passe au tone suivant;
- si une voix est trouvee, elle lit les donnees tone courantes dans les tables de voix autour de `0x80175858`, notamment `g_voiceToneVolume @ 0x80175990` et `g_voiceTonePan @ 0x801759F0`;
- elle applique plusieurs calculs quadratiques/fixes sur volume gauche/droite, parametre `a1`, parametre `a2`, pan et donnees VAB;
- elle appelle finalement `SetVoiceVolume @ 0x80095298` pour mettre a jour les volumes gauche/droite de la voix SPU.

Interpretation prudente: `FUN_80049794` n'est pas un simple `PlaySoundEffect` avec volume. C'est une fonction d'effet/mix qui modifie des voix deja actives, par tone, en conservant le modele de voix SPU original. Elle depend donc directement d'un vrai etat de voix runtime.

Etat C# actuel:

- `SoundManager.PlaySoundEffectWithToneVolumeMix(int param_1, int param_2, int param_3)` est un TODO;
- les scripts `Script_171_0AB` et `Script_191_0BF` l'appellent, mais l'appel ne produit aucun effet;
- un pseudo-code commente existe dans `SoundManager.cs`; les recherches pures `FindSfxRecordForSoundGroup @ 0x80048A14` et `FindVoiceBySfxIdAndToneIndex @ 0x8004974C` sont maintenant portees, mais les dependances d'execution restent non portees: `g_soundEffectData` mutable, `CopyVabProgramAttributes @ 0x800901A8`, tables VAB runtime, voix actives backend et `SetVoiceVolume`.

Conclusion: tous les comportements scriptes qui reposent sur `0x80049794` sont actuellement perdus dans le runtime C#.

## Decodeur ADPCM C#

Deux ecarts avaient ete observes dans `SoundBin.DecodeAdpcm()` par comparaison avec l'implementation C du port et le comportement PSX ADPCM attendu. Ils sont maintenant corriges dans le runtime C#.

1. Historique de samples entre blocs

Le decodeur PSX ADPCM doit conserver les deux derniers samples decodes pour le filtre du bloc suivant. Avant correction, `adpcmLastSamples` etait mis a jour avec `currentBlockSamples[0]` et `[1]`, mais ces cases contenaient les samples reportes de l'ancien bloc, pas les deux derniers samples du bloc decode. Le decodeur conserve maintenant directement `oldSmp` et `olderSmpl`, comme le port C.

2. Shift invalide `13..15`

Dans le C, `DecodeAdpcmBlock()` ramene un shift `> 12` a `9`, comme le comportement PSX documente. La ligne equivalente est maintenant active cote C#.

Le decodeur C# applique aussi la formule de filtre avec le biais `+32` avant division par `64`, comme le port C. Il reste a comparer auditivement et par hashes PCM sur un corpus de VAG, mais les divergences locales connues sont corrigees.

## Comparaison synthetique

| Sujet | Port C `Alundraportage` | C# jeu | C# editeur |
|---|---|---|---|
| Scan `SOUND.BIN` `pQES/pBAV` | Oui | Oui, enumeration seulement | Non |
| Menu selection musique | Oui, console `M/T` | Non | Non |
| Lecture BGM SEQ | Oui, partielle via FluidSynth | Non | Non |
| Conversion VAB/SoundFont | Oui | `SoundFont.cs` existe, non branche BGM | Non utilisee par soundboard |
| Lecture SFX VAG | Non principal | Oui, partielle | Oui, partielle |
| SFX map VAB | N/A musique | Oui, via `OpenMap` | Oui, via `ChangeMap` |
| SFX sequence (`SeqNum`) | BGM SEQ oui, SFX seq non verifie | Non | Non |
| Etat voix SPU | Partiel dans `SEQPlayer`, pas runtime C# | Non | Non |
| Effet `0x80049794` tone/volume | Non porte dans C# | TODO, aucun effet | Non applicable |

## Plan de portage detaille pour agent IA

Objectif: porter progressivement le systeme audio original vers C# en conservant le controle de flux, les globales, les tables, les appels et les effets de bord du runtime PSX. Le backend desktop peut adapter la sortie audio, mais il ne doit pas absorber la logique metier du driver son original.

Regles globales pour l'agent:

- fermer chaque decision avec une preuve: Ghidra, PCSX-Redux, `SLUS_006.62`, `PE.IMG`, `Alundraportage`, `docs/`, ou memoire repo;
- annoter chaque fonction originale portee avec `GHIDRA:` et chaque globale originale portee avec `GHIDRA:`;
- annoter chaque helper C# nouveau avec `JUSTIFICATION:`;
- garder les noms bruts lorsque la semantique n'est pas fermee: `FUN_...`, `DAT_...`, `param_1`, `iVar1`, etc.;
- ne pas remplacer les tables, pools, flags, bitfields ou etats de voix du runtime par `List<>`, `Dictionary<>`, LINQ, manager moderne ou API de confort;
- separer strictement le coeur translittere des adaptations desktop: lecture fichier, mixage audio, buffer PCM, sortie MonoGame ou autre backend;
- marquer explicitement `PARTIAL:` ou `BLOCKED:` quand un contrat original n'est pas ferme;
- valider chaque phase par `dotnet build AlundraTools\AlundraEngine\AlundraEngine.csproj --no-restore` et par au moins une verification ciblee du comportement porte.

### Phase 0 - Inventaire et baseline

But: figer les preuves disponibles avant d'ecrire du code audio plus profond.

Taches:

1. Relever les chemins C# actuels: `SoundManager`, `SoundBin`, scripts audio `Script_166_0A6` a `Script_171_0AB`, soundboard editeur, chargement map.
2. Relever les chemins C existants dans `Alundraportage`: scan `SOUND.BIN`, `AudioEngine`, `SEQPlayer`, parse VAB, conversion SoundFont.
3. Exporter ou noter les fonctions originales deja fermees: `LoadMapSounds @ 0x8004A09C`, `GetMapSoundIndex @ 0x80049D3C`, `GetSoundGroupByMapId @ 0x80049F00`, `LoadMapSoundGroup @ 0x800489C8`, `PlaySoundEffectWithToneVolumeMix @ 0x80049794`.
4. Identifier les donnees statiques originales deja representees en C#: `g_SoundOffsetList`, `g_defaultSoundOffsetList`, `SoundBin.VabIndexByMapId`, `SfxRecordsData`, `MapVabOffsets`, `SeqOffsets`.
5. Creer une petite matrice de validation: map id, sound offset, VAB index, SFX global, SFX map, BGM attendu, etat attendu des globals son.

Livrables:

- une note de preuves dans `docs/` ou une mise a jour de ce rapport;
- une liste courte des fonctions audio a porter par ordre d'appel original;
- aucune modification comportementale hors instrumentation ou documentation.

Gate de validation:

- le build C# passe;
- la matrice distingue clairement `Closed`, `Partial`, et `Blocked`;
- aucune semantique audio nouvelle n'est inventee.

### Phase 1 - Corriger le decode ADPCM C#

But: rendre le decode VAG assez fiable pour servir de base aux SFX et aux futures voix desktop.

Taches:

1. Comparer `SoundBin.DecodeAdpcm()` avec l'implementation C du port et avec le comportement PSX ADPCM connu.
2. Corriger le report d'historique inter-blocs: les deux derniers samples du bloc decode doivent alimenter le bloc suivant.
3. Retablir la regle de shift invalide: les shifts `13..15` doivent etre traites comme dans le decodeur PSX observe, actuellement `shift > 12 => 9` dans le port C.
4. Ajouter une verification locale sur plusieurs VAG globaux et map: longueur PCM, non-null, boucles, absence d'exception.
5. Ne pas modifier encore le modele de lecture `SoundPlayer`, sauf si une correction minimale est necessaire pour tester le decode.

Livrables:

- patch limite a `SoundBin.DecodeAdpcm()` et tests ou script de verification si l'infrastructure existe;
- commentaire seulement si necessaire pour expliquer le report d'historique PSX.

Gate de validation:

- les buffers PCM restent non vides pour les VAG deja verifies;
- les boucles `loopStart`, `loopEnd`, `repeat` restent coherentes;
- pas de changement dans les tables SFX ou VAB.

Stop conditions:

- si le port C et les preuves PSX divergent, documenter le cas et ne pas choisir arbitrairement;
- si un VAG produit un resultat douteux, conserver le dump de l'input et marquer `PARTIAL`.

### Phase 2 - Porter l'enumeration `SOUND.BIN` pour les musiques

But: donner au C# la meme visibilite que le port C sur les sections `pQES` et `pBAV`.

Taches:

1. Porter mecaniquement le scan des magics `pQES` et `pBAV` depuis `Alundraportage` vers une zone bas niveau de `SoundBin` ou une classe adjacente justifiee par `JUSTIFICATION: C# language bridge only`.
2. Stocker les offsets et tailles dans des tableaux bas niveau, pas dans une API de playlist moderne.
3. Verifier que les six premieres sequences SE sont identifiees comme dans le port C.
4. Verifier les couples exposes par le port C: par exemple `Title` `seq=6`, `vab=78`, `Jess` `7/79`, `Village Inoa` `21/93`, `Game Over` `44/116`.
5. Ne pas encore lancer de BGM depuis les scripts tant que le chemin de lecture SEQ n'est pas porte.

Livrables:

- donnees `pQES/pBAV` accessibles au runtime C#;
- dump ou test listant le nombre de sections, premiers offsets, tailles et couples connus.

Gate de validation:

- les nombres de sections correspondent au port C sur le meme `SOUND.BIN`;
- les offsets ne depassent jamais la taille du fichier;
- aucune lecture audio nouvelle n'est branchee dans le gameplay a cette phase.

### Phase 3 - Creer l'adaptation desktop de voix audio

But: remplacer progressivement `System.Media.SoundPlayer` par un backend de voix controlees, sans changer le modele original.

Taches:

1. Identifier les champs originaux necessaires: `g_voiceState @ 0x80175858`, `g_voiceToneVolume @ 0x80175990`, `g_voiceTonePan @ 0x801759F0`, tone index, VAB id, flags, priorite, max voices.
2. Corriger la representation C# des tables de voix si necessaire: les volumes gauche/droite originaux sont des tables par voix, pas des scalaires uniques.
3. Definir une couche backend minimale qui sait allouer une voix, jouer un buffer PCM, stopper une voix, changer volume gauche/droite, changer pitch si possible.
4. Garder les appels originaux visibles dans `SoundManager`; le backend doit etre un adaptateur, pas le nouveau proprietaire de la logique SFX.
5. Brancher les SFX simples sur ce backend seulement apres avoir conserve les memes gardes que `PlaySoundEffect` et les fonctions originales portees.

Livrables:

- tables de voix C# proches des adresses originales;
- adaptateur desktop avec fonctions justifiees `JUSTIFICATION: backend audio adaptation only`;
- un SFX global simple jouable avec voice id observable.

Gate de validation:

- jouer deux SFX ne perd pas l'etat de voix du premier;
- `StopAllSound()` peut stopper les voix lancees par le backend;
- les volumes gauche/droite sont mutables par voice id.

Stop conditions:

- ne pas faire un mixer moderne complet si le contrat original n'est pas ferme;
- ne pas deplacer les decisions de priorite ou de choix de voix dans le backend.

### Phase 4 - Fermer les dependances restantes de `LoadMapSounds`

But: transformer le port partiel de `LoadMapSounds @ 0x8004A09C` en port plus fidele du chargement VAB/sequence.

Taches:

1. Analyser et porter `LoadMapSoundVab @ 0x80048850`: liberation VAB courant, copie/chargement depuis `SOUND.BIN`, mise a jour de `g_mapSoundVabId`, gestion des morceaux de VAB si la taille depasse le bloc lu.
2. Remplacer l'appel partiel `SoundBin.OpenMap(mapId)` dans `FUN_800489C8` par le flux original ou par un adapter strictement equivalent, documente comme `PARTIAL` si l'equivalence n'est pas complete.
3. Analyser `FUN_8008A718 @ 0x8008A718`: determiner si c'est une attente, une pompe driver, une synchro SPU, ou une fonction de service sequence.
4. Analyser `FUN_8008F808 @ 0x8008F808`: fermer les parametres `(seqId, 0x7F, 10)` et porter le contrat de volume/fade si possible.
5. Verifier que `g_currentMapSoundIndex`, `g_requestedSeqId`, `g_currentSoundGroup`, `g_mapSoundVabId` mutent dans le meme ordre que l'original.

Livrables:

- `FUN_80048850`, `FUN_8008A718`, `FUN_8008F808` portes ou explicitement bloques;
- `LoadMapSounds` sans appels factices non documentes;
- une trace avant/apres pour au moins deux maps avec groupes VAB differents.

Gate de validation:

- map sans changement d'offset ne recharge pas inutilement la sequence;
- map avec changement de groupe met a jour `g_currentSoundGroup` avant le chargement VAB;
- map avec index son `0x2D` saute bien `LoadMapSequence(index, 0)`.

### Phase 5 - Porter le chemin BGM SEQ/VAB

But: brancher les musiques dans le runtime C# avec un controle de flux compatible avec l'original.

Taches:

1. Porter ou adapter le lecteur `SEQPlayer` du port C en gardant ses etats, deltas, events MIDI-like, tempo et fin de piste.
2. Porter les fonctions runtime qui chargent et initialisent les sequences: `LoadMapSequence @ 0x80049BE0`, `FUN_8004B114 @ 0x8004B114`, `LoadBgm @ 0x80049B7C`, `InitializeBgm @ 0x8008F458`, `FUN_8008F2E8 @ 0x8008F2E8`.
3. Conserver les globals d'etat sequence: `g_requestedSeqId`, `g_resetSoundFlag`, `g_soundLoadState`, `g_forceStopAllSound`, `g_loadedSequenceHandles`.
4. Adapter la sortie instrumentale vers le backend desktop choisi: SoundFont/FluidSynth, synth interne, ou autre sortie, mais uniquement comme adaptation de backend.
5. Brancher `Script_166_0A6`, `Script_167_0A7`, et `Script_168_0A8` seulement quand les fonctions originales appelees ont un comportement observable coherent.

Livrables:

- lecture BGM minimale depuis une sequence `pQES` et un VAB associe;
- etat de chargement observable par `IsSoundLoading()` au lieu du retour constant `0`;
- documentation des commandes SEQ encore non implementees.

Gate de validation:

- une piste connue comme `Title` peut etre lancee depuis les donnees `SOUND.BIN`;
- un changement de map peut selectionner le bon offset sonore sans casser les SFX;
- les fanfares ou pistes non bouclees respectent au moins le contrat de fin/loop deja connu dans le port C.

### Phase 6 - Porter le chemin SFX original avant l'effet `0x80049794`

But: creer les voix et les etats attendus par `FUN_80049794`, au lieu de rejouer des WAV autonomes.

Taches:

1. Analyser et porter `FUN_800490FC @ 0x800490FC`, fonction centrale de lecture SFX.
2. Analyser et porter les fonctions voisines deja identifiees: `StopSoundEffect @ 0x80049634`, `FindVoiceBySfxId @ 0x80049714`, `FindVoiceBySfxIdAndToneIndex @ 0x8004974C`.
3. Porter ou representer mecaniquement `g_soundEffectData @ 0x800A82E8` avec record size `0x16`.
4. Porter `FindSfxRecordForSoundGroup @ 0x80048A14` pour resoudre un SFX de map par `g_currentSoundGroup`.
5. Porter les controles de max voices, flags, priorite et recherche de voix active.
6. Brancher le backend de voix de la phase 3 uniquement au point ou l'original declenche effectivement une voix SPU.

Livrables:

- lecture SFX globale et map via le flux original;
- voice id stocke dans les tables de voix;
- `FindVoiceBySfxIdAndToneIndex` capable de retrouver une voix active par SFX/tone.

Gate de validation:

- les SFX map continuent de se resoudre par le VAB de map courant;
- un SFX avec plusieurs tones cree ou met a jour les voix attendues;
- `g_soundEffectState` bloque toujours les lectures comme dans le code original.

### Phase 7 - Porter `FUN_80049794 @ 0x80049794`

But: reproduire l'effet tone/volume qui modifie des voix deja actives.

Taches:

1. Ne pas implementer cette fonction comme un overload de volume global.
2. Porter le lookup de record: global si `VabId == -1`, map via `FindSfxRecordForSoundGroup(sfxId, g_currentSoundGroup)` sinon.
3. Porter l'appel `CopyVabProgramAttributes(vabId, programNumber, stack+0x10) @ 0x800901A8` ou documenter chaque champ de sortie encore bloque.
4. Boucler sur `ToneCount` et appeler `FindVoiceBySfxIdAndToneIndex(sfxId, toneNumber + toneIndex)` pour retrouver les voix existantes.
5. Si aucune voix n'existe, ne pas lancer de nouveau son; passer au tone suivant comme l'original.
6. Si une voix existe, lire les volumes/pan courants dans les tables de voix et reproduire les calculs fixes quadratiques.
7. Porter `SetVoiceVolume(voiceId, volumeLeft, volumeRight) @ 0x80095298` comme update de volume par voix dans le backend desktop.

Livrables:

- `PlaySoundEffectWithToneVolumeMix` remplace le TODO par une translitteration annotee;
- les scripts `Script_171_0AB` et `Script_191_0BF` produisent un effet observable sur des voix existantes;
- les inconnues de `CopyVabProgramAttributes @ 0x800901A8` sont fermees ou marquees `BLOCKED`.

Gate de validation:

- appeler `FUN_80049794` sans voix active ne joue rien;
- appeler `FUN_80049794` apres un SFX compatible modifie la voix existante;
- les volumes gauche/droite changent par voice id sans relancer le sample.

### Phase 8 - Comparaison et durcissement

But: verifier que le port suit le comportement original assez finement pour continuer le runtime.

Taches:

1. Capturer des traces PCSX-Redux pour quelques scenarios: chargement map, SFX global, SFX map, changement BGM, appel `0x80049794`.
2. Capturer les memes scenarios cote C# avec logs de globals audio et voice ids.
3. Comparer les mutations de globals, pas seulement le son audible.
4. Ajouter des tests ou scripts de non-regression quand une preuve est fermee.
5. Mettre a jour ce rapport apres chaque phase avec `Closed`, `Partial`, `Blocked`.

Definition de done globale:

- `LoadMapSounds` conserve son ordre d'appels original;
- les BGM peuvent etre lues depuis `SOUND.BIN` en suivant les donnees `pQES/pBAV`;
- les SFX passent par un modele de voix controlable;
- `FUN_80049794` modifie des voix existantes;
- les ecarts restants sont limites au backend audio desktop et documentes localement.

## Execution du plan dans cette passe

Taches realisees:

- Phase 0: inventaire des chemins audio C# et C, fonctions originales et tables deja presentes;
- Phase 1: correction de `SoundBin.DecodeAdpcm()` pour l'historique inter-blocs, le shift invalide `13..15`, et la formule de filtre alignee sur le port C;
- Phase 2: ajout du scan `SOUND.BIN` par magics `pQES` et `pBAV` dans `SoundBin`, avec tableaux d'offsets et tailles pour sections globales, sequences et VAB;
- Phase 3 partielle: correction de l'etat brut des voix en tables 24 entrees pour `g_voiceVabId`, `g_voiceToneIndex`, `g_voiceToneVolume`, `g_voiceTonePan`, et ajout de `g_voiceSfxId @ 0x80175870`;
- Phase 4 partielle: analyse de `FUN_8008A718`, `FUN_8008F808`, `FUN_8008F760`, `FUN_80048850`; les contrats de synchro/driver et de chargement VAB restent partiels;
- Phase 6 partielle: port de `FindSfxRecordForSoundGroup @ 0x80048A14`, `FindVoiceBySfxId @ 0x80049714`, et `FindVoiceBySfxIdAndToneIndex @ 0x8004974C`, qui sont des recherches pures dans les tables SFX/voix;
- Phase 7 preparation: analyse de `CopyVabProgramAttributes @ 0x800901A8` et `SetVoiceVolume @ 0x80095298`, mais pas de port complet de `FUN_80049794` tant que le backend de voix et les donnees VAB runtime ne sont pas fermes.
- Passe sequence/voix: analyse de `FUN_8008E3D8 @ 0x8008E3D8` comme dispatcher frame audio protege par `g_voiceCommandLock`; il appelle `UpdateSoundVoicesState`, parcourt les slots actifs de `g_sequenceSlotMask`, puis dispatch les flags sequence `+0x90`. Detail complet dans `docs/alundra-sound-ghidra-xrefs-structures.md`.
- Passe `PlaySoundEffect`: controle de flux affine pour `PlaySoundEffect @ 0x800490FC`, incluant les sorties precoces, les branches global/map, les chemins `SeqNum != -1` via `LoadSeq`/`PlaySeq`, et les chemins VAG directs via `TriggerVoice`.
- Passe types runtime audio: verification que seuls `VabHdr`, `ProgAtr`, et `VagAtr` correspondent aux structures SDK/PsyQ deja presentes; `g_sequenceStatePointers` est ferme comme table de pointeurs vers `SequenceTrackState` de taille `0xAC` (`FUN_8008EEAC(0x80175A50, 4, 1)`), `g_voiceRuntimeSlots` comme `VoiceRuntimeSlot[24]` stride `0x34`, et `g_soundBinSequenceBuffer @ 0x80173850` comme buffer brut `SOUND.BIN`. Les structures brutes correspondantes sont maintenant materialisees cote C# dans `StaticVariables.cs`.

Taches bloquees par preuve ou backend insuffisant:

- Phase 3 complete: aucun backend de voix controlees n'existe encore dans `AlundraEngine`; `AlundraGame` reference MonoGame, mais le coeur moteur ne possede pas encore de contrat audio desktop capable de remplacer les voix SPU;
- Phase 4 complete: `FUN_8008A718` touche la synchronisation bas niveau du driver, et `FUN_8008F808` delegue a `FUN_8008F760`, qui modifie les tracks sequence pointes par `g_sequenceStatePointers @ 0x801F6CE8`; le type cible est maintenant documente et represente en C#, mais ces fonctions de synchro/fade ne sont pas encore portees;
- Phase 5: le lecteur `SEQPlayer` et l'association runtime BGM `pQES/pBAV` ne sont pas encore portes;
- Phase 6 complete: `FUN_800490FC` appelle encore des fonctions non portees comme `FUN_80094660`, `FUN_80090370`, `FUN_8008BC00`, `FUN_8008F188`, et manipule les flags mutables de `g_soundEffectData`;
- Phase 7 complete: `FUN_80049794` depend maintenant de helpers de lookup portes, mais reste bloque par `CopyVabProgramAttributes @ 0x800901A8`, par les tables VAB runtime sous `0x801F7668`, et par `SetVoiceVolume @ 0x80095298` cote backend voix.

## Statut final

Closed:

- le port C sait lire des musiques via couples SEQ/VAB extraits de `SOUND.BIN`;
- le C# sait charger `SOUND.BIN`, les VAB global/map et decoder des VAG simples;
- le C# enumere maintenant les sections `pQES/pBAV` de `SOUND.BIN`;
- le decodeur ADPCM C# a ete corrige sur l'historique inter-blocs et le shift invalide `13..15`;
- le jeu et l'editeur C# passent par `SoundBin` pour les SFX;
- `LoadMapSounds @ 0x8004A09C` est maintenant analyse et son controle de flux principal est translittere en C#;
- `FUN_80049F00 @ 0x80049F00` est ferme comme lecture directe de `DAT_800C6D28[mapId]`;
- `FindSfxRecordForSoundGroup @ 0x80048A14`, `FindVoiceBySfxId @ 0x80049714`, et `FindVoiceBySfxIdAndToneIndex @ 0x8004974C` sont portes comme recherches pures dans les tables SFX/voix;
- `FUN_80049794 @ 0x80049794` est confirmee comme une fonction d'effet sur voix existantes, pas comme une lecture SFX standard.
- `FUN_8008E3D8 @ 0x8008E3D8` est fermee comme dispatcher de tick sequence/voix cote controle de flux, avec handlers de flags `0x01/0x02/0x04/0x08/0x10/0x20/0x40/0x80` documentes;
- les types des globals audio demandes sont fermes cote preuve et materialises cote C#: `g_sequenceStatePointers` expose 4 tracks sequence de taille `0xAC`, `g_voiceSlotSequenceKey @ 0x801F793E` est un `short` de `VoiceRuntimeSlot + 0x0E`, `g_voiceSlotNoiseState @ 0x801F794B` est un `byte` de `VoiceRuntimeSlot + 0x1B`, et `g_soundBinSequenceBuffer @ 0x80173850` est un buffer brut de sequences `SOUND.BIN`.

Partial:

- les SFX C# sont audibles/inspectables mais pas fideles au systeme SPU;
- l'etat brut des voix C# est plus proche de l'original, mais aucune allocation/mixage de voix fidele n'est encore branche;
- `FUN_800489C8 @ 0x800489C8` est porte partiellement: mise a jour de `g_currentSoundGroup` fermee, mais chargement VAB encore delegue a l'adapter C# `SoundBin.OpenMap(mapId)` au lieu d'un port complet de `FUN_80048850`;
- `FUN_8005AC90 @ 0x8005AC90` est appelee par `LoadMapSounds`, mais son `SetCdReadPosition` interne reste non porte;
- le port C des musiques est fonctionnel comme lecteur desktop, mais reste une adaptation FluidSynth partielle.

Blocked:

- lecture BGM C# non implementee;
- contrats `FUN_8008A718`, `FUN_8008F760`, et `FUN_8008F808` non implementes dans le backend C#;
- chemin complet `FUN_800490FC` non porte: les slots voix bruts existent maintenant en C#, mais l'allocation/stop de voix SPU et les structures VAB runtime ne sont pas encore implementees fidelement;
- handlers de commandes sequence appeles sous `FUN_8008BDD0` encore bruts (`FUN_8008C064`, `FUN_8008C144`, `FUN_8008C1B8`, `FUN_8008D4C0`, `FUN_8008D568`, `FUN_8008D8D0`), faute de semantique command-stream complete;
- SFX sequences `SeqNum != -1` non implementes;
- effet tone/volume `0x80049794` non implementable fidelement tant que l'etat de voix SPU/adaptation desktop n'existe pas;
- pas de comparaison audio originale vs C# effectuee dans cette passe.

## Mise a jour port C# audio 2026-05-14

Closed dans `SoundManager.cs`:

- `FUN_8008EEAC @ 0x8008EEAC` initialise maintenant `g_sequenceSlotCount`, `g_sequenceTrackCount` et les champs bruts par slot/track de `g_sequenceStatePointers`.
- `FUN_8008E3D8 @ 0x8008E3D8` est branche comme dispatcher de tick sequence protege par `g_voiceCommandLock`.
- Les handlers simples `FUN_8008EBF8`, `FUN_8008EB5C`, `FUN_8008EC24`, `FUN_8008F4AC`, `FUN_80093DE8`, `FUN_80093EF4`, `ResetSomethingSound2 @ 0x8008DD8C`, `SetVoiceVolume @ 0x80095298`, `StopVoice @ 0x80094F20`, `SyncSoundEffectVoiceStates @ 0x80048FCC` et `StopSoundEffect @ 0x80049634` sont materialises cote C#.
- `StopAllSound @ 0x80049AF4` repasse par `SetSeqVolume` et `PlaySeq` comme le chemin original commente.
- `g_soundEffectData @ 0x800A82E8` est maintenant represente cote C# par une table mutable de records bruts `0x16`, initialisee depuis `SoundBin.SfxRecords`; `StopSoundEffect` remet bien `Flags` a `0` et `ResetSoundEffectRuntime` efface au moins le bit voix `0x1` apres l'arret global des voix.
- `InitializeSoundSystem` charge maintenant le VAB SFX global dans `g_globalSoundVabId`, et `LoadMapSoundGroup` recharge aussi le VAB SFX map dans `g_mapSoundVabId` en plus du bridge desktop `SoundBin.OpenMap(mapId)`.

Partial:

- `FUN_8008E610` et `FUN_8008E8D0` portent le controle de flux et les champs de transition connus, mais la signification musicale exacte des champs `+0x3E/+0x40/+0x42` reste partielle.
- `SyncSoundEffectVoiceStates` utilise le snapshot expose par `SoundBin.VoicesAreActive`; le snapshot SPU original complet n'existe pas encore cote backend desktop.
- `ResetSoundEffectRuntime @ 0x80048E44` stoppe les voix SFX actives via `StopVoice` et nettoie maintenant le bit voix `0x1` dans `g_soundEffectData`, mais le nettoyage des SFX sequences reste bloque par `FUN_8008DD1C`.

Blocked:

- `PlaySoundEffectWithToneVolumeMix @ 0x80049794` reste volontairement bloque sur la formule fixe complete et sur `CopyVabProgramAttributes @ 0x800901A8`; le port ne doit pas inventer les termes manquants.
- `FUN_8009311C`, `FUN_80091134`, `FUN_80090168`, `FUN_8008A718`, `FUN_8008BCC4` et `FUN_8008D8D0` restent limites par les contrats SPU/backend ou par le parser de commandes sequence non ferme.

## Mise a jour port C# audio 2026-05-17

Closed dans `SoundManager.cs` et `GameEngine.cs`:

- `AreSoundEffectsIdle @ 0x80049E10` est maintenant porte avec le comportement ferme par ASM: scan de `g_voiceSfxId[24]`, puis scan des records `g_soundEffectData` sequences avec `Flags & 0x2` et `FUN_8008DD1C(seqSlot, 0) == 1`.
- `WaitForSoundEffectsIdle @ 0x80049FF8` est maintenant adapte cote C# sur le chemin warp: la transition ne se termine plus des la fin visuelle, elle attend d'abord l'expiration de `g_soundEffectState`, puis l'idle SFX, puis 3 frames audio supplementaires.
- `InitializeSoundSystem @ 0x800484E8` aligne maintenant le reset original des tables runtime en nettoyant aussi `g_voiceSfxId @ 0x80175870` avec `g_voiceState @ 0x80175858`.
- `FUN_8008A718 @ 0x8008A718` a maintenant un bridge hote minimal pour les dependances visibles de ce slice: decrement de `g_soundEffectState` et synchro des voix SFX desktop exposees par `SoundBin.VoicesAreActive`.

Partial:

- le bridge C# de `FUN_8008A718` ne remplace pas encore le vrai contrat driver PsyQ/SPU; il ne couvre que le countdown observable et la remise a jour des voix necessaires a `PlaySoundEffect`/`WaitForSoundEffectsIdle`.
- l'adaptation de `WaitForSoundEffectsIdle` est non bloquante par host frame pour respecter la boucle MonoGame; elle ne reproduit pas la boucle interne bloquante PSX au niveau implementation, seulement son effet de controle de flux visible.

Blocked:

- la garde `IsSoundEffectAlreadyPlaying @ 0x80048DF4` reste non branchee dans le runtime C# tant que le site d'effacement de la table `0x80165028` n'est pas prouve; les xrefs locaux essayes sur `InitializeSoundSystem @ 0x800484E8`, `MainLoop @ 0x8002BFE0`, `UpdateWorld @ 0x8002E34C`, et le helper audio `0x8008E034` n'ont montre aucun reset local de cette table.
- `TriggerVoice @ 0x80094660`, `CalculateVoicePitch @ 0x80091C18`, et le backend de voix SPU restent les prochains blocages structurants pour fermer le chemin direct VAG avec fidelite.