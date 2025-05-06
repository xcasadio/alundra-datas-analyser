namespace Alundra;

public class Renderer
{
    private readonly GameEngine _gameEngine;

    public Renderer(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    public void RenderScene(Graphics graphics)
    {
        byte localScratchpad = 0;
        StaticVariables.g_unusedByteArray = localScratchpad;

        StaticVariables.g_numberOfTilesDrawn = RenderTiles(/*StaticVariables.g_orderingTableBuffer[4]*/ null,
            StaticVariables.g_cameraLookAtX, StaticVariables.g_cameraLookAtY, StaticVariables.g_cameraLookAtZ);

        //StaticVariables.g_numberOfEntitiesDrawn = RenderEntitiesMaybe(StaticVariables.g_orderingTableBuffer[4],  StaticVariables.g_cameraScrollingX, StaticVariables.g_cameraScrollingY);

        if (StaticVariables.g_debugState < 0 && (StaticVariables.g_debugFlags & 0x40) != 0)
        {
            StaticVariables.g_numberOfLayersDrawn = 0;
        }
        else
        {
            //StaticVariables.g_numberOfLayersDrawn = RenderAllTileLayers(
            //    StaticVariables.g_orderingTableBuffer[0], StaticVariables.g_orderingTableBuffer[1],
            //        StaticVariables.g_cameraScrollingX, StaticVariables.g_cameraScrollingY);
        }

        //UpdateEntityGeometry(StaticVariables.g_orderingTableBuffer[2]);
        //RenderEffects(StaticVariables.g_orderingTableBuffer[3]);
        UpdatePostProcessingEffects();
        SwapBuffersAndDraw();
        StaticVariables.g_primitive_sync = GetDisplaySyncCounter();


        RendererHelper.Render(graphics, _gameEngine.DatasBin, _gameEngine.CurrentMap);
    }

    private int RenderTiles(int[] renderListBase, int offsetX, int offsetY, int offsetZ)
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
                ((offsetY - offsetZ) - (StaticVariables.g_cameraScrollingY + 0x88) >> 4) + StaticVariables.g_cameraOffsetY +
                StaticVariables.g_cameraDebugOffsetY;
        }
        else
        {
            StaticVariables.g_isCameraScrolling = 0;
            StaticVariables.g_cameraScrollingY = (offsetY - offsetZ) + -0x88;
            StaticVariables.g_cameraScrollingX = offsetX + -0xa0;
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

        var tileHeight = 0xf;
        var newCamRow = StaticVariables.g_cameraScrollingX / 0x18;
        var col = StaticVariables.g_cameraScrollingX % 0x18;

        if (col < 0x10)
        {
            tileHeight = 0xe;
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
        var camTileOffsetY = (short)StaticVariables.g_cameraScrollingY + (short)currentRow * -0x10;

        var i = 0x3bf;
        //tileDataPtr = &StaticVariables.g_tileVRAMClearTable;
        //do {
        //    *tileDataPtr = 0;
        //    i = i + -1;
        //    tileDataPtr = tileDataPtr + -1;
        //} while (-1 < i);
        //maxTileSprite = 0;
        //visibleTileCount = 0;
        i = 0;
        //spriteMapEntry = StaticVariables.g_spriteMapTable;
        //tilePrimPtr = StaticVariables.g_tileSpriteBuffer + (StaticVariables.g_tileAnimFrameCounter & 1U) * 0x2a8;
        //puVar4 = (uint *)(StaticVariables.INT_ARRAY_800e0758 + (StaticVariables.g_tileAnimFrameCounter & 1U) * 0xd48);

        do
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

            i++;
        } while (i < 6);

        //TODO

        return 0;
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

    private void UpdatePostProcessingEffects()
    {
        //todo
    }

    private void SwapBuffersAndDraw()
    {
        //todo
    }

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

    private int SetTransitionType(int transitionType)
    {
        int miscParam;
        int[] callbackData;
        int[] dataArgs;
        int[] drawArgs;
        int[] updateArgs;

        if (transitionType < 0xd)
        {
            //int index = transitionType * 7;
            //callbackData = StaticVariables.g_callbackTable[index];
            //dataArgs = StaticVariables.g_transitionFuncArgs[index + 1];
            //drawArgs = StaticVariables.g_transitionFuncArgs[index + 2];
            //updateArgs = StaticVariables.g_transitionFuncArgs[index + 3];
            //
            //StaticVariables.g_activeTransitionCallback = callbackData;
            //StaticVariables.g_callbackTable[index] = StaticVariables.g_transitionFuncArgs[index];
            //StaticVariables.g_callbackTable[index + 1] = dataArgs;
            //StaticVariables.g_callbackTable[index + 2] = drawArgs;
            //StaticVariables.g_callbackTable[index + 3] = updateArgs;
            //
            //dataArgs = StaticVariables.g_transitionFuncArgs[index + 5];
            //drawArgs = StaticVariables.g_transitionFuncArgs[index + 6];
            //
            //StaticVariables.g_callbackTable[index + 4] = StaticVariables.g_transitionFuncArgs[index + 4];
            //StaticVariables.g_callbackTable[index + 5] = dataArgs;
            //StaticVariables.g_callbackTable[index + 6] = drawArgs;
            //
            //StaticVariables.g_activeTransitionCallback[0] = StaticVariables.g_activeTransitionCallback[0] | 1;
            //StaticVariables.g_currentTransitionType = transitionType;
            //
            //if (StaticVariables.g_callbackTable[index + 4] != null)
            //{
            //    StaticVariables.g_callbackTable[index + 4](StaticVariables.g_activeTransitionCallback);
            //}

            miscParam = 1;
        }
        else
        {
            miscParam = 0;
        }

        return miscParam;
    }

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
}