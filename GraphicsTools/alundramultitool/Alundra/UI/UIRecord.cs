namespace GraphicsTools.Alundra
{
    public class UiRecord
    {
        public int Status;//1 = active
        public UiBoxAnimated BoxAnimated;//04 ptr or 0
        public short X;//8
        public short Y;//a
        public short Width;//c //in 8s
        public short Height;//e //in 8s
        public UiFunction SetupFunc;//10 setup function
        public UiFunction RenderFunc;//14 render function
        public int UnknownVal;//18 0, -1,5
    }

    public class UiBoxAnimated
    {
        public short X;
        public short Y;
        public short Width;//in 8s
        public short Height;// in 8s
        public UiDrawCmd[][] Boxcommands = new UiDrawCmd[0xa][];//drawareaid is an index into this
    }

    //20 byte records
    public class UiDrawCmd
    {
        public short X, Y;
        public byte U, V;
        public short Uipaletteindex;//(clut address - 0x7812)/ 64
        public short Spritesheet;
        public short W, H;

        public long Signature { get
            {
                return Spritesheet | Uipaletteindex << 8 | U << 16 | V << 24 | W << 32 | H << 38;
            }
        }
    }

    //this structure lerps (linear interpolation) coordinates over a period of ticks
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


    public class FontCharInfo
    {
        public int Width;//width (kerning)
        public int Height;//height
        public int Sx;//source bitmap x
        public int Sy;//source bitmap y
        public int Y;//y offset from top
    }

    

    public delegate bool UiFunction(UiRecord ui);
    //there are several of these
    //0 is for the dialog box
    //1 is for the main ui elements
    //6 is for the item menu
    //c is for the dialog name box

    /*
0					//dialog box
9ebc4,	10,5,20,6,	491a4,47de4,0

1					//main ui elements
0,	0,0,40,4,	4c998,4d218,-1

2
9ebc4,	8,c,20,4,	491a4,50bcc,0

3
a6bf4,	10,8,20,4,	0,518c4,5

4
0,	10,8,20,4,	550d4,54bcc,-1

5
9ebc4,	10,c,20,4,	491a4,4ba10,0

6					//item menu
0,	10,8,20,4,	0,5695c,0

7
9ebc4,	10,8,20,4,	491a4,0,0

8
9ebc4,	10,c,20,4,	491a4,4c170,0

9
9ebc4,	10,c,20,4,	491a4,52584,0

a
0,	10,c,20,4,	0,5a1f8,0

b
9ebc4,	8,c,20,4,	491a4,52c50,0

c					//dialog name box
a74c4	10,8,20,4,	5c300,5c4ac,5

     */
}
