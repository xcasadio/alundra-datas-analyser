using Alundra.DatasBin;
using Alundra.Gameplay;
using Alundra.Gameplay.Scripts;
using Alundra.Sound;
using Alundra.Sprite;
using Alundra.Text;
using System.Diagnostics;

namespace Alundra;

public class Game
{
    private readonly DatasBin.DatasBin _datasBin;
    private readonly BalanceBin _balanceBin;
    private readonly EtcResR _etcResR;
    private readonly Font3 _font3;

    private GameMap _map;
    private Dictionary<int, Bitmap> _cachedTiles;
    private Dictionary<int, List<Bitmap>> _cachedSprites;
    private readonly GameState _gameState;
    //private readonly EntityEventHandlers _entityEventHandlers;

    public Game(DatasBin.DatasBin datasBin, BalanceBin balanceBin, SoundBin soundBin, EtcResR etcResR, Font3 font3)
    {
        _datasBin = datasBin;
        _balanceBin = balanceBin;
        _etcResR = etcResR;
        _font3 = font3;

        using var reader = datasBin.OpenBin();

        if (!datasBin.AlundraGameMap.Loaded)
        {
            datasBin.AlundraGameMap.Load(reader, false);
        }

        _gameState = new GameState(datasBin.AlundraGameMap, _balanceBin, soundBin);
            //{ StaticVariables.g_cameraCurrentX = 100 << 16, g_cameraCurrentY = 100 << 16 };

        //_entityEventHandlers = new EntityEventHandlers(_gameState);
    }

    public void InitializeEngine()
    {
        _gameState.Initialize();
        StaticVariables.Initialize();
        LoadMap(389);
    }

    public void Render(Graphics g)
    {
        Renderer.Render(g, _datasBin, _map);
    }

    public void LoadMap(int mapId)
    {
        LoadMap(_datasBin.GameMaps[mapId]);
    }

    public void LoadMap(GameMap gmap)
    {
        _map = gmap;
        if (!_map.Loaded)
        {
            using var reader = _datasBin.OpenBin();
            _map.Load(reader, true);
        }

        _gameState.LoadMap(_map);

        //spriteinfo
        for (var i = 0; i < _map.SpriteInfo.Entities.Entities.Length; i++)
        {
            var entity = _map.SpriteInfo.Entities.Entities[i];
            if (entity != null)
            {
                //entity.
            }
        }


        for (var i = 0; i < _map.SpriteInfo.MapEvents.Records.Length; i++)
        {
            var record = _map.SpriteInfo.MapEvents.Records[i];
            if (record != null)
            {

            }
        }


        for (var i = 0; i < _map.SpriteInfo.Sprites.Length; i++)
        {
            var sprite = _map.SpriteInfo.Sprites[i];
            if (sprite != null)
            {

            }
        }
    }

    public void MainLoop(Graphics g)
    {
        try
        {
            do
            {
                //_2abe8();

                Render(g);

                MainUpdate(false);

                //_86220(game._1e60e8, r19);

                //if (game._1ac468[0] < 0
                //    && (game._1ac468[4] & 0x4000) != 0)
                //    framestoadvance = game._1ac468[14];

                //PresentFrame(framestoadvance);

                //EmptyFunc();

            } while (!_gameState.BreakoutGameLoop);
        }
        catch (Exception e)
        {
            System.Diagnostics.Debugger.Break();
            throw;
        }
        
    }

    public void MainUpdate(bool force)
    {
        //game._1d7a70 -= 8;
        //if (game._1d7a70 < 0)
        //    game._1d7a70 = 0;

        //game._1d7a78 = 0x140 - (game._1d7a70 * 2);

        //_1d7a74 -= 6;
        //if (game._1d7a74 < 0)
        //    game._1d7a74 = 0;

        //game._1d7a7c = 0xf0 - (game._1d7a74 * 2);

        GetPlayerInput();

        Update();

        //if (game._1ef998 != 0)
        //{
        //    game._1ef998--;
        //}

        /*if (game.g_playerControlFlags == 0
            && game.PlayerEntity._20 == 0
            && game._1f0fbc == 0
            && (game.1dd7ea & 0x803) != 0
            && game._1ef998 == 0
            && (game.PlayerInput & 0x100) == 0
            && _570f8() == 0)
            game.BreakoutGameLoop = true;*/

        //PlayMusic();
        //advance random seed num
        var rnd = (int)(StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7);
        StaticVariables.g_gameRandomSeed = (uint)rnd;

        if (force)
        {
            _gameState.BreakoutGameLoop = false;
        }
    }

    private void GetPlayerInput()
    {

    }

    public void Update()
    {

        /*
         a debug check
        if (0x38800 < game._1dd810) {

        }
         */

        //game._1ef1d0 = 0x1380f0;

        //number of spriterefs that will be rendered this frame, the following functions build up the list
        _gameState.NumSprites = 0;

        UpdateMapEvents();

        UpdateEntities();

        UpdateAllActiveEffectsPostLogic();

    }

