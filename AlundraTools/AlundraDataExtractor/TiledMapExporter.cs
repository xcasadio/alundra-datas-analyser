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
        var companionFileName = $"map_{mapId}.alundra.json";
        var tilesetLayout = SaveCompactTilesetImage(gameMap, catalog, Path.Combine(tiledPath, tilesetImageFileName));
        var tilesetJson = CreateTilesetJson(mapId, catalog, tilesetImageFileName, tilesetLayout);
        File.WriteAllText(Path.Combine(tiledPath, tilesetFileName), JsonSerializer.Serialize(tilesetJson, TiledJsonOptions));

        var companionJson = CreateCompanionJson(gameMap, mapId);
        File.WriteAllText(Path.Combine(tiledPath, companionFileName), JsonSerializer.Serialize(companionJson, TiledJsonOptions));

        var mapJson = CreateMapJson(gameMap, mapId, catalog, tilesetFileName, companionFileName);
        File.WriteAllText(Path.Combine(tiledPath, $"map_{mapId}.tmj"), JsonSerializer.Serialize(mapJson, TiledJsonOptions));
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

    private static TiledMapJson CreateMapJson(GameMap gameMap, int mapIndex, TiledTileCatalog catalog, string tilesetFileName, string companionFileName)
    {
        var groundLayer = CreateGroundLayer(gameMap, catalog, 1);

        return new TiledMapJson
        {
            Version = TiledJsonVersion,
            TiledVersion = TiledVersion,
            Width = gameMap.Map.Width,
            Height = gameMap.Map.Height,
            TileWidth = StaticVariables.MapTileWidth,
            TileHeight = StaticVariables.MapTileHeight,
            NextLayerId = 2,
            NextObjectId = 1,
            Tilesets =
            [
                new TiledMapTilesetJson
                {
                    FirstGid = 1,
                    Source = tilesetFileName.Replace('\\', '/')
                }
            ],
            Layers = [groundLayer],
            Properties =
            [
                TiledProperty.String("SourceFileName", $"map_{mapIndex}.json"),
                TiledProperty.File("SourceJson", $"../map_{mapIndex}.json"),
                TiledProperty.File("AlundraCompanionJson", companionFileName.Replace('\\', '/')),
                TiledProperty.Int("MapIndex", mapIndex),
                TiledProperty.Int("MapId", Convert.ToInt32(gameMap.Info.MapId))
            ]
        };
    }

    private static TiledLayerJson CreateGroundLayer(GameMap gameMap, TiledTileCatalog catalog, int layerId)
    {
        var map = gameMap.Map;
        var data = new int[map.MapTiles.Length];

        for (var index = 0; index < map.MapTiles.Length; index++)
        {
            data[index] = catalog.GetGidOrEmpty(map.MapTiles[index].TileId);
        }

        return new TiledLayerJson
        {
            Id = layerId,
            Name = "Ground",
            Type = "tilelayer",
            Width = map.Width,
            Height = map.Height,
            Data = data
        };
    }

    private static AlundraTiledCompanionJson CreateCompanionJson(GameMap gameMap, int mapIndex)
    {
        var map = gameMap.Map;
        var cells = new List<AlundraCellJson>(map.MapTiles.Length);

        for (var index = 0; index < map.MapTiles.Length; index++)
        {
            var tile = map.MapTiles[index];
            cells.Add(new AlundraCellJson
            {
                Index = index,
                X = index % map.Width,
                Y = index / map.Width,
                Walkability = tile.Walkability,
                GroundProperty = tile.GroundProperty,
                Slope = tile.Slope,
                Height = tile.Height,
                WallTilesOffset = tile.WallTilesOffset,
                TileId = tile.TileId,
                Palette = tile.Palette,
                Tile = tile.Tile,
                Flags = tile.Flags
            });
        }

        return new AlundraTiledCompanionJson
        {
            MapIndex = mapIndex,
            MapId = gameMap.Info.MapId,
            Width = map.Width,
            Height = map.Height,
            TileWidth = StaticVariables.MapTileWidth,
            TileHeight = StaticVariables.MapTileHeight,
            CellOrder = "y * Width + x",
            Cells = cells
        };
    }
}

public sealed class AlundraTiledCompanionJson
{
    public int MapIndex { get; init; }
    public uint MapId { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int TileWidth { get; init; }
    public int TileHeight { get; init; }
    public string CellOrder { get; init; } = "";
    public List<AlundraCellJson> Cells { get; init; } = [];
}

public sealed class AlundraCellJson
{
    public int Index { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public byte Walkability { get; init; }
    public byte GroundProperty { get; init; }
    public byte Slope { get; init; }
    public byte Height { get; init; }
    public short WallTilesOffset { get; init; }
    public ushort TileId { get; init; }
    public short Palette { get; init; }
    public short Tile { get; init; }
    public uint Flags { get; init; }
}

public sealed class TiledMapJson
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "map";

    [JsonPropertyName("version")]
    public string Version { get; init; } = "";

    [JsonPropertyName("tiledversion")]
    public string TiledVersion { get; init; } = "";

    [JsonPropertyName("orientation")]
    public string Orientation { get; init; } = "orthogonal";

    [JsonPropertyName("renderorder")]
    public string RenderOrder { get; init; } = "right-down";

    [JsonPropertyName("compressionlevel")]
    public int CompressionLevel { get; init; } = -1;

    [JsonPropertyName("width")]
    public int Width { get; init; }

    [JsonPropertyName("height")]
    public int Height { get; init; }

    [JsonPropertyName("tilewidth")]
    public int TileWidth { get; init; }

    [JsonPropertyName("tileheight")]
    public int TileHeight { get; init; }

    [JsonPropertyName("infinite")]
    public bool Infinite { get; init; }

    [JsonPropertyName("nextlayerid")]
    public int NextLayerId { get; init; }

    [JsonPropertyName("nextobjectid")]
    public int NextObjectId { get; init; }

    [JsonPropertyName("properties")]
    public List<TiledProperty> Properties { get; init; } = [];

    [JsonPropertyName("tilesets")]
    public List<TiledMapTilesetJson> Tilesets { get; init; } = [];

    [JsonPropertyName("layers")]
    public List<TiledLayerJson> Layers { get; init; } = [];
}

public sealed class TiledMapTilesetJson
{
    [JsonPropertyName("firstgid")]
    public int FirstGid { get; init; }

    [JsonPropertyName("source")]
    public string Source { get; init; } = "";
}

public sealed class TiledLayerJson
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("type")]
    public string Type { get; init; } = "";

    [JsonPropertyName("width")]
    public int Width { get; init; }

    [JsonPropertyName("height")]
    public int Height { get; init; }

    [JsonPropertyName("x")]
    public int X { get; init; }

    [JsonPropertyName("y")]
    public int Y { get; init; }

    [JsonPropertyName("opacity")]
    public double Opacity { get; init; } = 1;

    [JsonPropertyName("visible")]
    public bool Visible { get; init; } = true;

    [JsonPropertyName("data")]
    public int[]? Data { get; init; }
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

    public static TiledProperty String(string name, string value)
    {
        return new TiledProperty { Name = name, Type = "string", Value = value };
    }

    public static TiledProperty File(string name, string value)
    {
        return new TiledProperty { Name = name, Type = "file", Value = value };
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