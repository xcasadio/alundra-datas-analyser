namespace PsxSdk.Graphics;

/// <summary>
/// The PlayStation's 1024 x 512 framebuffer, addressed in 16-bit words, plus the TIM upload and
/// texture-sampling rules that go with it.
///
/// JUSTIFICATION: PSX hardware adaptation only.
/// RELATION: PSX UI code does not draw from decoded images. It uploads a TIM's pixel block and its
/// colour lookup table to fixed VRAM rectangles (<c>LoadImage</c>) and then draws sprites that
/// sample a texture page at a (u, v) offset with a CLUT address. Reproducing that addressing is
/// both more faithful and simpler than trying to work out which source image a given sprite meant,
/// because several images share a texture page and later uploads deliberately overwrite earlier
/// ones.
///
/// SOURCE: psx-spx (VRAM layout, texture pages, CLUT addressing, 15-bit colour).
/// </summary>
public sealed class PsxVram
{
    /// <summary>VRAM width in 16-bit words.</summary>
    public const int Width = 1024;

    /// <summary>VRAM height in lines.</summary>
    public const int Height = 512;

    private readonly ushort[] _words = new ushort[Width * Height];

    /// <summary>Colour depth of a texture page.</summary>
    public enum BitDepth
    {
        /// <summary>4 bits per pixel, CLUT indexed. Matches <c>GetTPage</c> mode 0.</summary>
        Bpp4 = 0,

        /// <summary>8 bits per pixel, CLUT indexed. Matches <c>GetTPage</c> mode 1.</summary>
        Bpp8 = 1,

        /// <summary>16 bits per pixel, direct colour. Matches <c>GetTPage</c> mode 2.</summary>
        Bpp16 = 2,
    }

    /// <summary>Raw word access, for diagnostics.</summary>
    public ushort this[int x, int y] => (uint)x < Width && (uint)y < Height ? _words[y * Width + x] : (ushort)0;

    /// <summary>Clears the whole framebuffer.</summary>
    public void Clear() => Array.Clear(_words);

    /// <summary>
    /// Copies 16-bit words into a VRAM rectangle, the way <c>LoadImage</c> does.
    /// </summary>
    /// <param name="x">Left edge, in words.</param>
    /// <param name="y">Top edge, in lines.</param>
    /// <param name="w">Width, in words.</param>
    /// <param name="h">Height, in lines.</param>
    /// <param name="source">Source bytes, two per word, little-endian, row-major.</param>
    /// <param name="sourceOffset">Offset of the first word inside <paramref name="source"/>.</param>
    public void LoadImage(int x, int y, int w, int h, byte[] source, int sourceOffset)
    {
        ArgumentNullException.ThrowIfNull(source);

        for (var row = 0; row < h; row++)
        {
            var destinationY = y + row;
            if ((uint)destinationY >= Height)
            {
                continue;
            }

            var destination = destinationY * Width;
            var read = sourceOffset + row * w * 2;
            for (var column = 0; column < w; column++)
            {
                var destinationX = x + column;
                var offset = read + column * 2;
                if ((uint)destinationX >= Width || offset + 1 >= source.Length)
                {
                    continue;
                }

                _words[destination + destinationX] = (ushort)(source[offset] | (source[offset + 1] << 8));
            }
        }
    }

    /// <summary>Description of a TIM uploaded into VRAM.</summary>
    /// <param name="Depth">Colour depth of the pixel block.</param>
    /// <param name="HasClut">Whether the file carried a colour lookup table.</param>
    /// <param name="ClutX">VRAM x of the CLUT, in words.</param>
    /// <param name="ClutY">VRAM y of the CLUT.</param>
    /// <param name="ImageX">VRAM x of the pixel block, in words.</param>
    /// <param name="ImageY">VRAM y of the pixel block.</param>
    /// <param name="ImageWidthWords">Width of the pixel block, in words.</param>
    /// <param name="ImageHeight">Height of the pixel block, in lines.</param>
    public readonly record struct TimUpload(
        BitDepth Depth,
        bool HasClut,
        int ClutX,
        int ClutY,
        int ImageX,
        int ImageY,
        int ImageWidthWords,
        int ImageHeight);

    /// <summary>
    /// Parses a TIM and uploads its CLUT and pixel block. Destination coordinates come from the
    /// file itself unless overridden, matching <c>SetTileLayerBounds</c>, which patches the
    /// rectangles before calling <c>LoadImage</c>.
    /// </summary>
    /// <param name="data">Buffer holding the TIM.</param>
    /// <param name="offset">Offset of the TIM's magic word.</param>
    /// <param name="imageX">Destination x of the pixel block, or null to keep the file's own.</param>
    /// <param name="imageY">Destination y of the pixel block, or null to keep the file's own.</param>
    /// <param name="clutX">Destination x of the CLUT, or null to keep the file's own.</param>
    /// <param name="clutY">Destination y of the CLUT, or null to keep the file's own.</param>
    public TimUpload UploadTim(byte[] data, int offset, int? imageX = null, int? imageY = null, int? clutX = null, int? clutY = null)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (ReadU32(data, offset) != 0x10)
        {
            throw new InvalidDataException($"No TIM magic at offset 0x{offset:X}.");
        }

