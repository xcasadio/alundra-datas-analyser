using System.Text;

namespace AlundraTools.Decompiler.LibModule;

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