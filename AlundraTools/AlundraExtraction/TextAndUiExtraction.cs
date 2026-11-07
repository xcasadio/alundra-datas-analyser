using System.Drawing;
using AlundraEngine;
using AlundraEngine.Balance;
using AlundraEngine.Closing;
using AlundraEngine.DatasBin;
using AlundraEngine.Editor;
using AlundraEngine.Etc;
using AlundraEngine.Text;
using AlundraDataExtractor;

namespace AlundraExtraction;

/// <summary>Family 1 of the extraction: memory card frames, closing images, balance, font3, wind, ETC strings, loading screen.</summary>
public static class TextAndUiExtraction
{
    // ---- Compute (no writing) ----

    public static Bitmap[] ComputeMemoryCardFrames(AlunCdExe alunCdExe) =>
        [alunCdExe.MemoryCardFrame1Image, alunCdExe.MemoryCardFrame2Image, alunCdExe.MemoryCardFrame3Image];

    public static IEnumerable<(string FileName, Bitmap Image)> ComputeClosingImages(ClosingExeInspector closingExeInspector) =>
        closingExeInspector.EnumerateImages();

    // E19.L-b1 (O-E19-66, docs/plan-e19-opcodes.md section 1.2w.2): FONT3.TIM's 4 bpp image decoded with entry 8 of
    // the binary's CLUT table, filled from WIND.CL (the loop at 0x80044B48-0x80044B9C reads it into the table, the text
    // bands and the cursor draw font3 with entry 8, read at 0x80050424). Only those two uses are proven to read entry 8
    // (docs/formats/font.md). The 15-bit colours are decoded as TimLoader does (bit replication, never the plain << 3 of
    // Font3.Palettes); colour 0x0000 stays transparent and keeps FONT3.TIM's own index-0 RGB. Lives here and calls the
    // public TimLoader API: the decompiled AlundraEngine is not modified.
    public static Bitmap DecodeFont3WithWindClut(string screenFolder, int windClutEntry) =>
        DecodeFont3WithWindClut(File.ReadAllBytes(Path.Combine(screenFolder, "FONT3.TIM")), File.ReadAllBytes(Path.Combine(screenFolder, "WIND.CL")), windClutEntry);

    /// <summary>Same as the folder overload, from the contents of FONT3.TIM and WIND.CL.</summary>
    public static Bitmap DecodeFont3WithWindClut(byte[] font3Tim, byte[] windCl, int windClutEntry)
    {
        using var stream = new MemoryStream(font3Tim, writable: false);
        using var reader = new BinaryReader(stream);
        var raw = AlundraEngine.Graphics.TimLoader.LoadTimRaw(reader, Color.FromArgb(255, 156, 165, 132));
        var palette = new Color[16];
        for (var i = 0; i < 16; i++)
        {
            int c = BitConverter.ToUInt16(windCl, (windClutEntry * 16 + i) * 2);
            if (c == 0)
            {
                palette[i] = raw.Palettes![0][0];
                continue;
            }

            int r = ((c & 0x1F) << 3) | ((c & 0x1F) >> 2);
            int g = (((c >> 5) & 31) << 3) | (((c >> 5) & 31) >> 2);
            int b = (((c >> 10) & 31) << 3) | (((c >> 10) & 31) >> 2);
            palette[i] = Color.FromArgb(255, r, g, b);
        }

        return AlundraEngine.Graphics.TimLoader.DecodeBuffer(0, raw.Width, raw.Height, raw.Bpp, new[] { palette }, raw.ImgWWords, raw.ImgData);
    }

    public static List<FontCharTile> ComputeFont3Records()
    {
        var fontCharTiles = new List<FontCharTile>();

        for (int i = 0; i < 16 * 16; i++)
        {
            var charValue = (char)i;
            //charValue = TextDecoder.ConvertCp850ToLatin1(charIndex);
            fontCharTiles.Add(new FontCharTile { Code = i, X = charValue % 16 * 16, Y = charValue / 16 * 16, Width = 16, Height = 16, Palette = 8 });
        }

        return fontCharTiles;
    }

