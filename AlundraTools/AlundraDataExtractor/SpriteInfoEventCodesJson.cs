using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record SpriteInfoEventCodesJson
{
    public short[] EventCodesATable { get; set; }
    public short[] EventCodesBTable { get; set; }
    public short[] EventCodesCTable { get; set; }
    public short[] EventCodesDTable { get; set; }
    public short[] EventCodesETable { get; set; }
    public short[] EventCodesFTable { get; set; }

    public SpriteInfoEventCodesJson(SpriteInfoEventCodes spriteInfoEventCodes)
    {
        EventCodesATable = spriteInfoEventCodes.EventCodesATable;
        EventCodesBTable = spriteInfoEventCodes.EventCodesBTable;
        EventCodesCTable = spriteInfoEventCodes.EventCodesCTable;
        EventCodesDTable = spriteInfoEventCodes.EventCodesDTable;
        EventCodesETable = spriteInfoEventCodes.EventCodesETable;
        EventCodesFTable = spriteInfoEventCodes.EventCodesFTable;
    }
}