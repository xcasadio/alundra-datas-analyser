using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;
using AlundraEngine.UI;

namespace AlundraEngine.Graphics;

public class GraphicManager
{
    private static readonly Font FontEntityId = new Font(FontFamily.GenericSansSerif, 9f);
    private static readonly Font FontTileInfo = new Font(FontFamily.GenericSansSerif, 7f);

    private static readonly Color EffectColor = Color.DarkViolet;
    private static Color EntityColor = Color.Blue;
    private static Color EntitySelectColor = Color.ForestGreen;
    private static Color EffectSelectedColor = Color.LightSeaGreen;
    private static Color ZColor = Color.Green;


    private readonly GameEngine _gameEngine;

    public GraphicManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    // 8002bd60
    public void RenderScene()
    {
        byte localScratchpad = 0;
        _gameEngine.StaticVariables.g_unusedByteArray = localScratchpad;

        _gameEngine.StaticVariables.g_numberOfTilesDrawn = RenderTiles(
            /*_gameEngine.StaticVariables.g_orderingTableBuffer[4]*/ null,
            _gameEngine.StaticVariables.g_cameraLookAtX, _gameEngine.StaticVariables.g_cameraLookAtY, _gameEngine.StaticVariables.g_cameraLookAtZ);

        _gameEngine.StaticVariables.g_numberOfEntitiesDrawn = RenderEntities(_gameEngine.StaticVariables.g_orderingTableBuffer[3], _gameEngine.StaticVariables.g_cameraScrollingX, _gameEngine.StaticVariables.g_cameraScrollingY);

        if (_gameEngine.StaticVariables.g_debugState < 0 && (_gameEngine.StaticVariables.g_debugFlags & 0x40) != 0)
        {
            _gameEngine.StaticVariables.g_numberOfLayersDrawn = 0;
        }
        else
        {
            _gameEngine.StaticVariables.g_numberOfLayersDrawn = RenderAllTileLayers(
                //_gameEngine.StaticVariables.g_orderingTableBuffer, _gameEngine.StaticVariables.g_orderingTableBuffer,
                null, null,
                    _gameEngine.StaticVariables.g_cameraScrollingX, _gameEngine.StaticVariables.g_cameraScrollingY);
        }

        DisplayDebugCollisionRectangle(_gameEngine.StaticVariables.g_orderingTableBuffer[2]);
        RenderTransitionEffects(_gameEngine.StaticVariables.g_orderingTableBuffer[3]);
        _gameEngine.MemoryCardManager.UpdateMemoryCardProcess();
        UpdateUserInterface();
        _gameEngine.StaticVariables.g_primitive_sync = DisplayUserInterface();

        _gameEngine.Renderer.Render();
        _gameEngine.Renderer.Clear();
    }

    // 8002cda0
    private int RenderTiles(int[] renderListBase, int offsetX, int offsetY, int offsetZ)
    {
        ResetTileAnimationState();

        if (_gameEngine.StaticVariables.g_isCameraScrolling == 0)
        {
            _gameEngine.StaticVariables.g_cameraScrollingX = _gameEngine.StaticVariables.g_cameraScrollingX 
                                                             + (offsetX - (_gameEngine.StaticVariables.g_cameraScrollingX + 0xa0) >> 4) 
                                                             + _gameEngine.StaticVariables.g_scrollingParameters.OffsetX 
                                                             + _gameEngine.StaticVariables.g_cameraDebugOffsetX;

            _gameEngine.StaticVariables.g_cameraScrollingY = _gameEngine.StaticVariables.g_cameraScrollingY 
                                                             + (offsetY - offsetZ - (_gameEngine.StaticVariables.g_cameraScrollingY + 0x88) >> 4) 
                                                             + _gameEngine.StaticVariables.g_scrollingParameters.OffsetY 
                                                             + _gameEngine.StaticVariables.g_cameraDebugOffsetY;
        }
        else
        {
            _gameEngine.StaticVariables.g_isCameraScrolling = 0;
            _gameEngine.StaticVariables.g_cameraScrollingY = offsetY - offsetZ - 0x88;
            _gameEngine.StaticVariables.g_cameraScrollingX = offsetX - 0xa0;
        }

        _gameEngine.StaticVariables.g_cameraDebugOffsetX = 0;
        _gameEngine.StaticVariables.g_cameraDebugOffsetY = 0;

        if (_gameEngine.StaticVariables.g_cameraScrollingX < 0)
        {
            _gameEngine.StaticVariables.g_cameraScrollingX = 0;
        }
        else if (0x39f < _gameEngine.StaticVariables.g_cameraScrollingX)
        {
            _gameEngine.StaticVariables.g_cameraScrollingX = 0x39f;
        }

        var nbColumns = 0xf;
        var newCamRow = _gameEngine.StaticVariables.g_cameraScrollingX / StaticVariables.MapTileWidth;
        var col = _gameEngine.StaticVariables.g_cameraScrollingX % StaticVariables.MapTileWidth; // divide by 15, approximation

        if (col < StaticVariables.MapTileHeight)
        {
            nbColumns = 0xe;
        }

        if (_gameEngine.StaticVariables.g_cameraScrollingY < 0)
        {
            _gameEngine.StaticVariables.g_cameraScrollingY = 0;
        }
        else if (0x2cf < _gameEngine.StaticVariables.g_cameraScrollingY)
        {
            _gameEngine.StaticVariables.g_cameraScrollingY = 0x2cf;
        }

        var currentRow = _gameEngine.StaticVariables.g_cameraScrollingY;

        if (_gameEngine.StaticVariables.g_cameraScrollingY < 0)
        {
            currentRow = _gameEngine.StaticVariables.g_cameraScrollingY + 0xf;
        }

        currentRow /= StaticVariables.MapTileHeight;
        var camTileOffsetY = (short)_gameEngine.StaticVariables.g_cameraScrollingY - (short)currentRow * StaticVariables.MapTileWidth;

        //i = 0x3bf;
        //tileOffset = g_tileOTFlags + 0x3bf;
        //do
        //{
        //    *tileOffset = 0;
        //    i = i + -1;
        //    tileOffset = tileOffset + -1;
        //} while (-1 < i);

        //Map animation
        //8002cfd8
        for (int i = 0; i < 6; i++)
        {
            var spriteMapEntry = _gameEngine.CurrentMap.Info.SpriteMapEntries[i];

            if (spriteMapEntry.Enabled == 0)
            {
                continue;
            }

            spriteMapEntry.Tick++;

            if (spriteMapEntry.FrameDuration <= spriteMapEntry.Tick)
            {
                spriteMapEntry.Index += spriteMapEntry.TileHeight;
                spriteMapEntry.Tick = 0;
                spriteMapEntry.FrameIndex++;

                if (spriteMapEntry.NumberOfFrame <= spriteMapEntry.FrameIndex)
                {
                    spriteMapEntry.Index = 0;
                    spriteMapEntry.FrameIndex = 0;
                }
            }
        }

        int layerFlag = 0;

        if (_gameEngine.StaticVariables.g_debugState < 0)
        {
            layerFlag = (int)(_gameEngine.StaticVariables.g_debugFlags >> 7);
        }

        if (layerFlag != _gameEngine.StaticVariables.g_LoadVRAMAssets_debug)
        {
            //load text to render tile information
            //_gameEngine.LoadVRAMAssets();
        }

        if ((_gameEngine.StaticVariables.g_debugState & 0x80000000U) == 0)
        {
            _gameEngine.StaticVariables.g_renderTileRowCount = 0x3c;
        }
        else
        {
            _gameEngine.StaticVariables.g_renderTileRowCount = _gameEngine.StaticVariables.g_mapLimits;
        }

        int maxTileSprite = 0;
        int visibleTileCount = 0;
        //var tileAnimFramIndex = (_gameEngine.StaticVariables.g_tileAnimFrameCounter & 1U) * 0x2a8;
        //var puVar4 = _gameEngine.StaticVariables.INT_ARRAY_800e0758[(_gameEngine.StaticVariables.g_tileAnimFrameCounter & 1U) * 0xd48];

        RenderTiles(newCamRow, col);

        if (visibleTileCount >= 599)
        {
            _gameEngine.StaticVariables.g_map_sprite = _gameEngine.StaticVariables.g_currentMap;
            //PrintInfo("MAP SPRT OVER!! : %d", g_currentMap);
        }

        if (maxTileSprite >= 199)
        {
            _gameEngine.StaticVariables.g_dr_tpage = _gameEngine.StaticVariables.g_currentMap;
            //PrintInfo("MAP DR_TPAGE OVER!! : %d", g_currentMap);
        }

        _gameEngine.StaticVariables.g_tileAnimFrameCounter++;

        return (maxTileSprite << 16) | visibleTileCount;
    }

