using AlundraEngine.Gameplay.Scripts.Boss;
using System;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;

namespace AlundraEngine.Gameplay.Scripts;

public static class FunctionTypeC
{
    // 80065ED4
    public static void AI_UpdateEntityAI_IdleSkittish(GameEngine gameEngine, Entity entity)
    {
        short delay;
        uint uVar2;
        int[] relPos = new int[6];
        short duration;

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relPos);

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
                    uVar2 = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = uVar2;
                    entity.TargetAnimationId = 3;
                    entity.Bytes[1] = 1;
                    return;
                }

                entity.TargetAnimationId = 1;
                var rand = ((Random.Next() * 0x3d) >> 32) + 0x3c;
                entity.AIValues[1] = (short)rand;
                if (relPos[0] < 4 && relPos[1] < 4)
                {
                    uVar2 = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
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
                    var rand2 = Random.Next();
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = (short)((short)((rand2 * 0x2000) >> 32) & 0x7f);
                    entity.TargetDirection = (uint)((rand2 * 0x2000) >> 40);
                    return;
                }

                var direction = (byte)ScriptHelper.DirectionTable[entity.TargetDirection]; // g_directionFlipTable //80028b34
                entity.ForceStepY = 0;
                entity.ForceStepX = 0;
                entity.ForceY = 0;
                entity.ForceX = 0;
                entity.TargetForceY = 0;
                entity.TargetForceX = 0;
                entity.TargetDirection = direction;
                delay = (short)(((Random.Next() * 0x1f) >> 32) + 0x1e);
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
                gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x28, 0x14);
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
    public static void AI_UpdateEntityAI_CuriousFlying(GameEngine gameEngine, Entity entity)
    {
        bool bVar1;
        uint direction;
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

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
                    gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x50, 0x1e);
                    entity.Bytes[0] = 0;
                }
                else if (entity.Bytes[0] == 0 && gameEngine.EntityGameplayManager.TryAttackPlayer(entity, relativePositions, 2, 0x100000))
                {
                    entity.TargetAnimationId = 0xd;
                    direction = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.AIValues[6] = 0;
                    entity.AIValues[7] = 0;
                }
                else if (entity.ForceAdjusted == 0)
                {
                    gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 1, 0x200000);
                }
                else
                {
                    gameEngine.EntityGameplayManager.UpdateDirectionForced(entity, 1, 4, 0x200000);
                }
                break;

            case 3:
                direction = (uint)(entity.AIValues[6] + 1);
                entity.AIValues[6] = (short)direction;
                if ((direction & 7) == 0)
                {
                    gameEngine.EffectManager.CreateEffectEntity(0, gameEngine.StaticVariables.g_imageBuffer[10], 0,
                        entity.PosX, entity.PosY, entity.TerrainHeight);
                }

                if (gameEngine.StaticVariables.PlayerEntity.TouchingEntity == entity)
                {
                    var rand = (Random.Next() * 3) >> 32;

                    if (rand == 0)
                    {
                        entity.TargetAnimationId = 1;
                        direction = (uint)ScriptHelper.GetDirectionToTarget(
                            entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX,
                            entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
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
                    entity.TargetAnimationId = 0;
                    var rand = ((Random.Next() * 0x29) >> 32) + 10;
                    entity.AIValues[1] = (short)rand;
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
    public static void AI_UpdateEntityAI_1(GameEngine gameEngine, Entity entity)
    {
        short frameTimer;
        uint direction;
        int[] relPos = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relPos);

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
                    entity.TargetAnimationId = 6;
                    entity.AIValues[1] = (short)(((Random.Next() * 0x1f) >> 0x20) + 0xb4);
                }

                if (entity.DelayOrAngle != 0 || 2 < relPos[0] || 2 < relPos[1] || 0x100000 < relPos[2])
                {
                    if (entity.ForceAdjusted != 0)
                    {
                        var bVar1 = gameEngine.StaticVariables.g_directionFlipTable[entity.TargetDirection];
                        entity.ForceStepY = 0;
                        entity.ForceStepX = 0;
                        entity.ForceY = 0;
                        entity.ForceX = 0;
                        entity.TargetForceY = 0;
                        entity.TargetForceX = 0;
                        entity.TargetDirection = bVar1;
                        return;
                    }

                    gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 1, 0);
                    return;
                }

                goto LAB_80066938;
            case 3:
                if (gameEngine.StaticVariables.PlayerEntity.TouchingEntity == entity)
                {
                    entity.Bytes[2] = 1;
                }

                frameTimer = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = frameTimer;
                if (frameTimer == 0)
                {
                    if (entity.Bytes[2] == 0)
                    {
                        entity.TargetAnimationId = 1;
                        var rand = (Random.Next() * 3) >> 0x20;

                        if (rand == 0)
                        {
                            entity.TargetDirection = (entity.TargetDirection + 0x10) & 0x1f;
                        }

                        entity.AIValues[1] = 0x3c;
                        entity.DelayOrAngle = 0x3c;
                    }
                    else
                    {
                        entity.DelayOrAngle = 0;
                        entity.TargetAnimationId = 6;
                        entity.AIValues[1] = (short)(((Random.Next() * 0x1f) >> 0x20) + 0xb4);
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
                        direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                        entity.TargetDirection = direction;
                        entity.AIValues[1] = 0x78;
                        entity.Bytes[2] = 0;
                        entity.Bytes[1] = 0;
                    }
                    else
                    {
                        gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x78, 0x14);
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
                direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                entity.TargetDirection = direction;
                entity.AIValues[1] = 0x78;
                entity.Bytes[2] = 0;
                break;
        }
    }

    //80066984
    public static void AI_FUN_80066984(GameEngine gameEngine, Entity entity)
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
                    entity.TargetAnimationId = (uint)gameEngine.StaticVariables.g_scriptAnimationTable[value];
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
                    gameEngine.SoundManager.PlaySoundEffect(0x18);
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
                        gameEngine.EffectManager.CreateEffectEntity(0, 9, 0, entity.PosX, entity.PosY, entity.PosZ + 0x80000);
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

    //80069f44
    public static void AI_FUN_80069f44(GameEngine gameEngine, Entity entity)
    {
        bool bVar1;
        byte bVar2;
        short sVar3;
        uint uVar4;
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);
        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.AIValues[1] == 0)
                {
                    gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x78, 0x46);
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
                    uVar4 = (uint)((Random.Next() * 0x20) >> 0x20);
                    entity.AIValues[1] = 0x78;
                }
                else
                {
                    bVar1 = gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 3, 3, 0);

                    if (bVar1)
                    {
                        var rand = (Random.Next() * 5) >> 0x20;

                        if ((int)rand == 0)
                        {
                            gameEngine.SoundManager.PlaySoundEffect(0x94);
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
                        uVar4 = (uint)((Random.Next() * 0x20) >> 0x20);
                        entity.AIValues[1] = 0x78;
                        entity.Bytes[1] = 3;
                    }
                    else
                    {
                        entity.Bytes[1] = (byte)(entity.Bytes[1] - 1);
                       uVar4 = (uint)((Random.Next() * 0x20) >> 0x20);
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
                    gameEngine.EffectManager.CreateEffectEntity(
                        (byte)0,
                        gameEngine.CurrentMap.Info.C, // gameEngine.CurrentMap.Info.C
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
                uVar4 = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
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

    //80064294
    public static void AI_FUN_80064294(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //800647b0
    public static void AI_FUN_800647b0(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80064884
    public static void AI_FUN_80064884(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80064d90
    public static void AI_FUN_80064d90(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80065100
    public static void AI_FUN_80065100(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80065204
    public static void AI_FUN_80065204(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80065750
    public static void AI_FUN_80065750(GameEngine gameEngine, Entity entity)
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
            entity.Flags = (uVar4 & 0xffffdfff) | 8;
        }

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);
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
                            gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                            gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                        entity.TargetDirection = uVar4;
                        entity.AIValues[1] = 0x50;
                        entity.Bytes[0] = 1;
                        return;
                    }

                    if (relativePositions[0] < 5 && relativePositions[1] < 5)
                    {
                        uVar4 = (uint)ScriptHelper.GetDirectionToTarget(
                            gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                            gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                        entity.TargetDirection = uVar4;
                        return;
                    }
                }

                if (entity.ForceAdjusted == 0)
                {
                    iVar5 = gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 1, 0x100000);

                    if (iVar5 == 0 && (entity.AIValues[1] & 0x3fU) == 0)
                    {
                        var rand = (Random.Next() * 3) >> 0x20;

                        if (rand == 0)
                        {
                            rand = ((Random.Next() * 9) >> 0x20) - 4;
                            entity.TargetDirection = (uint)((entity.TargetDirection + rand) & 0x1f);
                        }
                    }
                }
                else
                {
                    gameEngine.EntityGameplayManager.UpdateDirectionForced(entity, 1, 3, 0x100000);
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
                            gameEngine.EntityGameplayManager.StartFlying(entity, 1, 300, 0x14);
                            return;
                        }
                    }

                    entity.TargetAnimationId = 1;
                    uVar4 = (uint)ScriptHelper.GetDirectionToTarget(iVar7 * 0x180000, iVar6 * 0x100000);
                    uVar2 = ((Random.Next() * 9) >> 0x20) - 4;
                    entity.AIValues[1] = 0xf0;
                    entity.TargetDirection = (uint)((uVar4 + uVar2) & 0x1f);
                }
                else
                {
                    bVar1 = entity.Bytes[0];
                    entity.AIValues[1] = (short)(sVar3 + -1);

                    if (bVar1 != 0 && sVar3 == 0x47 &&
                        (gameEngine.StaticVariables.g_playerControlFlags & 4U) == 0)
                    {
                        gameEngine.SoundManager.PlaySoundEffect(0xca);
                    }
                }
            }
        }
        else if (uVar4 == 5 && entity.IsAboveGround != 0)
        {
            entity.TargetAnimationId = 1;
        }
    }

    //80065b0c
    public static void AI_FUN_80065b0c(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006b510
    public static void AI_FUN_8006b510(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006b848
    public static void AI_FUN_8006b848(GameEngine gameEngine, Entity entity)
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
            entity.TargetAnimationId = (uint)gameEngine.StaticVariables.g_scriptAnimationTable2[entity.Flags2];
        }

        entity.PlatformEntity.CarriedEntity = null;
        entity.PlatformEntity = null;
        entity.Flags = (entity.Flags | 0x30) & 0xffffff7f;//turn off bit 8, turn on bits 5 and 6
    }

    //8006b8cc
    public static void AI_FUN_8006b8cc(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006ce08
    public static void AI_FUN_8006ce08(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006e83c
    public static void AI_FUN_8006e83c(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006eb9c
    public static void AI_FUN_8006eb9c(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006f860
    public static void AI_FUN_8006f860(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006f8e4
    public static void AI_FUN_8006f8e4(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80073728
    public static void AI_FUN_80073728(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80074ae8
    public static void AI_FUN_80074ae8(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //800756ec
    public static void AI_FUN_800756ec(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80075a3c
    public static void AI_FUN_80075a3c(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007763c
    public static void AI_FUN_8007763c(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80077734
    public static void AI_FUN_80077734(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80078a5c
    public static void AI_FUN_80078a5c(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80078b54
    public static void AI_FUN_80078b54(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80079950
    public static void AI_FUN_80079950(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80079b14
    public static void AI_FUN_80079b14(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007a2f8
    public static void AI_FUN_8007a2f8(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007a4a8
    public static void AI_FUN_8007a4a8(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007a4b0
    public static void AI_FUN_8007a4b0(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007a680
    public static void AI_FUN_8007a680(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007a8a0
    public static void AI_UpdateIceProjectile(GameEngine gameEngine, Entity entity)
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

            var spriteEffect = gameEngine.EffectManager.CreateEffectEntity(
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

    //8007a958
    public static void AI_FUN_8007a958(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007a978
    public static void AI_FUN_8007a978(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007ac60
    public static void AI_FUN_8007ac60(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007b04c
    public static void AI_FUN_8007b04c(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007b1f0
    public static void AI_FUN_8007b1f0(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007b3c4
    public static void AI_FUN_8007b3c4(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007b6ec
    public static void AI_FUN_8007b6ec(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007b7b0
    //jar sandboxes
    public static void AI_FUN_8007b7b0(GameEngine gameEngine, Entity entity)
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
            entity.TargetAnimationId = (uint)gameEngine.StaticVariables.g_scriptAnimationTable3[entity.Flags2];
        }

        entity.PlatformEntity = null;
        entity.Flags = (entity.Flags | 0x34) & 0xffffff7f;//turn off bit 8, turn on bits 5 and 6
    }

    //8007b834
    public static void AI_FUN_8007b834(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007b998
    public static void AI_ProcessWarpTransitionState(GameEngine gameEngine, Entity entity)
    {
        int state = ReadWarpState(entity);
        uint idx = (uint)(state - 1);

        if (idx >= 6)
        {
            return;
        }

        string arg1;
        string arg2;

        switch (entity.Bytes[0])
        {
            case 1:

                if (gameEngine.IsDialogInProgress())
                {
                    ResetWarpState(gameEngine, entity);
                    return;
                }

                arg1 = gameEngine.EtcRes.GetEtcString(0x40);
                gameEngine.UIManager.InitializeDialogMessage(arg1, 1);
                gameEngine.SetEtcAnimationMode(4);

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
                gameEngine.StaticVariables.g_warpStatusFlag = 0;

                arg1 = gameEngine.EtcRes.GetEtcString(0x41);
                arg2 = gameEngine.EtcRes.GetEtcString(0x42);

                int r = gameEngine.InitializeAsyncOperation(arg1, arg2, result => gameEngine.StaticVariables.g_warpStatusFlag = (uint)result);

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
                    if (gameEngine.StaticVariables.g_warpStatusFlag == 0)
                    {
                        return;
                    }

                    // On essaye d’activer le "TextHold" (mise en pause du texte)
                    gameEngine.UIManager.TryActivateTextHoldState();

                    // si statut != 1 => Reset; sinon on attend 0x3C frames et on avance
                    if (gameEngine.StaticVariables.g_warpStatusFlag != 1)
                    {
                        ResetWarpState(gameEngine, entity);
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

                    gameEngine.UpdateSavedData();
                    WriteWarpState(entity, 6);
                    return;
                }

            // case 6
            case 6:
                {
                    if (gameEngine.StaticVariables.g_globalTransitionState != 0)
                    {
                        return;
                    }

                    ResetWarpState(gameEngine, entity);
                    return;
                }
        }
    }

    private static void ResetWarpState(GameEngine gameEngine, Entity entity)
    {
        WriteWarpState(entity, 0);
        gameEngine.StaticVariables.g_playerControlFlags &= 0xfffffffb;
    }

    private static int ReadWarpState(Entity e)
        => e.Bytes[0]
           | (e.Bytes[1] << 8)
           | (e.Bytes[2] << 16)
           | (e.Bytes[3] << 24);

    private static void WriteWarpState(Entity e, int v)
    {
        e.Bytes[0] = (byte)(v & 0xFF);
        e.Bytes[1] = (byte)((v >> 8) & 0xFF);
        e.Bytes[2] = (byte)((v >> 16) & 0xFF);
        e.Bytes[3] = (byte)((v >> 24) & 0xFF);
    }

    //8007bb30
    public static void AI_FUN_8007bb30(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007bb9c
    public static void AI_FUN_8007bb9c(GameEngine gameEngine, Entity entity)
    {
        switch ((int)entity.TargetAnimationId - 1)
        {
            case 1:
                {
                    if ((entity.FrameCounter & 3) != 0)
                    {
                        break;
                    }

                    var effect = gameEngine.EffectManager.CreateEffectEntity( 0, 0x0E, 0, entity.PosX, entity.PosY, entity.PosZ);
                    if (effect == null)
                    {
                        break;
                    }

                    {
                        var randForceY = Random.Next();
                        var randForceX = (Random.Next() * 0x30001) >> 0x20;
                        effect.ForceX = (int)(randForceX - 0x18000);
                        effect.ForceY = (int)(((randForceY * 0x20001) >> 0x20) - 0x10000);
                        effect.ForceZ = 0x20000;
                    }
                    break;
                }

            case 2:
                {
                    entity.ForceZ = ((int)entity.LastTargetDirection - entity.PosZ) >> 1;

                    if (gameEngine.StaticVariables.g_entitySlots[0].RidingEntity == entity)
                    {
                        entity.ForceZ = -0x8000;
                        entity.TargetAnimationId = 3;
                    }
                    break;
                }

            case 3:
                {
                    if (gameEngine.StaticVariables.g_entitySlots[0].RidingEntity != entity)
                    {
                        entity.TargetAnimationId = 2;
                    }
                    break;
                }

            case 4:
                {
                    if (gameEngine.StaticVariables.g_entitySlots[0].RidingEntity == entity)
                    {
                        break;
                    }

                    if (gameEngine.StaticVariables.g_entitySlots[0].ForceZ > 0)
                    {
                        gameEngine.StaticVariables.g_entitySlots[0].ForceZ = 0x00098000;
                        entity.TargetAnimationId = 5;
                    }
                    else
                    {
                        entity.TargetAnimationId = 2;
                    }
                    break;
                }

            default:
                {
                    entity.ForceZ = ((int)entity.LastTargetDirection - entity.PosZ) >> 1;
                    break;
                }
        }
    }

    //8007bd8c
    public static void AI_FUN_8007bd8c(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007c024
    public static void AI_FUN_8007c024(GameEngine gameEngine, Entity entity)
    {
        //Debugger.Break();

        if (gameEngine.StaticVariables.g_entitySlots[0].XCollisionEntity == entity)
        {
            if (entity.Bytes[0] < 0x1f)
            {
                entity.Bytes[0] = entity.Bytes[1];
                return;
            }

            gameEngine.SoundManager.PlaySoundEffect(0x19);
            entity.TargetAnimationId = 1;
            var result = FUN_8003ac9c(entity, gameEngine.StaticVariables.g_entitySlots[0]);

            if (result != -1)
            {
                entity.TargetDirection = (uint)((result + 0x10U) & 0x1f);
                return;
            }
        }

        if (entity.TargetAnimationId == 1)
        {
            gameEngine.SoundManager.PlaySoundEffect(0x19);
        }

        entity.Bytes[0] = 0;
        entity.Bytes[1] = 0;
        entity.Bytes[2] = 0;
        entity.Bytes[3] = 0;
        entity.TargetAnimationId = 0;
    }

    //8003ac9c
    private static int FUN_8003ac9c(Entity entity1, Entity entity2)
    {
        int diffX;

        if (entity2.XCollisionEntity != entity1)
        {
            return -1;
        }

        diffX = entity2.ModdedPosX - entity1.ModdedPosX;

        if (diffX < 0)
        {
            if (-diffX <= entity2.Width)
            {
                //goto LAB_8003acf4;

                return ((entity2.PosY < entity1.PosY) ? 1 : 0) << 4;
            }
        }
        else if (diffX <= entity1.Width)
        {
            LAB_8003acf4:
            return ((entity2.PosY < entity1.PosY) ? 1 : 0) << 4;
        }

        diffX = 0x18;

        if (entity2.PosX < entity1.PosX)
        {
            diffX = 8;
        }

        return diffX;
    }

    //8007c0d8
    public static void AI_FUN_8007c0d8(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007c768
    public static void AI_FUN_8007c768(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007d554
    public static void AI_FUN_8007d554(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006a564
    public static void AI_FUN_8006a564(GameEngine gameEngine, Entity entity)
    {
        //Debugger.Break();

        byte bVar1;
        bool bVar2;
        short sVar3;
        uint direction;
        int z;
        uint rand;
        int[] positions = new int[6];
        uint local_c;

        FUN_8007fe8c(gameEngine, entity, positions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.Bytes[0] == 0)
                {
                    sVar3 = entity.AIValues[1];
                    if (sVar3 == 0)
                    {
                        if ((positions[0] < 3) && ((positions[1] < 3 && (positions[2] < 0x200001))))
                        {
                            direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.g_entitySlots[0].PosX - entity.PosX,
                                    gameEngine.StaticVariables.g_entitySlots[0].PosY - entity.PosY);
                            entity.TargetDirection = direction;
                            entity.AIValues[1] = 0x3c;
                            return;
                        }

                        entity.TargetDirection = (uint)(Random.Next() % 32);
                        z = entity.PosZ + (entity.AIValues[0] * -0x10000);
                        rand = (uint)(((Random.Next() * 0x29) >> 0x20) + 0x50);
                        entity.AIValues[1] = (short)(rand);

                        if (0x4fffff < z)
                        {
                            LAB_8006a7dc:
                            entity.TargetAnimationId = 3;
                            return;
                        }
                        if (-0x500000 < z)
                        {
                            var next = (uint)Random.Next();
                            entity.TargetAnimationId = (uint)(((next * 3ul) >> 0x20) + 1ul);
                            return;
                        }
                    }
                    else
                    {
                        entity.AIValues[1] = (short)(sVar3 - 1);
                        if (sVar3 != 1)
                        {
                            return;
                        }

                        entity.AIValues[1] = 0x78;

                        if (0xfffff < positions[5])
                        {
                            if ((positions[5] < 0x180001) || (entity.IsAboveGround != 0))
                            {
                                //goto LAB_8006a7e4;
                                entity.TargetAnimationId = 1;
                                return;
                            }

                            //goto LAB_8006a7dc;
                            entity.TargetAnimationId = 3;
                            return;
                        }
                    }
                }
                else
                {
                    direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.g_entitySlots[0].PosX - entity.PosX,
                        gameEngine.StaticVariables.g_entitySlots[0].PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    bVar1 = entity.Bytes[0];
                    entity.AIValues[1] = 0x3c;
                    entity.Bytes[0] = (byte)(bVar1 - 1);

                    if (0xfffff < positions[5])
                    {
                        if (positions[5] < 0x180001)
                        {
                            LAB_8006a7e4:
                            entity.TargetAnimationId = 1;
                            return;
                        }

                        //goto LAB_8006a7dc;
                        entity.TargetAnimationId = 3;
                        return;
                    }
                }
                entity.TargetAnimationId = 2;
                break;

            case 1:
                sVar3 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar3;

                if (sVar3 == 0)
                {
                    entity.TargetAnimationId = 0;
                }

                if (entity.Bytes[0] != 0)
                {
                    return;
                }

                if (!gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, positions, 1, 4, 0x200000))
                {
                    if (entity.ForceAdjusted == 0)
                    {
                        return;
                    }

                    entity.TargetAnimationId = 0;
                    entity.TargetDirection = (entity.TargetDirection + 4) & 0x1f;
                }

                //goto LAB_8006a8e8;
                entity.TargetAnimationId = 0;
                direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.g_entitySlots[0].PosX - entity.PosX,
                    gameEngine.StaticVariables.g_entitySlots[0].PosY - entity.PosY);
                entity.TargetDirection = direction;
                entity.AIValues[1] = 0;
                entity.Bytes[0] = 10;
                break;

            case 2:
                sVar3 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar3;

                if (sVar3 == 0)
                {
                    entity.TargetAnimationId = 0;
                }
                goto LAB_8006a8b8;

            case 3:
                sVar3 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar3;

                if ((sVar3 == 0) || (entity.IsAboveGround != 0))
                {
                    entity.TargetAnimationId = 0;
                }

                LAB_8006a8b8:
                if ((entity.Bytes[0] == 0) 
                    && gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, positions, 1, 4, 0x200000))
                {
                    LAB_8006a8e8:
                    entity.TargetAnimationId = 0;
                    direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.g_entitySlots[0].PosX - entity.PosX,
                        gameEngine.StaticVariables.g_entitySlots[0].PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.AIValues[1] = 0;
                    entity.Bytes[0] = 10;
                }
                break;

            case 5:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[3] == 0)
                    {
                        entity.TargetAnimationId = 0;
                        entity.Bytes[0] = 0;
                        entity.AIValues[1] = 0;
                    }
                    else
                    {
                        entity.TargetAnimationId = 6;
                        entity.Flags = entity.Flags | 0x40;
                    }
                }

                break;
        }
    }

    private static void FUN_8007fe8c(GameEngine gameEngine, Entity entity, int[] positions)
    {
        int x;
        int z;
        int y;

        x = entity.TileX - gameEngine.StaticVariables.g_entitySlots[0].TileX;
        y = entity.TileY - gameEngine.StaticVariables.g_entitySlots[0].TileY;
        z = entity.PosZ - gameEngine.StaticVariables.g_entitySlots[0].FloorHeight;
        positions[3] = x;

        if (x < 0)
        {
            x = -x;
        }

        positions[4] = y;

        if (y < 0)
        {
            y = -y;
        }

        positions[5] = z;

        if (z < 0)
        {
            z = -z;
        }

        positions[0] = x;
        positions[1] = y;
        positions[2] = z;
    }

    // Placeholder methods for AI functions
    public static void AI_UpdateEntityAI_0(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_3(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_4(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_5(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_6(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_6_2(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_7(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_8(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_8_2(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_9(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_10(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_11(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006a974
    public static void AI_UpdateEntityAI_12(GameEngine gameEngine, Entity entity)
    {
        //Debugger.Break();

        int iVar1;
        uint targetAnimationId;
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);
        targetAnimationId = entity.TargetAnimationId;

        if (targetAnimationId == 1)
        {
            if (relativePositions[0] < 3 
                && relativePositions[1] < 3
                 && relativePositions[2] < 0x100001)
            {
                entity.Bytes[0] = 1;
                entity.TargetAnimationId = 0;
                entity.AIValues[1] = (short)(((Random.Next() * 0x10) >> 0x20) + 0x3c);
                return;
            }

            if (entity.ForceAdjusted == 0 && CanMoveForward(entity, 0) == 0)
            {
                return;
            }

            var rand = Random.Next();
            entity.TargetAnimationId = 0;
            entity.AIValues[1] = 0x3c;
            entity.TargetDirection = (uint)((rand * 0x20) >> 0x20);
            return;
        }

        if ((int)targetAnimationId < 2)
        {
            if (targetAnimationId != 0)
            {
                return;
            }

            if (entity.AIValues[1] != 0)
            {
                entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                return;
            }

            if (entity.Bytes[0] == 0)
            {
                entity.TargetAnimationId = 1;
                return;
            }

            targetAnimationId = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.g_entitySlots[0].PosX - entity.PosX, gameEngine.StaticVariables.g_entitySlots[0].PosY - entity.PosY);
            entity.TargetDirection = targetAnimationId;
            entity.TargetAnimationId = 3;
        }
        else
        {
            if (targetAnimationId == 5)
            {
                if (entity.IsAboveGround == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 0;
                entity.AIValues[1] = 0x14;
                gameEngine.SoundManager.PlaySoundEffect(0x96);
                return;
            }

            if (targetAnimationId != 7)
            {
                return;
            }

            if (entity.ForceResetAnimationFlag == 0)
            {
                return;
            }

            if (entity.Bytes[3] != 0)
            {
                entity.TargetAnimationId = 8;
                entity.Flags |= 0x40;
                return;
            }

            entity.TargetAnimationId = 0;
            entity.AIValues[1] = 0x14;
        }

        entity.Bytes[0] = 0;
    }

    private static int CanMoveForward(Entity entity, int zOffset)
    {
        int result;
        int floorToMapHeight3;
        int adjustedZOffset;
        int floorToMapHeight1;
        int floorToMapHeight2;
        int floorToMapHeight0;

        adjustedZOffset = zOffset + 1;
        floorToMapHeight3 = entity.FloorHeight;
        floorToMapHeight0 = floorToMapHeight3 - entity.MapHeights[0];
        floorToMapHeight1 = floorToMapHeight3 - entity.MapHeights[1];
        floorToMapHeight2 = floorToMapHeight3 - entity.MapHeights[2];
        floorToMapHeight3 = floorToMapHeight3 - entity.MapHeights[3];
        var checkSouthConditions = true;

        if (adjustedZOffset < floorToMapHeight0)
        {
            checkSouthConditions = false;

            if (adjustedZOffset < floorToMapHeight1)
            {
                if (entity.TargetDirection - 9 < 0xf)
                {
                    return 1;
                }

                checkSouthConditions = true;
            }
        }

        if (checkSouthConditions)
        {
            CheckSouthConditions:
            if (adjustedZOffset < floorToMapHeight1)
            {
                if (floorToMapHeight3 <= adjustedZOffset) goto SkipAllChecks;

                if (entity.TargetDirection - 0x11 < 0xf)
                {
                    return 1;
                }
            }
        }

        if (adjustedZOffset < floorToMapHeight3)
        {
            if (floorToMapHeight2 <= adjustedZOffset)
            {
                return 0;
            }

            if (0x10 < entity.TargetDirection - 8)
            {
                return 1;
            }
        }

        SkipAllChecks:
        result = 0;
        
        if (floorToMapHeight2 <= adjustedZOffset 
            || (adjustedZOffset < floorToMapHeight0 && 0xf < entity.TargetDirection))
        {
            result = 1;
        }

        return result;
    }

    public static void AI_UpdateEntityAI_13(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_14(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_15(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_17(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_18(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_19(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_20(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_20_2(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_21(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_22(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_23(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_23_2(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_Boss(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateBossEntityState(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_Boos(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_Spec(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityIA_Watc(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_Twin(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityAI_Warp(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateLoaderBossAI(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityIA_Fire(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_ApplyMatchingEntity(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateHomingProject(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_ApplyZGravityIfIdle(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateFollowerBehaviour(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    public static void AI_UpdateEntityDelayed(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80061eb8
    public static void AI_SpawnWarpIfValid(GameEngine gameEngine, Entity entity)
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
                gameEngine.DestroyEntity(entity, -1);
                return;
            }

            if (entity.IsAboveGround != 0)
            {
                gameEngine.SoundManager.PlaySoundEffect(0x19D);
                entity.Flags |= 0x40;
                entity.TargetAnimationId = 0x9;
                gameEngine.TriggerScreenEffect(unchecked(0x60000000), 2, 0, 1);
                return;
            }

            if (entity.Bytes[0] == 0x1 && aiState == 0x0F)
            {
                int parentZ = parentEntity.PosZ;
                int selfZ = entity.PosZ;

                if (selfZ >= parentZ + 0x01000000)
                {
                    var seed1 = Random.Next();
                    var rand1 = Random.Next();
                    var rand2 = Random.Next();

                    entity.TargetAnimationId = 10;
                    entity.Flags |= 0x100;

                    var randClamped = (int)((seed1 * 0x18) >> 0x20);
                    entity.PosX = randClamped * 0xc0000 + (int)((rand1 * 7) >> 0x20) * 0x10000 + 0x1e00000;

                    randClamped = (int)((rand2 * 5) >> 0x20);
                    entity.PosY = (int)((rand2 * 0x10) >> 0x20) * 0x80000 + randClamped * 0x10000 + 0x2800000;
                }
            }

            if (entity.Bytes[0] != 0x2)
            {
                return;
            }

            {
                var idx = entity.TargetDirection;
                short offX = gameEngine.StaticVariables.g_offsetXList[idx];
                int scale = entity.Bytes[1] << 11; // * 0x800
                int val = offX * scale;

                const int MAGIC = unchecked(0x2E8BA2E9);
                int hi = (val * MAGIC) >> 32;
                int v0 = (hi >> 1) - (val >> 31);
                entity.PreviousAdjustedForceX = v0;

                short offY = gameEngine.StaticVariables.g_offsetYList[idx];
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
        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);
        int slotIndex = entity.Bytes[0];
        WarpSlotState warpSlot = gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[slotIndex];

        if (parentEntity.Hp == 0)
        {
            entity.TargetAnimationId = 0x7;
            entity.Flags |= 0x40;

            if (warpSlot.Phase != 0)
            {
                return;
            }

            gameEngine.StaticVariables.g_entitySlots[0].PosX = entity.PosX;
            gameEngine.StaticVariables.g_entitySlots[0].PosY = entity.PosY;
            gameEngine.StaticVariables.g_entitySlots[0].TargetAnimationId = 0;
            gameEngine.StaticVariables.g_playerControlFlags = (uint)(gameEngine.StaticVariables.g_playerControlFlags & ~0x20);

            warpSlot.Phase = 0;
            return;
        }

        if (warpSlot.Phase == 0x2)
        {
            Entity player = gameEngine.StaticVariables.g_entitySlots[0];

            if (player.IsAboveGround != 0)
            {
                if (player.TargetAnimationId == 0x1C)
                {
                    if (player.ForceResetAnimationFlag != 0)
                    {
                        // if entity.0x276 == 0: start scrolling lock + spawn warp
                        if (entity.Bytes[2] == 0)
                        {
                            gameEngine.StaticVariables.g_scrollingParameters.Flag = 1;
                            gameEngine.StaticVariables.g_scrollingParameters.SpeedX = 1;
                            gameEngine.StaticVariables.g_scrollingParameters.SpeedY = 1;
                            gameEngine.StaticVariables.g_scrollingParameters.LimitX = 2; // d’après rand1=2 ici
                            gameEngine.StaticVariables.g_scrollingParameters.LimitY = 2;

                            var a2 = entity.SpriteTableIndex - 0x100;
                            int x = player.PosX;
                            int y = player.PosY;
                            int z = player.PosZ;

                            Entity spawned = gameEngine.SpawnWarpEntity(entity, 1, a2, x, y, z, 0);

                            gameEngine.StaticVariables.g_entitySpawned = spawned;

                            spawned.TargetAnimationId = 0x11;
                            spawned.SpriteProgramIndexes[2] = 0;
                            spawned.Flags |= 0x2;
                            spawned.Flags = (uint)(spawned.Flags & ~0x80);

                            player.AnimFlags &= ~0x40;
                            entity.Bytes[2] = 1;
                        }
                        else
                        {
                            if (player.DamagedTickCounter != 0)
                            {
                                if (gameEngine.StaticVariables.g_entitySpawned != null)
                                {
                                    gameEngine.DestroyEntity(gameEngine.StaticVariables.g_entitySpawned, 0);
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
                        gameEngine.StaticVariables.g_scrollingParameters.Flag = 0;
                        gameEngine.StaticVariables.g_playerControlFlags = (uint)(gameEngine.StaticVariables.g_playerControlFlags & ~0x20);

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
            int baseX = gameEngine.StaticVariables.INT_ARRAY_80026cdc[slotIndex];
            warpSlot.BaseY = 0x02A00000;
            warpSlot.BaseX = baseX;
            entity.Bytes[1] = 1;
        }

        // switch(entity.0x88) : énorme state machine
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

                    int add = gameEngine.StaticVariables.INT_ARRAY_80026cdc[slotIndex + 2];
                    entity.DelayOrAngle = (entity.DelayOrAngle + add) & 0x1FF;

                    short s = gameEngine.StaticVariables.g_sinus[entity.DelayOrAngle];
                    int sinTerm = ((s * 9) << 9);
                    entity.PosX = warpSlot.BaseX + sinTerm;

                    short c = gameEngine.StaticVariables.g_cosinus[entity.DelayOrAngle];
                    int cosTerm = ((c * 9) << 9);
                    entity.PosY = warpSlot.BaseY + cosTerm;

                    warpSlot.SavedX = entity.PosX;
                    warpSlot.SavedY = entity.PosY;

                    if ((gameEngine.StaticVariables.g_globalFlags[0] & 0x2) != 0)
                    {
                        if (relativePositions[1] < 3 && relativePositions[0] < 3 && entity.AIValues[1] == 0)
                        {
                            int other = slotIndex ^ 1;
                            if (gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[other].Phase == 0)
                            {
                                entity.TargetAnimationId = 0x10;
                                warpSlot.PlayerX = gameEngine.StaticVariables.g_entitySlots[0].PosX;
                                warpSlot.PlayerY = gameEngine.StaticVariables.g_entitySlots[0].PosY;
                            }
                        }
                    }
                    break;
                }

            // --------------------------------------------------------------------
            // case 2 (80062798...)  clamp + si fini . recalc base & timer
            // --------------------------------------------------------------------
            case 2:
                {
                    // if (PosZ < 0x08000000) warpMoveMode=0
                    // (ton ASM fait un test via add + sltu, résultat: si <= seuil . b8=0)
                    // Je traduis littéralement “si magnitude dans une fenêtre => b8=0”
                    // . garde ton exact si tu retrouves le seuil.
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
                        // slot.BaseX = INT_ARRAY_80026cdc[s4] + (entity.targetX - slot.SavedX) + slot.SavedX;
                        // => en réalité: slot.BaseX = INT_ARRAY_80026cdc[s4] + entity.targetX
                        // mais je garde l’intention originale:
                        warpSlot.BaseX = gameEngine.StaticVariables.INT_ARRAY_80026cdc[aiState] + (entity.PosX - warpSlot.SavedX) + warpSlot.SavedX;
                        warpSlot.BaseY = (entity.PosY - warpSlot.SavedY) + (int)0x2A000000u + warpSlot.SavedY;
                        entity.TargetAnimationId = 0;
                    }

                    goto default;
                }

            case 3:
                {
                    var index = entity.Bytes[0];
                    var timer = entity.AIValues[1];
                    if (timer == 0)
                    {
                        var gotoLAB_800625d0 = false;
                        var gotoLAB_80062624 = false;
                        var gotoLAB_80062678 = false;

                        var diffX = gameEngine.StaticVariables.g_entitySlots[0].ModdedPosX - entity.ModdedPosX;

                        if (diffX < 0)
                        {
                            if (entity.ModdedPosX - gameEngine.StaticVariables.g_entitySlots[0].ModdedPosX < gameEngine.StaticVariables.g_entitySlots[0].Width + 1)
                            {
                                //goto LAB_800625d0;
                                gotoLAB_800625d0 = true;
                            }
                        }
                        
                        if ((diffX < 0 && diffX < entity.Width + 1) || gotoLAB_800625d0)
                        {
                            LAB_800625d0:
                            diffX = gameEngine.StaticVariables.g_entitySlots[0].ModdedPosY - entity.ModdedPosY;

                            if (diffX < 0)
                            {
                                if (entity.ModdedPosY - gameEngine.StaticVariables.g_entitySlots[0].ModdedPosY < gameEngine.StaticVariables.g_entitySlots[0].Height + 1)
                                {
                                    //goto LAB_80062624;
                                    gotoLAB_80062624 = true;
                                }
                            }
                            
                            if ((diffX < 0 && diffX < entity.Height + 1) || gotoLAB_80062624)
                            {
                                LAB_80062624:
                                diffX = gameEngine.StaticVariables.g_entitySlots[0].ModdedPosZ - entity.ModdedPosZ;

                                if (diffX < 0)
                                {
                                    if (entity.ModdedPosZ - gameEngine.StaticVariables.g_entitySlots[0].ModdedPosZ < gameEngine.StaticVariables.g_entitySlots[0].Depth + 1)
                                    {
                                        //goto LAB_80062678;
                                        gotoLAB_80062678 = true;
                                    }
                                }
                                
                                if ((diffX < 0 && diffX < entity.Depth + 1) || gotoLAB_80062678)
                                {
                                    LAB_80062678:
                                    if ((((gameEngine.StaticVariables.g_entitySlots[0].AnimFlags & 0x40U) == 0)
                                        && (gameEngine.StaticVariables.g_entitySlots[0].DamagedTickCounter == 0))
                                        && (gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[index ^ 1].Phase == 0))
                                    {
                                        entity.TargetAnimationId = 4;
                                        entity.ForceZ = 0x20000;
                                        entity.Flags &= 0xfffffffe;
                                        diffX = gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[index].BaseX - entity.PosX;

                                        if (diffX < 0)
                                        {
                                            diffX += 0x1f;
                                        }

                                        var iVar2 = gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[index].BaseY;
                                        warpSlot.A0 = diffX >> 5;
                                        iVar2 -= entity.PosY;

                                        if (iVar2 < 0)
                                        {
                                            iVar2 += 0x1f;
                                        }

                                        gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[index].A1 = iVar2 >> 5;
                                        gameEngine.StaticVariables.g_entitySlots[0].TargetAnimationId = 0x56;
                                        gameEngine.StaticVariables.g_playerControlFlags |= 0x20;
                                        gameEngine.StaticVariables.g_entitySlots[0].Flags &= 0xfffffef7;
                                        gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[index].Phase = 1;
                                        break;
                                    }
                                }
                            }
                        }

                        if ((entity.CollidedWithEntityZ != 0) || (entity.PosZ < 0xa00001))
                        {
                            entity.AIValues[1] = 0x14;
                            entity.ForceZ = 0;
                            gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[index].A1 = 0;
                            warpSlot.A0 = 0;
                        }
                    }
                    else if (timer != 0)
                    {
                        timer--;
                        entity.AIValues[1] = timer;
                        
                        if (timer == 1)
                        {
                            break;
                        }
                    }

                    entity.TargetAnimationId = 0x2;
                    //Debugger.Break();

                    // ... le dump continue avec LAB_800629c8 etc.
                    // (le reste suit la même logique; si tu veux, je te génère aussi la suite
                    //  en gardant exactement tous les cases 0x2..0xF.)

                    //goto LAB_800629c8;
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


                    //entity.AIValues[1] = 0x14;
                    //entity.ForceZ = 0;
                    //warpSlot.A1 = 0;
                    //warpSlot.A0 = 0;
                    //goto default;
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
                    // . ici ton ASM déclenche une transition: flags, anim, copie positions dans g_entitySlots[0], etc.
                    if (entity.ForceZ == 0 && warpSlot.A0 == 0 && warpSlot.A1 == 0)
                    {
                        // 80062934.. : set warpSubState=5, set flag bit0 dans Flags, IsActive=2, init entitySlots[0]
                        entity.TargetAnimationId = 5;
                        entity.Flags |= 1u;
                        warpSlot.Phase = 2;

                        gameEngine.StaticVariables.g_entitySlots[0].TargetAnimationId = 0x31;
                        gameEngine.StaticVariables.g_entitySlots[0].TargetDirection = 0;
                        gameEngine.StaticVariables.g_entitySlots[0].Flags |= 0x108u;
                        gameEngine.StaticVariables.g_entitySlots[0].PosX = entity.PosX;
                        gameEngine.StaticVariables.g_entitySlots[0].PosY = entity.PosY;
                    }

                    // dans tous les cas, case 4 finit par:
                    // g_entitySlots[0].PreviousAdjustedForceX = slot.A0; PreviousAdjustedForceY = slot.A1; g_entitySlots[0].ForceZ = parent.ForceZ
                    gameEngine.StaticVariables.g_entitySlots[0].PreviousAdjustedForceX = warpSlot.A0;
                    gameEngine.StaticVariables.g_entitySlots[0].PreviousAdjustedForceY = warpSlot.A1;
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
                    int angle = entity.DelayOrAngle & 0x1FF;

                    {
                        int sinv = gameEngine.StaticVariables.g_sinus[angle];
                        int tmp = (sinv << 3) + sinv;
                        tmp <<= 9;
                        entity.PosX = warpSlot.BaseX + tmp;
                    }

                    entity.PosZ = unchecked((int)0xE0000000);

                    {
                        int cosv = gameEngine.StaticVariables.g_cosinus[angle];
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
        gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[slotIndex] = warpSlot;
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
    public static void AI_Melzas2_FinalBoss(GameEngine gameEngine, Entity entity)
    {
        AI_Melzas2.AI_Melzas2_FinalBoss(gameEngine, entity);
    }

    //80062bc0
    public static void AI_UpdateMelzas2CutsceneChannels(GameEngine gameEngine, Entity entity)
    {
        AI_Melzas2.AI_UpdateMelzas2CutsceneChannels(gameEngine, entity);
    }

    //800637d8
    public static void AI_UpdateEntityAI_IdleLookAround(GameEngine gameEngine, Entity entity)
    {
        ulong rand;
        uint direction;
        ushort uVar1;
        int verticalDelta;
        int[] relativePositions = new int[6];
        ulong seed;

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

        if (entity.TargetAnimationId != 2)
        {
            verticalDelta = entity.ModdedPosX - gameEngine.StaticVariables.PlayerEntity.HitBoxX;

            if (verticalDelta < 0)
            {
                if (entity.Width + 1 <= gameEngine.StaticVariables.PlayerEntity.HitBoxX - entity.ModdedPosX)
                {
                    goto ExitIdleCheck;
                }
            }
            else if (gameEngine.StaticVariables.PlayerEntity.CollisionWidth + 1 <= verticalDelta)
            {
                goto ExitIdleCheck;
            }

            verticalDelta = entity.ModdedPosY - gameEngine.StaticVariables.PlayerEntity.HitBoxY;

            if (verticalDelta < 0)
            {
                if (entity.Height + 1 <= gameEngine.StaticVariables.PlayerEntity.HitBoxY - entity.ModdedPosY)
                {
                    goto ExitIdleCheck;
                }
            }
            else if (gameEngine.StaticVariables.PlayerEntity.CollisionDepth + 1 <= verticalDelta)
            {
                goto ExitIdleCheck;
            }

            verticalDelta = entity.ModdedPosZ - gameEngine.StaticVariables.PlayerEntity.HitBoxZ;

            /*if (verticalDelta < 0)
            {
                if (gameEngine.StaticVariables.PlayerEntity.HitBoxZ - entity.ModdedPosZ < entity.Depth + 1)
                {
                    //goto TriggerLookAround;
                }
            }
            else*/
            if ((verticalDelta < 0 && gameEngine.StaticVariables.PlayerEntity.HitBoxZ - entity.ModdedPosZ < entity.Depth + 1)
                || verticalDelta < gameEngine.StaticVariables.PlayerEntity.CollisionHeight + 1)
            {
            TriggerLookAround:
                gameEngine.SoundManager.PlaySoundEffect(0x1d6);
                entity.Bytes[0] = 2;
                entity.TargetAnimationId = 2;
                direction = (uint)ScriptHelper.GetDirectionToTarget(
                    entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX,
                    entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
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
               (gameEngine.StaticVariables.g_playerControlFlags & 4U) == 0)
            {
                gameEngine.SoundManager.PlaySoundEffect(0x1d6);
            }

            if (entity.AIValues[5] == 0)
            {
                seed = Random.Next();
                entity.AIValues[5] = 1;
                rand = ((seed * 0xb) >> 0x20) + 0x19;
            }
            else
            {
                seed = Random.Next();
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
                        entity.TargetAnimationId = 1;
                        var rand2 = (Random.Next() * 3) >> 0x20;

                        if (rand2 == 0)
                        {
                            entity.TargetDirection = (uint)((Random.Next() * 0x20) >> 0x20);
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
                        var rand3 = (Random.Next() * 3) >> 0x20;

                        if (rand3 == 0)
                        {
                            entity.TargetDirection += (uint)((((Random.Next() * 0xd) >> 0x20) - 6) & 0x1f);
                        }
                    }

                    if (entity.ForceAdjusted != 0)
                    {
                        entity.Bytes[0] = (byte)entity.TargetAnimationId;
                        gameEngine.EntityGameplayManager.UpdateDirectionForced(entity, entity.Bytes[0], 3, 0x200000);

                        if (entity.TargetAnimationId == entity.Bytes[0])
                        {
                            var rand4 = (Random.Next() * 2) >> 0x20;

                            if (rand4 != 0)
                            {
                                entity.TargetDirection = (entity.TargetDirection + 0x10) & 0x1f;
                            }
                        }
                        else
                        {
                            gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, entity.Bytes[0], 0x400000);
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
    public static void AI_UpdateEntityTriggerWarpBehavior(GameEngine gameEngine, Entity entity)
    {
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

        if (entity.TargetAnimationId == 0)
        {
            if (relativePositions[0] < 4 && relativePositions[1] < 4 && relativePositions[5] < 1)
            {
                if ((gameEngine.StaticVariables.g_playerControlFlags & 4U) == 0)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x1d4);
                }

                entity.TargetAnimationId = 1;
                var rand = ((Random.Next() * 7) >> 0x20) + 5;
                entity.TargetDirection = (uint)rand;
            }
        }
        else if (0x3bfffff < entity.PosZ)
        {
            gameEngine.DestroyEntity(entity);
        }
    }

    //80063db4
    public static void AI_UpdateEntityAI_IdleCurious(GameEngine gameEngine, Entity entity)
    {
        byte remainingCycles;
        short rand;
        int deltaX;
        uint currentAnimId;
        int deltaY;
        int[] relativePos = new int[6];
        ulong seed;

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePos);

        if ((int)entity.TargetAnimationId < 10)
        {
            entity.TargetAnimationId = 10;
            entity.Flags = (entity.Flags & 0xfff8ff7fU) | 0x10000;
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
                    if (relativePos[0] < 9 && relativePos[1] < 9 && (gameEngine.StaticVariables.g_playerControlFlags & 4U) == 0)
                    {
                        gameEngine.SoundManager.PlaySoundEffect(0x192);
                    }

                    if (entity.AIValues[5] == 0)
                    {
                        seed = Random.Next();
                        entity.AIValues[5] = 1;
                        rand = (short)((short)((seed * 0xb) >> 0x20) + 0xf);
                    }
                    else
                    {
                        seed = Random.Next();
                        entity.AIValues[5] = 0;
                        rand = (short)((short)((seed * 0x3d) >> 0x20) + 100);
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
                            entity.Bytes[0] = (byte)(((Random.Next() * 3) >> 0x20) + 3);
                        }
                        else
                        {
                            deltaX = (int)((Random.Next() * 4) >> 0x20);
                        }

                        if (deltaX == 0)
                        {
                            entity.TargetDirection = (uint)(Random.Next() % 32);
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
                        var rand2 = (Random.Next() * 5) >> 0x20;

                        if (rand2 == 0 
                            && 0x31 < (ushort)entity.AIValues[1] 
                            && (ushort)entity.AIValues[1] < 0x3d 
                            && relativePos[0] < 9 && relativePos[1] < 9)
                        {
                            gameEngine.SoundManager.PlaySoundEffect(0x192);
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
                        deltaX = entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX;
                        deltaY = entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY;
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
    //Item
    public static void FUN_8007c174(GameEngine gameEngine, Entity entity)
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
                if (gameEngine.CdManager.FUN_8005a7d4())
                {
                    return;
                }

                entity.ItemState += 1;
                gameEngine.SetEtcAnimationMode(3);
                gameEngine.SoundManager.StopAllSound();
                return;
            }

            if (itemState < 2)
            {
                if (itemState != 0)
                {
                    return;
                }

                entity.ForceZ = 0;

                if (gameEngine.IsDialogInProgress())
                {
                    return;
                }

                if (entity.AIValues[2] == 0)
                {
                    entity.ItemState += 2;
                    gameEngine.StaticVariables.g_dropItemTextBuffer = gameEngine.EtcRes.GetOtherString(0x4e);
                    gameEngine.StaticVariables.g_dropItemTextBuffer += gameEngine.EtcRes.GetItemName((int)itemId);
                    gameEngine.StaticVariables.g_dropItemTextBuffer += gameEngine.EtcRes.GetOtherString(0x4f);

                    LAB_8007c320:
                    gameEngine.UIManager.InitializeDialogMessage(gameEngine.StaticVariables.g_dropItemTextBuffer, 1);
                    return;
                }

                gameEngine.StaticVariables.g_dropItemTextBuffer = gameEngine.EtcRes.GetOtherString(0x4c);
                gameEngine.StaticVariables.g_dropItemTextBuffer += gameEngine.EtcRes.GetItemName((int)itemId);
                gameEngine.StaticVariables.g_dropItemTextBuffer += gameEngine.EtcRes.GetOtherString(0x4d);

                if (entity.AIValues[4] == 0)
                {
                    entity.ItemState += 2;
                    Debugger.Break();
                    soundSfxIndex = gameEngine.StaticVariables.g_itemDropProperties[itemId].SoundSfxIndex; //itemId * 8 + 5

                    if (soundSfxIndex == 0)
                    {
                        gameEngine.StaticVariables.g_dropItemTextBuffer += gameEngine.EtcRes.GetOtherString(0x46);
                    }

                    gameEngine.SoundManager.PlaySoundEffect((uint)soundSfxIndex);
                    //goto LAB_8007c320;
                    gameEngine.UIManager.InitializeDialogMessage(gameEngine.StaticVariables.g_dropItemTextBuffer, 1);
                    return;
                }

                entity.ItemState += 1;
                //goto LAB_8007c684;
                gameEngine.UIManager.InitializeDialogMessage(gameEngine.StaticVariables.g_dropItemTextBuffer, 1);
                gameEngine.SetEtcAnimationMode(4);
                return;
            }

            if (itemState != 2)
            {
                return;
            }

            if (gameEngine.IsDialogInProgress())
            {
                return;
            }

            if (entity.AIValues[2] == 0)
            {
                entity.AIValues[4] = 0;
            }
            else
            {
                gameEngine.PlayerManager.FUN_80033dbc(gameEngine.StaticVariables.PlayerEntity, itemId);
                gameEngine.FUN_80032b28((uint)entity.ContentsGameFlag);
            }

            entity2 = gameEngine.StaticVariables.g_entitySlots[entity.AIValues[0]];
        }
        else
        {
            if (entity.IsAboveGround != 0 && entity.AIValues[2] != 0)
            {
                var value = (entity.AIValues[2] * 0xc) >> 4;
                entity.AIValues[2] = (short)value;

                if (value <= gameEngine.CurrentMap.Info.Gravity << 8)
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
                if (gameEngine.IsDialogInProgress())
                {
                    return;
                }

                var y = entity.ModdedPosX - gameEngine.StaticVariables.PlayerEntity.ModdedPosX;

                if (y < 0)
                {
                    if (entity.Width + 1 <= gameEngine.StaticVariables.PlayerEntity.ModdedPosX - entity.ModdedPosX)
                    {
                        return;
                    }
                }
                else if (gameEngine.StaticVariables.PlayerEntity.Width + 1 <= y)
                {
                    return;
                }

                y = entity.ModdedPosY - gameEngine.StaticVariables.PlayerEntity.ModdedPosY;

                if (y < 0)
                {
                    if (entity.Height + 1 <= gameEngine.StaticVariables.PlayerEntity.ModdedPosY - entity.ModdedPosY)
                    {
                        return;
                    }
                }
                else if (gameEngine.StaticVariables.PlayerEntity.Height + 1 <= y)
                {
                    return;
                }

                y = entity.ModdedPosZ - gameEngine.StaticVariables.PlayerEntity.ModdedPosZ;

                if (y < 0)
                {
                    if (entity.Depth + 1 <= gameEngine.StaticVariables.PlayerEntity.ModdedPosZ - entity.ModdedPosZ)
                    {
                        return;
                    }
                }
                else if (gameEngine.StaticVariables.PlayerEntity.Depth + 1 <= y)
                {
                    return;
                }

                var res = gameEngine.PlayerManager.FUN_80033f00(gameEngine.StaticVariables.PlayerEntity, (int)itemId);

                if (res == false && entity.Bytes[0] != 0)
                {
                    gameEngine.StaticVariables.g_dropItemTextBuffer = string.Empty;
                }
                else
                {
                    gameEngine.StaticVariables.g_dropItemTextBuffer = gameEngine.EtcRes.GetItemName((int)itemId);
                    gameEngine.StaticVariables.g_dropItemTextBuffer += gameEngine.EtcRes.GetOtherString(0x45);
                }

                //Debugger.Break();
                gameEngine.FUN_80032b28((uint)entity.ContentsGameFlag); //AIValues[0]
                soundSfxIndex = gameEngine.StaticVariables.g_itemDropProperties[itemId].SoundSfxIndex; //itemId * 8 + 5

                if (soundSfxIndex != 0)
                {
                    gameEngine.SoundManager.PlaySoundEffect((uint)soundSfxIndex);

                    if (gameEngine.StaticVariables.g_dropItemTextBuffer.Length > 0)
                    {
                        gameEngine.UIManager.InitializeDialogMessage(gameEngine.StaticVariables.g_dropItemTextBuffer, 0);
                    }
                    entity.ItemState += 2;
                    return;
                }

                entity.ItemState += 1;
                gameEngine.EntityManager.FUN_8003ad30(entity);
                gameEngine.SoundManager.LoadBgm(0);
                gameEngine.StartCdStreaming(0xb);

                if (gameEngine.StaticVariables.g_dropItemTextBuffer.Length == 0)
                {
                    goto LAB_8007c68c;
                }

                LAB_8007c684:
                gameEngine.UIManager.InitializeDialogMessage(gameEngine.StaticVariables.g_dropItemTextBuffer, 1);

                LAB_8007c68c:
                gameEngine.SetEtcAnimationMode(4);
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
                if (gameEngine.CdManager.FUN_8005a7d4())
                {
                    return;
                }

                entity.ItemState += 1;
                gameEngine.SetEtcAnimationMode(3);
                gameEngine.SoundManager.StopAllSound();
                return;
            }

            if (itemState != 3)
            {
                return;
            }

            entity2 = entity;

            if (gameEngine.IsDialogInProgress())
            {
                return;
            }
        }

        gameEngine.EntityManager.FUN_8003adac(entity2);

        LAB_8007c740:
        gameEngine.DestroyEntity(entity);
    }

}