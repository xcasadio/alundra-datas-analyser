using System.Diagnostics;
using System.Text;

namespace AlundraEngine.Loader;

/// <summary>
/// One of the four save records the loader reads off the memory card.
///
/// GHIDRA: g_saveSlotRecords — four records of 0x76C bytes. The layout is the game's SaveData
/// blob, so the fields the selection screen reads sit at:
/// +0x00 SlotData, +0x04 LastMapId, +0x08 CurrentFlagName (32 bytes),
/// +0x28 GameStateDescription (32 bytes).
///
/// SOURCE: the record size and the occupancy test come from
/// HandleResourceLoadFailureOrFallback @ 0x80024560, which probes offsets 8, 0x774, 0xEE0 and
/// 0x164C — four probes 0x76C apart, each reading the first byte of CurrentFlagName. The two text
/// fields come from ValidateSelection @ 0x80024b10, which turns the four ASCII digits at +8 into a
/// string-table index and copies 0x20 bytes from +0x28 as the save's name.
/// </summary>
public sealed class LoaderSaveSlotRecord
{
    /// <summary>
    /// GHIDRA: <c>g_saveSlotRecords[slot * 0x76C + 8] != 0</c>.
    /// </summary>
    /// <remarks>
    /// The original's test reads the first byte of CurrentFlagName, but what it is really asking is
    /// "did the card read fill this 0x76C block in?" — LoadSaveSlotsAndPickMessage @ 0x80024560
    /// zeroes 0x2000 bytes first, so any non-zero byte there means the block was written.
    ///
    /// CORRECTION: transliterating that test literally makes the loader blind to every save this
    /// port writes. Nothing in AlundraEngine ever assigns SaveData.CurrentFlagName — it is declared,
    /// copied by CopyFrom and read by MemoryCardManager, but never set — so it is always empty and
    /// every slot was rejected, giving "Aucune donnée Alundra enregistrée" (ETC entry 0xC8) with
    /// saves sitting on disk. On desktop the block-was-filled-in question is answered by whether a
    /// save file parsed, so that is what this now means.
    /// </remarks>
    public bool Occupied;

    /// <summary>
    /// GHIDRA: ValidateSelection @ 0x80024b10 —
    /// <c>(rec[8]-0x30)*1000 + (rec[9]-0x30)*100 + (rec[10]-0x30)*10 - 0x30 + rec[0xb]</c>, i.e. the
    /// four ASCII digits at the head of CurrentFlagName read as a decimal string-table index. It
    /// names the chapter the save sits in: entry 0 of ETC_RES.R is "Un Nouveau Départ", 1 is
    /// "Wendell Succombe", 2 "Fuite vers le Manoir de Tarn", and so on.
    /// </summary>
    /// <remarks>
    /// PARTIAL: -1 when the field cannot supply an index, which on this port is always, because
    /// CurrentFlagName is never written. The confirmation panel then shows the save's own summary
    /// line without a chapter above it. Defaulting to 0 instead would label every save
    /// "Un Nouveau Départ", which would be worse than saying nothing. Writing the four digits at
    /// save time is what would light this up.
    /// </remarks>
    public int PlaceStringIndex = -1;

    /// <summary>GHIDRA: <c>strncpy(name, rec + 0x28, 0x20)</c> — GameStateDescription.</summary>
    public byte[] Name = [];

    /// <summary>The record itself, copied into g_saveDataInRam once a slot is confirmed.</summary>
    public SaveData? Data;
}

/// <summary>
/// Fills the loader's four save records from the desktop save backend.
///
/// GHIDRA: ParseAndLoadResourceScript @ 0x80021ee8, reached through
/// HandleResourceLoadFailureOrFallback @ 0x80024560.
///
/// JUSTIFICATION: PSX hardware adaptation only.
/// RELATION: the original opens the memory card, walks its directory and copies four save blocks
/// into g_saveSlotRecords, returning -1..-5 for the various card failures. There is no card here;
/// AlundraEngine already keeps its saves as <c>{BaseDirectory}/saves/card{0|1}/{title}-{NN}.json</c>
/// and MemoryCardManager writes them, so this reads that directory instead. The five card errors
/// collapse to the two that can still happen on desktop: the directory is missing (-1, "no card")
/// or holds nothing usable, which is the empty-but-valid case the original signals by returning 0
/// with no occupied record.
/// </summary>
public static class LoaderSaveSlots
{
    /// <summary>GHIDRA: the stride between two records in g_saveSlotRecords.</summary>
    public const int RecordSize = 0x76C;

