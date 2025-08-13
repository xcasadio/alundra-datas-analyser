using System.Drawing.Imaging;
using AlundraEngine.Gameplay;
using AlundraEngine.Graphics;

namespace AlundraEngine;

public class Renderer(GameEngine gameEngine)
{
    private readonly GameEngine _gameEngine = gameEngine;
    private readonly SortedDictionary<int, List<Sprite>> _sprites = new();

    public void AddSprite(SPRT sprt, int depthSortValue, Bitmap bitmap, float alpha = 1.0f)
    {
        AddSprite(sprt.x0, sprt.y0, sprt.w, sprt.h, depthSortValue, bitmap, alpha);
    }

    public void AddSprite(int x, int y, int width, int height, int depthSortValue, Bitmap bitmap, float alpha = 1.0f)
    {
        var sprite = new Sprite(x, y, width, height, depthSortValue, bitmap, alpha);
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
        if (Math.Abs(sprite.Alpha - 1.0f) > 0.001f)
        {
            var colorMatrix = new ColorMatrix
            {
                Matrix33 = sprite.Alpha
            };

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

        public Sprite(int x, int y, int width, int height, int depthSortValue, Bitmap bitmap, float alpha = 1.0f)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            DepthSortValue = depthSortValue;
            Alpha = Math.Clamp(alpha, 0.0f, 1.0f);
            Bitmap = bitmap;
        }
    }
}

