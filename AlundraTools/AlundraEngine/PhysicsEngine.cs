using AlundraEngine.Gameplay;
using AlundraEngine.Gameplay.Scripts;
using AlundraEngine.RuntimeInspection;

namespace AlundraEngine;

public static class PhysicsEngine
{
    // 80038364
    public static void UpdateEntitiesPhysics(GameEngine gameEngine)
    {
        for (var i = 0; i < gameEngine.StaticVariables.g_activeEntityCount; i++)
        {
            var entity = gameEngine.StaticVariables.g_activeEntities[i];
            entity.PlatformUpdateFlag = 0;
            entity.CollidedWithEntityZ = 0;
            entity.ForceAdjusted = 0;

            entity.ModdedPosX = entity.PosX + entity.ModX;
            entity.ModdedPosY = entity.PosY + entity.ModY;
            entity.ModdedPosZ = entity.PosZ + entity.ModZ;
        }

        CheckRidingEntities(gameEngine);
        UpdateEntitiesForces(gameEngine);

        for (var i = 0; i < gameEngine.StaticVariables.g_collideableEntitiesCount; i++)
        {
            var entity = gameEngine.StaticVariables.g_collideableEntities[i];
            if (entity.RidingEntity != null)
            {
                UpdateRidingEntity(entity, entity.RidingEntity);
            }
        }

        for (var i = 0; i < gameEngine.StaticVariables.g_activeEntityCount; i++)
        {
            var entity = gameEngine.StaticVariables.g_activeEntities[i];
            if (entity.PlatformUpdateFlag == 0)
            {
                MoveEntity(entity, gameEngine);
            }
        }

        for (var i = 0; i < gameEngine.StaticVariables.g_activeEntityCount; i++)
        {
            var entity = gameEngine.StaticVariables.g_activeEntities[i];
            UpdateTileAttributes(entity, gameEngine);
        }
    }


    // 80037364
    private static void UpdateRidingEntity(Entity entity, Entity ridingEntity)
    {
        if (ridingEntity.RidingEntity != null)
        {
            UpdateRidingEntity(ridingEntity, ridingEntity.RidingEntity);
        }

        entity.FinalForceX += ridingEntity.AdjustedForceX;
        entity.FinalForceY += ridingEntity.AdjustedForceY;
        if (entity.IsZForceApplied == 0)
        {
            entity.ForceZ = ridingEntity.FinalForceZ;
            entity.FinalForceZ = ridingEntity.FinalForceZ;
        }
    }

    // 80037e34
    public static void MoveEntity(Entity entity, GameEngine gameEngine)
    {
        int updatedZPosition;
        Entity platformEntity;

        platformEntity = entity.PlatformEntity;
        entity.PlatformUpdateFlag = 1;

        if (platformEntity == null)
        {
            ComputeZPosition(entity, gameEngine);
            platformEntity = ComputeXYPosition(entity, gameEngine);
            entity.XCollisionEntity = platformEntity;
        }
        else
        {
            if (platformEntity.PlatformUpdateFlag == 0)
            {
                MoveEntity(platformEntity, gameEngine);
            }

            updatedZPosition = platformEntity.PosZ + platformEntity.RelativeWarpOffsetZ;

            entity.CollidedWithEntityZ = 1;
            entity.ForceAdjusted = 1;
            entity.XCollisionEntity = null;
            entity.FloorHeight = platformEntity.FloorHeight;
            entity.TerrainHeight = platformEntity.TerrainHeight;
            entity.PosX = platformEntity.PosX + platformEntity.RelativeWarpOffsetX;
            entity.PosY = platformEntity.PosY + platformEntity.RelativeWarpOffsetY;
            entity.PosZ = updatedZPosition;
            entity.ModdedPosX = entity.PosX + entity.ModX;
            entity.ModdedPosY = entity.PosY + entity.ModY;
            entity.ModdedPosZ = updatedZPosition + entity.ModZ;
        }
    }

    // 800375e0
    public static void ComputeZPosition(Entity entity, GameEngine gameEngine)
    {
        int groundHeight;
        int finalZVelocity;
        int platformHeight;
        Entity platformEntity;

        finalZVelocity = entity.FinalForceZ;

        if (finalZVelocity < 1)
        {
            groundHeight = ComputeEntityGroundHeight(entity, gameEngine);
            entity.TerrainHeight = groundHeight;

            while (CheckEntityCollisionDown(entity, out platformHeight, out platformEntity, gameEngine))
            {
                if (platformEntity == null || platformEntity.PlatformUpdateFlag != 0)
                {
                    entity.CollidedWithEntityZ = 1;
                    entity.PosZ = platformHeight - entity.ModZ;
                    if ((entity.Flags & 0x100) == 0)
                    {
                        return;
                    }

                    entity.ForceZ = 0;
                    return;
                }

                MoveEntity(platformEntity, gameEngine);
            }
        }
        else
        {
            groundHeight = ComputeEntityGroundHeight(entity, gameEngine);
            entity.TerrainHeight = groundHeight;

            while (CheckEntityCollisionUp(entity, out platformHeight, out platformEntity, gameEngine))
            {
                if (platformEntity == null || platformEntity.PlatformUpdateFlag != 0)
                {
                    entity.CollidedWithEntityZ = 1;
                    entity.PosZ = platformHeight - entity.ModZ - entity.Depth;
                    if ((entity.Flags & 0x100) == 0)
                    {
                        return;
                    }

                    entity.ForceZ = 0;
                    return;
                }

                MoveEntity(platformEntity, gameEngine);
            }
        }

        entity.PosZ = entity.PosZ + finalZVelocity;
    }

