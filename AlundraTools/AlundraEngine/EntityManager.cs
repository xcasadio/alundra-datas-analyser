using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;
using AlundraEngine.Gameplay.Scripts;
using AlundraEngine.Sound;
using System;
using System.Diagnostics;

namespace AlundraEngine;

public class EntityManager
{
    private readonly GameEngine _gameEngine;

    public EntityManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    public void InitializeEntitySlots()
    {
        //Entity entityCounter = null;
        //Entity writePtr = StaticVariables.PlayerEntity;
        //Entity readPtr = StaticVariables.g_emptyEntityForClearing;
        //Entity blockStart = writePtr;

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

        for (int i = 0; i < StaticVariables.g_entitySlots.Length; i++)
        {
            StaticVariables.g_entitySlots[i].Index = i;
            StaticVariables.g_entitySlots[i].EntityRefId = -1;
        }
    }

    // 80039ad0
    public Entity AllocateEntitySlot()
    {
        for (int i = 1; i < StaticVariables.g_entitySlots.Length; i++)
        {
            if (StaticVariables.g_entitySlots[i].Status == 0)
            {
                return StaticVariables.g_entitySlots[i];
            }
        }

        Debugger.Break();

        return null;
    }

    // 80039d04
    public void InitializeEntity(Entity entity, Entity parentEntity, SpriteRecord sprite, SiEntityRecord entityRecord,
        uint spriteTableIndex, int entityId, int x, int y, int z, uint animationId, uint direction, int paletteIndex,
        int sheetSize)
    {
        if (StaticVariables.g_numberOfEntity < entity.Index)
        {
            StaticVariables.g_numberOfEntity = entity.Index + 1;
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
        entity.EntityRecord = entityRecord;
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
        entity.Index2 = ++StaticVariables.g_nextEntityIndex;

        entity.CurrentAnimationId = ~animationId;
        entity.CurrentDirection = ~direction;
        entity.TargetAnimationId = animationId;
        entity.TargetDirection = direction;
        //uint flags = animData.Flags;
        entity.Flags = (uint)(sprite.Header.MoreFlags | sprite.Header.CanPickup << 8 |
                              sprite.Header.FlagsPortraitShadowType << 16);

        entity.SpriteProgramIndexes[ScriptHelper.ProgramALoad] = sprite.Header.ProgramLoad;
        entity.SpriteProgramIndexes[ScriptHelper.ProgramBMap] = 0;
        entity.SpriteProgramIndexes[ScriptHelper.ProgramCTick] = sprite.Header.ProgramTick;
        entity.SpriteProgramIndexes[ScriptHelper.ProgramDTouch] = sprite.Header.ProgramTouch;
        entity.SpriteProgramIndexes[ScriptHelper.ProgramEDeactivate] = sprite.Header.ProgramDeactivate;
        entity.SpriteProgramIndexes[ScriptHelper.ProgramFInteract] = sprite.Header.ProgramInteract;

        entity.AddedToPalette = paletteIndex;
        entity.AddedToSheet = sheetSize;

        //BalanceRecord balanceRecord = GetSpriteAnimationPtr(entity.SpriteTableIndex);
        BalanceRecord balanceRecord =
            _gameEngine.BalanceBin.GetBalanceRecordFromSpriteIndex((int)spriteTableIndex,
                _gameEngine.CurrentMap.Info.BalanceLevel);
        entity.BalanceRecord = balanceRecord;
        byte balanceHp = balanceRecord.Hp;
        entity.HpMax = balanceHp;
        entity.Hp = balanceHp;

        InitializeCodePrograms(entity);

        SetEntityDimensions(entity,
            sprite.Header.OffsetX, sprite.Header.OffsetY, sprite.Header.OffsetZ,
            sprite.Header.SizeX, sprite.Header.SizeY, sprite.Header.SizeZ);

        entity.PosX = x;
        entity.PosY = y;
        entity.PosZ = z - entity.ModZ + 1;

        UpdateAnimation(entity);

        entity.ModdedXPos = entity.PosX + entity.ModX;
        entity.ModdedYPos = entity.PosY + entity.ModY;
        entity.ModdedZPos = entity.PosZ + entity.ModZ;

        int height = ComputeEntityGroundHeight(entity);
        entity.TerrainHeight = height;

        if (entity.PosZ <= height + 1)
        {
            entity.PosZ = height + 1;
            entity.ModdedXPos = entity.PosX + entity.ModX;
            entity.ModdedYPos = entity.PosY + entity.ModY;
            entity.ModdedZPos = entity.PosZ + entity.ModZ;
        }

        UpdateTileAttributes(entity);
        _gameEngine.InitializeContents(entity);
    }

    private void InitializeCodePrograms(Entity entity)
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

    // 80039c40
    private void SetEntityDimensions(Entity entity, int offsetX, int offsetY, int offsetZ, int sizeX, int sizeY,
        int sizeZ)
    {
        entity.NegXMod = -(offsetX << 16);
        entity.NegYMod = -(offsetY << 16);
        entity.ModX = offsetX << 16;
        entity.ModY = offsetY << 16;
        entity.ModZ = offsetZ << 16;
        entity.ScreenClipX = 0x4e00000 - ((offsetX + sizeX) << 16);
        entity.ScreenClipY = 0x3c00000 - ((offsetY + sizeY) << 16);
        entity.ScreenClipZ = 0x7800000 - ((offsetZ + sizeZ) << 16);

        if (sizeX == 0)
        {
            entity.Width = 0;
        }
        else
        {
            entity.Width = (sizeX << 16) - 1;
        }

        if (sizeY == 0)
        {
            entity.Height = 0;
        }
        else
        {
            entity.Height = (sizeY << 16) - 1;
        }

        if (sizeZ == 0)
        {
            entity.Depth = 0;
        }
        else
        {
            entity.Depth = (sizeZ << 16) - 1;
        }
    }

    // 80038ab4
    private void UpdateAnimation(Entity entity)
    {
        AnimationSet? animRecordPtr;
        SiFrame? currentFrame;

        if (entity.CurrentAnimationId != entity.TargetAnimationId ||
            entity.CurrentDirection != entity.TargetDirection)
        {
            entity.CurrentAnimationId = entity.TargetAnimationId;
            entity.CurrentFrameIndex = 0;
            entity.AnimCompleteCounter = 0;
            animRecordPtr = entity.Sprite.AnimSets[entity.TargetAnimationId];
            currentFrame = animRecordPtr.PreloadedAnims[entity.TargetDirection >> 3].Frames[entity.CurrentFrameIndex];
            entity.AnimSet = animRecordPtr;
            entity.Frame = currentFrame;
            entity.FirstFrame = currentFrame;
            entity.IsZForceApplied = 0; // TODO entity.Sprite.Header.MoreFlags;

            entity.NextFrameDelay = entity.Frame.Delay & 0x7f;
            entity.ForceResetAnimationFlag = 0;
            entity.AnimFlags = entity.AnimSet.Flags; // TODO U6 ??
            entity.ZSortValue = entity.AnimSet.U6; // TODO U6 ??

            if (entity.BalanceRecord.NumAnimVals == 0)
            {
                entity.BalanceVal = null;
            }
            else
            {
                var index = entity.TargetAnimationId >= entity.BalanceRecord.NumAnimVals ? 0 : entity.TargetAnimationId;
                entity.BalanceVal = entity.BalanceRecord.AnimVals[index];
            }

            uint sfxId = entity.AnimSet.Sfx;
            if ((entity.AnimSet.Flags & 0x20) != 0)
            {
                sfxId += 0x100;
            }

            _gameEngine.PlaySoundEffect(sfxId);
        }
        else if (entity.NextFrameDelay != 0)
        {
            if (--entity.NextFrameDelay != 0)
            {
                return;
            }

            entity.Frame = entity.FirstFrame;
            entity.AnimCompleteCounter++;
        }

        uint frameFlags = entity.Frame.Delay;
        if ((frameFlags & 0x80) != 0)
        {
            entity.NextFrameDelay = (int)(frameFlags & 0x7F);
            animRecordPtr = entity.Sprite.AnimSets[entity.TargetAnimationId];
            currentFrame = animRecordPtr.PreloadedAnims[entity.TargetDirection >> 3].Frames[entity.CurrentFrameIndex];
            entity.Frame = currentFrame;
        }

        animRecordPtr = entity.Sprite.AnimSets[entity.TargetAnimationId];
        currentFrame = animRecordPtr.PreloadedAnims[entity.TargetDirection >> 3].Frames[entity.CurrentFrameIndex];

        if (currentFrame.CollisionData != null)
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

        if ((entity.Frame.Delay & 0x80) == 0 && (entity.Frame.TransformIndexLow & 0x80) == 0)
        {
            entity.TargetAnimationId = entity.Frame.TransformIndexLow;
            entity.ForceResetAnimationFlag = 1; // TODO ??
            UpdateAnimation(entity); // recursive call to update the animation
        }

        var anim = entity.AnimSet.PreloadedAnims[entity.TargetDirection >> 3];
        var nextFrameIndex = entity.CurrentFrameIndex + 1;

        if (nextFrameIndex >= anim.NumberOfFrames)
        {
            nextFrameIndex = 0;
            entity.ForceResetAnimationFlag = 1;
        }

        entity.CurrentFrameIndex = nextFrameIndex;
    }

    // 800370c4
    public int ComputeEntityGroundHeight(Entity entity)
    {
        var xs = new int[4];
        var ys = new int[4];
        var x1 = (entity.PosX + entity.ModX) >> 16;
        var x2 = (entity.PosX + entity.ModX + entity.Width) >> 16;
        var y1 = (entity.PosY + entity.ModY) >> 16;
        var y2 = (entity.PosY + entity.ModY + entity.Height) >> 16;
        xs[0] = x1;
        ys[0] = y1;
        xs[1] = x2;
        ys[1] = y1;
        xs[2] = x1;
        ys[2] = y2;
        xs[3] = x2;
        ys[3] = y2;
        int highest = 0;
        var slopesHit = 0;

        for (var i = 0; i < 4; i++)
        {
            var x = xs[i];
            var y = ys[i];
            var tileX = x / StaticVariables.MapTileWidth;
            //On PSX hardware we avoid using division, so we use a lookup table instead.
            //Debug.Assert(StaticVariables.g_tileToWorldXTable[x] == tileX); 

            if (tileX > 0)
            {
                if (tileX >= 0x34)
                {
                    tileX = 0x33;
                }
            }
            else
            {
                tileX = 0;
            }

            var tileY = y >> 4; // StaticVariables.MapTileHeight;
            if (tileY > 0)
            {
                if (tileY >= 0x3c)
                {
                    tileY = 0x3b;
                }
            }
            else
            {
                tileY = 0;
            }

            var mapWidth = _gameEngine.CurrentMap.Map.Width;
            var tile = _gameEngine.CurrentMap.Map.MapTiles[tileY * mapWidth + tileX];
            entity.MapTiles[i] = tile;
            int height = tile.Height;

            switch (tile.Slope & 0x3)
            {
                case 0:
                    height <<= 4;
                    break;
                case 1:
                    if ((slopesHit & 6) == 0)
                    {
                        var result = StaticVariables.g_heights_800236d4[0x17 - ys[i] % 0x18];
                        height = ((tile.Height - 1) << 4) + result;
                    }

                    slopesHit |= 1;
                    break;

                case 2:
                    if ((slopesHit & 5) == 0) //it already hit 1 or 3
                    {
                        var result = StaticVariables.g_heights_800236d4[0x17 - xs[i] % 0x18];
                        height = ((tile.Height - 1) << 4) + result;
                    }

                    slopesHit |= 2;
                    break;

                case 3:
                    if ((slopesHit & 3) == 0) //it already hit 1 or 2
                    {
                        var result = StaticVariables.g_heights_800236d4[xs[i] % 0x18];
                        height += result;
                    }

                    slopesHit |= 4;
                    break;
            }

            height = height << 16;

            entity.MapHeights[i] = height;

            if (highest < height)
            {
                highest = height;
            }
        }

        entity.FloorHeight = highest;

        return highest;
    }

    /*
    // 80038064
    private void UpdateTileAttributes(Entity entity)
    {
        //set of variables set by certain special frames of animation
        if (entity.FrameCollision != null)
        {
            entity.HitBoxX = entity.PosX + entity.FrameXOff;
            entity.HitBoxY = entity.PosY + entity.FrameYOff;
            entity.HitBoxZ = entity.PosZ + entity.FrameZOff;
        }

        entity.TileX = (entity.PosX >> 16) / 24;
        entity.TileY = entity.PosY >> 20;
        entity.TileZ = entity.PosZ >> 20; //(z >> 16) / 16

        var hitz = _gameEngine.GetCollisionOnZ(entity);
        int tohit;
        entity.FloorHeight = hitz;
        entity.CollidedWithEntityZ = hitz < entity.PosZ ? 0 : 1;

        if ((entity.Flags & 0x100) != 0)
        {
            tohit = 0xe00;
            var someVals = new int[4];

            for (var i = 0; i < 4; i++)
            {
                var tl = entity.MapTiles[i];
                var fullVal = tl.Walkability | tl.GroundProperty << 8 | tl.Slope << 16 | tl.SizeZ << 24;

                if (entity.MapHeights[i] + 1 == entity.ModdedZPos)
                {
                    //var val = (tl.groundproperty & 0xe) << 8;
                    if ((fullVal & 0xe00) < tohit)
                    {
                        someVals[i] = fullVal;
                        tohit = fullVal & 0xe00;
                    }
                }
                else
                {
                    someVals[i] = 0;
                    tohit = 0;
                }
            }

            entity.CombinedVramFlagsOR = someVals[0] | someVals[1] | someVals[2] | someVals[3];
            entity.CombinedVramFlagsAND = someVals[0] & someVals[1] & someVals[2] & someVals[3];

            var tileX = entity.TileX;
            if (tileX > 0)
            {
                if (tileX >= 0x34)
                {
                    tileX = 0x33;
                }
            }
            else
            {
                tileX = 0;
            }

            var tileY = entity.TileY;
            if (tileY > 0)
            {
                if (tileY >= 0x3c)
                {
                    tileY = 0x3b;
                }
            }
            else
            {
                tileY = 0;
            }

            var tile = _gameEngine.CurrentMap.Map.MapTiles[tileX + tileY * 52];
            var fullVal2 = tile.Walkability | tile.GroundProperty << 8 | tile.Slope << 16 | tile.SizeZ << 24;
            var height = (int)(fullVal2 & 0xff000000 >> 4) + 1;
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
    */

    // 8003b388
    public void UpdateEntities()
    {
        if ((StaticVariables.g_playerControlFlags & 0x48) == 0)
        {
            UpdateDestroyedEntities();
            UpdateEntitiesEvents();
            UpdateEntitiesCounters();
            //
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

        UpdateVisibleEntitiesZSort();

        if (StaticVariables.g_visibleEntityCount > 0) // add spriterefs
        {
            for (var i = 0; i < StaticVariables.g_visibleEntityCount; i++)
            {
                var entity = StaticVariables.g_visibleEntities[i];

                entity.SpriteRef.DepthSortVal = entity.ZSortValue;
                entity.SpriteRef.X = entity.PosX;
                entity.SpriteRef.Y = entity.PosY;
                entity.SpriteRef.Z = entity.PosZ;
                StaticVariables.g_spriteImages[StaticVariables.g_spriteNumberOfImage++] = entity.SpriteRef;
            }
        }
    }

    // 80038364
    private void UpdateEntitiesPhysics()
    {
        for (var i = 0; i < StaticVariables.g_activeEntityCount; i++)
        {
            var entity = StaticVariables.g_activeEntities[i];
            entity.PlatformUpdateFlag = 0;
            entity.CollidedWithEntityZ = 0;
            entity.ForceAdjusted = 0;

            entity.ModdedXPos = entity.PosX + entity.ModX;
            entity.ModdedYPos = entity.PosY + entity.ModY;
            entity.ModdedZPos = entity.PosZ + entity.ModZ;
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
            if (entity.PlatformUpdateFlag == 0)
            {
                MoveEntity(entity);
            }
        }

        for (var i = 0; i < StaticVariables.g_activeEntityCount; i++)
        {
            var entity = StaticVariables.g_activeEntities[i];
            UpdateTileAttributes(entity);
        }
    }

    // 80037364
    private void UpdateRidingEntity(Entity entity, Entity ridingEntity)
    {
        if (ridingEntity.RidingEntity != null)
        {
            //TODO: bug maybe in CheckRidingEntities why overflow ???
            UpdateRidingEntity(ridingEntity, ridingEntity.RidingEntity);
        }

        entity.FinalXForce += ridingEntity.AdjustedXForce;
        entity.FinalYForce += ridingEntity.AdjustedYForce;
        if (entity.IsZForceApplied == 0)
        {
            entity.ForceZ = ridingEntity.FinalZForce;
            entity.FinalYForce = ridingEntity.FinalZForce;
        }
    }

    // 80037e34
    private void MoveEntity(Entity entity)
    {
        int updatedZPosition;
        Entity platformEntity;

        platformEntity = entity.PlatformEntity;
        entity.PlatformUpdateFlag = 1;

        if (platformEntity == null)
        {
            ComputeZPosition(entity);
            platformEntity = ComputeXYPosition(entity);
            entity.XCollisionEntity = platformEntity;
        }
        else
        {
            if (platformEntity.PlatformUpdateFlag == 0)
            {
                MoveEntity(platformEntity);
            }

            entity.CollidedWithEntityZ = 1;
            entity.ForceAdjusted = 1;
            updatedZPosition = platformEntity.FloorHeight;
            entity.XCollisionEntity = null;
            entity.FloorHeight = updatedZPosition;
            entity.TerrainHeight = platformEntity.TerrainHeight;
            entity.PosX = platformEntity.PosX + platformEntity.RelativeWarpOffsetX;
            entity.PosY = platformEntity.PosY + platformEntity.RelativeWarpOffsetY;
            updatedZPosition = platformEntity.PosZ + platformEntity.RelativeWarpOffsetZ;
            entity.ModdedXPos = entity.PosX + entity.ModX;
            entity.PosZ = updatedZPosition;
            entity.ModdedYPos = entity.PosY + entity.ModY;
            entity.ModdedZPos = updatedZPosition + entity.ModZ;
        }
    }

    // 800375e0
    private void ComputeZPosition(Entity entity)
    {
        int groundHeight;
        int finalZVelocity;
        int platformHeight;
        Entity platformEntity;

        finalZVelocity = entity.FinalZForce;

        if (finalZVelocity < 1)
        {
            groundHeight = ComputeEntityGroundHeight(entity);
            entity.TerrainHeight = groundHeight;

            while (CheckEntityCollisionDown(entity, out platformHeight, out platformEntity))
            {
                if (platformEntity == null || platformEntity.PlatformUpdateFlag != 0)
                {
                    entity.CollidedWithEntityZ = 1;
                    entity.PosZ = platformHeight - entity.ModZ;
                    if ((entity.Flags & 0x100) == 0)
                    {
                        return;
                    }

                    entity.ForceZ = 0;
                    return;
                }

                MoveEntity(platformEntity);
            }
        }
        else
        {
            groundHeight = ComputeEntityGroundHeight(entity);
            entity.TerrainHeight = groundHeight;

            while (CheckEntityCollisionUp(entity, out platformHeight, out platformEntity))
            {
                if (platformEntity == null || platformEntity.PlatformUpdateFlag != 0)
                {
                    entity.CollidedWithEntityZ = 1;
                    entity.PosZ = platformHeight - entity.ModZ - entity.Depth;
                    if ((entity.Flags & 0x100) == 0)
                    {
                        return;
                    }

                    entity.ForceZ = 0;
                    return;
                }

                MoveEntity(platformEntity);
            }
        }

        entity.PosZ = entity.PosZ + finalZVelocity;
    }

    // 80036bfc
    private bool CheckEntityCollisionDown(Entity entity, out int platformHeight, out Entity platformEntity)
    {
        int deltaX;
        Entity candidateEntity;
        int deltaY;
        int platformTopZ;
        Entity bestCandidate;
        bool collisionDetected;
        Entity[] collidableEntityPtr;
        int entityIndex;

        platformTopZ = entity.ModdedZPos + entity.FinalZForce;
        collisionDetected = platformTopZ <= entity.TerrainHeight;
        bestCandidate = null;

        if (collisionDetected)
        {
            platformTopZ = entity.TerrainHeight + 1;
        }

        if ((entity.Flags & 0x80) != 0 && (entity.AnimFlags & 0x80) == 0 && entity.PlatformEntity == null)
        {
            entityIndex = 0;

            if (StaticVariables.g_collideableEntitiesCount > 0)
            {
                collidableEntityPtr = StaticVariables.g_collideableEntities;

                do
                {
                    candidateEntity = collidableEntityPtr[entityIndex];

                    if (entity != candidateEntity)
                    {
                        deltaY = candidateEntity.ModdedZPos + candidateEntity.Depth;

                        if (deltaY < entity.ModdedZPos && platformTopZ <= deltaY)
                        {
                            deltaX = candidateEntity.ModdedXPos - entity.ModdedXPos;

                            if (deltaX < 0)
                            {
                                if (entity.ModdedXPos - candidateEntity.ModdedXPos < candidateEntity.Width + 1)
                                {
                                    deltaX = candidateEntity.ModdedYPos - entity.ModdedYPos;

                                    if (deltaX < 0)
                                    {
                                        if (entity.ModdedYPos - candidateEntity.ModdedYPos < candidateEntity.Height + 1)
                                        {
                                            platformTopZ = deltaY + 1;
                                            collisionDetected = true;
                                            bestCandidate = candidateEntity;
                                        }
                                    }
                                    else if (deltaX < entity.Height + 1)
                                    {
                                        platformTopZ = deltaY + 1;
                                        collisionDetected = true;
                                        bestCandidate = candidateEntity;
                                    }
                                }
                            }
                            else if (deltaX < entity.Width + 1)
                            {
                                deltaX = candidateEntity.ModdedYPos - entity.ModdedYPos;

                                if (deltaX < 0)
                                {
                                    if (entity.ModdedYPos - candidateEntity.ModdedYPos < candidateEntity.Height + 1)
                                    {
                                        platformTopZ = deltaY + 1;
                                        collisionDetected = true;
                                        bestCandidate = candidateEntity;
                                    }
                                }
                                else if (deltaX < entity.Height + 1)
                                {
                                    platformTopZ = deltaY + 1;
                                    collisionDetected = true;
                                    bestCandidate = candidateEntity;
                                }
                            }
                        }
                    }

                    entityIndex++;
                } while (entityIndex < StaticVariables.g_collideableEntitiesCount);
            }
        }

        platformHeight = platformTopZ;
        platformEntity = bestCandidate;
        return collisionDetected;
    }

    // 80036d94
    private bool CheckEntityCollisionUp(Entity entity, out int platformHeight, out Entity platformEntity)
    {
        int deltaX;
        int entityTopZ;
        Entity candidateEntity;
        int candidateZPos;
        //Entity[] collidableEntityPtr;
        int platformCandidateZ;
        int entityIndex;
        bool collisionDetected;
        Entity bestCandidate;

        entityTopZ = entity.ModdedZPos + entity.Depth;
        platformCandidateZ = entityTopZ + entity.FinalZForce;
        collisionDetected = 0x7800000 < platformCandidateZ;
        bestCandidate = null;

        if (collisionDetected)
        {
            platformCandidateZ = 0x77fffff;
        }

        if ((entity.Flags & 0x80) != 0
            && (entity.AnimFlags & 0x80) == 0
            && entity.PlatformEntity == null)
        {
            entityIndex = 0;
            if (StaticVariables.g_collideableEntitiesCount > 0)
            {
                //collidableEntityPtr = StaticVariables.g_collideableEntities;
                do
                {
                    candidateEntity = StaticVariables.g_collideableEntities[entityIndex];

                    if (entity != candidateEntity)
                    {
                        candidateZPos = candidateEntity.ModdedZPos;

                        if (entityTopZ < candidateZPos
                            && candidateZPos <= platformCandidateZ)
                        {
                            deltaX = candidateEntity.ModdedXPos - entity.ModdedXPos;

                            if (deltaX < 0)
                            {
                                if (entity.ModdedXPos - candidateEntity.ModdedXPos < candidateEntity.Width + 1)
                                {
                                    deltaX = candidateEntity.ModdedYPos - entity.ModdedYPos;

                                    if (deltaX < 0)
                                    {
                                        if (entity.ModdedYPos - candidateEntity.ModdedYPos < candidateEntity.Height + 1)
                                        {
                                            platformCandidateZ = candidateZPos - 1;
                                            collisionDetected = true;
                                            bestCandidate = candidateEntity;
                                        }
                                    }
                                    else if (deltaX < entity.Height + 1)
                                    {
                                        platformCandidateZ = candidateZPos - 1;
                                        collisionDetected = true;
                                        bestCandidate = candidateEntity;
                                    }
                                }
                            }
                            else if (deltaX < entity.Width + 1)
                            {
                                deltaX = candidateEntity.ModdedYPos - entity.ModdedYPos;

                                if (deltaX < 0)
                                {
                                    if (entity.ModdedYPos - candidateEntity.ModdedYPos < candidateEntity.Height + 1)
                                    {
                                        platformCandidateZ = candidateZPos - 1;
                                        collisionDetected = true;
                                        bestCandidate = candidateEntity;
                                    }
                                }
                                else if (deltaX < entity.Height + 1)
                                {
                                    platformCandidateZ = candidateZPos - 1;
                                    collisionDetected = true;
                                    bestCandidate = candidateEntity;
                                }
                            }
                        }
                    }

                    entityIndex = entityIndex + 1;
                } while (entityIndex < StaticVariables.g_collideableEntitiesCount);
            }
        }

        platformHeight = platformCandidateZ;
        platformEntity = bestCandidate;
        return collisionDetected;
    }

    // 80037730
    Entity ComputeXYPosition(Entity entity)
    {
        Entity candidate = null;
        int i;
        int s6 = -1;
        int dy, dx;
        int groundHeight;
        int dz;
        int zTolerance;
        int posX, posY, posZ;
        uint[] collisionFlags = new uint[4];

        int modX = 0;
        int didAdjustForObstacle = 0;
        int isStraightDir = (entity.TargetDirection & 7) == 0 ? 1 : 0;

        Func<Entity, uint[], uint> collisionFunc =
            entity == StaticVariables.g_entitySlots[0] ? GetCollisionFlagsWithPlayer : GetCollisionFlags;

        START_COLLISION_CHECK:
        dx = entity.FinalXForce;
        dy = entity.FinalYForce;

        if (dx == 0 && dy == 0)
        {
            goto FINALIZE_NO_MOVE;
        }

        modX = 0;
        candidate = null;
        i = 0;

        TRY_ADVANCE:
        posX = entity.PosX;
        posY = entity.PosY;
        posZ = entity.PosZ;

        collisionFlags[0] = 0;
        collisionFlags[1] = 0;
        collisionFlags[2] = 0;
        collisionFlags[3] = 0;

        entity.PosX += dx;
        entity.PosY += dy;
        entity.ModdedXPos = entity.PosX + entity.ModX;
        entity.ModdedYPos = entity.PosY + entity.ModY; // -> 47235072 = 47628288 + -393216
        entity.ModdedZPos = entity.PosZ + entity.ModZ; // 5242881 -> 5308417

        groundHeight = ComputeEntityGroundHeight(entity);
        entity.TerrainHeight = groundHeight;

        int halfDxVal = dx >> 1;
        int halfDyVal = dy >> 1;

        // Essai de “collage” au sol si 0x100 (gravity) et pas de zForce
        if ((entity.Flags & 0x100) != 0 && entity.ForceZ == 0)
        {
            dz = groundHeight - entity.ModdedZPos - 1;
            zTolerance = 0x30000;
            if (dz < 0)
            {
                zTolerance = 0x30003;
                dz = -dz;
            }

            if (dz < zTolerance)
            {
                int savedZ = entity.PosZ;
                entity.PosZ = groundHeight + 1; //5242881 -> 5308417
                entity.ModdedXPos = entity.PosX + entity.ModX;
                entity.ModdedYPos = entity.PosY + entity.ModY;
                entity.ModdedZPos = entity.PosZ + entity.ModZ;

                if (FindEntityCollisionCandidate(entity) != null)
                {
                    /* collision verticale : on restaure Z */
                    entity.PosZ = savedZ;
                    entity.ModdedZPos = savedZ + entity.ModZ;
                    entity.ModdedXPos = entity.PosX + entity.ModX;
                    entity.ModdedYPos = entity.PosY + entity.ModY;
                    //goto RESTORE_POS;
                }
                /* sinon : on garde ce nouvel essai et on poursuit avec collision décor…   */
            }
            //else
            //{
            //    //RESTORE_POS:
            //    // on repassera dans collision décor juste après
            //}
        }

        CHECK_ENTITY_COLLISION:
        uint flags = collisionFunc(entity, collisionFlags);

        if (flags == 0) // aucun obstacle détecté
        {
            modX = 1;

            if (i == 0)
            {
                goto FINALIZE_OK;
            }

            if (dx == -1)
            {
                halfDxVal = 0;
            }

            if (dy == -1)
            {
                halfDyVal = 0;
            }

            if (isStraightDir == 0)
            {
                if (halfDxVal == 0)
                {
                    goto FINALIZE_OK;
                }
            }
            else if (halfDxVal != 0)
            {
                i++;
                dx = halfDxVal;
                dy = halfDyVal;
                goto TRY_ADVANCE;
            }

            if (halfDyVal == 0)
            {
                goto FINALIZE_OK;
            }

            i++;
            dx = halfDxVal;
            dy = halfDyVal;
            goto TRY_ADVANCE;
        }

        LAB_80037938:
        entity.PosX = posX;
        entity.PosY = posY;
        entity.PosZ = posZ;

        if (dx == -1)
        {
            halfDxVal = 0;
        }

        if (dy == -1)
        {
            halfDyVal = 0;
        }

        if (isStraightDir != 0)
        {
            if (halfDxVal != 0)
            {
                dx = halfDxVal;
                dy = halfDyVal;
                goto TRY_ADVANCE;
            }

            if (halfDyVal == 0)
            {
                goto LAB_8003799C;
            }

            i++;
            dx = halfDxVal;
            dy = halfDyVal;
            goto TRY_ADVANCE;
        }
        else
        {
            if (halfDxVal != 0 && halfDyVal != 0)
            {
                i++;
                dx = halfDxVal;
                dy = halfDyVal;
                goto TRY_ADVANCE;
            }
        }

        LAB_8003799C:
        if (modX != 0)
        {
            goto LAB_80037D58;
        }

        if (didAdjustForObstacle == 1 || (entity.Flags & 0x2000) != 0 || candidate != null)
        {
            goto FINAL_OBSTACLE;
        }

        didAdjustForObstacle = 1;

        switch (entity.TargetDirection)
        {
            case 0:
                if ((collisionFlags[2] != 0 && collisionFlags[3] != 0) ||
                    collisionFlags[0] != 0 || collisionFlags[1] != 0)
                {
                    goto FINAL_OBSTACLE;
                }

                entity.FinalYForce = 0;
                if (collisionFlags[2] != 0 && collisionFlags[3] == 0)
                {
                    entity.FinalXForce = 0xC000;
                }

                goto START_COLLISION_CHECK;

            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
                if (collisionFlags[0] != 0)
                {
                    if (collisionFlags[3] == 0)
                    {
                        LAB_80037C70:
                        entity.FinalXForce = 0;
                        goto START_COLLISION_CHECK;
                    }

                    goto FINAL_OBSTACLE;
                }

                if (collisionFlags[3] != 0)
                {
                    LAB_80037C88:
                    entity.FinalYForce = 0;
                }

                goto START_COLLISION_CHECK;

            case 8:
                if ((collisionFlags[0] != 0 && collisionFlags[2] != 0) ||
                    collisionFlags[1] != 0 || collisionFlags[3] != 0)
                {
                    goto FINAL_OBSTACLE;
                }

                entity.FinalXForce = 0;
                if (collisionFlags[0] != 0 && collisionFlags[2] == 0)
                {
                    entity.FinalYForce = 0x8000;
                }
                else if (collisionFlags[0] == 0 && collisionFlags[2] != 0)
                {
                    code_r0x80037C3C:
                    if (collisionFlags[0] == 0)
                    {
                        entity.FinalYForce = -0x8000;
                    }
                }

                goto START_COLLISION_CHECK;

            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
                if (collisionFlags[1] == 0)
                {
                    if (collisionFlags[2] != 0)
                    {
                        entity.FinalXForce = 0;
                    }
                }
                else
                {
                    if (collisionFlags[2] != 0)
                    {
                        goto FINAL_OBSTACLE;
                    }

                    entity.FinalYForce = 0;
                }

                goto START_COLLISION_CHECK;

            case 16:
                if ((collisionFlags[0] != 0 && collisionFlags[1] != 0) ||
                    collisionFlags[2] != 0 || collisionFlags[3] != 0)
                {
                    goto FINAL_OBSTACLE;
                }

                entity.FinalYForce = 0;
                if (collisionFlags[0] != 0 && collisionFlags[1] == 0)
                {
                    entity.FinalXForce = 0xC000;
                }

                goto START_COLLISION_CHECK;

            case 17:
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
                if (collisionFlags[0] != 0 && collisionFlags[3] != 0)
                {
                    goto FINAL_OBSTACLE;
                }

                if (collisionFlags[3] == 0)
                {
                    if (collisionFlags[0] != 0)
                    {
                        entity.FinalYForce = 0;
                    }
                }
                else
                {
                    //goto LAB_80037C70; /* FinalXForce = 0 */
                    entity.FinalXForce = 0;
                    goto START_COLLISION_CHECK;
                }

                goto START_COLLISION_CHECK;

            case 24:
                if ((collisionFlags[1] != 0 && collisionFlags[3] != 0)
                    || collisionFlags[0] != 0 || collisionFlags[2] != 0)
                {
                    goto FINAL_OBSTACLE;
                }

                entity.FinalXForce = 0;
                if (collisionFlags[1] != 0 && collisionFlags[3] == 0)
                {
                    entity.FinalYForce = 0x8000;
                }
                else if (collisionFlags[1] == 0 && collisionFlags[3] != 0)
                {
                    //goto code_r0x80037C3C; /* FinalYForce = -0x8000 */
                    if (collisionFlags[0] == 0)
                    {
                        entity.FinalYForce = -0x8000;
                    }
                }

                goto START_COLLISION_CHECK;

            case 25:
            case 26:
            case 27:
            case 28:
            case 29:
            case 30:
            case 31:
                if (collisionFlags[1] != 0 && collisionFlags[2] != 0)
                {
                    goto FINAL_OBSTACLE;
                }

                if (collisionFlags[2] != 0)
                {
                    //goto LAB_80037C88;
                    entity.FinalYForce = 0;
                }

                if (collisionFlags[1] != 0)
                {
                    entity.FinalXForce = 0;
                }

                goto START_COLLISION_CHECK;

            default:
                goto FINAL_OBSTACLE;
        }

        LAB_80037D58:
        //Cas “pas d’obstacle” (modX != 0)
        dy = entity.PosX;
        dx = entity.ModX;
        goto FINALIZE_COMMON;

        FINAL_OBSTACLE:
        entity.ForceAdjusted = 1;

        FINALIZE_OK:
        dy = entity.PosX;
        dx = entity.ModX;

        FINALIZE_COMMON:
        entity.ModdedXPos = dy + dx;
        entity.ModdedYPos = entity.PosY + entity.ModY;
        entity.ModdedZPos = entity.PosZ + entity.ModZ;
        entity.TerrainHeight = ComputeEntityGroundHeight(entity);
        return candidate;

        FINALIZE_NO_MOVE:
        entity.ModdedXPos = entity.PosX + entity.ModX;
        entity.ModdedYPos = entity.PosY + entity.ModY;
        entity.ModdedZPos = entity.PosZ + entity.ModZ;
        entity.TerrainHeight = ComputeEntityGroundHeight(entity);
        return null;
    }

    private void UpdateEntityPositions(Entity entity, bool clearForceAdjusted = true)
    {
        if (clearForceAdjusted)
        {
            entity.ForceAdjusted = 1;
        }

        entity.ModdedXPos = entity.PosX + entity.ModX;
        entity.ModdedYPos = entity.PosY + entity.ModY;
        entity.ModdedZPos = entity.PosZ + entity.ModZ;
        entity.TerrainHeight = ComputeEntityGroundHeight(entity);
    }

    public uint GetCollisionFlagsWithPlayer(Entity entity, uint[] collisionFlags)
    {
        uint flags;
        Entity player;
        uint[] colFlags;
        int index;
        int flag;
        bool gravityFlag;
        int lockTimer;
        int moddedZPos;

        moddedZPos = StaticVariables.g_entitySlots[0].ModdedZPos;
        lockTimer = StaticVariables.g_warpLockTimer;

        if (StaticVariables.g_debugState > -1 || (StaticVariables.g_debugFlags & 0x80000000) == 0)
        {
            flag = 0x40;

            if ((StaticVariables.g_entitySlots[0].Flags & 8U) != 0)
            {
                flag = 0x41;
            }

            if ((StaticVariables.g_entitySlots[0].Flags & 1U) != 0)
            {
                flag |= 0x1000;
            }

            index = 0;
            gravityFlag = StaticVariables.g_gravityFlag < 2;
            colFlags = collisionFlags;
            player = StaticVariables.g_entitySlots[0];

            for (int i = 0; i < 4; i++)
            {
                if ((player.MapTiles[i].Flags & flag) != 0 || moddedZPos <= player.MapHeights[i])
                {
                    colFlags[index] = 1;
                }

                if (gravityFlag && (player.MapTiles[i].Flags & 0xe00) == 0x800)
                {
                    colFlags[index] = 1;
                }

                if (lockTimer == 0x20
                    && moddedZPos == player.MapHeights[i] + 1
                    && (player.MapTiles[i].Flags & 0xe00) == 0x600)
                {
                    colFlags[index] = 1;
                }
            }

            flags = collisionFlags[0] | collisionFlags[1] | collisionFlags[2] | collisionFlags[3];
        }
        else
        {
            flags = 0;
        }

        return flags;
    }

    // 800373e4
    private uint GetCollisionFlags(Entity entity, uint[] flags)
    {
        ushort flag;
        int moddedZPos;

        flag = 0x40;

        if ((entity.Flags & 0x8) != 0) // 0x8 = « traverse cliff ? »
        {
            flag = 0x41;
        }

        if ((entity.Flags & 0x1) != 0) // 0x1 = « prend en compte les trous »
        {
            flag |= 0x1000;
        }

        moddedZPos = entity.ModdedZPos;

        for (int i = 0; i < 4; i++)
        {
            flags[i] = 0;

            if ((entity.MapTiles[i].Flags & flag) != 0 // != 0
                && entity.MapHeights[i] >= moddedZPos) // la case est plus basse
                //if ((entity.MapTiles[i].Flags & flag) != 0 
                //    || moddedZPos <= entity.MapHeights[i])
            {
                flags[i] = 1;
            }
        }

        return flags[0] | flags[1] | flags[2] | flags[3];
    }

    // 80036f34
    public Entity? FindEntityCollisionCandidate(Entity entity)
    {
        int value;
        Entity currentEntity;
        Entity[] collideableEntities;

        if ((entity != StaticVariables.g_entitySlots[0] || StaticVariables.g_debugState > -1 ||
             (StaticVariables.g_debugFlags & 0x80000000) == 0)
            && (entity.Flags & 0x80U) != 0
            && (entity.AnimFlags & 0x80U) == 0
            && entity.PlatformEntity == null)
        {
            if (StaticVariables.g_collideableEntitiesCount <= 0)
            {
                return null;
            }

            collideableEntities = StaticVariables.g_collideableEntities;

            for (int i = 0; i < StaticVariables.g_collideableEntitiesCount; i++)
            {
                currentEntity = collideableEntities[i];

                if (entity == currentEntity)
                {
                    continue;
                }

                value = currentEntity.ModdedXPos - entity.ModdedXPos;
                if (value < 0)
                {
                    if (entity.ModdedXPos - currentEntity.ModdedXPos < currentEntity.Width + 1)
                    {
                        value = currentEntity.ModdedYPos - entity.ModdedYPos;
                        if (value < 0)
                        {
                            if (entity.ModdedYPos - currentEntity.ModdedYPos < currentEntity.Height + 1)
                            {
                                value = currentEntity.ModdedZPos - entity.ModdedZPos;
                                if (value < 0)
                                {
                                    if (entity.ModdedZPos - currentEntity.ModdedZPos < currentEntity.Depth + 1)
                                    {
                                        return currentEntity;
                                    }
                                }
                                else if (value < entity.Depth + 1)
                                {
                                    return currentEntity;
                                }
                            }
                        }
                        else if (value < entity.Height + 1)
                        {
                            value = currentEntity.ModdedZPos - entity.ModdedZPos;
                            if (value < 0)
                            {
                                if (entity.ModdedZPos - currentEntity.ModdedZPos < currentEntity.Depth + 1)
                                {
                                    return currentEntity;
                                }
                            }
                            else if (value < entity.Depth + 1)
                            {
                                return currentEntity;
                            }
                        }
                    }
                }
                else if (value < entity.Width + 1)
                {
                    value = currentEntity.ModdedYPos - entity.ModdedYPos;
                    if (value < 0)
                    {
                        if (entity.ModdedYPos - currentEntity.ModdedYPos < currentEntity.Height + 1)
                        {
                            value = currentEntity.ModdedZPos - entity.ModdedZPos;
                            if (value < 0)
                            {
                                if (entity.ModdedZPos - currentEntity.ModdedZPos < currentEntity.Depth + 1)
                                {
                                    return currentEntity;
                                }
                            }
                            else if (value < entity.Depth + 1)
                            {
                                return currentEntity;
                            }
                        }
                    }
                    else if (value < entity.Height + 1)
                    {
                        value = currentEntity.ModdedZPos - entity.ModdedZPos;
                        if (value < 0)
                        {
                            if (entity.ModdedZPos - currentEntity.ModdedZPos < currentEntity.Depth + 1)
                            {
                                return currentEntity;
                            }
                        }
                        else if (value < entity.Depth + 1)
                        {
                            return currentEntity;
                        }
                    }
                }
            }
        }

        return null;
    }

    // CheckRidingEntities
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
                var otherEntity = StaticVariables.g_collideableEntities[j];
                if (entity == otherEntity)
                {
                    continue;
                }

                if (otherEntity.ModdedZPos + otherEntity.Depth + 1 == entity.ModdedZPos)
                {
                    if ((otherEntity.ModdedXPos - entity.ModdedXPos < 0
                         && entity.ModdedXPos - otherEntity.ModdedXPos < otherEntity.Width + 1)
                        || otherEntity.ModdedXPos - entity.ModdedXPos < entity.Width + 1)
                    {
                        var val = otherEntity.ModdedYPos - entity.ModdedYPos;

                        if (val < 0)
                        {
                            if (entity.ModdedYPos - otherEntity.ModdedYPos < otherEntity.Height + 1)
                            {
                                entity.RidingEntity = otherEntity;
                                break;
                            }
                        }
                        else if (val < entity.Height + 1)
                        {
                            entity.RidingEntity = otherEntity;
                            break;
                        }
                    }
                }

                //if ((otherEntity.ModdedXPos - entity.ModdedXPos >= 0 && otherEntity.ModdedXPos - entity.ModdedXPos < entity.SizeX + 1) 
                //    || (otherEntity.ModdedXPos - entity.ModdedXPos < 0 && entity.ModdedXPos - otherEntity.ModdedXPos < otherEntity.SizeX + 1))
                //{
                //    if (otherEntity.ModdedYPos - entity.ModdedYPos >= 0 && otherEntity.ModdedYPos - entity.ModdedYPos < entity.SizeY + 1)
                //    {
                //        entity.RidingEntity = otherEntity;
                //        break;
                //    }
                //
                //    if (otherEntity.ModdedYPos - entity.ModdedYPos < 0 && entity.ModdedYPos - otherEntity.ModdedYPos < otherEntity.SizeY + 1)
                //    {
                //        entity.RidingEntity = otherEntity;
                //        break;
                //    }
                //}
            }
        }
    }

    // 80036828
    public void UpdateEntitiesForces()
    {
        int spriteZForceTemp;
        int zForceMax;
        uint targetXForce;
        uint xForce;
        uint targetYForce;
        uint yForce;
        int spriteZForceAbs;

        for (int i = 0; i < StaticVariables.g_activeEntityCount; i++)
        {
            var entity = StaticVariables.g_activeEntities[i];

            if (entity == StaticVariables.PlayerEntity)
            {
                if (entity.IsZForceApplied == 0)
                {
                    if ((entity.Flags & 0x100U) != 0)
                    {
                        spriteZForceTemp = entity.ForceZ + _gameEngine.CurrentMap.Info.Gravity * -0x100;

                        spriteZForceAbs = spriteZForceTemp;
                        if (spriteZForceTemp < 0)
                        {
                            spriteZForceAbs = -spriteZForceTemp;
                        }

                        zForceMax = _gameEngine.CurrentMap.Info.TerminalVelocity * 0x100;
                        entity.ForceZ = spriteZForceTemp;
                        if (zForceMax < spriteZForceAbs && spriteZForceTemp < 1)
                        {
                            entity.ForceZ = _gameEngine.CurrentMap.Info.TerminalVelocity * -0x100;
                        }
                    }
                }
                else if ((entity.Flags & 0x100U) == 0
                         || (entity.CombinedVramFlagsOR & 0x10U) == 0
                         || 0 < StaticVariables.g_gravityFlag)
                {
                    entity.ForceZ = entity.IsZForceApplied << 8;
                }
                else
                {
                    entity.ForceZ = entity.IsZForceApplied * 0xa0;
                }

                UpdateEntityPhysics(entity);

                xForce = (uint)entity.ForceStepX;
                yForce = (uint)entity.ForceStepY;

                if ((entity.CombinedVramFlagsOR & 0x20U) != 0)
                {
                    xForce =
                        (uint)(((ulong)entity.ForceStepX * 0x1000) >> 0x10) |
                        (uint)(((long)entity.ForceStepX * 0x1000) >> 0x20) << 0x10;

                    yForce =
                        (uint)(((ulong)entity.ForceStepY * 0x1000) >> 0x10) |
                        (uint)(((long)entity.ForceStepY * 0x1000) >> 0x20) << 0x10;
                }

                targetXForce = (uint)entity.TargetXForce;
                targetYForce = (uint)entity.TargetYForce;

                if ((entity.CombinedVramFlagsOR & 8U) != 0
                    && StaticVariables.g_gravityFlag < 1)
                {
                    targetXForce =
                        (uint)(((long)entity.TargetXForce * 0x8000) >> 0x10) |
                        (uint)(((long)entity.TargetXForce * 0x8000) >> 0x20) << 0x10;

                    targetYForce =
                        (uint)(((long)entity.TargetYForce * 0x8000) >> 0x10) |
                        (uint)(((long)entity.TargetYForce * 0x8000) >> 0x20) << 0x10;
                }

                entity.ForceX = IncrementForce(entity.ForceX, (int)targetXForce, (int)xForce);
                entity.ForceY = IncrementForce(entity.ForceY, (int)targetYForce, (int)yForce);

                //LABEL_ProcessFinalForces:
                ApplyEntityForces(entity);
                entity.FinalXForce = entity.AdjustedXForce;
                entity.FinalYForce = entity.AdjustedYForce;
                entity.FinalZForce = entity.ForceZ;
            }
            else
            {
                if (entity.PlatformEntity == null)
                {
                    if (entity.IsZForceApplied == 0)
                    {
                        if ((entity.Flags & 0x100U) != 0)
                        {
                            var force = entity.ForceZ - (_gameEngine.CurrentMap.Info.Gravity << 8);
                            var forceAbs = force;
                            if (force < 0)
                            {
                                forceAbs = -force;
                            }

                            var terminal = _gameEngine.CurrentMap.Info.TerminalVelocity << 8;
                            if (terminal < forceAbs && force < 1)
                            {
                                force = -terminal;
                            }

                            entity.ForceZ = force;
                        }
                    }
                    else if ((short)entity.IsZForceApplied == -0x8000
                             && (entity.Flags & 0x100U) == 0)
                    {
                        entity.ForceZ = 0;
                    }
                    else
                    {
                        entity.ForceZ = entity.IsZForceApplied << 8;
                    }

                    UpdateEntityPhysics(entity);

                    entity.ForceX = IncrementForce(entity.ForceX, entity.TargetXForce, entity.ForceStepX);
                    entity.ForceY = IncrementForce(entity.ForceY, entity.TargetYForce, entity.ForceStepY);

                    //goto LABEL_ProcessFinalForces;
                    ApplyEntityForces(entity);
                    entity.FinalXForce = entity.AdjustedXForce;
                    entity.FinalYForce = entity.AdjustedYForce;
                    entity.FinalZForce = entity.ForceZ;
                    continue;
                }

                entity.ForceZ = 0;
                entity.ForceY = 0;
                entity.ForceX = 0;
                entity.AdjustedYForce = 0;
                entity.AdjustedXForce = 0;
                entity.FinalZForce = 0;
                entity.FinalYForce = 0;
                entity.FinalXForce = 0;
            }
        }
    }

    // 800366fc
    private void ApplyEntityForces(Entity entity)
    {
        var adjustedXForce = entity.PreviousAdjustedXForce;
        var adjustedYForce = entity.PreviousAdjustedYForce;
        var shiftAmount = _gameEngine.CurrentMap.Info.Gravity & 0x1f;
        var xForceComponent = entity.ForceX + ScriptHelper.XForceTable[entity.TileAttributes & 0xf] >> shiftAmount;
        var yForceComponent = entity.ForceY + ScriptHelper.YForceTable[entity.TileAttributes & 0xf] >> shiftAmount;
        xForceComponent += adjustedXForce;
        yForceComponent += adjustedYForce;

        entity.PreviousAdjustedYForce = 0;
        entity.PreviousAdjustedXForce = 0;

        if (entity.PosX + xForceComponent < entity.NegXMod || entity.ScreenClipX < entity.PosX + xForceComponent)
        {
            xForceComponent = entity.ScreenClipX - entity.PosX;
            entity.ForceAdjusted = 1;
        }

        if (entity.PosY + yForceComponent < entity.NegYMod || entity.ScreenClipY < entity.PosY + yForceComponent)
        {
            yForceComponent = entity.ScreenClipY - entity.PosY;
            entity.ForceAdjusted = 1;
        }

        entity.AdjustedXForce = (int)xForceComponent;
        entity.AdjustedYForce = (int)yForceComponent;
    }

    // 800367e4
    private int IncrementForce(int force, int targetForce, int step)
    {
        if (targetForce != force)
        {
            if (force < targetForce)
            {
                force = force + step;

                if (targetForce < force)
                {
                    return targetForce;
                }
            }
            else
            {
                force = force - step;

                if (force < targetForce)
                {
                    return targetForce;
                }
            }
        }

        return force;
    }

    // 80036614
    private void UpdateEntityPhysics(Entity entity)
    {
        if (entity.Speed == entity.AnimSet.Speed
            && entity.TargetDirection == entity.CurrentDirection)
        {
            if (entity.Acceleration == (entity.AnimSet.U6 & 0xf)) //acceleration ?
            {
                return;
            }
        }
        else
        {
            entity.CurrentDirection = entity.TargetDirection;
            entity.Speed = entity.AnimSet.Speed;
            entity.TargetXForce = StaticVariables.g_offsetXList[entity.TargetDirection] * entity.AnimSet.Speed;
            entity.TargetYForce = StaticVariables.g_offsetYList[entity.TargetDirection] * entity.AnimSet.Speed;
        }

        entity.Acceleration = entity.AnimSet.U6 & 0xf; // acceleration ?
        entity.ForceStepX = Math.Abs(entity.TargetXForce - entity.ForceX) >> entity.Acceleration;
        entity.ForceStepY = Math.Abs(entity.TargetYForce - entity.ForceY) >> entity.Acceleration;
    }

    // 800399b8
    private void UpdateVisibleEntitiesZSort()
    {
        if (StaticVariables.g_visibleEntityCount <= 0)
        {
            return;
        }

        for (var i = 0; i < StaticVariables.g_visibleEntityCount; i++)
        {
            var entity = StaticVariables.g_visibleEntities[i];
            entity.ZSortValue = 0;
            entity.ZSortDepth = entity.ModdedZPos + entity.Depth;
        }

        for (var i = 0; i < StaticVariables.g_visibleEntityCount; i++)
        {
            var entity = StaticVariables.g_visibleEntities[i];
            if (entity.ZSortValue == 0)
            {
                ComputeZSortValue(entity);
            }
        }

        for (var i = 0; i < StaticVariables.g_visibleEntityCount; i++)
        {
            var entity = StaticVariables.g_visibleEntities[i];
            entity.ZSortValue = (int)(entity.ZSortValue & 0xffff0000) + (entity.PosZ >> 16);
        }
    }

    // 800397ac
    private int ComputeZSortValue(Entity entity)
    {
        if (entity.ZSortValue != 0)
        {
            return entity.ZSortValue;
        }

        var sortValue = entity.PosY + (entity.SpriteRef.DepthSortVal << 16);
        if ((entity.Flags & 0x80) != 0
            || (entity.AnimFlags & 0x80) != 0)
        {
            entity.ZSortValue = sortValue; // 47824896
            return sortValue;
        }

        if (entity.PlatformEntity != null)
        {
            if (entity.PlatformEntity.ZSortValue == 0)
            {
                sortValue = ComputeZSortValue(entity.PlatformEntity);
            }

            if (sortValue < entity.PlatformEntity.ZSortValue)
            {
                entity.ZSortValue = entity.PlatformEntity.ZSortValue;
                return entity.PlatformEntity.ZSortValue;
            }
        }

        for (var dex = 0; dex < StaticVariables.g_collideableEntitiesCount; dex++)
        {
            var otherEntity = StaticVariables.g_collideableEntities[dex];
            if (otherEntity == entity)
            {
                continue;
            }

            if (otherEntity.ZSortDepth >= entity.ZSortDepth)
            {
                continue;
            }

            //X
            var x = otherEntity.PosX + otherEntity.ModX - entity.ModdedXPos;
            if (x >= 0)
            {
                if (x >= entity.Width + 1)
                {
                    continue;
                }
            }
            else
            {
                if (entity.ModdedXPos - (otherEntity.PosX + otherEntity.ModX) >= otherEntity.Width + 1)
                {
                    continue;
                }
            }

            //Y
            var y = otherEntity.PosY + otherEntity.ModY - entity.ModdedYPos;
            if (y >= 0)
            {
                if (y >= entity.Depth + 1)
                {
                    continue;
                }
            }
            else
            {
                if (entity.ModdedYPos - (otherEntity.PosY + otherEntity.ModY) >= otherEntity.Depth + 1)
                {
                    continue;
                }
            }

            if (otherEntity.ZSortValue == 0)
            {
                sortValue = ComputeZSortValue(otherEntity);
            }

            if (sortValue < otherEntity.ZSortValue)
            {
                sortValue = otherEntity.ZSortValue;
            }
        }

        entity.ZSortValue = sortValue;

        return sortValue;
    }

    private void UpdateBalanceRecords()
    {
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
                var difx = entity.HitBoxX - checkme.ModdedXPos;
                int width;
                if (difx > 0)
                {
                    width = checkme.Width + 1;
                }
                else
                {
                    difx = checkme.ModdedXPos - entity.HitBoxX;
                    width = entity.FrameWidth + 1;
                }

                if (difx >= width)
                {
                    continue;
                }

                //Y
                var dify = entity.HitBoxY - checkme.ModdedYPos;
                int depth;
                if (dify > 0)
                {
                    depth = checkme.Depth + 1;
                }
                else
                {
                    dify = checkme.ModdedYPos - entity.HitBoxY;
                    depth = entity.FrameDepth + 1;
                }

                if (dify >= depth)
                {
                    continue;
                }

                //Z
                var difz = entity.HitBoxZ - checkme.ModdedZPos;
                int height;
                if (difz > 0)
                {
                    height = checkme.Height + 1;
                }
                else
                {
                    difz = checkme.ModdedZPos - entity.HitBoxZ;
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
                        _gameEngine.EffectManager.CreateAttachedEffect(0, 4, 0, checkme, width, 0, 0, 0);
                    }

                    if (valdex == 7 || valdex == 9)
                    {
                        _gameEngine.EffectManager.CreateAttachedEffect(0, 5, 0, checkme, 1, 0, 0, 0);
                    }

                    checkme.TouchingEntity = entity;
                }

                checkme.FrameColTickCounter = 0x19;

                entity.HitCounter++;
                //X
                var xr = checkme.ModdedXPos + checkme.Width;
                if (entity.HitBoxX + entity.FrameWidth < xr)
                {
                    xr = entity.HitBoxX + entity.FrameWidth;
                }

                var xl = entity.HitBoxX;
                if (entity.HitBoxX < checkme.ModdedXPos)
                {
                    xl = checkme.ModdedXPos;
                }

                //Y
                var yr = checkme.ModdedYPos + checkme.Depth;
                if (entity.HitBoxY + entity.FrameDepth < yr)
                {
                    yr = entity.HitBoxY + entity.FrameDepth;
                }

                var yl = entity.HitBoxY;
                if (entity.HitBoxY < checkme.ModdedYPos)
                {
                    yl = checkme.ModdedYPos;
                }

                //Z
                var zr = checkme.ModdedZPos + checkme.Height;
                if (entity.HitBoxZ + entity.FrameHeight < zr)
                {
                    zr = entity.HitBoxZ + entity.FrameHeight;
                }

                var zl = entity.HitBoxZ;
                if (entity.HitBoxZ < checkme.ModdedZPos)
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
            var effect = _gameEngine.EffectManager.CreateEffect_Type0(0, 9, 0, x, y, z);
            if (effect == null)
            {
                continue;
            }

            var baseForce = 0xffff << 16;

            var i = StaticVariables.g_gameRandomSeed;
            var val1 = i * 0x7d2b89dd;
            var val2 = 0xe06a02e7 + val1;
            var targetVal = (int)(((long)val2 * 0x20001) >> 32);
            StaticVariables.g_gameRandomSeed = val2;

            effect.ForceX = targetVal + baseForce;

            i = StaticVariables.g_gameRandomSeed;
            val1 = i * 0x7d2b89dd;
            val2 = 0xe06a02e7 + val1;
            targetVal = (int)(((long)val2 * 0x20001) >> 32);
            StaticVariables.g_gameRandomSeed = val2;

            effect.ForceY = targetVal + baseForce;

            i = StaticVariables.g_gameRandomSeed;
            val1 = i * 0x7d2b89dd;
            val2 = 0xe06a02e7 + val1;
            targetVal = (int)(((long)val2 * 0x20001) >> 32);
            StaticVariables.g_gameRandomSeed = val2;

            effect.ForceZ = targetVal + baseForce;
        }
    }

    // 80038e84
    private void UpdateActiveEffects()
    {
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
                effect = _gameEngine.EffectManager.CreateEffect_Type3(0, 0, 0, entity, -1, 0, 0, 0);
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

            var animid = -1;
            if ((entity.Slope_18c == 4 || entity.Slope_190 == 4)
                && entity.Slope_18c != entity.Slope_190)
            {
                //sliding effect
                _gameEngine.EffectManager.CreateEffect_Type0(0, 6, 0, entity.PosX, entity.PosY,
                    entity.CollidedWithEntityZ);
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
                    effect.X = entity.PosX;
                    effect.Y = entity.PosY;
                    effect.Z = entity.CollidedWithEntityZ;
                    continue;
                case 4:
                    effect.Status = 1;
                    if ((entity.FrameCounter & 7) != 0)
                    {
                        continue;
                    }

                    if ((entity.ForceX | entity.ForceY) == 0)
                    {
                        continue;
                    }

                    _gameEngine.EffectManager.CreateEffect_Type0(0, 0x15, 0, entity.PosX, entity.PosY,
                        entity.CollidedWithEntityZ);
                    continue;
                case 3:
                    if ((entity.FrameCounter & 0x7) != 0)
                    {
                        break;
                    }

                    if ((entity.ForceX | entity.ForceY) == 0)
                    {
                        break;
                    }

                    _gameEngine.EffectManager.CreateEffect_Type0(0, _gameEngine.CurrentMap.Info.SlideEffectId, 0,
                        entity.PosX, entity.PosY, entity.CollidedWithEntityZ);
                    break;
                default:
                    break;
            }

            animid -= (entity.PosZ - entity.CollidedWithEntityZ) >> 20;

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

            effect.X = entity.PosX;
            effect.Y = entity.PosY;
            effect.Z = entity.CollidedWithEntityZ;
        }
    }

    // 80038e18
    private void UpdateEntitiesAnimation()
    {
        for (var i = 0; i < StaticVariables.g_activeEntityCount; i++)
        {
            var entity = StaticVariables.g_entitySlots[i];
            UpdateAnimation(entity);
            Debug.Assert(entity.AnimSet != null);
        }
    }

    //800386d0
    private void UpdateEntitiesEvents()
    {
        _gameEngine.MovePlayer();

        for (var i = 1; i < StaticVariables.g_numberOfEntity; i++)
        {
            var entity = StaticVariables.g_entitySlots[i];
            var eventProgramType = -1;

            if (entity.IsNotProcessable == 0)
            {
                switch (entity.Status)
                {
                    case (int)EntityStatus.Destroyed:
                    case (int)EntityStatus.FlagToDestroy:
                        eventProgramType = ScriptHelper.ProgramUnknown;
                        break;

                    case (int)EntityStatus.Loaded:
                        eventProgramType = ScriptHelper.ProgramALoad;
                        entity.Status = (int)EntityStatus.Normal;
                        break;

                    case (int)EntityStatus.Normal:
                        var flags = entity.Flags;
                        if ((flags & 0x100000) == 0 || entity.Slope_18c != 4)
                        {
                            if ((flags & 0x200000) == 0 || (entity.CombinedVramFlagsOR & 0x8004U) == 0)
                            {
                                if (((flags & 0x10) != 0 && (entity.ForceAdjusted != 0 || entity.IsAboveGround != 0))
                                    || ((flags & 0x20) != 0 && entity.HitCounter != 0)
                                    || ((flags & 0x40) != 0 && entity.ForceResetAnimationFlag != 0))
                                {
                                    entity.Status = 3;
                                    eventProgramType = ScriptHelper.ProgramEDeactivate;
                                    break;
                                }

                                eventProgramType = ScriptHelper.ProgramDTouch;

                                if (entity.TouchingEntity == null)
                                {
                                    eventProgramType = ScriptHelper.ProgramCTick;

                                    if (StaticVariables.g_activeCollisionEntity == entity)
                                    {
                                        eventProgramType = ScriptHelper.ProgramFInteract;

                                        if (entity.SpriteProgramIndexes[5] == 0 && entity.ProgramIndexes[5] == 0)
                                        {
                                            eventProgramType = ScriptHelper.ProgramCTick;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _gameEngine.DestroyEntity(entity, -1);
                                eventProgramType = ScriptHelper.ProgramUnknown;
                            }
                        }
                        else
                        {
                            _gameEngine.DestroyEntity(entity, 6);
                            eventProgramType = ScriptHelper.ProgramUnknown;
                        }

                        break;

                    case (int)EntityStatus.Deactivated:
                        eventProgramType = ScriptHelper.ProgramEDeactivate;
                        break;
                }
            }

            entity.EventTrigger = eventProgramType;
        }

        //run events
        bool keepGoing;

        do
        {
            keepGoing = false;
            for (var i = 1; i < StaticVariables.g_numberOfEntity; i++)
            {
                var entity = StaticVariables.g_entitySlots[i];

                if (entity.EventTrigger == ScriptHelper.ProgramUnknown)
                {
                    continue;
                }

                var programIndex = entity.ProgramIndexes[entity.EventTrigger] & 0x7f;

                if (programIndex == 0)
                {
                    // g_entityEventFunctionsByType => AI
                    _gameEngine.RunSpriteEvent(entity);
                }
                else
                {
                    _gameEngine.RunScript(entity, entity.EventTrigger);
                }

                entity.EventTrigger = -1;
                keepGoing = true;
            }
        } while (keepGoing);
    }

    // 80038998
    private void UpdateEntitiesCounters()
    {
        for (var i = 0; i <= StaticVariables.g_numberOfEntity; i++)
        {
            var entity = StaticVariables.g_entitySlots[i];

            entity.TouchingEntity = null;
            entity.RidingEntity = null;
            entity.HitCounter = 0;

            entity.FrameCounter++;

            if (entity.DamagedTickCounter != 0)
            {
                entity.DamagedTickCounter--;
            }

            if (entity.FrameColTickCounter != 0)
            {
                entity.FrameColTickCounter--;
            }
        }

        //displays debug records here
    }


    // 80038634
    private void UpdateDestroyedEntities()
    {
        var max = 0; // always player
        for (var i = 0; i < StaticVariables.g_entitySlots.Length; i++)
        {
            var entity = StaticVariables.g_entitySlots[i];

            if (entity.Status == 4)
            {
                //entity = new Entity(); //TODO: check if create bug with some code save a pointer on an entity
                entity.Clear();
                entity.Index = i;
                entity.EntityRefId = -1; // g_emptyEntityForClearing.EntityRefId == -1
                //entity.Index2 = 0;
                //...
                //StaticVariables.g_entitySlots[i] = entity;
            }
            else if (entity.Status != 0)
            {
                max = i;
            }
        }

        StaticVariables.g_numberOfEntity = max + 1;
    }

    // 800384f4
    private void UpdateEntityLists()
    {
        StaticVariables.g_activeEntityCount = 0;
        StaticVariables.g_collideableEntitiesCount = 0;
        StaticVariables.g_visibleEntityCount = 0;

        for (int i = 0; i < StaticVariables.g_numberOfEntity; i++)
        {
            var entity = StaticVariables.g_entitySlots[i];

            //processable
            if (entity.Status - 2 < 2 && entity.IsNotProcessable == 0)
            {
                StaticVariables.g_activeEntities[StaticVariables.g_activeEntityCount++] = entity;
            }

            //collidable
            if ((entity.Flags & 0x80) != 0 && (entity.AnimFlags & 0x80) == 0 && entity.IsNotProcessable == 0)
            {
                StaticVariables.g_collideableEntities[StaticVariables.g_collideableEntitiesCount++] = entity;
            }

            //renderable
            if (entity.Status - 2 < 2 &&
                (entity.DamagedTickCounter & 0x3) != 0x3) //flicker effect, every 3rd frame when being damaged
            {
                StaticVariables.g_visibleEntities[StaticVariables.g_visibleEntityCount++] = entity;
            }
        }
    }

    // 80038064
    private void UpdateTileAttributes(Entity entity)
    {
        int tileX;
        int tileY;
        uint tileAttr;
        uint tileFlags;
        uint bestFlagMask;
        uint[] tempFlags = new uint[4];

        if (entity.FrameCollision != null)
        {
            entity.HitBoxX = entity.PosX + entity.FrameXOff;
            entity.HitBoxY = entity.PosY + entity.FrameYOff;
            entity.HitBoxZ = entity.PosZ + entity.FrameZOff;
        }

        //entity.TileX = StaticVariables.g_tileToWorldXTable[entity.PosX + 2];
        entity.TileX = (entity.PosX >> 16) / StaticVariables.MapTileWidth;
        entity.TileY = entity.PosY >> 20;
        entity.TileZ = entity.PosZ >> 20;


        var hitz = _gameEngine.GetCollisionOnZ(entity);
        entity.TerrainHeight = hitz;
        entity.IsAboveGround = hitz < entity.PosZ ? 0 : 1;
        //entity.FloorHeight = hitz;
        //entity.CollidedWithEntityZ = hitz < entity.PosZ ? 0 : 1;

        if ((entity.Flags & 0x100U) == 0)
        {
            tileX = entity.TileX;
            bestFlagMask = 0;
            entity.CombinedVramFlagsOR = 0;
            entity.CombinedVramFlagsAND = 0;

            if (tileX < 1)
            {
                tileX = 0;
            }
            else if (0x33 < tileX)
            {
                tileX = 0x33;
            }

            tileY = entity.TileY;

            if (tileY < 1)
            {
                tileY = 0;
            }
            else if (0x3b < tileY)
            {
                tileY = 0x3b;
            }


            var tl = _gameEngine.CurrentMap.Map
                .MapTiles[tileX + tileY * 52]; //entity.MapTiles[tileY * 0xd0 + tileX * 4 + 0x302];
            tileFlags = tl.Flags;
            //tileFlags = StaticVariables.g_spriteVRAMPointer + tileY * 0xd0 + tileX * 4 + 0x302;
            if ((tileFlags & 0xc00000) == 0 || (tileFlags & 0x80000) == 0)
            {
                goto NoCollision;
            }
        }
        else
        {
            bestFlagMask = 0xe00;
            var i = 0;

            do
            {
                tileFlags = entity.MapTiles[i].Flags & 0xe00;
                if (entity.MapHeights[0] + 1 == entity.ModdedZPos)
                {
                    tempFlags[i] = entity.MapTiles[0].Flags;
                    if (tileFlags < bestFlagMask)
                    {
                        bestFlagMask = tileFlags;
                    }
                }
                else
                {
                    tempFlags[i] = 0;
                    bestFlagMask = 0;
                }

                i = i + 1;
            } while (i < 4);

            entity.CombinedVramFlagsOR = (int)(tempFlags[0] | tempFlags[1] | tempFlags[2] | tempFlags[3]);
            entity.CombinedVramFlagsAND = (int)(tempFlags[0] & tempFlags[1] & tempFlags[2] & tempFlags[3]);
            tileX = entity.TileX;

            if (tileX < 1)
            {
                tileX = 0;
            }
            else if (0x33 < tileX)
            {
                tileX = 0x33;
            }

            tileY = entity.TileY;

            if (tileY < 1)
            {
                tileY = 0;
            }
            else if (0x3b < tileY)
            {
                tileY = 0x3b;
            }

            var tl = _gameEngine.CurrentMap.Map
                .MapTiles[tileX + tileY * 52]; //entity.MapTiles[tileY * 0xd0 + tileX * 4 + 0x302];
            tileFlags = (uint)(tl.Walkability | tl.GroundProperty << 8 | tl.Slope << 16 | tl.Height << 24);
            //tileFlags = StaticVariables.g_spriteVRAMPointer + tileY * 0xd0 + tileX * 4 + 0x302;

            if ((tileFlags & 0xc00000) == 0)
            {
                goto NoCollision;
            }

            tileAttr = 0x40000;
            if (((tileFlags & 0xff000000) >> 4) + 1 != entity.ModdedZPos)
            {
                tileAttr = 0x80000;
            }

            if ((tileFlags & tileAttr) == 0)
            {
                goto NoCollision;
            }
        }

        tileAttr = 1U << ((int)(tileFlags >> 0x14) & 3);
        entity.TileAttributes = (int)tileAttr;
        if ((tileFlags & 0x800000) != 0)
        {
            entity.TileAttributes = (int)(tileAttr | 0x80);
        }

        FinishUpdate:
        tileX = entity.Slope_18c;
        entity.Slope_18c = (int)bestFlagMask >> 9;
        entity.Slope_190 = tileX;
        return;

        NoCollision:
        entity.TileAttributes = 0;
        goto FinishUpdate;
    }

    // 8003a374
    public bool UpdateEntityFacingDirection(Entity entity)
    {
        int damage;
        int strLength;
        BalanceRecord balanceRecord;
        string debugStr = string.Empty;

        if (entity == StaticVariables.PlayerEntity)
        {
            balanceRecord = StaticVariables.g_intArray_80127008[0];
        }
        else
        {
            /* Structure liée à la cible, probablement un script/AI */
            balanceRecord = entity.BalanceRecord;
        }

        /* Entité liée à l'effet ou au contexte de dégâts */
        damage = ResolveBalanceTarget(balanceRecord.AnimVals[0], entity.TouchingEntity, entity.Hp);

        if (StaticVariables.g_debugState < 0 && (StaticVariables.g_debugFlags & 0x800) != 0)
        {
            //debugStr = StaticVariables.g_messageDebug + entity.index * 0x100;
            if (StaticVariables.g_balanceHpTotal != -1)
            {
                if ((StaticVariables.g_balanceMultiplier & 0xc0U) == 0)
                {
                    if ((StaticVariables.g_balanceHpTotal & 0x80U) == 0)
                    {
                        debugStr += $"                     {StaticVariables.g_balanceHp}";
                    }
                    else
                    {
                        debugStr +=
                            $"      O{StaticVariables.g_balanceParams} + A{StaticVariables.g_balanceHp - StaticVariables.g_balanceParams} = T{StaticVariables.g_balanceParams}";
                    }

                    debugStr += $" (Parm) {StaticVariables.g_balanceMultiplier}\n\r";
                }

                if (damage == null)
                {
                    debugStr +=
                        $"              {StaticVariables.g_balanceResult} Damage(Result)HP M{entity.HpMax} C{entity.Hp} DEAD\n\r";
                    entity.Hp = 0;
                }
                else
                {
                    debugStr +=
                        $"               {StaticVariables.g_balanceResult} Damage(Result)HP M{entity.HpMax} C{entity.Hp} N{damage}\n\r";
                    entity.Hp = damage;
                }

                goto END;
            }

            debugStr += "   Balance patamator error(Result)No Damage\n\r";

            StaticVariables.g_messageDebug += debugStr;
        }

        entity.Hp = damage;

        END:
        //DisplayHpDebugString();
        //DisplayHpDebugString();
        //DoNothing();
        return damage == null;
    }

    // 8004464c
    private int ResolveBalanceTarget(BalanceAnimValRef balanceConfig, Entity targetEntity, int hp)
    {
        BalanceAnimValRef values;
        BalanceRecord[] balanceSources;
        BalanceRecord animPtr;
        int i;
        int adjustedHpValue;
        int newHp;
        byte balanceId;
        byte balanceMultiplier;
        bool isReduced;

        if (StaticVariables.g_debugState < 0 && (StaticVariables.g_debugFlags & 0x800) != 0)
        {
            StaticVariables.g_balanceHpTotal = -1;
        }

        if (targetEntity != null && balanceConfig != null)
        {
            balanceId = balanceConfig.Val;

            if (balanceId != 0)
            {
                // TODO understand this
                //int entityId = targetEntity + (balanceId & 0xf);
                //byte balanceMultiplier = targetEntity.Bytes[2];
                balanceMultiplier = 1;

                if ((balanceMultiplier & 0xc0) == 0)
                {
                    adjustedHpValue = balanceConfig.U2;

                    if ((balanceId & 0x80) != 0)
                    {
                        i = 0;
                        balanceSources = StaticVariables.g_balanceEffectSources;

                        do
                        {
                            animPtr = balanceSources[i];

                            if (animPtr != null)
                            {
                                if (animPtr.NumAnimVals == 0)
                                {
                                    values = null;
                                }
                                else if (StaticVariables.g_balanceAnimIndex + 1 < animPtr.NumAnimVals)
                                {
                                    values = animPtr.AnimVals[(StaticVariables.g_balanceAnimIndex << 1) + 0xf + 2];
                                    //values = animPtr.AnimVals[StaticVariables.g_balanceAnimIndex * 2 + 0xe];
                                }
                                else
                                {
                                    values = animPtr.AnimVals[0];
                                }

                                if (values != null)
                                {
                                    adjustedHpValue = adjustedHpValue + values.Val;
                                }
                            }

                            i = i + 1;
                        } while (i < 3);
                    }

                    i = (adjustedHpValue * balanceMultiplier) >> 4;
                    isReduced = i < hp;

                    if (i == 0)
                    {
                        i = 1;
                        isReduced = 1 < hp;
                    }

                    newHp = 0;

                    if (isReduced)
                    {
                        newHp = hp - i;
                    }

                    hp = newHp;

                    if (StaticVariables.g_debugState < 0 && (StaticVariables.g_debugFlags & 0x800) != 0)
                    {
                        StaticVariables.g_balanceHp = (short)adjustedHpValue;
                        StaticVariables.g_balanceParams = balanceConfig.U2;
                        StaticVariables.g_balanceResult = (short)i;
                        StaticVariables.g_balanceHpTotal = balanceConfig.Val;
                        StaticVariables.g_balanceMultiplier = balanceMultiplier;
                    }
                }
                else if ((balanceMultiplier & 0xc0) == 0x40)
                {
                    if (StaticVariables.g_debugState < 0 && (StaticVariables.g_debugFlags & 0x800) != 0)
                    {
                        StaticVariables.g_balanceHp = 0;
                        StaticVariables.g_balanceParams = 0;
                        StaticVariables.g_balanceResult = (short)hp;
                        StaticVariables.g_balanceHpTotal = balanceConfig.Val;
                        StaticVariables.g_balanceMultiplier = balanceMultiplier;
                    }

                    hp = 0;
                }
            }
        }

        return hp;
    }
}