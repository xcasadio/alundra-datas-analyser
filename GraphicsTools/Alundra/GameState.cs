using Alundra.DatasBin;
using Alundra.Gameplay;
using Alundra.Gameplay.Scripts;
using Alundra.Sound;

namespace Alundra;

public class GameState
{
    public BalanceBin BalanceBin;
    public SoundBin SoundBin;
    public readonly GameMap AlundraGameMap;
    public GameMap GameMap;

    public int Seed = 42;
    public readonly int[] GameFlagsMap = new int[1024];
    public readonly int[] GameFlagsGlobal = new int[1024];
    public readonly short[] PlayerInput = new short[16];//no idea how many there are
    public bool BreakoutGameLoop;
    public int SomeGravitySetting;//0x1d84e0

    public int DialogState, DialogNameState, DialogName;

    //for event processing
    public int ActiveEventCode, PrevEventCode, ActiveEventProgramType, ActiveEventProgIndex, ActiveEntityRefId;

    public int NumSprites;
    public readonly SpriteRef[] SpriteRefs = new SpriteRef[2048];
    public readonly SpriteEffect[] SpriteEffects = new SpriteEffect[0x80];

    public int EventProgsSet;//a prog was set by an event, main event handler will repond

    public int UnknownCounter = 0;

    public Entity ActiveCollisionEntity;

    public readonly EventProgramState GlobalEventData = new();

    public readonly Entity[] GetEntityList = new Entity[128];

    public List<MapEvent> MapEvents = new();

    private readonly Entity[] g_entitySlots = new Entity[64];
    public readonly Entity[] Entities = new Entity[0x40];
    public Entity PlayerEntity; // => Entities[0]


    public GameState(GameMap alundraGameMap, BalanceBin balanceBin, SoundBin soundBin)
    {
        AlundraGameMap = alundraGameMap;
        BalanceBin = balanceBin;
        SoundBin = soundBin;

        for (int i = 0; i < g_entitySlots.Length; i++)
        {
            g_entitySlots[i] = new Entity
            {
                Index = i,
                Status = 0,
                EntityRefId = -1
            };
        }
    }

    public void LoadMap(GameMap map)
    {
        GameMap = map;

        //load MapEvents
        MapEvents = new List<MapEvent>();
        for (var i = 0; i < map.SpriteInfo.MapEvents.Records.Length; i++)
        {
            var record = map.SpriteInfo.MapEvents.Records[i];
            if (record != null)
            {
                var mapEvent = new MapEvent
                {
                    Id = i,
                    MapEventRecord = record,
                    ProgramBMap = record.EventCodesBIndex,
                    //TODO special logic if the eventcodesindex is 0
                    Entity = PlayerEntity,
                    EventData = new EventProgramState()
                };
                MapEvents.Add(mapEvent);
            }
        }

        LoadEntities();
    }

    private void InitializePlayer()
    {
        //AlundraGameMap.SpriteInfo.Entities

        PlayerEntity = new Entity
        {
            Index = 0,
            EntityRefId = -1,
            Status = 1,
            XPos = 0,
            YPos = 0,
            ZPos = 0,
            XTile = 0,
            YTile = 0,
            ZTile = 0,
            ModdedXPos = 0,
            ModdedYPos = 0,
            ModdedZPos = 0
        };
    }

    public void LoadEntities()
    {
        for (var i = 0; i < Entities.Length; i++)
        {
            var entity = new Entity();
            entity.Index = i;
            entity.EntityRefId = -1;
            Entities[i] = entity;
        }

        StaticVariables.g_numberOfEntity = 0;

        InitializeEntitySlots();

        for (var i = 0; i < GameMap.SpriteInfo.Entities.Entities.Length; i++)
        {
            var record = GameMap.SpriteInfo.Entities.Entities[i];
            if (record == null)
            {
                break;
            }

            var entity = ActivateEntity(null, i, 0);

            //it does some checks here with memory at 0x1ac468
            if (entity == null && false && false)
            {
                throw new Exception("error loading entity");
            }
        }

        StaticVariables.g_entityFollowedByCamera = PlayerEntity;
    }

    public void Initialize()
    {
        StaticVariables.Initialize();

        InitGameStateFromWarpTrigger();
        InitializePlayer();
    }

    void InitGameStateFromWarpTrigger()
    {
        int iconIndex;
        int iconEtcEntryPtr;
        int playerTileX;
        int playerTileY;
        int playerZ;

        StaticVariables.g_warpTriggerType = 0;
        StaticVariables.g_warpPriorityFlag = 0;
        //InitializeWarpAndFadeSystem();

        if (StaticVariables.g_someDataIntoRam == 1)
        {
            //CopyInitialDataToRAM();
            playerTileX = StaticVariables.g_initialWarpTileX;
            playerTileY = StaticVariables.g_initialWarpTileY;
            playerZ = StaticVariables.g_initialWarpZ;
        }
        else
        {
            //LoadDefaultGameData();

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
                //UpdateEntityFromWarpFlag(10);
                //FinalizeWarpEntities(10);
                //SetMaxFadeLevel(0);
                //SetFadeTargetLevel(0);
                //ApplyFadeLevel(0);
            }
            else
            {
                playerTileX = 0x16;
                playerTileY = 0x1d;
                playerZ = 10;
                StaticVariables.g_initialWarpMap = 0xb;
                StaticVariables.g_initialWarpTileX = 0x16;
                StaticVariables.g_initialWarpTileY = 0x1d;
                StaticVariables.g_initialWarpZ = 10;
                StaticVariables.g_warpExtraParam = 0;
                //UpdateEntityFromWarpFlag(0x2d);
                //FinalizeWarpEntities(0x26);
                //SetMaxFadeLevel(3);
                //SetFadeTargetLevel(2);
                //ApplyFadeLevel(0x873);
                //SetupPostWarpGraphics();
            }

            iconIndex = 0;
            iconEtcEntryPtr = StaticVariables.g_iconNameEtcBase;
            StaticVariables.g_lastVisitedMapId = -1; //0xffffffff;
            StaticVariables.g_currentSaveSlotNameIndex = 0;
            StaticVariables.g_tempGameState = 0;

            do
            {
                //if ((*(byte*)((int)iconEtcEntryPtr + 6) & 0x80) != 0)
                //{
                //    GetMapUnlockRequirement(iconIndex);
                //}
                iconIndex = iconIndex + 1;
                iconEtcEntryPtr = iconEtcEntryPtr + 2;
            } while (iconIndex < 0x62);

            //LoadWarpVisuals(1);
            //InitializeExtraSystemState();
        }

