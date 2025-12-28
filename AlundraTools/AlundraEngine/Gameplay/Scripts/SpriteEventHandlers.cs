using AlundraEngine.Gameplay.Scripts.Boss;
using System;
using System.Diagnostics;

namespace AlundraEngine.Gameplay.Scripts;

public class SpriteEventHandlers
{
    private readonly GameEngine _gameEngine;
    public delegate void SpriteEventHandler(Entity entity);
    private readonly Dictionary<int, SpriteEventHandler?>[] _typeHandlers = new Dictionary<int, SpriteEventHandler?>[6];

    public SpriteEventHandlers(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;

        //g_entityEventFunctionsByType  // 80098f4c
        _typeHandlers[ScriptHelper.ProgramALoad] = new Dictionary<int, SpriteEventHandler?>();       // 800c4b34
        _typeHandlers[ScriptHelper.ProgramBMap] = null;                                              // there are no spriteEvent handlers for map
        _typeHandlers[ScriptHelper.ProgramCTick] = new Dictionary<int, SpriteEventHandler?>();       // 800c4f34  
        _typeHandlers[ScriptHelper.ProgramDTouch] = new Dictionary<int, SpriteEventHandler?>();      // 800c5334  
        _typeHandlers[ScriptHelper.ProgramEDeactivate] = new Dictionary<int, SpriteEventHandler?>(); // 800c5734  
        _typeHandlers[ScriptHelper.ProgramFInteract] = new Dictionary<int, SpriteEventHandler?>();   // 800c5b34  

        Register(ScriptHelper.ProgramALoad, 0, SetSpawnFlagFromZPos);
        Register(ScriptHelper.ProgramALoad, 1, SetAnimationTo2);
        Register(ScriptHelper.ProgramALoad, 2, SetRandomizedAnimIdAndFlag);
        Register(ScriptHelper.ProgramALoad, 3, SetAnimationTo4);
        Register(ScriptHelper.ProgramALoad, 4, SetAnimationTo9);
        Register(ScriptHelper.ProgramALoad, 5, SetAnimationTo7);
        Register(ScriptHelper.ProgramALoad, 6, SetAnimationTo3);
        Register(ScriptHelper.ProgramALoad, 7, SetAnimationTo9_2);
        Register(ScriptHelper.ProgramALoad, 8, SetAnimationTo8AndCustomByte3);
        Register(ScriptHelper.ProgramALoad, 9, SetAnimationTo10);
        Register(ScriptHelper.ProgramALoad, 10, SetAnimationTo6);
        Register(ScriptHelper.ProgramALoad, 11, SpawnWarpAndSetAnim);
        Register(ScriptHelper.ProgramALoad, 12, TriggerMultipleWarpsAndClearHistory);
        Register(ScriptHelper.ProgramALoad, 13, AI_EmptyFunction); // null
        Register(ScriptHelper.ProgramALoad, 14, SetAnim8AndResetLoader);
        Register(ScriptHelper.ProgramALoad, 15, SpawnSpecificWarpAndResetLoader);
        Register(ScriptHelper.ProgramALoad, 16, SetAnim0AndResetLoader);
        Register(ScriptHelper.ProgramALoad, 17, SetAnimEAndResetLoader);
        Register(ScriptHelper.ProgramALoad, 18, SetAnimEAndClearZForce);
        Register(ScriptHelper.ProgramALoad, 19, SetCustomByteFromProgramIndex);
        Register(ScriptHelper.ProgramALoad, 20, SetCustomByteFromZPos);
        Register(ScriptHelper.ProgramALoad, 21, SpawnWarpDropAndAdjustPosition);
        Register(ScriptHelper.ProgramALoad, 22, SpawnVerticalWarpColumns);
        Register(ScriptHelper.ProgramALoad, 254, FUN_80061bcc);
        Register(ScriptHelper.ProgramALoad, 255, FUN_80061bd4);

        Register(ScriptHelper.ProgramCTick, 0, AI_EmptyFunction); // null
        Register(ScriptHelper.ProgramCTick, 1, AI_UpdateEntityAI_IdleSkittish);
        Register(ScriptHelper.ProgramCTick, 2, AI_UpdateEntityAI_CuriousFlying);
        Register(ScriptHelper.ProgramCTick, 3, AI_UpdateEntityAI_1);
        Register(ScriptHelper.ProgramCTick, 4, AI_FUN_80066984);
        Register(ScriptHelper.ProgramCTick, 16, AI_FUN_80069f44);
        Register(ScriptHelper.ProgramCTick, 23, AI_FUN_8006b848);
        Register(ScriptHelper.ProgramCTick, 60, AI_UpdateIceProjectile);
        Register(ScriptHelper.ProgramCTick, 70, AI_FUN_8007b7b0);
        Register(ScriptHelper.ProgramCTick, 72, AI_ProcessWarpTransitionState);
        Register(ScriptHelper.ProgramCTick, 88, AI_Melzas2_FinalBoss);
        Register(ScriptHelper.ProgramCTick, 89, AI_SpawnWarpIfValid);
        Register(ScriptHelper.ProgramCTick, 90, AI_UpdateMelzas2CutsceneChannels);
        Register(ScriptHelper.ProgramCTick, 91, AI_UpdateEntityAI_IdleLookAround);
        Register(ScriptHelper.ProgramCTick, 92, AI_UpdateEntityTriggerWarpBehavior);
        Register(ScriptHelper.ProgramCTick, 93, AI_UpdateEntityAI_IdleCurious);
        Register(ScriptHelper.ProgramCTick, 101, AI_FUN_80065750);
        Register(ScriptHelper.ProgramCTick, 255, FUN_8007c174);

        Register(ScriptHelper.ProgramDTouch, 0, AI_EmptyFunction); // null
        Register(ScriptHelper.ProgramDTouch, 4, AI_EmptyFunction); // null
        Register(ScriptHelper.ProgramDTouch, 12, AI_FUN_8007df4c); // null
        Register(ScriptHelper.ProgramDTouch, 42, AI_FUN_8007eb58);
        Register(ScriptHelper.ProgramDTouch, 43, AI_FUN_8007eba8);

        Register(ScriptHelper.ProgramEDeactivate, 0, Script_Deactivate_FUN_8007ed10);
        Register(ScriptHelper.ProgramEDeactivate, 2, AI_FUN_8007ed30);
        Register(ScriptHelper.ProgramEDeactivate, 9, AI_HandleIceLightHitEffect);
        Register(ScriptHelper.ProgramEDeactivate, 17, AI_UpdateArrows);
        Register(ScriptHelper.ProgramEDeactivate, 27, AI_DestroyEntity);

        Register(ScriptHelper.ProgramFInteract, 0, AI_EmptyFunction); // null
        Register(ScriptHelper.ProgramFInteract, 1, Script_FInteract_FUN_8007fc64);
        Register(ScriptHelper.ProgramFInteract, 255, FUN_8007fc88);
    }

    private void Register(int type, byte code, SpriteEventHandler handler)
    {
        _typeHandlers[type].Add(code, handler);
    }

    public void RunSpriteHandler(int eventType, int eventId, Entity entity)
    {
        if (eventType == ScriptHelper.ProgramBMap)
        {
            return;
        }

        var handlers = _typeHandlers[eventType];

        if (handlers.TryGetValue(eventId, out var handler)) //g_entityEventFunctionsByType  // 80098f4c
        {
            if (handler != AI_EmptyFunction)
            {
                //Debug.WriteLine($"Entity {entity.Index} exec func[{eventType}][{eventId}] => {handler.Method.Name}");
            }

            handler(entity);
        }
        else
        {
            Debugger.Break();
        }
    }

    private void AI_EmptyFunction(Entity entity) { }

    #region function type A

    // 8006174C
    public void SetSpawnFlagFromZPos(Entity entity)
    {
        entity.AIValues[0] = (short)(entity.PosZ >> 16);
    }

    // 80061758
    public void SetAnimationTo2(Entity entity)
    {
        entity.TargetAnimationId = 2;
    }

