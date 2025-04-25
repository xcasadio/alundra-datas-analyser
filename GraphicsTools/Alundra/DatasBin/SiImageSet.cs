namespace Alundra.DatasBin;

public class SiImageSet
{
    public SiImageSet(BinaryReader br, int imagesetid, int memaddr, bool isportrait = false)
    {
        Memaddr = memaddr;
        Imagesetid = imagesetid;
        Unknown = br.ReadByte();//palette?
        NumberOfImages = br.ReadByte();
        if (isportrait)
        {
            NumberOfImages = 1;
        }

        Images = new SiImage[NumberOfImages];
        for (var dex = 0; dex < NumberOfImages; dex++)
        {
            Images[dex] = new SiImage(br);
        }
    }

    public readonly int Memaddr;
    public readonly int Imagesetid;
    public readonly byte Unknown;
    public readonly byte NumberOfImages;
    public readonly SiImage[] Images;
}