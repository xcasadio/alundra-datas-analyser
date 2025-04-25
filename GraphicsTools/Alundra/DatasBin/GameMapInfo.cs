namespace Alundra.DatasBin;

public class GameMapInfo
{
    public GameMapInfo(int mapId, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        MapId = mapId;
    }

    public GameMapInfo(BinaryReader br, int memoryAddress)
    {
        MemoryAddress = memoryAddress; // start just after the header ??
        var startPosition = br.BaseStream.Position;
        MapId = br.ReadInt32();//0
        Gravity = br.ReadInt16();//4
        TerminalVelocity = br.ReadInt16();//8
        SlideEffectId = br.ReadByte();//a
        BalanceLevel = br.ReadByte();//b
        C = br.ReadByte();//c
        D = br.ReadByte();//d
        E = br.ReadByte();//e
        F = br.ReadByte();//f
        _10 = br.ReadInt16();//10
        //read palettes
        var maxPalettes = 32;
        Palettes = new Color[maxPalettes][];
        var buff = new byte[maxPalettes * 16 * 2];
        br.Read(buff, 0, buff.Length);
        var buffIndex = 0;

        for (var i = 0; i < maxPalettes; i++)
        {
            Palettes[i] = new Color[16];
            for (var j = 0; j < 16; j++)
            {
                var b2 = buff[buffIndex++];
                var b1 = buff[buffIndex++];
                Palettes[i][j] = ImageHelper.FromPsxColor((b1 << 8) | b2);
            }
        }

        PalettesBitmap = ImageHelper.BitmapFromPsxBuff(buff, 16, maxPalettes, 16, null);

        //read portals
        br.BaseStream.Position = startPosition + 1066;
        PortalFlag1 = br.ReadByte();
        PortalFlag2 = br.ReadByte();
        var maxPortals = 64;
        Portals = new Portal[maxPortals];
        for (var i = 0; i < Portals.Length; i++)
        {
            Portals[i] = new Portal(br);
        }
    }

    public readonly int MemoryAddress;
    public readonly int MapId;
    public readonly short Gravity;
    public readonly short TerminalVelocity;
    public readonly byte SlideEffectId;
    public readonly byte BalanceLevel;
    public readonly byte C;
    public readonly byte D;
    public readonly byte E;
    public readonly byte F;
    public readonly short _10;
    public readonly Color[][] Palettes;
    public readonly byte PortalFlag1;
    public readonly byte PortalFlag2;
    public readonly Portal[] Portals;
    
    public readonly Bitmap PalettesBitmap;
}