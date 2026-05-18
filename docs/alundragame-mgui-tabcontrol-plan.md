# AlundraGame MGUI frmGame TabControl Plan

Goal: recreate in `AlundraGame` the debug tab control currently defined by `FrmGame`, using MGUI, displayed to the right of the game render. The game render keeps its current dimensions; only the window/backbuffer width grows by 512 pixels.

Generated XAML draft: `AlundraTools/AlundraGame/UI/FrmGameDebugPanel.xaml`.

## Status Icons

- ⬜ Not started
- 🔄 In progress
- ✅ Done
- ⛔ Blocked

Each AI agent must update exactly one task icon before and after work, commit after the task, and leave the next task untouched. Commit messages should stay small and monotonic.

## Hard Constraints

- Keep the game render at its current size and position: `0,0` to `StaticVariables.ScreenWidth * ScaleFactor` by `StaticVariables.ScreenHeight * ScaleFactor`. Current `AlundraGame` constants resolve to `1280x944` because `ScreenHeight = 236`.
- Increase the `AlundraGame` window/backbuffer width by exactly `512` pixels.
- Place the MGUI debug panel at `x = gameRenderWidth`, `y = 0`, width `512`, height equal to the game render height.
- Keep the recreated `tabControl1` dimensions at `511x767` pixels, matching `frmGame` `MinimumSize = 511` and `Size = 511x767`.
- Do not recreate `hScrollBarFrames`, `buttonSaveFrames`, `buttonLoadDump`, `buttonCompareWithDump`, or `buttonExtractToCsv`.
- Do not move runtime/debug behavior into a new engine architecture. Wire MGUI controls to the same `GameEngine` and `StaticVariables` state currently used by `FrmGame`.
- Preserve the existing dirty worktree. At the time this plan was written, `AlundraTools/AlundraGame/AlundraGame.cs` already had user changes.

## Original Layout Facts

Source: `AlundraTools/AlundraTools/GameControls/frmGame.Designer.cs`.

- `pctOut`: `Location=1,1`, `Size=1280x896`, black background, stretch image.
- `tabControl1`: `Location=1281,0`, `MinimumSize=511x0`, `Size=511x767`, `SelectedIndex=0`.
- `FrmGame.ClientSize`: `1793x898`.
- Side panel relative coordinates use `panelX = originalX - 1281` for top-level controls outside `tabControl1`.
- Tab page client size: `503x739`, tab header height offset `24`.

## AI Task Board

### ✅ T01 - Verify MGUI Host Wiring

Scope: inspect current MGUI references and choose the existing MGUI MonoGame host integration path.

Steps:

- Confirm whether `MGUI.MonoGame.LegacyRenderer` is required in addition to current `MGUI.MonoGame.Integration` references.
- Choose `DelegateRenderHost` unless `AlundraGame` already exposes the observable update hooks needed by `GameRenderHost<T>`.
- Document the final host choice in this file under `Integration Notes`.

Commit: `docs(plan): verify mgui host path for debug panel`.

### ✅ T02 - Include XAML Asset

Scope: make `FrmGameDebugPanel.xaml` available at runtime.

Steps:

- Add the XAML as project content if SDK defaults do not copy it to output.
- Keep the path stable: `AlundraTools/AlundraGame/UI/FrmGameDebugPanel.xaml`.
- Verify the file can be found from `AlundraGame` output directory.

Commit: `backend(mgui): include frmgame debug panel xaml`.

### ✅ T03 - Enlarge Backbuffer Width

Scope: resize only the MonoGame window/backbuffer.

Steps:

- Change `PreferredBackBufferWidth` from `StaticVariables.ScreenWidth * ScaleFactor` to that value plus `512`.
- Leave `PreferredBackBufferHeight` unchanged.
- Keep the game destination rectangle at `0,0,GameRenderWidth,GameRenderHeight` for `ScaleFactor = 4`.

Commit: `backend(monogame): reserve right debug panel width`.

### ✅ T04 - Create MGUI Desktop

Scope: initialize MGUI without changing game-loop ordering.

Steps:

- Create the MGUI host/runtime in `Initialize` or `LoadContent` following the selected path from T01.
- Load default MGUI resources before loading the XAML.
- Update MGUI during `Update` and draw it after the game render in `Draw`.

Commit: `backend(mgui): initialize desktop in alundragame`.

### ✅ T05 - Load Debug Panel XAML

Scope: load the XAML and keep its coordinates stable.

Steps:

