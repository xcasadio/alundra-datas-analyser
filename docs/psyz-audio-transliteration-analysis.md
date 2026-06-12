# PsyZ audio analysis for Alundra sound transliteration

This note analyses the local `psyz/` repository as a reference for the faithful
transliteration of Alundra's original PSX sound runtime. The goal is not to
replace Alundra's runtime with PsyZ. The useful parts are the PSY-Q/libspu/libsnd
contracts, the partially decompiled libsnd state model, and the SPU register/RAM
behaviour that can constrain the C# port.

## Scope and sources

Relevant PsyZ surfaces read for this pass:

- `psyz/psyz/include/psyz.h`
- `psyz/psyz/src/psyz/psyz_spu.c`
- `psyz/psyz/src/platform/sdl3_audio.c`
- `psyz/psyz/include/libspu.h`
- `psyz/psyz/include/libsnd.h`
- `psyz/psyz/src/psyz/libspu.c`
- `psyz/psyz/src/psyz/libsnd.c`
- `psyz/decomp/src/libspu/libspu_private.h`
- `psyz/decomp/src/libsnd/libsnd_private.h`
- `psyz/decomp/src/libsnd/vm_g.c`
- `psyz/decomp/src/libsnd/ut_key.c`
- `psyz/decomp/src/libsnd/vm_f.c`
- `psyz/decomp/src/libsnd/vs_vh.c`
- `psyz/decomp/src/libsnd/vs_vtb.c`
- `psyz/decomp/src/libsnd/sscall.c`
- `psyz/psyz/tests/test_libspu.cpp`

## High level conclusion

PsyZ is useful as a PSY-Q compatibility reference, but it is not a complete
drop-in sound engine for this port.

Closed from the local PsyZ code:

- SPU output contract is 44100 Hz stereo signed 16-bit interleaved frames.
- SPU has 512 KiB RAM and 24 voices.
- SPU register offsets are relative to `0x1F801C00` and cover `0x000..0x1FF`.
- SPU transfer address register `0x1A6` stores an address divided by 8.
- SPU FIFO register `0x1A8` writes little-endian 16-bit words into SPU RAM and
  advances the transfer cursor by 2 bytes.
- libsnd VAB/SEQ structures and global arrays match the original PSY-Q style:
  `_svm_vab_vh`, `_svm_vab_pg`, `_svm_vab_tn`, `_svm_vab_start`,
  `_svm_vab_total`, `_svm_voice`, `_svm_cur`, `_ss_score`.
- `SsUtKeyOn` gives a concrete decompiled path for SFX parameter staging before
  allocation and `_SsVmKeyOnNow`.
- `_SsVmFlush` gives a concrete decompiled path for dirty voice registers,
  key-on/key-off latching, auto-volume/auto-pan, and ENVX-based voice release.

Not closed / not implemented in PsyZ:

- `psyz/psyz/src/psyz/psyz_spu.c` currently has `TODO: voice mixing`; the mixer
  does not decode or mix SPU ADPCM voices yet.
- `psyz/psyz/src/psyz/libsnd.c` stubs most high-level sound APIs with
  `NOT_IMPLEMENTED`, including `_SsVmKeyOnNow`, `_SsNoteOn`, `SsSeqOpen`,
  `SsUtKeyOnV`, pitch bend and many control-change handlers.
- Many decomp files under `psyz/decomp/src/libsnd` are still `INCLUDE_ASM`,
  including note-to-pitch, key-on internals, MIDI event handlers, VAG-address
  helpers, and voice allocation internals.
- Therefore PsyZ can constrain the port, but cannot by itself explain all of
  Alundra's current BGM/SFX playback defects.

Alundra-specific follow-up from targeted PCSX/Ghidra evidence:

