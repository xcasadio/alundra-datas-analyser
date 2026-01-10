using AlundraEngine.DatasBin;

namespace AlundraEngine;

public class RendererHelper
{
    private static readonly Font FontEntityId = new Font(FontFamily.GenericSansSerif, 9f);
    private static readonly Font FontTileInfo = new Font(FontFamily.GenericSansSerif, 5.5f);
    
    private static readonly Dictionary<FlippedBitmapKey, Bitmap> _flippedBitmapCache = new();
    private static readonly Color EffectColor = Color.DarkViolet;
    private static Color EntityColor = Color.Blue;
    private static Color EntitySelectColor = Color.ForestGreen;
    private static Color EffectSelectedColor = Color.LightSeaGreen;
    private static Color ZColor = Color.Green;
    private const int MaxFlippedBitmapCacheSize = 10000;

    //Custom renderer
    public static void Render(System.Drawing.Graphics g, GameEngine gameEngine, int currentRow, int camTileOffsetY)
    {
        DatasBin.DatasBin datasBin = gameEngine.DatasBin;
        GameMap gameMap = gameEngine.CurrentMap;
        
        var cameraX = gameEngine.StaticVariables.g_cameraScrollingX;
        var cameraY = gameEngine.StaticVariables.g_cameraScrollingY;
        var curXTile = cameraX / StaticVariables.MapTileWidth;
        var curYTile = 0;

        curXTile = Math.Max(0, curXTile);
        curXTile = Math.Min(gameMap.Map.Width, curXTile);

        for (var y = curYTile; y < gameMap.Map.Height; y++)
        {
            //draw tiles on this row
            for (var x = curXTile; x < curXTile + StaticVariables.ScreenWidth / StaticVariables.MapTileWidth + 2 && x < gameMap.Map.Width; x++)
            {
                var tile = gameMap.Map.MapTiles[y * gameMap.Map.Width + x];
                var tileId = tile.TileId;

                //render tile
                var dx = x * StaticVariables.MapTileWidth - cameraX;
                var dy = (y - tile.Height) * StaticVariables.MapTileHeight - cameraY;
                
                if (tile.TileId != 0xffff && gameEngine.StaticVariables.DisplayTiles)
                {
                    if (dy > -StaticVariables.MapTileHeight 
                        && dy < StaticVariables.ScreenHeight)
                    {
                        tileId = GetAnimatedTileId(gameEngine, gameMap, tileId);
                        var z = ((y - tile.Height) * StaticVariables.MapTileHeight) << 16;
                        DrawTile(tileId, dx, dy, z, g, gameMap, gameEngine.Renderer);

                        if (gameEngine.StaticVariables.DisplayTileXY)
                        {
                            var text = $"{x}x{y}";
                            var textSize = g.MeasureString(text, FontTileInfo);
                            gameEngine.Renderer.DrawString(text, FontTileInfo, Color.White, 
                                (int)(dx + (StaticVariables.MapTileWidth - textSize.Width) / 2f), 
                                (int)(dy + (StaticVariables.MapTileHeight - textSize.Height) / 2f), 
                                SpriteDepth.DebugCollision);
                        }
                    }
                }
                
                if (tile.WallTiles != null && gameEngine.StaticVariables.DisplayWallTiles)
                {
                    var wallTiles = tile.WallTiles;
                    int i;
                    dy -= wallTiles.Offset * StaticVariables.MapTileHeight;

                    for (i = 0; i < wallTiles.Count; i++)
                    {
                        dy += StaticVariables.MapTileHeight;
                        var wallTileId = wallTiles.Tiles[i];

                        if (wallTileId != 0xffff 
                            && dy > -StaticVariables.MapTileHeight 
                            && dy < StaticVariables.ScreenHeight)
                        {
                            wallTileId = GetAnimatedTileId(gameEngine, gameMap, wallTileId);
                            var z = (dy + cameraY) << 16;
                            DrawTile(wallTileId, dx, dy, z, g, gameMap, gameEngine.Renderer);

                            if (gameEngine.StaticVariables.DisplayTileXY)
                            {
                                var text = $"{x}x{y}";
                                var textSize = g.MeasureString(text, FontTileInfo);
                                gameEngine.Renderer.DrawString(text, FontTileInfo, Color.BurlyWood,
                                    (int)(dx + (StaticVariables.MapTileWidth - textSize.Width) / 2f),
                                    (int)(dy + (StaticVariables.MapTileHeight - textSize.Height) / 2f), SpriteDepth.DebugCollision);
                            }
                        }
                    }
                }
            }

            //DRAW ENTITIES
            for (var i = 0; i < gameEngine.StaticVariables.g_visibleEntityCount; i++)
            {
                var entity = gameEngine.StaticVariables.g_visibleEntities[i];

                if (entity.TileY != y)
                {
                    continue;//if its not in this row, continue
                }

                var scx = (entity.PosX >> 16) - cameraX;
                var scy = (entity.PosY >> 16) - (entity.PosZ >> 16) - cameraY;
                
                if (entity.SpriteRecord != null)
                {
                    //display entity
                    var map = entity.IsMapSprite ? gameMap : datasBin.AlundraGameMap;

                    if (entity.Frame?.Images != null)
                    {
                        var iset = entity.Frame.Images;
                        for (var idex = iset.NumberOfImages - 1; idex >= 0; idex--)
                        {
                            var img = iset.Images[idex];

                            var bmp = map.GetSpriteBitmap(img);
                            DrawSprite(bmp, img, scx, scy, entity.ZSortValue, gameEngine.Renderer); 

                            if (gameEngine.StaticVariables.DisplayEntitiesPosition)
                            {
                                //display z
                                var pz = (entity.FloorHeight - entity.ModdedPosZ) >> 16;
                                if (pz != 0)
                                {
                                    gameEngine.Renderer.DrawLine(scx, scy, scx, scy - pz, ZColor);
                                    gameEngine.Renderer.DrawCross(scx, scy - pz, SpriteDepth.DebugCollision, ZColor);
                                }

                                //display entity position
                                gameEngine.Renderer.DrawCross(scx, scy, SpriteDepth.DebugCollision, EntityColor);
                            }
                        }
                    }

                    if (gameEngine.StaticVariables.DisplayEntityId)
                    {
                        var color = gameEngine.StaticVariables.EditorSelectEntityIndex == entity.Index
                            ? EntitySelectColor
                            : EntityColor;
                        var text = $"#{entity.Index}";
                        var textSize = g.MeasureString(text, FontEntityId);
                        gameEngine.Renderer.DrawString(text, FontEntityId, color,
                            (int)(scx - textSize.Width / 2f),
                            scy,
                            SpriteDepth.DebugCollision);
                    }
                }
            }
        }

        //DRAW EFFECTS
        for (var i = 0; i < gameEngine.StaticVariables.g_effectSlots.Length; i++)
        {
            var effect = gameEngine.StaticVariables.g_effectSlots[i];
            
            if (effect.Status != 2)
            {
                continue;
            }

            var scx = (effect.X >> 16) - cameraX;
            var scy = ((effect.Y - effect.Z) >> 16) - cameraY;

            if (effect.SpriteRef.Images != null)
            {
                var map = effect.CurrentIsMapSprite == 1 ? gameMap : datasBin.AlundraGameMap;
                var spriteRef = effect.SpriteRef;

                for (var idex = spriteRef.NumberOfImages - 1; idex >= 0; idex--)
                {
                    var img = spriteRef.Images[idex];
                    //effect._24 set alpha
                    var bmp = map.GetSpriteBitmap(img);
                    DrawSprite(bmp, img, scx, scy, effect.DepthSortValue + (effect.DepthSortOffset << 16), gameEngine.Renderer, 1f);

                    if (gameEngine.StaticVariables.DisplayEffectsPosition)
                    {                                
                        //display z
                        //var pz = (effect.Z) >> 16;
                        //if (pz != 0)
                        //{
                        //    gameEngine.Renderer.DrawLine(scx, scy, scx, scy - pz, ZColor);
                        //    gameEngine.Renderer.DrawCross(scx, scy - pz, SpriteDepth.DebugCollision, ZColor);
                        //}

                        gameEngine.Renderer.DrawCross(scx, scy, SpriteDepth.DebugCollision, EffectColor);
                    }
                }
            }

            if (gameEngine.StaticVariables.DisplayEffectId)
            {
                var color = gameEngine.StaticVariables.EditorSelectEffectIndex == effect.Id
                    ? EffectSelectedColor
                    : EffectColor;
                var text = $"#{effect.Id}";
                var textSize = g.MeasureString(text, FontEntityId);
                gameEngine.Renderer.DrawString(text,
                    FontEntityId,
                    color,
                    (int)(scx - textSize.Width / 2f),
                    scy,
                    SpriteDepth.DebugCollision
                );
            }
        }
    }

