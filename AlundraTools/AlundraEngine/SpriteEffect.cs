using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;

namespace AlundraEngine;

//0x80 byte record
public class SpriteEffect
{
    public int Id;//0
    public MapEffectRecord MapEffectRecord;//4
    public SpriteEffectRecord SpriteEffectRecord;//8
    public SpriteRef SpriteRef = new();//c-20
    public int _24;
    public int SheetSize;//28
    public int PaletteIndex;//2c
    public int MapEffectId;//30
    public int UpdateMode;//34 //effecttype?
    public Entity AttachedEntity;//38 pointer to something// attached to an entity?
    public int X, Y, Z;//3c,40,44
    public int XOff;//48
    public int YOff;//4c
    public int ZOff;//50
    public int XForce;//x forces?
    public int YForce;//y
    public int ZForce;//z
    public int DepthSortMod;//60
    public int ZSortValue;//stored to 1c, is it a depth sorting id? 64
    public int Status;//68  2 is active
    public byte TargetIsMapSprite;//6c
    public byte CurrentIsMapSprite;//6d
    public byte TargetSpriteTableIndex; //6e
    public byte CurrentSpriteTableIndex;    //6f
    public byte TargetAnimation;             //70
    public byte CurrentAnimation;                //71
    public short _72;
    public SiEffectFrame Frame;   //74
    public SiEffectFrame InitialFrame;      //78
    public byte Delay;                  //7c
    public byte DestroyFlag;            //7d  if this is set true the effect is destroyed on next update (status = 0)
    public byte _7e;
    public byte _7f;

    public int AnimIndex = 0;//use this extra field because we arent using frame pointers that we can simply ++ to the next one
}