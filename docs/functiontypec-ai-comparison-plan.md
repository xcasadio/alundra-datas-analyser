# FunctionTypeC AI Comparison Plan Status

## Goal

Comparer la version C# actuelle des handlers `FunctionTypeC` demandes avec leur version originale PSX, en gardant une methode de comparaison locale, falsifiable et exploitable pour le portage ulterieur.

Ce document couvre uniquement le plan et l'inventaire de comparaison. Il ne ferme pas encore la semantique originale de chaque handler.

## Requested Scope

La demande contenait `AI_FUN_80077734` deux fois.
Le perimetre reel contient donc `21` fonctions distinctes:

- `AI_FUN_800647b0`
- `AI_FUN_80064d90`
- `AI_FUN_80065204`
- `AI_FUN_8006eb9c`
- `AI_FUN_8006f8e4`
- `AI_FUN_80073728`
- `AI_FUN_80074ae8`
- `AI_FUN_80075a3c`
- `AI_FUN_80077734`
- `AI_FUN_8007ac60`
- `AI_FUN_8007b3c4`
- `AI_FUN_8007b6ec`
- `AI_FUN_8007bd8c`
- `AI_FUN_8007d554`
- `AI_UpdateEntityAI_4`
- `AI_UpdateEntityAI_5`
- `AI_UpdateEntityAI_7`
- `AI_UpdateEntityAI_8`
- `AI_UpdateEntityAI_8_2`
- `AI_UpdateEntityAI_9`
- `AI_UpdateEntityAI_13`

## Executed Tasks

- ✅ Deduplicate the requested function list.
- ✅ Resolve the `ProgramCTick` slot mapping for every requested handler from `SpriteEventHandlers.cs`.
- ✅ Resolve the current C# anchor and GHIDRA address for every requested handler from `FunctionTypeC.cs`.
- ✅ Inventory the current C# implementation state for every requested handler.
- ✅ Group the requested handlers into local comparison batches by code locality and shared helpers.
- ✅ Define the comparison checklist and the cheapest discriminating check for each batch.
- ✅ Record the resulting inventory in `docs/functiontypec-ai-comparison-notes.md`.

## Current Status Summary

- `18` handlers are still pure stubs with only a debug guard or a single `Breakpoint.TriggerBreak()`.
- `1` handler already preserves a large commented original skeleton: `AI_FUN_80064d90`.
- `2` handlers contain partial runtime logic that must be compared before any rewrite: `AI_FUN_800647b0` and `AI_FUN_80074ae8`.

This makes the best comparison order obvious:

1. start with the partial handlers and the commented skeleton, because they already expose concrete state writes to verify
2. then compare the adjacent pure stubs inside the same local cluster
3. only after that widen to the generic AI slot stubs around `AI_UpdateEntityAI_4/5/7/8/8_2/9/13`

## Comparison Order

### Batch A: Melzas2 local cluster

Functions:

- `AI_FUN_800647b0`
- `AI_FUN_80064d90`
- `AI_FUN_80065204`

Why first:

- `AI_FUN_800647b0` already mutates entity flags, direction, delay, and destroy paths.
- `AI_FUN_80064d90` already preserves a substantial commented decomp skeleton.
- all three sit next to `AI_FUN_80064884` and `AI_FUN_80065100`, so the comparison surface is narrow and locally coherent.

Cheapest discriminating check:

- confirm the exact `TargetAnimationId`, `Bytes`, `AIValues`, `DelayOrAngle`, `Flags`, and spawn-table writes performed by `0x800647B0`, `0x80064D90`, and `0x80065204` in the original
- compare them only against the current local C# bodies and the adjacent Melzas2 helpers already present in the same file

### Batch B: mid-file enemy and summon cluster

Functions:

- `AI_FUN_8006eb9c`
- `AI_FUN_8006f8e4`
- `AI_FUN_80073728`
- `AI_FUN_80074ae8`
- `AI_FUN_80075a3c`
- `AI_FUN_80077734`

Why second:

- `AI_FUN_80074ae8` is partially ported and already exposes parent-child counter writes and `SpawnWarpEntity` side effects.
- the other functions in this range are still stubs, but they are surrounded by related implemented handlers that give immediate local comparison boundaries.

Cheapest discriminating check:

- compare the state machine on `TargetAnimationId`, `AIValues[1]`, `Bytes[0..3]`, parent `AIValues[4]`, and entity spawn/destruction side effects first
- only widen to movement or physics helpers if the original clearly escapes this local state-machine boundary

### Batch C: late-file environment and special-entity cluster

Functions:

- `AI_FUN_8007ac60`
- `AI_FUN_8007b3c4`
- `AI_FUN_8007b6ec`
- `AI_FUN_8007bd8c`
- `AI_FUN_8007d554`

Why third:

- they are all stubs today
- each sits next to a strong local anchor already ported: orbit helpers, save-book warp state, pushable pillar state, or nearby common helpers

Cheapest discriminating check:

- recover only the first-level helper calls, raw global writes, and `TargetAnimationId` transitions for each handler
- compare them against the immediate neighboring C# helpers before following deeper call chains

### Batch D: generic slot-handler cluster

Functions:

- `AI_UpdateEntityAI_4`
- `AI_UpdateEntityAI_5`
- `AI_UpdateEntityAI_7`
- `AI_UpdateEntityAI_8`
- `AI_UpdateEntityAI_8_2`
- `AI_UpdateEntityAI_9`
- `AI_UpdateEntityAI_13`

Why last:

- all seven are still stubs
- their main value comes from comparing them against the already ported neighboring slot handlers `AI_UpdateEntityAI_6`, `AI_UpdateEntityAI_6_2`, `AI_UpdateEntityAI_10`, `AI_UpdateEntityAI_11`, and `AI_UpdateEntityAI_12`

Cheapest discriminating check:

- close each handler's input gate, `TargetAnimationId` switch shape, timer fields, and movement-helper usage before trying to name higher-level behavior

## Required Comparison Checklist

Every per-function comparison row must cover all of the following, not only the main success path:

- entry guard and early-exit conditions
- `TargetAnimationId` state-machine transitions
- all mutated `Bytes[]`, `AIValues[]`, `DelayOrAngle`, `ItemState`, and flag fields
- spawned or destroyed entities and effects
- sound calls
- global or table reads and writes
- cleanup and failure paths

If the original handler only differs through a direct helper call, the comparison row must name that helper explicitly instead of collapsing it into a vague description.

## Deliverables For The Next Comparison Pass

- one original-vs-C# matrix row per function in `docs/functiontypec-ai-comparison-notes.md`
- one short comparison summary per batch
- one explicit blocked note whenever the local helper boundary is still not closed by evidence

## Planning Status

All planning tasks requested in this pass are complete.