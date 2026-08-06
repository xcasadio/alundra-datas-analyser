namespace PsxSdk.Video;

/// <summary>
/// Playback parameters, matching the six arguments of the original movie entry point.
/// </summary>
/// <remarks>
/// GHIDRA: PlayMovie @ 0x80027ff4 (LOADER.EXE),
/// <c>PlayMovie(param_1, param_2, fileName, param_4, param_5, param_6)</c>.
/// </remarks>
public sealed class MoviePlaybackOptions
{
    /// <summary>Horizontal position of the movie on screen. Original <c>param_1</c>.</summary>
    public int ScreenX { get; init; }

    /// <summary>
    /// Vertical position base. The original display environment uses <c>ScreenY + 0x18</c>.
    /// Original <c>param_2</c>.
    /// </summary>
    public int ScreenY { get; init; }

    /// <summary>
    /// Frame number at which the original stops playback. Original <c>param_4</c>.
    /// Ignored unless <see cref="StopAtLastFrame"/> is set.
    /// </summary>
    public int LastFrame { get; init; }

    /// <summary>
    /// DELIBERATE DEVIATION from the original, requested for this port.
    ///
    /// The original passes a hard frame limit (0xAF4 = 2804 for EURO_OP.MOV, 0xEAB = 3755 for
    /// ARAN_OP.MOV) that is *below* the real frame count of those files (2820 and 3765): the last
    /// second or so of each movie is cut off. Leaving this false plays every stream to its natural
    /// end instead. Set it to true to reproduce the original's truncation exactly.
    /// </summary>
    public bool StopAtLastFrame { get; init; }

    /// <summary>
    /// Buttons that are allowed to skip the movie. Playback is only interrupted when the pressed
    /// set is non-empty and entirely contained in this mask. Original <c>param_5</c>.
    /// </summary>
    public uint SkipButtonMask { get; init; } = uint.MaxValue;

    /// <summary>
    /// Skipping is only accepted after this frame number. Original <c>param_6</c>.
    /// </summary>
    public int SkipAfterFrame { get; init; }

    /// <summary>
    /// Number of frames over which the CD volume ramps down at the end of the movie.
    /// The original starts the ramp at <c>LastFrame - 15</c>.
    /// </summary>
    public int FadeOutFrames { get; init; } = 15;

    /// <summary>Volume applied while the movie plays. Original uses 0x7FFF.</summary>
    public int Volume { get; init; } = 0x7FFF;

    /// <summary>Amount subtracted from the volume on each frame of the fade-out ramp.</summary>
    public int VolumeFadeStep { get; init; } = 0x400;
}
