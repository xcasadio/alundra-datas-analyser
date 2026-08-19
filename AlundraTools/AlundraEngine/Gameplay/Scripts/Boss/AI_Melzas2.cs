using System.Diagnostics;

namespace AlundraEngine.Gameplay.Scripts.Boss;

public static class AI_Melzas2
{
    // GHIDRA: SpawnVerticalWarpColumns @ 0x80061A6C
    //Load function
    public static void SpawnVerticalWarpColumns(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Après Melzas")
        {
            Breakpoint.TriggerBreak();
            return;
        }

        int z = 0xF40000;
        entity.ItemState = 1;
        entity.DelayOrAngleOrEntityId = 1;

        Entity parentEntity = gameEngine.SpawnWarpEntity(entity, 1, 0xD1, 0x2400000, 0x2600000, z, 0);
        parentEntity.Bytes[0] = 0;

        for (int i = 0; i < 7; i++)
        {
            z -= 0xC0000;
            Entity entitySpawned = gameEngine.SpawnWarpEntity(entity, 1, 0xD1, 0x2400000, 0x2600000, z, 0);
            entitySpawned.TargetAnimationId = 7;
            parentEntity.AIValues[2] = (short)entitySpawned.Index;
            parentEntity = entitySpawned;
        }

        z = 0xF40000;
        parentEntity = gameEngine.SpawnWarpEntity(entity, 1, 0xD1, 0x2D00000, 0x2600000, z, 0);
        parentEntity.Bytes[0] = 1;

        for (int i = 0; i < 7; i++)
        {
            z -= 0xC0000;
            Entity entitySpawned = gameEngine.SpawnWarpEntity(entity, 1, 0xD1, 0x2D00000, 0x2600000, z, 0);
            entitySpawned.TargetAnimationId = 7;
            parentEntity.AIValues[2] = (short)entitySpawned.Index;
            parentEntity = entitySpawned;
        }

        gameEngine.SoundManager.PlaySoundEffect(0x19C);
        gameEngine.StaticVariables.g_loaderInitialized = 0;
    }


    // GHIDRA: AI_Melzas2_FinalBoss @ 0x80061D14
    //tick function
    public static void AI_Melzas2_FinalBoss(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Après Melzas")
        {
            Breakpoint.TriggerBreak();
            return;
        }

        uint flagByte = entity.Bytes[3];

        if (flagByte != 0)
        {
            if (entity.TargetAnimationId == 0)
            {
                if (flagByte < 2)
                {
                    UpdateEntityAI_BossExplode(gameEngine, entity);
                    return;
                }

                gameEngine.StaticVariables.g_temporaryFlags[0] |= 1;
                return;
            }

            goto HANDLE_ANIMATION;
        }

        if (entity.TargetAnimationId != 0)
        {
            goto HANDLE_ANIMATION;
        }

        if (entity.DelayOrAngleOrEntityId != 0)
        {
            entity.DelayOrAngleOrEntityId -= 1;

            if (entity.DelayOrAngleOrEntityId == 0)
            {
                Entity warp = gameEngine.SpawnWarpEntity(
                    entity,
                    1,
                    0xD0,
                    0xF00000,
                    0xA00000,
                    0,
                    0
                );

                warp.Bytes[0] = 0;
                warp.Bytes[1] = 0;
                warp.TargetAnimationId = 8;
            }
        }

        if (entity.ItemState != 0)
        {
            entity.ItemState -= 1;

            if (entity.ItemState == 0)
            {
                Entity warp = gameEngine.SpawnWarpEntity(
                    entity,
                    1,
                    0xD0,
                    0xF00000,
                    0xA00000,
                    0,
                    8
                );

                warp.Bytes[0] = 1;
                warp.Bytes[1] = 0;
                warp.TargetAnimationId = 8;

                return;
            }
        }

        return;

    HANDLE_ANIMATION:
        if (entity.ForceResetAnimationFlag == 0)
        {
            return;
        }

        flagByte = entity.Bytes[3];

        if (flagByte != 0)
        {
            entity.AIValues[1] = 0x012C;
            entity.AIValues[5] = 0x001E;
            entity.Flags &= ~(EntityFlags.ClassA | EntityFlags.HitsClassB);
            entity.TargetAnimationId = 0;

            gameEngine.SoundManager.PlaySoundEffect(0x55);

            gameEngine.StaticVariables.g_scrollingParameters.Flag = 1;
            gameEngine.StaticVariables.g_scrollingParameters.SpeedX = 1;
            gameEngine.StaticVariables.g_scrollingParameters.SpeedY = 1;
            gameEngine.StaticVariables.g_scrollingParameters.LimitX = 2;
            gameEngine.StaticVariables.g_scrollingParameters.LimitY = 2;

            return;
        }

        // else (flagByte == 0):
        entity.TargetAnimationId = 0;
        entity.DamagedTickCounter = 0x5A;
    }

