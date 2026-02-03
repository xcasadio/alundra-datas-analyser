using AlundraEngine.DatasBin;
using System.Xml.Linq;

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

        //Debugger.Break();

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
        {0x01, new(0x01, 1, "Do nothing", nameof(EntityEventHandlers.Script_DoNothing)) },
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
        {0x10, new(0x10, 1, "Player lose control", nameof(EntityEventHandlers.Script_16_010)) },
        {0x11, new(0x11, 1, "Player gain control", nameof(EntityEventHandlers.Script_17_011)) },
        {0x12, new(0x12, 2, "Play sound 1", nameof(EntityEventHandlers.Script_18_012)) },
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
        {0x24, new(0x24, 1, "Wait force adjusted", nameof(EntityEventHandlers.Script_36_024)) },
        {0x25, new(0x25, 1, "Wait entity collision z or 144", nameof(EntityEventHandlers.Script_37_025)) },
        {0x26, new(0x26, 1, "Wait force adjusted or entity collision z", nameof(EntityEventHandlers.Script_38_026)) },
        {0x27, new(0x27, 1, "Face player", nameof(EntityEventHandlers.Script_39_027)) },
        {0x28, new(0x28, 1, "Gravity flag 2 on", nameof(EntityEventHandlers.Script_40_028)) },
        {0x29, new(0x29, 1, "Gravity flag 2 off", nameof(EntityEventHandlers.Script_41_029)) },
        {0x2A, new(0x2A, 1, "Gravity flag 3 on", nameof(EntityEventHandlers.Script_42_02A)) },
        {0x2B, new(0x2B, 1, "Gravity flag 3 off", nameof(EntityEventHandlers.Script_43_02B)) },
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
        {0x38, new(0x38, 5, "Register warp", nameof(EntityEventHandlers.Script_SetSaveMapIdToInternalMapIndex_038)) },
        {0x39, new(0x39, 1, "Wait for dialog", nameof(EntityEventHandlers.Script_IsDialogInProgress_039)) },
        {0x3B, new(0x3B, 7, "Check player in area", nameof(EntityEventHandlers.Script_59_03B)) },
        {0x40, new(0x40, 3, "Set program index", nameof(EntityEventHandlers.Script_64_040)) },
        {0x41, new(0x41, 3, "Set sprite program index", nameof(EntityEventHandlers.Script_65_041)) },
        {0x44, new(0x44, 1, "Wait dialog choice", nameof(EntityEventHandlers.Script_68_044)) },
        {0x45, new(0x45, 1, "Gravity flag 4 off", nameof(EntityEventHandlers.Script_69_045)) },
        {0x46, new(0x46, 1, "Gravity flag 4 on", nameof(EntityEventHandlers.Script_70_046)) },
        {0x49, new(0x49, 1, "Restart", nameof(EntityEventHandlers.Script_73_049)) },
        {0x4A, new(0x4A, 1, "If true restart", nameof(EntityEventHandlers.Script_74_04A)) },
        {0x4B, new(0x4B, 1, "If false restart", nameof(EntityEventHandlers.Script_75_04B)) },
        {0x4C, new(0x4C, 2, "Set dialog something", nameof(EntityEventHandlers.Script_76_04C)) },
        {0x4D, new(0x4D, 1, "Check dialog something", nameof(EntityEventHandlers.Script_77_04D)) },
        {0x50, new(0x50, 2, "Set dialog choice", nameof(EntityEventHandlers.Script_SetEtcAnimationMode)) },
        {0x51, new(0x51, 1, "Get dialog choice", nameof(EntityEventHandlers.Script_TryActivateTextHoldState)) },
        {0x54, new(0x54, 5, "Set walkable", nameof(EntityEventHandlers.Script_84_054)) },
        {0x55, new(0x55, 5, "Set unwalkable", nameof(EntityEventHandlers.Script_85_055)) },
        {0x58, new(0x58, 9, "Directional branch", nameof(EntityEventHandlers.Script_88_058)) },
        {0x59, new(0x59, 3, "Set entity anim", nameof(EntityEventHandlers.Script_89_059)) },
        {0x5A, new(0x5A, 3, "Turn entity", nameof(EntityEventHandlers.Script_90_05A)) },
        {0x5B, new(0x5B, 4, "Turn entity with anim", nameof(EntityEventHandlers.Script_91_05B)) },
        {0x5C, new(0x5C, 4, "Dialog with entity", nameof(EntityEventHandlers.Script_OpenDialogWithChoice_05C)) },
        {0x62, new(0x62, 4, "Set entities flags", nameof(EntityEventHandlers.Script_98_062)) },
        {0x63, new(0x63, 4, "Set entities gravity", nameof(EntityEventHandlers.Script_99_063)) },
        {0x64, new(0x64, 8, "Set entities position", nameof(EntityEventHandlers.Script_100_064)) },
        {0x65, new(0x65, 8, "Move entity position", nameof(EntityEventHandlers.Script_101_065)) },
        {0x67, new(0x67, 2, "Camera follow entity", nameof(EntityEventHandlers.Script_103_067)) },
        {0x69, new(0x69, 7, "Camera look at", nameof(EntityEventHandlers.Script_105_069)) },
        {0x70, new(0x70, 1, "Is above ground", nameof(EntityEventHandlers.Script_112_070)) },
        {0x73, new(0x73, 2, "Initialize timer _30", nameof(EntityEventHandlers.Script_115_073)) },
        {0x74, new(0x74, 3, "Update timer _30", nameof(EntityEventHandlers.Script_116_074)) },
        {0x78, new(0x78, 4, "Store choice param and jump", nameof(EntityEventHandlers.Script_StoreChoiceParamAndJump_078)) },
        {0x85, new(0x85, 7, "Set map tiles", nameof(EntityEventHandlers.Script_133_085)) },
        {0x8B, new(0x8B, 9, "Spawn entity according to entity position", nameof(EntityEventHandlers.Script_139_08B)) },
        {0x90, new(0x90, 2, "Create effect", nameof(EntityEventHandlers.Script_144_090)) },
        {0x91, new(0x91, 2, "Disable effect", nameof(EntityEventHandlers.Script_145_091)) },
        {0x92, new(0x92, 3, "Set effect anim", nameof(EntityEventHandlers.Script_146_092)) },
        {0x93, new(0x93, 8, "Set effect pos", nameof(EntityEventHandlers.Script_147_093)) },
        {0x94, new(0x94, 8, "Set effect forces", nameof(EntityEventHandlers.Script_148_094)) },
        {0xA0, new(0xA0, 8, "Adjusted effect pos", nameof(EntityEventHandlers.Script_160_0A0)) },
        {0xA1, new(0xA1, 9, "Set effect pos with entity", nameof(EntityEventHandlers.Script_161_0A1)) },
        {0xA2, new(0xA2, 8, "Create effect with pos", nameof(EntityEventHandlers.Script_162_0A2)) },
        {0xA3, new(0xA3, 9, "Create effect with entity pos", nameof(EntityEventHandlers.Script_163_0A3)) },
        {0xA7, new(0xA7, 3, "Play music", nameof(EntityEventHandlers.Script_167_0A7)) },
        {0xAC, new(0xAC, 4, "Set gravity flags on entity", nameof(EntityEventHandlers.Script_172_0AC)) },
        {0xBD, new(0xBD, 3, "Play sound 2", nameof(EntityEventHandlers.Script_189_0BD)) },
        {0xC4, new(0xC4, 6, "Dialog with entity and name", nameof(EntityEventHandlers.Script_196_0C4)) },
        {0xFF, new(0xFF, 1, "End script", "end script") }
    };

    public static string CreateLog(int position, byte code, byte[] parameters, bool withHandlerName)
    {
        var commandProperty = CommandPropertiesByCode.GetValueOrDefault(code);
        
        string log = $"{code:x2} {commandProperty?.Description ?? "?"}";
        log += GetEventDescriptionValue(position, code, parameters);

        if (withHandlerName)
        {
            log += $" ({commandProperty.HandlerName})";
        }

        return log;
    }

    private static string GetEventDescriptionValue(int position, byte code, byte[] parameters)
    {
        //var parametersWithCode = new byte[parameters.Length + 1];
        //parametersWithCode[0] = code;
        //Buffer.BlockCopy(parameters, 0, parametersWithCode, 1, parameters.Length);
        
        return code switch
        {
            0x02 or 0x03 or 0x04 => GetGotoDescription(position, parameters),
            0x05 or 0x06 => GetFlagDescription(code, parameters),
            0x0D => GetDialogDescription(code, parameters),
            0x30 or 0x31 => GetFlagDescription(code, parameters),
            0x35 or 0x36 => GetFlagDescription(code, parameters),
            0x37 => GetWaitDescription(code, parameters),
            0x1A => GetParametersAsDecimal(parameters, 1),
            0x1C or 0x1D => GetRepeatAnimationDescription(code, parameters),
            0x1E or 0x1F => GetWalkDescription(code, parameters),
            0x58 => new DirectionBranchCommand(code, parameters),
            0x5C => new DialogCommandWithChoice(code, parameters),
            0x64 => new SetPositionCommand(code, parameters),
            0x78 => new GotoCommand(code, parameters),
            0xC4 => new DialogWithEntityAndNameCommand(code, parameters),
            
            _ => string.Join(',', parameters)
        };
    }

    private static string GetGotoDescription(int position, byte[] parameters)
    {
        var jump = (parameters[0] | (parameters[1] << 8));
        return $"{position + jump} (jump={jump})";
    }
    
    private static string GetFlagDescription(int position, byte[] parameters)
    {
        var flag = (parameters[0] | (parameters[1] << 8));
        var name = (flag & 0x8000) == 0 ? "MapFlags" : "GlobalFlags";
        name += $"[{((flag >> 3) & 0xffc) >> 2}]";
        name += $" with mask {1 << (parameters[0] & 0x1f)}";
        return name;
    }
    
    private static string GetDialogDescription(int position, byte[] parameters)
    {
        return $"text id={parameters[0]} player control={parameters[1]}";
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
                return parameters[0].ToString("D3");
            case 2:
                return (parameters[0] | (parameters[1] << 8)).ToString("D3");
        }
        
        return string.Join(", ", parameters.Select(x => x.ToString("D3")));
    }
}