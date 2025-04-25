namespace Alundra.Sprite;

public class SiSector4Record
{
    public SiSector4Record(BinaryReader br)
    {
        U1 = br.ReadByte();
        U2 = br.ReadByte();
        U3 = br.ReadByte();
        U4 = br.ReadByte();
        U5 = br.ReadByte();
        U6 = br.ReadByte();
        U7 = br.ReadByte();
        U8 = br.ReadByte();
    }

    public byte U1;
    public byte U2;
    public byte U3;
    public byte U4;
    public byte U5;
    public byte U6;
    public byte U7;
    public byte U8;
}