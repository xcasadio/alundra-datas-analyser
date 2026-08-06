namespace PsxSdk.Mdec;

/// <summary>
/// Static tables shared by the MDEC bitstream decoder and the MDEC image decoder.
///
/// SOURCE: public PlayStation hardware documentation (psx-spx "Macroblock Decoder (MDEC)") and the
/// ISO/IEC 11172-2 (MPEG-1) variable length code tables that the PSX MDEC bitstream reuses.
///
/// RELATION to the original code: libpress's DecDCTvlc (GHIDRA: DecDCTvlc @ 0x8002ba84 in
/// LOADER.EXE) does not carry these tables in this readable form — it uses four pre-flattened
/// lookup tables that live as data inside the executable:
///   - 0x80132a88  DC size table, luma      (256 entries x 4 bytes, indexed by the top 8 bits)
///   - 0x80132e88  DC size table, chroma    (256 entries x 4 bytes)
///   - 0x80133288  AC primary lookup        (8192 entries x 8 bytes, indexed by the top 13 bits)
///   - 0x80143288  AC secondary/escape      (512 entries x 4 bytes, indexed by the top 9 bits)
/// Those flattened tables encode exactly the standard tables reproduced below (a decoding
/// accelerator, not a different code book). They are deliberately NOT read out of the Alundra
/// executable here: this assembly must stay usable for any other PSX title, so the code book is
/// expressed in its canonical documented form instead and expanded into an equivalent lookup at
/// static-init time (see <see cref="MdecVlcDecoder"/>).
/// </summary>
public static class MdecTables
{
    /// <summary>
    /// Zig-zag scan order: <c>ZigZagToRaster[k]</c> is the raster index (0..63) of the k-th
    /// coefficient in scan order.
    /// </summary>
    public static readonly int[] ZigZagToRaster =
    [
         0,  1,  8, 16,  9,  2,  3, 10,
        17, 24, 32, 25, 18, 11,  4,  5,
        12, 19, 26, 33, 40, 48, 41, 34,
        27, 20, 13,  6,  7, 14, 21, 28,
        35, 42, 49, 56, 57, 50, 43, 36,
        29, 22, 15, 23, 30, 37, 44, 51,
        58, 59, 52, 45, 38, 31, 39, 46,
        53, 60, 61, 54, 47, 55, 62, 63,
    ];

    /// <summary>
    /// Default intra quantisation matrix, in the zig-zag order the MDEC is fed with, so it is
    /// indexed directly by the scan position k.
    ///
    /// SOURCE: read out of LOADER.EXE at 0x80132944 — the buffer <c>MDEC_reset</c> (GHIDRA @
    /// 0x8002b5d0) hands to <c>MDEC_in</c> together with the 0x40000001 "set quantisation table"
    /// command. The upload contains this table twice, once for luma and once for chroma; both
    /// copies are byte-identical, so a single table serves both.
    ///
    /// This is the MPEG-1 default intra matrix mapped through <see cref="ZigZagToRaster"/>, with
    /// exactly ONE difference, verified entry by entry against the executable: position 0 holds
    /// <b>2</b> where MPEG-1 has 8. That factor of four is not cosmetic — it is what makes a
    /// decoded DC land on the right brightness, because the STR bitstream stores DC values
    /// multiplied by four in the MDEC's 10-bit DC field (explicitly so for version 3, where
    /// DecDCTvlc emits <c>(dc &amp; 0xff) &lt;&lt; 2</c>, and implicitly for version 2's raw
    /// 10-bit DC).
    /// </summary>
    public static readonly int[] DefaultQuantMatrixZigZag =
    [
         2, 16, 16, 19, 16, 19, 22, 22,
        22, 22, 22, 22, 26, 24, 26, 27,
        27, 27, 26, 26, 26, 26, 27, 27,
        27, 29, 29, 29, 34, 34, 34, 29,
        29, 29, 27, 27, 29, 29, 32, 32,
        34, 34, 37, 38, 37, 35, 35, 34,
        35, 38, 38, 40, 40, 40, 48, 48,
        46, 46, 56, 56, 58, 69, 69, 83,
    ];

    /// <summary>One entry of the AC coefficient code book.</summary>
    /// <param name="Code">Code value, right-aligned in <paramref name="Length"/> bits.</param>
    /// <param name="Length">Code length in bits, excluding the trailing sign bit.</param>
    /// <param name="Run">Number of zero coefficients preceding this one.</param>
    /// <param name="Level">Absolute coefficient value; the trailing sign bit gives the sign.</param>
    public readonly record struct AcEntry(int Code, int Length, int Run, int Level);

    /// <summary>Length in bits of the AC end-of-block code (<c>10</c>).</summary>
    public const int EndOfBlockLength = 2;

    /// <summary>Value of the AC end-of-block code.</summary>
    public const int EndOfBlockCode = 0b10;

    /// <summary>Length in bits of the AC escape code (<c>000001</c>).</summary>
    public const int EscapeLength = 6;

    /// <summary>Value of the AC escape code; followed by a raw 6-bit run and 10-bit signed level.</summary>
    public const int EscapeCode = 0b000001;

    /// <summary>
    /// Longest AC code, excluding the sign bit. Sets the width of the decoder's lookup table.
    /// </summary>
    public const int MaxAcCodeLength = 16;

