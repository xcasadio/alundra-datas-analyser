using AlundraEngine.Graphics;
using System.Drawing.Imaging;

namespace AlundraEngine.Loader;

/// <summary>
/// Gives access to the TIM resources embedded in the loader executable, plus the raw bytes.
///
/// JUSTIFICATION: PSX hardware adaptation only.
/// RELATION: on the console these resources are simply resident in RAM at fixed addresses once the
/// executable is loaded, and the code dereferences them directly. On desktop the executable is a
/// file, so each resource is reached by a file offset instead.
///
/// Nothing here is tied to one regional build. The RAM-to-file delta comes from the PS-EXE header,
/// the resources from the embedded container, and the sound tables from where that container ends;
/// what genuinely differs between discs lives in <see cref="LoaderBuild"/>.
/// </summary>
public class LoaderExeInspector
{
    /// <summary>The PS-EXE header every PSX executable carries ahead of its text segment.</summary>
    private const int PsxExeHeaderSize = 0x800;

    /// <summary>
    /// <c>fileOffset = ramAddress - RamToFileOffsetDelta</c>, i.e. the executable's load address
    /// minus the header the file carries ahead of it.
    ///
    /// SOURCE: read from this executable's own PS-EXE header (<c>t_addr</c> at +0x18) rather than
    /// hardcoded, so the conversion holds for any regional build. Every Alundra executable checked
    /// loads at 0x80020000 — LOADER.EXE and SLES_011.98 (France), SLUS_005.53 (USA 1.1), plus
    /// CLOSING.EXE and ALUN_CD.EXE on both discs — which is why 0x8001F800 was right everywhere so
    /// far; deriving it simply removes the assumption.
    /// </summary>
    public uint RamToFileOffsetDelta { get; }

    /// <summary>GHIDRA: the record id GetEtcResource stops on — "END" plus 0xFF.</summary>
    private const uint EndSentinelId = 0xFF444E45;

    /// <summary>
    /// The id the records past the sentinel carry: no tag, no index, just 0xFFFFFFFF.
    /// </summary>
    /// <remarks>
    /// GetEtcResource never sees these, but the container format continues across the sentinel and
    /// the France build parks <c>g_TitleFull</c> in one of them — which is why the original reaches
    /// that image by a direct pointer rather than through the lookup.
    /// </remarks>
    private const uint UntaggedRecordId = 0xFFFFFFFF;

    /// <summary>Ghidra symbol names for the France build's records, keyed by tag and index.</summary>
    /// <remarks>
    /// Cosmetic only — used to name the PNGs <see cref="SaveAllImages"/> writes. Resolution no
    /// longer goes through this table, so a record it does not know about is still readable; it
    /// simply gets its tag and index as a name.
    ///
    /// SOURCE: each name is the Ghidra symbol found at the record's payload address on the France
    /// build. Tags #1..#6 hold the same roles on the USA build, verified by shape.
    /// </remarks>
    private static readonly Dictionary<(string Tag, int Index), string> ResourceNames = new()
    {
        [("TIM", 1)] = "g_loadRoomBackgroundTimBuffer",
        [("TIM", 2)] = "g_loadRoomCloudsTim",
        [("TIM", 3)] = "g_loadRoomBackgroundMessageTim",
        [("TIM", 4)] = "g_loadRoomFontTim",
        [("TIM", 5)] = "g_loadRoomSpriteSheetTim",
        [("TIM", 6)] = "g_loadingScreenTim",
        [("TIM", 7)] = "g_licenceScreenTim",

        // The France build's single untagged record, past the sentinel. The USA build has none —
        // it carries the same image as TIM #0, which is why that tag has no entry here.
        [("", -1)] = "g_TitleFull",
    };

    private readonly byte[] _exeBytes;
    private readonly EtcResource[] _resources;
    private readonly Dictionary<int, Bitmap> _images = new();

    public int ImageCount => _resources.Length;

    /// <summary>Raw LOADER.EXE bytes, for reading data tables that sit alongside the TIMs.</summary>
    public byte[] ExeBytes => _exeBytes;

