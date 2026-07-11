namespace AlundraEngine.DatasBin;

public class SiEffectAnimation
{
    public int MemoryAddress;
    public int NumberOfFrames;
    public readonly SiEffectFrame[] Frames;

    public SiEffectAnimation(BinaryReader br, int effectid, int binoffset, int memoryAddress)
    {
        MemoryAddress = memoryAddress;

        // JUSTIFICATION: C# language bridge only
        // The raw animation data is a variable-length byte stream terminated by a
        // sentinel frame (top bit clear), not a fixed-size table, so the frame count
        // isn't known up front and must grow to fit whatever the stream contains.
        var frames = new List<SiEffectFrame>();

        while (true)
        {
            var i = frames.Count;
            var test = br.ReadByte();

            if ((test & 0x80) == 0) // != 0x80
            {
                var value = (byte)0;

                if (test == 0)
                {
                    value = br.ReadByte();
                }

                //check if the frame is a transition frame

                //if ((value & 0x80) == 0 /*&& value != 0*/) // TODO check value != 0
                frames.Add(new SiEffectFrame(test, value, memoryAddress + i * 3));
                break;
            }

            br.BaseStream.Position -= 1;

            var frame = new SiEffectFrame(br, effectid, binoffset, memoryAddress + i * 3);

            for (var j = 0; j < frames.Count; j++)
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