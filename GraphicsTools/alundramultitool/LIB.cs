using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GraphicsTools.LIB
{
    public class LIB
    {
        string[] hlines;
        public LIB(string path, string hpath, List<string> otherdefs)
        {
            if (File.Exists(hpath))
                hlines = File.ReadAllLines(hpath);
            else
            {
                hlines = otherdefs.ToArray();
            }
            name = Path.GetFileNameWithoutExtension(path);
            using (var br = new BinaryReader(File.OpenRead(path)))
            {
                header = new LIB_header(br);
                while(br.BaseStream.Position + 8 < br.BaseStream.Length)
                {
                    var module = new Lib_Module(br, this);
                    if (module.Link != null)
                        modules.Add(module);
                }
            }

            if (hlines!=null)
            {
                foreach(var symb in modules.SelectMany(x=>x.Link.symbols.Where(x2=>x2.Type == SYMBOL_TYPE.INTERNAL)))
                {
                    foreach(var hline in hlines)
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(hline, @"\s" + symb.Name + @"\s?\("))
                        {
                            ExportedFunctions.Add(symb.Mod.header.ModuleName + " : " + symb.Name);
                            break;
                        }
                    }
                }
            }
            
        }
        public override string ToString()
        {
            return name;
        }
        public string name;
        public LIB_header header;
        public List<Lib_Module> modules = new List<Lib_Module>();

        public List<string> ExportedFunctions = new List<string>();
    }

    public class LIB_header
    {
        public LIB_header(BinaryReader br)
        {
            Signature = br.ReadString(3);
            version = br.ReadByte();
        }
        public string Signature;//3 bytes
        public byte version;
    }
    public class Lib_Module
    {
        public LIB lib;
        int baseoffset;
        public Lib_Module(BinaryReader br, LIB lib)
        {
            this.lib = lib;
            baseoffset = (int)br.BaseStream.Position;
            header = new ModuleHeader(br);
            br.BaseStream.Position = baseoffset + header.LinkOffset;
            try
            {
                Link = new Link(br, baseoffset + header.NextOffset, this);
            }
            catch(Exception ex)
            {
                //failing reading module
            }
            
            br.BaseStream.Position = baseoffset + header.NextOffset;
        }
        public void Rerun(BinaryReader br)
        {
            br.BaseStream.Position = baseoffset;
            header = new ModuleHeader(br);
            br.BaseStream.Position = baseoffset + header.LinkOffset;
            Link = new Link(br, baseoffset + header.NextOffset, this);
            br.BaseStream.Position = baseoffset + header.NextOffset;
        }
        public ModuleHeader header;
        public Link Link;

        public override string ToString()
        {
            return header.ModuleName?.Trim();
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
        public Lib_Module Mod;
        public StringBuilder ActivityLog = new StringBuilder();
        void Log(string text)
        {
            ActivityLog.Append(text);
        }
        //void Log(byte state, string text)
        //{
        //    ActivityLog.Add(state.ToString() + " : " + text);
        //}
        public Link(BinaryReader br, int endpos, Lib_Module mod)
        {
            this.Mod = mod;
            string sig = br.ReadString(3);
            int version = br.ReadByte();
            Section curSection = null;
            int offsetadjust = 0;
            while(br.BaseStream.Position < endpos)
            {
                var byt = br.ReadByte();
                var state = (STATE_TYPE)byt;
                Log($"{byt.ToString()}({state.ToString()}) : ");
                switch(state)
                {
                    case STATE_TYPE.EOF:
                        return;
                    case STATE_TYPE.CODE:
                        {
                            offsetadjust = curSection.Code.Length;
                            //add code to existing section code
                            int size = br.ReadUInt16();
                            byte[] code = new byte[curSection.Code.Length + size];
                            curSection.Code.CopyTo(code, 0);
                            br.Read(code, curSection.Code.Length, size);
                            curSection.Code = code;
                        }
                        break;
                    case STATE_TYPE.SWITCH:
                        {
                            int dex = br.ReadUInt16();
                            curSection = sections.FirstOrDefault(x=>x.Symbol == dex);
                            Log($"switch to section {curSection.Symbol.ToString("x")}");
                        }
                        break;
                    case STATE_TYPE.BSS_ALLOC:
                        {
                            int size = br.ReadInt32();
                            curSection.BssSize = size;
                            curSection.RealBssSize += size;
                        }
                        break;
                    case STATE_TYPE.PATCH:
                        {
                            var patch = new Patch(br, offsetadjust);
                            curSection.patches.Add(patch);
                            Log(patch.ActivityLog.ToString());
                        }
                        break;
                    case STATE_TYPE.DEF:
                        {
                            var symb = new Symbol(br, SYMBOL_TYPE.INTERNAL, mod);
                            symbols.Add(symb);
                            Log($"symbol number {symb.Sym.ToString("x")} '{symb.Name}' at offset {symb.Offset.ToString("x")} in section {symb.Section.ToString("x")}");
                        }
                        break;
                    case STATE_TYPE.REF:
                        {
                            var symb = new Symbol(br, SYMBOL_TYPE.EXTERNAL, mod);
                            symbols.Add(symb);
                            Log($"symbol number {symb.Sym.ToString("x")} '{symb.Name}'");
                        }
                        break;
                    case STATE_TYPE.SECTION:
                        curSection = new Section(br);
                        sections.Add(curSection);
                        Log($"section symbol number {curSection.Symbol.ToString("x")} '{curSection.Name}' in group {curSection.Group} alignment {curSection.Alignment}");
                        break;
                    case STATE_TYPE.LOCAL:
                        {
                            var symb = new Symbol(br, SYMBOL_TYPE.LOCAL, mod);
                            symbols.Add(symb);
                            Log($"'{symb.Name}' at offset {symb.Offset.ToString("x")} in section {symb.Section.ToString("x")}");
                        }
                        break;
                    case STATE_TYPE.FILE:
                        {
                            var symbol = br.ReadUInt16();
                            var str = br.ReadString(-1);
                            //what are these for?
                        }
                        break;
                    case STATE_TYPE.PROCESSOR:
                        {
                            var type = br.ReadByte();
                            //what are these for?
                        }
                        break;
                    case STATE_TYPE.BSS:
                        {
                            var symb = new Symbol(br, SYMBOL_TYPE.BSS, mod);
                            symbols.Add(symb);
                            sections.FirstOrDefault(x => x.Symbol == symb.Section).RealBssSize += symb.Size;
                        }
                        break;    
                }
                Log("\r\n");
            }
        }

        public List<Section> sections = new List<Section>();
        public List<Symbol> symbols = new List<Symbol>();
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

        public List<Patch> patches = new List<Patch>();

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
                case (byte)RELOC_TYPE.WORD_LITERAL:
                case (byte)RELOC_TYPE.FUNCTION_CALL:
                case (byte)RELOC_TYPE.UPPER_IMMEDIATE:
                case (byte)RELOC_TYPE.LOWER_IMMEDIATE:
                    RelocType = (RELOC_TYPE)type;
                    break;
                default:
                    throw new Exception("bad patch");
            }
            Offset = br.ReadUInt16();

            var node = new PatchNode(br);
            if (node.Type == PATCH_TYPE.EXPR && node.Left.Type == PATCH_TYPE.VALUE)
            {
                if (node.Right.Type == PATCH_TYPE.SECTION_BASE)
                {
                    //swap left and right.  why?
                    var tmp = node.Left;
                    node.Left = node.Right;
                    node.Right = tmp;
                }
                else if(node.Right.Type != PATCH_TYPE.REF)
                {
                    node = node.Right;
                }
            }

            switch(node.Type)
            {
                case PATCH_TYPE.EXPR:
                    switch(node.Left.Type)
                    {
                        case PATCH_TYPE.SECTION_BASE:
                            PatchType = node.Left.Type;
                            Symbol = node.Left.Symbol;
                            Value = node.Right.Value;
                            break;
                        case PATCH_TYPE.SECTION_START:
                            PatchType = PATCH_TYPE.SECTION_SIZE;
                            Symbol = node.Left.Symbol;
                            break;
                        case PATCH_TYPE.VALUE:
                            PatchType = PATCH_TYPE.REF;
                            Symbol = node.Right.Symbol;
                            break;
                        default:
                            throw new Exception("bad patch");
                    }
                    break;
                case PATCH_TYPE.REF:
                case PATCH_TYPE.SECTION_BASE:
                case PATCH_TYPE.SECTION_START:
                case PATCH_TYPE.SECTION_END:
                    PatchType = node.Type;
                    Symbol = node.Symbol;
                    break;
                default:
                    throw new Exception("bad patch");
            }
        }
        public StringBuilder ActivityLog = new StringBuilder();
        void Log(string text)
        {
            ActivityLog.Append(text);
        }
        public Patch(BinaryReader br, int offsetadjust)
        {
            var type = br.ReadByte();
            
            switch (type)
            {
                case (byte)RELOC_TYPE.WORD_LITERAL:
                case (byte)RELOC_TYPE.FUNCTION_CALL:
                case (byte)RELOC_TYPE.UPPER_IMMEDIATE:
                case (byte)RELOC_TYPE.LOWER_IMMEDIATE:
                    RelocType = (RELOC_TYPE)type;
                    break;
                default:
                    throw new Exception("bad patch");
            }
            Offset = (ushort)(br.ReadUInt16() + offsetadjust);

            Log("Patch type " + type + " at offset " + Offset.ToString("x") + " with ");
            byte next = br.ReadByte();
            Log($"({next.ToString()}) ");
            br.BaseStream.Position--;

            var node = new PatchNode(br);
            if (node.Type == PATCH_TYPE.EXPR && node.Left.Type == PATCH_TYPE.VALUE)
            {
                if (node.Right.Type == PATCH_TYPE.SECTION_BASE)
                {
                    //swap left and right.  why?
                    var tmp = node.Left;
                    node.Left = node.Right;
                    node.Right = tmp;
                }
                else if (node.Right.Type != PATCH_TYPE.REF)
                {
                    node = node.Right;
                }
            }

            switch (node.Type)
            {
                case PATCH_TYPE.EXPR:
                    switch (node.Left.Type)
                    {
                        case PATCH_TYPE.SECTION_BASE:
                            PatchType = node.Left.Type;
                            Symbol = node.Left.Symbol;
                            Value = node.Right.Value;
                            break;
                        case PATCH_TYPE.SECTION_START:
                            PatchType = PATCH_TYPE.SECTION_SIZE;
                            Symbol = node.Left.Symbol;
                            break;
                        case PATCH_TYPE.VALUE:
                            PatchType = PATCH_TYPE.REF;
                            Symbol = node.Right.Symbol;
                            break;
                        default:
                            throw new Exception("bad patch");
                    }
                    break;
                case PATCH_TYPE.REF:
                case PATCH_TYPE.SECTION_BASE:
                case PATCH_TYPE.SECTION_START:
                case PATCH_TYPE.SECTION_END:
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
        public RELOC_TYPE RelocType;
        public PATCH_TYPE PatchType;

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
                    case (byte)PATCH_TYPE.VALUE:
                        Value = br.ReadUInt32();
                        Type = PATCH_TYPE.VALUE;
                        break;
                    case (byte)PATCH_TYPE.REF:
                    case (byte)PATCH_TYPE.SECTION_BASE:
                    case (byte)PATCH_TYPE.SECTION_START:
                    case (byte)PATCH_TYPE.SECTION_END:
                        Symbol = br.ReadUInt16();
                        Type = (PATCH_TYPE)state;
                        break;
                }
            }
            else
            {
                Type = PATCH_TYPE.EXPR;
                switch(state)
                {
                    case (byte)PATCH_OP.ADD:
                    case (byte)PATCH_OP.SUB:
                    case (byte)PATCH_OP.DIV:
                    case (byte)PATCH_OP.EXC:
                        break;
                    default:
                        throw new Exception("bad patch");
                }

                Op = (PATCH_OP)state;

                Left = new PatchNode(br);
                Right = new PatchNode(br);
            }
        }
        public PatchNode Left;
        public PatchNode Right;
        public uint Value;
        public ushort Symbol;
        public PATCH_TYPE Type;
        public PATCH_OP Op;
    }

    public enum PATCH_TYPE
    {
        REF=2,
        SECTION_BASE=4,
        SECTION_START = 12,
        SECTION_END = 22,
        VALUE = 44,
        EXPR = 45,
        SECTION_SIZE = 46
    }
    public enum RELOC_TYPE
    {
        WORD_LITERAL = 16,
        FUNCTION_CALL = 74,
        UPPER_IMMEDIATE = 82,
        LOWER_IMMEDIATE = 84
    }

    public class Symbol
    {
        public Symbol(BinaryReader br, SYMBOL_TYPE type, Lib_Module mod)
        {
            this.Mod = mod;
            this.Type = type;
            switch(type)
            {
                case SYMBOL_TYPE.INTERNAL:
                    Sym = br.ReadUInt16();
                    Section = br.ReadUInt16();
                    Offset = br.ReadInt32();
                    Name = br.ReadString(-1);
                    break;
                case SYMBOL_TYPE.EXTERNAL:
                    Sym = br.ReadUInt16();
                    Name = br.ReadString(-1);
                    break;
                case SYMBOL_TYPE.BSS:
                    Sym = br.ReadUInt16();
                    Section = br.ReadUInt16();
                    Size = br.ReadInt32();
                    Name = br.ReadString(-1);
                    break;
                case SYMBOL_TYPE.LOCAL:
                    Section = br.ReadUInt16();
                    Offset = br.ReadInt32();
                    Name = br.ReadString(-1);
                    break;
            }
            
        }
        public Lib_Module Mod;
        public SYMBOL_TYPE Type;
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

    public enum SYMBOL_TYPE
    {
        INTERNAL,
        EXTERNAL,
        LOCAL,
        BSS
    }

    public enum STATE_TYPE
    {
        EOF = 0,//
        CODE = 2,//
        SWITCH = 6,//
        BSS_ALLOC = 8,
        PATCH = 10,
        DEF=12,//
        REF=14,//
        SECTION=16,//
        LOCAL=18,//
        FILE=28,
        PROCESSOR=46,
        BSS=48//
    }

    public enum PATCH_OP
    {
        ADD = 44,
        SUB = 46,
        DIV = 50,
        EXC = 54
    }

    

    public static class Helper
    {
        public static string ReadString(this BinaryReader br, int length)
        {
            if (length == -1)
                length = br.ReadByte();
            var buff = new byte[length];
            br.Read(buff, 0, length);
            StringBuilder sb = new StringBuilder();
            foreach(byte b in buff)
            {
                if (b == 0)
                    break;
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
