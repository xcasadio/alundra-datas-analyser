using System.Diagnostics;
using System.Text.Json;
using AlundraEngine.Gameplay;
using AlundraEngine.Gameplay.Scripts;

namespace AlundraEngine.Editor;

public static class FrameSnapshotLoader
{
    public static FrameSnapshot LoadFromJson(string filePath)
    {
        string json = File.ReadAllText(filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true
        };

        FrameDump dump = null;

        try
        {
            dump = JsonSerializer.Deserialize<FrameDump>(json, options);
        }
        catch (Exception e)
        {
            Debugger.Break();
            Console.WriteLine(e);
        }

        var frameSnapshot = new FrameSnapshot();
        frameSnapshot.CopyFromMemory(); // TODO : remove

        //frameSnapshot.Entities = new Entity[StaticVariables.g_entitySlots.Length];
        for (int i = 0; i < frameSnapshot.Entities.Length; i++)
        {
            // TODO : create a new entity
            // we use frameSnapshot.CopyFromMemory() to copy all assets
            //frameSnapshot.Entities[i] = new Entity();

            if (i < dump.entities.Count)
            {
                dump.entities[i].CopyToEntity(frameSnapshot.Entities[i]);
            }
        }

        frameSnapshot.MapFlags = dump.g_mapFlags;
        frameSnapshot.GlobalFlags = dump.g_globalFlags;
        frameSnapshot.GameRandomSeed = dump.g_gameRandomSeed;
        frameSnapshot.LastWarpEntityIndex = dump.g_lastWarpEntityIndex;
        frameSnapshot.TileAnimFrameCounter = dump.g_TileAnimFrameCounter;
        frameSnapshot.DAT_80098f24 = dump.DAT_80098f24;
        frameSnapshot.INT_ARRAY_800a8284 = dump.INT_ARRAY_800a8284;
        frameSnapshot.SoundFadeTimer = dump.g_soundFadeTimer;
        frameSnapshot.GlobalTransitionState = dump.g_globalTransitionState;
        frameSnapshot.DefaultWarpDestinations = dump.g_defaultWarpDestinations;
        frameSnapshot.SoundGroupByMapId = dump.g_soundGroupByMapId;
        frameSnapshot.OrderingTableBuffer = dump.g_orderingTableBuffer;
        frameSnapshot.WarpDelayFrames = dump.g_warpDelayFrames;
        frameSnapshot.PlayerControlFlags = dump.g_playerControlFlags;
        frameSnapshot.IsWarpDisabled = dump.g_isWarpDisabled;
        frameSnapshot.WarpType = dump.g_warpType;
        frameSnapshot.DesiredMap = dump.g_desiredMap;
        frameSnapshot.WarpTriggerType = dump.g_warpTriggerType;
        frameSnapshot.WarpExtraParam = dump.g_warpExtraParam;
        frameSnapshot.CameraTargetX = dump.g_cameraTargetX;
        frameSnapshot.CameraTargetY = dump.g_cameraTargetY;
        frameSnapshot.AnimationId = dump.g_animationId;
        frameSnapshot.CurrentMap = dump.g_currentMap;
        frameSnapshot.IsCameraScrolling = dump.g_isCameraScrolling;
        frameSnapshot.CameraScrollingX = dump.g_cameraScrollingX;
        frameSnapshot.CameraScrollingY = dump.g_cameraScrollingY;
        frameSnapshot.BossCutsceneFlag = dump.g_bossCutsceneFlag;
        frameSnapshot.CameraOffsetX = dump.g_cameraOffsetX;
        frameSnapshot.CameraOffsetY = dump.g_cameraOffsetY;

        if (dump.g_padState1 != null)
        {
            frameSnapshot.PadState1 = dump.g_padState1;
        }

        frameSnapshot.GravityFlag = dump.g_gravityFlag;

        frameSnapshot.ActiveCollisionEntity = GetEntityFromIndex(dump.g_activeCollisionEntity, frameSnapshot.Entities);
        frameSnapshot.WarpLockTimer = dump.g_warpLockTimer;

        frameSnapshot.ActiveEntities = new Entity[dump.g_activeEntities?.Length ?? 0];
        for (int i = 0; i < frameSnapshot.ActiveEntities.Length; i++)
        {
            frameSnapshot.ActiveEntities[i] = GetEntityFromIndex(dump.g_activeEntities[i], frameSnapshot.Entities);
        }

        frameSnapshot.CollideableEntities = new Entity[dump.g_collideableEntities?.Length ?? 0];
        for (int i = 0; i < frameSnapshot.CollideableEntities.Length; i++)
        {
            frameSnapshot.CollideableEntities[i] = GetEntityFromIndex(dump.g_collideableEntities[i], frameSnapshot.Entities);
        }

        frameSnapshot.ActiveEntityCount = dump.g_activeEntityCount;
        frameSnapshot.CollideableEntitiesCount = dump.g_collideableEntitiesCount;

        frameSnapshot.VisibleEntities = new Entity[dump.g_visibleEntities?.Length ?? 0];
        for (int i = 0; i < frameSnapshot.VisibleEntities.Length; i++)
        {
            frameSnapshot.VisibleEntities[i] = GetEntityFromIndex(dump.g_visibleEntities[i], frameSnapshot.Entities);
        }

        frameSnapshot.CameraLookAtX = dump.g_cameraLookAtX;
        frameSnapshot.CameraLookAtY = dump.g_cameraLookAtY;
        frameSnapshot.CameraLookAtZ = dump.g_cameraLookAtZ;
        frameSnapshot.VisibleEntityCount = dump.g_visibleEntityCount;
        frameSnapshot.NumberOfEntity = dump.g_numberOfEntity;
        frameSnapshot.EntityFollowedByCamera = GetEntityFromIndex(dump.g_entityFollowedByCamera, frameSnapshot.Entities);
        frameSnapshot.NextEntityIndex = dump.g_nextEntityIndex;

        frameSnapshot.MapEvents = dump.g_mapEvents;

        if (dump.g_eventProgramState != null)
        {
            frameSnapshot.EventProgramState = dump.g_eventProgramState;
        }
        else
        {
            frameSnapshot.EventProgramState = new EventProgramState();
        }

        frameSnapshot.MapOffsetX = dump.g_mapOffsetX;
        frameSnapshot.MapOffsetY = dump.g_mapOffsetY;
        frameSnapshot.MapScreenPosX = dump.g_mapScreenPosX;
        frameSnapshot.MapScreenPosY = dump.g_mapScreenPosY;
        frameSnapshot.WarpFlags = dump.g_warpFlags;
        frameSnapshot.PlayerLastX = dump.g_playerLastX;
        frameSnapshot.PlayerLastY = dump.g_playerLastY;
        frameSnapshot.PlayerLastZ = dump.g_playerLastZ;
        frameSnapshot.PlayerStartX = dump.g_playerStartX;
        frameSnapshot.PlayerStartY = dump.g_playerStartY;
        frameSnapshot.PlayerStartZ = dump.g_playerStartZ;
        frameSnapshot.CameraDeltaX = dump.g_cameraDeltaX;
        frameSnapshot.CameraDeltaY = dump.g_cameraDeltaY;
        frameSnapshot.CameraCurrentX = dump.g_cameraCurrentX;
        frameSnapshot.CameraCurrentY = dump.g_cameraCurrentY;
        frameSnapshot.CameraX = dump.g_cameraX;
        frameSnapshot.CameraY = dump.g_cameraY;
        frameSnapshot.CutsceneScrollLimitX = dump.g_cutsceneScrollLimitX;
        frameSnapshot.CutsceneScrollLimitY = dump.g_cutsceneScrollLimitY;
        frameSnapshot.CutsceneScrollSpeedX = dump.g_cutsceneScrollSpeedX;
        frameSnapshot.CutsceneScrollSpeedY = dump.g_cutsceneScrollSpeedY;
        frameSnapshot.CutsceneXReachedMin = dump.g_cutsceneXReachedMin;
        frameSnapshot.CutsceneYReachedMin = dump.g_cutsceneYReachedMin;

        return frameSnapshot;
    }

