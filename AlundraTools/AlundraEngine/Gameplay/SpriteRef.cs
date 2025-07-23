using AlundraEngine.DatasBin;

namespace AlundraEngine.Gameplay;

public class SpriteRef
{
    public SiImage[] Images;        //c
    public int X;//4				//10
    public int Y;//8				//14
    public int Z;//c				//18
    public int DepthSortValue;//0x10		//1c
    public int NumberOfImages;//0x14		//20

    public void Reset()
    {
        Images = null;
        X = 0;
        Y = 0;
        Z = 0;
        DepthSortValue = 0;
        NumberOfImages = 0;
    }
}