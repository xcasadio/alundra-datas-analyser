using System.Text.RegularExpressions;

namespace AlundraEngine.Loader;

/// <summary>
/// Names one record of the executable's resource container.
/// </summary>
/// <param name="Tag">The three-character tag, "TIM" or "ANM"; empty for an untagged record.</param>
/// <param name="Index">Index within that tag; -1 for an untagged record.</param>
public readonly record struct LoaderResourceKey(string Tag, int Index)
{
    /// <summary>The records past the "END" sentinel, which carry no tag.</summary>
    public static LoaderResourceKey Untagged => new(string.Empty, -1);
}

/// <summary>
/// One message the loader draws, and where this build keeps it.
/// </summary>
/// <remarks>
/// The two discs do not agree on this. France reads every message out of <c>ETC_RES.R</c>; the USA
/// build keeps them all as literals in the executable and never touches its string table for them.
///
/// The difference is not cosmetic. The executable literals carry the <c>\N</c> line breaks; the
/// matching ETC_USA.R entries do not, because that file holds the lines concatenated with their
/// trailing spaces dropped — "History Bookin Slot 1.", "detected in anyslot.", "hold thechapter".
/// Drawing those gives one long run that the text box clips, which is what a USA player saw.
/// </remarks>
public readonly record struct LoaderMessage
{
    private LoaderMessage(int etcIndex, uint? exeAddress)
    {
        EtcIndex = etcIndex;
        ExeAddress = exeAddress;
    }

    /// <summary>Index into the build's string table, or -1 when the text is not there.</summary>
    public int EtcIndex { get; }

    /// <summary>RAM address of the literal, or null when the string table supplies the text.</summary>
    public uint? ExeAddress { get; }

    /// <summary>A message the build keeps in its string table.</summary>
    public static LoaderMessage Etc(int index) => new(index, null);

    /// <summary>A message the build keeps as a literal in the executable.</summary>
    public static LoaderMessage InExecutable(uint address) => new(-1, address);
}

/// <summary>The messages the loader draws, by role.</summary>
/// <param name="UsingMemoryCard">Shown while at least one save was found.</param>
/// <param name="InsertMemoryCard">Shown when no card is present at all.</param>
/// <param name="NoSaveData">Shown when the card holds no Alundra save.</param>
/// <param name="ChooseSlot">
/// The instruction shown once the selection screen has faded in, telling the player to walk onto
/// the save they want.
/// </param>
/// <param name="ConfirmLoad">The question above the confirmation prompt.</param>
/// <param name="Yes">The confirmation prompt's left label.</param>
/// <param name="No">Its right label.</param>
public readonly record struct LoaderMessages(
    LoaderMessage UsingMemoryCard,
    LoaderMessage InsertMemoryCard,
    LoaderMessage NoSaveData,
    LoaderMessage ChooseSlot,
    LoaderMessage ConfirmLoad,
    LoaderMessage Yes,
    LoaderMessage No);

/// <summary>
/// Everything about the loader that differs from one regional build of the disc to the next.
///
/// JUSTIFICATION: PSX hardware adaptation only.
/// RELATION: the original has no such notion — each disc simply ships the executable that matches
/// it. This port reads whichever disc it is pointed at, so the handful of facts that are not
/// derivable from the executable itself are collected here rather than hardcoded to one region.
///
/// Only what cannot be derived belongs here. The RAM-to-file delta comes from the PS-EXE header,
/// the TIM resources from the embedded container, and the sound tables from where that container
/// ends — see <see cref="LoaderExeInspector"/>.
/// </summary>
public sealed record LoaderBuild
{
    /// <summary>Which disc this is.</summary>
    public required AlundraVersion Version { get; init; }

    /// <summary>
    /// Candidate file names for the loader executable, most preferred first.
    /// </summary>
    /// <remarks>
    /// CAUTION: on the France disc <c>LOADER.EXE</c> and <c>SLES_011.98</c> are the same 1226752
    /// bytes and differ by exactly one: the immediate at file offset 0x005E84 (RAM 0x80025684,
    /// inside main @ 0x80025668) is 1 in LOADER.EXE and 0 in SLES_011.98. SYSTEM.CNF boots the
    /// latter; the former is the copy CLOSING.EXE re-launches once the credits are done — see the
    /// <c>LoadExec("cdrom:\LOADER.EXE;1")</c> at the end of ClosingEngine. They are not
    /// interchangeable, so this order is deliberate: it keeps the port on the copy it was
    /// transliterated from. The USA disc carries no such second copy.
    /// </remarks>
    public required string[] ExeFileNames { get; init; }

