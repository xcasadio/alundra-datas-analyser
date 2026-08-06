using AlundraEngine.Graphics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using PsxSdk.Graphics;

namespace AlundraEngine.Loader;

/// <summary>
/// Draws the loader's <see cref="UiBox"/> elements by sampling an emulated VRAM.
///
/// GHIDRA: RenderUIBox @ 0x80025dfc, RenderUiBoxFlatQuad @ 0x80026408,
/// InitializeUiBoxBasePosition @ 0x80026320, SetTileLayerBounds @ 0x800268ac (LOADER.EXE).
///
/// JUSTIFICATION: PSX hardware adaptation only.
/// RELATION: the original builds a SPRT (or a rotated POLY_FT4) and links it into the ordering
/// table, letting the GPU sample a texture page. Here the same sampling is done on the CPU through
/// <see cref="PsxVram"/> and the result handed to <see cref="IRenderer"/>. Sampling VRAM rather
/// than picking a decoded source image is what keeps this faithful: several elements share one
/// texture page and later uploads deliberately overwrite earlier ones.
/// </summary>
public sealed class LoaderUiRenderer(IRenderer renderer) : IDisposable
{
    /// <summary>
    /// The ordering table is walked so that higher indices are drawn later, hence on top; sprite
    /// depth follows the same order.
    /// </summary>
    private const int DepthBase = SpriteDepth.BackgroundUI;

    private readonly Dictionary<SpriteKey, Bitmap> _spriteCache = new();
    private byte[] _pixelScratch = new byte[256 * 256 * 4];

    /// <summary>The emulated framebuffer every UI element samples from.</summary>
    public PsxVram Vram { get; } = new();

    private readonly record struct SpriteKey(int Depth, int TpageX, int TpageY, int U, int V, int W, int H, int ClutX, int ClutY);

    /// <summary>
    /// GHIDRA: SetTileLayerBounds @ 0x800268ac — patches a tile layer's destination rectangles and
    /// uploads it. Note the original's parameter names: <c>srcX</c> / <c>srcY</c> are in fact the
    /// CLUT's destination in VRAM, not a source.
    /// </summary>
    public void UploadTim(byte[] data, int offset, int destX, int destY, int clutDestX, int clutDestY)
    {
        Vram.UploadTim(data, offset, destX, destY, clutDestX, clutDestY);
        InvalidateSpriteCache();
    }

    /// <summary>Drops every cached sprite, e.g. after VRAM was rewritten.</summary>
    public void InvalidateSpriteCache()
    {
        foreach (var bitmap in _spriteCache.Values)
        {
            renderer.InvalidateTexture(bitmap);
            bitmap.Dispose();
        }

        _spriteCache.Clear();
    }

    /// <summary>
    /// GHIDRA: InitializeUiBoxBasePosition @ 0x80026320 — propagates the first box's base position
    /// to <paramref name="count"/> consecutive boxes and draws each of them.
    /// </summary>
    public void RenderRun(IReadOnlyList<UiBox> boxes, int firstIndex, int count)
    {
        ArgumentNullException.ThrowIfNull(boxes);
        if (count <= 0 || firstIndex >= boxes.Count)
        {
            return;
        }

        var baseX = boxes[firstIndex].BaseX;
        var baseY = boxes[firstIndex].BaseY;

        for (var i = 0; i < count && firstIndex + i < boxes.Count; i++)
        {
            var box = boxes[firstIndex + i];
            box.BaseX = baseX;

            // CORRECTION of a decompiler artifact, not of the original: the loop writes baseY
            // through a second cursor that is advanced identically, so both writes land on the
            // same element every iteration.
            box.BaseY = baseY;
            Render(box);
        }
    }

    /// <summary>GHIDRA: RenderUIBox @ 0x80025dfc, plain sprite path (rotationZ == -1).</summary>
    public void Render(UiBox box)
    {
        ArgumentNullException.ThrowIfNull(box);

        if (box.OtIndex < 0)
        {
            return;
        }

        var posX = box.BaseX + box.OffsetX;
        var posY = box.BaseY + box.OffsetY;

        if (box.RotationZ == -1 &&
            !(posX < 0x140 && posY < 0xF0 && posX + box.Width >= 0 && posY + box.Height >= 0))
        {
            return;
        }

        // BLOCKED: the rotated path builds a POLY_FT4 and runs the four corners through the GTE
        // (RotMatrix / RotTrans). No loader element ever sets a rotation - every caller passes -1 -
        // so it is not ported; a rotated box would draw unrotated here.
        var (depth, tpageX, tpageY, u, v) = ResolveSource(box);
        var clutX = box.ClutXRaw & 0x3F0;
        var bitmap = GetSprite(depth, tpageX, tpageY, u, v, box.Width, box.Height, clutX, box.ClutY);
        if (bitmap is null)
        {
            return;
        }

        // The original's colour is a modulation where 0x80 means "unchanged".
        renderer.AddSprite(
            posX, posY, box.Width, box.Height,
            DepthBase + box.OtIndex,
            bitmap,
            1f,
            box.R / 128f, box.G / 128f, box.B / 128f,
            box.AbrOrMinus1 == -1 ? BlendMode.None : BlendMode.Average);
    }

