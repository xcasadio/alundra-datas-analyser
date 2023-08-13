using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GraphicsTools.Alundra.Sprite
{
    public class SpriteInfoSector4
    {
        public SpriteInfoSector4(BinaryReader br)
        {
            br.BaseStream.Position += 2;

            records = new SISector4Record[64];
            for (int dex = 0; dex < records.Length; dex++)
            {
                //read two test bytes to check for the end of the list
                short test = br.ReadInt16();
                if (test == 0)
                    break;
                br.BaseStream.Position -= 2;

                //read the record
                records[dex] = new SISector4Record(br);
            }
        }

        public SISector4Record[] records;
    }

    public class SISector4Record
    {
        public SISector4Record(BinaryReader br)
        {
            u1 = br.ReadByte();
            u2 = br.ReadByte();
            u3 = br.ReadByte();
            u4 = br.ReadByte();
            u5 = br.ReadByte();
            u6 = br.ReadByte();
            u7 = br.ReadByte();
            u8 = br.ReadByte();
        }

        public byte u1;
        public byte u2;
        public byte u3;
        public byte u4;
        public byte u5;
        public byte u6;
        public byte u7;
        public byte u8;
    }
}
