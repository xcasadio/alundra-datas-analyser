using PsxSdk.Graphics;

namespace AlundraEngine.Loader;

/// <summary>
/// LOADER.EXE's tile layer: a block of paletted pixels held in RAM together with the VRAM rectangle
/// it is uploaded to.
///
/// GHIDRA: struct TileMap (44 bytes), LOADER.EXE.
/// SOURCE: Ghidra structure editor plus InitializeTileMap @ 0x80026aa8 and InitializeTileLayer
/// @ 0x80026730, which are the only two functions that fill it.
///
/// JUSTIFICATION: C# language bridge only.
/// RELATION: the original holds raw pointers into the executable (a TIM's pixel block) or into a
/// static buffer. Here each pointer becomes a (buffer, offset) pair, which is the same addressing
/// expressed in a language without pointer arithmetic. Every field keeps its original meaning; the
/// names are corrected, because Ghidra's automatic ones are wrong in three places — see the
/// per-field notes.
/// </summary>
public sealed class LoaderTileMap
{
    /// <summary>
    /// +0x00. Byte size of the CLUT block, or 0 when the layer has no palette of its own.
    /// Doubles as the "should the CLUT be uploaded" flag.
    /// </summary>
    public int ClutBlockSizeBytes;

    // +0x04 - the CLUT's destination rectangle in VRAM (RECT: x, y, w, h).
    public short ClutX;
    public short ClutY;
    public short ClutW;
    public short ClutH;

    /// <summary>+0x0C. The palette entries themselves, 16-bit each.</summary>
    public byte[]? ClutBuffer;

    public int ClutOffset;

    /// <summary>+0x10. Colour depth: 0 = 4bpp, 1 = 8bpp, 2 = 16bpp.</summary>
    public int PMode;

    /// <summary>+0x14. Size of the pixel block in VRAM words; only ever tested against zero.</summary>
    public int ImageBlockSizeBytes;

    // +0x18 - the pixel block's destination rectangle in VRAM. w is in 16-bit words, not pixels.
    public short ImageX;
    public short ImageY;
    public short ImageW;
    public short ImageH;

    /// <summary>
    /// +0x20. The pixels. GHIDRA calls this <c>imageWPixels</c>, which is wrong: it is the pixel
    /// buffer, as ClearTile @ 0x800269e8 and FUN_80026958 @ 0x80026958 both show by indexing it.
    /// </summary>
    public byte[]? Pixels;

    public int PixelOffset;

    /// <summary>
    /// +0x24. Width in pixels. GHIDRA calls this <c>imageHPixels</c> — also wrong: ClearTile uses it
    /// both as the x bound and as the row stride.
    /// </summary>
    public int WidthPixels;

    /// <summary>+0x28. Height in pixels.</summary>
    public int HeightPixels;

    /// <summary>
    /// GHIDRA: InitializeTileMap @ 0x80026aa8 — sets a layer up over a caller-owned pixel buffer,
    /// with no palette of its own.
    /// </summary>
    /// <param name="pMode">0 = 4bpp, 1 = 8bpp, 2 = 16bpp.</param>
    /// <param name="widthPixels">Width in pixels.</param>
    /// <param name="heightPixels">Height in pixels.</param>
    /// <param name="buffer">Pixel buffer, at least <c>widthPixels * heightPixels</c> pixels long.</param>
    public void InitializeTileMap(int pMode, int widthPixels, int heightPixels, byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        WidthPixels = widthPixels;
        HeightPixels = heightPixels;

        // The original divides with a rounding correction for negatives, which a positive width
        // never takes; the shift is what the compiler emitted for "/ 4" and "/ 2".
        var widthWords = pMode switch
        {
            0 => widthPixels >> 2,
            1 => widthPixels / 2,
            _ => widthPixels,
        };

        PMode = pMode;
        ImageX = 0;
        ImageY = 0;
        ImageW = (short)widthWords;
        ImageH = (short)heightPixels;
        ClutBlockSizeBytes = 0;
        ClutBuffer = null;
        ClutOffset = 0;
        Pixels = buffer;
        PixelOffset = 0;
        ImageBlockSizeBytes = widthWords * heightPixels;
    }

    /// <summary>
    /// GHIDRA: InitializeTileLayer @ 0x80026730 — points a layer straight at a TIM that is already
    /// resident, without copying anything.
    /// </summary>
    /// <returns>0 on success, -1 when the header is not a TIM, exactly as the original.</returns>
    public int InitializeTileLayer(byte[] data, int offset)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (BitConverter.ToUInt32(data, offset) != 0x10)
        {
            return -1;
        }

