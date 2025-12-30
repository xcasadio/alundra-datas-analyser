using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record GameMapInfoJson
{
    public int MapId { get; set; }
    public int Gravity { get; set; }
    public int ZViscosity { get; set; }
    public int SlideEffectId { get; set; }
    public int BalanceLevel { get; set; }
    public int C { get; set; }
    public int D { get; set; }
    public int E { get; set; }
    public int F { get; set; }
    public int _10 { get; set; }
    public int _11 { get; set; }
    public SpriteMapEntryJson[] SpriteMapEntries { get; set; } = new SpriteMapEntryJson[6];
    public PortalJson[] Portals { get; set; } = new PortalJson[64];

    public GameMapInfoJson()
    {
    }

    public GameMapInfoJson(GameMapInfo gameMapInfo)
    {
        MapId = (int)gameMapInfo.MapId;
        Gravity = (int)gameMapInfo.Gravity;
        ZViscosity = (int)gameMapInfo.ZViscosity;
        SlideEffectId = (int)gameMapInfo.SlideEffectId;
        BalanceLevel = (int)gameMapInfo.BalanceLevel;
        C = (int)gameMapInfo.C;
        D = (int)gameMapInfo.D;
        E = (int)gameMapInfo.E;
        F = (int)gameMapInfo.F;
        _10 = (int)gameMapInfo._10;
        _11 = (int)gameMapInfo._11;

        for (int i = 0; i < SpriteMapEntries.Length; i++)
        {
            var spriteMapEntry = gameMapInfo.SpriteMapEntries[i];
            SpriteMapEntries[i] = new SpriteMapEntryJson(
                spriteMapEntry.Enabled,
                spriteMapEntry.NumberOfFrame,
                spriteMapEntry.TileHeight,
                spriteMapEntry.FrameDuration,
                spriteMapEntry.Index,
                spriteMapEntry.Tick,
                spriteMapEntry.FrameIndex);
        }

        for (int i = 0; i < Portals.Length; i++)
        {
            var portal = gameMapInfo.Portals[i];
            Portals[i] = new PortalJson(
                portal.X1,
                portal.Y1,
                portal.X2,
                portal.Y2,
                portal.DestMapId,
                portal.DestTileX,
                portal.DestTileY,
                portal.ZLevel,
                portal.Flags);
        }
    }
}