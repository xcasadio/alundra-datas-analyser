using AlundraEngine.DatasBin;

namespace AlundraEngine.Gameplay.Scripts;

public class EventCodeDebugger
{
    public class SiCode
    {
        public byte Code { get; init; }
        public string Name { get; init; }
        public int Size { get; init; }
    }

    public static SiCode GetCode(byte code)
    {
        if (CommandPropertiesByCode.TryGetValue(code, out var commandInfo))
        {
            return new SiCode
            {
                Code = code,
                Size = commandInfo.Size,
                Name = commandInfo?.Description ?? "?"
            };
        }

        //AlundraEngine.Debug.Debugger.Breakpoint();

        return new SiCode
        {
            Code = code,
            Size = 0,
            Name = ""
        };
    }

    public static List<SiCommand> GetCommandsOnlyAtOffset(byte[] codes, int eventCodesOffset)
    {
        var commands = new List<SiCommand>();
        var i = eventCodesOffset;

        while (i < codes.Length)
        {
            var offset = i;
            var value = codes[i++];
            var siCode = GetCode(value);

            if (siCode.Size < 1)
            {
                continue;
            }

            var size = siCode.Size;
            var name = siCode.Name;
            byte[] parameters;

            if (siCode.Code == 0)
            {
                parameters = Array.Empty<byte>();
            }
            else
            {
                parameters = new byte[size - 1];
                var j = 0;

                while (j < size - 1)
                {
                    parameters[j++] = codes[i++];
                }
            }
            var cmd = new SiCommand(value, parameters, name, offset);
            commands.Add(cmd);

            if (value == 0xff)
            {
                break;
            }
        }

        return commands;
    }

    public static List<SiCommand> GetCommands(byte[] codes, int startOffset)
    {
        var commands = new List<SiCommand>();
        var i = 0; //startOffset;

        while (i < codes.Length)
        {
            var offset = i;
            var value = codes[i++];
            var siCode = GetCode(value);

            if (siCode.Size < 1)
            {
                continue;
            }

            var size = siCode.Size;
            var name = siCode.Name;
            byte[] parameters = null;

            if (siCode.Code == 0) //break
            {
                parameters = Array.Empty<byte>();
            }
            else
            {
                parameters = new byte[size - 1];
                var j = 0;

                while (j < size - 1)
                {
                    parameters[j++] = codes[i++];
                }
            }

            var cmd = new SiCommand(value, parameters, name, offset);
            commands.Add(cmd);
        }

        return commands;
    }

    public record CommandInfo(byte Code, int Size, string Description, string HandlerName);

