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
                return (uint)_gameEngine.StaticVariables.g_cardinalDirectionTable[turndir & 0x3];

            case 3:
                var dfv = ScriptHelper.GetDirectionToTarget(_gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, _gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
                return (uint)((dfv + turndir) & 0x1f);

            case 4:
            {
                var i = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed;
                var val1 = (int)(i * 0x7d2b89dd);
                var val2 = (int)(0xe06a02e7 + val1);
                var val3 = (int)(((long)val2 * 4) >> 32);
                _gameEngine.StaticVariables.g_gameRandomSeed = (uint)val2;
                var dir = _gameEngine.StaticVariables.g_cardinalDirectionTable[val3];//val3 here is a number between 0 and 3
                return (uint)dir;
            }

            case 5:
            {
                var i = (ulong)_gameEngine.StaticVariables.g_gameRandomSeed;
                var val1 = (int)(i * 0x7d2b89dd);
                var val2 = (int)(0xe06a02e7 + val1);
                var val3 = (int)(((long)val2 * 0x20) >> 32);
                _gameEngine.StaticVariables.g_gameRandomSeed = (uint)val2;
                return (uint)val2;
            }

            case 6:
                return (uint)((_gameEngine.StaticVariables.PlayerEntity.TargetDirection + turndir) & 0x1f);

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
        if (entity == _gameEngine.StaticVariables.g_activeCollisionEntity)
        {
            return -1;
        }

        var difx = _gameEngine.StaticVariables.PlayerEntity.ModdedPosX - entity.ModdedPosX;

        if ((difx >= 0 && entity.Width < difx)
            || (difx < 0 && _gameEngine.StaticVariables.PlayerEntity.Width < -difx))
        {
            //checkx
            if (_gameEngine.StaticVariables.PlayerEntity.PosX < entity.PosX)
            {
                return 0x08;
            }

            return 0x18;
        }

        //checky
        if (_gameEngine.StaticVariables.PlayerEntity.PosY < entity.PosY)
        {
            return 0x10;
        }

        return 0x00;
    }

    public void StartFlying(Entity entity, uint animationId, short baseDelay, uint probabilityTargeted)
    {
        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;

        if ((uint)(((ulong)(ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x65) >> 32) < (probabilityTargeted & 0xff))
        {
            var direction = (uint)ScriptHelper.GetDirectionToTarget(
                _gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX,
                _gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);

            entity.TargetDirection = direction;
        }
        else
        {
            _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
            entity.TargetDirection = (uint)(((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x20) >> 32);
        }

        entity.TargetAnimationId = animationId & 0xff;

        if (baseDelay != 0)
        {
            _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
            var delay = (short)(((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x10) >> 32) + baseDelay;
            entity.AIValues.Set(delay, 1);
        }
    }

    public bool TryAttackPlayer(Entity entity, int[] relativePositions, int maxHorizontalRange, int maxVerticalRange)
    {
        int animationDirection;
        bool isWithinRange;

        if (maxVerticalRange < relativePositions[2])
        {
            return false;
        }

        animationDirection = entity.AnimationDirection;

        if (animationDirection == 1)
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
        else if (animationDirection < 2)
        {
            if (animationDirection != 0)
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
            if (animationDirection == 2)
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

            if (animationDirection != 3)
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
            newDirection = (byte)_gameEngine.StaticVariables.g_directionFlipTable[direction];
            entity.TargetAnimationId = newAnimId & 0xff;
            entity.ForceStepY = 0;
            entity.ForceStepX = 0;
            entity.ForceY = 0;
            entity.ForceX = 0;
            entity.TargetForceY = 0;
            entity.TargetForceX = 0;
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
            newDirection = (byte)_gameEngine.StaticVariables.g_directionFlipTable[entity.TargetDirection];
            entity.TargetAnimationId = newAnimId & 0xff;
            entity.ForceStepY = 0;
            entity.ForceStepX = 0;
            entity.ForceY = 0;
            entity.ForceX = 0;
            entity.TargetForceY = 0;
            entity.TargetForceX = 0;
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
        stepDistance = entity.SpriteRecord.AnimSets[animIndex].U6; // TODO check which property => flag or acceleration...
        height = GetTileHeightAtOffset(entity,
            _gameEngine.StaticVariables.g_offsetXList[direction] * (int)stepDistance,
            _gameEngine.StaticVariables.g_offsetYList[direction] * (int)stepDistance);

        return height;
    }

    //8003a9e0
    public int GetTileHeightAtOffset(Entity entity, int offsetX, int offsetY)
    {
        int tileX;
        uint uVar1;
        int tileY;
        int[] xCoords = new int[4];
        int[] yCoords = new int[4];
        int[] xCoordsPtr;
        uint maxHeight;
        ushort flagBits;

        tileX = entity.PosX + entity.ModX + offsetX;
        xCoords[2] = _gameEngine.StaticVariables.g_tileToWorldXTable[tileX >> 0x10];
        xCoords[0] = _gameEngine.StaticVariables.g_tileToWorldXTable[tileX >> 0x10];
        xCoords[3] = _gameEngine.StaticVariables.g_tileToWorldXTable[(tileX + entity.Width) >> 0x10];
        xCoords[1] = _gameEngine.StaticVariables.g_tileToWorldXTable[(tileX + entity.Width) >> 0x10];

        tileY = entity.PosY + entity.ModY + offsetY;
        yCoords[1] = tileY >> 0x14;
        yCoords[0] = yCoords[1];
        yCoords[3] = (tileY + entity.Height) >> 0x14;
        yCoords[2] = yCoords[3];

        flagBits = (ushort)((entity.Flags & 8U) != 0 ? 1 : 0);
        if ((entity.Flags & 1U) != 0)
        {
            flagBits |= 0x1000;
        }

        maxHeight = 0;

        for (var coordIndex = 0; coordIndex < 4; coordIndex++)
        {
            tileX = xCoords[coordIndex];
            if (tileX < 1)
            {
                tileX = 0;
            }
            else if (tileX > 0x33)
            {
                tileX = 0x33;
            }

            tileY = yCoords[coordIndex];
            if (tileY < 1)
            {
                tileY = 0;
            }
            else if (tileY > 0x3b)
            {
                tileY = 0x3b;
            }

            var mapWidth = _gameEngine.CurrentMap.Map.Width;
            var tile = _gameEngine.CurrentMap.Map.MapTiles[tileY * mapWidth + tileX];

            if ((tile.Walkability & flagBits) != 0)
            {
                return 0x7800000;
            }

            if (maxHeight < tile.Height)
            {
                maxHeight = tile.Height;
            }
        }
        
        return (int)(maxHeight << 0x14);
    }

    //800805c8
    public bool TryAttackPlayerFront(Entity entity, int[] relativePositions, int xThreshold, int yThreshold, int zThreshold)
    {
        int animationDirection;
        bool isOutOfRange;

        if (zThreshold < relativePositions[2])
        {
            return false;
        }

        animationDirection = entity.AnimationDirection;

        if (animationDirection == 1)
        {
            if (xThreshold < relativePositions[0])
            {
                return false;
            }

            if (relativePositions[4] < 0)
            {
                return false;
            }

            isOutOfRange = yThreshold < relativePositions[1];
        }
        else if (animationDirection < 2)
        {
            if (animationDirection != 0)
            {
                return false;
            }

            if (xThreshold < relativePositions[0])
            {
                return false;
            }

            if (0 < relativePositions[4])
            {
                return false;
            }

            isOutOfRange = yThreshold < relativePositions[1];
        }
        else
        {
            if (animationDirection == 2)
            {
                if (xThreshold < relativePositions[1])
                {
                    return false;
                }

                if (relativePositions[3] < 0)
                {
                    return false;
                }

                if (relativePositions[0] <= yThreshold)
                {
                    return true;
                }
                return false;
            }

            if (animationDirection != 3)
            {
                return false;
            }

            if (xThreshold < relativePositions[1])
            {
                return false;
            }

            if (0 < relativePositions[3])
            {
                return false;
            }

            isOutOfRange = yThreshold < relativePositions[0];
        }

        return !isOutOfRange;
    }
}