- Load `FrmGameDebugPanel.xaml` as a MGUI `Window`.
- Set or verify `Left=GameRenderWidth`, `Top=0`, `Width=512`, `Height=GameRenderHeight` for the current scale.
- Keep `WindowStyle=None` so it behaves like an embedded side panel.

Commit: `backend(mgui): load frmgame debug panel xaml`.

### ✅ T06 - Wire Button Commands

Scope: map MGUI `CommandName` values to the existing `FrmGame` button behavior.

Steps:

- Register MGUI commands matching the XAML `CommandName` values.
- Port command bodies mechanically from `FrmGame`, adjusted only for MonoGame/MGUI APIs.
- Excluded controls must remain absent.

Commit: `ui(mgui): wire debug panel button commands`.

### ✅ T07 - Wire Toggle And Radio State

Scope: implement checkboxes and speed radio buttons.

Steps:

- Bind or event-wire all display/debug/log checkboxes to their current `StaticVariables` destinations.
- Preserve initial checked states from `frmGame`: `Floor tiles=true`, `Wall tiles=true`, `Speed 1=true`.
- Use one radio group for speed values `0.25`, `0.5`, `0.75`, `1`, `1.5`, `2`.

Commit: `ui(mgui): wire debug toggles and speed radios`.

### ✅ T08 - Wire Numeric And Combo Controls

Scope: implement player status numeric controls and item/weapon combos.

Steps:

- Mirror the `numericUpDown*` `ValueChanged` behavior from `FrmGame`.
- Populate `comboBoxWeapon`, `comboBoxItem`, `comboBoxRandomItem`, `comboBoxSpawnItemId`, and `comboBoxLogCategories`.
- Verify current `frmGame` designer wires `comboBoxRandomItem` to `comboBoxItem_SelectedIndexChanged`; decide whether to preserve that exact wiring or use the existing `comboBoxRandomItem_SelectedIndexChanged` method after explicit review.

Commit: `ui(mgui): wire player status editors`.

### ✅ T09 - Recreate Entity And Effect Lists

Scope: implement `listBoxEntities`, `listBoxEffects`, `propertyGridEntity`, and `propertyGridEffect`.

Steps:

- Populate entity/effect lists on map change exactly as `RefreshUI` currently does.
- Selection must update `EditorSelectEntityIndex` and `EditorSelectEffectIndex`.
- Implement an MGUI property-grid adapter over `DataGrid`/`ListView` that preserves category, descriptor, and value rendering from `UniversalWrapper`.

Commit: `ui(mgui): recreate entity and effect inspectors`.

### ✅ T10 - Recreate Flags Tables

Scope: implement temporary/game flag grids and dynamic flag labels.

Steps:

- `dataGridViewTemporaryFlags`: columns `Index=40`, `Value=60`, `Size=110x334`, hide zero rows.
- `dataGridViewGameFlags`: columns `Index=40`, `Value=60`, `Size=117x334`, hide zero rows.
- Populate `panelFlags` from the same `_flagModels` sequence and refresh label values every UI refresh.

Commit: `ui(mgui): recreate flag grids and labels`.

### ✅ T11 - Recreate HUD And Dialog Readouts

Scope: implement read-only HUD/dialog labels and text boxes.

Steps:

- Mirror `RefreshDialogControls` and `RefreshHudControls`.
- Keep text boxes read-only, multiline, vertically scrollable if MGUI supports it.
- Preserve label names so runtime binding code can find controls by original `frmGame` names.

Commit: `ui(mgui): recreate hud and dialog readouts`.

### ✅ T12 - Recreate Logs Tab

Scope: implement logs list, filters, copy, clear, and toggles.

Steps:

- Populate `listBoxLogs` from `LogManager.Logs` or category-specific logs.
- Populate `comboBoxLogCategories` from `LogManager.LogByCategories.Keys`.
- Implement `Show all logs`, `Copy all logs`, `Refresh`, and `Clear logs` commands.

Commit: `ui(mgui): recreate log viewer tab`.

### ✅ T13 - Recreate Script Tab

Scope: implement the script view and refresh command.

Steps:

- Port `buttonRefreshScript_Click` logic mechanically.
- Convert `CommandsViewerForm.FillTreeView` output into MGUI `TreeViewItem` nodes.
- Preserve command offsets, names, and parameters in displayed node text.

Commit: `ui(mgui): recreate script tab`.

### 🔄 T14 - Visual Parity Pass

Scope: compare MGUI panel with `frmGame` rendering.

Steps:

