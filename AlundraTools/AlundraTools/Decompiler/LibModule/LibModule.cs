namespace AlundraTools.Decompiler.LibModule;

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