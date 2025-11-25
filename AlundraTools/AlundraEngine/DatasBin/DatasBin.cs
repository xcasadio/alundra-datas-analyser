using System.Diagnostics;

namespace AlundraEngine.DatasBin;

public class DatasBin
{
    public readonly DataBinHeader Header;
    public readonly GameMap[] GameMaps;
    public readonly GameMap AlundraGameMap;
    public readonly string Binfile;

    public DatasBin(string binfile)
    {
        Binfile = binfile;
        using var br = new BinaryReader(File.OpenRead(binfile));
        Header = new DataBinHeader(br);

        AlundraGameMap = new GameMap(br, Header);

#if DEBUG       
        //verify maps
        for (var i = 0; i < Header.GameMapOffsets.Length; i++)
        {
            if (Header.GameMapOffsets[i] > 0)
            {
                br.BaseStream.Position = Header.GameMapOffsets[i];
                if (br.BaseStream.Position != br.BaseStream.Length)
                {
                    var check = br.ReadInt32();
                    Debug.Assert(check == 28);
                }
            }
        }
#endif

        GameMaps = new GameMap[Header.GameMapOffsets.Length]; 
        for (var i = 0; i < Header.GameMapOffsets.Length; i++)
        {
            var gameMapOffset = Header.GameMapOffsets[i];
        
            if (gameMapOffset > 0 && gameMapOffset < br.BaseStream.Length)
            {
                GameMaps[i] = new GameMap(br, gameMapOffset);
            }
        }
    }

    public BinaryReader OpenBin()
    {
        return new BinaryReader(File.OpenRead(Binfile));
    }
}