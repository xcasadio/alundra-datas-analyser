# FUN_80044440 warp transition notes

## Scope

This note covers the render-side warp transition path around:

- `FUN_80044440 @ 0x80044440`
- `FUN_800435e0 @ 0x800435E0`
- `FUN_800436a0 @ 0x800436A0`
- `FUN_8004392c @ 0x8004392C`
- `FUN_80043b34 @ 0x80043B34`
- `FUN_80043d54 @ 0x80043D54`
- `FUN_80043f8c @ 0x80043F8C`
- `StartWarpTransition @ 0x80044320`
- `InitializeCutsceneWarp @ 0x80043540`

Evidence used:

- Ghidra/ReVa jump tables at `0x80023E88` and `0x80023E28`
- PCSX function listings for `0x80043540`, `0x800435E0`, `0x800436A0`, `0x8004392C`, `0x80043B34`, `0x80043D54`, `0x80043F8C`, `0x80044440`
- Existing C# fade state in `RenderTransitionEffects @ 0x80042CCC`

## High-level behavior

`StartWarpTransition` captures the already-rendered scene, resets warp/fade globals, initializes `g_warpEffectBuffer` for the selected effect, and configures the fade targets. `FUN_80044440` then re-renders only the captured frame through one of several transition paths, while `RenderTransitionEffects` updates the shared fade overlay.

The key PSX behavior is not "render the next map with a shader". The game first freezes the current frame into a saved image, then all transition variants sample that frozen image.

In the C# port, the closest equivalent is:

1. render the gameplay scene normally
2. capture the current framebuffer/render target once in `StartWarpTransition`
3. during the transition, draw quads/slices from that captured bitmap
4. keep the original fade globals and counters unchanged

## Dispatch table closed from `0x80023E88`

`FUN_80044440` is a jump-table dispatcher. The mapping is:

| transitionEffectId | target stub | render function | observed meaning |
| --- | --- | --- | --- |
| 0 | `0x80044470` | `FUN_800435e0` | full-screen frozen frame + fade |
| 1 | `0x800444D0` | `FUN_800435e0`, forced return `0` | unused / immediate end |
| 2 | `0x80044470` | `FUN_800435e0` | same image render as type 0, different fade init |
| 3 | `0x800444D0` | `FUN_800435e0`, forced return `0` | unused / immediate end |
| 4 | `0x80044480` | `FUN_800436a0` | 20x15 grid implosion |
| 5 | `0x80044490` | `FUN_8004392c` | random horizontal scanline shear |
| 6 | `0x800444A0` | `FUN_80043b34` | 20x15 cell burst / drift |
| 7 | `0x800444D0` | `FUN_800435e0`, forced return `0` | unused / immediate end |
| 8 | `0x800444B0` | `FUN_80043d54` | delayed tile shrink pattern |
| 9 | `0x80044470` | `FUN_800435e0` | same image render as type 0 |
| 10 | `0x80044470` | `FUN_800435e0` | same image render as type 2 |
| 11 | `0x800444C0` | `FUN_80043f8c` | vertical line compression |

Fallback for ids outside `0..11` calls `FUN_800435e0` and forces return `0`.

## Render paths

### `FUN_800435e0`

- Calls `RenderTransitionEffects` and returns its state.
- Requeues a full-screen copy of the captured frame.
- Uses `g_effectRenderToggle` only for PSX double-buffered primitive memory.
- Desktop/MonoGame port can ignore the primitive-buffer detail and just draw the full captured image.

### `FUN_800436a0`

- Operates on a 20x15 grid, 4 shorts per cell in `g_warpEffectBuffer`.
- Uses:
  - `cell[0]`: counter, incremented every frame
  - `cell[1]`: duration
  - `cell[2]`: random X seed from `InitInstantWarpEffect`
  - `cell[3]`: random Y seed from `InitInstantWarpEffect`
- Computes `progress = ((duration - counter) << 16) / duration`.
- Converts that to `shrink = 8 - ((progress * 8) >> 16)`.
- Draws a cropped sub-rectangle from the frozen frame for each active cell.
- Returns whether any cell is still active, not the fade state.
- Also updates `g_renderEffectDoneFlag` and `g_renderEffectCompleted` from that activity bit.

Observed visual: the screen breaks into 16x16 cells and collapses toward the screen center.

### `FUN_8004392c`

- Calls `RenderTransitionEffects` and returns its state.
- Uses the global RNG once per scanline.
- Reads the first 32-bit word of `g_warpEffectBuffer` as a horizontal amplitude.
- For each of the 240 rows, computes a signed X offset and draws only the visible part of that source row.
- Increments that amplitude word by `0x80` every frame.

Observed visual: horizontal strips of the frozen frame slide left/right with increasing strength.

Init-side detail closed from `InitFadeOutWarp @ 0x8004320C`:

- The effect amplitude is cleared with a 32-bit `sw` at `g_warpEffectBuffer + 0`, not just a single short write.

### `FUN_80043b34`

