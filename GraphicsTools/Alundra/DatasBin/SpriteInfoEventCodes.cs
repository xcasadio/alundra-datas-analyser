using System.Diagnostics;

namespace Alundra.DatasBin;

public class SpriteInfoEventCodes
{
    public static readonly byte[] Code = new byte[1024 * 1024];//1mb of event codes, too much prob but oh well;

    public SpriteInfoEventCodes(BinaryReader br, long binoffset, SpriteInfoHeader header, bool ismap)
    {
        var tableSize = 0;
        short firstoffset = 0;

        //read sector1a
        br.BaseStream.Position = binoffset + header.EventCodesAPointer;
        tableSize = header.EventCodesASize / 2;
        Eventcodesatable = new short[tableSize];
        for (var dex = 0; dex < tableSize; dex++)
        {
            Eventcodesatable[dex] = br.ReadInt16();
            if (firstoffset == 0 && Eventcodesatable[dex] != 0)
            {
                firstoffset = Eventcodesatable[dex];
            }
        }

        //read sector1b
        br.BaseStream.Position = binoffset + header.EventCodesBPointer;
        tableSize = header.EventCodesBSize / 2;
        Eventcodesbtable = new short[tableSize];
        for (var dex = 0; dex < tableSize; dex++)
        {
            Eventcodesbtable[dex] = br.ReadInt16();
            if (firstoffset == 0 && Eventcodesbtable[dex] != 0)
            {
                firstoffset = Eventcodesbtable[dex];
            }
        }

        //read sector1c
        br.BaseStream.Position = binoffset + header.EventCodesCPointer;
        tableSize = header.EventCodesCSize / 2;
        Eventcodesctable = new short[tableSize];
        for (var dex = 0; dex < tableSize; dex++)
        {
            Eventcodesctable[dex] = br.ReadInt16();
            if (firstoffset == 0 && Eventcodesctable[dex] != 0)
            {
                firstoffset = Eventcodesctable[dex];
            }
        }

        //read sector1d
        br.BaseStream.Position = binoffset + header.EventCodesDPointer;
        tableSize = header.EventCodesDSize / 2;
        Eventcodesdtable = new short[tableSize];
        for (var dex = 0; dex < tableSize; dex++)
        {
            Eventcodesdtable[dex] = br.ReadInt16();
            if (firstoffset == 0 && Eventcodesdtable[dex] != 0)
            {
                firstoffset = Eventcodesdtable[dex];
            }
        }

        //read sector1e
        br.BaseStream.Position = binoffset + header.EventCodesEPointer;
        tableSize = header.EventCodesESize / 2;
        Eventcodesetable = new short[tableSize];
        for (var dex = 0; dex < tableSize; dex++)
        {
            Eventcodesetable[dex] = br.ReadInt16();
            if (firstoffset == 0 && Eventcodesetable[dex] != 0)
            {
                firstoffset = Eventcodesetable[dex];
            }
        }

        //read sector1f
        header.EventCodesFSize = header.EventCodesAPointer + firstoffset - header.EventCodesFPointer;
        br.BaseStream.Position = binoffset + header.EventCodesFPointer;
        tableSize = header.EventCodesFSize / 2;
        if (tableSize < 0)
        {
            tableSize = 16;
        }

        Eventcodesftable = new short[tableSize];
        for (var dex = 0; dex < tableSize; dex++)
        {
            Eventcodesftable[dex] = br.ReadInt16();
        }

        //set binoffset for eventcodes
        _binoffset = binoffset + header.EventCodesAPointer;
        _memaddr = header.MemoryAddress + header.EventCodesAPointer;
        _datasize = header.EntitiesPointer - header.EventCodesAPointer;

        EventCodesTable.Add(Eventcodesatable);
        EventCodesTable.Add(Eventcodesbtable);
        EventCodesTable.Add(Eventcodesctable);
        EventCodesTable.Add(Eventcodesdtable);
        EventCodesTable.Add(Eventcodesetable);
        EventCodesTable.Add(Eventcodesftable);

        var top = 0;
        if (ismap)
        {
            top += 1024 * 512;
        }

        br.BaseStream.Position = binoffset;
        if (_datasize > 0)
        {
            br.Read(Code, top, _datasize);
        }
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
        var size = 1;
        var name = "";
        switch (b)
        {
            case 0x02:
                name = "goto";//skips forward or back
                size = 3;
                break;
            case 0x03:
                name = "iffalse";
                size = 3;
                break;
            case 0x04:
                name = "whilefalse";//loops back until condition met
                size = 3;
                break;
            case 0x05:
                name = "flagon";//turn on a logic switch
                size = 3;
                break;
            case 0x06:
                name = "flagoff";//turn off a logic switch
                size = 3;
                break;
            case 0x07:
                name = "checkentityinarea";
                size = 8;
                break;
            case 0x08:
                name = "turn";//turn direction
                size = 2;
                break;
            case 0x09:
                name = "setdir";//set direction
                size = 2;
                break;
            case 0x0a:
                name = "reverse";//switch direction, used for paceing npcs
                size = 1;
                break;
            case 0x0b:
                name = "animwaitdistance";
                size = 4;
                break;
            case 0x0c:
                name = "setdirectionwithmath";
                size = 1;
                break;
            case 0x0d:
                name = "dialog";//show dialog
                size = 3;
                break;
            case 0x10:
                name = "losecontrol";
                size = 1;
                break;
            case 0x11:
                name = "gaincontrol";
                size = 1;
                break;
            case 0x12:
                name = "playsound1";//only 1 byte sound index
                size = 2;
                break;
            case 0x15:
                name = "resetzpos";
                size = 1;
                break;
            case 0x16:
                name = "highgravity";//fall as normal  //bit 0x100
                size = 1;
                break;
            case 0x17:
                name = "lowgravity";//used for climbing ladders and flying
                size = 1;
                break;

            case 0x19:
                name = "deactivate?";
                size = 1;
                break;
            case 0x1a:
                name = "setanim";//set animation
                size = 2;
                break;

            //CURRENT IMPLEMENTATION PROGRESS
            case 0x1b:
                name = "fly"; //stop flying 0x0000   flying down 0xff7f     flying foward and up  0x0380
                size = 3;
                break;
            case 0x1c:
                name = "waitanim?";
                size = 2;
                break;
            case 0x1d:
                name = "waitanim2";
                size = 2;
                break;
            case 0x1e:
                name = "walk";//collision blocks/pauses the walk
                size = 3;
                break;
            case 0x1f:
                name = "walk2";//collision ends the walk
                size = 3;
                break;
            case 0x24:
                name = "waitforceadjusted";//waits until force adjust is > 0
                size = 1;
                break;
            case 0x25:
                name = "waitentitycollisionzor144";
                size = 1;
                break;
            case 0x26:
                name = "waitforceadjustedorentitycollisionz";
                size = 1;
                break;
            case 0x27:
                name = "faceplayer";
                size = 1;
                break;
            case 0x28:
                name = "gravityflag2on";//bit 0x8
                size = 1;
                break;
            case 0x29:
                name = "gravityflag2off";//bit 0x8
                size = 1;
                break;
            case 0x2a:
                name = "gravityflag3on";//bit 0x1
                size = 1;
                break;
            case 0x2b:
                name = "gravityflag3off";//bit 0x1
                size = 1;
                break;
            case 0x2d:
                name = "activateentity";//look into this event to study entity type
                size = 2;
                break;
            case 0x2e:
                name = "hide";
                size = 2;
                break;
            case 0x2f:
                name = "checkmovingindir";
                size = 4;
                break;
            case 0x30:
                name = "ifflagoff";//if a logic switch is on
                size = 5;
                break;
            case 0x31:
                name = "ifflagon";//if a logic switch is not on
                size = 5;
                break;
            case 0x32:
                name = "toggleflag";//toggle bit on a flag
                size = 3;
                break;
            case 0x33:
                name = "checkflagson";
                size = 9;
                break;
            case 0x34:
                name = "checkflagsoff";
                size = 9;
                break;
            case 0x35:
                name = "untilflagoff";//block until a flag is off
                size = 3;
                break;
            case 0x36:
                name = "untilflagon";//block until a flag is on
                size = 3;
                break;
            case 0x37:
                name = "wait";//waits for the specified time to pass
                size = 2;
                break;
            case 0x38:
                name = "registersomething?";
                size = 5;
                break;
            case 0x39:
                name = "waitfordaialog";//blocks until the dialog is finished
                size = 1;
                break;

            case 0x3b:
                name = "checkplayerinarea";
                size = 7;
                break;
            case 0x40:
                name = "setprogramindex";
                size = 3;
                break;
            case 0x41:
                name = "setspriteprogramindex";
                size = 3;
                break;
            case 0x44:
                name = "waitdialogchoice";
                size = 1;
                break;
            case 0x45:
                name = "gravityflag4off";//bit 0x2000
                size = 1;
                break;
            case 0x46:
                name = "gravityflag4on";//bit 0x2000
                size = 1;
                break;
            case 0x49:
                name = "restart";//seeks back to the beginning of event program
                size = 1;
                break;
            case 0x4a:
                name = "iftruerestart";
                size = 1;
                break;
            case 0x4b:
                name = "iffalserestart";
                size = 1;
                break;
            case 0x4c:
                name = "setdialogsoemthing";//*0x107200 = val
                size = 2;
                break;
            case 0x4d:
                name = "checkdialogsomething";//*0x107204 = *0x107200 & 0x4
                size = 1;
                break;
            case 0x50:
                name = "setdialogchoice";
                size = 2;
                break;
            case 0x51:
                name = "getdialogchoice";
                size = 1;
                break;

            case 0x54:
                name = "setwalkable";
                size = 5;
                break;
            case 0x55:
                name = "setunwalkable";
                size = 5;
                break;
            case 0x58:
                name = "directionalbranch";
                size = 9;
                break;
            case 0x59:
                name = "setentityanim";
                size = 3;
                break;
            case 0x5a:
                name = "turnentity";
                size = 3;
                break;
            case 0x5b:
                name = "turnentitywithanim";//also has anim flag for on ground or climbing, etc
                size = 4;
                break;
            case 0x5c:
                name = "dialogwithentity";
                size = 4;
                break;

            case 0x62:
                name = "setentitysomething?";
                size = 4;
                break;
            case 0x63:
                name = "setentitygravity";
                size = 4;
                break;
            case 0x64:
                name = "setentityposition";
                size = 8;
                break;
            case 0x65:
                name = "moveentityposition";
                size = 8;
                break;

            case 0x67:
                name = "followentity";
                size = 2;
                break;

            case 0x70:
                name = "checksomething?";
                size = 1;
                break;

            case 0x85:
                name = "setmaptiles";
                size = 7;
                break;

            case 0x8b:
                name = "spawnentity";//pulls entity to ones self and activates it at pixel offset
                size = 9;
                break;
            case 0x90:
                name = "createeffect";
                size = 2;
                break;
            case 0x91:
                name = "disableeffect";
                size = 2;
                break;
            case 0x92:
                name = "seteffectanim";
                size = 3;
                break;
            case 0x93:
                name = "seteeffectpos";
                size = 8;
                break;
            case 0x94:
                name = "seteeffectforces";
                size = 8;
                break;
            case 0xa0:
                name = "adjusteeffectpos";
                size = 8;
                break;
            case 0xa1:
                name = "seteeffectposwithentity";
                size = 9;
                break;
            case 0xa2:
                name = "createeffectwithpos";
                size = 8;
                break;
            case 0xa3:
                name = "createeffectwithentitypos";
                size = 9;
                break;
            case 0xa7:
                name = "playmusic";
                size = 3;
                break;
            case 0xac:
                name = "setgravityflagsonentity";
                size = 4;
                break;
            case 0xbd:
                name = "playsound2";//2 byte sound index
                size = 3;
                break;

            case 0xc4:
                name = "dialogwithentityandname";
                size = 6;
                break;

            //non-operations
            case 0x00:
                name = "break";
                size = 1;
                break;
            case 0xff:
                name = "end";
                size = 1;
                break;

            //unknowns, just get the size down
            case 0x69:
                size = 7;
                break;
            case 0x73:
                size = 2;
                break;
            case 0x74:
                size = 3;
                break;
            case 0x78:
                size = 3;
                break;
            case 0x98:
                size = 3;
                break;

            default:
                Debug.Print("Unknown code " + b.ToString("x2"));
                break;
        }

        return new SiCode { Code = b, Size = size, Name = name };
    }
    public List<SiCommand> GetCommands(BinaryReader br, int eventcodesoffset, bool stopatff = false, int comandssize = 0)
    {
        var commands = new List<SiCommand>();
        //var bytes = GetByteCode(br, sector1offset);
        br.BaseStream.Position = _binoffset + eventcodesoffset;
        var bytes = new byte[_datasize - eventcodesoffset];
        br.Read(bytes, 0, bytes.Length);
        var dex = 0;
        while (dex < bytes.Length && (comandssize == 0 || dex < comandssize))
        {
            var b = bytes[dex++];

            var sicode = GetCode(b);
            var size = sicode.Size;
            var name = sicode.Name;
            var parms = new byte[size - 1];
            var pdex = 0;
            while (pdex < size - 1)
                parms[pdex++] = bytes[dex++];
            SiCommand cmd;
            var addr = _memaddr + eventcodesoffset + dex - size;
            switch (name)
            {
                case "walk":
                case "walk2":
                    cmd = new WalkCommand(b, parms, name, addr);
                    break;
                case "setentityposition":
                    cmd = new SetPositionCommand(b, parms, name, addr);
                    break;
                case "flagon":
                case "flagoff":
                    cmd = new SetFlagCommand(b, parms, name, addr);
                    break;
                case "if":
                case "ifnot":
                    cmd = new BranchCommand(b, 5, parms, name, addr);
                    break;
                case "ifno":
                case "whilefalse":
                    cmd = new BranchCommand(b, 3, parms, name, addr);
                    break;
                case "goto":
                    cmd = new JumpCommand(b, parms, name, addr);
                    break;
                case "directionalbranch":
                    cmd = new DirectionBranchCommand(b, parms, name, addr);
                    break;
                default:
                    cmd = new SiCommand(b, size, parms, name, addr);
                    break;
            }

            commands.Add(cmd);
            if (stopatff && b == 0xff)
            {
                break;
            }
        }

        return commands;
    }

    public byte[] GetByteCode(BinaryReader br, int sector1Offset)
    {

        var bytes = new byte[_datasize - sector1Offset];
        var dex = 0;
        br.BaseStream.Position = _binoffset + sector1Offset;

        while (dex < bytes.Length)
        {
            //Debug.Assert(dex < bytes.Length, "ByteCodes larger than 255");

            var b = br.ReadByte();
            if (b == 0)//what does 0 mean?
            {
                bytes[dex++] = b;
            }
            else if (b == 0xff)//end
            {
                bytes[dex++] = b;
                return bytes;//for now
            }
            else
            {
                bytes[dex++] = b;
                //skip ahead by parameter length
            }
        }
        return bytes;
    }

    private readonly long _binoffset;
    private readonly int _datasize;
    private readonly int _memaddr;
    public readonly short[] Eventcodesatable;
    public readonly short[] Eventcodesbtable;
    public readonly short[] Eventcodesctable;
    public readonly short[] Eventcodesdtable;
    public readonly short[] Eventcodesetable;
    public readonly short[] Eventcodesftable;

    public readonly List<short[]> EventCodesTable = new();
}