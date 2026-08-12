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

    /// <summary>The France disc: SLES-01198, boots SLES_011.98.</summary>
    public static readonly LoaderBuild France = new()
    {
        Version = AlundraVersion.European,
        ExeFileNames = ["LOADER.EXE", "SLES_011.98"],
        TitleFull = LoaderResourceKey.Untagged,
        BootScreen = new LoaderResourceKey("TIM", 7),
        TitleAnimationLoopStart = 3,
    };

    /// <summary>The USA 1.1 disc: SLUS-00553, boots SLUS_005.53.</summary>
    public static readonly LoaderBuild Usa = new()
    {
        Version = AlundraVersion.Usa,
        ExeFileNames = ["SLUS_005.53"],
        TitleFull = new LoaderResourceKey("TIM", 0),
        BootScreen = null,
        TitleAnimationLoopStart = 4,
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
