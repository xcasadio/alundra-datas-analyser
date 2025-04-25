namespace Alundra.Sprite;

public class SiImage
{
    public SiImage(BinaryReader br)
    {
        U1 = br.ReadByte();
        U2 = br.ReadByte();
        Sx = br.ReadByte();
        Sy = br.ReadByte();
        Swidth = br.ReadByte();
        Sheight = br.ReadByte();
        X1 = br.ReadSByte();
        Y1 = br.ReadSByte();
        X2 = br.ReadSByte();
        Y2 = br.ReadSByte();
        X3 = br.ReadSByte();
        Y3 = br.ReadSByte();
        X4 = br.ReadSByte();
        Y4 = br.ReadSByte();
    }

    public byte U1;
    public byte U2;
    public byte Sx;
    public byte Sy;
    public byte Swidth;
    public byte Sheight;
    public sbyte X1;
    public sbyte Y1;
    public sbyte X2;
    public sbyte Y2;
    public sbyte X3;
    public sbyte Y3;
    public sbyte X4;
    public sbyte Y4;
}