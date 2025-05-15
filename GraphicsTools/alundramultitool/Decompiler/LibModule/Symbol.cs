namespace alundramultitool.Decompiler.LibModule;

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