- Capture screenshots of `frmGame` and `AlundraGame` MGUI panel at `1280x896 + 512`.
- Verify tab order, tab labels, group box positions, control sizes, defaults, and bottom action controls.
- Update XAML positions only for parity, not for redesign.

Commit: `ui(mgui): align debug panel visual parity`.

### 🔄 T15 - Build And Smoke Test

Scope: final verification.

Steps:

- Run `dotnet build AlundraTools/AlundraGame/AlundraGame.csproj`.
- Launch `AlundraGame` and verify game render dimensions are unchanged.
- Verify MGUI panel input does not break gamepad/keyboard input routing.
- Update this plan with final verification notes.

Commit: `test(mgui): verify debug panel integration`.

### ✅ T16 - Cache Heavy Debug Grids

Scope: remove per-frame rebuild cost from property grids and flag grids.

Steps:

- Cache reflected property/field accessors per inspected runtime type.
- Rebuild entity/effect property grids only when selected target or target type changes.
- Update existing row text values in place when the selected runtime object remains the same.
- Rebuild temporary/game flag grids only when their non-zero flag signature changes.

Commit: `ui(mgui): cache heavy debug grids`.

### ✅ T17 - Virtualize Large Logs And Script Lists

Scope: keep massive Logs and Script tabs from slowing the game loop.

Steps:

- Enable `ListBoxVirtualizationMode.Always` for the log and script list boxes.
- Display a capped live window for logs while preserving full log copy behavior.
- Replace the script `TreeView` with a virtualized `ListBox` so thousands of script rows do not instantiate thousands of tree nodes.
- Display a capped live window for script commands with an omitted-row marker.

Commit: `ui(mgui): virtualize large debug lists`.

### ✅ T18 - Use FontStashSharp JetBrainsMono

Scope: align MGUI text rendering and font size closer to the WinForms debug UI.

Steps:

- Load `JetBrainsMono-Regular.ttf`, `JetBrainsMono-Bold.ttf`, and `JetBrainsMono-BoldItalic.ttf` through `FontStashSharpTextEngine`.
- Register the loaded family as MGUI's default text engine/font family.
- Reduce default debug panel font size and grid row height to match the compact WinForms layout more closely.
- Copy the TTF files through the MonoGame content/build output paths.

Commit: `ui(mgui): use jetbrains mono fontstash fonts`.

### ✅ T19 - Stack GroupBox Content And Grid Separators

Scope: replace fragile absolute `Canvas` layouts inside group boxes and match WinForms-like table separators.

Steps:

- Convert group box internals from absolute `CanvasTop` label placement to stacked `Grid`/`StackPanel` layouts ordered by the original Y coordinates.
- Keep `panelFlagsContent` as a canvas only for the existing dynamic flag-label population path.
- Add horizontal and vertical separators to property grids and the Debug tab temporary/game flag grids.
- Add a draggable vertical splitter to the property/value boundary in entity and effect property grids.
- Keep Logs and Script as separator-free virtualized `ListBox` controls.
- Fix `MGGrid` vertical gridline drawing so empty data grids do not crash before their rows are populated.

Commit: `ui(mgui): stack debug groupbox content`.

## Integration Notes

- `AlundraGame` already renders the game into `_renderTarget` at native resolution and draws that texture scaled to the backbuffer. The MGUI panel must be composited after that texture draw; no new game render path is needed.
- Use `MonoGameBackendBootstrap.Create(new DelegateRenderHost(GraphicsDevice, () => new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Services))` so the MGUI backend observes the MonoGame host without changing the game-loop ownership.
- Add `MGUI.MonoGame.LegacyRenderer` to `AlundraGame` because the concrete `MainRenderer`/desktop backend implementation is supplied there while `MGUI.MonoGame.Integration` carries the public integration contracts.
- Use computed `GameRenderWidth` and `GameRenderHeight` in C# instead of preserving the stale `1280x896` note from WinForms. The WinForms designer remains the source for tab/control positions, but `AlundraGame`'s current screen height is `236 * 4 = 944`.
- Use `MGUI.FontStashSharp` with the JetBrainsMono TTFs for the debug panel text engine. The required font files are copied from `AlundraTools/AlundraGame/Content/JetBrainsMono`.
- Use `MGListBox` virtualization for Logs and Script. `MGTreeView` is intentionally avoided for the Script tab because it eagerly creates too many visual nodes for maps with thousands of commands.
- Keep `Copy all logs` sourced from the full `LogManager` lists; the capped live list affects only what is rendered every frame.

## Verification Notes

