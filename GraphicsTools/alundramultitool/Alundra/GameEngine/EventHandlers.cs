using System.Diagnostics;

namespace GraphicsTools.Alundra
{
    public class EventHandlers
    {
        private GameState _gameState;
        private Dictionary<int, ScriptEventHandler> _handlers = new();
        public readonly SpriteEventHandlers SpriteHandlers;
        public EventHandlers(GameState gameState)
        {
            _gameState = gameState;

            SpriteHandlers = new SpriteEventHandlers(gameState);
            //add handlers
            for (var dex = 0; dex <= 0xff; dex++)
            {
                _handlers.Add(dex, __Unknown_Handler);
            }

            _handlers[0x2] = _02_Goto_Handler;
            _handlers[0x3] = _03_BranchIfTrue_Handler;
            _handlers[0x4] = _04_BranchIfFalse_Handler;
            _handlers[0x5] = _05_FlagOn_Handler;
            _handlers[0x6] = _06_FlagOff_Handler;
            _handlers[0x7] = _07_CheckEntityInArea_Handler;
            _handlers[0x8] = _08_Turn_Handler;
            _handlers[0x9] = _09_SetDir_Handler;
            _handlers[0xa] = _0a_Reverse_Handler;
            _handlers[0xc] = _0c_SetRandomDir_Handler;
            _handlers[0xd] = _0d_Dialog_Handler;
            _handlers[0x10] = _10_LoseControl_Handler;
            _handlers[0x11] = _11_GainControl_Handler;
            _handlers[0x12] = _12_PlaySound1_Handler;
            _handlers[0x15] = _15_ResetZPos_Handler;
            _handlers[0x16] = _16_GravityOn_Handler;
            _handlers[0x17] = _17_GravityOff_Handler;
            _handlers[0x19] = _19_Deactivate_Handler;
            _handlers[0x1a] = _1a_SetAnim_Handler;
            _handlers[0x1b] = _1b_Fly_Handler;
            _handlers[0x1c] = _1c_WaitAnim_Handler;
            _handlers[0x1d] = _1d_WaitAnim2_Handler;
            _handlers[0x1e] = _1e_WaitWalk_Handler;
            _handlers[0x1f] = _1f_WaitWalk2_Handler;
            _handlers[0x24] = _24_WaitForceAdjusted_Handler;
            _handlers[0x25] = _25_WaitEntityCollisionZOr144_Handler;
            _handlers[0x26] = _26_WaitForceAdjustedOrEntityCollisionZ_Handler;
            _handlers[0x27] = _27_FacePlayer_Handler;
            _handlers[0x28] = _28_Flag2On_Handler;
            _handlers[0x29] = _29_Flag2Off_Handler;
            _handlers[0x2a] = _2a_Flag3On_Handler;
            _handlers[0x2b] = _2b_Flag3Off_Handler;
            _handlers[0x2d] = _2d_ActivateEntity_Handler;
            _handlers[0x2e] = _2e_Hide_Handler;
            _handlers[0x2f] = _2f_CheckPlayerInput_Handler;
            _handlers[0x30] = _30_IfFlagOff_Handler;
            _handlers[0x31] = _31_IfFlagOn_Handler;
            _handlers[0x32] = _32_FlagToggle_Handler;
            _handlers[0x33] = _33_CheckFlagsOn_Handler;
            _handlers[0x34] = _34_CheckFlagsOff_Handler;
            _handlers[0x35] = _35_UntilFlagOff_Handler;
            _handlers[0x36] = _36_UntilFlagOn_Handler;
            _handlers[0x37] = _37_Wait_Handler;
            _handlers[0x3b] = _3b_CheckPlayerInArea_Handler;
            _handlers[0x40] = _40_SetProgramIndex_Handler;
            _handlers[0x41] = _41_SetSpriteProgramIndex_Handler;
            _handlers[0x45] = _45_Flag4Off_Handler;
            _handlers[0x46] = _46_Flag4On_Handler;
            _handlers[0x49] = _49_Restart_Handler;
            _handlers[0x4a] = _4a_IfTrueRestart_Handler;
            _handlers[0x4b] = _4b_IfFalseRestart_Handler;
            _handlers[0x54] = _54_SetWalkable_Handler;
            _handlers[0x55] = _55_SetNonWalkable_Handler;
            _handlers[0x58] = _58_DirectionalBranch_Handler;
            _handlers[0x59] = _59_SetEntityAnim_Handler;
            _handlers[0x5a] = _5a_TurnEntity_Handler;
            _handlers[0x5b] = _5b_TurnEntityWithAnim_Handler;
            _handlers[0x62] = _62_EntityFlagsOn_Handler;
            _handlers[0x63] = _63_EntityFlagsOff_Handler;
            _handlers[0x64] = _64_SetEntityPos_Handler;
            _handlers[0x65] = _65_MoveEntityPos_Handler;
            _handlers[0x67] = _67_CamFollowEntity_Handler;
            _handlers[0x70] = _70_Check144_Handler;

        }

