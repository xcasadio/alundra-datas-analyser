using System.Diagnostics;

namespace AlundraEngine.Gameplay.Scripts.Boss;

public static class AncientGuardian
{
    //80071134
    public static void AI_ApplyZGravityIfIdle(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name == "◆Roche élémentaire")
        {
            Breakpoint.TriggerBreak();
        }

        if (entity.TargetAnimationId == 0 && -0x80001 < entity.ForceZ)
        {
            entity.ForceZ -= 0x4000;
        }
    }

    //80070598
    public static void AI_UpdateBossEntityState(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Élément Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        int delay;
        uint direction;
        uint i;
        int[] relPos = new int[6];
        short aiVal;
        byte val;

        delay = entity.DelayOrAngle - 1;

        if (entity.DelayOrAngle != 0)
        {
            entity.DelayOrAngle = delay;

            if (delay == 0)
            {
                gameEngine.StaticVariables.g_scrollingParameters.Flag = 0;
                gameEngine.SoundManager.PlaySoundEffect(0x55);
            }
        }

        if (entity.Bytes[3] != 0 && entity.TargetAnimationId == 0)
        {
            if (entity.Bytes[3] < 4)
            {
                AI_Melzas2.UpdateEntityAI_BossExplode(gameEngine, entity);
                return;
            }
            entity.TargetAnimationId = 10;
            entity.Flags |= 0x40;
            return;
        }

        if (entity.TargetAnimationId != 1 && entity.TargetAnimationId != 0xd)
        {
            entity.ItemState = 0;
        }

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relPos);

        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.Bytes[2] != 0)
                {
                    return;
                }

                if (entity.AIValues[1] != 0)
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                    return;
                }

                if (entity.AIValues[4] == 0 || (Random.Next() * (ulong)(entity.AIValues[4] + 1)) >> 0x20 != 0)
                {
                    val = entity.Bytes[1];
                    entity.AIValues[4] = 0;

                    if (val != 0)
                    {
                        gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x3c, 0x46);
                        entity.Bytes[0] = 2;
                        return;
                    }

                    if (relPos[0] < 3 && relPos[1] < 3 && relPos[2] < 0x100001)
                    {
                        entity.TargetAnimationId = 1;
                        direction = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX, entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
                        entity.TargetDirection = direction;
                        entity.AIValues[1] = 0x78;
                        entity.Bytes[0] = 1;
                        return;
                    }

                    gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x78, 0x14);
                    entity.Bytes[0] = 0;
                    return;
                }

                LAB_80070914:
                entity.TargetAnimationId = 2;
                entity.TargetDirection = 0;
                entity.AIValues[1] = 0x3c;
                break;

            case 1:
            case 0xd:
                delay = entity.HpMax;

                if (delay < 0)
                {
                    delay += 3;
                }

                if (entity.Hp <= delay >> 2)
                {
                    entity.TargetAnimationId = 0xd;
                }

                delay = entity.ItemState;

                if (delay == 0)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x1cf);
                    delay = 0x10;

                    if (entity.TargetAnimationId == 1)
                    {
                        delay = 0x1a;
                    }

                    entity.ItemState = delay;
                    delay = entity.ItemState;
                }

                aiVal = entity.AIValues[1];
                entity.Bytes[1] = 0;
                entity.AIValues[1] = (short)(aiVal - 1);
                val = entity.Bytes[0];
                entity.ItemState = delay - 1;

                if (val == 1)
                {
                    val = 3;
                    if (entity.AIValues[1] != 0 && entity.ForceAdjusted == 0)
                    {
                        return;
                    }
                }
                else
                {
                    if (1 < val)
                    {
                        if (val != 2)
                        {
                            return;
                        }

                        if (entity.AIValues[1] != 0 && entity.ForceAdjusted == 0)
                        {
                            return;
                        }

                        entity.TargetAnimationId = 0;
                        entity.AIValues[1] = 0x28;
                        return;
                    }

                    if (val != 0)
                    {
                        return;
                    }

                    if (entity.AIValues[1] == 0)
                    {
                        goto LAB_80070914;
                    }

                    val = 2;

                    if (!gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relPos, 1, 5, 0x100000))
                    {
                        if (entity.ForceAdjusted == 0)
                        {
                            return;
                        }

                        entity.TargetDirection = (entity.TargetDirection + 8) & 0x1f;
                        return;
                    }
                }

                entity.Bytes[1] = val;
                entity.TargetAnimationId = 0xc;
                direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                entity.TargetDirection = direction;
                entity.AIValues[1] = 0x3c;
                break;

            case 2:
                aiVal = entity.AIValues[1];
                if (aiVal != 0)
                {
                    entity.AIValues[1] = (short)(aiVal - 1);

                    if (aiVal == 1)
                    {
                        gameEngine.SoundManager.PlaySoundEffect(0x55);
                        gameEngine.StaticVariables.g_scrollingParameters.Flag = 1;
                        gameEngine.StaticVariables.g_scrollingParameters.SpeedX = 1;
                        gameEngine.StaticVariables.g_scrollingParameters.SpeedY = 1;
                        gameEngine.StaticVariables.g_scrollingParameters.LimitX = 2;
                        gameEngine.StaticVariables.g_scrollingParameters.LimitY = 2;
                        entity.DelayOrAngle = 0x28;
                    }
                }

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                entity.AIValues[1] = 0x3c;
                entity.TargetAnimationId = 0;
                delay = (int)((Random.Next() * 4) >> 0x20) + 4;
                entity.Bytes[1] = (byte)delay;

                if (delay != 4)
                {
                    direction = (uint)((Random.Next() * 4) >> 0x20);

                    if (0x1f < direction)
                    {
                        return;
                    }

                    //create ◆Élément Niv.1
                    do
                    {
                        i = direction + 4;
                        var entitySpawned = gameEngine.SpawnWarpEntity(entity, 1, 0xf3, entity.PosX, entity.PosY, entity.PosZ, direction);
                        direction = i;
                    } while ((int)i < 0x20);

                    return;
                }

                goto LAB_80070b70;

            case 6:
                if (!gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relPos, 3, 3, 0))
                {
                    return;
                }

                entity.TargetAnimationId = 0xb;
                entity.AIValues[1] = 0x78;
                direction = entity.Flags;
                i = 0xfffffeff;
                goto LAB_80070bd4;

            case 7:
                if (entity.IsOnGround != 0)
                {
                    entity.TargetAnimationId = 0;
                    gameEngine.SoundManager.PlaySoundEffect(0x65);
                }
                break;

            case 9:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[3] == 0)
                {
                    val = entity.Bytes[2];
                    entity.TargetAnimationId = 0;
                    entity.DamagedTickCounter = 0x5a;
                    entity.AIValues[4] = 0;

                    if (val != 0)
                    {
                        return;
                    }

                    delay = entity.HpMax;
                    entity.AIValues[1] = 0x14;

                    if (delay < 0)
                    {
                        delay += 3;
                    }

                    aiVal = 2;

                    if (entity.Hp <= delay >> 2)
                    {
                        aiVal = 1;
                    }
                    entity.AIValues[4] = aiVal;

                    return;
                }

                entity.AIValues[1] = 300;
                entity.AIValues[5] = 0x1e;
                direction = entity.Flags;
                i = 0xfffffffc;
                entity.TargetAnimationId = 0;

            LAB_80070bd4:
                entity.Flags = direction & i;
                break;

            case 0xb:
                aiVal = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = aiVal;
                if (aiVal == 0)
                {
                    entity.TargetAnimationId = 7;
                    entity.Flags |= 0x100;
                }
                break;

            case 0xc:
                aiVal = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = aiVal;

                if (aiVal != 0)
                {
                    return;
                }

                entity.TargetAnimationId = 0;
                entity.AIValues[1] = 0xf;

            LAB_80070b70:
                //create ◆Surveillance élémentaire
                var entitySpawned2 = gameEngine.SpawnWarpEntity(entity, 1, 0xf3, entity.PosX, entity.PosY, entity.PosZ, entity.TargetDirection);
                break;
        }
    }

    //80070c40
    public static void AI_UpdateBossEntityState2(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != null
            && entity.Name != "◆Surveillance élémentaire")
        {
            Breakpoint.TriggerBreak();
        }

        byte aiState;
        short animationTimer;
        uint uVar1;
        Entity parentEntity;
        int[] relPpos = new int[6];

        parentEntity = entity.ParentEntity;
        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relPpos);

        if (entity.TargetAnimationId == 0)
        {
            aiState = parentEntity.Bytes[1];
            entity.TargetAnimationId = 1;
            entity.Bytes[1] = aiState;
            parentEntity.Bytes[2] = 1;

            switch (entity.Bytes[1])
            {
                case 1:
                case 4:
                    entity.Bytes[2] = 0x14;
                    return;

                case 2:
                    uVar1 = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = uVar1;
                    entity.Bytes[2] = 0xf;
                    entity.AIValues[1] = 0xb4;
                    return;

                case 3:
                    aiState = gameEngine.StaticVariables.BYTE_ARRAY_80028b54[parentEntity.AnimationDirection];
                    entity.AIValues[1] = 0x14;
                    entity.Bytes[0] = 0;
                    uVar1 = (uint)(aiState + 8);
                    break;

                case 5:
                    entity.Bytes[2] = 1;
                    return;

                case 6:
                case 7:
                    entity.Bytes[2] = 6;
                    return;

                default:
                    return;
            }

        LAB_80071008:
            entity.TargetDirection = uVar1 & 0x1f;
            return;
        }

        if (entity.TargetAnimationId != 1)
        {
            return;
        }

        switch (entity.Bytes[1])
        {
            case 1:
            case 4:
                if (entity.AIValues[1] == 0)
                {
                    entity.AIValues[1] = 0xf;
                }

                animationTimer = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = animationTimer;

                if (animationTimer != 0)
                {
                    if (entity.ForceAdjusted == 0 && FunctionTypeC.CanMoveForward(entity, 0) == 0)
                    {
                        return;
                    }

                    entity.TargetDirection = gameEngine.StaticVariables.g_directionFlipTable[entity.TargetDirection];
                    return;
                }

                var entitySpawned = gameEngine.SpawnWarpEntity(entity, 1, 0xf2, entity.PosX, entity.PosY, entity.PosZ + 0xa00000, entity.TargetDirection);
                aiState = (byte)(entity.Bytes[2] - 1);
                entity.Bytes[2] = aiState;

                if (aiState != 0)
                {
                    entity.TargetDirection = (uint)((Random.Next() * 0x20) >> 0x20);
                    return;
                }

                break;

            case 2:
                animationTimer = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = animationTimer;

                if (animationTimer == 0x78 || animationTimer == 0x3c)
                {
                    uVar1 = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = uVar1;
                }

                if (entity.AIValues[1] != 0
                    && entity.ForceAdjusted == 0
                    && FunctionTypeC.CanMoveForward(entity, 0) == 0)
                {
                    aiState = (byte)(entity.Bytes[2] - 1);
                    entity.Bytes[2] = aiState;

                    if (aiState != 0)
                    {
                        return;
                    }

                    var entitySpawned2 = gameEngine.SpawnWarpEntity(entity, 1, 0xf2, entity.PosX, entity.PosY, entity.PosZ + 0xa00000, entity.TargetDirection);
                    entity.Bytes[2] = 0xf;
                    return;
                }
                break;

            case 3:
                animationTimer = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = animationTimer;

                if (animationTimer == 0)
                {
                    aiState = entity.Bytes[0];
                    entity.Bytes[0] = (byte)(aiState + 1);

                    switch (aiState)
                    {
                        case 0:
                            uVar1 = entity.TargetDirection;
                            animationTimer = 5;
                            break;

                        case 2:
                        case 4:
                        case 6:
                        case 8:
                        case 10:
                        case 0xc:
                        case 0xe:
                        case 0x10:
                            var entitySpawned3 = gameEngine.SpawnWarpEntity(entity, 1, 0xf2, entity.PosX, entity.PosY, entity.PosZ + 0xa00000, entity.TargetDirection);
                            goto LABEL1;

                        case 1:
                        case 3:
                        case 5:
                        case 7:
                        case 9:
                        case 0xb:
                        case 0xd:
                        case 0xf:
                        LABEL1:
                            uVar1 = entity.TargetDirection;
                            entity.AIValues[1] = 5;
                            uVar1 -= 1;
                            //goto LAB_80071008;
                            entity.TargetDirection = uVar1 & 0x1f;
                            return;

                        case 0x11:
                            uVar1 = entity.TargetDirection;
                            animationTimer = 0x14;
                            break;

                        case 0x12:
                            goto switchD_80070fac_caseD_12;

                        default:
                            return;
                    }

                    entity.AIValues[1] = animationTimer;
                    entity.TargetDirection = (uVar1 - 8) & 0x1f;
                    var entitySpawned4 = gameEngine.SpawnWarpEntity(entity, 1, 0xf2, entity.PosX, entity.PosY, entity.PosZ + 0xa00000, entity.TargetDirection);
                    return;
                }

                LAB_800710f0:
                if (entity.ForceAdjusted == 0 && FunctionTypeC.CanMoveForward(entity, 0) == 0)
                {
                    return;
                }

                break;

            case 5:
            case 6:
            case 7:
                if (entity.AIValues[1] == 0)
                {
                    entity.AIValues[1] = 0x14;
                }

                animationTimer = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = animationTimer;

                if (animationTimer != 0)
                {
                    goto LAB_800710f0;
                }

                var entitySpawned5 = gameEngine.SpawnWarpEntity(entity, 1, 0xf2, entity.PosX, entity.PosY, entity.PosZ + 0xa00000, entity.TargetDirection);
                aiState = (byte)(entity.Bytes[2] - 1);
                entity.Bytes[2] = aiState;

                if (aiState != 0)
                {
                    return;
                }
                break;

            default:
                return;
        }

    switchD_80070fac_caseD_12:
        gameEngine.DestroyEntity(entity);
        parentEntity.Bytes[2] = 0;
    }
}