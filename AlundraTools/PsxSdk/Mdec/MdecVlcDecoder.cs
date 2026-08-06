namespace PsxSdk.Mdec;

/// <summary>
/// Decodes a demuxed STR frame bitstream into the stream of 16-bit MDEC codes that the MDEC would
/// have been fed by DMA.
///
/// GHIDRA: DecDCTvlc @ 0x8002ba84 (LOADER.EXE)
/// SOURCE: Ghidra (ReVa get-decompilation) for the control flow, DC prediction and output word
/// layout; public MDEC/MPEG-1 documentation for the code book itself (see <see cref="MdecTables"/>).
///
/// JUSTIFICATION: C# language bridge only.
/// RELATION: the original is a single 832-byte routine driven by four pre-flattened lookup tables
/// held as data in the executable, written as resumable goto-based code because it must stop and
/// restart when its output buffer fills up (it returns 1 and is re-entered with <c>bs == NULL</c>).
/// This port keeps the original's semantics exactly — same header fields, same DC prediction per
/// block type, same output word format, same end-of-frame marker — but decodes a whole frame in
/// one call, because the desktop port has no fixed-size DMA staging buffer to run out of, and
/// rebuilds the code book from its documented canonical form so that this assembly stays
/// independent of any particular game's executable.
/// </summary>
public sealed class MdecVlcDecoder
{
    private const int AcLookupBits = MdecTables.MaxAcCodeLength;
    private const int DcLookupBits = MdecTables.MaxDcCodeLength;

    private const byte KindInvalid = 0;
    private const byte KindNormal = 1;
    private const byte KindEndOfBlock = 2;
    private const byte KindEscape = 3;

    /// <summary>Marks the end of a block in the MDEC code stream (run 63, level 0).</summary>
    public const ushort EndOfBlockCode = 0xFE00;

    private readonly struct AcLookupEntry(byte kind, byte length, byte run, short level)
    {
        public readonly byte Kind = kind;
        public readonly byte Length = length;
        public readonly byte Run = run;
        public readonly short Level = level;
    }

    private readonly struct DcLookupEntry(byte length, byte size)
    {
        public readonly byte Length = length;
        public readonly byte Size = size;
    }

    private static readonly AcLookupEntry[] AcLookup = BuildAcLookup();
    private static readonly DcLookupEntry[] LumaDcLookup = BuildDcLookup(MdecTables.LumaDcCodeBook);
    private static readonly DcLookupEntry[] ChromaDcLookup = BuildDcLookup(MdecTables.ChromaDcCodeBook);

    /// <summary>Result of decoding one frame.</summary>
    /// <param name="Codes">MDEC code stream: two command words followed by the block data.</param>
    /// <param name="Length">Number of valid entries in <paramref name="Codes"/>.</param>
    /// <param name="BlocksDecoded">Number of 8x8 blocks actually decoded.</param>
    /// <param name="HitEndMarker">True if the bitstream's end-of-frame marker was reached.</param>
    /// <param name="Truncated">True if the bitstream ran out before all blocks were decoded.</param>
    public readonly record struct Result(
        ushort[] Codes,
        int Length,
        int BlocksDecoded,
        bool HitEndMarker,
        bool Truncated);

