using PsxSdk.Audio;
using PsxSdk.Cd;
using PsxSdk.Mdec;
using PsxSdk.Streaming;

namespace PsxSdk.Video;

/// <summary>
/// Plays a PSX STR movie: pulls demuxed frames from a <see cref="StrSectorReader"/>, runs them
/// through the VLC and MDEC stages, and exposes the result as a 24-bit RGB frame buffer.
///
/// GHIDRA: PlayMovie @ 0x80027ff4 and its helpers FUN_80027c10 (open), FUN_80027888 (VLC),
/// FUN_800279b4 (wait), FUN_8002790c (MDEC kick-off), FUN_80027a4c (MDEC completion callback),
/// FUN_80027d6c (present) and FUN_80027f10 (CD volume), all in LOADER.EXE.
///
/// JUSTIFICATION: PSX hardware adaptation only.
/// RELATION: the original is a blocking <c>do/while</c> paced by <c>VSync(0)</c>, with the VLC
/// stage of frame N+1 overlapping the MDEC DMA of frame N and the frame emitted in 16-pixel-wide
/// strips so the two units stay busy. None of that overlap is observable in the output, and there
/// is no DMA to hide latency behind here, so this port decodes a whole frame per
/// <see cref="Tick"/> call and lets the host drive the loop. Everything that *is* observable —
/// the frame sequence, the skip rule, the end-of-movie volume ramp and the watchdog — is kept.
///
/// The class holds no reference to any renderer or game type: callers read
/// <see cref="FrameRgb24"/> and display it however they like.
/// </summary>
public sealed class StrMoviePlayer : IDisposable
{
    /// <summary>
    /// Sector delivery rate at CD-ROM single speed: 75 sectors per second (150 KB/s of 2048-byte
    /// user data).
    /// </summary>
    public const double SingleSpeedSectorsPerSecond = 75.0;

    /// <summary>
    /// Sector delivery rate at double speed, which is what the original selects: it opens the
    /// stream with <c>CdRead2(0x1c0)</c>, whose <c>CdlModeSpeed</c> bit (0x80) is set.
    /// </summary>
    /// <remarks>
    /// Cross-checked against the interleaved XA audio, which pins the rate down without relying on
    /// any assumption about the drive: the audio is 37800 Hz stereo 4-bit, so one Form 2 sector
    /// carries 2016 sample frames per channel = 53.33 ms, i.e. 18.75 audio sectors per second. The
    /// muxer places one audio sector every 8, so the stream must be delivered at 150 sectors per
    /// second. With ~10 sectors per frame that gives 15 fps, and dividing each movie's frame count
    /// by its total XA audio duration returns 15.00 / 14.86 / 14.96 / 14.99 fps for
    /// EURO_OP / ARAN_OP / ARAN_END / MATRIX.
    /// </remarks>
    public const double DoubleSpeedSectorsPerSecond = 2 * SingleSpeedSectorsPerSecond;

    private readonly StrSectorReader _reader;
    private readonly MoviePlaybackOptions _options;
    private readonly MdecVlcDecoder _vlcDecoder = new();
    private readonly MdecImageDecoder _imageDecoder = new();
    private readonly XaAdpcmDecoder _audioDecoder = new();
    private readonly short[] _audioScratch = new short[XaAdpcmDecoder.MaxSamplesPerSector];

    // Ring buffer holding decoded PCM until the host drains it. Four seconds of 37800 Hz stereo is
    // far more than the pipeline can get ahead by, since audio and video come from the same
    // sectors and are therefore produced at exactly the right ratio.
    private readonly short[] _audioRing = new short[37800 * 2 * 4];
    private int _audioRead;
    private int _audioWrite;
    private int _audioCount;

    private ushort[]? _codes;
    private byte[] _frameRgb24 = [];
    private double _frameClock;

    /// <summary>Opens a movie from a file path.</summary>
    public StrMoviePlayer(string path, MoviePlaybackOptions options)
        : this(StrSectorReader.OpenFile(path), options)
    {
    }

    /// <summary>Opens a movie from an already-constructed sector reader, which it takes ownership of.</summary>
    public StrMoviePlayer(StrSectorReader reader, MoviePlaybackOptions options)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(options);

        _reader = reader;
        _options = options;
        _reader.OnAudioSector = OnAudioSector;

        var tail = reader.ProbeTail();
        TotalFrames = (int)tail.LastFrameNumber;
        Volume = options.Volume;

