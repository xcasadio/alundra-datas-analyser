using AlundraEngine.Gameplay;
using AlundraEngine.Gameplay.Scripts;

namespace AlundraEngine.Editor;

public class FrameSnapshot
{
    private uint[] MapFlags { get; set; }
    private uint[] GlobalFlags { get; set; }
    private Entity[] Entities { get; set; }
    private uint GameRandomSeed { get; set; }

    private int LastWarpEntityIndex { get; set; }
    private int TileAnimFrameCounter { get; set; }
    private int DAT_80098f24 { get; set; }
    private int[] INT_ARRAY_800a8284 { get; set; }
    private short SoundFadeTimer { get; set; }
    private int GlobalTransitionState { get; set; }
    private uint[] DefaultWarpDestinations { get; set; }
    private uint[] SoundGroupByMapId { get; set; }
    private int[] OrderingTableBuffer { get; set; }
    private int WarpDelayFrames { get; set; }
    private int PlayerControlFlags { get; set; }
    private int IsWarpDisabled { get; set; }
    private int WarpType { get; set; }
    private int DesiredMap { get; set; }
    private int WarpTriggerType { get; set; }
    private int WarpExtraParam { get; set; }
    private int CameraTargetX { get; set; }
    private int CameraTargetY { get; set; }
    private int AnimationId { get; set; }
    private int CurrentMap { get; set; }
    private int IsCameraScrolling { get; set; }
    private int CameraScrollingX { get; set; }
    private int CameraScrollingY { get; set; }
    private int BossCutsceneFlag { get; set; }
    private int CameraOffsetX { get; set; }
    private int CameraOffsetY { get; set; }
    private PadState PadState1 { get; set; }
    private int GravityFlag { get; set; }
    private Entity ActiveCollisionEntity { get; set; }
    private int WarpLockTimer { get; set; }
    private Entity[] ActiveEntities { get; set; }
    private Entity[] CollideableEntities { get; set; }
    private int ActiveEntityCount { get; set; }
    private int CollideableEntitiesCount { get; set; }
    private Entity[] VisibleEntities { get; set; }
    private int CameraLookAtX { get; set; }
    private int CameraLookAtY { get; set; }
    private int CameraLookAtZ { get; set; }
    private int VisibleEntityCount { get; set; }
    private int NumberOfEntity { get; set; }
    private Entity EntityFollowedByCamera { get; set; }
    private int NextEntityIndex { get; set; }
    private MapEvent[] MapEvents { get; set; }
    private EventProgramState EventProgramState { get; set; }
    private int MapOffsetX { get; set; }
    private int MapOffsetY { get; set; }
    private int MapScreenPosX { get; set; }
    private int MapScreenPosY { get; set; }
    private int WarpFlags { get; set; }
    private int PlayerLastX { get; set; }
    private int PlayerLastY { get; set; }
    private int PlayerLastZ { get; set; }
    private int PlayerStartX { get; set; }
    private int PlayerStartY { get; set; }
    private int PlayerStartZ { get; set; }
    private int CameraDeltaX { get; set; }
    private int CameraDeltaY { get; set; }
    private int CameraCurrentX { get; set; }
    private int CameraCurrentY { get; set; }
    private int CameraX { get; set; }
    private int CameraY { get; set; }
    private int CutsceneScrollLimitX { get; set; }
    private int CutsceneScrollLimitY { get; set; }
    private int CutsceneScrollSpeedX { get; set; }
    private int CutsceneScrollSpeedY { get; set; }
    private int CutsceneXReachedMin { get; set; }
    private int CutsceneYReachedMin { get; set; }

