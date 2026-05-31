using AlundraEngine.Gameplay.Scripts.Boss;

namespace AlundraEngine.Gameplay.Scripts;

public static class FunctionTypeA
{

    // 8006174C
    public static void SetSpawnFlagFromZPos(GameEngine gameEngine, Entity entity)
    {
        entity.AIValues.Set(entity.PosZ >> 16);
    }

    // 80061758
    public static void SetAnimationTo2(GameEngine gameEngine, Entity entity)
    {
        entity.TargetAnimationId = 2;
    }

    // 80061764
    public static void SetRandomizedAnimIdAndFlag(GameEngine gameEngine, Entity entity)
    {
        entity.TargetAnimationId = 4;
        var rand = ((Random.Next() * 0x1F) >> 32);
        entity.AIValues[0] = (short)(rand + 0xB4);
    }

    // 800617B8
    public static void SetAnimationTo4(GameEngine gameEngine, Entity entity)
    {
        entity.TargetAnimationId = 4;
    }

    // 800617C4
    public static void SetAnimationTo9(GameEngine gameEngine, Entity entity)
    {
        entity.TargetAnimationId = 9;
    }

    // 800617D0
    public static void SetAnimationTo7(GameEngine gameEngine, Entity entity)
    {
        entity.TargetAnimationId = 7;
    }

    // 800617DC
    public static void SetAnimationTo3(GameEngine gameEngine, Entity entity)
    {
        entity.TargetAnimationId = 3;
    }

    // 800617E8
    public static void SetAnimationTo9_2(GameEngine gameEngine, Entity entity)
    {
        entity.TargetAnimationId = 9;
    }

    // 800617F4
    public static void SetAnimationTo8AndCustomByte3(GameEngine gameEngine, Entity entity)
    {
        entity.TargetAnimationId = 8;
        entity.Bytes[0] = 3;
    }

    // 80061808
    public static void SetAnimationTo10(GameEngine gameEngine, Entity entity)
    {
        entity.TargetAnimationId = 10;
    }

    // 80061814
    public static void SetAnimationTo6(GameEngine gameEngine, Entity entity)
    {
        entity.TargetAnimationId = 6;
    }

    // 80061820
    public static void SpawnWarpAndSetAnim(GameEngine gameEngine, Entity entity)
    {
        Entity spawned = gameEngine.SpawnWarpEntity(
            entity,
            1, 
            0xF5, 
            entity.PosX + 0xF00000,
            entity.PosY, 
            entity.PosZ, 
            entity.TargetDirection);

        entity.AIValues[0] = (short)spawned.Index;
        spawned.AIValues[1] = 0;
        entity.TargetAnimationId = 3;
        spawned.TargetAnimationId = 0;
    }

    // 80061888
    public static void TriggerMultipleWarpsAndClearHistory(GameEngine gameEngine, Entity entity)
    {
        var index = Array.IndexOf(gameEngine.StaticVariables.g_entitySlots, entity);

        for (int i = index + 1; i < index + 16; i++)
        {
            gameEngine.DestroyEntity(gameEngine.StaticVariables.g_entitySlots[i]);
        }

        for (int i = 0; i < 0x100; i++)
        {
            gameEngine.StaticVariables.g_loaderDirectionHistory[i] = 0;
            gameEngine.StaticVariables.DAT_80191508[i] = 0;
            gameEngine.StaticVariables.DAT_80191708[i] = 0;
        }

        gameEngine.StaticVariables.g_loaderInitialized = 0;
    }

    // 8006191C
    public static void SetAnim8AndResetLoader(GameEngine gameEngine, Entity entity)
    {
        entity.TargetAnimationId = 8;
        gameEngine.StaticVariables.g_loaderInitialized = 0;
    }

