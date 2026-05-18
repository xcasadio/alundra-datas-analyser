# Analyse Ghidra audio par xrefs

Date: 2026-05-14

Cette passe evite les recherches globales de decompilation Ghidra. Les fonctions ont ete suivies par xrefs/call-tree depuis les entrees son deja connues: `LoadMapSounds @ 0x8004A09C`, `PlaySoundEffect @ 0x800490FC`, `PlaySoundEffectWithToneVolumeMix @ 0x80049794`, et les helpers VAB/voix appeles directement.

## Verification SDK PSX

Les types SDK/PsyQ verifiables dans le depot sont les structures VAB deja presentes dans le port C et dans le C#: `VabHdr` (`0x20` octets), `ProgAtr` (`0x10` octets), et `VagAtr` (`0x20` octets). Elles correspondent aux tables VAB lues dans `SOUND.BIN` et ne doivent pas etre recriees sous un autre nom.

Les structures demandees ici ne correspondent pas a ces types SDK:

- `g_sequenceStatePointers @ 0x801F6CE8` pointe vers des tracks runtime de sequence de taille `0xAC`; le port C a deja une structure equivalente appelee `AudioData`, mais avec des pointeurs desktop `uint8_t*`. Pour le runtime PSX/C#, les pointeurs doivent rester des pointeurs 32-bit/offsets aux offsets originaux `0x04`, `0x08`, `0x0C`.
- `g_voiceRuntimeSlots @ 0x801F7930` est un tableau interne de slots voix de taille `0x34`, distinct des structures SDK SPU comme `SpuVoiceAttr`. Il stocke des cles sequence, VAB/program/tone, priorite/age de remplacement et etat noise/occupation.
- `g_soundBinSequenceBuffer @ 0x80173850` est un buffer brut charge depuis `data\sound.bin`; ce n'est pas une structure SDK.

Representation C# actuelle: `StaticVariables.cs` contient maintenant les structures brutes `SequenceTrackState` (`0xAC`) et `VoiceRuntimeSlot` (`0x34`), ainsi que les globals `g_sequenceStatePointers`, `g_sequenceSlotCount`, `g_sequenceTrackCount`, `g_voiceRuntimeSlots`, et `g_soundBinSequenceBuffer`. Dans l'original, `g_sequenceStatePointers @ 0x801F6CE8` est une table de pointeurs 32-bit; l'appel ferme `FUN_8008EEAC(0x80175A50, 4, 1)` fait pointer ses 4 entrees vers un bloc contigu `SequenceTrackState[4]` a `0x80175A50`. Cote C#, cette table est donc exposee comme `SequenceTrackState[4]` typé. Cette representation ne porte pas encore le comportement du driver son; elle materialise uniquement les layouts et globals fermes par xrefs.

## Fonctions fermees ou renommees

Labels Ghidra appliques avec confiance haute:

| Adresse | Label | Preuve |
|---|---|---|
| `0x80048850` | `LoadMapSoundVab` | libere `g_mapSoundVabId`, lit `SOUND.BIN` via table `0x800A7D34`, charge header/body VAB map |
| `0x800489C8` | `LoadMapSoundGroup` | appelle `GetSoundGroupByMapId`, stocke `g_currentSoundGroup`, puis appelle `LoadMapSoundVab` |
| `0x80048A80` | `PlayResolvedMapSoundEffect` | remet a jour le runtime SFX, suit la chaine `RefSfxId` jusqu'au groupe son courant, puis appelle `PlaySoundEffect` |
| `0x80048A14` | `FindSfxRecordForSoundGroup` | suit la chaine `SfxRecord.RefSfxId` jusqu'au record dont `VabId == soundGroup`, sinon `-1` |
| `0x800490FC` | `PlaySoundEffect` | point central SFX: garde `g_soundEffectState`, resolve global/map, charge seq si besoin, declenche voix |
| `0x80049714` | `FindVoiceBySfxId` | parcourt 24 voix et compare `g_voiceSfxId[i]` |
| `0x8004974C` | `FindVoiceBySfxIdAndToneIndex` | compare `g_voiceSfxId[i]` et `g_voiceToneIndex[i]` |
| `0x80049794` | `PlaySoundEffectWithToneVolumeMix` | appelee par `Script_171_0AB` et `Script_191_0BF`, modifie des voix existantes via `SetVoiceVolume` |
| `0x80049E10` | `AreSoundEffectsIdle` | retourne faux tant qu'une entree non nulle existe dans `g_voiceSfxId[24]` ou qu'une sequence SFX marquee bit `0x2` est encore active |
| `0x80049FF8` | `WaitForSoundEffectsIdle` | boucle la frame audio jusqu'a expiration de `g_soundEffectState`, puis jusqu'a `AreSoundEffectsIdle`, puis attend 3 frames |
| `0x800909E8` | `AllocateVoiceSlot` | choisit une voix libre ou remplace selon priorite/age, remet l'age a 0 et retourne l'index |
| `0x800901A8` | `CopyVabProgramAttributes` | appelle `SelectLoadedVabProgram`, copie les attributs programme VAB vers un buffer sortie |
| `0x800902AC` | `SelectLoadedVabProgram` | valide VAB/programme, installe les pointeurs courants VAB et le premier tone courant |
| `0x80094F20` | `StopVoice` | protege par `g_voiceCommandLock`, selectionne `g_currentVoiceIndex`, puis appelle `FUN_80091134(0)` |
| `0x8004A184` | `LoadMapSequenceVab` | lit la section VAB associee a l'index son courant, charge le header, puis streame le body par chunks |
| `0x8008FAAC` | `LoadVabHeader` | wrapper qui force le mode header/body standard et appelle `LoadVabHeaderCore` |
| `0x8008FB0C` | `LoadVabHeaderCore` | parse le header `pBAV`, reserve/valide un slot VAB et remplit les tables runtime VAB |
| `0x8008FFC0` | `UploadVabBodyChunk` | transfere un chunk du body VAB vers la SPU et passe `g_loadedVabState[vabId]` de `2` a `1` a la fin |
| `0x80091C18` | `CalculateVoicePitch` | calcule un pitch 16-bit depuis la note demandee, le tone courant et la table pitch compacte 12x16 sous `0x800C9798` |