    public void UpdateEntities()
    {
        if ((StaticVariables.g_playerControlFlags & 0x48) == 0)
        {
            UpdateDestroyedEntities();

            UpdateEntitiesEvents();

            UpdateEntitiesCounters();



            UpdateEntityLists();

            UpdateEntitiesAnimation();

            UpdateEntitiesPhysics();

            UpdateActiveEffects();

            UpdateBalanceRecords();
        }
        else
        {
            UpdateEntityLists();
        }

        if (StaticVariables.g_entityFollowedByCamera != null)
        {
            if (StaticVariables.g_entityFollowedByCamera.Status <= 3)
            {
                //gets halfwords
                StaticVariables.g_playerX = StaticVariables.g_entityFollowedByCamera.XPos >> 16;
                StaticVariables.g_playerY = StaticVariables.g_entityFollowedByCamera.YPos >> 16;
                StaticVariables.g_playerZ = StaticVariables.g_entityFollowedByCamera.ZPos >> 16;
            }
        }
        UpdateVisibleEntitiesZSort();

        //add spriterefs
        if (StaticVariables.g_visibleEntityCount > 0)
        {
            for (var dex = 0; dex < StaticVariables.g_visibleEntityCount; dex++)
            {
                var entity = StaticVariables.g_visibleEntities[dex];

                entity.SpriteRef.DepthSortVal = entity.DepthSortVal;
                entity.SpriteRef.X = entity.XPos;
                entity.SpriteRef.Y = entity.YPos;
                entity.SpriteRef.Z = entity.ZPos;
                _gameState.SpriteRefs[_gameState.NumSprites++] = entity.SpriteRef;
            }
        }
    }

    private void UpdateEntitiesPhysics()
    {
        for (var i = 0; i < StaticVariables.g_activeEntityCount; i++)
        {
            var entity = StaticVariables.g_activeEntities[i];
            entity.PlatformUpdateFlag = 0;
            entity.CollidedWithEntityZ = 0;
            entity.ForceAdjusted = 0;

            entity.ModdedXPos = entity.XPos + entity.XMod;
            entity.ModdedYPos = entity.YPos + entity.YMod;
            entity.ModdedZPos = entity.ZPos + entity.ZMod;
        }

        CheckRidingEntities();
        UpdateEntitiesForces();

        for (var i = 0; i < StaticVariables.g_collideableEntitiesCount; i++)
        {
            var entity = StaticVariables.g_collideableEntities[i];
            if (entity.RidingEntity != null)
            {
                UpdateRidingEntity(entity, entity.RidingEntity);
            }
        }

        for (var i = 0; i < StaticVariables.g_activeEntityCount; i++)
        {
            var entity = StaticVariables.g_activeEntities[i];
            if (entity.PlatformUpdateFlag != 0)
            {
                //MoveEntity(entity);
            }
        }

        for (var i = 0; i < StaticVariables.g_activeEntityCount; i++)
        {
            var entity = StaticVariables.g_activeEntities[i];
            _gameState.UpdateTile(entity);
        }
    }

    private void UpdateRidingEntity(Entity entity, Entity ridingEntity)
    {
        if (ridingEntity.RidingEntity != null)
        {
            //TODO: bug maybe in CheckRidingEntities why overflow ???
            //UpdateRidingEntity(ridingEntity, ridingEntity.RidingEntity);
        }

        entity.FinalXForce += ridingEntity.AdjustedXForce;
        entity.FinalYForce += ridingEntity.AdjustedYForce;
        if (entity.IsZForceApplied == 0)
        {
            entity.ZForce = ridingEntity.FinalZForce;
            entity.FinalYForce = ridingEntity.FinalZForce;
        }
    }

    private void CheckRidingEntities()
    {
        for (var i = 0; i < StaticVariables.g_collideableEntitiesCount; i++)
        {
            var entity = StaticVariables.g_collideableEntities[i];
            if ((entity.Flags & 0x4100) != 0x0100)
            {
                continue;
            }

            for (var j = 0; j < StaticVariables.g_collideableEntitiesCount; j++)
            {
                var entity2 = StaticVariables.g_collideableEntities[j];
                if (entity == entity2)
                {
                    continue;
                }

                if ((entity2.ModdedXPos - entity.ModdedXPos >= 0 && entity2.ModdedXPos - entity.ModdedXPos < entity.Width + 1) || (entity2.ModdedXPos - entity.ModdedXPos < 0 && entity.ModdedXPos - entity2.ModdedXPos < entity2.Width + 1))
                {
                    if (entity2.ModdedYPos - entity.ModdedYPos >= 0 && entity2.ModdedYPos - entity.ModdedYPos < entity.Depth + 1)
                    {
                        entity.RidingEntity = entity2;
                        break;
                    }

                    if (entity2.ModdedYPos - entity.ModdedYPos < 0 && entity.ModdedYPos - entity2.ModdedYPos < entity2.Depth + 1)
                    {
                        entity.RidingEntity = entity2;
                        break;
                    }
                }
            }
        }
    }

