using AlundraEngine.Gameplay;
using AlundraEngine.Gameplay.Scripts;

namespace AlundraEngine.Editor;

public class FrameSnapshot
{
    public Entity[] Entities { get; set; }

    public uint GameRandomSeed { get; set; }
    public uint[] MapFlags { get; set; }
    public uint[] GlobalFlags { get; set; }
    public int LastWarpEntityIndex { get; set; }
    public int TileAnimFrameCounter { get; set; }
    public int DAT_80098f24 { get; set; }
    public int[] INT_ARRAY_800a8284 { get; set; }
    public int GlobalTransitionState { get; set; }
    public uint[] DefaultWarpDestinations { get; set; }
    public uint[] SoundGroupByMapId { get; set; }
    public int[] OrderingTableBuffer { get; set; }
    public int WarpDelayFrames { get; set; }
    public uint PlayerControlFlags { get; set; }
    public int IsWarpDisabled { get; set; }
    public int MapTransitionEffectId { get; set; }
    public uint DesiredMap { get; set; }
    public uint ResetAnimationId { get; set; }
    public uint ResetDirectionId { get; set; }
    public int CameraTargetX { get; set; }
    public int CameraTargetY { get; set; }
    public int CameraTargetZ { get; set; }
    public uint CurrentMap { get; set; }
    public int IsCameraScrolling { get; set; }
    public int CameraScrollingX { get; set; }
    public int CameraScrollingY { get; set; }
    public ScrollingParameters ScrollingParameters { get; set; }
    public PadState PadState1 { get; set; }
    public uint GravityFlag { get; set; }
    public Entity ActiveCollisionEntity { get; set; }
    public int WarpLockTimer { get; set; }
    public Entity[] ActiveEntities { get; set; }
    public Entity[] CollideableEntities { get; set; }
    public int ActiveEntityCount { get; set; }
    public int CollideableEntitiesCount { get; set; }
    public Entity[] VisibleEntities { get; set; }
    public int CameraLookAtX { get; set; }
    public int CameraLookAtY { get; set; }
    public int CameraLookAtZ { get; set; }
    public int VisibleEntityCount { get; set; }
    public int NumberOfEntity { get; set; }
    public Entity EntityFollowedByCamera { get; set; }
    public int NextEntityIndex { get; set; }
    public MapEvent[] MapEvents { get; set; }
    public EventProgramState EventProgramState { get; set; }
    public int MapOffsetX { get; set; }
    public int MapOffsetY { get; set; }
    public int MapScreenPosX { get; set; }
    public int MapScreenPosY { get; set; }
    public uint WarpFlags { get; set; }
    public int PlayerLastX { get; set; }
    public int PlayerLastY { get; set; }
    public int PlayerLastZ { get; set; }
    public int PlayerStartX { get; set; }
    public int PlayerStartY { get; set; }
    public int PlayerStartZ { get; set; }
    public int HudDeltaX { get; set; }
    public int HudDeltaY { get; set; }
    public int HudCurrentX { get; set; }
    public int HudCurrentY { get; set; }
    public int HudX { get; set; }
    public int HudY { get; set; }

