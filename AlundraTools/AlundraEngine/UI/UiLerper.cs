namespace AlundraEngine.UI;

public class UiLerper
{
    public int Currenttick;//0 tick progress, starts at 9
    public int Numticks;//4 number of ticks to iterate
    public int Tickstolinger;//8 ticks to linger once the lerp is finished, countsdown to zero then lerp function returns true (finished)
    public short X1;//c
    public short Y1;//e
    public short X2;//10
    public short Y2;//12

    public short AfterX, AfterY;
}