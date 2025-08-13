namespace AlundraEngine.Graphics;

public class SPRT
{
    public ulong tag;
    public byte r0;
    public byte g0;
    public byte b0;
    public byte code;
    public short x0;
    public short y0;
    public byte u0;
    public byte v0;
    public ushort clut;
    public short w;
    public short h;

    public override string ToString()
    {
        return $"x0:{x0} y0:{y0} u0:{u0} v0:{v0} w:{w} h:{h} c:{clut} r0:{r0} g0:{g0} b0:{b0}";
    }
}