    private static Entity GetEntityFromIndex(int index, Entity[] entities)
    {
        if (index == -1 || index >= entities.Length)
        {
            return null;
        }

        return entities[index];
    }

    private class FrameDump
    {
        public List<EntityJson> entities { get; set; }

        public uint[] g_mapFlags { get; set; }
        public uint[] g_globalFlags { get; set; }
        public uint g_gameRandomSeed { get; set; }
        public int g_lastWarpEntityIndex { get; set; }
        public int g_TileAnimFrameCounter { get; set; }
        public int DAT_80098f24 { get; set; }
        public int[] INT_ARRAY_800a8284 { get; set; }
        public short g_soundFadeTimer { get; set; }
        public int g_globalTransitionState { get; set; }
        public uint[] g_defaultWarpDestinations { get; set; }
        public uint[] g_soundGroupByMapId { get; set; }
        public int[] g_orderingTableBuffer { get; set; }
        public int g_warpDelayFrames { get; set; }
        public int g_playerControlFlags { get; set; }
        public int g_isWarpDisabled { get; set; }
        public int g_warpType { get; set; }
        public int g_desiredMap { get; set; }
        public int g_warpTriggerType { get; set; }
        public int g_warpExtraParam { get; set; }
        public int g_cameraTargetX { get; set; }
        public int g_cameraTargetY { get; set; }
        public int g_animationId { get; set; }
        public int g_currentMap { get; set; }
        public int g_isCameraScrolling { get; set; }
        public int g_cameraScrollingX { get; set; }
        public int g_cameraScrollingY { get; set; }
        public int g_bossCutsceneFlag { get; set; }
        public int g_cameraOffsetX { get; set; }
        public int g_cameraOffsetY { get; set; }
        public PadState g_padState1 { get; set; }
        public int g_gravityFlag { get; set; }
        public int g_activeCollisionEntity { get; set; }
        public int g_warpLockTimer { get; set; }
        public int[] g_activeEntities { get; set; }
        public int[] g_collideableEntities { get; set; }
        public int g_activeEntityCount { get; set; }
        public int g_collideableEntitiesCount { get; set; }
        public int[] g_visibleEntities { get; set; }
        public int g_cameraLookAtX { get; set; }
        public int g_cameraLookAtY { get; set; }
        public int g_cameraLookAtZ { get; set; }
        public int g_visibleEntityCount { get; set; }
        public int g_numberOfEntity { get; set; }
        public int g_entityFollowedByCamera { get; set; }
        public int g_nextEntityIndex { get; set; }
        public MapEvent[] g_mapEvents { get; set; }
        public EventProgramState g_eventProgramState { get; set; }
        public int g_mapOffsetX { get; set; }
        public int g_mapOffsetY { get; set; }
        public int g_mapScreenPosX { get; set; }
        public int g_mapScreenPosY { get; set; }
        public int g_warpFlags { get; set; }
        public int g_playerLastX { get; set; }
        public int g_playerLastY { get; set; }
        public int g_playerLastZ { get; set; }
        public int g_playerStartX { get; set; }
        public int g_playerStartY { get; set; }
        public int g_playerStartZ { get; set; }
        public int g_cameraDeltaX { get; set; }
        public int g_cameraDeltaY { get; set; }
        public int g_cameraCurrentX { get; set; }
        public int g_cameraCurrentY { get; set; }
        public int g_cameraX { get; set; }
        public int g_cameraY { get; set; }
        public int g_cutsceneScrollLimitX { get; set; }
        public int g_cutsceneScrollLimitY { get; set; }
        public int g_cutsceneScrollSpeedX { get; set; }
        public int g_cutsceneScrollSpeedY { get; set; }
        public int g_cutsceneXReachedMin { get; set; }
        public int g_cutsceneYReachedMin { get; set; }
    }

