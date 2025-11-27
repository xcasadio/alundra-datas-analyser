# Structure of etc.res / etc.res (EtcRes)

This document describes the on-disk layout expected by `EtcRes` implementations (`EtcResR`, `EtcResUsa`) used by the engine to load item names, descriptions and other localized strings.

Source classes:
- `EtcRes` base: https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/EtcRes.cs
- `EtcResR` (region): https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/EtcResR.cs
- `EtcResUsa`: https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/EtcResUsa.cs

Overview
- File begins with an index table of 1024 entries (int16 each) — 2048 bytes total.
- After the index table, the file contains several string pools accessed by offsets stored in the index table.
- The code reads specific ranges of the index table to build `StringTable`, `DescriptionStrings`, `IconNames`, `DescriptionItems`, `OtherStrings`, and a general `Strings` pool.

Layout details

1) Index table
- Offset: 0 (file start)
- Length: 1024 × int16 = 2048 bytes
- Type: `int16[]` (signed short)
- Description: table of offsets (relative to file start) pointing to various string blocks. Negative values (-1) indicate "not present" for some entries.

2) StringTable (general strings)
- The implementation reads entries for `StringTable` using index table entries at indexes `0x100 .. 0x1FF`.
- For each i in `0..0xFF`:
  - offset = IndexTable[i + 0x100]
  - StringTable[i] = read zero-terminated string at `offset` (UTF-8/encoded decoding via TextDecoder)
- Type: `string[256]`

3) DescriptionStrings
- Read using index table entries at `0x00 .. 0xFF` (first 256 entries).
- For each i in `0..0xFF`:
  - offset = IndexTable[i]
  - if offset != -1: DescriptionStrings[i] = read zero-terminated string at `offset`
- Type: `string[256]`

4) General strings pool (`Strings`)
- The code then scans the file starting at `0x400 * 2` (decimal 2048) for additional strings. Implementation details:
  - `l` initialized to `0x400 * 2` (2048)
  - While `l < buffer.Length`: read string at `l`, store into `Strings` array and advance `l` over the read string (`l++` in code to skip terminator)
- Type: `string[]` (capacity 512 in class)

5) IconNames / DescriptionItems / OtherStrings
- For item-related text the code reads offsets from index table at these base indices:
  - Icon names: indexTable[0x200 .. 0x200 + N]
  - Description items: indexTable[0x280 .. 0x280 + N]
  - Other strings: indexTable[0x300 .. 0x300 + N]
- The code in `EtcResR`/`EtcResUsa` reads up to 0x62 (98) entries and extracts three strings per item using those offsets.
- Type: arrays sized 196 in the class (they store two* entries per item in code: index*2)

String encoding
- The code reads raw bytes and uses `TextDecoder.DecodeString` to transform PSX-encoded strings into .NET strings.
- Strings are zero-terminated in-file.

Notes
- Offsets in the index table are absolute offsets relative to file start (the code uses them directly when indexing into `buffer`).
- `EtcResR` stores a mapping from offset ? string for quick lookup (`_stringByIndex`).
- `EtcResUsa` uses the same structure but may have `-1` index values; it handles missing offsets accordingly.

If you want, I can generate a visual diagram and a small script that prints all index table entries and the first N resolved strings for inspection.