    // GHIDRA: UpdateEntityAI_BossExplode @ 0x80080AE0
    public static void UpdateEntityAI_BossExplode(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Dragon"
            && entity.Name != "◆Corps" // wilda
            && entity.Name != "◆Zazan-Roi Muruta"
            && entity.Name != "◆Ronan divin corrompu"
            && entity.Name != "◆Boss lézard Niv.1"
            && entity.Name != "◆Frère Rayon (aîné) Niv.1"
            && entity.Name != "◆Frère Rayon (cadet)"
            && entity.Name != "◆Monsieur Aspiration"
            && entity.Name != "◆Élément Niv.1"
            && entity.Name != "◆Abyss Niv.1"
            && entity.Name != "◆Après Melzas"
            && entity.Name != "◆Slime géant (corps)")
        {
            Breakpoint.TriggerBreak();
        }

        var frameCounter = (int)entity.AIValues[1];
        frameCounter = frameCounter - 1;
        entity.AIValues[1] = (short)frameCounter;

        // if phase (byte[3]) not in [0..2] => skip "periodic effect" block
        uint currentPhase = entity.Bytes[3];
        if (currentPhase < 3)
        {
            // every 8 ticks: play sfx + spawn effect entity at pseudo-random offsets
            if ((frameCounter & 7) == 0)
            {
                gameEngine.SoundManager.PlaySoundEffect(0x2E);

                var rand1 = Random.Next();
                var rand2 = Random.Next();
                int xOffset = (int)(((rand1 * (ulong)((entity.Width >> 0x14) + 1)) >> 0x20) * 0x10);
                xOffset = (xOffset + (int)((rand2 * 0x11) >> 0x20)) * 0x10000;
                
                var rand3 = Random.Next();
                int zOffset = (int)(((rand3 * (ulong)((entity.Depth >> 0x14) + 1)) >> 0x20) * 0x10);
                var effectOffsetZ = (Random.Next() * 0x11) >> 0x20;
                zOffset += (int)(effectOffsetZ * 0x10000);

                //var effectOffsetZ = ((Random.Next() * 0x11) >> 0x20);
                //var xOffset = entity.ModdedPosX + ((int)((Random.Next() * (ulong)((entity.Width >> 0x14) + 1)) >> 0x20) * 0x10 + (int)((Random.Next() * 0x11) >> 0x20)) * 0x10000;
                //var zOffset = entity.ModdedPosZ + ((int)((Random.Next() * (ulong)((entity.Depth >> 0x14) + 1)) >> 0x20) * 0x10 + (int)effectOffsetZ) * 0x10000;

                int effectX = entity.ModdedPosX + xOffset;
                int effectY = entity.ModdedPosY + entity.Height;
                int effectZ = entity.ModdedPosZ + zOffset;

                gameEngine.EffectManager.CreateEffectEntity(0, 0x1B, 0, effectX, effectY, effectZ);
            }
        }

        // --- Phase state machine (entity.Bytes[3]) ---
        currentPhase = entity.Bytes[3];

        if (currentPhase == 2)
        {
            // Phase 2: when aiValues[1] hits 0, trigger effect 0x1E with (0,0), reset timer, phase++
            ushort v = (ushort)entity.AIValues[1];
            if (v == 0)
            {
                gameEngine.TriggerScreenEffect(0, 0x1E, 0, 0);

                entity.AIValues[1] = 0x1E;
                entity.Bytes[3] = (byte)(currentPhase + 1);
            }
            return;
        }

