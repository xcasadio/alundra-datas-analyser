using AlundraEngine;
using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

public static class TiledMapExporter
{
    private const ushort EmptyTileId = 0xffff;

    public static void ExportMap(GameMap gameMap, int mapId, string extractionPath, TileAnimDescriptor[] tileAnimDescriptors)
    {
        _ = mapId;
        _ = tileAnimDescriptors;

        var tiledPath = Path.Combine(extractionPath, "tiled");
        Directory.CreateDirectory(tiledPath);

        _ = CreateTileCatalog(gameMap);
    }

    public static TiledTileCatalog CreateTileCatalog(GameMap gameMap)
    {
        return TiledTileCatalog.Create(EnumerateRawTileIds(gameMap));
    }

    private static IEnumerable<ushort> EnumerateRawTileIds(GameMap gameMap)
    {
        foreach (var mapTile in gameMap.Map.MapTiles)
        {
            if (mapTile.TileId != EmptyTileId)
            {
                yield return mapTile.TileId;
            }

            if (mapTile.WallTiles?.Tiles == null)
            {
                continue;
            }

            foreach (var wallTileId in mapTile.WallTiles.Tiles)
            {
                if (wallTileId != EmptyTileId)
                {
                    yield return wallTileId;
                }
            }
        }
    }
}

public sealed class TiledTileCatalog
{
    private readonly Dictionary<ushort, TiledTileCatalogEntry> _entriesByRawTileId;

    private TiledTileCatalog(TiledTileCatalogEntry[] entries)
    {
        Entries = entries;
        _entriesByRawTileId = entries.ToDictionary(entry => entry.RawTileId);
    }

    public IReadOnlyList<TiledTileCatalogEntry> Entries { get; }

    public static TiledTileCatalog Create(IEnumerable<ushort> rawTileIds)
    {
        var entries = rawTileIds
            .Where(rawTileId => rawTileId != 0xffff)
            .Distinct()
            .OrderBy(rawTileId => rawTileId)
            .Select((rawTileId, index) => new TiledTileCatalogEntry(
                rawTileId,
                index,
                index + 1,
                (rawTileId & 0xf000) >> 12,
                rawTileId & 0x3ff))
            .ToArray();

        return new TiledTileCatalog(entries);
    }

    public int GetGidOrEmpty(ushort rawTileId)
    {
        return rawTileId == 0xffff ? 0 : _entriesByRawTileId[rawTileId].Gid;
    }
}

public sealed record TiledTileCatalogEntry(
    ushort RawTileId,
    int LocalTileId,
    int Gid,
    int Palette,
    int Tile);