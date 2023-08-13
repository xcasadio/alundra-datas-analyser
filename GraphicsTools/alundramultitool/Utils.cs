using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace GraphicsTools
{
    public static class Utils
    {
        public static Color FromPsxColor(byte b1, byte b2)
        {
            return Color.FromArgb(255, b1 & 0x7c, ((b2 & 0xe0) >> 2) | ((b1 & 0x3) << 6), (b2 & 0x1f) << 3);
        }

        public static Color FromPsxColor(int c)
        {
            //return Color.FromArgb((c & 0x1f) << 3, (c & (0x1f << 5)) >> 2, (c & (0x1f << 10)) >> 7);
            //return Color.FromArgb((c & 0x1f) << 3, (c & 0x1f) << 3, (c & 0x1f) << 3);
            //return Color.FromArgb((c & (0x1f << 5)) >> 2, (c & (0x1f << 5)) >> 2, (c & (0x1f << 5)) >> 2);
            return Color.FromArgb(((c != 0) ? 255 : 0),  (c & (0x1f << 10)) >> 7,
                                    (c & (0x1f << 5)) >> 2,
                                    (c & 0x1f) << 3
                                 );
        }

        public static int Deflate(byte[]data,byte[]dest)
        {
                //compressed
                int dex = 0;
                int buffdex = 0;
                while (dex < dest.Length && buffdex < data.Length)
                {
                    byte b = data[buffdex++];
                    if (b == 0xad)
                    {
                        int seek = data[buffdex++];
                        if (seek == 0)
                        {
                            dest[dex++] = b;
                        }
                        else
                        {
                            int len = data[buffdex++];
                            int seekdex = dex - seek;
                            while (len-- > 0)
                                dest[dex++] = dest[seekdex++];
                        }
                    }
                    else
                        dest[dex++] = b;
                }
                return dex;
        }

        public static Bitmap BitmapFromPsxBuff(byte[] imagedata, int width, int height, int bpp, Color[] pal)
        {
            //bmp bmp = new bmp(width, height, 32);
            int rowsize = ((32 * width + 31) / 32) * 4;
            byte[] pixels = new byte[rowsize * Math.Abs(height)];

            if (bpp == 16)
            {
                int dex = 0;
                for (int y = 0; y < height; y++)
                {
                    int bmpdex = 0;
                    for (int x = 0; x < width; x++)
                    {
                        byte b2 = imagedata[dex++];
                        byte b1 = imagedata[dex++];
                        Color c = Utils.FromPsxColor((b1 << 8) | b2);
                        pixels[y * rowsize + bmpdex++] = c.R;
                        pixels[y * rowsize + bmpdex++] = c.G;
                        pixels[y * rowsize + bmpdex++] = c.B;
                        pixels[y * rowsize + bmpdex++] = c.A;
                    }
                }
                
            }
            else if (bpp == 4 && pal != null)
            {
                
                int dex = 0;
                for (int y = 0; y < height; y++)
                {
                    int bmpdex = 0;
                    for (int x = 0; x < width / 2; x++)
                    {
                        Color c = pal[imagedata[dex] & 0xf];

                        pixels[y * rowsize + bmpdex++] = c.R;
                        pixels[y * rowsize + bmpdex++] = c.G;
                        pixels[y * rowsize + bmpdex++] = c.B;
                        pixels[y * rowsize + bmpdex++] = c.A;
                        c = pal[(imagedata[dex] & 0xf0) >> 4];
                        pixels[y * rowsize + bmpdex++] = c.R;
                        pixels[y * rowsize + bmpdex++] = c.G;
                        pixels[y * rowsize + bmpdex++] = c.B;
                        pixels[y * rowsize + bmpdex++] = c.A;
                        dex++;

                    }
                }
            }

            //var ms = new MemoryStream();
            //bmp.Write(ms);
            //ms.Position = 0;
            var bitmap = new Bitmap(width,height,System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bdata = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bdata.Scan0, pixels.Length);
            bitmap.UnlockBits(bdata);
            return bitmap;

        }

        public static ushort RGB(byte r, byte g, byte b)
        {
            return (ushort)(((r & ~7) << 7) | ((g & ~7) << 2) | (b >> 3));
        }

        public static int RGB24(byte r, byte g, byte b)
        {
            return r | (g << 8) | (b << 16);
        }

        public static byte RED24(int color)
        {
            return (byte)(color & 0xff);
        }
        public static byte GREEN24(int color)
        {
            return (byte)((color & 0xff00)>>8);
        }
        public static byte BLUE24(int color)
        {
            return (byte)((color & 0xff0000)>>16);
        }
        public static byte RED(ushort color)
        {
            return (byte)(((color >> 10) & 255) << 3);
        }
        public static byte GREEN(ushort color)
        {
            return (byte)(((color >> 5) & 255) << 3);
        }
        public static byte BLUE(ushort color)
        {
            return (byte)((color & 31) << 3);
        }

        public static ushort ToRbg15(this Color c)
        {
            return RGB(c.R, c.G, c.B);
        }

        public static int ToRbg24(this Color c)
        {
            return RGB24(c.R, c.G, c.B);
        }


        public static List<Color> MedianCut(ref int[] cbuff, int maxcubes)
        {
            List<Cube> cubes = new List<Cube>();

            //first cube has all colors
            var cube = new Cube();
            cube.level = 0;
            for (int dex = 0; dex < cbuff.Length; dex++)
            {
                if (cbuff[dex] > 0)
                {
                    cube.colors.Add(new ColorEntry
                    {
                        count = cbuff[dex],
                        color = Color.FromArgb(RED24(dex), GREEN24(dex), BLUE24(dex))
                    });
                    cube.count += cbuff[dex];
                }
            }
            CalcMinMax(cube);
            cubes.Add(cube);
            //build cubes
            while (cubes.Count < maxcubes)
            {
                int level = 255;
                int splitpos = -1;
                for (int dex = 0; dex < cubes.Count; dex++)
                {
                    if (cubes[dex].colors.Count > 1 && cubes[dex].level < level)
                    {
                        level = cubes[dex].level;
                        splitpos = dex;
                    }
                }
                if (splitpos == -1)
                    break;//no more to split

                cube = cubes[splitpos];
                //sort by widest color range
                var cdif = Color.FromArgb(cube.max.R - cube.min.R, cube.max.G - cube.min.G, cube.max.B - cube.min.B);
                if (cdif.R >= cdif.G && cdif.R >= cdif.B)
                    cube.colors = cube.colors.OrderBy(x => x.color.R).ToList();
                else if (cdif.G >= cdif.R && cdif.G >= cdif.B)
                    cube.colors = cube.colors.OrderBy(x => x.color.G).ToList();
                else if (cdif.B >= cdif.R && cdif.B >= cdif.G)
                    cube.colors = cube.colors.OrderBy(x => x.color.B).ToList();

                //split cubes by half of count
                var cubea = new Cube();
                var cubeb = new Cube();
                foreach (var ce in cube.colors)
                {
                    if (cubea.count >= cube.count / 2 || cube.colors.IndexOf(ce) == cube.colors.Count -1)
                    {
                        cubeb.colors.Add(ce);
                        cubeb.count += ce.count;
                    }
                    else
                    {
                        cubea.colors.Add(ce);
                        cubea.count += ce.count;
                    }
                }


                Debug.Assert(cubea.colors.Count > 0 && cubeb.colors.Count > 0);

                cubea.level = cube.level + 1;
                CalcMinMax(cubea);
                cubeb.level = cube.level + 1;
                CalcMinMax(cubeb);

                //remove split cube
                cubes.RemoveAt(splitpos);
                //add new cubes
                cubes.Insert(splitpos, cubea);
                cubes.Add(cubeb);

            }

            return BuildPalette(cubes, ref cbuff, false);
        }

        static float ColorDistance(Color a, Color b)
        {
            float x = a.R - b.R;
            float y = a.G - b.G;
            float z = a.B - b.B;
            return ((x * x) + (y * y) + (z * z));
        }
        static List<Color> BuildPalette(List<Cube> cubes, ref int[] remapper, bool fast = false)
        {
            //build the color map
            List<Color> cmap = new List<Color>();


            foreach (var cube in cubes)
            {
                float rsum = 0;
                float gsum = 0;
                float bsum = 0;
                foreach (var ce in cube.colors)
                {
                    rsum += ce.color.R * ce.count;
                    gsum += ce.color.G * ce.count;
                    bsum += ce.color.B * ce.count;
                }
                cmap.Add(Color.FromArgb((int)(rsum / cube.count), (int)(gsum / cube.count), (int)(bsum / cube.count)));

            }
            if (fast)
            {
                for (int dex = 0; dex < cubes.Count; dex++)
                {
                    foreach (var ce in cubes[dex].colors)
                    {
                        remapper[RGB24(ce.color.R, ce.color.G, ce.color.B)] = dex;
                    }
                }
            }
            else
            {
                for (int dex = 0; dex < cubes.Count; dex++)
                {
                    foreach (var ce in cubes[dex].colors)
                    {
                        Color closest = cmap.First();
                        float shortestdist = float.MaxValue;
                        foreach (var c in cmap)
                        {
                            float dist = ColorDistance(c, ce.color);
                            if (dist < shortestdist)
                            {
                                shortestdist = dist;
                                closest = c;
                            }
                        }
                        remapper[RGB24(ce.color.R, ce.color.G, ce.color.B)] = cmap.IndexOf(closest);
                    }
                }
            }

            return cmap;
        }

        static void CalcMinMax(Cube cube)
        {
            cube.min = Color.FromArgb(255, 255, 255);
            cube.max = Color.FromArgb(0, 0, 0);
            foreach (var ce in cube.colors)
            {
                if (ce.color.R < cube.min.R)
                    cube.min = Color.FromArgb(ce.color.R, cube.min.G, cube.min.B);
                if (ce.color.G < cube.min.G)
                    cube.min = Color.FromArgb(cube.min.R, ce.color.G, cube.min.B);
                if (ce.color.B < cube.min.B)
                    cube.min = Color.FromArgb(cube.min.R, cube.min.G, ce.color.B);

                if (ce.color.R > cube.max.R)
                    cube.max = Color.FromArgb(ce.color.R, cube.max.G, cube.max.B);
                if (ce.color.G > cube.max.G)
                    cube.max = Color.FromArgb(cube.max.R, ce.color.G, cube.max.B);
                if (ce.color.B > cube.max.B)
                    cube.max = Color.FromArgb(cube.max.R, cube.max.G, ce.color.B);
            }
        }

        class ColorEntry
        {
            public Color color;
            public int count;
        }

        class Cube
        {
            public List<ColorEntry> colors = new List<ColorEntry>();
            public int count;
            public int level;
            public Color max;
            public Color min;
        }
    }
}
