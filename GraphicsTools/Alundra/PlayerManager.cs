using Alundra.DatasBin;
using Alundra.Gameplay;

namespace Alundra;

public class PlayerManager
{
    private readonly GameEngine _gameEngine;

    public PlayerManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    //80031b50
    public void MovePlayer()
    {/*
        int currentTileIndex;
        int iVar1;
        uint uVar2;
        uint dir;

        StaticVariables.g_activeCollisionEntity = null;
        currentTileIndex = (int)_gameEngine.GetCurrentTileIndex();
        iVar1 = StaticVariables.g_entitySlots[0].Slope_18c;
        StaticVariables.g_currentTileFlags = (uint)StaticVariables.g_tileAttributeLUT[currentTileIndex];
        CheckAndExecuteWarp();

        if (StaticVariables.g_entitySlots[0].IsNotProcessable != 0)
        {
            StaticVariables.g_playerWarpTimer = 0;
            StaticVariables.g_playerEffectTransitionCooldown = 0;
            goto END;
        }

        if ((StaticVariables.g_playerControlFlags & 0x34U) != 0)
        {
            PreparePlayerForWarpEntry(1);
            StaticVariables.g_playerWarpTimer = 0;
            StaticVariables.g_playerEffectTransitionCooldown = 0;
            _gameEngine.UpdatePlayerWarpDirection(1);
            _gameEngine.AnimateWarpEffect();

            if ((StaticVariables.g_playerControlFlags == 0x20) &&
                (dir = _gameEngine.FindWarpFacingDirection()) != 0xffffffff)
            {
                StaticVariables.g_entitySlots[0].DamagedTickCounter = 0x78;
            }

            goto END;
        }

        StaticVariables.g_entitySlots[0].Flags |= 0x100;
        _gameEngine.PreparePlayerForWarpEntry(0);
        dir = _gameEngine.FindWarpFacingDirection();

        if (dir != 0xffffffff)
        {
            StaticVariables.g_playerWarpTimer = 0;
            StaticVariables.g_playerEffectTransitionCooldown = 0;
            _gameEngine.UpdatePlayerWarpDirection(2);
            _gameEngine.MaybeStartWarpAnimation();
            StaticVariables.g_entitySlots[0].TargetAnimationId = 0x31;

            if (iVar1 == 4)
            {
                StaticVariables.g_entitySlots[0].TargetAnimationId = 0x3a;
            }

            StaticVariables.g_entitySlots[0].DamagedTickCounter = 0x78;
            StaticVariables.g_entitySlots[0].TargetDirection = dir;
            goto END;
        }

        if (StaticVariables.g_entitySlots[0].Hp == 0)
        {
            StaticVariables.g_playerWarpTimer = 0;
            StaticVariables.g_playerEffectTransitionCooldown = 0;
            _gameEngine.UpdatePlayerWarpDirection(2);
            _gameEngine.AnimateWarpEffect();

            switch (StaticVariables.g_entitySlots[0].TargetAnimationId)
            {
                case 0x1c:
                    if (iVar1 == 4)
                        StaticVariables.g_entitySlots[0].TargetAnimationId = 0x3c;
                    else if (StaticVariables.g_entitySlots[0].ForceResetAnimationFlag != 0)
                        StaticVariables.g_entitySlots[0].TargetAnimationId = 0x39;
                    break;

                case 0x31:
                case 0x39:
                    if (iVar1 == 4)
                        StaticVariables.g_entitySlots[0].TargetAnimationId = 0x3c;
                    break;

                case 0x3a:
                case 0x3c:
                    if (iVar1 != 4)
                        StaticVariables.g_entitySlots[0].TargetAnimationId = 0x39;
                    break;

                case 0x3b:
                    if (iVar1 == 4 && StaticVariables.g_entitySlots[0].ForceResetAnimationFlag != 0)
                        StaticVariables.g_entitySlots[0].TargetAnimationId = 0x3c;
                    else
                        StaticVariables.g_entitySlots[0].TargetAnimationId = 0x39;
                    break;

                case 0x4e:
                case 0x4f:
                    if (StaticVariables.g_entitySlots[0].ForceResetAnimationFlag != 0)
                    {
                        currentTileIndex = _gameEngine.IsMapUnlocked(0x27);
                        if (currentTileIndex == 0)
                        {
                            StaticVariables.g_isGameEnding = 1;
                            StaticVariables.g_warpType = 8;
                            StaticVariables.g_warpEntryBehavior = 0;
                            StaticVariables.g_desiredMap = 0x1dd;
                            StaticVariables.g_warpTriggerType = 0;
                            StaticVariables.g_playerControlFlags |= 4;
                            break;
                        }
                        _gameEngine.PlayCutscene(0x27);
                        if (iVar1 == 4)
                            goto LAB_80032830;
                        else
                            goto LAB_80031e7c;
                    }
                    break;

                default:
                    if (iVar1 == 4)
                        StaticVariables.g_entitySlots[0].TargetAnimationId = 0x3c;
                    else
                        StaticVariables.g_entitySlots[0].TargetAnimationId = 0x39;
                    break;
            }

            goto END;
        }

        if ((StaticVariables.g_entitySlots[0].TileAttributes & 0x80U) != 0)
        {
            StaticVariables.g_playerWarpTimer = 0;
            StaticVariables.g_playerEffectTransitionCooldown = 0;
            _gameEngine.AnimateWarpEffect();
            _gameEngine.UpdatePlayerWarpDirection(1);

            if (iVar1 == 4)
            {
                LAB_80032830:
                StaticVariables.g_entitySlots[0].TargetAnimationId = 0x1d;
                goto END;
            }

            if (StaticVariables.g_entitySlots[0].IsAboveGround == 0)
            {
                LAB_8003279c:
                StaticVariables.g_entitySlots[0].TargetAnimationId = 0x2d;
                goto END;
            }

            LAB_80031e7c:
            StaticVariables.g_entitySlots[0].TargetAnimationId = 0;
            goto END;
        }

        _gameEngine.MaybeStartWarpAnimation();
        uVar2 = (uint)(StaticVariables.g_padState1.ButtonsHold >> 12);
        dir = StaticVariables.UINT_ARRAY_80022c6c[uVar2];
        if (StaticVariables.UINT_ARRAY_80022c6c[uVar2] == 0xffffffff)
        {
            dir = StaticVariables.g_entitySlots[0].TargetDirection;
        }

        switch (iVar1)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 5:
            case 7:
                break;
            case 4:
                StaticVariables.g_playerWarpTimer = 0;
                StaticVariables.g_playerEffectTransitionCooldown = 0;
                _gameEngine.UpdatePlayerWarpDirection(2);
                switch (StaticVariables.g_entitySlots[0].TargetAnimationId)
                {
                    default:
                        goto switchD_80032650_caseD_d;
                    case 0xf:
                    case 0x1d:
                        StaticVariables.g_entitySlots[0].TargetDirection = dir;
                        iVar1 = _gameEngine.TryHandleWarpTrigger();

                        if (iVar1 == 0) goto END;

                        iVar1 = _gameEngine.CheckWarpTrigger();

                        if (iVar1 != 0)
                        {
                            if (iVar1 == 2)
                            {
                                StaticVariables.g_entitySlots[0].TargetAnimationId = 0x1d;
                            }
                            goto END;
                        }

                        if ((StaticVariables.g_padState1.ButtonsJustPressed & 0xd0) != 0)
                        {
                            StaticVariables.g_entitySlots[0].TargetAnimationId = 0x28;
                            goto END;
                        }

                        if (uVar2 != 0)
                        {
                            StaticVariables.g_entitySlots[0].TargetAnimationId = 0xf;
                            goto END;
                        }
                        goto LAB_80032830;
                    case 0x1c:
                    case 0x31:
                    case 0x3e:
                        StaticVariables.g_entitySlots[0].TargetDirection = StaticVariables.g_entitySlots[0].TargetDirection + 0x10 & 0x1f;
                        goto case 0;
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 0xb:
                    case 0xc:
                    case 0xe:
                    case 0x10:
                    case 0x11:
                    case 0x12:
                    case 0x13:
                    case 0x14:
                    case 0x15:
                    case 0x16:
                    case 0x17:
                    case 0x18:
                    case 0x19:
                    case 0x1f:
                    case 0x20:
                    case 0x21:
                    case 0x22:
                    case 0x23:
                    case 0x24:
                    case 0x25:
                    case 0x26:
                    case 0x29:
                    case 0x2a:
                    case 0x2b:
                    case 0x2c:
                    case 0x2d:
                    case 0x2e:
                    case 0x2f:
                    case 0x30:
                    case 0x32:
                    case 0x33:
                    case 0x34:
                    case 0x35:
                    case 0x36:
                    case 0x39:
                    case 0x3c:
                    case 0x3f:
                    case 0x40:
                    case 0x41:
                    case 0x42:
                    case 0x44:
                    case 0x46:
                    case 0x49:
                    case 0x4b:
                    case 0x4e:
                    case 0x4f:
                        StaticVariables.g_entitySlots[0].TargetAnimationId = 0x1d;
                        break;
                    case 0x28:
                    case 0x3a:
                        break;
                    case 0x3b:
                        iVar1 = _gameEngine.TryHandleWarpTrigger();
                        if (iVar1 != 0)
                        {
                            if (StaticVariables.g_entitySlots[0].ForceResetAnimationFlag == 1)
                            {
                                StaticVariables.g_entitySlots[0].TargetAnimationId = 0x1d;
                                StaticVariables.g_entitySlots[0].TargetDirection = StaticVariables.g_entitySlots[0].TargetDirection + 0x10 & 0x1f;
                            }
                            goto END;
                        }
                        goto LAB_8003270c;
                }
                goto switchD_80032650_caseD_28;
            case 6:
                if ((((uVar2 != 0) && (dir == 0x10)) &&
                     (StaticVariables.g_entitySlots[0].TargetDirection == 0x10)) &&
                    ((StaticVariables.g_entitySlots[0].ForceAdjusted != 0 && (StaticVariables.g_entitySlots[0].ActionState == 0))))
                {
                    StaticVariables.g_entitySlots[0].TargetAnimationId = 0xe;
                }
                break;
            default:
                goto END;
        }

        _gameEngine.UpdatePlayerWarpEffect();
        _gameEngine.UpdateWarpStepProgression();
        _gameEngine.UpdatePlayerWarpDirection(0);




        //===================================== TODO





        END:
        _gameEngine.UpdateWarpEffectState();
        _gameEngine.UpdateEntityFromWarpFlag(StaticVariables.g_entitySlots[0].HpMax);
        _gameEngine.FinalizeWarpEntities((short)StaticVariables.g_entitySlots[0].Hp);
        return;

        switchD_80031dac_caseD_f:
        if (StaticVariables.g_entitySlots[0].IsAboveGround == 0)
        {
            StaticVariables.g_entitySlots[0].TargetAnimationId = 0x2d;
        }
        else
        {
            StaticVariables.g_entitySlots[0].TargetAnimationId = 0;
        }
        switchD_80032650_caseD_28:
        StaticVariables.TryHandleWarpTrigger();
        goto END;*/
    }

