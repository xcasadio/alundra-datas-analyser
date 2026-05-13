using System.Diagnostics;
using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;

namespace AlundraEngine.Graphics;

public class EffectManager
{
    private readonly GameEngine _gameEngine;

    public EffectManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    // 8003c1a4
    public void InitializeEffectSlots()
    {
        foreach (var spriteEffect in _gameEngine.StaticVariables.g_effectSlots)
        {
            spriteEffect.Status = 0;
        }

        //TODO check this
        var mapEventsRecords = _gameEngine.CurrentMap.SpriteInfo.MapEvents.Records;

        for (int i = 0; i < mapEventsRecords.Length; i++)
        {
            var mapEventsRecord = mapEventsRecords[i];

            if (mapEventsRecord == null)
            {
                break;
            }

            if (mapEventsRecord.X1 == 0 && mapEventsRecord.X2 == 0 && mapEventsRecord.Y1 == 0 &&
                mapEventsRecord.Y2 == 0)
            {
                break;
            }

            var spriteEffect = SpawnSpriteEffect(i, 0);
            if (spriteEffect == null 
                  && (_gameEngine.StaticVariables.g_debugState & 0x80000000U) != 0
                 && (_gameEngine.StaticVariables.g_debugFlags & 0x20) != 0)
            {
                Breakpoint.TriggerBreak();
            }
        }
    }

    public SpriteEffect SpawnSpriteEffect(int effectId, int checkSpawnArea)
    {
        MapEffectRecord effectStatus;
        SpriteEffect effect;
        byte flags;

        effectStatus = GetMapEffectRecord(effectId, checkSpawnArea == 1);
        effect = null;

        var tileHalfWidth = StaticVariables.MapTileWidth / 2;
        var tileHalfHeight = StaticVariables.MapTileHeight / 2;

        if (effectStatus != null)
        {
            flags = effectStatus.Flags;

            if (checkSpawnArea != 0 || (flags & 0x40) != 0)
            {
                effect = GetFreeEffect();

                if (effect != null)
                {
                    InitializeEffects(
                        effect,
                        effectStatus,
                        effectId,
                        0,
                        flags & 0x80,
                        effectStatus.EffectId,
                        effectStatus.AnimId,
                        (int)(((uint)effectStatus.X * tileHalfWidth + tileHalfWidth) * 0x10000),
                        (int)(((uint)effectStatus.Y * tileHalfHeight + tileHalfHeight) * 0x10000),
                        (int)((uint)effectStatus.Z << 0x13)
                    );
                }
            }
        }

        return effect;
    }

    //8003ba70
    public MapEffectRecord GetMapEffectRecord(int id, bool checkBoundingBox)
    {
        if (id < _gameEngine.CurrentMap.SpriteInfo.MapEffectRecords.Length)
        {
            var record = _gameEngine.CurrentMap.SpriteInfo.MapEffectRecords[id];
            if (checkBoundingBox)
            {
                var playerEntity = _gameEngine.StaticVariables.PlayerEntity;
                if (playerEntity.TileX < record.X1 
                    || playerEntity.TileX > record.X2
                    || playerEntity.TileY < record.Y1 
                    || playerEntity.TileY > record.Y2)
                {
                    return null;
                }
            }

            return record;
        }
        return null;
    }

    //8003c410
    public void UpdateEffects()
    {
        foreach (var effect in _gameEngine.StaticVariables.g_effectSlots)
        {
            if (effect.Status != 2)
            {
                continue;
            }

            if ((_gameEngine.StaticVariables.g_playerControlFlags & 0x48) == 0)
            {
                if (effect.DestroyFlag != 0)
                {
                    effect.Status = 0;
                    continue;
                }

                UpdateEffectAnimation(effect);
                UpdateEffectPosition(effect);
            }

            effect.SpriteRef.DepthSortValue = effect.DepthSortValue;
            effect.SpriteRef.X = effect.X;
            effect.SpriteRef.Y = effect.Y;
            effect.SpriteRef.Z = effect.Z;
            _gameEngine.StaticVariables.g_spriteImages[_gameEngine.StaticVariables.g_spriteNumberOfImage++] = effect.SpriteRef;
        }
    }

