using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AlundraEngine.Graphics;

public static class TimLoader
{
    public static Bitmap LoadTim(string path, int paletteIndex = 0, Color? transparentKey = null, int tolerance = 0)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);
        return LoadTim(br, paletteIndex, transparentKey, tolerance);
    }

    public static Bitmap LoadTim(BinaryReader br, int paletteIndex = 0, Color? transparentKey = null, int tolerance = 0)
    {
        var raw = LoadTimRaw(br, transparentKey, tolerance);
        return DecodeBuffer(paletteIndex, raw.Width, raw.Height, raw.Bpp, raw.Palettes, raw.ImgWWords, raw.ImgData);
    }

    public record TimRawData(int Bpp, bool HasClut, Color[][]? Palettes, int Width, int Height, ushort ImgWWords, byte[] ImgData);

    // JUSTIFICATION: C# language bridge only
    // RELATION: exposes the parsed-but-undecoded TIM contents (palette + raw indexed pixel bytes)
    // for callers that need to operate on the indexed/nibble data directly (e.g. a nibble-level
    // software blit) instead of the final decoded ARGB Bitmap that LoadTim/DecodeBuffer produce.
    public static TimRawData LoadTimRaw(BinaryReader br, Color? transparentKey = null, int tolerance = 0)
    {
        uint magic = br.ReadUInt32(); // 0x10 00 00 00
        if (magic != 0x10)
        {
            throw new InvalidDataException("Not a TIM file (bad magic).");
        }

        uint flags = br.ReadUInt32();
        bool hasClut = (flags & 0x08) != 0;
        int bppCode = (int)(flags & 0x07);
        int bpp = bppCode switch { 0 => 4, 1 => 8, 2 => 16, 3 => 24, _ => throw new NotSupportedException("Unknown bpp") };
        if (bpp == 24)
        {
            throw new NotSupportedException("24bpp TIM not handled here.");
        }

        // --- Read CLUT (palette) if present ---
        Color[][] palettes = null;
        if (hasClut)
        {
            uint clutLen = br.ReadUInt32(); // includes header below (x,y,w,h)
            ushort clutX = br.ReadUInt16(); // VRAM coords, not needed for decoding
            ushort clutY = br.ReadUInt16();
            ushort clutW = br.ReadUInt16(); // colors per CLUT (16 or 256)
            ushort clutH = br.ReadUInt16(); // number of CLUTs
            int totalColors = clutW * clutH;

            palettes = new Color[clutH][];
            for (int p = 0; p < clutH; p++)
                palettes[p] = new Color[clutW];

            var key = transparentKey ?? Color.Empty;

            for (int i = 0; i < clutW * clutH; i++)
            {
                ushort c = br.ReadUInt16();              // BGR555 (+bit semi-trans)
                int r = ((c & 0x1F) << 3) | ((c & 0x1F) >> 2);
                int g = (((c >> 5) & 31) << 3) | (((c >> 5) & 31) >> 2);
                int b = (((c >> 10) & 31) << 3) | (((c >> 10) & 31) >> 2);

                bool makeTransparent = false;
                if (!key.IsEmpty)
                {
                    makeTransparent =
                        Math.Abs(r - key.R) <= tolerance &&
                        Math.Abs(g - key.G) <= tolerance &&
                        Math.Abs(b - key.B) <= tolerance;
                }
                else
                {
                    makeTransparent = (c == 0);
                }

                int a = makeTransparent ? 0 : 255;

                int clutIdx = i / clutW;
                int colorIdx = i % clutW;
                palettes[clutIdx][colorIdx] = Color.FromArgb(a, r, g, b);
            }
        }

        // --- Read image block ---
        uint imgLen = br.ReadUInt32(); // includes header below
        ushort imgX = br.ReadUInt16(); // VRAM coords, can be ignored
        ushort imgY = br.ReadUInt16();
        ushort imgWWords = br.ReadUInt16(); // width in 16-bit words
        ushort imgH = br.ReadUInt16();

        int width = bpp switch
        {
            4 => imgWWords * 4,
            8 => imgWWords * 2,
            16 => imgWWords * 1,
            _ => throw new NotSupportedException()
        };
        int height = imgH;

        int dataBytes = checked((int)imgLen - 12); // remove x,y,w,h (8 bytes) + length field was separate
        byte[] imgData = br.ReadBytes(dataBytes);

        return new TimRawData(bpp, hasClut, palettes, width, height, imgWWords, imgData);
    }

    public static Bitmap DecodeBuffer(int paletteIndex, int width, int height, int bpp, Color[][]? palettes,
        ushort imgWWords, byte[] imgData)
    {
        var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var rect = new Rectangle(0, 0, width, height);
        var data = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        try
        {
            int stride = data.Stride;
            byte[] dst = new byte[stride * height];

            if (bpp == 4)
            {
                if (palettes == null)
                {
                    throw new InvalidDataException("4bpp image without CLUT.");
                }

                var pal = palettes[Math.Clamp(paletteIndex, 0, palettes.Length - 1)];
                int bytesPerLine = imgWWords * 2;
                for (int y = 0; y < height; y++)
                {
                    int srcOff = y * bytesPerLine;
                    int dstOff = y * stride;
                    for (int bx = 0; bx < bytesPerLine; bx++)
                    {
                        byte b = imgData[srcOff + bx];
                        int x0 = bx * 2;

                        WriteBGRA(dst, dstOff + x0 * 4, pal[b & 0x0F]);
                        WriteBGRA(dst, dstOff + (x0 + 1) * 4, pal[(b >> 4) & 0x0F]);
                    }
                }
            }
            else if (bpp == 8)
            {
                if (palettes == null)
                {
                    throw new InvalidDataException("8bpp image without CLUT.");
                }

                var pal = palettes[Math.Clamp(paletteIndex, 0, palettes.Length - 1)];
                int bytesPerLine = imgWWords * 2; // 2 bytes per word = 1 pixel per byte
                for (int y = 0; y < height; y++)
                {
                    int srcOff = y * bytesPerLine;
                    int dstOff = y * stride;
                    for (int x = 0; x < bytesPerLine; x++)
                    {
                        byte idx = imgData[srcOff + x];
                        WriteBGRA(dst, dstOff + x * 4, pal[idx]);
                    }
                }
            }
            else if (bpp == 16)
            {
                int bytesPerLine = imgWWords * 2;
                for (int y = 0; y < height; y++)
                {
                    int srcOff = y * bytesPerLine;
                    int dstOff = y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        ushort c = BitConverter.ToUInt16(imgData, srcOff + x * 2);
                        int r5 = (c & 0x1F);
                        int g5 = (c >> 5) & 0x1F;
                        int b5 = (c >> 10) & 0x1F;
                        int r = (r5 << 3) | (r5 >> 2);
                        int g = (g5 << 3) | (g5 >> 2);
                        int b = (b5 << 3) | (b5 >> 2);
                        int a = (c == 0) ? 0 : 255;
                        WriteBGRA(dst, dstOff + x * 4, Color.FromArgb(a, r, g, b));
                    }
                }
            }

            Marshal.Copy(dst, 0, data.Scan0, dst.Length);
        }
        finally
        {
            bmp.UnlockBits(data);
        }

        return bmp;
    }

    private static void WriteBGRA(byte[] buf, int offset, Color c)
    {
        buf[offset + 0] = c.B;
        buf[offset + 1] = c.G;
        buf[offset + 2] = c.R;
        buf[offset + 3] = c.A;
    }
}