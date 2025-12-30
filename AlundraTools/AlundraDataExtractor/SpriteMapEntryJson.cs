namespace AlundraDataExtractor;

record SpriteMapEntryJson
{
    public int Enabled { get; set; }
    public int NumberOfFrame { get; set; }
    public int TileHeight { get; set; }
    public int FrameDuration { get; set; }
    public int Index { get; set; }
    public int Tick { get; set; }
    public int FrameIndex { get; set; }

    public SpriteMapEntryJson()
    {
    }

    public SpriteMapEntryJson(
        int enabled,
        int numberOfFrame,
        int tileHeight,
        int frameDuration,
        int index,
        int tick,
        int frameIndex)
    {
        Enabled = enabled;
        NumberOfFrame = numberOfFrame;
        TileHeight = tileHeight;
        FrameDuration = frameDuration;
        Index = index;
        Tick = tick;
        FrameIndex = frameIndex;
    }
}