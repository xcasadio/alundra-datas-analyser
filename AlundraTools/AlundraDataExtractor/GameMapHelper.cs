using AlundraEngine;
using AlundraEngine.DatasBin;
using System.Drawing;
using System.Drawing.Imaging;

namespace AlundraDataExtractor;

public enum SpriteSheetLayoutMode
{
    /// <summary>
    /// The eight native 256x256 VRAM pages stacked vertically, holes included. Default: it keeps the
    /// exported sheet aligned with the original VRAM coordinates.
    /// </summary>
    Original,

    /// <summary>
    /// Only the quads actually used, shelf-packed, one cell per (VRAM region, palette) pair.
    /// </summary>
    Compact
}

public static class GameMapHelper
{
    public static void SaveTileSheet(GameMap gameMap, string fileName, TileAnimDescriptor[] tileAnimDescriptors = null)
    {
        using var bitmap = new Bitmap(GameMapTilesheetLayout.OriginalImageWidth, GameMapTilesheetLayout.OriginalImageHeight);
        using var graphics = Graphics.FromImage(bitmap);
        var tileCache = new HashSet<ushort>();

        foreach (var mapTile in gameMap.Map.MapTiles)
        {
            var tile = mapTile.TileId;

            if (tile != 0xffff)
            {
                if (tileCache.Add(tile))
                {
                    if (tileAnimDescriptors != null)
                    {
                        DrawAllAnimatedTiles(gameMap, tile, graphics, tileAnimDescriptors);
                    }
                    else
                    {
                        DrawTile(gameMap, tile, graphics);
                    }
                }
            }

            if (mapTile.WallTiles != null)
            {
                foreach (var wallTile in mapTile.WallTiles.Tiles)
                {
                    if (wallTile != 0xffff && tileCache.Add(wallTile))
                    {
                        if (tileAnimDescriptors != null)
                        {
                            DrawAllAnimatedTiles(gameMap, wallTile, graphics, tileAnimDescriptors);
                        }
                        else
                        {
                            DrawTile(gameMap, wallTile, graphics);
                        }
                    }
                }
            }
        }

        bitmap.Save(fileName, ImageFormat.Png);
    }

    private static void DrawAllAnimatedTiles(GameMap gameMap, ushort tileId, Graphics graphics, TileAnimDescriptor[] tileAnimDescriptors)
    {
        var tile = tileId & 0x3ff;

        var spriteIndex = tile >= tileAnimDescriptors.Length ? 0 : tileAnimDescriptors[tile].SpriteIndex;

        if (spriteIndex != 0 && gameMap.Info.SpriteMapEntries[spriteIndex].Enabled == 1)
        {
            var entry = gameMap.Info.SpriteMapEntries[spriteIndex];

            for (int frame = 0; frame < entry.NumberOfFrame; frame++)
            {
                ushort animatedTileId = (ushort)(tileId + frame * entry.TileHeight);
                DrawTile(gameMap, animatedTileId, graphics);
            }
        }
        else
        {
            DrawTile(gameMap, tileId, graphics);
        }
    }

    private static void DrawTile(GameMap gameMap, ushort tileId, Graphics graphics)
    {
        var tileBitmap = gameMap.GetTileBitmap(tileId);
        var localTileId = tileId & 0x3ff;
        var x = GameMapTilesheetLayout.GetTileX(localTileId);
        var y = GameMapTilesheetLayout.GetTileY(localTileId);
        graphics.DrawImage(tileBitmap, x, y);
    }

    // Writes the map spritesheet PNG and records, on every SiImage, where its quad landed there
    // (AtlasX/AtlasY). Call this before serializing the map to JSON so those fields are set.
    //
    // Two layouts are available, see SpriteSheetLayoutMode. Both deduplicate on Signature
    // (Spritesheet+Palette+SourceX+SourceY+Swidth+Sheight already combined, see SiImage), so a quad
    // and its mirrored twin share one cell, and both stamp the resulting position on every SiImage
    // instance carrying that signature.
    public static void SaveSpriteSheet(GameMap gameMap, string fileName, SpriteSheetLayoutMode layoutMode = SpriteSheetLayoutMode.Original)
    {
        var uniqueImages = new List<SiImage>();
        var imagesBySignature = new Dictionary<long, List<SiImage>>();

        foreach (var image in EnumerateImages(gameMap))
        {
            if (!imagesBySignature.TryGetValue(image.Signature, out var images))
            {
                images = new List<SiImage>();
                imagesBySignature[image.Signature] = images;
                uniqueImages.Add(image);
            }

            images.Add(image);
        }

        var layout = layoutMode switch
        {
            SpriteSheetLayoutMode.Original => CreateOriginalSpriteSheetLayout(uniqueImages),
            SpriteSheetLayoutMode.Compact => CreateCompactSpriteSheetLayout(uniqueImages),
            _ => throw new ArgumentOutOfRangeException(nameof(layoutMode), layoutMode, "Unsupported spritesheet layout mode.")
        };

        using (var bitmap = new Bitmap(layout.Width, layout.Height))
        {
            using (var graphics = Graphics.FromImage(bitmap))
            {
                foreach (var image in layout.DrawOrder)
                {
                    var spriteBitmap = gameMap.GetSpriteBitmap(image);
                    var (x, y) = layout.PositionBySignature[image.Signature];
                    graphics.DrawImage(spriteBitmap, x, y);
                }
            }

            bitmap.Save(fileName, ImageFormat.Png);
        }

        foreach (var (signature, position) in layout.PositionBySignature)
        {
            foreach (var image in imagesBySignature[signature])
            {
                image.AtlasX = position.X;
                image.AtlasY = position.Y;
            }
        }
    }

