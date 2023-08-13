using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace GraphicsTools.Alundra.Sprite
{
    public class SIEventCodes
    {
        public SIEventCodes(BinaryReader br, long binoffset, SpriteInfoHeader header)
        {

            int table_size = 0;
            short firstoffset = 0;

            //read sector1a
            br.BaseStream.Position = binoffset + header.sector1apointer;
            table_size = header.sector1asize / 2;
            sector1atable = new short[table_size];
            for (int dex = 0; dex < table_size; dex++)
            {
                sector1atable[dex] = br.ReadInt16();
                if (firstoffset == 0 && sector1atable[dex] != 0)
                    firstoffset = sector1atable[dex];
            }

            //read sector1b
            br.BaseStream.Position = binoffset + header.sector1bpointer;
            table_size = header.sector1bsize / 2;
            sector1btable = new short[table_size];
            for (int dex = 0; dex < table_size; dex++)
            {
                sector1btable[dex] = br.ReadInt16();
                if (firstoffset == 0 && sector1btable[dex] != 0)
                    firstoffset = sector1btable[dex];
            }

            //read sector1c
            br.BaseStream.Position = binoffset + header.sector1cpointer;
            table_size = header.sector1csize / 2;
            sector1ctable = new short[table_size];
            for (int dex = 0; dex < table_size; dex++)
            {
                sector1ctable[dex] = br.ReadInt16();
                if (firstoffset == 0 && sector1ctable[dex] != 0)
                    firstoffset = sector1ctable[dex];
            }

            //read sector1d
            br.BaseStream.Position = binoffset + header.sector1dpointer;
            table_size = header.sector1dsize / 2;
            sector1dtable = new short[table_size];
            for (int dex = 0; dex < table_size; dex++)
            {
                sector1dtable[dex] = br.ReadInt16();
                if (firstoffset == 0 && sector1dtable[dex] != 0)
                    firstoffset = sector1dtable[dex];
            }

            //read sector1e
            br.BaseStream.Position = binoffset + header.sector1epointer;
            table_size = header.sector1esize / 2;
            sector1etable = new short[table_size];
            for (int dex = 0; dex < table_size; dex++)
            {
                sector1etable[dex] = br.ReadInt16();
                if (firstoffset == 0 && sector1etable[dex] != 0)
                    firstoffset = sector1etable[dex];
            }

            //read sector1f
            header.sector1fsize = (header.sector1apointer + firstoffset) - header.sector1fpointer;
            br.BaseStream.Position = binoffset + header.sector1fpointer;
            table_size = header.sector1fsize / 2;
            if (table_size < 0)
                table_size = 16;
            sector1ftable = new short[table_size];
            for (int dex = 0; dex < table_size; dex++)
            {
                sector1ftable[dex] = br.ReadInt16();
            }

            //set binoffset for sector1
            this.binoffset = binoffset + header.sector1apointer; ;
        }

        public byte[] GetByteCode(BinaryReader br, int sector1offset)
        {

            byte[] bytes = new byte[255];
            int dex = 0;
            br.BaseStream.Position = binoffset + sector1offset;

            while (true)
            {
                Debug.Assert(dex < bytes.Length, "ByteCodes larger than 255");

                byte b = br.ReadByte();
                if (b == 0)//what does 0 mean?
                {
                    bytes[dex++] = b;
                }
                else if (b == 0xff)//end
                {
                    bytes[dex++] = b;
                    return bytes;
                }
                else
                {
                    bytes[dex++] = b;
                    //skip ahead by parameter length
                }
            }
        }

        long binoffset;
        public short[] sector1atable;
        public short[] sector1btable;
        public short[] sector1ctable;
        public short[] sector1dtable;
        public short[] sector1etable;
        public short[] sector1ftable;
    }
}
