using System.Diagnostics;

namespace Alundra.DatasBin;

public class DatasBin
{
    public readonly DbHeader Header;
    public readonly GameMap[] GameMaps;
    public readonly GameMap AlundraGameMap;
    public readonly string Binfile;

    public DatasBin(string binfile)
    {
        Binfile = binfile;
        using var br = new BinaryReader(File.OpenRead(binfile));
        Header = new DbHeader(br);

        AlundraGameMap = new GameMap(br, Header);

#if DEBUG       //verify maps
        for (var dex = 0; dex < Header.GameMaps.Length; dex++)
        {
            if (Header.GameMaps[dex] > 0)
            {
                br.BaseStream.Position = Header.GameMaps[dex];
                if (br.BaseStream.Position != br.BaseStream.Length)
                {
                    var check = br.ReadInt32();
                    Debug.Assert(check == 28);
                }
            }
        }
#endif

        GameMaps = new GameMap[Header.GameMaps.Length];
        for (var i = 0; i < Header.GameMaps.Length; i++)
        {
            var gameMapOffset = Header.GameMaps[i];

            if (gameMapOffset > 0 && gameMapOffset < br.BaseStream.Length)
            {
                GameMaps[i] = new GameMap(br, gameMapOffset);
                GameMaps[i].Load(br, false);
            }
        }
    }

    public BinaryReader OpenBin()
    {
        return new BinaryReader(File.OpenRead(Binfile));
    }
}