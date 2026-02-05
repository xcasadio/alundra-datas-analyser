using System;
using AlundraEngine.Graphics;

namespace AlundraEngine.DatasBin;

public class ScrollParameters // LiningHeader
{
    public uint Graphics;
    public uint[] Layers;
    public uint ScriptTable;
    public uint Overlay;
    public uint OverlayExt;
    public uint WaveLUT;
    public Bitmap TileSheetBitmap;

    private readonly long _memoryAddress;

    public ScrollParameters(BinaryReader br, int dataSize)
    {
        _memoryAddress = br.BaseStream.Position;

        Graphics = br.ReadUInt32();
        Layers = new uint[2];
        Layers[0] = br.ReadUInt32();
        Layers[1] = br.ReadUInt32();
        ScriptTable = br.ReadUInt32();
        Overlay = br.ReadUInt32();
        OverlayExt = br.ReadUInt32();
        WaveLUT = br.ReadUInt32();

        var liningInfos = new LiningInfos(br);

        if (liningInfos.Enabled == 0)
        {
            return;
        }

        var hasGraphics = 256 < dataSize - Graphics;

        if (hasGraphics)
        {
            //palette 256 bytes
            var maxPalettes = 8;
            var palettes = new Color[maxPalettes][];

            for (var i = 0; i < maxPalettes; i++)
            {
                palettes[i] = new Color[16];

                for (var j = 0; j < 16; j++)
                {
                    palettes[i][j] = ImageHelper.FromPsxColor(br.ReadInt16());
                }
            }

            //sprite sheet
            var buffer = br.ReadBytes(0x8000); //256 * 256 * 6 / 2);
            var tileSheetImageData = ImageHelper.Unzip(buffer);
            TileSheetBitmap = ImageHelper.BitmapFromPsxBuff(tileSheetImageData, 256, 256, 4, palettes[0]);

            for (var layerID = 0; layerID < 2; layerID++)
            {
                switch (Layers[layerID])
                {
                    case 1: InitScrollar(br, layerID); break;
                    case 2: InitCellular(br, layerID); break;
                    default: break;
                }
            }
        }
    }

    private void InitCellular(BinaryReader br, int layerId)
    {
        //Cellular* cellular = (Cellular*)(_data + _header->Layers[layerID] + 4);
        //Cell* cells = (Cell*)(cellular + sizeof(Cellular));
        //
        //std::fill_n(_cellPosX[layerID], CELL_MAX, 0);
        //std::fill_n(_cellPosY[layerID], CELL_MAX, 0);
        //std::fill_n(_cellTickX[layerID], CELL_MAX, 0);
        //std::fill_n(_cellTickY[layerID], CELL_MAX, 0);
        //
        //const int cellNum = cellular->Divisions;
        //
        //for (int i = 0; i < cellNum; ++i)
        //{
        //    _cellPosX[layerID][i] = cells[i].X0;
        //    _cellPosY[layerID][i] = cells[i].Y0;
        //}
    }

    private void InitScrollar(BinaryReader br, int layerId)
    {

        br.BaseStream.Position = _memoryAddress + Layers[layerId] + 4;
        var scrollar = new ScrollScreen(br);

        //_scrollDirX[layerID] = 0;
        //_scrollDirY[layerID] = 0;
        //
        //if (scrollar->ScrollXPeriod != 0)
        //    _scrollDirX[layerID] = ((scrollar->ScrollXSpeed >= 0) ^ (scrollar->ScrollXPeriod < 0)) ? +1 : -1;
        //
        //if (scrollar->ScrollYPeriod != 0)
        //    _scrollDirY[layerID] = ((scrollar->ScrollYSpeed >= 0) ^ (scrollar->ScrollYPeriod < 0)) ? +1 : -1;

    }
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
}

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

    public ScrollScreen(BinaryReader br)
    {
        FactorXNum = br.ReadInt32();
        FactorXDenom = br.ReadInt32();
        FactorYNum = br.ReadInt32();
        FactorYDenom = br.ReadInt32();
        ScrollXSpeed = br.ReadInt32();
        ScrollXPeriod = br.ReadInt32();
        ScrollYSpeed = br.ReadInt32();
        ScrollYPeriod = br.ReadInt32();
    }

    public override string ToString()
    {
        return $"{FactorXDenom} {FactorXNum} {FactorYDenom} {FactorYNum} {ScrollXSpeed} {ScrollXPeriod} {ScrollYSpeed} {ScrollYPeriod}";
    }
}

public class Cellular
{
    public byte CountBase;
    public byte AWaveY;
    public byte AWavePhase;
    public byte AWaveAmp;
    public byte BWaveY;
    public byte BWavePhase;
    public byte BWaveWeight;
    public byte Divisions;
}

public class Cell
{
    public byte PalDex;
    public byte U0;
    public byte V0;
    public byte U1;
    public byte V1;
    public byte Type;
    public short X0;
    public short Y0;
    public sbyte CamXNum;
    public sbyte CamXDen;
    public sbyte CamYNum;
    public sbyte CamYDen;
    public sbyte DX;
    public sbyte PeriodX;
    public sbyte DY;
    public sbyte PeriodY;
    public byte Unused0;
    public byte Unused1;
}