    /// <summary>The wind image and its tiles ordered by (U0, V0), as written to wind.json.</summary>
    public static (Bitmap Image, List<SpriteSheetTile> OrderedTiles) ComputeWind(Font3 font3, StaticVariables staticVariables)
    {
        var windBitmap = new Bitmap(256, 256);
        var windTiles = DrawAllUITiles(font3, staticVariables, windBitmap);
        var ordered = windTiles.OrderBy(x => x.U0).ThenBy(x => x.V0).ToList();
        return (windBitmap, ordered);
    }

    public static Dictionary<int, string> ComputeSortedEtcStrings(EtcRes etcRes) =>
        etcRes.StringByIndex.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

    public static Bitmap ComputeLoadingScreen(DatasBin datasBin) => datasBin.LoadingScreen;

    // ---- Write entries ----

    public static void WriteMemoryCardFrames(AlunCdExe alunCdExe, IExtractionSink sink)
    {
        var frames = ComputeMemoryCardFrames(alunCdExe);
        for (var i = 0; i < frames.Length; i++)
        {
            sink.WriteBytes($"memorycard/memorycardframe{i + 1}.png", ExtractionImages.EncodePng(frames[i]));
        }
    }

    public static void WriteClosingImages(ClosingExeInspector closingExeInspector, IExtractionSink sink)
    {
        foreach (var (fileName, image) in ComputeClosingImages(closingExeInspector))
        {
            sink.WriteBytes($"closing/{fileName}", ExtractionImages.EncodePng(image));
        }
    }

    public static void WriteBalance(BalanceBin balanceBin, IExtractionSink sink)
    {
        sink.WriteBytes($"data/{Path.GetFileName(balanceBin.FileName)}.json", ExtractionJson.Serialize(balanceBin));
    }

    public static void WriteFont3AndWind(Font3 font3, StaticVariables staticVariables, string screenFolder, IExtractionSink sink) =>
        WriteFont3AndWind(font3, staticVariables, DecodeFont3WithWindClut(screenFolder, 8), sink);

    /// <summary>Same as the folder overload, from the contents of FONT3.TIM and WIND.CL.</summary>
    public static void WriteFont3AndWind(Font3 font3, StaticVariables staticVariables, byte[] font3Tim, byte[] windCl, IExtractionSink sink) =>
        WriteFont3AndWind(font3, staticVariables, DecodeFont3WithWindClut(font3Tim, windCl, 8), sink);

    // Takes ownership of font3Page (disposed here).
    private static void WriteFont3AndWind(Font3 font3, StaticVariables staticVariables, Bitmap font3Page, IExtractionSink sink)
    {
        using (font3Page)
        {
            sink.WriteBytes("ui/font3.png", ExtractionImages.EncodePng(font3Page));
        }

        sink.WriteBytes("ui/font3.json", ExtractionJson.Serialize(ComputeFont3Records()));

        var (windBitmap, windData) = ComputeWind(font3, staticVariables);
        sink.WriteBytes("ui/wind.png", ExtractionImages.EncodePng(windBitmap));
        sink.WriteBytes("ui/wind.json", ExtractionJson.Serialize(windData));
    }

    public static void WriteEtcStrings(EtcRes etcRes, IExtractionSink sink)
    {
        sink.WriteBytes($"data/{Path.GetFileName(etcRes.FileName)}.json", ExtractionJson.Serialize(ComputeSortedEtcStrings(etcRes)));
    }

    public static void WriteLoadingScreen(DatasBin datasBin, IExtractionSink sink)
    {
        sink.WriteBytes("data/loading_screen.png", ExtractionImages.EncodePng(ComputeLoadingScreen(datasBin)));
    }

    // ---- Wind tiles (moved unchanged from the extractor) ----

