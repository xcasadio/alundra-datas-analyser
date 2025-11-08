using System.Drawing.Imaging;
using AlundraEngine.Gameplay;
using AlundraEngine.Graphics;

namespace AlundraEngine;

public class SpriteDepth
{
    public const int BackgroundUI = int.MaxValue - 2;
    public const int ForegroundUI = int.MaxValue - 1;
    public const int ForegroundEffect = int.MaxValue;
}

public class Renderer(GameEngine gameEngine)
{
    private readonly GameEngine _gameEngine = gameEngine;
    private readonly SortedDictionary<int, List<Sprite>> _sprites = new();
    public readonly Bitmap WhiteBitmap = CreateWhiteBitmap();

    private static Bitmap CreateWhiteBitmap()
    {
        var bmp = new Bitmap(1, 1);
        bmp.SetPixel(0, 0, System.Drawing.Color.White);
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
        if (!_sprites.ContainsKey(sprite.DepthSortValue))
        {
            _sprites[sprite.DepthSortValue] = [];
        }

        _sprites[sprite.DepthSortValue].Add(sprite);
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

    public class Sprite
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int DepthSortValue;
        public float Alpha;
        public Bitmap Bitmap;
        public float R;
        public float G;
        public float B;

        public Sprite(int x, int y, int width, int height, int depthSortValue, Bitmap bitmap,
            float alpha = 1.0f, float r = 1.0f, float g = 1.0f, float b = 1.0f)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            DepthSortValue = depthSortValue;
            Bitmap = bitmap;
            Alpha = Math.Clamp(alpha, 0.0f, 1.0f);
            R = Math.Clamp(r, 0.0f, 1.0f);
            G = Math.Clamp(g, 0.0f, 1.0f);
            B = Math.Clamp(b, 0.0f, 1.0f);
        }
    }
}

