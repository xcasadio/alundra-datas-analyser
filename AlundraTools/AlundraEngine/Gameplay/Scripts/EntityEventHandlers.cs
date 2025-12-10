using AlundraEngine.DatasBin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace AlundraEngine.Gameplay.Scripts;

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
            _handlers.Add(i, _Unknown_Handler);
        }

        _handlers[0x00] = Script_DoNothing;
        _handlers[0x01] = Script_DoNothing;
        _handlers[0x02] = Script_2_002;
        _handlers[0x03] = Script_3_003;
        _handlers[0x04] = Script_4_004;
        _handlers[0x05] = Script_5_005;
        _handlers[0x06] = Script_6_006;
        _handlers[0x07] = Script_7_007;
        _handlers[0x08] = Script_8_008;
        _handlers[0x09] = Script_9_009;
        _handlers[0x0A] = Script_10_00A;
        _handlers[0x0B] = Script_11_00B;
        _handlers[0x0C] = Script_12_00C;
        _handlers[0x0D] = Script_OpenDialog_13_00D;
        _handlers[0x0E] = Script_DoNothing;
        _handlers[0x0F] = Script_DoNothing;
        _handlers[0x10] = Script_16_010;
        _handlers[0x11] = Script_17_011;
        _handlers[0x12] = Script_18_012;
        _handlers[0x13] = Script_DoNothing;
        _handlers[0x14] = Script_DoNothing2;
        _handlers[0x15] = Script_21_015;
        _handlers[0x16] = Script_22_016;
        _handlers[0x17] = Script_23_017;
        _handlers[0x18] = Script_DoNothing;
        _handlers[0x19] = Script_25_019;
        _handlers[0x1A] = Script_26_01A;
        _handlers[0x1B] = Script_27_01B;
        _handlers[0x1C] = Script_28_01C;
        _handlers[0x1D] = Script_29_01D;
        _handlers[0x1E] = Script_30_01E;
        _handlers[0x1F] = Script_31_01F;
        _handlers[0x20] = Script_32_020;
        _handlers[0x21] = Script_33_021;
        _handlers[0x22] = Script_34_022;
        _handlers[0x23] = Script_35_023;
        _handlers[0x24] = Script_36_024;
        _handlers[0x25] = Script_37_025;
        _handlers[0x26] = Script_38_026;
        _handlers[0x27] = Script_39_027;
        _handlers[0x28] = Script_40_028;
        _handlers[0x29] = Script_41_029;
        _handlers[0x2A] = Script_42_02A;
        _handlers[0x2B] = Script_43_02B;
        _handlers[0x2C] = Script_44_02C;
        _handlers[0x2D] = Script_45_02D;
        _handlers[0x2E] = Script_46_02E;
        _handlers[0x2F] = Script_47_02F;
        _handlers[0x30] = Script_48_030;
        _handlers[0x31] = Script_49_031;
        _handlers[0x32] = Script_50_032;
        _handlers[0x33] = Script_51_033;
        _handlers[0x34] = Script_52_034;
        _handlers[0x35] = Script_53_035;
        _handlers[0x36] = Script_54_036;
        _handlers[0x37] = Script_55_037;
        _handlers[0x38] = Script_56_038;
        _handlers[0x39] = Script_57_039;
        _handlers[0x3A] = Script_58_03A;
        _handlers[0x3B] = Script_59_03B;
        _handlers[0x3C] = Script_60_03C;
        _handlers[0x3D] = Script_61_03D;
        _handlers[0x3E] = Script_62_03E;
        _handlers[0x3F] = Script_63_03F;
        _handlers[0x40] = Script_64_040;
        _handlers[0x41] = Script_65_041;
        _handlers[0x42] = Script_66_042;
        _handlers[0x43] = Script_67_043;
        _handlers[0x44] = Script_68_044;
        _handlers[0x45] = Script_69_045;
        _handlers[0x46] = Script_70_046;
        _handlers[0x47] = Script_71_047;
        _handlers[0x48] = Script_72_048;
        _handlers[0x49] = Script_73_049;
        _handlers[0x4A] = Script_74_04A;
        _handlers[0x4B] = Script_75_04B;
        _handlers[0x4C] = Script_76_04C;
        _handlers[0x4D] = Script_77_04D;
        _handlers[0x4E] = Script_78_04E;
        _handlers[0x4F] = Script_ActivateDebugTextAutoAdvance;
        _handlers[0x50] = Script_SetEtcAnimationMode;
        _handlers[0x51] = Script_TryActivateTextHoldState;
        _handlers[0x52] = Script_82_052;
        _handlers[0x53] = Script_83_053;
        _handlers[0x54] = Script_84_054;
        _handlers[0x55] = Script_85_055;
        _handlers[0x56] = Script_86_056;
        _handlers[0x57] = Script_87_057;
        _handlers[0x58] = Script_88_058;
        _handlers[0x59] = Script_89_059;
        _handlers[0x5A] = Script_90_05A;
        _handlers[0x5B] = Script_91_05B;
        _handlers[0x5C] = Script_OpenDialogWithChoice;
        _handlers[0x5D] = Script_93_05D;
        _handlers[0x5E] = Script_94_05E;
        _handlers[0x5F] = Script_WaitForAnimOrDistance;
        _handlers[0x60] = Script_96_060;
        _handlers[0x61] = Script_97_061;
        _handlers[0x62] = Script_98_062;
        _handlers[0x63] = Script_99_063;
        _handlers[0x64] = Script_100_064;
        _handlers[0x65] = Script_101_065;
        _handlers[0x66] = Script_CopyLogicContextAndAssignScript;
        _handlers[0x67] = Script_103_067;
        _handlers[0x68] = Script_104_068;
        _handlers[0x69] = Script_105_069;
        _handlers[0x6A] = Script_106_06A;
        _handlers[0x6B] = Script_107_06B;
        _handlers[0x6C] = Script_108_06C;
        _handlers[0x6D] = Script_109_06D;
        _handlers[0x6E] = Script_110_06E;
        _handlers[0x6F] = Script_111_06F;
        _handlers[0x70] = Script_112_070;
        _handlers[0x71] = Script_113_071;
        _handlers[0x72] = Script_114_072;
        _handlers[0x73] = Script_115_073;
        _handlers[0x74] = Script_116_074;
        _handlers[0x75] = Script_117_075;
        _handlers[0x76] = Script_118_076;
        _handlers[0x77] = Script_119_077;
        _handlers[0x78] = Script_StoreChoiceParamAndJump;
        _handlers[0x79] = Script_JumpIfChoiceAccepted;
        _handlers[0x7A] = Script_JumpIfChoiceRejected;
        _handlers[0x7B] = Script_JumpIfFlagSetStoreParam;
        _handlers[0x7C] = Script_JumpIfFlagClearStoreParam;
        _handlers[0x7D] = Script_JumpRelativeFromStoredParam;
        _handlers[0x7E] = Script_ConditionalJumpFromStoredParamIfTrue;
        _handlers[0x7F] = Script_ConditionalJumpFromStoredParamIfFalse;
        _handlers[0x80] = Script_JumpFromStoredParamIfFlagSet;
        _handlers[0x81] = Script_JumpFromStoredParamIfFlagClear;
        _handlers[0x82] = Script_130_082;
        _handlers[0x83] = Script_131_083;
        _handlers[0x84] = Script_132_084;
        _handlers[0x85] = Script_133_085;
        _handlers[0x86] = Script_134_086;
        _handlers[0x87] = Script_135_087;
        _handlers[0x88] = Script_136_088;
        _handlers[0x89] = Script_137_089;
        _handlers[0x8A] = Script_138_08A;
        _handlers[0x8B] = Script_139_08B;
        _handlers[0x8C] = Script_140_08C;
        _handlers[0x8D] = Script_141_08D;
        _handlers[0x8E] = Script_142_08E;
        _handlers[0x8F] = Script_143_08F;
        _handlers[0x90] = Script_144_090;
        _handlers[0x91] = Script_145_091;
        _handlers[0x92] = Script_146_092;
        _handlers[0x93] = Script_147_093;
        _handlers[0x94] = Script_148_094;
        _handlers[0x95] = Script_149_095;
        _handlers[0x96] = Script_150_096;
        _handlers[0x97] = Script_151_097;
        _handlers[0x98] = Script_152_098;
        _handlers[0x99] = Script_153_099;
        _handlers[0x9A] = Script_154_09A;
        _handlers[0x9B] = Script_155_09B;
        _handlers[0x9C] = Script_156_09C;
        _handlers[0x9D] = Script_157_09D;
        _handlers[0x9E] = Script_158_09E;
        _handlers[0x9F] = Script_159_09F;
        _handlers[0xA0] = Script_160_0A0;
        _handlers[0xA1] = Script_161_0A1;
        _handlers[0xA2] = Script_162_0A2;
        _handlers[0xA3] = Script_163_0A3;
        _handlers[0xA4] = Script_164_0A4;
        _handlers[0xA5] = Script_165_0A5;
        _handlers[0xA6] = Script_166_0A6;
        _handlers[0xA7] = Script_167_0A7;
        _handlers[0xA8] = Script_168_0A8;
        _handlers[0xA9] = Script_169_0A9;
        _handlers[0xAA] = Script_170_0AA;
        _handlers[0xAB] = Script_171_0AB;
        _handlers[0xAC] = Script_172_0AC;
        _handlers[0xAD] = Script_173_0AD;
        _handlers[0xAE] = Script_174_0AE;
        _handlers[0xAF] = Script_175_0AF;
        _handlers[0xB0] = Script_176_0B0;
        _handlers[0xB1] = Script_177_0B1;
        _handlers[0xB2] = Script_CompareEntityGroupsForMatch;
        _handlers[0xB3] = Script_UpdatePadState;
        _handlers[0xB4] = Script_180_0B4;
        _handlers[0xB5] = Script_181_0B5;
        _handlers[0xB6] = Script_182_0B6;
        _handlers[0xB7] = Script_183_0B7;
        _handlers[0xB8] = Script_184_0B8;
        _handlers[0xB9] = Script_185_0B9;
        _handlers[0xBA] = Script_186_0BA;
        _handlers[0xBB] = Script_187_0BB;
        _handlers[0xBC] = Script_188_0BC;
        _handlers[0xBD] = Script_189_0BD;
        _handlers[0xBE] = Script_190_0BE;
        _handlers[0xBF] = Script_191_0BF;
        _handlers[0xC0] = Script_192_0C0;
        _handlers[0xC1] = Script_193_0C1;
        _handlers[0xC2] = Script_194_0C2;
        _handlers[0xC3] = Script_195_0C3;
        _handlers[0xC4] = Script_196_0C4;
    }

    //8004205c
    public void RunEntityEventScripts(Entity entity, int logicMode)
    {
        EventProgramState eventProgramState = _gameEngine.StaticVariables.g_eventProgramState;

        //var isDebug = _gameEngine.StaticVariables.g_debugState < 0;
        //var isDebugLogicTraceEnabled = (_gameEngine.StaticVariables.g_debugFlags & 0x10) != 0;

        if (logicMode < 6)
        {
            switch (logicMode)
            {
                case ScriptHelper.ProgramBMap:
                    eventProgramState = entity.EventProgramState;

                    if (eventProgramState.Codes != null) //.Parameters[0] != 0 && eventProgramState.Sp != 0)
                    {
                        goto SET_LOGIC_MODE;
                    }
                    break;

                case ScriptHelper.ProgramCTick:
                    eventProgramState = entity.EventProgramState;

                    if (eventProgramState.Codes != null) //Parameters[0] != 0 && eventProgramState.Sp != 0)
                    {
                        if (entity.MapEventProgramId != 2)
                        {
                            entity.TargetAnimationId = entity.LastTargetAnimationId;
                            entity.TargetDirection = entity.LastTargetDirection;
                        }

                        goto SET_LOGIC_MODE;
                    }

                    break;

                case ScriptHelper.ProgramFInteract:

                    _gameEngine.StaticVariables.PlayerEntity.ForceStepY = 0;
                    _gameEngine.StaticVariables.PlayerEntity.ForceStepX = 0;
                    _gameEngine.StaticVariables.PlayerEntity.ForceY = 0;
                    _gameEngine.StaticVariables.PlayerEntity.ForceX = 0;
                    break;

                default:
                    if (entity.MapEventProgramId == ScriptHelper.ProgramCTick)
                    {
                        entity.LastTargetAnimationId = entity.TargetAnimationId;
                        entity.LastTargetDirection = entity.TargetDirection;
                    }
                    break;
            }
        }
        else
        {
            Debugger.Break();
            //throw new Exception("Illegal logic entry!");
        }

        InitializeEventData(entity, logicMode, eventProgramState);

        SET_LOGIC_MODE:
        entity.MapEventProgramId = logicMode;

        END_LOGIC_SETUP:
        var wasEntityCleared = false;
        _gameEngine.StaticVariables.g_activeCommand = -1;
        _gameEngine.StaticVariables.g_activeEventProgramIndex = entity.ProgramIndexes[logicMode];
        _gameEngine.StaticVariables.g_clearProgramState = 0;
        _gameEngine.StaticVariables.g_activeEntityRefId = entity.EntityRefId;
        _gameEngine.StaticVariables.g_activeEventProgramType = logicMode;

        while (true)
        {
            int[] variables = FillDataFromCommand(eventProgramState);
            int command = variables[0];

            //LogCommand(entity, logicMode, command, variables, eventProgramState.CodeIndex);

            if (command == 0xFF)
            {
                //Debug.WriteLine("");
                goto END_SCRIPT;
            }

            if (command == 0x00) // break, skip the loop but do the next command
            {
                eventProgramState.Parameters[1] = 0;
                eventProgramState.CodeIndex++;
                FillDataFromCommand(eventProgramState); // needed because there is a check at the beginning of the function
                //Debug.WriteLine("");
                goto END_SCRIPT;
            }

            var logicContextEntity = entity.LogicContextEntity;
            _gameEngine.StaticVariables.g_lastCommand = _gameEngine.StaticVariables.g_activeCommand;
            _gameEngine.StaticVariables.g_activeCommand = command;

            var func = _handlers[command];
            var result = func(entity.LogicContextEntity, entity, variables, eventProgramState);

            //Debug.WriteLine($" = {result}");

            if (_gameEngine.StaticVariables.g_clearProgramState != 0)
            {
                if (logicContextEntity == entity)
                {
                    wasEntityCleared = true;
                }
                else
                {
                    _gameEngine.StaticVariables.g_clearProgramState = 0;
                    //Debug.WriteLine($"Entity[{logicContextEntity.Index}] clean EventProgramState");
                    logicContextEntity.EventProgramState.Sp = 0;
                    logicContextEntity.EventProgramState.Codes = null;
                }
            }

            if (result == 0)
            {
                goto END_SCRIPT;
            }

            eventProgramState.Parameters[1] = 0;
            eventProgramState.CodeIndex += result;

            if (eventProgramState.CodeIndex < 0)
            {
                Debugger.Break();
            }
        }

        END_SCRIPT:
        if (wasEntityCleared)
        {
            //Debug.WriteLine($"Entity[{entity.Index}] clean EventProgramState 2");
            eventProgramState.Sp = 0;
            eventProgramState.Codes = null;
        }
    }

    private static void LogCommand(Entity entity, int logicMode, int command, int[] variables, int codeIndex)
    {
        var name = SpriteInfoEventCodes.CommandNameByCode.GetValueOrDefault((byte)command, "?");
        var eventTypeName = logicMode == 0 ? "ALoad" : logicMode == 1 ? "BMap" : logicMode == 2 ? "CTick" : logicMode == 3 ? "DTouch" : logicMode == 4 ? "EDeactivate" : "FInteract";
        Debug.Write($"Entity[{entity.Index}] run[{eventTypeName}] 0x{command:x2} p:{codeIndex} '{name}' {string.Join(',', variables.Select(x => x.ToString("x2")))}");
    }

    private int[] FillDataFromCommand(EventProgramState eventProgramState)
    {
        if (eventProgramState.Codes == null || eventProgramState.CodeIndex >= eventProgramState.Codes.Length)
        {
            return [0xFF];
        }

        if (eventProgramState.CodeIndex < 0)
        {
            Debugger.Break();
        }

        eventProgramState.Sp = eventProgramState.Codes[eventProgramState.CodeIndex];
        int[] variables = new int[10];
        var length = Math.Min(10, eventProgramState.Codes.Length - eventProgramState.CodeIndex);
        Array.Copy(eventProgramState.Codes, eventProgramState.CodeIndex, variables, 0, length);
        return variables;
    }

    private void InitializeEventData(Entity entity, int eventProgramType, EventProgramState eventProgramState)
    {
        var codeIndex = entity.ProgramIndexes[eventProgramType];
        var spriteInfo = _gameEngine.AlundraMap.SpriteInfo;

        if ((codeIndex & 0x80) != 0)
        {
            spriteInfo = _gameEngine.CurrentMap.SpriteInfo;
        }

        using var br = _gameEngine.DatasBin.OpenBin();

        short[] eventCodesTable = eventProgramType switch
        {
            ScriptHelper.ProgramALoad => spriteInfo.EventCodes.EventCodesATable,
            ScriptHelper.ProgramBMap => spriteInfo.EventCodes.EventCodesBTable,
            ScriptHelper.ProgramCTick => spriteInfo.EventCodes.EventCodesCTable,
            ScriptHelper.ProgramDTouch => spriteInfo.EventCodes.EventCodesDTable,
            ScriptHelper.ProgramEDeactivate => spriteInfo.EventCodes.EventCodesETable,
            ScriptHelper.ProgramFInteract => spriteInfo.EventCodes.EventCodesFTable,
            _ => throw new InvalidOperationException()
        };

        eventProgramState.CodeIndex = 0;
        Array.Clear(eventProgramState.Parameters);

        //var commands = GetEventCodeCommands(br, entity.ProgramIndexes[eventProgramType], eventCodesTable, spriteInfo);
        eventProgramState.Codes = GetEventCodes(br, entity.ProgramIndexes[eventProgramType], eventCodesTable, spriteInfo);
        eventProgramState.Sp = eventProgramState.Codes?.Length > 0 ? eventProgramState.Codes[eventProgramState.CodeIndex] : 0;
        //eventProgramState.Parameters[0] = eventProgramState.Sp;
        eventProgramState.Parameters[0] = eventProgramState.CodeIndex;
    }

    public static List<SiCommand> GetEventCodeCommands(BinaryReader br, int index, short[] eventCodesTable, SpriteInfo spriteInfo)
    {
        if (index >= 0 && index < 0xff)
        {
            var i = index & 0x7f;

            if (i < eventCodesTable.Length - 2)
            {
                var j = i + 1;
                while (j < eventCodesTable.Length - 1 && eventCodesTable[j] == 0)
                {
                    j++;
                }

                var size = eventCodesTable[j] - eventCodesTable[i];
                return spriteInfo?.EventCodes?.GetCommands(br, eventCodesTable[i], true, size);
            }

            return spriteInfo?.EventCodes?.GetCommands(br, eventCodesTable[i]);
        }

        return [];
    }


    public static byte[] GetEventCodes(BinaryReader br, int index, short[] eventCodesTable, SpriteInfo spriteInfo)
    {
        if (index >= 0 && index < 0xff)
        {
            var i = index & 0x7f;
            if (i < eventCodesTable.Length)
            {
                return spriteInfo?.EventCodes?.GetByteCode(br, eventCodesTable[i]);
            }
        }
        return [];
    }

    //All Script_xxx functions

    //8003D158
    public int _Unknown_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        //Debug.WriteLine("Data Logic Error!");
        return 0;
    }

    // 8003D158
    private int Script_DoNothing(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        //_gameEngine.PrintCommandMap();
        return 0;
    }

    // 8003D6EC
    private int Script_DoNothing2(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        //_gameEngine.PrintCommandMap();
        return 0;
    }

    // 8003D17C
    private int Script_2_002(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        return (((variables[2] << 8) | variables[1]) * 0x10000) >> 0x10;
    }

    // 8003D1A0
    private int Script_3_003(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var result = 3;

        if (eventProgramState.Result != 0)
        {
            result = (((variables[2] << 8) | variables[1]) * 0x10000) >> 0x10;
        }

        return result;
    }

    // 8003D1D8
    private int Script_4_004(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var result = 3;

        if (eventProgramState.Result == 0)
        {
            result = (((variables[2] << 8) | variables[1]) * 0x10000) >> 0x10;
        }

        return result;
    }

    // 8003D210
    private int Script_5_005(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        uint[] flags;

        var flag = (uint)((variables[2] << 8) | variables[1]);

        if ((flag & 0x8000) == 0)
        {
            flags = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            flags = _gameEngine.StaticVariables.g_globalFlags;
        }

        var index = (flag >> 3) & 0xffc;
        var mask = (uint)(1 << (variables[1] & 0x1f));
        flags[index] |= mask;

        return 3;
    }

    // 8003D288
    private int Script_6_006(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        uint[] flags;

        var flag = (uint)((variables[2] << 8) | variables[1]);

        if ((flag & 0x8000) == 0)
        {
            flags = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            flags = _gameEngine.StaticVariables.g_globalFlags;
        }

        var index = flag >> 3 & 0xffc;
        flags[index] = (uint)(flags[index] & ~(1 << (variables[1] & 0x1f)));

        return 3;
    }

    // 8003D308
    private int Script_7_007(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var xmin = variables[2];
        var xmax = variables[3];
        var ymin = variables[4];
        var ymax = variables[5];
        var zmin = variables[6];
        var zmax = variables[7];

        var val = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (0 < val)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[val - 1];

            do
            {
                var previousEntity = entity;

                if (xmin <= previousEntity.TileX
                    && previousEntity.TileX <= xmax
                    && ymin <= previousEntity.TileY
                    && previousEntity.TileY <= ymax
                    && zmin <= previousEntity.TileZ
                    && previousEntity.TileZ <= zmax)
                {
                    eventProgramState.Result = 1;
                    return 8;
                }

                val -= 1;
                entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[val];
            } while (0 < val);
        }

        eventProgramState.Result = 0;

        return 8;
    }

    // 8003D404
    private int Script_8_008(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.TargetDirection = (uint)(logicEntity.TargetDirection + variables[1] & 0x1f);
        return 2;
    }

    // 8003D42C
    private int Script_9_009(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.TargetDirection = (uint)(variables[1] & 0x1f);
        return 2;
    }

    // 8003D44C
    private int Script_10_00A(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.TargetDirection = (logicEntity.TargetDirection + 0x10) & 0x1f;
        return 1;
    }

    // 8003D468
    private int Script_11_00B(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int result;

        logicEntity.TargetAnimationId = (uint)variables[1];

        if (eventProgramState.Parameters[1] == variables[0])
        {
            var x = eventProgramState.Parameters[2] - logicEntity.PosX;
            var y = eventProgramState.Parameters[3] - logicEntity.PosY;

            if (x < 0)
            {
                x = -x;
            }

            if (y < 0)
            {
                y = -y;
            }

            var uVar2 = (uint)eventProgramState.Parameters[3];
            result = 0;
            if ((int)uVar2 <= x >> 0x10 || (int)uVar2 <= y >> 0x10)
            {
                result = 4;
            }
        }
        else
        {
            eventProgramState.Parameters[1] = variables[0];
            eventProgramState.Parameters[2] = logicEntity.PosX;
            eventProgramState.Parameters[3] = logicEntity.PosY;
            result = 0;
        }

        return result;
    }

    // 8003D518
    private int Script_12_00C(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
        logicEntity.TargetDirection = (uint)_gameEngine.StaticVariables.g_cardinalDirectionTable[(uint)((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 4 >> 0x20)];
        return 1;
    }

    // 8003D578
    //open dialog
    private int Script_OpenDialog_13_00D(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        if ((logicEntity.Flags & 0x800000U) != 0) // has portrait
        {
            using var binaryReader = _gameEngine.DatasBin.OpenBin();
            var imgset = logicEntity.SpriteRecord.GetPortraitImageset(binaryReader);
            var img = imgset.Images[0];
            var bitmap = _gameEngine.CurrentMap.GenerateSpriteBitmap(img,
                    _gameEngine.CurrentMap.SpriteInfo.Palettes[img.Palette & 0x1f]);

            _gameEngine.MainInventoryManager.StartHudTransition(
                logicEntity.PosX, logicEntity.PosY, logicEntity.PosZ,
                _gameEngine.StaticVariables.g_cameraScrollingX, _gameEngine.StaticVariables.g_cameraScrollingY,
                img.Sx, img.Sy, img.Swidth, img.Sheight,
                /*_gameEngine.StaticVariables.g_drawPageInfoBase[logicEntity.SpriteSheetOffset + ((img.Spritesheet) & 0x3f)],
                _gameEngine.StaticVariables.g_tPageFadeLUT[logicEntity.PaletteOffset + ((img.Palette) & 7)]*/
                bitmap);
        }

        //Debugger.Break();
        _gameEngine.TriggerVisualUpdate((int)logicEntity.SpriteTableIndex);
        var res = _gameEngine.TryOpenDialog((uint)variables[1], variables[2]);
        // SetText(exp[1], exp[2]);

        if (res == 0)
        {
            return 0;
        }

        return 3;
    }

    // 8003D688
    private int Script_16_010(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        //dialog ??
        _gameEngine.StaticVariables.g_playerControlFlags |= 4;
        return 1;
    }

    // 8003D6A4
    private int Script_17_011(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_playerControlFlags &= 0xfffffffb;
        return 1;
    }

    // 8003D6C0
    private int Script_18_012(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.SoundManager.PlaySoundEffect((uint)variables[1]);
        return 2;
    }

    // 8003D710
    private int Script_21_015(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var entityRecord = logicEntity.EntityRecord;

        if (entityRecord == null)
        {
            //_gameEngine.PrintCommandMap();
        }

        logicEntity.PosZ = (int)(((uint)entityRecord.Height * 8 - logicEntity.ModZ) * 0x10000);
        return 1;
    }

    // 8003D774
    private int Script_22_016(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.Flags |= 0x100;
        return 1;
    }

    // 8003D78C
    private int Script_23_017(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.Flags &= 0xfffffeff;
        return 1;
    }

    // 8003D7A4
    private int Script_25_019(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.Status = 3;
        return 1;
    }

    // 8003D7B4
    private int Script_26_01A(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.TargetAnimationId = (uint)variables[1];
        return 2;
    }

    // 8003D7D0
    private int Script_27_01B(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.ForceZ = (((variables[2] << 8) | variables[1]) * 0x10000) >> 8;
        return 3;
    }

    // 8003D7FC
    private int Script_28_01C(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        if (eventProgramState.Parameters[1] != variables[0])
        {
            eventProgramState.Parameters[1] = variables[0];
            eventProgramState.Parameters[2] = 0;
            logicEntity.AnimCompleteCounter = 0;
            return 0;
        }

        if (logicEntity.ForceResetAnimationFlag == 0)
        {
            if (logicEntity.AnimCompleteCounter == 0)
            {
                goto LAB_8003d868;
            }
        }
        else
        {
            logicEntity.CurrentAnimationId = ~logicEntity.TargetAnimationId;
        }

        eventProgramState.Parameters[2] += 1;
        logicEntity.AnimCompleteCounter = 0;

        LAB_8003d868:
        return (eventProgramState.Parameters[2] < (variables[1] ^ 1) ? 1 : 0) << 1;
    }

    // 8003D890
    private int Script_29_01D(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var iVar1 = Script_28_01C(logicEntity, ownerEntity, variables, eventProgramState);
        var iVar2 = iVar1;
        iVar2 = 2;

        if (iVar1 == 0 && logicEntity.ForceAdjusted == 0)
        {
            iVar2 = iVar1;
        }

        return iVar2;
    }

    // 8003D8D8
    private int Script_30_01E(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var signature = (variables[2] << 16) | (variables[1] << 8) | variables[0];

        if (eventProgramState.Parameters[1] != signature)
        {
            eventProgramState.Parameters[1] = signature;
            eventProgramState.Parameters[2] = logicEntity.PosX;
            eventProgramState.Parameters[3] = logicEntity.PosY;
            return 0;
        }

        var dx = eventProgramState.Parameters[2] - logicEntity.PosX;
        var dy = eventProgramState.Parameters[3] - logicEntity.PosY;

        if (dx < 0)
        {
            dx = -dx;
        }

        if (dy < 0)
        {
            dy = -dy;
        }

        dx >>= 16;
        dy >>= 16;

        var threshold = (variables[2] << 8) | variables[1];

        if (threshold <= dx || threshold <= dy)
        {
            return 3;
        }

        return 0;
    }

    // 8003D974
    private int Script_31_01F(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var walkingState = Script_30_01E(logicEntity, ownerEntity, variables, eventProgramState);
        if (walkingState == 0 && logicEntity.ForceAdjusted == 0)
        {
            return 0;
        }

        return 3;
    }

    // 8003D9BC
    private int Script_32_020(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int iVar1;
        int iVar2;
        int iVar3;
        iVar3 = variables[0];

        iVar1 = 0;

        if (eventProgramState.Parameters[1] == iVar3)
        {
            iVar2 = eventProgramState.Parameters[2] - logicEntity.PosZ;

            if (iVar2 < 0)
            {
                iVar2 = -iVar2;
            }

            iVar1 = 3;

            if (iVar2 >> 0x10 < (int)(uint)CONCAT11((undefined1*)(iVar3 + 2), (undefined1*)(iVar3 + 1)))

            {
                iVar1 = 0;
            }
        }
        else
        {
            eventProgramState.Parameters[1] = iVar3;
            eventProgramState.Parameters[2] = logicEntity.PosZ;
        }

        return iVar1;*/
    }

    // 8003DA28
    private int Script_33_021(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var iVar1 = Script_32_020(logicEntity, ownerEntity, variables, eventProgramState);
        var result = 3;

        if (iVar1 == 0 && logicEntity.CollidedWithEntityZ == 0)
        {
            result = iVar1;
        }

        return result;
    }

    // 8003DA70
    private int Script_34_022(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        bool isSmaller;
        int result = 0;

        if (eventProgramState.Parameters[1] == variables[0])
        {
            if (eventProgramState.Parameters[2] == logicEntity.PosZ)
            {
                result = 1;
            }
            else
            {
                var zForce = eventProgramState.Parameters[2] - logicEntity.PosZ;

                if (zForce < 1)
                {
                    isSmaller = logicEntity.ForceZ < zForce;
                }
                else
                {
                    isSmaller = zForce < logicEntity.ForceZ;
                }

                if (isSmaller)
                {
                    logicEntity.ForceZ = zForce;
                }
            }
        }
        else
        {
            eventProgramState.Parameters[1] = variables[0];
            var entityRecord = logicEntity.EntityRecord;
            if (entityRecord == null)
            {
                //_gameEngine.PrintCommandMap();
                Debugger.Break();
            }

            eventProgramState.Parameters[2] = entityRecord.Height << 0x13;
        }

        return result;
    }

    // 8003DB28
    private int Script_35_023(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var iVar1 = Script_34_022(logicEntity, ownerEntity, variables, eventProgramState);
        var result = iVar1;
        result = 1;

        if (iVar1 == 0 && logicEntity.CollidedWithEntityZ == 0)
        {
            result = iVar1;
        }

        return result;
    }

    // 8003DB70
    private int Script_36_024(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        return logicEntity.ForceAdjusted != 0 ? 1 : 0;
    }

    // 8003DB7C
    private int Script_37_025(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var result = 1;

        if (logicEntity.CollidedWithEntityZ == 0 && logicEntity.IsAboveGround == 0)
        {
            result = 0;
        }

        return result;
    }

    // 8003DBA8
    private int Script_38_026(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var result = 1;

        if (logicEntity.ForceAdjusted == 0 && logicEntity.CollidedWithEntityZ == 0)
        {
            result = 0;
        }

        return result;
    }

    // 8003DBD4
    private int Script_39_027(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var dir = (uint)ScriptHelper.GetDirectionToTarget(_gameEngine.StaticVariables.PlayerEntity.PosX - logicEntity.PosX, _gameEngine.StaticVariables.PlayerEntity.PosY - logicEntity.PosY);
        logicEntity.TargetDirection = dir;
        return 1;
    }

    // 8003DC24
    private int Script_40_028(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.Flags |= 8;
        return 1;
    }

    // 8003DC3C
    private int Script_41_029(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.Flags &= 0xfffffff7;
        return 1;
    }

    // 8003DC54
    private int Script_42_02A(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.Flags |= 1;
        return 1;
    }

    // 8003DC6C
    private int Script_43_02B(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.Flags &= 0xfffffffe;
        return 1;
    }

    // 8003DC84
    private int Script_44_02C(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);
        eventProgramState.Result = num == 0 ? 1 : 0;
        return 2;
    }

    // 8003DCC4
    private int Script_45_02D(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var entity = _gameEngine.SpawnEntity(logicEntity, variables[1], 1);

        if (entity == null)
        {
            //_gameEngine.PrintCommandMap();
            Debugger.Break();
        }

        return 2;
    }

    // 8003DD00
    private int Script_46_02E(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int num;

        eventProgramState.Result = 0;

        num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (var i = 0; i < num; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];
            _gameEngine.DestroyEntity(entity);
            eventProgramState.Result = 1;
        }

        return 2;
    }

    // 8003DD8C
    private int Script_47_02F(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var flag = (uint)((variables[2] << 8) | variables[1]);

        uint value = variables[3] switch
        {
            0 => _gameEngine.StaticVariables.g_padState1.ButtonsHold,
            1 => _gameEngine.StaticVariables.g_padState1.ButtonsJustPressed,
            2 => _gameEngine.StaticVariables.g_padState1.ButtonsReleased,
            _ => _gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval
        };

        if ((value & flag) == 0)
        {
            eventProgramState.Result = 0;
        }
        else
        {
            eventProgramState.Result = 1;
        }

        return 4;
    }

    // 8003DDDC
    private int Script_48_030(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        uint[] flags;
        int result;
        uint flag;
        flag = (uint)(variables[1] + variables[2] * 0x100);

        if ((flag & 0x8000) == 0)
        {
            flags = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            flags = _gameEngine.StaticVariables.g_globalFlags;
        }

        result = 5;
        var mask = 1 << (variables[1] & 0x1f);
        var index = (flag >> 3) & 0xffc;

        if ((flags[index] & mask) != 0)
        {
            result = (((variables[4] << 8) | variables[3]) * 0x10000) >> 0x10;
        }

        return result;
    }

    // 8003DE6C
    private int Script_49_031(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        uint[] flags;
        int result;
        uint flag;
        flag = (uint)(variables[1] + variables[2] * 0x100);

        if ((flag & 0x8000) == 0)
        {
            flags = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            flags = _gameEngine.StaticVariables.g_globalFlags;
        }

        result = 5;
        var mask = 1 << (variables[1] & 0x1f);
        var index = (flag >> 3) & 0xffc;

        if ((flags[index] & mask) == 0)
        {
            result = (((variables[4] << 8) | variables[3]) * 0x10000) >> 0x10;
        }

        return result;
    }

    // 8003DEFC
    private int Script_50_032(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        uint[] flags;
        var flag = (variables[2] << 8) | variables[1];

        if ((flag & 0x8000) == 0)
        {
            flags = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            flags = _gameEngine.StaticVariables.g_globalFlags;
        }

        var index = (flag >> 3) & 0xffc;
        var mask = (uint)(1 << (variables[1] & 0x1f));
        flags[index] ^= mask;

        return 3;
    }

    // 8003DF74
    private int Script_51_033(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        //do this 4 times
        {
            var flagData = variables[1] + (variables[2] << 8);
            //int flag = (flagData >> 3) & 0xffc;
            var flag = (flagData >> 5) & 0x3ff;
            uint[] flags;
            //if the mapflag bit is set
            if ((flagData & 0x8000) != 0)
            {
                flags = _gameEngine.StaticVariables.g_mapFlags;
            }
            else//otherwise its a global flag
            {
                flags = _gameEngine.StaticVariables.g_globalFlags;
            }

            var bitToCheck = flagData & 0x1f;

            //check the bit for this flag
            if ((flags[flag] & (1 << bitToCheck)) == 0)
            {
                eventProgramState.Result = 0;
                return 9;
            }
        }

        {
            var flagData = variables[3] + (variables[4] << 8);
            //int flag = (flagData >> 3) & 0xffc;
            var flag = (flagData >> 5) & 0x3ff;
            uint[] flags;
            //if the mapflag bit is set
            if ((flagData & 0x8000) != 0)
            {
                flags = _gameEngine.StaticVariables.g_mapFlags;
            }
            else//otherwise its a global flag
            {
                flags = _gameEngine.StaticVariables.g_globalFlags;
            }

            var bitToCheck = flagData & 0x1f;

            //check the bit for this flag
            if ((flags[flag] & (1 << bitToCheck)) == 0)
            {
                eventProgramState.Result = 0;
                return 9;
            }
        }

        {
            var flagData = variables[5] + (variables[6] << 8);
            //int flag = (flagData >> 3) & 0xffc;
            var flag = (flagData >> 5) & 0x3ff;
            uint[] flags;
            //if the mapflag bit is set
            if ((flagData & 0x8000) != 0)
            {
                flags = _gameEngine.StaticVariables.g_mapFlags;
            }
            else//otherwise its a global flag
            {
                flags = _gameEngine.StaticVariables.g_globalFlags;
            }

            var bitToCheck = flagData & 0x1f;

            //check the bit for this flag
            if ((flags[flag] & (1 << bitToCheck)) == 0)
            {
                eventProgramState.Result = 0;
                return 9;
            }
        }

        {
            var flagData = variables[7] + (variables[8] << 8);
            //int flag = (flagData >> 3) & 0xffc;
            var flag = (flagData >> 5) & 0x3ff;
            uint[] flags;
            //if the mapflag bit is set
            if ((flagData & 0x8000) != 0)
            {
                flags = _gameEngine.StaticVariables.g_mapFlags;
            }
            else//otherwise its a global flag
            {
                flags = _gameEngine.StaticVariables.g_globalFlags;
            }

            var bitToCheck = flagData & 0x1f;

            //check the bit for this flag
            if ((flags[flag] & (1 << bitToCheck)) == 0)
            {
                eventProgramState.Result = 0;
                return 9;
            }
        }

        eventProgramState.Result = 1;//made it through them all
        return 9;
    }

    // 8003E128
    private int Script_52_034(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        uint[] piVar1;

        uint uVar2;

        int iVar3;

        uVar2 = variables[1] + variables[2] * 0x100;

        if ((uVar2 & 0x8000) == 0)
        {
            piVar1 = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            piVar1 = _gameEngine.StaticVariables.g_globalFlags;
        }

        iVar3 = variables;

        if (((uint)((uVar2 >> 3 & 0xffc) + piVar1) & 1 << ((iVar3 + 1) & 0x1f)) == 0)
        {
            uVar2 = iVar3 + 3 + (iVar3 + 4) * 0x100;

            if ((uVar2 & 0x8000) == 0)
            {
                piVar1 = _gameEngine.StaticVariables.g_mapFlags;
            }
            else
            {
                piVar1 = _gameEngine.StaticVariables.g_globalFlags;
            }

            iVar3 = variables;

            if (((uint)((uVar2 >> 3 & 0xffc) + piVar1) & 1 << ((iVar3 + 3) & 0x1f)) == 0)
            {
                uVar2 = iVar3 + 5 + (iVar3 + 6) * 0x100;

                if ((uVar2 & 0x8000) == 0)
                {
                    piVar1 = _gameEngine.StaticVariables.g_mapFlags;
                }
                else
                {
                    piVar1 = _gameEngine.StaticVariables.g_globalFlags;
                }

                iVar3 = variables;

                if (((uint)((uVar2 >> 3 & 0xffc) + piVar1) & 1 << ((iVar3 + 5) & 0x1f)) == 0)

                {
                    uVar2 = iVar3 + 7 + (iVar3 + 8) * 0x100;

                    if ((uVar2 & 0x8000) == 0)
                    {
                        piVar1 = _gameEngine.StaticVariables.g_mapFlags;
                    }
                    else
                    {
                        piVar1 = _gameEngine.StaticVariables.g_globalFlags;
                    }

                    if (((uint)((uVar2 >> 3 & 0xffc) + piVar1) &

                        1 << (variables[7] & 0x1f)) == 0)
                    {
                        eventProgramState.Result = 1;

                        return 9;
                    }
                }
            }
        }

        eventProgramState.Result = 0;

        return 9;*/
    }

    // 8003E2DC
    private int Script_53_035(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        uint[] flags;
        uint flag;

        flag = (uint)((variables[2] << 8) | variables[1]);

        if ((flag & 0x8000) == 0)
        {
            flags = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            flags = _gameEngine.StaticVariables.g_globalFlags;
        }

        var index = (flag >> 3) & 0xffc;
        var mask = 1 << (variables[1] & 0x1f);

        if ((flags[index] & mask) == 0)
        {
            return 3;
        }

        return 0;
    }

    // 8003E35C
    private int Script_54_036(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        uint[] flags;
        uint flag;

        flag = (uint)((variables[2] << 8) | variables[1]);

        if ((flag & 0x8000) == 0)
        {
            flags = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            flags = _gameEngine.StaticVariables.g_globalFlags;
        }

        var index = (flag >> 3) & 0xffc;
        var mask = 1 << (variables[1] & 0x1f);

        if ((flags[index] & mask) != 0)
        {
            return 3;
        }

        return 0;
    }

    // 8003E3DC
    private int Script_55_037(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int result;
        int command;
        command = variables[0];
        result = 0;

        if (command != eventProgramState.Parameters[1])
        {
            eventProgramState.Parameters[1] = command;
            eventProgramState.Parameters[2] = 0;
            return 0;
        }

        eventProgramState.Parameters[2]++;
        var toWait = variables[1];

        if (eventProgramState.Parameters[2] >= toWait)
        {
            return 2;
        }

        return 0;
    }

    // 8003E424
    private int Script_56_038(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_mapIdToInternalMapIndexTable[variables[1]] = (ushort)((variables[4] << 8) | variables[3]);
        return 5;
    }

    // 8003E464
    private int Script_57_039(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        return _gameEngine.Script_IsDialogInProgress() == 0 ? 0 : 1;
    }

    // 8003E484
    private int Script_58_03A(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.TargetDirection = (uint)_gameEngine.StaticVariables.g_cardinalDirectionTable[variables[1] & 0x3];
        return 2;
    }

    // 8003E4B4
    private int Script_59_03B(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        if (_gameEngine.StaticVariables.PlayerEntity.TileX >= variables[1]
            && _gameEngine.StaticVariables.PlayerEntity.TileX <= variables[2]
            && _gameEngine.StaticVariables.PlayerEntity.TileY >= variables[3]
            && _gameEngine.StaticVariables.PlayerEntity.TileY <= variables[4]
            && _gameEngine.StaticVariables.PlayerEntity.TileZ >= variables[5]
            && _gameEngine.StaticVariables.PlayerEntity.TileZ <= variables[6])
        {
            eventProgramState.Result = 1;
        }
        else
        {
            eventProgramState.Result = 0;
        }

        return 7;
    }

    // 8003E558
    private int Script_60_03C(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int iVar1;
        int piVar2;
        int iVar3;
        iVar1 = variables;
        iVar3 = 0;

        if (-1 < _gameEngine.StaticVariables.g_numberOfEntity)
        {
            piVar2 = _gameEngine.StaticVariables.PlayerEntity.TileZ;

            do
            {
                if (piVar2[-0x46] - 1U < 3 && (piVar2[-0x2f] & 0x80U) != 0 &&
                    (piVar2[-0x1d] & 0x80U) == 0 && piVar2[-0x40] == 0x0)
                {
                    if (iVar1 + 1 <= piVar2[-2] &&
                        piVar2[-2] <= iVar1 + 2 &&
                        iVar1 + 3 <= piVar2[-1] &&
                        piVar2[-1] <= iVar1 + 4 &&
                        iVar1 + 5 <= piVar2 &&
                        piVar2 <= iVar1 + 6)
                    {
                        eventProgramState.Result = 1;
                        return 7;
                    }
                }

                iVar3 = iVar3 + 1;
                piVar2 = piVar2 + 0xa5;
            } while (iVar3 <= _gameEngine.StaticVariables.g_numberOfEntity);
        }

        eventProgramState.Result = 0;

        return 7;*/
    }

    // 8003E64C
    private int Script_61_03D(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int iVar1;
        int piVar2;
        int iVar3;
        iVar1 = variables;
        iVar3 = 0;

        if (-1 < _gameEngine.StaticVariables.g_numberOfEntity)
        {
            piVar2 = _gameEngine.StaticVariables.PlayerEntity.TileZ;

            do
            {
                if (piVar2[-0x46] - 1U < 3)
                {
                    if (iVar1 + 1 <= piVar2[-2] &&
                        piVar2[-2] <= iVar1 + 2 &&
                        iVar1 + 3 <= piVar2[-1] &&
                        piVar2[-1] <= iVar1 + 4 &&
                        iVar1 + 5 <= piVar2 &&
                        piVar2 <= iVar1 + 6)
                    {
                        eventProgramState.Result = 1;
                        return 7;
                    }
                }

                iVar3 = iVar3 + 1;
                piVar2 = piVar2 + 0xa5;
            } while (iVar3 <= _gameEngine.StaticVariables.g_numberOfEntity);
        }

        eventProgramState.Result = 0;

        return 7;*/
    }

    // 8003E708
    private int Script_62_03E(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        if (_gameEngine.StaticVariables.PlayerEntity.RidingEntity == logicEntity)
        {
            eventProgramState.Result = 1;
        }
        else
        {
            eventProgramState.Result = 0;
        }

        return 1;
    }

    // 8003E734
    private int Script_63_03F(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        Entity ppEVar1;
        int iVar2;
        iVar2 = 0;

        if (-1 < _gameEngine.StaticVariables.g_numberOfEntity)
        {
            ppEVar1 = _gameEngine.StaticVariables.PlayerEntity.RidingEntity;

            do
            {
                if ((int)ppEVar1[-0x47] - 2U < 2 && ppEVar1[-0x43] == 0x0 && ppEVar1 == logicEntity)
                {
                    eventProgramState.Result = 1;
                    return 1;
                }

                iVar2 = iVar2 + 1;
                ppEVar1 = ppEVar1 + 0xa5;
            } while (iVar2 <= _gameEngine.StaticVariables.g_numberOfEntity);
        }

        eventProgramState.Result = 0;

        return 1;*/
    }

    // 8003E7B8
    private int Script_64_040(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_clearProgramState = 1;
        logicEntity.ProgramIndexes[variables[1]] = variables[2];

        return 3;
    }

    // 8003E7E4
    private int Script_65_041(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.SpriteProgramIndexes[variables[1]] = variables[2];

        return 3;
    }

    // 8003E808
    private int Script_66_042(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        ownerEntity.LogicContextEntity = _gameEngine.StaticVariables.PlayerEntity;

        return 1;
    }

    // 8003E81C
    private int Script_67_043(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int numberOfEntities = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (numberOfEntities < 1)
        {
            eventProgramState.Result = 0;
        }
        else
        {
            ownerEntity.LogicContextEntity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[numberOfEntities - 1];
            eventProgramState.Result = 1;
        }

        return 2;
    }

    // 8003E88C
    //wait dialog choice
    private int Script_68_044(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        if (eventProgramState.Parameters[1] == variables[0])
        {
            if (_gameEngine.StaticVariables.g_scriptDialogChoiceResult != 0)
            {
                if (_gameEngine.StaticVariables.g_scriptDialogChoiceResult == 1)
                {
                    eventProgramState.Result = 1;
                }
                else
                {
                    eventProgramState.Result = 0;
                }

                return 1;
            }
        }
        else
        {
            _gameEngine.StaticVariables.g_scriptDialogChoiceResult = 0;

            var arg1 = _gameEngine.EtcRes.GetEtcString(0x43);
            var arg2 = _gameEngine.EtcRes.GetEtcString(0x44);
            var res = _gameEngine.InitializeAsyncOperation(arg1, arg2, result => _gameEngine.StaticVariables.g_scriptDialogChoiceResult = result);

            if (res == 0)
            {
                return 0;
            }

            eventProgramState.Parameters[1] = variables[0];
        }

        return 0;
    }

    // 8003E954
    private int Script_69_045(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.Flags &= 0xffffdfff;
        return 1;
    }

    // 8003E96C
    private int Script_70_046(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.Flags |= 0x2000;
        return 1;
    }

    // 8003E984
    private int Script_71_047(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var result = 1;

        if (logicEntity.HitCounter == 0 && logicEntity.ForceAdjusted == 0)
        {
            result = 0;
        }

        return result;
    }

    // 8003E9B0
    private int Script_72_048(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var result = 1;

        if (logicEntity.HitCounter == 0 && logicEntity.CollidedWithEntityZ == 0)
        {
            result = 0;
        }

        return result;
    }

    // 8003E9DC
    private int Script_73_049(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        //return eventProgramState.Sp - eventProgramState.Parameters[0];
        return -eventProgramState.CodeIndex; //eventProgramState.Sp - variables[0];
    }

    // 8003E9EC
    private int Script_74_04A(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var result = 1;

        if (eventProgramState.Result != 0)
        {
            //return eventProgramState.Sp - eventProgramState.Parameters[0];
            return -eventProgramState.CodeIndex; //eventProgramState.Sp - variables[0];
        }

        return result;
    }

    // 8003EA14
    private int Script_75_04B(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var result = 1;

        if (eventProgramState.Result == 0)
        {
            //return eventProgramState.Sp - eventProgramState.Parameters[0];
            return -eventProgramState.CodeIndex; //eventProgramState.Sp - variables[0];
        }

        return result;
    }

    // 8003EA3C
    private int Script_76_04C(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.SetTextFlags((uint)variables[1]);
        return 2;
    }

    // 8003EA68
    private int Script_77_04D(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.ActivateTextAutoAdvanceFlag();
        return 1;
    }

    // 8003EA88
    private int Script_78_04E(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.SetDebugFlag((uint)variables[1]);
        return 2;
    }

    // 8003EAB4
    private int Script_ActivateDebugTextAutoAdvance(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.ActivateDebugTextAutoAdvance();
        return 1;
    }

    // 8003EAD4
    private int Script_SetEtcAnimationMode(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.SetEtcAnimationMode(variables[1]);
        return 2;
    }

    // 8003EB00
    private int Script_TryActivateTextHoldState(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.UIManager.TryActivateTextHoldState();
        return 1;
    }

    // 8003EB20
    private int Script_82_052(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        
        Portal portal;
        portal = _gameEngine.GetPortal();

        if (portal == null)
        {
            //_gameEngine.DoNothing();
            eventProgramState.Result = 0;
        }
        else
        {
            _gameEngine.PlayerManager.HandleWarpTransition(portal, 
                (int)_gameEngine.StaticVariables.PlayerEntity.TargetAnimationId, 
                (int)_gameEngine.StaticVariables.PlayerEntity.TargetDirection);
            eventProgramState.Result = 1;
        }

        return 1;
    }

    // 8003EB88
    private int Script_83_053(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_mapTransitionEffectId = variables[6];
        _gameEngine.StaticVariables.g_desiredMap = variables[1];
        _gameEngine.StaticVariables.g_warpEntryBehavior = variables[7];

        var y = variables[0];
        var x = ((y + 3) * 0x18 + 0xc) * 0x10000;
        var z = (y + 5) * 0x100000;
        y = ((y + 4) * 0x10 + 8) * 0x10000;

        if (_gameEngine.StaticVariables.g_mapTransitionEffectId == 3)
        {
            if (_gameEngine.StaticVariables.g_desiredMap == _gameEngine.StaticVariables.g_currentMap)
            {
                _gameEngine.StaticVariables.PlayerEntity.PosX = x;
                _gameEngine.StaticVariables.PlayerEntity.PosY = y;
                _gameEngine.StaticVariables.PlayerEntity.PosZ = z + 1;
                return 8;
            }

            //_gameEngine.DoNothing();

            _gameEngine.StaticVariables.g_mapTransitionEffectId = 0;
        }

        _gameEngine.StaticVariables.g_cameraTargetZ = z;
        _gameEngine.StaticVariables.g_cameraTargetY = y;
        _gameEngine.StaticVariables.g_cameraTargetX = x;
        _gameEngine.StaticVariables.g_warpExtraParam = (int)_gameEngine.StaticVariables.PlayerEntity.TargetDirection;
        _gameEngine.StaticVariables.g_warpTriggerType = (int)_gameEngine.StaticVariables.PlayerEntity.TargetAnimationId;
        _gameEngine.StaticVariables.g_isGameEnding = 1;

        return 8;
    }

    // 8003ECBC
    private int Script_84_054(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int tilex = variables[1];
        int tiley = variables[2];

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

        //_gameEngine.StaticVariables.g_spriteVRAMPointer[uVar2 * 0xd0 + uVar1 * 4 + 0x302] =
        //_gameEngine.StaticVariables.g_spriteVRAMPointer[uVar2 * 0xd0 + uVar1 * 4 + 0x302] | (ushort)variables[3] + (ushort)variables[4] * 0x100;
        var walkabilitybits = variables[3];
        var groundpropertybits = variables[4];
        var mapWidth = _gameEngine.CurrentMap.Map.Width;
        var tile = _gameEngine.CurrentMap.Map.MapTiles[tilex + tiley * mapWidth];

        tile.Walkability |= (byte)walkabilitybits;
        tile.GroundProperty |= (byte)groundpropertybits;

        return 5;
    }

    // 8003ED5C
    private int Script_85_055(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int tilex = variables[1];
        int tiley = variables[2];

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

        //_gameEngine.StaticVariables.g_spriteVRAMPointer[uVar2 * 0xd0 + uVar1 * 4 + 0x302] =
        //_gameEngine.StaticVariables.g_spriteVRAMPointer[uVar2 * 0xd0 + uVar1 * 4 + 0x302] & ~variables[3];
        var walkabilitybits = variables[3];
        var groundpropertybits = variables[4];
        var mapWidth = _gameEngine.CurrentMap.Map.Width;
        var tile = _gameEngine.CurrentMap.Map.MapTiles[tilex + tiley * mapWidth];

        tile.Walkability &= (byte)~walkabilitybits;
        tile.GroundProperty &= (byte)~groundpropertybits;

        return 5;
    }

    // 8003EDFC
    private int Script_86_056(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.ChangeAreaTileProperties(variables[1]);
        return 2;
    }

    // 8003EE28
    private int Script_87_057(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int pbVar1;
        pbVar1 = variables + _gameEngine.StaticVariables.PlayerEntity.CurrentFrameIndex * 2 + 1;
        return (int)((pbVar1 + (uint)pbVar1[1] * 0x100) * 0x10000) >> 0x10;*/
    }

    // 8003EE5C
    private int Script_88_058(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int pbVar1;

        pbVar1 = variables + logicEntity.CurrentFrameIndex * 2 + 1;

        return (int)((pbVar1 + (uint)pbVar1[1] * 0x100) * 0x10000) >> 0x10;*/
    }

    // 8003EE8C
    private int Script_89_059(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (var i = 0; i < num; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];
            entity.TargetAnimationId = (uint)variables[2];
        }

        return 3;
    }

    // 8003EEF4
    private int Script_90_05A(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        byte bVar1;
        int num;

        bVar1 = (byte)variables[2];
        num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (int i = 0; i < num; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];
            var targetDirection = _gameEngine.ResolveDirectionFromParam(entity, bVar1);
            entity.TargetDirection = targetDirection;
        }

        return 3;
    }

    // 8003EF80
    private int Script_91_05B(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        uint animationId;
        uint direction;
        int num;

        animationId = (uint)variables[2];
        direction = (uint)variables[3];
        num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (int i = 0; i < num; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];
            entity.TargetAnimationId = animationId;
            var targetDirection = _gameEngine.ResolveDirectionFromParam(entity, direction);
            //entity.TargetDirection = _gameEngine.EntityGameplayManager.TurnEntity(logicEntity, direction);
            entity.TargetDirection = targetDirection;
        }

        return 4;
    }

    // 8003F01C
    private int Script_OpenDialogWithChoice(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int matchCount;
        Entity matchedEntity;

        matchCount = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (matchCount != 0)
        {
            matchedEntity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0];
            
            if ((matchedEntity.Flags & 0x800000U) != 0)
            {
                Debugger.Break();

                using var binaryReader = _gameEngine.DatasBin.OpenBin();
                var imgset = matchedEntity.SpriteRecord.GetPortraitImageset(binaryReader);
                var img = imgset.Images[0];
                var bitmap = _gameEngine.CurrentMap.GenerateSpriteBitmap(img,
                    _gameEngine.CurrentMap.SpriteInfo.Palettes[img.Palette & 0x1f]);

                _gameEngine.MainInventoryManager.StartHudTransition(
                    matchedEntity.PosX,
                    matchedEntity.PosY,
                    matchedEntity.PosZ,
                    _gameEngine.StaticVariables.g_cameraScrollingX, 
                    _gameEngine.StaticVariables.g_cameraScrollingY,
                    img.Sx, img.Sy, img.Swidth, img.Sheight,
                    /*_gameEngine.StaticVariables.g_drawPageInfoBase[logicEntity.SpriteSheetOffset + ((img.Spritesheet) & 0x3f)],
                    _gameEngine.StaticVariables.g_tPageFadeLUT[logicEntity.PaletteOffset + ((img.Palette) & 7)]*/
                    bitmap);
            }

            _gameEngine.TriggerVisualUpdate((int)matchedEntity.SpriteTableIndex);
        }

        matchCount = _gameEngine.TryOpenDialog((uint)variables[2], variables[3]);

        return (matchCount != 0 ? 1 : 0) << 2;
    }

    // 8003F144
    private int Script_93_05D(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int num;
        num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (int i = 0; i < num; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];
            entity.Status = 3;
        }

        return 2;
    }

    // 8003F1A0
    private int Script_94_05E(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        byte bVar1;
        byte bVar2;
        int num;

        num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (int i = 0; i < num; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];
            entity.ForceZ = ((variables[2] + variables[3] * 0x100) * 0x10000) >> 8;
        }

        return 4;
    }

    // 8003F218
    private int Script_WaitForAnimOrDistance(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int command;
        int frameTimer;
        byte conditionFlags;

        if (eventProgramState.Parameters[1] != variables[0])
        {
            eventProgramState.Parameters[1] = variables[0];
            eventProgramState.Parameters[2] = logicEntity.PosX;
            eventProgramState.Parameters[3] = logicEntity.PosY;
            eventProgramState.Parameters[5] = 0;
            eventProgramState.Parameters[6] = 0;
            eventProgramState.Parameters[4] = logicEntity.PosZ;
            logicEntity.AnimCompleteCounter = 0;
            return 0;
        }

        if (logicEntity.ForceResetAnimationFlag == 0)
        {
            if (logicEntity.AnimCompleteCounter == 0)
                goto UPDATE_FRAME_COUNTER;
        }
        else
        {
            logicEntity.CurrentAnimationId = ~logicEntity.TargetAnimationId;
        }

        eventProgramState.Parameters[5] = eventProgramState.Parameters[5] + 1;
        logicEntity.AnimCompleteCounter = 0;

        UPDATE_FRAME_COUNTER:
        frameTimer = eventProgramState.Parameters[6] + 1;
        eventProgramState.Parameters[6] = frameTimer;
        command = variables[0];

        if (eventProgramState.Parameters[5] < (int)(uint)(command + 4))
        {
            return 0;
        }

        if (frameTimer < (int)(uint)(command + 5))
        {
            return 0;
        }

        conditionFlags = command + 6;

        if (((conditionFlags & 1) == 0 || logicEntity.ForceAdjusted != 0) &&
            ((conditionFlags & 2) == 0 || logicEntity.CollidedWithEntityZ != 0) &&
             ((conditionFlags & 3) == 0 || logicEntity.IsAboveGround != 0))
        {
            if ((conditionFlags & 4) != 0 && logicEntity.HitCounter == 0)
            {
                return 0;
            }

            frameTimer = variables[0];
            command = eventProgramState.Parameters[2] - logicEntity.PosX;
            if (command < 0)
            {
                command = -command;
            }

            if (command < (int)((uint)(frameTimer + 1) * 0x180000))
            {
                return 0;
            }

            command = eventProgramState.Parameters[3] - logicEntity.PosY;
            if (command < 0)
            {
                command = -command;
            }

            if ((int)((uint)(frameTimer + 2) << 0x14) <= command)
            {
                command = eventProgramState.Parameters[4] - logicEntity.PosZ;
                if (command < 0)
                {
                    command = -command;
                }

                return (command < (int)((uint)(frameTimer + 3) << 0x14) ^ 1) << 3;
            }
        }

        return 0;*/
    }

    // 8003F3F8
    private int Script_96_060(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int num;
        num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (int i = 0; i < num; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];
            Debugger.Break();
            if (entity.ChildEntity == logicEntity) // TODO: check if this is correct
            {
                eventProgramState.Result = 1;
                return 2;
            }
        }

        eventProgramState.Result = 0;
        return 2;
    }

    // 8003F488
    private int Script_97_061(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int num;
        num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (int i = 0; i < num; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];
            Debugger.Break();
            if (entity.RidingEntity == logicEntity) // TODO: check if this is correct
            {
                eventProgramState.Result = 1;
                return 2;
            }
        }

        eventProgramState.Result = 0;
        return 2;
    }

    // 8003F514
    private int Script_98_062(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        ushort flag;
        int num;

        flag = (ushort)((variables[3] << 8) | variables[2]);
        num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (int i = 0; i < num; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];
            entity.Flags |= flag;
        }

        return 4;
    }

    // 8003F590
    private int Script_99_063(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        ushort clearMask;
        int num;

        num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (int i = 0; i < num; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];
            //entity.Flags &= ~(uint)clearMask | 0xffff0000;

            clearMask = (ushort)((variables[3] << 8) | variables[2]);
            uint andMask = 0xFFFF0000u | (uint)(~clearMask & 0xFFFF);
            entity.Flags &= andMask;
        }

        return 4;
    }

    // 8003F610
    private int Script_100_064(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int x = ((variables[3] << 8) | variables[2]) << 16;
        int y = ((variables[5] << 8) | variables[4]) << 16;
        int z = (((variables[7] << 8) | variables[6]) << 16) + 1;

        var count = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (int i = 0; i < count; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];
            entity.PosX = x;
            entity.PosY = y;
            entity.PosZ = z;
        }

        return 8;
    }

    // 8003F6C8
    private int Script_101_065(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int x = (variables[2] | (variables[3] << 8)) << 16;
        int y = (variables[4] | (variables[5] << 8)) << 16;
        int z = (variables[6] | (variables[7] << 8)) << 16;

        var num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (int i = 0; i < num; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];
            entity.PosX += x;
            entity.PosY += y;
            entity.PosZ += z;
        }

        return 8;
    }

    // 8003F794
    private int Script_CopyLogicContextAndAssignScript(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        EventProgramState pEVar1;
        EventProgramState pEVar2;
        int script2;
        int command;
        int xpos;
        EventProgramState nextSource;
        EventProgramState nextTarget;
        pEVar1 = _gameEngine.StaticVariables.g_eventProgramState;
        pEVar2 = ownerEntity.EventProgramState;

        do
        {
            nextTarget = pEVar2;
            nextSource = pEVar1;
            script2 = nextSource.var0;
            command = nextSource.var1;
            xpos = nextSource.var2;
            nextTarget.Sp = nextSource.Sp;
            nextTarget.var0 = script2;
            nextTarget.var1 = command;
            nextTarget.var2 = xpos;
            pEVar1 = EventProgramState & nextSource.var3;
            pEVar2 = EventProgramState & nextTarget.var3;
        } while (&nextSource.var3 != _gameEngine.StaticVariables.g_eventProgramState._30);

        command = nextSource.var4;
        nextTarget.var3 = _gameEngine.StaticVariables.g_eventProgramState._30;
        nextTarget.var4 = command;
        logicEntity.LastTargetAnimationId = logicEntity.TargetAnimationId;
        logicEntity.LastTargetDirection = logicEntity.TargetDirection;

        command = variables;
        script2 = command + ((command + 1 + (command + 2) * 0x100) * 0x10000 >> 0x10);
        ownerEntity.EventProgramState.var0 = script2;
        ownerEntity.EventProgramState.Sp = script2;

        return 3;*/
    }

    // 8003F82C
    private int Script_103_067(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);
        _gameEngine.StaticVariables.g_entityFollowedByCamera = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0];
        return 2;
    }

    // 8003F868
    private int Script_104_068(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_entityFollowedByCamera = null;
        return 1;
    }

    // 8003F878
    private int Script_105_069(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_entityFollowedByCamera = null;
        _gameEngine.StaticVariables.g_cameraLookAtX = variables[1];
        _gameEngine.StaticVariables.g_cameraLookAtY = variables[3];
        _gameEngine.StaticVariables.g_cameraLookAtZ = variables[5];
        return 7;
    }

    // 8003F8DC
    private int Script_106_06A(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        eventProgramState.Parameters[2] = logicEntity.PosX;
        eventProgramState.Parameters[3] = logicEntity.PosY;
        eventProgramState.Parameters[4] = logicEntity.PosZ;
        return 1;
    }

    // 8003F908
    private int Script_107_06B(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var iVar1 = eventProgramState.Parameters[2] - logicEntity.PosX;

        if (iVar1 < 0)
        {
            iVar1 = -iVar1;
        }

        eventProgramState.Result = iVar1 < (variables[1] << 0x10 ^ 1) ? 1 : 0;

        return 3;
    }

    // 8003F94C
    private int Script_108_06C(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var iVar1 = eventProgramState.Parameters[3] - logicEntity.PosY;

        if (iVar1 < 0)
        {
            iVar1 = -iVar1;
        }

        eventProgramState.Result = iVar1 < (variables[1] << 0x10 ^ 1) ? 1 : 0;
        return 3;
    }

    // 8003F990
    private int Script_109_06D(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var iVar1 = eventProgramState.Parameters[4] - logicEntity.PosZ;

        if (iVar1 < 0)
        {
            iVar1 = -iVar1;
        }

        eventProgramState.Result = iVar1 < (variables[1] << 0x10 ^ 1) ? 1 : 0;
        return 3;
    }

    // 8003F9D4
    private int Script_110_06E(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        eventProgramState.Result = logicEntity.ForceAdjusted;
        return 1;
    }

    // 8003F9E8
    private int Script_111_06F(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        eventProgramState.Result = logicEntity.CollidedWithEntityZ;
        return 1;
    }

    // 8003F9FC
    private int Script_112_070(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        eventProgramState.Result = logicEntity.IsAboveGround;
        return 1;
    }

    // 8003FA10
    private int Script_113_071(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        eventProgramState.Result = logicEntity.HitCounter;
        return 1;
    }

    // 8003FA24
    private int Script_114_072(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        logicEntity.LastTargetAnimationId = logicEntity.TargetAnimationId;
        logicEntity.LastTargetDirection = logicEntity.TargetDirection;
        return 1;
    }

    // 8003FA3C
    private int Script_115_073(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        eventProgramState._30 = variables[1];
        return 2;
    }

    // 8003FA58
    private int Script_116_074(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var result = eventProgramState._30 + -1;
        eventProgramState._30 = result;

        if (result < 1)
        {
            result = 3;
        }
        else
        {
            result = (((variables[2] << 8) | variables[1]) * 0x10000) >> 0x10;
        }

        return result;
    }

    // 8003FA9C
    private int Script_117_075(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.SoundManager.PlaySoundEffect((uint)variables[1]);
        return 2;
    }

    // 8003FAC8
    private int Script_118_076(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        //_gameEngine.PrintCommandMap();
        return 0;
    }

    // 8003FAEC
    private int Script_119_077(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        //_gameEngine.PrintCommandMap();
        return 0;
    }

    // 8003FB10
    private int Script_StoreChoiceParamAndJump(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        eventProgramState._34 = eventProgramState.CodeIndex + 3; //variables[3];
        return (((variables[2] << 8) | variables[1]) * 0x10000) >> 0x10;
    }

    // 8003FB44
    private int Script_JumpIfChoiceAccepted(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int result = 3;

        if (eventProgramState.Result != 0)
        {
            eventProgramState._34 = eventProgramState.CodeIndex + 3; //variables[3];
            result = (((variables[2] << 8) | variables[1]) * 0x10000) >> 0x10;
        }

        return result;
    }

    // 8003FB8C
    private int Script_JumpIfChoiceRejected(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int result = 3;

        if (eventProgramState.Result == 0)
        {
            eventProgramState._34 = eventProgramState.CodeIndex + 3; //variables[3];
            result = (((variables[2] << 8) | variables[1]) * 0x10000) >> 0x10;
        }

        return result;
    }

    // 8003FBD4
    private int Script_JumpIfFlagSetStoreParam(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        uint[] flags;
        int result;

        var flag = (uint)((variables[2] << 8) | variables[1]);

        if ((flag & 0x8000) == 0)
        {
            flags = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            flags = _gameEngine.StaticVariables.g_globalFlags;
        }

        result = 5;
        var index = (flag >> 3) & 0xffc;
        var mask = (uint)(1 << (variables[1] & 0x1f));

        if ((flags[index] & mask) != 0)
        {
            eventProgramState._34 = eventProgramState.CodeIndex + 5; //variables[5];
            result = (((variables[4] << 8) | variables[3]) * 0x10000) >> 0x10;
        }

        return result;
    }

    // 8003FC74
    private int Script_JumpIfFlagClearStoreParam(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        uint[] flags;
        int result;

        var flag = (uint)((variables[2] << 8) | variables[1]);

        if ((flag & 0x8000) == 0)
        {
            flags = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            flags = _gameEngine.StaticVariables.g_globalFlags;
        }

        result = 5;
        var index = (flag >> 3) & 0xffc;
        var mask = (uint)(1 << (variables[1] & 0x1f));

        if ((flags[index] & mask) == 0)
        {
            eventProgramState._34 = eventProgramState.CodeIndex + 5; //variables[5];
            result = (((variables[4] << 8) | variables[3]) * 0x10000) >> 0x10;
        }

        return result;
    }

    // 8003FD14
    private int Script_JumpRelativeFromStoredParam(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var value = (variables[1] << 24) | (variables[1] << 16) | (variables[1] << 8) | variables[0];
        return eventProgramState._34 - eventProgramState.CodeIndex; //variables[0];
    }

    // 8003FD24
    private int Script_ConditionalJumpFromStoredParamIfTrue(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int result = 1;

        if (eventProgramState.Result != 0)
        {
            result = eventProgramState._34 - eventProgramState.CodeIndex; //eventProgramState._34 - variables[0];
        }

        return result;
    }

    // 8003FD4C
    private int Script_ConditionalJumpFromStoredParamIfFalse(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int result = 1;

        if (eventProgramState.Result == 0)
        {
            result = eventProgramState._34 - eventProgramState.CodeIndex; //eventProgramState._34 - variables[0];
        }

        return result;
    }

    // 8003FD74
    private int Script_JumpFromStoredParamIfFlagSet(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        uint[] flags;
        int result;

        var flag = (uint)((variables[2] << 8) | variables[1]);

        if ((flag & 0x8000) == 0)
        {
            flags = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            flags = _gameEngine.StaticVariables.g_globalFlags;
        }

        result = 5;
        var index = (flag >> 3) & 0xffc;
        var mask = (uint)(1 << (variables[1] & 0x1f));

        if ((flags[index] & mask) != 0)
        {
            result = eventProgramState._34 - eventProgramState.CodeIndex; //eventProgramState._34 - variables[0];
        }

        return result;
    }

    // 8003FDF8
    private int Script_JumpFromStoredParamIfFlagClear(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        uint[] flags;
        int result;

        var flag = (uint)((variables[2] << 8) | variables[1]);

        if ((flag & 0x8000) == 0)
        {
            flags = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            flags = _gameEngine.StaticVariables.g_globalFlags;
        }

        result = 5;
        var index = (flag >> 3) & 0xffc;
        var mask = (uint)(1 << (variables[1] & 0x1f));

        if ((flags[index] & mask) == 0)
        {
            result = eventProgramState._34 - eventProgramState.CodeIndex; //eventProgramState._34 - variables[0];
        }

        return result;
    }

    // 8003FE7C
    private int Script_130_082(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var succes = _gameEngine.PlayerManager.HandleMapTriggerCommand(variables[1]);

        if (succes == 0)
        {
            eventProgramState.Result = 0;
        }
        else
        {
            eventProgramState.Result = 1;
        }

        return 2;
    }

    // 8003FEC8
    private int Script_131_083(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var iVar1 = _gameEngine.PlayerManager.GetNumberOfItem(variables[1]);

        if (iVar1 < variables[2])
        {
            eventProgramState.Result = 0;
        }
        else
        {
            eventProgramState.Result = 1;
        }

        return 3;
    }

    // 8003FF34
    private int Script_132_084(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var iVar1 = _gameEngine.PlayerManager.UseItem((uint)variables[1]);

        if (iVar1 == -1)
        {
            eventProgramState.Result = 0;
        }
        else
        {
            eventProgramState.Result = 1;
        }

        return 2;
    }

    // 8003FF84
    private int Script_133_085(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.ChangeAreaTileProperties(variables[1], variables[2], variables[3], variables[4], variables[5], variables[6]);
        return 7;
    }

    // 8003FFD4
    private int Script_134_086(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var value = variables[2];
        var num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (var i = 0; i < num; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];
            entity.DamagedTickCounter += value;
        }

        return 4;
    }

    // 80040048
    private int Script_135_087(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        byte bVar1;
        byte bVar2;
        int iVar3;
        int piVar4;
        int iVar5;

        bVar1 = _gameEngine.StaticVariables.BYTE_ARRAY_80098fa4[variables[0][2]];
        iVar3 = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (0 < iVar3)
        {
            piVar4 = _gameEngine.StaticVariables.g_activeEntityRefId + iVar3;

            do
            {
                iVar5 = piVar4 + 0x224;
                bVar2 = (iVar5 + 0x1c8);

                if (iVar5 != 0 && iVar5 + 0x1d4 != 0
                               && (iVar5 + 0x1c8) != 0x0
                                && bVar2 != 0 && (bVar2 & 0xf) == bVar1)
                {
                    eventProgramState.Result = 1;

                    return 3;
                }

                iVar3 = iVar3 + -1;
                piVar4 = piVar4 + -1;
            } while (0 < iVar3);
        }

        eventProgramState.Result = 0;

        return 3;*/
    }

    // 8004011C
    private int Script_136_088(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int matchingEntityCount;
        int iVar1;
        int piVar2;
        int piVar3;
        piVar3 = (&PTR_DAT_80023d2c)[variables[0][2]];
        matchingEntityCount = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (0 < matchingEntityCount)
        {
            piVar2 = _gameEngine.StaticVariables.g_activeEntityRefId + matchingEntityCount;

            do
            {
                iVar1 = piVar2;
                piVar2 = piVar2 + -1;
                matchingEntityCount = matchingEntityCount + -1;
                (iVar1 + 0x10) = piVar3;
            } while (0 < matchingEntityCount);
        }

        return 3;*/
    }

    // 80040194
    private int Script_137_089(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        byte bVar1;
        byte bVar2;
        byte bVar3;
        byte bVar4;
        byte bVar5;
        byte bVar6;
        int iVar7;
        int iVar8;
        int iVar9;
        int iVar10;
        int piVar11;
        int iVar12;

        iVar7 = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (iVar7 != 0)
        {
            iVar12 = variables;
            bVar1 = iVar12 + 4;
            bVar2 = iVar12 + 3;
            bVar3 = iVar12 + 6;
            bVar4 = iVar12 + 5;
            bVar5 = iVar12 + 8;
            bVar6 = iVar12 + 7;
            iVar10 = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].PosX;
            iVar7 = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].PosY;
            iVar8 = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].PosZ;

            iVar12 = _gameEngine.GetNumberOfEntityByRefId(logicEntity, iVar12 + 2);

            if (0 < iVar12)
            {
                piVar11 = _gameEngine.StaticVariables.g_activeEntityRefId + iVar12;

                do
                {
                    iVar9 = piVar11;
                    piVar11 = piVar11 + -1;
                    iVar12 = iVar12 + -1;
                    (uint)(iVar9 + 0x114) = iVar10 + (bVar2 + (uint)bVar1 * 0x100) * 0x10000;
                    (uint)(iVar9 + 0x118) = iVar7 + (bVar4 + (uint)bVar3 * 0x100) * 0x10000;
                    (uint)(iVar9 + 0x11c) = iVar8 + (bVar6 + (uint)bVar5 * 0x100) * 0x10000;
                } while (0 < iVar12);
            }
        }

        return 9;*/
    }

    // 80040284
    private int Script_138_08A(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var entity = _gameEngine.SpawnEntity(logicEntity, variables[1], 1);

        if (entity == null)
        {
            //_gameEngine.PrintCommandMap();
        }

        entity.PosX = (variables[2] + variables[3] * 0x100) * 0x10000;
        entity.PosY = (variables[4] + variables[5] * 0x100) * 0x10000;
        entity.PosZ = (variables[6] + variables[7] * 0x100) * 0x10000 + 1;

        return 8;
    }

    // 8004033C
    private int Script_139_08B(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var entity = _gameEngine.SpawnEntity(logicEntity, variables[2], 1);

        if (entity == null)
        {
            //_gameEngine.PrintCommandMap();
        }

        var num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (num != 0)
        {
            var entity2 = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0];

            entity.PosX = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].PosX + (variables[3] + variables[4] * 0x100) * 0x10000;
            entity.PosY = entity2.PosY + (variables[5] + variables[6] * 0x100) * 0x10000;
            entity.PosZ = entity2.PosZ + (variables[7] + variables[8] * 0x100) * 0x10000;
        }

        return 9;
    }

    // 80040438
    private int Script_140_08C(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;

        if ((uint)((ulong)_gameEngine.StaticVariables.g_gameRandomSeed * 0x100 >> 0x20) < variables[1])
        {
            eventProgramState.Result = 0;
        }
        else
        {
            eventProgramState.Result = 1;
        }

        return 2;
    }

    // 800404A8
    private int Script_141_08D(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (var i = 0; i < num; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];

            if (entity.PosZ <= entity.TerrainHeight + 1)
            {
                eventProgramState.Result = 1;
                return 2;
            }
        }

        eventProgramState.Result = 0;

        return 2;
    }

    // 80040534
    private int Script_142_08E(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_scrollingParameters.Flag = 1;
        _gameEngine.StaticVariables.g_scrollingParameters.SpeedX = variables[1];
        _gameEngine.StaticVariables.g_scrollingParameters.SpeedY = variables[2];
        _gameEngine.StaticVariables.g_scrollingParameters.LimitX = variables[3];
        _gameEngine.StaticVariables.g_scrollingParameters.LimitY = variables[4];

        return 5;
    }

    // 80040598
    private int Script_143_08F(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_scrollingParameters.Flag = 0;
        return 1;
    }

    // 800405A8
    private int Script_144_090(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.EffectManager.SpawnSpriteEffect(variables[1], 1);
        return 2;
    }

    // 800405D4
    private int Script_145_091(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var effectId = variables[1];

        foreach (var effect in _gameEngine.StaticVariables.g_effectSlots)
        {
            if (effect.Status != 0 && effect.MapEffectId == effectId)
            {
                effect.Status = 0;
            }
        }

        return 2;
    }

    // 80040628
    private int Script_146_092(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var effectId = variables[1];
        var animId = (byte)variables[2];

        foreach (var effect in _gameEngine.StaticVariables.g_effectSlots)
        {
            if (effect.Status != 0 && effect.MapEffectId == effectId)
            {
                effect.TargetAnimation = animId;
            }
        }

        return 3;
    }

    // 80040680
    private int Script_147_093(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var effectId = variables[1];
        var x = ((variables[3] << 8) | variables[2]) << 16;
        var y = ((variables[5] << 8) | variables[4]) << 16;
        var z = ((variables[7] << 8) | variables[6]) << 16;

        foreach (var effect in _gameEngine.StaticVariables.g_effectSlots)
        {
            if (effect.Status != 0 && effect.MapEffectId == effectId)
            {
                effect.X = x;
                effect.Y = y;
                effect.Z = z;
            }
        }

        return 8;
    }

    // 8004071C
    private int Script_148_094(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var effectid = variables[1];
        var x = ((variables[3] << 8) | variables[2]) << 16;
        var y = ((variables[5] << 8) | variables[4]) << 16;
        var z = ((variables[7] << 8) | variables[6]) << 16;

        foreach (var effect in _gameEngine.StaticVariables.g_effectSlots)
        {
            if (effect.Status != 0 && effect.MapEffectId == effectid)
            {
                effect.ForceX = x;
                effect.ForceY = y;
                effect.ForceZ = z;
            }
        }

        return 8;
    }

    // 800407C0
    private int Script_149_095(Entity logicEntity, Entity ownerEntity, int[] variables,
        EventProgramState eventProgramState)
    {
        Debugger.Break();

        // variables est le bloc lu via lw a2,0(a2) puis lbu … dans l’ASM.
        // Conversions "fixed" identiques au MIPS :
        // centreX = (var1 * 3) << 19 ; centreY = var2 << 20 ; centreZ = var3 << 20
        // portées/rayons : rx = (var4 * 3) << 19 ; ry = ((var5 << 20) - 1) + 1 == var5 << 20
        if (variables == null || variables.Length < 10)
        {
            eventProgramState.Result = 0;
            return 10;
        }

        int var1 = variables[1]; // largeur “X” (tiles) → *3 << 19
        int var2 = variables[2]; // centre Y (<< 20)
        int var3 = variables[3]; // centre Z (<< 20)
        int var4 = variables[4]; // portée X (tiles) → *3 << 19
        int var5 = variables[5]; // portée Y (<< 20), ASM fait -1 puis +1 → net = <<20
        int maskLo = variables[7];
        int maskHi = variables[8];
        int flagMask = maskLo | (maskHi << 8); // t4
        byte idxType = (byte)variables[9];

        // Si aucun masque fourni → early out (beq t4,zero)
        if (flagMask == 0 || _gameEngine.StaticVariables.g_numberOfEntity < 0)
        {
            eventProgramState.Result = 0;
            return 10;
        }

        // Centres/portées dans le même “fixed” que le code MIPS
        int centerX = (var1 * 3) << 19; // t3
        int centerY = var2 << 20; // t2
        int centerZ = var3 << 20; // t1

        int rangeX = (var4 * 3) << 19; // ownerEntity (registre) recopié dans "variables" dans la boucle
        int rangeY = (var5 << 20); // (t7+1) dans l’ASM devient <<20

        // Type attendu (v1 & 0x0F) == t6
        int expectedNibble = _gameEngine.StaticVariables.BYTE_ARRAY_80098fa4[idxType];

        int n = _gameEngine.StaticVariables.g_numberOfEntity;
        var slots = _gameEngine.StaticVariables.g_entitySlots;

        for (int i = 0; i < n; i++)
        {
            Entity e = slots[i];

            if (e == null)
            {
                continue;
            }

            // (state-2) < 2  → Status 2 ou 3 uniquement
            if (e.Status < 2 || e.Status > 3)
            {
                continue;
            }

            // -0x1FC == 0 → IsNotProcessable == 0
            if (e.IsNotProcessable != 0)
            {
                continue;
            }

            // -0x48 != 0 → FrameCollision != null
            if (e.FrameCollision == null)
            {
                continue;
            }

            // (Flags & flagMask) != 0  — le MIPS AND à 0x6C ; en C# on utilise un champ flag.
            // Si tes bits de filtre sont sur Flags2, bascule sur e.Flags2.
            if (((int)e.Flags & flagMask) == 0)
            {
                continue;
            }

            // *(u8*)ptr1C8 != 0  et  ( *(u8*)ptr1C8 & 0x0F ) == table[index]
            int nibble = e.BalanceAnimValRef == null ? -1 : e.BalanceAnimValRef.Val & 0x0F;

            if (nibble < 0)
            {
                continue;
            }

            if (nibble != expectedNibble)
            {
                continue;
            }

            // Tests de volume en 3 passes, fidèles à l’ASM (gestion des “diff < 0” avec largeur+1 / depth+1 / height+1)

            // X : HitBoxX vs centerX, portée = rangeX ; si diff négative → (centerX - HitBoxX) < (CollisionWidth + 1)
            bool okX;
            {
                int dx = e.HitBoxX - centerX;

                if (dx >= 0)
                {
                    okX = dx < rangeX;
                }
                else
                {
                    int w = e.CollisionWidth + 1;
                    okX = (centerX - e.HitBoxX) < w;
                }
            }

            if (!okX)
            {
                continue;
            }

            // Y : HitBoxY vs centerY, portée = rangeY ; si diff négative → (centerY - HitBoxY) < (CollisionDepth + 1)
            bool okY;
            {
                int dy = e.HitBoxY - centerY;

                if (dy >= 0)
                {
                    okY = dy < rangeY;
                }
                else
                {
                    int d = e.CollisionDepth + 1;
                    okY = (centerY - e.HitBoxY) < d;
                }
            }

            if (!okY)
            {
                continue;
            }

            // Z : HitBoxZ vs centerZ, portée = rangeX (oui, l’ASM réutilise le même “variables” que pour X)
            // si diff négative → (centerZ - HitBoxZ) < (CollisionHeight + 1)
            bool okZ;
            {
                int dz = e.HitBoxZ - centerZ;

                if (dz >= 0)
                {
                    okZ = dz < rangeX;
                }
                else
                {
                    int h = e.CollisionHeight + 1;
                    okZ = (centerZ - e.HitBoxZ) < h;
                }
            }

            if (!okZ)
            {
                continue;
            }

            eventProgramState.Result = 1;

            return 10;
        }

        return 10;
    }

    // 800409A8
    private int Script_150_096(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var value = variables[1];
        var num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (var i = 0; i < num; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];
            entity.Hp += value;

            if (entity.HpMax < entity.Hp)
            {
                entity.Hp = entity.HpMax;
            }
        }

        return 3;
    }

    // 80040A2C
    private int Script_151_097(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.PlayerManager.SpendMoney(variables[1]);
        return 2;
    }

    // 80040A58
    private int Script_152_098(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.PlayerManager.AddMoney(variables[1]);
        return 3;
    }

    // 80040A8C
    private int Script_153_099(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var money = _gameEngine.PlayerManager.GetMoney();

        if (money < variables[1])
        {
            eventProgramState.Result = 0;
        }
        else
        {
            _gameEngine.PlayerManager.SpendMoney(variables[1]);
            eventProgramState.Result = 1;
        }

        return 3;
    }

    // 80040B00
    private int Script_154_09A(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var iVar1 = _gameEngine.PlayerManager.GetMoney();

        if (iVar1 < variables[1])
        {
            eventProgramState.Result = 0;
        }
        else
        {
            eventProgramState.Result = 1;
        }

        return 3;
    }

    // 80040B68
    private int Script_155_09B(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_isWarpDisabled = 1;
        return 1;
    }

    // 80040B78
    private int Script_156_09C(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_isWarpDisabled = 0;
        return 1;
    }

    // 80040B88
    private int Script_157_09D(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        //_gameEngine.DoNothing();
        Debugger.Break();
        Environment.Exit(-1);
        return 0;
    }

    // 80040BB4
    private int Script_158_09E(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int iVar1;

        ushort* puVar2;

        int iVar3;

        int iVar4;

        int iVar5;

        iVar4 = 0;

        iVar1 = variables;

        iVar5 = 0;

        if (iVar1 + 4 != 0)
        {
            do
            {
                iVar3 = 0;

                puVar2 = _gameEngine.StaticVariables.g_spriteVRAMPointer + (iVar1 + 2 + iVar4) * 0xd0 + (iVar1 + 1) * 4 + 0x302;

                if (iVar1 + 3 != 0)
                {
                    do
                    {
                        if ((puVar2 & 2) != 0)
                        {
                            iVar5 = iVar5 + 1;
                        }

                        iVar3 = iVar3 + 1;

                        puVar2 = puVar2 + 4;
                    } while (iVar3 < iVar1 + 3);
                }

                iVar4 = iVar4 + 1;
            } while (iVar4 < iVar1 + 4);

            iVar1 = variables;
        }

        if (iVar5 < iVar1 + 5)
        {
            eventProgramState.Result = 0;
        }
        else
        {
            eventProgramState.Result = 1;
        }

        return 6;*/
    }

    // 80040C80
    private int Script_159_09F(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        ushort uVar1;

        int iVar2;

        int piVar3;

        iVar2 = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (iVar2 != 0 && _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].ContentsGameFlag != 0)
        {
            uVar1 = (ushort)_gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].ContentsGameFlag;

            if ((uVar1 & 0x8000) == 0)
            {
                piVar3 = _gameEngine.StaticVariables.g_mapFlags;
            }
            else
            {
                piVar3 = _gameEngine.StaticVariables.g_globalFlags;
            }

            if (((uint)((uVar1 >> 3 & 0xffc) + piVar3) & 1 << (_gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].ContentsGameFlag & 0x1fU)) != 0)
            {
                eventProgramState.Result = 1;

                return 2;
            }
        }

        eventProgramState.Result = 0;

        return 2;*/
    }

    // 80040D60
    private int Script_160_0A0(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var effectid = variables[1];
        var x = ((variables[3] << 8) | variables[2]) << 16;
        var y = ((variables[5] << 8) | variables[4]) << 16;
        var z = ((variables[7] << 8) | variables[6]) << 16;

        foreach (var effect in _gameEngine.StaticVariables.g_effectSlots)
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

    // 80040E10
    private int Script_161_0A1(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var effectid = variables[1];
        var entityId = variables[2];

        var numEntities = _gameEngine.GetNumberOfEntityByRefId(logicEntity, entityId);
        if (numEntities == 0)
        {
            return 9;
        }
                                         
        var x = ((variables[4] << 8) | variables[3]) << 16;
        var y = ((variables[6] << 8) | variables[5]) << 16;
        var z = ((variables[8] << 8) | variables[7]) << 16;

        var playerEntity = _gameEngine.StaticVariables.PlayerEntity;

        x += playerEntity.PosX;
        y += playerEntity.PosY;
        z += playerEntity.PosZ;

        foreach (var effect in _gameEngine.StaticVariables.g_effectSlots)
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

    // 80040F00
    private int Script_162_0A2(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var spriteEffect = _gameEngine.EffectManager.SpawnSpriteEffect(variables[1], 1);

        if (spriteEffect != null)
        {
            spriteEffect.X = ((variables[3] << 8) | variables[2]) << 16;
            spriteEffect.Y = ((variables[5] << 8) | variables[4]) << 16;
            spriteEffect.Z = (((variables[7] << 8) | variables[6]) << 16) + 1;
        }

        return 8;
    }

    // 80040FAC
    private int Script_163_0A3(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[2]);
        var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0];

        if (num != 0)
        {
            var spriteEffect = _gameEngine.EffectManager.SpawnSpriteEffect(variables[1], 1);

            if (spriteEffect != null)
            {
                spriteEffect.X = entity.PosX + (((variables[4] << 8) | variables[3]) << 16);
                spriteEffect.Y = entity.PosY + (((variables[6] << 8) | variables[5]) << 16);
                spriteEffect.Z = entity.PosZ + (((variables[8] << 8) | variables[7]) << 16);
            }
        }

        return 9;
    }

    // 80041098
    private int Script_164_0A4(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.SetScrollingMode(variables[1], variables[2]);
        return 3;
    }

    // 800410C8
    private int Script_165_0A5(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.SoundManager.StopAllSound();
        return 1;
    }

    // 800410E8
    private int Script_166_0A6(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.SoundManager.LoadBgm(variables[1]);
        return 2;
    }

    // 80041114
    private int Script_167_0A7(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.SoundManager.FUN_8004b114(variables[1], variables[2]);
        return 3;
    }

    // 80041144
    private int Script_168_0A8(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 1;
        /*
        bool bVar1;

        undefined3 extraout_var;

        bVar1 = _gameEngine.IsSoundDriverReady();

        eventProgramState.Result = _gameEngine.CONCAT31(extraout_var, bVar1);

        return 1;*/
    }

    // 80041174
    private int Script_169_0A9(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int iVar1;
        int piVar2;
        iVar1 = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (0 < iVar1)
        {
            piVar2 = _gameEngine.StaticVariables.g_activeEntityRefId + iVar1;

            do
            {
                if (logicEntity.XCollisionEntity == piVar2)
                {
                    eventProgramState.Result = 1;
                    return 2;
                }

                iVar1 = iVar1 + -1;
                piVar2 = piVar2 + -1;
            } while (0 < iVar1);
        }

        eventProgramState.Result = 0;

        return 2;*/
    }

    // 80041200
    private int Script_170_0AA(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int iVar1;

        int piVar2;

        iVar1 = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (0 < iVar1)
        {
            piVar2 = _gameEngine.StaticVariables.g_activeEntityRefId + iVar1;

            do
            {
                if (piVar2 + 0x130 == logicEntity)
                {
                    eventProgramState.Result = 1;

                    return 2;
                }

                iVar1 = iVar1 + -1;

                piVar2 = piVar2 + -1;
            } while (0 < iVar1);
        }

        eventProgramState.Result = 0;

        return 2;*/
    }

    // 80041290
    private int Script_171_0AB(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int iVar1;

        iVar1 = variables;

        _gameEngine.SoundManager.FUN_80049794(iVar1 + 1, iVar1 + 2, iVar1 + 3);

        return 4;*/
    }

    // 800412C4
    private int Script_172_0AC(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var numberOfMatchingEntities = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (numberOfMatchingEntities != 0)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0];
            uint flags = entity.Flags & 0xfff8ffff;
            var newBits = (uint)(variables[2] & 7);
            newBits <<= 16;
            entity.Flags = flags | newBits;
        }

        return 4;
    }

    // 80041344
    private int Script_173_0AD(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        byte bVar1;
        byte bVar2;
        byte bVar3;
        int iVar4;
        int piVar5;
        int iVar6;
        int iVar7;
        int iVar8;
        int iVar9;

        iVar4 = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (iVar4 == 0)
        {
            eventProgramState.Result = 0;
        }
        else
        {
            iVar4 = variables;
            iVar9 = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].PosX + (char*)(iVar4 + 3) * 0x180000;
            iVar8 = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].PosY + (char*)(iVar4 + 4) * 0x100000;
            iVar7 = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].PosZ + (char*)(iVar4 + 5) * 0x100000;
            bVar1 = iVar4 + 6;
            bVar2 = iVar4 + 7;
            bVar3 = iVar4 + 8;
            iVar4 = _gameEngine.GetNumberOfEntityByRefId(logicEntity, iVar4 + 2);

            if (0 < iVar4)
            {
                piVar5 = _gameEngine.StaticVariables.g_activeEntityRefId + iVar4;

                do
                {
                    iVar6 = piVar5;

                    if (iVar9 <= iVar6 + 0x114 && iVar6 + 0x114 <= (int)(iVar9 + (uint)bVar1 * 0x180000) &&
                        iVar8 <= iVar6 + 0x118 && iVar6 + 0x118 <= (int)(iVar8 + (uint)bVar2 * 0x100000) &&
                        iVar7 <= iVar6 + 0x11c && iVar6 + 0x11c <= (int)(iVar7 + (uint)bVar3 * 0x100000))
                    {
                        eventProgramState.Result = 1;
                        return 9;
                    }

                    iVar4 = iVar4 + -1;
                    piVar5 = piVar5 + -1;
                } while (0 < iVar4);
            }

            eventProgramState.Result = 0;
        }

        return 9;*/
    }

    // 800414B4
    private int Script_174_0AE(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        //_gameEngine.DoNothing();
        Environment.Exit(-1);
        return 0;
    }

    // 800414E0
    private int Script_175_0AF(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_targetFadeColorB = variables[1] << 16;
        _gameEngine.StaticVariables.g_targetFadeColorG = variables[2] << 16;
        _gameEngine.StaticVariables.g_targetFadeColorR = variables[3] << 16;
        _gameEngine.StaticVariables.g_fadeFrameCounter = variables[6];
        _gameEngine.StaticVariables.g_warpStepFlags_2 = 1;
        _gameEngine.BeginFadeEffect(variables[4], variables[5]);

        return 7;
    }

    // 80041570
    private int Script_176_0B0(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_playerStartX = variables[1] << 16;
        _gameEngine.StaticVariables.g_playerStartY = variables[2] << 16;
        _gameEngine.StaticVariables.g_playerStartZ = variables[3] << 16;
        _gameEngine.StaticVariables.g_warpFlags = 1;
        _gameEngine.SetFadeDuration(variables[4]);

        return 5;
    }

    // 800415E8
    private int Script_177_0B1(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        if (_gameEngine.StaticVariables.g_warpStepFlags_2 == 0 && _gameEngine.StaticVariables.g_warpFlags == 0)
        {
            eventProgramState.Result = 1;
        }
        else
        {
            eventProgramState.Result = 0;
        }

        return 1;
    }

    // 80041628
    private int Script_CompareEntityGroupsForMatch(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int group1Count;
        int group2Count;
        Entity group2Entity1;
        Entity group2Entity2;
        int group1Ptr2;
        Entity group2Entity3;
        int matchIndex;
        Entity group1EntityList;
        int group1Ptr;

        int[] group1EntitiesBuffer = new int[66];
        group1Count = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);
        group1Ptr = group1EntitiesBuffer;

        if (group1Count == 0)
        {
            eventProgramState.Result = 0;
        }
        else
        {
            group1EntityList = _gameEngine.StaticVariables.g_matchingEntitiesBuffer;

            do
            {
                group2Entity1 = group1EntityList[1];
                group2Entity2 = group1EntityList[2];
                group2Entity3 = group1EntityList[3];
                *group1Ptr = (int)*group1EntityList;
                group1Ptr[1] = (int)group2Entity1;
                group1Ptr[2] = (int)group2Entity2;
                group1Ptr[3] = (int)group2Entity3;
                group1EntityList = group1EntityList + 4;
                group1Ptr = group1Ptr + 4;
            } while (group1EntityList != _gameEngine.StaticVariables.g_matchingEntitiesBuffer + 0x40);

            *group1Ptr = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0x40];

            group2Count = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[2]);

            if (0 < group2Count)
            {
                group1Ptr = _gameEngine.StaticVariables.g_activeEntityRefId + group2Count;

                do
                {
                    matchIndex = 0;
                    group1Ptr2 = group1EntitiesBuffer;

                    if (0 < group1Count)
                    {
                        do
                        {
                            matchIndex = matchIndex + 1;

                            if (*group1Ptr2 == *group1Ptr)
                            {
                                eventProgramState.Result = 1;
                                return 3;
                            }

                            group1Ptr2 = group1Ptr2 + 1;
                        } while (matchIndex < group1Count);
                    }

                    group2Count = group2Count + -1;
                    group1Ptr = group1Ptr + -1;
                } while (0 < group2Count);
            }

            eventProgramState.Result = 0;
        }

        return 3;*/
    }

    // 80041750
    private int Script_UpdatePadState(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        _gameEngine.StaticVariables.g_padState1.ButtonsHold = (ushort)variables[0][1] + (ushort)variables[0][2] * 0x100;
        _gameEngine.StaticVariables.g_padState1.ButtonsJustPressed = (ushort)variables[3] + (ushort)variables[4] * 0x100;
        _gameEngine.StaticVariables.g_padState1.ButtonsReleased = (ushort)variables[5] + (ushort)variables[6] * 0x100;
        _gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval = (ushort)variables[7] + (ushort)variables[8] * 0x100;
        return 9;*/
    }

    // 800417CC
    private int Script_180_0B4(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var iVar1 = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (iVar1 == 0 || _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].ForceZ < 1)
        {
            eventProgramState.Result = 0;
        }
        else
        {
            eventProgramState.Result = 1;
        }

        return 2;
    }

    // 80041830
    private int Script_181_0B5(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (num == 0 || -1 < _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].ForceZ)
        {
            eventProgramState.Result = 0;
        }
        else
        {
            eventProgramState.Result = 1;
        }

        return 2;
    }

    // 80041894
    private int Script_182_0B6(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (num == 0 || _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].ForceZ != 0)
        {
            eventProgramState.Result = 0;
        }
        else
        {
            eventProgramState.Result = 1;
        }

        return 2;
    }

    // 800418F8
    private int Script_183_0B7(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*var bVar1 = (byte)variables[2];
        var iVar2 = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (0 < iVar2)
        {
            var piVar3 = _gameEngine.StaticVariables.g_activeEntityRefId + iVar2;

            do
            {
                if ((uint)(piVar3 + 0x88) == bVar1)
                {
                    eventProgramState.Result = 1;

                    return 3;
                }

                iVar2 += -1;

                piVar3 += -1;
            } while (0 < iVar2);
        }

        eventProgramState.Result = 0;

        return 3;*/
    }

    // 80041988
    private int Script_184_0B8(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*var bVar1 = (byte)variables[2];
        var iVar2 = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (0 < iVar2)
        {
            var piVar3 = _gameEngine.StaticVariables.g_activeEntityRefId + iVar2;

            do
            {
                if ((uint)(piVar3 + 0x90) == bVar1)
                {
                    eventProgramState.Result = 1;
                    return 3;
                }

                iVar2 += -1;
                piVar3 += -1;
            } while (0 < iVar2);
        }

        eventProgramState.Result = 0;

        return 3;*/
    }

    // 80041A18
    private int Script_185_0B9(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StartCdStreaming(variables[1]);
        return 2;
    }

    // 80041A44
    private int Script_186_0BA(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        bool bVar1;
        bVar1 = _gameEngine.CdManager.FUN_8005a7d4();
        eventProgramState.Result = bVar1 ? 1 : 0;

        return 1;
    }

    // 80041A74
    // Menu after died
    private int Script_187_0BB(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        int result;

        if (_gameEngine.StaticVariables.g_currentMap == 0x1dd)
        {
            result = 1;

            if ((_gameEngine.StaticVariables.g_globalFlags[0] & 2U) == 0)
            {
                if (eventProgramState.Parameters[1] == variables[0])
                {
                    if (_gameEngine.StaticVariables.g_debugState < 0)
                    {
                        result = 0xff;

                        if (_gameEngine.StaticVariables.g_transitionCounter != 0xff)
                        {
                            result = _gameEngine.StaticVariables.g_transitionCounter + 1;
                        }

                        //Debug.WriteLine(_gameEngine.StaticVariables.g_debugMessage + iVar2, "Retry = %d", iVar1);
                    }

                    result = eventProgramState.Parameters[2];

                    eventProgramState.Parameters[2] = result + 1;

                    if (result < 0x3c)
                    {
                        result = 0;
                    }
                    else
                    {
                        result = 1;
                        _gameEngine.StaticVariables.g_isGameEnding = 1;
                        _gameEngine.StaticVariables.g_mapTransitionEffectId = 10;
                        _gameEngine.StaticVariables.g_warpEntryBehavior = 0;
                        _gameEngine.StaticVariables.g_desiredMap = 0xb;
                    }
                }
                else
                {
                    _gameEngine.PlayerManager.RestoreHpAndMpAndCreateEffect(_gameEngine.StaticVariables.PlayerEntity);
                    _gameEngine.SoundManager.PlaySoundEffect(0x31);
                    result = 0;
                    eventProgramState.Parameters[1] = variables[0];
                    eventProgramState.Parameters[2] = 0;
                }
            }
            else
            {
                _gameEngine.StaticVariables.g_isGameEnding = 1;
                _gameEngine.StaticVariables.g_mapTransitionEffectId = 0xb;
                _gameEngine.StaticVariables.g_warpEntryBehavior = 0;
            }
        }
        else
        {
            result = 1;
            _gameEngine.StaticVariables.g_isGameEnding = 1;
            _gameEngine.StaticVariables.g_mapTransitionEffectId = 9;
            _gameEngine.StaticVariables.g_warpEntryBehavior = 0;
        }

        return result;
    }

    // 80041C00
    private int Script_188_0BC(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.PlayerEntity.HpMax += variables[1];

        if (0x32 < _gameEngine.StaticVariables.PlayerEntity.HpMax)
        {
            _gameEngine.StaticVariables.PlayerEntity.HpMax = 0x32;
        }

        return 2;
    }

    // 80041C38
    private int Script_189_0BD(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var sfxId = (variables[2] << 8) | variables[1];
        _gameEngine.SoundManager.PlaySoundEffect((uint)sfxId);
        return 3;
    }

    // 80041C6C
    private int Script_190_0BE(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var sfxId = (variables[2] << 8) | variables[1];
        _gameEngine.SoundManager.PlaySoundEffect((uint)sfxId);
        return 3;
    }

    // 80041CA0
    private int Script_191_0BF(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.SoundManager.FUN_80049794(variables[1], variables[3], variables[4]);
        return 5;
    }

    // 80041CDC
    private int Script_192_0C0(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        _gameEngine.PlayerManager.SetPlayerWeaponId((ushort)(variables[1] + 1));
        _gameEngine.StaticVariables.g_playerControlFlags |= 0x80;
        return 2;
    }

    // 80041D18
    private int Script_193_0C1(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_playerControlFlags &= 0xffffff7f;
        return 1;
    }

    // 80041D34
    private int Script_194_0C2(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        var slotId = (variables[2] << 8) | variables[1];

        if ((byte)_gameEngine.StaticVariables.g_currentSaveSlotNameIndex < slotId)
        {
            eventProgramState.Result = 0;
        }
        else
        {
            eventProgramState.Result = 1;
        }

        return 2;
    }

    // 80041D6C
    private int Script_195_0C3(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();

        _gameEngine.StaticVariables.PlayerEntity.Hp = _gameEngine.StaticVariables.PlayerEntity.HpMax;
        _gameEngine.PlayerManager.SetPlayerMp((short)_gameEngine.PlayerManager.GetPlayerMpMax());
        _gameEngine.PlayerManager.InitializeHpAndMp();

        return 1;
    }

    // 80041DA8
    private int Script_196_0C4(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var entityCount = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (entityCount != 0)
        {
            if ((_gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].Flags & 0x800000U) != 0)
            {
                var targetEntity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0];

                //var textureIndex = (eventProgramState.Parameters[2] & 0x07) + targetEntity.PaletteOffset;
                //var textureId = _gameEngine.StaticVariables.g_tPageFadeLUT[textureIndex];

                //var spriteSheetOffset = targetEntity.SpriteSheetOffset + (eventProgramState.Parameters[3] & 0x3F);
                //var textureId2 = _gameEngine.StaticVariables.g_drawPageInfoBase[spriteSheetOffset];

                var image = targetEntity.Frame.Images.Images[targetEntity.CurrentFrameIndex];
                var bitmap = _gameEngine.AlundraMap.GenerateSpriteBitmap(image,
                        _gameEngine.AlundraMap.SpriteInfo.Palettes[image.Palette & 0x1f]);

                _gameEngine.MainInventoryManager.StartHudTransition(
                    targetEntity.PosX,
                    targetEntity.PosY,
                    targetEntity.PosZ,
                    -_gameEngine.StaticVariables.g_cameraScrollingX,
                    -_gameEngine.StaticVariables.g_cameraScrollingY,
                    (byte)eventProgramState.Parameters[4],
                    (byte)eventProgramState.Parameters[5],
                    (short)eventProgramState.Parameters[6],
                    (short)eventProgramState.Parameters[7],
                    bitmap);
            }

            int spriteUpdateId = (variables[3] << 8) | variables[2];
            _gameEngine.TriggerVisualUpdate(spriteUpdateId);
        }

        if (_gameEngine.TryOpenDialog((uint)variables[4], variables[5]) == 0)
        {
            return 0;
        }

        return 6;
    }


    public static Dictionary<int, string> GetHandlerNameByCodes()
    {
        Dictionary<int, string> handlerNameByCodes = new();

        for (var i = 0; i <= 0xff; i++)
        {
            handlerNameByCodes.Add(i, nameof(_Unknown_Handler));
        }

        handlerNameByCodes[0x00] = nameof(Script_DoNothing);
        handlerNameByCodes[0x01] = nameof(Script_DoNothing);
        handlerNameByCodes[0x02] = nameof(Script_2_002);
        handlerNameByCodes[0x03] = nameof(Script_3_003);
        handlerNameByCodes[0x04] = nameof(Script_4_004);
        handlerNameByCodes[0x05] = nameof(Script_5_005);
        handlerNameByCodes[0x06] = nameof(Script_6_006);
        handlerNameByCodes[0x07] = nameof(Script_7_007);
        handlerNameByCodes[0x08] = nameof(Script_8_008);
        handlerNameByCodes[0x09] = nameof(Script_9_009);
        handlerNameByCodes[0x0A] = nameof(Script_10_00A);
        handlerNameByCodes[0x0B] = nameof(Script_11_00B);
        handlerNameByCodes[0x0C] = nameof(Script_12_00C);
        handlerNameByCodes[0x0D] = nameof(Script_OpenDialog_13_00D);
        handlerNameByCodes[0x0E] = nameof(Script_DoNothing);
        handlerNameByCodes[0x0F] = nameof(Script_DoNothing);
        handlerNameByCodes[0x10] = nameof(Script_16_010);
        handlerNameByCodes[0x11] = nameof(Script_17_011);
        handlerNameByCodes[0x12] = nameof(Script_18_012);
        handlerNameByCodes[0x13] = nameof(Script_DoNothing);
        handlerNameByCodes[0x14] = nameof(Script_DoNothing2);
        handlerNameByCodes[0x15] = nameof(Script_21_015);
        handlerNameByCodes[0x16] = nameof(Script_22_016);
        handlerNameByCodes[0x17] = nameof(Script_23_017);
        handlerNameByCodes[0x18] = nameof(Script_DoNothing);
        handlerNameByCodes[0x19] = nameof(Script_25_019);
        handlerNameByCodes[0x1A] = nameof(Script_26_01A);
        handlerNameByCodes[0x1B] = nameof(Script_27_01B);
        handlerNameByCodes[0x1C] = nameof(Script_28_01C);
        handlerNameByCodes[0x1D] = nameof(Script_29_01D);
        handlerNameByCodes[0x1E] = nameof(Script_30_01E);
        handlerNameByCodes[0x1F] = nameof(Script_31_01F);
        handlerNameByCodes[0x20] = nameof(Script_32_020);
        handlerNameByCodes[0x21] = nameof(Script_33_021);
        handlerNameByCodes[0x22] = nameof(Script_34_022);
        handlerNameByCodes[0x23] = nameof(Script_35_023);
        handlerNameByCodes[0x24] = nameof(Script_36_024);
        handlerNameByCodes[0x25] = nameof(Script_37_025);
        handlerNameByCodes[0x26] = nameof(Script_38_026);
        handlerNameByCodes[0x27] = nameof(Script_39_027);
        handlerNameByCodes[0x28] = nameof(Script_40_028);
        handlerNameByCodes[0x29] = nameof(Script_41_029);
        handlerNameByCodes[0x2A] = nameof(Script_42_02A);
        handlerNameByCodes[0x2B] = nameof(Script_43_02B);
        handlerNameByCodes[0x2C] = nameof(Script_44_02C);
        handlerNameByCodes[0x2D] = nameof(Script_45_02D);
        handlerNameByCodes[0x2E] = nameof(Script_46_02E);
        handlerNameByCodes[0x2F] = nameof(Script_47_02F);
        handlerNameByCodes[0x30] = nameof(Script_48_030);
        handlerNameByCodes[0x31] = nameof(Script_49_031);
        handlerNameByCodes[0x32] = nameof(Script_50_032);
        handlerNameByCodes[0x33] = nameof(Script_51_033);
        handlerNameByCodes[0x34] = nameof(Script_52_034);
        handlerNameByCodes[0x35] = nameof(Script_53_035);
        handlerNameByCodes[0x36] = nameof(Script_54_036);
        handlerNameByCodes[0x37] = nameof(Script_55_037);
        handlerNameByCodes[0x38] = nameof(Script_56_038);
        handlerNameByCodes[0x39] = nameof(Script_57_039);
        handlerNameByCodes[0x3A] = nameof(Script_58_03A);
        handlerNameByCodes[0x3B] = nameof(Script_59_03B);
        handlerNameByCodes[0x3C] = nameof(Script_60_03C);
        handlerNameByCodes[0x3D] = nameof(Script_61_03D);
        handlerNameByCodes[0x3E] = nameof(Script_62_03E);
        handlerNameByCodes[0x3F] = nameof(Script_63_03F);
        handlerNameByCodes[0x40] = nameof(Script_64_040);
        handlerNameByCodes[0x41] = nameof(Script_65_041);
        handlerNameByCodes[0x42] = nameof(Script_66_042);
        handlerNameByCodes[0x43] = nameof(Script_67_043);
        handlerNameByCodes[0x44] = nameof(Script_68_044);
        handlerNameByCodes[0x45] = nameof(Script_69_045);
        handlerNameByCodes[0x46] = nameof(Script_70_046);
        handlerNameByCodes[0x47] = nameof(Script_71_047);
        handlerNameByCodes[0x48] = nameof(Script_72_048);
        handlerNameByCodes[0x49] = nameof(Script_73_049);
        handlerNameByCodes[0x4A] = nameof(Script_74_04A);
        handlerNameByCodes[0x4B] = nameof(Script_75_04B);
        handlerNameByCodes[0x4C] = nameof(Script_76_04C);
        handlerNameByCodes[0x4D] = nameof(Script_77_04D);
        handlerNameByCodes[0x4E] = nameof(Script_78_04E);
        handlerNameByCodes[0x4F] = nameof(Script_ActivateDebugTextAutoAdvance);
        handlerNameByCodes[0x50] = nameof(Script_SetEtcAnimationMode);
        handlerNameByCodes[0x51] = nameof(Script_TryActivateTextHoldState);
        handlerNameByCodes[0x52] = nameof(Script_82_052);
        handlerNameByCodes[0x53] = nameof(Script_83_053);
        handlerNameByCodes[0x54] = nameof(Script_84_054);
        handlerNameByCodes[0x55] = nameof(Script_85_055);
        handlerNameByCodes[0x56] = nameof(Script_86_056);
        handlerNameByCodes[0x57] = nameof(Script_87_057);
        handlerNameByCodes[0x58] = nameof(Script_88_058);
        handlerNameByCodes[0x59] = nameof(Script_89_059);
        handlerNameByCodes[0x5A] = nameof(Script_90_05A);
        handlerNameByCodes[0x5B] = nameof(Script_91_05B);
        handlerNameByCodes[0x5C] = nameof(Script_OpenDialogWithChoice);
        handlerNameByCodes[0x5D] = nameof(Script_93_05D);
        handlerNameByCodes[0x5E] = nameof(Script_94_05E);
        handlerNameByCodes[0x5F] = nameof(Script_WaitForAnimOrDistance);
        handlerNameByCodes[0x60] = nameof(Script_96_060);
        handlerNameByCodes[0x61] = nameof(Script_97_061);
        handlerNameByCodes[0x62] = nameof(Script_98_062);
        handlerNameByCodes[0x63] = nameof(Script_99_063);
        handlerNameByCodes[0x64] = nameof(Script_100_064);
        handlerNameByCodes[0x65] = nameof(Script_101_065);
        handlerNameByCodes[0x66] = nameof(Script_CopyLogicContextAndAssignScript);
        handlerNameByCodes[0x67] = nameof(Script_103_067);
        handlerNameByCodes[0x68] = nameof(Script_104_068);
        handlerNameByCodes[0x69] = nameof(Script_105_069);
        handlerNameByCodes[0x6A] = nameof(Script_106_06A);
        handlerNameByCodes[0x6B] = nameof(Script_107_06B);
        handlerNameByCodes[0x6C] = nameof(Script_108_06C);
        handlerNameByCodes[0x6D] = nameof(Script_109_06D);
        handlerNameByCodes[0x6E] = nameof(Script_110_06E);
        handlerNameByCodes[0x6F] = nameof(Script_111_06F);
        handlerNameByCodes[0x70] = nameof(Script_112_070);
        handlerNameByCodes[0x71] = nameof(Script_113_071);
        handlerNameByCodes[0x72] = nameof(Script_114_072);
        handlerNameByCodes[0x73] = nameof(Script_115_073);
        handlerNameByCodes[0x74] = nameof(Script_116_074);
        handlerNameByCodes[0x75] = nameof(Script_117_075);
        handlerNameByCodes[0x76] = nameof(Script_118_076);
        handlerNameByCodes[0x77] = nameof(Script_119_077);
        handlerNameByCodes[0x78] = nameof(Script_StoreChoiceParamAndJump);
        handlerNameByCodes[0x79] = nameof(Script_JumpIfChoiceAccepted);
        handlerNameByCodes[0x7A] = nameof(Script_JumpIfChoiceRejected);
        handlerNameByCodes[0x7B] = nameof(Script_JumpIfFlagSetStoreParam);
        handlerNameByCodes[0x7C] = nameof(Script_JumpIfFlagClearStoreParam);
        handlerNameByCodes[0x7D] = nameof(Script_JumpRelativeFromStoredParam);
        handlerNameByCodes[0x7E] = nameof(Script_ConditionalJumpFromStoredParamIfTrue);
        handlerNameByCodes[0x7F] = nameof(Script_ConditionalJumpFromStoredParamIfFalse);
        handlerNameByCodes[0x80] = nameof(Script_JumpFromStoredParamIfFlagSet);
        handlerNameByCodes[0x81] = nameof(Script_JumpFromStoredParamIfFlagClear);
        handlerNameByCodes[0x82] = nameof(Script_130_082);
        handlerNameByCodes[0x83] = nameof(Script_131_083);
        handlerNameByCodes[0x84] = nameof(Script_132_084);
        handlerNameByCodes[0x85] = nameof(Script_133_085);
        handlerNameByCodes[0x86] = nameof(Script_134_086);
        handlerNameByCodes[0x87] = nameof(Script_135_087);
        handlerNameByCodes[0x88] = nameof(Script_136_088);
        handlerNameByCodes[0x89] = nameof(Script_137_089);
        handlerNameByCodes[0x8A] = nameof(Script_138_08A);
        handlerNameByCodes[0x8B] = nameof(Script_139_08B);
        handlerNameByCodes[0x8C] = nameof(Script_140_08C);
        handlerNameByCodes[0x8D] = nameof(Script_141_08D);
        handlerNameByCodes[0x8E] = nameof(Script_142_08E);
        handlerNameByCodes[0x8F] = nameof(Script_143_08F);
        handlerNameByCodes[0x90] = nameof(Script_144_090);
        handlerNameByCodes[0x91] = nameof(Script_145_091);
        handlerNameByCodes[0x92] = nameof(Script_146_092);
        handlerNameByCodes[0x93] = nameof(Script_147_093);
        handlerNameByCodes[0x94] = nameof(Script_148_094);
        handlerNameByCodes[0x95] = nameof(Script_149_095);
        handlerNameByCodes[0x96] = nameof(Script_150_096);
        handlerNameByCodes[0x97] = nameof(Script_151_097);
        handlerNameByCodes[0x98] = nameof(Script_152_098);
        handlerNameByCodes[0x99] = nameof(Script_153_099);
        handlerNameByCodes[0x9A] = nameof(Script_154_09A);
        handlerNameByCodes[0x9B] = nameof(Script_155_09B);
        handlerNameByCodes[0x9C] = nameof(Script_156_09C);
        handlerNameByCodes[0x9D] = nameof(Script_157_09D);
        handlerNameByCodes[0x9E] = nameof(Script_158_09E);
        handlerNameByCodes[0x9F] = nameof(Script_159_09F);
        handlerNameByCodes[0xA0] = nameof(Script_160_0A0);
        handlerNameByCodes[0xA1] = nameof(Script_161_0A1);
        handlerNameByCodes[0xA2] = nameof(Script_162_0A2);
        handlerNameByCodes[0xA3] = nameof(Script_163_0A3);
        handlerNameByCodes[0xA4] = nameof(Script_164_0A4);
        handlerNameByCodes[0xA5] = nameof(Script_165_0A5);
        handlerNameByCodes[0xA6] = nameof(Script_166_0A6);
        handlerNameByCodes[0xA7] = nameof(Script_167_0A7);
        handlerNameByCodes[0xA8] = nameof(Script_168_0A8);
        handlerNameByCodes[0xA9] = nameof(Script_169_0A9);
        handlerNameByCodes[0xAA] = nameof(Script_170_0AA);
        handlerNameByCodes[0xAB] = nameof(Script_171_0AB);
        handlerNameByCodes[0xAC] = nameof(Script_172_0AC);
        handlerNameByCodes[0xAD] = nameof(Script_173_0AD);
        handlerNameByCodes[0xAE] = nameof(Script_174_0AE);
        handlerNameByCodes[0xAF] = nameof(Script_175_0AF);
        handlerNameByCodes[0xB0] = nameof(Script_176_0B0);
        handlerNameByCodes[0xB1] = nameof(Script_177_0B1);
        handlerNameByCodes[0xB2] = nameof(Script_CompareEntityGroupsForMatch);
        handlerNameByCodes[0xB3] = nameof(Script_UpdatePadState);
        handlerNameByCodes[0xB4] = nameof(Script_180_0B4);
        handlerNameByCodes[0xB5] = nameof(Script_181_0B5);
        handlerNameByCodes[0xB6] = nameof(Script_182_0B6);
        handlerNameByCodes[0xB7] = nameof(Script_183_0B7);
        handlerNameByCodes[0xB8] = nameof(Script_184_0B8);
        handlerNameByCodes[0xB9] = nameof(Script_185_0B9);
        handlerNameByCodes[0xBA] = nameof(Script_186_0BA);
        handlerNameByCodes[0xBB] = nameof(Script_187_0BB);
        handlerNameByCodes[0xBC] = nameof(Script_188_0BC);
        handlerNameByCodes[0xBD] = nameof(Script_189_0BD);
        handlerNameByCodes[0xBE] = nameof(Script_190_0BE);
        handlerNameByCodes[0xBF] = nameof(Script_191_0BF);
        handlerNameByCodes[0xC0] = nameof(Script_192_0C0);
        handlerNameByCodes[0xC1] = nameof(Script_193_0C1);
        handlerNameByCodes[0xC2] = nameof(Script_194_0C2);
        handlerNameByCodes[0xC3] = nameof(Script_195_0C3);
        handlerNameByCodes[0xC4] = nameof(Script_196_0C4);

        return handlerNameByCodes;
    }

}