    /// <summary>
    /// The AC coefficient code book. The two-bit code <c>10</c> is end-of-block and the six-bit
    /// code <c>000001</c> is the escape prefix; both are handled separately and are therefore not
    /// listed here.
    /// </summary>
    public static AcEntry[] BuildAcCodeBook()
    {
        var entries = new List<AcEntry>
        {
            new(0b11,       2,  0,  1),
            new(0b011,      3,  1,  1),
            new(0b0100,     4,  0,  2),
            new(0b0101,     4,  2,  1),
            new(0b00101,    5,  0,  3),
            new(0b00110,    5,  4,  1),
            new(0b00111,    5,  3,  1),
            new(0b000100,   6,  7,  1),
            new(0b000101,   6,  6,  1),
            new(0b000110,   6,  1,  2),
            new(0b000111,   6,  5,  1),
            new(0b0000100,  7,  2,  2),
            new(0b0000101,  7,  9,  1),
            new(0b0000110,  7,  0,  4),
            new(0b0000111,  7,  8,  1),
            new(0b00100000, 8, 13,  1),
            new(0b00100001, 8,  0,  6),
            new(0b00100010, 8, 12,  1),
            new(0b00100011, 8, 11,  1),
            new(0b00100100, 8,  3,  2),
            new(0b00100101, 8,  1,  3),
            new(0b00100110, 8,  0,  5),
            new(0b00100111, 8, 10,  1),
        };

        // 10-bit group: base 0b0000001000 + i
        int[,] tenBit = { { 16, 1 }, { 5, 2 }, { 0, 7 }, { 2, 3 }, { 1, 4 }, { 15, 1 }, { 14, 1 }, { 4, 2 } };
        for (var i = 0; i < 8; i++)
        {
            entries.Add(new AcEntry(0b0000001000 + i, 10, tenBit[i, 0], tenBit[i, 1]));
        }

        // 12-bit group: base 0b000000010000 + i
        int[,] twelveBit =
        {
            { 0, 11 }, { 8, 2 }, { 4, 3 }, { 0, 10 }, { 2, 4 }, { 7, 2 }, { 21, 1 }, { 20, 1 },
            { 0,  9 }, { 19, 1 }, { 18, 1 }, { 1, 5 }, { 3, 3 }, { 0, 8 }, { 6, 2 }, { 17, 1 },
        };
        for (var i = 0; i < 16; i++)
        {
            entries.Add(new AcEntry(0b000000010000 + i, 12, twelveBit[i, 0], twelveBit[i, 1]));
        }

        // 13-bit group: base 0b0000000010000 + i
        int[,] thirteenBit =
        {
            { 10, 2 }, { 9, 2 }, { 5, 3 }, { 3, 4 }, { 2, 5 }, { 1, 7 }, { 1, 6 }, { 0, 15 },
            { 0, 14 }, { 0, 13 }, { 0, 12 }, { 26, 1 }, { 25, 1 }, { 24, 1 }, { 23, 1 }, { 22, 1 },
        };
        for (var i = 0; i < 16; i++)
        {
            entries.Add(new AcEntry(0b0000000010000 + i, 13, thirteenBit[i, 0], thirteenBit[i, 1]));
        }

        // 14-bit group: base 0b00000000010000 + i -> run 0, level 31 down to 16
        for (var i = 0; i < 16; i++)
        {
            entries.Add(new AcEntry(0b00000000010000 + i, 14, 0, 31 - i));
        }

        // 15-bit group: base 0b000000000010000 + i
        //   i = 0..8  -> run 0, level 40 down to 32
        //   i = 9..15 -> run 1, level 14 down to 8
        for (var i = 0; i < 16; i++)
        {
            entries.Add(i < 9
                ? new AcEntry(0b000000000010000 + i, 15, 0, 40 - i)
                : new AcEntry(0b000000000010000 + i, 15, 1, 14 - (i - 9)));
        }

        // 16-bit group: base 0b0000000000010000 + i
        int[,] sixteenBit =
        {
            {  1, 18 }, {  1, 17 }, {  1, 16 }, {  1, 15 }, {  6, 3 }, { 16, 2 }, { 15, 2 }, { 14, 2 },
            { 13,  2 }, { 12,  2 }, { 11,  2 }, { 31,  1 }, { 30, 1 }, { 29, 1 }, { 28, 1 }, { 27, 1 },
        };
        for (var i = 0; i < 16; i++)
        {
            entries.Add(new AcEntry(0b0000000000010000 + i, 16, sixteenBit[i, 0], sixteenBit[i, 1]));
        }

        return entries.ToArray();
    }

    /// <summary>One entry of a DC size code book (STR version 3 differential DC coding).</summary>
    /// <param name="Code">Code value, right-aligned in <paramref name="Length"/> bits.</param>
    /// <param name="Length">Code length in bits.</param>
    /// <param name="Size">Number of raw bits that follow, carrying the differential value.</param>
    public readonly record struct DcEntry(int Code, int Length, int Size);

    /// <summary>Longest DC size code, in bits. Sets the width of the decoder's DC lookup tables.</summary>
    public const int MaxDcCodeLength = 8;

    /// <summary>
    /// Luma DC size code book. Flattened equivalent of the table at 0x80132a88 in LOADER.EXE.
    /// </summary>
    public static readonly DcEntry[] LumaDcCodeBook =
    [
        new(0b100,     3, 0),
        new(0b00,      2, 1),
        new(0b01,      2, 2),
        new(0b101,     3, 3),
        new(0b110,     3, 4),
        new(0b1110,    4, 5),
        new(0b11110,   5, 6),
        new(0b111110,  6, 7),
        new(0b1111110, 7, 8),
    ];

    /// <summary>
    /// Chroma DC size code book. Flattened equivalent of the table at 0x80132e88 in LOADER.EXE.
    /// </summary>
    public static readonly DcEntry[] ChromaDcCodeBook =
    [
        new(0b00,        2, 0),
        new(0b01,        2, 1),
        new(0b10,        2, 2),
        new(0b110,       3, 3),
        new(0b1110,      4, 4),
        new(0b11110,     5, 5),
        new(0b111110,    6, 6),
        new(0b1111110,   7, 7),
        new(0b11111110,  8, 8),
    ];
}