    private class EntityJson
    {
        public int hitFrameCounter { get; set; }
        public int isNotProcessable { get; set; }
        public int flags2 { get; set; }
        public int platformEntity { get; set; }
        public int actionState { get; set; }
        public int relativeWarpOffsetX { get; set; }
        public int relativeWarpOffsetY { get; set; }
        public int relativeWarpOffsetZ { get; set; }
        public uint contentsItemId { get; set; }
        public int contentsGameFlag { get; set; }
        public uint entityRecord { get; set; }
        public int entityRefId { get; set; }
        public int[] programIndexes { get; set; }
        public uint spriteRecord { get; set; }
        public uint spriteTableIndex { get; set; }
        public int[] spriteProgramIndexes { get; set; }
        public uint targetAnimationId { get; set; }
        public uint targetDirection { get; set; }
        public uint currentAnimationId { get; set; }
        public uint currentDirection { get; set; }
        public int currentFrameIndex { get; set; }
        public uint animSet { get; set; }
        //public uint initialFrame { get; set; }
        //public uint frame { get; set; }
        public int frameIndex { get; set; }
        public int nextFrameDelay { get; set; }
        public int forceResetAnimationFlag { get; set; }
        public int animCompleteCounter { get; set; }
        public int animFlags { get; set; }
        public int zForce { get; set; }
        public int targetXForce { get; set; }
        public int targetYForce { get; set; }
        public int xForce { get; set; }
        public int yForce { get; set; }
        public int previousAdjustedXForce { get; set; }
        public int previousAdjustedYForce { get; set; }
        public int xForceStep { get; set; }
        public int yForceStep { get; set; }
        public int adjustedXForce { get; set; }
        public int adjustedYForce { get; set; }
        public int finalXForce { get; set; }
        public int finalYForce { get; set; }
        public int finalZForce { get; set; }
        public int acceleration { get; set; }
        public int speed { get; set; }
        public int index { get; set; }
        public int screenClipX { get; set; }
        public int screenClipY { get; set; }
        public int screenClipZ { get; set; }
        public int negXMod { get; set; }
        public int negYMod { get; set; }
        public int negZMod { get; set; }
        public int xPos { get; set; }
        public int yPos { get; set; }
        public int zPos { get; set; }
        public int tileX { get; set; }
        public int tileY { get; set; }
        public int tileZ { get; set; }
        public uint ridingEntity { get; set; }
        public uint xCollisionEntity { get; set; }
        public int floorHeight { get; set; }
        public int terrainHeight { get; set; }
        public int forceAdjusted { get; set; }
        public int collidedWithEntityZ { get; set; }
        public int isAboveGround { get; set; }
        public int[] mapTiles { get; set; }
        public int[] mapHeights { get; set; }
        public int platformUpdateFlag { get; set; }
        public int _16c { get; set; }
        public int hitboxOriginX { get; set; }
        public int hitboxOriginY { get; set; }
        public int hitboxOriginZ { get; set; }
        public int _17c { get; set; }
        public int combinedVramFlagsOR { get; set; }
        public int combinedVramFlagsAND { get; set; }
        public int tileAttributes { get; set; }
        public int slope_18c { get; set; }
        public int slope_190 { get; set; }
        //public SpriteRefJson spriteRef { get; set; }
        public int spriteImageIndex { get; set; }
        public int paletteIndex { get; set; }
        public int sheetSize { get; set; }
        public uint activeEffect { get; set; }
        public int zSortValue { get; set; }
        public int zSortDepth { get; set; }
        public uint balanceRecord { get; set; }
        public uint balanceAnimValRef { get; set; }
        public int damagedTickCounter { get; set; }
        public int frameColTickCounter { get; set; }
        public uint frameCollisionData { get; set; }
        public int moddedXPos { get; set; }
        public int moddedYPos { get; set; }
        public int moddedZPos { get; set; }
        public int xMod { get; set; }
        public int yMod { get; set; }
        public int zMod { get; set; }
        public int hitBoxX { get; set; }
        public int hitBoxY { get; set; }
        public int hitBoxZ { get; set; }
        public int transformX { get; set; }
        public int transformY { get; set; }
        public int transformZ { get; set; }
        public int transformWidth { get; set; }
        public int transformDepth { get; set; }
        public int transformHeight { get; set; }
        public int hitCounter { get; set; }
        public uint touchingEntity { get; set; }
        public int eventTrigger { get; set; }
        public int mapEventProgramId { get; set; }
        public uint logicContextEntity { get; set; }
        //public EventProgramStateJson eventProgramState { get; set; }
        public string _268 { get; set; }
        public string _269 { get; set; }
        public string _26A { get; set; }
        public string _26B { get; set; }
        public uint lastTargetAnimationId { get; set; }
        public uint lastTargetDirection { get; set; }
        public int initialXPos { get; set; }
        public int initialYPos { get; set; }
        public short[] aiValues { get; set; }
        public int status { get; set; }
        public uint flags { get; set; }
        public int depth { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string[] bytes { get; set; }
        public int isZForceApplied { get; set; }
        public int index2 { get; set; }
        public uint childEntity { get; set; }
        public uint parentEntity { get; set; }
        public int hp { get; set; }
        public int hpMax { get; set; }

        public void CopyToEntity(Entity entity)
        {
            entity.FrameCounter = hitFrameCounter;
            entity.IsNotProcessable = isNotProcessable;
            entity.Flags2 = flags2;
            entity.PlatformEntity = StaticVariables.g_entitySlots[platformEntity];
            entity.ActionState = actionState;
            entity.RelativeWarpOffsetX = relativeWarpOffsetX;
            entity.RelativeWarpOffsetY = relativeWarpOffsetY;
            entity.RelativeWarpOffsetZ = relativeWarpOffsetZ;
            entity.ContentsItemId = contentsItemId;
            entity.ContentsGameFlag = contentsGameFlag;
            //entity.EntityRecord = entityRecord;
            entity.EntityRefId = entityRefId;
            if (programIndexes != null && entity.ProgramIndexes != null)
                Array.Copy(programIndexes, entity.ProgramIndexes, Math.Min(programIndexes.Length, entity.ProgramIndexes.Length));
            //entity.SpriteRecord = spriteRecord;
            entity.SpriteTableIndex = spriteTableIndex;
            if (spriteProgramIndexes != null && entity.SpriteProgramIndexes != null)
                Array.Copy(spriteProgramIndexes, entity.SpriteProgramIndexes, Math.Min(spriteProgramIndexes.Length, entity.SpriteProgramIndexes.Length));
            entity.TargetAnimationId = targetAnimationId;
            entity.TargetDirection = targetDirection;
            entity.CurrentAnimationId = currentAnimationId;
            entity.CurrentDirection = currentDirection;
            entity.CurrentFrameIndex = currentFrameIndex;
            //entity.AnimSet = animSet;
            if (entity.AnimSet != null 
                && entity.AnimSet.PreloadedAnims[entity.TargetDirection >> 3].NumberOfFrames > 0
                && frameIndex != -1)
            {
                var animRecordPtr = entity.Sprite.AnimSets[currentAnimationId];
                var currentFrame = animRecordPtr.PreloadedAnims[currentDirection >> 3].Frames[frameIndex];
                entity.AnimSet = animRecordPtr;
                entity.Frame = currentFrame;
                entity.FirstFrame = currentFrame;
            }
            //entity.FirstFrame = initialFrame;
            //entity.Frame = frame;
            entity.NextFrameDelay = nextFrameDelay;
            entity.ForceResetAnimationFlag = forceResetAnimationFlag;
            entity.AnimCompleteCounter = animCompleteCounter;
            entity.AnimFlags = animFlags;
            entity.ForceZ = zForce;
            entity.TargetXForce = targetXForce;
            entity.TargetYForce = targetYForce;
            entity.ForceX = xForce;
            entity.ForceY = yForce;
            entity.PreviousAdjustedXForce = previousAdjustedXForce;
            entity.PreviousAdjustedYForce = previousAdjustedYForce;
            entity.ForceStepX = xForceStep;
            entity.ForceStepY = yForceStep;
            entity.AdjustedXForce = adjustedXForce;
            entity.AdjustedYForce = adjustedYForce;
            entity.FinalXForce = finalXForce;
            entity.FinalYForce = finalYForce;
            entity.FinalZForce = finalZForce;
            entity.Acceleration = acceleration;
            entity.Speed = speed;
            entity.Index = index;
            entity.ScreenClipX = screenClipX;
            entity.ScreenClipY = screenClipY;
            entity.ScreenClipZ = screenClipZ;
            entity.NegXMod = negXMod;
            entity.NegYMod = negYMod;
            entity.NegZMod = negZMod;
            entity.PosX = xPos;
            entity.PosY = yPos;
            entity.PosZ = zPos;
            entity.TileX = tileX;
            entity.TileY = tileY;
            entity.TileZ = tileZ;
            //entity.RidingEntity = ridingEntity;
            //entity.XCollisionEntity = xCollisionEntity;
            entity.FloorHeight = floorHeight;
            entity.TerrainHeight = terrainHeight;
            entity.ForceAdjusted = forceAdjusted;
            entity.CollidedWithEntityZ = collidedWithEntityZ;
            entity.IsAboveGround = isAboveGround;
            //if (mapTiles != null && entity.MapTiles != null)
            //    Array.Copy(mapTiles, entity.MapTiles, Math.Min(mapTiles.Length, entity.MapTiles.Length));
            if (mapHeights != null && entity.MapHeights != null)
                Array.Copy(mapHeights, entity.MapHeights, Math.Min(mapHeights.Length, entity.MapHeights.Length));
            entity.PlatformUpdateFlag = platformUpdateFlag;
            entity._16c = _16c;
            entity.HitBoxOriginX = hitboxOriginX;
            entity.HitBoxOriginY = hitboxOriginY;
            entity.HitBoxOriginZ = hitboxOriginZ;
            entity._17c = _17c;
            entity.CombinedVramFlagsOR = combinedVramFlagsOR;
            entity.CombinedVramFlagsAND = combinedVramFlagsAND;
            entity.TileAttributes = tileAttributes;
            entity.Slope_18c = slope_18c;
            entity.Slope_190 = slope_190;
            //if (spriteRef != null && entity.SpriteRef != null)
            //    spriteRef.CopyToSpriteRef(entity.SpriteRef);
            //entity.SpriteImageIndex = spriteImageIndex;
            //entity.PaletteIndex = paletteIndex;
            //entity.SheetSize = sheetSize;
            //entity.ActiveEffect = activeEffect;
            entity.DepthSortVal = zSortValue;
            entity.SortTop = zSortDepth;
            //entity.BalanceRecord = balanceRecord;
            //entity.BalanceAnimValRef = balanceAnimValRef;
            entity.DamagedTickCounter = damagedTickCounter;
            entity.FrameColTickCounter = frameColTickCounter;
            //entity.FrameCollisionData = frameCollisionData;
            entity.ModdedXPos = moddedXPos;
            entity.ModdedYPos = moddedYPos;
            entity.ModdedZPos = moddedZPos;
            entity.ModX = xMod;
            entity.ModY = yMod;
            entity.ModZ = zMod;
            entity.HitBoxX = hitBoxX;
            entity.HitBoxY = hitBoxY;
            entity.HitBoxZ = hitBoxZ;
            entity.FrameXOff = transformX;
            entity.FrameYOff = transformY;
            entity.FrameZOff = transformZ;
            entity.FrameWidth = transformWidth;
            entity.FrameDepth = transformDepth;
            entity.FrameHeight = transformHeight;
            entity.HitCounter = hitCounter;
            //entity.TouchingEntity = touchingEntity;
            entity.EventTrigger = eventTrigger;
            entity.MapEventProgramId = mapEventProgramId;
            //entity.LogicContextEntity = logicContextEntity;
            //if (eventProgramState != null && entity.EventProgramState != null)
            //    eventProgramState.CopyToEventProgramState(entity.EventProgramState);
            // Les champs _268, _269, _26A, _26B, bytes sont ignorés (données brutes)
            entity.LastTargetAnimationId = lastTargetAnimationId;
            entity.LastTargetDirection = lastTargetDirection;
            entity.InitialXPos = initialXPos;
            entity.InitialYPos = initialYPos;
            if (aiValues != null && entity.AIValues != null)
                Array.Copy(aiValues, entity.AIValues, aiValues.Length);
            entity.Status = status;
            entity.Flags = flags;
            entity.Depth = depth;
            entity.Width = width;
            entity.Height = height;
            entity.IsZForceApplied = isZForceApplied;
            entity.Index2 = index2;
            //entity.ChildEntity = childEntity;
            //entity.ParentEntity = parentEntity;
            entity.Hp = hp;
            entity.HpMax = hpMax;
        }
    }

