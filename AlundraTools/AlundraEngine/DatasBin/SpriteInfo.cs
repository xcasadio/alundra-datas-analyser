using AlundraEngine.Graphics;

namespace AlundraEngine.DatasBin;

public class SpriteInfo
{
    public readonly SpriteInfoHeader Header;
    public readonly SpriteInfoEventCodes EventCodes;
    public readonly SpriteInfoEntities Entities;
    public readonly SpriteInfoMapEvents MapEvents;

    public readonly int[] SpriteTable;
    public readonly SpriteRecord[] SpriteRecords;

    public readonly int[] SpriteEffectTable;
    public readonly SpriteEffectRecord[] SpriteEffectRecords;
    public readonly int NumSpriteEffects;

    public readonly MapEffectRecord[] MapEffectRecords;

    private long _binOffset;

    public readonly Color[][] Palettes;
    public readonly Bitmap PalettesBitmap;

    public SpriteInfo(BinaryReader br, int memoryAddress, int sectorEnd)
    {
        _binOffset = br.BaseStream.Position;

        Header = new SpriteInfoHeader(br, memoryAddress);

        //read sprite table
        br.BaseStream.Position = _binOffset + Header.SpriteTablePointer;
        SpriteTable = new int[Header.SpriteTableSize / 4];
        for (var i = 0; i < SpriteTable.Length; i++)
        {
            SpriteTable[i] = br.ReadInt32();
        }

        //read sprite effect table
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
        var maxPalettes = 41; //32;
        Palettes = new Color[maxPalettes][];
        var buff = new byte[maxPalettes * 16 * 2];
        br.Read(buff, 0, buff.Length);
        var buffdex = 0;
        for (var i = 0; i < maxPalettes; i++)
        {
            Palettes[i] = new Color[16];

            for (var j = 0; j < 16; j++)
            {
                var b2 = buff[buffdex++];
                var b1 = buff[buffdex++];
                Palettes[i][j] = ImageHelper.FromPsxColor((b1 << 8) | b2);
            }
        }
        PalettesBitmap = ImageHelper.BitmapFromPsxBuff(buff, 16, maxPalettes, 16, null);

        //read eventcodes
        EventCodes = new SpriteInfoEventCodes(br, _binOffset, Header);

        //read entities
        br.BaseStream.Position = _binOffset + Header.EntitiesPointer;
        Entities = new SpriteInfoEntities(br, memoryAddress + Header.EntitiesPointer);

        //read mapevents
        br.BaseStream.Position = _binOffset + Header.MapEventsPointer;
        MapEvents = new SpriteInfoMapEvents(br, _binOffset, sectorEnd);

        //read sprite table records;
        SpriteRecords = new SpriteRecord[SpriteTable.Length];
        for (var i = 0; i < SpriteRecords.Length; i++)
        {
            if (SpriteTable[i] != -1 && SpriteTable[i] != 0)
            {
                br.BaseStream.Position = _binOffset + SpriteTable[i];
                SpriteRecords[i] = new SpriteRecord(br, _binOffset, i, memoryAddress + SpriteTable[i], memoryAddress);
            }
        }

        SpriteEffectRecords = new SpriteEffectRecord[NumSpriteEffects];
        for (var i = 0; i < SpriteEffectRecords.Length; i++)
        {
            if (SpriteEffectTable[i] != -1)
            {
                br.BaseStream.Position = _binOffset + SpriteEffectTable[i];
                SpriteEffectRecords[i] = new SpriteEffectRecord(br, _binOffset + SpriteEffectTable[i], i | 0x8000, memoryAddress + SpriteEffectTable[i], memoryAddress + SpriteEffectTable[i]);
            }
        }
    }
}