    /// <summary>
    /// File offset of the first byte past the resource container, where the sound offset tables
    /// begin.
    /// </summary>
    public int ContainerEndOffset { get; }

    /// <summary>Which disc this executable came from.</summary>
    public LoaderBuild Build { get; }

    public LoaderExeInspector(string gamePath)
        : this(gamePath, LoaderBuild.Detect(gamePath) ?? LoaderBuild.France)
    {
    }

    public LoaderExeInspector(string gamePath, LoaderBuild build)
    {
        Build = build;
        var exeFilePath = build.FindExeFilePath(gamePath)
            ?? throw new FileNotFoundException(
                $"No loader executable ({string.Join(" or ", build.ExeFileNames)}) found in '{gamePath}'.");

        _exeBytes = File.ReadAllBytes(exeFilePath);
        RamToFileOffsetDelta = ReadRamToFileOffsetDelta(_exeBytes, exeFilePath);
        _resources = WalkResourceContainer(_exeBytes, exeFilePath, out var containerEnd);
        ContainerEndOffset = containerEnd;
    }

    public int GetImageFileOffset(int index) => _resources[index].PayloadOffset;

    public string GetImageName(int index)
    {
        var resource = _resources[index];
        return ResourceNames.TryGetValue((resource.Name, resource.Index), out var name)
            ? name
            : $"{resource.Name}{resource.Index}";
    }

    /// <summary>Converts a RAM address from Ghidra into an offset into <see cref="ExeBytes"/>.</summary>
    public int RamToFileOffset(uint ramAddress) => (int)(ramAddress - RamToFileOffsetDelta);

    /// <summary>
    /// Reads the load address out of the PS-EXE header and turns it into the RAM-to-file delta.
    /// </summary>
    /// <remarks>
    /// The header is the 0x800-byte block every PSX executable starts with: the magic "PS-X EXE"
    /// at +0, then pc0 at +0x10, t_addr at +0x18 and t_size at +0x1C.
    /// </remarks>
    private static uint ReadRamToFileOffsetDelta(byte[] exeBytes, string exeFilePath)
    {
        if (exeBytes.Length < PsxExeHeaderSize ||
            System.Text.Encoding.ASCII.GetString(exeBytes, 0, 8) != "PS-X EXE")
        {
            throw new InvalidDataException($"'{exeFilePath}' does not start with a PS-EXE header.");
        }

        var loadAddress = BitConverter.ToUInt32(exeBytes, 0x18);
        return loadAddress - PsxExeHeaderSize;
    }

    /// <summary>One entry of the resource container embedded in LOADER.EXE.</summary>
    /// <param name="Name">Three-character tag, "TIM" or "ANM", or empty past the sentinel.</param>
    /// <param name="Index">Index within that tag, 1-based; -1 past the sentinel.</param>
    /// <param name="PayloadOffset">File offset of the payload, i.e. of the TIM's magic word.</param>
    /// <param name="PayloadSize">Payload length in bytes.</param>
    public readonly record struct EtcResource(string Name, int Index, int PayloadOffset, int PayloadSize)
    {
        /// <summary>True for the records past the sentinel, which carry no tag.</summary>
        public bool IsUntagged => Name.Length == 0;
    }

    /// <summary>Every record of the container, in file order, tagged ones first.</summary>
    public EtcResource[] ReadEtcResources() => _resources;