    private void UpdateEntitiesForces()
    {
        var player = StaticVariables.PlayerEntity;

        for (var i = 0; i < StaticVariables.g_activeEntityCount; i++)
        {
            var entity = StaticVariables.g_activeEntities[i];
            if (entity == player)
            {
                if (player.IsZForceApplied != 0)
                {
                    if ((player.Flags & 0x100) != 0
                        && (player.CombinedVramFlagsOR & 0x0010) != 0
                        && StaticVariables.g_gravityFlag <= 0)
                    {
                        player.ZForce = player.IsZForceApplied * 160;
                    }
                    else
                    {
                        player.ZForce = player.IsZForceApplied << 8;
                    }
                }
                else
                {
                    if ((player.Flags & 0x0100) != 0)
                    {
                        var force = player.ZForce - (_gameState.GameMap.Info.Gravity << 8);
                        if (force < 0)
                        {
                            force = -force;//abs
                        }

                        var terminal = _gameState.GameMap.Info.TerminalVelocity << 8;
                        if (terminal < force)
                        {
                            force = terminal;
                            if (force < 0)
                            {
                                force = -force;//abs
                            }
                        }
                        player.ZForce = force;
                    }
                }

                SetXyForces(player);

                int xforcestep, yforcestep;
                if ((player.CombinedVramFlagsOR & 0x0020) != 0)
                {
                    long resultx = player.XForceStep * 0x1000;
                    xforcestep = (int)(resultx >> 16);

                    long resulty = player.YForceStep * 0x1000;
                    yforcestep = (int)(resulty >> 16);
                }
                else
                {
                    xforcestep = player.XForceStep;
                    yforcestep = player.YForceStep;
                }

                int targetxforce, targetyforce;
                if ((player.CombinedVramFlagsOR & 0x0008) != 0
                    && StaticVariables.g_gravityFlag <= 0)
                {
                    long resultx = player.TargetXForce * 0x8000;
                    targetxforce = (int)(resultx >> 16);
                    long resulty = player.TargetYForce * 0x8000;
                    targetyforce = (int)(resulty >> 16);
                }
                else
                {
                    targetxforce = player.TargetXForce;
                    targetyforce = player.TargetYForce;
                }

                player.XForce = IncrementForce(player.XForce, targetxforce, xforcestep);
                player.YForce = IncrementForce(player.YForce, targetyforce, yforcestep);
            }
            else
            {
                if (entity.PlatformEntity != null)
                {
                    entity.ZForce = 0;
                    entity.YForce = 0;
                    entity.XForce = 0;
                    entity.AdjustedYForce = 0;
                    entity.AdjustedXForce = 0;
                    entity.FinalZForce = 0;
                    entity.FinalYForce = 0;
                    entity.FinalXForce = 0;
                }

                if (entity.IsZForceApplied != 0)
                {
                    if ((entity.IsZForceApplied & 0xffff) == 0x8000
                        && (entity.Flags & 0x0100) == 0)
                    {
                        entity.ZForce = entity.IsZForceApplied << 8;
                    }
                }

                if ((entity.Flags & 0x0100) != 0)
                {
                    //this applies gravity (limited by terminal velicity) to the z force
                    var force = entity.ZForce - (_gameState.GameMap.Info.Gravity << 8);
                    if (force < 0)
                    {
                        force = -force;//abs
                    }

                    var terminal = _gameState.GameMap.Info.TerminalVelocity << 8;
                    if (terminal < force)
                    {
                        force = terminal;
                        if (force < 0)
                        {
                            force = -force;//abs
                        }
                    }
                    entity.ZForce = force;
                }

                SetXyForces(entity);

                entity.XForce = IncrementForce(entity.XForce, entity.TargetXForce, entity.XForceStep);
                entity.YForce = IncrementForce(entity.YForce, entity.TargetYForce, entity.YForceStep);
            }

            SetAdjustedXyForces(entity);

            entity.FinalXForce = entity.AdjustedXForce;
            entity.FinalYForce = entity.AdjustedYForce;
            entity.FinalZForce = entity.ZForce;
        }
    }

    private void SetAdjustedXyForces(Entity entity)
    {
        var lastinteractx = entity.InteractXForce;
        var lastinteracty = entity.InteractYForce;
        entity.InteractYForce = 0;
        entity.InteractXForce = 0;
        var xval = entity.XForce + ScriptHelper.XForceTable[entity.TileAttributes & 0xf] >> _gameState.GameMap.Info.Gravity;
        var yval = entity.YForce + ScriptHelper.YForceTable[entity.TileAttributes & 0xf] >> _gameState.GameMap.Info.Gravity;

        xval += lastinteractx;
        yval += lastinteracty;

        if (xval + entity.XPos < entity.NegXMod)
        {
            xval = entity.NegXMod - entity.XPos;
            entity.ForceAdjusted = 1;
        }
        else if (xval + entity.XPos < entity.ScreenClipX)
        {
            xval = entity.ScreenClipX - entity.XPos;
            entity.ForceAdjusted = 1;
        }

        if (yval + entity.YPos < entity.NegYMod)
        {
            yval = entity.NegYMod - entity.YPos;
            entity.ForceAdjusted = 1;
        }
        else if (yval + entity.YPos < entity.ScreenClipY)
        {
            yval = entity.ScreenClipY - entity.YPos;
            entity.ForceAdjusted = 1;
        }


        entity.AdjustedXForce = xval;
        entity.AdjustedYForce = yval;
    }

