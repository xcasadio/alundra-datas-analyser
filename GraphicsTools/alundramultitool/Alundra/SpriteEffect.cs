using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsTools.Alundra
{
    //0x80 byte record
    public class SpriteEffect
    {
        public int Id;//0
        public MapEffectRecord MapEffectRecord;//4
        public SpriteEffectRecord SpriteEffectRecord;//8
        public SpriteRef SpriteRef = new SpriteRef();//c-20
        //24
        public int AddToSheet;//28
        public int AddToPalette;//2c
        public int MapEffectId;//30
        public int EffectType;//34 //effecttype?
        public SpriteInstance EntityRef;//38 pointer to something// attached to an entity?
        public int X, Y, Z;//3c,40,44
        public int XOff;//48
        public int YOff;//4c
        public int ZOff;//50
        public int XForce;//x forces?
        public int YForce;//y
        public int ZForce;//z
        public int DepthSortMod;//60
        public int DepthSortVal;//stored to 1c, is it a depth sorting id? 64
        public int Status;//68  2 is active
        public byte TargetIsMapSprite;//6c
        public byte CurIsMapSprite;//6d
        public byte TargetSpriteTableIndex; //6e
        public byte CurSpriteTableIndex;    //6f
        public byte TargetAnim;             //70
        public byte CurAnim;                //71

        public SIEffectFrame Frame;   //74
        public SIEffectFrame FirstFrame;      //78
        public byte Delay;                  //7c
        public byte DestroyFlag;            //7d  if this is set true the effect is destroyed on next update (status = 0)

        public int animdex = 0;//use this extra field because we arent using frame pointers that we can simply ++ to the next one
    }
}
