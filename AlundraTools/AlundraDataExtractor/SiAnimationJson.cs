using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record SiAnimationJson
{
    public int MemoryAddress { get; set; }
    public int NumberOfFrames { get; set; }
    public SiFrameJson[] Frames { get; set; }

    public SiAnimationJson(SiAnimation siAnimation)
    {
        MemoryAddress = siAnimation.MemoryAddress;
        NumberOfFrames = siAnimation.NumberOfFrames;

        if (NumberOfFrames > 0)
        {
            Frames = new SiFrameJson[NumberOfFrames];
            for (int i = 0; i < NumberOfFrames; i++)
            {
                Frames[i] = new SiFrameJson(siAnimation.Frames[i]);
            }
        }
    }
}