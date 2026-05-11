using System.Diagnostics;

namespace AlundraEngine.Gameplay.Scripts;

public static class FunctionTypeD
{
    //8007d9a4
    public static void AI_FUN_8007d9a4(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != null
            && entity.Name != "◆Beannoïde"
            && entity.Name != "◆Slime gélatineux")
        {
            Breakpoint.TriggerBreak();
        }


        HitCommon(gameEngine, entity, 5);
    }

    //8007da08
    public static void AI_FUN_8007da08(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Homme-lézard (épée) Niv.1")
        {
            Breakpoint.TriggerBreak();
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
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Tortue de roche Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

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
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Homme momie Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

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
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Ver Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        HitCommon(gameEngine, entity, 6);
    }

    //8007dd3c
    public static void AI_FUN_8007dd3c(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

        HitCommon(gameEngine, entity, 7);
    }

    //8007dda0
    public static void AI_FUN_8007dda0(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Fantôme Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        HitCommon(gameEngine, entity, 4);
    }

    //8007de04
    //muruta griffes nv1
    public static void AI_FUN_8007de04(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

        HitCommon(gameEngine, entity, 2);
    }

    //8007de68
    public static void AI_FUN_8007de68(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆P-Zoldia Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        HitCommon(gameEngine, entity, 6);

        if (entity.SpriteTableIndex - 0x1c4U < 2)
        {
            gameEngine.SoundManager.PlaySoundEffect(0x41);
        }
    }

    //8007dee8
    public static void AI_FUN_8007dee8(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Guêpe Niv.1"
            && entity.Name != "◆Homme de boue Niv.1"
            && entity.Name != "◆Zombie Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 4;
        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX, entity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY); ;
    }

    //8007df4c
    public static void AI_FUN_8007df4c(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Orc (hache) Niv.1"
            && entity.Name != "◆Orc (masse) Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

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
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }


        HitCommon(gameEngine, entity, 3);
    }

    //8007e0d8
    public static void AI_FUN_8007e0d8(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Abyss Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 5;
    }

    //8007e114
    public static void AI_FUN_8007e114(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Abyss (droite)"
            && entity.Name != "◆Abyss (gauche)")
        {
            Breakpoint.TriggerBreak();
        }

        uint animationId;

        if (entity.TargetAnimationId == 2)
        {
            animationId = 9;
        }
        else
        {
            animationId = 3;

            if (entity.TargetAnimationId == 1)
            {
                animationId = 10;
            }
        }

        entity.TargetAnimationId = animationId;
    }

    //8007e140
    public static void AI_FUN_8007e140(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name))
        {
            Breakpoint.TriggerBreak();
        }

        entity.Bytes[2] = 0;

        if (entity.Bytes[1] == 0)
        {
            if (gameEngine.EntityManager.ComputeNewHp(entity))
            {
                entity.Bytes[3] = 1;
            }

            entity.TargetAnimationId = 8;
            entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.g_entitySlots[0].PosX, entity.PosY - gameEngine.StaticVariables.g_entitySlots[0].PosY); ;
        }
        else
        {
            entity.TargetAnimationId = 0x10;
        }
    }

    //8007e1c4
    public static void AI_FUN_8007e1c4(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Élément Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

        HitCommon(gameEngine, entity, 8);
    }

    public static void HitCommon(GameEngine gameEngine, Entity entity, uint animationId)
    {
        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = animationId;
        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.g_entitySlots[0].PosX, entity.PosY - gameEngine.StaticVariables.g_entitySlots[0].PosY);
    }

    //8007e228
    public static void AI_FUN_8007e228(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
            entity.ParentEntity.DelayOrAngle -= 1;
        }

        entity.TargetAnimationId = 2;
        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.g_entitySlots[0].PosX, entity.PosY - gameEngine.StaticVariables.g_entitySlots[0].PosY);
    }

    //8007e2a0
    public static void AI_FUN_8007e2a0(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        HitCommon(gameEngine, entity, 8);
    }

    //8007e304
    public static void AI_FUN_8007e304(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        Entity primaryEntity = entity;
        Entity secondaryEntity;
        int entityRef = entity.AIValues.GetInt32(2);
        int direction;

        if (entityRef != 0)
        {
            secondaryEntity = gameEngine.StaticVariables.g_entitySlots[entityRef];
            direction = ScriptHelper.GetDirectionToTarget(primaryEntity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX, primaryEntity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
            primaryEntity.TargetDirection = (uint)direction;
            secondaryEntity.TargetDirection = (uint)((0x20 - direction) & 0x1f);

            if (gameEngine.EntityManager.ComputeNewHp(primaryEntity))
            {
                primaryEntity.Bytes[3] = 1;
            }

            secondaryEntity.Hp = primaryEntity.Hp;
        }
        else
        {
            secondaryEntity = primaryEntity;
            primaryEntity = secondaryEntity.ParentEntity!;
            direction = ScriptHelper.GetDirectionToTarget(secondaryEntity.PosX - gameEngine.StaticVariables.PlayerEntity.PosX, secondaryEntity.PosY - gameEngine.StaticVariables.PlayerEntity.PosY);
            secondaryEntity.TargetDirection = (uint)direction;
            primaryEntity.TargetDirection = (uint)((0x20 - direction) & 0x1f);

            if (gameEngine.EntityManager.ComputeNewHp(secondaryEntity))
            {
                primaryEntity.Bytes[3] = 1;
            }

            primaryEntity.Hp = secondaryEntity.Hp;
        }

        if (primaryEntity.Bytes[2] == 0)
        {
            primaryEntity.TargetAnimationId = 9;
            secondaryEntity.TargetAnimationId = 6;
        }
        else
        {
            primaryEntity.TargetAnimationId = 6;
            secondaryEntity.TargetAnimationId = 9;
        }
    }

    //8007e424
    public static void AI_FUN_8007e424(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        if (gameEngine.StaticVariables.g_entitySlots[2].Bytes[1] == 5 
            || gameEngine.StaticVariables.g_entitySlots[2].Bytes[1] == 7 
            || gameEngine.StaticVariables.g_entitySlots[2].Bytes[1] == 8 
            || gameEngine.StaticVariables.g_entitySlots[2].Bytes[0] != 0xf)
        {
            if (entity != gameEngine.StaticVariables.g_entitySlots[2])
            {
                return;
            }
        }
        else
        {
            var count = 0;

            if (entity != gameEngine.StaticVariables.g_entitySlots[2])
            {
                if (gameEngine.EntityManager.ComputeNewHp(entity))
                {
                    entity.Bytes[3] = 1;
                }
                entity.TargetAnimationId = 4;
                FUN_80073940(gameEngine, entity);
                return;
            }

            var i = 0;

            do
            {
                if (gameEngine.StaticVariables.g_entitySlots[2 + i].Bytes[3] != 0)
                {
                    count += 1;
                }

                i = i + 1;
            } while (i != 0xe);

            if (count == 6)
            {
                if (gameEngine.EntityManager.ComputeNewHp(gameEngine.StaticVariables.g_entitySlots[2]))
                {
                    gameEngine.StaticVariables.g_entitySlots[2].Bytes[3] = 1;
                    gameEngine.StaticVariables.g_entitySlots[2].DelayOrAngle = 0;
                    gameEngine.StaticVariables.g_entitySlots[2].Bytes[1] = 0x10;
                }
                gameEngine.StaticVariables.g_entitySlots[2].TargetAnimationId = 4;
            }
        }

        FunctionTypeC.AI_UpdateEntityAI_0_00(gameEngine, entity);
    }

    // GHIDRA: FUN_80073940 @ 0x80073940
    internal static void FUN_80073940(GameEngine gameEngine, Entity entity)
    {
        var entitySlots = gameEngine.StaticVariables.g_entitySlots;
        int entityIndex = Array.IndexOf(entitySlots, entity);

        if (entityIndex < 0 || entityIndex + 14 >= entitySlots.Length)
        {
            return;
        }

        for (int i = 0; i < 15; i++)
        {
            Entity current = entitySlots[entityIndex + i];

            if (current.Status != 2)
            {
                continue;
            }

            uint spriteTableIndex = current.SpriteTableIndex;
            if (spriteTableIndex < 0x1E8U || spriteTableIndex >= 0x1EDU)
            {
                continue;
            }

            if ((current.TargetAnimationId == 6 || current.TargetAnimationId == 7)
                && current.ForceResetAnimationFlag != 0
                && i != 0)
            {
                current.TargetAnimationId = current.Bytes[3] != 0 ? 10U : 0U;
            }

            current.TargetAnimationId &= 0xFEU;

            uint targetDirection = current.TargetDirection;
            int animationDirection;
            bool incrementAnimation;
            bool currentDirectionMatches;

            if (targetDirection < 2U || targetDirection >= 30U)
            {
                animationDirection = 0;
                incrementAnimation = false;
                currentDirectionMatches = current.CurrentDirection < 2U || current.CurrentDirection >= 30U;
            }
            else if (targetDirection < 6U)
            {
                animationDirection = 0;
                incrementAnimation = true;
                currentDirectionMatches = current.CurrentDirection >= 2U && current.CurrentDirection < 6U;
            }
            else if (targetDirection < 10U)
            {
                animationDirection = 2;
                incrementAnimation = false;
                currentDirectionMatches = current.CurrentDirection >= 6U && current.CurrentDirection < 10U;
            }
            else if (targetDirection < 14U)
            {
                animationDirection = 2;
                incrementAnimation = true;
                currentDirectionMatches = current.CurrentDirection >= 10U && current.CurrentDirection < 14U;
            }
            else if (targetDirection < 18U)
            {
                animationDirection = 1;
                incrementAnimation = false;
                currentDirectionMatches = current.CurrentDirection >= 14U && current.CurrentDirection < 18U;
            }
            else if (targetDirection < 22U)
            {
                animationDirection = 1;
                incrementAnimation = true;
                currentDirectionMatches = current.CurrentDirection >= 18U && current.CurrentDirection < 22U;
            }
            else if (targetDirection < 26U)
            {
                animationDirection = 3;
                incrementAnimation = false;
                currentDirectionMatches = current.CurrentDirection >= 22U && current.CurrentDirection < 26U;
            }
            else
            {
                animationDirection = 3;
                incrementAnimation = true;
                currentDirectionMatches = current.CurrentDirection >= 26U && current.CurrentDirection < 30U;
            }

            if (current.AnimationDirection != animationDirection || !currentDirectionMatches)
            {
                current.CurrentAnimationId = 0xFFFFFFFFU;
            }

            current.AnimationDirection = animationDirection;

            if (incrementAnimation)
            {
                current.TargetAnimationId += 1;
            }
        }
    }

    //8007e548
    public static void AI_FUN_8007e548(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        if (gameEngine.StaticVariables.g_currentMap != 0x7b)
        {
            if (gameEngine.EntityManager.ComputeNewHp(entity))
            {
                entity.Bytes[3] = 1;
            }
        }

        entity.TargetAnimationId = 6;
        entity.Bytes[1] += 1;
        entity.Bytes[0] += 1;
    }

    //8007e5b0
    public static void AI_FUN_8007e5b0(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
            entity.ParentEntity.Bytes[2] -= 1;
        }

        entity.TargetAnimationId = 5;
        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.g_entitySlots[0].PosX, entity.PosY - gameEngine.StaticVariables.g_entitySlots[0].PosY);
    }

    //8007e628
    public static void AI_FUN_8007e628(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        HitCommon(gameEngine, entity, 7);
    }

    //8007e68c
    public static void AI_FUN_8007e68c(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();
    }

    //8007e694
    public static void AI_FUN_8007e694(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        var value = entity.TouchingEntity.BalanceAnimValRef.Val & 0xf;

        if (value == 7 || value == 9)
        {
            HitCommon(gameEngine, entity, 8);
        }
    }

    //8007e704
    public static void AI_FUN_8007e704(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        if (entity.TargetAnimationId != 4)
        {
            HitCommon(gameEngine, entity, 6);
        }
    }

    //8007e754
    public static void AI_FUN_8007e754(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        HitCommon(gameEngine, entity, 7);
    }

    //8007e790
    public static void AI_FUN_8007e790(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        entity.TargetAnimationId = 0xd;
    }

    //8007e79c
    public static void AI_FUN_8007e79c(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Bombe")
        {
            Breakpoint.TriggerBreak();
        }

        var value = entity.TouchingEntity.BalanceAnimValRef.Val & 0xf;
        if (value == 4 || value == 6 || value == 10)
        {
            entity.Status = 3;
        }
    }

    //8007e7e4
    public static void AI_FUN_8007e7e4(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "Bombe"
            && entity.Name != "Mur à boule de fer (111)"
            && entity.Name != "Mur à boule de fer (112) – axe"
            && entity.Name != "Mur à boule de fer (222) – boule de fer"
            && entity.Name != "Mur à boule de fer (2×2×2) permanent")
        {
            Breakpoint.TriggerBreak();
        }

        if ((entity.TouchingEntity.BalanceAnimValRef.Val & 0xf) == 2)
        {
            entity.TargetAnimationId += 1;
            entity.Flags |= 0x40;
        }
    }

    //8007e828
    public static void AI_FUN_8007e828(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        if ((entity.TouchingEntity.BalanceAnimValRef.Val & 0xf) == 4)
        {
            entity.TargetAnimationId += 1;
            entity.Flags |= 0x40;
        }
    }

    //8007e86c
    public static void AI_FUN_8007e86c(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        if ((entity.TouchingEntity.BalanceAnimValRef.Val & 0xf) == 2)
        {
            entity.TargetAnimationId = 2;
            entity.Flags |= 0x40;
        }
    }

    //8007e8ac
    public static void AI_FUN_8007e8ac(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        if ((entity.TouchingEntity.BalanceAnimValRef.Val & 0xf) == 6)
        {
            entity.TargetAnimationId = 1;
            entity.Flags = (entity.Flags | 0x46U) & 0xFFFFFF7FU;
        }
    }

    //8007e8f0
    public static void AI_FUN_8007e8f0(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != null
            && entity.Name != "◆P-Zoldia Niv.1")
        {
            Breakpoint.TriggerBreak();
        }

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
            entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.g_entitySlots[0].PosX, entity.PosY - gameEngine.StaticVariables.g_entitySlots[0].PosY); ;
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
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Abyss (projectiles-bulles)")
        {
            Breakpoint.TriggerBreak();
        }

        entity.TargetAnimationId = 1;
        entity.Flags |= 0x40;
    }

    //8007e9ac
    public static void AI_FUN_8007e9ac(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 1;
    }

    //8007e9e8
    public static void AI_FUN_8007e9e8(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 2;
    }

    //8007ea24
    public static void AI_FUN_8007ea24(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        if (gameEngine.EntityManager.ComputeNewHp(entity)
            || entity.TouchingEntity!.SpriteTableIndex == 0x1ef)
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 3;
    }

    //8007ea84
    public static void AI_FUN_8007ea84(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        Entity parentEntity = entity.ParentEntity!;

        if (entity.SpriteTableIndex == 0x1d7)
        {
            if (gameEngine.EntityManager.ComputeNewHp(entity))
            {
                entity.Bytes[3] = 1;
            }

            entity.TargetAnimationId = 2;
            entity.AIValues[4] = 1;
            return;
        }

        entity.TargetAnimationId = 2;

        if (parentEntity.Bytes[0] < 0x60)
        {
            parentEntity.Bytes[0] = (byte)(parentEntity.Bytes[0] + 0x20);
            parentEntity.Bytes[1] = 0x10;

            if (parentEntity.Bytes[0] >= 0x61)
            {
                parentEntity.Bytes[0] = 0x60;
            }
        }
    }

    //8007eb1c
    public static void AI_FUN_8007eb1c(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 3;
    }

    //8007eb58
    public static void AI_FUN_8007eb58(GameEngine gameEngine, Entity entity)
    {
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Après Melzas")
        {
            Breakpoint.TriggerBreak();
        }

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
        if (!string.IsNullOrEmpty(entity.Name)
            && entity.Name != "◆Bras, projectiles")
        {
            Breakpoint.TriggerBreak();
        }

        entity.ForceZ = 0;
        entity.PreviousAdjustedForceY = 0;
        entity.PreviousAdjustedForceX = 0;

        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 6;
    } 

    //8007ebf0
    public static void AI_FUN_8007ebf0(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        Entity parentEntity = entity.ParentEntity!;
        uint soundEffectId = 3;

        if (entity.SpriteTableIndex == 0x14f)
        {
            soundEffectId = 2;
            parentEntity.Bytes[2] += 1;
        }
        else
        {
            parentEntity.Bytes[0] += 1;
        }

        gameEngine.SoundManager.PlaySoundEffect(soundEffectId);
        gameEngine.DestroyEntity(entity, 8);
    }

    //8007ec60
    public static void AI_FUN_8007ec60(GameEngine gameEngine, Entity entity)
    {
        Breakpoint.TriggerBreak();

        if ((entity.TouchingEntity!.BalanceAnimValRef!.Val & 0xf) == 2)
        {
            entity.TargetAnimationId = 4;
            entity.Bytes[2] += 1;
        }
    }

    //8007ec9c
    public static void AI_FUN_8007ec9c(GameEngine gameEngine, Entity entity)
    {
        if (entity.Name != "◆Slime géant (grand)"
            && entity.Name != "◆Slime géant (petit)")
        {
            Breakpoint.TriggerBreak();
        }

        if (gameEngine.EntityManager.ComputeNewHp(entity))
        {
            entity.Bytes[3] = 1;
        }

        entity.TargetAnimationId = 2;

        if (entity.SpriteTableIndex == 459)
        {
            entity.TargetAnimationId = 3;
        }

        var direction = (uint)ScriptHelper.GetDirectionToTarget(entity.PosX - gameEngine.StaticVariables.g_entitySlots[0].PosX, entity.PosY - gameEngine.StaticVariables.g_entitySlots[0].PosY);
        entity.TargetDirection = direction;
    }
}