    /// <summary>
    /// GHIDRA: RenderUiBoxFlatQuad @ 0x80026408 — an untextured POLY_F4. The field reuse is the
    /// original's, not a mistake: the quad's second corner comes from OffsetX / OffsetY read as
    /// absolute coordinates, and its colour from FlatColorR / FlatColorG / the low byte of
    /// RotationZ.
    /// </summary>
    public void RenderFlatQuad(UiBox box)
    {
        ArgumentNullException.ThrowIfNull(box);

        if (box.OtIndex < 0)
        {
            return;
        }

        // The high half of RotationZ is always 0 for the three boxes that reach this path.
        var x0 = box.BaseX;

        // PARTIAL: the original reads BaseY plus the 16-bit value formed by the R and G bytes.
        // For every box that reaches here those two are written together as one short, so the
        // reconstruction below is exact.
        var y0 = box.BaseY + (short)(box.R | (box.G << 8));
        var x1 = x0 + box.OffsetX;
        var y1 = x0 + box.OffsetY;

        if (x0 >= 0x140 || y0 >= 0xF0 || x1 < 0 || y1 < 0)
        {
            return;
        }

        renderer.DrawColoredRectangle(
            (short)x0, (short)y0, (short)(x1 - x0), (short)(y1 - y0),
            DepthBase + box.OtIndex,
            1f,
            box.FlatColorR / 255f, box.FlatColorG / 255f, (byte)box.RotationZ / 255f);
    }

    /// <summary>
    /// Splits the packed source coordinates into a texture page origin and an offset inside it,
    /// exactly as RenderUIBox does per colour depth.
    /// </summary>
    private static (PsxVram.BitDepth Depth, int TpageX, int TpageY, int U, int V) ResolveSource(UiBox box)
    {
        var v = box.PackedV & 0xFF;
        var tpageY = box.PackedV & ~0xFF;

        return box.BppMode switch
        {
            1 => (PsxVram.BitDepth.Bpp8, box.PackedU & ~0x7F, tpageY, (box.PackedU & 0x7F) << 1, v),
            0 => (PsxVram.BitDepth.Bpp4, box.PackedU & ~0x3F, tpageY, (box.PackedU & 0x3F) << 2, v),
            _ => (PsxVram.BitDepth.Bpp16, box.PackedU & ~0xFF, tpageY, box.PackedU & 0xFF, v),
        };
    }

    private Bitmap? GetSprite(PsxVram.BitDepth depth, int tpageX, int tpageY, int u, int v, int w, int h, int clutX, int clutY)
    {
        if (w <= 0 || h <= 0)
        {
            return null;
        }

        var key = new SpriteKey((int)depth, tpageX, tpageY, u, v, w, h, clutX, clutY);
        if (_spriteCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var required = w * h * 4;
        if (_pixelScratch.Length < required)
        {
            _pixelScratch = new byte[required];
        }

        Vram.ReadSprite(depth, tpageX, tpageY, u, v, w, h, clutX, clutY, _pixelScratch);

        var bitmap = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        var data = bitmap.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            // ReadSprite emits R, G, B, A while GDI+ stores B, G, R, A; swap while copying.
            var row = new byte[w * 4];
            for (var y = 0; y < h; y++)
            {
                var source = y * w * 4;
                for (var x = 0; x < w * 4; x += 4)
                {
                    row[x] = _pixelScratch[source + x + 2];
                    row[x + 1] = _pixelScratch[source + x + 1];
                    row[x + 2] = _pixelScratch[source + x];
                    row[x + 3] = _pixelScratch[source + x + 3];
                }

                Marshal.Copy(row, 0, data.Scan0 + y * data.Stride, row.Length);
            }
        }
        finally
        {
            bitmap.UnlockBits(data);
        }

        _spriteCache[key] = bitmap;
        return bitmap;
    }

    public void Dispose() => InvalidateSpriteCache();
}