Labels ajoutes mais non primaires car un ancien label primaire existe deja dans Ghidra:

| Adresse | Nouveau label ajoute | Ancien primaire observe | Raison |
|---|---|---|---|
| `0x80049F00` | `GetSoundGroupByMapId` | `GetSoundGroupBbyMapId` | correction typographique, lecture directe de table map->groupe son |
| `0x80048E44` | `ResetSoundEffectRuntime` | `InitializeSoundSomething` | stoppe les voix SFX actives, nettoie les sequences SFX terminees, puis appelle `0x80090168` |
| `0x80048FCC` | `SyncSoundEffectVoiceStates` | `UpdateSoundStream` | lit le statut backend des 24 voix et synchronise `g_voiceState`, `g_voiceSfxId`, `g_voiceVabId` |
| `0x80049BE0` | `LoadMapSequence` | `MaybeLoadSound` | charge la sequence BGM de map depuis `SOUND.BIN`, met a jour `g_requestedSeqId` et `g_currentMapSoundIndex` |
| `0x80049D3C` | `GetMapSoundIndex` | `GetSoundOffsetByMapId` | retourne l'index BGM/SOUND.BIN de la map, avec override par flags d'evenement |
| `0x8008F9A4` | `FreeLoadedVab` | `MaybeFreeSound` | si le VAB est charge, appelle `SpuFree`, efface `g_loadedVabState[vabId]`, puis decremente `g_loadedVabCount` |
| `0x80090370` | `CopyVabToneAttributes` | `GetVoiceVolumes` | le corps copie des attributs tone VAB, pas des volumes de voix |
| `0x80049634` | `StopSoundEffect` | `PlaySoundEffect` | le corps arrete toutes les voix d'un SFX via `FindVoiceBySfxId`/`StopVoice` puis remet `SfxRecord.Flags` a 0 |
| `0x80049060` | `CountActiveVoicesForSfx` | `FindAvailableSoundBank` | le corps compte les voix actives matching SFX/VAB/tone pour la limite `MaxVoices` |

Fonctions taguees `Sound` dans Ghidra: `LoadMapSounds`, `LoadMapSoundVab`, `LoadMapSoundGroup`, `LoadMapSequenceVab`, `PlayResolvedMapSoundEffect`, `ResetSoundEffectRuntime`, `SyncSoundEffectVoiceStates`, `PlaySoundEffect`, `PlaySoundEffectWithToneVolumeMix`, `TriggerVoice`, `FindSfxRecordForSoundGroup`, `FindVoiceBySfxId`, `FindVoiceBySfxIdAndToneIndex`, `StopSoundEffect`, `StopVoice`, `CountActiveVoicesForSfx`, `AllocateVoiceSlot`, `CopyVabProgramAttributes`, `CopyVabToneAttributes`, `SelectLoadedVabProgram`, `LoadVabHeader`, `LoadVabHeaderCore`, `UploadVabBodyChunk`, `FreeLoadedVab`, `CalculateVoicePitch`, `LoadSeq`, `PlaySeq`, `FUN_8008F808`, `LoadMapSequence`, `GetMapSoundIndex`, `AreSoundEffectsIdle`, `WaitForSoundEffectsIdle`, `IsSoundEffectAlreadyPlaying`.

Labels Ghidra globaux appliques pendant cette passe:

| Adresse | Label | Type ferme ou partiel | Preuve |
|---|---|---|---|
| `0x801F6CE8` | `g_sequenceStatePointers` | `SequenceTrackState*[g_sequenceSlotCount]` | `FUN_8008EEAC` initialise chaque entree vers un bloc de tracks `0xAC`; les handlers sequence dereferencent ensuite `slot + track * 0xAC` |
| `0x801F7568` | `g_sequenceSlotCount` | `short` | `FUN_8008EEAC` le pose depuis `param_2`; `FUN_8008E3D8` l'utilise comme borne de slots sequence |
| `0x801F7570` | `g_sequenceTrackCount` | `short` | `FUN_8008EEAC` le pose depuis `param_3`; `FUN_8008E3D8` l'utilise comme borne interne de tracks |
| `0x801F7930` | `g_voiceRuntimeSlots` | `VoiceRuntimeSlot[24]`, stride `0x34` | allocation/init/update indexent par `voice * 0x34` |
| `0x801F793E` | `g_voiceSlotSequenceKey` | `short`, champ `VoiceRuntimeSlot + 0x0E` | `TriggerVoice` ecrit `0x21`, les chemins sequence ecrivent `(track << 8) | seqId`, et les updates voix comparent cette cle |
| `0x801F794B` | `g_voiceSlotNoiseState` | `byte`, champ `VoiceRuntimeSlot + 0x1B` | valeurs observees: `0` libre, `1` voix active tonale, `2` voix noise active; `2` declenche `SpuSetNoiseVoice(0, ...)` a l'arret/remplacement |
| `0x80173850` | `g_soundBinSequenceBuffer` | `byte[]` | `InitializeSoundSystem` charge `data\sound.bin` dans ce buffer jusqu'a `SfxVabHeaderOffset`; `PlaySoundEffect` y lit les SFX sequences map |

