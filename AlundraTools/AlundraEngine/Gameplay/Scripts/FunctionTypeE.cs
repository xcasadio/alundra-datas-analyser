using System.Diagnostics;

namespace AlundraEngine.Gameplay.Scripts;

public static class FunctionTypeE
{
    //8007ed10
    public static void FUN_8007ed10(GameEngine gameEngine, Entity entity)
    {
        gameEngine.DestroyEntity(entity, -1);
    }

    //8007ed30
    public static void AI_FUN_8007ed30(GameEngine gameEngine, Entity entity)
    {
        if ((entity.TargetAnimationId == 2 && entity.ForceResetAnimationFlag == 1)
            || (entity.CombinedVramFlagsAND & 4U) != 0)
        {
            gameEngine.DestroyEntity(entity, -1);
        }
        else
        {
            entity.TargetAnimationId = 2;
            entity.Flags = (entity.Flags & 0xffffffcfU) | 0x40;
        }
    }

    //8007eda0
    //spores nv1
    public static void AI_FUN_8007eda0(GameEngine gameEngine, Entity entity)
    {
        if (entity.TargetAnimationId == 1 && entity.ForceResetAnimationFlag == 1)
        {
            gameEngine.DestroyEntity(entity, -1);
        }
        else
        {
            entity.TargetAnimationId = 1;
            entity.Flags = (entity.Flags & 0xffffffcfU) | 0x40;
        }
    }

