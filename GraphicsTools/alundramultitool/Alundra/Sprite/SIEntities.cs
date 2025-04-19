namespace GraphicsTools.Alundra.Sprite
{
    public class SiEntities
    {
        public SiEntities(BinaryReader br)
        {
            br.BaseStream.Position += 2;

            Entities = new SiEntityRecord[128];
            for (var dex = 0; dex < Entities.Length; dex++)
            {
                //read two test bytes to check for the end of the list
                var test = br.ReadInt16();
                if (test == 0)
                {
                    break;
                }

                br.BaseStream.Position -= 2;

                //read the record
                Entities[dex] = new SiEntityRecord(br);
            }
        }
        public readonly SiEntityRecord[] Entities;
    }

    public class SiEntityRecord
    {
        public SiEntityRecord(BinaryReader br)
        {
            U1 = br.ReadByte();
            U2 = br.ReadByte();
            U3 = br.ReadByte();
            Spritecode = br.ReadByte();
            Sector5Tableindex = br.ReadByte();
            Xpos = br.ReadByte();
            Ypos = br.ReadByte();
            Height = br.ReadByte();
            Sector1ABahaviorIndex = br.ReadByte();
            Sector1BUnknownIndex = br.ReadByte();
            Sector1CUnknownIndex = br.ReadByte();
            Sector1dUnknownIndex = br.ReadByte();
            Sector1EUnknownIndex = br.ReadByte();
            Sector1FDialogIndex = br.ReadByte();
            U7 = br.ReadByte();
            U8 = br.ReadByte();
            U9 = br.ReadByte();
            U10 = br.ReadByte();
            U11 = br.ReadByte();
            U12 = br.ReadByte();
        }

        public byte U1;//33
        public byte U2;//3b
        public byte U3;//1
        public byte Spritecode;//0,c0,c1,c2,c3,80
        public byte Sector5Tableindex;
        public byte Xpos;//divide by 2
        public byte Ypos;//divide by 2
        public byte Height;//divide by 2
        public byte Sector1ABahaviorIndex;
        public byte Sector1BUnknownIndex;
        public byte Sector1CUnknownIndex;
        public byte Sector1dUnknownIndex;
        public byte Sector1EUnknownIndex;
        public byte Sector1FDialogIndex;
        public byte U7;
        public byte U8;
        public byte U9;
        public byte U10;
        public byte U11;
        public byte U12;
    }
}
