# FunctionTypeC AI Transliteration Plan Status

## Goal

Transliterate the requested `FunctionTypeC` handlers from Ghidra into C# with the repository's existing low-level bridges, then validate with a bounded targeted build.

## Executed Tasks

- ✅ Resolve slot mappings and Ghidra addresses for all requested handlers.
- ✅ Confirm the `AI_UpdateEntityDelayedSoundTrigger` / `AI_UpdateEntityDelayed` slot conflict from the ProgramCTick table.
- ✅ Transliterate `AI_UpdateEntityAI_19`, `AI_UpdateEntityAI_20`, `AI_UpdateEntityAI_20_2`, `AI_UpdateEntityAI_21`, `AI_UpdateEntityAI_23`, and `AI_UpdateEntityAI_23_2`.
- ✅ Transliterate `AI_UpdateEntityAI_Boss`, `AI_UpdateEntityAI_BoosPhase3`, `AI_UpdateEntityAI_SpecialBoss`, `AI_UpdateEntityIA_WatcherBehavior`, and `AI_UpdateEntityAI_WarpBoss`.
- ✅ Transliterate `AI_UpdateEntityDelayedSoundTrigger`, `AI_UpdateLoaderBossAI`, `AI_UpdateEntityIA_Fire`, `AI_ApplyMatchingEntity`, `AI_UpdateHomingProject`, `AI_UpdateFollowerBehaviour`, and `AI_UpdateEntityDelayed`.
- ✅ Expose `DAT_8019119c` for `SpecialBoss`.
- ✅ Re-type `0x80191204..0x80191234` as `PTR_ARRAY_80191204` for `Fire`.
- ✅ Correct `g_loaderEffectEntityId` to the `SpriteEffect` surface used by `LoaderBossAI`.
- ✅ Keep the requested debug guard at the top of every requested function.
- ✅ Validate the final result with a targeted `dotnet build` on `AlundraGame.csproj`.

## Final Status By Function

- ✅ `AI_UpdateEntityAI_19`
- ✅ `AI_UpdateEntityAI_20`
- ✅ `AI_UpdateEntityAI_20_2`
- ✅ `AI_UpdateEntityAI_21`
- ✅ `AI_UpdateEntityAI_23`
- ✅ `AI_UpdateEntityAI_23_2`
- ✅ `AI_UpdateEntityAI_Boss`
- ✅ `AI_UpdateEntityAI_BoosPhase3`
- ✅ `AI_UpdateEntityAI_SpecialBoss`
- ✅ `AI_UpdateEntityIA_WatcherBehavior`
- ✅ `AI_UpdateEntityDelayedSoundTrigger`
- ✅ `AI_UpdateEntityAI_WarpBoss`
- ✅ `AI_UpdateLoaderBossAI`
- ✅ `AI_UpdateEntityIA_Fire`
- ✅ `AI_ApplyMatchingEntity`
- ✅ `AI_UpdateHomingProject`
- ✅ `AI_UpdateFollowerBehaviour`
- ✅ `AI_UpdateEntityDelayed`

## Residual Notes

- ⚠️ The public C# names for slot `46` and slot `87` remain those already wired in `SpriteEventHandlers.cs`; the exact Ghidra semantics are documented in `docs/functiontypec-ai-transliteration-notes.md`.
- ⚠️ `AI_UpdateEntityAI_WarpBoss` still uses the current repository names `PreviousAdjustedForceX/Y` for original raw slots `+0xCC/+0xD0`, because those names are the closest exposed bridge already present.