    private int IncrementForce(int force, int targetforce, int step)
    {
        if (force == targetforce)
        {
            return force;
        }

        if (force < targetforce)
        {
            force += step;
        }
        else
        {
            force -= step;
        }

        if (force < targetforce)
        {
            return force;
        }

        return targetforce;
    }

    private void SetXyForces(Entity entity)
    {
        if (entity.Speed != entity.AnimSet.Speed
            || entity.TargetDirection != entity.CurrentDirection)
        {
            entity.Speed = entity.AnimSet.Speed;

            entity.TargetXForce = StaticVariables.g_offsetXList[entity.TargetDirection] * entity.AnimSet.Speed;

            entity.CurrentDirection = entity.TargetDirection;

            entity.TargetYForce = StaticVariables.g_offsetYList[entity.TargetDirection] * entity.AnimSet.Speed;
        }
        else if (entity.Acceleration == (entity.AnimSet.Acceleration & 0xf))
        {
            return;
        }

        entity.Acceleration = entity.AnimSet.Acceleration & 0xf;

        entity.XForceStep = Math.Abs(entity.TargetXForce - entity.XForce) >> entity.Acceleration;

        entity.YForceStep = Math.Abs(entity.TargetYForce - entity.YForce) >> entity.Acceleration;
    }

    private void UpdateVisibleEntitiesZSort()
    {
        if (StaticVariables.g_visibleEntityCount <= 0)
        {
            return;
        }

        for (var dex = 0; dex < StaticVariables.g_visibleEntityCount; dex++)
        {
            var entity = StaticVariables.g_visibleEntities[dex];
            entity.DepthSortVal = 0;
            entity.SortTop = entity.ModdedZPos + entity.Height;
        }

        for (var dex = 0; dex < StaticVariables.g_visibleEntityCount; dex++)
        {
            var entity = StaticVariables.g_visibleEntities[dex];
            if (entity.DepthSortVal == 0)
            {
                SetDepthSortVal(entity);
            }
        }

        for (var dex = 0; dex < StaticVariables.g_visibleEntityCount; dex++)
        {
            var entity = StaticVariables.g_visibleEntities[dex];
            entity.DepthSortVal = (int)(entity.DepthSortVal & 0xffff0000) + (entity.ZPos & 0xffff);
        }
    }

    private void SetDepthSortVal(Entity entity)
    {
        if (entity.DepthSortVal != 0)
        {
            return;
        }

        var sortval = entity.YPos + (entity.SpriteRef.NumImages << 16);
        if ((entity.Flags & 0x80) != 0
            || (entity.AnimFlags & 0x80) != 0)
        {
            entity.DepthSortVal = sortval;
            return;
        }

        if (entity.PlatformEntity != null)
        {
            if (entity.PlatformEntity.DepthSortVal == 0)
            {
                SetDepthSortVal(entity.PlatformEntity);
            }
            if (sortval < entity.PlatformEntity.DepthSortVal)
            {
                entity.DepthSortVal = entity.PlatformEntity.DepthSortVal;
                return;
            }
        }

        for (var dex = 0; dex < StaticVariables.g_collideableEntitiesCount; dex++)
        {
            var checkme = StaticVariables.g_collideableEntities[dex];
            if (checkme == entity)
            {
                continue;
            }

            if (checkme.SortTop >= entity.SortTop)
            {
                continue;
            }

            //X
            var x = checkme.XPos + checkme.XMod - entity.ModdedXPos;
            if (x >= 0)
            {
                if (x >= entity.Width + 1)
                {
                    continue;
                }
            }
            else
            {
                if (entity.ModdedXPos - (checkme.XPos + checkme.XMod) >= checkme.Width + 1)
                {
                    continue;
                }
            }

            //Y
            var y = checkme.YPos + checkme.YMod - entity.ModdedYPos;
            if (y >= 0)
            {
                if (y >= entity.Depth + 1)
                {
                    continue;
                }
            }
            else
            {
                if (entity.ModdedYPos - (checkme.YPos + checkme.YMod) >= checkme.Depth + 1)
                {
                    continue;
                }
            }

            if (checkme.DepthSortVal == 0)
            {
                SetDepthSortVal(checkme);
            }

            if (sortval < checkme.DepthSortVal)
            {
                sortval = checkme.DepthSortVal;
            }
        }

        entity.DepthSortVal = sortval;
    }

