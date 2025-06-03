using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using GraphicsTools;

namespace alundramultitool.carpet
{
    public class DataDir
    {
        string path;
        public DataDir(string path)
        {
            this.path = path;
            MSPR = new SpriteSet(Path.Combine(path, "mspr0-0"));

            MainPalette = Get8bpp24bPalette(Path.Combine(path, "pal0-0.dat"));


        }
        public System.Drawing.Color[]  MainPalette;

        public SpriteSet MSPR;

        public Color[] Get8bpp24bPalette(string file)
        {
            byte[] paldata = File.ReadAllBytes(file);
            Color[] pal = new Color[256];
            int dex = 0;
            for (int c = 0; c < 256; c++)
            {
                byte red = paldata[dex++];
                byte green = paldata[dex++];
                byte blue = paldata[dex++];

                pal[c] = Color.FromArgb(red, green, blue);
            }
            return pal;
        }
    }

    public class SpriteSet
    {
        public SpriteSet(string path)
        {
            var br = new BinaryReader(File.OpenRead(Path.Combine(path, ".dat")));
            numSpriteDatas = br.ReadInt16();
            data = br.ReadBytes((int)br.BaseStream.Length - 2);
            br.Close();

            br = new BinaryReader(File.OpenRead(Path.Combine(path, ".tab")));
            while (br.BaseStream.Position <= br.BaseStream.Length-6)
            {
                Tabs.Add(new SpriteTab(br));
            }

            br.Close();
        }

        public SpriteData GetSpriteData(SpriteTab tab)
        {
            return new SpriteData(tab, data, numSpriteDatas != 0);
        }

        public List<SpriteTab> Tabs = new List<SpriteTab>();
        byte[] data;
        public int numSpriteDatas;

    }
    public class SpriteTab
    {
        public SpriteTab(BinaryReader br)
        {
            offset = br.ReadInt32();
            width = br.ReadByte();
            height = br.ReadByte();
        }
        public int offset;
        public byte width;
        public byte height;
    }


    public class SpriteData
    {
        int width;
        int height;
        byte[] imagedata;
        int bpp = 8;
        public SpriteData(SpriteTab tab, byte[] data, bool rle)
        {
            width = tab.width;
            height = tab.height;
            imagedata = new byte[width * height * bpp];
            int dex = tab.offset;
            if (rle)
            {
                for (int y = 0; y < height; y++)
                {
                    int bitdex = y * width * bpp / 8;
                    int rl = (sbyte)data[dex++];
                    while (rl != 0)
                    {
                        if (rl < 0)
                        {
                            rl *= -1;
                            while (rl > 0)
                            {
                                imagedata[bitdex++] = 255;
                                rl--;
                            }
                        }
                        else if (rl > 0)
                        {
                            while (rl > 0)
                            {
                                imagedata[bitdex++] = data[dex++];
                                rl--;
                            }
                        }
                        rl = (sbyte)data[dex++];
                    }

                }
            }
            else
                throw new Exception("sprite data may be block encoded!  not supported yet.");
        }

        public System.Drawing.Bitmap GetBitmap(Color[] palette)
        {
            bmp bmp = new bmp(width, height, 24);
            int dex = 0;
            for (int y = height - 1; y >= 0; y--)
            {
                int bmpdex = 0;
                if (bpp == 8)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color c = palette[imagedata[dex]];
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.R;
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.G;
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.B;
                        dex++;

                    }
                }
            }
            var ms = new MemoryStream();
            bmp.Write(ms);
            ms.Position = 0;
            return new System.Drawing.Bitmap(ms);
        }


    }
}