    // 80061930
    public static void SpawnSpecificWarpAndResetLoader(GameEngine gameEngine, Entity entity)
    {
        Entity spawned = gameEngine.SpawnWarpEntity(
            entity, 
            1, 
            0x9D,
            entity.PosX + 0x380000, 
            entity.PosY, 
            entity.PosZ - 0x200000, 
            entity.TargetDirection);

        entity.AIValues[2] = (short)spawned.Index;
        gameEngine.StaticVariables.g_loaderInitialized = 0;
    }

    // 80061998
    public static void SetAnim0AndResetLoader(GameEngine gameEngine, Entity entity)
    {
        entity.TargetAnimationId = 0;
        gameEngine.StaticVariables.g_loaderInitialized = 0;
    }

    // 800619A8
    public static void SetAnimEAndResetLoader(GameEngine gameEngine, Entity entity)
    {
        entity.TargetAnimationId = 0xE;
        gameEngine.StaticVariables.g_loaderInitialized = 0;
        entity.AIValues[2] = 0;
        entity.AIValues[3] = 0;
    }

    // 800619C0
    public static void SetTargetAnimationTo14(GameEngine gameEngine, Entity entity)
    {
        entity.TargetAnimationId = 0xE;
        entity.AIValues[2] = 0;
        entity.AIValues[3] = 0;
    }

    // 800619D0
    public static void SetCustomByteFromProgramIndex(GameEngine gameEngine, Entity entity)
    {
        entity.Bytes[1] = (byte)entity.ProgramIndexes[2];
        //entity.DelayOrAngle = entity.ProgramIndexes[2];
    }

    // 800619DC
    public static void SetCustomByteFromZPos(GameEngine gameEngine, Entity entity)
    {
        entity.Bytes.Set(entity.PosZ);
    }

    // 800619E8
    public static void SpawnWarpDropAndAdjustPosition(GameEngine gameEngine, Entity entity)
    {
        Entity spawned = gameEngine.SpawnWarpEntity(entity, 1, 0xD8, entity.PosX, entity.PosY, entity.PosZ, entity.TargetDirection);
        spawned.TargetAnimationId = 5;
        entity.TargetAnimationId = 3;
        entity.PosY -= 0x100000;
        entity.PosZ += 0x300000;
    }

    //80061bcc
    public static void FUN_80061bcc(GameEngine gameEngine, Entity entity)
    {
        //do nothing
    }

    //80061bd4
    //Chest
    public static void FUN_80061bd4(GameEngine gameEngine, Entity entity)
    {
        ushort contentFlags;
        uint[] flags;

        if (entity.SpriteTableIndex == 0x1e)
        {
            if (entity.ContentsItemId != 0)
            {
                return;
            }

            entity.TargetAnimationId = 1;

            return;
        } 

        if (entity.Bytes[0] == 1)
        {
            return;
        }

        if (entity.Bytes[0] == 2)
        {
            if (entity.ParentEntity.TargetAnimationId == 1)
            {
                if (entity.AIValues[4] != 0)
                {
                    gameEngine.CdManager.StartCdStreaming(0xb);
                }
            }
            else
            {
                entity.Status = 1;
            }

            return;
        }

        entity.Flags &= 0xffffff7f;

        if (entity.ContentsGameFlag != 0)
        {
            contentFlags = (ushort)entity.ContentsGameFlag;

            if ((contentFlags & 0x8000) == 0)
            {
                flags = gameEngine.StaticVariables.g_saveData.GameFlags;
            }
            else
            {
                flags = gameEngine.StaticVariables.g_temporaryFlags;
            }


            var index = ((contentFlags >> 3) & 0xffc) >> 2;
            var mask = 1 << (entity.ContentsGameFlag & 0x1f);

            if ((flags[index] & mask) == 0)
            {
                gameEngine.DestroyEntity(entity);
                return;
            }
        }

        entity.AIValues[0] = -1;
        entity.AIValues[1] = -1;
        entity.AIValues[2] = 0;
        entity.AIValues[3] = 0;
        entity.DelayOrAngleOrEntityId = entity.ContentsGameFlag;
        entity.ItemState = 0;
    }
}