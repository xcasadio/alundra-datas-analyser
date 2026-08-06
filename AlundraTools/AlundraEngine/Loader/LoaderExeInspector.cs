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
    /// <summary>
    /// LOADER.EXE loads at <c>t_addr = 0x80020000</c> and its PS-EXE header occupies the first
    /// 0x800 bytes, so <c>fileOffset = ramAddress - 0x8001F800</c>. Same delta as CLOSING.EXE.
    /// </summary>
    public const uint RamToFileOffsetDelta = 0x8001F800;

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
        ValidateResourceTable();
    }

    public int GetImageFileOffset(int index) => Resources[index].FileOffset;

    public string GetImageName(int index) => Resources[index].Name;

    /// <summary>Converts a RAM address from Ghidra into an offset into <see cref="ExeBytes"/>.</summary>
    public static int RamToFileOffset(uint ramAddress) => (int)(ramAddress - RamToFileOffsetDelta);

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
    /// The loader's sound-effect VAB bank table.
    /// </summary>
    /// <remarks>
    /// GHIDRA: DAT_8012d13c — entries of 8 bytes, again chaining into the next entry for the body's
    /// end. FUN_80028650 @ 0x80028650, called from InitializeSoundDriver with 0x25, is what loads
    /// the bank the menu sounds play from.
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

    /// <summary>Sound-effect bank the loader installs at boot.</summary>
    /// <remarks>GHIDRA: InitializeSoundDriver @ 0x80028338 calls FUN_80028650(0x25).</remarks>
    public const int SfxVabBankIndex = 0x25;

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
