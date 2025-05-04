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

    public readonly short[] PlayerInput = new short[16];//no idea how many there are
    public bool BreakoutGameLoop;

    public int DialogState, DialogNameState, DialogName;

    public int NumSprites;
    public readonly SpriteRef[] SpriteRefs = new SpriteRef[2048];
    
    public List<MapEvent> MapEvents = new();
    

    public GameState(GameMap alundraGameMap, BalanceBin balanceBin, SoundBin soundBin)
    {
        AlundraGameMap = alundraGameMap;
        BalanceBin = balanceBin;
        SoundBin = soundBin;

        for (int i = 0; i < StaticVariables.g_entitySlots.Length; i++)
        {
            StaticVariables.g_entitySlots[i] = new Entity
            {
                //Index = i,
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
                    Entity = StaticVariables.PlayerEntity,
                    EventData = new EventProgramState()
                };
                MapEvents.Add(mapEvent);
            }
        }

        LoadEntities();
    }

    private void InitializePlayer()
    {
        //AlundraGameMap.SpriteInfo.g_entitySlots

        //StaticVariables.PlayerEntity.Index = 0;
        StaticVariables.PlayerEntity.EntityRefId = -1;
        StaticVariables.PlayerEntity.Status = 1;
        StaticVariables.PlayerEntity.XPos = 0;
        StaticVariables.PlayerEntity.YPos = 0;
        StaticVariables.PlayerEntity.ZPos = 0;
        StaticVariables.PlayerEntity.TileX = 0;
        StaticVariables.PlayerEntity.TileY = 0;
        StaticVariables.PlayerEntity.TileZ = 0;
        StaticVariables.PlayerEntity.ModdedXPos = 0;
        StaticVariables.PlayerEntity.ModdedYPos = 0;
        StaticVariables.PlayerEntity.ModdedZPos = 0;
    }

    public void LoadEntities()
    {
        for (var i = 0; i < StaticVariables.g_entitySlots.Length; i++)
        {
            var entity = new Entity();
            //entity.Index = i;
            entity.EntityRefId = -1;
            StaticVariables.g_entitySlots[i] = entity;
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

        StaticVariables.g_entityFollowedByCamera = StaticVariables.PlayerEntity;
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
        StaticVariables.g_gravityFlag = 0;
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
            StaticVariables.g_lastVisitedMapId = 0xffffffff;
            StaticVariables.g_currentSaveSlotNameIndex = 0;
            StaticVariables.g_savedGameplayTime = 0;

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

        StaticVariables.g_cameraLookAtX = (playerTileX * 0x18 + 0xc) * 0x10000;
        StaticVariables.g_cameraLookAtY = (playerTileY * 0x10 + 8) * 0x10000;
        StaticVariables.g_cameraLookAtZ = playerZ << 0x14;
        StaticVariables.g_desiredMap = StaticVariables.g_initialWarpMap;
        StaticVariables.g_warpType = 0;
        StaticVariables.g_warpTriggerType = 0x36;
        StaticVariables.g_warpExtraParam = 0;
        StaticVariables.g_cameraTargetX = (StaticVariables.g_initialWarpTileX * 0x18 + 0xc) * 0x10000;
        StaticVariables.g_cameraTargetY = (StaticVariables.g_initialWarpTileY * 0x10 + 8) * 0x10000;
        StaticVariables.g_animation_id = StaticVariables.g_initialWarpZ << 0x14;
        StaticVariables.g_gameplayTime = StaticVariables.g_savedGameplayTime;
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
        writePtr = StaticVariables.PlayerEntity;
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

        var entity = AllocateEntitySlot();
        if (entity == null)
        {
            return null;
        }

        uint spriteTableIndex = entityRecord.SpriteTableIndex;
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
            0, //dir g_cardinalDirectionTable[flags & 3]
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
        StaticVariables.g_gravityFlag = 0;
        StaticVariables.g_playerWarpTimer = 0;
        StaticVariables.g_isWarpDisabled = 0;
        StaticVariables.g_frameTimer = 0;
        //tileIndex = GetCurrentTileIndex();
        //StaticVariables.g_currentTileFlags = StaticVariables.g_tileAttributeLUT[tileIndex];
        StaticVariables.g_playerEffectTransitionCooldown = 0;
        //ResetWarpLockTimer();
        return;
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
                return (uint)StaticVariables.g_cardinalDirectionTable[turndir & 0x3];
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
                    var dir = StaticVariables.g_cardinalDirectionTable[val3];//val3 here is a number between 0 and 3
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

    public void InitializeEntity(Entity entity, Entity ownerEntity, SpriteRecord sprite, SiEntityRecord initData, uint spriteTableIndex, int entityId, int x, int y, int z, uint anim, uint dir, int addedtosheet, int addedtopalette)
    {
        if (StaticVariables.g_numberOfEntity < entity.Index)
        {
            StaticVariables.g_numberOfEntity = entity.Index;
        }

        entity.ParentEntity = ownerEntity;
        entity.ChildEntity = ownerEntity?.ChildEntity;

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

        //entity.UnknownAfterIndex = ++UnknownCounter;

        entity.CurrentAnimationId = ~anim;
        entity.CurrentDirection = ~dir;
        entity.TargetAnimationId = anim;
        entity.TargetDirection = dir;
        entity.Flags = (uint)(sprite.Header.MoreFlags | sprite.Header.CanPickup << 8 | sprite.Header.FlagsPortraitShadowtype << 16);

        entity.SpriteProgramIndexes[1] = 0;
        entity.SpriteProgramIndexes[0] = sprite.Header.ProgramLoad;
        entity.SpriteProgramIndexes[2] = sprite.Header.ProgramTick;
        entity.SpriteProgramIndexes[3] = sprite.Header.ProgramTouch;
        entity.SpriteProgramIndexes[4] = sprite.Header.ProgramDeactivate;
        entity.SpriteProgramIndexes[5] = sprite.Header.ProgramInteract;

        entity.AddedToSheet = addedtosheet;
        entity.AddedToPalette = addedtopalette;

        var ret = BalanceBin.GetBalanceRecordFromSpriteIndex((int)spriteTableIndex, GameMap.Info.BalanceLevel);
        entity.BalanceRecord = ret;
        entity.Hp = ret.Hp;
        entity.HpMax = ret.Hp;

        InitCodePrograms(entity);

        SetEntityDimensions(entity, sprite.Header.Xmod, sprite.Header.Ymod, sprite.Header.Zmod, sprite.Header.Width, sprite.Header.Depth, sprite.Header.Height);

        entity.XPos = x;
        entity.YPos = y;
        entity.ZPos = z - entity.ZMod + 1;

        UpdateAnimation(entity);

        entity.ModdedXPos = entity.XPos + entity.XMod;
        entity.ModdedYPos = entity.YPos + entity.YMod;
        entity.ModdedZPos = entity.ZPos + entity.ZMod;

        var zhit = CollideWithMap(entity);

        entity.FloorHeight = zhit;
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
                var bittocheck = 1 << dif;
                if ((flag & bittocheck) != 0)
                {
                    entity.ContentsItemId = GetContentsItemId(0);
                    return;
                }
            }
            if (entity.EntityRecord.Contents != 0)
            {
                entity.ContentsItemId = GetContentsItemId((uint)entity.EntityRecord.Contents);
                return;
            }
        }
        else
        {
            entity.ContentsGameFlag = 0;
        }

        entity.ContentsItemId = GetContentsItemId(entity.Sprite.Header.Contents);
    }

    private uint GetContentsItemId(uint contentId)
    {
        do
        {
            if (contentId >= 0x100)
            {
                return 0;
            }

            if ((contentId & 0x80) == 0)
            {
                return contentId & (0u - (contentId < 0x62u ? 1u : 0u));
            }

            var i = StaticVariables.g_gameRandomSeed;
            var val1 = (int)(i * 0x7d2b89dd);
            var val2 = (int)(0xe06a02e7 + val1);
            var targetval = (int)(((long)val2 * 16) >> 32);
            StaticVariables.g_gameRandomSeed = (uint)val2;

            var tableid = contentId & 0x7f;
            //0x28db0 a table
            contentId = StaticVariables.g_contentsTable[tableid][targetval];
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

            var tile = GameMap.Map.MapTiles[tilex + tiley * 52];
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

    public void UpdateAnimation(Entity entity)
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
        var frameIndex = StaticVariables.g_frameIndexTable[directionIndex + (entity.CurrentFrameIndex << 3)];
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
                entity.TargetAnimationId = (uint)(frame.CollisionOffset & 0xff);
                entity.AnimCompleteCounter++;////we get here when the animation is nonrepeating and is finished, so it switches back to some other animation
                //call recursivly?
                UpdateAnimation(entity);
            }

            entity.AnimCompleteCounter++;
            entity.Frame = entity.FirstFrame;//the anim repeats

        } while (true);//will this ever be an infinite loop
    }

    public void SetEntityDimensions(Entity entity, int xmod, int ymod, int zmod, int width, int depth, int height)
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

    private SpriteRecord GetSpriteFromSpriteTable(bool isMapSprite, uint spritetableindex, out int addedtosheet, out int addedtopallette)
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
            return si.SpriteEffects[spritetableindex];
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

        var child = SpawnEntity(null, false, entity.ContentsItemId + 0x1e, entity.XPos, entity.YPos, entity.ZPos, 0);

        if (child == null)
        {
            return 0;
        }

        child.ZForce = 0xa0000;
        child.Bytes[0] = 1;

        child.Flags &= 0xff7f;

        //TODO: impliment this lookuptable
        /*int result = lookuptable[entity.ContentsItemId * 8];*/

        //if (result == 0)
        //    result = -1;

        //child.InitialXPos = result;

        child.InitialYPos = 0;

        child.AIValues.Set(0xa0000, 1);

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

    public MapEffectRecord GetMapEffectRecord(int id, bool checkBoundingBox)
    {
        if (id < GameMap.SpriteInfo.MapEffectRecords.Length)
        {
            var record = GameMap.SpriteInfo.MapEffectRecords[id];
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
            effect.AttachedEntity = entity;
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



    public Entity SpawnEntity(Entity ownerEntity, bool ismapsprite, uint tableindex, int xpos, int ypos, int zpos, uint dir)
    {
        int paletteIndex, sheetSize;

        var spriteRecord = GetSpriteFromSpriteTable(ismapsprite, tableindex, out paletteIndex, out sheetSize);

        if (spriteRecord == null)
        {
            return null;
        }

        var entity = AllocateEntitySlot();
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


    private Entity AllocateEntitySlot()
    {
        for (int i = 1; i < StaticVariables.g_entitySlots.Length; i++)
        {
            if (StaticVariables.g_entitySlots[i].Status == 0)
            {
                return StaticVariables.g_entitySlots[i];
            }
        }

        throw new("Character over execute");
        return null;

        //foreach (var entity in StaticVariables.g_entitySlots)
        //{
        //    if (entity.Status == 0)
        //    {
        //        return entity;
        //    }
        //}
        ////var newentity = new Entity { Index = g_entitySlots.Count + 1 };
        ////g_entitySlots.Add(newentity);
        ////return newentity;
        //return null;
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
}