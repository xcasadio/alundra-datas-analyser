# FunctionTypeC AI Transliteration Notes

## Scope

These notes capture the evidence and repository-specific bridges used to transliterate the following `FunctionTypeC` handlers in `AlundraTools/AlundraEngine/Gameplay/Scripts/FunctionTypeC.cs`:

- `AI_UpdateEntityAI_19`
- `AI_UpdateEntityAI_20`
- `AI_UpdateEntityAI_20_2`
- `AI_UpdateEntityAI_21`
- `AI_UpdateEntityAI_23`
- `AI_UpdateEntityAI_23_2`
- `AI_UpdateEntityAI_Boss`
- `AI_UpdateEntityAI_BoosPhase3`
- `AI_UpdateEntityAI_SpecialBoss`
- `AI_UpdateEntityIA_WatcherBehavior`
- `AI_UpdateEntityDelayedSoundTrigger`
- `AI_UpdateEntityAI_WarpBoss`
- `AI_UpdateLoaderBossAI`
- `AI_UpdateEntityIA_Fire`
- `AI_ApplyMatchingEntity`
- `AI_UpdateHomingProject`
- `AI_UpdateFollowerBehaviour`
- `AI_UpdateEntityDelayed`

All of these handlers keep the same debug guard at the top:

```csharp
if (!string.IsNullOrEmpty(entity.Name))
{
    Breakpoint.TriggerBreak();
}
```

## Closed Mappings

| C# method | Slot | GHIDRA symbol | Address | Notes |
| --- | ---: | --- | --- | --- |
| `AI_UpdateEntityAI_19` | 28 | `AI_UpdateEntityAI_19` | `0x8006C5CC` | Closed earlier in this batch. |
| `AI_UpdateEntityAI_20` | 29 | `AI_UpdateEntityAI_20` | `0x8006CA40` | Closed earlier in this batch. |
| `AI_UpdateEntityAI_20_2` | 31 | `AI_UpdateEntityAI_20_2` | `0x8006D550` | Closed earlier in this batch. |
| `AI_UpdateEntityAI_21` | 32 / 34 | `AI_UpdateEntityAI_21` | `0x8006D998` | Second parameter reconstruction remained probable, flow closed. |
| `AI_UpdateEntityAI_23` | 35 | `AI_UpdateEntityAI_23` | `0x8006E028` | Closed earlier in this batch. |
| `AI_UpdateEntityAI_23_2` | 37 | `AI_UpdateEntityAI_23_2` | `0x8006E6E8` | Closed earlier in this batch. |
| `AI_UpdateEntityAI_Boss` | 39 | `AI_UpdateEntityAI_Boss` | `0x8006FC7C` | Boss phase 1 main handler. |
| `AI_UpdateEntityAI_BoosPhase3` | 43 | `AI_UpdateEntityAI_BoosPhase3` | `0x80071164` | Case 8 overwrite rechecked in ASM. |
| `AI_UpdateEntityAI_SpecialBoss` | 44 | `AI_UpdateEntityAI_SpecialBoss` | `0x80071BF4` | Needs `DAT_8019119c`. |
| `AI_UpdateEntityIA_WatcherBehavior` | 45 | `AI_UpdateEntityIA_WatcherBehavior` | `0x8007252C` | Uses the shared boss completion flag. |
| `AI_UpdateEntityDelayedSoundTrigger` | 46 | `AI_UpdateEntityAI_TwinBoss` | `0x80072728` | Current C# method name follows slot mapping, not Ghidra semantics. |
| `AI_UpdateEntityAI_WarpBoss` | 48 | `AI_UpdateEntityAI_WarpBoss` | `0x80074D00` | `+0xCC/+0xD0` mapped to `PreviousAdjustedForceX/Y`. |
| `AI_UpdateLoaderBossAI` | 50 | `AI_UpdateLoaderBossAI` | `0x80076DA0` | Uses `g_loaderEffectEntityId` as `SpriteEffect`. |
| `AI_UpdateEntityIA_Fire` | 54 | `AI_UpdateEntityIA_Fire` | `0x80078E34` | Needs the raw pointer block at `0x80191204`. |
| `AI_ApplyMatchingEntity` | 64 | `AI_ApplyMatchingEntityKnockback` | `0x8007ADDC` | Name is slightly more semantic in Ghidra. |
| `AI_UpdateHomingProject` | 65 | `AI_UpdateHomingProjectileBehavior` | `0x8007AF20` | Nearby entity filter inlined locally. |
| `AI_UpdateFollowerBehaviour` | 81 | `AI_UpdateFollowerBehaviorIfTriggered` | `0x800749A4` | Triggered follower shutdown behavior. |
| `AI_UpdateEntityDelayed` | 87 | `AI_UpdateEntityDelayedSoundTrigger` | `0x80072680` | Slot address is certain; name conflict with slot 46 was resolved by table XREFs. |

