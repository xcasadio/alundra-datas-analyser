# ReVa Structure Tool Playbook

Date: 2026-05-14

## But

Ce memo sert a retrouver rapidement les bons outils ReVa pour manipuler des structures dans Ghidra, sans retomber sur le probleme rencontre pendant la passe audio:

- partir sur `modify-structure-field` alors que la structure n'existe pas encore;
- confondre suppression d'un type de structure et suppression des donnees deja definies a une adresse;
- perdre du temps avec `tool_search` quand il renvoie un sous-ensemble incomplet ou trompeur.

Le contexte concret de cette passe etait le programme `"/ALUN_CD.EXE"` et les structures audio `SequenceTrackState` et `VoiceRuntimeSlot`.

## Regle principale

Toujours separer 4 operations differentes:

1. verifier qu'une definition C est valide;
2. creer ou modifier le type dans l'archive de types Ghidra;
3. appliquer ce type a une adresse memoire;
4. gerer les conflits de donnees deja definies a cette adresse.

Si ces 4 etapes sont melangees, on finit facilement avec le mauvais outil.

## Outils confirmes utiles dans cette session

### Validation et creation de type

- `mcp_reva_validate-c-structure`
  - usage: verifier qu'une definition C parse bien et qu'elle a la bonne taille avant de l'injecter.
  - bon reflexe: l'utiliser avant toute creation pour fermer la taille (`0xAC`, `0x34`, etc.).

- `mcp_reva_parse-c-structure`
  - usage: creer une structure a partir d'une definition C.
  - point important: dans cette session, quand plusieurs `typedef struct` etaient donnes en une seule fois, le retour n'a confirme que le dernier type cree. Pour eviter toute ambiguite, preferer un type par appel.

- `mcp_reva_create-structure`
  - usage: creer une structure vide ou une union vide.
  - utile quand on veut construire le layout champ par champ.

- `mcp_reva_add-structure-field`
  - usage: ajouter un champ a une structure existante.
  - utile pour un layout partiel, ou pour completer une structure deja creee vide.

- `mcp_reva_modify-structure-from-c`
  - usage: modifier une structure existante a partir d'une definition C complete.
  - c'est l'outil a privilegier quand la structure existe deja et qu'il faut la corriger en bloc.

- `mcp_reva_delete-structure`
  - usage: supprimer le type de structure dans l'archive de types.
  - attention: cela ne supprime pas les donnees deja appliquees en memoire a une adresse.

### Inspection de type

- `mcp_reva_list-structures`
  - usage: verifier qu'une structure a bien ete creee, sous quel nom, dans quelle categorie, et avec quelle taille.
  - bon reflexe: l'utiliser juste apres une creation ou une modification.

### Application sur la memoire du programme

- `mcp_reva_apply-structure`
  - usage: appliquer un type a une adresse du programme.
  - parametre cle: `clearExisting=true`.
  - limite observee: `clearExisting=true` ne supprime pas tous les conflits possibles. Si Ghidra a deja des donnees materialisees sur quelques octets dans la plage, l'appel peut encore echouer avec `Conflicting data exists at address ...`.

### Inspection brute pour comprendre un conflit

- `mcp_reva_read-memory`
  - usage: lire les bytes a une adresse pour verifier si l'on est bien au bon endroit, ou pour comprendre une collision de taille/layout.

## Outils non confirmes pour ce probleme precis

Pendant cette session, aucun outil ReVa confirme n'a ete trouve pour l'equivalent direct de:

- `undefine data at address`
- `clear conflicting data bytes`
- `delete data at address`

Important:

- `mcp_reva_delete-structure` supprime un type, pas les donnees deja definies dans le listing.
- si `mcp_reva_apply-structure(..., clearExisting=true)` echoue encore avec `Conflicting data exists`, il manque probablement un vrai tool d'undefine des donnees, ou il faut passer par Ghidra interactif / un script headless quand le projet n'est pas locke.

## Triage du verrou de projet

Avant de partir sur un script headless, verifier si le projet Ghidra principal est deja ouvert.

Checks simples utilises dans cette passe:

- verifier un processus Ghidra/Java actif:
  - `Get-Process | Where-Object { $_.ProcessName -match 'ghidra|java' } | Select-Object ProcessName,Id,Path`
- verifier la ligne de commande du processus Java:
  - `Get-CimInstance Win32_Process -Filter "ProcessId = <pid>" | Select-Object ProcessId,Name,CommandLine | Format-List`
