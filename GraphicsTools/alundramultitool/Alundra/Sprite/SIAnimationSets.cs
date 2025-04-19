namespace GraphicsTools.Alundra.Sprite
{
    public class SiAnimationSet
    {
        public SiAnimationSet(BinaryReader br, long binoffset)
        {

            Header = new SiAnimationSetHeader(br, binoffset);
            Animoffsets = new int[(Header.Unknownpointer - Header.Animationoffsetspointer) / 2];
            for (var dex = 0; dex < Animoffsets.Length; dex++)
            {
                Animoffsets[dex] = br.ReadInt16();
            }
        }

        public SiAnimation GetAnimation(BinaryReader br, int animationoffset)
        {
            br.BaseStream.Position = Header.Binoffset + Header.Animationspointer + animationoffset;

            var anim = new SiAnimation(br, Header);

            return anim;
        }


        public readonly SiAnimationSetHeader Header;
        public readonly int[] Animoffsets;

    }

    public class SiAnimationSetHeader
    {
        public SiAnimationSetHeader(BinaryReader br, long binoffset)
        {
            Binoffset = binoffset;
            Animationoffsetspointer = br.ReadInt32();
            Animationspointer = br.ReadInt32();
            Unknownpointer = br.ReadInt32();
            Framespointer = br.ReadInt32();
            U1 = br.ReadByte();
            U2 = br.ReadByte();
            U3 = br.ReadByte();
            U4 = br.ReadByte();
            U5 = br.ReadByte();
            U6 = br.ReadByte();
            U7 = br.ReadByte();
            U8 = br.ReadByte();
            U9 = br.ReadByte();
            U10 = br.ReadByte();
            U11 = br.ReadByte();
            U12 = br.ReadByte();
            U13 = br.ReadByte();
            U14 = br.ReadByte();
            U15 = br.ReadByte();
            U16 = br.ReadByte();
        }
        public readonly long Binoffset;

        public readonly int Animationoffsetspointer;
        public readonly int Animationspointer;
        public readonly int Unknownpointer;
        public readonly int Framespointer;
        public byte U1;
        public byte U2;
        public byte U3;
        public byte U4;
        public byte U5;
        public byte U6;
        public byte U7;
        public byte U8;
        public byte U9;
        public byte U10;
        public byte U11;
        public byte U12;
        public byte U13;
        public byte U14;
        public byte U15;
        public byte U16;
    }

    public class SiAnimation
    {
        public SiAnimation(BinaryReader br, SiAnimationSetHeader header)
        {
            Frames = new SiFrame[32];//32 max frames?
            for (var dex = 0; dex < Frames.Length; dex++)
            {
                //read two test bytes to check for the end of the list
                var test = br.ReadInt16();
                if (test == 0)
                {
                    break;
                }

                Numframes++;
                br.BaseStream.Position -= 2;

                Frames[dex] = new SiFrame(br, header);
            }
        }
        public int Numframes;
        public readonly SiFrame[] Frames;
    }

    public class SiFrame
    {
        public SiFrame(BinaryReader br, SiAnimationSetHeader header)
        {
            Delay = br.ReadByte();
            Unknown = br.ReadInt16();
            Imagesetpointer = br.ReadInt16() * 2;


            //load images
            var savepos = br.BaseStream.Position;

            br.BaseStream.Position = header.Binoffset + header.Framespointer + Imagesetpointer;
            Images = new SiImageSet(br);

            br.BaseStream.Position = savepos;
        }



        public byte Delay;//top bit masked
        public short Unknown;//-1
        public readonly int Imagesetpointer;
        public SiImageSet Images;
    }

    public class SiImageSet
    {
        public SiImageSet(BinaryReader br)
        {
            Palette = br.ReadByte();//palette?
            Numimages = br.ReadByte();
            Images = new SiImage[Numimages];
            for (var dex = 0; dex < Numimages; dex++)
            {
                Images[dex] = new SiImage(br);
            }
        }

        public byte Palette;
        public readonly byte Numimages;
        public readonly SiImage[] Images;
    }

    public class SiImage
    {
        public SiImage(BinaryReader br)
        {
            U1 = br.ReadByte();
            U2 = br.ReadByte();
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
        }

        public byte U1;
        public byte U2;
        public byte Sx;
        public byte Sy;
        public byte Swidth;
        public byte Sheight;
        public sbyte X1;
        public sbyte Y1;
        public sbyte X2;
        public sbyte Y2;
        public sbyte X3;
        public sbyte Y3;
        public sbyte X4;
        public sbyte Y4;
    }
}
