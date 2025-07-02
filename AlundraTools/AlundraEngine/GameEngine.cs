using System.Diagnostics;
using AlundraEngine.DatasBin;
using AlundraEngine.Editor;
using AlundraEngine.Gameplay;
using AlundraEngine.Gameplay.Scripts;
using AlundraEngine.Sound;
using AlundraEngine.Text;

namespace AlundraEngine;

public class GameEngine
{
    public readonly ReplayManager ReplayManager = new();

    public readonly DatasBin.DatasBin DatasBin;
    public readonly BalanceBin BalanceBin;
    public readonly SoundBin SoundBin;
    private readonly EtcResR _etcResR;
    private readonly Font3 _font3;

    public GameMap CurrentMap { get; private set; }
    public GameMap AlundraMap => DatasBin.AlundraGameMap;

    public EntityGameplayManager EntityGameplayManager { get; }
    public EffectManager EffectManager { get; }
    public EntityManager EntityManager { get; }

    private readonly EntityEventHandlers _entityEventHandlers;
    private readonly GameInitializer _gameInitializer;
    private readonly Renderer _renderer;
    private readonly PlayerManager _playerManager;
    private readonly PadManager _padManager;

    //TODO : find the variable in StaticVariables
    public int DialogState, DialogNameState, DialogName;

    public GameEngine(DatasBin.DatasBin datasBin, BalanceBin balanceBin, SoundBin soundBin, EtcResR etcResR, Font3 font3)
    {
        DatasBin = datasBin;
        BalanceBin = balanceBin;
        SoundBin = soundBin;
        _etcResR = etcResR;
        _font3 = font3;

        _entityEventHandlers = new EntityEventHandlers(this);
        _gameInitializer = new GameInitializer(this);
        _renderer = new Renderer(this);
        _padManager = new PadManager();
        EntityManager = new EntityManager(this);
        EntityGameplayManager = new EntityGameplayManager(this);
        EffectManager = new EffectManager(this);
        _playerManager = new PlayerManager(this);
    }

    public void InitializeEngine()
    {
        StaticVariables.Initialize();
        _gameInitializer.Initialize();
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
            InitializeStaticVariable();
            StaticVariables.INT_800dc4e4 = 0;
            StaticVariables.g_isGameEnding = 0;
            StaticVariables.g_warpEntryBehavior = 0;

            if (StaticVariables.g_desiredMap != StaticVariables.g_currentMap)
            {
                StaticVariables.g_currentMap = StaticVariables.g_desiredMap;
                LoadMap(StaticVariables.g_currentMap); // Added by hand
                //ReadFileFromCDIntoBuffer(StaticVariables.DATAS_BIN, StaticVariables.g_compressedImageData, (StaticVariables.INT_801eab58)[StaticVariables.g_desiredMap], (StaticVariables.INT_801eab5c)[StaticVariables.g_desiredMap] - (StaticVariables.INT_801eab58)[StaticVariables.g_desiredMap]);
                InitializeMapSpriteTable(null, null, null);
                //StaticVariables.g_compressedImageData + ??,
                //StaticVariables.g_compressedImageData + StaticVariables.DAT_80191b34,
                //StaticVariables.g_compressedImageData + StaticVariables.DAT_80191b38);
                //LoadSpriteInfo(StaticVariables.g_compressedImageData[StaticVariables.g_mapIndexInDatasBin]);
                //SetEtcAnimTableAlt(StaticVariables.g_compressedImageData[StaticVariables.g_animTableAlt_80191b48]);
                //InitializeTileSet(StaticVariables.g_currentMap, StaticVariables.g_compressedImageData[StaticVariables.g_tileSet_index_80191b44]);

                //_datasBin.AlundraGameMap.SpriteInfo.Entities.Entities[0].PosX
                //StaticVariables.g_imageBuffer[0xc];
                //StaticVariables.g_imageBuffer[0xd];
                //StaticVariables.g_imageBuffer[0xe];
                var x = CurrentMap.SpriteInfo.Entities.Entities[0].XPos;
                var y = CurrentMap.SpriteInfo.Entities.Entities[0].YPos;
                var z = CurrentMap.SpriteInfo.Entities.Entities[0].Height;

                playerPosX = x << 3;
                playerPosY = y << 3;
                playerPosZ = z << 3;
            }

            //DoNothing();
            ClearGlobalFlags();
            ResetCameraAndLoadVRAMAssets();
            InitializeItems(StaticVariables.g_imageBuffer[0xb]);
            LoadMapAndInitializeEntities(null/*StaticVariables.g_compressedImageData + StaticVariables.DAT_80191b40*/);
            WarpPlayer(playerPosX, playerPosY, playerPosZ, StaticVariables.g_warpType);
            InitializeTileAnimationSystem();
            _renderer.PrepareBufferFlip();
            LoadMapSounds(StaticVariables.g_currentMap);
            Update(1);
            _renderer.ResetDebugRenderingState();
        }

        //do
        //{
        StaticVariables.g_debugMessage = "";
        //PrintDebug();
        RenderScene(graphics);

        if (!StaticVariables.IsGamePaused || StaticVariables.DoNextFrame || ReplayManager.ApplyCurrentFrame)
        {
            if (ReplayManager.ApplyCurrentFrame)
            {
                ReplayManager.PlayOneFrame();
            }
            else
            {
                Update(0);
            }
            
            if (ReplayManager.IsSaving)
            {
                ReplayManager.SaveFrame();
            }

            StaticVariables.DoNextFrame = false;
        }

        //FntPrint();
        var ndDebugFrame = 1;
        if (StaticVariables.g_debugState < 0 && (StaticVariables.g_debugFlags & 0x40000000) != 0)
        {
            ndDebugFrame = StaticVariables.g_debugVar_NbFrameBreak;
        }
        //PauseGameDuringNbFrame(ndDebugFrame);
        //DoNothing();
        //} while (StaticVariables.g_isGameEnding == 0);