    public void CopyToMemory(GameEngine gameEngine)
    {
        for (int i = 0; i < gameEngine.StaticVariables.g_entitySlots.Length; i++)
        {
            gameEngine.StaticVariables.g_entitySlots[i].CopyFrom(Entities[i]);
        }

        Array.Copy(MapFlags, gameEngine.StaticVariables.g_saveData.MapFlags, gameEngine.StaticVariables.g_saveData.MapFlags.Length);
        Array.Copy(GlobalFlags, gameEngine.StaticVariables.g_globalFlags, gameEngine.StaticVariables.g_globalFlags.Length);

        gameEngine.StaticVariables.g_gameRandomSeed = GameRandomSeed;

        gameEngine.StaticVariables.g_lastWarpEntityIndex = LastWarpEntityIndex;
        gameEngine.StaticVariables.g_tileAnimFrameCounter = TileAnimFrameCounter;
        gameEngine.StaticVariables.DAT_80098f24 = DAT_80098f24;
        Array.Copy(INT_ARRAY_800a8284, gameEngine.StaticVariables.INT_ARRAY_800a8284, INT_ARRAY_800a8284.Length);
        gameEngine.StaticVariables.g_globalTransitionState = GlobalTransitionState;
        Array.Copy(DefaultWarpDestinations, gameEngine.StaticVariables.g_defaultWarpDestinations, DefaultWarpDestinations.Length);
        Array.Copy(SoundGroupByMapId, gameEngine.StaticVariables.g_soundGroupByMapId, SoundGroupByMapId.Length);
        Array.Copy(OrderingTableBuffer, gameEngine.StaticVariables.g_orderingTableBuffer, OrderingTableBuffer.Length);
        gameEngine.StaticVariables.g_warpDelayFrames = WarpDelayFrames;
        gameEngine.StaticVariables.g_playerControlFlags = PlayerControlFlags;
        gameEngine.StaticVariables.g_isWarpDisabled = IsWarpDisabled;
        gameEngine.StaticVariables.g_mapTransitionEffectId = MapTransitionEffectId;
        gameEngine.StaticVariables.g_desiredMap = DesiredMap;
        gameEngine.StaticVariables.g_resetAnimationId = ResetAnimationId;
        gameEngine.StaticVariables.g_resetDirectionId = ResetDirectionId;
        gameEngine.StaticVariables.g_cameraTargetX = CameraTargetX;
        gameEngine.StaticVariables.g_cameraTargetY = CameraTargetY;
        gameEngine.StaticVariables.g_cameraTargetZ = CameraTargetZ;
        gameEngine.StaticVariables.g_currentMap = CurrentMap;
        gameEngine.StaticVariables.g_isCameraScrolling = IsCameraScrolling;
        gameEngine.StaticVariables.g_cameraScrollingX = CameraScrollingX;
        gameEngine.StaticVariables.g_cameraScrollingY = CameraScrollingY;
        gameEngine.StaticVariables.g_scrollingParameters = ScrollingParameters;
        gameEngine.StaticVariables.g_padState1 = PadState1.Copy();
        gameEngine.StaticVariables.g_gravityFlag = GravityFlag;
        gameEngine.StaticVariables.g_activeCollisionEntity = ActiveCollisionEntity;
        gameEngine.StaticVariables.g_warpLockTimer = WarpLockTimer;
        Array.Copy(ActiveEntities, gameEngine.StaticVariables.g_activeEntities, ActiveEntities.Length);
        Array.Copy(CollideableEntities, gameEngine.StaticVariables.g_collideableEntities, CollideableEntities.Length);
        gameEngine.StaticVariables.g_activeEntityCount = ActiveEntityCount;
        gameEngine.StaticVariables.g_collideableEntitiesCount = CollideableEntitiesCount;
        Array.Copy(VisibleEntities, gameEngine.StaticVariables.g_visibleEntities, VisibleEntities.Length);
        gameEngine.StaticVariables.g_cameraLookAtX = CameraLookAtX;
        gameEngine.StaticVariables.g_cameraLookAtY = CameraLookAtY;
        gameEngine.StaticVariables.g_cameraLookAtZ = CameraLookAtZ;
        gameEngine.StaticVariables.g_visibleEntityCount = VisibleEntityCount;
        gameEngine.StaticVariables.g_numberOfEntity = NumberOfEntity;
        gameEngine.StaticVariables.g_entityFollowedByCamera = EntityFollowedByCamera;
        gameEngine.StaticVariables.g_nextEntityIndex = NextEntityIndex;
        //Array.Copy(MapEvents, gameEngine.StaticVariables.g_mapEvents, MapEvents.Length);
        gameEngine.StaticVariables.g_eventProgramState.CopyFrom(EventProgramState);
        gameEngine.StaticVariables.g_mapOffsetX = MapOffsetX;
        gameEngine.StaticVariables.g_mapOffsetY = MapOffsetY;
        gameEngine.StaticVariables.g_mapScreenPosX = MapScreenPosX;
        gameEngine.StaticVariables.g_mapScreenPosY = MapScreenPosY;
        gameEngine.StaticVariables.g_warpFlags = WarpFlags;
        gameEngine.StaticVariables.g_warpFadeColorR = PlayerLastX;
        gameEngine.StaticVariables.g_warpFadeColorG = PlayerLastY;
        gameEngine.StaticVariables.g_warpFadeColorB = PlayerLastZ;
        gameEngine.StaticVariables.g_warpFadeColorR_Target = PlayerStartX;
        gameEngine.StaticVariables.g_warpFadeColorG_Target = PlayerStartY;
        gameEngine.StaticVariables.g_warpFadeColorB_Target = PlayerStartZ;
        gameEngine.StaticVariables.g_hudDeltaX = HudDeltaX;
        gameEngine.StaticVariables.g_hudDeltaY = HudDeltaY;
        //gameEngine.StaticVariables.g_hudCurrentX = HudCurrentX;
        //gameEngine.StaticVariables.g_hudCurrentY = HudCurrentY;
        gameEngine.StaticVariables.g_hudX = HudX;
        gameEngine.StaticVariables.g_hudY = HudY;
    }

