using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record SpriteRecordHeaderJson
{
    public byte MoreFlags { get; set; }
    public byte CanPickup { get; set; }
    public byte FlagsPortraitShadowType { get; set; }
    public byte ProgramLoad { get; set; }
    public byte ProgramTick { get; set; }
    public byte ProgramTouch { get; set; }
    public byte ProgramDeactivate { get; set; }
    public byte ProgramInteract { get; set; }
    public sbyte OffsetX { get; set; }
    public sbyte OffsetY { get; set; }
    public sbyte OffsetZ { get; set; }
    public byte SizeX { get; set; }
    public byte SizeY { get; set; }
    public byte SizeZ { get; set; }
    public byte BreakEffect { get; set; }
    public byte Contents { get; set; }

    public SpriteRecordHeaderJson(SpriteTableHeader spriteRecordHeader)
    {
        MoreFlags = spriteRecordHeader.MoreFlags;
        CanPickup = spriteRecordHeader.CanPickup;
        FlagsPortraitShadowType = spriteRecordHeader.FlagsPortraitShadowType;
        ProgramLoad = spriteRecordHeader.ProgramLoad;
        ProgramTick = spriteRecordHeader.ProgramTick;
        ProgramTouch = spriteRecordHeader.ProgramTouch;
        ProgramDeactivate = spriteRecordHeader.ProgramDeactivate;
        ProgramInteract = spriteRecordHeader.ProgramInteract;
        OffsetX = spriteRecordHeader.OffsetX;
        OffsetY = spriteRecordHeader.OffsetY;
        OffsetZ = spriteRecordHeader.OffsetZ;
        SizeX = spriteRecordHeader.SizeX;
        SizeY = spriteRecordHeader.SizeY;
        SizeZ = spriteRecordHeader.SizeZ;
        BreakEffect = spriteRecordHeader.BreakEffect;
        Contents = spriteRecordHeader.Contents;
    }
}