namespace AlundraEngine.DatasBin;

public class ScrollScreen
{
    public ScrollScreen(BinaryReader br)
    {
        Unknown1 = br.ReadInt32();
        Unknown2 = br.ReadInt32();
        Unknown3 = br.ReadInt32();
        Unknown4 = br.ReadInt32();
        Unknown5 = br.ReadInt32();
        Unknown6 = br.ReadInt32();
        Unknown7 = br.ReadInt32();
        Unknown8 = br.ReadInt32();
    }
    public readonly int Unknown1;
    public readonly int Unknown2;
    public readonly int Unknown3;
    public readonly int Unknown4;
    public readonly int Unknown5;
    public readonly int Unknown6;
    public readonly int Unknown7;
    public readonly int Unknown8;
}