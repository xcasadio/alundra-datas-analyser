using GraphicsTools.Alundra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsTools.Alundra
{
    public class EventHandlers
    {
        GameState gameState;
        Dictionary<int, ScriptEventHandler> Handlers = new Dictionary<int, ScriptEventHandler>();
        public SpriteEventHandlers SpriteHandlers;
        public EventHandlers(GameState gameState)
        {
            this.gameState = gameState;

            SpriteHandlers = new SpriteEventHandlers(gameState);
            //add handlers
            for (int dex = 0; dex <= 0xff; dex++)
                Handlers.Add(dex, __Unknown_Handler);

            Handlers[0x2] = _02_Goto_Handler;
            Handlers[0x3] = _03_BranchIfTrue_Handler;
            Handlers[0x4] = _04_BranchIfFalse_Handler;
            Handlers[0x5] = _05_FlagOn_Handler;
            Handlers[0x6] = _06_FlagOff_Handler;
            Handlers[0x7] = _07_CheckEntityInArea_Handler;
            Handlers[0x8] = _08_Turn_Handler;
            Handlers[0x9] = _09_SetDir_Handler;
            Handlers[0xa] = _0a_Reverse_Handler;
            Handlers[0xc] = _0c_SetRandomDir_Handler;
            Handlers[0xd] = _0d_Dialog_Handler;
            Handlers[0x10] = _10_LoseControl_Handler;
            Handlers[0x11] = _11_GainControl_Handler;
            Handlers[0x12] = _12_PlaySound1_Handler;
            Handlers[0x15] = _15_ResetZPos_Handler;
            Handlers[0x16] = _16_GravityOn_Handler;
            Handlers[0x17] = _17_GravityOff_Handler;
            Handlers[0x19] = _19_Deactivate_Handler;
            Handlers[0x1a] = _1a_SetAnim_Handler;
            Handlers[0x1b] = _1b_Fly_Handler;
            Handlers[0x1c] = _1c_WaitAnim_Handler;
            Handlers[0x1d] = _1d_WaitAnim2_Handler;
            Handlers[0x1e] = _1e_WaitWalk_Handler;
            Handlers[0x1f] = _1f_WaitWalk2_Handler;
            Handlers[0x24] = _24_WaitForceAdjusted_Handler;
            Handlers[0x25] = _25_WaitEntityCollisionZOr144_Handler;
            Handlers[0x26] = _26_WaitForceAdjustedOrEntityCollisionZ_Handler;
            Handlers[0x27] = _27_FacePlayer_Handler;
            Handlers[0x28] = _28_Flag2On_Handler;
            Handlers[0x29] = _29_Flag2Off_Handler;
            Handlers[0x2a] = _2a_Flag3On_Handler;
            Handlers[0x2b] = _2b_Flag3Off_Handler;
            Handlers[0x2d] = _2d_ActivateEntity_Handler;
            Handlers[0x2e] = _2e_Hide_Handler;
            Handlers[0x2f] = _2f_CheckPlayerInput_Handler;
            Handlers[0x30] = _30_IfFlagOff_Handler;
            Handlers[0x31] = _31_IfFlagOn_Handler;
            Handlers[0x32] = _32_FlagToggle_Handler;
            Handlers[0x33] = _33_CheckFlagsOn_Handler;
            Handlers[0x34] = _34_CheckFlagsOff_Handler;
            Handlers[0x35] = _35_UntilFlagOff_Handler;
            Handlers[0x36] = _36_UntilFlagOn_Handler;
            Handlers[0x37] = _37_Wait_Handler;
            Handlers[0x3b] = _3b_CheckPlayerInArea_Handler;
            Handlers[0x40] = _40_SetProgramIndex_Handler;
            Handlers[0x41] = _41_SetSpriteProgramIndex_Handler;
            Handlers[0x45] = _45_Flag4Off_Handler;
            Handlers[0x46] = _46_Flag4On_Handler;
            Handlers[0x49] = _49_Restart_Handler;
            Handlers[0x4a] = _4a_IfTrueRestart_Handler;
            Handlers[0x4b] = _4b_IfFalseRestart_Handler;
            Handlers[0x54] = _54_SetWalkable_Handler;
            Handlers[0x55] = _55_SetNonWalkable_Handler;
            Handlers[0x58] = _58_DirectionalBranch_Handler;
            Handlers[0x59] = _59_SetEntityAnim_Handler;
            Handlers[0x5a] = _5a_TurnEntity_Handler;
            Handlers[0x5b] = _5b_TurnEntityWithAnim_Handler;
            Handlers[0x62] = _62_EntityFlagsOn_Handler;
            Handlers[0x63] = _63_EntityFlagsOff_Handler;
            Handlers[0x64] = _64_SetEntityPos_Handler;
            Handlers[0x65] = _65_MoveEntityPos_Handler;
            Handlers[0x67] = _67_CamFollowEntity_Handler;
            Handlers[0x70] = _70_Check144_Handler;

        }

        public void RunEntityEventScripts(SpriteInstance entity, int eventprogramtype)
        {
            EventProgramState eventdata;
            if (eventprogramtype < 6)
            {
                switch (eventprogramtype)
                {
                    case Helper.PROGRAM_B_MAP:
                        eventdata = entity.eventdata;
                        if (eventdata.exp == 0
                            || eventdata.sp == 0)
                        {
                            InitEventData(entity, eventprogramtype, eventdata);
                        }
                        break;
                    case Helper.PROGRAM_C_TICK:
                        eventdata = entity.eventdata;
                        if (eventdata.exp == 0
                            || eventdata.sp == 0)
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
                        if (Helper.PROGRAM_F_INTERACT == 5)//have to do it here because switch fallthrough isnt allowed in c#
                        {
                            gameState.PlayerEntity.YForceStep = 0;
                            gameState.PlayerEntity.XForceStep = 0;
                            gameState.PlayerEntity.XForce = 0;
                            gameState.PlayerEntity.YForce = 0;
                        }

                        eventdata = gameState.GlobalEventData;

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

                gameState.ActiveEventProgramType = eventprogramtype;
                gameState.ActiveEventCode = -1;
                gameState.EventProgsSet = 0;
                gameState.ActiveEventProgIndex = entity.Program_Indexes[eventprogramtype];
                gameState.ActiveEntityRefId = entity.EntityRefId;

                var code = SpriteInfoEventCodes.Code;
                var evtcode = code[eventdata.exp];

                if (evtcode == 0xff)
                    break;

                if (evtcode == 0)
                {
                    eventdata.evttickprog = 0;
                    eventdata.exp++;
                    break;
                }

                var func = Handlers[evtcode];

                gameState.PrevEventCode = gameState.ActiveEventCode;
                gameState.ActiveEventCode = evtcode;

                int advanced = func(entity.EntitySelf, entity, eventdata.exp, eventdata, code);

                if (gameState.EventProgsSet != 0)
                {
                    gameState.EventProgsSet = 1;
                    if (entity != entity.EntitySelf)
                    {
                        entity.EntitySelf.eventdata.sp = 0;
                        entity.EntitySelf.eventdata.exp = 0;
                    }
                    else
                        sameasself = true;
                }

                if (advanced == 0)
                    break;

                eventdata.evttickprog = 0;
                eventdata.exp += advanced;
            } while (true);

            if (sameasself)
            {
                eventdata.sp = 0;
                eventdata.exp = 0;
            }

        }

        private void InitEventData(SpriteInstance entity, int eventprogramtype, EventProgramState eventdata)
        {
            var codeindex = entity.Program_Indexes[eventprogramtype];
            var si = gameState.global.spriteinfo;
            var mod = 0;
            if ((codeindex & 0x80) != 0)
            {
                si = gameState.gameMap.spriteinfo;
                mod = 1024 * 512;
            }

            var sp = si.eventcodes.eventcodestable[eventprogramtype][codeindex & 0x7f] + mod;
            eventdata.sp = sp;
            eventdata.exp = sp;

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
            short offset = (short)(code[exp + 1] + (code[exp + 2] << 8));

            return offset;
        }

        public int _03_BranchIfTrue_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (eventData.logicResult == 0)
                return 3;

            short offset = (short)(code[exp + 1] + (code[exp + 2] << 8));

            return offset;
        }

        public int _04_BranchIfFalse_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (eventData.logicResult != 0)
                return 3;

            short offset = (short)(code[exp + 1] + (code[exp + 2] << 8));

            return offset;
        }

        public int _05_FlagOn_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (eventData.logicResult != 0)
                return 3;

            int flagdata = (code[exp + 1] + (code[exp + 2] << 8));
            //int flag = (flagdata >> 3) & 0xffc;
            int flag = (flagdata >> 5) & 0x3ff;
            int[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = gameState.GameFlagsMap;
            }
            else//otherwise its a global flag
            {
                flags = gameState.GameFlagsGlobal;
            }

            var bittoset = flagdata & 0x1f;

            //turn on the bit for this flag
            flags[flag] |= 1 << bittoset;

            return 3;
        }

        public int _06_FlagOff_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (eventData.logicResult != 0)
                return 3;

            int flagdata = (code[exp + 1] + (code[exp + 2] << 8));
            //int flag = (flagdata >> 3) & 0xffc;
            int flag = (flagdata >> 5) & 0x3ff;
            int[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = gameState.GameFlagsMap;
            }
            else//otherwise its a global flag
            {
                flags = gameState.GameFlagsGlobal;
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
            int numentities = gameState.GetEntityFromRefId(entity, entityid);
            for (int dex = 0; dex < numentities; dex++)
            {
                var checkme = gameState.GetEntityList[dex];
                if (checkme.XTile >= x1 && checkme.XTile <= x2
                    && checkme.YTile >= y1 && checkme.YTile <= y2
                    && checkme.ZTile >= z1 && checkme.ZTile <= z2)
                {
                    eventData.logicResult = 1;
                    return 8;
                }
            }

            eventData.logicResult = 0;

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
            if (exp != eventData.evttickprog)
            {
                eventData.evttickprog = exp;
                eventData.evtvars[0] = entity.XPos;
                eventData.evtvars[1] = entity.YPos;
                return 0;
            }
            int difx = eventData.evtvars[0] - entity.XPos;
            int dify = eventData.evtvars[1] - entity.YPos;
            if (difx < 0)
                difx = -difx;
            if (dify < 0)
                dify = -dify;
            int distance = code[exp + 2] + (code[exp + 3] << 8);

            if (difx >> 16 >= distance)
                return 4;//done
            if (dify >> 16 >= distance)
                return 4;//done

            return 0;//keep blocking
        }

        public int _0c_SetRandomDir_Handler(SpriteInstance entity, SpriteInstance entityself, int exp, EventProgramState eventData, byte[] code)
        {
            int i = gameState.Seed;
            int val1 = (int)(i * 0x7d2b89dd);
            int val2 = (int)(0xe06a02e7 + val1);
            int val3 = (int)(((long)val2 * 4) >> 32);
            var dir = Helper.CardinalDirTable[val3];//val3 here is a number between 0 and 3
            gameState.Seed = val2;
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
            gameState.PlayerControlSetting |= 0x4;
            return 1;
        }

        public int _11_GainControl_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            gameState.PlayerControlSetting &= ~0x4;
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

            entity.ZPos = (entity.EntityRecord.height * 8 - entity.ZMod) << 16;
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
            int force = code[exp + 1] + (code[exp + 2] << 8);
            force = force << 16;//sign extend
            force = force >> 8;//get it to the correct multiple
            entity.ZForce = force;
            return 2;
        }

        public int _1c_WaitAnim_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (exp != eventData.evttickprog)
            {
                eventData.evttickprog = exp;
                eventData.evtvars[0] = 0;
                entity.AnimCompleteCounter = 0;
                return 0;
            }

            if (entity.WierdNextFrameDelayFlag != 0)
            {
                entity.CurAnim = ~entity.TargetAnim;
            }

            if (entity.WierdNextFrameDelayFlag != 0 || entity.AnimCompleteCounter != 0)
            {
                eventData.evtvars[0]++;
                entity.AnimCompleteCounter = 0;
            }


            var towait = code[exp + 1];
            if (eventData.evtvars[0] >= towait)
                return 2;
            else
                return 0;
        }

        //collision ends it
        public int _1d_WaitAnim2_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var ret = _1c_WaitAnim_Handler(entity, entityself, exp, eventData, code);

            if (ret != 0)
                return ret;

            if (entity.ForceAdjusted != 0)
                return 2;

            return 0;
        }

        //wait until they have walked a certain distance, collision pauses the walk
        public int _1e_WaitWalk_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (exp != eventData.evttickprog)
            {
                eventData.evttickprog = exp;
                eventData.evtvars[0] = entity.XPos;
                eventData.evtvars[1] = entity.YPos;
                return 0;
            }

            var x = Math.Abs(eventData.evtvars[0] - entity.XPos) >> 16;
            var y = Math.Abs(eventData.evtvars[1] - entity.YPos) >> 16;

            int distance = code[exp + 1] | (code[exp + 2] << 8);

            if (x >= distance || y >= distance)
                return 3;

            return 0;
        }

        //wait until they have walked a certain distance, collision ends the walk
        public int _1f_WaitWalk2_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var ret = _1e_WaitWalk_Handler(entity, entityself, exp, eventData, code);

            if (ret != 0)
                return ret;

            if (entity.ForceAdjusted != 0)
                return 3;

            return 0;
        }

        //wait until some force acts on the entity, like it bumps into something
        public int _24_WaitForceAdjusted_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (entity.ForceAdjusted > 0)
                return 1;

            return 0;
        }

        public int _25_WaitEntityCollisionZOr144_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (entity.CollidedWithEntityZ != 0)
                return 1;
            if (entity._144 != 0)
                return 1;

            return 0;
        }

        public int _26_WaitForceAdjustedOrEntityCollisionZ_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (entity.ForceAdjusted != 0)
                return 1;
            if (entity.CollidedWithEntityZ != 0)
                return 1;

            return 0;
        }

        public int _27_FacePlayer_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            entity.TargetDir = Helper.DirFromVector(gameState.PlayerEntity.XPos - entity.XPos, gameState.PlayerEntity.YPos - entity.YPos);
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
            var loaded = gameState.ActivateEntity(entity, entityid, 1);
            if (loaded == null)
                throw new Exception("Illigal InitData Number!!");
            return 2;
        }

        public int _2e_Hide_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            int numentities = gameState.GetEntityFromRefId(entity, entityid);
            for (int dex = 0; dex < numentities; dex++)
            {
                var checkme = gameState.GetEntityList[dex];
                gameState.HideEntity(checkme);
            }

            return 2;
        }

        public int _2f_CheckPlayerInput_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var inputid = code[exp + 3];
            var mask = code[exp + 1] | code[exp + 2] << 8;

            if ((gameState.PlayerInput[inputid] & mask) != 0)
                eventData.logicResult = 1;
            else
                eventData.logicResult = 0;

            return 4;
        }

        public int _30_IfFlagOff_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {

            int flagdata = (code[exp + 1] + (code[exp + 2] << 8));
            //int flag = (flagdata >> 3) & 0xffc;
            int flag = (flagdata >> 5) & 0x3ff;
            int[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = gameState.GameFlagsMap;
            }
            else//otherwise its a global flag
            {
                flags = gameState.GameFlagsGlobal;
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

            int flagdata = (code[exp + 1] + (code[exp + 2] << 8));
            //int flag = (flagdata >> 3) & 0xffc;
            int flag = (flagdata >> 5) & 0x3ff;
            int[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = gameState.GameFlagsMap;
            }
            else//otherwise its a global flag
            {
                flags = gameState.GameFlagsGlobal;
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
            if (eventData.logicResult != 0)
                return 3;

            int flagdata = (code[exp + 1] + (code[exp + 2] << 8));
            //int flag = (flagdata >> 3) & 0xffc;
            int flag = (flagdata >> 5) & 0x3ff;
            int[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = gameState.GameFlagsMap;
            }
            else//otherwise its a global flag
            {
                flags = gameState.GameFlagsGlobal;
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
                int flagdata = (code[exp + 1] + (code[exp + 2] << 8));
                //int flag = (flagdata >> 3) & 0xffc;
                int flag = (flagdata >> 5) & 0x3ff;
                int[] flags;
                //if the mapflag bit is set
                if ((flagdata & 0x8000) != 0)
                {
                    flags = gameState.GameFlagsMap;
                }
                else//otherwise its a global flag
                {
                    flags = gameState.GameFlagsGlobal;
                }

                var bittocheck = flagdata & 0x1f;

                //check the bit for this flag
                if ((flags[flag] & (1 << bittocheck)) == 0)
                {
                    eventData.logicResult = 0;
                    return 9;
                }
            }

            {
                int flagdata = (code[exp + 3] + (code[exp + 4] << 8));
                //int flag = (flagdata >> 3) & 0xffc;
                int flag = (flagdata >> 5) & 0x3ff;
                int[] flags;
                //if the mapflag bit is set
                if ((flagdata & 0x8000) != 0)
                {
                    flags = gameState.GameFlagsMap;
                }
                else//otherwise its a global flag
                {
                    flags = gameState.GameFlagsGlobal;
                }

                var bittocheck = flagdata & 0x1f;

                //check the bit for this flag
                if ((flags[flag] & (1 << bittocheck)) == 0)
                {
                    eventData.logicResult = 0;
                    return 9;
                }
            }

            {
                int flagdata = (code[exp + 5] + (code[exp + 6] << 8));
                //int flag = (flagdata >> 3) & 0xffc;
                int flag = (flagdata >> 5) & 0x3ff;
                int[] flags;
                //if the mapflag bit is set
                if ((flagdata & 0x8000) != 0)
                {
                    flags = gameState.GameFlagsMap;
                }
                else//otherwise its a global flag
                {
                    flags = gameState.GameFlagsGlobal;
                }

                var bittocheck = flagdata & 0x1f;

                //check the bit for this flag
                if ((flags[flag] & (1 << bittocheck)) == 0)
                {
                    eventData.logicResult = 0;
                    return 9;
                }
            }

            {
                int flagdata = (code[exp + 7] + (code[exp + 8] << 8));
                //int flag = (flagdata >> 3) & 0xffc;
                int flag = (flagdata >> 5) & 0x3ff;
                int[] flags;
                //if the mapflag bit is set
                if ((flagdata & 0x8000) != 0)
                {
                    flags = gameState.GameFlagsMap;
                }
                else//otherwise its a global flag
                {
                    flags = gameState.GameFlagsGlobal;
                }

                var bittocheck = flagdata & 0x1f;

                //check the bit for this flag
                if ((flags[flag] & (1 << bittocheck)) == 0)
                {
                    eventData.logicResult = 0;
                    return 9;
                }
            }


            eventData.logicResult = 1;//made it through them all
            return 9;
        }

        public int _34_CheckFlagsOff_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            //do this 4 times
            {
                int flagdata = (code[exp + 1] + (code[exp + 2] << 8));
                //int flag = (flagdata >> 3) & 0xffc;
                int flag = (flagdata >> 5) & 0x3ff;
                int[] flags;
                //if the mapflag bit is set
                if ((flagdata & 0x8000) != 0)
                {
                    flags = gameState.GameFlagsMap;
                }
                else//otherwise its a global flag
                {
                    flags = gameState.GameFlagsGlobal;
                }

                var bittocheck = flagdata & 0x1f;

                //check the bit for this flag
                if ((flags[flag] & (1 << bittocheck)) != 0)
                {
                    eventData.logicResult = 0;
                    return 9;
                }
            }

            {
                int flagdata = (code[exp + 3] + (code[exp + 4] << 8));
                //int flag = (flagdata >> 3) & 0xffc;
                int flag = (flagdata >> 5) & 0x3ff;
                int[] flags;
                //if the mapflag bit is set
                if ((flagdata & 0x8000) != 0)
                {
                    flags = gameState.GameFlagsMap;
                }
                else//otherwise its a global flag
                {
                    flags = gameState.GameFlagsGlobal;
                }

                var bittocheck = flagdata & 0x1f;

                //check the bit for this flag
                if ((flags[flag] & (1 << bittocheck)) != 0)
                {
                    eventData.logicResult = 0;
                    return 9;
                }
            }

            {
                int flagdata = (code[exp + 5] + (code[exp + 6] << 8));
                //int flag = (flagdata >> 3) & 0xffc;
                int flag = (flagdata >> 5) & 0x3ff;
                int[] flags;
                //if the mapflag bit is set
                if ((flagdata & 0x8000) != 0)
                {
                    flags = gameState.GameFlagsMap;
                }
                else//otherwise its a global flag
                {
                    flags = gameState.GameFlagsGlobal;
                }

                var bittocheck = flagdata & 0x1f;

                //check the bit for this flag
                if ((flags[flag] & (1 << bittocheck)) != 0)
                {
                    eventData.logicResult = 0;
                    return 9;
                }
            }

            {
                int flagdata = (code[exp + 7] + (code[exp + 8] << 8));
                //int flag = (flagdata >> 3) & 0xffc;
                int flag = (flagdata >> 5) & 0x3ff;
                int[] flags;
                //if the mapflag bit is set
                if ((flagdata & 0x8000) != 0)
                {
                    flags = gameState.GameFlagsMap;
                }
                else//otherwise its a global flag
                {
                    flags = gameState.GameFlagsGlobal;
                }

                var bittocheck = flagdata & 0x1f;

                //check the bit for this flag
                if ((flags[flag] & (1 << bittocheck)) != 0)
                {
                    eventData.logicResult = 0;
                    return 9;
                }
            }


            eventData.logicResult = 1;//made it through them all
            return 9;
        }


        //blocks until flag is off
        public int _35_UntilFlagOff_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {

            int flagdata = (code[exp + 1] + (code[exp + 2] << 8));
            //int flag = (flagdata >> 3) & 0xffc;
            int flag = (flagdata >> 5) & 0x3ff;
            int[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = gameState.GameFlagsMap;
            }
            else//otherwise its a global flag
            {
                flags = gameState.GameFlagsGlobal;
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

            int flagdata = (code[exp + 1] + (code[exp + 2] << 8));
            //int flag = (flagdata >> 3) & 0xffc;
            int flag = (flagdata >> 5) & 0x3ff;
            int[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = gameState.GameFlagsMap;
            }
            else//otherwise its a global flag
            {
                flags = gameState.GameFlagsGlobal;
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
            if (exp != eventData.evttickprog)
            {
                eventData.evttickprog = exp;
                eventData.evtvars[0] = 0;
                return 0;
            }

            eventData.evtvars[0]++;

            var towait = code[exp + 1];
            if (eventData.evtvars[0] >= towait)
                return 2;
            else
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

            var checkme = gameState.PlayerEntity;
            if (checkme.XTile >= x1 && checkme.XTile <= x2
                && checkme.YTile >= y1 && checkme.YTile <= y2
                && checkme.ZTile >= z1 && checkme.ZTile <= z2)
            {
                eventData.logicResult = 1;
                return 7;
            }


            eventData.logicResult = 0;

            return 7;
        }



        public int _40_SetProgramIndex_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var programid = code[exp + 1];
            var indexval = code[exp + 2];

            //somevariable = 1;
            gameState.EventProgsSet = 1;

            entity.Program_Indexes[programid] = indexval;

            return 3;
        }

        public int _41_SetSpriteProgramIndex_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var programid = code[exp + 1];
            var indexval = code[exp + 2];

            entity.Sprite_Program_Indexes[programid] = indexval;

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
            return eventData.exp - eventData.sp;
        }

        public int _4a_IfTrueRestart_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (eventData.logicResult != 0)
                return eventData.exp - eventData.sp;
            return 1;
        }

        public int _4b_IfFalseRestart_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            if (eventData.logicResult == 0)
                return eventData.exp - eventData.sp;
            return 1;
        }

        public int _54_SetWalkable_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            int tilex = code[exp + 1];
            int tiley = code[exp + 2];
            if (tilex < 0)
                tilex = 0;
            else if (tilex > 0x33)
                tilex = 0x33;
            if (tiley < 0)
                tiley = 0;
            if (tiley > 0x3b)
                tiley = 0x3b;

            var walkabilitybits = code[exp + 3];
            var groundpropertybits = code[exp + 4];
            var tile = gameState.gameMap.map.maptiles[tilex + tiley * 52];

            tile.walkability |= walkabilitybits;
            tile.groundproperty |= groundpropertybits;

            return 5;
        }

        public int _55_SetNonWalkable_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            int tilex = code[exp + 1];
            int tiley = code[exp + 2];
            if (tilex < 0)
                tilex = 0;
            else if (tilex > 0x33)
                tilex = 0x33;
            if (tiley < 0)
                tiley = 0;
            if (tiley > 0x3b)
                tiley = 0x3b;

            var walkabilitybits = code[exp + 3];
            var groundpropertybits = code[exp + 4];
            var tile = gameState.gameMap.map.maptiles[tilex + tiley * 52];

            tile.walkability &= (byte)~walkabilitybits;
            tile.groundproperty &= (byte)~groundpropertybits;

            return 5;
        }

        public int _58_DirectionalBranch_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var dir = entity.FrameDex;
            short jumpoffset = (short)(code[exp + (entity.FrameDex * 2) + 1] | code[exp + (entity.FrameDex * 2) + 2]);
            return jumpoffset;
        }

        public int _59_SetEntityAnim_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            int animid = code[exp + 2];
            int numentities = gameState.GetEntityFromRefId(entity, entityid);
            for (int dex = 0; dex < numentities; dex++)
            {
                var dome = gameState.GetEntityList[dex];
                dome.TargetAnim = animid;
            }

            return 3;
        }

        public int _5a_TurnEntity_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            var turncode = code[exp + 2];

            int numentities = gameState.GetEntityFromRefId(entity, entityid);
            for (int dex = 0; dex < numentities; dex++)
            {
                var dome = gameState.GetEntityList[dex];
                dome.TargetDir = gameState.TurnEntity(entity, turncode);
            }

            return 3;
        }

        public int _5b_TurnEntityWithAnim_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            int animid = code[exp + 2];
            var turncode = code[exp + 3];

            int numentities = gameState.GetEntityFromRefId(entity, entityid);
            for (int dex = 0; dex < numentities; dex++)
            {
                var dome = gameState.GetEntityList[dex];
                dome.TargetAnim = animid;
                dome.TargetDir = gameState.TurnEntity(entity, turncode);
            }

            return 4;
        }

        public int _62_EntityFlagsOn_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            int flagbits = (code[exp + 2] + (code[exp + 3] << 8));

            int numentities = gameState.GetEntityFromRefId(entity, entityid);
            for (int dex = 0; dex < numentities; dex++)
            {
                var dome = gameState.GetEntityList[dex];
                dome.Flags |= flagbits;
            }

            return 4;
        }

        public int _63_EntityFlagsOff_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            int flagbits = (code[exp + 2] + (code[exp + 3] << 8));

            int numentities = gameState.GetEntityFromRefId(entity, entityid);
            for (int dex = 0; dex < numentities; dex++)
            {
                var dome = gameState.GetEntityList[dex];
                dome.Flags &= ~flagbits;
            }

            return 4;
        }

        public int _64_SetEntityPos_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            int x = (code[exp + 2] + (code[exp + 3] << 8)) << 16;
            int y = (code[exp + 4] + (code[exp + 5] << 8)) << 16;
            int z = (code[exp + 6] + (code[exp + 7] << 8)) << 16;

            int numentities = gameState.GetEntityFromRefId(entity, entityid);
            for (int dex = 0; dex < numentities; dex++)
            {
                var dome = gameState.GetEntityList[dex];
                dome.XPos = x;
                dome.YPos = y;
                dome.ZPos = z + 1;
            }

            return 8;
        }

        public int _65_MoveEntityPos_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];
            int x = (code[exp + 2] + (code[exp + 3] << 8)) << 16;
            int y = (code[exp + 4] + (code[exp + 5] << 8)) << 16;
            int z = (code[exp + 6] + (code[exp + 7] << 8)) << 16;

            int numentities = gameState.GetEntityFromRefId(entity, entityid);
            for (int dex = 0; dex < numentities; dex++)
            {
                var dome = gameState.GetEntityList[dex];
                dome.XPos += x;
                dome.YPos += y;
                dome.ZPos += z;
            }

            return 8;
        }

        public int _67_CamFollowEntity_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            var entityid = code[exp + 1];

            int numentities = gameState.GetEntityFromRefId(entity, entityid);

            gameState.CamFollowEntity = gameState.GetEntityList[0];

            return 2;
        }

        public int _70_Check144_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            eventData.logicResult = entity._144;

            return 2;
        }

        public int _90_CreateEffect_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            byte effectid = code[1];
            gameState.CreateEffect_MapType(effectid, true);
            return 2;
        }

        public int _91_DisableEffect_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            byte effectid = code[1];
            foreach (var effect in gameState.SpriteEffects)
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
            byte effectid = code[1];
            byte animid = code[2];
            foreach (var effect in gameState.SpriteEffects)
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
            byte effectid = code[1];
            int x = (code[2] | code[3] << 8) << 16;
            int y = (code[4] | code[5] << 8) << 16;
            int z = (code[6] | code[7] << 8) << 16;
            foreach (var effect in gameState.SpriteEffects)
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
            byte effectid = code[1];
            int x = (code[2] | code[3] << 8) << 16;
            int y = (code[4] | code[5] << 8) << 16;
            int z = (code[6] | code[7] << 8) << 16;
            foreach (var effect in gameState.SpriteEffects)
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
            byte effectid = code[1];
            int x = (code[2] | code[3] << 8) << 16;
            int y = (code[4] | code[5] << 8) << 16;
            int z = (code[6] | code[7] << 8) << 16;
            foreach (var effect in gameState.SpriteEffects)
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
            byte effectid = code[1];
            byte entityid = code[2];

            int numentities = gameState.GetEntityFromRefId(entity, entityid);
            if (numentities == 0)
                return 9;

            var refentity = gameState.GetEntityList[0];

            int x = (code[3] | code[4] << 8) << 16;
            int y = (code[5] | code[6] << 8) << 16;
            int z = (code[7] | code[8] << 8) << 16;

            x += refentity.XPos;
            y += refentity.YPos;
            z += refentity.ZPos;

            foreach (var effect in gameState.SpriteEffects)
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
            byte effectid = code[1];
            var effect = gameState.CreateEffect_MapType(effectid, true);
            if (effect == null)
                return 8;
            int x = (code[2] | code[3] << 8) << 16;
            int y = (code[4] | code[5] << 8) << 16;
            int z = (code[6] | code[7] << 8) << 16;

            effect.X = x;
            effect.Y = y;
            effect.Z = z;

            return 8;
        }

        public int _a3_CreateEffectWithEntityPos_Handler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] code)
        {
            byte effectid = code[1];
            byte entityid = code[2];
            int numentities = gameState.GetEntityFromRefId(entity, entityid);
            if (numentities == 0)
                return 9;
            var effect = gameState.CreateEffect_MapType(effectid, true);
            if (effect == null)
                return 9;
            int x = (code[3] | code[4] << 8) << 16;
            int y = (code[5] | code[6] << 8) << 16;
            int z = (code[7] | code[8] << 8) << 16;

            effect.X = entity.XPos + x;
            effect.Y = entity.YPos + y;
            effect.Z = entity.ZPos + z;

            return 9;
        }


    }

    public delegate int ScriptEventHandler(SpriteInstance entity, SpriteInstance entityself/*?*/, int exp, EventProgramState eventData, byte[] eventcode);



    public static class Helper
    {
        public const int PROGRAM_A_LOAD = 0;
        public const int PROGRAM_B_MAP = 1;
        public const int PROGRAM_C_TICK = 2;
        public const int PROGRAM_D_TOUCH = 3;
        public const int PROGRAM_E_DEACTIVATE = 4;
        public const int PROGRAM_F_INTERACT = 5;

        public static int SignExtendWord(int i)
        {
            if ((i & 0x8000) == 0)
                return 0x0000FFFF & i;
            else
                return (int)(0xFFFF0000 | i);
        }

        public static int DirFromVector(int x, int y)
        {
            int flipper = 0;
            if (y < 1)
                flipper = 2;
            if (x < 0)
                flipper++;

            if (x < 0)
                x = -x;
            if (y < 0)
                y = -y;

            int greatest = x;
            if (x < y)
                y = greatest;

            int div = 0;
            var val = Helper.DivTable[div];
            if (val < greatest)
            {
                do
                {
                    div++;
                    val = DivTable[div];
                } while (val < greatest);
            }
            x = x >> div;
            y = y >> div;

            var direction = (int)DirectionTable[y * 16 + x];

            var ret = direction;
            if (flipper == 1)
                ret = 8 - direction;
            else if (flipper == 2)
                ret = 0x18 - direction;
            else if (flipper == 3)
                ret = 8 + direction;
            else if (flipper == 0)
                ret = 0x18 + direction;

            return ret & 0x1f;
        }

        public static int[] Anim_24_Table = new int[]{
0x00000000,//0x00
0x00000003,//0x01
0x00000001,//0x02
0x00000004,//0x03
0x00000000,//0x04
};

        public static int[] XForceTable = new int[]{
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

        public static int[] YForceTable = new int[]{
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

        public static short[] DirVectorsX = new short[]{
0x0,unchecked((short)0xff6a),unchecked((short)0xfeda),unchecked((short)0xfe5a),unchecked((short)0xfde1),unchecked((short)0xfd81),unchecked((short)0xfd3a),unchecked((short)0xfd0f),unchecked((short)0xfd00),unchecked((short)0xfd0f),unchecked((short)0xfd3a),unchecked((short)0xfd81),unchecked((short)0xfde1),unchecked((short)0xfe5a),unchecked((short)0xfeda),unchecked((short)0xff6a),
0x0,0x96,0x126,0x1a6,0x21f,0x27f,0x2c6,0x2f1,0x300,0x2f1,0x2c6,0x27f,0x21f,0x1a6,0x126,0x96};

        public static short[] DirVectorsY = new short[]{
0x200,0x1f6,0x1d9,0x1aa,0x16a,0x11c,0xc4,0x64,0x0,unchecked((short)0xff9c),unchecked((short)0xff3c),unchecked((short)0xfee4),unchecked((short)0xfe96),unchecked((short)0xfe56),unchecked((short)0xfe27),unchecked((short)0xfe0a),
unchecked((short)0xfe00),unchecked((short)0xfe0a),unchecked((short)0xfe27),unchecked((short)0xfe56),unchecked((short)0xfe96),unchecked((short)0xfee4),unchecked((short)0xff3c),unchecked((short)0xff9c),0x0,0x64,0xc4,0x11c,0x16a,0x1aa,0x1d9,0x1f6};

        public static short[] CardinalDirTable = new short[] { 0, 0x10, 0x08, 0x18 };

        static short[] DirectionTable = new short[]{
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

        static uint[] DivTable = new uint[]{
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
