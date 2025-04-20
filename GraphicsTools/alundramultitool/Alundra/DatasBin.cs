using System.Diagnostics;

namespace GraphicsTools.Alundra
{
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

    public class GameMap
    {
        public GameMap(BinaryReader br, DbHeader dbheader)
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
            br.BaseStream.Position = Offset + Header.InfoBlock;
            Info = new GameMapInfo(br.ReadInt32(), Memaddr + Header.InfoBlock);
        }

        public readonly long Offset;
        public static readonly int Memaddr = 0x153460;// + 0x260; Header size ??


        public readonly GameMapHeader Header;
        public GameMapInfo Info;
        public SpriteInfo Spriteinfo;
        public ScrollScreen ScrollScreen;
        public Map Map;
        public string[] Strings;
        public readonly bool Loaded = false;
        private byte[] _tilesheetimagedata;
        public Bitmap Tilesheetbmp;
        private byte[] _spritesheetimagedata;
        public Bitmap Spritesheetbmp;
        private int _numspritesheets = 8;

        public void Load(BinaryReader br, bool ismap)
        {
            //read info
            if (Header.InfoBlock != -1)
            {
                br.BaseStream.Position = Offset + Header.InfoBlock;
                Info = new GameMapInfo(br, Memaddr + Header.InfoBlock);
            }

            //map
            if (Header.MapBlock != -1)
            {
                br.BaseStream.Position = Offset + Header.MapBlock;
                Map = new Map(br, Memaddr + Header.MapBlock);
                Header.WallTilesSize = Header.TileSheets - (Map.WallTilesOffset + Header.MapBlock);
                Header.Mapsize -= Header.WallTilesSize;
            }

            //tilesheet
            if (Header.TileSheets != -1)
            {
                br.BaseStream.Position = Offset + Header.TileSheets + 6;
                var buff = new byte[Header.SpriteInfo - Header.TileSheets];
                br.Read(buff, 0, buff.Length);
                _tilesheetimagedata = new byte[256 * 256 * 6 / 2];//6 256x256 4bpp bitmaps
                Utils.Deflate(buff, _tilesheetimagedata);
            }

            //spriteinfo
            if (Header.SpriteInfo != -1)
            {
                br.BaseStream.Position = Offset + Header.SpriteInfo;
                Spriteinfo = new SpriteInfo(br, Memaddr + Header.SpriteInfo, Header.SpriteSheets, ismap);
            }

            //spritesheet
            if (Header.SpriteSheets != -1)
            {
                br.BaseStream.Position = Offset + Header.SpriteSheets + 6;
                var buff = new byte[Header.Spritessize - 6];
                br.Read(buff, 0, buff.Length);
                _spritesheetimagedata = new byte[256 * 256 * _numspritesheets / 2];//numspritesheets 256x256 4bpp bitmaps
                Utils.Deflate(buff, _spritesheetimagedata);
            }

            //scrollscreen
            if (Header.ScrollScreen != -1)
            {
                ScrollScreen = new ScrollScreen(br);
            }

            //read string table
            if (Header.StringTable != -1)
            {
                br.BaseStream.Position = Offset + Header.StringTable;
                Strings = new string[128];
                var stringoffsets = new short[128];
                for (var i = 0; i < 128; i++)
                {
                    stringoffsets[i] = br.ReadInt16();
                }
                for (var i = 0; i < 128; i++)
                {
                    if (stringoffsets[i] != -1)
                    {
                        Strings[i] = "";
                        br.BaseStream.Position = Offset + Header.StringTable + stringoffsets[i];
                        var c = br.ReadChar();
                        while (c != '\0')
                        {
                            Strings[i] += c;
                            c = br.ReadChar();
                        }

                        Strings[i] = TextInterpreter.DecodeString(Strings[i]);
                    }
                }

                Header.Stringsize = (int)(br.BaseStream.Position - Offset) - Header.StringTable;
            }
            //loaded = true;
        }

        private Dictionary<long, Bitmap> _spriteCache = new();
        public Bitmap GetSpriteBitmap(SiImage img)
        {
            var pal = Spriteinfo.Palettes[img.Palette & 0x1f];
            if (_spriteCache.ContainsKey(img.Signature))
            {
                return _spriteCache[img.Signature];
            }

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

            if (shiftleft)//make sure theres an extra byte if shifting left
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
                Buffer.BlockCopy(_spritesheetimagedata, ((img.Spritesheet & 0x7) * 256 + img.Sy + y) * 256 / 2 + img.Sx / 2, readbuff, 0, readwidth / 2);

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

            return Utils.BitmapFromPsxBuff(buff, outputwidth, img.Sheight, 4, pal);
        }

        private Dictionary<long, Bitmap> _tileCache = new();
        public Bitmap GetTileBitmap(int tileid)
        {
            var tiledex = tileid & 0x3ff;
            var paldex = (tileid & 0xf000) >> 12;
            if (_tileCache.ContainsKey(tileid))
            {
                return _tileCache[tileid];
            }

            var bmp = GenerateTileBitmap(tiledex, Info.Palettes[paldex]);
            _tileCache.Add(tileid, bmp);

            return bmp;
        }

        public Bitmap GenerateTileBitmap(int tile, Color[] pal)
        {
            Debug.Assert(tile < 10 * 16 * 6, "Bad tile index!", "unexpectedly large tile index of {0}", tile);
            var tilebuff = new byte[24 * 16 * 4 / 8];
            var tilex = tile % 10 * 24;
            var tiley = tile / 10 * 16;
            if (tile < 10 * 16 * 6)
            {
                for (var y = 0; y < 16; y++)
                    Buffer.BlockCopy(_tilesheetimagedata, (tiley + y) * 256 / 2 + tilex / 2, tilebuff, y * 24 / 2, 24 / 2);
            }
            return Utils.BitmapFromPsxBuff(tilebuff, 24, 16, 4, pal);
        }

        public Bitmap GenerateTileSheetBmp(Color[] pal)
        {
            Tilesheetbmp = Utils.BitmapFromPsxBuff(_tilesheetimagedata, 256, 256 * 6, 4, pal);
            return Tilesheetbmp;
        }

        public Bitmap GenerateSpriteSheetBmp(Color[] pal)
        {
            Spritesheetbmp = Utils.BitmapFromPsxBuff(_spritesheetimagedata, 256, 256 * _numspritesheets, 4, pal);
            return Spritesheetbmp;
        }

        public static readonly int EventobjectsMemaddr = 0x1ac498;// + 0x260;
        public static readonly int EventobjectSize = 0x294;

