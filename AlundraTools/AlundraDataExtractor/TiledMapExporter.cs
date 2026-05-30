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
        var tiledPath = Path.Combine(extractionPath, "tiled");
        Directory.CreateDirectory(tiledPath);

        var catalog = CreateTileCatalog(gameMap, tileAnimDescriptors);
        var tilesetImageFileName = $"map_{mapId}_tileset.png";
        var tilesetFileName = $"map_{mapId}_tileset.tsj";
        var companionFileName = $"map_{mapId}.alundra.json";
        var tilesetLayout = SaveCompactTilesetImage(gameMap, catalog, Path.Combine(tiledPath, tilesetImageFileName));
        var tilesetJson = CreateTilesetJson(gameMap, mapId, catalog, tilesetImageFileName, tilesetLayout, tileAnimDescriptors);
        File.WriteAllText(Path.Combine(tiledPath, tilesetFileName), JsonSerializer.Serialize(tilesetJson, TiledJsonOptions));

        var companionJson = CreateCompanionJson(gameMap, mapId);
        File.WriteAllText(Path.Combine(tiledPath, companionFileName), JsonSerializer.Serialize(companionJson, TiledJsonOptions));

        var mapJson = CreateMapJson(gameMap, mapId, catalog, tilesetFileName, companionFileName);
        File.WriteAllText(Path.Combine(tiledPath, $"map_{mapId}.tmj"), JsonSerializer.Serialize(mapJson, TiledJsonOptions));
    }

    public static TiledTileCatalog CreateTileCatalog(GameMap gameMap)
    {
        return CreateTileCatalog(gameMap, null);
    }

    private static TiledTileCatalog CreateTileCatalog(GameMap gameMap, TileAnimDescriptor[]? tileAnimDescriptors)
    {
        return TiledTileCatalog.Create(EnumerateRawTileIds(gameMap, tileAnimDescriptors));
    }

    private static IEnumerable<ushort> EnumerateRawTileIds(GameMap gameMap, TileAnimDescriptor[]? tileAnimDescriptors)
    {
        foreach (var rawTileId in EnumerateBaseRawTileIds(gameMap))
        {
            foreach (var animationFrameRawTileId in EnumerateAnimationFrameRawTileIds(gameMap, rawTileId, tileAnimDescriptors))
            {
                yield return animationFrameRawTileId;
            }
        }
    }

    private static IEnumerable<ushort> EnumerateBaseRawTileIds(GameMap gameMap)
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

    private static IEnumerable<ushort> EnumerateAnimationFrameRawTileIds(GameMap gameMap, ushort rawTileId, TileAnimDescriptor[]? tileAnimDescriptors)
    {
        var animationSource = GetTileAnimationSource(gameMap, rawTileId, tileAnimDescriptors);

        if (animationSource == null)
        {
            yield return rawTileId;
            yield break;
        }

        for (var frame = 0; frame < animationSource.Entry.NumberOfFrame; frame++)
        {
            yield return (ushort)(rawTileId + frame * animationSource.Entry.TileHeight);
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

    private static TiledTilesetJson CreateTilesetJson(GameMap gameMap, int mapId, TiledTileCatalog catalog, string imageFileName, TiledTilesetLayout layout, TileAnimDescriptor[]? tileAnimDescriptors)
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
                .Select(entry => CreateTileJson(gameMap, catalog, entry, tileAnimDescriptors))
                .ToList()
        };
    }

    private static TiledTileJson CreateTileJson(GameMap gameMap, TiledTileCatalog catalog, TiledTileCatalogEntry entry, TileAnimDescriptor[]? tileAnimDescriptors)
    {
        var properties = new List<TiledProperty>
        {
            TiledProperty.Int("TileId", entry.RawTileId),
            TiledProperty.Int("Palette", entry.Palette),
            TiledProperty.Int("Tile", entry.Tile)
        };
        var animationSource = GetTileAnimationSource(gameMap, entry.RawTileId, tileAnimDescriptors);
        var animation = CreateTileAnimationFrames(catalog, entry.RawTileId, animationSource);

        if (animationSource != null && animation != null)
        {
            properties.Add(TiledProperty.Int("AnimationSpriteIndex", animationSource.SpriteIndex));
            properties.Add(TiledProperty.Int("AnimationFrameCount", animationSource.Entry.NumberOfFrame));
            properties.Add(TiledProperty.Int("AnimationTileHeight", animationSource.Entry.TileHeight));
            properties.Add(TiledProperty.Int("AnimationFrameDuration", animationSource.Entry.FrameDuration));
        }

        return new TiledTileJson
        {
            Id = entry.LocalTileId,
            Properties = properties,
            Animation = animation
        };
    }

    private static List<TiledTileAnimationFrameJson>? CreateTileAnimationFrames(TiledTileCatalog catalog, ushort rawTileId, TiledTileAnimationSource? animationSource)
    {
        if (animationSource == null)
        {
            return null;
        }

        var frames = new List<TiledTileAnimationFrameJson>(animationSource.Entry.NumberOfFrame);

        for (var frame = 0; frame < animationSource.Entry.NumberOfFrame; frame++)
        {
            var animationFrameRawTileId = (ushort)(rawTileId + frame * animationSource.Entry.TileHeight);
            if (!catalog.TryGetLocalTileId(animationFrameRawTileId, out var localTileId))
            {
                return null;
            }

            frames.Add(new TiledTileAnimationFrameJson
            {
                TileId = localTileId,
                Duration = animationSource.Entry.FrameDuration
            });
        }

        return frames;
    }

    private static TiledTileAnimationSource? GetTileAnimationSource(GameMap gameMap, ushort rawTileId, TileAnimDescriptor[]? tileAnimDescriptors)
    {
        if (tileAnimDescriptors == null)
        {
            return null;
        }

        var tile = rawTileId & 0x3ff;
        if (tile >= tileAnimDescriptors.Length)
        {
            return null;
        }

        var spriteIndex = tileAnimDescriptors[tile].SpriteIndex;
        if (spriteIndex == 0 || spriteIndex >= gameMap.Info.SpriteMapEntries.Length)
        {
            return null;
        }

        var entry = gameMap.Info.SpriteMapEntries[spriteIndex];
        if (entry.Enabled != 1 || entry.NumberOfFrame == 0)
        {
            return null;
        }

        return new TiledTileAnimationSource(spriteIndex, entry);
    }

    private static TiledMapJson CreateMapJson(GameMap gameMap, int mapIndex, TiledTileCatalog catalog, string tilesetFileName, string companionFileName)
    {
        var layers = CreateRendererOrderedTileLayers(gameMap, catalog, 1);
        layers.Add(CreateGroundLayer(gameMap, catalog, layers.Count + 1, false));
        layers.AddRange(CreateWallLayers(gameMap, catalog, layers.Count + 1, false));
        layers.Add(CreatePortalLayer(gameMap, layers.Count + 1, 1));
        layers.Add(CreateMapEventLayer(gameMap, layers.Count + 1, GetNextObjectId(layers)));
        layers.Add(CreateEntityLayer(gameMap, layers.Count + 1, GetNextObjectId(layers)));
        var nextObjectId = GetNextObjectId(layers);

        return new TiledMapJson
        {
            Version = TiledJsonVersion,
            TiledVersion = TiledVersion,
            Width = gameMap.Map.Width,
            Height = gameMap.Map.Height,
            TileWidth = StaticVariables.MapTileWidth,
            TileHeight = StaticVariables.MapTileHeight,
            NextLayerId = layers.Count + 1,
            NextObjectId = nextObjectId,
            Tilesets =
            [
                new TiledMapTilesetJson
                {
                    FirstGid = 1,
                    Source = tilesetFileName.Replace('\\', '/')
                }
            ],
            Properties =
            [
                TiledProperty.String("SourceFileName", $"map_{mapIndex}.json"),
                TiledProperty.File("SourceJson", $"../map_{mapIndex}.json"),
                TiledProperty.File("AlundraCompanionJson", companionFileName.Replace('\\', '/')),
                TiledProperty.String("TileLayerPlacement", "RenderY_* layers are visible and packed by renderer target height; Ground/Walls_* layers are hidden raw data layers"),
                TiledProperty.String("WallLayerPlacement", "renderer target cell; source cell and wall offset data are stored in AlundraCompanionJson"),
                TiledProperty.Int("MapIndex", mapIndex),
                TiledProperty.Int("MapId", Convert.ToInt32(gameMap.Info.MapId)),
                TiledProperty.Int("Gravity", gameMap.Info.Gravity),
                TiledProperty.Int("ZViscosity", gameMap.Info.ZViscosity),
                TiledProperty.Int("SlideEffectId", gameMap.Info.SlideEffectId),
                TiledProperty.Int("BalanceLevel", gameMap.Info.BalanceLevel),
                TiledProperty.Int("C", gameMap.Info.C),
                TiledProperty.Int("D", gameMap.Info.D),
                TiledProperty.Int("E", gameMap.Info.E),
                TiledProperty.Int("F", gameMap.Info.F),
                TiledProperty.Int("_10", gameMap.Info._10),
                TiledProperty.Int("_11", gameMap.Info._11)
            ],
            Layers = layers
        };
    }

    private static List<TiledLayerJson> CreateRendererOrderedTileLayers(GameMap gameMap, TiledTileCatalog catalog, int firstLayerId)
    {
        var map = gameMap.Map;
        var layerDataByTargetY = new SortedDictionary<int, List<int[]>>();

        for (var sourceY = 0; sourceY < map.Height; sourceY++)
        {
            for (var sourceX = 0; sourceX < map.Width; sourceX++)
            {
                var sourceIndex = sourceY * map.Width + sourceX;
                var mapTile = map.MapTiles[sourceIndex];
                if (mapTile.TileId != EmptyTileId)
                {
                    AddRendererTile(layerDataByTargetY, map.Width, map.Height, sourceX, sourceY - mapTile.Height, catalog.GetGidOrEmpty(mapTile.TileId));
                }

                var wallTiles = mapTile.WallTiles;
                if (wallTiles?.Tiles == null)
                {
                    continue;
                }

                for (var stackIndex = 0; stackIndex < wallTiles.Count && stackIndex < wallTiles.Tiles.Length; stackIndex++)
                {
                    var wallTileId = wallTiles.Tiles[stackIndex];
                    if (wallTileId == EmptyTileId)
                    {
                        continue;
                    }

                    var targetY = sourceY - mapTile.Height - wallTiles.Offset + stackIndex + 1;
                    AddRendererTile(layerDataByTargetY, map.Width, map.Height, sourceX, targetY, catalog.GetGidOrEmpty(wallTileId));
                }
            }
        }

        var layers = new List<TiledLayerJson>();

        foreach (var (targetY, layerDataForTargetY) in layerDataByTargetY)
        {
            for (var planeIndex = 0; planeIndex < layerDataForTargetY.Count; planeIndex++)
            {
                var layerName = planeIndex == 0
                    ? $"RenderY_{targetY:D2}"
                    : $"RenderY_{targetY:D2}_{planeIndex}";
                layers.Add(CreateRendererTileLayer(
                    firstLayerId + layers.Count,
                    layerName,
                    map.Width,
                    map.Height,
                    layerDataForTargetY[planeIndex],
                    targetY,
                    planeIndex));
            }
        }

        return layers;
    }

    private static void AddRendererTile(SortedDictionary<int, List<int[]>> layerDataByTargetY, int width, int height, int targetX, int targetY, int gid)
    {
        if (gid == 0 || targetX < 0 || targetX >= width || targetY < 0 || targetY >= height)
        {
            return;
        }

        if (!layerDataByTargetY.TryGetValue(targetY, out var layerDataForTargetY))
        {
            layerDataForTargetY = [new int[width * height]];
            layerDataByTargetY.Add(targetY, layerDataForTargetY);
        }

        var targetIndex = targetY * width + targetX;
        foreach (var layerData in layerDataForTargetY)
        {
            if (layerData[targetIndex] == 0)
            {
                layerData[targetIndex] = gid;
                return;
            }
        }

        var collisionLayerData = new int[width * height];
        collisionLayerData[targetIndex] = gid;
        layerDataForTargetY.Add(collisionLayerData);
    }

    private static TiledLayerJson CreateRendererTileLayer(int layerId, string name, int width, int height, int[] data, int targetY, int collisionPlane)
    {
        return new TiledLayerJson
        {
            Id = layerId,
            Name = name,
            Type = "tilelayer",
            Width = width,
            Height = height,
            Data = data,
            Properties =
            [
                TiledProperty.Int("TargetY", targetY),
                TiledProperty.Int("CollisionPlane", collisionPlane),
                TiledProperty.String("Placement", "renderer target height"),
                TiledProperty.String("MergeStrategy", "one layer per targetY; extra planes only when multiple renderer tiles target the same cell")
            ]
        };
    }

    private static TiledLayerJson CreateGroundLayer(GameMap gameMap, TiledTileCatalog catalog, int layerId, bool visible = true)
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
            Visible = visible,
            Data = data,
            Properties =
            [
                TiledProperty.String("Role", "raw logical data")
            ]
        };
    }

    private static List<TiledLayerJson> CreateWallLayers(GameMap gameMap, TiledTileCatalog catalog, int firstLayerId, bool visible = true)
    {
        var map = gameMap.Map;
        var maxWallCount = map.MapTiles
            .Where(tile => tile.WallTiles != null)
            .Select(tile => (int)tile.WallTiles!.Count)
            .DefaultIfEmpty(0)
            .Max();
        var layers = new List<TiledLayerJson>(maxWallCount);

        for (var stackIndex = 0; stackIndex < maxWallCount; stackIndex++)
        {
            var data = new int[map.MapTiles.Length];

            for (var index = 0; index < map.MapTiles.Length; index++)
            {
                var mapTile = map.MapTiles[index];
                var wallTiles = mapTile.WallTiles;

                if (wallTiles?.Tiles == null || stackIndex >= wallTiles.Tiles.Length)
                {
                    continue;
                }

                var wallTileId = wallTiles.Tiles[stackIndex];
                if (wallTileId == EmptyTileId)
                {
                    continue;
                }

                var targetY = index / map.Width - mapTile.Height - wallTiles.Offset + stackIndex + 1;

                if (targetY < 0 || targetY >= map.Height)
                {
                    continue;
                }

                var targetIndex = targetY * map.Width + index % map.Width;
                data[targetIndex] = catalog.GetGidOrEmpty(wallTileId);
            }

            layers.Add(new TiledLayerJson
            {
                Id = firstLayerId + stackIndex,
                Name = $"Walls_{stackIndex}",
                Type = "tilelayer",
                Width = map.Width,
                Height = map.Height,
                Visible = visible,
                Data = data,
                Properties =
                [
                    TiledProperty.Int("StackIndex", stackIndex),
                    TiledProperty.String("Placement", "renderer target cell"),
                    TiledProperty.String("RendererYFormula", "(Y - Height - WallTiles.Offset + StackIndex + 1) * TileHeight")
                ]
            });
        }

        return layers;
    }

    private static TiledLayerJson CreatePortalLayer(GameMap gameMap, int layerId, int firstObjectId)
    {
        var map = gameMap.Map;
        var objects = new List<TiledObjectJson>();

        for (var index = 0; index < gameMap.Info.Portals.Length; index++)
        {
            var portal = gameMap.Info.Portals[index];

            if (!IsValidPortal(portal))
            {
                continue;
            }

            var tile = map.MapTiles[portal.X1 + portal.Y1 * map.Width];
            var x1 = portal.X1 * StaticVariables.MapTileWidth;
            var y1 = (portal.Y1 - tile.Height) * StaticVariables.MapTileHeight;
            var x2 = (portal.X2 + 1) * StaticVariables.MapTileWidth;
            var y2 = (portal.Y2 - tile.Height + 1) * StaticVariables.MapTileHeight;

            objects.Add(new TiledObjectJson
            {
                Id = firstObjectId + objects.Count,
                Name = $"Portal_{index}",
                Type = "Portal",
                X = x1,
                Y = y1,
                Width = x2 - x1,
                Height = y2 - y1,
                Properties =
                [
                    TiledProperty.Int("Index", index),
                    TiledProperty.Int("X1", portal.X1),
                    TiledProperty.Int("Y1", portal.Y1),
                    TiledProperty.Int("X2", portal.X2),
                    TiledProperty.Int("Y2", portal.Y2),
                    TiledProperty.Int("DestMapId", portal.DestMapId),
                    TiledProperty.Int("DestTileX", portal.DestTileX),
                    TiledProperty.Int("DestTileY", portal.DestTileY),
                    TiledProperty.Int("ZLevel", portal.ZLevel),
                    TiledProperty.Int("Flags", portal.Flags)
                ]
            });
        }

        return new TiledLayerJson
        {
            Id = layerId,
            Name = "Portals",
            Type = "objectgroup",
            Objects = objects,
            Properties =
            [
                TiledProperty.String("ValidityFilter", "X2 != 0xff && Y2 != 0xff"),
                TiledProperty.String("Placement", "frmAlundra portal rectangle formula")
            ]
        };
    }

    private static bool IsValidPortal(Portal portal) =>
        portal.X2 != 0xff && portal.Y2 != 0xff;

    private static TiledLayerJson CreateMapEventLayer(GameMap gameMap, int layerId, int firstObjectId)
    {
        var objects = new List<TiledObjectJson>();
        var records = gameMap.SpriteInfo?.MapEvents?.Records ?? [];

        for (var index = 0; index < records.Length; index++)
        {
            var record = records[index];

            if (record == null)
            {
                continue;
            }

            objects.Add(new TiledObjectJson
            {
                Id = firstObjectId + objects.Count,
                Name = $"MapEvent_{index}",
                Type = "MapEvent",
                X = record.X1 * StaticVariables.MapTileWidth,
                Y = record.Y1 * StaticVariables.MapTileHeight,
                Width = (record.X2 - record.X1 + 1) * StaticVariables.MapTileWidth,
                Height = (record.Y2 - record.Y1 + 1) * StaticVariables.MapTileHeight,
                Properties =
                [
                    TiledProperty.Int("Index", index),
                    TiledProperty.Int("X1", record.X1),
                    TiledProperty.Int("Y1", record.Y1),
                    TiledProperty.Int("X2", record.X2),
                    TiledProperty.Int("Y2", record.Y2),
                    TiledProperty.Int("EventCodesBIndex", record.EventCodesBIndex),
                    TiledProperty.Int("EventCodesBIndexMasked", record.EventCodesBIndex & 0x7f),
                    TiledProperty.Int("Ub1", record.Ub1),
                    TiledProperty.Int("Ub2", record.Ub2),
                    TiledProperty.Int("Ub3", record.Ub3)
                ]
            });
        }

        return new TiledLayerJson
        {
            Id = layerId,
            Name = "MapEvents",
            Type = "objectgroup",
            Objects = objects
        };
    }

    private static TiledLayerJson CreateEntityLayer(GameMap gameMap, int layerId, int firstObjectId)
    {
        var objects = new List<TiledObjectJson>();
        var records = gameMap.SpriteInfo?.Entities?.Entities ?? [];

        for (var index = 0; index < records.Length; index++)
        {
            var record = records[index];

            if (record == null)
            {
                continue;
            }

            var displayTileX = record.XPos / 2;
            var displayTileY = record.YPos / 2;
            var displayHeight = record.Height / 2;
            var displayPixelX = displayTileX * StaticVariables.MapTileWidth;
            var displayPixelY = (displayTileY - displayHeight) * StaticVariables.MapTileHeight;

            objects.Add(new TiledObjectJson
            {
                Id = firstObjectId + objects.Count,
                Name = $"Entity_{index}",
                Type = "Entity",
                X = displayPixelX,
                Y = displayPixelY,
                Width = StaticVariables.MapTileWidth,
                Height = StaticVariables.MapTileHeight,
                Properties = CreateEntityProperties(record, index)
            });
        }

        return new TiledLayerJson
        {
            Id = layerId,
            Name = "Entities",
            Type = "objectgroup",
            Objects = objects
        };
    }

    private static List<TiledProperty> CreateEntityProperties(SiEntityRecord record, int index)
    {
        var properties = new List<TiledProperty>
        {
            TiledProperty.Int("Index", index),
            TiledProperty.Int("XMin", record.XMin),
            TiledProperty.Int("YMin", record.YMin),
            TiledProperty.Int("XMax", record.XMax),
            TiledProperty.Int("YMax", record.YMax),
            TiledProperty.Int("IsEnabled", record.IsEnabled),
            TiledProperty.Int("SpriteDirection", record.SpriteDirection),
            TiledProperty.Int("SpriteTableIndex", record.SpriteTableIndex),
            TiledProperty.Int("XPos", record.XPos),
            TiledProperty.Int("YPos", record.YPos),
            TiledProperty.Int("Height", record.Height),
            TiledProperty.Int("EventCodesA_LoadIndex", record.EventCodesA_LoadIndex),
            TiledProperty.Int("EventCodesB_MapIndex", record.EventCodesB_MapIndex),
            TiledProperty.Int("EventCodesC_TickIndex", record.EventCodesC_TickIndex),
            TiledProperty.Int("EventCodesD_TouchIndex", record.EventCodesD_TouchIndex),
            TiledProperty.Int("EventCodesE_DeactivateIndex", record.EventCodesE_DeactivateIndex),
            TiledProperty.Int("EventCodesF_InteractIndex", record.EventCodesF_InteractIndex),
            TiledProperty.Int("_10", record._10),
            TiledProperty.Int("Contents", record.Contents),
            TiledProperty.Int("DisplayX", record.XPos / 2),
            TiledProperty.Int("DisplayY", record.YPos / 2),
            TiledProperty.Int("DisplayHeight", record.Height / 2),
            TiledProperty.Int("DisplayPixelX", (record.XPos / 2) * StaticVariables.MapTileWidth),
            TiledProperty.Int("DisplayPixelY", (record.YPos / 2 - record.Height / 2) * StaticVariables.MapTileHeight)
        };

        var entityName = EntityNames.GetName(record.SpriteDirection, record.SpriteTableIndex);
        if (entityName != null)
        {
            properties.Add(TiledProperty.String("EntityName", entityName));
        }

        return properties;
    }

    private static int GetNextObjectId(IEnumerable<TiledLayerJson> layers) =>
        layers
            .SelectMany(layer => layer.Objects ?? [])
            .Select(tiledObject => tiledObject.Id)
            .DefaultIfEmpty(0)
            .Max() + 1;

    private static AlundraTiledCompanionJson CreateCompanionJson(GameMap gameMap, int mapIndex)
    {
        var map = gameMap.Map;
        var cells = new List<AlundraCellJson>(map.MapTiles.Length);

        for (var index = 0; index < map.MapTiles.Length; index++)
        {
            var tile = map.MapTiles[index];
            var x = index % map.Width;
            var y = index / map.Width;
            cells.Add(new AlundraCellJson
            {
                Index = index,
                X = x,
                Y = y,
                Walkability = tile.Walkability,
                GroundProperty = tile.GroundProperty,
                Slope = tile.Slope,
                Height = tile.Height,
                WallTilesOffset = tile.WallTilesOffset,
                TileId = tile.TileId,
                Palette = tile.Palette,
                Tile = tile.Tile,
                Flags = tile.Flags,
                WallTiles = CreateWallTilesJson(tile, x, y)
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

    private static AlundraWallTilesJson? CreateWallTilesJson(MapTile tile, int x, int y)
    {
        if (tile.WallTiles?.Tiles == null)
        {
            return null;
        }

        return new AlundraWallTilesJson
        {
            Offset = tile.WallTiles.Offset,
            Count = tile.WallTiles.Count,
            Tiles = tile.WallTiles.Tiles.ToList(),
            RendererTileYByStackIndex = tile.WallTiles.Tiles
                .Select((_, stackIndex) => y - tile.Height - tile.WallTiles.Offset + stackIndex + 1)
                .ToList(),
            RendererPixelX = x * StaticVariables.MapTileWidth
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
    public AlundraWallTilesJson? WallTiles { get; init; }
}

public sealed class AlundraWallTilesJson
{
    public sbyte Offset { get; init; }
    public byte Count { get; init; }
    public List<ushort> Tiles { get; init; } = [];
    public List<int> RendererTileYByStackIndex { get; init; } = [];
    public int RendererPixelX { get; init; }
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

    [JsonPropertyName("objects")]
    public List<TiledObjectJson>? Objects { get; init; }

    [JsonPropertyName("properties")]
    public List<TiledProperty> Properties { get; init; } = [];
}

public sealed class TiledObjectJson
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("x")]
    public int X { get; init; }

    [JsonPropertyName("y")]
    public int Y { get; init; }

    [JsonPropertyName("width")]
    public int Width { get; init; }

    [JsonPropertyName("height")]
    public int Height { get; init; }

    [JsonPropertyName("point")]
    public bool? Point { get; init; }

    [JsonPropertyName("properties")]
    public List<TiledProperty> Properties { get; init; } = [];
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

    [JsonPropertyName("animation")]
    public List<TiledTileAnimationFrameJson>? Animation { get; init; }
}

public sealed class TiledTileAnimationFrameJson
{
    [JsonPropertyName("tileid")]
    public int TileId { get; init; }

    [JsonPropertyName("duration")]
    public int Duration { get; init; }
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

    public bool TryGetLocalTileId(ushort rawTileId, out int localTileId)
    {
        if (_entriesByRawTileId.TryGetValue(rawTileId, out var entry))
        {
            localTileId = entry.LocalTileId;
            return true;
        }

        localTileId = 0;
        return false;
    }
}

public sealed record TiledTileAnimationSource(int SpriteIndex, SpriteMapEntry Entry);

public sealed record TiledTileCatalogEntry(
    ushort RawTileId,
    int LocalTileId,
    int Gid,
    int Palette,
    int Tile);