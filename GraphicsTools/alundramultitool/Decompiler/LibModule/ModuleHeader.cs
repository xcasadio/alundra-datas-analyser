namespace alundramultitool.Decompiler.LibModule;

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