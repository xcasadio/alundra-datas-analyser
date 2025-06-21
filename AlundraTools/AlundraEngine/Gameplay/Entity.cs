using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay.Scripts;
using AlundraEngine.Sound;

namespace AlundraEngine.Gameplay;

public class Entity
{
    //for debugging
    public int SpriteInfoEntityIndex;

    public int Index;
    public int Index2;
    public Entity? ChildEntity; ////UnknownBeforeOwnerEntity;
    public Entity? ParentEntity; //OwnerEntity;
    public int Status;//0=destroyed,1=loaded,2=normal,3=deactivated,4=flagtodestroy,5=?
    public int Hp;
    public int HpMax;
    public int FrameCounter;//1c
    public int IsNotProcessable;
    public int Flags2;
    public Entity? PlatformEntity; //28
    public Entity? WarpEntity;
    public int RelativeWarpOffsetX;
    public int RelativeWarpOffsetY;
    public int RelativeWarpOffsetZ;
    public uint ContentsItemId; //3c
    public int ContentsGameFlag;
    public SiEntityRecord? EntityRecord;
    public int EntityRefId;
    public readonly int[] ProgramIndexes = new int[6]; //4c
    public SpriteRecord? Sprite;
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
    public AnimationSet? AnimSet;
    public SiFrame? FirstFrame;
    public SiFrame? Frame;
    public int NextFrameDelay;
    public int ForceResetAnimationFlag;
    public int AnimCompleteCounter;
    public int AnimFlags;
    public int ForceZ;//rise/fall speed
    public int TargetXForce, TargetYForce;
    public int ForceX, ForceY;
    public int PreviousAdjustedXForce;//?cc
    public int PreviousAdjustedYForce;//?d0
    public int ForceStepX, ForceStepY;//d4,d8
    public int AdjustedXForce, AdjustedYForce;//dc,e0
    public int FinalXForce, FinalYForce, FinalZForce;//e4,e8,ec
    public int Acceleration;//f0
    public int Speed;//f4
    public int IsZForceApplied;//this is probably named wrong, has to do with animation  f8
    public int ScreenClipX, ScreenClipY, ScreenClipZ;
    public int NegXMod, NegYMod, NegZMod;
    public int PosX; //114
    public int PosY;
    public int PosZ;
    public int TileX;
    public int TileY;
    public int TileZ;
    public Entity? RidingEntity; //12c
    public Entity? XCollisionEntity;
    public int FloorHeight;
    public int TerrainHeight;//map collision

    public int ForceAdjusted;
    public int CollidedWithEntityZ;//some boolean that has to do with if moddedzpos is greater than hity from collideentitiesz
    public int IsAboveGround;//collided with something
    public readonly MapTile[] MapTiles = new MapTile[4];
    public readonly int[] MapHeights = new int[4]; // 158
    public int PlatformUpdateFlag; //public bool DoneMoving;
    public int _16c;
    public int HitBoxOriginX;
    public int HitBoxOriginY;
    public int HitBoxOriginZ;
    public int _17c;
    public int CombinedVramFlagsOR;
    public int CombinedVramFlagsAND;
    public int TileAttributes; //188
    public int Slope_18c; // slopesomething?, 
    public int Slope_190; // slopesomethingprev?
    public SpriteRef SpriteRef = new SpriteRef();//194 
    //public int field91_0x1ac; // 1ac => SpriteRef
    public int AddedToSheet, AddedToPalette;//represents offset where the pallets and sheets are in memory for map vs global sprites, prob not used with my engine
    public SpriteEffect? ActiveEffect;
    public int ZSortValue;//1bc
    public int ZSortDepth;//1c0
    public BalanceRecord? BalanceRecord;//1c4
    public BalanceAnimValRef? BalanceVal;//1c8
    public int DamagedTickCounter;//1cc
    public int FrameColTickCounter;//1d0
    public FrameCollisionData? FrameCollision;//1d4
    public int ModdedXPos, ModdedYPos, ModdedZPos;
    public int ModX, ModY, ModZ;
    public int Width, Depth, Height;
    //this set of vars is set when an animation has a frame with attached data
    public int HitBoxX;//1fc
    public int HitBoxY;//200
    public int HitBoxZ;//204
    public int FrameXOff;//208
    public int FrameYOff;//20c
    public int FrameZOff;//210
    public int FrameWidth;//214
    public int FrameDepth;//218
    public int FrameHeight;//21c
    public int HitCounter;//220
    public Entity? TouchingEntity;//224
    public int EventTrigger;//228  for the player character this holds the id of the map event that is triggering, for other entities this holds the type of event slot to trigger
    public int MapEventProgramId;//22c
    public Entity LogicContextEntity; //self
    public readonly EventProgramState EventProgramState = new();
    public uint LastTargetAnimationId;//26c
    public uint LastTargetDirection;//270
    public byte[] Bytes = new byte[4];
    public int InitialXPos;//278
    public int InitialYPos;
    public short[] AIValues = new short[10];//280