        public static int EventObjectAddr(int eventobjectid)
        {
            return EventobjectsMemaddr + eventobjectid * EventobjectSize;
        }
    }

    public class Map
    {
        public Map(BinaryReader br, int memaddr)
        {
            Memaddr = memaddr;
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
        public readonly int Memaddr;

        public readonly int Width;
        public readonly int Height;
        public readonly int Width2;
        public readonly int Height2;

        public readonly int WallTilesOffset;

        public readonly MapTile[] MapTiles;

    }

    public class MapTile
    {
        public MapTile(BinaryReader br)
        {
            long i = br.ReadUInt32();
            Walkability = (byte)(i & 0xff);
            i >>= 8;
            GroundProperty = (byte)(i & 0xff);
            i >>= 8;
            Slope = (byte)(i & 0xff);
            i >>= 8;
            Height = (byte)(i & 0xff);

            i = br.ReadUInt16();
            TileId = (short)i;
            if (i == 0xffff)
            {
                Palette = -1;
                Tile = -1;
            }
            else
            {
                Palette = (short)((i & 0xf000) >> 12);
                Tile = (short)(i & 0x3ff);
            }
            TilesOffset = br.ReadInt16();
            if (TilesOffset != -1)
            {
                TilesOffset *= 2;
            }
        }
        public byte Walkability;
        public byte GroundProperty;
        public readonly byte Slope;
        public readonly byte Height;
        public readonly short TileId;
        public short Palette;
        public short Tile;
        public readonly short TilesOffset;
        public WallTiles WallTiles;
        public void LoadWallTiles(BinaryReader br, long offset)
        {
            if (TilesOffset != -1)
            {
                br.BaseStream.Position = offset + TilesOffset;
                WallTiles = new WallTiles(br);
            }
        }
    }

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

    public class ScrollScreen
    {
        public ScrollScreen(BinaryReader br)
        {
            Unknown1 = br.ReadInt32();
            Unknown2 = br.ReadInt32();
            Unknown3 = br.ReadInt32();
            Unknown4 = br.ReadInt32();
            Unknown5 = br.ReadInt32();
            Unknown6 = br.ReadInt32();
            Unknown7 = br.ReadInt32();
            Unknown8 = br.ReadInt32();
        }
        public readonly int Unknown1;
        public readonly int Unknown2;
        public readonly int Unknown3;
        public readonly int Unknown4;
        public readonly int Unknown5;
        public readonly int Unknown6;
        public readonly int Unknown7;
        public readonly int Unknown8;
    }

    public class SpriteInfo
    {
        public SpriteInfo(BinaryReader br, int memaddr, int sectorend, bool ismap)
        {
            _binoffset = br.BaseStream.Position;

            Header = new SpriteInfoHeader(br, memaddr);

            //read sprite table
            br.BaseStream.Position = _binoffset + Header.Spritetablepointer;
            SpriteTable = new int[0xff];
            for (var dex = 0; dex < SpriteTable.Length; dex++)
            {
                SpriteTable[dex] = br.ReadInt32();
            }

            br.BaseStream.Position = _binoffset + Header.Spriteeffectspointer;
            SpriteEffectTable = new int[0xff];
            for (var i = 0; i < SpriteEffectTable.Length; i++)
            {
                SpriteEffectTable[i] = br.ReadInt32();
                if (SpriteEffectTable[i] == 0)
                {
                    NumSpriteEffects = i;
                    break;
                }
            }

            //event effects
            br.BaseStream.Position = _binoffset + Header.Mapeffectsector3Pointer;
            MapEffectRecords = new MapEffectRecord[Header.Mapeffectsector3Size / 12];
            for (var i = 0; i < MapEffectRecords.Length; i++)
            {
                MapEffectRecords[i] = new MapEffectRecord(br);
            }

            //read palettes
            br.BaseStream.Position = _binoffset + Header.Spritepalettespointer;
            var maxpalettes = 32;
            Palettes = new Color[maxpalettes][];
            var buff = new byte[maxpalettes * 16 * 2];
            br.Read(buff, 0, buff.Length);
            var buffdex = 0;
            for (var dex = 0; dex < maxpalettes; dex++)
            {
                Palettes[dex] = new Color[16];
                for (var cdex = 0; cdex < 16; cdex++)
                {
                    var b2 = buff[buffdex++];
                    var b1 = buff[buffdex++];
                    Palettes[dex][cdex] = Utils.FromPsxColor((b1 << 8) | b2);
                }
            }
            Palettesbitmap = Utils.BitmapFromPsxBuff(buff, 16, maxpalettes, 16, null);

            //read eventcodes
            EventCodes = new SpriteInfoEventCodes(br, _binoffset, Header, ismap);

            //read entities
            br.BaseStream.Position = _binoffset + Header.Entitiespointer;
            Entities = new SpriteInfoEntities(br, memaddr + Header.Entitiespointer);

            //read mapevents
            br.BaseStream.Position = _binoffset + Header.Mapeventspointer;
            MapEvents = new SpriteInfoMapEvents(br, _binoffset, sectorend);

            //read sprite table records;
            Sprites = new SpriteRecord[SpriteTable.Length];
            for (var dex = 0; dex < Sprites.Length; dex++)
            {
                if (SpriteTable[dex] != -1)
                {
                    br.BaseStream.Position = _binoffset + SpriteTable[dex];
                    Sprites[dex] = new SpriteRecord(br, _binoffset, dex, memaddr + SpriteTable[dex], memaddr);
                }
            }

            Spriteeffects = new SpriteEffectRecord[NumSpriteEffects];
            for (var dex = 0; dex < Spriteeffects.Length; dex++)
            {
                if (SpriteEffectTable[dex] != -1)
                {
                    br.BaseStream.Position = _binoffset + SpriteEffectTable[dex];
                    Spriteeffects[dex] = new SpriteEffectRecord(br, _binoffset + SpriteEffectTable[dex], dex | 0x8000, memaddr + SpriteEffectTable[dex], memaddr + SpriteEffectTable[dex]);
                }
            }
        }

        public readonly SpriteInfoHeader Header;
        public readonly SpriteInfoEventCodes EventCodes;
        public readonly SpriteInfoEntities Entities;
        public readonly SpriteInfoMapEvents MapEvents;

        public readonly int[] SpriteTable;
        public readonly SpriteRecord[] Sprites;

        public readonly int[] SpriteEffectTable;
        public readonly SpriteEffectRecord[] Spriteeffects;
        public readonly int NumSpriteEffects;

        public readonly MapEffectRecord[] MapEffectRecords;

        private long _binoffset;

        public readonly Color[][] Palettes;
        public readonly Bitmap Palettesbitmap;
    }

    public class SpriteRecord
    {
        public SpriteRecord(BinaryReader br, long binoffset, int id, int memaddr, int spriteinfomemaddr)
        {

            Header = new SpriteTableHeader(br, binoffset, id, memaddr, spriteinfomemaddr);
            Animsets = new SiAnimSet[(Header.Animationspointer - Header.Animationoffsetspointer) / 14];
            for (var dex = 0; dex < Animsets.Length; dex++)
            {
                Animsets[dex] = new SiAnimSet(br, memaddr + 32 + dex * 14);
            }

            //preload all of the animations here
            int animdex;
            for (animdex = 0; animdex < Animsets.Length; animdex++)
            {
                int dirdex;
                for (dirdex = 0; dirdex < 4; dirdex++)
                {
                    if (Animsets[animdex].Animoffsets[dirdex] != 0xffff)
                    {
                        Animsets[animdex].PreloadedAnims[dirdex] = GetAnimation(br, Animsets[animdex].Animoffsets[dirdex]);

                        /*DBFrame* frames = (DBFrame*)&(*spr->framesdata)[spr->animsets[animdex].diroffsets[dirdex]];
                        int framedex;
                        for (framedex = 0; framedex < 32; framedex++)
                        {
                            DBFrame* frame = &frames[framedex];
                            if ((frame->delay & 0x80) != 0x80)
                                break;

                            DBImageSet* imageset = (DBImageSet*)&(*spr->imagesetdata)[flipu16(frame->imagesetoffset) << 1];

                            int imagedex;
                            for (imagedex = 0; imagedex < imageset->numimages; imagedex++)
                            {
                                numimages++;
                                if (dex > 0)
                                    cache_image(&imageset->images[imagedex], 1);
                            }

                        }*/

                    }
                }

            }

        }

        public SiAnimation GetAnimation(BinaryReader br, int animationoffset)
        {
            br.BaseStream.Position = Header.Binoffset + Header.Animationspointer + animationoffset;

            var anim = new SiAnimation(br, Header, Header.Spriteinfomemaddr + Header.Animationspointer + animationoffset);

            return anim;
        }

        public SiImageSet GetPortraitImageset(BinaryReader br)
        {
            var savepos = br.BaseStream.Position;
            var imagesetpointer = 0;//(its the first one)
            br.BaseStream.Position = Header.Binoffset + Header.Framespointer + 0;
            var imageset = new SiImageSet(br, Header.Sector5Id << 16 | imagesetpointer, Header.Spriteinfomemaddr + Header.Framespointer + imagesetpointer, true);

            br.BaseStream.Position = savepos;
            return imageset;
        }

        public readonly SpriteTableHeader Header;
        public readonly SiAnimSet[] Animsets;

    }

    public class SpriteEffectRecord
    {
        public SpriteEffectRecord(BinaryReader br, long binoffset, int id, int memaddr, int spriteinfomemaddr)
        {
            _effectid = id;
            _binoffset = binoffset;
            _spriteinfomemaddr = spriteinfomemaddr;

            _animoffsets = new int[255];
            var final = -1;
            for (var dex = 0; dex < _animoffsets.Length; dex++)
            {
                if (final != -1 && dex >= final)
                {
                    _numanims = dex;
                    break;
                }
                _animoffsets[dex] = br.ReadInt16();
                if (final == -1)
                {
                    final = _animoffsets[dex] / 2;
                }

            }
            //preload all of the animations here
            int animdex;
            PreloadedAnims = new SiEffectAnimation[_numanims];
            for (animdex = 0; animdex < _numanims; animdex++)
            {
                PreloadedAnims[animdex] = GetAnimation(br, _animoffsets[animdex]);
            }

        }

        private long _binoffset;
        private int _spriteinfomemaddr;
        private int[] _animoffsets;
        private int _numanims;
        private int _effectid;
        public readonly SiEffectAnimation[] PreloadedAnims;

        public SiEffectAnimation GetAnimation(BinaryReader br, int animationoffset)
        {
            br.BaseStream.Position = _binoffset + animationoffset;

            var anim = new SiEffectAnimation(br, _effectid, (int)_binoffset, _spriteinfomemaddr + animationoffset);

            return anim;
        }

    }

    public class SiAnimSet
    {
        public SiAnimSet(BinaryReader br, int memaddr)
        {
            Memaddr = memaddr;
            Animoffsets = new int[4];
            for (var dex = 0; dex < Animoffsets.Length; dex++)
                Animoffsets[dex] = br.ReadInt16();
            Speed = br.ReadUInt16();
            Sfx = br.ReadByte();
            Flags = br.ReadByte();
            Acceleration = br.ReadByte();
            U6 = br.ReadByte();
            PreloadedAnims = new SiAnimation[4];
        }
        public readonly int Memaddr;
        public readonly int[] Animoffsets;//4 of them for each direction
        public readonly SiAnimation[] PreloadedAnims;
        public readonly ushort Speed;
        public readonly byte Sfx;
        public readonly byte Flags;//0x80 adds 0x100 to sfx, does it mean global or map sfx?
        public readonly byte Acceleration;
        public readonly byte U6;

        public int Downoffset { get { return Animoffsets[(int)SiAnimDir.Down]; } }
        public int Upoffset { get { return Animoffsets[(int)SiAnimDir.Up]; } }
        public int Leftoffset { get { return Animoffsets[(int)SiAnimDir.Left]; } }
        public int Rightoffset { get { return Animoffsets[(int)SiAnimDir.Right]; } }
    }
    
    public enum SiAnimDir
    {
        Down = 0,
        Up = 1,
        Left = 2,
        Right = 3
    }

    public class SpriteTableHeader
    {
        public SpriteTableHeader(BinaryReader br, long binoffset, int id, int memaddr, int spriteinfomemaddr)
        {
            Spriteinfomemaddr = spriteinfomemaddr;
            Memaddr = memaddr;
            Sector5Id = id;
            Binoffset = binoffset;
            Animationoffsetspointer = br.ReadInt32();
            Animationspointer = br.ReadInt32();
            Framecollisionpointer = br.ReadInt32();
            Framespointer = br.ReadInt32();
            Ubuff = new byte[16];
            br.Read(Ubuff, 0, Ubuff.Length);

            br.BaseStream.Position -= 16;
            Moreflags = br.ReadByte();//10
            Canpickup = br.ReadByte();//11
            FlagsPortraitShadowtype = br.ReadByte();//12
            ProgramLoad = br.ReadByte();//13
            ProgramTick = br.ReadByte();//14
            ProgramTouch = br.ReadByte();//15
            ProgramDeactivate = br.ReadByte();//16
            ProgramInteract = br.ReadByte();//17
            Xmod = br.ReadSByte();//18+0
            Ymod = br.ReadSByte();//18+1
            Zmod = br.ReadSByte();//18+2
            Width = br.ReadByte();//18+3
            Depth = br.ReadByte();//18+4
            Height = br.ReadByte();//18+5
            Breakeffect = br.ReadByte();//18+6
            Contents = br.ReadByte();//18+7
        }
        public readonly int Sector5Id;
        public readonly long Binoffset;
        public readonly int Memaddr;
        public readonly int Spriteinfomemaddr;

        public readonly int Animationoffsetspointer;
        public readonly int Animationspointer;
        public readonly int Framecollisionpointer;
        public readonly int Framespointer;
        public readonly byte[] Ubuff;

        public readonly byte Moreflags;
        public readonly byte Canpickup;
        public readonly byte FlagsPortraitShadowtype;
        public readonly byte ProgramLoad;
        public readonly byte ProgramTick;
        public readonly byte ProgramTouch;
        public readonly byte ProgramDeactivate;
        public readonly byte ProgramInteract;
        public readonly sbyte Xmod;
        public readonly sbyte Ymod;
        public readonly sbyte Zmod;
        public readonly byte Width;
        public readonly byte Depth;
        public readonly byte Height;
        public readonly byte Breakeffect;
        public readonly byte Contents;
    }

    public class SiEffectAnimation
    {
        public SiEffectAnimation(BinaryReader br, int effectid, int binoffset, int memaddr)
        {
            Memaddr = memaddr;
            Frames = new SiEffectFrame[32];//32 max frames?
            for (var dex = 0; dex < Frames.Length; dex++)
            {
                //read test bytes to check for the end of the list
                short test = br.ReadByte();
                if ((test & 0x80) != 0x80)
                {
                    break;
                }

                Numframes++;
                br.BaseStream.Position -= 1;

                Frames[dex] = new SiEffectFrame(br, effectid, binoffset, memaddr + dex * 3);

                for (var dex2 = 0; dex2 < dex; dex2++)
                {
                    if (Frames[dex2].Imagesetpointer == Frames[dex].Imagesetpointer)
                    {
                        Frames[dex].Images = Frames[dex2].Images;
                        break;
                    }
                }
            }
        }
        public int Memaddr;
        public int Numframes;
        public readonly SiEffectFrame[] Frames;
    }

    public class SiAnimation
    {
        public SiAnimation(BinaryReader br, SpriteTableHeader header, int memaddr)
        {
            Memaddr = memaddr;
            Frames = new SiFrame[32];//32 max frames?
            for (var dex = 0; dex < Frames.Length; dex++)
            {
                //read two test bytes to check for the end of the list
                short test = br.ReadByte();
                if ((test & 0x80) != 0x80)
                {
                    break;
                }

                Numframes++;
                br.BaseStream.Position -= 1;

                Frames[dex] = new SiFrame(br, header, memaddr + dex * 5);

                for (var dex2 = 0; dex2 < dex; dex2++)
                {
                    if (Frames[dex2].Imagesetpointer == Frames[dex].Imagesetpointer)
                    {
                        Frames[dex].Images = Frames[dex2].Images;
                        break;
                    }
                }
            }
        }
        public readonly int Memaddr;
        public readonly int Numframes;
        public readonly SiFrame[] Frames;
    }

    public class SiFrame
    {
        public SiFrame(BinaryReader br, SpriteTableHeader header, int memaddr)
        {
            Memaddr = memaddr;
            Delay = br.ReadByte();

            Collisionoffset = br.ReadInt16();
            Imagesetpointer = br.ReadUInt16() * 2;

            //load images
            var savepos = br.BaseStream.Position;

            br.BaseStream.Position = header.Binoffset + header.Framespointer + Imagesetpointer;
            Images = new SiImageSet(br, header.Sector5Id << 16 | Imagesetpointer, header.Spriteinfomemaddr + header.Framespointer + Imagesetpointer);

            if (Collisionoffset != -1)
            {
                br.BaseStream.Position = header.Binoffset + header.Framecollisionpointer + Collisionoffset;
                CollisionData = new FrameCollisionData(br);
            }

            br.BaseStream.Position = savepos;
        }

        public readonly FrameCollisionData CollisionData;
        public readonly int Memaddr;
        public readonly byte Delay;//top bit masked
        public readonly short Collisionoffset;//-1
        public readonly int Imagesetpointer;
        public SiImageSet Images;
    }

    public class FrameCollisionData
    {
        public FrameCollisionData(BinaryReader br)
        {
            XOff = br.ReadByte();
            YOff = br.ReadByte();
            ZOff = br.ReadByte();
            Width = br.ReadByte();
            Depth = br.ReadByte();
            Height = br.ReadByte();
        }
        public readonly byte XOff;
        public readonly byte YOff;
        public readonly byte ZOff;
        public readonly byte Width;
        public readonly byte Depth;
        public readonly byte Height;
    }

    public class SiEffectFrame
    {
        public SiEffectFrame(BinaryReader br, int effectid, int binoffset, int memaddr)
        {
            Memaddr = memaddr;
            Delay = br.ReadByte();
            Imagesetpointer = br.ReadUInt16() * 2;

            //load images
            var savepos = br.BaseStream.Position;

            br.BaseStream.Position = binoffset + Imagesetpointer;
            Images = new SiImageSet(br, effectid << 16 | Imagesetpointer, memaddr + Imagesetpointer);

            br.BaseStream.Position = savepos;
        }

        public int Memaddr;
        public readonly byte Delay;//top bit masked
        public short Unknown;//-1
        public readonly int Imagesetpointer;
        public SiImageSet Images;
    }

    public class SiImageSet
    {
        public SiImageSet(BinaryReader br, int imagesetid, int memaddr, bool isportrait = false)
        {
            Memaddr = memaddr;
            Imagesetid = imagesetid;
            Unknown = br.ReadByte();//palette?
            Numimages = br.ReadByte();
            if (isportrait)
            {
                Numimages = 1;
            }

            Images = new SiImage[Numimages];
            for (var dex = 0; dex < Numimages; dex++)
            {
                Images[dex] = new SiImage(br);
            }
        }

        public readonly int Memaddr;
        public readonly int Imagesetid;
        public readonly byte Unknown;
        public readonly byte Numimages;
        public readonly SiImage[] Images;
    }

    public class SiImage
    {
        public readonly long Signature;
        public SiImage(BinaryReader br)
        {
            Spritesheet = br.ReadByte();
            Palette = br.ReadByte();
            Sx = br.ReadByte();
            Sy = br.ReadByte();
            Swidth = br.ReadByte();
            Sheight = br.ReadByte();
            X1 = br.ReadSByte();
            Y1 = br.ReadSByte();
            X2 = br.ReadSByte();
            Y2 = br.ReadSByte();
            X3 = br.ReadSByte();
            Y3 = br.ReadSByte();
            X4 = br.ReadSByte();
            Y4 = br.ReadSByte();

            Signature = Spritesheet | Palette << 8 | Sx << 16 | Sy << 24 | Swidth << 32 | Sheight << 38;
            /*if (rejigger)
            {//byte align
                if (sx % 2 == 1)
                {
                    sx--;
                    swidth++;
                    if (x2 > x1)
                        x1--;
                    else
                        x1++;
                    if (x4 > x3)
                        x3--;
                    else
                        x3++;
                }

                if (swidth % 2 == 1)
                {
                    swidth++;
                    if (x2 > x1)
                        x2++;
                    else
                        x2--;
                    if (x4 > x3)
                        x4++;
                    else
                        x4--;
                }
            }*/
        }

        public readonly byte Spritesheet;
        public readonly byte Palette;
        public readonly byte Sx;
        public readonly byte Sy;
        public byte Swidth;
        public readonly byte Sheight;
        public readonly sbyte X1;
        public readonly sbyte Y1;
        public sbyte X2;
        public sbyte Y2;
        public readonly sbyte X3;
        public readonly sbyte Y3;
        public sbyte X4;
        public sbyte Y4;
    }

    public class SpriteInfoHeader
    {
        public SpriteInfoHeader(BinaryReader br, int memaddr)
        {
            Entitiespointer = br.ReadInt32();
            Mapeffectsector3Pointer = br.ReadInt32();
            Mapeventspointer = br.ReadInt32();
            Spritetablepointer = br.ReadInt32();
            Spriteeffectspointer = br.ReadInt32();
            Spritepalettespointer = br.ReadInt32();
            Eventcodesapointer = br.ReadInt32();
            Eventcodesbpointer = br.ReadInt32();
            Eventcodescpointer = br.ReadInt32();
            Eventcodesdpointer = br.ReadInt32();
            Eventcodesepointer = br.ReadInt32();
            Eventcodesfpointer = br.ReadInt32();

            Memaddr = memaddr;
            Eventcodeaddr = memaddr + Eventcodesapointer;

            Entitiessize = Mapeffectsector3Pointer - Entitiespointer;
            Mapeffectsector3Size = Mapeventspointer - Mapeffectsector3Pointer;
            Mapeventssize = -1;// unknown4 - unknown3;
            Spritetablesize = Spriteeffectspointer - Spritetablepointer;
            Spriteeffectssize = Spritepalettespointer - Spriteeffectspointer;
            Spritepalettessize = Eventcodesapointer - Spritepalettespointer;
            Eventcodesasize = Eventcodesbpointer - Eventcodesapointer;
            Eventcodesbsize = Eventcodescpointer - Eventcodesbpointer;
            Eventcodescsize = Eventcodesdpointer - Eventcodescpointer;
            Eventcodesdsize = Eventcodesepointer - Eventcodesdpointer;
            Eventcodesesize = Eventcodesfpointer - Eventcodesepointer;
            Eventcodesfandremainingsize = Entitiespointer - Eventcodesfpointer;
        }
        public readonly int Memaddr;
        public readonly int Eventcodeaddr;

        public readonly int Entitiespointer;
        public readonly int Entitiessize;
        public readonly int Mapeffectsector3Pointer;
        public readonly int Mapeffectsector3Size;
        public readonly int Mapeventspointer;
        public readonly int Mapeventssize;
        public readonly int Spritetablepointer;
        public readonly int Spritetablesize;
        public readonly int Spriteeffectspointer;//0000333b000e240e0400000000000000
        public readonly int Spriteeffectssize;
        public readonly int Spritepalettespointer;
        public readonly int Spritepalettessize;
        public readonly int Eventcodesapointer;
        public readonly int Eventcodesasize;
        public readonly int Eventcodesbpointer;
        public readonly int Eventcodesbsize;
        public readonly int Eventcodescpointer;
        public readonly int Eventcodescsize;
        public readonly int Eventcodesdpointer;
        public readonly int Eventcodesdsize;
        public readonly int Eventcodesepointer;
        public readonly int Eventcodesesize;
        public readonly int Eventcodesfpointer;
        public int Eventcodesfsize;//calced when reading sector1
        public readonly int Eventcodesfandremainingsize;
    }

    public class SpriteInfoEventCodes
    {
        public static readonly byte[] Code = new byte[1024 * 1024];//1mb of event codes, too much prob but oh well;

        public SpriteInfoEventCodes(BinaryReader br, long binoffset, SpriteInfoHeader header, bool ismap)
        {
            var tableSize = 0;
            short firstoffset = 0;

            //read sector1a
            br.BaseStream.Position = binoffset + header.Eventcodesapointer;
            tableSize = header.Eventcodesasize / 2;
            Eventcodesatable = new short[tableSize];
            for (var dex = 0; dex < tableSize; dex++)
            {
                Eventcodesatable[dex] = br.ReadInt16();
                if (firstoffset == 0 && Eventcodesatable[dex] != 0)
                {
                    firstoffset = Eventcodesatable[dex];
                }
            }

            //read sector1b
            br.BaseStream.Position = binoffset + header.Eventcodesbpointer;
            tableSize = header.Eventcodesbsize / 2;
            Eventcodesbtable = new short[tableSize];
            for (var dex = 0; dex < tableSize; dex++)
            {
                Eventcodesbtable[dex] = br.ReadInt16();
                if (firstoffset == 0 && Eventcodesbtable[dex] != 0)
                {
                    firstoffset = Eventcodesbtable[dex];
                }
            }

            //read sector1c
            br.BaseStream.Position = binoffset + header.Eventcodescpointer;
            tableSize = header.Eventcodescsize / 2;
            Eventcodesctable = new short[tableSize];
            for (var dex = 0; dex < tableSize; dex++)
            {
                Eventcodesctable[dex] = br.ReadInt16();
                if (firstoffset == 0 && Eventcodesctable[dex] != 0)
                {
                    firstoffset = Eventcodesctable[dex];
                }
            }

            //read sector1d
            br.BaseStream.Position = binoffset + header.Eventcodesdpointer;
            tableSize = header.Eventcodesdsize / 2;
            Eventcodesdtable = new short[tableSize];
            for (var dex = 0; dex < tableSize; dex++)
            {
                Eventcodesdtable[dex] = br.ReadInt16();
                if (firstoffset == 0 && Eventcodesdtable[dex] != 0)
                {
                    firstoffset = Eventcodesdtable[dex];
                }
            }

            //read sector1e
            br.BaseStream.Position = binoffset + header.Eventcodesepointer;
            tableSize = header.Eventcodesesize / 2;
            Eventcodesetable = new short[tableSize];
            for (var dex = 0; dex < tableSize; dex++)
            {
                Eventcodesetable[dex] = br.ReadInt16();
                if (firstoffset == 0 && Eventcodesetable[dex] != 0)
                {
                    firstoffset = Eventcodesetable[dex];
                }
            }

            //read sector1f
            header.Eventcodesfsize = header.Eventcodesapointer + firstoffset - header.Eventcodesfpointer;
            br.BaseStream.Position = binoffset + header.Eventcodesfpointer;
            tableSize = header.Eventcodesfsize / 2;
            if (tableSize < 0)
            {
                tableSize = 16;
            }

            Eventcodesftable = new short[tableSize];
            for (var dex = 0; dex < tableSize; dex++)
            {
                Eventcodesftable[dex] = br.ReadInt16();
            }

            //set binoffset for eventcodes
            _binoffset = binoffset + header.Eventcodesapointer;
            _memaddr = header.Memaddr + header.Eventcodesapointer;
            _datasize = header.Entitiespointer - header.Eventcodesapointer;

            Eventcodestable.Add(Eventcodesatable);
            Eventcodestable.Add(Eventcodesbtable);
            Eventcodestable.Add(Eventcodesctable);
            Eventcodestable.Add(Eventcodesdtable);
            Eventcodestable.Add(Eventcodesetable);
            Eventcodestable.Add(Eventcodesftable);

            var top = 0;
            if (ismap)
            {
                top += 1024 * 512;
            }

            br.BaseStream.Position = binoffset;
            if (_datasize > 0)
            {
                br.Read(Code, top, _datasize);
            }
            //half mb for global codes, half mb for map codes
        }

        public class SiCode
        {
            public byte Code { get; set; }
            public string Name { get; set; }
            public int Size { get; set; }
        }
        public static SiCode GetCode(byte b)
        {
            var size = 1;
            var name = "";
            switch (b)
            {
                case 0x02:
                    name = "goto";//skips forward or back
                    size = 3;
                    break;
                case 0x03:
                    name = "iffalse";
                    size = 3;
                    break;
                case 0x04:
                    name = "whilefalse";//loops back until condition met
                    size = 3;
                    break;
                case 0x05:
                    name = "flagon";//turn on a logic switch
                    size = 3;
                    break;
                case 0x06:
                    name = "flagoff";//turn off a logic switch
                    size = 3;
                    break;
                case 0x07:
                    name = "checkentityinarea";
                    size = 8;
                    break;
                case 0x08:
                    name = "turn";//turn direction
                    size = 2;
                    break;
                case 0x09:
                    name = "setdir";//set direction
                    size = 2;
                    break;
                case 0x0a:
                    name = "reverse";//switch direction, used for paceing npcs
                    size = 1;
                    break;
                case 0x0b:
                    name = "animwaitdistance";
                    size = 4;
                    break;
                case 0x0c:
                    name = "setdirectionwithmath";
                    size = 1;
                    break;
                case 0x0d:
                    name = "dialog";//show dialog
                    size = 3;
                    break;
                case 0x10:
                    name = "losecontrol";
                    size = 1;
                    break;
                case 0x11:
                    name = "gaincontrol";
                    size = 1;
                    break;
                case 0x12:
                    name = "playsound1";//only 1 byte sound index
                    size = 2;
                    break;
                case 0x15:
                    name = "resetzpos";
                    size = 1;
                    break;
                case 0x16:
                    name = "highgravity";//fall as normal  //bit 0x100
                    size = 1;
                    break;
                case 0x17:
                    name = "lowgravity";//used for climbing ladders and flying
                    size = 1;
                    break;

                case 0x19:
                    name = "deactivate?";
                    size = 1;
                    break;
                case 0x1a:
                    name = "setanim";//set animation
                    size = 2;
                    break;

                //CURRENT IMPLEMENTATION PROGRESS
                case 0x1b:
                    name = "fly"; //stop flying 0x0000   flying down 0xff7f     flying foward and up  0x0380
                    size = 3;
                    break;
                case 0x1c:
                    name = "waitanim?";
                    size = 2;
                    break;
                case 0x1d:
                    name = "waitanim2";
                    size = 2;
                    break;
                case 0x1e:
                    name = "walk";//collision blocks/pauses the walk
                    size = 3;
                    break;
                case 0x1f:
                    name = "walk2";//collision ends the walk
                    size = 3;
                    break;
                case 0x24:
                    name = "waitforceadjusted";//waits until force adjust is > 0
                    size = 1;
                    break;
                case 0x25:
                    name = "waitentitycollisionzor144";
                    size = 1;
                    break;
                case 0x26:
                    name = "waitforceadjustedorentitycollisionz";
                    size = 1;
                    break;
                case 0x27:
                    name = "faceplayer";
                    size = 1;
                    break;
                case 0x28:
                    name = "gravityflag2on";//bit 0x8
                    size = 1;
                    break;
                case 0x29:
                    name = "gravityflag2off";//bit 0x8
                    size = 1;
                    break;
                case 0x2a:
                    name = "gravityflag3on";//bit 0x1
                    size = 1;
                    break;
                case 0x2b:
                    name = "gravityflag3off";//bit 0x1
                    size = 1;
                    break;
                case 0x2d:
                    name = "activateentity";//look into this event to study entity type
                    size = 2;
                    break;
                case 0x2e:
                    name = "hide";
                    size = 2;
                    break;
                case 0x2f:
                    name = "checkmovingindir";
                    size = 4;
                    break;
                case 0x30:
                    name = "ifflagoff";//if a logic switch is on
                    size = 5;
                    break;
                case 0x31:
                    name = "ifflagon";//if a logic switch is not on
                    size = 5;
                    break;
                case 0x32:
                    name = "toggleflag";//toggle bit on a flag
                    size = 3;
                    break;
                case 0x33:
                    name = "checkflagson";
                    size = 9;
                    break;
                case 0x34:
                    name = "checkflagsoff";
                    size = 9;
                    break;
                case 0x35:
                    name = "untilflagoff";//block until a flag is off
                    size = 3;
                    break;
                case 0x36:
                    name = "untilflagon";//block until a flag is on
                    size = 3;
                    break;
                case 0x37:
                    name = "wait";//waits for the specified time to pass
                    size = 2;
                    break;
                case 0x38:
                    name = "registersomething?";
                    size = 5;
                    break;
                case 0x39:
                    name = "waitfordaialog";//blocks until the dialog is finished
                    size = 1;
                    break;

                case 0x3b:
                    name = "checkplayerinarea";
                    size = 7;
                    break;
                case 0x40:
                    name = "setprogramindex";
                    size = 3;
                    break;
                case 0x41:
                    name = "setspriteprogramindex";
                    size = 3;
                    break;
                case 0x44:
                    name = "waitdialogchoice";
                    size = 1;
                    break;
                case 0x45:
                    name = "gravityflag4off";//bit 0x2000
                    size = 1;
                    break;
                case 0x46:
                    name = "gravityflag4on";//bit 0x2000
                    size = 1;
                    break;
                case 0x49:
                    name = "restart";//seeks back to the beginning of event program
                    size = 1;
                    break;
                case 0x4a:
                    name = "iftruerestart";
                    size = 1;
                    break;
                case 0x4b:
                    name = "iffalserestart";
                    size = 1;
                    break;
                case 0x4c:
                    name = "setdialogsoemthing";//*0x107200 = val
                    size = 2;
                    break;
                case 0x4d:
                    name = "checkdialogsomething";//*0x107204 = *0x107200 & 0x4
                    size = 1;
                    break;
                case 0x50:
                    name = "setdialogchoice";
                    size = 2;
                    break;
                case 0x51:
                    name = "getdialogchoice";
                    size = 1;
                    break;

                case 0x54:
                    name = "setwalkable";
                    size = 5;
                    break;
                case 0x55:
                    name = "setunwalkable";
                    size = 5;
                    break;
                case 0x58:
                    name = "directionalbranch";
                    size = 9;
                    break;
                case 0x59:
                    name = "setentityanim";
                    size = 3;
                    break;
                case 0x5a:
                    name = "turnentity";
                    size = 3;
                    break;
                case 0x5b:
                    name = "turnentitywithanim";//also has anim flag for on ground or climbing, etc
                    size = 4;
                    break;
                case 0x5c:
                    name = "dialogwithentity";
                    size = 4;
                    break;

                case 0x62:
                    name = "setentitysomething?";
                    size = 4;
                    break;
                case 0x63:
                    name = "setentitygravity";
                    size = 4;
                    break;
                case 0x64:
                    name = "setentityposition";
                    size = 8;
                    break;
                case 0x65:
                    name = "moveentityposition";
                    size = 8;
                    break;

                case 0x67:
                    name = "followentity";
                    size = 2;
                    break;

                case 0x70:
                    name = "checksomething?";
                    size = 1;
                    break;

                case 0x85:
                    name = "setmaptiles";
                    size = 7;
                    break;

                case 0x8b:
                    name = "spawnentity";//pulls entity to ones self and activates it at pixel offset
                    size = 9;
                    break;
                case 0x90:
                    name = "createeffect";
                    size = 2;
                    break;
                case 0x91:
                    name = "disableeffect";
                    size = 2;
                    break;
                case 0x92:
                    name = "seteffectanim";
                    size = 3;
                    break;
                case 0x93:
                    name = "seteeffectpos";
                    size = 8;
                    break;
                case 0x94:
                    name = "seteeffectforces";
                    size = 8;
                    break;
                case 0xa0:
                    name = "adjusteeffectpos";
                    size = 8;
                    break;
                case 0xa1:
                    name = "seteeffectposwithentity";
                    size = 9;
                    break;
                case 0xa2:
                    name = "createeffectwithpos";
                    size = 8;
                    break;
                case 0xa3:
                    name = "createeffectwithentitypos";
                    size = 9;
                    break;
                case 0xa7:
                    name = "playmusic";
                    size = 3;
                    break;
                case 0xac:
                    name = "setgravityflagsonentity";
                    size = 4;
                    break;
                case 0xbd:
                    name = "playsound2";//2 byte sound index
                    size = 3;
                    break;

                case 0xc4:
                    name = "dialogwithentityandname";
                    size = 6;
                    break;

                //non-operations
                case 0x00:
                    name = "break";
                    size = 1;
                    break;
                case 0xff:
                    name = "end";
                    size = 1;
                    break;

                //unknowns, just get the size down
                case 0x69:
                    size = 7;
                    break;
                case 0x73:
                    size = 2;
                    break;
                case 0x74:
                    size = 3;
                    break;
                case 0x78:
                    size = 3;
                    break;
                case 0x98:
                    size = 3;
                    break;

                default:
                    Debug.Print("Unknown code " + b.ToString("x2"));
                    break;
            }

            return new SiCode { Code = b, Size = size, Name = name };
        }
        public List<SiCommand> GetCommands(BinaryReader br, int eventcodesoffset, bool stopatff = false, int comandssize = 0)
        {
            var commands = new List<SiCommand>();
            //var bytes = GetByteCode(br, sector1offset);
            br.BaseStream.Position = _binoffset + eventcodesoffset;
            var bytes = new byte[_datasize - eventcodesoffset];
            br.Read(bytes, 0, bytes.Length);
            var dex = 0;
            while (dex < bytes.Length && (comandssize == 0 || dex < comandssize))
            {
                var b = bytes[dex++];

                var sicode = GetCode(b);
                var size = sicode.Size;
                var name = sicode.Name;
                var parms = new byte[size - 1];
                var pdex = 0;
                while (pdex < size - 1)
                    parms[pdex++] = bytes[dex++];
                SiCommand cmd;
                var addr = _memaddr + eventcodesoffset + dex - size;
                switch (name)
                {
                    case "walk":
                    case "walk2":
                        cmd = new WalkCommand(b, parms, name, addr);
                        break;
                    case "setentityposition":
                        cmd = new SetPositionCommand(b, parms, name, addr);
                        break;
                    case "flagon":
                    case "flagoff":
                        cmd = new SetFlagCommand(b, parms, name, addr);
                        break;
                    case "if":
                    case "ifnot":
                        cmd = new BranchCommand(b, 5, parms, name, addr);
                        break;
                    case "ifno":
                    case "whilefalse":
                        cmd = new BranchCommand(b, 3, parms, name, addr);
                        break;
                    case "goto":
                        cmd = new JumpCommand(b, parms, name, addr);
                        break;
                    case "directionalbranch":
                        cmd = new DirectionBranchCommand(b, parms, name, addr);
                        break;
                    default:
                        cmd = new SiCommand(b, size, parms, name, addr);
                        break;
                }

                commands.Add(cmd);
                if (stopatff && b == 0xff)
                {
                    break;
                }
            }

            return commands;
        }

        public byte[] GetByteCode(BinaryReader br, int sector1Offset)
        {

            var bytes = new byte[_datasize - sector1Offset];
            var dex = 0;
            br.BaseStream.Position = _binoffset + sector1Offset;

            while (dex < bytes.Length)
            {
                //Debug.Assert(dex < bytes.Length, "ByteCodes larger than 255");

                var b = br.ReadByte();
                if (b == 0)//what does 0 mean?
                {
                    bytes[dex++] = b;
                }
                else if (b == 0xff)//end
                {
                    bytes[dex++] = b;
                    return bytes;//for now
                }
                else
                {
                    bytes[dex++] = b;
                    //skip ahead by parameter length
                }
            }
            return bytes;
        }

        private long _binoffset;
        private int _datasize;
        private int _memaddr;
        public readonly short[] Eventcodesatable;
        public readonly short[] Eventcodesbtable;
        public readonly short[] Eventcodesctable;
        public readonly short[] Eventcodesdtable;
        public readonly short[] Eventcodesetable;
        public readonly short[] Eventcodesftable;

        public readonly List<short[]> Eventcodestable = new();
    }

    public class SetFlagCommand : SiCommand
    {
        public SetFlagCommand(byte command, byte[] parameters, string name, int memaddr)
            : base(command, 3, parameters, name, memaddr)
        {
        }

        public override string PrintParameters(List<SiCommand> commands)
        {
            return (Parameters[0] | (Parameters[1] << 8)).ToString("x4");
        }
    }

    public class WalkCommand : SiCommand
    {
        public WalkCommand(byte command, byte[] parameters, string name, int memaddr)
            : base(command, 3, parameters, name, memaddr)
        {
        }

        public override string PrintParameters(List<SiCommand> commands)
        {
            return (Parameters[0] | (Parameters[1] << 8)).ToString("x4");
        }
    }

    public class SetPositionCommand : SiCommand
    {
        public SetPositionCommand(byte command, byte[] parameters, string name, int memaddr)
            : base(command, 8, parameters, name, memaddr)
        {
        }

        public override string PrintParameters(List<SiCommand> commands)
        {
            var parms = new List<string>();

            parms.Add(Parameters[0].ToString("x2"));
            parms.Add((Parameters[1] | (Parameters[2] << 8)).ToString("x4"));
            parms.Add((Parameters[3] | (Parameters[4] << 8)).ToString("x4"));
            parms.Add((Parameters[5] | (Parameters[6] << 8)).ToString("x4"));

            return string.Join(", ", parms);
        }
    }

    public class BranchCommand : SiCommand
    {
        public BranchCommand(byte command, int size, byte[] parameters, string name, int memaddr)
            : base(command, size, parameters, name, memaddr)
        {
            Refoffset = (short)(parameters[size - 3] | (parameters[size - 2] << 8));
        }

        public override string PrintParameters(List<SiCommand> commands)
        {
            var parms = new List<string>();
            if (Size == 5)
            {
                parms.Add((Parameters[Size - 5] | (Parameters[Size - 4] << 8)).ToString("x4"));
            }
            else
            {

                for (var dex = 0; dex < Size - 3; dex++)
                {
                    parms.Add(Parameters[dex].ToString("x2"));
                }
            }
            parms.Add(Refoffset.ToString());

            return string.Join(", ", parms);
        }
    }

    public class DirectionBranchCommand : SiCommand
    {
        public DirectionBranchCommand(byte command, byte[] parameters, string name, int memaddr)
            : base(command, 9, parameters, name, memaddr)
        {

            _offsets[0] = (short)(parameters[Size - 9] | (parameters[Size - 8] << 8));
            _offsets[1] = (short)(parameters[Size - 7] | (parameters[Size - 6] << 8));
            _offsets[2] = (short)(parameters[Size - 5] | (parameters[Size - 4] << 8));
            _offsets[3] = (short)(parameters[Size - 3] | (parameters[Size - 2] << 8));
        }

        private int[] _offsets = new int[4];

        public override string PrintParameters(List<SiCommand> commands)
        {
            var parms = new List<string>();
            foreach (var offset in _offsets)
            {
                var jumpaddr = Memaddr + offset;
                int dex;
                for (dex = 0; dex < commands.Count; dex++)
                {
                    if (commands[dex].Memaddr == jumpaddr)
                    {
                        break;
                    }
                }
                parms.Add(dex < commands.Count ? dex.ToString() : "?");
            }

            return string.Join(", ", parms);
        }
    }

    public class JumpCommand : SiCommand
    {
        public JumpCommand(byte command, byte[] parameters, string name, int memaddr)
            : base(command, 3, parameters, name, memaddr)
        {
            Refoffset = (short)(parameters[0] | (parameters[1] << 8));
        }

        public override string PrintParameters(List<SiCommand> commands)
        {
            var jumpamount = (short)Refoffset;// (Int16)(parameters[0] | parameters[1] << 8);
            var jumpaddr = Memaddr + jumpamount;
            int dex;
            for (dex = 0; dex < commands.Count; dex++)
            {
                if (commands[dex].Memaddr == jumpaddr)
                {
                    break;
                }
            }
            return dex < commands.Count ? dex.ToString() : "?";
        }

    }

    public class SiCommand
    {
        public SiCommand(byte command, int size, byte[] parameters, string name, int memaddr)
        {
            Memaddr = memaddr;
            Command = command;
            Parameters = parameters;
            Size = size;
            Name = name;
        }
        public readonly int Memaddr;
        public readonly byte Command;
        public readonly byte[] Parameters;
        public readonly int Size;
        public int Refoffset;

        public readonly string Name;

        public string PrintName()
        {
            return !string.IsNullOrEmpty(Name) ? Name : Command.ToString("x2");
        }

        public virtual string PrintParameters(List<SiCommand> commands)
        {
            return string.Join(", ", Parameters.Select(x => x.ToString("x2")));
        }

        public string Print(int depth, List<SiCommand> commands)
        {
            var index = commands.IndexOf(this);
            var output = index.ToString("d3") + " ";
            output += new string(' ', depth * 4);
            output += PrintName();
            if (Command != 0 && Command != 0xff)
            {
                output += "(";
                output += PrintParameters(commands);
                output += ")";
            }
            return output;
        }
    }

    public class SpriteInfoEntities
    {
        public SpriteInfoEntities(BinaryReader br, int memaddr)
        {
            //br.BaseStream.Position += 2;//this is wrong, dont nudge it like this

            Entities = new SiEntityRecord[128];
            for (var i = 0; i < Entities.Length; i++)
            {
                //read two test bytes to check for the end of the list
                var test = br.ReadInt16();
                test = br.ReadInt16();
                if (test == 0)
                {
                    break;
                }

                br.BaseStream.Position -= 4;

                //read the record
                Entities[i] = new SiEntityRecord(br, memaddr + i * 20);
            }
        }
        public readonly SiEntityRecord[] Entities;
    }

    public class SiEntityRecord
    {
        public SiEntityRecord(BinaryReader br, int memaddr)
        {
            Memaddr = memaddr;
            //i used to think these were the last of the previous entry, but its the first of this one
            Minx = br.ReadByte();//0
            Miny = br.ReadByte();//1

            Maxx = br.ReadByte();//2
            Maxy = br.ReadByte();//3
            U3 = br.ReadByte();//4
            Spritedir = br.ReadByte();//5
            Spritetableindex = br.ReadByte();//6
            Xpos = br.ReadByte();//7
            Ypos = br.ReadByte();//8
            Height = br.ReadByte();//9
            EventCodesA_LoadIndex = br.ReadByte();
            EventCodesB_MapIndex = br.ReadByte();
            EventCodesC_TickIndex = br.ReadByte();
            EventCodesD_TouchIndex = br.ReadByte();
            EventCodesE_DeactivateIndex = br.ReadByte();
            EventCodesF_InteractIndex = br.ReadByte();
            U7 = br.ReadByte();//10
            U7 = (short)(U7 | (br.ReadByte() << 8));
            //u8 = br.ReadByte();//11
            Contents = br.ReadByte();//12
            U10 = br.ReadByte();//13

        }

        public SiAnimation GetSprite(BinaryReader br, SpriteInfo si)
        {
            var sector5 = si.Sprites[Spritetableindex];
            if (sector5 != null && Spritedir >> 4 != 0x4 && Spritedir >> 4 != 0x0)
            {
                var commands = new List<SiCommand>();
                if (EventCodesA_LoadIndex != 0xff && EventCodesA_LoadIndex != 0)
                {
                    commands.AddRange(si.EventCodes.GetCommands(br, si.EventCodes.Eventcodesatable[EventCodesA_LoadIndex & 0x7f], true));
                }

                if (commands.Count == 0 && EventCodesC_TickIndex != 0xff && EventCodesC_TickIndex != 0)
                {
                    commands.AddRange(si.EventCodes.GetCommands(br, si.EventCodes.Eventcodesctable[EventCodesC_TickIndex & 0x7f], true));
                }

                foreach (var cmd in commands)
                {
                    if (cmd.Command == 0x1a)//set sprite
                    {
                        var animset = sector5.Animsets[cmd.Parameters[0]];

                        return sector5.GetAnimation(br, animset.Animoffsets[Spritedir & 0x3]);
                    }
                }
                return sector5.GetAnimation(br, sector5.Animsets[0].Animoffsets[Spritedir & 0x3]);//default anim
            }

            return null;
        }
        public readonly int Memaddr;
        public readonly byte Minx;//if character isnt within this bounding box, dont activate the entity
        public readonly byte Miny;
        public readonly byte Maxx;//33
        public readonly byte Maxy;//3b
        public readonly byte U3;//1
        public readonly byte Spritedir;//0,c0,c1,c2,c3,80
        public readonly byte Spritetableindex;
        public readonly byte Xpos;//divide by 2
        public readonly byte Ypos;//divide by 2
        public readonly byte Height;//divide by 2
        public readonly byte EventCodesA_LoadIndex;
        public readonly byte EventCodesB_MapIndex;
        public readonly byte EventCodesC_TickIndex;
        public readonly byte EventCodesD_TouchIndex;
        public readonly byte EventCodesE_DeactivateIndex;
        public readonly byte EventCodesF_InteractIndex;
        public readonly short U7;
        //public byte u8;
        public readonly byte Contents;
        public readonly byte U10;

    }

    public class MapEffectRecord
    {
        public MapEffectRecord(BinaryReader br)
        {
            X1 = br.ReadByte();
            Y1 = br.ReadByte();
            X2 = br.ReadByte();
            Y2 = br.ReadByte();
            Flags = br.ReadByte();
            Effectid = br.ReadByte();
            X = br.ReadByte();
            Y = br.ReadByte();
            Z = br.ReadByte();
            Animid = br.ReadByte();

            U1 = br.ReadByte();//probably just padding
            U2 = br.ReadByte();//padding
        }
        public readonly byte X1;//player must be within these map tiles
        public readonly byte X2;//player must be within these map tiles
        public readonly byte Y1;//player must be within these map tiles
        public readonly byte Y2;//player must be within these map tiles
        public readonly byte Flags;//0x80 ismapsprite //4
        public readonly byte Effectid;//5
        public readonly byte X;//6
        public readonly byte Y;//7
        public readonly byte Z;//8
        public readonly byte Animid;//9

        public byte U1, U2;

    }

    public class SpriteInfoMapEvents
    {
        public SpriteInfoMapEvents(BinaryReader br, long sioffset, int sectorend)
        {

            Records = new SiMapEventRecord[64];
            for (var dex = 0; dex < Records.Length; dex++)
            {
                //read two test bytes to check for the end of the list
                var test = br.ReadInt32();
                if (test == 0)
                {
                    break;
                }

                br.BaseStream.Position -= 4;

                //read the record
                Records[dex] = new SiMapEventRecord(br);
            }

        }

        public readonly SiMapEventRecord[] Records;

    }

    public class SiMapEventRecord
    {
        public SiMapEventRecord(BinaryReader br)
        {

            X1 = br.ReadByte();
            Y1 = br.ReadByte();
            X2 = br.ReadByte();
            Y2 = br.ReadByte();
            Eventcodesbindex = br.ReadByte();
            Ub1 = br.ReadByte();
            Ub2 = br.ReadByte();
            Ub3 = br.ReadByte();
        }

        public readonly byte X1;
        public readonly byte Y1;
        public readonly byte X2;
        public readonly byte Y2;
        public readonly byte Eventcodesbindex;
        public readonly byte Ub1;
        public readonly byte Ub2;
        public readonly byte Ub3;
    }

    public class GameMapInfo
    {
        public GameMapInfo(int mapId, int memaddr)
        {
            Memaddr = memaddr;
            MapId = mapId;
        }

        public GameMapInfo(BinaryReader br, int memaddr)
        {
            Memaddr = memaddr; // start just after the header ??
            var startPosition = br.BaseStream.Position;
            MapId = br.ReadInt32();//0
            Gravity = br.ReadInt16();//4
            TerminalVelocity = br.ReadInt16();//8
            SlideEffectId = br.ReadByte();//a
            BalanceLevel = br.ReadByte();//b
            C = br.ReadByte();//c
            D = br.ReadByte();//d
            E = br.ReadByte();//e
            F = br.ReadByte();//f
            _10 = br.ReadInt16();//10
            //read palettes
            var maxpalettes = 32;
            Palettes = new Color[maxpalettes][];
            var buff = new byte[maxpalettes * 16 * 2];
            br.Read(buff, 0, buff.Length);
            var buffdex = 0;

            for (var dex = 0; dex < maxpalettes; dex++)
            {
                Palettes[dex] = new Color[16];
                for (var cdex = 0; cdex < 16; cdex++)
                {
                    var b2 = buff[buffdex++];
                    var b1 = buff[buffdex++];
                    Palettes[dex][cdex] = Utils.FromPsxColor((b1 << 8) | b2);
                }
            }

            Palettesbitmap = Utils.BitmapFromPsxBuff(buff, 16, maxpalettes, 16, null);

            //read portals
            br.BaseStream.Position = startPosition + 1066;
            PortalFlag1 = br.ReadByte();
            PortalFlag2 = br.ReadByte();
            var maxportals = 64;
            Portals = new Portal[maxportals];
            for (var dex = 0; dex < Portals.Length; dex++)
            {
                Portals[dex] = new Portal(br);
            }
        }

        public readonly int Memaddr;
        public readonly int MapId;
        public readonly short Gravity;
        public readonly short TerminalVelocity;
        public readonly byte SlideEffectId;
        public readonly byte BalanceLevel;
        public readonly byte C;
        public readonly byte D;
        public readonly byte E;
        public readonly byte F;
        public readonly short _10;
        public readonly Color[][] Palettes;
        public readonly Bitmap Palettesbitmap;
        public readonly byte PortalFlag1;
        public readonly byte PortalFlag2;
        public readonly Portal[] Portals;

    }
    
    public class Portal
    {
        public Portal(BinaryReader br)
        {
            X1 = br.ReadByte();
            Y1 = br.ReadByte();
            X2 = br.ReadByte();
            Y2 = br.ReadByte();
            DestMapId = br.ReadInt16();
            DestX = br.ReadByte();
            DestY = br.ReadByte();
            Unknown1 = br.ReadByte();
            Unknown2 = br.ReadByte();
            Unknown3 = br.ReadByte();
            Unknown4 = br.ReadByte();
        }
        public readonly byte X1;
        public readonly byte Y1;
        public readonly byte X2;
        public readonly byte Y2;
        public readonly short DestMapId;
        public readonly byte DestX;
        public readonly byte DestY;
        public readonly byte Unknown1;
        public readonly byte Unknown2;
        public readonly byte Unknown3;
        public readonly byte Unknown4;
    }
    
    public class GameMapHeader
    {
        public GameMapHeader(DbHeader header)
        {//alundra gamemap, just has sprites
            InfoBlock = -1;
            MapBlock = -1;
            TileSheets = -1;
            SpriteInfo = (int)header.Alundraspriteinfo;
            SpriteSheets = (int)header.Alundrasprites;
            ScrollScreen = -1;
            StringTable = (int)header.Alundrastringtable;

            Infosize = 0;
            Mapsize = 0;
            Tilessize = 0;
            Sinfosize = SpriteSheets - SpriteInfo;
            Spritessize = (int)header.Unknownmapa - SpriteSheets;
            Scrollsize = 0;
        }
        public GameMapHeader(BinaryReader br)
        {
            InfoBlock = br.ReadInt32();//0
            MapBlock = br.ReadInt32();//4
            TileSheets = br.ReadInt32();//8
            SpriteInfo = br.ReadInt32();//c
            SpriteSheets = br.ReadInt32();//10
            ScrollScreen = br.ReadInt32();//14
            StringTable = br.ReadInt32();//18

            Infosize = MapBlock - InfoBlock;
            Mapsize = TileSheets - MapBlock;
            Tilessize = SpriteInfo - TileSheets;
            Sinfosize = SpriteSheets - SpriteInfo;
            Spritessize = ScrollScreen - SpriteSheets;
            Scrollsize = StringTable - ScrollScreen;
            //string table is called later
        }

        public readonly int Infosize;
        public int Mapsize;
        public int WallTilesSize;
        public readonly int Tilessize;
        public readonly int Sinfosize;
        public readonly int Spritessize;
        public readonly int Scrollsize;
        public int Stringsize;

        public readonly int InfoBlock;
        public readonly int MapBlock;
        public readonly int TileSheets;
        public readonly int SpriteInfo;
        public readonly int SpriteSheets;
        public readonly int ScrollScreen;//shadow, sky or distant background
        public readonly int StringTable;
    }

    public class DbHeader
    {
        public DbHeader(BinaryReader br)
        {
            Alundraspriteinfo = br.ReadUInt32();//0
            Alundrasprites = br.ReadUInt32();//4
            Alundraspritesrepeat = br.ReadUInt32();//8
            Alundrastringtable = br.ReadUInt32();//c
            Alundrastringtablerepeat = br.ReadUInt32();//10

            Unknownmapa = br.ReadUInt32();//14
            Unknownmapb = br.ReadUInt32();//18
            Unknownmapb2 = br.ReadUInt32();//1c
            Unknownmapb3 = br.ReadUInt32();//20
            Unknownmapb4 = br.ReadUInt32();//24

            GameMaps = new uint[502];//28

            for (var i = 0; i < GameMaps.Length; i++)
            {
                GameMaps[i] = br.ReadUInt32();
            }
        }

        public readonly uint Alundraspriteinfo;
        public readonly uint Alundrasprites;
        public uint Alundraspritesrepeat;
        public readonly uint Alundrastringtable;
        public uint Alundrastringtablerepeat;
        public readonly uint Unknownmapa;
        public uint Unknownmapb;
        public uint Unknownmapb2;
        public uint Unknownmapb3;
        public uint Unknownmapb4;
        public readonly uint[] GameMaps;
    }
}