## Structure `SfxRecord @ 0x800A82E8`

`PlaySoundEffect`, `FindSfxRecordForSoundGroup`, `StopSoundEffect`, et `PlaySoundEffectWithToneVolumeMix` ferment une taille de record `0x16` octets. L'indexation originale calcule `id * 0x16`.

Representation C# actuelle: `StaticVariables.cs` contient maintenant une structure brute `SoundEffectRecord` de taille `0x16` et la table mutable `g_soundEffectData`, initialisee depuis `SoundBin.SfxRecords`. Cela ferme les champs necessaires aux flags runtime (`Flags`), aux resolutions map (`RefSfxId`), et aux limites de voix (`MaxVoices`, `ToneCount`) sans garder le placeholder entier precedent.

| Offset | Type | Nom ferme | Preuve |
|---:|---|---|---|
| `0x00` | `short` | `VabId` | `-1` = VAB global, autre valeur comparee a `g_currentSoundGroup`; `-2` ignore le SFX |
| `0x02` | `short` | `ProgramNumber` | passe a `SelectLoadedVabProgram(vabId, program)` |
| `0x04` | `short/ushort` | `ToneNumber` | additionne avec `toneIndex` et stocke dans `g_voiceToneIndex` |
| `0x06` | `short` | `Note` | passe a `TriggerVoice` comme note/hauteur |
| `0x08` | `ushort` | `Flags` | bit `0x1` mis apres lancement voix, bit `0x2` bloque certains chemins sequence; remis a 0 par `StopSoundEffect` |
| `0x0A` | `short` | `SeqNum` | `-1` = pas de sequence; sinon indexe `g_soundEffectSeqOffsets @ 0x800A81C8` |
| `0x0C` | `short` | `RefSfxId` | chaine de resolution map SFX dans `FindSfxRecordForSoundGroup` |
| `0x0E` | `short` | `field_0x0E` | lu/stocke ailleurs non ferme dans cette passe |
| `0x10` | `short` | `MaxVoices` | compare au retour de `CountActiveVoicesForSfx` avant allocation |
| `0x12` | `short` | `field_0x12` | non ferme |
| `0x14` | `short` | `ToneCount` | borne la boucle de creation/modification de tones |

## Tables de voix runtime `0x80175858`

Les fonctions `PlaySoundEffect`, `FindVoiceBySfxId`, `FindVoiceBySfxIdAndToneIndex`, `StopSoundEffect`, et `PlaySoundEffectWithToneVolumeMix` ferment une representation en tables paralleles de 24 voix. Les index de voix vont de `0` a `0x17`.

| Adresse | Type | Label ferme | Preuve |
|---|---|---|---|
| `0x80175850` | `int` | `g_soundEffectState` | garde global de `PlaySoundEffect` |
| `0x80175858` | `byte[24]` | `g_voiceState` | teste/pose l'etat actif; `PlaySoundEffect` met `0x80` apres allocation |
| `0x80175870` | `int[24]` | `g_voiceSfxId` | `FindVoiceBySfxId` compare `base + 0x18 + i*4` |
| `0x801758D0` | `int[24]` | `g_voiceVabId` | `PlaySoundEffect` stocke `SfxRecord.VabId`; `CountActiveVoicesForSfx` compare au record courant |
| `0x80175930` | `int[24]` | `g_voiceToneIndex` | stocke `SfxRecord.ToneNumber + toneLoopIndex`; utilise par `FindVoiceBySfxIdAndToneIndex` |
| `0x80175990` | `int[24]` | `g_voiceToneVolume` | recoit `VagToneAttr.Vol` depuis `CopyVabToneAttributes` sortie offset `0x02` |
| `0x801759F0` | `int[24]` | `g_voiceTonePan` | recoit `VagToneAttr.Pan` depuis `CopyVabToneAttributes` sortie offset `0x03`; utilise pour calcul gauche/droite dans `0x80049794` |

Point corrige: les deux champs `0x80175990` et `0x801759F0` ne sont pas les volumes gauche/droite de sortie. Les vrais volumes de sortie SPU sont dans les structures sous `0x801F7798/0x801F779A` et sont mis a jour par `SetVoiceVolume @ 0x80095298`.

`SyncSoundEffectVoiceStates @ 0x80048FCC` synchronise ces tables depuis un snapshot backend. Si le snapshot indique qu'une voix n'est plus active et que `g_voiceState[i]` n'est ni `0` ni `0x80`, elle met `g_voiceState[i] = 0`, `g_voiceSfxId[i] = 0`, et `g_voiceVabId[i] = -2`.

## Structures VAB runtime

`SelectLoadedVabProgram @ 0x800902AC` ferme le role des pointeurs courants utilises ensuite par `CopyVabProgramAttributes`, `CopyVabToneAttributes`, et `TriggerVoice`.

