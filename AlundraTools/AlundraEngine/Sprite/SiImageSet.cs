namespace AlundraEngine.Sprite;

public class SiImageSet
{
    public SiImageSet(BinaryReader br)
    {
        Palette = br.ReadByte();//palette?
        Numimages = br.ReadByte();
        Images = new SiImage[Numimages];
        for (var dex = 0; dex < Numimages; dex++)
        {
            Images[dex] = new SiImage(br);
        }
    }

    public byte Palette;
    public readonly byte Numimages;
    public readonly SiImage[] Images;
}