    public void CopyToMemory()
    {
        for (int i = 0; i < StaticVariables.g_entitySlots.Length; i++)
        {
            StaticVariables.g_entitySlots[i].CopyFrom(Entities[i]);
        }

        Array.Copy(MapFlags, StaticVariables.g_mapFlags, StaticVariables.g_mapFlags.Length);
        Array.Copy(GlobalFlags, StaticVariables.g_globalFlags, StaticVariables.g_globalFlags.Length);

        StaticVariables.g_gameRandomSeed = GameRandomSeed;

        StaticVariables.g_lastWarpEntityIndex = LastWarpEntityIndex;
        StaticVariables.g_tileAnimFrameCounter = TileAnimFrameCounter;
        StaticVariables.DAT_80098f24 = DAT_80098f24;
        Array.Copy(INT_ARRAY_800a8284, StaticVariables.INT_ARRAY_800a8284, INT_ARRAY_800a8284.Length);
        StaticVariables.g_soundFadeTimer = SoundFadeTimer;
        StaticVariables.g_globalTransitionState = GlobalTransitionState;
        Array.Copy(DefaultWarpDestinations, StaticVariables.g_defaultWarpDestinations, DefaultWarpDestinations.Length);
        Array.Copy(SoundGroupByMapId, StaticVariables.g_soundGroupByMapId, SoundGroupByMapId.Length);
        Array.Copy(OrderingTableBuffer, StaticVariables.g_orderingTableBuffer, OrderingTableBuffer.Length);
        StaticVariables.g_warpDelayFrames = WarpDelayFrames;
        StaticVariables.g_playerControlFlags = PlayerControlFlags;
        StaticVariables.g_isWarpDisabled = IsWarpDisabled;
        StaticVariables.g_warpType = WarpType;
        StaticVariables.g_desiredMap = DesiredMap;
        StaticVariables.g_warpTriggerType = WarpTriggerType;
        StaticVariables.g_warpExtraParam = WarpExtraParam;
        StaticVariables.g_cameraTargetX = CameraTargetX;
        StaticVariables.g_cameraTargetY = CameraTargetY;
        StaticVariables.g_animation_id = AnimationId;
        StaticVariables.g_currentMap = CurrentMap;
        StaticVariables.g_isCameraScrolling = IsCameraScrolling;
        StaticVariables.g_cameraScrollingX = CameraScrollingX;
        StaticVariables.g_cameraScrollingY = CameraScrollingY;
        StaticVariables.g_bossCutsceneFlag = BossCutsceneFlag;
        StaticVariables.g_cameraOffsetX = CameraOffsetX;
        StaticVariables.g_cameraOffsetY = CameraOffsetY;
        StaticVariables.g_padState1 = PadState1.Copy();
        StaticVariables.g_gravityFlag = GravityFlag;
        StaticVariables.g_activeCollisionEntity = ActiveCollisionEntity;
        StaticVariables.g_warpLockTimer = WarpLockTimer;
        Array.Copy(ActiveEntities, StaticVariables.g_activeEntities, ActiveEntities.Length);
        Array.Copy(CollideableEntities, StaticVariables.g_collideableEntities, CollideableEntities.Length);
        StaticVariables.g_activeEntityCount = ActiveEntityCount;
        StaticVariables.g_collideableEntitiesCount = CollideableEntitiesCount;
        Array.Copy(VisibleEntities, StaticVariables.g_visibleEntities, VisibleEntities.Length);
        StaticVariables.g_cameraLookAtX = CameraLookAtX;
        StaticVariables.g_cameraLookAtY = CameraLookAtY;
        StaticVariables.g_cameraLookAtZ = CameraLookAtZ;
        StaticVariables.g_visibleEntityCount = VisibleEntityCount;
        StaticVariables.g_numberOfEntity = NumberOfEntity;
        StaticVariables.g_entityFollowedByCamera = EntityFollowedByCamera;
        StaticVariables.g_nextEntityIndex = NextEntityIndex;
        Array.Copy(MapEvents, StaticVariables.g_mapEvents, MapEvents.Length);
        StaticVariables.g_eventProgramState.CopyFrom(EventProgramState);
        StaticVariables.g_mapOffsetX = MapOffsetX;
        StaticVariables.g_mapOffsetY = MapOffsetY;
        StaticVariables.g_mapScreenPosX = MapScreenPosX;
        StaticVariables.g_mapScreenPosY = MapScreenPosY;
        StaticVariables.g_warpFlags = WarpFlags;
        StaticVariables.g_playerLastX = PlayerLastX;
        StaticVariables.g_playerLastY = PlayerLastY;
        StaticVariables.g_playerLastZ = PlayerLastZ;
        StaticVariables.g_playerStartX = PlayerStartX;
        StaticVariables.g_playerStartY = PlayerStartY;
        StaticVariables.g_playerStartZ = PlayerStartZ;
        StaticVariables.g_cameraDeltaX = CameraDeltaX;
        StaticVariables.g_cameraDeltaY = CameraDeltaY;
        StaticVariables.g_cameraCurrentX = CameraCurrentX;
        StaticVariables.g_cameraCurrentY = CameraCurrentY;
        StaticVariables.g_cameraX = CameraX;
        StaticVariables.g_cameraY = CameraY;
        StaticVariables.g_cutsceneScrollLimitX = CutsceneScrollLimitX;
        StaticVariables.g_cutsceneScrollLimitY = CutsceneScrollLimitY;
        StaticVariables.g_cutsceneScrollSpeedX = CutsceneScrollSpeedX;
        StaticVariables.g_cutsceneScrollSpeedY = CutsceneScrollSpeedY;
        StaticVariables.g_cutsceneXReachedMin = CutsceneXReachedMin;
        StaticVariables.g_cutsceneYReachedMin = CutsceneYReachedMin;
    }

