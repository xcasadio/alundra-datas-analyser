using AlundraEngine.DatasBin;
using AlundraDataExtractor;

namespace AlundraExtraction;

/// <summary>
/// Sprite sheets, effect sheets and the map_alundra.json that carries their atlas positions. Packing stamps AtlasX/AtlasY
/// on the analyser objects, so it must run before the map is serialized (WriteAlundraMap does it in that order).
/// </summary>
public static class SpriteExtraction
{
    /// <summary>Packs the map's sprite sheet and stamps AtlasX/AtlasY; returns the PNG bytes.</summary>
    public static byte[] ComputeSpriteSheetPng(GameMap gameMap, SpriteSheetLayoutMode layoutMode)
    {
        using var sheet = GameMapHelper.ComputeSpriteSheet(gameMap, layoutMode);
        return ExtractionImages.EncodePng(sheet);
    }

    /// <summary>
    /// Packs the map's sprite sheet and stamps AtlasX/AtlasY now (the same packing, at the same point, as <see cref="ComputeSpriteSheetPng"/>),
    /// but neither draws nor encodes it: the result draws and encodes it when <see cref="DeferredSpriteSheet.EncodePng"/> is called.
    /// </summary>
    public static DeferredSpriteSheet PackSpriteSheet(GameMap gameMap, SpriteSheetLayoutMode layoutMode)
    {
        var layout = GameMapHelper.PackSpriteSheet(gameMap, layoutMode);
        return new DeferredSpriteSheet(() =>
        {
            using var sheet = GameMapHelper.DrawSpriteSheet(gameMap, layout);
            return ExtractionImages.EncodePng(sheet);
        });
    }

    /// <summary>Packs the map's effect sheet and stamps AtlasX/AtlasY; returns the PNG bytes, or null when the map has no effect quad.</summary>
    public static byte[]? ComputeEffectSheetPng(GameMap gameMap)
    {
        using var sheet = GameMapHelper.ComputeEffectSheet(gameMap);
        return sheet == null ? null : ExtractionImages.EncodePng(sheet);
    }

    /// <summary>Writes data/map_alundra_spritesheet.png, data/map_alundra_effectsheet.png (if any) and data/map_alundra.json, in that order.</summary>
    public static void WriteAlundraMap(GameMap gameMap, SpriteSheetLayoutMode layoutMode, IExtractionSink sink)
        => ProcessAlundraMap(gameMap, layoutMode, new SinkMapFamilyConsumer(sink));

    /// <summary>Packs map_alundra's sheets and hands them, then the stamped map, to the consumer, in that order.</summary>
    public static void ProcessAlundraMap(GameMap gameMap, SpriteSheetLayoutMode layoutMode, IMapFamilyConsumer consumer)
    {
        // Must run before the map is handed over: packing populates SiImage.AtlasX/AtlasY, which the
        // serialized map_alundra.json needs to carry.
        if (consumer.WantsSpriteSheetPngs)
        {
            consumer.AlundraMapSpriteSheet(ComputeSpriteSheetPng(gameMap, layoutMode));
        }
        else
        {
            consumer.AlundraMapSpriteSheetDeferred(PackSpriteSheet(gameMap, layoutMode));
        }

        consumer.AlundraMapEffectSheet(ComputeEffectSheetPng(gameMap));
        consumer.AlundraMap(gameMap);
    }

    /// <summary>Writes data/map_{id}_spritesheet.png and data/map_{id}_effectsheet.png (if any); call it before serializing the map.</summary>
    public static void WriteMapSheets(GameMap gameMap, int id, SpriteSheetLayoutMode layoutMode, IExtractionSink sink)
        => ProcessMapSheets(gameMap, id, layoutMode, new SinkMapFamilyConsumer(sink));

    /// <summary>Packs the map's sprite and effect sheets (stamping AtlasX/AtlasY) and hands them to the consumer, in that order.</summary>
    public static void ProcessMapSheets(GameMap gameMap, int id, SpriteSheetLayoutMode layoutMode, IMapFamilyConsumer consumer)
    {
        if (consumer.WantsSpriteSheetPngs)
        {
            consumer.MapSpriteSheet(id, ComputeSpriteSheetPng(gameMap, layoutMode));
        }
        else
        {
            consumer.MapSpriteSheetDeferred(id, PackSpriteSheet(gameMap, layoutMode));
        }

        consumer.MapEffectSheet(id, ComputeEffectSheetPng(gameMap));
    }
}

/// <summary>
/// A sprite sheet whose packing (AtlasX/AtlasY) is done and whose drawing and PNG encoding are not: <see cref="EncodePng"/> does
/// them, with the extractor's own drawing and encoder, so the bytes equal what <see cref="SpriteExtraction.ComputeSpriteSheetPng"/> gives.
/// Each call draws and encodes again (nothing is cached).
/// </summary>
public sealed class DeferredSpriteSheet
{
    private readonly Func<byte[]> _encodePng;

    internal DeferredSpriteSheet(Func<byte[]> encodePng) => _encodePng = encodePng;

    /// <summary>Draws the sheet and encodes it as PNG.</summary>
    public byte[] EncodePng() => _encodePng();
}
