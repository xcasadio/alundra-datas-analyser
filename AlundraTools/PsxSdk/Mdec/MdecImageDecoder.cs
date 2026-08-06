namespace PsxSdk.Mdec;

/// <summary>
/// Software equivalent of the PlayStation's MDEC hardware block: turns a stream of 16-bit MDEC
/// codes into 24-bit RGB pixels.
///
/// JUSTIFICATION: PSX hardware adaptation only.
/// RELATION: in the original there is no code to transliterate for this stage.
/// <c>DecDCTin</c> (GHIDRA @ 0x8002b4ac) and <c>DecDCTout</c> (GHIDRA @ 0x8002b528) are three-line
/// DMA kick-off stubs — every part of the work (inverse quantisation, IDCT, YUV to RGB conversion
/// and macroblock assembly) happens inside the MDEC peripheral. This class reimplements that
/// documented behaviour.
/// SOURCE: psx-spx, "Macroblock Decoder (MDEC)".
///
/// Macroblock ordering follows the PSX convention: macroblocks are emitted column by column
/// (top to bottom within a 16-pixel-wide column, then the next column to the right). That is what
/// makes the original's 16-pixel-wide strip decoding work — concatenating the macroblocks of one
/// column yields exactly the raster order of a 16 x height strip.
/// </summary>
public sealed class MdecImageDecoder
{
    /// <summary>Blocks per macroblock: Cr, Cb, then the four luma quadrants.</summary>
    public const int BlocksPerMacroblock = 6;

    private static readonly float[] IdctBasis = BuildIdctBasis();

    private readonly int[] _coefficients = new int[64];
    private readonly float[] _scratch = new float[64];
    private readonly int[][] _blocks =
    [
        new int[64], new int[64], new int[64], new int[64], new int[64], new int[64],
    ];

    /// <summary>Result of decoding a frame.</summary>
    /// <param name="MacroblocksDecoded">Number of macroblocks successfully decoded.</param>
    /// <param name="Truncated">True if the code stream ended before the frame was complete.</param>
    public readonly record struct Result(int MacroblocksDecoded, bool Truncated);

    /// <summary>
    /// Decodes a whole frame into <paramref name="rgb24"/>, which must hold
    /// <c>width * height * 3</c> bytes in raster order.
    /// </summary>
    /// <param name="codes">MDEC code stream produced by <see cref="MdecVlcDecoder"/>.</param>
    /// <param name="offset">Index of the first block word (2 when the command words are present).</param>
    /// <param name="length">Number of valid entries in <paramref name="codes"/>.</param>
    public Result DecodeFrame(ushort[] codes, int offset, int length, int width, int height, byte[] rgb24)
    {
        ArgumentNullException.ThrowIfNull(codes);
        ArgumentNullException.ThrowIfNull(rgb24);
        if (width % 16 != 0 || height % 16 != 0)
        {
            throw new ArgumentException("MDEC frames are made of 16x16 macroblocks; width and height must be multiples of 16.");
        }

        if (rgb24.Length < width * height * 3)
        {
            throw new ArgumentException("Destination buffer is too small for the requested frame size.", nameof(rgb24));
        }

        var columns = width / 16;
        var rows = height / 16;
        var position = offset;
        var decoded = 0;

        for (var column = 0; column < columns; column++)
        {
            for (var row = 0; row < rows; row++)
            {
                for (var block = 0; block < BlocksPerMacroblock; block++)
                {
                    if (!DecodeBlock(codes, length, ref position, _blocks[block]))
                    {
                        return new Result(decoded, true);
                    }
                }

                WriteMacroblock(column * 16, row * 16, width, rgb24);
                decoded++;
            }
        }

        return new Result(decoded, false);
    }

    /// <summary>
    /// Reads one 8x8 block from the code stream, inverse-quantises it and applies the IDCT.
    /// Output is a raster-order 8x8 block of signed samples clamped to -128..127.
    /// </summary>
    private bool DecodeBlock(ushort[] codes, int length, ref int position, int[] destination)
    {
        if (position >= length)
        {
            return false;
        }

        Array.Clear(_coefficients);

        // First word: quantisation scale in bits 10-15, signed 10-bit DC in bits 0-9.
        var first = codes[position++];
        var quantScale = (first >> 10) & 0x3F;
        var dc = SignExtend10(first & 0x3FF);

        // The DC is scaled by the quantisation matrix only; quantScale does not apply to it.
        // Entry 0 of the PSX table is 2 rather than MPEG-1's 8, which is what divides the
        // bitstream's four-times-scaled DC back down to a pixel value.
        _coefficients[0] = dc * MdecTables.DefaultQuantMatrixZigZag[0];

        var k = 0;
        while (position < length)
        {
            var word = codes[position++];
            if (word == MdecVlcDecoder.EndOfBlockCode)
            {
                break;
            }

            k += ((word >> 10) & 0x3F) + 1;
            if (k > 63)
            {
                // Corrupt run length: stop this block rather than writing out of bounds.
                break;
            }

            var level = SignExtend10(word & 0x3FF);
            _coefficients[MdecTables.ZigZagToRaster[k]] =
                (level * MdecTables.DefaultQuantMatrixZigZag[k] * quantScale + 4) / 8;
        }

        InverseDct(_coefficients, destination);
        return true;
    }