        StaticVariables.g_playerX = (playerTileX * 0x18 + 0xc) * 0x10000;
        StaticVariables.g_playerY = (playerTileY * 0x10 + 8) * 0x10000;
        StaticVariables.g_playerZ = playerZ << 0x14;
        StaticVariables.g_desiredMap = StaticVariables.g_initialWarpMap;
        StaticVariables.g_warpType = 0;
        StaticVariables.g_warpTriggerType = 0x36;
        StaticVariables.g_warpExtraParam = 0;
        StaticVariables.g_cameraTargetX = (StaticVariables.g_initialWarpTileX * 0x18 + 0xc) * 0x10000;
        StaticVariables.g_cameraTargetY = (StaticVariables.g_initialWarpTileY * 0x10 + 8) * 0x10000;
        StaticVariables.g_animation_id = StaticVariables.g_initialWarpZ << 0x14;
        StaticVariables.g_gameplayTime = StaticVariables.g_tempGameState;
    }

    void InitializeEntitySlots()
    {
        Entity result;
        Entity ent1;
        Entity ent2;
        Entity ent3;
        Entity readPtr;
        Entity writePtr;
        Entity blockStart;
        Entity entityCounter;
        int initDataIndex = 0;
        int initTableEntry = 0;

        entityCounter = null;
        writePtr = StaticVariables.g_player;
        //readPtr = StaticVariables.g_entities3;
        blockStart = writePtr;
        /*
        do
        {
            do
            {
                ent1 = readPtr[1];
                ent2 = readPtr[2];
                ent3 = readPtr[3];
                *writePtr = *readPtr;
                writePtr[1] = ent1;
                writePtr[2] = ent2;
                writePtr[3] = ent3;
                readPtr = readPtr + 4;
                writePtr = writePtr + 4;
            } while (readPtr != StaticVariables.PTR_801345f8);

            writePtr = StaticVariables.PTR_801345f8;
            blockStart = entityCounter;
            entityCounter = entityCounter->previousEntity + 1;
            writePtr = blockStart + 0xa5;
            readPtr = StaticVariables.g_entities3;
            blockStart = writePtr;
        } while ((int)entityCounter < 0x40); //64
        */
        //StaticVariables.g_entity = null;
        ResetEntityState();
        initDataIndex = 0;
        //var initTableEntry = 0; //StaticVariables.g_initTableEntry;

        do
        {
            if (GameMap.SpriteInfo.Entities.Entities[initTableEntry] == null)
            {
                break;
            }

            result = SpawnEntity(initDataIndex, 0);
            if (result == null && StaticVariables.g_debugState < 0 && (StaticVariables.g_debugFlags & 0x20) != 0)
            {
                //PrintInfo("Failed Character : ", characterIndex);
                throw new Exception($"Failed Character : {initDataIndex}");
            }
            initDataIndex++;
            //initTableEntry = initTableEntry + 5;
            initTableEntry++;
        } while (initDataIndex < 0x80);

        //StaticVariables.g_Entities = StaticVariables.g_player;
    }

    private Entity SpawnEntity(int initDataIndex, int checkSpawnZone)
    {
        var entityRecord = GameMap.SpriteInfo.Entities.Entities[initDataIndex];

        if (entityRecord == null)
        {
            return null;
        }

        int paletteIndex, sheetSize;

        var isMapSprite = ((uint)entityRecord.SpriteDirection & 0x40) != 0;
        var spriteRecord = GetSpriteFromSpriteTable(
            isMapSprite,
            entityRecord.SpriteTableIndex, out paletteIndex, out sheetSize);

        if (spriteRecord == null)
        {
            return null;
        }

        var entity = GetNextAvailableEntity();
        if (entity == null)
        {
            return null;
        }

        int spriteTableIndex = entityRecord.SpriteTableIndex;
        if (isMapSprite)
        {
            spriteTableIndex += 0x100;
        }

        InitializeEntity(entity, null, spriteRecord, entityRecord, spriteTableIndex,
            initDataIndex,
            entityRecord.XPos, //(x * 12 + 12) * 65536
            entityRecord.YPos, //(y * 8 + 8) * 65536
            entityRecord.Height, //h << 19
            0,
            0, //dir g_warpZones[flags & 3]
            paletteIndex,
            sheetSize);

        return entity;
    }

    void ResetEntityState()
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
        StaticVariables.g_warpStepCounter = 0;
        StaticVariables.g_playerWarpTimer = 0;
        StaticVariables.g_isWarpDisabled = 0;
        StaticVariables.g_frameTimer = 0;
        //tileIndex = GetCurrentTileIndex();
        //StaticVariables.g_currentTileFlags = StaticVariables.g_tileAttributeLUT[tileIndex];
        StaticVariables.g_warpTransitionCooldown = 0;
        //ResetWarpLockTimer();
        return;
    }

    private Entity AllocateEntitySlot()
    {
        for (int i = 1; i < g_entitySlots.Length; i++)
        {
            if (g_entitySlots[i].Status == 0)
            {
                return g_entitySlots[i];
            }
        }

        throw new("Character over execute");
        return null;
    }

    public SiEntityRecord GetInitData(int entityId)
    {
        SiEntityRecord res;

        if (entityId < 0 || GameMap.SpriteInfo.Entities.Entities.Length <= entityId) // StaticVariables.g_maxInitData
        {
            throw new Exception("Illegal character initial data!!");
            res = null;
        }
        else
        {
            res = GameMap.SpriteInfo.Entities.Entities[entityId];// * 0x14;

            if (GameMap.SpriteInfo.Entities.Entities[entityId + 1] == null)
            {
                res = null;
            }
        }
        return res;
    }

    private SiEntityRecord CheckValidEntityId(int entityid)
    {
        var rec = GetInitData(entityid);

        return rec;
    }

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
            entity.PlatformEntity._2c = 0;
        }
    }

    public int TurnEntity(Entity entity, int turnCode)
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
                return (entity.TargetDirection + turndir) & 0x1f;
            case 2:
                return ScriptHelper.CardinalDirTable[turndir & 0x3];
            case 3:
                var dfv = ScriptHelper.DirFromVector(PlayerEntity.XPos - entity.XPos, PlayerEntity.YPos - entity.YPos);
                return (dfv + turndir) & 0x1f;
            case 4:
                {
                    var i = Seed;
                    var val1 = (int)(i * 0x7d2b89dd);
                    var val2 = (int)(0xe06a02e7 + val1);
                    var val3 = (int)(((long)val2 * 4) >> 32);
                    Seed = val2;
                    var dir = ScriptHelper.CardinalDirTable[val3];//val3 here is a number between 0 and 3
                    return dir;
                }
            case 5:
                {
                    var i = Seed;
                    var val1 = (int)(i * 0x7d2b89dd);
                    var val2 = (int)(0xe06a02e7 + val1);
                    var val3 = (int)(((long)val2 * 0x20) >> 32);
                    Seed = val2;
                    return val2;
                }
            case 6:
                return (PlayerEntity.TargetDirection + turndir) & 0x1f;
            case 7:
                var ret = GetCardialDirToPlayer(entity);
                if (ret != -1)
                {
                    return (ret + turndir) & 0x1f;
                }

                break;
            case 0:
                break;
        }
        return turndir;
    }

    public int GetCardialDirToPlayer(Entity entity)
    {
        if (entity == ActiveCollisionEntity)
        {
            return -1;
        }

        var difx = PlayerEntity.ModdedXPos - entity.ModdedXPos;

        if ((difx >= 0 && entity.Width < difx)
            || (difx < 0 && PlayerEntity.Width < -difx))
        {
            //checkx
            if (PlayerEntity.XPos < entity.XPos)
            {
                return 0x08;
            }

            return 0x18;
        }

        //checky
        if (PlayerEntity.YPos < entity.YPos)
        {
            return 0x10;
        }

        return 0x00;

    }

    public int GetEntityFromRefId(Entity ownerEntity, int entityid)
    {
        var numgot = 0;
        if ((entityid & 0x80) == 0)
        {
            CheckValidEntityId(entityid);//calls getinitrecord which is a 20 byte datarecord SIEntityRecord
            foreach (var entity in Entities)
            {
                if ((ownerEntity.Status - 1 < 2 || ownerEntity.Status == 3) && entity.EntityRefId == entityid)
                {
                    GetEntityList[numgot++] = entity;
                }
            }
            return numgot;
        }

        var functionid = entityid & 0x7f;
        switch (functionid)
        {
            case 0://get owner
                GetEntityList[numgot++] = ownerEntity;
                return numgot;
            case 1://get player
                GetEntityList[numgot++] = PlayerEntity;
                return numgot;
            case 2://get all entities
                foreach (var entity in Entities)
                {
                    if (entity.Status - 1 < 2 || entity.Status == 3)
                    {
                        GetEntityList[numgot++] = entity;
                    }
                }
                return numgot;
            case 3://get all entities except player
                foreach (var entity in Entities.Skip(1))
                {
                    if (entity.Status - 1 < 2 || entity.Status == 3)
                    {
                        GetEntityList[numgot++] = entity;
                    }
                }
                return numgot;
            case 4://all entities on the ground
                foreach (var entity in Entities)
                {
                    if ((ownerEntity.Status - 1 < 2 || ownerEntity.Status == 3)
                        && (entity.Flags & 0x80) != 0
                        && (entity.AnimFlags & 0x80) == 0
                        && entity.PlatformEntity == null)
                    {
                        GetEntityList[numgot++] = entity;
                    }
                }
                return numgot;
            case 5://all entities besides player that the ownerentity is riding on
                foreach (var entity in Entities.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && ownerEntity.RidingEntity == entity)
                    {
                        GetEntityList[numgot++] = entity;
                    }

                }
                return numgot;
            case 6://all entities besides player that are riding on the ownerentity
                foreach (var entity in Entities.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && entity.RidingEntity == ownerEntity)
                    {
                        GetEntityList[numgot++] = entity;
                    }

                }
                return numgot;
            case 7://all entities besides player where ownerentity.xcollision? == entity
                foreach (var entity in Entities.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && ownerEntity.XCollisionEntity == entity)
                    {
                        GetEntityList[numgot++] = entity;
                    }

                }
                return numgot;
            case 8://all entities besides player where entity.xcollision? == ownerentity
                foreach (var entity in Entities.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && entity.XCollisionEntity == ownerEntity)
                    {
                        GetEntityList[numgot++] = entity;
                    }

                }
                return numgot;
            case 9://all entities besides player where entity.ownerentity [c] == ownerentity
                foreach (var entity in Entities.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && entity.OwnerEntity == ownerEntity)
                    {
                        GetEntityList[numgot++] = entity;
                    }

                }
                return numgot;
            case 10://all entities besides player where ownerentity.ownerentity [c] == entity
                foreach (var entity in Entities.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && ownerEntity.OwnerEntity == entity)
                    {
                        GetEntityList[numgot++] = entity;
                    }

                }
                return numgot;
            case 11://all entities besides player that are on a platform
                foreach (var entity in Entities.Skip(1))
                {
                    if ((entity.Status - 1 < 2 || entity.Status == 3)
                        && entity.PlatformEntity != null)
                    {
                        GetEntityList[numgot++] = entity;
                    }

                }
                return numgot;
        }

        return numgot;
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
            if (PlayerEntity.XTile < data.XMin)
            {
                return null;
            }

            if (data.XMax < PlayerEntity.XTile)
            {
                return null;
            }

            if (PlayerEntity.YTile < data.YMin)
            {
                return null;
            }

            if (data.YMax < PlayerEntity.YTile)
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

        var entity = GetNextAvailableEntity();

        if (entity == null)
        {
            return null;
        }

        var x = (data.XPos * 12 + 12) << 16;
        var y = (data.XPos * 8 + 8) << 16;
        var z = data.Height << 19;

        var directionTable = new[] { 0x00, 0x10, 0x08, 0x18 };
        var dir = directionTable[data.SpriteDirection & 0x3];

        var spriteTable = (int)data.SpriteTableIndex;
        if ((data.SpriteDirection & 0x80) != 0)
        {
            spriteTable += 0x100;
        }

        InitializeEntity(entity, ownerEntity, sprite, data, spriteTable, entityId, x, y, z, 0, dir, addedtosheet, addedtopalette);

        return entity;
    }

    public void InitializeEntity(Entity entity, Entity ownerEntity, SpriteRecord sprite, SiEntityRecord initData, int spriteTableIndex, int entityId, int x, int y, int z, int anim, int dir, int addedtosheet, int addedtopalette)
    {
        if (StaticVariables.g_numberOfEntity < entity.Index)
        {
            StaticVariables.g_numberOfEntity = entity.Index;
        }

        entity.OwnerEntity = ownerEntity;
        entity.UnknownBeforeOwnerEntity = ownerEntity?.UnknownBeforeOwnerEntity;

        entity.Sprite = sprite;
        entity.EntityRecord = initData;
        entity.SpriteTableIndex = spriteTableIndex;

        if (initData != null)
        {
            entity.EntityRefId = entityId;
        }
        else
        {
            entity.EntityRefId = -1;
        }

        entity.Status = 1;

        entity.UnknownAfterIndex = ++UnknownCounter;

        entity.CurrentAnimationId = ~anim;
        entity.CurrentDirection = ~dir;
        entity.TargetAnimationId = anim;
        entity.TargetDirection = dir;
        entity.Flags = sprite.Header.Moreflags | sprite.Header.CanPickup << 8 | sprite.Header.FlagsPortraitShadowtype << 16;

        entity.SpriteProgramIndexes[1] = 0;
        entity.SpriteProgramIndexes[0] = sprite.Header.ProgramLoad;
        entity.SpriteProgramIndexes[2] = sprite.Header.ProgramTick;
        entity.SpriteProgramIndexes[3] = sprite.Header.ProgramTouch;
        entity.SpriteProgramIndexes[4] = sprite.Header.ProgramDeactivate;
        entity.SpriteProgramIndexes[5] = sprite.Header.ProgramInteract;

        entity.AddedToSheet = addedtosheet;
        entity.AddedToPalette = addedtopalette;

        var ret = BalanceBin.GetBalanceRecordFromSpriteIndex(spriteTableIndex, GameMap.Info.BalanceLevel);
        entity.BalanceRecord = ret;
        entity.Hp = ret.Hp;
        entity.MaxHp = ret.Hp;

        InitCodePrograms(entity);

        InitEntityDimensions(entity, sprite.Header.Xmod, sprite.Header.Ymod, sprite.Header.Zmod, sprite.Header.Width, sprite.Header.Depth, sprite.Header.Height);

        entity.XPos = x;
        entity.YPos = y;
        entity.ZPos = z - entity.ZMod + 1;

        UpdateAnimation(entity);

        entity.ModdedXPos = entity.XPos + entity.XMod;
        entity.ModdedYPos = entity.YPos + entity.YMod;
        entity.ModdedZPos = entity.ZPos + entity.ZMod;

        var zhit = CollideWithMap(entity);

        entity.ZMapCollision = zhit;
        if (zhit + 1 >= entity.ZPos)
        {
            entity.ZPos = zhit + 1;
            entity.ModdedXPos = entity.XPos + entity.XMod;
            entity.ModdedYPos = entity.YPos + entity.YMod;
            entity.ModdedZPos = entity.ZPos + entity.ZMod;
        }

        UpdateTile(entity);

        InitContents(entity);
    }


    private void InitContents(Entity entity)
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

                int flag;
                if ((u7 & 0x8000) != 0)
                {
                    flag = GameFlagsMap[flagid];
                }
                else
                {
                    flag = GameFlagsGlobal[flagid];
                }

                var val = u7;
                if (u7 < 0)
                {
                    val = u7 + 0x1f;
                }
                var val2 = val >> 5;
                val2 = val2 << 5;
                var dif = val - val2;
                var bittocheck = 1 << dif;
                if ((flag & bittocheck) != 0)
                {
                    entity.ContentsItemId = GetContentsItemId(0);
                    return;
                }
            }
            if (entity.EntityRecord.Contents != 0)
            {
                entity.ContentsItemId = GetContentsItemId(entity.EntityRecord.Contents);
                return;
            }
        }
        else
        {
            entity.ContentsGameFlag = 0;
        }

        entity.ContentsItemId = GetContentsItemId(entity.Sprite.Header.Contents);
    }

    private byte[][] _contentstable =
    [
        [0,1,2,3,4,5],
        [0,1,2,3]

    ];

    private int GetContentsItemId(int contentsid)
    {
        do
        {
            if (contentsid >= 0x100)
            {
                return 0;
            }

            if ((contentsid & 0x80) == 0)
            {
                return contentsid & (0 - (contentsid < 0x62 ? 1 : 0));
            }

            var i = Seed;
            var val1 = (int)(i * 0x7d2b89dd);
            var val2 = (int)(0xe06a02e7 + val1);
            var targetval = (int)(((long)val2 * 16) >> 32);
            Seed = val2;

            var tableid = contentsid & 0x7f;
            //0x28db0 a table
            contentsid = _contentsTable[tableid][targetval];
        } while (true);
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

        entity.ZTile = entity.ZPos >> 20; //(z >> 16) / 16
        entity.XTile = (entity.XPos >> 16) / 24;
        entity.YTile = entity.YPos >> 20;

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

            entity.combinedVramFlagsOR = somevals[0] | somevals[1] | somevals[2] | somevals[3];
            entity.combinedVramFlagsAND = somevals[0] & somevals[1] & somevals[2] & somevals[3];

            var tilex = entity.XTile;

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
            var tiley = entity.YTile;
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

            var tile = GameMap.Map.MapTiles[tilex + tiley * 52];
            var fullval2 = tile.Walkability | tile.GroundProperty << 8 | tile.Slope << 16 | tile.Height << 24;
            var height = (int)(fullval2 & 0xff000000 >> 4) + 1;
            var r3 = height ^ entity.ModdedZPos;
        }
        else
        {
            tohit = 0;
            entity.combinedVramFlagsOR = 0;
            entity.combinedVramFlagsAND = 0;
        }

        //all that slope code is for setting this value
        entity.SomethingForceIndex = 0;

        var prevtohit = entity._18c;
        entity._18c = tohit;
        entity._190 = prevtohit;
    }

    public int CollideOnEntitiesZ(Entity entity)
    {
        var collision = entity.ZMapCollision + 1;
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

    public int CollideWithMap(Entity entity)
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
            var tile = GameMap.Map.MapTiles[tiley * 52 + tilex];
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

    //800237f4
    private readonly int[] _frameIndexTable =
    [
        0x00000000,
        0x00000000,
        0x00000002,
        0x00000001,
        0x00000001,
        0x00000001,
        0x00000003,
        0x00000000,
        0x00000000,
        0x00000000,
        0x00000002,
        0x00000001,
        0x00000001,
        0x00000001,
        0x00000003,
        0x00000000,
        0x00000000,
        0x00000002,
        0x00000002,
        0x00000002,
        0x00000001,
        0x00000003,
        0x00000003,
        0x00000003,
        0x00000000,
        0x00000002,
        0x00000002,
        0x00000002,
        0x00000001,
        0x00000003,
        0x00000003,
        0x00000003
    ];

    public void UpdateAnimation(Entity entity)
    {
        SiFrame currentFrame = null;
        bool noSkip = true;

        var frameDelay = entity.TargetAnimationId;
        entity.IsZForceApplied = 0;
        var animationFrameIndex = _frameIndexTable[(entity.TargetDirection + 2 & 0x1c) + entity.CurrentFrameIndex * 0x20];

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
                    entity.NextFrameDelay = frameDelay & 0x7f;
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
                frameDelay = currentFrame.CollisionOffset & 0x80;
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
            var animTableOffset = entity.AnimSet.AnimOffsets[entity.CurrentFrameIndex];
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
            //entity.AnimFlags = (byte)animRecordPtr.AnimOffsets[1];
            entity.AnimFlags = entity.AnimSet.Flags;

            if (entity.BalanceRecord.NumAnimVals == 0)
            {
                entity.BalanceVal = null;
            }
            else if (frameDelay + 1 < entity.BalanceRecord.NumAnimVals)
            {
                //entity.BalanceVal = entity.BalanceRecord.Vals[frameDelay * 2 + 0xe];
                System.Diagnostics.Debugger.Break();
            }
            else
            {
                entity.BalanceVal = entity.BalanceRecord.AnimVals[0]; // 0 ?? TODO: check if index 0
            }

            //frameDelay = (uint)(byte)&animSet.pointerListOffset;
            //
            //if (((uint)animSet.pointerListOffset & 0x2000) != 0)
            //{
            //    frameDelay += 0x100;
            //}
            //
            //PlaySoundEffect(frameDelay);

            noSkip = true; //imitate goto LOAD_ANIMATION
        }

        entity.NextFrameDelay = 0x7fffffff;
        entity.ForceResetAnimationFlag = 1;
    }

    public void UpdateAnimation2(Entity entity)
    {
        //TODO
        var directionIndex = ((entity.TargetDirection + 2) & 0x1c) >> 2;
        var frameIndex = _frameIndexTable[directionIndex + (entity.CurrentFrameIndex << 3)];
        SiFrame frame = null;
        entity.IsZForceApplied = 0;

        if (entity.TargetAnimationId != entity.CurrentAnimationId
            || frameIndex != entity.CurrentFrameIndex)
        {
            entity.AnimSet = entity.Sprite.AnimSets[entity.TargetAnimationId];
            entity.CurrentFrameIndex = frameIndex;
            //SIAnimSet se;
            //se.animoffsets[]

            entity.NextFrameDelay = 0;
            entity.CurrentAnimationId = entity.TargetAnimationId;
            //TODO: look into if these indexes are correct
            frame = entity.AnimSet.PreloadedAnims[entity.TargetAnimationId].Frames[frameIndex];

            entity.FirstFrame = frame;
            entity.Frame = frame;

            entity.IsZForceApplied = entity.AnimSet.Speed;

            entity.ForceResetAnimationFlag = 0;
            entity.AnimFlags = entity.AnimSet.Flags;
            BalanceAnimValRef avr = null;

            if (entity.BalanceRecord.NumAnimVals != 0)
            {
                if (entity.CurrentAnimationId + 1 < entity.BalanceRecord.NumAnimVals)
                {
                    avr = entity.BalanceRecord.AnimVals[entity.CurrentAnimationId + 1];
                }
                else
                {
                    avr = entity.BalanceRecord.AnimVals[0];
                }
            }
            entity.BalanceVal = avr;

            int sfx = entity.AnimSet.Sfx;
            if ((entity.AnimSet.Flags & 0x20) != 0)
            {
                sfx += 0x100;
            }
            //TODO:enable sfx
            //PlaySoundEffect(sfx);
        }
        else
        {
            if (--entity.NextFrameDelay != 0)
            {
                return;
            }

            //time to change the frame
            frame = entity.Frame;
        }

        do
        {
            //if it has a next frame
            if ((frame.Delay & 0x80) != 0)
            {
                entity.NextFrameDelay = frame.Delay & 0x7f;
                //TODO: better way to do this
                entity.Frame = entity.AnimSet.PreloadedAnims[entity.TargetAnimationId].Frames[entity.CurrentFrameIndex + 1];

                if (frame.CollisionOffset != -1)
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

                if (frame.ImageSetPointer != -1)
                {
                    entity.SpriteRef.Images = frame.Images.Images;
                    entity.SpriteRef.DepthSortVal = frame.Images.Unknown;
                    entity.SpriteRef.NumImages = frame.Images.NumberOfImages;
                    return;
                }
                entity.SpriteRef.Images = null;
                entity.SpriteRef.DepthSortVal = 0;
                entity.SpriteRef.NumImages = 0;
                return;
            }

            if (frame.Delay != 0)
            {
                if (frame.Delay != 1)
                {
                    //this is a bad state, output debug info
                    throw new Exception("this is a bad animation state");
                }
            }
            else
            {//frame.delay = 0, non repeating animation?
                if ((frame.CollisionOffset & 0x80) != 0)//why, it doesn't really make sense
                {
                    entity.NextFrameDelay = 0x7fffffff;//what will this mean
                    entity.ForceResetAnimationFlag = 1;
                    return;
                }
                frameIndex = entity.CurrentFrameIndex;
                entity.TargetAnimationId = frame.CollisionOffset & 0xff;
                entity.AnimCompleteCounter++;////we get here when the animation is nonrepeating and is finished, so it switches back to some other animation
                //call recursivly?
                UpdateAnimation(entity);
            }

            entity.AnimCompleteCounter++;
            entity.Frame = entity.FirstFrame;//the anim repeats

        } while (true);//will this ever be an infinite loop
    }

    public void InitEntityDimensions(Entity entity, int xmod, int ymod, int zmod, int width, int depth, int height)
    {
        entity.NegXMod = -(xmod << 16);
        entity.NegYMod = -(ymod << 16);
        entity.XMod = xmod << 16;
        entity.YMod = ymod << 16;
        entity.ZMod = zmod << 16;

        entity.ScreenClipX = 0x4e00000 - ((xmod + width) << 16);
        entity.ScreenClipY = 0x3c00000 - ((ymod + depth) << 16);
        entity.ScreenClipZ = 0x7800000 - ((zmod + height) << 16);

        if (width != 0)
        {
            entity.Width = (width << 16) - 1;
        }
        else
        {
            entity.Width = 0;
        }

        if (depth != 0)
        {
            entity.Depth = (depth << 16) - 1;
        }
        else
        {
            entity.Depth = 0;
        }

        if (height != 0)
        {
            entity.Height = (height << 16) - 1;
        }
        else
        {
            entity.Height = 0;
        }
    }

    public void InitCodePrograms(Entity entity)
    {
        entity.EntitySelf = entity;
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

    private SpriteRecord GetSpriteFromSpriteTable(bool isMapSprite, int spritetableindex, out int addedtosheet, out int addedtopallette)
    {
        SpriteInfo si;
        if (isMapSprite)
        {
            si = GameMap.SpriteInfo;
            addedtosheet = 0;
            addedtopallette = 0x20;
        }
        else
        {
            si = AlundraGameMap.SpriteInfo;
            addedtosheet = 0xb;
            addedtopallette = 0x60;
        }
        if (spritetableindex < 0)
        {
            throw new Exception("Illegal Character Race!");
        }

        if (spritetableindex >= si.SpriteTable.Length)
        {
            throw new Exception("Illegal Character Race!");
        }

        var sprite = si.Sprites[spritetableindex];
        return sprite;
    }

    public SpriteEffectRecord GetEffectSpriteFromSpriteTable(bool isMapSprite, int spritetableindex, out int addedtosheet, out int addedtopallette)
    {
        SpriteInfo si;
        if (isMapSprite)
        {
            si = GameMap.SpriteInfo;
            addedtosheet = 0;
            addedtopallette = 0x20;
        }
        else
        {
            si = AlundraGameMap.SpriteInfo;
            addedtosheet = 0xb;
            addedtopallette = 0x60;
        }
        if (spritetableindex >= 0 && spritetableindex < si.SpriteTable.Length)
        {
            return si.Spriteeffects[spritetableindex];
        }

        return null;
    }

    public SpriteEffect GetNextAvailableEffect()
    {
        foreach (var effect in SpriteEffects)
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
        effect.AddToSheet = 0;
        effect.AddToSheet = 0;
        effect.MapEffectId = 0;
        effect.EffectType = 0;
        effect.EntityRef = null;
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
        effect.TargetAnim = 0;
        effect.CurrentAnim = 0;
        effect.Frame = null;
        effect.FirstFrame = null;
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
        effect.TargetAnim = animid;
        effect.CurrentAnim = (byte)~animid;
        effect.X = x;
        effect.Y = y;
        effect.Z = z;
    }

    public void DestroyEntity(Entity entity, int effectid)
    {
        SpawnEntityContents(entity);

        entity.Status = 4;
        entity.EventTrigger = -1;
        if (entity.ActiveEffect != null)
        {
            entity.ActiveEffect.Status = 0;
            entity.ActiveEffect = null;
        }

        if (effectid == -1)
        {
            effectid = entity.Sprite.Header.Breakeffect;
        }

        if (effectid != 0)
        {
            CreateEffect_Type1(0, (byte)effectid, 0, entity, 1, 0, 0, 0);
        }

        if (entity.PlatformEntity != null)
        {
            //TODO: figure out what 2c is
            entity.PlatformEntity._2c = 0;
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

        var child = SpawnEntity(null, false, entity.ContentsItemId + 0x1e, entity.XPos, entity.YPos, entity.ZPos, 0);

        if (child == null)
        {
            return 0;
        }

        child.ZForce = 0xa0000;
        child._274 = 1;

        child.Flags &= 0xff7f;

        //TODO: impliment this lookuptable
        /*int result = lookuptable[entity.ContentsItemId * 8];*/

        //if (result == 0)
        //    result = -1;

        //child.SpawnedItemId = result;

        child._27c = 0;

        child.SpawnedZForce = 0xa0000;

        //TODO sfx
        //PlaySoundEffect(0x54);
        child.SpawnedGameFlag = entity.ContentsGameFlag;
        return 1;
    }

    private bool CheckItemId(int itemid)
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

    public MapEffectRecord GetMapEffectRecord(int id, bool checkBoundingBox)
    {
        if (id < GameMap.SpriteInfo.MapEffectRecords.Length)
        {
            var record = GameMap.SpriteInfo.MapEffectRecords[id];
            if (checkBoundingBox)
            {
                var playerEntity = PlayerEntity;
                if (playerEntity.XTile < record.X1 || playerEntity.XTile > record.X2
                                        || playerEntity.YTile < record.Y1 || playerEntity.YTile > record.Y2)
                {
                    return null;
                }
            }

            return record;
        }
        return null;
    }

    public SpriteEffect CreateEffect_Type0(byte ismapeffect, byte effectid, byte animid, int x, int y, int z)
    {
        var effect = GetNextAvailableEffect();

        if (effect != null)
        {
            InitEffect(effect, null, -1, 0, ismapeffect, effectid, animid, x, y, z);
            return effect;
        }
        return null;
    }


    public SpriteEffect CreateEffect_Type1(byte ismapeffect, byte effectid, byte animid, Entity entity, int depthsortmod, int xoff, int yoff, int zoff)
    {
        var effect = GetNextAvailableEffect();

        if (effect != null)
        {
            InitEffect(effect, null, -1, 1, ismapeffect, effectid, animid, entity.XPos, entity.YPos, entity.ZPos);
            effect.EntityRef = entity;
            effect.DepthSortMod = depthsortmod;
            effect.XOff = xoff;
            effect.YOff = yoff;
            effect.ZOff = zoff;
            return effect;
        }
        return null;
    }

    public SpriteEffect CreateEffect_Type3(byte ismapeffect, byte effectid, byte animid, Entity entity, int depthsortmod, int x, int y, int z)
    {
        var effect = GetNextAvailableEffect();

        if (effect != null)
        {
            InitEffect(effect, null, -1, 3, ismapeffect, effectid, animid, x, y, z);
            effect.EntityRef = entity;
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
                    (byte)((record.Flags & 0x80) >> 7), record.Effectid, record.Animid,
                    (record.X * 12 + 12) << 16, (record.Y * 8 + 8) << 16, record.Z << 19);

                return effect;
            }
        }
        return null;
    }



    public Entity SpawnEntity(Entity ownerEntity, bool ismapsprite, int tableindex, int xpos, int ypos, int zpos, int dir)
    {
        /*

        if (g_playerPosX < (int)(uint)initType->xMin) {
            spawnedEntity = (Entity *)0x0;
            return spawnedEntity;
        }
        if ((int)(uint)initType->xMax < g_playerPosX) {
            spawnedEntity = (Entity *)0x0;
            return spawnedEntity;
        }
        if (g_playerPosY < (int)(uint)initType->yMin) {
            spawnedEntity = (Entity *)0x0;
            return spawnedEntity;
        }
        if ((int)(uint)initType->yMax < g_playerPosY) {
            spawnedEntity = (Entity *)0x0;
            return spawnedEntity;
        }*/


        int paletteIndex, sheetSize;

        var spriteRecord = GetSpriteFromSpriteTable(ismapsprite, tableindex, out paletteIndex, out sheetSize);

        if (spriteRecord == null)
        {
            return null;
        }

        var entity = GetNextAvailableEntity();
        if (entity == null)
        {
            return null;
        }

        var spriteTableIndex = tableindex;
        if (ismapsprite)
        {
            spriteTableIndex += 0x100;
        }

        InitializeEntity(entity, ownerEntity, spriteRecord, null, spriteTableIndex, -1, xpos, ypos, zpos, 0, dir, paletteIndex, sheetSize);

        return entity;
    }


    private Entity GetNextAvailableEntity()
    {
        foreach (var entity in Entities)
        {
            if (entity.Status == 0)
            {
                return entity;
            }
        }
        //var newentity = new Entity { Index = Entities.Count + 1 };
        //Entities.Add(newentity);
        //return newentity;
        return null;
    }






    //sprite stuff
    //Dictionary<int, List<Bitmap>> CachedSprites;


    /*public List<Bitmap> GetSpriteImages(SIImageSet imgset, int sheetmod=0, int palmod=0)
    {

        if (!CachedSprites.ContainsKey(imgset.imagesetid))
        {
            var list = new List<Bitmap>();
            for (int dex = 0; dex < imgset.numimages; dex++)
                list.Add(gameMap.GenerateSpriteBitmap(imgset.images[dex], gameMap.spriteinfo.palettes[(imgset.images[dex].palette & 0x1f)]));
            CachedSprites.Add(imgset.imagesetid, list);
        }


        return CachedSprites[imgset.imagesetid];
    }*/


    private readonly byte[][] _contentsTable =
    [
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x24,0x25,0x45,0x46,0x47,0x48,0x4f,0x50,0x51,0x52,0x53,0x54,0x55,0x56,0x00,0x00],
        [0x54,0x54,0x54,0x55,0x55,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x29,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00],
        [0x45,0x45,0x45,0x45,0x45,0x46,0x46,0x46,0x46,0x46,0x46,0x46,0x47,0x47,0x47,0x47],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x24,0x24,0x24,0x24,0x24,0x24,0x24,0x24],
        [0x45,0x45,0x46,0x54,0x54,0x55,0x55,0x56,0x46,0x47,0x47,0x48,0x51,0x51,0x51,0x51],
        [0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48,0x48],
        [0xf6,0xf6,0xf6,0xf6,0xf6,0xf6,0xf6,0xf6,0xf6,0xf6,0xf6,0xf6,0xf9,0xf9,0xf9,0xf9],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xf6,0xf6,0xfd,0xfe,0xfe,0xfe,0xfe],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xf6,0xf6,0xfd,0xfe,0xfe],
        [0x45,0x45,0x45,0x45,0x45,0x45,0x45,0x45,0x45,0x46,0x46,0x46,0x46,0x46,0x47,0x47],
        [0x54,0x54,0x54,0x54,0x54,0x54,0x54,0x54,0x54,0x54,0x54,0x54,0x55,0x55,0x55,0x55],
        [0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xfd,0xfe],
        [0x00,0x00,0x41,0x20,0x4e,0x65,0x77,0x20,0x42,0x65,0x67,0x69,0x6e,0x6e,0x69,0x6e],
        [0x67,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x00,0x03,0x00,0x57,0x65,0x6e,0x64,0x65,0x6c,0x6c,0x20,0x73,0x75,0x63,0x63],
        [0x75,0x6d,0x62,0x73,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x00,0x08,0x00,0x45,0x73,0x63,0x61,0x70,0x65,0x20,0x74,0x6f,0x20],
        [0x54,0x61,0x72,0x6e,0x27,0x73,0x20,0x4d,0x61,0x6e,0x6f,0x72,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x00,0x6c,0x00,0x54,0x68,0x65,0x20,0x42,0x6f,0x6f,0x6b],
        [0x20,0x6f,0x66,0x20,0x45,0x6c,0x6e,0x61,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x76,0x06,0x57,0x65,0x6e,0x64,0x65,0x6c],
        [0x6c,0x27,0x73,0x20,0x53,0x61,0x6c,0x76,0x61,0x74,0x69,0x6f,0x6e,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x6d,0x00,0x43,0x6f,0x6c,0x6c],
        [0x61,0x70,0x73,0x65,0x20,0x6f,0x66,0x20,0x74,0x68,0x65,0x20,0x4d,0x69,0x6e,0x65],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0xef,0x00,0x41,0x20],
        [0x50,0x72,0x61,0x79,0x65,0x72,0x20,0x66,0x6f,0x72,0x20,0x74,0x68,0x65,0x20,0x4d],
        [0x69,0x6e,0x65,0x72,0x73,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x27,0x01],
        [0x43,0x72,0x6f,0x73,0x73,0x69,0x6e,0x67,0x20,0x74,0x68,0x65,0x20,0x4d,0x69,0x6e],
        [0x65,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00],
        [0xf7,0x00,0x44,0x72,0x65,0x61,0x6d,0x20,0x52,0x65,0x76,0x65,0x6c,0x61,0x74,0x69],
        [0x6f,0x6e,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x00,0xe9,0x00,0x4c,0x65,0x61,0x76,0x69,0x6e,0x67,0x20,0x74,0x68,0x65,0x20],
        [0x4d,0x69,0x6e,0x65,0x72,0x73,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x00,0xe8,0x00,0x49,0x6e,0x74,0x6f,0x20,0x74,0x68,0x65,0x20,0x43],
        [0x72,0x79,0x70,0x74,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x00,0x5c,0x01,0x53,0x6c,0x75,0x6d,0x62,0x65,0x72,0x20],
        [0x42,0x75,0x6d,0x6d,0x65,0x72,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x2b,0x01,0x54,0x6f,0x20,0x74,0x68,0x65],
        [0x20,0x44,0x65,0x73,0x65,0x72,0x74,0x20,0x6f,0x66,0x20,0x44,0x65,0x73,0x70,0x61],
        [0x69,0x72,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x8b,0x02,0x42,0x65,0x67,0x69],
        [0x6e,0x6e,0x69,0x6e,0x67,0x20,0x6f,0x66,0x20,0x74,0x68,0x65,0x20,0x45,0x6e,0x64],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x4b,0x01,0x41,0x20],
        [0x52,0x65,0x76,0x65,0x6c,0x61,0x74,0x69,0x6f,0x6e,0x20,0x69,0x6e,0x20,0x4d,0x65],
        [0x69,0x61,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x78,0x05],
        [0x41,0x20,0x48,0x61,0x6e,0x64,0x20,0x66,0x6f,0x72,0x20,0x4b,0x6c,0x69,0x6e,0x65],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00],
        [0x01,0x01,0x54,0x68,0x65,0x20,0x53,0x77,0x61,0x6d,0x70,0x20,0x54,0x68,0x69,0x6e],
        [0x67,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x00,0x79,0x05,0x47,0x69,0x6c,0x65,0x73,0x27,0x20,0x53,0x61,0x6c,0x76,0x61],
        [0x74,0x69,0x6f,0x6e,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x00,0x7a,0x05,0x43,0x61,0x76,0x65,0x20,0x6f,0x66,0x20,0x4d,0x61],
        [0x67,0x79,0x73,0x63,0x61,0x72,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x00,0x72,0x03,0x54,0x68,0x65,0x20,0x53,0x61,0x6e,0x63],
        [0x74,0x75,0x61,0x72,0x79,0x27,0x73,0x20,0x53,0x65,0x63,0x72,0x65,0x74,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x7b,0x05,0x53,0x79,0x62,0x69,0x6c,0x6c],
        [0x27,0x73,0x20,0x45,0x78,0x69,0x74,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x7c,0x05,0x4d,0x65,0x69,0x61],
        [0x27,0x73,0x20,0x50,0x61,0x73,0x74,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x7d,0x05,0x4e,0x61],
        [0x76,0x61,0x27,0x73,0x20,0x43,0x68,0x6f,0x69,0x63,0x65,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x85,0x03],
        [0x42,0x6f,0x75,0x72,0x6e,0x65,0x20,0x6f,0x66,0x20,0x57,0x61,0x74,0x65,0x72,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00],
        [0x7e,0x05,0x41,0x20,0x44,0x61,0x6e,0x63,0x65,0x20,0x77,0x69,0x74,0x68,0x20,0x4e],
        [0x69,0x72,0x75,0x64,0x65,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x00,0x10,0x05,0x52,0x6f,0x6e,0x61,0x6e,0x27,0x73,0x20,0x43,0x6f,0x6e,0x73],
        [0x70,0x69,0x72,0x61,0x63,0x79,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x00,0x23,0x00,0x41,0x74,0x20,0x4f,0x64,0x64,0x73,0x20,0x77,0x69],
        [0x74,0x68,0x20,0x52,0x6f,0x6e,0x61,0x6e,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x00,0x1f,0x00,0x41,0x20,0x4c,0x65,0x74,0x74,0x65,0x72],
        [0x20,0x66,0x72,0x6f,0x6d,0x20,0x4a,0x65,0x73,0x73,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x46,0x00,0x45,0x6c,0x65,0x6e,0x65,0x27],
        [0x73,0x20,0x31,0x35,0x20,0x4d,0x69,0x6e,0x75,0x74,0x65,0x73,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x48,0x00,0x47,0x69,0x6c,0x65],
        [0x73,0x2c,0x20,0x41,0x67,0x61,0x69,0x6e,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x4f,0x00,0x54,0x6f],
        [0x20,0x4d,0x75,0x72,0x67,0x67,0x20,0x57,0x6f,0x6f,0x64,0x73,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x7a,0x03],
        [0x4c,0x6f,0x73,0x74,0x20,0x69,0x6e,0x20,0x4d,0x75,0x72,0x67,0x67,0x20,0x57,0x6f],
        [0x6f,0x64,0x73,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00],
        [0xd3,0x01,0x54,0x68,0x65,0x20,0x47,0x69,0x61,0x6e,0x74,0x20,0x54,0x72,0x65,0x65],
        [0x20,0x54,0x6f,0x77,0x65,0x72,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x00,0x33,0x02,0x54,0x6f,0x72,0x6c,0x61,0x2c,0x20,0x4d,0x6f,0x75,0x6e,0x74],
        [0x61,0x69,0x6e,0x20,0x6f,0x66,0x20,0x46,0x69,0x72,0x65,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x00,0xa2,0x06,0x42,0x65,0x72,0x67,0x75,0x73,0x20,0x48,0x65,0x6c],
        [0x64,0x20,0x48,0x6f,0x73,0x74,0x61,0x67,0x65,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x00,0x7b,0x03,0x42,0x61,0x70,0x74,0x69,0x73,0x6d,0x20],
        [0x62,0x79,0x20,0x46,0x69,0x72,0x65,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0xe7,0x00,0x43,0x6f,0x6e,0x66,0x72,0x6f],
        [0x6e,0x74,0x69,0x6e,0x67,0x20,0x52,0x6f,0x6e,0x61,0x6e,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x4f,0x04,0x54,0x6f,0x20,0x4e],
        [0x61,0x76,0x61,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0xd0,0x03,0x54,0x68],
        [0x65,0x20,0x43,0x61,0x73,0x74,0x6c,0x65,0x20,0x69,0x6e,0x20,0x74,0x68,0x65,0x20],
        [0x4c,0x61,0x6b,0x65,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00,0x64,0x06],
        [0x4f,0x75,0x74,0x73,0x69,0x64,0x65,0x20,0x74,0x68,0x65,0x20,0x43,0x61,0x73,0x74],
        [0x6c,0x65,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x00],
        [0x9f,0x04,0x54,0x68,0x65,0x20,0x47,0x72,0x65,0x61,0x74,0x20,0x48,0x61,0x6c,0x6c],
        [0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x00,0xa0,0x04,0x4c,0x61,0x73,0x74,0x20,0x41,0x72,0x6d,0x61,0x67,0x65,0x64],
        [0x64,0x6f,0x6e,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20,0x20],
        [0x20,0x20,0x20,0x00,0xff,0xff,0x30,0x31,0x32,0x33,0x34,0x35,0x36,0x37,0x38,0x39],
        [0x41,0x42,0x43,0x44,0x45,0x46,0x30,0x31,0x32,0x33,0x34,0x35,0x36,0x37,0x38,0x39],
        [0x41,0x42,0x43,0x44,0x45,0x00,0x00,0x00,0x00,0x00,0x01,0x80,0x00,0x00,0x00,0x00],
        [0x01,0x00,0x00,0x00,0x63,0x64,0x72,0x6f,0x6d,0x3a,0x5c,0x53,0x4c,0x55,0x53,0x5f],
        [0x30,0x30,0x35,0x2e,0x35,0x33,0x3b,0x31,0x00,0x00,0x00,0x00,0x72,0x6d,0x2e,0x20],
        [0x52,0x65,0x73,0x6f,0x75,0x72,0x63,0x65,0x20,0x45,0x72,0x72,0x6f,0x72,0x20,0x21],
        [0x21,0x00,0x00,0x00,0x30,0x31,0x32,0x33,0x34,0x35,0x36,0x37,0x38,0x39,0x61,0x62],
        [0x63,0x64,0x65,0x66,0x00,0x00,0x00,0x00,0x28,0x6e,0x75,0x6c,0x6c,0x29,0x00,0x00],
        [0x30,0x31,0x32,0x33,0x34,0x35,0x36,0x37,0x38,0x39,0x41,0x42,0x43,0x44,0x45,0x46],
        [0x00,0x00,0x00,0x00,0xc8,0x47,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0x20,0x48,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0x30,0x48,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0x38,0x48,0x08,0x80],
        [0x5c,0x48,0x08,0x80,0xa0,0x4d,0x08,0x80,0x54,0x48,0x08,0x80,0x64,0x48,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xf0,0x48,0x08,0x80,0xf8,0x48,0x08,0x80,0xf8,0x48,0x08,0x80],
        [0xf8,0x48,0x08,0x80,0xf8,0x48,0x08,0x80,0xf8,0x48,0x08,0x80,0xf8,0x48,0x08,0x80],
        [0xf8,0x48,0x08,0x80,0xf8,0x48,0x08,0x80,0xf8,0x48,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0x84,0x49,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0x50,0x49,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0x08,0x4a,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xb4,0x4a,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xec,0x4a,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0x68,0x49,0x08,0x80,0x88,0x49,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0x58,0x49,0x08,0x80,0x88,0x49,0x08,0x80,0xa0,0x4d,0x08,0x80],
        [0xa0,0x4d,0x08,0x80,0x60,0x49,0x08,0x80,0xa0,0x4d,0x08,0x80,0xcc,0x49,0x08,0x80],
        [0x0c,0x4a,0x08,0x80,0x40,0x4a,0x08,0x80,0xa0,0x4d,0x08,0x80,0xa0,0x4d,0x08,0x80]
    ];
}