using System.Diagnostics;

namespace AlundraEngine.Gameplay.Scripts;

public static class FunctionTypeD
{
    //8007d9a4
    public static void AI_FUN_8007d9a4(GameEngine gameEngine, Entity entity)
    {
        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 5;
        var direction = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.g_entitySlots[0].PosX, entity.PosY - gameEngine.StaticVariables.g_entitySlots[0].PosY);
        entity.TargetDirection = direction;
    }

    //8007da08
    //◆Homme-lézard (épée) Niv.1
    public static void AI_FUN_8007da08(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Homme-lézard (épée) Niv.1")
        {
            System.Diagnostics.Debugger.Break();
        }

        byte bVar1;
        uint uVar3;
        uint iVar4;

        bVar1 = entity.TouchingEntity.BalanceAnimValRef.Val;
        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX, entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);

        if ((bVar1 & 0xf) - 1 < 3)
        {
            iVar4 = entity.SpriteTableIndex;

            if (iVar4 == 0x16a)
            {
                uVar3 = 0xc;

                if (entity.TargetAnimationId < 2)
                {
                    goto LAB_8007db1c;
                }

                if (entity.TargetAnimationId == 0xd)
                {
                    uVar3 = 0xc;
                    goto LAB_8007db1c;
                }

                iVar4 = entity.SpriteTableIndex;
            }

            if (iVar4 == 0x16d && entity.TargetAnimationId < 2)
            {
                uVar3 = 10;

                if (0x31 < (uint)((Random.Next() * 100) >> 0x20))
                {
                    goto LAB_8007db1c;
                }
            }
        }

        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        uVar3 = 7;

        LAB_8007db1c:
        entity.TargetAnimationId = uVar3;
    }

    //8007db38
    public static void AI_FUN_8007db38(GameEngine gameEngine, Entity entity)
    {
        uint direction;

        if ((entity.SpriteTableIndex == 0x1b5 || entity.SpriteTableIndex == 0x1b9) && entity.TargetAnimationId - 4 < 3)
        {
            entity.TargetAnimationId = 9;
            entity.AIValues[1] = 0x78;
        }
        else
        {
            if (gameEngine.EntityManager.ComputeNewHp(entity))
            {
                entity.Bytes[3] = 1;
            }

            entity.TargetAnimationId = 7;
        }

        direction = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.g_entitySlots[0].PosX, entity.PosY - gameEngine.StaticVariables.g_entitySlots[0].PosY);
        entity.TargetDirection = direction;
    }

    //8007dbe0
    //mimique niveau 1 touch
    public static void AI_FUN_8007dbe0(GameEngine gameEngine, Entity entity)
    {
        bool bVar1;
        uint direction;
        byte val;

        val = entity.TouchingEntity.BalanceAnimValRef.Val;
        direction = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.g_entitySlots[0].PosX, entity.PosY - gameEngine.StaticVariables.g_entitySlots[0].PosY);
        entity.TargetDirection = direction;

        if ((val & 0xf) - 1 < 3 && entity.SpriteTableIndex == 0x1a9 && entity.TargetAnimationId < 2)
        {
            direction = 0xb;

            if (0x3b < (uint)((Random.Next() * 100) >> 0x20))
            {
                goto LAB_8007dcbc;
            }
        }

        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        direction = 5;

        LAB_8007dcbc:
        entity.TargetAnimationId = direction;
    }

    //8007dcd8
    //fish
    public static void AI_FUN_8007dcd8(GameEngine gameEngine, Entity entity)
    {
        //System.Diagnostics.Debugger.Break();

        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 6;
        var direction = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.g_entitySlots[0].PosX, entity.PosY - gameEngine.StaticVariables.g_entitySlots[0].PosY);
        entity.TargetDirection = direction;
    }

    //8007dd3c
    public static void AI_FUN_8007dd3c(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007dda0
    public static void AI_FUN_8007dda0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007de04
    //muruta griffes nv1
    public static void AI_FUN_8007de04(GameEngine gameEngine, Entity entity)
    {
        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 2;
        var direction = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.g_entitySlots[0].PosX, entity.PosY - gameEngine.StaticVariables.g_entitySlots[0].PosY);
        entity.TargetDirection = direction;
    }

    //8007de68
    public static void AI_FUN_8007de68(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007dee8
    public static void AI_FUN_8007dee8(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Guêpe Niv.1")
        {
            Debugger.Break();
        }

        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 4;
        var direction = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX, entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
        entity.TargetDirection = direction;
    }

    //8007df4c
    public static void AI_FUN_8007df4c(GameEngine gameEngine, Entity entity)
    {
        bool bVar1;
        byte val;

        val = entity.TouchingEntity.BalanceAnimValRef.Val;
        var direction = ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.g_entitySlots[0].PosX, entity.PosY - gameEngine.StaticVariables.g_entitySlots[0].PosY);
        entity.TargetDirection = (uint)direction;

        if ((val & 0xf) - 1 < 3)
        {
            var rand = (Random.Next() * 100) >> 0x20;

            if (0x45 < rand)
            {
                if (entity.SpriteTableIndex == 0x152)
                {
                    direction = 6;

                    if (entity.TargetAnimationId < 2)
                    {
                        goto LAB_8007e058;
                    }

                    if (entity.SpriteTableIndex == 0x155)
                    {
                        direction = 6;

                        if (entity.TargetAnimationId < 2)
                        {
                            direction = 6;

                            if (entity.TargetAnimationId == 7)
                            {
                                goto LAB_8007e058;
                            }
                        }
                    }
                }
            }
        }

        bVar1 = gameEngine.EntityManager.ComputeNewHp(entity);

        if (bVar1)
        {
            entity.Bytes[3] = 1;
        }

        direction = 4;

        LAB_8007e058:
        entity.TargetAnimationId = (uint)direction;
    }

    //8007e074
    //muruta arc nv1
    public static void AI_FUN_8007e074(GameEngine gameEngine, Entity entity)
    {
        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 3;
        var direction = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.g_entitySlots[0].PosX, entity.PosY - gameEngine.StaticVariables.g_entitySlots[0].PosY);
        entity.TargetDirection = direction;
    }

    //8007e0d8
    public static void AI_FUN_8007e0d8(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e114
    public static void AI_FUN_8007e114(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e140
    public static void AI_FUN_8007e140(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e1c4
    //Élément Niv.1
    public static void AI_FUN_8007e1c4(GameEngine gameEngine, Entity entity)
    {
        uint direction;

        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 8;
        direction = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.g_entitySlots[0].PosX, entity.PosY - gameEngine.StaticVariables.g_entitySlots[0].PosY);
        entity.TargetDirection = direction;
    }

    //8007e228
    public static void AI_FUN_8007e228(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e2a0
    public static void AI_FUN_8007e2a0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e304
    public static void AI_FUN_8007e304(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e424
    public static void AI_FUN_8007e424(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e548
    public static void AI_FUN_8007e548(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e5b0
    public static void AI_FUN_8007e5b0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e628
    public static void AI_FUN_8007e628(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e68c
    public static void AI_FUN_8007e68c(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e694
    public static void AI_FUN_8007e694(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e704
    public static void AI_FUN_8007e704(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e754
    public static void AI_FUN_8007e754(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e790
    public static void AI_FUN_8007e790(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e79c
    public static void AI_FUN_8007e79c(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e7e4
    public static void AI_FUN_8007e7e4(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e828
    public static void AI_FUN_8007e828(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e86c
    public static void AI_FUN_8007e86c(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e8ac
    public static void AI_FUN_8007e8ac(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e8f0
    //P-Zoldia Niv.1
    public static void AI_FUN_8007e8f0(GameEngine gameEngine, Entity entity)
    {
        if (entity.Bytes[0] == 0)
        {
            if (!gameEngine.EntityManager.ComputeNewHp(entity))
            {
                gameEngine.SoundManager.PlaySoundEffect(0x11e);
            }
            else
            {
                gameEngine.SoundManager.PlaySoundEffect(0x123);
                entity.Bytes[3] = 1;
            }

            entity.TargetAnimationId = 6;
            var direction = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.g_entitySlots[0].PosX, entity.PosY - gameEngine.StaticVariables.g_entitySlots[0].PosY);
            entity.TargetDirection = direction;
        }
        else
        {
            gameEngine.SoundManager.PlaySoundEffect(0x120);
            entity.TargetAnimationId = 4;
        }
    }

    //8007e994
    public static void AI_FUN_8007e994(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e9ac
    public static void AI_FUN_8007e9ac(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007e9e8
    public static void AI_FUN_8007e9e8(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007ea24
    public static void AI_FUN_8007ea24(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007ea84
    public static void AI_FUN_8007ea84(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007eb1c
    public static void AI_FUN_8007eb1c(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007eb58
    public static void AI_FUN_8007eb58(GameEngine gameEngine, Entity entity)
    {
        if (entity.Hp == 0)
        {
            return;
        }

        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 1;
    }

    //8007eba8
    public static void AI_FUN_8007eba8(GameEngine gameEngine, Entity entity)
    {
        entity.ForceZ = 0;
        entity.PreviousAdjustedForceY = 0;
        entity.PreviousAdjustedForceX = 0;
        var bVar1 = gameEngine.EntityManager.ComputeNewHp(entity);

        if (bVar1)
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 6;
    } 

    //8007ebf0
    public static void AI_FUN_8007ebf0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007ec60
    public static void AI_FUN_8007ec60(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007ec9c
    public static void AI_FUN_8007ec9c(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007eda4
    public static void AI_FUN_8007eda4(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007ee00
    public static void AI_FUN_8007ee00(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007ee68
    public static void AI_FUN_8007ee68(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007eedc
    public static void AI_FUN_8007eedc(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007ef58
    public static void AI_FUN_8007ef58(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007efb0
    public static void AI_FUN_8007efb0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f004
    public static void AI_FUN_8007f004(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f07c
    public static void AI_FUN_8007f07c(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f0e0
    public static void AI_FUN_8007f0e0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f148
    public static void AI_FUN_8007f148(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f1b4
    public static void AI_FUN_8007f1b4(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f220
    public static void AI_FUN_8007f220(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f288
    public static void AI_FUN_8007f288(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f2ec
    public static void AI_FUN_8007f2ec(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f360
    public static void AI_FUN_8007f360(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f3bc
    public static void AI_FUN_8007f3bc(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f424
    public static void AI_FUN_8007f424(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f4a8
    public static void AI_FUN_8007f4a8(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f534
    public static void AI_FUN_8007f534(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f5a0
    public static void AI_FUN_8007f5a0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f608
    public static void AI_FUN_8007f608(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f668
    public static void AI_FUN_8007f668(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f6cc
    public static void AI_FUN_8007f6cc(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f748
    public static void AI_FUN_8007f748(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f7a4
    public static void AI_FUN_8007f7a4(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f800
    public static void AI_FUN_8007f800(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f868
    public static void AI_FUN_8007f868(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f8d0
    public static void AI_FUN_8007f8d0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f930
    public static void AI_FUN_8007f930(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007f9a8
    public static void AI_FUN_8007f9a8(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007fa04
    public static void AI_FUN_8007fa04(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007fa60
    public static void AI_FUN_8007fa60(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8007fac8
    public static void AI_FUN_8007fac8(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b1a0
    public static void AI_FUN_8008b1a0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b210
    public static void AI_FUN_8008b210(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b288
    public static void AI_FUN_8008b288(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b300
    public static void AI_FUN_8008b300(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b378
    public static void AI_FUN_8008b378(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b3f0
    public static void AI_FUN_8008b3f0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b448
    public static void AI_FUN_8008b448(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b4a0
    public static void AI_FUN_8008b4a0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b4f8
    public static void AI_FUN_8008b4f8(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b560
    public static void AI_FUN_8008b560(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b5c8
    public static void AI_FUN_8008b5c8(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b630
    public static void AI_FUN_8008b630(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b698
    public static void AI_FUN_8008b698(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b6f0
    public static void AI_FUN_8008b6f0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b748
    public static void AI_FUN_8008b748(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b7b0
    public static void AI_FUN_8008b7b0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b810
    public static void AI_FUN_8008b810(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b878
    public static void AI_FUN_8008b878(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b8e0
    public static void AI_FUN_8008b8e0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b950
    public static void AI_FUN_8008b950(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008b9c8
    public static void AI_FUN_8008b9c8(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008ba40
    public static void AI_FUN_8008ba40(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008baa8
    public static void AI_FUN_8008baa8(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008bb10
    public static void AI_FUN_8008bb10(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008bb78
    public static void AI_FUN_8008bb78(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008bc00
    public static void AI_FUN_8008bc00(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008bc68
    public static void AI_FUN_8008bc68(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008bcd0
    public static void AI_FUN_8008bcd0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008bd38
    public static void AI_FUN_8008bd38(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008bd70
    public static void AI_FUN_8008bd70(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008bd9c
    public static void AI_FUN_8008bd9c(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008be08
    public static void AI_FUN_8008be08(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008be70
    public static void AI_FUN_8008be70(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008beb8
    public static void AI_FUN_8008beb8(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008bf20
    public static void AI_FUN_8008bf20(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008bf7c
    public static void AI_FUN_8008bf7c(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c004
    public static void AI_FUN_8008c004(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c074
    public static void AI_FUN_8008c074(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c0d0
    public static void AI_FUN_8008c0d0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c138
    public static void AI_FUN_8008c138(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c1a0
    public static void AI_FUN_8008c1a0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c218
    public static void AI_FUN_8008c218(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c290
    public static void AI_FUN_8008c290(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c2f0
    public static void AI_FUN_8008c2f0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c358
    public static void AI_FUN_8008c358(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c3c0
    public static void AI_FUN_8008c3c0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c428
    public static void AI_FUN_8008c428(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c498
    public static void AI_FUN_8008c498(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c508
    public static void AI_FUN_8008c508(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c588
    public static void AI_FUN_8008c588(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c600
    public static void AI_FUN_8008c600(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c678
    public static void AI_FUN_8008c678(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c6f0
    public static void AI_FUN_8008c6f0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c748
    public static void AI_FUN_8008c748(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c7b0
    public static void AI_FUN_8008c7b0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c818
    public static void AI_FUN_8008c818(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c890
    public static void AI_FUN_8008c890(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c8f0
    public static void AI_FUN_8008c8f0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c948
    public static void AI_FUN_8008c948(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008c9b8
    public static void AI_FUN_8008c9b8(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008ca30
    public static void AI_FUN_8008ca30(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008ca80
    public static void AI_FUN_8008ca80(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008cb00
    public static void AI_FUN_8008cb00(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008cb80
    public static void AI_FUN_8008cb80(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008cc00
    public static void AI_FUN_8008cc00(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008cc60
    public static void AI_FUN_8008cc60(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008cce0
    public static void AI_FUN_8008cce0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008cd40
    public static void AI_FUN_8008cd40(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008cda0
    public static void AI_FUN_8008cda0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008ce00
    public static void AI_FUN_8008ce00(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008ce40
    public static void AI_FUN_8008ce40(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008ce80
    public static void AI_FUN_8008ce80(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008ced0
    public static void AI_FUN_8008ced0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008cf40
    public static void AI_FUN_8008cf40(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008cfa0
    public static void AI_FUN_8008cfa0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d004
    public static void AI_FUN_8008d004(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d074
    public static void AI_FUN_8008d074(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d0c0
    public static void AI_FUN_8008d0c0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d130
    public static void AI_FUN_8008d130(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d1a0
    public static void AI_FUN_8008d1a0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d218
    public static void AI_FUN_8008d218(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d290
    public static void AI_FUN_8008d290(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d2f0
    public static void AI_FUN_8008d2f0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d458
    public static void AI_FUN_8008d458(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d4a0
    public static void AI_FUN_8008d4a0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d508
    public static void AI_FUN_8008d508(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d578
    public static void AI_FUN_8008d578(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d600
    public static void AI_FUN_8008d600(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d678
    public static void AI_FUN_8008d678(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d6e0
    public static void AI_FUN_8008d6e0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d748
    public static void AI_FUN_8008d748(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d7b0
    public static void AI_FUN_8008d7b0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d818
    public static void AI_FUN_8008d818(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d898
    public static void AI_FUN_8008d898(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d900
    public static void AI_FUN_8008d900(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008d964
    public static void AI_FUN_8008d964(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008da0c
    public static void AI_FUN_8008da0c(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008da60
    public static void AI_FUN_8008da60(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008dac8
    public static void AI_FUN_8008dac8(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008db54
    public static void AI_FUN_8008db54(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008dbbc
    public static void AI_FUN_8008dbbc(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008dc38
    public static void AI_FUN_8008dc38(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008dc70
    public static void AI_FUN_8008dc70(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008dcd8
    public static void AI_FUN_8008dcd8(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008dd44
    public static void AI_FUN_8008dd44(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008ddd0
    public static void AI_FUN_8008ddd0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008de3c
    public static void AI_FUN_8008de3c(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008deec
    public static void AI_FUN_8008deec(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008df54
    public static void AI_FUN_8008df54(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008dfe4
    public static void AI_FUN_8008dfe4(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e054
    public static void AI_FUN_8008e054(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e0bc
    public static void AI_FUN_8008e0bc(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e148
    public static void AI_FUN_8008e148(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e1a0
    public static void AI_FUN_8008e1a0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e228
    public static void AI_FUN_8008e228(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e2b8
    public static void AI_FUN_8008e2b8(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e334
    public static void AI_FUN_8008e334(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e3a4
    public static void AI_FUN_8008e3a4(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e424
    public static void AI_FUN_8008e424(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e4b0
    public static void AI_FUN_8008e4b0(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e54c
    public static void AI_FUN_8008e54c(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e5d8
    public static void AI_FUN_8008e5d8(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e674
    public static void AI_FUN_8008e674(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e6ec
    public static void AI_FUN_8008e6ec(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e764
    public static void AI_FUN_8008e764(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e7dc
    public static void AI_FUN_8008e7dc(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e854
    public static void AI_FUN_8008e854(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e8cc
    public static void AI_FUN_8008e8cc(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008e944
    public static void AI_FUN_8008e944(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008ea3c
    public static void AI_FUN_8008ea3c(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008eaec
    public static void AI_FUN_8008eaec(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008eb74
    public static void AI_FUN_8008eb74(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    //8008ebfc
    public static void AI_FUN_8008ebfc(GameEngine gameEngine, Entity entity)
    {
        System.Diagnostics.Debugger.Break();
    }

    // ...existing code...
}