    private void RenderTiles(int row, int column)
    {
        DatasBin.DatasBin datasBin = _gameEngine.DatasBin;
        GameMap gameMap = _gameEngine.CurrentMap;

        var cameraX = _gameEngine.StaticVariables.g_cameraScrollingX;
        var cameraY = _gameEngine.StaticVariables.g_cameraScrollingY;
        //var curXTile = cameraX / StaticVariables.MapTileWidth;
        //curXTile = Math.Max(0, curXTile);
        //curXTile = Math.Min(gameMap.Map.Width, curXTile);
        //var curYTile = 0;
        var curXTile = 0;
        var curYTile = 0;

        for (var y = curYTile; y < gameMap.Map.Height; y++)
        {
            for (var x = curXTile; x < gameMap.Map.Width; x++)
            {
                var tile = gameMap.Map.MapTiles[y * gameMap.Map.Width + x];
                var tileId = tile.TileId;

                var dx = x * StaticVariables.MapTileWidth - cameraX;
                var dy = (y - tile.Height) * StaticVariables.MapTileHeight - cameraY;

                if (tile.TileId != 0xffff && _gameEngine.StaticVariables.DisplayTiles)
                {
                    if (dy > -StaticVariables.MapTileHeight
                        && dy < StaticVariables.ScreenHeight)
                    {
                        tileId = GetAnimatedTileId(_gameEngine, gameMap, tileId, out var height);
                        var z = y * StaticVariables.MapTileHeight;
                        z = DepthFloor(y, 0);

                        DrawTile(tileId, dx, dy, z << 16, gameMap, _gameEngine.Renderer);

                        if (_gameEngine.StaticVariables.DisplayTileZ)
                        {
                            _gameEngine.Renderer.DrawCenterString(z.ToString(), FontTileInfo, Color.Green,
                                (int)(dx + StaticVariables.MapTileWidth / 2f),
                                (int)(dy + StaticVariables.MapTileHeight / 2f),
                                SpriteDepth.DebugCollision);
                        }

                        if (_gameEngine.StaticVariables.DisplayTileXY)
                        {
                            var text = $"{x},{y}";
                            _gameEngine.Renderer.DrawCenterString(text, FontTileInfo, Color.White,
                                (int)(dx + StaticVariables.MapTileWidth / 2f),
                                (int)(dy + StaticVariables.MapTileHeight / 2f),
                                SpriteDepth.DebugCollision);
                        }
                    }
                }

                if (tile.WallTiles != null && _gameEngine.StaticVariables.DisplayWallTiles)
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
                            wallTileId = GetAnimatedTileId(_gameEngine, gameMap, wallTileId, out var height);
                            DrawTile(wallTileId, dx, dy, z << 16, gameMap, _gameEngine.Renderer);

                            if (_gameEngine.StaticVariables.DisplayWallTileZ)
                            {
                                _gameEngine.Renderer.DrawCenterString(z.ToString(), FontTileInfo, Color.LawnGreen,
                                    (int)(dx + StaticVariables.MapTileWidth / 2f),
                                    (int)(dy + StaticVariables.MapTileHeight / 2f),
                                    SpriteDepth.DebugCollision);
                            }

                            if (_gameEngine.StaticVariables.DisplayWallTileXY)
                            {
                                var text = $"{x},{y}";
                                _gameEngine.Renderer.DrawString(text, FontTileInfo, Color.BurlyWood,
                                    (int)(dx + (StaticVariables.MapTileWidth) / 2f),
                                    (int)(dy + StaticVariables.MapTileHeight / 2f), SpriteDepth.DebugCollision);
                            }
                        }
                    }
                }
            }
        }

    }

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

    // PSX: otIndex = depthSortValue >> 0x14, clamp to 0x3B, then otTable[otIndex * 0x10 + 6]
    const int ENTITY_SLOT = 6;  // Entities are between floors (0-6) and walls (7+)
    static int DepthEntity(int depthSortValue)
    {
        int otIndex = depthSortValue >> 20;  // >> 0x14
        if (otIndex > 0x3B) otIndex = 0x3B;  // Clamp to 59
        return otIndex * STRIDE + ENTITY_SLOT;
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

    private static void DrawTile(int tileMapIndex, int x, int y, int z, GameMap gameMap, IRenderer renderer)
    {
        var bmp = gameMap.GetTileBitmap(tileMapIndex);
        renderer.AddSprite(x, y, bmp.Width, bmp.Height, z, bmp);
    }

    //8002c894
    private void ResetTileAnimationState()
    {
        if (_gameEngine.StaticVariables.g_scrollingParameters.Flag == 0)
        {
            _gameEngine.StaticVariables.g_scrollingParameters.LimitY = 0;
            _gameEngine.StaticVariables.g_scrollingParameters.LimitX = 0;
            _gameEngine.StaticVariables.g_scrollingParameters.SpeedY = 0;
            _gameEngine.StaticVariables.g_scrollingParameters.SpeedX = 0;
            _gameEngine.StaticVariables.g_scrollingParameters.OffsetY = 0;
            _gameEngine.StaticVariables.g_scrollingParameters.OffsetX = 0;
        }
        else
        {
            if (_gameEngine.StaticVariables.g_scrollingParameters.LimitX == 0 || _gameEngine.StaticVariables.g_scrollingParameters.SpeedX == 0)
            {
                _gameEngine.StaticVariables.g_scrollingParameters.OffsetX = 0;
            }
            else if (_gameEngine.StaticVariables.g_scrollingParameters.XReachMin == 0)
            {
                _gameEngine.StaticVariables.g_scrollingParameters.OffsetX -= _gameEngine.StaticVariables.g_scrollingParameters.SpeedX;
                if (_gameEngine.StaticVariables.g_scrollingParameters.OffsetX <= -_gameEngine.StaticVariables.g_scrollingParameters.LimitX)
                {
                    _gameEngine.StaticVariables.g_scrollingParameters.XReachMin = 1;
                    _gameEngine.StaticVariables.g_scrollingParameters.OffsetX = -_gameEngine.StaticVariables.g_scrollingParameters.LimitX;
                }
            }
            else
            {
                _gameEngine.StaticVariables.g_scrollingParameters.OffsetX += _gameEngine.StaticVariables.g_scrollingParameters.SpeedX;
                if (_gameEngine.StaticVariables.g_scrollingParameters.OffsetX >= _gameEngine.StaticVariables.g_scrollingParameters.LimitX)
                {
                    _gameEngine.StaticVariables.g_scrollingParameters.OffsetX = _gameEngine.StaticVariables.g_scrollingParameters.LimitX;
                    _gameEngine.StaticVariables.g_scrollingParameters.XReachMin = 0;
                }
            }

            if (_gameEngine.StaticVariables.g_scrollingParameters.LimitY == 0)
            {
                _gameEngine.StaticVariables.g_scrollingParameters.OffsetY = 0;
            }
            else if (_gameEngine.StaticVariables.g_scrollingParameters.YReachMin == 0)
            {
                _gameEngine.StaticVariables.g_scrollingParameters.OffsetY -= _gameEngine.StaticVariables.g_scrollingParameters.SpeedY;
                if (_gameEngine.StaticVariables.g_scrollingParameters.OffsetY <= -_gameEngine.StaticVariables.g_scrollingParameters.LimitY)
                {
                    _gameEngine.StaticVariables.g_scrollingParameters.YReachMin = 1;
                    _gameEngine.StaticVariables.g_scrollingParameters.OffsetY = -_gameEngine.StaticVariables.g_scrollingParameters.LimitY;
                }
            }
            else
            {
                _gameEngine.StaticVariables.g_scrollingParameters.OffsetY += _gameEngine.StaticVariables.g_scrollingParameters.SpeedY;
                if (_gameEngine.StaticVariables.g_scrollingParameters.OffsetY >= _gameEngine.StaticVariables.g_scrollingParameters.LimitY)
                {
                    _gameEngine.StaticVariables.g_scrollingParameters.OffsetY = _gameEngine.StaticVariables.g_scrollingParameters.LimitY;
                    _gameEngine.StaticVariables.g_scrollingParameters.YReachMin = 0;
                }
            }
        }
    }

    //8002e130
    private int RenderEntities(int orderingTable, int cameraX, int cameraY)
    {
        //DRAW ENTITIES
        for (var i = 0; i < _gameEngine.StaticVariables.g_visibleEntityCount; i++)
        {
            var entity = _gameEngine.StaticVariables.g_visibleEntities[i];
        
            //if (entity.TileY != y)
            //{
            //    continue;//if its not in this row, continue
            //}
        
            var scx = (entity.PosX >> 16) - cameraX;
            var scy = (entity.PosY >> 16) - (entity.PosZ >> 16) - cameraY;
            
            if (entity.SpriteRecord != null)
            {
                //display entity
                var map = entity.IsMapSprite ? _gameEngine.CurrentMap : _gameEngine.DatasBin.AlundraGameMap;
        
                if (entity.SpriteRef?.Images != null)
                {
                    // Use ZSortValue directly for sorting - higher values = rendered later (in front)
                    var entityZ = entity.ZSortValue;
                    for (var idex = entity.SpriteRef.NumberOfImages - 1; idex >= 0; idex--)
                    {
                        var img = entity.SpriteRef.Images[idex];
        
                        var bmp = map.GetSpriteBitmap(img);
                        DrawSprite(bmp, img, scx, scy, entityZ, _gameEngine.Renderer); 
        
                        if (_gameEngine.StaticVariables.DisplayEntitiesPosition)
                        {
                            //display z
                            var pz = (entity.FloorHeight - entity.ModdedPosZ) >> 16;
                            if (pz != 0)
                            {
                                _gameEngine.Renderer.DrawLine(scx, scy, scx, scy - pz, ZColor);
                                _gameEngine.Renderer.DrawCross(scx, scy - pz, SpriteDepth.DebugCollision, ZColor);
                            }
        
                            //display entity position
                            _gameEngine.Renderer.DrawCross(scx, scy, SpriteDepth.DebugCollision, EntityColor);
                        }
                    }
                }
        
                if (_gameEngine.StaticVariables.DisplayEntityId)
                {
                    var color = _gameEngine.StaticVariables.EditorSelectEntityIndex == entity.Index
                        ? EntitySelectColor
                        : EntityColor;
                    var text = $"#{entity.Index}";
                    _gameEngine.Renderer.DrawString(text, FontEntityId, color, scx, scy, SpriteDepth.DebugCollision);
                }
            }
        }

        //DRAW EFFECTS
        for (var i = 0; i < _gameEngine.StaticVariables.g_effectSlots.Length; i++)
        {
            var effect = _gameEngine.StaticVariables.g_effectSlots[i];

            if (effect.Status != 2)
            {
                continue;
            }

            var scx = (effect.X >> 16) - cameraX;
            var scy = ((effect.Y - effect.Z) >> 16) - cameraY;

            if (effect.SpriteRef.Images != null)
            {
                var map = effect.CurrentIsMapSprite == 1 ? _gameEngine.CurrentMap : _gameEngine.DatasBin.AlundraGameMap;
                var spriteRef = effect.SpriteRef;
                // Use DepthSortValue directly for sorting - higher values = rendered later (in front)
                var effectZ = effect.DepthSortValue;

                for (var idex = spriteRef.NumberOfImages - 1; idex >= 0; idex--)
                {
                    var img = spriteRef.Images[idex];
                    //effect._24 set alpha
                    var bmp = map.GetSpriteBitmap(img);
                    DrawSprite(bmp, img, scx, scy, effectZ, _gameEngine.Renderer, 1f);

                    if (_gameEngine.StaticVariables.DisplayEffectsPosition)
                    {
                        //display z
                        //var pz = (effect.Z) >> 16;
                        //if (pz != 0)
                        //{
                        //    gameEngine.Renderer.DrawLine(scx, scy, scx, scy - pz, ZColor);
                        //    gameEngine.Renderer.DrawCross(scx, scy - pz, SpriteDepth.DebugCollision, ZColor);
                        //}

                        _gameEngine.Renderer.DrawCross(scx, scy, SpriteDepth.DebugCollision, EffectColor);
                    }
                }
            }

            if (_gameEngine.StaticVariables.DisplayEffectId)
            {
                var color = _gameEngine.StaticVariables.EditorSelectEffectIndex == effect.Id
                    ? EffectSelectedColor
                    : EffectColor;
                var text = $"#{effect.Id}";
                _gameEngine.Renderer.DrawString(text, FontEntityId, color, scx, scy, SpriteDepth.DebugCollision
                );
            }
        }

        return 0;
    }

    private static void DrawSprite(Bitmap bitmap, SiImage img, int x, int y, int z, IRenderer renderer, float alpha = 1f)
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

        // Extract blending info from Spritesheet byte
        // Bit 3 (& 0x8): Semi-transparency enabled
        // Bits 4-5 (& 0x30): Blending mode ABR (0-3)
        var blendMode = BlendMode.None;
        if ((img.Spritesheet & 0x8) != 0)
        {
            blendMode = (BlendMode)((img.Spritesheet & 0x30) >> 4);
        }

        renderer.AddSprite(drawX, drawY, w, h, z, bitmap, alpha, 1f, 1f, 1f, blendMode);
    }

    //80044c5c
    private int DisplayUserInterface()
    {
        //u_long uVar1;
        // DR_MODE *primitiveStart;
        // DR_MODE *pDVar2;
        // uint *primitiveChain;
        // int iVar3;
        // uint newBufferIndex;
        // uint *primitiveEnd;
        // DISPENV displayEnv;
        // DR_AREA drawArea;
        // 
        // GetDispEnv(&displayEnv);
        // SetDrawArea(&drawArea,&displayEnv.disp);
        FUN_800481f8();
        _gameEngine.MainInventoryManager.DisplayInventoryCharacterPortrait(/*_gameEngine.StaticVariables.DAT_80146f64[g_drawModes[0x14].tag * 0x28]*/); //SPRT ??
        // uVar1 = g_drawModes[0x14].tag;
        // primitiveStart = g_drawModes + g_drawModes[0x14].tag * 10 + 4;
        // iVar3 = g_drawModes[0x14].tag * 0x28;
        // primitiveChain = (uint *)(&DAT_80146f68 + iVar3);
        // primitiveEnd = (uint *)(&DAT_80146f64 + iVar3);
        //                   /* Probable PsyQ macro: addPrim(). */
        // primitiveStart.tag = primitiveStart.tag & 0xff000000 | *primitiveChain & 0xffffff;
        // pDVar2 = g_drawModes + g_drawModes[0x14].tag * 10 + 3;
        // *primitiveChain = *primitiveChain & 0xff000000 | (uint)primitiveStart & 0xffffff;
        // primitiveChain = (uint *)((int)g_drawModes + iVar3 + 0xf8);
        //                   /* Probable PsyQ macro: addPrim(). */
        // pDVar2.tag = pDVar2.tag & 0xff000000 | *primitiveEnd & 0xffffff;
        // primitiveStart = g_drawModes + g_drawModes[0x14].tag * 10;
        // *primitiveEnd = *primitiveEnd & 0xff000000 | (uint)pDVar2 & 0xffffff;
        //                   /* Probable PsyQ macro: addPrim(). */
        // primitiveStart.tag = primitiveStart.tag & 0xff000000 | *primitiveChain & 0xffffff;
        // pDVar2 = g_drawModes + g_drawModes[0x14].tag * 10 + 1;
        // *primitiveChain = *primitiveChain & 0xff000000 | (uint)primitiveStart & 0xffffff;
        // primitiveChain = (uint *)(&DAT_80146f5c + iVar3);
        //                   /* Probable PsyQ macro: addPrim(). */
        // pDVar2.tag = pDVar2.tag & 0xff000000 | *primitiveChain & 0xffffff;
        // primitiveStart = g_drawModes + g_drawModes[0x14].tag * 10 + 2;
        // *primitiveChain = *primitiveChain & 0xff000000 | (uint)pDVar2 & 0xffffff;
        // primitiveChain = (uint *)(&DAT_80146f60 + iVar3);
        //                   /* Probable PsyQ macro: addPrim(). */
        // primitiveStart.tag = primitiveStart.tag & 0xff000000 | *primitiveChain & 0xffffff;
        // pDVar2 = g_drawModes + g_drawModes[0x14].tag * 10 + 5;
        // *primitiveChain = *primitiveChain & 0xff000000 | (uint)primitiveStart & 0xffffff;
        // primitiveChain = (uint *)(&DAT_80146f6c + iVar3);
        //                   /* Probable PsyQ macro: addPrim(). */
        // pDVar2.tag = pDVar2.tag & 0xff000000 | *primitiveChain & 0xffffff;
        // primitiveStart = g_drawModes + g_drawModes[0x14].tag * 10 + 6;
        // *primitiveChain = *primitiveChain & 0xff000000 | (uint)pDVar2 & 0xffffff;
        // primitiveChain = (uint *)(&DAT_80146f70 + iVar3);
        // pDVar2 = g_drawModes + g_drawModes[0x14].tag * 10 + 7;
        //                   /* Probable PsyQ macro: addPrim(). */
        // primitiveStart.tag = primitiveStart.tag & 0xff000000 | *primitiveChain & 0xffffff;
        // *primitiveChain = *primitiveChain & 0xff000000 | (uint)primitiveStart & 0xffffff;
        // primitiveChain = (uint *)(&DAT_80146f74 + iVar3);
        // newBufferIndex = g_drawModes[0x14].tag + 1 & 1;
        //                   /* WARNING: Read-only address (ram,0x80146f50) is written */
        //                   /* Probable PsyQ macro: addPrim(). */
        // g_drawModes[0x14].tag = newBufferIndex;
        // pDVar2.tag = pDVar2.tag & 0xff000000 | *primitiveChain & 0xffffff;
        // *primitiveChain = *primitiveChain & 0xff000000 | (uint)pDVar2 & 0xffffff;
        // primitiveChain = UINT_ARRAY_80180108 + uVar1 * 3;
        //                   /* Probable PsyQ macro: addPrim(). */
        // *primitiveChain = *primitiveChain & 0xff000000 | *primitiveEnd & 0xffffff;
        // *primitiveEnd = *primitiveEnd & 0xff000000 | (uint)primitiveChain & 0xffffff;
        // return (u_long *)((int)g_drawModes + (newBufferIndex ^ 1) * 0x28 + 0xf8);

        return 0;
    }

    //800481f8
    private void FUN_800481f8()
    {
        ulong uVar1;
        uint puVar2;
        UIBoxConfiguration tilesConfiguration;
        SPRT sprite;
        int j;
        CallBackInfo callBackInfo;
        int iVar4;
        int i;
        int primitiveCount;

        i = 0;

        do
        {
            callBackInfo = _gameEngine.StaticVariables.g_callbackTable[i];
            _gameEngine.StaticVariables.g_activeTransitionCallback = _gameEngine.StaticVariables.g_callbackTable[i];

            if ((callBackInfo.Flags & 1) != 0)
            {
                tilesConfiguration = callBackInfo.Data;

                if (tilesConfiguration != null && callBackInfo.Arg != 0xffffffff)
                {
                    primitiveCount = tilesConfiguration.Width * tilesConfiguration.Height;
                    j = 0;

                    if (0 < primitiveCount)
                    {
                        do
                        {
                            sprite = tilesConfiguration.SpritesA[j];
                            //var spriteB = tilesConfiguration.SpritesB[j];

                            /* Probable PsyQ macro: addPrim(). */
                            //pSVar3.tag = pSVar3.tag & 0xff000000 | *(uint*)((int)_gameEngine.StaticVariables.g_drawModes + iVar4 + callbackTable.arg * 4 + 0xf8) & 0xffffff;
                            //puVar2 = (uint*)((int)_gameEngine.StaticVariables.g_drawModes + iVar4 + callbackTable.arg * 4 + 0xf8);
                            //*puVar2 = *puVar2 & 0xff000000 | (uint)pSVar3 & 0xffffff;

                            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                            _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.BackgroundUI, bitmap);

                            j += 1;
                        } while (j < primitiveCount);

                        //Dialog choice : we need to display the text
                        //In the game the sprite is created with the text once before displaying
                        if (i == 3) // callback.Id == 2
                        {
                            sprite = tilesConfiguration.SpritesA[0];
                            var xOffset = 0;

                            foreach (var dialogChoiceSprites in _gameEngine.UIManager.DialogChoiceSprites)
                            {
                                foreach (var spr in dialogChoiceSprites)
                                {
                                    _gameEngine.Renderer.AddSprite(
                                        spr.X + sprite.x0 + 16 + xOffset,
                                        spr.Y + sprite.y0 + 8,
                                        spr.Width, spr.Height,
                                        int.MaxValue, spr.Bitmap, spr.Alpha);
                                }

                                xOffset += 0x30; //48
                            }
                        }
                    }
                }
            }

            i += 1;

        } while (i < 0xd);
    }

    //8005b670
    private int RenderAllTileLayers(int[] orderingTableBuffer1, int[] orderingTableBuffer2, int cameraX, int cameraY)
    {
        int numberOfLayerRendered;
        int additionalTiles;

        numberOfLayerRendered = 0;

        if (_gameEngine.CurrentMap.ScrollScreen != null && _gameEngine.CurrentMap.ScrollScreen.ScrollYSpeed != 0)
        {
            _gameEngine.StaticVariables.g_renderingBufferIndex = _gameEngine.StaticVariables.g_renderingBufferIndex != 1 ? 1 : 0;
            _gameEngine.StaticVariables.INT_800c48c4 += 1;

            if (_gameEngine.StaticVariables.g_tileAnimationType != 0)
            {
                UpdateScrollingTileAnimation();
            }

            numberOfLayerRendered = 0;

            if ((_gameEngine.StaticVariables.g_tileAnimationMode & 1U) != 0)
            {
                //numberOfLayerRendered = RenderLayerToBuffer(0, orderingTableBuffer1, orderingTableBuffer2, cameraX, cameraY);
            }

            if ((_gameEngine.StaticVariables.g_tileAnimationMode & 2U) != 0)
            {
                //additionalTiles = RenderLayerToBuffer(1, orderingTableBuffer1, orderingTableBuffer2, cameraX, cameraY);
                //numberOfLayerRendered += additionalTiles;
            }

            if (_gameEngine.CurrentMap.ScrollScreen.ScrollYPeriod != 0)
            {
                //RenderTileOverlayLayer(orderingTableBuffer2);
            }
        }

        return numberOfLayerRendered;
    }

    //8005b7a0
    private void UpdateScrollingTileAnimation()
    {
        //_gameEngine.StaticVariables.g_tileOffset = _gameEngine.StaticVariables.g_animationData >> 5;
        //_gameEngine.StaticVariables.g_animationCounter += 1;
        //
        //if ((_gameEngine.StaticVariables.g_animationData.field1 & 0x1f) < _gameEngine.StaticVariables.g_animationCounter)
        //{
        //    _gameEngine.StaticVariables.g_animationData += 1;
        //    _gameEngine.StaticVariables.g_animationCounter = 2;
        //    _gameEngine.StaticVariables.g_animationFrameCounter += 1;
        //
        //    if (_gameEngine.StaticVariables.g_animationFrameCounter > 16)
        //    {
        //        _gameEngine.StaticVariables.g_animationFrameCounter = 1;
        //        //_gameEngine.StaticVariables.g_animationData = _gameEngine.StaticVariables.g_tile_set + (_gameEngine.StaticVariables.g_tileAnimationType + -1) * 0x10 + _gameEngine.StaticVariables.g_tileSetMetaData.TileAnimationOffset;
        //
        //        var tileSetMetaData = _gameEngine.StaticVariables.g_tileSetMetaData[_gameEngine.StaticVariables.g_tileAnimationType + -1];
        //        //_gameEngine.StaticVariables.g_animationData = _gameEngine.StaticVariables.g_tile_set + tileSetMetaData.tileAnimationOffset;
        //    }
        //}
    }

    static int Fixed16ToInt(int v) { return v >> 16; }

    //8003b51c
    private void DisplayDebugCollisionRectangle(int orderingTable)
    {
        if (!_gameEngine.StaticVariables.DisplayCollisions)
        {
            return;
        }

        //if (_gameEngine.StaticVariables.g_debugState >= 0)
        //{
        //    return;
        //}
        //
        //// doit avoir au moins un des flags 0x100 / 0x200 (car test &0x300)
        //if ((_gameEngine.StaticVariables.g_debugFlags & 0x300) == 0)
        //{
        //    return;
        //}

        // --- choose primitive buffer (double buffering) ---
        int bufferIndex = _gameEngine.StaticVariables.g_debugFrameCounter & 1;
        _gameEngine.StaticVariables.g_debugFrameCounter++;

        TILE tilePtr = new TILE(); //_gameEngine.StaticVariables.g_spriteTiles[bufferIndex << 12];

        // ============================================================
        // Mode 0x100 : rectangles basés sur (pos/mod/width/height/depth)
        // ============================================================
        //if (_gameEngine.StaticVariables.g_debugFlags & 0x100)
        {
            for (int i = 0; i <= _gameEngine.StaticVariables.g_numberOfEntities; i++)
            {
                Entity e = _gameEngine.StaticVariables.g_entitySlots[i];

                if (e.Status - 2 >= 2)
                {
                    continue;
                }

                if (e.BlockedByEntity != null)
                {
                    continue;
                }

                // --- rectangle #0 (bleu sombre) ---
                {
                    tilePtr.r0 = 0;
                    tilePtr.g0 = 0;
                    tilePtr.b0 = 0x30;

                    int x = Fixed16ToInt(e.PosX + e.ModX) - _gameEngine.StaticVariables.g_cameraScrollingX;
                    int basePosition = e.PosY - e.PosZ - e.ModZ - e.Depth;
                    basePosition -= 1;
                    int y = Fixed16ToInt(basePosition + e.ModY + e.Height + 1) - _gameEngine.StaticVariables.g_cameraScrollingY;
                    int w = Fixed16ToInt(e.Width + 1);
                    int h = Fixed16ToInt(e.Depth + 1);

                    tilePtr.x0 = (short)x;
                    tilePtr.y0 = (short)y;
                    tilePtr.w = (short)w;
                    tilePtr.h = (short)h;

                    //AddPrim(orderingTable, tilePtr);
                    _gameEngine.Renderer.AddRectangle(tilePtr, SpriteDepth.DebugCollision, 0.5f);
                }

                // --- rectangle #1 (bleu fort) ---
                {
                    tilePtr.r0 = 0;
                    tilePtr.g0 = 0;
                    tilePtr.b0 = 0xFF;

                    int x = Fixed16ToInt(e.PosX + e.ModX) - _gameEngine.StaticVariables.g_cameraScrollingX;
                    int basePosition = e.PosY - e.PosZ - e.ModZ - e.Depth;
                    basePosition -= 1;
                    int y = Fixed16ToInt(basePosition + e.ModY) - _gameEngine.StaticVariables.g_cameraScrollingY;
                    int w = Fixed16ToInt(e.Width + 1);
                    int h = Fixed16ToInt(e.Height + 1);

                    tilePtr.x0 = (short)x;
                    tilePtr.y0 = (short)y;
                    tilePtr.w = (short)w;
                    tilePtr.h = (short)h;

                    _gameEngine.Renderer.AddRectangle(tilePtr, SpriteDepth.DebugCollision, 0.5f);
                    //AddPrim(orderingTable, tilePtr);
                }
            }
        }

        // ============================================================
        // Mode 0x200 : rectangles basés sur FrameCollisionData + offsets
        // (collision box / hitbox en pratique)
        // ============================================================
        //if (_gameEngine.StaticVariables.g_debugFlags & 0x200)
        {
            for (int i = 0; i <= _gameEngine.StaticVariables.g_numberOfEntities; i++)
            {
                Entity e = _gameEngine.StaticVariables.g_entitySlots[i];

                if (e.Status - 2 >= 2)
                {
                    continue;
                }

                if (e.BlockedByEntity != null)
                {
                    continue;
                }

                if (e.FrameCollision == null)
                {
                    continue;
                }

                int highlight = 0;
                if (e.BalanceAnimValRef != null)
                {
                    if (e.BalanceAnimValRef.Val != 0)
                    {
                        highlight = 1;
                    }
                }

                // --- rectangle #2 ---
                {
                    if (highlight != 0)
                    {
                        tilePtr.r0 = 0x30;
                        tilePtr.g0 = 0x00;
                        tilePtr.b0 = 0x00;
                    }
                    else
                    {
                        tilePtr.r0 = 0x20;
                        tilePtr.g0 = 0x20;
                        tilePtr.b0 = 0x00;
                    }

                    int basePosition = e.PosY - e.PosZ - e.CollisionOffsetZ - e.CollisionHeight;
                    basePosition -= 1;

                    int x = Fixed16ToInt(e.PosX + e.CollisionOffsetX) - _gameEngine.StaticVariables.g_cameraScrollingX;
                    int y = Fixed16ToInt(basePosition + e.CollisionOffsetY + e.CollisionDepth + 1) - _gameEngine.StaticVariables.g_cameraScrollingY;
                    int w = Fixed16ToInt(e.CollisionWidth + 1);
                    int h = Fixed16ToInt(e.CollisionHeight + 1);

                    tilePtr.x0 = (short)x;
                    tilePtr.y0 = (short)y;
                    tilePtr.w = (short)w;
                    tilePtr.h = (short)h;

                    //AddPrim(orderingTable, tilePtr);
                    _gameEngine.Renderer.AddRectangle(tilePtr, SpriteDepth.DebugCollision, 0.5f);
                }

                // --- rectangle #3 ---
                {
                    if (highlight != 0)
                    {
                        tilePtr.r0 = 0xFF;
                        tilePtr.g0 = 0x00;
                        tilePtr.b0 = 0x00;
                    }
                    else
                    {
                        tilePtr.r0 = 0x80;
                        tilePtr.g0 = 0x80;
                        tilePtr.b0 = 0x00;
                    }

                    int basePosition = e.PosY - e.PosZ - e.CollisionOffsetZ - e.CollisionHeight;
                    basePosition -= 1;

                    int x = Fixed16ToInt(e.PosX + e.CollisionOffsetX) - _gameEngine.StaticVariables.g_cameraScrollingX;
                    int y = Fixed16ToInt(basePosition + e.CollisionOffsetY) - _gameEngine.StaticVariables.g_cameraScrollingY;
                    int w = Fixed16ToInt(e.CollisionWidth + 1);
                    int h = Fixed16ToInt(e.CollisionHeight + 1);

                    tilePtr.x0 = (short)x;
                    tilePtr.y0 = (short)y;
                    tilePtr.w = (short)w;
                    tilePtr.h = (short)h;

                    //AddPrim(orderingTable, tilePtr);
                    _gameEngine.Renderer.AddRectangle(tilePtr, SpriteDepth.DebugCollision, 0.5f);
                }
            }
        }

        // --- push DR_MODE for this bufferIndex ---
        // ASM: index = bufferIndex*3, *4 bytes => stride 12 bytes
        //DR_MODE* dr = (DR_MODE*)((u8*)DR_MODE_ARRAY_80134234 + (bufferIndex * 12));
        //AddPrim(orderingTable, dr);
    }

    //80042ccc
    private uint RenderTransitionEffects(int i)
    {
        if (_gameEngine.StaticVariables.g_warpFlags != 0)
        {
            _gameEngine.StaticVariables.g_warpFadeColorR = MoveTowards(_gameEngine.StaticVariables.g_warpFadeColorR, _gameEngine.StaticVariables.g_fadeColorR_Target, _gameEngine.StaticVariables.g_warpFadeColorR_Step);
            _gameEngine.StaticVariables.g_warpFadeColorG = MoveTowards(_gameEngine.StaticVariables.g_warpFadeColorG, _gameEngine.StaticVariables.g_fadeColorG_Target, _gameEngine.StaticVariables.g_warpFadeColorG_Step);
            _gameEngine.StaticVariables.g_warpFadeColorB = MoveTowards(_gameEngine.StaticVariables.g_warpFadeColorB, _gameEngine.StaticVariables.g_fadeColorB_Target, _gameEngine.StaticVariables.g_warpFadeColorB_Step);

            if (_gameEngine.StaticVariables.g_warpFadeColorR == _gameEngine.StaticVariables.g_fadeColorR_Target
                && _gameEngine.StaticVariables.g_warpFadeColorG == _gameEngine.StaticVariables.g_fadeColorG_Target
                && _gameEngine.StaticVariables.g_warpFadeColorB == _gameEngine.StaticVariables.g_fadeColorB_Target)
            {
                _gameEngine.StaticVariables.g_warpFlags = 0;
            }
        }

        _gameEngine.StaticVariables.g_displayEnvColorR = _gameEngine.StaticVariables.g_warpFadeColorR >> 0x10;
        _gameEngine.StaticVariables.g_displayEnvColorG = _gameEngine.StaticVariables.g_warpFadeColorG >> 0x10;
        _gameEngine.StaticVariables.g_displayEnvColorB = _gameEngine.StaticVariables.g_warpFadeColorB >> 0x10;

        if (_gameEngine.StaticVariables.g_fadeStepFlags == 0)
        {
            if (_gameEngine.StaticVariables.g_fadeFrameCounter == 0)
            {
                goto LAB_80042ee4;
            }
        }
        else
        {
            _gameEngine.StaticVariables.g_currentFadeColorB = MoveTowards(_gameEngine.StaticVariables.g_currentFadeColorB, _gameEngine.StaticVariables.g_targetFadeColorB, _gameEngine.StaticVariables.g_fadeColorStepB);
            _gameEngine.StaticVariables.g_currentFadeColorG = MoveTowards(_gameEngine.StaticVariables.g_currentFadeColorG, _gameEngine.StaticVariables.g_targetFadeColorG, _gameEngine.StaticVariables.g_fadeColorStepG);
            _gameEngine.StaticVariables.g_currentFadeColorR = MoveTowards(_gameEngine.StaticVariables.g_currentFadeColorR, _gameEngine.StaticVariables.g_targetFadeColorR, _gameEngine.StaticVariables.g_fadeColorStepR);

            if (_gameEngine.StaticVariables.g_currentFadeColorB == _gameEngine.StaticVariables.g_targetFadeColorB
                && _gameEngine.StaticVariables.g_currentFadeColorG == _gameEngine.StaticVariables.g_targetFadeColorG
               && _gameEngine.StaticVariables.g_currentFadeColorR == _gameEngine.StaticVariables.g_targetFadeColorR)
            {
                _gameEngine.StaticVariables.g_fadeStepFlags = 0;
            }
        }

        var tile = _gameEngine.StaticVariables.TILE_8013fb98;
        tile.r0 = (byte)(_gameEngine.StaticVariables.g_currentFadeColorB >> 0x10);
        tile.g0 = (byte)(_gameEngine.StaticVariables.g_currentFadeColorG >> 0x10);
        tile.b0 = (byte)(_gameEngine.StaticVariables.g_currentFadeColorR >> 0x10);

        //fullscreen image used to create fade effect
        _gameEngine.Renderer.DrawColoredRectangle(tile.x0, tile.y0, tile.w, tile.h, SpriteDepth.FadeTransitionEffect, tile.r0 / 255f, 0f, 0f, 0f);

    LAB_80042ee4:
        return _gameEngine.StaticVariables.g_warpFlags | _gameEngine.StaticVariables.g_fadeStepFlags;
    }

    //80042954
    private int MoveTowards(int value, int target, int step)

    {
        int result;
        bool isFinished;

        if (step < 0)
        {
            isFinished = value + step < target;
        }
        else
        {
            isFinished = target < value + step;
        }
        result = value + step;
        if (isFinished)
        {
            result = target;
        }
        return result;
    }

    //80048054
    private void UpdateUserInterface()
    {
        int i;

        if ((_gameEngine.StaticVariables.g_saveData.GameFlags[0x38] & 0x800000U) != 0)
        {
            _gameEngine.CdManager.SetCdToAranXaMusicIndex(7);
            _gameEngine.StaticVariables.g_saveData.GameFlags[0x38] &= 0xff7fffff;
        }

        if ((_gameEngine.StaticVariables.g_saveData.GameFlags[0x33] & 0x40000000U) == 0)
        {
            _gameEngine.HudManager.InitializeHudPosition();
        }

        if ((_gameEngine.StaticVariables.g_saveData.GameFlags[0x38] & 0x200000U) != 0)
        {
            _gameEngine.StaticVariables.g_saveData.GameFlags[0x38] &= 0xffdfffff;
            _gameEngine.StaticVariables.g_saveData.GameFlags[0x33] |= 0x40000000;
            _gameEngine.HudManager.InitializeHudPositionBeforeHide();
        }

        if ((_gameEngine.StaticVariables.g_saveData.GameFlags[0x38] & 0x400000U) != 0)
        {
            ResetDrawFrameFlags();
            _gameEngine.StaticVariables.g_saveData.GameFlags[0x33] &= 0xbfffffff;
            _gameEngine.StaticVariables.g_saveData.GameFlags[0x38] &= 0xffbfffff;
        }

        i = 0;

        do
        {
            var callback = _gameEngine.StaticVariables.g_callbackTable[i];
            _gameEngine.StaticVariables.g_activeTransitionCallback = callback;

            if ((callback.Flags & 1) != 0 && callback.RenderFunc != null)
            {
                callback.RenderFunc.Invoke(_gameEngine.StaticVariables.g_activeTransitionCallback);
            }

            i += 1;

        } while (i < 0xd);

        if (_gameEngine.StaticVariables.g_postProcessState != 0)
        {
            if (_gameEngine.StaticVariables.g_postProcessState == 1
                && (short)_gameEngine.StaticVariables.g_callbackTable[6].Flags == 0)
            {
                _gameEngine.StaticVariables.g_postProcessState = 0;
                StartFadeOut();
            }

            if (_gameEngine.StaticVariables.g_postProcessState == 2
                && (short)_gameEngine.StaticVariables.g_callbackTable[4].Flags == 0)
            {
                _gameEngine.StaticVariables.g_postProcessState = 0;
                _gameEngine.MainInventoryManager.DisplayInventory();
            }
        }
    }

    //80047f94
    public int SetTransitionType(int transitionType)
    {
        int result = 1;

        if (transitionType >= 13)
        {
            return 0;
        }

        var sourceCallbackInfo = _gameEngine.StaticVariables.g_initialCallbackTable[transitionType];
        var callbackData = _gameEngine.StaticVariables.g_callbackTable[transitionType];

        _gameEngine.StaticVariables.g_activeTransitionCallback = callbackData;
        _gameEngine.StaticVariables.g_currentTransitionType = transitionType;

        callbackData.Id = sourceCallbackInfo.Id;
        callbackData.Flags = sourceCallbackInfo.Flags;
        callbackData.Data = sourceCallbackInfo.Data;
        callbackData.X = sourceCallbackInfo.X;
        callbackData.Y = sourceCallbackInfo.Y;
        callbackData.Width = sourceCallbackInfo.Width;
        callbackData.Height = sourceCallbackInfo.Height;
        callbackData.InitializeFunc = sourceCallbackInfo.InitializeFunc;
        callbackData.RenderFunc = sourceCallbackInfo.RenderFunc;
        callbackData.Arg = sourceCallbackInfo.Arg;
        callbackData.Flags |= 0x0001;

        if (sourceCallbackInfo.InitializeFunc != null)
        {
            sourceCallbackInfo.InitializeFunc.Invoke(callbackData);
        }

        return result;
    }

    //80042748
    public void ResetDebugRenderingState()
    {
        //DISPENV *dispENv;
        //DrawSync(0);
        //VSync(0);
        //dispENv = g_currentDisplayEnv;
        //dispENv[6].screen.x = 0;
        //dispENv[6].screen.y = 0;
        _gameEngine.StaticVariables.g_RCnt1 = 0;
        _gameEngine.StaticVariables.g_primCount = 0;
        _gameEngine.StaticVariables.g_lineCount = 0;
        //ResetRCnt(0xf2000001);
    }

    //8004be00
    private void ResetDrawFrameFlags()
    {
        _gameEngine.StaticVariables.g_drawFrameFlags = 0;
    }

    //80052618
    public int StartFadeOut()
    {
        var player = _gameEngine.StaticVariables.PlayerEntity;

        _gameEngine.HudManager.InitializeHudPosition();
        SetTransitionType(4);
        var image = GetAnimationImageByIndex(0);
        var bitmap = _gameEngine.AlundraMap.GenerateSpriteBitmap(image,
            _gameEngine.AlundraMap.SpriteInfo.Palettes[image.Palette]);

        _gameEngine.MainInventoryManager.InitializeHudTransitionVariablesAndSetStart(
            player.PosX, player.PosY, player.PosZ,
            _gameEngine.StaticVariables.g_cameraScrollingX, _gameEngine.StaticVariables.g_cameraScrollingY,
            (sbyte)image.Sx, (sbyte)image.Sy, bitmap);
        _gameEngine.SoundManager.PlaySoundEffect(4);
        return 1;
    }

    //80057b40
    public SiImage GetAnimationImageByIndex(int index)
    {
        //return _gameEngine.AlundraMap.SpriteInfo.SpriteRecords[index].AnimSets[0].PreloadedAnims[0].Frames[0].Images.Images[0];
        //TODO : don't use OpenBin()
        using var br = _gameEngine.DatasBin.OpenBin();
        var siImageSet = _gameEngine.AlundraMap.SpriteInfo.SpriteRecords[index].GetPortraitImageset(br);
        return siImageSet.Images[0];
    }

    //800506fc
    public void InitializeFadeOverlaySprites(SPRT[] sprites)
    {
        int i = 0;

        do
        {
            var sprite = sprites[i];
            sprite.x0 = 0x10;
            sprite.y0 = 0;
            sprite.w = 0x10;
            sprite.h = 0;
            sprite.u0 = _gameEngine.StaticVariables.g_dialogCursorTextureUV[0];
            sprite.v0 = _gameEngine.StaticVariables.g_dialogCursorTextureUV[1];
            sprite.clut = 8;//_gameEngine.StaticVariables.g_clutTable[8];

            //SetSprt(sprite);
            //SetSemiTrans(sprite, 0);
            //SetShadeTex(sprite, 1);

            ApplyFadeTransform(sprites, 0, 0, i);

            i += 1;
        } while (i < 2);
    }

    //800506dc
    public void ApplyFadeTransform(SPRT[] sprites, short width, short height, int index)
    {
        sprites[index].w = width;
        sprites[index].h = height;
    }

    //800548a4
    void FUN_800548a4(UIBoxConfiguration textTilesConfig)
    {
        int index;
        int col;
        int row;
        UIBoxConfiguration tileConfig;
        int mode;
        short w;

        mode = 0;
        tileConfig = textTilesConfig;

        do
        {
            row = 0;

            if (0 < textTilesConfig.Height)
            {
                do
                {
                    col = 0;

                    if (0 < textTilesConfig.Width)
                    {
                        do
                        {
                            var sprites = mode == 0 ? tileConfig.SpritesA : tileConfig.SpritesB;
                            index = row * textTilesConfig.Width + col;

                            //SetSprt(sprites[index]);
                            //SetSemiTrans(sprites[index], 0);
                            //SetShadeTex(sprites[index], 1);

                            var sprite = sprites[index];
                            //var clut = _gameEngine.StaticVariables.g_clutTable[sprite.clut];
                            //sprite.clut = clut;

                            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                            _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

                            col += 1;
                        } while (col < textTilesConfig.Width);
                    }

                    row += 1;
                } while (row < textTilesConfig.Height);
            }

            mode += 1;
        } while (mode < 2);
    }

    public void DrawPolyFt4(POLY_FT4 polyFt4, Bitmap image)
    {
        DrawPolyFt4(polyFt4.r0, polyFt4.b0, polyFt4.b0,
            polyFt4.x0, polyFt4.y0,
            polyFt4.x1, polyFt4.y1,
            polyFt4.x2, polyFt4.y2,
            polyFt4.x3, polyFt4.y3,
            image);
    }

    public void DrawPolyFt4(
            byte r, byte g, byte b,
            int x0, int y0,
            int x1, int y1,
            int x2, int y2,
            int x3, int y3, Bitmap image)
    {
        var minX = Math.Min(Math.Min(x0, x1), Math.Min(x2, x3));
        var maxX = Math.Max(Math.Max(x0, x1), Math.Max(x2, x3));
        var minY = Math.Min(Math.Min(y0, y1), Math.Min(y2, y3));
        var maxY = Math.Max(Math.Max(y0, y1), Math.Max(y2, y3));

        var width = maxX - minX;
        var height = maxY - minY;

        _gameEngine.Renderer.AddSprite(minX, minY, width, height, SpriteDepth.BackgroundUI, image, 1.0f);
    }

    //8004e168
    public int GetItemTextureIdByItemId(int itemId)
    {
        return _gameEngine.StaticVariables.g_itemsProperties[itemId * 5 + 4];
    }

    //8004da0c
    public void InitializeSpriteWithImage(SPRT sprt, int textureId, short x, short y)
    {
        int index;
        SiImage image;

        if (textureId != -1)
        {
            sprt.x0 = x;
            sprt.y0 = y;
            index = GetItemTextureIdByItemId(textureId);
            image = GetAnimationImageByIndex(index);
            sprt.r0 = 0x80;
            sprt.g0 = 0x80;
            sprt.b0 = 0x80;
            sprt.u0 = image.Sx;
            sprt.v0 = image.Sy;
            sprt.w = image.Swidth;
            sprt.h = image.Sheight;
            //sprt.clut = image.Palette;
            //sprt.clut = _gameEngine.StaticVariables.g_clutTableBase[image.Palette]; //why ? => number bigger than 30000
            //SetSprt(sprt);
        }
    }
}