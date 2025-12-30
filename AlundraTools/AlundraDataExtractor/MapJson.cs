using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record MapJson
{
    public int Width { get; set; }
    public int Height { get; set; }
    public MapCopyJson[] MapCopies { get; set; } = new MapCopyJson[256];
    public MapTileJson[] MapTiles { get; set; }

    public MapJson()
    {
    }

    public MapJson(Map map)
    {
        Width = map.Width;
        Height = map.Height;
        MapCopies = map.MapCopies.Select(mc => new MapCopyJson(mc)).ToArray();
        MapTiles = map.MapTiles.Select(mt => new MapTileJson(mt)).ToArray();
    }
}