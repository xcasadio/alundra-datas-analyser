using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using AlundraEngine;
using AlundraEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = System.Drawing.Color;

namespace AlundraGame;

public class AlundraRenderer : IRenderer
{
    private readonly SpriteBatch _spriteBatch;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SortedDictionary<int, List<Sprite>> _sprites = new();
    private readonly Texture2D _whiteTexture;
    private readonly BasicEffect _basicEffect;
    private readonly VertexPositionColorTexture[] _quadVertices = new VertexPositionColorTexture[6];
    
    // BlendStates PSX
    private readonly BlendState _blendStateAverage;
    private readonly BlendState _blendStateAdditive;
    private readonly BlendState _blendStateSubtractive;
    private readonly BlendState _blendStateAdditiveDim;
    
    private readonly Dictionary<Bitmap, Texture2D> _textureCache = new();
    private readonly Dictionary<QuadColorKey, Texture2D> _quadColorCache = new();
    private readonly Dictionary<RectangleColorKey, Texture2D> _rectangleCache = new();
    private readonly Dictionary<CrossColorKey, Texture2D> _crossCache = new();
    private readonly Dictionary<LineColorKey, Texture2D> _lineCache = new();
    private readonly Dictionary<TextColorKey, Texture2D> _textCache = new();
    
    private const int MaxCacheSize = 10000;
    private int _crossSize = 5;

    public AlundraRenderer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        _spriteBatch = spriteBatch;
        _graphicsDevice = graphicsDevice;
        _whiteTexture = CreateWhiteTexture();
        
        // Initialiser BasicEffect pour le rendu de quads déformés (rotations)
        _basicEffect = new BasicEffect(graphicsDevice)
        {
            TextureEnabled = true,
            VertexColorEnabled = true,
            World = Matrix.Identity,
            View = Matrix.Identity,
            Projection = Matrix.CreateOrthographicOffCenter(
                0, graphicsDevice.Viewport.Width,
                graphicsDevice.Viewport.Height, 0,
                0, 1)
        };
        
        // Créer les BlendStates PSX personnalisés
        // Average: alpha is adjusted at draw time to preserve transparent texels.
        _blendStateAverage = BlendState.NonPremultiplied;
        
