using AlundraEngine.Gameplay;

namespace AlundraEngine;

public static class PhysicEngine
{
    //80037f28
    public static int GetCollisionOnZ(GameEngine gameEngine, Entity entity)
    {
        var collision = entity.TerrainHeight + 1;

        if ((entity.Flags & 0x80) == 0)
        {
            return collision;
        }

        if ((entity.AnimFlags & 0x80) != 0)
        {
            return collision;
        }

        if (entity.PlatformEntity != null)
        {
            return collision;
        }

        if (gameEngine.StaticVariables.g_collideableEntitiesCount <= 0)
        {
            return collision;
        }

        for (var dex = 0; dex < gameEngine.StaticVariables.g_collideableEntitiesCount; dex++)
        {
            var otherEntity = gameEngine.StaticVariables.g_collideableEntities[dex];

            if (otherEntity == entity)
            {
                continue;
            }

            if (otherEntity.ModdedPosZ + otherEntity.Height >= entity.ModdedPosZ
                || otherEntity.ModdedPosZ + otherEntity.Height < collision)
            {
                continue;
            }

            if (otherEntity.ModdedPosX - entity.ModdedPosX >= 0)
            {
                if (otherEntity.ModdedPosX - entity.ModdedPosX >= entity.Width + 1)
                {
                    continue;
                }
            }
            else
            {
                if (entity.ModdedPosX - otherEntity.ModdedPosX >= otherEntity.Width + 1)
                {
                    continue;
                }
            }

            if (otherEntity.ModdedPosY - entity.ModdedPosY >= 0)
            {
                if (otherEntity.ModdedPosY - entity.ModdedPosY < entity.Depth + 1)
                {
                    collision = otherEntity.ModdedPosZ + otherEntity.Depth;
                }
            }
            else
            {
                if (entity.ModdedPosY - otherEntity.ModdedPosY < otherEntity.Depth + 1)
                {
                    collision = otherEntity.ModdedPosZ + otherEntity.Depth;
                }
            }

        }

        return collision;
    }
}