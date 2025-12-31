using AlundraEngine.Balance;
using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;
using AlundraEngine.Gameplay.Scripts;
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

    //8003b24c
    public void InitializeEntitySlots()
    {
        for (int i = 0; i < _gameEngine.StaticVariables.g_entitySlots.Length; i++)
        {
            _gameEngine.StaticVariables.g_entitySlots[i].Clear();
            _gameEngine.StaticVariables.g_entitySlots[i].Index = i;
            _gameEngine.StaticVariables.g_entitySlots[i].EntityRefId = -1;
        }
    }

    // 80039ad0
    public Entity AllocateEntitySlot()
    {
        for (int i = 1; i < _gameEngine.StaticVariables.g_entitySlots.Length; i++)
        {
            if (_gameEngine.StaticVariables.g_entitySlots[i].Status == 0)
            {
                return _gameEngine.StaticVariables.g_entitySlots[i];
            }
        }

        Debugger.Break();

        return null;
    }

    // 80039d04
    public void InitializeEntity(Entity entity, Entity parentEntity, SpriteRecord spriteRecord, SiEntityRecord? entityRecord,
        uint spriteTableIndex, int entityId, int x, int y, int z, uint animationId, uint direction, int paletteIndex,
        int sheetSize)
    {
        if (_gameEngine.StaticVariables.g_numberOfEntities < entity.Index)
        {
            _gameEngine.StaticVariables.g_numberOfEntities = entity.Index + 1;
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

        entity.SpriteRecord = spriteRecord;
        entity.EntityRecord = entityRecord;
        entity.SpriteTableIndex = spriteTableIndex;
        //for debugging
        entity.SpriteName = EntityNames.GetName(entityRecord?.SpriteDirection ?? 0, spriteTableIndex);

        if (entityRecord != null)
        {
            entity.EntityRefId = entityId;
        }
        else
        {
            entity.EntityRefId = -1;
        }

        entity.Status = 1;
        entity.Index2 = ++_gameEngine.StaticVariables.g_nextEntityIndex;

        entity.CurrentAnimationId = ~animationId;
        entity.CurrentDirection = ~direction;
        entity.TargetAnimationId = animationId;
        entity.TargetDirection = direction;
        //uint flags = animData.Flags;
        entity.Flags = (uint)(spriteRecord.Header.MoreFlags | spriteRecord.Header.CanPickup << 8 |
                              spriteRecord.Header.FlagsPortraitShadowType << 16);

        entity.SpriteProgramIndexes[ScriptHelper.ProgramALoad] = spriteRecord.Header.ProgramLoad;
        entity.SpriteProgramIndexes[ScriptHelper.ProgramBMap] = 0;
        entity.SpriteProgramIndexes[ScriptHelper.ProgramCTick] = spriteRecord.Header.ProgramTick;
        entity.SpriteProgramIndexes[ScriptHelper.ProgramDTouch] = spriteRecord.Header.ProgramTouch;
        entity.SpriteProgramIndexes[ScriptHelper.ProgramEDeactivate] = spriteRecord.Header.ProgramDeactivate;
        entity.SpriteProgramIndexes[ScriptHelper.ProgramFInteract] = spriteRecord.Header.ProgramInteract;

        entity.PaletteOffset = paletteIndex;
        entity.SpriteSheetOffset = sheetSize;

        BalanceRecord balanceRecord = _gameEngine.BalanceBin.GetBalanceRecordFromSpriteIndex((int)spriteTableIndex, _gameEngine.StaticVariables.g_itemIdThreshold);
        entity.BalanceRecord = balanceRecord;
        byte balanceHp = balanceRecord.Hp;
        entity.HpMax = balanceHp;
        entity.Hp = balanceHp;

        InitializeCodePrograms(entity);

        SetEntityDimensions(entity,
            spriteRecord.Header.OffsetX, spriteRecord.Header.OffsetY, spriteRecord.Header.OffsetZ,
            spriteRecord.Header.SizeX, spriteRecord.Header.SizeY, spriteRecord.Header.SizeZ);

        entity.PosX = x;
        entity.PosY = y;
        entity.PosZ = z - entity.ModZ + 1;

        UpdateAnimation(entity);

        entity.ModdedPosX = entity.PosX + entity.ModX;
        entity.ModdedPosY = entity.PosY + entity.ModY;
        entity.ModdedPosZ = entity.PosZ + entity.ModZ;

        int height = ComputeEntityGroundHeight(entity);
        entity.TerrainHeight = height;

        if (entity.PosZ <= height + 1)
        {
            entity.PosZ = height + 1;
            entity.ModdedPosX = entity.PosX + entity.ModX;
            entity.ModdedPosY = entity.PosY + entity.ModY;
            entity.ModdedPosZ = entity.PosZ + entity.ModZ;
        }

        UpdateTileAttributes(entity);
        _gameEngine.InitializeContents(entity);
    }

    

    //8004201c
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
    private void SetEntityDimensions(Entity entity, 
        int offsetX, int offsetY, int offsetZ, 
        int sizeX, int sizeY, int sizeZ)
    {
        entity.NegModX = -(offsetX << 16);
        entity.NegModY = -(offsetY << 16);
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
        AnimationSet? animSet;
        SiFrame? currentFrame;

        entity.IsZForceApplied = 0;

        var animationDirectionTableIndex = ((entity.TargetDirection + 2) & 0x1C) + entity.AnimationDirection;

        if (animationDirectionTableIndex >= _gameEngine.StaticVariables.g_animationDirectionTable.Length)
        {
            Debugger.Break();
        }

        var animationDirectionFromTable = _gameEngine.StaticVariables.g_animationDirectionTable[animationDirectionTableIndex];

        if (entity.CurrentAnimationId != entity.TargetAnimationId ||
            entity.AnimationDirection != animationDirectionFromTable)
            //entity.CurrentDirection != entity.TargetDirection)
        {
            entity.CurrentAnimationId = entity.TargetAnimationId;
            entity.AnimationDirection = animationDirectionFromTable;
            entity.AnimCompleteCounter = 0;
            entity.AnimationFrameIndex = 0;
            animSet = null;

            try
            {
                animSet = entity.SpriteRecord.AnimSets[entity.TargetAnimationId];
            }
            catch (Exception e)
            {
                Debugger.Break();
            }

            currentFrame = null;
            try
            {
                currentFrame = animSet.PreloadedAnims[entity.TargetDirection >> 3].Frames[entity.AnimationFrameIndex];
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
            entity.AnimationSet = animSet;
            entity.Frame = currentFrame;
            entity.FirstFrame = currentFrame;
            entity.IsZForceApplied = animSet.IsZForceApplied;

            entity.NextFrameDelay = entity.Frame.Delay & 0x7f;
            entity.ForceResetAnimationFlag = 0;
            entity.AnimFlags = entity.AnimationSet.Acceleration; // TODO Acceleration ??

            if (entity.BalanceRecord.NumAnimVals == 0)
            {
                entity.BalanceAnimValRef = null;
            }
            else
            {
                var index = entity.TargetAnimationId + 1 >= entity.BalanceRecord.NumAnimVals ? 0 : entity.TargetAnimationId + 1;
                entity.BalanceAnimValRef = entity.BalanceRecord.AnimVals[index];
            }

            uint sfxId = entity.AnimationSet.Sfx;
            if ((entity.AnimationSet.Flags & 0x20) != 0)
            {
                sfxId += 0x100;
            }

            _gameEngine.SoundManager.PlaySoundEffect(sfxId);
        }
        else if (entity.NextFrameDelay != 0)
        {
            if (--entity.NextFrameDelay != 0)
            {
                return;
            }

            SiAnimation preloadedAnim = null;

            try
            {
                preloadedAnim = entity.AnimationSet.PreloadedAnims[entity.TargetDirection >> 3];
            }
            catch (Exception e)
            {
                Debugger.Break();
            }

            if (entity.AnimationFrameIndex == preloadedAnim.Frames.Length - 2)
            {
                var lastFrame = preloadedAnim.Frames[entity.AnimationFrameIndex + 1];

                if (lastFrame.Delay == 1)
                {
                    entity.AnimationFrameIndex = 0;
                    entity.Frame = entity.FirstFrame;
                    entity.AnimCompleteCounter++;
                }
                else if ((lastFrame.Delay & 0x80) == 0)
                {
                    if ((lastFrame.TransformIndexLow & 0x80) != 0)
                    {
                        entity.NextFrameDelay = 0x7fffffff;
                        entity.ForceResetAnimationFlag = 1;
                        return;
                    }

                    entity.TargetAnimationId = lastFrame.TransformIndexLow;
                    UpdateAnimation(entity); // recursive call to update the animation
                    return;
                }
            }
        }

        uint frameFlags = entity.Frame.Delay;
        if ((frameFlags & 0x80) != 0)
        {
            entity.NextFrameDelay = (int)(frameFlags & 0x7F);
            animSet = entity.SpriteRecord.AnimSets[entity.TargetAnimationId];
            currentFrame = animSet.PreloadedAnims[entity.TargetDirection >> 3].Frames[entity.AnimationFrameIndex];
            entity.Frame = currentFrame;
        }

        animSet = entity.SpriteRecord.AnimSets[entity.TargetAnimationId];
        currentFrame = animSet.PreloadedAnims[entity.TargetDirection >> 3].Frames[entity.AnimationFrameIndex];

        currentFrame = entity.Frame;

        if (currentFrame.CollisionData != null)
        {
            entity.FrameCollision = entity.Frame.CollisionData;
            entity.CollisionOffsetX = entity.FrameCollision.OffsetX << 16;
            entity.CollisionOffsetY = entity.FrameCollision.OffsetY << 16;
            entity.CollisionOffsetZ = entity.FrameCollision.OffsetZ << 16;
            entity.CollisionWidth = (entity.FrameCollision.Width << 16) - 1;
            entity.CollisionDepth = (entity.FrameCollision.Depth << 16) - 1;
            entity.CollisionHeight = (entity.FrameCollision.Height << 16) - 1;
        }
        else
        {
            entity.FrameCollision = null;
        }

        if (currentFrame.ImageSetPointer != -1)
        {
            entity.SpriteRef.Images = currentFrame.Images.Images;
            entity.SpriteRef.DepthSortValue = currentFrame.Images.DepthSortValue;
            entity.SpriteRef.NumberOfImages = currentFrame.Images.NumberOfImages;
        }
        else
        {
            entity.SpriteRef.Images = null;
            entity.SpriteRef.DepthSortValue = 0;
            entity.SpriteRef.NumberOfImages = 0;
        }

        var anim = entity.AnimationSet.PreloadedAnims[entity.TargetDirection >> 3];
        var nextFrameIndex = entity.AnimationFrameIndex + 1;

        if (nextFrameIndex >= anim.NumberOfFrames)
        {
            nextFrameIndex = 0;
            entity.Frame = entity.FirstFrame;
            entity.AnimCompleteCounter++;
        }
        else
        {
            if (entity.AnimationSet.PreloadedAnims[entity.TargetDirection >> 3].Frames[nextFrameIndex] == null)
            {
                entity.Frame = entity.FirstFrame;
            }
        }

        entity.AnimationFrameIndex = nextFrameIndex;
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
            var tileX = Math.Min(x / StaticVariables.MapTileWidth, 51);
            //On PSX hardware we avoid using division, so we use a lookup table instead.
            //x = Math.Clamp(x, 0, _gameEngine.StaticVariables.g_tileToWorldXTable.Length - 1);
            //Debug.Assert(_gameEngine.StaticVariables.g_tileToWorldXTable[x] == tileX); 

            tileX = Math.Clamp(tileX, 0, 0x33);
            var tileY = y / StaticVariables.MapTileHeight;
            tileY = Math.Clamp(tileY, 0, 0x3b);

            var mapWidth = _gameEngine.CurrentMap.Map.Width;
            var tile = _gameEngine.CurrentMap.Map.MapTiles[tileY * mapWidth + tileX];
            entity.MapTiles[i] = tile;
            int height = tile.Height;

            /*
            for (int j = 0; j < _gameEngine.CurrentMap.Map.MapTiles.Length; j++)
            {
                if (_gameEngine.CurrentMap.Map.MapTiles[j].Flags == 84180992)
                {
                    Debugger.Break();
                }

                if (_gameEngine.CurrentMap.Map.MapTiles[j].GroundProperty == 128)
                {
                    Debugger.Break();
                }
            }*/

            height = tile.Height * StaticVariables.MapTileHeight;

            switch (tile.Slope & 0x3)
            {
                case 0: //normal tile
                    break;

                case 1: //Stairs up/down
                    if ((slopesHit & 0x6) == 0)
                    {
                        int yInTile = ys[i];
                        int yMod = yInTile % StaticVariables.MapTileHeight;
                        height = (tile.Height - 1) * StaticVariables.MapTileHeight + StaticVariables.MapTileHeight - yMod;
                    }
                    else
                    {
                        //Debugger.Break();
                        height += StaticVariables.MapTileHeight;
                    }

                    slopesHit |= 1;
                    break;

                case 2: //ladders entering or stair side down
                    if ((slopesHit & 0x5) == 0)
                    {
                        int xPos = xs[i];
                        int xIndex = (23 - xs[i] % StaticVariables.MapTileWidth) % StaticVariables.MapTileWidth;
                        height = (tile.Height - 1) * StaticVariables.MapTileHeight + _gameEngine.StaticVariables.g_heights_800236d4[xIndex];
                    }
                    else
                    {
                        //Debugger.Break();
                        height += StaticVariables.MapTileHeight;
                    }

                    slopesHit |= 2;
                    break;

                case 3: //ladders exiting
                    if ((slopesHit & 0x3) == 0)
                    {
                        int xPos = xs[i];
                        int remainder = xPos % StaticVariables.MapTileWidth;
                        height = (tile.Height - 1) * StaticVariables.MapTileHeight + _gameEngine.StaticVariables.g_heights_800236d4[remainder];
                    }
                    else
                    {
                        //Debugger.Break();
                        height += StaticVariables.MapTileHeight;
                    }

                    slopesHit |= 4;
                    break;
            }

            height <<= 16;
            entity.MapHeights[i] = height;

            if (highest < height)
            {
                highest = height;
            }
        }

        return highest;
    }

    // 8003b388
    public void UpdateEntities()
    {
        if ((_gameEngine.StaticVariables.g_playerControlFlags & 0x48) == 0)
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

        if (_gameEngine.StaticVariables.g_visibleEntityCount > 0)
        {
            for (var i = 0; i < _gameEngine.StaticVariables.g_visibleEntityCount; i++)
            {
                var entity = _gameEngine.StaticVariables.g_visibleEntities[i];

                entity.SpriteRef.DepthSortValue = entity.ZSortValue;
                entity.SpriteRef.X = entity.PosX;
                entity.SpriteRef.Y = entity.PosY;
                entity.SpriteRef.Z = entity.PosZ;
                _gameEngine.StaticVariables.g_spriteImages[_gameEngine.StaticVariables.g_spriteNumberOfImage++] = entity.SpriteRef;
            }
        }
    }

    // 80038364
    private void UpdateEntitiesPhysics()
    {
        for (var i = 0; i < _gameEngine.StaticVariables.g_activeEntityCount; i++)
        {
            var entity = _gameEngine.StaticVariables.g_activeEntities[i];
            entity.PlatformUpdateFlag = 0;
            entity.CollidedWithEntityZ = 0;
            entity.ForceAdjusted = 0;

            entity.ModdedPosX = entity.PosX + entity.ModX;
            entity.ModdedPosY = entity.PosY + entity.ModY;
            entity.ModdedPosZ = entity.PosZ + entity.ModZ;
        }

        CheckRidingEntities();
        UpdateEntitiesForces();

        for (var i = 0; i < _gameEngine.StaticVariables.g_collideableEntitiesCount; i++)
        {
            var entity = _gameEngine.StaticVariables.g_collideableEntities[i];
            if (entity.RidingEntity != null)
            {
                UpdateRidingEntity(entity, entity.RidingEntity);
            }
        }

        for (var i = 0; i < _gameEngine.StaticVariables.g_activeEntityCount; i++)
        {
            var entity = _gameEngine.StaticVariables.g_activeEntities[i];
            if (entity.PlatformUpdateFlag == 0)
            {
                MoveEntity(entity);
            }
        }

        for (var i = 0; i < _gameEngine.StaticVariables.g_activeEntityCount; i++)
        {
            var entity = _gameEngine.StaticVariables.g_activeEntities[i];
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

        entity.FinalForceX += ridingEntity.AdjustedForceX;
        entity.FinalForceY += ridingEntity.AdjustedForceY;
        if (entity.IsZForceApplied == 0)
        {
            entity.ForceZ = ridingEntity.FinalForceZ;
            entity.FinalForceZ = ridingEntity.FinalForceZ;
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

            updatedZPosition = platformEntity.PosZ + platformEntity.RelativeWarpOffsetZ;

            entity.CollidedWithEntityZ = 1;
            entity.ForceAdjusted = 1;
            entity.XCollisionEntity = null;
            entity.FloorHeight = platformEntity.FloorHeight;
            entity.TerrainHeight = platformEntity.TerrainHeight;
            entity.PosX = platformEntity.PosX + platformEntity.RelativeWarpOffsetX;
            entity.PosY = platformEntity.PosY + platformEntity.RelativeWarpOffsetY;
            entity.PosZ = updatedZPosition;
            entity.ModdedPosX = entity.PosX + entity.ModX;
            entity.ModdedPosY = entity.PosY + entity.ModY;
            entity.ModdedPosZ = updatedZPosition + entity.ModZ;
        }
    }

    // 800375e0
    private void ComputeZPosition(Entity entity)
    {
        int groundHeight;
        int finalZVelocity;
        int platformHeight;
        Entity platformEntity;

        finalZVelocity = entity.FinalForceZ;

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

        platformTopZ = entity.ModdedPosZ + entity.FinalForceZ;
        collisionDetected = platformTopZ <= entity.TerrainHeight;
        bestCandidate = null;

        if (collisionDetected)
        {
            platformTopZ = entity.TerrainHeight + 1;
        }

        if ((entity.Flags & 0x80) != 0 && (entity.AnimFlags & 0x80) == 0 && entity.PlatformEntity == null)
        {
            entityIndex = 0;

            if (_gameEngine.StaticVariables.g_collideableEntitiesCount > 0)
            {
                collidableEntityPtr = _gameEngine.StaticVariables.g_collideableEntities;

                do
                {
                    candidateEntity = collidableEntityPtr[entityIndex];

                    if (entity != candidateEntity)
                    {
                        deltaY = candidateEntity.ModdedPosZ + candidateEntity.Depth;

                        if (deltaY < entity.ModdedPosZ && platformTopZ <= deltaY)
                        {
                            deltaX = candidateEntity.ModdedPosX - entity.ModdedPosX;

                            if (deltaX < 0)
                            {
                                if (entity.ModdedPosX - candidateEntity.ModdedPosX < candidateEntity.Width + 1)
                                {
                                    deltaX = candidateEntity.ModdedPosY - entity.ModdedPosY;

                                    if (deltaX < 0)
                                    {
                                        if (entity.ModdedPosY - candidateEntity.ModdedPosY < candidateEntity.Height + 1)
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
                                deltaX = candidateEntity.ModdedPosY - entity.ModdedPosY;

                                if (deltaX < 0)
                                {
                                    if (entity.ModdedPosY - candidateEntity.ModdedPosY < candidateEntity.Height + 1)
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
                } while (entityIndex < _gameEngine.StaticVariables.g_collideableEntitiesCount);
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
        int platformCandidateZ;
        int entityIndex;
        bool collisionDetected;
        Entity bestCandidate;

        entityTopZ = entity.ModdedPosZ + entity.Depth;
        platformCandidateZ = entityTopZ + entity.FinalForceZ;
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
            if (_gameEngine.StaticVariables.g_collideableEntitiesCount > 0)
            {
                do
                {
                    candidateEntity = _gameEngine.StaticVariables.g_collideableEntities[entityIndex];

                    if (entity != candidateEntity)
                    {
                        candidateZPos = candidateEntity.ModdedPosZ;

                        if (entityTopZ < candidateZPos
                            && candidateZPos <= platformCandidateZ)
                        {
                            deltaX = candidateEntity.ModdedPosX - entity.ModdedPosX;

                            if (deltaX < 0)
                            {
                                if (entity.ModdedPosX - candidateEntity.ModdedPosX < candidateEntity.Width + 1)
                                {
                                    deltaX = candidateEntity.ModdedPosY - entity.ModdedPosY;

                                    if (deltaX < 0)
                                    {
                                        if (entity.ModdedPosY - candidateEntity.ModdedPosY < candidateEntity.Height + 1)
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
                                deltaX = candidateEntity.ModdedPosY - entity.ModdedPosY;

                                if (deltaX < 0)
                                {
                                    if (entity.ModdedPosY - candidateEntity.ModdedPosY < candidateEntity.Height + 1)
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
                } while (entityIndex < _gameEngine.StaticVariables.g_collideableEntitiesCount);
            }
        }

        platformHeight = platformCandidateZ;
        platformEntity = bestCandidate;
        return collisionDetected;
    }

    // 80037730
    private Entity ComputeXYPosition(Entity entity)
    {
        Entity candidate = null;
        int i = 0;
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
            entity == _gameEngine.StaticVariables.PlayerEntity ? GetCollisionFlagsWithPlayer : GetCollisionFlags;

        START_COLLISION_CHECK:
        dx = entity.FinalForceX;
        dy = entity.FinalForceY;

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
        entity.ModdedPosX = entity.PosX + entity.ModX;
        entity.ModdedPosY = entity.PosY + entity.ModY;
        entity.ModdedPosZ = entity.PosZ + entity.ModZ;

        groundHeight = ComputeEntityGroundHeight(entity);
        entity.TerrainHeight = groundHeight;

        int halfDxVal = dx >> 1;
        int halfDyVal = dy >> 1;

        // Ground snapping logic
        if ((entity.Flags & 0x100) != 0 && entity.ForceZ == 0)
        {
            dz = groundHeight - entity.ModdedPosZ - 1;
            zTolerance = 0x30000;
            if (dz < 0)
            {
                zTolerance = 0x30003;
                dz = -dz;
            }

            if (dz < zTolerance)
            {
                int savedZ = entity.PosZ;
                entity.PosZ = groundHeight + 1;
                entity.ModdedPosX = entity.PosX + entity.ModX;
                entity.ModdedPosY = entity.PosY + entity.ModY;
                entity.ModdedPosZ = entity.PosZ + entity.ModZ;

                candidate = FindEntityCollisionCandidate(entity);
                if (candidate != null)
                {
                    entity.PosZ = savedZ;
                    entity.ModdedPosZ = savedZ + entity.ModZ;
                    entity.ModdedPosX = entity.PosX + entity.ModX;
                    entity.ModdedPosY = entity.PosY + entity.ModY;
                    goto RESTORE_POS;
                }
            }
            else
            {
                goto RESTORE_POS;
            }
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
            
        goto LAB_80037938;

        RESTORE_POS:
        candidate = FindEntityCollisionCandidate(entity);
        if (candidate == null) goto CHECK_ENTITY_COLLISION;

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

                entity.FinalForceY = 0;
                if (collisionFlags[2] != 0 && collisionFlags[3] == 0)
                {
                    entity.FinalForceX = 0xC000;
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
                        entity.FinalForceX = 0;
                        goto START_COLLISION_CHECK;
                    }

                    goto FINAL_OBSTACLE;
                }

                if (collisionFlags[3] != 0)
                {
                    LAB_80037C88:
                    entity.FinalForceY = 0;
                }

                goto START_COLLISION_CHECK;

            case 8:
                if ((collisionFlags[0] != 0 && collisionFlags[2] != 0) ||
                    collisionFlags[1] != 0 || collisionFlags[3] != 0)
                {
                    goto FINAL_OBSTACLE;
                }

                entity.FinalForceX = 0;
                if (collisionFlags[0] != 0 && collisionFlags[2] == 0)
                {
                    entity.FinalForceY = 0x8000;
                }
                else if (collisionFlags[0] == 0 && collisionFlags[2] != 0)
                {
                    code_r0x80037C3C:
                    if (collisionFlags[0] == 0)
                    {
                        entity.FinalForceY = -0x8000;
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
                        entity.FinalForceX = 0;
                    }
                }
                else
                {
                    if (collisionFlags[2] != 0)
                    {
                        goto FINAL_OBSTACLE;
                    }

                    entity.FinalForceY = 0;
                }

                goto START_COLLISION_CHECK;

            case 16:
                if ((collisionFlags[0] != 0 && collisionFlags[1] != 0) ||
                    collisionFlags[2] != 0 || collisionFlags[3] != 0)
                {
                    goto FINAL_OBSTACLE;
                }

                entity.FinalForceY = 0;
                if (collisionFlags[0] != 0 && collisionFlags[1] == 0)
                {
                    entity.FinalForceX = 0xC000;
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
                        entity.FinalForceY = 0;
                    }
                }
                else
                {
                    //goto LAB_80037C70;
                    entity.FinalForceX = 0;
                    goto START_COLLISION_CHECK;
                }

                goto START_COLLISION_CHECK;

            case 24:
                if ((collisionFlags[1] != 0 && collisionFlags[3] != 0)
                    || collisionFlags[0] != 0 || collisionFlags[2] != 0)
                {
                    goto FINAL_OBSTACLE;
                }

                entity.FinalForceX = 0;
                if (collisionFlags[1] != 0 && collisionFlags[3] == 0)
                {
                    entity.FinalForceY = 0x8000;
                }
                else if (collisionFlags[1] == 0 && collisionFlags[3] != 0)
                {
                    //goto code_r0x80037C3C;
                    if (collisionFlags[0] == 0)
                    {
                        entity.FinalForceY = -0x8000;
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
                    entity.FinalForceY = 0;
                }

                if (collisionFlags[1] != 0)
                {
                    entity.FinalForceX = 0;
                }

                goto START_COLLISION_CHECK;

            default:
                goto FINAL_OBSTACLE;
        }

        LAB_80037D58:
        dy = entity.PosX;
        dx = entity.ModX;
        goto FINALIZE_COMMON;

        FINAL_OBSTACLE:
        entity.ForceAdjusted = 1;

        FINALIZE_OK:
        dy = entity.PosX;
        dx = entity.ModX;
        goto FINALIZE_COMMON;

        FINALIZE_COMMON:
        entity.ModdedPosX = dy + dx;
        entity.ModdedPosY = entity.PosY + entity.ModY;
        entity.ModdedPosZ = entity.PosZ + entity.ModZ;
        entity.TerrainHeight = ComputeEntityGroundHeight(entity);
        return candidate;

        FINALIZE_NO_MOVE:
        entity.ModdedPosX = entity.PosX + entity.ModX;
        entity.ModdedPosY = entity.PosY + entity.ModY;
        entity.ModdedPosZ = entity.PosZ + entity.ModZ;
        entity.TerrainHeight = ComputeEntityGroundHeight(entity);
        return candidate;
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

        moddedZPos = _gameEngine.StaticVariables.PlayerEntity.ModdedPosZ;
        lockTimer = _gameEngine.StaticVariables.g_warpLockTimer;

        if (_gameEngine.StaticVariables.g_debugState != 0xFFFFFFFF 
            || (_gameEngine.StaticVariables.g_debugFlags & 0x80000000) == 0)
        {
            flag = 0x40;

            if ((_gameEngine.StaticVariables.PlayerEntity.Flags & 8U) != 0)
            {
                flag = 0x41;
            }

            if ((_gameEngine.StaticVariables.PlayerEntity.Flags & 1U) != 0)
            {
                flag |= 0x1000;
            }

            index = 0;
            gravityFlag = _gameEngine.StaticVariables.g_gravityFlag < 2;
            colFlags = collisionFlags;
            player = _gameEngine.StaticVariables.PlayerEntity;

            for (int i = 0; i < 4; i++)
            {
                //TODO : Flags or walkability ?
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

        if ((entity.Flags & 0x1) != 0) // 0x1 = hole
        {
            flag |= 0x1000;
        }

        moddedZPos = entity.ModdedPosZ;

        for (int i = 0; i < 4; i++)
        {
            flags[i] = 0;
            var tile = entity.MapTiles[i];
            var tileFlag = (uint)tile.Walkability;

            if ((tileFlag & flag) != 0 || entity.MapHeights[i] >= moddedZPos) // la case est plus basse
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

        if ((entity != _gameEngine.StaticVariables.PlayerEntity 
             || _gameEngine.StaticVariables.g_debugState != 0xFFFFFFFF 
             || (_gameEngine.StaticVariables.g_debugFlags & 0x80000000) == 0)
            && (entity.Flags & 0x80U) != 0
            && (entity.AnimFlags & 0x80U) == 0
            && entity.PlatformEntity == null)
        {
            if (_gameEngine.StaticVariables.g_collideableEntitiesCount <= 0)
            {
                return null;
            }

            collideableEntities = _gameEngine.StaticVariables.g_collideableEntities;

            for (int i = 0; i < _gameEngine.StaticVariables.g_collideableEntitiesCount; i++)
            {
                currentEntity = collideableEntities[i];

                if (entity == currentEntity)
                {
                    continue;
                }

                value = currentEntity.ModdedPosX - entity.ModdedPosX;
                if (value < 0)
                {
                    if (entity.ModdedPosX - currentEntity.ModdedPosX < currentEntity.Width + 1)
                    {
                        value = currentEntity.ModdedPosY - entity.ModdedPosY;
                        if (value < 0)
                        {
                            if (entity.ModdedPosY - currentEntity.ModdedPosY < currentEntity.Height + 1)
                            {
                                value = currentEntity.ModdedPosZ - entity.ModdedPosZ;
                                if (value < 0)
                                {
                                    if (entity.ModdedPosZ - currentEntity.ModdedPosZ < currentEntity.Depth + 1)
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
                            value = currentEntity.ModdedPosZ - entity.ModdedPosZ;
                            if (value < 0)
                            {
                                if (entity.ModdedPosZ - currentEntity.ModdedPosZ < currentEntity.Depth + 1)
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
                    value = currentEntity.ModdedPosY - entity.ModdedPosY;
                    if (value < 0)
                    {
                        if (entity.ModdedPosY - currentEntity.ModdedPosY < currentEntity.Height + 1)
                        {
                            value = currentEntity.ModdedPosZ - entity.ModdedPosZ;
                            if (value < 0)
                            {
                                if (entity.ModdedPosZ - currentEntity.ModdedPosZ < currentEntity.Depth + 1)
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
                        value = currentEntity.ModdedPosZ - entity.ModdedPosZ;
                        if (value < 0)
                        {
                            if (entity.ModdedPosZ - currentEntity.ModdedPosZ < currentEntity.Depth + 1)
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

    // 800364c8
    private void CheckRidingEntities()
    {
        for (var i = 0; i < _gameEngine.StaticVariables.g_collideableEntitiesCount; i++)
        {
            var entity = _gameEngine.StaticVariables.g_collideableEntities[i];
            if ((entity.Flags & 0x4100) != 0x0100)
            {
                continue;
            }

            int entityModdedXPos = entity.ModdedPosX;
            int entityModdedYPos = entity.ModdedPosY;
            int entityModdedZPos = entity.ModdedPosZ;

            int entityWidth = entity.Width + 1;
            int entityHeight = entity.Height + 1;
            int entityDepth = entity.Depth + 1;

            int entityMaxY = entity.ModdedPosY;
            int entityMaxYExtra = entity.Depth; // 0x1f8 is depth, attention: voir le code binaire, ici c'est additionné pour obtenir maxY
            int entityMaxYWithExtra = entityMaxY + entityMaxYExtra;

            entity.RidingEntity = null;

            for (var j = 0; j < _gameEngine.StaticVariables.g_collideableEntitiesCount; j++)
            {
                if (i == j)
                {
                    continue;
                }

                var other = _gameEngine.StaticVariables.g_collideableEntities[j];

                int otherMaxY = other.ModdedPosY + other.Depth + 1;
                if (otherMaxY != entityMaxY)
                    continue;

                // Y overlap
                int yDiff = other.ModdedPosX - entityModdedXPos;
                if (yDiff < 0)
                {
                    int val = other.Width + 1;
                    if (!(entityModdedXPos - other.ModdedPosX < val))
                        continue;
                }
                else
                {
                    if (!(yDiff < entityWidth))
                        continue;
                }

                // Z overlap
                int zDiff = other.ModdedPosZ - entityModdedZPos;
                if (zDiff < 0)
                {
                    int val = other.Height + 1;
                    if (!(entityModdedZPos - other.ModdedPosZ < val))
                        continue;
                }
                else
                {
                    if (!(zDiff < entityHeight))
                        continue;
                }

                entity.RidingEntity = other;
                break;
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

        for (int i = 0; i < _gameEngine.StaticVariables.g_activeEntityCount; i++)
        {
            var entity = _gameEngine.StaticVariables.g_activeEntities[i];

            if (entity == _gameEngine.StaticVariables.PlayerEntity)
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

                        zForceMax = _gameEngine.CurrentMap.Info.ZViscosity * 0x100;
                        entity.ForceZ = spriteZForceTemp;
                        if (zForceMax < spriteZForceAbs && spriteZForceTemp < 1)
                        {
                            entity.ForceZ = _gameEngine.CurrentMap.Info.ZViscosity * -0x100;
                        }
                    }
                }
                else if ((entity.Flags & 0x100U) == 0
                         || (entity.CombinedVramFlagsOR & 0x10U) == 0
                         || 0 < _gameEngine.StaticVariables.g_gravityFlag)
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

                targetXForce = (uint)entity.TargetForceX;
                targetYForce = (uint)entity.TargetForceY;

                if ((entity.CombinedVramFlagsOR & 8U) != 0
                    && _gameEngine.StaticVariables.g_gravityFlag < 1)
                {
                    targetXForce =
                        (uint)(((long)entity.TargetForceX * 0x8000) >> 0x10) |
                        (uint)(((long)entity.TargetForceX * 0x8000) >> 0x20) << 0x10;

                    targetYForce =
                        (uint)(((long)entity.TargetForceY * 0x8000) >> 0x10) |
                        (uint)(((long)entity.TargetForceY * 0x8000) >> 0x20) << 0x10;
                }

                entity.ForceX = IncrementForce(entity.ForceX, (int)targetXForce, (int)xForce);
                entity.ForceY = IncrementForce(entity.ForceY, (int)targetYForce, (int)yForce);

                //LABEL_ProcessFinalForces:
                ApplyEntityForces(entity);
                entity.FinalForceX = entity.AdjustedForceX;
                entity.FinalForceY = entity.AdjustedForceY;
                entity.FinalForceZ = entity.ForceZ;
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

                            var terminal = _gameEngine.CurrentMap.Info.ZViscosity << 8;
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

                    entity.ForceX = IncrementForce(entity.ForceX, entity.TargetForceX, entity.ForceStepX);
                    entity.ForceY = IncrementForce(entity.ForceY, entity.TargetForceY, entity.ForceStepY);

                    //goto LABEL_ProcessFinalForces;
                    ApplyEntityForces(entity);
                    entity.FinalForceX = entity.AdjustedForceX;
                    entity.FinalForceY = entity.AdjustedForceY;
                    entity.FinalForceZ = entity.ForceZ;
                    continue;
                }

                entity.ForceZ = 0;
                entity.ForceY = 0;
                entity.ForceX = 0;
                entity.AdjustedForceY = 0;
                entity.AdjustedForceX = 0;
                entity.FinalForceZ = 0;
                entity.FinalForceY = 0;
                entity.FinalForceX = 0;
            }
        }
    }

    // 800366fc
    private void ApplyEntityForces(Entity entity)
    {
        var adjustedXForce = entity.PreviousAdjustedForceX;
        var adjustedYForce = entity.PreviousAdjustedForceY;
        var shiftAmount = _gameEngine.CurrentMap.Info.Gravity & 0x1f;
        var xForceComponent = entity.ForceX + ScriptHelper.XForceTable[entity.TileAttributes & 0xf] >> shiftAmount;
        var yForceComponent = entity.ForceY + ScriptHelper.YForceTable[entity.TileAttributes & 0xf] >> shiftAmount;
        xForceComponent += adjustedXForce;
        yForceComponent += adjustedYForce;

        entity.PreviousAdjustedForceY = 0;
        entity.PreviousAdjustedForceX = 0;

        if (entity.PosX + xForceComponent < entity.NegModX || entity.ScreenClipX < entity.PosX + xForceComponent)
        {
            xForceComponent = entity.ScreenClipX - entity.PosX;
            entity.ForceAdjusted = 1;
        }

        if (entity.PosY + yForceComponent < entity.NegModY || entity.ScreenClipY < entity.PosY + yForceComponent)
        {
            yForceComponent = entity.ScreenClipY - entity.PosY;
            entity.ForceAdjusted = 1;
        }

        entity.AdjustedForceX = (int)xForceComponent;
        entity.AdjustedForceY = (int)yForceComponent;
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
        if (entity.Speed == entity.AnimationSet.Speed
            && entity.TargetDirection == entity.CurrentDirection
            && entity.Acceleration == (entity.AnimationSet.Acceleration & 0xf))//acceleration ?
        {
            return;
        }

        entity.CurrentDirection = entity.TargetDirection;

        entity.Speed = entity.AnimationSet.Speed;
        entity.Acceleration = entity.AnimationSet.Acceleration & 0xf; // acceleration ?

        entity.TargetForceX = _gameEngine.StaticVariables.g_offsetXList[entity.TargetDirection] * entity.AnimationSet.Speed;
        entity.TargetForceY = _gameEngine.StaticVariables.g_offsetYList[entity.TargetDirection] * entity.AnimationSet.Speed;

        entity.ForceStepX = Math.Abs(entity.TargetForceX - entity.ForceX) >> entity.Acceleration;
        entity.ForceStepY = Math.Abs(entity.TargetForceY - entity.ForceY) >> entity.Acceleration;
    }

    // 800399b8
    private void UpdateVisibleEntitiesZSort()
    {
        if (_gameEngine.StaticVariables.g_visibleEntityCount <= 0)
        {
            return;
        }

        for (var i = 0; i < _gameEngine.StaticVariables.g_visibleEntityCount; i++)
        {
            var entity = _gameEngine.StaticVariables.g_visibleEntities[i];
            entity.ZSortValue = 0;
            entity.ZSortDepth = entity.ModdedPosZ + entity.Depth;
        }

        for (var i = 0; i < _gameEngine.StaticVariables.g_visibleEntityCount; i++)
        {
            var entity = _gameEngine.StaticVariables.g_visibleEntities[i];
            if (entity.ZSortValue == 0)
            {
                ComputeZSortValue(entity);
            }
        }

        for (var i = 0; i < _gameEngine.StaticVariables.g_visibleEntityCount; i++)
        {
            var entity = _gameEngine.StaticVariables.g_visibleEntities[i];
            entity.ZSortValue = (int)(entity.ZSortValue & 0xffff0000) + ((entity.PosZ >> 16) & 0xFFFF);
        }
    }

    // 800397ac
    private int ComputeZSortValue(Entity entity)
    {
        if (entity.ZSortValue != 0)
        {
            return entity.ZSortValue;
        }

        var sortValue = entity.PosY + (entity.Frame.Images.DepthSortValue << 16);

        if ((entity.Flags & 0x80) != 0
            || (entity.AnimFlags & 0x80) != 0)
        {
            entity.ZSortValue = sortValue;
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

        for (var dex = 0; dex < _gameEngine.StaticVariables.g_collideableEntitiesCount; dex++)
        {
            var otherEntity = _gameEngine.StaticVariables.g_collideableEntities[dex];
            if (otherEntity == entity)
            {
                continue;
            }

            if (otherEntity.ZSortDepth >= entity.ZSortDepth)
            {
                continue;
            }

            //X
            var x = otherEntity.PosX + otherEntity.ModX - entity.ModdedPosX;
            if (x >= 0)
            {
                if (x >= entity.Width + 1)
                {
                    continue;
                }
            }
            else
            {
                if (entity.ModdedPosX - (otherEntity.PosX + otherEntity.ModX) >= otherEntity.Width + 1)
                {
                    continue;
                }
            }

            //Y
            var y = otherEntity.PosY + otherEntity.ModY - entity.ModdedPosY;
            if (y >= 0)
            {
                if (y >= entity.Depth + 1)
                {
                    continue;
                }
            }
            else
            {
                if (entity.ModdedPosY - (otherEntity.PosY + otherEntity.ModY) >= otherEntity.Depth + 1)
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

    //80039300
    private void UpdateBalanceRecords()
    {
        for (var i = 0; i < _gameEngine.StaticVariables.g_activeEntityCount; i++)
        {
            var entity = _gameEngine.StaticVariables.g_activeEntities[i];

            if (entity.FrameCollision == null)
            {
                continue;
            }

            if (entity.BalanceAnimValRef == null)
            {
                continue;
            }

            if (entity.BalanceAnimValRef.Val == 0)
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

            for (var j = 0; j < _gameEngine.StaticVariables.g_activeEntityCount; j++)
            {
                var otherEntity = _gameEngine.StaticVariables.g_activeEntities[j];

                if (otherEntity == entity)
                {
                    continue;
                }

                if (otherEntity.FrameCollisionTickCounter != 0)
                {
                    continue;
                }

                if (otherEntity.DamagedTickCounter != 0)
                {
                    continue;
                }

                if ((otherEntity.AnimFlags & 0x40) != 0)
                {
                    continue;
                }

                if ((otherEntity.Flags & flags) == 0)
                {
                    continue;
                }

                //X
                var difx = entity.HitBoxX - otherEntity.ModdedPosX;
                int width;
                if (difx > 0)
                {
                    width = otherEntity.Width + 1;
                }
                else
                {
                    difx = otherEntity.ModdedPosX - entity.HitBoxX;
                    width = entity.CollisionWidth + 1;
                }

                if (difx >= width)
                {
                    continue;
                }

                //Y
                var dify = entity.HitBoxY - otherEntity.ModdedPosY;
                int depth;
                if (dify > 0)
                {
                    depth = otherEntity.Depth + 1;
                }
                else
                {
                    dify = otherEntity.ModdedPosY - entity.HitBoxY;
                    depth = entity.CollisionDepth + 1;
                }

                if (dify >= depth)
                {
                    continue;
                }

                //Z
                var difz = entity.HitBoxZ - otherEntity.ModdedPosZ;
                int height;
                if (difz > 0)
                {
                    height = otherEntity.Height + 1;
                }
                else
                {
                    difz = otherEntity.ModdedPosZ - entity.HitBoxZ;
                    height = entity.CollisionHeight + 1;
                }

                if (difz >= height)
                {
                    continue;
                }

                //Debugger.Break();

                var balanceValueIndex = entity.BalanceAnimValRef.Val & 0xf;
                var val = otherEntity.BalanceRecord.Values[balanceValueIndex];

                //if (_gameEngine.StaticVariables.g_debugState < 0
                //    && (_gameEngine.StaticVariables.g_debugFlags & 0x800) != 0)
                {
                    //_gameEngine.StaticVariables.g_messageDebug += // + otherEntity->index * 0x100
                    var log = string.Format("{0} (Race) {1} -> {2} (Attr) {3} = {4}",
                            EntityNames.GetName(entity.SpriteTableIndex),
                            EntityNames.GetName(otherEntity.SpriteTableIndex),
                            _gameEngine.StaticVariables.g_weaponNames[balanceValueIndex],
                            _gameEngine.StaticVariables.g_damageNames[otherEntity.BalanceRecord.Values[balanceValueIndex] >> 6],
                            val);

                    _gameEngine.LogManager.Log(log);
                }

                if ((val & 0xc0) != 0x80)
                {
                    if (balanceValueIndex == 6 || balanceValueIndex == 0xa)
                    {
                        _gameEngine.EffectManager.CreateAttachedEffect(0, 4, 0, otherEntity, width, 0, 0, 0);
                    }

                    if (balanceValueIndex == 7 || balanceValueIndex == 9)
                    {
                        _gameEngine.EffectManager.CreateAttachedEffect(0, 5, 0, otherEntity, 1, 0, 0, 0);
                    }

                    otherEntity.TouchingEntity = entity;
                }

                otherEntity.FrameCollisionTickCounter = 0x19;
                entity.HitCounter++;

                //X
                var xr = otherEntity.ModdedPosX + otherEntity.Width;
                if (entity.HitBoxX + entity.CollisionWidth < xr)
                {
                    xr = entity.HitBoxX + entity.CollisionWidth;
                }

                var xl = entity.HitBoxX;
                if (entity.HitBoxX < otherEntity.ModdedPosX)
                {
                    xl = otherEntity.ModdedPosX;
                }

                //Y
                var yr = otherEntity.ModdedPosY + otherEntity.Depth;
                if (entity.HitBoxY + entity.CollisionDepth < yr)
                {
                    yr = entity.HitBoxY + entity.CollisionDepth;
                }

                var yl = entity.HitBoxY;
                if (entity.HitBoxY < otherEntity.ModdedPosY)
                {
                    yl = otherEntity.ModdedPosY;
                }

                //Z
                var zr = otherEntity.ModdedPosZ + otherEntity.Height;
                if (entity.HitBoxZ + entity.CollisionHeight < zr)
                {
                    zr = entity.HitBoxZ + entity.CollisionHeight;
                }

                var zl = entity.HitBoxZ;
                if (entity.HitBoxZ < otherEntity.ModdedPosZ)
                {
                    zl = otherEntity.ModdedPosZ;
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
            var effect = _gameEngine.EffectManager.CreateEffectEntity((byte)0, 9, 0, x, y, z);
            if (effect == null)
            {
                continue;
            }

            var baseForce = 0xffff << 16;

            var targetVal = (int)((Random.Next() * 0x20001) >> 32);
            effect.ForceX = targetVal + baseForce;

            targetVal = (int)((Random.Next() * 0x20001) >> 32);
            effect.ForceY = targetVal + baseForce;

            targetVal = (int)((Random.Next() * 0x20001) >> 32);
            effect.ForceZ = targetVal + baseForce;
        }
    }

    // 80038e84
    private void UpdateActiveEffects()
    {
        for (var i = 0; i < _gameEngine.StaticVariables.g_numberOfEntities; i++)
        {
            var entity = _gameEngine.StaticVariables.g_entitySlots[i];
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
                effect = _gameEngine.EffectManager.CreateDetachedEffect(
                    0, 0, 0,
                    entity, -1,
                    0, 0, 0);

                if (effect == null)
                {
                    continue;
                }

                entity.ActiveEffect = effect;
            }

            if ((entity.Flags & 0x7) == 0
                || (entity.AnimFlags & 0x10) != 0
                || entity.PlatformEntity != null)
            {
                effect.Status = 1;
                continue;
            }

            if ((entity.Slope_18c == 4 || entity.Slope_190 == 4)
                && entity.Slope_18c != entity.Slope_190)
            {
                //sliding effect
                var slidingEffect = _gameEngine.EffectManager.CreateEffectEntity(
                    (byte)0, 6, 0,
                    entity.PosX, entity.PosY, entity.FloorHeight);
            }

            if (entity.Slope_18c >= 8)
            {
                continue;
            }

            var spriteTableIndex = (byte)0;

            switch (entity.Slope_18c & 0x7)
            {
                case 1:
                case 2:
                    effect.TargetIsMapSprite = 0;
                    spriteTableIndex = 1;
                    break;

                case 3:
                    if ((entity.FrameCounter & 0x7) != 0)
                    {
                        break;
                    }

                    if ((entity.ForceX | entity.ForceY) == 0)
                    {
                        break;
                    }

                    _gameEngine.EffectManager.CreateEffectEntity(
                        (byte)0, _gameEngine.CurrentMap.Info.SlideEffectId, 0, //_gameEngine.CurrentMap.Info.C
                        entity.PosX, entity.PosY, entity.FloorHeight);
                    break;

                case 4:
                    effect.Status = 1;
                    if ((entity.FrameCounter & 7) != 0)
                    {
                        break;
                    }

                    if ((entity.ForceX | entity.ForceY) == 0)
                    {
                        break;
                    }

                    _gameEngine.EffectManager.CreateEffectEntity(
                        (byte)0, 0x15, 0,
                        entity.PosX, entity.PosY, entity.FloorHeight);
                    break;
            }

            var animId = 5 - (entity.ModdedPosZ - entity.FloorHeight) >> 20;

            if (animId >= 6)
            {
                animId = 5;
            }
            else if (animId < 0)
            {
                animId = 0;
            }

            effect.TargetIsMapSprite = 0;
            effect.TargetSpriteTableIndex = spriteTableIndex;
            effect.TargetAnimation = (byte)animId;
            effect.Status = 2;
            effect.X = entity.PosX;
            effect.Y = entity.PosY;
            effect.Z = entity.FloorHeight;
        }
    }

    // 80038e18
    private void UpdateEntitiesAnimation()
    {
        for (var i = 0; i < _gameEngine.StaticVariables.g_activeEntityCount; i++)
        {
            var entity = _gameEngine.StaticVariables.g_activeEntities[i];
            UpdateAnimation(entity);
            Debug.Assert(entity.Frame != null);
        }
    }

    //800386d0
    private void UpdateEntitiesEvents()
    {
        _gameEngine.PlayerManager.MovePlayer();

        for (var i = 1; i < _gameEngine.StaticVariables.g_numberOfEntities; i++)
        {
            var entity = _gameEngine.StaticVariables.g_entitySlots[i];
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
                                if (
                                    ((flags & 0x10) != 0 && (entity.ForceAdjusted != 0 || entity.IsAboveGround != 0))
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

                                    if (_gameEngine.StaticVariables.g_activeCollisionEntity == entity)
                                    {
                                        if (entity.ProgramIndexes[5] != 0 || entity.SpriteProgramIndexes[5] != 0)
                                        {
                                            eventProgramType = ScriptHelper.ProgramFInteract;
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

            for (var i = 1; i < _gameEngine.StaticVariables.g_numberOfEntities; i++)
            {
                var entity = _gameEngine.StaticVariables.g_entitySlots[i];

                if (entity.EventTrigger == ScriptHelper.ProgramUnknown)
                {
                    continue;
                }

                var programIndex = entity.ProgramIndexes[entity.EventTrigger] & 0x7f;

                _gameEngine.LogManager.SetCategory($"entity[{entity}]");

                if (programIndex == 0)
                {
                    // g_entityEventFunctionsByType => AI
                    _gameEngine.RunSpriteEvent(entity);
                }
                else
                {
                    _gameEngine.RunScript(entity, entity.EventTrigger);
                }

                _gameEngine.LogManager.ResetCategory();

                entity.EventTrigger = -1;
                keepGoing = true;
            }
        } while (keepGoing);
    }

    // 80038998
    private void UpdateEntitiesCounters()
    {
        for (var i = 0; i <= _gameEngine.StaticVariables.g_numberOfEntities; i++)
        {
            var entity = _gameEngine.StaticVariables.g_entitySlots[i];

            entity.TouchingEntity = null;
            entity.RidingEntity = null;
            entity.HitCounter = 0;

            entity.FrameCounter++;

            if (entity.DamagedTickCounter != 0)
            {
                entity.DamagedTickCounter--;
            }

            if (entity.FrameCollisionTickCounter != 0)
            {
                entity.FrameCollisionTickCounter--;
            }
        }

        //displays debug records here
    }


    // 80038634
    private void UpdateDestroyedEntities()
    {
        var max = 0; // always player
        for (var i = 0; i < _gameEngine.StaticVariables.g_entitySlots.Length; i++)
        {
            var entity = _gameEngine.StaticVariables.g_entitySlots[i];

            if (entity.Status == 4)
            {
                entity.Clear();
                entity.Index = i;
            }
            else if (entity.Status != 0)
            {
                max = i;
            }
        }

        _gameEngine.StaticVariables.g_numberOfEntities = max + 1;
    }

    // 800384f4
    private void UpdateEntityLists()
    {
        _gameEngine.StaticVariables.g_activeEntityCount = 0;
        _gameEngine.StaticVariables.g_collideableEntitiesCount = 0;
        _gameEngine.StaticVariables.g_visibleEntityCount = 0;

        Array.Clear(_gameEngine.StaticVariables.g_activeEntities);
        Array.Clear(_gameEngine.StaticVariables.g_collideableEntities);
        Array.Clear(_gameEngine.StaticVariables.g_visibleEntities);

        for (int i = 0; i < _gameEngine.StaticVariables.g_numberOfEntities; i++)
        {
            var entity = _gameEngine.StaticVariables.g_entitySlots[i];

            //processable
            if (entity.Status >= 2 && entity.Status <= 3 && entity.IsNotProcessable == 0)
            {
                _gameEngine.StaticVariables.g_activeEntities[_gameEngine.StaticVariables.g_activeEntityCount++] = entity;
            }

            //collidable
            if ((entity.Flags & 0x80) != 0 && (entity.AnimFlags & 0x80) == 0 && entity.IsNotProcessable == 0)
            {
                _gameEngine.StaticVariables.g_collideableEntities[_gameEngine.StaticVariables.g_collideableEntitiesCount++] = entity;
            }

            //renderable
            if (entity.Status >= 2 
                && entity.Status <= 3
                //flicker effect, every 3rd frame when being damaged
                && (entity.DamagedTickCounter & 0x3) != 0x3) 
            {
                _gameEngine.StaticVariables.g_visibleEntities[_gameEngine.StaticVariables.g_visibleEntityCount++] = entity;
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
            entity.HitBoxX = entity.PosX + entity.CollisionOffsetX;
            entity.HitBoxY = entity.PosY + entity.CollisionOffsetY;
            entity.HitBoxZ = entity.PosZ + entity.CollisionOffsetZ;
        }

        //var x = entity.PosX >> 16;
        //tileX = Math.Min((x / StaticVariables.MapTileWidth), 51);
        ////On PSX hardware we avoid using division, so we use a lookup table instead.
        //x = Math.Clamp(x, 0, _gameEngine.StaticVariables.g_tileToWorldXTable.Length - 1);
        entity.TileX = (entity.PosX >> 16) / StaticVariables.MapTileWidth;
        entity.TileY = (entity.PosY >> 16) / StaticVariables.MapTileHeight;
        entity.TileZ = entity.PosZ >> 20;


        var hitz = _gameEngine.GetCollisionOnZ(entity);
        entity.FloorHeight = hitz;
        entity.IsAboveGround = hitz < entity.PosZ ? 0 : 1;

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

            var mapTile = _gameEngine.CurrentMap.Map.MapTiles[tileX + tileY * _gameEngine.CurrentMap.Map.Width];
            tileFlags = mapTile.Flags;
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
                tileFlags = entity.MapTiles[i].Flags;

                if (entity.MapHeights[i] + 1 == entity.ModdedPosZ)
                {
                    tempFlags[i] = entity.MapTiles[i].Flags;

                    if ((tileFlags & 0xe00) < bestFlagMask)
                    {
                        bestFlagMask = tileFlags & 0xe00;
                    }
                }
                else
                {
                    tempFlags[i] = 0;
                    bestFlagMask = 0;
                }

                i += 1;
            } while (i < 4);

            entity.CombinedVramFlagsOR = tempFlags[0] | tempFlags[1] | tempFlags[2] | tempFlags[3];
            entity.CombinedVramFlagsAND = tempFlags[0] & tempFlags[1] & tempFlags[2] & tempFlags[3];
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

            var tile = _gameEngine.CurrentMap.Map.MapTiles[tileX + tileY * _gameEngine.CurrentMap.Map.Width];
            tileFlags = (uint)(tile.Walkability | tile.GroundProperty << 8 | tile.Slope << 16 | tile.Height << 24);

            if ((tileFlags & 0xc00000) == 0)
            {
                goto NoCollision;
            }

            tileAttr = 0x40000;
            if (((tileFlags & 0xff000000) >> 4) + 1 != entity.ModdedPosZ)
            {
                tileAttr = 0x80000;
            }

            if ((tileFlags & tileAttr) == 0)
            {
                goto NoCollision;
            }
        }

        tileAttr = 0x1U << ((int)(tileFlags >> 0x14) & 0x3);
        entity.TileAttributes = (int)tileAttr;
        if ((tileFlags & 0x800000) != 0)
        {
            entity.TileAttributes = (int)(tileAttr | 0x80);
        }

        FinishUpdate:
        entity.Slope_190 = entity.Slope_18c;
        entity.Slope_18c = (int)bestFlagMask >> 9;
        return;

        NoCollision:
        entity.TileAttributes = 0;
        goto FinishUpdate;
    }

    // 8003a374
    public bool ComputeNewHp(Entity entity)
    {
        int damage;
        int strLength;
        BalanceRecordData balanceRecord;
        string debugStr = string.Empty;

        if (entity == _gameEngine.StaticVariables.PlayerEntity)
        {
            balanceRecord = _gameEngine.StaticVariables.g_balanceRecord[0];
        }
        else
        {
            balanceRecord = new BalanceRecordData();
            balanceRecord.CopyFrom(entity.BalanceRecord);
        }

        damage = ResolveBalanceTarget(entity.TouchingEntity.BalanceAnimValRef, balanceRecord, entity.Hp);

        //if (_gameEngine.StaticVariables.g_debugState < 0 && (_gameEngine.StaticVariables.g_debugFlags & 0x800) != 0)
        {
            if (_gameEngine.StaticVariables.g_balanceHpTotal != -1)
            {
                if ((_gameEngine.StaticVariables.g_balanceMultiplier & 0xc0U) == 0)
                {
                    if ((_gameEngine.StaticVariables.g_balanceHpTotal & 0x80U) == 0)
                    {
                        debugStr += $"{_gameEngine.StaticVariables.g_balanceHp}";
                    }
                    else
                    {
                        debugStr += $" O{_gameEngine.StaticVariables.g_balanceParams} + A{_gameEngine.StaticVariables.g_balanceHp - _gameEngine.StaticVariables.g_balanceParams} = T{_gameEngine.StaticVariables.g_balanceParams}";
                    }

                    debugStr += $" (Parm) {_gameEngine.StaticVariables.g_balanceMultiplier}";

                    _gameEngine.LogManager.Log(entity, debugStr);
                    debugStr = string.Empty;
                }

                if (damage == null)
                {
                    debugStr += $"{_gameEngine.StaticVariables.g_balanceResult} Damage(Result)HP M{entity.HpMax} C{entity.Hp} DEAD";
                    entity.Hp = 0;
                }
                else
                {
                    debugStr += $"{_gameEngine.StaticVariables.g_balanceResult} Damage(Result)HP M{entity.HpMax} C{entity.Hp} N{damage}";
                    entity.Hp = damage;
                }

                _gameEngine.LogManager.Log(entity, debugStr);

                goto END;
            }

            debugStr += "Balance patamator error(Result)No Damage";
            _gameEngine.StaticVariables.g_messageDebug += debugStr;
            _gameEngine.LogManager.Log(entity, debugStr);
        }

        entity.Hp = damage;

        END:
        //DisplayHpDebugString();
        //DisplayHpDebugString();
        //DoNothing();
        return damage == null;
    }

    // 8004464c
    private int ResolveBalanceTarget(BalanceAnimValRef balanceConfig, BalanceRecordData balanceRecordData, int hp)
    {
        BalanceAnimValRef values;
        ItemBalanceRecord[] balanceSources;
        BalanceRecord balanceRecord;
        int i;
        int adjustedHpValue;
        int newHp;
        byte balanceId;
        byte balanceMultiplier;
        bool isReduced;

        if (_gameEngine.StaticVariables.g_debugState < 0 && (_gameEngine.StaticVariables.g_debugFlags & 0x800) != 0)
        {
            _gameEngine.StaticVariables.g_balanceHpTotal = -1;
        }

        if (balanceRecordData != null && balanceConfig != null)
        {
            balanceId = balanceConfig.Val;

            if (balanceId != 0)
            {
                balanceMultiplier = balanceRecordData.Values[(balanceId & 0xf) - 1];

                if ((balanceMultiplier & 0xc0) == 0)
                {
                    adjustedHpValue = balanceConfig.U2;

                    if ((balanceId & 0x80) != 0)
                    {
                        i = 0;
                        balanceSources = _gameEngine.StaticVariables.g_itemBalanceRecords;

                        do
                        {
                            balanceRecord = balanceSources[i].BalanceRecord;

                            if (balanceRecord != null)
                            {
                                if (balanceRecord.NumAnimVals == 0)
                                {
                                    values = null;
                                }
                                else if (_gameEngine.StaticVariables.g_balanceAnimIndex + 1 < balanceRecord.NumAnimVals)
                                {
                                    values = balanceRecord.AnimVals[(_gameEngine.StaticVariables.g_balanceAnimIndex << 1) + 0xf + 2];
                                }
                                else
                                {
                                    values = balanceRecord.AnimVals[0];
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

                    if (_gameEngine.StaticVariables.g_debugState < 0 && (_gameEngine.StaticVariables.g_debugFlags & 0x800) != 0)
                    {
                        _gameEngine.StaticVariables.g_balanceHp = (short)adjustedHpValue;
                        _gameEngine.StaticVariables.g_balanceParams = balanceConfig.U2;
                        _gameEngine.StaticVariables.g_balanceResult = (short)i;
                        _gameEngine.StaticVariables.g_balanceHpTotal = balanceConfig.Val;
                        _gameEngine.StaticVariables.g_balanceMultiplier = balanceMultiplier;
                    }
                }
                else if ((balanceMultiplier & 0xc0) == 0x40)
                {
                    if (_gameEngine.StaticVariables.g_debugState < 0 && (_gameEngine.StaticVariables.g_debugFlags & 0x800) != 0)
                    {
                        _gameEngine.StaticVariables.g_balanceHp = 0;
                        _gameEngine.StaticVariables.g_balanceParams = 0;
                        _gameEngine.StaticVariables.g_balanceResult = (short)hp;
                        _gameEngine.StaticVariables.g_balanceHpTotal = balanceConfig.Val;
                        _gameEngine.StaticVariables.g_balanceMultiplier = balanceMultiplier;
                    }

                    hp = 0;
                }
            }
        }

        return hp;
    }

    //8003ad30
    public int FUN_8003ad30(Entity entity)
    {
        Entity entity2;
        Entity currentEntity;
        int i;
        int result;
        int maxEntity;

        maxEntity = _gameEngine.StaticVariables.g_numberOfEntities;
        result = 0;
        i = 0;
        var j = 0;

        if (-1 < _gameEngine.StaticVariables.g_numberOfEntities)
        {
            currentEntity = _gameEngine.StaticVariables.PlayerEntity;
            entity2 = _gameEngine.StaticVariables.PlayerEntity;

            do
            {
                if (entity2 != entity 
                    && currentEntity.Status - 2 < 2 
                    && currentEntity.IsNotProcessable == 0)
                {
                    currentEntity = entity;
                    result = result + 1;
                    j = entity.Index;
                }

                i = i + 1;
                currentEntity = _gameEngine.StaticVariables.g_entitySlots[j];
                entity2 = _gameEngine.StaticVariables.g_entitySlots[i];
            } while (i <= maxEntity);
        }

        return result;
    }

    //8003adac
    public int FUN_8003adac(Entity entity)
    {
        Entity entity2;
        int i;
        int result;
        int maxEntity;

        maxEntity = _gameEngine.StaticVariables.g_numberOfEntities;
        result = 0;
        i = 0;

        if (-1 < _gameEngine.StaticVariables.g_numberOfEntities)
        {
            entity2 = _gameEngine.StaticVariables.g_entitySlots[0];

            do
            {
                if (entity2 == entity)
                {
                    entity2.IsNotProcessable = 0;
                    result = result + 1;
                }

                i = i + 1;
                entity2 = _gameEngine.StaticVariables.g_entitySlots[i];
            } while (i <= maxEntity);
        }

        return result;
    }
}