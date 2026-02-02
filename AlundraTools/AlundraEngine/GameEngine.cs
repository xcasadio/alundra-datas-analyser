using AlundraEngine.Balance;
using AlundraEngine.DatasBin;
using AlundraEngine.Editor;
using AlundraEngine.Gameplay;
using AlundraEngine.Gameplay.Scripts;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using AlundraEngine.UI;
using System;
using System.Diagnostics;
using System.Drawing.Imaging.Effects;
using Microsoft.VisualBasic.Logging;

namespace AlundraEngine;

public class GameEngine
{
    public readonly ReplayManager ReplayManager = new();

    public readonly DatasBin.DatasBin DatasBin;
    public readonly BalanceBin BalanceBin;
    public readonly EtcRes EtcRes;
    public readonly Font3 Font3;

    public GameMap CurrentMap { get; private set; }
    public GameMap AlundraMap => DatasBin.AlundraGameMap;

    public CdManager CdManager { get; }
    public EntityGameplayManager EntityGameplayManager { get; }
    public EffectManager EffectManager { get; }
    public EntityManager EntityManager { get; }
    public PlayerManager PlayerManager { get; }
    public GraphicManager GraphicManager { get; }
    public IRenderer Renderer { get; }
    public SoundManager SoundManager { get; }
    public SoundBin SoundBin { get; }
    public StaticVariables StaticVariables { get; }
    public UIManager UIManager { get; }
    public MainInventoryManager MainInventoryManager { get; }
    public SubInventoryManager SubInventoryManager { get; }
    public HudManager HudManager { get; }
    public UIDebugManager UIDebugManager { get; }
    public MemoryCardManager MemoryCardManager { get; }
    public LogManager LogManager { get; }

    private readonly EntityEventHandlers _entityEventHandlers;
    private readonly GameInitializer _gameInitializer;
    private readonly PadManager _padManager;

    //TODO : find the variable in StaticVariables
    public int DialogState, DialogNameState, DialogName;

    public GameEngine(DatasBin.DatasBin datasBin, BalanceBin balanceBin, SoundBin soundBin, EtcRes etcRes, Font3 font3, IRenderer renderer)
    {
        DatasBin = datasBin;
        BalanceBin = balanceBin;
        SoundBin = soundBin;
        EtcRes = etcRes;
        Font3 = font3;

        _entityEventHandlers = new EntityEventHandlers(this);
        _gameInitializer = new GameInitializer(this);
        _padManager = new PadManager(this);

        CdManager = new CdManager(this);
        EntityManager = new EntityManager(this);
        EntityGameplayManager = new EntityGameplayManager(this);
        EffectManager = new EffectManager(this);
        GraphicManager = new GraphicManager(this);
        PlayerManager = new PlayerManager(this);
        Renderer = renderer;
        SoundManager = new SoundManager(this);
        StaticVariables = new StaticVariables();
        UIManager = new UIManager(this);
        MainInventoryManager = new MainInventoryManager(this);
        SubInventoryManager = new SubInventoryManager(this);
        HudManager = new HudManager(this);
        UIDebugManager = new UIDebugManager(this);
        MemoryCardManager = new MemoryCardManager(this);
        LogManager = new LogManager(this);
    }

    public void InitializeEngine(bool loadExtraDebugResources = true)
    {
        if (loadExtraDebugResources)
        {
            EntityNames.Load(EntityNames.Language.French);
        }

        Random.Reset();
        StaticVariables.Initialize(this);
        _gameInitializer.Initialize();
    }

