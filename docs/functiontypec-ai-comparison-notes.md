# FunctionTypeC AI Comparison Notes

## Scope

These notes capture the current C# comparison inventory for the requested `FunctionTypeC` handlers in `AlundraTools/AlundraEngine/Gameplay/Scripts/FunctionTypeC.cs`.

They are not a transliteration log yet.
They freeze:

- the slot mapping
- the GHIDRA address
- the current C# state
- the narrowest local comparison boundary to use first

## Current Inventory

| C# method | Slot | Address | Current C# state | Compare locally with | Current comparison note |
| --- | ---: | --- | --- | --- | --- |
| `AI_FUN_800647b0` | 96 | `0x800647B0` | Partial body | `AI_FUN_80064884`, `AI_FUN_80065100`, `entity.ParentEntity.AIValues[4]` | Already mutates flags, direction, delay, and destroy/reflect paths. |
| `AI_FUN_80064d90` | 98 | `0x80064D90` | Commented skeleton | `AI_FUN_80065100`, `DAT_80191144`, `SHORT_80026F34/3C/60/84/B4`, `SpawnEntity` | Large commented decomp already preserved; best first original comparison candidate. |
| `AI_FUN_80065204` | 100 | `0x80065204` | Pure stub | `AI_FUN_80065100`, `AI_FUN_80065750` | Still only a guard; likely same local boss-family boundary. |
| `AI_FUN_8006eb9c` | 38 | `0x8006EB9C` | Pure stub | `AI_FUN_8006E83C`, `AI_FUN_8006F860` | Adjacent mid-file enemy helper boundary is already visible. |
| `AI_FUN_8006f8e4` | 42 | `0x8006F8E4` | Pure stub | `AI_FUN_8006F860`, `AI_FUN_80073728` | Compare only against local neighboring enemy-state handlers first. |
| `AI_FUN_80073728` | 85 | `0x80073728` | Pure stub | `AI_FUN_8006F8E4`, `AI_FUN_80074AE8` | Should be compared with the nearby summon/child-entity state cluster. |
| `AI_FUN_80074ae8` | 82 | `0x80074AE8` | Partial body | `SpawnWarpEntity`, `entity.ParentEntity.AIValues[4]`, `AI_FUN_800756EC`, `AI_FUN_8007763C` | Already exposes countdowns, parent counters, and 3-spawn burst behavior. |
| `AI_FUN_80075a3c` | 49 | `0x80075A3C` | Pure stub | `AI_FUN_800756EC`, `AI_FUN_8007763C` | Same local cluster as other partially ported summon handlers. |
| `AI_FUN_80077734` | 51 | `0x80077734` | Pure stub | `AI_FUN_8007763C`, `AI_FUN_80078A5C` | Compare against the neighboring Wilda-family state machine first. |
| `AI_FUN_8007ac60` | 63 | `0x8007AC60` | Pure stub | `AI_ApplyMatchingEntity`, `AI_FUN_8007B04C_common` | Late-file special-entity cluster with nearby shared helper already ported. |
| `AI_FUN_8007b3c4` | 68 | `0x8007B3C4` | Pure stub | `AI_FUN_8007B04C_common`, `AI_FUN_8007B7B0` | Compare after closing the local shared orbit/helper pattern. |
| `AI_FUN_8007b6ec` | 69 | `0x8007B6EC` | Pure stub | `AI_FUN_8007B7B0`, nearby environment handlers | Same late-file environment cluster as `0x8007B3C4`. |
| `AI_FUN_8007bd8c` | 75 | `0x8007BD8C` | Pure stub | `AI_UpdatePushablePillarPushState`, nearby environmental handlers | Compare locally against pillar / carried-object state surfaces first. |
| `AI_FUN_8007d554` | 84 | `0x8007D554` | Pure stub | `AI_FUN_8007C768`, `RandomRange`, `UpdatePZoldiaOrbitPosition` | Best local boundary is the already ported P-Zoldia helper cluster. |
| `AI_UpdateEntityAI_4` | 7 | `0x80067138` | Pure stub | `AI_UpdateEntityAI_6` | Start from shared slot-enemy state shape, not from the whole file. |
| `AI_UpdateEntityAI_5` | 8 | `0x8006790C` | Pure stub | `AI_UpdateEntityAI_6` | Same early generic-AI cluster as slot 4. |
| `AI_UpdateEntityAI_7` | 11 | `0x80068930` | Pure stub | `AI_UpdateEntityAI_6_2`, `AI_UpdateEntityAI_8`, `AI_UpdateEntityAI_9` | Compare as one contiguous generic state-machine block. |
| `AI_UpdateEntityAI_8` | 12 | `0x80068CC8` | Pure stub | `AI_UpdateEntityAI_6_2`, `AI_UpdateEntityAI_7`, `AI_UpdateEntityAI_8_2`, `AI_UpdateEntityAI_9` | Keep the boundary inside the generic slot cluster first. |
| `AI_UpdateEntityAI_8_2` | 13 | `0x80069684` | Pure stub | `AI_UpdateEntityAI_7`, `AI_UpdateEntityAI_8`, `AI_UpdateEntityAI_9` | Same generic slot cluster. |
| `AI_UpdateEntityAI_9` | 14 | `0x800699C4` | Pure stub | `AI_UpdateEntityAI_8`, `AI_UpdateEntityAI_10`, `AI_UpdateEntityAI_11` | Compare around the local enemy-AI slot chain. |
| `AI_UpdateEntityAI_13` | 20 | `0x8006ABB0` | Pure stub | `AI_UpdateEntityAI_10`, `AI_UpdateEntityAI_11`, `AI_UpdateEntityAI_12`, `CanMoveForward` | Best anchor is the nearby generic enemy-state cluster and movement helper. |