| Adresse | Type | Label ferme | Preuve |
|---|---|---|---|
| `0x800A8246` | `short` | `g_globalSoundVabId` | utilise quand `SfxRecord.VabId == -1` |
| `0x800A8248` | `short` | `g_mapSoundVabId` | VAB map courant, charge par `LoadMapSoundVab` |
| `0x801F7590` | `void*[16]` | `g_loadedVabProgramAttrPointers` | indexe par VAB id dans `SelectLoadedVabProgram`, devient `g_currentVabProgramAttrs` |
| `0x801F7618` | `void*[16]` | `g_loadedVabToneAttrPointers` | indexe par VAB id, devient `g_currentVabToneAttrs` |
| `0x801F75D0` | `void*[16]` | `g_loadedVabHeaderPointers` | `LoadVabHeaderCore` stocke le pointeur de header `pBAV` du slot VAB |
| `0x801F7668` | `VabProgramAttr*` | `g_currentVabProgramAttrs` | base lue par `CopyVabProgramAttributes` et `TriggerVoice` |
| `0x801F7680` | `VabToneAttr*` | `g_currentVabToneAttrs` | base lue par `CopyVabToneAttributes` et `TriggerVoice` |
| `0x801F7699` | `byte` | `g_currentVabId` | pose par `SelectLoadedVabProgram` |
| `0x801F769E` | `byte` | `g_currentVabProgramIndex` | pose par `SelectLoadedVabProgram` |
| `0x801F769F` | `byte` | `g_currentVabFirstToneIndex` | lu a `programAttr + 0x08`, sert a calculer l'offset dans la table tone |
| `0x801F76B8` | `byte[16]` | `g_loadedVabState` | `1` = VAB charge; garde commun des fonctions VAB |
| `0x801F76D0` | `uint[16]` | `g_loadedVabBodySizes` | taille body/sample calculee par `LoadVabHeaderCore`, reprise par `UploadVabBodyChunk` |
| `0x801F7710` | `short` | `g_loadedVabCount` | incremente/decremente par les chemins load/free VAB; `FreeLoadedVab` le decremente apres `SpuFree` |
| `0x801F7718` | `void*[16]` | `g_loadedVabSpuAllocationPointers` | pointeur passe a `SpuFree` par `FreeLoadedVab` |
| `0x801F7758` | `void*[16]` | `g_loadedVabSampleDataPointers` | pointeur vers la zone sample/body VAB calculee par `LoadVabHeaderCore` |

Chemin VAB ferme par xrefs:

1. `LoadMapSequenceVab @ 0x8004A184` lit d'abord le header VAB depuis `SOUND.BIN`, appelle `LoadVabHeader @ 0x8008FAAC`, puis ecrit le VAB id retourne dans le pointeur passe en parametre.
2. `LoadVabHeader @ 0x8008FAAC` est un wrapper vers `LoadVabHeaderCore(buffer, requestedVabId, 1, uploadBase)`.
3. `LoadVabHeaderCore @ 0x8008FB0C` reserve un slot VAB libre ou le slot demande, verifie la signature `pBAV`, remplit les pointeurs program/tone/sample, calcule la taille body, alloue une zone SPU si necessaire, puis pose `g_loadedVabState[vabId] = 2`.
4. `LoadMapSequenceVab` lit ensuite le body par chunks de `0x10000` octets maximum et appelle `UploadVabBodyChunk @ 0x8008FFC0` pour chaque chunk.
5. `UploadVabBodyChunk` garde un etat de transfert partiel et pose `g_loadedVabState[vabId] = 1` quand la taille restante tombe a zero.

## Structures voix SPU backend

`TriggerVoice @ 0x80094660`, `AllocateVoiceSlot @ 0x800909E8`, `StopVoice @ 0x80094F20`, `SetVoiceVolume @ 0x80095298`, `FUN_800912B4`, et `FUN_80090C58` ferment plusieurs bases runtime SPU. Cette couche reste partielle cote noms de tous les champs, mais les offsets suivants sont fermes par acces.

| Adresse | Type | Label | Preuve |
|---|---|---|---|
| `0x801F6CD0` | `int` | `g_voiceCommandLock` | garde anti-reentrance dans `TriggerVoice`, `StopVoice`, et les wrappers voix voisins |
| `0x801F7688` | `byte` | `g_loadedVoiceCount` | borne les boucles de voix backend; `TriggerVoice` rejette `AllocateVoiceSlot` si le retour egale cette valeur |
| `0x801F76B2` | `short` | `g_currentVoiceIndex` | pose avant `FUN_80091134`, `FUN_800912B4`, et les updates SPU |
| `0x801F7798` | `short stride 0x10` | `g_spuVoiceVolumeLeft` | ecrit par `SetVoiceVolume(voiceId, left, right)` avec index `voiceId << 4` |
| `0x801F779A` | `short stride 0x10` | `g_spuVoiceVolumeRight` | ecrit par `SetVoiceVolume(voiceId, left, right)` avec index `voiceId << 4` |
| `0x801F779C` | `short stride 0x10` | `g_spuVoicePitch` | ecrit par `FUN_80090C58` et par le wrapper qui appelle `CalculateVoicePitch` |
| `0x801F779E` | `short stride 0x10` | `g_spuVoiceReverb` | ecrit depuis les champs VAB program offsets `0x0C/0x0E` selon `g_sequenceKey` bit `0` |
| `0x801F77A0` | `short stride 0x10` | `g_spuVoiceAdsr1` | ecrit depuis `VabToneAttr + 0x10` par `FUN_800912B4` |
| `0x801F77A2` | `short stride 0x10` | `g_spuVoiceAdsr2` | ecrit depuis `VabToneAttr + 0x12`, additionne avec `g_sequenceKey @ 0x801F7610` |
| `0x801F7918` | `byte[24]` | `g_spuVoiceDirtyFlags` | `SetVoiceVolume` OR `0x3`; `FUN_800912B4` OR `0x8`; `FUN_80090C58` OR `0x7` |
| `0x801F7930` | `VoiceRuntimeSlot[24], stride 0x34` | `g_voiceRuntimeSlots` | `TriggerVoice`, `AllocateVoiceSlot`, et le code init calculent `voice * 0x34` |

