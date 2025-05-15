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

    // 80039d04
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
        entity.Flags = (uint)(sprite.Header.MoreFlags | sprite.Header.CanPickup << 8 | sprite.Header.FlagsPortraitShadowType << 16);

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
            sprite.Header.OffsetX, sprite.Header.OffsetY, sprite.Header.OffsetZ,
            sprite.Header.SizeX, sprite.Header.SizeY, sprite.Header.SizeZ);

        entity.XPos = x;
        entity.YPos = y;
        entity.ZPos = z - entity.ZMod + 1;

        UpdateAnimation(entity);

        entity.ModdedXPos = entity.XPos + entity.XMod;
        entity.ModdedYPos = entity.YPos + entity.YMod;
        entity.ModdedZPos = entity.ZPos + entity.ZMod;

        int height = ComputeEntityGroundHeight(entity);
        entity.TerrainHeight = height;

        if (entity.ZPos <= height + 1)
        {
            entity.ZPos = height + 1;
            entity.ModdedXPos = entity.XPos + entity.XMod;
            entity.ModdedYPos = entity.YPos + entity.YMod;
            entity.ModdedZPos = entity.ZPos + entity.ZMod;
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
    private void SetEntityDimensions(Entity entity, int offsetX, int offsetY, int offsetZ, int sizeX, int sizeY, int sizeZ)
    {
        entity.NegXMod = -(offsetX << 16);
        entity.NegYMod = -(offsetY << 16);
        entity.XMod = offsetX << 16;
        entity.YMod = offsetY << 16;
        entity.ZMod = offsetZ << 16;
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
        entity.IsZForceApplied = 0;
        var initializeValues = false;

        if (entity.TargetAnimationId != entity.CurrentAnimationId
            || entity.TargetDirection != entity.CurrentDirection)
        {
            entity.CurrentAnimationId = entity.TargetAnimationId;
            entity.CurrentFrameIndex = 0;
            entity.AnimCompleteCounter = 0;
            var animRecordPtr = entity.Sprite.AnimSets[entity.TargetAnimationId];
            var currentFrame = animRecordPtr.PreloadedAnims[entity.TargetDirection >> 3].Frames[entity.CurrentFrameIndex];
            entity.AnimSet = animRecordPtr;
            entity.Frame = currentFrame;
            entity.FirstFrame = currentFrame;
            entity.IsZForceApplied = entity.Sprite.Header.MoreFlags;
            //entity.NextFrameDelay = entity.Frame.Delay & 0x7f;

            initializeValues = true;

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
        else
        {
            entity.NextFrameDelay--;

            if (entity.NextFrameDelay == 0)
            {
                //var animRecordPtr = entity.Sprite.AnimSets[entity.CurrentAnimationId];
                //entity.AnimSet = animRecordPtr;
                //var anim = animRecordPtr.PreloadedAnims[entity.TargetDirection >> 3];
                var animRecordPtr = entity.AnimSet;
                var anim = entity.AnimSet.PreloadedAnims[entity.TargetDirection >> 3];
                entity.CurrentFrameIndex++;
                if (entity.CurrentFrameIndex >= anim.NumberOfFrames)
                {
                    entity.CurrentFrameIndex = 0;
                }

                var currentFrame = anim.Frames[entity.CurrentFrameIndex];
                //entity.NextFrameDelay = currentFrame.Delay & 0x7f;
                entity.Frame = currentFrame;
                entity.AnimCompleteCounter++;

                initializeValues = true;

                if (currentFrame.CollisionData != null) //currentFrame.CollisionOffset != 0xffff)
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
            }
            else
            {
                entity.ForceResetAnimationFlag = 1;
            }
        }

        if (initializeValues)
        {
            entity.NextFrameDelay = entity.Frame.Delay & 0x7f;
            entity.ForceResetAnimationFlag = 0;
            entity.AnimFlags = entity.AnimSet.Flags;
            entity.DepthSortVal = entity.AnimSet.U6;
            //entity.DepthSortVal = entity.Frame.Images.Unknown;
            //entity.DepthSortVal = entity.SpriteRef.DepthSortVal;
            //entity.SpriteRef.DepthSortVal = entity.DepthSortVal;
        }
    }

    // 80038ab4
    private void UpdateAnimation2(Entity entity)
    {
        SiFrame currentFrame = null;
        bool noSkip = true;

        entity.IsZForceApplied = 0;
        //var animationIndex  = entity.TargetAnimationId;
        var animationIndex = entity.CurrentAnimationId;
        var currentFrameIndex = -1; //StaticVariables.g_frameIndexTable[(entity.TargetDirection + 2 & 0x1c) + entity.CurrentFrameIndex * 0x20];

        if (entity.Frame != null
            && entity.AnimSet != null
            && entity.Frame == entity.AnimSet.PreloadedAnims[entity.TargetDirection >> 3].Frames[entity.CurrentFrameIndex])
        {
            currentFrameIndex = entity.CurrentFrameIndex;
        }

        //entity.Sprite.AnimSets[entity.CurrentAnimationId].PreloadedAnims[]
        //entity.AnimSet.PreloadedAnims[entity.TargetDirection >> 3].NumberOfFrames

        //var directionIndex = ((entity.TargetDirection + 2) & 0x1c) >> 2;
        //frameDelay = StaticVariables.g_frameIndexTable[directionIndex + (entity.CurrentFrameIndex << 3)];

        if (entity.TargetAnimationId != entity.CurrentAnimationId
            || currentFrameIndex != entity.CurrentFrameIndex)
        {
            noSkip = false;
        }

        if (noSkip)
        {
            entity.NextFrameDelay = entity.NextFrameDelay - 1;

            if (entity.NextFrameDelay != 0)
            {
                //entity.NextFrameDelay = 0x7fffffff;
                //entity.ForceResetAnimationFlag = 1;
                //return;


                //Debug.Assert(entity.AnimSet != null);
                //Debug.Assert(entity.Frame != null);
                //TODO: bug => remove this only to avoid null pointer
                //if (entity.AnimSet == null && entity.Sprite != null)
                //{
                //    var animSet = entity.Sprite.AnimSets[entity.CurrentAnimationId]; //frameDelay * 0xe
                //    entity.AnimSet = animSet;
                //    entity.Frame = animSet.PreloadedAnims[entity.TargetAnimationId >> 3].Frames[frameDelay];
                //}
            }

            currentFrame = entity.Frame;
            //currentFrame = entity.AnimSet.PreloadedAnims[entity.TargetAnimationId >> 3].Frames[frameDelay];
        }

        var frameDelay = 0;

        while (true)
        {
            while (noSkip)
            {
                frameDelay = entity.NextFrameDelay; //currentFrame.Delay;

                if (frameDelay == 0) //((uint)currentFrame.Delay & 0x80) != 0)
                {
                    Debug.Assert(entity.AnimSet != null);
                    //entity.NextFrameDelay = (int)(frameDelay & 0x7f);

                    var animSetPreloadedAnim = entity.AnimSet.PreloadedAnims[entity.TargetDirection >> 3];
                    entity.CurrentFrameIndex++;

                    try
                    {
                        var frame = animSetPreloadedAnim.Frames[entity.CurrentFrameIndex];
                        //entity.Frame = frame ?? entity.Frame; //TODO : why we don't check if the next frame exists?
                        if (frame == null)
                        {
                            entity.CurrentFrameIndex = 0;
                            frame = animSetPreloadedAnim.Frames[entity.CurrentFrameIndex];
                        }

                        entity.Frame = frame;
                        Debug.Assert(entity.Frame != null);
                    }
                    catch (Exception e)
                    {
                        Debugger.Break();
                    }

                    entity.NextFrameDelay = entity.Frame.Delay; //(int)(frameDelay & 0x7f);

                    if (entity.Frame.CollisionOffset != -1)
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

                    if (entity.Frame.ImageSetPointer != -1)
                    {
                        entity.SpriteRef.Images = entity.Frame.Images.Images;
                        entity.SpriteRef.DepthSortVal = entity.Frame.Images.Unknown;
                        entity.SpriteRef.NumImages = entity.Frame.Images.NumberOfImages;
                    }
                    else
                    {
                        entity.SpriteRef.Images = null;
                        entity.SpriteRef.DepthSortVal = 0;
                        entity.SpriteRef.NumImages = 0;
                    }

                    //currentFrame = entity.FirstFrame;
                    //entity.Frame = currentFrame;
                    entity.AnimCompleteCounter++;
                    return;
                }

                if (frameDelay == 0)
                {
                    break;
                }

                //if (frameDelay != 1)
                {
                    return;
                    //Debugger.Break();
                    //throw new Exception("Character Animation Error!!");
                }

                //currentFrame = entity.FirstFrame;
                //entity.Frame = currentFrame;
                //entity.AnimCompleteCounter++;
            }

            if (noSkip)
            {
                var animationId = (uint)currentFrame.CollisionOffset & 0xFF; // low part
                //frameDelay = (uint)currentFrame.transformIndexLow;

                if ((animationId & 0x80) != 0)
                {
                    break;
                }

                //frameDelay = entity.CurrentFrameIndex;
                entity.TargetAnimationId = (uint)animationId;
                entity.AnimCompleteCounter++;
            }

            LOAD_ANIMATION:
            Debug.Assert(frameDelay < entity.Sprite.AnimSets.Length);
            entity.CurrentAnimationId = entity.TargetAnimationId;
            var animSet = entity.Sprite.AnimSets[entity.CurrentAnimationId]; //frameDelay * 0xe
            entity.AnimSet = animSet;
            Debug.Assert(entity.AnimSet != null);
            //entity.AnimFlags = entity.AnimSet.Flags;
            //frameOffset = (ushort)((int)animSet.entries + animationFrameIndex * 2);
            //var animTableOffset = entity.AnimSet.AnimationOffsets[entity.CurrentFrameIndex];
            entity.CurrentFrameIndex = currentFrameIndex < 0 ? 0 : currentFrameIndex; // TODO : currentFrameIndex == -1
            //entity.CurrentAnimationId = (uint)frameDelay;

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

            entity.NextFrameDelay = currentFrame.Delay;
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

    // 800370c4
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
        int highest = 0;
        var slopesHit = 0;

        for (var i = 0; i < 4; i++)
        {
            var x = xs[i];
            var y = ys[i];
            var tilex = x / StaticVariables.MapTileWidth;
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
            var tiley = y / StaticVariables.MapTileHeight;
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
                            height += StaticVariables.MapTileHeight;//add a tile;
                        }
                        else
                        {
                            var my = ys[i];
                            var result = height + StaticVariables.MapTileHeight;
                            var my2 = my;
                            if (my < 0)
                            {
                                my2 = my + 15;
                            }

                            my2 = my2 / StaticVariables.MapTileHeight;
                            my2 = my2 * StaticVariables.MapTileHeight;
                            var remainder = my - my2;
                            height = result - remainder;
                        }
                        slopesHit |= 1;
                        break;
                    case 2:
                        if ((slopesHit & 5) != 0)//it already hit 1 or 3
                        {
                            height += StaticVariables.MapTileHeight;//add a tile;
                        }
                        else
                        {
                            var mx = xs[i];
                            var mx2 = mx / StaticVariables.MapTileWidth;
                            mx2 = mx2 * StaticVariables.MapTileWidth;
                            var remainder = mx - mx2;
                            // remainder = 0x17 - remainder;

                            //var result = (int)((float)remainder / 0x18 * StaticVariables.MapTileHeight);
                            var result = StaticVariables.g_heights_800236d4[0x17 - remainder % 0x18];

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
                            var mx2 = mx / StaticVariables.MapTileWidth;
                            mx2 = mx2 * StaticVariables.MapTileWidth;
                            var remainder = mx - mx2;
                            //remainder = 0x17 - remainder;

                            //var result = (int)((float)remainder / 0x18 * StaticVariables.MapTileHeight);
                            var result = StaticVariables.g_heights_800236d4[remainder % 0x18];
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
    /*
    // 80038064
    private void UpdateTileAttributes(Entity entity)
    {
        //set of variables set by certain special frames of animation
        if (entity.FrameCollision != null)
        {
            entity.HitBoxX = entity.XPos + entity.FrameXOff;
            entity.HitBoxY = entity.YPos + entity.FrameYOff;
            entity.HitBoxZ = entity.ZPos + entity.FrameZOff;
        }

        entity.TileX = (entity.XPos >> 16) / 24;
        entity.TileY = entity.YPos >> 20;
        entity.TileZ = entity.ZPos >> 20; //(z >> 16) / 16

        var hitz = _gameEngine.GetCollisionOnZ(entity);
        int tohit;
        entity.FloorHeight = hitz;
        entity.CollidedWithEntityZ = hitz < entity.ZPos ? 0 : 1;

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
            entity.ZForce = ridingEntity.FinalZForce;
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
            entity.XPos = platformEntity.XPos + platformEntity.RelativeWarpOffsetX;
            entity.YPos = platformEntity.YPos + platformEntity.RelativeWarpOffsetY;
            updatedZPosition = platformEntity.ZPos + platformEntity.RelativeWarpOffsetZ;
            entity.ModdedXPos = entity.XPos + entity.XMod;
            entity.ZPos = updatedZPosition;
            entity.ModdedYPos = entity.YPos + entity.YMod;
            entity.ModdedZPos = updatedZPosition + entity.ZMod;
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
                    entity.ZPos = platformHeight - entity.ZMod;
                    if ((entity.Flags & 0x100) == 0)
                    {
                        return;
                    }
                    entity.ZForce = 0;
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
                    entity.ZPos = platformHeight - entity.ZMod - entity.Depth;
                    if ((entity.Flags & 0x100) == 0)
                    {
                        return;
                    }
                    entity.ZForce = 0;
                    return;
                }

                MoveEntity(platformEntity);
            }
        }

        entity.ZPos = entity.ZPos + finalZVelocity;
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
                    entityIndex = entityIndex + 1;
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
        Entity[] collidableEntityPtr;
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
                        candidateZPos = candidateEntity.ModdedZPos;
                        if (entityTopZ < candidateZPos && candidateZPos <= platformCandidateZ)
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
                    collidableEntityPtr = collidableEntityPtr; // Already incremented by array index
                } while (entityIndex < StaticVariables.g_collideableEntitiesCount);
            }
        }

        platformHeight = platformCandidateZ;
        platformEntity = bestCandidate;
        return collisionDetected;
    }

    // 80037730
    private Entity ComputeXYPosition(Entity entity)
    {
        int flags;
        int dz;
        Entity otherEntity;
        int dy;
        int zTolerance;
        int dx;
        int halfDy;
        int halfDx;
        Entity candidate;
        int i;
        int posX;
        int posY;
        uint[] collisionFlags = new uint[4];
        int isStraightDir;
        int didAdjustForObstacle;
        int modX;
        int posZ;
        Entity result;

        didAdjustForObstacle = 0;
        isStraightDir = (entity.TargetDirection & 7) == 0 ? 1 : 0;

        START_COLLISION_CHECK:
        if (entity.FinalXForce == 0 && entity.FinalYForce == 0)
        {
            result = null;
            //goto FINALIZE;
            UpdateEntityPositions(entity, false);
            return result;
        }

        modX = 0;
        candidate = null;
        i = 0;
        dy = entity.FinalYForce;
        dx = entity.FinalXForce;

        TRY_ADVANCE:
        posX = entity.XPos;
        posY = entity.YPos;
        posZ = entity.ZPos;
        collisionFlags[0] = 0;
        collisionFlags[1] = 0;
        collisionFlags[2] = 0;
        collisionFlags[3] = 0;
        entity.XPos += dx;
        entity.YPos += dy;
        entity.ModdedXPos = entity.XPos + entity.XMod;
        entity.ModdedYPos = entity.YPos + entity.YMod;
        entity.ModdedZPos = entity.ZPos + entity.ZMod;
        result = candidate;
        var groundHeight = ComputeEntityGroundHeight(entity);
        entity.TerrainHeight = groundHeight;
        halfDx = dx >> 1;
        halfDy = dy >> 1;

        if ((entity.Flags & 0x100U) != 0 && entity.ZForce == 0)
        {
            dz = groundHeight - entity.ModdedZPos + -1;
            zTolerance = 0x30000;
            if (dz < 0)
            {
                zTolerance = 0x30003;
                dz = -dz;
            }
            if (zTolerance <= dz)
            {
                goto RESTORE_POS;
            }

            dz = entity.ZPos;
            entity.ZPos = groundHeight + 1;
            entity.ModdedXPos = entity.XPos + entity.XMod;
            entity.ModdedYPos = entity.YPos + entity.YMod;
            entity.ModdedZPos = entity.ZPos + entity.ZMod;
            otherEntity = FindEntityCollisionCandidate(entity);
            if (otherEntity != null)
            {
                entity.ZPos = dz;
                entity.ModdedXPos = entity.XPos + entity.XMod;
                entity.ModdedZPos = dz + entity.ZMod;
                entity.ModdedYPos = entity.YPos + entity.YMod;
                goto RESTORE_POS;
            }

            CHECK_ENTITY_COLLISION:
            if (entity == StaticVariables.g_entitySlots[0])
            {
                flags = (int)GetCollisionFlagsWithPlayer(entity, collisionFlags);
            }
            else
            {
                flags = (int)GetCollisionFlags(entity, collisionFlags);
            }

            if (flags != 0)
            {
                goto LAB_80037938;
            }

            modX = 1;
            if (i == 0)
            {
                return result;
            }

            if (dx == -1)
            {
                halfDx = 0;
            }

            if (dy == -1)
            {
                halfDy = 0;
            }

            if (isStraightDir == 0)
            {
                if (halfDx == 0)
                {
                    return result;
                }
            }
            else if (halfDx != 0)
            {
                i++;
                dy = halfDy;
                dx = halfDx;
                goto TRY_ADVANCE;
            }
            if (halfDy == 0)
            {
                return result;
            }

            LAB_80037db8:
            i++;
            dy = halfDy;
            dx = halfDx;
            goto TRY_ADVANCE;
        }

        RESTORE_POS:
        candidate = FindEntityCollisionCandidate(entity);
        if (candidate == null)
        {
            //goto CHECK_ENTITY_COLLISION
            if (entity == StaticVariables.g_entitySlots[0])
            {
                flags = (int)GetCollisionFlagsWithPlayer(entity, collisionFlags);
            }
            else
            {
                flags = (int)GetCollisionFlags(entity, collisionFlags);
            }

            if (flags != 0)
            {
                goto LAB_80037938;
            }

            modX = 1;
            if (i == 0)
            {
                return result;
            }

            if (dx == -1)
            {
                halfDx = 0;
            }

            if (dy == -1)
            {
                halfDy = 0;
            }

            if (isStraightDir == 0)
            {
                if (halfDx == 0)
                {
                    return result;
                }
            }
            else if (halfDx != 0)
            {
                i++;
                dy = halfDy;
                dx = halfDx;
                goto TRY_ADVANCE;
            }
            if (halfDy == 0)
            {
                return result;
            }

            LAB_80037db8:
            //i++;
            dy = halfDy;
            dx = halfDx;
            goto TRY_ADVANCE;
        }

        LAB_80037938:
        entity.XPos = posX;
        entity.YPos = posY;
        entity.ZPos = posZ;
        if (dx == -1)
        {
            halfDx = 0;
        }

        if (dy == -1)
        {
            halfDy = 0;
        }

        if (isStraightDir != 0)
        {
            if (halfDx != 0)
            {
                dy = halfDy;
                dx = halfDx;
                goto TRY_ADVANCE;
            }

            if (halfDy == 0)
            {
                goto LAB_8003799c;
            }

            i++;
            dy = halfDy;
            dx = halfDx;
            goto TRY_ADVANCE;
        }

        if (halfDx != 0 && halfDy != 0)
        {
            //goto LAB_80037db8;
            dy = halfDy;
            dx = halfDx;
            goto TRY_ADVANCE;
        }

        LAB_8003799c:
        if (modX != 0)
        {
            //goto LAB_80037d58;
            UpdateEntityPositions(entity, false);
            return result;
        }

        if (didAdjustForObstacle == 1 || (entity.Flags & 0x2000U) != 0 || candidate != null)
        {
            UpdateEntityPositions(entity);
            return result;
        }

        didAdjustForObstacle = 1;

        switch (entity.TargetDirection)
        {
            case 0:
                if ((collisionFlags[2] != 0 && collisionFlags[3] != 0) || collisionFlags[0] != 0 || collisionFlags[1] != 0)
                {
                    //goto switchD_80037a04_FINAL_OBSTACLE;
                    UpdateEntityPositions(entity);
                    return result;
                }

                entity.FinalYForce = 0;
                dy = (int)collisionFlags[2];
                dx = (int)collisionFlags[3];
                if (collisionFlags[2] == 0)
                {
                    if (dx == 0)
                    {
                        goto START_COLLISION_CHECK;
                    }
                }
                else if (collisionFlags[3] == 0)
                {
                    entity.FinalXForce = 0xc000;
                    goto START_COLLISION_CHECK;
                }
                break;

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
                        //goto LAB_80037c70;
                        entity.FinalXForce = 0;
                        goto START_COLLISION_CHECK;
                    }

                    //goto switchD_80037a04_FINAL_OBSTACLE;
                    UpdateEntityPositions(entity);
                    return result;
                }
                if (collisionFlags[3] != 0)
                {
                    LAB_80037c88:
                    entity.FinalYForce = 0;
                }
                goto START_COLLISION_CHECK;

            case 8:
                if ((collisionFlags[0] != 0 && collisionFlags[2] != 0) || collisionFlags[1] != 0 || collisionFlags[3] != 0)
                {
                    //goto switchD_80037a04_FINAL_OBSTACLE;
                    UpdateEntityPositions(entity);
                    return result;
                }

                entity.FinalXForce = 0;
                dy = (int)collisionFlags[0];
                dx = (int)collisionFlags[2];
                if (collisionFlags[0] == 0)
                {
                    if (dx != 0)
                    {
                        goto code_r0x80037c3c;
                    }

                    goto START_COLLISION_CHECK;
                }
                if (collisionFlags[2] == 0)
                {
                    entity.FinalYForce = 0x8000;
                    goto START_COLLISION_CHECK;
                }
                goto code_r0x80037c3c;

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
                        //goto switchD_80037a04_FINAL_OBSTACLE;
                        UpdateEntityPositions(entity);
                        return result;
                    }

                    entity.FinalYForce = 0;
                }
                goto START_COLLISION_CHECK;

            case 16:
                if ((collisionFlags[0] != 0 && collisionFlags[1] != 0) || collisionFlags[2] != 0 || collisionFlags[3] != 0)
                {
                    //goto switchD_80037a04_FINAL_OBSTACLE;
                    UpdateEntityPositions(entity);
                    return result;
                }

                entity.FinalYForce = 0;
                dy = (int)collisionFlags[0];
                dx = (int)collisionFlags[1];
                if (collisionFlags[0] == 0 && dx == 0)
                {
                    goto START_COLLISION_CHECK;
                }

                if (collisionFlags[1] == 0)
                {
                    entity.FinalXForce = 0xc000;
                    goto START_COLLISION_CHECK;
                }
                break;

            case 17:
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
                if (collisionFlags[0] != 0 && collisionFlags[3] != 0)
                {
                    //goto switchD_80037a04_FINAL_OBSTACLE;
                    UpdateEntityPositions(entity);
                    return result;
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
                    LAB_80037c70:
                    entity.FinalXForce = 0;
                }
                goto START_COLLISION_CHECK;

            case 24:
                if ((collisionFlags[1] != 0 && collisionFlags[3] != 0) || collisionFlags[0] != 0 || collisionFlags[2] != 0)
                {
                    //goto switchD_80037a04_FINAL_OBSTACLE;
                    UpdateEntityPositions(entity);
                    return result;
                }

                entity.FinalXForce = 0;
                dy = (int)collisionFlags[1];
                dx = (int)collisionFlags[3];
                if (collisionFlags[1] == 0)
                {
                    if (dx != 0)
                    {
                        goto code_r0x80037c3c;
                    }

                    goto START_COLLISION_CHECK;
                }
                if (collisionFlags[3] == 0)
                {
                    entity.FinalYForce = 0x8000;
                    goto START_COLLISION_CHECK;
                }
                code_r0x80037c3c:
                if (dy == 0)
                {
                    entity.FinalYForce = -0x8000;
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
                    //goto switchD_80037a04_FINAL_OBSTACLE;
                    UpdateEntityPositions(entity);
                    return result;
                }

                if (collisionFlags[2] != 0)
                {
                    //goto LAB_80037c88;
                    entity.FinalYForce = 0;
                    goto START_COLLISION_CHECK;
                }

                if (collisionFlags[1] != 0)
                {
                    entity.FinalXForce = 0;
                }

                goto START_COLLISION_CHECK;

            default:
                //goto switchD_80037a04_FINAL_OBSTACLE;
                UpdateEntityPositions(entity);
                return result;
        }

        if (dy == 0)
        {
            entity.FinalXForce = -0xc000;
        }

        goto START_COLLISION_CHECK;
    }

    private void UpdateEntityPositions(Entity entity, bool clearForceAdjusted = true)
    {
        if (clearForceAdjusted)
        {
            entity.ForceAdjusted = 1;
        }
        entity.ModdedXPos = entity.XPos + entity.XMod;
        entity.ModdedYPos = entity.YPos + entity.YMod;
        entity.ModdedZPos = entity.ZPos + entity.ZMod;
        entity.TerrainHeight = ComputeEntityGroundHeight(entity);
    }

    private uint GetCollisionFlagsWithPlayer(Entity entity, uint[] collisionFlags)
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

        if ((entity.Flags & 8U) != 0)
        {
            flag = 0x41;
        }

        if ((entity.Flags & 1U) != 0)
        {
            flag |= 0x1000;
        }

        moddedZPos = entity.ModdedZPos;

        for (int i = 0; i < 4; i++)
        {
            if ((entity.MapTiles[i].Flags & flag) != 0 || moddedZPos <= entity.MapHeights[i])
            {
                flags[i] = 1;
            }
        }

        return flags[0] | flags[1] | flags[2] | flags[3];
    }

    // 80036f34
    private Entity? FindEntityCollisionCandidate(Entity entity)
    {
        int value;
        Entity currentEntity;
        Entity[] collideableEntities;

        if ((entity != StaticVariables.g_entitySlots[0] || StaticVariables.g_debugState > -1 || (StaticVariables.g_debugFlags & 0x80000000) == 0)
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
                        spriteZForceTemp = entity.ZForce + _gameEngine.CurrentMap.Info.Gravity * -0x100;

                        spriteZForceAbs = spriteZForceTemp;
                        if (spriteZForceTemp < 0)
                        {
                            spriteZForceAbs = -spriteZForceTemp;
                        }

                        zForceMax = _gameEngine.CurrentMap.Info.TerminalVelocity * 0x100;
                        entity.ZForce = spriteZForceTemp;
                        if (zForceMax < spriteZForceAbs && spriteZForceTemp < 1)
                        {
                            entity.ZForce = _gameEngine.CurrentMap.Info.TerminalVelocity * -0x100;
                        }
                    }
                }
                else if ((entity.Flags & 0x100U) == 0
                         || (entity.CombinedVramFlagsOR & 0x10U) == 0
                            || 0 < StaticVariables.g_gravityFlag)
                {
                    entity.ZForce = entity.IsZForceApplied << 8;
                }
                else
                {
                    entity.ZForce = entity.IsZForceApplied * 0xa0;
                }

                UpdateEntityPhysics(entity);

                xForce = (uint)entity.XForceStep;
                yForce = (uint)entity.YForceStep;

                if ((entity.CombinedVramFlagsOR & 0x20U) != 0)
                {
                    xForce =
                        (uint)(((ulong)entity.XForceStep * 0x1000) >> 0x10) |
                        (uint)(((long)entity.XForceStep * 0x1000) >> 0x20) << 0x10;

                    yForce =
                        (uint)(((ulong)entity.YForceStep * 0x1000) >> 0x10) |
                        (uint)(((long)entity.YForceStep * 0x1000) >> 0x20) << 0x10;
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

                entity.XForce = IncrementForce(entity.XForce, (int)targetXForce, (int)xForce);
                entity.YForce = IncrementForce(entity.YForce, (int)targetYForce, (int)yForce);

                //LABEL_ProcessFinalForces:
                ApplyEntityForces(entity);
                entity.FinalXForce = entity.AdjustedXForce;
                entity.FinalYForce = entity.AdjustedYForce;
                entity.FinalZForce = entity.ZForce;
            }
            else
            {
                if (entity.PlatformEntity == null)
                {
                    if (entity.IsZForceApplied == 0)
                    {
                        if ((entity.Flags & 0x100U) != 0)
                        {
                            var force = entity.ZForce - (_gameEngine.CurrentMap.Info.Gravity << 8);
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

                            entity.ZForce = force;
                        }
                    }
                    else if ((short)entity.IsZForceApplied == -0x8000
                             && (entity.Flags & 0x100U) == 0)
                    {
                        entity.ZForce = 0;
                    }
                    else
                    {
                        entity.ZForce = entity.IsZForceApplied << 8;
                    }

                    UpdateEntityPhysics(entity);

                    entity.XForce = IncrementForce(entity.XForce, entity.TargetXForce, entity.XForceStep); // 2eme tour : -1536
                    entity.YForce = IncrementForce(entity.YForce, entity.TargetYForce, entity.YForceStep); // 2eme tour 1008

                    //goto LABEL_ProcessFinalForces;
                    ApplyEntityForces(entity);
                    entity.FinalXForce = entity.AdjustedXForce;
                    entity.FinalYForce = entity.AdjustedYForce;
                    entity.FinalZForce = entity.ZForce;
                    continue;
                }

                entity.ZForce = 0;
                entity.YForce = 0;
                entity.XForce = 0;
                entity.AdjustedYForce = 0;
                entity.AdjustedXForce = 0;
                entity.FinalZForce = 0;
                entity.FinalYForce = 0;
                entity.FinalXForce = 0;
            }
        }
    }

    // 80036828
    private void UpdateEntitiesForces2()
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
                    continue;
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

    // 800366fc
    private void ApplyEntityForces2(Entity entity)
    {
        int prevAdjustedX = entity.PreviousAdjustedXForce;
        int prevAdjustedY = entity.PreviousAdjustedYForce;

        entity.PreviousAdjustedXForce = 0;
        entity.PreviousAdjustedYForce = 0;

        int tableIndex = entity.TileAttributes & 0xF;
        int xForceComponent = (int)ScriptHelper.XForceTable[tableIndex];
        int yForceComponent = (int)ScriptHelper.YForceTable[tableIndex];

        xForceComponent += entity.XForce;
        yForceComponent += entity.YForce;

        int shiftAmount = _gameEngine.CurrentMap.Info.Gravity; // & 0xFF;
        xForceComponent >>= shiftAmount;
        yForceComponent >>= shiftAmount;

        xForceComponent += prevAdjustedX;
        yForceComponent += prevAdjustedY;

        int minX = entity.NegXMod;
        int newX = entity.TargetXForce + xForceComponent;

        if (newX < minX)
        {
            xForceComponent = minX - entity.TargetXForce;
            entity.CollidedWithEntityZ = 1;
        }
        else
        {
            int maxX = entity.ScreenClipX;
            if (newX > maxX)
            {
                xForceComponent = maxX - entity.TargetXForce;
                entity.CollidedWithEntityZ = 1;
            }
        }

        int minY = entity.NegYMod;
        int newY = entity.TargetYForce + yForceComponent;

        if (newY < minY)
        {
            yForceComponent = minY - entity.TargetYForce;
            entity.CollidedWithEntityZ = 1;
        }
        else
        {
            int maxY = entity.ScreenClipY;
            if (newY > maxY)
            {
                yForceComponent = maxY - entity.TargetYForce;
                entity.CollidedWithEntityZ = 1;
            }
        }

        entity.AdjustedXForce = xForceComponent;
        entity.AdjustedYForce = yForceComponent;
    }

    // 800366fc
    private void ApplyEntityForces(Entity entity)
    {
        var lastinteractx = entity.PreviousAdjustedXForce;
        var lastinteracty = entity.PreviousAdjustedYForce;
        entity.PreviousAdjustedYForce = 0;
        entity.PreviousAdjustedXForce = 0;
        var shitfAmount = _gameEngine.CurrentMap.Info.Gravity & 0x1f;
        var xval = entity.XForce + ScriptHelper.XForceTable[entity.TileAttributes & 0xf] >> shitfAmount;
        var yval = entity.YForce + ScriptHelper.YForceTable[entity.TileAttributes & 0xf] >> shitfAmount;

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

        entity.AdjustedXForce = (int)xval;
        entity.AdjustedYForce = (int)yval;
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

            var animid = -1;
            if ((entity.Slope_18c == 4 || entity.Slope_190 == 4)
                && entity.Slope_18c != entity.Slope_190)
            {
                //sliding effect
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
                    if ((entity.HitFrameCounter & 7) != 0)
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
                    if ((entity.HitFrameCounter & 0x7) != 0)
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

    //800386d0
    private void UpdateEntitiesEvents()
    {
        _gameEngine.MovePlayer();
        /*
        for (var i = 1; i < StaticVariables.g_numberOfEntity; i++)
        {
            var entity = StaticVariables.g_entitySlots[i];
            var eventProgramType = -1;

            if (entity.IsNotProcessable == 0 && entity.Status < 5)
            {
                switch (entity.Status)
                {
                    case (int)EntityStatus.Loaded:
                        eventProgramType = ScriptHelper.ProgramALoad;
                        entity.Status = (int)EntityStatus.Normal;
                        break;

                    case (int)EntityStatus.Normal:
                        var flags = entity.Flags;

                        if ((flags & 0x10) != 0)
                        {
                            if (entity.ActionState == 4)
                            {
                                _gameEngine.DestroyEntity(entity, 6);
                                eventProgramType = ScriptHelper.ProgramUnknown;
                                break;
                            }
                        }

                        if ((flags & 0x20) != 0 && (entity.ForceAdjusted & 0x8004) != 0)
                        {
                            _gameEngine.DestroyEntity(entity, -1);
                            eventProgramType = ScriptHelper.ProgramUnknown;
                            break;
                        }

                        if ((flags & 0x10) != 0 && entity.TerrainHeight == 0 && entity.IsAboveGround == 0)
                        {
                            entity.Status = (int)EntityStatus.Deactivated;
                            eventProgramType = ScriptHelper.ProgramDTouch;
                            break;
                        }

                        if ((flags & 0x20) != 0 && entity.HitCounter == 0)
                        {
                            entity.Status = (int)EntityStatus.Deactivated;
                            eventProgramType = ScriptHelper.ProgramDTouch;
                            break;
                        }

                        if ((flags & 0x40) != 0 && entity.TerrainHeight != 0)
                        {
                            entity.Status = (int)EntityStatus.Deactivated;
                            eventProgramType = ScriptHelper.ProgramEDeactivate;
                            break;
                        }

                        if (entity.TouchingEntity == null)
                        {
                            eventProgramType = ScriptHelper.ProgramDTouch;
                            break;
                        }

                        if (StaticVariables.g_activeCollisionEntity == entity)
                        {
                            if (entity.PlatformUpdateFlag == 0)
                            {
                                if (entity.ProgramIndexes[5] == 0)
                                {
                                    eventProgramType = ScriptHelper.ProgramCTick;
                                    break;
                                }
                            }
                            eventProgramType = ScriptHelper.ProgramFInteract;
                        }
                        break;

                    case (int)EntityStatus.Deactivated:
                        eventProgramType = ScriptHelper.ProgramEDeactivate;
                        break;
                }
            }

            entity.EventTrigger = eventProgramType;
        }
        */
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
                            else {
                                _gameEngine.DestroyEntity(entity,-1);
                                eventProgramType = ScriptHelper.ProgramUnknown;
                            }
                        }
                        else {
                            _gameEngine.DestroyEntity(entity,6);
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
        if (StaticVariables.g_numberOfEntity >= 0)
        {
            for (var i = 0; i <= StaticVariables.g_numberOfEntity; i++)
            {
                var entity = StaticVariables.g_entitySlots[i];
                //entity.HitFrameCounter++;
                //if (entity.DamagedTickCounter != 0)
                //{
                //    entity.DamagedTickCounter--;
                //}
                //
                //if (entity.FrameColTickCounter != 0)
                //{
                //    entity.FrameColTickCounter--;
                //}

                entity.TouchingEntity  = null;    
                entity.RidingEntity    = null;    
                entity.HitCounter      = 0;       

                entity.HitFrameCounter++;

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
                entity = new Entity(); //TODO: check if create bug with some code save a pointer on an entity
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
                StaticVariables.g_activeEntities[StaticVariables.g_activeEntityCount] = entity;
                StaticVariables.g_activeEntityCount++;
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
            entity.HitBoxX = entity.XPos + entity.FrameXOff;
            entity.HitBoxY = entity.YPos + entity.FrameYOff;
            entity.HitBoxZ = entity.ZPos + entity.FrameZOff;
        }

        //entity.TileX = StaticVariables.g_tileToWorldXTable[entity.XPos + 2];
        entity.TileX = (entity.XPos >> 16) / StaticVariables.MapTileWidth;
        entity.TileY = entity.YPos >> 20;
        entity.TileZ = entity.ZPos >> 20;


        var hitz = _gameEngine.GetCollisionOnZ(entity);
        entity.TerrainHeight = hitz;
        entity.IsAboveGround = hitz < entity.ZPos ? 0 : 1;
        //entity.FloorHeight = hitz;
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