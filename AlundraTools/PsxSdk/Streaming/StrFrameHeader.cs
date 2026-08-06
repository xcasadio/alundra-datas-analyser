namespace PsxSdk.Streaming;

/// <summary>
/// The 32-byte header that prefixes every video sector of a PSX STR stream.
///
/// SOURCE: measured directly on Alundra's MOVIE/*.MOV files and cross-checked against the fields
/// the original player reads (GHIDRA: FUN_80027738 @ 0x80027738 reads <c>header[2]</c> as the
/// frame number and <c>header[4]</c> / <c>header+0x12</c> as width and height).
/// </summary>
public readonly struct StrFrameHeader
{
    /// <summary>Size of the header in bytes.</summary>
    public const int Size = 32;

    /// <summary>Expected value of <see cref="Magic"/> for a video sector.</summary>
    public const ushort VideoMagic = 0x0160;

    /// <summary>Expected value of <see cref="StreamType"/> for a video sector.</summary>
    public const ushort VideoType = 0x8001;

    /// <summary>Expected value of <see cref="Magic3800"/>.</summary>
    public const ushort FrameMagic3800 = 0x3800;

    /// <summary>Number of payload bytes in a video sector (2048 minus this header).</summary>
    public const int PayloadSize = 2048 - Size;

    public ushort Magic { get; }
    public ushort StreamType { get; }

    /// <summary>Zero-based index of this sector within its frame.</summary>
    public ushort ChunkNumber { get; }

    /// <summary>Total number of video sectors making up this frame.</summary>
    public ushort ChunkCount { get; }

    /// <summary>One-based frame number.</summary>
    public uint FrameNumber { get; }

    /// <summary>Number of demuxed bytes actually used by this frame.</summary>
    public uint DemuxSizeBytes { get; }

    public ushort Width { get; }
    public ushort Height { get; }

    /// <summary>Number of run-length codes; also the MDEC DMA word count, rounded up to 32.</summary>
    public ushort RunLengthCodeCount { get; }

    public ushort Magic3800 { get; }
    public ushort QuantScale { get; }

    /// <summary>Bitstream version: 2 for raw 10-bit DC, 3 for differential DC coding.</summary>
    public ushort Version { get; }

    private StrFrameHeader(ReadOnlySpan<byte> sector)
    {
        Magic = ReadU16(sector, 0x00);
        StreamType = ReadU16(sector, 0x02);
        ChunkNumber = ReadU16(sector, 0x04);
        ChunkCount = ReadU16(sector, 0x06);
        FrameNumber = ReadU32(sector, 0x08);
        DemuxSizeBytes = ReadU32(sector, 0x0C);
        Width = ReadU16(sector, 0x10);
        Height = ReadU16(sector, 0x12);
        RunLengthCodeCount = ReadU16(sector, 0x14);
        Magic3800 = ReadU16(sector, 0x16);
        QuantScale = ReadU16(sector, 0x18);
        Version = ReadU16(sector, 0x1A);
    }

    /// <summary>True when this looks like a well-formed video sector header.</summary>
    public bool IsVideoSector =>
        Magic == VideoMagic &&
        StreamType == VideoType &&
        Magic3800 == FrameMagic3800 &&
        ChunkCount > 0 &&
        ChunkNumber < ChunkCount &&
        Width is > 0 and <= 640 &&
        Height is > 0 and <= 480;

    /// <summary>Parses the header at the start of <paramref name="sector"/>.</summary>
    public static StrFrameHeader Parse(ReadOnlySpan<byte> sector)
    {
        if (sector.Length < Size)
        {
            throw new ArgumentException("Sector is shorter than an STR frame header.", nameof(sector));
        }

        return new StrFrameHeader(sector);
    }

    /// <summary>
    /// The original validates a frame by checking that the first eight bytes of the demuxed
    /// payload repeat header bytes 0x14..0x1B. This reproduces that test.
    /// </summary>
    /// <remarks>GHIDRA: FUN_80027738 @ 0x80027738, <c>*addr == header[5] &amp;&amp; addr[1] == header[6]</c>.</remarks>
    public bool MatchesPayloadPrefix(ReadOnlySpan<byte> payload) =>
        payload.Length >= 8 &&
        ReadU16(payload, 0) == RunLengthCodeCount &&
        ReadU16(payload, 2) == Magic3800 &&
        ReadU16(payload, 4) == QuantScale &&
        ReadU16(payload, 6) == Version;

    private static ushort ReadU16(ReadOnlySpan<byte> data, int offset) =>
        (ushort)(data[offset] | (data[offset + 1] << 8));

    private static uint ReadU32(ReadOnlySpan<byte> data, int offset) =>
        (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24));
}
