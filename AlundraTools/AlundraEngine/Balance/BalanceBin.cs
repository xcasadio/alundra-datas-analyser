using System.Diagnostics;

namespace AlundraEngine.Balance;

public class BalanceBin
{
    public readonly string FileName;
    public readonly List<BalanceRecord> BalanceRecords = new();
    public readonly List<int> Offsets = new();

    public BalanceBin(string fileName)
    {
        FileName = fileName;
        using var br = new BinaryReader(File.OpenRead(fileName));
        var firstOffset = 0;

        while (firstOffset == 0 || br.BaseStream.Position < firstOffset)
        {
            int offset = br.ReadInt16();
            if (firstOffset == 0)
            {
                firstOffset = offset;
            }

            Offsets.Add(offset);
        }

        foreach (var offset in Offsets)
        {
            var record = new BalanceRecord(br, offset);
            BalanceRecords.Add(record);
        }
    }

    //80044550
    public BalanceRecord GetBalanceRecordFromSpriteIndex(int spriteIndex, int itemIdThreshold)
    {
        if ((uint)(spriteIndex - 0x1fU) < 0x61)
        {
            spriteIndex = 0x1e;
        }

        var offset = Offsets[spriteIndex];
        var balanceRecord = BalanceRecords[spriteIndex];

        if (balanceRecord.Offset != offset)
        {
            Debugger.Break();
        }

        var currentId = balanceRecord.Level;

        while (currentId < itemIdThreshold)
        {
            balanceRecord = balanceRecord.Next;
            currentId = balanceRecord.Level;
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
            Debugger.Break();
        }

        var offset = Offsets[itemId + 0x1e];
        var itemDataPtr = BalanceRecords[itemId + 0x1e];

        if (itemDataPtr.Offset != offset)
        {
            Debugger.Break();
        }

        var currentId = itemDataPtr.Level;
        
        while (currentId < itemIdThreshold)
        {
            itemDataPtr = itemDataPtr.Next;
            currentId = itemDataPtr.Level;
        }
        
        return itemDataPtr;
    }
}