using System.Diagnostics;
using Alundra.DatasBin;
using Alundra.Gameplay;
using Alundra.Gameplay.Scripts;
using Alundra.Sound;
//using Alundra.Sprite;
using Alundra.Text;

namespace Alundra;

public class GameEngine
{
    private readonly DatasBin.DatasBin _datasBin;
    public BalanceBin BalanceBin;
    public SoundBin SoundBin;
    private readonly EtcResR _etcResR;
    private readonly Font3 _font3;

    public GameMap CurrentMap { get; private set; }
    public GameMap AlundraMap => _datasBin.AlundraGameMap;

    //find the staticVariables for this=>
    //private int NumSprites;
    //private readonly SpriteRef[] SpriteRefs = new SpriteRef[10000];
    private readonly EntityEventHandlers _entityEventHandlers;


    public GameEngine(DatasBin.DatasBin datasBin, BalanceBin balanceBin, SoundBin soundBin, EtcResR etcResR, Font3 font3)
    {
        _datasBin = datasBin;
        BalanceBin = balanceBin;
        SoundBin = soundBin;
        _etcResR = etcResR;
        _font3 = font3;

        _entityEventHandlers = new EntityEventHandlers(this);
    }

    public void InitializeEngine()
    {
        StaticVariables.Initialize();
    }

    public void MainLoop(Graphics graphics)
    {
        //InitializeGame();
        StaticVariables.g_spriteNumberOfImage = 0;

        byte isEffectRunning = 0;
        var playerPosX = 0;
        var playerPosY = 0;
        var playerPosZ = 0;

        //do
        //{
        //while (true)
        //{

        if (StaticVariables.g_isGameEnding != 0)
        {
            InitStaticVariable();
            StaticVariables.INT_800dc4e4 = 0;
            StaticVariables.g_isGameEnding = 0;
            StaticVariables.g_warpEntryBehavior = 0;

            if (StaticVariables.g_desiredMap != StaticVariables.g_currentMap)
            {
                StaticVariables.g_currentMap = StaticVariables.g_desiredMap;
                //ReadFileFromCDIntoBuffer(StaticVariables.DATAS_BIN, StaticVariables.g_compressedImageData, (StaticVariables.INT_801eab58)[StaticVariables.g_desiredMap], (StaticVariables.INT_801eab5c)[StaticVariables.g_desiredMap] - (StaticVariables.INT_801eab58)[StaticVariables.g_desiredMap]);
                InitializeMapSpriteTable(null, null, 0);
                //StaticVariables.g_compressedImageData + ??,
                //StaticVariables.g_compressedImageData + StaticVariables.DAT_80191b34,
                //StaticVariables.g_compressedImageData + StaticVariables.DAT_80191b38);
                LoadMapSpriteTable(null); //StaticVariables.g_compressedImageData[StaticVariables.g_mapIndexInDatasBin]);
                FUN_800423ec(-1/*StaticVariables.g_compressedImageData[StaticVariables.g_animTableAlt_80191b48]*/);
                //InitializeTileSet(StaticVariables.g_currentMap, StaticVariables.g_compressedImageData[StaticVariables.g_tileSet_index_80191b44]);
                playerPosX = StaticVariables.g_spriteDataBase[0xc] << 3;
                playerPosY = StaticVariables.g_spriteDataBase[0xd] << 3;
                playerPosZ = StaticVariables.g_spriteDataBase[0xe] << 3;
            }

            //DoNothing();
            ClearGlobalFlags();
            ResetCameraAndLoadVRAMAssets();
            FUN_80044520(StaticVariables.g_spriteDataBase[0xb]);
            LoadMapAndInitializeEntities(null/*StaticVariables.g_compressedImageData + StaticVariables.DAT_80191b40*/);
            WarpPlayer(playerPosX, playerPosY, playerPosZ, StaticVariables.g_warpType);
            InitializeTileAnimationSystem();
            PrepareBufferFlip();
            OpenMap(StaticVariables.g_currentMap);
            UpdateEntities(1);
            ResetDebugRenderingState();
        }


        //do
        //{
        StaticVariables.g_debugMessage = "";
        //PrintDebug();
        RenderScene(graphics);
        UpdateEntities(0);
        //FntPrint();
        var ndDebugFrame = 1;
        if (StaticVariables.g_debugState < 0 && (StaticVariables.g_debugFlags & 0x40000000) != 0)
        {
            ndDebugFrame = StaticVariables.g_debugVar_NbFrameBreak;
        }
        //PauseGameDuringNbFrame(ndDebugFrame);
        //DoNothing();
        //} while (StaticVariables.g_isGameEnding == 0);

        if (StaticVariables.g_isGameEnding != 0)
        {
            //HandleMapSoundEffects(StaticVariables.g_desiredMap, StaticVariables.g_warpEntryBehavior);
            StaticVariables.g_warpEntryBehavior = 0;
            StartWarpTransition(StaticVariables.g_warpType);
            StaticVariables.INT_800dc4e4 = 1;
            do
            {
                StaticVariables.g_debugMessage = "";
                UpdatePads();
                //isEffectRunning = FUN_80044440(StaticVariables.g_orderingTableBuffer + 3, StaticVariables.g_warpType);
                //HandleMapSoundStreaming();
                //PauseGameDuringNbFrame(1);
                //DoNothing();
            } while (isEffectRunning != 0);

            EndFrame();
            //FUN_80049ff8(); //sound
            if (StaticVariables.g_warpType != 9)
            {
                return; //break;
            }

            //LoadBgm(0);
            //LoadSomethingInDatasBin(g_indexInDatasBin);
            //_96_remove();
            //_96_init();
            //syscall();
            //LoadExec();
            //DoNothing();
            System.Diagnostics.Debugger.Break();
            Environment.Exit(0);

            LAB_8002c590:
            //LoadBgm(0);
            StaticVariables.g_playerControlFlags = 0;
            //InitializeMapWarpPosition();
            //}
            //if (9 < StaticVariables.g_warpType)
            //{
            //    if (StaticVariables.g_warpType != 10)
            //    {
            //        if (StaticVariables.g_warpType == 0xb)
            //        {
            //            LoadBgm(0);
            //            LoadSomethingInDatasBin(g_indexInDatasBin);
            //            LoadLOADER_EXE();
            //            DoNothing();
            //            exit();
            //        }
            //        goto LAB_8002c5dc;
            //    }
            //    goto LAB_8002c590;
            //}
            //if (StaticVariables.g_warpType == 8)
            //{
            //    StaticVariables.g_systemFlags = StaticVariables.g_systemFlags & 0xbfffffff;
            //}
            //else
            //{
            //    //LAB_8002c5dc:
            //    //DoNothing();
            //}
        }

        //} while (true);
    }

    public void InitializeGame()
    {
        short tPagePtr = StaticVariables.g_tPageFadeLUT;
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
        InitPadController();
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
        //InitializeTileRenderingSystem(g_drawPageParam);

        InitSpriteResourcesFromFile(StaticVariables.DATAS_BIN,
            StaticVariables.g_datasBinHeaderOffset, StaticVariables.g_spriteBufferCDEnd,
            StaticVariables.g_imageBufferCDStart, StaticVariables.g_imageBufferCDEnd);
        //LoadBalance.bin();
        InitDebugVars();
        //LoadAnimTableMaybeInDatasBin(DATAS_BIN, INT_801eab40);
        FUN_80042984();
        //InitializeDrMoveBuffers();
        LoadFontInTakiFolder();
        //InitSoundSystem();
        InitTileRenderer(0x340, 0x100, 0x100, 0x1f0,
            StaticVariables.g_drawModeIndexInit, StaticVariables.g_paletteIndexInit, StaticVariables.g_tileScaleXInit,
            StaticVariables.g_tileScaleYInit, StaticVariables.g_uvLookupTableInit);
        StaticVariables.g_currentMap = ~StaticVariables.g_desiredMap;
    }

    private void InitPadController()
    {
        PadInit(0);

        StaticVariables.g_padState2 = new PadState();
        StaticVariables.g_padState1 = new PadState();

        ClearPadInputStates();
    }