    private void UpdateBalanceRecords()
    {
        if (StaticVariables.g_activeEntityCount <= 0)
        {
            return;
        }

        for (var i = 0; i < StaticVariables.g_activeEntityCount; i++)
        {
            var entity = StaticVariables.g_activeEntities[i];

            if (entity.FrameCollision == null)
            {
                continue;
            }

            if (entity.BalanceVal == null)
            {
                continue;
            }

            if (entity.BalanceVal.Val == 0)
            {
                continue;
            }

            var flags = ((entity.Flags >> 2) & 1) | ((entity.Flags << 2) & 8);
            if ((entity.Flags & 0x1000) != 0)
            {
                flags |= 0x800;
            }

            if (flags == 0)
            {
                continue;
            }

            for (var j = 0; j < StaticVariables.g_activeEntityCount; j++)
            {
                var checkme = StaticVariables.g_activeEntities[j];
                if (checkme == entity)
                {
                    continue;
                }

                if (checkme.FrameColTickCounter != 0)
                {
                    continue;
                }

                if (checkme.DamagedTickCounter != 0)
                {
                    continue;
                }

                if ((checkme.AnimFlags & 0x40) != 0)
                {
                    continue;
                }

                if ((checkme.Flags & flags) == 0)
                {
                    continue;
                }

                //X
                var difx = entity.FrameX - checkme.ModdedXPos;
                int width;
                if (difx > 0)
                {
                    width = checkme.Width + 1;
                }
                else
                {
                    difx = checkme.ModdedXPos - entity.FrameX;
                    width = entity.FrameWidth + 1;
                }
                if (difx >= width)
                {
                    continue;
                }

                //Y
                var dify = entity.FrameY - checkme.ModdedYPos;
                int depth;
                if (dify > 0)
                {
                    depth = checkme.Depth + 1;
                }
                else
                {
                    dify = checkme.ModdedYPos - entity.FrameY;
                    depth = entity.FrameDepth + 1;
                }
                if (dify >= depth)
                {
                    continue;
                }

                //Z
                var difz = entity.FrameZ - checkme.ModdedZPos;
                int height;
                if (difz > 0)
                {
                    height = checkme.Height + 1;
                }
                else
                {
                    difz = checkme.ModdedZPos - entity.FrameZ;
                    height = entity.FrameHeight + 1;
                }
                if (difz >= height)
                {
                    continue;
                }

                /*if (game._1ac468 < 0
                    && (game._1ac46c & 0x800) != 0)
                {
                DEBUG THING
                }*/
                var valdex = entity.BalanceVal.Val & 0xf;
                var val = checkme.BalanceRecord.Vals[valdex];

                if ((val & 0xc0) != 0x80)
                {
                    if (valdex == 6 || valdex == 0xa)
                    {
                        _gameState.CreateEffect_Type1(0, 4, 0, checkme, width, 0, 0, 0);
                    }
                    if (valdex == 7 || valdex == 9)
                    {
                        _gameState.CreateEffect_Type1(0, 5, 0, checkme, 1, 0, 0, 0);
                    }
                    checkme.TouchingEntity = entity;
                }

                checkme.FrameColTickCounter = 0x19;

                entity.HitCounter++;
                //X
                var xr = checkme.ModdedXPos + checkme.Width;
                if (entity.FrameX + entity.FrameWidth < xr)
                {
                    xr = entity.FrameX + entity.FrameWidth;
                }

                var xl = entity.FrameX;
                if (entity.FrameX < checkme.ModdedXPos)
                {
                    xl = checkme.ModdedXPos;
                }

                //Y
                var yr = checkme.ModdedYPos + checkme.Depth;
                if (entity.FrameY + entity.FrameDepth < yr)
                {
                    yr = entity.FrameY + entity.FrameDepth;
                }

                var yl = entity.FrameY;
                if (entity.FrameY < checkme.ModdedYPos)
                {
                    yl = checkme.ModdedYPos;
                }

                //Z
                var zr = checkme.ModdedZPos + checkme.Height;
                if (entity.FrameZ + entity.FrameHeight < zr)
                {
                    zr = entity.FrameZ + entity.FrameHeight;
                }

                var zl = entity.FrameZ;
                if (entity.FrameZ < checkme.ModdedZPos)
                {
                    zl = checkme.ModdedZPos;
                }

                var x = (xl + xr) / 2;
                var y = (yl + yr) / 2;
                var z = (zl + zr) / 2;
                CreateRandomPoofs(x, y, z);
            }
        }
    }

