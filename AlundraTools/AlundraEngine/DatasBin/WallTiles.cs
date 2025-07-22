namespace AlundraEngine.DatasBin;

public class WallTiles
{
    // TODO: remove, for debugging purpose
    public int TileX { get; set; }
    public int TileY { get; set; }

    public WallTiles(BinaryReader br)
    {
        Offset = br.ReadSByte();
        Count = br.ReadByte();
        Tiles = new ushort[Count];
        //if ((flag != 0 && flag != 255) || count==0 || count == 255)
        //{
        //    flag = flag;
        //}

        for (var i = 0; i < Count; i++)
        {
            Tiles[i] = br.ReadUInt16();
        }
    }
    public sbyte Offset;
    public byte Count;
    public ushort[] Tiles;
}