    /// <summary>
    /// Walks the resource container LOADER.EXE carries, the one <c>g_loadRoomBackgroundTimPtr</c>
    /// points at.
    /// </summary>
    /// <remarks>
    /// GHIDRA: GetEtcResource @ 0x800276bc. Each record is
    /// <c>[u32 packedId][u32 payloadSize][payload]</c>, where the id packs the three tag characters
    /// little-endian plus the index in the top byte, and the tagged list ends on the sentinel
    /// <c>0xFF444E45</c> ("END" plus 0xFF).
    ///
    /// GetEtcResource stops there, but the format does not: the sentinel is followed by records
    /// whose id is <see cref="UntaggedRecordId"/>, ending on the first one with a zero size. The
    /// France build keeps <c>g_TitleFull</c> in such a record, which is why the original reaches
    /// that one image by a direct pointer instead of through the lookup. The USA build has no
    /// untagged payload at all — it carries the same image as TIM #0 inside the container.
    ///
    /// SOURCE: walking replaces the offset table this class used to hardcode. It is self-validating
    /// in a way that table was not: the chain only walks to the exact sentinel if every size along
    /// the way is right, and every payload is checked to start on a TIM magic word. Verified to
    /// yield 16 records on the France build (TIM #1..#7, ANM #1..#8, then the untagged g_TitleFull)
    /// at exactly the sixteen offsets previously hardcoded, and 16 on the USA build (TIM #0..#6,
    /// ANM #1..#9) matching that build's jPSXdec catalogue.
    /// </remarks>
    private static EtcResource[] WalkResourceContainer(byte[] exeBytes, string exeFilePath, out int containerEnd)
    {
        var position = FindContainerStart(exeBytes, exeFilePath);
        var records = new List<EtcResource>();

        // The tagged run, the part GetEtcResource sees.
        while (true)
        {
            var packedId = BitConverter.ToUInt32(exeBytes, position);
            if (packedId == EndSentinelId)
            {
                position += 8;
                break;
            }

            var payloadSize = (int)BitConverter.ToUInt32(exeBytes, position + 4);
            var name = TagOf(packedId);

            if (payloadSize <= 0 || position + 8 + payloadSize > exeBytes.Length || !name.All(char.IsAsciiLetterUpper))
            {
                throw new InvalidDataException(
                    $"'{exeFilePath}': malformed resource record at 0x{position:X6} (id 0x{packedId:X8}, size 0x{payloadSize:X}).");
            }

            records.Add(new EtcResource(name, (int)((packedId >> 24) & 0xFF), position + 8, payloadSize));
            position += 8 + payloadSize;
        }

        // The untagged run past the sentinel, ending on the first zero-sized record.
        while (position + 8 <= exeBytes.Length && BitConverter.ToUInt32(exeBytes, position) == UntaggedRecordId)
        {
            var payloadSize = (int)BitConverter.ToUInt32(exeBytes, position + 4);
            position += 8;
            if (payloadSize <= 0 || position + payloadSize > exeBytes.Length)
            {
                break;
            }

            records.Add(new EtcResource(string.Empty, -1, position, payloadSize));
            position += payloadSize;
        }

        containerEnd = position;
        return records.ToArray();
    }

    /// <summary>
    /// Finds the container's first record header by scanning for a chain that walks to the sentinel.
    /// </summary>
    /// <remarks>
    /// The original dereferences <c>g_loadRoomBackgroundTimPtr</c>, a pointer this port has no
    /// equivalent for without hardcoding one address per regional build. A record header is
    /// recognisable enough on its own — an uppercase three-letter tag whose payload starts on a TIM
    /// magic word — and the candidate is only accepted once the whole chain from it reaches the
    /// sentinel, which no coincidental match survives.
    /// </remarks>
    private static int FindContainerStart(byte[] exeBytes, string exeFilePath)
    {
        for (var position = PsxExeHeaderSize; position + 12 <= exeBytes.Length; position += 4)
        {
            if (BitConverter.ToUInt32(exeBytes, position + 8) != TimMagic ||
                !TagOf(BitConverter.ToUInt32(exeBytes, position)).All(char.IsAsciiLetterUpper))
            {
                continue;
            }

            if (ChainReachesSentinel(exeBytes, position))
            {
                return position;
            }
        }

        throw new InvalidDataException($"'{exeFilePath}': no embedded resource container found.");
    }

    /// <summary>Follows a candidate chain and reports whether it lands exactly on the sentinel.</summary>
    private static bool ChainReachesSentinel(byte[] exeBytes, int position)
    {
        for (var record = 0; record < MaxContainerRecords; record++)
        {
            if (position + 8 > exeBytes.Length)
            {
                return false;
            }

            var packedId = BitConverter.ToUInt32(exeBytes, position);
            if (packedId == EndSentinelId)
            {
                return true;
            }

            var payloadSize = (int)BitConverter.ToUInt32(exeBytes, position + 4);
            if (payloadSize <= 0 ||
                position + 8 + payloadSize > exeBytes.Length ||
                !TagOf(packedId).All(char.IsAsciiLetterUpper) ||
                BitConverter.ToUInt32(exeBytes, position + 8) != TimMagic)
            {
                return false;
            }

            position += 8 + payloadSize;
        }

        return false;
    }

