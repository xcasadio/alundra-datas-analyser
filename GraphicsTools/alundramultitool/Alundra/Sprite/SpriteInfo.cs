using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace GraphicsTools.Alundra.Sprite
{
    public class SpriteInfo
    {
        public SpriteInfo(BinaryReader br)
        {
            binoffset = br.BaseStream.Position;

            header = new SpriteInfoHeader(br);

            //read sector5 table
            br.BaseStream.Position = binoffset + header.sector5tablepointer;
            sector5table = new int[0xff];
            for (int dex = 0; dex < sector5table.Length; dex++)
            {
                sector5table[dex] = br.ReadInt32();
            }

            //read palettes
            br.BaseStream.Position = binoffset + header.spritepalettespointer;
            int maxpalettes = 32;
            palettes = new System.Drawing.Color[maxpalettes][];
            byte[] buff = new byte[maxpalettes * 16 * 2];
            br.Read(buff, 0, buff.Length);
            int buffdex = 0;
            for (int dex = 0; dex < maxpalettes; dex++)
            {
                palettes[dex] = new System.Drawing.Color[16];
                for (int cdex = 0; cdex < 16; cdex++)
                {
                    byte b2 = buff[buffdex++];
                    byte b1 = buff[buffdex++];
                    palettes[dex][cdex] = Utils.FromPsxColor((b1 << 8) | b2);
                }
            }
            palettesbitmap = Utils.BitmapFromPsxBuff(buff, 16, maxpalettes, 16, null);

            //read sector1
            eventcodes = new SIEventCodes(br, binoffset, header);

            //read sector2
            br.BaseStream.Position = binoffset + header.sector2pointer;
            entities = new SIEntities(br);

            //read sector 3
            br.BaseStream.Position = binoffset + header.sector3pointer;
            sector3 = new SpriteInfoSector3(br);

            //read sector 4
            br.BaseStream.Position = binoffset + header.sector4pointer;
            sector4 = new SpriteInfoSector4(br);

            //read sector5 records;
            animationsets = new SIAnimationSet[sector5table.Length];
            for (int dex = 0; dex < animationsets.Length; dex++)
            {
                if (sector5table[dex] != -1)
                {
                    br.BaseStream.Position = binoffset + sector5table[dex];
                    animationsets[dex] = new SIAnimationSet(br, binoffset);
                }
            }
        }

        public SpriteInfoHeader header;
        public SIEventCodes eventcodes;
        public SIEntities entities;
        public SpriteInfoSector3 sector3;
        public SpriteInfoSector4 sector4;

        int[] sector5table;
        public SIAnimationSet[] animationsets;

        long binoffset;

        public System.Drawing.Color[][] palettes;
        public Bitmap palettesbitmap;
    }

    public class SpriteInfoHeader
    {
        public SpriteInfoHeader(BinaryReader br)
        {
            sector2pointer = br.ReadInt32();
            sector3pointer = br.ReadInt32();
            sector4pointer = br.ReadInt32();
            sector5tablepointer = br.ReadInt32();
            unknown1pointer = br.ReadInt32();
            spritepalettespointer = br.ReadInt32();
            sector1apointer = br.ReadInt32();
            sector1bpointer = br.ReadInt32();
            sector1cpointer = br.ReadInt32();
            sector1dpointer = br.ReadInt32();
            sector1epointer = br.ReadInt32();
            sector1fpointer = br.ReadInt32();

            sector2size = sector3pointer - sector2pointer;
            sector3size = sector4pointer - sector3pointer;
            sector4size = -1;// unknown4 - unknown3;
            sector5tablesize = unknown1pointer - sector5tablepointer;
            unknown1size = spritepalettespointer - unknown1pointer;
            spritepalettessize = sector1apointer - spritepalettespointer;
            sector1asize = sector1bpointer - sector1apointer;
            sector1bsize = sector1cpointer - sector1bpointer;
            sector1csize = sector1dpointer - sector1cpointer;
            sector1dsize = sector1epointer - sector1dpointer;
            sector1esize = sector1fpointer - sector1epointer;
            sector1fandremainingsize = sector2pointer - sector1fpointer;
        }

        public int sector2pointer;
        public int sector2size;
        public int sector3pointer;
        public int sector3size;
        public int sector4pointer;
        public int sector4size;
        public int sector5tablepointer;
        public int sector5tablesize;
        public int unknown1pointer;
        public int unknown1size;
        public int spritepalettespointer;
        public int spritepalettessize;
        public int sector1apointer;
        public int sector1asize;
        public int sector1bpointer;
        public int sector1bsize;
        public int sector1cpointer;
        public int sector1csize;
        public int sector1dpointer;
        public int sector1dsize;
        public int sector1epointer;
        public int sector1esize;
        public int sector1fpointer;
        public int sector1fsize;//calced when reading sector1
        public int sector1fandremainingsize;
    }
}
