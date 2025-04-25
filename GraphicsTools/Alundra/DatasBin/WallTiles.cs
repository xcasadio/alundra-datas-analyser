namespace Alundra.DatasBin;

public class WallTiles
{
    public WallTiles(BinaryReader br)
    {
        Offset = br.ReadSByte();
        Count = br.ReadByte();
        Tiles = new short[Count];
        //if ((flag != 0 && flag != 255) || count==0 || count == 255)
        //{
        //    flag = flag;
        //}

        for (var dex = 0; dex < Count; dex++)
        {
            Tiles[dex] = br.ReadInt16();
        }
    }
    public readonly sbyte Offset;
    public readonly byte Count;
    public readonly short[] Tiles;
}