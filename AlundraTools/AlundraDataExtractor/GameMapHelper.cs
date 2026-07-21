using AlundraEngine;
using AlundraEngine.DatasBin;
using System.Drawing;
using System.Drawing.Imaging;

namespace AlundraDataExtractor;

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

    // Native VRAM coordinates (Spritesheet/Sx/Sy) are not collision-free: the same VRAM region is
    // legitimately reused with a different palette across frames of the same animation (e.g. a
    // color-cycling sparkle effect). Packing straight at (Sx, Spritesheet*256+Sy) like the old
    // layout did means whichever quad is drawn last simply overwrites the others there, so every
    // earlier frame referencing that region ends up cropping the wrong color. Instead, pack one
    // cell per unique Signature (Spritesheet+Palette+Sx+Sy+Swidth+Sheight already combined, see
    // SiImage) and record where each one landed on every SiImage instance that shares it, via the
    // AtlasX/AtlasY fields. Call this before serializing the map to JSON so those fields are set.
    public static void SaveSpriteSheet(GameMap gameMap, string fileName)
    {
        const int canvasWidth = 512;
        const int padding = 1; // keep neighbouring cells from bleeding into each other when sampled

        var firstImageBySignature = new Dictionary<long, SiImage>();
        var imagesBySignature = new Dictionary<long, List<SiImage>>();

        foreach (var image in EnumerateImages(gameMap))
        {
            if (!imagesBySignature.TryGetValue(image.Signature, out var images))
            {
                images = new List<SiImage>();
                imagesBySignature[image.Signature] = images;
                firstImageBySignature[image.Signature] = image;
            }

            images.Add(image);
        }

        // Tallest-first shelf packing: simple, deterministic, and good enough for the small
        // (mostly 16-48px) quads found in practice.
        var packingOrder = firstImageBySignature.Values
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

        using (var bitmap = new Bitmap(canvasWidth, Math.Max(canvasHeight, 1)))
        {
            using (var graphics = Graphics.FromImage(bitmap))
            {
                foreach (var image in packingOrder)
                {
                    var spriteBitmap = gameMap.GetSpriteBitmap(image);
                    var (x, y) = positionBySignature[image.Signature];
                    graphics.DrawImage(spriteBitmap, x, y);
                }
            }

            bitmap.Save(fileName, ImageFormat.Png);
        }

        foreach (var (signature, position) in positionBySignature)
        {
            foreach (var image in imagesBySignature[signature])
            {
                image.AtlasX = position.X;
                image.AtlasY = position.Y;
            }
        }
    }

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