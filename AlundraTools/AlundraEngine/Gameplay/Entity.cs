using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay.Scripts;
using AlundraEngine.Sound;

namespace AlundraEngine.Gameplay;

public class Entity
{
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
    public Entity? CarriedEntity;
    public int RelativeWarpOffsetX;
    public int RelativeWarpOffsetY;
    public int RelativeWarpOffsetZ;
    public uint ContentsItemId; //3c
    public int ContentsGameFlag;
    public SiEntityRecord? EntityRecord;
    public int EntityRefId;
    public readonly int[] ProgramIndexes = new int[6]; //4c
    public SpriteRecord? SpriteRecord;
    public uint SpriteTableIndex;
    public uint Flags;//0x800000 = portrait,0x0100 = gravity,0xf = ?, 0x1 = ? , 0x80 = collidable
    public readonly int[] SpriteProgramIndexes = new int[6]; //70
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
    public int TargetForceX, TargetForceY;
    public int ForceX, ForceY;
    public int PreviousAdjustedForceX;//?cc
    public int PreviousAdjustedForceY;//?d0
    public int ForceStepX, ForceStepY;//d4,d8
    public int AdjustedForceX, AdjustedForceY;//dc,e0
    public int FinalForceX, FinalForceY, FinalForceZ;//e4,e8,ec
    public int Acceleration;//f0
    public int Speed;//f4
    public int IsZForceApplied;//this is probably named wrong, has to do with animation  f8
    public int ScreenClipX, ScreenClipY, ScreenClipZ;
    public int NegModX, NegModY, NegModZ;
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
    public int ForceAdjusted;//0x13c
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
    public int SpriteSheetOffset, PaletteOffset;//represents offset where the pallets and sheets are in memory for map vs global sprites, prob not used with my engine
    public SpriteEffect? ActiveEffect;
    public int ZSortValue;//1bc
    public int ZSortDepth;//1c0
    public BalanceRecord? BalanceRecord;//1c4
    public BalanceAnimValRef? BalanceAnimValRef;//1c8
    public int DamagedTickCounter;//1cc
    public int FrameCollisionTickCounter;//1d0
    public FrameCollisionData? FrameCollision;//1d4
    public int ModdedPosX, ModdedPosY, ModdedPosZ;
    public int ModX, ModY, ModZ;
    public int Width, Depth, Height;
    //this set of vars is set when an animation has a frame with attached data
    public int HitBoxX;//1fc
    public int HitBoxY;//200
    public int HitBoxZ;//204
    public int CollisionOffsetX;//208
    public int CollisionOffsetY;//20c
    public int CollisionOffsetZ;//210
    public int CollisionWidth;//214
    public int CollisionDepth;//218
    public int CollisionHeight;//21c
    public int HitCounter;//220
    public Entity? TouchingEntity;//224
    public int EventTrigger;//228  for the player character this holds the id of the map event that is triggering, for other entities this holds the type of event slot to trigger
    public int MapEventProgramId;//22c
    public Entity LogicContextEntity; //self
    public readonly EventProgramState EventProgramState = new();
    public byte _268;//0x268
    public byte _269;//0x269
    public byte _26a;//0x26a
    public byte _26b;//0x26b
    public uint LastTargetAnimationId;//26c
    public uint LastTargetDirection;//270
    public byte[] Bytes = new byte[4];
    public int ItemDelay;//278
    public int ItemState;
    public short[] AIValues = new short[10];//280

    public bool IsMapSprite => EntityRecord != null && (EntityRecord.SpriteDirection & 0x80) != 0;

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
        CarriedEntity = other.CarriedEntity;
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

        SpriteRecord = other.SpriteRecord;
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
        TargetForceX = other.TargetForceX;
        TargetForceY = other.TargetForceY;
        ForceX = other.ForceX;
        ForceY = other.ForceY;
        PreviousAdjustedForceX = other.PreviousAdjustedForceX;
        PreviousAdjustedForceY = other.PreviousAdjustedForceY;
        ForceStepX = other.ForceStepX;
        ForceStepY = other.ForceStepY;
        AdjustedForceX = other.AdjustedForceX;
        AdjustedForceY = other.AdjustedForceY;
        FinalForceX = other.FinalForceX;
        FinalForceY = other.FinalForceY;
        FinalForceZ = other.FinalForceZ;
        Acceleration = other.Acceleration;
        Speed = other.Speed;
        IsZForceApplied = other.IsZForceApplied;
        ScreenClipX = other.ScreenClipX;
        ScreenClipY = other.ScreenClipY;
        ScreenClipZ = other.ScreenClipZ;
        NegModX = other.NegModX;
        NegModY = other.NegModY;
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
        SpriteSheetOffset = other.SpriteSheetOffset;
        PaletteOffset = other.PaletteOffset;
        ActiveEffect = other.ActiveEffect;
        ZSortValue = other.ZSortValue;
        ZSortDepth = other.ZSortDepth;
        BalanceRecord = other.BalanceRecord;
        BalanceAnimValRef = other.BalanceAnimValRef;
        DamagedTickCounter = other.DamagedTickCounter;
        FrameCollisionTickCounter = other.FrameCollisionTickCounter;
        FrameCollision = other.FrameCollision;
        ModdedPosX = other.ModdedPosX;
        ModdedPosY = other.ModdedPosY;
        ModdedPosZ = other.ModdedPosZ;
        ModX = other.ModX;
        ModY = other.ModY;
        ModZ = other.ModZ;
        Width = other.Width;
        Depth = other.Depth;
        Height = other.Height;
        HitBoxX = other.HitBoxX;
        HitBoxY = other.HitBoxY;
        HitBoxZ = other.HitBoxZ;
        CollisionOffsetX = other.CollisionOffsetX;
        CollisionOffsetY = other.CollisionOffsetY;
        CollisionOffsetZ = other.CollisionOffsetZ;
        CollisionWidth = other.CollisionWidth;
        CollisionDepth = other.CollisionDepth;
        CollisionHeight = other.CollisionHeight;
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

