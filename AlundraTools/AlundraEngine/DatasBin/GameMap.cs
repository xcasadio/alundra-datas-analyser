using System.Diagnostics;
using AlundraEngine.Text;

namespace AlundraEngine.DatasBin;

public class GameMap
{
    public GameMap(BinaryReader br, DataBinHeader dbheader)
    {
        //the alundra gamemap (just has sprites)
        br.BaseStream.Position = Offset = 0;
        Header = new GameMapHeader(dbheader);
    }

    public GameMap(BinaryReader br, long offset)
    {
        //read header
        br.BaseStream.Position = Offset = offset;
        Header = new GameMapHeader(br);

        //just read mapid
        br.BaseStream.Position = Offset + Header.InfoBlockOffset;
        Info = new GameMapInfo(br.ReadInt32(), MemoryAddress + Header.InfoBlockOffset);
    }

    public readonly long Offset;
    public static readonly int MemoryAddress = 0x153460;// + 0x260; Header size ??


    public readonly GameMapHeader Header;
    public GameMapInfo Info;
    public SpriteInfo SpriteInfo;
    public ScrollScreen ScrollScreen;
    public Map Map;
    public string[] Strings;
    public readonly bool Loaded = false;
    private byte[] _tileSheetImageData;
    public Bitmap TileSheetBitmap;
    private byte[] _spriteSheetImageData;
    public Bitmap SpriteSheetBitmap;
    private readonly int _numSpriteSheets = 8;

    public void Load(BinaryReader br, bool isMap)
    {
        //read info
        if (Header.InfoBlockOffset != -1)
        {
            br.BaseStream.Position = Offset + Header.InfoBlockOffset;
            Info = new GameMapInfo(br, MemoryAddress + Header.InfoBlockOffset);
        }

        //map
        if (Header.MapBlockOffset != -1)
        {
            br.BaseStream.Position = Offset + Header.MapBlockOffset;
            Map = new Map(br, MemoryAddress + Header.MapBlockOffset);
            Header.WallTilesSize = Header.TileSheetsOffset - (Map.WallTilesOffset + Header.MapBlockOffset);
            Header.MapSize -= Header.WallTilesSize;
        }

        //tilesheet
        if (Header.TileSheetsOffset != -1)
        {
            br.BaseStream.Position = Offset + Header.TileSheetsOffset + 6;
            var buff = new byte[Header.SpriteRecordsOffset - Header.TileSheetsOffset];
            br.Read(buff, 0, buff.Length);
            _tileSheetImageData = new byte[256 * 256 * 6 / 2];//6 256x256 4bpp bitmaps
            ImageHelper.Deflate(buff, _tileSheetImageData);
        }

        //spriteinfo
        if (Header.SpriteRecordsOffset != -1)
        {
            br.BaseStream.Position = Offset + Header.SpriteRecordsOffset;
            SpriteInfo = new SpriteInfo(br, MemoryAddress + Header.SpriteRecordsOffset, Header.SpriteSheetOffset, isMap);
        }

        //spritesheet
        if (Header.SpriteSheetOffset != -1)
        {
            br.BaseStream.Position = Offset + Header.SpriteSheetOffset + 6;
            var buff = new byte[Header.SpritesSize - 6];
            br.Read(buff, 0, buff.Length);
            _spriteSheetImageData = new byte[256 * 256 * _numSpriteSheets / 2];//numspritesheets 256x256 4bpp bitmaps
            ImageHelper.Deflate(buff, _spriteSheetImageData);
        }

        //scrollscreen
        if (Header.ScrollScreenOffset != -1)
        {
            ScrollScreen = new ScrollScreen(br);
        }

        //read string table
        if (Header.StringTableOffset != -1)
        {
            br.BaseStream.Position = Offset + Header.StringTableOffset;
            Strings = new string[128];
            var stringOffsets = new short[128];

            for (var i = 0; i < 128; i++)
            {
                stringOffsets[i] = br.ReadInt16();
            }

            for (var i = 0; i < 128; i++)
            {
                if (stringOffsets[i] != -1)
                {
                    Strings[i] = "";
                    br.BaseStream.Position = Offset + Header.StringTableOffset + stringOffsets[i];
                    var c = br.ReadChar();
                    while (c != '\0')
                    {
                        Strings[i] += c;
                        c = br.ReadChar();
                    }

                    Strings[i] = TextDecoder.DecodeString(Strings[i]);
                }
            }

            Header.StringSize = (int)(br.BaseStream.Position - Offset) - Header.StringTableOffset;
        }
        //loaded = true;
    }

    private readonly Dictionary<long, Bitmap> _spriteCache = new();
    public Bitmap GetSpriteBitmap(SiImage img)
    {
        if (_spriteCache.TryGetValue(img.Signature, out var bitmap))
        {
            return bitmap;
        }

        var pal = SpriteInfo.Palettes[img.Palette]; // img.Palette & 0x1f;
        var bmp = GenerateSpriteBitmap(img, pal);
        _spriteCache.Add(img.Signature, bmp);

        return bmp;
    }

