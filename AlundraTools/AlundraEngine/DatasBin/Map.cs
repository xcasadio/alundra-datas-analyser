namespace AlundraEngine.DatasBin;

public class Map
{
    public Map(BinaryReader br, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        var binoffset = br.BaseStream.Position;

        Width = br.ReadByte();
        Height = br.ReadByte();
        Width2 = br.ReadByte();
        Height2 = br.ReadByte();

        br.BaseStream.Position = binoffset + 1540;//why this number?

        MapTiles = new MapTile[Width * Height];
        for (var i = 0; i < MapTiles.Length; i++)
        {
            MapTiles[i] = new MapTile(br);
        }

        WallTilesOffset = (int)(br.BaseStream.Position - binoffset);

        //load wall tiles
        for (var i = 0; i < MapTiles.Length; i++)
        {
            MapTiles[i].LoadWallTiles(br, binoffset + WallTilesOffset);
        }
    }
    public readonly int MemoryAddress;

    public readonly int Width;
    public readonly int Height;
    public readonly int Width2;
    public readonly int Height2;

    public readonly int WallTilesOffset;

    public readonly MapTile[] MapTiles;

}