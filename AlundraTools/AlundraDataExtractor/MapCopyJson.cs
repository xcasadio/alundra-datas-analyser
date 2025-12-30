using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record MapCopyJson
{
    public int FromX { get; set; }
    public int FromY { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int ToX { get; set; }
    public int ToY { get; set; }

    public MapCopyJson(MapCopy mapCopy)
    {
        FromX = mapCopy.FromX;
        FromY = mapCopy.FromY;
        Width = mapCopy.Width;
        Height = mapCopy.Height;
        ToX = mapCopy.ToX;
        ToY = mapCopy.ToY;
    }
}