    // 8002f120
    private void CheckAndExecuteWarp()
    {
        Portal warpData;
        int iVar1;
        Portal portal;
        int combinedVramFlagsAnd;
        string buffer;
        string fmt;
        uint uVar3;

        combinedVramFlagsAnd = StaticVariables.g_entitySlots[0].CombinedVramFlagsAND;

        if ((StaticVariables.g_debugState < 0)
            && ((StaticVariables.g_debugFlags & 4) != 0)
            && ((StaticVariables.g_debugFlags & 0x8000004) != 0x8000004))
        {
            if (StaticVariables.g_isWarpDisabled == 0)
            {
                StaticVariables.DAT_80098f24 = StaticVariables.DAT_80098f24 + 1;
                //StaticVariables.g_debugMessage += "Attr     : %08X("  + combinedVramFlagsAnd;

                if ((combinedVramFlagsAnd & 4U) == 0)
                {
                    if (((combinedVramFlagsAnd & 0x8000U) == 0) || (StaticVariables.g_playerControlFlags != 0))
                    {
                        //buffer = StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                        //fmt = "None)\n";
                    }
                    else
                    {
                        //string.Format(StaticVariables.g_debugMessage + combinedVramFlagsAnd, "Warp)\n");
                        portal = _gameEngine.GetWarpData();
                        if (portal == null)
                        {
                            //buffer = StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                            //fmt = "No WarpData.\n";
                        }
                        else
                        {
                            uVar3 = (uint)(portal.Flags >> 14);
                            //_gameEngine.PrintDebugWarpInfo(pbVar2, (int)uVar3);

                            ushort requiredInput = StaticVariables.BYTE_ARRAY_80022778[uVar3 * 2];

                            if (((StaticVariables.g_padState1.ButtonsHold & requiredInput) == 0)
                                || (StaticVariables.g_entitySlots[0].CurrentFrameIndex != uVar3))
                            {
                                //buffer = StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                                //fmt = "Warp Not Ready!\n";
                            }
                            else if ((StaticVariables.DAT_80098f24 & 4) == 0)
                            {
                                //buffer = StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                                //fmt = "Warp Ready!\n";
                            }
                            else
                            {
                                //buffer = StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                                //fmt = "\n";
                            }
                        }
                    }
                }
                else
                {
                    //string.Format(StaticVariables.g_debugMessage + combinedVramFlagsAnd, "Hole)\n");
                    portal = _gameEngine.GetWarpData();
                    if (portal == null)
                    {
                        //buffer = StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                        //fmt = "No WarpData.\n";
                    }
                    else
                    {
                        //_gameEngine.PrintDebugWarpInfo(pbVar2, 4);
                        if ((StaticVariables.DAT_80098f24 & 4) == 0)
                        {
                            //buffer = StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                            //fmt = "Warp Ready!\n";
                        }
                        else
                        {
                            //buffer = StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                            //fmt = "\n";
                        }
                    }
                }
            }
            else
            {
                //buffer = StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                //fmt = "WARP DISABLE!\n";
            }

            //string.Format(buffer, fmt);
            return;
        }

        if ((StaticVariables.g_entitySlots[0].CombinedVramFlagsAND & 4U) == 0)
        {
            if ((StaticVariables.g_entitySlots[0].CombinedVramFlagsAND & 0x8000U) == 0)
            {
                return;
            }
            if (StaticVariables.g_playerControlFlags != 0)
            {
                return;
            }

            warpData = _gameEngine.GetWarpData();
            if (warpData == null)
            {
                return;
            }

            uVar3 = (uint)(warpData.Flags >> 14);
            ushort requiredInput = StaticVariables.BYTE_ARRAY_80022778[uVar3 * 2];

            if ((StaticVariables.g_padState1.ButtonsHold & requiredInput) == 0)
            {
                return;
            }
            if (StaticVariables.g_entitySlots[0].CurrentFrameIndex != uVar3)
            {
                return;
            }

            combinedVramFlagsAnd = StaticVariables.g_cardinalDirectionTable[((warpData.Flags & 0x3000) >> 10) * 4];
        }
        else
        {
            warpData = _gameEngine.GetWarpData();
            if (warpData == null)
            {
                return;
            }

            combinedVramFlagsAnd = StaticVariables.g_cardinalDirectionTable[((warpData.Flags & 0x3000) >> 10) * 4];
        }

        HandleWarpTransition(warpData, 0x36, combinedVramFlagsAnd);
    }

