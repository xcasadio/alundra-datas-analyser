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

    // Mirroring is not stored as a flag: the quad carries it in its destination corners, which are
    // handed to the GPU with the source's left edge on corner 1 and its right edge on corner 2, so
    // X1 > X2 draws the texels right to left. GraphicManager.DrawSprite maps the UVs that way.
    public readonly bool IsMirroredX;
    public readonly bool IsMirroredY;

    // VRAM window this quad actually samples, and what every crop must use instead of Sx/Sy.
    //
    // A mirrored quad names its source one texel early: the mirrored and unmirrored halves of the
    // same artwork are stored with Sx and Sx+1 (bank 0 walk cycle: head 151/152, body 87/88, legs
    // 23/24), because the GPU walks the texels backwards from the far edge. Cropping at the raw Sx
    // therefore picks up one column of whatever sits before the sprite in VRAM and drops its own
    // last column - the stray pixels that showed up beside the head on every right-facing frame.
    //
    // Measured on the real data: 208830 of 964600 quads are mirrored horizontally, and 177564 of
    // those have an unmirrored twin at exactly Sx+1; comparing the two crops column by column over
    // 532 sampled pairs came out pixel-identical on 514. The vertical case behaves the same way
    // (8 of 8 pairs identical), so Sy shifts too.
    //
    // Signature is built from these rather than from Sx/Sy so that a mirrored quad and the
    // unmirrored quad it shares artwork with land on one cache entry and one atlas cell.
    public readonly byte SourceX;
    public readonly byte SourceY;

    // Not part of the original PSX binary layout. Populated by GameMapHelper.SaveSpriteSheet()
    // as this quad's position in the exported spritesheet PNG (same Swidth/Sheight as the crop
    // size). Unlike SourceX/SourceY (native VRAM coordinates, shared by any quad that reused the
    // same VRAM region under a different palette), AtlasX/AtlasY are unique per (region, palette)
    // so every quad crops the color it was actually meant to show.
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

        IsMirroredX = X1 > X2;
        IsMirroredY = Y1 > Y3;

        // A VRAM page is 256x256, so the shift is skipped where it would walk the crop off the
        // page edge - no quad in the shipped data does, but the crop must stay in bounds.
        SourceX = ShiftSource(Sx, Swidth, IsMirroredX);
        SourceY = ShiftSource(Sy, Sheight, IsMirroredY);

        Signature = (long)Spritesheet |
                    (long)Palette << 8 |
                    (long)SourceX << 16 |
                    (long)SourceY << 24 |
                    (long)Swidth << 32 |
                    (long)Sheight << 40;
    }

    private const int VramPageSize = 256;

    private static byte ShiftSource(byte origin, byte size, bool isMirrored)
    {
        return isMirrored && origin + size < VramPageSize ? (byte)(origin + 1) : origin;
    }
}