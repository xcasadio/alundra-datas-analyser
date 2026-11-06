namespace AlundraEngine.Sound;

// JUSTIFICATION: PSX hardware adaptation only
// RELATION: the cadence of the sound sequencer belongs to the host of the sound engine, not to the game code.
// The PAL binary (ALUN_CD.EXE, France) calls SsSetTickMode(4) once (0x8004859C -> FUN_8008EC8C). Its case 4 writes 0x32
// to the rate global 0x801F6CE0 (0x8008ED1C-0x8008ED24), the value the tempo formula divides by, and on a PAL console
// writes 5 to the tick mode 0x800C9774 (0x8008ED30, 0x8008ED60): one sequencer tick per VBlank. Alundra drives the GPU
// in PAL non-interlaced mode (PutDispEnv 0x80085518), whose VBlank runs at 49.761 Hz (psx-spx, GPU Timings, "PAL mode on
// PAL video clock, Non-interlaced"). The desktop runtime of this analyser (AlundraGame) ticks from its audio thread at
// 60 Hz and keeps the rate at 60. The host chooses the profile before InitializeSoundSystem.
// Parent repository: docs/plan-bgm-tick-pal.md (P2, D6), docs/plan-bgm-tick-pal-annexe/t0.1-pal-rate.md.
public sealed class SequencerTickProfile
{
    public const int OutputSampleRate = 44100;

    private SequencerTickProfile(string name, int driverRate, int tickMode, long tickRateNumerator, long tickRateDenominator)
    {
        Name = name;
        DriverRate = driverRate;
        TickMode = tickMode;
        TickRateNumerator = tickRateNumerator;
        TickRateDenominator = tickRateDenominator;
    }

    /// <summary>
    /// The desktop runtime: rate 60 and a 60 Hz tick, 735 output samples per tick. The default of the sound engine.
    /// Departs from the PAL binary on purpose: AlundraGame plays at 60 Hz.
    /// </summary>
    public static SequencerTickProfile Desktop60 { get; } = new("desktop60", 0x3C, 4, 60, 1);

    /// <summary>The PAL binary: rate 0x32 (50), tick mode 5, one tick per PAL VBlank at 49.761 Hz.</summary>
    public static SequencerTickProfile PalBinary { get; } = new("pal", 0x32, 5, 49761, 1000);

    public string Name { get; }

    /// <summary>The value FUN_8008ec8c(4) writes to the rate global 0x801F6CE0.</summary>
    public int DriverRate { get; }

    /// <summary>The value FUN_8008ec8c(4) leaves in the tick mode global 0x800C9774.</summary>
    public int TickMode { get; }

    /// <summary>Tick rate as an exact fraction, numerator (ticks per <see cref="TickRateDenominator"/> seconds).</summary>
    public long TickRateNumerator { get; }

    public long TickRateDenominator { get; }

    /// <summary>Ticks per second.</summary>
    public double TickRate => (double)TickRateNumerator / TickRateDenominator;

    /// <summary>The first output sample of tick n: floor(n * 44100 / f).</summary>
    public long StartSample(long tick) => tick * OutputSampleRate * TickRateDenominator / TickRateNumerator;

    /// <summary>The number of output samples of tick n.</summary>
    public int SamplesInTick(long tick) => (int)(StartSample(tick + 1) - StartSample(tick));

    /// <summary>The longest tick, in output samples: ceil(44100 / f) (735 at 60 Hz, 887 at 49.761 Hz).</summary>
    public int MaxSamplesInTick => (int)((OutputSampleRate * TickRateDenominator + TickRateNumerator - 1) / TickRateNumerator);

    /// <summary>The tick n that holds output sample i: StartSample(n) &lt;= i &lt; StartSample(n + 1).</summary>
    public long TickOfSample(long sample) => ((sample + 1) * TickRateNumerator - 1) / (OutputSampleRate * TickRateDenominator);

    /// <summary>The number of whole ticks that cover a duration in seconds, rounded up.</summary>
    public int TicksForSeconds(double seconds) => (int)Math.Ceiling(seconds * TickRate);

    /// <summary>Profile by name: "pal" or "desktop60".</summary>
    public static SequencerTickProfile Parse(string name) => name.ToLowerInvariant() switch
    {
        "pal" => PalBinary,
        "desktop60" => Desktop60,
        _ => throw new ArgumentException($"unknown sequencer tick profile '{name}' (expected pal or desktop60)", nameof(name)),
    };
}
