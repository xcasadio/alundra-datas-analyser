using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GraphicsTools.Alundra.Sprite
{
    public class SIAnimationSet
    {
        public SIAnimationSet(BinaryReader br, long binoffset)
        {

            header = new SIAnimationSetHeader(br, binoffset);
            animoffsets = new int[(header.unknownpointer - header.animationoffsetspointer) / 2];
            for (int dex = 0; dex < animoffsets.Length; dex++)
            {
                animoffsets[dex] = br.ReadInt16();
            }
        }

        public SIAnimation GetAnimation(BinaryReader br, int animationoffset)
        {
            br.BaseStream.Position = header.binoffset + header.animationspointer + animationoffset;

            var anim = new SIAnimation(br, header);

            return anim;
        }


        public SIAnimationSetHeader header;
        public int[] animoffsets;

    }

    public class SIAnimationSetHeader
    {
        public SIAnimationSetHeader(BinaryReader br, long binoffset)
        {
            this.binoffset = binoffset;
            animationoffsetspointer = br.ReadInt32();
            animationspointer = br.ReadInt32();
            unknownpointer = br.ReadInt32();
            framespointer = br.ReadInt32();
            u1 = br.ReadByte();
            u2 = br.ReadByte();
            u3 = br.ReadByte();
            u4 = br.ReadByte();
            u5 = br.ReadByte();
            u6 = br.ReadByte();
            u7 = br.ReadByte();
            u8 = br.ReadByte();
            u9 = br.ReadByte();
            u10 = br.ReadByte();
            u11 = br.ReadByte();
            u12 = br.ReadByte();
            u13 = br.ReadByte();
            u14 = br.ReadByte();
            u15 = br.ReadByte();
            u16 = br.ReadByte();
        }
        public long binoffset;

        public int animationoffsetspointer;
        public int animationspointer;
        public int unknownpointer;
        public int framespointer;
        public byte u1;
        public byte u2;
        public byte u3;
        public byte u4;
        public byte u5;
        public byte u6;
        public byte u7;
        public byte u8;
        public byte u9;
        public byte u10;
        public byte u11;
        public byte u12;
        public byte u13;
        public byte u14;
        public byte u15;
        public byte u16;
    }

    public class SIAnimation
    {
        public SIAnimation(BinaryReader br, SIAnimationSetHeader header)
        {
            frames = new SIFrame[32];//32 max frames?
            for (int dex = 0; dex < frames.Length; dex++)
            {
                //read two test bytes to check for the end of the list
                short test = br.ReadInt16();
                if (test == 0)
                    break;
                numframes++;
                br.BaseStream.Position -= 2;

                frames[dex] = new SIFrame(br, header);
            }
        }
        public int numframes;
        public SIFrame[] frames;
    }

    public class SIFrame
    {
        public SIFrame(BinaryReader br, SIAnimationSetHeader header)
        {
            delay = br.ReadByte();
            unknown = br.ReadInt16();
            imagesetpointer = br.ReadInt16() * 2;


            //load images
            long savepos = br.BaseStream.Position;

            br.BaseStream.Position = header.binoffset + header.framespointer + imagesetpointer;
            images = new SIImageSet(br);

            br.BaseStream.Position = savepos;
        }



        public byte delay;//top bit masked
        public short unknown;//-1
        public int imagesetpointer;
        public SIImageSet images;
    }

    public class SIImageSet
    {
        public SIImageSet(BinaryReader br)
        {
            palette = br.ReadByte();//palette?
            numimages = br.ReadByte();
            images = new SIImage[numimages];
            for (int dex = 0; dex < numimages; dex++)
            {
                images[dex] = new SIImage(br);
            }
        }

        public byte palette;
        public byte numimages;
        public SIImage[] images;
    }

    public class SIImage
    {
        public SIImage(BinaryReader br)
        {
            u1 = br.ReadByte();
            u2 = br.ReadByte();
            sx = br.ReadByte();
            sy = br.ReadByte();
            swidth = br.ReadByte();
            sheight = br.ReadByte();
            x1 = br.ReadSByte();
            y1 = br.ReadSByte();
            x2 = br.ReadSByte();
            y2 = br.ReadSByte();
            x3 = br.ReadSByte();
            y3 = br.ReadSByte();
            x4 = br.ReadSByte();
            y4 = br.ReadSByte();
        }

        public byte u1;
        public byte u2;
        public byte sx;
        public byte sy;
        public byte swidth;
        public byte sheight;
        public sbyte x1;
        public sbyte y1;
        public sbyte x2;
        public sbyte y2;
        public sbyte x3;
        public sbyte y3;
        public sbyte x4;
        public sbyte y4;
    }
}