        if (currentPhase < 3)
        {
            if (currentPhase == 1)
            {
                ushort v = (ushort)entity.AIValues[1];
                if (v == 0)
                {
                    gameEngine.TriggerScreenEffect(0xff0000, 0x1E, 1, 1);
                    entity.AIValues[1] = 0x1E;
                    entity.Bytes[3] = (byte)(currentPhase + 1);
                }
                else
                {
                    ushort remainingDelay = (ushort)entity.AIValues[5];
                    remainingDelay = (ushort)(remainingDelay - 1);
                    entity.AIValues[5] = (short)remainingDelay;

                    if (remainingDelay == 0)
                    {
                        gameEngine.TriggerScreenEffect(0xff0000, 4, 0, 1);

                        uint hi = (uint)((Random.Next() * 5ul) >> 32);
                        remainingDelay = (ushort)(((int)hi << 3) + 0x1E);

                        entity.AIValues[5] = (short)remainingDelay;
                    }
                }
            }

            // Phase 0: nothing else to do
            return;
        }

        if (currentPhase == 3)
        {
            ushort v = (ushort)entity.AIValues[1];
            if (v == 0)
            {
                entity.Bytes[3] = 4;
            }
            return;
        }

        return;
    }

    // GHIDRA: AI_UpdateMelzas2CutsceneChannels @ 0x80062BC0
    public static void AI_UpdateMelzas2CutsceneChannels(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Tentacule")
        {
            Breakpoint.TriggerBreak();
        }

        bool bVar1;
        bool bVar2;
        int amplitude;
        uint direction;
        uint uVar2;
        int i;
        CutsceneChannel cutSceneChannel2;
        CutsceneChannel cutSceneChannel;
        Entity parentEntity;
        uint channelIndex;
        int iVar4;
        int iVar5;
        int[] positions = new int[6];
        short angleSwing;
        short angleZ;
        Entity entity2;
        byte state;

        if (gameEngine.StaticVariables.g_loaderInitialized == 0)
        {
            i = 0;
            cutSceneChannel = gameEngine.StaticVariables.CutsceneChannel_ARRAY_80026d30[0];
            var baseXorTarget = new int[2]
            {
                cutSceneChannel.BaseY,
                ((cutSceneChannel.AngleSwing & 0xFFFF) << 16) | (cutSceneChannel.AngleMain & 0xFFFF)
            };
            
            do
            {
                cutSceneChannel2 = gameEngine.StaticVariables.CutsceneChannel_ARRAY_801910f0[i];
                cutSceneChannel2.Amplitude = 0x600;
                cutSceneChannel2.BiasY = 0;
                cutSceneChannel2.BaseY = 0x2200;
                cutSceneChannel2.AngleMain = 0;
                cutSceneChannel2.AngleSwing = 0;
                cutSceneChannel2.AngleZ = 0;
                cutSceneChannel2.ExtraFlagsOrScale = 0x200000;
                cutSceneChannel2.Phase = 0;
                cutSceneChannel2.BaseXorTarget = baseXorTarget[i];

                i++;
            } while (i < 2);

            gameEngine.StaticVariables.g_loaderInitialized = 1;
        }

        if (entity.TargetAnimationId - 7 < 2)
        {
            return;
        }

        channelIndex = entity.Bytes[0];
        amplitude = channelIndex == 0 ? gameEngine.StaticVariables.CutsceneChannel_ARRAY_80026d30[0].Amplitude : gameEngine.StaticVariables.CutsceneChannel_ARRAY_80026d30[0].BiasY;
        cutSceneChannel = gameEngine.StaticVariables.CutsceneChannel_ARRAY_801910f0[channelIndex];
        direction = (uint)cutSceneChannel.AngleMain;
        angleSwing = cutSceneChannel.AngleSwing;
        angleZ = cutSceneChannel.AngleZ;

        parentEntity = entity;

        i = 0;

        do
        {
            iVar5 = ((7 - i) * 0x80) / 7;

            iVar4 = (cutSceneChannel.BaseXorTarget + gameEngine.StaticVariables.g_sinus[direction] * cutSceneChannel.Amplitude) * iVar5;
            if (iVar4 < 0)
            {
                iVar4 += 0x7f;
            }
            parentEntity.PosX = amplitude + (iVar4 >> 7);

            iVar4 = (cutSceneChannel.ExtraFlagsOrScale + gameEngine.StaticVariables.g_cosinus[angleSwing] * cutSceneChannel.BiasY) * iVar5;

            if (iVar4 < 0)
            {
                iVar4 += 0x7f;
            }
            parentEntity.PosY = (iVar4 >> 7) + 0x2600000;

            iVar5 = cutSceneChannel.BaseY * iVar5;
            if (iVar5 < 0)
            {
                iVar5 += 0x7f;
            }

            i += 1;
            direction = (direction + 0x40) & 0x1ff;

            parentEntity.PosZ = gameEngine.StaticVariables.g_cosinus[angleZ] * (iVar5 >> 7) + 0xa00000;

            parentEntity = GetEntityById(gameEngine, parentEntity, parentEntity.AIValues[2]);

        } while (i != 8);

        bVar2 = false;
        cutSceneChannel.AngleMain = (short)((cutSceneChannel.AngleMain + 2U) & 0x1ff);
        bVar1 = false;
        parentEntity = entity.ParentEntity;

        direction = (uint)ScriptHelper.GetDirectionToTarget(
            gameEngine.StaticVariables.g_entitySlots[0].PosX - amplitude,
            gameEngine.StaticVariables.g_entitySlots[0].PosY + -0x2580000
        );

        if (parentEntity.Hp == 0)
        {
            i = 0;

            do
            {
                i += 1;
                entity.Flags &= ~(EntityFlags.ClassA | EntityFlags.HitsClassB);
                entity = GetEntityById(gameEngine, entity, entity.AIValues[2]);
            } while (i != 8);

            return;
        }

        state = entity.Bytes[1];

        switch (state)
        {
            case 1:
            {
                state = entity.Bytes[2];

                if (state == 1)
                {
                    angleSwing = (short)(entity.AIValues[1] - 1);
                    entity.AIValues[1] = angleSwing;

                    if (angleSwing == 0)
                    {
                        entity.AIValues[1] = 0x14;
                        entity.Bytes[2] = (byte)(entity.Bytes[2] + 1);

                        gameEngine.StaticVariables.g_scrollingParameters.LimitX = 2;
                        gameEngine.StaticVariables.g_scrollingParameters.LimitY = 2;
                        gameEngine.StaticVariables.g_scrollingParameters.Flag = 1;
                        gameEngine.StaticVariables.g_scrollingParameters.SpeedX = 1;
                        gameEngine.StaticVariables.g_scrollingParameters.SpeedY = 1;
                    }
                    else
                    {
                        i = cutSceneChannel.BaseXorTarget;

                        if (i < entity.DelayOrAngleOrEntityId)
                        {
                            cutSceneChannel.BaseXorTarget = i + 0x80000;
                            i = cutSceneChannel.BaseXorTarget;
                        }

                        if (entity.DelayOrAngleOrEntityId < i)
                        {
                            cutSceneChannel.BaseXorTarget = i - 0x80000;
                        }

                        if (cutSceneChannel.AngleSwing != 0)
                        {
                            cutSceneChannel.AngleSwing = (short)((cutSceneChannel.AngleSwing + 8U) & 0x1ff);
                        }

                        if (cutSceneChannel.AngleZ != 0x80)
                        {
                            cutSceneChannel.AngleZ = (short)((cutSceneChannel.AngleZ + 8U) & 0x1ff);
                        }
                    }
                }
                else if (state < 2)
                {
                    if (state == 0)
                    {
                        angleSwing = entity.AIValues[1];

                        if (angleSwing == 0)
                        {
                            gameEngine.SoundManager.PlaySoundEffect(0x19e);
                            entity.AIValues[1] = 0x1a;
                            entity.Bytes[2] = (byte)(entity.Bytes[2] + 1);

                            cutSceneChannel.AngleSwing = 0x180;
                            cutSceneChannel.BiasY = 0x3000;
                            cutSceneChannel.AngleZ = 0;
                        }
                        else
                        {
                            LAB_80063254:
                            LAB_80063254(entity, angleSwing, channelIndex, cutSceneChannel);
                        }
                    }
                }
                else if (state == 2)
                {
                    angleSwing = (short)(entity.AIValues[1] - 1);
                    entity.AIValues[1] = angleSwing;

                    if (angleSwing == 0)
                    {
                        gameEngine.StaticVariables.g_scrollingParameters.Flag = 0;

                        LAB_80063148:
                        entity.Bytes[2] = (byte)(entity.Bytes[2] + 1);

                        if (parentEntity.Hp < parentEntity.HpMax / 2)
                        {
                            entity.AIValues[1] = 0x46;
                        }
                        else
                        {
                            LAB_800636b8:
                            entity.AIValues[1] = 100;
                        }
                    }
                }
                else if (state == 3)
                {
                    angleSwing = (short)(entity.AIValues[1] - 1);
                    entity.AIValues[1] = angleSwing;

                    if (angleSwing == 0)
                    {
                        //goto LAB_800636e8;
                        bVar2 = true;
                        goto END;
                    }

                    if (cutSceneChannel.AngleSwing != 0x180)
                    {
                        cutSceneChannel.AngleSwing =
                            (short)((cutSceneChannel.AngleSwing - 4U) & 0x1ff);
                    }

                    if (cutSceneChannel.AngleZ != 0)
                    {
                        cutSceneChannel.AngleZ =
                            (short)((cutSceneChannel.AngleZ - 4U) & 0x1ff);
                    }

                    i = cutSceneChannel.BaseXorTarget;

                    var targetBaseX = GetMelzas2CutsceneTargetBaseX(gameEngine, channelIndex);
                    if (i != targetBaseX)
                    {
                        if (i < targetBaseX)
                        {
                            i += 0x40000;
                        }
                        else
                        {
                            LAB_80063474:
                            amplitude = -0x40000;

                            LAB_80063478:
                            i += amplitude;
                        }

                        cutSceneChannel.BaseXorTarget = i;
                    }

                    LAB_80063480:
                    if (cutSceneChannel.ExtraFlagsOrScale != 0x200000)
                    {
                        cutSceneChannel.ExtraFlagsOrScale += 0x20000;
                    }

                    if (cutSceneChannel.Amplitude != 0x600)
                    {
                        cutSceneChannel.Amplitude += 0x80;
                    }
                }

                break;
            }
            case 0:
            {
                /* NOTE: original writes multiple outputs; you used &positions but then read local_44/local_38 too.
                Keep your variables as-is; assumes CalculateEntityRelativePosition fills them somehow in real code. */
                ScriptHelper.CalculateEntityRelativePosition(entity, gameEngine.StaticVariables.PlayerEntity, positions);
                bVar1 = true;

                if (((gameEngine.StaticVariables.g_playerControlFlags & PlayerControlFlags.ForcedSequence) == 0) && ((gameEngine.StaticVariables.g_temporaryFlags[0] & 2) != 0))
                {
                    if (direction - 6 < 0x15)
                    {
                        LAB_80062f9c:
                        if ((positions[4] < 0) && (gameEngine.StaticVariables.CutsceneChannel_ARRAY_801910f0[channelIndex ^ 1].Phase != 2))
                        {
                            entity.Bytes[1] = 2;
                            cutSceneChannel.Phase = 2;

                            LAB_80062fd4:
                            entity.Bytes[2] = 0;
                            entity.AIValues[1] = 0x18;
                        }
                    }
                    else
                    {
                        if ((4 < positions[1]) || (4 < positions[0]))
                        {
                            if ((direction - 6 < 0x15) || (6 < positions[0] || (6 < positions[1])))
                            {
                                //goto LAB_80062f9c;
                                if ((positions[4] < 0) && (gameEngine.StaticVariables.CutsceneChannel_ARRAY_801910f0[channelIndex ^ 1].Phase != 2))
                                {
                                    entity.Bytes[1] = 2;
                                    cutSceneChannel.Phase = 2;

                                    //LAB_80062fd4:
                                    entity.Bytes[2] = 0;
                                    entity.AIValues[1] = 0x18;
                                }

                                goto END;
                            }

                            entity.Bytes[1] = 3;

                            //goto LAB_80062fd4;
                            entity.Bytes[2] = 0;
                            entity.AIValues[1] = 0x18;
                            goto END;
                        }

                        entity.AIValues[1] = 0x18;
                        entity.Bytes[2] = 0;
                        entity.Bytes[1] = 1;
                        entity.DelayOrAngleOrEntityId = (int)(gameEngine.StaticVariables.g_offsetXList[direction] * 0x2400 & 0xfff80000);
                    }
                }

                break;
            }
            default:
                entity2 = entity;

                switch (state)
                {
                    case 2:
                    {
                        state = entity.Bytes[2];

                        if (state == 1)
                        {
                            uVar2 = 0;

                            if ((ushort)entity.AIValues[4] != 0)
                            {
                                do
                                {
                                    entity2 = GetEntityById(gameEngine, entity2, entity2.AIValues[2]);
                                    uVar2 += 1;
                                } while (uVar2 != (ushort)entity.AIValues[4]);
                            }

                            if (entity2.TargetAnimationId == 8)
                            {
                                if (entity2.ForceResetAnimationFlag != 0)
                                {
                                    entity2.TargetAnimationId = 7;
                                    angleSwing = (short)(entity.AIValues[4] - 1);
                                    entity.AIValues[4] = angleSwing;

                                    if (angleSwing == 0)
                                    {
                                        amplitude = entity.PosY;
                                        entity.Bytes[2] += 1;
                                        i = entity.PosZ + 0x100000;
                                        LAB_80063584:
                                        gameEngine.EffectManager.CreateEffectEntity(1, 0, 0, entity.PosX, amplitude, i);
                                    }
                                }
                            }
                            else
                            {
                                LAB_8006359c:
                                entity2.TargetAnimationId = 8;
                            }
                        }
                        else if (state < 2)
                        {
                            if (state == 0)
                            {
                                angleSwing = entity.AIValues[1];

                                if (angleSwing != 0)
                                {
                                    //goto LAB_80063254;
                                    LAB_80063254(entity, angleSwing, channelIndex, cutSceneChannel);
                                    goto END;
                                }

                                uVar2 = entity.TargetAnimationId;
                                entity.Bytes[2] = 1;
                                entity.AIValues[4] = 7;
                                entity.DelayOrAngleOrEntityId = 0;

                                if (uVar2 == 0)
                                {
                                    entity.TargetAnimationId = 6;
                                }
                                else
                                {
                                    entity.TargetAnimationId = 5;
                                }
                            }
                        }
                        else if (state == 2)
                        {
                            angleSwing = entity.AIValues[1];

                            if (angleSwing == 0)
                            {
                                entity.AIValues[1] = 0x14;
                                gameEngine.SoundManager.PlaySoundEffect(0x19f);

                                entity2 = gameEngine.SpawnWarpEntity(parentEntity, 1, 0xd0,
                                    entity.PosX, entity.PosY, entity.PosZ,
                                    entity.TargetDirection);

                                gameEngine.EffectManager.CreateEffectEntity(1, 0, 1, entity.PosX, entity.PosY, entity.PosZ + 0x100000);

                                if (entity2 != null)
                                {
                                    entity2.TargetAnimationId = 0xf;
                                    entity2.ForceZ = 0x40000;
                                    entity2.Bytes[0] = 1;
                                    entity2.Flags &= ~EntityFlags.ClassA;

                                    i = entity.DelayOrAngleOrEntityId + 1;
                                    entity.DelayOrAngleOrEntityId = i;

                                    if (i == 10)
                                    {
                                        //goto LAB_80063148;
                                        entity.Bytes[2] = (byte)(entity.Bytes[2] + 1);

                                        if (parentEntity.Hp < parentEntity.HpMax / 2)
                                        {
                                            entity.AIValues[1] = 0x46;
                                        }
                                        else
                                        {
                                            LAB_800636b8:
                                            entity.AIValues[1] = 100;
                                        }

                                        goto END;
                                    }
                                }
                            }
                            else
                            {
                                LAB_800635b8:
                                entity.AIValues[1] = (short)(angleSwing - 1);
                            }
                        }
                        else if (state == 3)
                        {
                            angleSwing = (short)(entity.AIValues[1] - 1);
                            entity.AIValues[1] = angleSwing;

                            if (angleSwing != 0)
                            {
                                i = cutSceneChannel.BaseXorTarget;
                                bVar1 = true;

                                var targetBaseX = GetMelzas2CutsceneTargetBaseX(gameEngine, channelIndex);
                                if (i != targetBaseX)
                                {
                                    amplitude = 0x40000;

                                    if (targetBaseX <= i)
                                    {
                                        //goto LAB_80063474;
                                        amplitude = -0x40000;
                                    }

                                    //goto LAB_80063478;
                                    LAB_80063478:
                                    i += amplitude;

                                    cutSceneChannel.BaseXorTarget = i;
                                }

                                //goto LAB_80063480;
                                LAB_80063480:
                                if (cutSceneChannel.ExtraFlagsOrScale != 0x200000)
                                {
                                    cutSceneChannel.ExtraFlagsOrScale += 0x20000;
                                }

                                if (cutSceneChannel.Amplitude != 0x600)
                                {
                                    cutSceneChannel.Amplitude += 0x80;
                                }

                                goto END;
                            }

                            LAB_800636e8:
                            bVar2 = true;
                        }

                        break;
                    }
                    case 3:
                    {
                        state = entity.Bytes[2];

                        switch (state)
                        {
                            case 0:
                                state = entity.Bytes[2];
                                entity.AIValues[4] = 7;
                                entity.Bytes[2] = (byte)(state + 1);
                                break;

                            case 1:
                            {
                                uVar2 = 0;

                                if ((ushort)entity.AIValues[4] != 0)
                                {
                                    do
                                    {
                                        entity2 = GetEntityById(gameEngine, entity2, entity2.AIValues[2]);
                                        uVar2 += 1;
                                    } while (uVar2 != (ushort)entity.AIValues[4]);
                                }

                                if (entity2.TargetAnimationId != 8)
                                {
                                    //goto LAB_8006359c;
                                    entity2.TargetAnimationId = 8;
                                    goto END;
                                }

                                if (entity2.ForceResetAnimationFlag != 0)
                                {
                                    entity2.TargetAnimationId = 7;
                                    angleSwing = (short)(entity.AIValues[4] - 1);
                                    entity.AIValues[4] = angleSwing;

                                    if (angleSwing == 0)
                                    {
                                        amplitude = entity.PosY;
                                        entity.Bytes[2] += 1;
                                        i = entity.PosZ;
                                        //goto LAB_80063584;
                                        gameEngine.EffectManager.CreateEffectEntity(1, 0, 0, entity.PosX, amplitude, i);
                                        goto END;
                                    }
                                }

                                break;
                            }
                            case 2:
                            {
                                angleSwing = entity.AIValues[1];
                                bVar1 = true;

                                if (angleSwing != 0)
                                {
                                    //goto LAB_800635b8;
                                    entity.AIValues[1] = (short)(angleSwing - 1);
                                    goto END;
                                }

                                entity.AIValues[1] = 0xf;

                                i = entity.PosY;
                                amplitude = entity.PosZ;

                                gameEngine.SoundManager.PlaySoundEffect(0x19f);

                                entity2 = gameEngine.SpawnWarpEntity(parentEntity, 1, 0xd0,
                                    entity.PosX, i + 0x80000, amplitude - 0x80000,
                                    entity.TargetDirection);

                                gameEngine.EffectManager.CreateEffectEntity(1, 0, 1, entity.PosX, entity.PosY, entity.PosZ);

                                if (entity2 != null)
                                {
                                    entity2.TargetAnimationId = entity.TargetAnimationId + 10;
                                    entity2.Bytes[0] = 2;
                                    entity2.Flags = (entity2.Flags & ~EntityFlags.ClassA) | EntityFlags.Gravity;
                                    entity2.Bytes[1] = (byte)(entity.AIValues[4] + 2);

                                    angleSwing = (short)(entity.AIValues[4] + 1);
                                    entity.AIValues[4] = angleSwing;

                                    if (angleSwing == 5)
                                    {
                                        entity.Bytes[2] += 1;

                                        if (parentEntity.HpMax / 2 <= parentEntity.Hp)
                                        {
                                            //goto LAB_800636b8;
                                            entity.AIValues[1] = 100;
                                            goto END;
                                        }

                                        entity.AIValues[1] = 0x46;
                                    }
                                }

                                break;
                            }
                            case 3:
                            {
                                angleSwing = (short)(entity.AIValues[1] - 1);
                                entity.AIValues[1] = angleSwing;
                                if (angleSwing == 0)
                                {
                                    //goto LAB_800636e8;
                                    bVar2 = true;
                                    goto END;
                                }

                                break;
                            }
                        }

                        break;
                    }
                }

                break;
        }

        END:
        if (bVar1)
        {
            if (0x14 < direction - 6)
            {
                entity.TargetDirection = direction;
            }

            if (((int)direction < 2) || (direction == 0x1f))
            {
                entity.TargetAnimationId = 0;
            }
            else
            {
                uVar2 = 1;

                if (3 < (int)direction)
                {
                    uVar2 = 2;

                    if (0xf < (int)direction)
                    {
                        if ((int)direction < 0x1d)
                        {
                            uVar2 = 4;
                            if ((int)direction < 0x11)
                            {
                                goto LAB_8006375c;
                            }
                        }
                        else
                        {
                            uVar2 = 3;
                        }
                    }
                }
                entity.TargetAnimationId = uVar2;
            }
        }

        LAB_8006375c:
        if (bVar2)
        {
            i = GetMelzas2CutsceneTargetBaseX(gameEngine, channelIndex);

            cutSceneChannel.ExtraFlagsOrScale = 0x200000;
            cutSceneChannel.Amplitude = 0x600;
            cutSceneChannel.BiasY = 0;
            cutSceneChannel.AngleSwing = 0;
            cutSceneChannel.AngleZ = 0;
            cutSceneChannel.BaseXorTarget = i;

            entity.Bytes[1] = 0;
            cutSceneChannel.Phase = 0;
        }
    }

    // JUSTIFICATION: C# language bridge only
    // RELATION: extracted expression from AI_UpdateMelzas2CutsceneChannels @ 0x80062BC0
    private static int GetMelzas2CutsceneTargetBaseX(GameEngine gameEngine, uint channelIndex)
    {
        var config = gameEngine.StaticVariables.CutsceneChannel_ARRAY_80026d30[0];
        var left = config.BaseY;
        var right = ((config.AngleSwing & 0xFFFF) << 16) | (config.AngleMain & 0xFFFF);
        return channelIndex == 0 ? left : right;
    }


    // JUSTIFICATION: C# language bridge only
    // RELATION: extracted label LAB_80063254 from AI_UpdateMelzas2CutsceneChannels @ 0x80062BC0
    private static void LAB_80063254(Entity entity, short angleSwing, uint channelIndex, CutsceneChannel cutSceneChannel)
    {
        int i;
        entity.AIValues[1] = (short)(angleSwing - 1);

        if (cutSceneChannel.BaseXorTarget != 0)
        {
            i = -0x10000;

            if (channelIndex == 0)
            {
                i = 0x10000;
            }

            cutSceneChannel.BaseXorTarget += i;
        }

        if (cutSceneChannel.ExtraFlagsOrScale != 0)
        {
            cutSceneChannel.ExtraFlagsOrScale -= 0x40000;
        }

        if (cutSceneChannel.Amplitude != 0)
        {
            cutSceneChannel.Amplitude -= 0x80;
        }
    }

    // JUSTIFICATION: C# language bridge only
    // RELATION: resolves original Melzas2 entity pointer chain stored through AIValues[2]
    private static Entity? GetEntityById(GameEngine gameEngine, Entity entity, int id)
    {
        var entityId = id;

        if (entityId == -1)
        {
            return null;
        }

        for (int i = 0; i < gameEngine.StaticVariables.g_numberOfEntities; i++)
        {
            var childEntity = gameEngine.StaticVariables.g_entitySlots[i];

            if (childEntity.Index == entityId)
            {
                return childEntity;
            }
        }

        return null;
    }
}