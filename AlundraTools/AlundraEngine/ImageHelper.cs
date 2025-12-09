using System.Diagnostics;

namespace AlundraEngine;

public static class ImageHelper
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
        return Color.FromArgb(c != 0 ? 255 : 0, (c & (0x1f << 10)) >> 7,
            (c & (0x1f << 5)) >> 2,
            (c & 0x1f) << 3
        );
    }

    public static int Deflate(byte[] data, byte[] dest)
    {
        //compressed
        var i = 0;
        var bufferIndex = 0;

        while (i < dest.Length && bufferIndex < data.Length)
        {
            var b = data[bufferIndex++];

            if (b == 0xad)
            {
                int seek = data[bufferIndex++];

                if (seek == 0)
                {
                    dest[i++] = b;
                }
                else
                {
                    int len = data[bufferIndex++];
                    var seekIndex = i - seek;

                    while (len-- > 0)
                    {
                        dest[i++] = dest[seekIndex++];
                    }
                }
            }
            else
            {
                dest[i++] = b;
            }
        }

        return i;
    }

    public static byte[] Unzip(byte[] src)
    {
        if (src.Length < 2 || (src[0] | (src[1] << 8)) != 0x5A45)
        {
            int sheet = 0x8000;
            var dataBits = new byte[sheet];
            Array.Copy(src, dataBits, Math.Min(sheet, src.Length));
            for (int i = 0; i < dataBits.Length; i++)
            {
                dataBits[i] = Bswap(dataBits[i]);
            }

            return dataBits;
        }

        int rhead = 6;
        int whead = 0;
        bool keep = true;
        var buffer = new byte[0x80000];
        while (keep && whead < buffer.Length && rhead < src.Length)
        {
            byte cur = src[rhead++];
            if (cur == 0xAD)
            {
                if (rhead >= src.Length) break;
                byte dist = src[rhead++];
                if (dist == 0)
                {
                    buffer[whead++] = Bswap(cur);
                }
                else
                {
                    if (rhead >= src.Length) break;
                    byte len = src[rhead++];
                    keep = !(cur == 0xAD && dist == 0xFF && len == 0x00);
                    int seek = whead - dist;
                    while (len-- > 0 && whead < buffer.Length && seek < buffer.Length)
                    {
                        buffer[whead] = buffer[seek];
                        whead++;
                        seek++;
                    }
                }
            }
            else
            {
                buffer[whead++] = Bswap(cur);
            }
        }

        if (whead > 0)
        {
            var result = new byte[whead];
            Array.Copy(buffer, result, whead);
            return result;
        }

        return Array.Empty<byte>();
    }

    // bswap utility (swap nibbles)
    private static byte Bswap(byte x) => (byte)(((x << 4) | (x >> 4)) & 0xFF);

    public static Bitmap BitmapFromPsxBuff(byte[] imagedata, int width, int height, int bpp, Color[] pal)
    {
        return BitmapFromPsxBuff(imagedata, 0, 0, width, height, bpp, pal);
    }

    public static Bitmap BitmapFromPsxBuff(byte[] imagedata, int u, int v, int width, int height, int bpp, Color[] pal)
    {
        //bmp bmp = new bmp(width, height, 32);
        var rowsize = (32 * width + 31) / 32 * 4;
        var pixels = new byte[rowsize * Math.Abs(height)];

        if (bpp == 16)
        {
            var dex = u + v * width;

            for (var y = 0; y < height; y++)
            {
                var bmpdex = 0;

                for (var x = 0; x < width; x++)
                {
                    var b2 = imagedata[dex++];
                    var b1 = imagedata[dex++];
                    var c = FromPsxColor((b1 << 8) | b2);
                    pixels[y * rowsize + bmpdex++] = c.R;
                    pixels[y * rowsize + bmpdex++] = c.G;
                    pixels[y * rowsize + bmpdex++] = c.B;
                    pixels[y * rowsize + bmpdex++] = c.A;
                }
            }

        }
        else if (bpp == 4 && pal != null)
        {
            var i = u + v * width;

            for (var y = 0; y < height; y++)
            {
                var bmpdex = 0;

                for (var x = 0; x < width / 2; x++)
                {
                    var c = pal[imagedata[i] & 0xf];

                    pixels[y * rowsize + bmpdex++] = c.R;
                    pixels[y * rowsize + bmpdex++] = c.G;
                    pixels[y * rowsize + bmpdex++] = c.B;
                    pixels[y * rowsize + bmpdex++] = c.A;
                    c = pal[(imagedata[i] & 0xf0) >> 4];
                    pixels[y * rowsize + bmpdex++] = c.R;
                    pixels[y * rowsize + bmpdex++] = c.G;
                    pixels[y * rowsize + bmpdex++] = c.B;
                    pixels[y * rowsize + bmpdex++] = c.A;
                    i++;

                }
            }
        }

        //var ms = new MemoryStream();
        //bmp.Write(ms);
        //ms.Position = 0;
        var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var bdata = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bdata.Scan0, pixels.Length);
        bitmap.UnlockBits(bdata);
        return bitmap;

    }

    public static ushort Rgb(byte r, byte g, byte b)
    {
        return (ushort)(((r & ~7) << 7) | ((g & ~7) << 2) | (b >> 3));
    }

    public static int Rgb24(byte r, byte g, byte b)
    {
        return r | (g << 8) | (b << 16);
    }

    public static byte Red24(int color)
    {
        return (byte)(color & 0xff);
    }

    public static byte Green24(int color)
    {
        return (byte)((color & 0xff00) >> 8);
    }

    public static byte Blue24(int color)
    {
        return (byte)((color & 0xff0000) >> 16);
    }

    public static byte Red(ushort color)
    {
        return (byte)(((color >> 10) & 255) << 3);
    }

    public static byte Green(ushort color)
    {
        return (byte)(((color >> 5) & 255) << 3);
    }

    public static byte Blue(ushort color)
    {
        return (byte)((color & 31) << 3);
    }

    public static ushort ToRbg15(this Color c)
    {
        return Rgb(c.R, c.G, c.B);
    }

    public static int ToRbg24(this Color c)
    {
        return Rgb24(c.R, c.G, c.B);
    }

    public static List<Color> MedianCut(ref int[] cbuff, int maxcubes)
    {
        var cubes = new List<Cube>();

        //first cube has all colors
        var cube = new Cube();
        cube.Level = 0;
        for (var dex = 0; dex < cbuff.Length; dex++)
        {
            if (cbuff[dex] > 0)
            {
                cube.Colors.Add(new ColorEntry
                {
                    Count = cbuff[dex],
                    Color = Color.FromArgb(Red24(dex), Green24(dex), Blue24(dex))
                });
                cube.Count += cbuff[dex];
            }
        }
        CalcMinMax(cube);
        cubes.Add(cube);
        //build cubes
        while (cubes.Count < maxcubes)
        {
            var level = 255;
            var splitpos = -1;
            for (var dex = 0; dex < cubes.Count; dex++)
            {
                if (cubes[dex].Colors.Count > 1 && cubes[dex].Level < level)
                {
                    level = cubes[dex].Level;
                    splitpos = dex;
                }
            }
            if (splitpos == -1)
            {
                break;//no more to split
            }

            cube = cubes[splitpos];
            //sort by widest color range
            var cdif = Color.FromArgb(cube.Max.R - cube.Min.R, cube.Max.G - cube.Min.G, cube.Max.B - cube.Min.B);
            if (cdif.R >= cdif.G && cdif.R >= cdif.B)
            {
                cube.Colors = cube.Colors.OrderBy(x => x.Color.R).ToList();
            }
            else if (cdif.G >= cdif.R && cdif.G >= cdif.B)
            {
                cube.Colors = cube.Colors.OrderBy(x => x.Color.G).ToList();
            }
            else if (cdif.B >= cdif.R && cdif.B >= cdif.G)
            {
                cube.Colors = cube.Colors.OrderBy(x => x.Color.B).ToList();
            }

            //split cubes by half of count
            var cubea = new Cube();
            var cubeb = new Cube();
            foreach (var ce in cube.Colors)
            {
                if (cubea.Count >= cube.Count / 2 || cube.Colors.IndexOf(ce) == cube.Colors.Count - 1)
                {
                    cubeb.Colors.Add(ce);
                    cubeb.Count += ce.Count;
                }
                else
                {
                    cubea.Colors.Add(ce);
                    cubea.Count += ce.Count;
                }
            }


            Debug.Assert(cubea.Colors.Count > 0 && cubeb.Colors.Count > 0);

            cubea.Level = cube.Level + 1;
            CalcMinMax(cubea);
            cubeb.Level = cube.Level + 1;
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
        return x * x + y * y + z * z;
    }

    static List<Color> BuildPalette(List<Cube> cubes, ref int[] remapper, bool fast = false)
    {
        //build the color map
        var cmap = new List<Color>();


        foreach (var cube in cubes)
        {
            float rsum = 0;
            float gsum = 0;
            float bsum = 0;
            foreach (var ce in cube.Colors)
            {
                rsum += ce.Color.R * ce.Count;
                gsum += ce.Color.G * ce.Count;
                bsum += ce.Color.B * ce.Count;
            }
            cmap.Add(Color.FromArgb((int)(rsum / cube.Count), (int)(gsum / cube.Count), (int)(bsum / cube.Count)));

        }
        if (fast)
        {
            for (var dex = 0; dex < cubes.Count; dex++)
            {
                foreach (var ce in cubes[dex].Colors)
                {
                    remapper[Rgb24(ce.Color.R, ce.Color.G, ce.Color.B)] = dex;
                }
            }
        }
        else
        {
            for (var dex = 0; dex < cubes.Count; dex++)
            {
                foreach (var ce in cubes[dex].Colors)
                {
                    var closest = cmap.First();
                    var shortestdist = float.MaxValue;
                    foreach (var c in cmap)
                    {
                        var dist = ColorDistance(c, ce.Color);
                        if (dist < shortestdist)
                        {
                            shortestdist = dist;
                            closest = c;
                        }
                    }
                    remapper[Rgb24(ce.Color.R, ce.Color.G, ce.Color.B)] = cmap.IndexOf(closest);
                }
            }
        }

        return cmap;
    }

    static void CalcMinMax(Cube cube)
    {
        cube.Min = Color.FromArgb(255, 255, 255);
        cube.Max = Color.FromArgb(0, 0, 0);
        foreach (var ce in cube.Colors)
        {
            if (ce.Color.R < cube.Min.R)
            {
                cube.Min = Color.FromArgb(ce.Color.R, cube.Min.G, cube.Min.B);
            }

            if (ce.Color.G < cube.Min.G)
            {
                cube.Min = Color.FromArgb(cube.Min.R, ce.Color.G, cube.Min.B);
            }

            if (ce.Color.B < cube.Min.B)
            {
                cube.Min = Color.FromArgb(cube.Min.R, cube.Min.G, ce.Color.B);
            }

            if (ce.Color.R > cube.Max.R)
            {
                cube.Max = Color.FromArgb(ce.Color.R, cube.Max.G, cube.Max.B);
            }

            if (ce.Color.G > cube.Max.G)
            {
                cube.Max = Color.FromArgb(cube.Max.R, ce.Color.G, cube.Max.B);
            }

            if (ce.Color.B > cube.Max.B)
            {
                cube.Max = Color.FromArgb(cube.Max.R, cube.Max.G, ce.Color.B);
            }
        }
    }

    class ColorEntry
    {
        public Color Color;
        public int Count;
    }

    class Cube
    {
        public List<ColorEntry> Colors = new();
        public int Count;
        public int Level;
        public Color Max;
        public Color Min;
    }
}