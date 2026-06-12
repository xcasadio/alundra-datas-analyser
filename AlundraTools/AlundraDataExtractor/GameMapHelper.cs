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

    public static void SaveSpriteSheet(GameMap gameMap, string fileName)
    {
        using var bitmap = new Bitmap(256, 256 * 8);
        using var graphics = Graphics.FromImage(bitmap);

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
                            var spriteBitmap = gameMap.GetSpriteBitmap(image);
                            var x = image.Sx;
                            var y = (image.Spritesheet & 0x7) * 256 + image.Sy;
                            graphics.DrawImage(spriteBitmap, x, y);
                        }
                    }
                }
            }
        }

        bitmap.Save(fileName, ImageFormat.Png);
    }
}