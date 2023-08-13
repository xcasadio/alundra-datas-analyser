using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsTools.Alundra
{
    public class BalanceBin
    {
        List<BalanceRecord> records = new List<BalanceRecord>();

        public BalanceBin(BinaryReader br)
        {
            int[] offsets = new int[1024];
            int offset = 0;
            int firstoffset = 0;
            int numoffsets = 0;
            while (firstoffset == 0 || br.BaseStream.Position < firstoffset)
            {
                offset = br.ReadInt16();
                if (firstoffset == 0)
                    firstoffset = offset;
                offsets[numoffsets++] = offset;
            }

            for (int dex = 0; dex < numoffsets; dex++)
            {
                var record = new BalanceRecord(br, offsets[dex]);
                records.Add(record);
            }
        }

        public BalanceRecord GetBalanceRecordFromSpriteIndex(int index, int balancelevel)
        {
            var record = records[index];
            if (record.Level >= balancelevel)
                return record;
            do
            {
                record = record.Next;
            } while (record.Level < balancelevel);

            return record;
        }
    }

    public class BalanceRecord
    {
        public byte Level;//0
        public byte OffsetToNextLevel;//1
        public byte Hp;//2 
        public byte[] Vals = new byte[11];//supposed to be at 2
        //but i think ill put it at 3 and subtract q from the indexvals
        //3
        //4
        //5
        //6
        //7
        //8
        //9
        //a
        //b
        //c
        //d
        public byte NumAnimVals;//e
        public BalanceAnimValRef[] AnimVals;//targetanim+1 //f

        public int Offset;
        public BalanceRecord Next;

        public BalanceRecord(BinaryReader br, int offset)
        {
            Offset = offset;
            br.BaseStream.Position = offset;
            Level = br.ReadByte();
            OffsetToNextLevel = br.ReadByte();
            Hp = br.ReadByte();
            br.Read(Vals, 0, 11);
            NumAnimVals = br.ReadByte();
            if (NumAnimVals > 0)
            {
                AnimVals = new BalanceAnimValRef[NumAnimVals];
                for (int dex = 0; dex < NumAnimVals; dex++)
                {
                    AnimVals[dex] = new BalanceAnimValRef(br);
                }
            }
            if (Level < 255)
            {
                Next = new BalanceRecord(br, offset + OffsetToNextLevel);
            }
        }
    }

    public class BalanceAnimValRef
    {
        public byte Val;
        public byte u2;

        public BalanceAnimValRef(BinaryReader br)
        {
            Val = br.ReadByte();
            u2 = br.ReadByte();
        }
    }
}
