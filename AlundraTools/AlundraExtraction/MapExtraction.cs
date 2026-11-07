using AlundraEngine.DatasBin;
using AlundraDataExtractor;

namespace AlundraExtraction;

/// <summary>Family 4 of the extraction: the map_alundra sheets, and per map the tile sheet, sprite sheets, map JSON and Tiled export.</summary>
public static class MapExtraction
{
    // ---- Compute (no writing) ----

    /// <summary>The map's tile sheet as PNG bytes.</summary>
    public static byte[] ComputeTileSheetPng(GameMap gameMap, TileAnimDescriptor[] tileAnimDescriptors)
    {
        using var sheet = GameMapHelper.ComputeTileSheet(gameMap, tileAnimDescriptors);
        return ExtractionImages.EncodePng(sheet);
    }

    // ---- Write ----

    /// <summary>
    /// Loads and writes map_alundra, then the 483 maps, in the original order. Logs "Extract map alundra" and
    /// "Extract map {i}" through <paramref name="log"/>.
    /// </summary>
    public static void WriteAllMaps(
        DatasBin datasBin,
        TileAnimDescriptor[] tileAnimDescriptors,
        int psxFramesPerSecond,
        TiledTilesetLayoutMode tiledTilesetLayoutMode,
        SpriteSheetLayoutMode spriteSheetLayoutMode,
        IExtractionSink sink,
        Action<string> log)
    {
        log("Extract map alundra");
        using var br = datasBin.OpenBin();
        datasBin.AlundraGameMap.Load(br);
        // The inventory's opening portrait (sprite record 0, see GameMap.InventoryPortrait) is in no
        // animation: without this field, neither the atlas nor map_alundra.json would carry it.
        datasBin.AlundraGameMap.InventoryPortrait = datasBin.AlundraGameMap.SpriteInfo.SpriteRecords[0].GetPortraitImageset(br).Images[0];
        GameMapHelper.LoadDialoguePortraits(datasBin.AlundraGameMap, br);
        SpriteExtraction.WriteAlundraMap(datasBin.AlundraGameMap, spriteSheetLayoutMode, sink);

        for (int i = 0; i < 483; i++)
        {
            log($"Extract map {i}");
            var gameMap = datasBin.GameMaps[i];
            gameMap.Load(br);
            GameMapHelper.LoadDialoguePortraits(gameMap, br);
            WriteMap(gameMap, i, tileAnimDescriptors, psxFramesPerSecond, tiledTilesetLayoutMode, spriteSheetLayoutMode, sink);
        }
    }

    /// <summary>
    /// Writes data/map_{id}_tilesheet.png, the map's sprite and effect sheets, data/map_{id}.json, then
    /// data/tiled/map_{id}_tileset.png, .tsj, map_{id}.alundra.json and map_{id}.tmj.
    /// </summary>
    public static void WriteMap(
        GameMap gameMap,
        int id,
        TileAnimDescriptor[] tileAnimDescriptors,
        int psxFramesPerSecond,
        TiledTilesetLayoutMode tiledTilesetLayoutMode,
        SpriteSheetLayoutMode spriteSheetLayoutMode,
        IExtractionSink sink)
    {
        sink.WriteBytes($"data/map_{id}_tilesheet.png", ComputeTileSheetPng(gameMap, tileAnimDescriptors));

        // Must run before the JSON dump below: it populates SiImage.AtlasX/AtlasY, which the
        // serialized map_{id}.json needs to carry.
        SpriteExtraction.WriteMapSheets(gameMap, id, spriteSheetLayoutMode, sink);

        sink.WriteBytes($"data/map_{id}.json", ExtractionJson.Serialize(gameMap));

        WriteTiledMap(gameMap, id, tileAnimDescriptors, psxFramesPerSecond, tiledTilesetLayoutMode, sink);
    }

    /// <summary>Writes the map's Tiled files under data/tiled/: tileset image, tileset, companion, map.</summary>
    public static void WriteTiledMap(
        GameMap gameMap,
        int id,
        TileAnimDescriptor[] tileAnimDescriptors,
        int psxFramesPerSecond,
        TiledTilesetLayoutMode tiledTilesetLayoutMode,
        IExtractionSink sink)
    {
        using var tiled = TiledMapExporter.ComputeTiledMap(gameMap, id, tileAnimDescriptors, psxFramesPerSecond, tiledTilesetLayoutMode);
        sink.WriteBytes($"data/tiled/{tiled.TilesetImageFileName}", ExtractionImages.EncodePng(tiled.TilesetImage));
        sink.WriteBytes($"data/tiled/{tiled.TilesetFileName}", TiledMapExporter.Serialize(tiled.Tileset));
        sink.WriteBytes($"data/tiled/{tiled.CompanionFileName}", TiledMapExporter.Serialize(tiled.Companion));
        sink.WriteBytes($"data/tiled/{tiled.MapFileName}", TiledMapExporter.Serialize(tiled.Map));
    }
}
