using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record SiImageJson
{
    public byte Spritesheet { get; set; }
    public byte Palette { get; set; }
    public byte Sx { get; set; }
    public byte Sy { get; set; }
    public byte Swidth { get; set; }
    public byte Sheight { get; set; }
    public sbyte X1 { get; set; }
    public sbyte Y1 { get; set; }
    public sbyte X2 { get; set; }
    public sbyte Y2 { get; set; }
    public sbyte X3 { get; set; }
    public sbyte Y3 { get; set; }
    public sbyte X4 { get; set; }
    public sbyte Y4 { get; set; }

    public SiImageJson(SiImage siImage)
    {
        Spritesheet = siImage.Spritesheet;
        Palette = siImage.Palette;
        Sx = siImage.Sx;
        Sy = siImage.Sy;
        Swidth = siImage.Swidth;
        Sheight = siImage.Sheight;
        X1 = siImage.X1;
        Y1 = siImage.Y1;
        X2 = siImage.X2;
        Y2 = siImage.Y2;
        X3 = siImage.X3;
        Y3 = siImage.Y3;
        X4 = siImage.X4;
        Y4 = siImage.Y4;
    }
}