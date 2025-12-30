using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record MapEventRecordJson
{
    public byte X1 { get; set; }
    public byte Y1 { get; set; }
    public byte X2 { get; set; }
    public byte Y2 { get; set; }
    public byte EventCodesBIndex { get; set; }
    public byte Ub1 { get; set; }
    public byte Ub2 { get; set; }
    public byte Ub3 { get; set; }

    public MapEventRecordJson(SiMapEventRecord mapEventRecord)
    {
        X1 = mapEventRecord.X1;
        Y1 = mapEventRecord.Y1;
        X2 = mapEventRecord.X2;
        Y2 = mapEventRecord.Y2;
        EventCodesBIndex = mapEventRecord.EventCodesBIndex;
        Ub1 = mapEventRecord.Ub1;
        Ub2 = mapEventRecord.Ub2;
        Ub3 = mapEventRecord.Ub3;
    }
}