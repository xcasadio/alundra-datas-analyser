namespace AlundraDataExtractor;

record SpriteSheetTile
{
    public int U0;
    public int V0;
    public int Width;
    public int Height;
    public int PaletteIndex;

    public SpriteSheetTile(int u0, int v0, int width, int height, int paletteIndex)
    {
        U0 = u0;
        V0 = v0;
        Width = width;
        Height = height;
        PaletteIndex = paletteIndex;
    }
}