    public void CopyFromMemory()
    {
        Entities = new Entity[StaticVariables.g_entitySlots.Length];

        for (int i = 0; i < StaticVariables.g_entitySlots.Length; i++)
        {
            Entities[i] = new Entity();
            Entities[i].CopyFrom(StaticVariables.g_entitySlots[i]);
        }
        ;

        MapFlags = (uint[])StaticVariables.g_mapFlags.Clone();
        GlobalFlags = (uint[])StaticVariables.g_globalFlags.Clone();

        GameRandomSeed = StaticVariables.g_gameRandomSeed;

        LastWarpEntityIndex = StaticVariables.g_lastWarpEntityIndex;
        TileAnimFrameCounter = StaticVariables.g_tileAnimFrameCounter;
        DAT_80098f24 = StaticVariables.DAT_80098f24;
        INT_ARRAY_800a8284 = (int[])StaticVariables.INT_ARRAY_800a8284.Clone();
        SoundFadeTimer = StaticVariables.g_soundFadeTimer;
        GlobalTransitionState = StaticVariables.g_globalTransitionState;
        DefaultWarpDestinations = (uint[])StaticVariables.g_defaultWarpDestinations.Clone();
        SoundGroupByMapId = (uint[])StaticVariables.g_soundGroupByMapId.Clone();
        OrderingTableBuffer = (int[])StaticVariables.g_orderingTableBuffer.Clone();
        WarpDelayFrames = StaticVariables.g_warpDelayFrames;
        PlayerControlFlags = StaticVariables.g_playerControlFlags;
        IsWarpDisabled = StaticVariables.g_isWarpDisabled;
        WarpType = StaticVariables.g_warpType;
        DesiredMap = StaticVariables.g_desiredMap;
        WarpTriggerType = StaticVariables.g_warpTriggerType;
        WarpExtraParam = StaticVariables.g_warpExtraParam;
        CameraTargetX = StaticVariables.g_cameraTargetX;
        CameraTargetY = StaticVariables.g_cameraTargetY;
        AnimationId = StaticVariables.g_animation_id;
        CurrentMap = StaticVariables.g_currentMap;
        IsCameraScrolling = StaticVariables.g_isCameraScrolling;
        CameraScrollingX = StaticVariables.g_cameraScrollingX;
        CameraScrollingY = StaticVariables.g_cameraScrollingY;
        BossCutsceneFlag = StaticVariables.g_bossCutsceneFlag;
        CameraOffsetX = StaticVariables.g_cameraOffsetX;
        CameraOffsetY = StaticVariables.g_cameraOffsetY;
        PadState1 = StaticVariables.g_padState1.Copy();
        GravityFlag = StaticVariables.g_gravityFlag;
        ActiveCollisionEntity = StaticVariables.g_activeCollisionEntity;
        WarpLockTimer = StaticVariables.g_warpLockTimer;
        ActiveEntities = (Entity[])StaticVariables.g_activeEntities.Clone();
        CollideableEntities = (Entity[])StaticVariables.g_collideableEntities.Clone();
        ActiveEntityCount = StaticVariables.g_activeEntityCount;
        CollideableEntitiesCount = StaticVariables.g_collideableEntitiesCount;
        VisibleEntities = (Entity[])StaticVariables.g_visibleEntities.Clone();
        CameraLookAtX = StaticVariables.g_cameraLookAtX;
        CameraLookAtY = StaticVariables.g_cameraLookAtY;
        CameraLookAtZ = StaticVariables.g_cameraLookAtZ;
        VisibleEntityCount = StaticVariables.g_visibleEntityCount;
        NumberOfEntity = StaticVariables.g_numberOfEntity;
        EntityFollowedByCamera = StaticVariables.g_entityFollowedByCamera;
        NextEntityIndex = StaticVariables.g_nextEntityIndex;
        MapEvents = (MapEvent[])StaticVariables.g_mapEvents.Clone();
        EventProgramState = new EventProgramState();
        EventProgramState.CopyFrom(StaticVariables.g_eventProgramState);
        MapOffsetX = StaticVariables.g_mapOffsetX;
        MapOffsetY = StaticVariables.g_mapOffsetY;
        MapScreenPosX = StaticVariables.g_mapScreenPosX;
        MapScreenPosY = StaticVariables.g_mapScreenPosY;
        WarpFlags = StaticVariables.g_warpFlags;
        PlayerLastX = StaticVariables.g_playerLastX;
        PlayerLastY = StaticVariables.g_playerLastY;
        PlayerLastZ = StaticVariables.g_playerLastZ;
        PlayerStartX = StaticVariables.g_playerStartX;
        PlayerStartY = StaticVariables.g_playerStartY;
        PlayerStartZ = StaticVariables.g_playerStartZ;
        CameraDeltaX = StaticVariables.g_cameraDeltaX;
        CameraDeltaY = StaticVariables.g_cameraDeltaY;
        CameraCurrentX = StaticVariables.g_cameraCurrentX;
        CameraCurrentY = StaticVariables.g_cameraCurrentY;
        CameraX = StaticVariables.g_cameraX;
        CameraY = StaticVariables.g_cameraY;
        CutsceneScrollLimitX = StaticVariables.g_cutsceneScrollLimitX;
        CutsceneScrollLimitY = StaticVariables.g_cutsceneScrollLimitY;
        CutsceneScrollSpeedX = StaticVariables.g_cutsceneScrollSpeedX;
        CutsceneScrollSpeedY = StaticVariables.g_cutsceneScrollSpeedY;
        CutsceneXReachedMin = StaticVariables.g_cutsceneXReachedMin;
        CutsceneYReachedMin = StaticVariables.g_cutsceneYReachedMin;
    }
}