- SEQ `0xE0` is not just a stored MIDI pitch-bend value in Alundra. The original
  dispatcher calls `FUN_8008D4C0 @ 0x8008D4C0`, which consumes the second data
  byte, centers it around `0x40`, then routes through `FUN_80093030` and
  `FUN_80092E04` to recalculate active voice pitch with the tone bytes
  `PitchBendMin/PitchBendMax` at offsets `+0x0C/+0x0D` before setting dirty bit
  `0x04` on `g_spuVoiceDirtyFlags`. Ignoring this path leaves BGM voices at
  their key-on pitch while SFX can still sound correct.
- CC7/CC10/CC11 route through `FUN_8009410C @ 0x8009410C`. ReVa shows this
  refresh reads live VAB tone attrs for tone volume/pan, uses the voice slot
  `VabFirstToneIndex` for program pan, and always squares final left/right
  volumes before writing `g_spuVoiceVolumeLeft/Right`; the mono flag only forces
  both sides to the larger pre-square value.
- `FUN_8008D1F4 @ 0x8008D1F4` controller subcases `2` and `3` write the VAB
  tone range bytes at offsets `+0x06/+0x07` (`Min/Max`) before `FUN_80090824`.
  Leaving these as no-ops makes subsequent SEQ note-to-tone selection diverge.
- `FUN_80093A04 @ 0x80093A04` note-off only calls `FUN_80091B1C` for noise
  voices where the raw VAG id is `0xFF`; sampled voices set `g_currentVoiceIndex`
  and call `FUN_80091134`, which stages the original SPU key-off masks.
- `FUN_80090C58 @ 0x80090C58` key-on volume uses the same final rule as the
  CC refresh path: mono mode equalizes sides first, then left and right are
  always squared before `g_spuVoiceVolumeLeft/Right` are written.
- `FUN_8008C064 @ 0x8008C064` writes `field_0xA8` only after a non-zero velocity
  note-on. Velocity-zero note-off routes through `FUN_80093A04` without clearing
  that field.
- `FUN_8008BDD0 @ 0x8008BDD0` treats any status with high nibble `0xF0` as the
  meta/system path by setting message type `0xFF` and calling `FUN_8008D568` with
  the next byte. `FUN_8008D568` ignores unhandled meta types; it does not stop
  the sequence.
- `ProcessVoiceStop @ 0x8009261C` is not the desktop key-off-mask consumer. ReVa
  shows it is a per-voice volume transition update driven by
  `VoiceRuntimeSlot +0x28..+0x32`; desktop consumption of staged SPU key-off
  masks must remain a separate `JUSTIFICATION: PSX hardware adaptation only`
  helper.
- `FUN_800920E0 @ 0x800920E0` is the paired per-voice volume transition updater
  driven by `VoiceRuntimeSlot +0x1C..+0x26`; `UpdateSoundVoicesState @
  0x8009311C` calls it before dirty SPU volume/pitch/ADSR flush when
  `field_0x1C != 0`.
- Desktop key-off handling is a backend adaptation: staged SPU key-off masks now
  call `SoundBin.KeyOffTrackedVoice`, which keeps the tracked voice alive and
  fades its current desktop volume according to `ADSR2.Rr/Rm`. This uses the
  Alundraportage/P.E.Op.S rate-table convention and leaves hard
  `StopTrackedVoice` for replacement/reset cleanup.
- Desktop tracked voices now apply a minimal ADSR backend envelope from the raw
  `ADSR1/ADSR2` words staged by `FUN_800912B4 @ 0x800912B4`. `FUN_80090C58 @
  0x80090C58` pushes those words before the desktop volume/pitch bridge, and
  `SoundBin.AdvanceTrackedVoices` advances attack/decay/sustain/release against
  the raw SPU volume mirror. This is backend adaptation only; runtime ADSR words
  remain the original values.
- `FUN_8008BDD0 @ 0x8008BDD0` does not dispatch explicit MIDI `0x80` note-off
  or `0xA0/0xD0` statuses; it returns without clearing the sequence. Music
  note-off behavior is closed through `0x90` velocity zero -> `FUN_8008C064 @
  0x8008C064` -> `FUN_80093A04 @ 0x80093A04`. The C# desktop hard-stop helper
  must not be used for `0x80`.
