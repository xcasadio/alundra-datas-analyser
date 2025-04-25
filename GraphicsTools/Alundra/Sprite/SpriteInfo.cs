namespace Alundra.Sprite;

public class SpriteInfo
{
    public SpriteInfo(BinaryReader br)
    {
        _binoffset = br.BaseStream.Position;

        Header = new SpriteInfoHeader(br);

        //read sector5 table
        br.BaseStream.Position = _binoffset + Header.Sector5Tablepointer;
        _sector5Table = new int[0xff];
        for (var dex = 0; dex < _sector5Table.Length; dex++)
        {
            _sector5Table[dex] = br.ReadInt32();
        }

        //read palettes
        br.BaseStream.Position = _binoffset + Header.Spritepalettespointer;
        var maxpalettes = 32;
        Palettes = new Color[maxpalettes][];
        var buff = new byte[maxpalettes * 16 * 2];
        br.Read(buff, 0, buff.Length);
        var buffdex = 0;
        for (var dex = 0; dex < maxpalettes; dex++)
        {
            Palettes[dex] = new Color[16];
            for (var cdex = 0; cdex < 16; cdex++)
            {
                var b2 = buff[buffdex++];
                var b1 = buff[buffdex++];
                Palettes[dex][cdex] = ImageHelper.FromPsxColor((b1 << 8) | b2);
            }
        }
        Palettesbitmap = ImageHelper.BitmapFromPsxBuff(buff, 16, maxpalettes, 16, null);

        //read sector1
        Eventcodes = new SiEventCodes(br, _binoffset, Header);

        //read sector2
        br.BaseStream.Position = _binoffset + Header.Sector2Pointer;
        Entities = new SiEntities(br);

        //read sector 3
        br.BaseStream.Position = _binoffset + Header.Sector3Pointer;
        Sector3 = new SpriteInfoSector3(br);

        //read sector 4
        br.BaseStream.Position = _binoffset + Header.Sector4Pointer;
        Sector4 = new SpriteInfoSector4(br);

        //read sector5 records;
        Animationsets = new SiAnimationSet[_sector5Table.Length];
        for (var dex = 0; dex < Animationsets.Length; dex++)
        {
            if (_sector5Table[dex] != -1)
            {
                br.BaseStream.Position = _binoffset + _sector5Table[dex];
                Animationsets[dex] = new SiAnimationSet(br, _binoffset);
            }
        }
    }

    public readonly SpriteInfoHeader Header;
    public SiEventCodes Eventcodes;
    public SiEntities Entities;
    public SpriteInfoSector3 Sector3;
    public SpriteInfoSector4 Sector4;

    private int[] _sector5Table;
    public readonly SiAnimationSet[] Animationsets;

    private long _binoffset;

    public readonly Color[][] Palettes;
    public Bitmap Palettesbitmap;
}