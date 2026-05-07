using System.Drawing.Imaging;

namespace AlundraEngine.Graphics;

public class Renderer(System.Drawing.Graphics graphics, Bitmap? frameBuffer = null) : IRenderer
{
    private readonly SortedDictionary<int, List<Sprite>> _sprites = new();
    public readonly Bitmap WhiteBitmap = CreateWhiteBitmap();
    private readonly Bitmap? _frameBuffer = frameBuffer;

    private readonly Dictionary<QuadColorKey, Bitmap> _quadColorCache = new();
    private readonly Dictionary<RectangleColorKey, Bitmap> _rectangleCache = new();
    private readonly Dictionary<CrossColorKey, Bitmap> _crossCache = new();
    private readonly Dictionary<LineColorKey, Bitmap> _lineCache = new();
    private readonly Dictionary<TextColorKey, Bitmap> _textCache = new();
    private readonly int _crossSize = 5;

    private const int MaxCacheSize = 10000;

    private static Bitmap CreateWhiteBitmap()
    {
        var bmp = new Bitmap(StaticVariables.ScreenWidth, StaticVariables.ScreenHeight);

        for (int i = 0; i < StaticVariables.ScreenWidth * StaticVariables.ScreenHeight; i++)
        {
            bmp.SetPixel(i % StaticVariables.ScreenWidth, i / StaticVariables.ScreenWidth, System.Drawing.Color.White);
        }
        return bmp;
    }

    public void AddSprite(SPRT sprt, int depthSortValue, Bitmap bitmap, float alpha = 1.0f, float r = 1.0f, float g = 1.0f, float b = 1.0f)
    {
        AddSprite(sprt.x0, sprt.y0, sprt.w, sprt.h, depthSortValue, bitmap, alpha, r, g, b);
    }

    public void AddSprite(int x, int y, int width, int height, int depthSortValue, Bitmap bitmap, float alpha = 1.0f, float r = 1.0f, float g = 1.0f, float b = 1.0f, BlendMode blendMode = BlendMode.None)
    {
        var sprite = new Sprite(x, y, width, height, depthSortValue, bitmap, alpha, r, g, b, blendMode);
        AddSprite(sprite);
    }

    public void AddSprite(Sprite sprite)
    {
        if (!_sprites.TryGetValue(sprite.Depth, out var value))
        {
            value = [];
            _sprites[sprite.Depth] = value;
        }

        value.Add(sprite);
    }

    public void AddRectangle(TILE tile, int depthSortValue, float alpha = 1.0f)
    {
        if (tile.w <= 0 || tile.h <= 0) return;

        var cacheKey = new RectangleColorKey(tile.w, tile.h, tile.r0, tile.g0, tile.b0);

        if (!_rectangleCache.TryGetValue(cacheKey, out var bitmap))
        {
            if (_rectangleCache.Count >= MaxCacheSize)
            {
                ClearRectangleCache();
            }

            var color = Color.FromArgb(255, tile.r0, tile.g0, tile.b0);
            bitmap = new Bitmap(tile.w, tile.h);
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                using var brush = new SolidBrush(color);
                g.FillRectangle(brush, 0, 0, tile.w, tile.h);
            }

            _rectangleCache[cacheKey] = bitmap;
        }