- verifier les fichiers de lock du projet:
  - `Get-Item 'Ghidra\\Alundra.lock','Ghidra\\Alundra.lock~' -ErrorAction SilentlyContinue | Select-Object FullName,Length,LastWriteTime`

Resultat concret observe ici:

- un `javaw.exe` Ghidra etait actif;
- `Ghidra/Alundra.lock` etait present et recent;
- le lancement de `analyzeHeadless.bat` sur `Ghidra/Alundra` echouait avec `LockException: Unable to lock project!`.

Conclusion pratique:

- ne pas supprimer le lock a la main tant qu'un vrai processus Ghidra tourne;
- dans ce cas, le headless sur le projet principal est bloque par conception;
- il faut soit fermer Ghidra proprement, soit disposer d'un tool MCP capable d'executer le script dans l'instance Ghidra deja ouverte.
- un simple redemarrage de Ghidra ne suffit pas si le projet `Alundra` est rouvert ensuite: le headless reste bloque de la meme maniere.

### Cas specifique de `mcp_reva_checkin-program`

Pendant cette passe, `mcp_reva_checkin-program` a ete teste comme tentative non destructive pour liberer l'ecriture.

Resultat observe:

- echec avec `Attempted to release domain object with unknown consumer: {}`.

Conclusion:

- ne pas compter sur `mcp_reva_checkin-program` comme mecanisme de liberation de lock;
- ce tool est utile pour versionner un programme, pas pour fermer l'instance Ghidra ni lever un lock de projet tenu par le GUI.

## Fallback headless valide

Quand le projet n'est pas ouvert, le fallback valide est:

- script local: `obj/ghidra/UpdateAudioGhidra.py`
- lanceur: `D:\development\ghidra_12.0.1_PUBLIC\support\analyzeHeadless.bat`

Commande utilisee dans cette passe:

```powershell
& 'D:\development\ghidra_12.0.1_PUBLIC\support\analyzeHeadless.bat' `
  'D:\development\repo\alundra-datas-analyser\Ghidra' `
  'Alundra' `
  -process 'ALUN_CD.EXE' `
  -scriptPath 'D:\development\repo\alundra-datas-analyser\obj\ghidra' `
  -postScript 'UpdateAudioGhidra.py'
