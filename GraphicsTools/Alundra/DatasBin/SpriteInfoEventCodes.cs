using Alundra.Gameplay;
using Alundra.Gameplay.Scripts;
using Alundra.Sprite;
using System.Diagnostics;

namespace Alundra.DatasBin;

public class SpriteInfoEventCodes
{
    //public static readonly byte[] Codes = new byte[1024 * 1024]; //1mb of event codes, too much prob but oh well;

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
        _dataSize = (header.EntitiesPointer == 0 ? header.EventCodesFPointer: header.EntitiesPointer) - header.EventCodesAPointer;
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
        ////half mb for global codes, half mb for map codes
    }

    public class SiCode
    {
        public byte Code { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
    }

    public static SiCode GetCode(byte b)
    {
        if (CommandSizeByCode.TryGetValue(b, out var size))
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
        //throw new ArgumentException($"Unknown command code: {b:X2}");
    }

    public List<SiCommand> GetCommands(BinaryReader br, int eventCodesOffset, bool stopAtff = false, int commandsSize = 0)
    {
        var commands = new List<SiCommand>();
        //var bytes = GetByteCode(br, sector1offset);
        br.BaseStream.Position = _binOffset + eventCodesOffset;
        var bytes = new byte[_dataSize - eventCodesOffset];
        br.Read(bytes, 0, bytes.Length);
        var i = 0;

        while (i < bytes.Length && (commandsSize == 0 || i < commandsSize))
        {
            var b = bytes[i++];

            var sicode = GetCode(b);
            var size = sicode.Size;
            var name = sicode.Name;
            var parameters = new byte[size - 1];
            var j = 0;

            while (j < size - 1)
            {
                parameters[j++] = bytes[i++];
            }

            SiCommand cmd;
            var addr = _memoryAddress + eventCodesOffset + i - size;

            switch (b)
            {
                case 0x1E:
                case 0x1F:
                    cmd = new WalkCommand(b, parameters, name, addr);
                    break;
                case 0x64:
                    cmd = new SetPositionCommand(b, parameters, name, addr);
                    break;
                case 0x05:
                case 0x06:
                    cmd = new SetFlagCommand(b, parameters, name, addr);
                    break;
                //case "if":
                //case "if not":
                //    cmd = new BranchCommand(b, 5, parameters, name, addr);
                //    break;
                case 0x03:
                case 0x04:
                    cmd = new BranchCommand(b, 3, parameters, name, addr);
                    break;
                case 0x02:
                    cmd = new JumpCommand(b, parameters, name, addr);
                    break;
                case 0x58:
                    cmd = new DirectionBranchCommand(b, parameters, name, addr);
                    break;
                default:
                    cmd = new SiCommand(b, size, parameters, name, addr);
                    break;
            }

            commands.Add(cmd);
            if (stopAtff && b == 0xff)
            {
                break;
            }
        }

        return commands;
    }

    public byte[] GetByteCode(BinaryReader br, int sector1Offset)
    {

        var bytes = new byte[_dataSize - sector1Offset];
        var i = 0;
        br.BaseStream.Position = _binOffset + sector1Offset;

        while (i < bytes.Length)
        {
            //Debug.Assert(dex < bytes.Length, "ByteCodes larger than 255");

            var b = br.ReadByte();
            if (b == 0) //what does 0 mean?
            {
                bytes[i++] = b;
            }
            else if (b == 0xff) //end
            {
                bytes[i++] = b;
                return bytes; //for now
            }
            else
            {
                bytes[i++] = b;
                //skip ahead by parameter length
            }
        }

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

    public static readonly Dictionary<byte, int> CommandSizeByCode = new()
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
        { 0x00, "break" },
        { 0x02, "goto" },
        { 0x03, "if false" },
        { 0x04, "while false" },
        { 0x05, "flag on" },
        { 0x06, "flag off" },
        { 0x07, "check entity in area" },
        { 0x08, "turn" },
        { 0x09, "set dir" },
        { 0x0A, "reverse" }, //switch direction, used for paceing npcs
        { 0x0B, "anim wait distance" },
        { 0x0C, "set direction with math" },
        { 0x0D, "dialog" }, //show dialog
        { 0x10, "lose control" },
        { 0x11, "gain control" },
        { 0x12, "play sound 1" }, //only 1 byte sound index
        { 0x15, "reset z pos" },
        { 0x16, "high gravity" }, //fall as normal  //bit 0x100
        { 0x17, "low gravity" }, //used for climbing ladders and flying
        { 0x19, "deactivate?" },
        { 0x1A, "set anim" },
        { 0x1B, "fly" }, //stop flying 0x0000   flying down 0xff7f     flying foward and up  0x0380
        { 0x1C, "wait anim ?" },
        { 0x1D, "wait anim 2" },
        { 0x1E, "walk" }, //collision blocks/pauses the walk
        { 0x1F, "walk 2" }, //collision ends the walk
        { 0x24, "wait force adjusted" }, //waits until force adjust is > 0
        { 0x25, "wait entity collision z or 144" },
        { 0x26, "wait force adjusted or entity collision z" },
        { 0x27, "face player" },
        { 0x28, "gravity flag 2 on" }, //bit 0x8
        { 0x29, "gravity flag 2 off" }, //bit 0x8
        { 0x2A, "gravity flag 3 on" }, //bit 0x1
        { 0x2B, "gravity flag 3 off" }, //bit 0x1
        { 0x2D, "activate entity" }, //look into this event to study entity type
        { 0x2E, "hide" },
        { 0x2F, "check moving in dir" },
        { 0x30, "if flag off" },
        { 0x31, "if flag on" },
        { 0x32, "toggle flag" }, //toggle bit on a flag
        { 0x33, "check flags on" },
        { 0x34, "check flags off" },
        { 0x35, "until flag off" }, //block until a flag is off
        { 0x36, "until flag on" },  //block until a flag is on
        { 0x37, "wait" },
        { 0x38, "register warp" },
        { 0x39, "wait for dialog" }, //blocks until the dialog is finished
        { 0x3B, "check player in area" },
        { 0x40, "set program index" },
        { 0x41, "set sprite program index" },
        { 0x44, "wait dialog choice" },
        { 0x45, "gravity flag 4 off" }, //bit 0x2000
        { 0x46, "gravity flag 4 on" },
        { 0x49, "restart" }, //seeks back to the beginning of event program
        { 0x4A, "if true restart" },
        { 0x4B, "if false restart" },
        { 0x4C, "set dialog something" }, //*0x107200 = val
        { 0x4D, "check dialog something" }, //*0x107204 = *0x107200 & 0x4
        { 0x50, "set dialog choice" },
        { 0x51, "get dialog choice" },
        { 0x54, "set walkable" },
        { 0x55, "set unwalkable" },
        { 0x58, "directional branch" },
        { 0x59, "set entity anim" },
        { 0x5A, "turn entity" },
        { 0x5B, "turn entity with anim" }, //also has anim flag for on ground or climbing, etc
        { 0x5C, "dialog with entity" },
        { 0x62, "set entity something?" },
        { 0x63, "set entity gravity" },
        { 0x64, "set entity position" },
        { 0x65, "move entity position" },
        { 0x67, "follow entity" },
        { 0x69, "" },
        { 0x70, "check IsAboveGround" },
        { 0x73, "" },
        { 0x74, "" },
        { 0x78, "" },
        { 0x85, "set map tiles" },
        { 0x8B, "spawn entity" },
        { 0x90, "create effect" },
        { 0x91, "disable effect" },
        { 0x92, "set effect anim" },
        { 0x93, "set effect pos" },
        { 0x94, "set effect forces" },
        { 0xA0, "adjusted effect pos" },
        { 0xA1, "set e effect pos with entity" },
        { 0xA2, "create effect with pos" },
        { 0xA3, "create effect with entity pos" },
        { 0xA7, "play music" },
        { 0xAC, "set gravity flags on entity" },
        { 0xBD, "play sound 2" }, //2 byte sound index
        { 0xC4, "dialog with entity and name" },
        { 0xFF, "end" },
    };
}