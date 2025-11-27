# Structure of `balance.bin` (viewed by `BalanceBin`)

This document describes the layout of `balance.bin` as parsed by `AlundraEngine.Balance.BalanceBin` and `BalanceRecord`.

Source classes:
- `BalanceBin`: https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/Balance/BalanceBin.cs
- `BalanceRecord`: https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/Balance/BalanceRecord.cs
- `BalanceAnimValRef`: https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/Balance/BalanceAnimValRef.cs

Overview
- `balance.bin` appears to be a list of offset entries (int16) terminated when the first non-zero offset repeats (parsed by reading offsets until `br.BaseStream.Position < firstOffset`).
- Each offset points to a `BalanceRecord` structure which may chain to the next record using a relative `OffsetToNextLevel` byte.

Layout details

1) Offsets table
- The file begins with a sequence of `int16` offsets.
- The parser reads `int16` values until the file position reaches the firstOffset value read (first non-zero offset).
- Each offset is added to `_offsets` list and used later to instantiate a `BalanceRecord`.
- Interpretation: array of offsets indexing into chains of `BalanceRecord` entries.

2) `BalanceRecord`.
The `BalanceRecord` is read at the specified offset. On-disk layout (as parsed):

| Offset (relative) | Type | Name | Description |
|---:|---|---|---|
| 0 | `byte` | `Level` | Level identifier (0..255), if <255 then chain continues |
| 1 | `byte` | `OffsetToNextLevel` | Offset in bytes to the next `BalanceRecord` for the same item/group |
| 2 | `byte` | `Hp` | HP value for the level |
| 3..13 | `byte[11]` | `Values` | 11 bytes of values (various attributes) |
| 14 | `byte` | `NumAnimVals` | Number of `BalanceAnimValRef` entries following |
| 15.. | `BalanceAnimValRef[]` | `AnimVals` | `NumAnimVals` entries, each 2 bytes: (`Val` byte, `U2` byte) |

- After reading `NumAnimVals` entries, if `Level < 255`, the code constructs `Next = new BalanceRecord(br, offset + OffsetToNextLevel)` (recursive chain).

`BalanceAnimValRef`:
- 2 bytes per entry:
  - `Val` : `byte`
  - `U2`  : `byte`

Notes
- The data layout suggests a linked-list of levels per item, where each `BalanceRecord` contains stats for that level and a pointer (relative offset) to the next.
- Offsets table entry count determines how many items/groups are present.

If you want, I can:
- Add a table mapping sprite indexes to offsets (printing the parsed `_offsets`).
- Provide a small tool to dump `balance.bin` contents into a human-readable CSV.