    private void CreateRandomPoofs(int x, int y, int z)
    {
        //creates two poofs moving away from the impact at random speed and direction
        for (var dex = 0; dex < 2; dex++)
        {
            var effect = _gameState.CreateEffect_Type0(0, 9, 0, x, y, z);
            if (effect == null)
            {
                continue;
            }

            var baseforce = (int)(0xffff << 16);

            var i = StaticVariables.g_gameRandomSeed;
            var val1 = (int)(i * 0x7d2b89dd);
            var val2 = (int)(0xe06a02e7 + val1);
            var targetval = (int)(((long)val2 * 0x20001) >> 32);
            StaticVariables.g_gameRandomSeed = (uint)val2;

            effect.XForce = targetval + baseforce;

            i = StaticVariables.g_gameRandomSeed;
            val1 = (int)(i * 0x7d2b89dd);
            val2 = (int)(0xe06a02e7 + val1);
            targetval = (int)(((long)val2 * 0x20001) >> 32);
            StaticVariables.g_gameRandomSeed = (uint)val2;

            effect.YForce = targetval + baseforce;

            i = StaticVariables.g_gameRandomSeed;
            val1 = (int)(i * 0x7d2b89dd);
            val2 = (int)(0xe06a02e7 + val1);
            targetval = (int)(((long)val2 * 0x20001) >> 32);
            StaticVariables.g_gameRandomSeed = (uint)val2;

            effect.ZForce = targetval + baseforce;
        }
    }

    private void UpdateActiveEffects()
    {
        if (StaticVariables.g_numberOfEntity < 0)
        {
            return;
        }

        for (var i = 0; i < StaticVariables.g_numberOfEntity; i++)
        {
            var entity = StaticVariables.g_entitySlots[i];
            if (entity.Status - 2 >= 2 || (entity.DamagedTickCounter & 3) == 3)
            {
                if (entity.ActiveEffect != null)
                {
                    entity.ActiveEffect.Status = 0;
                    entity.ActiveEffect = null;
                }
                continue;
            }
            var effect = entity.ActiveEffect;
            if (effect == null)
            {
                effect = _gameState.CreateEffect_Type3(0, 0, 0, entity, -1, 0, 0, 0);
                if (effect == null)
                {
                    continue;
                }

                entity.ActiveEffect = effect;
            }

            if (((entity.Flags >> 16) & 7) == 0)
            {
                continue;
            }

            if ((entity.AnimFlags & 0x10) != 0)
            {
                continue;
            }

            if (entity.PlatformEntity != null)
            {
                effect.Status = 1;
                continue;
            }
            //TODO: what is 18c, something with slope and sliding?
            var animid = -1;
            if ((entity.Slope_18c == 4 || entity.Slope_190 == 4)
                && entity.Slope_18c != entity.Slope_190)
            {
                _gameState.CreateEffect_Type0(0, 6, 0, entity.XPos, entity.YPos, entity.CollidedWithEntityZ);
            }

            if (entity.Slope_18c >= 8)
            {
                continue;
            }

            switch (entity.Slope_18c)
            {
                case 1:
                case 2:
                    effect.TargetIsMapSprite = 0;
                    effect.TargetSpriteTableIndex = 1;
                    effect.TargetAnimation = (byte)animid;
                    effect.Status = 2;
                    effect.X = entity.XPos;
                    effect.Y = entity.YPos;
                    effect.Z = entity.CollidedWithEntityZ;
                    continue;
                case 4:
                    effect.Status = 1;
                    if ((entity.UnknownCounter & 7) != 0)
                    {
                        continue;
                    }

                    if ((entity.XForce | entity.YForce) == 0)
                    {
                        continue;
                    }

                    _gameState.CreateEffect_Type0(0, 0x15, 0, entity.XPos, entity.YPos, entity.CollidedWithEntityZ);
                    continue;
                case 3:
                    if ((entity.UnknownCounter & 0x7) != 0)
                    {
                        break;
                    }

                    if ((entity.XForce | entity.YForce) == 0)
                    {
                        break;
                    }

                    _gameState.CreateEffect_Type0(0, _gameState.GameMap.Info.SlideEffectId, 0, entity.XPos, entity.YPos, entity.CollidedWithEntityZ);
                    break;
                default:
                    break;
            }

            animid -= (entity.ZPos - entity.CollidedWithEntityZ) >> 20;

            if (animid >= 6)
            {
                animid = 5;
            }
            else if (animid < 0)
            {
                animid = 0;
            }

            effect.TargetIsMapSprite = 0;
            effect.TargetSpriteTableIndex = 0;

            effect.TargetAnimation = (byte)animid;
            effect.Status = 2;

            effect.X = entity.XPos;
            effect.Y = entity.YPos;
            effect.Z = entity.CollidedWithEntityZ;
        }
    }

    private void UpdateEntitiesAnimation()
    {
        for (var i = 0; i < StaticVariables.g_activeEntityCount; i++)
        {
            var entity = StaticVariables.g_entitySlots[i];
            _gameState.UpdateAnimation(entity);
        }
    }

    private void MovePlayer()
    {
        //TODO: impliment
        //this function is MASSIVE
        //perhaps the largest in the entire game
    }

