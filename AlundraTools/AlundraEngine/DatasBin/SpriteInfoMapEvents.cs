namespace AlundraEngine.DatasBin;

public class SpriteInfoMapEvents
{
    public readonly SiMapEventRecord[] Records;

    public SpriteInfoMapEvents(BinaryReader br, long sioffset, int sectorend)
    {
        Records = new SiMapEventRecord[64];

        for (var i = 0; i < Records.Length; i++)
        {
            //read two test bytes to check for the end of the list
            var test = br.ReadInt32();
            if (test == 0)
            {
                break;
            }

            br.BaseStream.Position -= 4;

            //read the record
            Records[i] = new SiMapEventRecord(br);
        }
    }
}