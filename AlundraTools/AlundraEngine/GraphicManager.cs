using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;
using AlundraEngine.UI;
using System;
using System.Diagnostics;
using System.Reflection;
using AlundraEngine.Graphics;
using static AlundraEngine.Renderer;

namespace AlundraEngine;

public class GraphicManager
{
    private readonly GameEngine _gameEngine;

    public GraphicManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    // 8002bd60
    public void RenderScene(System.Drawing.Graphics graphics)
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
    private int RenderTiles(int[] renderListBase, int offsetX, int offsetY, int offsetZ, System.Drawing.Graphics graphics)
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

        //TODO: replace with Renderer.Render() method
        RendererHelper.Render(graphics, _gameEngine, currentRow, camTileOffsetY);
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

    //8002c894
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

    //80044c5c
    private int GetDisplaySyncCounter()
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
        _gameEngine.HudManager.FUN_80058134(/*StaticVariables.DAT_80146f64[g_drawModes[0x14].tag * 0x28]*/); //SPRT ??
        // uVar1 = g_drawModes[0x14].tag;
        // primitiveStart = g_drawModes + g_drawModes[0x14].tag * 10 + 4;
        // iVar3 = g_drawModes[0x14].tag * 0x28;
        // primitiveChain = (uint *)(&DAT_80146f68 + iVar3);
        // primitiveEnd = (uint *)(&DAT_80146f64 + iVar3);
        //                   /* Probable PsyQ macro: addPrim(). */
        // primitiveStart->tag = primitiveStart->tag & 0xff000000 | *primitiveChain & 0xffffff;
        // pDVar2 = g_drawModes + g_drawModes[0x14].tag * 10 + 3;
        // *primitiveChain = *primitiveChain & 0xff000000 | (uint)primitiveStart & 0xffffff;
        // primitiveChain = (uint *)((int)g_drawModes + iVar3 + 0xf8);
        //                   /* Probable PsyQ macro: addPrim(). */
        // pDVar2->tag = pDVar2->tag & 0xff000000 | *primitiveEnd & 0xffffff;
        // primitiveStart = g_drawModes + g_drawModes[0x14].tag * 10;
        // *primitiveEnd = *primitiveEnd & 0xff000000 | (uint)pDVar2 & 0xffffff;
        //                   /* Probable PsyQ macro: addPrim(). */
        // primitiveStart->tag = primitiveStart->tag & 0xff000000 | *primitiveChain & 0xffffff;
        // pDVar2 = g_drawModes + g_drawModes[0x14].tag * 10 + 1;
        // *primitiveChain = *primitiveChain & 0xff000000 | (uint)primitiveStart & 0xffffff;
        // primitiveChain = (uint *)(&DAT_80146f5c + iVar3);
        //                   /* Probable PsyQ macro: addPrim(). */
        // pDVar2->tag = pDVar2->tag & 0xff000000 | *primitiveChain & 0xffffff;
        // primitiveStart = g_drawModes + g_drawModes[0x14].tag * 10 + 2;
        // *primitiveChain = *primitiveChain & 0xff000000 | (uint)pDVar2 & 0xffffff;
        // primitiveChain = (uint *)(&DAT_80146f60 + iVar3);
        //                   /* Probable PsyQ macro: addPrim(). */
        // primitiveStart->tag = primitiveStart->tag & 0xff000000 | *primitiveChain & 0xffffff;
        // pDVar2 = g_drawModes + g_drawModes[0x14].tag * 10 + 5;
        // *primitiveChain = *primitiveChain & 0xff000000 | (uint)primitiveStart & 0xffffff;
        // primitiveChain = (uint *)(&DAT_80146f6c + iVar3);
        //                   /* Probable PsyQ macro: addPrim(). */
        // pDVar2->tag = pDVar2->tag & 0xff000000 | *primitiveChain & 0xffffff;
        // primitiveStart = g_drawModes + g_drawModes[0x14].tag * 10 + 6;
        // *primitiveChain = *primitiveChain & 0xff000000 | (uint)pDVar2 & 0xffffff;
        // primitiveChain = (uint *)(&DAT_80146f70 + iVar3);
        // pDVar2 = g_drawModes + g_drawModes[0x14].tag * 10 + 7;
        //                   /* Probable PsyQ macro: addPrim(). */
        // primitiveStart->tag = primitiveStart->tag & 0xff000000 | *primitiveChain & 0xffffff;
        // *primitiveChain = *primitiveChain & 0xff000000 | (uint)primitiveStart & 0xffffff;
        // primitiveChain = (uint *)(&DAT_80146f74 + iVar3);
        // newBufferIndex = g_drawModes[0x14].tag + 1 & 1;
        //                   /* WARNING: Read-only address (ram,0x80146f50) is written */
        //                   /* Probable PsyQ macro: addPrim(). */
        // g_drawModes[0x14].tag = newBufferIndex;
        // pDVar2->tag = pDVar2->tag & 0xff000000 | *primitiveChain & 0xffffff;
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
        CallBackInfo callbackTable;
        int iVar4;
        int i;
        int primitiveCount;

