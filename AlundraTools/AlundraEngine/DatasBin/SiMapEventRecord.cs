namespace AlundraEngine.DatasBin;

public class SiMapEventRecord
{
    public SiMapEventRecord(BinaryReader br)
    {
        X1 = br.ReadByte();
        Y1 = br.ReadByte();
        X2 = br.ReadByte();
        Y2 = br.ReadByte();
        EventCodesBIndex = br.ReadByte();
        Ub1 = br.ReadByte();
        Ub2 = br.ReadByte();
        Ub3 = br.ReadByte();
    }

    public readonly byte X1;
    public readonly byte Y1;
    public readonly byte X2;
    public readonly byte Y2;
    public readonly byte EventCodesBIndex;
    public readonly byte Ub1;
    public readonly byte Ub2;
    public readonly byte Ub3;
}