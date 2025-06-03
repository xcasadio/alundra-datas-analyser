namespace AlundraEngine.DatasBin;

public class SiAnimation
{
    public SiAnimation(BinaryReader br, SpriteTableHeader header, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        Frames = new SiFrame[64];//32 max frames?

        for (var i = 0; i < Frames.Length; i++)
        {
            //read two test bytes to check for the end of the list
            var test = br.ReadByte();
            if ((test & 0x80) != 0x80)
            {
                var value = br.ReadByte();
                //check if the frame is a transition frame
                if ((value & 0x80) == 0 && value != 0) // TODO check value != 0
                {
                    NumberOfFrames++;
                    Frames[i] = new SiFrame(test, value, memoryAddress + i * 5);
                }
                
                break;
            }

            NumberOfFrames++;
            br.BaseStream.Position -= 1;

            Frames[i] = new SiFrame(br, header, memoryAddress + i * 5); // 5 = sizeof(SiFrame)

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
    public readonly int MemoryAddress;
    public readonly int NumberOfFrames;
    public readonly SiFrame[] Frames;
}