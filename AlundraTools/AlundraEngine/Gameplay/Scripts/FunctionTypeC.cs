using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay.Scripts.Boss;
using System;
using System.Diagnostics;

namespace AlundraEngine.Gameplay.Scripts;

public static class FunctionTypeC
{
    // GHIDRA: SHORT_ARRAY_80027e54 @ 0x80027E54
    private static readonly short[] SHORT_ARRAY_80027e54 = [0x64, 0x05, 0x05, 0x05, 0x05, 0x05];

    // GHIDRA: INT_ARRAY_80027e6c @ 0x80027E6C
    private static readonly int[] INT_ARRAY_80027e6c =
    [
        unchecked((int)0xFFB80000), 0x00100000, 0x00100000,
        unchecked((int)0xFFD80000), 0x00100000, 0x00200000,
        unchecked((int)0xFFE80000), 0x00100000, 0x00280000,
        0x00000000, 0x00100000, 0x00300000,
        0x00180000, 0x00100000, 0x00380000,
        unchecked((int)0xFFD00000), 0x00100000, 0x00300000
    ];

    // GHIDRA: INT_ARRAY_80027eb4 @ 0x80027EB4
    private static readonly int[] INT_ARRAY_80027eb4 =
    [
        0x00180000, 0x00D00000,
        0x01080000, 0x00D00000,
        0x00180000, 0x01200000,
        0x01080000, 0x01200000
    ];

    // GHIDRA: INT_ARRAY_80027ed4 @ 0x80027ED4
    private static readonly int[] INT_ARRAY_80027ed4 =
    [
        0x00300000, 0x00F00000,
        0x00900000, 0x01200000,
        0x00300000, 0x01500000,
        0x00900000, 0x00F00000,
        0x00300000, 0x01200000,
        0x00900000, 0x01500000
    ];

    // 80065ED4
    public static void AI_UpdateEntityAI_IdleSkittish(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Beannoïde"
            && entity.Name != "◆Petit slime")
        {
            Breakpoint.TriggerBreak();
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
                if (entity.IsOnGround != 0)
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
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Homme-lézard (épée) Niv.1"
            && entity.Name != "Melzas2_FinalBoss")
        {
            Breakpoint.TriggerBreak();
            return;
        }

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
                if (entity.IsOnGround != 0)
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

                if (entity.IsOnGround == 0)
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
        if (!string.IsNullOrEmpty(entity.Name) 
            && entity.Name != "◆Tortue de roche Niv.1")
        {
            Breakpoint.TriggerBreak();
            return;
        }

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

                if (entity.DelayOrAngleOrEntityId != 0)
                {
                    entity.DelayOrAngleOrEntityId -= 1;
                }

                if (entity.AIValues[1] == 0)
                {
                    entity.TargetAnimationId = 6;
                    entity.AIValues[1] = (short)(((Random.Next() * 0x1f) >> 0x20) + 0xb4);
                }

                if (entity.DelayOrAngleOrEntityId != 0 || 2 < relPos[0] || 2 < relPos[1] || 0x100000 < relPos[2])
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
                        entity.DelayOrAngleOrEntityId = 0x3c;
                    }
                    else
                    {
                        entity.DelayOrAngleOrEntityId = 0;
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
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Tonneau générique"
            && entity.Name != "I18_Veste en cuir"
            && entity.Name != "I20_Veste en argent"
            && entity.Name != "I36_Herbe médicinale"
            && entity.Name != "I37_Potion L"
            && entity.Name != "I38_Extrait magique"
            && entity.Name != "I41_Potion S"
            && entity.Name != "I51_Anneau d’orc"
            && entity.Name != "I53_Bracelet d’armure d’acier"
            && entity.Name != "I55_Anneau de régénération"
            && entity.Name != "I83_Récipient de vie"
            && entity.Name != "Objet étoile (transportable)"
            && entity.Name != "Objet lune (transportable)"
            && entity.Name != "Objet eau (transportable)"
            && entity.Name != "Objet soleil (transportable)"
            && entity.Name != "Pierre générique"
            && entity.Name != "Bloc de glace transportable"
            && entity.Name != "Tronc (transportable), petite boule de fer"
            && !string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
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
                if (entity.IsOnGround != 0 || entity.HitCounter != 0)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x18);
                    entity.DelayOrAngleOrEntityId = 0;
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

                if (entity.IsOnGround == 0)
                {
                    if (entity.ForceZ > 0 && entity.DelayOrAngleOrEntityId == 0)
                    {
                        entity.DelayOrAngleOrEntityId = 1;
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
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Orc (masse) Niv.1"
            && entity.Name != "Melzas2_FinalBoss")
        {
            Breakpoint.TriggerBreak();
            return;
        }

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
        if (!string.IsNullOrEmpty(entity.Name) 
            && entity.Name != "Melzas2_FinalBoss")
        {
            Breakpoint.TriggerBreak();
            return;
        }

        byte bVar1;
        ushort uVar2;
        bool bVar3;
        Entity entity2;
        uint uVar4;
        int iVar5;
        int entityRecordId;

        if ((gameEngine.StaticVariables.g_temporaryFlags[0] & 1) == 0)
        {
            return;
        }

        if (entity.AIValues[4] == 0)
        {
            bVar1 = entity.Bytes[1];
            entity.DelayOrAngleOrEntityId = 100;
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
                    entity2.DelayOrAngleOrEntityId = 0x20000;
                }
                else
                {
                    entity2.DelayOrAngleOrEntityId = 0x28000;
                }
            }

            gameEngine.StaticVariables.g_temporaryFlags[0] &= 0xfffffeff;
        }
        entityRecordId = 0;
        entity.AIValues[1] = (short)(entity.AIValues[1] + 1);
        uVar2 = (ushort)entity.AIValues[1];
        iVar5 = entity.DelayOrAngleOrEntityId + 1;
        entity.DelayOrAngleOrEntityId = iVar5;
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
                            gameEngine.StaticVariables.g_temporaryFlags[0] |= 0x100;
                        }

                        bVar1 = entity.Bytes[1];
                        entity.AIValues[4] = 0;
                        entity.Bytes[1] = (byte)(bVar1 + 1);
                        gameEngine.StaticVariables.g_temporaryFlags[0] &= 0xfffffffe;
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

        entity.DelayOrAngleOrEntityId = 0;

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
            entity2.DelayOrAngleOrEntityId = gameEngine.StaticVariables.DAT_80191130 + 0x12000;
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
            entity2.DelayOrAngleOrEntityId = iVar5;
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
            entity2.DelayOrAngleOrEntityId = -entity2.DelayOrAngleOrEntityId;
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

                entity2.DelayOrAngleOrEntityId = 0x22000;

                if (entity2.TargetDirection == 8)
                {
                    entity2.PosX += 0xf00000;
                    entity2.DelayOrAngleOrEntityId = -entity2.DelayOrAngleOrEntityId;
                }
            }
            else
            {
                entity.ItemState += -1;
            }
        }
    }

    // GHIDRA: AI_FUN_800647b0 @ 0x800647B0
    public static void AI_FUN_800647b0(GameEngine gameEngine, Entity entity)
    {
        if (entity.ParentEntity.AIValues[4] == 0)
        {
            entity.Flags &= 0xFFFFFFFE;
        }

        if (entity.Bytes[0] != 0)
        {
            if (entity.ForceAdjusted != 0)
            {
                if (entity.SpriteTableIndex == 0x125 && entity.ParentEntity.AIValues[4] != 0)
                {
                    entity.TargetDirection = (entity.TargetDirection + 0x10U) & 0x1F;
                    entity.DelayOrAngleOrEntityId = -entity.DelayOrAngleOrEntityId;
                }
                else
                {
                    gameEngine.DestroyEntity(entity);
                }
            }

            if (entity.Bytes[0] != 0)
            {
                entity.PreviousAdjustedForceX = entity.DelayOrAngleOrEntityId;
                return;
            }
        }

        entity.Bytes[0] = 1;
        entity.PreviousAdjustedForceX = entity.DelayOrAngleOrEntityId;
    }

    //80064884
    public static void AI_FUN_80064884(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name) 
            && entity.Name != "Melzas2_FinalBoss")
        {
            Breakpoint.TriggerBreak();
            return;
        }

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

        if ((gameEngine.StaticVariables.g_temporaryFlags[0] & 1) == 0)
        {
            return;
        }

        if (entity.Bytes[2] == 0)
        {
            if ((gameEngine.StaticVariables.g_temporaryFlags[0] & 0x80) != 0)
            {
                entity.Bytes[1] = 0;
                gameEngine.StaticVariables.g_temporaryFlags[0] &= 0xffffff7f;
            }

            bVar2 = false;
            gameEngine.StaticVariables.DAT_8019113c = entity.DelayOrAngleOrEntityId;
            local_c = (int)((Random.Next() * 3) >> 0x20);
            gameEngine.StaticVariables.DAT_80191140 = local_c + 3;
            entity.AIValues[1] = 0x3c;
            entity.AIValues[4] = 0;
            iVar5 = (int)((Random.Next() * 5) >> 0x20) + 0x46;
            entity.ItemState = iVar5;
            bVar1 = entity.Bytes[1];
            entity.DelayOrAngleOrEntityId = (entity.DelayOrAngleOrEntityId + iVar5) & 0xf;

            if (3 < bVar1 && (int)((Random.Next() * 3) >> 0x20) == 0)
            {
                iVar5 = entity.DelayOrAngleOrEntityId;

                if (iVar5 == 1)
                {
                    entity.DelayOrAngleOrEntityId = 0;
                    entity.ItemState -= 1;
                    iVar5 = entity.DelayOrAngleOrEntityId;
                }

                if (iVar5 == 0xf)
                {
                    entity.DelayOrAngleOrEntityId = 0;
                    entity.ItemState += 1;
                }
            }

            if (entity.DelayOrAngleOrEntityId == 0)
            {
            LAB_80064ae0:
                if (entity.Bytes[1] < 2 && (int)((Random.Next() * 3) >> 0x20) == 0)
                {
                    uVar7 = (uint)((entity.DelayOrAngleOrEntityId + 1U) & 0xf);

                    if (uVar7 != 0)
                    {
                        if (((gameEngine.StaticVariables.INT_ARRAY_80026d90[uVar7] - 0x8000) & 0x8000) == 0)
                        {
                            flags = gameEngine.StaticVariables.g_saveData.GameFlags;
                        }
                        else
                        {
                            flags = gameEngine.StaticVariables.g_temporaryFlags;
                        }

                        var index = ((gameEngine.StaticVariables.INT_ARRAY_80026d90[uVar7] - 0x8000) >> 3) & 0xffc;
                        var mask = 1 << (int)((gameEngine.StaticVariables.INT_ARRAY_80026d90[uVar7] + 0x8000U) & 0x1f);

                        if ((flags[index] & mask) != 0)
                        {
                            bVar2 = true;
                            entity.DelayOrAngleOrEntityId = (int)uVar7;
                            entity.ItemState += 1;
                        }
                    }
                }
            }
            else
            {
                if (((gameEngine.StaticVariables.INT_ARRAY_80026d90[entity.DelayOrAngleOrEntityId] - 0x8000) & 0x8000) == 0)
                {
                    flags = gameEngine.StaticVariables.g_saveData.GameFlags;
                }
                else
                {
                    flags = gameEngine.StaticVariables.g_temporaryFlags;
                }

                var index = ((gameEngine.StaticVariables.INT_ARRAY_80026d90[entity.DelayOrAngleOrEntityId] - 0x8000) >> 3) & 0xffc;
                var mask = 1 << (int)((gameEngine.StaticVariables.INT_ARRAY_80026d90[entity.DelayOrAngleOrEntityId] + 0x8000U) & 0x1f);

                if ((flags[index] & mask) == 0)
                {
                    //goto LAB_80064ae0;
                    if (entity.Bytes[1] < 2 && (int)((Random.Next() * 3) >> 0x20) == 0)
                    {
                        uVar7 = (uint)((entity.DelayOrAngleOrEntityId + 1U) & 0xf);

                        if (uVar7 != 0)
                        {
                            if (((gameEngine.StaticVariables.INT_ARRAY_80026d90[uVar7] - 0x8000) & 0x8000) == 0)
                            {
                                flags = gameEngine.StaticVariables.g_saveData.GameFlags;
                            }
                            else
                            {
                                flags = gameEngine.StaticVariables.g_temporaryFlags;
                            }

                            index = ((gameEngine.StaticVariables.INT_ARRAY_80026d90[uVar7] - 0x8000) >> 3) & 0xffc;
                            mask = 1 << (int)((gameEngine.StaticVariables.INT_ARRAY_80026d90[uVar7] + 0x8000U) & 0x1f);

                            if ((flags[index] & mask) != 0)
                            {
                                bVar2 = true;
                                entity.DelayOrAngleOrEntityId = (int)uVar7;
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
                gameEngine.StaticVariables.g_temporaryFlags[0] |= 0x100;
            }
            else
            {
                gameEngine.StaticVariables.g_temporaryFlags[0] &= 0xfffffeff;
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

            if ((gameEngine.StaticVariables.g_temporaryFlags[0] & 0x100) != 0)
            {
                gameEngine.StaticVariables.g_temporaryFlags[0] &= 0xfffffffe;
                gameEngine.SoundManager.PlaySoundEffect(0x1c6);
                return;
            }

            gameEngine.StaticVariables.g_temporaryFlags[0] &= 0xfffffffe;
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

    // GHIDRA: AI_FUN_80064d90 @ 0x80064D90
    public static void AI_FUN_80064d90(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name) 
            && entity.Name != "Melzas2_FinalBoss")
        {
            Breakpoint.TriggerBreak();
            return;
        }

        const int DAT_80026e5c_Index = 0x06;
        const int UNK_80026e6c_Index = 0x0E;
        const int SHORT_80026f34_Index = 0x72;
        const int SHORT_80026f3c_Index = 0x76;
        const int SHORT_80026f60_Index = 0x88;
        const int SHORT_80026f84_Index = 0x9A;
        const int SHORT_80026fb4_Index = 0xB2;

        var rawTable = gameEngine.StaticVariables.SHORT_ARRAY_80026e50;

        if ((gameEngine.StaticVariables.g_temporaryFlags[0] & 1U) == 0)
        {
            return;
        }

        if (entity.AIValues[4] == 0)
        {
            if ((gameEngine.StaticVariables.g_temporaryFlags[0] & 0x80U) != 0)
            {
                entity.Bytes[1] = 0;
                gameEngine.StaticVariables.g_temporaryFlags[0] &= 0xFFFFFF7F;
            }

            var phase = entity.Bytes[1];
            entity.Bytes[2] = 0;
            entity.ItemState = 0;
            entity.AIValues[1] = 0;
            gameEngine.StaticVariables.DAT_80191144 = UNK_80026e6c_Index + (phase * 20);
            gameEngine.StaticVariables.g_temporaryFlags[0] &= 0xFFFFFEFF;
            entity.AIValues[4] = 1;
        }

        var aiCounter = (ushort)(entity.AIValues[1] + 1);
        entity.AIValues[1] = unchecked((short)aiCounter);

        if (aiCounter >= 0x708)
        {
            gameEngine.StaticVariables.INT_ARRAY_80191908[0] = entity.Bytes[2];

            if (rawTable[entity.Bytes[1]] <= entity.Bytes[2])
            {
                gameEngine.StaticVariables.g_temporaryFlags[0] |= 0x100U;
            }

            entity.AIValues[4] = 0;
            entity.Bytes[1] = (byte)(entity.Bytes[1] + 1);
            gameEngine.StaticVariables.g_temporaryFlags[0] &= 0xFFFFFFFE;
            return;
        }

        for (var entityIndex = 0;
             entityIndex <= gameEngine.StaticVariables.g_numberOfEntities && entityIndex < gameEngine.StaticVariables.g_entitySlots.Length;
             entityIndex++)
        {
            var entity2 = gameEngine.StaticVariables.g_entitySlots[entityIndex];

            if (unchecked((uint)(entity2.EntityRefId - 4)) < 9U)
            {
                return;
            }
        }

        var delay = entity.DelayOrAngleOrEntityId - 1;

        if (entity.DelayOrAngleOrEntityId == 0)
        {
            var sequenceValue = rawTable[gameEngine.StaticVariables.DAT_80191144 + entity.ItemState];
            entity.ItemState += 1;

            int randomVariantCount;
            int patternBaseIndex;

            switch (sequenceValue)
            {
                case 1:
                    randomVariantCount = 0;
                    patternBaseIndex = SHORT_80026f34_Index;
                    break;

                case 2:
                    randomVariantCount = 2;
                    patternBaseIndex = SHORT_80026f3c_Index;
                    break;

                case 3:
                    randomVariantCount = 1;
                    patternBaseIndex = SHORT_80026f60_Index;
                    break;

                case 4:
                    randomVariantCount = 1;
                    patternBaseIndex = SHORT_80026f84_Index;
                    break;

                case 6:
                    randomVariantCount = 1;
                    patternBaseIndex = SHORT_80026fb4_Index;
                    break;

                default:
                    entity.ItemState -= 1;
                    return;
            }

            var patternIndex = patternBaseIndex + (sequenceValue * RandomRange((uint)(randomVariantCount + 1)) * 3);

            for (var spawnIndex = 0; spawnIndex < sequenceValue; spawnIndex++)
            {
                var nextAiValue = rawTable[patternIndex];
                var entityRecordId = (ushort)rawTable[patternIndex + 1];

                if ((entityRecordId & 0x80U) != 0)
                {
                    var pairIndex = DAT_80026e5c_Index + ((entityRecordId & 0x0FU) * 2);
                    entityRecordId = (ushort)(rawTable[pairIndex + 1] + RandomRange((uint)(rawTable[pairIndex] + 1)));
                }

                var spawnedEntity = gameEngine.SpawnEntity(entity, entityRecordId, 1);
                spawnedEntity.AIValues[1] = nextAiValue;
                spawnedEntity.ContentsItemId = 0;
                spawnedEntity.AIValues[4] = rawTable[patternIndex + 2];
                patternIndex += 3;
            }

            delay = 0x14;
        }

        entity.DelayOrAngleOrEntityId = delay;
    }

    //80065100
    public static void AI_FUN_80065100(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "Melzas2_FinalBoss")
        {
            Breakpoint.TriggerBreak();
        }

        short sVar1;
        uint targetAnimationId;

        if ((gameEngine.StaticVariables.g_temporaryFlags[0] & 1) == 0 && entity.TargetAnimationId != 0x10)
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

    // GHIDRA: AI_FUN_80065204 @ 0x80065204
    public static void AI_FUN_80065204(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Melzas2_FinalBoss")
        {
            Breakpoint.TriggerBreak();
        }

        if (entity.Bytes[0] == 0)
        {
            entity.Bytes[0] = 1;

            foreach (var effect in gameEngine.StaticVariables.g_effectSlots)
            {
                if (effect.Status != 2)
                {
                    continue;
                }

                switch (effect.MapEffectId)
                {
                    case 0:
                        if (gameEngine.StaticVariables.PTR_ARRAY_80191160[0] == null)
                        {
                            gameEngine.StaticVariables.PTR_ARRAY_80191160[0] = effect;
                        }
                        break;

                    case 1:
                        gameEngine.StaticVariables.PTR_ARRAY_80191160[1] = effect;
                        break;

                    case 2:
                        gameEngine.StaticVariables.PTR_ARRAY_80191160[2] = effect;
                        break;

                    case 3:
                        gameEngine.StaticVariables.PTR_ARRAY_80191160[3] = effect;
                        break;
                }
            }

            var effectY = 0x00A80000;

            for (var effectIndex = 0; effectIndex < 4; effectIndex++)
            {
                var effect = gameEngine.EffectManager.SpawnSpriteEffect(4, 1);

                if (effect != null)
                {
                    gameEngine.StaticVariables.PTR_ARRAY_80191150[effectIndex] = effect;
                    effect.Y = effectY;
                    effectY += 0x00700000;
                    effect.X = 0x01E00000;
                    effect.Z = 0x00300001;
                }
            }

            gameEngine.StaticVariables.DAT_80191170 = 0;
            entity.Flags &= 0xFFFFFF7F;
        }

        if ((gameEngine.StaticVariables.g_temporaryFlags[0] & 1U) == 0)
        {
            return;
        }

        if (entity.Bytes[1] == 0)
        {
            if (entity.TargetAnimationId == 0)
            {
                entity.TargetAnimationId = 1;
                gameEngine.StaticVariables.DAT_80191170 = gameEngine.StaticVariables.g_cameraScrollingY;
                entity.TargetDirection = 0;
            }
            else
            {
                var deltaY = (gameEngine.StaticVariables.g_cameraScrollingY - gameEngine.StaticVariables.DAT_80191170) << 16;
                gameEngine.StaticVariables.DAT_80191170 = gameEngine.StaticVariables.g_cameraScrollingY;

                if (deltaY != 0)
                {
                    gameEngine.StaticVariables.g_temporaryFlags[0] |= 2U;
                }

                foreach (var effect in gameEngine.StaticVariables.g_effectSlots)
                {
                    var skipTrackedEffect = false;

                    if (effect.Status != 2)
                    {
                        continue;
                    }

                    for (var trackedIndex = 0; trackedIndex < gameEngine.StaticVariables.PTR_ARRAY_80191150.Length; trackedIndex++)
                    {
                        if (ReferenceEquals(effect, gameEngine.StaticVariables.PTR_ARRAY_80191150[trackedIndex]))
                        {
                            skipTrackedEffect = true;
                            break;
                        }
                    }

                    if (!skipTrackedEffect)
                    {
                        effect.Y += deltaY;
                    }
                }

                var aiCounter = (ushort)(entity.AIValues[1] + 1);
                entity.AIValues[1] = unchecked((short)aiCounter);

                if (aiCounter == 0x130)
                {
                    for (var effectIndex = 0; effectIndex < gameEngine.StaticVariables.PTR_ARRAY_80191150.Length; effectIndex++)
                    {
                        gameEngine.StaticVariables.PTR_ARRAY_80191150[effectIndex]!.TargetAnimation = 1;
                    }
                }

                if (aiCounter == 0x180)
                {
                    gameEngine.StaticVariables.g_temporaryFlags[0] |= 4U;
                    entity.TargetAnimationId = 0;
                    entity.Bytes[1] = 1;
                    entity.AIValues[1] = 0;
                }
            }
        }

        entity.DelayOrAngleOrEntityId += 1;

        if ((entity.DelayOrAngleOrEntityId & 7) != 0)
        {
            return;
        }

        var effectId = 5;

        if (entity.ItemState == 0)
        {
            effectId = 6;
            entity.ItemState = 6;
        }

        for (var sourceIndex = 0; sourceIndex < 2; sourceIndex++)
        {
            entity.ItemState -= 1;

            var leftSpawnedEffect = gameEngine.EffectManager.SpawnSpriteEffect(effectId, 1);

            if (leftSpawnedEffect != null)
            {
                var sourceEffect = gameEngine.StaticVariables.PTR_ARRAY_80191160[sourceIndex]!;
                leftSpawnedEffect.X = sourceEffect.X + (RandomRange(Random.Next(), 5U) << 19) + 0x00400000;
                leftSpawnedEffect.Y = sourceEffect.Y - (RandomRange(Random.Next(), 0x0FU) << 19) + 0x00A00000;
                leftSpawnedEffect.Z = sourceEffect.Z + 0x00A00000;

                if (effectId == 6)
                {
                    leftSpawnedEffect.ForceZ = 0x4000;
                    leftSpawnedEffect.ForceX = -0x4000;
                }
            }

            var rightSpawnedEffect = gameEngine.EffectManager.SpawnSpriteEffect(effectId, 1);

            if (rightSpawnedEffect != null)
            {
                var sourceEffect = gameEngine.StaticVariables.PTR_ARRAY_80191160[sourceIndex + 2]!;
                rightSpawnedEffect.X = sourceEffect.X + (RandomRange(Random.Next(), 5U) << 19) - 0x00580000;
                rightSpawnedEffect.Y = sourceEffect.Y - (RandomRange(Random.Next(), 0x0FU) << 19) + 0x00A00000;
                rightSpawnedEffect.Z = sourceEffect.Z + 0x00A00000;

                if (effectId == 6)
                {
                    rightSpawnedEffect.ForceZ = 0x4000;
                    rightSpawnedEffect.ForceX = 0x4000;
                }
            }
        }
    }

    //80065750
    public static void AI_FUN_80065750(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name) 
            && entity.Name != "Toutou (chien)")
        {
            Breakpoint.TriggerBreak();
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
        else if (uVar4 == 5 && entity.IsOnGround != 0)
        {
            entity.TargetAnimationId = 1;
        }
    }

    //80065b0c
    public static void AI_FUN_80065b0c(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Slime géant (grand)"
            && entity.Name != "◆Slime géant (petit)")
        {
            Breakpoint.TriggerBreak();
        }

        short sVar1;
        short uVar2;
        Entity entitySpawned;
        int iVar4;
        int iVar6;
        uint targetAnimationId;

        if (entity.Bytes[0] == 0)
        {
            entity.AIValues[1] = 0x3c;
            entity.Bytes[0] = 1;
        }

        if (entity.SpriteTableIndex == 0x1cb)
        {
            targetAnimationId = entity.TargetAnimationId;

            if (targetAnimationId == 1)
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

            if ((int)targetAnimationId < 2)
            {
                if (targetAnimationId != 0)
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

            if (targetAnimationId != 3)
            {
                if (targetAnimationId != 6)
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

                targetAnimationId = 4;

                do
                {
                    entitySpawned = gameEngine.SpawnEntity(entity, 0x24, 1);

                    if (entitySpawned != null)
                    {
                        entitySpawned.TargetAnimationId = 1;
                        entitySpawned.PosX = entity.PosX;
                        entitySpawned.PosY = entity.PosY;
                        iVar4 = entity.PosZ;
                        entitySpawned.TargetDirection = targetAnimationId;
                        entitySpawned.AIValues[1] = 0x3c;
                        entitySpawned.PosZ = iVar4;
                    }

                    iVar6 += 1;
                    targetAnimationId += 8;
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
            targetAnimationId = entity.TargetAnimationId;

            if (targetAnimationId == 1)
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

            if ((int)targetAnimationId < 2)
            {
                if (targetAnimationId != 0)
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

            if (targetAnimationId != 2)
            {
                if (targetAnimationId != 3)
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

                targetAnimationId = 4;

                do
                {
                    entitySpawned = gameEngine.SpawnEntity(entity, 0x25, 1);
                    if (entitySpawned != null)
                    {
                        entitySpawned.TargetAnimationId = 0x13;
                        entitySpawned.PosX = entity.PosX;
                        entitySpawned.PosY = entity.PosY;
                        iVar4 = entity.PosZ;
                        entitySpawned.TargetDirection = targetAnimationId;
                        entitySpawned.SpriteProgramIndexes[0] = 0;
                        entitySpawned.AIValues[1] = 0x1e;
                        entitySpawned.PosZ = iVar4;
                    }
                    iVar6 += 1;
                    targetAnimationId += 8;
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

    // GHIDRA: AI_FUN_8006b510 @ 0x8006B510
    public static void AI_FUN_8006b510(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Mouche Niv.1"
            && entity.Name != "◆Mille-pattes (œufs explosifs)"
            && entity.Name != "Melzas2_FinalBoss")
        {
            Breakpoint.TriggerBreak();
        }

        ushort uVar1;
        byte bVar2;
        uint uVar3;
        int[] positions = new int[6];

        FUN_8007fe8c(gameEngine, entity, positions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                uVar1 = (ushort)entity.AIValues[1];

                if (uVar1 != 0)
                {
                    entity.AIValues[1] = unchecked((short)(uVar1 - 1));
                }

                if (uVar1 < 2)
                {

                    if (entity.Bytes[0] == 0)
                    {
                        entity.Bytes[0] = 10;
                    }

                    if (entity.Bytes[1] == 1)
                    {
                        uVar3 = 2;
                        if (entity.IsOnGround == 0)
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
                    || (entity.IsOnGround != 0 && entity.TargetAnimationId == 3)
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
                            entity.ParentEntity.AIValues[4] = unchecked((short)(((ushort)entity.ParentEntity.AIValues[4]) - 1));
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
                        entity.ParentEntity.AIValues[4] = unchecked((short)(((ushort)entity.ParentEntity.AIValues[4]) - 1));
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
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Caisse en bois générique" 
            && entity.Name != "Cruche générique"
            && entity.Name != "Pierre très lourde"
            && entity.Name != "Cruche n°2"
            && entity.Name != "Herbe transportable"
            && entity.Name != "Bloc de glace transportable"
            && entity.Name != "Rocher tortue non ennemi")
        {
            Breakpoint.TriggerBreak();
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
    public static void AI_FUN_8006b8cc(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Projectile"
            && entity.Name != "◆Projectile Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        //do nothing
    }

    //8006ce08
    public static void AI_FUN_8006ce08(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "Melzas2_FinalBoss")
        {
            Breakpoint.TriggerBreak();
        }

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
                iVar4 = entity.DelayOrAngleOrEntityId - 1;

                if (iVar4 >= 0)
                {
                    entity.DelayOrAngleOrEntityId = iVar4;
                }

                if ((entity.DelayOrAngleOrEntityId == 0 || iVar4 == 0)
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
                        if (entity.IsOnGround != 0)
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
                        entity.DelayOrAngleOrEntityId = 0x32;
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
                            if (entity.IsOnGround == 0)
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

    // GHIDRA: SHORT_ARRAY_80027644 @ 0x80027644
    private static readonly short[] SHORT_ARRAY_80027644 = new short[]
    {
        0x0C, 0x14, 0x0C, 0x14,
        0x0C, 0x14, 0x0C, 0x14,
        0x14, 0x1C, 0x14, 0x1C,
        0x04, 0x0C, 0x14, 0x1C,
        0x04, 0x0C, 0x04, 0x0C,
        0x04, 0x0C, 0x14, 0x1C,
        0x04, 0x0C, 0x14, 0x1C,
        0x14, 0x1C, 0x14, 0x1C,
        0x04, 0x0C, 0x14, 0x1C,
        0x04, 0x0C, 0x04, 0x0C,
        0x04, 0x1C, 0x04, 0x1C,
        0x04, 0x1C, 0x04, 0x1C,
    };

    //8006e83c
    public static void AI_FUN_8006e83c(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Abyss Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

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

    // GHIDRA: AI_FUN_8006eb9c @ 0x8006EB9C
    public static void AI_FUN_8006eb9c(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Abyss Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        if (entity.Bytes[3] != 0 && entity.TargetAnimationId == 0)
        {
            if (entity.Bytes[3] < 4)
            {
                AI_Melzas2.UpdateEntityAI_BossExplode(gameEngine, entity);
                return;
            }

            entity.TargetAnimationId = 7;
            entity.Flags |= 0x40;
            return;
        }

        switch (entity.TargetAnimationId)
        {
            case 0:
                switch (entity.Bytes[2])
                {
                    case 3:
                    {
                        ushort delay = (ushort)entity.AIValues[1];
                        if (delay != 0)
                        {
                            delay--;
                            entity.AIValues[1] = unchecked((short)delay);
                            if (delay != 0)
                            {
                                return;
                            }
                        }

                        entity.AIValues[1] = 0xB4;
                        var occupiedSlots = gameEngine.StaticVariables.DAT_80191178;
                        Array.Clear(occupiedSlots);

                        for (var spawnIndex = 0; spawnIndex != 12; spawnIndex++)
                        {
                            var column = (int)((Random.Next() * 6) >> 0x20);
                            var row = (int)((Random.Next() * 4) >> 0x20);
                            var occupiedIndex = row * 6 + column;

                            while (occupiedSlots[occupiedIndex] != 0)
                            {
                                column += 2;

                                if (column >= 6)
                                {
                                    column -= 6;
                                    row += 1;

                                    if (row >= 6)
                                    {
                                        row = 0;
                                    }
                                }

                                occupiedIndex = row * 6 + column;
                            }

                            occupiedSlots[occupiedIndex] = 1;

                            var posX = ((row * 5) << 19) + 0x01800000;
                            var posY = ((column * 3) << 20) + 0x01500000;
                            var spawned = gameEngine.SpawnWarpEntity(entity, 1, (uint)(0xE6 | (spawnIndex & 1)), posX, posY, entity.PosZ, 0);

                            if (spawned != null)
                            {
                                Random.Next();
                                spawned.TargetAnimationId = 8;
                                spawned.Bytes[2] = 3;
                                spawned.AIValues[1] = (short)(spawnIndex * 3 + 0x0A);
                            }
                        }

                        return;
                    }

                    case 4:
                    {
                        ushort delay = (ushort)entity.AIValues[1];
                        if (delay != 0)
                        {
                            delay--;
                            entity.AIValues[1] = unchecked((short)delay);
                            return;
                        }

                        entity.AIValues[1] = 0xC8;
                        var delayOrAngle = 0x0A;

                        for (var spawnIndex = 0; spawnIndex != 4; spawnIndex++)
                        {
                            var direction = ((int)((Random.Next() * 4) >> 0x20) << 3) + 4;
                            var phase = ((0x20 - direction) & 0x1F) << 3;
                            var posY = entity.PosY + ((gameEngine.StaticVariables.g_sinTable[phase] * 3) << 11);
                            var posX = entity.PosX + ((gameEngine.StaticVariables.g_cosTable[phase] * 3) << 11);

                            if ((uint)(posY - 0x01380000) <= 0x01200000 && 0x017FFFFF < posX && posX <= 0x0250FFFF)
                            {
                                var spawned = gameEngine.SpawnWarpEntity(entity, 1, (uint)(0xE6 | (spawnIndex & 1)), posX, posY, entity.PosZ, (uint)direction);

                                if (spawned != null)
                                {
                                    spawned.TargetAnimationId = 4;
                                    spawned.DelayOrAngleOrEntityId = delayOrAngle;
                                    spawned.Bytes[2] = 4;
                                    spawned.AIValues[1] = (short)(((Random.Next() * 0x15) >> 0x20) + 0x1E);

                                    if ((spawned.PosZ >> 16) != (entity.PosZ >> 16))
                                    {
                                        gameEngine.DestroyEntity(spawned);
                                    }
                                }
                            }

                            delayOrAngle += 0x1E;
                        }

                        return;
                    }

                    case 5:
                    {
                        var delay = (ushort)(entity.AIValues[1] - 1);
                        entity.AIValues[1] = unchecked((short)delay);

                        if (delay == 0)
                        {
                            entity.TargetAnimationId = 3;
                        }

                        return;
                    }
                }

                return;

            case 2:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0;
                }

                return;

            case 3:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 8;
                    entity.Bytes[2] = 0;
                    entity.AIValues[1] = 0x3C;
                }

                return;

            case 4:
            {
                ushort delay = (ushort)entity.AIValues[1];
                if (delay != 0)
                {
                    delay--;
                    entity.AIValues[1] = unchecked((short)delay);
                }

                if ((delay & 7) == 0)
                {
                    var spawned = gameEngine.SpawnWarpEntity(entity, 1, 0xE4, entity.PosX - 0x10000, entity.PosY, entity.PosZ, 0);

                    if (spawned != null)
                    {
                        spawned.TargetAnimationId = 2;
                        spawned.Flags = (spawned.Flags & 0xFFFFFFFCU) | 0x40;
                    }
                }

                if ((delay & 0x3F) == 0)
                {
                    var player = gameEngine.StaticVariables.PlayerEntity;
                    var posY = player.PosY + (((int)((Random.Next() * 0x0D) >> 0x20)) << 19) - 0x300000;
                    if (posY <= 0x0137FFFF)
                    {
                        posY = 0x012C0000;
                    }
                    else if (posY > 0x02580000)
                    {
                        posY = 0x02640000;
                    }

                    var posX = player.PosX + (((int)((Random.Next() * 0x11) >> 0x20)) << 18) - 0x200000;
                    if (posX <= 0x017FFFFF)
                    {
                        posX = 0x01780000;
                    }
                    else if (posX > 0x0250FFFF)
                    {
                        posX = 0x02580000;
                    }

                    var spawned = gameEngine.SpawnWarpEntity(entity, 1, 0xE6, posX, posY, player.PosZ, 0);
                    if (spawned != null)
                    {
                        spawned.TargetAnimationId = 2;
                        spawned.Bytes[2] = 1;
                    }
                }

                if (delay == 0)
                {
                    entity.TargetAnimationId = 8;
                    entity.AIValues[1] = 0x28;
                    entity.TargetDirection = (entity.TargetDirection + 0x10U) & 0x1FU;
                }

                return;
            }

            case 6:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[3] != 0)
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0x12C;
                    entity.AIValues[5] = 0x1E;
                    entity.Flags &= 0xFFFFFFFCU;
                    return;
                }

                entity.TargetAnimationId = 3;
                entity.Bytes[2] = 0;
                entity.AIValues[1] = 0x3C;
                return;

            case 8:
            {
                ushort delay = (ushort)entity.AIValues[1];
                if (delay != 0)
                {
                    delay--;
                    entity.AIValues[1] = unchecked((short)delay);
                    if (delay != 0)
                    {
                        return;
                    }
                }

                switch (entity.Bytes[2])
                {
                    case 0:
                        entity.Bytes[2] = 1;
                        entity.AIValues[1] = 0x3C;
                        entity.Bytes[1] = (byte)(((Random.Next() * 3) >> 0x20) + 1);
                        return;

                    case 1:
                        if (entity.Bytes[1] != 0)
                        {
                            entity.Bytes[1] = (byte)(entity.Bytes[1] - 1);
                            entity.Bytes[2] = 1;
                            entity.TargetAnimationId = 4;
                            entity.AIValues[1] = 0x40;

                            var directionIndex = (int)((Random.Next() * 4) >> 0x20);
                            var baseIndex = entity.Bytes[0] << 2;
                            short targetDirection;

                            do
                            {
                                targetDirection = SHORT_ARRAY_80027644[baseIndex + directionIndex];
                                directionIndex = (directionIndex + 1) & 3;
                            } while ((uint)targetDirection == entity.TargetDirection);

                            entity.TargetDirection = (uint)targetDirection;

                            if (targetDirection == 4)
                            {
                                entity.Bytes[0] = (byte)(entity.Bytes[0] - 3);
                            }
                            else if (targetDirection == 0x0C)
                            {
                                entity.Bytes[0] = (byte)(entity.Bytes[0] + 2);
                            }
                            else if (targetDirection == 0x14)
                            {
                                entity.Bytes[0] = (byte)(entity.Bytes[0] + 3);
                            }
                            else
                            {
                                entity.Bytes[0] = (byte)(entity.Bytes[0] - 2);
                            }

                            return;
                        }

                        if ((entity.Bytes[0] == 3 || entity.Bytes[0] == 5 || entity.Bytes[0] == 6 || entity.Bytes[0] == 8)
                            && (int)((Random.Next() * 3) >> 0x20) == 0)
                        {
                            entity.Bytes[2] = 5;
                            return;
                        }

                        switch ((int)((Random.Next() * 8) >> 0x20))
                        {
                            case 0:
                            case 2:
                            case 4:
                                entity.Bytes[2] = 2;
                                entity.AIValues[4] = 8;
                                entity.DelayOrAngleOrEntityId = (int)((Random.Next() * 0x20) >> 0x20);
                                return;

                            case 6:
                            case 7:
                                entity.Bytes[2] = 3;
                                return;

                            default:
                                entity.Bytes[2] = 4;
                                return;
                        }

                    case 2:
                        if (entity.AIValues[4] == 0)
                        {
                            entity.Bytes[2] = 0;
                            return;
                        }

                        entity.AIValues[4] = (short)(entity.AIValues[4] - 1);
                        entity.AIValues[1] = 0x0C;
                        entity.DelayOrAngleOrEntityId = (entity.DelayOrAngleOrEntityId + 2) & 0x1F;
                        gameEngine.SoundManager.PlaySoundEffect(0xBE);

                        for (var spawnIndex = 0; spawnIndex != 4; spawnIndex++)
                        {
                            var direction = (entity.DelayOrAngleOrEntityId + (spawnIndex << 3)) & 0x1F;
                            var phase = ((0x20 - direction) & 0x1F) << 3;
                            var posY = entity.PosY + ((gameEngine.StaticVariables.g_sinTable[phase] * 3) << 11);
                            var posX = entity.PosX + ((gameEngine.StaticVariables.g_cosTable[phase] * 3) << 11);
                            var spawned = gameEngine.SpawnWarpEntity(entity, 1, 0xE4, posX, posY, entity.PosZ, (uint)direction);

                            if (spawned != null)
                            {
                                spawned.TargetAnimationId = 3;
                                spawned.AIValues[1] = (short)(spawnIndex * 3);
                            }
                        }

                        return;

                    case 3:
                        entity.TargetAnimationId = 2;
                        entity.AIValues[1] = 0x14;
                        return;

                    case 4:
                        entity.TargetAnimationId = 2;
                        return;

                    case 5:
                    {
                        entity.TargetAnimationId = 2;
                        entity.AIValues[1] = 0x78;
                        var direction = 0;

                        for (var spawnIndex = 0; spawnIndex != 8; spawnIndex++)
                        {
                            var phase = ((0x20 - direction) & 0x1F) << 3;
                            var posY = entity.PosY + (gameEngine.StaticVariables.g_sinTable[phase] << 12);
                            var posX = entity.PosX + (gameEngine.StaticVariables.g_cosTable[phase] << 12);
                            var spawned = gameEngine.SpawnWarpEntity(entity, 1, (uint)(0xE6 | (spawnIndex & 1)), posX, posY, entity.PosZ, (uint)direction);

                            if (spawned != null)
                            {
                                spawned.TargetAnimationId = 2;
                                spawned.Bytes[2] = 5;
                                spawned.AIValues[1] = 0x5A;
                            }

                            direction = (direction - 4) & 0x1F;
                        }

                        return;
                    }
                }

                return;
            }
        }
    }

    //8006f860
    public static void AI_FUN_8006f860(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Abyss (projectiles-bulles)")
        {
            Breakpoint.TriggerBreak();
        }

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

    // GHIDRA: AI_FUN_8006f8e4 @ 0x8006F8E4
    public static void AI_FUN_8006f8e4(GameEngine gameEngine, Entity entity)
    {
        var parentEntity = entity.ParentEntity;

        if (parentEntity.Bytes[3] != 0)
        {
            if (entity.TargetAnimationId != 8 && entity.TargetAnimationId != 4)
            {
                entity.TargetAnimationId = 7;
                entity.Flags |= 0x40;
                return;
            }

            gameEngine.DestroyEntity(entity);
            return;
        }

        var relativePositions = new int[6];
        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

        switch (entity.TargetAnimationId)
        {
            case 1:
            {
                entity.DelayOrAngleOrEntityId += 1;

                if ((entity.DelayOrAngleOrEntityId & 7) == 0)
                {
                    var spawned = gameEngine.SpawnWarpEntity(entity, 1, 0xE4, entity.PosX, entity.PosY - 0x10000, entity.PosZ, 0);

                    if (spawned != null)
                    {
                        spawned.TargetAnimationId = 2;
                        spawned.Flags = (spawned.Flags & 0xFFFFFFFCU) | 0x40;
                    }
                }

                if (entity.Bytes[2] == 4)
                {
                    if (parentEntity.TargetAnimationId == 3)
                    {
                        entity.TargetAnimationId = 10;
                        return;
                    }

                    ushort delay = (ushort)entity.AIValues[1];
                    if (delay != 0)
                    {
                        delay--;
                        entity.AIValues[1] = unchecked((short)delay);
                        if (delay != 0)
                        {
                            return;
                        }
                    }

                    entity.AIValues[1] = 8;
                    entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.Bytes[0] = (byte)(entity.Bytes[0] + 1);
                    return;
                }

                if (entity.Bytes[2] != 5)
                {
                    return;
                }

                short delay2 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = delay2;

                if (delay2 != 0 && entity.ForceAdjusted == 0)
                {
                    byte state = (byte)(entity.Bytes[0] - 1);
                    entity.Bytes[0] = state;
                    if (state != 0)
                    {
                        return;
                    }

                    entity.Bytes[0] = 6;
                    entity.TargetDirection = (entity.TargetDirection + 1) & 0x1F;
                    return;
                }

                entity.TargetAnimationId = 10;
                return;
            }

            case 2:
            {
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                byte byte2 = entity.Bytes[2];
                if (byte2 == 3 || byte2 == 1)
                {
                    entity.TargetAnimationId = 3;
                    byte2 = entity.Bytes[2];
                }

                if (byte2 == 5)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x6E);
                    entity.TargetAnimationId = 1;
                    entity.Bytes[0] = 6;
                }

                if (entity.Bytes[2] == 4)
                {
                    entity.TargetAnimationId = 1;
                    entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                }

                return;
            }

            case 3:
            case 10:
            {
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 9;
                }

                return;
            }

            case 4:
            {
                if (parentEntity.Bytes[2] != entity.Bytes[2])
                {
                    gameEngine.DestroyEntity(entity);
                    return;
                }

                if (parentEntity.Bytes[2] != 4)
                {
                    return;
                }

                ushort delay = (ushort)entity.AIValues[1];
                if (delay != 0)
                {
                    delay--;
                    entity.AIValues[1] = unchecked((short)delay);
                    if (delay != 0 && entity.ForceAdjusted == 0)
                    {
                        return;
                    }
                }

                entity.TargetAnimationId = 8;
                entity.AIValues[1] = (short)entity.DelayOrAngleOrEntityId;
                return;
            }

            case 8:
            {
                if (parentEntity.Bytes[2] != entity.Bytes[2])
                {
                    gameEngine.DestroyEntity(entity);
                    return;
                }

                ushort delay = (ushort)entity.AIValues[1];
                delay--;
                entity.AIValues[1] = unchecked((short)delay);
                if (delay == 0)
                {
                    entity.TargetAnimationId = 2;
                }

                return;
            }

            case 9:
            {
                if (entity.ForceResetAnimationFlag != 0)
                {
                    gameEngine.DestroyEntity(entity);
                }

                return;
            }
        }
    }

    // GHIDRA: AI_FUN_80073728 @ 0x80073728
    public static void AI_FUN_80073728(GameEngine gameEngine, Entity entity)
    {
        var parent = entity.ParentEntity;
        entity.PosY = parent.PosY;

        if (parent.TargetAnimationId == 9 || parent.TargetAnimationId == 6)
        {
            entity.TargetAnimationId = 2;
            entity.Flags |= 0x40;
        }

        if (entity.TargetAnimationId == 1)
        {
            if (entity.ForceResetAnimationFlag != 0)
            {
                entity.TargetAnimationId = 0;
                entity.AIValues[1] = 0xB4;
            }

            return;
        }

        if (entity.TargetAnimationId != 0)
        {
            return;
        }

        var delay = (ushort)(entity.AIValues[1] - 1);
        entity.AIValues[1] = unchecked((short)delay);

        if (delay == 0)
        {
            entity.TargetAnimationId = 2;
            entity.Flags |= 0x40;
        }
    }

    // GHIDRA: AI_FUN_80074ae8 @ 0x80074AE8
    public static void AI_FUN_80074ae8(GameEngine gameEngine, Entity entity)
    {
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
        if (entity.Name != "Melzas2_FinalBoss"
            && entity.Name != "Giles (homme religieux)"
            && entity.Name != "Klein (rêve uniquement)")
        {
            Breakpoint.TriggerBreak();
        }

        byte bVar1;
        short sVar2;
        uint uVar3;

        gameEngine.GetMatchingEntityBySearchType(entity, 0);

        if (gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].DelayOrAngleOrEntityId != 0 ||
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

            if (sVar2 == 0 && entity.IsOnGround != 0)
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

    // GHIDRA: AI_FUN_80075a3c @ 0x80075A3C
    public static void AI_FUN_80075a3c(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "Melzas2_FinalBoss")
        {
            Breakpoint.TriggerBreak();
        }

        byte bVar2;
        short sVar4;
        int iVar5;
        int iVar9;
        uint uVar6;
        Entity? pEVar7;
        int[] positions = new int[6];
        var player = gameEngine.StaticVariables.PlayerEntity;
        var spawnTable = gameEngine.StaticVariables.INT_ARRAY_80027c80;

        if (entity.DelayOrAngleOrEntityId == 0)
        {
            entity.DelayOrAngleOrEntityId = 1;
            gameEngine.StaticVariables.DAT_801911cc = 0;
            gameEngine.StaticVariables.DAT_801911c8 = 0;
            gameEngine.StaticVariables.DAT_801911c4 = 0;
            gameEngine.StaticVariables.DAT_801911c0 = 0;
            gameEngine.StaticVariables.DAT_801911dc = 0;
            gameEngine.StaticVariables.DAT_801911d8 = 0;
            gameEngine.StaticVariables.DAT_801911d4 = 0;
            gameEngine.StaticVariables.DAT_801911d0 = 0;
            gameEngine.StaticVariables.DAT_801911e8 = 0;
            gameEngine.StaticVariables.DAT_801911e4 = 0;
            gameEngine.StaticVariables.DAT_801911e0 = 0;
            gameEngine.StaticVariables.PTR_801911ec = null;
        }

        iVar5 = gameEngine.StaticVariables.DAT_801911e8 - 1;
        if (gameEngine.StaticVariables.DAT_801911e8 != 0)
        {
            gameEngine.StaticVariables.DAT_801911e8 = iVar5;

            if (iVar5 == 0)
            {
                gameEngine.StaticVariables.g_scrollingParameters.Flag = 0;
            }
        }

        if (entity.Bytes[0] == 6)
        {
            entity.ItemState += 1;

            if ((entity.ItemState & 0xF) == 0)
            {
                int targetX;
                int targetY;

                if (gameEngine.StaticVariables.PTR_801911ec == null)
                {
                    targetX = entity.PosX;
                    targetY = entity.PosY;
                }
                else
                {
                    targetX = gameEngine.StaticVariables.PTR_801911ec.PosX;
                    targetY = gameEngine.StaticVariables.PTR_801911ec.PosY;
                }

                gameEngine.StaticVariables.DAT_801911dc = ScriptHelper.GetDirectionToTarget(player.PosX - targetX, player.PosY - targetY);

                if (entity.TargetAnimationId == 5)
                {
                    pEVar7 = gameEngine.SpawnWarpEntity(entity, 1, 0xE0,
                        entity.PosX - 0x1E0000,
                        entity.PosY + 0x200000,
                        entity.TerrainHeight,
                        0);

                    if (pEVar7 != null)
                    {
                        pEVar7.TargetAnimationId = 4;
                        pEVar7.Flags |= 0x40;
                    }
                }
            }

            iVar5 = gameEngine.StaticVariables.g_offsetXList[gameEngine.StaticVariables.DAT_801911dc] * 0xA0;
            if (((iVar5 >= 0) || (0x900000 < entity.ModdedPosX - 0x10000)) &&
                ((iVar5 <= 0) || (entity.ModdedPosX + entity.Width + 0x10001 < 0x2100000)))
            {
                entity.PreviousAdjustedForceX = iVar5;
            }

            iVar5 = gameEngine.StaticVariables.g_offsetYList[gameEngine.StaticVariables.DAT_801911dc] * 0xA0;
            if (((iVar5 >= 0) || (0x2700000 < entity.ModdedPosY - 0x10000)) &&
                ((iVar5 <= 0) || (entity.ModdedPosY + entity.Height + 0x10001 < 0x3900000)))
            {
                entity.PreviousAdjustedForceY = iVar5;
            }

            if (gameEngine.StaticVariables.PTR_801911ec != null)
            {
                gameEngine.StaticVariables.PTR_801911ec.PosX = entity.PosX - 0x1E0000;
                gameEngine.StaticVariables.PTR_801911ec.PosY = entity.PosY + 0x1A0000;
            }
        }

        if (entity.Bytes[3] != 0 && entity.TargetAnimationId == 0)
        {
            if (entity.Bytes[3] < 4)
            {
                AI_Melzas2.UpdateEntityAI_BossExplode(gameEngine, entity);
                return;
            }

            entity.TargetAnimationId = 8;
            entity.Flags |= 0x40;
            return;
        }

        FUN_8007fe8c(gameEngine, entity, positions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.AIValues[1] != 0)
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                    return;
                }

                if (entity.Bytes[0] == 8)
                {
                    if (entity.PosZ - entity.TerrainHeight < 0xA00000)
                    {
                        entity.TargetAnimationId = 2;
                        entity.Bytes[0] = 0;
                        break;
                    }

                    entity.TargetAnimationId = 3;
                    entity.AIValues[1] = 0;
                    entity.ItemState = 0x1C;
                    entity.Flags |= 0x100;
                    break;
                }

                bVar2 = entity.Bytes[2];
                if (bVar2 != 0)
                {
                    entity.Bytes[2] = (byte)(bVar2 - 1);
                }

                if ((bVar2 == 0 || bVar2 == 1) &&
                    entity.IsOnGround != 0 &&
                    positions[3] >= 0 &&
                    positions[3] < 2 &&
                    positions[4] < 0)
                {
                    entity.TargetAnimationId = 4;
                    return;
                }

                if (entity.Bytes[0] == 1)
                {
                    if (entity.Bytes[1] == 0)
                    {
                        gameEngine.StaticVariables.DAT_801911c8 = (gameEngine.StaticVariables.DAT_801911d8 - 3) * 0x80;
                        gameEngine.StaticVariables.DAT_801911cc = gameEngine.StaticVariables.DAT_801911c8;
                        entity.Bytes[1] = 1;
                        gameEngine.StaticVariables.DAT_801911c0 = 0x01500000 + (entity.PosX - spawnTable[gameEngine.StaticVariables.DAT_801911d8 * 2]);
                        gameEngine.StaticVariables.DAT_801911c4 = (entity.PosY - spawnTable[(gameEngine.StaticVariables.DAT_801911d8 * 2) + 1]) + 0x03100000;
                    }

                    gameEngine.StaticVariables.DAT_801911c8 = (gameEngine.StaticVariables.DAT_801911c8 + 2) & 0x1FF;
                    if (gameEngine.StaticVariables.DAT_801911c8 == gameEngine.StaticVariables.DAT_801911cc)
                    {
                        entity.Bytes[0] = 0;
                        entity.TargetAnimationId = 2;
                    }

                    entity.PosX = gameEngine.StaticVariables.DAT_801911c0 + gameEngine.StaticVariables.g_sinus[gameEngine.StaticVariables.DAT_801911c8] * 0x3000;
                    entity.PosY = gameEngine.StaticVariables.DAT_801911c4 + gameEngine.StaticVariables.g_cosinus[gameEngine.StaticVariables.DAT_801911c8] * -0x3000;
                    return;
                }

                if (entity.Bytes[0] != 2)
                {
                    sVar4 = entity.AIValues[4];
                    entity.Bytes[0] = 0;

                    switch (sVar4 == 0 ? 4 : (int)((Random.Next() * 4UL) >> 32))
                    {
                        case 0:
                        case 3:
                            entity.Bytes[0] = 4;
                            entity.Bytes[1] = 5;
                            entity.TargetAnimationId = 1;
                            entity.AIValues[4] = (short)(sVar4 - 1);
                            entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                            entity.AIValues[1] = 0x3C;
                            return;

                        case 1:
                            entity.Bytes[0] = 3;
                            entity.Bytes[1] = 5;
                            entity.AIValues[4] = (short)(sVar4 - 1);
                            entity.TargetAnimationId = 1;
                            entity.AIValues[1] = 0x3C;
                            entity.TargetDirection = (uint)((Random.Next() * 0x20UL) >> 32);
                            return;

                        case 2:
                            entity.Bytes[0] = 8;
                            entity.TargetAnimationId = 1;
                            entity.AIValues[4] = (short)(sVar4 - 1);
                            gameEngine.StaticVariables.DAT_801911d8 = 3;
                            return;

                        case 4:
                            entity.TargetAnimationId = 2;
                            entity.Bytes[0] = 5;
                            entity.AIValues[4] = (short)(((Random.Next() * 3UL) >> 32) + 2);
                            return;

                        default:
                            return;
                    }
                }

                switch (entity.Bytes[1])
                {
                    case 0:
                        entity.Bytes[1] = (byte)(gameEngine.StaticVariables.DAT_801911d8 + 1);
                        gameEngine.StaticVariables.DAT_801911c4 = (entity.PosY - spawnTable[(gameEngine.StaticVariables.DAT_801911d8 * 2) + 1]) + 0x02C00000;

                        if (gameEngine.StaticVariables.DAT_801911d8 == 0)
                        {
                            gameEngine.StaticVariables.DAT_801911c8 = 0x180;
                            iVar9 = entity.PosX - spawnTable[0];
                            iVar5 = 0x1080000;
                        }
                        else
                        {
                            if (gameEngine.StaticVariables.DAT_801911d8 == 1)
                            {
                                gameEngine.StaticVariables.DAT_801911c8 = 0x180;
                                iVar9 = entity.PosX - spawnTable[2];
                            }
                            else
                            {
                                gameEngine.StaticVariables.DAT_801911c8 = 0x80;
                                iVar9 = entity.PosX - spawnTable[gameEngine.StaticVariables.DAT_801911d8 * 2];
                            }

                            iVar5 = 0x1980000;
                        }

                        gameEngine.StaticVariables.DAT_801911c0 = iVar9 + iVar5;
                        break;

                    case 1:
                        gameEngine.StaticVariables.DAT_801911c8 = (gameEngine.StaticVariables.DAT_801911c8 + 4) & 0x1FF;
                        if (gameEngine.StaticVariables.DAT_801911c8 == 0x80)
                        {
                            if (gameEngine.StaticVariables.DAT_801911d8 == 1)
                            {
                                entity.Bytes[0] = 0x80;
                            }

                            gameEngine.StaticVariables.DAT_801911c8 = 0x180;
                            gameEngine.StaticVariables.DAT_801911c0 += 0x900000;
                            entity.Bytes[1] = 2;
                        }
                        break;

                    case 2:
                        gameEngine.StaticVariables.DAT_801911c8 = (gameEngine.StaticVariables.DAT_801911c8 - 4) & 0x1FF;
                        if (gameEngine.StaticVariables.DAT_801911c8 == 0x80)
                        {
                            if (gameEngine.StaticVariables.DAT_801911d8 == 2)
                            {
                                entity.Bytes[0] = 0x80;
                            }

                            entity.Bytes[1] = 3;
                        }
                        break;

                    case 3:
                        gameEngine.StaticVariables.DAT_801911c8 = (gameEngine.StaticVariables.DAT_801911c8 - 4) & 0x1FF;
                        if (gameEngine.StaticVariables.DAT_801911c8 == 0x180)
                        {
                            gameEngine.StaticVariables.DAT_801911c8 = 0x80;
                            gameEngine.StaticVariables.DAT_801911c0 -= 0x900000;
                            entity.Bytes[1] = 4;
                        }
                        break;

                    case 4:
                        gameEngine.StaticVariables.DAT_801911c8 = (gameEngine.StaticVariables.DAT_801911c8 + 4) & 0x1FF;
                        if (gameEngine.StaticVariables.DAT_801911c8 == 0x180)
                        {
                            if (gameEngine.StaticVariables.DAT_801911d8 == 0)
                            {
                                entity.Bytes[0] = 0x80;
                            }

                            entity.Bytes[1] = 1;
                        }
                        break;
                }

                entity.PosX = gameEngine.StaticVariables.DAT_801911c0 + gameEngine.StaticVariables.g_sinus[gameEngine.StaticVariables.DAT_801911c8] * 0x1800;
                entity.PosY = gameEngine.StaticVariables.DAT_801911c4 + gameEngine.StaticVariables.g_cosinus[gameEngine.StaticVariables.DAT_801911c8] * -0x1800;

                if (entity.Bytes[0] == 0x80)
                {
                    entity.Bytes[0] = 0;
                    entity.TargetAnimationId = 2;
                }

                return;

            case 1:
                bVar2 = entity.Bytes[0];
                if (bVar2 == 5 || bVar2 == 8)
                {
                    iVar9 = entity.PosX - spawnTable[gameEngine.StaticVariables.DAT_801911d8 * 2];
                    int iVar10 = entity.PosY - spawnTable[(gameEngine.StaticVariables.DAT_801911d8 * 2) + 1];
                    iVar5 = iVar9;
                    if (iVar5 < 0)
                    {
                        iVar5 = -iVar5;
                    }

                    if (0x60000 < iVar5)
                    {
                        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(-iVar9, -iVar10);
                        return;
                    }

                    iVar5 = iVar10;
                    if (iVar5 < 0)
                    {
                        iVar5 = -iVar5;
                    }

                    if (0x60000 < iVar5)
                    {
                        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(-iVar9, -iVar10);
                        return;
                    }

                    if (bVar2 == 5)
                    {
                        entity.TargetAnimationId = 3;
                        return;
                    }

                    entity.TargetAnimationId = 2;
                    return;
                }

                sVar4 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar4;
                if (sVar4 != 0)
                {
                    if (entity.ForceAdjusted != 0 ||
                        entity.PosX < 0x900000 ||
                        0x1F80000 < entity.PosX ||
                        entity.PosY < 0x2700000 ||
                        0x3A00000 < entity.PosY)
                    {
                        bVar2 = gameEngine.StaticVariables.g_directionFlipTable[entity.TargetDirection];
                        entity.ForceStepY = 0;
                        entity.ForceStepX = 0;
                        entity.ForceY = 0;
                        entity.ForceX = 0;
                        entity.TargetForceY = 0;
                        entity.TargetForceX = 0;
                        entity.TargetDirection = bVar2;
                        return;
                    }

                    if (positions[0] < 3 && positions[1] < 3)
                    {
                        entity.TargetAnimationId = 10;
                        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                        entity.Flags |= 0x100;
                        return;
                    }

                    if (0x35FFFFF < entity.PosY)
                    {
                        return;
                    }

                    if (!gameEngine.EntityGameplayManager.TryAttackPlayer2(entity, positions, 0, 3, 5, 0x500000))
                    {
                        return;
                    }

                    entity.Bytes[0] = 6;
                    entity.TargetAnimationId = 4;
                    entity.ItemState = 0;
                    gameEngine.StaticVariables.DAT_801911dc = ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                    entity.TargetDirection = (uint)gameEngine.StaticVariables.DAT_801911dc;
                    return;
                }

                bVar2 = (byte)(entity.Bytes[1] - 1);
                entity.Bytes[1] = bVar2;
                if (bVar2 != 0)
                {
                    if (entity.Bytes[0] == 3)
                    {
                        entity.TargetDirection = (uint)((Random.Next() * 0x20UL) >> 32);
                    }
                    else
                    {
                        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                    }

                    entity.AIValues[1] = 0x3C;
                    return;
                }

                entity.TargetAnimationId = 0;
                entity.AIValues[1] = 0x3C;
                entity.Bytes[0] = 0;
                break;

            case 2:
                iVar5 = entity.PosZ - entity.TerrainHeight;
                bVar2 = entity.Bytes[0];
                entity.AIValues[1] = (short)((entity.AIValues[1] + 1) & 0xF);

                if (bVar2 == 8)
                {
                    if (0x9FFFFF < iVar5)
                    {
                        entity.ForceZ = 0;
                        entity.Flags &= 0xFFFFFEFFU;
                        gameEngine.SoundManager.PlaySoundEffect(0x63);
                        entity.AIValues[1] = 0x28;
                        entity.TargetAnimationId = 0;
                        return;
                    }
                }
                else if (0x3FFFFF < iVar5)
                {
                    entity.ForceZ = 0;
                    entity.Flags &= 0xFFFFFEFFU;

                    if (entity.Bytes[0] == 5)
                    {
                        entity.TargetAnimationId = 1;
                        gameEngine.StaticVariables.DAT_801911d8 = (int)((Random.Next() * 7UL) >> 32);
                        return;
                    }

                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0x3C;
                    return;
                }

                if (0x2FFFFF < iVar5 || entity.AIValues[1] != 0 || entity.Bytes[0] == 7)
                {
                    entity.Flags &= 0xFFFFFEFFU;
                    return;
                }

                entity.Flags |= 0x100;
                break;

            case 3:
                if (entity.Bytes[0] == 8)
                {
                    iVar5 = entity.ItemState;
                    if (7 < iVar5)
                    {
                        gameEngine.StaticVariables.DAT_801911e4 = (positions[1] << 20) / iVar5;
                        gameEngine.StaticVariables.DAT_801911e0 = (positions[0] * 0x180000) / iVar5;

                        if (positions[3] >= 0)
                        {
                            gameEngine.StaticVariables.DAT_801911e0 = -gameEngine.StaticVariables.DAT_801911e0;
                        }

                        if (positions[4] >= 0)
                        {
                            gameEngine.StaticVariables.DAT_801911e4 = -gameEngine.StaticVariables.DAT_801911e4;
                        }

                        entity.ItemState -= 1;
                    }

                    if (entity.ModdedPosX - 0x10000 < 0x900001 ||
                        0x20FFFFF < entity.ModdedPosX + entity.Width + 0x10001)
                    {
                        gameEngine.StaticVariables.DAT_801911e0 = 0;
                    }
                    else if (entity.ModdedPosY - 0x10000 < 0x2700001 ||
                             0x3AFFFFF < entity.ModdedPosY + entity.Height + 0x10001)
                    {
                        gameEngine.StaticVariables.DAT_801911e4 = 0;
                    }

                    entity.PreviousAdjustedForceX = gameEngine.StaticVariables.DAT_801911e0;
                    entity.PreviousAdjustedForceY = gameEngine.StaticVariables.DAT_801911e4;
                }

                if (entity.AIValues[1] == 0 && entity.PosZ - entity.TerrainHeight < 0x200001)
                {
                    entity.AIValues[1] = 1;
                    entity.ForceZ >>= 1;

                    if (entity.Bytes[0] == 8)
                    {
                        entity.Flags &= 0xFFFFFEFFU;
                    }
                }

                if (entity.IsOnGround != 0)
                {
                    bVar2 = entity.Bytes[0];
                    entity.AIValues[1] = 0;
                    entity.TargetAnimationId = 0;

                    if (bVar2 == 5)
                    {
                        entity.Bytes[1] = 0;
                        bVar2 = 2;
                        if (2 < gameEngine.StaticVariables.DAT_801911d8)
                        {
                            bVar2 = 1;
                        }

                        entity.Bytes[0] = bVar2;
                    }

                    if (entity.Bytes[0] == 8)
                    {
                        gameEngine.StaticVariables.DAT_801911e4 = 0;
                        gameEngine.StaticVariables.DAT_801911e0 = 0;
                        entity.AIValues[1] = 0x1E;
                        gameEngine.StaticVariables.DAT_801911e8 = 0x14;
                        gameEngine.StaticVariables.g_scrollingParameters.Flag = 1;
                        gameEngine.StaticVariables.g_scrollingParameters.SpeedX = 1;
                        gameEngine.StaticVariables.g_scrollingParameters.SpeedY = 1;
                        gameEngine.StaticVariables.g_scrollingParameters.LimitX = 2;
                        gameEngine.StaticVariables.g_scrollingParameters.LimitY = 2;
                    }
                }
                break;

            case 4:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 5;
                    if (entity.IsOnGround == 0)
                    {
                        gameEngine.SoundManager.PlaySoundEffect(0x62);
                        gameEngine.StaticVariables.PTR_801911ec = gameEngine.SpawnWarpEntity(entity, 1, 0xE0,
                            entity.PosX - 0x1E0000,
                            entity.PosY + 0x1A0000,
                            entity.PosZ - 0x3C0000,
                            0);
                        entity.AIValues[1] = 0xF0;
                    }
                    else
                    {
                        entity.AIValues[1] = 1;
                        entity.Bytes[2] = 0;
                    }
                }
                break;

            case 5:
                sVar4 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar4;
                if (sVar4 == 0)
                {
                    if (entity.IsOnGround == 0)
                    {
                        gameEngine.SoundManager.PlaySoundEffect(0x62);
                        Entity effectEntity = gameEngine.StaticVariables.PTR_801911ec!;
                        entity.TargetAnimationId = 6;
                        gameEngine.StaticVariables.PTR_801911ec = null;
                        effectEntity.TargetAnimationId = 2;
                        effectEntity.Flags |= 0x40;
                    }
                    else
                    {
                        gameEngine.SoundManager.PlaySoundEffect(0x61);
                        uVar6 = 0x1D;
                        do
                        {
                            pEVar7 = gameEngine.SpawnWarpEntity(entity, 1, 0xE1,
                                entity.PosX - 0x1C0000,
                                entity.PosY + 0x200000,
                                entity.PosZ + 0x100000,
                                uVar6);

                            if (pEVar7 != null)
                            {
                                pEVar7.TargetAnimationId = 1;
                            }

                            uVar6 = (uVar6 + 1) & 0x1F;
                        } while (uVar6 != 4);

                        entity.AIValues[1] = 0x19;
                        bVar2 = (byte)(entity.Bytes[2] + 1);
                        entity.Bytes[2] = bVar2;
                        if (bVar2 == 3)
                        {
                            entity.TargetAnimationId = 6;
                        }
                    }
                }
                break;

            case 6:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 0;
                if (entity.IsOnGround != 0)
                {
                    entity.Bytes[2] = 0x28;
                    return;
                }

                entity.Bytes[0] = 0;
                break;

            case 7:
                if (gameEngine.StaticVariables.PTR_801911ec != null)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x62);
                    Entity effectEntity = gameEngine.StaticVariables.PTR_801911ec;
                    uint effectFlags = effectEntity.Flags;
                    gameEngine.StaticVariables.PTR_801911ec = null;
                    effectEntity.TargetAnimationId = 2;
                    effectEntity.Flags = effectFlags | 0x40;
                }

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[3] != 0)
                {
                    entity.AIValues[1] = 300;
                    entity.AIValues[5] = 0x1E;
                    entity.TargetAnimationId = 0;
                    entity.Flags &= 0xFFFFFFFCU;
                    gameEngine.StaticVariables.g_scrollingParameters.Flag = 0;
                    entity.ForceZ = 0;
                    return;
                }

                entity.DamagedTickCounter = 0x5A;
                if ((uint)(entity.Bytes[0] - 1) < 2U)
                {
                    entity.TargetAnimationId = 0;
                    entity.Bytes[2] = 0x28;
                    return;
                }

                entity.TargetAnimationId = 2;
                entity.Bytes[0] = 0;
                break;

            case 9:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 2;
                return;

            case 10:
                if (entity.IsOnGround == 0)
                {
                    return;
                }

                entity.AIValues[1] = 0;
                entity.Flags &= 0xFFFFFEFFU;
                if (((Random.Next() * 3UL) >> 32) == 0)
                {
                    entity.TargetAnimationId = 9;
                    return;
                }

                entity.TargetAnimationId = 2;
                return;
        }
    }

    //8007763c
    public static void AI_FUN_8007763c(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "Melzas2_FinalBoss")
        {
            Breakpoint.TriggerBreak();
        }

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

    // GHIDRA: AI_FUN_80077734 @ 0x80077734
    public static void AI_FUN_80077734(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Wilda (tête)")
        {
            Breakpoint.TriggerBreak();
        }

        byte bVar3 = 0;
        short sVar4 = 0;
        ushort uVar5;
        uint uVar6;
        Entity? entitySpawned;
        SpriteEffect? spriteEffect;
        int iVar9;
        int iVar10;
        int iVar11;
        int iVar12;
        int iVar13;
        int iVar15;
        int local_40 = 0;

        if (gameEngine.StaticVariables.g_loaderInitialized == 0)
        {
            gameEngine.StaticVariables.PTR_ARRAY_80191204[0] = null;
            gameEngine.StaticVariables.g_loaderInitialized = 1;
            gameEngine.StaticVariables.DAT_80191200 = 0;
            gameEngine.StaticVariables.DAT_801911fe = 0;
        }

        if (entity.TargetAnimationId == 0 && entity.ForceResetAnimationFlag != 0)
        {
            entity.CurrentAnimationId = 0xFF;
        }

        if (entity.TargetAnimationId == 1 && entity.ForceResetAnimationFlag != 0)
        {
            entity.CurrentAnimationId = 0xFF;
        }

        if ((gameEngine.StaticVariables.g_temporaryFlags[3] & 0x10) == 0)
        {
            return;
        }

        if (entity.DelayOrAngleOrEntityId != 0)
        {
            uVar6 = (uint)(entity.DelayOrAngleOrEntityId - 1);
            entity.DelayOrAngleOrEntityId = (int)uVar6;

            if (uVar6 == 0)
            {
                gameEngine.StaticVariables.g_scrollingParameters.Flag = 0;
            }
            else if ((uVar6 & 0x1F) == 0)
            {
                entity.AIValues[4] = 6;
                entity.Bytes[2] = (byte)((Random.Next() * 0x20UL) >> 32);
            }

            if (entity.AIValues[4] != 0)
            {
                Entity player = gameEngine.StaticVariables.PlayerEntity;
                entity.AIValues[4] -= 1;

                if (player.IsOnGround != 0 && (player.AnimFlags & 0x40) == 0)
                {
                    player.PreviousAdjustedForceX = gameEngine.StaticVariables.g_offsetXList[entity.Bytes[2]] * 0xC0;
                    player.PreviousAdjustedForceY = gameEngine.StaticVariables.g_offsetYList[entity.Bytes[2]] * 0xC0;
                }
            }
        }

        entitySpawned = gameEngine.StaticVariables.PTR_ARRAY_80191204[0];

        if (entity.TargetAnimationId == 8)
        {
            if (entitySpawned != null)
            {
                entitySpawned.TargetAnimationId = 2;
                gameEngine.StaticVariables.PTR_ARRAY_80191204[0] = null;
                entitySpawned.Flags |= 0x40;
            }

            if (entity.ForceResetAnimationFlag == 0)
            {
                return;
            }

            if (entity.Bytes[3] == 0)
            {
                if (entity.AIValues[5] == 0 && ((Random.Next() * 3UL) >> 32) != 0)
                {
                    entity.AIValues[5] = 1;
                    entity.TargetAnimationId = 6;

                    if (((Random.Next() * 2UL) >> 32) == 0)
                    {
                        entity.Bytes[0] = 9;
                        entity.Bytes[1] = 2;
                    }
                    else
                    {
                        entity.Bytes[0] = 2;
                        entity.Bytes[1] = 2;
                    }
                }
                else
                {
                    entity.TargetAnimationId = 3;
                    entity.Bytes[0] = 4;
                    entity.Bytes[1] = 0;
                    entity.AIValues[5] = 0;
                }

                entity.DamagedTickCounter = 0x5A;
                return;
            }

            Entity linkedEntity = gameEngine.StaticVariables.g_entitySlots[entity.AIValues.GetInt32(2)];
            entity.TargetAnimationId = 9;
            linkedEntity.TargetAnimationId = 1;
            linkedEntity.AIValues[1] = 300;
            linkedEntity.AIValues[5] = 0x1E;
            linkedEntity.Bytes[3] = entity.Bytes[3];
            entity.Bytes[0] = 5;
            entity.Bytes[1] = 0;
            entity.AIValues[1] = 0;
            gameEngine.StaticVariables.g_scrollingParameters.Flag = 0;
            return;
        }

        switch (entity.Bytes[0])
        {
            case 0:
                sVar4 = entity.AIValues[1];
                entity.TargetAnimationId = 0;
                entity.Bytes[1] = 0;

                if (sVar4 != 0)
                {
                    goto code_r0x80078450;
                }

                if (entity.ForceResetAnimationFlag == 0)
                {
                    goto switchD_80078810_caseD_5;
                }

                gameEngine.StaticVariables.DAT_80191200 += 1;

                if (gameEngine.StaticVariables.DAT_80191200 < 4)
                {
                    iVar13 = (int)((Random.Next() * 9UL) >> 32);
                }
                else
                {
                    gameEngine.StaticVariables.DAT_80191200 = 0;
                    iVar13 = (int)((Random.Next() * 3UL) >> 32) + 4;
                }

                switch (iVar13)
                {
                    case 0:
                    case 6:
                        entity.Bytes[0] = 2;
                        break;

                    case 1:
                    case 8:
                        entity.Bytes[0] = 1;
                        break;

                    case 2:
                    case 7:
                        entity.Bytes[0] = 9;
                        break;

                    case 3:
                        entity.Bytes[0] = 3;
                        break;

                    case 4:
                        if (entity.DelayOrAngleOrEntityId == 0)
                        {
                            entity.Bytes[0] = 7;
                            entity.TargetAnimationId = 0xB;
                            entity.AIValues[1] = 10;
                            break;
                        }

                        entity.Bytes[0] = 1;
                        break;

                    case 5:
                        entity.Bytes[0] = 8;
                        entity.AIValues[1] = 0x30;

                        if (gameEngine.StaticVariables.DAT_801911fe == 0)
                        {
                            iVar13 = (int)((Random.Next() * 4UL) >> 32);
                            entity.Bytes[1] = gameEngine.StaticVariables.BYTE_ARRAY_80028b54[iVar13];
                            gameEngine.StaticVariables.DAT_801911fe = (short)(iVar13 + 1);
                        }
                        else
                        {
                            entity.Bytes[1] = (byte)((gameEngine.StaticVariables.BYTE_ARRAY_80028b54[gameEngine.StaticVariables.DAT_801911fe - 1] + 0x10) & 0x1F);
                            gameEngine.StaticVariables.DAT_801911fe = 0;
                        }

                        break;
                }

                goto switchD_80078810_caseD_5;

            case 1:
                bVar3 = entity.Bytes[1];

                if (bVar3 == 1)
                {
                    sVar4 = (short)(entity.AIValues[1] - 1);
                    entity.AIValues[1] = sVar4;

                    if (sVar4 != 0)
                    {
                        goto switchD_80078810_caseD_5;
                    }

                    bVar3 = entity.Bytes[1];
                    entity.AIValues[1] = 1;
                }
                else
                {
                    if (bVar3 < 2)
                    {
                        if (bVar3 != 0)
                        {
                            goto switchD_80078810_caseD_5;
                        }

                        entity.AIValues[1] = 0x4E;
                        gameEngine.StaticVariables.DAT_801911fc = 0;
                        gameEngine.StaticVariables.DAT_801911f8 = (short)((Random.Next() * 3UL) >> 32);
                        gameEngine.StaticVariables.DAT_801911fa = (short)(gameEngine.StaticVariables.DAT_801911f8 + 2);
                        bVar3 = entity.Bytes[1];
                        uVar6 = 4;
                        goto LAB_80078970;
                    }

                    if (bVar3 != 2)
                    {
                        if (bVar3 != 3 || entity.ForceResetAnimationFlag == 0)
                        {
                            goto switchD_80078810_caseD_5;
                        }

                        entity.Bytes[1] = 0;

                        if (((Random.Next() * 2UL) >> 32) == 0)
                        {
                            entity.Bytes[0] = 2;
                            goto switchD_80078810_caseD_5;
                        }

                        entity.Bytes[0] = 0;
                        sVar4 = (short)((Random.Next() * 0x10UL) >> 32);
                        goto LAB_80078748;
                    }

                    sVar4 = entity.AIValues[1];

                    if (sVar4 != 0)
                    {
                        entity.AIValues[1] = (short)(sVar4 - 1);

                        if (sVar4 == 1)
                        {
                            iVar13 = gameEngine.StaticVariables.DAT_801911fc;

                            if (iVar13 < 5)
                            {
                                uVar6 = (uint)(0xC - iVar13 * 2);
                            }
                            else if (iVar13 < 10)
                            {
                                uVar6 = (uint)(iVar13 * 2 - 6);
                            }
                            else
                            {
                                uVar6 = (uint)(0xC - (iVar13 - 10) * 2);
                            }

                            entitySpawned = gameEngine.SpawnWarpEntity(entity, 1, 0x9F,
                                entity.PosX - 0x480000, entity.PosY, entity.PosZ + 0x300000, uVar6);

                            if (entitySpawned != null)
                            {
                                entitySpawned.Bytes[1] = 1;
                                entitySpawned.Bytes[0] = (byte)gameEngine.StaticVariables.DAT_801911fa;
                            }

                            gameEngine.StaticVariables.DAT_801911fc += 1;

                            if (gameEngine.StaticVariables.DAT_801911fc == 5)
                            {
                                gameEngine.StaticVariables.DAT_801911fa = (short)(gameEngine.StaticVariables.DAT_801911f8 == 1 ? 2 : 3);
                            }
                            else if (gameEngine.StaticVariables.DAT_801911fc == 10 || gameEngine.StaticVariables.DAT_801911fc == 0xF)
                            {
                                gameEngine.StaticVariables.DAT_801911fa = (short)(gameEngine.StaticVariables.DAT_801911f8 == 2 ? gameEngine.StaticVariables.DAT_801911f8 : 4);
                            }
                            else
                            {
                                entity.AIValues[1] = 0xF;
                            }
                        }
                    }

                    if (entity.ForceResetAnimationFlag == 0)
                    {
                        goto switchD_80078810_caseD_5;
                    }

                    if (gameEngine.StaticVariables.DAT_801911fc != 0xF)
                    {
                        entity.TargetAnimationId = 4;
                        entity.AIValues[1] = 0x4E;
                        entity.Bytes[1] = 1;
                        goto switchD_80078810_caseD_5;
                    }

                    bVar3 = entity.Bytes[1];
                    entity.TargetAnimationId = 0;
                }

                goto LAB_80078974;

            case 2:
                switch (entity.Bytes[1])
                {
                    case 0:
                        bVar3 = entity.Bytes[1];
                        uVar6 = 2;
                        goto LAB_80078970;

                    case 1:
                        if (entity.ForceResetAnimationFlag == 0)
                        {
                            goto switchD_80078810_caseD_5;
                        }

                        bVar3 = (byte)(entity.Bytes[1] + 1);
                        goto LAB_80078978;

                    case 2:
                        entity.TargetAnimationId = 6;
                        bVar3 = entity.Bytes[1];
                        entity.AIValues[1] = 0x14;
                        break;

                    case 3:
                        local_40 = 0x40;
                        sVar4 = (short)(entity.AIValues[1] - 1);
                        entity.AIValues[1] = sVar4;

                        if (sVar4 != 0)
                        {
                            goto switchD_80078810_caseD_5;
                        }

                        gameEngine.StaticVariables.PTR_ARRAY_80191204[0] = gameEngine.SpawnWarpEntity(entity, 1, 0x9E,
                            entity.PosX - 0xA00000, entity.PosY + 0x300000, entity.PosZ - 0x100000,
                            entity.TargetDirection);
                        bVar3 = entity.Bytes[1];
                        entity.ItemState = 0;
                        break;

                    case 4:
                        local_40 = 0xA0;
                        uVar5 = (ushort)(entity.AIValues[1] + 1);
                        entity.AIValues[1] = (short)uVar5;

                        if ((uVar5 & 3) == 0)
                        {
                            iVar13 = (int)((Random.Next() * 2UL) >> 32);
                            iVar15 = 0;

                            do
                            {
                                uVar6 = (uint)Random.Next();
                                iVar9 = entity.ItemState;
                                iVar11 = entity.ItemState + 1;
                                entity.ItemState = iVar11;
                                iVar10 = INT_ARRAY_80027ed4[iVar9 * 2];
                                iVar9 = INT_ARRAY_80027ed4[iVar9 * 2 + 1];

                                if (iVar11 > 5)
                                {
                                    entity.ItemState = 0;
                                }

                                Entity spawnedEntity = gameEngine.SpawnWarpEntity(entity, 0, 0xEC,
                                    iVar10 + (int)((uVar6 * 0x19UL) >> 32) * 0x40000,
                                    iVar9 + (int)((Random.Next() * 0x19UL) >> 32) * 0x20000,
                                    0x30000, 0)!;
                                iVar15 += 1;
                                spawnedEntity.ForceZ = 0x8000;
                            }
                            while (iVar15 != iVar13 + 2);
                        }

                        if (entity.ForceResetAnimationFlag == 0)
                        {
                            goto switchD_80078810_caseD_5;
                        }

                        bVar3 = entity.Bytes[1];
                        entity.CurrentAnimationId = 0xFFFFFFFFU;
                        break;

                    case 5:
                        local_40 = 0xA0;

                        if (entity.ForceResetAnimationFlag != 0)
                        {
                            entity.Bytes[1] = (byte)(entity.Bytes[1] + 1);
                            entitySpawned = gameEngine.StaticVariables.PTR_ARRAY_80191204[0];
                            entity.TargetAnimationId = 1;

                            if (entitySpawned != null)
                            {
                                entitySpawned.TargetAnimationId = 2;
                                gameEngine.StaticVariables.PTR_ARRAY_80191204[0] = null;
                                entitySpawned.Flags |= 0x40;
                            }
                        }

                        goto switchD_80078810_caseD_5;

                    case 6:
                        if (entity.ForceResetAnimationFlag == 0)
                        {
                            goto switchD_80078810_caseD_5;
                        }

                        if (entity.AIValues[5] == 0 && ((Random.Next() * 2UL) >> 32) == 0)
                        {
                            entity.Bytes[1] = 2;
                            entity.AIValues[5] = 1;
                            goto switchD_80078810_caseD_5;
                        }

                        bVar3 = (byte)(entity.Bytes[1] + 1);
                        goto LAB_80078978;

                    case 7:
                        uVar6 = 3;

                        if (entity.ForceResetAnimationFlag == 0)
                        {
                            goto switchD_80078810_caseD_5;
                        }

                        bVar3 = entity.Bytes[1];
                        goto LAB_80078970;

                    case 8:
                        if (entity.ForceResetAnimationFlag != 0)
                        {
                            entity.Bytes[0] = 0;
                            entity.AIValues[1] = 0x1E;
                            entity.AIValues[5] = 0;
                        }

                        goto switchD_80078810_caseD_5;
                }

                goto LAB_80078974;

            case 3:
                bVar3 = entity.Bytes[1];

                if (bVar3 == 1)
                {
                    if (entity.ForceResetAnimationFlag == 0)
                    {
                        goto switchD_80078810_caseD_5;
                    }

                    entity.TargetAnimationId = 3;
                    entity.Bytes[1] = 2;
                    goto switchD_80078810_caseD_5;
                }

                if (bVar3 < 2)
                {
                    if (bVar3 == 0)
                    {
                        entity.TargetAnimationId = 2;
                        entity.Bytes[1] = 1;
                    }

                    goto switchD_80078810_caseD_5;
                }

                sVar4 = 0x28;

                if (bVar3 != 2 || entity.ForceResetAnimationFlag == 0)
                {
                    goto switchD_80078810_caseD_5;
                }

                entity.Bytes[0] = 0;
                break;

            case 4:
                sVar4 = 0x1E;

                if (entity.ForceResetAnimationFlag == 0)
                {
                    goto switchD_80078810_caseD_5;
                }

                entity.Bytes[0] = 0;
                break;

            case 5:
                if (entity.TargetAnimationId == 9 && entity.ForceResetAnimationFlag != 0)
                {
                    if ((gameEngine.StaticVariables.g_temporaryFlags[0] & 1) == 0)
                    {
                        gameEngine.StaticVariables.g_temporaryFlags[0] |= 1;
                    }

                    if ((gameEngine.StaticVariables.g_temporaryFlags[0] & 0x20) != 0)
                    {
                        Entity linkedEntity = gameEngine.StaticVariables.g_entitySlots[entity.AIValues.GetInt32(2)];

                        if (linkedEntity.Bytes[3] < 2)
                        {
                            iVar15 = linkedEntity.ModdedPosZ;
                            iVar13 = linkedEntity.ModdedPosY;
                            linkedEntity.ModdedPosY = iVar13 + 0x500000;
                            linkedEntity.ModdedPosZ = iVar15 + 0x500000;
                            AI_Melzas2.UpdateEntityAI_BossExplode(gameEngine, linkedEntity);
                            linkedEntity.ModdedPosY = iVar13;
                            linkedEntity.ModdedPosZ = iVar15;
                        }
                        else if ((gameEngine.StaticVariables.g_temporaryFlags[0] & 0x40) == 0)
                        {
                            gameEngine.StaticVariables.g_temporaryFlags[0] |= 0x40;
                            entity.TargetAnimationId = 10;
                            entity.AIValues[1] = 0;
                        }
                    }

                    goto switchD_80078810_caseD_5;
                }

                if (entity.TargetAnimationId != 10)
                {
                    goto switchD_80078810_caseD_5;
                }

                sVar4 = entity.AIValues[1];

                if (sVar4 != 0)
                {
                    entity.AIValues[1] = (short)(sVar4 - 1);

                    if (sVar4 == 1)
                    {
                        iVar12 = entity.PosX;
                        iVar13 = entity.Bytes[1] * 3;
                        iVar11 = INT_ARRAY_80027e6c[iVar13];
                        iVar10 = entity.PosY;
                        iVar15 = INT_ARRAY_80027e6c[iVar13 + 1];
                        iVar13 = INT_ARRAY_80027e6c[iVar13 + 2];
                        iVar9 = entity.PosZ;
                        gameEngine.SoundManager.PlaySoundEffect(0xC4);
                        gameEngine.EffectManager.CreateEffectEntity(0, 8, 0,
                            iVar12 + iVar11,
                            iVar10 + iVar15 + 0x500000,
                            iVar9 + iVar13 + 0x500000);
                        bVar3 = (byte)(entity.Bytes[1] + 1);
                        entity.Bytes[1] = bVar3;

                        if (bVar3 == 6)
                        {
                            entity.Bytes[0] = 6;
                        }
                    }

                    goto switchD_80078810_caseD_5;
                }

                sVar4 = SHORT_ARRAY_80027e54[entity.Bytes[1]];
                goto code_r0x80078450;

            case 7:
                bVar3 = entity.Bytes[1];

                if (bVar3 != 1)
                {
                    if (bVar3 < 2)
                    {
                        if (bVar3 == 0)
                        {
                            sVar4 = entity.AIValues[1];

                            if (sVar4 != 0)
                            {
                                entity.AIValues[1] = (short)(sVar4 - 1);

                                if (sVar4 == 1)
                                {
                                    gameEngine.StaticVariables.g_scrollingParameters.Flag = 1;
                                    gameEngine.StaticVariables.g_scrollingParameters.LimitX = 4;
                                    gameEngine.StaticVariables.g_scrollingParameters.SpeedX = 1;
                                    gameEngine.StaticVariables.g_scrollingParameters.SpeedY = 1;
                                    gameEngine.StaticVariables.g_scrollingParameters.LimitY = 2;
                                    entity.Bytes[1] = 1;
                                    entity.AIValues[1] = 0x1E;
                                }
                            }
                        }

                        goto switchD_80078810_caseD_5;
                    }

                    if (bVar3 != 2)
                    {
                        goto switchD_80078810_caseD_5;
                    }

                    uVar5 = (ushort)(entity.AIValues[1] + 1);
                    entity.AIValues[1] = (short)uVar5;

                    if ((uVar5 & 0xF) == 0)
                    {
                        iVar13 = 0;

                        do
                        {
                            uVar6 = (uint)Random.Next();
                            spriteEffect = gameEngine.EffectManager.CreateEffectEntity(0, 0x16, 1,
                                INT_ARRAY_80027eb4[iVar13 * 2] + (int)((uVar6 * 0xFUL) >> 32) * 0xC0000,
                                INT_ARRAY_80027eb4[iVar13 * 2 + 1] + (int)((Random.Next() * 0x13UL) >> 32) * 0x80000,
                                0x1100000);

                            if (spriteEffect != null)
                            {
                                spriteEffect.ForceZ = (int)((Random.Next() * 5UL) >> 32) * 0x4000 - 0x40000;
                            }

                            iVar13 += 1;
                        }
                        while (iVar13 != 4);
                    }

                    if (entity.ForceResetAnimationFlag == 0)
                    {
                        goto switchD_80078810_caseD_5;
                    }

                    gameEngine.StaticVariables.g_scrollingParameters.LimitX = 2;
                    entity.Bytes[0] = 0;
                    sVar4 = (short)((Random.Next() * 0x10UL) >> 32);
                    entity.DelayOrAngleOrEntityId = 600;
                    goto LAB_80078748;
                }

                sVar4 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar4;

                if (sVar4 != 0)
                {
                    goto switchD_80078810_caseD_5;
                }

                entity.Bytes[1] = 2;
                goto switchD_80078810_caseD_5;

            case 8:
                sVar4 = gameEngine.StaticVariables.g_offsetXList[entity.Bytes[1]];
                iVar13 = entity.AIValues.GetInt32(2);

                if (iVar13 != 0)
                {
                    gameEngine.StaticVariables.g_entitySlots[iVar13].PreviousAdjustedForceX = sVar4 * 0x30;
                }

                entity.PreviousAdjustedForceX = sVar4 * 0x30;
                sVar4 = gameEngine.StaticVariables.g_offsetYList[entity.Bytes[1]];

                if (iVar13 != 0)
                {
                    gameEngine.StaticVariables.g_entitySlots[iVar13].PreviousAdjustedForceY = sVar4 * 0x30;
                }

                entity.PreviousAdjustedForceY = gameEngine.StaticVariables.g_offsetYList[entity.Bytes[1]] * 0x30;
                entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                sVar4 = 0x1E;

                if (entity.AIValues[1] != 0)
                {
                    goto switchD_80078810_caseD_5;
                }

                entity.Bytes[0] = 0;
                break;

            case 9:
                switch (entity.Bytes[1])
                {
                    case 0:
                        bVar3 = entity.Bytes[1];
                        uVar6 = 2;
                        goto LAB_80078970;

                    case 1:
                        if (entity.ForceResetAnimationFlag == 0)
                        {
                            break;
                        }

                        bVar3 = (byte)(entity.Bytes[1] + 1);
                        goto LAB_80078978;

                    case 2:
                        entity.TargetAnimationId = 6;
                        bVar3 = entity.Bytes[1];
                        entity.AIValues[1] = 0x14;
                        entity.Bytes[1] = (byte)(bVar3 + 1);
                        gameEngine.StaticVariables.DAT_801911fc = 0;
                        gameEngine.StaticVariables.DAT_801911f8 = (short)(gameEngine.StaticVariables.PlayerEntity.PosY < 0x1300000 ? 0 : 1);
                        goto switchD_80078810_caseD_5;

                    case 3:
                        iVar13 = gameEngine.StaticVariables.DAT_801911fc;

                        if (iVar13 != 6)
                        {
                            sVar4 = (short)(entity.AIValues[1] - 1);
                            entity.AIValues[1] = sVar4;

                            if (sVar4 == 0)
                            {
                                uVar6 = gameEngine.StaticVariables.DAT_801911f8 == 0
                                    ? (uint)(0xF - iVar13 * 2)
                                    : (uint)(iVar13 * 2 + 1);
                                entitySpawned = gameEngine.SpawnWarpEntity(entity, 1, 0x9F,
                                    entity.PosX - 0x780000, entity.PosY + 0x100000, entity.PosZ, uVar6);

                                if (entitySpawned != null)
                                {
                                    entitySpawned.Bytes[0] = 4;
                                    entitySpawned.Bytes[1] = 9;
                                }

                                entity.AIValues[1] = 10;
                                gameEngine.StaticVariables.DAT_801911fc += 1;
                            }
                        }

                        goto switchD_80077fb0_caseD_7;

                    case 4:
                        goto switchD_80078810_caseD_4;
                }

                goto switchD_80078810_caseD_5;
        }

        entity.AIValues[1] = sVar4;

    switchD_80078810_caseD_5:
        if (local_40 != 0)
        {
            for (iVar13 = 0; iVar13 != 0x40; iVar13++)
            {
                Entity candidate = gameEngine.StaticVariables.g_entitySlots[iVar13];
                if (iVar13 == 0 || candidate.SpriteTableIndex == 2 || (candidate.SpriteTableIndex == 0xEC && candidate.TargetAnimationId == 0))
                {
                    candidate.PreviousAdjustedForceX = gameEngine.StaticVariables.g_offsetXList[8] * local_40;
                }
            }
        }

        return;

    LAB_80078970:
        entity.TargetAnimationId = uVar6;
        goto LAB_80078974;

    LAB_80078974:
        bVar3 = (byte)(bVar3 + 1);

    LAB_80078978:
        entity.Bytes[1] = bVar3;
        goto switchD_80078810_caseD_5;

    LAB_80078748:
        entity.AIValues[1] = (short)(sVar4 + 0x1E);
        goto switchD_80078810_caseD_5;

    code_r0x80078450:
        entity.AIValues[1] = (short)(sVar4 - 1);
        goto switchD_80078810_caseD_5;

    switchD_80077fb0_caseD_7:
        uVar6 = 3;
        if (entity.ForceResetAnimationFlag == 0)
        {
            goto switchD_80078810_caseD_5;
        }
        bVar3 = entity.Bytes[1];
        goto LAB_80078970;

    switchD_80078810_caseD_4:
        if (entity.ForceResetAnimationFlag != 0)
        {
            entity.Bytes[0] = 0;
            entity.AIValues[1] = 0x1E;
            entity.AIValues[5] = 0;
        }

        goto switchD_80078810_caseD_5;
    }

    //80078a5c
    public static void AI_FUN_80078a5c(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "Melzas2_FinalBoss")
        {
            Breakpoint.TriggerBreak();
        }

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
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

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
                dx = entity.PosX - entity.DelayOrAngleOrEntityId;
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
                    dx = entity.PosX - entity.DelayOrAngleOrEntityId;
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
            if (entity.IsOnGround == 0)
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
                dx = entity.PosX - entity.DelayOrAngleOrEntityId;
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
                    dx = entity.PosX - entity.DelayOrAngleOrEntityId;
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
                entity.DelayOrAngleOrEntityId = gameEngine.StaticVariables.PlayerEntity.PosX;
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
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

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

            index = (uint)(parentEntity.DelayOrAngleOrEntityId + entity.DelayOrAngleOrEntityId & 0x1ff);
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
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

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
                    gameEngine.StaticVariables.g_temporaryFlags[0] |= 1;
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
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

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
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

        //do nothing
    }

    //8007a4b0
    public static void AI_FUN_8007a4b0(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

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
                if (entity.DelayOrAngleOrEntityId < 0x1800)
                {
                    entity.DelayOrAngleOrEntityId += 0x40;
                }
                else
                {
                    entity.ItemState -= 8;
                }

                entity.AIValues[4] = (short)((entity.AIValues[4] + 10U) & 0x1ff);
                entity.PosX = gameEngine.StaticVariables.g_sinus[(ushort)entity.AIValues[4]] * entity.DelayOrAngleOrEntityId + 0x01500000;
                entity.PosY = gameEngine.StaticVariables.g_cosinus[(ushort)entity.AIValues[4]] * entity.DelayOrAngleOrEntityId + 0x3100000;

                if (entity.ItemState == 0)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x127);
                    uVar1 = (ushort)entity.AIValues[4];
                    entity.TargetAnimationId = 2;
                    entity.TargetDirection = (uint)((0x20 - ((uVar1 >> 4) & 0x1f)) & 0x1f);
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
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

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
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Boule de feu"
            && entity.Name != "Boule de glace")
        {
            Breakpoint.TriggerBreak();
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
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Épée d’onde Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        gameEngine.CheckAndTriggerTileEffect(entity);
    }

    //8007a978
    //handle bombs
    public static void AI_FUN_8007a978(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name !=  null
            && entity.Name != "Bombe")
        {
            Breakpoint.TriggerBreak();
        }

        SpriteEffect effect;
        int i;
        uint rand2;

        if ((entity.CombinedVramFlagsAND & 4U) != 0)
        {
            gameEngine.DestroyEntity(entity);
            return;
        }

        rand2 = entity.Bytes.GetUInt32();
        entity.Bytes.Set(rand2 - 1);

        if (rand2 == 0)
        {
            entity.Status = 3;

            if (entity.PlatformEntity != null)
            {
                entity.PlatformEntity.CarriedEntity = null;
                entity.PlatformEntity = null;
                //goto LAB_8007aac0; useless entity.PlatformEntity is null
            }
        }
        else
        {
            if ((int)rand2 < 0x3c && (rand2 & 7) == 0)
            {
                effect = gameEngine.EffectManager.CreateEffectEntity(0, 9, 0, 
                    entity.PosX, entity.PosY, entity.PosZ + 0x80000);

                if (effect != null)
                {
                    effect.ForceX = (int)((Random.Next() * 0x30001) >> 0x20) + -0x18000;
                    effect.ForceY = (int)((Random.Next() * 0x20001) >> 0x20) + -0x10000;
                }
            }

            LAB_8007aac0:

            if (entity.PlatformEntity != null)
            {
                if (entity.Flags2 != 0)
                {
                    entity.TargetAnimationId = 2;
                    entity.Flags2 = 0;

                    if (entity.PlatformEntity != null)
                    {
                        entity.PlatformEntity.CarriedEntity = null;
                    }

                    entity.PlatformEntity = null;
                    return;
                }

                goto LAB_8007ac48;
            }
        }

        if (entity.TargetAnimationId == 0)
        {
            return;
        }

        if (entity.ForceAdjusted != 0)
        {
            i = 0;

            if (entity.IsOnGround != 0)
            {
                do
                {
                    if ((entity.MapTiles[i].Flags & 0x1001) == 0x1001)
                    {
                        entity.Status = 3;
                    }

                    i = i + 1;
                } while (i < 4);

                entity.TargetAnimationId = 0;
                return;
            }

            if (entity.ForceZ < 1)
            {
                return;
            }

            if (entity.DelayOrAngleOrEntityId != 0)
            {
                return;
            }

            entity.DelayOrAngleOrEntityId = 1;
            entity.TargetDirection = (entity.TargetDirection + 0x10) & 0x1f;
            gameEngine.EffectManager.CreateEffectEntity(0, 9, 0, entity.PosX, entity.PosY, entity.PosZ + 0x80000);
            return;
        }
        if ((entity.IsOnGround == 0) && (entity.HitCounter == 0))
        {
            return;
        }

        gameEngine.SoundManager.PlaySoundEffect(0x18);
        entity.DelayOrAngleOrEntityId = 0;
        i = 0;

        do
        {
            if ((entity.MapTiles[i].Flags & 0x1001) == 0x1001)
            {
                break;
            }

            i = i + 1;
        } while (i < 4);

        if (i != 4)
        {
            return;
        }

        LAB_8007ac48:
        entity.TargetAnimationId = 0;
    }

    // GHIDRA: AI_FUN_8007ac60 @ 0x8007AC60
    public static void AI_FUN_8007ac60(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Projectile de magie de feu Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        uint countdown = entity.Bytes.GetUInt32();

        if (countdown != 0)
        {
            entity.Bytes.Set((int)(countdown - 1));
            return;
        }

        entity.ForceZ = unchecked((int)0xFFFD0000);
        Entity trackedEntity = null;

        if (entity.DelayOrAngleOrEntityId != 0)
        {
            trackedEntity = gameEngine.StaticVariables.g_entitySlots[entity.DelayOrAngleOrEntityId];

            if (trackedEntity.Status != 2 || trackedEntity.Index2 != entity.ItemState)
            {
                entity.DelayOrAngleOrEntityId = 0;
                return;
            }
        }
        else
        {
            Entity[] matchingEntities = gameEngine.StaticVariables.g_matchingEntitiesBuffer;
            int[] distanceSquared = new int[matchingEntities.Length];
            int matchCount = gameEngine.FUN_8003AF70(entity, 1, entity.BalanceAnimValRef!.Val & 0x0F, matchingEntities, distanceSquared);

            if (matchCount == 0)
            {
                entity.Bytes.Set((int)((Random.Next() * 9UL) >> 32));
                return;
            }

            trackedEntity = matchingEntities[0];
            entity.DelayOrAngleOrEntityId = trackedEntity.Index;
            entity.ItemState = trackedEntity.Index2;
        }

        int direction = ScriptHelper.GetDirectionToTarget(trackedEntity.PosX - entity.PosX, trackedEntity.PosY - entity.PosY);
        int targetDirection = (int)entity.TargetDirection;

        if (targetDirection == direction)
        {
            return;
        }

        if (direction < targetDirection)
        {
            if (targetDirection - direction < 4)
            {
                entity.TargetDirection = (uint)direction;
            }
            else
            {
                entity.TargetDirection = (uint)(targetDirection - 4);
            }
        }
        else if (direction - targetDirection < 4)
        {
            entity.TargetDirection = (uint)direction;
        }
        else
        {
            entity.TargetDirection = (uint)(targetDirection + 4);
        }

        entity.Bytes.Set(2);
    }

    //8007b04c
    public static void AI_FUN_8007b04c(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Boule de fer (petite)")
        {
            Breakpoint.TriggerBreak();
        }

        AI_FUN_8007b04c_common(gameEngine, entity, 0x800, 0x180000);
    }

    private static void AI_FUN_8007b04c_common(GameEngine gameEngine, Entity entity, int factor, int offsetX)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Boule de fer (petite)")
        {
            Breakpoint.TriggerBreak();
        }

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
                entity.PosX = entity.DelayOrAngleOrEntityId + gameEngine.StaticVariables.g_sinus[(ushort)entity.AIValues[1]] * factor;
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
            entity.DelayOrAngleOrEntityId = entity.PosX + offsetX;
            entity.ItemState = entity.PosY;
        }

        entity.AIValues[1] = (short)((entity.AIValues[1] + 4U) & 0x1ff);
        entity.PosX = entity.DelayOrAngleOrEntityId + gameEngine.StaticVariables.g_sinus[(ushort)entity.AIValues[1]] * factor;
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
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Boule de fer (petite)")
        {
            Breakpoint.TriggerBreak();
        }

        AI_FUN_8007b04c_common(gameEngine, entity, 0xc00, 0x240000);
    }

    // 8007B3C4
    public static void AI_FUN_8007b3c4(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Mur à boule de fer (222) – boule de fer")
        {
            Breakpoint.TriggerBreak();
        }

        switch (entity.Bytes[0])
        {
            case 0:
                entity.Bytes[0] = 1;
                entity.AIValues[1] = 0x180;
                entity.DelayOrAngleOrEntityId = entity.PosX - gameEngine.StaticVariables.g_sinus[0x180] * 0x1400;
                entity.ItemState = entity.PosY - gameEngine.StaticVariables.g_cosinus[0x180] * 0x1400;

                entity.AIValues[1] = (short)((entity.AIValues[1] + 4U) & 0x1FF);
                entity.PosX = entity.DelayOrAngleOrEntityId + gameEngine.StaticVariables.g_sinus[(ushort)entity.AIValues[1]] * 0x1400;
                entity.PosY = entity.ItemState + gameEngine.StaticVariables.g_cosinus[(ushort)entity.AIValues[1]] * 0x1400;

                if (gameEngine.GetMatchingEntityBySearchType(entity, entity.EntityRefId - 1) != 0)
                {
                    return;
                }

                entity.Bytes[1] = 2;
                entity.Bytes[0] = (byte)(entity.Bytes[0] + 1);
                gameEngine.SoundManager.PlaySoundEffect(0x78);
                break;

            case 1:
                entity.AIValues[1] = (short)((entity.AIValues[1] + 4U) & 0x1FF);
                entity.PosX = entity.DelayOrAngleOrEntityId + gameEngine.StaticVariables.g_sinus[(ushort)entity.AIValues[1]] * 0x1400;
                entity.PosY = entity.ItemState + gameEngine.StaticVariables.g_cosinus[(ushort)entity.AIValues[1]] * 0x1400;
                entity.Bytes[1] = (byte)(entity.Bytes[1] - 1);

                if (entity.Bytes[1] != 0)
                {
                    return;
                }

                entity.TargetAnimationId = 3;
                entity.Bytes[1] = 0x50;
                entity.Bytes[0] = (byte)(entity.Bytes[0] + 1);
                entity.TargetDirection = (uint)((0x18 - ((ushort)entity.AIValues[1] >> 4)) & 0x1F);
                break;

            case 2:
                entity.Bytes[1] = (byte)(entity.Bytes[1] - 1);

                if (entity.ForceAdjusted == 0 && entity.Bytes[1] != 0)
                {
                    return;
                }

                gameEngine.SoundManager.PlaySoundEffect(0xE1);
                entity.TargetAnimationId = 2;
                entity.Flags |= 0x100;
                entity.Bytes[0] = (byte)(entity.Bytes[0] + 1);
                break;

            case 3:
                int deltaX = entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX;

                if (0x2FFFFF < Math.Abs(deltaX))
                {
                    entity.Flags = (entity.Flags & 0xFFFFFFFDU) | 0x81U;
                    entity.Bytes[0] = (byte)(entity.Bytes[0] + 1);
                    break;
                }

                int deltaY = entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY;

                if (0x1FFFFF < Math.Abs(deltaY))
                {
                    entity.Flags = (entity.Flags & 0xFFFFFFFDU) | 0x81U;
                    entity.Bytes[0] = (byte)(entity.Bytes[0] + 1);
                }
                break;

            case 4:
                if (entity.TargetAnimationId != 4)
                {
                    return;
                }

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[2] < 4)
                {
                    entity.TargetAnimationId = 2;
                    return;
                }

                gameEngine.SoundManager.PlaySoundEffect(0x2E);
                gameEngine.DestroyEntity(entity, 10);
                break;
        }
    }

    // GHIDRA: FUN_80080990 @ 0x80080990
    private static int FUN_80080990(Entity entity1, Entity entity2)
    {
        int entity1Pos = entity1.ModdedPosX;
        int entity2Pos = entity2.ModdedPosX;
        int val;

        if (entity1Pos < entity2Pos)
        {
            val = entity1.Width;
            entity1Pos = entity2Pos - entity1Pos;
        }
        else
        {
            val = entity2.Width;
            entity1Pos = entity1Pos - entity2Pos;
        }

        if (val >= entity1Pos)
        {
            return entity2.PosY < entity1.PosY ? 1 : 0;
        }

        entity1Pos = entity1.ModdedPosY;
        entity2Pos = entity2.ModdedPosY;

        if (entity1Pos < entity2Pos)
        {
            val = entity1.Height;
            entity1Pos = entity2Pos - entity1Pos;
        }
        else
        {
            val = entity2.Height;
            entity1Pos = entity1Pos - entity2Pos;
        }

        if (entity1Pos <= val)
        {
            if (entity1.PosX <= entity2.PosX)
            {
                return 3;
            }

            return 2;
        }

        return -1;
    }

    // GHIDRA: AI_FUN_8007b6ec @ 0x8007B6EC
    public static void AI_FUN_8007b6ec(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Colonne de glace")
        {
            Breakpoint.TriggerBreak();
        }

        var player = gameEngine.StaticVariables.PlayerEntity;

        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.XCollisionEntity == player || player.XCollisionEntity == entity)
                {
                    entity.DelayOrAngleOrEntityId += 1;

                    if (entity.DelayOrAngleOrEntityId != 0x14)
                    {
                        return;
                    }

                    entity.DelayOrAngleOrEntityId = 0;
                    entity.TargetAnimationId = 1;
                    //0: droite
                    //1: gauche
                    //2: bas
                    //3: haut

                    int directionIndex = FUN_80080990(player, entity);
                    entity.TargetDirection = directionIndex == -1
                        ? 0x18U
                        : gameEngine.StaticVariables.BYTE_ARRAY_80028b54[directionIndex];
                    return;
                }

                entity.DelayOrAngleOrEntityId = 0;
                break;

            case 1:
                if (entity.ForceAdjusted != 0)
                {
                    entity.TargetAnimationId = 0;
                }
                break;
        }
    }

    //8007b7b0
    //jar sandboxes
    public static void AI_FUN_8007b7b0(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Torche")
        {
            Breakpoint.TriggerBreak();
        }

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
            entity.TargetAnimationId = (uint)gameEngine.StaticVariables.g_scriptAnimationTable3[entity.Flags2];
        }

        entity.PlatformEntity.CarriedEntity = null;
        entity.PlatformEntity = null;
        entity.Flags = (entity.Flags | 0x34) & 0xffffff7f;//turn off bit 8, turn on bits 5 and 6
    }

    //8007b834
    //Goutte d’eau
    public static void AI_FUN_8007b834(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "Goutte d’eau")
        {
            Breakpoint.TriggerBreak();
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
                if (entity.CollidedWithEntityZ != 0 || entity.IsOnGround != 0)
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
        if (entity.Name != "SaveBook (Ne pas toucher !)"
            && !string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
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
                entity.DelayOrAngleOrEntityId = 0x3C;
                WriteWarpState(entity, state + 1);
                break;

            case 2:
                entity.DelayOrAngleOrEntityId -= 1;

                if (entity.DelayOrAngleOrEntityId != -1)
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

                    entity.DelayOrAngleOrEntityId = 0x3C;
                    WriteWarpState(entity, state + 1);
                    return;
                }

            case 5:
                {
                    entity.DelayOrAngleOrEntityId -= 1;

                    if (entity.DelayOrAngleOrEntityId != -1)
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
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Haricots de Jack")
        {
            Breakpoint.TriggerBreak();
        }

        if (entity.PlatformEntity == null)
        {
            if (entity.IsOnGround != 0)
            {
                entity.Status = 3;
            }
        }
        else if (entity.Flags2 == 0)
        {
            entity.TargetAnimationId = 0;
        }
        else
        {
            entity.TargetAnimationId = 2;
            entity.Flags2 = 0;
            if (entity.PlatformEntity != null)
            {
                entity.PlatformEntity.CarriedEntity = null;
            }
            entity.PlatformEntity = null;
            entity.Flags |= 0x10;
        }
    }

    //8007bb9c
    public static void AI_FUN_8007bb9c(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Pot magique (boïng)")
        {
            Breakpoint.TriggerBreak();
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

    // GHIDRA: AI_FUN_8007bd8c @ 0x8007BD8C
    public static void AI_FUN_8007bd8c(GameEngine gameEngine, Entity entity)
    {
        int word290 = entity.AIValues.GetInt32(8);

        if ((gameEngine.StaticVariables.g_padState1.ButtonsHold & PadState.Circle) == 0)
        {
            word290 -= 1;
            entity.AIValues[8] = (short)word290;
            entity.AIValues[9] = (short)(word290 >> 16);
        }

        if (word290 > 0)
        {
            int word274 = entity.Bytes.GetInt32();
            int word278 = entity.DelayOrAngleOrEntityId;
            int word27c = entity.ItemState;
            int word280 = entity.AIValues.GetInt32(0);
            int word284 = entity.AIValues.GetInt32(2);
            int word288 = entity.AIValues.GetInt32(4);
            int selectedWord = word284 == 0 ? word274 : word278;
            int compareWord = word288 < word274 ? 1 : 0;

            if (selectedWord < word288)
            {
                word288 -= word27c;

                if (!(selectedWord < word288))
                {
                    word288 = selectedWord;
                    word284 ^= 1;
                    entity.AIValues[2] = (short)word284;
                    entity.AIValues[3] = (short)(word284 >> 16);
                }
            }
            else
            {
                word288 += word27c + (compareWord << 6);

                if (!(word288 < selectedWord))
                {
                    word288 = selectedWord;
                    word284 ^= 1;
                    entity.AIValues[2] = (short)word284;
                    entity.AIValues[3] = (short)(word284 >> 16);
                }
            }

            entity.AIValues[4] = (short)word288;
            entity.AIValues[5] = (short)(word288 >> 16);

            int word28c = (entity.AIValues.GetInt32(6) + word280) & 0xff;
            entity.AIValues[6] = (short)word28c;
            entity.AIValues[7] = (short)(word28c >> 16);

            Entity player = gameEngine.StaticVariables.PlayerEntity;
            int forceX = gameEngine.StaticVariables.g_sinTable[word28c] * word288 * 3;
            int forceY = gameEngine.StaticVariables.g_cosTable[word28c] * word288 * 3;

            entity.PreviousAdjustedForceX = player.PosX + player.ForceX + forceX - entity.PosX;
            entity.PreviousAdjustedForceY = player.PosY + player.ForceY + forceY - entity.PosY;
            entity.ForceZ = player.PosZ + 0x00080000 - entity.PosZ;
            return;
        }

        if (word290 == 0)
        {
            Entity[] matchingEntities = gameEngine.StaticVariables.g_matchingEntitiesBuffer;
            int[] distanceSquared = new int[matchingEntities.Length];
            int matchCount = gameEngine.FUN_8003AF70(entity, 1, entity.BalanceAnimValRef!.Val & 0x0F, matchingEntities, distanceSquared);
            int direction;

            if (matchCount != 0 && distanceSquared[0] < 0x3840)
            {
                Entity trackedEntity = matchingEntities[0];
                direction = ScriptHelper.GetDirectionToTarget(trackedEntity.PosX - entity.PosX, trackedEntity.PosY - entity.PosY);
            }
            else
            {
                direction = (int)((Random.Next() * 0x20UL) >> 32);
            }

            entity.Bytes.Set(gameEngine.StaticVariables.g_offsetXList[direction] << 8);
            entity.DelayOrAngleOrEntityId = gameEngine.StaticVariables.g_offsetYList[direction] << 8;
            entity.Flags |= 0x2130;
            entity.ForceZ = 0x000A0000;
        }

        entity.PreviousAdjustedForceX = entity.Bytes.GetInt32();
        entity.PreviousAdjustedForceY = entity.DelayOrAngleOrEntityId;
    }

    //8007c024
    public static void AI_UpdatePushablePillarPushState(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Pilier poussable (PRG)")
        {
            Breakpoint.TriggerBreak();
        }

        if (gameEngine.StaticVariables.PlayerEntity.XCollisionEntity == entity)
        {
            if (entity.Bytes[0] < 0x1f)
            {
                entity.Bytes[0] = (byte)(entity.Bytes[0] + 1);
                return;
            }

            gameEngine.SoundManager.PlaySoundEffect(0x19);
            entity.TargetAnimationId = 1;
            var result = GetPushablePillarPushDirection(entity, gameEngine.StaticVariables.PlayerEntity);

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
        entity.TargetAnimationId = 0;
    }

    //8003ac9c
    private static int GetPushablePillarPushDirection(Entity entity1, Entity entity2)
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
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

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
    public static void AI_FUN_8007c768(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆P-Zoldia Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        var player = gameEngine.StaticVariables.PlayerEntity;

        if (entity.Bytes[0] == 1 && gameEngine.StaticVariables.PTR_801912e8!.Bytes[1] == 2)
        {
            var targetAnimationId = entity.TargetAnimationId;

            if (targetAnimationId == 8)
            {
                gameEngine.DestroyEntity(entity);
                targetAnimationId = entity.TargetAnimationId;
            }

            if (targetAnimationId == 0 || targetAnimationId == 5)
            {
                gameEngine.SoundManager.PlaySoundEffect(0x120);
                entity.TargetAnimationId = 4;
            }
        }

        switch (entity.TargetAnimationId)
        {
            case 0:
                if ((ushort)entity.AIValues[1] != 0)
                {
                    SetAiValue(entity, 1, entity.AIValues[1] - 1);
                    break;
                }

                switch (entity.Bytes[1])
                {
                    case 0:
                        gameEngine.SoundManager.PlaySoundEffect(0x120);
                        entity.TargetAnimationId = 4;
                        break;

                    case 3:
                        entity.TargetDirection = gameEngine.StaticVariables.BYTE_ARRAY_80028b54[RandomRange(4)];
                        goto case 4;

                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    {
                        var randomSide = RandomRange(2);
                        if (entity.TargetDirection == 0 || entity.TargetDirection == 0x10)
                        {
                            entity.TargetDirection = randomSide == 0 ? 8U : 0x18U;
                        }
                        else
                        {
                            entity.TargetDirection = randomSide == 0 ? 0x10U : 0U;
                        }

                        entity.Bytes[1] = (byte)(entity.Bytes[1] + 1);
                        gameEngine.SoundManager.PlaySoundEffect(0x11f);
                        entity.TargetAnimationId = 0xf;
                        entity.AIValues[1] = 0x1e;
                        break;
                    }

                    case 8:
                        entity.Bytes[1] = 0;
                        break;
                }
                break;

            case 3:
            case 5:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0;
                }
                break;

            case 4:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[0] != 0)
                    {
                        gameEngine.DestroyEntity(entity);
                    }
                    else
                    {
                        entity.TargetAnimationId = 8;
                        entity.AIValues[1] = 0x78;
                        entity.Bytes[1] = 10;
                    }
                }
                break;

            case 7:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[1] == 1)
                    {
                        entity.Bytes[1] = 2;
                    }

                    if (entity.Bytes[3] == 0)
                    {
                        gameEngine.SoundManager.PlaySoundEffect(0x120);
                        entity.TargetAnimationId = 4;
                    }
                    else
                    {
                        entity.ContentsItemId = 0;
                        entity.TargetAnimationId = 2;
                        entity.Flags |= 0x40;
                    }
                }
                break;

            case 8:
                if ((ushort)entity.AIValues[1] != 0)
                {
                    SetAiValue(entity, 1, entity.AIValues[1] - 1);
                    break;
                }

                if ((uint)(entity.Bytes[0] - 2) < 2U)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x121);
                    entity.TargetAnimationId = 0xd;

                    var spawned = gameEngine.SpawnWarpEntity(entity, 1, 0xc4, entity.PosX, entity.PosY + 0x10000, entity.PosZ, 0);
                    if (spawned != null)
                    {
                        spawned.TargetAnimationId = 0xc;
                        spawned.ContentsItemId = 0;
                        spawned.SpriteProgramIndexes[ScriptHelper.ProgramCTick] = 0x4e;
                    }
                }
                else if (entity.Bytes[1] == 1)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x122);
                    entity.TargetAnimationId = 5;
                }
                else if (entity.Bytes[1] == 9 && gameEngine.StaticVariables.DAT_80191300 == 0)
                {
                    entity.Bytes[1] = 0;
                    entity.AIValues[1] = 0x78;
                }
                else if (entity.Bytes[1] == 10)
                {
                    entity.Bytes[1] = 9;
                    gameEngine.StaticVariables.DAT_80191300 = 0;

                    if (RandomRange(3) == 0)
                    {
                        var directionGroupIndex = RandomRange(2) * 3 + 24;
                        for (var index = 0; index < 3; index++)
                        {
                            var spawned = gameEngine.SpawnWarpEntity(entity, 1, 0xc4,
                                player.PosX, player.PosY, player.FloorHeight,
                                gameEngine.StaticVariables.BYTE_ARRAY_80028230[directionGroupIndex + index]);

                            if (spawned != null)
                            {
                                SetPZoldiaSpawnDimensions(gameEngine, spawned, player);
                                spawned.TargetAnimationId = 9;
                                spawned.SpriteProgramIndexes[ScriptHelper.ProgramCTick] = 0x4e;
                                spawned.AIValues[1] = 0x1e;
                                spawned.Bytes[0] = 3;
                                spawned.ContentsItemId = 0;
                                spawned.PosZ = player.FloorHeight;
                                spawned.Flags |= 0x2000;
                                gameEngine.StaticVariables.DAT_80191300++;
                            }
                        }
                    }
                    else
                    {
                        var delay = 10;
                        for (var index = 0; index < 3; index++)
                        {
                            var randomX = Random.Next();
                            var randomY = Random.Next();
                            var spawned = gameEngine.SpawnWarpEntity(entity, 1, 0xc4,
                                RandomRange(randomX, 7) * 0x180000 + 0xc00000,
                                RandomRange(randomY, 7) * 0x100000 + 0xb00000,
                                entity.PosZ, 0);

                            if (spawned != null)
                            {
                                spawned.TargetAnimationId = 8;
                                spawned.SpriteProgramIndexes[ScriptHelper.ProgramCTick] = 0x4e;
                                spawned.Bytes[0] = 2;
                                spawned.AIValues[1] = (short)delay;
                                spawned.ContentsItemId = 0;
                                gameEngine.StaticVariables.DAT_80191300++;
                                spawned.Flags |= 0x2000;
                            }

                            delay += 0x28;
                        }
                    }
                }
                else if (entity.Bytes[1] == 0)
                {
                    if (RandomRange(2) == 0)
                    {
                        gameEngine.StaticVariables.DAT_801912f4 = 0;
                        gameEngine.StaticVariables.DAT_801912f8 = 0x2800;
                        entity.Bytes[2] = 0;
                        entity.AIValues[4] = 0;

                        gameEngine.StaticVariables.DAT_801912fc = RandomRange(8);
                        gameEngine.StaticVariables.DAT_80191304 = RandomRange(3);
                        gameEngine.StaticVariables.PTR_801912e8 = entity;
                        gameEngine.StaticVariables.DAT_80191300 = 0;
                        entity.ItemState = RandomRange(2) + 1;

                        do
                        {
                            var tableIndex = gameEngine.StaticVariables.DAT_80191304 * 8 + gameEngine.StaticVariables.DAT_80191300;
                            var angle = (int)(gameEngine.StaticVariables.BYTE_ARRAY_80028230[tableIndex] & 0x1ff);

                            if (gameEngine.StaticVariables.DAT_801912fc == gameEngine.StaticVariables.DAT_80191300)
                            {
                                entity.AIValues[1] = (short)(gameEngine.StaticVariables.DAT_801912fc << 4);
                                entity.Bytes[1] = 1;
                                entity.DelayOrAngleOrEntityId = angle;
                            }
                            else
                            {
                                var spawned = gameEngine.SpawnWarpEntity(entity, 1, 0xc4, entity.PosX, entity.PosY, entity.PosZ, 0);
                                spawned.Bytes[0] = 1;
                                spawned.Bytes[1] = 1;
                                spawned.TargetAnimationId = 8;
                                spawned.SpriteProgramIndexes[ScriptHelper.ProgramCTick] = 0x4e;
                                spawned.SpriteProgramIndexes[ScriptHelper.ProgramDTouch] = 0x23;
                                spawned.AIValues[1] = (short)(gameEngine.StaticVariables.DAT_80191300 << 4);
                                spawned.DelayOrAngleOrEntityId = angle;
                            }

                            gameEngine.StaticVariables.DAT_80191300++;
                        } while (gameEngine.StaticVariables.DAT_80191300 != 8);

                        gameEngine.StaticVariables.DAT_80191300 = 8;
                    }
                    else
                    {
                        var randomX = Random.Next();
                        var randomY = Random.Next();
                        entity.PosX = RandomRange(randomX, 5) * 0x180000 + 0xd80000;
                        entity.PosY = RandomRange(randomY, 5) * 0x100000 + 0xc00000;
                        gameEngine.SoundManager.PlaySoundEffect(0x122);
                        entity.TargetAnimationId = 5;
                        entity.TargetDirection = 0;
                        entity.Bytes[1] = 3;
                        entity.AIValues[1] = 2;
                    }
                }
                break;

            case 9:
                if ((uint)(entity.Bytes[0] - 2) < 2U)
                {
                    var timer = (ushort)(entity.AIValues[1] - 1);
                    entity.AIValues[1] = unchecked((short)timer);

                    if (timer == 0)
                    {
                        entity.TargetAnimationId = 8;
                        entity.AIValues[1] = 1;
                        entity.TargetDirection = entity.TargetDirection + 0x10 & 0x1f;
                    }
                    else if (entity.ForceAdjusted != 0)
                    {
                        gameEngine.DestroyEntity(entity);
                        gameEngine.StaticVariables.DAT_80191300--;
                    }
                }
                break;

            case 0xb:
            case 0xc:
            case 0xe:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    gameEngine.DestroyEntity(entity);
                }
                break;

            case 0xd:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0x10;
                    if (entity.Bytes[0] == 2)
                    {
                        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                    }
                }
                break;

            case 0xf:
            {
                var timer = (ushort)(entity.AIValues[1] - 1);
                entity.AIValues[1] = unchecked((short)timer);

                if ((timer & 3) == 0)
                {
                    var spawned = gameEngine.SpawnWarpEntity(entity, 1, 0xc4,
                        entity.PosX, entity.PosY, entity.PosZ, entity.TargetDirection);

                    if (spawned != null)
                    {
                        spawned.TargetAnimationId = 0xb;
                        spawned.SpriteProgramIndexes[ScriptHelper.ProgramCTick] = 0x4e;
                    }
                }

                if ((ushort)entity.AIValues[1] == 0 || entity.ForceAdjusted != 0)
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 10;
                }
                break;
            }

            case 0x10:
            {
                var timer = (ushort)(entity.AIValues[1] + 1);
                entity.AIValues[1] = unchecked((short)timer);

                if ((timer & 7) == 0)
                {
                    var spawned = gameEngine.SpawnWarpEntity(entity, 1, 0xc4,
                        entity.PosX, entity.PosY, entity.PosZ, entity.TargetDirection);

                    if (spawned != null)
                    {
                        spawned.TargetAnimationId = 0xe;
                        spawned.SpriteProgramIndexes[ScriptHelper.ProgramCTick] = 0x4e;
                        SetPZoldiaSpawnDimensions(gameEngine, spawned, player);
                        spawned.PosZ = player.FloorHeight;
                    }
                }

                if (entity.ForceAdjusted != 0 || player.TouchingEntity == entity)
                {
                    gameEngine.StaticVariables.DAT_80191300--;
                    gameEngine.DestroyEntity(entity, -1);
                }
                break;
            }
        }

        if ((uint)(entity.Bytes[1] - 1) >= 2U)
        {
            return;
        }

        var controller = gameEngine.StaticVariables.PTR_801912e8!;
        if ((ushort)controller.AIValues[4] == 3)
        {
            gameEngine.SoundManager.PlaySoundEffect(0x120);
            entity.TargetAnimationId = 4;
        }

        if (controller == entity)
        {
            gameEngine.StaticVariables.DAT_801912f4 = gameEngine.StaticVariables.DAT_801912f4 - 4 & 0x1ff;
            if (gameEngine.StaticVariables.DAT_801912f4 == 0)
            {
                SetAiValue(entity, 4, entity.AIValues[4] + 1);
            }

            gameEngine.StaticVariables.DAT_801912ec = gameEngine.StaticVariables.g_sinus[gameEngine.StaticVariables.DAT_801912f4] * 0x800 + 0x1080000;
            gameEngine.StaticVariables.DAT_801912f0 = gameEngine.StaticVariables.g_cosinus[gameEngine.StaticVariables.DAT_801912f4] * 0x800 + 0xe00000;

            if (entity.Bytes[2] == 0)
            {
                if ((ushort)entity.AIValues[4] == entity.ItemState)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x122);
                    entity.Bytes[2] = 1;
                }

                if (entity.Bytes[2] == 0)
                {
                    UpdatePZoldiaOrbitPosition(gameEngine.StaticVariables, entity);
                    return;
                }
            }

            if (entity.Bytes[2] == 1)
            {
                gameEngine.StaticVariables.DAT_801912f8 -= 0x80;
            }
            else
            {
                gameEngine.StaticVariables.DAT_801912f8 += 0x80;
            }

            if (gameEngine.StaticVariables.DAT_801912f8 == 0x1200)
            {
                entity.Bytes[2] = 2;
            }

            if (gameEngine.StaticVariables.DAT_801912f8 == 0x2800)
            {
                entity.Bytes[2] = 0;
            }
        }

        UpdatePZoldiaOrbitPosition(gameEngine.StaticVariables, entity);
    }

    private static int RandomRange(uint exclusiveMax)
    {
        return (int)((Random.Next() * exclusiveMax) >> 32);
    }

    private static int RandomRange(ulong seed, uint exclusiveMax)
    {
        return (int)((seed * exclusiveMax) >> 32);
    }

    private static void SetAiValue(Entity entity, int index, int value)
    {
        entity.AIValues[index] = unchecked((short)(ushort)value);
    }

    private static void SetPZoldiaSpawnDimensions(GameEngine gameEngine, Entity spawned, Entity player)
    {
        var playerHeader = player.SpriteRecord!.Header;
        var spawnedHeader = spawned.SpriteRecord!.Header;
        gameEngine.EntityManager.SetEntityDimensions(spawned,
            playerHeader.OffsetX, playerHeader.OffsetY, spawnedHeader.OffsetZ,
            playerHeader.SizeX, playerHeader.SizeY, spawnedHeader.SizeZ);
    }

    private static void UpdatePZoldiaOrbitPosition(StaticVariables staticVariables, Entity entity)
    {
        entity.DelayOrAngleOrEntityId = entity.DelayOrAngleOrEntityId + 2 & 0x1ff;
        var radius = staticVariables.DAT_801912f8;
        entity.PosX = staticVariables.DAT_801912ec + staticVariables.g_sinus[entity.DelayOrAngleOrEntityId] * radius;
        entity.PosY = staticVariables.DAT_801912f0 + staticVariables.g_cosinus[entity.DelayOrAngleOrEntityId] * radius;

        if (entity.TargetAnimationId != 0 && entity.TargetAnimationId != 8)
        {
            return;
        }

        var sector = entity.DelayOrAngleOrEntityId >> 4;
        if ((uint)(sector - 4) <= 7U)
        {
            entity.TargetDirection = 8;
        }
        else if ((uint)(sector - 0xc) < 8U)
        {
            entity.TargetDirection = 0;
        }
        else if ((uint)(sector - 0x14) <= 7U)
        {
            entity.TargetDirection = 0x18;
        }
        else
        {
            entity.TargetDirection = 0x10;
        }
    }

    // GHIDRA: AI_FUN_8007d554 @ 0x8007D554
    public static void AI_FUN_8007d554(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

        var player = gameEngine.StaticVariables.PlayerEntity;
        var parentEntity = entity.ParentEntity!;

        if ((ushort)parentEntity.AIValues[4] != 0)
        {
            parentEntity.AIValues[4] = 0;
            entity.TargetAnimationId = 3;
        }

        parentEntity.PosX = entity.PosX;
        parentEntity.PosY = entity.PosY - 0x00100000;

        if (entity.Bytes[0] != parentEntity.Bytes[2])
        {
            parentEntity.PosZ -= 0x8000;
            parentEntity.Bytes[2] = (byte)(parentEntity.Bytes[2] + 1);
        }
        else if (parentEntity.Bytes[1] != 0)
        {
            parentEntity.Bytes[1] = (byte)(parentEntity.Bytes[1] - 1);
        }
        else if (parentEntity.PosZ != entity.PosZ + 0x00300000)
        {
            parentEntity.PosZ += 0x8000;
            entity.Bytes[0] = (byte)(entity.Bytes[0] - 1);
            parentEntity.Bytes[2] = (byte)(parentEntity.Bytes[2] - 1);
        }

        if (entity.Bytes[3] != 0 && entity.TargetAnimationId == 0)
        {
            if (entity.Bytes[3] < 4)
            {
                AI_Melzas2.UpdateEntityAI_BossExplode(gameEngine, entity);
                return;
            }

            entity.TargetAnimationId = 4;
            entity.Flags |= 0x40;
            gameEngine.DestroyEntity(entity);
            entity.SpriteProgramIndexes[ScriptHelper.ProgramCTick] = 0;
            return;
        }

        switch (entity.TargetAnimationId)
        {
            case 0:
            {
                Entity? matchingEntity = null;

                for (var index = 0; index != 0x40; index++)
                {
                    var candidate = gameEngine.StaticVariables.g_entitySlots[index];

                    if ((uint)(candidate.Status - 2) >= 2U
                        || candidate.BlockedByEntity != null
                        || candidate == entity
                        || candidate.SpriteTableIndex != 0x1D8)
                    {
                        continue;
                    }

                    var diffX = candidate.ModdedPosX - entity.ModdedPosX;
                    if (diffX >= 0)
                    {
                        if (!(diffX < entity.Width + 1))
                        {
                            continue;
                        }
                    }
                    else if (!(entity.ModdedPosX - candidate.ModdedPosX < candidate.Width + 1))
                    {
                        continue;
                    }

                    var diffY = candidate.ModdedPosY - entity.ModdedPosY;
                    if (diffY >= 0)
                    {
                        if (!(diffY < entity.Height + 1))
                        {
                            continue;
                        }
                    }
                    else if (!(entity.ModdedPosY - candidate.ModdedPosY < candidate.Height + 1))
                    {
                        continue;
                    }

                    matchingEntity = candidate;
                    break;
                }

                if (matchingEntity != null)
                {
                    entity.TargetAnimationId = 1;
                    entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(
                        entity.PosX - matchingEntity.PosX,
                        entity.PosY - matchingEntity.PosY);
                    entity.AIValues[1] = 0x1E;
                    break;
                }

                if ((ushort)entity.AIValues[1] != 0)
                {
                    SetAiValue(entity, 1, entity.AIValues[1] - 1);
                    break;
                }

                gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x3C, 0x46);

                if (entity.Bytes[0] != 0)
                {
                    entity.Bytes[0] = 0;
                    entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(
                        player.PosX - entity.PosX,
                        player.PosY - entity.PosY);
                }
                break;
            }

            case 1:
                if ((ushort)entity.AIValues[1] != 0)
                {
                    SetAiValue(entity, 1, entity.AIValues[1] - 1);
                }
                else
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = (short)(RandomRange(0x1FU) + 0x3C);
                }
                break;

            case 2:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0;
                }
                break;

            case 3:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0;

                    if (parentEntity.Bytes[3] != 0)
                    {
                        entity.AIValues[1] = 0x12C;
                        entity.AIValues[5] = 0x1E;
                        entity.Bytes[3] = 1;
                        entity.Flags &= 0xFFFFFFFCU;
                    }
                    else
                    {
                        parentEntity.TargetAnimationId = 0;
                        entity.DamagedTickCounter = 0x5A;
                        parentEntity.DamagedTickCounter = 0x5A;
                        entity.Bytes[0] = 1;
                        entity.AIValues[1] = (short)(RandomRange(0x1FU) + 0x14);
                    }
                }
                break;

            case 4:
                break;

            case 5:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0;
                    parentEntity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0x3C;
                }
                break;
        }
    }

    //8006a564
    public static void AI_FUN_8006a564(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Guêpe Niv.1")
        {
            Breakpoint.TriggerBreak();
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
                            if (positions[5] < 0x180001 || entity.IsOnGround != 0)
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

                if (sVar3 == 0 || entity.IsOnGround != 0)
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

    // GHIDRA: FUN_8007fe8c @ 0x8007FE8C
    private static void FUN_8007fe8c(GameEngine gameEngine, Entity entity, int[] positions)
    {
        int x;
        int z;
        int y;

        x = entity.TileX - gameEngine.StaticVariables.PlayerEntity.TileX;
        y = entity.TileY - gameEngine.StaticVariables.PlayerEntity.TileY;
        z = entity.FloorHeight - gameEngine.StaticVariables.PlayerEntity.FloorHeight;
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
        if (entity.Name != "◆Homme momie Niv.1"
            && entity.Name != "◆Homme momie Niv.2")
        {
            Breakpoint.TriggerBreak();
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
                if (entity.IsOnGround == 0)
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
                entity.DelayOrAngleOrEntityId = 0;

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
                        entity.DelayOrAngleOrEntityId = 0;
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
                    entity.DelayOrAngleOrEntityId = 1;
                }

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.DelayOrAngleOrEntityId != 0)
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0x1e;
                    entity.DelayOrAngleOrEntityId = 0;
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
    public static void AI_UpdateEntityAI_3(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Mimique Niv.1")
        {
            Breakpoint.TriggerBreak();
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
                if (entity.IsOnGround != 0)
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

    // GHIDRA: AI_UpdateEntityAI_4 @ 0x80067138
    public static void AI_UpdateEntityAI_4(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Homme-insecte Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        short val;
        uint direction;
        int z;
        int[] positions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, positions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.Bytes[1] == 0)
                {
                    entity.Bytes[1] = 10;
                }

                if (entity.AIValues[1] != 0)
                {
                    val = (short)(entity.AIValues[1] - 1);
                    entity.AIValues[1] = val;

                    if (val != 0)
                    {
                        return;
                    }
                }

                if (entity.Bytes[1] == 10
                    && gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, positions, 1, 2, 0))
                {
                    direction = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.TargetAnimationId = 3;
                    entity.Bytes[1] = 0;
                    return;
                }

                if ((uint)(entity.Bytes[1] - 10) >= 7
                    && entity.Bytes[1] != 0x14
                    && entity.Bytes[1] != 0x15)
                {
                    return;
                }

                if (gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, positions, 2, 6, 0))
                {
                    direction = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.TargetAnimationId = 1;
                    entity.AIValues[1] = 0x78;
                    return;
                }

                entity.Bytes[1] = (byte)(entity.Bytes[1] + 1);

                switch (entity.Bytes[1])
                {
                    case 0x0b:
                    case 0x0e:
                        entity.AIValues[1] = 0x28;
                        entity.ForceStepY = 0;
                        entity.ForceStepX = 0;
                        entity.ForceY = 0;
                        entity.ForceX = 0;
                        entity.TargetForceY = 0;
                        entity.TargetForceX = 0;
                        entity.TargetDirection = (uint)((gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection] - 8) & 0x1f);
                        break;

                    case 0x0c:
                    case 0x0f:
                    case 0x15:
                        entity.AIValues[1] = 0x28;
                        entity.ForceStepY = 0;
                        entity.ForceStepX = 0;
                        entity.ForceY = 0;
                        entity.ForceX = 0;
                        entity.TargetForceY = 0;
                        entity.TargetForceX = 0;
                        entity.TargetDirection = (uint)((gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection] + 0x10) & 0x1f);
                        break;

                    case 0x0d:
                        gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x78, 0x1e);
                        break;

                    case 0x10:
                    case 0x16:
                        entity.TargetAnimationId = 5;
                        break;
                }
                break;

            case 1:
                val = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = val;

                if (val == 0)
                {
                    entity.TargetAnimationId = 0;
                    return;
                }

                if (entity.ForceAdjusted != 0)
                {
                    direction = gameEngine.StaticVariables.g_directionFlipTable[entity.TargetDirection];
                    entity.ForceStepY = 0;
                    entity.ForceStepX = 0;
                    entity.ForceY = 0;
                    entity.ForceX = 0;
                    entity.TargetForceY = 0;
                    entity.TargetForceX = 0;
                    entity.TargetDirection = direction;
                }
                else
                {
                    z = entity.FloorHeight - gameEngine.StaticVariables.PlayerEntity.FloorHeight;

                    if (z >= 0)
                    {
                        gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 1, z);
                    }
                }

                if (entity.Bytes[1] != 0)
                {
                    return;
                }

                if (gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, positions, 1, 2, 0))
                {
                    direction = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.TargetAnimationId = 3;
                    entity.Bytes[1] = 0;
                }
                break;

            case 2:
                if (entity.AIValues[1] != 0)
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                }

                if (entity.Bytes[1] != 0)
                {
                    return;
                }

                if (positions[0] < 3 && positions[1] < 3 && positions[2] <= 0)
                {
                    entity.TargetAnimationId = 4;
                    entity.Bytes[1] = 10;
                    return;
                }

                if (entity.AIValues[1] == 0)
                {
                    entity.TargetAnimationId = 4;
                    entity.Bytes[1] = 0x14;
                }
                break;

            case 3:
                if (gameEngine.StaticVariables.PlayerEntity.TouchingEntity == entity)
                {
                    entity.Bytes[1] = (byte)(entity.Bytes[1] + 1);
                }

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if ((entity.Bytes[1] & 0xf) != 0)
                {
                    direction = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.TargetAnimationId = 0;
                    entity.Bytes[1] = 10;
                    entity.AIValues[1] = (short)(((Random.Next() * 8) >> 0x20) + 0x14);
                    return;
                }

                if (gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, positions, 1, 1, 0))
                {
                    direction = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;

                    if (entity.Bytes[1] == 0)
                    {
                        entity.TargetAnimationId = 3;
                        entity.Bytes[1] = 0x80;
                    }
                    else
                    {
                        entity.TargetAnimationId = 1;
                        entity.Bytes[1] = 0;
                        entity.AIValues[1] = 0x14;
                    }

                    return;
                }

                entity.TargetAnimationId = 0;
                entity.Bytes[1] = 10;
                entity.AIValues[1] = (short)(((Random.Next() * 0x10) >> 0x20) + 0x14);
                break;

            case 4:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[1] != 10 && entity.Bytes[1] != 0x14)
                {
                    return;
                }

                direction = (uint)ScriptHelper.GetDirectionToTarget(
                    gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                    gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                entity.TargetDirection = direction;
                entity.TargetAnimationId = 0;
                entity.AIValues[1] = (short)(((Random.Next() * 0x10) >> 0x20) + 0x14);
                break;

            case 5:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 2;
                    entity.Bytes[1] = 0;
                    entity.AIValues[1] = 0x78;
                }
                break;

            case 6:
            case 9:
                break;

            case 7:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[3] != 0)
                {
                    entity.TargetAnimationId = 8;
                    entity.AIValues[1] = 8;
                    entity.Flags |= 0x40;
                    return;
                }

                if ((int)((Random.Next() * 4) >> 0x20) == 1)
                {
                    entity.TargetAnimationId = 5;
                    return;
                }

                entity.Bytes[1] = 0;
                entity.AIValues[1] = 0;
                entity.TargetAnimationId = 0;
                direction = gameEngine.StaticVariables.BYTE_ARRAY_80028b54[(int)((Random.Next() * 4) >> 0x20)];

                var entitySpawned = gameEngine.SpawnWarpEntity(entity, 1, 0x91,
                    entity.PosX, entity.PosY, entity.PosZ + 0x1000000, direction);

                if (entitySpawned != null)
                {
                    entitySpawned.SpriteProgramIndexes[0] = 0;
                    entitySpawned.TargetAnimationId = 0xb;
                    entitySpawned.ContentsItemId = 0;
                }
                break;

            case 8:
                val = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = val;

                if (val != 0)
                {
                    return;
                }

                direction = 4;

                for (int i = 0; i < 4; i++, direction += 8)
                {
                    entitySpawned = gameEngine.SpawnWarpEntity(entity, 1, 0x91,
                        entity.PosX, entity.PosY, entity.PosZ + 0x1000000, direction);

                    if (entitySpawned != null)
                    {
                        entitySpawned.SpriteProgramIndexes[0] = 0;
                        entitySpawned.TargetAnimationId = 0xb;
                        entitySpawned.ContentsItemId = 0;
                    }
                }
                break;

            case 10:
                val = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = val;

                if (val == 0)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x40);
                    gameEngine.DestroyEntity(entity, -1);
                }
                break;

            case 11:
                if (entity.IsOnGround != 0)
                {
                    entity.TargetAnimationId = 10;
                    entity.AIValues[1] = 0x3c;
                }
                break;
        }
    }

    // GHIDRA: AI_UpdateEntityAI_5 @ 0x8006790C
    public static void AI_UpdateEntityAI_5(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Tête de roche Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        short val;
        uint direction;
        int[] positions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, positions);

        switch (entity.TargetAnimationId)
        {
            case 0:
            case 1:
                val = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = val;

                if (val == 0)
                {
                    entity.TargetAnimationId = 4;
                    break;
                }

                if (positions[0] < 3 && positions[1] < 3 && positions[2] <= 0x100000)
                {
                    entity.TargetAnimationId = 5;
                    entity.Bytes[0] = 0;
                    entity.AIValues[1] = 0x5e;
                    entity.Flags |= 0x40;
                    break;
                }

                if (entity.ForceAdjusted != 0)
                {
                    direction = gameEngine.StaticVariables.g_directionFlipTable[entity.TargetDirection];
                    entity.ForceStepY = 0;
                    entity.ForceStepX = 0;
                    entity.ForceY = 0;
                    entity.ForceX = 0;
                    entity.TargetForceY = 0;
                    entity.TargetForceX = 0;
                    entity.TargetDirection = direction;
                }
                else
                {
                    gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 1, 0);
                }
                break;

            case 2:
                if (entity.AIValues[1] == 0)
                {
                    entity.TargetAnimationId = 3;
                    break;
                }

                entity.AIValues[1] = (short)(entity.AIValues[1] - 1);

                if (positions[0] < 5 && positions[1] < 5 && positions[2] <= 0x100000)
                {
                    entity.TargetAnimationId = 3;
                    entity.Bytes[1] = 3;
                }
                else
                {
                    entity.Bytes[1] = 0;
                }
                break;

            case 3:
                if (entity.IsOnGround != 0)
                {
                    if (entity.Bytes[1] == 3)
                    {
                        entity.TargetAnimationId = 0xb;
                        entity.AIValues[1] = 0x28;
                        entity.Bytes[1] = 0;
                    }
                    else
                    {
                        gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x78, 0x1e);
                    }
                }
                break;

            case 4:
                if (entity.IsOnGround != 0)
                {
                    entity.TargetAnimationId = 2;
                    entity.AIValues[1] = (short)(((Random.Next() * 0x15) >> 0x20) + 0x78);
                }
                break;

            case 5:
                if (entity.Bytes[0] != 0)
                {
                    return;
                }

                val = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = val;

                if (val != 0)
                {
                    return;
                }

                entity.Bytes[0] = 1;
                gameEngine.SoundManager.PlaySoundEffect(0x2d);
                direction = 4;

                for (int i = 0; i < 4; i++, direction += 8)
                {
                    var entitySpawned = gameEngine.SpawnWarpEntity(entity, 1, 0x8b,
                        entity.PosX, entity.PosY, entity.PosZ + 0x100000, direction);

                    if (entitySpawned != null)
                    {
                        entitySpawned.SpriteProgramIndexes[0] = 0;
                        entitySpawned.TargetAnimationId = 10;
                        entitySpawned.ContentsItemId = 0;
                    }
                }
                break;

            case 6:
                direction = (uint)(entity.AIValues[6] + 1);
                entity.AIValues[6] = (short)direction;

                if ((direction & 7) == 0)
                {
                    gameEngine.EffectManager.CreateEffectEntity(0, gameEngine.CurrentMap.Info.SlideEffectId, 0,
                        entity.PosX, entity.PosY, entity.FloorHeight);
                }

                val = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = val;

                if (val == 0)
                {
                    entity.TargetAnimationId = 4;
                }
                break;

            case 7:
            case 9:
                break;

            case 8:
                if (entity.IsOnGround == 0)
                {
                    return;
                }

                if (entity.Bytes[3] != 0)
                {
                    entity.TargetAnimationId = 9;
                    entity.Flags |= 0x40;
                    return;
                }

                if ((int)((Random.Next() * 5) >> 0x20) == 2)
                {
                    entity.TargetAnimationId = 5;
                    entity.Bytes[0] = 0;
                    entity.AIValues[1] = 0x5e;
                    entity.Flags |= 0x40;
                }
                else
                {
                    entity.TargetAnimationId = 4;
                    entity.AIValues[1] = 0x1e;
                }
                break;

            case 10:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    gameEngine.DestroyEntity(entity, -1);
                }
                break;

            case 11:
                val = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = val;

                if (val == 0)
                {
                    entity.TargetAnimationId = 6;
                    direction = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.AIValues[1] = 0x78;
                    entity.AIValues[6] = 0;
                    entity.AIValues[7] = 0;
                }
                break;
        }
    }

    //80067d98
    public static void AI_UpdateEntityAI_6(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Fantôme Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        byte bVar1;
        bool bVar2;
        uint uVar3;
        int z;
        int[] relativePositions = new int[6];
        short val;

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                val = entity.AIValues[1];
                entity.Bytes[0] = 0;

                if (val == 0)
                {
                    entity.AIValues[1] = 0x78;
                }

                val = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = val;

                if (val == 0)
                {
                    if (gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 1, 3, 0x800000))
                    {
                        z = entity.PosZ - gameEngine.StaticVariables.PlayerEntity.FloorHeight;
                        
                        if (0x100000 < z)
                        {
                            entity.TargetAnimationId = 9;
                            entity.AIValues[1] = (short)(z >> 0xf);
                            entity.ForceZ = -0x8000;
                            return;
                        }

                        if (z < -0x100000)
                        {
                            entity.TargetAnimationId = 9;

                            if (z < 0)
                            {
                                z = -z;
                            }

                            entity.AIValues[1] = (short)(z >> 0xf);
                            entity.ForceZ = 0x8000;
                            return;
                        }
                    }

                    if (relativePositions[0] < 7 
                        && relativePositions[1] < 7
                        && (Random.Next() * 3) >> 0x20 == 0)
                    {
                        entity.TargetAnimationId = 1;
                        uVar3 = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                        entity.TargetDirection = uVar3;
                        entity.AIValues[1] = 0x28;
                        entity.Bytes[1] = 1;
                    }
                    else
                    {
                        gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x1e, 0x28);
                        entity.Bytes[1] = 0;
                    }
                }
                else if (relativePositions[0] < 3 && relativePositions[1] < 3)
                {
                    entity.TargetAnimationId = 3;
                    uVar3 = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = uVar3;
                    entity.AIValues[1] = 0x6e;
                    entity.Bytes[2] = 0;
                    entity.Bytes[0] = 1;
                }
                break;

            case 1:
                val = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = val;

                if (val != 0)
                {
                    if (entity.ForceAdjusted == 0)
                    {
                        return;
                    }

                    bVar1 = entity.Bytes[1];
                    entity.AIValues[1] = 0;

                    if (bVar1 == 0)
                    {
                        entity.TargetAnimationId = 0;
                        bVar1 = gameEngine.StaticVariables.g_directionFlipTable[entity.TargetDirection];
                        entity.ForceStepY = 0;
                        entity.ForceStepX = 0;
                        entity.ForceY = 0;
                        entity.ForceX = 0;
                        entity.TargetForceY = 0;
                        entity.TargetForceX = 0;
                        entity.TargetDirection = (uint)bVar1;
                        return;
                    }

                    z = gameEngine.EntityGameplayManager.UpdateDirectionForced(entity, 1, 9, 0x500000);

                    if (entity.TargetAnimationId == 9)
                    {
                        entity.AIValues[1] = (short)(z >> 0xf);
                        return;
                    }
                }

                entity.TargetAnimationId = 0;
                break;

            case 3:
                val = entity.AIValues[1];

                if (val == 0)
                {
                    return;
                }

                if (entity.Bytes[2] != 0)
                {
                    return;
                }

                entity.AIValues[1] = (short)(val - 1);

                if (val != 1)
                {
                    return;
                }

                gameEngine.SoundManager.PlaySoundEffect(0x1cd);

                entity.Bytes[2] = 1;
                val = 1;
                goto LAB_80068138;

            case 5:
            case 7:
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

                val = 0x1e;
                entity.TargetAnimationId = 0;
                LAB_80068138:
                entity.AIValues[1] = val;
                break;

            case 9:
                val = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = val;

                if (val == 0)
                {
                    gameEngine.EntityGameplayManager.SetEntityRandomDirection(entity, 1, 0x78);
                    entity.Bytes[1] = 0;
                    entity.ForceZ = 0;
                }

                break;
        }
    }

    //80068154
    public static void AI_UpdateEntityAI_6_2(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Muruta (griffes) Niv.1")
        {
            Breakpoint.TriggerBreak();
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
                if (entity.IsOnGround == 0)
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

    // GHIDRA: AI_UpdateEntityAI_7 @ 0x80068930
    public static void AI_UpdateEntityAI_7(GameEngine gameEngine, Entity entity)
    {
        short sVar3;
        int iVar4;
        uint uVar5;
        int[] positions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, positions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                sVar3 = entity.AIValues[1];

                if (sVar3 != 0)
                {
                    entity.AIValues[1] = (short)(sVar3 - 1);

                    if (sVar3 != 1)
                    {
                        return;
                    }
                }

                if (5 < positions[0] || 5 < positions[1] || 0x100000 < positions[2])
                {
                    gameEngine.EntityGameplayManager.SetEntityRandomDirection(entity, 1, 300);
                    return;
                }

                if (gameEngine.StaticVariables.PlayerEntity.FrameCollision == null ||
                    gameEngine.StaticVariables.PlayerEntity.BalanceAnimValRef == null ||
                    gameEngine.StaticVariables.PlayerEntity.BalanceAnimValRef.Val == 0)
                {
                    uVar5 = (uint)ScriptHelper.GetDirectionToTarget(
                        entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX,
                        entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
                    entity.TargetDirection = uVar5;
                    entity.TargetAnimationId = 1;
                    entity.AIValues[1] = 300;
                    return;
                }

                goto LAB_80068c00;

            case 1:
                if (5 < positions[0] ||
                    5 < positions[1] ||
                    0x100000 < positions[2] ||
                    gameEngine.StaticVariables.PlayerEntity.FrameCollision == null ||
                    gameEngine.StaticVariables.PlayerEntity.BalanceAnimValRef == null ||
                    gameEngine.StaticVariables.PlayerEntity.BalanceAnimValRef.Val == 0)
                {
                    sVar3 = (short)(entity.AIValues[1] - 1);
                    entity.AIValues[1] = sVar3;

                    if (sVar3 != 0 && entity.ForceAdjusted == 0)
                    {
                        iVar4 = gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 1, 0x200000);

                        if (iVar4 == 0)
                        {
                            return;
                        }
                    }

                    sVar3 = 0x3c;
                    entity.TargetAnimationId = 0;
                    goto LAB_80068c58;
                }

            LAB_80068c00:
                entity.TargetAnimationId = 3;
                entity.Bytes[1] = 10;
                break;

            case 3:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    uVar5 = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    var uVar1 = ((Random.Next() * 9) >> 0x20) - 4;
                    var bVar2 = entity.Bytes[1];

                    entity.TargetAnimationId = 4;
                    entity.AIValues[1] = 0x3c;
                    entity.Bytes[1] = (byte)(bVar2 - 1);
                    entity.TargetDirection = (uint)((uVar5 + uVar1) & 0x1f);
                }
                break;

            case 4:
                sVar3 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar3;

                if (sVar3 != 0 && entity.ForceAdjusted == 0)
                {
                    iVar4 = gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 4, 0x200000);

                    if (iVar4 == 0)
                    {
                        return;
                    }
                }

                var bVar3 = (byte)(entity.Bytes[1] - 1);
                entity.Bytes[1] = bVar3;

                if (bVar3 != 0)
                {
                    gameEngine.EntityGameplayManager.StartFlying(entity, 4, 0x28, 0x1e);
                    return;
                }

                uVar5 = 5;
                goto LAB_80068cb0;

            case 5:
                sVar3 = 0x1e;

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 0;

            LAB_80068c58:
                entity.AIValues[1] = sVar3;
                break;

            case 7:
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

                if (entity.Bytes[1] == 0)
                {
                    entity.Bytes[1] = 10;
                }

                uVar5 = 3;

            LAB_80068cb0:
                entity.TargetAnimationId = uVar5;
                break;
        }
    }

    // GHIDRA: AI_UpdateEntityAI_8 @ 0x80068CC8
    public static void AI_UpdateEntityAI_8(GameEngine gameEngine, Entity entity)
    {
        ushort uVar1;
        ulong uVar2;
        byte bVar3;
        bool bVar4;
        short sVar5;
        int iVar6;
        int iVar7;
        uint uVar8;
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

        uVar8 = entity.TargetAnimationId;

        if (uVar8 == 6)
        {
            return;
        }

        if (uVar8 == 7)
        {
            if (entity.ForceResetAnimationFlag == 0)
            {
                return;
            }

            if (entity.Bytes[3] != 0)
            {
                gameEngine.SoundManager.PlaySoundEffect(0x40);
                entity.TargetAnimationId = 2;
                entity.Flags |= 0x40;
                return;
            }

            gameEngine.SoundManager.PlaySoundEffect(0x7d);
            bVar3 = entity.Bytes[2];
            entity.TargetAnimationId = 4;
            entity.DelayOrAngleOrEntityId = 0;

            if (bVar3 != 0)
            {
                entity.Bytes[0] = 4;
                entity.Bytes[1] = 7;
                return;
            }

            bVar3 = 3;
            goto LAB_800691a8;
        }

        if (((uVar8 < 2) || (uVar8 == 5)) &&
            gameEngine.StaticVariables.PlayerEntity.FrameCollision != null &&
            gameEngine.StaticVariables.PlayerEntity.BalanceAnimValRef != null &&
            gameEngine.StaticVariables.PlayerEntity.BalanceAnimValRef.Val != 0 &&
            relativePositions[0] < 3 &&
            relativePositions[1] < 3)
        {
            gameEngine.SoundManager.PlaySoundEffect(0x7d);
            entity.TargetAnimationId = 4;
            bVar3 = 3;
            goto LAB_800691a8;
        }

        bVar3 = entity.Bytes[0];

        if (bVar3 != 1)
        {
            if (bVar3 < 2)
            {
                if (bVar3 != 0)
                {
                    return;
                }

                if (entity.AIValues[1] != 0)
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                    return;
                }

                entity.Bytes[1] = 0;

                if ((int)((Random.Next() * 4) >> 0x20) != 0)
                {
                    entity.Bytes[0] = 3;
                    gameEngine.SoundManager.PlaySoundEffect(0x7d);
                    entity.TargetAnimationId = 4;
                    return;
                }

                entity.Bytes[0] = 1;
                uVar2 = Random.Next();
                entity.TargetAnimationId = 1;
                entity.AIValues[1] = 0x78;
                entity.TargetDirection = (uint)((uVar2 * 0x20) >> 0x20);
                return;
            }

            if (bVar3 == 3)
            {
                bVar3 = entity.Bytes[1];

                if (bVar3 != 1)
                {
                    if (bVar3 < 2)
                    {
                        if (bVar3 != 0)
                        {
                            return;
                        }

                        if (entity.ForceResetAnimationFlag == 0)
                        {
                            return;
                        }

                        gameEngine.EntityGameplayManager.StartFlying(entity, 9, 0x50, 0x46);
                        entity.Bytes[1] = 1;
                        return;
                    }

                    if (bVar3 != 2)
                    {
                        return;
                    }

                    if (entity.ForceResetAnimationFlag == 0)
                    {
                        return;
                    }

                    sVar5 = (short)((Random.Next() * 0x10) >> 0x20);
                    entity.TargetAnimationId = 0;
                    entity.Bytes[0] = 0;
                    goto LAB_80069628;
                }

                sVar5 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar5;

                if (sVar5 == 0)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x7d);
                    entity.TargetAnimationId = 5;
                    bVar3 = 2;
                }
                else
                {
                    if (entity.ForceAdjusted != 0 || CanMoveForward(entity, 0) != 0)
                    {
                        goto LAB_80069270;
                    }

                    bVar4 = gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 2, 5, 0);

                    if (!bVar4)
                    {
                        return;
                    }

                    entity.Bytes[0] = 4;
                    bVar3 = 2;
                }
            }
            else
            {
                if (bVar3 != 4)
                {
                    return;
                }

                switch (entity.Bytes[1])
                {
                    case 0:
                        gameEngine.SoundManager.PlaySoundEffect(0x7d);
                        entity.TargetAnimationId = 4;
                        bVar3 = 1;
                        break;

                    case 1:
                    case 2:
                        if (entity.ForceResetAnimationFlag == 0 && entity.Bytes[1] != 2)
                        {
                            return;
                        }

                        entity.TargetAnimationId = 9;
                        uVar8 = (uint)ScriptHelper.GetDirectionToTarget(
                            gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                            gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                        uVar2 = Random.Next();
                        entity.DelayOrAngleOrEntityId = 0;
                        entity.AIValues[1] = 0x50;
                        entity.Bytes[2] = 3;
                        entity.Bytes[1] = 3;
                        entity.TargetDirection = (uint)(((int)uVar8 + (int)((uVar2 * 9) >> 0x20) - 4) & 0x1f);
                        return;

                    case 3:
                        sVar5 = (short)(entity.AIValues[1] - 1);
                        entity.AIValues[1] = sVar5;

                        if (sVar5 == 0 || entity.ForceAdjusted != 0 || CanMoveForward(entity, 0) != 0)
                        {
                            bVar3 = (byte)(entity.Bytes[2] - 1);
                            entity.Bytes[2] = bVar3;

                            if (bVar3 != 0)
                            {
                                entity.TargetAnimationId = 9;
                                uVar8 = (uint)ScriptHelper.GetDirectionToTarget(
                                    gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                                    gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                                entity.TargetDirection = uVar8;
                                entity.AIValues[1] = 0x3c;
                                return;
                            }

                            gameEngine.SoundManager.PlaySoundEffect(0x7d);
                            entity.TargetAnimationId = 5;
                            bVar3 = 6;
                        }
                        else
                        {
                            iVar6 = entity.DelayOrAngleOrEntityId - 1;

                            if (entity.DelayOrAngleOrEntityId != 0)
                            {
                                entity.DelayOrAngleOrEntityId = iVar6;

                                if (iVar6 != 0)
                                {
                                    return;
                                }
                            }

                            if (2 < relativePositions[0] || 2 < relativePositions[1] || relativePositions[2] != 0)
                            {
                                return;
                            }

                            if (0x27 < (ushort)entity.AIValues[1])
                            {
                                return;
                            }

                            gameEngine.SoundManager.PlaySoundEffect(0x7d);
                            entity.TargetAnimationId = 5;
                            uVar8 = (uint)ScriptHelper.GetDirectionToTarget(
                                gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                                gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                            entity.TargetDirection = uVar8;
                            bVar3 = 4;
                        }
                        break;

                    case 4:
                        if (entity.ForceResetAnimationFlag == 0)
                        {
                            return;
                        }

                        entity.TargetAnimationId = 3;
                        bVar3 = 5;
                        break;

                    case 5:
                        if (entity.ForceResetAnimationFlag == 0 && entity.TargetAnimationId != 0)
                        {
                            return;
                        }

                        bVar3 = 6;

                        if (entity.Bytes[2] != 1)
                        {
                            gameEngine.SoundManager.PlaySoundEffect(0x7d);
                            entity.TargetAnimationId = 4;
                            entity.Bytes[1] = 7;
                            entity.DelayOrAngleOrEntityId = 0x1e;
                            return;
                        }

                        entity.TargetAnimationId = 0;
                        break;

                    case 6:
                        if (entity.ForceResetAnimationFlag == 0 && entity.TargetAnimationId != 0)
                        {
                            return;
                        }

                        entity.Bytes[0] = 0;
                        sVar5 = (short)((Random.Next() * 0x10) >> 0x20);
                        entity.TargetAnimationId = 0;
                        goto LAB_80069628;

                    case 7:
                        if (entity.ForceResetAnimationFlag == 0)
                        {
                            return;
                        }

                        entity.TargetAnimationId = 9;
                        entity.AIValues[1] = 1;
                        bVar3 = 3;
                        break;

                    default:
                        return;
                }
            }

        LAB_80069664:
            entity.Bytes[1] = bVar3;
            return;
        }

        switch (entity.Bytes[1])
        {
            case 0:
                sVar5 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar5;

                if (sVar5 == 0)
                {
                    entity.TargetAnimationId = 0;
                    bVar3 = gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection];
                    entity.Bytes[1] = 4;
                    entity.AIValues[1] = 0x3c;
                    entity.TargetDirection = (uint)((bVar3 + 8) & 0x1f);
                    return;
                }

                if (entity.ForceAdjusted != 0)
                {
                    var stepDistance = entity.SpriteRecord!.AnimSets[9].Acceleration;
                    iVar6 = gameEngine.EntityGameplayManager.GetTileHeightAtOffset(
                        entity,
                        gameEngine.StaticVariables.g_offsetXList[entity.TargetDirection] * stepDistance,
                        gameEngine.StaticVariables.g_offsetYList[entity.TargetDirection] * stepDistance);
                    iVar6 -= entity.FloorHeight;

                    if (iVar6 < 0x300001)
                    {
                        gameEngine.SoundManager.PlaySoundEffect(0x7d);
                        uVar1 = (ushort)entity.AIValues[1];
                        entity.TargetAnimationId = 4;
                        entity.DelayOrAngleOrEntityId = iVar6;
                        entity.Bytes[1] = 1;
                        entity.ItemState = uVar1;
                        return;
                    }

                    goto LAB_80069270;
                }

                if (CanMoveForward(entity, 0x300000) != 0)
                {
                    goto LAB_80069270;
                }

                iVar6 = 4;
                break;

            case 1:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                iVar6 = entity.PosZ;
                iVar7 = entity.DelayOrAngleOrEntityId;
                entity.AIValues[1] = 0xf;
                entity.TargetAnimationId = 9;
                entity.Bytes[1] = 2;
                entity.PosZ = iVar6 + iVar7;
                return;

            case 2:
                sVar5 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar5;

                if (sVar5 != 0)
                {
                    return;
                }

                gameEngine.SoundManager.PlaySoundEffect(0x7d);
                iVar6 = entity.ItemState;
                entity.TargetAnimationId = 5;
                entity.Bytes[1] = 3;
                entity.AIValues[1] = (short)iVar6;
                return;

            case 3:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 1;
                entity.Bytes[1] = 0;
                return;

            case 4:
            case 5:
            case 6:
                sVar5 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar5;

                if (sVar5 == 0)
                {
                    bVar3 = entity.Bytes[1];
                    entity.AIValues[1] = 0x28;

                    if (bVar3 == 4)
                    {
                        entity.TargetDirection = (uint)((gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection] + 0x10) & 0x1f);
                    }

                    bVar3 = entity.Bytes[1];

                    if (bVar3 == 5)
                    {
                        entity.TargetDirection = (uint)((gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection] - 8) & 0x1f);
                        bVar3 = entity.Bytes[1];
                    }

                    if (bVar3 == 6)
                    {
                        entity.Bytes[0] = 0;
                        entity.AIValues[1] = (short)((int)((Random.Next() * 0x21) >> 0x20) + entity.AIValues[1]);
                    }

                    bVar3 = (byte)(entity.Bytes[1] + 1);
                    entity.Bytes[1] = bVar3;
                    return;
                }

                iVar6 = 5;
                break;

            default:
                return;
        }

        bVar4 = gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 2, iVar6, 0);
        bVar3 = 4;

        if (!bVar4)
        {
            return;
        }

    LAB_800691a8:
        entity.Bytes[0] = bVar3;
        entity.Bytes[1] = 0;
        return;

    LAB_80069270:
        bVar3 = gameEngine.StaticVariables.g_directionFlipTable[entity.TargetDirection];
        entity.ForceStepY = 0;
        entity.ForceStepX = 0;
        entity.ForceY = 0;
        entity.ForceX = 0;
        entity.TargetForceY = 0;
        entity.TargetForceX = 0;
        entity.TargetDirection = bVar3;
        return;

    LAB_80069628:
        entity.AIValues[1] = (short)(sVar5 + 0x1e);
    }

    // GHIDRA: AI_UpdateEntityAI_8_2 @ 0x80069684
    public static void AI_UpdateEntityAI_8_2(GameEngine gameEngine, Entity entity)
    {
        bool bVar1;
        short sVar2;
        int iVar3;
        uint uVar4;
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                sVar2 = entity.AIValues[1];

                if (sVar2 != 0)
                {
                    entity.AIValues[1] = (short)(sVar2 - 1);

                    if (sVar2 != 1 || entity.Bytes[1] != 1)
                    {
                        return;
                    }

                    entity.TargetAnimationId = 3;
                    return;
                }

                if (relativePositions[0] < 5 && relativePositions[1] < 5 && relativePositions[2] < 0x200001)
                {
                    goto LAB_800697c0;
                }

                bVar1 = gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 1, 10, 0x200000);

                if (!bVar1)
                {
                    gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0xb4, 0x46);
                    return;
                }

                break;

            case 1:
                sVar2 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar2;

                if (sVar2 == 0)
                {
                    goto LAB_800698ac;
                }

                bVar1 = gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 2, 3, 0x300000);

                if (bVar1)
                {
                    goto LAB_800697c0;
                }

                bVar1 = gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 1, 5, 0x200000);

                if (!bVar1)
                {
                    if (entity.ForceAdjusted != 0)
                    {
                        gameEngine.EntityGameplayManager.UpdateDirectionForced(entity, 1, 2, 0x300000);
                        return;
                    }

                    gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 1, 0x300000);
                    return;
                }

                break;

            case 5:
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

                entity.TargetAnimationId = 0;
                entity.AIValues[1] = 0;
                goto LAB_800699a8;

            case 7:
                if (entity.IsOnGround == 0)
                {
                    return;
                }

                if (entity.Bytes[1] != 1)
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0;
                    return;
                }

                entity.TargetAnimationId = 9;
                uVar4 = (uint)ScriptHelper.GetDirectionToTarget(
                    entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX,
                    entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
                entity.TargetDirection = uVar4;
                entity.AIValues[1] = 0x78;
                goto LAB_800699a8;

            case 9:
                sVar2 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar2;

                if (sVar2 != 0)
                {
                    if (entity.ForceAdjusted != 0)
                    {
                        entity.TargetAnimationId = 8;
                        entity.TargetDirection = (entity.TargetDirection - 0x10) & 0x1f;
                        return;
                    }

                    uVar4 = entity.TargetDirection;
                    iVar3 = gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 1, 0);

                    if (iVar3 == 0)
                    {
                        return;
                    }

                    entity.TargetDirection = (byte)uVar4;
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0x1e;
                    return;
                }

            LAB_800698ac:
                entity.TargetAnimationId = 0;
                return;

            default:
                return;
        }

        entity.TargetAnimationId = 9;
        uVar4 = (uint)ScriptHelper.GetDirectionToTarget(
            gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
            gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
        entity.TargetDirection = uVar4;
        entity.AIValues[1] = 0xb4;
        return;

    LAB_800697c0:
        entity.Bytes[1] = 1;
        uVar4 = (uint)ScriptHelper.GetDirectionToTarget(
            gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
            gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
        entity.TargetDirection = uVar4;
        entity.AIValues[1] = 0x1e;
        return;

    LAB_800699a8:
        entity.Bytes[1] = 0;
    }

    // GHIDRA: AI_UpdateEntityAI_9 @ 0x800699C4
    public static void AI_UpdateEntityAI_9(GameEngine gameEngine, Entity entity)
    {
        byte bVar1;
        bool bVar2;
        ushort uVar3;
        short sVar4;
        uint uVar5;
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                entity.TargetAnimationId = 1;
                entity.AIValues[1] = (short)(((Random.Next() * 0x10) >> 0x20) + 0xb4);
                break;

            case 1:
                uVar3 = (ushort)(entity.AIValues[1] - 1);
                entity.AIValues[1] = (short)uVar3;

                if ((uVar3 & 0x1f) == 0)
                {
                    if (relativePositions[0] < 5
                        && relativePositions[1] < 5
                        && gameEngine.StaticVariables.PlayerEntity.FrameCollision != null
                        && gameEngine.StaticVariables.PlayerEntity.BalanceAnimValRef != null
                        && gameEngine.StaticVariables.PlayerEntity.BalanceAnimValRef.Val != 0)
                    {
                        entity.TargetAnimationId = 8;
                        uVar5 = (uint)ScriptHelper.GetDirectionToTarget(
                            entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX,
                            entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
                        entity.TargetDirection = uVar5;
                        return;
                    }

                    bVar2 = gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 1, 8, 0x100000);

                    if (bVar2)
                    {
                        uVar5 = (uint)ScriptHelper.GetDirectionToTarget(
                            gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                            gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                        entity.TargetDirection = uVar5;
                        entity.TargetAnimationId = 3;
                        entity.AIValues[1] = 0xb4;
                        return;
                    }
                }

                if (entity.AIValues[1] != 0)
                {
                    if (entity.ForceAdjusted != 0)
                    {
                        gameEngine.EntityGameplayManager.UpdateDirectionForced(entity, 1, 2, 0x300000);
                        return;
                    }

                    gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 1, 0x300000);
                    return;
                }

                entity.TargetAnimationId = 0;
                bVar1 = gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection];
                entity.ForceStepY = 0;
                entity.ForceStepX = 0;
                entity.ForceY = 0;
                entity.ForceX = 0;
                entity.TargetForceY = 0;
                entity.TargetForceX = 0;
                entity.TargetDirection = (uint)((bVar1 - 8) & 0x1f);
                break;

            case 3:
                sVar4 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar4;

                if (sVar4 == 0)
                {
                    entity.TargetAnimationId = 0;
                    break;
                }

                if (entity.ForceAdjusted == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 8;
                entity.TargetDirection = (entity.TargetDirection - 0x10) & 0x1f;
                break;

            case 5:
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

                entity.TargetAnimationId = 0;
                break;

            case 7:
            case 9:
                if (entity.IsOnGround != 0)
                {
                    entity.TargetAnimationId = 0;
                }
                break;
        }
    }

    //80069c84
    //◆Orc (hache) Niv.1
    public static void AI_UpdateEntityAI_10(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Orc (hache) Niv.1")
        {
            Breakpoint.TriggerBreak();
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
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Orc (armure de fer) Niv.1")
        {
            Breakpoint.TriggerBreak();
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
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Poisson Niv.1")
        {
            Breakpoint.TriggerBreak();
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
                if (entity.IsOnGround == 0)
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

    //80080260
    public static int CanMoveForward(Entity entity, int zOffset)
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
            || adjustedZOffset < floorToMapHeight0)
        {
            result = 1;

            if (0xf < entity.TargetDirection)
            {
                result = 0;
            }
        }

        return result;
    }

    // GHIDRA: AI_UpdateEntityAI_13 @ 0x8006ABB0
    public static void AI_UpdateEntityAI_13(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Homme-ombre Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        short sVar1 = 0;
        uint uVar2 = 0;
        int iVar3 = 0;
        byte bVar4 = 0;
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                uVar2 = 10;

                if (entity.Bytes[0] != 3)
                {
                    if (entity.AIValues[1] != 0)
                    {
                        entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                        return;
                    }

                    if (relativePositions[0] < 4 && relativePositions[1] < 4 && relativePositions[2] == 0)
                    {
                        uVar2 = (uint)((Random.Next() * 4) >> 0x20);
                        bVar4 = 1;

                        if (uVar2 == 1)
                        {
                            entity.TargetAnimationId = 1;
                            uVar2 = (uint)ScriptHelper.GetDirectionToTarget(
                                gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                                gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                            entity.TargetDirection = uVar2;
                        }
                        else
                        {
                            if (uVar2 < 2)
                            {
                                if (uVar2 != 0)
                                {
                                    return;
                                }

                                entity.TargetAnimationId = 3;
                                goto LAB_8006b0b0;
                            }

                            if (uVar2 == 2)
                            {
                                entity.TargetAnimationId = 1;
                                uVar2 = (uint)ScriptHelper.GetDirectionToTarget(
                                    gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                                    gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                                uVar2 += 4;
                            }
                            else
                            {
                                if (uVar2 != 3)
                                {
                                    return;
                                }

                                entity.TargetAnimationId = 1;
                                uVar2 = (uint)ScriptHelper.GetDirectionToTarget(
                                    gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                                    gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                                uVar2 -= 4;
                            }

                            entity.TargetDirection = uVar2 & 0x1f;
                        }

                        entity.AIValues[1] = 0x28;
                        return;
                    }

                    uVar2 = 9;

                    if (relativePositions[0] < 5 && relativePositions[1] < 5 && relativePositions[2] == 0)
                    {
                        iVar3 = (int)((Random.Next() * 2) >> 0x20);

                        if (iVar3 == 0)
                        {
                            entity.TargetAnimationId = 1;
                            uVar2 = (uint)ScriptHelper.GetDirectionToTarget(
                                gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                                gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                            entity.TargetDirection = uVar2;
                            entity.AIValues[1] = (short)(((Random.Next() * 0x10) >> 0x20) + 0x14);
                            return;
                        }

                        if (iVar3 != 1)
                        {
                            return;
                        }

                        entity.TargetAnimationId = 3;
                        uVar2 = (uint)ScriptHelper.GetDirectionToTarget(
                            gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                            gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                        entity.TargetDirection = uVar2;
                        entity.Bytes[0] = 2;
                        return;
                    }

                    goto LAB_8006b160;
                }

                break;

            case 1:
                sVar1 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar1;

                if (sVar1 == 0)
                {
                    entity.TargetAnimationId = 0;
                    return;
                }

                uVar2 = 9;

                if (4 < relativePositions[0] || 4 < relativePositions[1])
                {
                    goto LAB_8006b160;
                }

                if (entity.ForceAdjusted == 0 && CanMoveForward(entity, 0) == 0)
                {
                    return;
                }

                var stepDistance = entity.SpriteRecord!.AnimSets[10].Acceleration;
                iVar3 = gameEngine.EntityGameplayManager.GetTileHeightAtOffset(
                    entity,
                    gameEngine.StaticVariables.g_offsetXList[entity.TargetDirection] * stepDistance,
                    gameEngine.StaticVariables.g_offsetYList[entity.TargetDirection] * stepDistance);

                if (iVar3 == 0x7800000)
                {
                    return;
                }

                bVar4 = 3;

                if (relativePositions[2] != 0)
                {
                    entity.TargetAnimationId = 3;
                    goto LAB_8006b0b0;
                }

                entity.TargetAnimationId = 0;
                goto LAB_8006b080;

            case 2:
                sVar1 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar1;

                if (sVar1 != 0)
                {
                    return;
                }

                entity.TargetAnimationId = 5;
                return;

            case 3:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                bVar4 = entity.Bytes[0];

                if (bVar4 != 2)
                {
                    if (2 < bVar4)
                    {
                        uVar2 = 5;

                        if (bVar4 != 3)
                        {
                            return;
                        }

                        goto LAB_8006b160;
                    }

                    if (bVar4 != 1)
                    {
                        return;
                    }

                    entity.TargetAnimationId = 4;
                    goto LAB_8006b20c;
                }

                entity.TargetAnimationId = 2;
                uVar2 = (uint)ScriptHelper.GetDirectionToTarget(
                    gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                    gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                entity.TargetDirection = uVar2;
                sVar1 = 10;
                goto LAB_8006b208;

            case 4:
                uVar2 = 5;

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                break;

            case 5:
                sVar1 = 0x1e;

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                goto LAB_8006b1c0;

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

                sVar1 = 0x14;
                entity.TargetAnimationId = 0;
                goto LAB_8006b208;

            case 9:
                if (0x1e < relativePositions[0])
                {
                    return;
                }

                if (0x1e < relativePositions[1])
                {
                    return;
                }

                entity.TargetAnimationId = 10;
                goto LAB_8006b080;

            case 10:
                if (entity.Bytes[0] == 3)
                {
                    sVar1 = (short)(entity.AIValues[1] - 1);
                    entity.AIValues[1] = sVar1;

                    if (sVar1 != 0)
                    {
                        return;
                    }

                    entity.Bytes[0] = 0;
                    return;
                }

                if (relativePositions[0] != 0 || relativePositions[1] != 0 || relativePositions[2] != 0)
                {
                    uVar2 = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = uVar2;

                    if (entity.ForceAdjusted == 0)
                    {
                        return;
                    }

                    stepDistance = entity.SpriteRecord!.AnimSets[10].Acceleration;
                    iVar3 = gameEngine.EntityGameplayManager.GetTileHeightAtOffset(
                        entity,
                        gameEngine.StaticVariables.g_offsetXList[uVar2] * stepDistance,
                        gameEngine.StaticVariables.g_offsetYList[uVar2] * stepDistance);

                    if (iVar3 == 0x7800000)
                    {
                        return;
                    }

                    iVar3 -= entity.FloorHeight;
                    entity.TerrainHeight += iVar3;
                    entity.FloorHeight += iVar3;
                    entity.PosZ += iVar3;
                    return;
                }

                sVar1 = 0x78;
                goto LAB_8006b1c0;

            default:
                return;
        }

        entity.TargetAnimationId = uVar2;
        entity.AIValues[1] = 0x3c;
        return;

    LAB_8006b080:
        uVar2 = (uint)ScriptHelper.GetDirectionToTarget(
            gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
            gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
        entity.TargetDirection = uVar2;
        return;

    LAB_8006b0b0:
        uVar2 = (uint)ScriptHelper.GetDirectionToTarget(
            gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
            gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
        entity.TargetDirection = uVar2;
        entity.Bytes[0] = bVar4;
        return;

    LAB_8006b160:
        entity.TargetAnimationId = uVar2;
        entity.AIValues[1] = 0;
        return;

    LAB_8006b1c0:
        entity.TargetAnimationId = 0;
        entity.AIValues[1] = sVar1;
        return;

    LAB_8006b208:
        entity.AIValues[1] = sVar1;

    LAB_8006b20c:
        entity.Bytes[0] = 0;
    }

    //8006b234
    public static void AI_UpdateEntityAI_14(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Zombie Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        short val;
        int iVar1;
        uint targetAnimation;
        int[] relativePositions = new int[6];
        byte dir;

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);
        targetAnimation = entity.TargetAnimationId;

        if (targetAnimation == 1)
        {
            val = (short)(entity.AIValues[1] - 1);
            entity.AIValues[1] = val;

            if (val != 0)
            {
                if (relativePositions[0] < 3 
                    && relativePositions[1] < 3 
                    && relativePositions[2] == 0)
                {
                    entity.Bytes[0] = 1;
                    entity.TargetAnimationId = 0;
                    targetAnimation = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = targetAnimation;
                    entity.AIValues[1] = 10;
                    return;
                }

                if (entity.ForceAdjusted == 0 && CanMoveForward(entity, 0) == 0)
                {
                    return;
                }

                dir = gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection];
                entity.ForceStepY = 0;
                entity.ForceStepX = 0;
                entity.ForceY = 0;
                entity.ForceX = 0;
                entity.TargetForceY = 0;
                entity.TargetForceX = 0;
                entity.TargetDirection = (uint)((dir - 0x10) & 0x1f);
                return;
            }
        }
        else
        {
            if ((int)targetAnimation < 2)
            {
                if (targetAnimation != 0)
                {
                    return;
                }
                if (entity.AIValues[1] == 0)
                {
                    entity.AIValues[1] = 0x3c;
                    entity.TargetDirection = (uint)((Random.Next() * 0x20) >> 0x20);
                }

                val = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = val;

                if (val != 0)
                {
                    return;
                }

                if (entity.Bytes[0] != 0)
                {
                    entity.TargetAnimationId = 3;
                    targetAnimation = (uint)ScriptHelper.GetDirectionToTarget(gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = targetAnimation;
                    entity.Bytes[0] = 0;
                    entity.AIValues[6] = 0;
                    entity.AIValues[7] = 0;
                    return;
                }

                entity.TargetAnimationId = 1;
                entity.AIValues[1] = 0x78;
                return;
            }

            if (targetAnimation != 3)
            {
                if (targetAnimation != 5)
                {
                    return;
                }

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

                entity.Bytes[0] = 1;
                entity.TargetAnimationId = 0;
                entity.AIValues[1] = 0x1e;
                return;
            }

            targetAnimation = (uint)(entity.AIValues.GetInt32(6) + 1);
            entity.AIValues.Set(targetAnimation, 6);

            if ((targetAnimation & 7) == 0)
            {
                gameEngine.EffectManager.CreateEffectEntity(0, gameEngine.CurrentMap.Info.C, 0, entity.PosX, entity.PosY, entity.FloorHeight);
            }

            if (entity.ForceResetAnimationFlag == 0 && entity.ForceAdjusted == 0)
            {
                return;
            }
        }
        entity.TargetAnimationId = 0;
    }

    //8006b8d4
    public static void AI_UpdateEntityAI_15(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Champignon Niv.1")
        {
            Breakpoint.TriggerBreak();
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

                if (entity.DelayOrAngleOrEntityId == 0 && (int)((Random.Next() * 4) >> 0x20) == 0)
                {
                    entity.CurrentAnimationId = 0xffffffff;
                    entity.AIValues[1] = 0x10;
                    entity.DelayOrAngleOrEntityId = 1;
                    return;
                }

                entity.DelayOrAngleOrEntityId = 0;
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
    public static void AI_UpdateEntityAI_17(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name) 
            && entity.Name != "◆Slime gélatineux")
        {
            Breakpoint.TriggerBreak();
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

    //0x8006C100
    public static void AI_UpdateEntityAI_18(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Homme de bois Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        short newDelay;
        uint direction;
        ulong seed;
        ulong rand;
        Entity entitySpawned;
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                newDelay = entity.AIValues[1];
                if (newDelay != 0)
                {
                    entity.AIValues[1] = (short)(newDelay - 1);
                    if (newDelay != 1)
                    {
                        return;
                    }
                }

                gameEngine.EntityGameplayManager.SetEntityRandomDirection(entity, 1, 0x78);
                return;

            case 1:
                newDelay = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = newDelay;

                if (newDelay == 0)
                {
                    direction = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.Bytes[0] = 0;
                    entity.AIValues[1] = (short)(((Random.Next() * 0x20) >> 0x20) + 0x78);
                    return;
                }

                if (entity.Bytes[0] == 1)
                {
                    if (entity.ForceAdjusted == 0 && CanMoveForward(entity, 0) == 0)
                    {
                        return;
                    }

                    direction = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.Bytes[0] = 0;
                    entity.AIValues[1] = (short)(((Random.Next() * 0x20) >> 0x20) + 0x78);
                    return;
                }

                if (!gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 2, 4, 0x100000))
                {
                    if (entity.ForceAdjusted == 0 && CanMoveForward(entity, 0) == 0)
                    {
                        return;
                    }

                    entity.TargetDirection = (uint)((Random.Next() * 0x20) >> 0x20);
                    return;
                }

                entity.TargetAnimationId = 3;
                entity.AIValues[1] = 0x15;
                return;

            case 2:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 3;
                direction = (uint)ScriptHelper.GetDirectionToTarget(
                    gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                    gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                entity.TargetDirection = direction;
                entity.AIValues[1] = 0x15;
                return;

            case 3:
                newDelay = entity.AIValues[1];
                if (newDelay != 0)
                {
                    entity.AIValues[1] = (short)(newDelay - 1);

                    if (newDelay == 1)
                    {
                        int z = entity.PosZ;
                        direction = (entity.TargetDirection - 6) & 0x1f;

                        for (int i = 0; i < 4; i++)
                        {
                            entitySpawned = gameEngine.SpawnWarpEntity(
                                entity,
                                1,
                                0xce,
                                entity.PosX,
                                entity.PosY,
                                z + 0x200000,
                                direction);

                            entitySpawned.ForceZ = 0x40000;
                            direction = (direction + 4) & 0x1f;
                        }
                    }
                }

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                direction = (uint)ScriptHelper.GetDirectionToTarget(
                    entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX,
                    entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
                seed = Random.Next();
                rand = Random.Next();

                entity.TargetAnimationId = 1;
                entity.TargetDirection = (uint)((direction + (int)((seed * 5) >> 0x20) - 2) & 0x1f);
                entity.Bytes[0] = 1;
                entity.AIValues[1] = (short)(((rand * 0x20) >> 0x20) + 0x78);
                return;

            case 4:
                if (2 < relativePositions[0])
                {
                    return;
                }

                if (2 < relativePositions[1])
                {
                    return;
                }

                if (0x100000 < relativePositions[2])
                {
                    return;
                }

                entity.TargetAnimationId = 2;
                return;

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

                entity.TargetAnimationId = 0;
                entity.AIValues[1] = (short)(((Random.Next() * 0x10) >> 0x20) + 0x20);
                return;

            default:
                return;
        }
    }

    //8006c5cc
    public static void AI_UpdateEntityAI_19(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Fourneau Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        byte stateByte;
        short delay;
        uint direction;
        Entity entitySpawned;
        int spawnOffsetX = 0;
        int spawnOffsetY = 0;
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.IsOnGround != 0)
                {
                    if (entity.AIValues[1] == 0)
                    {
                        entity.AIValues[1] = 0x3c;
                    }

                    delay = (short)(entity.AIValues[1] - 1);
                    entity.AIValues[1] = delay;

                    if (delay == 0)
                    {
                        if (entity.Bytes[1] == 0)
                        {
                            entity.TargetDirection = (entity.TargetDirection + (uint)(((Random.Next() * 0x1f) >> 0x20) + 1)) & 0x1f;

                            if (entity.Bytes[0] == 3)
                            {
                                entity.TargetAnimationId = 9;
                                entity.Bytes[0] = 0;
                            }
                            else
                            {
                                entity.TargetAnimationId = 1;
                                entity.Bytes[1] = 4;
                            }
                        }
                        else
                        {
                            entity.TargetAnimationId = 1;
                            entity.TargetDirection = (uint)((Random.Next() * 0x20) >> 0x20);
                        }
                    }
                }
                break;

            case 1:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.Bytes[1] = (byte)(entity.Bytes[1] - 1);
                    stateByte = entity.Bytes[1];
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = (short)(((Random.Next() * 0x10) >> 0x20) + 0x1e);

                    if (stateByte == 0)
                    {
                        entity.Bytes[0] = (byte)(entity.Bytes[0] + 1);
                    }

                    break;
                }

                if (gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 2, 3, 0x300000))
                {
                    entity.TargetAnimationId = 3;
                    direction = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.Bytes[2] = 1;
                    break;
                }

                if (gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 1, 6, 0x300000))
                {
                    entity.TargetAnimationId = 4;
                    entity.AIValues[1] = 0x18;
                    break;
                }

                if (entity.ForceAdjusted == 0)
                {
                    gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 1, 0x300000);
                }
                else
                {
                    gameEngine.EntityGameplayManager.UpdateDirectionForced(entity, 1, 9, 0x300000);
                }
                break;

            case 4:
                delay = entity.AIValues[1];

                if (delay != 0)
                {
                    entity.AIValues[1] = (short)(delay - 1);

                    if (delay == 1)
                    {
                        switch (entity.AnimationDirection)
                        {
                            case 0:
                                spawnOffsetX = 0;
                                spawnOffsetY = 0x40000;
                                break;
                            case 1:
                                spawnOffsetX = 0;
                                spawnOffsetY = -0x40000;
                                break;
                            case 2:
                                spawnOffsetX = -0x40000;
                                spawnOffsetY = 0;
                                break;
                            case 3:
                                spawnOffsetX = 0x40000;
                                spawnOffsetY = 0;
                                break;
                        }

                        direction = (uint)ScriptHelper.GetDirectionToTarget(
                            gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                            gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);

                        entitySpawned = gameEngine.SpawnWarpEntity(
                            entity,
                            1,
                            0xd4,
                            entity.PosX + spawnOffsetX,
                            entity.PosY + spawnOffsetY,
                            entity.PosZ,
                            (direction + 2) & 0x1c);

                        if (entitySpawned != null)
                        {
                            entitySpawned.AnimationDirection = entity.AnimationDirection;
                        }
                    }
                }
                break;

            case 6:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[3] != 0)
                    {
                        entity.TargetAnimationId = 7;
                        entity.Flags |= 0x40;
                    }
                    else
                    {
                        entity.TargetAnimationId = 0;
                        entity.Bytes[2] = 0;
                        entity.AIValues[1] = (short)(((Random.Next() * 0x10) >> 0x20) + 0x20);
                    }
                }
                break;

            case 8:
                if (entity.IsOnGround != 0)
                {
                    if (entity.Bytes[2] != 0)
                    {
                        entity.TargetAnimationId = 9;
                        direction = (uint)ScriptHelper.GetDirectionToTarget(
                            entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX,
                            entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
                        entity.TargetDirection = direction;
                        entity.Bytes[2] = 0;
                    }
                    else
                    {
                        entity.TargetAnimationId = 0;
                        entity.AIValues[1] = 0x28;
                    }
                }
                break;
        }

        entity.TargetDirection = (entity.TargetDirection + 2) & 0x1c;
    }

    //8006ca40
    public static void AI_UpdateEntityAI_20(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Ver Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        byte phase;
        short delay;
        uint direction;
        int spawnOffsetX = 0;
        int spawnOffsetY = 0;
        int spawnOffsetZ = 0;
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.Bytes[2] == 0
                    && gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 1, 5, 0x200000))
                {
                    entity.TargetAnimationId = 3;
                    direction = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.AIValues[1] = 0x3b;
                    return;
                }

                delay = entity.AIValues[1];
                if (delay != 0)
                {
                    entity.AIValues[1] = (short)(delay - 1);
                    if (delay != 1)
                    {
                        return;
                    }
                }

                phase = entity.Bytes[0];
                entity.Bytes[0] = (byte)(phase + 1);

                if (phase == 0)
                {
                    entity.AIValues[1] = 0x14;
                    entity.TargetDirection = ((uint)(gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection] + 8)) & 0x1f;
                    return;
                }

                if (phase == 1)
                {
                    entity.AIValues[1] = 0x14;
                    entity.TargetDirection = ((uint)(gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection] + 0x10)) & 0x1f;
                    return;
                }

                if (phase == 2)
                {
                    gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x78, 0x14);
                    entity.Bytes[2] = 0;
                    entity.Bytes[0] = 0;
                    return;
                }

                entity.Bytes[0] = 0;
                return;

            case 1:
                delay = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = delay;

                if (delay == 0)
                {
                    entity.TargetAnimationId = 5;
                    return;
                }

                if (entity.ForceAdjusted == 0 && CanMoveForward(entity, 0) == 0)
                {
                    return;
                }

                entity.ForceStepY = 0;
                entity.ForceStepX = 0;
                entity.ForceY = 0;
                entity.ForceX = 0;
                entity.TargetForceY = 0;
                entity.TargetForceX = 0;
                entity.TargetDirection = (entity.TargetDirection + (uint)(((Random.Next() * 0x18) >> 0x20) + 1)) & 0x1f;
                return;

            case 2:
                if (entity.AIValues[1] == 0)
                {
                    entity.AIValues[1] = 0x78;
                }

                delay = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = delay;

                if (delay == 0)
                {
                    entity.TargetAnimationId = 4;
                }
                return;

            case 3:
                delay = entity.AIValues[1];
                if (delay != 0)
                {
                    entity.AIValues[1] = (short)(delay - 1);

                    if (delay == 1)
                    {
                        switch (entity.AnimationDirection)
                        {
                            case 0:
                                spawnOffsetX = 0;
                                spawnOffsetY = 0x60000;
                                spawnOffsetZ = 0x140000;
                                break;
                            case 1:
                                spawnOffsetX = 0;
                                spawnOffsetY = -0x60000;
                                spawnOffsetZ = 0x140000;
                                break;
                            case 2:
                                spawnOffsetX = -0xc0000;
                                spawnOffsetY = 0;
                                spawnOffsetZ = 0x140000;
                                break;
                            case 3:
                                spawnOffsetX = 0xc0000;
                                spawnOffsetY = 0;
                                spawnOffsetZ = 0x140000;
                                break;
                        }

                        gameEngine.SpawnWarpEntity(
                            entity,
                            1,
                            0x9b,
                            entity.PosX + spawnOffsetX,
                            entity.PosY + spawnOffsetY,
                            entity.PosZ + spawnOffsetZ,
                            entity.TargetDirection);
                    }
                }

                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 5;
                    entity.Bytes[2] = 1;
                }
                return;

            case 4:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 0;
                entity.AIValues[1] = 0x14;
                entity.Bytes[0] = 0;
                return;

            case 5:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[1] != 0)
                    {
                        entity.Bytes[1] = 0;
                        entity.PosX += entity.DelayOrAngleOrEntityId;
                        entity.PosY += entity.ItemState;
                    }

                    entity.TargetAnimationId = 2;
                }
                return;

            case 7:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[3] == 0)
                    {
                        entity.TargetAnimationId = 5;
                        entity.AIValues[1] = 0;
                        entity.Bytes[1] = 0;
                    }
                    else
                    {
                        entity.TargetAnimationId = 8;
                        entity.Flags |= 0x40;
                    }
                }
                return;

            default:
                return;
        }
    }

    // 0x8006D550
    public static void AI_UpdateEntityAI_20_2(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Homme de boue Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        byte phase;
        short delay;
        uint direction;
        Entity entitySpawned;
        int spawnOffsetX = 0;
        int spawnOffsetY = 0;
        int spawnOffsetZ = 0;
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

        if (entity.TargetAnimationId < 2
            && relativePositions[0] < 4
            && relativePositions[1] < 4
            && relativePositions[2] < 0x100001)
        {
            entity.TargetAnimationId = 3;
            entity.CurrentAnimationId = 0xffffffff;
            direction = (uint)ScriptHelper.GetDirectionToTarget(
                gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
            entity.TargetDirection = direction;
            entity.AIValues[1] = 0x20;
            return;
        }

        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.AIValues[1] != 0)
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                    return;
                }

                if (entity.Bytes[1] == 0)
                {
                    gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x50, 0x1e);
                    return;
                }

                phase = entity.Bytes[1];
                entity.AIValues[1] = 0x3c;
                entity.Bytes[1] = (byte)(phase - 1);

                if (phase == 1)
                {
                    entity.TargetDirection = (entity.TargetDirection + 0x10) & 0x1f;
                    return;
                }

                if (phase == 2)
                {
                    if (((Random.Next() * 2) >> 0x20) == 0)
                    {
                        entity.TargetDirection = ((uint)(gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection] - 8)) & 0x1f;
                    }
                    else
                    {
                        entity.TargetDirection = ((uint)(gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection] + 8)) & 0x1f;
                    }
                }

                return;

            case 1:
                delay = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = delay;

                if (delay == 0)
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = (short)(((Random.Next() * 0x20) >> 0x20) + 0x1e);
                    return;
                }

                if (entity.ForceAdjusted == 0)
                {
                    if (CanMoveForward(entity, 0) == 0)
                    {
                        return;
                    }

                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0;
                    entity.Bytes[1] = 2;
                    return;
                }

                direction = gameEngine.StaticVariables.g_directionFlipTable[entity.TargetDirection];
                entity.ForceStepY = 0;
                entity.ForceStepX = 0;
                entity.ForceY = 0;
                entity.ForceX = 0;
                entity.TargetForceY = 0;
                entity.TargetForceX = 0;
                entity.TargetDirection = (direction + 0x10) & 0x1f;
                return;

            case 3:
                delay = entity.AIValues[1];
                if (delay != 0)
                {
                    entity.AIValues[1] = (short)(delay - 1);

                    if (delay == 1)
                    {
                        switch (entity.AnimationDirection)
                        {
                            case 0:
                                spawnOffsetX = 0;
                                spawnOffsetY = 0x40000;
                                spawnOffsetZ = 0x120000;
                                break;
                            case 1:
                                spawnOffsetX = 0;
                                spawnOffsetY = -0x40000;
                                spawnOffsetZ = 0x120000;
                                break;
                            case 2:
                                spawnOffsetX = -0x80000;
                                spawnOffsetY = 0;
                                spawnOffsetZ = 0x120000;
                                break;
                            case 3:
                                spawnOffsetX = 0x80000;
                                spawnOffsetY = 0;
                                spawnOffsetZ = 0x120000;
                                break;
                        }

                        entitySpawned = gameEngine.SpawnWarpEntity(
                            entity,
                            1,
                            0xb0,
                            entity.PosX + spawnOffsetX,
                            entity.PosY + spawnOffsetY,
                            entity.PosZ + spawnOffsetZ,
                            entity.TargetDirection);

                        if (entitySpawned != null)
                        {
                            entitySpawned.ForceZ = 0x20000;
                        }

                        entity.Bytes[0] = 0;
                    }
                }

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 0;
                entity.AIValues[1] = 0x1e;
                return;

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

                if (((Random.Next() * 5) >> 0x20) == 0)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0x9e);
                }

                entity.TargetAnimationId = 1;
                direction = (uint)ScriptHelper.GetDirectionToTarget(
                    entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX,
                    entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
                entity.TargetDirection = direction;
                entity.AIValues[1] = 0x28;
                entity.Bytes[0] = 1;
                entity.Bytes[1] = 0;
                return;

            default:
                return;
        }
    }

    // 0x8006D998
    public static void AI_UpdateEntityAI_21(GameEngine gameEngine, Entity entity)
    {
        if (string.IsNullOrEmpty(entity.Name)
            || entity.Name != "◆Homme-lézard (projectiles) Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        var tickProgram = entity.SpriteProgramIndexes[ScriptHelper.ProgramCTick];

        short delay;
        uint direction;
        Entity entitySpawned;
        int spawnOffsetX = 0;
        int spawnOffsetY = 0;
        int spawnOffsetZ = 0;
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.AIValues[1] == 0)
                {
                    entity.TargetAnimationId = 1;
                    entity.AIValues[1] = (short)(((Random.Next() * 0x20) >> 0x20) + 0x78);

                    if ((relativePositions[0] < 0xf || relativePositions[1] < 0xf)
                        && relativePositions[2] < 0x700000)
                    {
                        direction = (uint)ScriptHelper.GetDirectionToTarget(
                            gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                            gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                        entity.TargetDirection = direction;
                        entity.AIValues[1] = (short)(entity.AIValues[1] + 0x28);
                    }
                    else
                    {
                        entity.TargetDirection = (uint)((Random.Next() * 0x20) >> 0x20);
                    }
                }
                else
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                }
                return;

            case 1:
                delay = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = delay;

                if (delay == 0)
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = (short)(((Random.Next() * 0x10) >> 0x20) + 0x3c);
                    return;
                }

                if (!gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 1, 4, 0x100000)
                    && (relativePositions[0] > 2 || relativePositions[1] > 2 || relativePositions[2] > 0x10000))
                {
                    if (entity.ForceAdjusted != 0)
                    {
                        gameEngine.EntityGameplayManager.UpdateDirectionForced(entity, 1, 4, 0x200000);
                    }
                    else
                    {
                        gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 1, 0x200000);
                    }
                    return;
                }

                entity.TargetAnimationId = 2;
                direction = (uint)ScriptHelper.GetDirectionToTarget(
                    entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX,
                    entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
                entity.TargetDirection = direction;
                return;

            case 3:
                delay = entity.AIValues[1];
                if (delay != 0)
                {
                    entity.AIValues[1] = (short)(delay - 1);

                    if (delay == 1)
                    {
                        spawnOffsetZ = 0xa0000;

                        if (tickProgram == 0x20)
                        {
                            switch (entity.AnimationDirection)
                            {
                                case 0:
                                    spawnOffsetX = 0;
                                    spawnOffsetY = -0x40000;
                                    break;
                                case 1:
                                    spawnOffsetX = 0;
                                    spawnOffsetY = 0x40000;
                                    break;
                                case 2:
                                    spawnOffsetX = -0xe0000;
                                    spawnOffsetY = 0;
                                    break;
                                case 3:
                                    spawnOffsetX = 0xe0000;
                                    spawnOffsetY = 0;
                                    break;
                            }

                            gameEngine.SpawnWarpEntity(
                                entity,
                                1,
                                0x6e,
                                entity.PosX + spawnOffsetX,
                                entity.PosY + spawnOffsetY,
                                entity.PosZ + spawnOffsetZ,
                                entity.TargetDirection);
                        }
                        else
                        {
                            switch (entity.AnimationDirection)
                            {
                                case 0:
                                    spawnOffsetX = 0;
                                    spawnOffsetY = 0xe0000;
                                    break;
                                case 1:
                                    spawnOffsetX = 0;
                                    spawnOffsetY = -0xe0000;
                                    break;
                                case 2:
                                    spawnOffsetX = -0x150000;
                                    spawnOffsetY = 0;
                                    break;
                                case 3:
                                    spawnOffsetX = 0x150000;
                                    spawnOffsetY = 0;
                                    break;
                            }

                            entitySpawned = gameEngine.SpawnWarpEntity(
                                entity,
                                1,
                                0x74,
                                entity.PosX + spawnOffsetX,
                                entity.PosY + spawnOffsetY,
                                entity.PosZ + spawnOffsetZ,
                                entity.TargetDirection);

                            if (entitySpawned != null)
                            {
                                direction = (uint)((Random.Next() * 3) >> 0x20);
                                if (direction != 0)
                                {
                                    direction += 1;
                                }

                                entitySpawned.TargetAnimationId = direction;
                                entitySpawned.ForceZ = 0x38000;
                            }
                        }
                    }
                }

                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0x78;
                }
                return;

            case 5:
                if (entity.IsOnGround == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 3;
                entity.AIValues[1] = 0xf;
                entity.TargetDirection = (entity.TargetDirection + 0x10) & 0x1f;
                return;

            case 6:
                if (entity.IsOnGround != 0)
                {
                    entity.TargetAnimationId = 1;
                }
                return;

            case 9:
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

                entity.TargetAnimationId = 2;
                direction = (uint)ScriptHelper.GetDirectionToTarget(
                    entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX,
                    entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
                entity.TargetDirection = direction;
                return;

            case 10:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 0;
                entity.AIValues[1] = 0x1e;
                entity.TargetDirection = (entity.TargetDirection + 0x10) & 0x1f;
                return;

            case 11:
                if (entity.TargetAnimationId == entity.CurrentAnimationId && entity.IsOnGround != 0)
                {
                    entity.TargetAnimationId = 10;
                }
                return;

            default:
                return;
        }
    }

    //8006de68
    public static void AI_UpdateEntityAI_22(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

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
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

        byte phase;
        byte attackMode;
        short delay;
        uint direction;
        int spawnOffsetX = 0;
        int spawnOffsetY = 0;
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

        if (entity.TargetAnimationId == 5)
        {
            delay = entity.AIValues[1];
            if (delay != 0)
            {
                entity.AIValues[1] = (short)(delay - 1);

                if (delay == 1)
                {
                    switch (entity.AnimationDirection)
                    {
                        case 0:
                            spawnOffsetX = 0;
                            spawnOffsetY = -0x80000;
                            break;
                        case 1:
                            spawnOffsetX = 0;
                            spawnOffsetY = 0x80000;
                            break;
                        case 2:
                            spawnOffsetX = -0xa0000;
                            spawnOffsetY = 0;
                            break;
                        case 3:
                            spawnOffsetX = 0xa0000;
                            spawnOffsetY = 0;
                            break;
                    }

                    gameEngine.SpawnWarpEntity(
                        entity,
                        1,
                        0x65,
                        entity.PosX + spawnOffsetX,
                        entity.PosY + spawnOffsetY,
                        entity.PosZ + 0xa0000,
                        (entity.TargetDirection - 7) & 0x1f);

                    entity.Bytes[2] = 0x1e;
                }
            }
        }
        else if (entity.TargetAnimationId < 6)
        {
            if (entity.TargetAnimationId == 0 && entity.IsOnGround != 0)
            {
                if (entity.Bytes[2] == 0)
                {
                    if (entity.AIValues[1] != 0)
                    {
                        entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                    }

                    if (entity.Bytes[1] == 2
                        || gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 1, 6, 0x200000))
                    {
                        entity.Bytes[1] = 0;

                        if (relativePositions[0] < 3 && relativePositions[1] < 3)
                        {
                            entity.TargetAnimationId = 9;
                            direction = (uint)ScriptHelper.GetDirectionToTarget(
                                entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX,
                                entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
                            entity.TargetDirection = direction;
                            entity.AIValues[1] = 0x3c;
                            entity.Bytes[1] = 1;
                        }
                        else
                        {
                            if (((Random.Next() * 5) >> 0x20) == 0)
                            {
                                gameEngine.SoundManager.PlaySoundEffect(0xcd);
                            }

                            entity.TargetAnimationId = 5;
                            direction = (uint)ScriptHelper.GetDirectionToTarget(
                                gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                                gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                            entity.TargetDirection = direction;
                            entity.AIValues[1] = 0x12;
                        }
                    }
                    else if (entity.AIValues[1] == 0)
                    {
                        phase = entity.Bytes[0];
                        entity.Bytes[0] = (byte)(phase + 1);

                        if (phase == 0)
                        {
                            entity.AIValues[1] = 0x3c;
                            entity.TargetDirection = ((uint)(gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection] + 8)) & 0x1f;
                        }
                        else if (phase == 1)
                        {
                            entity.AIValues[1] = 0x3c;
                            entity.TargetDirection = ((uint)(gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection] + 0x10)) & 0x1f;
                        }
                        else if (phase == 2)
                        {
                            entity.TargetAnimationId = 9;
                            entity.TargetDirection = (uint)((Random.Next() * 0x20) >> 0x20);
                            entity.Bytes[1] = 0;
                            entity.AIValues[1] = (short)(((Random.Next() * 0x10) >> 0x20) + 0x3c);
                        }
                        else
                        {
                            entity.Bytes[0] = 0;
                        }
                    }
                }
                else
                {
                    entity.Bytes[2] = (byte)(entity.Bytes[2] - 1);
                }
            }
        }
        else if (entity.TargetAnimationId == 7)
        {
            if (entity.ForceResetAnimationFlag != 0)
            {
                if (entity.Bytes[3] == 0)
                {
                    entity.TargetAnimationId = 0;
                    entity.Bytes[2] = 0;
                    entity.Bytes[0] = 0;
                    entity.Bytes[1] = 2;
                }
                else
                {
                    entity.TargetAnimationId = 8;
                    entity.Flags |= 0x40;
                }
            }
        }
        else if (entity.TargetAnimationId == 9)
        {
            attackMode = entity.Bytes[1];
            entity.AIValues[1] = (short)(entity.AIValues[1] - 1);

            if (attackMode == 1
                && (entity.AIValues[1] == 0 || entity.ForceAdjusted != 0 || CanMoveForward(entity, 0) != 0))
            {
                entity.TargetAnimationId = 5;
                direction = (uint)ScriptHelper.GetDirectionToTarget(
                    gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                    gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                entity.TargetDirection = direction;
                entity.AIValues[1] = 0x12;
            }
            else if (entity.AIValues[1] == 0)
            {
                entity.TargetAnimationId = 0;
                entity.AIValues[1] = 0x3c;
            }
            else if (gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relativePositions, 1, 6, 0x200000))
            {
                if (relativePositions[0] > 2 || relativePositions[1] > 2)
                {
                    if (((Random.Next() * 5) >> 0x20) == 0)
                    {
                        gameEngine.SoundManager.PlaySoundEffect(0xcd);
                    }

                    entity.TargetAnimationId = 5;
                    direction = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.AIValues[1] = 0x12;
                }
                else
                {
                    entity.TargetAnimationId = 9;
                    direction = (uint)ScriptHelper.GetDirectionToTarget(
                        entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX,
                        entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
                    entity.TargetDirection = direction;
                    entity.AIValues[1] = 0x3c;
                    entity.Bytes[1] = 1;
                }
            }
            else
            {
                if (entity.ForceAdjusted == 0)
                {
                    gameEngine.EntityGameplayManager.HandleAnimationDirection(entity, 9, 0x300000);
                }
                else
                {
                    gameEngine.EntityGameplayManager.UpdateDirectionForced(entity, 9, 3, 0x300000);
                }
            }
        }

        if (entity.TargetAnimationId == 3 || entity.TargetAnimationId == 5)
        {
            entity.TargetDirection = (entity.TargetDirection + 2) & 0x1c;
        }
    }

    //8006e89c
    public static void AI_UpdateEntityAI_23_2(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Loup-garou Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        byte localCounter;
        short delay;
        uint direction;
        int[] relativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                direction = (uint)((Random.Next() * 3) >> 0x20);
                if (direction == 2)
                {
                    entity.Bytes[0] = (byte)(entity.Bytes[0] + 1);
                }

                entity.Bytes[0] = (byte)(entity.Bytes[0] + 1);
                entity.TargetAnimationId = 2;
                entity.Bytes[2] = entity.Bytes[0];
                direction = (uint)ScriptHelper.GetDirectionToTarget(
                    gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                    gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                entity.TargetDirection = direction;
                entity.AIValues[1] = 0;
                return;

            case 1:
                if (entity.Bytes[1] == 0)
                {
                    gameEngine.EffectManager.CreateEffectEntity(0, 7, 0, entity.PosX, entity.PosY, entity.PosZ);
                    entity.Bytes[1] = 10;
                }

                localCounter = entity.Bytes[1];
                delay = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = delay;
                entity.Bytes[1] = (byte)(localCounter - 1);

                if (delay == 0)
                {
                    if (entity.Bytes[0] == 0)
                    {
                        entity.TargetAnimationId = 0;
                    }
                    else
                    {
                        entity.TargetAnimationId = 2;
                    }

                    direction = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    return;
                }

                if (entity.ForceAdjusted == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 2;
                direction = gameEngine.StaticVariables.g_directionFlipTable[entity.TargetDirection];
                entity.ForceStepY = 0;
                entity.ForceStepX = 0;
                entity.ForceY = 0;
                entity.ForceX = 0;
                entity.TargetForceY = 0;
                entity.TargetForceX = 0;
                entity.TargetDirection = direction;
                return;

            case 2:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[2] != 0)
                {
                    entity.CurrentAnimationId = 0;
                    direction = (uint)ScriptHelper.GetDirectionToTarget(
                        gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                        gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                    entity.TargetDirection = direction;
                    entity.Bytes[2] = (byte)(entity.Bytes[2] - 1);
                    return;
                }

                entity.TargetAnimationId = 1;

                if (entity.AIValues[1] != 0)
                {
                    return;
                }

                entity.AIValues[1] = 0x19;
                direction = (uint)ScriptHelper.GetDirectionToTarget(
                    gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                    gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                entity.TargetDirection = direction;
                entity.Bytes[0] = (byte)(entity.Bytes[0] - 1);
                return;

            case 5:
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

                entity.TargetAnimationId = 0;
                entity.DamagedTickCounter = 0x5a;
                entity.Bytes[1] = 0;
                entity.Bytes[0] = 0;
                return;

            default:
                return;
        }
    }

    // GHIDRA: AI_UpdateEntityAI_Boss @ 0x8006FC7C
    public static void AI_UpdateEntityAI_Boss(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Boss lézard Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        short sVar3;
        int i;
        uint uVar4;
        var player = gameEngine.StaticVariables.PlayerEntity;
        int[] relPos = new int[6];

        i = entity.DelayOrAngleOrEntityId - 1;
        if (entity.DelayOrAngleOrEntityId != 0)
        {
            entity.DelayOrAngleOrEntityId = i;
            if (i == 0)
            {
                gameEngine.StaticVariables.g_scrollingParameters.Flag = 0;
                gameEngine.SoundManager.PlaySoundEffect(0x55);
            }
        }

        if (player.CurrentAnimationId == 0x5A && player.IsOnGround != 0)
        {
            player.TargetAnimationId = 0x59;
        }

        if (player.CurrentAnimationId == 0x59 && player.ForceResetAnimationFlag != 0)
        {
            player.TargetAnimationId = 0;
        }

        if (entity.Bytes[3] != 0 && entity.TargetAnimationId == 0)
        {
            if (entity.Bytes[3] < 4)
            {
                AI_Melzas2.UpdateEntityAI_BossExplode(gameEngine, entity);
                return;
            }

            entity.TargetAnimationId = 7;
            entity.Flags |= 0x40;
            return;
        }

        ScriptHelper.CalculateEntityRelativePosition(entity, player, relPos);

        switch (entity.TargetAnimationId)
        {
            case 0:
                sVar3 = entity.AIValues[1];
                if (sVar3 == 0)
                {
                    if (entity.ItemState != 0)
                    {
                        entity.ItemState--;
                    }

                    entity.AIValues[4] = 0;

                    if (relPos[0] < 3 && relPos[1] < 3 && relPos[2] < 0x100001)
                    {
                        var rand = (ulong)Random.Next();
                        if ((int)((rand * 3UL) >> 32) == 0)
                        {
                            rand = (ulong)Random.Next();
                            uVar4 = (uint)((rand * 0x20UL) >> 32);
                        }
                        else
                        {
                            uVar4 = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                        }

                        entity.TargetDirection = uVar4;
                        entity.TargetAnimationId = 0x12;
                    }
                    else if (entity.HpMax / 5 < entity.Hp)
                    {
                        var rand = (ulong)Random.Next();
                        entity.Bytes[0] = (byte)(((rand * 4UL) >> 32) + 2);

                        rand = (ulong)Random.Next();
                        entity.TargetAnimationId = 0x13;

                        if ((int)((rand * 3UL) >> 32) == 0)
                        {
                            rand = (ulong)Random.Next();
                            entity.TargetDirection = (uint)((rand * 0x20UL) >> 32);
                        }
                        else
                        {
                            entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                            entity.AIValues[4] = 1;
                        }
                    }
                    else
                    {
                        entity.TargetAnimationId = 0x0F;
                    }
                }
                else
                {
                    i = entity.ItemState;
                    entity.AIValues[1] = (short)(sVar3 - 1);

                    if (i == 0 && (ushort)(sVar3 - 1) < 0x10 && relPos[0] < 5 && relPos[1] < 5)
                    {
                        var rand = (ulong)Random.Next();
                        entity.TargetAnimationId = 3;
                        entity.ItemState = (int)((rand * 3UL) >> 32) + 3;
                    }
                }
                break;

            case 2:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0;
                }
                break;

            case 3:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 4;
                }
                break;

            case 5:
                if (entity.IsOnGround != 0)
                {
                    entity.TargetAnimationId = 6;
                    entity.Bytes[2] = 5;
                    entity.DelayOrAngleOrEntityId = 0x32;
                    gameEngine.SoundManager.PlaySoundEffect(0x55);
                    gameEngine.StaticVariables.g_scrollingParameters.Flag = 1;
                    gameEngine.StaticVariables.g_scrollingParameters.SpeedX = 1;
                    gameEngine.StaticVariables.g_scrollingParameters.SpeedY = 1;
                    gameEngine.StaticVariables.g_scrollingParameters.LimitX = 2;
                    gameEngine.StaticVariables.g_scrollingParameters.LimitY = 2;
                }
                break;

            case 6:
                if (entity.Bytes[2] != 0)
                {
                    entity.Bytes[2]--;

                    for (int slotIndex = 0; slotIndex < 0x40; slotIndex++)
                    {
                        Entity entity2 = gameEngine.StaticVariables.g_entitySlots[slotIndex];
                        if ((slotIndex == 0 || entity2.SpriteTableIndex == 0x16A || entity2.SpriteTableIndex == 0x173)
                            && entity2.IsOnGround != 0
                            && (entity2.AnimFlags & 0x40U) == 0
                            && entity2.DamagedTickCounter == 0)
                        {
                            entity2.ForceStepY = 0;
                            entity2.ForceStepX = 0;
                            entity2.ForceY = 0;
                            entity2.ForceX = 0;
                            entity2.TargetForceY = 0;
                            entity2.TargetForceX = 0;

                            if (slotIndex == 0)
                            {
                                player.TargetAnimationId = 0x5A;
                            }
                            else
                            {
                                entity2.TargetAnimationId = 0x0B;
                            }
                        }
                    }
                }

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                entity.AIValues[1] = 0x3C;
                entity.TargetAnimationId = 0;

                if ((int)(((ulong)Random.Next() * 4UL) >> 32) != 0)
                {
                    return;
                }

                entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                return;

            case 9:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[3] == 0)
                    {
                        entity.DamagedTickCounter = 0x5A;
                        entity.TargetAnimationId = 0;
                        entity.AIValues[1] = 0x1E;
                        entity.Bytes[2] = 0;
                    }
                    else
                    {
                        gameEngine.StaticVariables.g_temporaryFlags[0] |= 1;
                        entity.AIValues[1] = 300;
                        entity.AIValues[5] = 0x1E;
                        entity.TargetAnimationId = 0;
                        entity.Flags &= 0xFFFFFFFCU;
                    }
                }
                break;

            case 10:
                if (entity.Bytes[1] == 0)
                {
                    if (gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relPos, 2, 6, 0x300000))
                    {
                        entity.TargetAnimationId = 2;
                    }
                }
                else
                {
                    if (entity.AIValues[1] == 0)
                    {
                        entity.AIValues[1] = 0x3C;
                    }

                    sVar3 = (short)(entity.AIValues[1] - 1);
                    entity.AIValues[1] = sVar3;

                    if (sVar3 == 0)
                    {
                        entity.Hp += entity.HpMax / 10;
                        gameEngine.SoundManager.PlaySoundEffect(0x31);
                        gameEngine.EffectManager.CreateEffectEntity(0, 0x0E, 0, entity.PosX - 0x140000, entity.PosY, entity.PosZ + 0x200000);
                        gameEngine.EffectManager.CreateEffectEntity(0, 0x0E, 0, entity.PosX - 0x100000, entity.PosY, entity.PosZ + 0x3C0000);
                        gameEngine.EffectManager.CreateEffectEntity(0, 0x0E, 0, entity.PosX, entity.PosY, entity.PosZ + 0x460000);
                        gameEngine.EffectManager.CreateEffectEntity(0, 0x0E, 0, entity.PosX + 0x100000, entity.PosY, entity.PosZ + 0x400000);
                        gameEngine.EffectManager.CreateEffectEntity(0, 0x0E, 0, entity.PosX + 0x140000, entity.PosY, entity.PosZ + 0x1C0000);

                        byte bVar1 = (byte)(entity.Bytes[1] - 1);
                        entity.Bytes[1] = bVar1;
                        if (bVar1 == 0)
                        {
                            entity.TargetAnimationId = 2;
                        }
                    }
                }
                break;

            case 0x0C:
                if (entity.IsOnGround == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 6;
                break;

            case 0x0E:
                if (entity.IsOnGround == 0)
                {
                    return;
                }

                byte bVar2 = entity.Bytes[0];
                if (bVar2 == 0)
                {
                    entity.TargetAnimationId = 6;
                    break;
                }

                entity.Bytes[0] = (byte)(bVar2 - 1);
                if (bVar2 == 1)
                {
                    entity.TargetAnimationId = 6;
                    break;
                }

                entity.TargetAnimationId = 0x13;
                if (entity.AIValues[4] == 0)
                {
                    entity.TargetDirection += 8;
                    if (entity.ForceAdjusted == 0)
                    {
                        return;
                    }

                    if ((int)(((ulong)Random.Next() * 4UL) >> 32) != 0)
                    {
                        return;
                    }

                    entity.TargetDirection = (entity.TargetDirection + 8U) & 0x18U;
                    return;
                }

                entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                return;

            case 0x0F:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 10;
                    entity.Bytes[1] = 4;
                    entity.AIValues[1] = 0;
                }
                break;

            case 0x11:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 10;
                    entity.Bytes[1] = 0;
                }
                break;

            case 0x12:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0x0B;
                }
                break;

            case 0x13:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0x0D;
                }
                break;
        }
    }


    // GHIDRA: AI_UpdateEntityAI_BoosPhase3 @ 0x80071164
    public static void AI_UpdateEntityAI_BoosPhase3(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

        short sVar5;
        short sVar6;
        int iVar7;
        uint uVar9;
        Entity? pEVar8;
        var player = gameEngine.StaticVariables.PlayerEntity;
        int[] relPos = new int[6];
        int[] uintArray80027840 =
        {
            0x00FC0000, 0x01480000,
            0x00FC0000, 0x01A80000,
            0x02940000, 0x01480000,
            0x02940000, 0x01A80000
        };

        iVar7 = entity.ItemState - 1;
        if (entity.ItemState != 0)
        {
            entity.ItemState = iVar7;
            if (iVar7 == 0)
            {
                gameEngine.StaticVariables.g_scrollingParameters.Flag = 0;
            }
        }

        if (entity.Bytes[3] != 0 && entity.TargetAnimationId == 0)
        {
            if (entity.Bytes[3] < 4)
            {
                AI_Melzas2.UpdateEntityAI_BossExplode(gameEngine, entity);
                return;
            }

            entity.TargetAnimationId = 9;
            entity.Flags |= 0x40;
            return;
        }

        ScriptHelper.CalculateEntityRelativePosition(entity, player, relPos);

        switch (entity.TargetAnimationId)
        {
            case 0:
                sVar5 = entity.AIValues[1];
                if (sVar5 != 0)
                {
                    entity.AIValues[1] = (short)(sVar5 - 1);
                    if (sVar5 != 1)
                    {
                        return;
                    }
                }

                if (relPos[0] < 5 && relPos[1] < 5 && relPos[2] < 0x100001)
                {
                    var rand = (ulong)Random.Next();
                    uVar9 = (uint)((rand * 4UL) >> 32);
                    if (uVar9 == 1)
                    {
                        entity.TargetDirection = 0x10;
                        entity.TargetAnimationId = 1;
                        entity.Bytes[1] = 3;
                        return;
                    }

                    if (uVar9 == 0)
                    {
                        entity.TargetAnimationId = 5;
                        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                        entity.AIValues[1] = 0x3C;
                        entity.Bytes[1] = 1;
                        entity.Bytes[2] = 10;
                        return;
                    }

                    if (uVar9 == 3)
                    {
                        entity.TargetAnimationId = 3;
                        sVar5 = 0x22;
                        goto SetDelay;
                    }

                    rand = (ulong)Random.Next();
                    uVar9 = (uint)((rand * 0x20UL) >> 32);
                    entity.TargetAnimationId = 1;
                }
                else
                {
                    var rand = (ulong)Random.Next();
                    uVar9 = (uint)((rand * 4UL) >> 32);
                    if (uVar9 == 0)
                    {
                        entity.TargetAnimationId = 1;
                        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - player.PosX, entity.PosY - player.PosY);
                        entity.AIValues[1] = 0x50;
                        entity.Bytes[1] = 5;
                        return;
                    }

                    if (uVar9 == 2)
                    {
                        rand = (ulong)Random.Next();
                        entity.TargetAnimationId = 5;
                        entity.AIValues[1] = 0x1E;
                        entity.Bytes[1] = 7;
                        entity.Bytes[2] = 5;
                        entity.TargetDirection = (uint)(((rand * 4UL) >> 32) * 8 + 4);
                        return;
                    }

                    if (uVar9 != 1 && uVar9 != 3)
                    {
                        return;
                    }

                    rand = (ulong)Random.Next();
                    uVar9 = (uint)((rand * 0x20UL) >> 32);
                    entity.TargetAnimationId = 1;
                }

                entity.AIValues[1] = 0x3C;
                entity.Bytes[1] = 6;
                entity.Bytes[2] = 3;
                entity.TargetDirection = uVar9;
                break;

            case 1:
                if (entity.Bytes[1] == 5)
                {
                    sVar5 = (short)(entity.AIValues[1] - 1);
                    entity.AIValues[1] = sVar5;

                    if (sVar5 != 0 && entity.ForceAdjusted == 0)
                    {
                        return;
                    }

                    entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                    if (relPos[0] < 9 && relPos[1] < 9)
                    {
                        entity.TargetAnimationId = 3;
                        entity.AIValues[1] = 0x22;
                        entity.Bytes[1] = 4;
                        return;
                    }

                    sVar5 = 0x28;
                    goto SetAnim0;
                }

                if (entity.Bytes[1] == 3)
                {
                    ushort uVar4 = (ushort)(entity.AIValues[1] + 1);
                    entity.AIValues[1] = (short)uVar4;
                    if (uVar4 < 0x1E)
                    {
                        return;
                    }

                    sVar5 = 0x22;
                    if (entity.ForceAdjusted == 0)
                    {
                        return;
                    }

                    entity.TargetAnimationId = 3;
                    goto SetDelay;
                }

                if (entity.Bytes[1] == 6)
                {
                    sVar5 = (short)(entity.AIValues[1] - 1);
                    entity.AIValues[1] = sVar5;

                    if (sVar5 != 0 && entity.ForceAdjusted == 0)
                    {
                        return;
                    }

                    entity.Bytes[2]--;
                    if (entity.Bytes[2] == 0)
                    {
                        sVar5 = 0x3C;
                        goto SetAnim0;
                    }

                    var rand = (ulong)Random.Next();
                    entity.TargetDirection = (uint)((rand * 0x20UL) >> 32);

                    rand = (ulong)Random.Next();
                    sVar5 = (short)(((rand * 0x10UL) >> 32) + 0x28);
                    goto SetDelay;
                }

                sVar6 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar6;
                if (sVar6 != 0)
                {
                    return;
                }

                entity.TargetAnimationId = 0;
                sVar5 = 0x28;
                goto SetDelay;

            case 2:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[1] == 3)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            int baseIndex = i * 2;
                            gameEngine.SpawnWarpEntity(entity, 1, 0xFC, uintArray80027840[baseIndex], uintArray80027840[baseIndex + 1], entity.PosZ + 0x0F00000, 0);
                        }

                        entity.TargetAnimationId = 0;
                        entity.AIValues[1] = 0x0B4;
                        entity.DelayOrAngleOrEntityId += 4;
                    }
                    else
                    {
                        entity.Bytes[1]++;
                        entity.CurrentAnimationId = 0xFFFFFFFF;
                    }
                }
                break;

            case 3:
                sVar5 = entity.AIValues[1];
                if (sVar5 != 0)
                {
                    entity.AIValues[1] = (short)(sVar5 - 1);
                    if (sVar5 == 1)
                    {
                        uVar9 = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                        pEVar8 = gameEngine.SpawnWarpEntity(entity, 1, 0xFB, entity.PosX, entity.PosY, entity.PosZ + 0x300000, uVar9);

                        if (pEVar8 != null)
                        {
                            if (relPos[0] < 6 && relPos[1] < 6)
                            {
                                pEVar8.TargetAnimationId = 2;
                            }
                            else if (relPos[0] < 9 && relPos[1] < 9)
                            {
                                pEVar8.TargetAnimationId = 3;
                            }
                            else
                            {
                                pEVar8.TargetAnimationId = 4;
                            }
                        }
                    }
                }
                break;

            case 4:
                if (entity.IsOnGround == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 0;
                entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                sVar5 = 0x14;
                goto SetDelay;

            case 5:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = entity.Bytes[1] == 1 ? 10U : 6U;
                }
                break;

            case 6:
            case 10:
                if (entity.Bytes[1] == 1)
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                }
                else if (entity.Bytes[1] != 7)
                {
                    return;
                }

                if (entity.AIValues[1] != 0 && entity.ForceAdjusted == 0)
                {
                    return;
                }

                entity.Bytes[2]--;
                if (entity.Bytes[2] == 0)
                {
                    sVar5 = 0x3C;
                    goto SetAnim0;
                }

                entity.ForceStepY = 0;
                entity.ForceStepX = 0;
                entity.ForceY = 0;
                entity.ForceX = 0;
                entity.TargetForceY = 0;
                entity.TargetForceX = 0;

                if (entity.Bytes[1] == 7)
                {
                    int mirrorBase;
                    if (entity.ModdedPosX - 0x10000 < 0x1080001 || 0x287FFFF < entity.ModdedPosX + entity.Width + 0x10001)
                    {
                        mirrorBase = 0;
                    }
                    else
                    {
                        if (0x1000000 < entity.ModdedPosY - 0x10000 && entity.ModdedPosY + entity.Height + 0x10001 < 0x2000000)
                        {
                            return;
                        }

                        mirrorBase = 0x10;
                    }

                    entity.TargetDirection = (uint)((mirrorBase - (int)entity.TargetDirection) & 0x1F);
                    return;
                }

                entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                if ((int)(((ulong)Random.Next() * 3UL) >> 32) == 0)
                {
                    var rand = (ulong)Random.Next();
                    entity.TargetDirection = (uint)(((int)entity.TargetDirection + (int)((rand * 9UL) >> 32) - 4) & 0x1F);
                }

                sVar5 = 0x1E;
                goto SetDelay;

            case 8:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[3] != 0)
                {
                    entity.AIValues[1] = 300;
                    entity.AIValues[5] = 0x1E;
                    entity.TargetAnimationId = 0;
                    entity.Flags &= 0xFFFFFFFCU;
                    gameEngine.StaticVariables.g_scrollingParameters.Flag = 0;
                    return;
                }

                iVar7 = entity.HpMax;
                entity.DamagedTickCounter = 0x5A;
                if (iVar7 < 0)
                {
                    iVar7 += 3;
                }

                if (entity.Hp <= (iVar7 >> 2) && entity.DelayOrAngleOrEntityId < 4)
                {
                    entity.TargetAnimationId = 2;
                    entity.Bytes[1] = 0;
                    return;
                }

                sVar5 = 0x28;
                if ((int)(((ulong)Random.Next() * 4UL) >> 32) == 0)
                {
                    entity.TargetAnimationId = 3;
                    entity.AIValues[1] = 0x22;
                    sVar5 = 0x28;
                }

                goto SetAnim0;
        }

        return;

SetAnim0:
        entity.TargetAnimationId = 0;

SetDelay:
        entity.AIValues[1] = sVar5;
    }

    //80071BF4
    public static void AI_UpdateEntityAI_SpecialBoss(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

        short sVar3;
        int iVar4;
        int rand2;
        uint direction;
        Entity? pEVar5;
        var player = gameEngine.StaticVariables.PlayerEntity;
        int[] positions = new int[6];
        int[] dat80027890 =
        {
            0x009C0000, 0x02780000,
            0x02040000, 0x02780000,
            0x009C0000, 0x03980000,
            0x02040000, 0x03980000,
            0x01500000, 0x03000000
        };
        int[] dat800278b8 =
        {
            0, 0, 1,
            -0x300000, 0, 0x0F,
            0x300000, 0, 0x0F,
            0, -0x200000, 0x0F,
            0, 0x200000, 0x0F,
            -0x600000, 0, 0x1E,
            0x600000, 0, 0x1E,
            0, -0x400000, 0x1E,
            0, 0x400000, 0x1E,

            0, 0, 1,
            -0x300000, -0x200000, 0x0F,
            -0x300000, 0x200000, 0x0F,
            0x300000, -0x200000, 0x0F,
            0x300000, 0x200000, 0x0F,
            -0x600000, -0x400000, 0x1E,
            -0x600000, 0x400000, 0x1E,
            0x600000, -0x400000, 0x1E,
            0x600000, 0x400000, 0x1E,

            -0x600000, 0, 1,
            0x600000, 0, 1,
            0, -0x400000, 1,
            0, 0x400000, 1,
            -0x300000, 0, 0x0F,
            0x300000, 0, 0x0F,
            0, -0x200000, 0x0F,
            0, 0x200000, 0x0F,
            0, 0, 0x1E
        };
        int dat8019119c = gameEngine.StaticVariables.DAT_8019119c;

        if ((gameEngine.StaticVariables.g_temporaryFlags[0] & 1U) != 0)
        {
            return;
        }

        if (entity.Bytes[2] < 5)
        {
            iVar4 = entity.DelayOrAngleOrEntityId + 1;
            entity.DelayOrAngleOrEntityId = iVar4;
            if (iVar4 == 300)
            {
                int baseIndex = entity.Bytes[1] * 2;
                pEVar5 = gameEngine.SpawnWarpEntity(entity, 1, 0xFF, dat80027890[baseIndex], dat80027890[baseIndex + 1], 0x01500000, 0);
                pEVar5.TargetAnimationId = 4;

                byte bVar1 = entity.Bytes[2];
                byte bVar2 = (byte)(entity.Bytes[1] + 1);
                entity.DelayOrAngleOrEntityId = 0;
                entity.Bytes[1] = bVar2;
                entity.Bytes[2] = (byte)(bVar1 + 1);

                if (bVar2 > 4)
                {
                    entity.Bytes[1] = 0;
                }
            }
        }

        iVar4 = entity.ItemState - 1;
        if (entity.ItemState != 0)
        {
            entity.ItemState = iVar4;
            if ((iVar4 & 0x0F) == 0)
            {
                var rand = (ulong)Random.Next();
                int offsetIndex = (int)((rand * 0x20UL) >> 32);
                int radius = dat8019119c * 0x40 + 0xA00;

                rand = (ulong)Random.Next();
                direction = (uint)((rand * 0x20UL) >> 32);

                pEVar5 = gameEngine.SpawnWarpEntity(
                    entity,
                    1,
                    0xD5,
                    entity.PosX + gameEngine.StaticVariables.g_offsetXList[offsetIndex] * radius,
                    entity.PosY + gameEngine.StaticVariables.g_offsetYList[offsetIndex] * radius,
                    entity.PosZ + 0x1000000,
                    direction);

                if (entity.PosZ + 0x1000000 < pEVar5.TerrainHeight)
                {
                    gameEngine.DestroyEntity(pEVar5);
                }
                else
                {
                    gameEngine.SoundManager.PlaySoundEffect(0xB7);
                    pEVar5.ForceZ = -0x80000;
                }

                dat8019119c += 8;
                gameEngine.StaticVariables.DAT_8019119c = dat8019119c;
            }
        }

        FUN_8007fe8c(gameEngine, entity, positions);

        switch (entity.TargetAnimationId)
        {
            case 0:
                sVar3 = entity.AIValues[1];
                if (sVar3 != 0)
                {
                    entity.AIValues[1] = (short)(sVar3 - 1);
                    if (sVar3 != 1)
                    {
                        return;
                    }
                }

                if (positions[0] > 2 || positions[1] > 2 || positions[2] > 0x200000)
                {
                    if (positions[0] < 6 && positions[1] < 6 && positions[2] < 0x200001)
                    {
                        entity.TargetAnimationId = 3;
                        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                        entity.AIValues[1] = 0x3A;
                        return;
                    }

                    if ((int)(((ulong)Random.Next() * 3UL) >> 32) == 0 && entity.ItemState == 0)
                    {
                        entity.TargetAnimationId = 4;
                        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                        break;
                    }
                }

                goto SetAnim6;

            case 1:
                byte bVar3 = entity.Bytes[0];
                sVar3 = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = sVar3;

                if (bVar3 == 0)
                {
                    if (sVar3 != 0 && entity.ForceAdjusted == 0)
                    {
                        return;
                    }

                    if ((int)(((ulong)Random.Next() * 4UL) >> 32) == 0)
                    {
                        entity.TargetAnimationId = 7;
                        return;
                    }

                    var rand = (ulong)Random.Next();
                    entity.TargetDirection = 0;
                    entity.TargetAnimationId = 2;
                    entity.AIValues[1] = (short)(((rand * 0x10UL) >> 32) + 0x1E);
                    return;
                }

                if (sVar3 == 0 || entity.ForceAdjusted != 0)
                {
                    entity.Bytes[0] = (byte)(bVar3 - 1);
                    if (bVar3 != 1)
                    {
                        gameEngine.EntityGameplayManager.StartFlying(entity, 1, 0x32, 0x32);
                        return;
                    }

                    var rand = (ulong)Random.Next();
                    entity.TargetAnimationId = 7;
                    if ((int)((rand * 3UL) >> 32) == 0)
                    {
                        return;
                    }

                    iVar4 = entity.PosX - player.PosX;
                    rand2 = entity.PosY - player.PosY;
                }
                else
                {
                    if (positions[0] > 3 || positions[1] > 3)
                    {
                        return;
                    }

                    var rand = (ulong)Random.Next();
                    entity.TargetAnimationId = 7;
                    if ((int)((rand * 3UL) >> 32) != 0)
                    {
                        rand = (ulong)Random.Next();
                        entity.TargetDirection = (uint)((rand * 0x20UL) >> 32);
                        return;
                    }

                    iVar4 = player.PosX - entity.PosX;
                    rand2 = player.PosY - entity.PosY;
                }

                entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(iVar4, rand2);
                break;

            case 2:
                sVar3 = entity.AIValues[1];
                if (sVar3 == 0 || sVar3 == 1)
                {
                    if (sVar3 == 1)
                    {
                        entity.AIValues[1] = 0;
                    }

                    if (positions[0] < 6 && positions[1] < 6 && positions[5] >= 0 && positions[5] < 0x600001)
                    {
                        entity.Bytes[0] = 3;
                        entity.TargetAnimationId = 1;
                        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                        entity.AIValues[1] = 0x3C;
                    }
                    else
                    {
                        entity.Bytes[0] = 0;
                        gameEngine.EntityGameplayManager.SetEntityRandomDirection(entity, 1, 0x3C);
                    }

                    if (entity.PosZ - entity.TerrainHeight > 0x600000)
                    {
                        entity.Bytes[0] = 1;
                    }
                }
                else
                {
                    entity.AIValues[1] = (short)(sVar3 - 1);
                }
                break;

            case 3:
                sVar3 = entity.AIValues[1];
                if (sVar3 != 0)
                {
                    entity.AIValues[1] = (short)(sVar3 - 1);
                    if (sVar3 == 1)
                    {
                        gameEngine.SoundManager.PlaySoundEffect(0xB9);

                        int patternBase = (int)(((ulong)Random.Next() * 3UL) >> 32) * 27;
                        for (int i = 0; i < 9; i++)
                        {
                            int tri = patternBase + i * 3;
                            pEVar5 = gameEngine.SpawnWarpEntity(
                                entity,
                                1,
                                0xFE,
                                player.PosX + dat800278b8[tri],
                                player.PosY + dat800278b8[tri + 1],
                                player.TerrainHeight,
                                0);

                            pEVar5.TargetAnimationId = 4;
                            pEVar5.AIValues[1] = (short)dat800278b8[tri + 2];
                        }
                    }
                }

                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 6;
                }
                break;

            case 4:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 5;
                    entity.AIValues[1] = 0x1C;
                }
                break;

            case 5:
                sVar3 = entity.AIValues[1];
                if (sVar3 != 0)
                {
                    entity.AIValues[1] = (short)(sVar3 - 1);
                    if (sVar3 == 1)
                    {
                        entity.ItemState = 0x100;
                        dat8019119c = 0;
                        gameEngine.StaticVariables.DAT_8019119c = 0;
                        gameEngine.InitializeAndBeginFadeEffect();
                    }
                }

                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 6;
                }
                break;

            case 6:
                if (entity.PosZ - entity.TerrainHeight > 0x4FFFFF)
                {
                    var rand = (ulong)Random.Next();
                    entity.ForceZ = 0;
                    entity.TargetAnimationId = 2;
                    entity.AIValues[1] = (short)(((rand * 0x10UL) >> 32) + 0x3C);
                }
                break;

            case 7:
                if (entity.IsOnGround != 0)
                {
                    entity.ForceZ = 0;
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0x0F;
                }
                break;

            case 9:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[3] == 0)
                    {
                        entity.TargetAnimationId = 6;
                        entity.DamagedTickCounter = 0x5A;
                    }
                    else
                    {
                        gameEngine.StaticVariables.g_temporaryFlags[0] |= 1;
                        entity.TargetAnimationId = 0;
                        entity.ForceZ = 0;
                    }
                }
                break;
        }

        return;

SetAnim6:
        entity.TargetAnimationId = 6;
    }

    // GHIDRA: AI_UpdateEntityIA_WatcherBehavior @ 0x8007252C
    public static void AI_UpdateEntityIA_WatcherBehavior(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

        short sVar1;
        var player = gameEngine.StaticVariables.PlayerEntity;

        if ((gameEngine.StaticVariables.g_temporaryFlags[0] & 1U) == 0)
        {
            if (entity.TargetAnimationId == 0)
            {
                sVar1 = entity.AIValues[1];
                if (sVar1 == 0)
                {
                    entity.TargetAnimationId = 2;
                    entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                }
                else
                {
                    entity.AIValues[1] = (short)(sVar1 - 1);
                    if (sVar1 == 1)
                    {
                        entity.TargetAnimationId = 2;
                        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                    }
                }
            }
            else if (entity.TargetAnimationId == 2)
            {
                if (entity.AIValues[1] == 0)
                {
                    entity.AIValues[1] = 0x30;
                }
                else
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);

                    if ((entity.AIValues[1] & 7) == 0)
                    {
                        Entity spawnedEntity = gameEngine.SpawnWarpEntity(
                            entity,
                            1,
                            0xFF,
                            entity.PosX,
                            entity.PosY,
                            entity.PosZ,
                            gameEngine.StaticVariables.BYTE_ARRAY_80028b54[entity.AnimationDirection]);

                        if (spawnedEntity != null)
                        {
                            spawnedEntity.TargetAnimationId = 3;
                            spawnedEntity.SpriteProgramIndexes[4] = 0x19;
                        }
                    }

                    if (entity.AIValues[1] == 0)
                    {
                        entity.TargetAnimationId = 0;
                        entity.AIValues[1] = 0x1E;
                    }
                }
            }
        }
        else
        {
            entity.TargetAnimationId = 1;
            entity.Flags |= 0x40;
        }
    }

    // GHIDRA: AI_UpdateEntityAI_TwinBoss @ 0x80072728
    public static void AI_UpdateEntityDelayedSoundTrigger(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

        var player = gameEngine.StaticVariables.PlayerEntity;
        Entity? otherEntity = null;

        for (int i = 0; i < gameEngine.StaticVariables.g_numberOfEntities; i++)
        {
            var candidate = gameEngine.StaticVariables.g_entitySlots[i];

            if (candidate.Index == entity.AIValues[2])
            {
                otherEntity = candidate;
                break;
            }
        }

        if (otherEntity == null)
        {
            Breakpoint.TriggerBreak();
            return;
        }

        int[] entityRelativePositions = new int[6];
        int[] otherEntityRelativePositions = new int[6];

        ScriptHelper.CalculateEntityRelativePosition(entity, player, entityRelativePositions);
        ScriptHelper.CalculateEntityRelativePosition(otherEntity, player, otherEntityRelativePositions);

        Entity leftEntity;
        Entity rightEntity;
        int[] leftRelativePositions;
        int[] rightRelativePositions;

        if (entity.PosX < otherEntity.PosX)
        {
            entity.Bytes[2] = 0;
            leftEntity = entity;
            rightEntity = otherEntity;
            leftRelativePositions = entityRelativePositions;
            rightRelativePositions = otherEntityRelativePositions;
        }
        else
        {
            entity.Bytes[2] = 1;
            leftEntity = otherEntity;
            rightEntity = entity;
            leftRelativePositions = otherEntityRelativePositions;
            rightRelativePositions = entityRelativePositions;
        }

        bool resetForces = false;

        uint[] mirroredAnimationIds =
        {
            3, 2, 1, 0,
            5, 4,
            9, 10, 11,
            6, 7, 8,
            15, 16, 17,
            12, 13, 14,
            19, 18,
            21, 20,
            0, 0
        };

        if (entity.Bytes[3] != 0)
        {
            uint targetAnimationId = entity.TargetAnimationId;

            if (targetAnimationId == 3 || targetAnimationId == 0)
            {
                if (entity.Bytes[3] < 4)
                {
                    AI_Melzas2.UpdateEntityAI_BossExplode(gameEngine, entity);
                    AI_Melzas2.UpdateEntityAI_BossExplode(gameEngine, otherEntity);
                    return;
                }

                entity.TargetAnimationId = 0x0B;
                entity.Flags |= 0x40;
                otherEntity.ForceResetAnimationFlag = 0;
                otherEntity.Flags |= 0x40;
                return;
            }
        }

        uint currentAnimationId = entity.TargetAnimationId;

        if (currentAnimationId > 0x15)
        {
            if (entity.Bytes[2] == 0)
            {
                otherEntity.TargetAnimationId = mirroredAnimationIds[(int)entity.TargetAnimationId];
            }
            else
            {
                uint current = entity.TargetAnimationId;
                otherEntity.TargetAnimationId = current;
                entity.TargetAnimationId = mirroredAnimationIds[(int)current];
            }

            otherEntity.TargetDirection = (0x20U - entity.TargetDirection) & 0x1F;
            otherEntity.PosY = entity.PosY;
            otherEntity.TargetForceY = entity.TargetForceY;
            otherEntity.ForceY = entity.ForceY;
            otherEntity.ForceStepY = entity.ForceStepY;
            otherEntity.TargetForceX = -entity.TargetForceX;
            otherEntity.ForceX = -entity.ForceX;
            otherEntity.ForceStepX = -entity.ForceStepX;
            return;
        }

        switch (currentAnimationId)
        {
            case 0:
            case 3:
            {
                ushort delay = (ushort)entity.AIValues[1];
                entity.TargetAnimationId = 3;

                if (delay != 0)
                {
                    delay--;
                    entity.AIValues[1] = (short)delay;

                    if (delay != 0)
                    {
                        break;
                    }
                }

                entity.TargetAnimationId = 2;

                if (rightEntity.PosX - leftEntity.PosX <= 0x480000)
                {
                    if (((Random.Next() * 2) >> 0x20) != 0)
                    {
                        uint direction = (uint)((Random.Next() * 9) >> 0x20);

                        if (leftEntity == entity)
                        {
                            direction += 4;
                        }
                        else
                        {
                            direction += 0x14;
                        }

                        entity.TargetDirection = direction;
                        entity.Bytes[0] = 1;
                        entity.AIValues[1] = 0x50;
                        entity.Bytes[1] = 7;
                        break;
                    }
                }

                switch ((int)((Random.Next() * 6) >> 0x20))
                {
                    case 0:
                        entity.Bytes[0] = 1;
                        entity.Bytes[1] = 1;
                        entity.TargetDirection = gameEngine.StaticVariables.BYTE_ARRAY_80028b54[(int)((Random.Next() * 2) >> 0x20)];
                        entity.AIValues[1] = (short)(((Random.Next() * 0x15) >> 0x20) + 0x64);
                        break;

                    case 1:
                        entity.Bytes[0] = 2;
                        entity.Bytes[1] = 2;
                        entity.TargetDirection = (uint)((Random.Next() * 0x20) >> 0x20);
                        break;

                    case 2:
                    case 4:
                        entity.Bytes[0] = 5;

                        if (((Random.Next() * 2) >> 0x20) != 0)
                        {
                            entity.TargetDirection = (uint)((Random.Next() * 0x20) >> 0x20);
                        }
                        else
                        {
                            entity.TargetDirection = entityRelativePositions[4] < 0 ? 0U : 0x10U;
                        }

                        entity.AIValues[1] = 0x3C;
                        entity.Bytes[1] = 5;
                        break;

                    case 3:
                    case 5:
                        entity.Bytes[0] = 5;
                        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                        entity.AIValues[1] = 0x3C;
                        entity.Bytes[1] = 4;
                        break;
                }

                break;
            }

            case 1:
            case 2:
            {
                entity.TargetAnimationId = 2;

                bool leftCanLineUp =
                    leftRelativePositions[1] <= 4 &&
                    leftRelativePositions[3] >= 0 &&
                    2 <= leftRelativePositions[0] &&
                    leftRelativePositions[0] <= 6;

                bool rightCanLineUp =
                    rightRelativePositions[1] <= 4 &&
                    rightRelativePositions[3] <= 0 &&
                    2 <= rightRelativePositions[0] &&
                    rightRelativePositions[0] <= 6;

                if (leftCanLineUp || rightCanLineUp)
                {
                    Entity attackEntity = leftCanLineUp ? leftEntity : rightEntity;
                    uint direction = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - attackEntity.PosX, player.PosY - attackEntity.PosY);

                    entity.TargetAnimationId = 0x15;
                    entity.Bytes[0] = 1;
                    entity.AIValues[1] = 0x3C;
                    entity.TargetDirection = attackEntity == entity ? direction : (0x20U - direction) & 0x1F;
                    break;
                }

                switch (entity.Bytes[1])
                {
                    case 0:
                    case 7:
                    {
                        ushort delay = (ushort)entity.AIValues[1];
                        delay--;
                        entity.AIValues[1] = (short)delay;

                        if (delay != 0 && entity.ForceAdjusted == 0 && otherEntity.ForceAdjusted == 0)
                        {
                            break;
                        }

                        resetForces = true;
                        entity.TargetAnimationId = 3;
                        entity.AIValues[1] = 0x28;
                        break;
                    }

                    case 1:
                    case 4:
                    case 5:
                    {
                        ushort delay = (ushort)entity.AIValues[1];
                        delay--;
                        entity.AIValues[1] = (short)delay;

                        if (delay == 0)
                        {
                            byte count = (byte)(entity.Bytes[0] - 1);
                            entity.Bytes[0] = count;

                            if (count == 0)
                            {
                                entity.TargetAnimationId = 3;
                                entity.AIValues[1] = 0x1E;
                                break;
                            }

                            if (entity.Bytes[1] == 4)
                            {
                                entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                            }
                            else
                            {
                                if (((Random.Next() * 3) >> 0x20) != 0 && rightEntity.TileX - leftEntity.TileX >= 0x0B)
                                {
                                    uint direction = (uint)((Random.Next() * 0x0F) >> 0x20);

                                    if (leftEntity == entity)
                                    {
                                        entity.TargetDirection = direction + 0x11;
                                    }
                                    else
                                    {
                                        entity.TargetDirection = direction + 1;
                                    }
                                }
                                else
                                {
                                    entity.TargetDirection = (uint)((Random.Next() * 0x20) >> 0x20);
                                }
                            }

                            entity.AIValues[1] = 0x3C;
                            resetForces = true;
                            break;
                        }

                        if (gameEngine.EntityGameplayManager.TryAttackPlayer2(leftEntity, leftRelativePositions, 3, 1, 5, 0x100000) ||
                            gameEngine.EntityGameplayManager.TryAttackPlayer2(rightEntity, rightRelativePositions, 2, 1, 5, 0x100000))
                        {
                            entity.TargetAnimationId = 5;
                            entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                            break;
                        }

                        if (rightEntity.TileX - leftEntity.TileX >= 4)
                        {
                            bool leftCanAdvance =
                                leftRelativePositions[1] <= 4 &&
                                leftRelativePositions[3] <= 0 &&
                                2 <= leftRelativePositions[0] &&
                                leftRelativePositions[0] <= 5;

                            bool rightCanAdvance =
                                rightRelativePositions[1] <= 4 &&
                                rightRelativePositions[3] >= 0 &&
                                2 <= rightRelativePositions[0] &&
                                rightRelativePositions[0] <= 5;

                            if (leftCanAdvance && rightCanAdvance)
                            {
                                entity.TargetAnimationId = 0x15;
                                entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY);
                                entity.AIValues[1] = 0x28;
                                entity.Bytes[0] = (byte)(((Random.Next() * 3) >> 0x20) + 1);
                                break;
                            }
                        }

                        if (entity.ForceAdjusted == 0 && otherEntity.ForceAdjusted == 0)
                        {
                            break;
                        }

                        resetForces = true;
                        entity.TargetDirection = (entity.TargetDirection + 0x10U) & 0x1F;
                        break;
                    }

                    case 2:
                    {
                        if (entity.ForceAdjusted == 0 && otherEntity.ForceAdjusted == 0)
                        {
                            break;
                        }

                        byte count = (byte)(entity.Bytes[0] - 1);
                        entity.Bytes[0] = count;

                        if (count == 0)
                        {
                            entity.TargetAnimationId = 3;
                            entity.AIValues[1] = 0x1E;
                            break;
                        }

                        entity.TargetDirection = (entity.TargetDirection + (uint)(((Random.Next() * 7) >> 0x20) + 9)) & 0x1F;
                        entity.AIValues[1] = 0x3C;
                        resetForces = true;
                        break;
                    }
                }

                break;
            }

            case 4:
            case 5:
                entity.TargetAnimationId = 5;

                if (entity.ForceResetAnimationFlag != 0)
                {
                    gameEngine.EntityGameplayManager.StartFlying(entity, 2, 0x1E, 0x3C);
                    entity.Bytes[1] = 0;
                }

                break;

            case 6:
            case 9:
                entity.TargetAnimationId = 9;
                break;

            case 7:
            case 10:
                entity.TargetAnimationId = 0x0A;

                if (entity.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[3] != 0)
                    {
                        entity.TargetAnimationId = 3;
                        entity.Flags &= 0xFFFFFFFCU;
                        otherEntity.Flags &= 0xFFFFFFFCU;
                        entity.AIValues[1] = 0x12C;
                        otherEntity.AIValues[1] = 0x12C;
                        entity.AIValues[5] = 0x1E;
                        otherEntity.AIValues[5] = 0x1E;
                        break;
                    }

                    entity.DamagedTickCounter = 0x5A;
                    otherEntity.DamagedTickCounter = 0x5A;

                    if (rightEntity.TileX - leftEntity.TileX >= 0x0E &&
                        entityRelativePositions[0] < 3 &&
                        otherEntityRelativePositions[0] < 3)
                    {
                        entity.TargetAnimationId = 0x15;
                        entity.TargetDirection = entityRelativePositions[4] < 0 ? 0U : 0x10U;
                        entity.Bytes[0] = 1;
                        entity.AIValues[1] = 0x28;
                        break;
                    }

                    entity.TargetAnimationId = 3;
                    entity.AIValues[1] = 0x0A;
                    entity.Bytes[1] = 0;
                    entity.Bytes[0] = 0;
                }

                break;

            case 8:
            case 11:
                entity.TargetAnimationId = 0x0B;
                break;

            case 12:
            case 15:
                entity.TargetAnimationId = 0x0F;

                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0x10;
                    entity.AIValues[1] = 0x00B4;

                    Entity spawned = gameEngine.SpawnWarpEntity(
                        leftEntity,
                        1,
                        0x00D6,
                        leftEntity.PosX + 0x00300000,
                        leftEntity.PosY,
                        leftEntity.PosZ + 0x000C0000,
                        0x18);

                    if (spawned != null)
                    {
                        spawned.TargetAnimationId = 1;
                    }

                    spawned = gameEngine.SpawnWarpEntity(
                        rightEntity,
                        1,
                        0x00D6,
                        rightEntity.PosX - 0x00300000,
                        rightEntity.PosY,
                        rightEntity.PosZ + 0x000C0000,
                        0);

                    if (spawned != null)
                    {
                        spawned.TargetAnimationId = 1;
                    }
                }

                break;

            case 13:
            case 16:
            {
                ushort delay = (ushort)entity.AIValues[1];
                delay--;
                entity.AIValues[1] = (short)delay;
                entity.TargetAnimationId = 0x10;

                if (delay % 0x0F == 0)
                {
                    entity.TargetDirection = entityRelativePositions[4] < 0 ? 0U : 0x10U;
                }

                if (delay == 0)
                {
                    entity.TargetAnimationId = 3;
                    entity.AIValues[1] = 0x28;
                }

                break;
            }

            case 14:
            case 17:
                break;

            case 18:
            case 19:
            {
                ushort delay = (ushort)entity.AIValues[1];
                entity.TargetAnimationId = 0x13;
                delay--;
                entity.AIValues[1] = (short)delay;

                if ((delay & 7) == 0)
                {
                    Entity spawned = gameEngine.SpawnWarpEntity(
                        leftEntity,
                        1,
                        0x00F4,
                        leftEntity.PosX,
                        leftEntity.PosY,
                        leftEntity.PosZ,
                        0x18);

                    if (spawned != null)
                    {
                        spawned.SpriteProgramIndexes[ScriptHelper.ProgramEDeactivate] = 0x19;
                        spawned.SpriteProgramIndexes[ScriptHelper.ProgramALoad] = 0;
                        spawned.SpriteProgramIndexes[ScriptHelper.ProgramCTick] = 0;
                        spawned.TargetAnimationId = 0x11;
                        spawned.Flags |= 0x40;
                    }

                    spawned = gameEngine.SpawnWarpEntity(
                        rightEntity,
                        1,
                        0x00F4,
                        rightEntity.PosX,
                        rightEntity.PosY,
                        rightEntity.PosZ,
                        8);

                    if (spawned != null)
                    {
                        spawned.SpriteProgramIndexes[ScriptHelper.ProgramEDeactivate] = 0x19;
                        spawned.SpriteProgramIndexes[ScriptHelper.ProgramALoad] = 0;
                        spawned.SpriteProgramIndexes[ScriptHelper.ProgramCTick] = 0;
                        spawned.TargetAnimationId = 0x0E;
                        spawned.Flags |= 0x40;
                    }
                }

                if (delay != 0 && entity.ForceAdjusted == 0)
                {
                    break;
                }

                byte count = (byte)(entity.Bytes[0] - 1);
                entity.Bytes[0] = count;

                if (count != 0)
                {
                    if (gameEngine.EntityGameplayManager.TryAttackPlayer2(leftEntity, leftRelativePositions, 3, 4, 5, 0x100000) ||
                        gameEngine.EntityGameplayManager.TryAttackPlayer2(rightEntity, rightRelativePositions, 2, 4, 5, 0x100000))
                    {
                        entity.TargetDirection = ((uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity.PosX, player.PosY - entity.PosY) + 2) & 0x1C;
                        entity.AIValues[1] = 0x3C;
                        break;
                    }
                }

                entity.TargetAnimationId = 2;

                uint direction = (uint)((Random.Next() * 9) >> 0x20);

                if (leftEntity == entity)
                {
                    direction += 4;
                }
                else
                {
                    direction += 0x14;
                }

                entity.TargetDirection = direction;
                entity.AIValues[1] = 0x3C;
                entity.Bytes[1] = 0;
                break;
            }

            case 20:
            case 21:
                entity.TargetAnimationId = 0x15;

                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0x13;
                }

                break;
        }

        if (resetForces)
        {
            entity.ForceStepY = 0;
            entity.ForceStepX = 0;
            entity.ForceY = 0;
            entity.ForceX = 0;
            entity.TargetForceY = 0;
            entity.TargetForceX = 0;
        }

        if (entity.Bytes[2] == 0)
        {
            otherEntity.TargetAnimationId = mirroredAnimationIds[(int)entity.TargetAnimationId];
        }
        else
        {
            uint current = entity.TargetAnimationId;
            otherEntity.TargetAnimationId = current;
            entity.TargetAnimationId = mirroredAnimationIds[(int)current];
        }

        otherEntity.TargetDirection = (0x20U - entity.TargetDirection) & 0x1F;
        otherEntity.PosY = entity.PosY;
        otherEntity.TargetForceY = entity.TargetForceY;
        otherEntity.ForceY = entity.ForceY;
        otherEntity.ForceStepY = entity.ForceStepY;
        otherEntity.TargetForceX = -entity.TargetForceX;
        otherEntity.ForceX = -entity.ForceX;
        otherEntity.ForceStepX = -entity.ForceStepX;
    }

    // GHIDRA: FUN_800737D0 @ 0x800737D0
    private static void FUN_800737D0(GameEngine gameEngine, Entity entity)
    {
        var entitySlots = gameEngine.StaticVariables.g_entitySlots;

        switch (entity.Bytes[2])
        {
            case 0:
                entity.AIValues[1] = 0x1E;
                entity.Bytes[1] = 4;
                return;

            case 1:
            {
                int entityIndex = Array.IndexOf(entitySlots, entity);
                if (entityIndex >= 0 && entityIndex + 14 < entitySlots.Length)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        Entity current = entitySlots[entityIndex + i];
                        current.DelayOrAngleOrEntityId = (int)current.TargetDirection << 4;
                    }
                }

                entity.AIValues[1] = 0x3C;
                entity.Bytes[1] = 5;
                entity.Bytes[2] = 0;
                return;
            }

            case 2:
                entity.TargetAnimationId = 2;
                entity.AIValues[1] = 4;
                entity.Bytes[1] = 6;

                if (entity.TargetDirection < 8U)
                {
                    entity.DelayOrAngleOrEntityId = 0;
                    entity.TargetDirection = 0;
                    entity.ItemState = 1;
                }
                else if (entity.TargetDirection < 0x10U)
                {
                    entity.DelayOrAngleOrEntityId = 0x10;
                    entity.TargetDirection = 0x10;
                    entity.ItemState = -1;
                }
                else if (entity.TargetDirection < 0x18U)
                {
                    entity.DelayOrAngleOrEntityId = 0x10;
                    entity.TargetDirection = 0x10;
                    entity.ItemState = 1;
                }
                else
                {
                    entity.DelayOrAngleOrEntityId = 0;
                    entity.TargetDirection = 0;
                    entity.ItemState = -1;
                }

                return;

            case 3:
            {
                Entity spawnedEntity = gameEngine.SpawnWarpEntity(entity, 1, 0xF8, entity.PosX, entity.PosY, entity.PosZ, entity.TargetDirection);
                spawnedEntity.AIValues[1] = 0x230;
                entity.AIValues[1] = 0x3C;
                entity.Bytes[1] = 4;
                return;
            }
        }
    }

    // GHIDRA: FUN_80073BD4 @ 0x80073BD4
    private static void UpdateWarpSpawnBehavior(GameEngine gameEngine, Entity entity, int param_2, int param_3)
    {
        entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
        if ((ushort)entity.AIValues[1] != 0)
        {
            return;
        }

        int targetY = param_2 + (param_3 != 0 ? 0x00700000 : unchecked((int)0xFF900000));
        entity.AIValues[1] = 6;
        int oldByte0 = entity.Bytes[0];
        entity.Bytes[0] = (byte)(oldByte0 + 1);

        int spawnIndex = oldByte0 - 1;
        if ((uint)spawnIndex >= 14U)
        {
            return;
        }

        uint spriteTableIndex = spawnIndex switch
        {
            0 => 0xEBU,
            1 => 0xECU,
            2 => 0xEBU,
            3 => 0xE9U,
            4 => 0xEBU,
            5 => 0xECU,
            6 => 0xEBU,
            7 => 0xE9U,
            8 => 0xEBU,
            9 => 0xECU,
            10 => 0xEBU,
            11 => 0xE9U,
            12 => 0xEBU,
            13 => 0xEAU,
            _ => 0U,
        };

        if (spawnIndex == 6)
        {
            gameEngine.StaticVariables.g_temporaryFlags[0] |= 2U;
        }

        int entityIndex = Array.IndexOf(gameEngine.StaticVariables.g_entitySlots, entity);
        if (entityIndex < 0)
        {
            return;
        }

        int targetIndex = entityIndex + oldByte0;
        if ((uint)targetIndex >= gameEngine.StaticVariables.g_entitySlots.Length)
        {
            return;
        }

        gameEngine.SpawnChildEntity(gameEngine.StaticVariables.g_entitySlots[targetIndex], entity, 1, spriteTableIndex, entity.PosX, targetY, entity.PosZ, (uint)param_3);
    }

    // GHIDRA: AI_UpdateEntityAI_0_00 @ 0x80073CFC
    public static void AI_UpdateEntityAI_0_00(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Mille-pattes (corps principal)")
        {
            Breakpoint.TriggerBreak();
        }

        var entitySlots = gameEngine.StaticVariables.g_entitySlots;
        var player = gameEngine.StaticVariables.PlayerEntity;
        int entityIndex = Array.IndexOf(entitySlots, entity);
        int[] relPos = new int[6];

        if (gameEngine.StaticVariables.g_loaderInitialized == 0)
        {
            gameEngine.StaticVariables.DAT_801911b4 = 0;
            gameEngine.StaticVariables.DAT_801911b0 = 0;
            gameEngine.StaticVariables.DAT_801911ac = 0;
            gameEngine.StaticVariables.DAT_801911a8 = 0;
            gameEngine.StaticVariables.DAT_801911a4 = 0;
            gameEngine.StaticVariables.DAT_801911a0 = 0;
            gameEngine.StaticVariables.g_loaderInitialized = 1;
        }

        ScriptHelper.CalculateEntityRelativePosition(entity, player, relPos);

        if (entity.FinalForceX != 0 || entity.FinalForceY != 0)
        {
            int historyIndex = gameEngine.StaticVariables.DAT_801911a0;
            gameEngine.StaticVariables.g_loaderDirectionHistory[historyIndex] = (short)entity.TargetDirection;
            gameEngine.StaticVariables.DAT_801911a0 = (historyIndex + 1) & 0xFF;
            gameEngine.StaticVariables.DAT_80191508[historyIndex] = (short)(entity.PosX >> 16);
            gameEngine.StaticVariables.DAT_80191708[historyIndex] = (short)(entity.PosY >> 16);
        }

        if (gameEngine.StaticVariables.DAT_801911b4 == 0)
        {
            int historyIndex = (gameEngine.StaticVariables.DAT_801911a0 - 0x54) & 0xFF;

            for (int i = 0; i < 14; i++)
            {
                Entity follower = entitySlots[16 - i];
                if (follower.Status == 2)
                {
                    follower.TargetDirection = (uint)(ushort)gameEngine.StaticVariables.g_loaderDirectionHistory[historyIndex];
                    follower.PosX = gameEngine.StaticVariables.DAT_80191508[historyIndex] << 16;
                    follower.PosY = gameEngine.StaticVariables.DAT_80191708[historyIndex] << 16;
                }

                historyIndex = (historyIndex + 6) & 0xFF;
            }
        }

        switch (entity.TargetAnimationId)
        {
            case 0:
            case 1:
                entity.TargetAnimationId = 0;
                if (entity.Bytes[3] != 0)
                {
                    break;
                }

                if ((ushort)entity.AIValues[1] != 0)
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                    break;
                }

                if (entity.Bytes[0] == 0)
                {
                    if ((gameEngine.StaticVariables.g_temporaryFlags[0] & 1U) == 0)
                    {
                        break;
                    }

                    gameEngine.StaticVariables.DAT_801911b0 = (int)entity.TargetDirection;
                    gameEngine.StaticVariables.DAT_801911ac = entity.PosY + (entity.TargetDirection != 0 ? unchecked((int)0xFF900000) : 0x00700000);
                    entity.TargetAnimationId = 2;
                    gameEngine.StaticVariables.DAT_801911a8 = entity.PosX;
                    entity.Bytes[0] = 1;
                    entity.AIValues[1] = 6;
                    break;
                }

                if (entity.Bytes[1] < 9)
                {
                    switch (entity.Bytes[1])
                    {
                        case 0:
                        case 1:
                        case 2:
                            entity.TargetAnimationId = 2;
                            entity.TargetDirection = (uint)((((Random.Next() * 9) >> 32) + 8) & 0x1F);
                            break;

                        case 3:
                        {
                            entity.TargetAnimationId = 2;
                            entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(
                                gameEngine.StaticVariables.DAT_801911a8 - entity.PosX,
                                gameEngine.StaticVariables.DAT_801911ac - entity.PosY);

                            int mode = (int)((Random.Next() * 5) >> 32);
                            entity.Bytes[2] = mode switch
                            {
                                0 => 1,
                                1 => 3,
                                2 => 0,
                                3 => 1,
                                4 => 3,
                                _ => entity.Bytes[2],
                            };
                            break;
                        }

                        case 4:
                        {
                            entity.TargetAnimationId = 2;
                            int aimPlayerRoll = (int)((Random.Next() * 4) >> 32);
                            if (aimPlayerRoll == 0)
                            {
                                entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(
                                    player.PosX - entity.PosX,
                                    player.PosY - entity.PosY);
                            }
                            else
                            {
                                int directionOffset = (int)((Random.Next() * 9) >> 32) - 4;
                                entity.TargetDirection = (uint)(((int)entity.TargetDirection + directionOffset) & 0x1F);
                            }

                            break;
                        }

                        case 5:
                            if (entityIndex >= 0 && entityIndex + 14 < entitySlots.Length)
                            {
                                if (entity.Bytes[2] == 0)
                                {
                                    int matchingFollowers = 0;

                                    for (int i = 0; i < 14; i++)
                                    {
                                        Entity currentEntity = entitySlots[entityIndex + i];
                                        Entity nextEntity = entitySlots[entityIndex + i + 1];
                                        if (currentEntity.DelayOrAngleOrEntityId == nextEntity.DelayOrAngleOrEntityId)
                                        {
                                            matchingFollowers += 1;
                                            continue;
                                        }

                                        int delta = currentEntity.DelayOrAngleOrEntityId - nextEntity.DelayOrAngleOrEntityId;
                                        int step = ((uint)(delta - 1) < 0xFFU || delta < -0x100) ? 4 : -4;
                                        nextEntity.DelayOrAngleOrEntityId = (nextEntity.DelayOrAngleOrEntityId + step) & 0x1FF;
                                        nextEntity.TargetDirection = (uint)(nextEntity.DelayOrAngleOrEntityId >> 4);

                                        int trigIndex = nextEntity.DelayOrAngleOrEntityId;
                                        nextEntity.PosX = currentEntity.PosX + gameEngine.StaticVariables.g_sinus[trigIndex] * 0x500;
                                        nextEntity.PosY = currentEntity.PosY - gameEngine.StaticVariables.g_cosinus[trigIndex] * 0x500;
                                    }

                                    if (matchingFollowers == 14)
                                    {
                                        entity.Bytes[2] = (byte)(entity.Bytes[2] + 1);
                                    }
                                }
                                else
                                {
                                    int radius = 0x500;
                                    for (int i = 0; i < 14; i++)
                                    {
                                        Entity currentEntity = entitySlots[entityIndex + i];
                                        Entity nextEntity = entitySlots[entityIndex + i + 1];
                                        nextEntity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(
                                            currentEntity.PosX - nextEntity.PosX,
                                            currentEntity.PosY - nextEntity.PosY);

                                        int multiplier = ((i + (i >> 31)) >> 1) + 1;
                                        int angle = (entity.DelayOrAngleOrEntityId + gameEngine.StaticVariables.DAT_801911a4 * multiplier) & 0x1FF;
                                        int trigIndex = angle;
                                        nextEntity.PosX = currentEntity.PosX + gameEngine.StaticVariables.g_sinus[trigIndex] * radius;
                                        nextEntity.PosY = currentEntity.PosY - gameEngine.StaticVariables.g_cosinus[trigIndex] * radius;
                                        radius += 4;
                                    }

                                    switch (entity.Bytes[2])
                                    {
                                        case 1:
                                            gameEngine.StaticVariables.DAT_801911a4 += 1;
                                            if (gameEngine.StaticVariables.DAT_801911a4 == 0x20)
                                            {
                                                entity.Bytes[2] = (byte)(entity.Bytes[2] + 1);
                                                gameEngine.SoundManager.PlaySoundEffect(0x10A);
                                            }
                                            break;

                                        case 2:
                                            gameEngine.StaticVariables.DAT_801911a4 -= 4;
                                            if (gameEngine.StaticVariables.DAT_801911a4 == -0x20)
                                            {
                                                entity.Bytes[2] = (byte)(entity.Bytes[2] + 1);
                                            }
                                            break;

                                        case 3:
                                            gameEngine.StaticVariables.DAT_801911a4 += 4;
                                            if (gameEngine.StaticVariables.DAT_801911a4 == 0x14)
                                            {
                                                entity.Bytes[2] = (byte)(entity.Bytes[2] + 1);
                                            }
                                            break;

                                        case 4:
                                            gameEngine.StaticVariables.DAT_801911a4 -= 1;
                                            if (gameEngine.StaticVariables.DAT_801911a4 == 0)
                                            {
                                                entity.AIValues[1] = 0x28;
                                                entity.Bytes[1] = entity.Bytes[2];
                                                gameEngine.StaticVariables.DAT_801911b4 = 0;

                                                int historyFill = 0;
                                                for (int i = 0; i <= 0x54; i++)
                                                {
                                                    int positionHistoryIndex = 0x54 - i;
                                                    int trigIndex = (int)entity.TargetDirection << 4;
                                                    gameEngine.StaticVariables.g_loaderDirectionHistory[i] = (short)entity.TargetDirection;
                                                    gameEngine.StaticVariables.DAT_80191508[positionHistoryIndex] = (short)((entity.PosX + gameEngine.StaticVariables.g_sinus[trigIndex] * historyFill) >> 16);
                                                    gameEngine.StaticVariables.DAT_80191708[positionHistoryIndex] = (short)((entity.PosY - gameEngine.StaticVariables.g_cosinus[trigIndex] * historyFill) >> 16);
                                                    historyFill += 0xD5;
                                                }

                                                gameEngine.StaticVariables.DAT_801911a0 = 0x55;

                                                if (entityIndex >= 0 && entityIndex < 17)
                                                {
                                                    for (int i = entityIndex; i < 17; i++)
                                                    {
                                                        entitySlots[i].TargetDirection = entity.TargetDirection;
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                            break;

                        case 6:
                            break;

                        case 7:
                            if (entityIndex >= 0 && entityIndex + 14 < entitySlots.Length)
                            {
                                for (int i = 1; i <= 14; i++)
                                {
                                    Entity current = entitySlots[entityIndex + i];
                                    if (current.SpriteTableIndex == 0x1E9 || current.SpriteTableIndex == 0x1EC)
                                    {
                                        current.TargetAnimationId = 0xE;
                                        current.Bytes[3] = 0;
                                        int hp = current.Hp;
                                        if (hp < 0)
                                        {
                                            hp += 3;
                                        }

                                        current.Hp = hp >> 2;
                                    }
                                }
                            }

                            entity.Bytes[1] = 8;
                            entity.AIValues[1] = 0x40;
                            break;

                        case 8:
                            entity.Bytes[1] = 4;
                            entity.AIValues[1] = 0x28;
                            break;
                    }
                }
                break;

            case 2:
            case 3:
                entity.TargetAnimationId = 2;
                if (entity.Bytes[0] != 0xF)
                {
                    UpdateWarpSpawnBehavior(gameEngine, entity, gameEngine.StaticVariables.DAT_801911ac, gameEngine.StaticVariables.DAT_801911b0);
                    break;
                }

                if (entity.Bytes[1] == 6)
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                    if ((ushort)entity.AIValues[1] == 0)
                    {
                        entity.TargetDirection = (entity.TargetDirection + (uint)entity.ItemState) & 0x1FU;
                        entity.AIValues[1] = 4;
                        if (entity.TargetDirection == (uint)entity.DelayOrAngleOrEntityId)
                        {
                            entity.AIValues[1] = 0x3C;
                            entity.TargetAnimationId = 0;
                            entity.Bytes[1] = 7;
                        }
                    }

                    break;
                }

                if (entity.Bytes[1] == 3)
                {
                    int deltaX = entity.PosX - gameEngine.StaticVariables.DAT_801911a8;
                    int deltaY = entity.PosY - gameEngine.StaticVariables.DAT_801911ac;
                    if (Math.Abs(deltaX) > 0x180000 || Math.Abs(deltaY) > 0x100000)
                    {
                        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(
                            gameEngine.StaticVariables.DAT_801911a8 - entity.PosX,
                            gameEngine.StaticVariables.DAT_801911ac - entity.PosY);
                        break;
                    }

                    entity.TargetAnimationId = 0;
                    entity.ForceStepY = 0;
                    entity.ForceStepX = 0;
                    entity.ForceY = 0;
                    entity.ForceX = 0;
                    entity.TargetForceY = 0;
                    entity.TargetForceX = 0;

                    if (entity.Bytes[2] == 1)
                    {
                        gameEngine.StaticVariables.DAT_801911a4 = 0;
                        gameEngine.StaticVariables.DAT_801911b4 = entity.Bytes[2];
                    }

                    FUN_800737D0(gameEngine, entity);
                    break;
                }

                if (entity.ChildEntity != null)
                {
                    int stateMinusOne = entity.Bytes[1] - 1;
                    if ((uint)stateMinusOne < 2U)
                    {
                        int spawnRoll = (int)((Random.Next() * 3) >> 32);
                        if (spawnRoll == 0 && (ushort)entity.AIValues[4] < 5)
                        {
                            gameEngine.SpawnWarpEntity(entity, 1, 0xF9, entitySlots[16].PosX, entitySlots[16].PosY, entity.PosZ, 0);
                        }
                    }

                    entity.AIValues[1] = 0x3C;
                    entity.TargetAnimationId = 0;
                    entity.ForceStepY = 0;
                    entity.ForceStepX = 0;
                    entity.ForceY = 0;
                    entity.ForceX = 0;
                    entity.TargetForceY = 0;
                    entity.TargetForceX = 0;

                    if (entity.Bytes[1] == 4)
                    {
                        entity.Bytes[1] = 1;
                    }
                    else
                    {
                        entity.Bytes[1] = (byte)(entity.Bytes[1] + 1);
                    }
                }
                break;

            case 6:
            case 7:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    break;
                }

                if (entity.Bytes[3] != 0)
                {
                    entity.TargetAnimationId = 0;
                    gameEngine.StaticVariables.DAT_801911b4 = 1;

                    if (entityIndex >= 0 && entityIndex + 13 < entitySlots.Length)
                    {
                        for (int i = 0; i < 14; i++)
                        {
                            Entity current = entitySlots[entityIndex + i];
                            current.AIValues[1] = 0x12C;
                            current.AIValues[5] = 0x1E;
                            current.Flags &= 0xFFFFFFFCU;
                        }
                    }

                    break;
                }

                entity.DamagedTickCounter = 0x5A;
                entity.TargetAnimationId = 2;
                if (entity.Bytes[1] == 6)
                {
                    break;
                }

                if (entity.Bytes[1] == 3 && entity.Bytes[2] == 2)
                {
                    break;
                }

                if (entity.Bytes[1] == 4)
                {
                    entity.Bytes[1] = 1;
                    break;
                }

                if (((Random.Next() * 3) >> 32) == 0)
                {
                    entity.Bytes[1] = 3;
                    entity.Bytes[2] = 2;
                }
                else
                {
                    entity.Bytes[1] = 1;
                }
                break;
        }

        FunctionTypeD.FUN_80073940(gameEngine, entity);

        if (entity.Bytes[3] != 0 && entity.TargetAnimationId < 2)
        {
            if (entity.Bytes[3] < 4)
            {
                AI_Melzas2.UpdateEntityAI_BossExplode(gameEngine, entity);
                if (entityIndex >= 0 && entityIndex + 8 < entitySlots.Length)
                {
                    AI_Melzas2.UpdateEntityAI_BossExplode(gameEngine, entitySlots[entityIndex + 8]);
                }
            }
            else
            {
                entity.DelayOrAngleOrEntityId += 1;
                if (entity.DelayOrAngleOrEntityId == 10)
                {
                    entity.DelayOrAngleOrEntityId = 0;
                    int targetIndex = entity.Bytes[1];
                    if ((uint)targetIndex < entitySlots.Length)
                    {
                        entitySlots[targetIndex].TargetAnimationId = 8;
                        entitySlots[targetIndex].Flags |= 0x40U;
                    }

                    entity.Bytes[1] = (byte)(entity.Bytes[1] - 1);
                }
            }
        }
    }

    // GHIDRA: AI_UpdateEntityAI_WarpBoss @ 0x80074D00
    public static void AI_UpdateEntityAI_WarpBoss(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name) 
            && entity.Name != "◆Monsieur Aspiration")
        {
            Breakpoint.TriggerBreak();
        }

        short sVar1;
        Entity effectEntity;
        Entity warpEntity;
        bool triggerFinalWarp = false;
        var player = gameEngine.StaticVariables.PlayerEntity;
        int[] relPos = new int[6];
        int[] gSpawnTablePairs =
        {
            0x03780000, 0x01500000,
            0x03A80000, 0x01800000,
            0x03F00000, 0x01B00000,
            0x03780000, 0x01B00000,
            0x03A80000, 0x01500000,
            0x03F00000, 0x01800000,
            0x03780000, 0x01800000,
            0x03A80000, 0x01B00000,
            0x03F00000, 0x01500000
        };
        short[] dat80027bf8 =
        {
            1, 0x1F, 4, 0x1C, 2, 0x1E,
            4, 0x1C, 1, 0x1F, 2, 0x1E,
            2, 0x1E, 4, 0x1C, 1, 0x1F
        };

        if (entity.DelayOrAngleOrEntityId != 0)
        {
            return;
        }

        if ((gameEngine.StaticVariables.g_playerControlFlags & 0x20U) != 0)
        {
            if (player.TouchingEntity == null)
            {
                if (player.TargetAnimationId == 0x58 && player.IsOnGround != 0)
                {
                    player.DamagedTickCounter = 0x78;
                    gameEngine.StaticVariables.g_playerControlFlags &= 0xFFFFFFDFU;
                    player.TargetAnimationId = 0;
                }
            }
            else
            {
                player.TargetAnimationId = 0x58;
                player.TargetDirection = 0;
            }
        }

        if (entity.Bytes[3] != 0 && entity.TargetAnimationId == 0)
        {
            if (entity.Bytes[3] < 4)
            {
                AI_Melzas2.UpdateEntityAI_BossExplode(gameEngine, entity);
                return;
            }

            entity.TargetAnimationId = 7;
            entity.Flags = (entity.Flags & 0xFFFFFEFFU) | 0x40U;
            entity.PosZ += 0x200000;
            return;
        }

        ScriptHelper.CalculateEntityRelativePosition(entity, player, relPos);

        switch (entity.TargetAnimationId)
        {
            case 0:
                if (entity.AIValues[1] != 0)
                {
                    entity.AIValues[1]--;
                    break;
                }

                if ((int)(((ulong)Random.Next() * 2UL) >> 32) == 0 && entity.Bytes[2] == 0)
                {
                    entity.TargetAnimationId = 5;
                    entity.AIValues[1] = 0x1E;
                    break;
                }

                entity.TargetAnimationId = 1;
                break;

            case 1:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 2;
                    entity.AIValues[1] = 0x708;

                    gameEngine.StaticVariables.g_bossSpawnedEffectEntity = gameEngine.SpawnWarpEntity(entity, 1, 0xDD, entity.PosX, entity.PosY, entity.PosZ, 0);
                    effectEntity = gameEngine.SpawnWarpEntity(entity, 0, 0xEC, 0x03D80000, 0x01480000, entity.PosZ, 0);
                    gameEngine.StaticVariables.g_bossEffectEntity = effectEntity;

                    if (effectEntity != null)
                    {
                        effectEntity.TargetAnimationId = 2;
                        effectEntity.Flags |= 4;
                    }
                }
                break;

            case 2:
                entity.AIValues[1]--;

                if ((entity.AIValues[1] & 3) == 0)
                {
                    int spawnCount = (int)(((ulong)Random.Next() * 2UL) >> 32);
                    int loopCount = 0;

                    if (spawnCount != -2)
                    {
                        do
                        {
                            var rand1 = (ulong)Random.Next();
                            var rand2 = (ulong)Random.Next();
                            int itemState = entity.ItemState;
                            int nextIndex = itemState + 1;
                            entity.ItemState = nextIndex;

                            int baseIndex = itemState * 2;
                            int x = gSpawnTablePairs[baseIndex] + (int)((rand1 * 0x31UL) >> 32) * 0x4000;
                            int y = gSpawnTablePairs[baseIndex + 1] + (int)((rand2 * 0x21UL) >> 32) * 0x4000;

                            if (nextIndex > 8)
                            {
                                entity.ItemState = 0;
                            }

                            loopCount++;
                            warpEntity = gameEngine.SpawnWarpEntity(entity, 0, 0xEC, x, y, entity.PosZ, 0);
                            warpEntity.ForceZ = 0x8000;
                        } while (loopCount != spawnCount + 2);
                    }
                }

                if (entity.AIValues[1] == 0)
                {
                    entity.TargetAnimationId = 3;
                    triggerFinalWarp = true;
                }
                else
                {
                    int speed = gameEngine.StaticVariables.g_currentMap == 0x7B ? 0xA0 : 0xB0;

                    for (int slotIndex = 0; slotIndex < 0x40; slotIndex++)
                    {
                        Entity entity2 = gameEngine.StaticVariables.g_entitySlots[slotIndex];
                        uint spriteTableIndex = entity2.SpriteTableIndex;

                        if (slotIndex == 0
                            || spriteTableIndex == 0x1DE
                            || spriteTableIndex == 2
                            || (spriteTableIndex == 0xEC && entity2.TargetAnimationId == 0)
                            || spriteTableIndex == 0x1B2
                            || spriteTableIndex == 0x112)
                        {
                            int dx = entity2.PosX - 0x03D80000;
                            int dy = entity2.PosY - 0x01380000;

                            if (dx < 0)
                            {
                                dx = -dx;
                            }

                            if (dx < 0x300001)
                            {
                                if (dy < 0)
                                {
                                    dy = -dy;
                                }

                                if (dy < 0x100001)
                                {
                                    if (entity.TargetAnimationId != 4)
                                    {
                                        if (slotIndex == 0
                                            && (entity2.AnimFlags & 0x40U) == 0
                                            && entity2.DamagedTickCounter == 0)
                                        {
                                            triggerFinalWarp = true;
                                            entity2.TargetAnimationId = 0x56;
                                            entity.TargetAnimationId = 4;
                                            gameEngine.StaticVariables.g_playerControlFlags |= 0x20U;
                                            entity.AIValues[1] = 0x9A;
                                            entity.AIValues[4] = 1;
                                            entity2.ForceStepY = 0;
                                            entity2.ForceStepX = 0;
                                            entity2.ForceY = 0;
                                            entity2.ForceX = 0;
                                            entity2.TargetForceY = 0;
                                            entity2.TargetForceX = 0;
                                        }

                                        if (spriteTableIndex == 0x1B2 || spriteTableIndex == 0x112)
                                        {
                                            triggerFinalWarp = true;
                                            entity2.TargetAnimationId = 0x12;
                                            entity.AIValues[1] = 0x9A;
                                            entity.TargetAnimationId = 4;
                                            entity.AIValues[4] = 2;
                                        }
                                    }

                                    continue;
                                }
                            }

                            uint direction = (uint)ScriptHelper.GetDirectionToTarget(0x03D80000 - entity2.PosX, 0x01380000 - entity2.PosY);
                            entity2.PreviousAdjustedForceX = gameEngine.StaticVariables.g_offsetXList[direction] * speed;
                            entity2.PreviousAdjustedForceY = gameEngine.StaticVariables.g_offsetYList[direction] * speed;
                        }
                    }
                }
                break;

            case 3:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0x78;
                    entity.Bytes[0] = 0;
                }
                break;

            case 4:
                sVar1 = entity.AIValues[1];
                if (sVar1 != 0)
                {
                    entity.AIValues[1] = (short)(sVar1 - 1);
                    if (sVar1 == 1)
                    {
                        gameEngine.SoundManager.PlaySoundEffect(0xB2);
                        effectEntity = gameEngine.SpawnWarpEntity(entity, 0, 0xEC, player.PosX, player.PosY - 0x10000, player.PosZ, 0);

                        if (effectEntity == null)
                        {
                            entity.AIValues[1] = 1;
                        }
                        else
                        {
                            effectEntity.TargetAnimationId = 1;
                            effectEntity.Flags |= 2;

                            if (entity.AIValues[4] == 1)
                            {
                                player.PosZ = entity.PosZ;
                                player.TargetDirection = 0x10;
                                player.TargetAnimationId = 0;

                                effectEntity.PosX = player.PosX;
                                effectEntity.PosY = player.PosY;
                                effectEntity.PosZ = player.PosZ;
                            }
                            else if (entity.AIValues[4] == 2)
                            {
                                if (gameEngine.StaticVariables.g_currentMap == 0x7B)
                                {
                                    gameEngine.GetMatchingEntityBySearchType(entity, 7);
                                }
                                else
                                {
                                    gameEngine.GetMatchingEntityBySearchType(entity, 1);
                                    effectEntity.TargetAnimationId = 3;
                                }

                                effectEntity = gameEngine.StaticVariables.g_matchingEntitiesBuffer[0];
                                effectEntity.PosZ = entity.PosZ;
                                effectEntity.TargetAnimationId = 0x10;
                                effectEntity.AIValues[1] = 0x1E;
                                effectEntity.TargetDirection = 0;
                                effectEntity.Bytes[2] = 1;
                            }

                            entity.AIValues[4] = 0;
                        }
                    }
                }

                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 3;
                    entity.AIValues[1] = 0x78;
                }
                break;

            case 5:
                sVar1 = entity.AIValues[1];
                if (sVar1 == 0)
                {
                    if (entity.ForceResetAnimationFlag != 0)
                    {
                        var rand = (ulong)Random.Next();
                        entity.TargetAnimationId = 0;
                        entity.AIValues[1] = (short)(((rand * 0x3DUL) >> 32) + 0x3C);
                    }
                }
                else
                {
                    entity.AIValues[1] = (short)(sVar1 - 1);
                    if (sVar1 == 1)
                    {
                        gameEngine.SoundManager.PlaySoundEffect(0xB2);

                        int patternBase = (int)(((ulong)Random.Next() * 3UL) >> 32) * 6;
                        for (int i = 0; i < 6; i++)
                        {
                            effectEntity = gameEngine.SpawnWarpEntity(
                                entity,
                                1,
                                0xDE,
                                entity.PosX,
                                entity.PosY + 0x1E0000,
                                entity.PosZ + 0x0A0000,
                                (uint)dat80027bf8[patternBase + i]);

                            if (effectEntity != null)
                            {
                                entity.Bytes[2]++;
                                effectEntity.TargetAnimationId = (uint)(i + 9);
                            }
                        }
                    }
                }
                break;

            case 6:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    break;
                }

                if (entity.Bytes[3] != 0)
                {
                    triggerFinalWarp = true;
                    entity.AIValues[1] = 300;
                    entity.AIValues[5] = 0x1E;
                    entity.TargetAnimationId = 0;
                    entity.Flags &= 0xFFFFFFFCU;
                    break;
                }

                if (gameEngine.StaticVariables.g_currentMap == 0x7B)
                {
                    if (entity.Bytes[0] < 2)
                    {
                        entity.TargetAnimationId = 2;
                        break;
                    }
                }
                else
                {
                    if (entity.Bytes[0] < 3)
                    {
                        entity.TargetAnimationId = 2;
                        break;
                    }
                }

                entity.TargetAnimationId = 3;
                triggerFinalWarp = true;

                if (gameEngine.StaticVariables.g_currentMap == 0x7B && entity.Bytes[1] > 0x13)
                {
                    gameEngine.GetMatchingEntityBySearchType(entity, 6);
                    gameEngine.DestroyEntity(gameEngine.StaticVariables.g_matchingEntitiesBuffer[0]);
                    entity.DelayOrAngleOrEntityId = 1;
                }
                break;
        }

        if (triggerFinalWarp)
        {
            gameEngine.StaticVariables.g_bossSpawnedEffectEntity.TargetAnimationId = 2;
            gameEngine.StaticVariables.g_bossSpawnedEffectEntity.Flags |= 0x40;
            gameEngine.DestroyEntity(gameEngine.StaticVariables.g_bossEffectEntity);
        }
    }

    // GHIDRA: AI_UpdateLoaderBossAI @ 0x80076DA0
    public static void AI_UpdateLoaderBossAI(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

        bool spawnAmbientEffect = false;
        short sVar4;
        var player = gameEngine.StaticVariables.PlayerEntity;
        int[] relPos = new int[6];
        short[] shortArray = gameEngine.StaticVariables.SHORT_ARRAY_80027d18;

        if (gameEngine.StaticVariables.g_loaderInitialized == 0)
        {
            gameEngine.StaticVariables.g_loaderEffectEntityId = gameEngine.EffectManager.CreateEffectEntity(1, 1, 0, entity.PosX, entity.PosY, entity.PosZ);
            gameEngine.StaticVariables.g_loaderInitialized = 1;
            entity.ItemState = 0x78;
            gameEngine.StaticVariables.g_loaderEventDelay1 = 0;
            gameEngine.StaticVariables.g_loaderEventDelay2 = 0;
        }

        var loaderEffect = gameEngine.StaticVariables.g_loaderEffectEntityId;
        loaderEffect.X = entity.PosX;
        loaderEffect.Y = entity.PosY;
        loaderEffect.Z = entity.PosZ + 0x400000;

        if (entity.Bytes[0] == 0 && entity.TileY > 0x18)
        {
            entity.Bytes[0] = 1;
            entity.TargetAnimationId = 10;
            entity.AIValues[1] = 0x1E;
        }

        if (entity.Bytes[0] == 1 && entity.TileY > 0x34)
        {
            entity.Bytes[0] = 2;
            entity.TargetAnimationId = 0x0B;
            gameEngine.StaticVariables.g_scrollingParameters.Flag = 0;
            entity.DelayOrAngleOrEntityId = 0;
        }

        if (entity.DelayOrAngleOrEntityId != 0)
        {
            int delay = entity.DelayOrAngleOrEntityId - 1;
            entity.DelayOrAngleOrEntityId = delay;
            if (delay == 0)
            {
                gameEngine.StaticVariables.g_scrollingParameters.Flag = 0;
            }
        }

        if (gameEngine.StaticVariables.g_loaderEventDelay2 != 0 || gameEngine.StaticVariables.g_loaderEventDelay1 != 0)
        {
            bool triggerScroll = false;

            if (gameEngine.StaticVariables.g_loaderEventDelay2 == 0)
            {
                if (gameEngine.StaticVariables.g_loaderEventDelay1 != 0)
                {
                    triggerScroll = gameEngine.StaticVariables.g_loaderEventDelay1 == 1;
                    gameEngine.StaticVariables.g_loaderEventDelay1--;
                }
            }
            else
            {
                triggerScroll = gameEngine.StaticVariables.g_loaderEventDelay2 == 1;
                gameEngine.StaticVariables.g_loaderEventDelay2--;
            }

            if (triggerScroll)
            {
                gameEngine.StaticVariables.g_scrollingParameters.Flag = 1;
                gameEngine.StaticVariables.g_scrollingParameters.SpeedX = 1;
                gameEngine.StaticVariables.g_scrollingParameters.SpeedY = 1;
                gameEngine.StaticVariables.g_scrollingParameters.LimitX = 2;
                gameEngine.StaticVariables.g_scrollingParameters.LimitY = 2;
                entity.DelayOrAngleOrEntityId = 0x1E;
            }
        }

        if (entity.DelayOrAngleOrEntityId == 0x1E || entity.DelayOrAngleOrEntityId == 0x19 || entity.DelayOrAngleOrEntityId == 0x14)
        {
            for (int i = 0; i < 2; i++)
            {
                int x = i == 0 ? 0x01E00000 : 0x00F00000;
                var rand1 = (ulong)Random.Next();
                var rand2 = (ulong)Random.Next();

                var effect = gameEngine.EffectManager.CreateEffectEntity(
                    0,
                    0x16,
                    1,
                    x + (int)((rand1 * 0x0BUL) >> 32) * 0x0C0000,
                    player.PosY + (int)((rand2 * 0x13UL) >> 32) * 0x080000 - 0x400000,
                    0x0A00000);

                if (effect != null)
                {
                    var rand3 = (ulong)Random.Next();
                    effect.ForceZ = (int)((rand3 * 5UL) >> 32) * 0x4000 - 0x40000;
                }
            }
        }

        if ((ushort)entity.AIValues[4] < 0x0D
            && entity.TargetAnimationId != 8
            && --entity.ItemState == 0)
        {
            entity.ItemState = 0x0B4;
            if (player.TileY - entity.TileY > 0x0E)
            {
                entity.ItemState = 0x78;
            }

            var rand = (ulong)Random.Next();
            int patternBase = (int)((rand * 9UL) >> 32) * 12 + 12;
            short spawnDelay = 0;

            for (int i = 0; (player.TileY - entity.TileY > 4 || (ushort)entity.AIValues[4] < 4) && i != 0x0C; i += 3)
            {
                short xMul = shortArray[patternBase + i];
                short yMul = shortArray[patternBase + i + 1];
                int spriteId = shortArray[patternBase + i + 2];

                if (spriteId == 0)
                {
                    break;
                }

                int spawnX = 0x01500000 + xMul * 0x180000;
                int spawnY = player.PosY + yMul * 0x100000;

                if (spriteId != 0x1F0)
                {
                    spawnX = 0x015C0000 + xMul * 0x180000;
                    spawnY += 0x080000;
                }

                if (spawnY > 0x33FFFFF)
                {
                    break;
                }

                Entity spawned = gameEngine.SpawnWarpEntity(entity, 1, (uint)(spriteId - 0x100), spawnX, spawnY, entity.PosZ + 0x0A0000, 0);
                if (spawned != null)
                {
                    spawned.TargetAnimationId = 5;
                    spawned.AIValues[1] = spawnDelay;
                    entity.AIValues[4]++;
                }

                spawnDelay += 0x14;
            }
        }

        ScriptHelper.CalculateEntityRelativePosition(entity, player, relPos);

        switch (entity.TargetAnimationId)
        {
            case 0:
                sVar4 = entity.AIValues[1];
                if (sVar4 == 0)
                {
                    uint nextAnim;
                    if (player.TileY - entity.TileY < 0x0F)
                    {
                        nextAnim = (uint)shortArray[(int)(((ulong)Random.Next() * 5UL) >> 32)];
                    }
                    else
                    {
                        nextAnim = 4;
                    }

                    entity.TargetAnimationId = nextAnim;
                    entity.CurrentAnimationId = 0xFFFFFFFF;

                    if (nextAnim == 0)
                    {
                        entity.AIValues[1] = 0x1E;
                    }
                    else
                    {
                        int delayIndex = ((int)nextAnim - 2) * 2 + 0x78;
                        gameEngine.StaticVariables.g_loaderEventDelay2 = shortArray[delayIndex];
                        gameEngine.StaticVariables.g_loaderEventDelay1 = shortArray[delayIndex + 1];
                    }
                }
                else
                {
                    entity.AIValues[1] = (short)(sVar4 - 1);
                }
                break;

            case 1:
                sVar4 = entity.AIValues[1];
                if (sVar4 != 0)
                {
                    entity.AIValues[1] = (short)(sVar4 - 1);
                    break;
                }

                uint nextAnimCase1;
                if (player.TileY - entity.TileY < 0x0F)
                {
                    nextAnimCase1 = (uint)shortArray[(int)(((ulong)Random.Next() * 5UL) >> 32) + 6];
                }
                else
                {
                    nextAnimCase1 = 7;
                }

                entity.TargetAnimationId = nextAnimCase1;
                entity.CurrentAnimationId = 0xFFFFFFFF;

                if (nextAnimCase1 == 1)
                {
                    entity.AIValues[1] = 0x1E;
                }
                else
                {
                    int delayIndex = ((int)nextAnimCase1 - 2) * 2 + 0x78;
                    gameEngine.StaticVariables.g_loaderEventDelay2 = shortArray[delayIndex];
                    gameEngine.StaticVariables.g_loaderEventDelay1 = shortArray[delayIndex + 1];
                }
                break;

            case 2:
            case 3:
            case 4:
                if (entity.ForceAdjusted != 0 && entity.XCollisionEntity == player)
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0x8C;
                    break;
                }

                spawnAmbientEffect = true;
                if (entity.ForceResetAnimationFlag != 0)
                {
                    goto case 0;
                }
                break;

            case 5:
            case 6:
            case 7:
                if (entity.ForceAdjusted != 0 && entity.XCollisionEntity == player)
                {
                    entity.TargetAnimationId = 1;
                    entity.AIValues[1] = 0x8C;
                    break;
                }

                spawnAmbientEffect = true;
                if (entity.ForceResetAnimationFlag != 0)
                {
                    goto case 1;
                }
                break;

            case 8:
                if (gameEngine.EntityGameplayManager.TryAttackPlayerFront(entity, relPos, 3, 10, 0x100000))
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = 0x28;
                }
                break;

            case 9:
                gameEngine.StaticVariables.g_temporaryFlags[0] |= 2;
                break;

            case 10:
                spawnAmbientEffect = true;
                break;

            case 0x0B:
                gameEngine.StaticVariables.g_temporaryFlags[0] |= 1;
                spawnAmbientEffect = true;
                break;
        }

        if (spawnAmbientEffect)
        {
            if (entity.Bytes[2] == 0)
            {
                var rand1 = (ulong)Random.Next();
                var rand2 = (ulong)Random.Next();
                int z = (int)((rand2 * 0x15UL) >> 32) * 0x10000;

                gameEngine.EffectManager.CreateEffectEntity(
                    1,
                    0,
                    0,
                    entity.PosX + (int)((rand1 * 0x79UL) >> 32) * 0x10000 - 0x3C0000,
                    entity.PosY + 0x200000,
                    entity.PosZ + z);

                gameEngine.SoundManager.PlaySoundEffect(0x2E);
                entity.Bytes[2] = 8;
            }

            entity.Bytes[2]--;
        }
    }

    // JUSTIFICATION: C# language bridge only
    private static Entity? GetFireAiEntityRef(GameEngine gameEngine, Entity entity)
    {
        int slotIndex = entity.AIValues[2];
        return slotIndex == 0 ? null : gameEngine.StaticVariables.g_entitySlots[slotIndex];
    }

    // JUSTIFICATION: C# language bridge only
    private static void SetFireAiEntityRef(Entity entity, Entity? target)
    {
        entity.AIValues[2] = (short)(target?.Index ?? 0);
        entity.AIValues[3] = 0;
    }

    // GHIDRA: AI_UpdateEntityIA_Fire @ 0x80078E34
    public static void AI_UpdateEntityIA_Fire(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

        int[] relPos = new int[6];

        if (gameEngine.StaticVariables.g_loaderInitialized == 0)
        {
            Array.Clear(gameEngine.StaticVariables.PTR_ARRAY_80191204, 1, 12);
            gameEngine.StaticVariables.g_fireCycleCounter = 0;
            gameEngine.StaticVariables.g_fireCyclePhase = 0;
            gameEngine.StaticVariables.g_fireCycleState = 0;
            gameEngine.StaticVariables.g_fireSummonCount = 0;
            gameEngine.StaticVariables.DAT_80191238 = 0;
            gameEngine.StaticVariables.g_loaderInitialized = 1;
        }

        if (entity.Bytes[3] != 0 && entity.TargetAnimationId == 0)
        {
            if (entity.Bytes[3] < 4)
            {
                AI_Melzas2.UpdateEntityAI_BossExplode(gameEngine, entity);
                return;
            }

            entity.TargetAnimationId = 7;
            entity.Flags |= 0x40;
            return;
        }

        if (gameEngine.StaticVariables.g_fireCyclePhase != 0)
        {
            gameEngine.StaticVariables.g_fireCyclePhase--;
        }

        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relPos);

        if (entity.AIValues[4] == 0)
        {
            Entity? attached = GetFireAiEntityRef(gameEngine, entity);
            if (attached != null)
            {
                gameEngine.DestroyEntity(attached);
                SetFireAiEntityRef(entity, null);
            }
        }
        else
        {
            Entity? attached = GetFireAiEntityRef(gameEngine, entity);
            entity.DelayOrAngleOrEntityId = (entity.DelayOrAngleOrEntityId - (ushort)entity.AIValues[4] + 0x0C) & 0x1FF;

            if (attached == null)
            {
                if (gameEngine.StaticVariables.DAT_80191254 == 0x708)
                {
                    Entity? spawned = gameEngine.SpawnWarpEntity(entity, 0, 0x19, entity.PosX, entity.PosY, entity.PosZ, 0);
                    SetFireAiEntityRef(entity, spawned);
                    if (spawned != null)
                    {
                        spawned.TargetAnimationId = 4;
                        spawned.Flags &= 0xFFFFFF7FU;
                    }
                }
                else
                {
                    int threshold = entity.ItemState + 0x1000;
                    bool bothNegative =
                        relPos[0] * 0x180000 - threshold * 0x300 < 0 &&
                        relPos[1] * 0x100000 - threshold * 0x200 < 0;

                    if (bothNegative || relPos[0] > 5 || relPos[1] > 5)
                    {
                        if (entity.ItemState != 0 || bothNegative)
                        {
                            entity.ItemState -= 0x16;
                            if (entity.ItemState < 0)
                            {
                                entity.ItemState = 0;
                            }
                        }
                        else
                        {
                            gameEngine.StaticVariables.DAT_80191254++;
                        }
                    }
                    else
                    {
                        gameEngine.StaticVariables.DAT_80191254 = 0;
                        entity.ItemState += 0x16;
                        if (entity.ItemState > 0x1000)
                        {
                            entity.ItemState = 0x1000;
                        }
                    }
                }
            }
        }

        switch ((int)entity.TargetAnimationId)
        {
            case 0:
            {
                short delay = (short)(entity.AIValues[1] - 1);

                if (entity.AIValues[1] == 0)
                {
                    if (entity.AIValues[4] != 0)
                    {
                        uint direction = (uint)ScriptHelper.GetDirectionToTarget(
                            gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                            gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);

                        if (gameEngine.StaticVariables.g_fireCycleCounter != 1 &&
                            (gameEngine.StaticVariables.g_fireCycleCounter == 2 ||
                             relPos[1] > 6 ||
                             (direction - 7) < 0x13 ||
                             ((Random.Next() * 3) >> 32) != 0))
                        {
                            if (gameEngine.StaticVariables.g_fireCyclePhase != 0)
                            {
                                return;
                            }

                            gameEngine.StaticVariables.g_fireCycleCounter = 0;
                            uint range = relPos[1] < 5 ? 6U : 2U;
                            entity.Bytes[0] = (byte)((Random.Next() * range) >> 32);
                            entity.TargetAnimationId = 4;
                            entity.AIValues[1] = 0x17;
                            entity.Bytes[1] = 0;
                            return;
                        }

                        entity.TargetAnimationId = 8;
                        gameEngine.StaticVariables.g_fireCycleCounter = 0;
                        gameEngine.StaticVariables.g_fireSummonCount = 0;

                        if (gameEngine.StaticVariables.DAT_80191238 == 0)
                        {
                            gameEngine.StaticVariables.g_fireCycleState = (int)((Random.Next() * 3) >> 32);
                        }
                        else if (gameEngine.StaticVariables.DAT_80191238 != 6)
                        {
                            gameEngine.StaticVariables.g_fireCycleState = (int)((Random.Next() * 9) >> 32);
                        }
                        else
                        {
                            gameEngine.StaticVariables.g_fireCycleState = 9;
                        }

                        if (gameEngine.StaticVariables.g_fireCycleState < 3 &&
                            ((Random.Next() * 3) >> 32) != 0)
                        {
                            if ((direction - 2) < 0x1D)
                            {
                                gameEngine.StaticVariables.g_fireCycleState = 1;
                                if (direction > 6)
                                {
                                    gameEngine.StaticVariables.g_fireCycleState = 2;
                                }
                            }
                            else
                            {
                                gameEngine.StaticVariables.g_fireCycleState = 0;
                            }
                        }

                        gameEngine.StaticVariables.DAT_80191238++;
                        if (gameEngine.StaticVariables.DAT_80191238 == 7)
                        {
                            gameEngine.StaticVariables.DAT_80191238 = 0;
                        }

                        entity.TargetDirection =
                            gameEngine.StaticVariables.g_directionCycleTable[gameEngine.StaticVariables.g_fireCycleState * 6 + gameEngine.StaticVariables.g_fireSummonCount];
                        break;
                    }

                    entity.TargetAnimationId = 4;
                    entity.Bytes[0] = 0xFF;
                    delay = 0x17;
                }

                entity.AIValues[1] = delay;
                break;
            }

            case 1:
            {
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                gameEngine.StaticVariables.g_fireSummonCount++;

                int spawnOffsetX;
                int spawnOffsetY;

                if (entity.TargetDirection == 0)
                {
                    spawnOffsetX = 0x150000;
                    spawnOffsetY = 0x260000;
                    gameEngine.StaticVariables.DAT_8019124c = 0;
                    gameEngine.StaticVariables.DAT_80191250 = 0x100000;
                }
                else if (entity.TargetDirection == 0x10)
                {
                    spawnOffsetX = -0x170000;
                    spawnOffsetY = 0x170000;
                    gameEngine.StaticVariables.DAT_8019124c = -0x160000;
                    gameEngine.StaticVariables.DAT_80191250 = 0x160000;
                }
                else
                {
                    spawnOffsetX = 0x3F0000;
                    spawnOffsetY = 0x170000;
                    gameEngine.StaticVariables.DAT_8019124c = 0x160000;
                    gameEngine.StaticVariables.DAT_80191250 = 0x160000;
                }

                gameEngine.InitializeAndBeginFadeEffect();

                gameEngine.StaticVariables.PTR_ARRAY_80191204[1] = gameEngine.SpawnWarpEntity(
                    entity,
                    1,
                    0xBF,
                    entity.PosX + spawnOffsetX,
                    entity.PosY + spawnOffsetY,
                    entity.PosZ,
                    entity.TargetDirection);

                Entity? first = gameEngine.StaticVariables.PTR_ARRAY_80191204[1];
                if (first == null)
                {
                    return;
                }

                entity.Bytes[2] = 1;
                entity.TargetAnimationId = 2;
                entity.AIValues[1] = 0x12;
                break;
            }

            case 2:
            {
                short countdown = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = countdown;

                if (countdown == 0)
                {
                    for (int i = 1; i <= 12; i++)
                    {
                        Entity spawned = gameEngine.StaticVariables.PTR_ARRAY_80191204[i]!;
                        spawned.TargetAnimationId = 1;
                        spawned.Flags |= 0x40;
                        gameEngine.StaticVariables.PTR_ARRAY_80191204[i] = null;
                    }

                    entity.TargetAnimationId = 3;
                    entity.Bytes[2] = 0;
                }
                else
                {
                    int index = entity.Bytes[2];
                    if (index != 0x0C)
                    {
                        Entity source = gameEngine.StaticVariables.PTR_ARRAY_80191204[index]!;
                        Entity? spawned = gameEngine.SpawnWarpEntity(
                            entity,
                            1,
                            0xBF,
                            source.PosX + gameEngine.StaticVariables.DAT_8019124c,
                            source.PosY + gameEngine.StaticVariables.DAT_80191250,
                            source.PosZ,
                            entity.TargetDirection);

                        gameEngine.StaticVariables.PTR_ARRAY_80191204[index + 1] = spawned;
                        if (spawned != null)
                        {
                            entity.Bytes[2]++;
                        }
                    }
                }

                break;
            }

            case 3:
            {
                if (entity.ForceResetAnimationFlag != 0)
                {
                    uint direction =
                        gameEngine.StaticVariables.g_directionCycleTable[gameEngine.StaticVariables.g_fireCycleState * 6 + gameEngine.StaticVariables.g_fireSummonCount];

                    if (direction == 0xFFFFFFFF)
                    {
                        entity.TargetAnimationId = 9;
                    }
                    else
                    {
                        entity.TargetAnimationId = 1;
                        entity.TargetDirection = direction;
                    }
                }

                break;
            }

            case 4:
            {
                short timer = entity.AIValues[1];
                if (timer != 0)
                {
                    entity.AIValues[1] = (short)(timer - 1);
                    if (timer == 1)
                    {
                        if (entity.Bytes[0] == 0xFF)
                        {
                            entity.AIValues[4] = 8;
                            gameEngine.StaticVariables.DAT_80191254 = 0;
                            entity.ItemState = 0;
                            entity.DelayOrAngleOrEntityId = 0;

                            for (int i = 0; i != 8; i++)
                            {
                                Entity flame = gameEngine.SpawnWarpEntity(entity, 1, 0xC2, 0, 0, entity.PosZ + 0xC0000, 0);
                                flame.DelayOrAngleOrEntityId = i << 6;
                                flame.TargetAnimationId = 1;
                            }
                        }
                        else
                        {
                            int posY = entity.PosY;
                            int posZ = entity.PosZ + 0x200000;

                            Entity left = gameEngine.SpawnWarpEntity(entity, 1, 0xC0, entity.PosX - 0xA0000, posY, posZ, 0);
                            if (left != null)
                            {
                                left.Bytes[0] = (byte)(entity.Bytes[1] << 1);
                            }

                            Entity right = gameEngine.SpawnWarpEntity(entity, 1, 0xC0, entity.PosX + 0xA0000, posY, posZ, 0);
                            if (right != null)
                            {
                                right.Bytes[0] = (byte)((entity.Bytes[1] << 1) + 1);
                            }

                            entity.Bytes[1]++;
                            if (entity.Bytes[1] != 3)
                            {
                                entity.AIValues[1] = 0x1E;
                            }
                        }
                    }
                }

                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0;
                    entity.AIValues[1] = (short)(((Random.Next() * 0x20) >> 32) + 0x1E);
                    gameEngine.StaticVariables.g_fireCyclePhase = 0xF0;
                }

                break;
            }

            case 6:
            {
                for (int i = 1; i <= 12; i++)
                {
                    Entity? spawned = gameEngine.StaticVariables.PTR_ARRAY_80191204[i];
                    if (spawned != null)
                    {
                        spawned.TargetAnimationId = 1;
                        spawned.Flags |= 0x40;
                        gameEngine.StaticVariables.PTR_ARRAY_80191204[i] = null;
                    }
                }

                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                if (entity.Bytes[3] != 0)
                {
                    entity.AIValues[1] = 300;
                    entity.AIValues[5] = 0x1E;
                    entity.TargetAnimationId = 0;
                    entity.Flags &= 0xFFFFFFFC;
                    return;
                }

                gameEngine.StaticVariables.g_fireCycleCounter =
                    (gameEngine.StaticVariables.DAT_80191238 == 0 || gameEngine.StaticVariables.DAT_80191238 == 6) ? 1 : 2;

                entity.DamagedTickCounter = 0x5A;
                entity.TargetAnimationId = 0;
                entity.AIValues[1] = 0x14;
                break;
            }

            case 8:
            {
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 1;
                entity.TargetDirection =
                    gameEngine.StaticVariables.g_directionCycleTable[gameEngine.StaticVariables.g_fireCycleState * 6 + gameEngine.StaticVariables.g_fireSummonCount];
                break;
            }

            case 9:
            {
                if (entity.ForceResetAnimationFlag == 0)
                {
                    return;
                }

                entity.TargetAnimationId = 0;
                entity.AIValues[1] = (short)(((Random.Next() * 0x20) >> 32) + 0x3C);
                break;
            }
        }
    }

    // GHIDRA: AI_ApplyMatchingEntityKnockback @ 0x8007ADDC
    public static void AI_ApplyMatchingEntity(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

        int matchCount = gameEngine.GetMatchingEntityBySearchType(entity, 10);
        Entity entityMatched = gameEngine.StaticVariables.g_matchingEntitiesBuffer[0];

        if (matchCount == 0)
        {
            return;
        }

        if (gameEngine.StaticVariables.PlayerEntity.CarriedEntity == entityMatched)
        {
            entity.ForceZ = 0;
            entity.Flags &= 0xFFFFFEFF;
            entity.PosZ = entityMatched.PosZ;
            entity.TargetForceX = -gameEngine.StaticVariables.PlayerEntity.TargetForceX;
            entity.TargetForceY = -gameEngine.StaticVariables.PlayerEntity.TargetForceY;
            entity.ForceX = -gameEngine.StaticVariables.PlayerEntity.ForceX;
            entity.ForceY = -gameEngine.StaticVariables.PlayerEntity.ForceY;
            entity.ForceStepX = -gameEngine.StaticVariables.PlayerEntity.ForceStepX;
            entity.ForceStepY = -gameEngine.StaticVariables.PlayerEntity.ForceStepY;
            return;
        }

        entity.Flags |= 0x100;
        entity.PosZ = entityMatched.PosZ;
        entity.TargetForceX = -entityMatched.TargetForceX;
        entity.TargetForceY = -entityMatched.TargetForceY;
        entity.ForceX = -entityMatched.ForceX;
        entity.ForceY = -entityMatched.ForceY;
        entity.ForceStepX = -entityMatched.ForceStepX;
        entity.ForceStepY = -entityMatched.ForceStepY;
    }

    // GHIDRA: AI_UpdateHomingProjectileBehavior @ 0x8007AF20
    public static void AI_UpdateHomingProject(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

        int distance = entity.BalanceAnimValRef!.Val & 0x0F;
        Entity?[] nearbyEntities = new Entity?[gameEngine.StaticVariables.g_numberOfEntities];
        int[] distanceSquared = new int[nearbyEntities.Length];
        int nearbyCount = 0;

        for (int i = 1; i < gameEngine.StaticVariables.g_numberOfEntities; i++)
        {
            Entity candidate = gameEngine.StaticVariables.g_entitySlots[i];

            if (candidate == entity)
            {
                continue;
            }

            if ((uint)(candidate.Status - 2) >= 2U)
            {
                continue;
            }

            if (candidate.BlockedByEntity != null)
            {
                continue;
            }

            if ((candidate.Flags & 1) == 0)
            {
                continue;
            }

            if (candidate.FrameCollisionTickCounter != 0)
            {
                continue;
            }

            if (candidate.DamagedTickCounter != 0)
            {
                continue;
            }

            if ((candidate.AnimFlags & 0x40) != 0)
            {
                continue;
            }

            if (candidate.BalanceRecord == null)
            {
                continue;
            }

            if ((candidate.BalanceRecord.Values[distance - 1] & 0xC0) == 0x80)
            {
                continue;
            }

            int x = (candidate.PosX - entity.PosX) >> 16;
            int y = (candidate.PosY - entity.PosY) >> 16;

            nearbyEntities[nearbyCount] = candidate;
            distanceSquared[nearbyCount] = x * x + y * y;
            nearbyCount++;
        }

        if (nearbyCount > 2)
        {
            Array.Sort(distanceSquared, nearbyEntities, 0, nearbyCount);
        }

        for (int i = 0; i < nearbyCount; i++)
        {
            Entity candidate = nearbyEntities[i]!;

            int deltaX = candidate.PosX - entity.PosX;
            int deltaY = candidate.PosY - entity.PosY;
            int absDeltaX = Math.Abs(deltaX);

            if (absDeltaX >= 0x1400001)
            {
                continue;
            }

            int absDeltaY = Math.Abs(deltaY);

            if (absDeltaY >= 0x0F00001)
            {
                continue;
            }

            uint direction = ((uint)ScriptHelper.GetDirectionToTarget(deltaX, deltaY) + 0x14U) & 0x1F;
            candidate.PreviousAdjustedForceX = gameEngine.StaticVariables.g_cosinus[(int)direction * 4] * 0x300;
            candidate.PreviousAdjustedForceY = gameEngine.StaticVariables.g_sinus[(int)direction * 4] << 9;
            return;
        }
    }

    // GHIDRA: AI_UpdateFollowerBehaviorIfTriggered @ 0x800749A4
    public static void AI_UpdateFollowerBehaviour(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Mille-pattes (projectiles réfléchis)")
        {
            Breakpoint.TriggerBreak();
        }

        if (entity.ParentEntity.Bytes[3] != 0)
        {
            entity.TargetAnimationId = 2;
            entity.Flags |= 0x40;
            return;
        }

        if (entity.TargetAnimationId != 0)
        {
            if (entity.TargetAnimationId != 1)
            {
                return;
            }

            if (entity.ForceResetAnimationFlag == 0)
            {
                return;
            }

            if (entity.Bytes[3] == 0)
            {
                entity.TargetAnimationId = 0;
                return;
            }

            entity.TargetAnimationId = 2;
            entity.Flags |= 0x40;
            return;
        }

        if (entity.AIValues[1] != 0)
        {
            entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
        }

        if (entity.ForceAdjusted == 0)
        {
            return;
        }

        if (entity.ModdedPosX - 0x10000 < 0x780001
            || 0x1F7FFFF < entity.ModdedPosX + entity.Width + 0x10001)
        {
            entity.TargetDirection = (0U - entity.TargetDirection) & 0x1F;
        }
        else if (entity.ModdedPosY - 0x10000 < 0xA00001
                 || 0x1DFFFFF < entity.ModdedPosY + entity.Height + 0x10001)
        {
            entity.TargetDirection = (0x10U - entity.TargetDirection) & 0x1F;
        }

        if (entity.AIValues[1] != 0)
        {
            return;
        }

        entity.TargetAnimationId = 2;
        entity.Flags |= 0x40;
    }

    // GHIDRA: AI_UpdateEntityDelayedSoundTrigger @ 0x80072680
    public static void AI_UpdateEntityDelayed(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

        if ((gameEngine.StaticVariables.g_temporaryFlags[0] & 1U) == 0)
        {
            if (entity.TargetAnimationId == 4)
            {
                short remainingFrames = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = remainingFrames;

                if (remainingFrames == 0)
                {
                    entity.TargetAnimationId = 2;
                    entity.AIValues[1] = 0x20;
                }
            }
            else if (entity.TargetAnimationId == 2)
            {
                short remainingFrames = (short)(entity.AIValues[1] - 1);
                entity.AIValues[1] = remainingFrames;

                if (remainingFrames == 0)
                {
                    gameEngine.SoundManager.PlaySoundEffect(0xB6);
                }
            }

            return;
        }

        entity.TargetAnimationId = 1;
        entity.Flags |= 0x40;
    }

    // GHIDRA: AI_SpawnWarpIfValid @ 0x80061EB8
    public static void AI_SpawnWarpIfValid(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Bras, projectiles")
        {
            Breakpoint.TriggerBreak();
        }

        Entity parentEntity = entity.ParentEntity!;
        uint aiState = entity.TargetAnimationId;

        if (aiState == 9)
        {
            return;
        }

        uint specialState = aiState - 10;
        if (specialState < 6U)
        {
            if (parentEntity.Hp == 0)
            {
                gameEngine.DestroyEntity(entity, -1);
                return;
            }

            if (entity.IsOnGround != 0)
            {
                gameEngine.SoundManager.PlaySoundEffect(0x19D);
                entity.TargetAnimationId = 9;
                entity.Flags |= 0x40U;
                gameEngine.TriggerScreenEffect(unchecked((int)0x60000000), 2, 0, 1);
                return;
            }

            if (entity.Bytes[0] == 1 && aiState == 0xF && entity.PosZ <= parentEntity.PosZ + 0x01000000)
            {
                ulong random1 = Random.Next();
                ulong random2 = Random.Next();
                ulong random3 = Random.Next();

                entity.TargetAnimationId = 10;
                entity.Flags |= 0x100U;
                entity.PosX = (int)((random1 * 0x18UL) >> 32) * 0xC0000 + (int)((random2 * 7UL) >> 32) * 0x10000 + 0x01E00000;
                entity.PosY = (int)((random3 * 0x10UL) >> 32) * 0x80000 + (int)((random3 * 5UL) >> 32) * 0x10000 + 0x02800000;
            }

            if (entity.Bytes[0] != 2)
            {
                return;
            }

            int direction = (int)entity.TargetDirection;
            int scale = entity.Bytes[1] << 11;
            const long Magic = 0x2E8BA2E9L;

            int forceX = gameEngine.StaticVariables.g_offsetXList[direction] * scale;
            int forceXHi = (int)(((long)forceX * Magic) >> 32);
            entity.PreviousAdjustedForceX = (forceXHi >> 1) - (forceX >> 31);

            int forceY = gameEngine.StaticVariables.g_offsetYList[direction] * scale;
            int forceYHi = (int)(((long)forceY * Magic) >> 32);
            entity.PreviousAdjustedForceY = (forceYHi >> 1) - (forceY >> 31);
            return;
        }

        int[] relativePositions = new int[6];
        ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, relativePositions);

        int slotIndex = entity.Bytes[0];
        WarpSlotState warpSlot = gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[slotIndex];
        Entity player = gameEngine.StaticVariables.PlayerEntity;

        if (parentEntity.Hp == 0)
        {
            entity.TargetAnimationId = 7;
            entity.Flags |= 0x40U;

            if (warpSlot.Phase == 0)
            {
                return;
            }

            player.PosX = entity.PosX;
            player.PosY = entity.PosY;
            player.TargetAnimationId = 0;
            gameEngine.StaticVariables.g_playerControlFlags &= 0xFFFFFFDFU;
            warpSlot.Phase = 0;
            return;
        }

        if (warpSlot.Phase == 2 && player.IsOnGround != 0)
        {
            if (player.TargetAnimationId == 0x1C)
            {
                if (player.ForceResetAnimationFlag != 0)
                {
                    if (entity.Bytes[2] == 0)
                    {
                        gameEngine.StaticVariables.g_scrollingParameters.Flag = 1;
                        gameEngine.StaticVariables.g_scrollingParameters.SpeedX = 1;
                        gameEngine.StaticVariables.g_scrollingParameters.SpeedY = 1;
                        gameEngine.StaticVariables.g_scrollingParameters.LimitX = 2;
                        gameEngine.StaticVariables.g_scrollingParameters.LimitY = 2;

                        Entity spawnedEntity = gameEngine.SpawnWarpEntity(
                            entity,
                            1,
                            entity.SpriteTableIndex - 0x100,
                            player.PosX,
                            player.PosY,
                            player.PosZ,
                            0);

                        gameEngine.StaticVariables.g_entitySpawned = spawnedEntity;
                        spawnedEntity.TargetAnimationId = 0x11;
                        spawnedEntity.SpriteProgramIndexes[2] = 0;
                        spawnedEntity.Flags = (spawnedEntity.Flags | 2U) & 0xFFFFFF7FU;
                        player.AnimFlags &= unchecked((int)0xFFFFFFBF);
                        entity.Bytes[2] = 1;
                    }
                    else if (player.DamagedTickCounter != 0)
                    {
                        gameEngine.DestroyEntity(gameEngine.StaticVariables.g_entitySpawned, 0);
                        entity.Bytes[2] = 0;
                        player.TargetAnimationId = 0x39;
                        player.TargetDirection = 0x10;
                    }
                }
            }
            else if (player.TargetAnimationId == 0x4E && player.ForceResetAnimationFlag != 0)
            {
                gameEngine.StaticVariables.g_scrollingParameters.Flag = 0;
                gameEngine.StaticVariables.g_playerControlFlags &= 0xFFFFFFDFU;
                if (player.Hp != 0)
                {
                    player.TargetAnimationId = 0;
                }

                warpSlot.Phase = 0;
            }
        }

        if (entity.Bytes[1] == 0)
        {
            warpSlot.BaseX = gameEngine.StaticVariables.INT_ARRAY_80026cdc[slotIndex];
            warpSlot.BaseY = 0x02A00000;
            entity.Bytes[1] = 1;
        }

        switch ((int)entity.TargetAnimationId)
        {
            case 0:
                if (entity.AIValues[1] != 0)
                {
                    entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                }

                entity.DelayOrAngleOrEntityId = (entity.DelayOrAngleOrEntityId + gameEngine.StaticVariables.INT_ARRAY_80026cdc[slotIndex + 2]) & 0x1FF;
                entity.PosX = warpSlot.BaseX + gameEngine.StaticVariables.g_sinus[entity.DelayOrAngleOrEntityId] * 0x1200;
                entity.PosY = warpSlot.BaseY + gameEngine.StaticVariables.g_cosinus[entity.DelayOrAngleOrEntityId] * 0x1200;
                warpSlot.SavedX = entity.PosX;
                warpSlot.SavedY = entity.PosY;

                if ((gameEngine.StaticVariables.g_temporaryFlags[0] & 2U) != 0
                    && relativePositions[1] < 3
                    && relativePositions[0] < 3
                    && entity.AIValues[1] == 0
                    && gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[slotIndex ^ 1].Phase == 0)
                {
                    entity.TargetAnimationId = 0x10;
                    warpSlot.PlayerX = player.PosX;
                    warpSlot.PlayerY = player.PosY;
                }

                break;

            case 2:
            {
                if ((uint)(entity.PosZ + unchecked((int)0xFF200000)) >= 0x00080000U)
                {
                    entity.ForceZ = 0;
                }

                int deltaX = entity.PosX - warpSlot.SavedX;
                int absDeltaX = deltaX >= 0 ? deltaX : -deltaX;
                if (absDeltaX <= 0x000BFFFF)
                {
                    warpSlot.A0 = 0;
                }

                int deltaY = entity.PosY - warpSlot.SavedY;
                int absDeltaY = deltaY >= 0 ? deltaY : -deltaY;
                if (absDeltaY <= 0x0007FFFF)
                {
                    warpSlot.A1 = 0;
                }

                if (entity.ForceZ == 0 && warpSlot.A0 == 0 && warpSlot.A1 == 0)
                {
                    entity.AIValues[1] = 0x78;
                    warpSlot.BaseX = gameEngine.StaticVariables.INT_ARRAY_80026cdc[slotIndex] + entity.PosX - warpSlot.SavedX;
                    warpSlot.BaseY = entity.PosY - warpSlot.SavedY + 0x02A00000;
                    entity.TargetAnimationId = 0;
                }

                break;
            }

            case 3:
                if (entity.AIValues[1] == 0)
                {
                    bool overlapX;
                    int delta = player.ModdedPosX - entity.ModdedPosX;
                    if (delta < 0)
                    {
                        overlapX = entity.ModdedPosX - player.ModdedPosX < player.Width + 1;
                    }
                    else
                    {
                        overlapX = delta < entity.Width + 1;
                    }

                    if (overlapX)
                    {
                        bool overlapY;
                        delta = player.ModdedPosY - entity.ModdedPosY;
                        if (delta < 0)
                        {
                            overlapY = entity.ModdedPosY - player.ModdedPosY < player.Height + 1;
                        }
                        else
                        {
                            overlapY = delta < entity.Height + 1;
                        }

                        if (overlapY)
                        {
                            bool overlapZ;
                            delta = player.ModdedPosZ - entity.ModdedPosZ;
                            if (delta < 0)
                            {
                                overlapZ = entity.ModdedPosZ - player.ModdedPosZ < player.Depth + 1;
                            }
                            else
                            {
                                overlapZ = delta < entity.Depth + 1;
                            }

                            if (overlapZ
                                && (player.AnimFlags & 0x40) == 0
                                && player.DamagedTickCounter == 0
                                && gameEngine.StaticVariables.WarpSlotState_ARRAY_801910a0[slotIndex ^ 1].Phase == 0)
                            {
                                entity.TargetAnimationId = 4;
                                entity.ForceZ = 0x00020000;
                                entity.Flags &= 0xFFFFFFFEU;

                                delta = warpSlot.BaseX - entity.PosX;
                                if (delta < 0)
                                {
                                    delta += 0x1F;
                                }

                                warpSlot.A0 = delta >> 5;
                                delta = warpSlot.BaseY - entity.PosY;
                                if (delta < 0)
                                {
                                    delta += 0x1F;
                                }

                                warpSlot.A1 = delta >> 5;
                                player.TargetAnimationId = 0x56;
                                gameEngine.StaticVariables.g_playerControlFlags |= 0x20U;
                                player.Flags &= 0xFFFFFEF7U;
                                warpSlot.Phase = 1;
                                break;
                            }
                        }
                    }

                    if (entity.CollidedWithEntityZ == 0 && entity.PosZ < 0x00A00000)
                    {
                        break;
                    }

                    entity.AIValues[1] = 0x14;
                    entity.ForceZ = 0;
                    warpSlot.A0 = 0;
                    warpSlot.A1 = 0;
                    break;
                }

                entity.AIValues[1] = (short)(entity.AIValues[1] - 1);
                if (entity.AIValues[1] != 0)
                {
                    break;
                }

                entity.TargetAnimationId = 2;
                entity.ForceZ = 0x00020000;
                int savedDeltaX = warpSlot.SavedX - entity.PosX;
                if (savedDeltaX < 0)
                {
                    savedDeltaX += 0xF;
                }

                warpSlot.A0 = savedDeltaX >> 4;

                int savedDeltaY = warpSlot.SavedY - entity.PosY;
                if (savedDeltaY < 0)
                {
                    savedDeltaY += 0xF;
                }

                warpSlot.A1 = savedDeltaY >> 4;
                break;

            case 4:
            {
                if (entity.PosZ <= 0x018FFFFF)
                {
                    entity.ForceZ = 0;
                }

                int delta = entity.PosX - warpSlot.BaseX;
                int absDelta = delta >= 0 ? delta : -delta;
                if (absDelta <= 0x000BFFFF)
                {
                    warpSlot.A0 = 0;
                }

                delta = entity.PosY - warpSlot.BaseY;
                absDelta = delta >= 0 ? delta : -delta;
                if (absDelta <= 0x0007FFFF)
                {
                    warpSlot.A1 = 0;
                }

                if (entity.ForceZ == 0 && warpSlot.A0 == 0 && warpSlot.A1 == 0)
                {
                    entity.TargetAnimationId = 5;
                    entity.Flags |= 1U;
                    warpSlot.Phase = 2;
                    player.TargetAnimationId = 0x31;
                    player.TargetDirection = 0;
                    player.Flags |= 0x108U;
                    player.PosX = entity.PosX;
                    player.PosY = entity.PosY;
                }

                player.PreviousAdjustedForceX = warpSlot.A0;
                player.PreviousAdjustedForceY = warpSlot.A1;
                player.ForceZ = entity.ForceZ;
                break;
            }

            case 5:
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 2;
                    entity.ForceZ = unchecked((int)0xFFFC0000);
                }

                savedDeltaX = warpSlot.SavedX - entity.PosX;
                if (savedDeltaX < 0)
                {
                    savedDeltaX += 0xF;
                }

                warpSlot.A0 = savedDeltaX >> 4;

                savedDeltaY = warpSlot.SavedY - entity.PosY;
                if (savedDeltaY < 0)
                {
                    savedDeltaY += 0xF;
                }

                warpSlot.A1 = savedDeltaY >> 4;
                break;

            case 6:
                warpSlot.A0 = 0;
                warpSlot.A1 = 0;
                if (entity.ForceResetAnimationFlag == 0)
                {
                    break;
                }

                if (entity.Bytes[3] != 0)
                {
                    entity.TargetAnimationId = 7;
                    entity.Flags |= 0x40U;
                    if (slotIndex == 0)
                    {
                        parentEntity.DelayOrAngleOrEntityId = 0x708;
                    }
                    else
                    {
                        parentEntity.ItemState = 0x708;
                    }

                    break;
                }

                savedDeltaX = warpSlot.SavedX - entity.PosX;
                if (savedDeltaX < 0)
                {
                    savedDeltaX += 0xF;
                }

                warpSlot.A0 = savedDeltaX >> 4;

                savedDeltaY = warpSlot.SavedY - entity.PosY;
                if (savedDeltaY < 0)
                {
                    savedDeltaY += 0xF;
                }

                warpSlot.A1 = savedDeltaY >> 4;
                entity.TargetAnimationId = 2;
                if (entity.PosZ <= 0x00DFFFFF)
                {
                    entity.ForceZ = 0x00020000;
                }
                else if (entity.PosZ > 0x00E7FFFF)
                {
                    entity.ForceZ = unchecked((int)0xFFFC0000);
                }

                break;

            case 8:
                entity.PosX = warpSlot.BaseX + gameEngine.StaticVariables.g_sinus[entity.DelayOrAngleOrEntityId] * 0x1200;
                entity.PosY = warpSlot.BaseY + gameEngine.StaticVariables.g_cosinus[entity.DelayOrAngleOrEntityId] * 0x1200;
                entity.PosZ = 0x00E00000;
                if (entity.ForceResetAnimationFlag != 0)
                {
                    entity.TargetAnimationId = 0;
                }

                break;

            case 0x10:
                if (entity.ForceResetAnimationFlag == 0)
                {
                    break;
                }

                entity.TargetAnimationId = 3;
                int playerDeltaX = warpSlot.PlayerX - entity.PosX;
                if (playerDeltaX < 0)
                {
                    playerDeltaX += 0xF;
                }

                warpSlot.A0 = playerDeltaX >> 4;

                int playerDeltaY = warpSlot.PlayerY - entity.PosY;
                if (playerDeltaY < 0)
                {
                    playerDeltaY += 0xF;
                }

                warpSlot.A1 = playerDeltaY >> 4;
                break;
        }

        if (warpSlot.A0 != 0)
        {
            entity.PreviousAdjustedForceX = warpSlot.A0;
        }

        if (warpSlot.A1 != 0)
        {
            entity.PreviousAdjustedForceY = warpSlot.A1;
        }

    }

    //80062bc0
    public static void AI_UpdateMelzas2CutsceneChannels(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Tentacule")
        {
            Breakpoint.TriggerBreak();
        }

        AI_Melzas2.AI_UpdateMelzas2CutsceneChannels(gameEngine, entity);
    }

    //800637d8
    public static void AI_UpdateEntityAI_IdleLookAround(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Poulet")
        {
            Breakpoint.TriggerBreak();
        }

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
        else if (direction == 3 && entity.IsOnGround != 0)
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
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Toutou (chien)")
        {
            Breakpoint.TriggerBreak();
        }

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
            entity.DelayOrAngleOrEntityId = entity.PosX;
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

                        deltaX = entity.PosX - entity.DelayOrAngleOrEntityId;
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

                        deltaX = entity.DelayOrAngleOrEntityId - entity.PosX;
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
                    entity.PosX = entity.DelayOrAngleOrEntityId;
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
        //if (!string.IsNullOrEmpty(entity.Name)
        //    && entity.Name != "I07_Bâton magique"
        //    && entity.Name != "I31_Haricots de Jack"
        //    && entity.Name != "I32_Cape de sable"
        //    && entity.Name != "I36_Herbe médicinale"
        //    && entity.Name != "I38_Extrait magique"
        //    && entity.Name != "I39_Breuvage de soin"
        //    && entity.Name != "I43_Tome de la Terre (haut)"
        //    && entity.Name != "I62_Emblème du sang de pigeon"
        //    && entity.Name != "I61_Clé"
        //    && entity.Name != "I69_1 Gilda"
        //    && entity.Name != "I70_5 Gildas"
        //    && entity.Name != "I71_10 Gildas"
        //    && entity.Name != "I72_30 Gildas"
        //    && entity.Name != "I79_Bec en or"
        //    && entity.Name != "I80_Graine magique"
        //    && entity.Name != "I83_Récipient de vie"
        //    && entity.Name != "I84_Petit cœur"
        //    && entity.Name != "I85_Cœur moyen"
        //    && entity.Name != "I86_Grand cœur"
        //    && !string.IsNullOrEmpty(entity.Name))
        //{
        //    Breakpoint.TriggerBreak();
        //}

        int itemState;
        Entity entity2;
        int soundSfxIndex;

        //AlundraEngine.Debug.Debugger.Breakpoint();
        var itemId = entity.SpriteTableIndex - 0x1e;

        if (entity.Bytes[0] == 2)
        {
            if (entity.DelayOrAngleOrEntityId != 0)
            {
                entity.DelayOrAngleOrEntityId -= 1;
                return;
            }

            itemState = entity.ItemState;

            if (itemState == 1)
            {
                //LAB_8007c6f4:
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

                    //LAB_8007c320:
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

            entity2 = gameEngine.StaticVariables.g_entitySlots[entity.AIValues[0]];

            if (entity.AIValues[2] == 0)
            {
                entity2.TargetAnimationId = 0;
            }
            else
            {
                gameEngine.PlayerManager.FUN_80033dbc(gameEngine.StaticVariables.PlayerEntity, itemId);
                gameEngine.SetGameOrTemporaryFlag((uint)entity2.ContentsGameFlag);
            }
        }
        else
        {
            if (entity.IsOnGround != 0 && entity.AIValues.GetInt32(2) != 0)
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

            var delay = entity.DelayOrAngleOrEntityId - 1;

            if (0 < entity.DelayOrAngleOrEntityId)
            {
                entity.DelayOrAngleOrEntityId = delay;

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

                if (res == false && (entity.Bytes[0] | entity.Bytes[1] | entity.Bytes[2] | entity.Bytes[3]) != 0)
                {
                    gameEngine.StaticVariables.g_dropItemTextBuffer = string.Empty;
                }
                else
                {
                    gameEngine.StaticVariables.g_dropItemTextBuffer = gameEngine.EtcRes.GetItemName((int)itemId);
                    gameEngine.StaticVariables.g_dropItemTextBuffer += gameEngine.EtcRes.GetOtherString(0x45);
                }

                uint flag = (ushort)entity.AIValues[0] | ((uint)(ushort)entity.AIValues[1] << 16); //AIValues[0]
                gameEngine.SetGameOrTemporaryFlag(flag);
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
