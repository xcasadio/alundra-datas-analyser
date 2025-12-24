using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;
using System.Diagnostics;
using AlundraEngine.Graphics;
using AlundraEngine.UI;

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
        //short tPagePtr = _gameEngine.StaticVariables.g_tPageFadeLUT;
        ushort tPage;
        ushort clutPtr;
        int screenX;
        int screenY;
        int clutLoopIndex = 0;
        int largestEntryIndex = 0;
        byte[] datasBinHeader;

        _gameEngine.StaticVariables.FrameNumber++;

        //VSync(0);
        //ResetCallback();
        //ResetGraph(3);
        //CdInit();
        //InitSound();
        InitializePadController();
        //InitCDRom();
        //InitMemoryCard();
        //FntLoad(0x3c0, 0x100);
        //_gameEngine.StaticVariables.g_fontLoaded = FntOpen(8, 0x20, 0x130, 0xc0, 0, 0x400);
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

        var table = new ushort[160];
        screenY = 0;

        do
        {
            largestEntryIndex = 0x1e0;

            do
            {
                var clut = GetClut(clutLoopIndex, largestEntryIndex);
                table[screenY] = clut;
                largestEntryIndex += 1;
                screenY += 1;
            } while (largestEntryIndex < 0x200);

            clutLoopIndex += 0x40;
        } while (clutLoopIndex < 0x140);

        for (int i = 0; i < _gameEngine.StaticVariables.g_drawPageInfoBase.Length; i++)
        {
            _gameEngine.StaticVariables.g_drawPageInfoBase[i] = table[i];
        }

        var offset = _gameEngine.StaticVariables.g_drawPageInfoBase.Length;

        for (int i = 0; i < _gameEngine.StaticVariables.g_clutTableBase.Length; i++)
        {
            _gameEngine.StaticVariables.g_clutTableBase[i] = table[offset + i];
        }

        offset = _gameEngine.StaticVariables.g_drawPageInfoBase.Length + _gameEngine.StaticVariables.g_clutTableBase.Length;

        for (int i = 0; i < _gameEngine.StaticVariables.g_scrollingClutTableInit.Length; i++)
        {
            _gameEngine.StaticVariables.g_scrollingClutTableInit[i] = table[offset + i];
        }

        //datasBinHeader = &g_dataBinHeader;
        //ReadFileFromCDIntoBuffer(DATAS_BIN, (u_long*)&g_dataBinHeader, 0, 0x7b8);
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
        InitializeTileRenderingSystem(_gameEngine.StaticVariables.g_drawPageParam);
        InitializeAlundraSpriteResourcesFromFile(StaticVariables.DATAS_BIN, _gameEngine.DatasBin.Header.AlundraSpriteRecordsOffset, _gameEngine.DatasBin.Header.AlundraSpriteSheetOffset, _gameEngine.DatasBin.Header.AlundraSpritesRepeatOffset, _gameEngine.DatasBin.Header.AlundraStringTableOffset);
        //LoadBalance_bin();
        InitializeDebugVars();
        //LoadAlundraStringTable(DATAS_BIN, _datasBin.Header.AlundraStringTableRepeatOffset);
        using var reader = _gameEngine.DatasBin.OpenBin(); //added by hand
        _gameEngine.AlundraMap.Load(reader, false);

        InitializeRenderingTiles();
        //InitializeDrMoveBuffers();
        LoadFontInTakiFolder();
        _gameEngine.SoundManager.InitializeSoundSystem();
        InitializeScrollingRenderer(0x340, 0x100, 0x100, 0x1f0,
            _gameEngine.StaticVariables.g_drawModeIndexInit, _gameEngine.StaticVariables.g_paletteIndexInit, _gameEngine.StaticVariables.g_tileScaleXInit,
            _gameEngine.StaticVariables.g_tileScaleYInit, _gameEngine.StaticVariables.g_scrollingClutTableInit);
        _gameEngine.StaticVariables.g_currentMap = ~_gameEngine.StaticVariables.g_desiredMap;
    }

    private ushort GetClut(int x, int y)
    {
        return (ushort)((y << 6) | ((x >> 4) & 0x3f));
    }

    private void InitializePadController()
    {
        PadInit(0);

        _gameEngine.StaticVariables.g_padState1 = new PadState();
        _gameEngine.StaticVariables.g_padState2 = new PadState();

        ClearPadInputStates();
    }

    private void PadInit(int mode)

    {
        _gameEngine.StaticVariables.g_padStateFromPsx = 0xffffffff;
        _gameEngine.StaticVariables.g_padMode = mode;
        //ResetCallback();
        //PAD_init();
        //ChangeClearPAD();
    }

    private void ClearPadInputStates()
    {
        _gameEngine.StaticVariables.g_padState1.ButtonsHold = 0;
        //_gameEngine.StaticVariables.g_padState1.MaxNbFrameHeld = 0;
        _gameEngine.StaticVariables.g_padState1.RepeatInterval = 0;
        _gameEngine.StaticVariables.g_padState1.IsOverThanMaxNbFrameHeld = 0;
        _gameEngine.StaticVariables.g_padState1.NumberOfFrameHold = 0;
        _gameEngine.StaticVariables.g_padState1.ButtonsHold = 0;
        _gameEngine.StaticVariables.g_padState1.ButtonsJustPressed = 0;
        _gameEngine.StaticVariables.g_padState1.ButtonsReleased = 0;
        _gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval = 0;
    }

    private void InitializeTileRenderingSystem(int drawPageParam)
    {
        _gameEngine.StaticVariables.g_drawPageInfoTable = _gameEngine.StaticVariables.g_drawPageInfoBase;
        _gameEngine.StaticVariables.g_drawPageTPageIDs = _gameEngine.StaticVariables.g_tPageIds;
        _gameEngine.StaticVariables.g_currentDrawPageParam = drawPageParam;

        //TODO: initialize g_tileSpriteBuffer ?

        _gameEngine.StaticVariables.g_tileAnimDescriptorTable = new TileAnimDescriptor[960];

        int tileAnimIndex = 0;

        for (byte spriteIndex = 0; spriteIndex < 6; spriteIndex++)
        {
            byte padding = 0;

            for (int i = 0; i < 16; i++, padding += 0x10)
            {
                drawPageParam = 0;

                while ((drawPageParam + 0x18) < 0x101)
                {
                    _gameEngine.StaticVariables.g_tileAnimDescriptorTable[tileAnimIndex] = new TileAnimDescriptor();
                    _gameEngine.StaticVariables.g_tileAnimDescriptorTable[tileAnimIndex].SpriteIndex = spriteIndex;
                    _gameEngine.StaticVariables.g_tileAnimDescriptorTable[tileAnimIndex].DrawPageOffset = (byte)drawPageParam;
                    _gameEngine.StaticVariables.g_tileAnimDescriptorTable[tileAnimIndex].Padding = padding;

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
        //        var pcVar1 = _gameEngine.StaticVariables.g_tileAnimDescriptorTable.field2_0x2;
        //        var spriteIndex = 0;
        //
        //        do {
        //            _gameEngine.StaticVariables.g_tileAnimDescriptorTable.field0_0x0 = (char)frameIndex;
        //            pcVar1[-1] = (char)spriteIndex;
        //            *pcVar1 = (char)spriteBufferOffset;
        //            pcVar1 = pcVar1 + 3;
        //            spriteOffset = spriteIndex + 0x30;
        //            _gameEngine.StaticVariables.g_tileAnimDescriptorTable = (astruct *)&_gameEngine.StaticVariables.g_tileAnimDescriptorTable.field_0x3;
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
        for (int i = 0; i < _gameEngine.StaticVariables.g_alundraSprites.Length; i++)
        {
            var poly = _gameEngine.StaticVariables.g_alundraSprites[i];
            poly.r0 = 255;
            poly.g0 = 255;
            poly.b0 = 255;
        }

        //ReadFileFromCDIntoBuffer(fileName,(u_long *)g_animationRawData,frameDataStart,frameDataEnd - frameDataStart);
        _gameEngine.StaticVariables.g_animationRawSize = (int)(frameDataEnd - frameDataStart);
        //InitAnimationData(_gameEngine.StaticVariables.g_animationStructs, _gameEngine.StaticVariables.g_animationRawData);
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
        for (int i = 0; i < _gameEngine.StaticVariables.g_effectSlots.Length; i++)
        {
            _gameEngine.StaticVariables.g_effectSlots[i].Id = i;
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
        _gameEngine.StaticVariables.g_nextEntityIndex = 0;
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
                _gameEngine.StaticVariables.g_tileToWorldXTable[innerTileIndex] = (short)index2; //useless => why an array ? (x / 24) is used
                //currentEntity = _gameEngine.StaticVariables.g_numberOfEntity;
                tileOffset += 1;
                innerTileIndex = layoutIndex + tileOffset;
            } while (tileOffset < 0x18);
            index2 += 1;
            layoutIndex += 0x18;
        } while (index2 < 0x34);
        newEntity = null;

        InitializeGameState();
        _gameEngine.StaticVariables.g_emptyEntityForClearing.EntityRefId = -1;
    }

    // 80031700
    private void InitializeGameState()
    {
        int iconIndex;
        int iconEtcEntryPtr;
        int playerTileX;
        int playerTileY;
        int playerZ;

        _gameEngine.StaticVariables.g_resetAnimationId = 0;
        _gameEngine.StaticVariables.g_gravityFlag = 0;
        InitializePlayerStatsAndItems();

        if (_gameEngine.StaticVariables.g_saveDataInRam.SlotData == 1)
        {
            _gameEngine.UpdateSaveData();
            playerTileX = _gameEngine.StaticVariables.g_saveData.CameraTileX;
            playerTileY = _gameEngine.StaticVariables.g_saveData.CameraTileY;
            playerZ = _gameEngine.StaticVariables.g_saveData.CameraTileZ;
        }
        else
        {
            ResetMapFlags();
            playerTileX = 0x16;

            if (_gameEngine.StaticVariables.g_saveDataInRam.SlotData == 0)
            {
                playerTileX = 0x21;
                playerTileY = 0x23;
                playerZ = 0;
                _gameEngine.StaticVariables.g_saveData.InitialMapId = 0x185;
                _gameEngine.StaticVariables.g_saveData.CameraTileX = 0x21;
                _gameEngine.StaticVariables.g_saveData.CameraTileY = 0x3b;
                _gameEngine.StaticVariables.g_saveData.CameraTileZ = 0;
                _gameEngine.StaticVariables.g_resetDirectionId = 0;
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
                _gameEngine.StaticVariables.g_saveData.InitialMapId = 0xb;
                _gameEngine.StaticVariables.g_saveData.CameraTileX = 0x16;
                _gameEngine.StaticVariables.g_saveData.CameraTileY = 0x1d;
                _gameEngine.StaticVariables.g_saveData.CameraTileZ = 10;
                _gameEngine.StaticVariables.g_resetDirectionId = 0;
                _gameEngine.PlayerManager.SetPlayerHpMax(0x2d);
                _gameEngine.PlayerManager.SetPlayerHp(0x26);
                _gameEngine.PlayerManager.SetPlayerMpMax(3);
                _gameEngine.PlayerManager.SetPlayerMp(2);
                _gameEngine.PlayerManager.SetMoney(0x873);
                _gameEngine.HudManager.InitializeHpAndMp();
            }

            iconIndex = 0;
            _gameEngine.StaticVariables.g_saveData.LastMapId = 0xffffffff;
            _gameEngine.StaticVariables.g_saveData.SaveSlotIndex = 0;
            _gameEngine.StaticVariables.g_saveData.GameTime = 0;

            do
            {
                var value = _gameEngine.StaticVariables.g_itemDropProperties[iconIndex].Field3;
                if ((value & 0x80) != 0)
                {
                    _gameEngine.PlayerManager.AddOneItemIfUnlocked(iconIndex);
                }
                iconIndex += 1;
            } while (iconIndex < 0x62);

            _gameEngine.PlayerManager.SetPlayerWeaponId(1);
            //InitializeNumberOfItems();
        }

        _gameEngine.StaticVariables.g_resetAnimationId = 0x36;
        _gameEngine.StaticVariables.g_mapTransitionEffectId = 0;
        _gameEngine.StaticVariables.g_resetDirectionId = 0;
        _gameEngine.StaticVariables.g_cameraLookAtX = (playerTileX * StaticVariables.MapTileWidth + StaticVariables.MapTileWidth / 2) * 0x10000;
        _gameEngine.StaticVariables.g_cameraLookAtY = (playerTileY * StaticVariables.MapTileHeight + StaticVariables.MapTileHeight / 2) * 0x10000;
        _gameEngine.StaticVariables.g_cameraLookAtZ = playerZ << 0x14;
        _gameEngine.StaticVariables.g_desiredMap = _gameEngine.StaticVariables.g_saveData.InitialMapId;
        _gameEngine.StaticVariables.g_cameraTargetX = (_gameEngine.StaticVariables.g_saveData.CameraTileX * StaticVariables.MapTileWidth + StaticVariables.MapTileWidth / 2) * 0x10000;
        _gameEngine.StaticVariables.g_cameraTargetY = (_gameEngine.StaticVariables.g_saveData.CameraTileY * StaticVariables.MapTileHeight + StaticVariables.MapTileHeight / 2) * 0x10000;
        _gameEngine.StaticVariables.g_cameraTargetZ = _gameEngine.StaticVariables.g_saveData.CameraTileZ << 0x14;
        _gameEngine.StaticVariables.g_gameplayTime = _gameEngine.StaticVariables.g_saveData.GameTime;

        //==== DEBUG
        //enable HUD
        //_gameEngine.StaticVariables.g_saveData.MapFlags[51] |= 40000000;

        //_gameEngine.StaticVariables.g_desiredMap = 452; //11; //471;

        //active la map 476 Alundra cabine
        //_gameEngine.StaticVariables.g_desiredMap = 476;
        //_gameEngine.StaticVariables.g_saveData.MapFlags[51] |= 256;
        //_gameEngine.StaticVariables.g_mapTransitionEffectId = 4;
        //_gameEngine.StaticVariables.g_warpSoundEffectId = 73;
        //_gameEngine.StaticVariables.g_cameraTargetZ = 786432;
        //_gameEngine.StaticVariables.g_cameraTargetY = 524288;
        //_gameEngine.StaticVariables.g_cameraTargetX = 3145728;
        //_gameEngine.StaticVariables.g_resetDirectionId = 0;
        //_gameEngine.StaticVariables.g_resetAnimationId = 13;
        //_gameEngine.StaticVariables.g_isGameEnding = 1;

        //active la map 416 Inoa beach
        //_gameEngine.StaticVariables.g_desiredMap = 416;
        //_gameEngine.StaticVariables.g_globalFlags[0] = 1;
        //_gameEngine.StaticVariables.g_saveData.MapFlags[6] = 256;
        //_gameEngine.StaticVariables.g_saveData.MapFlags[26] = 268435456;
        //_gameEngine.StaticVariables.g_saveData.MapFlags[27] = 228;
        //_gameEngine.StaticVariables.g_saveData.MapFlags[51] = 512;
        //_gameEngine.StaticVariables.g_mapTransitionEffectId = 2;
        //_gameEngine.StaticVariables.g_warpSoundEffectId = 0;
        //_gameEngine.StaticVariables.g_cameraTargetZ = 1048576;
        //_gameEngine.StaticVariables.g_cameraTargetY = 51904512;
        //_gameEngine.StaticVariables.g_cameraTargetX = 65273856;
        //_gameEngine.StaticVariables.g_resetDirectionId = 16;
        //_gameEngine.StaticVariables.g_resetAnimationId = 0;
        //_gameEngine.StaticVariables.g_isGameEnding = 1;

        //active la map 163 Jess's house
        //_gameEngine.StaticVariables.g_desiredMap = 163;
        //_gameEngine.StaticVariables.g_globalFlags[0] = 1;
        //_gameEngine.StaticVariables.g_saveData.MapFlags[6] = 256;
        //_gameEngine.StaticVariables.g_saveData.MapFlags[26] = 268435456;
        //_gameEngine.StaticVariables.g_saveData.MapFlags[27] = 228;
        //_gameEngine.StaticVariables.g_saveData.MapFlags[51] = 512;
        //_gameEngine.StaticVariables.g_mapTransitionEffectId = 2;
        //_gameEngine.StaticVariables.g_warpSoundEffectId = 0;
        //_gameEngine.StaticVariables.g_cameraTargetZ = 2097152;
        //_gameEngine.StaticVariables.g_cameraTargetY = 9961472;
        //_gameEngine.StaticVariables.g_cameraTargetX = 63700992;
        //_gameEngine.StaticVariables.g_resetDirectionId = 16;
        //_gameEngine.StaticVariables.g_resetAnimationId = 78;
        //_gameEngine.StaticVariables.g_isGameEnding = 1;
    }

    // 8004dac0
    public void InitializePlayerStatsAndItems()
    {
        int i = 0;
        int index = 0;

        _gameEngine.StaticVariables.g_saveData.PlayerStats = new PlayerStats();
        _gameEngine.StaticVariables.g_playerStats = _gameEngine.StaticVariables.g_saveData.PlayerStats;
        _gameEngine.StaticVariables.g_saveData.PlayerStats.HpMax = 1;
        _gameEngine.StaticVariables.g_saveData.PlayerStats.Hp = 1;
        _gameEngine.StaticVariables.g_saveData.PlayerStats.MpMax = 0;
        _gameEngine.StaticVariables.g_saveData.PlayerStats.Mp = 0;
        _gameEngine.StaticVariables.g_saveData.PlayerStats.MoneyAmount = 0;
        _gameEngine.StaticVariables.g_saveData.PlayerStats.FalconTemp = 0;
        _gameEngine.StaticVariables.g_saveData.PlayerStats.Falcon = 0;

        //while (i < 0x80)
        //{
        //    if (_gameEngine.StaticVariables.g_itemDropProperties[index] == 0)
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
        _gameEngine.StaticVariables.g_itemsCount = 99;
        index = 0;

        while (i < 0x80)
        {
            _gameEngine.StaticVariables.g_saveData.NumberOfItems[index] = 0;
            _gameEngine.StaticVariables.g_saveData.NumberOfItems[index + 1] = 0;
            i++;
            index += 2;
        }
    }

    //800814a0
    private void ResetMapFlags()
    {
        for (int i = 0; i < _gameEngine.StaticVariables.g_saveData.MapFlags.Length; i++)
        {
            _gameEngine.StaticVariables.g_saveData.MapFlags[i] = 0;
        }

        for (int i = 0; i < _gameEngine.StaticVariables.g_saveData.MapIdToInternalMapIndexTable.Length; i++)
        {
            _gameEngine.StaticVariables.g_saveData.MapIdToInternalMapIndexTable[i] = (ushort)(_gameEngine.StaticVariables.g_saveData.MapIdToInternalMapIndexTable.Length - i - 1);
        }
    }

    //8002ab50
    private void InitializeDebugVars()
    {
        _gameEngine.StaticVariables.g_debugFlags = 0;
        _gameEngine.StaticVariables.g_debugState = 0;
        _gameEngine.StaticVariables.g_debugVar_NbFrameBreak = 0x10;
        _gameEngine.StaticVariables.g_debug_desiredMapId = _gameEngine.StaticVariables.g_desiredMap;
    }

    //80042984
    private void InitializeRenderingTiles()
    {
        //SetTile(_gameEngine.StaticVariables.TILE_8013fb98);
        //SetTile(_gameEngine.StaticVariables.TILE_8013fba8);
        //SetSemiTrans(_gameEngine.StaticVariables.TILE_8013fb98, 1);
        //SetSemiTrans(_gameEngine.StaticVariables.TILE_8013fba8, 1);

        //_gameEngine.StaticVariables.TILE_8013fba8.w = StaticVariables.MapTileWidth;
        _gameEngine.StaticVariables.TILE_8013fb98.w = StaticVariables.ScreenWidth;
        //_gameEngine.StaticVariables.TILE_8013fba8.x0 = 0;
        _gameEngine.StaticVariables.TILE_8013fb98.x0 = 0;
        //_gameEngine.StaticVariables.TILE_8013fba8.y0 = 0;
        _gameEngine.StaticVariables.TILE_8013fb98.y0 = 0;
        //_gameEngine.StaticVariables.TILE_8013fba8.h = StaticVariables.MapTileHeight;
        _gameEngine.StaticVariables.TILE_8013fb98.h = StaticVariables.ScreenHeight;
    }

    //80044be4
    private void LoadFontInTakiFolder()
    {
        //ClearOrderTable(_gameEngine.StaticVariables.g_orderTableTaki,10);
        //ClearOrderTable(_gameEngine.StaticVariables.g_orderTableTaki2,10);
        //LoadTakiScreenWind_tx();
        LoadtakiScreenWind_cl();
        LoadFONT3_tim();
        InitializeTextSpriteTiles();
        //DrawSync(0);
        ResetTransitionSystem();
        InitializeCameraTransitionState();
    }

    //80044b48
    private void LoadtakiScreenWind_cl()
    {
        //u_short clutId;
        //int clutIndex;
        //ushort* clutTablePtr;
        //RECT clutRect;
        //
        //ReadFileFromCDIntoBuffer("taki\\screen\\wind.cl", (u_long*)g_wind_tx_buffer, 0, 0x200);

        var clutIndex = 0;

        do
        {
            var clutId = GetClut(0x120, clutIndex + 0x1e0);
            _gameEngine.StaticVariables.g_clutTable[clutIndex] = clutId;
            clutIndex = clutIndex + 1;
        } while (clutIndex < 0x10);

        //clutRect.x = 0x120;
        //clutRect.y = 0x1e0;
        //clutRect.w = 0x10;
        //clutRect.h = 0x10;
        //LoadImage(&clutRect, (u_long*)g_wind_tx_buffer);
    }

    //80044f88
    private void LoadFONT3_tim()
    {
        _gameEngine.StaticVariables.g_textState = 4;
        _gameEngine.StaticVariables.g_textPosX = 0;
        _gameEngine.StaticVariables.g_textPosY = 0;
        _gameEngine.StaticVariables.g_textOffsetX = 0;
        _gameEngine.StaticVariables.g_textOffsetY = 0;
        _gameEngine.StaticVariables.g_textCurrentPage = 0;
        _gameEngine.StaticVariables.g_textNextChoice = 8;
        _gameEngine.StaticVariables.g_textSelectionNext = 10;
        _gameEngine.StaticVariables.g_textBufferSize = 0x168;
        //ReadFileFromCDIntoBuffer("taki\\screen\\FONT3.tim", (u_long*)g_bufferFONT3_tim, 0, 0x8220);
    }

    //8005a0c8
    private void InitializeTextSpriteTiles()
    {
        int tileX;
        int tileY;
        UIBoxConfiguration tilesConfiguration;
        int surfaceIndex;

        surfaceIndex = 0;
        _gameEngine.StaticVariables.g_UIDisplayFlags = 0;
        tilesConfiguration = _gameEngine.StaticVariables.g_textTilesConfiguration;

        do
        {
            tileY = 0;

            if (0 < tilesConfiguration.Height)
            {
                do
                {
                    tileX = 0;

                    if (0 < tilesConfiguration.Width)
                    {
                        SPRT[] sprites = (surfaceIndex == 0) ? tilesConfiguration.SpritesA : tilesConfiguration.SpritesB;

                        do
                        {
                            var tileIndex = tileY * tilesConfiguration.Width + tileX;
                            var sprite = sprites[tileIndex];
                            //SetSprt(sprite);
                            //SetSemiTrans(sprite, 0);
                            //SetShadeTex(sprite, 1);
                            sprite.clut = 0; //_gameEngine.StaticVariables.g_clutTable[0];

                            //TODO cache images with u and v coordinates
                            //_gameEngine.Font3.GenerateFontBitmapFromSprite(sprite);
                            //_gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, _gameEngine.Font3.GenerateFontBitmapTim(_gameEngine.StaticVariables.g_clutTable[0]));

                            tileX += 1;
                        } while (tileX < tilesConfiguration.Width);
                    }

                    tileY += 1;

                } while (tileY < tilesConfiguration.Height);
            }

            surfaceIndex += 1;

        } while (surfaceIndex < 2);
    }

    //80047c50
    private void ResetTransitionSystem()
    {
        _gameEngine.StaticVariables.g_currentTransitionType = 0;
        _gameEngine.StaticVariables.g_activeTransitionCallback = null;
        _gameEngine.SubInventoryManager.InitializeSubInventorySprite();
        _gameEngine.MainInventoryManager.InitializeInventorySpriteNumberOf();
        FUN_80058394();
    }

    //80058394
    private void FUN_80058394()
    {
        FUN_80058204(_gameEngine.StaticVariables.UIBoxConfiguration_800bcb30);
        FUN_80058204(_gameEngine.StaticVariables.UIBoxConfiguration_800bf2a0);
        FUN_80058204(_gameEngine.StaticVariables.UIBoxConfiguration_800c1a10);
        FUN_80058204(_gameEngine.StaticVariables.UIBoxConfiguration_800c4180);
    }

    //80058204
    private void FUN_80058204(UIBoxConfiguration uiBoxConfig)
    {
        short sVar1;
        int index;
        int w;
        int h;
        int i;
        i = 0;

        //do
        //{
            h = 0;

            if (0 < uiBoxConfig.Height)
            {
                do
                {
                    w = 0;

                    if (0 < uiBoxConfig.Width)
                    {
                        do
                        {
                            index = h * uiBoxConfig.Width + w;
                            var sprite = uiBoxConfig.SpritesA[index];
                            //SetSprt(sprite);
                            //SetSemiTrans(sprite, 0);
                            //SetShadeTex(sprite, 1);
                            //sprite.clut = g_clutTable[sprite.clut];

                            w = w + 1;
                        } while (w < uiBoxConfig.Width);
                    }

                    h = h + 1;
                } while (h < uiBoxConfig.Height);
            }

            i = i + 1;
        //} while (i < 2);
    }

    //80057b64
    private void InitializeCameraTransitionState()
    {
        _gameEngine.StaticVariables.g_hudTransitionState = 0;
        _gameEngine.StaticVariables.g_hudTransitionStartX = 8;
        _gameEngine.StaticVariables.g_hudTransitionStartY = 0x78;
    }

    private void InitializeScrollingRenderer(int tPageX, int tPageY, int u, int v, short drawMode,
        short paletteIndex, short tileScaleX, short tileScaleY, ushort[] clutTable)
    {
        _gameEngine.StaticVariables.g_renderingBufferIndex = 0;
        _gameEngine.StaticVariables.g_drawModeIndex = drawMode;
        _gameEngine.StaticVariables.g_tilePaletteIndex = paletteIndex;
        _gameEngine.StaticVariables.g_tileScaleX = tileScaleX;
        _gameEngine.StaticVariables.g_tileScaleY = tileScaleY;
        _gameEngine.StaticVariables.g_scrollingClutTable = clutTable;
        _gameEngine.StaticVariables.g_tileTPageX = tPageX;
        _gameEngine.StaticVariables.g_tileTPageY = tPageY;
        _gameEngine.StaticVariables.g_scrollingTextureX = u;
        _gameEngine.StaticVariables.g_scrollingTextureY = v;
        _gameEngine.SetScrollingMode(3, 0);
    }
}