Layout partiel d'un `VoiceRuntimeSlot` sous `0x801F7930 + voice * 0x34`:

| Offset | Type | Sens partiel |
|---:|---|---|
| `+0x00` | `ushort` | champ VAB/tone derive de `VabToneAttr + 0x16`, initialise a `0x00FF` |
| `+0x02` | `short` | age/ordre de remplacement; incremente pour toutes les voix dans `AllocateVoiceSlot`, remis a 0 pour la voix choisie |
| `+0x04` | `short` | valeur passee a `FUN_80090C58`/note partielle, recopiee dans `0x801F779C` |
| `+0x06` | `ushort` | critere de remplacement compare avec la priorite courante; mis a `0x7FFF` par `FUN_800912B4` |
| `+0x0C` | `short` | parametre tonal stocke par `TriggerVoice`, compare par les wrappers de stop conditionnel |
| `+0x0E` | `short` | cle sequence proprietaire: `0x21` pour voix SFX directe, sinon `(track << 8) | seqId`; comparee par `UpdateSequenceVolumeBalance`, `FUN_8009410C`, `FUN_80093EF4`, et les stops conditionnels |
| `+0x10` | `short` | copie de `g_currentVabFirstToneIndex` |
| `+0x12` | `short` | program/tone input de `TriggerVoice`, compare par les wrappers voix |
| `+0x14` | `short` | tone index courant stocke par `TriggerVoice` |
| `+0x16` | `short` | VAB id stocke par `TriggerVoice` et compare avant stop conditionnel |
| `+0x18` | `short` | priorite effective utilisee par `AllocateVoiceSlot` |
| `+0x1B` | `byte` | etat actif/noise: `0` libre, `1` voix tonale active, `2` voix noise active; les chemins `UpdateSoundVoicesState`, `AllocateVoiceSlot`, et `FUN_800914CC` traitent specialement la valeur `2` |

### `VabProgramAttr` partiel

`CopyVabProgramAttributes @ 0x800901A8` copie les octets `0x00..0x04` et le halfword `0x06` du programme courant vers la sortie. `SelectLoadedVabProgram` lit aussi `programAttr + 0x08` comme premier index tone. Les noms exacts des champs restent partiels; la structure doit garder les offsets bruts tant que les semantiques PSX driver ne sont pas fermees.

Champs fermes par acces:

- `0x00..0x04`: octets copies tels quels;
- `0x06`: `ushort` copie tel quel;
- `0x08`: `byte` utilise comme `firstToneIndex` dans la table des tones.

### `VabToneAttr` partiel

`CopyVabToneAttributes @ 0x80090370` selectionne le VAB/programme puis copie les donnees de tone depuis `g_currentVabToneAttrs + ((firstToneIndex << 4) + toneIndex) * 0x20`.

Champs confirmes par correspondance avec les attributs VAB:

- `0x00`: priority;
- `0x01`: mode;
- `0x02`: volume, stocke dans `g_voiceToneVolume`;
- `0x03`: pan, stocke dans `g_voiceTonePan`;
- `0x04..0x0D`: bytes copies;
- `0x10`, `0x12`, `0x14`, `0x16`: halfwords copies.

`CalculateVoicePitch @ 0x80091C18` confirme que les octets `0x04` et `0x05` participent au calcul de pitch avec la note demandee et la table halfword compacte `0x800C9798` (`lui 0x800d` + offset signe `0x9798`). Les noms PSX exacts de ces deux champs restent partiels, donc ils ne sont pas encore renommes.

## Chemin voix SFX