    /// <summary>
    /// Where the full title screen lives, the image InitBootSequenceGraphics uploads to VRAM
    /// (0x180, 0).
    /// </summary>
    /// <remarks>
    /// The one resource whose place in the container differs between the two discs. France keeps it
    /// past the "END" sentinel, untagged, which is why the original reaches it by a direct pointer
    /// rather than through GetEtcResource; the USA build carries it inside the container as TIM #0.
    ///
    /// SOURCE: both decoded and compared — 320x240 8bpp, the "START / CONTINUE" screen, with the
    /// Psygnosis line on France and the Working Designs line on USA.
    /// </remarks>
    public required LoaderResourceKey TitleFull { get; init; }

    /// <summary>
    /// The image RunLoadingScreenIntro shows while the game loads, or null when the build has none.
    /// </summary>
    /// <remarks>
    /// GHIDRA: <c>GetEtcResource(g_loadRoomBackgroundTimPtr, "TIM", 7)</c> — on France that is
    /// g_licenceScreenTim, the "licensed by SCEE" screen PAL releases are required to show. The USA
    /// build has no TIM #7 at all: US discs get that screen from the console's own boot ROM, so
    /// there is nothing to display here.
    /// </remarks>
    public LoaderResourceKey? BootScreen { get; init; }

    /// <summary>
    /// The frame the title-logo animation restarts on once it has played through.
    /// </summary>
    /// <remarks>
    /// GHIDRA (France): FUN_80021a1c @ 0x80021a1c wraps the layer index from 8 back to 3 — the
    /// opening flourish walks all eight frames, then the animation settles into a five-frame loop.
    ///
    /// GHIDRA (USA): FUN_80021cb8 @ 0x80021cb8 does the same, holding each layer for 8 ticks and
    /// wrapping <c>if (layer == 9) layer = 3</c>. So the restart frame is 3 on both; the USA loop is
    /// six frames rather than five because that build carries nine ANM records instead of eight.
    ///
    /// CORRECTION: this was first set to 4 on the USA build, reasoning that the same five-frame loop
    /// would have been kept. It was not — the loop simply got longer.
    /// </remarks>
    public required int TitleAnimationLoopStart { get; init; }

    /// <summary>
    /// The string table under <c>DATA</c>, whose name carries the language.
    /// </summary>
    /// <remarks>
    /// GHIDRA: InitializePsx @ 0x80025238 calls <c>LoadEtcFile("DATA\ETC_RES.R", ...)</c> on the
    /// France build. Both discs use the same format — 1024 u16 offsets then the strings — but the
    /// USA one is called ETC_USA.R.
    /// </remarks>
    public required string EtcFileName { get; init; }

    /// <summary>
    /// The publisher movie under <c>MOVIE</c>, the one LaunchGame plays before the title menu.
    /// </summary>
    /// <remarks>
    /// GHIDRA: LaunchGame @ 0x800255f0 alternates this with ARAN_OP, which both discs share.
    /// France plays the Psygnosis logo, EURO_OP; the USA disc ships USA_OP in its place.
    /// </remarks>
    public required string PublisherMovieName { get; init; }

    /// <summary>The messages the loader draws, and where this build keeps each of them.</summary>
    public required LoaderMessages Messages { get; init; }

    /// <summary>
    /// VRAM row the save-slot pointer and yes/no cursor sample their 16x16 frames from.
    /// </summary>
    /// <remarks>
    /// GHIDRA (USA): FUN_80022d7c sets the box up with
    /// <c>(u, v) = (0x2c0, 0x170), 0x10 x 0x10, clut (0x100, 0x1e1)</c>; every other argument
    /// matches the France call this port was written from, which uses v = 0x100.
    ///
    /// France keeps its cursor frames on the font sheet's first row, which is why entries 0..15 of
    /// its metrics table are the sheet's 16x16 cells. The USA build has nothing there — its entries
    /// 0..15 are 1x1 — and puts the frames further down the sheet instead.
    /// </remarks>
    public required short SaveSlotCursorSourceY { get; init; }

