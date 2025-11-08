namespace AlundraEngine.Graphics;

public class TILE
{
    public ulong tag;
    public byte r0;
    public byte g0;
    public byte b0;
    public byte code;
    public short x0;
    public short y0;
    public short w;
    public short h;

    public override string ToString()
    {
        return $"x0:{x0} y0:{y0} w:{w} h:{h} r0:{r0} g0:{g0} b0:{b0} tag:{tag}";
    }
}