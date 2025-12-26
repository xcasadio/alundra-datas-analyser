namespace AlundraEngine.DatasBin;

public class FrameCollisionData
{
    public readonly sbyte OffsetX;
    public readonly sbyte OffsetY;
    public readonly sbyte OffsetZ;
    public readonly byte Width;
    public readonly byte Depth;
    public readonly byte Height;

    public FrameCollisionData(BinaryReader br)
    {
        OffsetX = br.ReadSByte();
        OffsetY = br.ReadSByte();
        OffsetZ = br.ReadSByte();
        Width = br.ReadByte();
        Depth = br.ReadByte();
        Height = br.ReadByte();
    }

    public override string ToString()
    {
        return $"Offset: ({OffsetX}, {OffsetY}, {OffsetZ}), Size: ({Width}, {Height}, {Depth})";
    }
}