1. `PlaySoundEffect @ 0x800490FC` sort immediatement si `g_soundEffectState != 0`, si `sfxId <= 0`, si `sfxId` depasse la borne globale lue sous `0x80026848`, ou si `IsSoundEffectAlreadyPlaying @ 0x80048DF4` retourne non-zero.
2. La fonction appelle ensuite `SyncSoundEffectVoiceStates @ 0x80048FCC`, calcule `SfxRecord = 0x800A82E8 + sfxId * 0x16`, et ignore le record si `VabId == -2`.
3. Si `VabId == -1`, le record utilise le VAB global `g_globalSoundVabId`. Si `SeqNum != -1` et que `Flags & 0x2` est nul, le chemin charge une sequence SFX via `LoadSeq @ 0x8008BC00`, stocke le slot sequence dans la table sous `0x80175D00 + SeqNum * 2`, appelle `PlaySeq @ 0x8008F188(seqSlot, 1, 1)`, pose `Flags |= 0x2`, puis sort.
4. Si `VabId == -1` et `SeqNum == -1`, le chemin direct VAG appelle `CountActiveVoicesForSfx @ 0x80049060`; si le resultat est superieur ou egal a `MaxVoices`, il imprime un message via `sprintf @ 0x80082918` et sort. Sinon il boucle sur `ToneCount` et appelle `TriggerVoice @ 0x80094660(g_globalSoundVabId, ProgramNumber, ToneNumber + toneIndex, Note, 0, 0x7F, 0x7F)`.
5. Si `VabId != -1`, le code compare le record au groupe son courant sous `0x80173848`. Si le record ne correspond pas, il suit la chaine `RefSfxId` jusqu'a trouver un record dont `VabId` matche le groupe courant; si la chaine tombe a `0`, il imprime un message via `sprintf` et sort.
6. Pour un record map avec `SeqNum != -1` et `Flags & 0x2 == 0`, le chemin charge une sequence depuis `0x80173850 + (g_soundEffectSeqOffsets[SeqNum] & ~3)`, utilise `g_mapSoundVabId`, stocke le slot sous `0x80175D00 + SeqNum * 2`, appelle `PlaySeq(seqSlot, 1, 1)`, pose `Flags |= 0x2`, puis sort.
7. Pour un record map direct VAG, la logique de limite `MaxVoices`, de boucle `ToneCount`, et d'appel `TriggerVoice` est identique au chemin global, mais avec `g_mapSoundVabId` comme VAB charge. Le code ecrit aussi une table byte sous `0x80165130 + sfxId * 4 + toneIndex`; le sens exact de cette table reste partiel.
8. Apres un `TriggerVoice` reussi, `PlaySoundEffect` pose `SfxRecord.Flags |= 0x1`, met `g_voiceState[voice] = 0x80`, puis remplit `g_voiceSfxId`, `g_voiceVabId`, `g_voiceToneIndex`. Il appelle `CopyVabToneAttributes @ 0x80090370` et stocke les octets tone `+0x02/+0x03` dans `g_voiceToneVolume/g_voiceTonePan`.
9. Si `TriggerVoice` retourne negatif dans le chemin map direct, la fonction imprime un message via `sprintf`, puis appelle `0x800815EC` sur ce buffer avant de continuer la boucle.
10. `PlaySoundEffectWithToneVolumeMix @ 0x80049794` ne lance aucune nouvelle voix: elle cherche les voix existantes par SFX/tone et appelle `SetVoiceVolume @ 0x80095298`.

## Chemin map/BGM

Les xrefs depuis `LoadMapSounds @ 0x8004A09C` et `HandleMapSoundEffects @ 0x80049F1C` ferment le role de plusieurs fonctions map audio.

1. `GetMapSoundIndex @ 0x80049D3C` retourne l'index son de map. Il consulte d'abord `g_SoundOffsetList @ 0x800A81E4`; chaque entree fait 0x0C octets: `MapId`, `FlagSpec`, `SoundIndex`. Si le bit de save/temporaire reference par `FlagSpec` est pose, `SoundIndex` remplace la table par defaut `0x800C659C`.
2. `LoadMapSequence @ 0x80049BE0` stoppe/reset la sequence courante, appelle `FreeLoadedVab` sur le VAB courant, lit la tranche `SOUND.BIN` depuis la table `0x800A7F90`, appelle `LoadSeq`, puis stocke `g_requestedSeqId @ 0x80165128` et `g_currentMapSoundIndex @ 0x80173844`.
3. `LoadMapSounds` recharge la BGM si `GetMapSoundIndex(mapId)` change et si l'index n'est pas `0x2D`, puis appelle `FUN_8008F808(g_requestedSeqId, 0x7F, 10)`.
4. `HandleMapSoundEffects` est appele depuis `MainLoop`; il appelle `ResetSoundEffectRuntime`, gere le delai `g_soundEffectState`, appelle `LoadBgm(0)` en cas de changement map avec SFX actif, puis lance `PlaySoundEffect`.
5. `WaitForSoundEffectsIdle @ 0x80049FF8` attend la fin de `g_soundEffectState`, attend `AreSoundEffectsIdle`, puis fait 3 frames audio supplementaires.
6. `InitializeSoundSystem @ 0x800484E8` remet explicitement a zero `g_voiceState @ 0x80175858` et `g_voiceSfxId @ 0x80175870` dans la meme boucle d'init; le port C# doit donc nettoyer les deux tables ensemble.
7. La remise a zero de la table de dedup `0x80165028` utilisee par `IsSoundEffectAlreadyPlaying @ 0x80048DF4` n'a pas encore ete retrouvee par xrefs locaux dans `InitializeSoundSystem`, `MainLoop`, `UpdateWorld`, ni dans le helper audio `0x8008E034`; la fonction est fermee au niveau controle de flux, mais son integration reste bloquee tant que ce cycle de vie n'est pas prouve.

## Structure sequence partielle

La passe BGM/sequence a suivi les xrefs de `PlaySeq @ 0x8008F188`, `StartSequencePlayback @ 0x8008F088`, `SetSeqVolume @ 0x8008F23C`, `FUN_8008F760`, et `FUN_8008F808`.

Label pose:

| Adresse | Type | Label | Preuve |
|---|---|---|---|
| `0x801F6CD8` | `uint` | `g_sequenceSlotMask` | `LoadSeq` refuse `-1`, cherche un bit libre, puis pose le bit du slot choisi; `FUN_8008E3D8` ne traite que les bits poses |
| `0x801F6CE0` | `int` | `DAT_801F6CE0` | utilise par `FUN_8008F4AC` dans le calcul d'un champ sequence; sens exact partiel |
| `0x801F6CE8` | `SequenceTrackState*[g_sequenceSlotCount]` | `g_sequenceStatePointers` | `FUN_8008EEAC(base, slotCount, trackCount)` initialise `ptr[i] = base + i * trackCount * 0xAC`; les fonctions sequence indexent ensuite `ptr[seqId] + track * 0xAC` |
| `0x801F7568` | `short` | `g_sequenceSlotCount` | borne la boucle externe de `FUN_8008E3D8` sur les slots sequence |
| `0x801F7570` | `short` | `g_sequenceTrackCount` | borne la boucle interne de `FUN_8008E3D8` sur les sous-slots/tracks, stride `0xAC` |

