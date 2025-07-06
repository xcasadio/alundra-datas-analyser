using AlundraEngine.Gameplay;
using AlundraEngine.Gameplay.Scripts;

namespace AlundraEngine;

public class EntityGameplayManager
{
    private GameEngine _gameEngine;

    public EntityGameplayManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    public void HideEntity(Entity entity)
    {
        entity.Status = 4;
        entity.EventTrigger = -1;
        if (entity.ActiveEffect != null)
        {
            entity.ActiveEffect.Status = 0;
            entity.ActiveEffect = null;
        }
        if (entity.PlatformEntity != null)
        {
            entity.PlatformEntity.WarpEntity = null;
        }
    }

    public uint TurnEntity(Entity entity, int turnCode)
    {
        var turndir = turnCode & 0x1f;
        var turntype = turnCode >> 5;
        if (turntype >= 8)
        {
            return 0;
        }

        switch (turntype)
        {
            case 1:
                return (uint)((entity.TargetDirection + turndir) & 0x1f);
            case 2:
                return (uint)StaticVariables.g_cardinalDirectionTable[turndir & 0x3];
            case 3:
                var dfv = ScriptHelper.GetDirectionToTarget(StaticVariables.PlayerEntity.PosX - entity.PosX, StaticVariables.PlayerEntity.PosY - entity.PosY);
                return (uint)((dfv + turndir) & 0x1f);
            case 4:
            {
                var i = StaticVariables.g_gameRandomSeed;
                var val1 = (int)(i * 0x7d2b89dd);
                var val2 = (int)(0xe06a02e7 + val1);
                var val3 = (int)(((long)val2 * 4) >> 32);
                StaticVariables.g_gameRandomSeed = (uint)val2;
                var dir = StaticVariables.g_cardinalDirectionTable[val3];//val3 here is a number between 0 and 3
                return (uint)dir;
            }
            case 5:
            {
                var i = StaticVariables.g_gameRandomSeed;
                var val1 = (int)(i * 0x7d2b89dd);
                var val2 = (int)(0xe06a02e7 + val1);
                var val3 = (int)(((long)val2 * 0x20) >> 32);
                StaticVariables.g_gameRandomSeed = (uint)val2;
                return (uint)val2;
            }
            case 6:
                return (uint)((StaticVariables.PlayerEntity.TargetDirection + turndir) & 0x1f);
            case 7:
                var ret = GetCardinalDirToPlayer(entity);
                if (ret != -1)
                {
                    return (uint)((ret + turndir) & 0x1f);
                }

                break;
            case 0:
                break;
        }
        return (uint)turndir;
    }

    public int GetCardinalDirToPlayer(Entity entity)
    {
        if (entity == StaticVariables.g_activeCollisionEntity)
        {
            return -1;
        }

        var difx = StaticVariables.PlayerEntity.ModdedXPos - entity.ModdedXPos;

        if ((difx >= 0 && entity.Width < difx)
            || (difx < 0 && StaticVariables.PlayerEntity.Width < -difx))
        {
            //checkx
            if (StaticVariables.PlayerEntity.PosX < entity.PosX)
            {
                return 0x08;
            }

            return 0x18;
        }

        //checky
        if (StaticVariables.PlayerEntity.PosY < entity.PosY)
        {
            return 0x10;
        }

        return 0x00;
    }

    public void StartFlying(Entity entity, uint animationId, short baseDelay, uint probabilityTargeted)
    {
        StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;

        if ((uint)((ulong)StaticVariables.g_gameRandomSeed * 0x65 >> 32) < (probabilityTargeted & 0xff))
        {
            var direction = (uint)ScriptHelper.GetDirectionToTarget(
                StaticVariables.g_entitySlots[0].PosX - entity.PosX,
                StaticVariables.g_entitySlots[0].PosY - entity.PosY);

            entity.TargetDirection = direction;
        }
        else
        {
            StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
            entity.TargetDirection = (uint)((ulong)StaticVariables.g_gameRandomSeed * 0x20 >> 32);
        }

        entity.TargetAnimationId = animationId & 0xff;

        if (baseDelay != 0)
        {
            StaticVariables.g_gameRandomSeed = StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
            var delay = (short)((ulong)StaticVariables.g_gameRandomSeed * 0x10 >> 32) + baseDelay;
            entity.AIValues.Set(delay, 1);
        }
    }

