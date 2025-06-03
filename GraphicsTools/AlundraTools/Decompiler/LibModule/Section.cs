namespace AlundraTools.Decompiler.LibModule;

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