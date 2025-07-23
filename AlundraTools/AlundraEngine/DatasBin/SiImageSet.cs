namespace AlundraEngine.DatasBin;

public class SiImageSet
{
    public SiImageSet(BinaryReader br, int imageSetId, int memoryAddress, bool isportrait = false)
    {
        MemoryAddress = memoryAddress;
        ImageSetId = imageSetId;
        DepthSortValue = br.ReadByte();
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
    public readonly byte DepthSortValue;
    public readonly byte NumberOfImages;
    public readonly SiImage[] Images;
}