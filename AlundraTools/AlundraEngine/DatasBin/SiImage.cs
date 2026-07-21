using System.Diagnostics;

namespace AlundraEngine.DatasBin;

public class SiImage
{
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

    public readonly long Signature;

    // Not part of the original PSX binary layout. Populated by GameMapHelper.SaveSpriteSheet()
    // as this quad's position in the exported spritesheet PNG (same Swidth/Sheight as the crop
    // size). Unlike Sx/Sy (native VRAM coordinates, shared by any quad that reused the same VRAM
    // region under a different palette), AtlasX/AtlasY are unique per (region, palette) so every
    // quad crops the color it was actually meant to show.
    public int AtlasX;
    public int AtlasY;

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

        Signature = (long)Spritesheet | 
                    (long)Palette << 8 | 
                    (long)Sx << 16 | 
                    (long)Sy << 24 | 
                    (long)Swidth << 32 | 
                    (long)Sheight << 40;
    }
}