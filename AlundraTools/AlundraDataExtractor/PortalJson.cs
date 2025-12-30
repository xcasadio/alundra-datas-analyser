namespace AlundraDataExtractor;

record PortalJson
{
    public int X1 { get; set; }
    public int Y1 { get; set; }
    public int X2 { get; set; }
    public int Y2 { get; set; }
    public int DestMapId { get; set; }
    public int DestTileX { get; set; }
    public int DestTileY { get; set; }
    public int ZLevel { get; set; }
    public int Flags { get; set; }

    public PortalJson()
    {
    }

    public PortalJson(
        int x1,
        int y1,
        int x2,
        int y2,
        int destMapId,
        int destTileX,
        int destTileY,
        int zLevel,
        int flags)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
        DestMapId = destMapId;
        DestTileX = destTileX;
        DestTileY = destTileY;
        ZLevel = zLevel;
        Flags = flags;
    }
}