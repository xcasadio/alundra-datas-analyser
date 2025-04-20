namespace GraphicsTools.Alundra
{
    public class GameEngine
    {
        public const int ScreenWidth = 320;
        public const int ScreenHeight = 224;
        public const int MapTileWidth = 24;
        public const int MapTileHeight = 16;

        private DatasBin _datasBin;
        private BalanceBin _balanceBin;
        private GameMap _map;
        private Dictionary<int, Bitmap> _cachedTiles;
        private Dictionary<int, List<Bitmap>> _cachedSprites;
        private GameState _game;
        private EventHandlers _eventHandlers;

        public GameEngine(DatasBin datasBin, BalanceBin balanceBin, SoundBin soundBin)
        {
            _datasBin = datasBin;
            _balanceBin = balanceBin;
            var reader = datasBin.OpenBin();
            if (!datasBin.AlundraGameMap.Loaded)
            {
                datasBin.AlundraGameMap.Load(reader, false);
            }

            reader.Close();

            _game = new GameState(datasBin.AlundraGameMap, _balanceBin, soundBin) { CamXPos = 100 << 16, CamYPos = 100 << 16 };

            _eventHandlers = new EventHandlers(_game);
        }

        public void Render(Graphics g)
        {
            var curxpos = _game.CamXPos >> 16;
            var curypos = _game.CamYPos >> 16;

            var curxtile = curxpos / 24;

            var sinfo = _map.Spriteinfo;
            var gensi = _datasBin.AlundraGameMap.Spriteinfo;

            for (var y = 0; y < 60; y++)
            {
                //draw tiles on this row
                for (var x = curxtile; x < curxtile + ScreenWidth / 24 + 2; x++)
                {
                    var tile = _map.Map.MapTiles[y * 52 + x];
                    //render tile
                    var dx = x * 24 - curxpos;
                    var dy = (y - tile.Height) * 16 - curypos;

                    if (dy > -16 && dy < ScreenHeight && tile.TileId != -1)
                    {
                        DrawTile(tile.TileId, dx, dy, g);
                    }

                    if (tile.WallTiles != null)
                    {
                        var walltiles = tile.WallTiles;
                        int dex;
                        dy -= walltiles.Offset * 16;
                        for (dex = 0; dex < walltiles.Count; dex++)
                        {
                            dy += 16;
                            //render wall tile
                            if (dy > -16 && dy < ScreenHeight && walltiles.Tiles[dex] != -1)
                            {
                                DrawTile(walltiles.Tiles[dex], dx, dy, g);
                            }
                        }
                    }
                }

                //draw sprites who are on this row
                for (var sdex = 0; sdex < _game.MaxEntity; sdex++)
                {
                    var si = _game.Entities[sdex];
                    if (si.Status == 5)
                    {
                        continue;
                    }

                    if (si.YTile != y)
                    {
                        continue;//if its not in this row, continue
                    }


                    //var tile = selectedGame.map.maptiles[sx + sy * selectedGame.map.width];
                    var scx = (si.ModdedXPos >> 16) - curxpos;
                    var scy = (si.ModdedYPos >> 16) - (si.ModdedZPos >> 16) - curypos;
                    if (si.Sprite != null)
                    {
                        int idex;

                        var gm = _datasBin.AlundraGameMap;//general sprite
                        if (si.IsMapSprite)
                        {
                            gm = _map;//map sprite
                        }

                        var iset = si.Frame.Images;
                        for (idex = iset.Numimages - 1; idex >= 0; idex--)
                        {
                            var img = iset.Images[idex];

                            DrawSprite(gm, img, scx, scy, g);
                        }

                    }
                }
            }
        }

        private Point[] _pnts = new Point[4];
        private void DrawSprite(GameMap gm, SiImage img, int x, int y, Graphics g)
        {
            var bmp = gm.GetSpriteBitmap(img);
            _pnts[0].X = x + img.X1;
            _pnts[1].X = x + img.X2;
            _pnts[2].X = x + img.X3;
            _pnts[3].X = x + img.X4;

            _pnts[0].Y = y + img.Y1;
            _pnts[1].Y = y + img.Y2;
            _pnts[2].Y = y + img.Y3;
            _pnts[3].Y = y + img.Y4;
            g.DrawImage(bmp, _pnts);
        }

        private void DrawTile(int tileid, int x, int y, Graphics g)
        {
            var bmp = _map.GetTileBitmap(tileid);
            g.DrawImage(bmp, x, y);
        }

        public void LoadMap(GameMap gmap)
        {
            _map = gmap;
            if (!_map.Loaded)
            {
                var reader = _datasBin.OpenBin();
                _map.Load(reader, true);
                reader.Close();
            }

            _game.LoadMap(_map);



            //spriteinfo
            for (var dex = 0; dex < _map.Spriteinfo.Entities.Entities.Length; dex++)
            {
                var entity = _map.Spriteinfo.Entities.Entities[dex];
                if (entity != null)
                {

                }
            }


            for (var dex = 0; dex < _map.Spriteinfo.MapEvents.Records.Length; dex++)
            {
                var record = _map.Spriteinfo.MapEvents.Records[dex];
                if (record != null)
                {

                }
            }


            for (var dex = 0; dex < _map.Spriteinfo.Sprites.Length; dex++)
            {
                var sprite = _map.Spriteinfo.Sprites[dex];
                if (sprite != null)
                {

                }
            }

        }

        public void MainLoop(Graphics g)
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

            } while (!_game.BreakoutGameLoop);
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

            /*if (game.PlayerControlSetting == 0
                && game.PlayerEntity._20 == 0
                && game._1f0fbc == 0
                && (game.1dd7ea & 0x803) != 0
                && game._1ef998 == 0
                && (game.PlayerInput & 0x100) == 0
                && _570f8() == 0)
                game.BreakoutGameLoop = true;*/

            //PlayMusic();
            //advance random seed num
            var rnd = (int)(_game.Seed * 0x7d2b89dd + 0xe06a02e7);
            _game.Seed = rnd;

            if (force)
            {
                _game.BreakoutGameLoop = false;
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
            _game.NumSprites = 0;

            UpdateMapEvents();

            UpdateEntities();

            UpdateEffects();

        }

        public void UpdateEntities()
        {
            if ((_game.PlayerControlSetting & 0x48) == 0)
            {
                ProcessDestroyedEntities();

                DoEvents();

                UpdateCounters();



                AddToLists();

                UpdateAnims();

                DoPhysics();

                UpdateActiveEffects();

                UpdateBalanceRecords();
            }
            else
            {
                AddToLists();
            }

            if (_game.CamFollowEntity != null)
            {
                if (_game.CamFollowEntity.Status <= 3)
                {
                    //gets halfwords
                    _game.CamTargetX = _game.CamFollowEntity.XPos >> 16;
                    _game.CamTargetY = _game.CamFollowEntity.YPos >> 16;
                    _game.CamTargetZ = _game.CamFollowEntity.ZPos >> 16;
                }
            }
            SetDepthSortVals();

            //add spriterefs
            if (_game.ToRenderCount > 0)
            {
                for (var dex = 0; dex < _game.ToRenderCount; dex++)
                {
                    var entity = _game.ToRenderList[dex];

                    entity.SpriteRef.DepthSortVal = entity.DepthSortVal;
                    entity.SpriteRef.X = entity.XPos;
                    entity.SpriteRef.Y = entity.YPos;
                    entity.SpriteRef.Z = entity.ZPos;
                    _game.SpriteRefs[_game.NumSprites++] = entity.SpriteRef;
                }
            }
        }

        private void DoPhysics()
        {
            for (var dex = 0; dex < _game.ToProcessesCount; dex++)
            {
                var entity = _game.ToProcessList[dex];
                entity.DoneMoving = false;
                entity.CollidedWithEntityZ = 0;
                entity.ForceAdjusted = 0;

                entity.ModdedXPos = entity.XPos + entity.XMod;
                entity.ModdedYPos = entity.YPos + entity.YMod;
                entity.ModdedZPos = entity.ZPos + entity.ZMod;
            }

            SetRidingEntities();
            UpdateForces();

            for (var dex = 0; dex < _game.ToCollideCount; dex++)
            {
                var entity = _game.ToCollideList[dex];
                if (entity.RidingEntity != null)
                {
                    UpdateRidingEntity(entity, entity.RidingEntity);
                }
            }

            for (var dex = 0; dex < _game.ToProcessesCount; dex++)
            {
                var entity = _game.ToProcessList[dex];
                if (!entity.DoneMoving)
                {
                    //MoveEntity(entity);
                }
            }

            for (var dex = 0; dex < _game.ToProcessesCount; dex++)
            {
                var entity = _game.ToProcessList[dex];
                _game.UpdateTile(entity);
            }
        }

        private void UpdateRidingEntity(SpriteInstance entity, SpriteInstance ridingEntity)
        {
            if (ridingEntity.RidingEntity != null)
            {
                UpdateRidingEntity(ridingEntity, ridingEntity.RidingEntity);
            }

            entity.FinalXForce += ridingEntity.AdjustedXForce;
            entity.FinalYForce += ridingEntity.AdjustedYForce;
            if (entity.AppliedZForce == 0)
            {
                entity.ZForce = ridingEntity.FinalZForce;
                entity.FinalYForce = ridingEntity.FinalZForce;
            }
        }

        private void SetRidingEntities()
        {
            for (var dex = 0; dex < _game.ToCollideCount; dex++)
            {
                var entity = _game.ToCollideList[dex];
                if ((entity.Flags & 0x4100) != 0x0100)
                {
                    continue;
                }

                for (var dex2 = 0; dex2 < _game.ToCollideCount; dex2++)
                {
                    var entity2 = _game.ToCollideList[dex2];
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

        private void UpdateForces()
        {
            var player = _game.PlayerEntity;
            for (var dex = 0; dex < _game.ToProcessesCount; dex++)
            {
                var entity = _game.ToProcessList[dex];
                if (entity == player)
                {
                    if (player.AppliedZForce != 0)
                    {
                        if ((player.Flags & 0x100) != 0
                            && (player._180 & 0x0010) != 0
                            && _game.SomeGravitySetting <= 0)
                        {
                            player.ZForce = player.AppliedZForce * 160;
                        }
                        else
                        {
                            player.ZForce = player.AppliedZForce << 8;
                        }
                    }
                    else
                    {
                        if ((player.Flags & 0x0100) != 0)
                        {
                            var force = player.ZForce - (_game.GameMap.Info.Gravity << 8);
                            if (force < 0)
                            {
                                force = -force;//abs
                            }

                            var terminal = _game.GameMap.Info.TerminalVelocity << 8;
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
                    if ((player._180 & 0x0020) != 0)
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
                    if ((player._180 & 0x0008) != 0
                        && _game.SomeGravitySetting <= 0)
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

                    if (entity.AppliedZForce != 0)
                    {
                        if ((entity.AppliedZForce & 0xffff) == 0x8000
                            && (entity.Flags & 0x0100) == 0)
                        {
                            entity.ZForce = entity.AppliedZForce << 8;
                        }
                    }

                    if ((entity.Flags & 0x0100) != 0)
                    {
                        //this applies gravity (limited by terminal velicity) to the z force
                        var force = entity.ZForce - (_game.GameMap.Info.Gravity << 8);
                        if (force < 0)
                        {
                            force = -force;//abs
                        }

                        var terminal = _game.GameMap.Info.TerminalVelocity << 8;
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

        private void SetAdjustedXyForces(SpriteInstance entity)
        {
            var lastinteractx = entity.InteractXForce;
            var lastinteracty = entity.InteractYForce;
            entity.InteractYForce = 0;
            entity.InteractXForce = 0;
            var xval = entity.XForce + Helper.XForceTable[entity.SomethingForceIndex & 0xf] >> _game.GameMap.Info.Gravity;
            var yval = entity.YForce + Helper.YForceTable[entity.SomethingForceIndex & 0xf] >> _game.GameMap.Info.Gravity;

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

        private void SetXyForces(SpriteInstance entity)
        {
            if (entity.Speed != entity.AnimSet.Speed
                || entity.TargetDir != entity.CurDir)
            {
                entity.Speed = entity.AnimSet.Speed;

                entity.TargetXForce = Helper.DirVectorsX[entity.TargetDir] * entity.AnimSet.Speed;

                entity.CurDir = entity.TargetDir;

                entity.TargetYForce = Helper.DirVectorsY[entity.TargetDir] * entity.AnimSet.Speed;
            }
            else if (entity.Acceleration == (entity.AnimSet.Acceleration & 0xf))
            {
                return;
            }

            entity.Acceleration = entity.AnimSet.Acceleration & 0xf;

            entity.XForceStep = Math.Abs(entity.TargetXForce - entity.XForce) >> entity.Acceleration;

            entity.YForceStep = Math.Abs(entity.TargetYForce - entity.YForce) >> entity.Acceleration;
        }

        private void SetDepthSortVals()
        {
            if (_game.ToRenderCount <= 0)
            {
                return;
            }

            for (var dex = 0; dex < _game.ToRenderCount; dex++)
            {
                var entity = _game.ToRenderList[dex];
                entity.DepthSortVal = 0;
                entity.SortTop = entity.ModdedZPos + entity.Height;
            }

            for (var dex = 0; dex < _game.ToRenderCount; dex++)
            {
                var entity = _game.ToRenderList[dex];
                if (entity.DepthSortVal == 0)
                {
                    SetDepthSortVal(entity);
                }
            }

            for (var dex = 0; dex < _game.ToRenderCount; dex++)
            {
                var entity = _game.ToRenderList[dex];
                entity.DepthSortVal = (int)(entity.DepthSortVal & 0xffff0000) + (entity.ZPos & 0xffff);
            }
        }

        private void SetDepthSortVal(SpriteInstance entity)
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

            for (var dex = 0; dex < _game.ToCollideCount; dex++)
            {
                var checkme = _game.ToCollideList[dex];
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
            if (_game.ToProcessesCount <= 0)
            {
                return;
            }

            for (var dex = 0; dex < _game.ToProcessesCount; dex++)
            {
                var entity = _game.ToProcessList[dex];

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

                for (var dex2 = 0; dex2 < _game.ToProcessesCount; dex2++)
                {
                    var checkme = _game.ToProcessList[dex2];
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
                            _game.CreateEffect_Type1(0, 4, 0, checkme, width, 0, 0, 0);
                        }
                        if (valdex == 7 || valdex == 9)
                        {
                            _game.CreateEffect_Type1(0, 5, 0, checkme, 1, 0, 0, 0);
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
                var effect = _game.CreateEffect_Type0(0, 9, 0, x, y, z);
                if (effect == null)
                {
                    continue;
                }

                var baseforce = (int)(0xffff << 16);

                var i = _game.Seed;
                var val1 = (int)(i * 0x7d2b89dd);
                var val2 = (int)(0xe06a02e7 + val1);
                var targetval = (int)(((long)val2 * 0x20001) >> 32);
                _game.Seed = val2;

                effect.XForce = targetval + baseforce;

                i = _game.Seed;
                val1 = (int)(i * 0x7d2b89dd);
                val2 = (int)(0xe06a02e7 + val1);
                targetval = (int)(((long)val2 * 0x20001) >> 32);
                _game.Seed = val2;

                effect.YForce = targetval + baseforce;

                i = _game.Seed;
                val1 = (int)(i * 0x7d2b89dd);
                val2 = (int)(0xe06a02e7 + val1);
                targetval = (int)(((long)val2 * 0x20001) >> 32);
                _game.Seed = val2;

                effect.ZForce = targetval + baseforce;
            }
        }

        private void UpdateActiveEffects()
        {
            if (_game.MaxEntity < 0)
            {
                return;
            }

            for (var dex = 0; dex < _game.MaxEntity; dex++)
            {
                var entity = _game.Entities[dex];
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
                    effect = _game.CreateEffect_Type3(0, 0, 0, entity, -1, 0, 0, 0);
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
                if ((entity._18c == 4 || entity._190 == 4)
                    && entity._18c != entity._190)
                {
                    _game.CreateEffect_Type0(0, 6, 0, entity.XPos, entity.YPos, entity.CollidedWithEntityZ);
                }

                if (entity._18c >= 8)
                {
                    continue;
                }

                switch (entity._18c)
                {
                    case 1:
                    case 2:
                        effect.TargetIsMapSprite = 0;
                        effect.TargetSpriteTableIndex = 1;
                        effect.TargetAnim = (byte)animid;
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

                        _game.CreateEffect_Type0(0, 0x15, 0, entity.XPos, entity.YPos, entity.CollidedWithEntityZ);
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

                        _game.CreateEffect_Type0(0, _game.GameMap.Info.SlideEffectId, 0, entity.XPos, entity.YPos, entity.CollidedWithEntityZ);
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

                effect.TargetAnim = (byte)animid;
                effect.Status = 2;

                effect.X = entity.XPos;
                effect.Y = entity.YPos;
                effect.Z = entity.CollidedWithEntityZ;
            }
        }

        private void UpdateAnims()
        {
            for (var dex = 0; dex < _game.ToProcessesCount; dex++)
            {
                var entity = _game.Entities[dex];
                _game.UpdateAnim(entity);
            }
        }

        private void MovePlayer()
        {
            //TODO: impliment
            //this function is MASSIVE
            //perhaps the largest in the entire game
        }

        private void DoEvents()
        {
            MovePlayer();

            if (_game.MaxEntity > 0)
            {
                for (var dex = 1; dex < _game.MaxEntity; dex++)
                {
                    var entity = _game.Entities[dex];
                    var evttype = -1;
                    if (entity._20 == 0 && entity.Status < 5)
                    {
                        switch (entity.Status)
                        {
                            case 1://loading/activating
                                evttype = Helper.ProgramALoad;
                                entity.Status = 2;
                                break;
                            case 2://normal

                                if ((entity.Flags & 0x100000) != 0)
                                {
                                    if (entity._18c == 4)
                                    {
                                        _game.DestroyEntity(entity, 6);

                                        evttype = -1;
                                        break;
                                    }
                                }

                                if ((entity.Flags & 0x200000) != 0)
                                {
                                    if ((entity._180 & 0x8004) != 0)
                                    {
                                        _game.DestroyEntity(entity, -1);

                                        evttype = -1;
                                        break;
                                    }
                                }

                                if ((entity.Flags & 0x10) != 0)
                                {
                                    if (entity.ForceAdjusted != 0 || entity._144 != 3)
                                    {
                                        entity.Status = 3;//decativate
                                        evttype = Helper.ProgramEDeactivate;
                                        break;
                                    }
                                }

                                if ((entity.Flags & 0x20) != 0)
                                {
                                    if (entity.HitCounter != 0)
                                    {
                                        entity.Status = 3;//decativate
                                        evttype = Helper.ProgramEDeactivate;
                                        break;
                                    }
                                }

                                if ((entity.Flags & 0x40) != 0)
                                {
                                    if (entity.WierdNextFrameDelayFlag != 0)
                                    {
                                        entity.Status = 3;//decativate
                                        evttype = Helper.ProgramEDeactivate;
                                        break;
                                    }
                                }

                                if (entity.TouchingEntity != null)
                                {
                                    evttype = Helper.ProgramDTouch;
                                    break;
                                }

                                //i think this gamevar is more than just activecollitionentity, 
                                //the interact button probabaly has to be down for this to be set
                                if (_game.ActiveCollisionEntity != entity)
                                {
                                    evttype = 2;
                                    break;
                                }
                                //if it gets here it means the player is interacting with this entity

                                if (entity.SpriteProgramIndexes[Helper.ProgramFInteract] != 0)
                                {
                                    evttype = 5;
                                    break;
                                }

                                if (entity.ProgramIndexes[Helper.ProgramFInteract] != 0)
                                {
                                    evttype = 5;
                                    break;
                                }
                                evttype = 2;
                                break;
                            case 3://deactivating
                                evttype = Helper.ProgramEDeactivate;
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
                if (_game.MaxEntity > 0)
                {
                    //foreach entity besides player
                    for (var dex = 1; dex < _game.MaxEntity; dex++)
                    {
                        var entity = _game.Entities[dex];

                        if (entity.EventTrigger != -1)
                        {
                            var progindex = entity.ProgramIndexes[entity.EventTrigger] & 0x7f;

                            if (progindex != 0)
                            {
                                //run the eventhandler script
                                _eventHandlers.RunEntityEventScripts(entity, entity.EventTrigger);
                                entity.EventTrigger = -1;
                            }
                            else
                            {
                                var eventid = entity.SpriteProgramIndexes[entity.EventTrigger];
                                //run the sprite event handler
                                _eventHandlers.SpriteHandlers.RunSpriteHandler(entity.EventTrigger, eventid, entity);
                                entity.EventTrigger = -1;
                            }

                            keepgoing = true;
                        }
                    }
                }

            } while (keepgoing);
        }

        private void UpdateCounters()
        {
            if (_game.MaxEntity >= 0)
            {
                for (var dex = 0; dex <= _game.MaxEntity; dex++)
                {
                    var entity = _game.Entities[dex];
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

        private void ProcessDestroyedEntities()
        {
            var max = 0;
            for (var dex = 0; dex < _game.Entities.Length; dex++)
            {
                var entity = _game.Entities[dex];
                if (entity.Status == 4)
                {
                    //zero out the properties
                    entity = new SpriteInstance();
                    entity.EntityRefId = -1;
                    _game.Entities[dex] = entity;
                    entity.Index = dex;
                }
                if (entity.Status != 0)
                {
                    max = dex;
                }
            }

            _game.MaxEntity = max;
        }

        private void AddToLists()
        {
            _game.ToProcessesCount = 0;
            _game.ToCollideCount = 0;
            _game.ToRenderCount = 0;

            if (_game.MaxEntity < 0)
            {
                return;
            }

            foreach (var entity in _game.Entities)
            {
                //processable
                if (entity.Status - 2 < 2 && entity._20 == 0)
                {
                    _game.ToProcessList[_game.ToProcessesCount++] = entity;
                }

                //collidable
                if ((entity.Flags & 0x80) != 0
                    && (entity.AnimFlags & 0x80) == 0
                    && entity.PlatformEntity == null)
                {
                    _game.ToCollideList[_game.ToCollideCount++] = entity;
                }

                //renderable
                if (entity.Status - 2 < 2 && (entity.DamagedTickCounter & 3) != 3)//flicker effect, every 3rd frame when being damaged
                {
                    _game.ToRenderList[_game.ToRenderCount++] = entity;
                }
            }

        }

        public void UpdateMapEvents()
        {
            if ((_game.PlayerControlSetting & 0x48) != 0)
            {
                return;
            }

            var medex = 0;
            var p = _game.PlayerEntity;
            foreach (var mapevent in _game.MapEvents)
            {
                var eventcode = mapevent.ProgramBMap;
                if ((eventcode & 0x7f) == 0)
                {
                    continue;
                }

                var rec = mapevent.MapEventRecord;
                if (p.XTile < rec.X1 || p.XTile > rec.X2 || p.YTile < rec.Y1 || p.YTile > rec.Y2)
                {
                    p.ProgramIndexes[Helper.ProgramBMap] = mapevent.ProgramBMap;
                    p.MapEventProgramId = mapevent.ProgramBMap;
                    p.Eventdata = mapevent.EventData;
                    p.EventTrigger = medex;
                    p.EntitySelf = mapevent.Entity;
                    _eventHandlers.RunEntityEventScripts(p, Helper.ProgramBMap);

                    mapevent.ProgramBMap = p.ProgramIndexes[Helper.ProgramBMap];
                    mapevent.EventData = p.Eventdata;
                    mapevent.Entity = p.EntitySelf;
                }
                else
                {
                    mapevent.EventData.Sp = 0;
                    mapevent.EventData.Exp = 0;
                    mapevent.EventData.LogicResult = 0;
                    mapevent.Entity = p;
                    mapevent.ProgramBMap = rec.Eventcodesbindex;
                }
                medex++;
            }
        }




        //updates effect animations and adds sprites to spritereflist
        public void UpdateEffects()
        {
            foreach (var effect in _game.SpriteEffects)
            {
                if (effect.Status != 2)
                {
                    continue;
                }

                if ((_game.PlayerControlSetting & 0x48) == 0)
                {
                    if (effect.DestroyFlag != 0)
                    {
                        effect.Status = 0;
                        continue;
                    }

                    UpdateEffectAnim(effect);

                    UpdateEffectByType(effect);
                }

                effect.SpriteRef.DepthSortVal = effect.DepthSortVal;
                effect.SpriteRef.X = effect.X;
                effect.SpriteRef.Y = effect.Y;
                effect.SpriteRef.Z = effect.Z;
                _game.SpriteRefs[_game.NumSprites++] = effect.SpriteRef;
            }
        }

        public void UpdateEffectAnim(SpriteEffect effect)
        {
            if (effect.CurSpriteTableIndex != effect.TargetSpriteTableIndex
                || effect.CurIsMapSprite != effect.TargetIsMapSprite)
            {
                int addtosheet, addtopal;
                var record = _game.GetEffectSpriteFromSpriteTable(effect.TargetIsMapSprite != 0, effect.TargetSpriteTableIndex, out addtosheet, out addtopal);
                if (record == null)
                {
                    effect.DestroyFlag = 1;
                    effect.SpriteRef.Images = null;
                    effect.SpriteRef.NumImages = 0;
                    //field after numimages = 0
                    return;
                }

                effect.SpriteEffectRecord = record;
                effect.CurIsMapSprite = effect.TargetIsMapSprite;
                effect.CurSpriteTableIndex = effect.TargetSpriteTableIndex;

                effect.AddToSheet = addtosheet;
                effect.AddToPalette = addtopal;
                effect.CurAnim = (byte)~effect.TargetAnim;
            }

            if (effect.CurAnim != effect.TargetAnim)
            {
                var animation = effect.SpriteEffectRecord.PreloadedAnims[effect.TargetAnim];
                effect.Animdex = 0;
                var nframe = animation.Frames[effect.Animdex];
                effect.CurAnim = effect.TargetAnim;
                effect.Delay = 0;
                effect.DestroyFlag = 0;
                effect.FirstFrame = nframe;
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
                    effect.Animdex++;
                    var anim = effect.SpriteEffectRecord.PreloadedAnims[effect.TargetAnim];
                    frame = anim.Frames[effect.Animdex];
                    effect.Frame = frame;
                    if (frame.Images != null)
                    {
                        effect.SpriteRef.Images = frame.Images.Images;
                        effect.SpriteRef.DepthSortVal = frame.Images.Unknown;
                        effect.SpriteRef.NumImages = frame.Images.Numimages;
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
                        effect.Animdex = 0;
                        effect.Frame = effect.FirstFrame;
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

        public void UpdateEffectByType(SpriteEffect effect)
        {
            if (effect.EffectType == 0)
            {
                effect.X += effect.XForce; //forces?
                effect.Y += effect.YForce;
                effect.Z += effect.ZForce;
                //some kind of unique id? maybe its used for zsorting
                effect.DepthSortVal = (int)(effect.Y & 0xffff0000) + (effect.Z >> 16) + (effect.SpriteRef.NumImages << 16);
                return;
            }

            if (effect.EffectType == 1)
            {
                var entity = effect.EntityRef;
                if (entity.Status != 0)
                {
                    effect.X = entity.XPos + effect.XOff;
                    effect.Y = entity.YPos + effect.YOff;
                    effect.Z = entity.ZPos + effect.ZOff;
                    effect.DepthSortVal = entity.DepthSortVal + effect.DepthSortMod;
                    if (entity.Status == 4)
                    {
                        effect.EffectType = 2;
                    }
                }
                else
                {
                    effect.EffectType = 2;
                }
            }
            else if (effect.EffectType != 3)
            {
                return;
            }
            //param3== 1 falls through to here, and param3 == 3 is here
            effect.X += effect.XForce; //forces?
            effect.Y += effect.YForce;
            effect.Z += effect.ZForce;

            if (effect.EntityRef.Status != 0)
            {
                effect.DepthSortVal = effect.EntityRef.DepthSortVal + effect.DepthSortMod;

                if (effect.EntityRef.Status == 4)
                {
                    effect.EffectType = 2;
                }
            }
            else
            {
                effect.EffectType = 2;
            }
        }
    }
}
