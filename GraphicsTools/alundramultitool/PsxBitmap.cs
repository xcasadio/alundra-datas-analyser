using System.Runtime.InteropServices;

namespace GraphicsTools
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BitmapHeader
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
    public struct BitmapInfoHeader
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
            for (var i = 0; i < Colors.Length; i++)
            {
                bw.Write(Colors[i].R);
                bw.Write(Colors[i].G);
                bw.Write(Colors[i].B);
            }
        }
    }

    public class PsxBitmap
    {
        public PsxBitmap(int width, int height, short bpp)
        {
            Header.signature = (byte)'B' | ((byte)'M' << 8);

            InfoHeader.header_size = Marshal.SizeOf(InfoHeader);
            InfoHeader.planes = 1;
            InfoHeader.image_width = width;
            InfoHeader.image_height = height;
            InfoHeader.bpp = bpp;
            InfoHeader.compression = 3;
            InfoHeader.image_size = (uint)RowSize * (uint)Math.Abs(height);
            InfoHeader.pixels_per_meter_x = 2835;
            InfoHeader.pixels_per_meter_y = 2835;
            InfoHeader.palette_size = 0;
            InfoHeader.important_color_count = 0;
            Header.pixel_offset = (uint)(Marshal.SizeOf(Header) + InfoHeader.header_size + 12 + (uint)InfoHeader.palette_size * 3);
            Header.pixel_offset += 4 - Header.pixel_offset % 4;
            Header.file_size = Header.pixel_offset + InfoHeader.image_size;
            Pixels = new byte[InfoHeader.image_size];
            Palette = new Palette(0);
            Palette.RedBitmask = 0x7c00;
            Palette.GreenBitmask = 0x03e0;
            Palette.BlueBitmask = 0x001f;
            //dibh.red_bitmask = 0x7c00;
            //dibh.green_bitmask = 0x03e0;
            //dibh.blue_bitmask = 0x001f;
        }

        public BitmapHeader Header;
        public BitmapInfoHeader InfoHeader;
        public Palette Palette;
        public byte[] Pixels;

        public int RowSize => (InfoHeader.bpp * InfoHeader.image_width + 31) / 32 * 4;

        public void Write(Stream stream)
        {
            Header.Write(stream);
            InfoHeader.Write(stream);
            Palette.Write(stream);
            stream.Position = Header.pixel_offset;
            stream.Write(Pixels, 0, (int)InfoHeader.image_size);
        }
    }
}