        i = 0;
        uVar1 = 0;//StaticVariables.g_drawModes[0x14].tag;
        iVar4 = 1;//StaticVariables.g_drawModes[0x14].tag * 0x28;

        do
        {
            callbackTable = StaticVariables.g_callbackTable[i];
            StaticVariables.g_activeTransitionCallback = StaticVariables.g_callbackTable[i];

            if ((callbackTable.Flags & 1) != 0)
            {
                tilesConfiguration = callbackTable.Data;

                if (tilesConfiguration != null && callbackTable.Arg != 0xffffffff)
                {
                    primitiveCount = tilesConfiguration.Width * tilesConfiguration.Height;
                    j = 0;

                    if (0 < primitiveCount)
                    {
                        do
                        {
                            sprite = tilesConfiguration.SpritesA[j];
                            var spriteB = tilesConfiguration.SpritesB[j];

                            /* Probable PsyQ macro: addPrim(). */
                            //pSVar3.tag = pSVar3.tag & 0xff000000 | *(uint*)((int)StaticVariables.g_drawModes + iVar4 + callbackTable.arg * 4 + 0xf8) & 0xffffff;
                            //puVar2 = (uint*)((int)StaticVariables.g_drawModes + iVar4 + callbackTable.arg * 4 + 0xf8);
                            //*puVar2 = *puVar2 & 0xff000000 | (uint)pSVar3 & 0xffffff;
                            
                            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                            _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

                            //bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(spriteB);
                            //_gameEngine.Renderer.AddSprite(spriteB, int.MaxValue, bitmap);

                            j = j + 1;
                        } while (j < primitiveCount);
                    }
                }
            }

            i = i + 1;

        } while (i < 0xd);
    }
    
    //8005b670
    private int RenderAllTileLayers(int i, int i1, int gCameraScrollingX, int gCameraScrollingY)
    {
        //todo
        return 0;
    }

    //8003b51c
    private void UpdateEntityGeometry(int i)
    {
        //todo
    }

    //80042ccc
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
        var image = GetAnimationImageByIndex(0);
        _gameEngine.HudManager.InitializeHudTransitionVariablesAndSetStart(
            player.PosX, player.PosY, player.PosZ,
            StaticVariables.g_cameraScrollingX, StaticVariables.g_cameraScrollingY,
            (sbyte)image.Sx, (sbyte)image.Sy, /*image.Swidth, image.Sheight,*/ image);
        _gameEngine.SoundManager.PlaySoundEffect(4);
        return 1;
    }

    //80057b40
    public SiImage GetAnimationImageByIndex(int index)
    {
        //Debugger.Break();
        //return _gameEngine.AlundraMap.SpriteInfo.Sprites[index].AnimSets[0].PreloadedAnims[0].Frames[0].Images.Images[0];
        //TODO : don't use OpenBin
        using var br = _gameEngine.DatasBin.OpenBin();
        var siImageSet = _gameEngine.AlundraMap.SpriteInfo.Sprites[index].GetPortraitImageset(br);
        return siImageSet.Images[0];
        //return (((g_initialAnimationTable.animationSet).animationOffsets + index * 2 + -0x10) + 0xc) + 2;
        //return null;
    }

    //800472d0
    public void DisplayIconName(SPRT[] sprites,
        char[] text, int textLength,
        short textCoordDstX, short textCoordDstY,
        int displayMode)
    {
        int i;
        short y;
        int j;
        int k = 0;
        char[] formattedText = new char[128];
        ushort clut;
        char currentChar;
        bool useStyledText;

        Array.Fill(formattedText, ' ');
        formattedText[^1] = '\0';
        Array.Clear(StaticVariables.g_textBuffer);
        formattedText[0] = '\0';

        if (3 < displayMode)
        {
            displayMode = displayMode + 1;
        }

        if (displayMode < 0xe)
        {
            useStyledText = _gameEngine.UIManager.ContainsSpecialTextFormatting(text[k]);

            if (useStyledText == false)
            {
                useStyledText = false;
                j = 0;

                while (true)
                {
                    currentChar = text[k];

                    if (currentChar == '\0')
                    {
                        break;
                    }

                    if (currentChar == '{')
                    {
                        k++;
                        formattedText[j] = (char)(text[k] + 'P');
                    }
                    else if (currentChar == '}')
                    {
                        k++;
                        formattedText[j] = (char)(text[k] - 0x70);
                    }
                    else
                    {
                        formattedText[j] = currentChar;
                    }

                    k++;
                    j = j + 1;
                }

                /*
                formattedText[j] = '\0';

                while (true)
                {
                    //j = strlen(formattedText);
                    if (0x7f < j)
                    {
                        break;
                    }

                    strcat(formattedText, " ");
                }*/
            }
            else
            {
                Array.Copy(formattedText, text, text.Length);
                useStyledText = true;
            }

            if (useStyledText)
            {
                RenderStyledText(formattedText, 0x3c0,
                                 (short)((uint)((displayMode * 0x10 + 0x120) * 0x10000) >> 0x10));
            }
            else
            {
                _gameEngine.UIManager.RenderTextBitmap(
                    formattedText, StaticVariables.g_textBuffer, 0x3c0,
                    (short)((uint)((displayMode * 0x10 + 0x120) * 0x10000) >> 0x10), 
                    0, 0, 0x100, 0x10);
            }

            j = 0;

            do
            {
                i = 0;
                y = textCoordDstY;

                do
                {
                    var sprite = sprites[i + j * 2];
                    sprite.w = 0xff;
                    sprite.h = 0x10;
                    sprite.u0 = (byte)'\0';
                    sprite.v0 = (byte)(displayMode * '\x10' + ' ');
                    //SetSprt(spritePtr);
                    //SetSemiTrans(spritePtr, 0);
                    //SetShadeTex(spritePtr, 1);
                    sprite.x0 = textCoordDstX;
                    sprite.y0 = y;
                    sprite.clut = (ushort)(8 - i); //StaticVariables.g_clutTable[8 - i]

                    var generateFontBitmapTim = _gameEngine.Font3.GenerateFontBitmapFromSprite(sprite);
                    _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, generateFontBitmapTim);

                    y = (short)(y - 1);
                    i = i + 1;
                } while (i < 1);

                j = j + 1;
            } while (j < 2);
        }
        else
        {
            Debugger.Break();
            //DoNothing();
        }
    }

    //8004f374
    private void RenderStyledText(char[] text, short posX, short posY)
    {
        //Debugger.Break();
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
            sprite.u0 = StaticVariables.g_dialogCursorTextureUV[0];
            sprite.v0 = StaticVariables.g_dialogCursorTextureUV[1];
            sprite.clut = 8;//StaticVariables.g_clutTable[8];

            //SetSprt(sprite);
            //SetSemiTrans(sprite, 0);
            //SetShadeTex(sprite, 1);

            ApplyFadeTransform(sprites, 0, 0, i);

            //var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            //_gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

            i = i + 1;
        } while (i < 2);
    }

    //800506dc
    private void ApplyFadeTransform(SPRT[] sprites, short width, short height, int index)
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
                            //var clut = StaticVariables.g_clutTable[sprite.clut];
                            //sprite.clut = clut;

                            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                            _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

                            col = col + 1;
                        } while (col < textTilesConfig.Width);
                    }

                    row = row + 1;
                } while (row < textTilesConfig.Height);
            }

            mode = mode + 1;
        } while (mode < 2);
    }

    public void DrawPolyFt4(POLY_FT4 polyFt4, SiImage image)
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
            int x3, int y3, SiImage image)
    {
        var minX = Math.Min(Math.Min(x0, x1), Math.Min(x2, x3));
        var maxX = Math.Max(Math.Max(x0, x1), Math.Max(x2, x3));
        var minY = Math.Min(Math.Min(y0, y1), Math.Min(y2, y3));
        var maxY = Math.Max(Math.Max(y0, y1), Math.Max(y2, y3));

        var width = maxX - minX;
        var height = maxY - minY;

        var bitmap = _gameEngine.AlundraMap.GetSpriteBitmap(image);
        _gameEngine.Renderer.AddSprite(minX, minY, width, height, int.MaxValue, bitmap, 1.0f);
    }

    //8004e168
    public int GetItemTextureIdByItemId(int itemId)
    {
        return StaticVariables.g_itemsProperties[itemId * 5 + 4];
    }
}