using System.Text.RegularExpressions;

namespace AlundraEngine.Loader;

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

    /// <summary>The France disc: SLES-01198, boots SLES_011.98.</summary>
    public static readonly LoaderBuild France = new()
    {
        Version = AlundraVersion.European,
        ExeFileNames = ["LOADER.EXE", "SLES_011.98"],
    };

    /// <summary>The USA 1.1 disc: SLUS-00553, boots SLUS_005.53.</summary>
    public static readonly LoaderBuild Usa = new()
    {
        Version = AlundraVersion.Usa,
        ExeFileNames = ["SLUS_005.53"],
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