    // 8002bfe0
    public void MainLoop()
    {
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
            StaticVariables.g_warpSoundEffectId = 0;

            if (StaticVariables.g_desiredMap != StaticVariables.g_currentMap)
            {
                StaticVariables.g_currentMap = StaticVariables.g_desiredMap;
                LoadMap(StaticVariables.g_currentMap); // Added by hand
                //ReadFileFromCDIntoBuffer(StaticVariables.DATAS_BIN, StaticVariables.g_compressedImageData, (StaticVariables.INT_801eab58)[StaticVariables.g_desiredMap], (StaticVariables.INT_801eab5c)[StaticVariables.g_desiredMap] - (StaticVariables.INT_801eab58)[StaticVariables.g_desiredMap]);
                InitializeMapSpriteTable(null, null, null);
                //LoadSpriteInfo(StaticVariables.g_compressedImageData[StaticVariables.g_mapIndexInDatasBin]);
                //SetEtcStrings(StaticVariables.g_compressedImageData[StaticVariables.g_animTableAlt_80191b48]);
                //InitializeScrollingData(StaticVariables.g_currentMap, g_currentMapBuffer.infoBlockOffset + g_currentMapBuffer.scrollingScreenOffset);

                //_datasBin.AlundraGameMap.SpriteInfo.Entities.Entities[0].PosX
                //StaticVariables.g_imageBuffer[0xc];
                //StaticVariables.g_imageBuffer[0xd];
                //StaticVariables.g_imageBuffer[0xe];

                byte x = 0; //CurrentMap.Info.E;
                byte y = 0; //CurrentMap.Info.F;
                byte z = 0; //CurrentMap.Info._10;

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
            InitializeItems(CurrentMap.Info._11); //StaticVariables.g_gameMapInfo->d
            LoadMapAndInitializeEntities(null);//((int)&g_currentMapBuffer.infoBlockOffset + g_currentMapBuffer.spriteSheetsOffset));
            WarpPlayer(playerPosX, playerPosY, playerPosZ, StaticVariables.g_mapTransitionEffectId);
            InitializeScrollingMode();
            HudManager.InitializeHudPositionBeforeHide();
            LoadMapSounds(StaticVariables.g_currentMap);
            Update(1);
            GraphicManager.ResetDebugRenderingState();
        }

        //do
        //{
        StaticVariables.g_debugMessage = "";
        //PrintDebug();
        RenderScene();

        if (IsRunning())
        {
            if (ReplayManager.ApplyCurrentFrame)
            {
                ReplayManager.PlayOneFrame(this);
            }
            else
            {
                Update(0);
                StaticVariables.FrameNumber++;
            }

            if (ReplayManager.IsSaving)
            {
                ReplayManager.SaveFrame(this);
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
            SoundManager.HandleMapSoundEffects(StaticVariables.g_desiredMap, StaticVariables.g_warpSoundEffectId);
            StaticVariables.g_warpSoundEffectId = 0;
            StartWarpTransition(StaticVariables.g_mapTransitionEffectId);
            StaticVariables.INT_800dc4e4 = 1;
            do
            {
                StaticVariables.g_debugMessage = "";
                _padManager.UpdatePads();
                //isEffectRunning = FUN_80044440(StaticVariables.g_orderingTableBuffer + 3, StaticVariables.g_mapTransitionEffectId);
                SoundManager.HandleMapSoundStreaming();
                //PauseGameDuringNbFrame(1);
                //DoNothing();
            } while (isEffectRunning != 0);

            EndGame();
            //FUN_80049ff8(); //sound
            if (StaticVariables.g_mapTransitionEffectId != 9)
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
            InitializeMapWarpPosition();
            //}
            //if (9 < StaticVariables.g_mapTransitionEffectId)
            //{
            //    if (StaticVariables.g_mapTransitionEffectId != 10)
            //    {
            //        if (StaticVariables.g_mapTransitionEffectId == 0xb)
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
            //if (StaticVariables.g_mapTransitionEffectId == 8)
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

    private bool IsRunning()
    {
        return !StaticVariables.IsGamePaused || StaticVariables.DoNextFrame || ReplayManager.ApplyCurrentFrame;
    }

    // 8002bd60
    private void RenderScene()
    {
        if (Renderer == null)
        {
            return;
        }

        GraphicManager.RenderScene();
    }

    //8005d668
    public void SetScrollingMode(int animationMode, int animationBankIndex)
    {
        StaticVariables.g_tileAnimationMode = animationMode;
        StaticVariables.g_tileAnimationType = animationBankIndex;
        StaticVariables.g_animationFrameCounter = 1;
        StaticVariables.g_tileOffset = 0;

        if (0 < animationBankIndex)
        {
            Debugger.Break();
            //
            // g_animationData =
            //    (int)g_scrollingParameters2 +
            //    (animationBankIndex + -1) * 0x10 + g_scrollingParameters->offsetX;
        }
    }

    // 
    private void InitializeStaticVariable()
    {
        StaticVariables.g_mapLimits = 0x3c;
        StaticVariables.g_debugFrameDelay = 0;
        StaticVariables.g_debugFlags &= 0xf7ffff3f;
    }

    // 8002cc58
    private void InitializeMapSpriteTable(byte[] buffer, ushort[] vramTable, byte[] otherPtr)
    {
        //already loaded in GameMap
    }

    private void LoadSpriteInfo(SpriteRecord spriteRecord)
    {
        InitializeSpriteInfo(StaticVariables.g_currentMapSpriteInfo, spriteRecord);
    }

    //8002d808
    private void InitializeSpriteInfo(SpriteInfoHeader spriteInfoHeader, SpriteRecord spriteRecord)
    {
        //Loaded in GameMap
    }

    //800423ec
    private void SetEtcStrings(string[] strings)
    {
        //StaticVariables.g_etcStrings = strings;
    }

    //8008159c
    private void ClearGlobalFlags()
    {
        var i = 0x3f;

        do
        {
            StaticVariables.g_globalFlags[i] = 0;
            i -= 1;
        } while (i >= 0);
    }

    //8002cd54
    private void ResetCameraAndLoadVRAMAssets()
    {
        StaticVariables.g_scrollingParameters.Flag = 0;
        StaticVariables.g_renderTileRowCount = 0x3c;
        StaticVariables.g_isCameraScrolling = 1;
        StaticVariables.g_cameraDebugOffsetY = 0;
        StaticVariables.g_cameraDebugOffsetX = 0;
        //LoadVRAMAssets();
    }

    //80044520
    private void InitializeItems(int threshold)
    {
        var i = 2;

        StaticVariables.g_itemIdThreshold = threshold;

        do
        {
            StaticVariables.g_itemBalanceRecords[i].ItemId = 0;
            i -= 1;
        } while (i >= 0);
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
            Array.Clear(StaticVariables.g_mapEvents[i].EventData.Parameters);

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
            //programBMapCode = pEmptyMapEvent.EventData.Parameters;
            //mapEventDest.EventData.Sp = StaticVariables.g_emptyMapEvent.EventData.Codes[1];
            //mapEventDest.EventData.Parameters = programBMapCode;
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
                StaticVariables.g_mapEvents[i].Entity = StaticVariables.PlayerEntity;
                StaticVariables.g_mapEvents[i].MapEventRecord = mapEventRecord;
                StaticVariables.g_mapEvents[i].ProgramBMap = programBMapCode;
                StaticVariables.g_mapEvents[i].EventData = new EventProgramState();
                StaticVariables.g_mapEvents[i].Id = i;
            }

            i++;
        }
    }

    public void LoadMap(uint mapId)
    {
        CurrentMap = DatasBin.GameMaps[mapId];
        using var br = DatasBin.OpenBin();
        CurrentMap.Load(br);

        if (!CurrentMap.Loaded)
        {
            using var reader = DatasBin.OpenBin();
            CurrentMap.Load(reader);
            SoundBin.OpenMap(mapId);
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

    public void InitializeEntitySlots()
    {
        EntityManager.InitializeEntitySlots();

        StaticVariables.g_numberOfEntities = 0;

        ResetEntityState();

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
            StaticVariables.g_resetAnimationId,
            StaticVariables.g_resetDirectionId,
            0xb, 0x60);

        StaticVariables.PlayerEntity.Status = 2;
        StaticVariables.PlayerEntity.HpMax = PlayerManager.GetPlayerHpMax();
        StaticVariables.PlayerEntity.Hp = PlayerManager.GetPlayerHp();
        StaticVariables.g_activeCollisionEntity = null;
        StaticVariables.g_playerWarpTimer = 0;
        StaticVariables.g_isWarpDisabled = 0;
        StaticVariables.g_playerWarpEffect = null;
        var weaponItemId = PlayerManager.GetItemIdFromCurrentWeapon();
        StaticVariables.g_currentWeaponFlags = StaticVariables.g_weaponFlagsByItemId[weaponItemId];
        Array.Clear(StaticVariables.g_playerEffectTransitionCooldown);
        ResetWarpLockTimer();
    }

    //8003295c
    private void ResetWarpLockTimer()
    {
        StaticVariables.g_warpLockTimer = 0;
    }

    // 8003a1b8
    public Entity SpawnEntity(Entity parent, int spriteInfoEntityIndex, int notCheckSpawnZone)
    {
        var entityRecord = GetEntityRecord(spriteInfoEntityIndex);

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
            return null;
        }

        entity.IsMapSprite = isMapSprite;
        int spriteTableIndex = entityRecord.SpriteTableIndex;
        if (isMapSprite)
        {
            spriteTableIndex |= 0x100;
        }

        var directionIndex = entityRecord.SpriteDirection & 0x3;

        var tileHalfWidth = StaticVariables.MapTileWidth / 2;
        var tileHalfHeight = StaticVariables.MapTileHeight / 2;

        EntityManager.InitializeEntity(
            entity, parent,
            spriteRecord, entityRecord, (uint)spriteTableIndex, spriteInfoEntityIndex,
            (entityRecord.XPos * tileHalfWidth + tileHalfWidth) * 0x10000,
            (entityRecord.YPos * tileHalfHeight + tileHalfHeight) * 0x10000,
            entityRecord.Height << 0x13,
            0,
            StaticVariables.g_cardinalDirectionTable[directionIndex],
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

        if (spriteTableIndex < 0 || spriteTableIndex >= spriteInfo.SpriteTable.Length)
        {
            Debugger.Break();
        }

        var sprite = spriteInfo.SpriteRecords[spriteTableIndex];
        return sprite;
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
                    flags = StaticVariables.g_saveData.MapFlags;
                }
                else
                {
                    flags = StaticVariables.g_globalFlags;
                }

                var index = ((contents >> 3) & 0xffc) >> 2;
                var mask = 1 << (val & 0x1f);

                if ((flags[index] & mask) == 0)
                {
                    entity.ContentsItemId = (uint)ChooseRandomlyAnItem(entity.EntityRecord.Contents);
                    return;
                }
            }

            if (entity.EntityRecord.Contents != 0)
            {
                entity.ContentsItemId = (uint)ChooseRandomlyAnItem(entity.EntityRecord.Contents);
                return;
            }
        }