    public bool TryAttackPlayer(Entity entity, int[] relativePositions, int maxHorizontalRange, int maxVerticalRange)
    {
        int currentFrame;
        bool isWithinRange;

        if (maxVerticalRange < relativePositions[2])
        {
            return false;
        }

        currentFrame = entity.CurrentFrameIndex;

        if (currentFrame == 1)
        {
            if (relativePositions[0] < 2 && -1 < relativePositions[4] &&
                relativePositions[1] <= maxHorizontalRange)
            {
                return true;
            }

            if (1 < relativePositions[1])
            {
                return false;
            }

            isWithinRange = maxHorizontalRange < relativePositions[0];
        }
        else if (currentFrame < 2)
        {
            if (currentFrame != 0)
            {
                return false;
            }

            if (relativePositions[0] < 2 && relativePositions[4] < 1 &&
                relativePositions[1] <= maxHorizontalRange)
            {
                return true;
            }

            if (1 < relativePositions[1])
            {
                return false;
            }

            isWithinRange = maxHorizontalRange < relativePositions[0];
        }
        else
        {
            if (currentFrame == 2)
            {
                if (relativePositions[1] < 2 && -1 < relativePositions[3] &&
                    relativePositions[0] <= maxHorizontalRange)
                {
                    return true;
                }

                if (1 < relativePositions[0])
                {
                    return false;
                }

                if (relativePositions[1] <= maxHorizontalRange)
                {
                    return true;
                }

                return false;
            }

            if (currentFrame != 3)
            {
                return false;
            }

            if (relativePositions[1] < 2 && relativePositions[3] < 1 &&
                relativePositions[0] <= maxHorizontalRange)
            {
                return true;
            }

            if (1 < relativePositions[0])
            {
                return false;
            }

            isWithinRange = maxHorizontalRange < relativePositions[1];
        }

        return !isWithinRange;
    }

    public int HandleAnimationDirection(Entity entity, uint newAnimId, int currentZ)
    {
        int deltaBottomRight;
        int returnValue = 0;
        uint direction = 0;
        int zTest;
        int deltaTopRight;
        int deltaBottomLeft;
        int deltaTopLeft;
        byte newDirection;
        bool needHandle = false;
        bool needSkip = false;
        bool needUpdate = false;

        zTest = currentZ + 1;
        deltaBottomRight = entity.TerrainHeight;
        deltaTopLeft = deltaBottomRight - entity.MapHeights[0];
        deltaTopRight = deltaBottomRight - entity.MapHeights[1];
        deltaBottomLeft = deltaBottomRight - entity.MapHeights[2];
        deltaBottomRight = deltaBottomRight - entity.MapHeights[3];

        if (zTest < deltaTopLeft)
        {
            if (deltaTopRight <= zTest)
            {
                needHandle = true;
            }

            direction = entity.TargetDirection;
            if (0xe < direction - 9)
            {
                needSkip = true;
            }

            needUpdate = true;
        }
        else
        {
            needSkip = true;
        }

        if (needSkip)
        {
            if (zTest < deltaTopRight)
            {
                if (zTest < deltaBottomRight)
                {
                    direction = entity.TargetDirection;
                    if (direction - 0x11 < 0xf)
                    {
                        needUpdate = true;
                    }

                    needHandle = true;
                }
            }
            else
            {
                needHandle = true;
            }

            if (zTest < deltaBottomLeft)
            {
                if (deltaTopLeft <= zTest)
                {
                    return 0;
                }

                direction = entity.TargetDirection;
                if ((int)direction < 0x10)
                {
                    needUpdate = true;
                }
            }

            returnValue = 0;
        }

        if (needUpdate)
        {
            returnValue = 1;
            newDirection = (byte)StaticVariables.g_directionFlipTable[direction];
            entity.TargetAnimationId = newAnimId & 0xff;
            entity.ForceStepY = 0;
            entity.ForceStepX = 0;
            entity.ForceY = 0;
            entity.ForceX = 0;
            entity.TargetYForce = 0;
            entity.TargetXForce = 0;
            entity.TargetDirection = newDirection;
        }

        if (needHandle)
        {
            if (zTest < deltaBottomRight)
            {
                if (deltaBottomLeft <= zTest)
                {
                    return 0;
                }

                direction = entity.TargetDirection;
                if (0x10 < direction - 8)
                {
                    needUpdate = true;
                }
            }
        }

        return returnValue;
    }

