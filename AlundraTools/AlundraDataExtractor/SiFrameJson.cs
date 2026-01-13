using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record SiFrameJson
{
    public int MemoryAddress { get; set; }
    public byte Delay { get; set; }
    public byte TransformIndexLow { get; set; }
    public byte TransformIndexHigh { get; set; }
    public byte SpriteIndexLow { get; set; }
    public byte SpriteIndexHigh { get; set; }
    public FrameCollisionDataJson CollisionData { get; set; }
    public SiImageSetJson Images { get; set; }

    public SiFrameJson(SiFrame siFrame)
    {
        MemoryAddress = siFrame.MemoryAddress;
        Delay = siFrame.Delay;
        TransformIndexLow = siFrame.TransformIndexLow;
        TransformIndexHigh = siFrame.TransformIndexHigh;
        SpriteIndexLow = siFrame.SpriteIndexLow;
        SpriteIndexHigh = siFrame.SpriteIndexHigh;

        if (siFrame.CollisionData != null)
        {
            CollisionData = new FrameCollisionDataJson(siFrame.CollisionData);
        }

        if (siFrame.Images != null)
        {
            Images = new SiImageSetJson(siFrame.Images);
        }
    }
}