    private void InverseDct(int[] coefficients, int[] destination)
    {
        // Pass 1: columns.
        for (var x = 0; x < 8; x++)
        {
            var nonZero = false;
            for (var v = 1; v < 8; v++)
            {
                if (coefficients[v * 8 + x] != 0)
                {
                    nonZero = true;
                    break;
                }
            }

            if (!nonZero)
            {
                // Only the DC term contributes: every output sample of this column is the same.
                var flat = coefficients[x] * IdctBasis[0];
                for (var y = 0; y < 8; y++)
                {
                    _scratch[y * 8 + x] = flat;
                }

                continue;
            }

            for (var y = 0; y < 8; y++)
            {
                var sum = 0f;
                for (var v = 0; v < 8; v++)
                {
                    var coefficient = coefficients[v * 8 + x];
                    if (coefficient != 0)
                    {
                        sum += coefficient * IdctBasis[v * 8 + y];
                    }
                }

                _scratch[y * 8 + x] = sum;
            }
        }

        // Pass 2: rows.
        for (var y = 0; y < 8; y++)
        {
            var rowBase = y * 8;
            for (var x = 0; x < 8; x++)
            {
                var sum = 0f;
                for (var u = 0; u < 8; u++)
                {
                    var value = _scratch[rowBase + u];
                    if (value != 0f)
                    {
                        sum += value * IdctBasis[u * 8 + x];
                    }
                }

                // The MDEC clamps its 9-bit signed output to -128..127.
                var rounded = (int)MathF.Round(sum);
                destination[rowBase + x] = rounded < -128 ? -128 : rounded > 127 ? 127 : rounded;
            }
        }
    }

    /// <summary>
    /// Converts the six decoded blocks into 16x16 RGB pixels at (<paramref name="originX"/>,
    /// <paramref name="originY"/>) of the frame buffer.
    /// </summary>
    private void WriteMacroblock(int originX, int originY, int frameWidth, byte[] rgb24)
    {
        var cr = _blocks[0];
        var cb = _blocks[1];

        for (var y = 0; y < 16; y++)
        {
            var chromaRow = (y >> 1) * 8;

            // _blocks[0] and _blocks[1] hold Cr and Cb, so the four luma quadrants start at
            // index 2: Y0 top-left, Y1 top-right, Y2 bottom-left, Y3 bottom-right.
            var lumaBlockRow = y < 8 ? 2 : 4;
            var lumaRow = (y & 7) * 8;
            var destination = ((originY + y) * frameWidth + originX) * 3;

            for (var x = 0; x < 16; x++)
            {
                var chroma = chromaRow + (x >> 1);
                var luma = _blocks[lumaBlockRow + (x < 8 ? 0 : 1)][lumaRow + (x & 7)];

                var crValue = cr[chroma];
                var cbValue = cb[chroma];

                var r = luma + (int)MathF.Round(1.402f * crValue);
                var g = luma - (int)MathF.Round(0.3437f * cbValue + 0.7143f * crValue);
                var b = luma + (int)MathF.Round(1.772f * cbValue);

                rgb24[destination] = ClampToByte(r + 128);
                rgb24[destination + 1] = ClampToByte(g + 128);
                rgb24[destination + 2] = ClampToByte(b + 128);
                destination += 3;
            }
        }
    }

    private static byte ClampToByte(int value) => value < 0 ? (byte)0 : value > 255 ? (byte)255 : (byte)value;

    private static int SignExtend10(int value) => (value & 0x200) != 0 ? value - 0x400 : value;

    /// <summary>
    /// <c>IdctBasis[u * 8 + x] = c(u) / 2 * cos((2x + 1) * u * PI / 16)</c>, the separable 1-D
    /// inverse DCT basis.
    /// </summary>
    private static float[] BuildIdctBasis()
    {
        var basis = new float[64];
        for (var u = 0; u < 8; u++)
        {
            var cu = u == 0 ? MathF.Sqrt(0.5f) : 1f;
            for (var x = 0; x < 8; x++)
            {
                basis[u * 8 + x] = 0.5f * cu * MathF.Cos((2 * x + 1) * u * MathF.PI / 16f);
            }
        }

        return basis;
    }
}
