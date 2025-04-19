namespace GraphicsTools.Alundra
{
    public class BalanceBin
    {
        private readonly string _balanceFile;
        private readonly List<BalanceRecord> _balanceRecords = new();

        public BalanceBin(string balanceFile)
        {
            _balanceFile = balanceFile;
            using var br = new BinaryReader(File.OpenRead(balanceFile));
            List<int> offsets = new ();
            var firstoffset = 0;

            while (firstoffset == 0 || br.BaseStream.Position < firstoffset)
            {
                int offset = br.ReadInt16();
                if (firstoffset == 0)
                {
                    firstoffset = offset;
                }

                offsets.Add(offset);
            }

            foreach (var offset in offsets)
            {
                var record = new BalanceRecord(br, offset);
                _balanceRecords.Add(record);
            }
        }

        public BalanceRecord GetBalanceRecordFromSpriteIndex(int index, int balancelevel)
        {
            var record = _balanceRecords[index];
            if (record.Level >= balancelevel)
            {
                return record;
            }

            do
            {
                record = record.Next;
            } while (record.Level < balancelevel);

            return record;
        }
    }

    public class BalanceRecord
    {
        public readonly byte Level;//0
        public readonly byte OffsetToNextLevel;//1
        public readonly byte Hp;//2 
        public readonly byte[] Vals = new byte[11];//supposed to be at 2
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
        public readonly byte NumAnimVals;//e
        public readonly BalanceAnimValRef[] AnimVals;//targetanim+1 //f

        public int Offset;
        public readonly BalanceRecord Next;

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
                for (var dex = 0; dex < NumAnimVals; dex++)
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
        public readonly byte Val;
        public byte U2;

        public BalanceAnimValRef(BinaryReader br)
        {
            Val = br.ReadByte();
            U2 = br.ReadByte();
        }
    }
}
