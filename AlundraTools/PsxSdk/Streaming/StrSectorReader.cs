using PsxSdk.Cd;

namespace PsxSdk.Streaming;

/// <summary>
/// Reads a PSX STR stream sector by sector and hands out complete demuxed video frames.
///
/// JUSTIFICATION: PSX hardware adaptation only.
/// RELATION: replaces the whole real-time CD streaming layer of the original —
/// <c>StSetRing</c> (GHIDRA @ 0x80030f54), <c>StSetStream</c> (@ 0x80034470),
/// <c>StGetNext</c> (@ 0x800345f4), <c>StFreeRing</c> (@ 0x800344f8),
/// <c>StCdInterrupt</c> (@ 0x800346d8) and <c>CdRead2</c> (@ 0x80033f14). Those drive CD-ROM
/// registers, DMA channels and an interrupt handler that fills a 32-sector ring buffer; none of
/// that has a desktop equivalent. Only the contract of <c>StGetNext</c> is preserved: return the
/// next complete, self-consistent frame, or report that none is available.
///
/// Audio sectors are skipped. XA-ADPCM decoding is out of scope for this class.
/// </summary>
public sealed class StrSectorReader : IDisposable
{
    private readonly Stream _stream;
    private readonly bool _ownsStream;
    private readonly byte[] _sector = new byte[CdSector.Form1UserDataSize];
    private byte[] _frameBuffer = new byte[CdSector.Form1UserDataSize * 16];

    private int _frameLength;
    private uint _pendingFrameNumber;
    private int _expectedChunk;
    private int _expectedChunkCount;
    private StrFrameHeader _pendingHeader;

    /// <summary>Opens a reader over a raw STR stream (2048-byte user-data sectors).</summary>
    public StrSectorReader(Stream stream, bool ownsStream = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _stream = stream;
        _ownsStream = ownsStream;
        SectorCount = (int)(stream.Length / CdSector.Form1UserDataSize);
    }

    /// <summary>Opens a reader over a file containing a raw STR stream.</summary>
    public static StrSectorReader OpenFile(string path) =>
        new(File.OpenRead(path), ownsStream: true);

    /// <summary>Total number of 2048-byte sectors in the stream.</summary>
    public int SectorCount { get; }

    /// <summary>Index of the next sector to be read.</summary>
    public int SectorPosition { get; private set; }

    /// <summary>Number of video sectors that failed validation and were dropped.</summary>
    public int DroppedSectors { get; private set; }

    /// <summary>True once every sector has been consumed.</summary>
    public bool EndOfStream => SectorPosition >= SectorCount;

    /// <summary>A complete demuxed frame.</summary>
    /// <param name="Header">Header of the frame's first sector.</param>
    /// <param name="Data">Buffer holding the demuxed payload; only valid until the next call.</param>
    /// <param name="Length">Number of valid bytes in <paramref name="Data"/>.</param>
    public readonly record struct Frame(StrFrameHeader Header, byte[] Data, int Length);

    /// <summary>Rewinds to the start of the stream.</summary>
    public void Rewind()
    {
        _stream.Seek(0, SeekOrigin.Begin);
        SectorPosition = 0;
        ResetPendingFrame();
    }

    /// <summary>Outcome of <see cref="ProbeTail"/>.</summary>
    /// <param name="LastFrameNumber">Frame number of the last video sector, or 0 if none.</param>
    /// <param name="LastVideoSector">Index of that sector, or -1 if none.</param>
    public readonly record struct TailInfo(uint LastFrameNumber, int LastVideoSector);

    /// <summary>
    /// Scans backwards from the end of the stream for the last video sector. Leaves the read
    /// position at the start of the stream.
    /// </summary>
    /// <remarks>
    /// The original never needs this: the caller hardcodes a frame limit. It is used here to drive
    /// the end-of-movie volume ramp when playing a stream to its natural end.
    ///
    /// The scan must not assume the stream ends on a video sector: Alundra's own files carry a
    /// long run of trailing padding (307 sectors in ARAN_OP.MOV, 120 in ARAN_END.MOV), so a small
    /// fixed window silently fails on exactly the two longest movies.
    /// </remarks>
    public TailInfo ProbeTail(int maxSectorsToScan = int.MaxValue)
    {
        var result = 0u;
        var lastVideoSector = -1;
        var sectors = new byte[StrFrameHeader.Size];
        var first = maxSectorsToScan >= SectorCount ? 0 : SectorCount - maxSectorsToScan;

        for (var index = SectorCount - 1; index >= first; index--)
        {
            _stream.Seek((long)index * CdSector.Form1UserDataSize, SeekOrigin.Begin);
            var read = 0;
            while (read < StrFrameHeader.Size)
            {
                var chunk = _stream.Read(sectors, read, StrFrameHeader.Size - read);
                if (chunk <= 0)
                {
                    break;
                }

                read += chunk;
            }

            if (read < StrFrameHeader.Size)
            {
                continue;
            }

            var header = StrFrameHeader.Parse(sectors);
            if (header.IsVideoSector)
            {
                result = header.FrameNumber;
                lastVideoSector = index;
                break;
            }
        }

        Rewind();
        return new TailInfo(result, lastVideoSector);
    }

