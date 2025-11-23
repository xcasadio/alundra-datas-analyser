using System.Diagnostics;

namespace AlundraEngine.Balance;

public class BalanceBin
{
    private readonly string _balanceFile;
    private readonly List<BalanceRecord> _balanceRecords = new();
    private readonly List<int> _offsets = new();

    public BalanceBin(string balanceFile)
    {
        _balanceFile = balanceFile;
        using var br = new BinaryReader(File.OpenRead(balanceFile));
        var firstOffset = 0;

        while (firstOffset == 0 || br.BaseStream.Position < firstOffset)
        {
            int offset = br.ReadInt16();
            if (firstOffset == 0)
            {
                firstOffset = offset;
            }

            _offsets.Add(offset);
        }

        foreach (var offset in _offsets)
        {
            var record = new BalanceRecord(br, offset);
            _balanceRecords.Add(record);
        }
    }

    //80044550
    //public BalanceRecord GetBalanceRecordFromSpriteIndex(int index, int balanceLevel)
    //{
    //    var record = _balanceRecords[index];
    //    
    //    if (record.Level >= balanceLevel)
    //    {
    //        return record;
    //    }
    //
    //    do
    //    {
    //        record = record.Next;
    //    } while (record.Level < balanceLevel);
    //
    //    return record;
    //}

    //80044550
    public BalanceRecord GetBalanceRecordFromSpriteIndex(int spriteIndex, int itemIdThreshold)
    {
        if (spriteIndex - 0x1fU < 0x61)
        {
            spriteIndex = 0x1e;
        }

        var offset = _offsets[spriteIndex];
        var animDataPtr = _balanceRecords[spriteIndex];

        if (animDataPtr.Offset != offset)
        {
            Debugger.Break();
        }

        var currentId = animDataPtr.Level;

        while ((int)(uint)currentId < itemIdThreshold)
        {
            animDataPtr = animDataPtr.Next;//(BalanceRecord*)(itemDataPtr->values + (itemDataPtr->offsetToNextLevel - 3));
            currentId = animDataPtr.Level;
        }
        return animDataPtr;
    }

    //800445c0
    public BalanceRecord GetItemDataPointer(int itemId, int itemIdThreshold)
    {
        //Debugger.Break();
        
        if (0x61 < itemId)
        {
            //DoNothing();
            itemId = 1;
            //exit();
            Debugger.Break();
        }

        var offset = _offsets[itemId + 0x1e];
        var itemDataPtr = _balanceRecords[itemId + 0x1e];

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