`LoadSeq @ 0x8008BC00` alloue un slot sequence via `g_sequenceSlotMask`. Si le masque vaut `-1`, il imprime une erreur et retourne `-1`. Sinon, il cherche le premier bit a `0`, pose ce bit, appelle `FUN_8008B8C8(slot, vabId)`, et retourne le slot si l'initialisation ne retourne pas `-1`.

Type cible ferme pour le port: `SequenceTrackState` de taille `0xAC`, pointe par `g_sequenceStatePointers`. Le port C l'a deja approxime sous le nom `AudioData`; pour rester fidele au runtime PSX, il faut garder les offsets bruts. Dans ce binaire, `InitializeSoundSystem` appelle `FUN_8008EEAC(0x80175A50, 4, 1)`: la table `g_sequenceStatePointers` contient donc 4 pointeurs vers 4 structures contigues, et chaque slot n'a qu'un track.

Layout partiel d'un `SequenceTrackState`:

| Offset | Type | Sens partiel |
|---:|---|---|
| `+0x04` | `byte*` | pointeur courant de lecture sequence; `FUN_8008BDD0` lit un octet puis l'incremente |
| `+0x2B` | `byte` | bascule par `FUN_8008EB5C`/`FUN_8008EC24`; sens exact partiel |
| `+0x3E` | `short` | volume cible ou valeur de transition ecrite par `FUN_8008F690` |
| `+0x40` | `short` | valeur courante/copie initiale de volume |
| `+0x42` | `short` | pas de transition calcule selon `fadeTicks` et delta volume |
| `+0x44` | `short` | pas ou cadence utilise par `FUN_8008F4AC` |
| `+0x4A` | `short` | facteur utilise par `FUN_8008F4AC` dans le calcul de `+0x70` |
| `+0x6E` | `short` | compteur/reload partiel pour l'avancement sequence dans `FUN_8008BCC4` |
| `+0x70` | `short` | periode ou cadence sequence; remis depuis `+0x72` par `FUN_8008F2E8` |
| `+0x78` | `short` | volume/copie courante gauche; remis a `0x7F` par `FUN_8008F2E8`, mis a jour via `FUN_80093DE8` |
| `+0x7A` | `short` | volume/copie courante droite; remis a `0x7F` par `FUN_8008F2E8`, mis a jour via `FUN_80093DE8` |
| `+0x88` | `int` | accumulateur d'avancement sequence, copie de `+0x7C` au reset |
| `+0x8C` | `int` | valeur courante de transition/modulation pour `FUN_8008F4AC`, copie de `+0x84` au reset |
| `+0x90` | `uint` | flags sequence; `FUN_8008F690` refuse si bits `0x4` ou `0x100` sont poses; `FUN_8008F760` pose `0x10` et efface `0x20`; la fonction voisine `0x8008F870` fait l'inverse |
| `+0x94` | `int` | fade ticks demandes |
| `+0x98` | `int` | ticks restants ou compteur de fade |
| `+0xA0` | `int` | ticks restants pour `FUN_8008F4AC` |
| `+0xA4` | `int` | cible de `+0x8C` pour `FUN_8008F4AC` |

`FUN_8008F808(seqId, volume, fadeTicks)` est ferme comme wrapper vers `FUN_8008F760(seqId, 0, volume, fadeTicks)`. Le nom fonctionnel exact de `FUN_8008F760` reste partiel: le corps lance une transition de volume sur le sous-slot 0 puis modifie les flags `0x10/0x20`, mais le sens exact de ces deux modes n'est pas ferme.

### `FUN_8008E3D8 @ 0x8008E3D8` dispatcher/tick sequence

PARTIAL: le controle de flux et la hierarchie d'appels sont fermes, mais les noms exacts des handlers sequence restent partiels. La fonction est protegee par `g_voiceCommandLock @ 0x801F6CD0`: si le lock vaut `1`, elle retourne sans effet; sinon elle pose le lock, appelle `UpdateSoundVoicesState @ 0x8009311C`, traite les slots sequence actifs, puis remet le lock a `0`.

Boucles et selection:

1. La boucle externe parcourt `seqSlot = 0 .. DAT_801F7568 - 1`.
2. Un slot est ignore si `(g_sequenceSlotMask & (1 << seqSlot)) == 0`.
3. La boucle interne parcourt `track = 0 .. DAT_801F7570 - 1`.
4. L'adresse du track est `g_sequenceStatePointers[seqSlot] + track * 0xAC`.
5. Les handlers sont dispatches depuis le champ `track + 0x90`.

Ordre des flags dans `+0x90`:

