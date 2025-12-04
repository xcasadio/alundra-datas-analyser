namespace AlundraEngine.DatasBin;

public class GameMapHeader
{
    public GameMapHeader(DataBinHeader header)
    {
        //alundra gamemap, just has sprites
        InfoBlockOffset = -1;
        MapBlockOffset = -1;
        TileSheetsOffset = -1;
        SpriteRecordsOffset = (int)header.AlundraSpriteRecordsOffset;
        SpriteSheetOffset = (int)header.AlundraSpriteSheetOffset;
        ScrollingScreenOffset = -1;
        StringTableOffset = (int)header.AlundraStringTableOffset;

        InfoSize = 0;
        MapSize = 0;
        TilesSize = 0;
        SpriteInfoSize = SpriteSheetOffset - SpriteRecordsOffset;
        SpritesSize = (int)header.DrawPageParam - SpriteSheetOffset;
        ScrollingSize = 0;
    }

    public GameMapHeader(BinaryReader br)
    {
        InfoBlockOffset = br.ReadInt32();//0
        MapBlockOffset = br.ReadInt32();//4
        TileSheetsOffset = br.ReadInt32();//8
        SpriteRecordsOffset = br.ReadInt32();//c
        SpriteSheetOffset = br.ReadInt32();//10
        ScrollingScreenOffset = br.ReadInt32();//14
        StringTableOffset = br.ReadInt32();//18

        InfoSize = MapBlockOffset - InfoBlockOffset;
        MapSize = TileSheetsOffset - MapBlockOffset;
        TilesSize = SpriteRecordsOffset - TileSheetsOffset;
        SpriteInfoSize = SpriteSheetOffset - SpriteRecordsOffset;
        SpritesSize = ScrollingScreenOffset - SpriteSheetOffset;
        ScrollingSize = StringTableOffset - ScrollingScreenOffset;
        //string table is called later
    }

    public readonly int InfoSize;
    public int MapSize;
    public int WallTilesSize;
    public readonly int TilesSize;
    public readonly int SpriteInfoSize;
    public readonly int SpritesSize;
    public readonly int ScrollingSize;
    public int StringSize;

    public readonly int InfoBlockOffset;
    public readonly int MapBlockOffset;
    public readonly int TileSheetsOffset;
    public readonly int SpriteRecordsOffset;
    public readonly int SpriteSheetOffset;
    public readonly int ScrollingScreenOffset;
    public readonly int StringTableOffset;
}