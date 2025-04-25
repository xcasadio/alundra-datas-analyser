namespace Alundra.DatasBin;

public class SiEffectFrame
{
    public SiEffectFrame(BinaryReader br, int effectid, int binoffset, int memaddr)
    {
        Memaddr = memaddr;
        Delay = br.ReadByte();
        Imagesetpointer = br.ReadUInt16() * 2;

        //load images
        var savepos = br.BaseStream.Position;

        br.BaseStream.Position = binoffset + Imagesetpointer;
        Images = new SiImageSet(br, effectid << 16 | Imagesetpointer, memaddr + Imagesetpointer);

        br.BaseStream.Position = savepos;
    }

    public int Memaddr;
    public readonly byte Delay;//top bit masked
    public short Unknown;//-1
    public readonly int Imagesetpointer;
    public SiImageSet Images;
}