namespace Alundra.DatasBin;

public class SpriteInfo
{
    public SpriteInfo(BinaryReader br, int memoryAddress, int sectorEnd, bool ismap)
    {
        _binOffset = br.BaseStream.Position;

        Header = new SpriteInfoHeader(br, memoryAddress);

        //read sprite table
        br.BaseStream.Position = _binOffset + Header.SpriteTablePointer;
        SpriteTable = new int[0xff];
        for (var i = 0; i < SpriteTable.Length; i++)
        {
            SpriteTable[i] = br.ReadInt32();
        }

        br.BaseStream.Position = _binOffset + Header.SpriteEffectsPointer;
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
        br.BaseStream.Position = _binOffset + Header.MapEffectSector3Pointer;
        MapEffectRecords = new MapEffectRecord[Header.MapEffectSector3Size / 12];
        for (var i = 0; i < MapEffectRecords.Length; i++)
        {
            MapEffectRecords[i] = new MapEffectRecord(br);
        }

        //read palettes
        br.BaseStream.Position = _binOffset + Header.SpritePalettesPointer;
        var maxpalettes = 32;
        Palettes = new Color[maxpalettes][];
        var buff = new byte[maxpalettes * 16 * 2];
        br.Read(buff, 0, buff.Length);
        var buffdex = 0;
        for (var i = 0; i < maxpalettes; i++)
        {
            Palettes[i] = new Color[16];
            for (var j = 0; j < 16; j++)
            {
                var b2 = buff[buffdex++];
                var b1 = buff[buffdex++];
                Palettes[i][j] = ImageHelper.FromPsxColor((b1 << 8) | b2);
            }
        }
        PalettesBitmap = ImageHelper.BitmapFromPsxBuff(buff, 16, maxpalettes, 16, null);

        //read eventcodes
        EventCodes = new SpriteInfoEventCodes(br, _binOffset, Header, ismap);

        //read entities
        br.BaseStream.Position = _binOffset + Header.EntitiesPointer;
        Entities = new SpriteInfoEntities(br, memoryAddress + Header.EntitiesPointer);

        //read mapevents
        br.BaseStream.Position = _binOffset + Header.MapEventsPointer;
        MapEvents = new SpriteInfoMapEvents(br, _binOffset, sectorEnd);

        //read sprite table records;
        Sprites = new SpriteRecord[SpriteTable.Length];
        for (var i = 0; i < Sprites.Length; i++)
        {
            if (SpriteTable[i] != -1)
            {
                br.BaseStream.Position = _binOffset + SpriteTable[i];
                Sprites[i] = new SpriteRecord(br, _binOffset, i, memoryAddress + SpriteTable[i], memoryAddress);
            }
        }

        SpriteEffects = new SpriteEffectRecord[NumSpriteEffects];
        for (var i = 0; i < SpriteEffects.Length; i++)
        {
            if (SpriteEffectTable[i] != -1)
            {
                br.BaseStream.Position = _binOffset + SpriteEffectTable[i];
                SpriteEffects[i] = new SpriteEffectRecord(br, _binOffset + SpriteEffectTable[i], i | 0x8000, memoryAddress + SpriteEffectTable[i], memoryAddress + SpriteEffectTable[i]);
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
    public readonly SpriteEffectRecord[] SpriteEffects;
    public readonly int NumSpriteEffects;

    public readonly MapEffectRecord[] MapEffectRecords;

    private long _binOffset;

    public readonly Color[][] Palettes;
    public readonly Bitmap PalettesBitmap;
}