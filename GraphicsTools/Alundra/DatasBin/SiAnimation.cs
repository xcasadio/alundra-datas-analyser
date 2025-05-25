namespace Alundra.DatasBin;

public class SiAnimation
{
    public SiAnimation(BinaryReader br, SpriteTableHeader header, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        Frames = new SiFrame[64];//32 max frames?

        for (var i = 0; i < Frames.Length; i++)
        {
            //read two test bytes to check for the end of the list
            short test = br.ReadByte();
            if ((test & 0x80) != 0x80)
            {
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