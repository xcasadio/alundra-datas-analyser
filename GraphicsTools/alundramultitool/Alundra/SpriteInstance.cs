using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphicsTools.Alundra
{
    public class SpriteInstance
    {
        public int Index;
        public int UnknownAfterIndex;
        public SpriteInstance UnknownBeforeOwnerEntity;
        public SpriteInstance OwnerEntity;
        public int Status;//0=destroyed,1=loaded,2=normal,3=deactivated,4=flagtodestroy,5=?
        public int HP;
        public int MaxHp;
        public int UnknownCounter;//1c
        public int _20;
        public int _24;
        public SpriteInstance PlatformEntity;
        public int _2c;

        public int ContentsItemId;
        public int ContentsGameFlag;
        public SIEntityRecord EntityRecord;
        public int EntityRefId;
        public int[] Program_Indexes = new int[6];
        //public int ProgramA_Load;
        //public int ProgramB_Map;
        //public int ProgramC_Tick;
        //public int ProgramD_Touch;
        //public int ProgramE_Deactivate;
        //public int ProgramF_Interact;
        public SpriteRecord Sprite;
        public int SpriteTableIndex;
        public int Flags;//0x800000 = portrait,0x0100 = gravity,0xf = ?, 0x1 = ? , 0x80 = collidable
        public int[] Sprite_Program_Indexes = new int[6];
        //public int SpriteU4;
        //public int UnkownBeforeThrowType;
        //public int ThrowType;
        //public int SpriteU6;
        //public int BreakSound;
        //public int SpriteU8;

        public int TargetAnim;
        public int TargetDir;
        public int CurAnim;
        public int CurDir;
        public int FrameDex;
        public SIAnimSet AnimSet;
        public SIFrame FirstFrame;
        public SIFrame Frame;
        public int NextFrameDelay;
        public int WierdNextFrameDelayFlag;
        public int AnimCompleteCounter;
        public int AnimFlags;
        public int ZForce;//rise/fall speed
        public int TargetXForce, TargetYForce, XForce, YForce;
        public int InteractXForce;//?cc
        public int InteractYForce;//?d0
        public int XForceStep, YForceStep;//d4,d8
        public int AdjustedXForce, AdjustedYForce;//dc,e0
        public int FinalXForce, FinalYForce, FinalZForce;//e4,e8,ec
        public int Acceleration;//f0
        public int Speed;//f4
        public int AppliedZForce;//this is probably named wrong, has to do with animation  f8
        public int ScreenClipX, ScreenClipY, ScreenClipZ;
        public int NegXMod, NegYMod;
        public int XPos;
        public int YPos;
        public int ZPos;
        public int XTile;
        public int YTile;
        public int ZTile;
        public SpriteInstance RidingEntity;
        public SpriteInstance XCollisionEntity;
        public int ZEntityCollision;
        public int ZMapCollision;//map collision

        public int ForceAdjusted;
        public int CollidedWithEntityZ;//some boolean that has to do with if moddedzpos is greater than hity from collideentitiesz
        public int _144;//collided with something
        public MapTile[] MapTiles = new MapTile[4];
        public int[] MapHeights = new int[4];
        public bool DoneMoving;

        public int _180, _184, SomethingForceIndex;//188
        public int _18c, _190;//slopesomething?, slopesomethingprev?
        public SpriteRef SpriteRef;//194 

        public int AddedToSheet, AddedToPalette;//represents offset where the pallets and sheets are in memory for map vs global sprites, prob not used with my engine
        public SpriteEffect ActiveEffect;
        public int DepthSortVal;//1bc
        public int SortTop;//1c0
        public BalanceRecord BalanceRecord;//1c4
        public BalanceAnimValRef BalanceVal;//1c8
        public int DamagedTickCounter;//1cc
        public int FrameColTickCounter;//1d0
        public FrameCollisionData FrameCollision;//1d4
        public int ModdedXPos, ModdedYPos;
        public int ModdedZPos, XMod, YMod;
        public int ZMod, Width, Depth;
        public int Height;
        //this set of vars is set when an animation has a frame with attached data
        public int FrameX;//1fc
        public int FrameY;//200
        public int FrameZ;//204
        public int FrameXOff;//208
        public int FrameYOff;//20c
        public int FrameZOff;//210
        public int FrameWidth;//214
        public int FrameDepth;//218
        public int FrameHeight;//21c
        public int HitCounter;//220
        public SpriteInstance TouchingEntity;//224
        public int EventTrigger;//228  for the player character this holds the id of the map event that is triggering, for other entities this holds the type of event slot to trigger
        public int MapEventProgramId;//22c
        public SpriteInstance EntitySelf;
        public EventProgramState eventdata = new EventProgramState();
        //public EventProgramState eventdata2 = new EventProgramState();
        public int UnknownEventAnim;//26c
        public int UnknownEventDir;//270
        public int _274;
        public int SpawnedItemId;//278
        public int _27c;
        public int SpawnedGameFlag;//280
        public int SpawnedZForce;//284
        public bool IsMapSprite
        {
            get
            {
                return (EntityRecord.spritedir & 0x80) != 0;
            }
        }
    }

    public class SpriteRef
    {
        public SIImage[] Images;        //c
        public int X;//4				//10
        public int Y;//8				//14
        public int Z;//c				//18
        public int DepthSortVal;//0x10		//1c
        public int NumImages;//0x14		//20
    }

    //public class EventData
    //{
    //	public int maploadprogram;//program that runs on map load
    //	public EventProgramState tickprogram;//event program that runs every tick
    //	public int interactprogram;
    //}

    public class EventProgramState
    {
        public int sp;
        public int exp;
        public int evttickprog;
        public int[] evtvars = new int[8];
        public int logicResult;

        public ushort elapsedMs;
        public byte isWaiting;
        public byte[] code;
    }
}