    /// <summary>
    /// The selection screen's hotspot map: records of five shorts, ending on code -1.
    /// </summary>
    /// <remarks>
    /// GHIDRA (France): DAT_800443b0, walked by FUN_800239c4 @ 0x800239c4.
    ///
    /// SOURCE (USA): the same 110 bytes, byte for byte, at 0x80043828 — the map is the same eleven
    /// rectangles, only relocated.
    /// </remarks>
    public required uint SelectionHotspotTableAddress { get; init; }

    /// <summary>The save marker's resting frame, the string "0".</summary>
    /// <remarks>
    /// GHIDRA (France): s_0_80044380. SOURCE (USA): the identical pair four bytes below, at
    /// 0x800437F8, confirmed by reading both strings back.
    /// </remarks>
    public required uint SlotMarkerRestingStringAddress { get; init; }

    /// <summary>The save marker's animation, "0123456345634563456345634563".</summary>
    /// <remarks>GHIDRA (France): s_01234563456..._80044384. SOURCE (USA): 0x800437FC, identical.</remarks>
    public required uint SlotMarkerAnimationStringAddress { get; init; }

    /// <summary>The proportional font's metrics: entries of five ints, one per character code.</summary>
    /// <remarks>
    /// GHIDRA (France): g_characterPositionInSpriteSheet @ 0x80042f80, consumed by FUN_800223ec.
    ///
    /// GHIDRA (USA): 0x80042df8, consumed by FUN_80022718 @ 0x80022718, which indexes it at a
    /// stride of 0x14 and hands the fields to the blit in the same order France does —
    /// <c>Blit(dst, cursorX, cursorY + e[16], font, e[8], e[12], e[0], e[4])</c>. The five field
    /// loads are what fix the base: e[0] at 0x80042df8, e[4] at 0x80042dfc, e[8] at 0x80042e00,
    /// e[12] at 0x80042e04 and e[16] at 0x80042e08.
    ///
    /// This is the one table that could not be carried over by content — it describes each build's
    /// own font sheet, so the bytes genuinely differ.
    /// </remarks>
    public required uint FontCharacterTableAddress { get; init; }

    /// <summary>Pixels added to a glyph's own width when advancing the text cursor.</summary>
    /// <remarks>
    /// GHIDRA (USA): FUN_80022718 advances with <c>cursorX = cursorX + 1 + width</c> on the
    /// single-byte path, the same <c>1 +</c> the two-byte path uses.
    ///
    /// GHIDRA (France): DrawGlyphToTileMap @ 0x800223ec advances with <c>cursorX = cursorX + width</c>
    /// there, and keeps the <c>1 +</c> for the two-byte path only. The two discs genuinely differ,
    /// so this is a real per-build value rather than an unverified carry-over.
    /// </remarks>
    public required int GlyphAdvancePadding { get; init; }

    /// <summary>Number of entries the font metrics table holds.</summary>
    /// <remarks>
    /// GHIDRA (USA): FUN_80022718 gates the table on <c>(code &amp; 0xffff) &lt; 0x80</c> and sends
    /// everything else to the kanji path, so its table stops at 128. GHIDRA (France):
    /// DrawGlyphToTileMap @ 0x800223ec gates on <c>&lt; 0x100</c>, so its table holds 256.
    ///
    /// The difference is not observable: the text walker only ever forms a code below 0x80 or a
    /// two-byte code of 0x8000 and up, so entries 0x80..0xFF are never indexed on either build.
    /// It is stated per build because it is what each one actually carries.
    /// </remarks>
    public required int FontCharacterCount { get; init; }

    /// <summary>The France disc: SLES-01198, boots SLES_011.98.</summary>
    public static readonly LoaderBuild France = new()
    {
        Version = AlundraVersion.European,
        ExeFileNames = ["LOADER.EXE", "SLES_011.98"],
        TitleFull = LoaderResourceKey.Untagged,
        BootScreen = new LoaderResourceKey("TIM", 7),
        TitleAnimationLoopStart = 3,
        EtcFileName = "ETC_RES.R",
        PublisherMovieName = "EURO_OP",
        Messages = new LoaderMessages(
            UsingMemoryCard: LoaderMessage.Etc(0xC1),
            InsertMemoryCard: LoaderMessage.Etc(0xC2),
            NoSaveData: LoaderMessage.Etc(0xC8),
            ChooseSlot: LoaderMessage.Etc(0xC7),
            ConfirmLoad: LoaderMessage.Etc(0xC9),
            Yes: LoaderMessage.Etc(0xCA),
            No: LoaderMessage.Etc(0xCB)),
        SelectionHotspotTableAddress = 0x800443B0,
        SlotMarkerRestingStringAddress = 0x80044380,
        SlotMarkerAnimationStringAddress = 0x80044384,
        FontCharacterTableAddress = 0x80042F80,
        FontCharacterCount = 256,
        GlyphAdvancePadding = 0,
        SaveSlotCursorSourceY = 0x100,
    };