        //end game
        if (StaticVariables.g_isGameEnding != 0)
        {
            //HandleMapSoundEffects(StaticVariables.g_desiredMap, StaticVariables.g_warpEntryBehavior);
            StaticVariables.g_warpEntryBehavior = 0;
            StartWarpTransition(StaticVariables.g_warpType);
            StaticVariables.INT_800dc4e4 = 1;
            do
            {
                StaticVariables.g_debugMessage = "";
                _padManager.UpdatePads();
                //isEffectRunning = FUN_80044440(StaticVariables.g_orderingTableBuffer + 3, StaticVariables.g_warpType);
                //HandleMapSoundStreaming();
                //PauseGameDuringNbFrame(1);
                //DoNothing();
            } while (isEffectRunning != 0);

            EndGame();
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
            Debugger.Break();
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

    // 8002bd60
    private void RenderScene(Graphics graphics)
    {
        _renderer.RenderScene(graphics);
    }

    // 8004dc68
    public int UpdateEntityFromWarpFlag(int param_1)
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

    // 8004dd30
    public int FinalizeWarpEntities(short warpEntityId)
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

    // 8004ddf4
    public int SetMaxFadeLevel(short maxFadeLevel)
    {
        if (maxFadeLevel < 5)
        {
            if (maxFadeLevel < 0)
            {
                StaticVariables.g_fadeControl.MaxTargetLevel = 0;
            }
            else
            {
                StaticVariables.g_fadeControl.MaxTargetLevel = maxFadeLevel;
            }
        }
        else
        {
            StaticVariables.g_fadeControl.MaxTargetLevel = 4;
        }

        return StaticVariables.g_fadeControl.MaxTargetLevel;
    }

    // 8004debc
    public int SetFadeTargetLevel(short targetLevel)
    {
        if (StaticVariables.g_fadeControl.MaxTargetLevel < targetLevel)
        {
            StaticVariables.g_fadeControl.TargetLevel = StaticVariables.g_fadeControl.MaxTargetLevel;
        }
        else if (targetLevel < 0)
        {
            StaticVariables.g_fadeControl.TargetLevel = 0;
        }
        else
        {
            StaticVariables.g_fadeControl.TargetLevel = targetLevel;
        }

        return StaticVariables.g_fadeControl.TargetLevel;
    }

    // 8004df80
    public int ApplyFadeLevel(short newFadeValue)
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

    // 8004b730
    public void SetupPostWarpGraphics()
    {
        StaticVariables.INT_ARRAY_800a8284[0] = GetFadeControlWarpVisualId();
        StaticVariables.INT_ARRAY_800a8284[1] = StaticVariables.INT_ARRAY_800a8284[0];
        StaticVariables.INT_ARRAY_800a8284[3] = GetCurrentPaletteFadeLevel();
        StaticVariables.INT_ARRAY_800a8284[2] = StaticVariables.INT_ARRAY_800a8284[3];
    }

    // 8004e78c
    private int GetFadeControlWarpVisualId()
    {
        return StaticVariables.g_fadeControl.WarpVisualId;
    }

    // 8004dd18
    private int GetFadeControl()
    {
        return StaticVariables.g_fadeControl.CurrentWarpEntityId;
    }

    // 8004dddc
    public int GetCurrentPaletteFadeLevel()
    {
        return StaticVariables.g_fadeControl.MaxTargetLevel;
    }

    // 8004e484
    public void LoadWarpVisuals(ushort warpVisualId)
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

    // 8004e030
    public uint GetCurrentTileIndex()
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

    // 8004e18c
    private uint SelectTileMapSection(uint sectionId)
    {
        // Check if sectionId is valid (less than 0x20)
        if (sectionId >= 0x20)
        {
            Debugger.Break();
            Debug.WriteLine($"Invalid tile map section ID: {sectionId}");
            return 0xFFFFFFFF; // Return -1 as uint
        }

        int bestMatchIndex = -1;
        int currentIndex = 0;

        // Loop through up to 0x80 (128) section entries
        while (currentIndex < 0x80)
        {
            // Get the section ID from the current entry in g_tileMapWarpSections
            // Each entry is 10 bytes (5 shorts), with the first short being the section ID
            short entrySectionId = StaticVariables.g_tileMapWarpSections[currentIndex * 5];

            // Check if this entry matches our target sectionId
            if (entrySectionId == sectionId)
            {
                // Check if this warp is enabled (usage count > 0)
                short usageCount = StaticVariables.g_warpUsageTable[currentIndex * 2 + 1];

                if (usageCount > 0)
                {
                    if (bestMatchIndex == -1)
                    {
                        // This is the first match we've found
                        bestMatchIndex = currentIndex;

                        // Check bit 0 of the second short in the entry
                        // If bit 0 is not set, return this index immediately
                        short flags = StaticVariables.g_tileMapWarpSections[currentIndex * 5 + 1];
                        if ((flags & 0x1) == 0)
                        {
                            return (uint)currentIndex;
                        }
                    }
                    else
                    {
                        // We already have a match, check if this one has higher priority
                        // Compare priority value (the third short in each entry)
                        short currentPriority = StaticVariables.g_tileMapWarpSections[currentIndex * 5 + 2];
                        short bestPriority = StaticVariables.g_tileMapWarpSections[bestMatchIndex * 5 + 2];

                        if (bestPriority < currentPriority)
                        {
                            bestMatchIndex = currentIndex;
                        }
                    }
                }
            }

            currentIndex++;
        }

        return (uint)bestMatchIndex;
    }

    public void SetTileAnimationMode(int animationMode, int animationBankIndex)
    {
        StaticVariables.g_tileAnimationMode = animationMode;
        StaticVariables.g_tileAnimationType = animationBankIndex;
        StaticVariables.g_animationFrameCounter = 1;
        StaticVariables.g_tileOffset = 0;

        if (0 < animationBankIndex)
        {
            //StaticVariables.g_animationData = StaticVariables.g_tile_set + (animationBankIndex + -1) * 0x10 + StaticVariables.g_tileSetMetaData.tileAnimationOffset;
        }
    }

    // 
    private void InitializeStaticVariable()
    {
        StaticVariables.g_mapLimits = 0x3c;
        StaticVariables.g_debugFrameDelay = 0;
        StaticVariables.g_debugFlags = StaticVariables.g_debugFlags & 0xf7ffff3f;
    }

    // 8002cc58
    private void InitializeMapSpriteTable(byte[] buffer, ushort[] vramTable, byte[] otherPtr)
    {
        //already loaded in GameMap

        //StaticVariables.g_spriteVRAMPointer = vramTable;
        //StaticVariables.g_imageBufferCompressed = otherPtr;
        //StaticVariables.g_imageBuffer = buffer;

        //StaticVariables.g_spriteMapTable[]
    }

    private void LoadSpriteInfo(SpriteRecord spriteRecord)
    {
        InitializeSpriteInfo(StaticVariables.g_currentMapSpriteInfo, spriteRecord);
    }

    private void InitializeSpriteInfo(SpriteInfoHeader spriteInfoHeader, SpriteRecord spriteRecord)
    {
        //Loaded in GameMap
    }

    private void SetEtcAnimTableAlt(int param_1)
    {
        StaticVariables.g_etcAnimTable = param_1;
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

    private void InitializeItems(int param_1)
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
        //LoadImageArea(StaticVariables.g_bufferImage, 0x40, 0x1e0, 0x40);
        //LoadCompressedImageToBuffer(bufferImage, 0x140, 0, 5, StaticVariables.g_bufferImage2);
        InitializeEntitySlots();
        InitializeMapEvents();
        InitializeEffectSlots();
    }

    // 8003c510
    private void InitializeMapEvents()
    {
        int programBMapCode;
        int i = 0;
        MapEvent mapEventDest;
        MapEvent emptyMapEvent;
        //SiMapEventRecord mapEventRecord;
        Entity entity;
        byte monitorEnabledFlag;

        //MapEvent[] mapEvents = StaticVariables.g_mapEvents;
        //MapEvent pEmptyMapEvent = StaticVariables.g_emptyMapEvent;
        //MapEvent pMapEvents = mapEvents[0];

        do
        {
            StaticVariables.g_mapEvents[i].Id = i;
            StaticVariables.g_mapEvents[i].ProgramBMap = 0;
            StaticVariables.g_mapEvents[i].MapEventRecord = null;
            StaticVariables.g_mapEvents[i].Entity = null;
            StaticVariables.g_mapEvents[i].EventData.Sp = 0;
            StaticVariables.g_mapEvents[i].EventData.CodeIndex = 0;
            Array.Clear(StaticVariables.g_mapEvents[i].EventData.Exp);

            //StaticVariables.g_mapEvents[i].EventData.Tick = 0;
            //for (int j = 0; j < StaticVariables.g_mapEvents[i].EventData.Variables.Length; j++)
            //{
            //    StaticVariables.g_mapEvents[i].EventData.Variables[j] = 0;
            //}
            //StaticVariables.g_mapEvents[i].EventData.LogicResult = 0;
            //StaticVariables.g_mapEvents[i].EventData.ElapsedMs = 0;
            //StaticVariables.g_mapEvents[i].EventData.IsWaiting = 0;
            //for (int j = 0; j < StaticVariables.g_mapEvents[i].EventData.Codes.Length; j++)
            //{
            //    StaticVariables.g_mapEvents[i].EventData.Codes[j] = 0;
            //}


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

        //byte[] MonitorFlags = StaticVariables.g_initMapEventRecords[4] + 1; //.Skip(4).ToArray();
        //entity = StaticVariables.PlayerEntity;//StaticVariables.g_mapEvents[0].Entity;

        foreach (var mapEventRecord in CurrentMap.SpriteInfo.MapEvents.Records)
        {
            //mapEventRecord = StaticVariables.g_initMapEventRecords[i];

            if (mapEventRecord == null)
            {
                break;
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

                StaticVariables.g_mapEvents[i].Entity = StaticVariables.PlayerEntity;
                StaticVariables.g_mapEvents[i].MapEventRecord = mapEventRecord;
                StaticVariables.g_mapEvents[i].ProgramBMap = programBMapCode;
                StaticVariables.g_mapEvents[i].EventData = new EventProgramState();
                StaticVariables.g_mapEvents[i].Id = i;
            }

            i++;
        }


        //do
        //{
        //    mapEventRecord = StaticVariables.g_initMapEventRecords[i];
        //
        //    if (mapEventRecord == null)
        //    {
        //        return;
        //    }
        //
        //    programBMapCode = mapEventRecord.EventCodesBIndex;
        //
        //    if (programBMapCode == 0)
        //    {
        //        if (StaticVariables.g_debugState < 0 && (StaticVariables.g_debugFlags & 0x20) != 0)
        //        {
        //            //PrintInfo();
        //        }
        //    }
        //    else
        //    {
        //        //entity.MapEventProgramId = mapEventRecord.EventCodesBIndex;
        //        //entity.AIValues[6] = mapEventRecord;
        //        //entity.AIValues[8] = (short)programBMapCode;
        //
        //        StaticVariables.g_mapEvents[i].Entity = entity;
        //        StaticVariables.g_mapEvents[i].MapEventRecord = mapEventRecord;
        //        StaticVariables.g_mapEvents[i].ProgramBMap = programBMapCode;
        //        StaticVariables.g_mapEvents[i].EventData = new EventProgramState();
        //        StaticVariables.g_mapEvents[i].Id = i;
        //    }
        //
        //    i++;
        //    //MonitorFlags = MonitorFlags + 8; //.Skip(8).ToArray();
        //    //entity = entity.ChildEntity;
        //} while (i < 0x80);
    }

    private void InitializeEffectSlots()
    {
        SpriteEffect effect;
        int val;
        int effectIndex;
        int[] effectInitTable;

        EffectManager.InitializeEffectSlots();

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

    public SpriteEffect SpawnSpriteEffect(int effectId, int checkSpawnArea)
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
                effect = EffectManager.GetFreeEffect();
                if (effect == null)
                {
                    effect = null;
                }
                else
                {
                    EffectManager.InitializeEffects(
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
        CurrentMap = DatasBin.GameMaps[mapId];

        if (!CurrentMap.Loaded)
        {
            using var reader = DatasBin.OpenBin();
            CurrentMap.Load(reader, true);
        }

        LoadMap(CurrentMap);

        //TODO : remove this debug code
        //var mapTiles = this.CurrentMap.Map.MapTiles;
        //
        //for (var i = 0; i < mapTiles.Length; i++)
        //{
        //    Debug.WriteLine($"{mapTiles[i].TileX}:{mapTiles[i].TileY} {mapTiles[i].Walkability} {mapTiles[i].GroundProperty} {mapTiles[i].Slope} {mapTiles[i].Height} {mapTiles[i].TileId} {mapTiles[i].TilesOffset}");
        //}
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

    private void InitializeEntitySlots()
    {
        EntityManager.InitializeEntitySlots();

        StaticVariables.g_numberOfEntity = 0;

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

        for (int i = 0; i < CurrentMap.SpriteInfo.Entities.Entities.Length; i++)
        {
            if (CurrentMap.SpriteInfo.Entities.Entities[i] == null)
            {
                break;
            }

            var entity = SpawnEntity(null, i, 0);

            if (entity != null)
            {

            }
        }

        StaticVariables.g_entityFollowedByCamera = StaticVariables.PlayerEntity;
    }

    private void ResetEntityState()
    {
        var spriteRecord = GetSpriteFromSpriteTable(false, 0, out _, out _);

        EntityManager.InitializeEntity(StaticVariables.PlayerEntity, null,
            spriteRecord, null, 0, -1,
            StaticVariables.g_cameraTargetX, StaticVariables.g_cameraTargetY,
            StaticVariables.g_animation_id, (uint)StaticVariables.g_warpTriggerType,
            (uint)StaticVariables.g_warpExtraParam,
            0xb, 0x60);

        StaticVariables.g_entitySlots[0].Status = 2;
        StaticVariables.g_entitySlots[0].HpMax = GetFadeControlWarpVisualId();
        StaticVariables.g_entitySlots[0].Hp = GetFadeControl();
        StaticVariables.g_activeCollisionEntity = null;
        StaticVariables.g_playerWarpTimer = 0;
        StaticVariables.g_isWarpDisabled = 0;
        StaticVariables.g_frameTimer = 0;
        //var tileIndex = GetCurrentTileIndex();
        //StaticVariables.g_currentTileFlags = StaticVariables.g_tileAttributeLUT[tileIndex];
        Array.Clear(StaticVariables.g_playerEffectTransitionCooldown);
        ResetWarpLockTimer();
    }

    private void ResetWarpLockTimer()
    {
        StaticVariables.g_warpLockTimer = 0;
    }

    public Entity SpawnEntity(Entity ownerEntity, bool isMapSprite, uint tableIndex, int xpos, int ypos, int zpos, uint dir)
    {
        int paletteIndex, sheetSize;

        var spriteRecord = GetSpriteFromSpriteTable(isMapSprite, tableIndex, out paletteIndex, out sheetSize);

        if (spriteRecord == null)
        {
            return null;
        }

        var entity = EntityManager.AllocateEntitySlot();
        if (entity == null)
        {
            return null;
        }

        var spriteTableIndex = tableIndex;
        if (isMapSprite)
        {
            spriteTableIndex += 0x100;
        }

        EntityManager.InitializeEntity(entity, ownerEntity, spriteRecord, null, spriteTableIndex, -1, xpos, ypos, zpos, 0, dir, paletteIndex, sheetSize);

        return entity;
    }

    // 8003a1b8
    public Entity SpawnEntity(Entity parent, int spriteInfoEntityIndex, int notCheckSpawnZone)
    {
        var entityRecord = CurrentMap.SpriteInfo.Entities.Entities[spriteInfoEntityIndex];

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

        var entity = EntityManager.AllocateEntitySlot();
        if (entity == null)
        {
            Debugger.Break();
            return null;
        }

        int spriteTableIndex = entityRecord.SpriteTableIndex;
        if (isMapSprite)
        {
            spriteTableIndex |= 0x100;
        }

        var directionIndex = entityRecord.SpriteDirection & 3;

        EntityManager.InitializeEntity(
            entity, parent, 
            spriteRecord, entityRecord, (uint)spriteTableIndex, spriteInfoEntityIndex,
            (entityRecord.XPos * 0xc + 0xc) * 0x10000,
            (entityRecord.YPos * 8 + 8) * 0x10000,
            entityRecord.Height << 0x13,
            0,
            (uint)StaticVariables.g_cardinalDirectionTable[directionIndex],
            paletteIndex,
            sheetSize);

        return entity;
    }

    public SpriteRecord GetSpriteFromSpriteTable(bool isMapSprite, uint spriteTableIndex, out int addedtosheet, out int addedtopallette)
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
            si = DatasBin.AlundraGameMap.SpriteInfo;
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

    public void PlaySoundEffect(uint sfxId)
    {
        //TODO
    }

    //TODO all the slope stuff
    // 80037f28
    public int GetCollisionOnZ(Entity entity)
    {
        var collision = entity.TerrainHeight + 1;

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
            var otherEntity = StaticVariables.g_collideableEntities[dex];

            if (otherEntity == entity)
            {
                continue;
            }

            if (otherEntity.ModdedZPos + otherEntity.Height >= entity.ModdedZPos
                || otherEntity.ModdedZPos + otherEntity.Height < collision)
            {
                continue;
            }

            if (otherEntity.ModdedXPos - entity.ModdedXPos >= 0)
            {
                if (otherEntity.ModdedXPos - entity.ModdedXPos >= entity.Width + 1)
                {
                    continue;
                }
            }
            else
            {
                if (entity.ModdedXPos - otherEntity.ModdedXPos >= otherEntity.Width + 1)
                {
                    continue;
                }
            }

            if (otherEntity.ModdedYPos - entity.ModdedYPos >= 0)
            {
                if (otherEntity.ModdedYPos - entity.ModdedYPos < entity.Depth + 1)
                {
                    collision = otherEntity.ModdedZPos + otherEntity.Depth;
                }
            }
            else
            {
                if (entity.ModdedYPos - otherEntity.ModdedYPos < otherEntity.Depth + 1)
                {
                    collision = otherEntity.ModdedZPos + otherEntity.Depth; 
                }
            }

        }

        return collision;
    }

    // 80032a40
    public void InitializeContents(Entity entity)
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
                    entity.ContentsItemId = (uint)GetContentsItemId(0);
                    return;
                }
            }
            if (entity.EntityRecord.Contents != 0)
            {
                entity.ContentsItemId = (uint)GetContentsItemId(entity.EntityRecord.Contents);
                return;
            }
        }
        else
        {
            entity.ContentsGameFlag = 0;
        }

        entity.ContentsItemId = (uint)GetContentsItemId(entity.Sprite.Header.Contents);
    }

