using AlundraEngine.Gameplay.Scripts.Boss;
using System;
using System.Diagnostics;

namespace AlundraEngine.Gameplay.Scripts;

public static class FunctionTypeC
{
    // 80065ED4
    //◆Beannoïde
    public static void AI_UpdateEntityAI_IdleSkittish(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Beannoïde")
        {
            Debugger.Break();
        }

        short delay = 0;
        uint direction;
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

                if (relPos[0] < 3 && relPos[1] < 3 && relPos[2] < 0x200001 && entity.Bytes[1] == 0)
                {
                    direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.TargetAnimationId = 3;
                    entity.Bytes[1] = 1;
                    return;
                }

                entity.TargetAnimationId = 1;
                var rand = ((Random.Next() * 0x3d) >> 32) + 0x3c;
                entity.AIValues[1] = (short)rand;

                if (relPos[0] < 4 && relPos[1] < 4)
                {
                    direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                }

                entity.Bytes[1] = 0;
                break;

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

                direction = (byte)ScriptHelper.DirectionTable[entity.TargetDirection]; // g_directionFlipTable //80028b34
                entity.ForceStepY = 0;
                entity.ForceStepX = 0;
                entity.ForceY = 0;
                entity.ForceX = 0;
                entity.TargetForceY = 0;
                entity.TargetForceX = 0;
                entity.TargetDirection = direction;
                delay = (short)(((Random.Next() * 0x1f) >> 32) + 0x1e);
                entity.TargetAnimationId = 0;
                entity.AIValues[1] = delay;
                break;

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
                if (entity.Bytes[3] != 0)
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

            LAB_80066234:
                entity.TargetAnimationId = 0;
            LAB_80066238:
                entity.AIValues[1] = delay;
                break;
        }
    }

    // 80066250
    public static void AI_UpdateEntityAI_CuriousFlying(GameEngine gameEngine, Entity entity)
    {
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
                    direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
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
                        direction = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX, entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
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
        if (entity.Name != "Tonneau générique")
        {
            Debugger.Break();
        }

        int value = 0;
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
        else if (entity.SpriteTableIndex != 0x175 || entity.TargetAnimationId != 5)
        {
            value = 0x175;

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
                    if (gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 3, 3, 0))
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
                        gameEngine.CurrentMap.Info.C,
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
                    entity.Flags |= 0x40;
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

        byte bVar1;
        ushort uVar2;
        bool bVar3;
        Entity entity2;
        uint uVar4;
        int iVar5;
        int entityRecordId;

        if ((gameEngine.StaticVariables.g_globalFlags[0] & 1) == 0)
        {
            return;
        }

        if (entity.AIValues[4] == 0)
        {
            bVar1 = entity.Bytes[1];
            entity.DelayOrAngle = 100;
            entity.AIValues[4] = 1;
            entity.AIValues[1] = 0;
            entity.Bytes[2] = 0;
            entity.Bytes[0] = 0;
            gameEngine.StaticVariables.DAT_80191134 = 0;
            gameEngine.StaticVariables.DAT_80191130 = gameEngine.StaticVariables.INT_ARRAY_80026d70[bVar1];

            if (entity.Bytes[1] - 1 < 2)
            {
                entity2 = gameEngine.SpawnEntity(entity, 0, 1);
                entity2.TargetDirection = 0x18;

                if (entity.Bytes[1] == 1)
                {
                    entity2.DelayOrAngle = 0x20000;
                }
                else
                {
                    entity2.DelayOrAngle = 0x28000;
                }
            }

            gameEngine.StaticVariables.g_globalFlags[0] &= 0xfffffeff;
        }
        entityRecordId = 0;
        entity.AIValues[1] = (short)(entity.AIValues[1] + 1);
        uVar2 = (ushort)entity.AIValues[1];
        iVar5 = entity.DelayOrAngle + 1;
        entity.DelayOrAngle = iVar5;
        if (uVar2 < 600)
        {
            entityRecordId = (iVar5 < 121 ? 1 : 0) << 1;
        }
        else
        {
            bVar3 = iVar5 < 100;

            if (0x3bf < uVar2)
            {
                bVar3 = iVar5 < 0x78;

                if (0x4eb < uVar2)
                {
                    bVar3 = iVar5 < 0x50;

                    if (0x743 < uVar2)
                    {
                        gameEngine.StaticVariables.INT_ARRAY_80191908[0] = entity.Bytes[2];
                        gameEngine.StaticVariables.INT_ARRAY_80191908[1] = entity.Bytes[0];
                        gameEngine.StaticVariables.INT_ARRAY_80191908[2] = gameEngine.StaticVariables.INT_ARRAY_80191908[0] - gameEngine.StaticVariables.INT_ARRAY_80191908[1];

                        if (gameEngine.StaticVariables.INT_ARRAY_80191908[2] < 0)
                        {
                            gameEngine.StaticVariables.INT_ARRAY_80191908[2] = 0;
                        }

                        if (gameEngine.StaticVariables.INT_ARRAY_80026d84[(uint)entity.Bytes[1] * 2] <= gameEngine.StaticVariables.INT_ARRAY_80191908[2])
                        {
                            gameEngine.StaticVariables.g_globalFlags[0] |= 0x100;
                        }

                        bVar1 = entity.Bytes[1];
                        entity.AIValues[4] = 0;
                        entity.Bytes[1] = (byte)(bVar1 + 1);
                        gameEngine.StaticVariables.g_globalFlags[0] &= 0xfffffffe;
                        return;
                    }
                }
            }

            if (!bVar3)
            {
                entityRecordId = 2;
            }
        }

        if (entityRecordId == 0)
        {
            goto LAB_800645f8;
        }

        entity2 = gameEngine.SpawnEntity(entity, entityRecordId, 1);
        entity2.ContentsItemId = 0;
        entity2.Flags = (entity2.Flags & 0xfff8ffffU) | 0x30000;

        if (entityRecordId == 0)
        {
            goto LAB_800645f8;
        }

        entity.DelayOrAngle = 0;

        if (gameEngine.StaticVariables.DAT_80191134 == 0)
        {
            gameEngine.StaticVariables.DAT_80191138 = (int)((Random.Next() * 6) >> 0x20);
            gameEngine.StaticVariables.DAT_80191134 = 3;
        }
        else
        {
            gameEngine.StaticVariables.DAT_80191134 += -1;
        }

        uVar2 = (ushort)(gameEngine.StaticVariables.DAT_80191134 * 2 + gameEngine.StaticVariables.DAT_80191138 * 8
                                                                     + gameEngine.StaticVariables.CutsceneChannel_ARRAY_80026d30[0].AngleZ);

        if ((uVar2 & 1) == 0)
        {
            entity2.DelayOrAngle = gameEngine.StaticVariables.DAT_80191130 + 0x12000;
            uVar4 = 0x18;

            if ((uVar2 & 2) != 0)
            {
                //goto LAB_800645bc;
                entity2.TargetDirection = uVar4;
                entity2.PosY += -0x100000;
            }
            else
            {
                entity2.TargetDirection = 8;
            }
        }
        else
        {
            iVar5 = gameEngine.StaticVariables.DAT_80191130 + 0x14000;
            entity2.PosZ += 0x180000;
            entity2.DelayOrAngle = iVar5;
            if ((uVar2 & 2) == 0)
            {
                entity2.TargetDirection = 0x18;
            }
            else
            {
                uVar4 = 8;
            LAB_800645bc:
                entity2.TargetDirection = uVar4;
                entity2.PosY += -0x100000;
            }
        }

        if (entity2.TargetDirection == 8)
        {
            entity2.PosX += 0xf00000;
            entity2.DelayOrAngle = -entity2.DelayOrAngle;
        }

    LAB_800645f8:
        if (2 < entity.Bytes[1] && 0x3b < (ushort)entity.AIValues[1])
        {
            if (entity.ItemState == 0)
            {
                iVar5 = (int)(((Random.Next() * 0xb) >> 0x20) * 2);
                bVar1 = entity.Bytes[1];
                entity.ItemState = iVar5 + 0x8c;

                if (bVar1 == 3)
                {
                    entity.ItemState = iVar5 + 0xb4;
                }

                entity2 = gameEngine.SpawnEntity(entity, 1, 1);
                entity2.ContentsItemId = 0;
                entity2.Flags = (entity2.Flags & 0xfff8ffffU) | 0x30000;
                uVar2 = (ushort)(gameEngine.StaticVariables.DAT_80191134 + (int)((((Random.Next() * 4) >> 0x20) & 3U) * 2)
                                                                         + gameEngine.StaticVariables.DAT_80191138 * 8
                                                                         + gameEngine.StaticVariables.CutsceneChannel_ARRAY_80026d30[0].AngleZ);

                if ((uVar2 & 1) != 0)
                {
                    entity2.PosZ += 0x180000;
                }

                if ((uVar2 & 2) != 0)
                {
                    entity2.TargetDirection = 8;
                }

                entity2.DelayOrAngle = 0x22000;

                if (entity2.TargetDirection == 8)
                {
                    entity2.PosX += 0xf00000;
                    entity2.DelayOrAngle = -entity2.DelayOrAngle;
                }
            }
            else
            {
                entity.ItemState += -1;
            }
        }
    }

    //800647b0
    public static void AI_FUN_800647b0(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        if (entity.ParentEntity.AIValues[4] == 0)
        {
            entity.Flags &= 0xfffffffe;
        }

        if (entity.Bytes[0] != 0)
        {
            if (entity.ForceAdjusted != 0)
            {
                if (entity.SpriteTableIndex == 0x125 && entity.ParentEntity.AIValues[4] != 0)
                {
                    entity.TargetDirection = (entity.TargetDirection + 0x10) & 0x1f;
                    entity.DelayOrAngle = -entity.DelayOrAngle;
                }
                else
                {
                    gameEngine.DestroyEntity(entity);
                }
            }
            if (entity.Bytes[0] != 0)
            {
                goto LAB_80064864;
            }
        }

        entity.Bytes[0] = 1;

    LAB_80064864:
        entity.PreviousAdjustedForceX = entity.DelayOrAngle;
    }

    //80064884
    public static void AI_FUN_80064884(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        byte bVar1;
        bool bVar2;
        short sVar3;
        SpriteEffect effect;
        int iVar5;
        uint[] flags;
        uint uVar7;
        int local_c;

        if (entity.AIValues[2] == 0)
        {
            effect = gameEngine.EffectManager.SpawnSpriteEffect(0, 1);
            entity.AIValues[2] = (short)effect.Id;
        }

        if ((gameEngine.StaticVariables.g_globalFlags[0] & 1) == 0)
        {
            return;
        }

        if (entity.Bytes[2] == 0)
        {
            if ((gameEngine.StaticVariables.g_globalFlags[0] & 0x80) != 0)
            {
                entity.Bytes[1] = 0;
                gameEngine.StaticVariables.g_globalFlags[0] &= 0xffffff7f;
            }

            bVar2 = false;
            gameEngine.StaticVariables.DAT_8019113c = entity.DelayOrAngle;
            local_c = (int)((Random.Next() * 3) >> 0x20);
            gameEngine.StaticVariables.DAT_80191140 = local_c + 3;
            entity.AIValues[1] = 0x3c;
            entity.AIValues[4] = 0;
            iVar5 = (int)((Random.Next() * 5) >> 0x20) + 0x46;
            entity.ItemState = iVar5;
            bVar1 = entity.Bytes[1];
            entity.DelayOrAngle = (entity.DelayOrAngle + iVar5) & 0xf;

            if (3 < bVar1 && (int)((Random.Next() * 3) >> 0x20) == 0)
            {
                iVar5 = entity.DelayOrAngle;

                if (iVar5 == 1)
                {
                    entity.DelayOrAngle = 0;
                    entity.ItemState -= 1;
                    iVar5 = entity.DelayOrAngle;
                }

                if (iVar5 == 0xf)
                {
                    entity.DelayOrAngle = 0;
                    entity.ItemState += 1;
                }
            }

            if (entity.DelayOrAngle == 0)
            {
            LAB_80064ae0:
                if (entity.Bytes[1] < 2 && (int)((Random.Next() * 3) >> 0x20) == 0)
                {
                    uVar7 = (uint)((entity.DelayOrAngle + 1U) & 0xf);

                    if (uVar7 != 0)
                    {
                        if (((gameEngine.StaticVariables.INT_ARRAY_80026d90[uVar7] - 0x8000) & 0x8000) == 0)
                        {
                            flags = gameEngine.StaticVariables.g_saveData.MapFlags;
                        }
                        else
                        {
                            flags = gameEngine.StaticVariables.g_globalFlags;
                        }

                        var index = ((gameEngine.StaticVariables.INT_ARRAY_80026d90[uVar7] - 0x8000) >> 3) & 0xffc;
                        var mask = 1 << (int)((gameEngine.StaticVariables.INT_ARRAY_80026d90[uVar7] + 0x8000U) & 0x1f);

                        if ((flags[index] & mask) != 0)
                        {
                            bVar2 = true;
                            entity.DelayOrAngle = (int)uVar7;
                            entity.ItemState += 1;
                        }
                    }
                }
            }
            else
            {
                if (((gameEngine.StaticVariables.INT_ARRAY_80026d90[entity.DelayOrAngle] - 0x8000) & 0x8000) == 0)
                {
                    flags = gameEngine.StaticVariables.g_saveData.MapFlags;
                }
                else
                {
                    flags = gameEngine.StaticVariables.g_globalFlags;
                }

                var index = ((gameEngine.StaticVariables.INT_ARRAY_80026d90[entity.DelayOrAngle] - 0x8000) >> 3) & 0xffc;
                var mask = 1 << (int)((gameEngine.StaticVariables.INT_ARRAY_80026d90[entity.DelayOrAngle] + 0x8000U) & 0x1f);

                if ((flags[index] & mask) == 0)
                {
                    //goto LAB_80064ae0;
                    if (entity.Bytes[1] < 2 && (int)((Random.Next() * 3) >> 0x20) == 0)
                    {
                        uVar7 = (uint)((entity.DelayOrAngle + 1U) & 0xf);

                        if (uVar7 != 0)
                        {
                            if (((gameEngine.StaticVariables.INT_ARRAY_80026d90[uVar7] - 0x8000) & 0x8000) == 0)
                            {
                                flags = gameEngine.StaticVariables.g_saveData.MapFlags;
                            }
                            else
                            {
                                flags = gameEngine.StaticVariables.g_globalFlags;
                            }

                            index = ((gameEngine.StaticVariables.INT_ARRAY_80026d90[uVar7] - 0x8000) >> 3) & 0xffc;
                            mask = 1 << (int)((gameEngine.StaticVariables.INT_ARRAY_80026d90[uVar7] + 0x8000U) & 0x1f);

                            if ((flags[index] & mask) != 0)
                            {
                                bVar2 = true;
                                entity.DelayOrAngle = (int)uVar7;
                                entity.ItemState += 1;
                            }
                        }
                    }
                }
                else
                {
                    bVar2 = true;
                }
            }

            if (bVar2)
            {
                gameEngine.StaticVariables.g_globalFlags[0] |= 0x100;
            }
            else
            {
                gameEngine.StaticVariables.g_globalFlags[0] &= 0xfffffeff;
            }
            entity.Bytes[2] = 1;
        }

        sVar3 = (short)(entity.AIValues[1] - 1);
        entity.AIValues[1] = sVar3;

        if (sVar3 != 0)
        {
            return;
        }

        if (entity.ItemState == 0)
        {
            entity.Bytes[2] = 0;
            entity.Bytes[1] = (byte)(entity.Bytes[1] + 1);

            if ((gameEngine.StaticVariables.g_globalFlags[0] & 0x100) != 0)
            {
                gameEngine.StaticVariables.g_globalFlags[0] &= 0xfffffffe;
                gameEngine.SoundManager.PlaySoundEffect(0x1c6);
                return;
            }

            gameEngine.StaticVariables.g_globalFlags[0] &= 0xfffffffe;
            gameEngine.SoundManager.PlaySoundEffect(0x1c7);
            return;
        }

        entity.ItemState -= 1;
        gameEngine.StaticVariables.DAT_8019113c = (gameEngine.StaticVariables.DAT_8019113c + 1) & 0xf;
        SpriteEffect spriteEffect = gameEngine.StaticVariables.g_effectSlots[entity.AIValues[4]];
        spriteEffect.X = gameEngine.StaticVariables.INT_ARRAY_80026dd0[gameEngine.StaticVariables.DAT_8019113c * 2];
        spriteEffect.Y = gameEngine.StaticVariables.INT_ARRAY_80026dd0[gameEngine.StaticVariables.DAT_8019113c * 2 + 1];

        gameEngine.SoundManager.PlaySoundEffect(0x1c8);

        if (gameEngine.StaticVariables.DAT_80191140 < entity.ItemState)
        {
            if (4 < (ushort)entity.AIValues[4])
            {
                sVar3 = 6;
                goto LAB_80064d78;
            }
        }
        else if (4 < (ushort)entity.AIValues[4])
        {
            sVar3 = (short)((gameEngine.StaticVariables.DAT_80191140 - entity.ItemState) * 8 + 8);
            goto LAB_80064d78;
        }

        sVar3 = (short)(entity.AIValues[4] * -6 + 0x24);

    LAB_80064d78:
        entity.AIValues[1] = sVar3;
    }

    //80064d90
    public static void AI_FUN_80064d90(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        //byte bVar1;
        //short sVar2;
        //short uVar3;
        //Entity spawnedEntity;
        //Entity entity2;
        //int iVar5;
        //int iVar6;
        //uint entityRecordId;
        //short psVar7;
        //short psVar8;
        //
        //if ((gameEngine.StaticVariables.g_globalFlags[0] & 1) != 0)
        //{
        //    if (entity.AIValues[4] == 0)
        //    {
        //        if ((gameEngine.StaticVariables.g_globalFlags[0] & 0x80) != 0)
        //        {
        //            entity.Bytes[1] = 0;
        //            gameEngine.StaticVariables.g_globalFlags[0] = gameEngine.StaticVariables.g_globalFlags[0] & 0xffffff7f;
        //        }
        //        bVar1 = entity.Bytes[1];
        //        entity.Bytes[2] = 0;
        //        entity.ItemState = 0;
        //        entity.AIValues[1] = 0;
        //        gameEngine.StaticVariables.DAT_80191144 = UNK_80026e6c + (uint)bVar1 * 0x28;
        //        gameEngine.StaticVariables.g_globalFlags[0] = gameEngine.StaticVariables.g_globalFlags[0] & 0xfffffeff;
        //        entity.AIValues[4] = 1;
        //    }
        //
        //    uVar3 = (short)(entity.AIValues[1] + 1);
        //    entity.AIValues[1] = uVar3;
        //    iVar5 = 0;
        //    if (uVar3 < 0x708)
        //    {
        //        if (-1 < gameEngine.StaticVariables.g_numberOfEntities)
        //        {
        //            do
        //            {
        //                entity2 = gameEngine.StaticVariables.g_entitySlots[iVar5];
        //                iVar5 = iVar5 + 1;
        //
        //                if (entity2.EntityRefId - 4U < 9)
        //                {
        //                    return;
        //                }
        //            } while (iVar5 <= gameEngine.StaticVariables.g_numberOfEntities);
        //        }
        //
        //        iVar5 = entity.DelayOrAngle - 1;
        //
        //        if (entity.DelayOrAngle == 0)
        //        {
        //            iVar5 = gameEngine.StaticVariables.DAT_80191144[entity.ItemState * 2];
        //            entity.ItemState = entity.ItemState + 1;
        //
        //            switch (iVar5)
        //            {
        //                case 1:
        //                    iVar6 = 0;
        //                    psVar8 = gameEngine.StaticVariables.SHORT_80026f34;
        //                    break;
        //
        //                case 2:
        //                    iVar6 = 2;
        //                    psVar8 = gameEngine.StaticVariables.SHORT_80026f3c;
        //                    break;
        //
        //                case 3:
        //                    iVar6 = 1;
        //                    psVar8 = gameEngine.StaticVariables.SHORT_80026f60;
        //                    break;
        //
        //                case 4:
        //                    iVar6 = 1;
        //                    psVar8 = gameEngine.StaticVariables.SHORT_80026f84;
        //                    break;
        //
        //                case 6:
        //                    iVar6 = 1;
        //                    psVar8 = gameEngine.StaticVariables.SHORT_80026fb4;
        //                    break;
        //
        //                default:
        //                    entity.ItemState = entity.ItemState - 1;
        //                    return;
        //            }
        //
        //            psVar8 = (short)(psVar8 + iVar5 * (int)((Random.Next() * (ulong)(iVar6 + 1)) >> 0x20) * 3);
        //            
        //            if (iVar5 != 0)
        //            {
        //                psVar7 = psVar8 + 2;
        //
        //                do
        //                {
        //                    entityRecordId = (uint)psVar7[-1];
        //
        //                    if ((entityRecordId & 0x80) != 0)
        //                    {
        //                        entityRecordId = (gameEngine.StaticVariables.DAT_80026e5e + (entityRecordId & 0xf) * 4) +
        //                            (int)(Random.Next() * (ulong)((gameEngine.StaticVariables.SHORT_80026e5c)[(entityRecordId & 0xf) * 2] + 1) >> 0x20);
        //                    }
        //
        //                    spawnedEntity = gameEngine.SpawnEntity(entity, entityRecordId, 1);
        //                    sVar2 = *psVar8;
        //                    psVar8 = psVar8 + 3;
        //                    iVar5 = iVar5 - 1;
        //                    spawnedEntity.AIValues[1] = sVar2;
        //                    sVar2 = *psVar7;
        //                    psVar7 = psVar7 + 3;
        //                    spawnedEntity.ContentsItemId = 0;
        //                    spawnedEntity.AIValues[4] = sVar2;
        //                } while (iVar5 != 0);
        //            }
        //
        //            iVar5 = 0x14;
        //        }
        //
        //        entity.DelayOrAngle = iVar5;
        //    }
        //    else
        //    {
        //        gameEngine.StaticVariables.INT_ARRAY_80191908[0] = (int)entity.Bytes[2];
        //
        //        if (gameEngine.StaticVariables.INT_80026e50[entity.Bytes[1] * 2] <= entity.Bytes[2])
        //        {
        //            gameEngine.StaticVariables.g_globalFlags[0] = gameEngine.StaticVariables.g_globalFlags[0] | 0x100;
        //        }
        //
        //        bVar1 = entity.Bytes[1];
        //        entity.AIValues[4] = 0;
        //        entity.Bytes[1] = (byte)(bVar1 + 1);
        //
        //        gameEngine.StaticVariables.g_globalFlags[0] = gameEngine.StaticVariables.g_globalFlags[0] & 0xfffffffe;
        //    }
        //}
    }

    //80065100
    public static void AI_FUN_80065100(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        short sVar1;
        uint targetAnimationId;

        if ((gameEngine.StaticVariables.g_globalFlags[0] & 1) == 0 && entity.TargetAnimationId != 0x10)
        {
            entity.TargetAnimationId = 0x10;
        }
        else
        {
            targetAnimationId = entity.TargetAnimationId;

            if (targetAnimationId == 0x10)
            {
                if (entity.ForceResetAnimationFlag != 0)
                {
                    gameEngine.DestroyEntity(entity);
                }
            }
            else if ((int)targetAnimationId < 0x11)
            {
                sVar1 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar1;

                if (targetAnimationId == 0 && entity.AIValues[1] == 0)
                {
                    entity.TargetAnimationId = 0x10;
                }
            }
            else if (targetAnimationId == 0x11)
            {
                sVar1 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar1;

                if (sVar1 == 0)
                {
                    sVar1 = entity.AIValues[4];
                    entity.TargetAnimationId = 0x12;
                    entity.AIValues[1] = sVar1;
                }
            }
            else if (targetAnimationId == 0x12 && entity.ForceResetAnimationFlag != 0)
            {
                entity.TargetAnimationId = 0;
            }
        }
    }

    //80065204
    public static void AI_FUN_80065204(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80065750
    public static void AI_FUN_80065750(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "Toutou (chien)")
        {
            Debugger.Break();
        }

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
            sVar3 = (short)(entity.AIValues[1] - 1);
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
                        uVar4 = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                        entity.TargetDirection = uVar4;
                        entity.AIValues[1] = 0x50;
                        entity.Bytes[0] = 1;
                        return;
                    }

                    if (relativePositions[0] < 5 && relativePositions[1] < 5)
                    {
                        uVar4 = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
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
                    entity.AIValues[1] = (short)(sVar3 - 1);

                    if (bVar1 != 0
                        && sVar3 == 0x47
                        && (gameEngine.StaticVariables.g_playerControlFlags & 4U) == 0)
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

        short sVar1;
        short uVar2;
        Entity entitySpawned;
        int iVar4;
        int iVar6;
        uint iVar7;

        if (entity.Bytes[0] == 0)
        {
            entity.AIValues[1] = 0x3c;
            entity.Bytes[0] = 1;
        }

        if (entity.SpriteTableIndex == 0x1cb)
        {
            iVar7 = entity.TargetAnimationId;

            if (iVar7 == 1)
            {
                sVar1 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar1;
                uVar2 = 0x3c;

                if (sVar1 != 0)
                {
                    iVar6 = entity.ForceAdjusted;

                joined_r0x80065d70:
                    if (iVar6 == 0)
                    {
                        return;
                    }

                    entity.TargetDirection = (uint)((entity.TargetDirection + (int)((Random.Next() * 5) >> 0x20) - 0x14) & 0x1f);
                    return;
                }

                goto LAB_80065e00;
            }

            if ((int)iVar7 < 2)
            {
                if (iVar7 != 0)
                {
                    return;
                }

                sVar1 = entity.AIValues[1];

                if (sVar1 == 0)
                {
                    gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x3c, 100);
                    return;
                }

            LAB_80065d28:
                entity.AIValues[1] = (short)(sVar1 - 1);
                return;
            }

            if (iVar7 != 3)
            {
                if (iVar7 != 6)
                {
                    return;
                }

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                iVar6 = 0;

                if (entity.Bytes[1] != 0)
                {
                LAB_80065e2c:
                    gameEngine.DestroyEntity(entity);
                    return;
                }

                iVar7 = 4;

                do
                {
                    entitySpawned = gameEngine.SpawnEntity(entity, 0x24, 1);

                    if (entitySpawned != null)
                    {
                        entitySpawned.TargetAnimationId = 1;
                        entitySpawned.PosX = entity.PosX;
                        entitySpawned.PosY = entity.PosY;
                        iVar4 = entity.PosZ;
                        entitySpawned.TargetDirection = iVar7;
                        entitySpawned.AIValues[1] = 0x3c;
                        entitySpawned.PosZ = iVar4;
                    }

                    iVar6 += 1;
                    iVar7 += 8;
                } while (iVar6 != 4);

            LAB_80065ea8:
                entity.Bytes[1] = 1;
                return;
            }

            if (entity.ForceResetAnimationFlag == 0)
            {
                return;
            }

            if (entity.Bytes[3] != 0)
            {
                entity.TargetAnimationId = 6;
                return;
            }
        }
        else
        {
            iVar7 = entity.TargetAnimationId;

            if (iVar7 == 1)
            {
                sVar1 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar1;
                uVar2 = 0x3c;

                if (sVar1 != 0)
                {
                    iVar6 = entity.ForceAdjusted;
                    //goto joined_r0x80065d70;

                    if (iVar6 == 0)
                    {
                        return;
                    }

                    entity.TargetDirection = (uint)((entity.TargetDirection + (int)((Random.Next() * 5) >> 0x20) - 0x14) & 0x1f);
                    return;
                }

                goto LAB_80065e00;
            }

            if ((int)iVar7 < 2)
            {
                if (iVar7 != 0)
                {
                    return;
                }

                sVar1 = entity.AIValues[1];

                if (sVar1 == 0)
                {
                    gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x3c, 0x14);
                    return;
                }

                //goto LAB_80065d28;
                entity.AIValues[1] = (short)(sVar1 - 1);
                return;
            }

            if (iVar7 != 2)
            {
                if (iVar7 != 3)
                {
                    return;
                }

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                iVar6 = 0;

                if (entity.Bytes[1] != 0)
                {
                    //goto LAB_80065e2c;
                    gameEngine.DestroyEntity(entity);
                    return;
                }

                iVar7 = 4;

                do
                {
                    entitySpawned = gameEngine.SpawnEntity(entity, 0x25, 1);
                    if (entitySpawned != null)
                    {
                        entitySpawned.TargetAnimationId = 0x13;
                        entitySpawned.PosX = entity.PosX;
                        entitySpawned.PosY = entity.PosY;
                        iVar4 = entity.PosZ;
                        entitySpawned.TargetDirection = iVar7;
                        entitySpawned.SpriteProgramIndexes[0] = 0;
                        entitySpawned.AIValues[1] = 0x1e;
                        entitySpawned.PosZ = iVar4;
                    }
                    iVar6 += 1;
                    iVar7 += 8;
                } while (iVar6 != 4);

                //goto LAB_80065ea8;
                entity.Bytes[1] = 1;
                return;
            }

            if (entity.ForceResetAnimationFlag == 0)
            {
                return;
            }

            if (entity.Bytes[3] != 0)
            {
                entity.TargetAnimationId = 3;
                return;
            }
        }

        entity.DamagedTickCounter = 0x5a;
        uVar2 = 0x28;

    LAB_80065e00:
        entity.TargetAnimationId = 0;
        entity.AIValues[1] = uVar2;
    }

    //8006b510
    public static void AI_FUN_8006b510(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        short sVar1;
        byte bVar2;
        uint uVar3;
        int[] positions = new int[6];

        FUN_8007fe8c(gameEngine, entity, positions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                sVar1 = entity.AIValues[1];

                if (sVar1 > 0)
                {
                    entity.AIValues[1] = (short)(sVar1 - 1);
                }

                if (sVar1 == 0 || sVar1 == 1)
                {

                    if (entity.Bytes[0] == 0)
                    {
                        entity.Bytes[0] = 10;
                    }

                    if (entity.Bytes[1] == 1)
                    {
                        uVar3 = 2;
                        if (entity.IsAboveGround == 0)
                        {
                            uVar3 = (uint)(((Random.Next() * 3) >> 0x20) + 1);
                        }

                        entity.TargetAnimationId = uVar3;
                        entity.TargetDirection = (uint)((Random.Next() * 0x20) >> 0x20);
                    }
                    else
                    {
                        if (entity.ForceAdjusted == 0)
                        {
                            uVar3 = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX, entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
                        }
                        else
                        {
                            uVar3 = gameEngine.StaticVariables.g_directionFlipTable[entity.TargetDirection];
                            entity.ForceStepY = 0;
                            entity.ForceStepX = 0;
                            entity.ForceY = 0;
                            entity.ForceX = 0;
                            entity.TargetForceY = 0;
                            entity.TargetForceX = 0;
                        }

                        entity.TargetDirection = uVar3;
                        entity.TargetAnimationId = 2;
                    }
                }
                break;

            case 1:
            case 2:
            case 3:
                if (entity.ForceResetAnimationFlag != 0
                    || (entity.IsAboveGround != 0 && entity.TargetAnimationId == 3)
                    || entity.ForceAdjusted != 0)
                {
                    bVar2 = entity.Bytes[0];
                    entity.AIValues[1] = 10;
                    entity.TargetAnimationId = 0;
                    bVar2 = (byte)(bVar2 - 1);
                    entity.Bytes[0] = bVar2;

                    if (bVar2 == 0)
                    {
                        if (entity.Bytes[1] == 1)
                        {
                            entity.Bytes[1] = 2;
                        }
                        else
                        {
                            entity.TargetAnimationId = 8;
                        }
                    }
                }
                break;

            case 5:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[3] == 0)
                    {
                        if (entity.Bytes[1] == 1)
                        {
                            entity.Bytes[0] = 0;
                        }

                        entity.TargetAnimationId = 0;
                    }
                    else
                    {
                        if (gameEngine.StaticVariables.g_currentMap == 0x5f)
                        {
                            entity.ParentEntity.AIValues[4] = (short)(entity.ParentEntity.AIValues[4] - 1);
                        }

                        entity.TargetAnimationId = 6;
                        entity.Flags |= 0x40;
                    }
                }
                break;

            case 7:
                if (positions[0] < 3 && positions[1] < 3)
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0;
                    entity.Bytes[1] = 1;
                }
                break;

            case 8:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (gameEngine.StaticVariables.g_currentMap == 0x5f)
                    {
                        entity.ParentEntity.AIValues[4] = (short)(entity.ParentEntity.AIValues[4] - 1);
                        gameEngine.DestroyEntity(entity);
                    }
                    else
                    {
                        gameEngine.ResetEntity(entity);
                    }
                }

                break;
        }
    }

    //8006b848
    //Caisse en bois générique
    public static void AI_FUN_8006b848(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "Caisse en bois générique" && entity.Name != "Cruche générique")
        {
            Debugger.Break();
        }

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
    //spores
    public static void AI_FUN_8006b8cc(GameEngine gameEngine, Entity entity)
    {
        //do nothing
    }

    //8006ce08
    //Oiseau de feu Niv.1
    public static void AI_FUN_8006ce08(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        bool bVar2;
        short sVar3;
        int iVar4;
        uint direction;
        Entity pEVar5;
        byte bVar6;
        int[] positions = new int[6];
        uint local_1c;

        FUN_8007fe8c(gameEngine, entity, positions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                entity.TargetAnimationId = 1;
                local_1c = (uint)((Random.Next() * 0x20) >> 0x20);
                entity.TargetDirection = local_1c;
                entity.AIValues[1] = (short)(((Random.Next() * 0x20) >> 0x20) + 0x50);
                break;

            case 1:
            case 7:
            case 8:
                iVar4 = entity.DelayOrAngle - 1;

                if (iVar4 >= 0)
                {
                    entity.DelayOrAngle = iVar4;
                }

                if ((entity.DelayOrAngle == 0 || iVar4 == 0)
                    && entity.Bytes[1] == 0
                    && entity.Bytes[2] == 0
                    && -1 < positions[5]
                    && gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, positions, 2, 5, 0x400000))
                {
                    entity.TargetAnimationId = 1;
                    direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.AIValues[1] = 0x32;
                    entity.Bytes[2] = 4;
                    return;
                }

                sVar3 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar3;

                if (sVar3 == 0)
                {
                    if (entity.Bytes[2] == 0)
                    {
                        if (entity.IsAboveGround != 0)
                        {
                            sVar3 = (short)((Random.Next() * 0x20) >> 0x20);
                            entity.TargetAnimationId = 7;
                            //goto LAB_8006d3d8;
                            entity.AIValues[1] = (short)(sVar3 + 0x28);
                            break;
                        }

                        if ((int)((Random.Next() * 3) >> 0x20) == 0)
                        {
                            entity.TargetAnimationId = 8;
                            entity.AIValues[1] = 0x1e;
                            return;
                        }

                        direction = (uint)((Random.Next() * 0x20) >> 0x20);
                        entity.TargetAnimationId = 1;
                        entity.AIValues[1] = 100;
                        entity.Bytes[1] = 0;
                    }
                    else
                    {
                        if ((int)((Random.Next() * 3) >> 0x20) == 0)
                        {
                            gameEngine.SoundManager.PlaySoundEffect(0x171);
                        }

                        pEVar5 = gameEngine.SpawnWarpEntity(entity, 1, 0xda, entity.PosX, entity.PosY, entity.PosZ + 0x100000, entity.TargetDirection);

                        if (pEVar5 != null)
                        {
                            pEVar5.ForceZ = -0x10000;
                            pEVar5.Flags &= 0xfffffeff;
                        }

                        bVar6 = (byte)(entity.Bytes[2] - 1);
                        entity.Bytes[2] = bVar6;
                        entity.AIValues[1] = (short)(((Random.Next() * 0x10) >> 0x20) + 0x32);
                        if (bVar6 != 0)
                        {
                            direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                            entity.TargetDirection = direction;
                            return;
                        }

                        direction = (uint)((Random.Next() * 0x20) >> 0x20);
                        entity.DelayOrAngle = 0x32;
                    }

                LAB_8006d430:
                    entity.TargetDirection = direction;
                }
                else
                {
                    direction = entity.TargetAnimationId;
                    if (direction == 7)
                    {
                        if ((int)(entity.PosZ + (uint)(ushort)entity.AIValues[0] * -0x10000) < 0x500000)
                        {
                            return;
                        }

                        direction = 8;

                        if (entity.PosZ - entity.TerrainHeight < 0x200001)
                        {
                            direction = 1;
                        }

                        entity.TargetAnimationId = direction;
                        sVar3 = (short)((Random.Next() * 0x20) >> 0x20);
                        entity.Bytes[0] = 0;
                        entity.Bytes[1] = 0;
                    }
                    else
                    {
                        if ((int)direction < 8)
                        {
                            if (direction != 1)
                            {
                                return;
                            }
                            if (entity.ForceAdjusted == 0)
                            {
                                return;
                            }

                            bVar6 = entity.Bytes[0];
                            entity.AIValues[1] = 0x28;
                            entity.TargetDirection = (uint)((Random.Next() * 0x20) >> 0x20);

                            if (bVar6 == 1)
                            {
                                entity.TargetAnimationId = 7;
                                return;
                            }

                            entity.TargetAnimationId = 8;
                            return;
                        }

                        if (direction != 8)
                        {
                            return;
                        }

                        iVar4 = entity.PosZ + entity.AIValues[0] * -0x10000;

                        if (-0x600000 < iVar4)
                        {
                            if (entity.IsAboveGround == 0)
                            {
                                return;
                            }

                            if (0x200000 < iVar4)
                            {
                                direction = (uint)((Random.Next() * 0x20) >> 0x20);
                                entity.TargetAnimationId = 1;
                                entity.AIValues[1] = 0x3c;
                                //goto LAB_8006d430;
                                entity.TargetDirection = direction;
                                break;
                            }
                        }

                        sVar3 = (short)((Random.Next() * 0x11) >> 0x20);
                        entity.TargetAnimationId = 7;
                        entity.Bytes[0] = 1;
                    }

                LAB_8006d3d8:
                    entity.AIValues[1] = (short)(sVar3 + 0x28);
                }
                break;

            case 2:
                if (entity.ForceResetAnimationFlag != 0 && entity.Bytes[0] == 1)
                {
                    entity.TargetAnimationId = 7;
                    entity.AIValues[1] = (short)(((Random.Next() * 0x10) >> 0x20) + 300);
                }
                break;

            case 5:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[3] == 0)
                    {
                        entity.TargetAnimationId = 7;
                        direction = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX, entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
                        entity.TargetDirection = direction;
                        entity.AIValues[1] = 0x78;
                        entity.Bytes[1] = 1;
                    }
                    else
                    {
                        entity.TargetAnimationId = 6;
                        entity.Flags |= 0x40;
                    }
                }
                break;

            case 9:
                if (positions[0] < 5 && positions[1] < 5 && positions[2] < 0x200001)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x171);
                    entity.TargetAnimationId = 2;
                    entity.Bytes[0] = 1;
                }

                break;
        }
    }

    //8006e83c
    public static void AI_FUN_8006e83c(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        uint uVar1;

        if (entity.TargetAnimationId == 0 && entity.Bytes[0] != 0x40)
        {
            if (entity.AIValues[1] == 0)
            {
                entity.Bytes[0] = (byte)(entity.Bytes[0] + 1);
                uVar1 = entity.TargetDirection;
                entity.AIValues[1] = 4;
                entity.TargetDirection = (uVar1 + 1) & 0x1f;
            }
            else
            {
                entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
            }
        }
    }

    //8006eb9c
    //Abyss Niv.1
    public static void AI_FUN_8006eb9c(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006f860
    public static void AI_FUN_8006f860(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        if (entity.TargetAnimationId == 0)
        {
            if (entity.ForceResetAnimationFlag != 0
                || entity.ForceAdjusted != 0
                || gameEngine.StaticVariables.PlayerEntity.TouchingEntity == entity)
            {
                entity.TargetAnimationId = 1;
                entity.Flags |= 0x40;
            }
        }
        else if (entity.TargetAnimationId == 3)
        {
            if (entity.AIValues[1] == 0)
            {
                entity.TargetAnimationId = 0;
            }
            else
            {
                entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
            }
        }
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

        short sVar1;
        uint uVar2;
        Entity entity2;
        int iVar3;

        uVar2 = entity.TargetAnimationId;

        if (uVar2 == 1)
        {
            sVar1 = (short)(entity.AIValues[1] - 1);
            entity.AIValues[1] = sVar1;

            if (sVar1 == 0)
            {
                entity.TargetAnimationId = 4;
            }
        }
        else if ((int)uVar2 < 2)
        {
            if (uVar2 == 0 && entity.AIValues[1] == 0)
            {
                entity2 = entity.ParentEntity;
                entity.AIValues[1] = 600;
                entity2.AIValues[4] = (short)(entity2.AIValues[4] + 3);
            }
        }
        else if (uVar2 == 2)
        {
            if (entity.ForceResetAnimationFlag != 0)
            {
                if (entity.Bytes[3] == 0)
                {
                    entity.TargetAnimationId = 1;
                }
                else
                {
                    entity.TargetAnimationId = 3;
                    entity.Flags |= 0x40;
                    entity.ParentEntity.AIValues[4] = (short)(entity.ParentEntity.AIValues[4] - 3);
                }
            }
        }
        else if (uVar2 == 4)
        {
            iVar3 = 0;

            if (entity.ForceResetAnimationFlag != 0)
            {
                entity2 = entity.ParentEntity;
                entity.Status = 3;

                do
                {
                    iVar3 += 1;
                    gameEngine.SpawnWarpEntity(entity2, 1, 0xca,
                        entity.PosX + (int)((Random.Next() * 0x19) >> 0x20) * 0x10000 - 0xc0000,
                        entity.PosY + (int)((Random.Next() * 0x11) >> 0x20) * 0x10000 - 0xc0000,
                        entity.PosZ, 0);
                } while (iVar3 != 3);
            }
        }
    }

    //800756ec
    //Giles (homme religieux)
    public static void AI_FUN_800756ec(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        byte bVar1;
        short sVar2;
        uint uVar3;

        gameEngine.GetMatchingEntityBySearchType(entity, 0);

        if (gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].DelayOrAngle != 0 ||
           gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].Bytes[3] != 0)
        {
            entity.TargetAnimationId = 0;
            return;
        }

        if (entity.Bytes[2] != 0)
        {
            goto LAB_80075814;
        }

        if (entity.Bytes[1] != 0)
        {
            if (gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].TargetAnimationId != 2 && 0x1afffff < entity.PosY)
            {
                entity.Bytes[1] = 0;
                entity.AIValues[1] = 0x1e;
                goto LAB_80075814;
            }

            if (entity.Bytes[1] != 0)
            {
                goto LAB_80075814;
            }
        }

        if (gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].TargetAnimationId == 2 || entity.PosY < 0x1900001)
        {
            entity.Bytes[1] = 1;
            entity.TargetAnimationId = 0;
            entity.AIValues[1] = (short)((short)((Random.Next() * 0x20) >> 0x20) + 0x14);
            return;
        }

    LAB_80075814:
        uVar3 = entity.TargetAnimationId;

        if (uVar3 == 1)
        {
            if (entity.Bytes[1] == 0)
            {
                sVar2 = entity.AIValues[1];

                if (sVar2 > 0)
                {
                    entity.AIValues[1] = (short)(sVar2 - 1);
                }

                if (entity.AIValues[1] == 0 || entity.ForceAdjusted != 0)
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = (short)(((Random.Next() * 0x1f) >> 0x20) + 0x1e);
                }
            }
        }
        else if ((int)uVar3 < 2)
        {
            if (uVar3 == 0)
            {
                if (entity.AIValues[1] == 0)
                {
                    if (entity.Bytes[1] == 0)
                    {
                        if (entity.Bytes[0] == 0)
                        {
                            bVar1 = gameEngine.StaticVariables.BYTE_ARRAY_80028b54[(int)((Random.Next() * 4) >> 0x20)];
                            entity.AIValues[1] = 0x3c;
                            entity.Bytes[0] = 1;
                            entity.TargetDirection = bVar1;
                        }
                        else
                        {
                            entity.Bytes[0] = 0;
                            entity.TargetAnimationId = 1;
                            entity.AIValues[1] = (short)(((Random.Next() * 0x3d) >> 0x20) + 0x3c);
                        }
                    }
                    else
                    {
                        entity.TargetAnimationId = 1;
                        entity.TargetDirection = 0;
                    }
                }
                else
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                }
            }
        }
        else if (uVar3 == 0x10)
        {
            sVar2 = (short)(entity.AIValues[1] - 1);
            entity.AIValues[1] = sVar2;

            if (sVar2 == 0 && entity.IsAboveGround != 0)
            {
                entity.TargetAnimationId = 0x11;
            }
        }
        else if (uVar3 == 0x11 && entity.ForceResetAnimationFlag != 0)
        {
            entity.TargetAnimationId = 0;
            entity.Bytes[2] = 0;
        }
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

        uint direction;

        direction = entity.TargetAnimationId;

        if (direction == 3)
        {
            if (entity.ForceResetAnimationFlag == 0)
            {
                return;
            }

            if (entity.Bytes[3] != 0)
            {
                entity.TargetAnimationId = 4;
                entity.Flags |= 0x40;
                entity.ParentEntity.AIValues[4] = (short)(entity.ParentEntity.AIValues[4] - 1);
                return;
            }

            if (entity.CollidedWithEntityZ != 0)
            {
                entity.TargetAnimationId = 0;
                return;
            }
        }
        else
        {
            if ((int)direction < 4)
            {
                if (direction != 1)
                {
                    return;
                }
                if (entity.TerrainHeight + 1 < entity.PosZ)
                {
                    return;
                }

                entity.TargetAnimationId = 2;
                entity.Flags = (entity.Flags & 0xffffbfffU) | 0x100;
                return;
            }

            if (direction != 5)
            {
                return;
            }

            if (entity.AIValues[1] != 0)
            {
                entity.AIValues[1] += -1;
                return;
            }
        }

        entity.TargetAnimationId = 1;
    }

    //80077734
    //◆Wilda (tête)
    public static void AI_FUN_80077734(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80078a5c
    public static void AI_FUN_80078a5c(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        byte bVar1;
        short uVar2;
        SpriteEffect? spriteEffect;

        if (entity.TargetAnimationId == 0)
        {
            if (entity.ForceResetAnimationFlag != 0)
            {
                bVar1 = entity.Bytes[1];
                entity.TargetAnimationId = entity.Bytes[0];

                if (bVar1 == 1)
                {
                    entity.ForceZ = (int)((Random.Next() * 9) >> 0x20) * 0x800 - 0x18000;
                }
            }
        }
        else
        {
            uVar2 = (short)(entity.AIValues[1] + 1);
            entity.AIValues[1] = uVar2;

            if ((uVar2 & 0xf) == 0)
            {
                spriteEffect = gameEngine.EffectManager.CreateEffectEntity(1, 0, 0, entity.PosX, entity.PosY, entity.PosZ);

                if (spriteEffect != null)
                {
                    spriteEffect.ForceZ = 0x10000;
                }
            }
        }
    }

    //80078b54
    public static void AI_FUN_80078b54(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        bool bVar1;
        short sVar2;
        int dx;
        uint uVar3;
        int directionTableIndex;
        Entity parentEntity;
        int dy;
        byte index;

        parentEntity = entity.ParentEntity;

        if (parentEntity.Bytes[3] != 0)
        {
            entity.TargetAnimationId = 2;
            entity.Flags = entity.Flags | 0x40;
        }

        uVar3 = entity.TargetAnimationId;

        if (uVar3 == 3)
        {
        LAB_80078ce8:
            bVar1 = entity.ForceAdjusted != 0;

            if (parentEntity.Bytes[0] == 0)
            {
            LAB_80078d70:
                dx = entity.PosX - entity.DelayOrAngle;
                dy = entity.PosY - entity.ItemState;

                if (dx < 0)
                {
                    dx = -dx;
                }

                if (dx < 0x100001)
                {
                    if (dy < 0)
                    {
                        dy = -dy;
                    }

                    if (dy < 0x100001)
                    {
                        bVar1 = true;
                    }
                }
            }
            else
            {
                if (parentEntity.Bytes[0] == 1)
                {
                    index = (byte)(entity.Bytes[1] - 1);
                    entity.Bytes[1] = index;

                    if (index == 0)
                    {
                        uVar3 = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                        entity.TargetDirection = uVar3;
                        entity.Bytes[1] = 10;
                    }

                    sVar2 = (short)(entity.AIValues[1] - 1);
                    entity.AIValues[1] = sVar2;

                    if (sVar2 == 0)
                    {
                        bVar1 = true;
                    }

                    //goto LAB_80078d70;
                    dx = entity.PosX - entity.DelayOrAngle;
                    dy = entity.PosY - entity.ItemState;

                    if (dx < 0)
                    {
                        dx = -dx;
                    }

                    if (dx < 0x100001)
                    {
                        if (dy < 0)
                        {
                            dy = -dy;
                        }

                        if (dy < 0x100001)
                        {
                            bVar1 = true;
                        }
                    }
                }

                sVar2 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar2;

                if (sVar2 == 0)
                {
                    bVar1 = true;
                }
            }

            if (!bVar1)
            {
                return;
            }

            entity.TargetAnimationId = 1;
            uVar3 = entity.Flags | 0x100;

        LAB_80078e18:
            entity.Flags = uVar3;
            return;
        }

        if ((int)uVar3 < 4)
        {
            if (uVar3 != 1)
            {
                return;
            }
            if (entity.IsAboveGround == 0)
            {
                return;
            }

            entity.TargetAnimationId = 2;
            uVar3 = entity.Flags | 0x40;
            //goto LAB_80078e18;
            entity.Flags = uVar3;
            return;
        }

        if (uVar3 != 4)
        {
            if (uVar3 != 5)
            {
                return;
            }

            //goto LAB_80078ce8;
            bVar1 = entity.ForceAdjusted != 0;

            if (parentEntity.Bytes[0] == 0)
            {
            LAB_80078d70:
                dx = entity.PosX - entity.DelayOrAngle;
                dy = entity.PosY - entity.ItemState;

                if (dx < 0)
                {
                    dx = -dx;
                }

                if (dx < 0x100001)
                {
                    if (dy < 0)
                    {
                        dy = -dy;
                    }

                    if (dy < 0x100001)
                    {
                        bVar1 = true;
                    }
                }
            }
            else
            {
                if (parentEntity.Bytes[0] == 1)
                {
                    index = (byte)(entity.Bytes[1] - 1);
                    entity.Bytes[1] = index;

                    if (index == 0)
                    {
                        uVar3 = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                        entity.TargetDirection = uVar3;
                        entity.Bytes[1] = 10;
                    }

                    sVar2 = (short)(entity.AIValues[1] - 1);
                    entity.AIValues[1] = sVar2;

                    if (sVar2 == 0)
                    {
                        bVar1 = true;
                    }

                    //goto LAB_80078d70;
                    dx = entity.PosX - entity.DelayOrAngle;
                    dy = entity.PosY - entity.ItemState;

                    if (dx < 0)
                    {
                        dx = -dx;
                    }

                    if (dx < 0x100001)
                    {
                        if (dy < 0)
                        {
                            dy = -dy;
                        }

                        if (dy < 0x100001)
                        {
                            bVar1 = true;
                        }
                    }
                }

                sVar2 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar2;

                if (sVar2 == 0)
                {
                    bVar1 = true;
                }
            }

            if (!bVar1)
            {
                return;
            }

            entity.TargetAnimationId = 1;
            uVar3 = entity.Flags | 0x100;

        LAB_80078e18:
            entity.Flags = uVar3;
            return;
        }

        if (entity.PosZ < parentEntity.PosZ + 0xa00000)
        {
            return;
        }

        entity.ForceZ = 0;
        entity.TargetAnimationId = 5;

        switch (parentEntity.Bytes[0])
        {
            case 1:
                entity.Bytes[1] = 0x1e;
                entity.AIValues[1] = 0xf0;
                goto case 0;

            case 0:
                uVar3 = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                entity.TargetDirection = uVar3;
                entity.DelayOrAngle = gameEngine.StaticVariables.PlayerEntity.PosX;
                entity.ItemState = gameEngine.StaticVariables.PlayerEntity.PosY;
                return;

            case 2:
                entity.AIValues[1] = 0x3c;
                index = entity.Bytes[0];
                directionTableIndex = 0;
                break;

            case 3:
                index = entity.Bytes[0];
                uVar3 = 2;
                goto LAB_80078ca4;

            case 4:
                index = entity.Bytes[0];
                uVar3 = 0x1e;

            LAB_80078ca4:
                entity.TargetDirection = uVar3;
                entity.AIValues[1] = (short)((index + 1) * 0x18);
                return;

            case 5:
                entity.AIValues[1] = 0x5a;
                index = entity.Bytes[0];
                directionTableIndex = 4;
                break;

            default:
                return;
        }

        entity.TargetDirection = gameEngine.StaticVariables.UINT_ARRAY_80027f90[directionTableIndex + (uint)index];
    }

    //80079950
    public static void AI_FUN_80079950(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        int iVar1 = 0;
        int iVar2 = 0;
        uint targetAnimationId;
        uint index;
        Entity parentEntity;
        bool label_800799a4_activated = true;

        parentEntity = entity.ParentEntity;

        if (entity.TargetAnimationId != 2)
        {
            if (parentEntity.Bytes[3] == 0)
            {
                label_800799a4_activated = false;

                if (parentEntity.AIValues[2] == 0)
                {
                    //goto LAB_800799b4;
                    iVar2 = parentEntity.PosX;
                    iVar1 = parentEntity.PosY;
                }

                if (parentEntity.AIValues[2] + 0x118 < 0x1e00000)
                {
                    //goto LAB_800799a4;
                    label_800799a4_activated = true;
                }
            }
        }

        if (entity.TargetAnimationId == 2)
        {
            if (label_800799a4_activated)
            {
                //LAB_800799a4:
                iVar1 = parentEntity.AIValues[2];

                if (iVar1 == 0)
                {
                LAB_800799b4:
                    iVar2 = parentEntity.PosX;
                    iVar1 = parentEntity.PosY;
                }
                else
                {
                    iVar2 = iVar1 + 0x114;
                    iVar1 = iVar1 + 0x118;
                }
            }

            index = (uint)(parentEntity.DelayOrAngle + entity.DelayOrAngle & 0x1ff);
            entity.PosX = iVar2 + gameEngine.StaticVariables.g_sinus[index] * (parentEntity.ItemState + 0x1000);
            entity.PosY = iVar1 - gameEngine.StaticVariables.g_cosinus[index] * (parentEntity.ItemState + 0x1000);
            targetAnimationId = entity.TargetAnimationId;

            if (targetAnimationId == 1)
            {
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }
            }
            else
            {
                if ((int)targetAnimationId < 2)
                {
                    return;
                }

                if (targetAnimationId == 2)
                {
                    if (entity.ForceResetAnimationFlag == 0)
                    {
                        return;
                    }

                    parentEntity.AIValues[4] = (short)(parentEntity.AIValues[4] - 1);
                    gameEngine.DestroyEntity(entity, -1);
                    return;
                }

                if (targetAnimationId != 3)
                {
                    return;
                }

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[3] != 0)
                {
                    //goto LAB_80079a94;
                    entity.TargetAnimationId = 2;
                    return;
                }
            }

            entity.TargetAnimationId = 0;
        }
        else
        {
        //if (parentEntity.Bytes[3] == 0)
        //{
        //    if (parentEntity.AIValues[2] == 0)
        //    {
        //        goto LAB_800799b4;
        //    }
        //
        //    if (parentEntity.AIValues[2] + 0x118 < 0x1e00000)
        //    {
        //        goto LAB_800799a4;
        //    }
        //}

        LAB_80079a94:
            entity.TargetAnimationId = 2;
        }
    }

    //80079b14
    public static void AI_FUN_80079b14(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        bool bVar1;
        byte bVar2;
        ulong uVar3;
        SpriteEffect? effect;
        Entity? entitySpawned;
        uint direction;
        int posX;
        int posY;
        int val;
        int i;
        short sVar4;
        int positionIndex;

        if (gameEngine.StaticVariables.g_loaderInitialized == 0)
        {
            gameEngine.StaticVariables.g_ai_spriteEffect_ptr = null;
            gameEngine.StaticVariables.DAT_80191258 = 0;
            gameEngine.StaticVariables.g_loaderInitialized = 1;
        }

        FUN_80079ad4(gameEngine.StaticVariables.g_ai_spriteEffect_ptr);
        FUN_80079ad4(gameEngine.StaticVariables.g_effectSlots[entity.AIValues[2]]);

        switch (entity.TargetAnimationId)
        {
            case 1:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[1] == 0)
                    {
                        entity.TargetAnimationId = 4;
                    }
                    else
                    {
                        gameEngine.SoundManager.PlaySoundEffect(0x128);
                        entity.TargetAnimationId = 3;
                        entity.AIValues[1] = 0x1e;
                        entity.Bytes[0] = 0;
                    }
                }
                break;

            case 2:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.Bytes[0] = 0;
                    entity.AIValues[1] = 0xb4;
                    effect = gameEngine.StaticVariables.g_ai_spriteEffect_ptr;
                    entity.TargetAnimationId = 0xe;

                    if (gameEngine.StaticVariables.g_ai_spriteEffect_ptr != null)
                    {
                        effect.TargetAnimation = 2;
                        gameEngine.StaticVariables.g_ai_spriteEffect_ptr = null;
                    }

                    gameEngine.StaticVariables.g_effectSlots[entity.AIValues[2]].TargetAnimation = 2;
                    entity.AIValues[2] = 0;
                    entity.AIValues[3] = 0;
                }
                break;

            case 3:
                sVar4 = entity.AIValues[1];
                if (sVar4 == 0)
                {
                    if (gameEngine.StaticVariables.g_ai_spriteEffect_ptr.TargetAnimation != 1)
                    {
                        return;
                    }

                    gameEngine.InitializeAndBeginFadeEffect();

                    if (entity.Bytes[1] == 1)
                    {
                        gameEngine.SoundManager.PlaySoundEffect(0x129);
                        entity.TargetAnimationId = 2;
                        direction = 4;

                        do
                        {
                            entitySpawned = gameEngine.SpawnWarpEntity(entity, 1, 0xa7, 0x1500000, 0x3100000, 0x1480000, direction);

                            if (entitySpawned != null)
                            {
                                entitySpawned.Bytes[0] = 4;
                                entitySpawned.AIValues[1] = 0x78;
                                entitySpawned.TargetAnimationId = 1;
                                entity.Bytes[2] = 3;
                            }
                            direction += 0xc;
                        } while (direction != 0x28);

                        return;
                    }

                    i = 0;
                    entity.TargetAnimationId = 2;
                    val = (int)((Random.Next() * 3) >> 0x20) + 1;
                    direction = (uint)(val * 0x10);
                    gameEngine.SoundManager.PlaySoundEffect(0x127);

                    do
                    {
                        entitySpawned = gameEngine.SpawnWarpEntity(entity, 1, 0xa4, 0x1500000, 0x3100000, 0x1480000, 0);

                        if (entitySpawned != null)
                        {
                            entitySpawned.TargetAnimationId = 0;
                            entitySpawned.ItemState = val * 0x200;
                            entitySpawned.AIValues[4] = (short)direction;
                            direction = (direction + 0x40) & 0x1ff;
                            entity.AIValues[4] = (short)(entity.AIValues[4] + 1);
                        }

                        i += 1;
                    } while (i != 8);

                    return;
                }

                entity.AIValues[1] = (short)(sVar4 - 1);

                if (sVar4 != 1)
                {
                    return;
                }
                if (entity.Bytes[0] == 0)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x12a);
                    gameEngine.StaticVariables.g_ai_spriteEffect_ptr = gameEngine.EffectManager.CreateEffectEntity(1, 0, 0, 0x1500000, 0x3100000, 0x1400000);
                    entity.Bytes[0] = 1;
                    return;
                }
                goto LAB_8007a278;

            case 4:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 5;
                    entity.AIValues[1] = 0xb4;
                }
                break;

            case 5:
                sVar4 = entity.AIValues[1];

                if (sVar4 == 0)
                {
                    entity.TargetAnimationId = 6;
                    return;
                }

                if (sVar4 == 0xb4 || sVar4 == 0x78 || sVar4 == 0x3c)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x125);
                    i = 0;
                    gameEngine.InitializeAndBeginFadeEffect();
                    direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    val = entity.PosZ;

                    while (true)
                    {
                        posX = entity.PosX + gameEngine.StaticVariables.g_offsetXList[direction] * i;
                        posY = entity.PosY + gameEngine.StaticVariables.g_offsetYList[direction] * i;
                        if (0x17ffffe < posX - 0x900001U || posY < 0x2700001 || 0x3afffff < posY) break;

                        i += 0xc00;
                        gameEngine.SpawnWarpEntity(entity, 1, 0xa3,
                            posX, posY, val + 0x80000,
                            (uint)gameEngine.StaticVariables.BYTE_ARRAY_80028b54[(int)((Random.Next() * 4) >> 0x20)]);
                    }
                }
                sVar4 = entity.AIValues[1];
                goto LAB_8007a1b0;

            case 6:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 2;
                }
                break;

            case 7:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[3] != 0)
                {
                    entity.TargetAnimationId = 0xf;
                    entity.AIValues[1] = 0x3c;
                    effect = gameEngine.StaticVariables.g_ai_spriteEffect_ptr;
                    entity.Flags &= 0xfffffffc;

                    if (gameEngine.StaticVariables.g_ai_spriteEffect_ptr == null)
                    {
                        return;
                    }

                    effect.TargetAnimation = 2;
                    gameEngine.StaticVariables.g_ai_spriteEffect_ptr = null;
                    return;
                }

            LAB_8007a278:
                entity.TargetAnimationId = 2;
                break;

            case 0xe:
                sVar4 = entity.AIValues[1];

                if (sVar4 == 0)
                {
                    if (entity.AIValues[4] != 0)
                    {
                        return;
                    }

                    if (entity.Bytes[0] == 1)
                    {
                        gameEngine.SoundManager.PlaySoundEffect(0x12a);
                        effect = gameEngine.EffectManager.CreateEffectEntity(1, 0, 0, entity.PosX, entity.PosY, entity.PosZ);
                        bVar2 = entity.Bytes[0];
                        entity.AIValues[2] = (short)effect.Id;
                        entity.Bytes[0] = (byte)(bVar2 + 1);
                        return;
                    }

                    if (entity.Bytes[0] == 2 && gameEngine.StaticVariables.g_effectSlots[entity.AIValues[2]].TargetAnimation == 1)
                    {
                        entity.TargetAnimationId = 1;
                    }

                    if (entity.Bytes[0] != 0)
                    {
                        return;
                    }

                    uVar3 = Random.Next();
                    entity.Bytes[1] = (byte)((uVar3 * 3) >> 0x20);

                    if ((int)((uVar3 * 3) >> 0x20) == 0)
                    {
                        positionIndex = 0;
                        gameEngine.StaticVariables.DAT_80191258 = 2;
                    }
                    else
                    {
                        if (entity.Bytes[2] != 0)
                        {
                            entity.Bytes[1] = 2;
                        }
                        positionIndex = 6;
                        gameEngine.StaticVariables.DAT_80191258 = 3;
                    }

                    i = 0;
                    val = (int)((Random.Next() * (ulong)(gameEngine.StaticVariables.DAT_80191258 + 1)) >> 0x20) << 1;

                    if (gameEngine.StaticVariables.DAT_80191258 != 0)
                    {
                        sVar4 = 0;

                        do
                        {
                            entitySpawned = gameEngine.SpawnWarpEntity(entity, 1, 0xa8,
                                gameEngine.StaticVariables.g_ai_EntityPositionXY[positionIndex + val],
                                gameEngine.StaticVariables.g_ai_EntityPositionXY[positionIndex + val + 1],
                                entity.PosZ, entity.TargetDirection);

                            if (entitySpawned != null)
                            {
                                entitySpawned.TargetAnimationId = 0xe;
                                entitySpawned.Bytes[0] = 1;
                                entitySpawned.AIValues[1] = sVar4;
                            }

                            val += 2;

                            if ((gameEngine.StaticVariables.DAT_80191258 + 1) * 2 == val)
                            {
                                val = 0;
                            }

                            i += 1;
                            sVar4 = (short)(sVar4 + 0xb);
                        } while (i != gameEngine.StaticVariables.DAT_80191258);
                    }
                    entity.PosX = gameEngine.StaticVariables.g_ai_EntityPositionXY[positionIndex + val];
                    entity.PosY = gameEngine.StaticVariables.g_ai_EntityPositionXY[positionIndex + val + 1];
                    entity.Bytes[0] = 1;
                    entity.AIValues[1] = (short)(i * 0xb);
                    return;
                }

            LAB_8007a1b0:
                entity.AIValues[1] = (short)(sVar4 - 1);
                break;

            case 0xf:
                sVar4 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar4;

                if (sVar4 == 0)
                {
                    gameEngine.StaticVariables.g_globalFlags[0] |= 1;
                    gameEngine.StaticVariables.PlayerEntity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX, entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
                }

                break;
        }
    }

    //80079ad4
    private static void FUN_80079ad4(SpriteEffect? effect)
    {
        if (effect != null
            && effect.TargetAnimation == 0
            && effect.DestroyFlag != 0)
        {
            effect.Status = 2;
            effect.DestroyFlag = 0;
            effect.TargetAnimation = 1;
        }
    }

    //8007a2f8
    public static void AI_FUN_8007a2f8(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        SpriteEffect effect;
        uint uVar1;
        uint targetAnimationId;
        int iVar3;
        Entity parentEntity;

        FUN_80079ad4(gameEngine.StaticVariables.g_effectSlots[entity.AIValues[2]]);
        parentEntity = entity.ParentEntity;

        if (parentEntity.TargetAnimationId == 7)
        {
        LAB_8007a460:
            entity.TargetAnimationId = 0xd;
            return;
        }

        switch (entity.TargetAnimationId)
        {
            case 1:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (parentEntity.Bytes[1] == 0)
                    {
                        entity.TargetAnimationId = 4;
                    }
                    else
                    {
                        entity.TargetAnimationId = 3;
                    }
                }
                break;

            case 3:
                if (parentEntity.TargetAnimationId == 2)
                {
                    entity.TargetAnimationId = 0xd;
                }
                break;

            case 4:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 5;
                }
                break;

            case 5:
                targetAnimationId = parentEntity.TargetAnimationId;
                uVar1 = 6;
                goto LAB_8007a440;

            case 6:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                //goto LAB_8007a460;
                entity.TargetAnimationId = 0xd;
                return;

            case 0xd:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.Flags |= 0x40;
                    gameEngine.StaticVariables.g_effectSlots[entity.AIValues[2]].TargetAnimation = 2;
                    entity.AIValues[2] = 0;
                    entity.AIValues[3] = 0;
                }
                break;

            case 0xe:
                if (entity.AIValues[1] != 0)
                {
                    entity.AIValues[1] += -1;
                    return;
                }

                if (entity.Bytes[0] != 0)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x12a);
                    effect = gameEngine.EffectManager.CreateEffectEntity(1, 0, 0, entity.PosX, entity.PosY, entity.PosZ);
                    entity.AIValues[2] = (short)effect.Id;
                    entity.Bytes[0] = 0;
                    return;
                }

                targetAnimationId = gameEngine.StaticVariables.g_effectSlots[entity.AIValues[2]].TargetAnimation;
                uVar1 = 1;
            LAB_8007a440:

                if (targetAnimationId == uVar1)
                {
                    entity.TargetAnimationId = targetAnimationId;
                }

                break;
        }
    }

    //8007a4a8
    public static void AI_FUN_8007a4a8(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
        //do nothing
    }

    //8007a4b0
    public static void AI_FUN_8007a4b0(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        ushort uVar1;
        Entity parentEntity;

        parentEntity = entity.ParentEntity;

        if (parentEntity.Bytes[3] == 0)
        {
            if (entity.TargetAnimationId - 1 < 2)
            {
                uVar1 = (ushort)(entity.AIValues[1] + 1);
                entity.AIValues[1] = (short)uVar1;

                if ((uVar1 & 3) == 0)
                {
                    gameEngine.EffectManager.CreateEffectEntity(1, 1, 0, entity.PosX, entity.PosY, entity.PosZ);
                }
            }

            if (entity.TargetAnimationId == 1)
            {
                if (entity.DelayOrAngle < 0x1800)
                {
                    entity.DelayOrAngle += 0x40;
                }
                else
                {
                    entity.ItemState -= 8;
                }

                entity.AIValues[4] = (short)((entity.AIValues[4] + 10U) & 0x1ff);
                entity.PosX = gameEngine.StaticVariables.g_sinus[(ushort)entity.AIValues[4]] * entity.DelayOrAngle + 0x01500000;
                entity.PosY = gameEngine.StaticVariables.g_cosinus[(ushort)entity.AIValues[4]] * entity.DelayOrAngle + 0x3100000;

                if (entity.ItemState == 0)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x127);
                    uVar1 = (ushort)entity.AIValues[4];
                    entity.TargetAnimationId = 2;
                    entity.TargetDirection = (uint)(0x20 - ((uVar1 >> 4) & 0x1f));
                }
            }
            else if (entity.TargetAnimationId == 2 && entity.ForceAdjusted != 0)
            {
                entity.TargetAnimationId = 3;
                entity.Flags |= 0x40;
                parentEntity.AIValues[4] = (short)(parentEntity.AIValues[4] - 1);
            }
        }
        else
        {
            entity.TargetAnimationId = 3;
            entity.Flags |= 0x40;
        }
    }

    //8007a680
    public static void AI_FUN_8007a680(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        byte bVar1;
        short sVar2;
        uint uVar3;
        int iVar4;
        Entity parentEntity;

        parentEntity = entity.ParentEntity;
        if (parentEntity.Bytes[3] != 0)
        {
            entity.TargetAnimationId = 2;
            entity.Flags |= 0x40;
            return;
        }

        switch (entity.TargetAnimationId)
        {
            case 0:
                sVar2 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar2;

                if (sVar2 != 0)
                {
                    return;
                }

                bVar1 = entity.Bytes[0];
                entity.AIValues[1] = 0x28;
                goto LAB_8007a738;

            case 1:
                sVar2 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar2;

                if (sVar2 != 0)
                {
                    return;
                }

                bVar1 = entity.Bytes[0];
                entity.AIValues[1] = 0x14;
            LAB_8007a738:
                entity.TargetAnimationId = bVar1;
                break;

            case 3:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[3] == 0)
                    {
                        bVar1 = entity.Bytes[0];
                        entity.AIValues[1] = 0x1e;
                        entity.TargetAnimationId = 0;
                        if (bVar1 != 7)
                        {
                            entity.Bytes[0] = (byte)(bVar1 + 1);
                        }
                    }
                    else
                    {
                        entity.TargetAnimationId = 2;
                        entity.Flags |= 0x40;
                        parentEntity.Bytes[2] -= 1;
                    }
                }
                break;

            case 4:
            case 5:
            case 6:
            case 7:
                sVar2 = entity.AIValues[1];

                if (entity.AIValues[1] > 0)
                {
                    entity.AIValues[1] = (short)(sVar2 - 1);
                }

                if (entity.AIValues[1] == 0)
                {
                    uVar3 = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    iVar4 = (int)(entity.TargetDirection - uVar3);

                    if (0xf < iVar4 || iVar4 + 0x10U < 0x10)
                    {
                        entity.Bytes[1] = 1;
                    }

                    if (iVar4 - 1U < 0xf || iVar4 < -0x10)
                    {
                        entity.Bytes[1] = 0xff;
                    }

                    if (iVar4 < 0)
                    {
                        iVar4 = -iVar4;
                    }

                    if (7 < iVar4)
                    {
                        entity.Bytes[1] <<= 1;
                    }

                    entity.TargetDirection = entity.TargetDirection + entity.Bytes[1] & 0x1f;
                    entity.AIValues[1] = (short)((entity.TargetAnimationId - 4) * -4 + 0x12);
                }

                break;
        }
    }

    //8007a8a0
    public static void AI_UpdateIceProjectile(GameEngine gameEngine, Entity entity)
    {
        //if (entity.Name != "mimique nv1")
        {
            Debugger.Break();
        }

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

        gameEngine.CheckAndTriggerTileEffect(entity);
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

        AI_FUN_8007b04c_common(gameEngine, entity, 0x800, 0x180000);
    }

    private static void AI_FUN_8007b04c_common(GameEngine gameEngine, Entity entity, int factor, int offsetX)
    {
        var byte0 = entity.Bytes[0];

        if (byte0 != 1)
        {
            if (1 < byte0)
            {
                if (byte0 != 2)
                {
                    return;
                }

                entity.AIValues[1] = (short)((entity.AIValues[1] + 4U) & 0x1ff);
                entity.PosX = entity.DelayOrAngle + gameEngine.StaticVariables.g_sinus[(ushort)entity.AIValues[1]] * factor;
                entity.PosY = entity.ItemState + gameEngine.StaticVariables.g_cosinus[(ushort)entity.AIValues[1]] * factor;
                entity.Bytes[1] = (byte)(entity.Bytes[1] - 1);

                if (entity.Bytes[1] != 0)
                {
                    return;
                }

                entity.Status = 3;
                gameEngine.SoundManager.PlaySoundEffect(0xe1);
                return;
            }

            if (byte0 != 0)
            {
                return;
            }

            entity.AIValues[1] = 0x180;
            entity.Bytes[0] = 1;
            entity.DelayOrAngle = entity.PosX + offsetX;
            entity.ItemState = entity.PosY;
        }

        entity.AIValues[1] = (short)((entity.AIValues[1] + 4U) & 0x1ff);
        entity.PosX = entity.DelayOrAngle + gameEngine.StaticVariables.g_sinus[(ushort)entity.AIValues[1]] * factor;
        entity.PosY = entity.ItemState + gameEngine.StaticVariables.g_cosinus[(ushort)entity.AIValues[1]] * factor;
        var iVar3 = gameEngine.GetMatchingEntityBySearchType(entity, entity.EntityRefId - 1);

        if (iVar3 == 0)
        {
            entity.Bytes[0] = (byte)(entity.Bytes[0] + 1);
            entity.Bytes[1] = 4;
        }
    }

    //8007b1f0
    public static void AI_FUN_8007b1f0(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();

        AI_FUN_8007b04c_common(gameEngine, entity, 0xc00, 0x240000);
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
            entity.TargetAnimationId = 0;
            return;
        }

        if (entity.Flags2 == -1)
        {
            entity.TargetAnimationId = 3;
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
    //Goutte d’eau
    public static void AI_FUN_8007b834(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "Goutte d’eau")
        {
            Debugger.Break();
        }

        byte bVar1;
        short sVar2;
        int iVar3;
        Entity entity2;

        switch (entity.Bytes[0])
        {
            case 0:
                entity.Bytes[0] = 1;
                entity.AIValues[1] = 0x28;
                break;

            case 1:
                sVar2 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar2;

                if (sVar2 == 0)
                {
                    entity.TargetAnimationId = 1;
                    entity.Bytes[0] = 2;
                    entity.Flags |= 0x100;
                }
                break;

            case 2:
                if (entity.CollidedWithEntityZ != 0 || entity.IsAboveGround != 0)
                {
                    entity.TargetAnimationId = 2;
                    entity.Bytes[0] = 3;
                }
                break;

            case 3:
                entity.Bytes[0] = 4;
                entity.Flags = entity.Flags & 0xffffff7f;
                entity2 = entity.RidingEntity;

                if (entity.RidingEntity == gameEngine.StaticVariables.PlayerEntity)
                {
                    entity2 = gameEngine.StaticVariables.PlayerEntity.CarriedEntity;
                }

                if (entity2 != null && entity2.SpriteTableIndex == 0x189)
                {
                    entity2.Status = 3;
                    entity2.Bytes[0] = 1;
                }
                break;

            case 4:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    bVar1 = entity.Bytes[1];
                    entity.Bytes[0] = 0;
                    gameEngine.StaticVariables.g_clearProgramState = 1;
                    entity.TargetAnimationId = 0;
                    entity.ForceZ = 0;
                    entity.Flags = entity.Flags & 0xfffffeffU | 0x80;
                    iVar3 = entity.PosZ;
                    entity.ProgramIndexes[2] = bVar1;
                    entity.PosZ = iVar3 + 0xa00000;
                }

                break;
        }
    }

    //8007b998
    public static void AI_ProcessWarpTransitionState(GameEngine gameEngine, Entity entity)
    {
        //if (entity.Name != "mimique nv1")
        {
            Debugger.Break();
        }

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

                if (gameEngine.IsDialogFinished())
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
                entity.DelayOrAngle -= 1;

                if (entity.DelayOrAngle != -1)
                {
                    return;
                }

                gameEngine.StaticVariables.g_warpStatusFlag = 0;

                arg1 = gameEngine.EtcRes.GetEtcString(0x41);
                arg2 = gameEngine.EtcRes.GetEtcString(0x42);

                int r = gameEngine.InitializeAsyncOperation(arg1, arg2, result => gameEngine.StaticVariables.g_warpStatusFlag = (uint)result);

                if (r != 0)
                {
                    WriteWarpState(entity, state + 1);
                    state++;
                }

                WriteWarpState(entity, state + 1);
                return;

            case 4:
                {
                    if (gameEngine.StaticVariables.g_warpStatusFlag == 0)
                    {
                        return;
                    }

                    gameEngine.UIManager.TryActivateTextHoldState();

                    if (gameEngine.StaticVariables.g_warpStatusFlag != 1)
                    {
                        ResetWarpState(gameEngine, entity);
                        return;
                    }

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
        if (entity.Name != "Pot magique (boïng)")
        {
            Debugger.Break();
        }

        switch ((int)entity.TargetAnimationId - 1)
        {
            case 1:
                {
                    if ((entity.FrameCounter & 3) != 0)
                    {
                        break;
                    }

                    var effect = gameEngine.EffectManager.CreateEffectEntity(0, 0x0E, 0, entity.PosX, entity.PosY, entity.PosZ);
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

                    if (gameEngine.StaticVariables.PlayerEntity.RidingEntity == entity)
                    {
                        entity.ForceZ = -0x8000;
                        entity.TargetAnimationId = 3;
                    }
                    break;
                }

            case 3:
                {
                    if (gameEngine.StaticVariables.PlayerEntity.RidingEntity != entity)
                    {
                        entity.TargetAnimationId = 2;
                    }
                    break;
                }

            case 4:
                {
                    if (gameEngine.StaticVariables.PlayerEntity.RidingEntity == entity)
                    {
                        break;
                    }

                    if (gameEngine.StaticVariables.PlayerEntity.ForceZ > 0)
                    {
                        gameEngine.StaticVariables.PlayerEntity.ForceZ = 0x00098000;
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
        Debugger.Break();

        if (gameEngine.StaticVariables.PlayerEntity.XCollisionEntity == entity)
        {
            if (entity.Bytes[0] < 0x1f)
            {
                entity.Bytes[0] = entity.Bytes[1];
                return;
            }

            gameEngine.SoundManager.PlaySoundEffect(0x19);
            entity.TargetAnimationId = 1;
            var result = FUN_8003ac9c(entity, gameEngine.StaticVariables.PlayerEntity);

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

                return (entity2.PosY < entity1.PosY ? 1 : 0) << 4;
            }
        }
        else if (diffX <= entity1.Width)
        {
        LAB_8003acf4:
            return (entity2.PosY < entity1.PosY ? 1 : 0) << 4;
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

        SpriteEffect effect;

        gameEngine.CheckAndTriggerTileEffect(entity);

        if ((entity.FrameCounter & 3U) == 0)
        {
            effect = gameEngine.EffectManager.CreateEffectEntity(0, 0x19, (byte)entity.AnimationDirection, entity.PosX, entity.PosY, entity.PosZ + 0x80000);

            if (effect != null)
            {
                effect.ForceX = -entity.ForceX >> 1;
                effect.ForceY = -entity.ForceY >> 1;
            }
        }
    }

    //8007c768
    //P-Zoldia Niv.1
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
        if (entity.Name != "◆Guêpe Niv.1")
        {
            Debugger.Break();
        }

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
                        if (positions[0] < 3 && positions[1] < 3 && positions[2] < 0x200001)
                        {
                            direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                                    gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                            entity.TargetDirection = direction;
                            entity.AIValues[1] = 0x3c;
                            return;
                        }

                        entity.TargetDirection = (uint)(Random.Next() % 32);
                        z = entity.PosZ + entity.AIValues[0] * -0x10000;
                        rand = (uint)(((Random.Next() * 0x29) >> 0x20) + 0x50);
                        entity.AIValues[1] = (short)rand;

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
                            if (positions[5] < 0x180001 || entity.IsAboveGround != 0)
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
                    direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
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
                direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                    gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
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

                if (sVar3 == 0 || entity.IsAboveGround != 0)
                {
                    entity.TargetAnimationId = 0;
                }

                LAB_8006a8b8:
                if (entity.Bytes[0] == 0
                    && gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, positions, 1, 4, 0x200000))
                {
                    LAB_8006a8e8:
                    entity.TargetAnimationId = 0;
                    direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
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
                        entity.Flags |= 0x40;
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

        x = entity.TileX - gameEngine.StaticVariables.PlayerEntity.TileX;
        y = entity.TileY - gameEngine.StaticVariables.PlayerEntity.TileY;
        z = entity.PosZ - gameEngine.StaticVariables.PlayerEntity.FloorHeight;
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

    // 80066bf8
    public static void AI_UpdateEntityAI_0(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Homme momie Niv.1")
        {
            Debugger.Break();
        }

        short sVar1;
        bool bVar2;
        uint direction;
        int iVar3;
        int[] relPos = new int[6];
        byte newPhase;

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relPos);

        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.IsAboveGround == 0)
                {
                    return;
                }

                if (4 < entity.Bytes[1] - 3)
                {
                    entity.AIValues[1] = 0;
                    entity.Bytes[1] = 0;
                    goto switchD_80066c3c_PHASE_1_START;
                }

                sVar1 = entity.AIValues[1];
                entity.AIValues[1] = (short)(sVar1 - 1);

                if (entity.AIValues[1] > 0)
                {
                    return;
                }

                direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                newPhase = entity.Bytes[1];
                entity.TargetDirection = direction;
                entity.TargetAnimationId = 3;
                entity.DelayOrAngle = 0;

                if (newPhase == 6)
                {
                    entity.Bytes[1] = 0;
                    return;
                }

                newPhase = (byte)(newPhase + 1);
                break;

            case 1:
            switchD_80066c3c_PHASE_1_START:
                if (entity.ItemState != 0)
                {
                    entity.ItemState = entity.ItemState + -1;
                }

                if (entity.AIValues[1] != 0)
                {
                    iVar3 = entity.ForceAdjusted;
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);

                    if (iVar3 != 0)
                    {
                        gameEngine.EntityGameplayManager.UpdateDirectionForced(entity, 1, 4, 0x100000);
                        return;
                    }

                    if (entity.ItemState != 0)
                    {
                        return;
                    }

                    if (2 < relPos[0])
                    {
                        return;
                    }

                    if (2 < relPos[1])
                    {
                        return;
                    }

                    if (0x100000 < relPos[2])
                    {
                        return;
                    }

                    direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    newPhase = entity.Bytes[1];
                    entity.TargetDirection = direction;

                    if (newPhase == 1)
                    {
                        entity.TargetAnimationId = 3;
                        entity.DelayOrAngle = 0;
                        return;
                    }

                    if (newPhase != 2)
                    {
                        return;
                    }

                    entity.Bytes[1] = 7;
                    entity.AIValues[1] = 0x3c;
                    entity.TargetAnimationId = 0;
                    return;
                }

                entity.TargetAnimationId = 1;
                entity.AIValues[1] = 0xf0;

                if (gameEngine.EntityGameplayManager.TryAttackPlayer(entity, relPos, 6, 0x100000))
                {
                    direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.Bytes[1] = 1;
                    return;
                }

                gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0xb4, 0x14);
                newPhase = 2;
                break;

            case 3:
                if (gameEngine.StaticVariables.PlayerEntity.TouchingEntity == entity)
                {
                    entity.DelayOrAngle = 1;
                }

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.DelayOrAngle != 0)
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0x1e;
                    entity.DelayOrAngle = 0;
                    return;
                }

                gameEngine.EntityGameplayManager.SetEntityRandomDirection(entity, 1, 0x78);
                entity.ItemState = entity.AIValues[1];
                return;

            case 6:
            case 0xb:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[3] != 0)
                {
                    entity.TargetAnimationId = 7;
                    entity.Flags = entity.Flags | 0x40;
                    return;
                }

                direction = entity.TargetDirection;
                entity.AIValues[1] = 10;
                newPhase = entity.Bytes[1];
                entity.TargetAnimationId = 0;
                entity.TargetDirection = direction + 0x10 & 0x1f;

                if (newPhase - 3 < 4)
                {
                    return;
                }

                newPhase = 3;
                break;

            default:
                goto switchD_80066c3c_caseD_2;
        }

        entity.Bytes[1] = newPhase;

        switchD_80066c3c_caseD_2:
        return;
    }

    //80066f38
    //◆Mimique Niv.1
    public static void AI_UpdateEntityAI_3(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Mimique Niv.1")
        {
            Debugger.Break();
        }

        bool bVar1;
        short sVar2;
        uint direction;
        int[] positions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, positions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.AIValues[1] == 0)
                {
                    entity.TargetAnimationId = 1;
                    entity.AIValues[1] = 1;
                }
                else
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                }
                break;

            case 1:
                sVar2 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar2;

                if (sVar2 == 0)
                {
                    gameEngine.EntityGameplayManager.SetEntityRandomDirection(entity, 1, 0xf0);
                }
                else if (entity.ForceAdjusted != 0)
                {
                    gameEngine.EntityGameplayManager.UpdateDirectionForced(entity, 1, 2, 0x100000);

                    if (entity.TargetAnimationId == 1)
                    {
                        entity.TargetAnimationId = 0;
                    }
                    entity.AIValues[1] = 0x3c;
                }
                break;

            case 2:
                if (entity.IsAboveGround != 0)
                {
                    entity.TargetAnimationId = 1;
                }
                break;

            case 3:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 1;
                    direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.AIValues[1] = 300;
                }
                break;

            case 4:
                if (gameEngine.EntityGameplayManager.TryAttackPlayer(entity, positions, 4, 0x100000))
                {
                    entity.TargetAnimationId = 3;
                }
                break;

            case 6:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[3] == 0)
                    {
                        if ((int)((Random.Next() * 4) >> 0x20) == 0)
                        {
                            gameEngine.SpawnEntityContents(entity);
                        }

                        entity.TargetAnimationId = 0;
                        entity.AIValues[1] = 0x1e;
                    }
                    else
                    {
                        entity.TargetAnimationId = 7;
                        entity.Flags |= 0x40;
                    }
                }
                break;
        }
    }

    //80067138
    public static void AI_UpdateEntityAI_4(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006790c
    public static void AI_UpdateEntityAI_5(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80067d98
    public static void AI_UpdateEntityAI_6(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80068154
    //muruta griffes nv1
    public static void AI_UpdateEntityAI_6_2(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Muruta (griffes) Niv.1")
        {
            Debugger.Break();
        }

        byte state;
        bool bVar2;
        short sVar3;
        uint direction;
        int iVar4;
        int[] positions = new int[6];

        if (gameEngine.StaticVariables.g_currentMap == 0x99 && entity.ParentEntity.Bytes[3] != 0)
        {
            gameEngine.DestroyEntity(entity, -1);
            return;
        }

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, positions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.Bytes[0] == 0)
                {
                    if (gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, positions, 2, 6, 0x100000))
                    {
                        entity.Bytes[0] = 1;
                        entity.AIValues[1] = 0;
                        goto switchD_800685cc_caseD_8;
                    }

                    sVar3 = (short)(entity.AIValues[1] - 1);

                    if (entity.AIValues[1] != 0)
                    {
                        //goto LAB_80068900;
                        entity.AIValues[1] = sVar3;
                        goto switchD_800685cc_caseD_8;
                    }

                    if (entity.Bytes[1] == 0 && (int)((Random.Next() * 4) >> 0x20) != 1)
                    {
                        goto switchD_800685cc_caseD_8;
                    }

                    state = entity.Bytes[1];
                    entity.Bytes[1] = (byte)(state + 1);

                    switch (state)
                    {
                        case 0:
                        case 1:
                        case 2:
                            if ((int)((Random.Next() * 4) >> 0x20) == 0)
                            {
                                gameEngine.SoundManager.PlaySoundEffect(0xcd);
                            }
                            entity.TargetAnimationId = 8;
                            direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                            entity.TargetDirection = direction;
                            goto switchD_800685cc_caseD_8;

                        case 3:
                            state = gameEngine.StaticVariables.BYTE_ARRAY_80028b54[(int)((Random.Next() * 4) >> 0x20)];
                            break;

                        case 4:
                            state = gameEngine.StaticVariables.g_directionFlipTable[entity.TargetDirection];
                            break;

                        case 5:
                            entity.TargetAnimationId = 0xc;
                            sVar3 = 0x78;
                            //goto LAB_80068900;
                            entity.AIValues[1] = sVar3;
                            goto switchD_800685cc_caseD_8;

                        default:
                            entity.Bytes[1] = 0;
                            entity.AIValues[1] = 0;
                            goto switchD_800685cc_caseD_8;
                    }
                    entity.AIValues[1] = 0x14;
                    entity.ForceStepY = 0;
                    entity.ForceStepX = 0;
                    entity.ForceY = 0;
                    entity.ForceX = 0;
                    entity.TargetForceY = 0;
                    entity.TargetForceX = 0;
                    entity.TargetDirection = state;
                    goto switchD_800685cc_caseD_8;
                }

                if (!gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, positions, 2, 6, 0x100000))
                {
                    sVar3 = 0x1e;

                    if (!gameEngine.EntityGameplayManager.TryAttackPlayer2(entity, positions, (byte)entity.AnimationDirection ^ 1, 2, 3, 0x100000))
                    {
                        entity.Bytes[1] = 0;
                        entity.Bytes[0] = 0;
                        //goto LAB_80068900;
                        entity.AIValues[1] = sVar3;
                        goto switchD_800685cc_caseD_8;
                    }
                }

                sVar3 = (short)(entity.AIValues[1] - 1);

                if (entity.AIValues[1] != 0)
                {
                    //goto LAB_80068900;
                    entity.AIValues[1] = sVar3;
                    goto switchD_800685cc_caseD_8;
                }

                direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                entity.TargetDirection = direction;
                if (gameEngine.StaticVariables.PlayerEntity.FrameCollision != null &&
                    gameEngine.StaticVariables.PlayerEntity.BalanceAnimValRef != null &&
                    gameEngine.StaticVariables.PlayerEntity.BalanceAnimValRef.Val != 0 &&
                    positions[0] < 5 && positions[1] < 5)
                {
                    if ((int)((Random.Next() * 4) >> 0x20) == 0)
                    {
                        gameEngine.SoundManager.PlaySoundEffect(0xcd);
                    }

                    entity.TargetAnimationId = 3;
                    direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    break;
                }

                if (2 < positions[0] || 2 < positions[1])
                {
                    if (positions[0] < 5 && positions[1] < 5)
                    {
                        switch ((int)((Random.Next() * 5) >> 0x20))
                        {
                            case 0:
                            case 1:
                            switchD_80068710_caseD_0:
                                entity.TargetAnimationId = 8;
                                goto switchD_800685cc_caseD_8;

                            case 2:
                            case 3:
                                entity.AIValues[1] = 0x14;
                                entity.TargetAnimationId = 0xc;
                                sVar3 = 0x1e;
                                break;

                            case 4:
                                entity.TargetAnimationId = 0xc;
                                sVar3 = 0x1e;
                                break;

                            default:
                                goto switchD_800685cc_caseD_8;
                        }
                        goto LAB_80068900;
                    }

                    if (6 < positions[0] || 6 < positions[1])
                    {
                        goto switchD_800685cc_caseD_8;
                    }

                    switch ((int)((Random.Next() * 8) >> 0x20))
                    {
                        case 0:
                        case 1:
                        case 2:
                            entity.TargetAnimationId = 0xc;
                            sVar3 = 0x3c;
                            break;

                        case 3:
                            entity.TargetAnimationId = 0xc;
                            sVar3 = 0x1e;
                            break;

                        case 4:
                        case 5:
                            entity.TargetAnimationId = 4;
                            goto switchD_800685cc_caseD_8;

                        case 6:
                        case 7:
                            //goto switchD_80068710_caseD_0;
                            entity.TargetAnimationId = 8;
                            goto switchD_800685cc_caseD_8;

                        default:
                            goto switchD_800685cc_caseD_8;
                    }
                LAB_80068900:
                    entity.AIValues[1] = sVar3;
                    goto switchD_800685cc_caseD_8;
                }

                switch ((int)((Random.Next() * 8) >> 0x20))
                {
                    case 0:
                        entity.TargetAnimationId = 0xc;
                        sVar3 = 0x14;
                        //goto LAB_80068900;
                        entity.AIValues[1] = sVar3;
                        goto switchD_800685cc_caseD_8;

                    case 1:
                        entity.TargetAnimationId = 0xc;
                        direction = entity.TargetDirection;
                        entity.AIValues[1] = 0x3c;
                        break;

                    case 2:
                    case 3:
                        entity.TargetAnimationId = 0xc;
                        direction = entity.TargetDirection;
                        entity.AIValues[1] = 100;
                        break;

                    case 4:
                    case 5:
                        if ((int)((Random.Next() * 4) >> 0x20) == 0)
                        {
                            gameEngine.SoundManager.PlaySoundEffect(0xcd);
                        }
                        entity.TargetAnimationId = 7;
                        goto switchD_800685cc_caseD_8;

                    case 6:
                    case 7:
                        direction = entity.TargetDirection;
                        entity.TargetAnimationId = 4;
                        break;
                    default:
                        goto switchD_800685cc_caseD_8;
                }
                entity.TargetDirection = (direction - 0x10) & 0x1f;
                goto switchD_800685cc_caseD_8;

            case 5:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    goto switchD_800685cc_caseD_8;
                }

                if (entity.Bytes[3] != 0)
                {
                    entity.TargetAnimationId = 6;
                    entity.Flags |= 0x40;
                    goto switchD_800685cc_caseD_8;
                }

            LAB_800688d4:
                entity.Bytes[0] = 1;
                entity.TargetAnimationId = 0;
                break;

            case 9:
            case 10:
                sVar3 = 0x14;
                if (entity.IsAboveGround == 0)
                {
                    goto switchD_800685cc_caseD_8;
                }

                entity.TargetAnimationId = 0;
                //goto LAB_80068900;
                entity.AIValues[1] = sVar3;
                goto switchD_800685cc_caseD_8;

            case 0xc:
                if (entity.Bytes[0] == 0 && gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, positions, 2, 6, 0x100000))
                {
                    goto LAB_800688d4;
                }

                sVar3 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar3;

                if (sVar3 == 0 || entity.ForceAdjusted != 0)
                {
                    entity.TargetAnimationId = 0;
                    entity.ForceStepY = 0;
                    entity.ForceStepX = 0;
                    entity.ForceY = 0;
                    entity.ForceX = 0;
                    entity.TargetForceY = 0;
                    entity.TargetForceX = 0;
                }
                else
                {
                    if (gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 0xc, 0x400000) == 0)
                    {
                        goto switchD_800685cc_caseD_8;
                    }

                    entity.TargetAnimationId = 0;
                }
                break;

            default:
                //goto switchD_800685cc_caseD_8;
                entity.TargetDirection = (entity.TargetDirection + 2) & 0x1c;
                break;
        }

        entity.AIValues[1] = 0x1e;
        entity.Bytes[1] = 0;

    switchD_800685cc_caseD_8:
        entity.TargetDirection = (entity.TargetDirection + 2) & 0x1c;
    }

    //80068930
    public static void AI_UpdateEntityAI_7(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80068cc8
    public static void AI_UpdateEntityAI_8(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80069684
    public static void AI_UpdateEntityAI_8_2(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //800699c4
    public static void AI_UpdateEntityAI_9(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80069c84
    //◆Orc (hache) Niv.1
    public static void AI_UpdateEntityAI_10(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Orc (hache) Niv.1")
        {
            Debugger.Break();
        }

        byte bVar1;
        short sVar2;
        ulong uVar3;
        bool bVar4;
        byte bVar5;
        uint uVar6;
        int iVar7;
        int[] positions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, positions);

        switch (entity.TargetAnimationId)
        {
            case 0:
            case 1:
                if (entity.Bytes[0] == 0)
                {
                    if (entity.Bytes[1] == 0)
                    {
                        bVar5 = gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection];
                        entity.Bytes[1] = 4;
                        entity.ForceStepY = 0;
                        entity.ForceStepX = 0;
                        entity.ForceY = 0;
                        entity.ForceX = 0;
                        entity.TargetForceY = 0;
                        entity.TargetForceX = 0;
                        entity.TargetDirection = (uint)((bVar5 - 8) & 0x1f);
                    }

                    sVar2 = entity.AIValues[1];
                    entity.TargetAnimationId = 1;

                    if (sVar2 == 0)
                    {
                        entity.AIValues[1] = 0x1e;

                        if (gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, positions, 2, 2, 0))
                        {
                            if ((int)((Random.Next() * 5) >> 0x20) == 0)
                            {
                                gameEngine.SoundManager.PlaySoundEffect(0x94);
                            }

                            entity.Bytes[0] = 3;
                        }
                    }
                    else
                    {
                        iVar7 = entity.ForceAdjusted;
                        entity.AIValues[1] = (short)(sVar2 - 1);

                        if (iVar7 != 0)
                        {
                            bVar5 = entity.Bytes[1];
                            bVar1 = gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection];
                            entity.ForceStepY = 0;
                            entity.ForceStepX = 0;
                            entity.ForceY = 0;
                            entity.ForceX = 0;
                            entity.TargetForceY = 0;
                            entity.TargetForceX = 0;
                            entity.Bytes[1] = (byte)(bVar5 - 1);
                            entity.TargetDirection = (uint)((bVar1 - 0x10) & 0x1f);
                        }
                    }
                }
                else
                {
                    entity.TargetAnimationId = 3;
                    uVar6 = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = uVar6;
                }
                break;

            case 3:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    bVar5 = (byte)(entity.Bytes[0] - 1);
                    entity.Bytes[0] = bVar5;

                    if (bVar5 == 0)
                    {
                        entity.TargetAnimationId = 0;
                        entity.AIValues[1] = 0;
                        entity.Bytes[1] = 0;
                        entity.TargetDirection = (uint)((Random.Next() * 4) >> 0x20) << 3;
                    }
                    else
                    {
                        entity.CurrentAnimationId = 0xffffffff;
                    }
                }
                break;

            case 5:
            case 6:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[3] == 0)
                    {
                        if (positions[0] < 2 && positions[1] < 2)
                        {
                            entity.Bytes[0] = 3;
                        }

                        entity.TargetAnimationId = 0;
                    }
                    else
                    {
                        entity.TargetAnimationId = 2;
                        entity.Flags |= 0x40;
                    }
                }

                break;
        }
    }

    //8006a29c
    //orc armure de fer
    public static void AI_UpdateEntityAI_11(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "orc armure de fer")
        {
            Debugger.Break();
        }

        short sVar2;
        int iVar3;
        uint uVar4;
        int[] positions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, positions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.AIValues[1] == 0)
                {
                    entity.AIValues[1] = 0x3c;
                    entity.TargetDirection = (uint)((Random.Next() * 0x20) >> 0x20);
                }
                sVar2 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar2;

                if (sVar2 != 0)
                {
                    return;
                }

                entity.TargetAnimationId = 1;
                sVar2 = (short)(((Random.Next() * 0x20) >> 0x20) + 0x78);
                break;

            case 1:
                sVar2 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar2;

                if (sVar2 == 0)
                {
                    entity.TargetAnimationId = 0;
                    return;
                }

                if (positions[4] < 1 && positions[0] < 4 && positions[2] < 0x100001)
                {
                    uVar4 = 4;

                    if (positions[0] != 0 || positions[4] < -4)
                    {
                        if (-2 < positions[4])
                        {
                            uVar4 = 2;

                            if (positions[3] < -1)
                            {
                                LAB_8006a468:
                                entity.TargetAnimationId = uVar4;
                                entity.TargetDirection = 0x18;
                                return;
                            }

                            entity.TargetAnimationId = 2;
                            goto LAB_8006a460;
                        }

                        if (positions[4] < -4)
                        {
                            goto LAB_8006a478;
                        }

                        uVar4 = 3;

                        if (positions[3] < -1)
                        {
                            //goto LAB_8006a468;
                            entity.TargetAnimationId = uVar4;
                            entity.TargetDirection = 0x18;
                            return;
                        }
                    }

                    entity.TargetAnimationId = uVar4;
                    LAB_8006a460:
                    entity.TargetDirection = 0;
                    return;
                }

                LAB_8006a478:
                if (entity.ForceAdjusted != 0)
                {
                    entity.TargetDirection = (entity.TargetDirection - 0x10) & 0x1f;
                    sVar2 = 0x78;

                    if (entity.AnimationDirection == 0)
                    {
                        sVar2 = 0xf0;
                    }

                    entity.AIValues[1] = sVar2;
                    entity.TargetAnimationId = 1;
                    return;
                }

                iVar3 = CanMoveForward(entity, 0);

                code_r0x8006a4d0:
                if (iVar3 == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 0;
                entity.AIValues[1] = 0;
                return;

            case 2:
            case 3:
            case 4:
                iVar3 = entity.ForceResetAnimationFlag;
                goto code_r0x8006a4d0;

            case 6:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[3] != 0)
                {
                    entity.TargetAnimationId = 7;
                    entity.Flags |= 0x40;
                    return;
                }

                entity.TargetAnimationId = 1;
                uVar4 = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                entity.TargetDirection = uVar4;
                sVar2 = 0x78;
                break;

            default:
                return;
        }

        entity.AIValues[1] = sVar2;
    }

    //8006a974
    public static void AI_UpdateEntityAI_12(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Poisson Niv.1")
        {
            Debugger.Break();
        }

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

            targetAnimationId = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
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
        floorToMapHeight3 -= entity.MapHeights[3];
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
                if (floorToMapHeight3 <= adjustedZOffset)
                {
                    goto SkipAllChecks;
                }

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

    //8006abb0
    public static void AI_UpdateEntityAI_13(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006b234
    public static void AI_UpdateEntityAI_14(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006b8d4
    //Champignon
    public static void AI_UpdateEntityAI_15(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "Champignon")
        {
            Debugger.Break();
        }

        short delay;
        uint direction;
        int state;
        byte val1;
        int[] positions = new int[6];
        byte val0;

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, positions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                delay = (short)(entity.AIValues[1] - 1);
                if (entity.AIValues[1] != 0)
                {
                    goto LAB_8006bd14;
                }

                if (2 < positions[0] || 2 < positions[1])
                {
                    gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x3c, 100);
                    return;
                }

                entity.TargetAnimationId = 5;
                direction = (uint)ScriptHelper.GetDirectionToTarget(
                    gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                    gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                entity.TargetDirection = direction;
                goto LAB_8006ba40;

            case 1:
                state = entity.ItemState - 1;

                if (entity.ItemState != 0)
                {

                    entity.ItemState = state;

                    if (state != 0 || 2 < positions[0] || 2 < positions[1])
                    {
                        delay = (short)(entity.AIValues[1] - 1);
                        entity.AIValues[1] = delay;

                        if (delay != 0)
                        {
                            if (entity.ForceAdjusted != 0)
                            {
                                entity.Bytes[0] = (byte)(entity.Bytes[0] + 1);
                                gameEngine.EntityGameplayManager.UpdateDirectionForced(entity, 1, 2, 0x100000);
                                return;
                            }

                            gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 1, 0x100000);
                            return;
                        }

                        val0 = entity.Bytes[0];
                        val1 = (byte)(entity.Bytes[1] + 1);
                        entity.Bytes[1] = val1;

                        if (val0 < 3 && val1 < 5)
                        {
                            gameEngine.EntityGameplayManager.SetEntityRandomDirection(entity, 1, 0x3c);
                            return;
                        }

                        entity.TargetAnimationId = 10;
                        entity.Bytes[1] = 0;
                        entity.Bytes[0] = 0;
                        return;
                    }
                }

                entity.TargetAnimationId = 5;

            LAB_8006ba40:
                entity.AIValues[1] = 0x10;
                entity.Bytes[1] = 0;
                break;

            case 3:
                if (positions[0] < 5 && positions[1] < 5 && positions[2] < 0x100001)
                {
                    entity.TargetAnimationId = 4;
                }
                break;

            case 4:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0;
                }
                break;

            case 5:
                delay = entity.AIValues[1];
                if (delay != 0)
                {
                    entity.AIValues[1] = (short)(delay - 1);

                    if (delay == 1)
                    {
                        state = entity.PosZ + 0x200000;
                        gameEngine.SpawnWarpEntity(entity, 1, 0xbc, entity.PosX, entity.PosY, state, entity.TargetDirection);
                        gameEngine.SpawnWarpEntity(entity, 1, 0xbc, entity.PosX, entity.PosY, state, (entity.TargetDirection + 8) & 0x1f);
                        gameEngine.SpawnWarpEntity(entity, 1, 0xbc, entity.PosX, entity.PosY, state, (entity.TargetDirection + 0x10) & 0x1f);
                        gameEngine.SpawnWarpEntity(entity, 1, 0xbc, entity.PosX, entity.PosY, state, (entity.TargetDirection + 0x18) & 0x1f);
                    }
                }

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.DelayOrAngle == 0 && (int)((Random.Next() * 4) >> 0x20) == 0)
                {
                    entity.CurrentAnimationId = 0xffffffff;
                    entity.AIValues[1] = 0x10;
                    entity.DelayOrAngle = 1;
                    return;
                }

                entity.DelayOrAngle = 0;
                entity.TargetAnimationId = 1;
                entity.ItemState = 0x1e;
                delay = (short)(((Random.Next() * 0x10) >> 0x20) + 0x1e);
                goto LAB_8006bd14;

            case 7:
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

                delay = 0x28;
                entity.TargetAnimationId = 0;
            LAB_8006bd14:
                entity.AIValues[1] = delay;
                break;

            case 10:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 3;
                }
                break;
        }
    }

    //8006bd30
    //◆Slime gélatineux
    public static void AI_UpdateEntityAI_17(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Slime gélatineux")
        {
            Debugger.Break();
        }

        uint direction;
        Entity entity2;
        int[] positions = new int[6];
        byte newDirection;
        ulong rand;
        short state;

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, positions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                state = entity.AIValues[1];
                entity.AIValues[1] = (short)(state - 1);

                if (state > 0)
                {
                    return;
                }

                if (positions[0] < 3 && positions[1] < 3
                                     && positions[2] < 0x200001
                                     && entity.Bytes[1] == 0)
                {
                    direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.TargetAnimationId = 3;
                    entity.Bytes[1] = 1;
                    state = 0x37;
                    goto LAB_8006c0e8;
                }

                entity.TargetAnimationId = 1;
                entity.AIValues[1] = (short)(((Random.Next() * 0x3d) >> 0x20) + 0x3c);

                if (positions[0] < 4 && positions[1] < 4)
                {
                    direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                }
                break;

            case 1:
                state = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = state;

                if (state != 0)
                {
                    return;
                }

                if (entity.ForceAdjusted == 0)
                {
                    rand = Random.Next();
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = (short)(((rand * 0x2000) >> 0x20) & 0x7f);
                    entity.TargetDirection = (uint)((rand * 0x2000) >> 0x28);
                    return;
                }

                newDirection = gameEngine.StaticVariables.g_directionFlipTable[entity.TargetDirection];
                entity.ForceStepY = 0;
                entity.ForceStepX = 0;
                entity.ForceY = 0;
                entity.ForceX = 0;
                entity.TargetForceY = 0;
                entity.TargetForceX = 0;
                entity.TargetDirection = newDirection;
                state = (short)(((Random.Next() * 0x1f) >> 0x20) + 0x1e);
                goto LAB_8006c0e8;

            case 2:
                if (3 < positions[0])
                {
                    return;
                }

                if (3 < positions[1])
                {
                    return;
                }

                entity.TargetAnimationId = 4;
                return;

            case 3:
                state = entity.AIValues[1];
                entity.AIValues[1] = (short)(state - 1);

                if (state == 0)
                {
                    entity2 = gameEngine.SpawnWarpEntity(
                        entity,
                        1,
                        0x4a,
                        entity.PosX + gameEngine.StaticVariables.INT_ARRAY_80027440[entity.AnimationDirection * 2],
                        entity.PosY + gameEngine.StaticVariables.INT_ARRAY_80027440[entity.AnimationDirection * 2 + 1],
                        entity.PosZ + 0xa0000,
                        gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection]);

                    entity2.ForceZ = 0x20000;
                }

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x28, 0x1e);
                break;

            case 4:
                state = 0x1e;
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                goto LAB_8006c0e4;

            case 7:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[3] != 0)
                {
                    entity.TargetAnimationId = 6;
                    entity.Flags |= 0x40;
                    return;
                }
                state = 0x28;

            LAB_8006c0e4:
                entity.TargetAnimationId = 0;

            LAB_8006c0e8:
                entity.AIValues[1] = state;
                return;

            default:
                return;
        }

        entity.Bytes[1] = 0;
    }

    //8006c100
    public static void AI_UpdateEntityAI_18(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006c5cc
    public static void AI_UpdateEntityAI_19(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006ca40
    public static void AI_UpdateEntityAI_20(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006d550
    public static void AI_UpdateEntityAI_20_2(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006d998
    public static void AI_UpdateEntityAI_21(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006de68
    //muruta arc nv1
    public static void AI_UpdateEntityAI_22(GameEngine gameEngine, Entity entity)
    {
        short sVar2;
        int iVar4;
        Entity entitySpawned;
        uint direction;
        int[] positions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, positions);
        direction = entity.TargetAnimationId;

        if (direction == 2)
        {
            sVar2 = entity.AIValues[1];

            if (sVar2 != 0)
            {
                entity.AIValues[1] = (short)(sVar2 - 1);

                if (sVar2 == 1)
                {
                    entitySpawned = gameEngine.SpawnWarpEntity(
                        entity, 1, 0x5f,
                        entity.PosX + gameEngine.StaticVariables.SHORT_ARRAY_80027604[entity.AnimationDirection * 4],
                        entity.PosY + gameEngine.StaticVariables.SHORT_ARRAY_80027604[entity.AnimationDirection * 4 + 2],
                        entity.PosZ + 0x100000, entity.TargetDirection);

                    if (entitySpawned != null)
                    {
                        entitySpawned.AnimationDirection = entity.AnimationDirection;
                    }
                }
            }

            if (entity.ForceResetAnimationFlag == 0)
            {
                return;
            }

            if (6 < positions[0] || 6 < positions[1] || 0x200000 < positions[2])
            {
                sVar2 = 10;
                entity.TargetAnimationId = 0;
                //goto LAB_8006e2bc;
                entity.AIValues[1] = sVar2;
                entity.Bytes[0] = 0;
                return;
            }

            entity.CurrentAnimationId = 0xffffffff;
        }
        else
        {
            if ((int)direction < 3)
            {
                if (direction != 0)
                {
                    return;
                }

                if (entity.AIValues[1] != 0)
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                    return;
                }

                entity.TargetAnimationId = 6;
                entity.TargetDirection = gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection];
                return;
            }
            if (direction == 4)
            {
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[3] != 0)
                {
                    entity.TargetAnimationId = 5;
                    entity.Flags |= 0x40;
                    return;
                }

                entity.TargetAnimationId = 2;
                direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                entity.TargetDirection = direction;
                sVar2 = 0x21;
            LAB_8006e2bc:
                entity.AIValues[1] = sVar2;
                entity.Bytes[0] = 0;
                return;
            }

            if (direction != 6)
            {
                return;
            }

            if (entity.Bytes[0] == 0)
            {
                if (6 < positions[0] || 6 < positions[1] || 0x200000 < positions[2])
                {
                    if (entity.ForceAdjusted == 0 && CanMoveForward(entity, 0) == 0)
                    {
                        return;
                    }

                    entity.Bytes[2] = (byte)(entity.Bytes[2] + 1);
                    entity.TargetAnimationId = 0;
                    direction = (uint)(((Random.Next() * 0x20) >> 0x20) + 0x3c);
                    entity.AIValues[1] = (short)direction;

                    if (entity.Bytes[2] < 3)
                    {
                        direction = (uint)(gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection] - 0x10);
                    }
                    else
                    {
                        entity.Bytes[2] = 0;

                        if ((direction & 1) == 0)
                        {
                            direction = (uint)(gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection] - 8);
                        }
                        else
                        {
                            direction = (uint)(gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection] + 8);
                        }
                    }

                    entity.TargetDirection = direction & 0x1f;
                    return;
                }

                if (positions[0] < 3 && positions[1] < 3)
                {
                    entity.Bytes[0] = 1;
                    direction = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX, entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
                    entity.TargetDirection = direction;
                    entity.AIValues[1] = 0x1e;
                    return;
                }

                entity.TargetAnimationId = 2;
            }
            else
            {
                if (entity.Bytes[0] != 1)
                {
                    return;
                }

                sVar2 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar2;

                if (sVar2 != 0 && entity.ForceAdjusted == 0 && CanMoveForward(entity, 0) == 0)
                {
                    return;
                }

                if ((int)((Random.Next() * 5) >> 0x20) == 0)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0xcd);
                }
                entity.TargetAnimationId = 2;
            }
        }
        direction = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
        entity.TargetDirection = direction;
        entity.AIValues[1] = 0x21;
    }

    //8006e2d8
    public static void AI_UpdateEntityAI_23(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006e89c
    public static void AI_UpdateEntityAI_23_2(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8006fc7c
    public static void AI_UpdateEntityAI_Boss(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80070598
    //element nv 1
    public static void AI_UpdateBossEntityState(GameEngine gameEngine, Entity entity)
    {
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
                AI_Melzas2.UpdateEntityAI_ExecuteBossSpecialMove(gameEngine, entity);
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
                    do
                    {
                        i = direction + 4;
                        gameEngine.SpawnWarpEntity(entity, 1, 0xf3, entity.PosX, entity.PosY, entity.PosZ, direction);
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
                if (entity.IsAboveGround != 0)
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
                gameEngine.SpawnWarpEntity(entity, 1, 0xf3, entity.PosX, entity.PosY, entity.PosZ, entity.TargetDirection);
                break;
        }
    }

    //80070c40
    // Surveillance élémentaire
    public static void AI_UpdateBossEntityState2(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "Surveillance élémentaire")
        {
            Debugger.Break();
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
                    if (entity.ForceAdjusted == 0 && CanMoveForward(entity, 0) == 0)
                    {
                        return;
                    }

                    entity.TargetDirection = gameEngine.StaticVariables.g_directionFlipTable[entity.TargetDirection];
                    return;
                }

                gameEngine.SpawnWarpEntity(entity, 1, 0xf2, entity.PosX, entity.PosY, entity.PosZ + 0xa00000, entity.TargetDirection);
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
                    && CanMoveForward(entity, 0) == 0)
                {
                    aiState = (byte)(entity.Bytes[2] - 1);
                    entity.Bytes[2] = aiState;

                    if (aiState != 0)
                    {
                        return;
                    }

                    gameEngine.SpawnWarpEntity(entity, 1, 0xf2, entity.PosX, entity.PosY, entity.PosZ + 0xa00000, entity.TargetDirection);
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
                            gameEngine.SpawnWarpEntity(entity, 1, 0xf2, entity.PosX, entity.PosY, entity.PosZ + 0xa00000, entity.TargetDirection);
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
                    gameEngine.SpawnWarpEntity(entity, 1, 0xf2, entity.PosX, entity.PosY, entity.PosZ + 0xa00000, entity.TargetDirection);
                    return;
                }

                LAB_800710f0:
                if (entity.ForceAdjusted == 0 && CanMoveForward(entity, 0) == 0)
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

                gameEngine.SpawnWarpEntity(entity, 1, 0xf2, entity.PosX, entity.PosY, entity.PosZ + 0xa00000, entity.TargetDirection);
                aiState = (byte)(entity.Bytes[2] - 1);
                entity.Bytes[2] = aiState;

                if (aiState != 0)
                {
                    return;
                }
                break;

            default:
                goto switchD_80070fac_caseD_13;
        }

        switchD_80070fac_caseD_12:
        gameEngine.DestroyEntity(entity);
        parentEntity.Bytes[2] = 0;

        switchD_80070fac_caseD_13:
        return;
    }

    //80071164
    public static void AI_UpdateEntityAI_BoosPhase3(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80071bf4
    public static void AI_UpdateEntityAI_SpecialBoss(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007252c
    public static void AI_UpdateEntityIA_WatcherBehavior(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80072680
    public static void AI_UpdateEntityDelayedSoundTrigger(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80074d00
    //Monsieur Aspiration
    public static void AI_UpdateEntityAI_WarpBoss(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //80076da0
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

    //80071134
    //Roche élémentaire
    public static void AI_ApplyZGravityIfIdle(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name == "Roche élémentaire")
        {
            Debugger.Break();
        }

        if (entity.TargetAnimationId == 0 && -0x80001 < entity.ForceZ)
        {
            entity.ForceZ -= 0x4000;
        }
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
        //if (entity.Name == "Roche élémentaire")
        {
            Debugger.Break();
        }

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

            gameEngine.StaticVariables.PlayerEntity.PosX = entity.PosX;
            gameEngine.StaticVariables.PlayerEntity.PosY = entity.PosY;
            gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = 0;
            gameEngine.StaticVariables.g_playerControlFlags = (uint)(gameEngine.StaticVariables.g_playerControlFlags & ~0x20);

            warpSlot.Phase = 0;
            return;
        }

        if (warpSlot.Phase == 0x2)
        {
            Entity player = gameEngine.StaticVariables.PlayerEntity;

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
                    int sinTerm = (s * 9) << 9;
                    entity.PosX = warpSlot.BaseX + sinTerm;

                    short c = gameEngine.StaticVariables.g_cosinus[entity.DelayOrAngle];
                    int cosTerm = (c * 9) << 9;
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
                                warpSlot.PlayerX = gameEngine.StaticVariables.PlayerEntity.PosX;
                                warpSlot.PlayerY = gameEngine.StaticVariables.PlayerEntity.PosY;
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
                        warpSlot.BaseY = entity.PosY - warpSlot.SavedY + (int)0x2A000000u + warpSlot.SavedY;
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

                        var diffX = gameEngine.StaticVariables.PlayerEntity.ModdedPosX - entity.ModdedPosX;

                        if (diffX < 0)
                        {
                            if (entity.ModdedPosX - gameEngine.StaticVariables.PlayerEntity.ModdedPosX < gameEngine.StaticVariables.PlayerEntity.Width + 1)
                            {
                                //goto LAB_800625d0;
                                gotoLAB_800625d0 = true;
                            }
                        }

                        if ((diffX < 0 && diffX < entity.Width + 1) || gotoLAB_800625d0)
                        {
                        LAB_800625d0:
                            diffX = gameEngine.StaticVariables.PlayerEntity.ModdedPosY - entity.ModdedPosY;

                            if (diffX < 0)
                            {
                                if (entity.ModdedPosY - gameEngine.StaticVariables.PlayerEntity.ModdedPosY < gameEngine.StaticVariables.PlayerEntity.Height + 1)
                                {
                                    //goto LAB_80062624;
                                    gotoLAB_80062624 = true;
                                }
                            }

                            if ((diffX < 0 && diffX < entity.Height + 1) || gotoLAB_80062624)
                            {
                            LAB_80062624:
                                diffX = gameEngine.StaticVariables.PlayerEntity.ModdedPosZ - entity.ModdedPosZ;

                                if (diffX < 0)
                                {
                                    if (entity.ModdedPosZ - gameEngine.StaticVariables.PlayerEntity.ModdedPosZ < gameEngine.StaticVariables.PlayerEntity.Depth + 1)
                                    {
                                        //goto LAB_80062678;
                                        gotoLAB_80062678 = true;
                                    }
                                }

                                if ((diffX < 0 && diffX < entity.Depth + 1) || gotoLAB_80062678)
                                {
                                LAB_80062678:
                                    if ((gameEngine.StaticVariables.PlayerEntity.AnimFlags & 0x40U) == 0
                                        && gameEngine.StaticVariables.PlayerEntity.DamagedTickCounter == 0
                                        && gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[index ^ 1].Phase == 0)
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
                                        gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = 0x56;
                                        gameEngine.StaticVariables.g_playerControlFlags |= 0x20;
                                        gameEngine.StaticVariables.PlayerEntity.Flags &= 0xfffffef7;
                                        gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[index].Phase = 1;
                                        break;
                                    }
                                }
                            }
                        }

                        if (entity.CollidedWithEntityZ != 0 || entity.PosZ < 0xa00001)
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

                        gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = 0x31;
                        gameEngine.StaticVariables.PlayerEntity.TargetDirection = 0;
                        gameEngine.StaticVariables.PlayerEntity.Flags |= 0x108u;
                        gameEngine.StaticVariables.PlayerEntity.PosX = entity.PosX;
                        gameEngine.StaticVariables.PlayerEntity.PosY = entity.PosY;
                    }

                    // dans tous les cas, case 4 finit par:
                    // g_entitySlots[0].PreviousAdjustedForceX = slot.A0; PreviousAdjustedForceY = slot.A1; g_entitySlots[0].ForceZ = parent.ForceZ
                    gameEngine.StaticVariables.PlayerEntity.PreviousAdjustedForceX = warpSlot.A0;
                    gameEngine.StaticVariables.PlayerEntity.PreviousAdjustedForceY = warpSlot.A1;
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
                            entity.TargetDirection = (uint)((entity.TargetDirection + ((Random.Next() * 0xd) >> 0x20) - 6) & 0x1f);
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
                rand = (short)(entity.AIValues[4] - 1);

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

                        entity.AIValues[1] = (short)(entity.AIValues[1] - 1);

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
                        entity.Flags &= 0xfffffeff;
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
                entity.Flags |= 0x100;
            }
        }
    }

    //8007c174
    //Item spawn
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
                entity.DelayOrAngle -= 1;
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

                if (gameEngine.IsDialogFinished())
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

            if (gameEngine.IsDialogFinished())
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
            if (entity.IsAboveGround != 0 && entity.AIValues.GetInt32(2) != 0)
            {
                int force = entity.AIValues.GetInt32(2);

                var value = (force * 0xc) >> 4;
                entity.AIValues.Set(value, 2);

                if (value <= gameEngine.CurrentMap.Info.Gravity << 8)
                {
                    entity.AIValues.Set(0, 2);
                    value = 0;
                }

                entity.ForceZ = value;
            }

            var delay = entity.DelayOrAngle - 1;

            if (0 < entity.DelayOrAngle)
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
                if (gameEngine.IsDialogFinished())
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

                uint flag = (uint)(entity.AIValues[0] | (entity.AIValues[1] >> 16));
                gameEngine.FUN_80032b28(flag); //AIValues[0]
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
                gameEngine.EntityManager.BlockEntitiesBy(entity);
                gameEngine.SoundManager.LoadBgm(0);
                gameEngine.CdManager.StartCdStreaming(0xb);

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

            if (gameEngine.IsDialogFinished())
            {
                return;
            }
        }

        gameEngine.EntityManager.UnblockEntitiesBy(entity2);

    LAB_8007c740:
        gameEngine.DestroyEntity(entity);
    }

}