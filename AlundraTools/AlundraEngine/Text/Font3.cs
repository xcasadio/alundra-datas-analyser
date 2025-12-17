using AlundraEngine.Gameplay;
using AlundraEngine.Graphics;

namespace AlundraEngine.Text;

public class Font3
{
    public Color[][] Palettes;
    public Bitmap PalettesBitmap;
    public Bitmap HudBitmap;
    public Bitmap FontBitmapTim;

    private readonly Dictionary<int, Bitmap> _hudBitmapByPalette = new();
    private readonly Dictionary<(int, int), Bitmap> _hudBitmapBySprite = new();
    private readonly Dictionary<(int, int), Bitmap> _fontBitmapBySprite = new();
    private byte[] _hudImageData;

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
        var spriteKey = (x << 24) | (y << 16) | (w << 8) | h;
        var paletteKey = GetPaletteHashCode(pal);
        var key = (spriteKey, paletteKey);

        if (!_hudBitmapBySprite.TryGetValue(key, out var bitmap))
        {
            if (!_hudBitmapByPalette.TryGetValue(paletteKey, out var fullBitmap))
            {
                fullBitmap = ImageHelper.BitmapFromPsxBuff(_hudImageData, 0, 0, 256, 256, 4, pal);
                _hudBitmapByPalette.Add(paletteKey, fullBitmap);
            }

            var rect = new Rectangle(x, y, w, h);
            bitmap = fullBitmap.Clone(rect, FontBitmapTim.PixelFormat);
            _hudBitmapBySprite.Add(key, bitmap);
        }

        return bitmap;
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
        return GenerateFontBitmapTim(0, 0, 256, 256, pal);
    }

    public Bitmap GenerateFontBitmapTim(int x, int y, int w, int h, int paletteIndex)
    {
        return GenerateFontBitmapTim(x, y, w, h, Palettes[paletteIndex]);
    }

    public Bitmap GenerateFontBitmapTim(int x, int y, int w, int h, Color[] pal)
    { 
        var spriteKey = (x << 24) | (y << 16) | (w << 8) | h;
        var paletteKey = GetPaletteHashCode(pal);
        var key = (spriteKey, paletteKey);

        if (!_fontBitmapBySprite.TryGetValue(key, out var bitmap))
        {
            var rect = new Rectangle(x, y, w, h);
            bitmap = FontBitmapTim.Clone(rect, FontBitmapTim.PixelFormat); 
            _fontBitmapBySprite.Add(key, bitmap);
        }

        return bitmap;
    }

    private static int GetPaletteHashCode(Color[] pal)
    {
        var hash = new HashCode();
        foreach (var color in pal)
        {
            hash.Add(color.ToArgb());
        }
        return hash.ToHashCode();
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
        FontBitmapTim = TimLoader.LoadTim(Path.Combine(folderName, "FONT3.TIM"), 
            0, Color.FromArgb(255, 156, 165, 132));
        //_fontImageDataTim = File.ReadAllBytes(Path.Combine(folderName, "FONT3.TIM"));
    }
}