    /// <summary>The USA 1.1 disc: SLUS-00553, boots SLUS_005.53.</summary>
    public static readonly LoaderBuild Usa = new()
    {
        Version = AlundraVersion.Usa,
        ExeFileNames = ["SLUS_005.53"],
        TitleFull = new LoaderResourceKey("TIM", 0),
        BootScreen = null,
        TitleAnimationLoopStart = 3,
        EtcFileName = "ETC_USA.R",
        PublisherMovieName = "USA_OP",

        // GHIDRA: this build never reads its string table for these. FUN_800248b4 points the layer
        // at 0x8002020c, FUN_80024bf8 at 0x80020284, FUN_80024e78 assigns 0x8002032c, and
        // FUN_80022d7c draws 0x800200e4 / 0x800200e8 as the two confirmation labels. The remaining
        // two were read back from the same run of literals.
        //
        // ETC_USA.R does hold near-matches one index below the France ones, and using those was the
        // first attempt here. They are the wrong text: they lack the \N line breaks and read "in
        // Slot 1." where the executable says "in Memory Card Slot 1.".
        Messages = new LoaderMessages(
            UsingMemoryCard: LoaderMessage.InExecutable(0x80020258),
            InsertMemoryCard: LoaderMessage.InExecutable(0x8002010C),
            NoSaveData: LoaderMessage.InExecutable(0x8002020C),
            ChooseSlot: LoaderMessage.InExecutable(0x80020284),
            ConfirmLoad: LoaderMessage.InExecutable(0x8002032C),
            Yes: LoaderMessage.InExecutable(0x800200E4),
            No: LoaderMessage.InExecutable(0x800200E8)),
        SelectionHotspotTableAddress = 0x80043828,
        SlotMarkerRestingStringAddress = 0x800437F8,
        SlotMarkerAnimationStringAddress = 0x800437FC,
        FontCharacterTableAddress = 0x80042DF8,
        FontCharacterCount = 128,
        GlyphAdvancePadding = 1,
        SaveSlotCursorSourceY = 0x170,
    };

    private static readonly LoaderBuild[] KnownBuilds = [France, Usa];

    /// <summary>
    /// Works out which disc <paramref name="gamePath"/> holds.
    /// </summary>
    /// <remarks>
    /// <c>SYSTEM.CNF</c> is the authority: its <c>BOOT</c> line names the executable the console
    /// runs, and that name carries the region code. Falling back to looking for the executables
    /// themselves covers extractions where SYSTEM.CNF was not kept.
    /// </remarks>
    public static LoaderBuild? Detect(string gamePath)
    {
        var bootName = ReadBootExeName(gamePath);
        if (bootName != null)
        {
            foreach (var build in KnownBuilds)
            {
                if (build.ExeFileNames.Contains(bootName, StringComparer.OrdinalIgnoreCase))
                {
                    return build;
                }
            }
        }

        foreach (var build in KnownBuilds)
        {
            if (build.ExeFileNames.Any(name => File.Exists(Path.Combine(gamePath, name))))
            {
                return build;
            }
        }

        return null;
    }

    /// <summary>Full path of the loader executable on this disc, or null when none is present.</summary>
    public string? FindExeFilePath(string gamePath)
    {
        foreach (var name in ExeFileNames)
        {
            var path = Path.Combine(gamePath, name);
            if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }

    /// <summary>Pulls the executable name out of the <c>BOOT = cdrom:\NAME;1</c> line.</summary>
    private static string? ReadBootExeName(string gamePath)
    {
        var systemCnf = Path.Combine(gamePath, "SYSTEM.CNF");
        if (!File.Exists(systemCnf))
        {
            return null;
        }

        var match = Regex.Match(
            File.ReadAllText(systemCnf),
            @"^\s*BOOT\s*=\s*cdrom:\\?(?<name>[^;\s]+)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        return match.Success ? match.Groups["name"].Value : null;
    }
}