    private void HandleWarpTransition(Portal warpData, int warpType, int extraData)
    {
        int targetCamZ;
        int targetCamX;
        int targetCamY;

        if (StaticVariables.g_isWarpDisabled != 0)
        {
            return;
        }

        StaticVariables.g_warpType = (warpData.Flags & 0x70) >> 4;
        StaticVariables.g_desiredMap = (short)StaticVariables.g_mapIdToInternalMapIndexTable[warpData.DestMapId];

        var tileHeight = StaticVariables.MapTileHeight;
        var tileWidth = StaticVariables.MapTileWidth;

        int tileOffsetY = warpData.DestTileY * tileHeight +
                          (StaticVariables.g_entitySlots[0].YPos / 2) +
                          warpData.Y1 * -tileHeight;

        targetCamY = ((tileOffsetY << 16 >> 0x14) * tileHeight + 8) << 16;

        int tileOffsetX = (int)(((uint)warpData.DestTileX * tileWidth +
                                 (StaticVariables.g_entitySlots[0].XPos >> tileHeight) + warpData.X1 * -tileWidth) << 16) >> 0x0f;

        targetCamX = (StaticVariables.g_tileToWorldXTable[tileOffsetX * 2] * tileWidth + 0xc) << 16;

        StaticVariables.g_warpEntryBehavior = StaticVariables.g_warpBehaviorTable[warpData.Flags & 0xf];
        targetCamZ = warpData.ZLevel << 16;

        if (StaticVariables.g_warpType == 3)
        {
            if (StaticVariables.g_desiredMap == StaticVariables.g_currentMap)
            {
                if (StaticVariables.g_entitySlots[0].ActionState == 0)
                {
                    StaticVariables.g_entitySlots[0].XPos = targetCamX;
                    StaticVariables.g_entitySlots[0].YPos = targetCamY;
                    StaticVariables.g_entitySlots[0].ZPos = targetCamZ;
                    return;
                }

                //playerPtr = StaticVariables.g_entitySlots[0].ActionState;
                //(playerPtr + 0x114) = (playerPtr + 0x114) + (targetCamX - StaticVariables.g_entitySlots[0].XPos);
                //(playerPtr + 0x118) = (playerPtr + 0x118) + (targetCamY - StaticVariables.g_entitySlots[0].YPos);
                //(playerPtr + 0x11c) = (playerPtr + 0x11c) + (targetCamZ - StaticVariables.g_entitySlots[0].ZPos);

                StaticVariables.g_entitySlots[0].XPos = targetCamX;
                StaticVariables.g_entitySlots[0].YPos = targetCamY;
                StaticVariables.g_entitySlots[0].ZPos = targetCamZ;
                return;
            }

            //_gameEngine.DoNothing();
            StaticVariables.g_warpType = 0;
        }

        StaticVariables.g_isGameEnding = 1;
        StaticVariables.g_warpTriggerType = warpType;
        StaticVariables.g_warpExtraParam = extraData;
        StaticVariables.g_cameraTargetX = targetCamX;
        StaticVariables.g_cameraTargetY = targetCamY;
        StaticVariables.g_animation_id = targetCamZ;
    }

