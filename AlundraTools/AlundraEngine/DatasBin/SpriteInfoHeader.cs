namespace AlundraEngine.DatasBin;

public class SpriteInfoHeader
{
    public SpriteInfoHeader(BinaryReader br, int memoryAddress)
    {
        EntitiesPointer = br.ReadInt32();
        MapEffectSector3Pointer = br.ReadInt32();
        MapEventsPointer = br.ReadInt32();
        SpriteTablePointer = br.ReadInt32();
        SpriteEffectsPointer = br.ReadInt32();
        SpritePalettesPointer = br.ReadInt32();
        EventCodesAPointer = br.ReadInt32();
        EventCodesBPointer = br.ReadInt32();
        EventCodesCPointer = br.ReadInt32();
        EventCodesDPointer = br.ReadInt32();
        EventCodesEPointer = br.ReadInt32();
        EventCodesFPointer = br.ReadInt32();

        MemoryAddress = memoryAddress;
        EventCodeAddress = memoryAddress + EventCodesAPointer;

        EntitiesSize = MapEffectSector3Pointer - EntitiesPointer;
        MapEffectSector3Size = MapEventsPointer - MapEffectSector3Pointer;
        MapEventsSize = -1;// unknown4 - unknown3;
        SpriteTableSize = SpriteEffectsPointer - SpriteTablePointer;
        SpriteEffectsSize = SpritePalettesPointer - SpriteEffectsPointer;
        SpritePalettesSize = EventCodesAPointer - SpritePalettesPointer;
        EventCodesASize = EventCodesBPointer - EventCodesAPointer;
        EventCodesBSize = EventCodesCPointer - EventCodesBPointer;
        EventCodesCSize = EventCodesDPointer - EventCodesCPointer;
        EventCodesDSize = EventCodesEPointer - EventCodesDPointer;
        EventCodesESize = EventCodesFPointer - EventCodesEPointer;
        EventCodesFAndRemainingSize = EntitiesPointer - EventCodesFPointer;
    }
    public readonly int MemoryAddress;
    public readonly int EventCodeAddress;

    public readonly int EntitiesPointer;
    public readonly int EntitiesSize;
    public readonly int MapEffectSector3Pointer;
    public readonly int MapEffectSector3Size;
    public readonly int MapEventsPointer;
    public readonly int MapEventsSize;
    public readonly int SpriteTablePointer;
    public readonly int SpriteTableSize;
    public readonly int SpriteEffectsPointer;//0000333b000e240e0400000000000000
    public readonly int SpriteEffectsSize;
    public readonly int SpritePalettesPointer;
    public readonly int SpritePalettesSize;
    public readonly int EventCodesAPointer;
    public readonly int EventCodesASize;
    public readonly int EventCodesBPointer;
    public readonly int EventCodesBSize;
    public readonly int EventCodesCPointer;
    public readonly int EventCodesCSize;
    public readonly int EventCodesDPointer;
    public readonly int EventCodesDSize;
    public readonly int EventCodesEPointer;
    public readonly int EventCodesESize;
    public readonly int EventCodesFPointer;
    public int EventCodesFSize;//calced when reading sector1
    public readonly int EventCodesFAndRemainingSize;
}