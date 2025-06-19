namespace AlundraEngine.DatasBin;

public class GameMapHeader
{
    public GameMapHeader(DbHeader header)
    {
        //alundra gamemap, just has sprites
        InfoBlockOffset = -1;
        MapBlockOffset = -1;
        TileSheetsOffset = -1;
        SpriteInfoOffset = (int)header.AlundraSpriteInfoOffset;
        SpriteSheetsOffset = (int)header.AlundraSpritesOffset;
        ScrollScreenOffset = -1;
        StringTableOffset = (int)header.AlundraStringTableOffset;

        InfoSize = 0;
        MapSize = 0;
        TilesSize = 0;
        SpriteInfoSize = SpriteSheetsOffset - SpriteInfoOffset;
        SpritesSize = (int)header.UnknownMapA - SpriteSheetsOffset;
        ScrollSize = 0;
    }

    public GameMapHeader(BinaryReader br)
    {
        InfoBlockOffset = br.ReadInt32();//0
        MapBlockOffset = br.ReadInt32();//4
        TileSheetsOffset = br.ReadInt32();//8
        SpriteInfoOffset = br.ReadInt32();//c
        SpriteSheetsOffset = br.ReadInt32();//10
        ScrollScreenOffset = br.ReadInt32();//14
        StringTableOffset = br.ReadInt32();//18

        InfoSize = MapBlockOffset - InfoBlockOffset;
        MapSize = TileSheetsOffset - MapBlockOffset;
        TilesSize = SpriteInfoOffset - TileSheetsOffset;
        SpriteInfoSize = SpriteSheetsOffset - SpriteInfoOffset;
        SpritesSize = ScrollScreenOffset - SpriteSheetsOffset;
        ScrollSize = StringTableOffset - ScrollScreenOffset;
        //string table is called later
    }

    public readonly int InfoSize;
    public int MapSize;
    public int WallTilesSize;
    public readonly int TilesSize;
    public readonly int SpriteInfoSize;
    public readonly int SpritesSize;
    public readonly int ScrollSize;
    public int StringSize;

    public readonly int InfoBlockOffset;
    public readonly int MapBlockOffset;
    public readonly int TileSheetsOffset;
    public readonly int SpriteInfoOffset;
    public readonly int SpriteSheetsOffset;
    public readonly int ScrollScreenOffset;//shadow, sky or distant background
    public readonly int StringTableOffset;
}