- Operates on the same 20x15 grid, again 4 shorts per cell.
- Uses the first 32-bit word as a signed counter/delay word.
- Uses `cell[2]` as X motion scale and `cell[3]` as a Y accumulator.
- Draws only when the counter word is positive.
- Increments the Y accumulator by `counter << 2` each frame.
- Clips each 16x16 cell against the screen before drawing.
- Returns whether any cell was still visible.
- When a cell is already fully outside the screen bounds, the original skips the draw and also skips the `counter + 1` store for that frame.

Observed visual: cells drift away with per-cell motion and leave the screen progressively.

### `FUN_80043d54`

- Calls `RenderTransitionEffects` first.
- While fade is still active, it simply draws the full frozen frame and returns `1`.
- After fade ends, it switches to a 20x15 tile pass:
  - `cell word != 0`: countdown delay, decrement only
  - `cell word == 0`: `cell[2] += 0x80`, `shrink = cell[2] >> 8`
- Each tile is drawn from the same source cell but cropped inward on all sides.
- Returns whether at least one tile is still in the shrinking phase.

Observed visual: a delayed mosaic where tiles remain static for a per-cell delay, then contract inward.

Closed detail:

- When the effect finishes, the original sets `0x801FB424 |= 0x00400000`.
- In the current C# runtime, `0x801FB424` falls inside `g_heapBuffer @ 0x801F7F24`, at offset `0x3500`.
- The faithful bridge is therefore a raw little-endian word write on `g_heapBuffer[0x3500..0x3503]`.

### `FUN_80043f8c`

- Calls `RenderTransitionEffects` and returns its state.
- Uses `g_warpEffectBuffer[3]` as a vertical clamp line.
- Replays the frozen frame one source row at a time.
- Destination Y is `min(row, g_warpEffectBuffer[3])`.
- Decrements `g_warpEffectBuffer[3]` until it reaches `0`.

Observed visual: the lower rows collapse upward onto a shrinking line.

## Start/init side

### `StartWarpTransition @ 0x80044320`

Important order:

1. capture the current frame
2. reset `g_mapScreenPosX/Y`, `g_mapOffsetX/Y`, `g_fadeFrameCounter`, `g_fadeStepFlags`, `g_warpFlags`
3. dispatch to the init function for the selected `warpType`

The capture must happen before any transition frame is rendered. In PSX this is the `MoveImage(...)` copy of the current draw environment.

### `InitInstantWarpEffect @ 0x80042FDC`

Closed detail:

- Each cell consumes four sequential RNG states in the original:
  - one for the negative start counter
  - one for the duration
  - one for the random X seed
  - one distinct RNG step for the random Y seed
- Reusing the same RNG value for both X and Y is not faithful.

### `InitializeCutsceneWarp @ 0x80043540`

Closed facts:

- It initializes two 240-entry DR_MOVE strips in the original PSX backend.
- It sets `g_warpEffectBuffer[3] = 0x00EF`.
- It configures a fade with `ApplyScreenFade(2, 300)`.

The existing C# line that tried to write `0xEF << 16` into a single `short` was not faithful. The value that matters for the render path is the short at index `3`.

## Best MonoGame transliteration strategy

### What should stay in the runtime

- `FUN_80044440` remains the dispatcher.
- Each original callee remains a separate C# function.
- `g_warpEffectBuffer`, `g_effectRenderToggle`, fade globals, and timers stay in `StaticVariables`.
- Geometry decisions stay in `GraphicManager`, not in `Game1` or a new effect manager.

### What belongs to the backend boundary

- Capturing the current frame from the active render target.
- Replaying cropped rows/cells of that frozen frame as textured quads.

This is why the desktop bridge added `IRenderer.CaptureFrameBuffer()`:

- WinForms/GDI clones the backbuffer bitmap.
- MonoGame reads the active `RenderTarget2D` and converts it to a `Bitmap` once.

### Recommended MonoGame execution model

For visual fidelity, the transition should advance one original frame per host frame.

The original runtime expects a present/sync cadence inside the transition loop. Because the C# `MainLoop()` computes one host frame, the blocking PSX loop has to be bridged into persistent state. The current port now keeps a warp-transition-active state in `GameEngine` and advances `FUN_80044440` once per `MainLoop()` call until it returns `0`.

The rule stays the same:

1. keep the original state machines and per-frame functions unchanged
2. move the host-frame pacing out of the tight blocking loop
3. call `FUN_80044440` once per MonoGame draw/update step until it returns `0`

That is a backend/host-loop adaptation only. The transition math itself should not be rewritten.

## Current C# port status

Implemented now:

- framebuffer capture at `StartWarpTransition`
- dispatch and render functions for the closed render paths listed above
- corrected cutscene init write to `g_warpEffectBuffer[3]`
- build validation on both `AlundraGame.csproj` and `AlundraTools.csproj`

Still intentionally not closed:

- the higher-level semantics of the heap word at `0x801FB424`, even though the raw side effect is now ported