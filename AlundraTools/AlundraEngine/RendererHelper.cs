using AlundraEngine.DatasBin;
using System.Drawing;

namespace AlundraEngine;

public class RendererHelper
{
    private static readonly Font FontEntityId = new Font(FontFamily.GenericSansSerif, 9f);
    private static readonly Font FontTileInfo = new Font(FontFamily.GenericSansSerif, 5.5f);

    //Custom renderer
    public static void Render(Graphics g, DatasBin.DatasBin datasBin, GameMap gameMap, int currentRow, int camTileOffsetY)
    {
        var textToRender = new List<TextDisplayParameter>();

        var currentXPosition = StaticVariables.g_cameraCurrentX;// >> 16;
        var currentYPosition = StaticVariables.g_cameraCurrentY;// >> 16;
        //var currentXPosition = 0;
        //var currentYPosition = 0;
                     
        var curXTile = currentXPosition / StaticVariables.MapTileWidth;
        var curYTile = 0;
        //var curXTile = currentRow;
        //var curYTile = camTileOffsetY;

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
                //var dx = (x - currentRow) * StaticVariables.MapTileWidth - currentXPosition;
                //var dy = (y - tile.SizeZ - camTileOffsetY) * StaticVariables.MapTileHeight - currentYPosition;
                var dx = x * StaticVariables.MapTileWidth - currentXPosition;
                var dy = (y - tile.Height) * StaticVariables.MapTileHeight - currentYPosition;

                if (dy > -StaticVariables.MapTileHeight && dy < StaticVariables.ScreenHeight && tile.TileId != -1)
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

                        if (dy > -StaticVariables.MapTileHeight && dy < StaticVariables.ScreenHeight && wallTileId != -1)
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

    private static short GetAnimatedTileId(GameMap gameMap, short tileId)
    {
        var tile = tileId & 0x3ff;

        var spriteIndex = StaticVariables.g_tileAnimDescriptorTable[tile].SpriteIndex;
        if (spriteIndex != 0)
        {
            var entry = gameMap.Info.SpriteMapEntries[spriteIndex];
            if (entry.Enabled == 1)
            {
                //tileY + 1
                tileId = (short)( (tileId & 0xF000) | ((tileId + 10 * entry.FrameIndex * 2) & 0x03FF));
            }
        }

        return tileId;
    }

    private static void DrawSprite(GameMap gm, SiImage img, int x, int y, Graphics g)
    {
        var bmp = gm.GetSpriteBitmap(img);
        //_pnts[0].X = x + img.X1;
        //_pnts[0].Y = y + img.Y1;
        //
        //_pnts[1].X = x + img.X2;
        //_pnts[1].Y = y + img.Y2;
        //
        //_pnts[2].X = x + img.X3;
        //_pnts[2].Y = y + img.Y3;
        //
        //_pnts[3].X = x + img.X4;
        //_pnts[3].Y = y + img.Y4;
        //
        //g.DrawImage(bmp, _pnts);

        var w = img.X4 - img.X1;
        var h = img.Y4 - img.Y1;
        if (w != 0 && h != 0)
        {
            g.DrawImage(bmp, x + img.X1, y + img.Y1, w, h);
        }

        //var rectangle = new Rectangle(
        //    x + Math.Min(img.X1, Math.Min(img.X2, Math.Min(img.X3, img.X4))), 
        //    y + Math.Min(img.Y1, Math.Min(img.Y2, Math.Min(img.Y3, img.Y4))), 
        //    x+ Math.Max(img.X1, Math.Max(img.X2, Math.Max(img.X3, img.X4))), 
        //    y + Math.Max(img.Y1, Math.Max(img.Y2, Math.Max(img.Y3, img.Y4))));
        //
        //g.DrawImage(bmp, rectangle);
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