    void PadInit(int mode)

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
        StaticVariables.g_padState1.ButtonReleased = 0;
        StaticVariables.g_padState1.ButtonsJustPressedByInterval = 0;
    }


    private void InitSpriteResourcesFromFile(string fileName, int frameDataStart, int frameDataEnd,
        int imageDataStart, int imageDataEnd)
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
                polyFt4->r0 = 0x80;
                polyFt4->g0 = 0x80;
                polyFt4->b0 = 0x80;
                j = j + 1;
                polyFt4 = polyFt4 + 0x200;
            } while (j < 2);
            i = i + 1;
            pPolyFt4 = (POLY_FT4 **)((int)pPolyFt4 + 0x28);
        } while (i < 0x200);*/
        //ReadFileFromCDIntoBuffer(fileName,(u_long *)g_animationRawData,frameDataStart,frameDataEnd - frameDataStart);
        StaticVariables.g_animationRawSize = frameDataEnd - frameDataStart;
        //InitAnimationData(StaticVariables.g_animationStructs, StaticVariables.g_animationRawData);
        //LoadImageArea(g_animationStructs_paletteClut,0xc0,0x1e0,0x28);
        //ReadFileFromCDIntoBuffer(fileName,&g_compressedImageData,imageDataStart,imageDataEnd - imageDataStart);
        //LoadCompressedImageToBuffer(&g_compressedImageData,0x140,0x100,8,(u_long *)&g_bufferImage2);
        //DoNothing();
        //InitSpriteGroupIndex();
        InitSpriteTileLayouts();
    }

    private void InitSpriteTileLayouts()
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
                StaticVariables.g_tileToWorldXTable[innerTileIndex] = (short)index2;
                currentEntity = StaticVariables.g_numberOfEntity;
                tileOffset = tileOffset + 1;
                innerTileIndex = layoutIndex + tileOffset;
            } while (tileOffset < 0x18);
            index2 = index2 + 1;
            layoutIndex = layoutIndex + 0x18;
        } while (index2 < 0x34);
        newEntity = null;

        InitGameStateFromWarpTrigger();
        StaticVariables.g_emptyEntityForClearing.EntityRefId = -1;
    }

    private void InitGameStateFromWarpTrigger()
    {
        int iconIndex;
        int iconEtcEntryPtr;
        int playerTileX;
        int playerTileY;
        int playerZ;

        StaticVariables.g_warpTriggerType = 0;
        StaticVariables.g_gravityFlag = 0;
        InitializeWarpAndFadeSystem();
        if (StaticVariables.g_someDataIntoRam == 1)
        {
            //CopyInitialDataToRAM();
            playerTileX = StaticVariables.g_initialWarpTileX;
            playerTileY = StaticVariables.g_initialWarpTileY;
            playerZ = StaticVariables.g_initialWarpZ;
        }
        else
        {
            //ClearSomeArrays();
            playerTileX = 0x16;
            if (StaticVariables.g_someDataIntoRam == 0)
            {
                playerTileX = 0x21;
                playerTileY = 0x23;
                playerZ = 0;
                StaticVariables.g_initialWarpMap = 0x185;
                StaticVariables.g_initialWarpTileX = 0x21;
                StaticVariables.g_initialWarpTileY = 0x3b;
                StaticVariables.g_initialWarpZ = 0;
                StaticVariables.g_warpExtraParam = 0;
                UpdateEntityFromWarpFlag(10);
                FinalizeWarpEntities(10);
                SetMaxFadeLevel(0);
                SetFadeTargetLevel(0);
                ApplyFadeLevel(0);
            }
            else
            {
                playerTileY = 0x1d;
                playerZ = 10;
                StaticVariables.g_initialWarpMap = 0xb;
                StaticVariables.g_initialWarpTileX = 0x16;
                StaticVariables.g_initialWarpTileY = 0x1d;
                StaticVariables.g_initialWarpZ = 10;
                StaticVariables.g_warpExtraParam = 0;
                UpdateEntityFromWarpFlag(0x2d);
                FinalizeWarpEntities(0x26);
                SetMaxFadeLevel(3);
                SetFadeTargetLevel(2);
                ApplyFadeLevel(0x873);
                SetupPostWarpGraphics();
            }
            iconIndex = 0;
            //iconEtcEntryPtr = &StaticVariables.g_iconNameEtcBase;
            StaticVariables.g_lastVisitedMapId = 0xffffffff;
            StaticVariables.g_currentSaveSlotNameIndex = 0;
            StaticVariables.g_savedGameplayTime = 0;
            //do
            //{
            //    if ((*(byte*)((int)iconEtcEntryPtr + 6) & 0x80) != 0)
            //    {
            //        GetMapUnlockRequirement(iconIndex);
            //    }
            //    iconIndex = iconIndex + 1;
            //    iconEtcEntryPtr = iconEtcEntryPtr + 2;
            //} while (iconIndex < 0x62);
            LoadWarpVisuals(1);
            //InitializeExtraSystemState();
        }
        StaticVariables.g_warpTriggerType = 0x36;
        StaticVariables.g_warpType = 0;
        StaticVariables.g_warpExtraParam = 0;
        StaticVariables.g_cameraLookAtX = (playerTileX * 0x18 + 0xc) * 0x10000;
        StaticVariables.g_cameraLookAtY = (playerTileY * 0x10 + 8) * 0x10000;
        StaticVariables.g_cameraLookAtZ = playerZ << 0x14;
        StaticVariables.g_desiredMap = StaticVariables.g_initialWarpMap;
        StaticVariables.g_cameraTargetX = (StaticVariables.g_initialWarpTileX * 0x18 + 0xc) * 0x10000;
        StaticVariables.g_cameraTargetY = (StaticVariables.g_initialWarpTileY * 0x10 + 8) * 0x10000;
        StaticVariables.g_animation_id = StaticVariables.g_initialWarpZ << 0x14;
        StaticVariables.g_gameplayTime = StaticVariables.g_savedGameplayTime;
    }

    private void InitializeWarpAndFadeSystem()
    {
        int i = 0;
        int index = 0;

        //StaticVariables.g_fadeControl = StaticVariables.g_fadeControl2;
        //StaticVariables.g_warpUsageTable = StaticVariables.DAT_801eb83e;
        StaticVariables.DAT_801eb82e = 1;
        StaticVariables.g_fadeControl2 = 1;
        StaticVariables.DAT_801eb832 = 0;
        StaticVariables.DAT_801eb830 = 0;
        StaticVariables.DAT_801eb834 = 0;
        StaticVariables.DAT_801eb83a = 0;
        StaticVariables.DAT_801eb83c = 0;

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
        StaticVariables.g_totalWarpEntries = 99;
        index = 0;

        while (i < 0x80)
        {
            StaticVariables.g_warpUsageTable[index] = 0;
            StaticVariables.g_warpUsageTable[index + 1] = 0;
            i++;
            index += 2;
        }
    }

    private int UpdateEntityFromWarpFlag(int param_1)
    {
        if (param_1 < 0x33)
        {
            if (param_1 < 0)
            {
                StaticVariables.g_fadeControl.WarpVisualId = 0;
            }
            else
            {
                StaticVariables.g_fadeControl.WarpVisualId = (short)param_1;
            }
        }
        else
        {
            StaticVariables.g_fadeControl.WarpVisualId = 0x32;
        }

        return StaticVariables.g_fadeControl.WarpVisualId;
    }

    private int FinalizeWarpEntities(short warpEntityId)
    {
        if (StaticVariables.g_fadeControl.WarpVisualId < warpEntityId)
        {
            StaticVariables.g_fadeControl.CurrentWarpEntityId = StaticVariables.g_fadeControl.WarpVisualId;
        }
        else if (warpEntityId < 0)
        {
            StaticVariables.g_fadeControl.CurrentWarpEntityId = 0;
        }
        else
        {
            StaticVariables.g_fadeControl.CurrentWarpEntityId = warpEntityId;
        }

        return StaticVariables.g_fadeControl.CurrentWarpEntityId;
    }

    private int SetMaxFadeLevel(short maxFadeLevel)
    {
        if (maxFadeLevel < 5)
        {
            if (maxFadeLevel < 0)
            {
                StaticVariables.g_fadeControl.MaxFadeLevel = 0;
            }
            else
            {
                StaticVariables.g_fadeControl.MaxFadeLevel = maxFadeLevel;
            }
        }
        else
        {
            StaticVariables.g_fadeControl.MaxFadeLevel = 4;
        }

        return StaticVariables.g_fadeControl.MaxFadeLevel;
    }

    private int SetFadeTargetLevel(short targetLevel)
    {
        if (StaticVariables.g_fadeControl.MaxFadeLevel < targetLevel)
        {
            StaticVariables.g_fadeControl.TargetFadeLevel = StaticVariables.g_fadeControl.MaxFadeLevel;
        }
        else if (targetLevel < 0)
        {
            StaticVariables.g_fadeControl.TargetFadeLevel = 0;
        }
        else
        {
            StaticVariables.g_fadeControl.TargetFadeLevel = targetLevel;
        }

        return StaticVariables.g_fadeControl.TargetFadeLevel;
    }

    private int ApplyFadeLevel(short newFadeValue)
    {
        if (newFadeValue < 10000)
        {
            if (newFadeValue < 0)
            {
                StaticVariables.g_fadeControl.CurrentWarpEntityId = 0;
            }
            else
            {
                StaticVariables.g_fadeControl.CurrentWarpEntityId = newFadeValue;
            }
        }
        else
        {
            StaticVariables.g_fadeControl.CurrentWarpEntityId = 9999;
        }

        return StaticVariables.g_fadeControl.CurrentWarpEntityId;
    }

    private void SetupPostWarpGraphics()
    {
        StaticVariables.INT_ARRAY_800a8284[0] = FUN_8004dc50();
        StaticVariables.INT_ARRAY_800a8284[1] = StaticVariables.INT_ARRAY_800a8284[0];
        StaticVariables.INT_ARRAY_800a8284[3] = GetCurrentPaletteFadeLevel();
        StaticVariables.INT_ARRAY_800a8284[2] = StaticVariables.INT_ARRAY_800a8284[3];
    }

    private int FUN_8004dc50()
    {
        return StaticVariables.g_fadeControl.WarpVisualId;
    }

    private int GetCurrentPaletteFadeLevel()
    {
        return StaticVariables.g_fadeControl.MaxFadeLevel;
    }

    private void LoadWarpVisuals(ushort warpVisualId)
    {
        if (warpVisualId == 0xffffffff || warpVisualId - 1 < 6)
        {
            StaticVariables.g_fadeControl.WarpVisualId = (short)warpVisualId;
        }
        else
        {
            //LogDebugMessage(StaticVariables.g_logMessage_InvalidWarpVisualId, WarpVisualId);
        }

        GetCurrentTileIndex();
    }

    private uint GetCurrentTileIndex()
    {
        uint tileIndex = 0xffffffff;
        int caseValue = StaticVariables.g_fadeControl.WarpVisualId - 1;

        switch (caseValue)
        {
            case 0:
                tileIndex = GetCurrentTile_Zone1();
                break;
            case 1:
                tileIndex = GetCurrentTile_Zone2();
                break;
            case 2:
                tileIndex = GetCurrentTile_Zone3();
                break;
            case 3:
                tileIndex = GetCurrentTile_Zone4();
                break;
            case 4:
                tileIndex = GetCurrentTile_Zone5();
                break;
            case 5:
                tileIndex = GetCurrentTile_Zone6();
                break;
        }

        return tileIndex;
    }

    private uint GetCurrentTile_Zone1()
    {
        return SelectTileMapSection(1);
    }

    private uint GetCurrentTile_Zone2()
    {
        return SelectTileMapSection(2);
    }

    private uint GetCurrentTile_Zone3()
    {
        return SelectTileMapSection(3);
    }

    private uint GetCurrentTile_Zone4()
    {
        return SelectTileMapSection(4);
    }

    private uint GetCurrentTile_Zone5()
    {
        return SelectTileMapSection(5);
    }

    private uint GetCurrentTile_Zone6()
    {
        return SelectTileMapSection(6);
    }

    private uint SelectTileMapSection(uint sectionId)
    {
        int bestMatchIndex = -1;

        //if (sectionId < 0x20)
        //{
        //    int currentIndex = 0;
        //    int sectionTableIndex = 0;
        //    int bestPriorityIndex = 0;
        //
        //    do
        //    {
        //        if (StaticVariables.g_tileMapWarpSections[sectionTableIndex] == sectionId && StaticVariables.g_warpUsageTable[bestPriorityIndex + 1] > 0)
        //        {
        //            if (bestMatchIndex == -1)
        //            {
        //                bestMatchIndex = currentIndex;
        //                if ((StaticVariables.g_tileMapWarpSections[sectionTableIndex + 1] & 1U) == 0)
        //                {
        //                    return (uint)currentIndex;
        //                }
        //            }
        //            else if (StaticVariables.g_tileMapWarpSections[bestMatchIndex * 5] < StaticVariables.g_tileMapWarpSections[sectionTableIndex + 2])
        //            {
        //                bestMatchIndex = currentIndex;
        //            }
        //        }
        //        bestPriorityIndex += 2;
        //        currentIndex++;
        //        sectionTableIndex += 5;
        //    } while (currentIndex < 0x80);
        //}
        //else
        //{
        //    //LogDebugMessage(StaticVariables.g_debugMessage_SelectTileMapSection, sectionId);
        //    bestMatchIndex = -1;
        //}

        return (uint)bestMatchIndex;
    }

    private void InitDebugVars()
    {
        StaticVariables.g_debugFlags = 0;
        StaticVariables.g_debugState = 0;
        StaticVariables.g_debugVar_NbFrameBreak = 0x10;
        StaticVariables.g_debugVar_WarpDestinationId = StaticVariables.g_desiredMap;
    }

    private void FUN_80042984()
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
        InitCameraTransitionState();
    }

    private void InitCameraTransitionState()
    {
        StaticVariables.g_cameraTransitionState = 0;
        StaticVariables.g_cameraTransitionStartX = 8;
        StaticVariables.g_cameraTransitionStartY = 0x78;
    }

    private void InitTileRenderer(int tPageX, int tPageY, int paletteX, int paletteY, short drawMode,
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
        SetTileAnimationMode(3, 0);
    }

    private void SetTileAnimationMode(int animationMode, int animationBankIndex)
    {
        StaticVariables.g_tileAnimationMode = animationMode;
        StaticVariables.g_tileAnimationType = animationBankIndex;
        StaticVariables.g_animationFrameCounter = 1;
        StaticVariables.g_tileOffset = 0;

        if (0 < animationBankIndex)
        {
            //StaticVariables.g_animationData = StaticVariables.g_tile_set + (animationBankIndex + -1) * 0x10 + StaticVariables.g_tileSetMetaData->tileAnimationOffset;
        }
    }

    private void InitStaticVariable()
    {
        StaticVariables.g_mapLimits = 0x3c;
        StaticVariables.g_debugFrameDelay = 0;
        StaticVariables.g_debugFlags = StaticVariables.g_debugFlags & 0xf7ffff3f;
    }

    private void InitializeMapSpriteTable(byte[] buffer, ushort[] vramTable, int otherPtr)
    {
        //TODO

        //int spriteIndex = 0;
        //
        //StaticVariables.g_spriteVRAMPointer = vramTable;
        //StaticVariables.g_spriteOtherPointer = otherPtr;
        //StaticVariables.g_spriteDataBase = buffer;
        //
        //do
        //{
        //    int spriteDataOffset = spriteIndex * 2;
        //
        //    if (StaticVariables.g_spriteDataBase[spriteDataOffset + 0x420] == 0 ||
        //        StaticVariables.g_spriteDataBase[spriteDataOffset + 0x421] == 0)
        //    {
        //        StaticVariables.g_spriteMapTable[spriteIndex].Enabled = false;
        //        StaticVariables.g_spriteMapTable[spriteIndex].OffsetX = 0;
        //    }
        //    else
        //    {
        //        StaticVariables.g_spriteMapTable[spriteIndex].Enabled = true;
        //        StaticVariables.g_spriteMapTable[spriteIndex].VramShift = (byte)(1 << (StaticVariables.g_spriteDataBase[spriteDataOffset + 0x420] & 0x1f));
        //
        //        if (StaticVariables.g_spriteMapTable[spriteIndex].VramShift == 0)
        //        {
        //            System.Diagnostics.Debugger.Break();
        //            //Trap(0x1c00);
        //        }
        //
        //        StaticVariables.g_spriteMapTable[spriteIndex].TileWidth = (char)(0xa0 / StaticVariables.g_spriteMapTable[spriteIndex].VramShift);
        //        byte vramY = StaticVariables.g_spriteDataBase[spriteDataOffset + 0x421];
        //
        //        StaticVariables.g_spriteMapTable[spriteIndex].OffsetZ = 0;
        //        StaticVariables.g_spriteMapTable[spriteIndex].OffsetY = 0;
        //        StaticVariables.g_spriteMapTable[spriteIndex].OffsetX = 0;
        //        StaticVariables.g_spriteMapTable[spriteIndex].RowCount = vramY;
        //    }
        //
        //    spriteIndex++;
        //} while (spriteIndex < 6);
    }

    private void LoadMapSpriteTable(AnimationData animationData)
    {
        //InitAnimationData(StaticVariables.g_animationStructs2, animationData);
    }

    private void InitAnimationData(AnimationTable table, AnimationData data)
    {
        //int pointerListCount = 0;
        //
        //table.baseDataPtr = data;
        //table.rawPointerList = data.entriesOffset + data.rawPtrListOffset;
        //int[] pointerList = data.entriesOffset + data.pointerListOffset;
        //table.pointerList = pointerList;
        //
        //while (pointerListCount < 0x100)
        //{
        //    int offset = pointerList[pointerListCount];
        //    if (offset == 0)
        //        break;
        //
        //    if (offset != -1)
        //    {
        //        pointerList[pointerListCount] = data.entriesOffset + offset;
        //        int entryPtr = pointerList[pointerListCount];
        //        entryPtr = data.entriesOffset + entryPtr;
        //        pointerList[pointerListCount + 1] = data.entriesOffset + pointerList[pointerListCount + 1];
        //        pointerList[pointerListCount + 2] = data.entriesOffset + pointerList[pointerListCount + 2];
        //        pointerList[pointerListCount + 3] = data.entriesOffset + pointerList[pointerListCount + 3];
        //    }
        //
        //    pointerListCount++;
        //}
        //
        //table.pointerListCount = pointerListCount;
        //
        //int[] entriesPtr = data.entriesOffset + data.entries;
        //if (data.entries == 0)
        //{
        //    table.field2_0x8 = 0;
        //    table.field5_0x14 = 0;
        //}
        //else
        //{
        //    table.field2_0x8 = entriesPtr;
        //    pointerListCount = 0;
        //    while (pointerListCount < 0x80)
        //    {
        //        if (entriesPtr[pointerListCount * 5] == 0)
        //            break;
        //
        //        pointerListCount++;
        //    }
        //    table.field5_0x14 = pointerListCount;
        //}
        //
        //pointerListCount = 0;
        //int[] flagsPtr = data.entriesOffset + data.flags;
        //table.field7_0x1c = flagsPtr;
        //while (pointerListCount < 0x100)
        //{
        //    int offset = flagsPtr[pointerListCount];
        //    if (offset == 0)
        //        break;
        //
        //    if (offset != -1)
        //    {
        //        flagsPtr[pointerListCount] = data.entriesOffset + offset;
        //    }
        //
        //    pointerListCount++;
        //}
        //table.field8_0x20 = pointerListCount;
        //
        //int[] frameListPtr = data.entriesOffset + data.frameListOffset;
        //if (data.frameListOffset == 0)
        //{
        //    table.frameList = 0;
        //    table.frameCount = 0;
        //}
        //else
        //{
        //    table.frameList = frameListPtr;
        //    pointerListCount = 0;
        //    while (pointerListCount < 0x80)
        //    {
        //        if (frameListPtr[pointerListCount * 3] == 0)
        //            break;
        //
        //        pointerListCount++;
        //    }
        //    table.frameCount = pointerListCount;
        //}
        //
        //int[] scriptListPtr = data.entriesOffset + data.entryIndex;
        //if (data.entryIndex == 0)
        //{
        //    table.scriptList = 0;
        //    table.scriptCount = 0;
        //}
        //else
        //{
        //    table.scriptList = scriptListPtr;
        //    pointerListCount = 0;
        //    while (pointerListCount < 0x80)
        //    {
        //        if (scriptListPtr[pointerListCount * 2] == 0)
        //            break;
        //
        //        pointerListCount++;
        //    }
        //    table.scriptCount = pointerListCount;
        //}
        //
        //table.field12_0x30 = data.entriesOffset + data.offsetX;
        //AnimationData pAVar4 = data;
        //
        //pointerListCount = 0;
        //while (pointerListCount < 6)
        //{
        //    byte[] pbVar1 = pAVar4.offsetX;
        //    pAVar4 = pAVar4.frameListOffset;
        //    pointerListCount++;
        //
        //    table.field13_0x34 = data.entriesOffset + BitConverter.ToInt32(pbVar1, 0);
        //    table = table.rawPointerList;
        //}
    }

    private void FUN_800423ec(int param_1)
    {
        StaticVariables.g_etcAnimTableAlt = param_1;
    }

    private void ClearGlobalFlags()
    {
        int iVar2 = 0x3f;
        int piVar1 = 0x3f;

        do
        {
            StaticVariables.g_globalFlags[piVar1] = 0;
            iVar2 = iVar2 - 1;
            piVar1 = piVar1 - 1;
        } while (iVar2 >= 0);
    }

    private void ResetCameraAndLoadVRAMAssets()
    {
        StaticVariables.g_bossCutsceneFlag = 0;
        StaticVariables.g_renderTileRowCount = 0x3c;
        StaticVariables.g_isCameraScrolling = 1;
        StaticVariables.g_cameraDebugOffsetY = 0;
        StaticVariables.g_cameraDebugOffsetX = 0;
        //LoadVRAMAssets();
    }

    private void FUN_80044520(int param_1)
    {
        int iVar2 = 2;
        int piVar1 = 2;

        StaticVariables.g_itemIdThreshold = param_1;

        do
        {
            StaticVariables.g_items[piVar1 + 2] = 0;
            iVar2 = iVar2 - 1;
            piVar1 = piVar1 - 2;
        } while (iVar2 >= 0);
    }

    private void LoadMapAndInitializeEntities(uint[] bufferImage)
    {
        InitializeMapEvents();

        LoadMap(StaticVariables.g_currentMap); // Added

        //Added by hand
        LoadAlundra();

        //LoadImageArea(StaticVariables.g_bufferImage, 0x40, 0x1e0, 0x40);
        //LoadCompressedImageToBuffer(bufferImage, 0x140, 0, 5, StaticVariables.g_bufferImage2);
        InitializeEntitySlots();
        //InitializeMapEvents();
        InitEffectSlots();
    }

    private void InitializeMapEvents()
    {
        int programBMapCode;
        int i = 0;
        MapEvent mapEventDest;
        MapEvent emptyMapEvent;
        SiMapEventRecord mapEventRecord;
        Entity entity;
        byte monitorEnabledFlag;

        MapEvent[] mapEvents = StaticVariables.g_mapEvents;
        //MapEvent pEmptyMapEvent = StaticVariables.g_emptyMapEvent;
        //MapEvent pMapEvents = mapEvents[0];

        do
        {
            mapEvents[i].Id = i;
            mapEvents[i].ProgramBMap = 0;
            mapEvents[i].MapEventRecord = null;
            mapEvents[i].Entity = null;
            mapEvents[i].EventData.Sp = 0;
            mapEvents[i].EventData.Exp = 0;
            mapEvents[i].EventData.Tick = 0;
            for (int j = 0; j < mapEvents[i].EventData.Variables.Length; j++)
            {
                mapEvents[i].EventData.Variables[j] = 0;
            }
            mapEvents[i].EventData.LogicResult = 0;
            mapEvents[i].EventData.ElapsedMs = 0;
            mapEvents[i].EventData.IsWaiting = 0;
            for (int j = 0; j < mapEvents[i].EventData.Code.Length; j++)
            {
                mapEvents[i].EventData.Code[j] = 0;
            }


            i++;
            //do
            //{
            //    mapEventDest = pMapEvents;
            //    emptyMapEvent = pEmptyMapEvent;
            //
            //    mapEventRecord = emptyMapEvent.MapEventRecord;
            //    programBMapCode = emptyMapEvent.ProgramBMap;
            //    entity = emptyMapEvent.Entity;
            //
            //    mapEventDest.Id = emptyMapEvent.Id;
            //    mapEventDest.MapEventRecord = mapEventRecord;
            //    mapEventDest.ProgramBMap = programBMapCode;
            //    mapEventDest.Entity = entity;
            //
            //    pEmptyMapEvent = emptyMapEvent.EventData;
            //    pMapEvents = mapEventDest.EventData;
            //
            //} while (pEmptyMapEvent != StaticVariables.g_emptyMapEvent.EventData.Codes[1]);
            //
            //programBMapCode = pEmptyMapEvent.EventData.Exp;
            //mapEventDest.EventData.Sp = StaticVariables.g_emptyMapEvent.EventData.Codes[1];
            //mapEventDest.EventData.Exp = programBMapCode;
            //
            //mapEvents[i].Id = i;
            //i++;
            //mapEvents[i - 1].ProgramBMap = 0;
            //
            //if (i < 0x40)
            //{
            //    pEmptyMapEvent = StaticVariables.g_emptyMapEvent;
            //    pMapEvents = mapEvents[i];
            //}
        } while (i < 0x40);

        i = 0;
        int index = 0;

        //byte[] MonitorFlags = StaticVariables.g_initMapEventRecords[4] + 1; //.Skip(4).ToArray();
        entity = StaticVariables.g_mapEvents[0].Entity;

        do
        {
            mapEventRecord = StaticVariables.g_initMapEventRecords[4 + i];

            if (mapEventRecord == null)
            {
                return;
            }
            
            programBMapCode = mapEventRecord.EventCodesBIndex;

            if (programBMapCode == 0)
            {
                if (StaticVariables.g_debugState < 0 && (StaticVariables.g_debugFlags & 0x20) != 0)
                {
                    //PrintInfo();
                }
            }
            else
            {
                //entity.MapEventProgramId = mapEventRecord.EventCodesBIndex;
                //entity.AIValues[6] = mapEventRecord;
                //entity.AIValues[8] = (short)programBMapCode;

                //StaticVariables.g_mapEvents[index].Entity = entity;
                //StaticVariables.g_mapEvents[index].MapEventRecord = StaticVariables.g_initMapEventRecords[4].
                //StaticVariables.g_mapEvents[index].ProgramBMap = programBMapCode;
                //StaticVariables.g_mapEvents[index].MapEntity2 = StaticVariables.g_entitySlots;
                //StaticVariables.g_mapEvents[index].MonitorFlag = MonitorFlags[0];
            }

            i++;
            //MonitorFlags = MonitorFlags + 8; //.Skip(8).ToArray();
            entity = entity.ChildEntity;
        } while (i < 0x80);
    } 

    private void InitEffectSlots()
    {
        SpriteEffect effect;
        int val;
        int effectIndex;
        int[] effectInitTable;

        effect = StaticVariables.g_effectSlots[0];
        effectIndex = 0x7f;

        do
        {
            StaticVariables.g_effectSlots[effectIndex].Status = 0;
            effectIndex--;
        } while (effectIndex >= 0);

        //effectIndex = 0;
        //val = StaticVariables.g_initMapEventRecords[0];
        //effectInitTable = StaticVariables.g_initMapEventRecords;
        //
        //while (val != 0)
        //{
        //    effectSlotPtr = SpawnSpriteEffect(effectIndex, 0);
        //    if (effectSlotPtr == null && (StaticVariables.g_debugState & 0x80000000U) != 0 &&
        //        (StaticVariables.g_debugFlags & 0x20) != 0)
        //    {
        //        //PrintInfo();
        //    }
        //    effectInitTable = effectInitTable + 3; //.Skip(3).ToArray();
        //    effectIndex = effectIndex + 1;
        //    val = effectInitTable[0];
        //}

        effectIndex = 0;

        foreach (var mapEventRecord in StaticVariables.g_initMapEventRecords)
        {
            effect = SpawnSpriteEffect(effectIndex, 0);

            if (effect == null 
                && (StaticVariables.g_debugState & 0x80000000U) != 0 
                && (StaticVariables.g_debugFlags & 0x20) != 0)
            {
                //PrintInfo();
            }

            effectIndex++;
        }
    }

    private SpriteEffect SpawnSpriteEffect(int effectId, int checkSpawnArea)
    {
        MapEffectRecord effectStatus;
        SpriteEffect effect;
        byte bVar1;

        effectStatus = GetMapEffectRecord(effectId, checkSpawnArea == 1);
        effect = null;

        if (effectStatus != null)
        {
            bVar1 = effectStatus.Flags;
            if (checkSpawnArea != 0 || (bVar1 & 0x40) != 0)
            {
                effect = GetFreeEffect();
                if (effect == null)
                {
                    effect = null;
                }
                else
                {
                    InitEffectEntity(
                        effect,
                        effectStatus,
                        effectId,
                        0,
                        bVar1 & 0x80,
                        effectStatus.EffectId,
                        effectStatus.AnimId,
                        (int)(((uint)effectStatus.X * 12 + 12) * 0x10000),
                        (int)(((uint)effectStatus.Y * 8 + 8) * 0x10000),
                        (int)((uint)effectStatus.Z << 0x13)
                    );
                }
            }
        }

        return effect;
    }

    public void LoadMap(int mapId)
    {
        CurrentMap = _datasBin.GameMaps[mapId];

        if (!CurrentMap.Loaded)
        {
            using var reader = _datasBin.OpenBin();
            CurrentMap.Load(reader, true);
        }

        LoadMap(CurrentMap);
    }

    public void LoadMap(GameMap map)
    {
        for (var i = 0; i < map.SpriteInfo.MapEvents.Records.Length; i++)
        {
            var record = map.SpriteInfo.MapEvents.Records[i];
            if (record != null)
            {
                var mapEvent = StaticVariables.g_mapEvents[i];
                mapEvent.Id = i;
                mapEvent.MapEventRecord = record;
                mapEvent.ProgramBMap = record.EventCodesBIndex;
                //TODO special logic if the eventcodesindex is 0
                mapEvent.Entity = StaticVariables.PlayerEntity;
                mapEvent.EventData = new EventProgramState();
            }
        }

        //LoadEntities();
    }

    private void LoadAlundra()
    {
        using var reader = _datasBin.OpenBin();
        AlundraMap.Load(reader, false);

        int paletteIndex;
        int sheetSize;
        var spriteRecord = GetSpriteFromSpriteTable(false, 0, out paletteIndex, out sheetSize);

        //if (spriteRecord == null)
        //{
        //    return;
        //}

        StaticVariables.PlayerEntity.Index = 1;

        InitializeEntity(StaticVariables.PlayerEntity, null, spriteRecord,
            null, 0, 0,
            50, 50, 0,
            0,
            0, //dir g_warpZones[flags & 3]
            paletteIndex,
            sheetSize);
    }

    private void InitializeEntitySlots()
    {
        Entity entityCounter = null;
        Entity writePtr = StaticVariables.PlayerEntity;
        Entity readPtr = StaticVariables.g_emptyEntityForClearing;
        Entity blockStart = writePtr;

        //do
        //{
        //    do
        //    {
        //        Entity ent1 = readPtr.NextEntity;
        //        Entity ent2 = readPtr.ChildEntity;
        //        Entity ent3 = readPtr.ParentEntity;
        //
        //        writePtr.PreviousEntity = readPtr.PreviousEntity;
        //        writePtr.NextEntity = ent1;
        //        writePtr.ChildEntity = ent2;
        //        writePtr.ParentEntity = ent3;
        //
        //        readPtr = readPtr.Status;
        //        writePtr = writePtr.Status;
        //    }
        //    while (readPtr != StaticVariables.g_emptyEntityForClearing.SpawnedGameFlag[4]);
        //
        //    writePtr = (Entity)StaticVariables.g_emptyEntityForClearing.SpawnedGameFlag[4];
        //
        //    blockStart.PreviousEntity = entityCounter;
        //    entityCounter = (Entity)((int)entityCounter.PreviousEntity + 1);
        //
        //    writePtr = blockStart + 1;
        //    readPtr = StaticVariables.g_emptyEntityForClearing;
        //    blockStart = writePtr;
        //}
        //while ((int)entityCounter < 0x40);

        StaticVariables.g_numberOfEntity = 1; //alundra already loaded

        ResetEntityState();

        //int characterIndex = 0;
        //EntityRecord[] initTableEntry = StaticVariables.g_initTableEntry;
        //
        //do
        //{
        //    EntityRecord entityRecord = initTableEntry[characterIndex];
        //    if (entityRecord == null)
        //    {
        //        break;
        //    }
        //
        //    Entity spawnedEntity = SpawnEntity(characterIndex, 0);
        //    if (spawnedEntity == null && StaticVariables.g_debugState < 0 && (StaticVariables.g_debugFlags & 0x20) != 0)
        //    {
        //        //PrintInfo();
        //    }
        //
        //    characterIndex++;
        //}
        //while (characterIndex < 0x80);

        for (int i = 1; i < StaticVariables.g_entitySlots.Length; i++)
        {
            var entity = SpawnEntity(null, i, 0);

            if (entity == null)
            {
                continue;
            }
        }

        StaticVariables.g_entityFollowedByCamera = StaticVariables.PlayerEntity;
    }

    private void ResetEntityState()
    {
        int tileIndex;

        //InitializeEntity(PlayerEntity, null, 
        //    /*StaticVariables.g_initialAnimationTable->entries*/null, 
        //    null, 0,
        //    -1, 
        //    StaticVariables.g_cameraTargetX, StaticVariables.g_cameraTargetY, 
        //    StaticVariables.g_animation_id, StaticVariables.g_warpTriggerType, StaticVariables.g_warpExtraParam,
        //    0xb, 0x60);
        StaticVariables.g_playerInitState = 2;
        //StaticVariables.g_warpTarget = FUN_8004dc50();
        //StaticVariables.g_warpAnimEntity = GetFadeControl();
        StaticVariables.g_gravityFlag = 0;
        StaticVariables.g_playerWarpTimer = 0;
        StaticVariables.g_isWarpDisabled = 0;
        StaticVariables.g_frameTimer = 0;
        //tileIndex = GetCurrentTileIndex();
        //StaticVariables.g_currentTileFlags = StaticVariables.g_tileAttributeLUT[tileIndex];
        StaticVariables.g_playerEffectTransitionCooldown = 0;
        //ResetWarpLockTimer();
    }

    public Entity SpawnEntity(Entity ownerEntity, bool isMapSprite, uint tableIndex, int xpos, int ypos, int zpos, uint dir)
    {
        int paletteIndex, sheetSize;

        var spriteRecord = GetSpriteFromSpriteTable(isMapSprite, tableIndex, out paletteIndex, out sheetSize);

        if (spriteRecord == null)
        {
            return null;
        }

        var entity = AllocateEntitySlot();
        if (entity == null)
        {
            return null;
        }

        var spriteTableIndex = tableIndex;
        if (isMapSprite)
        {
            spriteTableIndex += 0x100;
        }

        InitializeEntity(entity, ownerEntity, spriteRecord, null, spriteTableIndex, -1, xpos, ypos, zpos, 0, dir, paletteIndex, sheetSize);

        return entity;
    }

    private Entity SpawnEntity(Entity parent, int entityId, int notCheckSpawnZone)
    {
        var entityRecord = CurrentMap.SpriteInfo.Entities.Entities[entityId];

        if (entityRecord == null)
        {
            return null;
        }

        if (notCheckSpawnZone == 0)
        {
            if (StaticVariables.PlayerEntity.TileX < entityRecord.XMin)
            {
                return null;
            }

            if (entityRecord.XMax < StaticVariables.PlayerEntity.TileX)
            {
                return null;
            }

            if (StaticVariables.PlayerEntity.TileY < entityRecord.YMin)
            {
                return null;
            }

            if (entityRecord.YMax < StaticVariables.PlayerEntity.TileY)
            {
                return null;
            }
        }

        int paletteIndex, sheetSize;

        if (((uint)entityRecord.SpriteDirection & 0x40) == 0 && notCheckSpawnZone == 0)
        {
            return null;
        }

        var isMapSprite = (entityRecord.SpriteDirection & 0x80) != 0;
        var spriteRecord = GetSpriteFromSpriteTable(isMapSprite, entityRecord.SpriteTableIndex, out paletteIndex, out sheetSize);

        if (spriteRecord == null)
        {
            return null;
        }

        var entity = AllocateEntitySlot();
        if (entity == null)
        {
            return null;
        }

        int spriteTableIndex = entityRecord.SpriteTableIndex;
        if (isMapSprite)
        {
            spriteTableIndex += 0x100;
        }

        InitializeEntity(entity, parent, spriteRecord,
            entityRecord, (uint)spriteTableIndex, entityId,
            entityRecord.XPos, //(x * 12 + 12) * 65536
            entityRecord.YPos, //(y * 8 + 8) * 65536
            entityRecord.Height, //h << 19
            0,
            0, //dir g_warpZones[flags & 3]
            paletteIndex,
            sheetSize);

        return entity;
    }

    private SpriteRecord GetSpriteFromSpriteTable(bool isMapSprite, uint spriteTableIndex, out int addedtosheet, out int addedtopallette)
    {
        SpriteInfo si;

        if (isMapSprite)
        {
            si = CurrentMap.SpriteInfo;
            addedtosheet = 0;
            addedtopallette = 0x20;
        }
        else
        {
            si = _datasBin.AlundraGameMap.SpriteInfo;
            addedtosheet = 0xb;
            addedtopallette = 0x60;
        }
        if (spriteTableIndex < 0)
        {
            throw new Exception("Illegal Character Race!");
        }

        if (spriteTableIndex >= si.SpriteTable.Length)
        {
            throw new Exception("Illegal Character Race!");
        }

        var sprite = si.Sprites[spriteTableIndex];
        return sprite;
    }

    private Entity AllocateEntitySlot()
    {
        int i = 1;
        int index = 0;
        Entity entity = StaticVariables.g_entitySlots[index];

        do
        {
            i = i + 1;
            if (entity.Status == 0)
            {
                return entity;
            }
            index++;
            entity = StaticVariables.g_entitySlots[index];
        } while (i < 0x40);

        //DoNothing();
        return null;
    }

    private void InitializeEntity(Entity entity, Entity parentEntity,
        SpriteRecord sprite, SiEntityRecord entityRecord,
        uint spriteTableIndex,
        int entityId, int x, int y, int z,
        uint animationId, uint direction, int paletteIndex, int sheetSize)
    {
        if (StaticVariables.g_numberOfEntity < entity.Index)
        {
            StaticVariables.g_numberOfEntity = entity.Index;
        }

        entity.ParentEntity = parentEntity;

        if (parentEntity != null)
        {
            Entity linkedEntity = parentEntity.ChildEntity;
            if (parentEntity.ChildEntity == null)
            {
                linkedEntity = parentEntity;
            }
            entity.ChildEntity = linkedEntity;
        }

        entity.Sprite = sprite;
        entity.ProgramIndexes[1] = entityId; //entityRecord;
        entity.SpriteTableIndex = spriteTableIndex;

        if (entityRecord != null)
        {
            entity.EntityRefId = entityId;
        }
        else
        {
            entity.EntityRefId = -1;
        }

        entity.Status = 1;
        entity.Index2++;
        //entity.Index = StaticVariables.g_nextEntityIndex++;

        entity.CurrentAnimationId = ~animationId;
        entity.CurrentDirection = ~direction;
        entity.TargetAnimationId = animationId;
        entity.TargetDirection = direction;
        //uint flags = animData.Flags;
        entity.Flags = (uint)(sprite.Header.MoreFlags | sprite.Header.CanPickup << 8 | sprite.Header.FlagsPortraitShadowtype << 16); ;

        entity.SpriteProgramIndexes[0] = sprite.Header.ProgramLoad;
        entity.SpriteProgramIndexes[1] = 0;
        entity.SpriteProgramIndexes[2] = sprite.Header.ProgramTick;
        entity.SpriteProgramIndexes[3] = sprite.Header.ProgramTouch;
        entity.SpriteProgramIndexes[4] = sprite.Header.ProgramDeactivate;
        entity.SpriteProgramIndexes[5] = sprite.Header.ProgramInteract;

        entity.AddedToPalette = paletteIndex;
        entity.AddedToSheet = sheetSize;

        //BalanceRecord balanceRecord = GetSpriteAnimationPtr(entity.SpriteTableIndex);
        BalanceRecord balanceRecord = BalanceBin.GetBalanceRecordFromSpriteIndex((int)spriteTableIndex, CurrentMap.Info.BalanceLevel);
        entity.BalanceRecord = balanceRecord;
        byte balanceHp = balanceRecord.Hp;
        entity.HpMax = balanceHp;
        entity.Hp = balanceHp;

        InitCodePrograms(entity);

        SetEntityDimensions(entity,
            sprite.Header.Xmod, sprite.Header.Ymod, sprite.Header.Zmod,
            sprite.Header.Width, sprite.Header.Depth, sprite.Header.Height);

        entity.XPos = x;
        entity.YPos = y;
        entity.ZPos = z - entity.ZMod + 1;

        UpdateAnimation(entity);
        Debug.Assert(entity.AnimSet != null);

        entity.ModdedXPos = entity.XPos + entity.XMod;
        entity.ModdedYPos = entity.YPos + entity.YMod;
        entity.ModdedZPos = entity.ZPos + entity.ZMod;

        int height = ComputeEntityGroundHeight(entity);
        entity.FloorHeight = height;

        if (entity.ZPos <= height + 1)
        {
            entity.ZPos = height + 1;
            entity.ModdedXPos = entity.XPos + entity.XMod;
            entity.ModdedYPos = entity.YPos + entity.YMod;
            entity.ModdedZPos = entity.ZPos + entity.ZMod;
        }

        UpdateEntityCollisionData(entity);
        InitializeContents(entity);
    }

    private void InitCodePrograms(Entity entity)
    {
        entity.LogicContextEntity = entity;

        if (entity.EntityRecord != null)
        {
            entity.ProgramIndexes[ScriptHelper.ProgramALoad] = entity.EntityRecord.EventCodesA_LoadIndex;
            entity.ProgramIndexes[ScriptHelper.ProgramBMap] = entity.EntityRecord.EventCodesB_MapIndex;
            entity.ProgramIndexes[ScriptHelper.ProgramCTick] = entity.EntityRecord.EventCodesC_TickIndex;
            entity.ProgramIndexes[ScriptHelper.ProgramDTouch] = entity.EntityRecord.EventCodesD_TouchIndex;
            entity.ProgramIndexes[ScriptHelper.ProgramEDeactivate] = entity.EntityRecord.EventCodesE_DeactivateIndex;
            entity.ProgramIndexes[ScriptHelper.ProgramFInteract] = entity.EntityRecord.EventCodesF_InteractIndex;
        }
    }

    private void SetEntityDimensions(Entity entity, int offsetX, int offsetY, int offsetZ, int sizeX, int sizeY, int sizeZ)
    {
        entity.NegXMod = offsetX * -0x10000;
        entity.NegYMod = offsetY * -0x10000;
        entity.ModdedXPos = offsetX * 0x10000;
        entity.ModdedYPos = offsetY * 0x10000;
        entity.ModdedZPos = offsetZ << 16;

        entity.ScreenClipX = (offsetX + sizeX) * -0x10000 + 0x4e00000;
        entity.ScreenClipY = (offsetY + sizeY) * -0x10000 + 0x3c00000;
        entity.ScreenClipZ = (offsetZ + sizeZ) * -0x10000 + 0x7800000;

        if (sizeX == 0)
        {
            entity.Width = 0;
        }
        else
        {
            entity.Width = sizeX * 0x10000 - 1;
        }

        if (sizeY == 0)
        {
            entity.Height = 0;
        }
        else
        {
            entity.Height = sizeY * 0x10000 - 1;
        }

        if (sizeZ == 0)
        {
            entity.Depth = 0;
        }
        else
        {
            entity.Depth = sizeZ * 0x10000 - 1;
        }
    }

    private void UpdateAnimation(Entity entity)
    {
        SiFrame currentFrame = null;
        bool noSkip = true;

        var frameDelay = entity.TargetAnimationId;
        entity.IsZForceApplied = 0;
        var animationFrameIndex = StaticVariables.g_frameIndexTable[(entity.TargetDirection + 2 & 0x1c) + entity.CurrentFrameIndex * 0x20];

        if (frameDelay != entity.CurrentAnimationId)
        {
            noSkip = false;
        }

        if (animationFrameIndex != entity.CurrentAnimationId)
        {
            noSkip = false;
        }

        if (noSkip)
        {
            animationFrameIndex = entity.NextFrameDelay - 1;
            entity.NextFrameDelay = animationFrameIndex;

            if (animationFrameIndex != 0)
            {
                return;
            }

            currentFrame = entity.Frame;
        }

        while (true)
        {
            while (noSkip)
            {
                frameDelay = currentFrame.Delay;

                if (((uint)currentFrame.Delay & 0x80) != 0)
                {
                    entity.NextFrameDelay = (int)(frameDelay & 0x7f);
                    //entity.Frame = entity.AnimSet
                    //    .PreloadedAnims[entity.TargetAnimationId]
                    //    .Frames[entity.CurrentFrameIndex + 1];


                    System.Diagnostics.Debug.Assert(entity.Frame != null);

                    if (currentFrame.CollisionOffset != -1)
                    {
                        entity.FrameCollision = entity.Frame.CollisionData;
                        entity.FrameXOff = entity.FrameCollision.XOff << 16;
                        entity.FrameYOff = entity.FrameCollision.YOff << 16;
                        entity.FrameZOff = entity.FrameCollision.ZOff << 16;
                        entity.Width = (entity.FrameCollision.Width << 16) - 1;
                        entity.Depth = (entity.FrameCollision.Depth << 16) - 1;
                        entity.Height = (entity.FrameCollision.Height << 16) - 1;
                    }
                    else
                    {
                        entity.FrameCollision = null;
                    }

                    if (currentFrame.ImageSetPointer != -1)
                    {
                        entity.SpriteRef.Images = currentFrame.Images.Images;
                        entity.SpriteRef.DepthSortVal = currentFrame.Images.Unknown;
                        entity.SpriteRef.NumImages = currentFrame.Images.NumberOfImages;
                    }
                    else
                    {
                        entity.SpriteRef.Images = null;
                        entity.SpriteRef.DepthSortVal = 0;
                        entity.SpriteRef.NumImages = 0;
                    }

                    return;
                }

                if (frameDelay == 0)
                {
                    break;
                }

                if (frameDelay != 1)
                {

                    throw new Exception("Character Animation Error!!");
                }

                currentFrame = entity.FirstFrame;
                entity.AnimCompleteCounter++;
                entity.Frame = currentFrame;
            }

            if (noSkip)
            {
                frameDelay = (uint)(currentFrame.CollisionOffset & 0x80);
                //frameDelay = (uint)currentFrame->transformIndexLow;

                if (frameDelay != 0)
                {
                    break;
                }

                animationFrameIndex = entity.CurrentFrameIndex;
                entity.TargetAnimationId = frameDelay;
                entity.AnimCompleteCounter++;
            }

            LOAD_ANIMATION:
            var animSet = entity.Sprite.AnimSets[frameDelay]; //frameDelay * 0xe
            entity.AnimSet = animSet;
            System.Diagnostics.Debug.Assert(entity.AnimSet != null);
            //frameOffset = (ushort)((int)animSet.entries + animationFrameIndex * 2);
            var animTableOffset = entity.AnimSet.AnimationOffsets[entity.CurrentFrameIndex];
            entity.CurrentFrameIndex = animationFrameIndex;
            entity.NextFrameDelay = 0;
            entity.CurrentAnimationId = frameDelay;

            //currentFrame = entity.AnimSet.PreloadedAnims[entity.TargetAnimationId].Frames[frameIndex];
            //currentFrame = animTableOffset[entity.CurrentFrameIndex];
            //System.Diagnostics.Debug.Assert(animTableOffset < entity.AnimSet.PreloadedAnims.Length);
            //System.Diagnostics.Debug.Assert(entity.CurrentFrameIndex < entity.AnimSet.PreloadedAnims[animTableOffset].Frames.Length);

            //currentFrame = entity.AnimSet.PreloadedAnims[animTableOffset].Frames[entity.CurrentFrameIndex];
            currentFrame = entity.AnimSet.PreloadedAnims[entity.TargetAnimationId].Frames[entity.CurrentFrameIndex];
            entity.Frame = currentFrame;
            entity.FirstFrame = currentFrame;
            //entity.IsZForceApplied = animSet.isZForceApplied;
            entity.ForceResetAnimationFlag = 0;
            //entity.AnimFlags = (byte)animRecordPtr.AnimationOffsets[1];
            entity.AnimFlags = entity.AnimSet.Flags;

            if (entity.BalanceRecord.NumAnimVals == 0)
            {
                entity.BalanceVal = null;
            }
            else if (frameDelay + 1 < entity.BalanceRecord.NumAnimVals)
            {
                //entity.BalanceRecord.Vals ??
                entity.BalanceVal = entity.BalanceRecord.AnimVals[frameDelay * 2 + 0xe];
                //System.Diagnostics.Debugger.Break();
            }
            else
            {
                entity.BalanceVal = entity.BalanceRecord.AnimVals[0]; // 0 ?? TODO: check if index 0
            }

            //var sfxId = (uint)(byte)&animSet.pointerListOffset;
            //
            //if (((uint)animSet.pointerListOffset & 0x2000) != 0)
            //{
            //    sfxId += 0x100;
            //}
            //
            //PlaySoundEffect(sfxId);

            noSkip = true; //imitate goto LOAD_ANIMATION
        }

        entity.NextFrameDelay = 0x7fffffff;
        entity.ForceResetAnimationFlag = 1;
    }

    public void PlaySoundEffect(uint sfxId)
    {
        //TODO
    }

    private int ComputeEntityGroundHeight(Entity entity)
    {
        var xs = new int[4];
        var ys = new int[4];
        var x1 = (entity.XPos + entity.XMod) >> 16;
        var x2 = (entity.XPos + entity.XMod + entity.Width) >> 16;
        var y1 = (entity.YPos + entity.YMod) >> 16;
        var y2 = (entity.YPos + entity.YMod + entity.Depth) >> 16;
        xs[0] = x1;
        ys[0] = y1;
        xs[1] = x2;
        ys[1] = y1;
        xs[2] = x1;
        ys[2] = y2;
        xs[3] = x2;
        ys[3] = y2;
        var highest = 0;
        var slopesHit = 0;
        for (var dex = 0; dex < 4; dex++)
        {
            var x = xs[dex];
            var y = ys[dex];
            var tilex = x / 24;
            if (tilex > 0)
            {
                if (tilex >= 0x34)
                {
                    tilex = 0x33;
                }

                tilex = tilex << 16;
                tilex = tilex >> 16;
            }
            else
            {
                tilex = 0;
            }
            var tiley = y / 16;
            if (tiley > 0)
            {
                if (tiley >= 0x3c)
                {
                    tiley = 0x3b;
                }

                tiley = tiley << 16;
                tiley = tiley >> 16;
            }
            else
            {
                tiley = 0;
            }
            //int offset = (tilex * 8) + (tiley * 8 * 52);
            var tile = CurrentMap.Map.MapTiles[tiley * 52 + tilex];
            entity.MapTiles[dex] = tile;
            int height;
            if ((tile.Slope & 0x3) != 0)
            {
                height = tile.Height * 16;//puts it in pixels
                //bunch of slope stuff
                switch (tile.Slope & 0x3)
                {
                    case 1:
                        if ((slopesHit & 6) != 0)//it already hit 2 or 3
                        {
                            height += 0x10;//add a tile;
                        }
                        else
                        {
                            var my = ys[dex];
                            var result = height + 0x10;
                            var my2 = my;
                            if (my < 0)
                            {
                                my2 = my + 15;
                            }

                            my2 = my2 / 16;
                            my2 = my2 * 16;
                            var remainder = my - my2;
                            height = result - remainder;
                        }
                        slopesHit |= 1;
                        break;
                    case 2:
                        if ((slopesHit & 5) != 0)//it already hit 1 or 3
                        {
                            height += 0x10;//add a tile;
                        }
                        else
                        {
                            var mx = xs[dex];
                            var mx2 = mx / 24;
                            mx2 = mx2 * 24;
                            var remainder = mx - mx2;
                            remainder = 0x17 - remainder;

                            var result = (int)((float)remainder / 0x18 * 0x10);
                            /*var result = (int)((mx * (long)0x2aaaaaab)>>32);//get the high dword
                            int neg = result >> 31;
                            int res2 = result >> 2;//divide by 4
                            res2 = res2 - neg;
                            res2 = res2 * 3;
                            res2 = mx - res2;
                            res2 = 0x17 - res2;
                            res2 = res2 * 4;
                            //result = 0x236d4[res2];some lookuptable of heights based on width*/
                            height += result;
                        }
                        slopesHit |= 2;
                        break;
                    case 3:
                        if ((slopesHit & 3) != 0)//it already hit 1 or 2
                        {
                            height += 0x10;
                        }
                        else
                        {
                            var mx = xs[dex];
                            var mx2 = mx / 24;
                            mx2 = mx2 * 24;
                            var remainder = mx - mx2;
                            //remainder = 0x17 - remainder;

                            var result = (int)((float)remainder / 0x18 * 0x10);
                            /*var result = (int)((mx * (long)0x2aaaaaab) >> 32);//get the high dword
                            int neg = result >> 31;
                            int res2 = result >> 2;//divide by 4
                            res2 = res2 - neg;
                            res2 = res2 * 3;
                            res2 = mx - res2;
                            //res2 = 0x17 - res2; (only diff with other slope is subtracting it from 23, which is tilewidth-1)
                            res2 = res2 * 4;
                            //result = 0x236d4[res2];some lookuptable*/
                            height += result;
                        }
                        slopesHit |= 4;
                        break;
                }

                height = height << 16;//shift it over to fixed float
            }
            else
            {
                height = (tile.Height * 16) << 16;//put in pixels then shift over to fixed float
            }

            entity.MapHeights[dex] = height;
            if (highest < height)
            {
                highest = height;
            }
        }
        return highest;
    }

    //TODO all the slope stuff
    private void UpdateEntityCollisionData(Entity entity)
    {
        //set of variables set by certain special frames of animation
        if (entity.FrameCollision != null)
        {
            entity.FrameX = entity.XPos + entity.FrameXOff;
            entity.FrameY = entity.YPos + entity.FrameYOff;
            entity.FrameZ = entity.ZPos + entity.FrameZOff;
        }

        entity.TileZ = entity.ZPos >> 20; //(z >> 16) / 16
        entity.TileX = (entity.XPos >> 16) / 24;
        entity.TileY = entity.YPos >> 20;

        var hitz = GetCollisionOnZ(entity);
        int tohit;
        entity.ZEntityCollision = hitz;
        entity.CollidedWithEntityZ = hitz < entity.ZPos ? 0 : 1;
        if ((entity.Flags & 0x100) != 0)
        {
            tohit = 0xe00;
            var somevals = new int[4];
            for (var dex = 0; dex < 4; dex++)
            {
                var tl = entity.MapTiles[dex];
                var fullval = tl.Walkability | tl.GroundProperty << 8 | tl.Slope << 16 | tl.Height << 24;
                if (entity.MapHeights[dex] + 1 == entity.ModdedZPos)
                {

                    //var val = (tl.groundproperty & 0xe) << 8;
                    if ((fullval & 0xe00) < tohit)
                    {
                        somevals[dex] = fullval;
                        tohit = fullval & 0xe00;
                    }
                }
                else
                {
                    somevals[dex] = 0;
                    tohit = 0;
                }
            }

            entity.CombinedVramFlagsOR = somevals[0] | somevals[1] | somevals[2] | somevals[3];
            entity.CombinedVramFlagsAND = somevals[0] & somevals[1] & somevals[2] & somevals[3];

            var tilex = entity.TileX;

            if (tilex > 0)
            {
                if (tilex >= 0x34)
                {
                    tilex = 0x33;
                }
            }
            else
            {
                tilex = 0;
            }
            var tiley = entity.TileY;
            if (tiley > 0)
            {
                if (tiley >= 0x3c)
                {
                    tiley = 0x3b;
                }
            }
            else
            {
                tiley = 0;
            }

            var tile = CurrentMap.Map.MapTiles[tilex + tiley * 52];
            var fullval2 = tile.Walkability | tile.GroundProperty << 8 | tile.Slope << 16 | tile.Height << 24;
            var height = (int)(fullval2 & 0xff000000 >> 4) + 1;
            var r3 = height ^ entity.ModdedZPos;
        }
        else
        {
            tohit = 0;
            entity.CombinedVramFlagsOR = 0;
            entity.CombinedVramFlagsAND = 0;
        }

        //all that slope code is for setting this value
        entity.TileAttributes = 0;

        var prevtohit = entity.Slope_18c;
        entity.Slope_18c = tohit;
        entity.Slope_190 = prevtohit;
    }

    private int GetCollisionOnZ(Entity entity)
    {
        var collision = entity.FloorHeight + 1;
        if ((entity.Flags & 0x80) == 0)
        {
            return collision;
        }

        if ((entity.AnimFlags & 0x80) != 0)
        {
            return collision;
        }

        if (entity.PlatformEntity != null)
        {
            return collision;
        }

        if (StaticVariables.g_collideableEntitiesCount <= 0)
        {
            return collision;
        }

        for (var dex = 0; dex < StaticVariables.g_collideableEntitiesCount; dex++)
        {
            var checkme = StaticVariables.g_collideableEntities[dex];

            if (checkme == entity)
            {
                continue;
            }

            if (checkme.ModdedZPos + checkme.Height >= entity.ModdedZPos
                || checkme.ModdedZPos + checkme.Height < collision)
            {
                continue;
            }

            if (checkme.ModdedXPos - entity.ModdedXPos >= 0)
            {
                if (checkme.ModdedXPos - entity.ModdedXPos >= entity.Width + 1)
                {
                    continue;
                }
            }
            else
            {
                if (entity.ModdedXPos - checkme.ModdedXPos >= checkme.Width + 1)
                {
                    continue;
                }
            }

            if (checkme.ModdedYPos - entity.ModdedYPos >= 0)
            {
                if (checkme.ModdedYPos - entity.ModdedYPos < entity.Depth + 1)
                {
                    collision = checkme.ModdedZPos + checkme.Height;
                }
            }
            else
            {
                if (entity.ModdedYPos - checkme.ModdedYPos < checkme.Depth + 1)
                {
                    collision = checkme.ModdedZPos + checkme.Height;
                }
            }

        }
        return collision;
    }

    private void InitializeContents(Entity entity)
    {
        if (entity.EntityRecord != null)
        {
            int u7 = entity.EntityRecord.U7;

            if ((u7 & 0x7ffff) >= 800)
            {
                u7 = 0;
            }

            entity.ContentsGameFlag = u7;
            if (u7 != 0)
            {
                var flagid = ((u7 >> 3) & 0xffc) >> 2;

                uint flag;
                if ((u7 & 0x8000) != 0)
                {
                    flag = StaticVariables.g_mapFlags[flagid];
                }
                else
                {
                    flag = StaticVariables.g_globalFlags[flagid];
                }

                var val = u7;
                if (u7 < 0)
                {
                    val = u7 + 0x1f;
                }
                var val2 = val >> 5;
                val2 = val2 << 5;
                var dif = val - val2;
                var bitToCheck = 1 << dif;
                if ((flag & bitToCheck) != 0)
                {
                    entity.ContentsItemId = (uint)GetContentsItemId((short)0);
                    return;
                }
            }
            if (entity.EntityRecord.Contents != 0)
            {
                entity.ContentsItemId = (uint)GetContentsItemId((short)entity.EntityRecord.Contents);
                return;
            }
        }
        else
        {
            entity.ContentsGameFlag = 0;
        }

        entity.ContentsItemId = (uint)GetContentsItemId((short)entity.Sprite.Header.Contents);
    }

    private int GetContentsItemId(short contentId)
    {
        var isValid = (int)contentId < 0x100;

        while (true)
        {
            if (!isValid)
            {
                return 0;
            }
            if ((contentId & 0x80) == 0) break;
            StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
            contentId = (short)((contentId & 0x7f) * 0x10 + -0x7ffd71f4 + StaticVariables.g_gameRandomSeed * 0x10 >> 0x20);
            isValid = contentId < 0x100;
        }

        if (0x61 < (int)contentId)
        {
            return 0;
        }

        return contentId;
    }

    private void WarpPlayer(int posX, int posY, int posZ, int transitionType)
    {
        int drawPage;
        int transitionDuration;

        StaticVariables.g_playerStartX = posX << 16;
        StaticVariables.g_playerStartY = posY << 16;
        StaticVariables.g_playerStartZ = posZ << 16;
        StaticVariables.g_fadeFrameCounter = 0;
        StaticVariables.g_warpStepFlags_2 = 0;
        StaticVariables.g_warpFlags = 0;
        StaticVariables.g_mapOffsetY = 0;
        StaticVariables.g_mapOffsetX = 0;
        StaticVariables.g_warpDelayFrames = 10;

        switch (transitionType)
        {
            case 0:
                drawPage = 2;
                transitionDuration = 0x10;
                StaticVariables.g_currentFadeColorR = 0xff0000;
                StaticVariables.g_currentFadeColorG = 0xff0000;
                StaticVariables.g_currentFadeColorB = 0xff0000;
                StaticVariables.g_targetFadeColorR = 0;
                StaticVariables.g_targetFadeColorG = 0;
                StaticVariables.g_targetFadeColorB = 0;
                StaticVariables.g_warpStepFlags_2 = 1;
                break;

            default:
                drawPage = 2;
                transitionDuration = 1;
                break;

            case 2:
            case 10:
                drawPage = 1;
                transitionDuration = 0x10;
                StaticVariables.g_currentFadeColorR = 0xff0000;
                StaticVariables.g_currentFadeColorG = 0xff0000;
                StaticVariables.g_currentFadeColorB = 0xff0000;
                StaticVariables.g_targetFadeColorR = 0;
                StaticVariables.g_targetFadeColorG = 0;
                StaticVariables.g_targetFadeColorB = 0;
                StaticVariables.g_warpStepFlags_2 = 1;
                break;

            case 4:
                StaticVariables.g_currentFadeColorR = 0xff0000;
                StaticVariables.g_currentFadeColorG = 0xff0000;
                StaticVariables.g_currentFadeColorB = 0xff0000;
                StaticVariables.g_targetFadeColorR = 0;
                StaticVariables.g_targetFadeColorG = 0;
                StaticVariables.g_targetFadeColorB = 0;
                StaticVariables.g_warpStepFlags_2 = 1;
                ApplyScreenFade(1, 8);
                StaticVariables.g_fadeColorStepB = StaticVariables.g_fadeColorStepB >> 1;
                StaticVariables.g_warpColorStepG = StaticVariables.g_warpColorStepG >> 1;
                return;

            case 5:
                drawPage = 2;
                transitionDuration = 1;
                StaticVariables.g_mapOffsetX = 0xa0;
                StaticVariables.g_mapOffsetY = 0x78;
                break;

            case 6:
                drawPage = 1;
                transitionDuration = 0x18;
                StaticVariables.g_mapOffsetX = 0xa0;
                StaticVariables.g_mapOffsetY = 0x78;
                StaticVariables.g_currentFadeColorR = 0xff0000;
                StaticVariables.g_currentFadeColorG = 0xff0000;
                StaticVariables.g_currentFadeColorB = 0xff0000;
                StaticVariables.g_targetFadeColorR = 0;
                StaticVariables.g_targetFadeColorG = 0;
                StaticVariables.g_targetFadeColorB = 0;
                StaticVariables.g_warpStepFlags_2 = 1;
                break;

            case 8:
                StaticVariables.g_currentFadeColorR = 0xff0000;
                StaticVariables.g_currentFadeColorG = 0xff0000;
                StaticVariables.g_currentFadeColorB = 0xff0000;
                StaticVariables.g_targetFadeColorR = 0;
                StaticVariables.g_targetFadeColorG = 0;
                StaticVariables.g_targetFadeColorB = 0;
                StaticVariables.g_warpStepFlags_2 = 1;
                ApplyScreenFade(2, 0x3c);
                StaticVariables.g_fadeColorStepB = StaticVariables.g_fadeColorStepR << 2;
                return;
        }

        ApplyScreenFade(drawPage, transitionDuration);
    }

    private void ApplyScreenFade(int fadeMode, int fadeDuration)
    {
        BeginFadeEffect(fadeMode, fadeDuration);
        SetFadeDuration(fadeDuration);
    }

    private void BeginFadeEffect(int fadeTPageIndex, int fadeDuration)
    {
        if (StaticVariables.g_warpStepFlags_2 == 0)
        {
            StaticVariables.g_fadeColorStepR = 0;
            StaticVariables.g_warpColorStepG = 0;
            StaticVariables.g_fadeColorStepB = 0;
            StaticVariables.g_currentFadeColorB = StaticVariables.g_targetFadeColorB;
            StaticVariables.g_currentFadeColorG = StaticVariables.g_targetFadeColorG;
            StaticVariables.g_currentFadeColorR = StaticVariables.g_targetFadeColorR;
        }
        else
        {
            //SetDrawTPage(StaticVariables.g_fadeTPagePrim1, 0, 0, StaticVariables.g_tPageFadeLUT[fadeTPageIndex * 0x16]);
            //SetDrawTPage(StaticVariables.g_fadeTPagePrim2, 0, 0, StaticVariables.g_tPageFadeLUT[fadeTPageIndex * 0x16]);

            StaticVariables.g_fadeColorStepB = (StaticVariables.g_targetFadeColorB - StaticVariables.g_currentFadeColorB) / fadeDuration;
            if (fadeDuration == 0)
            {
                //Trap(0x1c00);
            }
            if (fadeDuration == -1 && StaticVariables.g_targetFadeColorB - StaticVariables.g_currentFadeColorB == -0x80000000)
            {
                //Trap(0x1800);
            }

            StaticVariables.g_warpColorStepG = (StaticVariables.g_targetFadeColorG - StaticVariables.g_currentFadeColorG) / fadeDuration;
            if (fadeDuration == 0)
            {
                //Trap(0x1c00);
            }
            if (fadeDuration == -1 && StaticVariables.g_targetFadeColorG - StaticVariables.g_currentFadeColorG == -0x80000000)
            {
                //Trap(0x1800);
            }

            StaticVariables.g_fadeColorStepR = (StaticVariables.g_targetFadeColorR - StaticVariables.g_currentFadeColorR) / fadeDuration;
            if (fadeDuration == 0)
            {
                //Trap(0x1c00);
            }
            if (fadeDuration == -1 && StaticVariables.g_targetFadeColorR - StaticVariables.g_currentFadeColorR == -0x80000000)
            {
                //Trap(0x1800);
            }
        }
    }

    private void SetFadeDuration(int fadeDuration)
    {
        if (StaticVariables.g_warpFlags == 0)
        {
            StaticVariables.g_playerStepZ = 0;
            StaticVariables.g_playerStepY = 0;
            StaticVariables.g_playerStepX = 0;
            StaticVariables.g_playerLastX = StaticVariables.g_playerStartX;
            StaticVariables.g_playerLastY = StaticVariables.g_playerStartY;
            StaticVariables.g_playerLastZ = StaticVariables.g_playerStartZ;
        }
        else
        {
            StaticVariables.g_playerStepX = (StaticVariables.g_playerStartX - StaticVariables.g_playerLastX) / fadeDuration;
            if (fadeDuration == 0)
            {
                //Trap(0x1c00);
            }
            if (fadeDuration == -1 && StaticVariables.g_playerStartX - StaticVariables.g_playerLastX == -0x80000000)
            {
                //Trap(0x1800);
            }

            StaticVariables.g_playerStepY = (StaticVariables.g_playerStartY - StaticVariables.g_playerLastY) / fadeDuration;
            if (fadeDuration == 0)
            {
                //Trap(0x1c00);
            }
            if (fadeDuration == -1 && StaticVariables.g_playerStartY - StaticVariables.g_playerLastY == -0x80000000)
            {
                //Trap(0x1800);
            }

            StaticVariables.g_playerStepZ = (StaticVariables.g_playerStartZ - StaticVariables.g_playerLastZ) / fadeDuration;
            if (fadeDuration == 0)
            {
                //Trap(0x1c00);
            }
            if (fadeDuration == -1 && StaticVariables.g_playerStartZ - StaticVariables.g_playerLastZ == -0x80000000)
            {
                //Trap(0x1800);
            }
        }
    }

    private void InitializeTileAnimationSystem()
    {
        SetTileAnimationMode(3, 0);
        StaticVariables.g_tileOffset = 0;
        StaticVariables.g_animationCounter = 2;
    }

    private int OpenMap(int mapId)
    {
        var iVar1 = GetMapWarpDestination(mapId);

        if (iVar1 != 0)
        {
            int iVar2 = StaticVariables.g_currentMapSoundIndex;
            iVar1 = GetMapWarpDestination(mapId);

            if (iVar2 != iVar1)
            {
                if (StaticVariables.g_requestedSeqId >= 0)
                {
                    //InitBgm(StaticVariables.g_requestedSeqId);
                    //ResetSomethingSound(StaticVariables.g_requestedSeqId);
                }

                iVar1 = GetMapWarpDestination(mapId);
                if (iVar1 != 0x2d)
                {
                    iVar1 = GetMapWarpDestination(mapId);
                    //MaybeLoadSound(iVar1, 0);
                }

                //FUN_8008f808(StaticVariables.g_requestedSeqId, 0x7f, 10);
            }
        }

        //iVar1 = GetSoundGroupBbyMapId(mapId);
        //
        //if (StaticVariables.g_currentSoundGroup != iVar1)
        //{
        //    FUN_800489c8(mapId);
        //}
        //
        //FUN_8005ac90();
        PrepareBufferFlip();

        return 1;
    }

    private uint GetMapWarpDestination(int mapId)
    {
        uint currentMapId;
        int warpDataIndex = 0;

        if (StaticVariables.g_warpMapList[0] != 0)
        {
            var warpDataPtr = StaticVariables.g_warpMapList;
            currentMapId = warpDataPtr[warpDataIndex];

            do
            {
                if (mapId == currentMapId)
                {
                    var bitfieldPtr = StaticVariables.g_mapFlags;

                    if ((warpDataPtr[warpDataIndex + 1] & 0x8000) != 0)
                    {
                        bitfieldPtr = StaticVariables.g_globalFlags;
                    }

                    var bitfieldOffset = (warpDataPtr[warpDataIndex + 1] >> 3) & 0xffc;
                    var bitfieldValue = bitfieldPtr[bitfieldOffset / 4];

                    if ((bitfieldValue & (1 << (int)(warpDataPtr[warpDataIndex + 1] & 0x1f))) != 0)
                    {
                        return warpDataPtr[warpDataIndex + 2];
                    }
                }

                warpDataIndex += 3;
                currentMapId = warpDataPtr[warpDataIndex];
            }
            while (currentMapId != 0);
        }

        return StaticVariables.g_defaultWarpDestinations[mapId];
    }

    private uint GetSoundGroupBbyMapId(int warpId)
    {
        return StaticVariables.g_soundGroupByMapId[warpId];
    }

    private void PrepareBufferFlip()
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



    private void ResetDebugRenderingState()
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

    private void EndFrame()
    {
        //DrawSync(0);
        //(*(code *)g_screenUpdateFunc_ClearOrderTables)((uint)g_display_overflow_message & 1);
        //VSync(0);
        //UpdateDisplayEnvironments();
        if (StaticVariables.g_gameplayTime < 0x14996c4)
        {
            StaticVariables.g_gameplayTime++;
        }
        //ResetRCnt(0xf2000001);
        //MoveImage(g_currentDrawEnv + 1,(int)g_currentDrawEnv->x,(int)g_currentDrawEnv->y);
    }

    private void UpdatePads()
    {
        var padState = PadRead();
        UpdatePad(StaticVariables.g_padState1, (ushort)padState);
        UpdatePad(StaticVariables.g_padState2, (ushort)(padState >> 0x10));
    }

    private ulong PadRead()
    {
        return 0L;
    }

    private void UpdatePad(PadState padState, ushort buttonState)
    {
        uint numberOfFrameHold;

        padState.ButtonsJustPressed = (ushort)(buttonState & (buttonState ^ padState.ButtonsHold));
        padState.ButtonReleased = (ushort)(padState.ButtonsHold & (buttonState ^ padState.ButtonsHold));

        if (padState.ButtonsHold != buttonState || padState.ButtonsHold == 0)
        {
            padState.IsOverThanMaxNbFrameHeld = 0;
            padState.NumberOfFrameHold = 0;
            padState.ButtonsJustPressedByInterval = padState.ButtonsJustPressed;
            padState.ButtonsHold = buttonState;
            return;
        }

        if (padState.IsOverThanMaxNbFrameHeld == 0)
        {
            numberOfFrameHold = padState.NumberOfFrameHold;

            if (numberOfFrameHold < padState.MaxNbFrameHeld)
            {
                LAB_8002e2fc:
                padState.NumberOfFrameHold = numberOfFrameHold + 1;
                padState.ButtonsJustPressedByInterval = 0;
                padState.ButtonsHold = buttonState;
                return;
            }

            padState.IsOverThanMaxNbFrameHeld = 1;
        }
        else
        {
            numberOfFrameHold = padState.NumberOfFrameHold;

            if (numberOfFrameHold < padState.RepeatInterval)
            {
                padState.NumberOfFrameHold = numberOfFrameHold + 1;
                padState.ButtonsJustPressedByInterval = 0;
                padState.ButtonsHold = buttonState;
                return;
            }
        }

        padState.NumberOfFrameHold = 0;
        padState.ButtonsJustPressedByInterval = buttonState;
        padState.ButtonsHold = buttonState;
    }

    private void StartWarpTransition(int warpType)
    {
        //DrawSync(0);
        //MoveImage(StaticVariables.g_currentDrawEnv, 0x140, 0);
        //DrawSync(0);

        StaticVariables.g_mapScreenPosX = 0x140;
        StaticVariables.g_mapScreenPosY = 0xf0;
        StaticVariables.g_mapOffsetX = 0;
        StaticVariables.g_mapOffsetY = 0;
        StaticVariables.g_fadeFrameCounter = 0;
        StaticVariables.g_warpStepFlags_2 = 0;
        StaticVariables.g_warpFlags = 0;

        //switch (warpType)
        //{
        //    case 0:
        //    case 9:
        //        InitStandardWarpEffect();
        //        break;
        //    default:
        //        FUN_80042f18();
        //        break;
        //    case 2:
        //    case 10:
        //        InitUnknownWarpEffect();
        //        break;
        //    case 4:
        //        InitInstantWarpEffect();
        //        break;
        //    case 5:
        //        InitFadeOutWarp();
        //        break;
        //    case 6:
        //        InitSpecialWarpEffect();
        //        break;
        //    case 8:
        //        InitializeMapChangeWarp();
        //        break;
        //    case 11:
        //        InitializeCutsceneWarp();
        //        break;
        //}
    }

    private void RenderScene(Graphics graphics)
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


        Renderer.Render(graphics, _datasBin, CurrentMap);
    }

    private int RenderTiles(int[] renderListBase, int offsetX, int offsetY, int offsetZ)
    {
        //TODO
        ResetTileAnimationState();
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

    private void UpdateEntities(int endGame)
    {
        StaticVariables.g_mapOffsetX -= 8;
        if (StaticVariables.g_mapOffsetX < 0)
        {
            StaticVariables.g_mapOffsetX = 0;
        }
        StaticVariables.g_mapScreenPosX = StaticVariables.g_mapOffsetX * -2 + 0x140;

        StaticVariables.g_mapOffsetY -= 6;
        if (StaticVariables.g_mapOffsetY < 0)
        {
            StaticVariables.g_mapOffsetY = 0;
        }
        StaticVariables.g_mapScreenPosY = StaticVariables.g_mapOffsetY * -2 + 0xf0;

        UpdatePads();

        int entityBeforeWarp = StaticVariables.g_lastWarpEntityIndex;
        int finalEntity;

        if (StaticVariables.g_playerControlFlags == 0 &&
            StaticVariables.PlayerEntity.IsNotProcessable == 0 &&
            StaticVariables.g_warpLockTimer == 0 &&
            StaticVariables.g_padState1.ButtonsHold == 0x900 &&
            StaticVariables.g_warpDelayFrames == 0 &&
            StaticVariables.g_globalTransitionState == 0)
        {
            finalEntity = StaticVariables.g_lastWarpEntityIndex + 1;

            if (StaticVariables.g_lastWarpEntityIndex == 0x78)
            {
                finalEntity = StaticVariables.g_lastWarpEntityIndex;

                if (StaticVariables.PlayerEntity.Hp != 0)
                {
                    int canWarp = IsMapUnlocked(0x27);
                    if (canWarp != 0)
                    {
                        StartWarpToMap(0x27);
                    }
                    StaticVariables.g_lastWarpEntityIndex = 0;
                    StaticVariables.PlayerEntity.Hp = 0;
                    //StaticVariables.PlayerEntity.DamagedTickCounter = entityBeforeWarp;
                    StaticVariables.INT_ARRAY_800a8284[0] = 0;
                    FinalizeWarpEntities(0);
                    finalEntity = StaticVariables.g_lastWarpEntityIndex;
                }
            }
        }
        else
        {
            StaticVariables.g_lastWarpEntityIndex = 0;
            finalEntity = StaticVariables.g_lastWarpEntityIndex;
        }

        StaticVariables.g_lastWarpEntityIndex = finalEntity;

        UpdateEntitiesPostUpdateLogic();

        if (StaticVariables.g_warpDelayFrames != 0)
        {
            StaticVariables.g_warpDelayFrames--;
        }

        if (StaticVariables.g_playerControlFlags == 0 &&
            StaticVariables.PlayerEntity.IsNotProcessable == 0 &&
            StaticVariables.g_warpLockTimer == 0 &&
            (StaticVariables.g_padState1.ButtonsJustPressed & 0x803) != 0 &&
            StaticVariables.g_warpDelayFrames == 0 &&
            (StaticVariables.g_padState1.ButtonsHold & 0x100) == 0 &&
            StaticVariables.g_globalTransitionState == 0 &&
            IsInForbiddenWarpZone() == 0)
        {
            StaticVariables.g_isGameEnding = 1;
        }

        //HandleMapSoundStreaming();

        StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;

        if (endGame != 0)
        {
            StaticVariables.g_isGameEnding = 0;
        }
    }

    private int IsMapUnlocked(int warpIndex)
    {
        int iVar1;

        if (warpIndex < 0 || StaticVariables.g_totalWarpEntries <= warpIndex)
        {
            //LogDebugMessage(StaticVariables.g_buffer_isMapUnlocked, warpIndex);
            iVar1 = 0;
        }
        else
        {
            iVar1 = StaticVariables.g_warpUsageTable[warpIndex * 2 + 1];
        }

        return iVar1;
    }

    private int StartWarpToMap(uint mapIndex)
    {
        int warpEntryOffset;
        short remainingWarps;

        if ((int)mapIndex < 0 || StaticVariables.g_totalWarpEntries <= (int)mapIndex)
        {
            //LogDebugMessage(StaticVariables.g_logMessage_InvalidWarpVisualId + 0x54, mapIndex);
            warpEntryOffset = 0;
        }
        else
        {
            warpEntryOffset = (int)(mapIndex * 2) * 2 + StaticVariables.g_warpUsageTable[0];
            remainingWarps = (short)StaticVariables.g_warpUsageTable[mapIndex * 2 + 1];
            remainingWarps--;

            if (remainingWarps == -1)
            {
                warpEntryOffset = -1;
            }
            else
            {
                StaticVariables.g_warpUsageTable[mapIndex * 2 + 1] = remainingWarps;
                warpEntryOffset = remainingWarps;
            }
        }

        return warpEntryOffset;
    }

    private int IsInForbiddenWarpZone()
    {
        //uint isSpecialWarpTriggered;
        //
        //if (StaticVariables.g_forbiddenWarpFlag == 0)
        //{
        //    isSpecialWarpTriggered = CheckSpecialWarpCondition(0);
        //    if (isSpecialWarpTriggered != 0)
        //    {
        //        return 1;
        //    }
        //
        //    isSpecialWarpTriggered = CheckSpecialWarpCondition(0xb);
        //    if (isSpecialWarpTriggered != 0)
        //    {
        //        return 1;
        //    }
        //
        //    if (StaticVariables.g_cdIsReady == 0)
        //    {
        //        if ((StaticVariables.g_playerTileAttribute & 0x2000) != 0)
        //        {
        //            TriggerWarpTypeA();
        //            return 1;
        //        }
        //        if ((StaticVariables.g_playerTileAttribute & 0x8000) != 0)
        //        {
        //            TriggerWarpTypeB();
        //            return 0;
        //        }
        //        if ((StaticVariables.g_playerTileAttribute & 0x1000) != 0)
        //        {
        //            StartFadeOut();
        //            return 1;
        //        }
        //        if ((StaticVariables.g_playerTileAttribute & 0x4000) != 0)
        //        {
        //            TriggerWarpTypeC();
        //            return 1;
        //        }
        //    }
        //
        //    InitializeFrame();
        //    SetTransitionType(6);
        //    GetFadeSettings(0);
        //    InitCameraTransition();
        //    DisplayWarpNames();
        //    PlaySoundEffect(4);
        //}

        return 1;
    }

    private void UpdateEntitiesPostUpdateLogic()
    {
        if (StaticVariables.g_animationRawSize > 0x38800)
        {
            if ((StaticVariables.g_display_overflow_message & 4) == 0)
            {
                //iVar2 = strlen(&g_debugMessage);
                //sprintf(&g_debugMessage + iVar2,"\n");
            }
            else
            {
                //iVar1 = strlen(&g_debugMessage);
                //iVar2 = g_animationRawSize;
                //if (g_animationRawSize < 0) {
                //    iVar2 = g_animationRawSize + 0x7ff;
                //}
                //sprintf(&g_debugMessage + iVar1,"RESIDENT DATA OVER!! %d/%d\n",iVar2 >> 0xb,0x71);
            }
        }

        //StaticVariables.g_currentEntitySpriteImages = 0x8011cb60;
        StaticVariables.g_spriteNumberOfImage = 0;

        RunMapEvents();
        UpdateAllEntitiesPostLogic();
        UpdateAllActiveEffectsPostLogic();
    }

    private void RunMapEvents()
    {
        if ((StaticVariables.g_playerControlFlags & 0x48) != 0)
        {
            return;
        }

        var medex = 0;
        var playerEntity = StaticVariables.PlayerEntity;

        foreach (var mapEvent in StaticVariables.g_mapEvents)
        {
            var eventCode = mapEvent.ProgramBMap;
            if ((eventCode & 0x7f) == 0)
            {
                continue;
            }

            var rec = mapEvent.MapEventRecord;
            if (playerEntity.TileX < rec.X1 || playerEntity.TileX > rec.X2 || playerEntity.TileY < rec.Y1 || playerEntity.TileY > rec.Y2)
            {
                playerEntity.ProgramIndexes[ScriptHelper.ProgramBMap] = mapEvent.ProgramBMap;
                playerEntity.MapEventProgramId = mapEvent.ProgramBMap;
                playerEntity.EventProgramState = mapEvent.EventData;
                playerEntity.EventTrigger = medex;
                playerEntity.LogicContextEntity = mapEvent.Entity;

                _entityEventHandlers.RunEntityEventScripts(playerEntity, ScriptHelper.ProgramBMap);

                mapEvent.ProgramBMap = playerEntity.ProgramIndexes[ScriptHelper.ProgramBMap];
                mapEvent.EventData = playerEntity.EventProgramState;
                mapEvent.Entity = playerEntity.LogicContextEntity;
            }
            else
            {
                mapEvent.EventData.Sp = 0;
                mapEvent.EventData.Exp = 0;
                mapEvent.EventData.LogicResult = 0;
                mapEvent.Entity = playerEntity;
                mapEvent.ProgramBMap = rec.EventCodesBIndex;
            }
            medex++;
        }
    }

    public void UpdateAllActiveEffectsPostLogic()
    {
        foreach (var effect in StaticVariables.g_effectSlots)//g_effectSlots)
        {
            if (effect.Status != 2)
            {
                continue;
            }

            if ((StaticVariables.g_playerControlFlags & 0x48) == 0)
            {
                if (effect.DestroyFlag != 0)
                {
                    effect.Status = 0;
                    continue;
                }

                UpdateEffectAnimation(effect);
                UpdateEffectPosition(effect);
            }

            effect.SpriteRef.DepthSortVal = effect.DepthSortVal;
            effect.SpriteRef.X = effect.X;
            effect.SpriteRef.Y = effect.Y;
            effect.SpriteRef.Z = effect.Z;
            StaticVariables.g_spriteImages[StaticVariables.g_spriteNumberOfImage++] = effect.SpriteRef;
            //StaticVariables.g_currentEntitySpriteImages++;
            //StaticVariables.g_spriteNumberOfImage++;
        }
    }


    private void UpdateEffectAnimation(SpriteEffect effect)
    {
        if (effect.CurrentSpriteTableIndex != effect.TargetSpriteTableIndex
            || effect.CurrentIsMapSprite != effect.TargetIsMapSprite)
        {
            int addtosheet, addtopal;
            var record = GetEffectSpriteFromSpriteTable(effect.TargetIsMapSprite != 0, effect.TargetSpriteTableIndex, out addtosheet, out addtopal);
            if (record == null)
            {
                effect.DestroyFlag = 1;
                effect.SpriteRef.Images = null;
                effect.SpriteRef.NumImages = 0;
                //field after numimages = 0
                return;
            }

            effect.SpriteEffectRecord = record;
            effect.CurrentIsMapSprite = effect.TargetIsMapSprite;
            effect.CurrentSpriteTableIndex = effect.TargetSpriteTableIndex;

            effect.SheetSize = addtosheet;
            effect.PaletteIndex = addtopal;
            effect.CurrentAnimation = (byte)~effect.TargetAnimation;
        }

        if (effect.CurrentAnimation != effect.TargetAnimation)
        {
            var animation = effect.SpriteEffectRecord.PreloadedAnims[effect.TargetAnimation];
            effect.AnimIndex = 0;
            var nframe = animation.Frames[effect.AnimIndex];
            effect.CurrentAnimation = effect.TargetAnimation;
            effect.Delay = 0;
            effect.DestroyFlag = 0;
            effect.InitialFrame = nframe;
            effect.Frame = nframe;
            Debug.Assert(nframe != null);
        }
        else
        {
            effect.Delay--;
            if ((effect.Delay & 0xff) != 0)
            {
                return;
            }
        }

        do
        {
            var frame = effect.Frame;
            if ((frame.Delay & 0x80) != 00)
            {
                //effect.AnimIndex++;
                var anim = effect.SpriteEffectRecord.PreloadedAnims[effect.TargetAnimation];
                frame = anim.Frames[effect.AnimIndex];
                effect.Frame = frame;
                Debug.Assert(frame != null);
                if (frame?.Images != null)
                {
                    effect.SpriteRef.Images = frame.Images.Images;
                    effect.SpriteRef.DepthSortVal = frame.Images.Unknown;
                    effect.SpriteRef.NumImages = frame.Images.NumberOfImages;
                    return;
                }
                effect.SpriteRef.Images = null;
                effect.SpriteRef.DepthSortVal = 0;
                effect.SpriteRef.NumImages = 0;
                return;
            }
            if (frame.Delay != 0)
            {
                if (frame.Delay == 1)
                {
                    //repeat
                    effect.AnimIndex = 0;
                    effect.Frame = effect.InitialFrame;
                    continue;
                }
                throw new Exception("Error with Effect Animation!");
            }

            //its 0  which means its non repeating so flag for destroy
            effect.Delay = 0xff;
            effect.DestroyFlag = 1;
            return;
        } while (true);

    }

    private SpriteEffectRecord GetEffectSpriteFromSpriteTable(bool isMapSprite, int spritetableindex, out int addedtosheet, out int addedtopallette)
    {
        SpriteInfo si;
        if (isMapSprite)
        {
            si = CurrentMap.SpriteInfo;
            addedtosheet = 0;
            addedtopallette = 0x20;
        }
        else
        {
            si = _datasBin.AlundraGameMap.SpriteInfo;
            addedtosheet = 0xb;
            addedtopallette = 0x60;
        }
        if (spritetableindex >= 0 && spritetableindex < si.SpriteTable.Length)
        {
            return si.SpriteEffects[spritetableindex];
        }

        return null;
    }

    private void UpdateEffectPosition(SpriteEffect effect)
    {
        if (effect.UpdateMode == 0)
        {
            effect.X += effect.XForce; //forces?
            effect.Y += effect.YForce;
            effect.Z += effect.ZForce;
            //some kind of unique id? maybe its used for zsorting
            effect.DepthSortVal = (int)(effect.Y & 0xffff0000) + (effect.Z >> 16) + (effect.SpriteRef.NumImages << 16);
            return;
        }

        if (effect.UpdateMode == 1)
        {
            var entity = effect.AttachedEntity;
            if (entity.Status != 0)
            {
                effect.X = entity.XPos + effect.XOff;
                effect.Y = entity.YPos + effect.YOff;
                effect.Z = entity.ZPos + effect.ZOff;
                effect.DepthSortVal = entity.DepthSortVal + effect.DepthSortMod;
                if (entity.Status == 4)
                {
                    effect.UpdateMode = 2;
                }
            }
            else
            {
                effect.UpdateMode = 2;
            }
        }
        else if (effect.UpdateMode != 3)
        {
            return;
        }
        //param3== 1 falls through to here, and param3 == 3 is here
        effect.X += effect.XForce; //forces?
        effect.Y += effect.YForce;
        effect.Z += effect.ZForce;

        if (effect.AttachedEntity.Status != 0)
        {
            effect.DepthSortVal = effect.AttachedEntity.DepthSortVal + effect.DepthSortMod;

            if (effect.AttachedEntity.Status == 4)
            {
                effect.UpdateMode = 2;
            }
        }
        else
        {
            effect.UpdateMode = 2;
        }
    }

    private void UpdateAllEntitiesPostLogic()
    {
        if ((StaticVariables.g_playerControlFlags & 0x48) == 0)
        {
            UpdateDestroyedEntities();

            UpdateEntitiesEvents();

            UpdateEntitiesCounters();



            UpdateEntityLists();

            UpdateEntitiesAnimation();

            UpdateEntitiesPhysics();

            UpdateActiveEffects();

            UpdateBalanceRecords();
        }
        else
        {
            UpdateEntityLists();
        }

        if (StaticVariables.g_entityFollowedByCamera != null && StaticVariables.g_entityFollowedByCamera.Status <= 3)
        {
            StaticVariables.g_cameraLookAtX = StaticVariables.g_entityFollowedByCamera.XPos + 2; // >> 16;
            StaticVariables.g_cameraLookAtY = StaticVariables.g_entityFollowedByCamera.YPos + 2; // >> 16;
            StaticVariables.g_cameraLookAtZ = StaticVariables.g_entityFollowedByCamera.ZPos + 2; // >> 16;
        }

        UpdateVisibleEntitiesZSort();

        //add spriterefs
        if (StaticVariables.g_visibleEntityCount > 0)
        {
            for (var i = 0; i < StaticVariables.g_visibleEntityCount; i++)
            {
                var entity = StaticVariables.g_visibleEntities[i];

                entity.SpriteRef.DepthSortVal = entity.DepthSortVal;
                entity.SpriteRef.X = entity.XPos;
                entity.SpriteRef.Y = entity.YPos;
                entity.SpriteRef.Z = entity.ZPos;
                StaticVariables.g_spriteImages[StaticVariables.g_spriteNumberOfImage++] = entity.SpriteRef;
            }
        }
    }

    private void UpdateEntitiesPhysics()
    {
        for (var i = 0; i < StaticVariables.g_activeEntityCount; i++)
        {
            var entity = StaticVariables.g_activeEntities[i];
            entity.PlatformUpdateFlag = 0;
            entity.CollidedWithEntityZ = 0;
            entity.ForceAdjusted = 0;

            entity.ModdedXPos = entity.XPos + entity.XMod;
            entity.ModdedYPos = entity.YPos + entity.YMod;
            entity.ModdedZPos = entity.ZPos + entity.ZMod;
        }

        CheckRidingEntities();
        UpdateEntitiesForces();

        for (var i = 0; i < StaticVariables.g_collideableEntitiesCount; i++)
        {
            var entity = StaticVariables.g_collideableEntities[i];
            if (entity.RidingEntity != null)
            {
                UpdateRidingEntity(entity, entity.RidingEntity);
            }
        }

        for (var i = 0; i < StaticVariables.g_activeEntityCount; i++)
        {
            var entity = StaticVariables.g_activeEntities[i];
            if (entity.PlatformUpdateFlag != 0)
            {
                //MoveEntity(entity);
            }
        }

        for (var i = 0; i < StaticVariables.g_activeEntityCount; i++)
        {
            var entity = StaticVariables.g_activeEntities[i];
            UpdateTile(entity);
        }
    }

    private void UpdateRidingEntity(Entity entity, Entity ridingEntity)
    {
        if (ridingEntity.RidingEntity != null)
        {
            //TODO: bug maybe in CheckRidingEntities why overflow ???
            //UpdateRidingEntity(ridingEntity, ridingEntity.RidingEntity);
        }

        entity.FinalXForce += ridingEntity.AdjustedXForce;
        entity.FinalYForce += ridingEntity.AdjustedYForce;
        if (entity.IsZForceApplied == 0)
        {
            entity.ZForce = ridingEntity.FinalZForce;
            entity.FinalYForce = ridingEntity.FinalZForce;
        }
    }

    private void CheckRidingEntities()
    {
        for (var i = 0; i < StaticVariables.g_collideableEntitiesCount; i++)
        {
            var entity = StaticVariables.g_collideableEntities[i];
            if ((entity.Flags & 0x4100) != 0x0100)
            {
                continue;
            }

            for (var j = 0; j < StaticVariables.g_collideableEntitiesCount; j++)
            {
                var entity2 = StaticVariables.g_collideableEntities[j];
                if (entity == entity2)
                {
                    continue;
                }

                if ((entity2.ModdedXPos - entity.ModdedXPos >= 0 && entity2.ModdedXPos - entity.ModdedXPos < entity.Width + 1) || (entity2.ModdedXPos - entity.ModdedXPos < 0 && entity.ModdedXPos - entity2.ModdedXPos < entity2.Width + 1))
                {
                    if (entity2.ModdedYPos - entity.ModdedYPos >= 0 && entity2.ModdedYPos - entity.ModdedYPos < entity.Depth + 1)
                    {
                        entity.RidingEntity = entity2;
                        break;
                    }

                    if (entity2.ModdedYPos - entity.ModdedYPos < 0 && entity.ModdedYPos - entity2.ModdedYPos < entity2.Depth + 1)
                    {
                        entity.RidingEntity = entity2;
                        break;
                    }
                }
            }
        }
    }

    private void UpdateEntitiesForces()
    {
        var player = StaticVariables.PlayerEntity;

        for (var i = 0; i < StaticVariables.g_activeEntityCount; i++)
        {
            var entity = StaticVariables.g_activeEntities[i];
            if (entity == player)
            {
                if (player.IsZForceApplied != 0)
                {
                    if ((player.Flags & 0x100) != 0
                        && (player.CombinedVramFlagsOR & 0x0010) != 0
                        && StaticVariables.g_gravityFlag <= 0)
                    {
                        player.ZForce = player.IsZForceApplied * 160;
                    }
                    else
                    {
                        player.ZForce = player.IsZForceApplied << 8;
                    }
                }
                else
                {
                    if ((player.Flags & 0x0100) != 0)
                    {
                        var force = player.ZForce - (CurrentMap.Info.Gravity << 8);
                        if (force < 0)
                        {
                            force = -force;//abs
                        }

                        var terminal = CurrentMap.Info.TerminalVelocity << 8;
                        if (terminal < force)
                        {
                            force = terminal;
                            if (force < 0)
                            {
                                force = -force;//abs
                            }
                        }
                        player.ZForce = force;
                    }
                }

                SetXyForces(player);

                int xforcestep, yforcestep;
                if ((player.CombinedVramFlagsOR & 0x0020) != 0)
                {
                    long resultx = player.XForceStep * 0x1000;
                    xforcestep = (int)(resultx >> 16);

                    long resulty = player.YForceStep * 0x1000;
                    yforcestep = (int)(resulty >> 16);
                }
                else
                {
                    xforcestep = player.XForceStep;
                    yforcestep = player.YForceStep;
                }

                int targetxforce, targetyforce;
                if ((player.CombinedVramFlagsOR & 0x0008) != 0
                    && StaticVariables.g_gravityFlag <= 0)
                {
                    long resultx = player.TargetXForce * 0x8000;
                    targetxforce = (int)(resultx >> 16);
                    long resulty = player.TargetYForce * 0x8000;
                    targetyforce = (int)(resulty >> 16);
                }
                else
                {
                    targetxforce = player.TargetXForce;
                    targetyforce = player.TargetYForce;
                }

                player.XForce = IncrementForce(player.XForce, targetxforce, xforcestep);
                player.YForce = IncrementForce(player.YForce, targetyforce, yforcestep);
            }
            else
            {
                if (entity.PlatformEntity != null)
                {
                    entity.ZForce = 0;
                    entity.YForce = 0;
                    entity.XForce = 0;
                    entity.AdjustedYForce = 0;
                    entity.AdjustedXForce = 0;
                    entity.FinalZForce = 0;
                    entity.FinalYForce = 0;
                    entity.FinalXForce = 0;
                }

                if (entity.IsZForceApplied != 0)
                {
                    if ((entity.IsZForceApplied & 0xffff) == 0x8000
                        && (entity.Flags & 0x0100) == 0)
                    {
                        entity.ZForce = entity.IsZForceApplied << 8;
                    }
                }

                if ((entity.Flags & 0x0100) != 0)
                {
                    //this applies gravity (limited by terminal velicity) to the z force
                    var force = entity.ZForce - (CurrentMap.Info.Gravity << 8);
                    if (force < 0)
                    {
                        force = -force;//abs
                    }

                    var terminal = CurrentMap.Info.TerminalVelocity << 8;
                    if (terminal < force)
                    {
                        force = terminal;
                        if (force < 0)
                        {
                            force = -force;//abs
                        }
                    }
                    entity.ZForce = force;
                }

                SetXyForces(entity);

                entity.XForce = IncrementForce(entity.XForce, entity.TargetXForce, entity.XForceStep);
                entity.YForce = IncrementForce(entity.YForce, entity.TargetYForce, entity.YForceStep);
            }

            SetAdjustedXyForces(entity);

            entity.FinalXForce = entity.AdjustedXForce;
            entity.FinalYForce = entity.AdjustedYForce;
            entity.FinalZForce = entity.ZForce;
        }
    }

    private void SetAdjustedXyForces(Entity entity)
    {
        var lastinteractx = entity.InteractXForce;
        var lastinteracty = entity.InteractYForce;
        entity.InteractYForce = 0;
        entity.InteractXForce = 0;
        var xval = entity.XForce + ScriptHelper.XForceTable[entity.TileAttributes & 0xf] >> CurrentMap.Info.Gravity;
        var yval = entity.YForce + ScriptHelper.YForceTable[entity.TileAttributes & 0xf] >> CurrentMap.Info.Gravity;

        xval += lastinteractx;
        yval += lastinteracty;

        if (xval + entity.XPos < entity.NegXMod)
        {
            xval = entity.NegXMod - entity.XPos;
            entity.ForceAdjusted = 1;
        }
        else if (xval + entity.XPos < entity.ScreenClipX)
        {
            xval = entity.ScreenClipX - entity.XPos;
            entity.ForceAdjusted = 1;
        }

        if (yval + entity.YPos < entity.NegYMod)
        {
            yval = entity.NegYMod - entity.YPos;
            entity.ForceAdjusted = 1;
        }
        else if (yval + entity.YPos < entity.ScreenClipY)
        {
            yval = entity.ScreenClipY - entity.YPos;
            entity.ForceAdjusted = 1;
        }


        entity.AdjustedXForce = xval;
        entity.AdjustedYForce = yval;
    }

    private int IncrementForce(int force, int targetforce, int step)
    {
        if (force == targetforce)
        {
            return force;
        }

        if (force < targetforce)
        {
            force += step;
        }
        else
        {
            force -= step;
        }

        if (force < targetforce)
        {
            return force;
        }

        return targetforce;
    }

    private void SetXyForces(Entity entity)
    {
        if (entity.Speed != entity.AnimSet.Speed
            || entity.TargetDirection != entity.CurrentDirection)
        {
            entity.Speed = entity.AnimSet.Speed;

            entity.TargetXForce = StaticVariables.g_offsetXList[entity.TargetDirection] * entity.AnimSet.Speed;

            entity.CurrentDirection = entity.TargetDirection;

            entity.TargetYForce = StaticVariables.g_offsetYList[entity.TargetDirection] * entity.AnimSet.Speed;
        }
        else if (entity.Acceleration == (entity.AnimSet.Acceleration & 0xf))
        {
            return;
        }

        entity.Acceleration = entity.AnimSet.Acceleration & 0xf;

        entity.XForceStep = Math.Abs(entity.TargetXForce - entity.XForce) >> entity.Acceleration;

        entity.YForceStep = Math.Abs(entity.TargetYForce - entity.YForce) >> entity.Acceleration;
    }

    private void UpdateVisibleEntitiesZSort()
    {
        if (StaticVariables.g_visibleEntityCount <= 0)
        {
            return;
        }

        for (var dex = 0; dex < StaticVariables.g_visibleEntityCount; dex++)
        {
            var entity = StaticVariables.g_visibleEntities[dex];
            entity.DepthSortVal = 0;
            entity.SortTop = entity.ModdedZPos + entity.Height;
        }

        for (var dex = 0; dex < StaticVariables.g_visibleEntityCount; dex++)
        {
            var entity = StaticVariables.g_visibleEntities[dex];
            if (entity.DepthSortVal == 0)
            {
                SetDepthSortVal(entity);
            }
        }

        for (var dex = 0; dex < StaticVariables.g_visibleEntityCount; dex++)
        {
            var entity = StaticVariables.g_visibleEntities[dex];
            entity.DepthSortVal = (int)(entity.DepthSortVal & 0xffff0000) + (entity.ZPos & 0xffff);
        }
    }

    private void SetDepthSortVal(Entity entity)
    {
        if (entity.DepthSortVal != 0)
        {
            return;
        }

        var sortval = entity.YPos + (entity.SpriteRef.NumImages << 16);
        if ((entity.Flags & 0x80) != 0
            || (entity.AnimFlags & 0x80) != 0)
        {
            entity.DepthSortVal = sortval;
            return;
        }

        if (entity.PlatformEntity != null)
        {
            if (entity.PlatformEntity.DepthSortVal == 0)
            {
                SetDepthSortVal(entity.PlatformEntity);
            }
            if (sortval < entity.PlatformEntity.DepthSortVal)
            {
                entity.DepthSortVal = entity.PlatformEntity.DepthSortVal;
                return;
            }
        }

        for (var dex = 0; dex < StaticVariables.g_collideableEntitiesCount; dex++)
        {
            var checkme = StaticVariables.g_collideableEntities[dex];
            if (checkme == entity)
            {
                continue;
            }

            if (checkme.SortTop >= entity.SortTop)
            {
                continue;
            }

            //X
            var x = checkme.XPos + checkme.XMod - entity.ModdedXPos;
            if (x >= 0)
            {
                if (x >= entity.Width + 1)
                {
                    continue;
                }
            }
            else
            {
                if (entity.ModdedXPos - (checkme.XPos + checkme.XMod) >= checkme.Width + 1)
                {
                    continue;
                }
            }

            //Y
            var y = checkme.YPos + checkme.YMod - entity.ModdedYPos;
            if (y >= 0)
            {
                if (y >= entity.Depth + 1)
                {
                    continue;
                }
            }
            else
            {
                if (entity.ModdedYPos - (checkme.YPos + checkme.YMod) >= checkme.Depth + 1)
                {
                    continue;
                }
            }

            if (checkme.DepthSortVal == 0)
            {
                SetDepthSortVal(checkme);
            }

            if (sortval < checkme.DepthSortVal)
            {
                sortval = checkme.DepthSortVal;
            }
        }

        entity.DepthSortVal = sortval;
    }

    private void UpdateBalanceRecords()
    {
        if (StaticVariables.g_activeEntityCount <= 0)
        {
            return;
        }

        for (var i = 0; i < StaticVariables.g_activeEntityCount; i++)
        {
            var entity = StaticVariables.g_activeEntities[i];

            if (entity.FrameCollision == null)
            {
                continue;
            }

            if (entity.BalanceVal == null)
            {
                continue;
            }

            if (entity.BalanceVal.Val == 0)
            {
                continue;
            }

            var flags = ((entity.Flags >> 2) & 1) | ((entity.Flags << 2) & 8);
            if ((entity.Flags & 0x1000) != 0)
            {
                flags |= 0x800;
            }

            if (flags == 0)
            {
                continue;
            }

            for (var j = 0; j < StaticVariables.g_activeEntityCount; j++)
            {
                var checkme = StaticVariables.g_activeEntities[j];
                if (checkme == entity)
                {
                    continue;
                }

                if (checkme.FrameColTickCounter != 0)
                {
                    continue;
                }

                if (checkme.DamagedTickCounter != 0)
                {
                    continue;
                }

                if ((checkme.AnimFlags & 0x40) != 0)
                {
                    continue;
                }

                if ((checkme.Flags & flags) == 0)
                {
                    continue;
                }

                //X
                var difx = entity.FrameX - checkme.ModdedXPos;
                int width;
                if (difx > 0)
                {
                    width = checkme.Width + 1;
                }
                else
                {
                    difx = checkme.ModdedXPos - entity.FrameX;
                    width = entity.FrameWidth + 1;
                }
                if (difx >= width)
                {
                    continue;
                }

                //Y
                var dify = entity.FrameY - checkme.ModdedYPos;
                int depth;
                if (dify > 0)
                {
                    depth = checkme.Depth + 1;
                }
                else
                {
                    dify = checkme.ModdedYPos - entity.FrameY;
                    depth = entity.FrameDepth + 1;
                }
                if (dify >= depth)
                {
                    continue;
                }

                //Z
                var difz = entity.FrameZ - checkme.ModdedZPos;
                int height;
                if (difz > 0)
                {
                    height = checkme.Height + 1;
                }
                else
                {
                    difz = checkme.ModdedZPos - entity.FrameZ;
                    height = entity.FrameHeight + 1;
                }
                if (difz >= height)
                {
                    continue;
                }

                /*if (game._1ac468 < 0
                    && (game._1ac46c & 0x800) != 0)
                {
                DEBUG THING
                }*/
                var valdex = entity.BalanceVal.Val & 0xf;
                var val = checkme.BalanceRecord.Vals[valdex];

                if ((val & 0xc0) != 0x80)
                {
                    if (valdex == 6 || valdex == 0xa)
                    {
                        CreateEffect_Type1(0, 4, 0, checkme, width, 0, 0, 0);
                    }
                    if (valdex == 7 || valdex == 9)
                    {
                        CreateEffect_Type1(0, 5, 0, checkme, 1, 0, 0, 0);
                    }
                    checkme.TouchingEntity = entity;
                }

                checkme.FrameColTickCounter = 0x19;

                entity.HitCounter++;
                //X
                var xr = checkme.ModdedXPos + checkme.Width;
                if (entity.FrameX + entity.FrameWidth < xr)
                {
                    xr = entity.FrameX + entity.FrameWidth;
                }

                var xl = entity.FrameX;
                if (entity.FrameX < checkme.ModdedXPos)
                {
                    xl = checkme.ModdedXPos;
                }

                //Y
                var yr = checkme.ModdedYPos + checkme.Depth;
                if (entity.FrameY + entity.FrameDepth < yr)
                {
                    yr = entity.FrameY + entity.FrameDepth;
                }

                var yl = entity.FrameY;
                if (entity.FrameY < checkme.ModdedYPos)
                {
                    yl = checkme.ModdedYPos;
                }

                //Z
                var zr = checkme.ModdedZPos + checkme.Height;
                if (entity.FrameZ + entity.FrameHeight < zr)
                {
                    zr = entity.FrameZ + entity.FrameHeight;
                }

                var zl = entity.FrameZ;
                if (entity.FrameZ < checkme.ModdedZPos)
                {
                    zl = checkme.ModdedZPos;
                }

                var x = (xl + xr) / 2;
                var y = (yl + yr) / 2;
                var z = (zl + zr) / 2;
                CreateRandomPoofs(x, y, z);
            }
        }
    }

    private void CreateRandomPoofs(int x, int y, int z)
    {
        //creates two poofs moving away from the impact at random speed and direction
        for (var dex = 0; dex < 2; dex++)
        {
            var effect = CreateEffect_Type0(0, 9, 0, x, y, z);
            if (effect == null)
            {
                continue;
            }

            var baseforce = (int)(0xffff << 16);

            var i = StaticVariables.g_gameRandomSeed;
            var val1 = (uint)(i * 0x7d2b89dd);
            var val2 = (uint)(0xe06a02e7 + val1);
            var targetval = (int)(((long)val2 * 0x20001) >> 32);
            StaticVariables.g_gameRandomSeed = val2;

            effect.XForce = targetval + baseforce;

            i = StaticVariables.g_gameRandomSeed;
            val1 = (uint)(i * 0x7d2b89dd);
            val2 = (uint)(0xe06a02e7 + val1);
            targetval = (int)(((long)val2 * 0x20001) >> 32);
            StaticVariables.g_gameRandomSeed = val2;

            effect.YForce = targetval + baseforce;

            i = StaticVariables.g_gameRandomSeed;
            val1 = (uint)(i * 0x7d2b89dd);
            val2 = (uint)(0xe06a02e7 + val1);
            targetval = (int)(((long)val2 * 0x20001) >> 32);
            StaticVariables.g_gameRandomSeed = val2;

            effect.ZForce = targetval + baseforce;
        }
    }

    private void UpdateActiveEffects()
    {
        if (StaticVariables.g_numberOfEntity < 0)
        {
            return;
        }

        for (var i = 0; i < StaticVariables.g_numberOfEntity; i++)
        {
            var entity = StaticVariables.g_entitySlots[i];
            if (entity.Status - 2 >= 2 || (entity.DamagedTickCounter & 3) == 3)
            {
                if (entity.ActiveEffect != null)
                {
                    entity.ActiveEffect.Status = 0;
                    entity.ActiveEffect = null;
                }
                continue;
            }
            var effect = entity.ActiveEffect;
            if (effect == null)
            {
                effect = CreateEffect_Type3(0, 0, 0, entity, -1, 0, 0, 0);
                if (effect == null)
                {
                    continue;
                }

                entity.ActiveEffect = effect;
            }

            if (((entity.Flags >> 16) & 7) == 0)
            {
                continue;
            }

            if ((entity.AnimFlags & 0x10) != 0)
            {
                continue;
            }

            if (entity.PlatformEntity != null)
            {
                effect.Status = 1;
                continue;
            }
            //TODO: what is 18c, something with slope and sliding?
            var animid = -1;
            if ((entity.Slope_18c == 4 || entity.Slope_190 == 4)
                && entity.Slope_18c != entity.Slope_190)
            {
                CreateEffect_Type0(0, 6, 0, entity.XPos, entity.YPos, entity.CollidedWithEntityZ);
            }

            if (entity.Slope_18c >= 8)
            {
                continue;
            }

            switch (entity.Slope_18c)
            {
                case 1:
                case 2:
                    effect.TargetIsMapSprite = 0;
                    effect.TargetSpriteTableIndex = 1;
                    effect.TargetAnimation = (byte)animid;
                    effect.Status = 2;
                    effect.X = entity.XPos;
                    effect.Y = entity.YPos;
                    effect.Z = entity.CollidedWithEntityZ;
                    continue;
                case 4:
                    effect.Status = 1;
                    if ((entity.UnknownCounter & 7) != 0)
                    {
                        continue;
                    }

                    if ((entity.XForce | entity.YForce) == 0)
                    {
                        continue;
                    }

                    CreateEffect_Type0(0, 0x15, 0, entity.XPos, entity.YPos, entity.CollidedWithEntityZ);
                    continue;
                case 3:
                    if ((entity.UnknownCounter & 0x7) != 0)
                    {
                        break;
                    }

                    if ((entity.XForce | entity.YForce) == 0)
                    {
                        break;
                    }

                    CreateEffect_Type0(0, CurrentMap.Info.SlideEffectId, 0, entity.XPos, entity.YPos, entity.CollidedWithEntityZ);
                    break;
                default:
                    break;
            }

            animid -= (entity.ZPos - entity.CollidedWithEntityZ) >> 20;

            if (animid >= 6)
            {
                animid = 5;
            }
            else if (animid < 0)
            {
                animid = 0;
            }

            effect.TargetIsMapSprite = 0;
            effect.TargetSpriteTableIndex = 0;

            effect.TargetAnimation = (byte)animid;
            effect.Status = 2;

            effect.X = entity.XPos;
            effect.Y = entity.YPos;
            effect.Z = entity.CollidedWithEntityZ;
        }
    }

    private void UpdateEntitiesAnimation()
    {
        for (var i = 0; i < StaticVariables.g_activeEntityCount; i++)
        {
            var entity = StaticVariables.g_entitySlots[i];
            UpdateAnimation(entity);
        }
    }

    private void MovePlayer()
    {
        //TODO: implement
        //this function is MASSIVE
        //perhaps the largest in the entire game
    }

    private void UpdateEntitiesEvents()
    {
        MovePlayer();

        if (StaticVariables.g_numberOfEntity > 0)
        {
            for (var i = 1; i < StaticVariables.g_numberOfEntity; i++)
            {
                var entity = StaticVariables.g_entitySlots[i];
                var eventType = -1;
                if (entity.PlatformEntity == null && entity.Status < 5)
                {
                    switch (entity.Status)
                    {
                        case 1://loading/activating
                            eventType = ScriptHelper.ProgramALoad;
                            entity.Status = 2;
                            break;
                        case 2://normal

                            if ((entity.Flags & 0x100000) != 0)
                            {
                                if (entity.Slope_18c == 4)
                                {
                                    DestroyEntity(entity, 6);

                                    eventType = -1;
                                    break;
                                }
                            }

                            if ((entity.Flags & 0x200000) != 0)
                            {
                                if ((entity.CombinedVramFlagsOR & 0x8004) != 0)
                                {
                                    DestroyEntity(entity, -1);

                                    eventType = -1;
                                    break;
                                }
                            }

                            if ((entity.Flags & 0x10) != 0)
                            {
                                if (entity.ForceAdjusted != 0 || entity.IsAboveGround != 3)
                                {
                                    entity.Status = 3;//deactivate
                                    eventType = ScriptHelper.ProgramEDeactivate;
                                    break;
                                }
                            }

                            if ((entity.Flags & 0x20) != 0)
                            {
                                if (entity.HitCounter != 0)
                                {
                                    entity.Status = 3;//decativate
                                    eventType = ScriptHelper.ProgramEDeactivate;
                                    break;
                                }
                            }

                            if ((entity.Flags & 0x40) != 0)
                            {
                                if (entity.ForceResetAnimationFlag != 0)
                                {
                                    entity.Status = 3;//decativate
                                    eventType = ScriptHelper.ProgramEDeactivate;
                                    break;
                                }
                            }

                            if (entity.TouchingEntity != null)
                            {
                                eventType = ScriptHelper.ProgramDTouch;
                                break;
                            }

                            //i think this gamevar is more than just activecollitionentity, 
                            //the interact button probabaly has to be down for this to be set
                            if (StaticVariables.g_activeCollisionEntity != entity)
                            {
                                eventType = 2;
                                break;
                            }
                            //if it gets here it means the player is interacting with this entity

                            if (entity.SpriteProgramIndexes[ScriptHelper.ProgramFInteract] != 0)
                            {
                                eventType = 5;
                                break;
                            }

                            if (entity.ProgramIndexes[ScriptHelper.ProgramFInteract] != 0)
                            {
                                eventType = 5;
                                break;
                            }
                            eventType = 2;
                            break;
                        case 3://deactivating
                            eventType = ScriptHelper.ProgramEDeactivate;
                            break;
                        case 0:
                        case 4:
                            eventType = -1;
                            break;
                    }
                }
                entity.EventTrigger = eventType;
            }
        }

        //run events
        bool keepGoing;

        do
        {
            keepGoing = false;
            if (StaticVariables.g_numberOfEntity > 0)
            {
                //foreach entity besides player
                for (var i = 1; i < StaticVariables.g_numberOfEntity; i++)
                {
                    var entity = StaticVariables.g_entitySlots[i];

                    if (entity.EventTrigger != -1)
                    {
                        var programIndex = entity.ProgramIndexes[entity.EventTrigger] & 0x7f;

                        if (programIndex != 0)
                        {
                            //run the eventhandler script
                            _entityEventHandlers.RunEntityEventScripts(entity, entity.EventTrigger);
                            entity.EventTrigger = -1;
                        }
                        else
                        {
                            var eventId = entity.SpriteProgramIndexes[entity.EventTrigger];
                            //run the sprite event handler
                            _entityEventHandlers.SpriteHandlers.RunSpriteHandler(entity.EventTrigger, eventId, entity);
                            entity.EventTrigger = -1;
                        }

                        keepGoing = true;
                    }
                }
            }

        } while (keepGoing);
    }

    private void UpdateEntitiesCounters()
    {
        if (StaticVariables.g_numberOfEntity >= 0)
        {
            for (var i = 0; i <= StaticVariables.g_numberOfEntity; i++)
            {
                var entity = StaticVariables.g_entitySlots[i];
                entity.UnknownCounter++;
                if (entity.DamagedTickCounter != 0)
                {
                    entity.DamagedTickCounter--;
                }

                if (entity.FrameColTickCounter != 0)
                {
                    entity.FrameColTickCounter--;
                }
            }
        }

        //displays debug records here

    }

    private void UpdateDestroyedEntities()
    {
        var max = 0;
        for (var i = 0; i < StaticVariables.g_entitySlots.Length; i++)
        {
            var entity = StaticVariables.g_entitySlots[i];

            if (entity.Status == 4)
            {
                //zero out the properties
                entity = new Entity();
                entity.EntityRefId = -1;
                StaticVariables.g_entitySlots[i] = entity;
                //entity.Index = i;
            }

            if (entity.Status != 0)
            {
                max = i + 1;
            }
        }

        StaticVariables.g_numberOfEntity = max;
    }

    private void UpdateEntityLists()
    {
        StaticVariables.g_activeEntityCount = 0;
        StaticVariables.g_collideableEntitiesCount = 0;
        StaticVariables.g_visibleEntityCount = 0;

        if (StaticVariables.g_numberOfEntity < 0)
        {
            return;
        }

        for (int i = 0; i < StaticVariables.g_numberOfEntity; i++)
        {
            var entity = StaticVariables.g_entitySlots[i];

            //processable
            if (entity.Status - 2 < 2 && entity.PlatformEntity == null)
            {
                StaticVariables.g_activeEntities[StaticVariables.g_activeEntityCount++] = entity;
            }

            //collidable
            if ((entity.Flags & 0x80) != 0 && (entity.AnimFlags & 0x80) == 0 && entity.PlatformEntity == null)
            {
                StaticVariables.g_collideableEntities[StaticVariables.g_collideableEntitiesCount++] = entity;
            }

            //renderable
            if (entity.Status - 2 < 2 && (entity.DamagedTickCounter & 3) != 3)//flicker effect, every 3rd frame when being damaged
            {
                StaticVariables.g_visibleEntities[StaticVariables.g_visibleEntityCount++] = entity;
            }
        }
    }

    public void TriggerWarp(Entity entity)
    {
        SpawnEntityContents(entity);

        entity.Status = 4;
        entity.EventTrigger = -1;

        if (entity.ActiveEffect != null)
        {
            entity.ActiveEffect.CurrentSpriteTableIndex = 0; //.spriteTableIndex = 0;
            entity.ActiveEffect = null;
        }

        if (entity.PlatformEntity != null)
        {
            entity.PlatformEntity.ActionState = 0;
        }
    }

    public void DestroyEntity(Entity entity, int effectid)
    {
        SpawnEntityContents(entity);

        entity.Status = 4;
        entity.EventTrigger = -1;
        if (entity.ActiveEffect != null)
        {
            entity.ActiveEffect.Status = 0;
            entity.ActiveEffect.Status = 0;
            //.spriteTableIndex = 0;
            entity.ActiveEffect = null;
        }

        if (effectid == -1)
        {
            effectid = entity.Sprite.Header.BreakEffect;
        }

        if (effectid != 0)
        {
            CreateEffect_Type1(0, (byte)effectid, 0, entity, 1, 0, 0, 0);
        }

        if (entity.PlatformEntity != null)
        {
            //TODO: figure out what 2c is
            entity.PlatformEntity.ActionState = 0;
        }
    }

    private int SpawnEntityContents(Entity entity)
    {
        if (entity.ContentsItemId == 0)
        {
            return 0;
        }

        if (!CheckItemId(entity.ContentsItemId))
        {
            return 0;
        }

        //SpawnEntity(Entity parent, int initDataIndex, int checkSpawnZone)
        //SpawnEntity(Entity ownerEntity, bool ismapsprite, uint tableindex, int xpos, int ypos, int zpos, uint dir)
        var child = SpawnEntity(null, false, entity.ContentsItemId + 0x1e,
            entity.XPos, entity.YPos, entity.ZPos, 0);

        if (child == null)
        {
            return 0;
        }

        child.ZForce = 0xa0000;
        child.Bytes[0] = 1;

        child.Flags &= 0xff7f;

        //TODO: implement this lookuptable
        /*int result = lookuptable[entity.ContentsItemId * 8];*/

        //if (result == 0)
        //    result = -1;

        //child.InitialXPos = result;

        child.InitialYPos = 0;

        child.Bytes.Set(0xa0000);

        //TODO sfx
        //PlaySoundEffect(0x54);
        child.AIValues.Set(entity.ContentsGameFlag);
        return 1;
    }

    private bool CheckItemId(uint itemid)
    {
        if (itemid != 0x26)
        {
            if (itemid - 0x51 >= 2)
            {
                return false;
            }
        }
        var ret = GetSomething();
        return ret < 1 ? true : false;
    }

    private int GetSomething()//0x4f380
    {
        //TODO: what is this actually checking?
        //ptr = *0x119888
        //return (short)ptr[6]
        return 0;
    }

    private MapEffectRecord GetMapEffectRecord(int id, bool checkBoundingBox)
    {
        if (id < CurrentMap.SpriteInfo.MapEffectRecords.Length)
        {
            var record = CurrentMap.SpriteInfo.MapEffectRecords[id];
            if (checkBoundingBox)
            {
                var playerEntity = StaticVariables.PlayerEntity;
                if (playerEntity.TileX < record.X1 || playerEntity.TileX > record.X2
                                        || playerEntity.TileY < record.Y1 || playerEntity.TileY > record.Y2)
                {
                    return null;
                }
            }

            return record;
        }
        return null;
    }

    private SpriteEffect CreateEffect_Type0(byte ismapeffect, byte effectid, byte animid, int x, int y, int z)
    {
        var effect = GetNextAvailableEffect();

        if (effect != null)
        {
            InitEffect(effect, null, -1, 0, ismapeffect, effectid, animid, x, y, z);
            return effect;
        }
        return null;
    }


    private SpriteEffect CreateEffect_Type1(byte ismapeffect, byte effectid, byte animid, Entity entity, int depthsortmod, int xoff, int yoff, int zoff)
    {
        var effect = GetNextAvailableEffect();

        if (effect != null)
        {
            InitEffect(effect, null, -1, 1, ismapeffect, effectid, animid, entity.XPos, entity.YPos, entity.ZPos);
            effect.AttachedEntity = entity;
            effect.DepthSortMod = depthsortmod;
            effect.XOff = xoff;
            effect.YOff = yoff;
            effect.ZOff = zoff;
            return effect;
        }
        return null;
    }

    private SpriteEffect CreateEffect_Type3(byte ismapeffect, byte effectid, byte animid, Entity entity, int depthsortmod, int x, int y, int z)
    {
        var effect = GetNextAvailableEffect();

        if (effect != null)
        {
            InitEffect(effect, null, -1, 3, ismapeffect, effectid, animid, x, y, z);
            effect.AttachedEntity = entity;
            effect.DepthSortMod = depthsortmod;
            return effect;
        }
        return null;
    }

    public SpriteEffect CreateEffect_MapType(byte mapeffectid, bool checkBoundingbox)
    {
        var record = GetMapEffectRecord(mapeffectid, checkBoundingbox);
        if (record != null)
        {
            if (!checkBoundingbox && (record.Flags & 0x40) == 0)
            {
                return null;
            }

            var effect = GetNextAvailableEffect();

            if (effect != null)
            {
                InitEffect(effect, record, mapeffectid, 0,
                    (byte)((record.Flags & 0x80) >> 7), record.EffectId, record.AnimId,
                    (record.X * 12 + 12) << 16, (record.Y * 8 + 8) << 16, record.Z << 19);

                return effect;
            }
        }
        return null;
    }


    public SpriteEffect GetNextAvailableEffect()
    {
        foreach (var effect in StaticVariables.g_effectSlots)
        {
            if (effect.Status == 0)
            {
                return effect;
            }
        }
        return null;
    }

    public void InitEffect(SpriteEffect effect, MapEffectRecord mapEffectRecord, int mapeffectid, int effecttype, byte ismapeffect, byte effectid, byte animid, int x, int y, int z)
    {
        //initialize
        effect.MapEffectRecord = null;
        effect.SpriteEffectRecord = null;
        effect.SpriteRef = new SpriteRef();
        effect.SheetSize = 0;
        effect.PaletteIndex = 0;
        effect.MapEffectId = 0;
        effect.UpdateMode = 0;
        effect.AttachedEntity = null;
        effect.X = 0;
        effect.Y = 0;
        effect.Z = 0;
        effect.XOff = 0;
        effect.YOff = 0;
        effect.ZOff = 0;
        effect.XForce = 0;
        effect.YForce = 0;
        effect.ZForce = 0;
        effect.DepthSortMod = 0;
        effect.DepthSortVal = 0;
        effect.Status = 0;
        effect.TargetIsMapSprite = 0;
        effect.CurrentIsMapSprite = 0;
        effect.TargetSpriteTableIndex = 0;
        effect.CurrentSpriteTableIndex = 0;
        effect.TargetAnimation = 0;
        effect.CurrentAnimation = 0;
        effect.Frame = null;
        effect.InitialFrame = null;
        effect.Delay = 0;
        effect.DestroyFlag = 0;

        effect.AnimIndex = 0;


        effect.MapEffectRecord = mapEffectRecord;
        if (mapEffectRecord != null)
        {
            effect.MapEffectId = mapeffectid;
        }
        else
        {
            effect.MapEffectId = -1;
        }

        effect.Status = 2;
        effect.CurrentSpriteTableIndex = (byte)~effectid;
        effect.TargetIsMapSprite = ismapeffect;
        effect.CurrentIsMapSprite = (byte)~ismapeffect;
        effect.TargetSpriteTableIndex = effectid;
        effect.TargetAnimation = animid;
        effect.CurrentAnimation = (byte)~animid;
        effect.X = x;
        effect.Y = y;
        effect.Z = z;
    }



    //TODO all the slope stuff
    public void UpdateTile(Entity entity)
    {
        //set of variables set by certain special frames of animation
        if (entity.FrameCollision != null)
        {
            entity.FrameX = entity.XPos + entity.FrameXOff;
            entity.FrameY = entity.YPos + entity.FrameYOff;
            entity.FrameZ = entity.ZPos + entity.FrameZOff;
        }

        entity.TileZ = entity.ZPos >> 20; //(z >> 16) / 16
        entity.TileX = (entity.XPos >> 16) / 24;
        entity.TileY = entity.YPos >> 20;

        var hitz = CollideOnEntitiesZ(entity);
        int tohit;
        entity.ZEntityCollision = hitz;
        entity.CollidedWithEntityZ = hitz < entity.ZPos ? 0 : 1;
        if ((entity.Flags & 0x100) != 0)
        {
            tohit = 0xe00;
            var somevals = new int[4];
            for (var dex = 0; dex < 4; dex++)
            {
                var tl = entity.MapTiles[dex];
                var fullval = tl.Walkability | tl.GroundProperty << 8 | tl.Slope << 16 | tl.Height << 24;
                if (entity.MapHeights[dex] + 1 == entity.ModdedZPos)
                {

                    //var val = (tl.groundproperty & 0xe) << 8;
                    if ((fullval & 0xe00) < tohit)
                    {
                        somevals[dex] = fullval;
                        tohit = fullval & 0xe00;
                    }
                }
                else
                {
                    somevals[dex] = 0;
                    tohit = 0;
                }
            }

            entity.CombinedVramFlagsOR = somevals[0] | somevals[1] | somevals[2] | somevals[3];
            entity.CombinedVramFlagsAND = somevals[0] & somevals[1] & somevals[2] & somevals[3];

            var tilex = entity.TileX;

            if (tilex > 0)
            {
                if (tilex >= 0x34)
                {
                    tilex = 0x33;
                }
            }
            else
            {
                tilex = 0;
            }
            var tiley = entity.TileY;
            if (tiley > 0)
            {
                if (tiley >= 0x3c)
                {
                    tiley = 0x3b;
                }
            }
            else
            {
                tiley = 0;
            }

            var tile = CurrentMap.Map.MapTiles[tilex + tiley * 52];
            var fullval2 = tile.Walkability | tile.GroundProperty << 8 | tile.Slope << 16 | tile.Height << 24;
            var height = (int)(fullval2 & 0xff000000 >> 4) + 1;
            var r3 = height ^ entity.ModdedZPos;
        }
        else
        {
            tohit = 0;
            entity.CombinedVramFlagsOR = 0;
            entity.CombinedVramFlagsAND = 0;
        }

        //all that slope code is for setting this value
        entity.TileAttributes = 0;

        var prevtohit = entity.Slope_18c;
        entity.Slope_18c = tohit;
        entity.Slope_190 = prevtohit;
    }

    public int CollideOnEntitiesZ(Entity entity)
    {
        var collision = entity.FloorHeight + 1;
        if ((entity.Flags & 0x80) == 0)
        {
            return collision;
        }

        if ((entity.AnimFlags & 0x80) != 0)
        {
            return collision;
        }

        if (entity.PlatformEntity != null)
        {
            return collision;
        }

        if (StaticVariables.g_collideableEntitiesCount <= 0)
        {
            return collision;
        }

        for (var dex = 0; dex < StaticVariables.g_collideableEntitiesCount; dex++)
        {
            var checkme = StaticVariables.g_collideableEntities[dex];

            if (checkme == entity)
            {
                continue;
            }

            if (checkme.ModdedZPos + checkme.Height >= entity.ModdedZPos
                || checkme.ModdedZPos + checkme.Height < collision)
            {
                continue;
            }

            if (checkme.ModdedXPos - entity.ModdedXPos >= 0)
            {
                if (checkme.ModdedXPos - entity.ModdedXPos >= entity.Width + 1)
                {
                    continue;
                }
            }
            else
            {
                if (entity.ModdedXPos - checkme.ModdedXPos >= checkme.Width + 1)
                {
                    continue;
                }
            }

            if (checkme.ModdedYPos - entity.ModdedYPos >= 0)
            {
                if (checkme.ModdedYPos - entity.ModdedYPos < entity.Depth + 1)
                {
                    collision = checkme.ModdedZPos + checkme.Height;
                }
            }
            else
            {
                if (entity.ModdedYPos - checkme.ModdedYPos < checkme.Depth + 1)
                {
                    collision = checkme.ModdedZPos + checkme.Height;
                }
            }

        }
        return collision;
    }


    public int GetEntityFromRefId(Entity ownerEntity, int entityid)
    {
        var numgot = 0;
        if ((entityid & 0x80) == 0)
        {
            CheckValidEntityId(entityid);//calls getinitrecord which is a 20 byte datarecord SIEntityRecord
            foreach (var entity in StaticVariables.g_entitySlots)
            {
                if ((ownerEntity.Status - 1 < 2 || ownerEntity.Status == 3) && entity.EntityRefId == entityid)
                {
                    StaticVariables.g_entitySlots[numgot++] = entity;
                }
            }
            return numgot;
        }

        var functionid = entityid & 0x7f;
        switch (functionid)
        {
            case 0://get owner
                StaticVariables.g_entitySlots[numgot++] = ownerEntity;
                return numgot;
            case 1://get player
                StaticVariables.g_entitySlots[numgot++] = StaticVariables.PlayerEntity;
                return numgot;
            case 2://get all entities
                foreach (var entity in StaticVariables.g_entitySlots)
                {
                    if (entity.Status - 1 < 2 || entity.Status == 3)
                    {
                        StaticVariables.g_entitySlots[numgot++] = entity;
                    }
                }
                return numgot;
            case 3://get all entities except player
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if (entity.Status - 1 < 2 || entity.Status == 3)
                    {
                        StaticVariables.g_entitySlots[numgot++] = entity;
                    }
                }
                return numgot;
            case 4://all entities on the ground
                foreach (var entity in StaticVariables.g_entitySlots)
                {
                    if ((ownerEntity.Status - 1 < 2 || ownerEntity.Status == 3)
                        && (entity.Flags & 0x80) != 0
                        && (entity.AnimFlags & 0x80) == 0
                        && entity.PlatformEntity == null)
                    {
                        StaticVariables.g_entitySlots[numgot++] = entity;
                    }
                }
                return numgot;
            case 5://all entities besides player that the ownerentity is riding on
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && ownerEntity.RidingEntity == entity)
                    {
                        StaticVariables.g_entitySlots[numgot++] = entity;
                    }

                }
                return numgot;
            case 6://all entities besides player that are riding on the ownerentity
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && entity.RidingEntity == ownerEntity)
                    {
                        StaticVariables.g_entitySlots[numgot++] = entity;
                    }

                }
                return numgot;
            case 7://all entities besides player where ownerentity.xcollision? == entity
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && ownerEntity.XCollisionEntity == entity)
                    {
                        StaticVariables.g_entitySlots[numgot++] = entity;
                    }

                }
                return numgot;
            case 8://all entities besides player where entity.xcollision? == ownerentity
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && entity.XCollisionEntity == ownerEntity)
                    {
                        StaticVariables.g_entitySlots[numgot++] = entity;
                    }

                }
                return numgot;
            case 9://all entities besides player where entity.ownerentity [c] == ownerentity
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && entity.ParentEntity == ownerEntity)
                    {
                        StaticVariables.g_entitySlots[numgot++] = entity;
                    }

                }
                return numgot;
            case 10://all entities besides player where ownerentity.ownerentity [c] == entity
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && ownerEntity.ParentEntity == entity)
                    {
                        StaticVariables.g_entitySlots[numgot++] = entity;
                    }

                }
                return numgot;
            case 11://all entities besides player that are on a platform
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && entity.PlatformEntity != null)
                    {
                        StaticVariables.g_entitySlots[numgot++] = entity;
                    }

                }
                return numgot;
        }

        return numgot;
    }

    private SiEntityRecord CheckValidEntityId(int entityId)
    {
        var rec = GetInitData(entityId);
        return rec;
    }

    public Entity ActivateEntity(Entity ownerEntity, int entityId, int forceActivate)
    {
        var data = GetInitData(entityId);

        if (data == null)
        {
            return null;
        }

        if (forceActivate == 0)
        {
            //if player is outside of the activation zone dont activate
            //  this is used when a map has multiple rooms, the activate zone is set to the room where
            //  the entity is, if the player loads in a different room then the entity wont activate
            if (StaticVariables.PlayerEntity.TileX < data.XMin)
            {
                return null;
            }

            if (data.XMax < StaticVariables.PlayerEntity.TileX)
            {
                return null;
            }

            if (StaticVariables.PlayerEntity.TileY < data.YMin)
            {
                return null;
            }

            if (data.YMax < StaticVariables.PlayerEntity.TileY)
            {
                return null;
            }
        }

        if ((data.SpriteDirection & 0x40) == 0 && forceActivate == 0)
        {
            return null;
        }

        int addedtosheet, addedtopalette;
        var isMapSprite = (data.SpriteDirection & 0x80) != 0;
        var sprite = GetSpriteFromSpriteTable(isMapSprite, data.SpriteTableIndex, out addedtosheet, out addedtopalette);

        if (sprite == null)
        {
            return null;
        }

        var entity = AllocateEntitySlot();

        if (entity == null)
        {
            return null;
        }

        var x = (data.XPos * 12 + 12) << 16;
        var y = (data.XPos * 8 + 8) << 16;
        var z = data.Height << 19;

        var directionTable = new uint[] { 0x00, 0x10, 0x08, 0x18 };
        var dir = directionTable[data.SpriteDirection & 0x3];

        var spriteTable = (uint)data.SpriteTableIndex;
        if ((data.SpriteDirection & 0x80) != 0)
        {
            spriteTable += 0x100;
        }

        InitializeEntity(entity, ownerEntity, sprite, data, spriteTable, entityId, x, y, z, 0, dir, addedtosheet, addedtopalette);

        return entity;
    }

    public SiEntityRecord GetInitData(int entityId)
    {
        SiEntityRecord res;

        if (entityId < 0 || CurrentMap.SpriteInfo.Entities.Entities.Length <= entityId) // StaticVariables.g_maxInitData
        {
            throw new Exception("Illegal character initial data!!");
            res = null;
        }
        else
        {
            res = CurrentMap.SpriteInfo.Entities.Entities[entityId];// * 0x14;

            if (CurrentMap.SpriteInfo.Entities.Entities[entityId + 1] == null)
            {
                res = null;
            }
        }
        return res;
    }

    public Entity SpawnWarpEntity(Entity parentEntity, int entityType, uint subtype, int posX, int posY, int posZ, uint direction)
    {
        SpriteRecord spriteRecord;
        Entity entityResult;
        Entity entity;
        int paletteIndex;
        int sheetSize;

        spriteRecord = GetSpriteFromSpriteTable(entityType == 1, subtype, out paletteIndex, out sheetSize);

        entity = null;

        if (spriteRecord != null)
        {
            entityResult = AllocateEntitySlot();
            entity = null;

            if (entityResult != null)
            {
                if (entityType != 0)
                {
                    subtype = subtype + 0x100;
                }

                InitializeEntity(entityResult, parentEntity, spriteRecord, null, subtype, -1,
                    posX, posY, posZ, 0, direction, paletteIndex, sheetSize);

                entity = entityResult;
            }
        }

        return entity;
    }

    public SpriteEffect CreateEffectEntity(int behaviorFlags, byte spriteTableIndex, byte animationIndex, int x, int y, int z)
    {
        SpriteEffect effect = GetFreeEffect();

        if (effect != null)
        {
            InitEffectEntity(effect, null, -1, 0, behaviorFlags, spriteTableIndex, animationIndex, x, y, z);
        }

        return effect;
    }

    private SpriteEffect GetFreeEffect()
    {
        SpriteEffect slot;
        int i = 0;

        do
        {
            slot = StaticVariables.g_effectSlots[i];

            if (slot.Status == 0)
            {
                return slot;
            }

            i = i + 1;
        }
        while (i < 0x80);

        return null;
    }

    private void InitEffectEntity(SpriteEffect effect, MapEffectRecord mapEffectRecord, int effectId, int updateMode,
        int behaviorFlag, byte spriteTableIndex, byte animationIndex, int x, int y, int z)
    {
        MapEffectRecord pMVar1;
        SpriteEffectRecord pSVar2;
        int[] piVar3;
        int[][] baseTemplate;
        SpriteEffect pEffect;
        int originalId;

        //baseTemplate = StaticVariables.DAT_8013c608;
        originalId = effect.Id;
        //pEffect = effect;
        //
        //do
        //{
        //    pMVar1 = (MapEffectRecord)(object)baseTemplate[1];
        //    pSVar2 = (SpriteEffectRecord)(object)baseTemplate[2];
        //    piVar3 = baseTemplate[3];
        //    pEffect.Id = baseTemplate[0][0];
        //    pEffect.MapEffectRecord = pMVar1;
        //    pEffect.SpriteEffectRecord = pSVar2;
        //    pEffect.SpriteRef.Images = piVar3;
        //    baseTemplate = baseTemplate.Skip(4).ToArray();
        //    pEffect = (SpriteEffect)(object)pEffect.SpriteRef.X;
        //}
        //while (!ReferenceEquals(baseTemplate, StaticVariables.g_monitorBase));

        effect.Id = originalId;
        effect.MapEffectRecord = mapEffectRecord;

        if (mapEffectRecord == null)
        {
            effect.MapEffectId = -1;
        }
        else
        {
            effect.MapEffectId = effectId;
        }

        effect.UpdateMode = updateMode;
        effect.Status = 2;
        effect.TargetSpriteTableIndex = spriteTableIndex;
        effect.TargetAnimation = animationIndex;
        effect.TargetIsMapSprite = (byte)(behaviorFlag != 0 ? 1 : 0);
        effect.CurrentIsMapSprite = (byte)(behaviorFlag == 0 ? 1 : 0);
        effect.CurrentSpriteTableIndex = (byte)~spriteTableIndex;
        effect.CurrentAnimation = (byte)~animationIndex;
        effect.X = x;
        effect.Y = y;
        effect.Z = z;
    }


    #region Entity

    public void HideEntity(Entity entity)
    {
        entity.Status = 4;
        entity.EventTrigger = -1;
        if (entity.ActiveEffect != null)
        {
            entity.ActiveEffect.Status = 0;
            entity.ActiveEffect = null;
        }
        if (entity.PlatformEntity != null)
        {
            entity.PlatformEntity.ActionState = 0;
        }
    }

    public uint TurnEntity(Entity entity, int turnCode)
    {
        var turndir = turnCode & 0x1f;
        var turntype = turnCode >> 5;
        if (turntype >= 8)
        {
            return 0;
        }

        switch (turntype)
        {
            case 1:
                return (uint)((entity.TargetDirection + turndir) & 0x1f);
            case 2:
                return (uint)ScriptHelper.CardinalDirTable[turndir & 0x3];
            case 3:
                var dfv = ScriptHelper.GetDirectionToTarget(StaticVariables.PlayerEntity.XPos - entity.XPos, StaticVariables.PlayerEntity.YPos - entity.YPos);
                return (uint)((dfv + turndir) & 0x1f);
            case 4:
                {
                    var i = StaticVariables.g_gameRandomSeed;
                    var val1 = (int)(i * 0x7d2b89dd);
                    var val2 = (int)(0xe06a02e7 + val1);
                    var val3 = (int)(((long)val2 * 4) >> 32);
                    StaticVariables.g_gameRandomSeed = (uint)val2;
                    var dir = ScriptHelper.CardinalDirTable[val3];//val3 here is a number between 0 and 3
                    return (uint)dir;
                }
            case 5:
                {
                    var i = StaticVariables.g_gameRandomSeed;
                    var val1 = (int)(i * 0x7d2b89dd);
                    var val2 = (int)(0xe06a02e7 + val1);
                    var val3 = (int)(((long)val2 * 0x20) >> 32);
                    StaticVariables.g_gameRandomSeed = (uint)val2;
                    return (uint)val2;
                }
            case 6:
                return (uint)((StaticVariables.PlayerEntity.TargetDirection + turndir) & 0x1f);
            case 7:
                var ret = GetCardinalDirToPlayer(entity);
                if (ret != -1)
                {
                    return (uint)((ret + turndir) & 0x1f);
                }

                break;
            case 0:
                break;
        }
        return (uint)turndir;
    }

    public int GetCardinalDirToPlayer(Entity entity)
    {
        if (entity == StaticVariables.g_activeCollisionEntity)
        {
            return -1;
        }

        var difx = StaticVariables.PlayerEntity.ModdedXPos - entity.ModdedXPos;

        if ((difx >= 0 && entity.Width < difx)
            || (difx < 0 && StaticVariables.PlayerEntity.Width < -difx))
        {
            //checkx
            if (StaticVariables.PlayerEntity.XPos < entity.XPos)
            {
                return 0x08;
            }

            return 0x18;
        }

        //checky
        if (StaticVariables.PlayerEntity.YPos < entity.YPos)
        {
            return 0x10;
        }

        return 0x00;
    }

    public void StartFlying(Entity entity, uint animationId, short baseDelay, uint probabilityTargeted)
    {
        StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;

        if ((uint)((ulong)StaticVariables.g_gameRandomSeed * 0x65 >> 32) < (probabilityTargeted & 0xff))
        {
            var direction = (uint)ScriptHelper.GetDirectionToTarget(
                StaticVariables.g_entitySlots[0].XPos - entity.XPos,
                StaticVariables.g_entitySlots[0].YPos - entity.YPos);

            entity.TargetDirection = direction;
        }
        else
        {
            StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
            entity.TargetDirection = (uint)((ulong)StaticVariables.g_gameRandomSeed * 0x20 >> 32);
        }

        entity.TargetAnimationId = animationId & 0xff;

        if (baseDelay != 0)
        {
            StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
            var delay = (short)((ulong)StaticVariables.g_gameRandomSeed * 0x10 >> 32) + baseDelay;
            entity.AIValues.Set(delay, 1);
        }
    }

    public bool TryAttackPlayer(Entity entity, int[] relativePositions, int maxHorizontalRange, int maxVerticalRange)
    {
        int currentFrame;
        bool isWithinRange;

        if (maxVerticalRange < relativePositions[2])
        {
            return false;
        }

        currentFrame = entity.CurrentFrameIndex;

        if (currentFrame == 1)
        {
            if (relativePositions[0] < 2 && -1 < relativePositions[4] &&
                relativePositions[1] <= maxHorizontalRange)
            {
                return true;
            }

            if (1 < relativePositions[1])
            {
                return false;
            }

            isWithinRange = maxHorizontalRange < relativePositions[0];
        }
        else if (currentFrame < 2)
        {
            if (currentFrame != 0)
            {
                return false;
            }

            if (relativePositions[0] < 2 && relativePositions[4] < 1 &&
                relativePositions[1] <= maxHorizontalRange)
            {
                return true;
            }

            if (1 < relativePositions[1])
            {
                return false;
            }

            isWithinRange = maxHorizontalRange < relativePositions[0];
        }
        else
        {
            if (currentFrame == 2)
            {
                if (relativePositions[1] < 2 && -1 < relativePositions[3] &&
                    relativePositions[0] <= maxHorizontalRange)
                {
                    return true;
                }

                if (1 < relativePositions[0])
                {
                    return false;
                }

                if (relativePositions[1] <= maxHorizontalRange)
                {
                    return true;
                }

                return false;
            }

            if (currentFrame != 3)
            {
                return false;
            }

            if (relativePositions[1] < 2 && relativePositions[3] < 1 &&
                relativePositions[0] <= maxHorizontalRange)
            {
                return true;
            }

            if (1 < relativePositions[0])
            {
                return false;
            }

            isWithinRange = maxHorizontalRange < relativePositions[1];
        }

        return !isWithinRange;
    }

    public int HandleAnimationDirection(Entity entity, uint newAnimId, int currentZ)
    {
        int deltaBottomRight;
        int returnValue = 0;
        uint direction = 0;
        int zTest;
        int deltaTopRight;
        int deltaBottomLeft;
        int deltaTopLeft;
        byte newDirection;
        bool needHandle = false;
        bool needSkip = false;
        bool needUpdate = false;

        zTest = currentZ + 1;
        deltaBottomRight = entity.FloorHeight;
        deltaTopLeft = deltaBottomRight - entity.MapHeights[0];
        deltaTopRight = deltaBottomRight - entity.MapHeights[1];
        deltaBottomLeft = deltaBottomRight - entity.MapHeights[2];
        deltaBottomRight = deltaBottomRight - entity.MapHeights[3];

        if (zTest < deltaTopLeft)
        {
            if (deltaTopRight <= zTest) needHandle = true;
            direction = entity.TargetDirection;
            if (0xe < direction - 9) needSkip = true;
            needUpdate = true;
        }
        else
        {
            needSkip = true;
        }

        if (needSkip)
        {
            if (zTest < deltaTopRight)
            {
                if (zTest < deltaBottomRight)
                {
                    direction = entity.TargetDirection;
                    if (direction - 0x11 < 0xf) needUpdate = true;
                    needHandle = true;
                }
            }
            else
            {
                needHandle = true;
            }

            if (zTest < deltaBottomLeft)
            {
                if (deltaTopLeft <= zTest)
                {
                    return 0;
                }

                direction = entity.TargetDirection;
                if ((int)direction < 0x10) needUpdate = true;
            }

            returnValue = 0;
        }

        if (needUpdate)
        {
            returnValue = 1;
            newDirection = (byte)StaticVariables.g_directionFlipTable[direction];
            entity.TargetAnimationId = newAnimId & 0xff;
            entity.YForceStep = 0;
            entity.XForceStep = 0;
            entity.YForce = 0;
            entity.XForce = 0;
            entity.TargetYForce = 0;
            entity.TargetXForce = 0;
            entity.TargetDirection = newDirection;
        }

        if (needHandle)
        {
            if (zTest < deltaBottomRight)
            {
                if (deltaBottomLeft <= zTest)
                {
                    return 0;
                }

                direction = entity.TargetDirection;
                if (0x10 < direction - 8) needUpdate = true;
            }
        }

        return returnValue;
    }

    public int UpdateDirectionForced(Entity entity, uint newAnimId, uint fallbackAnimId, int zThreshold)
    {
        int heightDiff;
        byte newDirection;

        heightDiff = GetEntityTileHeight(entity, newAnimId & 0xff, entity.TargetDirection);
        heightDiff = heightDiff - entity.FloorHeight;

        if (zThreshold < heightDiff || heightDiff < 1)
        {
            newDirection = (byte)StaticVariables.g_directionFlipTable[entity.TargetDirection];
            entity.TargetAnimationId = newAnimId & 0xff;
            entity.YForceStep = 0;
            entity.XForceStep = 0;
            entity.YForce = 0;
            entity.XForce = 0;
            entity.TargetYForce = 0;
            entity.TargetXForce = 0;
            entity.TargetDirection = newDirection;
        }
        else
        {
            entity.TargetAnimationId = fallbackAnimId & 0xff;
        }

        return heightDiff;
    }

    private int GetEntityTileHeight(Entity entity, uint animIndex, uint direction)
    {
        int height;
        uint stepDistance;

        //stepDistance = entity.SpriteRecord.AnimationOffsetsPointer[animIndex * 0xe + 8];
        stepDistance = entity.Sprite.AnimSets[animIndex].U6; // TODO check which property => flag or acceleration...
        height = GetTileHeightAtOffset(entity,
            StaticVariables.g_offsetXList[direction] * (int)stepDistance,
            StaticVariables.g_offsetYList[direction] * (int)stepDistance);

        return height;
    }

    private int GetTileHeightAtOffset(Entity entity, int offsetX, int offsetY)
    {
        int tileXIndex;
        uint uVar1;
        int tileYIndex;
        int[] xCoords = new int[4];
        int[] yCoords = new int[4];
        int[] xCoordsPtr;
        uint uVar2;
        ushort flagBits;

        xCoordsPtr = xCoords;
        tileXIndex = entity.XPos + entity.XMod + offsetX;
        xCoords[2] = StaticVariables.g_tileToWorldXTable[tileXIndex >> 0x10];
        xCoords[0] = StaticVariables.g_tileToWorldXTable[tileXIndex >> 0x10];

        tileYIndex = entity.YPos + entity.YMod + offsetY;
        yCoords[1] = tileYIndex >> 0x14;
        yCoords[0] = yCoords[1];

        xCoords[3] = StaticVariables.g_tileToWorldXTable[(tileXIndex + entity.Width) >> 0x10];
        xCoords[1] = StaticVariables.g_tileToWorldXTable[(tileXIndex + entity.Width) >> 0x10];

        yCoords[3] = (tileYIndex + entity.Height) >> 0x14;
        yCoords[2] = yCoords[3];

        flagBits = (ushort)((entity.Flags & 8U) != 0 ? 1 : 0);
        if ((entity.Flags & 1U) != 0)
        {
            flagBits |= 0x1000;
        }

        uVar2 = 0;

        while (true)
        {
            tileXIndex = xCoordsPtr[0];
            if (tileXIndex < 1)
            {
                tileXIndex = 0;
            }
            else if (tileXIndex > 0x33)
            {
                tileXIndex = 0x33;
            }

            tileYIndex = xCoordsPtr[4];
            if (tileYIndex < 1)
            {
                tileYIndex = 0;
            }
            else if (tileYIndex > 0x3b)
            {
                tileYIndex = 0x3b;
            }

            int tileOffset = tileYIndex * 0xd0 + tileXIndex * 4 + 0x302;

            if ((CurrentMap.Map.MapTiles[tileOffset].GroundProperty & flagBits) != 0)
            //if ((StaticVariables.g_spriteVRAMPointer[tileOffset] & flagBits) != 0)
            {
                break;
            }

            //uVar1 = StaticVariables.g_spriteVRAMPointer[tileOffset + 3];
            uVar1 = CurrentMap.Map.MapTiles[tileOffset + 3].GroundProperty;
            if (uVar2 < uVar1)
            {
                uVar2 = uVar1;
            }

            xCoordsPtr = xCoordsPtr.Skip(1).ToArray();
            if (xCoordsPtr.Length == 0)
            {
                return (int)(uVar2 << 0x14);
            }
        }

        return 0x7800000;
    }


    #endregion
}