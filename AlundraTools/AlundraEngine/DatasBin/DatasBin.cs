using System.Diagnostics;
using AlundraEngine.Graphics;

namespace AlundraEngine.DatasBin;

public class DatasBin
{
    public readonly DataBinHeader Header;
    public readonly GameMap AlundraGameMap;
    public readonly GameMap[] GameMaps;
    public readonly Bitmap LoadingScreen;

    public readonly string Binfile;

    public DatasBin(string binfile)
    {
        Binfile = binfile;
        using var br = new BinaryReader(File.OpenRead(binfile));
        Header = new DataBinHeader(br);
        AlundraGameMap = new GameMap(br, Header);
        GameMaps = new GameMap[Header.GameMapOffsets.Length]; 

        for (var i = 0; i < Header.GameMapOffsets.Length; i++)
        {
            var gameMapOffset = Header.GameMapOffsets[i];
        
            if (gameMapOffset > 0 && gameMapOffset < br.BaseStream.Length)
            {
                GameMaps[i] = new GameMap(br, gameMapOffset);
            }
        }

        var offsets = new[]
        {
            Header.LoadingScreen0,
            Header.LoadingScreen1,
            Header.LoadingScreen2,
            Header.LoadingScreen3
        };

        LoadingScreen = new Bitmap(320, 240);
        using var graphics2 = System.Drawing.Graphics.FromImage(LoadingScreen);
        var index = 0;

        foreach (var offset in offsets)
        {
            br.BaseStream.Position = offset;
            var buffer = br.ReadBytes(320 * 60 * 2);
            var bitmapChunk = TimLoader.DecodeBuffer(0, 320, 60, 16, null, 320, buffer);
            graphics2.DrawImage(bitmapChunk, 0, 60 * index);
            index++;
        }
    }

    public BinaryReader OpenBin()
    {
        return new BinaryReader(File.OpenRead(Binfile));
    }
}