namespace AlundraEngine.DatasBin;

public class WallTiles
{
    public sbyte Offset;
    public byte Count;
    public ushort[] Tiles;

    // TODO: remove, for debugging purpose
    public int TileX { get; set; }
    public int TileY { get; set; }

    public WallTiles()
    {

    }

    public WallTiles(BinaryReader br)
    {
        Offset = br.ReadSByte();
        Count = br.ReadByte();
        Tiles = new ushort[Count];

        for (var i = 0; i < Count; i++)
        {
            Tiles[i] = br.ReadUInt16();
        }
    }
}