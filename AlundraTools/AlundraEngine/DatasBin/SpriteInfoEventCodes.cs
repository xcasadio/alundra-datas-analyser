using static AlundraEngine.Gameplay.Scripts.EntityEventHandlers;
namespace AlundraEngine.DatasBin;

public class SpriteInfoEventCodes
{
    private readonly long _binOffset;
    private readonly int _dataSize;
    private readonly int _memoryAddress;
    public readonly short[] EventCodesATable;
    public readonly short[] EventCodesBTable;
    public readonly short[] EventCodesCTable;
    public readonly short[] EventCodesDTable;
    public readonly short[] EventCodesETable;
    public readonly short[] EventCodesFTable;

    public readonly byte[] Codes;

    public SpriteInfoEventCodes(BinaryReader br, long binOffset, SpriteInfoHeader header, bool ismap)
    {
        var tableSize = 0;
        short firstOffset = 0;

        //read sector1a
        br.BaseStream.Position = binOffset + header.EventCodesAPointer;
        tableSize = header.EventCodesASize / 2;
        EventCodesATable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesATable[i] = br.ReadInt16();
            if (firstOffset == 0 && EventCodesATable[i] != 0)
            {
                firstOffset = EventCodesATable[i];
            }
        }

        //read sector1b
        br.BaseStream.Position = binOffset + header.EventCodesBPointer;
        tableSize = header.EventCodesBSize / 2;
        EventCodesBTable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesBTable[i] = br.ReadInt16();
            if (firstOffset == 0 && EventCodesBTable[i] != 0)
            {
                firstOffset = EventCodesBTable[i];
            }
        }

        //read sector1c
        br.BaseStream.Position = binOffset + header.EventCodesCPointer;
        tableSize = header.EventCodesCSize / 2;
        EventCodesCTable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesCTable[i] = br.ReadInt16();
            if (firstOffset == 0 && EventCodesCTable[i] != 0)
            {
                firstOffset = EventCodesCTable[i];
            }
        }

        //read sector1d
        br.BaseStream.Position = binOffset + header.EventCodesDPointer;
        tableSize = header.EventCodesDSize / 2;
        EventCodesDTable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesDTable[i] = br.ReadInt16();
            if (firstOffset == 0 && EventCodesDTable[i] != 0)
            {
                firstOffset = EventCodesDTable[i];
            }
        }

        //read sector1e
        br.BaseStream.Position = binOffset + header.EventCodesEPointer;
        tableSize = header.EventCodesESize / 2;
        EventCodesETable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesETable[i] = br.ReadInt16();
            if (firstOffset == 0 && EventCodesETable[i] != 0)
            {
                firstOffset = EventCodesETable[i];
            }
        }

        //read sector1f
        header.EventCodesFSize = header.EventCodesAPointer + firstOffset - header.EventCodesFPointer;
        br.BaseStream.Position = binOffset + header.EventCodesFPointer;
        tableSize = header.EventCodesFSize / 2;
        if (tableSize < 0)
        {
            tableSize = 16;
        }

        EventCodesFTable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesFTable[i] = br.ReadInt16();
        }

        //set binOffset for eventcodes
        _binOffset = binOffset + header.EventCodesAPointer;
        _memoryAddress = header.MemoryAddress + header.EventCodesAPointer;
        _dataSize = (header.EntitiesPointer == 0 ? header.EventCodesFPointer : header.EntitiesPointer) - header.EventCodesAPointer;
        Codes = new byte[_dataSize];
        br.BaseStream.Position = _binOffset;
        br.Read(Codes, 0, Codes.Length);
    }

    public class SiCode
    {
        public byte Code { get; init; }
        public string Name { get; init; }
        public int Size { get; init; }
    }

    public static SiCode GetCode(byte code)
    {
        if (CommandSizeByCode.TryGetValue(code, out var size))
        {
            return new SiCode
            {
                Code = code,
                Size = size,
                Name = CommandNameByCode.GetValueOrDefault(code, "")
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

    public List<SiCommand> GetCommands(BinaryReader br, int eventCodesOffset, bool stopAtff = false, int commandsSize = 0)
    {
        var commands = new List<SiCommand>();
        br.BaseStream.Position = _binOffset + eventCodesOffset;
        var bytes = new byte[_dataSize - eventCodesOffset];
        br.Read(bytes, 0, bytes.Length);
        var i = 0;

        while (i < bytes.Length && (commandsSize == 0 || i < commandsSize))
        {
            var value = bytes[i++];
            var sicode = GetCode(value);

            if (sicode.Size < 1)
            {
                continue;
            }

            var size = sicode.Size;
            var name = sicode.Name;
            var parameters = new byte[size - 1];
            var j = 0;
            var offset = i;

            while (j < size - 1)
            {
                parameters[j++] = bytes[i++];
            }

            var address = _memoryAddress + eventCodesOffset + i - size;
            var cmd = new SiCommand(value, parameters, name, offset);
            commands.Add(cmd);

            if (stopAtff && value == 0xff)
            {
                break;
            }
        }

        return commands;
    }

    public List<SiCommand> GetCommands(int startOffset)
    {
        var commands = new List<SiCommand>();
        var i = 0; //startOffset;

        while (i < Codes.Length)
        {
            var value = Codes[i++];
            var siCode = GetCode(value);

            if (siCode.Size < 1)
            {
                continue;
            }

            var size = siCode.Size;
            var name = siCode.Name;
            var parameters = new byte[size - 1];
            var j = 0;
            var offset = i;

            while (j < size - 1)
            {
                parameters[j++] = Codes[i++];
            }

            var cmd = new SiCommand(value, parameters, name, offset);
            commands.Add(cmd);
        }

        return commands;
    }

    public static readonly Dictionary<byte, int> CommandSizeByCode = new()
    {
        { 0x00, 0 }, //nothing
        { 0x01, 0 }, //debug
        { 0x02, 3 },
        { 0x03, 3 },
        { 0x04, 3 },
        { 0x05, 3 },
        { 0x06, 3 },
        { 0x07, 8 },
        { 0x08, 2 },
        { 0x09, 2 },
        { 0x0A, 1 },
        { 0x0B, 4 },
        { 0x0C, 1 },
        { 0x0D, 3 },
        { 0x0E, 0 }, //null
        { 0x0F, 0 }, //null
        { 0x10, 1 },
        { 0x11, 1 },
        { 0x12, 2 },
        { 0x13, 0 }, //null
        { 0x14, 0 }, //null
        { 0x15, 1 },
        { 0x16, 1 },
        { 0x17, 1 },
        { 0x18, 0 }, //null
        { 0x19, 1 },
        { 0x1A, 2 },
        { 0x1B, 3 },
        { 0x1C, 2 },
        { 0x1D, 2 },
        { 0x1E, 3 },
        { 0x1F, 3 },
        { 0x20, 3 },
        { 0x21, 3 },
        { 0x22, 1 },
        { 0x23, 1 },
        { 0x24, 1 },
        { 0x25, 1 },
        { 0x26, 1 },
        { 0x27, 1 },
        { 0x28, 1 },
        { 0x29, 1 },
        { 0x2A, 1 },
        { 0x2B, 1 },
        { 0x2C, 2 },
        { 0x2D, 2 },
        { 0x2E, 2 },
        { 0x2F, 4 },
        { 0x30, 5 },
        { 0x31, 5 },
        { 0x32, 3 },
        { 0x33, 9 },
        { 0x34, 9 },
        { 0x35, 3 },
        { 0x36, 3 },
        { 0x37, 2 },
        { 0x38, 5 },
        { 0x39, 1 },
        { 0x3A, 2 },
        { 0x3B, 7 },
        { 0x3C, 7 },
        { 0x3D, 7 },
        { 0x3E, 1 },
        { 0x3F, 1 },
        { 0x40, 3 },
        { 0x41, 3 },
        { 0x42, 1 },
        { 0x43, 2 },
        { 0x44, 1 },
        { 0x45, 1 },
        { 0x46, 1 },
        { 0x47, 1 },
        { 0x48, 1 },
        { 0x49, 1 },
        { 0x4A, 1 },
        { 0x4B, 1 },
        { 0x4C, 2 },
        { 0x4D, 1 },
        { 0x4E, 2 },
        { 0x4F, 1 },
        { 0x50, 2 },
        { 0x51, 1 },
        { 0x52, 1 },
        { 0x53, 8 },
        { 0x54, 5 },
        { 0x55, 5 },
        { 0x56, 2 },
        { 0x57, 9 },
        { 0x58, 9 },
        { 0x59, 3 },
        { 0x5A, 3 },
        { 0x5B, 4 },
        { 0x5C, 4 },
        { 0x5D, 2 },
        { 0x5E, 4 },
        { 0x5F, 1 },
        { 0x60, 2 },
        { 0x61, 2 },
        { 0x62, 4 },
        { 0x63, 4 },
        { 0x64, 8 },
        { 0x65, 8 },
        { 0x66, 3 },
        { 0x67, 2 },
        { 0x68, 1 },
        { 0x69, 7 },
        { 0x6A, 1 },
        { 0x6B, 3 },
        { 0x6C, 3 },
        { 0x6D, 3 },
        { 0x6E, 1 },
        { 0x6F, 1 },
        { 0x70, 1 },
        { 0x71, 1 },
        { 0x72, 1 },
        { 0x73, 2 },
        { 0x74, 3 },
        { 0x75, 2 },
        { 0x76, 0 }, //debug
        { 0x77, 0 }, //debug
        { 0x78, 4 },
        { 0x79, 3 },
        { 0x7A, 3 },
        { 0x7B, 5 },
        { 0x7C, 5 },
        { 0x7D, 1 },
        { 0x7E, 1 },
        { 0x7F, 1 },
        { 0x80, 5 },
        { 0x81, 5 },
        { 0x82, 2 },
        { 0x83, 3 },
        { 0x84, 2 },
        { 0x85, 7 },
        { 0x86, 4 },
        { 0x87, 3 },
        { 0x88, 3 },
        { 0x89, 9 },
        { 0x8A, 8 },
        { 0x8B, 9 },
        { 0x8C, 2 },
        { 0x8D, 2 },
        { 0x8E, 5 },
        { 0x8F, 1 },
        { 0x90, 2 },
        { 0x91, 2 },
        { 0x92, 3 },
        { 0x93, 8 },
        { 0x94, 8 },
        { 0x95, 10},
        { 0x96, 3 },
        { 0x97, 2 },
        { 0x98, 3 },
        { 0x99, 3 },
        { 0x9A, 3 },
        { 0x9B, 1 },
        { 0x9C, 1 },
        { 0x9D, 0 }, // fatal error
        { 0x9E, 6 },
        { 0x9F, 2 },
        { 0xA0, 8 },
        { 0xA1, 9 },
        { 0xA2, 8 },
        { 0xA3, 9 },
        { 0xA4, 3 },
        { 0xA5, 1 },
        { 0xA6, 2 },
        { 0xA7, 3 },
        { 0xA8, 1 },
        { 0xA9, 2 },
        { 0xAA, 2 },
        { 0xAB, 4 },
        { 0xAC, 4 },
        { 0xAD, 9 },
        { 0xAE, 0 }, // fatal error
        { 0xAF, 7 },
        { 0xB0, 5 },
        { 0xB1, 1 },
        { 0xB2, 3 },
        { 0xB3, 9 },
        { 0xB4, 2 },
        { 0xB5, 2 },
        { 0xB6, 2 },
        { 0xB7, 3 },
        { 0xB8, 3 },
        { 0xB9, 2 },
        { 0xBA, 1 },
        { 0xBB, 1 },
        { 0xBC, 2 },
        { 0xBD, 3 },
        { 0xBE, 3 },
        { 0xBF, 5 },
        { 0xC0, 2 },
        { 0xC1, 1 },
        { 0xC2, 2 },
        { 0xC3, 1 },
        { 0xC4, 6 },
        // preserve original entries beyond 0xC4
        { 0xFF, 1 },
    };

    public static readonly Dictionary<byte, string> CommandNameByCode = new()
    {
        { 0x00, "Break" },
        { 0x01, "Do nothing" },
        { 0x02, "Goto" },
        { 0x03, "If true goto" },
        { 0x04, "If false goto" },
        { 0x05, "Flag on" },
        { 0x06, "Flag off" },
        { 0x07, "Check entity in area" },
        { 0x08, "Turn" },
        { 0x09, "Set direction" },
        { 0x0A, "Reverse direction" },
        { 0x0B, "Wait until entity moves beyond radius" },
        { 0x0C, "Set random dir" },
        { 0x0D, "Dialog" },
        { 0x0E, "Do nothing" },
        { 0x0F, "Do nothing" },
        { 0x10, "Player lose control" },
        { 0x11, "Player gain control" },
        { 0x12, "Play sound 1" },
        { 0x13, "Do nothing" },
        { 0x14, "Do nothing" },
        { 0x15, "Reset z pos" },
        { 0x16, "High gravity" },
        { 0x17, "Low gravity" },
        { 0x18, "Do nothing" },
        { 0x19, "Deactivate entity" },
        { 0x1A, "Set anim" },
        { 0x1B, "Fly" },
        { 0x1C, "Repeat anim" },
        { 0x1D, "Repeat anim with collision" },
        { 0x1E, "Walk" },
        { 0x1F, "Walk with collision" },
        { 0x20, "Check ZDistance and collidedWithEntityZ" },
        { 0x21, "Is within Z distance" },
        { 0x22, "Clamp forceZ to height target" },
        { 0x23, "Clamp forceZ to height target and no collidedWithEntityZ" },
        { 0x24, "Wait force adjusted" },
        { 0x25, "Wait entity collision z or 144" },
        { 0x26, "Wait force adjusted or entity collision z" },
        { 0x27, "Face player" },
        { 0x28, "Gravity flag 2 on" },
        { 0x29, "Gravity flag 2 off" },
        { 0x2A, "Gravity flag 3 on" },
        { 0x2B, "Gravity flag 3 off" },
        { 0x2C, "Check no entity found by function id" },
        { 0x2D, "Activate entity" },
        { 0x2E, "Hide" },
        { 0x2F, "Check moving in dir" },
        { 0x30, "If flag on" },
        { 0x31, "If flag off" },
        { 0x32, "Toggle flag" },
        { 0x33, "Check flags on" },
        { 0x34, "Check flags off" },
        { 0x35, "Until flag off" },
        { 0x36, "Until flag on" },
        { 0x37, "Wait" },
        { 0x38, "Register warp" },
        { 0x39, "Wait for dialog" },
        { 0x3A, "Set TargetDirection" },
        { 0x3B, "Check player in area" },
        { 0x3C, "??? 0x3C" },
        { 0x3D, "??? 0x3D" },
        { 0x3E, "??? 0x3E" },
        { 0x3F, "??? 0x3F" },
        { 0x40, "Set program index" },
        { 0x41, "Set sprite program index" },
        { 0x42, "??? 0x42" },
        { 0x43, "??? 0x43" },
        { 0x44, "Wait dialog choice" },
        { 0x45, "Gravity flag 4 off" },
        { 0x46, "Gravity flag 4 on" },
        { 0x47, "??? 0x47" },
        { 0x48, "??? 0x48" },
        { 0x49, "Restart" },
        { 0x4A, "If true restart" },
        { 0x4B, "If false restart" },
        { 0x4C, "Set dialog something" },
        { 0x4D, "Check dialog something" },
        { 0x4E, "??? 0x4E" },
        { 0x4F, "??? 0x4F" },
        { 0x50, "Set dialog choice" },
        { 0x51, "Get dialog choice" },
        { 0x52, "??? 0x52" },
        { 0x53, "Change map" },
        { 0x54, "Set walkable" },
        { 0x55, "Set unwalkable" },
        { 0x56, "??? 0x56" },
        { 0x57, "??? 0x57" },
        { 0x58, "Directional branch" },
        { 0x59, "Set entity anim" },
        { 0x5A, "Turn entity" },
        { 0x5B, "Turn entity with anim" },
        { 0x5C, "Dialog with entity" },
        { 0x5D, "??? 0x5D" },
        { 0x5E, "??? 0x5E" },
        { 0x5F, "??? 0x5F" },
        { 0x60, "??? 0x60" },
        { 0x61, "??? 0x61" },
        { 0x62, "Set entities flags" },
        { 0x63, "Set entities gravity" },
        { 0x64, "Set entities position" },
        { 0x65, "Move entity position" },
        { 0x66, "??? 0x66" },
        { 0x67, "Follow entity" },
        { 0x68, "??? 0x68" },
        { 0x69, "Camera look at" },
        { 0x6A, "??? 0x6A" },
        { 0x6B, "??? 0x6B" },
        { 0x6C, "??? 0x6C" },
        { 0x6D, "??? 0x6D" },
        { 0x6E, "Is force adjusted" },
        { 0x6F, "Is collided with entity Z" },
        { 0x70, "Is above ground" },
        { 0x71, "Get hit counter" },
        { 0x72, "Set LastTargetAnimationId and LastTargetDirection" },
        { 0x73, "Initialize timer _30" },
        { 0x74, "Update timer _30" },
        { 0x75, "Play sound 1 byte" },
        { 0x76, "Do nothing - PrintCommandMap" },
        { 0x77, "Do nothing - PrintCommandMap" },
        { 0x78, "Store choice param and jump" },
        { 0x79, "Jump if choice accepted" },
        { 0x7A, "Jump if choice rejected" },
        { 0x7B, "Jump if flag set store param" },
        { 0x7C, "Jump if flag clear store param" },
        { 0x7D, "Jump relative from stored param" },
        { 0x7E, "Conditional jump from stored param if true" },
        { 0x7F, "Conditional jump from stored param if false" },
        { 0x80, "Jump from stored param if flag set" },
        { 0x81, "Jump from stored param if flag clear" },
        { 0x82, "HandleMapTriggerCommand" },
        { 0x83, "If number of item > 0" },
        { 0x84, "Use item" },
        { 0x85, "Set map tiles" },
        { 0x86, "??? 0x86" },
        { 0x87, "??? 0x87" },
        { 0x88, "??? 0x88" },
        { 0x89, "??? 0x89" },
        { 0x8A, "Spawn entity" },
        { 0x8B, "Spawn entity according to entity position" },
        { 0x8C, "Random < variable" },
        { 0x8D, "??? 0x8D" },
        { 0x8E, "??? 0x8E" },
        { 0x8F, "??? 0x8F" },
        { 0x90, "Create effect" },
        { 0x91, "Disable effect" },
        { 0x92, "Set effect anim" },
        { 0x93, "Set effect pos" },
        { 0x94, "Set effect forces" },
        { 0x95, "??? 0x95" },
        { 0x96, "Restore HP" },
        { 0x97, "Spend money" },
        { 0x98, "Add money" },
        { 0x99, "Try spend money" },
        { 0x9A, "Enough money ?" },
        { 0x9B, "Set g_isWarpDisabled = 1" },
        { 0x9C, "Set g_isWarpDisabled = 0" },
        { 0x9D, "Exit() - Fatal Error" },
        { 0x9E, "??? 0x9E" },
        { 0x9F, "??? 0x9F" },
        { 0xA0, "Adjusted effect pos" },
        { 0xA1, "Set effect pos with entity" },
        { 0xA2, "Create effect with pos" },
        { 0xA3, "Create effect with entity pos" },
        { 0xA4, "??? 0xA4" },
        { 0xA5, "Stop all sound" },
        { 0xA6, "Load bgm" },
        { 0xA7, "Play music" },
        { 0xA8, "??? 0xA8" },
        { 0xA9, "??? 0xA9" },
        { 0xAA, "??? 0xAA" },
        { 0xAB, "??? 0xAB" },
        { 0xAC, "Set gravity flags on entity" },
        { 0xAD, "Check any entity in relativeAABB_RefIdPair" },
        { 0xAE, "Exit() - Fatal Error" },
        { 0xAF, "Set fade transition with color" },
        { 0xB0, "Set player position and warp" },
        { 0xB1, "??? 0xB1" },
        { 0xB2, "??? 0xB2" },
        { 0xB3, "??? 0xB3" },
        { 0xB4, "??? 0xB4" },
        { 0xB5, "??? 0xB5" },
        { 0xB6, "??? 0xB6" },
        { 0xB7, "??? 0xB7" },
        { 0xB8, "??? 0xB8" },
        { 0xB9, "Start cd streaming" },
        { 0xBA, "??? 0xBA" },
        { 0xBB, "??? 0xBB" },
        { 0xBC, "Increase player HPMax" },
        { 0xBD, "Play sound 2" },
        { 0xBE, "Play sound 2 (bis)" },
        { 0xBF, "Play sound effect with tone volume mix" },
        { 0xC0, "Set equipped weapon" },
        { 0xC1, "Set player flag &= 0xffffff7f" },
        { 0xC2, "Check something save" },
        { 0xC3, "Restore and initialize HP and MP" },
        { 0xC4, "Dialog with entity and name" },
        { 0xFF, "End script" },
    };

    public record CommandProperties(byte Code, int Size, string Name, string Description);

    private static string CreateDescription(byte code, string desc)
    {
        var handlers =  GetHandlerNameByCodes();
        if (handlers.TryGetValue(code, out var handlerName))
        {
            return $"{handlerName}(...) {desc}";
        }

        return desc;
    }

    public static readonly Dictionary<byte, CommandProperties> CommandPropertiesByCode = new()
    {
        {0x00, new(0x00, CommandSizeByCode[0x00], CommandNameByCode[0x00], CreateDescription(0x00, "")) },
        {0x01, new(0x01, CommandSizeByCode[0x01], CommandNameByCode[0x01], CreateDescription(0x01, "")) },
        {0x02, new(0x02, CommandSizeByCode[0x02], CommandNameByCode[0x02], CreateDescription(0x02, "")) },
        {0x03, new(0x03, CommandSizeByCode[0x03], CommandNameByCode[0x03], CreateDescription(0x03, "")) },
        {0x04, new(0x04, CommandSizeByCode[0x04], CommandNameByCode[0x04], CreateDescription(0x04, "")) },
        {0x05, new(0x05, CommandSizeByCode[0x05], CommandNameByCode[0x05], CreateDescription(0x05, "")) },
        {0x06, new(0x06, CommandSizeByCode[0x06], CommandNameByCode[0x06], CreateDescription(0x06, "")) },
        {0x07, new(0x07, CommandSizeByCode[0x07], CommandNameByCode[0x07], CreateDescription(0x07, "")) },
        {0x08, new(0x08, CommandSizeByCode[0x08], CommandNameByCode[0x08], CreateDescription(0x08, "")) },
        {0x09, new(0x09, CommandSizeByCode[0x09], CommandNameByCode[0x09], CreateDescription(0x09, "")) },
        {0x0A, new(0x0A, CommandSizeByCode[0x0A], CommandNameByCode[0x0A], CreateDescription(0x0A, "switch direction, used for placing NPCs")) },
        {0x0B, new(0x0B, CommandSizeByCode[0x0B], CommandNameByCode[0x0B], CreateDescription(0x0B, "")) },
        {0x0C, new(0x0C, CommandSizeByCode[0x0C], CommandNameByCode[0x0C], CreateDescription(0x0C, "")) },
        {0x0D, new(0x0D, CommandSizeByCode[0x0D], CommandNameByCode[0x0D], CreateDescription(0x0D, "show dialog")) },
        {0x10, new(0x10, CommandSizeByCode[0x10], CommandNameByCode[0x10], CreateDescription(0x10, "")) },
        {0x11, new(0x11, CommandSizeByCode[0x11], CommandNameByCode[0x11], CreateDescription(0x11, "")) },
        {0x12, new(0x12, CommandSizeByCode[0x12], CommandNameByCode[0x12], CreateDescription(0x12, "only1 byte sound index")) },
        {0x15, new(0x15, CommandSizeByCode[0x15], CommandNameByCode[0x15], CreateDescription(0x15, "")) },
        {0x16, new(0x16, CommandSizeByCode[0x16], CommandNameByCode[0x16], CreateDescription(0x16, "fall as normal (bit0x100)")) },
        {0x17, new(0x17, CommandSizeByCode[0x17], CommandNameByCode[0x17], CreateDescription(0x17, "used for climbing ladders and flying")) },
        {0x18, new(0x18, CommandSizeByCode[0x18], CommandNameByCode[0x18], CreateDescription(0x18, "")) },
        {0x19, new(0x19, CommandSizeByCode[0x19], CommandNameByCode[0x19], CreateDescription(0x19, "")) },
        {0x1A, new(0x1A, CommandSizeByCode[0x1A], CommandNameByCode[0x1A], CreateDescription(0x1A, "")) },
        {0x1B, new(0x1B, CommandSizeByCode[0x1B], CommandNameByCode[0x1B], CreateDescription(0x1B, "stop flying0x0000; down0xff7f; forward & up0x0380")) },
        {0x1C, new(0x1C, CommandSizeByCode[0x1C], CommandNameByCode[0x1C], CreateDescription(0x1C, "")) },
        {0x1D, new(0x1D, CommandSizeByCode[0x1D], CommandNameByCode[0x1D], CreateDescription(0x1D, "")) },
        {0x1E, new(0x1E, CommandSizeByCode[0x1E], CommandNameByCode[0x1E], CreateDescription(0x1E, "collision blocks/pauses the walk")) },
        {0x1F, new(0x1F, CommandSizeByCode[0x1F], CommandNameByCode[0x1F], CreateDescription(0x1F, "collision ends the walk")) },
        {0x24, new(0x24, CommandSizeByCode[0x24], CommandNameByCode[0x24], CreateDescription(0x24, "waits until force adjust is >0")) },
        {0x25, new(0x25, CommandSizeByCode[0x25], CommandNameByCode[0x25], CreateDescription(0x25, "")) },
        {0x26, new(0x26, CommandSizeByCode[0x26], CommandNameByCode[0x26], CreateDescription(0x26, "")) },
        {0x27, new(0x27, CommandSizeByCode[0x27], CommandNameByCode[0x27], CreateDescription(0x27, "")) },
        {0x28, new(0x28, CommandSizeByCode[0x28], CommandNameByCode[0x28], CreateDescription(0x28, "bit0x8")) },
        {0x29, new(0x29, CommandSizeByCode[0x29], CommandNameByCode[0x29], CreateDescription(0x29, "bit0x8")) },
        {0x2A, new(0x2A, CommandSizeByCode[0x2A], CommandNameByCode[0x2A], CreateDescription(0x2A, "bit0x1")) },
        {0x2B, new(0x2B, CommandSizeByCode[0x2B], CommandNameByCode[0x2B], CreateDescription(0x2B, "bit0x1")) },
        {0x2D, new(0x2D, CommandSizeByCode[0x2D], CommandNameByCode[0x2D], CreateDescription(0x2D, "look into this event to study entity type")) },
        {0x2E, new(0x2E, CommandSizeByCode[0x2E], CommandNameByCode[0x2E], CreateDescription(0x2E, "")) },
        {0x2F, new(0x2F, CommandSizeByCode[0x2F], CommandNameByCode[0x2F], CreateDescription(0x2F, "")) },
        {0x30, new(0x30, CommandSizeByCode[0x30], CommandNameByCode[0x30], CreateDescription(0x30, "")) },
        {0x31, new(0x31, CommandSizeByCode[0x31], CommandNameByCode[0x31], CreateDescription(0x31, "")) },
        {0x32, new(0x32, CommandSizeByCode[0x32], CommandNameByCode[0x32], CreateDescription(0x32, "toggle bit on a flag")) },
        {0x33, new(0x33, CommandSizeByCode[0x33], CommandNameByCode[0x33], CreateDescription(0x33, "")) },
        {0x34, new(0x34, CommandSizeByCode[0x34], CommandNameByCode[0x34], CreateDescription(0x34, "")) },
        {0x35, new(0x35, CommandSizeByCode[0x35], CommandNameByCode[0x35], CreateDescription(0x35, "block until a flag is off")) },
        {0x36, new(0x36, CommandSizeByCode[0x36], CommandNameByCode[0x36], CreateDescription(0x36, "block until a flag is on")) },
        {0x37, new(0x37, CommandSizeByCode[0x37], CommandNameByCode[0x37], CreateDescription(0x37, "")) },
        {0x38, new(0x38, CommandSizeByCode[0x38], CommandNameByCode[0x38], CreateDescription(0x38, "")) },
        {0x39, new(0x39, CommandSizeByCode[0x39], CommandNameByCode[0x39], CreateDescription(0x39, "blocks until the dialog is finished")) },
        {0x3B, new(0x3B, CommandSizeByCode[0x3B], CommandNameByCode[0x3B], CreateDescription(0x3B, "")) },
        {0x40, new(0x40, CommandSizeByCode[0x40], CommandNameByCode[0x40], CreateDescription(0x40, "")) },
        {0x41, new(0x41, CommandSizeByCode[0x41], CommandNameByCode[0x41], CreateDescription(0x41, "")) },
        {0x44, new(0x44, CommandSizeByCode[0x44], CommandNameByCode[0x44], CreateDescription(0x44, "")) },
        {0x45, new(0x45, CommandSizeByCode[0x45], CommandNameByCode[0x45], CreateDescription(0x45, "bit0x2000")) },
        {0x46, new(0x46, CommandSizeByCode[0x46], CommandNameByCode[0x46], CreateDescription(0x46, "")) },
        {0x49, new(0x49, CommandSizeByCode[0x49], CommandNameByCode[0x49], CreateDescription(0x49, "seeks back to the beginning of event program")) },
        {0x4A, new(0x4A, CommandSizeByCode[0x4A], CommandNameByCode[0x4A], CreateDescription(0x4A, "")) },
        {0x4B, new(0x4B, CommandSizeByCode[0x4B], CommandNameByCode[0x4B], CreateDescription(0x4B, "")) },
        {0x4C, new(0x4C, CommandSizeByCode[0x4C], CommandNameByCode[0x4C], CreateDescription(0x4C, "*0x107200 = val")) },
        {0x4D, new(0x4D, CommandSizeByCode[0x4D], CommandNameByCode[0x4D], CreateDescription(0x4D, "*0x107204 = *0x107200 &0x4")) },
        {0x50, new(0x50, CommandSizeByCode[0x50], CommandNameByCode[0x50], CreateDescription(0x50, "")) },
        {0x51, new(0x51, CommandSizeByCode[0x51], CommandNameByCode[0x51], CreateDescription(0x51, "")) },
        {0x54, new(0x54, CommandSizeByCode[0x54], CommandNameByCode[0x54], CreateDescription(0x54, "")) },
        {0x55, new(0x55, CommandSizeByCode[0x55], CommandNameByCode[0x55], CreateDescription(0x55, "")) },
        {0x58, new(0x58, CommandSizeByCode[0x58], CommandNameByCode[0x58], CreateDescription(0x58, "")) },
        {0x59, new(0x59, CommandSizeByCode[0x59], CommandNameByCode[0x59], CreateDescription(0x59, "")) },
        {0x5A, new(0x5A, CommandSizeByCode[0x5A], CommandNameByCode[0x5A], CreateDescription(0x5A, "")) },
        {0x5B, new(0x5B, CommandSizeByCode[0x5B], CommandNameByCode[0x5B], CreateDescription(0x5B, "also has anim flag for on ground or climbing, etc")) },
        {0x5C, new(0x5C, CommandSizeByCode[0x5C], CommandNameByCode[0x5C], CreateDescription(0x5C, "")) },
        {0x62, new(0x62, CommandSizeByCode[0x62], CommandNameByCode[0x62], CreateDescription(0x62, "")) },
        {0x63, new(0x63, CommandSizeByCode[0x63], CommandNameByCode[0x63], CreateDescription(0x63, "")) },
        {0x64, new(0x64, CommandSizeByCode[0x64], CommandNameByCode[0x64], CreateDescription(0x64, "")) },
        {0x65, new(0x65, CommandSizeByCode[0x65], CommandNameByCode[0x65], CreateDescription(0x65, "")) },
        {0x67, new(0x67, CommandSizeByCode[0x67], CommandNameByCode[0x67], CreateDescription(0x67, "")) },
        {0x69, new(0x69, CommandSizeByCode[0x69], CommandNameByCode[0x69], CreateDescription(0x69, "")) },
        {0x70, new(0x70, CommandSizeByCode[0x70], CommandNameByCode[0x70], CreateDescription(0x70, "")) },
        {0x73, new(0x73, CommandSizeByCode[0x73], CommandNameByCode[0x73], CreateDescription(0x73, "")) },
        {0x74, new(0x74, CommandSizeByCode[0x74], CommandNameByCode[0x74], CreateDescription(0x74, "")) },
        {0x78, new(0x78, CommandSizeByCode[0x78], CommandNameByCode[0x78], CreateDescription(0x78, "")) },
        {0x85, new(0x85, CommandSizeByCode[0x85], CommandNameByCode[0x85], CreateDescription(0x85, "")) },
        {0x8B, new(0x8B, CommandSizeByCode[0x8B], CommandNameByCode[0x8B], CreateDescription(0x8B, "")) },
        {0x90, new(0x90, CommandSizeByCode[0x90], CommandNameByCode[0x90], CreateDescription(0x90, "")) },
        {0x91, new(0x91, CommandSizeByCode[0x91], CommandNameByCode[0x91], CreateDescription(0x91, "")) },
        {0x92, new(0x92, CommandSizeByCode[0x92], CommandNameByCode[0x92], CreateDescription(0x92, "")) },
        {0x93, new(0x93, CommandSizeByCode[0x93], CommandNameByCode[0x93], CreateDescription(0x93, "")) },
        {0x94, new(0x94, CommandSizeByCode[0x94], CommandNameByCode[0x94], CreateDescription(0x94, "")) },
        {0xA0, new(0xA0, CommandSizeByCode[0xA0], CommandNameByCode[0xA0], CreateDescription(0xA0, "")) },
        {0xA1, new(0xA1, CommandSizeByCode[0xA1], CommandNameByCode[0xA1], CreateDescription(0xA1, "")) },
        {0xA2, new(0xA2, CommandSizeByCode[0xA2], CommandNameByCode[0xA2], CreateDescription(0xA2, "")) },
        {0xA3, new(0xA3, CommandSizeByCode[0xA3], CommandNameByCode[0xA3], CreateDescription(0xA3, "")) },
        {0xA7, new(0xA7, CommandSizeByCode[0xA7], CommandNameByCode[0xA7], CreateDescription(0xA7, "")) },
        {0xAC, new(0xAC, CommandSizeByCode[0xAC], CommandNameByCode[0xAC], CreateDescription(0xAC, "")) },
        {0xBD, new(0xBD, CommandSizeByCode[0xBD], CommandNameByCode[0xBD], CreateDescription(0xBD, "")) },
        {0xC4, new(0xC4, CommandSizeByCode[0xC4], CommandNameByCode[0xC4], CreateDescription(0xC4, "")) },
        {0xFF, new(0xFF, CommandSizeByCode[0xFF], CommandNameByCode[0xFF], CreateDescription(0xFF, "")) },
    };
}