namespace Alundra.DatasBin;

public class SpriteInfo
{
    public SpriteInfo(BinaryReader br, int memaddr, int sectorend, bool ismap)
    {
        _binoffset = br.BaseStream.Position;

        Header = new SpriteInfoHeader(br, memaddr);

        //read sprite table
        br.BaseStream.Position = _binoffset + Header.Spritetablepointer;
        SpriteTable = new int[0xff];
        for (var dex = 0; dex < SpriteTable.Length; dex++)
        {
            SpriteTable[dex] = br.ReadInt32();
        }

        br.BaseStream.Position = _binoffset + Header.Spriteeffectspointer;
        SpriteEffectTable = new int[0xff];
        for (var i = 0; i < SpriteEffectTable.Length; i++)
        {
            SpriteEffectTable[i] = br.ReadInt32();
            if (SpriteEffectTable[i] == 0)
            {
                NumSpriteEffects = i;
                break;
            }
        }

        //event effects
        br.BaseStream.Position = _binoffset + Header.Mapeffectsector3Pointer;
        MapEffectRecords = new MapEffectRecord[Header.Mapeffectsector3Size / 12];
        for (var i = 0; i < MapEffectRecords.Length; i++)
        {
            MapEffectRecords[i] = new MapEffectRecord(br);
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

        //read eventcodes
        EventCodes = new SpriteInfoEventCodes(br, _binoffset, Header, ismap);

        //read entities
        br.BaseStream.Position = _binoffset + Header.Entitiespointer;
        Entities = new SpriteInfoEntities(br, memaddr + Header.Entitiespointer);

        //read mapevents
        br.BaseStream.Position = _binoffset + Header.Mapeventspointer;
        MapEvents = new SpriteInfoMapEvents(br, _binoffset, sectorend);

        //read sprite table records;
        Sprites = new SpriteRecord[SpriteTable.Length];
        for (var dex = 0; dex < Sprites.Length; dex++)
        {
            if (SpriteTable[dex] != -1)
            {
                br.BaseStream.Position = _binoffset + SpriteTable[dex];
                Sprites[dex] = new SpriteRecord(br, _binoffset, dex, memaddr + SpriteTable[dex], memaddr);
            }
        }

        Spriteeffects = new SpriteEffectRecord[NumSpriteEffects];
        for (var dex = 0; dex < Spriteeffects.Length; dex++)
        {
            if (SpriteEffectTable[dex] != -1)
            {
                br.BaseStream.Position = _binoffset + SpriteEffectTable[dex];
                Spriteeffects[dex] = new SpriteEffectRecord(br, _binoffset + SpriteEffectTable[dex], dex | 0x8000, memaddr + SpriteEffectTable[dex], memaddr + SpriteEffectTable[dex]);
            }
        }
    }

    public readonly SpriteInfoHeader Header;
    public readonly SpriteInfoEventCodes EventCodes;
    public readonly SpriteInfoEntities Entities;
    public readonly SpriteInfoMapEvents MapEvents;

    public readonly int[] SpriteTable;
    public readonly SpriteRecord[] Sprites;

    public readonly int[] SpriteEffectTable;
    public readonly SpriteEffectRecord[] Spriteeffects;
    public readonly int NumSpriteEffects;

    public readonly MapEffectRecord[] MapEffectRecords;

    private long _binoffset;

    public readonly Color[][] Palettes;
    public readonly Bitmap Palettesbitmap;
}