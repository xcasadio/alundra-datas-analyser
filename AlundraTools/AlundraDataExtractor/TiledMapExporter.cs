using AlundraEngine;
using AlundraEngine.DatasBin;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlundraDataExtractor;

public static class TiledMapExporter
{
    private const ushort EmptyTileId = 0xffff;
    private const int TilesetColumns = 16;
    private const string TiledJsonVersion = "1.10";
    private const string TiledVersion = "1.10";
    private static readonly JsonSerializerOptions TiledJsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static void ExportMap(GameMap gameMap, int mapId, string extractionPath, TileAnimDescriptor[] tileAnimDescriptors)
    {
        _ = tileAnimDescriptors;

        var tiledPath = Path.Combine(extractionPath, "tiled");
        Directory.CreateDirectory(tiledPath);

        var catalog = CreateTileCatalog(gameMap);
        var tilesetImageFileName = $"map_{mapId}_tileset.png";
        var tilesetFileName = $"map_{mapId}_tileset.tsj";
        var tilesetLayout = SaveCompactTilesetImage(gameMap, catalog, Path.Combine(tiledPath, tilesetImageFileName));
        var tilesetJson = CreateTilesetJson(mapId, catalog, tilesetImageFileName, tilesetLayout);
        File.WriteAllText(Path.Combine(tiledPath, tilesetFileName), JsonSerializer.Serialize(tilesetJson, TiledJsonOptions));
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

    private static TiledTilesetLayout SaveCompactTilesetImage(GameMap gameMap, TiledTileCatalog catalog, string fileName)
    {
        var tileCount = catalog.Entries.Count;
        var columns = tileCount == 0 ? 1 : Math.Min(TilesetColumns, tileCount);
        var rows = Math.Max(1, (tileCount + columns - 1) / columns);
        var imageWidth = columns * StaticVariables.MapTileWidth;
        var imageHeight = rows * StaticVariables.MapTileHeight;

        using var bitmap = new Bitmap(imageWidth, imageHeight);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);

        foreach (var entry in catalog.Entries)
        {
            var x = entry.LocalTileId % columns * StaticVariables.MapTileWidth;
            var y = entry.LocalTileId / columns * StaticVariables.MapTileHeight;
            graphics.DrawImage(gameMap.GetTileBitmap(entry.RawTileId), x, y, StaticVariables.MapTileWidth, StaticVariables.MapTileHeight);
        }

        bitmap.Save(fileName, ImageFormat.Png);
        return new TiledTilesetLayout(columns, tileCount, imageWidth, imageHeight);
    }

    private static TiledTilesetJson CreateTilesetJson(int mapId, TiledTileCatalog catalog, string imageFileName, TiledTilesetLayout layout)
    {
        return new TiledTilesetJson
        {
            Version = TiledJsonVersion,
            TiledVersion = TiledVersion,
            Name = $"map_{mapId}_tileset",
            TileWidth = StaticVariables.MapTileWidth,
            TileHeight = StaticVariables.MapTileHeight,
            Columns = layout.Columns,
            TileCount = layout.TileCount,
            Image = imageFileName.Replace('\\', '/'),
            ImageWidth = layout.ImageWidth,
            ImageHeight = layout.ImageHeight,
            Tiles = catalog.Entries
                .Select(entry => new TiledTileJson
                {
                    Id = entry.LocalTileId,
                    Properties =
                    [
                        TiledProperty.Int("TileId", entry.RawTileId),
                        TiledProperty.Int("Palette", entry.Palette),
                        TiledProperty.Int("Tile", entry.Tile)
                    ]
                })
                .ToList()
        };
    }
}

public sealed record TiledTilesetLayout(int Columns, int TileCount, int ImageWidth, int ImageHeight);

public sealed class TiledTilesetJson
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "tileset";

    [JsonPropertyName("version")]
    public string Version { get; init; } = "";

    [JsonPropertyName("tiledversion")]
    public string TiledVersion { get; init; } = "";

    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("tilewidth")]
    public int TileWidth { get; init; }

    [JsonPropertyName("tileheight")]
    public int TileHeight { get; init; }

    [JsonPropertyName("spacing")]
    public int Spacing { get; init; }

    [JsonPropertyName("margin")]
    public int Margin { get; init; }

    [JsonPropertyName("columns")]
    public int Columns { get; init; }

    [JsonPropertyName("tilecount")]
    public int TileCount { get; init; }

    [JsonPropertyName("image")]
    public string Image { get; init; } = "";

    [JsonPropertyName("imagewidth")]
    public int ImageWidth { get; init; }

    [JsonPropertyName("imageheight")]
    public int ImageHeight { get; init; }

    [JsonPropertyName("tiles")]
    public List<TiledTileJson> Tiles { get; init; } = [];
}

public sealed class TiledTileJson
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("properties")]
    public List<TiledProperty> Properties { get; init; } = [];
}

public sealed class TiledProperty
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("type")]
    public string Type { get; init; } = "";

    [JsonPropertyName("value")]
    public object Value { get; init; } = "";

    public static TiledProperty Int(string name, int value)
    {
        return new TiledProperty { Name = name, Type = "int", Value = value };
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