    private static HashSet<SpriteSheetTile> DrawAllUITiles(Font3 font3, StaticVariables staticVariables, Bitmap windBitmap)
    {
        var spriteSheetTiles = new HashSet<SpriteSheetTile>();

        //cursor dialog
        for (int i = 0; i < 4; i++)
        {
            spriteSheetTiles.Add(new SpriteSheetTile(
                staticVariables.g_dialogCursorTextureUV[i * 0x28],
                staticVariables.g_dialogCursorTextureUV[i * 0x28 + 1],
                0x10,
                0x10,
                8));
        }

        //message background
        foreach (var sprite in staticVariables.g_uiBoxesInventoryDescriptionBackground.SpritesA)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        //numbers
        for (int i = 0; i < 10; i++)
        {
            spriteSheetTiles.Add(new SpriteSheetTile(
                staticVariables.g_numbersSpriteSheetUVs[i * 0x14],
                staticVariables.g_numbersSpriteSheetUVs[i * 0x14 + 1],
                8,
                0x10,
                5));
        }

        AddHudTiles(staticVariables, spriteSheetTiles);
        AddInventoryTiles(staticVariables, spriteSheetTiles);

        //draw
        using var g = Graphics.FromImage(windBitmap);
        foreach (var tile in spriteSheetTiles)
        {
            var tileBitmap = font3.GenerateHudBitmap(tile.U0, tile.V0, tile.Width, tile.Height, tile.PaletteIndex);
            g.DrawImage(tileBitmap, tile.U0, tile.V0, tile.Width, tile.Height);
        }

        return spriteSheetTiles;
    }

    private static void AddHudTiles(StaticVariables staticVariables, HashSet<SpriteSheetTile> spriteSheetTiles)
    {
        //'/'
        spriteSheetTiles.Add(new SpriteSheetTile(
            staticVariables.g_numbersSpriteSheetUVs[200],  //0x50
            staticVariables.g_numbersSpriteSheetUVs[201],  //0x28
            8,
            0x10,
            5));

        //full life big icons
        spriteSheetTiles.Add(new SpriteSheetTile(
            staticVariables.g_fullLifeBigIconUVs[0],
            staticVariables.g_fullLifeBigIconUVs[1],
            0x10,
            0x10,
            7));


        //empty life big icons
        spriteSheetTiles.Add(new SpriteSheetTile(
            staticVariables.g_emptyLifeBigIconUVs[0],
            staticVariables.g_emptyLifeBigIconUVs[1],
            0x10,
            0x10,
            7));


        //full life small icons
        spriteSheetTiles.Add(new SpriteSheetTile(
            staticVariables.g_fullLifeSmallIconUVs[0],
            staticVariables.g_fullLifeSmallIconUVs[1],
            8,
            8,
            7));


        //empty life small icons
        spriteSheetTiles.Add(new SpriteSheetTile(
            staticVariables.g_emptyLifeSmallIconUVs[0],
            staticVariables.g_emptyLifeSmallIconUVs[1],
            8,
            8,
            7));

        // mp cristal
        for (int i = 0; i < 14; i++)
        {
            spriteSheetTiles.Add(new SpriteSheetTile(
                i * 8,
                56,
                8,
                0x10,
                5));
        }

        //money icons
        for (int i = 0; i < 4; i++)
        {
            spriteSheetTiles.Add(new SpriteSheetTile(
                staticVariables.g_hudMoneyIconUVs[i * 20],
                staticVariables.g_hudMoneyIconUVs[i * 20 + 1],
                8,
                0x10,
                5));
        }
    }

    private static void AddInventoryTiles(StaticVariables staticVariables, HashSet<SpriteSheetTile> spriteSheetTiles)
    {
        //cursor
        for (int i = 0; i < 4; i++)
        {
            var sprite = new SpriteSheetTile(
                staticVariables.g_inventoryCursorTextureUVs[i * 0x28],
                staticVariables.g_inventoryCursorTextureUVs[i * 0x28 + 1],
                0x10,
                0x10,
                0);
            spriteSheetTiles.Add(sprite);
        }

        //icons
        foreach (var sprite in staticVariables.g_moneyFalconKeyIconSpritesA)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        //rectangle selection
        spriteSheetTiles.Add(new SpriteSheetTile(48, 0x98, 0x18, 0x20, 0));

        //background
        foreach (var sprite in staticVariables.g_MainInventoryWeaponBackgroundSpritesA)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.g_MainInventoryItemBackgroundSpritesA)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800ad594)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800af674)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800b06ec)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800b123c)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800b1d8c)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800ba3d0)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800bcb40)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800bf2b0)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800c1a20)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }
    }
}
