using PsxSdk.Audio;
using PsxSdk.Cd;

namespace PsxSdk.Streaming;

/// <summary>
/// Reads a PSX STR stream sector by sector and hands out complete demuxed video frames, plus the
/// interleaved XA audio sectors when the source carries them.
///
/// Two source layouts are accepted:
///   - <b>raw</b>, 2352 bytes per sector (Alundra's ".STR" re-extractions): the sector subheader is
///     present, so Form 1 (video) and Form 2 (audio) sectors are told apart by their submode and
///     the full 2324-byte audio payload is available. This is the only layout that yields complete
///     audio.
///   - <b>user data only</b>, 2048 bytes per sector (Alundra's ".MOV" files): what a naive
///     extractor produces. Video is intact and byte-identical to the raw source's. Audio sectors
///     are still present but truncated from 2324 to 2048 bytes, which keeps 16 of their 18 ADPCM
///     sound groups; there is no subheader to identify them, so they are recognised by structure
///     instead — see <see cref="LooksLikeXaSector"/>.
///
/// JUSTIFICATION: PSX hardware adaptation only.
/// RELATION: replaces the whole real-time CD streaming layer of the original —
/// <c>StSetRing</c> (GHIDRA @ 0x80030f54), <c>StSetStream</c> (@ 0x80034470),
/// <c>StGetNext</c> (@ 0x800345f4), <c>StFreeRing</c> (@ 0x800344f8),
/// <c>StCdInterrupt</c> (@ 0x800346d8) and <c>CdRead2</c> (@ 0x80033f14). Those drive CD-ROM
/// registers, DMA channels and an interrupt handler that fills a 32-sector ring buffer; none of
/// that has a desktop equivalent. Only the contract of <c>StGetNext</c> is preserved: return the
/// next complete, self-consistent frame, or report that none is available.
/// </summary>
public sealed class StrSectorReader : IDisposable
{
    /// <summary>Offset of the subheader inside a raw sector.</summary>
    private const int SubHeaderOffset = 16;

    /// <summary>Offset of the user data inside a raw sector.</summary>
    private const int RawUserDataOffset = 24;

    /// <summary>Submode bit marking a Form 2 sector.</summary>
    private const byte SubModeForm2 = 0x20;

    /// <summary>Submode bit marking an audio sector.</summary>
    private const byte SubModeAudio = 0x04;

    /// <summary>
    /// Coding info assumed for a source that has no subheader to state it: 4-bit stereo, 37800 Hz.
    /// </summary>
    /// <remarks>
    /// SOURCE: every audio sector of every ".STR" on the France disc carries exactly this value —
    /// 16824 of them across EURO_OP, ARAN_OP, MATRIX and ARAN_END, with no other value present.
    /// ARAN_OP and ARAN_END are the same files on the USA disc, so the only movie this is not
    /// directly evidenced on is USA_OP, which the same muxer produced.
    /// </remarks>
    private const byte AssumedCodingInfo = 0x01;

    private readonly Stream _stream;
    private readonly bool _ownsStream;
    private readonly int _sectorSize;
    private readonly int _userDataOffset;
    private readonly byte[] _sector;
    private byte[] _frameBuffer;

    private int _frameLength;
    private uint _pendingFrameNumber;
    private int _expectedChunk;
    private int _expectedChunkCount;
    private StrFrameHeader _pendingHeader;

    /// <summary>Opens a reader over an STR stream, detecting its sector layout from the length.</summary>
    public StrSectorReader(Stream stream, bool ownsStream = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        _stream = stream;
        _ownsStream = ownsStream;

        if (stream.Length % CdSector.RawSize == 0)
        {
            _sectorSize = CdSector.RawSize;
            _userDataOffset = RawUserDataOffset;
        }
        else
        {
            _sectorSize = CdSector.Form1UserDataSize;
            _userDataOffset = 0;
        }

        _sector = new byte[_sectorSize];
        _frameBuffer = new byte[StrFrameHeader.PayloadSize * 16];
        SectorCount = (int)(stream.Length / _sectorSize);
    }

    /// <summary>Opens a reader over a file containing an STR stream.</summary>
    public static StrSectorReader OpenFile(string path) =>
        new(File.OpenRead(path), ownsStream: true);

    /// <summary>Total number of sectors in the stream.</summary>
    public int SectorCount { get; }

    /// <summary>Index of the next sector to be read.</summary>
    public int SectorPosition { get; private set; }

    /// <summary>Number of sectors that failed validation and were dropped.</summary>
    public int DroppedSectors { get; private set; }

    /// <summary>True when the source is raw, i.e. carries subheaders and complete audio payloads.</summary>
    public bool IsRawSource => _sectorSize == CdSector.RawSize;

    /// <summary>True when audio sectors can be dispatched at all, whatever the layout.</summary>
    public bool HasAudio => true;

    /// <summary>
    /// ADPCM bytes each audio sector of this source yields: the full 2304 on a raw source, 2048 on
    /// a user-data one, which is 16 of the 18 sound groups.
    /// </summary>
    public int AdpcmBytesPerAudioSector => IsRawSource
        ? XaAdpcmDecoder.AdpcmBytesPerSector
        : CdSector.Form1UserDataSize / XaAdpcmDecoder.SoundGroupSize * XaAdpcmDecoder.SoundGroupSize;

    /// <summary>True once every sector has been consumed.</summary>
    public bool EndOfStream => SectorPosition >= SectorCount;