- `DisplayDebugBgmMenu @ 0x8004A9D8` routes BGM index `0` to `LoadBgm(0)` and
  BGM indexes `> 0` through `FUN_80049BC0(index)` -> `LoadMapSequence(index, 1)`.
  A live PCSX comparison on current original BGM index `21` closed one note-on
  against the C# trace: original `FUN_800934B8 @ 0x800934B8` entered with
  `sequenceKey=0`, `vabId=1`, `program=4`, `note=50`, `velocity=75`,
  `orientation=64`; after return, voice 3 had SPU `left=826`, `right=1775`,
  `pitch=1528`, `ADSR1=0x80FF`, `ADSR2=0xCF29`. C# trace
  `obj/audio_compare/bgm_021_csharp_trace.jsonl` reaches the same note tuple at
  frame 32 and produces the same SPU `left/right/pitch/ADSR` values. The VAB
  slot identity now matches too: C# sound-only loading reports `currentVabId=1`
  after allowing original VAB slot 0, matching original RAM
  `g_currentVabId=1`, `g_globalSoundVabId=0`, `g_mapSoundVabId=3`.
- `InitializeSoundSystem @ 0x800484E8` sets the original global SPU reverb state:
  `g_spuReverbAttr.mask=7`, `mode=0x104`, `depth.left=0x2A00`,
  `depth.right=0x2A00`, then calls `SpuSetReverbModeParam`,
  `SpuSetReverbDepth`, `SpuSetReverbVoice(1, 0xFFFFFF)`, and `SpuSetReverb(1)`.
  PCSX RAM at `0x80166128` confirms `07 00 00 00 04 01 00 00 00 2A 00 2A`.
  The C# port now records this in the desktop adaptation reverb state; BGM21 and
  BGM1 traces show nonzero per-voice reverb words.
- CLOSED (backend synthesis): the desktop backend is now a software SPU-style
  mixer, `AlundraEngine/Sound/SpuMixerSoundPlaybackBackend.cs`, producing one
  44100 Hz stereo s16 stream (the PsyZ/SDL SPU output contract). It consumes the
  raw staged SPU words directly: independent left/right voice gains
  (`raw/0x4000`, replacing the MonoGame volume+pan law), raw SPU pitch step
  `pitch/0x1000` (replacing the host `+/-1` octave pitch clamp), sample-accurate
  VAG loop points, and the SPU reverb unit. `MonoGameSoundPlaybackBackend` is
  now a thin wrapper that pushes the mixed stream into a single
  `DynamicSoundEffectInstance`.
- CLOSED (reverb synthesis): the mixer implements the nocash psx-spx reverb
  formula at 22050 Hz with the `SPU_REV_MODE_STUDIO_C` ("Studio Large") preset
  registers (work area `0x6FE0` bytes), selected by Alundra's mode `0x104` and
  scaled by the recorded depth `0x2A00`. Per-voice reverb enables flow through
  `ISoundPlaybackBackend.UpdateVoiceReverb` (BGM traces show `reverb=-1`, i.e.
  all-voices, matching `SpuSetReverbVoice(1, 0xFFFFFF)`). Offline A/B on BGM 1
  (`--render-bgm ... --no-reverb`) measures the reverb contribution at diff RMS
  ~4638 (~-17 dBFS), stable over 10 s (no filter runaway).
- CLOSED (sustain decrease): VAB ADSR2 words for BGM tones (e.g. `0xCF29`,
  `0x5009`, `0xCCAA`) have `Sd=1`: on the SPU, ENVX keeps decreasing during the
  sustain phase. The desktop envelope in `SoundBin` previously held the
  sustain-entry level forever, leaving every sustained voice at full envelope.
  `AdvanceTrackedVoiceSustain` now steps a 31-bit envelope mirror down with the
  P.E.Op.S rate-table convention (linear and pseudo-exponential modes).
  Sustain-increase (`Sd=0`) still holds, pending a proven case. Note: this also
  changes `Voice.status`/ENVX feedback into `UpdateSoundVoicesState`, so voice
  slot allocation order can shift; the proven BGM21 note-on tuple
  (program 4, note 50, velocity 75 -> left 826, right 1775, pitch 1528,
  ADSR1 0x80FF, ADSR2 0xCF29) still appears at frame 32, on a different slot.