    // 80036bfc
    public static bool CheckEntityCollisionDown(Entity entity, out int platformHeight, out Entity platformEntity, GameEngine gameEngine)
    {
        int deltaX;
        Entity candidateEntity;
        int deltaY;
        int platformTopZ;
        Entity bestCandidate;
        bool collisionDetected;
        Entity[] collidableEntityPtr;
        int entityIndex;

        platformTopZ = entity.ModdedPosZ + entity.FinalForceZ;
        collisionDetected = platformTopZ <= entity.TerrainHeight;
        bestCandidate = null;

        if (collisionDetected)
        {
            platformTopZ = entity.TerrainHeight + 1;
        }

        if ((entity.Flags & 0x80) != 0 && (entity.AnimFlags & 0x80) == 0 && entity.PlatformEntity == null)
        {
            entityIndex = 0;

            if (gameEngine.StaticVariables.g_collideableEntitiesCount > 0)
            {
                collidableEntityPtr = gameEngine.StaticVariables.g_collideableEntities;

                do
                {
                    candidateEntity = collidableEntityPtr[entityIndex];

                    if (entity != candidateEntity)
                    {
                        deltaY = candidateEntity.ModdedPosZ + candidateEntity.Depth;

                        if (deltaY < entity.ModdedPosZ && platformTopZ <= deltaY)
                        {
                            deltaX = candidateEntity.ModdedPosX - entity.ModdedPosX;

                            if (deltaX < 0)
                            {
                                if (entity.ModdedPosX - candidateEntity.ModdedPosX < candidateEntity.Width + 1)
                                {
                                    deltaX = candidateEntity.ModdedPosY - entity.ModdedPosY;

                                    if (deltaX < 0)
                                    {
                                        if (entity.ModdedPosY - candidateEntity.ModdedPosY < candidateEntity.Height + 1)
                                        {
                                            platformTopZ = deltaY + 1;
                                            collisionDetected = true;
                                            bestCandidate = candidateEntity;
                                        }
                                    }
                                    else if (deltaX < entity.Height + 1)
                                    {
                                        platformTopZ = deltaY + 1;
                                        collisionDetected = true;
                                        bestCandidate = candidateEntity;
                                    }
                                }
                            }
                            else if (deltaX < entity.Width + 1)
                            {
                                deltaX = candidateEntity.ModdedPosY - entity.ModdedPosY;

                                if (deltaX < 0)
                                {
                                    if (entity.ModdedPosY - candidateEntity.ModdedPosY < candidateEntity.Height + 1)
                                    {
                                        platformTopZ = deltaY + 1;
                                        collisionDetected = true;
                                        bestCandidate = candidateEntity;
                                    }
                                }
                                else if (deltaX < entity.Height + 1)
                                {
                                    platformTopZ = deltaY + 1;
                                    collisionDetected = true;
                                    bestCandidate = candidateEntity;
                                }
                            }
                        }
                    }

                    entityIndex++;
                } while (entityIndex < gameEngine.StaticVariables.g_collideableEntitiesCount);
            }
        }

        platformHeight = platformTopZ;
        platformEntity = bestCandidate;
        return collisionDetected;
    }

    // 80036d94
    public static bool CheckEntityCollisionUp(Entity entity, out int platformHeight, out Entity platformEntity, GameEngine gameEngine)
    {
        int deltaX;
        int entityTopZ;
        Entity candidateEntity;
        int candidateZPos;
        int platformCandidateZ;
        int entityIndex;
        bool collisionDetected;
        Entity bestCandidate;

        entityTopZ = entity.ModdedPosZ + entity.Depth;
        platformCandidateZ = entityTopZ + entity.FinalForceZ;
        collisionDetected = 0x7800000 < platformCandidateZ;
        bestCandidate = null;

        if (collisionDetected)
        {
            platformCandidateZ = 0x77fffff;
        }

        if ((entity.Flags & 0x80) != 0
            && (entity.AnimFlags & 0x80) == 0
            && entity.PlatformEntity == null)
        {
            entityIndex = 0;
            if (gameEngine.StaticVariables.g_collideableEntitiesCount > 0)
            {
                do
                {
                    candidateEntity = gameEngine.StaticVariables.g_collideableEntities[entityIndex];

                    if (entity != candidateEntity)
                    {
                        candidateZPos = candidateEntity.ModdedPosZ;

                        if (entityTopZ < candidateZPos
                            && candidateZPos <= platformCandidateZ)
                        {
                            deltaX = candidateEntity.ModdedPosX - entity.ModdedPosX;

                            if (deltaX < 0)
                            {
                                if (entity.ModdedPosX - candidateEntity.ModdedPosX < candidateEntity.Width + 1)
                                {
                                    deltaX = candidateEntity.ModdedPosY - entity.ModdedPosY;

                                    if (deltaX < 0)
                                    {
                                        if (entity.ModdedPosY - candidateEntity.ModdedPosY < candidateEntity.Height + 1)
                                        {
                                            platformCandidateZ = candidateZPos - 1;
                                            collisionDetected = true;
                                            bestCandidate = candidateEntity;
                                        }
                                    }
                                    else if (deltaX < entity.Height + 1)
                                    {
                                        platformCandidateZ = candidateZPos - 1;
                                        collisionDetected = true;
                                        bestCandidate = candidateEntity;
                                    }
                                }
                            }
                            else if (deltaX < entity.Width + 1)
                            {
                                deltaX = candidateEntity.ModdedPosY - entity.ModdedPosY;

                                if (deltaX < 0)
                                {
                                    if (entity.ModdedPosY - candidateEntity.ModdedPosY < candidateEntity.Height + 1)
                                    {
                                        platformCandidateZ = candidateZPos - 1;
                                        collisionDetected = true;
                                        bestCandidate = candidateEntity;
                                    }
                                }
                                else if (deltaX < entity.Height + 1)
                                {
                                    platformCandidateZ = candidateZPos - 1;
                                    collisionDetected = true;
                                    bestCandidate = candidateEntity;
                                }
                            }
                        }
                    }

                    entityIndex = entityIndex + 1;
                } while (entityIndex < gameEngine.StaticVariables.g_collideableEntitiesCount);
            }
        }

        platformHeight = platformCandidateZ;
        platformEntity = bestCandidate;
        return collisionDetected;
    }