    private void UpdateEntitiesEvents()
    {
        MovePlayer();

        if (StaticVariables.g_numberOfEntity > 0)
        {
            for (var dex = 1; dex < StaticVariables.g_numberOfEntity; dex++)
            {
                var entity = StaticVariables.g_entitySlots[dex];
                var evttype = -1;
                if (entity.PlatformEntity == null && entity.Status < 5)
                {
                    switch (entity.Status)
                    {
                        case 1://loading/activating
                            evttype = ScriptHelper.ProgramALoad;
                            entity.Status = 2;
                            break;
                        case 2://normal

                            if ((entity.Flags & 0x100000) != 0)
                            {
                                if (entity.Slope_18c == 4)
                                {
                                    _gameState.DestroyEntity(entity, 6);

                                    evttype = -1;
                                    break;
                                }
                            }

                            if ((entity.Flags & 0x200000) != 0)
                            {
                                if ((entity.CombinedVramFlagsOR & 0x8004) != 0)
                                {
                                    _gameState.DestroyEntity(entity, -1);

                                    evttype = -1;
                                    break;
                                }
                            }

                            if ((entity.Flags & 0x10) != 0)
                            {
                                if (entity.ForceAdjusted != 0 || entity.IsAboveGround != 3)
                                {
                                    entity.Status = 3;//deactivate
                                    evttype = ScriptHelper.ProgramEDeactivate;
                                    break;
                                }
                            }

                            if ((entity.Flags & 0x20) != 0)
                            {
                                if (entity.HitCounter != 0)
                                {
                                    entity.Status = 3;//decativate
                                    evttype = ScriptHelper.ProgramEDeactivate;
                                    break;
                                }
                            }

                            if ((entity.Flags & 0x40) != 0)
                            {
                                if (entity.ForceResetAnimationFlag != 0)
                                {
                                    entity.Status = 3;//decativate
                                    evttype = ScriptHelper.ProgramEDeactivate;
                                    break;
                                }
                            }

                            if (entity.TouchingEntity != null)
                            {
                                evttype = ScriptHelper.ProgramDTouch;
                                break;
                            }

                            //i think this gamevar is more than just activecollitionentity, 
                            //the interact button probabaly has to be down for this to be set
                            if (StaticVariables.g_activeCollisionEntity != entity)
                            {
                                evttype = 2;
                                break;
                            }
                            //if it gets here it means the player is interacting with this entity

                            if (entity.SpriteProgramIndexes[ScriptHelper.ProgramFInteract] != 0)
                            {
                                evttype = 5;
                                break;
                            }

                            if (entity.ProgramIndexes[ScriptHelper.ProgramFInteract] != 0)
                            {
                                evttype = 5;
                                break;
                            }
                            evttype = 2;
                            break;
                        case 3://deactivating
                            evttype = ScriptHelper.ProgramEDeactivate;
                            break;
                        case 0:
                        case 4:
                            evttype = -1;
                            break;
                    }
                }
                entity.EventTrigger = evttype;
            }
        }

        //run events
        var keepgoing = false;
        do
        {
            keepgoing = false;
            if (StaticVariables.g_numberOfEntity > 0)
            {
                //foreach entity besides player
                for (var dex = 1; dex < StaticVariables.g_numberOfEntity; dex++)
                {
                    var entity = StaticVariables.g_entitySlots[dex];

                    if (entity.EventTrigger != -1)
                    {
                        var progindex = entity.ProgramIndexes[entity.EventTrigger] & 0x7f;

                        if (progindex != 0)
                        {
                            System.Diagnostics.Debugger.Break();
                            //_entityEventHandlers.RunEntityEventScripts(entity, entity.EventTrigger);
                            entity.EventTrigger = -1;
                        }
                        else
                        {
                            var eventid = entity.SpriteProgramIndexes[entity.EventTrigger];
                            System.Diagnostics.Debugger.Break();
                            //_entityEventHandlers.SpriteHandlers.RunSpriteHandler(entity.EventTrigger, eventid, entity);
                            entity.EventTrigger = -1;
                        }

                        keepgoing = true;
                    }
                }
            }

        } while (keepgoing);
    }

    private void UpdateEntitiesCounters()
    {
        if (StaticVariables.g_numberOfEntity >= 0)
        {
            for (var dex = 0; dex <= StaticVariables.g_numberOfEntity; dex++)
            {
                var entity = StaticVariables.g_entitySlots[dex];
                entity.UnknownCounter++;
                if (entity.DamagedTickCounter != 0)
                {
                    entity.DamagedTickCounter--;
                }

                if (entity.FrameColTickCounter != 0)
                {
                    entity.FrameColTickCounter--;
                }
            }
        }

        //displays debug records here

    }

    private void UpdateDestroyedEntities()
    {
        var max = 0;
        for (var i = 0; i < StaticVariables.g_entitySlots.Length; i++)
        {
            var entity = StaticVariables.g_entitySlots[i];

            if (entity.Status == 4)
            {
                //zero out the properties
                entity = new Entity();
                entity.EntityRefId = -1;
                StaticVariables.g_entitySlots[i] = entity;
                //entity.Index = i;
            }

            if (entity.Status != 0)
            {
                max = i;
            }
        }

        StaticVariables.g_numberOfEntity = max;
    }

