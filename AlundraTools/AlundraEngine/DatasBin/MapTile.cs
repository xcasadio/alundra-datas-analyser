namespace AlundraEngine.DatasBin;

public class MapTile
{
    // TODO: remove, for debugging purpose
    public int TileX { get; set; }
    public int TileY { get; set; }

    public MapTile()
    {
    }

    public MapTile(BinaryReader br)
    {
        //long i = br.ReadUInt32();

        Walkability = br.ReadByte();
        GroundProperty = br.ReadByte();
        Slope = br.ReadByte();
        Height = br.ReadByte();


        //Walkability = (byte)(i & 0xff);
        //i >>= 8;
        //GroundProperty = (byte)(i & 0xff);
        //i >>= 8;
        //Slope = (byte)(i & 0xff);
        //i >>= 8;
        //Height = (byte)(i & 0xff);

        TileId = br.ReadUInt16();

        if (TileId == 0xffff)
        {
            Palette = -1;
            Tile = -1;
        }
        else
        {
            Palette = (short)((TileId & 0xf000) >> 12);
            Tile = (short)(TileId & 0x3ff);
        }

        TilesOffset = br.ReadInt16();

        if (TilesOffset != -1)
        {
            TilesOffset *= 2;
        }
    }

    public byte Walkability;
    public byte GroundProperty;
    public byte Slope;
    public byte Height;
    public ushort TileId;
    public short Palette;
    public short Tile;
    public short TilesOffset;
    public WallTiles WallTiles;

    public uint Flags => (uint)(Walkability | (GroundProperty << 8) | (Slope << 16) | (Height << 24));

    public void LoadWallTiles(BinaryReader br, long offset)
    {
        if (TilesOffset != -1)
        {
            br.BaseStream.Position = offset + TilesOffset;
            WallTiles = new WallTiles(br);
        }
    }
}