namespace Alundra.DatasBin;

public class FrameCollisionData
{
    public FrameCollisionData(BinaryReader br)
    {
        XOff = br.ReadByte();
        YOff = br.ReadByte();
        ZOff = br.ReadByte();
        Width = br.ReadByte();
        Depth = br.ReadByte();
        Height = br.ReadByte();
    }
    public readonly byte XOff;
    public readonly byte YOff;
    public readonly byte ZOff;
    public readonly byte Width;
    public readonly byte Depth;
    public readonly byte Height;
}