    /// <summary>
    /// Decodes <paramref name="blockCount"/> blocks out of a demuxed frame payload.
    /// </summary>
    /// <param name="frameData">Demuxed frame bytes; must start with the 8-byte MDEC frame header.</param>
    /// <param name="frameLength">Number of valid bytes in <paramref name="frameData"/>.</param>
    /// <param name="blockCount">Expected block count, normally <c>macroblocks * 6</c>.</param>
    /// <param name="output">Reusable output buffer; grown if too small.</param>
    public Result Decode(byte[] frameData, int frameLength, int blockCount, ref ushort[]? output)
    {
        ArgumentNullException.ThrowIfNull(frameData);
        if (frameLength < 8)
        {
            throw new ArgumentOutOfRangeException(nameof(frameLength), "Frame payload is shorter than the MDEC frame header.");
        }

        // Frame header, identical to bytes 0x14..0x1B of the STR sector header (the original
        // validates one against the other before calling into the VLC decoder).
        var runLengthCodeCount = (ushort)(frameData[0] | (frameData[1] << 8));
        var quantScale = (ushort)(frameData[4] | (frameData[5] << 8));
        var version = (ushort)(frameData[6] | (frameData[7] << 8));

        // GHIDRA: uVar18 = (version - 3 >= 0). Version 3 codes the DC differentially per block
        // type and cycles the block index 1..6; version 2 stores a raw 10-bit DC and never
        // distinguishes block types.
        var isVersion3 = version >= 3;

        var requiredCapacity = blockCount * 65 + 2;
        if (output is null || output.Length < requiredCapacity)
        {
            output = new ushort[requiredCapacity];
        }

        var reader = new MdecBitReader(frameData, 8, frameLength - 8);

        // GHIDRA: the original copies the first four header bytes to the head of the output
        // buffer; DecDCTin then turns that into the MDEC "decode macroblock" command word.
        output[0] = runLengthCodeCount;
        output[1] = 0x3800;
        var write = 2;

        // GHIDRA: iVar20 / iVar21 / _LoadTPage - DC predictors for block 1 (Cr), block 2 (Cb) and
        // the four luma blocks. Reset once per frame, not per macroblock.
        var dcCr = 0;
        var dcCb = 0;
        var dcLuma = 0;

        // GHIDRA: uVar18 - 1-based block index inside the macroblock for version 3.
        var blockIndex = isVersion3 ? 1 : 0;

        var blocksDecoded = 0;
        var hitEndMarker = false;

        for (; blocksDecoded < blockCount; blocksDecoded++)
        {
            if (reader.Exhausted)
            {
                break;
            }

            // ---- DC coefficient -------------------------------------------------------------
            int dcWord;
            if (!isVersion3)
            {
                if (reader.Peek(10) == 0x1FF)
                {
                    hitEndMarker = true;
                    break;
                }

                dcWord = reader.Read(10);
            }
            else
            {
                if (reader.Peek(10) == 0x3FF)
                {
                    hitEndMarker = true;
                    break;
                }

                // GHIDRA: blockIndex >= 3 selects the luma DC table (0x80132a88), otherwise the
                // chroma one (0x80132e88).
                var dcTable = blockIndex >= 3 ? LumaDcLookup : ChromaDcLookup;
                var dcEntry = dcTable[reader.Peek(DcLookupBits)];
                if (dcEntry.Length == 0)
                {
                    break;
                }

                reader.Skip(dcEntry.Length);
                var delta = reader.ReadDifferential(dcEntry.Size);

                int predicted;
                switch (blockIndex)
                {
                    case 1:
                        dcCr += delta;
                        predicted = dcCr;
                        break;
                    case 2:
                        dcCb += delta;
                        predicted = dcCb;
                        break;
                    default:
                        dcLuma += delta;
                        predicted = dcLuma;
                        break;
                }

                // GHIDRA: (predicted & 0xff) << 2 - the DC keeps 8-bit precision and is stored
                // multiplied by four in the MDEC's 10-bit signed DC field.
                dcWord = (predicted & 0xFF) << 2;

                blockIndex = blockIndex == 6 ? 1 : blockIndex + 1;
            }

            output[write++] = (ushort)(((quantScale & 0x3F) << 10) | (dcWord & 0x3FF));

            // ---- AC coefficients ------------------------------------------------------------
            // A block holds at most 63 AC coefficients plus its end-of-block marker, which is what
            // sizes `requiredCapacity` above. A corrupt bitstream could otherwise emit codes
            // indefinitely without ever producing an end-of-block and run off the buffer.
            var acWritten = 0;
            while (acWritten < 63)
            {
                if (reader.Exhausted)
                {
                    break;
                }

                var entry = AcLookup[reader.Peek(AcLookupBits)];
                switch (entry.Kind)
                {
                    case KindEndOfBlock:
                        reader.Skip(entry.Length);
                        output[write++] = EndOfBlockCode;
                        break;

                    case KindEscape:
                    {
                        reader.Skip(entry.Length);
                        var run = reader.Read(6);
                        var level = SignExtend10(reader.Read(10));
                        output[write++] = (ushort)(((run & 0x3F) << 10) | (level & 0x3FF));
                        acWritten++;
                        continue;
                    }

                    case KindNormal:
                    {
                        reader.Skip(entry.Length);
                        var negative = reader.Read(1) != 0;
                        var level = negative ? -entry.Level : entry.Level;
                        output[write++] = (ushort)(((entry.Run & 0x3F) << 10) | (level & 0x3FF));
                        acWritten++;
                        continue;
                    }

                    default:
                        // Invalid code: the bitstream is corrupt or we lost sync. Close the block
                        // so the image decoder still produces a well-formed (if wrong) frame.
                        output[write++] = EndOfBlockCode;
                        break;
                }

                acWritten = -1; // signals that the block was closed by an end-of-block word
                break;
            }

            // Ran out of coefficient room, or out of bitstream, without an end-of-block: terminate
            // the block explicitly so the image decoder does not read into the next one.
            if (acWritten >= 0)
            {
                output[write++] = EndOfBlockCode;
            }
        }

        return new Result(output, write, blocksDecoded, hitEndMarker, reader.Exhausted && blocksDecoded < blockCount);
    }

