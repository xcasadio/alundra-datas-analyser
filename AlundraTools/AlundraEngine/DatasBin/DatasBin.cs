using System.Diagnostics;
using AlundraEngine.Graphics;

namespace AlundraEngine.DatasBin;

public class DatasBin
{
    public readonly DataBinHeader Header;
    public readonly GameMap AlundraGameMap;
    public readonly GameMap[] GameMaps;
    public readonly Bitmap LoadingScreen;

    // Null when built from bytes (no file behind it).
    public readonly string? Binfile;

    private readonly byte[]? _datasBin;

    public DatasBin(string binfile)
    {
        Binfile = binfile;
        using var br = new BinaryReader(File.OpenRead(binfile));
        (Header, AlundraGameMap, GameMaps, LoadingScreen) = Parse(br);
    }

    // The array is kept, not copied: OpenBin() returns readers over it.
    public DatasBin(byte[] datasBin)
    {
        _datasBin = datasBin;
        using var br = new BinaryReader(new MemoryStream(datasBin, writable: false));
        (Header, AlundraGameMap, GameMaps, LoadingScreen) = Parse(br);
    }

    private static (DataBinHeader Header, GameMap AlundraGameMap, GameMap[] GameMaps, Bitmap LoadingScreen) Parse(BinaryReader br)
    {
        var header = new DataBinHeader(br);
        var alundraGameMap = new GameMap(br, header);
        var gameMaps = new GameMap[header.GameMapOffsets.Length]; 

        for (var i = 0; i < header.GameMapOffsets.Length; i++)
        {
            var gameMapOffset = header.GameMapOffsets[i];
        
            if (gameMapOffset > 0 && gameMapOffset < br.BaseStream.Length)
            {
                gameMaps[i] = new GameMap(br, gameMapOffset);
            }
        }

        var offsets = new[]
        {
            header.LoadingScreen0,
            header.LoadingScreen1,
            header.LoadingScreen2,
            header.LoadingScreen3
        };

        var loadingScreen = new Bitmap(320, 240);
        using var graphics2 = System.Drawing.Graphics.FromImage(loadingScreen);
        var index = 0;

        foreach (var offset in offsets)
        {
            br.BaseStream.Position = offset;
            var buffer = br.ReadBytes(320 * 60 * 2);
            var bitmapChunk = TimLoader.DecodeBuffer(0, 320, 60, 16, null, 320, buffer);
            graphics2.DrawImage(bitmapChunk, 0, 60 * index);
            index++;
        }

        return (header, alundraGameMap, gameMaps, loadingScreen);
    }

    public BinaryReader OpenBin()
    {
        if (_datasBin != null)
        {
            return new BinaryReader(new MemoryStream(_datasBin, writable: false));
        }

        return new BinaryReader(File.OpenRead(Binfile!));
    }
}