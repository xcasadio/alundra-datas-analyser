namespace AlundraEngine.DatasBin;

public class MapCopy
{
    public sbyte FromX;
    public sbyte FromY;
    public byte Width;
    public byte Height;
    public byte ToX;
    public byte ToY;

    public MapCopy(BinaryReader br)
    {
        FromX = br.ReadSByte();
        FromY = br.ReadSByte();
        Width = br.ReadByte();
        Height = br.ReadByte();
        ToX = br.ReadByte();
        ToY = br.ReadByte();
    }

    public override string ToString()
    {
        return $"x:{FromX} y:{FromY} w:{Width} h:{Height} ->x:{ToX} ->y:{ToY}";
    }
};