        public void RunEntityEventScripts(SpriteInstance entity, int eventprogramtype)
        {
            EventProgramState eventdata;
            if (eventprogramtype < 6)
            {
                switch (eventprogramtype)
                {
                    case Helper.ProgramBMap:
                        eventdata = entity.Eventdata;
                        if (eventdata.Exp == 0
                            || eventdata.Sp == 0)
                        {
                            InitEventData(entity, eventprogramtype, eventdata);
                        }
                        break;
                    case Helper.ProgramCTick:
                        eventdata = entity.Eventdata;
                        if (eventdata.Exp == 0
                            || eventdata.Sp == 0)
                        {
                            InitEventData(entity, eventprogramtype, eventdata);
                        }
                        else
                        {
                            if (entity.MapEventProgramId != 2)
                            {
                                //TODO make sure these event vars are right
                                entity.TargetAnim = entity.UnknownEventAnim;
                                entity.TargetDir = entity.UnknownEventDir;
                            }
                        }
                        break;
                    default:
                        if (Helper.ProgramFInteract == 5)//have to do it here because switch fallthrough isnt allowed in c#
                        {
                            _gameState.PlayerEntity.YForceStep = 0;
                            _gameState.PlayerEntity.XForceStep = 0;
                            _gameState.PlayerEntity.XForce = 0;
                            _gameState.PlayerEntity.YForce = 0;
                        }

                        eventdata = _gameState.GlobalEventData;

                        if (entity.MapEventProgramId != 2)
                        {
                            entity.UnknownEventAnim = entity.TargetAnim;
                            entity.UnknownEventDir = entity.TargetDir;
                        }

                        InitEventData(entity, eventprogramtype, eventdata);
                        break;
                }
            }
            else
            {
                throw new Exception("Illegal logic entry!");
            }
            bool sameasself;
            do
            {
                sameasself = false;

                _gameState.ActiveEventProgramType = eventprogramtype;
                _gameState.ActiveEventCode = -1;
                _gameState.EventProgsSet = 0;
                _gameState.ActiveEventProgIndex = entity.ProgramIndexes[eventprogramtype];
                _gameState.ActiveEntityRefId = entity.EntityRefId;

                var code = SpriteInfoEventCodes.Code;
                var evtcode = code[eventdata.Exp];

                if (evtcode == 0xff)
                {
                    break;
                }

                if (evtcode == 0)
                {
                    eventdata.Tick = 0;
                    eventdata.Exp++;
                    break;
                }

                var func = _handlers[evtcode];

                _gameState.PrevEventCode = _gameState.ActiveEventCode;
                _gameState.ActiveEventCode = evtcode;

                var advanced = func(entity.EntitySelf, entity, eventdata.Exp, eventdata, code);

                if (_gameState.EventProgsSet != 0)
                {
                    _gameState.EventProgsSet = 1;
                    if (entity != entity.EntitySelf)
                    {
                        entity.EntitySelf.Eventdata.Sp = 0;
                        entity.EntitySelf.Eventdata.Exp = 0;
                    }
                    else
                    {
                        sameasself = true;
                    }
                }

                if (advanced == 0)
                {
                    break;
                }

                eventdata.Tick = 0;
                eventdata.Exp += advanced;
            } while (true);

            if (sameasself)
            {
                eventdata.Sp = 0;
                eventdata.Exp = 0;
            }

        }

        private void InitEventData(SpriteInstance entity, int eventprogramtype, EventProgramState eventdata)
        {
            var codeindex = entity.ProgramIndexes[eventprogramtype];
            var si = _gameState.Global.SpriteInfo;
            var mod = 0;
            if ((codeindex & 0x80) != 0)
            {
                si = _gameState.GameMap.SpriteInfo;
                mod = 1024 * 512;
            }

            var sp = si.EventCodes.Eventcodestable[eventprogramtype][codeindex & 0x7f] + mod;
            eventdata.Sp = sp;
            eventdata.Exp = sp;

            //some error checking here, 
            //looking to see if the code pointers are in the correct range of where they should be
        }










