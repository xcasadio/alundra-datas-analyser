namespace AlundraEngine.DatasBin;

public class MapCopy
{
    public byte FromX;
    public byte FromY;
    public byte Width;
    public byte Height;
    public byte ToX;
    public byte ToY;

    public MapCopy(BinaryReader br)
    {
        FromX = br.ReadByte();
        FromY = br.ReadByte();
        Width = br.ReadByte();
        Height = br.ReadByte();
        ToX = br.ReadByte();
        ToY = br.ReadByte();
    }
};