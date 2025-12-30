using AlundraEngine;
using AlundraEngine.DatasBin;
using System.Drawing;

namespace AlundraDataExtractor;

record GameMapJson
{
    public GameMapInfoJson GameMapInfo { get; set; }
    public MapJson Map { get; set; }
    public SpriteInfoJson SpriteInfo { get; set; }
    public ScrollParametersJson ScrollParameters { get; set; }
    public string[] Strings { get; set; }

    public GameMapJson()
    {
    }

    public GameMapJson(GameMap gameMap)
    {
        GameMapInfo = new GameMapInfoJson(gameMap.Info);
        Map = new MapJson(gameMap.Map);
        SpriteInfo = new SpriteInfoJson(gameMap.SpriteInfo);
        ScrollParameters = new ScrollParametersJson(gameMap.ScrollParameters);
        Strings = gameMap.Strings;
    }

    public void SaveTileSheetBitmap(GameMap gameMap, string fileName, TileAnimDescriptor[] tileAnimDescriptors)
    {
        using var bitmap = new Bitmap(256, 256 * 8);
        using var graphics = Graphics.FromImage(bitmap);
        var tileCache = new HashSet<ushort>();

        foreach (var mapTile in gameMap.Map.MapTiles)
        {
            var tile = mapTile.TileId;

            if (tile != 0xffff)
            {
                if (tileCache.Add(tile))
                {
                    DrawAllAnimatedTiles(gameMap, tile, graphics, tileAnimDescriptors);
                }
            }

            if (mapTile.WallTiles != null)
            {
                foreach (var wallTile in mapTile.WallTiles.Tiles)
                {
                    if (wallTile != 0xffff && tileCache.Add(wallTile))
                    {
                        DrawAllAnimatedTiles(gameMap, wallTile, graphics, tileAnimDescriptors);
                    }
                }
            }
        }

        bitmap.Save(fileName);
    }

    public void SaveSpriteSheetBitmap(GameMap gameMap, string path, TileAnimDescriptor[] tileAnimDescriptors)
    {
        using var bitmap = new Bitmap(256, 256 * 8);
        using var graphics = Graphics.FromImage(bitmap);

    }

    private static void DrawAllAnimatedTiles(GameMap gameMap, ushort tileId, Graphics graphics, TileAnimDescriptor[] tileAnimDescriptors)
    {
        var tile = tileId & 0x3ff;

        var spriteIndex = tileAnimDescriptors[tile].SpriteIndex;

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
        var position = tileId & 0x3ff;
        var x = position % 10 * StaticVariables.MapTileWidth; //(tileIndex % (gameMap.TileSheetBitmap.Width / 16)) * 16;
        var y = position / 10 * StaticVariables.MapTileHeight; //(tileIndex / (gameMap.TileSheetBitmap.Width / 16)) * 16;
        graphics.DrawImage(tileBitmap, x, y);

        Console.WriteLine($"Drawn tileId {tileId} at ({x}, {y})");
    }
}