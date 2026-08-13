namespace AlundraEngine;

/// <summary>
/// The story's chapter list: one entry per chapter, in order, each naming the flag that closes it.
/// Where the player is in the story is simply the first entry whose flag is not set yet.
///
/// GHIDRA: g_flagIdList — records of 0x22 bytes laid out as <c>[32-byte text][u16 flagId]</c>, so
/// the symbol points at the first record's id field rather than at its start. ALUN_CD.EXE keeps it
/// at 0x8002962e on the France disc and 0x800295d2 on the USA one, walked by
/// GetFirstEnabledFlagIndex @ 0x800813b0.
///
/// SOURCE: the 41 flag ids plus the 0xFFFF terminator are byte for byte identical on both discs,
/// which is why they are data here rather than something read out of the executable. Only the text
/// differs, and this port does not need it — see <see cref="CurrentFlagName"/>.
/// </summary>
public static class ChapterFlags
{
    /// <summary>An id of 0x8000 or above ends the walk; the last chapter has no flag to close it.</summary>
    private const ushort Terminator = 0x8000;

    /// <summary>
    /// The flag closing each chapter, in story order. Entry 41 is the terminator.
    /// </summary>
    private static readonly ushort[] FlagIds =
    [
        0x0003, 0x0008, 0x006C, 0x0676, 0x006D, 0x00EF, 0x0127, 0x00F7,
        0x00E9, 0x00E8, 0x015C, 0x012B, 0x028B, 0x014B, 0x0578, 0x0101,
        0x0579, 0x057A, 0x0372, 0x057B, 0x057C, 0x057D, 0x0385, 0x057E,
        0x0510, 0x0023, 0x001F, 0x0046, 0x0048, 0x004F, 0x037A, 0x01D3,
        0x0233, 0x06A2, 0x037B, 0x00E7, 0x044F, 0x03D0, 0x0664, 0x049F,
        0x04A0, 0xFFFF,
    ];

    /// <summary>
    /// GHIDRA: GetFirstEnabledFlagIndex @ 0x800813b0 — the index of the chapter the player is in.
    /// </summary>
    /// <remarks>
    /// The walk stops on the first entry whose flag is clear, or on the terminator once every flag
    /// is set. An id of 0 is skipped rather than tested; neither disc's table holds one, but the
    /// original checks for it.
    ///
    /// The original also reads <c>g_temporaryFlags</c> when an id has bit 0x8000 set. That branch
    /// cannot be reached: the same bit is what the preceding test treats as the terminator, so it
    /// returns first. Left out rather than transliterated as dead code.
    /// </remarks>
    public static int GetFirstEnabledFlagIndex(uint[] gameFlags)
    {
        ArgumentNullException.ThrowIfNull(gameFlags);

        for (var index = 0; index < FlagIds.Length; index++)
        {
            var flagId = FlagIds[index];
            if (flagId == 0)
            {
                continue;
            }

            if (flagId >= Terminator)
            {
                return index;
            }

            var word = flagId >> 5;
            if (word >= gameFlags.Length || (gameFlags[word] & (1u << (flagId & 0x1F))) == 0)
            {
                return index;
            }
        }

        return FlagIds.Length - 1;
    }

    /// <summary>
    /// The value <c>UpdateMenuStatusText</c> stores in <c>SaveData.CurrentFlagName</c>, which the
    /// loader turns back into the chapter's name.
    /// </summary>
    /// <remarks>
    /// The original copies the chapter record's 32-byte text. On the France disc that text is the
    /// chapter index as four ASCII digits followed by the Japanese name, and its loader uses only
    /// the digits — it reads them back as an index into ETC_RES.R, whose entry 0 is
    /// "Un Nouveau Départ". So the digits are the whole payload as far as this port is concerned.
    ///
    /// DELIBERATE DEVIATION: the USA disc drops the digits and keeps only the Japanese, and its
    /// loader draws those 32 bytes directly instead of looking anything up — on a console they come
    /// out of the BIOS kanji ROM. This port has no kanji ROM (see LoaderTextLayer), so writing that
    /// text would render nothing at all. Writing the digits on both discs makes the USA loader
    /// resolve the chapter through ETC_USA.R exactly as France does, and show "A New Beginning"
    /// where the original shows Japanese.
    /// </remarks>
    public static string CurrentFlagName(uint[] gameFlags) =>
        GetFirstEnabledFlagIndex(gameFlags).ToString("0000");
}
