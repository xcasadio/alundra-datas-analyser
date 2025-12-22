using AlundraEngine;
using AlundraEngine.Balance;
using AlundraEngine.DatasBin;
using AlundraEngine.Editor;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;

namespace AlundraDataExtractor;

internal class Program
{
    static JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

    static void Main(string[] args)
    {
        var gamePath = args[0];
        var extractionPath = args[1];
        Console.WriteLine($"Extract data from {gamePath}");
        Console.WriteLine($"To {extractionPath}");

        var dataFolder = Path.Combine(gamePath, "DATA");

        var datasBin = new DatasBin(Path.Combine(dataFolder, "DATAS.BIN"));
        var balanceFile = Path.Combine(dataFolder, "BALANCE.BIN");
        var balanceBin = new BalanceBin(balanceFile);
        var soundBinFileName = Path.Combine(dataFolder, "SOUND.BIN");
        var soundBin = new SoundBin(soundBinFileName);
        var font3 = new Font3(Path.Combine(dataFolder, "..", "TAKI\\SCREEN"));
        var etcResFileName = PathHelper.GetEtcFileName(dataFolder);
        EtcRes etcRes;

        if (Path.GetFileName(etcResFileName).Contains("usa", StringComparison.InvariantCultureIgnoreCase))
        {
            etcRes = new EtcResUsa(etcResFileName);
        }
        else
        {
            etcRes = new EtcResR(etcResFileName);
        }

        var gameEngine = new GameEngine(datasBin, balanceBin, soundBin, etcRes, font3);
        gameEngine.InitializeEngine();

        var alunCdExe = new AlunCdExe(gamePath);
        ExtractDataFromAlunCdExe(alunCdExe, extractionPath);
        ExtractDataFromBalanceBin(balanceBin, extractionPath);
        ExtractDataFromScreenFolder(font3, gameEngine, extractionPath);
    }

    private static void ExtractDataFromAlunCdExe(AlunCdExe alunCdExe, string extractionPath)
    {
        var memoryCardPath = Path.Combine(extractionPath, "memorycard");
        Directory.CreateDirectory(memoryCardPath);
        alunCdExe.MemoryCardFrame1Image.Save(Path.Combine(memoryCardPath, "memorycardframe1.png"), ImageFormat.Png);
        alunCdExe.MemoryCardFrame2Image.Save(Path.Combine(memoryCardPath, "memorycardframe2.png"), ImageFormat.Png);
        alunCdExe.MemoryCardFrame3Image.Save(Path.Combine(memoryCardPath, "memorycardframe3.png"), ImageFormat.Png);
    }

    private static void ExtractDataFromBalanceBin(BalanceBin balanceBin, string extractionPath)
    {
        var balanceBinPath = Path.Combine(extractionPath, "balance");
        var elements = balanceBin.BalanceRecords.Select(x => new BalanceRecordJson(x));

        Directory.CreateDirectory(balanceBinPath);
        File.WriteAllText(Path.Combine(balanceBinPath, "balance.json"), JsonSerializer.Serialize(elements, _jsonSerializerOptions));
    }

    private static void ExtractDataFromScreenFolder(Font3 font3, GameEngine gameEngine, string extractionPath)
    {
        var screenPath = Path.Combine(extractionPath, "TAKI", "SCREEN");
        Directory.CreateDirectory(screenPath);
        font3.FontBitmapTim.Save(Path.Combine(screenPath, "font3.png"), ImageFormat.Png);
        var fontCharTiles = new List<FontCharTile>();

        for (int i = 0; i < 16 * 16; i++)
        {
            var charValue = (char)i;
            //charValue = TextDecoder.ConvertCp850ToLatin1(charIndex);
            fontCharTiles.Add(new FontCharTile { Code = i, X = charValue % 16 * 16, Y = charValue / 16 * 16, Width = 16, Height = 16, Palette = 8 });
        }
        //all tiles
        File.WriteAllText(Path.Combine(screenPath, "font3.json"), JsonSerializer.Serialize(fontCharTiles, _jsonSerializerOptions));

        //all hud sprites
        //font3.GenerateHudBitmapFromSprite()
        var windBitmap = new Bitmap(256, 256);




        windBitmap.Save(Path.Combine(screenPath, "wind.png"), ImageFormat.Png);

        //all tiles
    }
}