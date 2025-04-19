namespace GraphicsTools.Alundra.Sprite
{
    public class SpriteInfo
    {
        public SpriteInfo(BinaryReader br)
        {
            _binoffset = br.BaseStream.Position;

            Header = new SpriteInfoHeader(br);

            //read sector5 table
            br.BaseStream.Position = _binoffset + Header.Sector5Tablepointer;
            _sector5Table = new int[0xff];
            for (var dex = 0; dex < _sector5Table.Length; dex++)
            {
                _sector5Table[dex] = br.ReadInt32();
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

            //read sector1
            Eventcodes = new SiEventCodes(br, _binoffset, Header);

            //read sector2
            br.BaseStream.Position = _binoffset + Header.Sector2Pointer;
            Entities = new SiEntities(br);

            //read sector 3
            br.BaseStream.Position = _binoffset + Header.Sector3Pointer;
            Sector3 = new SpriteInfoSector3(br);

            //read sector 4
            br.BaseStream.Position = _binoffset + Header.Sector4Pointer;
            Sector4 = new SpriteInfoSector4(br);

            //read sector5 records;
            Animationsets = new SiAnimationSet[_sector5Table.Length];
            for (var dex = 0; dex < Animationsets.Length; dex++)
            {
                if (_sector5Table[dex] != -1)
                {
                    br.BaseStream.Position = _binoffset + _sector5Table[dex];
                    Animationsets[dex] = new SiAnimationSet(br, _binoffset);
                }
            }
        }

        public readonly SpriteInfoHeader Header;
        public SiEventCodes Eventcodes;
        public SiEntities Entities;
        public SpriteInfoSector3 Sector3;
        public SpriteInfoSector4 Sector4;

        private int[] _sector5Table;
        public readonly SiAnimationSet[] Animationsets;

        private long _binoffset;

        public readonly Color[][] Palettes;
        public Bitmap Palettesbitmap;
    }

    public class SpriteInfoHeader
    {
        public SpriteInfoHeader(BinaryReader br)
        {
            Sector2Pointer = br.ReadInt32();
            Sector3Pointer = br.ReadInt32();
            Sector4Pointer = br.ReadInt32();
            Sector5Tablepointer = br.ReadInt32();
            Unknown1Pointer = br.ReadInt32();
            Spritepalettespointer = br.ReadInt32();
            Sector1Apointer = br.ReadInt32();
            Sector1Bpointer = br.ReadInt32();
            Sector1Cpointer = br.ReadInt32();
            Sector1dpointer = br.ReadInt32();
            Sector1Epointer = br.ReadInt32();
            Sector1Fpointer = br.ReadInt32();

            Sector2Size = Sector3Pointer - Sector2Pointer;
            Sector3Size = Sector4Pointer - Sector3Pointer;
            Sector4Size = -1;// unknown4 - unknown3;
            Sector5Tablesize = Unknown1Pointer - Sector5Tablepointer;
            Unknown1Size = Spritepalettespointer - Unknown1Pointer;
            Spritepalettessize = Sector1Apointer - Spritepalettespointer;
            Sector1Asize = Sector1Bpointer - Sector1Apointer;
            Sector1Bsize = Sector1Cpointer - Sector1Bpointer;
            Sector1Csize = Sector1dpointer - Sector1Cpointer;
            Sector1dsize = Sector1Epointer - Sector1dpointer;
            Sector1Esize = Sector1Fpointer - Sector1Epointer;
            Sector1Fandremainingsize = Sector2Pointer - Sector1Fpointer;
        }

        public readonly int Sector2Pointer;
        public int Sector2Size;
        public readonly int Sector3Pointer;
        public int Sector3Size;
        public readonly int Sector4Pointer;
        public int Sector4Size;
        public readonly int Sector5Tablepointer;
        public int Sector5Tablesize;
        public readonly int Unknown1Pointer;
        public int Unknown1Size;
        public readonly int Spritepalettespointer;
        public int Spritepalettessize;
        public readonly int Sector1Apointer;
        public readonly int Sector1Asize;
        public readonly int Sector1Bpointer;
        public readonly int Sector1Bsize;
        public readonly int Sector1Cpointer;
        public readonly int Sector1Csize;
        public readonly int Sector1dpointer;
        public readonly int Sector1dsize;
        public readonly int Sector1Epointer;
        public readonly int Sector1Esize;
        public readonly int Sector1Fpointer;
        public int Sector1Fsize;//calced when reading sector1
        public int Sector1Fandremainingsize;
    }
}