- CLOSED (sequencer tempo, root cause of "music plays everything at once"): the
  pQES SEQ header stores resolution (ticks per quarter) big-endian at bytes
  8-9, like the big-endian u24 tempo at bytes 10-12. The C# seq init read it
  little-endian, turning BGM21's `01 E0` (480 PPQN) into 0xE001 (57345), so
  `CurrentTempo` came out 15929 instead of 133 and the sequencer consumed ~119x
  too many delay ticks per 60 Hz frame. Every BGM executed its whole event
  stream in a few frames and relooped continuously: all channels keyed
  simultaneously, sequenced echo layers (e.g. BGM21 ch8 note 52 at ticks
  720/760/800/840) landed in the same frame in phase, and the mix sat at RMS
  ~14700 with ~4% rail clipping. Fixed to a big-endian read in the seq init
  (`SoundManager.cs`, GHIDRA seq-open path near `LoadSeq @ 0x8008BC00`).
  After the fix, BGM21 traces show `seqTempo=133` (= 480 PPQN x 100 BPM x 10 /
  3600), zero voices before frame 36 (first SEQ note is at tick 480 = 0.6 s),
  the ch8 echo spread over frames 64/69/73/78, and renders with no rail
  clipping at RMS ~3500 (BGM1: ~3030). The earlier "frame 32" reference for the
  proven note-on tuple was an artifact of the fast tempo; the tuple pipeline
  itself (FUN_800934B8 -> 826/1775/1528/0x80FF/0xCF29) is unchanged by this
  fix. Next PCSX-Redux session: re-anchor one BGM21 note-on frame-for-frame at
  the correct tempo to confirm tick alignment against the original.

## SPU backend contract from PsyZ

`psyz/psyz/include/psyz.h` defines the platform-facing audio contract:

- `PSYZ_SPU_RAM_SIZE = 512 * 1024`
- `PSYZ_SPU_NUM_VOICES = 24`
- `PSYZ_SPU_SAMPLE_RATE = 44100`
- `Psyz_SpuPullSamples(short* out, int num_frames)` generates stereo signed
  16-bit frames interleaved L/R.

`psyz/psyz/src/platform/sdl3_audio.c` opens the host audio device as:

- format: `SDL_AUDIO_S16`
- channels: 2
- frequency: `PSYZ_SPU_SAMPLE_RATE`

This strongly argues that the desktop backend for Alundra should converge toward
a fixed 44100 Hz stereo mixer. Per-voice `SoundEffect` instances with per-sample
sample-rate changes are a useful bridge, but they are not structurally close to
the SPU. The faithful long-term adapter should accept the same raw voice/SPU
state writes as the original runtime, then emit a single 44100 Hz stereo stream.

## SPU register and RAM behaviour

`psyz/decomp/src/libspu/libspu_private.h` defines the register file layout:

- each voice register block is `0x10` bytes:
  - `0x00`: left/right volume
  - `0x04`: pitch
  - `0x06`: start address
  - `0x08`: ADSR1/ADSR2
  - `0x0C`: current envelope/ENVX
  - `0x0E`: loop address
- common registers begin at `0x180`:
  - `0x180`: main volume
  - `0x184`: reverb volume
  - `0x188`: key on low/high
  - `0x18C`: key off low/high
  - `0x190`: pitch modulation bits
  - `0x194`: noise bits
  - `0x198`: reverb voice bits
  - `0x1A6`: transfer address
  - `0x1A8`: transfer FIFO
  - `0x1AA`: SPUCNT
  - `0x1B0`: CD volume

`psyz/psyz/src/psyz/psyz_spu.c` implements these side effects:

- `Psyz_SpuWrite(offset, value)` stores the raw register word.
- Writing `0x1A6` sets the SPU RAM transfer cursor to `value << 3`.
- Writing `0x1A8` writes the word to SPU RAM and advances the cursor by 2 bytes.
- `Psyz_SpuRead(0x1A6)` returns `transfer_addr >> 3`.
- `Psyz_SpuRead(0x1AE)` mirrors the low six bits of SPUCNT.

`psyz/psyz/tests/test_libspu.cpp` confirms the FIFO byte order: writing word
`0xDEAD` produces bytes `AD DE` in SPU RAM. This matters for VAB body upload and
for any future SPU-RAM-backed playback adapter.

## Current PsyZ mixer limitation

`spu_tick` in `psyz_spu.c` currently mixes CD audio through SPUCNT/CD volume/main
volume. Voice mixing is explicitly still a TODO. This means:

- PsyZ cannot yet be used as an oracle for VAG voice playback quality.
- PsyZ cannot directly solve the current distorted SFX problem by being run as a
  backend.
- PsyZ is still useful for output shape: one 44100 Hz stereo stream, SPUCNT gate,
  main volume scaling, CD volume scaling, capture buffers, and register layout.

For Alundra, do not replace the runtime with PsyZ's incomplete voice mixer. Use
its register contract and decompiled libsnd pieces to make the existing C# port
more faithful.

## libspu constants useful to Alundra

`psyz/psyz/include/libspu.h` fixes several ranges that should constrain the C#
port:

- voice id range: 0..23
- pitch range: `0x0000..0x3fff`
- voice mask: `SPU_ALLCH = 0x00FFFFFF`
- direct voice volume is represented by signed 16-bit left/right values; PsyZ
  comments document the extended range as `-0x4000..0x3fff`.
- libspu masks exist for pitch, note, sample note, waveform start address,
  loop start address, ADSR fields, and direct ADSR1/ADSR2.

For the C# port, this supports keeping raw SPU pitch as the primary truth and
deriving any desktop-only sample rate from it only at the backend boundary.

## libsnd VAB/SEQ structures

`psyz/psyz/include/libsnd.h` defines the public PSY-Q structures:

- `ProgAtr`:
  - `tones`
  - `mvol`
  - `prior`
  - `mode`
  - `mpan`
  - `attr`
- `VabHdr`:
  - `form`
  - `ver`
  - `id`
  - `fsize`
  - `ps`
  - `ts`
  - `vs`
  - `mvol`
  - `pan`
  - `attr1`
  - `attr2`
- `VagAtr`:
  - `prior`
  - `mode`
  - `vol`
  - `pan`
  - `center`
  - `shift`
  - `min`
  - `max`
  - vibrato/portamento/pitch-bend fields
  - `adsr1`
  - `adsr2`
  - `prog`
  - `vag`

Useful closed conventions:

- libsnd SFX and SEQ volumes are 0..127 before being transformed into SPU direct
  volumes.
- `SndVoiceStats.vagId` documents VAG numbers as 1..254.
- Pan is documented as 0 left, about 63/64 center, 127 right.
- `VagAtr.vag` is the VAG id used by the tone. This aligns with the existing
  Alundraportage note that tone `vag` is 1-based for sample identity while VAB
  body offset lookup must keep the raw VAB layout semantics.

## VAB header/body handling

`psyz/decomp/src/libsnd/vs_vh.c` gives a useful partial decompilation of
`SsVabOpenHeadWithMode`:

- the VAB magic is validated against `VABp`/`VAB` style form data.
- version >= 5 allows up to `0x80` programs; older versions use `0x40`.
- program attributes are at `sizeof(VabHdr)`.
- tone attributes begin after `kMaxPrograms * 0x10` bytes.
- VAG offset table is located at `tone_ptr + vab_header->ps * 512`.
- VAG sizes are computed from offset-table entries:
  - version >= 5: `entry * 8`
  - older: `entry * 4`