    /// <summary>The three tag characters a record id packs, little-endian.</summary>
    private static string TagOf(uint packedId) =>
        new([(char)(packedId & 0xFF), (char)((packedId >> 8) & 0xFF), (char)((packedId >> 16) & 0xFF)]);

    /// <summary>First word of a TIM header.</summary>
    private const uint TimMagic = 0x10;

    /// <summary>
    /// Walk cutoff for a candidate chain. Both known builds hold 15 tagged records; this only has
    /// to stay above that and below "scans the whole file on every false positive".
    /// </summary>
    private const int MaxContainerRecords = 64;

    /// <summary>
    /// Finds a resource by tag and index, the way <c>GetEtcResource</c> does; returns null when
    /// absent.
    /// </summary>
    public EtcResource? FindEtcResource(string name, int index)
    {
        foreach (var entry in ReadEtcResources())
        {
            if (entry.Index == index && string.Equals(entry.Name, name, StringComparison.Ordinal))
            {
                return entry;
            }
        }

        return null;
    }

    /// <summary>Same lookup, for the roles <see cref="LoaderBuild"/> names.</summary>
    public EtcResource? FindEtcResource(LoaderResourceKey key) => FindEtcResource(key.Tag, key.Index);

    /// <summary>
    /// Every animation frame of the title logo, in container order.
    /// </summary>
    /// <remarks>
    /// GHIDRA: InitBootSequenceGraphics @ 0x800213c4 builds one tile layer per "ANM" resource and
    /// FUN_80021a1c walks them by index, so the container's own order and count are what matter.
    /// France holds eight, matching the loop bound read off that function; USA holds nine.
    /// </remarks>
    public EtcResource[] ReadTitleAnimationFrames() =>
        [.. _resources.Where(r => r.Name == "ANM").OrderBy(r => r.Index)];

    /// <summary>Decodes the resource a role names, or null when the build does not carry it.</summary>
    public Bitmap? LoadImage(LoaderResourceKey key)
    {
        var index = IndexOf(key);
        return index < 0 ? null : LoadImage(index);
    }

