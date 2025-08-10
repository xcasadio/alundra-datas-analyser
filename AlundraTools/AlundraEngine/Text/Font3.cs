using AlundraEngine.Gameplay;
using AlundraEngine.Graphics;

namespace AlundraEngine.Text;

public class Font3
{
    public Color[][] Palettes;
    public Bitmap PalettesBitmap;
    public Bitmap HudBitmap;
    public Bitmap FontBitmapTim;
    //character tiles description

    private byte[] _hudImageData;
    private byte[] _fontImageDataTim;

    public Font3(string folderName)
    {
        LoadPalette(folderName);
        LoadImage(folderName);
        LoadImageTim(folderName);
    }

    public Bitmap GenerateHudBitmapFromSprite(SPRT sprite)
    {
        return GenerateHudBitmap(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut);
    }

    public Bitmap GenerateHudBitmap(int paletteIndex)
    {
        return GenerateHudBitmap(Palettes[paletteIndex]);
    }

    public Bitmap GenerateHudBitmap(Color[] pal)
    {
        HudBitmap = GenerateHudBitmap(0, 0, 256, 256, pal);
        return HudBitmap;
    }

    public Bitmap GenerateHudBitmap(int x, int y, int w, int h, int paletteIndex)
    {
        return GenerateHudBitmap(x, y, w, h, Palettes[paletteIndex]);
    }

    public Bitmap GenerateHudBitmap(int x, int y, int w, int h, Color[] pal)
    {
        return ImageHelper.BitmapFromPsxBuff(_hudImageData, x, y, w, h, 4, pal);
    }

    public Bitmap GenerateFontBitmapFromSprite(SPRT sprite)
    {
        return GenerateFontBitmapTim(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut);
    }

    public Bitmap GenerateFontBitmapTim(int paletteIndex)
    {
        return GenerateFontBitmapTim(Palettes[paletteIndex]);
    }

    public Bitmap GenerateFontBitmapTim(Color[] pal)
    {
        FontBitmapTim = GenerateFontBitmapTim(0, 0, 256, 256, pal);
        return FontBitmapTim;
    }

    public Bitmap GenerateFontBitmapTim(int x, int y, int w, int h, int paletteIndex)
    {
        return GenerateFontBitmapTim(x, y, w, h, Palettes[paletteIndex]);
    }

    public Bitmap GenerateFontBitmapTim(int x, int y, int w, int h, Color[] pal)
    {
        return ImageHelper.BitmapFromPsxBuff(_fontImageDataTim, x, y, w, h, 4, pal);
    }

    private void LoadPalette(string folderName)
    {
        var maxPalettes = 16;
        Palettes = new Color[maxPalettes][];
        var buffer = File.ReadAllBytes(Path.Combine(folderName, "WIND.CL"));
        var buffIndex = 0;

        for (var i = 0; i < maxPalettes; i++)
        {
            Palettes[i] = new Color[16];

            for (var j = 0; j < 16; j++)
            {
                var b2 = buffer[buffIndex++];
                var b1 = buffer[buffIndex++];
                Palettes[i][j] = ImageHelper.FromPsxColor((b1 << 8) | b2);
            }
        }

        PalettesBitmap = ImageHelper.BitmapFromPsxBuff(buffer, 16, maxPalettes, 16, null);
    }

    private void LoadImage(string folderName)
    {
        _hudImageData = File.ReadAllBytes(Path.Combine(folderName, "WIND.TX"));
    }

    private void LoadImageTim(string folderName)
    {
        _fontImageDataTim = File.ReadAllBytes(Path.Combine(folderName, "FONT3.TIM"));
    }
}