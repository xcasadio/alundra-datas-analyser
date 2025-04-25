namespace Alundra.DatasBin;

public class SpriteInfoMapEvents
{
    public SpriteInfoMapEvents(BinaryReader br, long sioffset, int sectorend)
    {

        Records = new SiMapEventRecord[64];
        for (var dex = 0; dex < Records.Length; dex++)
        {
            //read two test bytes to check for the end of the list
            var test = br.ReadInt32();
            if (test == 0)
            {
                break;
            }

            br.BaseStream.Position -= 4;

            //read the record
            Records[dex] = new SiMapEventRecord(br);
        }

    }

    public readonly SiMapEventRecord[] Records;

}