    //private class SpriteRefJson
    //{
    //    public int images { get; set; }
    //    public int depthSortVal { get; set; }
    //    public int numImages { get; set; }
    //    public int x { get; set; }
    //    public int y { get; set; }
    //    public int z { get; set; }
    //
    //    public void CopyToSpriteRef(SpriteRef spriteRef)
    //    {
    //        spriteRef.Images = images;
    //        spriteRef.DepthSortVal = depthSortVal;
    //        spriteRef.NumImages = numImages;
    //        spriteRef.X = x;
    //        spriteRef.Y = y;
    //        spriteRef.Z = z;
    //    }
    //}
    //
    //private class EventProgramStateJson
    //{
    //    public int var1 { get; set; }
    //    public int var2 { get; set; }
    //    public int var3 { get; set; }
    //    public int var4 { get; set; }
    //    public int var5 { get; set; }
    //    public int var6 { get; set; }
    //    public int var7 { get; set; }
    //    public int var8 { get; set; }
    //    public int var9 { get; set; }
    //    public int _30 { get; set; }
    //    public int sp { get; set; }
    //    public int result { get; set; }
    //    public int parameters { get; set; }
    //
    //    public void CopyToEventProgramState(EventProgramState state)
    //    {
    //        state.var1 = var1;
    //        state.var2 = var2;
    //        state.var3 = var3;
    //        state.var4 = var4;
    //        state.var5 = var5;
    //        state.var6 = var6;
    //        state.var7 = var7;
    //        state.var8 = var8;
    //        state.var9 = var9;
    //        state._30 = _30;
    //        state.sp = sp;
    //        state.result = result;
    //        state.parameters = parameters;
    //    }
    //}
}