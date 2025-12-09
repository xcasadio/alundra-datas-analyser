using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AlundraEngine.DatasBin;

public class ScrollScreen // Scrollar
{
    public readonly int FactorXNum; // How many pixels to scroll horizontal against camera movement (negative = scroll with camera)
    public readonly int FactorXDenom;
    public readonly int FactorYNum; // How many pixels to scroll vertical against camera movement (negative = scroll with camera)
    public readonly int FactorYDenom;
    public readonly int ScrollXSpeed; // How many pixels to scroll horizontal per tick
    public readonly int ScrollXPeriod; // How many ticks to skip for horizontal scrolling
    public readonly int ScrollYSpeed; // How many pixels to scroll vertical per tick
    public readonly int ScrollYPeriod; // How many ticks to skip for vertical scrolling

    public ScrollScreen(ScrollParameters scrollParameters)
    {
        //FactorXNum = br.ReadInt32();
        //FactorXDenom = br.ReadInt32();
        //FactorYNum = br.ReadInt32();
        //FactorYDenom = br.ReadInt32();
        //ScrollXSpeed = br.ReadInt32();
        //ScrollXPeriod = br.ReadInt32();
        //ScrollYSpeed = br.ReadInt32();
        //ScrollYPeriod = br.ReadInt32();
    }

    public override string ToString()
    {
        return $"{FactorXDenom} {FactorXNum} {FactorYDenom} {FactorYNum} {ScrollXSpeed} {ScrollXPeriod} {ScrollYSpeed} {ScrollYPeriod}";
    }
}

public class ScrollParameters // LiningHeader
{
    public uint Graphics;
    public uint[] Layers;
    public uint ScriptTable;
    public uint Overlay;
    public uint OverlayExt;
    public uint WaveLUT;
    public Bitmap TileSheetBitmap;

    public ScrollParameters(BinaryReader br, int dataSize)
    {
        Graphics = br.ReadUInt32();
        Layers = new uint[2];
        Layers[0] = br.ReadUInt32();
        Layers[1] = br.ReadUInt32();
        ScriptTable = br.ReadUInt32();
        Overlay = br.ReadUInt32();
        OverlayExt = br.ReadUInt32();
        WaveLUT = br.ReadUInt32();

        //see int Lining::Init(Drawer* drawer)
        //read LiningInfos
        var liningInfos = new LiningInfos(br);

        if (liningInfos.Enabled == 0)
        {
            return;
        }

        var hasGraphics = 256 < dataSize - Graphics;

        if (hasGraphics)
        {
            //palette
            //var palette = new Color[16];
            //
            //for (var i = 0; i < palette.Length; i++)
            //{
            //    palette[i] = ImageHelper.FromPsxColor(br.ReadInt32());
            //}
            var maxPalettes = 16;
            var palettes = new Color[maxPalettes][];
            var buff = new byte[maxPalettes * 32];
            br.Read(buff, 0, buff.Length);
            var buffIndex = 0;

            for (var i = 0; i < maxPalettes; i++)
            {
                palettes[i] = new Color[16];
                for (var j = 0; j < 16; j++)
                {
                    var b2 = buff[buffIndex++];
                    var b1 = buff[buffIndex++];
                    palettes[i][j] = ImageHelper.FromPsxColor((b1 << 8) | b2);
                }
            }

            //img
            var buffer = br.ReadBytes(0x8000); //256 * 256 * 6 / 2);
            //var tileSheetImageData = new byte[0x80000];
            //ImageHelper.Deflate(buffer, tileSheetImageData);
            var tileSheetImageData = ImageHelper.Unzip(buffer);
            TileSheetBitmap = ImageHelper.BitmapFromPsxBuff(tileSheetImageData, 256, 256, 4, palettes[0]);
            //TileSheetBitmap.Save("c:\\image.bmp");
        }
    }

    //public override string ToString()
    //{
    //    return $"{FactorXDenom} {FactorXNum} {FactorYDenom} {FactorYNum} {ScrollXSpeed} {ScrollXPeriod} {ScrollYSpeed} {ScrollYPeriod}";
    //}
}

public class LiningInfos
{
    public byte Enabled;
    public byte AnimNum;
    public byte[] ModeLayer;
    public byte BGColorR;
    public byte BGColorB;
    public byte BGColorG;
    public byte BGColorA;

    public LiningInfos(BinaryReader br)
    {
        Enabled = br.ReadByte();
        AnimNum = br.ReadByte();
        ModeLayer = new byte[2];
        ModeLayer[0] = br.ReadByte();
        ModeLayer[1] = br.ReadByte();
        BGColorR = br.ReadByte();
        BGColorB = br.ReadByte();
        BGColorG = br.ReadByte();
        BGColorA = br.ReadByte();
    }
};