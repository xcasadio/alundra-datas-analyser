using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record SiImageSetJson
{
    public int MemoryAddress { get; set; }
    public ulong ImageSetId { get; set; }
    public byte DepthSortValue { get; set; }
    public byte NumberOfImages { get; set; }
    public SiImageJson[] Images { get; set; }

    public SiImageSetJson(SiImageSet siImageSet)
    {
        MemoryAddress = siImageSet.MemoryAddress;
        ImageSetId = siImageSet.ImageSetId;
        DepthSortValue = siImageSet.DepthSortValue;
        NumberOfImages = siImageSet.NumberOfImages;

        if (siImageSet.NumberOfImages > 0)
        {
            Images = new SiImageJson[siImageSet.NumberOfImages];
            for (int i = 0; i < siImageSet.NumberOfImages; i++)
            {
                Images[i] = new SiImageJson(siImageSet.Images[i]);
            }
        }
    }
}