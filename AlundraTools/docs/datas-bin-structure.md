# Structure of `datas.bin`

This document describes the layout of the `datas.bin` file as read by `AlundraEngine.DatasBin.DatasBin` and its helper classes (`DataBinHeader`, `GameMapHeader`, `GameMapInfo`, `SpriteInfoHeader`, `SpriteInfoEventCodes`, `WarpData`, ...).
Offsets are relative to the start of the referenced region (file start or a `GameMap` block). Sizes are given in bytes.

Repository source: https://github.com/xcasadio/alundra-datas-analyser

---

## 1) Main header: `DataBinHeader` ([source](https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/DatasBin/DataBinHeader.cs))
Location: offset `0x00` (file start)
Total read size: 0x800 (2048) bytes

Fields (offset relative to file start):

| Offset (decimal) | Type | Name | Description |
|---:|---|---|---|
| 0 | `uint32` | `AlundraSpriteRecordsOffset` | Offset to Alundra sprite records block |
| 4 | `uint32` | `AlundraSpriteSheetOffset` | Offset to Alundra sprite sheet data |
| 8 | `uint32` | `AlundraSpritesRepeatOffset` | Repeat offset for sprites (format-specific) |
| 12 | `uint32` | `AlundraStringTableOffset` | Offset to Alundra string table |
| 16 | `uint32` | `AlundraStringTableRepeatOffset` | Repeat offset for string table (format-specific) |
| 20 | `uint32` | `DrawPageParam` | Draw page parameter / used for spritesheet sizing |
| 24 | `uint32` | `LoadingScreen0` | Loading screen data pointer 0 |
| 28 | `uint32` | `LoadingScreen1` | Loading screen data pointer 1 |
| 32 | `uint32` | `LoadingScreen2` | Loading screen data pointer 2 |
| 36 | `uint32` | `LoadingScreen3` | Loading screen data pointer 3 |
| 40 | `uint32[483]` | `GameMapOffsets` | Table of 483 offsets to `GameMap` blocks (each entry is an offset relative to file start) |

---

## 2) `GameMapOffsets` table ([related source: DatasBin](https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/DatasBin/DatasBin.cs))
- Location: inside `DataBinHeader` at decimal offset `40`.
- Entries: 483 `uint32` values (each 4 bytes).
- Each non-zero entry is an offset (relative to file start) to a `GameMap` block.

Usage: `DatasBin` creates `new GameMap(br, offset)` for each valid entry.

---

## 3) `GameMap` block and `GameMapHeader` ([source](https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/DatasBin/GameMapHeader.cs))
- A `GameMap` starts at the offset listed in `GameMapOffsets`.
- First structure at that offset is `GameMapHeader`.

`GameMapHeader` (offsets relative to the `GameMap` block start):

| Offset (decimal) | Type | Name | Description |
|---:|---|---|---|
| 0 | `int32` | `InfoBlockOffset` | Offset to Info block (relative to GameMap start) |
| 4 | `int32` | `MapBlockOffset` | Offset to Map block |
| 8 | `int32` | `TileSheetsOffset` | Offset to tile sheet data |
| 12 | `int32` | `SpriteRecordsOffset` | Offset to sprite records (SpriteInfo) |
| 16 | `int32` | `SpriteSheetOffset` | Offset to sprite sheet binary data |
| 20 | `int32` | `ScrollScreenOffset` | Offset to scroll/sky/background data |
| 24 | `int32` | `StringTableOffset` | Offset to string table block |

Physical size read = 28 bytes (7 × 4).

Derived fields (examples):
- `InfoSize = MapBlockOffset - InfoBlockOffset`
- `MapSize = TileSheetsOffset - MapBlockOffset`

Access rule: absolute address of a sub-block = `GameMap.Offset + FieldOffset`.

---

## 4) `GameMapInfo` (InfoBlock) ([source](https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/DatasBin/GameMapInfo.cs))
Location: `GameMap.Offset + Header.InfoBlockOffset` (when `InfoBlockOffset != -1`)

Layout (offsets relative to `InfoBlock` start):

| Offset (decimal) | Type | Name | Description |
|---:|---|---|---|
| 0 | `int32` | `MapId` | Map identifier |
| 4 | `int16` | `Gravity` | Gravity value |
| 6 | `int16` | `TerminalVelocity` | Terminal velocity / Z viscosity |
| 8 | `byte` | `SlideEffectId` | Slide effect id / XY resistance |
| 9 | `byte` | `BalanceLevel` | Balance/anim deletion parameter |
| 10 | `byte` | `C` | Unknown / reserved |
| 11 | `byte` | `D` | Unknown / reserved |
| 12 | `byte` | `E` | Unknown / reserved |
| 13 | `byte` | `F` | Unknown / reserved |
| 14 | `byte` | `_10` | AnimLandFloor or similar |
| 15 | `byte` | `_11` | Item-related or reserved |
| 16 | `byte[1024]` | `Palettes` | 32 palettes × 16 colors × 2 bytes (PSX color) |
| 1040 | `byte[16]` | `Unused` | Reserved / padding (16 bytes at offset 16 + 1024) |
| 1056-ish | series | `SpriteMapEntries` | 6 entries × (val1:byte,val2:byte) - map sprite table descriptors |