## Repository-Specific Bridges

### Shared boss-completion flag

Earlier Ghidra notes referred to `g_globalFlags[0]`.
In this repository, the equivalent already exposed surface is `StaticVariables.g_temporaryFlags[0]`.

Use:

- `g_temporaryFlags[0] |= 1` for the boss-complete path used by boss watcher / delayed handlers.
- `g_temporaryFlags[0] |= 2` for the loader boss side flag.

### SpecialBoss radial counter

`AI_UpdateEntityAI_SpecialBoss` requires the raw global:

- `StaticVariables.DAT_8019119c // 0x8019119C`

This counter is incremented by `8` during the radial projectile phase and reset to `0` when the fade path starts.

### Fire pointer block

`AI_UpdateEntityIA_Fire` proved that `0x80191204..0x80191234` is a contiguous block of `13` entity pointers.

The current repository surface is now:

```csharp
public Entity?[] PTR_ARRAY_80191204 = new Entity?[13]; // 80191204..80191234
```

The function clears slots `1..12` and uses them as the chained spawn list for the `0xBF` wave.

### Loader effect surface

`g_loaderEffectEntityId` is not an `Entity`. It is a `SpriteEffect`.

The raw writes in `AI_UpdateLoaderBossAI` map to:

- `g_loaderEffectEntityId.X = entity.PosX`
- `g_loaderEffectEntityId.Y = entity.PosY`
- `g_loaderEffectEntityId.Z = entity.PosZ + 0x400000`

This closes the previous false blocker around a supposed `Entity+0x44` scratch field.

### Stored entity references inside AIValues

Two places needed a C# bridge for original raw pointers stored in entity state:

- `AI_UpdateEntityDelayedSoundTrigger` (`AI_UpdateEntityAI_TwinBoss`) resolves the paired entity from `AIValues[2]` using the entity slot index bridge already used elsewhere in the repository.
- `AI_UpdateEntityIA_Fire` uses minimal helper functions to store / recover an attached entity via `AIValues[2]` as a slot index.

These helpers are language bridges only. They do not change the original control flow.

## Important Behavioral Notes

- Slot `46` and slot `87` were the main naming trap in this batch.
  Slot `46` points to `0x80072728`, named `AI_UpdateEntityAI_TwinBoss` in Ghidra.
  Slot `87` points to `0x80072680`, named `AI_UpdateEntityDelayedSoundTrigger` in Ghidra.
  The current C# public method names remain aligned with existing slot registration, not with Ghidra semantics.

- `AI_UpdateEntityAI_BoosPhase3` case `8` was rechecked against ASM.
  The temporary write `TargetAnimationId = 3; AIValues[1] = 0x22;` is immediately overwritten by `TargetAnimationId = 0; AIValues[1] = 0x28;` on that path.

- `AI_UpdateEntityAI_WarpBoss` writes original raw slots `+0xCC/+0xD0`.
  In the current model, the least lossy existing mapping is `PreviousAdjustedForceX` / `PreviousAdjustedForceY`.

- `AI_UpdateLoaderBossAI` pulls its spawn patterns from `SHORT_ARRAY_80027d18` with an effective base offset of `+12` bytes into the exposed table.

## Validation

Targeted validation used:

```powershell
dotnet build .\AlundraTools\AlundraGame\AlundraGame.csproj --no-restore "-clp:ErrorsOnly;Summary" -p:OutDir=.\artifacts\tmp-build\
```

Result after the final patch set: build succeeded, warnings only.