- `rtk dotnet build AlundraTools/AlundraGame/AlundraGame.csproj` passes with 0 errors and 0 warnings after the performance/font pass.
- `rtk dotnet build AlundraTools/AlundraGame/AlundraGame.csproj` passes after T19 with 0 errors and 1 existing WinForms high-DPI manifest warning (`WFO0003`).
- `dotnet .\AlundraTools\AlundraGame\bin\Debug\net9.0-windows\AlundraGame.dll` stayed running past the 12-second smoke window after the final T19 changes; it was terminated manually after no startup, XAML, or MGUI gridline draw exception appeared.
- `dotnet .\AlundraTools\AlundraGame\bin\Debug\net9.0-windows\AlundraGame.dll` started and remained running past the 12-second smoke window after the FontStashSharp/XAML fixes; it was terminated manually after no startup or XAML load exception appeared.
- Visual parity screenshots and manual input-routing checks remain open because they require interactive desktop inspection of the running MonoGame window and the original `frmGame` side-by-side.
- The implementation pass did not create commits because the working tree already contained unrelated modified/untracked files (`Alundra.sln`, Ghidra database files, and the `MGUI` submodule state). Stage only task-related files before committing.

## Controls Intentionally Omitted

Requested exclusions:

- `hScrollBarFrames`
- `buttonSaveFrames`
- `buttonLoadDump`
- `buttonCompareWithDump`
- `buttonExtractToCsv`

Other non-XAML runtime surface:

- `pctOut` is not recreated in MGUI; the game render remains the existing MonoGame render target.
- `buttonForceRandomItem` is declared in `frmGame.Designer.cs` but is not instantiated in `InitializeComponent`; it is treated as stale designer residue.

## MGUI Equivalents

| WinForms | MGUI XAML | Notes |
|---|---|---|
| `TabControl` | `TabControl` | Same tab order and header text. |
| `TabPage` | `TabItem` | `TabItem.Header` uses `TextBlock`. |
| `Label` | `TextBlock` | Original `AutoSize=true`; XAML keeps text-sized labels. |
| `Button` | `Button` | Uses `CommandName` for command registration. |
| `CheckBox` | `CheckBox` | State wiring in C# agent task. |
| `RadioButton` | `RadioButton` | Group name `runtimeSpeed`. |
| `ComboBox` | `ComboBox` | String item type with original item text. |
| `NumericUpDown` | `NumericUpDown` | `Minimum=0`, `Maximum=100` unless WinForms set another maximum. |
| `ListBox` | `ListBox` | Runtime-populated; Logs and Script are virtualized. |
| `PropertyGrid` | `DataGrid` placeholder | Needs property-grid adapter in T09 for behavior parity. |
| `DataGridView` | `DataGrid` | Two-column flag grids. |
| `Panel` | `ScrollViewer` + `Canvas` | Used for dynamic `panelFlags`. |
| `TreeView` | `ListBox` | Script tab uses a virtualized flat list for large command sets. |

## Control Inventory

The following inventory lists the controls to recreate from `frmGame`, excluding only the requested controls.

### Top-Level Side Panel Controls

| Name | MGUI | Panel location | Size | Text/default | Behavior |
|---|---|---:|---:|---|---|
| `tabControl1` | `TabControl` | `0,0` | `511x767` | selected index `0` | Hosts all tabs. |
| `buttonPauseGame` | `Button` | `7,772` | `75x23` | `Running`, green text | Toggle pause/play. |
| `buttonRunOneFrame` | `Button` | `88,772` | `35x23` | `>|` | Pause and advance one frame. |
| `labelFrames` | `TextBlock` | `139,776` | auto | `Frames 0/0` | Refreshed by replay manager. |
| `buttonSnapshot` | `Button` | `425,772` | `75x23` | `Snapshot` | Save snapshot image. |
| `buttonSaveState` | `Button` | `7,867` | `75x23` | `Save state` | Save JSON state. |

### Entities Tab

