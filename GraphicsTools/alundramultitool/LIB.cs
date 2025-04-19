using System.Text;

namespace GraphicsTools.LIB
{
    public class Lib
    {
        string[] _hlines;
        public Lib(string path, string hpath, List<string> otherdefs)
        {
            if (File.Exists(hpath))
            {
                _hlines = File.ReadAllLines(hpath);
            }
            else
            {
                _hlines = otherdefs.ToArray();
            }
            Name = Path.GetFileNameWithoutExtension(path);
            using (var br = new BinaryReader(File.OpenRead(path)))
            {
                Header = new LibHeader(br);
                while(br.BaseStream.Position + 8 < br.BaseStream.Length)
                {
                    var module = new LibModule(br, this);
                    if (module.Link != null)
                    {
                        Modules.Add(module);
                    }
                }
            }

            if (_hlines!=null)
            {
                foreach(var symb in Modules.SelectMany(x=>x.Link.Symbols.Where(x2=>x2.Type == SymbolType.Internal)))
                {
                    foreach(var hline in _hlines)
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(hline, @"\s" + symb.Name + @"\s?\("))
                        {
                            ExportedFunctions.Add(symb.Mod.Header.ModuleName + " : " + symb.Name);
                            break;
                        }
                    }
                }
            }
            
        }
        public override string ToString()
        {
            return Name;
        }
        public string Name;
        public LibHeader Header;
        public List<LibModule> Modules = new();

        public List<string> ExportedFunctions = new();
    }

    public class LibHeader
    {
        public LibHeader(BinaryReader br)
        {
            Signature = br.ReadString(3);
            Version = br.ReadByte();
        }
        public string Signature;//3 bytes
        public byte Version;
    }
    public class LibModule
    {
        public Lib Lib;
        int _baseoffset;
        public LibModule(BinaryReader br, Lib lib)
        {
            Lib = lib;
            _baseoffset = (int)br.BaseStream.Position;
            Header = new ModuleHeader(br);
            br.BaseStream.Position = _baseoffset + Header.LinkOffset;
            try
            {
                Link = new Link(br, _baseoffset + Header.NextOffset, this);
            }
            catch(Exception ex)
            {
                //failing reading module
            }
            
            br.BaseStream.Position = _baseoffset + Header.NextOffset;
        }
        public void Rerun(BinaryReader br)
        {
            br.BaseStream.Position = _baseoffset;
            Header = new ModuleHeader(br);
            br.BaseStream.Position = _baseoffset + Header.LinkOffset;
            Link = new Link(br, _baseoffset + Header.NextOffset, this);
            br.BaseStream.Position = _baseoffset + Header.NextOffset;
        }
        public ModuleHeader Header;
        public Link Link;

        public override string ToString()
        {
            return Header.ModuleName?.Trim();
        }
    }
    public class ModuleHeader
    {
        public ModuleHeader(BinaryReader br)
        {
            ModuleName = br.ReadString(8).Trim();
            Date = br.ReadInt32();
            LinkOffset = br.ReadInt32();
            NextOffset = br.ReadInt32();
        }
        public string ModuleName;//8 bytes
        public int Date;
        public int LinkOffset;
        public int NextOffset;
        
    }

    public class Link
    {
        public LibModule Mod;
        public StringBuilder ActivityLog = new();
        void Log(string text)
        {
            ActivityLog.Append(text);
        }
        //void Log(byte state, string text)
        //{
        //    ActivityLog.Add(state.ToString() + " : " + text);
        //}
        public Link(BinaryReader br, int endpos, LibModule mod)
        {
            Mod = mod;
            var sig = br.ReadString(3);
            int version = br.ReadByte();
            Section curSection = null;
            var offsetadjust = 0;
            while(br.BaseStream.Position < endpos)
            {
                var byt = br.ReadByte();
                var state = (StateType)byt;
                Log($"{byt.ToString()}({state.ToString()}) : ");
                switch(state)
                {
                    case StateType.Eof:
                        return;
                    case StateType.Code:
                        {
                            offsetadjust = curSection.Code.Length;
                            //add code to existing section code
                            int size = br.ReadUInt16();
                            var code = new byte[curSection.Code.Length + size];
                            curSection.Code.CopyTo(code, 0);
                            br.Read(code, curSection.Code.Length, size);
                            curSection.Code = code;
                        }
                        break;
                    case StateType.Switch:
                        {
                            int dex = br.ReadUInt16();
                            curSection = Sections.FirstOrDefault(x=>x.Symbol == dex);
                            Log($"switch to section {curSection.Symbol.ToString("x")}");
                        }
                        break;
                    case StateType.BssAlloc:
                        {
                            var size = br.ReadInt32();
                            curSection.BssSize = size;
                            curSection.RealBssSize += size;
                        }
                        break;
                    case StateType.Patch:
                        {
                            var patch = new Patch(br, offsetadjust);
                            curSection.Patches.Add(patch);
                            Log(patch.ActivityLog.ToString());
                        }
                        break;
                    case StateType.Def:
                        {
                            var symb = new Symbol(br, SymbolType.Internal, mod);
                            Symbols.Add(symb);
                            Log($"symbol number {symb.Sym.ToString("x")} '{symb.Name}' at offset {symb.Offset.ToString("x")} in section {symb.Section.ToString("x")}");
                        }
                        break;
                    case StateType.Ref:
                        {
                            var symb = new Symbol(br, SymbolType.External, mod);
                            Symbols.Add(symb);
                            Log($"symbol number {symb.Sym.ToString("x")} '{symb.Name}'");
                        }
                        break;
                    case StateType.Section:
                        curSection = new Section(br);
                        Sections.Add(curSection);
                        Log($"section symbol number {curSection.Symbol.ToString("x")} '{curSection.Name}' in group {curSection.Group} alignment {curSection.Alignment}");
                        break;
                    case StateType.Local:
                        {
                            var symb = new Symbol(br, SymbolType.Local, mod);
                            Symbols.Add(symb);
                            Log($"'{symb.Name}' at offset {symb.Offset.ToString("x")} in section {symb.Section.ToString("x")}");
                        }
                        break;
                    case StateType.File:
                        {
                            var symbol = br.ReadUInt16();
                            var str = br.ReadString(-1);
                            //what are these for?
                        }
                        break;
                    case StateType.Processor:
                        {
                            var type = br.ReadByte();
                            //what are these for?
                        }
                        break;
                    case StateType.Bss:
                        {
                            var symb = new Symbol(br, SymbolType.Bss, mod);
                            Symbols.Add(symb);
                            Sections.FirstOrDefault(x => x.Symbol == symb.Section).RealBssSize += symb.Size;
                        }
                        break;    
                }
                Log("\r\n");
            }
        }

        public List<Section> Sections = new();
        public List<Symbol> Symbols = new();
    }

    public class Section
    {
        public Section(BinaryReader br)
        {
            Symbol = br.ReadUInt16();
            Group = br.ReadUInt16();
            Alignment = br.ReadByte();
            Name = br.ReadString(-1);
        }
        public byte[] Code = new byte[0];
        public int BssSize;//what is this for
        public int RealBssSize;

        public ushort Symbol;
        public ushort Group;
        public byte Alignment;
        public string Name;

        public List<Patch> Patches = new();

        public override string ToString()
        {
            return Name;
        }
    }

    public class Patch
    {
        public void Patch2(BinaryReader br)
        {
            var type = br.ReadByte();
            switch(type)
            {
                case (byte)RelocType.WordLiteral:
                case (byte)RelocType.FunctionCall:
                case (byte)RelocType.UpperImmediate:
                case (byte)RelocType.LowerImmediate:
                    RelocType = (RelocType)type;
                    break;
                default:
                    throw new Exception("bad patch");
            }
            Offset = br.ReadUInt16();

            var node = new PatchNode(br);
            if (node.Type == PatchType.Expr && node.Left.Type == PatchType.Value)
            {
                if (node.Right.Type == PatchType.SectionBase)
                {
                    //swap left and right.  why?
                    var tmp = node.Left;
                    node.Left = node.Right;
                    node.Right = tmp;
                }
                else if(node.Right.Type != PatchType.Ref)
                {
                    node = node.Right;
                }
            }

            switch(node.Type)
            {
                case PatchType.Expr:
                    switch(node.Left.Type)
                    {
                        case PatchType.SectionBase:
                            PatchType = node.Left.Type;
                            Symbol = node.Left.Symbol;
                            Value = node.Right.Value;
                            break;
                        case PatchType.SectionStart:
                            PatchType = PatchType.SectionSize;
                            Symbol = node.Left.Symbol;
                            break;
                        case PatchType.Value:
                            PatchType = PatchType.Ref;
                            Symbol = node.Right.Symbol;
                            break;
                        default:
                            throw new Exception("bad patch");
                    }
                    break;
                case PatchType.Ref:
                case PatchType.SectionBase:
                case PatchType.SectionStart:
                case PatchType.SectionEnd:
                    PatchType = node.Type;
                    Symbol = node.Symbol;
                    break;
                default:
                    throw new Exception("bad patch");
            }
        }
        public StringBuilder ActivityLog = new();
        void Log(string text)
        {
            ActivityLog.Append(text);
        }
        public Patch(BinaryReader br, int offsetadjust)
        {
            var type = br.ReadByte();
            
            switch (type)
            {
                case (byte)RelocType.WordLiteral:
                case (byte)RelocType.FunctionCall:
                case (byte)RelocType.UpperImmediate:
                case (byte)RelocType.LowerImmediate:
                    RelocType = (RelocType)type;
                    break;
                default:
                    throw new Exception("bad patch");
            }
            Offset = (ushort)(br.ReadUInt16() + offsetadjust);

            Log("Patch type " + type + " at offset " + Offset.ToString("x") + " with ");
            var next = br.ReadByte();
            Log($"({next.ToString()}) ");
            br.BaseStream.Position--;

            var node = new PatchNode(br);
            if (node.Type == PatchType.Expr && node.Left.Type == PatchType.Value)
            {
                if (node.Right.Type == PatchType.SectionBase)
                {
                    //swap left and right.  why?
                    var tmp = node.Left;
                    node.Left = node.Right;
                    node.Right = tmp;
                }
                else if (node.Right.Type != PatchType.Ref)
                {
                    node = node.Right;
                }
            }

            switch (node.Type)
            {
                case PatchType.Expr:
                    switch (node.Left.Type)
                    {
                        case PatchType.SectionBase:
                            PatchType = node.Left.Type;
                            Symbol = node.Left.Symbol;
                            Value = node.Right.Value;
                            break;
                        case PatchType.SectionStart:
                            PatchType = PatchType.SectionSize;
                            Symbol = node.Left.Symbol;
                            break;
                        case PatchType.Value:
                            PatchType = PatchType.Ref;
                            Symbol = node.Right.Symbol;
                            break;
                        default:
                            throw new Exception("bad patch");
                    }
                    break;
                case PatchType.Ref:
                case PatchType.SectionBase:
                case PatchType.SectionStart:
                case PatchType.SectionEnd:
                    PatchType = node.Type;
                    Symbol = node.Symbol;
                    break;
                default:
                    throw new Exception("bad patch");
            }

            Log($"{PatchType} {Symbol.ToString("x")} {Value.ToString("x")}");
        }
        public uint Value;
        public ushort Offset;
        public ushort Symbol;
        public RelocType RelocType;
        public PatchType PatchType;

        public override string ToString()
        {
            return $"{PatchType}:{RelocType}";
        }
    }

    public class PatchNode
    {
        public PatchNode(BinaryReader br)
        {
            var state = br.ReadByte();
            if (state<=24)
            {
                switch(state)
                {
                    case 0:
                    case (byte)PatchType.Value:
                        Value = br.ReadUInt32();
                        Type = PatchType.Value;
                        break;
                    case (byte)PatchType.Ref:
                    case (byte)PatchType.SectionBase:
                    case (byte)PatchType.SectionStart:
                    case (byte)PatchType.SectionEnd:
                        Symbol = br.ReadUInt16();
                        Type = (PatchType)state;
                        break;
                }
            }
            else
            {
                Type = PatchType.Expr;
                switch(state)
                {
                    case (byte)PatchOp.Add:
                    case (byte)PatchOp.Sub:
                    case (byte)PatchOp.Div:
                    case (byte)PatchOp.Exc:
                        break;
                    default:
                        throw new Exception("bad patch");
                }

                Op = (PatchOp)state;

                Left = new PatchNode(br);
                Right = new PatchNode(br);
            }
        }
        public PatchNode Left;
        public PatchNode Right;
        public uint Value;
        public ushort Symbol;
        public PatchType Type;
        public PatchOp Op;
    }

    public enum PatchType
    {
        Ref=2,
        SectionBase=4,
        SectionStart = 12,
        SectionEnd = 22,
        Value = 44,
        Expr = 45,
        SectionSize = 46
    }
    public enum RelocType
    {
        WordLiteral = 16,
        FunctionCall = 74,
        UpperImmediate = 82,
        LowerImmediate = 84
    }

    public class Symbol
    {
        public Symbol(BinaryReader br, SymbolType type, LibModule mod)
        {
            Mod = mod;
            Type = type;
            switch(type)
            {
                case SymbolType.Internal:
                    Sym = br.ReadUInt16();
                    Section = br.ReadUInt16();
                    Offset = br.ReadInt32();
                    Name = br.ReadString(-1);
                    break;
                case SymbolType.External:
                    Sym = br.ReadUInt16();
                    Name = br.ReadString(-1);
                    break;
                case SymbolType.Bss:
                    Sym = br.ReadUInt16();
                    Section = br.ReadUInt16();
                    Size = br.ReadInt32();
                    Name = br.ReadString(-1);
                    break;
                case SymbolType.Local:
                    Section = br.ReadUInt16();
                    Offset = br.ReadInt32();
                    Name = br.ReadString(-1);
                    break;
            }
            
        }
        public LibModule Mod;
        public SymbolType Type;
        public ushort Sym;
        public ushort Section;
        public int Offset;
        public int Size;//used for bss symbol
        public string Name;

        public override string ToString()
        {
            return Name;
        }
    }

    public enum SymbolType
    {
        Internal,
        External,
        Local,
        Bss
    }

    public enum StateType
    {
        Eof = 0,//
        Code = 2,//
        Switch = 6,//
        BssAlloc = 8,
        Patch = 10,
        Def=12,//
        Ref=14,//
        Section=16,//
        Local=18,//
        File=28,
        Processor=46,
        Bss=48//
    }

    public enum PatchOp
    {
        Add = 44,
        Sub = 46,
        Div = 50,
        Exc = 54
    }

    

    public static class Helper
    {
        public static string ReadString(this BinaryReader br, int length)
        {
            if (length == -1)
            {
                length = br.ReadByte();
            }

            var buff = new byte[length];
            br.Read(buff, 0, length);
            var sb = new StringBuilder();
            foreach(var b in buff)
            {
                if (b == 0)
                {
                    break;
                }

                sb.Append((char)b);
            }
            return sb.ToString();
        }
        /*public static string ReadString(BinaryReader br)
        {
            int length = br.ReadByte();
            return ReadString(br, length);
        }*/
    }
}
