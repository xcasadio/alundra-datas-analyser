using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AlundraEngine.Loader;

/// <summary>
/// Reusable <see cref="Bitmap"/> fed from a 24-bit RGB frame buffer.
///
/// JUSTIFICATION: PSX hardware adaptation only.
/// RELATION: on the console the decoded movie frame is DMA'd straight into VRAM by
/// <c>LoadImage</c> (GHIDRA: FUN_80027a4c @ 0x80027a4c) and shown by pointing the display
/// environment at that VRAM rect. There is no VRAM here, so the frame is uploaded into a Bitmap
/// that <see cref="Graphics.IRenderer"/> can sample - the same "LoadImage becomes a texture
/// upload" adaptation already used elsewhere in this codebase.
/// </summary>
public sealed class MovieFrameBitmap : IDisposable
{
    // One Bitmap per distinct movie size, kept alive for the lifetime of the loader. A renderer
    // that caches a GPU texture keyed on the Bitmap instance (AlundraRenderer does) would
    // otherwise accumulate one orphaned texture per size change; the attract loop alternates
    // between a 320x160 and a 304x224 movie forever, so those changes keep coming.
    private readonly Dictionary<(int Width, int Height), Bitmap> _bitmaps = new();
    private Bitmap? _bitmap;

    /// <summary>The most recently uploaded frame, or null before the first upload.</summary>
    public Bitmap? Bitmap => _bitmap;

    /// <summary>
    /// Copies <paramref name="rgb24"/> into the bitmap for this movie size, allocating one the
    /// first time a given size is seen and reusing it afterwards.
    /// </summary>
    public Bitmap Update(byte[] rgb24, int width, int height)
    {
        if (!_bitmaps.TryGetValue((width, height), out var target))
        {
            target = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            _bitmaps[(width, height)] = target;
        }

        _bitmap = target;

        var data = _bitmap.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format24bppRgb);

        try
        {
            // GDI+ stores Format24bppRgb as B, G, R per pixel while the decoder produces R, G, B,
            // so the two outer channels are swapped during the copy. Rows are padded to a
            // four-byte stride, hence the per-row destination pointer.
            var row = new byte[width * 3];
            for (var y = 0; y < height; y++)
            {
                var source = y * width * 3;
                for (var x = 0; x < width * 3; x += 3)
                {
                    row[x] = rgb24[source + x + 2];
                    row[x + 1] = rgb24[source + x + 1];
                    row[x + 2] = rgb24[source + x];
                }

                Marshal.Copy(row, 0, data.Scan0 + y * data.Stride, row.Length);
            }
        }
        finally
        {
            _bitmap.UnlockBits(data);
        }

        return _bitmap;
    }

    public void Dispose()
    {
        foreach (var bitmap in _bitmaps.Values)
        {
            bitmap.Dispose();
        }

        _bitmaps.Clear();
        _bitmap = null;
    }
}