        var flags = ReadU32(data, offset + 4);
        var depth = (flags & 0x07) switch
        {
            0 => BitDepth.Bpp4,
            1 => BitDepth.Bpp8,
            2 => BitDepth.Bpp16,
            _ => throw new InvalidDataException($"Unsupported TIM colour depth {flags & 0x07}."),
        };

        var hasClut = (flags & 0x08) != 0;
        var position = offset + 8;

        var clutDestX = 0;
        var clutDestY = 0;
        if (hasClut)
        {
            var clutBlockSize = (int)ReadU32(data, position);
            clutDestX = clutX ?? ReadU16(data, position + 4);
            clutDestY = clutY ?? ReadU16(data, position + 6);
            var clutW = ReadU16(data, position + 8);
            var clutH = ReadU16(data, position + 10);
            LoadImage(clutDestX, clutDestY, clutW, clutH, data, position + 12);
            position += clutBlockSize;
        }

        var imageBlockSize = (int)ReadU32(data, position);
        var imageDestX = imageX ?? ReadU16(data, position + 4);
        var imageDestY = imageY ?? ReadU16(data, position + 6);
        var imageW = ReadU16(data, position + 8);
        var imageH = ReadU16(data, position + 10);
        LoadImage(imageDestX, imageDestY, imageW, imageH, data, position + 12);
        _ = imageBlockSize;

        return new TimUpload(depth, hasClut, clutDestX, clutDestY, imageDestX, imageDestY, imageW, imageH);
    }

    /// <summary>
    /// Reads a rectangle of texture out of VRAM the way the GPU samples a sprite, resolving CLUT
    /// indices and expanding 15-bit colour to 32-bit RGBA.
    /// </summary>
    /// <param name="depth">Colour depth of the texture page.</param>
    /// <param name="tpageX">Texture page origin x, in words (a multiple of 64).</param>
    /// <param name="tpageY">Texture page origin y (a multiple of 256).</param>
    /// <param name="u">Horizontal offset inside the page, in pixels of <paramref name="depth"/>.</param>
    /// <param name="v">Vertical offset inside the page, in lines.</param>
    /// <param name="w">Width in pixels.</param>
    /// <param name="h">Height in pixels.</param>
    /// <param name="clutX">CLUT x in words.</param>
    /// <param name="clutY">CLUT y.</param>
    /// <param name="destination">RGBA output, four bytes per pixel, at least w*h*4 long.</param>
    /// <param name="treatIndexZeroAsTransparent">
    /// PSX convention: for CLUT modes, index 0 is fully transparent; for 16-bit, the all-zero word
    /// is transparent.
    /// </param>
    public void ReadSprite(
        BitDepth depth,
        int tpageX,
        int tpageY,
        int u,
        int v,
        int w,
        int h,
        int clutX,
        int clutY,
        byte[] destination,
        bool treatIndexZeroAsTransparent = true)
    {
        ArgumentNullException.ThrowIfNull(destination);
        if (destination.Length < w * h * 4)
        {
            throw new ArgumentException("Destination is too small for the requested rectangle.", nameof(destination));
        }

        for (var row = 0; row < h; row++)
        {
            var sourceY = tpageY + v + row;
            var write = row * w * 4;

            for (var column = 0; column < w; column++)
            {
                var pixelX = u + column;
                ushort colour;
                var transparent = false;

                switch (depth)
                {
                    case BitDepth.Bpp4:
                    {
                        var word = this[tpageX + (pixelX >> 2), sourceY];
                        var index = (word >> ((pixelX & 3) * 4)) & 0x0F;
                        transparent = treatIndexZeroAsTransparent && index == 0;
                        colour = this[clutX + index, clutY];
                        break;
                    }

                    case BitDepth.Bpp8:
                    {
                        var word = this[tpageX + (pixelX >> 1), sourceY];
                        var index = (pixelX & 1) != 0 ? word >> 8 : word & 0xFF;
                        transparent = treatIndexZeroAsTransparent && index == 0;
                        colour = this[clutX + index, clutY];
                        break;
                    }

                    default:
                        colour = this[tpageX + pixelX, sourceY];
                        transparent = treatIndexZeroAsTransparent && colour == 0;
                        break;
                }

                // 15-bit BGR, five bits per channel, replicated into the top bits so full-scale
                // stays full-scale.
                destination[write] = (byte)(((colour & 0x1F) << 3) | ((colour & 0x1F) >> 2));
                destination[write + 1] = (byte)((((colour >> 5) & 0x1F) << 3) | (((colour >> 5) & 0x1F) >> 2));
                destination[write + 2] = (byte)((((colour >> 10) & 0x1F) << 3) | (((colour >> 10) & 0x1F) >> 2));
                destination[write + 3] = transparent ? (byte)0 : (byte)255;
                write += 4;
            }
        }
    }

    private static uint ReadU32(byte[] data, int offset) =>
        (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24));

    private static ushort ReadU16(byte[] data, int offset) =>
        (ushort)(data[offset] | (data[offset + 1] << 8));
}