        // Additive: Back + Front
        _blendStateAdditive = new BlendState
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.One,
            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.One
        };
        
        // Subtractive: Back - Front
        _blendStateSubtractive = new BlendState
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.ReverseSubtract,
            AlphaSourceBlend = Blend.SourceAlpha,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add
        };
        
        // AdditiveDim: alpha is adjusted at draw time to preserve transparent texels.
        _blendStateAdditiveDim = _blendStateAdditive;
    }

    private Texture2D CreateWhiteTexture()
    {
        var texture = new Texture2D(_graphicsDevice, 1, 1);
        texture.SetData([Microsoft.Xna.Framework.Color.White]);
        return texture;
    }

    public void Render()
    {
        BlendState currentBlendState = BlendState.AlphaBlend;
        _spriteBatch.Begin(SpriteSortMode.Deferred, currentBlendState, SamplerState.PointClamp);
        
        foreach (var kvp in _sprites)
        {
            foreach (var sprite in kvp.Value)
            {
                // Changer de BlendState si nécessaire
                var requiredBlendState = GetBlendState(sprite.BlendMode);
                if (requiredBlendState != currentBlendState || sprite.IsDeformed)
                {
                    _spriteBatch.End();

                    currentBlendState = requiredBlendState;
                    if (sprite.IsDeformed)
                    {
                        _graphicsDevice.BlendState = currentBlendState;
                        _graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                        _graphicsDevice.RasterizerState = RasterizerState.CullNone;
                        RenderDeformedSprite(sprite);
                    }

                    _spriteBatch.Begin(SpriteSortMode.Deferred, currentBlendState, SamplerState.PointClamp);
                    
                    if (!sprite.IsDeformed)
                    {
                        RenderSprite(sprite);
                    }
                }
                else
                {
                    RenderSprite(sprite);
                }
            }
        }
        
        _spriteBatch.End();
    }

    private void RenderSprite(Sprite sprite)
    {
        Texture2D? texture = sprite.Texture;
        
        if (texture == null && sprite.Bitmap != null)
        {
            texture = GetOrCreateTexture(sprite.Bitmap);
        }
        
        if (texture == null) return;

        // Le BlendState est déjà configuré dans Render(), on applique juste la couleur
        var alpha = sprite.BlendMode switch
        {
            BlendMode.Average => sprite.Alpha * 0.5f,
            BlendMode.AdditiveDim => sprite.Alpha * 0.25f,
            _ => sprite.Alpha,
        };

        var color = new Microsoft.Xna.Framework.Color(
            sprite.R,
            sprite.G,
            sprite.B,
            alpha);

        var x = sprite.X;
        var y = sprite.Y;
        var width = sprite.Width;
        var height = sprite.Height;
        var effects = SpriteEffects.None;

        if (width < 0)
        {
            x += width;
            width = -width;
            effects |= SpriteEffects.FlipHorizontally;
        }

        if (height < 0)
        {
            y += height;
            height = -height;
            effects |= SpriteEffects.FlipVertically;
        }

        if (width == 0 || height == 0)
        {
            return;
        }

        var destRect = new Microsoft.Xna.Framework.Rectangle(
            x,
            y,
            width,
            height);

        _spriteBatch.Draw(texture, destRect, null, color, 0f, Vector2.Zero, effects, 0f);
    }
    
    private BlendState GetBlendState(BlendMode blendMode)
    {
        return blendMode switch
        {
            BlendMode.Average => _blendStateAverage,
            BlendMode.Additive => _blendStateAdditive,
            BlendMode.Subtractive => _blendStateSubtractive,
            BlendMode.AdditiveDim => _blendStateAdditiveDim,
            _ => BlendState.AlphaBlend
        };
    }

    private Texture2D? GetOrCreateTexture(Bitmap bitmap)
    {
        if (_textureCache.TryGetValue(bitmap, out var texture))
        {
            return texture;
        }

        if (_textureCache.Count >= MaxCacheSize)
        {
            ClearTextureCache();
        }

        texture = BitmapToTexture2D(bitmap);
        if (texture != null)
        {
            _textureCache[bitmap] = texture;
        }
        
        return texture;
    }

    /// <summary>
    /// Re-uploads a cached bitmap whose pixels were modified in place.
    ///
    /// JUSTIFICATION: backend MonoGame only
    /// RELATION: <see cref="_textureCache"/> is keyed on the Bitmap instance, which assumes a
    /// bitmap's contents never change once uploaded. That holds for every static resource, but not
    /// for a movie frame buffer, which is one long-lived Bitmap rewritten 30 times a second. Rather
    /// than allocating a Bitmap per frame (which would push a new Texture2D into the cache every
    /// frame), the owner tells the renderer the pixels moved and the existing texture is refreshed
    /// in place.
    /// </summary>
    public void InvalidateTexture(Bitmap? bitmap)
    {
        if (bitmap is null || !_textureCache.TryGetValue(bitmap, out var texture))
        {
            // Never uploaded yet: the next RenderSprite will build it from the current pixels.
            return;
        }

        if (texture.Width != bitmap.Width || texture.Height != bitmap.Height)
        {
            _textureCache.Remove(bitmap);
            texture.Dispose();
            return;
        }

        UploadBitmapInto(texture, bitmap);
    }

    private Texture2D? BitmapToTexture2D(Bitmap bitmap)
    {
        if (bitmap.Width <= 0 || bitmap.Height <= 0) return null;

        var texture = new Texture2D(_graphicsDevice, bitmap.Width, bitmap.Height);
        UploadBitmapInto(texture, bitmap);
        return texture;
    }

    private static void UploadBitmapInto(Texture2D texture, Bitmap bitmap)
    {
        var bitmapData = bitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

        try
        {
            var pixels = new Microsoft.Xna.Framework.Color[bitmap.Width * bitmap.Height];
            unsafe
            {
                byte* scan0 = (byte*)bitmapData.Scan0;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    byte* row = scan0 + y * bitmapData.Stride;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        int offset = x * 4;
                        var alpha = row[offset + 3];
                        pixels[y * bitmap.Width + x] = alpha == 0
                            ? Microsoft.Xna.Framework.Color.Transparent
                            : new Microsoft.Xna.Framework.Color(
                                row[offset + 2],
                                row[offset + 1],
                                row[offset],
                                alpha);
                    }
                }
            }
            texture.SetData(pixels);
        }
        finally
        {
            bitmap.UnlockBits(bitmapData);
        }
    }

    public void Clear()
    {
        _sprites.Clear();
    }

    // JUSTIFICATION: backend MonoGame only
    public Bitmap? CaptureFrameBuffer()
    {
        var renderTargets = _graphicsDevice.GetRenderTargets();
        if (renderTargets.Length == 0)
        {
            return null;
        }

        var renderTarget = renderTargets[0].RenderTarget as RenderTarget2D;
        if (renderTarget == null)
        {
            return null;
        }

        _graphicsDevice.SetRenderTarget(null);

        try
        {
            return CreateBitmapFromTexture(renderTarget);
        }
        finally
        {
            _graphicsDevice.SetRenderTargets(renderTargets);
        }
    }

    private void ClearTextureCache()
    {
        foreach (var texture in _textureCache.Values)
        {
            texture?.Dispose();
        }
        _textureCache.Clear();
    }

    private void ClearQuadCache()
    {
        foreach (var texture in _quadColorCache.Values)
        {
            texture?.Dispose();
        }
        _quadColorCache.Clear();
    }

    private void ClearRectangleCache()
    {
        foreach (var texture in _rectangleCache.Values)
        {
            texture?.Dispose();
        }
        _rectangleCache.Clear();
    }

    private void ClearCrossCache()
    {
        foreach (var texture in _crossCache.Values)
        {
            texture?.Dispose();
        }
        _crossCache.Clear();
    }

    private void ClearLineCache()
    {
        foreach (var texture in _lineCache.Values)
        {
            texture?.Dispose();
        }
        _lineCache.Clear();
    }

    private void ClearTextCache()
    {
        foreach (var texture in _textCache.Values)
        {
            texture?.Dispose();
        }
        _textCache.Clear();
    }

    public void AddSprite(SPRT sprt, int depthSortValue, Bitmap bitmap, float alpha = 1, float r = 1, float g = 1, float b = 1)
    {
        AddSprite(sprt.x0, sprt.y0, sprt.w, sprt.h, depthSortValue, bitmap, alpha, r, g, b);
    }

    public void AddSprite(int x, int y, int width, int height, int depthSortValue, Bitmap bitmap, float alpha = 1, float r = 1,
        float g = 1, float b = 1, BlendMode blendMode = BlendMode.None)
    {
        var sprite = new Sprite(x, y, width, height, depthSortValue, bitmap, alpha, r, g, b, blendMode);
        AddSpriteInternal(sprite);
    }

    public void AddSprite(Renderer.Sprite sprite)
    {
        if (!_sprites.ContainsKey(sprite.Depth))
        {
            _sprites[sprite.Depth] = [];
        }

        _sprites[sprite.Depth].Add(new Sprite(
            sprite.X, sprite.Y, sprite.Width, sprite.Height,
            sprite.Depth, sprite.Bitmap, sprite.Alpha,
            sprite.R, sprite.G, sprite.B, sprite.BlendMode));
    }

    private void AddSpriteInternal(Sprite sprite)
    {
        if (!_sprites.ContainsKey(sprite.Depth))
        {
            _sprites[sprite.Depth] = [];
        }

        _sprites[sprite.Depth].Add(sprite);
    }

    public void AddRectangle(TILE tile, int depthSortValue, float alpha = 1)
    {
        if (tile.w <= 0 || tile.h <= 0) return;

        var cacheKey = new RectangleColorKey(tile.w, tile.h, tile.r0, tile.g0, tile.b0);

        if (!_rectangleCache.TryGetValue(cacheKey, out var texture))
        {
            if (_rectangleCache.Count >= MaxCacheSize)
            {
                ClearRectangleCache();
            }

            texture = CreateSolidColorTexture(tile.w, tile.h, tile.r0, tile.g0, tile.b0);
            _rectangleCache[cacheKey] = texture;
        }

        AddSpriteFromTexture(tile.x0, tile.y0, tile.w, tile.h, depthSortValue, texture, alpha);
    }

    private Texture2D CreateSolidColorTexture(int width, int height, byte r, byte g, byte b)
    {
        var texture = new Texture2D(_graphicsDevice, width, height);
        var color = new Microsoft.Xna.Framework.Color(r, g, b);
        var pixels = new Microsoft.Xna.Framework.Color[width * height];
        Array.Fill(pixels, color);
        texture.SetData(pixels);
        return texture;
    }

    // JUSTIFICATION: backend MonoGame only
    private static Bitmap CreateBitmapFromTexture(Texture2D texture)
    {
        var pixels = new Microsoft.Xna.Framework.Color[texture.Width * texture.Height];
        texture.GetData(pixels);

        var bitmap = new Bitmap(texture.Width, texture.Height, PixelFormat.Format32bppArgb);
        var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        try
        {
            unsafe
            {
                byte* scan0 = (byte*)bitmapData.Scan0;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    byte* row = scan0 + y * bitmapData.Stride;
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        var color = pixels[y * bitmap.Width + x];
                        var offset = x * 4;
                        row[offset] = color.B;
                        row[offset + 1] = color.G;
                        row[offset + 2] = color.R;
                        row[offset + 3] = color.A;
                    }
                }
            }
        }
        finally
        {
            bitmap.UnlockBits(bitmapData);
        }

        return bitmap;
    }

    public void AddQuadColor(POLY_G4 polyG4, int depthSortValue, float alpha = 1, float r = 1, float g = 1, float b = 1)
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

        if (!_quadColorCache.TryGetValue(cacheKey, out var texture))
        {
            if (_quadColorCache.Count >= MaxCacheSize)
            {
                ClearQuadCache();
            }

            texture = CreateGradientTexture(width, height, polyG4);
            _quadColorCache[cacheKey] = texture;
        }

        AddSpriteFromTexture(minX, minY, width, height, depthSortValue, texture, alpha, r, g, b);
    }

    private Texture2D CreateGradientTexture(int width, int height, POLY_G4 polyG4)
    {
        var texture = new Texture2D(_graphicsDevice, width, height);
        var pixels = new Microsoft.Xna.Framework.Color[width * height];

        for (int py = 0; py < height; py++)
        {
            int ty = height > 1 ? (py << 8) / (height - 1) : 0;
            int invTy = 256 - ty;

            for (int px = 0; px < width; px++)
            {
                int tx = width > 1 ? (px << 8) / (width - 1) : 0;
                int invTx = 256 - tx;

                int r = ((polyG4.r0 * invTx + polyG4.r1 * tx) * invTy +
                         (polyG4.r2 * invTx + polyG4.r3 * tx) * ty) >> 16;
                int g = ((polyG4.g0 * invTx + polyG4.g1 * tx) * invTy +
                         (polyG4.g2 * invTx + polyG4.g3 * tx) * ty) >> 16;
                int b = ((polyG4.b0 * invTx + polyG4.b1 * tx) * invTy +
                         (polyG4.b2 * invTx + polyG4.b3 * tx) * ty) >> 16;

                pixels[py * width + px] = new Microsoft.Xna.Framework.Color(
                    (byte)r, (byte)g, (byte)b, (byte)255);
            }
        }

        texture.SetData(pixels);
        return texture;
    }

    private void AddSpriteFromTexture(int x, int y, int width, int height, int depth, Texture2D texture,
        float alpha = 1.0f, float r = 1.0f, float g = 1.0f, float b = 1.0f, BlendMode blendMode = BlendMode.None)
    {
        var sprite = new Sprite(x, y, width, height, depth, texture, alpha, r, g, b, blendMode);
        AddSpriteInternal(sprite);
    }

    public void DrawCross(int x, int y, int z, byte r, byte g, byte b)
    {
        var color = Color.FromArgb(r, g, b);
        DrawCross(x, y, z, color);
    }

    public void DrawCross(int x, int y, int z, Color color)
    {
        var cacheKey = new CrossColorKey(_crossSize, color.R, color.G, color.B);

        if (!_crossCache.TryGetValue(cacheKey, out var texture))
        {
            if (_crossCache.Count >= MaxCacheSize)
            {
                ClearCrossCache();
            }

            var bmp = new Bitmap(_crossSize * 2 + 1, _crossSize * 2 + 1);
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using var pen = new Pen(color);
                g.DrawLine(pen, _crossSize, 0, _crossSize, _crossSize * 2);
                g.DrawLine(pen, 0, _crossSize, _crossSize * 2, _crossSize);
            }

            texture = BitmapToTexture2D(bmp);
            bmp.Dispose();
            
            if (texture != null)
            {
                _crossCache[cacheKey] = texture;
            }
        }

        if (texture != null)
        {
            AddSpriteFromTexture(x - _crossSize, y - _crossSize, texture.Width, texture.Height, z, texture);
        }
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

        if (!_lineCache.TryGetValue(cacheKey, out var texture))
        {
            if (_lineCache.Count >= MaxCacheSize)
            {
                ClearLineCache();
            }

            var bmp = new Bitmap(width, height);
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                using var pen = new Pen(color);
                g.DrawLine(pen, localX1, localY1, localX2, localY2);
            }

            texture = BitmapToTexture2D(bmp);
            bmp.Dispose();
            
            if (texture != null)
            {
                _lineCache[cacheKey] = texture;
            }
        }

        if (texture != null)
        {
            AddSpriteFromTexture(minX, minY, width, height, SpriteDepth.DebugCollision, texture);
        }
    }

    public void DrawCenterString(string text, Font font, Color color, int x, int y, int z)
    {
        if (string.IsNullOrEmpty(text)) return;

        using var tempBitmap = new Bitmap(1, 1);
        using var tempGraphics = System.Drawing.Graphics.FromImage(tempBitmap);
        var textSize = tempGraphics.MeasureString(text, font);
        
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

        if (!_textCache.TryGetValue(cacheKey, out var texture))
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

            var bmp = new Bitmap(width, height);
            using (var g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                using var brush = new SolidBrush(color);
                g.DrawString(text, font, brush, 0, 0);
            }

            texture = BitmapToTexture2D(bmp);
            bmp.Dispose();
            
            if (texture != null)
            {
                _textCache[cacheKey] = texture;
            }
        }

        if (texture != null)
        {
            AddSpriteFromTexture(x, y, texture.Width, texture.Height, z, texture);
        }
    }

    public void DrawColoredRectangle(short tileX0, short tileY0, short tileW, short tileH, int fadeTransitionEffect, float alpha,
        float r, float g, float b, BlendMode blendMode = BlendMode.None)
    {
        AddSpriteFromTexture(tileX0, tileY0, tileW, tileH, fadeTransitionEffect, _whiteTexture, alpha, r, g, b, blendMode);
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
        var texture = GetOrCreateTexture(bitmap);
        if (texture == null) return;
        
        var sprite = new Sprite(
            texture,
            x0, y0, u0, v0,
            x1, y1, u1, v1,
            x2, y2, u2, v2,
            x3, y3, u3, v3,
            depthSortValue,
            PsxColorMultiplier(r), PsxColorMultiplier(g), PsxColorMultiplier(b), alpha,
            blendMode);
        
        AddSpriteInternal(sprite);
    }

    private static float PsxColorMultiplier(byte color) => color / 128f;

    private void RenderDeformedSprite(Sprite sprite)
    {
        if (sprite.Texture == null) return;

        var viewport = _graphicsDevice.Viewport;
        _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(
            0, viewport.Width,
            viewport.Height, 0,
            0, 1);
        _graphicsDevice.DepthStencilState = DepthStencilState.None;
        
        var alpha = sprite.BlendMode switch
        {
            BlendMode.Average => sprite.Alpha * 0.5f,
            BlendMode.AdditiveDim => sprite.Alpha * 0.25f,
            _ => sprite.Alpha,
        };

        var color = new Microsoft.Xna.Framework.Color(sprite.R, sprite.G, sprite.B, alpha);
        
        // Triangle 1: 0-1-2
        _quadVertices[0] = new VertexPositionColorTexture(
            new Vector3(sprite.X0, sprite.Y0, 0), color, new Vector2(sprite.U0, sprite.V0));
        _quadVertices[1] = new VertexPositionColorTexture(
            new Vector3(sprite.X1, sprite.Y1, 0), color, new Vector2(sprite.U1, sprite.V1));
        _quadVertices[2] = new VertexPositionColorTexture(
            new Vector3(sprite.X2, sprite.Y2, 0), color, new Vector2(sprite.U2, sprite.V2));
        
        // Triangle 2: 2-1-3
        _quadVertices[3] = new VertexPositionColorTexture(
            new Vector3(sprite.X2, sprite.Y2, 0), color, new Vector2(sprite.U2, sprite.V2));
        _quadVertices[4] = new VertexPositionColorTexture(
            new Vector3(sprite.X1, sprite.Y1, 0), color, new Vector2(sprite.U1, sprite.V1));
        _quadVertices[5] = new VertexPositionColorTexture(
            new Vector3(sprite.X3, sprite.Y3, 0), color, new Vector2(sprite.U3, sprite.V3));
        
        _basicEffect.Texture = sprite.Texture;
        
        foreach (var pass in _basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphicsDevice.DrawUserPrimitives(
                PrimitiveType.TriangleList,
                _quadVertices,
                0,
                2);  // 2 triangles
        }
    }

    private class Sprite
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int Depth;
        public float Alpha;
        public Bitmap? Bitmap;
        public Texture2D? Texture;
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
        }

        public Sprite(int x, int y, int width, int height, int depth, Texture2D texture,
            float alpha = 1.0f, float r = 1.0f, float g = 1.0f, float b = 1.0f, BlendMode blendMode = BlendMode.None)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Depth = depth;
            Texture = texture;
            Alpha = Math.Clamp(alpha, 0.0f, 1.0f);
            R = Math.Clamp(r, 0.0f, 1.0f);
            G = Math.Clamp(g, 0.0f, 1.0f);
            B = Math.Clamp(b, 0.0f, 1.0f);
            BlendMode = blendMode;
            IsDeformed = false;
        }
        
        // Constructor pour quad déformé (rotation)
        public Sprite(
            Texture2D texture,
            int x0, int y0, float u0, float v0,
            int x1, int y1, float u1, float v1,
            int x2, int y2, float u2, float v2,
            int x3, int y3, float u3, float v3,
            int depth,
            float r = 1.0f, float g = 1.0f, float b = 1.0f, float alpha = 1.0f,
            BlendMode blendMode = BlendMode.None)
        {
            Texture = texture;
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
}