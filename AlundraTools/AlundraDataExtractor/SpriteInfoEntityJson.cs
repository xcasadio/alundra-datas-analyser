using AlundraEngine;
using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record SpriteInfoEntityJson
{
    public byte XMin { get; set; }
    public byte YMin { get; set; }
    public byte XMax { get; set; }
    public byte YMax { get; set; }
    public byte IsEnabled { get; set; }
    public byte SpriteDirection { get; set; }
    public byte SpriteTableIndex { get; set; }
    public byte XPos { get; set; }
    public byte YPos { get; set; }
    public byte Height { get; set; }
    public byte EventCodesA_LoadIndex { get; set; }
    public byte EventCodesB_MapIndex { get; set; }
    public byte EventCodesC_TickIndex { get; set; }
    public byte EventCodesD_TouchIndex { get; set; }
    public byte EventCodesE_DeactivateIndex { get; set; }
    public byte EventCodesF_InteractIndex { get; set; }
    public ushort _10 { get; set; }
    public ushort Contents { get; set; }
    public string? Name { get; set; }

    public SpriteInfoEntityJson(SiEntityRecord EntityRecord)
    {
        XMin = EntityRecord.XMin;
        YMin = EntityRecord.YMin;
        XMax = EntityRecord.XMax;
        YMax = EntityRecord.YMax;
        IsEnabled = EntityRecord.IsEnabled;
        SpriteDirection = EntityRecord.SpriteDirection;
        SpriteTableIndex = EntityRecord.SpriteTableIndex;
        XPos = EntityRecord.XPos;
        YPos = EntityRecord.YPos;
        Height = EntityRecord.Height;
        EventCodesA_LoadIndex = EntityRecord.EventCodesA_LoadIndex;
        EventCodesB_MapIndex = EntityRecord.EventCodesB_MapIndex;
        EventCodesC_TickIndex = EntityRecord.EventCodesC_TickIndex;
        EventCodesD_TouchIndex = EntityRecord.EventCodesD_TouchIndex;
        EventCodesE_DeactivateIndex = EntityRecord.EventCodesE_DeactivateIndex;
        EventCodesF_InteractIndex = EntityRecord.EventCodesF_InteractIndex;
        _10 = EntityRecord._10;
        Contents = EntityRecord.Contents;

        Name = EntityNames.GetName(SpriteDirection, SpriteTableIndex);
    }
}