    // 80061764
    public void SetRandomizedAnimIdAndFlag(Entity entity)
    {
        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7D2B89DD + 0xE06A02E7;
        entity.TargetAnimationId = 4;
        entity.AIValues[0] = (short)((((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x1F) >> 32) + 0xB4);
    }

    // 800617B8
    public void SetAnimationTo4(Entity entity)
    {
        entity.TargetAnimationId = 4;
    }

    // 800617C4
    public void SetAnimationTo9(Entity entity)
    {
        entity.TargetAnimationId = 9;
    }

    // 800617D0
    public void SetAnimationTo7(Entity entity)
    {
        entity.TargetAnimationId = 7;
    }

    // 800617DC
    public void SetAnimationTo3(Entity entity)
    {
        entity.TargetAnimationId = 3;
    }

    // 800617E8
    public void SetAnimationTo9_2(Entity entity)
    {
        entity.TargetAnimationId = 9;
    }

    // 800617F4
    public void SetAnimationTo8AndCustomByte3(Entity entity)
    {
        entity.TargetAnimationId = 8;
        entity.Bytes[0] = 3;
    }

    // 80061808
    public void SetAnimationTo10(Entity entity)
    {
        entity.TargetAnimationId = 10;
    }

    // 80061814
    public void SetAnimationTo6(Entity entity)
    {
        entity.TargetAnimationId = 6;
    }

    // 80061820
    public void SpawnWarpAndSetAnim(Entity entity)
    {
        Entity spawned = _gameEngine.SpawnWarpEntity(entity, 1, 0xF5, entity.PosX + 0xF00000,
            entity.PosY, entity.PosZ, entity.TargetDirection);
        entity.AIValues[0] = (short)spawned.Index;
        spawned.AIValues[1] = 0;
        entity.TargetAnimationId = 3;
        spawned.TargetAnimationId = 0;
    }

    // 80061888
    public void TriggerMultipleWarpsAndClearHistory(Entity entity)
    {
        var index = Array.IndexOf(_gameEngine.StaticVariables.g_entitySlots, entity);
        for (int i = index + 1; i < index + 15; i++)
        {
            _gameEngine.DestroyEntity(_gameEngine.StaticVariables.g_entitySlots[i]);
        }

        for (int i = 0; i < 0x100; i++)
        {
            _gameEngine.StaticVariables.g_loaderDirectionHistory[i] = 0;
            _gameEngine.StaticVariables.DAT_80191508[i] = 0;
            _gameEngine.StaticVariables.DAT_80191708[i] = 0;
        }

        _gameEngine.StaticVariables.g_loaderInitialized = 0;
    }

    // 8006191C
    public void SetAnim8AndResetLoader(Entity entity)
    {
        entity.TargetAnimationId = 8;
        _gameEngine.StaticVariables.g_loaderInitialized = 0;
    }

    // 80061930
    public void SpawnSpecificWarpAndResetLoader(Entity entity)
    {
        Entity spawned = _gameEngine.SpawnWarpEntity(entity, 1, 0x9D,
            entity.PosX + 0x380000, entity.PosY, entity.PosZ - 0x200000, entity.TargetDirection);
        entity.AIValues.Set(0xa000, 1); //spawned;
        _gameEngine.StaticVariables.g_loaderInitialized = 0;
    }

    // 80061998
    public void SetAnim0AndResetLoader(Entity entity)
    {
        entity.TargetAnimationId = 0;
        _gameEngine.StaticVariables.g_loaderInitialized = 0;
    }

    // 800619A8
    public void SetAnimEAndResetLoader(Entity entity)
    {
        entity.TargetAnimationId = 0xE;
        _gameEngine.StaticVariables.g_loaderInitialized = 0;
        entity.AIValues[1] = 0;
    }

    // 800619C0
    public void SetAnimEAndClearZForce(Entity entity)
    {
        entity.TargetAnimationId = 0xE;
        entity.AIValues[1] = 0;
    }

    // 800619D0
    public void SetCustomByteFromProgramIndex(Entity entity)
    {
        entity.DelayOrAngle = entity.ProgramIndexes[2]; //Bytes[0] + 1
    }

    // 800619DC
    public void SetCustomByteFromZPos(Entity entity)
    {
        entity.Bytes[0] = (byte)(entity.PosZ & 0xFF);
    }

    // 800619E8
    public void SpawnWarpDropAndAdjustPosition(Entity entity)
    {
        Entity spawned = _gameEngine.SpawnWarpEntity(entity, 1, 0xD8, entity.PosX, entity.PosY, entity.PosZ, entity.TargetDirection);
        spawned.TargetAnimationId = 5;
        entity.TargetAnimationId = 3;
        entity.PosY -= 0x100000;
        entity.PosZ += 0x300000;
    }

    // 80061A6C
    public void SpawnVerticalWarpColumns(Entity entity)
    {
        int z = 0xF40000;
        entity.ItemState = 1;
        entity.DelayOrAngle = 1;

        Entity parentEntity = _gameEngine.SpawnWarpEntity(entity, 1, 0xD1, 0x2400000, 0x2600000, z, 0);
        parentEntity.Bytes[0] = 0;
        
        for (int i = 0; i < 7; i++)
        {
            z -= 0xC0000;
            Entity entitySpawned = _gameEngine.SpawnWarpEntity(entity, 1, 0xD1, 0x2400000, 0x2600000, z, 0);
            entitySpawned.TargetAnimationId = 7;
            parentEntity.AIValues[2] = (short)entitySpawned.Index;
            parentEntity = entitySpawned;
        }

        z = 0xF40000;
        parentEntity = _gameEngine.SpawnWarpEntity(entity, 1, 0xD1, 0x2D00000, 0x2600000, z, 0);
        parentEntity.Bytes[0] = 1;

        for (int i = 0; i < 7; i++)
        {
            z -= 0xC0000;
            Entity entitySpawned = _gameEngine.SpawnWarpEntity(entity, 1, 0xD1, 0x2D00000, 0x2600000, z, 0);
            entitySpawned.TargetAnimationId = 7;
            parentEntity.AIValues[2] = (short)entitySpawned.Index;
            parentEntity = entitySpawned;
        }

        _gameEngine.SoundManager.PlaySoundEffect(0x19C);
        _gameEngine.StaticVariables.g_loaderInitialized = 0;
    }

    //80061bcc
    private void FUN_80061bcc(Entity entity)
    {
        //do nothing
    }

    //80061bd4
    private void FUN_80061bd4(Entity entity)
    {
        ushort contentFlags;
        uint[] flags;

        if (entity.SpriteTableIndex == 0x1e)
        {
            if (entity.ContentsItemId == 0)
            {
                entity.TargetAnimationId = 1;
            }

            return;
        }

        if (entity.Bytes[0] == 1)
        {
            return;
        }

        if (entity.Bytes[0] == 2)
        {
            if (entity.AIValues[4] == 1)
            {
                if (entity.AIValues[4] != 0)
                {
                    _gameEngine.StartCdStreaming(0xb);
                }
            }
            else
            {
                entity.Status = 1;
            }

            return;
        }

        entity.Flags &= 0xffffff7f;

        if (entity.ContentsGameFlag != 0)
        {
            contentFlags = (ushort)entity.ContentsGameFlag;

            if ((contentFlags & 0x8000) == 0)
            {
                flags = _gameEngine.StaticVariables.g_saveData.MapFlags;
            }
            else
            {
                flags = _gameEngine.StaticVariables.g_globalFlags;
            }


            var index = ((contentFlags >> 3) & 0xffc) >> 2;
            var mask = 1 << (entity.ContentsGameFlag & 0x1f);

            if ((flags[index] & mask) == 0)
            {
                _gameEngine.DestroyEntity(entity);
                return;
            }
        }

        entity.AIValues[0] = -1;
        entity.AIValues[1] = -1;
        entity.AIValues[2] = 0;
        entity.AIValues[3] = 0;
        entity.DelayOrAngle = entity.ContentsGameFlag;
        entity.ItemState = 0;
    }

    #endregion

    #region function type C

    // 80065ED4
    private void AI_UpdateEntityAI_IdleSkittish(Entity entity)
    {
        short delay;
        uint uVar2;
        int[] relPos = new int[6];
        short duration;

        ScriptHelper.CalculateEntityRelativePosition(entity, _gameEngine.StaticVariables.PlayerEntity, relPos);

        switch (entity.TargetAnimationId)
        {
            case 0:
                duration = entity.AIValues[1];
                if (duration != 0)
                {
                    entity.AIValues[1] = (short)(duration - 1);
                    if (duration != 1)
                    {
                        return;
                    }
                }

                if (relPos[0] < 3 && relPos[1] < 3 && relPos[2] < 0x200001 && entity.Bytes[0] + 1 == 0)
                {
                    uVar2 = (uint)ScriptHelper.GetDirectionToTarget(_gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, _gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = uVar2;
                    entity.TargetAnimationId = 3;
                    entity.Bytes[1] = 1;
                    return;
                }

                _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                entity.TargetAnimationId = 1;
                entity.AIValues[1] = (short)((short)(((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x3d) >> 32) + 0x3c);
                if (relPos[0] < 4 && relPos[1] < 4)
                {
                    uVar2 = (uint)ScriptHelper.GetDirectionToTarget(_gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, _gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = uVar2;
                }
                goto case 3;

            case 1:
                duration = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = duration;

                if (duration != 0)
                {
                    return;
                }

                if (entity.ForceAdjusted == 0)
                {
                    _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    var uVar1 = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed;
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = (short)((short)((uVar1 * 0x2000) >> 32) & 0x7f);
                    entity.TargetDirection = (uint)((uVar1 * 0x2000) >> 40);
                    return;
                }

                _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                var direction = (byte)ScriptHelper.DirectionTable[entity.TargetDirection]; // g_directionFlipTable //80028b34
                entity.ForceStepY = 0;
                entity.ForceStepX = 0;
                entity.ForceY = 0;
                entity.ForceX = 0;
                entity.TargetForceY = 0;
                entity.TargetForceX = 0;
                entity.TargetDirection = direction;
                delay = (short)(((uint)((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x1f) >> 32) + 0x1e);
                goto case 4;

            case 2:
                if (relPos[0] < 4 && relPos[1] < 4)
                {
                    entity.TargetAnimationId = 4;
                }
                break;

            case 3:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }
                _gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x28, 0x14);
                entity.Bytes[1] = 0;
                break;

            case 4:
                delay = 0x1e;
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }
                entity.TargetAnimationId = 0;
                entity.AIValues[1] = delay;
                break;

            case 7:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }
                if (entity.Bytes[0] + 3 != 0)
                {
                    entity.TargetAnimationId = 6;
                    entity.Flags |= 0x40;
                    return;
                }
                delay = 0x28;
                entity.TargetAnimationId = 0;
                entity.AIValues[1] = delay;
                break;

            case 8:
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
                if (entity.IsAboveGround != 0)
                {
                    entity.TargetAnimationId = 2;
                }
                break;
            case 19:
                duration = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = duration;
                delay = 0x1e;
                if (duration != 0)
                {
                    return;
                }
                entity.TargetAnimationId = 0;
                entity.AIValues[1] = delay;
                break;
        }
    }

    // 80066250
    private void AI_UpdateEntityAI_CuriousFlying(Entity entity)
    {
        bool bVar1;
        uint direction;
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, _gameEngine.StaticVariables.PlayerEntity, relativePositions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.AIValues[1] != 0)
                {
                    entity.AIValues[1]--;
                    return;
                }
                entity.AIValues[1] = 0x50;
                direction = 1;
                goto SetNewAnim;

            case 1:
                var sVar2 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar2;
                if (sVar2 == 0)
                {
                    _gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x50, 0x1e);
                    entity.Bytes[0] = 0;
                }
                else if (entity.Bytes[0] == 0 && _gameEngine.EntityGameplayManager.TryAttackPlayer(entity, relativePositions, 2, 0x100000))
                {
                    entity.TargetAnimationId = 0xd;
                    direction = (uint)ScriptHelper.GetDirectionToTarget(
                        _gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        _gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.AIValues[6] = 0;
                    entity.AIValues[7] = 0;
                }
                else if (entity.ForceAdjusted == 0)
                {
                    _gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 1, 0x200000);
                }
                else
                {
                    _gameEngine.EntityGameplayManager.UpdateDirectionForced(entity, 1, 4, 0x200000);
                }
                break;

            case 3:
                direction = (uint)(entity.AIValues[6] + 1);
                entity.AIValues[6] = (short)direction;
                if ((direction & 7) == 0)
                {
                    _gameEngine.EffectManager.CreateEffectEntity(0, _gameEngine.StaticVariables.g_imageBuffer[10], 0,
                        entity.PosX, entity.PosY, entity.TerrainHeight);
                }

                if (_gameEngine.StaticVariables.PlayerEntity.TouchingEntity == entity)
                {
                    _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    if ((int)(((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 3) >> 32) == 0)
                    {
                        entity.TargetAnimationId = 1;
                        direction = (uint)ScriptHelper.GetDirectionToTarget(
                            entity.PosX - _gameEngine.StaticVariables.PlayerEntity.PosX,
                            entity.PosY - _gameEngine.StaticVariables.PlayerEntity.PosY);
                        entity.TargetDirection = direction;
                        entity.AIValues[1] = 0x3c;
                        entity.Bytes[0] = 1;
                    }
                    else
                    {
                        entity.TargetAnimationId = 0;
                        entity.AIValues[1] = 0x14;
                    }
                }

                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 10;
                }
                break;