        entity.ContentsItemId = (uint)ChooseRandomlyAnItem(entity.SpriteRecord.Header.Contents);
    }

    //80032968
    public int ChooseRandomlyAnItem(ushort contentId)
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

            var rand = (uint)(Random.Next() % 17);
            var index = ((contentId & 0x7F) << 4) | rand;

            index = (int)((Random.Next() * 0x10) >> 0x20) + (contentId & 0x7f) * 0x10;
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

        StaticVariables.g_warpFadeColorR_Target = posX << 16;
        StaticVariables.g_warpFadeColorG_Target = posY << 16;
        StaticVariables.g_warpFadeColorB_Target = posZ << 16;
        StaticVariables.g_fadeFrameCounter = 0;
        StaticVariables.g_fadeStepFlags = 0;
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
                StaticVariables.g_fadeStepFlags = 1;
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
                StaticVariables.g_fadeStepFlags = 1;
                break;

            case 4:
                StaticVariables.g_currentFadeColorR = 0xff0000;
                StaticVariables.g_currentFadeColorG = 0xff0000;
                StaticVariables.g_currentFadeColorB = 0xff0000;
                StaticVariables.g_targetFadeColorR = 0;
                StaticVariables.g_targetFadeColorG = 0;
                StaticVariables.g_targetFadeColorB = 0;
                StaticVariables.g_fadeStepFlags = 1;
                ApplyScreenFade(1, 8);
                StaticVariables.g_fadeColorStepB >>= 1;
                StaticVariables.g_fadeColorStepG >>= 1;
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
                StaticVariables.g_fadeStepFlags = 1;
                break;

            case 8:
                StaticVariables.g_currentFadeColorR = 0xff0000;
                StaticVariables.g_currentFadeColorG = 0xff0000;
                StaticVariables.g_currentFadeColorB = 0xff0000;
                StaticVariables.g_targetFadeColorR = 0;
                StaticVariables.g_targetFadeColorG = 0;
                StaticVariables.g_targetFadeColorB = 0;
                StaticVariables.g_fadeStepFlags = 1;
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
        if (StaticVariables.g_fadeStepFlags == 0)
        {
            StaticVariables.g_fadeColorStepR = 0;
            StaticVariables.g_fadeColorStepG = 0;
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

            StaticVariables.g_fadeColorStepG = (StaticVariables.g_targetFadeColorG - StaticVariables.g_currentFadeColorG) / fadeDuration;
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
            StaticVariables.g_warpFadeColorB_Step = 0;
            StaticVariables.g_warpFadeColorG_Step = 0;
            StaticVariables.g_warpFadeColorR_Step = 0;
            StaticVariables.g_warpFadeColorR = StaticVariables.g_warpFadeColorR_Target;
            StaticVariables.g_warpFadeColorG = StaticVariables.g_warpFadeColorG_Target;
            StaticVariables.g_warpFadeColorB = StaticVariables.g_warpFadeColorB_Target;
        }
        else
        {
            StaticVariables.g_warpFadeColorR_Step = (StaticVariables.g_warpFadeColorR_Target - StaticVariables.g_warpFadeColorR) / fadeDuration;
            if (fadeDuration == 0)
            {
                //Trap(0x1c00);
            }
            if (fadeDuration == -1 && StaticVariables.g_warpFadeColorR_Target - StaticVariables.g_warpFadeColorR == -0x80000000)
            {
                //Trap(0x1800);
            }

            StaticVariables.g_warpFadeColorG_Step = (StaticVariables.g_warpFadeColorG_Target - StaticVariables.g_warpFadeColorG) / fadeDuration;
            if (fadeDuration == 0)
            {
                //Trap(0x1c00);
            }
            if (fadeDuration == -1 && StaticVariables.g_warpFadeColorG_Target - StaticVariables.g_warpFadeColorG == -0x80000000)
            {
                //Trap(0x1800);
            }

            StaticVariables.g_warpFadeColorB_Step = (StaticVariables.g_warpFadeColorB_Target - StaticVariables.g_warpFadeColorB) / fadeDuration;
            if (fadeDuration == 0)
            {
                //Trap(0x1c00);
            }
            if (fadeDuration == -1 && StaticVariables.g_warpFadeColorB_Target - StaticVariables.g_warpFadeColorB == -0x80000000)
            {
                //Trap(0x1800);
            }
        }
    }

    // 8005b63c
    private void InitializeScrollingMode()
    {
        SetScrollingMode(3, 0);
        StaticVariables.g_tileOffset = 0;
        StaticVariables.g_animationCounter = 2;
    }

    // 8004a09c
    private int LoadMapSounds(uint mapId)
    {
        SoundManager.LoadMapSounds(mapId);
        HudManager.InitializeHudPositionBeforeHide();
        return 1;
    }

    //80049d3c
    public int GetSoundOffsetByMapId(uint mapId)
    {
        int currentMapId;
        var i = 0;

        if (StaticVariables.g_SoundOffsetList[0] != 0)
        {
            var soundOffsetList = StaticVariables.g_SoundOffsetList;
            currentMapId = soundOffsetList[i];

            do
            {
                if (mapId == currentMapId)
                {
                    var flags = StaticVariables.g_saveData.MapFlags;

                    if ((soundOffsetList[i + 1] & 0x8000) != 0)
                    {
                        flags = StaticVariables.g_globalFlags;
                    }

                    var index = ((soundOffsetList[i + 1] >> 3) & 0xffc) >> 2;

                    if ((flags[index] & (1 << (int)(soundOffsetList[i + 1] & 0x1f))) != 0)
                    {
                        return soundOffsetList[i + 2];
                    }
                }

                i += 3;
                currentMapId = soundOffsetList[i];
            }
            while (currentMapId != 0);
        }

        return StaticVariables.g_defaultSoundOffsetList[mapId];
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
        StaticVariables.g_fadeStepFlags = 0;
        StaticVariables.g_warpFlags = 0;

        switch (warpType)
        {
            case 0:
            case 9:
                InitStandardWarpEffect();
                break;
            case 2:
            case 10:
                InitUnknownWarpEffect();
                break;
            case 4:
                InitInstantWarpEffect();
                break;
            case 5:
                InitFadeOutWarp();
                break;
            case 6:
                InitSpecialWarpEffect();
                break;
            case 8:
                InitializeMapChangeWarp();
                break;
            case 11:
                InitializeCutsceneWarp();
                break;
            default:
                FUN_80042f18();
                break;
        }
    }

    //80043540
    private void InitializeCutsceneWarp()
    {
        //DR_MOVE effectPtr, effectPtr2;
        //int i = 0;
        //
        //do
        //{
        //    effectPtr = StaticVariables.g_drMoveBuffer[i];
        //    effectPtr2 = StaticVariables.g_drMoveBuffer[300 + i];
        //    effectPtr2.x0 = 0;
        //    effectPtr.x0 = 0;
        //    effectPtr2.w = 0x140;
        //    effectPtr.w = 0x140;
        //    effectPtr2.h = 1;
        //    effectPtr.h = 1;
        //    effectPtr2.sx = 0x140;
        //    effectPtr.sx = 0x140;
        //    i = i + 1;
        //} while (i < 0xf0);

        StaticVariables.g_warpEffectBuffer[1] = (short)((StaticVariables.g_warpEffectBuffer[1] & 0x0000_FFFF) | (0xEF << 16));
        StaticVariables.g_targetFadeColorR = 0xff0000;
        StaticVariables.g_targetFadeColorG = 0xff0000;
        StaticVariables.g_targetFadeColorB = 0xff0000;
        StaticVariables.g_currentFadeColorR = 0;
        StaticVariables.g_currentFadeColorG = 0;
        StaticVariables.g_currentFadeColorB = 0;
        StaticVariables.g_fadeStepFlags = 1;
        ApplyScreenFade(2, 300);
    }

    //80043458
    private void InitializeMapChangeWarp()
    {
        int rowIndex;
        int columnOffset;
        uint columnIndex;
        int frameOffset;
        int tableOffset;

        frameOffset = 0;
        tableOffset = 0;
        columnOffset = 0;

        do
        {
            columnIndex = 0;
            //columnOffset = tableOffset;
            int warpPatternIndex = 0;

            do
            {
                if ((columnIndex & 1) == 0)
                {
                    rowIndex = StaticVariables.g_mapWarpPattern[warpPatternIndex] + (0xe - frameOffset) * 2;
                }
                else
                {
                    rowIndex = StaticVariables.g_mapWarpPattern[warpPatternIndex] + frameOffset * 2;
                }

                StaticVariables.g_warpEffectBuffer[columnOffset] = (short)rowIndex;
                StaticVariables.g_warpEffectBuffer[columnOffset + 1] = 0;
                StaticVariables.g_warpEffectBuffer[columnOffset + 2] = 0;
                StaticVariables.g_warpEffectBuffer[columnOffset + 3] = 0;
                columnOffset += 4;
                columnIndex += 1;
                warpPatternIndex++;

            } while ((int)columnIndex < 0x14);

            frameOffset += 1;
            tableOffset += 0xa0;

        } while (frameOffset < 0xf);

        StaticVariables.g_targetFadeColorR = 0xf00000;
        StaticVariables.g_targetFadeColorG = 0xf00000;
        StaticVariables.g_currentFadeColorR = 0;
        StaticVariables.g_currentFadeColorG = 0;
        StaticVariables.g_currentFadeColorB = 0;
        StaticVariables.g_targetFadeColorB = 0;
        StaticVariables.g_fadeFrameCounter = 1;
        StaticVariables.g_fadeStepFlags = 1;
        StaticVariables.g_warpFadeColorB_Target = 0;
        StaticVariables.g_warpFadeColorG_Target = 0;
        StaticVariables.g_warpFadeColorR_Target = 0;
        ApplyScreenFade(2, 0xb4);
    }

    //800432a4
    private void InitSpecialWarpEffect()
    {
        ulong uVar1;
        int iVar2;
        ulong uVar3;
        int iVar4;
        int iVar5;
        int iVar6;
        int iVar7;

        iVar6 = 0;
        iVar7 = 0;

        Debugger.Break();

        do
        {
            iVar4 = 0;
            iVar5 = iVar7;

            do
            {
                if (iVar4 < 10)
                {
                    iVar2 = iVar4 + 7;
                }
                else
                {
                    iVar2 = 0x1a - iVar4;
                }

                if (7 - iVar6 < 0)
                {
                    iVar2 = iVar2 + 7 - iVar6;
                }
                else
                {
                    iVar2 = iVar2 + -7 + iVar6;
                }

                StaticVariables.g_warpEffectBuffer[iVar5] = (short)(iVar2 * -2);
                uVar3 = Random.Next();
                uVar1 = Random.Next();
                iVar4 += 1;
                StaticVariables.g_warpEffectBuffer[iVar5 + 4] = (short)(0x40 - (short)((uVar3 * 0x81) >> 0x20));
                StaticVariables.g_warpEffectBuffer[iVar5 + 6] = (short)(-0x10 - (short)((uVar1 * 0x41) >> 0x20));
                iVar5 += 8;
            } while (iVar4 < 0x14);

            iVar6 += 1;
            iVar7 += 0xa0;

        } while (iVar6 < 0xf);

        StaticVariables.g_warpFadeColorG = 0xff0000;
        StaticVariables.g_warpFadeColorR = 0xff0000;
        StaticVariables.g_warpFadeColorB = 0;
        StaticVariables.g_warpFadeColorB_Target = 0;
        StaticVariables.g_warpFadeColorG_Target = 0;
        StaticVariables.g_warpFadeColorR_Target = 0;
        StaticVariables.g_warpFlags = 1;
        StaticVariables.g_targetFadeColorG = 0xff0000;
        StaticVariables.g_targetFadeColorB = 0xff0000;
        StaticVariables.g_currentFadeColorR = 0;
        StaticVariables.g_currentFadeColorG = 0;
        StaticVariables.g_currentFadeColorB = 0;
        StaticVariables.g_targetFadeColorR = 0;
        StaticVariables.g_fadeFrameCounter = 1;
        StaticVariables.g_fadeStepFlags = 1;
        ApplyScreenFade(2, 100);
    }

    //8004320c
    private void InitFadeOutWarp()
    {
        //DR_MOVE* drMovePtr;
        //int entityIndex;
        //
        //entityIndex = 0;
        //drMovePtr = g_drMoveBuffer;
        //
        //do
        //{
        //    drMovePtr[300].h = 1;
        //    drMovePtr->h = 1;
        //    drMovePtr[300].sy = (short)entityIndex;
        //    drMovePtr->sy = (short)entityIndex;
        //    entityIndex = entityIndex + 1;
        //    drMovePtr = drMovePtr + 1;
        //} while (entityIndex < 0xf0);

        StaticVariables.g_warpEffectBuffer[0] = 0;
        StaticVariables.g_targetFadeColorR = 0xff0000;
        StaticVariables.g_targetFadeColorG = 0xff0000;
        StaticVariables.g_targetFadeColorB = 0xff0000;
        StaticVariables.g_currentFadeColorR = 0;
        StaticVariables.g_currentFadeColorG = 0;
        StaticVariables.g_currentFadeColorB = 0;
        StaticVariables.g_fadeFrameCounter = 1;
        StaticVariables.g_fadeStepFlags = 1;
        StaticVariables.g_warpFadeColorB_Target = 0;
        StaticVariables.g_warpFadeColorG_Target = 0;
        StaticVariables.g_warpFadeColorR_Target = 0;
        ApplyScreenFade(2, 0x1e);
    }

    //80042f3c
    private void InitStandardWarpEffect()
    {
        StaticVariables.g_targetFadeColorR = 0xff0000;
        StaticVariables.g_targetFadeColorG = 0xff0000;
        StaticVariables.g_targetFadeColorB = 0xff0000;
        StaticVariables.g_currentFadeColorR = 0;
        StaticVariables.g_currentFadeColorG = 0;
        StaticVariables.g_currentFadeColorB = 0;
        StaticVariables.g_fadeStepFlags = 1;
        ApplyScreenFade(2,0x10);
    }

    //80042f8c
    private void InitUnknownWarpEffect()
    {
        StaticVariables.g_targetFadeColorR = 0xff0000;
        StaticVariables.g_targetFadeColorG = 0xff0000;
        StaticVariables.g_targetFadeColorB = 0xff0000;
        StaticVariables.g_currentFadeColorR = 0;
        StaticVariables.g_currentFadeColorG = 0;
        StaticVariables.g_currentFadeColorB = 0;
        StaticVariables.g_fadeStepFlags = 1;
        ApplyScreenFade(1, 0x10);
    }

    //80042fdc
    private void InitInstantWarpEffect()
    {
        ulong randomSeed1;
        ulong randomSeed2;
        ulong randomSeed3;
        int innerLoopCounter;
        int offsetY;
        int offsetX;
        int iterationCounter = 0;
        int outerLoopCounter;
        int tableOffset;

        outerLoopCounter = 0;
        offsetY = -0x70;
        tableOffset = 0;

        do
        {
            innerLoopCounter = 0;
            offsetX = -0x98;
            //iterationCounter = tableOffset;

            do
            {
                randomSeed1 = Random.Next();
                randomSeed2 = Random.Next();
                randomSeed3 = Random.Next();
                StaticVariables.g_warpEffectBuffer[iterationCounter * 4 + 0] = (short)(-(short)((randomSeed1 * 0x15) >> 0x20) - (short)((offsetX * offsetX + offsetY * offsetY) >> 10));
                StaticVariables.g_warpEffectBuffer[iterationCounter * 4 + 1] = (short)((short)((randomSeed2 * 0x15) >> 0x20) + 0x14);
                StaticVariables.g_warpEffectBuffer[iterationCounter * 4 + 2] = (short)((randomSeed3 * 0x130) >> 0x20);
                StaticVariables.g_warpEffectBuffer[iterationCounter * 4 + 3] = (short)((uint)((randomSeed3 * 0xe0) >> 0x20));
                offsetX += 0x10;
                iterationCounter += 1;
                innerLoopCounter += 1;
            } while (innerLoopCounter < 0x14);

            offsetY += 0x10;
            outerLoopCounter += 1;
            tableOffset += 0xa0;

        } while (outerLoopCounter < 0xf);

        StaticVariables.g_targetFadeColorR = 0x400000;
        StaticVariables.g_targetFadeColorG = 0x400000;
        StaticVariables.g_targetFadeColorB = 0x200000;
        StaticVariables.g_fadeFrameCounter = 1;
        StaticVariables.g_fadeStepFlags = 1;
        StaticVariables.g_currentFadeColorR = 0;
        StaticVariables.g_currentFadeColorG = 0;
        StaticVariables.g_currentFadeColorB = 0;
        StaticVariables.g_warpFadeColorB_Target = 0xff0000;
        StaticVariables.g_warpFadeColorG_Target = 0xff0000;
        StaticVariables.g_warpFadeColorR_Target = 0xff0000;
        ApplyScreenFade(1, 0x50);
    }

    //80042f18
    private void FUN_80042f18()
    {
        ApplyScreenFade(2, 1);
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

    //800315b0
    private void InitializeMapWarpPosition()
    {
        if (StaticVariables.g_saveDataInRam.SaveSlotIndex != 0xff)
        {
            StaticVariables.g_saveDataInRam.SaveSlotIndex += 1;
        }

        _gameInitializer.InitializePlayerStatsAndItems();
        UpdateSaveData();
        StaticVariables.g_resetAnimationId = 0x36;
        StaticVariables.g_resetDirectionId = 0;
        StaticVariables.g_desiredMap = StaticVariables.g_saveData.InitialMapId;
        StaticVariables.g_cameraLookAtX = (StaticVariables.g_saveData.CameraTileX * 0x18 + 0xc) * 0x10000;
        StaticVariables.g_cameraTargetX = StaticVariables.g_cameraLookAtX;
        StaticVariables.g_cameraLookAtY = (StaticVariables.g_saveData.CameraTileY * 0x10 + 8) * 0x10000;
        StaticVariables.g_cameraTargetY = StaticVariables.g_cameraLookAtY;
        StaticVariables.g_cameraLookAtZ = StaticVariables.g_saveData.CameraTileZ << 0x14;
        StaticVariables.g_cameraTargetZ = StaticVariables.g_saveData.CameraTileZ << 0x14;
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
            StaticVariables.PlayerEntity.BlockedByEntity == null &&
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
                    StaticVariables.g_playerDataHud[0] = 0;
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
            StaticVariables.PlayerEntity.BlockedByEntity == null &&
            StaticVariables.g_warpLockTimer == 0 &&
            (StaticVariables.g_padState1.ButtonsJustPressed & PadState.OpenInventory) != 0 &&
            StaticVariables.g_warpDelayFrames == 0 &&
            (StaticVariables.g_padState1.ButtonsHold & PadState.Select) == 0 &&
            StaticVariables.g_globalTransitionState == 0 &&
            MainInventoryManager.DisplayInventory() == 0)
        {
            StaticVariables.g_isGameEnding = 1;
        }

        //HandleMapSoundStreaming();

        Random.Next();

        if (endGame != 0)
        {
            StaticVariables.g_isGameEnding = 0;
        }
    }

    //80047c8c
    public uint CheckSpecialWarpCondition(int index)
    {
        return (uint)StaticVariables.g_callbackTable[index].Flags & 1;
    }

    //80051f1c
    public void TriggerWarpTypeA()
    {
        StaticVariables.UINT_8017e8d8 = 0;
        StaticVariables.DAT_8017e990 = 0;
        StaticVariables.DAT_8017e9ac = 0;
        StaticVariables.DAT_8017e998 = 0x4f824f82;
        StaticVariables.DAT_8017e99c = 0x4f82;
        StaticVariables.DAT_8017e99e = 0;
        GraphicManager.SetTransitionType(0xb);
    }

    //80051f1c
    public void ActivateDebugSoundMenu()
    {
        //StaticVariables.g_SE_BGM_array //800a82c0
        string[] args = ["SE", "BGM"];
        StartAsyncCallback(SetDisplaySoundMenuCallback, 1, ref args);
    }

    //80050670
    void SetDisplaySoundMenuCallback(int param_1)
    {
        int transitionType;

        if (param_1 == 2)
        {
            transitionType = 8;
        }
        else
        {
            transitionType = 5;

            if (param_1 != 1)
            {
                return;
            }
        }

        GraphicManager.SetTransitionType(transitionType);
    }

    //8002e058
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
            var programId = mapEventEntity.MapEventProgramId;

            var record = currentMapEvent.MapEventRecord;
            var px = playerEntity.TileX;
            var py = playerEntity.TileY;

            if (px < record.X1 || px > record.X2 || py < record.Y1 || py > record.Y2)
            {
                mapEventEntity.ChildEntity = null;
                mapEventEntity.EventProgramState.Sp = 0;
                mapEventEntity.RelativeWarpOffsetX = 0;
                mapEventEntity.Index = playerEntity.Index;
                continue;
            }

            playerEntity.ProgramIndexes[ScriptHelper.ProgramBMap] = currentMapEvent.ProgramBMap;
            playerEntity.MapEventProgramId = currentMapEvent.ProgramBMap;

            playerEntity.EventTrigger = i;
            playerEntity.LogicContextEntity = mapEventEntity;
            playerEntity.EventProgramState.CopyFrom(currentMapEvent.EventData);

            LogManager.SetCategory($"MapEvent {i}");

            RunScript(playerEntity, ScriptHelper.ProgramBMap);

            currentMapEvent.EventData.CopyFrom(playerEntity.EventProgramState);
            currentMapEvent.Entity = playerEntity.LogicContextEntity;
            currentMapEvent.ProgramBMap = playerEntity.ProgramIndexes[ScriptHelper.ProgramBMap];
        }

        LogManager.ResetCategory();
    }

    public SpriteEffectRecord GetEffectSpriteFromSpriteTable(bool isMapSprite, int spriteTableIndex, out int addedtosheet, out int addedtopallette)
    {
        SpriteInfo spriteInfo;
        if (isMapSprite)
        {
            spriteInfo = CurrentMap.SpriteInfo;
            addedtosheet = 0;
            addedtopallette = 0x20;
        }
        else
        {
            spriteInfo = DatasBin.AlundraGameMap.SpriteInfo;
            addedtosheet = 0xb;
            addedtopallette = 0x60;
        }
        if (spriteTableIndex >= 0 && spriteTableIndex < spriteInfo.SpriteEffectRecords.Length)
        {
            return spriteInfo.SpriteEffectRecords[spriteTableIndex];
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

    // 8003a774
    public void DestroyEntity(Entity entity)
    {
        //SpawnEntityContents(entity);
        //
        //entity.Status = 4;
        //entity.EventTrigger = -1;
        //
        //if (entity.ActiveEffect != null)
        //{
        //    entity.ActiveEffect.CurrentSpriteTableIndex = 0;
        //    entity.ActiveEffect = null;
        //}
        //
        //if (entity.PlatformEntity != null)
        //{
        //    entity.PlatformEntity.CarriedEntity = null;
        //}

        DestroyEntity(entity, -2);
    }

    // 8003a59c
    public void DestroyEntity(Entity entity, int effectId)
    {
        LogManager.Log(entity, $"to destroy => status:{entity.Status} flags:{entity.Flags} Bytes:{string.Join('-', entity.Bytes)} AIValues:{string.Join('-', entity.AIValues)}");

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
        //Debugger.Break();
        if (StaticVariables.g_itemDropProperties[entity.ContentsItemId].Field1 == 0)
        {
            delay = -1;
        }

        spawnedEntity.DelayOrAngle = delay;
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
    public int GetMatchingEntityBySearchType(Entity ownerEntity, int searchType)
    {
        var matchCount = 0;

        if ((searchType & 0x80) == 0)
        {
            GetEntityRecord(searchType);
            
            foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
            {
                if (ownerEntity.Status - 1 < 3 && entity.EntityRefId == searchType)
                {
                    StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                }
            }

            return matchCount;
        }

        var functionId = searchType & 0x7f;

        switch (functionId)
        {
            case 0://get owner
                StaticVariables.g_matchingEntitiesBuffer[matchCount++] = ownerEntity;
                break;

            case 1://get player
                StaticVariables.g_matchingEntitiesBuffer[matchCount++] = StaticVariables.PlayerEntity;
                break;

            case 2://get all entities
                for (int i = 0; i < StaticVariables.g_numberOfEntities; i++)
                {
                    var entity = StaticVariables.g_entitySlots[i];

                    if (entity.Status - 1 < 3)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }
                }

                break;

            case 3://get all entities except player
                for (int i = 0; i < StaticVariables.g_numberOfEntities; i++)
                {
                    var entity = StaticVariables.g_entitySlots[i];

                    if (entity.Status - 1 < 3)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }
                }

                break;

            case 4://all entities on the ground
                for (int i = 0; i < StaticVariables.g_numberOfEntities; i++)
                {
                    var entity = StaticVariables.g_entitySlots[i];

                    if (ownerEntity.Status - 1 < 3
                        && (entity.Flags & 0x80) != 0
                        && (entity.AnimFlags & 0x80) == 0
                        && entity.PlatformEntity == null)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }
                }

                break;

            case 5://all entities besides player that the ownerentity is riding on
                for (int i = 1; i < StaticVariables.g_numberOfEntities; i++)
                {
                    var entity = StaticVariables.g_entitySlots[i];

                    if (entity.Status - 1 < 3 && ownerEntity.RidingEntity == entity)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }
                }

                break;

            case 6://all entities besides player that are riding on the ownerentity
                for (int i = 1; i < StaticVariables.g_numberOfEntities; i++)
                {
                    var entity = StaticVariables.g_entitySlots[i];

                    if (entity.Status - 1 < 3 && entity.RidingEntity == ownerEntity)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }
                }

                break;

            case 7://all entities besides player where ownerentity.xcollision? == entity
                foreach (var entity in StaticVariables.g_entitySlots.Skip(1))
                {
                    if (entity.Status - 1 < 3 && ownerEntity.XCollisionEntity == entity)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }

                }

                break;

            case 8://all entities besides player where entity.xcollision? == ownerentity
                for (int i = 1; i < StaticVariables.g_numberOfEntities; i++)
                {
                    var entity = StaticVariables.g_entitySlots[i];

                    if (entity.Status - 1 < 3 && entity.XCollisionEntity == ownerEntity)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }

                }

                break;

            case 9://all entities besides player where entity.ownerentity [c] == ownerentity
                for (int i = 1; i < StaticVariables.g_numberOfEntities; i++)
                {
                    var entity = StaticVariables.g_entitySlots[i];

                    if (entity.Status - 1 < 3 && entity.ParentEntity == ownerEntity)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }

                }

                break;

            case 10://all entities besides player where ownerentity.ownerentity [c] == entity
                for (int i = 1; i < StaticVariables.g_numberOfEntities; i++)
                {
                    var entity = StaticVariables.g_entitySlots[i];

                    if (entity.Status - 1 < 3 && ownerEntity.ParentEntity == entity)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }

                }

                break;

            case 11://all entities besides player that are on a platform
                for (int i = 1; i < StaticVariables.g_numberOfEntities; i++)
                {
                    var entity = StaticVariables.g_entitySlots[i];

                    if (entity.Status - 1 < 3 && entity.PlatformEntity != null)
                    {
                        StaticVariables.g_matchingEntitiesBuffer[matchCount++] = entity;
                    }

                }

                break;

            default: //"Illegal Destination!"
                Debugger.Break();
                break;
        }

        return matchCount;
    }

    public SiEntityRecord GetEntityRecord(int id)
    {
        SiEntityRecord res;

        if (id < 0 || CurrentMap.SpriteInfo.Entities.Entities.Length <= id) // StaticVariables.g_maxEntityRecord
        {
            Debugger.Break();
            //"Illegal character initial data!!
            res = null;
        }
        else
        {
            res = CurrentMap.SpriteInfo.Entities.Entities[id];

            if (res.IsEnabled == 0)
            {
                res = null;
            }
        }

        return res;
    }

    //80039f58
    public Entity SpawnWarpEntity(Entity parentEntity, int isCurrentMapSprite, uint spriteTableIndex, int posX, int posY, int posZ, uint direction)
    {
        SpriteRecord spriteRecord;
        Entity entityResult = null;
        int paletteIndex;
        int sheetSize;

        spriteRecord = GetSpriteFromSpriteTable(isCurrentMapSprite != 0, spriteTableIndex, out paletteIndex, out sheetSize);

        if (spriteRecord != null)
        {
            entityResult = EntityManager.AllocateEntitySlot();

            if (entityResult != null)
            {
                if (isCurrentMapSprite != 0)
                {
                    spriteTableIndex += 0x100;
                }

                entityResult.IsMapSprite = isCurrentMapSprite != 0;

                EntityManager.InitializeEntity(entityResult, parentEntity,
                    spriteRecord, null, spriteTableIndex, -1,
                    posX, posY, posZ,
                    0, direction,
                    paletteIndex, sheetSize);

                LogManager.Log(entityResult, $"Spawned=> parent:[{(parentEntity == null ? "null" : parentEntity)}] x:{entityResult.PosX >> 16} y:{entityResult.PosY >> 16} z:{entityResult.PosZ >> 16} from:{(isCurrentMapSprite != 0 ? "currentMap" : "alundraMap")} spriteIndex:{spriteTableIndex}");
            }
        }

        return entityResult;
    }

    public void RunScript(Entity entity, int eventType)
    {
        _entityEventHandlers.RunScript(entity, eventType);
    }

    public void RunSpriteEvent(Entity entity)
    {
        var eventId = entity.SpriteProgramIndexes[entity.EventTrigger];
        _entityEventHandlers.SpriteHandlers.RunSpriteEvent(entity.EventTrigger, eventId, entity);
    }

    //8002d7b0
    public void ChangeAreaTileProperties(int tileId)
    {
        //Debugger.Break();
        var mapCopy = CurrentMap.Map.MapCopies[tileId]; //tileId - 2
        ChangeAreaTileProperties(mapCopy.FromX, mapCopy.FromY, mapCopy.Width, mapCopy.Height, mapCopy.ToX, mapCopy.ToY);
    }

    //8002d608
    public void ChangeAreaTileProperties(int startX, int startY, int sizeX, int sizeY, int destX, int destY)
    {
        int x;
        int y;

        if (startX < 0 || startY < 0 || sizeX < 0 || sizeY < 0 || destX < 0 || destY < 0)
        {
            Debugger.Break();
        }

        var mapWidth = CurrentMap.Map.Width;
        var mapHeight = CurrentMap.Map.Height;

        if (mapWidth < startX + sizeX
            || mapHeight < startY + sizeY
            || mapWidth < destX + sizeX
            || mapHeight < destY + sizeY)
        {
            Debugger.Break();
        }

        y = 0;

        if (0 < sizeY)
        {
            var map = CurrentMap.Map;

            do
            {
                x = 0;

                if (0 < sizeX)
                {
                    do
                    {
                        var tileDestination = map.MapTiles[destX + x + (destY + y) * mapWidth];
                        var tileSource = map.MapTiles[startX + x + (startY + y) * mapWidth];
                        tileDestination.Walkability = tileSource.Walkability;
                        tileDestination.GroundProperty = tileSource.GroundProperty;
                        tileDestination.Height = tileSource.Height;
                        tileDestination.TileId = tileSource.TileId;
                        tileDestination.WallTilesOffset = tileSource.WallTilesOffset;

                        if (tileSource.WallTiles != null)
                        {
                            //override in original source code
                            //if (tileSource.WallTiles.Tiles.Length != (tileDestination.WallTiles?.Tiles?.Length ?? 0))
                            //{
                            //    Debugger.Break();
                            //}
                            tileDestination.WallTiles ??= new WallTiles();
                            tileDestination.WallTiles.Tiles ??= new ushort[tileSource.WallTiles.Tiles.Length];

                            int length = Math.Min(tileSource.WallTiles.Tiles.Length, tileDestination.WallTiles.Tiles.Length);

                            tileDestination.WallTiles.Offset = tileSource.WallTiles.Offset;
                            tileDestination.WallTiles.Count = tileSource.WallTiles.Count;

                            if (tileDestination.WallTiles.Tiles.Length < tileSource.WallTiles.Tiles.Length)
                            {
                                tileDestination.WallTiles.Tiles = new ushort[tileSource.WallTiles.Tiles.Length];
                                length = tileSource.WallTiles.Tiles.Length;
                            }

                            Array.Copy(tileSource.WallTiles.Tiles, tileDestination.WallTiles.Tiles, length);
                        }

                        x++;
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
                result = StaticVariables.g_cardinalDirectionTable[encodedDir & 3];
                break;

            case 3:
                direction = (uint)ScriptHelper.GetDirectionToTarget(
                    StaticVariables.PlayerEntity.PosX - entity.PosX,
                    StaticVariables.PlayerEntity.PosY - entity.PosY);
                direction += encodedDir;
                goto LAB_8003d110;

            case 4:
                var rand = (int)((Random.Next() * 4) >> 0x20);
                result = StaticVariables.g_cardinalDirectionTable[rand];
                break;

            case 5:
                result = (uint)((Random.Next() * 0x20) >> 0x20);
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
    public Portal? GetActivatedPortal()
    {
        foreach (var infoPortal in CurrentMap.Info.Portals)
        {
            if (StaticVariables.PlayerEntity.TileX >= infoPortal.X1
                && StaticVariables.PlayerEntity.TileX <= infoPortal.X2
                && StaticVariables.PlayerEntity.TileY >= infoPortal.Y1
                && StaticVariables.PlayerEntity.TileY <= infoPortal.Y2)
            {
                if (infoPortal.DestMapId == 0)
                {
                    return null;
                    //Debugger.Break();
                }

                return infoPortal;
            }
        }

        return null;
    }

    //8003a7b0
    public void CheckAndTriggerTileEffect(Entity entity)
    {
        if (entity.FrameCollision == null)
        {
            return;
        }

        int[] worldXCoords = new int[4];
        int[] worldYCoords = new int[4];
        int tileZ, height;

        if (entity.HitBoxX >> 16 >= StaticVariables.g_tileToWorldXTable.Length
            || (entity.HitBoxX + entity.CollisionWidth) >> 16 >= StaticVariables.g_tileToWorldXTable.Length)
        {
            Debugger.Break();
            return;
        }

        var val = (short)entity.HitBoxX;
        worldXCoords[2] = (entity.HitBoxX >> 16) / StaticVariables.MapTileWidth; //StaticVariables.g_tileToWorldXTable[entity.HitBoxX >> 16];
        worldXCoords[0] = worldXCoords[2];
        worldXCoords[3] = ((entity.HitBoxX + entity.CollisionWidth) >> 16) / StaticVariables.MapTileWidth; //StaticVariables.g_tileToWorldXTable[(entity.HitBoxX + entity.CollisionWidth) >> 16];
        worldXCoords[1] = worldXCoords[3];

        worldYCoords[1] = (entity.HitBoxY >> 16) / StaticVariables.MapTileHeight;
        worldYCoords[0] = worldYCoords[1];
        worldYCoords[3] = ((entity.HitBoxY + entity.CollisionDepth) >> 20) / StaticVariables.MapTileHeight;
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
            var tileFlags = tile.Walkability | (tile.GroundProperty << 8);

            if ((tileFlags & 2) == 0)
            {
                continue;
            }

            int tileHeightWorld = (tile.Height << 20) + 0x80000; //0x80_000; //(tile.Height & 0xFF) << 20;
            //tileEffectZ = tile.Height * 0x100000;

            if (tileZ <= tileHeightWorld && tileHeightWorld <= tileZ + height)
            {
                tile.Walkability = (byte)(tileFlags & 0xFFFD);
                //tile.Height = (byte)((tileFlags & 0xFFFF) >> 8);
                tile.WallTilesOffset = -1; // tile.TileId = 0xFFFF;
                tile.WallTiles = null;

                int effectX = worldXCoords[i] * 0x180000 + 0xC0000; // center on tile
                int effectY = worldYCoords[i] * 0x100000 + 0x80000;

                EffectManager.CreateEffectEntity(0,
                    CurrentMap.Info.BalanceLevel, //C?
                    0,
                    effectX, effectY, tileHeightWorld);
                EffectManager.RandomlySpawnItem(0xFF, effectX, effectY, tileHeightWorld);
                SoundManager.PlaySoundEffect(0x1F);
            }
        }
    }

    //80059f6c
    public void TryOpenDialogWithName(int spriteTableIndex)
    {
        var tableIndex = spriteTableIndex - 256;

        if (tableIndex < 0
            || tableIndex >= EtcRes.StringTable.Length)
        {
            return;
        }

        if (tableIndex >= 0
            && tableIndex < EtcRes.StringTable.Length
            && EtcRes.StringTable[tableIndex] == null)
        {
            return;
        }

        if ((StaticVariables.g_UIDisplayFlags & 4) == 0
            && spriteTableIndex - 0x100U < 0x100)
        {
            StaticVariables.g_entitySpriteNameTableIndex = spriteTableIndex;
            GraphicManager.SetTransitionType(0xc);
        }
    }

    //800423f8
    public int TryOpenDialog(uint textId, int playerControlMode)
    {
        string[] strings;

        if (IsDialogFinished())
        {
            return 0;
        }

        strings = AlundraMap.Strings;

        if ((textId & 0x80) != 0)
        {
            strings = CurrentMap.Strings;
        }

        var text = strings[textId & 0x7f];

        DialogEmptyFunction();
        UIManager.InitializeDialogMessage(text, playerControlMode);

        return 1;
    }

    //80045004
    public bool IsDialogFinished()
    {
        return (StaticVariables.g_dialog_flags & 4) != 0;
    }

    //8008167c
    private void DialogEmptyFunction()
    {
        //empty function
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

        if (flag == 0)
        {
            return;
        }

        if ((flag & 0x8000) == 0)
        {
            flags = StaticVariables.g_saveData.MapFlags;
        }
        else
        {
            flags = StaticVariables.g_globalFlags;
        }

        var index = ((flag >> 3) & 0xffc) >> 2;
        flags[index] |= (uint)(1 << (int)(flag & 0x1f));
    }

    //8004248c
    public int IsDialogFinished2()
    {
        return IsDialogFinished() ? 1 : 0;
    }

    //80050ba8
    public int InitializeAsyncOperation(string arg1, string arg2, Action<int> callback)
    {
        StaticVariables.g_asyncCallbackArgs[0] = arg1;
        StaticVariables.g_asyncCallbackArgs[1] = arg2;
        StartAsyncCallback(callback, 1, ref StaticVariables.g_asyncCallbackArgs);
        //StartAsyncCallback(AsyncCallbackHandler, 1, ref StaticVariables.g_asyncCallbackArgs);
        //StaticVariables.g_asyncOperationResultPtr = result;
        StaticVariables.g_asyncOperationCountdown = 0;
        return 1;
    }

    //80050c00
    public int StartAsyncOperation(string arg1, string arg2, Action<int> callback)
    {
        StaticVariables.g_asyncCallbackArgs[0] = arg1;
        StaticVariables.g_asyncCallbackArgs[1] = arg2;
        StartAsyncCallback(callback, 1, ref StaticVariables.g_asyncCallbackArgs);
        //StartAsyncCallback(AsyncCallbackHandler, 1, ref StaticVariables.g_asyncCallbackArgs);
        //StaticVariables.g_asyncOperationResultPtr = result;
        StaticVariables.g_asyncOperationCountdown = 0;
        return 1;
    }

    //800505fc
    public void StartAsyncCallback(Action<int> asyncCallbackHandler, short i, ref string[] args)
    {
        StaticVariables.g_asyncCallbackCounter = i;
        StaticVariables.g_asyncCallback = asyncCallbackHandler;
        StaticVariables.g_asyncCallbackArgs2 = args;
        GraphicManager.InitializeFadeOverlaySprites(StaticVariables.g_sprites);
        SoundManager.PlaySoundEffect(4);
        GraphicManager.SetTransitionType(3);
    }

    //80050b98
    void AsyncCallbackHandler(int result)
    {
        //not used replace par Action<int> callback in InitializeAsyncOperation

        //StaticVariables.g_asyncOperationResultPtr
        //StaticVariables.g_asyncOperationResultPtr = result;
    }

    //8003153c
    public void UpdateSavedData(bool displayMenu = true)
    {
        StaticVariables.g_saveData.InitialMapId = StaticVariables.g_currentMap;
        StaticVariables.g_saveData.CameraTileX = StaticVariables.PlayerEntity.TileX;
        StaticVariables.g_saveData.CameraTileY = StaticVariables.PlayerEntity.TileY;
        StaticVariables.g_saveData.CameraTileZ = StaticVariables.PlayerEntity.TileZ;
        UpdateMenuStatusText();
        StaticVariables.g_saveData.GameTime = StaticVariables.g_gameplayTime;

        if (displayMenu)
        {
            InitializeSaveDataCopy(StaticVariables.g_saveData, 0x758, 1);
        }
    }

    //8005ec44
    private uint InitializeSaveDataCopy(SaveData source, int byteCount, int state)
    {
        uint result = 0xffffffff;

        if (StaticVariables.g_isMemoryCopyInProgress == 0)
        {
            StaticVariables.g_globalTransitionState = 10000;
            result = (uint)(byteCount < 0x76d ? 1 : 0);
            StaticVariables.g_saveDataSize = byteCount;
            StaticVariables.g_saveDataCopyPtr = source;
            StaticVariables.g_postProcessingState = state;

            if (result == 0)
            {
                StaticVariables.g_globalTransitionState = 0;
                result = 0xffffffff;
            }
        }

        return result;
    }

    //800814e8
    public void UpdateSaveData()
    {
        StaticVariables.g_saveData.CopyFrom(StaticVariables.g_saveDataInRam);
        //Array.Copy(StaticVariables.g_saveDataInRam, StaticVariables.g_saveData, 0x758);
    }

    //80030fc8
    private void UpdateMenuStatusText()
    {
        string template = "  HP 00       TIME 00:00:00   ";
        var chars = template.ToCharArray();

        // --- HP (de PlayerEntity) ---
        int hp = StaticVariables.PlayerEntity.Hp;
        if (hp < 0)
        {
            hp = 0;
        }

        if (hp > 99)
        {
            hp = 99;
        }

        chars[5] = (char)('0' + (hp / 10));
        chars[6] = (char)('0' + (hp % 10));
        int totalSeconds = (int)StaticVariables.g_gameplayTime;

        if (totalSeconds < 0)
        {
            totalSeconds = 0;
        }

        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        hours %= 100;
        minutes %= 100;
        seconds %= 100;

        chars[19] = (char)('0' + (hours / 10));
        chars[20] = (char)('0' + (hours % 10));
        chars[22] = (char)('0' + (minutes / 10));
        chars[23] = (char)('0' + (minutes % 10));
        chars[25] = (char)('0' + (seconds / 10));
        chars[26] = (char)('0' + (seconds % 10));

        StaticVariables.g_saveData.GameStateDescription = new string(chars);
    }

    //800450b0
    public void SetTextFlags(uint flags)
    {
        StaticVariables.g_textFlags = flags;
    }

    //80045088
    public void ActivateTextAutoAdvanceFlag()
    {
        if ((StaticVariables.g_textFlags & 4U) != 0)
        {
            StaticVariables.g_textAutoAdvanceFlag = 1;
        }
    }

    //800450e4
    public void SetDebugFlag(uint flags)
    {
        StaticVariables.g_debugFlags_2 = flags;
    }

    //800450bc
    public void ActivateDebugTextAutoAdvance()
    {
        if ((StaticVariables.g_debugFlags_2 & 4) != 0)
        {
            StaticVariables.g_textAutoAdvanceFlag_2 = 1;
        }
    }

    //80080a88
    public void TriggerScreenEffect(int fadeColor, int duration, int frameCount, int resetBackgroundColor)
    {
        if (resetBackgroundColor != 0)
        {
            StaticVariables.g_currentFadeColorR = 0;
            StaticVariables.g_currentFadeColorG = 0;
            StaticVariables.g_currentFadeColorB = 0;
        }
        StaticVariables.g_fadeStepFlags = 1;
        StaticVariables.g_fadeFrameCounter = frameCount;
        StaticVariables.g_targetFadeColorB = fadeColor;
        StaticVariables.g_targetFadeColorG = fadeColor;
        StaticVariables.g_targetFadeColorR = fadeColor;
        BeginFadeEffect(1, duration);
    }

    //8003abcc
    public bool ResetEntity(Entity entity)
    {
        var entityRecord = entity.EntityRecord;

        if (entityRecord != null)
        {
            EntityManager.InitializeEntity(entity, entity.ParentEntity, entity.SpriteRecord, entity.EntityRecord,
                entity.SpriteTableIndex, entity.EntityRefId,
                (entityRecord.XPos * 0xc + 0xc) * 0x10000,
                (entityRecord.YPos * 8 + 8) * 0x10000, 
                (entityRecord.Height << 0x13)
                , 0, StaticVariables.g_cardinalDirectionTable[entityRecord.SpriteDirection & 3],
                entity.PaletteOffset, entity.SpriteSheetOffset);
        }
        return entityRecord != null;
    }

    //80080a34
    public void InitializeAndBeginFadeEffect()
    {
        StaticVariables.g_targetFadeColorR = 0xff0000;
        StaticVariables.g_targetFadeColorG = 0xff0000;
        StaticVariables.g_targetFadeColorB = 0xff0000;
        StaticVariables.g_currentFadeColorR = 0;
        StaticVariables.g_currentFadeColorG = 0;
        StaticVariables.g_currentFadeColorB = 0;
        StaticVariables.g_fadeStepFlags = 1;
        StaticVariables.g_fadeFrameCounter = 0;
        BeginFadeEffect(1, 8);
    }
}