Portals (warp table): the code seeks to `startPosition + 1066` before reading; offsets below are relative to that seek location.

| Offset (decimal from startPosition+1066) | Type | Name | Description |
|---:|---|---|---|
| 0 | `byte` | `PortalFlag1` | Portal flags byte 1 |
| 1 | `byte` | `PortalFlag2` | Portal flags byte 2 |
| 2 | `WarpData[64]` | `Portals` | 64 warp entries (64 × 12 = 768 bytes) |

Note: the explicit `startPosition + 1066` is used to match the original binary layout.

---

## 5) `WarpData` (portal entry) ([source](https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/DatasBin/WarpData.cs))
Size: 12 bytes

| Offset (decimal) | Type | Name | Description |
|---:|---|---|---|
| 0 | `byte` | `X1` | Top-left X of portal rectangle |
| 1 | `byte` | `Y1` | Top-left Y of portal rectangle |
| 2 | `byte` | `X2` | Bottom-right X of portal rectangle |
| 3 | `byte` | `Y2` | Bottom-right Y of portal rectangle |
| 4 | `int16` | `DestMapId` | Destination map ID |
| 6 | `byte` | `DestTileX` | Destination tile X |
| 7 | `byte` | `DestTileY` | Destination tile Y |
| 8 | `int16` | `ZLevel` | Destination Z level |
| 10 | `uint16` | `Flags` | Flags for warp behavior |

---

## 6) `SpriteInfoHeader` (found at `SpriteRecordsOffset`) ([source](https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/DatasBin/SpriteInfoHeader.cs))
Location: `GameMap.Offset + GameMapHeader.SpriteRecordsOffset`
Read size: 48 bytes (12 × 4)

| Offset (decimal) | Type | Name | Description |
|---:|---|---|---|
| 0 | `int32` | `EntitiesPointer` | Offset to entities list |
| 4 | `int32` | `MapEffectSector3Pointer` | Offset to effect sector |
| 8 | `int32` | `MapEventsPointer` | Offset to map events |
| 12 | `int32` | `SpriteTablePointer` | Offset to sprite table |
| 16 | `int32` | `SpriteEffectsPointer` | Offset to sprite effects |
| 20 | `int32` | `SpritePalettesPointer` | Offset to sprite palettes |
| 24 | `int32` | `EventCodesAPointer` | Pointer to EventCodes A table |
| 28 | `int32` | `EventCodesBPointer` | Pointer to EventCodes B table |
| 32 | `int32` | `EventCodesCPointer` | Pointer to EventCodes C table |
| 36 | `int32` | `EventCodesDPointer` | Pointer to EventCodes D table |
| 40 | `int32` | `EventCodesEPointer` | Pointer to EventCodes E table |
| 44 | `int32` | `EventCodesFPointer` | Pointer to EventCodes F table |

Derived sizes are computed in code using these pointers.

---

## 7) Event codes (`SpriteInfoEventCodes`) ([source](https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/DatasBin/SpriteInfoEventCodes.cs))
- Base for event bytecodes: `_binOffset = binOffset + SpriteInfoHeader.EventCodesAPointer`, where `binoffset = GameMap.Offset + SpriteRecordsOffset`.
- Tables A..F: arrays of `short` (offsets relative to `_binOffset`).
- `GetByteCode(br, sectorOffset)` reads bytes starting at `_binOffset + sectorOffset` until `0xFF` terminator.
- `GetCommands(...)` parses commands into `SiCommand` objects using `CommandSizeByCode` (size per opcode).

`CommandSizeByCode` maps each opcode to the total size (code + parameters).

---

## 8) Other blocks ([source: GameMap](https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/DatasBin/GameMap.cs), [SpriteInfo](https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/DatasBin/SpriteInfo.cs))
- `Map`: at `GameMap.Offset + Header.MapBlockOffset` - contains `MapTiles`, `WallTilesOffset`, etc.
- `TileSheets`: at `GameMap.Offset + Header.TileSheetsOffset` - compressed data, decompressed with `ImageHelper.Deflate`.
- `SpriteSheet`: at `GameMap.Offset + Header.SpriteSheetOffset` - compressed data, decompressed.
- `ScrollScreen`: instantiated if `Header.ScrollScreenOffset != -1` via `new ScrollScreen(br)`.
- `StringTable`: at `GameMap.Offset + Header.StringTableOffset` - format: `128 * int16` offsets then null-terminated strings.

---

## 9) Important notes
- Offsets in headers are usually relative to the base of the corresponding block (file start for `DataBinHeader`, `GameMap` start for `GameMapHeader`, etc.). Always use "base + offset".
- The code contains magic constants (e.g. `startPosition + 1066` for portals). Follow the code's positioning for compatibility with original binary layout.
- Event codes use `_binOffset` and tables A..F of `int16` offsets; parsing relies on `CommandSizeByCode`.

