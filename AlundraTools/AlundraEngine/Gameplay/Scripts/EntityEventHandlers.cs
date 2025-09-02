using AlundraEngine.DatasBin;
using System.Collections.Generic;
using System.Diagnostics;

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
        _handlers[0x0D] = Script_13_00D;
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
        _handlers[0x5C] = Script_UpdateCameraToEntityAndCheckCondition;
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
        _handlers[0x78] = Script_120_078;
        _handlers[0x79] = Script_121_079;
        _handlers[0x7A] = Script_122_07A;
        _handlers[0x7B] = Script_123_07B;
        _handlers[0x7C] = Script_124_07C;
        _handlers[0x7D] = Script_125_07D;
        _handlers[0x7E] = Script_126_07E;
        _handlers[0x7F] = Script_127_07F;
        _handlers[0x80] = Script_128_080;
        _handlers[0x81] = Script_129_081;
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

                    if (eventProgramState.Codes != null) //.Exp[0] != 0 && eventProgramState.Sp != 0)
                    {
                        goto SET_LOGIC_MODE;
                    }
                    break;

                case ScriptHelper.ProgramCTick:
                    eventProgramState = entity.EventProgramState;

                    if (eventProgramState.Codes != null) //Exp[0] != 0 && eventProgramState.Sp != 0)
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

            if (command == 0xFF)
            {
                //Debug.WriteLine($"Entity[{entity.Index}] end script");
                goto END_SCRIPT;
            }

            if (command == 0x00) // break, skip the loop but do the next command
            {
                //Debug.WriteLine($"Entity[{entity.Index}] break");
                eventProgramState.Exp[1] = 0;
                eventProgramState.CodeIndex++;
                FillDataFromCommand(eventProgramState); // needed because there is a check at the beginning of the function
                goto END_SCRIPT;
            }

            var logicContextEntity = entity.LogicContextEntity;
            var lastCommand = _gameEngine.StaticVariables.g_activeCommand;
            _gameEngine.StaticVariables.g_activeCommand = command;
            
            //LogCommand(entity, logicMode, command, variables);

            var func = _handlers[command];
            var result = func(entity.LogicContextEntity, entity, variables, eventProgramState);
            
            //Debug.WriteLine($"{result}");

            _gameEngine.StaticVariables.g_lastCommand = lastCommand;

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

            eventProgramState.Exp[1] = 0;
            eventProgramState.CodeIndex += result;
        }

        END_SCRIPT:
        if (wasEntityCleared)
        {
            //Debug.WriteLine($"Entity[{entity.Index}] clean EventProgramState 2");
            eventProgramState.Sp = 0;
            eventProgramState.Codes = null;
        }
    }

    private static void LogCommand(Entity entity, int logicMode, int command, int[] variables)
    {
        var name = SpriteInfoEventCodes.CommandNameByCode.GetValueOrDefault((byte)command, "?");
        var eventTypeName = logicMode == 0 ? "ALoad" : logicMode == 1 ? "BMap" : logicMode == 2 ? "CTick" : logicMode == 3 ? "DTouch" : logicMode == 4 ? "EDeactivate" : "FInteract";
        Debug.Write($"Entity[{entity.Index}] run {eventTypeName} command 0x{command:x2} '{name}' {string.Join(',', variables.Select(x => x.ToString("x2")))} = ");
    }

    private int[] FillDataFromCommand(EventProgramState eventProgramState)
    {
        if (eventProgramState.Codes == null || eventProgramState.CodeIndex >= eventProgramState.Codes.Length)
        {
            return [0xFF];
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
        Array.Clear(eventProgramState.Exp);

        eventProgramState.Codes = GetEventCodes(br, entity.ProgramIndexes[eventProgramType], eventCodesTable, spriteInfo);
        eventProgramState.Sp = eventProgramState.Codes?.Length > 0 ? eventProgramState.Codes[eventProgramState.CodeIndex] : 0;
        eventProgramState.Exp[0] = eventProgramState.Sp;
    }

    public static List<SiCommand> GetEventCodeCommands(BinaryReader br, int index, short[] eventCodesTable, SpriteInfo spriteInfo)
    {
        if (index > 0 && index < 0xff)
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
                return spriteInfo?.EventCodes?.GetCommands(br, eventCodesTable[i], false, size);
            }

            return spriteInfo?.EventCodes?.GetCommands(br, eventCodesTable[i]);
        }
        return [];
    }


    public static byte[] GetEventCodes(BinaryReader br, int index, short[] eventCodesTable, SpriteInfo spriteInfo)
    {
        if (index > 0 && index < 0xff)
        {
            var i = index & 0x7f;
            if (i < eventCodesTable.Length)
            {
                //var size = eventCodesTable[i + 1] - eventCodesTable[i];
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
    /*
    public int _0c_SetRandomDir_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var i = _gameEngine.StaticVariables.g_gameRandomSeed;
        var val1 = (int)(i * 0x7d2b89dd);
        var val2 = (int)(0xe06a02e7 + val1);
        var val3 = (int)(((long)val2 * 4) >> 32);
        var dir = (uint)_gameEngine.StaticVariables.g_cardinalDirectionTable[val3];//val3 here is a number between 0 and 3
        _gameEngine.StaticVariables.g_gameRandomSeed = (uint)val2;
        entity.TargetDirection = dir;
        return 1;
    }

    public int _0d_Dialog_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        if ((entity.Flags & 0x800000) != 0)//has portrait
        {
            //TODO get it from memory, not disk
            //SIImageSet portrait = entity.SpriteRecord.GetPortraitImageset(datasReader);
            //var img = portrait.Images[0];
            //var bmps = gameState.GetSpriteImages(portrait);
            //var bmp = bmps[0];
            //WrapsDialogSetupPortrait(entity.PosX, entity.PosY, entity.PosZ, gameState.g_hudCurrentX, gameState.g_hudCurrentY, img.Sx, img.Sy, img.Swidth, img.Sheight, bmp);
        }
        //SetName(entity.NameId);

        //var ret = SetText(exp[1], exp[2]);

        //if (ret > 0)
        //    return 3;
        return 0;
    }

    public int _10_LoseControl_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_playerControlFlags |= 0x4;
        return 1;
    }

    public int _11_GainControl_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_playerControlFlags &= ~0x4;
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

        entity.PosZ = (entity.EntityRecord.Height * 8 - entity.ModZ) << 16;
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
        force <<= 16;//sign extend
        force >>= 8;//get it to the correct multiple
        entity.ForceZ = force;
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
            eventProgramState.Exp[2] = entity.PosX;
            eventProgramState.Exp[3] = entity.PosY;
            return 0;
        }

        var x = Math.Abs(eventProgramState.Exp[2] - entity.PosX) >> 16;
        var y = Math.Abs(eventProgramState.Exp[3] - entity.PosY) >> 16;

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
        entity.TargetDirection = (uint)ScriptHelper.GetDirectionToTarget(_gameEngine.StaticVariables.PlayerEntity.PosX - entity.PosX, _gameEngine.StaticVariables.PlayerEntity.PosY - entity.PosY);
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
        var numEntities = _gameEngine.GetNumberOfEntityByRefId(entity, entityId);
        for (var dex = 0; dex < numEntities; dex++)
        {
            var checkme = _gameEngine.StaticVariables.g_entitySlots[dex];
            _gameEngine.EntityGameplayManager.HideEntity(checkme);
        }

        return 2;
    }

    public int _2f_CheckPlayerInput_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var inputId = exp[3];
        var mask = exp[1] | exp[2] << 8;

        //_gameEngine.StaticVariables.g_padState1.ButtonsHold
        //if ((_gameEngine.PlayerInput[inputid] & mask) != 0)
        if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & mask) != 0)
        {
            eventProgramState.Result = 1;
        }
        else
        {
            eventProgramState.Result = 0;
        }

        return 4;
    }

    public int _30_IfFlagOff_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var flagData = exp[1] + (exp[2] << 8);
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
        if ((flags[flag] & (1 << bitToCheck)) != 0)
        {
            int jumpOffset = (short)(exp[3] | exp[4] << 8);
            return jumpOffset;
        }

        return 5;
    }

    public int _31_IfFlagOn_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var flagData = exp[1] + (exp[2] << 8);
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
            int jumpOffset = (short)(exp[3] | exp[4] << 8);
            return jumpOffset;
        }

        return 5;
    }

    public int _32_FlagToggle_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        if (eventProgramState.Result != 0)
        {
            return 3;
        }

        var flagData = exp[1] + (exp[2] << 8);
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

        var bitToSet = flagData & 0x1f;

        //toggle the bit for this flag
        flags[flag] ^= (uint)1 << bitToSet;//xor, toggles

        return 3;
    }

    public int _33_CheckFlagsOn_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        //do this 4 times
        {
            var flagData = exp[1] + (exp[2] << 8);
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
            var flagData = exp[3] + (exp[4] << 8);
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
            var flagData = exp[5] + (exp[6] << 8);
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
            var flagData = exp[7] + (exp[8] << 8);
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

    public int _34_CheckFlagsOff_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        //do this 4 times
        {
            var flagData = exp[1] + (exp[2] << 8);
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
            if ((flags[flag] & (1 << bitToCheck)) != 0)
            {
                eventProgramState.Result = 0;
                return 9;
            }
        }

        {
            var flagData = exp[3] + (exp[4] << 8);
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
            if ((flags[flag] & (1 << bitToCheck)) != 0)
            {
                eventProgramState.Result = 0;
                return 9;
            }
        }

        {
            var flagData = exp[5] + (exp[6] << 8);
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
            if ((flags[flag] & (1 << bitToCheck)) != 0)
            {
                eventProgramState.Result = 0;
                return 9;
            }
        }

        {
            var flagData = exp[7] + (exp[8] << 8);
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
            if ((flags[flag] & (1 << bitToCheck)) != 0)
            {
                eventProgramState.Result = 0;
                return 9;
            }
        }

        eventProgramState.Result = 1;//made it through them all
        return 9;
    }

    //blocks until flag is off
    public int _35_UntilFlagOff_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var flagData = exp[1] + (exp[2] << 8);
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
            return 3;
        }

        return 0;
    }

    //blocks until flag is on
    public int _36_UntilFlagOn_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var flagData = exp[1] + (exp[2] << 8);
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
        if ((flags[flag] & (1 << bitToCheck)) != 0)
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

        var checkme = _gameEngine.StaticVariables.PlayerEntity;
        if (checkme.TileX >= x1 && checkme.TileX <= x2
                                && checkme.TileY >= y1 && checkme.TileY <= y2
                                && checkme.TileZ >= z1 && checkme.TileZ <= z2)
        {
            eventProgramState.Result = 1;
            return 7;
        }

        eventProgramState.Result = 0;

        return 7;
    }

    public int _40_SetProgramIndex_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var programid = exp[1];
        var indexval = exp[2];

        //somevariable = 1;
        _gameEngine.StaticVariables.g_clearProgramState = 1;

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
        if (eventProgramState.Result != 0)
        {
            return eventProgramState.Exp[0] - entity.EventProgramState.Sp;
        }

        return 1;
    }

    public int _4b_IfFalseRestart_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        if (eventProgramState.Result == 0)
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
        var jumpOffset = (short)(exp[entity.CurrentFrameIndex * 2 + 1] | exp[entity.CurrentFrameIndex * 2 + 2]);
        return jumpOffset;
    }

    public int _59_SetEntityAnim_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var entityId = exp[1];
        int animid = exp[2];
        var numEntities = _gameEngine.GetNumberOfEntityByRefId(entity, entityId);
        for (var i = 0; i < numEntities; i++)
        {
            var dome = _gameEngine.StaticVariables.g_entitySlots[i];
            dome.TargetAnimationId = (uint)animid;
        }

        return 3;
    }

    public int _5a_TurnEntity_Handler(Entity entity, Entity entitySelf, int[] exp, EventProgramState eventProgramState)
    {
        var entityId = exp[1];
        var turncode = exp[2];

        var numEntities = _gameEngine.GetNumberOfEntityByRefId(entity, entityId);
        for (var i = 0; i < numEntities; i++)
        {
            var dome = _gameEngine.StaticVariables.g_entitySlots[i];
            dome.TargetDirection = _gameEngine.EntityGameplayManager.TurnEntity(entity, turncode);
        }

        return 3;
    }
        

    */
    //=========================================================================================
    //=========================================================================================
    //=========================================================================================
    //=========================================================================================
    //=========================================================================================
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
        return ((variables[1] + variables[2] * 0x100) * 0x10000) >> 0x10;
    }

    // 8003D1A0
    private int Script_3_003(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var result = 3;

        if (eventProgramState.Result != 0)
        {
            result = (variables[1] + variables[2] * 0x100) * 0x10000 >> 0x10;
        }

        return result;
    }

    // 8003D1D8
    private int Script_4_004(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var result = 3;

        if (eventProgramState.Result == 0)
        {
            result = ((variables[1] + variables[2] * 0x100) * 0x10000) >> 0x10;
        }

        return result;
    }

    // 8003D210
    private int Script_5_005(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        uint[] flags;

        var key = (uint)(variables[1] + variables[2] * 0x100);  // variables[2] << 8 | variables[1];

        if ((key & 0x8000) == 0)
        {
            flags = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            flags = _gameEngine.StaticVariables.g_globalFlags;
        }

        var index = (key >> 3) & 0xffc;
        var mask = (uint)(1 << (variables[1] & 0x1f));
        flags[index] |= mask;

        return 3;
    }

    // 8003D288
    private int Script_6_006(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        uint[] flags;

        var uVar2 = (uint)(variables[1] + variables[2] * 0x100);

        if ((uVar2 & 0x8000) == 0)
        {
            flags = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            flags = _gameEngine.StaticVariables.g_globalFlags;
        }

        var puVar3 = flags[uVar2 >> 3 & 0xffc];
        flags[uVar2 >> 3 & 0xffc] = (uint)(puVar3 & ~(1 << (variables[1] & 0x1f)));

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

        if (eventProgramState.Exp[1] == variables[0])
        {
            var x = eventProgramState.Exp[2] - logicEntity.PosX;
            var y = eventProgramState.Exp[3] - logicEntity.PosY;

            if (x < 0)
            {
                x = -x;
            }

            if (y < 0)
            {
                y = -y;
            }

            var uVar2 = (uint)eventProgramState.Exp[3];
            result = 0;
            if ((int)uVar2 <= x >> 0x10 || (int)uVar2 <= y >> 0x10)
            {
                result = 4;
            }
        }
        else
        {
            eventProgramState.Exp[1] = variables[0];
            eventProgramState.Exp[2] = logicEntity.PosX;
            eventProgramState.Exp[3] = logicEntity.PosY;
            result = 0;
        }

        return result;
    }

    // 8003D518
    private int Script_12_00C(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;
        logicEntity.TargetDirection = (uint)_gameEngine.StaticVariables.g_cardinalDirectionTable[_gameEngine.StaticVariables.g_gameRandomSeed * 4 >> 0x20];
        return 1;
    }

    // 8003D578
    private int Script_13_00D(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        if ((logicEntity.Flags & 0x800000U) != 0) // has portrait
        {
            //var iVar2 = logicEntity.SpriteRecord.Header.FramesPointer;
            //
            ////WrapsDialogSetupPortrait
            //_gameEngine.StartHudTransition(
            //    logicEntity.PosX, logicEntity.PosY, logicEntity.PosZ,
            //    _gameEngine.StaticVariables.g_cameraScrollingX, _gameEngine.StaticVariables.g_cameraScrollingY,
            //
            //    (iVar2 + 4), (iVar2 + 5), (ushort)(iVar2 + 6), (ushort)(iVar2 + 7),
            //    _gameEngine.StaticVariables.g_drawPageInfoBase[logicEntity.SheetSize + ((iVar2 + 3) & 0x3f)],
            //    _gameEngine.StaticVariables.g_tPageFadeLUT[logicEntity.PaletteIndex + ((iVar2 + 2) & 7)]);

            Debugger.Break();

            using var binaryReader = _gameEngine.DatasBin.OpenBin();
            var imgset = logicEntity.SpriteRecord.GetPortraitImageset(binaryReader);
            var img = imgset.Images[0];
            var bmp = _gameEngine.CurrentMap.GenerateSpriteBitmap(
                imgset.Images[0],
                _gameEngine.CurrentMap.SpriteInfo.Palettes[imgset.Images[0].Palette & 0x1f]);

            _gameEngine.MainInventoryManager.StartHudTransition(
                logicEntity.PosX, logicEntity.PosY, logicEntity.PosZ,
                _gameEngine.StaticVariables.g_cameraScrollingX, _gameEngine.StaticVariables.g_cameraScrollingY,
                img.Sx, img.Sy, img.Swidth, img.Sheight,
                /*_gameEngine.StaticVariables.g_drawPageInfoBase[logicEntity.SpriteSheetOffset + ((img.Spritesheet) & 0x3f)],
                _gameEngine.StaticVariables.g_tPageFadeLUT[logicEntity.PaletteOffset + ((img.Palette) & 7)]*/
                img);

            //TODO get it from memory, not disk
            //SIImageSet portrait = entity.SpriteRecord.GetPortraitImageset(datasReader);
            //var img = portrait.Images[0];
            //var bmps = gameState.GetSpriteImages(portrait);
            //var bmp = bmps[0];
            //WrapsDialogSetupPortrait(entity.PosX, entity.PosY, entity.PosZ,
            //  gameState.g_hudCurrentX, gameState.g_hudCurrentY,
            //  img.Sx, img.Sy, img.Swidth, img.Sheight, bmp);
        }

        //Debugger.Break();
        _gameEngine.TriggerVisualUpdate((int)logicEntity.SpriteTableIndex);
        var res = _gameEngine.TryPlayEtcAnimation((uint)variables[1], variables[2]);  
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
        logicEntity.ForceZ = ((variables[1] | (variables[2] << 8)) << 16) >> 8;
        return 3;
    }

    // 8003D7FC
    private int Script_28_01C(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        if (eventProgramState.Exp[1] != variables[0])
        {
            eventProgramState.Exp[1] = variables[0];
            eventProgramState.Exp[2] = 0;
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

        eventProgramState.Exp[2] += 1;
        logicEntity.AnimCompleteCounter = 0;

        LAB_8003d868:
        return (eventProgramState.Exp[2] < (variables[1] ^ 1) ? 1 : 0) << 1;
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
        var signature =  variables[0] | (variables[1] <<  8) | (variables[2] << 16);

        if (eventProgramState.Exp[1] != signature)
        {
            eventProgramState.Exp[1] = signature;
            eventProgramState.Exp[2] = logicEntity.PosX;
            eventProgramState.Exp[3] = logicEntity.PosY;
            return 0;
        }

        var dx = eventProgramState.Exp[2] - logicEntity.PosX;
        var dy = eventProgramState.Exp[3] - logicEntity.PosY;

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

        var threshold =  (variables[2] << 8) | variables[1];

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

        if (eventProgramState.Exp[1] == iVar3)
        {
            iVar2 = eventProgramState.Exp[2] - logicEntity.PosZ;

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
            eventProgramState.Exp[1] = iVar3;
            eventProgramState.Exp[2] = logicEntity.PosZ;
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

        if (eventProgramState.Exp[1] == variables[0])
        {
            if (eventProgramState.Exp[2] == logicEntity.PosZ)
            {
                result = 1;
            }
            else
            {
                var zForce = eventProgramState.Exp[2] - logicEntity.PosZ;

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
            eventProgramState.Exp[1] = variables[0];
            var entityRecord = logicEntity.EntityRecord;
            if (entityRecord == null)
            {
                //_gameEngine.PrintCommandMap();
                Debugger.Break();
            }

            eventProgramState.Exp[2] = entityRecord.Height << 0x13;
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
        var flag = (uint)(variables[1] + variables[2] * 0x100);

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
            result = ((variables[3] + variables[4] * 0x100) * 0x10000) >> 0x10;
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
            result = ((variables[3] + variables[4] * 0x100) * 0x10000) >> 0x10;
        }

        return result;
    }

    // 8003DEFC
    private int Script_50_032(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        uint[] piVar1;

        uint uVar2;

        uint puVar3;

        uVar2 = variables[1] + variables[2] * 0x100;

        if ((uVar2 & 0x8000) == 0)
        {
            piVar1 = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            piVar1 = _gameEngine.StaticVariables.g_globalFlags;
        }

        puVar3 = (uint)((uVar2 >> 3 & 0xffc) + piVar1);

        puVar3 = puVar3 ^ 1 << (variables[1] & 0x1f);

        return 3;*/
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

        /*uint[] piVar1;
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

        if (((uint)((uVar2 >> 3 & 0xffc) + piVar1) & 1 << ((iVar3 + 1) & 0x1f)) != 0)
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

            if (((uint)((uVar2 >> 3 & 0xffc) + piVar1) & 1 << ((iVar3 + 3) & 0x1f)) != 0)
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

                if (((uint)((uVar2 >> 3 & 0xffc) + piVar1) & 1 << ((iVar3 + 5) & 0x1f)) != 0)
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

                    if (((uint)((uVar2 >> 3 & 0xffc) + piVar1) & 1 << (variables[7] & 0x1f)) != 0)
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
        Debugger.Break();
        return 0;
        /*
        uint[] piVar1;

        int iVar2;

        uint uVar3;

        uVar3 = variables[1] + variables[2] * 0x100;

        if ((uVar3 & 0x8000) == 0)
        {
            piVar1 = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            piVar1 = _gameEngine.StaticVariables.g_globalFlags;
        }

        iVar2 = 0;

        if (((uint)((uVar3 >> 3 & 0xffc) + piVar1) & 1 << (variables[1] & 0x1f)) == 0)
        {
            iVar2 = 3;
        }

        return iVar2;*/
    }

    // 8003E35C
    private int Script_54_036(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        uint[] flags;
        uint key;

        key = (uint)(variables[1] + variables[2] * 0x100); // variables[2] << 8 | variables[1];

        if ((key & 0x8000) == 0)
        {
            flags = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            flags = _gameEngine.StaticVariables.g_globalFlags;
        }

        var index = (key >> 3) & 0xffc;
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

        if (command != eventProgramState.Exp[1])
        {
            eventProgramState.Exp[1] = command;
            eventProgramState.Exp[2] = 0;
            return 0;
        }

        eventProgramState.Exp[2]++;
        var toWait = variables[1];

        if (eventProgramState.Exp[2] >= toWait)
        {
            return 2;
        }

        return 0;
    }

    // 8003E424
    private int Script_56_038(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_mapIdToInternalMapIndexTable[variables[1]] = (ushort)(variables[3] + variables[4] * 0x100);
        return 5;
    }

    // 8003E464
    private int Script_57_039(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        return _gameEngine.FUN_8004248c() == 0 ? 0 : 1;
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
            && _gameEngine.StaticVariables.PlayerEntity.TileZ <= variables[6] )
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
        Debugger.Break();
        return 0;
        /*
        int iVar1;
        iVar1 = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (iVar1 < 1)
        {
            eventProgramState.Result = 0;
        }
        else
        {
            ownerEntity.LogicContextEntity = _gameEngine.StaticVariables.g_activeEntityRefId[iVar1];
            eventProgramState.Result = 1;
        }

        return 2;*/
    }

    // 8003E88C
    private int Script_68_044(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        if (eventProgramState.Exp[1] == variables[0])
        {
            if (_gameEngine.StaticVariables.INT_8013d8d0 != 0)
            {
                if (_gameEngine.StaticVariables.INT_8013d8d0 == 1)
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
            _gameEngine.StaticVariables.INT_8013d8d0 = 0;

            var arg1 = _gameEngine.EtcRes.GetEtcString(0x43);
            var arg2 = _gameEngine.EtcRes.GetEtcString(0x44);
            var res = _gameEngine.InitializeAsyncOperation(arg1, arg2, ref _gameEngine.StaticVariables.INT_8013d8d0);

            if (res == 0)
            {
                return 0;
            }

            eventProgramState.Exp[1] = variables[0];
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
        var iVar1 = 1;

        if (logicEntity.HitCounter == 0 && logicEntity.ForceAdjusted == 0)
        {
            iVar1 = 0;
        }

        return iVar1;
    }

    // 8003E9B0
    private int Script_72_048(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var iVar1 = 1;

        if (logicEntity.HitCounter == 0 && logicEntity.CollidedWithEntityZ == 0)
        {
            iVar1 = 0;
        }

        return iVar1;
    }

    // 8003E9DC
    private int Script_73_049(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        return -eventProgramState.CodeIndex; //eventProgramState.Sp - eventProgramState.Exp[0];
    }

    // 8003E9EC
    private int Script_74_04A(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var result = 1;

        if (eventProgramState.Result != 0)
        {
            return -eventProgramState.CodeIndex; //eventProgramState.Sp - eventProgramState.Exp[0];
        }

        return result;
    }

    // 8003EA14
    private int Script_75_04B(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var result = 1;

        if (eventProgramState.Result == 0)
        {
            return -eventProgramState.CodeIndex; //eventProgramState.Sp - eventProgramState.Exp[0];
        }

        return result;
    }

    // 8003EA3C
    private int Script_76_04C(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        _gameEngine.SetTextFlags(variables[1]);
        return 2;*/
    }

    // 8003EA68
    private int Script_77_04D(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        _gameEngine.FUN_80045088();
        return 1;*/
    }

    // 8003EA88
    private int Script_78_04E(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        _gameEngine.SetDebugFlag(variables[1]);
        return 2;*/
    }

    // 8003EAB4
    private int Script_ActivateDebugTextAutoAdvance(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        _gameEngine.ActivateDebugTextAutoAdvance();
        return 1;*/
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
        return 0;
        /*
        WarpData warpData;
        warpData = _gameEngine.GetWarpData();

        if (warpData == null)
        {
            //_gameEngine.DoNothing();
            eventProgramState.Result = 0;
        }
        else
        {
            _gameEngine.HandleWarpTransition(warpData, _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId, _gameEngine.StaticVariables.PlayerEntity.TargetDirection);
            eventProgramState.Result = 1;
        }

        return 1;*/
    }

    // 8003EB88
    private int Script_83_053(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_warpType = variables[6];
        _gameEngine.StaticVariables.g_desiredMap = variables[1];
        _gameEngine.StaticVariables.g_warpEntryBehavior = variables[7];

        var y = variables[0];
        var x = ((y + 3) * 0x18 + 0xc) * 0x10000;
        var z = (y + 5) * 0x100000;
        y = ((y + 4) * 0x10 + 8) * 0x10000;

        if (_gameEngine.StaticVariables.g_warpType == 3)
        {
            if (_gameEngine.StaticVariables.g_desiredMap == _gameEngine.StaticVariables.g_currentMap)
            {
                _gameEngine.StaticVariables.PlayerEntity.PosX = x;
                _gameEngine.StaticVariables.PlayerEntity.PosY = y;
                _gameEngine.StaticVariables.PlayerEntity.PosZ = z + 1;
                return 8;
            }

            //_gameEngine.DoNothing();

            _gameEngine.StaticVariables.g_warpType = 0;
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
    private int Script_UpdateCameraToEntityAndCheckCondition(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int matchCount;
        Entity matchedEntity;

        matchCount = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);
        matchedEntity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0];

        if (matchCount != 0)
        {
            if ((_gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].Flags & 0x800000U) != 0)
            {
                matchCount = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].SpriteRecord.Header.FramesPointer;
                _gameEngine.StartHudTransition(_gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].PosX, _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].PosY, _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].PosZ, -0x7ff1bcd8, -0x7ff1bcd4, (u_char*)(matchCount + 4), (u_char*)(matchCount + 5), (ushort)(matchCount + 6), (ushort)(matchCount + 7), (_gameEngine.StaticVariables.g_drawPageInfoBase)_gameEngine.StaticVariables.[g_matchingEntitiesBuffer[0].SheetSize + ((matchCount + 3) & 0x3f)], (_gameEngine.StaticVariables.g_tPageFadeLUT)_gameEngine.StaticVariables.[g_matchingEntitiesBuffer[0].PaletteIndex + ((matchCount + 2) & 7)]);
            }

            _gameEngine.TriggerVisualUpdate(matchedEntity.SpriteTableIndex);
        }

        matchCount = _gameEngine.TryPlayEtcAnimation(variables[2], variables[3]);

        return (matchCount != 0) << 2;*/
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

        bVar1 = (byte)variables[3];
        bVar2 = (byte)variables[2];
        num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (int i = 0; i < num; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];
            entity.ForceZ = (int)((bVar2 + (uint)bVar1 * 0x100) * 0x10000) >> 8;
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

        if (eventProgramState.Exp[1] != variables[0])
        {
            eventProgramState.Exp[1] = variables[0];
            eventProgramState.Exp[2] = logicEntity.PosX;
            eventProgramState.Exp[3] = logicEntity.PosY;
            command = logicEntity.PosZ;
            eventProgramState.Exp[5] = 0;
            eventProgramState.Exp[6] = 0;
            eventProgramState.Exp[4] = command;
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

        eventProgramState.Exp[5] = eventProgramState.Exp[5] + 1;
        logicEntity.AnimCompleteCounter = 0;

        UPDATE_FRAME_COUNTER:
        frameTimer = eventProgramState.Exp[6] + 1;
        eventProgramState.Exp[6] = frameTimer;
        command = variables[0];

        if (eventProgramState.Exp[5] < (int)(uint)(command + 4))
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
            command = eventProgramState.Exp[2] - logicEntity.PosX;
            if (command < 0)
            {
                command = -command;
            }

            if (command < (int)((uint)(frameTimer + 1) * 0x180000))
            {
                return 0;
            }

            command = eventProgramState.Exp[3] - logicEntity.PosY;
            if (command < 0)
            {
                command = -command;
            }

            if ((int)((uint)(frameTimer + 2) << 0x14) <= command)
            {
                command = eventProgramState.Exp[4] - logicEntity.PosZ;
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
        ushort flag;
        int num;
        
        flag = (ushort)((variables[3] << 8) | variables[2]);
        num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        for (int i = 0; i < num; i++)
        {
            var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[i];
            entity.Flags &= ~(uint)flag | 0xffff0000;
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
        eventProgramState.Exp[2] = logicEntity.PosX;
        eventProgramState.Exp[3] = logicEntity.PosY;
        eventProgramState.Exp[4] = logicEntity.PosZ;
        return 1;
    }

    // 8003F908
    private int Script_107_06B(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var iVar1 = eventProgramState.Exp[2] - logicEntity.PosX;

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
        var iVar1 = eventProgramState.Exp[3] - logicEntity.PosY;

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
        var iVar1 = eventProgramState.Exp[4] - logicEntity.PosZ;

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
        var iVar1 = eventProgramState._30 + -1;
        eventProgramState._30 = iVar1;

        if (iVar1 < 1)
        {
            iVar1 = 3;
        }
        else
        {
            iVar1 = (variables[1] + variables[2] * 0x100) * 0x10000 >> 0x10;
        }

        return iVar1;
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
    private int Script_120_078(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        eventProgramState[1].Sp = variables[3];
        return (variables[1] + variables[2] * 0x100) * 0x10000 >> 0x10;*/
    }

    // 8003FB44
    private int Script_121_079(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int iVar1;
        iVar1 = 3;

        if (eventProgramState.Result != 0)
        {
            eventProgramState[1].Sp = variables[3];
            iVar1 = (variables[1] + variables[2] * 0x100) * 0x10000 >> 0x10;
        }

        return iVar1;*/
    }

    // 8003FB8C
    private int Script_122_07A(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int iVar1;
        iVar1 = 3;

        if (eventProgramState.Result == 0)
        {
            eventProgramState[1].Sp = variables[3];
            iVar1 = (variables[1] + variables[2] * 0x100) * 0x10000 >> 0x10;
        }

        return iVar1;*/
    }

    // 8003FBD4
    private int Script_123_07B(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        uint bitfieldBase;
        int iVar1;
        uint uVar2;

        uVar2 = variables[1] + variables[2] * 0x100;

        if ((uVar2 & 0x8000) == 0)
        {
            bitfieldBase = (uint)_gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            bitfieldBase = (uint)_gameEngine.StaticVariables.g_globalFlags;
        }

        iVar1 = 5;

        if (((uint)((uVar2 >> 3 & 0xffc) + (int)bitfieldBase) & 1 << (variables[1] & 0x1f)) != 0)
        {
            eventProgramState[1].Sp = variables[5];
            iVar1 = (variables[3] + variables[4] * 0x100) * 0x10000 >> 0x10;
        }

        return iVar1;*/
    }

    // 8003FC74
    private int Script_124_07C(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        uint[] piVar1;
        int iVar2;
        uint uVar3;

        uVar3 = (uint)(variables[1] + variables[2] * 0x100);

        if ((uVar3 & 0x8000) == 0)
        {
            piVar1 = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            piVar1 = _gameEngine.StaticVariables.g_globalFlags;
        }

        iVar2 = 5;

        if (((uint)((uVar3 >> 3 & 0xffc) + piVar1) & 1 << (variables[1] & 0x1f)) == 0)
        {
            eventProgramState[1].Sp = variables[5];
            iVar2 = (variables[3] + variables[4] * 0x100) * 0x10000 >> 0x10;
        }

        return iVar2;*/
    }

    // 8003FD14
    private int Script_125_07D(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        return eventProgramState[1].Sp - variables;*/
    }

    // 8003FD24
    private int Script_126_07E(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int iVar1;
        iVar1 = 1;

        if (eventProgramState.Result != 0)
        {
            iVar1 = eventProgramState[1].Sp - variables;
        }

        return iVar1;*/
    }

    // 8003FD4C
    private int Script_127_07F(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int iVar1;
        iVar1 = 1;

        if (eventProgramState.Result == 0)
        {
            iVar1 = eventProgramState[1].Sp - variables;
        }

        return iVar1;*/
    }

    // 8003FD74
    private int Script_128_080(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        uint[] piVar1;
        int iVar2;
        uint uVar3;

        uVar3 = variables[1] + variables[2] * 0x100;

        if ((uVar3 & 0x8000) == 0)
        {
            piVar1 = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            piVar1 = _gameEngine.StaticVariables.g_globalFlags;
        }

        iVar2 = 3;

        if (((uint)((uVar3 >> 3 & 0xffc) + piVar1) &

            1 << (variables[1] & 0x1f)) != 0)
        {
            iVar2 = eventProgramState[1].Sp - variables;
        }

        return iVar2;*/
    }

    // 8003FDF8
    private int Script_129_081(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        uint[] piVar1;

        int iVar2;

        uint uVar3;

        uVar3 = variables[1] + variables[2] * 0x100;

        if ((uVar3 & 0x8000) == 0)
        {
            piVar1 = _gameEngine.StaticVariables.g_mapFlags;
        }
        else
        {
            piVar1 = _gameEngine.StaticVariables.g_globalFlags;
        }

        iVar2 = 3;

        if (((uint)((uVar3 >> 3 & 0xffc) + piVar1) &

            1 << (variables[1] & 0x1f)) == 0)
        {
            iVar2 = eventProgramState[1].Sp - variables;
        }

        return iVar2;*/
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
        Debugger.Break();
        return 0;
        /*
        ushort uVar1;
        int iVar2;
        int iVar3;
        int piVar4;

        uVar1 = variables[2];
        iVar2 = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (0 < iVar2)
        {
            piVar4 = _gameEngine.StaticVariables.g_activeEntityRefId + iVar2;

            do
            {
                iVar3 = piVar4;
                piVar4 = piVar4 + -1;
                iVar2 = iVar2 + -1;
                (uint)(iVar3 + 0x1cc) = uVar1;
            } while (0 < iVar2);
        }

        return 4;*/
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
        var pEVar1 = _gameEngine.SpawnEntity(logicEntity, variables[1], 1);

        if (pEVar1 == null)
        {
            //_gameEngine.PrintCommandMap();
        }

        pEVar1.PosX = (variables[2] + variables[3] * 0x100) * 0x10000;
        pEVar1.PosY = (variables[4] + variables[5] * 0x100) * 0x10000;
        pEVar1.PosZ = (variables[6] + variables[7] * 0x100) * 0x10000 + 1;

        return 8;
    }

    // 8004033C
    private int Script_139_08B(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var iVar2 = _gameEngine.SpawnEntity(logicEntity, variables[2], 1);

        if (iVar2 == null)
        {
            //_gameEngine.PrintCommandMap();
        }

        var iVar3 = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        var pEVar1 = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0];

        if (iVar3 != 0)
        {
            iVar2.PosX = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0].PosX + (variables[3] + variables[4] * 0x100) * 0x10000;
            iVar2.PosY = pEVar1.PosY + (variables[5] + variables[6] * 0x100) * 0x10000;
            iVar2.PosZ = pEVar1.PosZ + (variables[7] + variables[8] * 0x100) * 0x10000;
        }

        return 9;
    }

    // 80040438
    private int Script_140_08C(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_gameRandomSeed = _gameEngine.StaticVariables.g_gameRandomSeed * 0x7d2b89dd + 0xe06a02e7;

        if (_gameEngine.StaticVariables.g_gameRandomSeed * 0x100 >> 0x20 < variables[1])
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
        Debugger.Break();
        return 0;
        /*
        var num = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[1]);

        if (0 < num)
        {
            var piVar2 = _gameEngine.StaticVariables.g_activeEntityRefId + num;

            do
            {
                if (piVar2 + 0x11c <= piVar2 + 0x138 + 1)
                {
                    eventProgramState.Result = 1;
                    return 2;
                }

                num += -1;
                piVar2 += -1;
            } while (0 < num);
        }

        eventProgramState.Result = 0;

        return 2;
        */
    }

    // 80040534
    private int Script_142_08E(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_bossCutsceneFlag = 1;
        _gameEngine.StaticVariables.g_cutsceneScrollSpeedX = variables[1];
        _gameEngine.StaticVariables.g_cutsceneScrollSpeedY = variables[2];
        _gameEngine.StaticVariables.g_cutsceneScrollLimitX = variables[3];
        _gameEngine.StaticVariables.g_cutsceneScrollLimitY = variables[4];

        return 5;
    }

    // 80040598
    private int Script_143_08F(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.StaticVariables.g_bossCutsceneFlag = 0;
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
        var x = (variables[2] | variables[3] << 8) << 16;
        var y = (variables[4] | variables[5] << 8) << 16;
        var z = (variables[6] | variables[7] << 8) << 16;

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
        var x = (variables[2] | variables[3] << 8) << 16;
        var y = (variables[4] | variables[5] << 8) << 16;
        var z = (variables[6] | variables[7] << 8) << 16;
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
    private int Script_149_095(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        byte bVar1;

        int iVar2;

        int iVar3;

        uint puVar4;

        int iVar5;

        int iVar6;

        iVar5 = variables;

        iVar3 = (iVar5 + 4) * 0x180000;

        if (iVar5 + 7 != 0)
        {
            iVar6 = 0;

            if (-1 < _gameEngine.StaticVariables.g_numberOfEntity)
            {
                puVar4 = _gameEngine.StaticVariables.PlayerEntity.TransformHeight;

                do
                {
                    if (puVar4[-0x83] - 2 < 2 && puVar4[-0x7f] == 0 &&

                        (FrameCollisionData*)puVar4[-0x12] != (FrameCollisionData*)0x0 &&

                        ((BalanceAnimValRef*)puVar4[-0x15] != (BalanceAnimValRef*)0x0 &&

                         (bVar1 = ((BalanceAnimValRef*)puVar4[-0x15]).val, bVar1 != 0)) &&

                        (puVar4[-0x6c] & (iVar5 + 7)) != 0 &&

                        (bVar1 & 0xf) == BYTE_ARRAY_80098fa4[iVar5 + 9])
                    {
                        iVar2 = puVar4[-8] + (iVar5 + 1) * -0x180000;

                        if (iVar2 < 0)
                        {
                            if ((iVar5 + 1) * 0x180000 - puVar4[-8] < puVar4[-2] + 1)

                                goto LAB_80040908;
                        }
                        else if (iVar2 < iVar3)
                        {
                            LAB_80040908:

                            iVar2 = puVar4[-7] + (iVar5 + 2) * -0x100000;

                            if (iVar2 < 0)
                            {
                                if ((iVar5 + 2) * 0x100000 - puVar4[-7] < puVar4[-1] + 1)

                                    goto LAB_80040948;
                            }
                            else if (iVar2 < (iVar5 + 5) * 0x100000)
                            {
                                LAB_80040948:

                                iVar2 = puVar4[-6] + (iVar5 + 3) * -0x100000;

                                if (iVar2 < 0)
                                {
                                    if ((iVar5 + 3) * 0x100000 - puVar4[-6] < (int)(puVar4 + 1))

                                    {
                                        eventProgramState.Result = 1;

                                        return 10;
                                    }
                                }
                                else if (iVar2 < iVar3)
                                {
                                    eventProgramState.Result = 1;

                                    return 10;
                                }
                            }
                        }
                    }

                    iVar6 = iVar6 + 1;

                    puVar4 = puVar4 + 0xa5;
                } while (iVar6 <= _gameEngine.StaticVariables.g_numberOfEntity);
            }
        }

        eventProgramState.Result = 0;

        return 10;*/
    }

    // 800409A8
    private int Script_150_096(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        byte bVar1;

        int iVar2;

        int piVar3;

        int iVar4;

        int iVar5;

        bVar1 = variables[1];

        iVar2 = _gameEngine.GetNumberOfEntityByRefId(logicEntity, (uint)bVar1);

        if (0 < iVar2)
        {
            piVar3 = _gameEngine.StaticVariables.g_activeEntityRefId + iVar2;

            do
            {
                iVar5 = piVar3;

                iVar4 = iVar5 + 0x14 + (uint)bVar1;

                if (iVar5 + 0x18 < iVar4)
                {
                    iVar4 = iVar5 + 0x18;
                }

                (iVar5 + 0x14) = iVar4;

                iVar2 = iVar2 + -1;

                piVar3 = piVar3 + -1;
            } while (0 < iVar2);
        }

        return 3;*/
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
        var x = (variables[2] | variables[3] << 8) << 16;
        var y = (variables[4] | variables[5] << 8) << 16;
        var z = (variables[6] | variables[7] << 8) << 16;

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

        var x = (variables[3] | variables[4] << 8) << 16;
        var y = (variables[5] | variables[6] << 8) << 16;
        var z = (variables[7] | variables[8] << 8) << 16;

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
            spriteEffect.X = (variables[2] + variables[3] << 8) << 16;
            spriteEffect.Y = (variables[4] + variables[5] << 8) << 16;
            spriteEffect.Z = ((variables[6] + variables[7] << 8) << 16) + 1;
        }

        return 8;
    }

    // 80040FAC
    private int Script_163_0A3(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var iVar2 = _gameEngine.GetNumberOfEntityByRefId(logicEntity, variables[2]);
        var entity = _gameEngine.StaticVariables.g_matchingEntitiesBuffer[0];

        if (iVar2 != 0)
        {
            var spriteEffect = _gameEngine.EffectManager.SpawnSpriteEffect(variables[1], 1);

            if (spriteEffect != null)
            {
                spriteEffect.X = entity.PosX + (variables[3] + variables[4] << 8) << 16;
                spriteEffect.Y = entity.PosY + (variables[5] + variables[6] << 8) << 16;
                spriteEffect.Z = entity.PosZ + (variables[7] + variables[8] << 8) << 16;
            }
        }

        return 9;
    }

    // 80041098
    private int Script_164_0A4(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        _gameEngine.SetTileAnimationMode(variables[1], variables[2]);
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
    private int Script_187_0BB(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        Debugger.Break();
        return 0;
        /*
        int iVar1;
        int iVar2;

        if (_gameEngine.StaticVariables.g_currentMap == 0x1dd)
        {
            iVar1 = 1;

            if ((_gameEngine.StaticVariables.g_globalFlags[0] & 2U) == 0)
            {
                if (eventProgramState.Exp[1] == variables[0])
                {
                    iVar1 = 0xff;

                    if (PTR_g_someDataIntoRam_80029bc4[0x756] != 0xff)
                    {
                        iVar1 = _gameEngine.StaticVariables.PTR_g_someDataIntoRam_80029bc4[0x756] + 1;
                    }

                    if (_gameEngine.StaticVariables.g_debugState < 0)
                    {
                        //Debug.WriteLine(_gameEngine.StaticVariables.g_debugMessage + iVar2, "Retry = %d", iVar1);
                    }

                    iVar1 = eventProgramState.Exp[2];

                    eventProgramState.Exp[2] = iVar1 + 1;

                    if (iVar1 < 0x3c)
                    {
                        iVar1 = 0;
                    }
                    else
                    {
                        iVar1 = 1;
                        _gameEngine.StaticVariables.g_isGameEnding = 1;
                        _gameEngine.StaticVariables.g_warpType = 10;
                        _gameEngine.StaticVariables.g_warpEntryBehavior = 0;
                        _gameEngine.StaticVariables.g_desiredMap = 0xb;
                    }
                }
                else
                {
                    _gameEngine.FUN_80033a2c(_gameEngine.StaticVariables.g_entitySlots);
                    _gameEngine.SoundManager.PlaySoundEffect(0x31);
                    iVar2 = variables;
                    iVar1 = 0;
                    eventProgramState.Exp[2] = 0;
                    eventProgramState.Exp[1] = iVar2;
                }
            }
            else
            {
                _gameEngine.StaticVariables.g_isGameEnding = 1;
                _gameEngine.StaticVariables.g_warpEntryBehavior = 0;
                _gameEngine.StaticVariables.g_warpType = 0xb;
            }
        }
        else
        {
            iVar1 = 1;
            _gameEngine.StaticVariables.g_isGameEnding = 1;
            _gameEngine.StaticVariables.g_warpType = 9;
            _gameEngine.StaticVariables.g_warpEntryBehavior = 0;
        }

        return iVar1;*/
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
        var sfxId = variables[1] | (variables[2] << 8);
        _gameEngine.SoundManager.PlaySoundEffect((uint)sfxId);
        return 3;
    }

    // 80041C6C
    private int Script_190_0BE(Entity logicEntity, Entity ownerEntity, int[] variables, EventProgramState eventProgramState)
    {
        var sfxId = variables[1] | (variables[2] << 8);
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
        var slotId = variables[1] | (variables[2] << 8);

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
        var targetLevel = _gameEngine.PlayerManager.GetPlayerMpMax();
        _gameEngine.PlayerManager.SetPlayerMp((short)targetLevel);
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

                //var textureIndex = (eventProgramState.Exp[2] & 0x07) + targetEntity.PaletteOffset;
                //var textureId = _gameEngine.StaticVariables.g_tPageFadeLUT[textureIndex];

                //var spriteSheetOffset = targetEntity.SpriteSheetOffset + (eventProgramState.Exp[3] & 0x3F);
                //var textureId2 = _gameEngine.StaticVariables.g_drawPageInfoBase[spriteSheetOffset];

                var image = targetEntity.Frame.Images.Images[targetEntity.CurrentFrameIndex];

                _gameEngine.MainInventoryManager.StartHudTransition(
                    targetEntity.PosX, 
                    targetEntity.PosY,
                    targetEntity.PosZ, 
                    -_gameEngine.StaticVariables.g_cameraScrollingX, 
                    -_gameEngine.StaticVariables.g_cameraScrollingY,
                    (byte)eventProgramState.Exp[4],
                    (byte)eventProgramState.Exp[5],
                    (short)eventProgramState.Exp[6],
                    (short)eventProgramState.Exp[7],
                    image);
            }

            int spriteUpdateId = variables[2] | (variables[3] << 8);
            _gameEngine.TriggerVisualUpdate(spriteUpdateId);
        }

        if (_gameEngine.TryPlayEtcAnimation((uint)variables[4], variables[5]) == 0)
        {
            return 0;
        }

        return 6;
    }
}