    //8003bbdc
    private void UpdateEffectAnimation(SpriteEffect effect)
    {
        if (effect.CurrentSpriteTableIndex != effect.TargetSpriteTableIndex
            || effect.CurrentIsMapSprite != effect.TargetIsMapSprite)
        {
            var record = _gameEngine.GetEffectSpriteFromSpriteTable(
                effect.TargetIsMapSprite != 0,
                effect.TargetSpriteTableIndex,
                out var sheetSize, out var paletteIndex);

            if (record == null)
            {
                effect.DestroyFlag = 1;
                effect.SpriteRef.Images = null;
                effect.SpriteRef.NumberOfImages = 0;
                effect.SpriteRef.ImageDepthSortValue = 0;
                return;
            }

            effect.SpriteEffectRecord = record;
            effect.SheetSize = sheetSize;
            effect.PaletteIndex = paletteIndex;
            effect.CurrentIsMapSprite = effect.TargetIsMapSprite;
            effect.CurrentSpriteTableIndex = effect.TargetSpriteTableIndex;
            effect.CurrentAnimation = (byte)~effect.TargetAnimation;
        }

        var anim = effect.SpriteEffectRecord.PreloadedAnims[effect.TargetAnimation];

        if (effect.CurrentAnimation != effect.TargetAnimation)
        {
            effect.CurrentAnimation = effect.TargetAnimation;
            effect.NextFrameDelay = 0;
            effect.DestroyFlag = 0;

            effect.CurrentFrameIndex = 0;
            effect.Frame = anim.Frames[0];
            effect.FirstFrame = effect.Frame;
        }
        else
        {
            effect.NextFrameDelay--;

            if ((effect.NextFrameDelay & 0xff) != 0)
            {
                return;
            }
        }

        while (true)
        {
            var frameData = anim.Frames[effect.CurrentFrameIndex];

            if ((frameData.Delay & 0x80) != 0)
            {
                effect.NextFrameDelay = (byte)(frameData.Delay & 0x7f);
                effect.CurrentFrameIndex++;

                //Todo fix bug : during the first scene with Lars the effect bugs
                effect.CurrentFrameIndex = Math.Min(effect.CurrentFrameIndex, anim.Frames.Length - 1);

                try
                {
                    effect.Frame = anim.Frames[effect.CurrentFrameIndex];
                }
                catch (Exception e)
                {
                    Breakpoint.TriggerBreak();
                }

                if (frameData.Images != null /*&& effect.Frame.ImageSetPointer != -1*/)
                {
                    effect.SpriteRef.Images = frameData.Images.Images;
                    effect.SpriteRef.ImageDepthSortValue = frameData.Images.DepthSortValue;
                    effect.SpriteRef.NumberOfImages = frameData.Images.NumberOfImages;
                    effect._24 = frameData.Images.NumberOfImages;
                }
                else
                {
                    effect.SpriteRef.Images = null;
                    effect.SpriteRef.ImageDepthSortValue = 0;
                    effect.SpriteRef.NumberOfImages = 0;
                    effect._24 = 0;
                }
                return;
            }

            if (frameData.Delay == 0) //end anim => destroy the effect
            {
                effect.NextFrameDelay = 0xff;
                effect.DestroyFlag = 1;
                return;
            }

            if (frameData.Delay == 1) //loop
            {
                effect.CurrentFrameIndex = 0;
                effect.Frame = effect.FirstFrame;
                continue;
            }

            Breakpoint.TriggerBreak();
            //throw new Exception("Effect Animation Error!!");
        }
    }

    //8003c284
    private void UpdateEffectPosition(SpriteEffect effect)
    {
        if (effect.UpdateMode == 0)
        {
            effect.X += effect.ForceX;
            effect.Y += effect.ForceY;
            effect.Z += effect.ForceZ;
            effect.DepthSortValue = (int)(effect.Y & 0xffff0000) + (effect.Z >> 16) + (effect.SpriteRef.ImageDepthSortValue << 16);
            return;
        }

        if (effect.UpdateMode == 1)
        {
            var entity = effect.AttachedEntity;
            if (entity.Status != 0)
            {
                effect.X = entity.PosX + effect.OffsetX;
                effect.Y = entity.PosY + effect.OffsetY;
                effect.Z = entity.PosZ + effect.OffsetZ;
                effect.DepthSortValue = entity.ZUpperBound + effect.DepthSortOffset;
                if (entity.Status == 4)
                {
                    effect.UpdateMode = 2;
                }
            }
            else
            {
                effect.UpdateMode = 2;
            }
        }
        else if (effect.UpdateMode != 3)
        {
            return;
        }

        //param3== 1 falls through to here, and param3 == 3 is here
        effect.X += effect.ForceX; //forces?
        effect.Y += effect.ForceY;
        effect.Z += effect.ForceZ;

        if (effect.AttachedEntity.Status != 0)
        {
            effect.DepthSortValue = effect.AttachedEntity.ZUpperBound + effect.DepthSortOffset;

            if (effect.AttachedEntity.Status == 4)
            {
                effect.UpdateMode = 2;
            }
        }
        else
        {
            effect.UpdateMode = 2;
        }
    }

    //8003bdd8
    public SpriteEffect CreateEffectEntity(byte ismapeffect, byte effectid, byte animid, int x, int y, int z)
    {
        var effect = GetNextAvailableEffect();

        if (effect != null)
        {
            InitializeEffects(effect, null, -1, 0, ismapeffect, effectid, animid, x, y, z);
            return effect;
        }

        return null;
    }

