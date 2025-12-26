namespace AlundraEngine.DatasBin;

public class SiEffectAnimation
{
    public int MemoryAddress;
    public int NumberOfFrames;
    public readonly SiEffectFrame[] Frames;

    public SiEffectAnimation(BinaryReader br, int effectid, int binoffset, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        Frames = new SiEffectFrame[32];//we can compute the number of frames before

        for (var i = 0; i < Frames.Length; i++)
        {
            var test = br.ReadByte();

            if ((test & 0x80) == 0) // != 0x80
            {
                var value = br.ReadByte();
                //check if the frame is a transition frame

                //if ((value & 0x80) == 0 /*&& value != 0*/) // TODO check value != 0
                NumberOfFrames++;
                Frames[i] = new SiEffectFrame(test, value, memoryAddress + i * 3);
                break;
            }

            NumberOfFrames++;
            br.BaseStream.Position -= 1;

            Frames[i] = new SiEffectFrame(br, effectid, binoffset, memoryAddress + i * 3);

            for (var j = 0; j < i; j++)
            {
                if (Frames[j].ImageSetPointer == Frames[i].ImageSetPointer)
                {
                    Frames[i].Images = Frames[j].Images;
                    break;
                }
            }
        }
    }
}