    // GHIDRA: ComputeXYPosition @ 0x80037730
    public static Entity ComputeXYPosition(Entity entity, GameEngine gameEngine)
    {
        Entity? candidate = null;
        Entity? result = null;
        int i = 0;
        const int s6 = -1;

        int entryPosX = entity.PosX;
        int entryPosY = entity.PosY;
        int entryPosZ = entity.PosZ;
        int entryFinalForceX = entity.FinalForceX;
        int entryFinalForceY = entity.FinalForceY;

        int dx, dy;
        int posX, posY, posZ;
        uint[] collisionFlags = new uint[4];
        uint collisionFlagsOr = 0;
        int attemptedPosX = entity.PosX;
        int attemptedPosY = entity.PosY;
        int attemptedPosZ = entity.PosZ;
        int attemptedGroundHeight = entity.TerrainHeight;
        int candidateIndex = -1;

        int modX = 0;
        int didAdjustForObstacle = 0;

        int isStraightDir = ((entity.TargetDirection & 7) == 0) ? 1 : 0;

        Func<Entity, uint[], uint> collisionFunc =
            (entity == gameEngine.StaticVariables.PlayerEntity)
                ? (entity1, collisionFlags1) => GetCollisionFlagsWithPlayer(entity1, collisionFlags1, gameEngine)
                : GetCollisionFlags;

    START_COLLISION_CHECK:
        dx = entity.FinalForceX;
        dy = entity.FinalForceY;

        if (dx == 0 && dy == 0)
        {
            goto FINALIZE_NO_MOVE;
        }

        modX = 0;
        candidate = null;
        result = null;
        i = 0;

    TRY_ADVANCE:
        posX = entity.PosX;
        posY = entity.PosY;
        posZ = entity.PosZ;

        collisionFlags[0] = 0;
        collisionFlags[1] = 0;
        collisionFlags[2] = 0;
        collisionFlags[3] = 0;

        entity.PosX = entity.PosX + dx;
        entity.PosY = entity.PosY + dy;

        attemptedPosX = entity.PosX;
        attemptedPosY = entity.PosY;
        attemptedPosZ = entity.PosZ;

        entity.ModdedPosZ = entity.PosZ + entity.ModZ;
        entity.ModdedPosX = entity.PosX + entity.ModX;
        entity.ModdedPosY = entity.PosY + entity.ModY;

        int groundHeight = ComputeEntityGroundHeight(entity, gameEngine);
        entity.TerrainHeight = groundHeight;
        attemptedGroundHeight = groundHeight;

        if ((entity.Flags & 0x100) != 0)
        {
            if (entity.ForceZ == 0)
            {
                int dz = groundHeight - entity.ModdedPosZ;
                dz -= 1;

                int zTolerance = 0x30000;

                if (dz < 0)
                {
                    zTolerance = 0x30003;
                    dz = -dz;
                }

                if (dz < zTolerance)
                {
                    int savedZ = entity.PosZ;
                    entity.PosZ = groundHeight + 1;
                    entity.ModdedPosX = entity.PosX + entity.ModX;
                    entity.ModdedPosY = entity.PosY + entity.ModY;
                    entity.ModdedPosZ = entity.PosZ + entity.ModZ;

                    Entity? otherEntity = FindEntityCollisionCandidate(entity, gameEngine);
                    if (otherEntity != null)
                    {
                        entity.PosZ = savedZ;
                        entity.ModdedPosX = entity.PosX + entity.ModX;
                        entity.ModdedPosY = entity.PosY + entity.ModY;
                        entity.ModdedPosZ = entity.PosZ + entity.ModZ;

                        goto RESTORE_POS;
                    }
                }
                else
                {
                    goto RESTORE_POS;
                }
            }
        }

    RESTORE_POS:
        candidate = FindEntityCollisionCandidate(entity, gameEngine);
        candidateIndex = candidate?.Index ?? -1;

        if (candidate != null)
        {
            result = candidate;
        }

        if (candidate == null)
        {
            goto CHECK_ENTITY_COLLISION;
        }

        goto OBSTACLE_PATH;

    CHECK_ENTITY_COLLISION:
        uint flags = collisionFunc(entity, collisionFlags);
        collisionFlagsOr = flags;

        if (flags == 0)
        {
            goto NO_OBSTACLE_PATH;
        }

    OBSTACLE_PATH:
        // LAB_80037938
        entity.PosX = posX;
        entity.PosY = posY;
        entity.PosZ = posZ;

        int halfDx = (dx == s6) ? 0 : (dx >> 1);
        int halfDy = (dy == s6) ? 0 : (dy >> 1);

        if (isStraightDir != 0)
        {
            if (halfDx != 0)
            {
                i = i + 1;
                dx = halfDx;
                dy = halfDy;
                goto TRY_ADVANCE;
            }

            if (halfDy == 0)
            {
                goto DECIDE_FINAL_OBSTACLE;
            }

            i = i + 1;
            dx = halfDx;
            dy = halfDy;
            goto TRY_ADVANCE;
        }
        else
        {
            if (halfDx == 0)
            {
                goto DECIDE_FINAL_OBSTACLE;
            }

            if (halfDy != 0)
            {
                i = i + 1;
                dx = halfDx;
                dy = halfDy;
                goto TRY_ADVANCE;
            }

            goto DECIDE_FINAL_OBSTACLE;
        }

    NO_OBSTACLE_PATH:
        // LAB_80037d68
        modX = 1;

        if (i == 0)
        {
            goto RETURN_RESULT;
        }

        int halfDx2 = (dx == s6) ? 0 : (dx >> 1);
        int halfDy2 = (dy == s6) ? 0 : (dy >> 1);

        if (isStraightDir != 0)
        {
            if (halfDx2 == 0)
            {
                if (halfDy2 == 0)
                {
                    goto RETURN_RESULT;
                }

                i = i + 1;
                dx = halfDx2;
                dy = halfDy2;
                goto TRY_ADVANCE;
            }

            i = i + 1;
            dx = halfDx2;
            dy = halfDy2;
            goto TRY_ADVANCE;
        }
        else
        {
            // not straight: if halfDx==0 => return
            if (halfDx2 == 0)
            {
                goto RETURN_RESULT;
            }

            // then must also have halfDy!=0
            if (halfDy2 == 0)
            {
                goto RETURN_RESULT;
            }

            i = i + 1;
            dx = halfDx2;
            dy = halfDy2;
            goto TRY_ADVANCE;
        }

    DECIDE_FINAL_OBSTACLE:
        // LAB_8003799C:
        if (modX != 0)
        {
            goto FINALIZE_COMMON;
        }

        if (didAdjustForObstacle == 1 || (entity.Flags & 0x2000) != 0 || candidate != null)
        {
            goto FINAL_OBSTACLE;
        }

        didAdjustForObstacle = 1;

        uint dir = entity.TargetDirection;
        if (dir >= 0x20)
        {
            goto FINAL_OBSTACLE;
        }

        switch ((int)dir)
        {
            case 0:
                if ((collisionFlags[2] != 0 && collisionFlags[3] != 0) ||
                    collisionFlags[0] != 0 || collisionFlags[1] != 0)
                {
                    goto FINAL_OBSTACLE;
                }

                entity.FinalForceY = 0;

                if (collisionFlags[2] != 0 && collisionFlags[3] == 0)
                {
                    entity.FinalForceX = 0xC000;
                }
                else if (collisionFlags[2] == 0 && collisionFlags[3] != 0)
                {
                    entity.FinalForceX = -0xC000;
                }

                goto START_COLLISION_CHECK;

            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
                if (collisionFlags[0] != 0)
                {
                    if (collisionFlags[3] == 0)
                    {
                        entity.FinalForceX = 0; // LAB_80037C70
                        goto START_COLLISION_CHECK;
                    }
                    goto FINAL_OBSTACLE;
                }

                if (collisionFlags[3] != 0)
                {
                    entity.FinalForceY = 0; // LAB_80037C88
                }

                goto START_COLLISION_CHECK;

            case 8:
                if ((collisionFlags[0] != 0 && collisionFlags[2] != 0) ||
                    collisionFlags[1] != 0 || collisionFlags[3] != 0)
                {
                    goto FINAL_OBSTACLE;
                }

                entity.FinalForceX = 0;

                if (collisionFlags[0] != 0 && collisionFlags[2] == 0)
                {
                    entity.FinalForceY = 0x8000;
                    goto START_COLLISION_CHECK;
                }

                if (collisionFlags[0] == 0 && collisionFlags[2] != 0)
                {
                    entity.FinalForceY = -0x8000;
                    goto START_COLLISION_CHECK;
                }

                goto START_COLLISION_CHECK;

            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
                if (collisionFlags[1] == 0)
                {
                    if (collisionFlags[2] != 0)
                    {
                        entity.FinalForceX = 0;
                    }
                }
                else
                {
                    if (collisionFlags[2] != 0)
                    {
                        goto FINAL_OBSTACLE;
                    }

                    entity.FinalForceY = 0;
                }

                goto START_COLLISION_CHECK;

            case 16:
                if ((collisionFlags[0] != 0 && collisionFlags[1] != 0) ||
                    collisionFlags[2] != 0 || collisionFlags[3] != 0)
                {
                    goto FINAL_OBSTACLE;
                }

                entity.FinalForceY = 0;

                if (collisionFlags[0] != 0 && collisionFlags[1] == 0)
                {
                    entity.FinalForceX = 0xC000;
                }
                else if (collisionFlags[0] == 0 && collisionFlags[1] != 0)
                {
                    entity.FinalForceX = -0xC000;
                }

                goto START_COLLISION_CHECK;

            case 17:
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
                if (collisionFlags[0] != 0 && collisionFlags[3] != 0)
                {
                    goto FINAL_OBSTACLE;
                }

                if (collisionFlags[3] == 0)
                {
                    if (collisionFlags[0] != 0)
                    {
                        entity.FinalForceY = 0;
                    }
                }
                else
                {
                    entity.FinalForceX = 0;
                    goto START_COLLISION_CHECK;
                }

                goto START_COLLISION_CHECK;

            case 24:
                if ((collisionFlags[1] != 0 && collisionFlags[3] != 0) ||
                    collisionFlags[0] != 0 || collisionFlags[2] != 0)
                {
                    goto FINAL_OBSTACLE;
                }

                entity.FinalForceX = 0;

                if (collisionFlags[1] != 0 && collisionFlags[3] == 0)
                {
                    entity.FinalForceY = 0x8000;
                    goto START_COLLISION_CHECK;
                }

                if (collisionFlags[1] == 0 && collisionFlags[3] != 0)
                {
                    entity.FinalForceY = -0x8000;
                    goto START_COLLISION_CHECK;
                }

                goto START_COLLISION_CHECK;

            case 25:
            case 26:
            case 27:
            case 28:
            case 29:
            case 30:
            case 31:
                if (collisionFlags[1] != 0 && collisionFlags[2] != 0)
                {
                    goto FINAL_OBSTACLE;
                }

                if (collisionFlags[2] != 0)
                {
                    entity.FinalForceY = 0;
                }

                if (collisionFlags[1] != 0)
                {
                    entity.FinalForceX = 0;
                }

                goto START_COLLISION_CHECK;

            default:
                goto FINAL_OBSTACLE;
        }

    FINAL_OBSTACLE:
        entity.ForceAdjusted = 1;
        goto FINALIZE_COMMON;

    FINALIZE_COMMON:
        entity.ModdedPosX = entity.PosX + entity.ModX;
        entity.ModdedPosY = entity.PosY + entity.ModY;
        entity.ModdedPosZ = entity.PosZ + entity.ModZ;

        entity.TerrainHeight = ComputeEntityGroundHeight(entity, gameEngine);
        RecordPlayerXYMoveSnapshot(
            entity,
            gameEngine,
            "FinalizeCommon",
            entryPosX,
            entryPosY,
            entryPosZ,
            entryFinalForceX,
            entryFinalForceY,
            attemptedPosX,
            attemptedPosY,
            attemptedPosZ,
            attemptedGroundHeight,
            collisionFlagsOr,
            collisionFlags,
            candidateIndex,
            result,
            i,
            didAdjustForObstacle,
            modX);
        return result;

    FINALIZE_NO_MOVE:
        result = null;

        entity.ModdedPosX = entity.PosX + entity.ModX;
        entity.ModdedPosY = entity.PosY + entity.ModY;
        entity.ModdedPosZ = entity.PosZ + entity.ModZ;

        entity.TerrainHeight = ComputeEntityGroundHeight(entity, gameEngine);
        RecordPlayerXYMoveSnapshot(
            entity,
            gameEngine,
            "FinalizeNoMove",
            entryPosX,
            entryPosY,
            entryPosZ,
            entryFinalForceX,
            entryFinalForceY,
            attemptedPosX,
            attemptedPosY,
            attemptedPosZ,
            attemptedGroundHeight,
            collisionFlagsOr,
            collisionFlags,
            candidateIndex,
            result,
            i,
            didAdjustForObstacle,
            modX);
        return result;

    RETURN_RESULT:
        RecordPlayerXYMoveSnapshot(
            entity,
            gameEngine,
            "ReturnResult",
            entryPosX,
            entryPosY,
            entryPosZ,
            entryFinalForceX,
            entryFinalForceY,
            attemptedPosX,
            attemptedPosY,
            attemptedPosZ,
            attemptedGroundHeight,
            collisionFlagsOr,
            collisionFlags,
            candidateIndex,
            result,
            i,
            didAdjustForObstacle,
            modX);
        return result;
    }