        // Frames are delivered at a constant sector rate, so the nominal frame rate follows from
        // how many sectors the muxer allotted per frame (video plus interleaved audio). Trailing
        // padding after the last video sector carries no frame and must stay out of the average.
        var streamedSectors = tail.LastVideoSector >= 0 ? tail.LastVideoSector + 1 : reader.SectorCount;
        FramesPerSecond = TotalFrames > 0 && streamedSectors > 0
            ? DoubleSpeedSectorsPerSecond / ((double)streamedSectors / TotalFrames)
            : 30.0;
    }

    /// <summary>Movie width in pixels; known once the first frame has been decoded.</summary>
    public int Width { get; private set; }

    /// <summary>Movie height in pixels; known once the first frame has been decoded.</summary>
    public int Height { get; private set; }

    /// <summary>Total number of frames in the stream, probed at construction time.</summary>
    public int TotalFrames { get; }

    /// <summary>Nominal playback rate derived from the stream's sector budget per frame.</summary>
    public double FramesPerSecond { get; }

    /// <summary>Frame number of the most recently decoded frame.</summary>
    public int CurrentFrame { get; private set; }

    /// <summary>
    /// Decoded frame, <see cref="Width"/> * <see cref="Height"/> * 3 bytes in raster RGB order.
    /// Empty until the first successful <see cref="Tick"/>.
    /// </summary>
    public byte[] FrameRgb24 => _frameRgb24;

    /// <summary>True once playback has ended, for any reason.</summary>
    public bool IsFinished { get; private set; }

    /// <summary>True when the viewer interrupted playback with the skip buttons.</summary>
    public bool WasSkipped { get; private set; }

    /// <summary>
    /// Current CD volume. Mirrors <c>g_volume</c>; the host is free to apply it to its own audio
    /// output. Reaching zero during the end-of-movie ramp stops playback.
    /// </summary>
    /// <remarks>GHIDRA: g_volume, written by FUN_80027f10 @ 0x80027f10.</remarks>
    public int Volume { get; private set; }

    /// <summary>
    /// Horizontal on-screen position of the movie, in pixels. For Alundra's two movies this is
    /// exactly the horizontal centring offset: 0 for the 320-wide EURO_OP, 8 for the 304-wide
    /// ARAN_OP.
    /// </summary>
    public int ScreenX => _options.ScreenX;

    /// <summary>Vertical on-screen position of the movie, in pixels.</summary>
    /// <remarks>
    /// GHIDRA: FUN_80027d6c @ 0x80027d6c builds the display environment with
    /// <c>screen.y = param_2 + 0x18</c>. The 0x18 is the PAL vertical blanking start that the PSX
    /// display environment counts from, not part of the movie's position on screen, so it is not
    /// included here.
    /// </remarks>
    public int ScreenY => _options.ScreenY;

    /// <summary>
    /// Raw value the original writes into <c>DISPENV.screen.y</c>, kept for reference and for
    /// hosts that emulate PSX display timing.
    /// </summary>
    public int PsxDisplayScreenY => _options.ScreenY + 0x18;

    /// <summary>
    /// Advances playback by <paramref name="deltaSeconds"/> of wall-clock time, decoding as many
    /// frames as the nominal frame rate calls for.
    /// </summary>
    /// <param name="padButtons">Currently pressed pad buttons, in PSX pad bit order.</param>
    /// <returns>True if a new frame was decoded into <see cref="FrameRgb24"/>.</returns>
    public bool Tick(double deltaSeconds, uint padButtons)
    {
        if (IsFinished)
        {
            return false;
        }

        _frameClock += deltaSeconds * FramesPerSecond;
        if (_frameClock < 1.0)
        {
            return false;
        }

        // Never decode more than a handful of frames in one call: a long stall in the host should
        // slow the movie down, not freeze it while the decoder catches up.
        var budget = (int)Math.Min(_frameClock, 4);
        _frameClock -= budget;

        var produced = false;
        for (var i = 0; i < budget && !IsFinished; i++)
        {
            produced |= DecodeNextFrame(padButtons);
        }

        return produced;
    }

    /// <summary>
    /// Decodes exactly one frame, ignoring playback pacing. Useful for offline extraction.
    /// </summary>
    public bool DecodeNextFrame(uint padButtons)
    {
        if (IsFinished)
        {
            return false;
        }

        // GHIDRA: FUN_80027888 -> FUN_80027738 -> StGetNext. The original retries up to 0x800000
        // times waiting for the CD ring to fill; reading a file either succeeds or is at EOF.
        if (!_reader.TryGetNextFrame(out var frame))
        {
            IsFinished = true;
            return false;
        }

        var header = frame.Header;

        // GHIDRA: FUN_80027738 clears VRAM and re-latches the rects whenever the dimensions change
        // mid-stream. Here that just means resizing the destination buffer.
        if (header.Width != Width || header.Height != Height)
        {
            Width = header.Width;
            Height = header.Height;
            _frameRgb24 = new byte[Width * Height * 3];
        }

        // GHIDRA: the watchdog in PlayMovie's loop condition, `prevFrame <= curFrame`: a frame
        // number that goes backwards means the stream is damaged or looped, and playback stops.
        if (CurrentFrame > 0 && header.FrameNumber < (uint)CurrentFrame)
        {
            IsFinished = true;
            return false;
        }

        CurrentFrame = (int)header.FrameNumber;

        var blockCount = Width / 16 * (Height / 16) * MdecImageDecoder.BlocksPerMacroblock;
        var vlc = _vlcDecoder.Decode(frame.Data, frame.Length, blockCount, ref _codes);
        _imageDecoder.DecodeFrame(vlc.Codes, 2, vlc.Length, Width, Height, _frameRgb24);

        UpdateSkipState(padButtons);
        UpdateVolumeRamp();
        UpdateStopConditions();

        return true;
    }

    /// <summary>
    /// GHIDRA: PlayMovie @ 0x80027ff4 — skipping requires that the pressed set is non-empty, is
    /// entirely contained in the allowed mask, and that we are past the protected opening frames.
    /// </summary>
    private void UpdateSkipState(uint padButtons)
    {
        if (WasSkipped)
        {
            return;
        }

        if (CurrentFrame > _options.SkipAfterFrame &&
            (padButtons | _options.SkipButtonMask) == _options.SkipButtonMask &&
            (padButtons & _options.SkipButtonMask) != 0)
        {
            WasSkipped = true;
        }
    }

    /// <summary>
    /// GHIDRA: PlayMovie @ 0x80027ff4 — once within <c>FadeOutFrames</c> of the end, or as soon as
    /// a skip is requested, the CD volume ramps down by one step per frame and playback ends when
    /// it reaches zero.
    /// </summary>
    private void UpdateVolumeRamp()
    {
        var fadeStart = FadeOutStartFrame();
        var fading = WasSkipped || (fadeStart > 0 && CurrentFrame > fadeStart);
        if (!fading)
        {
            return;
        }

        Volume -= _options.VolumeFadeStep;
        if (Volume < 0)
        {
            Volume = 0;
        }
    }

    private void UpdateStopConditions()
    {
        if (WasSkipped && Volume == 0)
        {
            IsFinished = true;
            return;
        }

        // The deliberate deviation: unless StopAtLastFrame is set, the hard frame limit the
        // original passes in is ignored and the stream plays to its natural end.
        if (_options.StopAtLastFrame && _options.LastFrame > 0 && CurrentFrame >= _options.LastFrame)
        {
            IsFinished = true;
            return;
        }

        if (TotalFrames > 0 && CurrentFrame >= TotalFrames)
        {
            IsFinished = true;
        }
    }

    private int FadeOutStartFrame()
    {
        var lastFrame = _options.StopAtLastFrame && _options.LastFrame > 0 ? _options.LastFrame : TotalFrames;
        return lastFrame > 0 ? lastFrame - _options.FadeOutFrames : 0;
    }

    /// <summary>
    /// True when the source carries usable XA audio, i.e. when it is a raw 2352-byte-per-sector
    /// stream. A source written as flat 2048-byte user data has lost 2 of every 18 ADPCM sound
    /// groups and offers no audio at all rather than a broken one.
    /// </summary>
    public bool HasAudio => _reader.HasAudio;

    /// <summary>Sample rate of the decoded audio, valid once the first audio sector was read.</summary>
    public int AudioSampleRate => _audioDecoder.SampleRate;

    /// <summary>Channel count of the decoded audio.</summary>
    public int AudioChannels => _audioDecoder.Channels;

    /// <summary>Number of decoded PCM shorts waiting to be drained.</summary>
    public int AvailableAudioSamples => _audioCount;

    /// <summary>
    /// Number of PCM shorts dropped because the host was not draining fast enough. Should stay 0;
    /// anything else means the audio sink is not keeping up.
    /// </summary>
    public int DroppedAudioSamples { get; private set; }

    /// <summary>
    /// Drains decoded PCM into <paramref name="destination"/>, interleaved at
    /// <see cref="AudioChannels"/> channels.
    /// </summary>
    /// <returns>Number of shorts written, which may be fewer than requested.</returns>
    public int ReadAudio(short[] destination, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(destination);
        var taken = Math.Min(count, _audioCount);
        for (var i = 0; i < taken; i++)
        {
            destination[offset + i] = _audioRing[_audioRead];
            _audioRead = (_audioRead + 1) % _audioRing.Length;
        }

        _audioCount -= taken;
        return taken;
    }

    private void OnAudioSector(ReadOnlySpan<byte> userData, byte codingInfo)
    {
        var written = _audioDecoder.Decode(userData, codingInfo, _audioScratch);
        for (var i = 0; i < written; i++)
        {
            if (_audioCount >= _audioRing.Length)
            {
                DroppedAudioSamples += written - i;
                return;
            }

            _audioRing[_audioWrite] = _audioScratch[i];
            _audioWrite = (_audioWrite + 1) % _audioRing.Length;
            _audioCount++;
        }
    }

    /// <summary>Number of sectors in the underlying stream.</summary>
    public int SectorCount => _reader.SectorCount;

    /// <summary>Sector size this player expects, exposed for host-side diagnostics.</summary>
    public static int SectorSize => CdSector.Form1UserDataSize;

    public void Dispose() => _reader.Dispose();
}
