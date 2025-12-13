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
        MemoryAddress = memoryAddress;

        MapId = br.ReadInt32();//0
        Gravity = br.ReadInt16();//4
        ZViscosity = br.ReadInt16();//6
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
        var buff = new byte[maxPalettes * 32];
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

        SpriteMapEntries = new SpriteMapEntry[6]; //14

        //InitializeMapSpriteTable 8002cc58
        int spriteIndex = 0;
        do
        {
            var numberOfFrame = br.ReadByte();
            var frameDuration = br.ReadByte();
        
            SpriteMapEntries[spriteIndex] = new SpriteMapEntry();
        
            if (numberOfFrame == 0 ||
                frameDuration == 0)
            {
                SpriteMapEntries[spriteIndex].Enabled = 0;
                SpriteMapEntries[spriteIndex].Index = 0;
            }
            else
            {
                SpriteMapEntries[spriteIndex].Enabled = 1;
                SpriteMapEntries[spriteIndex].NumberOfFrame = (byte)(1 << (numberOfFrame & 0x1f));
        
                if (SpriteMapEntries[spriteIndex].NumberOfFrame == 0)
                {
                    Debugger.Break();
                    //Trap(0x1c00);
                }
        
                SpriteMapEntries[spriteIndex].TileWidth = (byte)(0xa0 / SpriteMapEntries[spriteIndex].NumberOfFrame);
                SpriteMapEntries[spriteIndex].FrameIndex = 0;
                SpriteMapEntries[spriteIndex].Tick = 0;
                SpriteMapEntries[spriteIndex].Index = 0;
                SpriteMapEntries[spriteIndex].FrameDuration = frameDuration;
            }
        
            spriteIndex++;
        } while (spriteIndex < SpriteMapEntries.Length);

        //read portals
        var maxPortals = 64;
        Portals = new Portal[maxPortals];
        for (var i = 0; i < Portals.Length; i++)
        {
            Portals[i] = new Portal(br);
        }
    }

    public readonly int MemoryAddress;

    public readonly int MapId; //0
    public readonly short Gravity; //4
    public readonly short ZViscosity; //6
    public readonly byte SlideEffectId; // XYResistance
    public readonly byte BalanceLevel; // AnimDeleteWall
    public readonly byte C; 
    public readonly byte D;
    public readonly byte E;
    public readonly byte F;
    public readonly byte _10; // AnimLandFloor
    public readonly byte _11; // item something
    public readonly Color[][] Palettes;
    public readonly SpriteMapEntry[] SpriteMapEntries;
    public readonly Portal[] Portals;
    
    public readonly Bitmap PalettesBitmap;

    public override string ToString()
    {
        return $"gravity:{Gravity} term_vel:{ZViscosity} _a:{SlideEffectId} balance:{BalanceLevel} _c:{C} _d:{D} _e:{E} _f:{F} _10:{_10} _11:{_11}";
    }
}