- `_svm_vab_start[vab_id]` stores the allocated SPU base address.
- `_svm_vab_total[vab_id]` stores the total VAB body transfer size.
- each pair of program `reserved2` shorts receives cumulative SPU addresses
  divided by 8.

`psyz/decomp/src/libsnd/vs_vtb.c` transfers the body by:

- setting transfer mode to 0
- setting transfer start address to `_svm_vab_start[vabid]`
- writing `_svm_vab_total[vabid]` bytes

For Alundra, this supports a raw VAB-body model where SPU addresses and loop
addresses should remain byte/8-byte aware rather than being hidden behind a high
level sample list.

## SFX key-on path from libsnd

`psyz/decomp/src/libsnd/ut_key.c` gives a concrete `SsUtKeyOn` flow:

1. guard `_snd_ev_flag`.
2. call `_SsVmVSetUp(vabId, prog)`.
3. set `_svm_cur.seq_sep_no = 0x21` for utility SFX.
4. store note, fine, and requested tone.
5. convert requested left/right volumes into a master volume and pan-like value:
   - if L == R: `voll` plus center `0x40`
   - if R < L: master is L, pan value is `(R * 0x40) / L`
   - if L < R: master is R, pan value is `0x7F - ((L * 0x40) / R)`
6. copy program master volume/pan and tone count.
7. compute tone index as `tone + fake_program * 0x10`.
8. copy tone priority, VAG id, tone volume, tone pan, center, shift, mode,
   min/max.
9. reject `vag == 0`.
10. allocate a voice using the VAG id.
11. fill `_svm_voice[voice]` raw fields.
12. call `_SsVmDoAllocate()`.
13. if VAG id is `0xFF`, use noise; otherwise call `_SsVmKeyOnNow(1,
    note2pitch2(note, fine))`.

This is highly relevant to the current distorted SFX issue. The C# port should
check that its SFX path applies the same volume/pan decomposition and does not
send raw `voll`/`volr` directly as if they were already SPU direct volumes.

## Register flush path from libsnd

`psyz/decomp/src/libsnd/vm_f.c` gives a concrete `_SsVmFlush` implementation
for the PsyZ build:

- samples ENVX/current volume into `_svm_voice[i].unk6`.
- tracks 16 frames of zero ENVX history.
- auto-releases voices after sustained zero ENVX when auto key-off mode allows.
- applies `_autovol` and `_autopan` for voices with active automation.
- writes dirty buffered voice registers into the SPU register file:
  - dirty bit `1`: left/right volume
  - dirty bit `4`: pitch
  - dirty bit `8`: start address
  - dirty bit `0x10`: ADSR1/ADSR2
- clears dirty flags.
- latches pending key-off and key-on words to SPU common registers.
- latches reverb voice mask words.

For Alundra, this reinforces that the runtime can show voices as allocated while
the actual audible backend only becomes correct after the flush has delivered
volume, pitch, address, ADSR, and key-on in the right order.

## SEQ tick path

`psyz/decomp/src/libsnd/sscall.c` shows `SsSeqCalledTbyT` order:

1. guard `_snd_ev_flag`.
2. call `_SsVmFlush()` first.
3. iterate open sequence slots and tracks.
4. process play, crescendo, decrescendo, tempo, pause, replay, and stop flags.
5. clear `_snd_ev_flag`.

This supports keeping Alundra's current order strict: pending voice-register
state must be flushed once per tick/frame before or alongside sequence event
processing as in the original control flow. If music voices are active but
inaudible, inspect whether the backend has received the complete flushed state,
not only the high-level note-on call.

## Practical implications for the current C# port

### Music is active but inaudible

Prior runtime evidence showed BGM voices in a playing backend state with nonzero
volume. PsyZ narrows the remaining backend-suspicion area:

- The final desktop output should be one 44100 Hz stereo stream. Multiple dynamic
  per-voice instances are a bridge and can report `Playing` while their sample
  format, loop range, or submitted buffers are wrong.
