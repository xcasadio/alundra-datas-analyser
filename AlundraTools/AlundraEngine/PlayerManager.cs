using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;
using System;
using System.Diagnostics;
using AlundraEngine.Gameplay.Scripts;

namespace AlundraEngine;

public class PlayerManager
{
    private readonly GameEngine _gameEngine;

    public PlayerManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    //80031b50
    public void MovePlayer()
    {
        int currentTileIndex;
        int slope;
        uint uVar2;
        uint dir;

        StaticVariables.g_activeCollisionEntity = null;
        currentTileIndex = (int)_gameEngine.GetCurrentTileIndex();
        slope = StaticVariables.PlayerEntity.Slope_18c;
        StaticVariables.g_currentTileFlags = StaticVariables.g_tileAttributeLUT[currentTileIndex];
        CheckAndExecuteWarp();

        if (StaticVariables.PlayerEntity.IsNotProcessable != 0)
        {
            StaticVariables.g_playerWarpTimer = 0;
            Array.Clear(StaticVariables.g_playerEffectTransitionCooldown);
            goto END;
        }

        if ((StaticVariables.g_playerControlFlags & 0x34U) != 0)
        {
            PreparePlayerForWarpEntry(1);
            StaticVariables.g_playerWarpTimer = 0;
            Array.Clear(StaticVariables.g_playerEffectTransitionCooldown);
            UpdatePlayerWarpDirection(1);
            AnimateWarpEffect();

            if (StaticVariables.g_playerControlFlags == 0x20)
            {
                dir = FindWarpFacingDirection();

                if (dir != 0xffffffff)
                {
                    StaticVariables.PlayerEntity.DamagedTickCounter = 0x78;
                }
            }

            goto END;
        }

        StaticVariables.PlayerEntity.Flags |= 0x100;
        PreparePlayerForWarpEntry(0);
        dir = FindWarpFacingDirection();

        if (dir != 0xffffffff)
        {
            StaticVariables.g_playerWarpTimer = 0;
            Array.Clear(StaticVariables.g_playerEffectTransitionCooldown);
            UpdatePlayerWarpDirection(2);
            MaybeStartWarpAnimation();
            StaticVariables.PlayerEntity.TargetAnimationId = 0x31;

            if (slope == 4)
            {
                StaticVariables.PlayerEntity.TargetAnimationId = 0x3a;
            }

            StaticVariables.PlayerEntity.DamagedTickCounter = 0x78;
            StaticVariables.PlayerEntity.TargetDirection = dir;
            goto END;
        }

        //death
        if (StaticVariables.PlayerEntity.Hp == 0)
        {
            StaticVariables.g_playerWarpTimer = 0;
            Array.Clear(StaticVariables.g_playerEffectTransitionCooldown);
            UpdatePlayerWarpDirection(2);
            AnimateWarpEffect();

            switch (StaticVariables.PlayerEntity.TargetAnimationId)
            {
                case 0x1c:
                    if (slope == 4)
                    {
                        StaticVariables.PlayerEntity.TargetAnimationId = 0x3c;
                    }
                    else if (StaticVariables.PlayerEntity.ForceResetAnimationFlag != 0)
                    {
                        StaticVariables.PlayerEntity.TargetAnimationId = 0x39;
                    }

                    break;

                case 0x31:
                case 0x39:
                    if (slope == 4)
                    {
                        StaticVariables.PlayerEntity.TargetAnimationId = 0x3c;
                    }

                    break;

                case 0x3a:
                case 0x3c:
                    if (slope != 4)
                    {
                        StaticVariables.PlayerEntity.TargetAnimationId = 0x39;
                    }

                    break;

                case 0x3b:
                    if (slope == 4 && StaticVariables.PlayerEntity.ForceResetAnimationFlag != 0)
                    {
                        StaticVariables.PlayerEntity.TargetAnimationId = 0x3c;
                    }
                    else
                    {
                        StaticVariables.PlayerEntity.TargetAnimationId = 0x39;
                    }

                    break;

                case 0x4e:
                case 0x4f:
                    if (StaticVariables.PlayerEntity.ForceResetAnimationFlag != 0)
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
                        if (slope == 4)
                        {
                            StaticVariables.PlayerEntity.TargetAnimationId = 0x1d;
                            goto END;
                        }
                        else
                        {
                            StaticVariables.PlayerEntity.TargetAnimationId = 0;
                            goto END;
                        }
                    }
                    break;

                default:
                    if (slope == 4)
                    {
                        StaticVariables.PlayerEntity.TargetAnimationId = 0x3c;
                    }
                    else
                    {
                        StaticVariables.PlayerEntity.TargetAnimationId = 0x39;
                    }

                    break;
            }

            goto END;
        }

        if ((StaticVariables.PlayerEntity.TileAttributes & 0x80U) != 0)
        {
            StaticVariables.g_playerWarpTimer = 0;
            Array.Clear(StaticVariables.g_playerEffectTransitionCooldown);
            AnimateWarpEffect();
            UpdatePlayerWarpDirection(1);

            if (slope == 4)
            {
                LAB_80032830:
                StaticVariables.PlayerEntity.TargetAnimationId = 0x1d;
                goto END;
            }

            if (StaticVariables.PlayerEntity.IsAboveGround == 0)
            {
                LAB_8003279c:
                StaticVariables.PlayerEntity.TargetAnimationId = 0x2d;
                goto END;
            }

            LAB_80031e7c:
            StaticVariables.PlayerEntity.TargetAnimationId = 0;
            goto END;
        }

        MaybeStartWarpAnimation();
        uVar2 = (uint)(StaticVariables.g_padState1.ButtonsHold >> 0xc);
        dir = StaticVariables.UINT_ARRAY_80022c6c[uVar2];
        if (StaticVariables.UINT_ARRAY_80022c6c[uVar2] == 0xffffffff)
        {
            dir = StaticVariables.PlayerEntity.TargetDirection;
        }

