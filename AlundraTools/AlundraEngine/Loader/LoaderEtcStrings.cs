using System.Diagnostics;

namespace AlundraEngine.Loader;

/// <summary>
/// The loader's string table, <c>DATA/ETC_RES.R</c>.
///
/// GHIDRA: loaded by InitializePsx @ 0x80025238 with
/// <c>LoadEtcFile("DATA\ETC_RES.R", &amp;g_etcResRBuffer, 0x3000)</c>, read by GetEtcResourceEntry
/// @ 0x80025184, which is a one-liner:
/// <c>return g_etcResRBuffer.Index + (u16)g_etcResRBuffer.Index[entryIndex];</c>
///
/// The file therefore starts with an array of 16-bit offsets, each relative to the start of the file
/// itself, and the strings follow. On the France build the array is 0x800 bytes (1024 entries) and
/// entry 0 points at 0x0801.
///
/// JUSTIFICATION: PSX hardware adaptation only.
/// RELATION: the original reads the file off the CD into a fixed RAM buffer and hands out pointers
/// into it. Here the file is read into a byte array and an entry is a (buffer, offset) pair — the
/// same addressing. The buffer is deliberately shared by every entry rather than sliced per string,
/// because the typewriter's blip is gated on the parity of the character's address and slicing would
/// change it.
/// </summary>
public sealed class LoaderEtcStrings
{
    /// <summary>GHIDRA: the buffer size InitializePsx reserves.</summary>
    private const int BufferSize = 0x3000;

    /// <summary>GHIDRA: g_etcResRBuffer @ 0x8014f8a8.</summary>
    public byte[] Buffer { get; }

    /// <summary>How many 16-bit offsets the index holds, derived from where the first one points.</summary>
    public int EntryCount { get; }

    private LoaderEtcStrings(byte[] buffer, int entryCount)
    {
        Buffer = buffer;
        EntryCount = entryCount;
    }

    /// <summary>
    /// Reads <c>DATA/ETC_RES.R</c>, or returns null when it is missing or malformed.
    /// </summary>
    public static LoaderEtcStrings? Load(string gamePath)
    {
        var path = Path.Combine(gamePath, "DATA", "ETC_RES.R");
        if (!File.Exists(path))
        {
            Debug.WriteLine($"ETC_RES.R not found at '{path}'; the loader will run without its strings.");
            return null;
        }

        var content = File.ReadAllBytes(path);
        var buffer = new byte[Math.Max(BufferSize, content.Length)];
        Array.Copy(content, buffer, content.Length);

        if (content.Length < 4)
        {
            return null;
        }

        // The index runs up to the first string, so the first offset is also the index's byte size.
        var firstOffset = BitConverter.ToUInt16(buffer, 0);
        var entryCount = firstOffset / 2;
        if (entryCount <= 0 || firstOffset > content.Length)
        {
            Debug.WriteLine($"ETC_RES.R at '{path}' has an unusable index (first offset 0x{firstOffset:X}).");
            return null;
        }

        return new LoaderEtcStrings(buffer, entryCount);
    }

    /// <summary>
    /// GHIDRA: GetEtcResourceEntry @ 0x80025184 — the byte offset of one entry inside
    /// <see cref="Buffer"/>, or -1 when the index is out of range.
    /// </summary>
    /// <remarks>
    /// The original does no range check at all; a bad index reads whatever u16 follows the array.
    /// Returning -1 instead is the only deviation, and no caller in the loader passes an index the
    /// table does not hold.
    /// </remarks>
    public int GetEntryOffset(int entryIndex)
    {
        if ((uint)entryIndex >= (uint)EntryCount)
        {
            return -1;
        }

        var offset = BitConverter.ToUInt16(Buffer, entryIndex * 2);
        return offset < Buffer.Length ? offset : -1;
    }

    /// <summary>Decodes one entry to text, for diagnostics only — the loader draws the bytes.</summary>
    public string GetEntryText(int entryIndex)
    {
        var offset = GetEntryOffset(entryIndex);
        if (offset < 0)
        {
            return string.Empty;
        }

        var end = offset;
        while (end < Buffer.Length && Buffer[end] != 0)
        {
            end++;
        }

        return System.Text.Encoding.Latin1.GetString(Buffer, offset, end - offset);
    }
}
