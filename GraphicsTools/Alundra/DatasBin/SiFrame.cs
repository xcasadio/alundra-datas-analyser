namespace Alundra.DatasBin;

public class SiFrame
{
    public SiFrame(BinaryReader br, SpriteTableHeader header, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        Delay = br.ReadByte();

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

    public readonly byte Delay;//top bit masked
    public readonly short CollisionOffset;//-1
    public readonly int ImageSetPointer;

    public readonly FrameCollisionData CollisionData;
    public readonly int MemoryAddress;
    public SiImageSet Images;
}