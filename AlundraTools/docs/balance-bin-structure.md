# Structure of `balance.bin` (viewed by `BalanceBin`)

This document describes the layout of `balance.bin` as parsed by `AlundraEngine.Balance.BalanceBin` and `BalanceRecord`.

Source classes:
- `BalanceBin`: https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/Balance/BalanceBin.cs
- `BalanceRecord`: https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/Balance/BalanceRecord.cs
- `BalanceAttack`: https://github.com/xcasadio/alundra-datas-analyser/blob/master/AlundraTools/AlundraEngine/Balance/BalanceAttack.cs

Overview

`balance.bin` is the game's **combat table**. For every sprite (and every item) it stores how much
damage the entity takes from each attack type, its max HP, and which attack it deals during each
of its animations.

- The file starts with a table of `int16` offsets, followed by a contiguous array of variable-size
  records. Verified on the USA 1.1 file: 512 offsets, 613 records, the last record ending exactly at
  the end of the file, no offset shared between two entries.
- Records are chained by power tier. The map selects the tier (see "Level selection" below).

Layout details

1) Offsets table
- The file begins with a sequence of `int16` offsets (512 entries in the retail file).
- The parser reads `int16` values until the file position reaches the firstOffset value read (first non-zero offset).
- The index into this table **is** the sprite table index, with one exception: the window
  `0x1F..0x7F` is reserved for **items** (`GetItemDataPointer` reads `offsets[itemId + 0x1e]`), and any
  sprite whose index falls inside that window is clamped to entry `0x1E` (`GetBalanceRecordFromSpriteIndex`).

2) `BalanceRecord`
The `BalanceRecord` is read at the specified offset. On-disk layout (as parsed):

| Offset (relative) | Type | Name | Description |
|---:|---|---|---|
| 0 | `byte` | `BalanceLevel` | Power tier of this record: 0..11, or 255 for the default/terminator entry |
| 1 | `byte` | `RecordSize` | Size of this record in bytes. Always `15 + 2 * AttackCount`, terminators included |
| 2 | `byte` | `MaxHp` | Max HP. Also aliases `DamageResponses[-1]`, i.e. slot 0 of the response array |
| 3..13 | `byte[11]` | `DamageResponses` | Damage response for attack types 1..11 (see below) |
| 14 | `byte` | `AttackCount` | Number of `BalanceAttack` entries following |
| 15.. | `BalanceAttack[]` | `Attacks` | `AttackCount` entries, 2 bytes each |

- Records of a chain are contiguous, so the next tier is at `offset + RecordSize`. The chain is walked
  while `BalanceLevel < g_itemIdThreshold`; a record with `BalanceLevel == 255` always terminates it.

3) Attack types

The 12 slots at offsets 2..13 are indexed by attack type. Names come from `g_weaponNames`
(pointer table at `0x80098f64`, strings at `0x800238FC`):

| 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 |
|---|---|---|---|---|---|---|---|---|---|---|---|
| No Effect | Sword | Hammer | Arrow | Bom | Tackle | Fire | Ice | Earth Magic | Water Magic | Fire Magic | Air Magic |

Slot 0 is the `MaxHp` byte; `DamageResponses[i]` holds the response to attack type `i + 1`.

Cross-check: in `UpdateBalanceRecords` (`0x80039300`) attack types 6 and 10 spawn effect 4
(Fire / Fire Magic → burn) and types 7 and 9 spawn effect 5 (Ice / Water Magic → freeze).

4) `DamageResponses` byte encoding

- **bits 7-6** — interaction kind, printed through `g_damageNames[value >> 6]`:
  - `0` Normal Damage
  - `1` Critical — sets HP to 0 (instant kill)
  - `2` No Effect — immune, the hit is ignored
  - `3` Error!
- **bits 5-0** — damage multiplier in 4.4 fixed point: `damage = (power * value) >> 4`, so `0x10` is ×1.
  Values present in the retail file: 0, 1, 2, 4, 5, 6, 8, 12, 15, 16, 31, 32.

5) `BalanceAttack` — 2 bytes per entry

- `AttackAttribute` : `byte`
  - bits 0-3 = attack type (index into the table above)
  - bit 7 = add the power of the player's equipped items to this attack (`0x8004464c`)
- `Power` : `byte` — base damage of the attack

**Indexing** (`0x80038ab4` and `0x8004464c`): the attack for animation `N` is `Attacks[N + 1]`;
element 0 is the fallback, used when `N + 1 >= AttackCount`. Use
`BalanceRecord.GetAttackForAnimation(animationId)` rather than indexing by hand.

6) Level selection

`g_itemIdThreshold` is set once per map by `InitializeItems` (`0x80044520`) from the map info byte at
offset `0x0B` (`lbu a0,0xb(v0)` at `0x8002c39c`). Chains exist only for a handful of weapon/projectile
sprites, and inside a chain only the attack power changes — this is how Alundra scales weapon damage
as the game progresses.

Notes
- Offsets table entry count determines how many sprites/items are present.
- `EffectiveBalanceStats` is *not* this on-disk record: it is the mutable per-frame copy of the
  player's effective stats (base record folded with the equipped weapon and shield), rebuilt by
  `UpdateItemEffectState`.
