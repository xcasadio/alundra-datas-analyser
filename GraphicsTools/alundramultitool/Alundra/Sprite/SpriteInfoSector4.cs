namespace GraphicsTools.Alundra.Sprite
{
    public class SpriteInfoSector4
    {
        public SpriteInfoSector4(BinaryReader br)
        {
            br.BaseStream.Position += 2;

            Records = new SiSector4Record[64];
            for (var dex = 0; dex < Records.Length; dex++)
            {
                //read two test bytes to check for the end of the list
                var test = br.ReadInt16();
                if (test == 0)
                {
                    break;
                }

                br.BaseStream.Position -= 2;

                //read the record
                Records[dex] = new SiSector4Record(br);
            }
        }

        public readonly SiSector4Record[] Records;
    }

    public class SiSector4Record
    {
        public SiSector4Record(BinaryReader br)
        {
            U1 = br.ReadByte();
            U2 = br.ReadByte();
            U3 = br.ReadByte();
            U4 = br.ReadByte();
            U5 = br.ReadByte();
            U6 = br.ReadByte();
            U7 = br.ReadByte();
            U8 = br.ReadByte();
        }

        public byte U1;
        public byte U2;
        public byte U3;
        public byte U4;
        public byte U5;
        public byte U6;
        public byte U7;
        public byte U8;
    }
}
