using Alundra.DatasBin;
using Alundra.Gameplay.Scripts;
using Alundra.Sound;

namespace Alundra.Gameplay;

public class Entity
{
    public int Index;
    public int Index2;
    public Entity ChildEntity; ////UnknownBeforeOwnerEntity;
    public Entity ParentEntity; //OwnerEntity;
    public int Status;//0=destroyed,1=loaded,2=normal,3=deactivated,4=flagtodestroy,5=?
    public int Hp;
    public int HpMax;
    public int UnknownCounter;//1c
    public int IsNotProcessable;
    public int _24;
    public Entity PlatformEntity;
    public int _2c;
    public int RelativeWarpOffsetX;
    public int RelativeWarpOffsetY;
    public int RelativeWarpOffsetZ;
    public uint ContentsItemId; //3c
    public int ContentsGameFlag; 
    public SiEntityRecord EntityRecord;
    public int EntityRefId;
    public readonly int[] ProgramIndexes = new int[6]; //4c
    //public int ProgramA_Load;
    //public int ProgramB_Map;
    //public int ProgramC_Tick;
    //public int ProgramD_Touch;
    //public int ProgramE_Deactivate;
    //public int ProgramF_Interact;
    public SpriteRecord Sprite;
    public uint SpriteTableIndex;
    public uint Flags;//0x800000 = portrait,0x0100 = gravity,0xf = ?, 0x1 = ? , 0x80 = collidable
    public readonly int[] SpriteProgramIndexes = new int[6]; //70
    //public int SpriteU4;
    //public int UnkownBeforeThrowType;
    //public int ThrowType;
    //public int SpriteU6;
    //public int BreakSound;
    //public int SpriteU8;

    public uint TargetAnimationId;
    public uint TargetDirection;
    public uint CurrentAnimationId;
    public uint CurrentDirection;
    public int CurrentFrameIndex;
    public SiAnimSet AnimSet;
    public SiFrame FirstFrame;
    public SiFrame Frame;
    public int NextFrameDelay;
    public int ForceResetAnimationFlag;
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
    public int IsZForceApplied;//this is probably named wrong, has to do with animation  f8
    public int ScreenClipX, ScreenClipY, ScreenClipZ;
    public int NegXMod, NegYMod;
    public int XPos; //114
    public int YPos;
    public int ZPos;
    public int XTile;
    public int YTile;
    public int ZTile;
    public Entity RidingEntity; //12c
    public Entity XCollisionEntity;
    public int ZEntityCollision;
    public int TerrainHeight;//map collision

    public int ForceAdjusted;
    public int CollidedWithEntityZ;//some boolean that has to do with if moddedzpos is greater than hity from collideentitiesz
    public int _144;//collided with something
    public readonly MapTile[] MapTiles = new MapTile[4];
    public readonly int[] MapHeights = new int[4];
    public bool DoneMoving;

    public int combinedVramFlagsOR, combinedVramFlagsAND, SomethingForceIndex;//188
    public int _18c, _190;//slopesomething?, slopesomethingprev?
    public SpriteRef SpriteRef = new SpriteRef();//194 
    //public int field91_0x1ac; // 1ac => SpriteRef
    public int AddedToSheet, AddedToPalette;//represents offset where the pallets and sheets are in memory for map vs global sprites, prob not used with my engine
    public SpriteEffect ActiveEffect;
    public int DepthSortVal;//1bc
    public int SortTop;//1c0
    public BalanceRecord BalanceRecord;//1c4
    public BalanceAnimValRef BalanceVal;//1c8
    public int DamagedTickCounter;//1cc
    public int FrameColTickCounter;//1d0
    public FrameCollisionData FrameCollision;//1d4
    public int ModdedXPos, ModdedYPos, ModdedZPos;
    public int XMod, YMod, ZMod;
    public int Width, Depth, Height;
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
    public Entity TouchingEntity;//224
    public int EventTrigger;//228  for the player character this holds the id of the map event that is triggering, for other entities this holds the type of event slot to trigger
    public int MapEventProgramId;//22c
    public Entity LogicContextEntity; //self
    public EventProgramState EventProgramState = new();
    //public EventProgramState eventdata2 = new EventProgramState();
    public uint UnknownEventAnim;//26c
    public uint UnknownEventDir;//270
    public int _274;
    public int SpawnedItemId;//278
    public int _27c;
    public int SpawnedGameFlag;//280
    public int SpawnedZForce;//284

    public bool IsMapSprite => (EntityRecord.SpriteDirection & 0x80) != 0;
}

//public class EventProgramState
//{
//	public int maploadprogram;//program that runs on map load
//	public EventProgramState tickprogram;//event program that runs every tick
//	public int interactprogram;
//}