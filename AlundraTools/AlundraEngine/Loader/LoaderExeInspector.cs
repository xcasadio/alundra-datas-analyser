using AlundraEngine.Graphics;
using System.Drawing.Imaging;

namespace AlundraEngine.Loader;

/// <summary>
/// Gives access to the 16 TIM resources embedded in LOADER.EXE, plus the raw executable bytes.
///
/// JUSTIFICATION: PSX hardware adaptation only.
/// RELATION: on the console these resources are simply resident in RAM at fixed addresses once the
/// executable is loaded, and the code dereferences them directly. On desktop the executable is a
/// file, so each resource is reached by a file offset instead.
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

    /// <summary>
    /// The embedded TIM resources, in the order jPSXdec catalogues them in <c>loader.idx</c>.
    ///
    /// SOURCE: each entry's file offset resolves from the index as
    /// <c>sectorStart * 2048 + startOffset</c>; all sixteen were verified to land on a valid TIM
    /// header whose dimensions and bit depth match the index. Adding
    /// <see cref="RamToFileOffsetDelta"/> to each offset lands on an already-named Ghidra symbol,
    /// which is where the names below come from.
    ///
    /// Unlike <see cref="Closing.ClosingExeInspector"/> these are hardcoded rather than located by
    /// scanning for a matching TIM signature. The eight title frames share one signature
    /// (320x160, 8bpp, with CLUT), so a signature scan could only tell them apart by file order
    /// anyway, and the offsets here are backed by named symbols.
    ///
    /// CAUTION: established on the France build. The USA build may order these differently — the
    /// same check must be re-run against its LOADER.EXE before this table is trusted there.
    /// </summary>
    private static readonly (string Name, int FileOffset, int Width, int Height, int Bpp)[] Resources =
    [
        ("g_loadRoomBackgroundTimBuffer", 0x024C38, 320, 384,  8), //  0
        ("g_loadRoomCloudsTim",           0x042E60, 320, 128,  4), //  1
        ("g_loadRoomBackgroundMessageTim",0x047EA8, 256, 256,  4), //  2
        ("g_loadRoomFontTim",             0x04FEF0, 256, 256,  4), //  3
        ("g_loadRoomSpriteSheetTim",      0x057F38, 256, 256,  8), //  4
        ("g_loadingScreenTim",            0x068160, 320, 240, 16), //  5
        ("g_licenceScreenTim",            0x08D97C, 256, 256,  4), //  6
        ("g_TitleFrame0",                 0x0959C4, 320, 160,  8), //  7
        ("g_TitleFrame1",                 0x0A23EC, 320, 160,  8), //  8
        ("g_TitleFrame2",                 0x0AEE14, 320, 160,  8), //  9
        ("g_TitleFrame3",                 0x0BB83C, 320, 160,  8), // 10
        ("g_TitleFrame4",                 0x0C8264, 320, 160,  8), // 11
        ("g_TitleFrame5",                 0x0D4C8C, 320, 160,  8), // 12
        ("g_TitleFrame6",                 0x0E16B4, 320, 160,  8), // 13
        ("g_TitleFrame7",                 0x0EE0DC, 320, 160,  8), // 14
        ("g_TitleFull",                   0x0FAB0C, 320, 240,  8), // 15
    ];

    /// <summary>Index of the first frame of the animated title logo.</summary>
    public const int TitleFrame0Index = 7;

    /// <summary>Number of frames in the animated title logo.</summary>
    public const int TitleFrameCount = 8;

    /// <summary>Index of the full title screen image.</summary>
    public const int TitleFullIndex = 15;

    /// <summary>Index of the boot loading screen.</summary>
    public const int LoadingScreenIndex = 5;

    /// <summary>Index of the licence screen.</summary>
    public const int LicenceScreenIndex = 6;

    private readonly byte[] _exeBytes;
    private readonly Dictionary<int, Bitmap> _images = new();

    public static int ImageCount => Resources.Length;

    /// <summary>Raw LOADER.EXE bytes, for reading data tables that sit alongside the TIMs.</summary>
    public byte[] ExeBytes => _exeBytes;

    public LoaderExeInspector(string gamePath)
    {
        var exeFilePath = Path.Combine(gamePath, "LOADER.EXE");
        _exeBytes = File.ReadAllBytes(exeFilePath);
        RamToFileOffsetDelta = ReadRamToFileOffsetDelta(_exeBytes, exeFilePath);
        ValidateResourceTable();
    }

    public int GetImageFileOffset(int index) => Resources[index].FileOffset;

    public string GetImageName(int index) => Resources[index].Name;

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
    /// <param name="Name">Three-character tag, "TIM" or "ANM".</param>
    /// <param name="Index">Index within that tag, 1-based.</param>
    /// <param name="PayloadOffset">File offset of the payload, i.e. of the TIM's magic word.</param>
    /// <param name="PayloadSize">Payload length in bytes.</param>
    public readonly record struct EtcResource(string Name, int Index, int PayloadOffset, int PayloadSize);

    /// <summary>
    /// Walks the resource container LOADER.EXE carries, the one <c>g_loadRoomBackgroundTimPtr</c>
    /// points at.
    /// </summary>
    /// <remarks>
    /// GHIDRA: GetEtcResource @ 0x800276bc. Each entry is
    /// <c>[u32 packedId][u32 payloadSize][payload]</c>, where the id packs the three tag characters
    /// little-endian plus the index in the top byte, and the list ends on the sentinel
    /// <c>0xFF444E45</c> ("END" plus 0xFF).
    ///
    /// SOURCE: walked directly in the France build and cross-checked against loader.idx — the 15
    /// payload offsets land exactly on catalogue entries 0..14. That identifies the eight "ANM"
    /// entries as g_TitleFrame0..7, the frames FUN_80021a1c cycles into VRAM to animate the title
    /// logo, and "TIM" #3 as the selection screen's backdrop.
    ///
    /// g_TitleFull is deliberately absent: it sits past the sentinel and InitBootSequenceGraphics
    /// passes it to InitializeTileLayer directly rather than through GetEtcResource.
    /// </remarks>
    public EtcResource[] ReadEtcResources()
    {
        // The container starts eight bytes before the first catalogued TIM, i.e. at that entry's
        // header rather than its payload.
        var position = Resources[0].FileOffset - 8;
        var entries = new List<EtcResource>();

        while (position + 8 <= _exeBytes.Length)
        {
            var packedId = BitConverter.ToUInt32(_exeBytes, position);
            if (packedId == 0xFF444E45)
            {
                break;
            }

            var payloadSize = (int)BitConverter.ToUInt32(_exeBytes, position + 4);
            var name = new string([(char)(packedId & 0xFF), (char)((packedId >> 8) & 0xFF), (char)((packedId >> 16) & 0xFF)]);
            var index = (int)((packedId >> 24) & 0xFF);

            if (payloadSize <= 0 || position + 8 + payloadSize > _exeBytes.Length || !name.All(char.IsAsciiLetterUpper))
            {
                throw new InvalidDataException(
                    $"LOADER.EXE: malformed resource entry at 0x{position:X6} (id 0x{packedId:X8}, size 0x{payloadSize:X}).");
            }

            entries.Add(new EtcResource(name, index, position + 8, payloadSize));
            position += 8 + payloadSize;
        }

        return entries.ToArray();
    }

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

    /// <summary>Where one BGM track's data lives inside SOUND.BIN.</summary>
    /// <param name="SeqOffset">Start of the sequence.</param>
    /// <param name="SeqEnd">End of the sequence, which is also the start of the VAB header.</param>
    /// <param name="VabBodyOffset">Start of the VAB body.</param>
    /// <param name="VabBodyEnd">End of the VAB body.</param>
    public readonly record struct BgmTrack(int SeqOffset, int SeqEnd, int VabBodyOffset, int VabBodyEnd);

    /// <summary>Where one sound-effect VAB bank lives inside SOUND.BIN.</summary>
    public readonly record struct SfxVabBank(int HeaderOffset, int BodyOffset, int BodyEnd);

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
        const uint ramAddress = 0x8012D398;
        var offset = RamToFileOffset(ramAddress);
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
        const uint ramAddress = 0x8012D134;
        var offset = RamToFileOffset(ramAddress);

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
        const uint ramAddress = 0x8012D13C;
        var offset = RamToFileOffset(ramAddress);
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
        const uint ramAddress = 0x8012D5E4;
        const int recordCount = 0x3C2;
        const int recordSize = 22;

        var offset = RamToFileOffset(ramAddress);
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
        const uint ramAddress = 0x80042F80;
        const int entryCount = 256;
        const int entrySize = 20;

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
    /// (6, 320x16 at y = 0xF0) and six walls closing the map in.
    /// </remarks>
    public SelectionHotspot[] ReadSelectionHotspots()
    {
        const uint ramAddress = 0x800443B0;
        var offset = RamToFileOffset(ramAddress);
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

    /// <summary>GHIDRA: s_0_80044380 — the marker's resting frame.</summary>
    public const uint SlotMarkerRestingStringAddress = 0x80044380;

    /// <summary>GHIDRA: s_01234563456345634563456345634563_80044384 — the marker's animation.</summary>
    public const uint SlotMarkerAnimationStringAddress = 0x80044384;

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

        if ((uint)index >= (uint)Resources.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var offset = Resources[index].FileOffset;
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
            SaveImage(index, Path.Combine(directoryPath, $"loader_{index:D2}_{Resources[index].Name}.png"));
        }
    }

    /// <summary>
    /// Fails fast if the hardcoded offsets do not point at TIM headers of the expected shape,
    /// which is what a different regional build would look like.
    /// </summary>
    private void ValidateResourceTable()
    {
        for (var index = 0; index < Resources.Length; index++)
        {
            var (name, offset, width, height, bpp) = Resources[index];
            if (offset < 0 || offset + 20 > _exeBytes.Length)
            {
                throw new InvalidDataException($"LOADER.EXE: resource #{index} ({name}) is outside the file.");
            }

            if (BitConverter.ToUInt32(_exeBytes, offset) != 0x10)
            {
                throw new InvalidDataException($"LOADER.EXE: resource #{index} ({name}) at 0x{offset:X6} is not a TIM.");
            }

            var flags = BitConverter.ToUInt32(_exeBytes, offset + 4);
            var actualBpp = (flags & 0x07) switch { 0 => 4, 1 => 8, 2 => 16, 3 => 24, _ => -1 };
            var headerPos = offset + 8;
            if ((flags & 0x08) != 0)
            {
                headerPos += (int)BitConverter.ToUInt32(_exeBytes, headerPos);
            }

            var widthWords = BitConverter.ToUInt16(_exeBytes, headerPos + 8);
            var actualHeight = BitConverter.ToUInt16(_exeBytes, headerPos + 10);
            var actualWidth = actualBpp switch { 4 => widthWords * 4, 8 => widthWords * 2, 16 => widthWords, _ => 0 };

            if (actualBpp != bpp || actualWidth != width || actualHeight != height)
            {
                throw new InvalidDataException(
                    $"LOADER.EXE: resource #{index} ({name}) is {actualWidth}x{actualHeight} {actualBpp}bpp, expected {width}x{height} {bpp}bpp. " +
                    "This is most likely a different regional build; the offset table in LoaderExeInspector was established on the France version.");
            }
        }
    }
}