    public int UpdateDirectionForced(Entity entity, uint newAnimId, uint fallbackAnimId, int zThreshold)
    {
        int heightDiff;
        byte newDirection;

        heightDiff = GetEntityTileHeight(entity, newAnimId & 0xff, entity.TargetDirection);
        heightDiff = heightDiff - entity.TerrainHeight;

        if (zThreshold < heightDiff || heightDiff < 1)
        {
            newDirection = (byte)StaticVariables.g_directionFlipTable[entity.TargetDirection];
            entity.TargetAnimationId = newAnimId & 0xff;
            entity.ForceStepY = 0;
            entity.ForceStepX = 0;
            entity.ForceY = 0;
            entity.ForceX = 0;
            entity.TargetYForce = 0;
            entity.TargetXForce = 0;
            entity.TargetDirection = newDirection;
        }
        else
        {
            entity.TargetAnimationId = fallbackAnimId & 0xff;
        }

        return heightDiff;
    }

    private int GetEntityTileHeight(Entity entity, uint animIndex, uint direction)
    {
        int height;
        uint stepDistance;

        //stepDistance = entity.SpriteRecord.AnimationOffsetsPointer[animIndex * 0xe + 8];
        stepDistance = entity.Sprite.AnimSets[animIndex].U6; // TODO check which property => flag or acceleration...
        height = GetTileHeightAtOffset(entity,
            StaticVariables.g_offsetXList[direction] * (int)stepDistance,
            StaticVariables.g_offsetYList[direction] * (int)stepDistance);

        return height;
    }

    //8003a9e0
    public int GetTileHeightAtOffset(Entity entity, int offsetX, int offsetY)
    {
        int tileXIndex;
        uint uVar1;
        int tileYIndex;
        int[] xCoords = new int[4];
        int[] yCoords = new int[4];
        int[] xCoordsPtr;
        uint uVar2;
        ushort flagBits;

        xCoordsPtr = xCoords;
        tileXIndex = entity.PosX + entity.ModX + offsetX;
        xCoords[2] = StaticVariables.g_tileToWorldXTable[tileXIndex >> 0x10];
        xCoords[0] = StaticVariables.g_tileToWorldXTable[tileXIndex >> 0x10];

        tileYIndex = entity.PosY + entity.ModY + offsetY;
        yCoords[1] = tileYIndex >> 0x14;
        yCoords[0] = yCoords[1];

        xCoords[3] = StaticVariables.g_tileToWorldXTable[(tileXIndex + entity.Width) >> 0x10];
        xCoords[1] = StaticVariables.g_tileToWorldXTable[(tileXIndex + entity.Width) >> 0x10];

        yCoords[3] = (tileYIndex + entity.Height) >> 0x14;
        yCoords[2] = yCoords[3];

        flagBits = (ushort)((entity.Flags & 8U) != 0 ? 1 : 0);
        if ((entity.Flags & 1U) != 0)
        {
            flagBits |= 0x1000;
        }

        uVar2 = 0;

        for (var coordIndex = 0; coordIndex < 4; coordIndex++)
        {
            tileXIndex = xCoordsPtr[coordIndex];
            if (tileXIndex < 1)
            {
                tileXIndex = 0;
            }
            else if (tileXIndex > 0x33)
            {
                tileXIndex = 0x33;
            }

            tileYIndex = xCoordsPtr[coordIndex];
            if (tileYIndex < 1)
            {
                tileYIndex = 0;
            }
            else if (tileYIndex > 0x3b)
            {
                tileYIndex = 0x3b;
            }

            int tileOffset = tileYIndex * 0xd0 + tileXIndex * 4 + 0x302;

            if ((_gameEngine.CurrentMap.Map.MapTiles[tileOffset].GroundProperty & flagBits) != 0)
                //if ((StaticVariables.g_spriteVRAMPointer[tileOffset] & flagBits) != 0)
            {
                break;
            }

            //uVar1 = StaticVariables.g_spriteVRAMPointer[tileOffset + 3];
            uVar1 = _gameEngine.CurrentMap.Map.MapTiles[tileOffset + 3].GroundProperty;
            if (uVar2 < uVar1)
            {
                uVar2 = uVar1;
            }

            xCoordsPtr = xCoordsPtr.Skip(1).ToArray();
            if (xCoordsPtr.Length == 0)
            {
                return (int)(uVar2 << 0x14);
            }
        }

        return 0x7800000;
    }
}