    // JUSTIFICATION: backend MonoGame only
    private static void RecordPlayerXYMoveSnapshot(
        Entity entity,
        GameEngine gameEngine,
        string exitPath,
        int entryPosX,
        int entryPosY,
        int entryPosZ,
        int entryFinalForceX,
        int entryFinalForceY,
        int attemptedPosX,
        int attemptedPosY,
        int attemptedPosZ,
        int attemptedGroundHeight,
        uint collisionFlagsOr,
        uint[] collisionFlags,
        int candidateIndex,
        Entity? result,
        int iterationCount,
        int didAdjustForObstacle,
        int modX)
    {
        if (entity != gameEngine.StaticVariables.PlayerEntity || gameEngine.RuntimeInspector == null)
        {
            return;
        }

        gameEngine.RuntimeInspector.RecordPlayerXYMoveSnapshot(new RuntimePlayerXYMoveSnapshot
        {
            Frame = gameEngine.StaticVariables.FrameNumber,
            StartPosX = entryPosX,
            StartPosY = entryPosY,
            StartPosZ = entryPosZ,
            EntryFinalForceX = entryFinalForceX,
            EntryFinalForceY = entryFinalForceY,
            AttemptedPosX = attemptedPosX,
            AttemptedPosY = attemptedPosY,
            AttemptedPosZ = attemptedPosZ,
            AttemptedGroundHeight = attemptedGroundHeight,
            ExitPosX = entity.PosX,
            ExitPosY = entity.PosY,
            ExitPosZ = entity.PosZ,
            ExitTerrainHeight = entity.TerrainHeight,
            CollisionFlagsOr = collisionFlagsOr,
            CollisionFlags = collisionFlags.ToArray(),
            CandidateIndex = candidateIndex,
            ResultIndex = result?.Index ?? -1,
            IterationCount = iterationCount,
            DidAdjustForObstacle = didAdjustForObstacle,
            ModXState = modX,
            ExitPath = exitPath,
        });
    }