    /// <summary>
    /// Called for every XA audio sector encountered while reading. The span holds the sector's
    /// user data and stays valid only for the duration of the call.
    /// </summary>
    public delegate void AudioSectorHandler(ReadOnlySpan<byte> userData, byte codingInfo);

    /// <summary>
    /// Receives audio sectors as they are read. The span holds
    /// <see cref="AdpcmBytesPerAudioSector"/> bytes, which is short of a whole sector on a
    /// user-data source.
    /// </summary>
    public AudioSectorHandler? OnAudioSector { get; set; }

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
    /// The scan must not assume the stream ends on a video sector: Alundra's own files carry a
    /// long run of trailing padding (307 sectors in ARAN_OP, 120 in ARAN_END), so a small fixed
    /// window silently fails on exactly the two longest movies.
    /// </remarks>
    public TailInfo ProbeTail(int maxSectorsToScan = int.MaxValue)
    {
        var result = 0u;
        var lastVideoSector = -1;
        var first = maxSectorsToScan >= SectorCount ? 0 : SectorCount - maxSectorsToScan;

        for (var index = SectorCount - 1; index >= first; index--)
        {
            if (!ReadSectorAt(index))
            {
                continue;
            }

            if (!IsVideoSector(out var header))
            {
                continue;
            }

            result = header.FrameNumber;
            lastVideoSector = index;
            break;
        }

        Rewind();
        return new TailInfo(result, lastVideoSector);
    }

    /// <summary>
    /// Reads forward until a complete frame has been assembled, dispatching any audio sectors
    /// passed on the way to <see cref="OnAudioSector"/>.
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
            if (!ReadNextSector())
            {
                break;
            }

            if (!IsVideoSector(out var header))
            {
                DispatchAudioSector();
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

    /// <summary>
    /// True when the sector currently in the buffer is a well-formed STR video sector.
    /// On a raw source the subheader settles it; otherwise the STR header itself is the only clue.
    /// </summary>
    private bool IsVideoSector(out StrFrameHeader header)
    {
        if (IsRawSource && (_sector[SubHeaderOffset + 2] & SubModeForm2) != 0)
        {
            header = default;
            return false;
        }

        header = StrFrameHeader.Parse(_sector.AsSpan(_userDataOffset));
        return header.IsVideoSector;
    }

    private void DispatchAudioSector()
    {
        if (OnAudioSector is null)
        {
            return;
        }

        if (IsRawSource)
        {
            var subMode = _sector[SubHeaderOffset + 2];
            if ((subMode & SubModeAudio) == 0 || (subMode & SubModeForm2) == 0)
            {
                return;
            }

            OnAudioSector(_sector.AsSpan(_userDataOffset, CdSector.Form2UserDataSize), _sector[SubHeaderOffset + 3]);
            return;
        }

        if (LooksLikeXaSector())
        {
            OnAudioSector(_sector.AsSpan(0, AdpcmBytesPerAudioSector), AssumedCodingInfo);
        }
    }

    /// <summary>
    /// Decides whether a non-video sector of a user-data source is XA audio, by structure alone.
    /// </summary>
    /// <remarks>
    /// A user-data source has no subheader, so the submode that would settle it is gone. What
    /// remains is an invariant of the ADPCM format itself: in every 128-byte sound group the four
    /// parameter bytes at 00h..03h are a copy of those at 04h..07h, and 08h..0Bh a copy of
    /// 0Ch..0Fh. Sixteen groups have to agree, which is 128 bytes of coincidence for a sector that
    /// is not audio.
    ///
    /// SOURCE: checked against every audio sector of the four France ".STR" files — 16824 of them,
    /// all satisfying it over the 16 groups that survive truncation.
    ///
    /// Trailing padding, which is all zeroes, passes too and decodes to silence. That is harmless:
    /// padding only ever follows the last video sector, which playback stops at.
    /// </remarks>
    private bool LooksLikeXaSector()
    {
        for (var offset = 0; offset + XaAdpcmDecoder.SoundGroupSize <= AdpcmBytesPerAudioSector;
             offset += XaAdpcmDecoder.SoundGroupSize)
        {
            for (var i = 0; i < 4; i++)
            {
                if (_sector[offset + i] != _sector[offset + 4 + i] ||
                    _sector[offset + 8 + i] != _sector[offset + 12 + i])
                {
                    return false;
                }
            }
        }

        return true;
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
        Array.Copy(_sector, _userDataOffset + StrFrameHeader.Size, _frameBuffer, _frameLength, StrFrameHeader.PayloadSize);
        _frameLength += StrFrameHeader.PayloadSize;
    }

    private void ResetPendingFrame()
    {
        _expectedChunk = 0;
        _expectedChunkCount = 0;
        _frameLength = 0;
        _pendingFrameNumber = 0;
    }

    private bool ReadNextSector()
    {
        var read = 0;
        while (read < _sectorSize)
        {
            var chunk = _stream.Read(_sector, read, _sectorSize - read);
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

    private bool ReadSectorAt(int index)
    {
        _stream.Seek((long)index * _sectorSize, SeekOrigin.Begin);
        var read = 0;
        while (read < _sectorSize)
        {
            var chunk = _stream.Read(_sector, read, _sectorSize - read);
            if (chunk <= 0)
            {
                return false;
            }

            read += chunk;
        }

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
