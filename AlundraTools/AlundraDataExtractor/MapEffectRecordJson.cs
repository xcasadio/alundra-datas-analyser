using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record MapEffectRecordJson
{
    public byte X1 { get; set; }
    public byte X2 { get; set; }
    public byte Y1 { get; set; }
    public byte Y2 { get; set; }
    public byte Flags { get; set; }
    public byte EffectId { get; set; }
    public byte X { get; set; }
    public byte Y { get; set; }
    public byte Z { get; set; }
    public byte AnimId { get; set; }
    public byte U1 { get; set; }
    public byte U2 { get; set; }

    public MapEffectRecordJson(MapEffectRecord mapEffectRecord)
    {
        X1 = mapEffectRecord.X1;
        X2 = mapEffectRecord.X2;
        Y1 = mapEffectRecord.Y1;
        Y2 = mapEffectRecord.Y2;
        Flags = mapEffectRecord.Flags;
        EffectId = mapEffectRecord.EffectId;
        X = mapEffectRecord.X;
        Y = mapEffectRecord.Y;
        Z = mapEffectRecord.Z;
        AnimId = mapEffectRecord.AnimId;
        U1 = mapEffectRecord.U1;
        U2 = mapEffectRecord.U2;
    }
}