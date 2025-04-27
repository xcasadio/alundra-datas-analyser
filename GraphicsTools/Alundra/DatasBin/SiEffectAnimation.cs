namespace Alundra.DatasBin;

public class SiEffectAnimation
{
    public SiEffectAnimation(BinaryReader br, int effectid, int binoffset, int memaddr)
    {
        Memaddr = memaddr;
        Frames = new SiEffectFrame[32];//32 max frames?
        for (var dex = 0; dex < Frames.Length; dex++)
        {
            //read test bytes to check for the end of the list
            short test = br.ReadByte();
            if ((test & 0x80) != 0x80)
            {
                break;
            }

            Numframes++;
            br.BaseStream.Position -= 1;

            Frames[dex] = new SiEffectFrame(br, effectid, binoffset, memaddr + dex * 3);

            for (var dex2 = 0; dex2 < dex; dex2++)
            {
                if (Frames[dex2].ImageSetPointer == Frames[dex].ImageSetPointer)
                {
                    Frames[dex].Images = Frames[dex2].Images;
                    break;
                }
            }
        }
    }
    public int Memaddr;
    public int Numframes;
    public readonly SiEffectFrame[] Frames;
}