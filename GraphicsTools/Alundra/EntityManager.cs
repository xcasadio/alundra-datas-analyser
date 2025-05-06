using System.Diagnostics;
using Alundra.DatasBin;
using Alundra.Gameplay;
using Alundra.Gameplay.Scripts;
using Alundra.Sound;

namespace Alundra;

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

    public Entity AllocateEntitySlot()
    {
        for (int i = 1; i < StaticVariables.g_entitySlots.Length; i++)
        {
            if (StaticVariables.g_entitySlots[i].Status == 0)
            {
                return StaticVariables.g_entitySlots[i];
            }
        }

        //int i = 1;
        //int index = i;
        //Entity entity = StaticVariables.g_entitySlots[index];
        //
        //do
        //{
        //    i = i + 1;
        //    if (entity.Status == 0)
        //    {
        //        return entity;
        //    }
        //    index++;
        //    entity = StaticVariables.g_entitySlots[index];
        //} while (i < 0x40);

        //DoNothing();
        return null;
    }

    public void InitializeEntity(Entity entity, Entity parentEntity, SpriteRecord sprite, SiEntityRecord entityRecord,
        uint spriteTableIndex, int entityId, int x, int y, int z, uint animationId, uint direction, int paletteIndex, int sheetSize)
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
        entity.Flags = (uint)(sprite.Header.MoreFlags | sprite.Header.CanPickup << 8 | sprite.Header.FlagsPortraitShadowtype << 16); ;

        entity.SpriteProgramIndexes[ScriptHelper.ProgramALoad] = sprite.Header.ProgramLoad;
        entity.SpriteProgramIndexes[ScriptHelper.ProgramBMap] = 0;
        entity.SpriteProgramIndexes[ScriptHelper.ProgramCTick] = sprite.Header.ProgramTick;
        entity.SpriteProgramIndexes[ScriptHelper.ProgramDTouch] = sprite.Header.ProgramTouch;
        entity.SpriteProgramIndexes[ScriptHelper.ProgramEDeactivate] = sprite.Header.ProgramDeactivate;
        entity.SpriteProgramIndexes[ScriptHelper.ProgramFInteract] = sprite.Header.ProgramInteract;

        entity.AddedToPalette = paletteIndex;
        entity.AddedToSheet = sheetSize;

        //BalanceRecord balanceRecord = GetSpriteAnimationPtr(entity.SpriteTableIndex);
        BalanceRecord balanceRecord = _gameEngine.BalanceBin.GetBalanceRecordFromSpriteIndex((int)spriteTableIndex, _gameEngine.CurrentMap.Info.BalanceLevel);
        entity.BalanceRecord = balanceRecord;
        byte balanceHp = balanceRecord.Hp;
        entity.HpMax = balanceHp;
        entity.Hp = balanceHp;

        InitializeCodePrograms(entity);

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

        entity.IsZForceApplied = 0;
        var frameDelay = entity.TargetAnimationId;
        var animationFrameIndex = StaticVariables.g_frameIndexTable[(entity.TargetDirection + 2 & 0x1c) + entity.CurrentFrameIndex * 0x20];

        //var directionIndex = ((entity.TargetDirection + 2) & 0x1c) >> 2;
        //frameDelay = StaticVariables.g_frameIndexTable[directionIndex + (entity.CurrentFrameIndex << 3)];

        if (entity.TargetAnimationId != entity.CurrentAnimationId
            || animationFrameIndex != entity.CurrentFrameIndex)
        {
            noSkip = false;
        }

        if (noSkip)
        {
            animationFrameIndex = entity.NextFrameDelay - 1;
            entity.NextFrameDelay = animationFrameIndex;

            if (animationFrameIndex != 0)
            {
                entity.NextFrameDelay = 0x7fffffff;
                entity.ForceResetAnimationFlag = 1;

                //Debug.Assert(entity.AnimSet != null);
                //Debug.Assert(entity.Frame != null);

                //TODO: bug => remove this only to avoid null pointer
                if (entity.AnimSet == null && entity.Sprite != null)
                {
                    var animSet = entity.Sprite.AnimSets[entity.CurrentAnimationId]; //frameDelay * 0xe
                    entity.AnimSet = animSet;
                    entity.Frame = animSet.PreloadedAnims[entity.TargetAnimationId >> 3].Frames[frameDelay];
                }
                return;
            }

            currentFrame = entity.Frame;
            //currentFrame = entity.AnimSet.PreloadedAnims[entity.TargetAnimationId >> 3].Frames[frameDelay];
        }

        while (true)
        {
            while (noSkip)
            {
                frameDelay = currentFrame.Delay;

                if (((uint)currentFrame.Delay & 0x80) != 0)
                {
                    entity.NextFrameDelay = (int)(frameDelay & 0x7f);
                    try
                    {
                        var frame = entity.AnimSet.PreloadedAnims[entity.TargetDirection >> 3].Frames[entity.CurrentFrameIndex + 1];
                        entity.Frame = frame ?? entity.Frame;
                    }
                    catch (Exception e)
                    {
                        Debugger.Break();
                    }

                    //Debug.Assert(entity.Frame != null);

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

                    Debug.Assert(entity.AnimSet != null);
                    Debug.Assert(entity.Frame != null);
                    return;
                }

                if (frameDelay == 0)
                {
                    break;
                }

                if (frameDelay != 1)
                {
                    Debugger.Break();
                    throw new Exception("Character Animation Error!!");
                }

                currentFrame = entity.FirstFrame;
                entity.AnimCompleteCounter++;
                entity.Frame = currentFrame;
            }

            if (noSkip)
            {
                frameDelay = (uint)currentFrame.CollisionOffset;
                //frameDelay = (uint)currentFrame.transformIndexLow;

                if ((currentFrame.CollisionOffset & 0x80) != 0)
                {
                    break;
                }

                //frameDelay = entity.CurrentFrameIndex;
                entity.TargetAnimationId = (uint)frameDelay;
                entity.AnimCompleteCounter++;
            }

            LOAD_ANIMATION:
            Debug.Assert(frameDelay < entity.Sprite.AnimSets.Length);
            var animSet = entity.Sprite.AnimSets[frameDelay]; //frameDelay * 0xe
            entity.AnimSet = animSet;
            Debug.Assert(entity.AnimSet != null);
            //frameOffset = (ushort)((int)animSet.entries + animationFrameIndex * 2);
            //var animTableOffset = entity.AnimSet.AnimationOffsets[entity.CurrentFrameIndex];
            entity.CurrentFrameIndex = animationFrameIndex;
            entity.NextFrameDelay = 0;
            entity.CurrentAnimationId = (uint)frameDelay;

            //currentFrame = entity.AnimSet.PreloadedAnims[entity.TargetAnimationId].Frames[frameIndex];
            //currentFrame = animTableOffset[entity.CurrentFrameIndex];
            //System.Diagnostics.Debug.Assert(animTableOffset < entity.AnimSet.PreloadedAnims.Length);
            //System.Diagnostics.Debug.Assert(entity.CurrentFrameIndex < entity.AnimSet.PreloadedAnims[animTableOffset].Frames.Length);

            //currentFrame = entity.AnimSet.PreloadedAnims[animTableOffset].Frames[entity.CurrentFrameIndex];
            //TargetAnimationId ??
            try
            {
                currentFrame = entity.AnimSet.PreloadedAnims[entity.TargetDirection >> 3].Frames[entity.CurrentFrameIndex];
            }
            catch (Exception e)
            {
                Debugger.Break();
            }

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
                //entity.BalanceVal = entity.BalanceRecord.AnimVals[frameIndex * 2 + 0xe];
                entity.BalanceVal = entity.BalanceRecord.AnimVals[frameDelay + 1];
                //System.Diagnostics.Debugger.Break();
            }
            else
            {
                entity.BalanceVal = entity.BalanceRecord.AnimVals[0]; // 0 ?? TODO: check if index 0
            }

            uint sfxId = entity.AnimSet.Sfx;
            if ((entity.AnimSet.Flags & 0x20) != 0)
                //if (((uint)animSet.pointerListOffset & 0x2000) != 0)
            {
                sfxId += 0x100;
            }

            _gameEngine.PlaySoundEffect(sfxId);

            noSkip = true; //imitate goto LOAD_ANIMATION
        }

        entity.NextFrameDelay = 0x7fffffff;
        entity.ForceResetAnimationFlag = 1;

        Debug.Assert(entity.AnimSet != null);
        Debug.Assert(entity.Frame != null);
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
        for (var i = 0; i < 4; i++)
        {
            var x = xs[i];
            var y = ys[i];
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
            var tile = _gameEngine.CurrentMap.Map.MapTiles[tiley * 52 + tilex];
            entity.MapTiles[i] = tile;
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
                            var my = ys[i];
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
                            var mx = xs[i];
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
                            var mx = xs[i];
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

            entity.MapHeights[i] = height;
            if (highest < height)
            {
                highest = height;
            }
        }
        return highest;
    }

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

        var hitz = _gameEngine.GetCollisionOnZ(entity);
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

            var tile = _gameEngine.CurrentMap.Map.MapTiles[tilex + tiley * 52];
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
                        var force = player.ZForce - (_gameEngine.CurrentMap.Info.Gravity << 8);
                        if (force < 0)
                        {
                            force = -force;//abs
                        }

                        var terminal = _gameEngine.CurrentMap.Info.TerminalVelocity << 8;
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

                UpdateEntityPhysics(player);

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
                    var force = entity.ZForce - (_gameEngine.CurrentMap.Info.Gravity << 8);
                    if (force < 0)
                    {
                        force = -force;//abs
                    }

                    var terminal = _gameEngine.CurrentMap.Info.TerminalVelocity << 8;
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

                UpdateEntityPhysics(entity);

                entity.XForce = IncrementForce(entity.XForce, entity.TargetXForce, entity.XForceStep);
                entity.YForce = IncrementForce(entity.YForce, entity.TargetYForce, entity.YForceStep);
            }

            ApplyEntityForces(entity);

            entity.FinalXForce = entity.AdjustedXForce;
            entity.FinalYForce = entity.AdjustedYForce;
            entity.FinalZForce = entity.ZForce;
        }
    }

    private void ApplyEntityForces(Entity entity)
    {
        var lastinteractx = entity.InteractXForce;
        var lastinteracty = entity.InteractYForce;
        entity.InteractYForce = 0;
        entity.InteractXForce = 0;
        var xval = entity.XForce + ScriptHelper.XForceTable[entity.TileAttributes & 0xf] >> _gameEngine.CurrentMap.Info.Gravity;
        var yval = entity.YForce + ScriptHelper.YForceTable[entity.TileAttributes & 0xf] >> _gameEngine.CurrentMap.Info.Gravity;

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

    private void UpdateEntityPhysics(Entity entity)
    {
        //TODO: bug with animation
        if (entity.AnimSet == null)
        {
            return;
        }

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

        for (var i = 0; i < StaticVariables.g_visibleEntityCount; i++)
        {
            var entity = StaticVariables.g_visibleEntities[i];
            entity.DepthSortVal = 0;
            entity.SortTop = entity.ModdedZPos + entity.Height;
        }

        for (var i = 0; i < StaticVariables.g_visibleEntityCount; i++)
        {
            var entity = StaticVariables.g_visibleEntities[i];
            if (entity.DepthSortVal == 0)
            {
                SetDepthSortVal(entity);
            }
        }

        for (var i = 0; i < StaticVariables.g_visibleEntityCount; i++)
        {
            var entity = StaticVariables.g_visibleEntities[i];
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
                        _gameEngine.EffectManager.CreateEffect_Type1(0, 4, 0, checkme, width, 0, 0, 0);
                    }
                    if (valdex == 7 || valdex == 9)
                    {
                        _gameEngine.EffectManager.CreateEffect_Type1(0, 5, 0, checkme, 1, 0, 0, 0);
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
            var effect = _gameEngine.EffectManager.CreateEffect_Type0(0, 9, 0, x, y, z);
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
            //TODO: what is 18c, something with slope and sliding?
            var animid = -1;
            if ((entity.Slope_18c == 4 || entity.Slope_190 == 4)
                && entity.Slope_18c != entity.Slope_190)
            {
                _gameEngine.EffectManager.CreateEffect_Type0(0, 6, 0, entity.XPos, entity.YPos, entity.CollidedWithEntityZ);
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

                    _gameEngine.EffectManager.CreateEffect_Type0(0, 0x15, 0, entity.XPos, entity.YPos, entity.CollidedWithEntityZ);
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

                    _gameEngine.EffectManager.CreateEffect_Type0(0, _gameEngine.CurrentMap.Info.SlideEffectId, 0, entity.XPos, entity.YPos, entity.CollidedWithEntityZ);
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
        for (var i = 1; i < StaticVariables.g_activeEntityCount; i++)
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

                if (entity.EventTrigger == 0)// && entity.Status < 5)
                {
                    switch (entity.Status)
                    {
                        case 1://loading/activating
                            eventType = ScriptHelper.ProgramALoad;
                            entity.Status = 2;
                            break;
                        case 2://normal
                            var flags = entity.Flags;

                            if ((flags & 0x10) != 0)
                            {
                                if (entity.ActionState == 4)
                                {
                                    _gameEngine.DestroyEntity(entity, 6);
                                    eventType = -1;
                                }
                                else
                                {
                                    if ((flags & 0x20) != 0 && (entity.FloorHeight == 0) && entity.IsAboveGround == 0)
                                    {
                                        eventType = 3;
                                    }
                                    if ((flags & 0x40) != 0 && (entity.HitCounter == 0) && (entity.Height != 0))
                                    {
                                        eventType = 3;
                                    }
                                }
                            }
                            else if ((flags & 0x20) != 0 && (entity.FloorHeight == 0) && entity.IsAboveGround == 0)
                            {
                                eventType = 3;
                            }

                            entity.Status = eventType;

                            /*
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
                            eventType = 2;*/
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
                for (var i = 1; i < StaticVariables.g_numberOfEntity; i++)
                {
                    var entity = StaticVariables.g_entitySlots[i];

                    if (entity.EventTrigger == -1) continue;

                    var programIndex = entity.ProgramIndexes[entity.EventTrigger] & 0x7f;

                    if (programIndex == 0)
                    {
                        // g_entityEventFunctionsByType => AI
                        _gameEngine.RunSpriteEvent(entity);
                        entity.EventTrigger = -1;
                    }
                    else
                    {
                        _gameEngine.RunScript(entity);
                        entity.EventTrigger = -1;
                    }

                    keepGoing = true;
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
                //entity.Index = 0;
                //entity.Index2 = 0;
                //...
                entity = new Entity(); //TODO: chack if create some bug with some code save a pointer on a entity
                entity.EntityRefId = -1; // g_emptyEntityForClearing.EntityRefId == -1
                StaticVariables.g_entitySlots[i] = entity;
            }

            if (entity.Status != 0)
            {
                max = i;
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

    private void UpdateTile(Entity entity)
    {
        int tileX;
        int tileY;
        uint tileAttr;
        uint tileFlags;
        uint bestFlagMask;
        uint[] tempFlags = new uint[4];

        if (entity.FrameCollision != null)
        {
            entity.FrameX = entity.XPos + entity.FrameXOff;
            entity.FrameY = entity.YPos + entity.FrameYOff;
            entity.FrameZ = entity.ZPos + entity.FrameZOff;
        }

        entity.TileX = (entity.XPos >> 16) / 24;
        entity.TileY = entity.YPos >> 20;
        entity.TileZ = entity.ZPos >> 20;


        var hitz = _gameEngine.GetCollisionOnZ(entity);
        entity.FloorHeight = hitz;
        entity.IsAboveGround = hitz < entity.ZPos ? 0 : 1;
        //entity.ZEntityCollision = hitz;
        //entity.CollidedWithEntityZ = hitz < entity.ZPos ? 0 : 1;

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


            var tl = _gameEngine.CurrentMap.Map.MapTiles[tileX + tileY * 52]; //entity.MapTiles[tileY * 0xd0 + tileX * 4 + 0x302];
            tileFlags = tl.Flags;
            //tileFlags = StaticVariables.g_spriteVRAMPointer + tileY * 0xd0 + tileX * 4 + 0x302;
            if (((tileFlags & 0xc00000) == 0) || ((tileFlags & 0x80000) == 0)) goto NoCollision;
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

            var tl = _gameEngine.CurrentMap.Map.MapTiles[tileX + tileY * 52]; //entity.MapTiles[tileY * 0xd0 + tileX * 4 + 0x302];
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
}