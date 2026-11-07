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
        => ProcessAllMaps(datasBin, tileAnimDescriptors, psxFramesPerSecond, tiledTilesetLayoutMode, spriteSheetLayoutMode, new SinkMapFamilyConsumer(sink), log);

    /// <summary>
    /// Loads map_alundra then the 483 maps, in the original order, computing each item where the default extraction
    /// computes it and handing it to <paramref name="consumer"/> (see <see cref="IMapFamilyConsumer"/> for the callback
    /// order). Logs "Extract map alundra" and "Extract map {i}" through <paramref name="log"/>.
    /// </summary>
    public static void ProcessAllMaps(
        DatasBin datasBin,
        TileAnimDescriptor[] tileAnimDescriptors,
        int psxFramesPerSecond,
        TiledTilesetLayoutMode tiledTilesetLayoutMode,
        SpriteSheetLayoutMode spriteSheetLayoutMode,
        IMapFamilyConsumer consumer,
        Action<string> log)
    {
        log("Extract map alundra");
        using var br = datasBin.OpenBin();
        datasBin.AlundraGameMap.Load(br);
        // The inventory's opening portrait (sprite record 0, see GameMap.InventoryPortrait) is in no
        // animation: without this field, neither the atlas nor map_alundra.json would carry it.
        datasBin.AlundraGameMap.InventoryPortrait = datasBin.AlundraGameMap.SpriteInfo.SpriteRecords[0].GetPortraitImageset(br).Images[0];
        GameMapHelper.LoadDialoguePortraits(datasBin.AlundraGameMap, br);
        SpriteExtraction.ProcessAlundraMap(datasBin.AlundraGameMap, spriteSheetLayoutMode, consumer);

        for (int i = 0; i < 483; i++)
        {
            log($"Extract map {i}");
            var gameMap = datasBin.GameMaps[i];
            gameMap.Load(br);
            GameMapHelper.LoadDialoguePortraits(gameMap, br);
            ProcessMap(gameMap, i, tileAnimDescriptors, psxFramesPerSecond, tiledTilesetLayoutMode, spriteSheetLayoutMode, consumer);
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
        => ProcessMap(gameMap, id, tileAnimDescriptors, psxFramesPerSecond, tiledTilesetLayoutMode, spriteSheetLayoutMode, new SinkMapFamilyConsumer(sink));

    /// <summary>
    /// Computes one already loaded map and hands to the consumer, in order: tile sheet, sprite sheet, effect sheet (or null),
    /// the stamped map, the Tiled export.
    /// </summary>
    public static void ProcessMap(
        GameMap gameMap,
        int id,
        TileAnimDescriptor[] tileAnimDescriptors,
        int psxFramesPerSecond,
        TiledTilesetLayoutMode tiledTilesetLayoutMode,
        SpriteSheetLayoutMode spriteSheetLayoutMode,
        IMapFamilyConsumer consumer)
    {
        // The tile sheet only fills GameMap's pure per-index tile bitmap memo (GetTileBitmap), which the Tiled export fills
        // identically on demand: skipping it for a consumer that declines it changes no later output.
        if (consumer.WantsTileSheets)
        {
            consumer.MapTileSheet(id, ComputeTileSheetPng(gameMap, tileAnimDescriptors));
        }

        // Must run before the map is handed over: packing populates SiImage.AtlasX/AtlasY, which the
        // serialized map_{id}.json needs to carry.
        SpriteExtraction.ProcessMapSheets(gameMap, id, spriteSheetLayoutMode, consumer);

        consumer.Map(id, gameMap);

        ProcessTiledMap(gameMap, id, tileAnimDescriptors, psxFramesPerSecond, tiledTilesetLayoutMode, consumer);
    }

    /// <summary>Writes the map's Tiled files under data/tiled/: tileset image, tileset, companion, map.</summary>
    public static void WriteTiledMap(
        GameMap gameMap,
        int id,
        TileAnimDescriptor[] tileAnimDescriptors,
        int psxFramesPerSecond,
        TiledTilesetLayoutMode tiledTilesetLayoutMode,
        IExtractionSink sink)
        => ProcessTiledMap(gameMap, id, tileAnimDescriptors, psxFramesPerSecond, tiledTilesetLayoutMode, new SinkMapFamilyConsumer(sink));

    /// <summary>Computes the map's Tiled export, hands it to the consumer, then disposes it (its bitmap must not outlive the call).</summary>
    public static void ProcessTiledMap(
        GameMap gameMap,
        int id,
        TileAnimDescriptor[] tileAnimDescriptors,
        int psxFramesPerSecond,
        TiledTilesetLayoutMode tiledTilesetLayoutMode,
        IMapFamilyConsumer consumer)
    {
        using var tiled = TiledMapExporter.ComputeTiledMap(gameMap, id, tileAnimDescriptors, psxFramesPerSecond, tiledTilesetLayoutMode);
        consumer.TiledMap(id, gameMap, tiled);
    }
}