    private static ushort GetAnimatedTileId(GameEngine gameEngine, GameMap gameMap, ushort tileId)
    {
        var tile = tileId & 0x3ff;

        if (tile >= gameEngine.StaticVariables.g_tileAnimDescriptorTable.Length)
        {
            return tileId;
        }

        var spriteIndex = gameEngine.StaticVariables.g_tileAnimDescriptorTable[tile].SpriteIndex;
        if (spriteIndex != 0)
        {
            var entry = gameMap.Info.SpriteMapEntries[spriteIndex];
            if (entry.Enabled == 1)
            {
                //var paletteId = tileId & 0xF000;
                //tileId = (ushort)(paletteId | (tile + entry.FrameIndex * entry.TileHeight));
                tileId += (ushort)(entry.FrameIndex * entry.TileHeight);
            }
        }

        return tileId;
    }

    private static void DrawSprite(Bitmap bitmap, SiImage img, int x, int y, int z, Renderer renderer, float alpha = 1f)
    {
        var w = img.X4 - img.X1;
        var h = img.Y4 - img.Y1;

        if (w == 0 || h == 0)
        {
            return;
        }

        var absW = Math.Abs(w);
        var absH = Math.Abs(h);
        var drawX = x + (w < 0 ? img.X4 : img.X1);
        var drawY = y + (h < 0 ? img.Y4 : img.Y1);

        var flipX = w < 0;
        var flipY = h < 0;

        if (flipX || flipY)
        {
            var cacheKey = new FlippedBitmapKey(bitmap.GetHashCode(), flipX, flipY, bitmap.Width, bitmap.Height);

            if (!_flippedBitmapCache.TryGetValue(cacheKey, out var flippedBitmap))
            {
                if (_flippedBitmapCache.Count >= MaxFlippedBitmapCacheSize)
                {
                    ClearFlippedBitmapCache();
                }

                flippedBitmap = new Bitmap(bitmap.Width, bitmap.Height);
                using (var g = System.Drawing.Graphics.FromImage(flippedBitmap))
                {
                    g.Clear(Color.Transparent);

                    if (flipX && flipY)
                    {
                        g.ScaleTransform(-1, -1);
                        g.TranslateTransform(-bitmap.Width, -bitmap.Height);
                    }
                    else if (flipX)
                    {
                        g.ScaleTransform(-1, 1);
                        g.TranslateTransform(-bitmap.Width, 0);
                    }
                    else if (flipY)
                    {
                        g.ScaleTransform(1, -1);
                        g.TranslateTransform(0, -bitmap.Height);
                    }

                    g.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
                }

                _flippedBitmapCache[cacheKey] = flippedBitmap;
            }

            renderer.AddSprite(drawX, drawY, absW, absH, z, flippedBitmap, alpha);
        }
        else
        {
            renderer.AddSprite(drawX, drawY, absW, absH, z, bitmap, alpha);
        }
    }

    private static void DrawTile(int tileMapIndex, int x, int y, int z, System.Drawing.Graphics g, GameMap gameMap, Renderer renderer)
    {
        var bmp = gameMap.GetTileBitmap(tileMapIndex);
        renderer.AddSprite(x, y, bmp.Width, bmp.Height, z, bmp);
    }

    public static void ClearFlippedBitmapCache()
    {
        foreach (var bitmap in _flippedBitmapCache.Values)
        {
            bitmap.Dispose();
        }
        _flippedBitmapCache.Clear();
    }

    private readonly record struct FlippedBitmapKey(
        int BitmapHashCode,
        bool FlipX,
        bool FlipY,
        int Width,
        int Height);
}
