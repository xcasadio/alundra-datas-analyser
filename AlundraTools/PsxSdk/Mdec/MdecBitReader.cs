namespace PsxSdk.Mdec;

/// <summary>
/// Bit reader for a PSX MDEC bitstream: the payload is a sequence of little-endian 16-bit words,
/// and inside each word bits are consumed most-significant first.
///
/// RELATION to the original code: libpress keeps a 32-bit sliding window (<c>uVar7</c>) plus a
/// 0..15 bit counter (<c>uVar8</c>) and pulls a fresh <c>u16</c> whenever the counter crosses 16
/// (GHIDRA: DecDCTvlc @ 0x8002ba84). That is an in-register optimisation of exactly this
/// behaviour; expressed here as an explicit accumulator so the bit order stays readable and
/// testable.
/// </summary>
public sealed class MdecBitReader
{
    private readonly byte[] _data;
    private readonly int _end;
    private int _position;
    private ulong _accumulator;
    private int _available;

    /// <summary>Creates a reader over <paramref name="data"/> starting at <paramref name="offset"/>.</summary>
    public MdecBitReader(byte[] data, int offset, int length)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (offset < 0 || length < 0 || offset + length > data.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        _data = data;
        _position = offset;
        _end = offset + length;
    }

    /// <summary>True once the reader has run past the end of the payload.</summary>
    public bool Exhausted { get; private set; }

    /// <summary>Number of bits consumed so far.</summary>
    public int BitsConsumed { get; private set; }

    /// <summary>Returns the next <paramref name="count"/> bits without consuming them.</summary>
    public int Peek(int count)
    {
        Fill(count);
        return (int)((_accumulator >> (_available - count)) & ((1UL << count) - 1));
    }

    /// <summary>Consumes <paramref name="count"/> bits.</summary>
    public void Skip(int count)
    {
        Fill(count);
        _available -= count;
        BitsConsumed += count;
        _accumulator &= (1UL << _available) - 1;
    }

    /// <summary>Reads and consumes <paramref name="count"/> bits.</summary>
    public int Read(int count)
    {
        var value = Peek(count);
        Skip(count);
        return value;
    }

    /// <summary>
    /// Reads <paramref name="size"/> bits and interprets them the way MPEG-1 codes a differential
    /// DC value: if the most significant bit is clear the value is negative and
    /// <c>(2^size - 1)</c> is subtracted.
    /// </summary>
    /// <remarks>
    /// RELATION: matches the <c>uVar12 - (0xffffffff &gt;&gt; (0x20 - uVar16))</c> branch of
    /// DecDCTvlc, taken when the sign bit of the freshly shifted window is clear.
    /// </remarks>
    public int ReadDifferential(int size)
    {
        if (size == 0)
        {
            return 0;
        }

        var value = Read(size);
        if ((value & (1 << (size - 1))) == 0)
        {
            value -= (1 << size) - 1;
        }

        return value;
    }

    private void Fill(int count)
    {
        if (count is < 0 or > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        while (_available < count)
        {
            ushort word;
            if (_position + 1 < _end)
            {
                word = (ushort)(_data[_position] | (_data[_position + 1] << 8));
                _position += 2;
            }
            else
            {
                // Past the end of the demuxed payload. Feeding zeroes keeps the decoder well
                // defined; callers detect the condition through Exhausted rather than by
                // trusting the decoded values.
                word = 0;
                Exhausted = true;
            }

            _accumulator = (_accumulator << 16) | word;
            _available += 16;
        }
    }
}
