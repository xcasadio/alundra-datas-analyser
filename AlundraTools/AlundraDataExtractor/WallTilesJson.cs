using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record WallTilesJson
{
    public sbyte Offset { get; set; }
    public byte Count { get; set; }
    public ushort[] Tiles { get; set; }

    public WallTilesJson(WallTiles wallTiles)
    {
        Offset = wallTiles.Offset;
        Count = wallTiles.Count;
        Tiles = wallTiles.Tiles;
    }
}