            case 6:
                if (entity.IsAboveGround != 0)
                {
                    _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = (short)((short)(((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x29) >> 32) + 10);
                }
                break;

            case 7:
            case 0xc:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[3] == 0)
                    {
                        if (entity.TargetAnimationId == 0xc)
                        {
                            entity.TargetDirection = (entity.TargetDirection + 0x10) & 0x1f;
                        }
                        entity.TargetAnimationId = 0;
                        entity.AIValues[1] = 0x1e;
                        entity.Bytes[0] = 0;
                    }
                    else
                    {
                        entity.TargetAnimationId = 8;
                        entity.Flags |= 0x40;
                    }
                }
                break;

            case 0xb:
                if (entity.TargetAnimationId != entity.CurrentAnimationId)
                {
                    return;
                }
                direction = 10;
                if (entity.IsAboveGround == 0)
                {
                    return;
                }
            SetNewAnim:
                entity.TargetAnimationId = direction;
                break;
        }
    }

    // 800665a0
    private void AI_UpdateEntityAI_1(Entity entity)
    {
        short frameTimer;
        uint direction;
        int[] relPos = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, _gameEngine.StaticVariables.PlayerEntity, relPos);

        switch (entity.TargetAnimationId)
        {
            case 0:
            case 1:
                if (entity.AIValues[1] != 0)
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                }

                if (entity.DelayOrAngle != 0)
                {
                    entity.DelayOrAngle -= 1;
                }

                if (entity.AIValues[1] == 0)
                {
                    _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    entity.TargetAnimationId = 6;
                    entity.AIValues[1] = (short)((((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x1f) >> 0x20) + 0xb4);
                }

                if (entity.DelayOrAngle != 0 || 2 < relPos[0] || 2 < relPos[1] || 0x100000 < relPos[2])
                {
                    if (entity.ForceAdjusted != 0)
                    {
                        var bVar1 = _gameEngine.StaticVariables.g_directionFlipTable[entity.TargetDirection];
                        entity.ForceStepY = 0;
                        entity.ForceStepX = 0;
                        entity.ForceY = 0;
                        entity.ForceX = 0;
                        entity.TargetForceY = 0;
                        entity.TargetForceX = 0;
                        entity.TargetDirection = bVar1;
                        return;
                    }

                    _gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 1, 0);
                    return;
                }

                goto LAB_80066938;
            case 3:
                if (_gameEngine.StaticVariables.PlayerEntity.TouchingEntity == entity)
                {
                    entity.Bytes[2] = 1;
                }

                frameTimer = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = frameTimer;
                if (frameTimer == 0)
                {
                    if (entity.Bytes[2] == 0)
                    {
                        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                        entity.TargetAnimationId = 1;
                        if (((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 3) >> 0x20 == 0)
                        {
                            entity.TargetDirection = (entity.TargetDirection + 0x10) & 0x1f;
                        }

                        entity.AIValues[1] = 0x3c;
                        entity.DelayOrAngle = 0x3c;
                    }
                    else
                    {
                        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                        entity.DelayOrAngle = 0;
                        entity.TargetAnimationId = 6;
                        entity.AIValues[1] = (short)((((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x1f) >> 0x20) + 0xb4);
                    }
                }
                break;
            case 4:
                frameTimer = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = frameTimer;
                if (frameTimer == 0)
                {
                    entity.TargetAnimationId = 5;
                }

                if (relPos[0] < 3 && relPos[1] < 3 && relPos[2] < 0x100001)
                {
                    entity.TargetAnimationId = 5;
                    entity.Bytes[1] = 3;
                }
                break;
            case 5:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[1] == 3)
                    {
                        entity.TargetAnimationId = 3;
                        direction = (uint)ScriptHelper.GetDirectionToTarget(_gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, _gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                        entity.TargetDirection = direction;
                        entity.AIValues[1] = 0x78;
                        entity.Bytes[2] = 0;
                        entity.Bytes[1] = 0;
                    }
                    else
                    {
                        _gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x78, 0x14);
                    }
                }
                break;
            case 8:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[3] != 0)
                {
                    entity.TargetAnimationId = 2;
                    entity.Flags |= 0x40;
                    return;
                }

            LAB_80066938:
                entity.TargetAnimationId = 3;
                direction = (uint)ScriptHelper.GetDirectionToTarget(_gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, _gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                entity.TargetDirection = direction;
                entity.AIValues[1] = 0x78;
                entity.Bytes[2] = 0;
                break;
        }
    }

    //80066984
    private void AI_FUN_80066984(Entity entity)
    {
        int value;
        Entity entity2;

        if (entity.PlatformEntity != null)
        {
            value = entity.Flags2;
            if (value != 0)
            {
                if (value == -1)
                {
                    value = entity.SpriteProgramIndexes[4];
                    entity.TargetAnimationId = 3;
                    if (value == 2)
                    {
                        entity.Flags = (entity.Flags & 0xffffff7fU) | 0x10;
                    }
                }
                else
                {
                    entity.TargetAnimationId = (uint)_gameEngine.StaticVariables.g_scriptAnimationTable[value];
                }
                entity.Flags2 = 0;
                if (entity.PlatformEntity != null)
                {
                    entity.PlatformEntity.CarriedEntity = null;
                }
                entity.PlatformEntity = null;
                return;
            }
            goto LAB_80066be0;
        }

        if (entity.TargetAnimationId == 0)
        {
            value = (int)entity.SpriteTableIndex;
        }
        else if (entity.SpriteTableIndex != 0x175 || ((value = 0x175) != 0 && entity.TargetAnimationId != 5))
        {
            if (entity.ForceAdjusted == 0)
            {
                if (entity.IsAboveGround != 0 || entity.HitCounter != 0)
                {
                    _gameEngine.SoundManager.PlaySoundEffect(0x18);
                    entity.DelayOrAngle = 0;
                    value = 0;
                    entity2 = entity;
                    do
                    {
                        var tile = entity2.MapTiles[value];
                        if (((tile.Walkability | (tile.GroundProperty << 8)) & 0x1001) == 0x1001)
                        {
                            break;
                        }
                        value++;
                        //entity2 = entity2.Index2; // ??
                    } while (value < 4);

                    if (value == 4)
                    {
                        entity.TargetAnimationId = 0;
                    }
                }
            }
            else
            {
                value = 0;
                entity2 = entity;
                if (entity.IsAboveGround == 0)
                {
                    if (entity.ForceZ > 0 && entity.DelayOrAngle == 0)
                    {
                        entity.DelayOrAngle = 1;
                        entity.TargetDirection = (entity.TargetDirection + 0x10) & 0x1f;
                        _gameEngine.EffectManager.CreateEffectEntity(0, 9, 0, entity.PosX, entity.PosY, entity.PosZ + 0x80000);
                    }
                }
                else
                {
                    do
                    {
                        var tile = entity2.MapTiles[value];
                        if (((tile.Walkability | (tile.GroundProperty << 8)) & 0x1001) == 0x1001)
                        {
                            entity.Status = 3;
                        }
                        value++;
                        Debugger.Break();
                        //entity2 = entity2.Index2; // ??
                    } while (value < 4);

                    entity.TargetAnimationId = 0;
                }
            }

            value = (int)entity.SpriteTableIndex;
        }

        if (value != 0x175)
        {
            return;
        }

        if (entity.TargetAnimationId == 0 && entity.Slope_190 == 4)
        {
            entity.TargetAnimationId = 5;
            return;
        }

        if (entity.TargetAnimationId != 5)
        {
            return;
        }

        if (entity.Slope_190 == 4)
        {
            return;
        }

    LAB_80066be0:
        entity.TargetAnimationId = 0;
    }

    //80066984
    private void AI_FUN_80069f44(Entity entity)
    {
        bool bVar1;
        byte bVar2;
        short sVar3;
        uint uVar4;
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, _gameEngine.StaticVariables.PlayerEntity, relativePositions);
        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.AIValues[1] == 0)
                {
                    _gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x78, 0x46);
                    entity.Bytes[1] = 0;
                }
                else
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                }
                break;

            case 1:
                sVar3 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar3;

                if (sVar3 == 0)
                {
                    if (entity.Bytes[1] == 0)
                    {
                        entity.TargetAnimationId = 0;
                        entity.AIValues[1] = 0x14;
                        entity.Bytes[0] = 0;
                        return;
                    }
                    entity.Bytes[1] = (byte)(entity.Bytes[1] - 1);
                    _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    uVar4 = (uint)((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x20) >> 0x20;
                    entity.AIValues[1] = 0x78;
                }
                else
                {
                    bVar1 = _gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 3, 3, 0);

                    if (bVar1)
                    {
                        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;

                        if ((int)(((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 5) >> 0x20) == 0)
                        {
                            _gameEngine.SoundManager.PlaySoundEffect(0x94);
                        }

                        goto LAB_8006a258;
                    }

                    if (entity.ForceAdjusted == 0)
                    {
                        return;
                    }

                    if (entity.Bytes[1] == 0)
                    {
                        bVar2 = (byte)(entity.Bytes[0] + 1);
                        entity.Bytes[0] = bVar2;

                        if (bVar2 < 4)
                        {
                            entity.TargetAnimationId = 0;
                            return;
                        }

                        entity.Bytes[0] = 0;
                        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                        uVar4 = (uint)((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x20) >> 0x20;
                        entity.AIValues[1] = 0x78;
                        entity.Bytes[1] = 3;
                    }
                    else
                    {
                        entity.Bytes[1] = (byte)(entity.Bytes[1] - 1);
                        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                        uVar4 = (uint)((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x20) >> 0x20;
                        entity.AIValues[1] = 0x78;
                    }
                }
                entity.TargetDirection = uVar4;
                break;

            case 3:
                uVar4 = (uint)(entity.AIValues[6] + 1);
                entity.AIValues[6] = (short)uVar4;

                if ((uVar4 & 7) == 0)
                {
                    _gameEngine.EffectManager.CreateEffectEntity(
                        (byte)0,
                        _gameEngine.CurrentMap.Info.C, // _gameEngine.CurrentMap.Info.C
                        0,
                        entity.PosX, entity.PosY, entity.FloorHeight);
                }

                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0x1e;
                }
                break;

            case 5:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[3] != 0)
                {
                    entity.TargetAnimationId = 2;
                    entity.Flags = entity.Flags | 0x40;
                    return;
                }

            LAB_8006a258:
                entity.TargetAnimationId = 7;
                uVar4 = (uint)ScriptHelper.GetDirectionToTarget(_gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, _gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                entity.TargetDirection = uVar4;
                entity.AIValues[6] = 0;
                entity.AIValues[7] = 0;
                break;

            case 6:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    uVar4 = entity.TargetDirection;
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0x14;
                    entity.TargetDirection = (uVar4 + 0x10) & 0x1f;
                }
                break;
        }
    }

    //8006b848
    void AI_FUN_8006b848(Entity entity)
    {
        if (entity.PlatformEntity == null)
        {
            return;
        }

        if (entity.Flags2 == 0)
        {
            entity.TargetAnimationId = 0; // idle carried
            return;
        }


        if (entity.Flags2 == -1)
        {
            entity.TargetAnimationId = 3; //begin throw
        }
        else
        {
            //same as g_scriptAnimationTable
            entity.TargetAnimationId = (uint)_gameEngine.StaticVariables.g_scriptAnimationTable2[entity.Flags2];
        }

        entity.PlatformEntity.CarriedEntity = null;
        entity.PlatformEntity = null;
        entity.Flags = (entity.Flags | 0x30) & 0xffffff7f;//turn off bit 8, turn on bits 5 and 6
    }

    //8007a8a0
    public void AI_UpdateIceProjectile(Entity entity)
    {
        if ((entity.FrameCounter & 0x3) == 0)
        {
            var offsetZ = 0;
            byte spriteTableIndex = 0x12;

            if (entity.SpriteTableIndex - 0xb < 2)
            {
                spriteTableIndex = 0x11;
                offsetZ = 0x80000; // << 19
            }

            var spriteEffect = _gameEngine.EffectManager.CreateEffectEntity(
                0, spriteTableIndex, 0,
                entity.PosX,
                entity.PosY,
                entity.PosZ + offsetZ);

            if (spriteEffect != null)
            {
                spriteEffect.ForceX = -entity.ForceX;
                spriteEffect.ForceY = -entity.ForceY;
                spriteEffect.ForceZ = -entity.ForceZ;
            }
        }
    }

    //8007b7b0
    //etick_17_jarsandboxes_Handler
    public void AI_FUN_8007b7b0(Entity entity)
    {
        Debugger.Break();

        if (entity.PlatformEntity == null)
        {
            return;
        }

        if (entity.Flags2 == 0)
        {
            entity.TargetAnimationId = (int)PlayerAnimation.Idle;
            return;
        }

        if (entity.Flags2 == -1)
        {
            entity.TargetAnimationId = (int)PlayerAnimation.Sprint;
        }
        else
        {
            //same as g_scriptAnimationTable
            entity.TargetAnimationId = (uint)_gameEngine.StaticVariables.g_scriptAnimationTable3[entity.Flags2];
        }

        entity.PlatformEntity = null;
        entity.Flags = (entity.Flags | 0x34) & 0xffffff7f;//turn off bit 8, turn on bits 5 and 6
    }

    //8007b998
    public void AI_ProcessWarpTransitionState(Entity entity)
    {
        int state = ReadWarpState(entity);     // lw 0x274(a0)
        uint idx = (uint)(state - 1);          // v1 = state-1; if (idx>=6) return
        if (idx >= 6)
        {
            return;
        }

        string arg1;
        string arg2;

        switch (entity.Bytes[0])
        {
            case 1:

                if (_gameEngine.IsDialogInProgress())
                {
                    ResetWarpState(entity);
                    return;
                }

                arg1 = _gameEngine.EtcRes.GetEtcString(0x40);
                _gameEngine.UIManager.InitializeDialogMessage(arg1, 1);
                _gameEngine.SetEtcAnimationMode(4);

            LABEL_WaitBeforeNextWarpStep:
                entity.DelayOrAngle = 0x3C;
                WriteWarpState(entity, state + 1);
                break;

            case 2:
                // countdown: if (--DelayOrAngle != -1) return
                entity.DelayOrAngle -= 1;
                if (entity.DelayOrAngle != -1)
                {
                    return;
                }

                // g_warpStatusFlag = 0; lancer une "opération asynchrone" (2 textes ETC)
                _gameEngine.StaticVariables.g_warpStatusFlag = 0;

                arg1 = _gameEngine.EtcRes.GetEtcString(0x41);
                arg2 = _gameEngine.EtcRes.GetEtcString(0x42);

                int r = _gameEngine.InitializeAsyncOperation(arg1, arg2, result => _gameEngine.StaticVariables.g_warpStatusFlag = (uint)result);

                // Si terminé immédiatement, on saute un état (state+=1) puis on avancera encore (state+=1) => skip vers case 4
                if (r != 0)
                {
                    WriteWarpState(entity, state + 1);
                    state++;
                }

                // Avance d’un état (vers 3 si non terminé, vers 4 si terminé de suite)
                WriteWarpState(entity, state + 1);
                return;

            case 4:
                {
                    // si pas encore de statut, on attend
                    if (_gameEngine.StaticVariables.g_warpStatusFlag == 0)
                    {
                        return;
                    }

                    // On essaye d’activer le "TextHold" (mise en pause du texte)
                    _gameEngine.UIManager.TryActivateTextHoldState();

                    // si statut != 1 => Reset; sinon on attend 0x3C frames et on avance
                    if (_gameEngine.StaticVariables.g_warpStatusFlag != 1)
                    {
                        ResetWarpState(entity);
                        return;
                    }

                    // WaitBeforeNextWarpStep: timer=0x3C; state++
                    entity.DelayOrAngle = 0x3C;
                    WriteWarpState(entity, state + 1);
                    return;
                }

            case 5:
                {
                    entity.DelayOrAngle -= 1;

                    if (entity.DelayOrAngle != -1)
                    {
                        return;
                    }

                    _gameEngine.UpdateSavedData();
                    WriteWarpState(entity, 6);
                    return;
                }

            // case 6
            case 6:
                {
                    if (_gameEngine.StaticVariables.g_globalTransitionState != 0)
                    {
                        return;
                    }

                    ResetWarpState(entity);
                    return;
                }
        }
    }

    private void ResetWarpState(Entity entity)
    {
        WriteWarpState(entity, 0);
        _gameEngine.StaticVariables.g_playerControlFlags &= 0xfffffffb;
    }

    private int ReadWarpState(Entity e)
        => e.Bytes[0]
           | (e.Bytes[1] << 8)
           | (e.Bytes[2] << 16)
           | (e.Bytes[3] << 24);

    private void WriteWarpState(Entity e, int v)
    {
        e.Bytes[0] = (byte)(v & 0xFF);
        e.Bytes[1] = (byte)((v >> 8) & 0xFF);
        e.Bytes[2] = (byte)((v >> 16) & 0xFF);
        e.Bytes[3] = (byte)((v >> 24) & 0xFF);
    }

    //80061eb8
    public void AI_SpawnWarpIfValid(Entity entity)
    {
        int[] relativePositions = new int[6];

        Entity parentEntity = entity.ParentEntity;
        var aiState = entity.TargetAnimationId;

        if (aiState == 0x9)
        {
            return;
        }

        var t = aiState - 0xA;
        bool inSpecialRange = t < 6;

        if (inSpecialRange)
        {
            if (parentEntity.Hp == 0)
            {
                _gameEngine.DestroyEntity(entity, -1);
                return;
            }

            if (entity.IsAboveGround != 0)
            {
                _gameEngine.SoundManager.PlaySoundEffect(0x19D);
                entity.Flags |= 0x40;
                entity.TargetAnimationId = 0x9;
                _gameEngine.TriggerScreenEffect(unchecked(0x60000000), 2, 0, 1);
                return;
            }

            if (entity.Bytes[0] == 0x1 && aiState == 0x0F)
            {
                int parentZ = parentEntity.PosZ;
                int selfZ = entity.PosZ;

                if (selfZ >= parentZ + 0x01000000)
                {
                    const int MUL = 0x7D2B89DD;
                    const int ADD = unchecked((int)0xE06A02E7);

                    var gSeed = _gameEngine.StaticVariables.g_gameRandomSeed;
                    var seed1 = gSeed * MUL;
                    seed1 = (uint)(seed1 + ADD);

                    var rand1 = seed1 * MUL;
                    var hi18 = (seed1 * 0x18) >> 32;
                    var lo18 = seed1 *  0x18;

                    rand1 = (uint)(rand1 + ADD);
                    var seed1b = rand1 * MUL;

                    var hi7 = (rand1 * 0x7) >> 32;
                    var lo7 = rand1 * 0x7;

                    var rand2 = (rand1 + ADD);
                    var t6 = rand2 * MUL;

                    var hi16 = (rand2 * 0x10) >> 32;
                    var lo16 = rand2 * 0x10;

                    var t0 = (t6 + ADD);

                    entity.TargetAnimationId = 0x0A;
                    entity.Flags |= 0x100;

                    _gameEngine.StaticVariables.g_gameRandomSeed = rand1;
                    _gameEngine.StaticVariables.g_gameRandomSeed = (uint)t0;

                    var a2 = hi18;
                    var tmp = (a2 << 1) + a2;
                    tmp <<= 0x12;

                    var v0 = (hi7 << 0x10) + 0x1E000000;
                    var posX = unchecked(tmp + v0);
                    entity.PosX = (int)posX;

                    var t5 = (t0 * 0x5) >> 32;
                    var posYBase = (t5 << 0x13);
                    var v0y = (hi16 << 0x10) + 0x02800000;
                    var posY = unchecked(posYBase + v0y);
                    entity.PosY = (int)posY;
                }
            }

            if (entity.Bytes[0] != 0x2)
            {
                return;
            }

            {
                var idx = entity.TargetDirection;
                short offX = _gameEngine.StaticVariables.g_offsetXList[idx];
                int scale = entity.Bytes[1] << 11; // * 0x800
                int val = offX * scale;

                const int MAGIC = unchecked(0x2E8BA2E9);
                int hi = (val * MAGIC) >> 32;
                int v0 = (hi >> 1) - (val >> 31);
                entity.PreviousAdjustedForceX = v0;

                short offY = _gameEngine.StaticVariables.g_offsetYList[idx];
                int valY = offY * scale;
                int hiY = (valY * MAGIC) >> 32;
                int v0Y = (hiY >> 1) - (valY >> 31);
                entity.PreviousAdjustedForceY = v0Y;
                return;
            }
        }

        // --------------------------------------------------------------------
        // LAB_80062164 : “cas normal” (gros state machine switch sur 0x88)
        // --------------------------------------------------------------------
        ScriptHelper.CalculateEntityRelativePosition(entity, _gameEngine.StaticVariables.PlayerEntity, relativePositions);
        int slotIndex = entity.Bytes[0];
        WarpSlotState warpSlot = _gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[slotIndex];

        if (parentEntity.Hp == 0)
        {
            entity.TargetAnimationId = 0x7;
            entity.Flags |= 0x40;

            if (warpSlot.Phase != 0)
            {
                return;
            }

            _gameEngine.StaticVariables.g_entitySlots[0].PosX = entity.PosX;
            _gameEngine.StaticVariables.g_entitySlots[0].PosY = entity.PosY;
            _gameEngine.StaticVariables.g_entitySlots[0].TargetAnimationId = 0;
            _gameEngine.StaticVariables.g_playerControlFlags = (uint)(_gameEngine.StaticVariables.g_playerControlFlags & ~0x20);

            warpSlot.Phase = 0;
            return;
        }

        if (warpSlot.Phase == 0x2)
        {
            Entity player = _gameEngine.StaticVariables.g_entitySlots[0];

            if (player.IsAboveGround != 0)
            {
                if (player.TargetAnimationId == 0x1C)
                {
                    if (player.ForceResetAnimationFlag != 0)
                    {
                        // if entity->0x276 == 0: start scrolling lock + spawn warp
                        if (entity.Bytes[2] == 0)
                        {
                            _gameEngine.StaticVariables.g_scrollingParameters.Flag = 1;
                            _gameEngine.StaticVariables.g_scrollingParameters.SpeedX = 1;
                            _gameEngine.StaticVariables.g_scrollingParameters.SpeedY = 1;
                            _gameEngine.StaticVariables.g_scrollingParameters.LimitX = 2; // d’après rand1=2 ici
                            _gameEngine.StaticVariables.g_scrollingParameters.LimitY = 2;

                            var a2 = entity.SpriteTableIndex - 0x100;
                            int x = player.PosX;
                            int y = player.PosY;
                            int z = player.PosZ;

                            Entity spawned = _gameEngine.SpawnWarpEntity(entity, 1, a2, x, y, z, 0);

                            _gameEngine.StaticVariables.g_entitySpawned = spawned;

                            spawned.TargetAnimationId = 0x11;
                            spawned.SpriteProgramIndexes[2] = 0;
                            spawned.Flags |= 0x2;
                            spawned.Flags = (uint)(spawned.Flags & ~0x80);

                            player.AnimFlags = player.AnimFlags & ~0x40;
                            entity.Bytes[2] = 1;
                        }
                        else
                        {
                            if (player.DamagedTickCounter != 0)
                            {
                                if (_gameEngine.StaticVariables.g_entitySpawned != null)
                                {
                                    _gameEngine.DestroyEntity(_gameEngine.StaticVariables.g_entitySpawned, 0);
                                }

                                entity.Bytes[2] = 0;
                                player.TargetAnimationId = 0x39;
                                player.TargetDirection = 0x10;
                            }
                        }
                    }
                }
                else if (player.TargetAnimationId == 0x4E)
                {
                    if (player.ForceResetAnimationFlag != 0)
                    {
                        _gameEngine.StaticVariables.g_scrollingParameters.Flag = 0;
                        _gameEngine.StaticVariables.g_playerControlFlags = (uint)(_gameEngine.StaticVariables.g_playerControlFlags & ~0x20);

                        if (player.Hp != 0)
                        {
                            player.TargetAnimationId = 0;
                        }

                        warpSlot.Phase = 0;
                    }
                }
            }
        }

        if (entity.Bytes[1] == 0)
        {
            int baseX = _gameEngine.StaticVariables.INT_ARRAY_80026cdc[slotIndex];
            warpSlot.BaseY = 0x02A00000;
            warpSlot.BaseX = baseX;
            entity.Bytes[1] = 1;
        }

        // switch(entity->0x88) : énorme state machine
        // Pour rester fidèle au dump, je garde les “cases” structurés comme le MIPS.
        // NB: Beaucoup de cases convergent vers le même épilogue (caseD_1).
        switch (entity.TargetAnimationId)
        {
            case 0x0:
                {
                    // timer 0x282--
                    var w = entity.AIValues[1];

                    if (w != 0)
                    {
                        entity.AIValues[1] = (short)(w - 1);
                    }

                    int add = _gameEngine.StaticVariables.INT_ARRAY_80026cdc[slotIndex + 2];
                    entity.DelayOrAngle = (entity.DelayOrAngle + add) & 0x1FF;

                    short s = _gameEngine.StaticVariables.g_sinus[entity.DelayOrAngle];
                    int sinTerm = ((s * 9) << 9);
                    entity.PosX = warpSlot.BaseX + sinTerm;

                    short c = _gameEngine.StaticVariables.g_cosinus[entity.DelayOrAngle];
                    int cosTerm = ((c * 9) << 9);
                    entity.PosY = warpSlot.BaseY + cosTerm;

                    warpSlot.SavedX = entity.PosX;
                    warpSlot.SavedY = entity.PosY;

                    if ((_gameEngine.StaticVariables.g_globalFlags[0] & 0x2) != 0)
                    {
                        if (relativePositions[1] < 3 && relativePositions[0] < 3 && entity.AIValues[1] == 0)
                        {
                            int other = slotIndex ^ 1;
                            if (_gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[other].Phase == 0)
                            {
                                entity.TargetAnimationId = 0x10;
                                warpSlot.PlayerX = _gameEngine.StaticVariables.g_entitySlots[0].PosX;
                                warpSlot.PlayerY = _gameEngine.StaticVariables.g_entitySlots[0].PosY;
                            }
                        }
                    }
                    break;
                }

            // --------------------------------------------------------------------
            // case 2 (80062798...)  clamp + si fini -> recalc base & timer
            // --------------------------------------------------------------------
            case 2:
                {
                    // if (PosZ < 0x08000000) warpMoveMode=0
                    // (ton ASM fait un test via add + sltu, résultat: si <= seuil -> b8=0)
                    // Je traduis littéralement “si magnitude dans une fenêtre => b8=0”
                    // -> garde ton exact si tu retrouves le seuil.
                    // Ici je colle l’effet visible:
                    if ((uint)(entity.PosZ + 0xFF200000u) <= 0x08000000u)
                    {
                        entity.ForceZ = 0;
                    }

                    // clamp stepX si abs(targetX - SavedX) > 0xB000FFFF
                    {
                        int dx = entity.PosX - warpSlot.SavedX;
                        int adx = Math.Abs(dx);
                        if (adx > 0xB000FFFFu)
                        {
                            warpSlot.A0 = 0;
                        }
                    }

                    // clamp stepY si abs(targetY - SavedY) > 0x7000FFFF
                    {
                        int dy = entity.PosY - warpSlot.SavedY;
                        int ady = Math.Abs(dy);
                        if (ady > (int)0x7000FFFFu)
                        {
                            warpSlot.A1 = 0;
                        }
                    }

                    // si ForceZ==0 et A0==0 et A1==0 => timer=0x78, recalc bases, warpSubState=0
                    if (entity.ForceZ == 0 && warpSlot.A0 == 0 && warpSlot.A1 == 0)
                    {
                        entity.AIValues[1] = 0x78;

                        // baseX = INT_ARRAY_80026cdc[s4] + (targetX - SavedX) + SavedX? (ton ASM fait un mélange)
                        // Je retranscris l’ASM:
                        // slot->BaseX = INT_ARRAY_80026cdc[s4] + (entity.targetX - slot->SavedX) + slot->SavedX;
                        // => en réalité: slot->BaseX = INT_ARRAY_80026cdc[s4] + entity.targetX
                        // mais je garde l’intention originale:
                        warpSlot.BaseX = _gameEngine.StaticVariables.INT_ARRAY_80026cdc[aiState] + (entity.PosX - warpSlot.SavedX) + warpSlot.SavedX;
                        warpSlot.BaseY = (entity.PosY - warpSlot.SavedY) + (int)0x2A000000u + warpSlot.SavedY;
                        entity.TargetAnimationId = 0;
                    }

                    goto default;
                }

            case 3:
                {
                    var timer = entity.AIValues[1];

                    if (timer != 0)
                    {
                        timer--;
                        entity.AIValues[1] = timer;
                        if (timer != 0)
                        {
                            break;
                        }
                    }

                    entity.TargetAnimationId = 0x2;
                    Debugger.Break();

                    // ... le dump continue avec LAB_800629c8 etc.
                    // (le reste suit la même logique; si tu veux, je te génère aussi la suite
                    //  en gardant exactement tous les cases 0x2..0xF.)


                    entity.AIValues[1] = 0x14;
                    entity.ForceZ = 0;
                    warpSlot.A1 = 0;
                    warpSlot.A0 = 0;
                    goto default;
                }

            // --------------------------------------------------------------------
            // case 4 (80062890...) (similaire à case 2, mais autre seuil et peut déclencher une init joueur/slot0)
            // --------------------------------------------------------------------
            case 4:
                {
                    // si PosZ <= 0x18F00000? => warpMoveMode=0  (effet visible identique)
                    if ((uint)entity.PosZ <= 0x18F00000u)
                    {
                        entity.ForceZ = 0;
                    }

                    // clamp comme case 2
                    {
                        int dx = entity.PosX - warpSlot.BaseX;
                        if (Math.Abs(dx) > 0xB000FFFFu)
                        {
                            warpSlot.A0 = 0;
                        }
                    }
                    {
                        int dy = entity.PosY - warpSlot.BaseY;
                        if (Math.Abs(dy) > (int)0x7000FFFFu)
                        {
                            warpSlot.A1 = 0;
                        }
                    }

                    // si ForceZ==0 et A0==0 et A1==0:
                    // -> ici ton ASM déclenche une transition: flags, anim, copie positions dans g_entitySlots[0], etc.
                    if (entity.ForceZ == 0 && warpSlot.A0 == 0 && warpSlot.A1 == 0)
                    {
                        // 80062934.. : set warpSubState=5, set flag bit0 dans Flags, IsActive=2, init entitySlots[0]
                        entity.TargetAnimationId = 5;
                        entity.Flags |= 1u;
                        warpSlot.Phase = 2;

                        _gameEngine.StaticVariables.g_entitySlots[0].TargetAnimationId = 0x31;
                        _gameEngine.StaticVariables.g_entitySlots[0].TargetDirection = 0;
                        _gameEngine.StaticVariables.g_entitySlots[0].Flags |= 0x108u;
                        _gameEngine.StaticVariables.g_entitySlots[0].PosX = entity.PosX;
                        _gameEngine.StaticVariables.g_entitySlots[0].PosY = entity.PosY;
                    }

                    // dans tous les cas, case 4 finit par:
                    // g_entitySlots[0].PreviousAdjustedForceX = slot->A0; PreviousAdjustedForceY = slot->A1; g_entitySlots[0].ForceZ = parent->ForceZ
                    _gameEngine.StaticVariables.g_entitySlots[0].PreviousAdjustedForceX = warpSlot.A0;
                    _gameEngine.StaticVariables.g_entitySlots[0].PreviousAdjustedForceY = warpSlot.A1;
                    // “b8” ici, dans ton ASM, c’est écrit aussi dans g_entitySlots[0] (offset +0xB8). On ne l’a pas typé;
                    // tu peux ajouter un champ si tu veux.
                    goto default;
                }

            // --------------------------------------------------------------------
            // case 5 (800629b0...) => warpSubState=2, warpMoveMode=0xFFFC0000, step >>4
            // --------------------------------------------------------------------
            case 5:
                {
                    if (entity.ForceResetAnimationFlag == 0)
                    {
                        goto default;
                    }

                    entity.TargetAnimationId = 2;
                    entity.ForceZ = unchecked((int)0xFFFC0000u);

                LAB_800629c8:
                    // stepX = (SavedX - targetX + 0xF) >> 4  (ASM: rand2=SavedX - targetX)
                    {
                        int dx = warpSlot.SavedX - entity.PosX;
                        if (dx < 0)
                        {
                            dx += 0xF;
                        }

                        warpSlot.A0 = dx >> 4;
                    }
                    {
                        int dy = warpSlot.SavedY - entity.PosY;
                        if (dy < 0)
                        {
                            dy += 0xF;
                        }

                        warpSlot.A1 = dy >> 4;
                    }

                    goto default;
                }

            // --------------------------------------------------------------------
            // case 6 (80062a18...) => reset steps, puis branche selon Bytes[3]
            // --------------------------------------------------------------------
            case 6:
                {
                    warpSlot.A1 = 0;
                    warpSlot.A0 = 0;

                    if (entity.ForceResetAnimationFlag == 0)
                    {
                        goto default;
                    }

                    if (entity.Bytes[3] != 0)
                    {
                        entity.TargetAnimationId = 7;
                        entity.Flags |= 0x40u;

                        if (aiState == 0)
                        {
                            entity.DelayOrAngle = 0x708;
                        }
                        else
                        {
                            entity.ItemState = 0x708;
                        }

                        goto default;
                    }

                    {
                        int dx = warpSlot.SavedX - entity.PosX;
                        if (dx < 0)
                        {
                            dx += 0xF;
                        }

                        warpSlot.A0 = dx >> 4;
                    }
                    {
                        int dy = warpSlot.SavedY - entity.PosY;
                        if (dy < 0)
                        {
                            dy += 0xF;
                        }

                        warpSlot.A1 = dy >> 4;
                    }

                    // tests magnitude contre 0xDFFFF... puis 0xE7FFFF... (exact ASM)
                    if ((uint)entity.PosZ <= 0xDFFFFFFFu)
                    {
                        entity.TargetAnimationId = 2;
                        entity.ForceZ = 0x00020000;
                    }
                    else if ((uint)entity.PosZ <= 0xE7FFFFFF)
                    {
                        entity.ForceZ = unchecked((int)0xFFFC0000);
                    }

                    goto default;
                }

            // --------------------------------------------------------------------
            // case 8 (80062af8...) => recalc X/Y depuis sin/cos, magnitude=0xE0000000, si ForceResetAnimationFlag==0 reset state
            // --------------------------------------------------------------------
            case 8:
                {
                    int angle = (int)entity.DelayOrAngle & 0x1FF;

                    {
                        int sinv = (int)_gameEngine.StaticVariables.g_sinus[angle];
                        int tmp = (sinv << 3) + sinv;
                        tmp <<= 9;
                        entity.PosX = warpSlot.BaseX + tmp;
                    }

                    entity.PosZ = unchecked((int)0xE0000000);

                    {
                        int cosv = (int)_gameEngine.StaticVariables.g_cosinus[angle];
                        int tmp = (cosv << 3) + cosv;
                        tmp <<= 9;
                        entity.PosY = warpSlot.BaseY + tmp;
                    }

                    if (entity.ForceResetAnimationFlag != 0)
                    {
                        entity.TargetAnimationId = 0; // ton ASM fait sw zero,0x88(s2) après le beq
                    }

                    goto default;
                }

            case 0x10:
            {
                if (entity.ForceResetAnimationFlag == 0)
                {
                    break;
                }

                entity.TargetAnimationId = 0x3;

                // warpSlot.A0/A1 = (playerPos - curPos + 0xF) >> 4
                int dx = warpSlot.PlayerX - entity.PosX;
                if (dx < 0)
                {
                    dx += 0xF;
                }

                warpSlot.A0 = dx >> 4;

                int dy = warpSlot.PlayerY - entity.PosY;
                if (dy < 0)
                {
                    dy += 0xF;
                }

                warpSlot.A1 = dy >> 4;

                goto default; // converger vers l’épilogue commun
            }

            default:
                {
                    if (warpSlot.A0 != 0)
                    {
                        entity.PreviousAdjustedForceX = warpSlot.A0;
                    }

                    if (warpSlot.A1 != 0)
                    {
                        entity.PreviousAdjustedForceY = warpSlot.A1;
                    }

                    break;
                }
        }

        // writeback du slot (struct en valeur si c’est un struct)
        _gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[slotIndex] = warpSlot;
        return;

        //LAB_80062760:
        //// 80062760..94: recharge timer=0x14, warpMoveMode=0, A0/A1=0
        //// condition sur phaseLatch_140 + PosZ (je garde l’effet final, à toi d’ajouter le test exact)
        //entity.AIValues[1] = 0x14;
        //entity.ForceZ = 0;
        //warpSlot.A1 = 0;
        //warpSlot.A0 = 0;
        //goto default;

    }

    //80061d14
    public void AI_Melzas2_FinalBoss(Entity entity)
    {
        AI_Melzas2.AI_Melzas2_FinalBoss(_gameEngine, entity);
    }

    //80062bc0
    public void AI_UpdateMelzas2CutsceneChannels(Entity entity)
    {
        AI_Melzas2.AI_UpdateMelzas2CutsceneChannels(_gameEngine, entity);
    }

    //800637d8
    public void AI_UpdateEntityAI_IdleLookAround(Entity entity)
    {
        ulong rand;
        uint direction;
        ushort uVar1;
        int verticalDelta;
        int[] relativePositions = new int[6];
        ulong seed;

        ScriptHelper.CalculateEntityRelativePosition(entity, _gameEngine.StaticVariables.PlayerEntity, relativePositions);

        if (entity.TargetAnimationId != 2)
        {
            verticalDelta = entity.ModdedPosX - _gameEngine.StaticVariables.PlayerEntity.HitBoxX;

            if (verticalDelta < 0)
            {
                if (entity.Width + 1 <= _gameEngine.StaticVariables.PlayerEntity.HitBoxX - entity.ModdedPosX)
                {
                    goto ExitIdleCheck;
                }
            }
            else if (_gameEngine.StaticVariables.PlayerEntity.CollisionWidth + 1 <= verticalDelta)
            {
                goto ExitIdleCheck;
            }

            verticalDelta = entity.ModdedPosY - _gameEngine.StaticVariables.PlayerEntity.HitBoxY;

            if (verticalDelta < 0)
            {
                if (entity.Height + 1 <= _gameEngine.StaticVariables.PlayerEntity.HitBoxY - entity.ModdedPosY)
                {
                    goto ExitIdleCheck;
                }
            }
            else if (_gameEngine.StaticVariables.PlayerEntity.CollisionDepth + 1 <= verticalDelta)
            {
                goto ExitIdleCheck;
            }

            verticalDelta = entity.ModdedPosZ - _gameEngine.StaticVariables.PlayerEntity.HitBoxZ;

            /*if (verticalDelta < 0)
            {
                if (_gameEngine.StaticVariables.PlayerEntity.HitBoxZ - entity.ModdedPosZ < entity.Depth + 1)
                {
                    //goto TriggerLookAround;
                }
            }
            else*/
            if ((verticalDelta < 0 && _gameEngine.StaticVariables.PlayerEntity.HitBoxZ - entity.ModdedPosZ < entity.Depth + 1)
                || verticalDelta < _gameEngine.StaticVariables.PlayerEntity.CollisionHeight + 1)
            {
            TriggerLookAround:
                _gameEngine.SoundManager.PlaySoundEffect(0x1d6);
                entity.Bytes[0] = 2;
                entity.TargetAnimationId = 2;
                direction = (uint)ScriptHelper.GetDirectionToTarget(
                    entity.PosX - _gameEngine.StaticVariables.PlayerEntity.PosX,
                    entity.PosY - _gameEngine.StaticVariables.PlayerEntity.PosY);
                entity.TargetDirection = direction;
                entity.AIValues[1] = 600;
                return;
            }
        }

    ExitIdleCheck:
        rand = (ulong)(entity.AIValues[4] - 1);

        if (entity.AIValues[4] == 0)
        {
            if (relativePositions[0] < 9 && relativePositions[1] < 9 &&
               (_gameEngine.StaticVariables.g_playerControlFlags & 4U) == 0)
            {
                _gameEngine.SoundManager.PlaySoundEffect(0x1d6);
            }

            if (entity.AIValues[5] == 0)
            {
                _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                seed = _gameEngine.StaticVariables.g_gameRandomSeed;
                entity.AIValues[5] = 1;
                rand = ((seed * 0xb) >> 0x20) + 0x19;
            }
            else
            {
                _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                seed = _gameEngine.StaticVariables.g_gameRandomSeed;
                entity.AIValues[5] = 0;
                rand = ((seed * 0x3d) >> 0x20) + 100;
            }
        }

        entity.AIValues[4] = (short)rand;
        direction = entity.TargetAnimationId;

        if ((int)direction < 3)
        {
            if ((int)direction < 1)
            {
                if (direction == 0)
                {
                    if (entity.AIValues[1] == 0)
                    {
                        entity.Bytes[0] = 1;
                        entity.AIValues[1] = 0x78;
                        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                        entity.TargetAnimationId = 1;

                        if ((uint)((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 3 >> 0x20) == 0)
                        {
                            _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                            entity.TargetDirection = (uint)(((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x20) >> 0x20);
                        }
                    }
                    else
                    {
                        entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                    }
                }
            }
            else
            {
                rand = (ulong)entity.AIValues[1];
                uVar1 = (ushort)(rand - 1);
                entity.AIValues[1] = (short)uVar1;

                if (rand == 1)
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0x50;
                }
                else
                {
                    if ((uVar1 & 0x3f) == 0)
                    {
                        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;

                        if ((uint)((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 3) >> 0x20 == 0)
                        {
                            _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                            entity.TargetDirection += (uint)((((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0xd >> 0x20) - 6) & 0x1f);
                        }
                    }

                    if (entity.ForceAdjusted != 0)
                    {
                        entity.Bytes[0] = (byte)entity.TargetAnimationId;
                        _gameEngine.EntityGameplayManager.UpdateDirectionForced(entity, entity.Bytes[0], 3, 0x200000);

                        if (entity.TargetAnimationId == entity.Bytes[0])
                        {
                            _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;

                            if (((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 2) >> 0x20 != 0)
                            {
                                entity.TargetDirection = (entity.TargetDirection + 0x10) & 0x1f;
                            }
                        }
                        else
                        {
                            _gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, entity.Bytes[0], 0x400000);
                        }
                    }
                }
            }
        }
        else if (direction == 3 && entity.IsAboveGround != 0)
        {
            entity.TargetAnimationId = entity.Bytes[0];
        }
    }

    //80063cb4
    public void AI_UpdateEntityTriggerWarpBehavior(Entity entity)
    {
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, _gameEngine.StaticVariables.PlayerEntity, relativePositions);

        if (entity.TargetAnimationId == 0)
        {
            if (relativePositions[0] < 4 && relativePositions[1] < 4 && relativePositions[5] < 1)
            {
                if ((_gameEngine.StaticVariables.g_playerControlFlags & 4U) == 0)
                {
                    _gameEngine.SoundManager.PlaySoundEffect(0x1d4);
                }
                _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                entity.TargetAnimationId = 1;
                entity.TargetDirection = (uint)((((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 7) >> 0x20) + 5);
            }
        }
        else if (0x3bfffff < entity.PosZ)
        {
            _gameEngine.DestroyEntity(entity);
        }
    }

    //80063db4
    public void AI_UpdateEntityAI_IdleCurious(Entity entity)
    {
        byte remainingCycles;
        short rand;
        int deltaX;
        uint currentAnimId;
        int deltaY;
        int[] relativePos = new int[6];
        ulong seed;

        ScriptHelper.CalculateEntityRelativePosition(entity, _gameEngine.StaticVariables.PlayerEntity, relativePos);

        if ((int)entity.TargetAnimationId < 10)
        {
            entity.TargetAnimationId = 10;
            entity.Flags = entity.Flags & 0xfff8ff7fU | 0x10000;
            entity.ItemState = entity.PosY;
            entity.DelayOrAngle = entity.PosX;
        }
        else
        {
            if (entity.TargetAnimationId - 10 < 2)
            {
                rand = (short)(entity.AIValues[4] + -1);

                if (entity.AIValues[4] == 0)
                {
                    if (relativePos[0] < 9 && relativePos[1] < 9 && (_gameEngine.StaticVariables.g_playerControlFlags & 4U) == 0)
                    {
                        _gameEngine.SoundManager.PlaySoundEffect(0x192);
                    }

                    if (entity.AIValues[5] == 0)
                    {
                        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                        seed = _gameEngine.StaticVariables.g_gameRandomSeed;
                        entity.AIValues[5] = 1;
                        rand = (short)((short)(seed * 0xb >> 0x20) + 0xf);
                    }
                    else
                    {
                        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                        seed = _gameEngine.StaticVariables.g_gameRandomSeed;
                        entity.AIValues[5] = 0;
                        rand = (short)((short)(seed * 0x3d >> 0x20) + 100);
                    }
                }

                entity.AIValues[4] = rand;
            }

            currentAnimId = entity.TargetAnimationId;

            if (currentAnimId == 0xb)
            {
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 10;
                    remainingCycles = (byte)(entity.Bytes[0] - 1);
                    entity.Bytes[0] = remainingCycles;

                    if (remainingCycles == 0)
                    {
                        entity.AIValues[1] = 100;
                    }
                    else
                    {
                        entity.AIValues[1] = 0x14;
                    }
                }
            }
            else if ((int)currentAnimId < 0xc)
            {
                if (currentAnimId == 10)
                {
                    if (entity.AIValues[1] == 0)
                    {
                        deltaX = 0;
                        entity.TargetAnimationId = 0xb;

                        if (entity.Bytes[0] == 0)
                        {
                            _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                            entity.Bytes[0] = (byte)(((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 3 >> 0x20) + 3);
                        }
                        else
                        {
                            _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                            deltaX = (int)((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 4 >> 0x20);
                        }

                        if (deltaX == 0)
                        {
                            _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                            entity.TargetDirection = (uint)(((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x20) >> 0x20);
                        }

                        deltaX = entity.PosX - entity.DelayOrAngle;
                        deltaY = entity.PosY - entity.ItemState;

                        if (deltaX < 0)
                        {
                            deltaX = -deltaX;
                        }

                        if (deltaY < 0)
                        {
                            deltaY = -deltaY;
                        }

                        if (deltaX < 0x900000 && deltaY < 0x600000)
                        {
                            return;
                        }

                        deltaX = entity.DelayOrAngle - entity.PosX;
                        deltaY = entity.ItemState - entity.PosY;
                    }
                    else
                    {
                        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;

                        if ((int)((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 5 >> 0x20) == 0 &&
                            0x31 < (ushort)entity.AIValues[1] && (ushort)entity.AIValues[1] < 0x3d &&
                            relativePos[0] < 9 && relativePos[1] < 9)
                        {
                            _gameEngine.SoundManager.PlaySoundEffect(0x192);
                        }

                        entity.AIValues[1] = (short)(entity.AIValues[1] + -1);

                        if (3 < relativePos[0])
                        {
                            return;
                        }

                        if (3 < relativePos[1])
                        {
                            return;
                        }

                        if (3 < relativePos[5])
                        {
                            return;
                        }

                        entity.TargetAnimationId = 0xc;
                        entity.Flags = entity.Flags & 0xfffffeff;
                        deltaX = entity.PosX - _gameEngine.StaticVariables.PlayerEntity.PosX;
                        deltaY = entity.PosY - _gameEngine.StaticVariables.PlayerEntity.PosY;
                    }

                    currentAnimId = (uint)ScriptHelper.GetDirectionToTarget(deltaX, deltaY);
                    entity.TargetDirection = currentAnimId;
                }
            }
            else if (currentAnimId == 0xc)
            {
                if (0x3bfffff < entity.PosZ)
                {
                    entity.TargetAnimationId = 0xd;
                    entity.ForceZ = 0;
                    entity.PosX = entity.DelayOrAngle;
                    entity.PosY = entity.ItemState;
                }
            }
            else if (currentAnimId == 0xd && 0xd < relativePos[0])
            {
                entity.TargetAnimationId = 10;
                entity.PosZ = 0;
                entity.Flags = entity.Flags | 0x100;
            }
        }
    }

    //80065750
    public void AI_FUN_80065750(Entity entity)
    {
        byte bVar1;
        ulong uVar2;
        short sVar3;
        uint uVar4;
        int iVar5;
        int iVar6;
        int iVar7;
        int[] relativePositions = new int[6];

        if (entity.Bytes[1] == 0)
        {
            iVar5 = entity.TileX;
            iVar6 = entity.TileY;
            entity.Bytes[1] = 1;
            uVar4 = entity.Flags;
            entity.AIValues[4] = (short)iVar5;
            entity.AIValues[5] = (short)iVar6;
            entity.Flags = uVar4 & 0xffffdfff | 8;
        }

        ScriptHelper.CalculateEntityRelativePosition(entity, _gameEngine.StaticVariables.PlayerEntity, relativePositions);
        uVar4 = entity.TargetAnimationId;

        if (uVar4 == 1)
        {
            sVar3 = (short)(entity.AIValues[1] + -1);
            entity.AIValues[1] = sVar3;
            if (sVar3 == 0)
            {
                entity.TargetAnimationId = 0;
                entity.AIValues[1] = 0x78;
                entity.Bytes[0] = 0;
            }
            else
            {
                if (entity.Bytes[0] == 0 && relativePositions[2] < 0x200001)
                {
                    if (relativePositions[0] < 3 && relativePositions[1] < 3)
                    {
                        entity.TargetAnimationId = 0;
                        uVar4 = (uint)ScriptHelper.GetDirectionToTarget(
                            _gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                            _gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                        entity.TargetDirection = uVar4;
                        entity.AIValues[1] = 0x50;
                        entity.Bytes[0] = 1;
                        return;
                    }

                    if (relativePositions[0] < 5 && relativePositions[1] < 5)
                    {
                        uVar4 = (uint)ScriptHelper.GetDirectionToTarget(
                            _gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                            _gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                        entity.TargetDirection = uVar4;
                        return;
                    }
                }

                if (entity.ForceAdjusted == 0)
                {
                    iVar5 = _gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 1, 0x100000);

                    if (iVar5 == 0 && (entity.AIValues[1] & 0x3fU) == 0)
                    {
                        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;

                        if ((int)((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 3 >> 0x20) == 0)
                        {
                            _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                            entity.TargetDirection = (uint)(entity.TargetDirection + (int)((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 9 >> 0x20) - 4 & 0x1f);
                        }
                    }
                }
                else
                {
                    _gameEngine.EntityGameplayManager.UpdateDirectionForced(entity, 1, 3, 0x100000);
                }

            }
        }
        else if ((int)uVar4 < 2)
        {
            if (uVar4 == 0)
            {
                sVar3 = entity.AIValues[1];

                if (sVar3 == 0)
                {
                    iVar7 = (int)((uint)(ushort)entity.AIValues[4] - entity.TileX);
                    iVar6 = (int)((uint)(ushort)entity.AIValues[5] - entity.TileY);
                    iVar5 = iVar7;

                    if (iVar7 < 0)
                    {
                        iVar5 = -iVar7;
                    }

                    if (iVar5 < 8)
                    {
                        iVar5 = iVar6;

                        if (iVar6 < 0)
                        {
                            iVar5 = -iVar6;
                        }

                        if (iVar5 < 8)
                        {
                            _gameEngine.EntityGameplayManager.StartFlying(entity, 1, 300, 0x14);
                            return;
                        }
                    }

                    entity.TargetAnimationId = 1;
                    uVar4 = (uint)ScriptHelper.GetDirectionToTarget(iVar7 * 0x180000, iVar6 * 0x100000);
                    _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    uVar2 = _gameEngine.StaticVariables.g_gameRandomSeed;
                    entity.AIValues[1] = 0xf0;
                    entity.TargetDirection = (uint)((uVar4 + (int)(uVar2 * 9 >> 0x20) - 4) & 0x1f);
                }
                else
                {
                    bVar1 = entity.Bytes[0];
                    entity.AIValues[1] = (short)(sVar3 + -1);

                    if (bVar1 != 0 && sVar3 == 0x47 &&
                        (_gameEngine.StaticVariables.g_playerControlFlags & 4U) == 0)
                    {
                        _gameEngine.SoundManager.PlaySoundEffect(0xca);
                    }
                }
            }
        }
        else if (uVar4 == 5 && entity.IsAboveGround != 0)
        {
            entity.TargetAnimationId = 1;
        }
    }


    //8007c174
    //Item
    public void FUN_8007c174(Entity entity)
    {
        int itemState;
        Entity entity2;
        int soundSfxIndex;

        //Debugger.Break();
        var itemId = entity.SpriteTableIndex - 0x1e;

        if (entity.Bytes[0] == 2)
        {
            if (entity.DelayOrAngle != 0)
            {
                entity.DelayOrAngle += -1;
                return;
            }

            itemState = entity.ItemState;

            if (itemState == 1)
            {
            LAB_8007c6f4:
                if (_gameEngine.CdManager.FUN_8005a7d4())
                {
                    return;
                }

                entity.ItemState += 1;
                _gameEngine.SetEtcAnimationMode(3);
                _gameEngine.SoundManager.StopAllSound();
                return;
            }

            if (itemState < 2)
            {
                if (itemState != 0)
                {
                    return;
                }

                entity.ForceZ = 0;

                if (_gameEngine.IsDialogInProgress())
                {
                    return;
                }

                if (entity.AIValues[2] == 0)
                {
                    entity.ItemState += 2;
                    _gameEngine.StaticVariables.g_dropItemTextBuffer = _gameEngine.EtcRes.GetOtherString(0x4e);
                    _gameEngine.StaticVariables.g_dropItemTextBuffer += _gameEngine.EtcRes.GetItemName((int)itemId);
                    _gameEngine.StaticVariables.g_dropItemTextBuffer += _gameEngine.EtcRes.GetOtherString(0x4f);

                LAB_8007c320:
                    _gameEngine.UIManager.InitializeDialogMessage(_gameEngine.StaticVariables.g_dropItemTextBuffer, 1);
                    return;
                }

                _gameEngine.StaticVariables.g_dropItemTextBuffer = _gameEngine.EtcRes.GetOtherString(0x4c);
                _gameEngine.StaticVariables.g_dropItemTextBuffer += _gameEngine.EtcRes.GetItemName((int)itemId);
                _gameEngine.StaticVariables.g_dropItemTextBuffer += _gameEngine.EtcRes.GetOtherString(0x4d);

                if (entity.AIValues[4] == 0)
                {
                    entity.ItemState += 2;
                    Debugger.Break();
                    soundSfxIndex = _gameEngine.StaticVariables.g_itemDropProperties[itemId].SoundSfxIndex; //itemId * 8 + 5

                    if (soundSfxIndex == 0)
                    {
                        _gameEngine.StaticVariables.g_dropItemTextBuffer += _gameEngine.EtcRes.GetOtherString(0x46);
                    }

                    _gameEngine.SoundManager.PlaySoundEffect((uint)soundSfxIndex);
                    //goto LAB_8007c320;
                    _gameEngine.UIManager.InitializeDialogMessage(_gameEngine.StaticVariables.g_dropItemTextBuffer, 1);
                    return;
                }

                entity.ItemState += 1;
                //goto LAB_8007c684;
                _gameEngine.UIManager.InitializeDialogMessage(_gameEngine.StaticVariables.g_dropItemTextBuffer, 1);
                _gameEngine.SetEtcAnimationMode(4);
                return;
            }

            if (itemState != 2)
            {
                return;
            }

            if (_gameEngine.IsDialogInProgress())
            {
                return;
            }

            if (entity.AIValues[2] == 0)
            {
                entity.AIValues[4] = 0;
            }
            else
            {
                _gameEngine.PlayerManager.FUN_80033dbc(_gameEngine.StaticVariables.PlayerEntity, itemId);
                _gameEngine.FUN_80032b28((uint)entity.ContentsGameFlag);
            }

            entity2 = _gameEngine.StaticVariables.g_entitySlots[entity.AIValues[0]];
        }
        else
        {
            if (entity.IsAboveGround != 0 && entity.AIValues[2] != 0)
            {
                var value = (entity.AIValues[2] * 0xc) >> 4;
                entity.AIValues[2] = (short)value;

                if (value <= _gameEngine.CurrentMap.Info.Gravity << 8)
                {
                    entity.AIValues[2] = 0;
                    entity.AIValues[3] = 0;
                }

                entity.ForceZ = entity.AIValues[2];
            }

            var delay = entity.DelayOrAngle + -1;

            if (1 < entity.DelayOrAngle + 1)
            {
                entity.DelayOrAngle = delay;

                if (delay == 0)
                {
                    goto LAB_8007c740;
                }

                if (delay == 0x78)
                {
                    entity.DamagedTickCounter = 0x78;
                }
            }

            itemState = entity.ItemState;

            if (itemState == 1)
            {
                if (_gameEngine.IsDialogInProgress())
                {
                    return;
                }

                var y = entity.ModdedPosX - _gameEngine.StaticVariables.PlayerEntity.ModdedPosX;

                if (y < 0)
                {
                    if (entity.Width + 1 <= _gameEngine.StaticVariables.PlayerEntity.ModdedPosX - entity.ModdedPosX)
                    {
                        return;
                    }
                }
                else if (_gameEngine.StaticVariables.PlayerEntity.Width + 1 <= y)
                {
                    return;
                }

                y = entity.ModdedPosY - _gameEngine.StaticVariables.PlayerEntity.ModdedPosY;

                if (y < 0)
                {
                    if (entity.Height + 1 <= _gameEngine.StaticVariables.PlayerEntity.ModdedPosY - entity.ModdedPosY)
                    {
                        return;
                    }
                }
                else if (_gameEngine.StaticVariables.PlayerEntity.Height + 1 <= y)
                {
                    return;
                }

                y = entity.ModdedPosZ - _gameEngine.StaticVariables.PlayerEntity.ModdedPosZ;

                if (y < 0)
                {
                    if (entity.Depth + 1 <= _gameEngine.StaticVariables.PlayerEntity.ModdedPosZ - entity.ModdedPosZ)
                    {
                        return;
                    }
                }
                else if (_gameEngine.StaticVariables.PlayerEntity.Depth + 1 <= y)
                {
                    return;
                }

                var res = _gameEngine.PlayerManager.FUN_80033f00(_gameEngine.StaticVariables.PlayerEntity, (int)itemId);

                if (res == false && entity.Bytes[0] != 0)
                {
                    _gameEngine.StaticVariables.g_dropItemTextBuffer = string.Empty;
                }
                else
                {
                    _gameEngine.StaticVariables.g_dropItemTextBuffer = _gameEngine.EtcRes.GetItemName((int)itemId);
                    _gameEngine.StaticVariables.g_dropItemTextBuffer += _gameEngine.EtcRes.GetOtherString(0x45);
                }

                //Debugger.Break();
                _gameEngine.FUN_80032b28((uint)entity.ContentsGameFlag); //AIValues[0]
                soundSfxIndex = _gameEngine.StaticVariables.g_itemDropProperties[itemId].SoundSfxIndex; //itemId * 8 + 5

                if (soundSfxIndex != 0)
                {
                    _gameEngine.SoundManager.PlaySoundEffect((uint)soundSfxIndex);

                    if (_gameEngine.StaticVariables.g_dropItemTextBuffer.Length > 0)
                    {
                        _gameEngine.UIManager.InitializeDialogMessage(_gameEngine.StaticVariables.g_dropItemTextBuffer, 0);
                    }
                    entity.ItemState += 2;
                    return;
                }

                entity.ItemState += 1;
                _gameEngine.EntityManager.FUN_8003ad30(entity);
                _gameEngine.SoundManager.LoadBgm(0);
                _gameEngine.StartCdStreaming(0xb);

                if (_gameEngine.StaticVariables.g_dropItemTextBuffer.Length == 0)
                {
                    goto LAB_8007c68c;
                }

            LAB_8007c684:
                _gameEngine.UIManager.InitializeDialogMessage(_gameEngine.StaticVariables.g_dropItemTextBuffer, 1);

            LAB_8007c68c:
                _gameEngine.SetEtcAnimationMode(4);
                return;
            }

            if (itemState < 2)
            {
                if (itemState != 0)
                {
                    return;
                }

                if (0 < entity.ForceZ)
                {
                    return;
                }

                entity.ItemState = 1;
                return;
            }

            if (itemState == 2)
            {
                //goto LAB_8007c6f4;
                if (_gameEngine.CdManager.FUN_8005a7d4())
                {
                    return;
                }

                entity.ItemState += 1;
                _gameEngine.SetEtcAnimationMode(3);
                _gameEngine.SoundManager.StopAllSound();
                return;
            }

            if (itemState != 3)
            {
                return;
            }

            entity2 = entity;

            if (_gameEngine.IsDialogInProgress())
            {
                return;
            }
        }

        _gameEngine.EntityManager.FUN_8003adac(entity2);

    LAB_8007c740:
        _gameEngine.DestroyEntity(entity);
    }

    #endregion

    #region function type D

    //8007df4c
    private void AI_FUN_8007df4c(Entity entity)
    {
        bool bVar1;
        byte val;

        val = entity.TouchingEntity.BalanceAnimValRef.Val;
        var direction = ScriptHelper.GetDirectionToTarget(entity.PosX - _gameEngine.StaticVariables.g_entitySlots[0].PosX, entity.PosY - _gameEngine.StaticVariables.g_entitySlots[0].PosY);
        entity.TargetDirection = (uint)direction;

        if (((val & 0xf) - 1 < 3))
        {
            _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;

            if (0x45 < _gameEngine.StaticVariables.g_gameRandomSeed * 100 >> 0x20)
            {
                if (entity.SpriteTableIndex == 0x152)
                {
                    direction = 6;
                    
                    if (entity.TargetAnimationId < 2)
                    {
                        goto LAB_8007e058;
                    }

                    if (entity.SpriteTableIndex == 0x155)
                    {
                        direction = 6;

                        if (entity.TargetAnimationId < 2)
                        {
                            direction = 6;

                            if (entity.TargetAnimationId == 7)
                            {
                                goto LAB_8007e058;
                            }
                        }
                    }
                }
            }
        }

        bVar1 = _gameEngine.EntityManager.ComputeNewHp(entity);

        if (bVar1)
        {
            entity.Bytes[3] = 1;
        }

        direction = 4;

        LAB_8007e058:
        entity.TargetAnimationId = (uint)direction;
    }

    //8007eb58
    private void AI_FUN_8007eb58(Entity entity)
    {
        if (entity.Hp == 0)
        {
            return;
        }

        if (_gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 1;
    }

    //8007eba8
    private void AI_FUN_8007eba8(Entity entity)
    {
        entity.ForceZ = 0;
        entity.PreviousAdjustedForceY = 0;
        entity.PreviousAdjustedForceX = 0;
        var bVar1 = _gameEngine.EntityManager.ComputeNewHp(entity);

        if (bVar1)
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 6;
    }

    #endregion

    #region function type E

    //8007ed10
    private void Script_Deactivate_FUN_8007ed10(Entity entity)
    {
        _gameEngine.DestroyEntity(entity, -1);
    }

    //8007ed30
    void AI_FUN_8007ed30(Entity entity)
    {
        if ((entity.TargetAnimationId == 2 && entity.ForceResetAnimationFlag == 1)
            || (entity.CombinedVramFlagsAND & 4U) != 0)
        {
            _gameEngine.DestroyEntity(entity, -1);
        }
        else
        {
            entity.TargetAnimationId = 2;
            entity.Flags = (entity.Flags & 0xffffffcfU) | 0x40;
        }
    }

    //8007ef50
    void AI_HandleIceLightHitEffect(Entity entity)
    {
        ulong rand;
        SpriteEffect effectEntity;
        int isSmallSprite;
        byte spriteTableIndex;
        int randomOffset;

        if (entity.TargetAnimationId == (int)PlayerAnimation.Moving)
        {
            if (entity.Slope_18c == 1 || entity.Slope_18c == 4)
            {
                _gameEngine.DestroyEntity(entity, 6);
            }

            if (entity.ForceResetAnimationFlag == 1)
            {
                _gameEngine.DestroyEntity(entity, -1);
            }
            else if ((entity.FrameCounter & 7U) == 0)
            {
                isSmallSprite = entity.SpriteTableIndex - 0xbU < 2 ? 1 : 0;
                spriteTableIndex = 0x12;

                if (isSmallSprite != 0)
                {
                    spriteTableIndex = 0x11;
                }

                effectEntity = _gameEngine.EffectManager.CreateEffectEntity(
                    0, spriteTableIndex, 0,
                    entity.PosX, entity.PosY, entity.PosZ + isSmallSprite * 0x80000);

                if (effectEntity != null)
                {
                    isSmallSprite = (int)((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7);
                    _gameEngine.StaticVariables.g_gameRandomSeed = (uint)(isSmallSprite * 0x7d2b89dd + 0xe06a02e7);
                    rand = _gameEngine.StaticVariables.g_gameRandomSeed;
                    randomOffset = (isSmallSprite * 0x30001) >> 0x20;

                    effectEntity.ForceZ = 0x20000;
                    effectEntity.ForceX = randomOffset + -0x18000;
                    effectEntity.ForceY = (int)((rand * 0x20001) >> 0x20) + -0x10000;
                }
            }
        }
        else
        {
            var loopCounter = 0;
            spriteTableIndex = 0x12;

            if (entity.SpriteTableIndex - 0xbU < 2)
            {
                spriteTableIndex = 0x11;
            }

            if (entity.Slope_18c == 1 || entity.Slope_18c == 4)
            {
                _gameEngine.DestroyEntity(entity, 6);
            }
            else
            {
                do
                {
                    effectEntity = _gameEngine.EffectManager.CreateEffectEntity(
                        0, spriteTableIndex, 0,
                        entity.PosX, entity.PosY, entity.PosZ);

                    if (effectEntity != null)
                    {
                        isSmallSprite = (int)((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7);
                        _gameEngine.StaticVariables.g_gameRandomSeed = (uint)(isSmallSprite * 0x7d2b89dd + 0xe06a02e7);
                        rand = _gameEngine.StaticVariables.g_gameRandomSeed;
                        randomOffset = (isSmallSprite * 0x60001) >> 0x20;

                        effectEntity.ForceX = randomOffset + -0x30000;
                        effectEntity.ForceY = (int)((rand * 0x40001) >> 0x20) + -0x20000;
                    }

                    loopCounter += 1;

                } while (loopCounter < 5);

                entity.TargetAnimationId = 1;
                entity.Flags = (entity.Flags & 0xffffffcfU) | 0x40;
            }
        }
    }

    //8007f420
    void AI_UpdateArrows(Entity entity)
    {
        int entityTargetIndex = 0;

        if (entity.TargetAnimationId == 1) //after hit a wall (turn around)
        {
            if (entity.ForceResetAnimationFlag == 1)
            {
                _gameEngine.DestroyEntity(entity, -1);
            }
        }
        else
        {
            if (entity.HitCounter != 0)
            {
                var flags = entity.Flags;
                var collisionMask = (flags & 2) << 2;

                if ((flags & 0x4) != 0)
                {
                    collisionMask |= 1;
                }

                if ((flags & 0x1000) != 0)
                {
                    collisionMask |= 0x800;
                }

                var noValidCollisionFound = true;

                if (collisionMask != 0)
                {
                    var entityIndex = 0;

                    if (-1 < _gameEngine.StaticVariables.g_numberOfEntities)
                    {
                        do
                        {
                            var entityTarget = _gameEngine.StaticVariables.g_entitySlots[entityTargetIndex];

                            if (entity != entityTarget
                                && (entityTarget.AnimFlags & 0x40) == 0
                                && entityTarget.BalanceRecord.Values[5] == 0
                                && (entityTarget.Flags & collisionMask) != 0)
                            {
                                var withinX = entity.HitBoxX - entityTarget.HitBoxOriginX; //entityTarget.ModdedPosX
                                bool withinY;

                                if (withinX < 0)
                                {
                                    withinY = entityTarget.HitBoxOriginX - entity.HitBoxX < entity.CollisionWidth + 1;
                                }
                                else
                                {
                                    withinY = withinX < entityTarget.TileAttributes + 1;
                                }

                                if (withinY)
                                {
                                    withinX = entity.HitBoxY - entityTarget.HitBoxOriginY;

                                    if (withinX < 0)
                                    {
                                        withinY = entityTarget.HitBoxOriginY - entity.HitBoxY < entity.CollisionDepth + 1;
                                    }
                                    else
                                    {
                                        withinY = withinX < entityTarget.Slope_18c + 1;
                                    }

                                    if (withinY)
                                    {
                                        withinX = entity.HitBoxZ - entityTarget.HitBoxOriginZ;

                                        if (withinX < 0)
                                        {
                                            withinY = entityTarget.HitBoxOriginZ - entity.HitBoxZ < entity.CollisionHeight + 1;
                                        }
                                        else
                                        {
                                            withinY = withinX < entityTarget.Slope_190 + 1;
                                        }

                                        if (withinY && entityTarget.Index != 0X1AD)
                                        {
                                            noValidCollisionFound = false;
                                            break;
                                        }
                                    }
                                }
                            }

                            entityIndex += 1;
                            entityTargetIndex += 1;

                        } while (entityIndex <= _gameEngine.StaticVariables.g_numberOfEntities);
                    }

                    if (noValidCollisionFound)
                    {
                        entity.Status = 2;
                        return;
                    }
                }

                _gameEngine.DestroyEntity(entity, -1);
            }

            entity.TargetAnimationId = 1;
            entity.TargetDirection = (entity.TargetDirection + 0x10) & 0X1F;
            entity.Flags = (entity.Flags & 0XFFFFFFCF) | 0x140;
        }
    }

    //8007fb38
    private void AI_DestroyEntity(Entity entity)
    {
        _gameEngine.DestroyEntity(entity, -1);
    }
    
    #endregion

    #region function type F

    // 8007fc64
    private void Script_FInteract_FUN_8007fc64(Entity entity)
    {
        entity.Bytes[0] = 1;
        entity.Bytes[1] = 0;
        entity.Bytes[2] = 0;
        entity.Bytes[3] = 0;
        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = 0;
        _gameEngine.StaticVariables.g_playerControlFlags |= 4;
    }

    //8007fc88
    //open a treasure ?
    private void FUN_8007fc88(Entity entity)
    {
        Entity entitySpawn;
        string scriptText;
        int iVar4;
        int itemId;

        if (_gameEngine.StaticVariables.g_playerControlFlags == 0
            && _gameEngine.StaticVariables.g_isGameEnding == 0
            && _gameEngine.StaticVariables.g_entitySlots[0].CarriedEntity == null
            && !_gameEngine.IsDialogInProgress())
        {
            itemId = (int)entity.ContentsItemId;
            iVar4 = 0x48;

            if (itemId != 0 && entity.TargetAnimationId == 0)
            {
                entity.TargetAnimationId = 2;
                entitySpawn = _gameEngine.SpawnWarpEntity(entity, 0, (uint)(itemId + 0x1e), entity.PosX, entity.PosY, entity.PosZ + 0x100000, 0);
                iVar4 = 0x49;

                if (entitySpawn != null)
                {
                    _gameEngine.EntityManager.FUN_8003ad30(entity);
                    entitySpawn.ForceZ = 0x8000;
                    entitySpawn.DelayOrAngle = 0x40;
                    entitySpawn.Flags &= 0xfffffe7f;
                    entitySpawn.IsNotProcessable = 0;
                    entitySpawn.Bytes[0] = 2;
                    entitySpawn.Bytes[1] = 0;
                    entitySpawn.Bytes[2] = 0;
                    entitySpawn.Bytes[3] = 0;
                    entitySpawn.ItemState = 0;
                    entitySpawn.AIValues[0] = (short)entity.Index; // entity but we can't so we store only the Index
                    iVar4 = _gameEngine.PlayerManager.FUN_80033d34(itemId) ? 1 : 0;
                    entitySpawn.AIValues[2] = (short)iVar4;

                    if (iVar4 != 0 && _gameEngine.StaticVariables.g_itemDropProperties[itemId].SoundSfxIndex == 0)
                    {
                        entitySpawn.AIValues[4] = 1;
                        entitySpawn.AIValues[5] = 0;
                        _gameEngine.SoundManager.LoadBgm(0);
                        return;
                    }

                    entitySpawn.AIValues[4] = 0;
                    entitySpawn.AIValues[5] = 0;
                    return;
                }
            }

            scriptText = _gameEngine.EtcRes.GetEtcString(iVar4); //0x48 or 0x49
            _gameEngine.UIManager.InitializeDialogMessage(scriptText, 0);
        }
    }

    #endregion
}