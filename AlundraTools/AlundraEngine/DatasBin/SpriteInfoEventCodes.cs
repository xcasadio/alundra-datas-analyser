using System.Reflection.Emit;

namespace AlundraEngine.DatasBin;

public class SpriteInfoEventCodes
{
    private readonly long _binOffset;
    private readonly int _dataSize;
    private readonly int _memoryAddress;
    public readonly short[] EventCodesATable;
    public readonly short[] EventCodesBTable;
    public readonly short[] EventCodesCTable;
    public readonly short[] EventCodesDTable;
    public readonly short[] EventCodesETable;
    public readonly short[] EventCodesFTable;
    public readonly byte[] Codes;

    public SpriteInfoEventCodes(BinaryReader br, long binOffset, SpriteInfoHeader header)
    {
        var tableSize = 0;
        short firstOffset = 0;

        //read sector1a
        br.BaseStream.Position = binOffset + header.EventCodesAPointer;
        tableSize = header.EventCodesASize / 2;
        EventCodesATable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesATable[i] = br.ReadInt16();
            if (firstOffset == 0 && EventCodesATable[i] != 0)
            {
                firstOffset = EventCodesATable[i];
            }
        }

        //read sector1b
        br.BaseStream.Position = binOffset + header.EventCodesBPointer;
        tableSize = header.EventCodesBSize / 2;
        EventCodesBTable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesBTable[i] = br.ReadInt16();
            if (firstOffset == 0 && EventCodesBTable[i] != 0)
            {
                firstOffset = EventCodesBTable[i];
            }
        }

        //read sector1c
        br.BaseStream.Position = binOffset + header.EventCodesCPointer;
        tableSize = header.EventCodesCSize / 2;
        EventCodesCTable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesCTable[i] = br.ReadInt16();
            if (firstOffset == 0 && EventCodesCTable[i] != 0)
            {
                firstOffset = EventCodesCTable[i];
            }
        }

        //read sector1d
        br.BaseStream.Position = binOffset + header.EventCodesDPointer;
        tableSize = header.EventCodesDSize / 2;
        EventCodesDTable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesDTable[i] = br.ReadInt16();
            if (firstOffset == 0 && EventCodesDTable[i] != 0)
            {
                firstOffset = EventCodesDTable[i];
            }
        }

        //read sector1e
        br.BaseStream.Position = binOffset + header.EventCodesEPointer;
        tableSize = header.EventCodesESize / 2;
        EventCodesETable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesETable[i] = br.ReadInt16();
            if (firstOffset == 0 && EventCodesETable[i] != 0)
            {
                firstOffset = EventCodesETable[i];
            }
        }

        //read sector1f
        header.EventCodesFSize = header.EventCodesAPointer + firstOffset - header.EventCodesFPointer;
        br.BaseStream.Position = binOffset + header.EventCodesFPointer;
        tableSize = header.EventCodesFSize / 2;
        if (tableSize < 0)
        {
            tableSize = 16;
        }

        EventCodesFTable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesFTable[i] = br.ReadInt16();
        }

        //set binOffset for eventcodes
        _binOffset = binOffset + header.EventCodesAPointer;
        _memoryAddress = header.MemoryAddress + header.EventCodesAPointer;
        _dataSize = (header.EntitiesPointer == 0 ? header.EventCodesFPointer : header.EntitiesPointer) - header.EventCodesAPointer;
        Codes = new byte[_dataSize];
        br.BaseStream.Position = _binOffset;
        br.Read(Codes, 0, Codes.Length);
    }
}