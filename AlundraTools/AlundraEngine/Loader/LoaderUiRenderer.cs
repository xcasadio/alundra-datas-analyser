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
    /// The loader's ordering table holds 0x100 slots and higher indices end up on top — the fade
    /// quad sits at 200, the cursor at 0x78, the text at 0x6E, the panels at 100 and the backdrop at
    /// 0. Sprite depth follows the same order, so the whole range is mapped just below
    /// <see cref="SpriteDepth.BackgroundUI"/>.
    /// </summary>
    /// <remarks>
    /// CORRECTION: this used to be <c>SpriteDepth.BackgroundUI</c> itself. That constant is
    /// <c>int.MaxValue - 4</c>, so <c>DepthBase + otIndex</c> overflowed for any slot past 4 and
    /// came out as a large negative depth — behind everything. The title screen only ever used
    /// slots 0 and 1, which is why it never showed; the selection screen uses 0 to 200 and its
    /// backdrop fill, ornaments, panels and text all landed underneath the map.
    /// </remarks>
    private const int OrderingTableSize = 0x100;

    private const int DepthBase = SpriteDepth.BackgroundUI - OrderingTableSize;

    // Sprite bitmaps are kept for the lifetime of the loader and refilled in place when VRAM
    // changes, never disposed and rebuilt. A renderer that caches a GPU texture keyed on the
    // Bitmap instance (AlundraRenderer does) would otherwise be left holding a disposed key.
    private readonly Dictionary<SpriteKey, Bitmap> _spriteCache = new();
    private readonly Dictionary<SpriteKey, int> _spriteGeneration = new();
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
    }

    /// <summary>
    /// A cached sprite is stale as soon as VRAM has been written to since it was sampled.
    /// <see cref="PsxVram.Generation"/> counts those writes, so every upload path — a TIM, a tile
    /// layer, a single typed glyph — invalidates without having to report itself here. The bitmaps
    /// are kept and refilled in place, never disposed.
    /// </summary>
    private int VramGeneration => Vram.Generation;

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

        // The clip test is skipped entirely on the rotated path, because a rotated quad can reach
        // the screen from a base position that is off it.
        if (box.RotationZ == -1 &&
            !(posX < 0x140 && posY < 0xF0 && posX + box.Width >= 0 && posY + box.Height >= 0))
        {
            return;
        }

        var (depth, tpageX, tpageY, u, v) = ResolveSource(box);
        var clutX = box.ClutXRaw & 0x3F0;
        var bitmap = GetSprite(depth, tpageX, tpageY, u, v, box.Width, box.Height, clutX, box.ClutY);
        if (bitmap is null)
        {
            return;
        }

        // GHIDRA: the abr field selects the GPU's semi-transparency rate and -1 means opaque; the
        // primitive's semi-transparency bit is set from `abr != -1`, so the rate itself is passed
        // straight through rather than being forced to one mode.
        var blend = box.AbrOrMinus1 == -1 ? BlendMode.None : (BlendMode)box.AbrOrMinus1;

        if (box.RotationZ != -1)
        {
            RenderRotated(box, posX, posY, bitmap, blend);
            return;
        }

        // The original's colour is a modulation where 0x80 means "unchanged".
        renderer.AddSprite(
            posX, posY, box.Width, box.Height,
            DepthBase + box.OtIndex,
            bitmap,
            1f,
            box.R / 128f, box.G / 128f, box.B / 128f,
            blend);
    }

    /// <summary>
    /// GHIDRA: RenderUIBox @ 0x80025dfc, rotated path (rotationZ != -1).
    /// </summary>
    /// <remarks>
    /// CORRECTION: an earlier pass recorded this path as unreachable, on the grounds that every
    /// loader element passes -1. That is wrong. InitSaveSlotSelectionUI @ 0x80023500 gives
    /// UIBox_ARRAY_8014f390[0..1] a rotation of 0 and UIBox_ARRAY_8014f548[1..3] rotations that
    /// UpdateMenuGraphics @ 0x80023b14 advances every frame, so the selection screen's spinning
    /// ornaments and the walking sprite's shadow all take it.
    ///
    /// The original builds a POLY_FT4 whose four corners are (+/-(w-1)/2, +/-(h-1)/2) put through
    /// RotMatrix / RotTrans with only vz set — a plain Z rotation in the GTE's 1.12 fixed point,
    /// 4096 units to the turn — and translated to the box's centre. The texture coordinates stay
    /// the axis-aligned rectangle, which here is the whole sampled bitmap.
    /// </remarks>
    private void RenderRotated(UiBox box, int posX, int posY, Bitmap bitmap, BlendMode blend)
    {
        var spanX = box.Width - 1;
        var spanY = box.Height - 1;
        var halfX = spanX >> 1;
        var halfY = spanY >> 1;

        var centreX = posX + halfX;
        var centreY = posY + halfY;

        var cos = FixedCosine(box.RotationZ);
        var sin = FixedSine(box.RotationZ);

        (int X, int Y) Rotate(int x, int y) =>
            (centreX + ((cos * x - sin * y) >> 12), centreY + ((sin * x + cos * y) >> 12));

        var (x0, y0) = Rotate(-halfX, -halfY);
        var (x1, y1) = Rotate(halfX, -halfY);
        var (x2, y2) = Rotate(-halfX, halfY);
        var (x3, y3) = Rotate(halfX, halfY);

        renderer.DrawDeformedQuad(
            bitmap,
            x0, y0, 0, 0,
            x1, y1, spanX, 0,
            x2, y2, 0, spanY,
            x3, y3, spanX, spanY,
            DepthBase + box.OtIndex,
            box.R, box.G, box.B,
            1f,
            blend);
    }

    /// <summary>PsyQ <c>ccos</c> / <c>csin</c>: 1.12 fixed point, 4096 units to a full turn.</summary>
    /// <remarks>GHIDRA: ccos @ 0x80030910, csin @ 0x80030918.</remarks>
    private static int FixedCosine(int angle) =>
        (int)Math.Round(Math.Cos(angle * 2.0 * Math.PI / 4096.0) * 4096.0);

    private static int FixedSine(int angle) =>
        (int)Math.Round(Math.Sin(angle * 2.0 * Math.PI / 4096.0) * 4096.0);

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

        var x0 = box.BaseX + box.FlatOffsetX;
        var y0 = box.BaseY + box.FlatOffsetY;
        var x1 = x0 + box.OffsetX;

        // Not a typo, and not a decompiler artifact: the original really computes the bottom edge
        // from x0, not y0 (`iVar8 = iVar6 + uiBox->offsetY`). Every quad that reaches this path is
        // positioned at x0 = 0, so the two agree and the quirk has never been visible.
        var y1 = x0 + box.OffsetY;

        if (x0 >= 0x140 || y0 >= 0xF0 || x1 < 0 || y1 < 0)
        {
            return;
        }

        var abr = box.FlatAbr;
        var blend = abr == -1 ? BlendMode.None : (BlendMode)abr;

        renderer.DrawColoredRectangle(
            (short)x0, (short)y0, (short)(x1 - x0), (short)(y1 - y0),
            DepthBase + box.OtIndex,
            1f,
            box.FlatColorR / 255f, box.FlatColorG / 255f, box.FlatColorB / 255f,
            blend);
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
        var cached = _spriteCache.TryGetValue(key, out var existing);
        if (cached && _spriteGeneration.GetValueOrDefault(key, -1) == VramGeneration)
        {
            return existing;
        }

        var required = w * h * 4;
        if (_pixelScratch.Length < required)
        {
            _pixelScratch = new byte[required];
        }

        Vram.ReadSprite(depth, tpageX, tpageY, u, v, w, h, clutX, clutY, _pixelScratch);

        var bitmap = cached ? existing! : new Bitmap(w, h, PixelFormat.Format32bppArgb);
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
        _spriteGeneration[key] = VramGeneration;

        if (cached)
        {
            // Same instance, new pixels: the renderer's GPU copy has to be refreshed.
            renderer.InvalidateTexture(bitmap);
        }

        return bitmap;
    }

    public void Dispose()
    {
        foreach (var bitmap in _spriteCache.Values)
        {
            bitmap.Dispose();
        }

        _spriteCache.Clear();
        _spriteGeneration.Clear();
    }
}
