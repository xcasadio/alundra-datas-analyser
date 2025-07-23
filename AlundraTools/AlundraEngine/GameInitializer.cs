using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;
using System.Diagnostics;

namespace AlundraEngine;

public class GameInitializer
{
    private GameEngine _gameEngine;

    public GameInitializer(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    public void Initialize()
    {
        //short tPagePtr = StaticVariables.g_tPageFadeLUT;
        ushort tPage;
        ushort clutPtr;
        int screenX;
        int screenY;
        int clutLoopIndex = 0;
        int largestEntryIndex = 0;
        byte[] datasBinHeader;

        //VSync(0);
        //ResetCallback();
        //ResetGraph(3);
        //CdInit();
        //InitSound();
        InitializePadController();
        //InitCDRom();
        //InitMemoryCard();
        //FntLoad(0x3c0, 0x100);
        //StaticVariables.g_fontLoaded = FntOpen(8, 0x20, 0x130, 0xc0, 0, 0x400);
        //SetDumpFnt(g_fontLoaded);

        //do
        //{
        //    largestEntryIndex = 0;
        //    screenY = 0;
        //    do
        //    {
        //        screenX = 0x140;
        //        clutPtr = tPagePtr + largestEntryIndex;
        //        do
        //        {
        //            tPage = GetTPage(0, clutLoopIndex, screenX, screenY);
        //            *clutPtr = tPage;
        //            screenX = screenX + 0x40;
        //            clutPtr = clutPtr + 1;
        //            largestEntryIndex = largestEntryIndex + 1;
        //        } while (screenX < 0x400);
        //        screenY = screenY + 0x100;
        //    } while (screenY < 0x200);
        //    tPagePtr = tPagePtr + 0x16;
        //    clutLoopIndex = clutLoopIndex + 1;
        //    screenY = 0;
        //} while (clutLoopIndex < 4);
        //clutLoopIndex = 0;
        //do
        //{
        //    largestEntryIndex = 0x1e0;
        //    clutPtr = &g_drawPageInfoBase + screenY;
        //    do
        //    {
        //        tPage = GetClut(clutLoopIndex, largestEntryIndex);
        //        *clutPtr = tPage;
        //        largestEntryIndex = largestEntryIndex + 1;
        //        clutPtr = clutPtr + 1;
        //        screenY = screenY + 1;
        //    } while (largestEntryIndex < 0x200);
        //    clutLoopIndex = clutLoopIndex + 0x40;
        //} while (clutLoopIndex < 0x140);
        //datasBinHeader = &g_datasBinHeaderOffset;
        //ReadFileFromCDIntoBuffer(DATAS_BIN, (u_long*)&g_datasBinHeaderOffset, 0, 0x7b8);
        //clutLoopIndex = 0;
        //g_data_buffer = 0;
        //g_data_buffer_length = 0;
        //do
        //{
        //    if (g_data_buffer_length < (int)datasBinHeader[0xb] - (int)datasBinHeader[10])
        //    {
        //        g_data_buffer = clutLoopIndex;
        //        g_data_buffer_length = (int)datasBinHeader[0xb] - (int)datasBinHeader[10];
        //    }
        //    clutLoopIndex = clutLoopIndex + 1;
        //    datasBinHeader = datasBinHeader + 1;
        //} while (clutLoopIndex < 0x1e3);
        //LoadEtc();
        //InitDisplaySystem((int*)DrawOTags, (int*)ClearOrderTables);
        //InitializeOrderingTables();
        InitializeTileRenderingSystem(StaticVariables.g_drawPageParam);
        InitializeAlundraSpriteResourcesFromFile(StaticVariables.DATAS_BIN, _gameEngine.DatasBin.Header.AlundraSpriteInfoOffset, _gameEngine.DatasBin.Header.AlundraSpritesOffset, _gameEngine.DatasBin.Header.AlundraSpritesRepeatOffset, _gameEngine.DatasBin.Header.AlundraStringTableOffset);
        //LoadBalance_bin();
        InitializeDebugVars();
        //LoadAlundraStringTable(DATAS_BIN, _datasBin.Header.AlundraStringTableRepeatOffset);
        using var reader = _gameEngine.DatasBin.OpenBin(); //added by hand
        _gameEngine.AlundraMap.Load(reader, false);

        InitializeRenderingTiles();
        //InitializeDrMoveBuffers();
        LoadFontInTakiFolder();
        //InitSoundSystem();
        InitializeTileRenderer(0x340, 0x100, 0x100, 0x1f0,
            StaticVariables.g_drawModeIndexInit, StaticVariables.g_paletteIndexInit, StaticVariables.g_tileScaleXInit,
            StaticVariables.g_tileScaleYInit, StaticVariables.g_uvLookupTableInit);
        StaticVariables.g_currentMap = ~StaticVariables.g_desiredMap;
    }

    private void InitializePadController()
    {
        PadInit(0);

        StaticVariables.g_padState1 = new PadState();
        StaticVariables.g_padState2 = new PadState();

        ClearPadInputStates();
    }

    private void PadInit(int mode)

    {
        StaticVariables.g_padStateFromPsx = 0xffffffff;
        StaticVariables.g_padMode = mode;
        //ResetCallback();
        //PAD_init();
        //ChangeClearPAD();
    }

    private void ClearPadInputStates()
    {
        StaticVariables.g_padState1.ButtonsHold = 0;
        StaticVariables.g_padState1.MaxNbFrameHeld = 0;
        StaticVariables.g_padState1.RepeatInterval = 0;
        StaticVariables.g_padState1.IsOverThanMaxNbFrameHeld = 0;
        StaticVariables.g_padState1.NumberOfFrameHold = 0;
        StaticVariables.g_padState1.ButtonsHold = 0;
        StaticVariables.g_padState1.ButtonsJustPressed = 0;
        StaticVariables.g_padState1.ButtonsReleased = 0;
        StaticVariables.g_padState1.ButtonsJustPressedByInterval = 0;
    }

    private void InitializeTileRenderingSystem(int drawPageParam)
    {
        StaticVariables.g_drawPageInfoTable = StaticVariables.g_drawPageInfoBase;
        StaticVariables.g_drawPageTPageIDs = StaticVariables.g_tPageIds;
        StaticVariables.g_currentDrawPageParam = drawPageParam;

        //TODO: initialize g_tileSpriteBuffer ?

        StaticVariables.g_tileAnimDescriptorTable = new TileAnimDescriptor[960];

        int tileAnimIndex = 0;

        for (byte spriteIndex = 0; spriteIndex < 6; spriteIndex++)
        {
            byte padding = 0;

            for (int i = 0; i < 16; i++, padding += 0x10)
            {
                drawPageParam = 0;

                while ((drawPageParam + 0x18) < 0x101)
                {
                    StaticVariables.g_tileAnimDescriptorTable[tileAnimIndex] = new TileAnimDescriptor();
                    StaticVariables.g_tileAnimDescriptorTable[tileAnimIndex].SpriteIndex = spriteIndex;
                    StaticVariables.g_tileAnimDescriptorTable[tileAnimIndex].DrawPageOffset = (byte)drawPageParam;
                    StaticVariables.g_tileAnimDescriptorTable[tileAnimIndex].Padding = padding;

                    tileAnimIndex++;
                    drawPageParam += 0x18;
                }
            }
        }

        //var frameIndex = 0;
        //var spriteBufferOffset = 0;
        //var spriteOffset = 0;
        //
        //do {
        //    do {
        //        var pcVar1 = StaticVariables.g_tileAnimDescriptorTable.field2_0x2;
        //        var spriteIndex = 0;
        //
        //        do {
        //            StaticVariables.g_tileAnimDescriptorTable.field0_0x0 = (char)frameIndex;
        //            pcVar1[-1] = (char)spriteIndex;
        //            *pcVar1 = (char)spriteBufferOffset;
        //            pcVar1 = pcVar1 + 3;
        //            spriteOffset = spriteIndex + 0x30;
        //            StaticVariables.g_tileAnimDescriptorTable = (astruct *)&StaticVariables.g_tileAnimDescriptorTable.field_0x3;
        //            spriteIndex = spriteIndex + 0x18;
        //        } while (spriteOffset < 0x101);
        //
        //        spriteBufferOffset = spriteBufferOffset + 0x10;
        //
        //    } while (spriteBufferOffset < 0x100);
        //
        //    frameIndex = frameIndex + 1;
        //    spriteBufferOffset = 0;
        //
        //} while (frameIndex < 6);
    }

    private void InitializeAlundraSpriteResourcesFromFile(string fileName, uint frameDataStart, uint frameDataEnd, uint imageDataStart, uint imageDataEnd)
    {
        /*POLY_FT4 *polyFt4;
        int j;
        POLY_FT4 **pPolyFt4;
        int i;

        i = 0;
        pPolyFt4 = &g_polyFT4Table;
        do {
            j = 0;
            polyFt4 = (POLY_FT4 *)pPolyFt4;
            do {
                SetPolyFT4(polyFt4);
                SetShadeTex(polyFt4,1);
                polyFt4.r0 = 0x80;
                polyFt4.g0 = 0x80;
                polyFt4.b0 = 0x80;
                j = j + 1;
                polyFt4 = polyFt4 + 0x200;
            } while (j < 2);
            i = i + 1;
            pPolyFt4 = (POLY_FT4 **)((int)pPolyFt4 + 0x28);
        } while (i < 0x200);*/
        //ReadFileFromCDIntoBuffer(fileName,(u_long *)g_animationRawData,frameDataStart,frameDataEnd - frameDataStart);
        StaticVariables.g_animationRawSize = (int)(frameDataEnd - frameDataStart);
        //InitAnimationData(StaticVariables.g_animationStructs, StaticVariables.g_animationRawData);
        //LoadImageArea(g_animationStructs_paletteClut,0xc0,0x1e0,0x28);
        //ReadFileFromCDIntoBuffer(fileName,&g_compressedImageData,imageDataStart,imageDataEnd - imageDataStart);
        //LoadCompressedImageToBuffer(&g_compressedImageData,0x140,0x100,8,(u_long *)&g_bufferImage2);
        //DoNothing();
        InitializeEffects();
        InitializeSpriteTileLayouts();
    }

    //8003c17c
    private void InitializeEffects()
    {
        for (int i = 0; i < StaticVariables.g_effectSlots.Length; i++)
        {
            StaticVariables.g_effectSlots[i].Id = i;
        }
    }

    //8003b0e8
    private void InitializeSpriteTileLayouts()
    {
        int entityLinkPtr;
        int innerTileIndex;
        int tileOffset;
        int layoutIndex;
        int index2;
        Entity newEntity;
        int currentEntity;

        layoutIndex = 0;
        StaticVariables.g_nextEntityIndex = 0;
        index2 = 0;
        /*do {
            innerTileIndex = 0;
            tilePtr = (TILE *)&g_spriteTiles;
            do {
                SetTile(tilePtr + layoutIndex * 0x100);
                SetSemiTrans(tilePtr + layoutIndex * 0x100,1);
                innerTileIndex = innerTileIndex + 1;
                tilePtr = tilePtr + 1;
            } while (innerTileIndex < 0x100);
            p = (DR_MODE *)(&DAT_80134234 + index2);
            index2 = index2 + 0xc;
            layoutIndex = layoutIndex + 1;
            SetDrawMode(p,0,0,(uint)DAT_800dc51c,(RECT *)0x0);
        } while (layoutIndex < 2);*/
        index2 = 0;
        layoutIndex = 0;
        do
        {
            tileOffset = 0;
            innerTileIndex = layoutIndex;
            do
            {
                StaticVariables.g_tileToWorldXTable[innerTileIndex] = (short)index2; //useless => why an array ? (x / 24) is used
                //currentEntity = StaticVariables.g_numberOfEntity;
                tileOffset = tileOffset + 1;
                innerTileIndex = layoutIndex + tileOffset;
            } while (tileOffset < 0x18);
            index2 = index2 + 1;
            layoutIndex = layoutIndex + 0x18;
        } while (index2 < 0x34);
        newEntity = null;

        InitializeGameState();
        StaticVariables.g_emptyEntityForClearing.EntityRefId = -1;
    }

    // 80031700
    private void InitializeGameState()
    {
        int iconIndex;
        int iconEtcEntryPtr;
        int playerTileX;
        int playerTileY;
        int playerZ;

        StaticVariables.g_warpTriggerType = 0;
        StaticVariables.g_gravityFlag = 0;
        InitializePlayerStatsAndItems();
        if (StaticVariables.g_someDataIntoRam == 1)
        {
            //CopyInitialDataToRAM(); // maybe map datas already loaded ?
            playerTileX = StaticVariables.g_initialCameraTileX;
            playerTileY = StaticVariables.g_initialCameraTileY;
            playerZ = StaticVariables.g_initialCameraTileZ;
        }
        else
        {
            ClearMapArrays();
            playerTileX = 0x16;
            if (StaticVariables.g_someDataIntoRam == 0)
            {
                playerTileX = 0x21;
                playerTileY = 0x23;
                playerZ = 0;
                StaticVariables.g_initialMapId = 0x185;
                StaticVariables.g_initialCameraTileX = 0x21;
                StaticVariables.g_initialCameraTileY = 0x3b;
                StaticVariables.g_initialCameraTileZ = 0;
                StaticVariables.g_warpExtraParam = 0;
                _gameEngine.PlayerManager.SetPlayerHpMax(10);
                _gameEngine.PlayerManager.SetPlayerHp(10);
                _gameEngine.PlayerManager.SetPlayerMpMax(0);
                _gameEngine.PlayerManager.SetPlayerMp(0);
                _gameEngine.PlayerManager.SetMoney(0);
            }
            else
            {
                playerTileY = 0x1d;
                playerZ = 10;
                StaticVariables.g_initialMapId = 0xb;
                StaticVariables.g_initialCameraTileX = 0x16;
                StaticVariables.g_initialCameraTileY = 0x1d;
                StaticVariables.g_initialCameraTileZ = 10;
                StaticVariables.g_warpExtraParam = 0;
                _gameEngine.PlayerManager.SetPlayerHpMax(0x2d);
                _gameEngine.PlayerManager.SetPlayerHp(0x26);
                _gameEngine.PlayerManager.SetPlayerMpMax(3);
                _gameEngine.PlayerManager.SetPlayerMp(2);
                _gameEngine.PlayerManager.SetMoney(0x873);
                _gameEngine.PlayerManager.InitializeHpAndMp();
            }
            iconIndex = 0;
            //iconEtcEntryPtr = &StaticVariables.g_iconNameEtcBase;
            StaticVariables.g_lastVisitedMapId = 0xffffffff;
            StaticVariables.g_currentSaveSlotNameIndex = 0;
            StaticVariables.g_savedGameplayTime = 0;
            do
            {
                var value = StaticVariables.g_iconNameEtcBase[iconIndex * 2 + 1] >> 16;
                value &= 0xFF;
                if ((value & 0x80) != 0)
                {
                    GetItemUnlockRequirement(iconIndex);
                }
                iconIndex = iconIndex + 1;
                //iconEtcEntryPtr = iconEtcEntryPtr + 2;
            } while (iconIndex < 0x62);
            _gameEngine.PlayerManager.SetPlayerWeaponId(1);
            //InitializeExtraSystemState();
        }

        StaticVariables.g_warpTriggerType = 0x36;
        StaticVariables.g_warpType = 0;
        StaticVariables.g_warpExtraParam = 0;
        StaticVariables.g_cameraLookAtX = (playerTileX * StaticVariables.MapTileWidth + StaticVariables.MapTileWidth / 2) * 0x10000;
        StaticVariables.g_cameraLookAtY = (playerTileY * StaticVariables.MapTileHeight + StaticVariables.MapTileHeight / 2) * 0x10000;
        StaticVariables.g_cameraLookAtZ = playerZ << 0x14;
        StaticVariables.g_desiredMap = StaticVariables.g_initialMapId;
        StaticVariables.g_cameraTargetX = (StaticVariables.g_initialCameraTileX * StaticVariables.MapTileWidth + StaticVariables.MapTileWidth / 2) * 0x10000;
        StaticVariables.g_cameraTargetY = (StaticVariables.g_initialCameraTileY * StaticVariables.MapTileHeight + StaticVariables.MapTileHeight / 2) * 0x10000;
        StaticVariables.g_cameraTargetZ = StaticVariables.g_initialCameraTileZ << 0x14;
        StaticVariables.g_gameplayTime = StaticVariables.g_savedGameplayTime;
    }

    // 8004e530
    private int GetItemUnlockRequirement(int itemId)
    {
        if (itemId < 0 || itemId >= StaticVariables.g_itemsCount)
        {
            Debugger.Break();
            Debug.WriteLine("Invalid itemId in GetItemUnlockRequirement");
            return 0;
        }

        int itemIdIndex = itemId * 2; // In the assembly: itemIdIndex = (itemId * 4) + g_numberOfItems
        short currentUsage = StaticVariables.g_numberOfItems[itemIdIndex + 1];
        int itemPropertyId = (itemId * 5);
        short unlockRequirement = StaticVariables.g_itemsProperties[itemPropertyId + 3];

        // If current usage doesn't match the requirement, increment it
        if (currentUsage != unlockRequirement)
        {
            StaticVariables.g_numberOfItems[itemIdIndex + 1] = (short)(currentUsage + 1);
            return currentUsage + 1;
        }

        return itemId;
    }

    // 8004dac0
    private void InitializePlayerStatsAndItems()
    {
        int i = 0;
        int index = 0;

        StaticVariables.g_initialPlayerStats = new PlayerStats();
        StaticVariables.g_playerStats = StaticVariables.g_initialPlayerStats;
        StaticVariables.g_initialPlayerStats.HpMax = 1;
        StaticVariables.g_initialPlayerStats.Hp = 1;
        StaticVariables.g_initialPlayerStats.MpMax = 0;
        StaticVariables.g_initialPlayerStats.Mp = 0;
        StaticVariables.g_initialPlayerStats.MoneyAmount = 0;
        StaticVariables.g_initialPlayerStats.FalconTemp = 0;
        StaticVariables.g_initialPlayerStats.Falcon = 0;

        //while (i < 0x80)
        //{
        //    if (StaticVariables.g_iconNameEtcBase[index] == 0)
        //    {
        //        break;
        //    }
        //    i++;
        //    index += 2;
        //}

        if (i == 0x80)
        {
            //DoNothing();
        }

        i = 0;
        StaticVariables.g_itemsCount = 99;
        index = 0;

        while (i < 0x80)
        {
            StaticVariables.g_numberOfItems[index] = 0;
            StaticVariables.g_numberOfItems[index + 1] = 0;
            i++;
            index += 2;
        }
    }

    //800814a0
    private void ClearMapArrays()
    {
        for (int i = 0; i < StaticVariables.g_mapFlags.Length; i++)
        {
            StaticVariables.g_mapFlags[i] = 0;
        }

        for (int i = 0; i < StaticVariables.g_mapIdToInternalMapIndexTable.Length; i++)
        {
            StaticVariables.g_mapIdToInternalMapIndexTable[i] = (ushort)(StaticVariables.g_mapIdToInternalMapIndexTable.Length - i - 1);
        }
    }

    //8002ab50
    private void InitializeDebugVars()
    {
        StaticVariables.g_debugFlags = 0;
        StaticVariables.g_debugState = 0;
        StaticVariables.g_debugVar_NbFrameBreak = 0x10;
        StaticVariables.g_debug_desiredMapId = StaticVariables.g_desiredMap;
    }

    //80042984
    private void InitializeRenderingTiles()
    {
        //SetTile(StaticVariables.TILE_8013fb98);
        //SetTile(StaticVariables.TILE_8013fba8);
        //SetSemiTrans(StaticVariables.TILE_8013fb98, 1);
        //SetSemiTrans(StaticVariables.TILE_8013fba8, 1);

        //StaticVariables.TILE_8013fba8.w = 0x140;
        //StaticVariables.TILE_8013fb98.w = 0x140;
        //StaticVariables.TILE_8013fba8.x0 = 0;
        //StaticVariables.TILE_8013fb98.x0 = 0;
        //StaticVariables.TILE_8013fba8.y0 = 0;
        //StaticVariables.TILE_8013fb98.y0 = 0;
        //StaticVariables.TILE_8013fba8.h = 0xf0;
        //StaticVariables.TILE_8013fb98.h = 0xf0;
    }

    private void LoadFontInTakiFolder()
    {
        //ClearOrderTable(StaticVariables.g_orderTableTaki,10);
        //ClearOrderTable(StaticVariables.g_orderTableTaki2,10);
        //LoadTakiScreenWind.tx();
        //LoadtakiScreenWind.cl();
        //LoadFONT3.tim();
        //InitializeTextSpriteTiles();
        //DrawSync(0);
        //ResetTransitionSystem();
        InitializeCameraTransitionState();
    }

    private void InitializeCameraTransitionState()
    {
        StaticVariables.g_cameraTransitionState = 0;
        StaticVariables.g_cameraTransitionStartX = 8;
        StaticVariables.g_cameraTransitionStartY = 0x78;
    }

    private void InitializeTileRenderer(int tPageX, int tPageY, int paletteX, int paletteY, short drawMode,
        short paletteIndex, short tileScaleX, short tileScaleY, int[] uvLookupTablePtr)
    {
        StaticVariables.g_renderingBufferIndex = 0;
        StaticVariables.g_drawModeIndex = drawMode;
        StaticVariables.g_tilePaletteIndex = paletteIndex;
        StaticVariables.g_tileScaleX = tileScaleX;
        StaticVariables.g_tileScaleY = tileScaleY;
        StaticVariables.g_tileUVLookup = uvLookupTablePtr;
        StaticVariables.g_tileTPageX = tPageX;
        StaticVariables.g_tileTPageY = tPageY;
        StaticVariables.g_paletteX = paletteX;
        StaticVariables.g_paletteY = paletteY;
        _gameEngine.SetTileAnimationMode(3, 0);
    }
}