    //8003be74
    public SpriteEffect CreateAttachedEffect(byte ismapeffect, byte effectid, byte animid, Entity entity, int depthsortmod, int xoff, int yoff, int zoff)
    {
        var effect = GetNextAvailableEffect();

        if (effect != null)
        {
            InitializeEffects(effect, null, -1, 1, ismapeffect, effectid, animid, entity.PosX, entity.PosY, entity.PosZ);
            effect.AttachedEntity = entity;
            effect.DepthSortOffset = depthsortmod;
            effect.OffsetX = xoff;
            effect.OffsetY = yoff;
            effect.OffsetZ = zoff;
            return effect;
        }

        return null;
    }

    //8003bfe8
    public SpriteEffect CreateDetachedEffect(byte ismapeffect, byte effectid, byte animid, Entity entity, int depthsortmod, int x, int y, int z)
    {
        var effect = GetNextAvailableEffect();

        if (effect != null)
        {
            InitializeEffects(effect, null, -1, 3, ismapeffect, effectid, animid, x, y, z);
            effect.AttachedEntity = entity;
            effect.DepthSortOffset = depthsortmod;
            return effect;
        }
        return null;
    }

    //8003b9c4
    public SpriteEffect? GetNextAvailableEffect()
    {
        foreach (var effect in _gameEngine.StaticVariables.g_effectSlots)
        {
            if (effect.Status == 0)
            {
                return effect;
            }
        }

        return null;
    }

    // 8003bdd8
    public SpriteEffect? CreateEffectEntity(int behaviorFlags, byte spriteTableIndex, byte animationIndex, int x, int y, int z)
    {
        var effect = GetFreeEffect();

        if (effect != null)
        {
            InitializeEffects(effect, null, -1, 0, behaviorFlags, spriteTableIndex, animationIndex, x, y, z);
        }

        return effect;
    }

    // 8003b9c4
    public SpriteEffect? GetFreeEffect()
    {
        int i = 0;

        do
        {
            var slot = _gameEngine.StaticVariables.g_effectSlots[i];

            if (slot.Status == 0)
            {
                return slot;
            }

            i = i + 1;
        }
        while (i < 0x80);

        Breakpoint.TriggerBreak();

        return null;
    }

    // 8003bb14
    public void InitializeEffects(
        SpriteEffect effect, MapEffectRecord? mapEffectRecord, 
        int effectId, int updateMode, int behaviorFlag, 
        byte spriteTableIndex, byte animationIndex, 
        int x, int y, int z)
    {
        var originalId = effect.Id;
        effect.Reset(); //reset all fields with g_emptySpriteEffect
        effect.Id = originalId;
        effect.MapEffectRecord = mapEffectRecord;

        if (mapEffectRecord == null)
        {
            effect.MapEffectId = -1;
        }
        else
        {
            effect.MapEffectId = effectId;
        }

        effect.UpdateMode = updateMode;
        effect.Status = 2;
        effect.TargetSpriteTableIndex = spriteTableIndex;
        effect.TargetAnimation = animationIndex;
        effect.TargetIsMapSprite = (byte)(behaviorFlag != 0 ? 1 : 0);
        effect.CurrentIsMapSprite = (byte)(behaviorFlag == 0 ? 1 : 0);
        effect.CurrentSpriteTableIndex = (byte)~spriteTableIndex;
        effect.CurrentAnimation = (byte)~animationIndex;
        effect.X = x;
        effect.Y = y;
        effect.Z = z;
    }

    // 80032c7c
    public int RandomlySpawnItem(uint contentId, int x, int y, int z)
    {
        var itemId = _gameEngine.ChooseRandomlyAnItem((ushort)contentId);

        if (itemId == 0)
        {
            return 0;
        }

        if (_gameEngine.CanDropMpItems((uint)itemId))
        {
            return 0;
        }

        Entity itemEntity = _gameEngine.SpawnWarpEntity(null, 0, (uint)(itemId + 0x1e), x, y, z, 0);

        if (itemEntity == null)
        {
            return 0;
        }

        itemEntity.ForceZ = 0xA0000;
        itemEntity.Bytes[0] = 1;
        itemEntity.Bytes[1] = 0;
        itemEntity.Bytes[2] = 0;
        itemEntity.Bytes[3] = 0;
        itemEntity.Flags &= 0xffffff7f; // ~0x80

        ////[itemId * 2 + 1]
        //AlundraEngine.Debug.Debugger.Breakpoint();
        var initPosX = _gameEngine.StaticVariables.g_itemDropProperties[itemId].Field1 == 0 ? -1 : 600;

        itemEntity.DelayOrAngleOrEntityId = initPosX;
        itemEntity.ItemState = 0;
        itemEntity.AIValues[0] = 0;
        itemEntity.AIValues[1] = 0;
        itemEntity.AIValues[2] = 0;
        itemEntity.AIValues[3] = 10;
        _gameEngine.SoundManager.PlaySoundEffect(0x54);

        return 1;
    }
}