        switch (slope)
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
                Array.Clear(StaticVariables.g_playerEffectTransitionCooldown);
                UpdatePlayerWarpDirection(2);
                switch (StaticVariables.PlayerEntity.TargetAnimationId)
                {
                    default:
                        AnimateWarpEffect();
                        goto END;
                    case 0xf:
                    case 0x1d:
                        StaticVariables.PlayerEntity.TargetDirection = dir;
                        slope = TryHandleWarpTrigger();

                        if (slope == 0)
                        {
                            goto END;
                        }

                        slope = CheckWarpTrigger();

                        if (slope != 0)
                        {
                            if (slope == 2)
                            {
                                StaticVariables.PlayerEntity.TargetAnimationId = 0x1d;
                            }
                            goto END;
                        }

                        if ((StaticVariables.g_padState1.ButtonsJustPressed & 0xd0) != 0)
                        {
                            StaticVariables.PlayerEntity.TargetAnimationId = 0x28;
                            goto END;
                        }

                        if (uVar2 != 0)
                        {
                            StaticVariables.PlayerEntity.TargetAnimationId = 0xf;
                            goto END;
                        }
                        StaticVariables.PlayerEntity.TargetAnimationId = 0x1d;
                        goto END;
                    case 0x1c:
                    case 0x31:
                    case 0x3e:
                        StaticVariables.PlayerEntity.TargetDirection = StaticVariables.PlayerEntity.TargetDirection + 0x10 & 0x1f;
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
                        StaticVariables.PlayerEntity.TargetAnimationId = 0x1d;
                        break;
                    case 0x28:
                    case 0x3a:
                        break;
                    case 0x3b:
                        slope = TryHandleWarpTrigger();
                        if (slope != 0)
                        {
                            if (StaticVariables.PlayerEntity.ForceResetAnimationFlag == 1)
                            {
                                StaticVariables.PlayerEntity.TargetAnimationId = 0x1d;
                                StaticVariables.PlayerEntity.TargetDirection = StaticVariables.PlayerEntity.TargetDirection + 0x10 & 0x1f;
                            }
                            goto END;
                        }
                        StaticVariables.PlayerEntity.TargetDirection = StaticVariables.PlayerEntity.TargetDirection + 0x10 & 0x1f;
                        goto END;
                }
                TryHandleWarpTrigger();
                goto END;
            case 6:
                if (uVar2 != 0 
                    && dir == 0x10 
                    && StaticVariables.PlayerEntity.TargetDirection == 0x10 
                    && StaticVariables.PlayerEntity.ForceAdjusted != 0 
                    && StaticVariables.PlayerEntity.WarpEntity == null)
                {
                    StaticVariables.PlayerEntity.TargetAnimationId = 0xe;
                }
                break;
            default:
                goto END;
        }

        UpdatePlayerWarpEffect();
        UpdateWarpStepProgression();
        UpdatePlayerWarpDirection(0);

        int iVar2;

        switch (StaticVariables.PlayerEntity.TargetAnimationId)
        {
            case 0:
            case 1:
                StaticVariables.PlayerEntity.TargetDirection = dir;
                iVar2 = TryHandleWarpTrigger();
                if (iVar2 == 0 || FUN_8002eeac() != 0)
                {
                    break;
                }

                iVar2 = CheckWarpTrigger();
                if (iVar2 != 0)
                {
                    if (iVar2 != 2)
                    {
                        break;
                    }

                    //goto LAB_80031e7c;
                    StaticVariables.PlayerEntity.TargetAnimationId = 0;
                    goto END;
                }
                goto LAB_80031ea8;
            case 2:
            case 0x2b:
            case 0x2c:
            case 0x2d:
                StaticVariables.PlayerEntity.TargetDirection = dir;
                iVar2 = TryHandleWarpTrigger();
                if (iVar2 == 0 || FUN_8002eeac() != 0)
                {
                    break;
                }

                LAB_80031ea8:
                iVar2 = CheckTileWarpTrigger();
                if (iVar2 != 0)
                {
                    break;
                }

                if (StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    if (uVar2 != 0)
                    {
                        StaticVariables.PlayerEntity.TargetAnimationId = 0x2c;
                        break;
                    }
                    goto LAB_80032604;
                }
                if ((StaticVariables.g_padState1.ButtonsJustPressed & 0x40) != 0)
                {
                    if ((StaticVariables.PlayerEntity.CombinedVramFlagsOR & 0x4000U) == 0)
                    {
                        if (uVar2 == 0)
                        {
                            StaticVariables.PlayerEntity.TargetAnimationId = 0x2b;
                        }
                        else
                        {
                            StaticVariables.PlayerEntity.TargetAnimationId = 2;
                        }
                    }
                    break;
                }
                if ((StaticVariables.g_padState1.ButtonsHold & 0x10) != 0)
                {
                    StaticVariables.PlayerEntity.TargetAnimationId = 0x29;
                    StaticVariables.PlayerEntity.AnimCompleteCounter = 0;
                    break;
                }
                if (uVar2 != 0)
                {
                    StaticVariables.PlayerEntity.TargetAnimationId = 1;
                    break;
                }
                goto LAB_800325e0;
            case 3:
                iVar2 = FUN_8002eeac();
                if (iVar2 != 0)
                {
                    break;
                }

                if (StaticVariables.PlayerEntity.IsAboveGround != 0)
                {
                    if (StaticVariables.PlayerEntity.ForceAdjusted == 0)
                    {
                        if (uVar2 == 0)
                        {
                            StaticVariables.PlayerEntity.TargetAnimationId = 0x2a;
                            StaticVariables.INT_ARRAY_80126fe8[3] = 1;
                        }
                        else if ((StaticVariables.g_padState1.ButtonsHold & 0x10) == 0 ||
                                (dir != StaticVariables.UINT_ARRAY_80022cec[StaticVariables.PlayerEntity.CurrentFrameIndex * 3] &&
                                 dir != StaticVariables.UINT_ARRAY_80022cec[StaticVariables.PlayerEntity.CurrentFrameIndex * 3 + 1] &&
                                 dir != StaticVariables.UINT_ARRAY_80022cec[StaticVariables.PlayerEntity.CurrentFrameIndex * 3 + 2]))
                        {
                            StaticVariables.PlayerEntity.TargetAnimationId = 0x2a;
                            StaticVariables.INT_ARRAY_80126fe8[3] = 0;
                        }
                        else if ((StaticVariables.g_padState1.ButtonsHold & 0xe0) != 0)
                        {
                            StaticVariables.PlayerEntity.TargetAnimationId = 4;
                        }
                    }
                    else
                    {
                        _gameEngine.EffectManager.CreateEffectEntity(
                            0, 9, 0,
                            StaticVariables.PlayerEntity.PosX, 
                            StaticVariables.PlayerEntity.PosY,
                            StaticVariables.PlayerEntity.PosZ + 0x100000);
                        StaticVariables.PlayerEntity.TargetAnimationId = 0x3e;
                        StaticVariables.PlayerEntity.TargetDirection = StaticVariables.PlayerEntity.TargetDirection + 0x10 & 0x1f;
                    }
                    break;
                }
                goto LAB_8003279c;
            case 4:
            case 8:
            case 9:
            case 0x10:
            case 0x11:
            case 0x12:
            case 0x13:
            case 0x18:
            case 0x19:
            case 0x25:
            case 0x3f:
            case 0x40:
            case 0x44:
            case 0x49:
                if (StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    StaticVariables.PlayerEntity.TargetAnimationId = 0x2d;
                }
                //goto switchD_80032650_caseD_28;
                TryHandleWarpTrigger();
                goto END;
            case 5:
                iVar2 = TryHandleWarpTrigger();
                if (iVar2 == 0)
                {
                    break;
                }

                if (StaticVariables.PlayerEntity.WarpEntity != null)
                {
                    if (StaticVariables.PlayerEntity.IsAboveGround == 0)
                    {
                        StaticVariables.PlayerEntity.TargetAnimationId = 0x30;
                    }
                    break;
                }
                goto LAB_80032604;
            case 6:
            case 7:
            case 0xc:
            case 0x2e:
            case 0x2f:
            case 0x30:
                StaticVariables.PlayerEntity.TargetDirection = dir;
                iVar2 = TryHandleWarpTrigger();
                var warpEntity = StaticVariables.PlayerEntity.WarpEntity;
                if (iVar2 == 0)
                {
                    break;
                }

                if (StaticVariables.PlayerEntity.WarpEntity != null)
                {
                    if ((StaticVariables.g_padState1.ButtonsJustPressed & 0x80) == 0)
                    {
                        if ((StaticVariables.PlayerEntity.WarpEntity.Flags & 0x600U) == 0x600)
                        {
                            StaticVariables.PlayerEntity.TargetAnimationId = 0x30;
                            if (StaticVariables.PlayerEntity.IsAboveGround != 0)
                            {
                                StaticVariables.PlayerEntity.TargetAnimationId = 0xc;
                            }
                        }
                        else if (StaticVariables.PlayerEntity.IsAboveGround == 0)
                        {
                            if (uVar2 == 0)
                            {
                                StaticVariables.PlayerEntity.TargetAnimationId = 0x30;
                            }
                            else
                            {
                                StaticVariables.PlayerEntity.TargetAnimationId = 0x2f;
                            }
                        }
                        else if ((StaticVariables.g_padState1.ButtonsJustPressed & 0x40) == 0)
                        {
                            if (uVar2 == 0)
                            {
                                StaticVariables.PlayerEntity.TargetAnimationId = 0xc;
                            }
                            else
                            {
                                StaticVariables.PlayerEntity.TargetAnimationId = 7;
                            }
                        }
                        else if ((StaticVariables.PlayerEntity.CombinedVramFlagsOR & 0x4000U) == 0)
                        {
                            if (uVar2 == 0)
                            {
                                StaticVariables.PlayerEntity.TargetAnimationId = 0x2e;
                            }
                            else
                            {
                                StaticVariables.PlayerEntity.TargetAnimationId = 6;
                            }
                        }
                    }
                    else if ((StaticVariables.PlayerEntity.CombinedVramFlagsOR & 0x2000U) == 0)
                    {
                        StaticVariables.PlayerEntity.WarpEntity.TargetDirection =
                            (uint)StaticVariables.g_cardinalDirectionTable[StaticVariables.PlayerEntity.CurrentFrameIndex];
                        warpEntity.PosX = StaticVariables.PlayerEntity.PosX;
                        warpEntity.PosY = StaticVariables.PlayerEntity.PosY;
                        warpEntity.PosZ = StaticVariables.PlayerEntity.PosZ + 0x200000;
                        if (StaticVariables.PlayerEntity.IsAboveGround == 0)
                        {
                            StaticVariables.PlayerEntity.TargetAnimationId = 10;
                            if (uVar2 != 0)
                            {
                                warpEntity.Flags2 = 3;
                                break;
                            }
                        }
                        else
                        {
                            StaticVariables.PlayerEntity.TargetAnimationId = 9;
                            if (uVar2 == 0)
                            {
                                warpEntity.Flags2 = 1;
                                break;
                            }
                        }
                        warpEntity.Flags2 = 2;
                    }
                    break;
                }

                if (StaticVariables.PlayerEntity.IsAboveGround != 0)
                {
                    //goto LAB_80031e7c;
                    StaticVariables.PlayerEntity.TargetAnimationId = 0;
                    goto END;
                }
                LAB_8003279c:
                StaticVariables.PlayerEntity.TargetAnimationId = 0x2d;
                break;
            case 10:
            case 0x14:
            case 0x15:
            case 0x16:
            case 0x17:
            case 0x1f:
            case 0x26:
            case 0x41:
            case 0x42:
            case 0x46:
            case 0x4b:
                if (StaticVariables.PlayerEntity.IsAboveGround != 0)
                {
                    StaticVariables.PlayerEntity.TargetAnimationId = 0;
                }
                //goto switchD_80032650_caseD_28;
                TryHandleWarpTrigger();
                goto END;
            case 0xb:
                iVar2 = TryHandleWarpTrigger();
                if (iVar2 == 0)
                {
                    break;
                }

                if (StaticVariables.PlayerEntity.WarpEntity != null)
                {
                    if (StaticVariables.PlayerEntity.IsAboveGround != 0)
                    {
                        StaticVariables.PlayerEntity.TargetAnimationId = 0xc;
                    }
                    break;
                }
                goto LAB_800325e0;
            default:
                AnimateWarpEffect();
                break;
            case 0xe:
            case 0x35:
                iVar2 = TryHandleWarpTrigger();
                if (iVar2 == 0 || FUN_8002eeac() != 0)
                {
                    break;
                }

                if (uVar2 != 0 && dir != 0 && dir != 0x10)
                {
                    StaticVariables.PlayerEntity.TargetAnimationId = 0x2d;
                    break;
                }
                StaticVariables.PlayerEntity.TargetDirection = 0x10;
                if (uVar2 == 0)
                {
                    StaticVariables.PlayerEntity.TargetAnimationId = 0x35;
                    StaticVariables.PlayerEntity.ForceZ = 0;
                    LAB_800322a8:
                    StaticVariables.PlayerEntity.Flags = StaticVariables.PlayerEntity.Flags & 0xfffffeff;
                }
                else
                {
                    if (dir == 0)
                    {
                        if (StaticVariables.PlayerEntity.FloorHeight + 1 < StaticVariables.PlayerEntity.PosZ)
                        {
                            StaticVariables.PlayerEntity.ForceZ = -0x10000;
                            LAB_8003229c:
                            StaticVariables.PlayerEntity.TargetAnimationId = 0xe;
                            //goto LAB_800322a8;             
                            StaticVariables.PlayerEntity.Flags &= 0xfffffeff;
                            goto END;
                        }
                    }
                    else
                    {
                        if (dir != 0x10)
                        {
                            break;
                        }

                        iVar2 = _gameEngine.EntityGameplayManager.GetTileHeightAtOffset(StaticVariables.PlayerEntity, 0, -0x10000);
                        if (StaticVariables.PlayerEntity.PosZ <= iVar2)
                        {
                            StaticVariables.PlayerEntity.ForceZ = 0x10000;
                            //goto LAB_8003229c;
                            StaticVariables.PlayerEntity.TargetAnimationId = 0xe;
                            StaticVariables.PlayerEntity.Flags &= 0xfffffeff;
                            goto END;
                        }
                    }
                    LAB_80031e7c:
                    StaticVariables.PlayerEntity.TargetAnimationId = 0;
                }
                break;
            case 0xf:
            case 0x1d:
            case 0x28:
            case 0x39:
            case 0x3c:
            case 0x4e:
            case 0x4f:
                //goto switchD_80031dac_caseD_f;
                if (StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    StaticVariables.PlayerEntity.TargetAnimationId = 0x2d;
                }
                else
                {
                    StaticVariables.PlayerEntity.TargetAnimationId = 0;
                }
                TryHandleWarpTrigger();
                goto END;
            case 0x1c:
            case 0x3e:
                iVar2 = TryHandleWarpTrigger();
                if (iVar2 == 0)
                {
                    StaticVariables.PlayerEntity.TargetDirection = StaticVariables.PlayerEntity.TargetDirection + 0x10 & 0x1f;
                    break;
                }
                if (StaticVariables.PlayerEntity.ForceResetAnimationFlag != 1)
                {
                    break;
                }

                StaticVariables.PlayerEntity.TargetDirection = StaticVariables.PlayerEntity.TargetDirection + 0x10 & 0x1f;
                if (StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    StaticVariables.PlayerEntity.TargetAnimationId = (uint)StaticVariables.PlayerEntity.ForceResetAnimationFlag;
                    break;
                }
                goto LAB_80032594;
            case 0x20:
            case 0x24:
                iVar2 = TryHandleWarpTrigger();
                if (iVar2 == 0 || StaticVariables.g_warpLockTimer == 0x20)
                {
                    break;
                }

                goto LAB_8003253c;
            case 0x21:
            case 0x31:
            case 0x34:
                //goto switchD_80032650_caseD_28;
                TryHandleWarpTrigger();
                goto END;
            case 0x22:
            case 0x23:
                StaticVariables.PlayerEntity.TargetDirection = dir;
                iVar2 = TryHandleWarpTrigger();
                if (iVar2 == 0)
                {
                    break;
                }

                if (StaticVariables.g_warpLockTimer == 0x20)
                {
                    if ((StaticVariables.g_padState1.ButtonsJustPressed & 0xd0) == 0)
                    {
                        if (uVar2 == 0)
                        {
                            StaticVariables.PlayerEntity.TargetAnimationId = 0x22;
                        }
                        else
                        {
                            StaticVariables.PlayerEntity.TargetAnimationId = 0x23;
                        }
                    }
                    else
                    {
                        StaticVariables.PlayerEntity.TargetAnimationId = 0x24;
                    }
                    break;
                }
                LAB_8003253c:
                if (StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    StaticVariables.PlayerEntity.TargetAnimationId = 0x2d;
                    break;
                }
                goto LAB_800325e0;
            case 0x29:
                iVar2 = TryHandleWarpTrigger();

                if (iVar2 == 0 
                    || FUN_8002eeac() != 0 
                    || CheckTileWarpTrigger()  != 0)
                {
                    break;
                }

                if (StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    StaticVariables.PlayerEntity.TargetAnimationId = 0x2d;
                    StaticVariables.PlayerEntity.TargetDirection = dir;
                    break;
                }
                StaticVariables.PlayerEntity.TargetDirection = dir;
                if ((StaticVariables.g_padState1.ButtonsHold & 0x10) != 0)
                {
                    StaticVariables.PlayerEntity.TargetDirection = dir;
                    if (StaticVariables.PlayerEntity.AnimCompleteCounter != 0 &&
                       StaticVariables.UINT_ARRAY_80022cac[uVar2] != 0xffffffff)
                    {
                        StaticVariables.PlayerEntity.TargetAnimationId = 3;
                        StaticVariables.PlayerEntity.TargetDirection = StaticVariables.UINT_ARRAY_80022cac[uVar2];
                    }
                    break;
                }
                LAB_80032594:
                StaticVariables.PlayerEntity.TargetAnimationId = 0;
                break;
            case 0x2a:
                iVar2 = TryHandleWarpTrigger();
                if (iVar2 != 0 && FUN_8002eeac() == 0)
                {
                    if (StaticVariables.PlayerEntity.IsAboveGround == 0)
                    {
                        StaticVariables.PlayerEntity.TargetAnimationId = 0x2d;
                    }
                    else if (StaticVariables.INT_ARRAY_80126fe8[3] != 0)
                    {
                        if (StaticVariables.PlayerEntity.ForceAdjusted == 0 &&
                           (StaticVariables.g_padState1.ButtonsHold & 0x10) != 0)
                        {
                            StaticVariables.INT_ARRAY_80126fe8[3] = StaticVariables.INT_ARRAY_80126fe8[3] + 1; 

                            if (10 < StaticVariables.INT_ARRAY_80126fe8[3])
                            {
                                if (StaticVariables.UINT_ARRAY_80022cac[uVar2] != 0xffffffff)
                                {
                                    StaticVariables.PlayerEntity.TargetAnimationId = 3;
                                    StaticVariables.PlayerEntity.TargetDirection = StaticVariables.UINT_ARRAY_80022cac[uVar2];
                                }
                                break;
                            }
                            if (uVar2 == 0)
                            {
                                break;
                            }
                        }
                        StaticVariables.INT_ARRAY_80126fe8[3] = 0;
                    }
                }
                break;
            case 0x32:
            case 0x33:
                iVar2 = TryHandleWarpTrigger();
                if (iVar2 == 0 || StaticVariables.g_warpLockTimer - 0x2bU < 8)
                {
                    break;
                }

                if (StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    goto LAB_80032604;
                }

                LAB_800325e0:
                StaticVariables.PlayerEntity.TargetAnimationId = 0;
                break;
            case 0x36:
                if (StaticVariables.PlayerEntity.IsAboveGround != 0)
                {
                    break;
                }

                LAB_80032604:
                StaticVariables.PlayerEntity.TargetAnimationId = 0x2d;
                break;
            case 0x3a:
            case 0x3b:
                StaticVariables.PlayerEntity.TargetDirection = StaticVariables.PlayerEntity.TargetDirection + 0x10 & 0x1f;
                //goto switchD_80031dac_caseD_f;
                if (StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    StaticVariables.PlayerEntity.TargetAnimationId = 0x2d;
                }
                else
                {
                    StaticVariables.PlayerEntity.TargetAnimationId = 0;
                }
                TryHandleWarpTrigger();
                goto END;
        }

        END:
        UpdateWarpEffectState();
        _gameEngine.UpdateEntityFromWarpFlag(StaticVariables.PlayerEntity.HpMax);
        _gameEngine.FinalizeWarpEntities((short)StaticVariables.PlayerEntity.Hp);
    }

    //8002eaf4
    private int CheckTileWarpTrigger()
    {
        if ((StaticVariables.PlayerEntity.CombinedVramFlagsOR & 0x2000) != 0)
        {
            return 0;
        }

        if ((StaticVariables.g_padState1.ButtonsReleased & 0x0080) == 0)
        {
            return 0;
        }

        if (StaticVariables.g_playerWarpTimer >= 0x3C)
        {
            return 0;
        }

        if (StaticVariables.PlayerEntity.IsAboveGround != 0)
        {
            return 0;
        }

        for (int s0 = 0; s0 < 16; s0++)
        {
            // on lit posY et posZ+0x10 pour positionner l’effet
            int y = StaticVariables.PlayerEntity.PosY;
            int z = StaticVariables.PlayerEntity.PosZ + 0x10;

            // on crée l’effet 3D
            var effect = _gameEngine.EffectManager.CreateEffectEntity(
                0, 0x10, 0,
                StaticVariables.PlayerEntity.PosX,
                StaticVariables.PlayerEntity.PosY,
                StaticVariables.PlayerEntity.PosZ + 0x100000
                );
            if (effect != null)
            {
                // on ajuste les 16 primitives du sprite selon les listes d’offset
                for (int i = 0; i < 16; i++)
                {
                    Debugger.Break(); //TODO check i
                    effect.ForceX = StaticVariables.g_offsetXList[i];
                    effect.ForceY = StaticVariables.g_offsetYList[i];
                }
            }
        }

        _gameEngine.PlaySoundEffect(0x2B);

        // 7) Calcul de l’index de tileWarpData dans la table (bit-bashing fidèle)
        var flags = StaticVariables.g_currentTileFlags;
        var baseIdx = (flags << 1) + flags;              // flags*3
        baseIdx = (baseIdx << 2) + baseIdx;              // baseIdx*5 → flags*15
        int tableBase = unchecked((int)0x80030000) - 0x7387;
        var warpData = tableBase + baseIdx + 8;          // +8 pour g_tileWarpInitFlags
        byte warpType = StaticVariables.g_tileWarpInitFlags[warpData];

        // 8) Si on vient juste d’appuyer (buttonsJustPressed & 0x80), on décale de +4
        if ((StaticVariables.g_padState1.ButtonsJustPressed & 0x0080) != 0)
        {
            if (StaticVariables.PlayerEntity.IsAboveGround != 0)
            {
                return 0;
            }

            var tb = StaticVariables.g_currentTileFlags;
            var idx = (tb << 1) + tb;
            idx = (idx << 2) + idx;                        // tb*15
            warpData = tableBase + idx + 4;                // +4 pour l’autre liste
        }

        // 9) Activation ou désactivation du warp
        byte active = StaticVariables.g_tileWarpInitFlags[warpData];
        if (active != 0)
        {
            // désactive l’effet : on mémorise cooldown et on zappe les flags
            StaticVariables.g_playerEffectTransitionCooldown[0] = (byte)warpData;
            StaticVariables.g_playerEffectStepFlags = 0;
        }
        else
        {
            // active l’animation de transition sur l’entité 0
            StaticVariables.PlayerEntity.TargetAnimationId = (byte)warpData;
            _gameEngine.PlaySoundEffect(3);
        }

        return 0;
    }

    //8002eeac
    private int FUN_8002eeac()
    {
        var player = StaticVariables.PlayerEntity;

        if (player.WarpEntity != null)
        {
            player.TargetAnimationId = (byte)(player.IsAboveGround == 1 ? 0x0C : 0x30);
            return 1;
        }

        if (player.XCollisionEntity != null)
        {
            if ((StaticVariables.g_padState1.ButtonsJustPressed & 0x0080) != 0)
            {
                return TryTriggerWarpFromEntity(player.XCollisionEntity);
            }
        }

        int count = StaticVariables.g_numberOfEntity;
        if (count < 0)
        {
            return 0;
        }

        for (int i = 0; i < count; i++)
        {
            ref var entity = ref StaticVariables.g_entitySlots[i];

            // status ∈ {2, 3}  <=> (status - 2) in [0,1]
            int status = entity.Status - 2;
            if (status >= 0 && status < 2)
            {
                if (entity.IsNotProcessable == 0)
                {
                    if (entity.RidingEntity == StaticVariables.PlayerEntity)
                    {
                        int res = TryTriggerWarpFromEntity(entity);
                        if (res != 0)
                        {
                            return res;
                        }
                    }
                }
            }
        }

        return 0;
    }

    //8002edbc
    private int TryTriggerWarpFromEntity(Entity entity)
    {
        var platformFlagBits = (entity.Flags & 0x600) >> 9;
        int result = 1;

        if (platformFlagBits == 1)
        {
            goto TriggerWarp;
        }

        if (platformFlagBits == 0)
        {
            return 0;
        }

        // Si bits ∈ {2,3} ⇒ test de déverrouillage de la carte (warpIndex = 0x3B)
        if (platformFlagBits < 4)
        {
            var isUnlocked = _gameEngine.IsMapUnlocked(0x3B);
            if (isUnlocked != 0)
            {
                return 0;
            }

            goto TriggerWarp;
        }

        // Cas bits ≥ 4 (impossible avec le masque 0x600) : retourne le résultat par défaut
        return result;

        TriggerWarp:
        StaticVariables.PlayerEntity.WarpEntity = entity;
        entity.PlatformEntity = StaticVariables.PlayerEntity;

        // Selon isAboveGround, on choisit l’animation 0x0C ou 0x30
        var above = StaticVariables.PlayerEntity.IsAboveGround;
        StaticVariables.PlayerEntity.TargetAnimationId = (byte)(above != 0 ? 0x0C : 0x30);

        // On calcule les offsets relatifs X, Y, Z
        StaticVariables.PlayerEntity.RelativeWarpOffsetX = entity.PosX - StaticVariables.PlayerEntity.PosX;
        StaticVariables.PlayerEntity.RelativeWarpOffsetY = entity.PosY - StaticVariables.PlayerEntity.PosY;
        StaticVariables.PlayerEntity.RelativeWarpOffsetZ = entity.PosZ - StaticVariables.PlayerEntity.PosZ;

        return result;
    }

    // 800307e8
    private void UpdateWarpEffectState()
    {
        // Vérifie si la carte est déverrouillée pour différents indices de warp
        if (_gameEngine.IsMapUnlocked(0x1C) != 0)
        {
            StaticVariables.g_gravityFlag = 3;
        }
        else if (_gameEngine.IsMapUnlocked(0x1B) != 0)
        {
            StaticVariables.g_gravityFlag = 2;
        }
        else if (_gameEngine.IsMapUnlocked(0x1A) != 0)
        {
            StaticVariables.g_gravityFlag = 1;
        }
        else
        {
            StaticVariables.g_gravityFlag = 0;
        }

        // Initialisation des tableaux d'objets et d'armes
        int iconIndex = 0x61; // Index 97
        int requiredFlag = 2;
        int iconBase = 0;

        // Nettoie le tableau des items
        StaticVariables.g_items[0] = 0;
        StaticVariables.g_items[1] = 0;
        StaticVariables.g_items[2] = 0;
        StaticVariables.g_items[3] = 0;
        StaticVariables.g_items[4] = 0;
        StaticVariables.g_balanceEffectSources = null;

        // Parcourt les icônes en commençant par l'index 97
        int iconOffset = 97 * 8 + 6; // Offset dans le tableau g_iconNameEtcBase
        while (iconIndex >= 0)
        {
            // Vérifie le bit 0x7F du troisième byte (index+2) de l'icône
            byte iconFlags = (byte)(StaticVariables.g_iconNameEtcBase[iconOffset / 4] & 0x7F);

            if (iconFlags == requiredFlag && _gameEngine.IsMapUnlocked(iconIndex) != 0)
            {
                var itemData = _gameEngine.GetItemDataPointer(iconIndex);
                //StaticVariables.g_balanceEffectSources = itemData;
                StaticVariables.g_items[0] = iconIndex + 0x1E;
                break;
            }

            iconIndex--;
            iconOffset -= 8;
        }

        // Vérification des tuiles actuelles
        int currentTileIndex = (int)_gameEngine.GetCurrentTileIndex();
        if (currentTileIndex > 0 && currentTileIndex < 0x61)
        {
            byte tileIconFlags = (byte)(StaticVariables.g_iconNameEtcBase[(currentTileIndex * 8 + 6) / 4] & 0x7F);

            if (tileIconFlags == 1)
            {
                var itemData = _gameEngine.GetItemDataPointer(currentTileIndex);
                StaticVariables.g_items[2] = itemData;
                StaticVariables.g_items[3] = currentTileIndex + 0x1E;
            }
        }

        // Vérification des warps déclenchés
        int triggeredWarpMapId = _gameEngine.GetTriggeredWarpMapId();
        if (triggeredWarpMapId > 0 && triggeredWarpMapId < 0x61)
        {
            byte warpIconFlags = (byte)(StaticVariables.g_iconNameEtcBase[(triggeredWarpMapId * 8 + 6) / 4] & 0x7F);

            if (warpIconFlags == 3)
            {
                var itemData = _gameEngine.GetItemDataPointer(triggeredWarpMapId);
                StaticVariables.g_items[3] = itemData;
                StaticVariables.g_items[4] = triggeredWarpMapId + 0x1E;
            }
        }

        // Copie les données d'effet de balance
        if (StaticVariables.g_balanceEffectSources != null)
        {
            // Copie de g_balanceEffectSources vers g_intArray_80127008
            Array.Copy(StaticVariables.g_balanceEffectSources, 0, StaticVariables.g_intArray_80127008, 0, 0xF0 / 4);
        }
        else if (StaticVariables.PlayerEntity.BalanceRecord != null)
        {
            StaticVariables.g_intArray_80127008[0] = StaticVariables.PlayerEntity.BalanceRecord;
            // Copie de la BalanceRecord du joueur vers g_intArray_80127008
            //Array.Copy(StaticVariables.PlayerEntity.BalanceRecord, 0, StaticVariables.g_intArray_80127008, 0, 0xF0 / 4);
        }

        // Gestion des valeurs d'effet pour l'animation
        if (StaticVariables.g_playerControlFlags == 0 && StaticVariables.PlayerEntity.IsNotProcessable == 0 &&
            StaticVariables.g_padState1.ButtonsHold == 0)
        {
            // Si le joueur a des points de vie et n'a pas sa vie au maximum
            if (StaticVariables.PlayerEntity.Hp != 0 && StaticVariables.PlayerEntity.Hp < StaticVariables.PlayerEntity.HpMax)
            {
                // Parcourt les effets et incrémente les HP en fonction des animations
                for (int i = 0; i < 3; i++)
                {
                    if (StaticVariables.g_items[i] != null && StaticVariables.g_items[i] != 0)
                    {
                        byte animationFrames = StaticVariables.g_intArray_80127008[i * 2].Hp;

                        if (animationFrames != 0)
                        {
                            StaticVariables.INT_ARRAY_80126fe8[i]++;

                            if (animationFrames <= StaticVariables.INT_ARRAY_80126fe8[i])
                            {
                                StaticVariables.INT_ARRAY_80126fe8[i] = 0;
                                StaticVariables.PlayerEntity.Hp++;
                            }
                        }
                        else
                        {
                            StaticVariables.INT_ARRAY_80126fe8[i] = 0;
                        }
                    }
                    else
                    {
                        StaticVariables.INT_ARRAY_80126fe8[i] = 0;
                    }
                }

                // Vérification que les HP ne dépassent pas le maximum
                if (StaticVariables.PlayerEntity.Hp > StaticVariables.PlayerEntity.HpMax)
                {
                    StaticVariables.PlayerEntity.Hp = StaticVariables.PlayerEntity.HpMax;
                }
            }
        }
        else
        {
            // Réinitialise les compteurs d'effets si le joueur est sous contrôle
            StaticVariables.INT_ARRAY_80126fe8[0] = 0;
            StaticVariables.INT_ARRAY_80126fe8[1] = 0;
            StaticVariables.INT_ARRAY_80126fe8[2] = 0;
        }

        // Affichage des informations de debug si nécessaire
        if (StaticVariables.g_debugState < 0 && (StaticVariables.g_debugFlags & 0x400) != 0)
        {
            //// Code pour l'affichage des informations de debug
            //bool debugHeaderPrinted = false;
            //
            //// Affiche les informations sur les effets actifs
            //for (int i = 0; i < 3; i++)
            //{
            //    if (StaticVariables.g_items[i] != null && StaticVariables.g_items[i] != 0)
            //    {
            //        if (!debugHeaderPrinted)
            //        {
            //            StaticVariables.g_debugMessage += "\n";
            //            debugHeaderPrinted = true;
            //        }
            //
            //        byte numAnimVals = (byte)((StaticVariables.g_intArray_80127008[i * 2] >> 16) & 0xFF);
            //        byte animVal = (byte)((StaticVariables.g_intArray_80127008[i * 2 + 1] >> 8) & 0xFF);
            //        var effectType = StaticVariables.g_effectDebugFlagNames[i];
            //
            //        if (numAnimVals != 0)
            //        {
            //            if (animVal != 0)
            //            {
            //                StaticVariables.g_debugMessage += string.Format("{0}({1:D2}/A{2:D3}/T{3:D3})",
            //                    effectType, StaticVariables.g_items[i] - 0x1E, animVal, StaticVariables.INT_ARRAY_80126fe8[i]);
            //            }
            //            else
            //            {
            //                StaticVariables.g_debugMessage += string.Format("{0}({1:D2}/A{2:D3})",
            //                    effectType, StaticVariables.g_items[i] - 0x1E, animVal);
            //            }
            //        }
            //        else if (numAnimVals != 0)
            //        {
            //            StaticVariables.g_debugMessage += string.Format("{0}({1:D2}/T{2:D3})",
            //                effectType, StaticVariables.g_items[i] - 0x1E, StaticVariables.INT_ARRAY_80126fe8[i]);
            //        }
            //        else
            //        {
            //            StaticVariables.g_debugMessage += string.Format("{0}({1:D2})",
            //                effectType, StaticVariables.g_items[i] - 0x1E);
            //        }
            //    }
            //}
            //
            //if (debugHeaderPrinted)
            //{
            //    StaticVariables.g_debugMessage += "\n";
            //}
            //
            // Affiche les informations sur les sources d'effets
            //if (StaticVariables.g_balanceEffectSources != null)
            //{
            //    _gameEngine.AppendHexVisualDebugLine(StaticVariables.g_balanceEffectSources, "ARM");
            //}
            //else
            //{
            //    _gameEngine.AppendHexVisualDebugLine(StaticVariables.PlayerEntity.BalanceRecord, "ALN");
            //}
            //
            //if (StaticVariables.g_items[2] != null && StaticVariables.g_items[2] != 0)
            //{
            //    _gameEngine.AppendHexVisualDebugLine(StaticVariables.g_items[2], "WEP");
            //}
            //
            //if (StaticVariables.g_items[3] != null && StaticVariables.g_items[3] != 0)
            //{
            //    _gameEngine.AppendHexVisualDebugLine(StaticVariables.g_items[3], "ITM");
            //}
            //
            //_gameEngine.AppendHexVisualDebugLine(StaticVariables.g_intArray_80127008, "DEF");
        }
    }

    // 8002f768
    private void UpdateWarpStepProgression()
    {
        if (StaticVariables.g_playerEffectTransitionCooldown[0] != 0)
        {
            if (StaticVariables.g_playerEffectTransitionCooldown[1] == 0)
            {
                Array.Clear(StaticVariables.g_playerEffectTransitionCooldown);
                return;
            }
        }

        var player = StaticVariables.PlayerEntity;
        var animId = player.TargetAnimationId;
        var warpType = StaticVariables.g_tileWarpTypeList != null && animId < StaticVariables.g_tileWarpTypeList.Length
            ? StaticVariables.g_tileWarpTypeList[animId]
            : (byte)0;

        if (warpType == 0)
        {
            Array.Clear(StaticVariables.g_playerEffectTransitionCooldown);
            return;
        }

        int requiredSteps = StaticVariables.g_playerEffectTransitionCooldown[2];
        int stepCounter = StaticVariables.g_playerEffectStepFlags;

        if (stepCounter < requiredSteps)
        {
            StaticVariables.g_playerEffectStepFlags = stepCounter + 1;
            return;
        }

        int posX = player.PosX;
        int posY = player.PosY;
        int posZ = player.PosZ;
        int zOffset = StaticVariables.g_playerEffectTransitionCooldown[3] << 16;
        int direction = 0;
        if (StaticVariables.g_cardinalDirectionTable != null && player.CurrentFrameIndex < StaticVariables.g_cardinalDirectionTable.Length)
        {
            direction = StaticVariables.g_cardinalDirectionTable[player.CurrentFrameIndex];
        }

        _gameEngine.SpawnWarpEntity(player, 0, StaticVariables.g_playerEffectTransitionCooldown[1],
            posX, posY, posZ + zOffset, (uint)direction);
        Array.Clear(StaticVariables.g_playerEffectTransitionCooldown);
        StaticVariables.g_playerEffectStepFlags = 0;
    }

    //8002f49c
    private int UpdatePlayerWarpEffect()
    {
        // Check if specific pad button (0x80) is held or was just released
        bool buttonHeld = (StaticVariables.g_padState1.ButtonsHold & 0x80) != 0;
        bool buttonReleased = (StaticVariables.g_padState1.ButtonsReleased & 0x80) != 0;

        if (buttonHeld || buttonReleased)
        {
            // Get current tile flags and check warp initialization flags
            uint currentTileFlags = StaticVariables.g_currentTileFlags;
            uint tileWarpInitFlag = (uint)StaticVariables.g_tileWarpInitFlags[currentTileFlags];

            if (tileWarpInitFlag != 0)
            {
                // Check if player animation has warp type
                uint playerAnimId = StaticVariables.PlayerEntity.TargetAnimationId;
                byte warpType = 0;
                if (StaticVariables.g_tileWarpTypeList != null && playerAnimId < StaticVariables.g_tileWarpTypeList.Length)
                {
                    warpType = (byte)StaticVariables.g_tileWarpTypeList[playerAnimId];
                }

                if (warpType != 0)
                {
                    // Increment warp timer (max 0x3C = 60)
                    if (StaticVariables.g_playerWarpTimer < 0x3C)
                    {
                        StaticVariables.g_playerWarpTimer++;
                    }
                }
                else
                {
                    StaticVariables.g_playerWarpTimer = 0;
                }
            }
            else
            {
                StaticVariables.g_playerWarpTimer = 0;
            }
        }
        else
        {
            StaticVariables.g_playerWarpTimer = 0;
        }

        // Main effect processing logic
        if (StaticVariables.g_playerWarpTimer >= 0x0B) // Timer threshold of 11
        {
            // Check if we need to create/manage frame timer effect
            if (StaticVariables.g_playerWarpEffect == null)
            {
                // Create attached effect type 3 with specific parameters
                var attachedEffect = _gameEngine.EffectManager.CreateAttachedEffect(
                    0,    // ismapeffect
                    3,    // effectid  
                    0,    // animid
                    StaticVariables.PlayerEntity, // entity
                    0x10000, // depthsortmod
                    0,    // xoff
                    0,    // yoff
                    0     // zoff
                );

                StaticVariables.g_playerWarpEffect = attachedEffect;
            }
            else
            {
                // Check if effect should be destroyed
                if (StaticVariables.g_playerWarpEffect.Status == 0)
                {
                    StaticVariables.g_playerWarpEffect = null;
                    return StaticVariables.g_playerWarpTimer;
                }
            }

            // Process frame-based effects every 4 frames
            if ((StaticVariables.PlayerEntity.FrameCounter & 0x3) == 0)
            {
                if (StaticVariables.g_playerWarpTimer < 0x3C) // Less than 60
                {
                    // Play sound effect
                    _gameEngine.PlaySoundEffect(0x2A); // Sound ID 42
                }
                else
                {
                    // Create effect entity at player position
                    var spriteEffect = _gameEngine.EffectManager.CreateEffectEntity(
                        0,    // behaviorFlags
                        0x10, // spriteTableIndex (16)
                        0,    // animationIndex
                        StaticVariables.PlayerEntity.PosX,
                        StaticVariables.PlayerEntity.PosY,
                        StaticVariables.PlayerEntity.PosZ + 0x100000 // Z offset
                    );

                    if (spriteEffect != null)
                    {
                        // Generate random sound effect (0x1AC or 0x1AD)
                        uint soundId = (StaticVariables.PlayerEntity.FrameCounter & 0x7) == 0 ? 0x1ADU : 0x1ACU;
                        _gameEngine.PlaySoundEffect(soundId);

                        // Generate random forces using game's random seed
                        StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                        uint randomSeed1 = StaticVariables.g_gameRandomSeed;

                        StaticVariables.g_gameRandomSeed = randomSeed1 * 0x7d2b89dd + 0xe06a02e7;
                        uint randomSeed2 = StaticVariables.g_gameRandomSeed;

                        // Calculate random force components
                        // Complex math for random X force
                        ulong temp1 = (ulong)randomSeed1 * 0x60001;
                        int randomXComponent = (int)(temp1 >> 32);
                        randomXComponent -= 0x30000; // Bias
                        int adjustedXForce = randomXComponent * 5;

                        // Complex math for random Y force  
                        StaticVariables.g_gameRandomSeed = randomSeed2 * 0x7d2b89dd + 0xe06a02e7;
                        ulong temp2 = (ulong)StaticVariables.g_gameRandomSeed * 0x40001;
                        int randomYComponent = (int)(temp2 >> 32);
                        randomYComponent -= 0x20000; // Bias
                        int adjustedYForce = randomYComponent * 5;

                        // Apply forces relative to player's current forces
                        spriteEffect.ForceX = spriteEffect.ForceX + adjustedXForce;
                        spriteEffect.ForceY = spriteEffect.ForceY + adjustedYForce;

                        // Subtract player forces to create relative motion
                        spriteEffect.ForceX = StaticVariables.PlayerEntity.ForceX - randomXComponent;
                        spriteEffect.ForceY = StaticVariables.PlayerEntity.ForceY - randomYComponent;
                    }
                }
            }
        }
        else
        {
            // Timer below threshold - clean up effects
            if (StaticVariables.g_playerWarpEffect != null)
            {
                StaticVariables.g_playerWarpEffect.Status = 0; // Destroy effect
                StaticVariables.g_playerWarpEffect = null;
            }
        }

        return StaticVariables.g_playerWarpTimer;
    }

    // 8002e910
    private int CheckWarpTrigger()
    {
        Debugger.Break();
        return 0;
    }

    // 8002ed64
    private int TryHandleWarpTrigger()
    {
        if ((StaticVariables.g_padState1.ButtonsJustPressed & PadState.Circle) != 0)
        {
            if (StaticVariables.g_playerControlFlags == 0)
            {
                return HandleWarpEvent();
            }
            else
            {
                _gameEngine.PlaySoundEffect(3);
            }
        }

        return 1;
    }

    //8003499c
    private int HandleWarpEvent()
    {
        
    }

    // 8003634c
    private int MaybeStartWarpAnimation()
    {
        int iVar1;

        if (StaticVariables.g_warpLockTimer == 0)
        {
            iVar1 = 1;
        }
        else
        {
            switch (StaticVariables.g_warpLockTimer)
            {
                case 0x1f:
                    iVar1 = FUN_80035204();
                    break;
                case 0x20:
                    iVar1 = FUN_80035260();
                    break;
                default:
                    iVar1 = 1;
                    break;
                case 0x23:
                    iVar1 = FUN_800352c4();
                    break;
                case 0x2b:
                    iVar1 = FUN_80035320();
                    break;
                case 0x2c:
                    iVar1 = FUN_800354d0();
                    break;
                case 0x2d:
                case 0x2e:
                    iVar1 = ProcessPlayerEffectSequence(StaticVariables.g_warpLockTimer);
                    break;
                case 0x2f:
                    iVar1 = FUN_80035a84();
                    break;
                case 0x30:
                    iVar1 = FUN_80035c64();
                    break;
                case 0x31:
                    iVar1 = FUN_80035eb0();
                    break;
                case 0x32:
                    iVar1 = FUN_80036218();
                    break;
            }

            if (StaticVariables.g_playerEffectCurrentFrame < 0x7fffffff)
            {
                StaticVariables.g_playerEffectCurrentFrame = StaticVariables.g_playerEffectCurrentFrame + 1;
            }

            if (iVar1 != 0)
            {
                StaticVariables.g_warpLockTimer = 0;
            }
        }

        return iVar1;
    }

    private int FUN_80036218()
    {

        Debugger.Break();
        return 0;
    }

    private int FUN_80035eb0()
    {

        Debugger.Break();
        return 0;
    }

    private int FUN_80035c64()
    {

        Debugger.Break();
        return 0;
    }

    private int FUN_80035a84()
    {

        Debugger.Break();
        return 0;
    }

    private int ProcessPlayerEffectSequence(int timer)
    {

        Debugger.Break();
        return 0;
    }

    private int FUN_800354d0()
    {

        Debugger.Break();
        return 0;
    }

    private int FUN_80035320()
    {

        Debugger.Break();
        return 0;
    }

    private int FUN_800352c4()
    {

        Debugger.Break();
        return 0;
    }

    private int FUN_80035260()
    {

        Debugger.Break();
        return 0;
    }

    private int FUN_80035204()
    {
        Debugger.Break();
        return 0;
    }

    // 80031a68
    private uint FindWarpFacingDirection()
    {
        uint direction;
        uint entryType;
        int iVar1;
        int hp;
        Entity entity2;

        var player = StaticVariables.PlayerEntity;

        entity2 = player.TouchingEntity;
        if (player.TouchingEntity == null)
        {
            direction = 0xffffffff;
            if (player.DamagedTickCounter == 0)
            {
                direction = 0xffffffff;
                if (StaticVariables.g_gravityFlag < 3)
                {
                    entryType = (uint)((player.CombinedVramFlagsOR & 0x180U) >> 7);
                    direction = 0xffffffff;

                    if (entryType != 0)
                    {
                        iVar1 = StaticVariables.g_warpStepThresholdTable[entryType];
                        hp = 0;

                        if (iVar1 < player.Hp)
                        {
                            hp = player.Hp - iVar1;
                        }

                        direction = player.TargetDirection + 0x10 & 0x1f;
                        player.Hp = hp;
                    }
                }
            }
        }
        else
        {
            _gameEngine.EntityManager.UpdateEntityFacingDirection(player);
            direction = (uint)ScriptHelper.GetDirectionToTarget(player.PosX - entity2.PosX, player.PosY - entity2.PosY);
        }
        return direction;
    }

    // 8002f884
    private void UpdatePlayerWarpDirection(int mode)
    {
        int dx, dy, dz;          /* s2, s1, s0               */
        Entity warpEntity;             /* a3  – pointeur entité-warp*/
        int newDx, newDy, newDz; /* registres temporaires     */
        int v0, v1;              /* registre de travail       */

        /* ---------- prologue (push RA/s0-s2) ------------------- */

        /* récupère le pointeur vers l’entité “effet de warp”      */
        warpEntity = StaticVariables.PlayerEntity.WarpEntity;             /* lw */

        /* si aucune entité-warp active, on saute directement      */
        if (warpEntity == null)                                          /* beq */
        {
            goto LAB_8002FAF0;
        }

        /* si le champ byte 0x0E de balanceRecord est 0 ET mode=1,
           on réinterprète l’appel comme mode=0                    */
        v0 = warpEntity.BalanceRecord.NumAnimVals; // 0x0E
        if (v0 == 0 && mode == 1)                                     /* bne / beq */
        {
            mode = 0;
        }

        /**********************  CAS mode == 0  ********************/
        if (mode == 0)                                                /* beq */
        {
            LAB_8002F8E0:
            /* joueur (slot-0)                                     */
            Entity player = StaticVariables.PlayerEntity;

            /* delta X, Y, Z entre entité-warp et joueur           */
            dx = warpEntity.PosX - player.PosX;
            dy = warpEntity.PosY - player.PosY;
            dz = warpEntity.PosZ - player.PosZ;

            /* |dx|, |dy|                                          */
            v1 = dx >= 0 ? dx : -dx;
            newDy = dy >= 0 ? dy : -dy;

            /* newDz = dz − 0x0020_0000  (0xFFE0 0000)             */
            newDz = dz - 0x200000;
            if (newDz < 0)                                           /* bgez */
            {
                newDz = 0x200000 - dz;
            }

            /* si max(|dx|,|dy|) >= newDz  → on “snap” sur X/Y     */
            if (v1 >= newDy)                                         /* slt */
            {
                /* v1 garde |dx| */
            }
            else
            {
                v1 = newDy;
            }

            if (v1 < newDz)                                          /* slt */
            {   /* assez proche sur Z, on amortit X & Y            */
                /* StepTowards( dx, 0, 0x0001_0000 )               */
                dx = StepTowards(dx, 0, 0x00010000);
                /* StepTowards( dy, 0, 0x0001_0000 )               */
                dy = StepTowards(dy, 0, 0x00010000);
            }
            /* StepTowards( dz, 0x0020_0000, 0x0001_0000 )         */
            dz = StepTowards(dz, 0x00200000, 0x00010000);

            /* stocke les offsets relatifs du joueur               */
            player.RelativeWarpOffsetX = dx;
            player.RelativeWarpOffsetY = dy;
            player.RelativeWarpOffsetZ = dz;
            goto LAB_8002FAF0;
        }

        /**********************  CAS mode == 1  ********************/
        if (mode == 1)                                                /* beq */
        {
            LAB_8002F9AC:
            /* si byte 0x0E non-nul → gestion “delay”              */
            v0 = warpEntity.BalanceRecord.NumAnimVals; // 0x0E
            if (v0 == 0)                                             /* beq */
            {
                goto LAB_8002FAF0;
            }

            /* incrémente g_warpDelayCounter (max 5)               */
            if (StaticVariables.g_warpDelayCounter < 5)
            {
                StaticVariables.g_warpDelayCounter++;
                goto LAB_8002FAAC;                                   /* saute stockage offsets */
            }

            /* recalcul des deltas avec le joueur ---------------- */
            var player = StaticVariables.PlayerEntity;
            dx = warpEntity.PosX - player.PosX;
            dy = warpEntity.PosY - player.PosY;
            dz = warpEntity.PosZ - player.PosZ;

            /* |dx|,|dy|,|dz|                                      */
            newDx = dx >= 0 ? dx : -dx;
            newDy = dy >= 0 ? dy : -dy;
            newDz = dz >= 0 ? dz : -dz;

            /* si déjà très proche (<0x0000_FFFF) sur chaque axe,
               on détruit l’entité-warp et termine                 */
            if (newDx < 0x0000FFFF && newDy < 0x0000FFFF && newDz < 0x0000FFFF)
            {
                _gameEngine.DestroyEntity(warpEntity);
                goto LAB_8002FAF8;
            }

            /* sinon, amortissement avec step 0x0002_0000          */
            dx = StepTowards(dx, 0, 0x00020000);
            dy = StepTowards(dy, 0, 0x00020000);
            dz = StepTowards(dz, 0, 0x00020000);

            /* stocke dans le joueur                               */
            player.RelativeWarpOffsetX = dx;
            player.RelativeWarpOffsetY = dy;
            player.RelativeWarpOffsetZ = dz;
            LAB_8002FAAC:
            /* rien d’autre, on sort                               */
            goto LAB_8002FAF8;
        }

        /**********************  CAS mode == -1  *******************/
        LAB_8002FAB4:                                                     /* (mode autre) */
        {
            var player = StaticVariables.PlayerEntity;

            warpEntity.Flags2 = -1; // TODO flags2 ??
            warpEntity.TargetDirection = player.TargetDirection;

            warpEntity.PosX = player.PosX;
            warpEntity.PosY = player.PosY;
            warpEntity.PosZ = player.PosZ + 0x00200000;

            goto LAB_8002FAF0;
        }

        /**********************  FIN COMMUNES **********************/
        LAB_8002FAF0:
        /* réinitialise le compte à rebours global                */
        StaticVariables.g_warpDelayCounter = 0;

        LAB_8002FAF8:                                                     /* épilogue */
        return;
    }

    // 8002f854
    private int StepTowards(int current, int target, int maxStep)
    {
        int absDelta;
        int delta;

        delta = target - current;
        absDelta = delta;
        if (delta < 0)
        {
            absDelta = -delta;
        }

        if (maxStep <= absDelta)
        {
            target = current + maxStep;

            if (delta < 1)
            {
                target = current - maxStep;
            }
        }

        return target;
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

        combinedVramFlagsAnd = StaticVariables.PlayerEntity.CombinedVramFlagsAND;

        if (StaticVariables.g_debugState < 0
            && (StaticVariables.g_debugFlags & 4) != 0
            && (StaticVariables.g_debugFlags & 0x8000004) != 0x8000004)
        {
            if (StaticVariables.g_isWarpDisabled == 0)
            {
                StaticVariables.DAT_80098f24 = StaticVariables.DAT_80098f24 + 1;
                //StaticVariables.g_debugMessage += "Attr     : %08X("  + combinedVramFlagsAnd;

                if ((combinedVramFlagsAnd & 4U) == 0)
                {
                    if ((combinedVramFlagsAnd & 0x8000U) == 0 || StaticVariables.g_playerControlFlags != 0)
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

                            if ((StaticVariables.g_padState1.ButtonsHold & requiredInput) == 0
                                || StaticVariables.PlayerEntity.CurrentFrameIndex != uVar3)
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

        if ((StaticVariables.PlayerEntity.CombinedVramFlagsAND & 4U) == 0)
        {
            if ((StaticVariables.PlayerEntity.CombinedVramFlagsAND & 0x8000U) == 0)
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

            if (StaticVariables.PlayerEntity.CurrentFrameIndex != uVar3)
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

    // 80031340
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
                          StaticVariables.PlayerEntity.PosY / 2 +
                          warpData.Y1 * -tileHeight;

        targetCamY = ((tileOffsetY << 16 >> 0x14) * tileHeight + 8) << 16;

        int tileOffsetX = (int)(((uint)warpData.DestTileX * tileWidth +
                                 (StaticVariables.PlayerEntity.PosX >> tileHeight) + warpData.X1 * -tileWidth) << 16) >> 0x0f;

        targetCamX = (StaticVariables.g_tileToWorldXTable[tileOffsetX * 2] * tileWidth + 0xc) << 16;

        StaticVariables.g_warpEntryBehavior = StaticVariables.g_warpBehaviorTable[warpData.Flags & 0xf];
        targetCamZ = warpData.ZLevel << 16;

        if (StaticVariables.g_warpType == 3)
        {
            if (StaticVariables.g_desiredMap == StaticVariables.g_currentMap)
            {
                if (StaticVariables.PlayerEntity.WarpEntity == null)
                {
                    StaticVariables.PlayerEntity.PosX = targetCamX;
                    StaticVariables.PlayerEntity.PosY = targetCamY;
                    StaticVariables.PlayerEntity.PosZ = targetCamZ;
                    return;
                }

                //playerPtr = StaticVariables.PlayerEntity.WarpEntity;
                //(playerPtr + 0x114) = (playerPtr + 0x114) + (targetCamX - StaticVariables.PlayerEntity.PosX);
                //(playerPtr + 0x118) = (playerPtr + 0x118) + (targetCamY - StaticVariables.PlayerEntity.PosY);
                //(playerPtr + 0x11c) = (playerPtr + 0x11c) + (targetCamZ - StaticVariables.PlayerEntity.PosZ);

                StaticVariables.PlayerEntity.PosX = targetCamX;
                StaticVariables.PlayerEntity.PosY = targetCamY;
                StaticVariables.PlayerEntity.PosZ = targetCamZ;
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
    {
        // Variables locales pour stocker les états et résultats temporaires
        int effectEntityId = 0;
        byte effectId = 0;
        int animIndex = 0;
        int frameOffset = 0;
        SpriteEffect entityCreated = null;
        SpriteEffect spriteEffect = null;

        // Récupération de l'animation actuelle du joueur
        uint playerAnimId = StaticVariables.PlayerEntity.TargetAnimationId;

        // Traitement en fonction de l'animation actuelle du joueur
        switch (playerAnimId)
        {
            // Cas où l'animation du joueur nécessite un déclenchement des effets de tuile
            case 4:  // Ces valeurs correspondent à différentes animations de mouvement
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
                _gameEngine.CheckAndTriggerTileEffect(StaticVariables.PlayerEntity);
                break;

            // Animation de type "frappe"
            case 0x29:
                // Jouer un son à intervalle régulier
                if ((StaticVariables.PlayerEntity.FrameCounter & 7) == 0)
                {
                    _gameEngine.PlaySoundEffect((uint)StaticVariables.g_hitSoundEffects[StaticVariables.PlayerEntity.Slope_18c]);
                }

                // Déterminer le type d'effet à créer en fonction du type de terrain (pente)
                if (StaticVariables.PlayerEntity.Slope_18c - 1 < 2 || StaticVariables.PlayerEntity.Slope_18c == 4)
                {
                    effectEntityId = 0; // Utilise les données d'effet standard
                    effectId = 6;
                }
                else
                {
                    effectEntityId = 1; // Utilise les données d'effet alternatif
                    effectId = (byte)_gameEngine.CurrentMap.Info._10;
                }

                // Calcul du retard d'animation
                animIndex = StaticVariables.PlayerEntity.AnimCompleteCounter - 1;
                if (animIndex < 0)
                {
                    break;
                }

                frameOffset = animIndex * 2;
                if (animIndex > 3)
                {
                    animIndex = 3;
                    frameOffset = 6;
                }

                // Si le compteur de frame n'est pas dans la plage des effets, ne rien faire
                if ((StaticVariables.g_hitSoundEffects[effectEntityId + frameOffset] & StaticVariables.PlayerEntity.FrameCounter) != 0)
                {
                    break;
                }

                // Créer un effet à la position du joueur
                if (effectId != 0)
                {
                    spriteEffect = _gameEngine.EffectManager.CreateEffectEntity(
                        0,
                        effectId,
                        0,
                        StaticVariables.PlayerEntity.PosX,
                        StaticVariables.PlayerEntity.PosY,
                        StaticVariables.PlayerEntity.TerrainHeight);
                }

                // Si l'effet a été créé avec succès, configurer ses propriétés
                if (spriteEffect != null)
                {
                    // Calculer les forces aléatoires pour l'effet
                    frameOffset = effectEntityId + StaticVariables.PlayerEntity.CurrentFrameIndex * 4;
                    int randomMult = StaticVariables.g_hitSoundEffects[animIndex * 2 + 8];

                    // Générer des valeurs aléatoires pour les forces
                    StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;

                    // Appliquer les forces dans différentes directions avec des composantes aléatoires
                    spriteEffect.ForceX = StaticVariables.g_hitSoundEffects[frameOffset + 0x12] * randomMult +
                        (int)(((ulong)StaticVariables.g_gameRandomSeed * (ulong)(StaticVariables.g_hitSoundEffects[frameOffset + 0x10] * randomMult + 1)) >> 32);

                    StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;

                    spriteEffect.ForceY = StaticVariables.g_hitSoundEffects[frameOffset + 0x26] * randomMult +
                        (int)(((ulong)StaticVariables.g_gameRandomSeed * (ulong)(StaticVariables.g_hitSoundEffects[frameOffset + 0x24] * randomMult + 1)) >> 32);

                    StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;

                    spriteEffect.ForceZ = StaticVariables.g_hitSoundEffects[effectEntityId + 0x32] * randomMult +
                        (int)(((ulong)StaticVariables.g_gameRandomSeed * (ulong)(StaticVariables.g_hitSoundEffects[effectEntityId + 0x30] * randomMult + 1)) >> 32);
                }
                break;
        }

        // Vérifier si le joueur est en mouvement
        if (StaticVariables.PlayerEntity.ForceX == 0 && StaticVariables.PlayerEntity.ForceY == 0)
        {
            goto SkipEffects;
        }

        // Traitement des effets pour le joueur en mouvement
        switch (StaticVariables.PlayerEntity.TargetAnimationId)
        {
            // Animations de marche/course - jouer des sons de pas
            case 1:
            case 7:
                if ((StaticVariables.PlayerEntity.FrameCounter & 0xf) == 0)
                {
                    _gameEngine.PlaySoundEffect((uint)StaticVariables.SHORT_ARRAY_800227f4[StaticVariables.PlayerEntity.Slope_18c]);
                }
                goto SkipEffects;

            // Animation de type "coup"
            case 3:
                effectEntityId = 0;
                if ((StaticVariables.PlayerEntity.FrameCounter & 7) == 0)
                {
                    _gameEngine.PlaySoundEffect((uint)StaticVariables.g_hitSoundEffects[StaticVariables.PlayerEntity.Slope_18c]);
                }
                goto CaseEffect;

            // Autres animations qui produisent des effets
            case 4:
            case 0x2a:
                effectEntityId = 1;
                CaseEffect:
                // Déterminer le type d'effet en fonction du type de terrain
                if (StaticVariables.PlayerEntity.Slope_18c - 1 < 2 || StaticVariables.PlayerEntity.Slope_18c == 4)
                {
                    animIndex = 0;
                    effectId = 6;
                }
                else
                {
                    animIndex = 1;
                    effectId = (byte)_gameEngine.CurrentMap.Info._10;
                }

                // Vérifier si on doit créer un effet pour cette frame
                if ((StaticVariables.g_hitSoundEffects[animIndex + 0x34] & StaticVariables.PlayerEntity.FrameCounter) != 0)
                {
                    goto SkipEffects;
                }

                // Créer un effet à la position du joueur
                if (effectId != 0)
                {
                    spriteEffect = _gameEngine.EffectManager.CreateEffectEntity(
                        0,
                        effectId,
                        0,
                        StaticVariables.PlayerEntity.PosX,
                        StaticVariables.PlayerEntity.PosY,
                        StaticVariables.PlayerEntity.TerrainHeight);
                }

                // Si l'effet a été créé, lui donner une force proportionnelle à celle du joueur
                if (spriteEffect != null)
                {
                    int forceMult = StaticVariables.g_hitSoundEffects[effectEntityId * 2 + animIndex + 0x36];
                    spriteEffect.ForceX = (StaticVariables.PlayerEntity.ForceX * forceMult) >> 8;
                    spriteEffect.ForceY = (StaticVariables.PlayerEntity.ForceY * forceMult) >> 8;

                    // Ajouter une composante aléatoire à la force verticale
                    StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    int zOffset = StaticVariables.g_hitSoundEffects[animIndex + 0x3c];
                    spriteEffect.ForceZ = zOffset + (int)(((ulong)StaticVariables.g_gameRandomSeed * (ulong)(StaticVariables.g_hitSoundEffects[animIndex + 0x3a] + 1)) >> 32);
                }
                break;

            // Animation de glissade ou de course rapide
            case 0x23:
                if ((StaticVariables.PlayerEntity.FrameCounter & 7) != 0)
                {
                    goto SkipEffects;
                }

                goto CaseRandomEffect;

            case 0x24:
                if ((StaticVariables.PlayerEntity.FrameCounter & 3) != 0)
                {
                    goto SkipEffects;
                }

                CaseRandomEffect:
                // Créer un effet de poussière/particule
                spriteEffect = _gameEngine.EffectManager.CreateEffectEntity(
                    0,
                    (byte)_gameEngine.CurrentMap.Info._10,
                    0,
                    StaticVariables.PlayerEntity.PosX,
                    StaticVariables.PlayerEntity.PosY,
                    StaticVariables.PlayerEntity.TerrainHeight);

                if (spriteEffect != null)
                {
                    // Générer des positions aléatoires autour du joueur
                    StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    uint randomSeed1 = StaticVariables.g_gameRandomSeed;

                    StaticVariables.g_gameRandomSeed = randomSeed1 * 0x7d2b89dd + 0xe06a02e7;
                    uint randomSeed2 = StaticVariables.g_gameRandomSeed;

                    StaticVariables.g_gameRandomSeed = randomSeed2 * 0x7d2b89dd + 0xe06a02e7;

                    // Appliquer les offsets aléatoires à la position de l'effet
                    spriteEffect.X += -0xc0000 + (int)(((ulong)randomSeed1 * 0x180001) >> 32);
                    spriteEffect.Y += -0x80000 + (int)(((ulong)randomSeed2 * 0x100001) >> 32);
                    spriteEffect.ForceZ += 0x10000 + (int)(((ulong)StaticVariables.g_gameRandomSeed * 0x10001) >> 32);
                }
                break;

            // Animation de glissade
            case 0x28:
                if ((StaticVariables.PlayerEntity.FrameCounter & 7) != 0)
                {
                    goto SkipEffects;
                }

                spriteEffect = _gameEngine.EffectManager.CreateEffectEntity(
                    0,
                    6,
                    0,
                    StaticVariables.PlayerEntity.PosX,
                    StaticVariables.PlayerEntity.PosY,
                    StaticVariables.PlayerEntity.TerrainHeight);

                if (spriteEffect == null)
                {
                    goto SkipEffects;
                }

                // Calculer les forces opposées au mouvement du joueur (effet de friction)
                int effectXForce = -StaticVariables.PlayerEntity.ForceX;
                if (StaticVariables.PlayerEntity.ForceX > 0)
                {
                    effectXForce += 3;
                }

                spriteEffect.ForceX = effectXForce >> 2;

                int effectYForce = -StaticVariables.PlayerEntity.ForceY;
                if (StaticVariables.PlayerEntity.ForceY > 0)
                {
                    effectYForce += 3;
                }

                spriteEffect.ForceY = effectYForce >> 2;
                break;
        }

        SkipEffects:
        // Traitement des animations d'atterrissage et de saut
        switch (StaticVariables.PlayerEntity.TargetAnimationId)
        {
            case 2:
            case 6:
            case 10:
            case 0xb:
            case 0x14:
            case 0x15:
            case 0x16:
            case 0x17:
            case 0x1f:
            case 0x26:
            case 0x2b:
            case 0x2c:
            case 0x2d:
            case 0x2e:
            case 0x2f:
            case 0x30:
            case 0x41:
            case 0x42:
            case 0x46:
            case 0x4b:
                // Si on est en mode normal (param_1 == 0) et que le joueur est au sol
                if ((param_1 == 0 || StaticVariables.DAT_80098f30 == 0) &&
                    StaticVariables.PlayerEntity.IsAboveGround != 0 &&
                    StaticVariables.PlayerEntity.ForceZ < 1)
                {
                    // Jouer un son d'atterrissage
                    _gameEngine.PlaySoundEffect((uint)StaticVariables.SHORT_ARRAY_80022804[StaticVariables.PlayerEntity.Slope_18c]);

                    // Créer des effets visuels d'atterrissage en fonction du type de terrain
                    if (StaticVariables.PlayerEntity.Slope_18c < 1 || (2 < StaticVariables.PlayerEntity.Slope_18c && StaticVariables.PlayerEntity.Slope_18c != 4))
                    {
                        // Créer plusieurs particules pour les terrains normaux
                        for (int i = 0; i < 3; i++)
                        {
                            if (_gameEngine.CurrentMap.Info._10 != 0)
                            {
                                entityCreated = _gameEngine.EffectManager.CreateEffectEntity(
                                    0,
                                    (byte)_gameEngine.CurrentMap.Info._10,
                                    0,
                                    StaticVariables.PlayerEntity.PosX,
                                    StaticVariables.PlayerEntity.PosY,
                                    StaticVariables.PlayerEntity.TerrainHeight);

                                if (entityCreated != null)
                                {
                                    // Générer des mouvements aléatoires
                                    StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                                    entityCreated.ForceX = (int)(((ulong)StaticVariables.g_gameRandomSeed * 0x30001) >> 32) - 0x18000;

                                    StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                                    entityCreated.ForceY = (int)(((ulong)StaticVariables.g_gameRandomSeed * 0x20001) >> 32) - 0x10000;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Créer un effet unique pour les pentes
                        _gameEngine.EffectManager.CreateEffectEntity(
                            0,
                            6,
                            0,
                            StaticVariables.PlayerEntity.PosX,
                            StaticVariables.PlayerEntity.PosY,
                            StaticVariables.PlayerEntity.TerrainHeight);
                    }

                    StaticVariables.DAT_80098f30 = 1;
                }
                break;

            default:
                StaticVariables.DAT_80098f30 = 0;
                break;
        }

        // En mode normal (pas en mode préparation pour l'entrée de warp)
        if (param_1 == 0)
        {
            // Gestion du son de battement de coeur quand les HP sont bas
            if (StaticVariables.PlayerEntity.Hp != 0 &&
                StaticVariables.PlayerEntity.Hp * 5 <= StaticVariables.PlayerEntity.HpMax)
            {
                StaticVariables.DAT_80098f2c--;

                if (StaticVariables.DAT_80098f2c == -1)
                {
                    _gameEngine.PlaySoundEffect(400); // Son de battement de cœur
                    StaticVariables.DAT_80098f2c = 0x2d; // Réinitialiser le compteur
                }
            }

            // Déterminer le type d'attaque en fonction de l'animation
            int attackType = -1;

            if (StaticVariables.PlayerEntity.TargetAnimationId == 0x16)
            {
                attackType = 0;
            }
            else if (StaticVariables.PlayerEntity.TargetAnimationId == 0x12)
            {
                attackType = 0;
            }
            else if (StaticVariables.PlayerEntity.TargetAnimationId == 0x40)
            {
                attackType = 1;
            }
            else if (StaticVariables.PlayerEntity.TargetAnimationId == 0x42)
            {
                attackType = 1;
            }
            else
            {
                return; // Sortir si ce n'est pas une animation d'attaque
            }

            // Ne faire l'attaque que si le compteur de coup est à 0
            if (StaticVariables.PlayerEntity.HitCounter == 0)
            {
                // Vérifier les collisions avec les tuiles
                if (StaticVariables.PlayerEntity.FrameCollision != null)
                {
                    int[] worldXCoords = new int[4];
                    int[] worldYCoords = new int[4];

                    // Convertir les coordonnées de la boîte de collision en coordonnées de tuile
                    worldXCoords[0] = StaticVariables.g_tileToWorldXTable[StaticVariables.PlayerEntity.HitBoxX >> 16];
                    worldXCoords[2] = worldXCoords[0];
                    worldXCoords[1] = StaticVariables.g_tileToWorldXTable[(StaticVariables.PlayerEntity.HitBoxX + StaticVariables.PlayerEntity.FrameWidth) >> 16];
                    worldXCoords[3] = worldXCoords[1];

                    worldYCoords[0] = StaticVariables.PlayerEntity.HitBoxY >> 20;
                    worldYCoords[1] = worldYCoords[0];
                    worldYCoords[2] = (StaticVariables.PlayerEntity.HitBoxY + StaticVariables.PlayerEntity.FrameDepth) >> 20;
                    worldYCoords[3] = worldYCoords[2];

                    // Parcourir les coins de la boîte de collision
                    for (int i = 0; i < 4; i++)
                    {
                        // Limiter les coordonnées aux bornes de la carte
                        int tileX = worldXCoords[i];
                        if (tileX < 1)
                        {
                            tileX = 0;
                        }
                        else if (tileX > 0x33)
                        {
                            tileX = 0x33;
                        }

                        int tileY = worldYCoords[i];
                        if (tileY < 1)
                        {
                            tileY = 0;
                        }
                        else if (tileY > 0x3B)
                        {
                            tileY = 0x3B;
                        }

                        // Calculer l'index de la tuile
                        int tileIndex = tileY * 0xd0 + tileX * 4 + 0x302;

                        // Vérifier si la tuile a un attribut d'effet (bit 1)
                        if ((StaticVariables.g_spriteVRAMPointer[tileIndex] & 2) != 0)
                        {
                            // Vérifier si la hauteur de la boîte de collision croise la hauteur de l'effet
                            int tileEffectZ = (StaticVariables.g_spriteVRAMPointer[tileIndex + 3] & 0xFF) << 20;

                            if (StaticVariables.PlayerEntity.HitBoxZ <= tileEffectZ + 0x80000 &&
                                tileEffectZ + 0x80000 <= StaticVariables.PlayerEntity.HitBoxZ + StaticVariables.PlayerEntity.FrameHeight)
                            {
                                // Désactiver l'effet pour éviter de le déclencher plusieurs fois
                                StaticVariables.g_spriteVRAMPointer[tileIndex] &= 0xFFFD;
                                StaticVariables.g_spriteVRAMPointer[tileIndex + 3] = 0xFFFF;

                                // Calculer la position de l'effet
                                int effectX = worldXCoords[i] * 0x180000 + 0xC0000;
                                int effectY = worldYCoords[i] * 0x100000 + 0x80000;

                                // Créer l'effet visuel
                                _gameEngine.EffectManager.CreateEffectEntity(
                                    0,
                                    _gameEngine.CurrentMap.Info.F,
                                    0,
                                    effectX,
                                    effectY,
                                    tileEffectZ);

                                // Déclencher l'effet de warp si nécessaire
                                _gameEngine.EffectManager.CreateWarpEffect(0xFF, effectX, effectY, tileEffectZ);

                                // Jouer un son d'effet
                                _gameEngine.PlaySoundEffect(0x1F);
                            }
                        }
                    }
                }
            }
            else
            {
                // Si on est en train de prendre un coup, ajuster l'animation en fonction de si on est au sol
                if (StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    StaticVariables.PlayerEntity.TargetAnimationId = StaticVariables.BYTE_ARRAY_800228a4[attackType * 2 + 1];
                }
                else
                {
                    StaticVariables.PlayerEntity.TargetAnimationId = StaticVariables.BYTE_ARRAY_800228a4[attackType * 2];
                }
            }
        }
    }

    // 800350c0
    public void AnimateWarpEffect()
    {
        if (StaticVariables.g_warpLockTimer != 0)
        {
            switch (StaticVariables.g_warpLockTimer)
            {
                case 0x1f:
                    FUN_80034acc();
                    break;
                case 0x23:
                    FUN_80034b54();
                    break;
                case 0x2c:
                    FUN_80034bdc();
                    break;
                case 0x2d:
                case 0x2e:
                    FUN_80034c54();
                    break;
                case 0x2f:
                    FUN_80034d2c();
                    break;
                case 0x30:
                    FUN_80034e08();
                    break;
                case 0x32:
                    FUN_80034ec4();
                    break;
            }

            StaticVariables.g_warpLockTimer = 0;
        }
    }

    private void FUN_80034ec4()
    {
        Debugger.Break();
    }

    private void FUN_80034e08()
    {
        Debugger.Break();
    }

    private void FUN_80034d2c()
    {
        Debugger.Break();
    }

    private void FUN_80034c54()
    {
        Debugger.Break();
    }

    private void FUN_80034bdc()
    {
        Debugger.Break();
    }

    private void FUN_80034b54()
    {
        Debugger.Break();
    }

    private void FUN_80034acc()
    {
        Debugger.Break();
    }
}