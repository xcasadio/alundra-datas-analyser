using System.Diagnostics;

namespace Alundra.Gameplay.Scripts;

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

        Register(ScriptHelper.ProgramCTick, 0, AI_EmptyFunction); // null
        Register(ScriptHelper.ProgramCTick, 1, AI_UpdateEntityAI_IdleSkittish);
        Register(ScriptHelper.ProgramCTick, 2, AI_UpdateEntityAI_CuriousFlying);
        Register(ScriptHelper.ProgramCTick, 3, AI_UpdateEntityAI_1);
        Register(ScriptHelper.ProgramCTick, 4, AI_FUN_80066984);

        Register(ScriptHelper.ProgramCTick, 0x17, etick_17_jarsandboxes_Handler);
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
                Debug.WriteLine($"Entity {entity.Index} exec func[{eventType}][{eventId}] => {handler.Method.Name}");
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
        entity.AIValues[0] = (short)(entity.ZPos >> 16);
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
        Entity spawned = _gameEngine.SpawnWarpEntity(entity, 1, 0xF5, entity.XPos + 0xF00000, 
            entity.YPos, entity.ZPos, entity.TargetDirection);
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
            _gameEngine.TriggerWarp(StaticVariables.g_entitySlots[i]);
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
            entity.XPos + 0x380000, entity.YPos, entity.ZPos - 0x200000, entity.TargetDirection);
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
        entity.Bytes[0] = (byte)(entity.ZPos & 0xFF);
    }

    // 800619E8
    public void SpawnWarpDropAndAdjustPosition(Entity entity)
    {
        Entity spawned = _gameEngine.SpawnWarpEntity(entity, 1, 0xD8, entity.XPos, entity.YPos, entity.ZPos, entity.TargetDirection);
        spawned.TargetAnimationId = 5;
        entity.TargetAnimationId = 3;
        entity.YPos -= 0x100000;
        entity.ZPos += 0x300000;
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

        _gameEngine.PlaySoundEffect(0x19C);
        StaticVariables.g_loaderInitialized = 0;
    }

    #endregion

    #region function type C

    // 80065ED4
    private void AI_UpdateEntityAI_IdleSkittish(Entity entity)
    {
        ulong uVar1;
        short delay;
        uint uVar2;
        int[] relPos = new int[6];
        byte direction;
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
                    uVar2 = (uint)ScriptHelper.GetDirectionToTarget(StaticVariables.g_entitySlots[0].XPos - entity.XPos, StaticVariables.g_entitySlots[0].YPos - entity.YPos);
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
                    uVar2 = (uint)ScriptHelper.GetDirectionToTarget(StaticVariables.g_entitySlots[0].XPos - entity.XPos, StaticVariables.g_entitySlots[0].YPos - entity.YPos);
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
                    uVar1 = (ulong)StaticVariables.g_gameRandomSeed;
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = (short)((short)((uVar1 * 0x2000) >> 32) & 0x7f);
                    entity.TargetDirection = (uint)((uVar1 * 0x2000) >> 40);
                    return;
                }

                StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                direction = (byte)ScriptHelper.DirectionTable[entity.TargetDirection]; // g_directionFlipTable //80028b34
                entity.YForceStep = 0;
                entity.XForceStep = 0;
                entity.YForce = 0;
                entity.XForce = 0;
                entity.TargetYForce = 0;
                entity.TargetXForce = 0;
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
        short sVar2;
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
                sVar2 = (short)(entity.AIValues[1] - 1);
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
                        StaticVariables.g_entitySlots[0].XPos - entity.XPos,
                        StaticVariables.g_entitySlots[0].YPos - entity.YPos);
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
                        entity.XPos, entity.YPos, entity.FloorHeight);
                }

                if (StaticVariables.g_entitySlots[0].TouchingEntity == entity)
                {
                    StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    if ((int)((ulong)StaticVariables.g_gameRandomSeed * 3 >> 32) == 0)
                    {
                        entity.TargetAnimationId = 1;
                        direction = (uint)ScriptHelper.GetDirectionToTarget(
                            entity.XPos - StaticVariables.g_entitySlots[0].XPos,
                            entity.YPos - StaticVariables.g_entitySlots[0].YPos);
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
        byte bVar1;
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
                    entity.AIValues[1] = (short)(entity.AIValues[1] - (short)1);
                }

                if (entity.InitialXPos != 0)
                {
                    entity.InitialXPos = entity.InitialXPos - 1;
                }

                if (entity.AIValues[1] == 0)
                {
                    StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    entity.TargetAnimationId = 6;
                    entity.AIValues[1] = (short)((StaticVariables.g_gameRandomSeed * 0x1f >> 0x20) + 0xb4);
                }

                if (((entity.InitialXPos != 0) || (2 < relPos[0])) || ((2 < relPos[1] || (0x100000 < relPos[2]))))
                {
                    if (entity.ForceAdjusted != 0)
                    {
                        bVar1 = (byte)StaticVariables.g_directionFlipTable[entity.TargetDirection];
                        entity.YForceStep = 0;
                        entity.XForceStep = 0;
                        entity.YForce = 0;
                        entity.XForce = 0;
                        entity.TargetYForce = 0;
                        entity.TargetXForce = 0;
                        entity.TargetDirection = bVar1;
                        return;
                    }

                    _gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 1, 0);
                    return;
                }

                goto LAB_80066938;
            case 3:
                if (StaticVariables.g_entitySlots[0].TouchingEntity == entity)
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
                        if ((StaticVariables.g_gameRandomSeed * 3 >> 0x20) == 0)
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

                if (((relPos[0] < 3) && (relPos[1] < 3)) && (relPos[2] < 0x100001))
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
                        direction = (uint)ScriptHelper.GetDirectionToTarget(StaticVariables.g_entitySlots[0].XPos - entity.XPos, StaticVariables.g_entitySlots[0].YPos - entity.YPos);
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
                    entity.Flags = entity.Flags | 0x40;
                    return;
                }

                LAB_80066938:
                entity.TargetAnimationId = 3;
                direction = (uint)ScriptHelper.GetDirectionToTarget(StaticVariables.g_entitySlots[0].XPos - entity.XPos, StaticVariables.g_entitySlots[0].YPos - entity.YPos);
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
                    entity.PlatformEntity.ActionState = 0;
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
        else if ((entity.SpriteTableIndex != 0x175) || ((value = 0x175) != 0 && entity.TargetAnimationId != 5))
        {
            if (entity.ForceAdjusted == 0)
            {
                if (entity.IsAboveGround != 0 || entity.HitCounter != 0)
                {
                    _gameEngine.PlaySoundEffect(0x18);
                    entity.InitialXPos = 0;
                    value = 0;
                    entity2 = entity;
                    do
                    {
                        if ((entity2.MapTiles[0].Walkability & 0x1001) == 0x1001)
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
                    if (entity.ZForce > 0 && entity.InitialXPos == 0)
                    {
                        entity.InitialXPos = 1;
                        entity.TargetDirection = (entity.TargetDirection + 0x10) & 0x1f;
                        _gameEngine.EffectManager.CreateEffectEntity(0, 9, 0, entity.XPos, entity.YPos, entity.ZPos + 0x80000);
                    }
                }
                else
                {
                    do
                    {
                        if ((entity2.MapTiles[0].Walkability & 0x1001) == 0x1001)
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
    
    //8007b7b0
    public void etick_17_jarsandboxes_Handler(Entity entity)
    {
        if (entity.PlatformEntity == null)
        {
            return;
        }

        if (entity.Flags2 == 0)
        {
            entity.TargetAnimationId = 0;
            return;
        }


        if (entity.Flags2 == -1)
        {
            entity.TargetAnimationId = 3;
        }
        else
        {
            entity.TargetAnimationId = (uint)StaticVariables.g_scriptAnimationTable[entity.Flags2];
        }

        if (entity.PlatformEntity != null)//redudant check
        {
            entity.ActionState = 0;
        }

        entity.PlatformEntity = null;
        entity.Flags = (entity.Flags | 0x30) & 0xff7f;//turn off bit 8, turn on bits 5 and 6
    }

    #endregion
}
