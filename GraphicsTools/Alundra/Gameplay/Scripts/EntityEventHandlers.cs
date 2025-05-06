using System.Diagnostics;
using Alundra.DatasBin;

namespace Alundra.Gameplay.Scripts;

public class EntityEventHandlers
{
    private readonly GameEngine _gameEngine;

    public delegate int EntityEventHandler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState);

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

    //8004205c
    public void RunEntityEventScripts(Entity entity, int logicMode)
    {
        EventProgramState eventProgramState = StaticVariables.g_eventProgramState;

        var isDebug = StaticVariables.g_debugState < 0;
        var isDebugLogicTraceEnabled = (StaticVariables.g_debugFlags & 0x10) != 0;

        if (logicMode < 6)
        {
            switch (logicMode)
            {
                case ScriptHelper.ProgramBMap:
                    eventProgramState = entity.EventProgramState;

                    if (eventProgramState.Exp[0] != 0 && eventProgramState.Sp != 0)
                    {
                        entity.MapEventProgramId = logicMode;
                        break;
                        //goto END_LOGIC_SETUP;
                    }
                    
                    InitializeEventData(entity, logicMode, eventProgramState);
                    entity.MapEventProgramId = logicMode;
                    break;

                case ScriptHelper.ProgramCTick:
                    eventProgramState = entity.EventProgramState;

                    if (eventProgramState.Exp[0] != 0 && eventProgramState.Sp != 0) 
                    {
                        if (entity.MapEventProgramId != 2) 
                        {
                            entity.TargetAnimationId = entity.LastTargetAnimationId;
                            entity.TargetDirection = entity.LastTargetDirection;
                        }

                        entity.MapEventProgramId = logicMode;
                        //goto SET_LOGIC_MODE;
                    }

                    break;

                case ScriptHelper.ProgramFInteract:

                    StaticVariables.PlayerEntity.YForceStep = 0;
                    StaticVariables.PlayerEntity.XForceStep = 0;
                    StaticVariables.PlayerEntity.YForce = 0;
                    StaticVariables.PlayerEntity.XForce = 0;
                    InitializeEventData(entity, logicMode, eventProgramState);
                    entity.MapEventProgramId = logicMode;
                    break;

                default:
                    //if (entity.ProgramIndexes[ScriptHelper.ProgramCTick] != 2)
                    if (entity.MapEventProgramId == ScriptHelper.ProgramCTick)
                    {
                        entity.LastTargetAnimationId = entity.TargetAnimationId;
                        entity.LastTargetDirection = entity.TargetDirection;
                    }
                    
                    InitializeEventData(entity, logicMode, eventProgramState);
                    entity.MapEventProgramId = logicMode;
                    break;
            }
        }
        else
        {
            throw new Exception("Illegal logic entry!");
        }

        if (isDebug && isDebugLogicTraceEnabled)
        {
            if (logicMode == 1)
            {
                //StaticVariables.g_debugMessage += $"mon{entity.EventTrigger}:";
            }
            else
            {
                //StaticVariables.g_debugMessage += $"%{entity.EntityRefId:x2}({logicMode}):";
            }
        }

        SET_LOGIC_MODE:
        entity.MapEventProgramId = logicMode;

        END_LOGIC_SETUP:
        if (isDebug)
        {
            if (logicMode == 1)
            {
                //DebugMessageFormat("mon{0:D2}:", entity.EventTrigger);
            }
            else
            {
                //DebugMessageFormat("{0:X2}({1}):", entity.SpriteProgramIndexes[0], logicMode);
            }
        }

        //eventProgramState = StaticVariables.g_eventProgramState;                                 
        var wasEntityCleared = false;
        StaticVariables.g_activeCommand = -1;
        StaticVariables.g_activeEventProgramIndex = entity.ProgramIndexes[logicMode];
        StaticVariables.g_clearProgramState = 0;
        StaticVariables.g_activeEntityRefId = entity.EntityRefId;
        StaticVariables.g_activeEventProgramType = logicMode;

        // Main script execution loop
        while (true)
        {
            NextCommand(eventProgramState);
            int command = eventProgramState.Sp;
            //if (isDebug)
            //    DebugMessageFormat(" {0:D3}", command);

            if (command == 0xFF) goto END_SCRIPT; // end of script
            if (command == 0x00)
            {
                break;
            }

            var logicContextEntity = entity.LogicContextEntity;
            var lastCommand = StaticVariables.g_activeCommand;
            StaticVariables.g_activeCommand = command;
            
            //var codes = SpriteInfoEventCodes.Codes;
            //var eventCode = codes[eventProgramState.Exp[0]];

            var func = _handlers[command];
            var result = func(entity.LogicContextEntity, entity, eventProgramState.Exp, eventProgramState);

            var name = SpriteInfoEventCodes.CommandNameByCode.GetValueOrDefault((byte)command, "");
            Debug.WriteLine($"Entity[{entity.Index}] run command '{name}' {string.Join(',', eventProgramState.Exp)} = {result}");

            StaticVariables.g_lastCommand = lastCommand;

            if (StaticVariables.g_clearProgramState != 0)
            {
                if (logicContextEntity != entity)
                {
                    StaticVariables.g_clearProgramState = 0;
                    ClearEventProgramState(logicContextEntity, true);
                }
                else
                {
                    wasEntityCleared = true;
                }
            }

            if (result == 0)
                break;
            
            ClearExp(entity.EventProgramState);
            eventProgramState.CommandIndex++;
        }
           
        ClearExp(eventProgramState);
        eventProgramState.CommandIndex++;

        END_SCRIPT:
        if (wasEntityCleared) 
        {
            ClearEventProgramState(entity, true);
        }

        //if (isDebug)
        //    DebugMessageFormat("\n");
    }

    private void NextCommand(EventProgramState eventProgramState)
    {
        if (eventProgramState.Commands == null || eventProgramState.CommandIndex >= eventProgramState.Commands.Count)
        {
            return;
        }

        var siCommand = eventProgramState.Commands[eventProgramState.CommandIndex];
        ClearExp(eventProgramState);

        eventProgramState.Sp = siCommand.Command;

        for (int i = 0; i < siCommand.Parameters.Length; i++)
        {
            eventProgramState.Exp[i] = siCommand.Parameters[i];
        }
    }

    private static void ClearEventProgramState(Entity entity, bool clearCommandIndex = false)
    {
        if (clearCommandIndex)
        {
            entity.EventProgramState.CommandIndex = 0;
        }

        entity.EventProgramState.Sp = 0;
        ClearExp(entity.EventProgramState);
    }

    private static void ClearExp(EventProgramState eventProgramState)
    {
        for (int i = 0; i < eventProgramState.Exp.Length; i++)
        {
            eventProgramState.Exp[i] = 0;
        }
    }

    private void InitializeEventData(Entity entity, int eventProgramType, EventProgramState eventProgramState)
    {
        var codeIndex = entity.ProgramIndexes[eventProgramType];
        var spriteInfo = _gameEngine.AlundraMap.SpriteInfo;

        if ((codeIndex & 0x80) != 0) {
            spriteInfo = _gameEngine.CurrentMap.SpriteInfo;
        }

        using var br = _gameEngine.DatasBin.OpenBin();
        eventProgramState.Commands = spriteInfo.EventCodes.GetCommands(br, entity.ProgramIndexes[eventProgramType], true);
        eventProgramState.CommandIndex = 0;
        eventProgramState.Sp = 0;

        //for (var i = 0; i < siCommands.Count; i++)
        //{
        //    eventProgramState.Sp.Add(siCommands[i].Command);
        //}

        for (int i = 0; i < eventProgramState.Exp.Length; i++)
        {
            eventProgramState.Exp[i] = 0;
        }

        //for (int i = 0; i < siCommands[eventProgramState.CommandIndex].Parameters.Length; i++)
        //{
        //    eventProgramState.Exp[i] = siCommands[eventProgramState.CommandIndex].Parameters[i];
        //}
    }

    //All Script_xxx functions
    //8003D158
    public int __Unknown_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        //Debug.WriteLine("Data Logic Error!");
        return 0;
    }

    public int _02_Goto_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        //var offset = (short)(exp[1] + (exp[2] << 8));
        //return offset;
        return (exp[1] + exp[2] * 0x100 * 0x10000) >> 0x10;
    }

    public int _03_BranchIfTrue_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        if (eventProgramState.Exp[9] == 0)
        {
            return 3;
        }

        var offset = (short)(exp[1] + (exp[2] << 8));

        return offset;
    }

    public int _04_BranchIfFalse_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        if (eventProgramState.Exp[9] != 0)
        {
            return 3;
        }

        var offset = (short)(exp[1] + (exp[2] << 8));

        return offset;
    }

    public int _05_FlagOn_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        if (eventProgramState.Exp[9] != 0)
        {
            return 3;
        }

        uint flagdata = (uint)(exp[1] + (exp[2] << 8));
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

    public int _06_FlagOff_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        if (eventProgramState.Exp[9] != 0)
        {
            return 3;
        }

        var flagdata = exp[1] + (exp[2] << 8);
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

    public int _07_CheckEntityInArea_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var entityId = exp[1];
        int x1 = exp[2];
        int x2 = exp[3];
        int y1 = exp[4];
        int y2 = exp[5];
        int z1 = exp[6];
        int z2 = exp[7];
        var numEntities = _gameEngine.GetEntityFromRefId(entity, entityId);
        for (var i = 0; i < numEntities; i++)
        {
            var checkme = StaticVariables.g_entitySlots[i];
            if (checkme.TileX >= x1 && checkme.TileX <= x2
                                    && checkme.TileY >= y1 && checkme.TileY <= y2
                                    && checkme.TileZ >= z1 && checkme.TileZ <= z2)
            {
                eventProgramState.Exp[9] = 1;
                return 8;
            }
        }

        eventProgramState.Exp[9] = 0;

        return 8;
    }

    public int _08_Turn_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        entity.TargetDirection = (uint)((entity.TargetDirection + exp[1]) & 0x1f);
        return 2;
    }

    public int _09_SetDir_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        entity.TargetDirection = (uint)(exp[1] & 0x1f);
        return 2;
    }

    public int _0a_Reverse_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        entity.TargetDirection = (entity.TargetDirection + 0x10) & 0x1f;
        return 1;
    }

    //set animation, and block until entity has moved specified distance
    public int _0b_AnimWaitDistance_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        uint animid = (uint)exp[1];
        entity.TargetAnimationId = animid;
        if (exp[0] != eventProgramState.Exp[1])
        {
            eventProgramState.Exp[1] = exp[0];
            eventProgramState.Exp[2] = entity.XPos;
            eventProgramState.Exp[3] = entity.YPos;
            return 0;
        }
        var difx = eventProgramState.Exp[2] - entity.XPos;
        var dify = eventProgramState.Exp[3] - entity.YPos;
        if (difx < 0)
        {
            difx = -difx;
        }

        if (dify < 0)
        {
            dify = -dify;
        }

        var distance = exp[2] + (exp[3] << 8);

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

    public int _0c_SetRandomDir_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var i = StaticVariables.g_gameRandomSeed;
        var val1 = (int)(i * 0x7d2b89dd);
        var val2 = (int)(0xe06a02e7 + val1);
        var val3 = (int)(((long)val2 * 4) >> 32);
        var dir = (uint)StaticVariables.g_cardinalDirectionTable[val3];//val3 here is a number between 0 and 3
        StaticVariables.g_gameRandomSeed = (uint)val2;
        entity.TargetDirection = dir;
        return 1;
    }

    public int _0d_Dialog_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
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

        //var ret = SetText(exp[1], exp[2]);

        //if (ret > 0)
        //    return 3;
        return 0;
    }

    public int _10_LoseControl_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        StaticVariables.g_playerControlFlags |= 0x4;
        return 1;
    }

    public int _11_GainControl_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        StaticVariables.g_playerControlFlags &= ~0x4;
        return 1;
    }

    public int _12_PlaySound1_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        //throw new Exception("impliment this one!");
        int soundid = exp[1];
        return 2;
    }

    public int _15_ResetZPos_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        if (entity.EntityRecord == null)
        {
            Debug.Print("No InitData");
        }

        entity.ZPos = (entity.EntityRecord.Height * 8 - entity.ZMod) << 16;
        return 1;
    }

    public int _16_GravityOn_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        entity.Flags |= 0x100;
        return 1;
    }

    public int _17_GravityOff_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var flag = ~0x100;
        entity.Flags &= (uint)flag;
        return 1;
    }

    public int _19_Deactivate_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        entity.Status = 3;
        return 1;
    }

    public int _1a_SetAnim_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        entity.TargetAnimationId = (uint)exp[1];
        return 2;
    }

    //sets zforce
    public int _1b_Fly_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var force = exp[1] + (exp[2] << 8);
        force = force << 16;//sign extend
        force = force >> 8;//get it to the correct multiple
        entity.ZForce = force;
        return 2;
    }

    public int _1c_WaitAnim_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        if (exp[0] != eventProgramState.Exp[1])
        {
            eventProgramState.Exp[1] = exp[0];
            eventProgramState.Exp[2] = 0;
            entity.AnimCompleteCounter = 0;
            return 0;
        }

        if (entity.ForceResetAnimationFlag != 0)
        {
            entity.CurrentAnimationId = ~entity.TargetAnimationId;
        }

        if (entity.ForceResetAnimationFlag != 0 || entity.AnimCompleteCounter != 0)
        {
            eventProgramState.Exp[2]++;
            entity.AnimCompleteCounter = 0;
        }


        var towait = exp[1];
        if (eventProgramState.Exp[2] >= towait)
        {
            return 2;
        }

        return 0;
    }

    //collision ends it
    public int _1d_WaitAnim2_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var ret = _1c_WaitAnim_Handler(entity, entitySelf, exp, eventProgramState);

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
    public int _1e_WaitWalk_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        if (exp[0] != eventProgramState.Exp[1])
        {
            eventProgramState.Exp[1] = exp[0];
            eventProgramState.Exp[2] = entity.XPos;
            eventProgramState.Exp[3] = entity.YPos;
            return 0;
        }

        var x = Math.Abs(eventProgramState.Exp[2] - entity.XPos) >> 16;
        var y = Math.Abs(eventProgramState.Exp[3] - entity.YPos) >> 16;

        var distance = exp[1] | (exp[2] << 8);

        if (x >= distance || y >= distance)
        {
            return 3;
        }

        return 0;
    }

    //wait until they have walked a certain distance, collision ends the walk
    public int _1f_WaitWalk2_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var ret = _1e_WaitWalk_Handler(entity, entitySelf, exp, eventProgramState);

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
    public int _24_WaitForceAdjusted_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        if (entity.ForceAdjusted > 0)
        {
            return 1;
        }

        return 0;
    }

    public int _25_WaitEntityCollisionZOr144_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
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

    public int _26_WaitForceAdjustedOrEntityCollisionZ_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
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

    public int _27_FacePlayer_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(StaticVariables.PlayerEntity.XPos - entity.XPos, StaticVariables.PlayerEntity.YPos - entity.YPos);
        return 1;
    }

    public int _28_Flag2On_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        entity.Flags |= 0x8;
        return 1;
    }

    public int _29_Flag2Off_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var flags = ~0x8;
        entity.Flags &= (uint)flags;
        return 1;
    }

    public int _2a_Flag3On_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        entity.Flags |= 0x1;
        return 1;
    }

    public int _2b_Flag3Off_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var flags = ~0x1;
        entity.Flags &= (uint)flags;
        return 1;
    }

    public int _2d_ActivateEntity_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var entityId = exp[1];
        var loaded = _gameEngine.ActivateEntity(entity, entityId, 1);
        if (loaded == null)
        {
            throw new Exception("Illegal InitData Number!!");
        }

        return 2;
    }

    public int _2e_Hide_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var entityId = exp[1];
        var numEntities = _gameEngine.GetEntityFromRefId(entity, entityId);
        for (var dex = 0; dex < numEntities; dex++)
        {
            var checkme = StaticVariables.g_entitySlots[dex];
            _gameEngine.EntityGameplayManager.HideEntity(checkme);
        }

        return 2;
    }

    public int _2f_CheckPlayerInput_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var inputid = exp[3];
        var mask = exp[1] | exp[2] << 8;

        //StaticVariables.g_padState1.ButtonsHold
        //if ((_gameEngine.PlayerInput[inputid] & mask) != 0)
        if ((StaticVariables.g_padState1.ButtonsJustPressed & mask) != 0)
        {
            eventProgramState.Exp[9] = 1;
        }
        else
        {
            eventProgramState.Exp[9] = 0;
        }

        return 4;
    }

    public int _30_IfFlagOff_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {

        var flagdata = exp[1] + (exp[2] << 8);
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
            int jumpoffset = (short)(exp[3] | exp[4] << 8);
            return jumpoffset;
        }

        return 5;
    }

    public int _31_IfFlagOn_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {

        var flagdata = exp[1] + (exp[2] << 8);
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
            int jumpoffset = (short)(exp[3] | exp[4] << 8);
            return jumpoffset;
        }

        return 5;
    }

    public int _32_FlagToggle_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        if (eventProgramState.Exp[9] != 0)
        {
            return 3;
        }

        var flagdata = exp[1] + (exp[2] << 8);
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

    public int _33_CheckFlagsOn_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        //do this 4 times
        {
            var flagdata = exp[1] + (exp[2] << 8);
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
                eventProgramState.Exp[9] = 0;
                return 9;
            }
        }

        {
            var flagdata = exp[3] + (exp[4] << 8);
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
                eventProgramState.Exp[9] = 0;
                return 9;
            }
        }

        {
            var flagdata = exp[5] + (exp[6] << 8);
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
                eventProgramState.Exp[9] = 0;
                return 9;
            }
        }

        {
            var flagdata = exp[7] + (exp[8] << 8);
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
                eventProgramState.Exp[9] = 0;
                return 9;
            }
        }


        eventProgramState.Exp[9] = 1;//made it through them all
        return 9;
    }

    public int _34_CheckFlagsOff_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        //do this 4 times
        {
            var flagdata = exp[1] + (exp[2] << 8);
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
                eventProgramState.Exp[9] = 0;
                return 9;
            }
        }

        {
            var flagdata = exp[3] + (exp[4] << 8);
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
                eventProgramState.Exp[9] = 0;
                return 9;
            }
        }

        {
            var flagdata = exp[5] + (exp[6] << 8);
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
                eventProgramState.Exp[9] = 0;
                return 9;
            }
        }

        {
            var flagdata = exp[7] + (exp[8] << 8);
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
                eventProgramState.Exp[9] = 0;
                return 9;
            }
        }


        eventProgramState.Exp[9] = 1;//made it through them all
        return 9;
    }


    //blocks until flag is off
    public int _35_UntilFlagOff_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {

        var flagdata = exp[1] + (exp[2] << 8);
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
    public int _36_UntilFlagOn_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {

        var flagdata = exp[1] + (exp[2] << 8);
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
    public int _37_Wait_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        if (exp[0] != eventProgramState.Exp[1])
        {
            eventProgramState.Exp[1] = exp[0];
            eventProgramState.Exp[2] = 0;
            return 0;
        }

        eventProgramState.Exp[2]++;

        var towait = exp[1];
        if (eventProgramState.Exp[2] >= towait)
        {
            return 2;
        }

        return 0;
    }

    public int _3b_CheckPlayerInArea_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        int x1 = exp[1];
        int x2 = exp[2];
        int y1 = exp[3];
        int y2 = exp[4];
        int z1 = exp[5];
        int z2 = exp[6];

        var checkme = StaticVariables.PlayerEntity;
        if (checkme.TileX >= x1 && checkme.TileX <= x2
                                && checkme.TileY >= y1 && checkme.TileY <= y2
                                && checkme.TileZ >= z1 && checkme.TileZ <= z2)
        {
            eventProgramState.Exp[9] = 1;
            return 7;
        }


        eventProgramState.Exp[9] = 0;

        return 7;
    }

    public int _40_SetProgramIndex_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var programid = exp[1];
        var indexval = exp[2];

        //somevariable = 1;
        StaticVariables.g_clearProgramState = 1;

        entity.ProgramIndexes[programid] = indexval;

        return 3;
    }

    public int _41_SetSpriteProgramIndex_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var programid = exp[1];
        var indexval = exp[2];

        entity.SpriteProgramIndexes[programid] = indexval;

        return 3;
    }

    public int _45_Flag4Off_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        entity.Flags &= ~(uint)0x2000;
        return 1;
    }

    public int _46_Flag4On_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        entity.Flags |= 0x2000;
        return 1;
    }

    public int _49_Restart_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        return eventProgramState.Exp[0] - entity.EventProgramState.Sp;
    }

    public int _4a_IfTrueRestart_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        if (eventProgramState.Exp[9] != 0)
        {
            return eventProgramState.Exp[0] - entity.EventProgramState.Sp;
        }

        return 1;
    }

    public int _4b_IfFalseRestart_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        if (eventProgramState.Exp[9] == 0)
        {
            return eventProgramState.Exp[0] - entity.EventProgramState.Sp;
        }

        return 1;
    }

    public int _54_SetWalkable_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        int tilex = exp[1];
        int tiley = exp[2];
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

        var walkabilitybits = (byte)exp[3];
        var groundpropertybits = (byte)exp[4];
        var tile = _gameEngine.CurrentMap.Map.MapTiles[tilex + tiley * 52];

        tile.Walkability |= walkabilitybits;
        tile.GroundProperty |= groundpropertybits;

        return 5;
    }

    public int _55_SetNonWalkable_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        int tilex = exp[1];
        int tiley = exp[2];
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

        var walkabilitybits = exp[3];
        var groundpropertybits = exp[4];
        var tile = _gameEngine.CurrentMap.Map.MapTiles[tilex + tiley * 52];

        tile.Walkability &= (byte)~walkabilitybits;
        tile.GroundProperty &= (byte)~groundpropertybits;

        return 5;
    }

    public int _58_DirectionalBranch_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var dir = entity.CurrentFrameIndex;
        var jumpoffset = (short)(exp[entity.CurrentFrameIndex * 2 + 1] | exp[entity.CurrentFrameIndex * 2 + 2]);
        return jumpoffset;
    }

    public int _59_SetEntityAnim_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var entityId = exp[1];
        int animid = exp[2];
        var numEntities = _gameEngine.GetEntityFromRefId(entity, entityId);
        for (var i = 0; i < numEntities; i++)
        {
            var dome = StaticVariables.g_entitySlots[i];
            dome.TargetAnimationId = (uint)animid;
        }

        return 3;
    }

    public int _5a_TurnEntity_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var entityId = exp[1];
        var turncode = exp[2];

        var numEntities = _gameEngine.GetEntityFromRefId(entity, entityId);
        for (var i = 0; i < numEntities; i++)
        {
            var dome = StaticVariables.g_entitySlots[i];
            dome.TargetDirection = _gameEngine.EntityGameplayManager.TurnEntity(entity, turncode);
        }

        return 3;
    }

    public int _5b_TurnEntityWithAnim_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var entityId = exp[1];
        int animid = exp[2];
        var turncode = exp[3];

        var numEntities = _gameEngine.GetEntityFromRefId(entity, entityId);
        for (var i = 0; i < numEntities; i++)
        {
            var dome = StaticVariables.g_entitySlots[i];
            dome.TargetAnimationId = (uint)animid;
            dome.TargetDirection = _gameEngine.EntityGameplayManager.TurnEntity(entity, turncode);
        }

        return 4;
    }

    public int _62_EntityFlagsOn_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var entityId = exp[1];
        var flagbits = exp[2] + (exp[3] << 8);

        var numEntities = _gameEngine.GetEntityFromRefId(entity, entityId);
        for (var i = 0; i < numEntities; i++)
        {
            var dome = StaticVariables.g_entitySlots[i];
            dome.Flags |= (uint)flagbits;
        }

        return 4;
    }

    public int _63_EntityFlagsOff_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var entityId = exp[1];
        var flagbits = exp[2] + (exp[3] << 8);

        var numEntities = _gameEngine.GetEntityFromRefId(entity, entityId);
        for (var i = 0; i < numEntities; i++)
        {
            var dome = StaticVariables.g_entitySlots[i];
            dome.Flags &= ~(uint)flagbits;
        }

        return 4;
    }

    public int _64_SetEntityPos_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var entityId = exp[1];
        var x = (exp[2] + (exp[3] << 8)) << 16;
        var y = (exp[4] + (exp[5] << 8)) << 16;
        var z = (exp[6] + (exp[7] << 8)) << 16;

        var numEntities = _gameEngine.GetEntityFromRefId(entity, entityId);
        for (var i = 0; i < numEntities; i++)
        {
            var entity2 = StaticVariables.g_entitySlots[i];
            entity2.XPos = x;
            entity2.YPos = y;
            entity2.ZPos = z + 1;
        }

        return 8;
    }

    public int _65_MoveEntityPos_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var entityId = exp[1];
        var x = (exp[2] + (exp[3] << 8)) << 16;
        var y = (exp[4] + (exp[5] << 8)) << 16;
        var z = (exp[6] + (exp[7] << 8)) << 16;

        var numEntities = _gameEngine.GetEntityFromRefId(entity, entityId);
        for (var i = 0; i < numEntities; i++)
        {
            var dome = StaticVariables.g_entitySlots[i];
            dome.XPos += x;
            dome.YPos += y;
            dome.ZPos += z;
        }

        return 8;
    }

    public int _67_CamFollowEntity_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var entityId = exp[1];

        var numEntities = _gameEngine.GetEntityFromRefId(entity, entityId);

        StaticVariables.g_entityFollowedByCamera = StaticVariables.g_entitySlots[0];

        return 2;
    }

    public int _70_Check144_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        eventProgramState.Exp[9] = entity.IsAboveGround;

        return 2;
    }

    public int _90_CreateEffect_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var effectid = (byte)exp[1];
        _gameEngine.EffectManager.GameEngine.CreateEffect_MapType(effectid, true, _gameEngine.EffectManager);
        return 2;
    }

    public int _91_DisableEffect_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var effectid = exp[1];
        foreach (var effect in StaticVariables.g_effectSlots)
        {
            if (effect.Status != 0 && effect.MapEffectId == effectid)
            {
                effect.Status = 0;
            }
        }

        return 2;
    }

    public int _92_SetEffectAnim_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var effectid = exp[1];
        var animid = (byte)exp[2];
        foreach (var effect in StaticVariables.g_effectSlots)
        {
            if (effect.Status != 0 && effect.MapEffectId == effectid)
            {
                effect.TargetAnimation = animid;
            }
        }

        return 3;
    }

    public int _93_SetEffectPos_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var effectid = exp[1];
        var x = (exp[2] | exp[3] << 8) << 16;
        var y = (exp[4] | exp[5] << 8) << 16;
        var z = (exp[6] | exp[7] << 8) << 16;
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

    public int _94_SetEffectForces_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var effectid = exp[1];
        var x = (exp[2] | exp[3] << 8) << 16;
        var y = (exp[4] | exp[5] << 8) << 16;
        var z = (exp[6] | exp[7] << 8) << 16;
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

    public int _a0_AdjustEffectPos_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var effectid = exp[1];
        var x = (exp[2] | exp[3] << 8) << 16;
        var y = (exp[4] | exp[5] << 8) << 16;
        var z = (exp[6] | exp[7] << 8) << 16;
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

    public int _a1_AdjustEffectPosWithEntity_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var effectid = exp[1];
        var entityId = exp[2];

        var numEntities = _gameEngine.GetEntityFromRefId(entity, entityId);
        if (numEntities == 0)
        {
            return 9;
        }

        var refentity = StaticVariables.g_entitySlots[0];

        var x = (exp[3] | exp[4] << 8) << 16;
        var y = (exp[5] | exp[6] << 8) << 16;
        var z = (exp[7] | exp[8] << 8) << 16;

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

    public int _a2_CreateEffectWithPos_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var effectid = (byte)exp[1];
        var effect = _gameEngine.EffectManager.GameEngine.CreateEffect_MapType(effectid, true, _gameEngine.EffectManager);
        if (effect == null)
        {
            return 8;
        }

        var x = (exp[2] | exp[3] << 8) << 16;
        var y = (exp[4] | exp[5] << 8) << 16;
        var z = (exp[6] | exp[7] << 8) << 16;

        effect.X = x;
        effect.Y = y;
        effect.Z = z;

        return 8;
    }

    public int _a3_CreateEffectWithEntityPos_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var effectid = (byte)exp[1];
        var entityId = exp[2];
        var numEntities = _gameEngine.GetEntityFromRefId(entity, entityId);
        if (numEntities == 0)
        {
            return 9;
        }

        var effect = _gameEngine.EffectManager.GameEngine.CreateEffect_MapType(effectid, true, _gameEngine.EffectManager);
        if (effect == null)
        {
            return 9;
        }

        var x = (exp[3] | exp[4] << 8) << 16;
        var y = (exp[5] | exp[6] << 8) << 16;
        var z = (exp[7] | exp[8] << 8) << 16;

        effect.X = entity.XPos + x;
        effect.Y = entity.YPos + y;
        effect.Z = entity.ZPos + z;

        return 9;
    }
}