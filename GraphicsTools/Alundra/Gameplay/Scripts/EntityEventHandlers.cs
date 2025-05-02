using System.Diagnostics;
using Alundra.DatasBin;

namespace Alundra.Gameplay.Scripts;

public class EntityEventHandlers
{
    private readonly GameEngine _gameEngine;

    public delegate int EntityEventHandler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] eventCode);

    public readonly SpriteEventHandlers SpriteHandlers;
    private readonly Dictionary<int, EntityEventHandler> _handlers = new();

    public EntityEventHandlers(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
        SpriteHandlers = new SpriteEventHandlers(gameEngine);

        for (var i = 0; i <= 0xff; i++)
        {
            _handlers.Add(i, __Unknown_Handler);
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

    //RunScript
    public void RunEntityEventScripts(Entity entity, int eventProgramType)
    {
        Debugger.Break();
        EventProgramState eventProgramState;

        var isDebug = StaticVariables.g_debugState < 0;
        var isDebugLogicTraceEnabled = (StaticVariables.g_debugFlags & 0x10) != 0;

        if (eventProgramType < 6)
        {
            switch (eventProgramType)
            {
                case ScriptHelper.ProgramBMap:
                    eventProgramState = entity.EventProgramState;
                    if (eventProgramState.Exp == 0
                        || eventProgramState.Sp == 0)
                    {
                        InitializeEventData(entity, eventProgramType, eventProgramState);
                    }
                    break;
                case ScriptHelper.ProgramCTick:
                    eventProgramState = entity.EventProgramState;
                    if (eventProgramState.Exp == 0
                        || eventProgramState.Sp == 0)
                    {
                        InitializeEventData(entity, eventProgramType, eventProgramState);
                    }
                    else
                    {
                        if (entity.MapEventProgramId != 2)
                        {
                            //TODO make sure these event vars are right
                            entity.TargetAnimationId = entity.LastTargetAnimationId;
                            entity.TargetDirection = entity.LastTargetDirection;
                        }
                    }
                    break;
                default:
                    eventProgramState = StaticVariables.g_eventProgramState;

                    if (entity.MapEventProgramId != 2)
                    {
                        entity.LastTargetAnimationId = entity.TargetAnimationId;
                        entity.LastTargetDirection = entity.TargetDirection;
                    }
                    
                    //why ?
                    //if (ScriptHelper.ProgramFInteract == 5)//have to do it here because switch fallthrough isnt allowed in c#
                    //{
                    //    StaticVariables.PlayerEntity.YForceStep = 0;
                    //    StaticVariables.PlayerEntity.XForceStep = 0;
                    //    StaticVariables.PlayerEntity.XForce = 0;
                    //    StaticVariables.PlayerEntity.YForce = 0;
                    //}

                    InitializeEventData(entity, eventProgramType, eventProgramState);
                    break;
            }
        }
        else
        {
            throw new Exception("Illegal logic entry!");
        }

        if (isDebug && isDebugLogicTraceEnabled)
        {
            if (eventProgramType == 1)
            {
                //StaticVariables.g_debugMessage += $"mon{entity.EventTrigger}:";
            }
            else
            {
                //StaticVariables.g_debugMessage += $"%{entity.EntityRefId:x2}({eventProgramType}):";
            }
        }

        Debug.WriteLine($"[{entity.Index}] {entity.EntityRefId} {eventProgramType}");
        
        StaticVariables.g_activeEntityRefId = entity.EntityRefId;
        StaticVariables.g_activeEventProgramType = eventProgramType;
        StaticVariables.g_activeCommand = -1;
        StaticVariables.g_clearProgramState = 0;
        StaticVariables.g_activeEventProgramIndex = entity.ProgramIndexes[eventProgramType];
        bool sameAsSelf;

        while (true)
        {
            sameAsSelf = false;

            //var code = SpriteInfoEventCodes.Codes;
            //var eventCode = code[eventProgramState.Exp];
            var codes = SpriteInfoEventCodes.Codes;
            var eventCode = codes[eventProgramState.Exp];

            if (eventCode == 0xff)
            {
                break;
            }

            if (eventCode == 0)
            {
                eventProgramState.Tick = 0;
                eventProgramState.Exp++;
                break;
            }

            StaticVariables.g_lastCommand = StaticVariables.g_activeCommand;
            StaticVariables.g_activeCommand = eventCode;
            
            var func = _handlers[eventCode];
            var advanced = func(entity.LogicContextEntity, entity, eventProgramState.Exp, eventProgramState, codes);

            if (StaticVariables.g_clearProgramState != 0)
            {
                StaticVariables.g_clearProgramState = 1;
                if (entity != entity.LogicContextEntity)
                {
                    entity.LogicContextEntity.EventProgramState.Sp = 0;
                    entity.LogicContextEntity.EventProgramState.Exp = 0;
                }
                else
                {
                    sameAsSelf = true;
                }
            }

            if (advanced == 0)
            {
                break;
            }

            eventProgramState.Variables[0] = 0; // eventProgramState.Tick = 0; ??
            eventProgramState.Exp += advanced;
        }

        if (sameAsSelf)
        {
            eventProgramState.Sp = 0;
            eventProgramState.Exp = 0;
        }

        if (isDebug && isDebugLogicTraceEnabled)
        {
            //StaticVariables.g_debugMessage += Environment.NewLine;
        }
    }

    private void InitializeEventData(Entity entity, int eventProgramType, EventProgramState eventProgramState)
    {
        var codeIndex = entity.ProgramIndexes[eventProgramType];
        var spriteInfo = _gameEngine.AlundraMap.SpriteInfo;
        var mod = 0;

        if ((codeIndex & 0x80) != 0)
        {
            spriteInfo = _gameEngine.CurrentMap.SpriteInfo;
            mod = 1024 * 512;
        }

        var sp = spriteInfo.EventCodes.EventCodesTable[eventProgramType][codeIndex & 0x7f] + mod;
        eventProgramState.Sp = sp;
        eventProgramState.Exp = sp;

        //some error checking here, 
        //looking to see if the code pointers are in the correct range of where they should be
    }

    //All Script_xxx functions
    //8003D158
    public int __Unknown_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        Debug.WriteLine("Data Logic Error!");
        return 0;
    }

    public int _02_Goto_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        //var offset = (short)(code[exp + 1] + (code[exp + 2] << 8));
        //return offset;
        return (code[1] + code[2] * 0x100 * 0x10000) >> 0x10;
    }

    public int _03_BranchIfTrue_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        if (eventProgramState.LogicResult == 0)
        {
            return 3;
        }

        var offset = (short)(code[exp + 1] + (code[exp + 2] << 8));

        return offset;
    }

    public int _04_BranchIfFalse_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        if (eventProgramState.LogicResult != 0)
        {
            return 3;
        }

        var offset = (short)(code[exp + 1] + (code[exp + 2] << 8));

        return offset;
    }

    public int _05_FlagOn_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        if (eventProgramState.LogicResult != 0)
        {
            return 3;
        }

        uint flagdata = (uint)(code[exp + 1] + (code[exp + 2] << 8));
        //int flag = (flagdata >> 3) & 0xffc;
        var flag = (flagdata >> 5) & 0x3ff;
        uint[] flags;
        //if the mapflag bit is set
        if ((flagdata & 0x8000) != 0)
        {
            flags = StaticVariables.g_mapFlags;
        }
        else//otherwise its a global flag
        {
            flags = StaticVariables.g_globalFlags; //StaticVariables.g_globalFlags;
        }

        uint bittoset = flagdata & 0x1f;

        //turn on the bit for this flag
        flags[flag] |= (uint)1 << (int)bittoset;

        return 3;
    }

    public int _06_FlagOff_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        if (eventProgramState.LogicResult != 0)
        {
            return 3;
        }

        var flagdata = code[exp + 1] + (code[exp + 2] << 8);
        //int flag = (flagdata >> 3) & 0xffc;
        var flag = (flagdata >> 5) & 0x3ff;
        uint[] flags;
        //if the mapflag bit is set
        if ((flagdata & 0x8000) != 0)
        {
            flags = StaticVariables.g_mapFlags;
        }
        else//otherwise its a global flag
        {
            flags = StaticVariables.g_globalFlags;
        }

        var bittoset = flagdata & 0x1f;

        //turn off the bit for this flag
        flags[flag] &= ~((uint)1 << (int)bittoset);

        return 3;
    }

    public int _07_CheckEntityInArea_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var entityid = code[exp + 1];
        int x1 = code[exp + 2];
        int x2 = code[exp + 3];
        int y1 = code[exp + 4];
        int y2 = code[exp + 5];
        int z1 = code[exp + 6];
        int z2 = code[exp + 7];
        var numentities = _gameEngine.GetEntityFromRefId(entity, entityid);
        for (var i = 0; i < numentities; i++)
        {
            var checkme = StaticVariables.g_entitySlots[i];
            if (checkme.TileX >= x1 && checkme.TileX <= x2
                                    && checkme.TileY >= y1 && checkme.TileY <= y2
                                    && checkme.TileZ >= z1 && checkme.TileZ <= z2)
            {
                eventProgramState.LogicResult = 1;
                return 8;
            }
        }

        eventProgramState.LogicResult = 0;

        return 8;
    }

    public int _08_Turn_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        entity.TargetDirection = (entity.TargetDirection + code[exp + 1]) & 0x1f;
        return 2;
    }

    public int _09_SetDir_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        entity.TargetDirection = (uint)(code[exp + 1] & 0x1f);
        return 2;
    }

    public int _0a_Reverse_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        entity.TargetDirection = (entity.TargetDirection + 0x10) & 0x1f;
        return 1;
    }

    //set animation, and block until entity has moved specified distance
    public int _0b_AnimWaitDistance_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        uint animid = code[exp + 1];
        entity.TargetAnimationId = animid;
        if (exp != eventProgramState.Tick)
        {
            eventProgramState.Tick = exp;
            eventProgramState.Variables[0] = entity.XPos;
            eventProgramState.Variables[1] = entity.YPos;
            return 0;
        }
        var difx = eventProgramState.Variables[0] - entity.XPos;
        var dify = eventProgramState.Variables[1] - entity.YPos;
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

    public int _0c_SetRandomDir_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var i = StaticVariables.g_gameRandomSeed;
        var val1 = (int)(i * 0x7d2b89dd);
        var val2 = (int)(0xe06a02e7 + val1);
        var val3 = (int)(((long)val2 * 4) >> 32);
        var dir = (uint)ScriptHelper.CardinalDirTable[val3];//val3 here is a number between 0 and 3
        StaticVariables.g_gameRandomSeed = (uint)val2;
        entity.TargetDirection = dir;
        return 1;
    }

    public int _0d_Dialog_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        if ((entity.Flags & 0x800000) != 0)//has portrait
        {
            //TODO get it from memory, not disk
            //SIImageSet portrait = entity.Sprite.GetPortraitImageset(datasReader);
            //var img = portrait.images[0];
            //var bmps = gameState.GetSpriteImages(portrait);
            //var bmp = bmps[0];
            //WrapsDialogSetupPortrait(entity.XPos, entity.YPos, entity.ZPos, gameState.g_cameraCurrentX, gameState.g_cameraCurrentY, img.sx, img.sy, img.swidth, img.sheight, bmp);
        }
        //SetName(entity.NameId);

        //var ret = SetText(code[exp + 1], code[exp + 2]);

        //if (ret > 0)
        //    return 3;
        return 0;
    }

    public int _10_LoseControl_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        StaticVariables.g_playerControlFlags |= 0x4;
        return 1;
    }

    public int _11_GainControl_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        StaticVariables.g_playerControlFlags &= ~0x4;
        return 1;
    }

    public int _12_PlaySound1_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        //throw new Exception("impliment this one!");
        int soundid = code[exp + 1];
        return 2;
    }

    public int _15_ResetZPos_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        if (entity.EntityRecord == null)
        {
            Debug.Print("No InitData");
        }

        entity.ZPos = (entity.EntityRecord.Height * 8 - entity.ZMod) << 16;
        return 1;
    }

    public int _16_GravityOn_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        entity.Flags |= 0x100;
        return 1;
    }

    public int _17_GravityOff_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var flag = ~0x100;
        entity.Flags &= (uint)flag;
        return 1;
    }

    public int _19_Deactivate_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        entity.Status = 3;
        return 1;
    }

    public int _1a_SetAnim_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        entity.TargetAnimationId = code[exp + 1];
        return 2;
    }

    //sets zforce
    public int _1b_Fly_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var force = code[exp + 1] + (code[exp + 2] << 8);
        force = force << 16;//sign extend
        force = force >> 8;//get it to the correct multiple
        entity.ZForce = force;
        return 2;
    }

    public int _1c_WaitAnim_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        if (exp != eventProgramState.Tick)
        {
            eventProgramState.Tick = exp;
            eventProgramState.Variables[0] = 0;
            entity.AnimCompleteCounter = 0;
            return 0;
        }

        if (entity.ForceResetAnimationFlag != 0)
        {
            entity.CurrentAnimationId = ~entity.TargetAnimationId;
        }

        if (entity.ForceResetAnimationFlag != 0 || entity.AnimCompleteCounter != 0)
        {
            eventProgramState.Variables[0]++;
            entity.AnimCompleteCounter = 0;
        }


        var towait = code[exp + 1];
        if (eventProgramState.Variables[0] >= towait)
        {
            return 2;
        }

        return 0;
    }

    //collision ends it
    public int _1d_WaitAnim2_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var ret = _1c_WaitAnim_Handler(entity, entitySelf, exp, eventProgramState, code);

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
    public int _1e_WaitWalk_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        if (exp != eventProgramState.Tick)
        {
            eventProgramState.Tick = exp;
            eventProgramState.Variables[0] = entity.XPos;
            eventProgramState.Variables[1] = entity.YPos;
            return 0;
        }

        var x = Math.Abs(eventProgramState.Variables[0] - entity.XPos) >> 16;
        var y = Math.Abs(eventProgramState.Variables[1] - entity.YPos) >> 16;

        var distance = code[exp + 1] | (code[exp + 2] << 8);

        if (x >= distance || y >= distance)
        {
            return 3;
        }

        return 0;
    }

    //wait until they have walked a certain distance, collision ends the walk
    public int _1f_WaitWalk2_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var ret = _1e_WaitWalk_Handler(entity, entitySelf, exp, eventProgramState, code);

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
    public int _24_WaitForceAdjusted_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        if (entity.ForceAdjusted > 0)
        {
            return 1;
        }

        return 0;
    }

    public int _25_WaitEntityCollisionZOr144_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        if (entity.CollidedWithEntityZ != 0)
        {
            return 1;
        }

        if (entity.IsAboveGround != 0)
        {
            return 1;
        }

        return 0;
    }

    public int _26_WaitForceAdjustedOrEntityCollisionZ_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
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

    public int _27_FacePlayer_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(StaticVariables.PlayerEntity.XPos - entity.XPos, StaticVariables.PlayerEntity.YPos - entity.YPos);
        return 1;
    }

    public int _28_Flag2On_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        entity.Flags |= 0x8;
        return 1;
    }

    public int _29_Flag2Off_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var flags = ~0x8;
        entity.Flags &= (uint)flags;
        return 1;
    }

    public int _2a_Flag3On_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        entity.Flags |= 0x1;
        return 1;
    }

    public int _2b_Flag3Off_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var flags = ~0x1;
        entity.Flags &= (uint)flags;
        return 1;
    }

    public int _2d_ActivateEntity_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var entityid = code[exp + 1];
        var loaded = _gameEngine.ActivateEntity(entity, entityid, 1);
        if (loaded == null)
        {
            throw new Exception("Illigal InitData Number!!");
        }

        return 2;
    }

    public int _2e_Hide_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var entityid = code[exp + 1];
        var numentities = _gameEngine.GetEntityFromRefId(entity, entityid);
        for (var dex = 0; dex < numentities; dex++)
        {
            var checkme = StaticVariables.g_entitySlots[dex];
            _gameEngine.HideEntity(checkme);
        }

        return 2;
    }

    public int _2f_CheckPlayerInput_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var inputid = code[exp + 3];
        var mask = code[exp + 1] | code[exp + 2] << 8;

        //StaticVariables.g_padState1.ButtonsHold
        //if ((_gameEngine.PlayerInput[inputid] & mask) != 0)
        if ((StaticVariables.g_padState1.ButtonsJustPressed & mask) != 0)
        {
            eventProgramState.LogicResult = 1;
        }
        else
        {
            eventProgramState.LogicResult = 0;
        }

        return 4;
    }

    public int _30_IfFlagOff_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {

        var flagdata = code[exp + 1] + (code[exp + 2] << 8);
        //int flag = (flagdata >> 3) & 0xffc;
        var flag = (flagdata >> 5) & 0x3ff;
        uint[] flags;
        //if the mapflag bit is set
        if ((flagdata & 0x8000) != 0)
        {
            flags = StaticVariables.g_mapFlags;
        }
        else//otherwise its a global flag
        {
            flags = StaticVariables.g_globalFlags;
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

    public int _31_IfFlagOn_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {

        var flagdata = code[exp + 1] + (code[exp + 2] << 8);
        //int flag = (flagdata >> 3) & 0xffc;
        var flag = (flagdata >> 5) & 0x3ff;
        uint[] flags;
        //if the mapflag bit is set
        if ((flagdata & 0x8000) != 0)
        {
            flags = StaticVariables.g_mapFlags;
        }
        else//otherwise its a global flag
        {
            flags = StaticVariables.g_globalFlags;
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

    public int _32_FlagToggle_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        if (eventProgramState.LogicResult != 0)
        {
            return 3;
        }

        var flagdata = code[exp + 1] + (code[exp + 2] << 8);
        //int flag = (flagdata >> 3) & 0xffc;
        var flag = (flagdata >> 5) & 0x3ff;
        uint[] flags;
        //if the mapflag bit is set
        if ((flagdata & 0x8000) != 0)
        {
            flags = StaticVariables.g_mapFlags;
        }
        else//otherwise its a global flag
        {
            flags = StaticVariables.g_globalFlags;
        }

        var bittoset = flagdata & 0x1f;

        //toggle the bit for this flag
        flags[flag] ^= (uint)1 << bittoset;//xor, toggles

        return 3;
    }

    public int _33_CheckFlagsOn_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        //do this 4 times
        {
            var flagdata = code[exp + 1] + (code[exp + 2] << 8);
            //int flag = (flagdata >> 3) & 0xffc;
            var flag = (flagdata >> 5) & 0x3ff;
            uint[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = StaticVariables.g_mapFlags;
            }
            else//otherwise its a global flag
            {
                flags = StaticVariables.g_globalFlags;
            }

            var bittocheck = flagdata & 0x1f;

            //check the bit for this flag
            if ((flags[flag] & (1 << bittocheck)) == 0)
            {
                eventProgramState.LogicResult = 0;
                return 9;
            }
        }

        {
            var flagdata = code[exp + 3] + (code[exp + 4] << 8);
            //int flag = (flagdata >> 3) & 0xffc;
            var flag = (flagdata >> 5) & 0x3ff;
            uint[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = StaticVariables.g_mapFlags;
            }
            else//otherwise its a global flag
            {
                flags = StaticVariables.g_globalFlags;
            }

            var bittocheck = flagdata & 0x1f;

            //check the bit for this flag
            if ((flags[flag] & (1 << bittocheck)) == 0)
            {
                eventProgramState.LogicResult = 0;
                return 9;
            }
        }

        {
            var flagdata = code[exp + 5] + (code[exp + 6] << 8);
            //int flag = (flagdata >> 3) & 0xffc;
            var flag = (flagdata >> 5) & 0x3ff;
            uint[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = StaticVariables.g_mapFlags;
            }
            else//otherwise its a global flag
            {
                flags = StaticVariables.g_globalFlags;
            }

            var bittocheck = flagdata & 0x1f;

            //check the bit for this flag
            if ((flags[flag] & (1 << bittocheck)) == 0)
            {
                eventProgramState.LogicResult = 0;
                return 9;
            }
        }

        {
            var flagdata = code[exp + 7] + (code[exp + 8] << 8);
            //int flag = (flagdata >> 3) & 0xffc;
            var flag = (flagdata >> 5) & 0x3ff;
            uint[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = StaticVariables.g_mapFlags;
            }
            else//otherwise its a global flag
            {
                flags = StaticVariables.g_globalFlags;
            }

            var bittocheck = flagdata & 0x1f;

            //check the bit for this flag
            if ((flags[flag] & (1 << bittocheck)) == 0)
            {
                eventProgramState.LogicResult = 0;
                return 9;
            }
        }


        eventProgramState.LogicResult = 1;//made it through them all
        return 9;
    }

    public int _34_CheckFlagsOff_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        //do this 4 times
        {
            var flagdata = code[exp + 1] + (code[exp + 2] << 8);
            //int flag = (flagdata >> 3) & 0xffc;
            var flag = (flagdata >> 5) & 0x3ff;
            uint[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = StaticVariables.g_mapFlags;
            }
            else//otherwise its a global flag
            {
                flags = StaticVariables.g_globalFlags;
            }

            var bittocheck = flagdata & 0x1f;

            //check the bit for this flag
            if ((flags[flag] & (1 << bittocheck)) != 0)
            {
                eventProgramState.LogicResult = 0;
                return 9;
            }
        }

        {
            var flagdata = code[exp + 3] + (code[exp + 4] << 8);
            //int flag = (flagdata >> 3) & 0xffc;
            var flag = (flagdata >> 5) & 0x3ff;
            uint[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = StaticVariables.g_mapFlags;
            }
            else//otherwise its a global flag
            {
                flags = StaticVariables.g_globalFlags;
            }

            var bittocheck = flagdata & 0x1f;

            //check the bit for this flag
            if ((flags[flag] & (1 << bittocheck)) != 0)
            {
                eventProgramState.LogicResult = 0;
                return 9;
            }
        }

        {
            var flagdata = code[exp + 5] + (code[exp + 6] << 8);
            //int flag = (flagdata >> 3) & 0xffc;
            var flag = (flagdata >> 5) & 0x3ff;
            uint[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = StaticVariables.g_mapFlags;
            }
            else//otherwise its a global flag
            {
                flags = StaticVariables.g_globalFlags;
            }

            var bittocheck = flagdata & 0x1f;

            //check the bit for this flag
            if ((flags[flag] & (1 << bittocheck)) != 0)
            {
                eventProgramState.LogicResult = 0;
                return 9;
            }
        }

        {
            var flagdata = code[exp + 7] + (code[exp + 8] << 8);
            //int flag = (flagdata >> 3) & 0xffc;
            var flag = (flagdata >> 5) & 0x3ff;
            uint[] flags;
            //if the mapflag bit is set
            if ((flagdata & 0x8000) != 0)
            {
                flags = StaticVariables.g_mapFlags;
            }
            else//otherwise its a global flag
            {
                flags = StaticVariables.g_globalFlags;
            }

            var bittocheck = flagdata & 0x1f;

            //check the bit for this flag
            if ((flags[flag] & (1 << bittocheck)) != 0)
            {
                eventProgramState.LogicResult = 0;
                return 9;
            }
        }


        eventProgramState.LogicResult = 1;//made it through them all
        return 9;
    }


    //blocks until flag is off
    public int _35_UntilFlagOff_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {

        var flagdata = code[exp + 1] + (code[exp + 2] << 8);
        //int flag = (flagdata >> 3) & 0xffc;
        var flag = (flagdata >> 5) & 0x3ff;
        uint[] flags;
        //if the mapflag bit is set
        if ((flagdata & 0x8000) != 0)
        {
            flags = StaticVariables.g_mapFlags;
        }
        else//otherwise its a global flag
        {
            flags = StaticVariables.g_globalFlags;
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
    public int _36_UntilFlagOn_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {

        var flagdata = code[exp + 1] + (code[exp + 2] << 8);
        //int flag = (flagdata >> 3) & 0xffc;
        var flag = (flagdata >> 5) & 0x3ff;
        uint[] flags;
        //if the mapflag bit is set
        if ((flagdata & 0x8000) != 0)
        {
            flags = StaticVariables.g_mapFlags;
        }
        else//otherwise its a global flag
        {
            flags = StaticVariables.g_globalFlags;
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
    public int _37_Wait_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        if (exp != eventProgramState.Tick)
        {
            eventProgramState.Tick = exp;
            eventProgramState.Variables[0] = 0;
            return 0;
        }

        eventProgramState.Variables[0]++;

        var towait = code[exp + 1];
        if (eventProgramState.Variables[0] >= towait)
        {
            return 2;
        }

        return 0;
    }

    public int _3b_CheckPlayerInArea_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        int x1 = code[exp + 1];
        int x2 = code[exp + 2];
        int y1 = code[exp + 3];
        int y2 = code[exp + 4];
        int z1 = code[exp + 5];
        int z2 = code[exp + 6];

        var checkme = StaticVariables.PlayerEntity;
        if (checkme.TileX >= x1 && checkme.TileX <= x2
                                && checkme.TileY >= y1 && checkme.TileY <= y2
                                && checkme.TileZ >= z1 && checkme.TileZ <= z2)
        {
            eventProgramState.LogicResult = 1;
            return 7;
        }


        eventProgramState.LogicResult = 0;

        return 7;
    }



    public int _40_SetProgramIndex_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var programid = code[exp + 1];
        var indexval = code[exp + 2];

        //somevariable = 1;
        StaticVariables.g_clearProgramState = 1;

        entity.ProgramIndexes[programid] = indexval;

        return 3;
    }

    public int _41_SetSpriteProgramIndex_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var programid = code[exp + 1];
        var indexval = code[exp + 2];

        entity.SpriteProgramIndexes[programid] = indexval;

        return 3;
    }

    public int _45_Flag4Off_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        entity.Flags &= ~(uint)0x2000;
        return 1;
    }

    public int _46_Flag4On_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        entity.Flags |= 0x2000;
        return 1;
    }

    public int _49_Restart_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        return eventProgramState.Exp - eventProgramState.Sp;
    }

    public int _4a_IfTrueRestart_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        if (eventProgramState.LogicResult != 0)
        {
            return eventProgramState.Exp - eventProgramState.Sp;
        }

        return 1;
    }

    public int _4b_IfFalseRestart_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        if (eventProgramState.LogicResult == 0)
        {
            return eventProgramState.Exp - eventProgramState.Sp;
        }

        return 1;
    }

    public int _54_SetWalkable_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
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
        var tile = _gameEngine.CurrentMap.Map.MapTiles[tilex + tiley * 52];

        tile.Walkability |= walkabilitybits;
        tile.GroundProperty |= groundpropertybits;

        return 5;
    }

    public int _55_SetNonWalkable_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
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
        var tile = _gameEngine.CurrentMap.Map.MapTiles[tilex + tiley * 52];

        tile.Walkability &= (byte)~walkabilitybits;
        tile.GroundProperty &= (byte)~groundpropertybits;

        return 5;
    }

    public int _58_DirectionalBranch_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var dir = entity.CurrentFrameIndex;
        var jumpoffset = (short)(code[exp + entity.CurrentFrameIndex * 2 + 1] | code[exp + entity.CurrentFrameIndex * 2 + 2]);
        return jumpoffset;
    }

    public int _59_SetEntityAnim_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var entityid = code[exp + 1];
        int animid = code[exp + 2];
        var numentities = _gameEngine.GetEntityFromRefId(entity, entityid);
        for (var dex = 0; dex < numentities; dex++)
        {
            var dome = StaticVariables.g_entitySlots[dex];
            dome.TargetAnimationId = (uint)animid;
        }

        return 3;
    }

    public int _5a_TurnEntity_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var entityid = code[exp + 1];
        var turncode = code[exp + 2];

        var numentities = _gameEngine.GetEntityFromRefId(entity, entityid);
        for (var dex = 0; dex < numentities; dex++)
        {
            var dome = StaticVariables.g_entitySlots[dex];
            dome.TargetDirection = _gameEngine.TurnEntity(entity, turncode);
        }

        return 3;
    }

    public int _5b_TurnEntityWithAnim_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var entityid = code[exp + 1];
        int animid = code[exp + 2];
        var turncode = code[exp + 3];

        var numentities = _gameEngine.GetEntityFromRefId(entity, entityid);
        for (var dex = 0; dex < numentities; dex++)
        {
            var dome = StaticVariables.g_entitySlots[dex];
            dome.TargetAnimationId = (uint)animid;
            dome.TargetDirection = _gameEngine.TurnEntity(entity, turncode);
        }

        return 4;
    }

    public int _62_EntityFlagsOn_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var entityid = code[exp + 1];
        var flagbits = code[exp + 2] + (code[exp + 3] << 8);

        var numentities = _gameEngine.GetEntityFromRefId(entity, entityid);
        for (var dex = 0; dex < numentities; dex++)
        {
            var dome = StaticVariables.g_entitySlots[dex];
            dome.Flags |= (uint)flagbits;
        }

        return 4;
    }

    public int _63_EntityFlagsOff_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var entityid = code[exp + 1];
        var flagbits = code[exp + 2] + (code[exp + 3] << 8);

        var numentities = _gameEngine.GetEntityFromRefId(entity, entityid);
        for (var dex = 0; dex < numentities; dex++)
        {
            var dome = StaticVariables.g_entitySlots[dex];
            dome.Flags &= ~(uint)flagbits;
        }

        return 4;
    }

    public int _64_SetEntityPos_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var entityid = code[exp + 1];
        var x = (code[exp + 2] + (code[exp + 3] << 8)) << 16;
        var y = (code[exp + 4] + (code[exp + 5] << 8)) << 16;
        var z = (code[exp + 6] + (code[exp + 7] << 8)) << 16;

        var numentities = _gameEngine.GetEntityFromRefId(entity, entityid);
        for (var i = 0; i < numentities; i++)
        {
            var entity2 = StaticVariables.g_entitySlots[i];
            entity2.XPos = x;
            entity2.YPos = y;
            entity2.ZPos = z + 1;
        }

        return 8;
    }

    public int _65_MoveEntityPos_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var entityid = code[exp + 1];
        var x = (code[exp + 2] + (code[exp + 3] << 8)) << 16;
        var y = (code[exp + 4] + (code[exp + 5] << 8)) << 16;
        var z = (code[exp + 6] + (code[exp + 7] << 8)) << 16;

        var numentities = _gameEngine.GetEntityFromRefId(entity, entityid);
        for (var dex = 0; dex < numentities; dex++)
        {
            var dome = StaticVariables.g_entitySlots[dex];
            dome.XPos += x;
            dome.YPos += y;
            dome.ZPos += z;
        }

        return 8;
    }

    public int _67_CamFollowEntity_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var entityid = code[exp + 1];

        var numentities = _gameEngine.GetEntityFromRefId(entity, entityid);

        StaticVariables.g_entityFollowedByCamera = StaticVariables.g_entitySlots[0];

        return 2;
    }

    public int _70_Check144_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        eventProgramState.LogicResult = entity.IsAboveGround;

        return 2;
    }

    public int _90_CreateEffect_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var effectid = code[1];
        _gameEngine.CreateEffect_MapType(effectid, true);
        return 2;
    }

    public int _91_DisableEffect_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var effectid = code[1];
        foreach (var effect in StaticVariables.g_effectSlots)
        {
            if (effect.Status != 0 && effect.MapEffectId == effectid)
            {
                effect.Status = 0;
            }
        }

        return 2;
    }

    public int _92_SetEffectAnim_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var effectid = code[1];
        var animid = code[2];
        foreach (var effect in StaticVariables.g_effectSlots)
        {
            if (effect.Status != 0 && effect.MapEffectId == effectid)
            {
                effect.TargetAnimation = animid;
            }
        }

        return 3;
    }

    public int _93_SetEffectPos_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var effectid = code[1];
        var x = (code[2] | code[3] << 8) << 16;
        var y = (code[4] | code[5] << 8) << 16;
        var z = (code[6] | code[7] << 8) << 16;
        foreach (var effect in StaticVariables.g_effectSlots)
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

    public int _94_SetEffectForces_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var effectid = code[1];
        var x = (code[2] | code[3] << 8) << 16;
        var y = (code[4] | code[5] << 8) << 16;
        var z = (code[6] | code[7] << 8) << 16;
        foreach (var effect in StaticVariables.g_effectSlots)
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

    public int _a0_AdjustEffectPos_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var effectid = code[1];
        var x = (code[2] | code[3] << 8) << 16;
        var y = (code[4] | code[5] << 8) << 16;
        var z = (code[6] | code[7] << 8) << 16;
        foreach (var effect in StaticVariables.g_effectSlots)
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

    public int _a1_AdjustEffectPosWithEntity_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var effectid = code[1];
        var entityid = code[2];

        var numentities = _gameEngine.GetEntityFromRefId(entity, entityid);
        if (numentities == 0)
        {
            return 9;
        }

        var refentity = StaticVariables.g_entitySlots[0];

        var x = (code[3] | code[4] << 8) << 16;
        var y = (code[5] | code[6] << 8) << 16;
        var z = (code[7] | code[8] << 8) << 16;

        x += refentity.XPos;
        y += refentity.YPos;
        z += refentity.ZPos;

        foreach (var effect in StaticVariables.g_effectSlots)
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

    public int _a2_CreateEffectWithPos_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var effectid = code[1];
        var effect = _gameEngine.CreateEffect_MapType(effectid, true);
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

    public int _a3_CreateEffectWithEntityPos_Handler(Entity entity, Entity entitySelf, int exp, EventProgramState eventProgramState, byte[] code)
    {
        var effectid = code[1];
        var entityid = code[2];
        var numentities = _gameEngine.GetEntityFromRefId(entity, entityid);
        if (numentities == 0)
        {
            return 9;
        }

        var effect = _gameEngine.CreateEffect_MapType(effectid, true);
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