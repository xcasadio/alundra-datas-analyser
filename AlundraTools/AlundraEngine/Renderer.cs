using System.Drawing.Imaging;
using AlundraEngine.Graphics;

namespace AlundraEngine;

public class SpriteDepth
{
    public const int DebugCollision = int.MaxValue - 5;

    public const int BackgroundUI = int.MaxValue - 4;
    public const int ForegroundUI = int.MaxValue - 3;
    public const int ForegroundUICursor = int.MaxValue - 2;
    public const int ForegroundUICursor2 = int.MaxValue - 1;
    public const int ForegroundEffect = int.MaxValue;
}

public class Renderer(GameEngine gameEngine)
{
    private readonly GameEngine _gameEngine = gameEngine;
    private readonly SortedDictionary<int, List<Sprite>> _sprites = new();
    public readonly Bitmap WhiteBitmap = CreateWhiteBitmap();

    // Cache pour les quads colorés
    private readonly Dictionary<QuadColorKey, Bitmap> _quadColorCache = new();
    // Cache pour les rectangles
    private readonly Dictionary<RectangleColorKey, Bitmap> _rectangleCache = new();
    private const int MaxCacheSize = 256;

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

    public void AddSprite(int x, int y, int width, int height, int depthSortValue, Bitmap bitmap, float alpha = 1.0f, float r = 1.0f, float g = 1.0f, float b = 1.0f)
    {
        var sprite = new Sprite(x, y, width, height, depthSortValue, bitmap, alpha, r, g, b);
        AddSprite(sprite);
    }

    public void AddSprite(Sprite sprite)
    {
        if (!_sprites.ContainsKey(sprite.Depth))
        {
            _sprites[sprite.Depth] = [];
        }

        _sprites[sprite.Depth].Add(sprite);
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

    public void Render(System.Drawing.Graphics graphics)
    {
        foreach (var kvp in _sprites)
        {
            foreach (var sprite in kvp.Value)
            {
                RenderSprite(graphics, sprite);
            }
        }
    }

    private void RenderSprite(System.Drawing.Graphics graphics, Sprite sprite)
    {
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

            var destRect = new Rectangle(sprite.X, sprite.Y, sprite.Width, sprite.Height);
            graphics.DrawImage(sprite.Bitmap, destRect, 0, 0, sprite.Bitmap.Width, sprite.Bitmap.Height, GraphicsUnit.Pixel, imageAttributes);
        }
        else
        {
            graphics.DrawImage(sprite.Bitmap, sprite.X, sprite.Y, sprite.Width, sprite.Height);
        }
    }

    public void Clear()
    {
        _sprites.Clear();
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

        public Sprite(int x, int y, int width, int height, int depth, Bitmap bitmap,
            float alpha = 1.0f, float r = 1.0f, float g = 1.0f, float b = 1.0f)
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
}

