using AlundraEngine.DatasBin;
using AlundraEngine.Editor;
using AlundraEngine.Gameplay;
using AlundraEngine.Gameplay.Scripts;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using System;
using System.Diagnostics;
using AlundraEngine.UI;
using WarpData = AlundraEngine.DatasBin.WarpData;

namespace AlundraEngine;

public class GameEngine
{
    public readonly ReplayManager ReplayManager = new();

    public readonly DatasBin.DatasBin DatasBin;
    public readonly BalanceBin BalanceBin;
    public readonly EtcResR EtcResR;
    public readonly Font3 Font3;

    public GameMap CurrentMap { get; private set; }
    public GameMap AlundraMap => DatasBin.AlundraGameMap;

    public CdManager CdManager { get; }
    public EntityGameplayManager EntityGameplayManager { get; }
    public EffectManager EffectManager { get; }
    public EntityManager EntityManager { get; }
    public PlayerManager PlayerManager { get; }
    public Renderer Renderer { get; }
    public SoundManager SoundManager { get; }
    public SoundBin SoundBin { get; }
    public UIManager UIManager { get; }

    private readonly EntityEventHandlers _entityEventHandlers;
    private readonly GameInitializer _gameInitializer;
    private readonly PadManager _padManager;

    //TODO : find the variable in StaticVariables
    public int DialogState, DialogNameState, DialogName;

    public GameEngine(DatasBin.DatasBin datasBin, BalanceBin balanceBin, SoundBin soundBin, EtcResR etcResR, Font3 font3)
    {
        DatasBin = datasBin;
        BalanceBin = balanceBin;
        SoundBin = soundBin;
        EtcResR = etcResR;
        Font3 = font3;

        _entityEventHandlers = new EntityEventHandlers(this);
        _gameInitializer = new GameInitializer(this);
        Renderer = new Renderer(this);
        _padManager = new PadManager();

        CdManager = new CdManager(this);
        EntityManager = new EntityManager(this);
        EntityGameplayManager = new EntityGameplayManager(this);
        EffectManager = new EffectManager(this);
        PlayerManager = new PlayerManager(this);
        SoundManager = new SoundManager(this);
        UIManager = new UIManager(this);
    }

    public void InitializeEngine()
    {
        StaticVariables.Initialize(this);
        _gameInitializer.Initialize();
    }

    // 8002bfe0
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

                byte x = 0;
                byte y = 0;
                byte z = 0;