        public int __Unknown_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] eventcode)
        {
            Debug.WriteLine("Data Logic Error!");
            return 0;
        }
        public int _02_Goto_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var offset = (short)(code[exp + 1] + (code[exp + 2] << 8));

            return offset;
        }

        public int _03_BranchIfTrue_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (eventData.LogicResult == 0)
            {
                return 3;
            }

            var offset = (short)(code[exp + 1] + (code[exp + 2] << 8));

            return offset;
        }

        public int _04_BranchIfFalse_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (eventData.LogicResult != 0)
            {
                return 3;
            }

            var offset = (short)(code[exp + 1] + (code[exp + 2] << 8));

            return offset;
        }

        public int _05_FlagOn_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (eventData.LogicResult != 0)
            {
                return 3;
            }

            var flagdata = code[exp + 1] + (code[exp + 2] << 8);
            //int flag = (flagdata >> 3) & 0xffc;
            var flag = (flagdata >> 5) & 0x3ff;
            int[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = _gameState.GameFlagsMap;
            }
            else//otherwise its a global flag
            {
                flags = _gameState.GameFlagsGlobal;
            }

            var bittoset = flagdata & 0x1f;

            //turn on the bit for this flag
            flags[flag] |= 1 << bittoset;

            return 3;
        }

        public int _06_FlagOff_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (eventData.LogicResult != 0)
            {
                return 3;
            }

            var flagdata = code[exp + 1] + (code[exp + 2] << 8);
            //int flag = (flagdata >> 3) & 0xffc;
            var flag = (flagdata >> 5) & 0x3ff;
            int[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = _gameState.GameFlagsMap;
            }
            else//otherwise its a global flag
            {
                flags = _gameState.GameFlagsGlobal;
            }

            var bittoset = flagdata & 0x1f;

            //turn off the bit for this flag
            flags[flag] &= ~(1 << bittoset);

            return 3;
        }

        public int _07_CheckEntityInArea_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            int x1 = code[exp + 2];
            int x2 = code[exp + 3];
            int y1 = code[exp + 4];
            int y2 = code[exp + 5];
            int z1 = code[exp + 6];
            int z2 = code[exp + 7];
            var numentities = _gameState.GetEntityFromRefId(entity, entityid);
            for (var dex = 0; dex < numentities; dex++)
            {
                var checkme = _gameState.GetEntityList[dex];
                if (checkme.XTile >= x1 && checkme.XTile <= x2
                    && checkme.YTile >= y1 && checkme.YTile <= y2
                    && checkme.ZTile >= z1 && checkme.ZTile <= z2)
                {
                    eventData.LogicResult = 1;
                    return 8;
                }
            }

            eventData.LogicResult = 0;

            return 8;
        }

        public int _08_Turn_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            entity.TargetDir = (entity.TargetDir + code[exp + 1]) & 0x1f;
            return 2;
        }

        public int _09_SetDir_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            entity.TargetDir = code[exp + 1] & 0x1f;
            return 2;
        }

        public int _0a_Reverse_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            entity.TargetDir = (entity.TargetDir + 0x10) & 0x1f;
            return 1;
        }

        //set animation, and block until entity has moved specified distance
        public int _0b_AnimWaitDistance_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            int animid = code[exp + 1];
            entity.TargetAnim = animid;
            if (exp != eventData.Tick)
            {
                eventData.Tick = exp;
                eventData.Variables[0] = entity.XPos;
                eventData.Variables[1] = entity.YPos;
                return 0;
            }
            var difx = eventData.Variables[0] - entity.XPos;
            var dify = eventData.Variables[1] - entity.YPos;
            if (difx < 0)
            {
                difx = -difx;
            }

            if (dify < 0)
            {
                dify = -dify;
            }

            var distance = code[exp + 2] + (code[exp + 3] << 8);

            if (difx >> 16 >= distance)
            {
                return 4;//done
            }

            if (dify >> 16 >= distance)
            {
                return 4;//done
            }

            return 0;//keep blocking
        }

        public int _0c_SetRandomDir_Handler(SpriteInstance entity, SpriteInstance entityself, int exp, EventProgramState eventData, byte[] code)
        {
            var i = _gameState.Seed;
            var val1 = (int)(i * 0x7d2b89dd);
            var val2 = (int)(0xe06a02e7 + val1);
            var val3 = (int)(((long)val2 * 4) >> 32);
            var dir = Helper.CardinalDirTable[val3];//val3 here is a number between 0 and 3
            _gameState.Seed = val2;
            entity.TargetDir = dir;
            return 1;
        }

        public int _0d_Dialog_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if ((entity.Flags & 0x800000) != 0)//has portrait
            {
                //TODO get it from memory, not disk
                //SIImageSet portrait = entity.Sprite.GetPortraitImageset(datasReader);
                //var img = portrait.images[0];
                //var bmps = gameState.GetSpriteImages(portrait);
                //var bmp = bmps[0];
                //WrapsDialogSetupPortrait(entity.XPos, entity.YPos, entity.ZPos, gameState.CamXPos, gameState.CamYPos, img.sx, img.sy, img.swidth, img.sheight, bmp);
            }
            //SetName(entity.NameId);

            //var ret = SetText(code[exp + 1], code[exp + 2]);

            //if (ret > 0)
            //    return 3;
            return 0;
        }

        public int _10_LoseControl_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            _gameState.PlayerControlSetting |= 0x4;
            return 1;
        }

        public int _11_GainControl_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            _gameState.PlayerControlSetting &= ~0x4;
            return 1;
        }

        public int _12_PlaySound1_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            //throw new Exception("impliment this one!");
            int soundid = code[exp + 1];
            return 2;
        }

        public int _15_ResetZPos_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (entity.EntityRecord == null)
            {
                Debug.Print("No InitData");
            }

            entity.ZPos = (entity.EntityRecord.Height * 8 - entity.ZMod) << 16;
            return 1;
        }

        public int _16_GravityOn_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            entity.Flags |= 0x100;
            return 1;
        }

        public int _17_GravityOff_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            entity.Flags &= ~0x100;
            return 1;
        }

        public int _19_Deactivate_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            entity.Status = 3;
            return 1;
        }

        public int _1a_SetAnim_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            entity.TargetAnim = code[exp + 1];
            return 2;
        }

        //sets zforce
        public int _1b_Fly_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var force = code[exp + 1] + (code[exp + 2] << 8);
            force = force << 16;//sign extend
            force = force >> 8;//get it to the correct multiple
            entity.ZForce = force;
            return 2;
        }

        public int _1c_WaitAnim_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (exp != eventData.Tick)
            {
                eventData.Tick = exp;
                eventData.Variables[0] = 0;
                entity.AnimCompleteCounter = 0;
                return 0;
            }

            if (entity.WierdNextFrameDelayFlag != 0)
            {
                entity.CurAnim = ~entity.TargetAnim;
            }

            if (entity.WierdNextFrameDelayFlag != 0 || entity.AnimCompleteCounter != 0)
            {
                eventData.Variables[0]++;
                entity.AnimCompleteCounter = 0;
            }


            var towait = code[exp + 1];
            if (eventData.Variables[0] >= towait)
            {
                return 2;
            }

            return 0;
        }

        //collision ends it
        public int _1d_WaitAnim2_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var ret = _1c_WaitAnim_Handler(entity, entityself, exp, eventData, code);

            if (ret != 0)
            {
                return ret;
            }

            if (entity.ForceAdjusted != 0)
            {
                return 2;
            }

            return 0;
        }

        //wait until they have walked a certain distance, collision pauses the walk
        public int _1e_WaitWalk_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (exp != eventData.Tick)
            {
                eventData.Tick = exp;
                eventData.Variables[0] = entity.XPos;
                eventData.Variables[1] = entity.YPos;
                return 0;
            }

            var x = Math.Abs(eventData.Variables[0] - entity.XPos) >> 16;
            var y = Math.Abs(eventData.Variables[1] - entity.YPos) >> 16;

            var distance = code[exp + 1] | (code[exp + 2] << 8);

            if (x >= distance || y >= distance)
            {
                return 3;
            }

            return 0;
        }

        //wait until they have walked a certain distance, collision ends the walk
        public int _1f_WaitWalk2_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var ret = _1e_WaitWalk_Handler(entity, entityself, exp, eventData, code);

            if (ret != 0)
            {
                return ret;
            }

            if (entity.ForceAdjusted != 0)
            {
                return 3;
            }

            return 0;
        }

        //wait until some force acts on the entity, like it bumps into something
        public int _24_WaitForceAdjusted_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (entity.ForceAdjusted > 0)
            {
                return 1;
            }

            return 0;
        }

        public int _25_WaitEntityCollisionZOr144_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (entity.CollidedWithEntityZ != 0)
            {
                return 1;
            }

            if (entity._144 != 0)
            {
                return 1;
            }

            return 0;
        }

        public int _26_WaitForceAdjustedOrEntityCollisionZ_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (entity.ForceAdjusted != 0)
            {
                return 1;
            }

            if (entity.CollidedWithEntityZ != 0)
            {
                return 1;
            }

            return 0;
        }

        public int _27_FacePlayer_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            entity.TargetDir = Helper.DirFromVector(_gameState.PlayerEntity.XPos - entity.XPos, _gameState.PlayerEntity.YPos - entity.YPos);
            return 1;
        }

        public int _28_Flag2On_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            entity.Flags |= 0x8;
            return 1;
        }

        public int _29_Flag2Off_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            entity.Flags &= ~0x8;
            return 1;
        }

        public int _2a_Flag3On_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            entity.Flags |= 0x1;
            return 1;
        }

        public int _2b_Flag3Off_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            entity.Flags &= ~0x1;
            return 1;
        }

        public int _2d_ActivateEntity_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            var loaded = _gameState.ActivateEntity(entity, entityid, 1);
            if (loaded == null)
            {
                throw new Exception("Illigal InitData Number!!");
            }

            return 2;
        }

        public int _2e_Hide_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            var numentities = _gameState.GetEntityFromRefId(entity, entityid);
            for (var dex = 0; dex < numentities; dex++)
            {
                var checkme = _gameState.GetEntityList[dex];
                _gameState.HideEntity(checkme);
            }

            return 2;
        }

        public int _2f_CheckPlayerInput_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var inputid = code[exp + 3];
            var mask = code[exp + 1] | code[exp + 2] << 8;

            if ((_gameState.PlayerInput[inputid] & mask) != 0)
            {
                eventData.LogicResult = 1;
            }
            else
            {
                eventData.LogicResult = 0;
            }

            return 4;
        }

        public int _30_IfFlagOff_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {

            var flagdata = code[exp + 1] + (code[exp + 2] << 8);
            //int flag = (flagdata >> 3) & 0xffc;
            var flag = (flagdata >> 5) & 0x3ff;
            int[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = _gameState.GameFlagsMap;
            }
            else//otherwise its a global flag
            {
                flags = _gameState.GameFlagsGlobal;
            }

            var bittocheck = flagdata & 0x1f;

            //check the bit for this flag
            if ((flags[flag] & (1 << bittocheck)) != 0)
            {
                int jumpoffset = (short)(code[exp + 3] | code[exp + 4] << 8);
                return jumpoffset;
            }

            return 5;
        }

        public int _31_IfFlagOn_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {

            var flagdata = code[exp + 1] + (code[exp + 2] << 8);
            //int flag = (flagdata >> 3) & 0xffc;
            var flag = (flagdata >> 5) & 0x3ff;
            int[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = _gameState.GameFlagsMap;
            }
            else//otherwise its a global flag
            {
                flags = _gameState.GameFlagsGlobal;
            }

            var bittocheck = flagdata & 0x1f;

            //check the bit for this flag
            if ((flags[flag] & (1 << bittocheck)) == 0)
            {
                int jumpoffset = (short)(code[exp + 3] | code[exp + 4] << 8);
                return jumpoffset;
            }

            return 5;
        }

        public int _32_FlagToggle_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (eventData.LogicResult != 0)
            {
                return 3;
            }

            var flagdata = code[exp + 1] + (code[exp + 2] << 8);
            //int flag = (flagdata >> 3) & 0xffc;
            var flag = (flagdata >> 5) & 0x3ff;
            int[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = _gameState.GameFlagsMap;
            }
            else//otherwise its a global flag
            {
                flags = _gameState.GameFlagsGlobal;
            }

            var bittoset = flagdata & 0x1f;

            //toggle the bit for this flag
            flags[flag] ^= 1 << bittoset;//xor, toggles

            return 3;
        }

        public int _33_CheckFlagsOn_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            //do this 4 times
            {
                var flagdata = code[exp + 1] + (code[exp + 2] << 8);
                //int flag = (flagdata >> 3) & 0xffc;
                var flag = (flagdata >> 5) & 0x3ff;
                int[] flags;
                //if the mapflag bit is set
                if ((flagdata & 0x8000) != 0)
                {
                    flags = _gameState.GameFlagsMap;
                }
                else//otherwise its a global flag
                {
                    flags = _gameState.GameFlagsGlobal;
                }

                var bittocheck = flagdata & 0x1f;

                //check the bit for this flag
                if ((flags[flag] & (1 << bittocheck)) == 0)
                {
                    eventData.LogicResult = 0;
                    return 9;
                }
            }

            {
                var flagdata = code[exp + 3] + (code[exp + 4] << 8);
                //int flag = (flagdata >> 3) & 0xffc;
                var flag = (flagdata >> 5) & 0x3ff;
                int[] flags;
                //if the mapflag bit is set
                if ((flagdata & 0x8000) != 0)
                {
                    flags = _gameState.GameFlagsMap;
                }
                else//otherwise its a global flag
                {
                    flags = _gameState.GameFlagsGlobal;
                }

                var bittocheck = flagdata & 0x1f;

                //check the bit for this flag
                if ((flags[flag] & (1 << bittocheck)) == 0)
                {
                    eventData.LogicResult = 0;
                    return 9;
                }
            }

            {
                var flagdata = code[exp + 5] + (code[exp + 6] << 8);
                //int flag = (flagdata >> 3) & 0xffc;
                var flag = (flagdata >> 5) & 0x3ff;
                int[] flags;
                //if the mapflag bit is set
                if ((flagdata & 0x8000) != 0)
                {
                    flags = _gameState.GameFlagsMap;
                }
                else//otherwise its a global flag
                {
                    flags = _gameState.GameFlagsGlobal;
                }

                var bittocheck = flagdata & 0x1f;

                //check the bit for this flag
                if ((flags[flag] & (1 << bittocheck)) == 0)
                {
                    eventData.LogicResult = 0;
                    return 9;
                }
            }

            {
                var flagdata = code[exp + 7] + (code[exp + 8] << 8);
                //int flag = (flagdata >> 3) & 0xffc;
                var flag = (flagdata >> 5) & 0x3ff;
                int[] flags;
                //if the mapflag bit is set
                if ((flagdata & 0x8000) != 0)
                {
                    flags = _gameState.GameFlagsMap;
                }
                else//otherwise its a global flag
                {
                    flags = _gameState.GameFlagsGlobal;
                }

                var bittocheck = flagdata & 0x1f;

                //check the bit for this flag
                if ((flags[flag] & (1 << bittocheck)) == 0)
                {
                    eventData.LogicResult = 0;
                    return 9;
                }
            }


            eventData.LogicResult = 1;//made it through them all
            return 9;
        }

        public int _34_CheckFlagsOff_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            //do this 4 times
            {
                var flagdata = code[exp + 1] + (code[exp + 2] << 8);
                //int flag = (flagdata >> 3) & 0xffc;
                var flag = (flagdata >> 5) & 0x3ff;
                int[] flags;
                //if the mapflag bit is set
                if ((flagdata & 0x8000) != 0)
                {
                    flags = _gameState.GameFlagsMap;
                }
                else//otherwise its a global flag
                {
                    flags = _gameState.GameFlagsGlobal;
                }

                var bittocheck = flagdata & 0x1f;

                //check the bit for this flag
                if ((flags[flag] & (1 << bittocheck)) != 0)
                {
                    eventData.LogicResult = 0;
                    return 9;
                }
            }

            {
                var flagdata = code[exp + 3] + (code[exp + 4] << 8);
                //int flag = (flagdata >> 3) & 0xffc;
                var flag = (flagdata >> 5) & 0x3ff;
                int[] flags;
                //if the mapflag bit is set
                if ((flagdata & 0x8000) != 0)
                {
                    flags = _gameState.GameFlagsMap;
                }
                else//otherwise its a global flag
                {
                    flags = _gameState.GameFlagsGlobal;
                }

                var bittocheck = flagdata & 0x1f;

                //check the bit for this flag
                if ((flags[flag] & (1 << bittocheck)) != 0)
                {
                    eventData.LogicResult = 0;
                    return 9;
                }
            }

            {
                var flagdata = code[exp + 5] + (code[exp + 6] << 8);
                //int flag = (flagdata >> 3) & 0xffc;
                var flag = (flagdata >> 5) & 0x3ff;
                int[] flags;
                //if the mapflag bit is set
                if ((flagdata & 0x8000) != 0)
                {
                    flags = _gameState.GameFlagsMap;
                }
                else//otherwise its a global flag
                {
                    flags = _gameState.GameFlagsGlobal;
                }

                var bittocheck = flagdata & 0x1f;

                //check the bit for this flag
                if ((flags[flag] & (1 << bittocheck)) != 0)
                {
                    eventData.LogicResult = 0;
                    return 9;
                }
            }

            {
                var flagdata = code[exp + 7] + (code[exp + 8] << 8);
                //int flag = (flagdata >> 3) & 0xffc;
                var flag = (flagdata >> 5) & 0x3ff;
                int[] flags;
                //if the mapflag bit is set
                if ((flagdata & 0x8000) != 0)
                {
                    flags = _gameState.GameFlagsMap;
                }
                else//otherwise its a global flag
                {
                    flags = _gameState.GameFlagsGlobal;
                }

                var bittocheck = flagdata & 0x1f;

                //check the bit for this flag
                if ((flags[flag] & (1 << bittocheck)) != 0)
                {
                    eventData.LogicResult = 0;
                    return 9;
                }
            }


            eventData.LogicResult = 1;//made it through them all
            return 9;
        }


        //blocks until flag is off
        public int _35_UntilFlagOff_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {

            var flagdata = code[exp + 1] + (code[exp + 2] << 8);
            //int flag = (flagdata >> 3) & 0xffc;
            var flag = (flagdata >> 5) & 0x3ff;
            int[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = _gameState.GameFlagsMap;
            }
            else//otherwise its a global flag
            {
                flags = _gameState.GameFlagsGlobal;
            }

            var bittocheck = flagdata & 0x1f;

            //check the bit for this flag
            if ((flags[flag] & (1 << bittocheck)) == 0)
            {
                return 3;
            }

            return 0;
        }

        //blocks until flag is on
        public int _36_UntilFlagOn_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {

            var flagdata = code[exp + 1] + (code[exp + 2] << 8);
            //int flag = (flagdata >> 3) & 0xffc;
            var flag = (flagdata >> 5) & 0x3ff;
            int[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = _gameState.GameFlagsMap;
            }
            else//otherwise its a global flag
            {
                flags = _gameState.GameFlagsGlobal;
            }

            var bittocheck = flagdata & 0x1f;

            //check the bit for this flag
            if ((flags[flag] & (1 << bittocheck)) != 0)
            {
                return 3;
            }

            return 0;
        }


        //block until specified number of ticks
        public int _37_Wait_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (exp != eventData.Tick)
            {
                eventData.Tick = exp;
                eventData.Variables[0] = 0;
                return 0;
            }

            eventData.Variables[0]++;

            var towait = code[exp + 1];
            if (eventData.Variables[0] >= towait)
            {
                return 2;
            }

            return 0;
        }

        public int _3b_CheckPlayerInArea_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            int x1 = code[exp + 1];
            int x2 = code[exp + 2];
            int y1 = code[exp + 3];
            int y2 = code[exp + 4];
            int z1 = code[exp + 5];
            int z2 = code[exp + 6];

            var checkme = _gameState.PlayerEntity;
            if (checkme.XTile >= x1 && checkme.XTile <= x2
                && checkme.YTile >= y1 && checkme.YTile <= y2
                && checkme.ZTile >= z1 && checkme.ZTile <= z2)
            {
                eventData.LogicResult = 1;
                return 7;
            }


            eventData.LogicResult = 0;

            return 7;
        }



        public int _40_SetProgramIndex_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var programid = code[exp + 1];
            var indexval = code[exp + 2];

            //somevariable = 1;
            _gameState.EventProgsSet = 1;

            entity.ProgramIndexes[programid] = indexval;

            return 3;
        }

        public int _41_SetSpriteProgramIndex_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var programid = code[exp + 1];
            var indexval = code[exp + 2];

            entity.SpriteProgramIndexes[programid] = indexval;

            return 3;
        }

        public int _45_Flag4Off_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            entity.Flags &= ~0x2000;
            return 1;
        }

        public int _46_Flag4On_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            entity.Flags |= 0x2000;
            return 1;
        }

        public int _49_Restart_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            return eventData.Exp - eventData.Sp;
        }

        public int _4a_IfTrueRestart_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (eventData.LogicResult != 0)
            {
                return eventData.Exp - eventData.Sp;
            }

            return 1;
        }

        public int _4b_IfFalseRestart_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (eventData.LogicResult == 0)
            {
                return eventData.Exp - eventData.Sp;
            }

            return 1;
        }

        public int _54_SetWalkable_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            int tilex = code[exp + 1];
            int tiley = code[exp + 2];
            if (tilex < 0)
            {
                tilex = 0;
            }
            else if (tilex > 0x33)
            {
                tilex = 0x33;
            }

            if (tiley < 0)
            {
                tiley = 0;
            }

            if (tiley > 0x3b)
            {
                tiley = 0x3b;
            }

            var walkabilitybits = code[exp + 3];
            var groundpropertybits = code[exp + 4];
            var tile = _gameState.GameMap.Map.MapTiles[tilex + tiley * 52];

            tile.Walkability |= walkabilitybits;
            tile.GroundProperty |= groundpropertybits;

            return 5;
        }

        public int _55_SetNonWalkable_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            int tilex = code[exp + 1];
            int tiley = code[exp + 2];
            if (tilex < 0)
            {
                tilex = 0;
            }
            else if (tilex > 0x33)
            {
                tilex = 0x33;
            }

            if (tiley < 0)
            {
                tiley = 0;
            }

            if (tiley > 0x3b)
            {
                tiley = 0x3b;
            }

            var walkabilitybits = code[exp + 3];
            var groundpropertybits = code[exp + 4];
            var tile = _gameState.GameMap.Map.MapTiles[tilex + tiley * 52];

            tile.Walkability &= (byte)~walkabilitybits;
            tile.GroundProperty &= (byte)~groundpropertybits;

            return 5;
        }

        public int _58_DirectionalBranch_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var dir = entity.CurrentFrame;
            var jumpoffset = (short)(code[exp + entity.CurrentFrame * 2 + 1] | code[exp + entity.CurrentFrame * 2 + 2]);
            return jumpoffset;
        }

        public int _59_SetEntityAnim_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            int animid = code[exp + 2];
            var numentities = _gameState.GetEntityFromRefId(entity, entityid);
            for (var dex = 0; dex < numentities; dex++)
            {
                var dome = _gameState.GetEntityList[dex];
                dome.TargetAnim = animid;
            }

            return 3;
        }

        public int _5a_TurnEntity_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            var turncode = code[exp + 2];

            var numentities = _gameState.GetEntityFromRefId(entity, entityid);
            for (var dex = 0; dex < numentities; dex++)
            {
                var dome = _gameState.GetEntityList[dex];
                dome.TargetDir = _gameState.TurnEntity(entity, turncode);
            }

            return 3;
        }

        public int _5b_TurnEntityWithAnim_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            int animid = code[exp + 2];
            var turncode = code[exp + 3];

            var numentities = _gameState.GetEntityFromRefId(entity, entityid);
            for (var dex = 0; dex < numentities; dex++)
            {
                var dome = _gameState.GetEntityList[dex];
                dome.TargetAnim = animid;
                dome.TargetDir = _gameState.TurnEntity(entity, turncode);
            }

            return 4;
        }

        public int _62_EntityFlagsOn_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            var flagbits = code[exp + 2] + (code[exp + 3] << 8);

            var numentities = _gameState.GetEntityFromRefId(entity, entityid);
            for (var dex = 0; dex < numentities; dex++)
            {
                var dome = _gameState.GetEntityList[dex];
                dome.Flags |= flagbits;
            }

            return 4;
        }

        public int _63_EntityFlagsOff_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            var flagbits = code[exp + 2] + (code[exp + 3] << 8);

            var numentities = _gameState.GetEntityFromRefId(entity, entityid);
            for (var dex = 0; dex < numentities; dex++)
            {
                var dome = _gameState.GetEntityList[dex];
                dome.Flags &= ~flagbits;
            }

            return 4;
        }

        public int _64_SetEntityPos_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            var x = (code[exp + 2] + (code[exp + 3] << 8)) << 16;
            var y = (code[exp + 4] + (code[exp + 5] << 8)) << 16;
            var z = (code[exp + 6] + (code[exp + 7] << 8)) << 16;

            var numentities = _gameState.GetEntityFromRefId(entity, entityid);
            for (var dex = 0; dex < numentities; dex++)
            {
                var dome = _gameState.GetEntityList[dex];
                dome.XPos = x;
                dome.YPos = y;
                dome.ZPos = z + 1;
            }

            return 8;
        }

        public int _65_MoveEntityPos_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            var x = (code[exp + 2] + (code[exp + 3] << 8)) << 16;
            var y = (code[exp + 4] + (code[exp + 5] << 8)) << 16;
            var z = (code[exp + 6] + (code[exp + 7] << 8)) << 16;

            var numentities = _gameState.GetEntityFromRefId(entity, entityid);
            for (var dex = 0; dex < numentities; dex++)
            {
                var dome = _gameState.GetEntityList[dex];
                dome.XPos += x;
                dome.YPos += y;
                dome.ZPos += z;
            }

            return 8;
        }

        public int _67_CamFollowEntity_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];

            var numentities = _gameState.GetEntityFromRefId(entity, entityid);

            _gameState.CamFollowEntity = _gameState.GetEntityList[0];

            return 2;
        }

        public int _70_Check144_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            eventData.LogicResult = entity._144;

            return 2;
        }

        public int _90_CreateEffect_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var effectid = code[1];
            _gameState.CreateEffect_MapType(effectid, true);
            return 2;
        }

        public int _91_DisableEffect_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var effectid = code[1];
            foreach (var effect in _gameState.SpriteEffects)
            {
                if (effect.Status != 0 && effect.MapEffectId == effectid)
                {
                    effect.Status = 0;
                }
            }

            return 2;
        }

        public int _92_SetEffectAnim_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var effectid = code[1];
            var animid = code[2];
            foreach (var effect in _gameState.SpriteEffects)
            {
                if (effect.Status != 0 && effect.MapEffectId == effectid)
                {
                    effect.TargetAnim = animid;
                }
            }

            return 3;
        }

        public int _93_SetEffectPos_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var effectid = code[1];
            var x = (code[2] | code[3] << 8) << 16;
            var y = (code[4] | code[5] << 8) << 16;
            var z = (code[6] | code[7] << 8) << 16;
            foreach (var effect in _gameState.SpriteEffects)
            {
                if (effect.Status != 0 && effect.MapEffectId == effectid)
                {
                    effect.X = x;
                    effect.Y = y;
                    effect.Z = z;
                }
            }

            return 8;
        }

        public int _94_SetEffectForces_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var effectid = code[1];
            var x = (code[2] | code[3] << 8) << 16;
            var y = (code[4] | code[5] << 8) << 16;
            var z = (code[6] | code[7] << 8) << 16;
            foreach (var effect in _gameState.SpriteEffects)
            {
                if (effect.Status != 0 && effect.MapEffectId == effectid)
                {
                    effect.XForce = x;
                    effect.YForce = y;
                    effect.ZForce = z;
                }
            }

            return 8;
        }

        public int _a0_AdjustEffectPos_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var effectid = code[1];
            var x = (code[2] | code[3] << 8) << 16;
            var y = (code[4] | code[5] << 8) << 16;
            var z = (code[6] | code[7] << 8) << 16;
            foreach (var effect in _gameState.SpriteEffects)
            {
                if (effect.Status != 0 && effect.MapEffectId == effectid)
                {
                    effect.X += x;
                    effect.Y += y;
                    effect.Z += z;
                }
            }

            return 8;
        }

        public int _a1_AdjustEffectPosWithEntity_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var effectid = code[1];
            var entityid = code[2];

            var numentities = _gameState.GetEntityFromRefId(entity, entityid);
            if (numentities == 0)
            {
                return 9;
            }

            var refentity = _gameState.GetEntityList[0];

            var x = (code[3] | code[4] << 8) << 16;
            var y = (code[5] | code[6] << 8) << 16;
            var z = (code[7] | code[8] << 8) << 16;

            x += refentity.XPos;
            y += refentity.YPos;
            z += refentity.ZPos;

            foreach (var effect in _gameState.SpriteEffects)
            {
                if (effect.Status != 0 && effect.MapEffectId == effectid)
                {
                    effect.X = x;
                    effect.Y = y;
                    effect.Z = z;
                }
            }

            return 9;
        }

        public int _a2_CreateEffectWithPos_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var effectid = code[1];
            var effect = _gameState.CreateEffect_MapType(effectid, true);
            if (effect == null)
            {
                return 8;
            }

            var x = (code[2] | code[3] << 8) << 16;
            var y = (code[4] | code[5] << 8) << 16;
            var z = (code[6] | code[7] << 8) << 16;

            effect.X = x;
            effect.Y = y;
            effect.Z = z;

            return 8;
        }

        public int _a3_CreateEffectWithEntityPos_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var effectid = code[1];
            var entityid = code[2];
            var numentities = _gameState.GetEntityFromRefId(entity, entityid);
            if (numentities == 0)
            {
                return 9;
            }

            var effect = _gameState.CreateEffect_MapType(effectid, true);
            if (effect == null)
            {
                return 9;
            }

            var x = (code[3] | code[4] << 8) << 16;
            var y = (code[5] | code[6] << 8) << 16;
            var z = (code[7] | code[8] << 8) << 16;

            effect.X = entity.XPos + x;
            effect.Y = entity.YPos + y;
            effect.Z = entity.ZPos + z;

            return 9;
        }


    }

    public delegate int ScriptEventHandler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] eventcode);



    public static class Helper
    {
        public const int ProgramALoad = 0;
        public const int ProgramBMap = 1;
        public const int ProgramCTick = 2;
        public const int ProgramDTouch = 3;
        public const int ProgramEDeactivate = 4;
        public const int ProgramFInteract = 5;

        public static int SignExtendWord(int i)
        {
            if ((i & 0x8000) == 0)
            {
                return 0x0000FFFF & i;
            }

            return (int)(0xFFFF0000 | i);
        }

        public static int DirFromVector(int x, int y)
        {
            var flipper = 0;
            if (y < 1)
            {
                flipper = 2;
            }

            if (x < 0)
            {
                flipper++;
            }

            if (x < 0)
            {
                x = -x;
            }

            if (y < 0)
            {
                y = -y;
            }

            var greatest = x;
            if (x < y)
            {
                y = greatest;
            }

            var div = 0;
            var val = _divTable[div];
            if (val < greatest)
            {
                do
                {
                    div++;
                    val = _divTable[div];
                } while (val < greatest);
            }
            x = x >> div;
            y = y >> div;

            var direction = (int)_directionTable[y * 16 + x];

            var ret = direction;
            if (flipper == 1)
            {
                ret = 8 - direction;
            }
            else if (flipper == 2)
            {
                ret = 0x18 - direction;
            }
            else if (flipper == 3)
            {
                ret = 8 + direction;
            }
            else if (flipper == 0)
            {
                ret = 0x18 + direction;
            }

            return ret & 0x1f;
        }

        public static readonly int[] Anim24Table = new int[]{
0x00000000,//0x00
0x00000003,//0x01
0x00000001,//0x02
0x00000004,//0x03
0x00000000,//0x04
};

        public static readonly int[] XForceTable = new int[]{
0x00000000,//0x00
0x00000000,//0x01
0x00000000,//0x02
0x00000000,//0x03
unchecked((int)0xffff1000),//0x04
unchecked((int)0xffff1000),//0x05
unchecked((int)0xffff1000),//0x06
unchecked((int)0xfff10000),//0x07
0x0000f000,//0x08
0x0000f000,//0x09
0x0000f000,//0x0a
0x000f0000,//0x0b
0x00000000,//0x0c
0x00000000,//0x0d
0x00000000,//0x0e
0x00000000,//0x0f
};

        public static readonly int[] YForceTable = new int[]{
0x00000000,//0x00
unchecked((int)0xffff6000),//0x01
0x0000a000,//0x02
0x00000000,//0x03
0x00000000,//0x04
unchecked((int)0xffff6000),//0x05
0x0000a000,//0x06
0x00000000,//0x07
0x00000000,//0x08
unchecked((int)0xffff6000),//0x09
0x0000a000,//0x0a
0x00000000,//0x0b
0x00000000,//0x0c
unchecked((int)0xffff6000),//0x0d
0x0000a000,//0x0e
0x00000000,//0x0f
};

        public static readonly short[] DirVectorsX = new short[]{
0x0,unchecked((short)0xff6a),unchecked((short)0xfeda),unchecked((short)0xfe5a),unchecked((short)0xfde1),unchecked((short)0xfd81),unchecked((short)0xfd3a),unchecked((short)0xfd0f),unchecked((short)0xfd00),unchecked((short)0xfd0f),unchecked((short)0xfd3a),unchecked((short)0xfd81),unchecked((short)0xfde1),unchecked((short)0xfe5a),unchecked((short)0xfeda),unchecked((short)0xff6a),
0x0,0x96,0x126,0x1a6,0x21f,0x27f,0x2c6,0x2f1,0x300,0x2f1,0x2c6,0x27f,0x21f,0x1a6,0x126,0x96};

        public static readonly short[] DirVectorsY = new short[]{
0x200,0x1f6,0x1d9,0x1aa,0x16a,0x11c,0xc4,0x64,0x0,unchecked((short)0xff9c),unchecked((short)0xff3c),unchecked((short)0xfee4),unchecked((short)0xfe96),unchecked((short)0xfe56),unchecked((short)0xfe27),unchecked((short)0xfe0a),
unchecked((short)0xfe00),unchecked((short)0xfe0a),unchecked((short)0xfe27),unchecked((short)0xfe56),unchecked((short)0xfe96),unchecked((short)0xfee4),unchecked((short)0xff3c),unchecked((short)0xff9c),0x0,0x64,0xc4,0x11c,0x16a,0x1aa,0x1d9,0x1f6};

        public static readonly short[] CardinalDirTable = new short[] { 0, 0x10, 0x08, 0x18 };

        private static short[] _directionTable = new short[]{
0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,0x0,
0x8,0x4,0x2,0x2,0x1,0x1,0x1,0x1,0x1,0x1,0x1,0x0,0x0,0x0,0x0,0x0,
0x8,0x6,0x4,0x3,0x2,0x2,0x2,0x1,0x1,0x1,0x1,0x1,0x1,0x1,0x1,0x1,
0x8,0x6,0x5,0x4,0x3,0x3,0x2,0x2,0x2,0x2,0x1,0x1,0x1,0x1,0x1,0x1,
0x8,0x7,0x6,0x5,0x4,0x3,0x3,0x3,0x2,0x2,0x2,0x2,0x2,0x2,0x1,0x1,
0x8,0x7,0x6,0x5,0x5,0x4,0x4,0x3,0x3,0x3,0x2,0x2,0x2,0x2,0x2,0x2,
0x8,0x7,0x6,0x6,0x5,0x4,0x4,0x4,0x3,0x3,0x3,0x3,0x2,0x2,0x2,0x2,
0x8,0x7,0x7,0x6,0x5,0x5,0x4,0x4,0x4,0x3,0x3,0x3,0x3,0x3,0x2,0x2,
0x8,0x7,0x7,0x6,0x6,0x5,0x5,0x4,0x4,0x4,0x3,0x3,0x3,0x3,0x3,0x2,
0x8,0x7,0x7,0x6,0x6,0x5,0x5,0x5,0x4,0x4,0x4,0x3,0x3,0x3,0x3,0x3,
0x8,0x7,0x7,0x7,0x6,0x6,0x5,0x5,0x5,0x4,0x4,0x4,0x4,0x3,0x3,0x3,
0x8,0x8,0x7,0x7,0x6,0x6,0x5,0x5,0x5,0x5,0x4,0x4,0x4,0x4,0x3,0x3,
0x8,0x8,0x7,0x7,0x6,0x6,0x6,0x5,0x5,0x5,0x4,0x4,0x4,0x4,0x4,0x3,
0x8,0x8,0x7,0x7,0x6,0x6,0x6,0x5,0x5,0x5,0x5,0x4,0x4,0x4,0x4,0x4,
0x8,0x8,0x7,0x7,0x7,0x6,0x6,0x6,0x5,0x5,0x5,0x5,0x4,0x4,0x4,0x4,
0x8,0x8,0x7,0x7,0x7,0x6,0x6,0x6,0x6,0x5,0x5,0x5,0x5,0x4,0x4,0x4,
};

        private static uint[] _divTable = new uint[]{
0x0000000f,
0x0000001f,
0x0000003f,
0x0000007f,
0x000000ff,
0x000001ff,
0x000003ff,
0x000007ff,
0x00000fff,
0x00001fff,
0x00003fff,
0x00007fff,
0x0000ffff,
0x0001ffff,
0x0003ffff,
0x0007ffff,
0x000fffff,
0x001fffff,
0x003fffff,
0x007fffff,
0x00ffffff,
0x01ffffff,
0x03ffffff,
0x07ffffff,
0x0fffffff,
0x1fffffff,
0x3fffffff,
0x7fffffff,
0xffffffff,
};

    }
}