| Name | MGUI | Location | Size | Text/default | Behavior |
|---|---|---:|---:|---|---|
| `tabPageEntities` | `TabItem` | `4,24` | `503x739` | `Entities` | First selected tab. |
| `groupBox1` | `GroupBox` | `5,5` | `143x90` | `Entities` | Entity counts. |
| `label1` | `TextBlock` | `6,19` | auto | `# entity:` | Static label. |
| `labelNumberOfEntity` | `TextBlock` | `77,19` | auto | `0` | `g_numberOfEntities`. |
| `label2` | `TextBlock` | `6,34` | auto | `# activated` | Static label. |
| `labelNumberOfActivatedEntity` | `TextBlock` | `77,34` | auto | `0` | `g_activeEntityCount`. |
| `label5` | `TextBlock` | `6,49` | auto | `# collideable` | Static label. |
| `labelNumberOfCollideableEntity` | `TextBlock` | `77,49` | auto | `0` | `g_collideableEntitiesCount`. |
| `label7` | `TextBlock` | `6,64` | auto | `# visible` | Static label. |
| `labelNumberOfVisibleEntity` | `TextBlock` | `77,64` | auto | `0` | `g_visibleEntityCount`. |
| `groupBox3` | `GroupBox` | `155,5` | `137x128` | `Map` | Map readouts. |
| `label21` | `TextBlock` | `6,19` | auto | `id:` | Static label. |
| `labelMapId` | `TextBlock` | `77,19` | auto | `0` | `g_currentMap`. |
| `label19` | `TextBlock` | `6,34` | auto | `size:` | Static label. |
| `labelMapSize` | `TextBlock` | `77,34` | auto | `0` | Current map size. |
| `label17` | `TextBlock` | `6,49` | auto | `gravity` | Static label. |
| `labelMapGravity` | `TextBlock` | `77,49` | auto | `0` | Current map gravity. |
| `label15` | `TextBlock` | `6,64` | auto | `# entity` | Static label. |
| `labelMapNumberOfEntity` | `TextBlock` | `77,64` | auto | `0` | Map entity count. |
| `label10` | `TextBlock` | `6,79` | auto | `pos offset` | Static label. |
| `labelMapOffset` | `TextBlock` | `77,79` | auto | `0` | `g_mapOffsetX/Y`. |
| `label4` | `TextBlock` | `6,94` | auto | `screen pos` | Static label. |
| `labelMapScreenPos` | `TextBlock` | `77,94` | auto | `0` | `g_mapScreenPosX/Y`. |
| `groupBox2` | `GroupBox` | `298,5` | `200x90` | `Camera` | Camera readouts. |
| `label13` | `TextBlock` | `5,19` | auto | `current pos` | Static label. |
| `labelCameraPosition` | `TextBlock` | `76,19` | auto | `0` | HUD current X/Y. |
| `label6` | `TextBlock` | `5,34` | auto | `look at` | Static label. |
| `labelCameraLookAt` | `TextBlock` | `76,34` | auto | `0` | Camera look-at XYZ. |
| `label9` | `TextBlock` | `5,49` | auto | `offset` | Static label. |
| `labelCameraOffset` | `TextBlock` | `76,49` | auto | `0` | Scrolling parameters. |
| `label29` | `TextBlock` | `5,64` | auto | `scrolling` | Static label. |
| `labelCameraScrolling` | `TextBlock` | `76,64` | auto | `0` | Camera scrolling X/Y. |
| `label56` | `TextBlock` | `298,99` | auto | `active collision entity` | Static label. |
| `labelActiveCollisionEntity` | `TextBlock` | `422,99` | auto | empty | `g_activeCollisionEntity`. |
| `label14` | `TextBlock` | `3,125` | auto | `Entities` | Static label. |
| `listBoxEntities` | `ListBox` | `4,143` | `103x574` | runtime items | Selection updates entity inspector. |
| `propertyGridEntity` | `DataGrid` | `113,143` | `386x589` | property/value | MGUI property-grid adapter. |

### Effects Tab

| Name | MGUI | Location | Size | Text/default | Behavior |
|---|---|---:|---:|---|---|
| `tabPageEffects` | `TabItem` | `4,24` | `503x739` | `Effects` | Effects inspector. |
| `label28` | `TextBlock` | `2,2` | auto | `Effects` | Static label. |
| `listBoxEffects` | `ListBox` | `3,20` | `103x709` | runtime items | Selection updates effect inspector. |
| `propertyGridEffect` | `DataGrid` | `112,20` | `386x714` | property/value | MGUI property-grid adapter. |

### Player Status Tab