```

Ce fallback est correct techniquement, mais il ne peut pas contourner un projet deja locke par l'application Ghidra ouverte.

## Limites outillage observees dans cette passe

### `mcp_reva_create-label`

Teste pour pousser des noms fermes sur des fonctions deja analysees:

- `LoadMapSequence @ 0x80049BE0`
- `FreeLoadedVab @ 0x8008F9A4`
- `GetSoundGroupByMapId @ 0x80049F00`
- `GetMapSoundIndex @ 0x80049D3C`
- `ResetSoundEffectRuntime @ 0x80048E44`

Resultat observe:

- le tool cree bien le label;
- mais le retour peut rester `isPrimary: false` meme avec `setAsPrimary=true`.

Conclusion:

- ne pas considerer `mcp_reva_create-label` comme un rename primaire fiable dans cette configuration;
- il est utile pour ajouter un alias documente, pas pour garantir que le nom affiche dans Ghidra devienne le primaire.

### `mcp_reva_read-memory`

Tentative faite pour relire les zones conflictuelles en RAM:

- `0x80175CF8`
- `0x801F7930`

Resultat observe:

- `Memory access error at address` sur ces adresses runtime.

Conclusion:

- dans cette configuration, `mcp_reva_read-memory` n'est pas un outil fiable pour inspecter directement les adresses RAM du runtime PSX mappees dans l'analyse;
- il ne remplace donc ni une vraie lecture du listing Ghidra, ni un tool d'undefine applique a ces zones.

## Workflow recommande

### Cas A - nouvelle structure a creer depuis une preuve Ghidra/C

1. Ecrire la definition C minimale.
2. Appeler `mcp_reva_validate-c-structure`.
3. Si la taille est correcte, appeler `mcp_reva_parse-c-structure`.
4. Verifier le resultat avec `mcp_reva_list-structures`.
5. Appliquer a l'adresse avec `mcp_reva_apply-structure`.
6. Si l'application echoue a cause d'un conflit, ne pas repartir sur `delete-structure`. Le probleme n'est plus le type, mais les donnees deja definies dans le listing.

### Cas B - structure existante a corriger

1. Verifier qu'elle existe avec `mcp_reva_list-structures`.
2. Si la correction est globale, utiliser `mcp_reva_modify-structure-from-c`.
3. Si la correction est locale, utiliser `mcp_reva_add-structure-field` ou `modify-structure-field` si ce tool est expose dans la session.
4. Reappliquer ensuite le type a l'adresse si necessaire.

### Cas C - la mauvaise structure existe deja et doit etre remplacee

1. Supprimer le type avec `mcp_reva_delete-structure` seulement si c'est bien le type lui-meme qui est faux.
2. Recreer le bon type avec `mcp_reva_parse-c-structure` ou `mcp_reva_create-structure` + `mcp_reva_add-structure-field`.
3. Reappliquer a l'adresse.

### Cas D - l'application echoue avec `Conflicting data exists`

1. Ne pas confondre ce cas avec un probleme de type.
2. Lire la zone avec `mcp_reva_read-memory` si besoin.
3. Chercher explicitement un tool ReVa d'undefine data.
4. Si aucun tool n'est expose, basculer sur Ghidra interactif ou headless.

## Queries `tool_search` qui ont ete utiles

Les recherches en langage naturel ont mieux marche que certaines recherches trop exactes.

Queries utiles:

- `ReVa tool to create a new empty structure in Ghidra`
- `ReVa tool to apply a structure at an address in Ghidra`

Queries peu fiables ou trompeuses pendant cette session:

- `mcp_reva_parse-c-structure`
- `mcp_reva_list-structures`
- `ReVa tool to undefine existing data at an address in Ghidra`

Constat pratique: `tool_search` peut renvoyer un outil partiellement pertinent, ou un tool de la meme famille mais pas le bon. Quand un nom d'outil est deja connu par un succes precedent dans la session, il vaut mieux le reappeler directement que relancer `tool_search` en boucle.

## Sequence minimale recommande pour les structures

Version courte, a reutiliser telle quelle:

1. `mcp_reva_validate-c-structure`
2. `mcp_reva_parse-c-structure`
3. `mcp_reva_list-structures`
4. `mcp_reva_apply-structure`
5. si conflit: chercher un vrai tool d'undefine data, sinon passer par Ghidra interactif/headless

## Exemple reel de cette passe

Types crees avec succes:

- `SequenceTrackState` taille `0xAC`
- `SequenceTrackStateArray4`
- `SequenceTrackStatePointer`
- `SequenceTrackStatePointerTable`
- `VoiceRuntimeSlot` taille `0x34`

Applications reussies:

- `SequenceTrackStatePointer` a `0x801F6CE8`
- `SequenceTrackStatePointer` a `0x801F6CEC`
- `SequenceTrackStatePointer` a `0x801F6CF0`
- `SequenceTrackStatePointer` a `0x801F6CF4`
- `SequenceTrackState` a `0x80175A50`
- `SequenceTrackState` a `0x80175AFC`
- `SequenceTrackState` a `0x80175BA8`

Applications bloquees par conflit de donnees deja definies:

- `SequenceTrackState` a `0x80175C54`, conflit final sur `0x80175CFE-0x80175CFF`
- `VoiceRuntimeSlot` a `0x801F7930`, conflit sur `0x801F7932-0x801F7933`

Conclusion du cas reel:

- la creation de structures via ReVa fonctionnait bien;
- le mauvais diagnostic initial venait du choix d'outil;
- le blocage restant n'est plus un probleme de definition de type, mais un probleme d'undefine de donnees deja presentes.

## Anti-pattern a eviter

- Commencer par `modify-structure-field` sans verifier que la structure existe.
- Utiliser `delete-structure` en pensant que cela supprimera les donnees deja appliquees en memoire.
- Donner plusieurs structures dans un seul gros blob C sans verifier individuellement ce que ReVa a effectivement cree.
- Faire confiance aveuglement a `tool_search` quand il renvoie un outil proche mais pas le bon.

## Decision tree rapide

- Je veux verifier une taille avant creation:
  - `mcp_reva_validate-c-structure`
- Je veux creer un type depuis une definition C:
  - `mcp_reva_parse-c-structure`
- Je veux creer un squelette vide:
  - `mcp_reva_create-structure`
- Je veux completer un type champ par champ:
  - `mcp_reva_add-structure-field`
- Je veux corriger un type deja existant:
  - `mcp_reva_modify-structure-from-c`
- Je veux verifier que le type existe bien:
  - `mcp_reva_list-structures`
- Je veux poser le type a une adresse:
  - `mcp_reva_apply-structure`
- Je veux supprimer le type de l'archive:
  - `mcp_reva_delete-structure`
- Je veux supprimer des donnees deja definies a une adresse:
  - aucun tool confirme dans cette session