    public static readonly Dictionary<byte, CommandInfo> CommandPropertiesByCode = new()
    {
        {0x00, new(0x00, 1, "Break", nameof(EntityEventHandlers.Script_DoNothing)) },
        {0x01, new(0x01, 1, "Do nothing (debug command)", nameof(EntityEventHandlers.Script_DoNothing)) },
        {0x02, new(0x02, 3, "Goto", nameof(EntityEventHandlers.Script_2_002)) },
        {0x03, new(0x03, 3, "If true goto", nameof(EntityEventHandlers.Script_3_003)) },
        {0x04, new(0x04, 3, "If false goto", nameof(EntityEventHandlers.Script_4_004)) },
        {0x05, new(0x05, 3, "Flag on", nameof(EntityEventHandlers.Script_5_005)) },
        {0x06, new(0x06, 3, "Flag off", nameof(EntityEventHandlers.Script_6_006)) },
        {0x07, new(0x07, 8, "Check entity in area", nameof(EntityEventHandlers.Script_7_007)) },
        {0x08, new(0x08, 2, "Turn", nameof(EntityEventHandlers.Script_8_008)) },
        {0x09, new(0x09, 2, "Set direction", nameof(EntityEventHandlers.Script_9_009)) },
        {0x0A, new(0x0A, 1, "Reverse direction", nameof(EntityEventHandlers.Script_10_00A)) },
        {0x0B, new(0x0B, 4, "Wait until entity moves beyond radius", nameof(EntityEventHandlers.Script_WaitUntilEntityMovesBeyondRadius_00B)) },
        {0x0C, new(0x0C, 1, "Set random dir", nameof(EntityEventHandlers.Script_12_00C)) },
        {0x0D, new(0x0D, 3, "Dialog", nameof(EntityEventHandlers.Script_OpenDialog_13_00D)) },
        {0x0E, new(0x0E, 0, "Do nothing (debug command)", nameof(EntityEventHandlers.Script_DoNothing)) },
        {0x0F, new(0x0F, 0, "Do nothing (debug command)", nameof(EntityEventHandlers.Script_DoNothing)) },
        {0x10, new(0x10, 1, "Player lose control", nameof(EntityEventHandlers.Script_16_010)) },
        {0x11, new(0x11, 1, "Player gain control", nameof(EntityEventHandlers.Script_17_011)) },
        {0x12, new(0x12, 2, "Play sound 1", nameof(EntityEventHandlers.Script_18_012)) },
        {0x13, new(0x13, 0, "Do nothing (debug command)", nameof(EntityEventHandlers.Script_DoNothing)) },
        {0x14, new(0x14, 0, "Do nothing (debug command)", nameof(EntityEventHandlers.Script_DoNothing2)) },
        {0x15, new(0x15, 1, "Reset z pos", nameof(EntityEventHandlers.Script_21_015)) },
        {0x16, new(0x16, 1, "High gravity", nameof(EntityEventHandlers.Script_22_016)) },
        {0x17, new(0x17, 1, "Low gravity", nameof(EntityEventHandlers.Script_23_017)) },
        {0x18, new(0x18, 0, "Do nothing", nameof(EntityEventHandlers.Script_DoNothing)) },
        {0x19, new(0x19, 1, "Deactivate entity", nameof(EntityEventHandlers.Script_25_019)) },
        {0x1A, new(0x1A, 2, "Set anim", nameof(EntityEventHandlers.Script_26_01A)) },
        {0x1B, new(0x1B, 3, "Fly", nameof(EntityEventHandlers.Script_27_01B)) },
        {0x1C, new(0x1C, 2, "Repeat anim", nameof(EntityEventHandlers.Script_28_01C)) },
        {0x1D, new(0x1D, 2, "Repeat anim with collision", nameof(EntityEventHandlers.Script_29_01D)) },
        {0x1E, new(0x1E, 3, "Walk", nameof(EntityEventHandlers.Script_30_01E)) },
        {0x1F, new(0x1F, 3, "Walk with collision", nameof(EntityEventHandlers.Script_31_01F)) },
        {0x20, new(0x20, 3, "Check Z distance and collided Z", nameof(EntityEventHandlers.Script_32_020)) },
        {0x21, new(0x21, 3, "Is within Z distance", nameof(EntityEventHandlers.Script_33_021)) },
        {0x22, new(0x22, 1, "Clamp forceZ to height target", nameof(EntityEventHandlers.Script_34_022)) },
        {0x23, new(0x23, 1, "Clamp forceZ and no collided Z", nameof(EntityEventHandlers.Script_35_023)) },
        {0x24, new(0x24, 1, "Wait force adjusted", nameof(EntityEventHandlers.Script_36_024)) },
        {0x25, new(0x25, 1, "Wait entity collision z or 144", nameof(EntityEventHandlers.Script_37_025)) },
        {0x26, new(0x26, 1, "Wait force adjusted or entity collision z", nameof(EntityEventHandlers.Script_38_026)) },
        {0x27, new(0x27, 1, "Face player", nameof(EntityEventHandlers.Script_39_027)) },
        {0x28, new(0x28, 1, "Gravity flag 2 on", nameof(EntityEventHandlers.Script_40_028)) },
        {0x29, new(0x29, 1, "Gravity flag 2 off", nameof(EntityEventHandlers.Script_41_029)) },
        {0x2A, new(0x2A, 1, "Gravity flag 3 on", nameof(EntityEventHandlers.Script_42_02A)) },
        {0x2B, new(0x2B, 1, "Gravity flag 3 off", nameof(EntityEventHandlers.Script_43_02B)) },
        {0x2C, new(0x2C, 2, "Check no entity by function id", nameof(EntityEventHandlers.Script_44_02C)) },
        {0x2D, new(0x2D, 2, "Activate entity", nameof(EntityEventHandlers.Script_45_02D)) },
        {0x2E, new(0x2E, 2, "Hide", nameof(EntityEventHandlers.Script_46_02E)) },
        {0x2F, new(0x2F, 4, "Check moving in dir", nameof(EntityEventHandlers.Script_47_02F)) },
        {0x30, new(0x30, 5, "If flag on", nameof(EntityEventHandlers.Script_48_030)) },
        {0x31, new(0x31, 5, "If flag off", nameof(EntityEventHandlers.Script_49_031)) },
        {0x32, new(0x32, 3, "Toggle flag", nameof(EntityEventHandlers.Script_50_032)) },
        {0x33, new(0x33, 9, "Check flags on", nameof(EntityEventHandlers.Script_51_033)) },
        {0x34, new(0x34, 9, "Check flags off", nameof(EntityEventHandlers.Script_52_034)) },
        {0x35, new(0x35, 3, "Until flag off", nameof(EntityEventHandlers.Script_53_035)) },
        {0x36, new(0x36, 3, "Until flag on", nameof(EntityEventHandlers.Script_54_036)) },
        {0x37, new(0x37, 2, "Wait", nameof(EntityEventHandlers.Script_55_037)) },
        {0x38, new(0x38, 5, "Set MapIdToInternalMapIndex", nameof(EntityEventHandlers.Script_SetSaveMapIdToInternalMapIndex_038)) },
        {0x39, new(0x39, 1, "Wait for dialog", nameof(EntityEventHandlers.Script_IsDialogInProgress_039)) },
        {0x3A, new(0x3A, 2, "Set TargetDirection", nameof(EntityEventHandlers.Script_SetTargetDirection_03A)) },
        {0x3B, new(0x3B, 7, "Check player in area", nameof(EntityEventHandlers.Script_59_03B)) },
        {0x3C, new(0x3C, 7, "If entity in zone with flag 0x80", nameof(EntityEventHandlers.Script_60_03C)) },
        {0x3D, new(0x3D, 7, "If entity in zone", nameof(EntityEventHandlers.Script_61_03D)) },
        {0x3E, new(0x3E, 1, "Is player riding entity", nameof(EntityEventHandlers.Script_62_03E)) },
        {0x3F, new(0x3F, 1, "If entity riding me", nameof(EntityEventHandlers.Script_63_03F)) },
        {0x40, new(0x40, 3, "Set program index", nameof(EntityEventHandlers.Script_64_040)) },
        {0x41, new(0x41, 3, "Set sprite program index", nameof(EntityEventHandlers.Script_65_041)) },
        {0x42, new(0x42, 1, "LogicContextEntity = PlayerEntity", nameof(EntityEventHandlers.Script_66_042)) },
        {0x43, new(0x43, 2, "LogicContextEntity by function id", nameof(EntityEventHandlers.Script_67_043)) },
        {0x44, new(0x44, 1, "Wait dialog choice", nameof(EntityEventHandlers.Script_68_044)) },
        {0x45, new(0x45, 1, "Gravity flag 4 off", nameof(EntityEventHandlers.Script_69_045)) },
        {0x46, new(0x46, 1, "Gravity flag 4 on", nameof(EntityEventHandlers.Script_70_046)) },
        {0x47, new(0x47, 1, "If collided Z or ForceAdjusted", nameof(EntityEventHandlers.Script_71_047)) },
        {0x48, new(0x48, 1, "If collided Z or hit", nameof(EntityEventHandlers.Script_72_048)) },
        {0x49, new(0x49, 1, "Restart", nameof(EntityEventHandlers.Script_73_049)) },
        {0x4A, new(0x4A, 1, "If true restart", nameof(EntityEventHandlers.Script_74_04A)) },
        {0x4B, new(0x4B, 1, "If false restart", nameof(EntityEventHandlers.Script_75_04B)) },
        {0x4C, new(0x4C, 2, "Set dialog something", nameof(EntityEventHandlers.Script_76_04C)) },
        {0x4D, new(0x4D, 1, "Check dialog something", nameof(EntityEventHandlers.Script_77_04D)) },
        {0x4E, new(0x4E, 2, "Set debug flag", nameof(EntityEventHandlers.Script_78_04E)) },
        {0x4F, new(0x4F, 1, "Activate debug text auto advance", nameof(EntityEventHandlers.Script_ActivateDebugTextAutoAdvance)) },
        {0x50, new(0x50, 2, "Set dialog choice", nameof(EntityEventHandlers.Script_SetEtcAnimationMode)) },
        {0x51, new(0x51, 1, "Get dialog choice", nameof(EntityEventHandlers.Script_TryActivateTextHoldState)) },
        {0x52, new(0x52, 1, "Use portal", nameof(EntityEventHandlers.Script_82_052)) },
        {0x53, new(0x53, 8, "Change map", nameof(EntityEventHandlers.Script_ChangeMap_053)) },
        {0x54, new(0x54, 5, "Set walkable", nameof(EntityEventHandlers.Script_84_054)) },
        {0x55, new(0x55, 5, "Set unwalkable", nameof(EntityEventHandlers.Script_85_055)) },
        {0x56, new(0x56, 2, "Change area tiles", nameof(EntityEventHandlers.Script_86_056)) },
        {0x57, new(0x57, 9, "Goto by animation direction", nameof(EntityEventHandlers.Script_87_057)) },
        {0x58, new(0x58, 9, "Directional branch", nameof(EntityEventHandlers.Script_88_058)) },
        {0x59, new(0x59, 3, "Set entity anim", nameof(EntityEventHandlers.Script_89_059)) },
        {0x5A, new(0x5A, 3, "Turn entity", nameof(EntityEventHandlers.Script_90_05A)) },
        {0x5B, new(0x5B, 4, "Turn entity with anim", nameof(EntityEventHandlers.Script_91_05B)) },
        {0x5C, new(0x5C, 4, "Dialog with entity", nameof(EntityEventHandlers.Script_OpenDialogWithChoice_05C)) },
        {0x5D, new(0x5D, 2, "Set entity Status = 3", nameof(EntityEventHandlers.Script_93_05D)) },
        {0x5E, new(0x5E, 4, "Set forceZ for entities", nameof(EntityEventHandlers.Script_94_05E)) },
        {0x5F, new(0x5F, 1, "Wait for anim or distance", nameof(EntityEventHandlers.Script_WaitForAnimOrDistance)) },
        {0x60, new(0x60, 2, "Is riding entity", nameof(EntityEventHandlers.Script_96_060)) },
        {0x61, new(0x61, 2, "Is riding entity 2", nameof(EntityEventHandlers.Script_97_061)) },
        {0x62, new(0x62, 4, "Set entities flags", nameof(EntityEventHandlers.Script_98_062)) },
        {0x63, new(0x63, 4, "Set entities gravity", nameof(EntityEventHandlers.Script_99_063)) },
        {0x64, new(0x64, 8, "Set entities position", nameof(EntityEventHandlers.Script_100_064)) },
        {0x65, new(0x65, 8, "Move entity position", nameof(EntityEventHandlers.Script_101_065)) },
        {0x66, new(0x66, 3, "Copy context and assign script", nameof(EntityEventHandlers.Script_CopyLogicContextAndAssignScript)) },
        {0x67, new(0x67, 2, "Camera follow entity", nameof(EntityEventHandlers.Script_103_067)) },
        {0x68, new(0x68, 1, "Camera stop follow entity", nameof(EntityEventHandlers.Script_104_068)) },
        {0x69, new(0x69, 7, "Camera look at", nameof(EntityEventHandlers.Script_105_069)) },
        {0x6A, new(0x6A, 1, "Save entity position", nameof(EntityEventHandlers.Script_106_06A)) },
        {0x6B, new(0x6B, 3, "If entity x < threshold", nameof(EntityEventHandlers.Script_107_06B)) },
        {0x6C, new(0x6C, 3, "If entity y < threshold", nameof(EntityEventHandlers.Script_108_06C)) },
        {0x6D, new(0x6D, 3, "If entity z < threshold", nameof(EntityEventHandlers.Script_109_06D)) },
        {0x6E, new(0x6E, 1, "Is force adjusted", nameof(EntityEventHandlers.Script_110_06E)) },
        {0x6F, new(0x6F, 1, "Is collided with entity Z", nameof(EntityEventHandlers.Script_111_06F)) },
        {0x70, new(0x70, 1, "Is above ground", nameof(EntityEventHandlers.Script_112_070)) },
        {0x71, new(0x71, 1, "Get hit counter", nameof(EntityEventHandlers.Script_113_071)) },
        {0x72, new(0x72, 1, "Set LastTargetAnim and Direction", nameof(EntityEventHandlers.Script_114_072)) },
        {0x73, new(0x73, 2, "Initialize timer _30", nameof(EntityEventHandlers.Script_115_073)) },
        {0x74, new(0x74, 3, "Update timer _30", nameof(EntityEventHandlers.Script_116_074)) },
        {0x75, new(0x75, 2, "Play sound effect", nameof(EntityEventHandlers.Script_117_075)) },
        {0x76, new(0x76, 0, "not implemented", nameof(EntityEventHandlers.Script_118_076)) },
        {0x77, new(0x77, 0, "not implemented", nameof(EntityEventHandlers.Script_119_077)) },
        {0x78, new(0x78, 4, "Store choice param and jump", nameof(EntityEventHandlers.Script_StoreChoiceParamAndJump_078)) },
        {0x79, new(0x79, 3, "Jump if choice accepted", nameof(EntityEventHandlers.Script_JumpIfChoiceAccepted_079)) },
        {0x7A, new(0x7A, 3, "Jump if choice rejected", nameof(EntityEventHandlers.Script_JumpIfChoiceRejected_07A)) },
        {0x7B, new(0x7B, 5, "Jump if flag set store param", nameof(EntityEventHandlers.Script_JumpIfFlagSetStoreParam_07B)) },
        {0x7C, new(0x7C, 5, "Jump if flag clear store param", nameof(EntityEventHandlers.Script_JumpIfFlagClearStoreParam_07C)) },
        {0x7D, new(0x7D, 1, "Jump relative from stored param", nameof(EntityEventHandlers.Script_JumpRelativeFromStoredParam_07D)) },
        {0x7E, new(0x7E, 1, "Conditional jump if true", nameof(EntityEventHandlers.Script_ConditionalJumpFromStoredParamIfTrue_07E)) },
        {0x7F, new(0x7F, 1, "Conditional jump if false", nameof(EntityEventHandlers.Script_ConditionalJumpFromStoredParamIfFalse_07F)) },
        {0x80, new(0x80, 5, "Jump from param if flag set", nameof(EntityEventHandlers.Script_JumpFromStoredParamIfFlagSet_080)) },
        {0x81, new(0x81, 5, "Jump from param if flag clear", nameof(EntityEventHandlers.Script_JumpFromStoredParamIfFlagClear_081)) },
        {0x82, new(0x82, 2, "Handle map trigger", nameof(EntityEventHandlers.Script_130_082)) },
        {0x83, new(0x83, 3, "If number of item > 0", nameof(EntityEventHandlers.Script_131_083)) },
        {0x84, new(0x84, 2, "Use item", nameof(EntityEventHandlers.Script_132_084)) },
        {0x85, new(0x85, 7, "Set map tiles", nameof(EntityEventHandlers.Script_133_085)) },
        {0x86, new(0x86, 4, "Add DamagedTickCounter", nameof(EntityEventHandlers.Script_134_086)) },
        {0x87, new(0x87, 3, "If TouchingEntity anim value", nameof(EntityEventHandlers.Script_135_087)) },
        {0x88, new(0x88, 3, "", nameof(EntityEventHandlers.Script_136_088)) },
        {0x89, new(0x89, 9, "Move entities", nameof(EntityEventHandlers.Script_137_089)) },
        {0x8A, new(0x8A, 8, "Spawn entity", nameof(EntityEventHandlers.Script_138_08A)) },
        {0x8B, new(0x8B, 9, "Spawn entity at position", nameof(EntityEventHandlers.Script_139_08B)) },
        {0x8C, new(0x8C, 2, "Random < variable", nameof(EntityEventHandlers.Script_140_08C)) },
        {0x8D, new(0x8D, 2, "Check PosZ <= TerrainHeight + 1", nameof(EntityEventHandlers.Script_141_08D)) },
        {0x8E, new(0x8E, 5, "Set scrolling parameters", nameof(EntityEventHandlers.Script_142_08E)) },
        {0x8F, new(0x8F, 1, "Set scrolling flag = 0", nameof(EntityEventHandlers.Script_143_08F)) },
        {0x90, new(0x90, 2, "Create effect", nameof(EntityEventHandlers.Script_144_090)) },
        {0x91, new(0x91, 2, "Disable effect", nameof(EntityEventHandlers.Script_145_091)) },
        {0x92, new(0x92, 3, "Set effect anim", nameof(EntityEventHandlers.Script_146_092)) },
        {0x93, new(0x93, 8, "Set effect pos", nameof(EntityEventHandlers.Script_147_093)) },
        {0x94, new(0x94, 8, "Set effect forces", nameof(EntityEventHandlers.Script_148_094)) },
        {0x95, new(0x95, 10, "Check entities within hitbox", nameof(EntityEventHandlers.Script_149_095)) },
        {0x96, new(0x96, 3, "Restore HP", nameof(EntityEventHandlers.Script_150_096)) },
        {0x97, new(0x97, 2, "Spend money", nameof(EntityEventHandlers.Script_151_097)) },
        {0x98, new(0x98, 3, "Add money", nameof(EntityEventHandlers.Script_152_098)) },
        {0x99, new(0x99, 3, "Try spend money", nameof(EntityEventHandlers.Script_153_099)) },
        {0x9A, new(0x9A, 3, "Enough money?", nameof(EntityEventHandlers.Script_154_09A)) },
        {0x9B, new(0x9B, 1, "Set warp disabled = 1", nameof(EntityEventHandlers.Script_155_09B)) },
        {0x9C, new(0x9C, 1, "Set warp disabled = 0", nameof(EntityEventHandlers.Script_156_09C)) },
        {0x9D, new(0x9D, 0, "Fatal Error - Exit", nameof(EntityEventHandlers.Script_157_09D)) },
        {0x9E, new(0x9E, 6, "Count walkable tile", nameof(EntityEventHandlers.Script_158_09E)) },
        {0x9F, new(0x9F, 2, "Wait open chest", nameof(EntityEventHandlers.Script_159_09F)) },
        {0xA0, new(0xA0, 8, "Adjusted effect pos", nameof(EntityEventHandlers.Script_160_0A0)) },
        {0xA1, new(0xA1, 9, "Set effect pos with entity", nameof(EntityEventHandlers.Script_161_0A1)) },
        {0xA2, new(0xA2, 8, "Create effect with pos", nameof(EntityEventHandlers.Script_162_0A2)) },
        {0xA3, new(0xA3, 9, "Create effect with entity pos", nameof(EntityEventHandlers.Script_163_0A3)) },
        {0xA4, new(0xA4, 3, "Set scrolling mode", nameof(EntityEventHandlers.Script_164_0A4)) },
        {0xA5, new(0xA5, 1, "Stop all sound", nameof(EntityEventHandlers.Script_165_0A5)) },
        {0xA6, new(0xA6, 2, "Load bgm", nameof(EntityEventHandlers.Script_166_0A6)) },
        {0xA7, new(0xA7, 3, "Play music", nameof(EntityEventHandlers.Script_167_0A7)) },
        {0xA8, new(0xA8, 1, "Is sound loading", nameof(EntityEventHandlers.Script_168_0A8)) },
        {0xA9, new(0xA9, 2, "", nameof(EntityEventHandlers.Script_169_0A9)) },
        {0xAA, new(0xAA, 2, "", nameof(EntityEventHandlers.Script_170_0AA)) },
        {0xAB, new(0xAB, 4, "Play sound with tone/volume/mix", nameof(EntityEventHandlers.Script_171_0AB)) },
        {0xAC, new(0xAC, 4, "Set gravity flags on entity", nameof(EntityEventHandlers.Script_172_0AC)) },
        {0xAD, new(0xAD, 9, "Check entity in AABB", nameof(EntityEventHandlers.Script_173_0AD)) },
        {0xAE, new(0xAE, 0, "Fatal Error - Exit", nameof(EntityEventHandlers.Script_174_0AE)) },
        {0xAF, new(0xAF, 7, "Set fade transition with color", nameof(EntityEventHandlers.Script_175_0AF)) },
        {0xB0, new(0xB0, 5, "Set warp fade color and duration", nameof(EntityEventHandlers.Script_176_0B0)) },
        {0xB1, new(0xB1, 1, "Check fade and warp flags", nameof(EntityEventHandlers.Script_177_0B1)) },
        {0xB2, new(0xB2, 3, "Compare entity groups", nameof(EntityEventHandlers.Script_CompareEntityGroupsForMatch_0B2)) },
        {0xB3, new(0xB3, 9, "Set pad buttons", nameof(EntityEventHandlers.Script_UpdatePadState)) },
        {0xB4, new(0xB4, 2, "Check ForceZ < 0", nameof(EntityEventHandlers.Script_180_0B4)) },
        {0xB5, new(0xB5, 2, "Check ForceZ >= 0", nameof(EntityEventHandlers.Script_181_0B5)) },
        {0xB6, new(0xB6, 2, "Check ForceZ != 0", nameof(EntityEventHandlers.Script_182_0B6)) },
        {0xB7, new(0xB7, 3, "Check TargetAnimation", nameof(EntityEventHandlers.Script_183_0B7)) },
        {0xB8, new(0xB8, 3, "Check TargetDirection", nameof(EntityEventHandlers.Script_184_0B8)) },
        {0xB9, new(0xB9, 2, "Start CD streaming", nameof(EntityEventHandlers.Script_185_0B9)) },
        {0xBA, new(0xBA, 1, "Check if loading from CD", nameof(EntityEventHandlers.Script_186_0BA)) },
        {0xBB, new(0xBB, 1, "Check retry or title screen", nameof(EntityEventHandlers.Script_187_0BB)) },
        {0xBC, new(0xBC, 2, "Increase player HPMax", nameof(EntityEventHandlers.Script_188_0BC)) },
        {0xBD, new(0xBD, 3, "Play sound 2", nameof(EntityEventHandlers.Script_189_0BD)) },
        {0xBE, new(0xBE, 3, "Play sound 2 (bis)", nameof(EntityEventHandlers.Script_190_0BE)) },
        {0xBF, new(0xBF, 5, "Play sound with tone/volume/mix", nameof(EntityEventHandlers.Script_191_0BF)) },
        {0xC0, new(0xC0, 2, "Set equipped weapon", nameof(EntityEventHandlers.Script_192_0C0)) },
        {0xC1, new(0xC1, 1, "Set player flag &= 0xffffff7f", nameof(EntityEventHandlers.Script_193_0C1)) },
        {0xC2, new(0xC2, 2, "Check something save", nameof(EntityEventHandlers.Script_194_0C2)) },
        {0xC3, new(0xC3, 1, "Restore and init HP and MP", nameof(EntityEventHandlers.Script_195_0C3)) },
        {0xC4, new(0xC4, 6, "Dialog with entity and name", nameof(EntityEventHandlers.Script_196_0C4)) },
        {0xFF, new(0xFF, 1, "End script", "end script") }
    };