    private void UpdateEntityLists()
    {
        StaticVariables.g_activeEntityCount = 0;
        StaticVariables.g_collideableEntitiesCount = 0;
        StaticVariables.g_visibleEntityCount = 0;

        if (StaticVariables.g_numberOfEntity < 0)
        {
            return;
        }

        for (int i = 0; i < StaticVariables.g_numberOfEntity; i++)
        {
            var entity = StaticVariables.g_entitySlots[i];

            //processable
            if (entity.Status - 2 < 2 && entity.PlatformEntity == null)
            {
                StaticVariables.g_activeEntities[StaticVariables.g_activeEntityCount++] = entity;
            }

            //collidable
            if ((entity.Flags & 0x80) != 0 && (entity.AnimFlags & 0x80) == 0 && entity.PlatformEntity == null)
            {
                StaticVariables.g_collideableEntities[StaticVariables.g_collideableEntitiesCount++] = entity;
            }

            //renderable
            if (entity.Status - 2 < 2 && (entity.DamagedTickCounter & 3) != 3)//flicker effect, every 3rd frame when being damaged
            {
                StaticVariables.g_visibleEntities[StaticVariables.g_visibleEntityCount++] = entity;
            }
        }
    }

    public void UpdateMapEvents()
    {
        if ((StaticVariables.g_playerControlFlags & 0x48) != 0)
        {
            return;
        }

        var medex = 0;
        var playerEntity = StaticVariables.PlayerEntity;

        foreach (var mapEvent in _gameState.MapEvents)
        {
            var eventCode = mapEvent.ProgramBMap;
            if ((eventCode & 0x7f) == 0)
            {
                continue;
            }

            var rec = mapEvent.MapEventRecord;
            if (playerEntity.TileX < rec.X1 || playerEntity.TileX > rec.X2 || playerEntity.TileY < rec.Y1 || playerEntity.TileY > rec.Y2)
            {
                playerEntity.ProgramIndexes[ScriptHelper.ProgramBMap] = mapEvent.ProgramBMap;
                playerEntity.MapEventProgramId = mapEvent.ProgramBMap;
                playerEntity.EventProgramState = mapEvent.EventData;
                playerEntity.EventTrigger = medex;
                playerEntity.LogicContextEntity = mapEvent.Entity;
                System.Diagnostics.Debugger.Break();
                //_entityEventHandlers.RunEntityEventScripts(playerEntity, ScriptHelper.ProgramBMap);

                mapEvent.ProgramBMap = playerEntity.ProgramIndexes[ScriptHelper.ProgramBMap];
                mapEvent.EventData = playerEntity.EventProgramState;
                mapEvent.Entity = playerEntity.LogicContextEntity;
            }
            else
            {
                mapEvent.EventData.Sp = 0;
                mapEvent.EventData.Exp = 0;
                mapEvent.EventData.LogicResult = 0;
                mapEvent.Entity = playerEntity;
                mapEvent.ProgramBMap = rec.EventCodesBIndex;
            }
            medex++;
        }
    }




    //updates effect animations and adds sprites to spriteRefList
    public void UpdateAllActiveEffectsPostLogic()
    {
        foreach (var effect in StaticVariables.g_effectSlots)
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

            effect.SpriteRef.DepthSortVal = effect.DepthSortVal;
            effect.SpriteRef.X = effect.X;
            effect.SpriteRef.Y = effect.Y;
            effect.SpriteRef.Z = effect.Z;
            _gameState.SpriteRefs[_gameState.NumSprites++] = effect.SpriteRef;
        }
    }

    public void UpdateEffectAnimation(SpriteEffect effect)
    {
        if (effect.CurrentSpriteTableIndex != effect.TargetSpriteTableIndex
            || effect.CurrentIsMapSprite != effect.TargetIsMapSprite)
        {
            int addtosheet, addtopal;
            var record = _gameState.GetEffectSpriteFromSpriteTable(effect.TargetIsMapSprite != 0, effect.TargetSpriteTableIndex, out addtosheet, out addtopal);
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
                effect.AnimIndex++;
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

    public void UpdateEffectPosition(SpriteEffect effect)
    {
        if (effect.UpdateMode == 0)
        {
            effect.X += effect.XForce; //forces?
            effect.Y += effect.YForce;
            effect.Z += effect.ZForce;
            //some kind of unique id? maybe its used for zsorting
            effect.DepthSortVal = (int)(effect.Y & 0xffff0000) + (effect.Z >> 16) + (effect.SpriteRef.NumImages << 16);
            return;
        }

        if (effect.UpdateMode == 1)
        {
            var entity = effect.AttachedEntity;
            if (entity.Status != 0)
            {
                effect.X = entity.XPos + effect.XOff;
                effect.Y = entity.YPos + effect.YOff;
                effect.Z = entity.ZPos + effect.ZOff;
                effect.DepthSortVal = entity.DepthSortVal + effect.DepthSortMod;
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
            effect.DepthSortVal = effect.AttachedEntity.DepthSortVal + effect.DepthSortMod;

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
}