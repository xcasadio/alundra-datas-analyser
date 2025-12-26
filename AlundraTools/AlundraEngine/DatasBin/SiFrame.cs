using System.Diagnostics;

namespace AlundraEngine.DatasBin;

public class SiFrame
{
    public readonly int MemoryAddress;
    public readonly byte Delay;
    public readonly short CollisionOffset;//-1
    public readonly int ImageSetPointer;

    public readonly byte TransformIndexLow;
    public readonly byte TransformIndexHigh;
    public readonly byte SpriteIndexLow;
    public readonly byte SpriteIndexHigh;

    public readonly FrameCollisionData CollisionData;
    public SiImageSet Images;

    public SiFrame(BinaryReader br, SpriteTableHeader header, int memoryAddress)
    {
        MemoryAddress = memoryAddress;

        Delay = br.ReadByte();

        TransformIndexLow = br.ReadByte();
        TransformIndexHigh = br.ReadByte();
        SpriteIndexLow = br.ReadByte();
        SpriteIndexHigh = br.ReadByte();

        CollisionOffset = (short)((TransformIndexHigh << 8) | TransformIndexLow);
        ImageSetPointer = ((SpriteIndexHigh << 8) | SpriteIndexLow) * 2;

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
}