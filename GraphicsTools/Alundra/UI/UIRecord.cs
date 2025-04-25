namespace Alundra.UI;

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

//20 byte records

//this structure lerps (linear interpolation) coordinates over a period of ticks

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