        var flags = BitConverter.ToUInt32(data, offset + 4);
        PMode = (int)(flags & 7);
        ClutBlockSizeBytes = (int)((flags >> 3) & 1);

        // "data" in the original is the TIM plus 12: the first field past magic, flags and the
        // length word of whichever block comes first.
        var blockData = offset + 12;

        if (ClutBlockSizeBytes != 0)
        {
            ClutBlockSizeBytes = BitConverter.ToInt32(data, offset + 8);
            ClutX = BitConverter.ToInt16(data, blockData);
            ClutY = BitConverter.ToInt16(data, blockData + 2);
            ClutW = BitConverter.ToInt16(data, blockData + 4);
            ClutH = BitConverter.ToInt16(data, blockData + 6);
            ClutBuffer = data;
            ClutOffset = blockData + 8;
        }
        else
        {
            ClutBuffer = null;
            ClutOffset = 0;
        }

        var skip = (ClutBlockSizeBytes >> 2) * 4;
        ImageBlockSizeBytes = BitConverter.ToInt32(data, blockData + skip - 4);
        ImageX = BitConverter.ToInt16(data, blockData + skip);
        ImageY = BitConverter.ToInt16(data, blockData + skip + 2);
        ImageW = BitConverter.ToInt16(data, blockData + skip + 4);
        ImageH = BitConverter.ToInt16(data, blockData + skip + 6);

        Pixels = data;
        PixelOffset = blockData + skip + 8;

        // The original leaves WidthPixels alone for any pMode outside 0..3, which cannot happen for
        // a valid TIM; the switch below covers every case it handles.
        WidthPixels = PMode switch
        {
            0 => ImageW << 2,
            1 => ImageW << 1,
            2 or 3 => ImageW,
            _ => WidthPixels,
        };