    /// <summary>GHIDRA: HandleResourceLoadFailureOrFallback probes exactly four records.</summary>
    public const int SlotCount = 4;

    /// <summary>Return codes of ParseAndLoadResourceScript that the desktop backend can produce.</summary>
    public const int ResultOk = 0;

    public const int ResultNoCard = -1;

    /// <summary>
    /// Reads up to four saves out of the desktop memory-card directories.
    /// </summary>
    /// <remarks>
    /// Both cards are scanned, card 0 then card 1, because <c>g_memorySlotId</c> starts at 0x270F —
    /// which <c>MemoryCardManager.DesktopMemoryCardDirectory</c> clamps to 1 — and the player can
    /// move it to either slot while playing. Looking at only one of the two would leave the loader
    /// blind to saves depending on where the last one went. The four records are filled in the order
    /// the files are found, which is what the original does with the four blocks it reads off its
    /// single card.
    /// </remarks>
    /// <returns>0 on success, -1 when neither card directory exists.</returns>
    public static int Read(LoaderSaveSlotRecord[] records)
    {
        ArgumentNullException.ThrowIfNull(records);

        for (var index = 0; index < records.Length; index++)
        {
            records[index] = new LoaderSaveSlotRecord();
        }

        var slot = 0;
        var anyCard = false;

        for (var card = 0; card < 2 && slot < records.Length; card++)
        {
            var directory = Path.Combine(AppContext.BaseDirectory, "saves", $"card{card}");
            if (!Directory.Exists(directory))
            {
                continue;
            }

            anyCard = true;

            var files = Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                if (slot >= records.Length)
                {
                    break;
                }

                SaveData data;
                try
                {
                    data = SaveData.LoadFromJson(file);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"'{file}' is not a usable save: {exception.Message}");
                    continue;
                }

                // A file that parsed is a block the card read filled in; see the note on Occupied.
                records[slot].Occupied = true;
                records[slot].Data = data;
                records[slot].PlaceStringIndex = ParsePlaceIndex(data.CurrentFlagName);
                records[slot].Name = ToFixedLatin1(data.GameStateDescription, 0x20);
                slot++;
            }
        }

        return anyCard ? ResultOk : ResultNoCard;
    }

    /// <summary>
    /// GHIDRA: ValidateSelection @ 0x80024b10 — the four digits are read individually and combined
    /// arithmetically, so on the console a non-digit simply produces a nonsense index.
    /// </summary>
    /// <remarks>
    /// Here the field is empty on every save this port writes, and an arithmetic result of 0 would
    /// be indistinguishable from a genuine chapter 0. So the four leading characters must all be
    /// digits for the index to count; anything else returns -1 and the chapter line is skipped.
    /// </remarks>
    private static int ParsePlaceIndex(string? flagName)
    {
        if (flagName is null || flagName.Length < 4)
        {
            return -1;
        }

        var index = 0;
        for (var i = 0; i < 4; i++)
        {
            if (!char.IsAsciiDigit(flagName[i]))
            {
                return -1;
            }

            index = index * 10 + (flagName[i] - '0');
        }

        return index;
    }

    /// <summary>
    /// GHIDRA: <c>strncpy(dst, src, 0x20)</c> followed by an explicit NUL — the original copies at
    /// most 0x20 bytes and terminates the buffer itself.
    /// </summary>
    private static byte[] ToFixedLatin1(string? value, int length)
    {
        var source = Encoding.Latin1.GetBytes(value ?? string.Empty);
        var result = new byte[length + 1];
        Array.Copy(source, result, Math.Min(source.Length, length));
        return result;
    }
}
