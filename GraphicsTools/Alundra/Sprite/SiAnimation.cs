namespace Alundra.Sprite;

public class SiAnimation
{
    public SiAnimation(BinaryReader br, SiAnimationSetHeader header)
    {
        Frames = new SiFrame[32];//32 max frames?
        for (var dex = 0; dex < Frames.Length; dex++)
        {
            //read two test bytes to check for the end of the list
            var test = br.ReadInt16();
            if (test == 0)
            {
                break;
            }

            Numframes++;
            br.BaseStream.Position -= 2;

            Frames[dex] = new SiFrame(br, header);
        }
    }
    public int Numframes;
    public readonly SiFrame[] Frames;
}