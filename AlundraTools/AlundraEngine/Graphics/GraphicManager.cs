using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;
using AlundraEngine.UI;

namespace AlundraEngine.Graphics;

public class GraphicManager
{
    private const int WarpTransitionDepth = SpriteDepth.FadeTransitionEffect - 100;

    private static readonly Font FontEntityId = new Font(FontFamily.GenericSansSerif, 9f);
    private static readonly Font FontTileInfo = new Font(FontFamily.GenericSansSerif, 7f);

    private static readonly Color EffectColor = Color.DarkViolet;
    private static Color EntityColor = Color.Blue;
    private static Color EntitySelectColor = Color.ForestGreen;
    private static Color EffectSelectedColor = Color.LightSeaGreen;
    private static Color ZColor = Color.Green;


    private readonly GameEngine _gameEngine;
    private Bitmap? _warpTransitionBitmap;

    public GraphicManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    // JUSTIFICATION: backend renderer adaptation only
    public void CaptureWarpTransitionFrame()
    {
        _warpTransitionBitmap?.Dispose();
        _warpTransitionBitmap = _gameEngine.Renderer.CaptureFrameBuffer();
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
                        var z = DepthFloor(y, GetTileDepthSlot(tileId));

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

                        if (wallTileId != 0xffff
                            && dy > -StaticVariables.MapTileHeight
                            && dy < StaticVariables.ScreenHeight)
                        {
                            wallTileId = GetAnimatedTileId(_gameEngine, gameMap, wallTileId, out var height);
                            z = DepthWallBlock(y, GetTileDepthSlot(wallTileId));
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

    private int GetTileDepthSlot(int tileMapIndex)
    {
        var tileIndex = tileMapIndex & 0x3ff;
        if (tileIndex >= _gameEngine.StaticVariables.g_tileAnimDescriptorTable.Length)
        {
            return 0;
        }

        return _gameEngine.StaticVariables.g_tileAnimDescriptorTable[tileIndex].SpriteIndex;
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
        return ((otIndex * STRIDE + ENTITY_SLOT) << 16) + (depthSortValue & 0xffff);
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
            var scx = (entity.PosX >> 16) - cameraX;
            var scy = (entity.PosY >> 16) - (entity.PosZ >> 16) - cameraY;
            
            if (entity.SpriteRecord != null)
            {
                //display entity
                var map = entity.IsMapSprite ? _gameEngine.CurrentMap : _gameEngine.DatasBin.AlundraGameMap;
        
                if (entity.SpriteRef?.Images != null)
                {
                    var entityZ = DepthEntity(entity.ZUpperBound);
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
                var effectZ = DepthEntity(effect.DepthSortValue);

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
        if (img.Swidth == 0 || img.Sheight == 0)
        {
            return;
        }

        // Extract blending info from Spritesheet byte
        // Bit 3 (& 0x8): Semi-transparency enabled
        // Bits 4-5 (& 0x30): Blending mode ABR (0-3)
        var blendMode = BlendMode.None;
        if ((img.Spritesheet & 0x8) != 0)
        {
            blendMode = (BlendMode)((img.Spritesheet & 0x30) >> 4);
        }

        var u1 = img.Swidth / (float)bitmap.Width;
        var v1 = img.Sheight / (float)bitmap.Height;

        renderer.DrawDeformedQuad(
            bitmap,
            x + img.X1, y + img.Y1, 0f, 0f,
            x + img.X2, y + img.Y2, u1, 0f,
            x + img.X3, y + img.Y3, 0f, v1,
            x + img.X4, y + img.Y4, u1, v1,
            z,
            0x80, 0x80, 0x80,
            alpha,
            blendMode);
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

    // GHIDRA: RenderAllTileLayers @ 0x8005B670
    private int RenderAllTileLayers(int[] orderingTableBuffer1, int[] orderingTableBuffer2, int cameraX, int cameraY)
    {
        var scrollParameters = _gameEngine.CurrentMap.ScrollParameters;
        if (scrollParameters == null || scrollParameters.Infos.Enabled == 0)
        {
            return 0;
        }

        var numberOfLayerRendered = 0;

        if (scrollParameters.HasGraphics)
        {
            _gameEngine.StaticVariables.g_renderingBufferIndex = _gameEngine.StaticVariables.g_renderingBufferIndex != 1 ? 1 : 0;
            _gameEngine.StaticVariables.INT_800c48c4 += 1;

            if (_gameEngine.StaticVariables.g_tileAnimationType != 0)
            {
                UpdateScrollingTileAnimation();
            }

            if ((_gameEngine.StaticVariables.g_tileAnimationMode & 1) != 0)
            {
                numberOfLayerRendered += RenderLayerToBuffer(0, orderingTableBuffer1, orderingTableBuffer2, cameraX, cameraY);
            }

            if ((_gameEngine.StaticVariables.g_tileAnimationMode & 2) != 0)
            {
                numberOfLayerRendered += RenderLayerToBuffer(1, orderingTableBuffer1, orderingTableBuffer2, cameraX, cameraY);
            }
        }

        if (scrollParameters.Infos.BGColorA != 0)
        {
            RenderTileOverlayLayer(orderingTableBuffer2);
        }

        return numberOfLayerRendered;
    }

    // GHIDRA: UpdateScrollingTileAnimation @ 0x8005B7A0
    private void UpdateScrollingTileAnimation()
    {
        var scrollParameters = _gameEngine.CurrentMap.ScrollParameters;
        if (scrollParameters == null || scrollParameters.Data.Length < 0x18)
        {
            return;
        }

        if ((uint)_gameEngine.StaticVariables.g_animationData >= (uint)scrollParameters.Data.Length)
        {
            return;
        }

        var oldCounter = _gameEngine.StaticVariables.g_animationCounter;
        var currentByte = scrollParameters.Data[_gameEngine.StaticVariables.g_animationData];

        _gameEngine.StaticVariables.g_tileOffset = currentByte >> 5;
        _gameEngine.StaticVariables.g_animationCounter = oldCounter + 1;

        if ((currentByte & 0x1f) < oldCounter)
        {
            _gameEngine.StaticVariables.g_animationData += 1;
            _gameEngine.StaticVariables.g_animationCounter = 2;

            var oldFrameCounter = _gameEngine.StaticVariables.g_animationFrameCounter;
            _gameEngine.StaticVariables.g_animationFrameCounter = oldFrameCounter + 1;

            if (oldFrameCounter >= 16)
            {
                _gameEngine.StaticVariables.g_animationFrameCounter = 1;

                var animationDataOffset = _gameEngine.StaticVariables.DAT_80186788
                    + BitConverter.ToInt32(scrollParameters.Data, _gameEngine.StaticVariables.PTR_8018CF58 + 0x14)
                    + ((_gameEngine.StaticVariables.g_tileAnimationType - 1) << 4);

                if ((uint)animationDataOffset < (uint)scrollParameters.Data.Length)
                {
                    _gameEngine.StaticVariables.g_animationData = animationDataOffset;
                }
            }
        }
    }

    // GHIDRA: RenderLayerToBuffer @ 0x8005B848
    private int RenderLayerToBuffer(int layerId, int[] orderingTableBuffer1, int[] orderingTableBuffer2, int cameraX, int cameraY)
    {
        const int scrollScreenWidth = 320;
        const int scrollScreenHeight = 240;
        const int scrollTileSize = 16;
        const int scrollWrapWidth = 640;
        const int scrollWrapHeight = 480;
        const int scrollMapWidth = 0x28;
        const int scrollMapHeight = 0x1E;
        const int scrollRowStride = 0x50;
        const int secondScrollOffset = 0x960;
        const int scrollBackgroundDepthBase = -0x10000000;
        const int scrollForegroundDepthBase = SpriteDepth.BackgroundUI - 1000;

        var scrollParameters = _gameEngine.CurrentMap.ScrollParameters;
        if (scrollParameters == null || layerId < 0 || layerId >= scrollParameters.Infos.ModeLayer.Length)
        {
            return 0;
        }

        if (!scrollParameters.HasGraphics)
        {
            return 0;
        }

        var layerMode = scrollParameters.Infos.ModeLayer[layerId];
        if (layerMode == 0)
        {
            return 0;
        }

        var layerInfos = scrollParameters.LayerInfos[layerId];
        var shaderBlendMode = layerInfos.BlendMode;
        var blendMode = shaderBlendMode switch
        {
            1 => BlendMode.Average,
            2 => BlendMode.Additive,
            3 => BlendMode.Subtractive,
            4 => BlendMode.AdditiveDim,
            _ => BlendMode.None,
        };
        var layerOrderOffset = layerId == 0 ? 1 : 0;
        var layerDepth = layerInfos.Ground != 0
            ? scrollForegroundDepthBase + layerOrderOffset
            : scrollBackgroundDepthBase + layerOrderOffset;

        if (layerMode == 1)
        {
            var scrollar = scrollParameters.Scrollars[layerId];
            if (scrollar.FactorXDenom == 0 || scrollar.FactorYDenom == 0)
            {
                return 0;
            }

            scrollParameters.ParallaxOffsetX[layerId] = cameraX * scrollar.FactorXNum / scrollar.FactorXDenom;
            scrollParameters.ParallaxOffsetY[layerId] = cameraY * scrollar.FactorYNum / scrollar.FactorYDenom;

            var animNum = scrollParameters.Infos.AnimNum <= 0 ? 1 : scrollParameters.Infos.AnimNum;
            if (++scrollParameters.AnimFrameTimer[layerId] > layerInfos.AnimTimer)
            {
                if (++scrollParameters.AnimFrameCounter[layerId] >= animNum)
                {
                    scrollParameters.AnimFrameCounter[layerId] = 0;
                }

                scrollParameters.AnimFrameTimer[layerId] = 0;
            }

            scrollParameters.TimerX[layerId]++;
            scrollParameters.OffsetX[layerId] += scrollar.ScrollXSpeed;
            var periodX = Math.Abs((int)scrollar.ScrollXPeriod);
            if (periodX > 0 && scrollParameters.TimerX[layerId] >= periodX)
            {
                scrollParameters.OffsetX[layerId] += scrollParameters.ScrollDirX[layerId];
                scrollParameters.TimerX[layerId] = 0;
            }

            scrollParameters.TimerY[layerId]++;
            scrollParameters.OffsetY[layerId] += scrollar.ScrollYSpeed;
            var periodY = Math.Abs((int)scrollar.ScrollYPeriod);
            if (periodY > 0 && scrollParameters.TimerY[layerId] >= periodY)
            {
                scrollParameters.OffsetY[layerId] += scrollParameters.ScrollDirY[layerId];
                scrollParameters.TimerY[layerId] = 0;
            }

            var screenPosX = scrollParameters.OffsetX[layerId] + scrollParameters.ParallaxOffsetX[layerId];
            var screenPosY = scrollParameters.OffsetY[layerId] + scrollParameters.ParallaxOffsetY[layerId];

            while (screenPosX < 0)
            {
                scrollParameters.OffsetX[layerId] += scrollWrapWidth;
                screenPosX += scrollWrapWidth;
            }

            while (screenPosX >= scrollWrapWidth)
            {
                scrollParameters.OffsetX[layerId] -= scrollWrapWidth;
                screenPosX -= scrollWrapWidth;
            }

            while (screenPosY < 0)
            {
                scrollParameters.OffsetY[layerId] += scrollWrapHeight;
                screenPosY += scrollWrapHeight;
            }

            while (screenPosY >= scrollWrapHeight)
            {
                scrollParameters.OffsetY[layerId] -= scrollWrapHeight;
                screenPosY -= scrollWrapHeight;
            }

            var tileX = screenPosX >> 4;
            var tileY = screenPosY >> 4;
            var subX = screenPosX & 15;
            var subY = screenPosY & 15;

            var secondScroll = layerId != 0 && scrollParameters.Infos.ModeLayer[0] == 1 && scrollParameters.Infos.ModeLayer[1] == 1;
            var baseMapOffset = (int)scrollParameters.Graphics + 0x8100 + (secondScroll ? secondScrollOffset : 0);
            if (baseMapOffset < 0 || baseMapOffset >= scrollParameters.DataSize)
            {
                return 0;
            }

            var xStart = -subX;
            var yStart = -subY;
            var cols = (scrollScreenWidth - xStart + 15) >> 4;
            var rows = (scrollScreenHeight - yStart + 15) >> 4;
            var vAnim = (scrollParameters.AnimFrameCounter[layerId] << 8) / animNum;
            var primCount = 0;

            for (var y = 0; y < rows; y++)
            {
                var ty = tileY + y;
                if (ty >= scrollMapHeight)
                {
                    ty -= scrollMapHeight;
                }

                var rowOffset = baseMapOffset + ty * scrollRowStride;

                for (var x = 0; x < cols; x++)
                {
                    var tx = tileX + x;
                    if (tx >= scrollMapWidth)
                    {
                        tx -= scrollMapWidth;
                    }

                    var entryOffset = rowOffset + (tx << 1);
                    if (entryOffset + 1 >= scrollParameters.DataSize)
                    {
                        continue;
                    }

                    var tileVal = scrollParameters.Data[entryOffset];
                    if (tileVal == 0)
                    {
                        continue;
                    }

                    var palDex = scrollParameters.Data[entryOffset + 1];
                    var x0 = xStart + (x << 4);
                    var y0 = yStart + (y << 4);
                    AddScrollBitmap(scrollParameters, palDex, (tileVal & 0x0F) << 4, ((tileVal & 0xF0) + vAnim) & 0xFF, scrollTileSize, scrollTileSize, x0, y0, layerDepth, shaderBlendMode, blendMode);
                    primCount++;
                }
            }

            return primCount;
        }

        if (layerMode != 2)
        {
            return 0;
        }

        var cellular = scrollParameters.Cellulars[layerId];
        var cells = scrollParameters.Cells[layerId];
        if (cells.Length == 0)
        {
            return 0;
        }

        var layerAnimNum = scrollParameters.Infos.AnimNum <= 0 ? 1 : scrollParameters.Infos.AnimNum;
        if (++scrollParameters.AnimFrameTimer[layerId] > layerInfos.AnimTimer)
        {
            if (++scrollParameters.AnimFrameCounter[layerId] >= layerAnimNum)
            {
                scrollParameters.AnimFrameCounter[layerId] = 0;
            }

            scrollParameters.AnimFrameTimer[layerId] = 0;
        }

        var phase = (scrollParameters.AnimFrameCounter[layerId] << 8) / layerAnimNum;
        scrollParameters.WaveTick[layerId] = (byte)(scrollParameters.WaveTick[layerId] + 1);

        var cellularPrimCount = 0;
        var cellNum = Math.Min(cells.Length, ScrollParameters.CellMax);

        for (var i = 0; i < cellNum; i++)
        {
            var curCell = cells[i];
            switch ((CellType)curCell.Type)
            {
                case CellType.Normal:
                {
                    var posX = scrollParameters.CellPosX[layerId][i];
                    var posY = scrollParameters.CellPosY[layerId][i];
                    var tickX = scrollParameters.CellTickX[layerId][i];
                    var tickY = scrollParameters.CellTickY[layerId][i];

                    posX += curCell.DX;
                    posY += curCell.DY;

                    if (curCell.PeriodX != 0)
                    {
                        var stepX = (curCell.DX < 0 || curCell.PeriodX < 0) ? -1 : +1;
                        var absPX = Math.Abs((int)curCell.PeriodX);
                        if (++tickX >= absPX)
                        {
                            posX += stepX;
                            tickX = 0;
                        }
                    }

                    if (curCell.PeriodY != 0)
                    {
                        var stepY = (curCell.DY < 0 || curCell.PeriodY < 0) ? -1 : +1;
                        var absPY = Math.Abs((int)curCell.PeriodY);
                        if (++tickY >= absPY)
                        {
                            posY += stepY;
                            tickY = 0;
                        }
                    }

                    var baseX = 0;
                    var baseY = 0;
                    if (curCell.CamXDen != 0)
                    {
                        baseX = cameraX * curCell.CamXNum / curCell.CamXDen;
                    }

                    if (curCell.CamYDen != 0)
                    {
                        baseY = cameraY * curCell.CamYNum / curCell.CamYDen;
                    }

                    var sx = posX - baseX;
                    var sy = posY - baseY;

                    var minX = curCell.U0 - curCell.U1;
                    if (sx < minX)
                    {
                        posX += scrollScreenWidth - minX;
                        sx = posX - baseX;
                    }
                    else if (sx > scrollScreenWidth - 1)
                    {
                        posX += -scrollScreenWidth + minX;
                        sx = posX - baseX;
                    }

                    var minY = curCell.V0 - curCell.V1;
                    if (sy < minY)
                    {
                        posY += scrollScreenHeight - minY;
                        sy = posY - baseY;
                    }
                    else if (sy > scrollScreenHeight - 1)
                    {
                        posY += -scrollScreenHeight + minY;
                        sy = posY - baseY;
                    }

                    scrollParameters.CellPosX[layerId][i] = posX;
                    scrollParameters.CellPosY[layerId][i] = posY;
                    scrollParameters.CellTickX[layerId][i] = tickX;
                    scrollParameters.CellTickY[layerId][i] = tickY;

                    var width = curCell.U1 - curCell.U0 + 1;
                    var height = curCell.V1 - curCell.V0 + 1;
                    AddScrollBitmap(scrollParameters, curCell.PalDex, curCell.U0, (curCell.V0 + phase) & 0xFF, width, height, sx, sy, layerDepth, shaderBlendMode, blendMode);
                    cellularPrimCount++;
                    break;
                }

                case CellType.ScriptTrack:
                    break;

                case CellType.FallRespawn:
                {
                    var posX = scrollParameters.CellPosX[layerId][i];
                    var posY = scrollParameters.CellPosY[layerId][i];
                    var tickX = scrollParameters.CellTickX[layerId][i];
                    var tickY = scrollParameters.CellTickY[layerId][i];

                    posX += curCell.DX;
                    posY += curCell.DY;

                    var stepX = 0;
                    var stepY = 0;
                    if (curCell.PeriodX != 0)
                    {
                        stepX = (curCell.DX < 0 || curCell.PeriodX < 0) ? -1 : +1;
                    }

                    if (curCell.PeriodY != 0)
                    {
                        stepY = (curCell.DY < 0 || curCell.PeriodY < 0) ? -1 : +1;
                    }

                    var absPX = Math.Abs((int)curCell.PeriodX);
                    var absPY = Math.Abs((int)curCell.PeriodY);
                    if (absPX > 0 && ++tickX >= absPX)
                    {
                        posX += stepX;
                        tickX = 0;
                    }

                    if (absPY > 0 && ++tickY >= absPY)
                    {
                        posY += stepY;
                        tickY = 0;
                    }

                    var baseX = 0;
                    var baseY = 0;
                    if (curCell.CamXDen != 0)
                    {
                        baseX = cameraX * curCell.CamXNum / curCell.CamXDen;
                    }

                    if (curCell.CamYDen != 0)
                    {
                        baseY = cameraY * curCell.CamYNum / curCell.CamYDen;
                    }

                    var sx = posX - baseX;
                    var sy = posY - baseY;

                    var minX = curCell.U0 - curCell.U1;
                    if (sx < minX)
                    {
                        posX += scrollScreenWidth - minX;
                        sx = posX - baseX;
                    }
                    else if (sx > scrollScreenWidth - 1)
                    {
                        posX += -scrollScreenWidth + minX;
                        sx = posX - baseX;
                    }

                    if (sy > scrollScreenHeight - 1)
                    {
                        posX = (int)((AlundraEngine.Random.Next() * (ulong)scrollScreenWidth) >> 32);
                        posY += -scrollScreenHeight + (curCell.V0 - curCell.V1);
                        sx = posX - baseX;
                        sy = posY - baseY;
                    }

                    scrollParameters.CellPosX[layerId][i] = posX;
                    scrollParameters.CellPosY[layerId][i] = posY;
                    scrollParameters.CellTickX[layerId][i] = tickX;
                    scrollParameters.CellTickY[layerId][i] = tickY;

                    var width = curCell.U1 - curCell.U0 + 1;
                    var height = curCell.V1 - curCell.V0 + 1;
                    AddScrollBitmap(scrollParameters, curCell.PalDex, curCell.U0, (curCell.V0 + phase) & 0xFF, width, height, sx, sy, layerDepth, shaderBlendMode, blendMode);
                    cellularPrimCount++;
                    break;
                }

                case CellType.WaveX:
                {
                    if (scrollParameters.WaveLut.Length == 0)
                    {
                        break;
                    }

                    var width = curCell.U1 - curCell.U0 + 1;
                    var height = curCell.V1 - curCell.V0 + 1;

                    var idxA1 = (curCell.Y0 * cellular.AWaveY) & 0xFF;
                    var idxA2 = (scrollParameters.WaveTick[layerId] * cellular.AWavePhase) & 0xFF;
                    var aW = scrollParameters.WaveLut[idxA1] * scrollParameters.WaveLut[idxA2] * cellular.AWaveAmp;
                    if (aW < 0)
                    {
                        aW += 0x7F;
                    }

                    var idxB = (curCell.Y0 * cellular.BWaveY + scrollParameters.WaveTick[layerId] * cellular.BWavePhase) & 0xFF;
                    var bW = scrollParameters.WaveLut[idxB] * cellular.BWaveWeight;

                    var tSum = (aW >> 7) + bW;
                    if (tSum < 0)
                    {
                        tSum += 0x7F;
                    }

                    var x = curCell.X0 + (tSum >> 7) - 8;
                    var y = curCell.Y0;
                    AddScrollBitmap(scrollParameters, curCell.PalDex, curCell.U0, (curCell.V0 + phase) & 0xFF, width, height, x, y, layerDepth, shaderBlendMode, blendMode);
                    cellularPrimCount++;
                    break;
                }
            }
        }

        return cellularPrimCount;
    }

    // JUSTIFICATION: backend renderer adaptation only
    // RELATION: adapter for the C port MainShader STP split used by scrolling Batch draws
    private void AddScrollBitmap(ScrollParameters scrollParameters, int paletteIndex, int u, int v, int width, int height, int x, int y, int depth, byte shaderBlendMode, BlendMode blendMode)
    {
        var opaqueBitmap = scrollParameters.GetScrollBitmap(paletteIndex, u, v, width, height, shaderBlendMode, semiTransOnly: false);
        if (opaqueBitmap != null)
        {
            _gameEngine.Renderer.AddSprite(x, y, width, height, depth, opaqueBitmap);
        }

        if (blendMode == BlendMode.None)
        {
            return;
        }

        var semiTransBitmap = scrollParameters.GetScrollBitmap(paletteIndex, u, v, width, height, shaderBlendMode, semiTransOnly: true);
        if (semiTransBitmap != null)
        {
            _gameEngine.Renderer.AddSprite(x, y, width, height, depth, semiTransBitmap, 1.0f, 1.0f, 1.0f, 1.0f, blendMode);
        }
    }

    // GHIDRA: RenderTileOverlayLayer @ 0x8005BA40
    private void RenderTileOverlayLayer(int[] orderingTableBuffer2)
    {
        const int overlayDepth = SpriteDepth.BackgroundUI - 2000;
        const int overlayWidth = 320;
        const int overlayHeight = 240;

        var scrollParameters = _gameEngine.CurrentMap.ScrollParameters;
        if (scrollParameters == null || scrollParameters.Infos.Enabled == 0)
        {
            return;
        }

        var flag = scrollParameters.Infos.BGColorA;
        if (flag == 0)
        {
            return;
        }

        var extended = !(flag < 0x65);
        scrollParameters.OvrTick++;

        var ovrPtr = extended ? scrollParameters.OverlayExt : scrollParameters.Overlay;
        var ovrSize = extended ? 0x10u : 0x04u;
        if (ovrPtr == 0 || ovrPtr >= scrollParameters.DataSize)
        {
            return;
        }

        if (scrollParameters.OvrTick >= scrollParameters.OvrHold)
        {
            var frameOffset = (int)(ovrPtr + scrollParameters.OvrOff);
            if (frameOffset + ovrSize > scrollParameters.DataSize)
            {
                scrollParameters.OvrOff = 0;
                frameOffset = (int)ovrPtr;
            }

            scrollParameters.OvrHold = extended
                ? scrollParameters.Data[frameOffset + 12]
                : scrollParameters.Data[frameOffset + 3];
            scrollParameters.OvrTick = 0;
            scrollParameters.OvrOff += ovrSize;

            if (scrollParameters.OvrHold == 0)
            {
                scrollParameters.OvrOff = 0;
            }
        }

        var prevOffset = scrollParameters.OvrOff >= ovrSize ? scrollParameters.OvrOff - ovrSize : 0;
        var baseOffset = (int)(ovrPtr + prevOffset);
        if (baseOffset + ovrSize > scrollParameters.DataSize)
        {
            return;
        }

        var alpha = flag < 0x65 ? 0.5f : 0.25f;
        if (!extended)
        {
            var tile = new TILE
            {
                x0 = 0,
                y0 = 0,
                w = overlayWidth,
                h = overlayHeight,
                r0 = scrollParameters.Data[baseOffset],
                g0 = scrollParameters.Data[baseOffset + 1],
                b0 = scrollParameters.Data[baseOffset + 2],
            };

            _gameEngine.Renderer.AddRectangle(tile, overlayDepth, alpha);
            return;
        }

        var poly = new POLY_G4
        {
            x0 = 0,
            y0 = 0,
            x1 = overlayWidth,
            y1 = 0,
            x2 = 0,
            y2 = overlayHeight,
            x3 = overlayWidth,
            y3 = overlayHeight,
            r0 = scrollParameters.Data[baseOffset],
            g0 = scrollParameters.Data[baseOffset + 1],
            b0 = scrollParameters.Data[baseOffset + 2],
            r1 = scrollParameters.Data[baseOffset + 3],
            g1 = scrollParameters.Data[baseOffset + 4],
            b1 = scrollParameters.Data[baseOffset + 5],
            r2 = scrollParameters.Data[baseOffset + 6],
            g2 = scrollParameters.Data[baseOffset + 7],
            b2 = scrollParameters.Data[baseOffset + 8],
            r3 = scrollParameters.Data[baseOffset + 9],
            g3 = scrollParameters.Data[baseOffset + 10],
            b3 = scrollParameters.Data[baseOffset + 11],
        };

        _gameEngine.Renderer.AddQuadColor(poly, overlayDepth, alpha);
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

                if (!e.Status.IsActive())
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

                if (!e.Status.IsActive())
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
                if (e.CurrentAttack != null)
                {
                    if (e.CurrentAttack.AttackAttribute != 0)
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
        var fadeBlendMode = _gameEngine.StaticVariables.g_fadeTPagePrim1 switch
        {
            1 => BlendMode.Additive,
            2 => BlendMode.Subtractive,
            _ => BlendMode.None,
        };

        _gameEngine.Renderer.DrawColoredRectangle(
            tile.x0,
            tile.y0,
            tile.w,
            tile.h,
            SpriteDepth.FadeTransitionEffect,
            1.0f,
            tile.r0 / 255f,
            tile.g0 / 255f,
            tile.b0 / 255f,
            fadeBlendMode);

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
        DrawPolyFt4(polyFt4.r0, polyFt4.g0, polyFt4.b0,
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
        // Version simplifiée : utiliser les UVs par défaut (rectangle complet)
        _gameEngine.Renderer.DrawDeformedQuad(
            image,
            x0, y0, 0f, 0f,
            x1, y1, 1f, 0f,
            x2, y2, 0f, 1f,
            x3, y3, 1f, 1f,
            SpriteDepth.BackgroundUI,
            r, g, b,
            1.0f);
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

    // JUSTIFICATION: C# language bridge only
    private static int GetWarpTransitionOrderingTable(OrderingTableBuffer orderingTableBuffer)
    {
        return orderingTableBuffer[3];
    }

    // JUSTIFICATION: C# language bridge only
    private int GetWarpEffectWord(int shortIndex)
    {
        var warpEffectBuffer = _gameEngine.StaticVariables.g_warpEffectBuffer;
        return unchecked((int)((uint)(ushort)warpEffectBuffer[shortIndex] | ((uint)(ushort)warpEffectBuffer[shortIndex + 1] << 16)));
    }

    // JUSTIFICATION: C# language bridge only
    private void SetWarpEffectWord(int shortIndex, int value)
    {
        var warpEffectBuffer = _gameEngine.StaticVariables.g_warpEffectBuffer;
        warpEffectBuffer[shortIndex] = (short)value;
        warpEffectBuffer[shortIndex + 1] = (short)(value >> 16);
    }

    // JUSTIFICATION: backend renderer adaptation only
    private void QueueWarpTransitionSlice(int sourceX, int sourceY, int width, int height, int destinationX, int destinationY)
    {
        if (_warpTransitionBitmap == null || width <= 0 || height <= 0)
        {
            return;
        }

        if (destinationX < 0)
        {
            sourceX -= destinationX;
            width += destinationX;
            destinationX = 0;
        }

        if (destinationY < 0)
        {
            sourceY -= destinationY;
            height += destinationY;
            destinationY = 0;
        }

        if (sourceX < 0)
        {
            destinationX -= sourceX;
            width += sourceX;
            sourceX = 0;
        }

        if (sourceY < 0)
        {
            destinationY -= sourceY;
            height += sourceY;
            sourceY = 0;
        }

        width = Math.Min(width, StaticVariables.ScreenWidth - destinationX);
        width = Math.Min(width, _warpTransitionBitmap.Width - sourceX);
        height = Math.Min(height, StaticVariables.ScreenHeight - destinationY);
        height = Math.Min(height, _warpTransitionBitmap.Height - sourceY);

        if (width <= 0 || height <= 0)
        {
            return;
        }

        var u0 = sourceX / (float)_warpTransitionBitmap.Width;
        var v0 = sourceY / (float)_warpTransitionBitmap.Height;
        var u1 = (sourceX + width) / (float)_warpTransitionBitmap.Width;
        var v1 = (sourceY + height) / (float)_warpTransitionBitmap.Height;

        _gameEngine.Renderer.DrawDeformedQuad(
            _warpTransitionBitmap,
            destinationX, destinationY, u0, v0,
            destinationX + width, destinationY, u1, v0,
            destinationX, destinationY + height, u0, v1,
            destinationX + width, destinationY + height, u1, v1,
            WarpTransitionDepth,
            0x80, 0x80, 0x80,
            1.0f);
    }

    // GHIDRA: FUN_800435e0 @ 0x800435E0
    private byte FUN_800435e0(OrderingTableBuffer orderingTableBuffer)
    {
        var transitionState = (byte)RenderTransitionEffects(GetWarpTransitionOrderingTable(orderingTableBuffer));

        _gameEngine.StaticVariables.g_effectRenderToggle += 1;
        QueueWarpTransitionSlice(0, 0, StaticVariables.ScreenWidth, StaticVariables.ScreenHeight, 0, 0);

        return transitionState;
    }

    // GHIDRA: FUN_800436a0 @ 0x800436A0
    private byte FUN_800436a0(OrderingTableBuffer orderingTableBuffer)
    {
        RenderTransitionEffects(GetWarpTransitionOrderingTable(orderingTableBuffer));

        byte isEffectRunning = 0;
        var warpEffectBuffer = _gameEngine.StaticVariables.g_warpEffectBuffer;

        _gameEngine.StaticVariables.g_effectRenderToggle += 1;

        for (var row = 0; row < 0x0f; row++)
        {
            var sourceY = row << 4;

            for (var column = 0; column < 0x14; column++)
            {
                var sourceX = column << 4;
                var baseIndex = row * 80 + column * 4;
                var counter = (short)(warpEffectBuffer[baseIndex] + 1);
                warpEffectBuffer[baseIndex] = counter;

                if (counter <= 0)
                {
                    QueueWarpTransitionSlice(sourceX, sourceY, 0x10, 0x10, sourceX, sourceY);
                    isEffectRunning = 1;
                    continue;
                }

                var duration = warpEffectBuffer[baseIndex + 1];
                var remaining = duration - counter;
                if (remaining <= 0)
                {
                    continue;
                }

                var progress = (remaining << 16) / duration;
                var shrink = 8 - ((progress * 8) >> 16);
                if ((uint)shrink >= 8u)
                {
                    continue;
                }

                var interpolatedX = ((progress * (warpEffectBuffer[baseIndex + 2] - 0x98)) >> 16) + 0x98;
                var destinationX = interpolatedX + ((progress * (sourceX - interpolatedX)) >> 16) + shrink;
                var interpolatedY = ((progress * (warpEffectBuffer[baseIndex + 3] - 0x70)) >> 16) + 0x70;
                var destinationY = interpolatedY + ((progress * (sourceY - interpolatedY)) >> 16) + shrink;
                var size = 0x10 - (shrink << 1);

                QueueWarpTransitionSlice(sourceX + shrink, sourceY + shrink, size, size, destinationX, destinationY);
                isEffectRunning = 1;
            }
        }

        _gameEngine.StaticVariables.g_renderEffectDoneFlag = isEffectRunning == 0;
        _gameEngine.StaticVariables.g_renderEffectCompleted = isEffectRunning == 0;

        return isEffectRunning;
    }

    // GHIDRA: FUN_8004392c @ 0x8004392C
    private byte FUN_8004392c(OrderingTableBuffer orderingTableBuffer)
    {
        var transitionState = (byte)RenderTransitionEffects(GetWarpTransitionOrderingTable(orderingTableBuffer));
        var amplitude = GetWarpEffectWord(0);

        _gameEngine.StaticVariables.g_effectRenderToggle += 1;

        for (var row = 0; row < StaticVariables.ScreenHeight; row++)
        {
            var randomValue = (uint)Random.Next();
            var centeredRandom = (int)((randomValue * 0x201UL) >> 32) - 0x100;
            var offsetX = (int)(((long)centeredRandom * amplitude) >> 16);

            if (offsetX >= 0)
            {
                if (offsetX < StaticVariables.ScreenWidth)
                {
                    QueueWarpTransitionSlice(0, row, StaticVariables.ScreenWidth - offsetX, 1, offsetX, row);
                }

                continue;
            }

            if (offsetX <= -StaticVariables.ScreenWidth)
            {
                continue;
            }

            QueueWarpTransitionSlice(-offsetX, row, StaticVariables.ScreenWidth + offsetX, 1, 0, row);
        }

        SetWarpEffectWord(0, amplitude + 0x80);

        return transitionState;
    }

    // GHIDRA: FUN_80043b34 @ 0x80043B34
    private byte FUN_80043b34(OrderingTableBuffer orderingTableBuffer)
    {
        RenderTransitionEffects(GetWarpTransitionOrderingTable(orderingTableBuffer));

        byte isEffectRunning = 0;
        var warpEffectBuffer = _gameEngine.StaticVariables.g_warpEffectBuffer;

        _gameEngine.StaticVariables.g_effectRenderToggle += 1;

        for (var row = 0; row < 0x0f; row++)
        {
            for (var column = 0; column < 0x14; column++)
            {
                var sourceX = column << 4;
                var sourceY = row << 4;
                var destinationX = sourceX;
                var destinationY = sourceY;
                var width = 0x10;
                var height = 0x10;
                var baseIndex = row * 80 + column * 4;
                var counter = GetWarpEffectWord(baseIndex);

                if (counter > 0)
                {
                    destinationX += (warpEffectBuffer[baseIndex + 2] * counter) >> 4;
                    destinationY += warpEffectBuffer[baseIndex + 3] >> 4;

                    if (destinationX < 0)
                    {
                        if (destinationX < -0x0f)
                        {
                            continue;
                        }

                        width = destinationX + 0x10;
                        sourceX -= destinationX;
                        destinationX = 0;
                    }
                    else if (destinationX >= StaticVariables.ScreenWidth)
                    {
                        if (destinationX >= 0x140)
                        {
                            continue;
                        }

                        width = StaticVariables.ScreenWidth - destinationX;
                    }

                    if (destinationY < 0)
                    {
                        if (destinationY < -0x0f)
                        {
                            continue;
                        }

                        height = destinationY + 0x10;
                        sourceY -= destinationY;
                        destinationY = 0;
                    }
                    else if (destinationY >= StaticVariables.ScreenHeight)
                    {
                        if (destinationY >= 0x0f0)
                        {
                            continue;
                        }

                        height = StaticVariables.ScreenHeight - destinationY;
                    }

                    warpEffectBuffer[baseIndex + 3] = (short)(warpEffectBuffer[baseIndex + 3] + (counter << 2));
                    QueueWarpTransitionSlice(sourceX, sourceY, width, height, destinationX, destinationY);
                    isEffectRunning = 1;
                }

                SetWarpEffectWord(baseIndex, counter + 1);
            }
        }

        return isEffectRunning;
    }

    // GHIDRA: FUN_80043d54 @ 0x80043D54
    private byte FUN_80043d54(OrderingTableBuffer orderingTableBuffer)
    {
        var transitionState = (byte)RenderTransitionEffects(GetWarpTransitionOrderingTable(orderingTableBuffer));
        var warpEffectBuffer = _gameEngine.StaticVariables.g_warpEffectBuffer;

        _gameEngine.StaticVariables.g_effectRenderToggle += 1;

        if (transitionState != 0)
        {
            QueueWarpTransitionSlice(0, 0, StaticVariables.ScreenWidth, StaticVariables.ScreenHeight, 0, 0);
            return 1;
        }

        byte isEffectRunning = 0;

        for (var row = 0; row < 0x0f; row++)
        {
            var sourceY = row << 4;

            for (var column = 0; column < 0x14; column++)
            {
                var sourceX = column << 4;
                var baseIndex = row * 80 + column * 4;
                var countdown = GetWarpEffectWord(baseIndex);
                var shrink = 0;

                if (countdown != 0)
                {
                    SetWarpEffectWord(baseIndex, countdown - 1);
                }
                else
                {
                    var phase = warpEffectBuffer[baseIndex + 2] + 0x80;
                    if (phase >= 0x800)
                    {
                        continue;
                    }

                    warpEffectBuffer[baseIndex + 2] = (short)phase;
                    shrink = phase >> 8;
                    isEffectRunning = 1;
                }

                var size = 0x10 - (shrink << 1);
                QueueWarpTransitionSlice(sourceX + shrink, sourceY + shrink, size, size, sourceX + shrink, sourceY + shrink);
            }
        }

        if (isEffectRunning == 0)
        {
            // 0x801FB424 sits inside g_heapBuffer at 0x801F7F24 + 0x3500.
            var heapBuffer = _gameEngine.StaticVariables.g_heapBuffer;
            var heapBufferWord = heapBuffer[0x3500]
                | (heapBuffer[0x3501] << 8)
                | (heapBuffer[0x3502] << 16)
                | (heapBuffer[0x3503] << 24);

            heapBufferWord |= 0x00400000;
            heapBuffer[0x3500] = (byte)heapBufferWord;
            heapBuffer[0x3501] = (byte)(heapBufferWord >> 8);
            heapBuffer[0x3502] = (byte)(heapBufferWord >> 16);
            heapBuffer[0x3503] = (byte)(heapBufferWord >> 24);
        }

        return isEffectRunning;
    }

    // GHIDRA: FUN_80043f8c @ 0x80043F8C
    private byte FUN_80043f8c(OrderingTableBuffer orderingTableBuffer)
    {
        var transitionState = (byte)RenderTransitionEffects(GetWarpTransitionOrderingTable(orderingTableBuffer));
        var warpEffectBuffer = _gameEngine.StaticVariables.g_warpEffectBuffer;

        _gameEngine.StaticVariables.g_effectRenderToggle += 1;

        for (var row = 0; row < StaticVariables.ScreenHeight; row++)
        {
            var destinationY = warpEffectBuffer[3] < row ? warpEffectBuffer[3] : row;
            QueueWarpTransitionSlice(0, row, StaticVariables.ScreenWidth, 1, 0, destinationY);
        }

        if (warpEffectBuffer[3] > 0)
        {
            warpEffectBuffer[3] -= 1;
        }

        return transitionState;
    }

    // GHIDRA: FUN_80044440 @ 0x80044440
    public byte FUN_80044440(OrderingTableBuffer orderingTableBuffer, int transitionEffectId)
    {
        return transitionEffectId switch
        {
            0 => FUN_800435e0(orderingTableBuffer),
            1 => (byte)(FUN_800435e0(orderingTableBuffer) & 0),
            2 => FUN_800435e0(orderingTableBuffer),
            3 => (byte)(FUN_800435e0(orderingTableBuffer) & 0),
            4 => FUN_800436a0(orderingTableBuffer),
            5 => FUN_8004392c(orderingTableBuffer),
            6 => FUN_80043b34(orderingTableBuffer),
            7 => (byte)(FUN_800435e0(orderingTableBuffer) & 0),
            8 => FUN_80043d54(orderingTableBuffer),
            9 => FUN_800435e0(orderingTableBuffer),
            10 => FUN_800435e0(orderingTableBuffer),
            11 => FUN_80043f8c(orderingTableBuffer),
            _ => (byte)(FUN_800435e0(orderingTableBuffer) & 0),
        };
    }
}