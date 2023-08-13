using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsTools.Alundra
{
    public class UIRecord
    {
        public int Status;//1 = active
        public UIBoxAnimated boxAnimated;//04 ptr or 0
        public short x;//8
        public short y;//a
        public short width;//c //in 8s
        public short height;//e //in 8s
        public UIFunction SetupFunc;//10 setup function
        public UIFunction RenderFunc;//14 render function
        public int UnknownVal;//18 0, -1,5
    }

    public class UIBoxAnimated
    {
        public short x;
        public short y;
        public short width;//in 8s
        public short height;// in 8s
        public UIDrawCmd[][] boxcommands = new UIDrawCmd[0xa][];//drawareaid is an index into this
    }

    //20 byte records
    public class UIDrawCmd
    {
        public short x, y;
        public byte u, v;
        public short uipaletteindex;//(clut address - 0x7812)/ 64
        public short spritesheet;
        public short w, h;

        public long signature { get
            {
                return spritesheet | uipaletteindex << 8 | u << 16 | v << 24 | w << 32 | h << 38;
            }
        }
    }

    //this structure lerps (linear interpolation) coordinates over a period of ticks
    public class UILerper
    {
        public int currenttick;//0 tick progress, starts at 9
        public int numticks;//4 number of ticks to iterate
        public int tickstolinger;//8 ticks to linger once the lerp is finished, countsdown to zero then lerp function returns true (finished)
        public short x1;//c
        public short y1;//e
        public short x2;//10
        public short y2;//12

        public short AfterX, AfterY;
    }


    public class FontCharInfo
    {
        public int width;//width (kerning)
        public int height;//height
        public int sx;//source bitmap x
        public int sy;//source bitmap y
        public int y;//y offset from top
    }

    

    public delegate bool UIFunction(UIRecord ui);
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
