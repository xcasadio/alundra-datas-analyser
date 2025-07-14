namespace AlundraEngine.DatasBin;

public class SiAnimation
{
    public SiAnimation(BinaryReader br, SpriteTableHeader header, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        var frames = new SiFrame[64]; //we can compute the number of frames before

        for (var i = 0; i < frames.Length; i++)
        {
            //read two test bytes to check for the end of the list
            var test = br.ReadByte();
            if ((test & 0x80) == 0)
            {
                var value = br.ReadByte();
                //check if the frame is a transition frame
                //if ((value & 0x80) == 0 /*&& value != 0*/) // TODO check value != 0
                {
                    NumberOfFrames++;
                    frames[i] = new SiFrame(test, value, memoryAddress + i * 5);
                }

                Array.Resize(ref frames, NumberOfFrames);
                Frames = frames;
                break;
            }

            NumberOfFrames++;
            br.BaseStream.Position -= 1;

            frames[i] = new SiFrame(br, header, memoryAddress + i * 5); // 5 = sizeof(SiFrame)

            for (var j = 0; j < i; j++)
            {
                if (frames[j].ImageSetPointer == frames[i].ImageSetPointer)
                {
                    frames[i].Images = frames[j].Images;
                    break;
                }
            }
        }
    }
    public readonly int MemoryAddress;
    public readonly int NumberOfFrames;
    public readonly SiFrame[] Frames;
}