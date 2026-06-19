namespace AlundraEngine.DatasBin;

public class SiAnimation
{
    public readonly int MemoryAddress;
    public readonly int NumberOfFrames;
    public readonly SiFrame[] Frames;

    public SiAnimation(BinaryReader br, SpriteTableHeader header, int memoryAddress)
    {
        MemoryAddress = memoryAddress;

        var frames = new List<SiFrame>();

        while (true)
        {
            var i = frames.Count;

            //read two test bytes to check for the end of the list
            var test = br.ReadByte();
            if ((test & 0x80) == 0)
            {
                var value = (byte)0;

                if (test == 0)
                {
                    value = br.ReadByte();
                }

                //check if the frame is a transition frame
                //if ((value & 0x80) == 0 /*&& value != 0*/) // TODO check value != 0

                frames.Add(new SiFrame(test, value, memoryAddress + i * 5));
                break;
            }

            br.BaseStream.Position -= 1;

            var frame = new SiFrame(br, header, memoryAddress + i * 5); // 5 = sizeof(SiFrame)

            for (var j = 0; j < i; j++)
            {
                if (frames[j].ImageSetPointer == frame.ImageSetPointer)
                {
                    frame.Images = frames[j].Images;
                    break;
                }
            }

            frames.Add(frame);
        }

        Frames = frames.ToArray();
        NumberOfFrames = Frames.Length;
    }
}