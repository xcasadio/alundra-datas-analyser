using System.Diagnostics;

namespace AlundraEngine.Balance;

public class BalanceBin
{
    public readonly string BalanceFile;
    public readonly List<BalanceRecord> BalanceRecords = new();
    public readonly List<int> Offsets = new();

    public BalanceBin(string balanceFile)
    {
        BalanceFile = balanceFile;
        using var br = new BinaryReader(File.OpenRead(balanceFile));
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
        if (spriteIndex - 0x1fU < 0x61)
        {
            spriteIndex = 0x1e;
        }

        var offset = Offsets[spriteIndex];
        var animDataPtr = BalanceRecords[spriteIndex];

        if (animDataPtr.Offset != offset)
        {
            Debugger.Break();
        }

        var currentId = animDataPtr.Level;

        while (currentId < itemIdThreshold)
        {
            animDataPtr = animDataPtr.Next;//(BalanceRecord*)(itemDataPtr->values + (itemDataPtr->offsetToNextLevel - 3));
            currentId = animDataPtr.Level;
        }
        return animDataPtr;
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

        //itemDataPtr = (BalanceRecord*)((int)g_balanceBin.offsets + (uint)(ushort)g_balanceBin.offsets[itemId + 0x1e]);
        var currentId = itemDataPtr.Level;
        
        while (currentId < itemIdThreshold) //g_itemIdThreshold
        {
            itemDataPtr = itemDataPtr.Next;//(BalanceRecord*)(itemDataPtr->values + (itemDataPtr->offsetToNextLevel - 3));
            currentId = itemDataPtr.Level;
        }
        
        return itemDataPtr;
    }
}