namespace AlundraEngine.Sprite;

public class SiFrame
{
    public SiFrame(BinaryReader br, SiAnimationSetHeader header)
    {
        Delay = br.ReadByte();
        Unknown = br.ReadInt16();
        Imagesetpointer = br.ReadInt16() * 2;


        //load images
        var savepos = br.BaseStream.Position;

        br.BaseStream.Position = header.Binoffset + header.Framespointer + Imagesetpointer;
        Images = new SiImageSet(br);

        br.BaseStream.Position = savepos;
    }



    public byte Delay;//top bit masked
    public short Unknown;//-1
    public readonly int Imagesetpointer;
    public SiImageSet Images;
}