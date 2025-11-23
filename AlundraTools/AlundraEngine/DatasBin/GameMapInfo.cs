using System.Diagnostics;

namespace AlundraEngine.DatasBin;

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
        TerminalVelocity = br.ReadInt16();//6
        SlideEffectId = br.ReadByte();//a
        BalanceLevel = br.ReadByte();//b
        C = br.ReadByte();//c
        D = br.ReadByte();//d
        E = br.ReadByte();//e
        F = br.ReadByte();//f
        _10 = br.ReadByte();//10
        _11 = br.ReadByte();//10

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

        byte[] unused = new byte[16];
        br.Read(unused, 0, 16);

        SpriteMapEntries = new SpriteMapEntry[6];

        //InitializeMapSpriteTable 8002cc58
        int spriteIndex = 0;
        do
        {
            var val1 = br.ReadByte();
            var val2 = br.ReadByte();
        
            SpriteMapEntries[spriteIndex] = new SpriteMapEntry();
        
            if (val1 == 0 ||
                val2 == 0)
            {
                SpriteMapEntries[spriteIndex].Enabled = 0;
                SpriteMapEntries[spriteIndex].Index = 0;
            }
            else
            {
                SpriteMapEntries[spriteIndex].Enabled = 1;
                SpriteMapEntries[spriteIndex].NumberOfFrame = (byte)(1 << (val1 & 0x1f));
        
                if (SpriteMapEntries[spriteIndex].NumberOfFrame == 0)
                {
                    Debugger.Break();
                    //Trap(0x1c00);
                }
        
                SpriteMapEntries[spriteIndex].TileWidth = (byte)(0xa0 / SpriteMapEntries[spriteIndex].NumberOfFrame);
                byte rowCount = val2;
        
                SpriteMapEntries[spriteIndex].FrameIndex = 0;
                SpriteMapEntries[spriteIndex].Tick = 0;
                SpriteMapEntries[spriteIndex].Index = 0;
                SpriteMapEntries[spriteIndex].FrameDuration = rowCount;
            }
        
            spriteIndex++;
        } while (spriteIndex < 6);

        //read portals
        br.BaseStream.Position = startPosition + 1066;
        PortalFlag1 = br.ReadByte();
        PortalFlag2 = br.ReadByte();
        var maxPortals = 64;
        Portals = new WarpData[maxPortals];
        for (var i = 0; i < Portals.Length; i++)
        {
            Portals[i] = new WarpData(br);
        }
    }

    public readonly int MemoryAddress;

    public readonly int MapId; //0
    public readonly short Gravity; //4
    public readonly short TerminalVelocity; // ZViscosity
    public readonly byte SlideEffectId; // XYResistance
    public readonly byte BalanceLevel; // AnimDeleteWall
    public readonly byte C; 
    public readonly byte D;
    public readonly byte E;
    public readonly byte F;
    public readonly byte _10; // AnimLandFloor
    public readonly byte _11; // item something
    public readonly Color[][] Palettes;
    public readonly byte PortalFlag1;
    public readonly byte PortalFlag2;
    public readonly SpriteMapEntry[] SpriteMapEntries;
    public readonly WarpData[] Portals;
    
    public readonly Bitmap PalettesBitmap;

    public override string ToString()
    {
        return $"gravity:{Gravity} term_vel:{TerminalVelocity} _a:{SlideEffectId} balance:{BalanceLevel} _c:{C} _d:{D} _e:{E} _f:{F} _10:{_10} _11:{_11} portalFlags:{PortalFlag1} {PortalFlag2}";
    }
}