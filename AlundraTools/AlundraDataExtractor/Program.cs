using AlundraEngine;
using AlundraEngine.Balance;
using AlundraEngine.DatasBin;
using AlundraEngine.Editor;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;
using AlundraEngine.Etc;

namespace AlundraDataExtractor;

internal class Program
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };
    private static Dictionary<string, HashSet<string>> entitySpriteSheetIds = new();
    private static HashSet<string> entitySpriteSheetAlreadySaved = new();

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

        var gameEngine = new GameEngine(datasBin, balanceBin, soundBin, etcRes, font3, null);
        gameEngine.InitializeEngine();

        var alunCdExe = new AlunCdExe(gamePath);

        ExtractDataFromAlunCdExe(alunCdExe, extractionPath);
        ExtractDataFromBalanceBin(balanceBin, extractionPath);
        ExtractDataFromScreenFolder(font3, gameEngine.StaticVariables, extractionPath);
        ExtractDataFromEtcRes(etcRes, gameEngine.StaticVariables, extractionPath);
        ExtractDataFromDatasBin(datasBin, gameEngine.StaticVariables, extractionPath);
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
        var elements = balanceBin.BalanceRecords.Select(x => new BalanceRecordJson(x));
        var balanceBinPath = Path.Combine(extractionPath, "data");
        Directory.CreateDirectory(balanceBinPath);
        File.WriteAllText(Path.Combine(balanceBinPath, $"{Path.GetFileName(balanceBin.FileName)}.json"), JsonSerializer.Serialize(elements, _jsonSerializerOptions));
    }

    private static void ExtractDataFromScreenFolder(Font3 font3, StaticVariables staticVariables, string extractionPath)
    {
        var screenPath = Path.Combine(extractionPath, "ui");
        Directory.CreateDirectory(screenPath);
        font3.FontBitmapTim.Save(Path.Combine(screenPath, "font3.png"), ImageFormat.Png);
        var fontCharTiles = new List<FontCharTile>();

        for (int i = 0; i < 16 * 16; i++)
        {
            var charValue = (char)i;
            //charValue = TextDecoder.ConvertCp850ToLatin1(charIndex);
            fontCharTiles.Add(new FontCharTile { Code = i, X = charValue % 16 * 16, Y = charValue / 16 * 16, Width = 16, Height = 16, Palette = 8 });
        }

        //all tiles data
        File.WriteAllText(Path.Combine(screenPath, "font3.json"), JsonSerializer.Serialize(fontCharTiles, _jsonSerializerOptions));

        //all hud sprites
        var windBitmap = new Bitmap(256, 256);
        var windTiles = DrawAllUITiles(font3, staticVariables, windBitmap);
        windBitmap.Save(Path.Combine(screenPath, "wind.png"), ImageFormat.Png);

        //all tiles data
        var windData = windTiles.OrderBy(x => x.U0).ThenBy(x => x.V0);
        File.WriteAllText(Path.Combine(screenPath, "wind.json"), JsonSerializer.Serialize(windData, _jsonSerializerOptions));
    }

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

    private static void ExtractDataFromEtcRes(EtcRes etcRes, StaticVariables staticVariables, string extractionPath)
    {
        var dataPath = Path.Combine(extractionPath, "data");
        Directory.CreateDirectory(dataPath);
        var path = Path.Combine(dataPath, $"{Path.GetFileName(etcRes.FileName)}.json");
        var sortedData = etcRes.StringByIndex.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        File.WriteAllText(path, JsonSerializer.Serialize(sortedData, _jsonSerializerOptions));
    }

    private static void ExtractDataFromDatasBin(DatasBin datasBin, StaticVariables staticVariables, string extractionPath)
    {
        datasBin.LoadingScreen.Save(Path.Combine(extractionPath, "data", "loading_screen.png"), ImageFormat.Png);

        var tileAnimDescriptors = GameInitializer.CreateTileAnimDescriptors(0);
        EntityNames.Load(EntityNames.Language.French);

        var dataPath = Path.Combine(extractionPath, "data");
        Directory.CreateDirectory(dataPath);

        Console.WriteLine("Extract map alundra");
        using var br = datasBin.OpenBin();
        datasBin.AlundraGameMap.Load(br);
        SaveAlundraMap(datasBin.AlundraGameMap, dataPath);

        for (int i = 0; i < 483; i++)
        {
            Console.WriteLine($"Extract map {i}");
            var gameMap = datasBin.GameMaps[i];
            gameMap.Load(br);
            SaveMap(gameMap, i, dataPath, tileAnimDescriptors);
        }

        foreach (var entitySpriteSheet in entitySpriteSheetIds)
        {
            Console.WriteLine($"Entity {entitySpriteSheet.Key}");

            foreach (var idName in entitySpriteSheet.Value)
            {
                Console.WriteLine($"\t{idName}");
            }
        }
    }

    private static void SaveAlundraMap(GameMap gameMap, string extractionPath)
    {
        var gameMapJson = ConvertGameMap(gameMap);
        File.WriteAllText(Path.Combine(extractionPath, "map_alundra.json"), JsonSerializer.Serialize(gameMapJson, _jsonSerializerOptions));

        gameMapJson.SaveSpriteSheet(gameMap, Path.Combine(extractionPath, "map_alundra_spritesheet.png"));
    }

    private static void SaveMap(GameMap gameMap, int id, string extractionPath, TileAnimDescriptor[] tileAnimDescriptors)
    {
        var gameMapJson = ConvertGameMap(gameMap);
        File.WriteAllText(Path.Combine(extractionPath, $"map_{id}.json"), JsonSerializer.Serialize(gameMapJson, _jsonSerializerOptions));

        //try to extract all entity infos from all map
        //GetEntitySpriteSheets(gameMap, id, extractionPath);

        gameMapJson.SaveTileSheet(gameMap, Path.Combine(extractionPath, $"map_{id}_tilesheet.png"), tileAnimDescriptors);
        gameMapJson.SaveSpriteSheet(gameMap, Path.Combine(extractionPath, $"map_{id}_spritesheet.png"));
    }

    private static void GetEntitySpriteSheets(GameMap gameMap, int id, string extractionPath)
    {
        foreach (var entityRecord in gameMap.SpriteInfo.Entities.Entities.Where(x => x != null))
        {
            if (entityRecord.SpriteTableIndex >= 255)
            {
                continue;
            }

            var spriteRecord = gameMap.SpriteInfo.SpriteRecords[entityRecord.SpriteTableIndex];

            if (spriteRecord?.AnimSets == null)
            {
                continue;
            }

            HashSet<string> spriteSheetIds = new();
            int minSpriteSheetId = int.MaxValue;

            foreach (var animationSet in spriteRecord.AnimSets.Where(x => x != null))
            {
                foreach (var animation in animationSet.PreloadedAnims)
                {
                    if (animation.Frames == null)
                    {
                        continue;
                    }

                    foreach (var frame in animation.Frames)
                    {
                        if (frame.Images?.Images == null)
                        {
                            continue;
                        }

                        foreach (var image in frame.Images?.Images)
                        {
                            spriteSheetIds.Add($"map:{id} spritesheet:{image.Spritesheet & 0x7}");
                            minSpriteSheetId = Math.Min(minSpriteSheetId, image.Spritesheet & 0x7);
                        }
                    }
                }
            }

            if (spriteSheetIds.Count == 0)
            {
                continue;
            }

            var name = EntityNames.GetNameWithIndex(entityRecord.SpriteDirection, entityRecord.SpriteTableIndex);
            //name = name.Replace("_", $"_{id}_");
            //var name = EntityNames.GetName(entityRecord.SpriteTableIndex);

            //if (!entitySpriteSheetIds.TryAdd(name, spriteSheetIds))
            //{
            //    foreach (var spriteSheetId in spriteSheetIds)
            //    {
            //        entitySpriteSheetIds[name].Add(spriteSheetId);
            //    }
            //}

            if (entitySpriteSheetAlreadySaved.Add(name))
            {
                var fileName = Path.Combine(extractionPath, name.Replace('\\', '-').Replace('/', '-') + ".png");
                //SaveEntitySpriteSheet(gameMap, fileName, spriteSheetIds, spriteRecord, minSpriteSheetId);
            }
        }
    }

    public static void SaveEntitySpriteSheet(GameMap gameMap, string fileName, HashSet<string> spriteSheetIds, SpriteRecord spriteRecord, int minSpriteSheetId)
    {
        using var bitmap = new Bitmap(256, 256 * spriteSheetIds.Count);
        using var graphics = Graphics.FromImage(bitmap);

        foreach (var animationSet in spriteRecord.AnimSets)
        {
            if (animationSet == null)
            {
                continue;
            }

            foreach (var animation in animationSet.PreloadedAnims)
            {
                if (animation?.Frames == null)
                {
                    continue;
                }

                for (int i = 0; i < animation.NumberOfFrames; i++)
                {
                    var frame = animation.Frames[i];

                    if (frame?.Images?.Images == null)
                    {
                        continue;
                    }

                    foreach (var image in frame.Images.Images)
                    {
                        var spriteBitmap = gameMap.GetSpriteBitmap(image);
                        var x = image.Sx;
                        var y = ((image.Spritesheet & 0x7) - minSpriteSheetId) * 256 + image.Sy;
                        graphics.DrawImage(spriteBitmap, x, y);
                    }
                }
            }
        }

        bitmap.Save(fileName, ImageFormat.Png);
    }

    private static GameMapJson ConvertGameMap(GameMap gameMap)
    {
        return new GameMapJson(gameMap);
    }
}