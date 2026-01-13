namespace AlundraEngine.DatasBin;

public class SiImageSet
{
    public readonly int MemoryAddress;
    public readonly ulong ImageSetId;
    public readonly byte DepthSortValue;
    public readonly byte NumberOfImages;
    public readonly SiImage[] Images;

    public SiImageSet(BinaryReader br, ulong imageSetId, int memoryAddress, bool isPortrait = false)
    {
        MemoryAddress = memoryAddress;
        ImageSetId = imageSetId;
        DepthSortValue = br.ReadByte();
        NumberOfImages = br.ReadByte();

        if (isPortrait)
        {
            NumberOfImages = 1;
        }

        Images = new SiImage[NumberOfImages];

        for (var i = 0; i < NumberOfImages; i++)
        {
            Images[i] = new SiImage(br);
        }
    }
}