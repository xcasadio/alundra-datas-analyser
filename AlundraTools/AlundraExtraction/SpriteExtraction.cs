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

    /// <summary>Packs the map's effect sheet and stamps AtlasX/AtlasY; returns the PNG bytes, or null when the map has no effect quad.</summary>
    public static byte[]? ComputeEffectSheetPng(GameMap gameMap)
    {
        using var sheet = GameMapHelper.ComputeEffectSheet(gameMap);
        return sheet == null ? null : ExtractionImages.EncodePng(sheet);
    }

    /// <summary>Writes data/map_alundra_spritesheet.png, data/map_alundra_effectsheet.png (if any) and data/map_alundra.json, in that order.</summary>
    public static void WriteAlundraMap(GameMap gameMap, SpriteSheetLayoutMode layoutMode, IExtractionSink sink)
    {
        // Must run before the JSON dump below: it populates SiImage.AtlasX/AtlasY, which the
        // serialized map_alundra.json needs to carry.
        sink.WriteBytes("data/map_alundra_spritesheet.png", ComputeSpriteSheetPng(gameMap, layoutMode));
        var effectSheet = ComputeEffectSheetPng(gameMap);

        if (effectSheet != null)
        {
            sink.WriteBytes("data/map_alundra_effectsheet.png", effectSheet);
        }

        sink.WriteBytes("data/map_alundra.json", ExtractionJson.Serialize(gameMap));
    }

    /// <summary>Writes data/map_{id}_spritesheet.png and data/map_{id}_effectsheet.png (if any); call it before serializing the map.</summary>
    public static void WriteMapSheets(GameMap gameMap, int id, SpriteSheetLayoutMode layoutMode, IExtractionSink sink)
    {
        sink.WriteBytes($"data/map_{id}_spritesheet.png", ComputeSpriteSheetPng(gameMap, layoutMode));
        var effectSheet = ComputeEffectSheetPng(gameMap);

        if (effectSheet != null)
        {
            sink.WriteBytes($"data/map_{id}_effectsheet.png", effectSheet);
        }
    }
}