    // 800370c4
    public static int ComputeEntityGroundHeight(Entity entity, GameEngine gameEngine)
    {
        var xs = new int[4];
        var ys = new int[4];
        var x1 = (entity.PosX + entity.ModX) >> 16;
        var x2 = (entity.PosX + entity.ModX + entity.Width) >> 16;
        var y1 = (entity.PosY + entity.ModY) >> 16;
        var y2 = (entity.PosY + entity.ModY + entity.Height) >> 16;
        xs[0] = x1;
        ys[0] = y1;
        xs[1] = x2;
        ys[1] = y1;
        xs[2] = x1;
        ys[2] = y2;
        xs[3] = x2;
        ys[3] = y2;
        int highest = 0;
        var slopesHit = 0;

        for (var i = 0; i < 4; i++)
        {
            var x = xs[i];
            var y = ys[i];
            var tileX = Math.Min(x / StaticVariables.MapTileWidth, 51);
            //On PSX hardware we avoid using division, so we use a lookup table instead.
            //x = Math.Clamp(x, 0, gameEngine.StaticVariables.g_tileToWorldXTable.Length - 1);
            //Debug.Assert(gameEngine.StaticVariables.g_tileToWorldXTable[x] == tileX); 

            tileX = Math.Clamp(tileX, 0, 0x33);
            var tileY = y / StaticVariables.MapTileHeight;
            tileY = Math.Clamp(tileY, 0, 0x3b);

            var mapWidth = gameEngine.CurrentMap.Map.Width;
            var tile = gameEngine.CurrentMap.Map.MapTiles[tileY * mapWidth + tileX];
            entity.MapTiles[i] = tile;
            int height = tile.Height;

            /*
            for (int j = 0; j < gameEngine.CurrentMap.Map.MapTiles.Length; j++)
            {
                if (gameEngine.CurrentMap.Map.MapTiles[j].Flags == 84180992)
                {
                    AlundraEngine.Debug.Debugger.Breakpoint();
                }

                if (gameEngine.CurrentMap.Map.MapTiles[j].GroundProperty == 128)
                {
                    AlundraEngine.Debug.Debugger.Breakpoint();
                }
            }*/

            height = tile.Height * StaticVariables.MapTileHeight;

            switch (tile.Slope & 0x3)
            {
                case 0: //normal tile
                    break;

                case 1: //Stairs up/down
                    if ((slopesHit & 0x6) == 0)
                    {
                        int yInTile = ys[i];
                        int yMod = yInTile % StaticVariables.MapTileHeight;
                        height = (tile.Height - 1) * StaticVariables.MapTileHeight + StaticVariables.MapTileHeight - yMod;
                    }
                    else
                    {
                        //AlundraEngine.Debug.Debugger.Breakpoint();
                        height += StaticVariables.MapTileHeight;
                    }

                    slopesHit |= 1;
                    break;

                case 2: //ladders entering or stair side down
                    if ((slopesHit & 0x5) == 0)
                    {
                        int xPos = xs[i];
                        int xIndex = (23 - xs[i] % StaticVariables.MapTileWidth) % StaticVariables.MapTileWidth;
                        height = (tile.Height - 1) * StaticVariables.MapTileHeight + gameEngine.StaticVariables.g_heights_800236d4[xIndex];
                    }
                    else
                    {
                        //AlundraEngine.Debug.Debugger.Breakpoint();
                        height += StaticVariables.MapTileHeight;
                    }

                    slopesHit |= 2;
                    break;

                case 3: //ladders exiting
                    if ((slopesHit & 0x3) == 0)
                    {
                        int xPos = xs[i];
                        int remainder = xPos % StaticVariables.MapTileWidth;
                        height = (tile.Height - 1) * StaticVariables.MapTileHeight + gameEngine.StaticVariables.g_heights_800236d4[remainder];
                    }
                    else
                    {
                        //AlundraEngine.Debug.Debugger.Breakpoint();
                        height += StaticVariables.MapTileHeight;
                    }

                    slopesHit |= 4;
                    break;
            }

            height <<= 16;
            entity.MapHeights[i] = height;

            if (highest < height)
            {
                highest = height;
            }
        }

        return highest;
    }

    // GHIDRA: GetCollisionFlagsWithPlayer @ 0x80037488
    public static uint GetCollisionFlagsWithPlayer(Entity entity, uint[] collisionFlags, GameEngine gameEngine)
    {
        //Disable collision
        if ((gameEngine.StaticVariables.g_debugState & 0x80000000) != 0)
        {
            return 0;
        }

        Entity player = gameEngine.StaticVariables.PlayerEntity;
        uint flag;

        if ((player.Flags & 0x8) != 0)
        {
            flag = 0x41;
        }
        else
        {
            flag = 0x40;
        }

        if ((player.Flags & 0x1) != 0)
        {
            flag |= 0x1000;
        }

        int moddedZPos = player.ModdedPosZ;
        int lockTimer = gameEngine.StaticVariables.g_warpLockTimer;

        for (int i = 0; i < 4; i++)
        {
            var tileFlags = player.MapTiles[i].Walkability | (player.MapTiles[i].GroundProperty << 8);

            if ((tileFlags & flag) != 0 || player.MapHeights[i] >= moddedZPos)
            {
                collisionFlags[i] = 1;
            }

            if (gameEngine.StaticVariables.g_gravityFlag < 2
                && (tileFlags & 0xE00) == 0x0800)
            {
                collisionFlags[i] = 1;
            }

            if (lockTimer == 0x20
                && (moddedZPos != player.MapHeights[i] + 1
                    || (player.MapTiles[i].Flags & 0xe00) != 0x600))
            {
                collisionFlags[i] = 1;
            }
        }

        uint flags =
            collisionFlags[0] |
            collisionFlags[1] |
            collisionFlags[2] |
            collisionFlags[3];

        return flags;
    }

