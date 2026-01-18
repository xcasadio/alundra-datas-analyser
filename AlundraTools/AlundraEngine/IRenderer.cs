using AlundraEngine.Graphics;

namespace AlundraEngine;

public interface IRenderer
{
    void Render();
    void Clear();

    void AddSprite(SPRT sprt, int depthSortValue, Bitmap bitmap, float alpha = 1.0f, float r = 1.0f, float g = 1.0f, float b = 1.0f);
    void AddSprite(int x, int y, int width, int height, int depthSortValue, Bitmap bitmap, float alpha = 1.0f, float r = 1.0f, float g = 1.0f, float b = 1.0f, BlendMode blendMode = BlendMode.None);
    void AddSprite(Renderer.Sprite sprite);
    void AddRectangle(TILE tile, int depthSortValue, float alpha = 1.0f);
    void AddQuadColor(POLY_G4 polyG4, int depthSortValue, float alpha = 1.0f, float r = 1.0f, float g = 1.0f, float b = 1.0f);
    void DrawCross(int x, int y, int z, byte r, byte g, byte b);
    void DrawCross(int x, int y, int z, Color color);
    void DrawLine(int x1, int y1, int x2, int y2, Color color);
    void DrawCenterString(string text, Font font, Color color, int x, int y, int z);
    void DrawString(string text, Font font, Color color, int x, int y, int z);
    void DrawColoredRectangle(short tileX0, short tileY0, short tileW, short tileH, int fadeTransitionEffect, float tileR0, float f, float f1, float f2);
}