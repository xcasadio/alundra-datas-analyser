using System.Diagnostics;
using System.Windows.Forms;

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
        Register(ScriptHelper.ProgramCTick, 23, AI_FUN_8006b848);
        Register(ScriptHelper.ProgramCTick, 60, AI_UpdateIceProjectile);
        Register(ScriptHelper.ProgramCTick, 70, AI_FUN_8007b7b0);
        Register(ScriptHelper.ProgramCTick, 255, FUN_8007c174);


        Register(ScriptHelper.ProgramDTouch, 0, AI_EmptyFunction); // null
        Register(ScriptHelper.ProgramDTouch, 4, AI_EmptyFunction); // null

        Register(ScriptHelper.ProgramEDeactivate, 0, Script_Deactivate_FUN_8007ed10);
        Register(ScriptHelper.ProgramEDeactivate, 2, AI_FUN_8007ed30);
        Register(ScriptHelper.ProgramEDeactivate, 9, AI_HandleIceLightHitEffect);
        Register(ScriptHelper.ProgramEDeactivate, 17, AI_UpdateArrows);

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

        if (handlers.TryGetValue(eventId, out var handler))
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
        StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7D2B89DD + 0xE06A02E7;
        entity.TargetAnimationId = 4;
        entity.AIValues[0] = (short)(((ulong)StaticVariables.g_gameRandomSeed * 0x1F >> 32) + 0xB4);
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
        var index = Array.IndexOf(StaticVariables.g_entitySlots, entity);
        for (int i = index + 1; i < index + 15; i++)
        {
            _gameEngine.DestroyEntity(StaticVariables.g_entitySlots[i]);
        }

        for (int i = 0; i < 0x100; i++)
        {
            StaticVariables.g_loaderDirectionHistory[i] = 0;
            StaticVariables.DAT_80191508[i] = 0;
            StaticVariables.DAT_80191708[i] = 0;
        }

        StaticVariables.g_loaderInitialized = 0;
    }

    // 8006191C
    public void SetAnim8AndResetLoader(Entity entity)
    {
        entity.TargetAnimationId = 8;
        StaticVariables.g_loaderInitialized = 0;
    }

    // 80061930
    public void SpawnSpecificWarpAndResetLoader(Entity entity)
    {
        Entity spawned = _gameEngine.SpawnWarpEntity(entity, 1, 0x9D,
            entity.PosX + 0x380000, entity.PosY, entity.PosZ - 0x200000, entity.TargetDirection);
        entity.AIValues.Set(0xa000, 1); //spawned;
        StaticVariables.g_loaderInitialized = 0;
    }

    // 80061998
    public void SetAnim0AndResetLoader(Entity entity)
    {
        entity.TargetAnimationId = 0;
        StaticVariables.g_loaderInitialized = 0;
    }

    // 800619A8
    public void SetAnimEAndResetLoader(Entity entity)
    {
        entity.TargetAnimationId = 0xE;
        StaticVariables.g_loaderInitialized = 0;
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
        entity.InitialXPos = entity.ProgramIndexes[2]; //Bytes[0] + 1
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
        entity.InitialYPos = 1;
        entity.InitialXPos = 1;
        Entity baseEntity = _gameEngine.SpawnWarpEntity(entity, 1, 0xD1, 0x2400000, 0x2600000, z, 0);
        baseEntity.Bytes[0] = 0;
        for (int i = 0; i < 7; i++)
        {
            z -= 0xC0000;
            Entity next = _gameEngine.SpawnWarpEntity(entity, 1, 0xD1, 0x2400000, 0x2600000, z, 0);
            next.TargetAnimationId = 7;
            baseEntity.AIValues[1] = (short)next.Index;
            baseEntity = next;
        }

        z = 0xF40000;
        baseEntity = _gameEngine.SpawnWarpEntity(entity, 1, 0xD1, 0x2D00000, 0x2600000, z, 0);
        baseEntity.Bytes[0] = 1;
        for (int i = 0; i < 7; i++)
        {
            z -= 0xC0000;
            Entity next = _gameEngine.SpawnWarpEntity(entity, 1, 0xD1, 0x2D00000, 0x2600000, z, 0);
            next.TargetAnimationId = 7;
            baseEntity.AIValues[1] = (short)next.Index;
            baseEntity = next;
        }

        _gameEngine.SoundManager.PlaySoundEffect(0x19C);
        StaticVariables.g_loaderInitialized = 0;
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
        int iVar3;

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
                    _gameEngine.SetNextMapId(0xb);
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
                flags = StaticVariables.g_mapFlags;
            }
            else
            {
                flags = StaticVariables.g_globalFlags;
            }


            var index = (contentFlags >> 3) & 0xffc;
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
        entity.InitialXPos = entity.ContentsGameFlag;
        entity.InitialYPos = 0;
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

        ScriptHelper.CalculateEntityRelativePosition(entity, relPos);

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
                    uVar2 = (uint)ScriptHelper.GetDirectionToTarget(StaticVariables.PlayerEntity.PosX - entity.PosX, StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = uVar2;
                    entity.TargetAnimationId = 3;
                    entity.Bytes[1] = 1;
                    return;
                }

                StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                entity.TargetAnimationId = 1;
                entity.AIValues[1] = (short)((short)((ulong)StaticVariables.g_gameRandomSeed * 0x3d >> 32) + 0x3c);
                if (relPos[0] < 4 && relPos[1] < 4)
                {
                    uVar2 = (uint)ScriptHelper.GetDirectionToTarget(StaticVariables.PlayerEntity.PosX - entity.PosX, StaticVariables.PlayerEntity.PosY - entity.PosY);
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
                    StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    var uVar1 = (ulong)StaticVariables.g_gameRandomSeed;
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = (short)((short)((uVar1 * 0x2000) >> 32) & 0x7f);
                    entity.TargetDirection = (uint)((uVar1 * 0x2000) >> 40);
                    return;
                }

                StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                var direction = (byte)ScriptHelper.DirectionTable[entity.TargetDirection]; // g_directionFlipTable //80028b34
                entity.ForceStepY = 0;
                entity.ForceStepX = 0;
                entity.ForceY = 0;
                entity.ForceX = 0;
                entity.TargetForceY = 0;
                entity.TargetForceX = 0;
                entity.TargetDirection = direction;
                delay = (short)((StaticVariables.g_gameRandomSeed * 0x1f >> 32) + 0x1e);
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

        ScriptHelper.CalculateEntityRelativePosition(entity, relativePositions);

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
                        StaticVariables.PlayerEntity.PosX - entity.PosX,
                        StaticVariables.PlayerEntity.PosY - entity.PosY);
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
                    _gameEngine.EffectManager.CreateEffectEntity(0, StaticVariables.g_imageBuffer[10], 0,
                        entity.PosX, entity.PosY, entity.TerrainHeight);
                }

                if (StaticVariables.PlayerEntity.TouchingEntity == entity)
                {
                    StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    if ((int)((ulong)StaticVariables.g_gameRandomSeed * 3 >> 32) == 0)
                    {
                        entity.TargetAnimationId = 1;
                        direction = (uint)ScriptHelper.GetDirectionToTarget(
                            entity.PosX - StaticVariables.PlayerEntity.PosX,
                            entity.PosY - StaticVariables.PlayerEntity.PosY);
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
                    StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = (short)((short)((ulong)StaticVariables.g_gameRandomSeed * 0x29 >> 32) + 10);
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

        ScriptHelper.CalculateEntityRelativePosition(entity, relPos);

        switch (entity.TargetAnimationId)
        {
            case 0:
            case 1:
                if (entity.AIValues[1] != 0)
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                }

                if (entity.InitialXPos != 0)
                {
                    entity.InitialXPos -= 1;
                }

                if (entity.AIValues[1] == 0)
                {
                    StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    entity.TargetAnimationId = 6;
                    entity.AIValues[1] = (short)((StaticVariables.g_gameRandomSeed * 0x1f >> 0x20) + 0xb4);
                }

                if (entity.InitialXPos != 0 || 2 < relPos[0] || 2 < relPos[1] || 0x100000 < relPos[2])
                {
                    if (entity.ForceAdjusted != 0)
                    {
                        var bVar1 = StaticVariables.g_directionFlipTable[entity.TargetDirection];
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
                if (StaticVariables.PlayerEntity.TouchingEntity == entity)
                {
                    entity.Bytes[2] = 1;
                }

                frameTimer = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = frameTimer;
                if (frameTimer == 0)
                {
                    if (entity.Bytes[2] == 0)
                    {
                        StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                        entity.TargetAnimationId = 1;
                        if (StaticVariables.g_gameRandomSeed * 3 >> 0x20 == 0)
                        {
                            entity.TargetDirection = entity.TargetDirection + 0x10 & 0x1f;
                        }

                        entity.AIValues[1] = 0x3c;
                        entity.InitialXPos = 0x3c;
                    }
                    else
                    {
                        StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                        entity.InitialXPos = 0;
                        entity.TargetAnimationId = 6;
                        entity.AIValues[1] = (short)((StaticVariables.g_gameRandomSeed * 0x1f >> 0x20) + 0xb4);
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
                        direction = (uint)ScriptHelper.GetDirectionToTarget(StaticVariables.PlayerEntity.PosX - entity.PosX, StaticVariables.PlayerEntity.PosY - entity.PosY);
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
                direction = (uint)ScriptHelper.GetDirectionToTarget(StaticVariables.PlayerEntity.PosX - entity.PosX, StaticVariables.PlayerEntity.PosY - entity.PosY);
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
                        entity.Flags = entity.Flags & 0xffffff7fU | 0x10;
                    }
                }
                else
                {
                    entity.TargetAnimationId = (uint)StaticVariables.g_scriptAnimationTable[value];
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
                    entity.InitialXPos = 0;
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
                    if (entity.ForceZ > 0 && entity.InitialXPos == 0)
                    {
                        entity.InitialXPos = 1;
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
                        System.Diagnostics.Debugger.Break();
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
            entity.TargetAnimationId = (uint)StaticVariables.g_scriptAnimationTable2[entity.Flags2];
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
            entity.TargetAnimationId = (uint)StaticVariables.g_scriptAnimationTable3[entity.Flags2];
        }

        entity.PlatformEntity = null;
        entity.Flags = (entity.Flags | 0x34) & 0xffffff7f;//turn off bit 8, turn on bits 5 and 6
    }

    //8007c174
    public void FUN_8007c174(Entity entity)
    {
        bool bVar2;
        int initialYPos;
        Entity entity2;
        int uVar6;

        //Debugger.Break();
        var itemId = entity.SpriteTableIndex - 0x1e;

        if (entity.Bytes[0] == 2)
        {
            if (entity.InitialXPos != 0)
            {
                entity.InitialXPos += -1;
                return;
            }

            initialYPos = entity.InitialYPos;

            if (initialYPos == 1)
            {
                LAB_8007c6f4:
                bVar2 = _gameEngine.CdManager.FUN_8005a7d4();

                if (bVar2)
                {
                    return;
                }

                entity.InitialYPos += 1;
                _gameEngine.SetEtcAnimationMode(3);
                _gameEngine.SoundManager.StopAllSound();
                return;
            }

            if (initialYPos < 2)
            {
                if (initialYPos != 0)
                {
                    return;
                }

                entity.ForceZ = 0;
                bVar2 = _gameEngine.IsWarpInProgress();

                if (bVar2)
                {
                    return;
                }

                if (entity.AIValues[2] == 0)
                {
                    entity.InitialYPos += 2;
                    StaticVariables.g_dropItemTextBuffer = _gameEngine.EtcResR.GetEtcString(0x4e);
                    StaticVariables.g_dropItemTextBuffer += _gameEngine.EtcResR.GetIconName((int)itemId);
                    StaticVariables.g_dropItemTextBuffer += _gameEngine.EtcResR.GetEtcString(0x4f);

                    LAB_8007c320:
                    _gameEngine.PlayEtcAnimation(StaticVariables.g_dropItemTextBuffer, 1);
                    return;
                }


                StaticVariables.g_dropItemTextBuffer = _gameEngine.EtcResR.GetEtcString(0x4c);
                StaticVariables.g_dropItemTextBuffer += _gameEngine.EtcResR.GetIconName((int)itemId);
                StaticVariables.g_dropItemTextBuffer += _gameEngine.EtcResR.GetEtcString(0x4d);

                if (entity.AIValues[4] == 0)
                {
                    entity.InitialYPos += 2;
                    uVar6 = StaticVariables.g_iconNameEtcBase[(int)(itemId * 8 + 5)];

                    if (uVar6 == 0)
                    {
                        StaticVariables.g_dropItemTextBuffer += _gameEngine.EtcResR.GetEtcString(0x46);
                    }

                    _gameEngine.SoundManager.PlaySoundEffect((uint)uVar6);
                    //goto LAB_8007c320;
                    _gameEngine.PlayEtcAnimation(StaticVariables.g_dropItemTextBuffer, 1);
                    return;
                }

                entity.InitialYPos += 1;
                //goto LAB_8007c684;
                _gameEngine.PlayEtcAnimation(StaticVariables.g_dropItemTextBuffer, 1);
                _gameEngine.SetEtcAnimationMode(4);
                return;
            }

            if (initialYPos != 2)
            {
                return;
            }

            bVar2 = _gameEngine.IsWarpInProgress();

            if (bVar2)
            {
                return;
            }

            if (entity.AIValues[2] == 0)
            {
                entity.AIValues[4] = 0;
            }
            else
            {
                _gameEngine.PlayerManager.FUN_80033dbc(StaticVariables.PlayerEntity, itemId );
                _gameEngine.FUN_80032b28((uint)entity.ContentsGameFlag);
            }

            Debugger.Break();
            entity2 = null;
            //entity2 = entity.AIValues[0];
        }
        else
        {
            if (entity.IsAboveGround != 0 && entity.AIValues[2] != 0)
            {
                initialYPos = entity.AIValues[2] * 0xc >> 4;
                entity.AIValues[2] = (short)initialYPos;

                //StaticVariables.g_sharedBuffer2 + 4 => _gameEngine.CurrentMap.Info.Gravity
                if (initialYPos <= _gameEngine.CurrentMap.Info.Gravity << 8)
                {
                    entity.AIValues[2] = 0;
                    entity.AIValues[3] = 0;
                }

                entity.ForceZ = entity.AIValues[2];
            }

            initialYPos = entity.InitialXPos + -1;

            if (1 < entity.InitialXPos + 1U)
            {
                entity.InitialXPos = initialYPos;

                if (initialYPos == 0)
                {
                    goto LAB_8007c740;
                }

                if (initialYPos == 0x78)
                {
                    entity.DamagedTickCounter = 0x78;
                }
            }

            initialYPos = entity.InitialYPos;

            if (initialYPos == 1)
            {
                if (_gameEngine.IsWarpInProgress())
                {
                    return;
                }

                initialYPos = entity.ModdedPosX - StaticVariables.PlayerEntity.ModdedPosX;

                if (initialYPos < 0)
                {
                    if (entity.Width + 1 <= StaticVariables.PlayerEntity.ModdedPosX - entity.ModdedPosX)
                    {
                        return;
                    }
                }
                else if (StaticVariables.PlayerEntity.Width + 1 <= initialYPos)
                {
                    return;
                }

                initialYPos = entity.ModdedPosY - StaticVariables.PlayerEntity.ModdedPosY;

                if (initialYPos < 0)
                {
                    if (entity.Height + 1 <= StaticVariables.PlayerEntity.ModdedPosY - entity.ModdedPosY)
                    {
                        return;
                    }
                }
                else if (StaticVariables.PlayerEntity.Height + 1 <= initialYPos)
                {
                    return;
                }

                initialYPos = entity.ModdedPosZ - StaticVariables.PlayerEntity.ModdedPosZ;

                if (initialYPos < 0)
                {
                    if (entity.Depth + 1 <= StaticVariables.PlayerEntity.ModdedPosZ - entity.ModdedPosZ)
                    {
                        return;
                    }
                }
                else if (StaticVariables.PlayerEntity.Depth + 1 <= initialYPos)
                {
                    return;
                }

                var res = _gameEngine.PlayerManager.FUN_80033f00(StaticVariables.PlayerEntity, (int)itemId);

                if (res == false && entity.Bytes[0] != 0)
                {
                    StaticVariables.g_dropItemTextBuffer = string.Empty;
                }
                else
                {
                    StaticVariables.g_dropItemTextBuffer = _gameEngine.EtcResR.GetIconName((int)itemId);
                    StaticVariables.g_dropItemTextBuffer += _gameEngine.EtcResR.GetEtcString(0x45);
                }

                Debugger.Break();
                _gameEngine.FUN_80032b28((uint)entity.ContentsGameFlag); //AIValues
                uVar6 = StaticVariables.g_iconNameEtcBase[(int)(itemId * 8 + 5)];

                if (uVar6 != 0)
                {
                    _gameEngine.SoundManager.PlaySoundEffect((uint)uVar6);

                    if (StaticVariables.g_dropItemTextBuffer.Length > 0)
                    {
                        _gameEngine.PlayEtcAnimation(StaticVariables.g_dropItemTextBuffer, 0);
                    }
                    entity.InitialYPos += 2;
                    return;
                }

                entity.InitialYPos += 1;
                _gameEngine.EntityManager.FUN_8003ad30(entity);
                _gameEngine.SoundManager.LoadBgm(0);
                _gameEngine.SetNextMapId(0xb);

                if (StaticVariables.g_dropItemTextBuffer.Length == 0)
                {
                    goto LAB_8007c68c;
                }

                LAB_8007c684:
                _gameEngine.PlayEtcAnimation(StaticVariables.g_dropItemTextBuffer, 1);

                LAB_8007c68c:
                _gameEngine.SetEtcAnimationMode(4);
                return;
            }

            if (initialYPos < 2)
            {
                if (initialYPos != 0)
                {
                    return;
                }
                if (0 < entity.ForceZ)
                {
                    return;
                }
                entity.InitialYPos = 1;
                return;
            }

            if (initialYPos == 2)
            {
                //goto LAB_8007c6f4;
                bVar2 = _gameEngine.CdManager.FUN_8005a7d4();

                if (bVar2)
                {
                    return;
                }

                entity.InitialYPos += 1;
                _gameEngine.SetEtcAnimationMode(3);
                _gameEngine.SoundManager.StopAllSound();
                return;
            }

            if (initialYPos != 3)
            {
                return;
            }

            bVar2 = _gameEngine.IsWarpInProgress();
            entity2 = entity;

            if (bVar2)
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
            entity.Flags = entity.Flags & 0xffffffcfU | 0x40;
        }
    }

    //8007ef50
    void AI_HandleIceLightHitEffect(Entity entity)
    {
        long rand;
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
                    isSmallSprite = (int)(StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7);
                    StaticVariables.g_gameRandomSeed = (uint)(isSmallSprite * 0x7d2b89dd + 0xe06a02e7);
                    rand = StaticVariables.g_gameRandomSeed;
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
                        isSmallSprite = (int)(StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7);
                        StaticVariables.g_gameRandomSeed = (uint)(isSmallSprite * 0x7d2b89dd + 0xe06a02e7);
                        rand = StaticVariables.g_gameRandomSeed;
                        randomOffset = (isSmallSprite * 0x60001) >> 0x20;

                        effectEntity.ForceX = randomOffset + -0x30000;
                        effectEntity.ForceY = (int)((rand * 0x40001) >> 0x20) + -0x20000;
                    }

                    loopCounter += 1;

                } while (loopCounter < 5);

                entity.TargetAnimationId = 1;
                entity.Flags = entity.Flags & 0xffffffcfU | 0x40;
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

                    if (-1 < StaticVariables.g_numberOfEntity)
                    {
                        do
                        {
                            var entityTarget = StaticVariables.g_entitySlots[entityTargetIndex];

                            if (entity != entityTarget
                                && (entityTarget.AnimFlags & 0x40) == 0
                                && entityTarget.BalanceRecord.Vals[5] == 0
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

                        } while (entityIndex <= StaticVariables.g_numberOfEntity);
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
            entity.Flags = entity.Flags & 0XFFFFFFCF | 0x140;
        }
    }

    #endregion

    #region function type E



    #endregion

    #region function type F

    // 8007fc64
    private void Script_FInteract_FUN_8007fc64(Entity entity)
    {
        entity.Bytes[0] = 1;
        entity.Bytes[1] = 0;
        entity.Bytes[2] = 0;
        entity.Bytes[3] = 0;
        StaticVariables.PlayerEntity.TargetAnimationId = 0;
        StaticVariables.g_playerControlFlags |= 4;
    }

    //8007fc88
    private void FUN_8007fc88(Entity entity)
    {
        Debugger.Break();
    }

    #endregion
}
