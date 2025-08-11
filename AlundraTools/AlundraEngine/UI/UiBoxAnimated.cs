using AlundraEngine.Gameplay;
using AlundraEngine.Graphics;

namespace AlundraEngine.UI;

public class UiBoxAnimated
{
    public short X;
    public short Y;
    public short Width;//in 8s
    public short Height;// in 8s
    public UiDrawCmd[][] Boxcommands = new UiDrawCmd[0xa][];//drawareaid is an index into this
}

public class UIBoxConfiguration
{
    public short X;
    public short Y;
    public short Width;
    public short Height;
    public SPRT[] SpritesA;
    public SPRT[] SpritesB;
}

public class TextToDisplay
{
    public int tick;
    public int speed;
    public int mode;
    public short x;
    public short y;
    public short startX;
    public short startY;
    public byte _14;		
    public byte	_15;		
    public byte	_16;
    public byte _17;
    public short originX;
    public short originY;

    public override string ToString()
    {
        return $"{tick} {speed}  {mode}  {x}  {y}  {startX}  {startY}  {originX}  {originY}";
    }
}