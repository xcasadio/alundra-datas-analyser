using AlundraEngine.Gameplay.Scripts;
using static AlundraEngine.Gameplay.Scripts.EntityEventHandlers;
namespace AlundraEngine.DatasBin;

public class SpriteInfoEventCodes
{
    //public readonly byte[] Codes = new byte[1024 * 1024]; //1mb of event codes, too much prob but oh well;

    public SpriteInfoEventCodes(BinaryReader br, long binOffset, SpriteInfoHeader header, bool ismap)
    {
        var tableSize = 0;
        short firstoffset = 0;

        //read sector1a
        br.BaseStream.Position = binOffset + header.EventCodesAPointer;
        tableSize = header.EventCodesASize / 2;
        EventCodesATable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesATable[i] = br.ReadInt16();
            if (firstoffset == 0 && EventCodesATable[i] != 0)
            {
                firstoffset = EventCodesATable[i];
            }
        }

        //read sector1b
        br.BaseStream.Position = binOffset + header.EventCodesBPointer;
        tableSize = header.EventCodesBSize / 2;
        EventCodesBTable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesBTable[i] = br.ReadInt16();
            if (firstoffset == 0 && EventCodesBTable[i] != 0)
            {
                firstoffset = EventCodesBTable[i];
            }
        }

        //read sector1c
        br.BaseStream.Position = binOffset + header.EventCodesCPointer;
        tableSize = header.EventCodesCSize / 2;
        EventCodesCTable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesCTable[i] = br.ReadInt16();
            if (firstoffset == 0 && EventCodesCTable[i] != 0)
            {
                firstoffset = EventCodesCTable[i];
            }
        }

        //read sector1d
        br.BaseStream.Position = binOffset + header.EventCodesDPointer;
        tableSize = header.EventCodesDSize / 2;
        EventCodesDTable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesDTable[i] = br.ReadInt16();
            if (firstoffset == 0 && EventCodesDTable[i] != 0)
            {
                firstoffset = EventCodesDTable[i];
            }
        }

        //read sector1e
        br.BaseStream.Position = binOffset + header.EventCodesEPointer;
        tableSize = header.EventCodesESize / 2;
        EventCodesETable = new short[tableSize];
        for (var i = 0; i < tableSize; i++)
        {
            EventCodesETable[i] = br.ReadInt16();
            if (firstoffset == 0 && EventCodesETable[i] != 0)
            {
                firstoffset = EventCodesETable[i];
            }
        }

        //read sector1f
        header.EventCodesFSize = header.EventCodesAPointer + firstoffset - header.EventCodesFPointer;
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
        _dataSize = (header.EntitiesPointer == 0 ? header.EventCodesFPointer : header.EntitiesPointer) -
                    header.EventCodesAPointer;
        //Debug.Assert(_dataSize > 0);

        //Preload all commands
        //optim: only load index used by the current map
        //CommandsByTypes.Add(ScriptHelper.ProgramALoad, new List<SiCommand>());
        //CommandsByTypes.Add(ScriptHelper.ProgramBMap, new List<SiCommand>());
        //CommandsByTypes.Add(ScriptHelper.ProgramCTick, new List<SiCommand>());
        //CommandsByTypes.Add(ScriptHelper.ProgramDTouch, new List<SiCommand>());
        //CommandsByTypes.Add(ScriptHelper.ProgramEDeactivate, new List<SiCommand>());
        //CommandsByTypes.Add(ScriptHelper.ProgramFInteract, new List<SiCommand>());
        //
        //foreach (var index in EventCodesATable)
        //{
        //    var commands = GetCommands(br, index, true);
        //    CommandsByTypes[ScriptHelper.ProgramALoad].AddRange(commands);
        //}
        //
        //foreach (var index in EventCodesBTable)
        //{
        //    var commands = GetCommands(br, index, true);
        //    CommandsByTypes[ScriptHelper.ProgramBMap].AddRange(commands);
        //}
        //
        //foreach (var index in EventCodesCTable)
        //{
        //    var commands = GetCommands(br, index, true);
        //    CommandsByTypes[ScriptHelper.ProgramCTick].AddRange(commands);
        //}
        //
        //foreach (var index in EventCodesDTable)
        //{
        //    var commands = GetCommands(br, index, true);
        //    CommandsByTypes[ScriptHelper.ProgramDTouch].AddRange(commands);
        //}
        //
        //foreach (var index in EventCodesETable)
        //{
        //    var commands = GetCommands(br, index, true);
        //    CommandsByTypes[ScriptHelper.ProgramEDeactivate].AddRange(commands);
        //}
        //
        ////TODO bug with F codes
        ////foreach (var index in EventCodesFTable)
        ////{
        ////    var commands = GetCommands(br, index, true);
        ////    CommandsByTypes[ScriptHelper.ProgramFInteract].AddRange(commands);
        ////}
        ////
        //foreach (var commandsByType in CommandsByTypes)
        //{
        //    Debug.WriteLine($"Code type {commandsByType.Key}");
        //
        //    foreach (var siCommand in commandsByType.Value)
        //    {
        //        Debug.WriteLine($"  {siCommand.Print(2, commandsByType.Value)}");
        //    }
        //}

        ////remove this ??  =>
        //var top = 0;
        //if (ismap)
        //{
        //    top += 1024 * 512;
        //}
        //
        //br.BaseStream.Position = binOffset;
        //if (_dataSize > 0)
        //{
        //    br.Read(Codes, top, _dataSize);
        //}
        //half mb for global codes, half mb for map codes
    }

    public class SiCode
    {
        public byte Code { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
    }

    public static SiCode GetCode(byte b)
    {
        if (CommandSizeByCodes.TryGetValue(b, out var size))
        {
            return new SiCode
            {
                Code = b,
                Size = size,
                Name = CommandNameByCode.GetValueOrDefault(b, "")
            };
        }

        return new SiCode
        {
            Code = b,
            Size = 1,
            Name = ""
        };
        //throw new ArgumentException($"DepthSortValue command code: {b:Width}");
    }

    public List<SiCommand> GetCommands(BinaryReader br, int eventCodesOffset, bool stopAtff = false,
        int commandsSize = 0)
    {
        var commands = new List<SiCommand>();
        //var bytes = GetByteCode(br, sector1offset);
        br.BaseStream.Position = _binOffset + eventCodesOffset;
        var bytes = new byte[_dataSize - eventCodesOffset];
        br.Read(bytes, 0, bytes.Length);
        var i = 0;

        while (i < bytes.Length && (commandsSize == 0 || i < commandsSize))
        {
            var value = bytes[i++];

            var sicode = GetCode(value);
            var size = sicode.Size;
            var name = sicode.Name;
            var parameters = new byte[size - 1];
            var j = 0;

            while (j < size - 1)
            {
                parameters[j++] = bytes[i++];
            }

            SiCommand cmd;
            var address = _memoryAddress + eventCodesOffset + i - size;
            cmd = new SiCommand(value, parameters, name, address);
            commands.Add(cmd);
            if (stopAtff && value == 0xff)
            {
                break;
            }
        }

        return commands;
    }

    public byte[] GetByteCode(BinaryReader br, int sectorOffset)
    {

        var bytes = new byte[_dataSize - sectorOffset];
        var i = 0;
        br.BaseStream.Position = _binOffset + sectorOffset;

        br.Read(bytes, 0, bytes.Length);

        //while (i < bytes.Length)
        //{
        //    //Debug.Assert(dex < bytes.Length, "ByteCodes larger than 255");
        //
        //    var b = br.ReadByte();
        //    if (b == 0) //what does 0 mean?
        //    {
        //        bytes[i++] = b;
        //    }
        //    else if (b == 0xff) //end
        //    {
        //        bytes[i++] = b;
        //        return bytes; //for now
        //    }
        //    else
        //    {
        //        bytes[i++] = b;
        //        //skip ahead by parameter length
        //    }
        //}

        return bytes;
    }

    private readonly long _binOffset;
    private readonly int _dataSize;
    private readonly int _memoryAddress;
    public readonly short[] EventCodesATable;
    public readonly short[] EventCodesBTable;
    public readonly short[] EventCodesCTable;
    public readonly short[] EventCodesDTable;
    public readonly short[] EventCodesETable;
    public readonly short[] EventCodesFTable;
    //public readonly Dictionary<int, List<SiCommand>> CommandsByTypes = new();

    public static readonly Dictionary<byte, int> CommandSizeByCodes = new()
    {
        { 0x00, 1 },
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
        { 0x10, 1 },
        { 0x11, 1 },
        { 0x12, 2 },
        { 0x15, 1 },
        { 0x16, 1 },
        { 0x17, 1 },
        { 0x19, 1 },
        { 0x1A, 2 },
        { 0x1B, 3 },
        { 0x1C, 2 },
        { 0x1D, 2 },
        { 0x1E, 3 },
        { 0x1F, 3 },
        { 0x24, 1 },
        { 0x25, 1 },
        { 0x26, 1 },
        { 0x27, 1 },
        { 0x28, 1 },
        { 0x29, 1 },
        { 0x2A, 1 },
        { 0x2B, 1 },
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
        { 0x3B, 7 },
        { 0x40, 3 },
        { 0x41, 3 },
        { 0x44, 1 },
        { 0x45, 1 },
        { 0x46, 1 },
        { 0x49, 1 },
        { 0x4A, 1 },
        { 0x4B, 1 },
        { 0x4C, 2 },
        { 0x4D, 1 },
        { 0x50, 2 },
        { 0x51, 1 },
        { 0x54, 5 },
        { 0x55, 5 },
        { 0x58, 9 },
        { 0x59, 3 },
        { 0x5A, 3 },
        { 0x5B, 4 },
        { 0x5C, 4 },
        { 0x62, 4 },
        { 0x63, 4 },
        { 0x64, 8 },
        { 0x65, 8 },
        { 0x67, 2 },
        { 0x69, 7 },
        { 0x70, 1 },
        { 0x73, 2 },
        { 0x74, 3 },
        { 0x78, 3 },
        { 0x85, 7 },
        { 0x8B, 9 },
        { 0x90, 2 },
        { 0x91, 2 },
        { 0x92, 3 },
        { 0x93, 8 },
        { 0x94, 8 },
        { 0xA0, 8 },
        { 0xA1, 9 },
        { 0xA2, 8 },
        { 0xA3, 9 },
        { 0xA7, 3 },
        { 0xAC, 4 },
        { 0xBD, 3 },
        { 0xC4, 6 },
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
        { 0x0A, "Reverse direction" }, //switch direction, used for placeing npcs
        { 0x0B, "Anim wait distance" },
        { 0x0C, "Set random dir" },
        { 0x0D, "Dialog" }, //show dialog
        { 0x10, "Player lose control" },
        { 0x11, "Player gain control" },
        { 0x12, "Play sound 1" }, //only 1 byte sound index
        { 0x15, "Reset z pos" },
        { 0x16, "High gravity" }, //fall as normal  //bit 0x100
        { 0x17, "Low gravity" }, //used for climbing ladders and flying
        { 0x19, "Deactivate entity" },
        { 0x1A, "Set anim" },
        { 0x1B, "Fly" }, //stop flying 0x0000   flying down 0xff7f     flying foward and up  0x0380
        { 0x1C, "Repeat anim" },
        { 0x1D, "Repeat anim with collision" },
        { 0x1E, "Walk" }, //collision blocks/pauses the walk
        { 0x1F, "Walk with collision" }, //collision ends the walk
        { 0x24, "Wait force adjusted" }, //waits until force adjust is > 0
        { 0x25, "Wait entity collision z or 144" },
        { 0x26, "Wait force adjusted or entity collision z" },
        { 0x27, "Face player" },
        { 0x28, "Gravity flag 2 on" }, //bit 0x8
        { 0x29, "Gravity flag 2 off" }, //bit 0x8
        { 0x2A, "Gravity flag 3 on" }, //bit 0x1
        { 0x2B, "Gravity flag 3 off" }, //bit 0x1
        { 0x2D, "Activate entity" }, //look into this event to study entity type
        { 0x2E, "Hide" },
        { 0x2F, "Check moving in dir" },
        { 0x30, "If flag on" },
        { 0x31, "If flag off" },
        { 0x32, "Toggle flag" }, //toggle bit on a flag
        { 0x33, "Check flags on" },
        { 0x34, "Check flags off" },
        { 0x35, "Until flag off" }, //block until a flag is off
        { 0x36, "Until flag on" }, //block until a flag is on
        { 0x37, "Wait" },
        { 0x38, "Register warp" },
        { 0x39, "Wait for dialog" }, //blocks until the dialog is finished
        { 0x3B, "Check player in area" },
        { 0x40, "Set program index" },
        { 0x41, "Set sprite program index" },
        { 0x44, "Wait dialog choice" },
        { 0x45, "Gravity flag 4 off" }, //bit 0x2000
        { 0x46, "Gravity flag 4 on" },
        { 0x49, "Restart" }, //seeks back to the beginning of event program
        { 0x4A, "If true restart" },
        { 0x4B, "If false restart" },
        { 0x4C, "Set dialog something" }, //*0x107200 = val
        { 0x4D, "Check dialog something" }, //*0x107204 = *0x107200 & 0x4
        { 0x50, "Set dialog choice" },
        { 0x51, "Get dialog choice" },
        { 0x54, "Set walkable" },
        { 0x55, "Set unwalkable" },
        { 0x58, "Directional branch" },
        { 0x59, "Set entity anim" },
        { 0x5A, "Turn entity" },
        { 0x5B, "Turn entity with anim" }, //also has anim flag for on ground or climbing, etc
        { 0x5C, "Dialog with entity" },
        { 0x62, "Set entities flags" },
        { 0x63, "Set entities gravity" },
        { 0x64, "Set entities position" },
        { 0x65, "Move entity position" },
        { 0x67, "Follow entity" },
        { 0x69, "Camera look at" },
        { 0x70, "Check IsAboveGround" },
        { 0x73, "" },
        { 0x74, "" },
        { 0x78, "" },
        { 0x85, "Set map tiles" },
        { 0x8B, "Spawn entity" },
        { 0x90, "Create effect" },
        { 0x91, "Disable effect" },
        { 0x92, "Set effect anim" },
        { 0x93, "Set effect pos" },
        { 0x94, "Set effect forces" },
        { 0xA0, "Adjusted effect pos" },
        { 0xA1, "Set effect pos with entity" },
        { 0xA2, "Create effect with pos" },
        { 0xA3, "Create effect with entity pos" },
        { 0xA7, "Play music" },
        { 0xAC, "Set gravity flags on entity" },
        { 0xBD, "Play sound 2" }, //2 byte sound index
        { 0xC4, "Dialog with entity and name" },
        { 0xFF, "End" },
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
        {0x00, new(0x00, CommandSizeByCodes[0x00], CommandNameByCode[0x00], CreateDescription(0x00, "")) },
        {0x01, new(0x01,1, CommandNameByCode[0x01], CreateDescription(0x01, "")) },
        {0x02, new(0x02, CommandSizeByCodes[0x02], CommandNameByCode[0x02], CreateDescription(0x02, "")) },
        {0x03, new(0x03, CommandSizeByCodes[0x03], CommandNameByCode[0x03], CreateDescription(0x03, "")) },
        {0x04, new(0x04, CommandSizeByCodes[0x04], CommandNameByCode[0x04], CreateDescription(0x04, "")) },
        {0x05, new(0x05, CommandSizeByCodes[0x05], CommandNameByCode[0x05], CreateDescription(0x05, "")) },
        {0x06, new(0x06, CommandSizeByCodes[0x06], CommandNameByCode[0x06], CreateDescription(0x06, "")) },
        {0x07, new(0x07, CommandSizeByCodes[0x07], CommandNameByCode[0x07], CreateDescription(0x07, "")) },
        {0x08, new(0x08, CommandSizeByCodes[0x08], CommandNameByCode[0x08], CreateDescription(0x08, "")) },
        {0x09, new(0x09, CommandSizeByCodes[0x09], CommandNameByCode[0x09], CreateDescription(0x09, "")) },
        {0x0A, new(0x0A, CommandSizeByCodes[0x0A], CommandNameByCode[0x0A], CreateDescription(0x0A, "switch direction, used for placing NPCs")) },
        {0x0B, new(0x0B, CommandSizeByCodes[0x0B], CommandNameByCode[0x0B], CreateDescription(0x0B, "")) },
        {0x0C, new(0x0C, CommandSizeByCodes[0x0C], CommandNameByCode[0x0C], CreateDescription(0x0C, "")) },
        {0x0D, new(0x0D, CommandSizeByCodes[0x0D], CommandNameByCode[0x0D], CreateDescription(0x0D, "show dialog")) },
        {0x10, new(0x10, CommandSizeByCodes[0x10], CommandNameByCode[0x10], CreateDescription(0x10, "")) },
        {0x11, new(0x11, CommandSizeByCodes[0x11], CommandNameByCode[0x11], CreateDescription(0x11, "")) },
        {0x12, new(0x12, CommandSizeByCodes[0x12], CommandNameByCode[0x12], CreateDescription(0x12, "only1 byte sound index")) },
        {0x15, new(0x15, CommandSizeByCodes[0x15], CommandNameByCode[0x15], CreateDescription(0x15, "")) },
        {0x16, new(0x16, CommandSizeByCodes[0x16], CommandNameByCode[0x16], CreateDescription(0x16, "fall as normal (bit0x100)")) },
        {0x17, new(0x17, CommandSizeByCodes[0x17], CommandNameByCode[0x17], CreateDescription(0x17, "used for climbing ladders and flying")) },
        {0x19, new(0x19, CommandSizeByCodes[0x19], CommandNameByCode[0x19], CreateDescription(0x19, "")) },
        {0x1A, new(0x1A, CommandSizeByCodes[0x1A], CommandNameByCode[0x1A], CreateDescription(0x1A, "")) },
        {0x1B, new(0x1B, CommandSizeByCodes[0x1B], CommandNameByCode[0x1B], CreateDescription(0x1B, "stop flying0x0000; down0xff7f; forward & up0x0380")) },
        {0x1C, new(0x1C, CommandSizeByCodes[0x1C], CommandNameByCode[0x1C], CreateDescription(0x1C, "")) },
        {0x1D, new(0x1D, CommandSizeByCodes[0x1D], CommandNameByCode[0x1D], CreateDescription(0x1D, "")) },
        {0x1E, new(0x1E, CommandSizeByCodes[0x1E], CommandNameByCode[0x1E], CreateDescription(0x1E, "collision blocks/pauses the walk")) },
        {0x1F, new(0x1F, CommandSizeByCodes[0x1F], CommandNameByCode[0x1F], CreateDescription(0x1F, "collision ends the walk")) },
        {0x24, new(0x24, CommandSizeByCodes[0x24], CommandNameByCode[0x24], CreateDescription(0x24, "waits until force adjust is >0")) },
        {0x25, new(0x25, CommandSizeByCodes[0x25], CommandNameByCode[0x25], CreateDescription(0x25, "")) },
        {0x26, new(0x26, CommandSizeByCodes[0x26], CommandNameByCode[0x26], CreateDescription(0x26, "")) },
        {0x27, new(0x27, CommandSizeByCodes[0x27], CommandNameByCode[0x27], CreateDescription(0x27, "")) },
        {0x28, new(0x28, CommandSizeByCodes[0x28], CommandNameByCode[0x28], CreateDescription(0x28, "bit0x8")) },
        {0x29, new(0x29, CommandSizeByCodes[0x29], CommandNameByCode[0x29], CreateDescription(0x29, "bit0x8")) },
        {0x2A, new(0x2A, CommandSizeByCodes[0x2A], CommandNameByCode[0x2A], CreateDescription(0x2A, "bit0x1")) },
        {0x2B, new(0x2B, CommandSizeByCodes[0x2B], CommandNameByCode[0x2B], CreateDescription(0x2B, "bit0x1")) },
        {0x2D, new(0x2D, CommandSizeByCodes[0x2D], CommandNameByCode[0x2D], CreateDescription(0x2D, "look into this event to study entity type")) },
        {0x2E, new(0x2E, CommandSizeByCodes[0x2E], CommandNameByCode[0x2E], CreateDescription(0x2E, "")) },
        {0x2F, new(0x2F, CommandSizeByCodes[0x2F], CommandNameByCode[0x2F], CreateDescription(0x2F, "")) },
        {0x30, new(0x30, CommandSizeByCodes[0x30], CommandNameByCode[0x30], CreateDescription(0x30, "")) },
        {0x31, new(0x31, CommandSizeByCodes[0x31], CommandNameByCode[0x31], CreateDescription(0x31, "")) },
        {0x32, new(0x32, CommandSizeByCodes[0x32], CommandNameByCode[0x32], CreateDescription(0x32, "toggle bit on a flag")) },
        {0x33, new(0x33, CommandSizeByCodes[0x33], CommandNameByCode[0x33], CreateDescription(0x33, "")) },
        {0x34, new(0x34, CommandSizeByCodes[0x34], CommandNameByCode[0x34], CreateDescription(0x34, "")) },
        {0x35, new(0x35, CommandSizeByCodes[0x35], CommandNameByCode[0x35], CreateDescription(0x35, "block until a flag is off")) },
        {0x36, new(0x36, CommandSizeByCodes[0x36], CommandNameByCode[0x36], CreateDescription(0x36, "block until a flag is on")) },
        {0x37, new(0x37, CommandSizeByCodes[0x37], CommandNameByCode[0x37], CreateDescription(0x37, "")) },
        {0x38, new(0x38, CommandSizeByCodes[0x38], CommandNameByCode[0x38], CreateDescription(0x38, "")) },
        {0x39, new(0x39, CommandSizeByCodes[0x39], CommandNameByCode[0x39], CreateDescription(0x39, "blocks until the dialog is finished")) },
        {0x3B, new(0x3B, CommandSizeByCodes[0x3B], CommandNameByCode[0x3B], CreateDescription(0x3B, "")) },
        {0x40, new(0x40, CommandSizeByCodes[0x40], CommandNameByCode[0x40], CreateDescription(0x40, "")) },
        {0x41, new(0x41, CommandSizeByCodes[0x41], CommandNameByCode[0x41], CreateDescription(0x41, "")) },
        {0x44, new(0x44, CommandSizeByCodes[0x44], CommandNameByCode[0x44], CreateDescription(0x44, "")) },
        {0x45, new(0x45, CommandSizeByCodes[0x45], CommandNameByCode[0x45], CreateDescription(0x45, "bit0x2000")) },
        {0x46, new(0x46, CommandSizeByCodes[0x46], CommandNameByCode[0x46], CreateDescription(0x46, "")) },
        {0x49, new(0x49, CommandSizeByCodes[0x49], CommandNameByCode[0x49], CreateDescription(0x49, "seeks back to the beginning of event program")) },
        {0x4A, new(0x4A, CommandSizeByCodes[0x4A], CommandNameByCode[0x4A], CreateDescription(0x4A, "")) },
        {0x4B, new(0x4B, CommandSizeByCodes[0x4B], CommandNameByCode[0x4B], CreateDescription(0x4B, "")) },
        {0x4C, new(0x4C, CommandSizeByCodes[0x4C], CommandNameByCode[0x4C], CreateDescription(0x4C, "*0x107200 = val")) },
        {0x4D, new(0x4D, CommandSizeByCodes[0x4D], CommandNameByCode[0x4D], CreateDescription(0x4D, "*0x107204 = *0x107200 &0x4")) },
        {0x50, new(0x50, CommandSizeByCodes[0x50], CommandNameByCode[0x50], CreateDescription(0x50, "")) },
        {0x51, new(0x51, CommandSizeByCodes[0x51], CommandNameByCode[0x51], CreateDescription(0x51, "")) },
        {0x54, new(0x54, CommandSizeByCodes[0x54], CommandNameByCode[0x54], CreateDescription(0x54, "")) },
        {0x55, new(0x55, CommandSizeByCodes[0x55], CommandNameByCode[0x55], CreateDescription(0x55, "")) },
        {0x58, new(0x58, CommandSizeByCodes[0x58], CommandNameByCode[0x58], CreateDescription(0x58, "")) },
        {0x59, new(0x59, CommandSizeByCodes[0x59], CommandNameByCode[0x59], CreateDescription(0x59, "")) },
        {0x5A, new(0x5A, CommandSizeByCodes[0x5A], CommandNameByCode[0x5A], CreateDescription(0x5A, "")) },
        {0x5B, new(0x5B, CommandSizeByCodes[0x5B], CommandNameByCode[0x5B], CreateDescription(0x5B, "also has anim flag for on ground or climbing, etc")) },
        {0x5C, new(0x5C, CommandSizeByCodes[0x5C], CommandNameByCode[0x5C], CreateDescription(0x5C, "")) },
        {0x62, new(0x62, CommandSizeByCodes[0x62], CommandNameByCode[0x62], CreateDescription(0x62, "")) },
        {0x63, new(0x63, CommandSizeByCodes[0x63], CommandNameByCode[0x63], CreateDescription(0x63, "")) },
        {0x64, new(0x64, CommandSizeByCodes[0x64], CommandNameByCode[0x64], CreateDescription(0x64, "")) },
        {0x65, new(0x65, CommandSizeByCodes[0x65], CommandNameByCode[0x65], CreateDescription(0x65, "")) },
        {0x67, new(0x67, CommandSizeByCodes[0x67], CommandNameByCode[0x67], CreateDescription(0x67, "")) },
        {0x69, new(0x69, CommandSizeByCodes[0x69], CommandNameByCode[0x69], CreateDescription(0x69, "")) },
        {0x70, new(0x70, CommandSizeByCodes[0x70], CommandNameByCode[0x70], CreateDescription(0x70, "")) },
        {0x73, new(0x73, CommandSizeByCodes[0x73], CommandNameByCode[0x73], CreateDescription(0x73, "")) },
        {0x74, new(0x74, CommandSizeByCodes[0x74], CommandNameByCode[0x74], CreateDescription(0x74, "")) },
        {0x78, new(0x78, CommandSizeByCodes[0x78], CommandNameByCode[0x78], CreateDescription(0x78, "")) },
        {0x85, new(0x85, CommandSizeByCodes[0x85], CommandNameByCode[0x85], CreateDescription(0x85, "")) },
        {0x8B, new(0x8B, CommandSizeByCodes[0x8B], CommandNameByCode[0x8B], CreateDescription(0x8B, "")) },
        {0x90, new(0x90, CommandSizeByCodes[0x90], CommandNameByCode[0x90], CreateDescription(0x90, "")) },
        {0x91, new(0x91, CommandSizeByCodes[0x91], CommandNameByCode[0x91], CreateDescription(0x91, "")) },
        {0x92, new(0x92, CommandSizeByCodes[0x92], CommandNameByCode[0x92], CreateDescription(0x92, "")) },
        {0x93, new(0x93, CommandSizeByCodes[0x93], CommandNameByCode[0x93], CreateDescription(0x93, "")) },
        {0x94, new(0x94, CommandSizeByCodes[0x94], CommandNameByCode[0x94], CreateDescription(0x94, "")) },
        {0xA0, new(0xA0, CommandSizeByCodes[0xA0], CommandNameByCode[0xA0], CreateDescription(0xA0, "")) },
        {0xA1, new(0xA1, CommandSizeByCodes[0xA1], CommandNameByCode[0xA1], CreateDescription(0xA1, "")) },
        {0xA2, new(0xA2, CommandSizeByCodes[0xA2], CommandNameByCode[0xA2], CreateDescription(0xA2, "")) },
        {0xA3, new(0xA3, CommandSizeByCodes[0xA3], CommandNameByCode[0xA3], CreateDescription(0xA3, "")) },
        {0xA7, new(0xA7, CommandSizeByCodes[0xA7], CommandNameByCode[0xA7], CreateDescription(0xA7, "")) },
        {0xAC, new(0xAC, CommandSizeByCodes[0xAC], CommandNameByCode[0xAC], CreateDescription(0xAC, "")) },
        {0xBD, new(0xBD, CommandSizeByCodes[0xBD], CommandNameByCode[0xBD], CreateDescription(0xBD, "2 byte sound index")) },
        {0xC4, new(0xC4, CommandSizeByCodes[0xC4], CommandNameByCode[0xC4], CreateDescription(0xC4, "")) },
        {0xFF, new(0xFF, CommandSizeByCodes[0xFF], CommandNameByCode[0xFF], CreateDescription(0xFF, "")) },
    };
}