| Name | MGUI | Location | Size | Text/default | Behavior |
|---|---|---:|---:|---|---|
| `tabPagePlayerStatus` | `TabItem` | `4,24` | `503x739` | `Player status` | Player edits. |
| `label20` | `TextBlock` | `5,10` | auto | `HP max` | Static label. |
| `numericUpDownHpMax` | `NumericUpDown` | `66,7` | `131x23` | value `10`, max `100` | Set `HpMax`. |
| `label24` | `TextBlock` | `5,33` | auto | `HP` | Static label. |
| `numericUpDownHp` | `NumericUpDown` | `66,32` | `131x23` | value `10`, max `100` | Existing code sets `HpMax`; verify. |
| `label23` | `TextBlock` | `5,58` | auto | `MP max` | Static label. |
| `numericUpDownMpMax` | `NumericUpDown` | `66,56` | `131x23` | value `0`, max `100` | Set `MpMax`. |
| `label22` | `TextBlock` | `5,83` | auto | `MP` | Static label. |
| `numericUpDownMp` | `NumericUpDown` | `66,80` | `131x23` | value `0`, max `100` | Set `Mp`. |
| `label25` | `TextBlock` | `5,108` | auto | `Money` | Static label. |
| `numericUpDownMoney` | `NumericUpDown` | `66,105` | `131x23` | max `30000` | Set money. |
| `label26` | `TextBlock` | `5,133` | auto | `Falcon 1` | Static label. |
| `numericUpDownFalcon1` | `NumericUpDown` | `66,130` | `131x23` | max `100` | Set Falcon. |
| `label27` | `TextBlock` | `5,158` | auto | `Falcon 2` | Static label. |
| `numericUpDownFalcon2` | `NumericUpDown` | `66,154` | `131x23` | max `100` | Set FalconTemp. |
| `label53` | `TextBlock` | `5,182` | auto | `Keys` | Static label. |
| `numericUpDownKeys` | `NumericUpDown` | `66,179` | `131x23` | max `100` | Set item key count. |
| `label16` | `TextBlock` | `5,211` | auto | `Weapon` | Static label. |
| `comboBoxWeapon` | `ComboBox` | `66,208` | `133x23` | 6 weapon items | Set weapon and ensure item ownership. |
| `label18` | `TextBlock` | `5,235` | auto | `Item` | Static label. |
| `comboBoxItem` | `ComboBox` | `66,232` | `133x23` | 27 item entries | Set current item and count. |
| `buttonAllItems` | `Button` | `5,269` | `193x22` | `All items BUG !!` | Fill all item counts with `1`. |
| `buttonRestoreHpAndMp` | `Button` | `214,7` | `150x22` | `Restore Hp and Mp` | PlayerManager effect. |
| `buttonIncreaseMpMax` | `Button` | `214,43` | `150x22` | `Increase Mp Max` | PlayerManager effect. |
| `buttonRestoreMp` | `Button` | `214,70` | `150x22` | `Restore Mp` | PlayerManager effect. |
| `buttonIncreaseMp` | `Button` | `214,96` | `150x22` | `Increase Mp` | PlayerManager effect. |
| `buttonIncreaseHpMax` | `Button` | `214,131` | `150x22` | `Increase Hp Max` | PlayerManager effect. |
| `buttonRestoreHp` | `Button` | `214,157` | `150x22` | `Restore Hp` | PlayerManager effect. |
| `buttonIncreaseHp` | `Button` | `214,184` | `150x22` | `Increase Hp` | PlayerManager effect. |
| `buttonAddLowHp` | `Button` | `214,210` | `150x22` | `Add low Hp` | PlayerManager effect. |
| `buttonAddMediumHp` | `Button` | `214,236` | `150x22` | `Add medium Hp` | PlayerManager effect. |
| `buttonAddHugeHp` | `Button` | `214,262` | `150x22` | `Add huge Hp` | PlayerManager effect. |

### Debug Tab

