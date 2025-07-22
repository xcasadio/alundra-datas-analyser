using AlundraEngine.DatasBin;
using System.Drawing.Imaging;

namespace AlundraEngine;

public class RendererHelper
{
    private static readonly Font FontEntityId = new Font(FontFamily.GenericSansSerif, 9f);
    private static readonly Font FontTileInfo = new Font(FontFamily.GenericSansSerif, 5.5f);

    //Custom renderer
    public static void Render(Graphics g, DatasBin.DatasBin datasBin, GameMap gameMap, int currentRow, int camTileOffsetY)
    {
        var textToRender = new List<TextDisplayParameter>();

        var currentXPosition = StaticVariables.g_cameraScrollingX;
        var currentYPosition = StaticVariables.g_cameraScrollingY;

        if (StaticVariables.UseDebugCamera)
        {
            currentXPosition = StaticVariables.g_cameraCurrentX;
            currentYPosition = StaticVariables.g_cameraCurrentY;
        }

        var curXTile = currentXPosition / StaticVariables.MapTileWidth;
        var curYTile = 0;
        //var curXTile = currentRow;
        //var curYTile = camTileOffsetY;

        curXTile = Math.Max(0, curXTile);
        curXTile = Math.Min(gameMap.Map.Width, curXTile);

        var sinfo = gameMap.SpriteInfo;
        var gensi = datasBin.AlundraGameMap.SpriteInfo;

        for (var y = curYTile; y < gameMap.Map.Height ;y++)
        {
            //draw tiles on this row
            for (var x = curXTile; x < curXTile + StaticVariables.ScreenWidth / StaticVariables.MapTileWidth + 2; x++)
            {
                var tile = gameMap.Map.MapTiles[y * gameMap.Map.Width + x];
                var tileId = tile.TileId;

                //render tile
                var dx = x * StaticVariables.MapTileWidth - currentXPosition;
                var dy = (y - tile.Height) * StaticVariables.MapTileHeight - currentYPosition;

                if (dy > -StaticVariables.MapTileHeight && dy < StaticVariables.ScreenHeight && tile.TileId != 0xffff)
                {
                    tileId = GetAnimatedTileId(gameMap, tileId);
                    DrawTile(tileId, dx, dy, g, gameMap);

                    if (StaticVariables.DisplayTileXY)
                    {
                        var text = $"{x}x{y}";
                        var textSize = g.MeasureString(text, FontTileInfo);
                        textToRender.Add(new TextDisplayParameter
                        {
                            Text = text,
                            Font = FontTileInfo,
                            Color = Brushes.White,
                            X = dx + (StaticVariables.MapTileWidth + textSize.Width) / 2f,
                            Y = dy + (StaticVariables.MapTileHeight + textSize.Height) / 2f
                        });
                    }
                }

                if (tile.WallTiles != null)
                {
                    var wallTiles = tile.WallTiles;
                    int i;
                    dy -= wallTiles.Offset * StaticVariables.MapTileHeight;

                    for (i = 0; i < wallTiles.Count; i++)
                    {
                        dy += StaticVariables.MapTileHeight;
                        //render wall tile
                        var wallTileId = wallTiles.Tiles[i];

                        if (dy > -StaticVariables.MapTileHeight && dy < StaticVariables.ScreenHeight && wallTileId != 0xffff)
                        {
                            wallTileId = GetAnimatedTileId(gameMap, wallTileId);
                            DrawTile(wallTileId, dx, dy, g, gameMap);

                            if (StaticVariables.DisplayTileXY)
                            {
                                var text = $"{x}x{y}";
                                var textSize = g.MeasureString(text, FontTileInfo);

                                textToRender.Add(new TextDisplayParameter
                                {
                                    Text = text,
                                    Font = FontTileInfo,
                                    Color = Brushes.White,
                                    X = dx + (StaticVariables.MapTileWidth + textSize.Width) / 2f,
                                    Y = dy + (StaticVariables.MapTileHeight + textSize.Height) / 2f
                                });
                            }
                        }
                    }
                }
            }

            //draw sprites who are on this row
            for (var i = 0; i < StaticVariables.g_numberOfEntity; i++) // g_visibleEntityCount
            {
                var entity = StaticVariables.g_entitySlots[i]; // g_visibleEntities
                if (entity.Status == 5)
                {
                    continue;
                }

                if (entity.TileY != y)
                {
                    continue;//if its not in this row, continue
                }

                //var tile = selectedGame.map.maptiles[sx + sy * selectedGame.map.width];
                //var scx = (entity.ModdedXPos >> 16) - (currentRow * StaticVariables.MapTileWidth);
                //var scy = (entity.ModdedYPos >> 16) - (entity.ModdedZPos >> 16) - (camTileOffsetY * StaticVariables.MapTileHeight);
                var scx = (entity.ModdedXPos >> 16) - currentXPosition + StaticVariables.MapTileWidth / 2;
                var scy = (entity.ModdedYPos >> 16) - (entity.ModdedZPos >> 16) - currentYPosition + StaticVariables.MapTileHeight / 2;

                if (entity.Sprite != null)
                {
                    //display attached effect
                    if (entity.ActiveEffect?.Status == 2)
                    {
                        var effect = entity.ActiveEffect;

                        var eX = (effect.X >> 16) - currentXPosition;
                        var eY = (effect.Y >> 16) - (effect.Z >> 16) - currentYPosition;

                        //if (effect.Sprite != null)
                        {
                            var mapSprite = effect.CurrentIsMapSprite == 1 ? gameMap : datasBin.AlundraGameMap;

                            if (effect.Frame?.Images != null) // why?? TODO, not initialized when we load a dump?
                            {
                                var iset = effect.Frame.Images;
                                for (var idex = iset.NumberOfImages - 1; idex >= 0; idex--)
                                {
                                    var img = iset.Images[idex];
                                    DrawSprite(mapSprite, img, eX, eY, g, 0.5f);
                                }
                            }
                        }

                        if (StaticVariables.DisplayEffectId)
                        {
                            var brush = StaticVariables.EditorSelectEffectIndex == effect.Id
                                ? Brushes.LightSeaGreen
                                : Brushes.DarkViolet;
                            var text = $"#{effect.Id}";
                            var textSize = g.MeasureString(text, FontEntityId);

                            textToRender.Add(new TextDisplayParameter
                            {
                                Text = text,
                                Font = FontEntityId,
                                Color = brush,
                                X = scx - textSize.Width / 2,
                                Y = scy
                            });
                        }
                    }

                    //display entity
                    var map = entity.IsMapSprite ? gameMap : datasBin.AlundraGameMap;

                    if (entity.Frame?.Images != null) // why?? TODO, not initialized when we load a dump?
                    {
                        var iset = entity.Frame.Images;
                        for (var idex = iset.NumberOfImages - 1; idex >= 0; idex--)
                        {
                            var img = iset.Images[idex];
                            DrawSprite(map, img, scx, scy, g);
                        }
                    }

                    if (StaticVariables.DisplayEntityId)
                    {
                        var brush = StaticVariables.EditorSelectEntityIndex == entity.Index
                            ? Brushes.ForestGreen
                            : Brushes.Blue;
                        var text = $"#{entity.Index}";
                        var textSize = g.MeasureString(text, FontEntityId);

                        textToRender.Add(new TextDisplayParameter
                        {
                            Text = text,
                            Font = FontEntityId,
                            Color = brush,
                            X = scx - textSize.Width / 2,
                            Y = scy
                        });
                    }
                }
            }
        }

        for (var i = 0; i < StaticVariables.g_effectSlots.Length; i++)
        {
            var effect = StaticVariables.g_effectSlots[i];

            if (effect.Status != 2 || effect.AttachedEntity != null)
            {
                continue;
            }
            
            var scx = (effect.X >> 16) - currentXPosition;
            var scy = (effect.Y >> 16) - (effect.Z >> 16) - currentYPosition;
        
            //if (effect.Sprite != null)
            {
                var map = effect.CurrentIsMapSprite == 1 ? gameMap : datasBin.AlundraGameMap;
        
                if (effect.Frame?.Images != null) // why?? TODO, not initialized when we load a dump?
                {
                    var iset = effect.Frame.Images;
                    for (var idex = iset.NumberOfImages - 1; idex >= 0; idex--)
                    {
                        var img = iset.Images[idex];
                        DrawSprite(map, img, scx, scy, g, 0.8f);
                    }
                }
            }

            if (StaticVariables.DisplayEffectId)
            {
                var brush = StaticVariables.EditorSelectEffectIndex == effect.Id
                    ? Brushes.LightSeaGreen
                    : Brushes.DarkViolet;
                var text = $"#{effect.Id}";
                var textSize = g.MeasureString(text, FontEntityId);

                textToRender.Add(new TextDisplayParameter
                {
                    Text = text,
                    Font = FontEntityId,
                    Color = brush,
                    X = scx - textSize.Width / 2,
                    Y = scy
                });
            }
        }

        foreach (var textDisplayParameter in textToRender)
        {
            //background
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    g.DrawString(textDisplayParameter.Text, textDisplayParameter.Font, Brushes.Black, textDisplayParameter.X + dx, textDisplayParameter.Y + dy);
                }
            }

