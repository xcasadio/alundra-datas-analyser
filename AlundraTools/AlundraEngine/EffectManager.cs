using System.Diagnostics;
using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;

namespace AlundraEngine;

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
        foreach (var spriteEffect in StaticVariables.g_effectSlots)
        {
            spriteEffect.Status = 0;
        }

        //TODO check this
        var mapEffectRecords = _gameEngine.CurrentMap.SpriteInfo.SpriteEffects;
        for (int i = 0; i < mapEffectRecords.Length; i++)
        {
            if (mapEffectRecords[i] == null)
            {
                break;
            }
        }

        //var effectIndex = 0;
        //var mapEventRecord = _gameEngine.CurrentMap.SpriteInfo.MapEffectRecords[effectIndex];
        ////var mapEventRecord = StaticVariables.g_initMapEventRecords[effectIndex];
        //var val = (uint)(mapEventRecord.X1 | (mapEventRecord.Y1 << 8) | (mapEventRecord.X2 << 16) | (mapEventRecord.Y2 << 24));
        //
        //while (val != 0)
        //{
        //    var effectSlotPtr = SpawnSpriteEffect(effectIndex, 0);
        //    if (effectSlotPtr == null 
        //        && (StaticVariables.g_debugState & 0x80000000U) != 0 
        //        && (StaticVariables.g_debugFlags & 0x20) != 0)
        //    {
        //        Debugger.Break();
        //        //PrintInfo();
        //    }
        //
        //    effectIndex += 1;
        //    //mapEventRecord = StaticVariables.g_initMapEventRecords[effectIndex];
        //    mapEventRecord = _gameEngine.CurrentMap.SpriteInfo.MapEffectRecords[effectIndex];
        //    val = (uint)(mapEventRecord.X1 | (mapEventRecord.Y1 << 8) | (mapEventRecord.X2 << 16) | (mapEventRecord.Y2 << 24));
        //}
    }

    public SpriteEffect SpawnSpriteEffect(int effectId, int checkSpawnArea)
    {
        MapEffectRecord effectStatus;
        SpriteEffect effect;
        byte flags;

        effectStatus = GetMapEffectRecord(effectId, checkSpawnArea == 1);
        effect = null;

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
                        (int)(((uint)effectStatus.X * 12 + 12) * 0x10000),
                        (int)(((uint)effectStatus.Y * 8 + 8) * 0x10000),
                        (int)((uint)effectStatus.Z << 0x13)
                    );
                }
            }
        }

        return effect;
    }
    
    public MapEffectRecord GetMapEffectRecord(int id, bool checkBoundingBox)
    {
        if (id < _gameEngine.CurrentMap.SpriteInfo.MapEffectRecords.Length)
        {
            var record = _gameEngine.CurrentMap.SpriteInfo.MapEffectRecords[id];
            if (checkBoundingBox)
            {
                var playerEntity = StaticVariables.PlayerEntity;
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
        foreach (var effect in StaticVariables.g_effectSlots)
        {
            if (effect.Status != 2)
            {
                continue;
            }

            if ((StaticVariables.g_playerControlFlags & 0x48) == 0)
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
            StaticVariables.g_spriteImages[StaticVariables.g_spriteNumberOfImage++] = effect.SpriteRef;
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
                effect.SpriteRef.DepthSortValue = 0;
                return;
            }

            effect.SpriteEffectRecord = record;
            effect.SheetSize = sheetSize;
            effect.PaletteIndex = paletteIndex;
            effect.CurrentIsMapSprite = effect.TargetIsMapSprite;
            effect.CurrentSpriteTableIndex = effect.TargetSpriteTableIndex;

            effect.CurrentAnimation = (byte)~effect.TargetAnimation;
        }

        if (effect.CurrentAnimation != effect.TargetAnimation)
        {
            var anim = effect.SpriteEffectRecord.PreloadedAnims[effect.TargetAnimation];
            effect.Frame = anim.Frames[0];
            effect.FirstFrame = effect.Frame;

            effect.CurrentAnimation = effect.TargetAnimation;
            effect.NextFrameDelay = 0;
            effect.DestroyFlag = 0;
            effect.CurrentFrameIndex = 0;
        }
        else
        {
            effect.NextFrameDelay--;

            if ((effect.NextFrameDelay & 0xff) != 0)
            {
                return; // Pas encore temps de changer de frame
            }

            if (effect.NextFrameDelay == 0)
            {
                effect.CurrentFrameIndex++;
            }
        }

        // Boucle de traitement des frames
        while (true)
        {
            var frameData = effect.Frame;

            if ((frameData.Delay & 0x80) != 0)
            {
                //effect.CurrentFrameIndex++;
                var anim = effect.SpriteEffectRecord.PreloadedAnims[effect.TargetAnimation];
                effect.Frame = anim.Frames[effect.CurrentFrameIndex];
                effect.NextFrameDelay = (byte)(frameData.Delay & 0x7f);
                var imageOffset = (frameData.ImageSetPointer >> 8) | ((frameData.ImageSetPointer & 0xff) << 8);

                if (effect.Frame.Images != null) // (imageOffset != 0xffff)
                {
                    effect.SpriteRef.Images = effect.Frame.Images.Images;
                    effect.SpriteRef.DepthSortValue = effect.Frame.Images.DepthSortValue;
                    effect.SpriteRef.NumberOfImages = effect.Frame.Images.NumberOfImages;
                }
                else
                {
                    effect.SpriteRef.Images = null;
                    effect.SpriteRef.DepthSortValue = 0;
                    effect.SpriteRef.NumberOfImages = 0;
                }
                return;
            }

            if (frameData.Delay == 0)
            {
                effect.NextFrameDelay = 0xff; // Animation non-répétitive, marquer pour destruction
                effect.DestroyFlag = 1;
                return;
            }

            if (frameData.Delay == 1) //loop
            {
                effect.CurrentFrameIndex = 0;
                effect.Frame = effect.FirstFrame;
            }
            else
            {
                Debugger.Break();
                throw new Exception("Effect Animation Error!!");
            }
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
            //some kind of unique id? maybe its used for zsorting
            effect.DepthSortValue = (int)(effect.Y & 0xffff0000) + (effect.Z >> 16) + (effect.SpriteRef.DepthSortValue << 16);
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
                effect.DepthSortValue = entity.ZSortValue + effect.DepthSortOffset;
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
            effect.DepthSortValue = effect.AttachedEntity.ZSortValue + effect.DepthSortOffset;

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
        foreach (var effect in StaticVariables.g_effectSlots)
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
            var slot = StaticVariables.g_effectSlots[i];

            if (slot.Status == 0)
            {
                return slot;
            }

            i = i + 1;
        }
        while (i < 0x80);

        Debugger.Break();

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
    public int CreateWarpEffect(uint actionId, int x, int y, int z)
    {
        var resolvedAction = _gameEngine.GetContentsItemId((short)actionId);

        if (resolvedAction == 0)
        {
            return 0;
        }

        if (_gameEngine.CheckItemId((uint)resolvedAction))
        {
            return 0;
        }

        Entity warpEntity = _gameEngine.SpawnWarpEntity(null, 0, (uint)(resolvedAction + 0x1e), x, y, z, 0);

        if (warpEntity == null)
        {
            return 0;
        }

        warpEntity.ForceZ = 0xA0000;
        warpEntity.Bytes[0] = 1;
        warpEntity.Bytes[1] = 0;
        warpEntity.Bytes[2] = 0;
        warpEntity.Bytes[3] = 0;
        warpEntity.Flags &= 0xffffff7f; // ~0x80
        
        var initPosX = StaticVariables.g_iconNameEtcBase[resolvedAction * 2 + 1] == 0 ? -1 : 600;

        warpEntity.InitialXPos = initPosX;
        warpEntity.InitialYPos = 0;
        warpEntity.AIValues[0] = 0;
        warpEntity.AIValues[1] = 0;
        warpEntity.AIValues[2] = 0;
        warpEntity.AIValues[3] = 10;
        _gameEngine.SoundManager.PlaySoundEffect(0x54);

        return 1;
    }
}