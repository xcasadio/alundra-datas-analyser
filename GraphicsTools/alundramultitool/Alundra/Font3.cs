namespace GraphicsTools.Alundra;

public class Font3
{
    public Color[][] Palettes;
    public Bitmap PalettesBitmap;
    public Bitmap FontBitmap;
    //character tiles description

    private byte[] _fontImageData;

    public Font3(string folderName)
    {
        LoadPalette(folderName);
        LoadImage(folderName);
        LoadCharacterDescriptions(folderName);
    }
    
    public Bitmap GenerateFontBitmap(Color[] pal)
    {
        FontBitmap = Utils.BitmapFromPsxBuff(_fontImageData, 256, 256, 4, pal);
        return FontBitmap;
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
                Palettes[i][j] = Utils.FromPsxColor((b1 << 8) | b2);
            }
        }

        PalettesBitmap = Utils.BitmapFromPsxBuff(buffer, 16, maxPalettes, 16, null);
    }

    private void LoadImage(string folderName)
    {
        _fontImageData = File.ReadAllBytes(Path.Combine(folderName, "WIND.TX"));
    }

    private void LoadCharacterDescriptions(string folderName)
    {
        var buffer = File.ReadAllBytes(Path.Combine(folderName, "FONT3.TIM")); // 32 832
        var numChar = 256;
        _fontImageData = new byte[256 * 256 * numChar / 2];//numspritesheets 256x256 4bpp bitmaps
        Utils.Deflate(buffer, _fontImageData);
    }
}