                //when alundra dies, the map hasn't entities
                if (CurrentMap.SpriteInfo.Entities.Entities[0] != null)
                {
                    x = CurrentMap.SpriteInfo.Entities.Entities[0].XPos;
                    y = CurrentMap.SpriteInfo.Entities.Entities[0].YPos;
                    z = CurrentMap.SpriteInfo.Entities.Entities[0].Height;
                }

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
            Renderer.PrepareBufferFlip();
            LoadMapSounds(StaticVariables.g_currentMap);
            Update(1);
            Renderer.ResetDebugRenderingState();
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
        Renderer.RenderScene(graphics);
    }

    // 8004e030
    public uint GetItemIdFromCurrentWeapon()
    {
        var itemId = 0xffffffff;

        switch (StaticVariables.g_playerStats.WeaponId - 1)
        {
            case 0:
                itemId = GetWeaponIdFromSlot1();
                break;
            case 1:
                itemId = GetWeaponIdFromSlot2();
                break;
            case 2:
                itemId = GetWeaponIdFromSlot3();
                break;
            case 3:
                itemId = GetWeaponIdFromSlot4();
                break;
            case 4:
                itemId = GetWeaponIdFromSlot5();
                break;
            case 5:
                itemId = GetWeaponIdFromSlot6();
                break;
        }

        return itemId;
    }

    public uint GetWeaponIdFromSlot1()
    {
        return GetWeaponIdFromSlot(1);
    }

    public uint GetWeaponIdFromSlot2()
    {
        return GetWeaponIdFromSlot(2);
    }

    public uint GetWeaponIdFromSlot3()
    {
        return GetWeaponIdFromSlot(3);
    }

    public uint GetWeaponIdFromSlot4()
    {
        return GetWeaponIdFromSlot(4);
    }

    public uint GetWeaponIdFromSlot5()
    {
        return GetWeaponIdFromSlot(5);
    }

    private uint GetWeaponIdFromSlot6()
    {
        return GetWeaponIdFromSlot(6);
    }

    // 8004e18c
    private uint GetWeaponIdFromSlot(uint weaponTypeId)
    {
        if (weaponTypeId >= 0x20)
        {
            Debugger.Break();
            Debug.WriteLine($"Invalid weaponTypeId: {weaponTypeId}");
            return 0xFFFFFFFF;
        }

        uint bestMatchIndex = 0xFFFFFFFF;
        uint currentIndex = 0;

        while (currentIndex < 0x80)
        {
            var entrySectionId = StaticVariables.g_itemsProperties[currentIndex * 5];

            if (entrySectionId == weaponTypeId)
            {
                var usageCount = StaticVariables.g_numberOfItems[currentIndex * 2 + 1];

                if (usageCount > 0)
                {
                    if (bestMatchIndex == 0xFFFFFFFF)
                    {
                        bestMatchIndex = currentIndex;

                        var flags = StaticVariables.g_itemsProperties[currentIndex * 5 + 1];
                        if ((flags & 0x1) == 0)
                        {
                            return currentIndex;
                        }
                    }
                    else
                    {
                        var currentPriority = StaticVariables.g_itemsProperties[currentIndex * 5 + 2];
                        var bestPriority = StaticVariables.g_itemsProperties[bestMatchIndex * 5 + 2];

                        if (bestPriority < currentPriority)
                        {
                            bestMatchIndex = currentIndex;
                        }
                    }
                }
            }

            currentIndex++;
        }

        return bestMatchIndex;
    }

    public void SetTileAnimationMode(int animationMode, int animationBankIndex)
    {
        StaticVariables.g_tileAnimationMode = animationMode;
        StaticVariables.g_tileAnimationType = animationBankIndex;
        StaticVariables.g_animationFrameCounter = 1;
        StaticVariables.g_tileOffset = 0;

        if (0 < animationBankIndex)
        {
            Debugger.Break();
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
        var iVar2 = 0x3f;
        var piVar1 = 0x3f;

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

    private void InitializeItems(int threshold)
    {
        var iVar2 = 2;
        var piVar1 = 2;

        StaticVariables.g_itemIdThreshold = threshold;

        do
        {
            StaticVariables.g_items[piVar1 + 2] = 0;
            iVar2 = iVar2 - 1;
            piVar1 = piVar1 - 2;
        } while (iVar2 >= 0);
    }

    //8002dfe4
    private void LoadMapAndInitializeEntities(uint[] bufferImage)
    {
        //LoadImageArea(StaticVariables.g_bufferImage, 0x40, 0x1e0, 0x40);
        //LoadCompressedImageToBuffer(bufferImage, 0x140, 0, 5, StaticVariables.g_bufferImage2);
        InitializeEntitySlots();
        InitializeMapEvents();
        EffectManager.InitializeEffectSlots();
    }

    // 8003c510
    private void InitializeMapEvents()
    {
        int programBMapCode;
        var i = 0;
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

    public void LoadMap(int mapId)
    {
        CurrentMap = DatasBin.GameMaps[mapId];

        if (!CurrentMap.Loaded)
        {
            using var reader = DatasBin.OpenBin();
            CurrentMap.Load(reader, true);
            SoundBin.OpenMap(mapId);
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

        for (var i = 0; i < CurrentMap.SpriteInfo.Entities.Entities.Length; i++)
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

    //80031974
    private void ResetEntityState()
    {
        var spriteRecord = GetSpriteFromSpriteTable(false, 0, out _, out _);

        EntityManager.InitializeEntity(StaticVariables.PlayerEntity, null,
            spriteRecord, null, 0, -1,
            StaticVariables.g_cameraTargetX,
            StaticVariables.g_cameraTargetY,
            StaticVariables.g_cameraTargetZ,
            (uint)StaticVariables.g_warpTriggerType,
            (uint)StaticVariables.g_warpExtraParam,
            0xb, 0x60);

        StaticVariables.PlayerEntity.Status = 2;
        StaticVariables.PlayerEntity.HpMax = PlayerManager.GetPlayerHpMax();
        StaticVariables.PlayerEntity.Hp = PlayerManager.GetPlayerHp();
        StaticVariables.g_activeCollisionEntity = null;
        StaticVariables.g_playerWarpTimer = 0;
        StaticVariables.g_isWarpDisabled = 0;
        StaticVariables.g_playerWarpEffect = null;
        var weaponItemId = GetItemIdFromCurrentWeapon();
        StaticVariables.g_currentWeaponFlags = StaticVariables.g_weaponFlagsByItemId[weaponItemId];
        Array.Clear(StaticVariables.g_playerEffectTransitionCooldown);
        ResetWarpLockTimer();
    }

    //8003295c
    private void ResetWarpLockTimer()
    {
        StaticVariables.g_warpLockTimer = 0;
    }

    //8003a1b8
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

        var directionIndex = entityRecord.SpriteDirection & 0x3;

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

    //80039b28
    public SpriteRecord GetSpriteFromSpriteTable(bool isMapSprite, uint spriteTableIndex, out int paletteOffset, out int sheetSize)
    {
        SpriteInfo spriteInfo;

        if (isMapSprite)
        {
            spriteInfo = CurrentMap.SpriteInfo;
            paletteOffset = 0;
            sheetSize = 0x20;
        }
        else
        {
            spriteInfo = DatasBin.AlundraGameMap.SpriteInfo;
            paletteOffset = 0xb;
            sheetSize = 0x60;
        }
        if (spriteTableIndex < 0)
        {
            throw new Exception("Illegal Character Race!");
        }

        if (spriteTableIndex >= spriteInfo.SpriteTable.Length)
        {
            throw new Exception("Illegal Character Race!");
        }

        var sprite = spriteInfo.Sprites[spriteTableIndex];
        return sprite;
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

            if (otherEntity.ModdedPosZ + otherEntity.Height >= entity.ModdedPosZ
                || otherEntity.ModdedPosZ + otherEntity.Height < collision)
            {
                continue;
            }

            if (otherEntity.ModdedPosX - entity.ModdedPosX >= 0)
            {
                if (otherEntity.ModdedPosX - entity.ModdedPosX >= entity.Width + 1)
                {
                    continue;
                }
            }
            else
            {
                if (entity.ModdedPosX - otherEntity.ModdedPosX >= otherEntity.Width + 1)
                {
                    continue;
                }
            }

            if (otherEntity.ModdedPosY - entity.ModdedPosY >= 0)
            {
                if (otherEntity.ModdedPosY - entity.ModdedPosY < entity.Depth + 1)
                {
                    collision = otherEntity.ModdedPosZ + otherEntity.Depth;
                }
            }
            else
            {
                if (entity.ModdedPosY - otherEntity.ModdedPosY < otherEntity.Depth + 1)
                {
                    collision = otherEntity.ModdedPosZ + otherEntity.Depth;
                }
            }

        }

        return collision;
    }

    // 80032a40
    public void InitializeContents(Entity entity)
    {
        if (entity.EntityRecord == null)
        {
            entity.ContentsGameFlag = 0;
        }
        else
        {
            int contents = entity.EntityRecord._10;

            if ((contents & 0x7ff) >= 800)
            {
                contents = 0;
            }

            entity.ContentsGameFlag = contents;
            if (contents != 0)
            {
                uint[] flags;
                var val = contents;

                if ((contents & 0x8000) == 0)
                {
                    flags = StaticVariables.g_mapFlags;
                }
                else
                {
                    flags = StaticVariables.g_globalFlags;
                }

                var index = (contents >> 3) & 0xffc;
                var mask = 1 << (val & 0x1f);

                if ((flags[index] & mask) == 0)
                {
                    entity.ContentsItemId = (uint)GetContentsItemId(entity.EntityRecord.Contents);
                    return;
                }
            }

            if (entity.EntityRecord.Contents != 0)
            {
                entity.ContentsItemId = (uint)GetContentsItemId(entity.EntityRecord.Contents);
                return;
            }
        }

        entity.ContentsItemId = (uint)GetContentsItemId((ushort)entity.SpriteRecord.Header.Contents);
    }

    //80032968
    public int GetContentsItemId(ushort contentId)
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
            uint rand = StaticVariables.g_gameRandomSeed >> 28;
            var index = ((contentId & 0x7F) << 4) | rand;
            contentId = StaticVariables.g_itemRandomTable[index];
            isValid = contentId < 0x100;
        }

        if (0x61 < contentId)
        {
            return 0;
        }

        return contentId;
    }

    //800440fc
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
        SoundManager.LoadMapSounds(mapId);
        Renderer.PrepareBufferFlip();
        return 1;
    }

    public uint GetMapWarpDestination(int mapId)
    {
        uint currentMapId;
        var warpDataIndex = 0;

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

        var entityBeforeWarp = StaticVariables.g_lastWarpEntityIndex;
        int finalEntity;

        if (StaticVariables.g_playerControlFlags == 0 &&
            StaticVariables.PlayerEntity.IsNotProcessable == 0 &&
            StaticVariables.g_warpLockTimer == 0 &&
            StaticVariables.g_padState1.ButtonsHold == (PadState.Start | PadState.Select) &&
            StaticVariables.g_warpDelayFrames == 0 &&
            StaticVariables.g_globalTransitionState == 0)
        {
            finalEntity = StaticVariables.g_lastWarpEntityIndex + 1;

            if (StaticVariables.g_lastWarpEntityIndex == 0x78)
            {
                finalEntity = StaticVariables.g_lastWarpEntityIndex;

                if (StaticVariables.PlayerEntity.Hp != 0)
                {
                    var itemCount = PlayerManager.GetNumberOfItem(0x27);
                    if (itemCount != 0)
                    {
                        PlayerManager.UseItem(0x27);
                    }
                    StaticVariables.g_lastWarpEntityIndex = 0;
                    StaticVariables.PlayerEntity.Hp = 0;
                    //StaticVariables.PlayerEntity.DamagedTickCounter = entityBeforeWarp;
                    StaticVariables.INT_ARRAY_800a8284[0] = 0;
                    PlayerManager.SetPlayerHp(0);
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
            (StaticVariables.g_padState1.ButtonsJustPressed & PadState.OpenInventory) != 0 &&
            StaticVariables.g_warpDelayFrames == 0 &&
            (StaticVariables.g_padState1.ButtonsHold & PadState.Select) == 0 &&
            StaticVariables.g_globalTransitionState == 0 &&
            DisplayInventory() == 0)
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

    //80055570
    private int DisplayInventory()
    {
        uint isSpecialWarpTriggered;
        
        if (StaticVariables.g_forbiddenWarpFlag == 0)
        {
            isSpecialWarpTriggered = CheckSpecialWarpCondition(0);
            if (isSpecialWarpTriggered != 0)
            {
                return 1;
            }
        
            isSpecialWarpTriggered = CheckSpecialWarpCondition(0xb);
            if (isSpecialWarpTriggered != 0)
            {
                return 1;
            }
        
            if (StaticVariables.g_cdIsReady == 0)
            {
                if ((StaticVariables.g_padState1.ButtonsHold & PadState.Right) != 0)
                {
                    TriggerWarpTypeA();
                    return 1;
                }
                if ((StaticVariables.g_padState1.ButtonsHold & PadState.Left) != 0)
                {
                    //TriggerWarpTypeB();
                    return 0;
                }
                if ((StaticVariables.g_padState1.ButtonsHold & PadState.Up) != 0)
                {
                    //StartFadeOut();
                    return 1;
                }
                if ((StaticVariables.g_padState1.ButtonsHold & PadState.Down) != 0)
                {
                    //TriggerWarpTypeC();
                    return 1;
                }
            }

            Renderer.InitializeFrame();
            Renderer.SetTransitionType(6);
            var sprite = Renderer.GetAnimationImageByIndex(0);
            //InitCameraTransition(-player.PosX, -player.PosY, -player.PosZ,
            //    -StaticVariables.g_cameraScrollingX, -StaticVariables.g_cameraScrollingY,
            //    sprite.U, sprite.V, sprite.Witdh, sprite.Height);
            //DisplayWarpNames();
            SoundManager.PlaySoundEffect(4);
        }

        return 1;
    }

    //80047c8c
    private uint CheckSpecialWarpCondition(int index)
    {
        return (uint)StaticVariables.g_callbackTable[index].Flags & 1;
    }

    //80051f1c
    private void TriggerWarpTypeA()
    {
        StaticVariables.DAT_8017e8d8 = 0;
        StaticVariables.DAT_8017e990 = 0;
        StaticVariables.DAT_8017e9ac = 0;
        StaticVariables.DAT_8017e998 = 0x4f824f82;
        StaticVariables.DAT_8017e99c = 0x4f82;
        StaticVariables.DAT_8017e99e = 0;
        Renderer.SetTransitionType(0xb);
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

        for (var i = 0; i < StaticVariables.g_mapEvents.Length; ++i)
        {
            var currentMapEvent = StaticVariables.g_mapEvents[i];

            if ((currentMapEvent.ProgramBMap & 0x7F) == 0)
            {
                continue;
            }

            var mapEventEntity = currentMapEvent.Entity;
            var programId = mapEventEntity.EventTrigger;

            var record = currentMapEvent.MapEventRecord;
            var px = playerEntity.TileX;
            var py = playerEntity.TileY;

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
        //    if (playerEntity.TileX > mapEventRecord.X 
        //        && playerEntity.TileX < mapEventRecord.Width 
        //        && playerEntity.TileY > mapEventRecord.Y 
        //        && playerEntity.TileY < mapEventRecord.Height)
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
        //        playerEntity.Index = StaticVariables.PlayerEntity.Index; // ??
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
        PlayerManager.MovePlayer();
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
            entity.PlatformEntity.CarriedEntity = null;
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
            entity.ActiveEffect = null;
        }

        if (effectId == -1)
        {
            effectId = entity.SpriteRecord.Header.BreakEffect;
        }

        if (effectId != 0)
        {
            EffectManager.CreateAttachedEffect(0, (byte)effectId, 0, entity, 1, 0, 0, 0);
        }

        if (entity.PlatformEntity != null)
        {
            entity.PlatformEntity.CarriedEntity = null;
        }
    }

    //80032b90
    public int SpawnEntityContents(Entity entity)
    {
        if (entity.ContentsItemId == 0)
        {
            return 0;
        }

        if (CanDropMpItems(entity.ContentsItemId))
        {
            return 0;
        }

        var spawnedEntity = SpawnWarpEntity(null, 0, entity.ContentsItemId + 0x1e,
            entity.PosX, entity.PosY, entity.PosZ, 0);

        if (spawnedEntity == null)
        {
            return 0;
        }

        spawnedEntity.ForceZ = 0xa0000;
        spawnedEntity.Bytes[0] = 1;
        spawnedEntity.Bytes[1] = 0;
        spawnedEntity.Bytes[2] = 0;
        spawnedEntity.Bytes[3] = 0;
        spawnedEntity.Flags &= 0xffffff7f;

        var delay = 600;
        if (StaticVariables.g_iconNameEtcBase[(entity.ContentsItemId * 8 + 4) / 4] == 0)
        {
            delay = -1;
        }

        spawnedEntity.ItemDelay = delay;
        spawnedEntity.ItemState = 0;
        spawnedEntity.AIValues[0] = (short)(entity.ContentsGameFlag & 0xFFFF);
        spawnedEntity.AIValues[1] = (short)((entity.ContentsGameFlag >> 16) & 0xFFFF);
        spawnedEntity.AIValues[2] = 0;
        spawnedEntity.AIValues[3] = 10;

        SoundManager.PlaySoundEffect(0x54);
        return 1;
    }

    //80032a00
    public bool CanDropMpItems(uint itemId)
    {
        if (itemId == 0x26
            || itemId == 0x51
            || itemId == 0x52)
        {
            return PlayerManager.GetPlayerMpMax() == 0;
        }

        return false;
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

        EntityManager.InitializeEntity(entity, ownerEntity,
            sprite, data, spriteTable, entityId,
            x, y, z,
            0, dir,
            addedtosheet, addedtopalette);

        return entity;
    }

    public SiEntityRecord GetInitData(int entityId)
    {
        SiEntityRecord res;

        if (entityId < 0 || CurrentMap.SpriteInfo.Entities.Entities.Length <= entityId) // StaticVariables.g_maxInitData
        {
            Debugger.Break();
            //"Illegal character initial data!!
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

    //80039f58
    public Entity SpawnWarpEntity(Entity parentEntity, int entityType, uint subtype, int posX, int posY, int posZ, uint direction)
    {
        SpriteRecord spriteRecord;
        Entity entityResult = null;
        int paletteIndex;
        int sheetSize;

        spriteRecord = GetSpriteFromSpriteTable(entityType == 1, subtype, out paletteIndex, out sheetSize);

        if (spriteRecord != null)
        {
            entityResult = EntityManager.AllocateEntitySlot();

            if (entityResult != null)
            {
                if (entityType != 0)
                {
                    subtype = subtype + 0x100;
                }

                EntityManager.InitializeEntity(entityResult, parentEntity,
                    spriteRecord, null, subtype, -1,
                    posX, posY, posZ,
                    0, direction,
                    paletteIndex, sheetSize);
            }
        }

        return entityResult;
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

    // 8005a9e0
    public void SetNextMapId(int mapIndex)
    {
        Debugger.Break();
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

    //8002d7b0
    public void ChangeAreaTileProperties(int mapTileIndex)
    {
        Debugger.Break();
        //spriteData = g_spriteVRAMPointer + mapTileIndex * 3 + 2;
        //var mapTile = CurrentMap.Map.MapTiles[mapTileIndex];


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

        if (0x34 < startX + sizeX
            || 0x3c < startY + sizeY
            || 0x34 < distX + sizeX
            || 0x3c < distY + sizeY)
        {
            Debugger.Break();
            //Debug.WriteLine(startX,startY,sizeX,sizeY,distX,distY);
        }

        y = 0;

        if (0 < sizeY)
        {
            var map = CurrentMap.Map;
            var mapWidth = CurrentMap.Map.Width;

            do
            {
                x = 0;
                distX2 = distX;

                if (0 < sizeX)
                {
                    do
                    {
                        var tileDestination = map.MapTiles[distX2 + (distY + y) * mapWidth];
                        var tileSource = map.MapTiles[startX + x + (startY + y) * mapWidth];
                        tileDestination.Walkability = tileSource.Walkability;
                        tileDestination.GroundProperty = tileSource.GroundProperty;
                        tileDestination.Height = tileSource.Height;
                        tileDestination.TileId = tileSource.TileId;
                        tileDestination.TilesOffset = tileSource.TilesOffset;

                        if (tileSource.WallTiles != null)
                        {
                            tileDestination.WallTiles.Offset = tileSource.WallTiles.Offset;

                            var length = Math.Min(tileSource.WallTiles.Tiles.Length, tileDestination.WallTiles.Tiles.Length);
                            for (int i = 0; i < length; i++)
                            {
                                tileDestination.WallTiles.Tiles[i] = tileSource.WallTiles.Tiles[i];
                            }
                        }

                        x++;
                        distX2 = distX + x;
                    } while (x < sizeX);
                }

                y++;

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
                direction = direction + encodedDir;
                goto LAB_8003d110;

            case 4:
                StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                var rand = (int)((ulong)(StaticVariables.g_gameRandomSeed * 4) >> 0x20);
                result = (uint)StaticVariables.g_cardinalDirectionTable[rand];
                break;

            case 5:
                StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                result = (uint)((ulong)(StaticVariables.g_gameRandomSeed * 0x20) >> 0x20);
                break;

            case 6:
                direction = StaticVariables.PlayerEntity.TargetDirection + result;
                LAB_8003d110:
                result = direction & 0x1F;
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

        deltaX = StaticVariables.PlayerEntity.ModdedPosX - entity.ModdedPosX;

        if (deltaX < 0)
        {
            if (-deltaX <= StaticVariables.PlayerEntity.Width)
            {
                return (int)((StaticVariables.PlayerEntity.PosY < entity.PosY ? 1U : 0U) << 4);
            }
        }
        else if (deltaX <= entity.Width)
        {
            return (int)((StaticVariables.PlayerEntity.PosY < entity.PosY ? 1U : 0U) << 4);
        }

        deltaX = 0x18;
        if (StaticVariables.PlayerEntity.PosX < entity.PosX)
        {
            deltaX = 0x08;
        }

        return deltaX;
    }

    // 8003166c
    public WarpData GetWarpData()
    {
        foreach (var infoPortal in CurrentMap.Info.Portals)
        {
            if (StaticVariables.PlayerEntity.TileX >= infoPortal.X1
                && StaticVariables.PlayerEntity.TileX <= infoPortal.X2
                && StaticVariables.PlayerEntity.TileY >= infoPortal.Y1
                && StaticVariables.PlayerEntity.TileY <= infoPortal.Y2)
            {
                return infoPortal;
            }
        }

        return null;
    }

    //8003a7b0
    public void CheckAndTriggerTileEffect(Entity entity)
    {
        if (entity.FrameCollision != null)
        {
            int[] worldXCoords = new int[4];
            int[] worldYCoords = new int[4];
            int tileZ, height;

            worldXCoords[2] = StaticVariables.g_tileToWorldXTable[(short)(entity.HitBoxX >> 16)];
            worldXCoords[0] = worldXCoords[2];
            worldXCoords[3] = StaticVariables.g_tileToWorldXTable[(int)((entity.HitBoxX + entity.CollisionWidth) >> 16)];
            worldXCoords[1] = worldXCoords[3];

            worldYCoords[1] = entity.HitBoxY >> 20;
            worldYCoords[0] = worldYCoords[1];
            worldYCoords[3] = (int)((entity.HitBoxY + entity.CollisionDepth) >> 20);
            worldYCoords[2] = worldYCoords[3];

            tileZ = entity.HitBoxZ;
            height = entity.CollisionHeight;

            for (int i = 0; i < 4; i++)
            {
                int tileX = worldXCoords[i];
                if (tileX < 1)
                {
                    tileX = 0;
                }
                else if (tileX > 0x33)
                {
                    tileX = 0x33;
                }

                int tileY = worldYCoords[i];
                if (tileY < 1)
                {
                    tileY = 0;
                }
                else if (tileY > 0x3B)
                {
                    tileY = 0x3B;
                }

                var mapWidth = CurrentMap.Map.Width;
                var tile = CurrentMap.Map.MapTiles[tileY * mapWidth + tileX];
                var tileFlags = tile.Walkability | tile.GroundProperty << 8;

                if ((tileFlags & 2) != 0)
                {
                    int tileEffectZ = (tile.Height & 0xFF) << 20;
                    //tileEffectZ = tile.Height * 0x100000;

                    if (tileZ <= tileEffectZ + 0x80000 && tileEffectZ + 0x80000 <= tileZ + height)
                    {
                        tile.Walkability = (byte)(tileFlags & 0xFFFD);
                        tile.Height = (byte)((tileFlags & 0xFFFF) >> 8);
                        tile.TilesOffset = -1;

                        int effectX = worldXCoords[i] * 0x180000 + 0xC0000;
                        int effectY = worldYCoords[i] * 0x100000 + 0x80000;

                        EffectManager.CreateEffectEntity(0,
                            CurrentMap.Info.SlideEffectId,
                            0,
                            effectX, effectY, tileEffectZ);
                        EffectManager.CreateWarpEffect(0xFF, effectX, effectY, tileEffectZ);
                        SoundManager.PlaySoundEffect(0x1F);
                    }
                }
            }
        }
    }

    // 80033a2c
    public void FUN_80033a2c(Entity entity)
    {
        entity.Hp = entity.HpMax;
        PlayerManager.SetPlayerMp((short)PlayerManager.GetPlayerMpMax());

        // Create first set of effects in a loop (8 effects total)
        for (var i = 0; i < 8; i++)
        {
            // Create an effect at player position with effect ID 14 (0xE)
            var useFlag = (i & 1) == 0;
            var flag = useFlag ? 0 : 1;

            // Get player position
            var playerY = StaticVariables.PlayerEntity.PosY;

            // Create an effect entity
            var effect = EffectManager.CreateEffectEntity(
                0,              // behaviorFlags
                14,             // spriteTableIndex (0xE)
                (byte)flag,     // animationIndex
                StaticVariables.PlayerEntity.PosX,    // x position
                playerY,        // y position
                StaticVariables.PlayerEntity.PosZ + 0x80000  // z position (slightly above player)
            );

            // If effect was created successfully, set its parameters
            if (effect != null)
            {
                // Add offset to X and Y position based on index
                var xOffset = StaticVariables.g_offsetXList[i];
                var yOffset = StaticVariables.g_offsetYList[i];

                // Apply offsets to effect position
                effect.X += xOffset << 11;
                effect.Y += yOffset << 11;

                // Calculate effect index based on loop counter
                var effectIndex = ((i << 2) + 8) & 0x1F;
                effectIndex <<= 1;

                // Apply additional offsets and force values
                var xForce = StaticVariables.g_offsetXList[effectIndex / 2 + 4];
                var yForce = StaticVariables.g_offsetYList[effectIndex / 2 + 4];

                // Set effect parameters
                effect.ForceZ = 0x30000;
                effect.ForceX = (xForce * 8 - xForce) << 6;
                effect.ForceY = (yForce * 8 - yForce) << 6;
            }
        }

        // Create second set of random-positioned effects (4 effects total)
        for (var i = 0; i < 4; i++)
        {
            // Create another effect entity
            var effect = EffectManager.CreateEffectEntity(
                0,              // behaviorFlags
                14,             // spriteTableIndex (0xE)
                (byte)i,        // animationIndex (use loop counter as animation index)
                StaticVariables.PlayerEntity.PosX,    // x position
                StaticVariables.PlayerEntity.PosY,    // y position
                StaticVariables.PlayerEntity.PosZ + 0x100000  // z position (higher above player)
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
        for (var i = 0; i < 4; i++)
        {
            var effect = EffectManager.CreateEffectEntity(
                0,              // behaviorFlags
                14,             // spriteTableIndex (0xE)
                2,              // animationIndex fixed at 2
                StaticVariables.PlayerEntity.PosX,    // x position
                StaticVariables.PlayerEntity.PosY,    // y position
                StaticVariables.PlayerEntity.PosZ + 0x100000  // z position (higher above player)
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
        var highBits = (int)(((ulong)seed * 0x30001) >> 32);
        return (highBits - 0x18000) & (int)maskValue;
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
        var itemId = StaticVariables.g_playerStats.ItemId;
        var numberOfItem = PlayerManager.GetNumberOfItem(itemId);

        if (numberOfItem == 0)
        {
            return -1;
        }

        // Calcul de l'indice dans la table g_itemsProperties
        // (warpId * 5) est l'indice multiplié par la taille de chaque entrée
        var sectionId = StaticVariables.g_itemsProperties[itemId * 5];

        var weaponId = GetWeaponIdFromSlot((uint)sectionId);
        SetCurrentItemId(weaponId);
        return itemId;
    }

    // 8004e4d8
    public void SetCurrentItemId(uint itemId)
    {
        if ((int)itemId < 0 || itemId >= StaticVariables.g_itemsCount)
        {
            Debugger.Break();
            return;
        }

        StaticVariables.g_playerStats.ItemId = (short)itemId;
    }

    public void SpawnSpinningParticleRing()
    {
        Debugger.Break();
    }

    //80057c84
    public void ApplyCameraEffect(int x, int y, int z,
        int destX, int destY,
        byte uvX, byte uvY,
        short width, short height,
        short textureId1, short textureId2)

    {
        StaticVariables.g_cameraTransitionStartX = 8;
        StaticVariables.g_cameraTransitionStartY = 0x74;
        InitCameraTransitionEffect(x, y, z, destX, destY, uvX, uvY, width, height, textureId1, textureId2);
    }

    //80057cf0
    public void InitCameraTransitionEffect(
        int srcX, int srcY, int srcZ,
        int dstXPtr, int dstYPtr,
        byte uvX, byte uvY,
        short width, short height,
        short textureId1, short textureId2)
    {
        short puVar1;
        int i;
        byte uvBottom;
        byte uvRight;

        puVar1 = StaticVariables.g_cameraTransitionState;

        if (StaticVariables.g_cameraTransitionState == 0)
        {
            i = 0;
            uvRight = (byte)(uvX + width);
            uvBottom = (byte)(uvY + height);
            StaticVariables.g_cameraTransitionDstYPtr = dstYPtr;
            StaticVariables.g_cameraTransitionState = 5;
            StaticVariables.g_cameraTransitionSrcX = srcX;
            StaticVariables.g_cameraTransitionSrcY = srcY;
            StaticVariables.g_cameraTransitionSrcZ = srcZ;
            StaticVariables.g_cameraTransitionDstXPtr = dstXPtr;

            Debugger.Break();
            
            do
            {
                var poly = StaticVariables.g_spriteInventoryAlundraPotrait[i];

                //SetPolyFT4((POLY_FT4*)poly);
                poly.r0 = 0xff;
                poly.g0 = 0xff;
                poly.b0 = 0xff;
                poly._2 = uvY;
                poly.u1 = uvRight;
                poly._3 = uvY;
                poly.v2 = uvBottom;
                poly.u3 = uvRight;
                poly.v3 = uvBottom;
                poly.x0 = 100;
                poly.y0 = 100;
                poly.x1 = (short)(width + 100);
                poly.y1 = 100;
                poly.x2 = 100;
                poly.u0 = uvX;
                poly.u2 = uvX;
                poly.x3 = (short)(width + 100);
                poly.y2 = (short)(height + 100);
                poly.y3 = (short)(height + 100);
                
                //puVar1[9] = textureId1;
                //puVar1[0xd] = textureId2;
                //puVar1 = puVar1 + 0x14;

                i = i + 1;
            } while (i < 2);
            

            StaticVariables.g_cameraDeltaX = -(StaticVariables.g_cameraTransitionSrcX + 2)
            - StaticVariables.g_cameraTransitionDstXPtr;

            StaticVariables.g_cameraX = StaticVariables.g_cameraDeltaX - StaticVariables.g_cameraTransitionStartX;
            StaticVariables.g_cameraTransitionHalfWidth = 0x30;
            StaticVariables.g_cameraTransitionHalfHeight = 0x38;
            StaticVariables.g_cameraCurrentX = StaticVariables.g_cameraTransitionStartX;
            StaticVariables.g_cameraCurrentY = StaticVariables.g_cameraTransitionStartY;
            StaticVariables.g_cameraTransitionStepValue = 0xf;
            StaticVariables.g_cameraDeltaY =
                 StaticVariables.g_cameraTransitionSrcY + 2
                 - StaticVariables.g_cameraTransitionDstYPtr
                 - (StaticVariables.g_cameraTransitionSrcZ + 2)
                 - 0x20;
            StaticVariables.g_cameraY = StaticVariables.g_cameraDeltaY - StaticVariables.g_cameraTransitionStartY;
        }
    }

    //80059f6c
    public void TriggerVisualUpdate(int spriteTableIndex)
    {
        if ((StaticVariables.g_etcDisplayFlags & 4) == 0
            && spriteTableIndex - 0x100U < 0x100
            && StaticVariables.g_entitySpriteNamesTable[spriteTableIndex] != null
            //&& StaticVariables.g_entitySpriteNamesTable[spriteTableIndex * 4] != 0
            //&& StaticVariables.g_entitySpriteNamesTable[spriteTableIndex * 4] != '\0'
            )
        {
            StaticVariables.g_entitySpriteNameTableIndex = spriteTableIndex;
            Renderer.SetTransitionType(0xc);
        }
    }

    //800423f8
    public int TryPlayEtcAnimation(uint textId, int animationMode)
    {
        string[] strings;

        if (IsWarpInProgress())
        {
            return 0;
        }

        //Debugger.Break();

        strings = AlundraMap.Strings;
        //tableBase = StaticVariables.g_etcAnimTable; //alundra string table

        if ((textId & 0x80) != 0)
        {
            strings = CurrentMap.Strings;
            //tableBase = StaticVariables.g_etcAnimTableAlt; //currentmapstringtable
        }

        var text = strings[(textId & 0x7f)];

        SetupEtcAnimation();
        PlayEtcAnimation(text, animationMode);

        return 1;
    }

    //80045004
    public bool IsWarpInProgress()
    {
        return (StaticVariables.g_warpFlags_2 & 4) != 0;
    }

    //8008167c
    private void SetupEtcAnimation()
    {
        //empty function ???
    }

    //800450f0
    public int PlayEtcAnimation(string scriptText, int animationMode)
    {
        if (Renderer.SetTransitionType(0) == 0)
        {
            return 0;
        }

        if (scriptText.Length >= 0x960)
        {
            Debugger.Break();
            var message = "the sub text is too long"; //サブテキストが長すぎます!
            Array.Copy(message.ToCharArray(), StaticVariables.g_scriptBuffer, message.Length);
        }
        else
        {
            Array.Copy(scriptText.ToCharArray(), StaticVariables.g_scriptBuffer, scriptText.Length);
        }

        // Configure le mode texte pour l’animation
        StaticVariables.g_textToDisplay.tick = 0;
        StaticVariables.g_textToDisplay.mode = 2;
        StaticVariables.g_textToDisplay.speed = 0xF;

        // Configure les coordonnées de départ du texte (X, Y)
        // selon g_textTilesConfiguration2[] qui contient des offsets
        var baseX = StaticVariables.g_textTilesConfiguration2.X;
        if (baseX < 0)
        {
            var offset = StaticVariables.g_textTilesConfiguration2.Width;
            baseX = (short)(StaticVariables.g_textTilesConfiguration2.X - (offset << 3));
        }

        StaticVariables.g_textToDisplay.x = baseX;
        StaticVariables.g_textToDisplay.y = 0xF0; // position Y fixe

        var startX = StaticVariables.g_textTilesConfiguration2.X;

        if (startX < 0)
        {
            var offset = StaticVariables.g_textTilesConfiguration2.Width;
            startX = (short)(StaticVariables.g_textTilesConfiguration2.X - (offset << 3));
        }

        StaticVariables.g_textToDisplay.startX = startX;

        var startY = StaticVariables.g_textTilesConfiguration2.Y;

        if (startY < 0)
        {
            var offset = StaticVariables.g_textTilesConfiguration2.Height;
            startY = (short)(StaticVariables.g_textTilesConfiguration2.Y - (offset << 3));
        }
        StaticVariables.g_textToDisplay.startY = startY;

        StaticVariables.g_playerControlFlags |= (uint)(animationMode == 1 ? 0x10 : 0x8);

        // Réinitialise divers états liés au texte
        StaticVariables.g_textPrimitives = 0;
        StaticVariables.g_textHoldState_2 = 0;
        StaticVariables.g_textMessageConfirmed = 0;
        StaticVariables.g_textAutoAdvanceFlag_2 = 0;
        StaticVariables.g_textAutoAdvanceFlag = 0;
        StaticVariables.g_currentVoiceSfxId = -1;
        StaticVariables.g_textBufferX = 0;
        StaticVariables.g_textLineIndex = 0;
        StaticVariables.g_textCursor = 0;
        StaticVariables.g_textRenderStep = 0;

        byte yOffset = 0x20;

        // 3 lines to display the text in the dialog box
        for (int group = 0; group < 3; ++group)
        {
            for (int y = 0; y < 2; ++y)
            {
                for (int x = 0; x < 1; ++x)
                {
                    //var sprt = StaticVariables.g_textFullLinesSprites[x + y * 2];
                    var index = x + y + group;

                    StaticVariables.g_textFullLinesSprites[index].w = 0xff;
                    StaticVariables.g_textFullLinesSprites[index].h = 0x10;
                    StaticVariables.g_textFullLinesSprites[index].u0 = 0;
                    StaticVariables.g_textFullLinesSprites[index].v0 = yOffset;

                    //SetSprt(sprt);
                    //SetSemiTrans(sprt, 0);
                    //SetShadeTex(sprt, 1);

                    var clutIndex = 8 - x;
                    x = x + 1;
                    StaticVariables.g_textFullLinesSprites[index].clut = StaticVariables.g_clutTable[clutIndex];
                }
            }

            yOffset += 0x10;
        }

        //dialog cursor
        var i = 0;

        do
        {
            StaticVariables.g_cursorTextSprites[i].w = 0x10;
            StaticVariables.g_cursorTextSprites[i].h = 0x10;
            StaticVariables.g_cursorTextSprites[i].u0 = StaticVariables.g_dialogCursorTextureU;
            StaticVariables.g_cursorTextSprites[i].v0 = StaticVariables.g_dialogCursorTextureV;
            StaticVariables.g_cursorTextSprites[i].x0 = 0;
            StaticVariables.g_cursorTextSprites[i].y0 = 0;
            StaticVariables.g_cursorTextSprites[i].clut = StaticVariables.g_clutTable[8];

            //SetSprt(StaticVariables.g_cursorTextSprites[i]);
            //SetSemiTrans(StaticVariables.g_cursorTextSprites[i], 0);
            //SetShadeTex(StaticVariables.g_cursorTextSprites[i], 1);

            i = i + 1;
        } while (i < 2);

        // Efface une zone d’écran pour préparer l’affichage
        //RECT clearRect;
        //clearRect.x = 0x3C0;
        //clearRect.y = 0x120;
        //clearRect.w = 0x40;
        //clearRect.h = 0x30;
        //ClearImage(&clearRect, 0, 0, 0);

        StaticVariables.g_textFlags = 3;
        StaticVariables.g_textDelayReset = 4;
        StaticVariables.g_textDelay = 1;
        StaticVariables.g_debugFlags_2 = 3;
        StaticVariables.g_etcAnimationMode = 3;
        StaticVariables.g_textLineStartX = 0;
        StaticVariables.g_textHoldState = 0;
        Array.Clear(StaticVariables.g_textBuffer);
        SoundManager.PlaySoundEffect(6);

        return 1;
    }

    //8004507c
    public void SetEtcAnimationMode(int mode)
    {
        StaticVariables.g_etcAnimationMode = mode;
    }

    //80032b28
    public void FUN_80032b28(uint flag)
    {
        uint[] flags;
        uint value;

        if (flag == 0)
        {
            return;
        }

        if ((flag & 0x8000) == 0)
        {
            flags = StaticVariables.g_mapFlags;
        }
        else
        {
            flags = StaticVariables.g_globalFlags;
        }

        flags[(flag >> 3) & 0xffc] |= (uint)(1 << (int)(flag & 0x1f));
    }

    //8004248c
    public int FUN_8004248c()
    {
        return IsWarpInProgress() ? 1 : 0;
    }

    //80050ba8
    public int InitializeAsyncOperation(string arg1, string arg2, ref int operationCounter)
    {
        StaticVariables.g_asyncCallbackArgs[0] = arg1;
        StaticVariables.g_asyncCallbackArgs[1] = arg2;
        StartAsyncCallback(AsyncCallbackHandler, 1, ref StaticVariables.g_asyncCallbackArgs);
        StaticVariables.g_asyncOperationCounterPtr = operationCounter;
        StaticVariables.g_asyncOperationCountdown = 0;
        return 1;
    }

    //800505fc
    private void StartAsyncCallback(Action<int> asyncCallbackHandler, short i, ref string[] args)
    {
        StaticVariables.g_asyncCallbackCounter = i;
        StaticVariables.g_asyncCallback = asyncCallbackHandler;
        StaticVariables.g_asyncCallbackArgs2 = args;
        Renderer.InitFadeOverlaySprites(StaticVariables.g_sprites);
        SoundManager.PlaySoundEffect(4);
        Renderer.SetTransitionType(3);
    }

    //80050b98
    void AsyncCallbackHandler(int counter)
    {
        StaticVariables.g_asyncOperationCounterPtr = counter;
    }
}