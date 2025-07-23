namespace AlundraEngine.DatasBin;

public class FrameCollisionData
{
    public FrameCollisionData(BinaryReader br)
    {
        OffsetX = br.ReadByte();
        OffsetY = br.ReadByte();
        OffsetZ = br.ReadByte();
        Width = br.ReadByte();
        Depth = br.ReadByte();
        Height = br.ReadByte();
    }
    public readonly byte OffsetX;
    public readonly byte OffsetY;
    public readonly byte OffsetZ;
    public readonly byte Width;
    public readonly byte Depth;
    public readonly byte Height;
}