using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace GraphicsTools
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct bmp_header
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
    public struct bitmapinfo_header
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

    public class palette
    {
        public palette(int num_colors)
        {
            red_bitmask = 0xff << 16;
            green_bitmask = 0xff << 8;
            blue_bitmask = 0xff;
            colors = new Color[num_colors];
        }

        public uint red_bitmask;
        public uint green_bitmask;
        public uint blue_bitmask;
        public Color[] colors;

        public void Write(Stream stream)
        {
            var bw = new BinaryWriter(stream);
            bw.Write(red_bitmask);
            bw.Write(green_bitmask);
            bw.Write(blue_bitmask);
            for (int dex = 0; dex < colors.Length; dex++)
            {
                bw.Write(colors[dex].R);
                bw.Write(colors[dex].G);
                bw.Write(colors[dex].B);
            }
        }
    }

    public class bmp
    {
        public bmp(int width, int height, short bpp)
        {
            bmph.signature = (byte)'B' | ((byte)'M' << 8);

            dibh.header_size = Marshal.SizeOf(dibh);
            dibh.planes = 1;
            dibh.image_width = width;
            dibh.image_height = height;
            dibh.bpp = bpp;
            dibh.compression = 3;
            dibh.image_size = (uint)rowsize * (uint)Math.Abs(height);
            dibh.pixels_per_meter_x = 2835;
            dibh.pixels_per_meter_y = 2835;
            dibh.palette_size = 0;
            dibh.important_color_count = 0;
            bmph.pixel_offset = (uint)(Marshal.SizeOf(bmph) + dibh.header_size + 12 + (uint)dibh.palette_size * 3);
            bmph.pixel_offset += 4 - bmph.pixel_offset % 4;
            bmph.file_size = bmph.pixel_offset + dibh.image_size;
            pixels = new byte[dibh.image_size];
            pal = new palette(0);
            pal.red_bitmask = 0x7c00;
            pal.green_bitmask = 0x03e0;
            pal.blue_bitmask = 0x001f;
            //dibh.red_bitmask = 0x7c00;
            //dibh.green_bitmask = 0x03e0;
            //dibh.blue_bitmask = 0x001f;
        }
        public bmp_header bmph;
        public bitmapinfo_header dibh;
        public palette pal;
        public byte[] pixels;

        public int rowsize
        {
            get
            {
                return ((dibh.bpp * dibh.image_width + 31) / 32) * 4;
            }
        }

        public void Write(Stream stream)
        {
            bmph.Write(stream);
            dibh.Write(stream);
            pal.Write(stream);
            stream.Position = bmph.pixel_offset;
            stream.Write(pixels, 0, (int)dibh.image_size);
        }
    }
}
