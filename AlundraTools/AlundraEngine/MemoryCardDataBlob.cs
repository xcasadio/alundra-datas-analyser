namespace AlundraEngine;

public class MemoryCardDataBlob
{
    public char[] Header = new char[2];
    public byte IconFlags;
    public byte BlockCount;
    public string Title;
    //public byte[] Title = new byte[64];
    public byte[] Reserved_0044 = new byte[28];
    public ushort[] IconClut16 = new ushort[16];
    public byte[] IconFrame4bpp_0 = new byte[128];
    public byte[] IconFrame4bpp_1 = new byte[128];
    public byte[] IconFrame4bpp_2 = new byte[128];
    public byte[] SavePayload = new byte[7636];
    public string DeveloperWatermark;
    //public byte[] DeveloperWatermark = new byte[40];
    public uint Checksum;
}