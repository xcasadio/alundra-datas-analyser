namespace Alundra.DatasBin;

public class SiImageSet
{
    public SiImageSet(BinaryReader br, int imageSetId, int memoryAddress, bool isportrait = false)
    {
        MemoryAddress = memoryAddress;
        ImageSetId = imageSetId;
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

    public readonly int MemoryAddress;
    public readonly int ImageSetId;
    public readonly byte Unknown;
    public readonly byte NumberOfImages;
    public readonly SiImage[] Images;
}