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

    public GameEngine GameEngine
    {
        get { return _gameEngine; }
    }

    public void InitializeEffectSlots()
    {
        SpriteEffect effect;
        int val;
        int effectIndex;
        int[] effectInitTable;

        effect = StaticVariables.g_effectSlots[0];
        effectIndex = 0x7f;

        do
        {
            StaticVariables.g_effectSlots[effectIndex].Status = 0;
            effectIndex--;
        } while (effectIndex >= 0);
    }

    public void UpdateEffects()
    {
        foreach (var effect in StaticVariables.g_effectSlots)//g_effectSlots)
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

            effect.SpriteRef.DepthSortVal = effect.ZSortValue;
            effect.SpriteRef.X = effect.X;
            effect.SpriteRef.Y = effect.Y;
            effect.SpriteRef.Z = effect.Z;
            StaticVariables.g_spriteImages[StaticVariables.g_spriteNumberOfImage++] = effect.SpriteRef;
            //StaticVariables.g_currentEntitySpriteImages++;
            //StaticVariables.g_spriteNumberOfImage++;
        }
    }

    private void UpdateEffectAnimation(SpriteEffect effect)
    {
        if (effect.CurrentSpriteTableIndex != effect.TargetSpriteTableIndex
            || effect.CurrentIsMapSprite != effect.TargetIsMapSprite)
        {
            int addtosheet, addtopal;
            var record = _gameEngine.GetEffectSpriteFromSpriteTable(effect.TargetIsMapSprite != 0, effect.TargetSpriteTableIndex, out addtosheet, out addtopal);
            if (record == null)
            {
                effect.DestroyFlag = 1;
                effect.SpriteRef.Images = null;
                effect.SpriteRef.NumImages = 0;
                //field after numimages = 0
                return;
            }

            effect.SpriteEffectRecord = record;
            effect.CurrentIsMapSprite = effect.TargetIsMapSprite;
            effect.CurrentSpriteTableIndex = effect.TargetSpriteTableIndex;

            effect.SheetSize = addtosheet;
            effect.PaletteIndex = addtopal;
            effect.CurrentAnimation = (byte)~effect.TargetAnimation;
        }

        if (effect.CurrentAnimation != effect.TargetAnimation)
        {
            var animation = effect.SpriteEffectRecord.PreloadedAnims[effect.TargetAnimation];
            effect.AnimIndex = 0;
            var nframe = animation.Frames[effect.AnimIndex];
            effect.CurrentAnimation = effect.TargetAnimation;
            effect.Delay = 0;
            effect.DestroyFlag = 0;
            effect.InitialFrame = nframe;
            effect.Frame = nframe;
            Debug.Assert(nframe != null);
        }
        else
        {
            effect.Delay--;
            if ((effect.Delay & 0xff) != 0)
            {
                return;
            }
        }

        do
        {
            var frame = effect.Frame;
            if ((frame.Delay & 0x80) != 00)
            {
                //effect.AnimIndex++;
                var anim = effect.SpriteEffectRecord.PreloadedAnims[effect.TargetAnimation];
                frame = anim.Frames[effect.AnimIndex];
                effect.Frame = frame;
                Debug.Assert(frame != null);
                if (frame?.Images != null)
                {
                    effect.SpriteRef.Images = frame.Images.Images;
                    effect.SpriteRef.DepthSortVal = frame.Images.Unknown;
                    effect.SpriteRef.NumImages = frame.Images.NumberOfImages;
                    return;
                }
                effect.SpriteRef.Images = null;
                effect.SpriteRef.DepthSortVal = 0;
                effect.SpriteRef.NumImages = 0;
                return;
            }
            if (frame.Delay != 0)
            {
                if (frame.Delay == 1)
                {
                    //repeat
                    effect.AnimIndex = 0;
                    effect.Frame = effect.InitialFrame;
                    continue;
                }
                throw new Exception("Error with Effect Animation!");
            }

            //its 0  which means its non repeating so flag for destroy
            effect.Delay = 0xff;
            effect.DestroyFlag = 1;
            return;
        } while (true);

    }

    private void UpdateEffectPosition(SpriteEffect effect)
    {
        if (effect.UpdateMode == 0)
        {
            effect.X += effect.XForce; //forces?
            effect.Y += effect.YForce;
            effect.Z += effect.ZForce;
            //some kind of unique id? maybe its used for zsorting
            effect.ZSortValue = (int)(effect.Y & 0xffff0000) + (effect.Z >> 16) + (effect.SpriteRef.DepthSortVal << 16);
            return;
        }

        if (effect.UpdateMode == 1)
        {
            var entity = effect.AttachedEntity;
            if (entity.Status != 0)
            {
                effect.X = entity.PosX + effect.XOff;
                effect.Y = entity.PosY + effect.YOff;
                effect.Z = entity.PosZ + effect.ZOff;
                effect.ZSortValue = entity.ZSortValue + effect.DepthSortMod;
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
        effect.X += effect.XForce; //forces?
        effect.Y += effect.YForce;
        effect.Z += effect.ZForce;

        if (effect.AttachedEntity.Status != 0)
        {
            effect.ZSortValue = effect.AttachedEntity.ZSortValue + effect.DepthSortMod;

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

    public SpriteEffect CreateEffect_Type0(byte ismapeffect, byte effectid, byte animid, int x, int y, int z)
    {
        var effect = GetNextAvailableEffect();

        if (effect != null)
        {
            InitializeEffects(effect, null, -1, 0, ismapeffect, effectid, animid, x, y, z);
            return effect;
        }
        return null;
    }

    public SpriteEffect CreateAttachedEffect(byte ismapeffect, byte effectid, byte animid, Entity entity, int depthsortmod, int xoff, int yoff, int zoff)
    {
        var effect = GetNextAvailableEffect();

        if (effect != null)
        {
            InitializeEffects(effect, null, -1, 1, ismapeffect, effectid, animid, entity.PosX, entity.PosY, entity.PosZ);
            effect.AttachedEntity = entity;
            effect.DepthSortMod = depthsortmod;
            effect.XOff = xoff;
            effect.YOff = yoff;
            effect.ZOff = zoff;
            return effect;
        }
        return null;
    }

    public SpriteEffect CreateEffect_Type3(byte ismapeffect, byte effectid, byte animid, Entity entity, int depthsortmod, int x, int y, int z)
    {
        var effect = GetNextAvailableEffect();

        if (effect != null)
        {
            InitializeEffects(effect, null, -1, 3, ismapeffect, effectid, animid, x, y, z);
            effect.AttachedEntity = entity;
            effect.DepthSortMod = depthsortmod;
            return effect;
        }
        return null;
    }

    public SpriteEffect GetNextAvailableEffect()
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

    public void InitializeEffects(SpriteEffect effect, MapEffectRecord mapEffectRecord, int mapeffectid, int effecttype, byte ismapeffect, byte effectid, byte animid, int x, int y, int z)
    {
        //initialize
        effect.MapEffectRecord = null;
        effect.SpriteEffectRecord = null;
        effect.SpriteRef = new SpriteRef();
        effect.SheetSize = 0;
        effect.PaletteIndex = 0;
        effect.MapEffectId = 0;
        effect.UpdateMode = 0;
        effect.AttachedEntity = null;
        effect.X = 0;
        effect.Y = 0;
        effect.Z = 0;
        effect.XOff = 0;
        effect.YOff = 0;
        effect.ZOff = 0;
        effect.XForce = 0;
        effect.YForce = 0;
        effect.ZForce = 0;
        effect.DepthSortMod = 0;
        effect.ZSortValue = 0;
        effect.Status = 0;
        effect.TargetIsMapSprite = 0;
        effect.CurrentIsMapSprite = 0;
        effect.TargetSpriteTableIndex = 0;
        effect.CurrentSpriteTableIndex = 0;
        effect.TargetAnimation = 0;
        effect.CurrentAnimation = 0;
        effect.Frame = null;
        effect.InitialFrame = null;
        effect.Delay = 0;
        effect.DestroyFlag = 0;

        effect.AnimIndex = 0;


        effect.MapEffectRecord = mapEffectRecord;
        if (mapEffectRecord != null)
        {
            effect.MapEffectId = mapeffectid;
        }
        else
        {
            effect.MapEffectId = -1;
        }

        effect.Status = 2;
        effect.CurrentSpriteTableIndex = (byte)~effectid;
        effect.TargetIsMapSprite = ismapeffect;
        effect.CurrentIsMapSprite = (byte)~ismapeffect;
        effect.TargetSpriteTableIndex = effectid;
        effect.TargetAnimation = animid;
        effect.CurrentAnimation = (byte)~animid;
        effect.X = x;
        effect.Y = y;
        effect.Z = z;
    }

    public SpriteEffect CreateEffectEntity(int behaviorFlags, byte spriteTableIndex, byte animationIndex, int x, int y, int z)
    {
        SpriteEffect effect = GetFreeEffect();

        if (effect != null)
        {
            InitEffectEntity(effect, null, -1, 0, behaviorFlags, spriteTableIndex, animationIndex, x, y, z);
        }

        return effect;
    }

    public SpriteEffect GetFreeEffect()
    {
        SpriteEffect slot;
        int i = 0;

        do
        {
            slot = StaticVariables.g_effectSlots[i];

            if (slot.Status == 0)
            {
                return slot;
            }

            i = i + 1;
        }
        while (i < 0x80);

        return null;
    }

    public void InitEffectEntity(SpriteEffect effect, MapEffectRecord mapEffectRecord, int effectId, int updateMode,
        int behaviorFlag, byte spriteTableIndex, byte animationIndex, int x, int y, int z)
    {
        MapEffectRecord pMVar1;
        SpriteEffectRecord pSVar2;
        int[] piVar3;
        int[][] baseTemplate;
        SpriteEffect pEffect;
        int originalId;

        //baseTemplate = StaticVariables.DAT_8013c608;
        originalId = effect.Id;
        //pEffect = effect;
        //
        //do
        //{
        //    pMVar1 = (MapEffectRecord)(object)baseTemplate[1];
        //    pSVar2 = (SpriteEffectRecord)(object)baseTemplate[2];
        //    piVar3 = baseTemplate[3];
        //    pEffect.Id = baseTemplate[0][0];
        //    pEffect.MapEffectRecord = pMVar1;
        //    pEffect.SpriteEffectRecord = pSVar2;
        //    pEffect.SpriteRef.Images = piVar3;
        //    baseTemplate = baseTemplate.Skip(4).ToArray();
        //    pEffect = (SpriteEffect)(object)pEffect.SpriteRef.X;
        //}
        //while (!ReferenceEquals(baseTemplate, StaticVariables.g_monitorBase));

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
}