namespace AlundraEngine.DatasBin;

public class SiFrame
{
    public SiFrame(BinaryReader br, SpriteTableHeader header, int memoryAddress)
    {
        MemoryAddress = memoryAddress;

        Delay = br.ReadByte();

        //for debugging
        var pos = br.BaseStream.Position;
        TransformIndexLow = br.ReadByte();
        TransformIndexHigh = br.ReadByte();
        SpriteIndexLow = br.ReadByte();
        SpriteIndexHigh = br.ReadByte();
        br.BaseStream.Position = pos;
        //

        CollisionOffset = br.ReadInt16();
        ImageSetPointer = br.ReadUInt16() * 2;

        var streamPosition = br.BaseStream.Position;

        br.BaseStream.Position = header.BinOffset + header.FramesPointer + ImageSetPointer;
        Images = new SiImageSet(br, header.Sector5Id << 16 | ImageSetPointer, header.SpriteInfoMemoryAddress + header.FramesPointer + ImageSetPointer);

        if (CollisionOffset != -1)
        {
            br.BaseStream.Position = header.BinOffset + header.FrameCollisionPointer + CollisionOffset;
            CollisionData = new FrameCollisionData(br);
        }

        br.BaseStream.Position = streamPosition;
    }

    // Used for transition frame
    public SiFrame(byte flag, byte animationId, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        Delay = flag;
        TransformIndexLow = animationId;

        CollisionOffset = -1;
        ImageSetPointer = -1;
    }

    public readonly byte Delay;//top bit masked
    public readonly short CollisionOffset;//-1
    public readonly int ImageSetPointer;

    public readonly byte TransformIndexLow;
    public readonly byte TransformIndexHigh;
    public readonly byte SpriteIndexLow;
    public readonly byte SpriteIndexHigh;

    public readonly FrameCollisionData CollisionData;
    public readonly int MemoryAddress;
    public SiImageSet Images;
}