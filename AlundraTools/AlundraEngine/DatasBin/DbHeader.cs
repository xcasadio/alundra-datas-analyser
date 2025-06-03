namespace AlundraEngine.DatasBin;

public class DbHeader
{
    public DbHeader(BinaryReader br)
    {
        AlundraSpriteInfoOffset = br.ReadUInt32();//0
        AlundraSpritesOffset = br.ReadUInt32();//4
        AlundraSpritesRepeatOffset = br.ReadUInt32();//8
        AlundraStringTableOffset = br.ReadUInt32();//c
        AlundraStringTableRepeatOffset = br.ReadUInt32();//10

        UnknownMapA = br.ReadUInt32();//14 g_currentDrawPageParam
        UnknownMapB = br.ReadUInt32();//18 offset to fill g_orderingTableBuffer in 8002be98, used to exit game
        UnknownMapB2 = br.ReadUInt32();//1c
        UnknownMapB3 = br.ReadUInt32();//20
        UnknownMapB4 = br.ReadUInt32();//24

        GameMaps = new uint[502];//28

        for (var i = 0; i < GameMaps.Length; i++)
        {
            GameMaps[i] = br.ReadUInt32();
        }
    }

    public readonly uint AlundraSpriteInfoOffset;
    public readonly uint AlundraSpritesOffset;
    public uint AlundraSpritesRepeatOffset;
    public readonly uint AlundraStringTableOffset;
    public uint AlundraStringTableRepeatOffset;
    public readonly uint UnknownMapA;
    public uint UnknownMapB;
    public uint UnknownMapB2;
    public uint UnknownMapB3;
    public uint UnknownMapB4;
    public readonly uint[] GameMaps;
}