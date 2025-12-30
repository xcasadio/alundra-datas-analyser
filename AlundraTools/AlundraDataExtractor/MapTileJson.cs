using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record MapTileJson
{
    public int Walkability { get; set; }
    public int GroundProperty { get; set; }
    public int Slope { get; set; }
    public int Height { get; set; }
    public int TileId { get; set; }
    public int Palette { get; set; }
    public int Tile { get; set; }
    public int TilesOffset { get; set; }
    public WallTilesJson? WallTiles { get; set; }

    public MapTileJson(MapTile mapTile)
    {
        Walkability = mapTile.Walkability;
        GroundProperty = mapTile.GroundProperty;
        Slope = mapTile.Slope;
        Height = mapTile.Height;
        TileId = mapTile.TileId;
        Palette = mapTile.Palette;
        Tile = mapTile.Tile;
        TilesOffset = mapTile.TilesOffset;
        WallTiles = mapTile.WallTiles != null ? new WallTilesJson(mapTile.WallTiles) : null;
    }
}