    public static string CreateLog(int position, byte code, byte[] parameters, bool withHandlerName)
    {
        var commandProperty = CommandPropertiesByCode.GetValueOrDefault(code);
        
        string log = $"0x{code:x2} {commandProperty?.Description ?? "?"}";
        log += $" {GetEventDescriptionValue(position, code, parameters)}";

        if (withHandlerName)
        {
            log += $" ({commandProperty.HandlerName})";
        }

        return log;
    }

    public static string GetHandlerName(byte code)
    {
        var commandProperty = CommandPropertiesByCode.GetValueOrDefault(code);
        return commandProperty?.HandlerName ?? "?";
    }

    private static string GetEventDescriptionValue(int position, byte code, byte[] parameters)
    {
        return code switch
        {
            0x00 or 0xFF => string.Empty,
            0x02 or 0x03 or 0x04 => GetGotoDescription(position, parameters),
            0x05 or 0x06 => GetFlagDescription(position, parameters),
            0x0B => $"{(parameters[2] << 8) | parameters[1]}",
            0x0D => GetDialogDescription(position, parameters),
            0x20 => GetParametersAsDecimal(parameters, 2),
            0x30 or 0x31 => GetFlagDescription(position, parameters),
            0x33 => GetCheckFlagOnDescription(position, parameters),
            0x35 or 0x36 => GetFlagDescription(position, parameters),
            0x37 => GetWaitDescription(position, parameters),
            0x38 => $"[{GetParametersAsDecimal(parameters, 2)}] = {parameters[2] | (parameters[3] << 8)}",
            0x3B => $"TileX >= {parameters[0]} and TileX <= {parameters[1]} and TileY >= {parameters[2]} and TileY <= {parameters[3]} and TileZ >= {parameters[4]} and TileZ <= {parameters[5]}",
            0x40 => $"[{parameters[0]}]={parameters[1]}",
            0x09 or 0x1A or 0x2D or 0xA6 or 0x67 or 0x50 or 0x2C or 0x9F => GetParametersAsDecimal(parameters, 1),
            0x1C or 0x1D => GetRepeatAnimationDescription(position, parameters),
            0x1E or 0x1F => GetWalkDescription(position, parameters),
            0x55 => $"x:{parameters[0]} y:{parameters[1]} -> walkability:{parameters[2]} groundProperty:{parameters[3]}",
            0x57 or 0x58 => GetDirectionBranchDescription(position, parameters),
            0x59 => $"search type:{parameters[0]} -> TargetAnimationId:{parameters[1]}",
            0x5B => $"search type:{parameters[0]} -> TargetAnimationId:{parameters[1]} TargetDirection:{parameters[2]}",
            0x5C => GetDialogWithChoiceDescription(position, parameters),
            0x62 => $"search type:{parameters[0]} {(parameters[2] << 8) | parameters[1]}",
            0x63 => GetSetGravityFlagDescription(parameters),
            0x64 => GetSetPositionDescription(position, parameters),
            0x78 => $"entity[{GetParametersAsDecimal(parameters, 1)}]",
            0x85 => $"x:{parameters[0]} y:{parameters[1]} w:{parameters[2]} h:{parameters[3]} -> x:{parameters[4]} y:{parameters[5]}",
            0x89 => GetMoveEntitiesDescription(parameters),
            0x8D => $"search type:{parameters[0]}",
            0x8E => $"speedX:{parameters[0]} speedY:{parameters[1]} limitX:{parameters[2]} limitY:{parameters[3]}",
            0x92 => $"MapEffectId:{parameters[0]} TargetAnimationId:{parameters[1]}",
            0xA0 => $"MapEffectId:{parameters[0]} x+={(parameters[2] << 8) | parameters[1]} y+={(parameters[4] << 8) | parameters[3]} z+={(parameters[6] << 8) | parameters[5]}", 
            0xA7 => $"sound:{parameters[0]} stop all sound:{parameters[1]}",
            0xBD or 0xBE => GetParametersAsDecimal(parameters, 2),
            0xC4 => GetDialogWithChoiceDescription(position, parameters),
            _ => parameters.Length > 0 ? $"p:[{string.Join(',', parameters)}]" : string.Empty
        };
    }

