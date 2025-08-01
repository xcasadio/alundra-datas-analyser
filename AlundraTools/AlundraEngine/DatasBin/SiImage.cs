using System.Diagnostics;

namespace AlundraEngine.DatasBin;

public class SiImage
{
    public readonly long Signature;

    public SiImage(BinaryReader br)
    {
        Spritesheet = br.ReadByte();
        Palette = br.ReadByte();
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

        Signature = Spritesheet | Palette << 8 | Sx << 16 | Sy << 24 | Swidth << 32 | Sheight << 38;
    }

    public readonly byte Spritesheet;
    public readonly byte Palette;
    public readonly byte Sx;
    public readonly byte Sy;
    public byte Swidth;
    public readonly byte Sheight;
    public readonly sbyte X1;
    public readonly sbyte Y1;
    public sbyte X2;
    public sbyte Y2;
    public readonly sbyte X3;
    public readonly sbyte Y3;
    public sbyte X4;
    public sbyte Y4;
}