    /// <summary>
    /// Reads forward until a complete frame has been assembled.
    /// </summary>
    /// <remarks>
    /// RELATION: this is the <c>StGetNext</c> + <c>FUN_80027738</c> pair collapsed into one call.
    /// The original polls <c>StGetNext</c> up to 0x800000 times waiting for the CD interrupt to
    /// fill the ring; reading from a file cannot block, so the retry budget disappears.
    /// </remarks>
    /// <returns>True if <paramref name="frame"/> was filled in.</returns>
    public bool TryGetNextFrame(out Frame frame)
    {
        while (SectorPosition < SectorCount)
        {
            if (!ReadSector())
            {
                break;
            }

            var header = StrFrameHeader.Parse(_sector);
            if (!header.IsVideoSector)
            {
                // XA-ADPCM audio sector (or padding): not our business.
                continue;
            }

            if (header.ChunkNumber == 0)
            {
                StartFrame(header);
            }
            else if (_expectedChunkCount == 0 ||
                     header.FrameNumber != _pendingFrameNumber ||
                     header.ChunkNumber != _expectedChunk)
            {
                // Lost sync (seek, damaged stream, or we joined mid-frame). Drop and resynchronise
                // on the next chunk 0.
                DroppedSectors++;
                ResetPendingFrame();
                continue;
            }

            AppendPayload();
            _expectedChunk++;

            if (_expectedChunk < _expectedChunkCount)
            {
                continue;
            }

            var completed = _pendingHeader;
            var length = _frameLength;
            ResetPendingFrame();

            // Same validity test as the original: the demuxed payload must start with a copy of
            // header bytes 0x14..0x1B.
            if (!completed.MatchesPayloadPrefix(_frameBuffer.AsSpan(0, Math.Min(8, length))))
            {
                DroppedSectors++;
                continue;
            }

            // Only the bytes the muxer declared as used carry data; the rest of the allotted
            // sectors is zero padding inserted to keep the stream at a constant bitrate.
            if (completed.DemuxSizeBytes > 0 && completed.DemuxSizeBytes < (uint)length)
            {
                length = (int)completed.DemuxSizeBytes;
            }

            frame = new Frame(completed, _frameBuffer, length);
            return true;
        }

        frame = default;
        return false;
    }

    private void StartFrame(StrFrameHeader header)
    {
        _pendingHeader = header;
        _pendingFrameNumber = header.FrameNumber;
        _expectedChunkCount = header.ChunkCount;
        _expectedChunk = 0;
        _frameLength = 0;

        var required = header.ChunkCount * StrFrameHeader.PayloadSize;
        if (_frameBuffer.Length < required)
        {
            _frameBuffer = new byte[required];
        }
    }

    private void AppendPayload()
    {
        Array.Copy(_sector, StrFrameHeader.Size, _frameBuffer, _frameLength, StrFrameHeader.PayloadSize);
        _frameLength += StrFrameHeader.PayloadSize;
    }

    private void ResetPendingFrame()
    {
        _expectedChunk = 0;
        _expectedChunkCount = 0;
        _frameLength = 0;
        _pendingFrameNumber = 0;
    }

    private bool ReadSector()
    {
        var read = 0;
        while (read < CdSector.Form1UserDataSize)
        {
            var chunk = _stream.Read(_sector, read, CdSector.Form1UserDataSize - read);
            if (chunk <= 0)
            {
                SectorPosition = SectorCount;
                return false;
            }

            read += chunk;
        }

        SectorPosition++;
        return true;
    }

    public void Dispose()
    {
        if (_ownsStream)
        {
            _stream.Dispose();
        }
    }
}