    public void CopyFromMemory(GameEngine gameEngine)
    {
        Entities = new Entity[gameEngine.StaticVariables.g_entitySlots.Length];

        for (int i = 0; i < gameEngine.StaticVariables.g_entitySlots.Length; i++)
        {
            Entities[i] = new Entity();
            Entities[i].CopyFrom(gameEngine.StaticVariables.g_entitySlots[i]);
        }
        ;

        MapFlags = (uint[])gameEngine.StaticVariables.g_saveData.MapFlags.Clone();
        GlobalFlags = (uint[])gameEngine.StaticVariables.g_globalFlags.Clone();

        GameRandomSeed = gameEngine.StaticVariables.g_gameRandomSeed;

        LastWarpEntityIndex = gameEngine.StaticVariables.g_lastWarpEntityIndex;
        TileAnimFrameCounter = gameEngine.StaticVariables.g_tileAnimFrameCounter;
        DAT_80098f24 = gameEngine.StaticVariables.DAT_80098f24;
        INT_ARRAY_800a8284 = (int[])gameEngine.StaticVariables.INT_ARRAY_800a8284.Clone();
        GlobalTransitionState = gameEngine.StaticVariables.g_globalTransitionState;
        DefaultWarpDestinations = (uint[])gameEngine.StaticVariables.g_defaultWarpDestinations.Clone();
        SoundGroupByMapId = (uint[])gameEngine.StaticVariables.g_soundGroupByMapId.Clone();
        OrderingTableBuffer = (int[])gameEngine.StaticVariables.g_orderingTableBuffer.Clone();
        WarpDelayFrames = gameEngine.StaticVariables.g_warpDelayFrames;
        PlayerControlFlags = gameEngine.StaticVariables.g_playerControlFlags;
        IsWarpDisabled = gameEngine.StaticVariables.g_isWarpDisabled;
        MapTransitionEffectId = gameEngine.StaticVariables.g_mapTransitionEffectId;
        DesiredMap = gameEngine.StaticVariables.g_desiredMap;
        ResetAnimationId = gameEngine.StaticVariables.g_resetAnimationId;
        ResetDirectionId = gameEngine.StaticVariables.g_resetDirectionId;
        CameraTargetX = gameEngine.StaticVariables.g_cameraTargetX;
        CameraTargetY = gameEngine.StaticVariables.g_cameraTargetY;
        CameraTargetZ = gameEngine.StaticVariables.g_cameraTargetZ;
        CurrentMap = gameEngine.StaticVariables.g_currentMap;
        IsCameraScrolling = gameEngine.StaticVariables.g_isCameraScrolling;
        CameraScrollingX = gameEngine.StaticVariables.g_cameraScrollingX;
        CameraScrollingY = gameEngine.StaticVariables.g_cameraScrollingY;
        ScrollingParameters = gameEngine.StaticVariables.g_scrollingParameters;
        PadState1 = gameEngine.StaticVariables.g_padState1.Copy();
        GravityFlag = gameEngine.StaticVariables.g_gravityFlag;
        ActiveCollisionEntity = gameEngine.StaticVariables.g_activeCollisionEntity;
        WarpLockTimer = gameEngine.StaticVariables.g_warpLockTimer;
        ActiveEntities = (Entity[])gameEngine.StaticVariables.g_activeEntities.Clone();
        CollideableEntities = (Entity[])gameEngine.StaticVariables.g_collideableEntities.Clone();
        ActiveEntityCount = gameEngine.StaticVariables.g_activeEntityCount;
        CollideableEntitiesCount = gameEngine.StaticVariables.g_collideableEntitiesCount;
        VisibleEntities = (Entity[])gameEngine.StaticVariables.g_visibleEntities.Clone();
        CameraLookAtX = gameEngine.StaticVariables.g_cameraLookAtX;
        CameraLookAtY = gameEngine.StaticVariables.g_cameraLookAtY;
        CameraLookAtZ = gameEngine.StaticVariables.g_cameraLookAtZ;
        VisibleEntityCount = gameEngine.StaticVariables.g_visibleEntityCount;
        NumberOfEntity = gameEngine.StaticVariables.g_numberOfEntity;
        EntityFollowedByCamera = gameEngine.StaticVariables.g_entityFollowedByCamera;
        NextEntityIndex = gameEngine.StaticVariables.g_nextEntityIndex;
        MapEvents = (MapEvent[])gameEngine.StaticVariables.g_mapEvents.Clone();
        EventProgramState = new EventProgramState();
        EventProgramState.CopyFrom(gameEngine.StaticVariables.g_eventProgramState);
        MapOffsetX = gameEngine.StaticVariables.g_mapOffsetX;
        MapOffsetY = gameEngine.StaticVariables.g_mapOffsetY;
        MapScreenPosX = gameEngine.StaticVariables.g_mapScreenPosX;
        MapScreenPosY = gameEngine.StaticVariables.g_mapScreenPosY;
        WarpFlags = gameEngine.StaticVariables.g_warpFlags;
        PlayerLastX = gameEngine.StaticVariables.g_warpFadeColorR;
        PlayerLastY = gameEngine.StaticVariables.g_warpFadeColorG;
        PlayerLastZ = gameEngine.StaticVariables.g_warpFadeColorB;
        PlayerStartX = gameEngine.StaticVariables.g_warpFadeColorR_Target;
        PlayerStartY = gameEngine.StaticVariables.g_warpFadeColorG_Target;
        PlayerStartZ = gameEngine.StaticVariables.g_warpFadeColorB_Target;
        HudDeltaX = gameEngine.StaticVariables.g_hudDeltaX;
        HudDeltaY = gameEngine.StaticVariables.g_hudDeltaY;
        HudCurrentX = gameEngine.StaticVariables.g_hudCurrentX;
        HudCurrentY = gameEngine.StaticVariables.g_hudCurrentY;
        HudX = gameEngine.StaticVariables.g_hudX;
        HudY = gameEngine.StaticVariables.g_hudY;
    }
}