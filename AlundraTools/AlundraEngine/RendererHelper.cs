using AlundraEngine.DatasBin;

namespace AlundraEngine;

public class RendererHelper
{
    private static readonly Font FontEntityId = new Font(FontFamily.GenericSansSerif, 9f);
    private static readonly Font FontTileInfo = new Font(FontFamily.GenericSansSerif, 7f);
    
    private static readonly Color EffectColor = Color.DarkViolet;
    private static Color EntityColor = Color.Blue;
    private static Color EntitySelectColor = Color.ForestGreen;
    private static Color EffectSelectedColor = Color.LightSeaGreen;
    private static Color ZColor = Color.Green;

    const int STRIDE = 16;  
    const int WALL_BIAS = 7;
    const int FLOOR_MAX = 6;

    static int DepthFloor(int tileY, int floorSlot)
    {
        floorSlot = Math.Clamp(floorSlot, 0, FLOOR_MAX);
        return tileY * STRIDE + floorSlot;
    }

    static int DepthWallBlock(int baseTileY, int wallSlot)
    {
        wallSlot = Math.Clamp(wallSlot, 0, FLOOR_MAX);
        return baseTileY * STRIDE + WALL_BIAS + wallSlot;
    }

    public static void Render(System.Drawing.Graphics g, GameEngine gameEngine, int curXTile, int curYTile)
    {
        DatasBin.DatasBin datasBin = gameEngine.DatasBin;
        GameMap gameMap = gameEngine.CurrentMap;
        
        var cameraX = gameEngine.StaticVariables.g_cameraScrollingX;
        var cameraY = gameEngine.StaticVariables.g_cameraScrollingY;
        //var curXTile = cameraX / StaticVariables.MapTileWidth;
        //curXTile = Math.Max(0, curXTile);
        //curXTile = Math.Min(gameMap.Map.Width, curXTile);
        //var curYTile = 0;
        curXTile = 0;
        curYTile = 0;

        for (var y = curYTile; y < gameMap.Map.Height; y++)
        {
            for (var x = curXTile; x < gameMap.Map.Width; x++)
            {
                var tile = gameMap.Map.MapTiles[y * gameMap.Map.Width + x];
                var tileId = tile.TileId;

                var dx = x * StaticVariables.MapTileWidth - cameraX;
                var dy = (y - tile.Height) * StaticVariables.MapTileHeight - cameraY;
                
                if (tile.TileId != 0xffff && gameEngine.StaticVariables.DisplayTiles)
                {
                    if (dy > -StaticVariables.MapTileHeight 
                        && dy < StaticVariables.ScreenHeight)
                    {
                        tileId = GetAnimatedTileId(gameEngine, gameMap, tileId, out var height);
                        var z = (y * StaticVariables.MapTileHeight);
                        z = DepthFloor(y, 0);

                        DrawTile(tileId, dx, dy, z << 16, g, gameMap, gameEngine.Renderer);

                        if (gameEngine.StaticVariables.DisplayTileZ)
                        {
                            var textSize2 = g.MeasureString(z.ToString(), FontTileInfo);
                            gameEngine.Renderer.DrawString(z.ToString(), FontTileInfo, Color.Green,
                                (int)(dx + (StaticVariables.MapTileWidth - textSize2.Width) / 2f),
                                (int)(dy + (StaticVariables.MapTileHeight - textSize2.Height) / 2f),
                                SpriteDepth.DebugCollision);
                        }

                        if (gameEngine.StaticVariables.DisplayTileXY)
                        {
                            var text = $"{x},{y}";
                            var textSize = g.MeasureString(text, FontTileInfo);
                            gameEngine.Renderer.DrawString(text, FontTileInfo, Color.White, 
                                (int)(dx + (StaticVariables.MapTileWidth - textSize.Width) / 2f), 
                                (int)(dy + (StaticVariables.MapTileHeight / 2f) - textSize.Height), 
                                SpriteDepth.DebugCollision);
                        }
                    }
                }
                
                if (tile.WallTiles != null && gameEngine.StaticVariables.DisplayWallTiles)
                {
                    var wallTiles = tile.WallTiles;
                    int i;
                    dy -= wallTiles.Offset * StaticVariables.MapTileHeight;

                    var z = (y - tile.Height) * StaticVariables.MapTileHeight;

                    for (i = 0; i < wallTiles.Count; i++)
                    {
                        dy += StaticVariables.MapTileHeight;
                        var wallTileId = wallTiles.Tiles[i];

                        z = DepthWallBlock(y, 0);

                        if (wallTileId != 0xffff 
                            && dy > -StaticVariables.MapTileHeight 
                            && dy < StaticVariables.ScreenHeight)
                        {
                            wallTileId = GetAnimatedTileId(gameEngine, gameMap, wallTileId, out var height);
                            DrawTile(wallTileId, dx, dy, z << 16, g, gameMap, gameEngine.Renderer);

                            if (gameEngine.StaticVariables.DisplayWallTileZ)
                            {
                                var textSize = g.MeasureString(z.ToString(), FontTileInfo);
                                gameEngine.Renderer.DrawString(z.ToString(), FontTileInfo, Color.LawnGreen,
                                    (int)(dx + (StaticVariables.MapTileWidth - textSize.Width) / 2f),
                                    (int)(dy + (StaticVariables.MapTileHeight / 2f) - textSize.Height),
                                    SpriteDepth.DebugCollision);
                            }

                            if (gameEngine.StaticVariables.DisplayWallTileXY)
                            {
                                var text = $"{x},{y}";
                                var textSize = g.MeasureString(text, FontTileInfo);
                                gameEngine.Renderer.DrawString(text, FontTileInfo, Color.BurlyWood,
                                    (int)(dx + (StaticVariables.MapTileWidth - textSize.Width) / 2f),
                                    (int)(dy + (StaticVariables.MapTileHeight / 2f) - textSize.Height), SpriteDepth.DebugCollision);
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

    private static ushort GetAnimatedTileId(GameEngine gameEngine, GameMap gameMap, ushort tileId, out int height)
    {
        height = 0;
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
                height = entry.TileHeight;
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

        // Utiliser X1/Y1 comme position de base, w et h peuvent être négatifs pour le flip
        var drawX = x + img.X1;
        var drawY = y + img.Y1;

        renderer.AddSprite(drawX, drawY, w, h, z, bitmap, alpha);
    }

    private static void DrawTile(int tileMapIndex, int x, int y, int z, System.Drawing.Graphics g, GameMap gameMap, Renderer renderer)
    {
        var bmp = gameMap.GetTileBitmap(tileMapIndex);
        renderer.AddSprite(x, y, bmp.Width, bmp.Height, z, bmp);
    }
}