        AddSprite(tile.x0, tile.y0, tile.w, tile.h, depthSortValue, bitmap, alpha);
    }

    public void AddQuadColor(POLY_G4 polyG4, int depthSortValue, float alpha = 1.0f, float r = 1.0f, float g = 1.0f, float b = 1.0f)
    {
        int minX = Math.Min(Math.Min(polyG4.x0, polyG4.x1), Math.Min(polyG4.x2, polyG4.x3));
        int maxX = Math.Max(Math.Max(polyG4.x0, polyG4.x1), Math.Max(polyG4.x2, polyG4.x3));
        int minY = Math.Min(Math.Min(polyG4.y0, polyG4.y1), Math.Min(polyG4.y2, polyG4.y3));
        int maxY = Math.Max(Math.Max(polyG4.y0, polyG4.y1), Math.Max(polyG4.y2, polyG4.y3));

        int width = maxX - minX;
        int height = maxY - minY;

        if (width <= 0 || height <= 0) return;

        var cacheKey = new QuadColorKey(
            width, height,
            polyG4.r0, polyG4.g0, polyG4.b0,
            polyG4.r1, polyG4.g1, polyG4.b1,
            polyG4.r2, polyG4.g2, polyG4.b2,
            polyG4.r3, polyG4.g3, polyG4.b3);

        if (!_quadColorCache.TryGetValue(cacheKey, out var bitmap))
        {
            if (_quadColorCache.Count >= MaxCacheSize)
            {
                ClearQuadCache();
            }

            bitmap = CreateGradientBitmap(width, height, polyG4);
            _quadColorCache[cacheKey] = bitmap;
        }

        AddSprite(minX, minY, width, height, depthSortValue, bitmap, alpha, r, g, b);
    }

    public void Render()
    {
        foreach (var kvp in _sprites)
        {
            foreach (var sprite in kvp.Value)
            {
                if (sprite.IsDeformed)
                {
                    RenderDeformedSprite(graphics, sprite);
                }
                else
                {
                    RenderSprite(graphics, sprite);
                }
            }
        }
    }

    private void RenderSprite(System.Drawing.Graphics graphics, Sprite sprite)
    {
        // Note: GDI+ ne supporte pas nativement les blend modes PSX additif/soustractif.
        // Average/AdditiveDim use the source factor from the C port shader as a desktop approximation.
        float effectiveAlpha = sprite.BlendMode switch
        {
            BlendMode.Average => sprite.Alpha * 0.5f,
            BlendMode.AdditiveDim => sprite.Alpha * 0.25f,
            _ => sprite.Alpha,
        };

        var useMatrix = Math.Abs(effectiveAlpha - 1.0f) > 0.001f ||
                        Math.Abs(sprite.R - 1.0f) > 0.001f ||
                        Math.Abs(sprite.G - 1.0f) > 0.001f ||
                        Math.Abs(sprite.B - 1.0f) > 0.001f;

        if (useMatrix)
        {
            var colorMatrix = new ColorMatrix([
                [sprite.R, 0f, 0f, 0f, 0f],
                [0f, sprite.G, 0f, 0f, 0f],
                [0f, 0f, sprite.B, 0f, 0f],
                [0f, 0f, 0f, effectiveAlpha, 0f],
                [0f, 0f, 0f, 0f, 1f]
            ]);

            using var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            var destRect = new Rectangle(sprite.X, sprite.Y, sprite.Width, sprite.Height);
            graphics.DrawImage(sprite.Bitmap, destRect, 0, 0, sprite.Bitmap.Width, sprite.Bitmap.Height, GraphicsUnit.Pixel, imageAttributes);
        }
        else
        {
            graphics.DrawImage(sprite.Bitmap, sprite.X, sprite.Y, sprite.Width, sprite.Height);
        }
    }

    private void RenderDeformedSprite(System.Drawing.Graphics graphics, Sprite sprite)
    {
        // Utiliser DrawImage avec 3 points pour créer un parallélogramme
        // Note: GDI+ ne supporte que les parallélogrammes, pas les quads arbitraires
        // Pour un quad complet, il faudrait utiliser TextureBrush.Transform ou diviser en 2 triangles
        
        var destPoints = new PointF[]
        {
            new PointF(sprite.X0, sprite.Y0),  // Top-left
            new PointF(sprite.X1, sprite.Y1),  // Top-right
            new PointF(sprite.X2, sprite.Y2)   // Bottom-left
        };
        
        // Calculer le rectangle source en fonction des UVs
        int srcX = (int)(sprite.U0 * sprite.Bitmap.Width);
        int srcY = (int)(sprite.V0 * sprite.Bitmap.Height);
        int srcWidth = (int)((sprite.U1 - sprite.U0) * sprite.Bitmap.Width);
        int srcHeight = (int)((sprite.V2 - sprite.V0) * sprite.Bitmap.Height);
        
        var srcRect = new RectangleF(srcX, srcY, Math.Max(1, srcWidth), Math.Max(1, srcHeight));
        
        // Appliquer la couleur et l'alpha
        var useMatrix = Math.Abs(sprite.Alpha - 1.0f) > 0.001f ||
                        Math.Abs(sprite.R - 1.0f) > 0.001f ||
                        Math.Abs(sprite.G - 1.0f) > 0.001f ||
                        Math.Abs(sprite.B - 1.0f) > 0.001f;
        
        if (useMatrix)
        {
            var colorMatrix = new ColorMatrix([
                [sprite.R, 0f, 0f, 0f, 0f],
                [0f, sprite.G, 0f, 0f, 0f],
                [0f, 0f, sprite.B, 0f, 0f],
                [0f, 0f, 0f, sprite.Alpha, 0f],
                [0f, 0f, 0f, 0f, 1f]
            ]);
            
            using var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            
            graphics.DrawImage(sprite.Bitmap, destPoints, srcRect, GraphicsUnit.Pixel, imageAttributes);
        }
        else
        {
            graphics.DrawImage(sprite.Bitmap, destPoints, srcRect, GraphicsUnit.Pixel);
        }
    }

    public void Clear()
    {
        _sprites.Clear();
    }

    // JUSTIFICATION: backend renderer adaptation only
    public Bitmap? CaptureFrameBuffer()
    {
        return _frameBuffer == null ? null : (Bitmap)_frameBuffer.Clone();
    }

    public void ClearQuadCache()
    {
        foreach (var bitmap in _quadColorCache.Values)
        {
            bitmap.Dispose();
        }
        _quadColorCache.Clear();
    }

    public void ClearRectangleCache()
    {
        foreach (var bitmap in _rectangleCache.Values)
        {
            bitmap.Dispose();
        }
        _rectangleCache.Clear();
    }

    public void ClearCrossCache()
    {
        foreach (var bitmap in _crossCache.Values)
        {
            bitmap.Dispose();
        }
        _crossCache.Clear();
    }

    public void ClearLineCache()
    {
        foreach (var bitmap in _lineCache.Values)
        {
            bitmap.Dispose();
        }
        _lineCache.Clear();
    }

    public void ClearTextCache()
    {
        foreach (var bitmap in _textCache.Values)
        {
            bitmap.Dispose();
        }
        _textCache.Clear();
    }

    public void DrawDeformedQuad(
        Bitmap bitmap,
        int x0, int y0, float u0, float v0,
        int x1, int y1, float u1, float v1,
        int x2, int y2, float u2, float v2,
        int x3, int y3, float u3, float v3,
        int depthSortValue,
        byte r, byte g, byte b, float alpha = 1.0f,
        BlendMode blendMode = BlendMode.None)
    {
        var sprite = new Sprite(
            bitmap,
            x0, y0, u0, v0,
            x1, y1, u1, v1,
            x2, y2, u2, v2,
            x3, y3, u3, v3,
            depthSortValue,
            PsxColorMultiplier(r), PsxColorMultiplier(g), PsxColorMultiplier(b), alpha,
            blendMode);
        
        AddSprite(sprite);
    }

    private static float PsxColorMultiplier(byte color) => color / 128f;

    public class Sprite
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int Depth;
        public float Alpha;
        public Bitmap Bitmap;
        public float R;
        public float G;
        public float B;
        public BlendMode BlendMode;
        
        // Pour les quads déformés (rotation)
        public bool IsDeformed;
        public int X0, Y0, X1, Y1, X2, Y2, X3, Y3;
        public float U0, V0, U1, V1, U2, V2, U3, V3;

        public Sprite(int x, int y, int width, int height, int depth, Bitmap bitmap,
            float alpha = 1.0f, float r = 1.0f, float g = 1.0f, float b = 1.0f, BlendMode blendMode = BlendMode.None)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Depth = depth;
            Bitmap = bitmap;
            Alpha = Math.Clamp(alpha, 0.0f, 1.0f);
            R = Math.Clamp(r, 0.0f, 1.0f);
            G = Math.Clamp(g, 0.0f, 1.0f);
            B = Math.Clamp(b, 0.0f, 1.0f);
            BlendMode = blendMode;
            IsDeformed = false;
        }
        
        // Constructor pour quad déformé (rotation)
        public Sprite(
            Bitmap bitmap,
            int x0, int y0, float u0, float v0,
            int x1, int y1, float u1, float v1,
            int x2, int y2, float u2, float v2,
            int x3, int y3, float u3, float v3,
            int depth,
            float r = 1.0f, float g = 1.0f, float b = 1.0f, float alpha = 1.0f,
            BlendMode blendMode = BlendMode.None)
        {
            Bitmap = bitmap;
            Depth = depth;
            R = Math.Clamp(r, 0.0f, 255f / 128f);
            G = Math.Clamp(g, 0.0f, 255f / 128f);
            B = Math.Clamp(b, 0.0f, 255f / 128f);
            Alpha = Math.Clamp(alpha, 0.0f, 1.0f);
            IsDeformed = true;
            X0 = x0; Y0 = y0; U0 = u0; V0 = v0;
            X1 = x1; Y1 = y1; U1 = u1; V1 = v1;
            X2 = x2; Y2 = y2; U2 = u2; V2 = v2;
            X3 = x3; Y3 = y3; U3 = u3; V3 = v3;
            BlendMode = blendMode;
        }
    }

    private readonly record struct QuadColorKey(
        int Width, int Height,
        byte R0, byte G0, byte B0,
        byte R1, byte G1, byte B1,
        byte R2, byte G2, byte B2,
        byte R3, byte G3, byte B3);

    private readonly record struct RectangleColorKey(
        int Width, int Height,
        byte R, byte G, byte B);

    private readonly record struct CrossColorKey(
        int Size,
        byte R, byte G, byte B);

    private readonly record struct LineColorKey(
        int Width, int Height,
        int X1, int Y1, int X2, int Y2,
        byte R, byte G, byte B);

    private readonly record struct TextColorKey(
        string Text,
        string FontFamily,
        float FontSize,
        int FontStyle,
        byte R, byte G, byte B);

    private static unsafe Bitmap CreateGradientBitmap(int width, int height, POLY_G4 polyG4)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb);

        byte* ptr = (byte*)bitmapData.Scan0;
        int stride = bitmapData.Stride;

        for (int py = 0; py < height; py++)
        {
            // Facteur d'interpolation verticale (fixed-point 8.8)
            int ty = height > 1 ? (py << 8) / (height - 1) : 0;
            int invTy = 256 - ty;

            byte* row = ptr + py * stride;

            for (int px = 0; px < width; px++)
            {
                // Facteur d'interpolation horizontale (fixed-point 8.8)
                int tx = width > 1 ? (px << 8) / (width - 1) : 0;
                int invTx = 256 - tx;

                // Interpolation bilinéaire en fixed-point
                int r = ((polyG4.r0 * invTx + polyG4.r1 * tx) * invTy +
                         (polyG4.r2 * invTx + polyG4.r3 * tx) * ty) >> 16;
                int g = ((polyG4.g0 * invTx + polyG4.g1 * tx) * invTy +
                         (polyG4.g2 * invTx + polyG4.g3 * tx) * ty) >> 16;
                int b = ((polyG4.b0 * invTx + polyG4.b1 * tx) * invTy +
                         (polyG4.b2 * invTx + polyG4.b3 * tx) * ty) >> 16;

                int offset = px * 4;
                row[offset] = (byte)b;     // Blue
                row[offset + 1] = (byte)g; // Green
                row[offset + 2] = (byte)r; // Red
                row[offset + 3] = 255;     // Alpha
            }
        }

        bitmap.UnlockBits(bitmapData);
        return bitmap;
    }

    public void DrawCross(int x, int y, int z, byte r, byte g, byte b)
    {
        var color = Color.FromArgb(r, g, b);
        DrawCross(x, y, z, color);
    }

    public void DrawCross(int x, int y, int z, Color color)
    {
        var cacheKey = new CrossColorKey(_crossSize, color.R, color.G, color.B);

        if (!_crossCache.TryGetValue(cacheKey, out var bmp))
        {
            if (_crossCache.Count >= MaxCacheSize)
            {
                ClearCrossCache();
            }

            bmp = new Bitmap(_crossSize * 2 + 1, _crossSize * 2 + 1);
            using (var gBmp = System.Drawing.Graphics.FromImage(bmp))
            {
                using var pen = new Pen(color);
                gBmp.DrawLine(pen, _crossSize, 0, _crossSize, _crossSize * 2);
                gBmp.DrawLine(pen, 0, _crossSize, _crossSize * 2, _crossSize);
            }

            _crossCache[cacheKey] = bmp;
        }

        AddSprite(x - _crossSize, y - _crossSize, bmp.Width, bmp.Height, z, bmp);
    }

    public void DrawLine(int x1, int y1, int x2, int y2, Color color)
    {
        int minX = Math.Min(x1, x2);
        int minY = Math.Min(y1, y2);
        int maxX = Math.Max(x1, x2);
        int maxY = Math.Max(y1, y2);

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        if (width <= 0 || height <= 0) return;

        int localX1 = x1 - minX;
        int localY1 = y1 - minY;
        int localX2 = x2 - minX;
        int localY2 = y2 - minY;

        var cacheKey = new LineColorKey(width, height, localX1, localY1, localX2, localY2, color.R, color.G, color.B);

        if (!_lineCache.TryGetValue(cacheKey, out var bmp))
        {
            if (_lineCache.Count >= MaxCacheSize)
            {
                ClearLineCache();
            }

            bmp = new Bitmap(width, height);
            using (var gBmp = System.Drawing.Graphics.FromImage(bmp))
            {
                gBmp.Clear(Color.Transparent);
                using var pen = new Pen(color);
                gBmp.DrawLine(pen, localX1, localY1, localX2, localY2);
            }

            _lineCache[cacheKey] = bmp;
        }

        AddSprite(minX, minY, width, height, SpriteDepth.DebugCollision, bmp);
    }

    public void DrawCenterString(string text, Font font, Color color, int x, int y, int z)
    {
        var textSize = graphics.MeasureString(text, font);
        DrawString(text, font, color, (int)(x - textSize.Width / 2), (int)(y - textSize.Height / 2), z);
    }

    public void DrawString(string text, Font font, Color color, int x, int y, int z)
    {
        if (string.IsNullOrEmpty(text)) return;

        var cacheKey = new TextColorKey(
            text,
            font.FontFamily.Name,
            font.Size,
            (int)font.Style,
            color.R,
            color.G,
            color.B);

        if (!_textCache.TryGetValue(cacheKey, out var bmp))
        {
            if (_textCache.Count >= MaxCacheSize)
            {
                ClearTextCache();
            }

            using var tempBitmap = new Bitmap(1, 1);
            using var tempGraphics = System.Drawing.Graphics.FromImage(tempBitmap);
            var textSize = tempGraphics.MeasureString(text, font);

            int width = (int)Math.Ceiling(textSize.Width);
            int height = (int)Math.Ceiling(textSize.Height);

            if (width <= 0 || height <= 0) return;

            bmp = new Bitmap(width, height);
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                using var brush = new SolidBrush(color);
                g.DrawString(text, font, brush, 0, 0);
            }

            _textCache[cacheKey] = bmp;
        }

        AddSprite(x, y, bmp.Width, bmp.Height, z, bmp);
    }

    public void DrawColoredRectangle(short tileX0, short tileY0, short tileW, short tileH, int fadeTransitionEffect, float tileR0,
        float f, float f1, float f2)
    {
        AddSprite(tileX0, tileY0, tileW, tileH, fadeTransitionEffect, WhiteBitmap, tileR0, f, f1, f2);
    }
}