| Name | MGUI | Location | Size | Text/default | Behavior |
|---|---|---:|---:|---|---|
| `tabPageDebug` | `TabItem` | `4,24` | `503x739` | `Debug` | Flags, pad, callbacks, item spawn. |
| `label54` | `TextBlock` | `3,9` | auto | `Display only flags != 0` | Static label. |
| `label8` | `TextBlock` | `6,26` | auto | `Temporary flags` | Static label. |
| `dataGridViewTemporaryFlags` | `DataGrid` | `6,44` | `110x334` | columns `Index`, `Value` | Temporary flags, hide zero rows. |
| `label12` | `TextBlock` | `122,26` | auto | `Game flags` | Static label. |
| `dataGridViewGameFlags` | `DataGrid` | `122,44` | `117x334` | columns `Index`, `Value` | Game flags, hide zero rows. |
| `groupBox6` | `GroupBox` | `242,9` | `254x145` | `Pad` | Pad readouts. |
| `label40`..`label52` | `TextBlock` | see XAML | auto | pad field labels | Static labels. |
| `labelPadMaxNbHeld`..`labelPadButtonJustPressedByInterval` | `TextBlock` | see XAML | auto | `0` | Refreshed from `g_padState1`. |
| `groupBox7` | `GroupBox` | `242,159` | `254x219` | `Display callbacks` | Callback labels. |
| `labelCallback0`..`labelCallback12` | `TextBlock` | see XAML | auto | callback names | Foreground gray/black by active flag. |
| `groupBoxFlags` | `GroupBox` | `6,384` | `490x280` | `Flags` | Dynamic flag readouts. |
| `panelFlags` | `ScrollViewer` | `3,19` | `484x258` | empty | Populated from `_flagModels`. |
| `label30` | `TextBlock` | `3,671` | auto | `Force random item` | Static label. |
| `comboBoxRandomItem` | `ComboBox` | `126,669` | `108x23` | item ids | Force random item table after wiring review. |
| `buttonSpawnItem` | `Button` | `3,694` | `108x22` | `Spawn item` | Spawn item at player. |
| `comboBoxSpawnItemId` | `ComboBox` | `126,695` | `108x23` | item ids | Item id source for spawn. |
| `buttonAlundraCabine` | `Button` | `247,671` | `142x23` | `Pass alundra cabine` | Set flags 27 bits. |
| `checkBoxDisableCollision` | `CheckBox` | `395,675` | auto | `Disable collision` | Toggle `g_debugState` bit. |
| `buttonControlAlundra` | `Button` | `247,699` | `142x22` | `Control alundra` | Clear player control flag bit. |

### HUD Tab

| Name | MGUI | Location | Size | Text/default | Behavior |
|---|---|---:|---:|---|---|
| `tabPageHud` | `TabItem` | `4,24` | `503x739` | `HUD` | Dialog and HUD readouts. |
| `groupBox5` | `GroupBox` | `5,5` | `494x291` | `Dialog` | Dialog state. |
| `label38`, `label37`, `label35`, `label33`, `label41`, `label43`, `label49`, `label47`, `label45`, `label55`, `label57` | `TextBlock` | see XAML | auto | static labels | Static labels. |
| `labelTextFlag`, `labelTextAutoAdvance`, `labelTextDelayReset`, `labelTextDelay`, `labelTextBufferX`, `labelLineIndex`, `labelTextCursor`, `labelTextRenderStep`, `labelTextLinesWidth`, `labelHudDebug1`, `labelHudDebug2` | `TextBlock` | see XAML | auto | `0` | Refreshed from dialog/HUD state. |
| `label31` | `TextBlock` | `179,18` | auto | `Text displayed` | Static label. |
| `textBoxTextInDialog` | `TextBox` | `181,35` | `308x75` | CRLF | Read-only multiline. |
| `label32` | `TextBlock` | `179,121` | auto | `Full text` | Static label. |
| `textBoxFullText` | `TextBox` | `179,138` | `309x148` | CRLF | Read-only multiline. |
| `groupBoxHud` | `GroupBox` | `5,301` | `494x95` | `HUD` | HUD state. |
| `label34`, `label36`, `label3`, `label11` | `TextBlock` | see XAML | auto | static labels | Static labels. |
| `labelHudActivate`, `labelHudDebug`, `labelHudXY`, `labelHudDelta` | `TextBlock` | see XAML | auto | `0` | Refreshed from HUD state. |

### Display Tab