    private static int SignExtend10(int value) => (value & 0x200) != 0 ? value - 0x400 : value;

    private static AcLookupEntry[] BuildAcLookup()
    {
        var table = new AcLookupEntry[1 << AcLookupBits];

        Fill(table, MdecTables.EndOfBlockCode, MdecTables.EndOfBlockLength,
            new AcLookupEntry(KindEndOfBlock, MdecTables.EndOfBlockLength, 0, 0));

        Fill(table, MdecTables.EscapeCode, MdecTables.EscapeLength,
            new AcLookupEntry(KindEscape, MdecTables.EscapeLength, 0, 0));

        foreach (var entry in MdecTables.BuildAcCodeBook())
        {
            Fill(table, entry.Code, entry.Length,
                new AcLookupEntry(KindNormal, (byte)entry.Length, (byte)entry.Run, (short)entry.Level));
        }

        return table;

        static void Fill(AcLookupEntry[] table, int code, int length, AcLookupEntry value)
        {
            var shift = AcLookupBits - length;
            var start = code << shift;
            var count = 1 << shift;
            for (var i = 0; i < count; i++)
            {
                if (table[start + i].Kind != KindInvalid)
                {
                    throw new InvalidOperationException(
                        $"MDEC AC code book is not prefix-free: code 0x{code:x} ({length} bits) overlaps an existing entry.");
                }

                table[start + i] = value;
            }
        }
    }

    private static DcLookupEntry[] BuildDcLookup(MdecTables.DcEntry[] codeBook)
    {
        var table = new DcLookupEntry[1 << DcLookupBits];
        foreach (var entry in codeBook)
        {
            var shift = DcLookupBits - entry.Length;
            var start = entry.Code << shift;
            var count = 1 << shift;
            for (var i = 0; i < count; i++)
            {
                if (table[start + i].Length != 0)
                {
                    throw new InvalidOperationException(
                        $"MDEC DC code book is not prefix-free: code 0x{entry.Code:x} ({entry.Length} bits) overlaps an existing entry.");
                }

                table[start + i] = new DcLookupEntry((byte)entry.Length, (byte)entry.Size);
            }
        }

        return table;
    }
}
