namespace AlundraEngine.UI;

public class UiBoxAnimated
{
    public short X;
    public short Y;
    public short Width;//in 8s
    public short Height;// in 8s
    public UiDrawCmd[][] Boxcommands = new UiDrawCmd[0xa][];//drawareaid is an index into this
}