    private static string GetMoveEntitiesDescription(byte[] parameters)
    {
        return $"search type:{parameters[0]} and {parameters[1]} move offset x:{(parameters[3] << 8) | parameters[2]} y:{(parameters[5] << 8) | parameters[4]} z:{(parameters[7] << 8) | parameters[6]}";
    }

    private static string GetCheckFlagOnDescription(int position, byte[] parameters)
    {
        var description = "";
        
        for (int i = 0; i < 4; i++)
        {
            var type = parameters[i * 2] + (parameters[i * 2 + 1] << 8);
            var flag = ((type >> 3) & 0x3ff) >> 2;
            description += (flag & 0x8000) == 0 ? "GameFlags" : "TemporaryFlags";
            var bitToCheck = type & 0x1f;
            var mask = 1 << bitToCheck;
            description += $"[{flag}] & 0x{mask:X} != 0 and ";
        }

        return description;
    }

    private static string GetSetGravityFlagDescription(byte[] parameters)
    {
        var clearMask = (ushort)((parameters[2] << 8) | parameters[1]);
        uint andMask = 0xFFFF0000u | (uint)(~clearMask & 0xFFFF);
        return $"search type:{parameters[0]} Flags &= 0x{andMask:X}";
    }

    private static string GetGotoDescription(int position, byte[] parameters)
    {
        var jump = ((parameters[0] | (parameters[1] << 8)) * 0x10000) >> 0x10;
        return $"{position + jump} (jump={jump})";
    }
    