| Flag | Fonction appelee | Effet ferme ou partiel |
|---:|---|---|
| `0x01` | `FUN_8008EBF8(seqSlot, track)` -> `FUN_8008BCC4` | avance le parseur sequence; les flags `0x10/0x20/0x40/0x80` ne sont testes que si ce bit est pose |
| `0x10` | `FUN_8008E610(seqSlot, track)` | transition de volume partielle; decompte `+0x98`, utilise `+0x40/+0x42/+0x94`, appelle `FUN_80093DE8` et `UpdateSequenceVolumeBalance @ 0x80093C78`, efface `0x10` quand terminee |
| `0x20` | `FUN_8008E8D0(seqSlot, track)` | transition de volume opposee/voisine; meme famille de champs, appelle `FUN_80093DE8` et `UpdateSequenceVolumeBalance`, efface `0x20` quand terminee |
| `0x40` | `FUN_8008F4AC(seqSlot, track)` | transition partielle sur `+0x8C` vers `+0xA4`, cadence `+0x44`, compteur `+0xA0`, recalcule `+0x70` |
| `0x80` | `FUN_8008F4AC(seqSlot, track)` | meme handler que `0x40`; quand la cible est atteinte, le handler efface `0x40` et `0x80` |
| `0x02` | `FUN_8008EB5C(seqSlot, track)` | appelle `FUN_80093EF4((track << 8) | seqSlot)`, met `+0x2B = 0`, efface `0x02` |
| `0x08` | `FUN_8008EC24(seqSlot, track)` | met `+0x2B = 1`, efface `0x08` |
| `0x04` | `FUN_8008F2E8(seqSlot, track)` | reset du track: efface plusieurs flags, appelle `FUN_80093EF4`, remet de nombreux champs a zero/valeurs par defaut; `FUN_8008E3D8` met ensuite `+0x90 = 0` |

Hierarchie appelee observee depuis `FUN_8008E3D8`:

| Niveau | Fonction | Role ferme/partiel |
|---:|---|---|
| 1 | `UpdateSoundVoicesState @ 0x8009311C` | update backend voix avant dispatch sequence; la descente atteint `ProcessVoiceStop @ 0x8009261C`, `FUN_80091134`, `SpuSetNoiseVoice` et des helpers SPU partiels |
| 1 | `FUN_8008EBF8 @ 0x8008EBF8` | wrapper vers `FUN_8008BCC4` |
| 2 | `FUN_8008BCC4 @ 0x8008BCC4` | gere l'accumulateur d'avancement sequence (`+0x6E/+0x70/+0x88`) et appelle `FUN_8008BDD0` tant que des commandes doivent etre consommees |
| 3 | `FUN_8008BDD0 @ 0x8008BDD0` | lit les octets de commande depuis `+0x04`; parse au moins les familles `0x90/0xB0/0xC0` et les commandes basses |
| 4 | `FUN_8008C064`, `FUN_8008C144`, `FUN_8008C1B8`, `FUN_8008D4C0`, `FUN_8008D568`, `FUN_8008D8D0` | handlers de commandes sequence; semantique complete non fermee dans cette passe |
| 1 | `FUN_8008E610 @ 0x8008E610` | handler flag `0x10`, transition volume partielle |
| 1 | `FUN_8008E8D0 @ 0x8008E8D0` | handler flag `0x20`, transition volume partielle |
| 1 | `FUN_8008EB5C @ 0x8008EB5C` | handler flag `0x02`, clear `+0x2B`, appelle `FUN_80093EF4` |
| 1 | `FUN_8008EC24 @ 0x8008EC24` | handler flag `0x08`, set `+0x2B` |
| 1 | `FUN_8008F2E8 @ 0x8008F2E8` | reset/initialisation d'un track sequence |
| 1 | `FUN_8008F4AC @ 0x8008F4AC` | handler flags `0x40/0x80`, transition/modulation partielle |
| 2 | `UpdateSequenceVolumeBalance @ 0x80093C78` | applique une paire de valeurs volume/balance au track selectionne |
| 2 | `FUN_80093DE8 @ 0x80093DE8` | lit ou synchronise une paire de valeurs sequence dans les buffers fournis |
| 2 | `FUN_80093EF4 @ 0x80093EF4` | stop/clear partiel pour la cle `(track << 8) | seqSlot` |

## Bloque ou partiel

- `FUN_800912B4`, `FUN_80090C58`, et `FUN_800914CC` sont lies au backend SPU/voix; leurs controles de flux sont documentes partiellement, mais leurs noms fonctionnels exacts restent bloques.
- `FUN_8008E3D8` est ferme comme dispatcher/tick sequence+voix, mais son nom fonctionnel exact reste partiel; aucun label primaire n'a ete propose en dehors de cette description.
- `FUN_8008E610`, `FUN_8008E8D0`, `FUN_8008F4AC`, `FUN_8008BCC4`, `FUN_8008BDD0`, et les handlers `FUN_8008C064/FUN_8008C144/FUN_8008C1B8/FUN_8008D4C0/FUN_8008D568/FUN_8008D8D0` restent bruts tant que les commandes sequence exactes ne sont pas fermees.
- `FUN_8008F760` et `FUN_8008F808` restent partiels cote nommage, meme si leurs acces a `g_sequenceStatePointers` et aux champs de fade sont documentes.
- `ResetSoundEffectRuntime`, `SyncSoundEffectVoiceStates`, `LoadMapSequence`, et `GetMapSoundIndex` ont ete ajoutes comme labels, mais les anciens primaires Ghidra restent visibles pour ces adresses.
- `FreeLoadedVab` a ete ajoute comme label, mais l'ancien primaire `MaybeFreeSound` reste visible dans Ghidra.
- `CopyVabToneAttributes` a ete ajoute comme label, mais `GetVoiceVolumes` reste label primaire dans Ghidra. Le primaire devrait etre corrige manuellement ou via une operation de rename stricte si disponible.
- `StopSoundEffect` et `CountActiveVoicesForSfx` ont ete ajoutes comme labels, mais les anciens primaires `PlaySoundEffect` et `FindAvailableSoundBank` restent prioritaires dans Ghidra.
- Aucune recherche globale de decompilation n'a ete lancee; les fonctions non atteintes par ces xrefs restent hors scope de cette passe.
