using AlundraEngine.Balance;
using AlundraEngine.Gameplay;
using AlundraEngine.Gameplay.Scripts;
using System;
using System.Diagnostics;
using AlundraEngine.DatasBin;
using static OfficeOpenXml.ExcelErrorValue;

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
        int weaponId;
        int slope;
        uint dir;

        _gameEngine.StaticVariables.g_activeCollisionEntity = null;
        weaponId = (int)_gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();
        //weapon slot : 0 6 8 5 0 10
        //si sword niveau 1 alors 1
        _gameEngine.StaticVariables.g_currentWeaponFlags = _gameEngine.StaticVariables.g_weaponFlagsByItemId[weaponId];
        CheckAndExecuteWarp();
        slope = _gameEngine.StaticVariables.PlayerEntity.Slope_18c;

        if (_gameEngine.StaticVariables.PlayerEntity.IsNotProcessable != 0)
        {
            _gameEngine.StaticVariables.g_playerWarpTimer = 0;
            Array.Clear(_gameEngine.StaticVariables.g_playerEffectTransitionCooldown);
            goto END;
        }

        if ((_gameEngine.StaticVariables.g_playerControlFlags & 0x34U) != 0)
        {
            UpdatePlayerAnimationEffects(1);
            _gameEngine.StaticVariables.g_playerWarpTimer = 0;
            Array.Clear(_gameEngine.StaticVariables.g_playerEffectTransitionCooldown);
            UpdatePlayerCarriedEntity(1);
            AnimateWarpEffect();

            if (_gameEngine.StaticVariables.g_playerControlFlags == 0x20)
            {
                dir = FindWarpFacingDirection();

                if (dir != 0xffffffff)
                {
                    _gameEngine.StaticVariables.PlayerEntity.DamagedTickCounter = 0x78;
                }
            }

            goto END;
        }

        _gameEngine.StaticVariables.PlayerEntity.Flags |= 0x100;
        UpdatePlayerAnimationEffects(0);
        dir = FindWarpFacingDirection();

        if (dir != 0xffffffff)
        {
            _gameEngine.StaticVariables.g_playerWarpTimer = 0;
            Array.Clear(_gameEngine.StaticVariables.g_playerEffectTransitionCooldown);
            UpdatePlayerCarriedEntity(2);
            MaybeStartWarpAnimation();
            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.DamageTaken;

            if (slope == 4)
            {
                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.DamageTakenSwimming;
            }

            _gameEngine.StaticVariables.PlayerEntity.DamagedTickCounter = 0x78;
            _gameEngine.StaticVariables.PlayerEntity.TargetDirection = dir;
            goto END;
        }

        //death
        if (_gameEngine.StaticVariables.PlayerEntity.Hp == 0)
        {
            _gameEngine.StaticVariables.g_playerWarpTimer = 0;
            Array.Clear(_gameEngine.StaticVariables.g_playerEffectTransitionCooldown);
            UpdatePlayerCarriedEntity(2);
            AnimateWarpEffect();

            switch (_gameEngine.StaticVariables.PlayerEntity.TargetAnimationId)
            {
                case (int)PlayerAnimation.DamageKnockBack:
                    if (slope == 4)
                    {
                        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Reserved3C;
                    }
                    else if (_gameEngine.StaticVariables.PlayerEntity.ForceResetAnimationFlag != 0)
                    {
                        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Reserved39;
                    }

                    break;

                case (int)PlayerAnimation.DamageTaken:
                case (int)PlayerAnimation.Reserved39:
                    if (slope == 4)
                    {
                        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Reserved3C;
                    }

                    break;

                case (int)PlayerAnimation.DamageTakenSwimming:
                case (int)PlayerAnimation.Reserved3C:
                    if (slope != 4)
                    {
                        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Reserved39;
                    }

                    break;

                case (int)PlayerAnimation.DamageKnockBackSwimming:
                    if (slope == 4 && _gameEngine.StaticVariables.PlayerEntity.ForceResetAnimationFlag != 0)
                    {
                        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Reserved3C;
                    }
                    else
                    {
                        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Reserved39;
                    }

                    break;

                case (int)PlayerAnimation.Dead:
                case (int)PlayerAnimation.Reserved4F:
                    if (_gameEngine.StaticVariables.PlayerEntity.ForceResetAnimationFlag != 0)
                    {
                        weaponId = _gameEngine.PlayerManager.GetNumberOfItem(0x27);
                        if (weaponId == 0)
                        {
                            _gameEngine.StaticVariables.g_isGameEnding = 1;
                            _gameEngine.StaticVariables.g_mapTransitionEffectId = 8;
                            _gameEngine.StaticVariables.g_warpEntryBehavior = 0;
                            _gameEngine.StaticVariables.g_desiredMap = 0x1dd;
                            _gameEngine.StaticVariables.g_warpTriggerType = 0;
                            _gameEngine.StaticVariables.g_playerControlFlags |= 4;
                            break;
                        }

                        PlayCutscene(0x27);

                        if (slope == 4)
                        {
                            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.SwimmingStill;
                        }
                        else
                        {
                            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Idle;
                        }
                    }
                    break;

                default:
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = slope == 4 ?
                        (uint)PlayerAnimation.Reserved3C : (uint)PlayerAnimation.Reserved39;

                    break;
            }

            goto END;
        }

        if ((_gameEngine.StaticVariables.PlayerEntity.TileAttributes & 0x80U) != 0)
        {
            _gameEngine.StaticVariables.g_playerWarpTimer = 0;
            Array.Clear(_gameEngine.StaticVariables.g_playerEffectTransitionCooldown);
            AnimateWarpEffect();
            UpdatePlayerCarriedEntity(1);

            if (slope == 4)
            {
                LAB_80032830:
                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.SwimmingStill;
                goto END;
            }

            if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround == 0)
            {
                LAB_8003279c:
                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Jump;
                goto END;
            }

            LAB_80031e7c:
            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Idle;
            goto END;
        }

        MaybeStartWarpAnimation();
        var buttonsHold = (uint)(_gameEngine.StaticVariables.g_padState1.ButtonsHold >> 0xc);
        dir = _gameEngine.StaticVariables.g_directionByButtons[buttonsHold];

        if (_gameEngine.StaticVariables.g_directionByButtons[buttonsHold] == 0xffffffff)
        {
            dir = _gameEngine.StaticVariables.PlayerEntity.TargetDirection;
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
                _gameEngine.StaticVariables.g_playerWarpTimer = 0;
                Array.Clear(_gameEngine.StaticVariables.g_playerEffectTransitionCooldown);
                UpdatePlayerCarriedEntity(2);
                switch (_gameEngine.StaticVariables.PlayerEntity.TargetAnimationId)
                {
                    default:
                        AnimateWarpEffect();
                        goto END;
                    case (int)PlayerAnimation.SwimmingSlow:
                    case (int)PlayerAnimation.SwimmingStill:
                        _gameEngine.StaticVariables.PlayerEntity.TargetDirection = dir;

                        if (TryUseItem() == 0)
                        {
                            goto END;
                        }

                        slope = CheckEntityInteraction();

                        if (slope != 0)
                        {
                            if (slope == 2)
                            {
                                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.SwimmingStill;
                            }
                            goto END;
                        }

                        if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & 0xd0) != 0)
                        {
                            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.SwimmingDash;
                            goto END;
                        }

                        if (buttonsHold != 0)
                        {
                            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.SwimmingSlow;
                            goto END;
                        }
                        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.SwimmingStill;
                        goto END;
                    case 0x1c:
                    case 0x31:
                    case 0x3e:
                        _gameEngine.StaticVariables.PlayerEntity.TargetDirection = _gameEngine.StaticVariables.PlayerEntity.TargetDirection + 0x10 & 0x1f;
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
                        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.SwimmingStill;
                        break;
                    case 0x28:
                    case 0x3a:
                        break;
                    case 0x3b:
                        slope = TryUseItem();
                        if (slope != 0)
                        {
                            if (_gameEngine.StaticVariables.PlayerEntity.ForceResetAnimationFlag == 1)
                            {
                                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.SwimmingStill;
                                _gameEngine.StaticVariables.PlayerEntity.TargetDirection = _gameEngine.StaticVariables.PlayerEntity.TargetDirection + 0x10 & 0x1f;
                            }
                            goto END;
                        }
                        _gameEngine.StaticVariables.PlayerEntity.TargetDirection = _gameEngine.StaticVariables.PlayerEntity.TargetDirection + 0x10 & 0x1f;
                        goto END;
                }
                TryUseItem();
                goto END;
            case 6:
                if (buttonsHold != 0
                    && dir == 0x10
                    && _gameEngine.StaticVariables.PlayerEntity.TargetDirection == 0x10
                    && _gameEngine.StaticVariables.PlayerEntity.ForceAdjusted != 0
                    && _gameEngine.StaticVariables.PlayerEntity.CarriedEntity == null)
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Climbing;
                }
                break;
            default:
                goto END;
        }

        UpdatePlayerWeaponEffect();
        UpdateWeaponStepProgression();
        UpdatePlayerCarriedEntity(0);

        int iVar2;

        switch (_gameEngine.StaticVariables.PlayerEntity.TargetAnimationId)
        {
            case (int)PlayerAnimation.Idle:
            case (int)PlayerAnimation.Moving:
                _gameEngine.StaticVariables.PlayerEntity.TargetDirection = dir;
                if (TryUseItem() == 0 || PlayerTryAction() != 0)
                {
                    break;
                }

                iVar2 = CheckEntityInteraction();
                if (iVar2 != 0)
                {
                    if (iVar2 != 2)
                    {
                        break;
                    }

                    //goto LAB_80031e7c;
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Idle;
                    goto END;
                }
                goto LAB_80031ea8;

            case (int)PlayerAnimation.StartJumpWhileMoving:
            case (int)PlayerAnimation.StartJump:
            case (int)PlayerAnimation.JumpMoving:
            case (int)PlayerAnimation.Jump:
                _gameEngine.StaticVariables.PlayerEntity.TargetDirection = dir;

                if (TryUseItem() == 0 || PlayerTryAction() != 0)
                {
                    break;
                }

                LAB_80031ea8:
                if (PlayerTryAttack() != 0)
                {
                    break;
                }

                if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    if (buttonsHold != 0)
                    {
                        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.JumpMoving;
                        break;
                    }

                    goto LAB_80032604;
                }

                if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & PadState.Cross) != 0)
                {
                    if ((_gameEngine.StaticVariables.PlayerEntity.CombinedVramFlagsOR & 0x4000U) == 0)
                    {
                        if (buttonsHold == 0)
                        {
                            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.StartJump;
                        }
                        else
                        {
                            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.StartJumpWhileMoving;
                        }
                    }

                    break;
                }

                if ((_gameEngine.StaticVariables.g_padState1.ButtonsHold & PadState.Triangle) != 0)
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.PrepareSprint;
                    _gameEngine.StaticVariables.PlayerEntity.AnimCompleteCounter = 0;
                    break;
                }

                if (buttonsHold != 0)
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Moving;
                    goto END;
                }
                //goto LAB_800325e0;

                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Idle;
                break;

            case (int)PlayerAnimation.Sprint:
                if (PlayerTryAction() != 0)
                {
                    break;
                }

                if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround != 0)
                {
                    if (_gameEngine.StaticVariables.PlayerEntity.ForceAdjusted == 0)
                    {
                        var dirIndex = _gameEngine.StaticVariables.PlayerEntity.CurrentDirection >> 3;
                        dirIndex = dirIndex switch
                        {
                            1 => 2,
                            2 => 1,
                            _ => dirIndex
                        } * 3;

                        if (buttonsHold == 0)
                        {
                            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.StopSprint;
                            _gameEngine.StaticVariables.INT_ARRAY_80126fe8[3] = 1;
                        }
                        else if ((_gameEngine.StaticVariables.g_padState1.ButtonsHold & PadState.Triangle) == 0 ||
                                (dir != _gameEngine.StaticVariables.UINT_ARRAY_80022cec[dirIndex] &&
                                 dir != _gameEngine.StaticVariables.UINT_ARRAY_80022cec[dirIndex + 1] &&
                                 dir != _gameEngine.StaticVariables.UINT_ARRAY_80022cec[dirIndex + 2]))
                        {
                            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.StopSprint;
                            _gameEngine.StaticVariables.INT_ARRAY_80126fe8[3] = 0;
                        }
                        else if ((_gameEngine.StaticVariables.g_padState1.ButtonsHold & 0xe0) != 0)
                        {
                            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.SprintDash;
                        }
                    }
                    else
                    {
                        _gameEngine.EffectManager.CreateEffectEntity(
                            0, 9, 0,
                            _gameEngine.StaticVariables.PlayerEntity.PosX,
                            _gameEngine.StaticVariables.PlayerEntity.PosY,
                            _gameEngine.StaticVariables.PlayerEntity.PosZ + 0x100000);
                        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.SprintAgainstWall;
                        _gameEngine.StaticVariables.PlayerEntity.TargetDirection = _gameEngine.StaticVariables.PlayerEntity.TargetDirection + 0x10 & 0x1f;
                    }

                    break;
                }
                goto LAB_8003279c;

            case (int)PlayerAnimation.SprintDash:
            case (int)PlayerAnimation.FlailHitIron:
            case (int)PlayerAnimation.ThrowObject:
            case (int)PlayerAnimation.AttackSwordDaggerLegend:
            case (int)PlayerAnimation.AttackWandIceChargedFireCharged:
            case (int)PlayerAnimation.AttackFlailIron:
            case (int)PlayerAnimation.AttackBowHunterWillowCharged:
            case (int)PlayerAnimation.ChargeAttackSword:
            case (int)PlayerAnimation.ChargeAttackFlailSteel:
            case (int)PlayerAnimation.FlailHitWallSteel:
            case (int)PlayerAnimation.AttackSword:
            case (int)PlayerAnimation.AttackFlailSteel:
            case (int)PlayerAnimation.AttackSwordFiendBlade:
            case (int)PlayerAnimation.AttackSwordHoly:
                if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Jump;
                }

                //goto switchD_80032650_caseD_28;
                TryUseItem();
                goto END;

            case (int)PlayerAnimation.PickupObject:
                if (TryUseItem() == 0)
                {
                    break;
                }

                if (_gameEngine.StaticVariables.PlayerEntity.CarriedEntity != null)
                {
                    if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround == 0)
                    {
                        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.JumpWithObject;
                    }
                    break;
                }
                goto LAB_80032604;

            case (int)PlayerAnimation.StartJumpWithObjectWhileMoving:
            case (int)PlayerAnimation.MovingWithObject:
            case (int)PlayerAnimation.HoldObject:
            case (int)PlayerAnimation.StartJumpWithObject:
            case (int)PlayerAnimation.JumpMovingWithObject:
            case (int)PlayerAnimation.JumpWithObject:
                _gameEngine.StaticVariables.PlayerEntity.TargetDirection = dir;
                var carriedEntity = _gameEngine.StaticVariables.PlayerEntity.CarriedEntity;

                if (TryUseItem() == 0)
                {
                    break;
                }

                if (_gameEngine.StaticVariables.PlayerEntity.CarriedEntity != null)
                {
                    if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & PadState.Square) == 0)
                    {
                        if ((_gameEngine.StaticVariables.PlayerEntity.CarriedEntity.Flags & 0x600U) == 0x600)
                        {
                            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.JumpWithObject;

                            if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround != 0)
                            {
                                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.HoldObject;
                            }
                        }
                        else if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround == 0)
                        {
                            if (buttonsHold == 0)
                            {
                                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.JumpWithObject;
                            }
                            else
                            {
                                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.JumpMovingWithObject;
                            }
                        }
                        else if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & PadState.Cross) == 0)
                        {
                            if (buttonsHold == 0)
                            {
                                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.HoldObject;
                            }
                            else
                            {
                                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.MovingWithObject;
                            }
                        }
                        else if ((_gameEngine.StaticVariables.PlayerEntity.CombinedVramFlagsOR & 0x4000U) == 0)
                        {
                            if (buttonsHold == 0)
                            {
                                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.StartJumpWithObject;
                            }
                            else
                            {
                                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.StartJumpWithObjectWhileMoving;
                            }
                        }
                    }
                    else if ((_gameEngine.StaticVariables.PlayerEntity.CombinedVramFlagsOR & 0x2000U) == 0)
                    {
                        var direction = _gameEngine.StaticVariables.PlayerEntity.CurrentDirection; //(uint)_gameEngine.StaticVariables.g_cardinalDirectionTable[_gameEngine.StaticVariables.PlayerEntity.CurrentDirection >> 3];
                        _gameEngine.StaticVariables.PlayerEntity.CarriedEntity.TargetDirection = direction;
                        carriedEntity.PosX = _gameEngine.StaticVariables.PlayerEntity.PosX;
                        carriedEntity.PosY = _gameEngine.StaticVariables.PlayerEntity.PosY;
                        carriedEntity.PosZ = _gameEngine.StaticVariables.PlayerEntity.PosZ + 0x200000;

                        if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround == 0)
                        {
                            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.ThrowObjectWhileJumping;

                            if (buttonsHold != 0)
                            {
                                carriedEntity.Flags2 = 3;
                                break;
                            }
                        }
                        else
                        {
                            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.ThrowObject;

                            if (buttonsHold == 0)
                            {
                                carriedEntity.Flags2 = 1;
                                break;
                            }
                        }

                        carriedEntity.Flags2 = 2;
                    }
                    break;
                }

                if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround != 0)
                {
                    //goto LAB_80031e7c;
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Idle;
                    goto END;
                }

                LAB_8003279c:
                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Jump;
                break;

            case (int)PlayerAnimation.ThrowObjectWhileJumping:
            case (int)PlayerAnimation.JumpAttackSwordDagger:
            case (int)PlayerAnimation.JumpAttackWandIceFire:
            case (int)PlayerAnimation.JumpAttackFlailIron:
            case (int)PlayerAnimation.JumpAttackBowHunterWillow:
            case (int)PlayerAnimation.Reserved1F:
            case (int)PlayerAnimation.Reserved26:
            case (int)PlayerAnimation.JumpAttackSword:
            case (int)PlayerAnimation.JumpAttackFlailSteel:
            case (int)PlayerAnimation.JumpAttackSwordFiendBlade:
            case (int)PlayerAnimation.JumpAttackSwordHoly:
                if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround != 0)
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Idle;
                }
                //goto switchD_80032650_caseD_28;
                TryUseItem();
                goto END;

            case (int)PlayerAnimation.Reserved0B:
                if (TryUseItem() == 0)
                {
                    break;
                }

                if (_gameEngine.StaticVariables.PlayerEntity.CarriedEntity != null)
                {
                    if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround != 0)
                    {
                        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.HoldObject;
                    }
                    break;
                }
                goto LAB_800325e0;

            case (int)PlayerAnimation.Climbing:
            case (int)PlayerAnimation.ClimbStill:
                if (TryUseItem() == 0 || PlayerTryAction() != 0)
                {
                    break;
                }

                if (buttonsHold != 0 && dir != 0 && dir != 0x10)
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Jump;
                    break;
                }

                _gameEngine.StaticVariables.PlayerEntity.TargetDirection = 0x10;

                if (buttonsHold == 0)
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.ClimbStill;
                    _gameEngine.StaticVariables.PlayerEntity.ForceZ = 0;
                    LAB_800322a8:
                    _gameEngine.StaticVariables.PlayerEntity.Flags &= 0xfffffeff;
                }
                else
                {
                    if (dir == 0)
                    {
                        if (_gameEngine.StaticVariables.PlayerEntity.FloorHeight + 1 < _gameEngine.StaticVariables.PlayerEntity.PosZ)
                        {
                            _gameEngine.StaticVariables.PlayerEntity.ForceZ = -0x10000;
                            LAB_8003229c:
                            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Climbing;
                            //goto LAB_800322a8;             
                            _gameEngine.StaticVariables.PlayerEntity.Flags &= 0xfffffeff;
                            goto END;
                        }
                    }
                    else
                    {
                        if (dir != 0x10)
                        {
                            break;
                        }

                        iVar2 = _gameEngine.EntityGameplayManager.GetTileHeightAtOffset(_gameEngine.StaticVariables.PlayerEntity, 0, -0x10000);
                        if (_gameEngine.StaticVariables.PlayerEntity.PosZ <= iVar2)
                        {
                            _gameEngine.StaticVariables.PlayerEntity.ForceZ = 0x10000;
                            //goto LAB_8003229c;
                            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Climbing;
                            _gameEngine.StaticVariables.PlayerEntity.Flags &= 0xfffffeff;
                            goto END;
                        }
                    }
                    LAB_80031e7c:
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Idle;
                }
                break;

            case (int)PlayerAnimation.SwimmingSlow:
            case (int)PlayerAnimation.SwimmingStill:
            case (int)PlayerAnimation.SwimmingDash:
            case (int)PlayerAnimation.Reserved39:
            case (int)PlayerAnimation.Reserved3C:
            case (int)PlayerAnimation.Dead:
            case (int)PlayerAnimation.Reserved4F:
                //goto switchD_80031dac_caseD_f;
                if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Jump;
                }
                else
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Idle;
                }
                TryUseItem();
                goto END;

            case (int)PlayerAnimation.DamageKnockBack:
            case (int)PlayerAnimation.SprintAgainstWall:
                if (TryUseItem() == 0)
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetDirection = _gameEngine.StaticVariables.PlayerEntity.TargetDirection + 0x10 & 0x1f;
                    break;
                }

                if (_gameEngine.StaticVariables.PlayerEntity.ForceResetAnimationFlag != 1)
                {
                    break;
                }

                _gameEngine.StaticVariables.PlayerEntity.TargetDirection = _gameEngine.StaticVariables.PlayerEntity.TargetDirection + 0x10 & 0x1f;

                if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Jump;
                    //(uint)_gameEngine.StaticVariables.PlayerEntity.ForceResetAnimationFlag;
                    break;
                }
                goto LAB_80032594;

            case (int)PlayerAnimation.EnterSand:
            case (int)PlayerAnimation.InSandDash:
                if (TryUseItem() == 0 || _gameEngine.StaticVariables.g_warpLockTimer == 0x20)
                {
                    break;
                }

                goto LAB_8003253c;

            case (int)PlayerAnimation.ExitSand:
            case (int)PlayerAnimation.DamageTaken:
            case (int)PlayerAnimation.EndSpellCast:
                //goto switchD_80032650_caseD_28;
                TryUseItem();
                goto END;

            case (int)PlayerAnimation.InSand:
            case (int)PlayerAnimation.InSandMoving:
                _gameEngine.StaticVariables.PlayerEntity.TargetDirection = dir;

                if (TryUseItem() == 0)
                {
                    break;
                }

                if (_gameEngine.StaticVariables.g_warpLockTimer == 0x20)
                {
                    if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & 0xd0) == 0)
                    {
                        if (buttonsHold == 0)
                        {
                            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.InSand;
                        }
                        else
                        {
                            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.InSandMoving;
                        }
                    }
                    else
                    {
                        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.InSandDash;
                    }
                    break;
                }

                LAB_8003253c:
                if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Jump;
                    break;
                }

                goto LAB_800325e0;

            case (int)PlayerAnimation.PrepareSprint:
                if (TryUseItem() == 0
                    || PlayerTryAction() != 0
                    || PlayerTryAttack() != 0)
                {
                    break;
                }

                if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Jump;
                    _gameEngine.StaticVariables.PlayerEntity.TargetDirection = dir;
                    break;
                }

                _gameEngine.StaticVariables.PlayerEntity.TargetDirection = dir;

                if ((_gameEngine.StaticVariables.g_padState1.ButtonsHold & PadState.Triangle) != 0)
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetDirection = dir;

                    if (_gameEngine.StaticVariables.PlayerEntity.AnimCompleteCounter != 0 &&
                       _gameEngine.StaticVariables.g_dashDirections[buttonsHold] != 0xffffffff)
                    {
                        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Sprint;
                        _gameEngine.StaticVariables.PlayerEntity.TargetDirection = _gameEngine.StaticVariables.g_dashDirections[buttonsHold];
                    }
                    break;
                }

                LAB_80032594:
                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Idle;
                break;

            case (int)PlayerAnimation.StopSprint:
                if (TryUseItem() != 0 && PlayerTryAction() == 0)
                {
                    if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround == 0)
                    {
                        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Jump;
                    }
                    else if (_gameEngine.StaticVariables.INT_ARRAY_80126fe8[3] != 0)
                    {
                        if (_gameEngine.StaticVariables.PlayerEntity.ForceAdjusted == 0 &&
                           (_gameEngine.StaticVariables.g_padState1.ButtonsHold & PadState.Triangle) != 0)
                        {
                            _gameEngine.StaticVariables.INT_ARRAY_80126fe8[3] += 1;

                            if (10 < _gameEngine.StaticVariables.INT_ARRAY_80126fe8[3])
                            {
                                if (_gameEngine.StaticVariables.g_dashDirections[buttonsHold] != 0xffffffff)
                                {
                                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Sprint;
                                    _gameEngine.StaticVariables.PlayerEntity.TargetDirection = _gameEngine.StaticVariables.g_dashDirections[buttonsHold];
                                }
                                break;
                            }

                            if (buttonsHold == 0)
                            {
                                break;
                            }
                        }

                        _gameEngine.StaticVariables.INT_ARRAY_80126fe8[3] = 0;
                    }
                }
                break;

            case (int)PlayerAnimation.StartSpellCast:
            case (int)PlayerAnimation.LoopSpellCast:
                if (TryUseItem() == 0 || _gameEngine.StaticVariables.g_warpLockTimer - 0x2bU < 8)
                {
                    break;
                }

                if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    goto LAB_80032604;
                }

                LAB_800325e0:
                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Idle;
                break;

            case (int)PlayerAnimation.LoadingMap:
                if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround != 0)
                {
                    break;
                }

                LAB_80032604:
                _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Jump;
                break;

            case (int)PlayerAnimation.DamageTakenSwimming:
            case (int)PlayerAnimation.DamageKnockBackSwimming:
                _gameEngine.StaticVariables.PlayerEntity.TargetDirection = _gameEngine.StaticVariables.PlayerEntity.TargetDirection + 0x10 & 0x1f;
                //goto switchD_80031dac_caseD_f;
                if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Jump;
                }
                else
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (int)PlayerAnimation.Idle;
                }

                TryUseItem();
                goto END;

            default:
                AnimateWarpEffect();
                break;
        }

        //_gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = 0;

        END:
        UpdateItemEffectState();
        _gameEngine.PlayerManager.SetPlayerHpMax(_gameEngine.StaticVariables.PlayerEntity.HpMax);
        _gameEngine.PlayerManager.SetPlayerHp((short)_gameEngine.StaticVariables.PlayerEntity.Hp);
    }

    //8002eaf4
    private int PlayerTryAttack()
    {
        byte animId;
        var weaponFlagsIndex = _gameEngine.StaticVariables.g_currentWeaponFlags * 13;

        if ((_gameEngine.StaticVariables.PlayerEntity.CombinedVramFlagsOR & 0x2000U) != 0)
        {
            return 0;
        }

        if ((_gameEngine.StaticVariables.g_padState1.ButtonsReleased & 0x80) == 0)
        {
            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & 0x80) == 0)
            {
                return 0;
            }

            if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround == 0)
            {
                weaponFlagsIndex += 5;
                animId = _gameEngine.StaticVariables.g_weaponInitFlags[weaponFlagsIndex];
            }
            else
            {
                weaponFlagsIndex += 1;
                animId = _gameEngine.StaticVariables.g_weaponInitFlags[weaponFlagsIndex];
            }

            if (animId == 0)
            {
                _gameEngine.SoundManager.PlaySoundEffect(3);
                return 0;
            }
        }
        else
        {
            if (_gameEngine.StaticVariables.g_playerWarpTimer < 0x3c)
            {
                return 0;
            }

            if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround == 0)
            {
                return 0;
            }

            var i = 0;

            do
            {
                var effect = _gameEngine.EffectManager.CreateEffectEntity(
                    0, 0x10, 0,
                    _gameEngine.StaticVariables.PlayerEntity.PosX,
                    _gameEngine.StaticVariables.PlayerEntity.PosY,
                    _gameEngine.StaticVariables.PlayerEntity.PosZ + 0x10 //0x100000
                );

                if (effect != null)
                {
                    effect.ForceX = _gameEngine.StaticVariables.g_offsetXList[i * 2] * 0x1c0; //448
                    effect.ForceY = _gameEngine.StaticVariables.g_offsetYList[i * 2] * 0x1c0; //448
                }

                i += 1;

            } while (i < 0x10);

            _gameEngine.SoundManager.PlaySoundEffect(0x2b);
            animId = _gameEngine.StaticVariables.g_weaponInitFlags[_gameEngine.StaticVariables.g_currentWeaponFlags * 0xd + 9];
            weaponFlagsIndex += 9;
        }

        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = animId;
        _gameEngine.StaticVariables.g_playerEffectStepFlags = 0;
        _gameEngine.StaticVariables.g_playerEffectTransitionCooldown[0] = _gameEngine.StaticVariables.g_weaponInitFlags[weaponFlagsIndex + 0];
        _gameEngine.StaticVariables.g_playerEffectTransitionCooldown[1] = _gameEngine.StaticVariables.g_weaponInitFlags[weaponFlagsIndex + 1];
        _gameEngine.StaticVariables.g_playerEffectTransitionCooldown[2] = _gameEngine.StaticVariables.g_weaponInitFlags[weaponFlagsIndex + 2];
        _gameEngine.StaticVariables.g_playerEffectTransitionCooldown[3] = _gameEngine.StaticVariables.g_weaponInitFlags[weaponFlagsIndex + 3];

        return 1;
    }

    //8002eeac
    private int PlayerTryAction()
    {
        var player = _gameEngine.StaticVariables.PlayerEntity;

        if (player.CarriedEntity != null)
        {
            player.TargetAnimationId = (byte)(player.IsAboveGround == 1 ? (int)PlayerAnimation.HoldObject : (int)PlayerAnimation.JumpWithObject);
            return 1;
        }

        if (player.XCollisionEntity != null)
        {
            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & PadState.Square) != 0)
            {
                return PlayerTryInteractWithEntity(player.XCollisionEntity);
            }
        }

        var count = _gameEngine.StaticVariables.g_numberOfEntity;
        if (count < 0)
        {
            return 0;
        }

        for (var i = 0; i < count; i++)
        {
            ref var entity = ref _gameEngine.StaticVariables.g_entitySlots[i];

            // status ∈ {2, 3}  <=> (status - 2) in [0,1]
            var status = entity.Status - 2;
            if (status >= 0 && status < 2)
            {
                if (entity.IsNotProcessable == 0)
                {
                    if (entity.RidingEntity == _gameEngine.StaticVariables.PlayerEntity)
                    {
                        var res = PlayerTryInteractWithEntity(entity);
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
    private int PlayerTryInteractWithEntity(Entity entity)
    {
        var platformFlagBits = (entity.Flags & 0x600) >> 9;
        var result = 1;

        if (platformFlagBits == 1)
        {
            goto TriggerWarp;
        }

        if (platformFlagBits == 0)
        {
            return 0;
        }

        // have keys ?
        if (platformFlagBits < 4)
        {
            if (_gameEngine.PlayerManager.GetNumberOfItem(0x3B) != 0)
            {
                return 0;
            }

            goto TriggerWarp;
        }

        return result;

        TriggerWarp:
        _gameEngine.StaticVariables.PlayerEntity.CarriedEntity = entity;
        entity.PlatformEntity = _gameEngine.StaticVariables.PlayerEntity;

        var above = _gameEngine.StaticVariables.PlayerEntity.IsAboveGround;

        if (entity.RidingEntity == _gameEngine.StaticVariables.PlayerEntity)
        {
            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (byte)(above != 0 ? (int)PlayerAnimation.HoldObject : (int)PlayerAnimation.JumpWithObject);
        }
        else
        {
            _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = (byte)(above != 0 ? (int)PlayerAnimation.PickupObject : (int)PlayerAnimation.Reserved0B);
        }

        _gameEngine.StaticVariables.PlayerEntity.RelativeWarpOffsetX = entity.PosX - _gameEngine.StaticVariables.PlayerEntity.PosX;
        _gameEngine.StaticVariables.PlayerEntity.RelativeWarpOffsetY = entity.PosY - _gameEngine.StaticVariables.PlayerEntity.PosY;
        _gameEngine.StaticVariables.PlayerEntity.RelativeWarpOffsetZ = entity.PosZ - _gameEngine.StaticVariables.PlayerEntity.PosZ;

        return result;
    }

    // 800307e8
    private void UpdateItemEffectState()
    {
        if (_gameEngine.PlayerManager.GetNumberOfItem(0x1C) != 0)
        {
            _gameEngine.StaticVariables.g_gravityFlag = 3;
        }
        else if (_gameEngine.PlayerManager.GetNumberOfItem(0x1B) != 0)
        {
            _gameEngine.StaticVariables.g_gravityFlag = 2;
        }
        else if (_gameEngine.PlayerManager.GetNumberOfItem(0x1A) != 0)
        {
            _gameEngine.StaticVariables.g_gravityFlag = 1;
        }
        else
        {
            _gameEngine.StaticVariables.g_gravityFlag = 0;
        }

        var i = 0x61; // Index 97
        var requiredFlag = 2;
        var iconBase = 0;

        _gameEngine.StaticVariables.g_itemBalanceRecords[2].BalanceRecord = null;
        _gameEngine.StaticVariables.g_itemBalanceRecords[1].BalanceRecord = null;
        _gameEngine.StaticVariables.g_itemBalanceRecords[0].BalanceRecord = null;
        _gameEngine.StaticVariables.g_itemBalanceRecords[2].ItemId = 0;
        _gameEngine.StaticVariables.g_itemBalanceRecords[1].ItemId = 0;
        _gameEngine.StaticVariables.g_itemBalanceRecords[0].ItemId = 0;

        while (i >= 0)
        {
            // Vérifie le bit 0x7F du troisième byte (index+2) de l'icône
            var iconFlags = (byte)(_gameEngine.StaticVariables.g_itemDropProperties[i].Field3 & 0x7F);

            if (iconFlags == requiredFlag && _gameEngine.PlayerManager.GetNumberOfItem(i) != 0)
            {
                var itemData = _gameEngine.BalanceBin.GetItemDataPointer(i, _gameEngine.StaticVariables.g_itemIdThreshold);
                //Debugger.Break();
                _gameEngine.StaticVariables.g_itemBalanceRecords[0].BalanceRecord = itemData;
                _gameEngine.StaticVariables.g_itemBalanceRecords[0].ItemId = i + 0x1E;
                break;
            }

            i--;
        }

        var itemId = (int)_gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();
        if (itemId > 0 && itemId < 0x61)
        {
            var tileIconFlags = (byte)(_gameEngine.StaticVariables.g_itemDropProperties[itemId].Field3 & 0x7F);

            if (tileIconFlags == 1)
            {
                var itemData = _gameEngine.BalanceBin.GetItemDataPointer(itemId, _gameEngine.StaticVariables.g_itemIdThreshold);
                _gameEngine.StaticVariables.g_itemBalanceRecords[1].BalanceRecord = itemData;
                _gameEngine.StaticVariables.g_itemBalanceRecords[1].ItemId = i + 0x1E;
            }
        }

        var currentItemId = _gameEngine.PlayerManager.SetItemIdFromCurrentItemId();
        if (currentItemId > 0 && currentItemId < 0x61)
        {
            var warpIconFlags = (byte)(_gameEngine.StaticVariables.g_itemDropProperties[currentItemId].Field3 & 0x7F);

            if (warpIconFlags == 3)
            {
                var itemData = _gameEngine.BalanceBin.GetItemDataPointer((int)currentItemId, _gameEngine.StaticVariables.g_itemIdThreshold);
                _gameEngine.StaticVariables.g_itemBalanceRecords[2].BalanceRecord = itemData;
                _gameEngine.StaticVariables.g_itemBalanceRecords[2].ItemId = i + 0x1E;
            }
        }

        var balanceRecordSource = _gameEngine.StaticVariables.g_itemBalanceRecords[0].BalanceRecord ?? _gameEngine.StaticVariables.PlayerEntity.BalanceRecord;
        _gameEngine.StaticVariables.g_balanceRecord[0].CopyFrom(balanceRecordSource);

        var valueB = 0x10;
        var valueA = 0x10;
        i = 1;

        do
        {
            var balanceRecord = _gameEngine.StaticVariables.g_balanceRecord[i - 1];

            var result = (uint)balanceRecord.Hp;

            if (_gameEngine.StaticVariables. g_itemBalanceRecords[1].BalanceRecord != null)
            {
                valueA = _gameEngine.StaticVariables.g_itemBalanceRecords[1].BalanceRecord.Values[i + -1];
            }

            if (_gameEngine.StaticVariables.g_itemBalanceRecords[2].BalanceRecord != null)
            {
                valueB = _gameEngine.StaticVariables.g_itemBalanceRecords[2].BalanceRecord.Values[i + -1];
            }

            var flagsA = valueA & 0xc0;
            var flagsB = valueB & 0xc0;

            if ((result & 0xc0) == 0x80 || flagsA == 0x80 || flagsB == 0x80)
            {
                result = 0x80;
            }
            else
            {
                if ((balanceRecord.Hp & 0xc0) != 0)
                {
                    result = 0x10;

                    if (flagsA != 0)
                    {
                        result = 0x40;

                        if (flagsB != 0)
                        {
                            goto LAB_80030c0c;
                        }

                        result = 0x10;
                    }
                }

                if (flagsA == 0)
                {
                    result = (uint)(result - 0x10 + valueA);
                }

                if (flagsB == 0)
                {
                    result = (uint)(result - 0x10 + valueB);
                }

                if ((int)result < 0x20)
                {
                    if ((int)result < 0)
                    {
                        result = 0;
                    }
                }
                else
                {
                    result = 0x1f;
                }
            }

            LAB_80030c0c:
            balanceRecord.Hp = (byte)result;

            i = i + 1;
        } while (i < 0xc);


        if (_gameEngine.StaticVariables.g_playerControlFlags == 0
            && _gameEngine.StaticVariables.PlayerEntity.IsNotProcessable == 0
            && _gameEngine.StaticVariables.g_padState1.ButtonsHold == 0)
        {
            if (_gameEngine.StaticVariables.PlayerEntity.Hp != 0
                && _gameEngine.StaticVariables.PlayerEntity.Hp < _gameEngine.StaticVariables.PlayerEntity.HpMax)
            {
                for (i = 0; i < 3; i++)
                {
                    var balanceRecord2 = _gameEngine.StaticVariables.g_itemBalanceRecords[i].BalanceRecord;

                    if (balanceRecord2 != null && balanceRecord2.Hp != 0)
                    {
                        _gameEngine.StaticVariables.INT_ARRAY_80126fe8[i]++;

                        if (balanceRecord2.Hp <= _gameEngine.StaticVariables.INT_ARRAY_80126fe8[i])
                        {
                            _gameEngine.StaticVariables.INT_ARRAY_80126fe8[i] = 0;
                            _gameEngine.StaticVariables.PlayerEntity.Hp++;
                        }
                    }
                    else
                    {
                        _gameEngine.StaticVariables.INT_ARRAY_80126fe8[i] = 0;
                    }
                }

                if (_gameEngine.StaticVariables.PlayerEntity.Hp > _gameEngine.StaticVariables.PlayerEntity.HpMax)
                {
                    _gameEngine.StaticVariables.PlayerEntity.Hp = _gameEngine.StaticVariables.PlayerEntity.HpMax;
                }
            }
        }
        else
        {
            _gameEngine.StaticVariables.INT_ARRAY_80126fe8[0] = 0;
            _gameEngine.StaticVariables.INT_ARRAY_80126fe8[1] = 0;
            _gameEngine.StaticVariables.INT_ARRAY_80126fe8[2] = 0;
        }

        if (_gameEngine.StaticVariables.g_debugState < 0 && (_gameEngine.StaticVariables.g_debugFlags & 0x400) != 0)
        {
            //// Code pour l'affichage des informations de debug
            //bool debugHeaderPrinted = false;
            //
            //// Affiche les informations sur les effets actifs
            //for (int i = 0; i < 3; i++)
            //{
            //    if (_gameEngine.StaticVariables.g_items[i] != null && _gameEngine.StaticVariables.g_items[i] != 0)
            //    {
            //        if (!debugHeaderPrinted)
            //        {
            //            _gameEngine.StaticVariables.g_debugMessage += "\n";
            //            debugHeaderPrinted = true;
            //        }
            //
            //        byte numAnimVals = (byte)((_gameEngine.StaticVariables.g_balanceRecord[i * 2] >> 16) & 0xFF);
            //        byte animVal = (byte)((_gameEngine.StaticVariables.g_balanceRecord[i * 2 + 1] >> 8) & 0xFF);
            //        var effectType = _gameEngine.StaticVariables.g_effectDebugFlagNames[i];
            //
            //        if (numAnimVals != 0)
            //        {
            //            if (animVal != 0)
            //            {
            //                _gameEngine.StaticVariables.g_debugMessage += string.Format("{0}({1:D2}/A{2:D3}/T{3:D3})",
            //                    effectType, _gameEngine.StaticVariables.g_items[i] - 0x1E, animVal, _gameEngine.StaticVariables.INT_ARRAY_80126fe8[i]);
            //            }
            //            else
            //            {
            //                _gameEngine.StaticVariables.g_debugMessage += string.Format("{0}({1:D2}/A{2:D3})",
            //                    effectType, _gameEngine.StaticVariables.g_items[i] - 0x1E, animVal);
            //            }
            //        }
            //        else if (numAnimVals != 0)
            //        {
            //            _gameEngine.StaticVariables.g_debugMessage += string.Format("{0}({1:D2}/T{2:D3})",
            //                effectType, _gameEngine.StaticVariables.g_items[i] - 0x1E, _gameEngine.StaticVariables.INT_ARRAY_80126fe8[i]);
            //        }
            //        else
            //        {
            //            _gameEngine.StaticVariables.g_debugMessage += string.Format("{0}({1:D2})",
            //                effectType, _gameEngine.StaticVariables.g_items[i] - 0x1E);
            //        }
            //    }
            //}
            //
            //if (debugHeaderPrinted)
            //{
            //    _gameEngine.StaticVariables.g_debugMessage += "\n";
            //}
            //
            // Affiche les informations sur les sources d'effets
            //if (_gameEngine.StaticVariables.g_balanceEffectSources != null)
            //{
            //    _gameEngine.AppendHexVisualDebugLine(_gameEngine.StaticVariables.g_balanceEffectSources, "ARM");
            //}
            //else
            //{
            //    _gameEngine.AppendHexVisualDebugLine(_gameEngine.StaticVariables.PlayerEntity.BalanceRecord, "ALN");
            //}
            //
            //if (_gameEngine.StaticVariables.g_items[2] != null && _gameEngine.StaticVariables.g_items[2] != 0)
            //{
            //    _gameEngine.AppendHexVisualDebugLine(_gameEngine.StaticVariables.g_items[2], "WEP");
            //}
            //
            //if (_gameEngine.StaticVariables.g_items[3] != null && _gameEngine.StaticVariables.g_items[3] != 0)
            //{
            //    _gameEngine.AppendHexVisualDebugLine(_gameEngine.StaticVariables.g_items[3], "ITM");
            //}
            //
            //_gameEngine.AppendHexVisualDebugLine(_gameEngine.StaticVariables.g_balanceRecord, "DEF");
        }
    }

    // 8002f768
    private int UpdateWeaponStepProgression()
    {
        var stepCounter = _gameEngine.StaticVariables.g_playerEffectStepFlags;

        if (_gameEngine.StaticVariables.g_playerEffectTransitionCooldown[0] != 0)
        {
            var weaponInitFlagsIndex = _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId * 2 + 0x9d;

            if (_gameEngine.StaticVariables.g_playerEffectTransitionCooldown[1] == 0
                || _gameEngine.StaticVariables.g_weaponInitFlags[weaponInitFlagsIndex] == 0)
            {
                _gameEngine.StaticVariables.g_playerEffectTransitionCooldown[0] = 0;
                _gameEngine.StaticVariables.g_playerEffectTransitionCooldown[1] = 0;
                _gameEngine.StaticVariables.g_playerEffectTransitionCooldown[2] = 0;
                _gameEngine.StaticVariables.g_playerEffectTransitionCooldown[3] = 0;
            }
            else
            {
                stepCounter = _gameEngine.StaticVariables.g_playerEffectStepFlags + 1;

                if (_gameEngine.StaticVariables.g_playerEffectTransitionCooldown[2] <= _gameEngine.StaticVariables.g_playerEffectStepFlags)
                {
                    var zOffset = _gameEngine.StaticVariables.g_playerEffectTransitionCooldown[3] << 16;
                    var directionIndex = _gameEngine.StaticVariables.PlayerEntity.CurrentDirection >> 3;
                    //TODO check direction
                    directionIndex = directionIndex switch
                    {
                        1 => 2,
                        2 => 1,
                        _ => directionIndex
                    };
                    var direction = _gameEngine.StaticVariables.g_cardinalDirectionTable[directionIndex];

                    var entity = _gameEngine.SpawnWarpEntity(
                        _gameEngine.StaticVariables.PlayerEntity,
                        0,
                        _gameEngine.StaticVariables.g_playerEffectTransitionCooldown[1],
                        _gameEngine.StaticVariables.PlayerEntity.PosX,
                        _gameEngine.StaticVariables.PlayerEntity.PosY,
                        _gameEngine.StaticVariables.PlayerEntity.PosZ + zOffset,
                        (uint)direction);

                    _gameEngine.StaticVariables.g_playerEffectTransitionCooldown[0] = 0;
                    _gameEngine.StaticVariables.g_playerEffectTransitionCooldown[1] = 0;
                    _gameEngine.StaticVariables.g_playerEffectTransitionCooldown[2] = 0;
                    _gameEngine.StaticVariables.g_playerEffectTransitionCooldown[3] = 0;
                    return 1;
                }
            }
        }

        _gameEngine.StaticVariables.g_playerEffectStepFlags = stepCounter;
        return 0;
    }

    //8002f49c
    private int UpdatePlayerWeaponEffect()
    {
        var buttonHeld = (_gameEngine.StaticVariables.g_padState1.ButtonsHold & PadState.Square) != 0;
        var buttonReleased = (_gameEngine.StaticVariables.g_padState1.ButtonsReleased & PadState.Square) != 0;

        if (buttonHeld || buttonReleased)
        {
            var currentWeaponFlags = _gameEngine.StaticVariables.g_currentWeaponFlags;
            var weaponInitFlag = (uint)_gameEngine.StaticVariables.g_weaponInitFlags[currentWeaponFlags];

            if (weaponInitFlag != 0)
            {
                var playerAnimId = _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId;
                byte initFlag = _gameEngine.StaticVariables.g_weaponInitFlags[playerAnimId * 2 + 0x9c];

                if (initFlag != 0)
                {
                    // Increment warp timer (max 0x3C = 60)
                    if (_gameEngine.StaticVariables.g_playerWarpTimer < 0x3C)
                    {
                        _gameEngine.StaticVariables.g_playerWarpTimer++;
                    }
                }
                else
                {
                    _gameEngine.StaticVariables.g_playerWarpTimer = 0;
                }
            }
            else
            {
                _gameEngine.StaticVariables.g_playerWarpTimer = 0;
            }
        }
        else
        {
            _gameEngine.StaticVariables.g_playerWarpTimer = 0;
        }

        // Main effect processing logic
        if (_gameEngine.StaticVariables.g_playerWarpTimer >= 0x0B) // Timer threshold of 11
        {
            // Check if we need to create/manage frame timer effect
            if (_gameEngine.StaticVariables.g_playerWarpEffect == null)
            {
                // Create attached effect type 3 with specific parameters
                var attachedEffect = _gameEngine.EffectManager.CreateAttachedEffect(
                    0,    // ismapeffect
                    3,    // effectid  
                    0,    // animid
                    _gameEngine.StaticVariables.PlayerEntity, // entity
                    0x10000, // depthsortmod
                    0,    // xoff
                    0,    // yoff
                    0     // zoff
                );

                _gameEngine.StaticVariables.g_playerWarpEffect = attachedEffect;
            }
            else
            {
                // Check if effect should be destroyed
                if (_gameEngine.StaticVariables.g_playerWarpEffect.Status == 0)
                {
                    _gameEngine.StaticVariables.g_playerWarpEffect = null;
                    return _gameEngine.StaticVariables.g_playerWarpTimer;
                }
            }

            // Process frame-based effects every 4 frames
            if ((_gameEngine.StaticVariables.PlayerEntity.FrameCounter & 0x3) == 0)
            {
                if (_gameEngine.StaticVariables.g_playerWarpTimer < 0x3C) // Less than 60
                {
                    // Play sound effect
                    _gameEngine.SoundManager.PlaySoundEffect(0x2A); // Sound ID 42
                }
                else
                {
                    // Create effect entity at player position
                    var spriteEffect = _gameEngine.EffectManager.CreateEffectEntity(
                        0,    // behaviorFlags
                        0x10, // spriteTableIndex (16)
                        0,    // animationIndex
                        _gameEngine.StaticVariables.PlayerEntity.PosX,
                        _gameEngine.StaticVariables.PlayerEntity.PosY,
                        _gameEngine.StaticVariables.PlayerEntity.PosZ + 0x100000 // Z offset
                    );

                    if (spriteEffect != null)
                    {
                        // Generate random sound effect (0x1AC or 0x1AD)
                        var soundId = (_gameEngine.StaticVariables.PlayerEntity.FrameCounter & 0x7) == 0 ? 0x1ADU : 0x1ACU;
                        _gameEngine.SoundManager.PlaySoundEffect(soundId);

                        // Generate random forces using game's random seed
                        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                        var randomSeed1 = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed;

                        _gameEngine.StaticVariables.g_gameRandomSeed = (uint)(randomSeed1 * 0x7d2b89dd + 0xe06a02e7);
                        var randomSeed2 = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed;

                        // Calculate random force components
                        // Complex math for random X force
                        var temp1 = randomSeed1 * 0x60001;
                        var randomXComponent = (int)(temp1 >> 32);
                        randomXComponent -= 0x30000; // Bias
                        var adjustedXForce = randomXComponent * 5;

                        // Complex math for random Y force  
                        _gameEngine.StaticVariables.g_gameRandomSeed = (uint)(randomSeed2 * 0x7d2b89dd + 0xe06a02e7);
                        var temp2 = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x40001;
                        var randomYComponent = (int)(temp2 >> 32);
                        randomYComponent -= 0x20000; // Bias
                        var adjustedYForce = randomYComponent * 5;

                        // Apply forces relative to player's current forces
                        spriteEffect.ForceX += adjustedXForce;
                        spriteEffect.ForceY += adjustedYForce;

                        // Subtract player forces to create relative motion
                        spriteEffect.ForceX = _gameEngine.StaticVariables.PlayerEntity.ForceX - randomXComponent;
                        spriteEffect.ForceY = _gameEngine.StaticVariables.PlayerEntity.ForceY - randomYComponent;
                    }
                }
            }
        }
        else
        {
            // Timer below threshold - clean up effects
            if (_gameEngine.StaticVariables.g_playerWarpEffect != null)
            {
                _gameEngine.StaticVariables.g_playerWarpEffect.Status = 0; // Destroy effect
                _gameEngine.StaticVariables.g_playerWarpEffect = null;
            }
        }

        return _gameEngine.StaticVariables.g_playerWarpTimer;
    }

    // 8002e910
    private int CheckEntityInteraction()
    {
        int res; 
        var playerEntity = _gameEngine.StaticVariables.PlayerEntity;
        Entity collidedEntity;

        collidedEntity = playerEntity.XCollisionEntity;

        if (playerEntity.XCollisionEntity == null)
        {
            if (_gameEngine.StaticVariables.g_lastValidWarpEntity == null ||
                (_gameEngine.StaticVariables.g_lastValidWarpEntity.Index2 ==
                 _gameEngine.StaticVariables.g_lastWarpFacing &&
                 _gameEngine.StaticVariables.g_lastValidWarpEntity.PosX ==
                 _gameEngine.StaticVariables.g_lastWarpTargetX &&
                 _gameEngine.StaticVariables.g_lastValidWarpEntity.PosY ==
                 _gameEngine.StaticVariables.g_lastWarpTargetY &&
                 _gameEngine.StaticVariables.g_lastValidWarpEntity.PosZ ==
                 _gameEngine.StaticVariables.g_lastWarpTargetZ &&
                 playerEntity.PosX == _gameEngine.StaticVariables.g_lastWarpCamX &&
                 playerEntity.PosY == _gameEngine.StaticVariables.g_lastWarpCamY &&
                 playerEntity.PosZ == _gameEngine.StaticVariables.g_lastWarpCamZ))
            {
                collidedEntity = _gameEngine.StaticVariables.g_lastValidWarpEntity;

                if (playerEntity.TargetDirection == _gameEngine.StaticVariables.g_lastWarpDirection)
                {
                    goto FinalCheck;
                }
            }
        }
        else if ((playerEntity.XCollisionEntity.Flags & 0x8000U) != 0)
        {
            _gameEngine.StaticVariables.g_lastWarpFacing = playerEntity.XCollisionEntity.Index2;
            _gameEngine.StaticVariables.g_lastValidWarpEntity = playerEntity.XCollisionEntity;
            _gameEngine.StaticVariables.g_lastWarpTargetX = playerEntity.XCollisionEntity.PosX;
            _gameEngine.StaticVariables.g_lastWarpTargetY = playerEntity.XCollisionEntity.PosY;
            _gameEngine.StaticVariables.g_lastWarpTargetZ = playerEntity.XCollisionEntity.PosZ;
            _gameEngine.StaticVariables.g_lastWarpCamX = playerEntity.PosX;
            _gameEngine.StaticVariables.g_lastWarpCamY = playerEntity.PosY;
            _gameEngine.StaticVariables.g_lastWarpCamZ = playerEntity.PosZ;
            _gameEngine.StaticVariables.g_lastWarpDirection = playerEntity.TargetDirection;
            goto FinalCheck;
        }

        _gameEngine.StaticVariables.g_lastValidWarpEntity = null;
        collidedEntity = playerEntity.XCollisionEntity;

        FinalCheck:
        res = 0;

        if (collidedEntity != null &&
            (collidedEntity.ProgramIndexes[5] != 0 || collidedEntity.SpriteProgramIndexes[5] != 0))
        {
            if ((collidedEntity.Flags & 0x8000U) == 0)
            {
                res = 1;
                _gameEngine.StaticVariables.g_activeCollisionEntity = collidedEntity;
            }
            else if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & 0x80) == 0)
            {
                res = 0;
            }
            else
            {
                res = 2;
                _gameEngine.StaticVariables.g_activeCollisionEntity = collidedEntity;
            }
        }

        return res;
    }


    // 8004df80
    public int SetMoney(short amount)
    {
        if (amount < 10000)
        {
            if (amount < 0)
            {
                _gameEngine.StaticVariables.g_playerStats.MoneyAmount = 0;
            }
            else
            {
                _gameEngine.StaticVariables.g_playerStats.MoneyAmount = amount;
            }
        }
        else
        {
            _gameEngine.StaticVariables.g_playerStats.MoneyAmount = 9999;
        }

        return _gameEngine.StaticVariables.g_playerStats.MoneyAmount;
    }

    // 8004b730
    public void InitializeHpAndMp()
    {
        _gameEngine.StaticVariables.INT_ARRAY_800a8284[0] = GetPlayerHpMax();
        _gameEngine.StaticVariables.INT_ARRAY_800a8284[1] = _gameEngine.StaticVariables.INT_ARRAY_800a8284[0];
        _gameEngine.StaticVariables.INT_ARRAY_800a8284[3] = GetPlayerMpMax();
        _gameEngine.StaticVariables.INT_ARRAY_800a8284[2] = _gameEngine.StaticVariables.INT_ARRAY_800a8284[3];
    }

    // 8004e78c
    public int GetPlayerHpMax()
    {
        return _gameEngine.StaticVariables.g_playerStats.HpMax;
    }

    // 8004dd18
    public int GetPlayerHp()
    {
        return _gameEngine.StaticVariables.g_playerStats.Hp;
    }

    // 8004dc68
    public int SetPlayerHpMax(int hpMax)
    {
        //Set HP
        if (hpMax < 0x33)
        {
            if (hpMax < 0)
            {
                _gameEngine.StaticVariables.g_playerStats.HpMax = 0;
            }
            else
            {
                _gameEngine.StaticVariables.g_playerStats.HpMax = (short)hpMax;
            }
        }
        else
        {
            _gameEngine.StaticVariables.g_playerStats.HpMax = 0x32;
        }

        return _gameEngine.StaticVariables.g_playerStats.HpMax;
    }

    // 8004dd30
    public int SetPlayerHp(short amount)
    {
        //set HP
        if (_gameEngine.StaticVariables.g_playerStats.HpMax < amount)
        {
            _gameEngine.StaticVariables.g_playerStats.Hp = _gameEngine.StaticVariables.g_playerStats.HpMax;
        }
        else if (amount < 0)
        {
            _gameEngine.StaticVariables.g_playerStats.Hp = 0;
        }
        else
        {
            _gameEngine.StaticVariables.g_playerStats.Hp = amount;
        }

        return _gameEngine.StaticVariables.g_playerStats.Hp;
    }

    //8004df10
    public void IncreaseMp(int amount)
    {
        SetPlayerMp((short)(amount + _gameEngine.StaticVariables.g_playerStats.Mp));
    }

    //8004de4c
    public void IncreaseMpMax(int amount)
    {
        SetPlayerMpMax((short)(amount + _gameEngine.StaticVariables.g_playerStats.MpMax));
    }

    //8004dea4
    private int GetPlayerMp()
    {
        return _gameEngine.StaticVariables.g_playerStats.Mp;
    }

    // 8004dddc
    public int GetPlayerMpMax()
    {
        return _gameEngine.StaticVariables.g_playerStats.MpMax;
    }

    // 8004ddf4
    public int SetPlayerMpMax(short mpMax)
    {
        if (mpMax < 5)
        {
            if (mpMax < 0)
            {
                _gameEngine.StaticVariables.g_playerStats.MpMax = 0;
            }
            else
            {
                _gameEngine.StaticVariables.g_playerStats.MpMax = mpMax;
            }
        }
        else
        {
            _gameEngine.StaticVariables.g_playerStats.MpMax = 4;
        }

        return _gameEngine.StaticVariables.g_playerStats.MpMax;
    }

    // 8004debc
    public int SetPlayerMp(short amount)
    {
        if (_gameEngine.StaticVariables.g_playerStats.MpMax < amount)
        {
            _gameEngine.StaticVariables.g_playerStats.Mp = _gameEngine.StaticVariables.g_playerStats.MpMax;
        }
        else if (amount < 0)
        {
            _gameEngine.StaticVariables.g_playerStats.Mp = 0;
        }
        else
        {
            _gameEngine.StaticVariables.g_playerStats.Mp = amount;
        }

        return _gameEngine.StaticVariables.g_playerStats.Mp;
    }

    // 8004e484
    public void SetPlayerWeaponId(ushort weaponId)
    {
        if (weaponId == 0xffffffff || weaponId - 1 < 6)
        {
            _gameEngine.StaticVariables.g_playerStats.WeaponId = (short)weaponId;
        }
        else
        {
            //LogDebugMessage(_gameEngine.StaticVariables.g_logMessage_InvalidWarpVisualId, HpMax);
        }

        _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();
    }

    // 800347d4
    public int PlayCutscene(uint itemId)
    {
        int iVar1;
        int iVar2;

        if (_gameEngine.StaticVariables.PlayerEntity.HpMax <= _gameEngine.StaticVariables.PlayerEntity.Hp)
        {
            iVar1 = GetPlayerMp();
            iVar2 = GetPlayerMpMax();
            if (iVar2 <= iVar1)
            {
                _gameEngine.SoundManager.PlaySoundEffect(3);
                return 1;
            }
        }

        _gameEngine.PlayerManager.RestoreHpAndMpAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity);
        _gameEngine.SoundManager.PlaySoundEffect(0x30);
        UseItem(itemId);
        return 1;
    }

    //8004e5c4
    public int UseItem(uint itemId)
    {
        int remainingItem;
        short itemCount;

        if (itemId < 0 || _gameEngine.StaticVariables.g_itemsCount <= itemId)
        {
            //LogDebugMessage(_gameEngine.StaticVariables.g_logMessage_InvalidWarpVisualId + 0x54, itemId);
            remainingItem = 0;
        }
        else
        {
            remainingItem = (int)(itemId * 2 * 2 + _gameEngine.StaticVariables.g_numberOfItems[0]);
            itemCount = _gameEngine.StaticVariables.g_numberOfItems[itemId * 2 + 1];
            itemCount--;

            if (itemCount == -1)
            {
                remainingItem = -1;
            }
            else
            {
                _gameEngine.StaticVariables.g_numberOfItems[itemId * 2 + 1] = itemCount;
                remainingItem = itemCount;
            }
        }

        return remainingItem;
    }

    // 8002ed64
    private int TryUseItem()
    {
        if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & PadState.Circle) != 0)
        {
            if (_gameEngine.StaticVariables.g_playerControlFlags == 0)
            {
                return UseItem();
            }
            else
            {
                _gameEngine.SoundManager.PlaySoundEffect(3);
            }
        }

        return 1;
    }

    //8003499c
    private int UseItem()
    {
        var itemId = _gameEngine.PlayerManager.SetItemIdFromCurrentItemId();

        if (itemId >= 0x62)
        {
            goto DefaultCase;
        }

        if (_gameEngine.StaticVariables.g_warpLockTimer != 0 && _gameEngine.StaticVariables.g_warpLockTimer != itemId)
        {
            goto DefaultCase;
        }

        var switchValue = itemId - 0x1f;

        if (switchValue >= 0x14)
        {
            goto DefaultCase;
        }

        int result;

        switch (switchValue)
        {
            case 0: // mapId == 0x1f
                result = TrySpawnWarpEntity(itemId);
                break;

            case 1: // mapId == 0x20
                result = FUN_80034320(itemId);
                break;

            case 4: // mapId == 0x23
                result = FUN_8003453c(itemId);
                break;

            case 5: // mapId == 0x24
                FUN_80034680(itemId);
                return 1;

            case 6: // mapId == 0x25
                TryWarpToMap(itemId);
                return 1;

            case 7: // mapId == 0x26
                UseMagicalItem(itemId);
                return 1;

            case 8: // mapId == 0x27
                PlayCutscene(itemId);
                return 1;

            case 10: // mapId == 0x29
                TryWarpWithExplosionEffect(itemId);
                return 1;

            case 13: // mapId == 0x2c
            case 14: // mapId == 0x2d
            case 15: // mapId == 0x2e
            case 16: // mapId == 0x2f
            case 17: // mapId == 0x30
            case 18: // mapId == 0x31
            case 19: // mapId == 0x32
            case 12: // mapId == 0x2b
                result = TryStartMapWarp(itemId);
                break;

            default:
                goto DefaultCase;
        }

        if (result == 0)
        {
            _gameEngine.StaticVariables.g_warpLockTimer = (int)itemId;
            _gameEngine.StaticVariables.g_playerEffectCurrentFrame = 0;
            _gameEngine.StaticVariables.g_playerEffectPhase = 0;
        }

        return result;

        DefaultCase:
        _gameEngine.SoundManager.PlaySoundEffect(3);
        return 1;
    }

    //80034224
    private int TrySpawnWarpEntity(uint itemId)
    {
        Debugger.Break();
        return 0;
    }

    //80034320
    private int FUN_80034320(uint itemId)
    {
        Debugger.Break();
        return 0;
    }

    //8003453c
    private int FUN_8003453c(uint itemId)
    {
        Debugger.Break();
        return 0;
    }

    //80034680
    private void FUN_80034680(uint itemId)
    {
        Debugger.Break();
    }

    //800346f0
    private void TryWarpToMap(uint itemId)
    {
        Debugger.Break();
    }

    //80034760
    private int UseMagicalItem(uint itemId)
    {
        int mp;
        int mpMax;

        mp = GetPlayerMp();
        mpMax = GetPlayerMpMax();

        if (mp < mpMax)
        {
            _gameEngine.PlayerManager.RestoreMpAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity);
            _gameEngine.SoundManager.PlaySoundEffect(0x30);
            UseItem(itemId);
        }
        else
        {
            _gameEngine.SoundManager.PlaySoundEffect(3);
        }

        return 1;
    }

    private void TryWarpWithExplosionEffect(uint itemId)
    {
        Debugger.Break();
    }

    //80034870
    private int TryStartMapWarp(uint itemId)
    {
        Debugger.Break();
        return 0;
    }

    //8003634c
    private int MaybeStartWarpAnimation()
    {
        int iVar1;

        if (_gameEngine.StaticVariables.g_warpLockTimer == 0)
        {
            iVar1 = 1;
        }
        else
        {
            switch (_gameEngine.StaticVariables.g_warpLockTimer)
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
                    iVar1 = ProcessPlayerEffectSequence(_gameEngine.StaticVariables.g_warpLockTimer);
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

            if (_gameEngine.StaticVariables.g_playerEffectCurrentFrame < 0x7fffffff)
            {
                _gameEngine.StaticVariables.g_playerEffectCurrentFrame += 1;
            }

            if (iVar1 != 0)
            {
                _gameEngine.StaticVariables.g_warpLockTimer = 0;
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

        var player = _gameEngine.StaticVariables.PlayerEntity;

        entity2 = player.TouchingEntity;
        if (player.TouchingEntity == null)
        {
            direction = 0xffffffff;
            if (player.DamagedTickCounter == 0)
            {
                direction = 0xffffffff;
                if (_gameEngine.StaticVariables.g_gravityFlag < 3)
                {
                    entryType = (uint)((player.CombinedVramFlagsOR & 0x180U) >> 7);
                    direction = 0xffffffff;

                    if (entryType != 0)
                    {
                        iVar1 = _gameEngine.StaticVariables.g_warpStepThresholdTable[entryType];
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
    private void UpdatePlayerCarriedEntity(int mode)
    {
        int dx, dy, dz;
        Entity carriedEntity;
        int newDx, newDy, newDz;

        carriedEntity = _gameEngine.StaticVariables.PlayerEntity.CarriedEntity;

        if (carriedEntity == null)
        {
            goto LAB_8002FAF0;
        }

        if (carriedEntity.BalanceRecord.NumAnimVals == 0 && mode == 1)
        {
            mode = 0;
        }

        if (mode == 0)
        {
            LAB_8002F8E0:
            var player = _gameEngine.StaticVariables.PlayerEntity;

            dx = carriedEntity.PosX - player.PosX;
            dy = carriedEntity.PosY - player.PosY;
            dz = carriedEntity.PosZ - player.PosZ;

            newDx = dx >= 0 ? dx : -dx;
            newDy = dy >= 0 ? dy : -dy;
            newDz = dz - 0x200000;

            if (newDz < 0)
            {
                newDz = 0x200000 - dz;
            }

            if (newDx < newDy)
            {
                newDx = newDy;
            }

            if (newDx < newDz)
            {   /* assez proche sur Z, on amortit X & Y */
                dx = StepTowards(dx, 0, 0x00010000);
                dy = StepTowards(dy, 0, 0x00010000);
            }

            dz = StepTowards(dz, 0x00200000, 0x00010000);

            player.RelativeWarpOffsetX = dx;
            player.RelativeWarpOffsetY = dy;
            player.RelativeWarpOffsetZ = dz;
            goto LAB_8002FAF0;
        }

        if (mode == 1)
        {
            LAB_8002F9AC:
            if (carriedEntity.BalanceRecord.NumAnimVals == 0)
            {
                goto LAB_8002FAF0;
            }

            if (_gameEngine.StaticVariables.g_warpDelayCounter < 5)
            {
                _gameEngine.StaticVariables.g_warpDelayCounter++;
                goto LAB_8002FAAC;
            }

            var player = _gameEngine.StaticVariables.PlayerEntity;
            dx = carriedEntity.PosX - player.PosX;
            dy = carriedEntity.PosY - player.PosY;
            dz = carriedEntity.PosZ - player.PosZ;

            newDx = dx >= 0 ? dx : -dx;
            newDy = dy >= 0 ? dy : -dy;
            newDz = dz >= 0 ? dz : -dz;

            /* si déjà très proche (<0x0000_FFFF) sur chaque axe,
               on détruit l’entité-warp et termine                 */
            if (newDx < 0x0000FFFF && newDy < 0x0000FFFF && newDz < 0x0000FFFF)
            {
                _gameEngine.DestroyEntity(carriedEntity);
                goto LAB_8002FAF8;
            }

            /* sinon, amortissement avec step 0x0002_0000          */
            dx = StepTowards(dx, 0, 0x00020000);
            dy = StepTowards(dy, 0, 0x00020000);
            dz = StepTowards(dz, 0, 0x00020000);

            player.RelativeWarpOffsetX = dx;
            player.RelativeWarpOffsetY = dy;
            player.RelativeWarpOffsetZ = dz;

            LAB_8002FAAC:
            goto LAB_8002FAF8;
        }

        LAB_8002FAB4:
        {
            var player = _gameEngine.StaticVariables.PlayerEntity;

            carriedEntity.Flags2 = -1; // TODO flags2 ??
            carriedEntity.TargetDirection = player.TargetDirection;

            carriedEntity.PosX = player.PosX;
            carriedEntity.PosY = player.PosY;
            carriedEntity.PosZ = player.PosZ + 0x00200000;

            goto LAB_8002FAF0;
        }

        LAB_8002FAF0:
        _gameEngine.StaticVariables.g_warpDelayCounter = 0;

        LAB_8002FAF8:
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
        Portal portal;
        int iVar1;
        int combinedVramFlagsAnd;
        string buffer;
        string fmt;
        uint direction;

        combinedVramFlagsAnd = _gameEngine.StaticVariables.PlayerEntity.CombinedVramFlagsAND;

        if (_gameEngine.StaticVariables.g_debugState < 0
            && (_gameEngine.StaticVariables.g_debugFlags & 4) != 0
            && (_gameEngine.StaticVariables.g_debugFlags & 0x8000004) != 0x8000004)
        {
            if (_gameEngine.StaticVariables.g_isWarpDisabled == 0)
            {
                _gameEngine.StaticVariables.DAT_80098f24 += 1;
                //_gameEngine.StaticVariables.g_debugMessage += "Attr     : %08X("  + combinedVramFlagsAnd;

                if ((combinedVramFlagsAnd & 4U) == 0)
                {
                    if ((combinedVramFlagsAnd & 0x8000U) == 0 || _gameEngine.StaticVariables.g_playerControlFlags != 0)
                    {
                        //buffer = _gameEngine.StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                        //fmt = "None)\n";
                    }
                    else
                    {
                        //string.Format(_gameEngine.StaticVariables.g_debugMessage + combinedVramFlagsAnd, "Warp)\n");
                        portal = _gameEngine.GetPortal();
                        if (portal == null)
                        {
                            //buffer = _gameEngine.StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                            //fmt = "No Portal.\n";
                        }
                        else
                        {
                            direction = (uint)(portal.Flags >> 14);
                            //_gameEngine.PrintDebugWarpInfo(pbVar2, (int)uVar3);

                            ushort requiredInput = _gameEngine.StaticVariables.BYTE_ARRAY_80022778[direction * 2];

                            if ((_gameEngine.StaticVariables.g_padState1.ButtonsHold & requiredInput) == 0
                                || _gameEngine.StaticVariables.PlayerEntity.CurrentFrameIndex != direction)
                            {
                                //buffer = _gameEngine.StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                                //fmt = "Warp Not Ready!\n";
                            }
                            else if ((_gameEngine.StaticVariables.DAT_80098f24 & 4) == 0)
                            {
                                //buffer = _gameEngine.StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                                //fmt = "Warp Ready!\n";
                            }
                            else
                            {
                                //buffer = _gameEngine.StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                                //fmt = "\n";
                            }
                        }
                    }
                }
                else
                {
                    //string.Format(_gameEngine.StaticVariables.g_debugMessage + combinedVramFlagsAnd, "Hole)\n");
                    portal = _gameEngine.GetPortal();
                    if (portal == null)
                    {
                        //buffer = _gameEngine.StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                        //fmt = "No Portal.\n";
                    }
                    else
                    {
                        //_gameEngine.PrintDebugWarpInfo(pbVar2, 4);
                        if ((_gameEngine.StaticVariables.DAT_80098f24 & 4) == 0)
                        {
                            //buffer = _gameEngine.StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                            //fmt = "Warp Ready!\n";
                        }
                        else
                        {
                            //buffer = _gameEngine.StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                            //fmt = "\n";
                        }
                    }
                }
            }
            else
            {
                //buffer = _gameEngine.StaticVariables.g_debugMessage + combinedVramFlagsAnd;
                //fmt = "WARP DISABLE!\n";
            }

            //string.Format(buffer, fmt);
            return;
        }

        if ((_gameEngine.StaticVariables.PlayerEntity.CombinedVramFlagsAND & 4U) == 0)
        {
            if ((_gameEngine.StaticVariables.PlayerEntity.CombinedVramFlagsAND & 0x8000U) == 0)
            {
                return;
            }

            if (_gameEngine.StaticVariables.g_playerControlFlags != 0)
            {
                return;
            }

            portal = _gameEngine.GetPortal();
            if (portal == null)
            {
                return;
            }

            direction = (uint)(portal.Flags >> 14);
            ushort requiredInput = _gameEngine.StaticVariables.BYTE_ARRAY_80022778[direction * 2];

            if (((_gameEngine.StaticVariables.g_padState1.ButtonsHold >> 8) & requiredInput) == 0)
            {
                return;
            }

            direction = direction switch
            {
                1 => 2,
                2 => 1,
                _ => direction
            };

            if (_gameEngine.StaticVariables.PlayerEntity.CurrentDirection >> 3 != direction)
            {
                return;
            }

            if (((portal.Flags & 0x3000) >> 11) > 3) Debugger.Break();

            combinedVramFlagsAnd = _gameEngine.StaticVariables.g_cardinalDirectionTable[(portal.Flags & 0x3000) >> 11];
        }
        else
        {
            portal = _gameEngine.GetPortal();
            if (portal == null)
            {
                return;
            }

            if (((portal.Flags & 0x3000) >> 11) > 3) Debugger.Break();

            combinedVramFlagsAnd = _gameEngine.StaticVariables.g_cardinalDirectionTable[(portal.Flags & 0x3000) >> 11];
        }

        HandleWarpTransition(portal, 0x36, combinedVramFlagsAnd);
    }

    // 80031340
    public void HandleWarpTransition(Portal portal, int warpType, int extraData)
    {
        if (_gameEngine.StaticVariables.g_isWarpDisabled != 0)
        {
            return;
        }

        _gameEngine.StaticVariables.g_mapTransitionEffectId = (portal.Flags & 0x70) >> 4;

        int internalMapIdx = _gameEngine.StaticVariables.g_mapIdToInternalMapIndexTable[portal.DestMapId];
        _gameEngine.StaticVariables.g_desiredMap = portal.DestMapId;

        Entity playerEntity = _gameEngine.StaticVariables.PlayerEntity;

        int deltaX = portal.DestTileX * StaticVariables.MapTileWidth + (playerEntity.PosX >> 16) - portal.X1 * StaticVariables.MapTileWidth;
        int deltaY = portal.DestTileY * StaticVariables.MapTileHeight + (playerEntity.PosY >> 16) - portal.Y1 * StaticVariables.MapTileHeight;


        int tileX = _gameEngine.StaticVariables.g_tileToWorldXTable[deltaX];
        deltaY /= StaticVariables.MapTileHeight;

        int targetCamX = (tileX * StaticVariables.MapTileWidth + StaticVariables.MapTileWidth / 2) << 16;
        int targetCamY = (deltaY * StaticVariables.MapTileHeight + StaticVariables.MapTileHeight / 2) << 16;
        int targetCamZ = portal.ZLevel << 20;

        _gameEngine.StaticVariables.g_warpEntryBehavior = _gameEngine.StaticVariables.g_warpBehaviorTable[portal.Flags & 0xF];

        if (_gameEngine.StaticVariables.g_mapTransitionEffectId == 3)
        {
            if (internalMapIdx != _gameEngine.StaticVariables.g_currentMap)
            {
                //_gameEngine.DoNothing();
                _gameEngine.StaticVariables.g_mapTransitionEffectId = 0;
            }
            else if (playerEntity.CarriedEntity != null)
            {
                Entity warpEntity = playerEntity.CarriedEntity;
                warpEntity.PosX += targetCamX - playerEntity.PosX;
                warpEntity.PosY += targetCamY - playerEntity.PosY;
                warpEntity.PosZ += targetCamZ - playerEntity.PosZ;
            }

            playerEntity.PosX = targetCamX;
            playerEntity.PosY = targetCamY;
            playerEntity.PosZ = targetCamZ;
        }
        else
        {
            _gameEngine.StaticVariables.g_isGameEnding = 1;
            _gameEngine.StaticVariables.g_warpTriggerType = warpType;
            _gameEngine.StaticVariables.g_warpExtraParam = extraData;
            _gameEngine.StaticVariables.g_cameraTargetX = targetCamX;
            _gameEngine.StaticVariables.g_cameraTargetY = targetCamY;
            _gameEngine.StaticVariables.g_cameraTargetZ = targetCamZ;
        }
    }

    // 8002fb14
    public void UpdatePlayerAnimationEffects(int mode)
    {
        var effectEntityId = 0;
        byte effectId = 0;
        var animIndex = 0;
        var frameOffset = 0;
        SpriteEffect entityCreated = null;
        SpriteEffect spriteEffect = null;

        switch (_gameEngine.StaticVariables.PlayerEntity.TargetAnimationId)
        {
            case (int)PlayerAnimation.SprintDash:
            case (int)PlayerAnimation.AttackSwordDaggerLegend:
            case (int)PlayerAnimation.AttackFlailIron:
            case (int)PlayerAnimation.JumpAttackSwordDagger:
            case (int)PlayerAnimation.JumpAttackFlailIron:
            case (int)PlayerAnimation.ChargeAttackSword:
            case (int)PlayerAnimation.ChargeAttackFlailSteel:
            case (int)PlayerAnimation.AttackSword:
            case (int)PlayerAnimation.AttackFlailSteel:
            case (int)PlayerAnimation.JumpAttackSword:
            case (int)PlayerAnimation.JumpAttackFlailSteel:
            case (int)PlayerAnimation.AttackSwordFiendBlade:
            case (int)PlayerAnimation.JumpAttackSwordFiendBlade:
            case (int)PlayerAnimation.AttackSwordHoly:
            case (int)PlayerAnimation.JumpAttackSwordHoly:
                _gameEngine.CheckAndTriggerTileEffect(_gameEngine.StaticVariables.PlayerEntity);
                break;

            case (int)PlayerAnimation.PrepareSprint:
                if ((_gameEngine.StaticVariables.PlayerEntity.FrameCounter & 0x7) == 0)
                {
                    var sfxId = (uint)_gameEngine.StaticVariables.g_hitSoundEffects[_gameEngine.StaticVariables.PlayerEntity.Slope_18c];
                    _gameEngine.SoundManager.PlaySoundEffect(sfxId);
                }

                if (IsSlopeInAquaticTile())
                {
                    effectEntityId = 40;
                    effectId = 6;
                }
                else
                {
                    effectEntityId = 8;
                    effectId = _gameEngine.CurrentMap.Info.C; //slideEffectId ?
                }

                animIndex = _gameEngine.StaticVariables.PlayerEntity.AnimCompleteCounter - 1;
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

                if ((_gameEngine.StaticVariables.g_hitSoundEffects[effectEntityId + frameOffset] & _gameEngine.StaticVariables.PlayerEntity.FrameCounter) != 0)
                {
                    break;
                }

                if (effectId != 0)
                {
                    spriteEffect = _gameEngine.EffectManager.CreateEffectEntity(
                        0,
                        effectId,
                        0,
                        _gameEngine.StaticVariables.PlayerEntity.PosX,
                        _gameEngine.StaticVariables.PlayerEntity.PosY,
                        _gameEngine.StaticVariables.PlayerEntity.FloorHeight);
                }

                if (spriteEffect != null)
                {
                    frameOffset = _gameEngine.StaticVariables.PlayerEntity.CurrentFrameIndex;
                    animIndex = _gameEngine.StaticVariables.g_hitSoundEffects[effectEntityId + animIndex + 4];

                    var rand = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    var index = effectEntityId + _gameEngine.StaticVariables.PlayerEntity.CurrentFrameIndex * 2 + 9;
                    var index2 = effectEntityId + _gameEngine.StaticVariables.PlayerEntity.CurrentFrameIndex * 2 + 8;
                    spriteEffect.ForceX =
                        _gameEngine.StaticVariables.g_hitSoundEffects[index] * animIndex +
                        (int)((rand * (ulong)(_gameEngine.StaticVariables.g_hitSoundEffects[index2] * animIndex + 1)) >> 0x20);

                    rand = rand * 0x7d2b89dd + 0xe06a02e7;
                    index = effectEntityId + frameOffset * 2 + 0x13;
                    index2 = effectEntityId + frameOffset * 2 + 0x12;
                    spriteEffect.ForceY = _gameEngine.StaticVariables.g_hitSoundEffects[index] * animIndex +
                                          (int)((rand * (ulong)(_gameEngine.StaticVariables.g_hitSoundEffects[index2] * animIndex + 1)) >> 0x20);

                    _gameEngine.StaticVariables.g_gameRandomSeed = (uint)(rand * 0x7d2b89dd + 0xe06a02e7);
                    rand = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed;
                    index = effectEntityId + 0x19;
                    index2 = effectEntityId + 0x18;
                    spriteEffect.ForceZ = (int)(_gameEngine.StaticVariables.g_hitSoundEffects[index] * animIndex +
                        (uint)(rand * (ulong)(_gameEngine.StaticVariables.g_hitSoundEffects[index2] * animIndex + 1)) >> 0x20);
                }
                break;
        }

        if (_gameEngine.StaticVariables.PlayerEntity.ForceX == 0 && _gameEngine.StaticVariables.PlayerEntity.ForceY == 0)
        {
            goto SkipEffects;
        }

        switch (_gameEngine.StaticVariables.PlayerEntity.TargetAnimationId)
        {
            case (int)PlayerAnimation.Moving:
            case (int)PlayerAnimation.MovingWithObject:
                if ((_gameEngine.StaticVariables.PlayerEntity.FrameCounter & 0xf) == 0)
                {
                    _gameEngine.SoundManager.PlaySoundEffect((uint)_gameEngine.StaticVariables.SHORT_ARRAY_800227f4[_gameEngine.StaticVariables.PlayerEntity.Slope_18c]);
                }
                goto SkipEffects;

            case (int)PlayerAnimation.Sprint:
                effectEntityId = 0;
                if ((_gameEngine.StaticVariables.PlayerEntity.FrameCounter & 7) == 0)
                {
                    _gameEngine.SoundManager.PlaySoundEffect((uint)_gameEngine.StaticVariables.g_hitSoundEffects[_gameEngine.StaticVariables.PlayerEntity.Slope_18c]);
                }
                goto CaseEffect;

            case (int)PlayerAnimation.SprintDash:
            case (int)PlayerAnimation.StopSprint:
                effectEntityId = 1;

                CaseEffect:
                if (IsSlopeInAquaticTile())
                {
                    animIndex = 40;
                    effectId = 6;
                }
                else
                {
                    animIndex = 8;
                    effectId = _gameEngine.CurrentMap.Info.C;
                }

                if ((_gameEngine.StaticVariables.g_hitSoundEffects[animIndex /*+ 0x34*/] & _gameEngine.StaticVariables.PlayerEntity.FrameCounter) != 0)
                {
                    goto SkipEffects;
                }

                if (effectId != 0)
                {
                    spriteEffect = _gameEngine.EffectManager.CreateEffectEntity(
                        0,
                        effectId,
                        0,
                        _gameEngine.StaticVariables.PlayerEntity.PosX,
                        _gameEngine.StaticVariables.PlayerEntity.PosY,
                        _gameEngine.StaticVariables.PlayerEntity.FloorHeight);
                }

                // Si l'effet a été créé, lui donner une force proportionnelle à celle du joueur
                if (spriteEffect != null)
                {
                    int forceMult = _gameEngine.StaticVariables.g_hitSoundEffects[effectEntityId * 2 + animIndex/*+ 0x36*/];
                    spriteEffect.ForceX = (_gameEngine.StaticVariables.PlayerEntity.ForceX * forceMult) >> 8;
                    spriteEffect.ForceY = (_gameEngine.StaticVariables.PlayerEntity.ForceY * forceMult) >> 8;

                    // Ajouter une composante aléatoire à la force verticale
                    _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    int zOffset = (int)(((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * (ulong)(_gameEngine.StaticVariables.g_hitSoundEffects[animIndex + 0x3a] + 1)) >> 32);
                    spriteEffect.ForceZ = _gameEngine.StaticVariables.g_hitSoundEffects[animIndex + 0x3c] + zOffset;
                }
                break;

            case (int)PlayerAnimation.InSandMoving:
                if ((_gameEngine.StaticVariables.PlayerEntity.FrameCounter & 7) != 0)
                {
                    goto SkipEffects;
                }

                goto CaseRandomEffect;

            case (int)PlayerAnimation.InSandDash:
                if ((_gameEngine.StaticVariables.PlayerEntity.FrameCounter & 3) != 0)
                {
                    goto SkipEffects;
                }

                CaseRandomEffect:
                spriteEffect = _gameEngine.EffectManager.CreateEffectEntity(
                    0,
                    (byte)_gameEngine.CurrentMap.Info._10,
                    0,
                    _gameEngine.StaticVariables.PlayerEntity.PosX,
                    _gameEngine.StaticVariables.PlayerEntity.PosY,
                    _gameEngine.StaticVariables.PlayerEntity.TerrainHeight);

                if (spriteEffect != null)
                {
                    _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                    var randomSeed1 = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed;

                    _gameEngine.StaticVariables.g_gameRandomSeed = (uint)(randomSeed1 * 0x7d2b89dd + 0xe06a02e7);
                    var randomSeed2 = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed;

                    _gameEngine.StaticVariables.g_gameRandomSeed = (uint)(randomSeed2 * 0x7d2b89dd + 0xe06a02e7);

                    // Appliquer les offsets aléatoires à la position de l'effet
                    spriteEffect.X += -0xc0000 + (int)((randomSeed1 * 0x180001) >> 32);
                    spriteEffect.Y += -0x80000 + (int)((randomSeed2 * 0x100001) >> 32);
                    spriteEffect.ForceZ += 0x10000 + (int)(((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x10001) >> 32);
                }
                break;

            case (int)PlayerAnimation.SwimmingDash:
                if ((_gameEngine.StaticVariables.PlayerEntity.FrameCounter & 7) != 0)
                {
                    goto SkipEffects;
                }

                spriteEffect = _gameEngine.EffectManager.CreateEffectEntity(
                    0,
                    6,
                    0,
                    _gameEngine.StaticVariables.PlayerEntity.PosX,
                    _gameEngine.StaticVariables.PlayerEntity.PosY,
                    _gameEngine.StaticVariables.PlayerEntity.TerrainHeight);

                if (spriteEffect == null)
                {
                    goto SkipEffects;
                }

                // Calculer les forces opposées au mouvement du joueur (effet de friction)
                var effectXForce = -_gameEngine.StaticVariables.PlayerEntity.ForceX;
                if (_gameEngine.StaticVariables.PlayerEntity.ForceX > 0)
                {
                    effectXForce += 3;
                }

                spriteEffect.ForceX = effectXForce >> 2;

                var effectYForce = -_gameEngine.StaticVariables.PlayerEntity.ForceY;
                if (_gameEngine.StaticVariables.PlayerEntity.ForceY > 0)
                {
                    effectYForce += 3;
                }

                spriteEffect.ForceY = effectYForce >> 2;
                break;
        }

        SkipEffects:
        switch (_gameEngine.StaticVariables.PlayerEntity.TargetAnimationId)
        {
            case (int)PlayerAnimation.StartJumpWhileMoving:
            case (int)PlayerAnimation.StartJumpWithObjectWhileMoving:
            case (int)PlayerAnimation.ThrowObjectWhileJumping:
            case (int)PlayerAnimation.Reserved0B:
            case (int)PlayerAnimation.JumpAttackSwordDagger:
            case (int)PlayerAnimation.JumpAttackWandIceFire:
            case (int)PlayerAnimation.JumpAttackFlailIron:
            case (int)PlayerAnimation.JumpAttackBowHunterWillow:
            case (int)PlayerAnimation.Reserved1F:
            case (int)PlayerAnimation.Reserved26:
            case (int)PlayerAnimation.StartJump:
            case (int)PlayerAnimation.JumpMoving:
            case (int)PlayerAnimation.Jump:
            case (int)PlayerAnimation.StartJumpWithObject:
            case (int)PlayerAnimation.JumpMovingWithObject:
            case (int)PlayerAnimation.JumpWithObject:
            case (int)PlayerAnimation.JumpAttackSword:
            case (int)PlayerAnimation.JumpAttackFlailSteel:
            case (int)PlayerAnimation.JumpAttackSwordFiendBlade:
            case (int)PlayerAnimation.JumpAttackSwordHoly:
                // Si on est en mode normal (mode == 0) et que le joueur est au sol
                if ((mode == 0 || _gameEngine.StaticVariables.DAT_80098f30 == 0)
                    && _gameEngine.StaticVariables.PlayerEntity.IsAboveGround != 0
                    && _gameEngine.StaticVariables.PlayerEntity.ForceZ < 1)
                {
                    // Jouer un son d'atterrissage
                    _gameEngine.SoundManager.PlaySoundEffect((uint)_gameEngine.StaticVariables.g_hitSfxIdByTileSlope[_gameEngine.StaticVariables.PlayerEntity.Slope_18c]);

                    // Créer des effets visuels d'atterrissage en fonction du type de terrain
                    if (_gameEngine.StaticVariables.PlayerEntity.Slope_18c < 1 || (2 < _gameEngine.StaticVariables.PlayerEntity.Slope_18c && _gameEngine.StaticVariables.PlayerEntity.Slope_18c != 4))
                    {
                        for (var i = 0; i < 3; i++)
                        {
                            if (_gameEngine.CurrentMap.Info._10 != 0)
                            {
                                entityCreated = _gameEngine.EffectManager.CreateEffectEntity(
                                    0,
                                    (byte)_gameEngine.CurrentMap.Info._10,
                                    0,
                                    _gameEngine.StaticVariables.PlayerEntity.PosX,
                                    _gameEngine.StaticVariables.PlayerEntity.PosY,
                                    _gameEngine.StaticVariables.PlayerEntity.TerrainHeight);

                                if (entityCreated != null)
                                {
                                    _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                                    entityCreated.ForceX = (int)(((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x30001) >> 32) - 0x18000;

                                    _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                                    entityCreated.ForceY = (int)(((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x20001) >> 32) - 0x10000;
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
                            _gameEngine.StaticVariables.PlayerEntity.PosX,
                            _gameEngine.StaticVariables.PlayerEntity.PosY,
                            _gameEngine.StaticVariables.PlayerEntity.TerrainHeight);
                    }

                    _gameEngine.StaticVariables.DAT_80098f30 = 1;
                }
                break;

            default:
                _gameEngine.StaticVariables.DAT_80098f30 = 0;
                break;
        }

        if (mode == 0)
        {
            // heartBeat
            if (_gameEngine.StaticVariables.PlayerEntity.Hp != 0 &&
                _gameEngine.StaticVariables.PlayerEntity.Hp * 5 <= _gameEngine.StaticVariables.PlayerEntity.HpMax)
            {
                _gameEngine.StaticVariables.DAT_80098f2c--;

                if (_gameEngine.StaticVariables.DAT_80098f2c == -1)
                {
                    _gameEngine.SoundManager.PlaySoundEffect(400);
                    _gameEngine.StaticVariables.DAT_80098f2c = 0x2d; // Réinitialiser le compteur
                }
            }

            var attackType = -1;

            if (_gameEngine.StaticVariables.PlayerEntity.TargetAnimationId == (int)PlayerAnimation.JumpAttackFlailIron)
            {
                attackType = 0;
            }
            else if (_gameEngine.StaticVariables.PlayerEntity.TargetAnimationId == (int)PlayerAnimation.AttackFlailIron)
            {
                attackType = 0;
            }
            else if (_gameEngine.StaticVariables.PlayerEntity.TargetAnimationId == (int)PlayerAnimation.AttackFlailSteel)
            {
                attackType = 1;
            }
            else if (_gameEngine.StaticVariables.PlayerEntity.TargetAnimationId == (int)PlayerAnimation.JumpAttackFlailSteel)
            {
                attackType = 1;
            }
            else
            {
                return;
            }

            if (_gameEngine.StaticVariables.PlayerEntity.HitCounter == 0)
            {
                if (_gameEngine.StaticVariables.PlayerEntity.FrameCollision != null)
                {
                    var worldXCoords = new int[4];
                    var worldYCoords = new int[4];

                    worldXCoords[0] = _gameEngine.StaticVariables.g_tileToWorldXTable[_gameEngine.StaticVariables.PlayerEntity.HitBoxX >> 16];
                    worldXCoords[2] = worldXCoords[0];
                    worldXCoords[1] = _gameEngine.StaticVariables.g_tileToWorldXTable[(_gameEngine.StaticVariables.PlayerEntity.HitBoxX + _gameEngine.StaticVariables.PlayerEntity.CollisionWidth) >> 16];
                    worldXCoords[3] = worldXCoords[1];

                    worldYCoords[0] = _gameEngine.StaticVariables.PlayerEntity.HitBoxY >> 20;
                    worldYCoords[1] = worldYCoords[0];
                    worldYCoords[2] = (_gameEngine.StaticVariables.PlayerEntity.HitBoxY + _gameEngine.StaticVariables.PlayerEntity.CollisionDepth) >> 20;
                    worldYCoords[3] = worldYCoords[2];

                    for (var i = 0; i < 4; i++)
                    {
                        var tileX = worldXCoords[i];
                        if (tileX < 1)
                        {
                            tileX = 0;
                        }
                        else if (tileX > 0x33)
                        {
                            tileX = 0x33;
                        }

                        var tileY = worldYCoords[i];
                        if (tileY < 1)
                        {
                            tileY = 0;
                        }
                        else if (tileY > 0x3B)
                        {
                            tileY = 0x3B;
                        }

                        var mapWidth = _gameEngine.CurrentMap.Map.Width;
                        var tile = _gameEngine.CurrentMap.Map.MapTiles[tileY * mapWidth + tileX];

                        if ((tile.Walkability & 2) != 0)
                        {
                            // Vérifier si la hauteur de la boîte de collision croise la hauteur de l'effet
                            var tileEffectZ = (tile.Height & 0xFF) << 20;

                            if (_gameEngine.StaticVariables.PlayerEntity.HitBoxZ <= tileEffectZ + 0x80000 &&
                                tileEffectZ + 0x80000 <= _gameEngine.StaticVariables.PlayerEntity.HitBoxZ + _gameEngine.StaticVariables.PlayerEntity.CollisionHeight)
                            {
                                // Désactiver l'effet pour éviter de le déclencher plusieurs fois
                                //TODO
                                //tile.GroundProperty &= 0xFFFD;
                                //tile.Height = 0xFFFF;
                                Debugger.Break();

                                var effectX = worldXCoords[i] * 0x180000 + 0xC0000;
                                var effectY = worldYCoords[i] * 0x100000 + 0x80000;

                                _gameEngine.EffectManager.CreateEffectEntity(
                                    0,
                                    _gameEngine.CurrentMap.Info.F,
                                    0,
                                    effectX,
                                    effectY,
                                    tileEffectZ);
                                _gameEngine.EffectManager.RandomlySpawnItem(0xFF, effectX, effectY, tileEffectZ);
                                _gameEngine.SoundManager.PlaySoundEffect(0x1F);
                            }
                        }
                    }
                }
            }
            else
            {
                if (_gameEngine.StaticVariables.PlayerEntity.IsAboveGround == 0)
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = _gameEngine.StaticVariables.g_flailHitAnimations[attackType * 2 + 1];
                }
                else
                {
                    _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = _gameEngine.StaticVariables.g_flailHitAnimations[attackType * 2];
                }
            }
        }
    }

    private bool IsSlopeInAquaticTile()
    {
        return (_gameEngine.StaticVariables.PlayerEntity.Slope_18c >= 1
                && _gameEngine.StaticVariables.PlayerEntity.Slope_18c <= 2)
               || _gameEngine.StaticVariables.PlayerEntity.Slope_18c == 4;
    }

    // 800350c0
    public void AnimateWarpEffect()
    {
        if (_gameEngine.StaticVariables.g_warpLockTimer != 0)
        {
            switch (_gameEngine.StaticVariables.g_warpLockTimer)
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

            _gameEngine.StaticVariables.g_warpLockTimer = 0;
        }
    }

    //80034ec4
    private void FUN_80034ec4()
    {
        Debugger.Break();
    }

    //80034e08
    private void FUN_80034e08()
    {
        Debugger.Break();
    }

    //80034d2c
    private void FUN_80034d2c()
    {
        Debugger.Break();
    }

    //80034c54
    private void FUN_80034c54()
    {
        Debugger.Break();
    }

    //80034bdc
    private void FUN_80034bdc()
    {
        Debugger.Break();
    }

    //80034b54
    private void FUN_80034b54()
    {
        Debugger.Break();
    }

    //80034acc
    private void FUN_80034acc()
    {
        Debugger.Break();
    }

    //8004df68
    public int GetMoney()
    {
        return _gameEngine.StaticVariables.g_playerStats.MoneyAmount;
    }

    //8004e004
    public void SpendMoney(int amount)
    {
        SetMoney((short)(_gameEngine.StaticVariables.g_playerStats.MoneyAmount - amount));
    }

    //8004dfd8
    public void AddMoney(int amount)
    {
        SetMoney((short)(amount + _gameEngine.StaticVariables.g_playerStats.MoneyAmount));
    }

    //80034108
    public int HandleMapTriggerCommand(int itemId)
    {
        int result;
        int moneyAmount;

        switch (itemId)
        {
            case 0:
                result = 0;
                break;

            case 0x45:
                moneyAmount = 1;
                goto ApplyFadeShortcut;

            case 0x46:
                moneyAmount = 5;
                goto ApplyFadeShortcut;

            case 0x47:
                moneyAmount = 10;
                goto ApplyFadeShortcut;

            case 0x48:
                moneyAmount = 0x1e;
                ApplyFadeShortcut:
                AddMoney(moneyAmount);
                result = 1;
                break;

            case 0x4f:
                IncreaseFalcon2(1);
                result = 1;
                break;

            case 0x50:
                IncreaseMpMaxAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity);
                result = 1;
                break;

            case 0x51:
                IncreaseMpAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity);
                result = 1;
                break;

            case 0x52:
                RestoreMpAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity);
                result = 1;
                break;

            case 0x53:
                IncreaseHpMaxAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity);
                result = 1;
                break;

            case 0x54:
                AddLifeToEntity(_gameEngine.StaticVariables.PlayerEntity);
                result = 1;
                break;

            case 0x55:
                AddLowHpAndSpawnEffect(_gameEngine.StaticVariables.PlayerEntity);
                result = 1;
                break;

            case 0x56:
                AddMediumHpAndSpawnEffect(_gameEngine.StaticVariables.PlayerEntity);
                result = 1;
                break;

            default:
                result = IsMapRequirementMet(itemId) ? 1 : 0;
                break;
        }

        return result;
    }

    //80033ec8
    private bool IsMapRequirementMet(int itemId)
    {
        int currentProgress;
        int requiredProgress;

        currentProgress = GetNumberOfItem(itemId);
        requiredProgress = AddOneItemIfUnlocked(itemId);
        return currentProgress < requiredProgress;
    }

    //8004e428
    public int GetNumberOfItem(int itemId)
    {
        int nbItem;

        if (itemId < 0 || _gameEngine.StaticVariables.g_itemsCount <= itemId)
        {
            //LogDebugMessage(_gameEngine.StaticVariables.g_buffer_isMapUnlocked, itemId);
            nbItem = 0;
        }
        else
        {
            nbItem = _gameEngine.StaticVariables.g_numberOfItems[itemId * 2 + 1];
        }

        return nbItem;
    }



    // 8004e0f8
    public uint SetItemIdFromCurrentItemId()
    {
        var currentItemId = _gameEngine.StaticVariables.g_playerStats.ItemId;
        var numberOfItem = GetNumberOfItem(currentItemId);

        if (numberOfItem == 0)
        {
            return 0xffffffff;
        }

        var slotId = _gameEngine.StaticVariables.g_itemsProperties[currentItemId * 5];
        var itemId = GetItemIdFromSlotId((uint)slotId);
        SetCurrentItemId(itemId);

        return (uint)currentItemId;
    }


    // 8004e18c
    public uint GetItemIdFromSlotId(uint slotId)
    {
        if (slotId >= 0x20)
        {
            Debugger.Break();
            Debug.WriteLine($"Invalid slotId: {slotId}");
            return 0xFFFFFFFF;
        }

        uint bestMatchIndex = 0xFFFFFFFF;
        uint currentIndex = 0;

        while (currentIndex < 99)//0x80)
        {
            var entrySectionId = _gameEngine.StaticVariables.g_itemsProperties[currentIndex * 5];

            if (entrySectionId == slotId)
            {
                var usageCount = _gameEngine.StaticVariables.g_numberOfItems[currentIndex * 2 + 1];

                if (usageCount > 0)
                {
                    if (bestMatchIndex == 0xFFFFFFFF)
                    {
                        bestMatchIndex = currentIndex;

                        var flags = _gameEngine.StaticVariables.g_itemsProperties[currentIndex * 5 + 1];
                        if ((flags & 0x1) == 0)
                        {
                            return currentIndex;
                        }
                    }
                    else
                    {
                        var currentPriority = _gameEngine.StaticVariables.g_itemsProperties[currentIndex * 5 + 2];
                        var bestPriority = _gameEngine.StaticVariables.g_itemsProperties[bestMatchIndex * 5 + 2];

                        if (bestPriority < currentPriority)
                        {
                            bestMatchIndex = currentIndex;
                        }
                    }
                }
            }

            currentIndex++;
        }

        return bestMatchIndex;
    }

    // 8004e4d8
    public void SetCurrentItemId(uint itemId)
    {
        if ((int)itemId < 0 || itemId >= _gameEngine.StaticVariables.g_itemsCount)
        {
            Debugger.Break();
            return;
        }

        _gameEngine.StaticVariables.g_playerStats.ItemId = (short)itemId;
    }

    public uint GetWeaponIdBySlotId(int slotId)
    {
        var itemId = 0xffffffff;

        switch (slotId)
        {
            case 0:
                itemId = GetWeaponIdFromSlot1();
                break;
            case 1:
                itemId = GetWeaponIdFromSlot2();
                break;
            case 2:
                itemId = GetWeaponIdFromSlot3();
                break;
            case 3:
                itemId = GetWeaponIdFromSlot4();
                break;
            case 4:
                itemId = GetWeaponIdFromSlot5();
                break;
            case 5:
                itemId = GetWeaponIdFromSlot6();
                break;
        }

        return itemId;
    }

    // 8004e030
    public uint GetItemIdFromCurrentWeapon()
    {
        return GetWeaponIdBySlotId(_gameEngine.StaticVariables.g_playerStats.WeaponId - 1);
    }

    public uint GetWeaponIdFromSlot1()
    {
        return GetItemIdFromSlotId(1);
    }

    public uint GetWeaponIdFromSlot2()
    {
        return GetItemIdFromSlotId(2);
    }

    public uint GetWeaponIdFromSlot3()
    {
        return GetItemIdFromSlotId(3);
    }

    public uint GetWeaponIdFromSlot4()
    {
        return GetItemIdFromSlotId(4);
    }

    public uint GetWeaponIdFromSlot5()
    {
        return GetItemIdFromSlotId(5);
    }

    private uint GetWeaponIdFromSlot6()
    {
        return GetItemIdFromSlotId(6);
    }


    // 80033dbc
    public void FUN_80033dbc(Entity entity, uint itemId)
    {
        switch (itemId)
        {
            case 0x45:
                AddMoney(1);
                break;
            case 0x46:
                AddMoney(5);
                break;
            case 0x47:
                AddMoney(10);
                break;
            case 0x48:
                AddMoney(0x1e);
                break;
            case 0x4f:
                IncreaseFalcon2(1);
                break;
            case 0x50:
                IncreaseMpMaxAndCreateEffect(entity);
                break;
            case 0x51:
                IncreaseMpAndCreateEffect(entity);
                break;
            case 0x52:
                RestoreMpAndCreateEffect(entity);
                break;
            case 0x53:
                IncreaseHpMaxAndCreateEffect(entity);
                break;
            case 0x54:
                AddLifeToEntity(entity);
                break;
            case 0x55:
                AddLowHpAndSpawnEffect(entity);
                break;
            case 0x56:
                AddMediumHpAndSpawnEffect(entity);
                break;
            default:
                AddOneItemIfUnlocked((int)itemId);
                break;
        }
    }

    //8004e6ec
    private void IncreaseFalcon2(short amount)
    {
        var playerStats = _gameEngine.StaticVariables.g_playerStats;
        short number = (short)(_gameEngine.StaticVariables.g_playerStats.FalconTemp + amount);
        _gameEngine.StaticVariables.g_playerStats.FalconTemp = number;

        if (0x32 < number)
        {
            playerStats.FalconTemp = 0x32;
        }

        _gameEngine.StaticVariables.g_progressStateFlags |= 0x400;
    }

    //80032e2c
    private void AddLifeToEntity(Entity entity)
    {
        var newHp = entity.Hp + 2;

        if (entity.HpMax < newHp)
        {
            newHp = entity.HpMax;
        }

        entity.Hp = newHp;
    }

    //80032eec
    public void AddLowHpAndSpawnEffect(Entity entity)
    {
        int amountHp;
        SpriteEffect effect;
        int hpMax;

        hpMax = entity.HpMax;
        amountHp = hpMax;

        if (hpMax < 0)
        {
            amountHp = hpMax + 3;
        }
        amountHp >>= 2;

        if (amountHp < 1)
        {
            amountHp = 1;
        }

        amountHp = entity.Hp + amountHp;

        if (hpMax < amountHp)
        {
            amountHp = hpMax;
        }

        entity.Hp = amountHp;
        effect = _gameEngine.EffectManager.CreateEffectEntity(0, 0xe, 0,
            entity.PosX,
            entity.PosY,
            entity.PosZ + 0x100000);

        if (effect != null)
        {
            effect.ForceZ = 0x20000;
        }
    }

    //80032f84
    public void AddMediumHpAndSpawnEffect(Entity entity)
    {
        SpriteEffect effect;
        ulong rand1;
        int hpMax;
        int vx;
        ulong rand2;

        hpMax = entity.HpMax;
        var hp = hpMax / 2;

        if (hp < 5)
        {
            hp = 5;
        }

        hp = entity.Hp + hp;

        if (hpMax < hp)
        {
            hp = hpMax;
        }

        entity.Hp = hp;

        var i = 0;

        do
        {
            effect = _gameEngine.EffectManager.CreateEffectEntity(0, 0xe, 0,
                entity.PosX,
                entity.PosY,
                entity.PosZ + 0x100000);

            if (effect != null)
            {
                rand1 = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                _gameEngine.StaticVariables.g_gameRandomSeed = (uint)(rand1 * 0x7d2b89dd + 0xe06a02e7);
                rand2 = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed;
                vx = (int)((ulong)rand1 * 0x20001 >> 0x20);

                effect.ForceX = vx + -0x10000;
                effect.ForceY = (int)(rand2 * 0x30001 >> 0x20) + -0x18000;
                effect.ForceZ = 0x20000;
            }

            i += 1;

        } while (i < 4);
    }

    // 8004e530
    public int AddOneItemIfUnlocked(int itemId)
    {
        if (itemId < 0 || itemId >= _gameEngine.StaticVariables.g_itemsCount)
        {
            Debugger.Break();
            Debug.WriteLine("Invalid itemId in AddOneItemIfUnlocked");
            return 0;
        }

        int itemIdIndex = itemId * 2; // In the assembly: itemIdIndex = (itemId * 4) + g_numberOfItems
        short currentUsage = _gameEngine.StaticVariables.g_numberOfItems[itemIdIndex + 1];
        int itemPropertyId = itemId * 5;
        short unlockRequirement = _gameEngine.StaticVariables.g_itemsProperties[itemPropertyId + 3];

        if (currentUsage != unlockRequirement)
        {
            _gameEngine.StaticVariables.g_numberOfItems[itemIdIndex + 1] = (short)(currentUsage + 1);
            return currentUsage + 1;
        }

        return itemId;
    }

    //8003382c
    public void IncreaseMpMaxAndCreateEffect(Entity entity)
    {
        ulong uVar1;
        int effectParams;
        SpriteEffect pEffect;
        SpriteEffect pEffect2;
        ulong rand;
        int index;
        short offsetX;
        short offsetZ;

        IncreaseMpMax(1);
        index = 0;
        SetPlayerMp((short)GetPlayerMpMax());
        _gameEngine.SoundManager.PlaySoundEffect(0x36);

        do
        {
            pEffect = _gameEngine.EffectManager.CreateEffectEntity(
                0, 0xe, 2, 
                entity.PosX, entity.PosY, entity.PosZ + 0x100000);

            if (pEffect != null)
            {
                offsetX = _gameEngine.StaticVariables.g_offsetXList[index * 4];
                offsetZ = _gameEngine.StaticVariables.g_offsetYList[index * 4];
                pEffect.X += offsetX * 0x2000;
                pEffect.Y += offsetZ * 0x2000;
                pEffect.ForceX = offsetX * -0x200;
                pEffect.ForceY = offsetZ * -0x200;
            }

            index += 1;

        } while (index < 8);

        effectParams = 0;

        do
        {
            pEffect2 = _gameEngine.EffectManager.CreateEffectEntity(
                0, 0xe, 2, 
                entity.PosX, entity.PosY, entity.PosZ + 0x100000);
            
            if (pEffect2 != null)
            {
                rand = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                _gameEngine.StaticVariables.g_gameRandomSeed = (uint)(rand * 0x7d2b89dd + 0xe06a02e7);
                uVar1 = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed;
                pEffect2.ForceX = ((int)(rand * 0x20001) >> 0x20) + -0x10000;
                pEffect2.ForceY = (int)((uVar1 * 0x30001) >> 0x20) + -0x18000;
                pEffect2.ForceZ = 0x40000;
            }

            effectParams += 1;

        } while (effectParams < 4);
    }

    //800335ac
    public void IncreaseMpAndCreateEffect(Entity entity)
    {
        SpriteEffect effect;
        ulong nextSeed;
        int i;
        int randOffsetX;
        ulong randProductZ;

        IncreaseMp(1);
        i = 0;

        do
        {
            effect = _gameEngine.EffectManager.CreateEffectEntity(
                0, 0xe, 2, 
                entity.PosX, entity.PosY, entity.PosZ + 0x100000);

            if (effect != null)
            {
                nextSeed = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                _gameEngine.StaticVariables.g_gameRandomSeed = (uint)(nextSeed * 0x7d2b89dd + 0xe06a02e7);
                randProductZ = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed;
                randOffsetX = (int)((ulong)(nextSeed * 0x20001) >> 0x20);

                effect.ForceX = randOffsetX + -0x10000;
                effect.ForceY = (int)((randProductZ * 0x30001) >> 0x20) + -0x18000;
                effect.ForceZ = 0x20000;
            }

            i += 1;

        } while (i < 4);
    }

    //800336f0
    public void RestoreMpAndCreateEffect(Entity entity)
    {
        int index;
        SpriteEffect pEffect;
        uint angleIndex;

        SetPlayerMp((short)GetPlayerMpMax());
        index = 0;
        angleIndex = 8;

        do
        {
            pEffect = _gameEngine.EffectManager.CreateEffectEntity(
                0, 0xe, 2, 
                entity.PosX, 
                entity.PosY,
                entity.PosZ + 0x80000);

            if (pEffect != null)
            {
                pEffect.X += _gameEngine.StaticVariables.g_offsetXList[index * 4] * 0x800;
                pEffect.Y += _gameEngine.StaticVariables.g_offsetYList[index * 4] * 0x800;
                pEffect.ForceX = _gameEngine.StaticVariables.g_offsetXList[angleIndex & 0x1f] * 0x1c0;
                pEffect.ForceY = _gameEngine.StaticVariables.g_offsetYList[angleIndex & 0x1f] * 0x1c0;
                pEffect.ForceZ = 0x30000;
            }

            angleIndex += 4;
            index += 1;

        } while (index < 8);
    }

    //800333ac
    public void IncreaseHpMaxAndCreateEffect(Entity entity)
    {
        SpriteEffect pEffect;
        SpriteEffect sparkEffect;
        ulong randomSeed;
        int j;
        int i;
        int randomXPart;
        short offsetX;
        short offsetZ;
        ulong randomZPart;

        j = 0;
        entity.HpMax += 1;
        entity.Hp = entity.HpMax;
        _gameEngine.SoundManager.PlaySoundEffect(0x31);

        do
        {
            pEffect = _gameEngine.EffectManager.CreateEffectEntity(
                0, 0xe, 0, 
                entity.PosX, entity.PosY, entity.PosZ + 0x100000);

            if (pEffect != null)
            {
                offsetX = _gameEngine.StaticVariables.g_offsetXList[j * 4];
                offsetZ = _gameEngine.StaticVariables.g_offsetYList[j * 4];
                pEffect.X += offsetX * 0x2000;
                pEffect.Y += offsetZ * 0x2000;
                pEffect.ForceX = offsetX * -0x200;
                pEffect.ForceY = offsetZ * -0x200;
            }

            j += 1;

        } while (j < 8);

        i = 0;

        do
        {
            sparkEffect = _gameEngine.EffectManager.CreateEffectEntity(
                0, 0xe, 0,
                entity.PosX, entity.PosY, entity.PosZ + 0x100000);

            if (sparkEffect != null)
            {
                randomSeed = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                _gameEngine.StaticVariables.g_gameRandomSeed = (uint)(randomSeed * 0x7d2b89dd + 0xe06a02e7);
                randomXPart = (int)((ulong)randomSeed * 0x20001 >> 0x20);
                randomZPart = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed;

                sparkEffect.ForceX = randomXPart + -0x10000;
                sparkEffect.ForceY = (int)((randomZPart * 0x30001) >> 0x20) + -0x18000;
                sparkEffect.ForceZ = 0x40000;
            }

            i += 1;

        } while (i < 4);
    }

    //80032e50
    public void IncreaseHpAndCreateEffect(Entity entity)
    {
        int newHp;
        SpriteEffect effect;
        int hpMax;

        hpMax = entity.HpMax;
        newHp = hpMax;

        if (hpMax < 0)
        {
            newHp = hpMax + 3;
        }

        newHp >>= 2;

        if (newHp < 5)
        {
            newHp = 5;
        }

        newHp = entity.Hp + newHp;

        if (hpMax < newHp)
        {
            newHp = hpMax;
        }
        entity.Hp = newHp;
        effect = _gameEngine.EffectManager.CreateEffectEntity(
            0, 0xe, 0, 
            entity.PosX, entity.PosY, entity.PosZ + 0x100000);

        if (effect != null)
        {
            effect.ForceZ = 0x20000;
        }
    }

    //80033274
    public void RestoreHpAndCreateEffect(Entity entity)
    {
        SpriteEffect effect;
        uint indexMask;
        int i;

        i = 0;
        indexMask = 8;
        entity.Hp = entity.HpMax;

        do
        {
            effect = _gameEngine.EffectManager.CreateEffectEntity(
                0, 0xe, 0,
                entity.PosX, entity.PosY, entity.PosZ + 0x80000);

            if (effect != null)
            {
                var offsetXNext = _gameEngine.StaticVariables.g_offsetXList[indexMask & 0x1f];
                var offsetYNext = _gameEngine.StaticVariables.g_offsetYList[indexMask & 0x1f];

                effect.X += offsetXNext * 0x800;
                effect.Y += offsetYNext * 0x800;
                effect.ForceX = offsetXNext * 0x1c0;
                effect.ForceY = offsetYNext * 0x1c0;
                effect.ForceZ = 0x30000;
            }

            indexMask += 4;
            i += 1;

        } while (i < 8);
    }

    //80033a2c
    public void RestoreHpAndMpAndCreateEffect(Entity entity)
    {
        ulong rand;
        SpriteEffect effect;
        SpriteEffect effect2;
        SpriteEffect effect3;
        int i;

        i = 0;
        entity.Hp = entity.HpMax;
        SetPlayerMp((short)GetPlayerMpMax());

        do
        {
            var animId = (byte)(((i & 1) == 0 ? 1 : 0) << 1);

            effect = _gameEngine.EffectManager.CreateEffectEntity(0, 0xe, animId,
                entity.PosX, entity.PosY, entity.PosZ + 0x80000);

            if (effect != null)
            {
                effect.X += _gameEngine.StaticVariables.g_offsetXList[i * 4] * 0x800;
                effect.Y += _gameEngine.StaticVariables.g_offsetYList[i * 4] * 0x800;
                effect.ForceX = _gameEngine.StaticVariables.g_offsetXList[i * 4 + 8 & 0x1f] * 0x1c0;
                effect.ForceY = _gameEngine.StaticVariables.g_offsetYList[i * 4 + 8 & 0x1f] * 0x1c0;
                effect.ForceZ = 0x30000;
            }

            i += 1;

        } while (i < 8);

        i = 0;

        do
        {
            effect2 = _gameEngine.EffectManager.CreateEffectEntity(0, 0xe, 0,
                entity.PosX, entity.PosY, entity.PosZ + 0x100000);

            if (effect2 != null)
            {
                var seed = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                _gameEngine.StaticVariables.g_gameRandomSeed = (uint)(seed * 0x7d2b89dd + 0xe06a02e7);
                rand = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed;

                effect2.ForceZ = 0x40000;
                effect2.ForceX = (int)(seed * 0x20001 >> 0x20) + -0x10000;
                effect2.ForceY = (int)(rand * 0x30001 >> 0x20) + -0x18000;
            }

            effect3 = _gameEngine.EffectManager.CreateEffectEntity(0, 0xe, 2,
                entity.PosX, entity.PosY, entity.PosZ + 0x100000);

            if (effect3 != null)
            {
                var seed = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                _gameEngine.StaticVariables.g_gameRandomSeed = (uint)(seed * 0x7d2b89dd + 0xe06a02e7);
                rand = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed;

                effect3.ForceZ = 0x20000;
                effect3.ForceX = (int)(seed * 0x20001 >> 0x20) + -0x10000;
                effect3.ForceY = (int)(rand * 0x30001 >> 0x20) + -0x18000;
            }

            i += 1;

        } while (i < 4);
    }

    //800330fc
    public void AddHugeHpAndSpawnEffect(Entity entity)
    {
        SpriteEffect effect;
        ulong rand;
        int hpMax;
        int i;
        int calculatedVelocity;
        ulong seed;

        hpMax = entity.HpMax;
        i = hpMax / 2;

        if (i < 10)
        {
            i = 10;
        }

        i = entity.Hp + i;

        if (hpMax < i)
        {
            i = hpMax;
        }

        entity.Hp = i;
        i = 0;

        do
        {
            effect = _gameEngine.EffectManager.CreateEffectEntity(0, 0xe, 0,
                entity.PosX, entity.PosY, entity.PosZ + 0x100000);

            if (effect != null)
            {
                rand = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
                _gameEngine.StaticVariables.g_gameRandomSeed = (uint)(rand * 0x7d2b89dd + 0xe06a02e7);
                seed = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed;
                calculatedVelocity = (int)((ulong)rand * 0x20001 >> 0x20);

                effect.ForceX = calculatedVelocity + -0x10000;
                effect.ForceY = (int)(seed * 0x30001 >> 0x20) + -0x18000;
                effect.ForceZ = 0x20000;
            }

            i = i + 1;

        } while (i < 4);
    }

    //80033f00
    public bool FUN_80033f00(Entity entity, int itemId)
    {
        bool result;

        switch (itemId)
        {
            case 0:
                return false;

            case 0x24:
                result = IsMapRequirementMet(itemId);
                if (result == false)
                {
                    IncreaseHpAndCreateEffect(entity);
                }
                break;

            case 0x25:
                result = IsMapRequirementMet(itemId);
                if (result == false)
                {
                    RestoreHpAndCreateEffect(entity);
                }
                break;

            case 0x26:
                result = IsMapRequirementMet(itemId);
                if (result == false)
                {
                    IncreaseMpAndCreateEffect(entity);
                }
                break;

            case 0x27:
                result = IsMapRequirementMet(itemId);
                if (result == false)
                {
                    RestoreHpAndMpAndCreateEffect(entity);
                }
                break;

            case 0x29:
                result = IsMapRequirementMet(itemId);
                if (result == false)
                {
                    AddHugeHpAndSpawnEffect(entity);
                }
                break;

            case 0x45:
                AddMoney(1);
                break;

            case 0x46:
                AddMoney(5);
                break;

            case 0x47:
                AddMoney(10);
                break;
            case 0x48:

                AddMoney(0x1e);
                break;
            case 0x4f:

                IncreaseFalcon2(1);
                break;

            case 0x50:
                IncreaseMpMaxAndCreateEffect(entity);
                break;

            case 0x51:
                IncreaseMpAndCreateEffect(entity);
                break;

            case 0x52:
                RestoreMpAndCreateEffect(entity);
                break;

            case 0x53:
                IncreaseHpMaxAndCreateEffect(entity);
                break;

            case 0x54:
                AddLifeToEntity(entity);
                break;

            case 0x55:
                AddLowHpAndSpawnEffect(entity);
                break;

            case 0x56:
                AddMediumHpAndSpawnEffect(entity);
                break;

            default:
                AddOneItemIfUnlocked(itemId);
                break;
        }

        return _gameEngine.StaticVariables.g_itemDropProperties[itemId].Field1 == 0;
    }

    //8004e7a4
    public int GetNumberOfFalconTemp()
    {
        return _gameEngine.StaticVariables.g_playerStats.FalconTemp;
    }

    //8004e78c
    public int GetNumberOfFalcon()
    {
        return _gameEngine.StaticVariables.g_playerStats.Falcon;
    }

    //8004e738
    public void UpdateNumberOfFalcon()
    {
        PlayerStats playerStats = _gameEngine.StaticVariables.g_playerStats;
        _gameEngine.StaticVariables.g_playerStats.Falcon += _gameEngine.StaticVariables.g_playerStats.FalconTemp;
        playerStats.FalconTemp = 0;

        if (0x32 < playerStats.Falcon)
        {
            playerStats.Falcon = 0x32;
        }

        _gameEngine.StaticVariables.g_progressStateFlags = (int)(_gameEngine.StaticVariables.g_progressStateFlags & 0xfffffbff);
    }


    //80033d34
    public bool FUN_80033d34(int itemId)
    {
        int num;

        if (itemId < 0x28)
        {
            if (itemId < 0x24)
            {
                if (itemId != 0)
                {
                    return true;
                }
                return false;
            }
        }
        else if (itemId != 0x29)
        {
            return true;
        }

        num = GetNumberOfItem(itemId);
        return num < _gameEngine.StaticVariables.g_itemDropProperties[itemId].Field4;
    }
}