| Name | MGUI | Location | Size | Text/default | Behavior |
|---|---|---:|---:|---|---|
| `tabPage1` | `TabItem` | `4,24` | `503x739` | `Display` | Display toggles, zoom, speed. |
| `groupBox4` | `GroupBox` | `6,5` | `163x307` | `Display` | Overlay toggles. |
| `checkBoxDisplayEntityId` | `CheckBox` | `5,20` | auto | `Entity id` | `DisplayEntityId`. |
| `checkBoxDisplayEntityPositions` | `CheckBox` | `6,42` | auto | `Entity positions` | `DisplayEntitiesPosition`. |
| `checkBoxDisplayEffectId` | `CheckBox` | `6,65` | auto | `Effect id` | `DisplayEffectId`. |
| `checkBoxEffectPositions` | `CheckBox` | `6,88` | auto | `Effect positions` | `DisplayEffectsPosition`. |
| `checkBoxDisplayCollision` | `CheckBox` | `5,110` | auto | `Hitboxes` | `DisplayCollisions`. |
| `checkBoxTileXY` | `CheckBox` | `5,133` | auto | `Tile xy` | `DisplayTileXY`. |
| `checkBoxWallTileXY` | `CheckBox` | `6,156` | auto | `WallTile xy` | `DisplayWallTileXY`. |
| `checkBoxTileZ` | `CheckBox` | `6,179` | auto | `Tile z` | `DisplayTileZ`. |
| `checkBoxWallTileZ` | `CheckBox` | `6,202` | auto | `WallTile z` | `DisplayWallTileZ`. |
| `checkBoxDisplayFloorTiles` | `CheckBox` | `6,225` | auto | `Floor tiles`, checked | `DisplayTiles`. |
| `checkBoxDisplayWallTiles` | `CheckBox` | `6,248` | auto | `Wall tiles`, checked | `DisplayWallTiles`. |
| `groupBox8` | `GroupBox` | `175,6` | `321x60` | `Zoom` | Zoom buttons. |
| `buttonZoomx1`, `buttonZoomX2`, `buttonZoomX4`, `buttonZoomX8` | `Button` | see XAML | `39x23` | `x1`, `x2`, `x4`, `x6` | Set zoom `1`, `2`, `4`, `6`. |
| `groupBox9` | `GroupBox` | `181,72` | `315x48` | `Speed` | Runtime speed radios. |
| `radioButtonSpeed0_25`, `radioButtonSpeed0_5`, `radioButtonSpeed0_75`, `radioButtonSpeed1`, `radioButtonSpeed1_5`, `radioButtonSpeed2` | `RadioButton` | see XAML | auto | `0.25`, `0.5`, `0.75`, `1`, `1.5`, `2` | Set `StaticVariables.Speed`. |

### Logs Tab

| Name | MGUI | Location | Size | Text/default | Behavior |
|---|---|---:|---:|---|---|
| `tabPage2` | `TabItem` | `4,24` | `503x739` | `Logs` | Log viewer. |
| `checkBoxAddLogInVS` | `CheckBox` | `14,11` | auto | `Add Logs in VS Studio` | `LogManager.TraceEnabled`. |
| `label58` | `TextBlock` | `14,33` | auto | `Filter by category` | Static label. |
| `comboBoxLogCategories` | `ComboBox` | `118,30` | `186x23` | runtime categories | Refresh filter. |
| `buttonShowAllLogs` | `Button` | `14,61` | `117x23` | `Show all logs` | Clear filter and refresh. |
| `buttonClearLog` | `Button` | `137,61` | `117x23` | `Clear logs` | Clear log manager. |
| `buttonCopyAllLogs` | `Button` | `260,61` | `117x23` | `Copy all logs` | Copy visible log lines. |
| `buttonRefreshLogs` | `Button` | `14,99` | `117x23` | `Refresh` | Refresh logs/categories. |
| `checkBoxLogScript` | `CheckBox` | `380,4` | auto | `Log script` | `IsLogScriptEnabled`. |
| `checkBoxLogDamage` | `CheckBox` | `380,29` | auto | `Log damage` | Existing code uses `checkBoxLogScript.Checked`; verify. |
| `checkBoxDebugPortal` | `CheckBox` | `380,54` | auto | `Log portal` | `DebugPortalsEnabled`. |
| `checkBoxLogAI` | `CheckBox` | `380,79` | auto | `Log AI values` | `IsLogAIEnabled`. |
| `listBoxLogs` | `ListBox` | `14,128` | `482x604` | runtime logs | Display logs. |

### Script Tab

| Name | MGUI | Location | Size | Text/default | Behavior |
|---|---|---:|---:|---|---|
| `tabPage3` | `TabItem` | `4,24` | `503x739` | `Script` | Script tree. |
| `buttonRefreshScript` | `Button` | `6,6` | `75x23` | `Refresh` | Parse current map event codes. |
| `treeViewScript` | `TreeView` | `6,35` | `490x698` | runtime nodes | Display parsed commands. |

## Behavior Checklist

- Labels refreshed by `RefreshUI`: entity counts, camera, map, active collision entity, flags, dialog, HUD, pad, callbacks.
- Lists refreshed on map change: entities and effects.
- Controls that mutate state: display toggles, player status editors, log toggles, speed radios, item spawn/random controls, save state, snapshot.
- Property grid parity requires category/descriptors from `_entityCategories`, `_entityDescriptors`, `_effectCategories`, and `_effectDescriptors`.
- Top-level frame recording/dump controls are excluded by request; no replay scrollbar behavior is needed for this panel.