    public bool IsMapSprite => EntityRecord == null ? false : (EntityRecord.SpriteDirection & 0x80) != 0;

    public void CopyFrom(Entity other)
    {
        if (other == null)
        {
            return;
        }

        Index = other.Index;
        Index2 = other.Index2;
        ChildEntity = other.ChildEntity;
        ParentEntity = other.ParentEntity;
        Status = other.Status;
        Hp = other.Hp;
        HpMax = other.HpMax;
        FrameCounter = other.FrameCounter;
        IsNotProcessable = other.IsNotProcessable;
        Flags2 = other.Flags2;
        PlatformEntity = other.PlatformEntity;
        WarpEntity = other.WarpEntity;
        RelativeWarpOffsetX = other.RelativeWarpOffsetX;
        RelativeWarpOffsetY = other.RelativeWarpOffsetY;
        RelativeWarpOffsetZ = other.RelativeWarpOffsetZ;
        ContentsItemId = other.ContentsItemId;
        ContentsGameFlag = other.ContentsGameFlag;
        EntityRecord = other.EntityRecord;
        EntityRefId = other.EntityRefId;
        for (int i = 0; i < ProgramIndexes.Length; i++)
        {
            ProgramIndexes[i] = other.ProgramIndexes[i];
        }

        Sprite = other.Sprite;
        SpriteTableIndex = other.SpriteTableIndex;
        Flags = other.Flags;
        for (int i = 0; i < SpriteProgramIndexes.Length; i++)
        {
            SpriteProgramIndexes[i] = other.SpriteProgramIndexes[i];
        }

        TargetAnimationId = other.TargetAnimationId;
        TargetDirection = other.TargetDirection;
        CurrentAnimationId = other.CurrentAnimationId;
        CurrentDirection = other.CurrentDirection;
        CurrentFrameIndex = other.CurrentFrameIndex;
        AnimSet = other.AnimSet;
        FirstFrame = other.FirstFrame;
        Frame = other.Frame;
        NextFrameDelay = other.NextFrameDelay;
        ForceResetAnimationFlag = other.ForceResetAnimationFlag;
        AnimCompleteCounter = other.AnimCompleteCounter;
        AnimFlags = other.AnimFlags;
        ForceZ = other.ForceZ;
        TargetXForce = other.TargetXForce;
        TargetYForce = other.TargetYForce;
        ForceX = other.ForceX;
        ForceY = other.ForceY;
        PreviousAdjustedXForce = other.PreviousAdjustedXForce;
        PreviousAdjustedYForce = other.PreviousAdjustedYForce;
        ForceStepX = other.ForceStepX;
        ForceStepY = other.ForceStepY;
        AdjustedXForce = other.AdjustedXForce;
        AdjustedYForce = other.AdjustedYForce;
        FinalXForce = other.FinalXForce;
        FinalYForce = other.FinalYForce;
        FinalZForce = other.FinalZForce;
        Acceleration = other.Acceleration;
        Speed = other.Speed;
        IsZForceApplied = other.IsZForceApplied;
        ScreenClipX = other.ScreenClipX;
        ScreenClipY = other.ScreenClipY;
        ScreenClipZ = other.ScreenClipZ;
        NegXMod = other.NegXMod;
        NegYMod = other.NegYMod;
        PosX = other.PosX;
        PosY = other.PosY;
        PosZ = other.PosZ;
        TileX = other.TileX;
        TileY = other.TileY;
        TileZ = other.TileZ;
        RidingEntity = other.RidingEntity;
        XCollisionEntity = other.XCollisionEntity;
        FloorHeight = other.FloorHeight;
        TerrainHeight = other.TerrainHeight;
        ForceAdjusted = other.ForceAdjusted;
        CollidedWithEntityZ = other.CollidedWithEntityZ;
        IsAboveGround = other.IsAboveGround;
        for (int i = 0; i < MapTiles.Length; i++)
        {
            MapTiles[i] = other.MapTiles[i];
        }

        for (int i = 0; i < MapHeights.Length; i++)
        {
            MapHeights[i] = other.MapHeights[i];
        }

        PlatformUpdateFlag = other.PlatformUpdateFlag;
        _16c = other._16c;
        HitBoxOriginX = other.HitBoxOriginX;
        HitBoxOriginY = other.HitBoxOriginY;
        HitBoxOriginZ = other.HitBoxOriginZ;
        _17c = other._17c;
        CombinedVramFlagsOR = other.CombinedVramFlagsOR;
        CombinedVramFlagsAND = other.CombinedVramFlagsAND;
        TileAttributes = other.TileAttributes;
        Slope_18c = other.Slope_18c;
        Slope_190 = other.Slope_190;
        SpriteRef.X = other.SpriteRef.X;
        SpriteRef.Y = other.SpriteRef.Y;
        SpriteRef.Z = other.SpriteRef.Z;
        AddedToSheet = other.AddedToSheet;
        AddedToPalette = other.AddedToPalette;
        ActiveEffect = other.ActiveEffect;
        ZSortValue = other.ZSortValue;
        ZSortDepth = other.ZSortDepth;
        BalanceRecord = other.BalanceRecord;
        BalanceVal = other.BalanceVal;
        DamagedTickCounter = other.DamagedTickCounter;
        FrameColTickCounter = other.FrameColTickCounter;
        FrameCollision = other.FrameCollision;
        ModdedXPos = other.ModdedXPos;
        ModdedYPos = other.ModdedYPos;
        ModdedZPos = other.ModdedZPos;
        ModX = other.ModX;
        ModY = other.ModY;
        ModZ = other.ModZ;
        Width = other.Width;
        Depth = other.Depth;
        Height = other.Height;
        HitBoxX = other.HitBoxX;
        HitBoxY = other.HitBoxY;
        HitBoxZ = other.HitBoxZ;
        FrameXOff = other.FrameXOff;
        FrameYOff = other.FrameYOff;
        FrameZOff = other.FrameZOff;
        FrameWidth = other.FrameWidth;
        FrameDepth = other.FrameDepth;
        FrameHeight = other.FrameHeight;
        HitCounter = other.HitCounter;
        TouchingEntity = other.TouchingEntity;
        EventTrigger = other.EventTrigger;
        MapEventProgramId = other.MapEventProgramId;
        LogicContextEntity = other.LogicContextEntity;
        EventProgramState.CopyFrom(other.EventProgramState);
        LastTargetAnimationId = other.LastTargetAnimationId;
        LastTargetDirection = other.LastTargetDirection;
        for (int i = 0; i < Bytes.Length; i++)
        {
            Bytes[i] = other.Bytes[i];
        }

        InitialXPos = other.InitialXPos;
        InitialYPos = other.InitialYPos;
        for (int i = 0; i < AIValues.Length; i++)
        {
            AIValues[i] = other.AIValues[i];
        }
    }

    public void Clear()
    {
        //throw new NotImplementedException();
    }
}