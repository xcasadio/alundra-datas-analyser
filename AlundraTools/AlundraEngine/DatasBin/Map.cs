using System;
using System.Diagnostics;
using System.Text;

namespace AlundraEngine.DatasBin;

public class Map
{
    public readonly int MemoryAddress;

    public readonly int Width;
    public readonly int Height;
    public readonly int Width2;
    public readonly int Height2;
    public readonly MapCopy[] MapCopies = new MapCopy[256];
    public readonly int WallTilesOffset;
    public readonly MapTile[] MapTiles;

    public Map(BinaryReader br, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        var binaryOffset = br.BaseStream.Position;

        //read sizes
        Width = br.ReadByte();
        Height = br.ReadByte();
        Width2 = br.ReadByte();
        Height2 = br.ReadByte();

        //map copies
        for (int i = 0; i < MapCopies.Length; i++)
        {
            MapCopies[i] = new MapCopy(br);
        }
        
        //read MapTiles
        MapTiles = new MapTile[Width * Height];
        for (var i = 0; i < MapTiles.Length; i++)
        {
            MapTiles[i] = new MapTile(br);

            //TODO : remove, for debugging purpose
            MapTiles[i].TileY = i / Width;
            MapTiles[i].TileX = (i % Width);
        }

        WallTilesOffset = (int)(br.BaseStream.Position - binaryOffset);

        //load wall tiles
        for (var i = 0; i < MapTiles.Length; i++)
        {
            MapTiles[i].LoadWallTiles(br, binaryOffset + WallTilesOffset);

            if (MapTiles[i].WallTiles != null)
            {
                for (int j = 0; j < MapTiles[i].WallTiles.Count; j++)
                {
                    MapTiles[i].WallTiles.TileY = i / Width;
                    MapTiles[i].WallTiles.TileX = (i % Width);
                }
            }
        }

        /*
        var log = new StringBuilder();

        for (int y = 0; y < 60; y++)
        {
            for (int x = 0; x < 52; x++)
            {
                var mapTile = MapTiles[y * Width + x];
                log.AppendFormat("x:{0} y:{1} ", x, y);

                if (mapTile.WallTiles != null)
                {
                    log.AppendFormat("wallTiles:{0} {1} tiles:", mapTile.WallTiles.Count, mapTile.WallTiles.Offset);

                    for (int i = 0; i < mapTile.WallTiles.Count; i++)
                    {
                        log.AppendFormat("{0} ", mapTile.WallTiles.Tiles[i]);
                    }
                }

                log.AppendLine();
            }
        }

        Debug.WriteLine(log.ToString());
        */
    }
}