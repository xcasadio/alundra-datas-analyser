namespace AlundraEngine.Sound;

/// <summary>Which of the three note-to-pitch call sites asked for a table index. Kept separate so the
/// hit counts can be reported per site: the sequencer site is the only one known to fire, and the other
/// two must be able to PROVE they stayed inert rather than be assumed to
/// (docs/plan-extraction-bgm.md, decision D-X-6).</summary>
public enum VoicePitchSite
{
    /// <summary>The sequencer note-on path (<c>FUN_80091b60</c>).</summary>
    Sequencer,

    /// <summary>The sound-effect trigger path (<c>SoundManager.CalculateVoicePitch</c>).</summary>
    Sfx,

    /// <summary>The offline exporter's sample-rate derivation (<c>SoundBin.CalculateToneRawPitch</c>).</summary>
    Exporter,
}

/// <summary>
/// The one and only policy point for an out-of-range voice-pitch table index.
///
/// <para>WHAT THE ORIGINAL DOES, and why this class is a DEVIATION rather than a fix. The three
/// note-to-pitch sites are transliterations of Sony's libsnd <c>note2pitch</c>, and they are FAITHFUL:
/// verified against two independent reference decompilations, the 192-entry pitch table matches
/// byte for byte and the canonical code performs the same TRUNCATING division and remainder. The
/// <c>+0x3C</c> bias in that algorithm covers notes down to five octaves below the tone's centre, and
/// nothing in it handles a note below that.</para>
///
/// <para>Retail data goes past it anyway. BGM track 19's program 1 carries a full-range layer
/// (<c>min 0, max 127</c>) whose centre is 100, so its own score's note 38 sits 62 semitones below the
/// centre - two too many. The remainder goes negative and the computed index is <c>-23</c>. On the PSX
/// that is harmless: C has no bounds check, so the console reads the halfword sitting BEFORE the table
/// and carries on. In C# the array throws.</para>
///
/// <para><b>The original's result is not reproducible, and no tooling changes that.</b> In the
/// reference link the memory immediately before the table is libsnd RUNTIME state - written during
/// play, not constant data - so there is no fixed value to port. Every possible behaviour here is a
/// deviation; this class picks the one that invents the least.</para>
///
/// <para>CHOSEN POLICY: refuse the voice. It adds nothing that was not there, and it is deterministic.
/// The alternative worth knowing about is a FLOORED (Euclidean) decomposition, which yields the
/// musically exact pitch (index 169 and one more octave down, for the case above) and is identical to
/// the current arithmetic for every non-negative delta - i.e. everywhere that works today. It was NOT
/// chosen because it would make a layer audible that the PSX never played properly: an improvement,
/// not a restoration. Switching is a one-line change to <see cref="TryGetTableIndex"/> and nowhere
/// else, which is the whole reason this policy lives in one place.</para>
/// </summary>
public static class VoicePitchGuard
{
    /// <summary>12 semitones x 16 fine steps. Both copies of the table in this assembly are this long.</summary>
    public const int TableLength = 192;

    private static readonly int[] s_hits = new int[3];

    /// <summary>
    /// Computes the pitch-table index the canonical algorithm asks for, and reports whether it is
    /// inside the table. Returns <see langword="false"/> exactly where the original read out of
    /// bounds - the caller then refuses the voice (see this class's own doc for why that is a choice
    /// and not a restoration).
    /// </summary>
    public static bool TryGetTableIndex(int semitoneRemainder, int fineIndex, VoicePitchSite site, out int index)
    {
        index = (semitoneRemainder << 4) + fineIndex;
        if ((uint)index < TableLength)
        {
            return true;
        }

        s_hits[(int)site]++;
        return false;
    }

    /// <summary>How many times <paramref name="site"/> asked for an out-of-range index this process.</summary>
    public static int Hits(VoicePitchSite site) => s_hits[(int)site];

    /// <summary>One line naming every site's count, for a run to print. The acceptance of
    /// docs/plan-extraction-bgm.md requires the SFX and exporter counts to read zero: that is what
    /// turns "those two guards are inert" from an assumption into a measurement.</summary>
    public static string FormatReport()
        => $"pitch guard hits: sequencer={s_hits[(int)VoicePitchSite.Sequencer]} "
           + $"sfx={s_hits[(int)VoicePitchSite.Sfx]} exporter={s_hits[(int)VoicePitchSite.Exporter]}";

    /// <summary>Resets the counters, so a caller can attribute hits to one stretch of work.</summary>
    public static void ResetHits() => Array.Clear(s_hits);
}