    private static string GetFlagDescription(int position, byte[] parameters)
    {
        var flag = parameters[0] | (parameters[1] << 8);
        var name = (flag & 0x8000) == 0 ? "GameFlags" : "TemporaryFlags";
        name += $"[{((flag >> 3) & 0xffc) >> 2}]";
        name += $" with mask 0x{1 << (parameters[0] & 0x1f):X}";
        return name;
    }
    
    private static string GetDialogDescription(int position, byte[] parameters)
    {
        return $"text id:{parameters[0]} player control:{parameters[1]}";
    }
    
    private static string GetWaitDescription(int position, byte[] parameters)
    {
        return $"{GetParametersAsDecimal(parameters, 1)} frames";
    }
    
    private static string GetRepeatAnimationDescription(int position, byte[] parameters)
    {
        return $"for {GetParametersAsDecimal(parameters, 1)} times";
    }
    
    private static string GetWalkDescription(int position, byte[] parameters)
    {
        return $"at least {GetParametersAsDecimal(parameters, 2)} pixels";
    }

    private static string GetParametersAsDecimal(byte[] parameters, int numberOfIndices)
    {
        switch (numberOfIndices)
        {
            case 1:
                return parameters[0].ToString();
            case 2:
                return (parameters[0] | (parameters[1] << 8)).ToString("D3");
        }
        
        return string.Join(", ", parameters.Select(x => x.ToString("D3")));
    }

    private static string GetDirectionBranchDescription(int position, byte[] parameters)
    {
        var log = "";

        for (int i = 0; i < 4; i++)
        {
            int v1 = parameters[i * 2 + 0];
            int v2 = parameters[i * 2 + 1];
            var jump = ((v1 + v2 * 0x100) * 0x10000) >> 0x10;
            log += $"{i}:{position + jump}(jump:{jump}) ";
        }

        return log;
    }

    private static string GetDialogWithChoiceDescription(int position, byte[] parameters)
    {
        return $"entity index:{parameters[0]} text id:{parameters[1]} player control:{parameters[2]}";
    }

    private static string GetSetPositionDescription(int position, byte[] parameters)
    {
        var searchType = parameters[0];
        var x = parameters[1] | (parameters[2] << 8);
        var y = parameters[3] | (parameters[4] << 8);
        var z = parameters[5] | (parameters[6] << 8);
        return $"search type:{searchType} x:{x} y:{y} z:{z}";
    }
}