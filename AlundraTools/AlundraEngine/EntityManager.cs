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
        entity.Name = EntityNames.GetName(spriteTableIndex);

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

        int height = PhysicsEngine.ComputeEntityGroundHeight(entity, _gameEngine);
        entity.TerrainHeight = height;

        if (entity.PosZ <= height + 1)
        {
            entity.PosZ = height + 1;
            entity.ModdedPosX = entity.PosX + entity.ModX;
            entity.ModdedPosY = entity.PosY + entity.ModY;
            entity.ModdedPosZ = entity.PosZ + entity.ModZ;
        }

        PhysicsEngine.UpdateTileAttributes(entity, _gameEngine);
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
        var updateFrameIndex = true;

        entity.IsZForceApplied = 0;

        var row = entity.AnimationDirection;
        var col = ((entity.TargetDirection + 2) & 0x1c) >> 2; // 0..7
        var animationDirectionTableIndex = row * 8 + col; // 0..31
        var animationDirectionFromTargetDirection = _gameEngine.StaticVariables.g_animationDirectionTable[animationDirectionTableIndex];

        if (entity.CurrentAnimationId != entity.TargetAnimationId ||
            entity.AnimationDirection != animationDirectionFromTargetDirection)
        {
            updateFrameIndex = false;

            entity.CurrentAnimationId = entity.TargetAnimationId;
            entity.AnimationDirection = animationDirectionFromTargetDirection;
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
            if (--entity.NextFrameDelay > 0)
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
                    //UpdateAnimation(entity); // recursive call to update the animation
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
        //currentFrame = animSet.PreloadedAnims[entity.TargetDirection >> 3].Frames[entity.AnimationFrameIndex];
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

        if (updateFrameIndex)
        {
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
            PhysicsEngine.UpdateEntitiesPhysics(_gameEngine);
            UpdateActiveEffects();
            UpdateBalanceRecords();
        }
        else
        {
            UpdateEntityLists();
        }

        UpdateVisibleEntitiesZSort(_gameEngine);

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

            if (entity.IsBlockedByEntity == 0)
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
            if (entity.Status >= 2 && entity.Status <= 3 && entity.IsBlockedByEntity == 0)
            {
                _gameEngine.StaticVariables.g_activeEntities[_gameEngine.StaticVariables.g_activeEntityCount++] = entity;
            }

            //collidable
            if ((entity.Flags & 0x80) != 0 && (entity.AnimFlags & 0x80) == 0 && entity.IsBlockedByEntity == 0)
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

    // 800399b8
    public static void UpdateVisibleEntitiesZSort(GameEngine gameEngine)
    {
        if (gameEngine.StaticVariables.g_visibleEntityCount <= 0)
        {
            return;
        }

        for (var i = 0; i < gameEngine.StaticVariables.g_visibleEntityCount; i++)
        {
            var entity = gameEngine.StaticVariables.g_visibleEntities[i];
            entity.ZSortValue = 0;
            entity.ZSortDepth = entity.ModdedPosZ + entity.Depth;
        }

        for (var i = 0; i < gameEngine.StaticVariables.g_visibleEntityCount; i++)
        {
            var entity = gameEngine.StaticVariables.g_visibleEntities[i];
            if (entity.ZSortValue == 0)
            {
                ComputeZSortValue(entity, gameEngine);
            }
        }

        for (var i = 0; i < gameEngine.StaticVariables.g_visibleEntityCount; i++)
        {
            var entity = gameEngine.StaticVariables.g_visibleEntities[i];
            entity.ZSortValue = (int)(entity.ZSortValue & 0xffff0000) + ((entity.PosZ >> 16) & 0xFFFF);
        }
    }


    // 800397ac
    public static int ComputeZSortValue(Entity entity, GameEngine gameEngine)
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
                sortValue = ComputeZSortValue(entity.PlatformEntity, gameEngine);
            }

            if (sortValue < entity.PlatformEntity.ZSortValue)
            {
                entity.ZSortValue = entity.PlatformEntity.ZSortValue;
                return entity.PlatformEntity.ZSortValue;
            }
        }

        for (var dex = 0; dex < gameEngine.StaticVariables.g_collideableEntitiesCount; dex++)
        {
            var otherEntity = gameEngine.StaticVariables.g_collideableEntities[dex];
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
                sortValue = ComputeZSortValue(otherEntity, gameEngine);
            }

            if (sortValue < otherEntity.ZSortValue)
            {
                sortValue = otherEntity.ZSortValue;
            }
        }

        entity.ZSortValue = sortValue;

        return sortValue;
    }

    // 8003a374
    public bool ComputeNewHp(Entity entity)
    {
        int newHp;
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

        newHp = ResolveBalanceTarget(entity.TouchingEntity.BalanceAnimValRef, balanceRecord, entity.Hp);

        //if (_gameEngine.StaticVariables.g_debugState < 0 && (_gameEngine.StaticVariables.g_debugFlags & 0x800) != 0)
        if (_gameEngine.StaticVariables.IsLogDamageEnabled)
        {
            if (_gameEngine.StaticVariables.g_balanceHpTotal != -1)
            {
                if ((_gameEngine.StaticVariables.g_balanceMultiplier & 0xc0U) == 0)
                {
                    if ((_gameEngine.StaticVariables.g_balanceHpTotal & 0x80U) == 0)
                    {
                        debugStr = $"{_gameEngine.StaticVariables.g_balanceHp}";
                    }
                    else
                    {
                        debugStr = $"O{_gameEngine.StaticVariables.g_balanceParams} + A{_gameEngine.StaticVariables.g_balanceHp - _gameEngine.StaticVariables.g_balanceParams} = T{_gameEngine.StaticVariables.g_balanceParams}";
                    }

                    debugStr += $" (Parm) {_gameEngine.StaticVariables.g_balanceMultiplier}";

                    _gameEngine.LogManager.Log(entity, debugStr);
                    debugStr = string.Empty;
                }

                if (newHp == 0)
                {
                    debugStr += $" {_gameEngine.StaticVariables.g_balanceResult} Damage(Result)HP M{entity.HpMax} C{entity.Hp} DEAD";
                    entity.Hp = 0;
                }
                else
                {
                    debugStr += $" {_gameEngine.StaticVariables.g_balanceResult} Damage(Result)HP M{entity.HpMax} C{entity.Hp} N{newHp}";
                    entity.Hp = newHp;
                }

                _gameEngine.LogManager.Log(entity, debugStr);

                goto END;
            }

            debugStr += "Balance patamator error(Result)No Damage";
            _gameEngine.LogManager.Log(entity, debugStr);
            //_gameEngine.StaticVariables.g_messageDebug += debugStr;
        }

        entity.Hp = newHp;

        END:
        //DisplayHpDebugString();
        //DisplayHpDebugString();
        //DoNothing();
        return newHp == 0;
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

        if (_gameEngine.StaticVariables.IsLogDamageEnabled)
        //if (_gameEngine.StaticVariables.g_debugState < 0 && (_gameEngine.StaticVariables.g_debugFlags & 0x800) != 0)
        {
            _gameEngine.StaticVariables.g_balanceHpTotal = -1;
        }

        if (balanceRecordData == null || balanceConfig == null)
        {
            return hp;
        }

        balanceId = balanceConfig.Val;

        if (balanceId == 0)
        {
            return hp;
        }

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

            if (_gameEngine.StaticVariables.IsLogDamageEnabled)
                //if (_gameEngine.StaticVariables.g_debugState < 0 && (_gameEngine.StaticVariables.g_debugFlags & 0x800) != 0)
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

            if (_gameEngine.StaticVariables.IsLogDamageEnabled) 
                //if (_gameEngine.StaticVariables.g_debugState < 0 && (_gameEngine.StaticVariables.g_debugFlags & 0x800) != 0)
            {
                _gameEngine.StaticVariables.g_balanceHp = 0;
                _gameEngine.StaticVariables.g_balanceParams = 0;
                _gameEngine.StaticVariables.g_balanceResult = (short)hp;
                _gameEngine.StaticVariables.g_balanceHpTotal = balanceConfig.Val;
                _gameEngine.StaticVariables.g_balanceMultiplier = balanceMultiplier;
            }

            hp = 0;
        }

        return hp;
    }

    //8003ad30
    public int BlockEntitiesBy(Entity entity)
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
            do
            {
                entity2 = _gameEngine.StaticVariables.g_entitySlots[i];

                if (entity2 != entity 
                    && entity2.Status - 2 < 2 
                    && entity2.IsBlockedByEntity == 0)
                {
                    entity2.IsBlockedByEntity = entity.Index; // pointer of the entity
                    result++;
                    entity2 = _gameEngine.StaticVariables.g_entitySlots[entity.Index];
                }

                i++;
            } while (i <= maxEntity);
        }

        return result;
    }

    //8003adac
    public int UnblockEntitiesBy(Entity entity)
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
                if (entity2.IsBlockedByEntity == entity.Index)
                {
                    entity2.IsBlockedByEntity = 0;
                    result = result + 1;
                }

                i = i + 1;
                entity2 = _gameEngine.StaticVariables.g_entitySlots[i];
            } while (i <= maxEntity);
        }

        return result;
    }
}