    // Historical layout: the eight 256x256 VRAM pages stacked vertically, each quad drawn at the
    // VRAM window it samples (SourceX/SourceY, not Sx/Sy - a mirrored quad names its source one
    // texel early, see SiImage). Pages keep their holes, so the sheet stays readable next to the
    // original VRAM dumps, and AtlasX/AtlasY come out equal to the native coordinates.
    //
    // These coordinates are not collision-free: the same VRAM region is legitimately reused with a
    // different palette across frames of the same animation (e.g. a color-cycling sparkle), and all
    // of those quads land on one cell here, so the last one drawn wins and the others crop the
    // wrong color. Draw order is first-seen order, as the historical export had it. Use Compact
    // when every (region, palette) pair must survive.
    private static SpriteSheetLayout CreateOriginalSpriteSheetLayout(List<SiImage> uniqueImages)
    {
        var positionBySignature = new Dictionary<long, (int X, int Y)>();

        foreach (var image in uniqueImages)
        {
            positionBySignature[image.Signature] = (image.SourceX, (image.Spritesheet & 0x7) * VramPageSize + image.SourceY);
        }

        return new SpriteSheetLayout(VramPageSize, VramPageSize * VramPageCount, uniqueImages, positionBySignature);
    }

    // Compact layout: one cell per unique Signature, so a region reused under several palettes gets
    // one cell per palette and every quad crops the color it was meant to show. Tallest-first shelf
    // packing: simple, deterministic, and good enough for the small (mostly 16-48px) quads found in
    // practice.
    private static SpriteSheetLayout CreateCompactSpriteSheetLayout(List<SiImage> uniqueImages)
    {
        const int canvasWidth = 512;
        const int padding = 1; // keep neighbouring cells from bleeding into each other when sampled

        var packingOrder = uniqueImages
            .OrderByDescending(image => image.Sheight)
            .ThenBy(image => image.Signature)
            .ToList();

        var positionBySignature = new Dictionary<long, (int X, int Y)>();
        int cursorX = 0, cursorY = 0, shelfHeight = 0, canvasHeight = 0;

        foreach (var image in packingOrder)
        {
            int cellWidth = image.Swidth + padding;
            int cellHeight = image.Sheight + padding;

            if (cursorX + cellWidth > canvasWidth)
            {
                cursorX = 0;
                cursorY += shelfHeight;
                shelfHeight = 0;
            }

            positionBySignature[image.Signature] = (cursorX, cursorY);
            cursorX += cellWidth;
            shelfHeight = Math.Max(shelfHeight, cellHeight);
            canvasHeight = Math.Max(canvasHeight, cursorY + shelfHeight);
        }

        return new SpriteSheetLayout(canvasWidth, Math.Max(canvasHeight, 1), packingOrder, positionBySignature);
    }

    // JUSTIFICATION: C# language bridge only - carries one resolved spritesheet layout so both modes
    // share the drawing and AtlasX/AtlasY stamping code above.
    private sealed record SpriteSheetLayout(
        int Width,
        int Height,
        IReadOnlyList<SiImage> DrawOrder,
        Dictionary<long, (int X, int Y)> PositionBySignature);

    private const int VramPageSize = 256;
    private const int VramPageCount = 8;

    private static IEnumerable<SiImage> EnumerateImages(GameMap gameMap)
    {
        foreach (var spriteRecord in gameMap.SpriteInfo.SpriteRecords.Where(x => x != null))
        {
            if (spriteRecord.AnimSets == null)
            {
                continue;
            }

            foreach (var animationSet in spriteRecord.AnimSets)
            {
                if (animationSet == null)
                {
                    continue;
                }

                foreach (var animation in animationSet.PreloadedAnims)
                {
                    if (animation?.Frames == null)
                    {
                        continue;
                    }

                    for (int i = 0; i < animation.NumberOfFrames; i++)
                    {
                        var frame = animation.Frames[i];

                        if (frame?.Images?.Images == null)
                        {
                            continue;
                        }

                        foreach (var image in frame.Images.Images)
                        {
                            yield return image;
                        }
                    }
                }
            }
        }
    }
}