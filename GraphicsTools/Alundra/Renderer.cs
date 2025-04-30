using Alundra.DatasBin;

namespace Alundra;

public class Renderer
{
    public static void Render(Graphics g, DatasBin.DatasBin datasBin, GameMap gameMap)
    {
        var curxpos = StaticVariables.g_cameraCurrentX;// >> 16;
        var curypos = StaticVariables.g_cameraCurrentY;// >> 16;

        var curxtile = curxpos / StaticVariables.MapTileWidth;

        var sinfo = gameMap.SpriteInfo;
        var gensi = datasBin.AlundraGameMap.SpriteInfo;

        for (var y = 0; y < StaticVariables.g_mapLimits; y++)
        {
            //draw tiles on this row
            for (var x = curxtile; x < curxtile + StaticVariables.ScreenWidth / StaticVariables.MapTileWidth + 2; x++)
            {
                //StaticVariables.g_mapLimits
                var tile = gameMap.Map.MapTiles[y * 52 + x];
                //render tile
                var dx = x * StaticVariables.MapTileWidth - curxpos;
                var dy = (y - tile.Height) * StaticVariables.MapTileHeight - curypos;

                if (dy > -StaticVariables.MapTileHeight && dy < StaticVariables.ScreenHeight && tile.TileId != -1)
                {
                    DrawTile(tile.TileId, dx, dy, g, gameMap);
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
                        if (dy > -StaticVariables.MapTileHeight && dy < StaticVariables.ScreenHeight && wallTiles.Tiles[i] != -1)
                        {
                            DrawTile(wallTiles.Tiles[i], dx, dy, g, gameMap);
                        }
                    }
                }
            }

            //draw sprites who are on this row
            for (var i = 0; i < StaticVariables.g_numberOfEntity; i++)
            {
                var entity = StaticVariables.g_entitySlots[i];
                if (entity.Status == 5)
                {
                    continue;
                }

                if (entity.TileY != y)
                {
                    continue;//if its not in this row, continue
                }

                //var tile = selectedGame.map.maptiles[sx + sy * selectedGame.map.width];
                var scx = (entity.ModdedXPos >> 16) - curxpos;
                var scy = (entity.ModdedYPos >> 16) - (entity.ModdedZPos >> 16) - curypos;

                if (entity.Sprite != null)
                {
                    int idex;

                    var map = entity.IsMapSprite ? gameMap : datasBin.AlundraGameMap;

                    if (entity.Frame == null) // why?? TODO, not initialized ?
                    {
                        continue;
                    }

                    var iset = entity.Frame.Images;
                    for (idex = iset.NumberOfImages - 1; idex >= 0; idex--)
                    {
                        var img = iset.Images[idex];
                        DrawSprite(map, img, scx, scy, g);
                    }
                }
            }
        }
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

        var rectangle = new Rectangle(
            x + Math.Min(img.X1, Math.Min(img.X2, Math.Min(img.X3, img.X4))), 
            y + Math.Min(img.Y1, Math.Min(img.Y2, Math.Min(img.Y3, img.Y4))), 
            x+ Math.Max(img.X1, Math.Max(img.X2, Math.Max(img.X3, img.X4))), 
            y + Math.Max(img.Y1, Math.Max(img.Y2, Math.Max(img.Y3, img.Y4))));

        g.DrawImage(bmp, rectangle);
        //g.DrawImage(bmp, _pnts);
    }

    private static void DrawTile(int tileid, int x, int y, Graphics g, GameMap gameMap)
    {
        var bmp = gameMap.GetTileBitmap(tileid);
        g.DrawImage(bmp, x, y);
    }
}