            g.DrawString(textDisplayParameter.Text, textDisplayParameter.Font, textDisplayParameter.Color, textDisplayParameter.X, textDisplayParameter.Y);
        }
    }

    private static ushort GetAnimatedTileId(GameMap gameMap, ushort tileId)
    {
        var tile = tileId & 0x3ff;

        var spriteIndex = StaticVariables.g_tileAnimDescriptorTable[tile].SpriteIndex;
        if (spriteIndex != 0)
        {
            var entry = gameMap.Info.SpriteMapEntries[spriteIndex];
            if (entry.Enabled == 1)
            {
                //tileY + 1
                tileId = (ushort)((tileId & 0xF000) | ((tileId + 10 * entry.FrameIndex * 2) & 0x03FF));
            }
        }

        return tileId;
    }

    private static void DrawSprite(GameMap gm, SiImage img, int x, int y, Graphics g, float alpha = 1f)
    {
        var bmp = gm.GetSpriteBitmap(img);
        var w = img.X4 - img.X1;
        var h = img.Y4 - img.Y1;

        if (w == 0 || h == 0)
        {
            return;
        }

        // Déterminer les dimensions et la position en tenant compte des valeurs négatives (miroir)
        var absW = Math.Abs(w);
        var absH = Math.Abs(h);
        var drawX = x + (w < 0 ? img.X4 : img.X1);
        var drawY = y + (h < 0 ? img.Y4 : img.Y1);

        // Déterminer les transformations de miroir
        var flipX = w < 0;
        var flipY = h < 0;

        // Si alpha est différent de 1.0, utiliser ImageAttributes pour la transparence
        if (Math.Abs(alpha - 1.0f) > 0.001f)
        {
            ColorMatrix cm = new ColorMatrix
            {
                Matrix33 = alpha
            };

            using ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            Rectangle destRect = new Rectangle(drawX, drawY, absW, absH);
            
            // Gérer les effets de miroir avec les transformations graphiques
            if (flipX || flipY)
            {
                var state = g.Save();
                
                // Appliquer les transformations de miroir
                if (flipX && flipY)
                {
                    g.ScaleTransform(-1, -1);
                    g.TranslateTransform(-(drawX * 2 + absW), -(drawY * 2 + absH));
                }
                else if (flipX)
                {
                    g.ScaleTransform(-1, 1);
                    g.TranslateTransform(-(drawX * 2 + absW), 0);
                }
                else if (flipY)
                {
                    g.ScaleTransform(1, -1);
                    g.TranslateTransform(0, -(drawY * 2 + absH));
                }

                g.DrawImage(bmp, destRect, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, imageAttributes);
                g.Restore(state);
            }
            else
            {
                g.DrawImage(bmp, destRect, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, imageAttributes);
            }
        }
        else
        {
            // Alpha = 1.0, pas besoin d'ImageAttributes
            if (flipX || flipY)
            {
                var state = g.Save();
                
                // Appliquer les transformations de miroir
                if (flipX && flipY)
                {
                    g.ScaleTransform(-1, -1);
                    g.TranslateTransform(-(drawX * 2 + absW), -(drawY * 2 + absH));
                }
                else if (flipX)
                {
                    g.ScaleTransform(-1, 1);
                    g.TranslateTransform(-(drawX * 2 + absW), 0);
                }
                else if (flipY)
                {
                    g.ScaleTransform(1, -1);
                    g.TranslateTransform(0, -(drawY * 2 + absH));
                }

                g.DrawImage(bmp, drawX, drawY, absW, absH);
                g.Restore(state);
            }
            else
            {
                g.DrawImage(bmp, drawX, drawY, absW, absH);
            }
        }
    }

    private static void DrawTile(int tileMapIndex, int x, int y, Graphics g, GameMap gameMap)
    {
        var bmp = gameMap.GetTileBitmap(tileMapIndex);
        g.DrawImage(bmp, x, y);
    }
}

public class TextDisplayParameter
{
    public string Text { get; set; }
    public Font Font { get; set; }
    public Brush Color { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
}