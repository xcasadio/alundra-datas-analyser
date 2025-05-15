namespace alundramultitool.Decompiler.LibModule;

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