    public int GetContentsItemId(short contentId)
    {
        var isValid = contentId < 0x100;

        while (true)
        {
            if (!isValid)
            {
                return 0;
            }
            if ((contentId & 0x80) == 0)
            {
                break;
            }

            StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
            contentId = (short)((contentId & 0x7f) * 0x10 + -0x7ffd71f4 + StaticVariables.g_gameRandomSeed * 0x10 >> 0x20);
            isValid = contentId < 0x100;
        }

        if (0x61 < contentId)
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

    public void BeginFadeEffect(int fadeTPageIndex, int fadeDuration)
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

    public void SetFadeDuration(int fadeDuration)
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

    // 8004a09c
    private int LoadMapSounds(int mapId)
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
        _renderer.PrepareBufferFlip();

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

    public void EndGame()
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
        //MoveImage(g_currentDrawEnv + 1,(int)g_currentDrawEnv.x,(int)g_currentDrawEnv.y);
    }

    private void Update(int endGame)
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

        _padManager.UpdatePads();

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
                    var canWarp = IsMapUnlocked(0x27);
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

        UpdateWorld();

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
            TriggerDebugZone() == 0)
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

    public int IsMapUnlocked(int warpIndex)
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

    // 8004e5c4
    public int StartWarpToMap(int mapIndex)
    {
        int warpEntryOffset;
        short remainingWarps;

        if (mapIndex < 0 || StaticVariables.g_totalWarpEntries <= mapIndex)
        {
            //LogDebugMessage(StaticVariables.g_logMessage_InvalidWarpVisualId + 0x54, mapIndex);
            warpEntryOffset = 0;
        }
        else
        {
            warpEntryOffset = (mapIndex * 2) * 2 + StaticVariables.g_warpUsageTable[0];
            remainingWarps = StaticVariables.g_warpUsageTable[mapIndex * 2 + 1];
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

    private int TriggerDebugZone()
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

    private void UpdateWorld()
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
        UpdateEntities();
        EffectManager.UpdateEffects();
    }

    //8003c67c
    private void RunMapEvents()
    {
        if ((StaticVariables.g_playerControlFlags & 0x48) != 0)
        {
            return;
        }

        var playerEntity = StaticVariables.PlayerEntity;

        for (int i = 0; i < 64; ++i)
        {
            var currentMapEvent = StaticVariables.g_mapEvents[i];

            if ((currentMapEvent.ProgramBMap & 0x7F) == 0)
            {
                continue;
            }

            var mapEventEntity = currentMapEvent.Entity;
            int programId = mapEventEntity.EventTrigger;

            var record = currentMapEvent.MapEventRecord;
            int px = playerEntity.TileX;
            int py = playerEntity.TileY;

            if (px < record.X1 || px > record.X2 || py < record.Y1 || py > record.Y2)
            {
                mapEventEntity.ChildEntity = null;
                mapEventEntity.EventProgramState.Sp = 0;
                mapEventEntity.RelativeWarpOffsetX = 0;
                mapEventEntity.Index = playerEntity.Index;
                mapEventEntity.EventTrigger = record.EventCodesBIndex;
                continue;
            }

            playerEntity.ProgramIndexes[ScriptHelper.ProgramBMap] = currentMapEvent.ProgramBMap;
            playerEntity.MapEventProgramId = currentMapEvent.ProgramBMap;

            playerEntity.EventTrigger = i;
            playerEntity.LogicContextEntity = mapEventEntity;
            playerEntity.EventProgramState.CopyFrom(currentMapEvent.EventData);

            RunScript(playerEntity, ScriptHelper.ProgramBMap);

            currentMapEvent.EventData.CopyFrom(playerEntity.EventProgramState);
            currentMapEvent.Entity = playerEntity.LogicContextEntity;
            mapEventEntity.EventTrigger = playerEntity.EventTrigger;
        }
        
        //var medex = 0;
        //foreach (var mapEvent in StaticVariables.g_mapEvents)
        //{
        //    var eventCode = mapEvent.ProgramBMap;
        //
        //    if ((eventCode & 0x7f) == 0)
        //    {
        //        continue;
        //    }
        //
        //    var mapEventRecord = mapEvent.MapEventRecord;
        //
        //    if (playerEntity.TileX > mapEventRecord.X1 
        //        && playerEntity.TileX < mapEventRecord.X2 
        //        && playerEntity.TileY > mapEventRecord.Y1 
        //        && playerEntity.TileY < mapEventRecord.Y2)
        //    {
        //        playerEntity.ProgramIndexes[ScriptHelper.ProgramBMap] = mapEvent.ProgramBMap;
        //        playerEntity.MapEventProgramId = mapEvent.ProgramBMap;
        //        playerEntity.EventProgramState.CopyFrom(mapEvent.EventData);
        //        playerEntity.EventTrigger = medex;
        //        playerEntity.LogicContextEntity = mapEvent.Entity;
        //
        //        _entityEventHandlers.RunEntityEventScripts(playerEntity, ScriptHelper.ProgramBMap);
        //
        //        mapEvent.ProgramBMap = playerEntity.ProgramIndexes[ScriptHelper.ProgramBMap];
        //        mapEvent.EventData.CopyFrom(playerEntity.EventProgramState);
        //        mapEvent.Entity = playerEntity.LogicContextEntity;
        //    }
        //    else
        //    {
        //        playerEntity.Index = StaticVariables.g_entitySlots[0].Index; // ??
        //        playerEntity.Index2 = 0;
        //        playerEntity.RelativeWarpOffsetX = 0; // new EventProgramState();
        //        playerEntity.ChildEntity = null;
        //
        //        mapEvent.Entity = playerEntity;
        //        mapEvent.ProgramBMap = mapEventRecord.EventCodesBIndex;
        //        playerEntity.ProgramIndexes[ScriptHelper.ProgramBMap] = mapEventRecord.EventCodesBIndex; // ??
        //
        //    }
        //    medex++;
        //}
    }

    public SpriteEffectRecord GetEffectSpriteFromSpriteTable(bool isMapSprite, int spritetableindex, out int addedtosheet, out int addedtopallette)
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
            si = DatasBin.AlundraGameMap.SpriteInfo;
            addedtosheet = 0xb;
            addedtopallette = 0x60;
        }
        if (spritetableindex >= 0 && spritetableindex < si.SpriteTable.Length)
        {
            return si.SpriteEffects[spritetableindex];
        }

        return null;
    }

    private void UpdateEntities()
    {
        EntityManager.UpdateEntities();

        if (StaticVariables.g_entityFollowedByCamera != null && StaticVariables.g_entityFollowedByCamera.Status <= 3)
        {
            StaticVariables.g_cameraLookAtX = StaticVariables.g_entityFollowedByCamera.PosX >> 16;
            StaticVariables.g_cameraLookAtY = StaticVariables.g_entityFollowedByCamera.PosY >> 16;
            StaticVariables.g_cameraLookAtZ = StaticVariables.g_entityFollowedByCamera.PosZ >> 16;
        }
    }

    //80031b50
    public void MovePlayer()
    {
        _playerManager.MovePlayer();
    }

    // 8003a774
    public void DestroyEntity(Entity entity)
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
            entity.PlatformEntity.WarpEntity = null;
        }
    }

    // 8003a59c
    public void DestroyEntity(Entity entity, int effectId)
    {
        SpawnEntityContents(entity);

        entity.Status = 4;
        entity.EventTrigger = -1;

        if (entity.ActiveEffect != null)
        {
            entity.ActiveEffect.Status = 0;
            //.spriteTableIndex = 0;
            entity.ActiveEffect = null;
        }

        if (effectId == -1)
        {
            effectId = entity.Sprite.Header.BreakEffect;
        }

        if (effectId != 0)
        {
            EffectManager.CreateAttachedEffect(0, (byte)effectId, 0, entity, 1, 0, 0, 0);
        }

        if (entity.PlatformEntity != null)
        {
            //TODO: figure out what 2c is
            entity.PlatformEntity.WarpEntity = null;
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
            entity.PosX, entity.PosY, entity.PosZ, 0);

        if (child == null)
        {
            return 0;
        }

        child.ForceZ = 0xa0000;
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

    public bool CheckItemId(uint itemid)
    {
        if (itemid != 0x26)
        {
            if (itemid - 0x51 >= 2)
            {
                return false;
            }
        }
        var ret = GetCurrentPaletteFadeLevel();
        return ret < 1 ? true : false;
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

    public int CollideOnEntitiesZ(Entity entity)
    {
        var collision = entity.TerrainHeight + 1;
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

    // 8003c954
    public int GetNumberOfEntityByRefId(Entity ownerEntity, int entityId)
    {
        var matchCount = 0;

        if ((entityId & 0x80) == 0)
        {
            CheckValidEntityId(entityId);//calls getinitrecord which is a 20 byte datarecord SIEntityRecord
            foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
            {
                if ((ownerEntity.Status - 1 < 2 || ownerEntity.Status == 3) && entity.EntityRefId == entityId)
                {
                    StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                }
            }
            return matchCount;
        }

        var functionId = entityId & 0x7f;

        switch (functionId)
        {
            case 0://get owner
                StaticVariables.g_matchingEntitiesBuffer[matchCount++] = ownerEntity;
                return matchCount;

            case 1://get player
                StaticVariables.g_matchingEntitiesBuffer[matchCount++] = StaticVariables.PlayerEntity;
                return matchCount;

            case 2://get all entities
                foreach (var entity in StaticVariables.g_entitySlots)
                {
                    if (entity.Status - 1 < 2 || entity.Status == 3)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }
                }
                return matchCount;

            case 3://get all entities except player
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if (entity.Status - 1 < 2 || entity.Status == 3)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }
                }
                return matchCount;

            case 4://all entities on the ground
                foreach (var entity in StaticVariables.g_entitySlots)
                {
                    if ((ownerEntity.Status - 1 < 2 || ownerEntity.Status == 3)
                        && (entity.Flags & 0x80) != 0
                        && (entity.AnimFlags & 0x80) == 0
                        && entity.PlatformEntity == null)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }
                }
                return matchCount;

            case 5://all entities besides player that the ownerentity is riding on
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && ownerEntity.RidingEntity == entity)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }
                }
                return matchCount;

            case 6://all entities besides player that are riding on the ownerentity
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && entity.RidingEntity == ownerEntity)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }
                }
                return matchCount;

            case 7://all entities besides player where ownerentity.xcollision? == entity
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && ownerEntity.XCollisionEntity == entity)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }

                }
                return matchCount;

            case 8://all entities besides player where entity.xcollision? == ownerentity
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && entity.XCollisionEntity == ownerEntity)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }

                }
                return matchCount;

            case 9://all entities besides player where entity.ownerentity [c] == ownerentity
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && entity.ParentEntity == ownerEntity)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }

                }
                return matchCount;

            case 10://all entities besides player where ownerentity.ownerentity [c] == entity
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && ownerEntity.ParentEntity == entity)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }

                }
                return matchCount;

            case 11://all entities besides player that are on a platform
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && entity.PlatformEntity != null)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }

                }
                return matchCount;
        }

        return matchCount;
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

        var entity = EntityManager.AllocateEntitySlot();

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

        EntityManager.InitializeEntity(entity, ownerEntity, sprite, data, spriteTable, entityId, x, y, z, 0, dir, addedtosheet, addedtopalette);

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
            entityResult = EntityManager.AllocateEntitySlot();
            entity = null;

            if (entityResult != null)
            {
                if (entityType != 0)
                {
                    subtype = subtype + 0x100;
                }

                EntityManager.InitializeEntity(entityResult, parentEntity, spriteRecord, null, subtype, -1,
                    posX, posY, posZ, 0, direction, paletteIndex, sheetSize);

                entity = entityResult;
            }
        }

        return entity;
    }

    public void RunScript(Entity entity, int eventType)
    {
        _entityEventHandlers.RunEntityEventScripts(entity, eventType);
    }

    public void RunSpriteEvent(Entity entity)
    {
        var eventId = entity.SpriteProgramIndexes[entity.EventTrigger];
        _entityEventHandlers.SpriteHandlers.RunSpriteHandler(entity.EventTrigger, eventId, entity);
    }

    public SpriteEffect CreateEffect_MapType(byte mapeffectid, bool checkBoundingbox, EffectManager effectManager)
    {
        var record = this.GetMapEffectRecord(mapeffectid, checkBoundingbox);
        if (record != null)
        {
            if (!checkBoundingbox && (record.Flags & 0x40) == 0)
            {
                return null;
            }

            var effect = effectManager.GetNextAvailableEffect();

            if (effect != null)
            {
                effectManager.InitializeEffects(effect, record, mapeffectid, 0,
                    (byte)((record.Flags & 0x80) >> 7), record.EffectId, record.AnimId,
                    (record.X * 12 + 12) << 16, (record.Y * 8 + 8) << 16, record.Z << 19);

                return effect;
            }
        }
        return null;
    }

    // 8005a9e0
    public void SetNextMapId(int mapIndex)
    {
        Debugger.Break();
        var mapId = Array.IndexOf(DatasBin.Header.GameMaps, mapIndex);
        StaticVariables.g_desiredMap = mapId;
        /*
        bool bVar1;
        undefined3 extraout_var;
        code *previousVSyncCallback;

        if (((g_isCdResetRequested != 0) || ((g_cdIsReady != 0 && (g_cdDataLoaded == 0)))) &&
            (bVar1 = IsSoundDriverReady(), CONCAT31(extraout_var,bVar1) == 0)) {
            g_cdDataStartPtr = DAT_CDAranXa_pos + g_mapCdDataOffsets[mapIndex * 3];
            g_cdDataEndPtr = g_cdDataStartPtr + g_mapCdDataOffsets[mapIndex * 3 + 2] * 8 + -1;
            g_cdReadPtr = g_cdDataStartPtr;
            previousVSyncCallback = (code *)VSyncCallback(OnCdDataStreamComplete);
            if ((previousVSyncCallback != OnCdDataStreamComplete) && (previousVSyncCallback != (code *)0x0))
            {
                g_previousVSyncCallback = (int)previousVSyncCallback;
            }
            g_cdControlCommand = 1;
            g_cdTrackIndex = (undefined1)g_mapCdDataOffsets[mapIndex * 3 + 1];
            CdControlF('\r',&g_cdControlCommand);
            g_cdReadComplete = 0;
            g_cdInitRequired = 2;
        }*/
    }

    // 8004b114
    public void FUN_8004b114(int variable, int i)
    {
        Debugger.Break();
    }

    // 80049b7c
    public void LoadBgm(int bgmIndex)
    {
        StaticVariables.g_resetSoundFlag = 0;
        if (bgmIndex == 0)
        {
            InitializeBgm(StaticVariables.g_requestedSeqId);
        }
        else
        {
            StaticVariables.g_soundEffectState = 0x78;
        }
    }

    // 8008f458
    private void InitializeBgm(short seqId)
    {
        FUN_8008f2e8(seqId, 0);
    }

    // 8008f2e8
    private void FUN_8008f2e8(short seqId, short i)
    {
        Debugger.Break();
    }

    //80049af4
    public void StopAllSound()
    {
        Debugger.Break();
    }

    //8004df68
    public int FUN_8004df68()
    {
        Debugger.Break();
        return 0;
        //return (int)StaticVariables.g_fadeControl[1].currentWarpEntityId;
    }

    //8004e004
    public void FUN_8004e004(int param_1)
    {
        Debugger.Break();
        //ApplyFadeLevel(StaticVariables.g_fadeControl[1].currentWarpEntityId - param_1);
    }

    //8004dfd8
    public void AdjustFadeLevelRelative(int relativeFadeValue)
    {
        Debugger.Break();
        //ApplyFadeLevel(relativeFadeValue + StaticVariables.g_fadeControl[1].currentWarpEntityId);
    }

    //8004df10
    public void FUN_8004df10(int index)
    {
        Debugger.Break();
        //SetFadeTargetLevel(index + StaticVariables.g_fadeControl.TargetLevel);
    }

    public int HandleMapTriggerCommand(int commandId)
    {
        Debugger.Break();
        return 0;
        /*
        int result;
        int fadeLevel;

        switch (commandId)
        {
            case 0:
                result = 0;
                break;
            default:
                result = IsMapRequirementMet(commandId) ? 1 : 0;
                break;
            case 0x45:
                fadeLevel = 1;
                goto ApplyFadeShortcut;
            case 0x46:
                fadeLevel = 5;
                goto ApplyFadeShortcut;
            case 0x47:
                fadeLevel = 10;
                goto ApplyFadeShortcut;
            case 0x48:
                fadeLevel = 0x1e;
                ApplyFadeShortcut:
                AdjustFadeLevelRelative(fadeLevel);
                result = 1;
                break;
            case 0x4f:
                IncreaseFadeLevel(1);
                result = 1;
                break;
            case 0x50:
                SpawnCamExplosionEffects();
                result = 1;
                break;
            case 0x51:
                SpawnRandomExplosionParticles();
                result = 1;
                break;
            case 0x52:
                SpawnSpinningParticleRing();
                result = 1;
                break;
            case 0x53:
                TriggerExplosionEffect(StaticVariables.g_entitySlots);
                result = 1;
                break;
            case 0x54:
                AddLifeToEntity(StaticVariables.g_entitySlots);
                result = 1;
                break;
            case 0x55:
                AddLowHpAndSpawnEffect(StaticVariables.g_entitySlots);
                result = 1;
                break;
            case 0x56:
                AddMediumHpAndSpawnEffect(StaticVariables.g_entitySlots);
                result = 1;
                break;
        }
        return result;*/
    }

    //8002d7b0
    public void ChangeAreaTileProperties(int mapTileIndex)
    {
        Debugger.Break();
        //spriteData = g_spriteVRAMPointer + mapTileIndex * 3 + 2;
        var mapTile = CurrentMap.Map.MapTiles[mapTileIndex];


        //ChangeAreaTileProperties();
    }

    //8002d608
    public void ChangeAreaTileProperties(int startX, int startY, int sizeX, int sizeY, int distX, int distY)
    {
        int distX2;
        int x;
        int y;

        if (startX < 0 || startY < 0 || sizeX < 0 || sizeY < 0 || distX < 0 || distY < 0)
        {
            Debugger.Break();
            //Debug.WriteLine(startX,startY,sizeX,sizeY,distX,distY);
        }

        if (0x34 < startX + sizeX || 0x3c < startY + sizeY || 0x34 < distX + sizeX || 0x3c < distY + sizeY)
        {
            Debugger.Break();
            //Debug.WriteLine(startX,startY,sizeX,sizeY,distX,distY);
        }

        y = 0;

        if (0 < sizeY)
        {
            do
            {
                x = 0;
                distX2 = distX;

                if (0 < sizeX)
                {
                    do
                    {
                        var tile = CurrentMap.Map.MapTiles[distX2 + (distY + y) * 52];
                        var tile2 = CurrentMap.Map.MapTiles[startX + x + (startY + y) * 52];
                        tile.GroundProperty = tile2.GroundProperty;
                        tile.Walkability = tile.Walkability;
                        x = x + 1;
                        distX2 = distX + x;
                    } while (x < sizeX);
                }
                y = y + 1;

            } while (y < sizeY);
        }
    }

    //8003cfc8
    public uint ResolveDirectionFromParam(Entity entity, uint encodedDir)
    {
        uint direction;
        int facingDirection;
        uint result;

        result = encodedDir & 0x1F;

        switch ((int)encodedDir >> 5)
        {
            case 0:
                return result;

            case 1:
                direction = entity.TargetDirection + result;
                goto LAB_8003d110;

            case 2:
                result = (uint)StaticVariables.g_cardinalDirectionTable[encodedDir & 3];
                break;

            case 3:
                direction = (uint)ScriptHelper.GetDirectionToTarget(
                    StaticVariables.PlayerEntity.PosX - entity.PosX,
                    StaticVariables.PlayerEntity.PosY - entity.PosY);
                direction += result;
                goto LAB_8003d110;

            case 4:
                StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                var rand = (int)((ulong)StaticVariables.g_gameRandomSeed * 4 >> 0x20);
                result = (uint)StaticVariables.g_cardinalDirectionTable[rand];
                break;

            case 5:
                StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                result = (uint)((ulong)StaticVariables.g_gameRandomSeed * 0x20 >> 0x20);
                break;

            case 6:
                direction = StaticVariables.PlayerEntity.TargetDirection + result;
                LAB_8003d110:
                result = (direction & 0x1F);
                break;

            case 7:
                facingDirection = GetWarpFacingDirection(entity);
                if (facingDirection == -1)
                {
                    return result;
                }
                return (uint)((facingDirection + result) & 0x1F);

            default:
                result = 0;
                break;
        }

        return result;
    }

    // 8003cf20
    private int GetWarpFacingDirection(Entity entity)
    {
        int deltaX;

        if (StaticVariables.g_activeCollisionEntity != entity)
        {
            return -1;
        }

        deltaX = StaticVariables.g_entitySlots[0].ModdedXPos - entity.ModdedXPos;

        if (deltaX < 0)
        {
            if (-deltaX <= StaticVariables.g_entitySlots[0].Width)
            {
                return (int)((StaticVariables.g_entitySlots[0].PosY < entity.PosY ? 1U : 0U) << 4);
            }
        }
        else if (deltaX <= entity.Width)
        {
            return (int)((StaticVariables.g_entitySlots[0].PosY < entity.PosY ? 1U : 0U) << 4);
        }

        deltaX = 0x18;
        if (StaticVariables.g_entitySlots[0].PosX < entity.PosX)
        {
            deltaX = 0x08;
        }

        return deltaX;
    }

    // 8003166c
    public Portal GetWarpData()
    {
        foreach (var infoPortal in CurrentMap.Info.Portals)
        {
            if (StaticVariables.PlayerEntity.TileX > infoPortal.X1
                || StaticVariables.PlayerEntity.TileX < infoPortal.X2
                || StaticVariables.PlayerEntity.TileY > infoPortal.Y1
                || StaticVariables.PlayerEntity.TileY < infoPortal.Y2)
            {
                return infoPortal;
            }
        }

        return null;
    }

    //8003a7b0
    public void CheckAndTriggerTileEffect(Entity entity)
    {
        /*
        if (entity.FrameCollision != null)
        {
            int[] worldXCoords = new int[4];
            int[] worldYCoords = new int[4];
            int tileZ, height;

            worldXCoords[2] = StaticVariables.g_tileToWorldXTable[(short)(entity.HitBoxX >> 16)];
            worldXCoords[0] = worldXCoords[2];
            worldXCoords[3] = StaticVariables.g_tileToWorldXTable[(int)((entity.HitBoxX + entity.TransformWidth) >> 16)];
            worldXCoords[1] = worldXCoords[3];

            worldYCoords[1] = entity.HitBoxY >> 20;
            worldYCoords[0] = worldYCoords[1];
            worldYCoords[3] = (int)((entity.HitBoxY + entity.TransformDepth) >> 20);
            worldYCoords[2] = worldYCoords[3];

            tileZ = entity.HitBoxZ;
            height = entity.TransformHeight;

            int[] xCoordListPtr = worldXCoords;

            for (int i = 0; i < 4; i++)
            {
                int tileX1 = xCoordListPtr[i];
                if (tileX1 < 1)
                    tileX1 = 0;
                else if (tileX1 > 0x33)
                    tileX1 = 0x33;

                int tileX2 = worldYCoords[i];
                if (tileX2 < 1)
                    tileX2 = 0;
                else if (tileX2 > 0x3B)
                    tileX2 = 0x3B;

                int index = tileX2 * 0xd0 + tileX1 * 4 + 0x302;
                ushort* tileDataPointer = (ushort*)(StaticVariables.g_spriteVRAMPointer + index);

                if ((*tileDataPointer & 2) != 0)
                {
                    int tileEffectZ = (StaticVariables.g_spriteVRAMPointer[index + 3] & 0xFF) << 20;
                    if (tileZ <= tileEffectZ + 0x80000 && tileEffectZ + 0x80000 <= tileZ + height)
                    {
                        *tileDataPointer = (ushort)(*tileDataPointer & 0xFFFD);
                        tileDataPointer[3] = 0xFFFF;

                        int effectX = xCoordListPtr[i] * 0x180000 + 0xC0000;
                        int effectY = worldYCoords[i] * 0x100000 + 0x80000;

                        EffectManager.CreateEffectEntity(0, StaticVariables.g_sharedBuffer2[9], 0, effectX, effectY, tileEffectZ);
                        CreateWarpEffect(0xFF, effectX, effectY, tileEffectZ);
                        PlaySoundEffect(0x1F);
                    }
                }
            }
        }*/
    }

    // 800347d4
    public int PlayCutscene(int mapId)
    {
        int iVar1;
        int iVar2;

        if (StaticVariables.PlayerEntity.HpMax <= StaticVariables.PlayerEntity.Hp)
        {
            iVar1 = GetMaxUnlockedMap();
            iVar2 = GetCurrentPaletteFadeLevel();
            if (iVar2 <= iVar1)
            {
                PlaySoundEffect(3);
                return 1;
            }
        }
        FUN_80033a2c(StaticVariables.PlayerEntity);
        PlaySoundEffect(0x30);
        StartWarpToMap(mapId);
        return 1;
    }

    // 80033a2c
    private void FUN_80033a2c(Entity entity)
    {
        // Set entity's HP to its max HP
        entity.Hp = entity.HpMax;

        // Set fade target level
        SetFadeTargetLevel((short)GetCurrentPaletteFadeLevel());

        // Create first set of effects in a loop (8 effects total)
        for (int i = 0; i < 8; i++)
        {
            // Create an effect at player position with effect ID 14 (0xE)
            bool useFlag = (i & 1) == 0;
            int flag = useFlag ? 0 : 1;

            // Get player position
            int playerY = StaticVariables.g_entitySlots[0].PosY;

            // Create an effect entity
            SpriteEffect effect = EffectManager.CreateEffectEntity(
                0,              // behaviorFlags
                14,             // spriteTableIndex (0xE)
                (byte)flag,     // animationIndex
                StaticVariables.g_entitySlots[0].PosX,    // x position
                playerY,        // y position
                StaticVariables.g_entitySlots[0].PosZ + 0x80000  // z position (slightly above player)
            );

            // If effect was created successfully, set its parameters
            if (effect != null)
            {
                // Add offset to X and Y position based on index
                short xOffset = StaticVariables.g_offsetXList[i];
                short yOffset = StaticVariables.g_offsetYList[i];

                // Apply offsets to effect position
                effect.X += xOffset << 11;
                effect.Y += yOffset << 11;

                // Calculate effect index based on loop counter
                int effectIndex = ((i << 2) + 8) & 0x1F;
                effectIndex <<= 1;

                // Apply additional offsets and force values
                short xForce = StaticVariables.g_offsetXList[effectIndex / 2 + 4];
                short yForce = StaticVariables.g_offsetYList[effectIndex / 2 + 4];

                // Set effect parameters
                effect.ForceZ = 0x30000;
                effect.ForceX = (xForce * 8 - xForce) << 6;
                effect.ForceY = (yForce * 8 - yForce) << 6;
            }
        }

        // Create second set of random-positioned effects (4 effects total)
        for (int i = 0; i < 4; i++)
        {
            // Create another effect entity
            SpriteEffect effect = EffectManager.CreateEffectEntity(
                0,              // behaviorFlags
                14,             // spriteTableIndex (0xE)
                (byte)i,        // animationIndex (use loop counter as animation index)
                StaticVariables.g_entitySlots[0].PosX,    // x position
                StaticVariables.g_entitySlots[0].PosY,    // y position
                StaticVariables.g_entitySlots[0].PosZ + 0x100000  // z position (higher above player)
            );

            if (effect != null)
            {
                // Set different Z force value
                effect.ForceZ = 0x40000;

                // Generate random offset values using the game's RNG
                StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                effect.ForceX = GenerateRandomOffset(StaticVariables.g_gameRandomSeed, 0xFFFF0000);

                StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                effect.ForceY = GenerateRandomOffset(StaticVariables.g_gameRandomSeed, 0xFFFF8000);
            }
        }

        // Create third set of effects
        for (int i = 0; i < 4; i++)
        {
            SpriteEffect effect = EffectManager.CreateEffectEntity(
                0,              // behaviorFlags
                14,             // spriteTableIndex (0xE)
                2,              // animationIndex fixed at 2
                StaticVariables.g_entitySlots[0].PosX,    // x position
                StaticVariables.g_entitySlots[0].PosY,    // y position
                StaticVariables.g_entitySlots[0].PosZ + 0x100000  // z position (higher above player)
            );

            if (effect != null)
            {
                // Set different Z force value
                effect.ForceZ = 0x20000;

                // Generate random offset values using the game's RNG
                StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                effect.ForceX = GenerateRandomOffset(StaticVariables.g_gameRandomSeed, 0xFFFF0000);

                StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                effect.ForceY = GenerateRandomOffset(StaticVariables.g_gameRandomSeed, 0xFFFF8000);
            }
        }
    }

    private int GenerateRandomOffset(uint seed, uint maskValue)
    {
        // Generate a value between -0x18000 and +0x18000 using high bits of the seed
        int highBits = (int)(((ulong)seed * 0x30001) >> 32);
        return (highBits - 0x18000) & (int)maskValue;
    }

    // 8004dea4
    private int GetMaxUnlockedMap()
    {
        return StaticVariables.g_fadeControl.TargetLevel;
    }

    // 800445c0
    public int GetItemDataPointer(int itemId)
    {
        Debugger.Break();
        // Check if the item ID is valid (less than 0x62/98)
        if (itemId >= 0x62)
        {
            throw new Exception("Illegal Item No!");
        }

        // Get the offset for this item ID from the balance bin buffer
        using var br = DatasBin.OpenBin();
        br.BaseStream.Position = itemId * 2 + 0x32c;
        var currentRecord = 0;

        // Traverse the balance record chain until we find one with Level less than the threshold
        // or reach the end of the chain
        //while (currentRecord != null && currentRecord.Level < StaticVariables.g_itemIdThreshold)
        //{
        //    // Add the current record to our list
        //    balanceRecords.Add(currentRecord);
        //
        //    // Follow the chain to the next record
        //    currentRecord = currentRecord.Next;
        //}

        // Return the array of balance records
        return currentRecord;
    }

    // 8004e0f8
    public int GetTriggeredWarpMapId()
    {
        short warpId = StaticVariables.g_fadeControl.CurrentWarpEntityId;
        int isUnlocked = IsMapUnlocked(warpId);

        if (isUnlocked == 0)
        {
            return -1;
        }

        // Calcul de l'indice dans la table g_tileMapWarpSections
        // (warpId * 5) est l'indice multiplié par la taille de chaque entrée
        var sectionId = StaticVariables.g_tileMapWarpSections[warpId * 5];

        uint tileMapSectionIndex = SelectTileMapSection((uint)sectionId);
        FUN_8004e4d8(tileMapSectionIndex);
        return warpId;
    }

    // 8004e4d8
    private void FUN_8004e4d8(uint tileMapSectionIndex)
    {
        if ((int)tileMapSectionIndex < 0)
        {
            Debugger.Break();
            Debug.WriteLine("Invalid tile map section index in FUN_8004e4d8");
            return;
        }

        if (tileMapSectionIndex >= StaticVariables.g_totalWarpEntries)
        {
            Debugger.Break();
            Debug.WriteLine("Invalid tile map section index in FUN_8004e4d8");
            return;
        }

        StaticVariables.g_fadeControl.CurrentWarpEntityId = (short)tileMapSectionIndex;
    }
}