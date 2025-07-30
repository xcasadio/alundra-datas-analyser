namespace AlundraEngine.Text;

public class Font3
{
    public Color[][] Palettes;
    public Bitmap PalettesBitmap;
    public Bitmap FontBitmap;
    public Bitmap FontBitmapTim;
    //character tiles description

    private byte[] _fontImageData;
    private byte[] _fontImageDataTim;

    public Font3(string folderName)
    {
        LoadPalette(folderName);
        LoadImage(folderName);
        LoadImageTim(folderName);
    }
    
    public Bitmap GenerateFontBitmap(Color[] pal)
    {
        FontBitmap = ImageHelper.BitmapFromPsxBuff(_fontImageData, 256, 256, 4, pal);
        return FontBitmap;
    }

    public Bitmap GenerateFontBitmapTim(Color[] pal)
    {
        FontBitmapTim = ImageHelper.BitmapFromPsxBuff(_fontImageDataTim, 256, 256, 4, pal);
        return FontBitmapTim;
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
        _fontImageData = File.ReadAllBytes(Path.Combine(folderName, "WIND.TX"));
    }

    private void LoadImageTim(string folderName)
    {
        _fontImageDataTim = File.ReadAllBytes(Path.Combine(folderName, "FONT3.TIM"));
    }
}