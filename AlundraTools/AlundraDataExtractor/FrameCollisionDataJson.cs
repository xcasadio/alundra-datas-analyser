using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record FrameCollisionDataJson
{
    public sbyte OffsetX { get; set; }
    public sbyte OffsetY { get; set; }
    public sbyte OffsetZ { get; set; }
    public byte Width { get; set; }
    public byte Depth { get; set; }
    public byte Height { get; set; }

    public FrameCollisionDataJson(FrameCollisionData frameCollisionData)
    {
        OffsetX = frameCollisionData.OffsetX;
        OffsetY = frameCollisionData.OffsetY;
        OffsetZ = frameCollisionData.OffsetZ;
        Width = frameCollisionData.Width;
        Depth = frameCollisionData.Depth;
        Height = frameCollisionData.Height;
    }
}