    /// <summary>Position of a role's resource in container order, or -1 when absent.</summary>
    public int IndexOf(LoaderResourceKey key)
    {
        for (var index = 0; index < _resources.Length; index++)
        {
            if (_resources[index].Index == key.Index &&
                string.Equals(_resources[index].Name, key.Tag, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>Where one BGM track's data lives inside SOUND.BIN.</summary>
    /// <param name="SeqOffset">Start of the sequence.</param>
    /// <param name="SeqEnd">End of the sequence, which is also the start of the VAB header.</param>
    /// <param name="VabBodyOffset">Start of the VAB body.</param>
    /// <param name="VabBodyEnd">End of the VAB body.</param>
    public readonly record struct BgmTrack(int SeqOffset, int SeqEnd, int VabBodyOffset, int VabBodyEnd);

    /// <summary>Where one sound-effect VAB bank lives inside SOUND.BIN.</summary>
    public readonly record struct SfxVabBank(int HeaderOffset, int BodyOffset, int BodyEnd);

    /// <summary>
    /// Where the four sound tables begin: on the first byte past the resource container.
    /// </summary>
    /// <remarks>
    /// SOURCE: on the France build this lands on 0x10D934, which is exactly
    /// <c>DAT_8012d134</c> — the first of the four. The USA build has the same adjacency at
    /// 0x11178C, and its four tables are byte for byte the France ones (SOUND.BIN is identical
    /// across the two discs, down to its length), so the offsets below hold for both.
    ///
    /// This is what replaces four hardcoded RAM addresses. Those addresses are not portable: the
    /// USA build's sound tables sit 0x3E58 higher, while its selection hotspots sit 0xB88 lower,
    /// so no single regional delta exists.
    /// </remarks>
    private int SoundTableBaseOffset => ContainerEndOffset;

    /// <summary>GHIDRA (France): DAT_8012d134, the global sound-effect VAB's three offsets.</summary>
    private const int SfxVabRelativeOffset = 0x000;

    /// <summary>GHIDRA (France): DAT_8012d13c, the sound-effect VAB group bank table.</summary>
    private const int SfxVabBankTableRelativeOffset = 0x008;

    /// <summary>GHIDRA (France): DAT_8012d398, the BGM track table.</summary>
    private const int BgmTrackTableRelativeOffset = 0x264;

    /// <summary>GHIDRA (France): BYTE_ARRAY_8012d5e4, the sound-effect record table.</summary>
    private const int SoundEffectTableRelativeOffset = 0x4B0;

    /// <summary>
    /// The loader's BGM track table.
    /// </summary>
    /// <remarks>
    /// GHIDRA: DAT_8012d398 — entries of 12 bytes (three u32). The boundaries chain into the next
    /// entry: PlayBgmTrack reads +0 and +4 for the sequence, ReadSound reads +4 and +8 for the VAB
    /// header and +8 and +12 (the next entry's first field) for the body.
    ///
    /// SOURCE: read out of LOADER.EXE and cross-checked against SOUND.BIN — 47 entries have
    /// strictly increasing offsets that stay inside the file, and the magics land where predicted
    /// ("SEQp" at each SeqOffset, "VABp" at each SeqEnd).
    ///
    /// Track 1 is the title screen's music: MainLoop @ 0x8002538c calls
    /// PromptNewGameOrContinue(0x708, 1).
    /// </remarks>
    public BgmTrack[] ReadBgmTrackTable(int soundBinLength)
    {
        var offset = SoundTableBaseOffset + BgmTrackTableRelativeOffset;
        var tracks = new List<BgmTrack>();

        for (var index = 0; ; index++)
        {
            var entry = offset + index * 12;
            if (entry + 16 > _exeBytes.Length)
            {
                break;
            }

            var seq = BitConverter.ToInt32(_exeBytes, entry);
            var seqEnd = BitConverter.ToInt32(_exeBytes, entry + 4);
            var body = BitConverter.ToInt32(_exeBytes, entry + 8);
            var bodyEnd = BitConverter.ToInt32(_exeBytes, entry + 12);

            if (seq <= 0 || seq >= seqEnd || seqEnd >= body || body >= bodyEnd || bodyEnd > soundBinLength)
            {
                break;
            }

            tracks.Add(new BgmTrack(seq, seqEnd, body, bodyEnd));
        }

        return tracks.ToArray();
    }

    /// <summary>
    /// The global sound-effect VAB — the one the menu sounds actually play from.
    /// </summary>
    /// <remarks>
    /// GHIDRA: FUN_80028504 @ 0x80028504, called from InitializeSoundDriver @ 0x80028338. It reads
    /// three fixed offsets rather than a table: head = [DAT_8012d134, DAT_8012d138), body =
    /// [DAT_8012d138, DAT_8012d13c). The handle it produces is <c>DAT_8012d5d2</c>, which is
    /// precisely the one PlaySoundEffect's direct branch (VabId == -1) passes to SsUtKeyOnV.
    ///
    /// CORRECTION: this port first used bank 0x25 of <see cref="ReadSfxVabBankTable"/> instead.
    /// That bank is a valid VAB, so the mistake survived a magic check, but it is the *SeGroup*
    /// VAB loaded by FUN_80028650 into a different handle (<c>DAT_8012d5d4</c>) for the records
    /// that reference a group. Playing the cursor sound out of it produced noise on a runaway loop,
    /// because the program/tone indices addressed unrelated samples.
    ///
    /// The three offsets sit immediately before the bank table, so the whole run is one contiguous
    /// chain of u32 offsets and DAT_8012d13c serves both as this VAB's body end and as the bank
    /// table's first entry.
    /// </remarks>
    public SfxVabBank ReadSfxVab()
    {
        var offset = SoundTableBaseOffset + SfxVabRelativeOffset;

        return new SfxVabBank(
            BitConverter.ToInt32(_exeBytes, offset),
            BitConverter.ToInt32(_exeBytes, offset + 4),
            BitConverter.ToInt32(_exeBytes, offset + 8));
    }

    /// <summary>
    /// The loader's sound-effect VAB *group* bank table.
    /// </summary>
    /// <remarks>
    /// GHIDRA: DAT_8012d13c — entries of 8 bytes, chaining into the next entry for the body's end.
    /// FUN_80028650 @ 0x80028650 loads one of these into <c>DAT_8012d5d4</c>; InitializeSoundDriver
    /// asks for 0x25.
    ///
    /// This is NOT where the menu sounds live — see <see cref="ReadSfxVab"/>. These banks serve the
    /// records whose VabId is a group reference, a branch of PlaySoundEffect that is not ported yet.
    /// </remarks>
    public SfxVabBank[] ReadSfxVabBankTable(int soundBinLength)
    {
        var offset = SoundTableBaseOffset + SfxVabBankTableRelativeOffset;
        var banks = new List<SfxVabBank>();

        for (var index = 0; ; index++)
        {
            var entry = offset + index * 8;
            if (entry + 12 > _exeBytes.Length)
            {
                break;
            }

            var head = BitConverter.ToInt32(_exeBytes, entry);
            var body = BitConverter.ToInt32(_exeBytes, entry + 4);
            var next = BitConverter.ToInt32(_exeBytes, entry + 8);

            if (head <= 0 || head >= body || body >= next || next > soundBinLength)
            {
                break;
            }

            banks.Add(new SfxVabBank(head, body, next));
        }

        return banks.ToArray();
    }

    /// <summary>Sound-effect *group* bank the loader installs at boot, for the group branch.</summary>
    /// <remarks>GHIDRA: InitializeSoundDriver @ 0x80028338 calls FUN_80028650(0x25).</remarks>
    public const int SfxVabGroupBankIndex = 0x25;

    /// <summary>
    /// The loader's sound-effect table.
    /// </summary>
    /// <remarks>
    /// GHIDRA: BYTE_ARRAY_8012d5e4 — 962 entries (the count PlaySoundEffect checks against
    /// DAT_8002031c = 0x3C2) of 22 bytes, which is 21164 bytes in total.
    ///
    /// The record layout is byte for byte the <see cref="Sound.SoundEffectRecord"/> the game
    /// already uses; LOADER.EXE simply carries its own instance of the table. Entry 1 is the cursor
    /// move (program 0, tone 0, note 60) and entry 2 the confirmation.
    /// </remarks>
    public Sound.SoundEffectRecord[] ReadSoundEffectTable()
    {
        const int recordCount = 0x3C2;
        const int recordSize = 22;

        var offset = SoundTableBaseOffset + SoundEffectTableRelativeOffset;
        var records = new Sound.SoundEffectRecord[recordCount];

        for (var index = 0; index < recordCount; index++)
        {
            var entry = offset + index * recordSize;
            if (entry + recordSize > _exeBytes.Length)
            {
                break;
            }

            records[index] = new Sound.SoundEffectRecord
            {
                VabId = BitConverter.ToInt16(_exeBytes, entry),
                ProgramNumber = BitConverter.ToInt16(_exeBytes, entry + 2),
                ToneNumber = BitConverter.ToInt16(_exeBytes, entry + 4),
                Note = BitConverter.ToInt16(_exeBytes, entry + 6),
                Flags = BitConverter.ToInt16(_exeBytes, entry + 8),
                SeqNum = BitConverter.ToInt16(_exeBytes, entry + 10),
                RefSfxId = BitConverter.ToInt16(_exeBytes, entry + 12),
                field_0x0E = BitConverter.ToInt16(_exeBytes, entry + 14),
                MaxVoices = BitConverter.ToInt16(_exeBytes, entry + 16),
                field_0x12 = BitConverter.ToInt16(_exeBytes, entry + 18),
                ToneCount = BitConverter.ToInt16(_exeBytes, entry + 20),
            };
        }

        return records;
    }

    /// <summary>
    /// The loader's proportional font metrics.
    /// </summary>
    /// <remarks>
    /// GHIDRA: g_characterPositionInSpriteSheet @ 0x80042f80 — 256 entries of 20 bytes (five int).
    ///
    /// SOURCE: the field roles come from FUN_800223ec @ 0x800223ec, the only consumer, which passes
    /// them to the tile blit as
    /// <c>Blit(dst, cursorX, cursorY + e[16], font, e[8], e[12], e[0], e[4])</c> and then advances
    /// the cursor by <c>e[0]</c>. Ghidra's FontCharacter field names are shuffled with respect to
    /// the memory order, so the mapping is stated by offset rather than by name.
    ///
    /// Entries 0 to 15 are the 16x16 cells of the sheet's first row — the animated cursor
    /// g_saveSlotBox steps through comes from there.
    /// </remarks>
    public LoaderFontCharacter[] ReadFontCharacterTable()
    {
        const int entryCount = 256;
        const int entrySize = 20;

        if (Build.FontCharacterTableAddress is not { } ramAddress)
        {
            // See the GAP note on LoaderBuild.FontCharacterTableAddress: an all-zero table means
            // every glyph is zero-width, so the text layers draw nothing rather than garbage.
            return new LoaderFontCharacter[entryCount];
        }

        var offset = RamToFileOffset(ramAddress);
        var characters = new LoaderFontCharacter[entryCount];

        for (var index = 0; index < entryCount; index++)
        {
            var entry = offset + index * entrySize;
            if (entry + entrySize > _exeBytes.Length)
            {
                break;
            }

            characters[index] = new LoaderFontCharacter(
                BitConverter.ToInt32(_exeBytes, entry),
                BitConverter.ToInt32(_exeBytes, entry + 4),
                BitConverter.ToInt32(_exeBytes, entry + 8),
                BitConverter.ToInt32(_exeBytes, entry + 12),
                BitConverter.ToInt32(_exeBytes, entry + 16));
        }

        return characters;
    }

    /// <summary>One rectangle of the selection screen's hotspot map.</summary>
    /// <param name="Code">What walking into it means: 0..3 a save slot, 6 the way out, 0x64+ a wall.</param>
    public readonly record struct SelectionHotspot(int Code, int X, int Y, int Width, int Height);

    /// <summary>
    /// The selection screen's hotspot map.
    /// </summary>
    /// <remarks>
    /// GHIDRA: DAT_800443b0 — records of five shorts (code, x, y, w, h), ending on code -1. Walked
    /// by FUN_800239c4 @ 0x800239c4, which tests the camera's *tentative* next position against
    /// every rectangle and, on a hit, returns the code without committing the move. That single
    /// detail is what makes codes 0x64 and up walls: they stop the walk and are then rejected by
    /// ValidateSelection, which only accepts 0..3.
    ///
    /// SOURCE: read out of the France build. The eleven records are the four save houses
    /// (0..3, 16x44 at x = 0x50, 0x80, 0xB0, 0xE0, y = 0x78), the exit strip along the bottom
    /// (6, 320x16 at y = 0xF0) and six walls closing the map in. The USA build carries the same
    /// 110 bytes at a different address — see <see cref="LoaderBuild.SelectionHotspotTableAddress"/>.
    /// </remarks>
    public SelectionHotspot[] ReadSelectionHotspots()
    {
        var offset = RamToFileOffset(Build.SelectionHotspotTableAddress);
        var hotspots = new List<SelectionHotspot>();

        for (var index = 0; ; index++)
        {
            var entry = offset + index * 10;
            if (entry + 10 > _exeBytes.Length)
            {
                break;
            }

            var code = BitConverter.ToInt16(_exeBytes, entry);
            if (code == -1)
            {
                break;
            }

            hotspots.Add(new SelectionHotspot(
                code,
                BitConverter.ToInt16(_exeBytes, entry + 2),
                BitConverter.ToInt16(_exeBytes, entry + 4),
                BitConverter.ToInt16(_exeBytes, entry + 6),
                BitConverter.ToInt16(_exeBytes, entry + 8)));
        }

        return hotspots.ToArray();
    }

    /// <summary>
    /// Reads a NUL-terminated byte string out of the executable at a RAM address.
    /// </summary>
    /// <remarks>
    /// Used for the two strings that drive the save-marker animation:
    /// <c>s_0_80044380</c> ("0", the resting frame) and
    /// <c>s_01234563456..._80044384</c>, which UpdateMenuGraphics @ 0x80023b14 consumes one
    /// character per frame — an opening run of 0 to 6 followed by a 3456 loop. Offset 0x24 into the
    /// second string lands on its terminator, which is how the marker parks on frame 3: reaching the
    /// NUL steps the cursor one character back, so it oscillates on the last '3' forever.
    /// </remarks>
    public byte[] ReadStringAt(uint ramAddress)
    {
        var offset = RamToFileOffset(ramAddress);
        var end = offset;
        while (end < _exeBytes.Length && _exeBytes[end] != 0)
        {
            end++;
        }

        return _exeBytes[offset..(end + 1)];
    }

    /// <summary>The marker's resting frame, at this build's address.</summary>
    public byte[] ReadSlotMarkerRestingString() => ReadStringAt(Build.SlotMarkerRestingStringAddress);

    /// <summary>The marker's animation, at this build's address.</summary>
    public byte[] ReadSlotMarkerAnimationString() => ReadStringAt(Build.SlotMarkerAnimationStringAddress);

    /// <summary>
    /// GHIDRA: ValidateSelection parks the marker on <c>s_..._80044384 + 0x24</c>, which is that
    /// string's terminator.
    /// </summary>
    public const int SlotMarkerParkedOffset = 0x24;

    public Bitmap LoadImage(int index)
    {
        if (_images.TryGetValue(index, out var cached))
        {
            return cached;
        }

        if ((uint)index >= (uint)_resources.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var offset = _resources[index].PayloadOffset;
        using var stream = new MemoryStream(_exeBytes, offset, _exeBytes.Length - offset, writable: false);
        using var br = new BinaryReader(stream);
        var bitmap = TimLoader.LoadTim(br);
        _images[index] = bitmap;
        return bitmap;
    }

    public void SaveImage(int index, string filePath) => LoadImage(index).Save(filePath, ImageFormat.Png);

    public void SaveAllImages(string directoryPath)
    {
        Directory.CreateDirectory(directoryPath);
        for (var index = 0; index < ImageCount; index++)
        {
            SaveImage(index, Path.Combine(directoryPath, $"loader_{index:D2}_{GetImageName(index)}.png"));
        }
    }

    /// <summary>What a TIM resource looks like, without decoding its pixels.</summary>
    public readonly record struct TimShape(int Width, int Height, int Bpp);

    /// <summary>
    /// Reads one resource's dimensions and bit depth straight out of its TIM header.
    /// </summary>
    /// <remarks>
    /// This is what identifies a record's role across regional builds: the container states the tag
    /// and index, the header states the shape, and the two together are enough to tell the load-room
    /// background from the font sheet without knowing either build's addresses.
    /// </remarks>
    public TimShape ReadShape(int index)
    {
        var offset = _resources[index].PayloadOffset;
        var flags = BitConverter.ToUInt32(_exeBytes, offset + 4);
        var bpp = (flags & 0x07) switch { 0 => 4, 1 => 8, 2 => 16, 3 => 24, _ => -1 };

        var headerPos = offset + 8;
        if ((flags & 0x08) != 0)
        {
            headerPos += (int)BitConverter.ToUInt32(_exeBytes, headerPos);
        }

        var widthWords = BitConverter.ToUInt16(_exeBytes, headerPos + 8);
        var height = BitConverter.ToUInt16(_exeBytes, headerPos + 10);
        var width = bpp switch { 4 => widthWords * 4, 8 => widthWords * 2, 16 => widthWords, _ => 0 };

        return new TimShape(width, height, bpp);
    }
}