    // 8002fb14
    public void PreparePlayerForWarpEntry(int param_1)
    {/*
        int frameOffset;
        Entity entityCreated;
        SpriteEffect pSVar1;
        Entity entityCreated2;
        byte efffectId;
        uint uVar4;
        int[] piVar5;
        uint uVar6;
        int animIndex;
        int effectEntityId;
        int[] local_50 = new int[4];
        int[] local_40 = new int[4];
        long local_30;
        long uStack_28;
        long lVar2;
        ulong uVar1;

        switch (StaticVariables.g_entitySlots[0].TargetAnimationId)
        {
            case 4:
            case 0x10:
            case 0x12:
            case 0x14:
            case 0x16:
            case 0x18:
            case 0x19:
            case 0x3f:
            case 0x40:
            case 0x41:
            case 0x42:
            case 0x44:
            case 0x46:
            case 0x49:
            case 0x4b:
                _gameEngine.CheckAndTriggerTileEffect(StaticVariables.g_entitySlots[0]);
                break;
            case 0x29:
                if ((StaticVariables.g_entitySlots[0].HitFrameCounter & 7U) == 0)
                {
                    _gameEngine.PlaySoundEffect(StaticVariables.g_hitSoundEffects[StaticVariables.g_entitySlots[0].Slope_18c]);
                }
                if ((StaticVariables.g_entitySlots[0].Slope_18c - 1U < 2) || (StaticVariables.g_entitySlots[0].Slope_18c == 4))
                {
                    effectEntityId = -0x7ffdd79c;
                    efffectId = 6;
                }
                else
                {
                    effectEntityId = -0x7ffdd7dc;
                    efffectId = StaticVariables.g_sharedBuffer2[10];
                }
                animIndex = StaticVariables.g_entitySlots[0].AnimCompleteCounter - 1;
                if (animIndex < 0) break;
                frameOffset = animIndex * 2;
                if (animIndex > 3)
                {
                    animIndex = 3;
                    frameOffset = 6;
                }
                if ((*(short*)(effectEntityId + frameOffset) & StaticVariables.g_entitySlots[0].HitFrameCounter) == 0)
                {
                    if (efffectId == 0)
                    {
                        entityCreated = null;
                    }
                    else
                    {
                        entityCreated = (Entity)_gameEngine.CreateEffectEntity(0, efffectId, 0,
                            StaticVariables.g_entitySlots[0].XPos,
                            StaticVariables.g_entitySlots[0].YPos,
                            StaticVariables.g_entitySlots[0].FloorHeight);
                    }

                    if (entityCreated != null)
                    {
                        frameOffset = effectEntityId + StaticVariables.g_entitySlots[0].CurrentFrameIndex * 4;
                        animIndex = *(short*)(effectEntityId + 8 + animIndex * 2);
                        uVar6 = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                        entityCreated.ProgramIndexes[2] =
                            *(short*)(frameOffset + 0x12) * animIndex +
                            (int)(((ulong)uVar6 * (ulong)(*(short*)(frameOffset + 0x10) * animIndex + 1)) >> 32);
                        uVar6 = uVar6 * 0x7d2b89dd + 0xe06a02e7;
                        entityCreated.ProgramIndexes[3] =
                            *(short*)(frameOffset + 0x26) * animIndex +
                            (int)(((ulong)uVar6 * (ulong)(*(short*)(frameOffset + 0x24) * animIndex + 1)) >> 32);
                        StaticVariables.g_gameRandomSeed = uVar6 * 0x7d2b89dd + 0xe06a02e7;
                        lVar2 = (long)((ulong)StaticVariables.g_gameRandomSeed *
                            (ulong)(*(short*)(effectEntityId + 0x30) * animIndex + 1));
                        entityCreated.ProgramIndexes[4] = *(short*)(effectEntityId + 0x32) * animIndex + (int)(lVar2 >> 32);
                    }
                }
                break;
        }

        if ((StaticVariables.g_entitySlots[0].XForce == 0) && (StaticVariables.g_entitySlots[0].YForce == 0)) goto SkipEffects;

        switch (StaticVariables.g_entitySlots[0].TargetAnimationId)
        {
            case 1:
            case 7:
                if ((StaticVariables.g_entitySlots[0].HitFrameCounter & 0xfU) == 0)
                {
                    _gameEngine.PlaySoundEffect(StaticVariables.SHORT_ARRAY_800227f4[StaticVariables.g_entitySlots[0].Slope_18c]);
                }
                goto SkipEffects;
            case 3:
                effectEntityId = 0;
                if ((StaticVariables.g_entitySlots[0].HitFrameCounter & 7U) == 0)
                {
                    _gameEngine.PlaySoundEffect(StaticVariables.g_hitSoundEffects[StaticVariables.g_entitySlots[0].Slope_18c]);
                }
                goto CaseEffect;
            case 4:
            case 0x2a:
                effectEntityId = 1;
                CaseEffect:
                if ((StaticVariables.g_entitySlots[0].Slope_18c - 1U < 2) || (StaticVariables.g_entitySlots[0].Slope_18c == 4))
                {
                    animIndex = -0x7ffdd79c;
                    efffectId = 6;
                }
                else
                {
                    animIndex = -0x7ffdd7dc;
                    efffectId = StaticVariables.g_sharedBuffer2[10];
                }
                if ((*(short*)(animIndex + 0x34) & StaticVariables.g_entitySlots[0].HitFrameCounter) != 0) goto SkipEffects;
                pSVar1 = (efffectId == 0) ? null :
                    _gameEngine.CreateEffectEntity(0, efffectId, 0,
                        StaticVariables.g_entitySlots[0].XPos,
                        StaticVariables.g_entitySlots[0].YPos,
                        StaticVariables.g_entitySlots[0].FloorHeight);
                if (pSVar1 != null)
                {
                    effectEntityId = *(short*)(effectEntityId * 2 + animIndex + 0x36);
                    pSVar1.XForce = StaticVariables.g_entitySlots[0].XForce * effectEntityId >> 8;
                    pSVar1.YForce = StaticVariables.g_entitySlots[0].YForce * effectEntityId >> 8;
                    StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    lVar2 = (long)((ulong)StaticVariables.g_gameRandomSeed * (ulong)((int)*(short*)(animIndex + 0x3a) + 1));
                    animIndex = *(short*)(animIndex + 0x3c);
                    pSVar1.ZForce = animIndex + (int)(lVar2 >> 32);
                }
                break;

            case 0x23:
                uVar6 = StaticVariables.g_entitySlots[0].HitFrameCounter & 7;
                goto CaseRandomEffect;
            case 0x24:
                uVar6 = StaticVariables.g_entitySlots[0].HitFrameCounter & 3;
                CaseRandomEffect:
                if (uVar6 != 0) goto SkipEffects;
                pSVar1 = _gameEngine.CreateEffectEntity(0, StaticVariables.g_sharedBuffer2[10], 0,
                    StaticVariables.g_entitySlots[0].XPos,
                    StaticVariables.g_entitySlots[0].YPos,
                    StaticVariables.g_entitySlots[0].FloorHeight);
                if (pSVar1 != null)
                {
                    uVar6 = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    uVar4 = uVar6 * 0x7d2b89dd + 0xe06a02e7;
                    StaticVariables.g_gameRandomSeed = uVar4 * 0x7d2b89dd + 0xe06a02e7;
                    uVar1 = (ulong)StaticVariables.g_gameRandomSeed;
                    pSVar1.X += -0xc0000 + (int)(((ulong)uVar6 * 0x180001) >> 32);
                    pSVar1.Y += -0x80000 + (int)(((ulong)uVar4 * 0x100001) >> 32);
                    animIndex = pSVar1.ZForce + 0x10000;
                    pSVar1.ZForce = animIndex + (int)((uVar1 * 0x10001) >> 32);
                }
                break;
            case 0x28:
                if ((StaticVariables.g_entitySlots[0].HitFrameCounter & 7U) != 0 ||
                    (pSVar1 = _gameEngine.CreateEffectEntity(0, 6, 0,
                        StaticVariables.g_entitySlots[0].XPos,
                        StaticVariables.g_entitySlots[0].YPos,
                        StaticVariables.g_entitySlots[0].FloorHeight)) == null)
                    goto SkipEffects;
                effectEntityId = -StaticVariables.g_entitySlots[0].XForce;
                if (StaticVariables.g_entitySlots[0].XForce > 0)
                {
                    effectEntityId += 3;
                }
                pSVar1.XForce = effectEntityId >> 2;
                effectEntityId = -StaticVariables.g_entitySlots[0].YForce;
                if (StaticVariables.g_entitySlots[0].YForce > 0)
                {
                    effectEntityId += 3;
                }
                pSVar1.YForce = effectEntityId >> 2;
                break;
        }

        SkipEffects:
        switch (StaticVariables.g_entitySlots[0].TargetAnimationId)
        {
            case 2:
            case 6:
            case 10:
            case 11:
            case 20:
            case 21:
            case 22:
            case 23:
            case 31:
            case 38:
            case 43:
            case 44:
            case 45:
            case 46:
            case 47:
            case 48:
            case 65:
            case 66:
            case 70:
            case 75:
                if (((param_1 == 0 || StaticVariables.DAT_80098f30 == 0) && StaticVariables.g_entitySlots[0].IsAboveGround != 0) && StaticVariables.g_entitySlots[0].ZForce < 1)
                {
                    _gameEngine.PlaySoundEffect((int)StaticVariables.SHORT_ARRAY_80022804[StaticVariables.g_entitySlots[0].Slope18c]);
                    if ((StaticVariables.g_entitySlots[0].Slope18c < 1) || (2 < StaticVariables.g_entitySlots[0].Slope18c && StaticVariables.g_entitySlots[0].Slope18c != 4))
                    {
                        effectEntityId = 0;
                        do
                        {
                            if (StaticVariables.g_sharedBuffer2[10] == 0)
                            {
                                entityCreated2 = null;
                            }
                            else
                            {
                                entityCreated2 = _gameEngine.CreateEffectEntity(0, StaticVariables.g_sharedBuffer2[10], 0, StaticVariables.g_entitySlots[0].XPos, StaticVariables.g_entitySlots[0].YPos, StaticVariables.g_entitySlots[0].FloorHeight);
                            }
                            if (entityCreated2 != null)
                            {
                                uVar6 = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                                local_30 = (long)uVar6 * 0x30001;
                                StaticVariables.g_gameRandomSeed = uVar6 * 0x7d2b89dd + 0xe06a02e7;
                                uVar1 = (ulong)StaticVariables.g_gameRandomSeed;
                                entityCreated2.ProgramIndexes[2] = (int)(local_30 >> 32) - 0x18000;
                                entityCreated2.ProgramIndexes[3] = (int)(uVar1 * 0x20001 >> 32) - 0x10000;
                            }
                            effectEntityId++;
                        } while (effectEntityId < 3);
                    }
                    else
                    {
                        _gameEngine.CreateEffectEntity(0, 6, 0, StaticVariables.g_entitySlots[0].XPos, StaticVariables.g_entitySlots[0].YPos, StaticVariables.g_entitySlots[0].FloorHeight);
                    }
                    StaticVariables.DAT_80098f30 = 1;
                }
                break;
            default:
                StaticVariables.DAT_80098f30 = 0;
                break;
        }

        if (param_1 == 0)
        {
            if ((StaticVariables.g_entitySlots[0].HP != 0) && (StaticVariables.g_entitySlots[0].HP * 5 <= StaticVariables.g_entitySlots[0].HPMax) && (--StaticVariables.DAT_80098f2c == -1))
            {
                _gameEngine.PlaySoundEffect(400);
                StaticVariables.DAT_80098f2c = 0x2d;
            }

            if (StaticVariables.g_entitySlots[0].TargetAnimationId == 0x16)
            {
                effectEntityId = 0;
            }
            else if ((int)StaticVariables.g_entitySlots[0].TargetAnimationId < 0x17)
            {
                effectEntityId = 0;
                if (StaticVariables.g_entitySlots[0].TargetAnimationId != 0x12)
                {
                    return;
                }
            }
            else if (StaticVariables.g_entitySlots[0].TargetAnimationId == 0x40)
            {
                effectEntityId = 1;
            }
            else
            {
                effectEntityId = 1;
                if (StaticVariables.g_entitySlots[0].TargetAnimationId != 0x42)
                {
                    return;
                }
            }

            if (StaticVariables.g_entitySlots[0].HitCounter == 0)
            {
                if (StaticVariables.g_entitySlots[0].FrameCollisionData != null)
                {
                    piVar5 = local_50;
                    local_50[2] = (int)StaticVariables.g_tileToWorldXTable[StaticVariables.g_entitySlots[0].HitBoxX >> 0x10];
                    local_50[0] = (int)StaticVariables.g_tileToWorldXTable[StaticVariables.g_entitySlots[0].HitBoxX >> 0x10];
                    local_40[3] = (int)(StaticVariables.g_entitySlots[0].HitBoxY + StaticVariables.g_entitySlots[0].TransformDepth) >> 0x14;
                    local_40[1] = StaticVariables.g_entitySlots[0].HitBoxY >> 0x14;
                    local_40[0] = StaticVariables.g_entitySlots[0].HitBoxY >> 0x14;
                    local_40[2] = local_40[3];
                    local_50[3] = (int)StaticVariables.g_tileToWorldXTable[(int)(StaticVariables.g_entitySlots[0].HitBoxX + StaticVariables.g_entitySlots[0].TransformWidth) >> 0x10];
                    local_50[1] = (int)StaticVariables.g_tileToWorldXTable[(int)(StaticVariables.g_entitySlots[0].HitBoxX + StaticVariables.g_entitySlots[0].TransformWidth) >> 0x10];

                    for (int i = 0; i < 4; i++)
                    {
                        animIndex = piVar5[i];
                        if (animIndex < 1) animIndex = 0;
                        else if (0x33 < animIndex) animIndex = 0x33;

                        frameOffset = piVar5[i + 4];
                        if (frameOffset < 1) frameOffset = 0;
                        else if (0x3b < frameOffset) frameOffset = 0x3b;

                        if ((StaticVariables.g_entitySlots[0].HitBoxZ < ((int)((uint)StaticVariables.g_spriteVRAMPointer[frameOffset * 0xd0 + animIndex * 4 + 0x302 + 3]) << 0x14)) || ((StaticVariables.g_spriteVRAMPointer[frameOffset * 0xd0 + animIndex * 4 + 0x302] & 0x40) != 0))
                        {
                            if (StaticVariables.g_entitySlots[0].IsAboveGround == 0)
                            {
                                StaticVariables.g_entitySlots[0].TargetAnimationId = StaticVariables.BYTE_ARRAY_800228a4[effectEntityId * 2 + 1];
                                return;
                            }
                            StaticVariables.g_entitySlots[0].TargetAnimationId = StaticVariables.BYTE_ARRAY_800228a4[effectEntityId * 2];
                            return;
                        }
                    }
                }
            }
            else if (StaticVariables.g_entitySlots[0].IsAboveGround == 0)
            {
                StaticVariables.g_entitySlots[0].TargetAnimationId = StaticVariables.BYTE_ARRAY_800228a4[effectEntityId * 2 + 1];
            }
            else
            {
                StaticVariables.g_entitySlots[0].TargetAnimationId = StaticVariables.BYTE_ARRAY_800228a4[effectEntityId * 2];
            }
        }*/
    }

}