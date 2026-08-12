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
/// The entries of <c>ETC_RES.R</c> the loader draws, by role.
/// </summary>
/// <param name="UsingMemoryCard">Shown while at least one save was found.</param>
/// <param name="InsertMemoryCard">Shown when no card is present at all.</param>
/// <param name="NoSaveData">Shown when the card holds no Alundra save.</param>
/// <param name="ConfirmLoad">The question above the confirmation prompt.</param>
/// <param name="Yes">The confirmation prompt's left label, or -1 when the build has none.</param>
/// <param name="No">Its right label, or -1 when the build has none.</param>
/// <remarks>
/// The indices are not stable across regions. The USA table runs one lower throughout — its 0xC0 is
/// the France 0xC1, its 0xC7 the France 0xC8 — and stops at 0xC8: every index above that reads
/// 0xFFFF, the table's "no entry" marker.
///
/// GAP: that truncation is why <see cref="Yes"/> and <see cref="No"/> are -1 on the USA build. Its
/// confirmation prompt has to source those two labels somewhere this port has not found yet; the
/// drawing calls fail soft on -1, so the panel comes up without them. Reading the indices
/// SetSpriteImage is given in that build's equivalent of the selection screen is what would close it.
/// </remarks>
public readonly record struct LoaderStringIds(
    int UsingMemoryCard,
    int InsertMemoryCard,
    int NoSaveData,
    int ConfirmLoad,
    int Yes,
    int No)
{
    /// <summary>The value both drawing helpers treat as "nothing to draw".</summary>
    public const int Absent = -1;
}

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
    /// PROBABLE on USA: that build carries nine ANM records rather than eight, and 4 keeps the same
    /// five-frame loop. Not read off its code — confirm against its FUN_80021a1c before trusting it.
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

    /// <summary>Which entries of <see cref="EtcFileName"/> the loader draws.</summary>
    public required LoaderStringIds Strings { get; init; }

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

    /// <summary>Number of entries the font metrics table holds.</summary>
    /// <remarks>
    /// GHIDRA (USA): FUN_80022718 gates the table on <c>(code &amp; 0xffff) &lt; 0x80</c> and sends
    /// everything else to the kanji path, so its table stops at 128. France's port reads 256.
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
        Strings = new LoaderStringIds(
            UsingMemoryCard: 0xC1,
            InsertMemoryCard: 0xC2,
            NoSaveData: 0xC8,
            ConfirmLoad: 0xC9,
            Yes: 0xCA,
            No: 0xCB),
        SelectionHotspotTableAddress = 0x800443B0,
        SlotMarkerRestingStringAddress = 0x80044380,
        SlotMarkerAnimationStringAddress = 0x80044384,
        FontCharacterTableAddress = 0x80042F80,
        FontCharacterCount = 256,
    };

    /// <summary>The USA 1.1 disc: SLUS-00553, boots SLUS_005.53.</summary>
    public static readonly LoaderBuild Usa = new()
    {
        Version = AlundraVersion.Usa,
        ExeFileNames = ["SLUS_005.53"],
        TitleFull = new LoaderResourceKey("TIM", 0),
        BootScreen = null,
        TitleAnimationLoopStart = 4,
        EtcFileName = "ETC_USA.R",
        PublisherMovieName = "USA_OP",

        // SOURCE: matched against the France entries by meaning, one index lower throughout —
        // 0xC0 "Using the Book in Slot 1.", 0xC1 "Please insert a History Book in Slot 1.",
        // 0xC7 "No record exists of Alundra's adventures.", 0xC8 "Would you like to rejoin the
        // tale in this chapter?". The table ends there; see the GAP note on LoaderStringIds.
        Strings = new LoaderStringIds(
            UsingMemoryCard: 0xC0,
            InsertMemoryCard: 0xC1,
            NoSaveData: 0xC7,
            ConfirmLoad: 0xC8,
            Yes: LoaderStringIds.Absent,
            No: LoaderStringIds.Absent),
        SelectionHotspotTableAddress = 0x80043828,
        SlotMarkerRestingStringAddress = 0x800437F8,
        SlotMarkerAnimationStringAddress = 0x800437FC,
        FontCharacterTableAddress = 0x80042DF8,
        FontCharacterCount = 128,
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
