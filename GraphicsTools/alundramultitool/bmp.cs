using System.Runtime.InteropServices;

namespace GraphicsTools
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BmpHeader
    {
        public short signature;
        public uint file_size;
        public short reserved1;
        public short reserved2;
        public uint pixel_offset;

        public void Write(Stream stream)
        {
            var bw = new BinaryWriter(stream);
            bw.Write(signature);
            bw.Write(file_size);
            bw.Write(reserved1);
            bw.Write(reserved2);
            bw.Write(pixel_offset);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BitmapinfoHeader
    {
        public int header_size;
        public int image_width;
        public int image_height;
        public short planes;
        public short bpp;
        public int compression;
        public uint image_size;
        public int pixels_per_meter_x;
        public int pixels_per_meter_y;
        public int palette_size;
        public int important_color_count;
        //public uint red_bitmask;
        //public uint green_bitmask;
        //public uint blue_bitmask;
        //public uint alpha_bitmask;
        //public int color_space_type;
        //public long color_space_endpoints12;
        //public long color_space_endpoints34;
        //public long color_space_endpoints56;
        //public long color_space_endpoints78;
        //public int color_space_endpoints9;

        public void Write(Stream stream)
        {
            var bw = new BinaryWriter(stream);
            bw.Write(header_size);
            bw.Write(image_width);
            bw.Write(image_height);
            bw.Write(planes);
            bw.Write(bpp);
            bw.Write(compression);
            bw.Write(image_size);
            bw.Write(pixels_per_meter_x);
            bw.Write(pixels_per_meter_y);
            bw.Write(palette_size);
            bw.Write(important_color_count);
            //bw.Write(red_bitmask);
            //bw.Write(green_bitmask);
            //bw.Write(blue_bitmask);
        }

    }

    public class Palette
    {
        public Palette(int numColors)
        {
            RedBitmask = 0xff << 16;
            GreenBitmask = 0xff << 8;
            BlueBitmask = 0xff;
            Colors = new Color[numColors];
        }

        public uint RedBitmask;
        public uint GreenBitmask;
        public uint BlueBitmask;
        public Color[] Colors;

        public void Write(Stream stream)
        {
            var bw = new BinaryWriter(stream);
            bw.Write(RedBitmask);
            bw.Write(GreenBitmask);
            bw.Write(BlueBitmask);
            for (var dex = 0; dex < Colors.Length; dex++)
            {
                bw.Write(Colors[dex].R);
                bw.Write(Colors[dex].G);
                bw.Write(Colors[dex].B);
            }
        }
    }

    public class Bmp
    {
        public Bmp(int width, int height, short bpp)
        {
            Bmph.signature = (byte)'B' | ((byte)'M' << 8);

            Dibh.header_size = Marshal.SizeOf(Dibh);
            Dibh.planes = 1;
            Dibh.image_width = width;
            Dibh.image_height = height;
            Dibh.bpp = bpp;
            Dibh.compression = 3;
            Dibh.image_size = (uint)Rowsize * (uint)Math.Abs(height);
            Dibh.pixels_per_meter_x = 2835;
            Dibh.pixels_per_meter_y = 2835;
            Dibh.palette_size = 0;
            Dibh.important_color_count = 0;
            Bmph.pixel_offset = (uint)(Marshal.SizeOf(Bmph) + Dibh.header_size + 12 + (uint)Dibh.palette_size * 3);
            Bmph.pixel_offset += 4 - Bmph.pixel_offset % 4;
            Bmph.file_size = Bmph.pixel_offset + Dibh.image_size;
            Pixels = new byte[Dibh.image_size];
            Pal = new Palette(0);
            Pal.RedBitmask = 0x7c00;
            Pal.GreenBitmask = 0x03e0;
            Pal.BlueBitmask = 0x001f;
            //dibh.red_bitmask = 0x7c00;
            //dibh.green_bitmask = 0x03e0;
            //dibh.blue_bitmask = 0x001f;
        }
        public BmpHeader Bmph;
        public BitmapinfoHeader Dibh;
        public Palette Pal;
        public byte[] Pixels;

        public int Rowsize
        {
            get
            {
                return (Dibh.bpp * Dibh.image_width + 31) / 32 * 4;
            }
        }

        public void Write(Stream stream)
        {
            Bmph.Write(stream);
            Dibh.Write(stream);
            Pal.Write(stream);
            stream.Position = Bmph.pixel_offset;
            stream.Write(Pixels, 0, (int)Dibh.image_size);
        }
    }
}
