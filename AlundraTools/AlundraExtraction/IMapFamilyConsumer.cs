using AlundraEngine.DatasBin;
using AlundraDataExtractor;

namespace AlundraExtraction;

/// <summary>
/// Receives, in the order <see cref="MapExtraction.ProcessAllMaps"/> computes them, the results of family 4 (map_alundra and
/// the 483 maps). Each item is handed over at the point where the default extraction writes it, so a consumer that
/// serializes the <see cref="GameMap"/> in its callback sees the AtlasX/AtlasY stamped by the packing that precedes it.
/// </summary>
public interface IMapFamilyConsumer
{
    /// <summary>map_alundra: sprite sheet PNG (data/map_alundra_spritesheet.png).</summary>
    void AlundraMapSpriteSheet(byte[] png);

    /// <summary>map_alundra: effect sheet PNG, or null when it has no effect quad (data/map_alundra_effectsheet.png). Always called, after the sprite sheet.</summary>
    void AlundraMapEffectSheet(byte[]? png);

    /// <summary>map_alundra: the game map, once the packing has stamped AtlasX/AtlasY (data/map_alundra.json is serialized here).</summary>
    void AlundraMap(GameMap gameMap);

    /// <summary>Map <paramref name="id"/>: tile sheet PNG (data/map_{id}_tilesheet.png).</summary>
    void MapTileSheet(int id, byte[] png);

    /// <summary>Map <paramref name="id"/>: sprite sheet PNG (data/map_{id}_spritesheet.png).</summary>
    void MapSpriteSheet(int id, byte[] png);

    /// <summary>Map <paramref name="id"/>: effect sheet PNG, or null when it has no effect quad (data/map_{id}_effectsheet.png). Always called, after the sprite sheet.</summary>
    void MapEffectSheet(int id, byte[]? png);

    /// <summary>Map <paramref name="id"/>: the game map, once the packing has stamped AtlasX/AtlasY (data/map_{id}.json is serialized here).</summary>
    void Map(int id, GameMap gameMap);

    /// <summary>
    /// Map <paramref name="id"/>: the Tiled export (data/tiled/ files). The result is disposed by the caller as soon as this
    /// returns: do not keep its <see cref="TiledMapResult.TilesetImage"/> beyond the call.
    /// </summary>
    void TiledMap(int id, GameMap gameMap, TiledMapResult tiled);
}

/// <summary>Writes into an <see cref="IExtractionSink"/> exactly what the default extraction has always written for family 4.</summary>
public sealed class SinkMapFamilyConsumer : IMapFamilyConsumer
{
    private readonly IExtractionSink _sink;

    public SinkMapFamilyConsumer(IExtractionSink sink) => _sink = sink;

    public void AlundraMapSpriteSheet(byte[] png) => _sink.WriteBytes("data/map_alundra_spritesheet.png", png);

    public void AlundraMapEffectSheet(byte[]? png)
    {
        if (png != null)
        {
            _sink.WriteBytes("data/map_alundra_effectsheet.png", png);
        }
    }

    public void AlundraMap(GameMap gameMap) => _sink.WriteBytes("data/map_alundra.json", ExtractionJson.Serialize(gameMap));

    public void MapTileSheet(int id, byte[] png) => _sink.WriteBytes($"data/map_{id}_tilesheet.png", png);

    public void MapSpriteSheet(int id, byte[] png) => _sink.WriteBytes($"data/map_{id}_spritesheet.png", png);

    public void MapEffectSheet(int id, byte[]? png)
    {
        if (png != null)
        {
            _sink.WriteBytes($"data/map_{id}_effectsheet.png", png);
        }
    }

    public void Map(int id, GameMap gameMap) => _sink.WriteBytes($"data/map_{id}.json", ExtractionJson.Serialize(gameMap));

    public void TiledMap(int id, GameMap gameMap, TiledMapResult tiled)
    {
        _sink.WriteBytes($"data/tiled/{tiled.TilesetImageFileName}", ExtractionImages.EncodePng(tiled.TilesetImage));
        _sink.WriteBytes($"data/tiled/{tiled.TilesetFileName}", TiledMapExporter.Serialize(tiled.Tileset));
        _sink.WriteBytes($"data/tiled/{tiled.CompanionFileName}", TiledMapExporter.Serialize(tiled.Companion));
        _sink.WriteBytes($"data/tiled/{tiled.MapFileName}", TiledMapExporter.Serialize(tiled.Map));
    }
}
