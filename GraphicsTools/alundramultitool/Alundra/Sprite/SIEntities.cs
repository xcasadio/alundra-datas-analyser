using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GraphicsTools.Alundra.Sprite
{
    public class SIEntities
    {
        public SIEntities(BinaryReader br)
        {
            br.BaseStream.Position += 2;

            entities = new SIEntityRecord[128];
            for (int dex = 0; dex < entities.Length; dex++)
            {
                //read two test bytes to check for the end of the list
                short test = br.ReadInt16();
                if (test == 0)
                    break;
                br.BaseStream.Position -= 2;

                //read the record
                entities[dex] = new SIEntityRecord(br);
            }
        }
        public SIEntityRecord[] entities;
    }

    public class SIEntityRecord
    {
        public SIEntityRecord(BinaryReader br)
        {
            u1 = br.ReadByte();
            u2 = br.ReadByte();
            u3 = br.ReadByte();
            spritecode = br.ReadByte();
            sector5tableindex = br.ReadByte();
            xpos = br.ReadByte();
            ypos = br.ReadByte();
            height = br.ReadByte();
            sector1a_bahavior_index = br.ReadByte();
            sector1b_unknown_index = br.ReadByte();
            sector1c_unknown_index = br.ReadByte();
            sector1d_unknown_index = br.ReadByte();
            sector1e_unknown_index = br.ReadByte();
            sector1f_dialog_index = br.ReadByte();
            u7 = br.ReadByte();
            u8 = br.ReadByte();
            u9 = br.ReadByte();
            u10 = br.ReadByte();
            u11 = br.ReadByte();
            u12 = br.ReadByte();
        }

        public byte u1;//33
        public byte u2;//3b
        public byte u3;//1
        public byte spritecode;//0,c0,c1,c2,c3,80
        public byte sector5tableindex;
        public byte xpos;//divide by 2
        public byte ypos;//divide by 2
        public byte height;//divide by 2
        public byte sector1a_bahavior_index;
        public byte sector1b_unknown_index;
        public byte sector1c_unknown_index;
        public byte sector1d_unknown_index;
        public byte sector1e_unknown_index;
        public byte sector1f_dialog_index;
        public byte u7;
        public byte u8;
        public byte u9;
        public byte u10;
        public byte u11;
        public byte u12;
    }
}