    //8007ee04
    public static void AI_FUN_8007ee04(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007ee68
    public static void AI_FUN_8007ee68(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007eef0
    public static void AI_FUN_8007eef0(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007ef10  
    public static void AI_FUN_8007ef10(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007ef30
    public static void AI_FUN_8007ef30(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007ef50
    public static void AI_HandleIceLightHitEffect(GameEngine gameEngine, Entity entity)
    {
        ulong rand;
        SpriteEffect effectEntity;
        int isSmallSprite;
        byte spriteTableIndex;
        int randomOffset;

        if (entity.TargetAnimationId == (int)PlayerAnimation.Moving)
        {
            if (entity.Slope_18c == 1 || entity.Slope_18c == 4)
            {
                gameEngine.DestroyEntity(entity, 6);
            }

            if (entity.ForceResetAnimationFlag == 1)
            {
                gameEngine.DestroyEntity(entity, -1);
            }
            else if ((entity.FrameCounter & 7U) == 0)
            {
                isSmallSprite = entity.SpriteTableIndex - 0xbU < 2 ? 1 : 0;
                spriteTableIndex = 0x12;

                if (isSmallSprite != 0)
                {
                    spriteTableIndex = 0x11;
                }

                effectEntity = gameEngine.EffectManager.CreateEffectEntity(
                    0, spriteTableIndex, 0,
                    entity.PosX, entity.PosY, entity.PosZ + isSmallSprite * 0x80000);

                if (effectEntity != null)
                {
                    isSmallSprite = (int)Random.Next();
                    rand = Random.Next();
                    randomOffset = (isSmallSprite * 0x30001) >> 0x20;

                    effectEntity.ForceZ = 0x20000;
                    effectEntity.ForceX = randomOffset + -0x18000;
                    effectEntity.ForceY = (int)((rand * 0x20001) >> 0x20) + -0x10000;
                }
            }
        }
        else
        {
            var loopCounter = 0;
            spriteTableIndex = 0x12;

            if (entity.SpriteTableIndex - 0xbU < 2)
            {
                spriteTableIndex = 0x11;
            }

            if (entity.Slope_18c == 1 || entity.Slope_18c == 4)
            {
                gameEngine.DestroyEntity(entity, 6);
            }
            else
            {
                do
                {
                    effectEntity = gameEngine.EffectManager.CreateEffectEntity(
                        0, spriteTableIndex, 0,
                        entity.PosX, entity.PosY, entity.PosZ);

                    if (effectEntity != null)
                    {
                        isSmallSprite = (int)Random.Next();
                        rand = Random.Next();
                        randomOffset = (isSmallSprite * 0x60001) >> 0x20;

                        effectEntity.ForceX = randomOffset + -0x30000;
                        effectEntity.ForceY = (int)((rand * 0x40001) >> 0x20) + -0x20000;
                    }

                    loopCounter += 1;

                } while (loopCounter < 5);

                entity.TargetAnimationId = 1;
                entity.Flags = (entity.Flags & 0xffffffcfU) | 0x40;
            }
        }
    }

    //8007f23c
    public static void AI_FUN_8007f23c(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007f25c
    public static void AI_FUN_8007f25c(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007f27c
    public static void AI_FUN_8007f27c(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007f30c
    public static void AI_FUN_8007f30c(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007f378
    public static void AI_FUN_8007f378(GameEngine gameEngine, Entity entity)
    {
        gameEngine.SoundManager.PlaySoundEffect(0x2d);
        gameEngine.DestroyEntity(entity, -1);
    }

    //8007f3b0
    public static void AI_FUN_8007f3b0(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007f3e8
    public static void AI_FUN_8007f3e8(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007f420
    public static void AI_UpdateArrows(GameEngine gameEngine, Entity entity)
    {
        int entityTargetIndex = 0;

        if (entity.TargetAnimationId == 1) //after hit a wall (turn around)
        {
            if (entity.ForceResetAnimationFlag == 1)
            {
                gameEngine.DestroyEntity(entity, -1);
            }
        }
        else
        {
            if (entity.HitCounter != 0)
            {
                var flags = entity.Flags;
                var collisionMask = (flags & 2) << 2;

                if ((flags & 0x4) != 0)
                {
                    collisionMask |= 1;
                }

                if ((flags & 0x1000) != 0)
                {
                    collisionMask |= 0x800;
                }

                var noValidCollisionFound = true;

                if (collisionMask != 0)
                {
                    var entityIndex = 0;

                    if (-1 < gameEngine.StaticVariables.g_numberOfEntities)
                    {
                        do
                        {
                            var entityTarget = gameEngine.StaticVariables.g_entitySlots[entityTargetIndex];

                            if (entity != entityTarget
                                && (entityTarget.AnimFlags & 0x40) == 0
                                && entityTarget.BalanceRecord.Values[5] == 0
                                && (entityTarget.Flags & collisionMask) != 0)
                            {
                                var withinX = entity.HitBoxX - entityTarget.HitBoxOriginX; //entityTarget.ModdedPosX
                                bool withinY;

                                if (withinX < 0)
                                {
                                    withinY = entityTarget.HitBoxOriginX - entity.HitBoxX < entity.CollisionWidth + 1;
                                }
                                else
                                {
                                    withinY = withinX < entityTarget.TileAttributes + 1;
                                }

                                if (withinY)
                                {
                                    withinX = entity.HitBoxY - entityTarget.HitBoxOriginY;

                                    if (withinX < 0)
                                    {
                                        withinY = entityTarget.HitBoxOriginY - entity.HitBoxY < entity.CollisionDepth + 1;
                                    }
                                    else
                                    {
                                        withinY = withinX < entityTarget.Slope_18c + 1;
                                    }

                                    if (withinY)
                                    {
                                        withinX = entity.HitBoxZ - entityTarget.HitBoxOriginZ;

                                        if (withinX < 0)
                                        {
                                            withinY = entityTarget.HitBoxOriginZ - entity.HitBoxZ < entity.CollisionHeight + 1;
                                        }
                                        else
                                        {
                                            withinY = withinX < entityTarget.Slope_190 + 1;
                                        }

                                        if (withinY && entityTarget.Index != 0X1AD)
                                        {
                                            noValidCollisionFound = false;
                                            break;
                                        }
                                    }
                                }
                            }

                            entityIndex += 1;
                            entityTargetIndex += 1;

                        } while (entityIndex <= gameEngine.StaticVariables.g_numberOfEntities);
                    }

                    if (noValidCollisionFound)
                    {
                        entity.Status = 2;
                        return;
                    }
                }

                gameEngine.DestroyEntity(entity, -1);
            }

            entity.TargetAnimationId = 1;
            entity.TargetDirection = (entity.TargetDirection + 0x10) & 0X1F;
            entity.Flags = (entity.Flags & 0XFFFFFFCF) | 0x140;
        }
    }

    //8007fb38
    public static void AI_FUN_8007fb38(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007f5c0
    public static void AI_FUN_8007f658(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007f690
    public static void AI_FUN_8007f690(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007f6c8
    public static void AI_FUN_8007f6c8(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007f7a0
    public static void AI_FUN_8007f7cc(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007f878
    public static void AI_FUN_8007f878(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007f8ac
    public static void AI_FUN_8007f8ac(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007f974
    public static void AI_FUN_8007f974(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007f9a0
    public static void AI_FUN_8007faa0(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007fac0
    public static void AI_FUN_8007fac0(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }

    //8007fb38
    public static void AI_DestroyEntity(GameEngine gameEngine, Entity entity)
    {
        gameEngine.DestroyEntity(entity, -1);
    }

    //8007fb58
    public static void AI_FUN_8007fb58(GameEngine gameEngine, Entity entity)
    {
        Debugger.Break();
    }
}