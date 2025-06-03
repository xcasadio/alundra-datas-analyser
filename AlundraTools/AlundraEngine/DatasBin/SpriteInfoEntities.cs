namespace AlundraEngine.DatasBin;

public class SpriteInfoEntities
{
    public SpriteInfoEntities(BinaryReader br, int memaddr)
    {
        //br.BaseStream.Position += 2;//this is wrong, dont nudge it like this

        Entities = new SiEntityRecord[128];
        for (var i = 0; i < Entities.Length; i++)
        {
            //read two test bytes to check for the end of the list
            var test = br.ReadInt16();
            test = br.ReadInt16();
            if (test == 0)
            {
                break;
            }

            br.BaseStream.Position -= 4;

            //read the record
            Entities[i] = new SiEntityRecord(br, memaddr + i * 20);
        }
    }
    public readonly SiEntityRecord[] Entities;
}