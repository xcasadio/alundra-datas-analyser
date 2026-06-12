using AlundraEngine;

namespace AlundraDataExtractor;

internal static class GameMapTilesheetLayout
{
    public const int OriginalColumns = 10;
    public const int OriginalImageWidth = 256;
    public const int OriginalImageHeight = 256 * 6;
    public const int OriginalTileCount = OriginalColumns * (OriginalImageHeight / StaticVariables.MapTileHeight);

    public static bool TryGetOriginalLocalTileId(ushort rawTileId, out int localTileId)
    {
        localTileId = rawTileId & 0x3ff;
        return localTileId >= 0 && localTileId < OriginalTileCount;
    }

    public static int GetTileX(int localTileId)
    {
        return localTileId % OriginalColumns * StaticVariables.MapTileWidth;
    }

    public static int GetTileY(int localTileId)
    {
        return localTileId / OriginalColumns * StaticVariables.MapTileHeight;
    }
}