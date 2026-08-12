namespace PsxSdk.Audio;

/// <summary>
/// Decodes CD-XA ADPCM audio into 16-bit PCM.
///
/// SOURCE: public PlayStation / CD-ROM XA documentation (psx-spx, "CDROM XA Audio ADPCM
/// Compression"). There is nothing to transliterate here: on the console the CD controller feeds
/// XA sectors straight to the SPU's ADPCM decoder, so no game code ever touches these samples.
///
/// One Form 2 sector carries 2304 bytes of ADPCM as 18 sound groups of 128 bytes. Each group holds
/// 8 sound units of 28 samples (4-bit mode) or 4 sound units (8-bit mode); in stereo the units
/// alternate left/right, so a group yields 4 x 28 = 112 sample frames per channel and a whole
/// sector yields 2016 - which at 37800 Hz is exactly 53.33 ms.
/// </summary>
public sealed class XaAdpcmDecoder
{
    /// <summary>ADPCM bytes in a Form 2 sector: 18 sound groups of 128 bytes.</summary>
    public const int AdpcmBytesPerSector = 2304;

    /// <summary>Bytes in one sound group, the smallest independently decodable unit.</summary>
    public const int SoundGroupSize = 128;

    /// <summary>Sound groups a complete Form 2 sector carries.</summary>
    public const int SoundGroupCount = 18;

    private const int SamplesPerSoundUnit = 28;

    // Prediction filter coefficients, in 1/64 units.
    private static readonly int[] FilterK0 = [0, 60, 115, 98];
    private static readonly int[] FilterK1 = [0, 0, -52, -55];

    // Two samples of history per channel, carried across sectors.
    private readonly int[] _previous1 = new int[2];
    private readonly int[] _previous2 = new int[2];

    /// <summary>Sample rate of the most recently decoded sector.</summary>
    public int SampleRate { get; private set; } = 37800;

    /// <summary>Channel count of the most recently decoded sector.</summary>
    public int Channels { get; private set; } = 2;

    /// <summary>Resets the inter-sector prediction history. Call when seeking.</summary>
    public void Reset()
    {
        Array.Clear(_previous1);
        Array.Clear(_previous2);
    }

    /// <summary>Sample rate encoded in a subheader coding-info byte.</summary>
    public static int SampleRateOf(byte codingInfo) => (codingInfo & 0x0C) != 0 ? 18900 : 37800;

    /// <summary>Channel count encoded in a subheader coding-info byte.</summary>
    public static int ChannelsOf(byte codingInfo) => (codingInfo & 0x03) != 0 ? 2 : 1;

    /// <summary>Bits per sample encoded in a subheader coding-info byte.</summary>
    public static int BitsPerSampleOf(byte codingInfo) => (codingInfo & 0x30) != 0 ? 8 : 4;

    /// <summary>
    /// Maximum number of shorts <see cref="Decode"/> can write for one sector, whatever the
    /// coding: 18 groups x 8 units x 28 samples.
    /// </summary>
    public const int MaxSamplesPerSector = SoundGroupCount * 8 * SamplesPerSoundUnit;

    /// <summary>
    /// Decodes one sector's ADPCM payload into interleaved PCM.
    /// </summary>
    /// <param name="adpcm">
    /// The sector's user-data bytes. A complete Form 2 sector supplies
    /// <see cref="AdpcmBytesPerSector"/>; a shorter span is decoded as far as its whole sound
    /// groups reach, which is what a movie extracted at 2048 bytes per sector leaves behind — 16 of
    /// the 18 groups. Whatever is missing is the caller's to account for, since only the caller
    /// knows whether the gap should shorten the stream or be held open.
    /// </param>
    /// <param name="codingInfo">Coding-info byte from the sector subheader.</param>
    /// <param name="destination">Buffer of at least <see cref="MaxSamplesPerSector"/> shorts.</param>
    /// <returns>Number of shorts written (frames * channels).</returns>
    public int Decode(ReadOnlySpan<byte> adpcm, byte codingInfo, short[] destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        var groupCount = Math.Min(adpcm.Length / SoundGroupSize, SoundGroupCount);
        if (groupCount == 0)
        {
            throw new ArgumentException(
                $"An XA payload needs at least one {SoundGroupSize}-byte sound group; got {adpcm.Length} bytes.",
                nameof(adpcm));
        }

        if (destination.Length < MaxSamplesPerSector)
        {
            throw new ArgumentException($"Destination must hold at least {MaxSamplesPerSector} shorts.", nameof(destination));
        }

        SampleRate = SampleRateOf(codingInfo);
        Channels = ChannelsOf(codingInfo);
        var bitsPerSample = BitsPerSampleOf(codingInfo);
        var unitsPerGroup = bitsPerSample == 8 ? 4 : 8;

        // Frames land at a stride of `Channels`; each sound unit feeds one channel, and in stereo
        // consecutive units alternate left and right.
        var frameCursor = new int[2];
        var written = 0;

        for (var group = 0; group < groupCount; group++)
        {
            var groupOffset = group * SoundGroupSize;

            for (var unit = 0; unit < unitsPerGroup; unit++)
            {
                // Parameters for units 0..3 live at 04h..07h and for units 4..7 at 0Ch..0Fh; the
                // bytes at 00h..03h and 08h..0Bh are redundant copies.
                var parameter = bitsPerSample == 8
                    ? adpcm[groupOffset + 4 + unit]
                    : adpcm[groupOffset + (unit < 4 ? 4 + unit : 0x0C + unit - 4)];

                var shift = parameter & 0x0F;
                if (shift > 12)
                {
                    // Not a valid range on a well-formed disc; the SPU behaves as if fully shifted.
                    shift = 12;
                }

                var filter = (parameter >> 4) & 0x03;
                var k0 = FilterK0[filter];
                var k1 = FilterK1[filter];

                var channel = Channels == 2 ? unit & 1 : 0;

                for (var i = 0; i < SamplesPerSoundUnit; i++)
                {
                    int raw;
                    if (bitsPerSample == 8)
                    {
                        raw = (sbyte)adpcm[groupOffset + 16 + i * 4 + unit] << 8;
                    }
                    else
                    {
                        var b = adpcm[groupOffset + 16 + i * 4 + (unit >> 1)];
                        var nibble = (unit & 1) != 0 ? b >> 4 : b & 0x0F;
                        raw = (short)(nibble << 12);
                    }

                    // Arithmetic shift, not a division: `/ 64` truncates toward zero on negative
                    // sums and the error compounds through the prediction filter. Measured against
                    // jPSXdec on MATRIX: shift gives max deviation 16 / mean 1.69, divide gives
                    // max 78 / mean 17.3.
                    var sample = (raw >> shift) + ((k0 * _previous1[channel] + k1 * _previous2[channel] + 32) >> 6);
                    sample = sample < short.MinValue ? short.MinValue : sample > short.MaxValue ? short.MaxValue : sample;

                    _previous2[channel] = _previous1[channel];
                    _previous1[channel] = sample;

                    var index = frameCursor[channel] * Channels + channel;
                    destination[index] = (short)sample;
                    frameCursor[channel]++;
                    written++;
                }
            }
        }

        return written;
    }
}