## Batch Boundaries

### Batch A: `0x800647B0..0x80065204`

Functions:

- `AI_FUN_800647b0`
- `AI_FUN_80064d90`
- `AI_FUN_80065204`

Shared local evidence to compare first:

- `AI_FUN_80064884`
- `AI_FUN_80065100`
- `entity.ParentEntity.AIValues[4]`
- `DAT_8019113c`
- `DAT_80191140`
- `DAT_80191144`
- spawn tables `SHORT_80026F34/3C/60/84/B4`

Primary comparison questions:

- Which exact boss-state transitions are still missing from the current C# bodies?
- Which spawn-table and timer writes are already preserved in comments but not yet executed in code?

### Batch B: `0x8006EB9C..0x80077734`

Functions:

- `AI_FUN_8006eb9c`
- `AI_FUN_8006f8e4`
- `AI_FUN_80073728`
- `AI_FUN_80074ae8`
- `AI_FUN_80075a3c`
- `AI_FUN_80077734`

Shared local evidence to compare first:

- `SpawnWarpEntity`
- `DestroyEntity`
- parent `AIValues[4]`
- `AI_FUN_800756EC`
- `AI_FUN_8007763C`
- `AI_FUN_80078A5C`

Primary comparison questions:

- Which handlers are pure child-effect/state relays versus full AI state machines?
- Which counters belong to the child entity and which ones must mutate the parent slot?

### Batch C: `0x8007AC60..0x8007D554`

Functions:

- `AI_FUN_8007ac60`
- `AI_FUN_8007b3c4`
- `AI_FUN_8007b6ec`
- `AI_FUN_8007bd8c`
- `AI_FUN_8007d554`

Shared local evidence to compare first:

- `AI_FUN_8007B04C_common`
- `AI_ProcessWarpTransitionState`
- `AI_UpdatePushablePillarPushState`
- `AI_FUN_8007C768`
- `RandomRange`
- `UpdatePZoldiaOrbitPosition`

Primary comparison questions:

- Are these handlers just thin state relays into nearby helpers, or do they own hidden side effects on globals / entity bytes?
- Which late-file globals are written directly by the original handlers and not by the neighboring helpers?

### Batch D: generic AI slot cluster

Functions:

- `AI_UpdateEntityAI_4`
- `AI_UpdateEntityAI_5`
- `AI_UpdateEntityAI_7`
- `AI_UpdateEntityAI_8`
- `AI_UpdateEntityAI_8_2`
- `AI_UpdateEntityAI_9`
- `AI_UpdateEntityAI_13`

Shared local evidence to compare first:

- `AI_UpdateEntityAI_6`
- `AI_UpdateEntityAI_6_2`
- `AI_UpdateEntityAI_10`
- `AI_UpdateEntityAI_11`
- `AI_UpdateEntityAI_12`
- `CanMoveForward`

Primary comparison questions:

- Which of these slot handlers share the same generic enemy-state skeleton as the neighboring implemented slots?
- Which ones introduce unique movement or attack helpers that force a one-hop comparison beyond the local slot cluster?

## Required Per-Function Comparison Row

Every future comparison row should close the following fields explicitly:

- entry guard
- early exits
- `TargetAnimationId` branches
- mutated `Bytes[]`
- mutated `AIValues[]`
- mutated flags and movement fields
- spawned or destroyed entities / effects
- sound calls
- global writes
- end-of-handler cleanup path

## Current Gap Summary

- Pure stubs: `18`
- Commented skeletons: `1`
- Partial handlers: `2`

This means the next productive comparison pass should start with `AI_FUN_800647b0`, `AI_FUN_80064d90`, and `AI_FUN_80074ae8`, because they already expose enough structure to falsify local hypotheses without reopening the whole `FunctionTypeC.cs` surface.