    // 800373e4
    public static uint GetCollisionFlags(Entity entity, uint[] flags)
    {
        var flag = 0x40;

        if ((entity.Flags & 0x8) != 0) // 0x8 = � traverse cliff ? �
        {
            flag = 0x41;
        }

        if ((entity.Flags & 0x1) != 0) // 0x1 = hole
        {
            flag |= 0x1000;
        }

        var moddedZPos = entity.ModdedPosZ;

        for (int i = 0; i < 4; i++)
        {
            flags[i] = 0;
            var tile = entity.MapTiles[i];
            var tileFlag = tile.Walkability | (tile.GroundProperty << 8);

            if ((tileFlag & flag) != 0 || entity.MapHeights[i] >= moddedZPos) // la case est plus basse
            {
                flags[i] = 1;
            }
        }

        return flags[0] | flags[1] | flags[2] | flags[3];
    }

    // GHIDRA: FindEntityCollisionCandidate @ 0x80036F34
    public static Entity? FindEntityCollisionCandidate(Entity entity, GameEngine gameEngine)
    {
        int value;
        Entity currentEntity;
        Entity[] collideableEntities;

        //Disable collision
        if (entity == gameEngine.StaticVariables.PlayerEntity
            && (gameEngine.StaticVariables.g_debugState & 0x80000000) != 0
            && (gameEngine.StaticVariables.g_debugFlags & 0x80000000) != 0)
        {
            return null;
        }

        if ((entity.Flags & 0x80U) != 0
            && (entity.AnimFlags & 0x80U) == 0
            && entity.PlatformEntity == null
            && gameEngine.StaticVariables.g_collideableEntitiesCount > 0)
        {
            collideableEntities = gameEngine.StaticVariables.g_collideableEntities;

            for (int i = 0; i < gameEngine.StaticVariables.g_collideableEntitiesCount; i++)
            {
                currentEntity = collideableEntities[i];

                if (entity == currentEntity)
                {
                    continue;
                }

                value = currentEntity.ModdedPosX - entity.ModdedPosX;
                if (value < 0)
                {
                    if (entity.ModdedPosX - currentEntity.ModdedPosX < currentEntity.Width + 1)
                    {
                        value = currentEntity.ModdedPosY - entity.ModdedPosY;
                        if (value < 0)
                        {
                            if (entity.ModdedPosY - currentEntity.ModdedPosY < currentEntity.Height + 1)
                            {
                                value = currentEntity.ModdedPosZ - entity.ModdedPosZ;
                                if (value < 0)
                                {
                                    if (entity.ModdedPosZ - currentEntity.ModdedPosZ < currentEntity.Depth + 1)
                                    {
                                        return currentEntity;
                                    }
                                }
                                else if (value < entity.Depth + 1)
                                {
                                    return currentEntity;
                                }
                            }
                        }
                        else if (value < entity.Height + 1)
                        {
                            value = currentEntity.ModdedPosZ - entity.ModdedPosZ;
                            if (value < 0)
                            {
                                if (entity.ModdedPosZ - currentEntity.ModdedPosZ < currentEntity.Depth + 1)
                                {
                                    return currentEntity;
                                }
                            }
                            else if (value < entity.Depth + 1)
                            {
                                return currentEntity;
                            }
                        }
                    }
                }
                else if (value < entity.Width + 1)
                {
                    value = currentEntity.ModdedPosY - entity.ModdedPosY;
                    if (value < 0)
                    {
                        if (entity.ModdedPosY - currentEntity.ModdedPosY < currentEntity.Height + 1)
                        {
                            value = currentEntity.ModdedPosZ - entity.ModdedPosZ;
                            if (value < 0)
                            {
                                if (entity.ModdedPosZ - currentEntity.ModdedPosZ < currentEntity.Depth + 1)
                                {
                                    return currentEntity;
                                }
                            }
                            else if (value < entity.Depth + 1)
                            {
                                return currentEntity;
                            }
                        }
                    }
                    else if (value < entity.Height + 1)
                    {
                        value = currentEntity.ModdedPosZ - entity.ModdedPosZ;
                        if (value < 0)
                        {
                            if (entity.ModdedPosZ - currentEntity.ModdedPosZ < currentEntity.Depth + 1)
                            {
                                return currentEntity;
                            }
                        }
                        else if (value < entity.Depth + 1)
                        {
                            return currentEntity;
                        }
                    }
                }
            }
        }

        return null;
    }

    // 800364c8
    public static void CheckRidingEntities(GameEngine gameEngine)
    {
        for (var i = 0; i < gameEngine.StaticVariables.g_collideableEntitiesCount; i++)
        {
            var entity = gameEngine.StaticVariables.g_collideableEntities[i];
            
            if ((entity.Flags & 0x4100) != 0x0100)
            {
                continue;
            }

            int entityModdedXPos = entity.ModdedPosX;
            int entityModdedYPos = entity.ModdedPosY;
            int entityWidth = entity.Width + 1;
            int entityDepth = entity.Depth + 1;

            entity.RidingEntity = null;

            for (var j = 0; j < gameEngine.StaticVariables.g_collideableEntitiesCount; j++)
            {
                if (i == j)
                {
                    continue;
                }

                var other = gameEngine.StaticVariables.g_collideableEntities[j];

                // Z overlap
                int otherTopZ = other.ModdedPosZ + other.Depth + 1;
                if (otherTopZ != entity.ModdedPosZ)
                {
                    continue;
                }

                // X overlap
                int xDiff = other.ModdedPosX - entityModdedXPos;

                if (xDiff < 0)
                {
                    int val = other.Width + 1;

                    if (!(entityModdedXPos - other.ModdedPosX < val))
                    {
                        continue;
                    }
                }
                else
                {
                    if (!(xDiff < entityWidth))
                    {
                        continue;
                    }
                }

                // Y overlap
                int yDiff = other.ModdedPosY - entityModdedYPos;

                if (yDiff < 0)
                {
                    int val = other.Height + 1;

                    if (!(entityModdedYPos - other.ModdedPosY < val))
                    {
                        continue;
                    }
                }
                else
                {
                    if (!(yDiff < entityDepth))
                    {
                        continue;
                    }
                }

                entity.RidingEntity = other;
                break;
            }
        }
    }

