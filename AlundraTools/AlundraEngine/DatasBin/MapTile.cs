namespace AlundraEngine.DatasBin;

public class MapTile
{
    public byte Walkability;
    public byte GroundProperty;
    public byte Slope;
    public byte Height;
    public ushort TileId;
    public short Palette;
    public short Tile;
    public short WallTilesOffset;
    public WallTiles? WallTiles;

    // TODO: remove, for debugging purpose
    public int TileX { get; set; }
    public int TileY { get; set; }

    public MapTile() { }

    public MapTile(BinaryReader br)
    {
        Walkability = br.ReadByte();
        GroundProperty = br.ReadByte();
        Slope = br.ReadByte();
        Height = br.ReadByte();
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

        WallTilesOffset = br.ReadInt16();
        if (WallTilesOffset != -1)
        {
            WallTilesOffset *= 2;
        }
    }

    public uint Flags => (uint)(Walkability | (GroundProperty << 8) | (Slope << 16) | (Height << 24));

    public void LoadWallTiles(BinaryReader br, long offset)
    {
        if (WallTilesOffset != -1)
        {
            br.BaseStream.Position = offset + WallTilesOffset;
            WallTiles = new WallTiles(br);
        }
    }

    public override string ToString()
    {
        var wallInfo = WallTiles == null ? "-1" : WallTiles.Count.ToString();
        return $"w:{Walkability} g:{GroundProperty} s:{Slope} h:{Height} id:{TileId} p:{Palette} t:{Tile} o:{WallTilesOffset} x:{TileX} y:{TileY} wall:{wallInfo}";
    }
}