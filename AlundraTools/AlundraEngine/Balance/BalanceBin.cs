using System.Diagnostics;

namespace AlundraEngine.Balance;

public class BalanceBin
{
    public readonly string FileName;

    // Têtes de chaîne indexées par index de la table de sprites.
    // La fenêtre 0x1f..0x7f est réservée aux objets (voir GetItemDataPointer) ;
    // les sprites qui tombent dedans sont rabattus sur l'entrée 0x1e.
    public readonly List<BalanceRecord> EntriesBySpriteIndex = new();

    public BalanceBin(string fileName)
    {
        FileName = fileName;
        using var br = new BinaryReader(File.OpenRead(fileName));
        var firstOffset = 0;
        var offsets = new List<int>();

        while (firstOffset == 0 || br.BaseStream.Position < firstOffset)
        {
            int offset = br.ReadInt16();
            if (firstOffset == 0)
            {
                firstOffset = offset;
            }

            offsets.Add(offset);
        }

        foreach (var offset in offsets)
        {
            var record = new BalanceRecord(br, offset);
            EntriesBySpriteIndex.Add(record);
        }
    }

    //80044550
    public BalanceRecord GetBalanceRecordFromSpriteIndex(int spriteIndex, int itemIdThreshold)
    {
        if ((uint)(spriteIndex - 0x1fU) < 0x61)
        {
            spriteIndex = 0x1e;
        }

        var balanceRecord = EntriesBySpriteIndex[spriteIndex];
        var currentId = balanceRecord.BalanceLevel;

        while (currentId < itemIdThreshold)
        {
            balanceRecord = balanceRecord.NextLevel;
            currentId = balanceRecord.BalanceLevel;
        }
        return balanceRecord;
    }

    //800445c0
    public BalanceRecord GetItemDataPointer(int itemId, int itemIdThreshold)
    {
        if (0x61 < itemId)
        {
            //DoNothing();
            itemId = 1;
            //exit();
            Breakpoint.TriggerBreak();
        }

        var itemDataPtr = EntriesBySpriteIndex[itemId + 0x1e];
        var currentId = itemDataPtr.BalanceLevel;

        while (currentId < itemIdThreshold)
        {
            itemDataPtr = itemDataPtr.NextLevel;
            currentId = itemDataPtr.BalanceLevel;
        }

        return itemDataPtr;
    }
}