    // 80036828
    public static void UpdateEntitiesForces(GameEngine gameEngine)
    {
        int spriteZForceTemp;
        int zForceMax;
        uint targetXForce;
        uint xForce;
        uint targetYForce;
        uint yForce;
        int spriteZForceAbs;

        for (int i = 0; i < gameEngine.StaticVariables.g_activeEntityCount; i++)
        {
            var entity = gameEngine.StaticVariables.g_activeEntities[i];

            if (entity == gameEngine.StaticVariables.PlayerEntity)
            {
                if (entity.IsZForceApplied == 0)
                {
                    if ((entity.Flags & 0x100U) != 0)
                    {
                        spriteZForceTemp = entity.ForceZ + gameEngine.CurrentMap.Info.Gravity * -0x100;

                        spriteZForceAbs = spriteZForceTemp;
                        if (spriteZForceTemp < 0)
                        {
                            spriteZForceAbs = -spriteZForceTemp;
                        }

                        zForceMax = gameEngine.CurrentMap.Info.ZViscosity * 0x100;
                        entity.ForceZ = spriteZForceTemp;
                        if (zForceMax < spriteZForceAbs)
                        {
                            entity.ForceZ = spriteZForceTemp < 0
                                ? gameEngine.CurrentMap.Info.ZViscosity * -0x100
                                : gameEngine.CurrentMap.Info.ZViscosity * 0x100;
                        }
                    }
                }
                else if ((entity.Flags & 0x100U) == 0
                         || (entity.CombinedVramFlagsOR & 0x10U) == 0
                         || 0 < gameEngine.StaticVariables.g_gravityFlag)
                {
                    entity.ForceZ = entity.IsZForceApplied << 8;
                }
                else
                {
                    entity.ForceZ = entity.IsZForceApplied * 0xa0;
                }

                UpdateEntityPhysics(entity, gameEngine);

                xForce = (uint)entity.ForceStepX;
                yForce = (uint)entity.ForceStepY;

                if ((entity.CombinedVramFlagsOR & 0x20U) != 0)
                {
                    xForce =
                        (uint)(((ulong)entity.ForceStepX * 0x1000) >> 0x10) |
                        (uint)(((long)entity.ForceStepX * 0x1000) >> 0x20) << 0x10;

                    yForce =
                        (uint)(((ulong)entity.ForceStepY * 0x1000) >> 0x10) |
                        (uint)(((long)entity.ForceStepY * 0x1000) >> 0x20) << 0x10;
                }

                targetXForce = (uint)entity.TargetForceX;
                targetYForce = (uint)entity.TargetForceY;

                if ((entity.CombinedVramFlagsOR & 8U) != 0
                    && gameEngine.StaticVariables.g_gravityFlag < 1)
                {
                    targetXForce =
                        (uint)(((long)entity.TargetForceX * 0x8000) >> 0x10) |
                        (uint)(((long)entity.TargetForceX * 0x8000) >> 0x20) << 0x10;

                    targetYForce =
                        (uint)(((long)entity.TargetForceY * 0x8000) >> 0x10) |
                        (uint)(((long)entity.TargetForceY * 0x8000) >> 0x20) << 0x10;
                }

                entity.ForceX = IncrementForce(entity.ForceX, (int)targetXForce, (int)xForce);
                entity.ForceY = IncrementForce(entity.ForceY, (int)targetYForce, (int)yForce);

                //LABEL_ProcessFinalForces:
                ApplyEntityForces(entity, gameEngine);
                entity.FinalForceX = entity.AdjustedForceX;
                entity.FinalForceY = entity.AdjustedForceY;
                entity.FinalForceZ = entity.ForceZ;
            }
            else
            {
                if (entity.PlatformEntity == null)
                {
                    if (entity.IsZForceApplied == 0)
                    {
                        if ((entity.Flags & 0x100U) != 0)
                        {
                            var force = entity.ForceZ - (gameEngine.CurrentMap.Info.Gravity << 8);
                            var forceAbs = force;
                            if (force < 0)
                            {
                                forceAbs = -force;
                            }

                            var terminal = gameEngine.CurrentMap.Info.ZViscosity << 8;
                            if (terminal < forceAbs && force < 1)
                            {
                                force = -terminal;
                            }

                            entity.ForceZ = force;
                        }
                    }
                    else if ((short)entity.IsZForceApplied == -0x8000
                             && (entity.Flags & 0x100U) == 0)
                    {
                        entity.ForceZ = 0;
                    }
                    else
                    {
                        entity.ForceZ = entity.IsZForceApplied << 8;
                    }

                    UpdateEntityPhysics(entity, gameEngine);

                    entity.ForceX = IncrementForce(entity.ForceX, entity.TargetForceX, entity.ForceStepX);
                    entity.ForceY = IncrementForce(entity.ForceY, entity.TargetForceY, entity.ForceStepY);

                    //goto LABEL_ProcessFinalForces;
                    ApplyEntityForces(entity, gameEngine);
                    entity.FinalForceX = entity.AdjustedForceX;
                    entity.FinalForceY = entity.AdjustedForceY;
                    entity.FinalForceZ = entity.ForceZ;
                    continue;
                }

                entity.ForceZ = 0;
                entity.ForceY = 0;
                entity.ForceX = 0;
                entity.AdjustedForceY = 0;
                entity.AdjustedForceX = 0;
                entity.FinalForceZ = 0;
                entity.FinalForceY = 0;
                entity.FinalForceX = 0;
            }
        }
    }

    // 800366fc
    public static void ApplyEntityForces(Entity entity, GameEngine gameEngine)
    {
        var shiftAmount = gameEngine.CurrentMap.Info.SlideEffectId & 0x1f; //Gravity
        var index = entity.TileAttributes & 0xf;
        var xForceComponent = entity.ForceX + (ScriptHelper.XForceTable[index] >> shiftAmount) + entity.PreviousAdjustedForceX;
        var yForceComponent = entity.ForceY + (ScriptHelper.YForceTable[index] >> shiftAmount) + entity.PreviousAdjustedForceY;

        entity.PreviousAdjustedForceY = 0;
        entity.PreviousAdjustedForceX = 0;

        if (entity.PosX + xForceComponent < entity.NegModX)
        {
            xForceComponent = entity.NegModX - entity.PosX;
            entity.ForceAdjusted = 1;
        }
        else if (entity.ScreenClipX < entity.PosX + xForceComponent)
        {
            xForceComponent = entity.ScreenClipX - entity.PosX;
            entity.ForceAdjusted = 1;
        }

        if (entity.PosY + yForceComponent < entity.NegModY)
        {
            yForceComponent = entity.NegModY - entity.PosY;
            entity.ForceAdjusted = 1;
        }
        else if (entity.ScreenClipY < entity.PosY + yForceComponent)
        {
            yForceComponent = entity.ScreenClipY - entity.PosY;
            entity.ForceAdjusted = 1;
        }

        entity.AdjustedForceX = (int)xForceComponent;
        entity.AdjustedForceY = (int)yForceComponent;
    }

