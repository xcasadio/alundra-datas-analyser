namespace Alundra.Sprite;

public class SpriteInfoSector4
{
    public SpriteInfoSector4(BinaryReader br)
    {
        br.BaseStream.Position += 2;

        Records = new SiSector4Record[64];
        for (var dex = 0; dex < Records.Length; dex++)
        {
            //read two test bytes to check for the end of the list
            var test = br.ReadInt16();
            if (test == 0)
            {
                break;
            }

            br.BaseStream.Position -= 2;

            //read the record
            Records[dex] = new SiSector4Record(br);
        }
    }

    public readonly SiSector4Record[] Records;
}