    public Bitmap GenerateSpriteBitmap(SiImage img, Color[] pal)
    {
        var shiftleft = img.Sx % 2 == 1;
        int swidth = img.Swidth;
        var readwidth = swidth;
        int outputwidth = img.Swidth;

        if (outputwidth % 8 > 0)//make output interval of 8
        {
            outputwidth += 8 - outputwidth % 8;
        }

        if (shiftleft)//make sure there is an extra byte if shifting left
        {
            readwidth++;
        }

        if (readwidth % 2 == 1)// or if odd width
        {
            readwidth++;
        }

        var buff = new byte[outputwidth * img.Sheight / 2];
        var readbuff = new byte[readwidth / 2];

        for (var y = 0; y < img.Sheight; y++)
        {
            Buffer.BlockCopy(_spriteSheetImageData, ((img.Spritesheet & 0x7) * 256 + img.Sy + y) * 256 / 2 + img.Sx / 2, readbuff, 0, readwidth / 2);

            if (shiftleft)
            {
                int dex;
                for (dex = 0; dex < readbuff.Length - 1; dex++)
                {
                    buff[y * outputwidth / 2 + dex] = (byte)((readbuff[dex] & 0xf0) >> 4 | (readbuff[dex + 1] & 0x0f) << 4);
                }

            }
            else
            {
                Buffer.BlockCopy(readbuff, 0, buff, y * outputwidth / 2, readwidth / 2);
            }

            if (swidth % 2 == 1)
            {
                buff[y * outputwidth / 2 + swidth / 2] = (byte)(buff[y * outputwidth / 2 + swidth / 2] & 0x0f);
            }

        }

        if (outputwidth > swidth)
        {
            img.Swidth = (byte)outputwidth;

            var vec1X = (img.X2 - img.X1) / (float)swidth;//get the normalized (normalized to ratio of swidth/outputwidth) vector of point 1
            var vec1Y = (img.Y2 - img.Y1) / (float)swidth;

            var vec3X = (img.X4 - img.X3) / (float)swidth;//get normalized vector of point 3
            var vec3Y = (img.Y4 - img.Y3) / (float)swidth;

            img.X2 = (sbyte)(img.X1 + vec1X * outputwidth);//extend point 2 to new width
            img.Y2 = (sbyte)(img.Y1 + vec1Y * outputwidth);

            img.X4 = (sbyte)(img.X3 + vec3X * outputwidth);//extend point 4 to new width
            img.Y4 = (sbyte)(img.Y3 + vec3Y * outputwidth);
        }

        return ImageHelper.BitmapFromPsxBuff(buff, outputwidth, img.Sheight, 4, pal);
    }

    private readonly Dictionary<long, Bitmap> _tileCache = new();
    public Bitmap GetTileBitmap(int tileMapIndex)
    {
        if (_tileCache.TryGetValue(tileMapIndex, out var bitmap))
        {
            return bitmap;
        }
        
        var tileId = tileMapIndex & 0x3ff;
        var paletteId = (tileMapIndex & 0xf000) >> 12;
        var bmp = GenerateTileBitmap(tileId, Info.Palettes[paletteId]);
        _tileCache.Add(tileMapIndex, bmp);

        return bmp;
    }

    public Bitmap GenerateTileBitmap(int tile, Color[] pal)
    {
        Debug.Assert(tile < 10 * StaticVariables.MapTileHeight * 6, "Bad tile index!", "unexpectedly large tile index of {0}", tile);

        var tileBuff = new byte[StaticVariables.MapTileWidth * StaticVariables.MapTileHeight * 4 / 8];
        var tileX = tile % 10 * StaticVariables.MapTileWidth;
        var tileY = tile / 10 * StaticVariables.MapTileHeight;

        if (tile < 10 * StaticVariables.MapTileHeight * 6)
        {
            for (var y = 0; y < StaticVariables.MapTileHeight; y++)
            {
                Buffer.BlockCopy(_tileSheetImageData, (tileY + y) * 256 / 2 + tileX / 2, tileBuff, y * StaticVariables.MapTileWidth / 2, StaticVariables.MapTileWidth / 2);
            }
        }

        return ImageHelper.BitmapFromPsxBuff(tileBuff, StaticVariables.MapTileWidth, StaticVariables.MapTileHeight, 4, pal);
    }

    public Bitmap GenerateTileSheetBmp(Color[] pal)
    {
        TileSheetBitmap = ImageHelper.BitmapFromPsxBuff(_tileSheetImageData, 256, 256 * 6, 4, pal);
        return TileSheetBitmap;
    }

    public Bitmap GenerateSpriteSheetBmp(Color[] pal)
    {
        SpriteSheetBitmap = ImageHelper.BitmapFromPsxBuff(_spriteSheetImageData, 256, 256 * _numSpriteSheets, 4, pal);
        return SpriteSheetBitmap;
    }

    public static readonly int EventObjectsMemoryAddress = 0x1ac498;// + 0x260;
    public static readonly int EventObjectSize = 0x294;

    public static int EventObjectAddr(int eventobjectid)
    {
        return EventObjectsMemoryAddress + eventobjectid * EventObjectSize;
    }
}