        HeightPixels = ImageH;
        return 0;
    }

    /// <summary>
    /// GHIDRA: SetTileLayerBounds @ 0x800268ac — patches whichever destination rectangles were
    /// given (-1 keeps the current value) and uploads the layer to VRAM.
    /// </summary>
    /// <remarks>
    /// Note the original's parameter names, which are misleading: <c>srcX</c> / <c>srcY</c> are the
    /// CLUT's destination, not a source. <paramref name="shouldSync"/> of -1 skips the upload
    /// entirely; every other value uploads and then waits on the GPU, which has no counterpart here.
    /// </remarks>
    public void SetTileLayerBounds(PsxVram vram, int destX, int destY, int clutDestX, int clutDestY, int shouldSync)
    {
        ArgumentNullException.ThrowIfNull(vram);

        if (destX != -1)
        {
            ImageX = (short)destX;
        }

        if (destY != -1)
        {
            ImageY = (short)destY;
        }

        if (clutDestX != -1)
        {
            ClutX = (short)clutDestX;
        }

        if (clutDestY != -1)
        {
            ClutY = (short)clutDestY;
        }

        if (shouldSync == -1)
        {
            return;
        }

        if (ClutBlockSizeBytes != 0 && ClutBuffer is not null)
        {
            vram.LoadImage(ClutX, ClutY, ClutW, ClutH, ClutBuffer, ClutOffset);
        }

        if (ImageBlockSizeBytes != 0 && Pixels is not null)
        {
            vram.LoadImage(ImageX, ImageY, ImageW, ImageH, Pixels, PixelOffset);
        }
    }

    /// <summary>
    /// GHIDRA: ClearTile @ 0x800269e8.
    /// </summary>
    /// <remarks>
    /// CORRECTION: the Ghidra name is wrong. The function writes one pixel of an arbitrary value at
    /// (x, y); "clearing" is only what the callers that pass 0 happen to do. Renamed
    /// <c>SetTileMapPixel</c> in Ghidra, with the evidence on a plate comment.
    ///
    /// Both bounds tests are unsigned in the original, so a negative coordinate fails them.
    /// </remarks>
    public void SetTileMapPixel(int x, int y, int value)
    {
        if (Pixels is null || (uint)x >= (uint)WidthPixels || (uint)y >= (uint)HeightPixels)
        {
            return;
        }

        var rowBase = y * WidthPixels;

        switch (PMode)
        {
            case 0:
            {
                var index = PixelOffset + (rowBase >> 1) + (x >> 1);
                Pixels[index] = (x & 1) == 0
                    ? (byte)((Pixels[index] & 0xF0) | (value & 0x0F))
                    : (byte)((Pixels[index] & 0x0F) | (value << 4));
                break;
            }

            case 1:
                Pixels[PixelOffset + rowBase + x] = (byte)value;
                break;

            case 2:
            {
                var index = PixelOffset + (rowBase + x) * 2;
                Pixels[index] = (byte)value;
                Pixels[index + 1] = (byte)(value >> 8);
                break;
            }
        }
    }

    /// <summary>
    /// GHIDRA: FUN_80026958 @ 0x80026958 — reads one pixel. Renamed <c>GetTileMapPixel</c>.
    /// </summary>
    /// <remarks>
    /// C# language bridge only: the original has no bounds check and would read whatever RAM sits
    /// past the buffer. Every caller feeds it coordinates taken from the font table, so the guard
    /// below never changes an in-range result; it only replaces an out-of-bounds read with 0, which
    /// the blit treats as transparent.
    /// </remarks>
    public ushort GetTileMapPixel(int x, int y)
    {
        if (Pixels is null || (uint)x >= (uint)WidthPixels || (uint)y >= (uint)HeightPixels)
        {
            return PMode == 2 ? (ushort)0 : (ushort)0;
        }

        var rowBase = y * WidthPixels;

        switch (PMode)
        {
            case 0:
            {
                var packed = Pixels[PixelOffset + (rowBase >> 1) + (x >> 1)];
                return (x & 1) != 0 ? (ushort)(packed >> 4) : (ushort)(packed & 0x0F);
            }

            case 1:
                return Pixels[PixelOffset + rowBase + x];

            case 2:
                return BitConverter.ToUInt16(Pixels, PixelOffset + (rowBase + x) * 2);

            default:
                return 0xFFFF;
        }
    }

    /// <summary>
    /// GHIDRA: FUN_80026b0c @ 0x80026b0c — copies a rectangle from another layer, keeping every
    /// pixel including the transparent ones. Renamed <c>BlitTileMapOpaque</c>.
    /// </summary>
    public void BlitOpaque(LoaderTileMap source, int destX, int destY, int sourceX, int sourceY, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(source);

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                SetTileMapPixel(destX + column, destY + row, source.GetTileMapPixel(sourceX + column, sourceY + row));
            }
        }
    }

    /// <summary>
    /// GHIDRA: FUN_80026c10 @ 0x80026c10 — copies a rectangle from another layer, skipping the
    /// pixels that would be transparent. Renamed <c>BlitTileMapTransparent</c>.
    /// </summary>
    /// <remarks>
    /// The transparency test depends on the source: an indexed layer that carries a CLUT is tested
    /// through it (a palette entry of 0 is transparent on this hardware), a 16bpp layer is tested on
    /// the colour itself, and an indexed layer with no CLUT has nothing to test against, so the
    /// original falls back to the opaque copy.
    /// </remarks>
    public void BlitTransparent(LoaderTileMap source, int destX, int destY, int sourceX, int sourceY, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.PMode < 2)
        {
            if (source.ClutBlockSizeBytes == 0 || source.ClutBuffer is null)
            {
                BlitOpaque(source, destX, destY, sourceX, sourceY, width, height);
                return;
            }

            for (var row = 0; row < height; row++)
            {
                for (var column = 0; column < width; column++)
                {
                    var value = source.GetTileMapPixel(sourceX + column, sourceY + row);
                    var entry = source.ClutOffset + value * 2;
                    if (entry + 1 >= source.ClutBuffer.Length)
                    {
                        continue;
                    }

                    if (BitConverter.ToInt16(source.ClutBuffer, entry) != 0)
                    {
                        SetTileMapPixel(destX + column, destY + row, value);
                    }
                }
            }

            return;
        }

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                var value = source.GetTileMapPixel(sourceX + column, sourceY + row);
                if (value != 0)
                {
                    SetTileMapPixel(destX + column, destY + row, value);
                }
            }
        }
    }

    /// <summary>
    /// GHIDRA: FUN_80026e20 @ 0x80026e20 — moves every row up by one and blanks the last one.
    /// Renamed <c>ScrollTileMapUpOneLine</c>.
    /// </summary>
    /// <remarks>
    /// PARTIAL: the original only implements the 4bpp case and returns without doing anything for
    /// any other depth. That dead branch is kept as-is; all three text layers are 4bpp.
    /// </remarks>
    public void ScrollUpOneLine()
    {
        if (PMode != 0 || Pixels is null)
        {
            return;
        }

        var strideBytes = WidthPixels / 2;

        for (var row = 1; row < HeightPixels; row++)
        {
            Array.Copy(Pixels, PixelOffset + row * strideBytes, Pixels, PixelOffset + (row - 1) * strideBytes, strideBytes);
        }

        Array.Clear(Pixels, PixelOffset + (HeightPixels - 1) * strideBytes, strideBytes);
    }
}
