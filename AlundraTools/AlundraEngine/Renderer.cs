using System.Diagnostics;
using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;
using AlundraEngine.UI;

namespace AlundraEngine;

public class Renderer
{
    private readonly GameEngine _gameEngine;

    public Renderer(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    // 8002bd60
    public void RenderScene(Graphics graphics)
    {
        byte localScratchpad = 0;
        StaticVariables.g_unusedByteArray = localScratchpad;

        StaticVariables.g_numberOfTilesDrawn = RenderTiles(
            /*StaticVariables.g_orderingTableBuffer[4]*/ null,
            StaticVariables.g_cameraLookAtX, StaticVariables.g_cameraLookAtY, StaticVariables.g_cameraLookAtZ,
            graphics);

        //StaticVariables.g_numberOfEntitiesDrawn = RenderEntitiesMaybe(StaticVariables.g_orderingTableBuffer[4],  StaticVariables.g_cameraScrollingX, StaticVariables.g_cameraScrollingY);

        if (StaticVariables.g_debugState < 0 && (StaticVariables.g_debugFlags & 0x40) != 0)
        {
            StaticVariables.g_numberOfLayersDrawn = 0;
        }
        else
        {
            StaticVariables.g_numberOfLayersDrawn = RenderAllTileLayers(
                StaticVariables.g_orderingTableBuffer[0], StaticVariables.g_orderingTableBuffer[1],
                    StaticVariables.g_cameraScrollingX, StaticVariables.g_cameraScrollingY);
        }

        //UpdateEntityGeometry(StaticVariables.g_orderingTableBuffer[2]);
        //RenderEffects(StaticVariables.g_orderingTableBuffer[3]);
        UpdatePostProcessingEffects();
        SwapBuffersAndDraw();
        StaticVariables.g_primitive_sync = GetDisplaySyncCounter();
    }

    // 8002cda0
    private int RenderTiles(int[] renderListBase, int offsetX, int offsetY, int offsetZ, Graphics graphics)
    {
        //TODO
        ResetTileAnimationState();

        if (StaticVariables.g_isCameraScrolling == 0)
        {
            StaticVariables.g_cameraScrollingX =
                StaticVariables.g_cameraScrollingX +
                (offsetX - (StaticVariables.g_cameraScrollingX + 0xa0) >> 4) + StaticVariables.g_cameraOffsetX + StaticVariables.g_cameraDebugOffsetX;
            StaticVariables.g_cameraScrollingY =
                StaticVariables.g_cameraScrollingY +
                (offsetY - offsetZ - (StaticVariables.g_cameraScrollingY + 0x88) >> 4) + StaticVariables.g_cameraOffsetY +
                StaticVariables.g_cameraDebugOffsetY;
        }
        else
        {
            StaticVariables.g_isCameraScrolling = 0;
            StaticVariables.g_cameraScrollingY = offsetY - offsetZ - 0x88;
            StaticVariables.g_cameraScrollingX = offsetX - 0xa0;
        }

        StaticVariables.g_cameraDebugOffsetX = 0;
        StaticVariables.g_cameraDebugOffsetY = 0;

        if (StaticVariables.g_cameraScrollingX < 0)
        {
            StaticVariables.g_cameraScrollingX = 0;
        }
        else if (0x39f < StaticVariables.g_cameraScrollingX)
        {
            StaticVariables.g_cameraScrollingX = 0x39f;
        }

        var nbColumns = 0xf;
        var newCamRow = StaticVariables.g_cameraScrollingX / 0x18;
        var col = StaticVariables.g_cameraScrollingX % 0x18; // divide by 15, approximation

        if (col < StaticVariables.MapTileHeight)
        {
            nbColumns = 0xe;
        }

        if (StaticVariables.g_cameraScrollingY < 0)
        {
            StaticVariables.g_cameraScrollingY = 0;
        }
        else if (0x2cf < StaticVariables.g_cameraScrollingY)
        {
            StaticVariables.g_cameraScrollingY = 0x2cf;
        }

        var currentRow = StaticVariables.g_cameraScrollingY;

        if (StaticVariables.g_cameraScrollingY < 0)
        {
            currentRow = StaticVariables.g_cameraScrollingY + 0xf;
        }

        currentRow = currentRow >> 4;
        var camTileOffsetY = (short)StaticVariables.g_cameraScrollingY + (short)currentRow * -StaticVariables.MapTileHeight;

        //for (int i = 0; i < 0x3C0; i++) 
        //{
        //    StaticVariables.g_tileVRAMClearTable[i] = 0;
        //}

        //Map animation
        for (int i = 0; i < 6; i++)
        {
            var spriteMapEntry = _gameEngine.CurrentMap.Info.SpriteMapEntries[i];
            spriteMapEntry.Tick++;

            if (spriteMapEntry.Enabled == 1 && spriteMapEntry.FrameDuration <= spriteMapEntry.Tick)
            {
                spriteMapEntry.Index += spriteMapEntry.TileWidth;
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

        if (StaticVariables.g_debugState < 0)
        {
            layerFlag = (int)(StaticVariables.g_debugFlags >> 7);
        }

        if (layerFlag != StaticVariables.g_LoadVRAMAssets_debug)
        {
            //load text to render tile information
            //_gameEngine.LoadVRAMAssets();
        }

        if ((StaticVariables.g_debugState & 0x80000000U) == 0)
        {
            StaticVariables.g_renderTileRowCount = 0x3c;
        }
        else
        {
            StaticVariables.g_renderTileRowCount = StaticVariables.g_mapLimits;
        }

        int maxTileSprite = 0;
        int visibleTileCount = 0;
        var tileAnimFramIndex = (StaticVariables.g_tileAnimFrameCounter & 1U) * 0x2a8;
        var puVar4 = StaticVariables.INT_ARRAY_800e0758[(StaticVariables.g_tileAnimFrameCounter & 1U) * 0xd48];

        RendererHelper.Render(graphics, _gameEngine.DatasBin, _gameEngine.CurrentMap, currentRow, camTileOffsetY);
        /*
        if (currentRow < StaticVariables.g_renderTileRowCount)
        {
            var local_tileHeight2 = currentRow * 0x1a0 + 0x604;
            var i = currentRow;

            do
            {
                var y = 0;
                if (nbColumns != 0)
                {
                    //var tileAnimListPtr = StaticVariables.g_spriteVRAMPointer[newCamRow * 8 + local_tileHeight2 + 6];
                    var tileAnimListPtr = _gameEngine.CurrentMap.Map.MapTiles[newCamRow * 52 + local_tileHeight2];
                    var tileOffsetYPtr = StaticVariables.g_tileOffsetYTable;
                    var primitivePtrIndex = tileAnimFramIndex;
                    var primitivePtr2Index = tileAnimFramIndex;

                    do
                    {
                        var tilePrimPtr = StaticVariables.g_tileSpriteBuffer[tileAnimFramIndex];
                        var primitivePtr = StaticVariables.g_tileSpriteBuffer[primitivePtrIndex];

                        layerFlag = (int)(i - tileAnimListPtr.Tile - currentRow);
                        var tileXRel = (short)col;

                        if (layerFlag < StaticVariables.MapTileHeight)
                        {
                            var spriteAttr = (uint)tileAnimListPtr.Flags;

                            if ((spriteAttr != 0xffff) && ((spriteAttr & 0x3ff) < 0x3c0))
                            {
                                primitivePtr.x0 = (short)(tileOffsetYPtr[0] - tileXRel);
                                primitivePtr.y0 = (short)(StaticVariables.g_tileOffsetYTable[layerFlag + StaticVariables.MapTileHeight] - camTileOffsetY);
                                primitivePtr.u0 = (byte)StaticVariables.g_drawPageInfoTable[(spriteAttr >> 0xc) * 2];
                                var index = StaticVariables.g_spriteMapTable[StaticVariables.g_tileAnimDescriptorTable[spriteAttr & 0x3ff].SpriteIndex].Index;
                                var tileAnimDescriptor = StaticVariables.g_tileAnimDescriptorTable[(spriteAttr & 0x3ff) + index];
                                primitivePtr.v0 = tileAnimDescriptor.DrawPageOffset;

                                primitivePtr.tag = tileAnimDescriptor.Padding;
                                var otIndex = i * StaticVariables.MapTileHeight + (uint)tileAnimDescriptor.SpriteIndex;
                                var puVar1 = renderListBase[otIndex];

                                // Probable PsyQ macro: addPrim().
                                spriteAttr = (uint)(tilePrimPtr.tag & 0xffffff);
                                tilePrimPtr.tag = tilePrimPtr.tag & 0xff000000 | (ulong)(puVar1 & 0xffffff);
                                primitivePtrIndex++;
                                puVar1 = (int)(puVar1 & 0xff000000 | spriteAttr);
                                StaticVariables.g_tileOTFlags[otIndex] = 1;
                                
                                tileAnimFramIndex++;
                                visibleTileCount = visibleTileCount + 1;
                            }
                        }

                        if (tileAnimListPtr.TileId != 0xffff)
                        {
                            var tileListPtr = StaticVariables.g_spriteVRAMPointer[tileAnimListPtr + 0x33c2];
                            var spriteAttr = (uint)*(byte*)((int)tileListPtr + 1);
                            layerFlag = layerFlag + (spriteAttr - (int)(char)*tileListPtr);

                            if (spriteAttr != 0)
                            {                                  
                                var psVar3 = StaticVariables.g_tileOffsetYTable[layerFlag + StaticVariables.MapTileHeight];
                                tileListPtr = tileListPtr + spriteAttr;
                                var primitivePtr2 = StaticVariables.g_tileSpriteBuffer[primitivePtr2Index];

                                do
                                {
                                    if (layerFlag < StaticVariables.MapTileHeight)
                                    {
                                        var spriteIndex = (uint)*tileListPtr;

                                        if ((spriteIndex != 0xffff) && ((spriteIndex & 0x3ff) < 0x3c0))
                                        {
                                            primitivePtr2.x0 = *tileOffsetYPtr - tileXRel;
                                            primitivePtr2.y0 = *psVar3 - camTileOffsetY;
                                            primitivePtr2.u0 = (((int)spriteIndex >> 0xc) * 2 + (int)StaticVariables.g_drawPageInfoTable);
                                            var tileAnimDescriptor = StaticVariables.g_tileAnimDescriptorTable + (spriteIndex & 0x3ff) +
                                                                     StaticVariables.g_spriteMapTable[StaticVariables.g_tileAnimDescriptorTable[spriteIndex & 0x3ff].spriteIndex].index;
                                            primitivePtr = primitivePtr + 1;
                                            primitivePtr2.v0 = tileAnimDescriptor.drawPageOffset;

                                            primitivePtr2.tag = tileAnimDescriptor.padding;
                                            primitivePtr2Index++;

                                            var otIndex = i * StaticVariables.MapTileHeight + (uint)tileAnimDescriptor.spriteIndex + 7;
                                            var puVar1 = (uint*)(renderListBase + otIndex);
                                            tilePrimPtr.tag = tilePrimPtr.tag & 0xff000000 | *puVar1 & 0xffffff;
                                            spriteIndex = (uint)tilePrimPtr & 0xffffff;
                                            *puVar1 = *puVar1 & 0xff000000 | spriteIndex;
                                            StaticVariables.g_tileOTFlags[otIndex] = 1;
                                            
                                            tileAnimFramIndex++;
                                            visibleTileCount = visibleTileCount + 1;
                                        }
                                    }

                                    tileListPtr = tileListPtr + -1;
                                    spriteAttr = spriteAttr - 1;
                                    psVar3 = psVar3 + -1;
                                    layerFlag = layerFlag - 1;

                                } while (0 < (int)spriteAttr);
                            }
                        }
                        
                        y = y + 1;
                        tileOffsetYPtr = tileOffsetYPtr + 1;
                        tileAnimListPtr = tileAnimListPtr + 4;
                    } while (y < nbColumns);
                }

                i = i + 1;
                local_tileHeight2 = local_tileHeight2 + 0x1a0;

            } while (i < StaticVariables.g_renderTileRowCount);
        }

        newCamRow = 0;

        do
        {
            col = 0;
            var tileOrderingIndex = 0;

            do
            {
                currentRow = newCamRow * StaticVariables.MapTileHeight + col;

                if (StaticVariables.g_tileOTFlags[currentRow] == 1)
                {
                    layerFlag = StaticVariables.g_tileOrderingTable[tileOrderingIndex].code;
                    puVar4 = StaticVariables.g_tileOrderingTable[tileOrderingIndex].tag;
                    StaticVariables.g_tileOrderingTable[tileOrderingIndex].code = layerFlag;

                    maxTileSprite = maxTileSprite + 1;
                    var puVar1 = (uint*)(renderListBase + currentRow);
                    // Probable PsyQ macro: addPrim().
                    layerFlag = (uint)puVar4 & 0xffffff;
                    *puVar4 = *puVar4 & 0xff000000 | *puVar1 & 0xffffff;
                    puVar4 = puVar4 + 2;
                    *puVar1 = *puVar1 & 0xff000000 | layerFlag;
                }

                if (StaticVariables.g_tileOTFlags[currentRow + 7] == 1)
                {
                    layerFlag = StaticVariables.g_tileOrderingTable[tileOrderingIndex].code;
                    puVar4 = StaticVariables.g_tileOrderingTable[tileOrderingIndex].tag;
                    puVar4[1] = layerFlag;
                    maxTileSprite = maxTileSprite + 1;
                    var puVar1 = (uint*)(renderListBase + currentRow + 7);
                    // Probable PsyQ macro: addPrim().
                    layerFlag = (uint)puVar4 & 0xffffff;
                    *puVar4 = *puVar4 & 0xff000000 | *puVar1 & 0xffffff;
                    puVar4 = puVar4 + 2;
                    *puVar1 = *puVar1 & 0xff000000 | layerFlag;
                }

                col = col + 1;
                tileOrderingIndex = tileOrderingIndex + 1;
            } while (col < 6);

            newCamRow = newCamRow + 1;

        } while (newCamRow < 0x3c);
        */

        // Vérifications de débordement
        if (visibleTileCount >= 599)
        {
            StaticVariables.g_map_sprite = StaticVariables.g_currentMap;
            //PrintInfo("MAP SPRT OVER!! : %d", g_currentMap);
        }

        if (maxTileSprite >= 199)
        {
            StaticVariables.g_dr_tpage = StaticVariables.g_currentMap;
            //PrintInfo("MAP DR_TPAGE OVER!! : %d", g_currentMap);
        }

        StaticVariables.g_tileAnimFrameCounter++;

        return (maxTileSprite << 16) | visibleTileCount;
    }

    private void ResetTileAnimationState()
    {
        if (StaticVariables.g_bossCutsceneFlag == 0)
        {
            StaticVariables.g_cutsceneScrollLimitY = 0;
            StaticVariables.g_cutsceneScrollLimitX = 0;
            StaticVariables.g_cutsceneScrollSpeedY = 0;
            StaticVariables.g_cutsceneScrollSpeedX = 0;
            StaticVariables.g_cameraOffsetY = 0;
            StaticVariables.g_cameraOffsetX = 0;
        }
        else
        {
            if (StaticVariables.g_cutsceneScrollLimitX == 0 || StaticVariables.g_cutsceneScrollSpeedX == 0)
            {
                StaticVariables.g_cameraOffsetX = 0;
            }
            else if (StaticVariables.g_cutsceneXReachedMin == 0)
            {
                StaticVariables.g_cameraOffsetX -= StaticVariables.g_cutsceneScrollSpeedX;
                if (StaticVariables.g_cameraOffsetX <= -StaticVariables.g_cutsceneScrollLimitX)
                {
                    StaticVariables.g_cutsceneXReachedMin = 1;
                    StaticVariables.g_cameraOffsetX = -StaticVariables.g_cutsceneScrollLimitX;
                }
            }
            else
            {
                StaticVariables.g_cameraOffsetX += StaticVariables.g_cutsceneScrollSpeedX;
                if (StaticVariables.g_cameraOffsetX >= StaticVariables.g_cutsceneScrollLimitX)
                {
                    StaticVariables.g_cameraOffsetX = StaticVariables.g_cutsceneScrollLimitX;
                    StaticVariables.g_cutsceneXReachedMin = 0;
                }
            }

            if (StaticVariables.g_cutsceneScrollLimitY == 0)
            {
                StaticVariables.g_cameraOffsetY = 0;
            }
            else if (StaticVariables.g_cutsceneYReachedMin == 0)
            {
                StaticVariables.g_cameraOffsetY -= StaticVariables.g_cutsceneScrollSpeedY;
                if (StaticVariables.g_cameraOffsetY <= -StaticVariables.g_cutsceneScrollLimitY)
                {
                    StaticVariables.g_cutsceneYReachedMin = 1;
                    StaticVariables.g_cameraOffsetY = -StaticVariables.g_cutsceneScrollLimitY;
                }
            }
            else
            {
                StaticVariables.g_cameraOffsetY += StaticVariables.g_cutsceneScrollSpeedY;
                if (StaticVariables.g_cameraOffsetY >= StaticVariables.g_cutsceneScrollLimitY)
                {
                    StaticVariables.g_cameraOffsetY = StaticVariables.g_cutsceneScrollLimitY;
                    StaticVariables.g_cutsceneYReachedMin = 0;
                }
            }
        }
    }

    //8002e130
    private int RenderEntitiesMaybe(int i, int gCameraScrollingX, int gCameraScrollingY)
    {
        //todo
        return 0;
    }

    private int GetDisplaySyncCounter()
    {
        //todo
        return 0;
    }

    private int RenderAllTileLayers(int i, int i1, int gCameraScrollingX, int gCameraScrollingY)
    {
        //todo
        return 0;
    }

    private void UpdateEntityGeometry(int i)
    {
        //todo
    }

    private void RenderEffects(int i)
    {
        //todo
    }

    //8005ec98
    private int UpdatePostProcessingEffects()
    {
        var result = StaticVariables.g_globalTransitionState == 1;

        if (StaticVariables.g_globalTransitionState != 0)
        {
            result = StaticVariables.g_postProcessingState < 3;

            if (StaticVariables.g_postProcessingState == 2)
            {
                RunFadeEffect();
            }
            else if (StaticVariables.g_postProcessingState == 0)
            {
                result = true;

                if (StaticVariables.g_postProcessingState == 3)
                {
                    result = RunWaveEffect() != 0;
                }
            }
            else
            {
                result = true;

                if (StaticVariables.g_postProcessingState == 1)
                {
                    RunFadeEffect();
                }
            }
        }

        return result ? 1 : 0;
    }

    //8005f458
    private void RunFadeEffect()
    {

    }

    //8005ed2c
    private int RunWaveEffect()
    {
        return 0;
    }

    //80048054
    private void SwapBuffersAndDraw()
    {
        int i;

        if ((StaticVariables.g_renderFlags & 0x800000U) != 0)
        {
            ActivateSpecialRenderMode(7);
            StaticVariables.g_renderFlags = (int)(StaticVariables.g_renderFlags & 0xff7fffff);
        }

        if ((StaticVariables.g_systemFlags & 0x40000000U) == 0)
        {
            InitializeFrame();
        }

        if ((StaticVariables.g_renderFlags & 0x200000U) != 0)
        {
            StaticVariables.g_renderFlags = (int)(StaticVariables.g_renderFlags & 0xffdfffff);
            StaticVariables.g_systemFlags = StaticVariables.g_systemFlags | 0x40000000;
            PrepareBufferFlip();
        }

        if ((StaticVariables.g_renderFlags & 0x400000U) != 0)
        {
            ResetDrawFrameFlags();
            StaticVariables.g_systemFlags = (int)(StaticVariables.g_systemFlags & 0xbfffffff);
            StaticVariables.g_renderFlags = (int)(StaticVariables.g_renderFlags & 0xffbfffff);
        }

        i = 0;

        do
        {
            var callback = StaticVariables.g_callbackTable[i];
            StaticVariables.g_activeTransitionCallback = callback;

            if ((callback.Flags & 1) != 0 && callback.RenderFunc != null)
            {
                callback.RenderFunc.Invoke(StaticVariables.g_activeTransitionCallback);
            }

            i = i + 1;

        } while (i < 0xd);

        if (StaticVariables.g_postProcessState != 0)
        {
            if (StaticVariables.g_postProcessState == 1 /*&& _DAT_801530d0 == 0*/)
            {
                StaticVariables.g_postProcessState = 0;
                StartFadeOut();
            }

            if (StaticVariables.g_postProcessState == 2
                && (short)StaticVariables.g_callbackTable[4].Flags == 0)
            {
                StaticVariables.g_postProcessState = 0;
                //TriggerDebugZone();
            }
        }
    }

    //8005abe0
    private void ActivateSpecialRenderMode(int mode)
    {
        /*
          CdlLOC aCStack_18 [2];
           u_char auStack_10 [8];
           
           if ((g_isCdResetRequested != 0) || ((g_cdIsReady != 0 && (g_cdDataLoaded == 0)))) {
             g_cdDataStartPtr = DAT_CDAranXa_pos + g_mapCdDataOffsets[mode * 3];
             CdIntToPos(g_cdDataStartPtr,aCStack_18);
             CdControl('\x02',&aCStack_18[0].minute,auStack_10);
             CdControl('\x15',(u_char *)0x0,auStack_10);
           }
         */
    }

    //8004bd9c
    public void InitializeFrame()
    {
        if ((StaticVariables.g_drawFrameFlags & 3U) == 1)
        {
            StaticVariables.g_drawState = 2;
            StaticVariables.g_fadeTimer = 0;
            StaticVariables.g_fadeStep = 0xf;
            StaticVariables.g_blendRed = 0;
            StaticVariables.g_blendGreen = 0x10;
            StaticVariables.g_blendBlue = 0;
            StaticVariables.g_blendAlpha = (short)~(StaticVariables.g_soundFadeTimer << 3);
            StaticVariables.g_drawFrameFlags |= 2;
        }
    }

    //8004be0c
    public void PrepareBufferFlip()
    {
        if ((StaticVariables.g_systemFlags & 0x40000000U) != 0 && StaticVariables.g_drawFrameFlags == 0)
        {
            SetTransitionType(1);
            StaticVariables.g_drawState = 2;
            StaticVariables.g_fadeTimer = 0;
            StaticVariables.g_fadeStep = 0xf;
            StaticVariables.g_blendAlpha = 0x10;
            StaticVariables.g_blendRed = 0;
            StaticVariables.g_blendBlue = 0;
            StaticVariables.g_drawFrameFlags = 5;
            StaticVariables.g_blendGreen = (short)~(ushort)(StaticVariables.g_soundFadeTimer << 3);
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

        var transitionFuncArgs = StaticVariables.g_transitionFuncArgs[transitionType];
        var callbackData = StaticVariables.g_callbackTable[transitionType];

        StaticVariables.g_activeTransitionCallback = callbackData;
        StaticVariables.g_currentTransitionType = transitionType;

        callbackData.Flags = transitionFuncArgs.Flags;
        callbackData.Data = transitionFuncArgs.Data;
        callbackData.X = transitionFuncArgs.X;
        callbackData.Y = transitionFuncArgs.Y;
        callbackData.Width = transitionFuncArgs.Width;
        callbackData.Height = transitionFuncArgs.Height;
        callbackData.InitializeFunc = transitionFuncArgs.InitializeFunc;
        callbackData.RenderFunc = transitionFuncArgs.RenderFunc;
        callbackData.Arg = transitionFuncArgs.Arg;

        // Active le flag de transition dans le callback
        callbackData.Flags |= 0x0001;

        if (transitionFuncArgs.InitializeFunc != null)
        {
            transitionFuncArgs.InitializeFunc.Invoke(callbackData);
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
        StaticVariables.g_RCnt1 = 0;
        StaticVariables.g_primCount = 0;
        StaticVariables.g_lineCount = 0;
        //ResetRCnt(0xf2000001);
    }

    //8004be00
    private void ResetDrawFrameFlags()
    {
        StaticVariables.g_drawFrameFlags = 0;
    }

    //80052618
    private int StartFadeOut()
    {
        var player = StaticVariables.PlayerEntity;

        InitializeFrame();
        SetTransitionType(4);
        Debugger.Break();
        var sprite = GetAnimationImageByIndex(0);
        //InitCameraTransition(-player.PosX, -player.PosY, -player.PosZ,
        //    -StaticVariables.g_cameraScrollingX, -StaticVariables.g_cameraScrollingY,
        //    sprite.U, sprite.V, sprite.Witdh, sprite.Height);
        _gameEngine.SoundManager.PlaySoundEffect(4);
        return 1;
    }

    //80057b40
    public SiImage GetAnimationImageByIndex(int index)
    {
        //return (((g_initialAnimationTable.animationSet).animationOffsets + index * 2 + -0x10) + 0xc) + 2;
        return null;
    }

    //80057c18
    private void InitCameraTransition(int srcX, int srcY, int srcZ,
        int dstX, int dstY,
        sbyte u, sbyte v,
        ushort width,
        ushort height)
    {
        StaticVariables.g_cameraTransitionStartX = 0xf8;
        StaticVariables.g_cameraTransitionStartY = 0x68;
        _gameEngine.InitCameraTransitionEffect(srcX, srcY, srcZ, dstX, dstY, (byte)u, (byte)v, 0x30, 0x38, (short)width, (short)height);
    }

    //800472d0
    public void DisplayIconName(SPRT sprite, 
        char[] text, int textLength, 
        short textCoordDstX, short textCoordDstY, 
        int displayMode)
    {
        Debugger.Break();
    }

    //800506fc
    public void InitFadeOverlaySprites(SPRT[] sprites)
    {
        int i = 0;

        do
        {
            var sprite = sprites[i];
            sprite.x0 = 0x10;
            sprite.y0 = 0;
            sprite.w = 0x10;
            sprite.h = 0;
            sprite.u0 = StaticVariables.g_dialogCursorTextureU;
            sprite.v0 = StaticVariables.g_dialogCursorTextureV;
            sprite.clut = StaticVariables.g_clutTable[8];

            //SetSprt(sprite);
            //SetSemiTrans(sprite, 0);
            //SetShadeTex(sprite, 1);
            ApplyFadeTransform(sprites, 0, 0, i);
            
            i = i + 1;
        } while (i < 2);
    }

    //800506dc
    private void ApplyFadeTransform(SPRT[] sprites, short width, short height, int index)
    {
        sprites[index].w = width;
        sprites[index].h = height;
    }

    //80054a34
    public void InitializeInventorySpriteNumberOf()
    {
        
    }

    //800548a4
    void FUN_800548a4(TextTilesConfiguration textTilesConfig)
    {
        int index;
        int col;
        int row;
        TextTilesConfiguration tileConfig;
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

                            sprites[index].clut = StaticVariables.g_clutTable[sprites[index].clut];

                            col = col + 1;
                        } while (col < textTilesConfig.Width);
                    }

                    row = row + 1;
                } while (row < textTilesConfig.Height);
            }

            mode = mode + 1;
        } while (mode < 2);
    }
}