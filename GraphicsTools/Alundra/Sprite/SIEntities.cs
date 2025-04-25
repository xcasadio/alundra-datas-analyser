namespace Alundra.Sprite;

public class SiEntities
{
    public SiEntities(BinaryReader br)
    {
        br.BaseStream.Position += 2;

        Entities = new SiEntityRecord[128];
        for (var dex = 0; dex < Entities.Length; dex++)
        {
            //read two test bytes to check for the end of the list
            var test = br.ReadInt16();
            if (test == 0)
            {
                break;
            }

            br.BaseStream.Position -= 2;

            //read the record
            Entities[dex] = new SiEntityRecord(br);
        }
    }
    public readonly SiEntityRecord[] Entities;
}