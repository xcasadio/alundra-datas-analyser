namespace AlundraEngine.DatasBin;

public class DataBinHeader
{
    public DataBinHeader(BinaryReader br)
    {
        AlundraSpriteRecordsOffset = br.ReadUInt32();//0
        AlundraSpriteSheetOffset = br.ReadUInt32();//4
        AlundraSpritesRepeatOffset = br.ReadUInt32();//8
        AlundraStringTableOffset = br.ReadUInt32();//c
        AlundraStringTableRepeatOffset = br.ReadUInt32();//10

        DrawPageParam = br.ReadUInt32();//14 g_currentDrawPageParam
        LoadingScreen0 = br.ReadUInt32();//18 offset to fill g_orderingTableBuffer in 8002be98, used to exit game
        LoadingScreen1 = br.ReadUInt32();//1c
        LoadingScreen2 = br.ReadUInt32();//20
        LoadingScreen3 = br.ReadUInt32();//24

        GameMapOffsets = new uint[502];//28

        for (var i = 0; i < GameMapOffsets.Length; i++)
        {
            GameMapOffsets[i] = br.ReadUInt32();
        }
    }

    public readonly uint AlundraSpriteRecordsOffset;
    public readonly uint AlundraSpriteSheetOffset;
    public uint AlundraSpritesRepeatOffset;
    public readonly uint AlundraStringTableOffset;
    public uint AlundraStringTableRepeatOffset;
    public readonly uint DrawPageParam;
    public uint LoadingScreen0;
    public uint LoadingScreen1;
    public uint LoadingScreen2;
    public uint LoadingScreen3;
    public readonly uint[] GameMapOffsets;
}