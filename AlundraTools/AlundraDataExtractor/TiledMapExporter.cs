using AlundraEngine;
using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

public static class TiledMapExporter
{
    public static void ExportMap(GameMap gameMap, int mapId, string extractionPath, TileAnimDescriptor[] tileAnimDescriptors)
    {
        _ = gameMap;
        _ = mapId;
        _ = tileAnimDescriptors;

        var tiledPath = Path.Combine(extractionPath, "tiled");
        Directory.CreateDirectory(tiledPath);
    }
}