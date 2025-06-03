namespace AlundraEngine.DatasBin;

public class SiEffectFrame
{
    public SiEffectFrame(BinaryReader br, int effectid, int binoffset, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        Delay = br.ReadByte();
        ImageSetPointer = br.ReadUInt16() * 2;

        //load images
        var savepos = br.BaseStream.Position;

        br.BaseStream.Position = binoffset + ImageSetPointer;
        Images = new SiImageSet(br, effectid << 16 | ImageSetPointer, memoryAddress + ImageSetPointer);

        br.BaseStream.Position = savepos;
    }

    public int MemoryAddress;
    public readonly byte Delay;//top bit masked
    public short Unknown;//-1
    public readonly int ImageSetPointer;
    public SiImageSet Images;
}