        ItemDelay = other.ItemDelay;
        ItemState = other.ItemState;
        for (int i = 0; i < AIValues.Length; i++)
        {
            AIValues[i] = other.AIValues[i];
        }
    }

    public void Clear()
    {
        Index = 0;
        Index2 = 0;
        ChildEntity = null;
        ParentEntity = null;
        Status = 0;
        Hp = 0;
        HpMax = 0;
        FrameCounter = 0;
        IsNotProcessable = 0;
        Flags2 = 0;
        PlatformEntity = null;
        CarriedEntity = null;
        RelativeWarpOffsetX = 0;
        RelativeWarpOffsetY = 0;
        RelativeWarpOffsetZ = 0;
        ContentsItemId = 0;
        ContentsGameFlag = 0;
        EntityRecord = null;
        EntityRefId = 0;

        Array.Clear(ProgramIndexes);

        SpriteRecord = null;
        SpriteTableIndex = 0;
        Flags = 0;

        Array.Clear(SpriteProgramIndexes);

        TargetAnimationId = 0;
        TargetDirection = 0;
        CurrentAnimationId = 0;
        CurrentDirection = 0;
        CurrentFrameIndex = 0;
        AnimSet = null;
        FirstFrame = null;
        Frame = null;
        NextFrameDelay = 0;
        ForceResetAnimationFlag = 0;
        AnimCompleteCounter = 0;
        AnimFlags = 0;
        ForceZ = 0;
        TargetForceX = 0;
        TargetForceY = 0;
        ForceX = 0;
        ForceY = 0;
        PreviousAdjustedForceX = 0;
        PreviousAdjustedForceY = 0;
        ForceStepX = 0;
        ForceStepY = 0;
        AdjustedForceX = 0;
        AdjustedForceY = 0;
        FinalForceX = 0;
        FinalForceY = 0;
        FinalForceZ = 0;
        Acceleration = 0;
        Speed = 0;
        IsZForceApplied = 0;
        ScreenClipX = 0;
        ScreenClipY = 0;
        ScreenClipZ = 0;
        NegModX = 0;
        NegModY = 0;
        NegModZ = 0;
        PosX = 0;
        PosY = 0;
        PosZ = 0;
        TileX = 0;
        TileY = 0;
        TileZ = 0;
        RidingEntity = null;
        XCollisionEntity = null;
        FloorHeight = 0;
        TerrainHeight = 0;
        ForceAdjusted = 0;
        CollidedWithEntityZ = 0;
        IsAboveGround = 0;

        Array.Clear(MapTiles);
        Array.Clear(MapHeights);

        PlatformUpdateFlag = 0;
        _16c = 0;
        HitBoxOriginX = 0;
        HitBoxOriginY = 0;
        HitBoxOriginZ = 0;
        _17c = 0;
        CombinedVramFlagsOR = 0;
        CombinedVramFlagsAND = 0;
        TileAttributes = 0;
        Slope_18c = 0;
        Slope_190 = 0;

        SpriteRef.Images = null;
        SpriteRef.X = 0;
        SpriteRef.Y = 0;
        SpriteRef.Z = 0;
        SpriteRef.DepthSortValue = 0;
        SpriteRef.NumberOfImages = 0;

        SpriteSheetOffset = 0;
        PaletteOffset = 0;
        ActiveEffect = null;
        ZSortValue = 0;
        ZSortDepth = 0;
        BalanceRecord = null;
        BalanceAnimValRef = null;
        DamagedTickCounter = 0;
        FrameCollisionTickCounter = 0;
        FrameCollision = null;
        ModdedPosX = 0;
        ModdedPosY = 0;
        ModdedPosZ = 0;
        ModX = 0;
        ModY = 0;
        ModZ = 0;
        Width = 0;
        Depth = 0;
        Height = 0;
        HitBoxX = 0;
        HitBoxY = 0;
        HitBoxZ = 0;
        CollisionOffsetX = 0;
        CollisionOffsetY = 0;
        CollisionOffsetZ = 0;
        CollisionWidth = 0;
        CollisionDepth = 0;
        CollisionHeight = 0;
        HitCounter = 0;
        TouchingEntity = null;
        EventTrigger = 0;
        MapEventProgramId = 0;
        LogicContextEntity = null;

        EventProgramState.Sp = 0;
        Array.Clear(EventProgramState.Parameters);
        EventProgramState.Result = 0;
        EventProgramState._30 = 0;
        EventProgramState._34 = 0;
        EventProgramState.Codes = null;
        EventProgramState.CodeIndex = 0;

        LastTargetAnimationId = 0;
        LastTargetDirection = 0;

        Array.Clear(Bytes);

        ItemDelay = 0;
        ItemState = 0;

        Array.Clear(AIValues);
    }

    public override string ToString()
    {
        return $"#{Index} #{Index2}";
    }
}