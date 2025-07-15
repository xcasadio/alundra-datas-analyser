namespace AlundraEngine.DatasBin;

public class SiEffectFrame
{
    public SiEffectFrame(BinaryReader br, int effectId, int binoffset, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        Delay = br.ReadByte();

        var pos = br.BaseStream.Position;
        TransformIndexLow = br.ReadByte();
        br.BaseStream.Position = pos;

        ImageSetPointer = br.ReadUInt16() * 2;

        pos = br.BaseStream.Position;

        br.BaseStream.Position = binoffset + ImageSetPointer;
        Images = new SiImageSet(br, effectId << 16 | ImageSetPointer, memoryAddress + ImageSetPointer);

        br.BaseStream.Position = pos;
    }

    // Used for transition frame
    public SiEffectFrame(byte flag, byte animationId, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        Delay = flag;
        TransformIndexLow = animationId;

        ImageSetPointer = -1;
    }

    public int MemoryAddress;
    public readonly byte Delay;//top bit masked
    //public short Unknown;//-1
    public readonly int ImageSetPointer;
    public readonly byte TransformIndexLow;

    public SiImageSet Images;
}