- BGM loop samples should be treated as raw SPU voice playback with start/loop
  addresses and pitch, not as whole-file sound effects.
- Missing ADSR can leave a voice either too quiet, too abrupt, or release-tracked
  incorrectly. PsyZ's `_SsVmFlush` depends on ENVX, which our current high-level
  backend does not faithfully model.
- If active BGM voices have decoded nonzero PCM but no audible music, the next
  discriminating check is to inspect the exact backend-submitted PCM format:
  sample rate, bytes queued, loop byte range, channel count, and whether buffers
  are continuously submitted after `BufferNeeded`.

### SFX are audible but distorted

PsyZ points to four likely parameter classes:

1. raw pitch/sample-rate conversion
   - keep raw SPU pitch first
   - desktop sample rate should be derived at the backend boundary
   - raw pitch range is `0x0000..0x3fff`
2. volume/pan decomposition
   - `SsUtKeyOn` converts L/R volumes into a master volume and pan-style value
   - direct SPU volume is not the same as libsnd `voll`/`volr` 0..127
3. ADSR
   - `VagAtr.adsr1/adsr2` are part of the key-on state
   - current direct PCM playback without envelope can sound harsh or wrong
4. loop/start addresses
   - loop start is a voice register field independent from whole-sample looping
   - PSX samples may loop only a subrange

The highest-value immediate comparison is not broad ReVa. It is a local xref or
runtime trace around Alundra's C# equivalent of `_SsVmKeyOnNow`/flush: verify the
voice's final raw pitch, left/right SPU volumes, start address, loop address,
ADSR1/ADSR2, and key-on bit before the backend starts playback.

## Recommended proof targets

Use xrefs/local reads only, matching the user's constraint.

1. Close Alundra's equivalent of `SsUtKeyOn` volume/pan decomposition.
   - Compare current SFX `voll`/`volr` handling against PsyZ `ut_key.c`.
   - Check whether C# bypasses the original master-volume/pan path.

2. Close Alundra's equivalent of `_SsVmFlush` dirty-register semantics.
   - Ensure pitch, volume, address, ADSR, key-on and key-off are flushed in the
     original order.
   - Ensure backend playback starts only with the initial raw pitch already set.

3. Compare one known distorted SFX against original runtime parameters.
   - Use PCSX-Redux if needed to read SPU registers for that one voice after
     key-on.
   - Compare with C# runtime inspector values for the same SFX.

4. Compare one inaudible BGM voice backend submission.
   - Capture the C# decoded PCM metadata and loop range.
   - Compare its raw pitch and expected effective sample rate.

5. Treat a single 44100 Hz stereo SPU-style mixer as the long-term backend
   direction.
   - This is a backend adaptation, not a runtime redesign.
   - It should consume raw voice state already produced by the transliterated
     runtime.

## Do not use PsyZ for these without more proof

- Do not port `psyz/psyz/src/psyz/libsnd.c` as-is: it is mostly stubs.
- Do not assume PsyZ's SPU mixer closes VAG playback: voice mixing is currently
  missing.
- Do not replace Alundra's hand-transliterated sequence runtime with PsyZ's
  high-level `libsnd` API.
- Do not rename Alundra raw fields based only on PsyZ comments unless the field
  correspondence is closed by address/xref/decomp evidence.

## Bottom line

PsyZ gives strong constraints for the desktop audio adapter:

- fixed 44100 Hz stereo s16 output
- 512 KiB SPU RAM model
- exact transfer-address and FIFO behaviour
- exact SPU register layout
- useful libsnd globals and some decompiled control flow

For the current Alundra bugs, the most actionable PsyZ evidence is `SsUtKeyOn`
and `_SsVmFlush`: verify that C# SFX/BGM voices carry original raw pitch,
original volume/pan decomposition, start/loop address, ADSR1/ADSR2, and key-on
state into the backend before playback. Distortion is more likely a parameter or
backend-mixing mismatch than a VAB off-by-one, and music silence should be
debugged at the exact backend-submitted PCM/loop/buffer level.