    // 800367e4
    public static int IncrementForce(int force, int targetForce, int step)
    {
        if (targetForce != force)
        {
            if (force < targetForce)
            {
                force = force + step;

                if (targetForce < force)
                {
                    return targetForce;
                }
            }
            else
            {
                force = force - step;

                if (force < targetForce)
                {
                    return targetForce;
                }
            }
        }

        return force;
    }

    // 80036614
    public static void UpdateEntityPhysics(Entity entity, GameEngine gameEngine)
    {
        if (entity.Speed == entity.AnimationSet.Speed
            && entity.TargetDirection == entity.CurrentDirection
            && entity.Acceleration == (entity.AnimationSet.Acceleration & 0xf))
        {
            return;
        }

        entity.CurrentDirection = entity.TargetDirection;

        entity.Speed = entity.AnimationSet.Speed;
        entity.Acceleration = entity.AnimationSet.Acceleration & 0xf;

        entity.TargetForceX = gameEngine.StaticVariables.g_offsetXList[entity.TargetDirection] * entity.AnimationSet.Speed;
        entity.TargetForceY = gameEngine.StaticVariables.g_offsetYList[entity.TargetDirection] * entity.AnimationSet.Speed;

        entity.ForceStepX = Math.Abs(entity.TargetForceX - entity.ForceX) >> entity.Acceleration;
        entity.ForceStepY = Math.Abs(entity.TargetForceY - entity.ForceY) >> entity.Acceleration;
    }


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

            var otherEntityZTop = otherEntity.ModdedPosZ + otherEntity.Depth;

            if (otherEntityZTop >= entity.ModdedPosZ || otherEntityZTop < collision)
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
                if (otherEntity.ModdedPosY - entity.ModdedPosY < entity.Height + 1)
                {
                    collision = otherEntityZTop + 1;
                }
            }
            else
            {
                if (entity.ModdedPosY - otherEntity.ModdedPosY < otherEntity.Height + 1)
                {
                    collision = otherEntityZTop + 1;
                }
            }

        }

        return collision;
    }

    // 80038064
    public static void UpdateTileAttributes(Entity entity, GameEngine gameEngine)
    {
        int tileX;
        int tileY;
        uint tileAttr;
        uint tileFlags;
        uint bestFlagMask;
        uint[] tempFlags = new uint[4];

        if (entity.FrameCollision != null)
        {
            entity.HitBoxX = entity.PosX + entity.CollisionOffsetX;
            entity.HitBoxY = entity.PosY + entity.CollisionOffsetY;
            entity.HitBoxZ = entity.PosZ + entity.CollisionOffsetZ;
        }

        //var x = entity.PosX >> 16;
        //tileX = Math.Min((x / StaticVariables.MapTileWidth), 51);
        ////On PSX hardware we avoid using division, so we use a lookup table instead.
        //x = Math.Clamp(x, 0, _gameEngine.StaticVariables.g_tileToWorldXTable.Length - 1);
        entity.TileX = (entity.PosX >> 16) / StaticVariables.MapTileWidth;
        entity.TileY = (entity.PosY >> 16) / StaticVariables.MapTileHeight;
        entity.TileZ = entity.PosZ >> 20;

        var hitz = PhysicsEngine.GetCollisionOnZ(gameEngine, entity);
        entity.FloorHeight = hitz;
        entity.IsOnGround = hitz < entity.PosZ ? 0 : 1;

        if ((entity.Flags & 0x100U) == 0)
        {
            tileX = entity.TileX;
            bestFlagMask = 0;
            entity.CombinedVramFlagsOR = 0;
            entity.CombinedVramFlagsAND = 0;

            if (tileX < 1)
            {
                tileX = 0;
            }
            else if (0x33 < tileX)
            {
                tileX = 0x33;
            }

            tileY = entity.TileY;

            if (tileY < 1)
            {
                tileY = 0;
            }
            else if (0x3b < tileY)
            {
                tileY = 0x3b;
            }

            var mapTile = gameEngine.CurrentMap.Map.MapTiles[tileX + tileY * gameEngine.CurrentMap.Map.Width];
            tileFlags = mapTile.Flags;
            if ((tileFlags & 0xc00000) == 0 || (tileFlags & 0x80000) == 0)
            {
                goto NoCollision;
            }
        }
        else
        {
            bestFlagMask = 0xe00;
            var i = 0;

            do
            {
                tileFlags = entity.MapTiles[i].Flags;

                if (entity.MapHeights[i] + 1 == entity.ModdedPosZ)
                {
                    tempFlags[i] = entity.MapTiles[i].Flags;

                    if ((tileFlags & 0xe00) < bestFlagMask)
                    {
                        bestFlagMask = tileFlags & 0xe00;
                    }
                }
                else
                {
                    tempFlags[i] = 0;
                    bestFlagMask = 0;
                }

                i += 1;
            } while (i < 4);

            entity.CombinedVramFlagsOR = tempFlags[0] | tempFlags[1] | tempFlags[2] | tempFlags[3];
            entity.CombinedVramFlagsAND = tempFlags[0] & tempFlags[1] & tempFlags[2] & tempFlags[3];
            tileX = entity.TileX;

            if (tileX < 1)
            {
                tileX = 0;
            }
            else if (0x33 < tileX)
            {
                tileX = 0x33;
            }

            tileY = entity.TileY;

            if (tileY < 1)
            {
                tileY = 0;
            }
            else if (0x3b < tileY)
            {
                tileY = 0x3b;
            }

            var tile = gameEngine.CurrentMap.Map.MapTiles[tileX + tileY * gameEngine.CurrentMap.Map.Width];
            tileFlags = (uint)(tile.Walkability | tile.GroundProperty << 8 | tile.Slope << 16 | tile.Height << 24);

            if ((tileFlags & 0xc00000) == 0)
            {
                goto NoCollision;
            }

            tileAttr = 0x40000;
            if (((tileFlags & 0xff000000) >> 4) + 1 != entity.ModdedPosZ)
            {
                tileAttr = 0x80000;
            }

            if ((tileFlags & tileAttr) == 0)
            {
                goto NoCollision;
            }
        }

        tileAttr = 0x1U << ((int)(tileFlags >> 0x14) & 0x3);
        entity.TileAttributes = (int)tileAttr;
        if ((tileFlags & 0x800000) != 0)
        {
            entity.TileAttributes = (int)(tileAttr | 0x80);
        }

    FinishUpdate:
        entity.Slope_190 = entity.Slope_18c;
        entity.Slope_18c = (int)bestFlagMask >> 9;
        return;

    NoCollision:
        entity.TileAttributes = 0;
        goto FinishUpdate;
    }

}