using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using AlundraEngine.Graphics;

namespace AlundraEngine.DatasBin;

public class ScrollParameters // LiningHeader
{
    private const int LayerCount = 2;
    private const int PaletteCount = 8;
    private const int PaletteColorCount = 16;
    private const int TileSheetSize = 0x8000;
    private const int TileSheetWidth = 256;
    private const int TileSheetHeight = 256;
    private const int TileSheetStride = TileSheetWidth >> 1;
    private const int WaveLutEntryCount = 0x100;

    private readonly Dictionary<long, Bitmap> _scrollBitmapCache = new();

    public readonly byte[] Data;
    public readonly int DataSize;

    public uint Graphics;
    public uint[] Layers;
    public uint ScriptTable;
    public uint Overlay;
    public uint OverlayExt;
    public uint WaveLUT;

    public bool HasGraphics;
    public Bitmap? TileSheetBitmap;
    public Bitmap[] TileSheetBitmapsByPalette;
    public byte[] TileSheetImageData;
    public ushort[][] PaletteWords;
    public Color[][] Palettes;
    public int[] WaveLut;

    public LiningInfos Infos;
    public LayerInfos[] LayerInfos;
    public ScrollScreen[] Scrollars;
    public Cellular[] Cellulars;
    public Cell[][] Cells;

    public int[] AnimFrameTimer;
    public int[] AnimFrameCounter;
    public int[] ParallaxOffsetX;
    public int[] ParallaxOffsetY;
    public int[] OffsetX;
    public int[] OffsetY;
    public int[] TimerX;
    public int[] TimerY;
    public int[] ScrollDirX;
    public int[] ScrollDirY;

    public const int CellMax = 200;
    public int[][] CellPosX;
    public int[][] CellPosY;
    public int[][] CellTickX;
    public int[][] CellTickY;
    public byte[] WaveTick;

    public uint OvrOff;
    public ushort OvrTick;
    public byte OvrHold;

    public ScrollParameters(BinaryReader br, int dataSize)
    {
        Data = br.ReadBytes(dataSize);
        DataSize = Data.Length;

        Layers = new uint[LayerCount];
        TileSheetBitmapsByPalette = Array.Empty<Bitmap>();
        TileSheetImageData = Array.Empty<byte>();
        PaletteWords = Array.Empty<ushort[]>();
        Palettes = Array.Empty<Color[]>();
        WaveLut = Array.Empty<int>();

        Infos = new LiningInfos();
        LayerInfos = [new LayerInfos(), new LayerInfos()];
        Scrollars = [new ScrollScreen(), new ScrollScreen()];
        Cellulars = [new Cellular(), new Cellular()];
        Cells = [Array.Empty<Cell>(), Array.Empty<Cell>()];

        AnimFrameTimer = new int[LayerCount];
        AnimFrameCounter = new int[LayerCount];
        ParallaxOffsetX = new int[LayerCount];
        ParallaxOffsetY = new int[LayerCount];
        OffsetX = new int[LayerCount];
        OffsetY = new int[LayerCount];
        TimerX = new int[LayerCount];
        TimerY = new int[LayerCount];
        ScrollDirX = new int[LayerCount];
        ScrollDirY = new int[LayerCount];

        CellPosX = new int[LayerCount][];
        CellPosY = new int[LayerCount][];
        CellTickX = new int[LayerCount][];
        CellTickY = new int[LayerCount][];
        for (var layerId = 0; layerId < LayerCount; layerId++)
        {
            CellPosX[layerId] = new int[CellMax];
            CellPosY[layerId] = new int[CellMax];
            CellTickX[layerId] = new int[CellMax];
            CellTickY[layerId] = new int[CellMax];
        }

        WaveTick = new byte[LayerCount];

        if (DataSize < 0x24)
        {
            return;
        }

        using var dataStream = new MemoryStream(Data, writable: false);
        using var dataReader = new BinaryReader(dataStream);

        Graphics = dataReader.ReadUInt32();
        Layers[0] = dataReader.ReadUInt32();
        Layers[1] = dataReader.ReadUInt32();
        ScriptTable = dataReader.ReadUInt32();
        Overlay = dataReader.ReadUInt32();
        OverlayExt = dataReader.ReadUInt32();
        WaveLUT = dataReader.ReadUInt32();

        Infos = new LiningInfos(dataReader);

        if (Infos.Enabled == 0)
        {
            return;
        }

        HasGraphics = Graphics < DataSize && 256 < DataSize - Graphics;

        if (HasGraphics)
        {
            dataReader.BaseStream.Position = Graphics;

            PaletteWords = new ushort[PaletteCount][];
            Palettes = new Color[PaletteCount][];
            for (var paletteIndex = 0; paletteIndex < PaletteCount; paletteIndex++)
            {
                PaletteWords[paletteIndex] = new ushort[PaletteColorCount];
                Palettes[paletteIndex] = new Color[PaletteColorCount];

                for (var colorIndex = 0; colorIndex < PaletteColorCount; colorIndex++)
                {
                    var paletteWord = dataReader.ReadUInt16();
                    PaletteWords[paletteIndex][colorIndex] = paletteWord;
                    Palettes[paletteIndex][colorIndex] = ImageHelper.FromPsxColor(paletteWord);
                }
            }

            var sheetSize = Math.Min(TileSheetSize, Math.Max(0, DataSize - (int)Graphics - 256));
            var sheetBuffer = dataReader.ReadBytes(sheetSize);
            TileSheetImageData = ImageHelper.Unzip(sheetBuffer);

            if (TileSheetImageData.Length > 0)
            {
                TileSheetBitmapsByPalette = new Bitmap[Palettes.Length];
                for (var paletteIndex = 0; paletteIndex < Palettes.Length; paletteIndex++)
                {
                    TileSheetBitmapsByPalette[paletteIndex] = ImageHelper.BitmapFromPsxBuff(TileSheetImageData, TileSheetWidth, TileSheetHeight, 4, Palettes[paletteIndex]);
                }

                TileSheetBitmap = TileSheetBitmapsByPalette[0];
            }
        }

        if (WaveLUT != 0)
        {
            var waveLutOffset = (int)WaveLUT;
            if (waveLutOffset >= 0 && waveLutOffset + WaveLutEntryCount * sizeof(int) <= DataSize)
            {
                WaveLut = new int[WaveLutEntryCount];
                for (var i = 0; i < WaveLut.Length; i++)
                {
                    WaveLut[i] = BitConverter.ToInt32(Data, waveLutOffset + i * sizeof(int));
                }
            }
        }

        for (var layerId = 0; layerId < LayerCount; layerId++)
        {
            var layerOffset = (int)Layers[layerId];
            if (layerOffset < 0 || layerOffset + sizeof(byte) * 4 > DataSize)
            {
                continue;
            }

            using var layerStream = new MemoryStream(Data, layerOffset, DataSize - layerOffset, writable: false);
            using var layerReader = new BinaryReader(layerStream);
            LayerInfos[layerId] = new LayerInfos(layerReader);

            switch (Infos.ModeLayer[layerId])
            {
                case 1:
                    InitScrollar(layerId);
                    break;
                case 2:
                    InitCellular(layerId);
                    break;
            }
        }

        if (Infos.BGColorA != 0)
        {
            OvrOff = 0;
            OvrTick = 0;
            OvrHold = 0;
        }
    }

    private void InitCellular(int layerId)
    {
        var cellularOffset = (int)Layers[layerId] + 4;
        if (cellularOffset < 0 || cellularOffset + 8 > DataSize)
        {
            return;
        }

        using var cellularStream = new MemoryStream(Data, cellularOffset, DataSize - cellularOffset, writable: false);
        using var cellularReader = new BinaryReader(cellularStream);

        Cellulars[layerId] = new Cellular(cellularReader);

        var cellCount = Math.Min(CellMax, Cellulars[layerId].CountBase + Cellulars[layerId].Divisions);
        Cells[layerId] = new Cell[cellCount];

        for (var i = 0; i < cellCount; i++)
        {
            Cells[layerId][i] = new Cell(cellularReader);
        }

        var divisionCount = Math.Min(cellCount, Cellulars[layerId].Divisions);
        for (var i = 0; i < divisionCount; i++)
        {
            CellPosX[layerId][i] = Cells[layerId][i].X0;
            CellPosY[layerId][i] = Cells[layerId][i].Y0;
            CellTickX[layerId][i] = 0;
            CellTickY[layerId][i] = 0;
        }
    }

    private void InitScrollar(int layerId)
    {
        var scrollarOffset = (int)Layers[layerId] + 4;
        if (scrollarOffset < 0 || scrollarOffset + 8 > DataSize)
        {
            return;
        }

        using var scrollarStream = new MemoryStream(Data, scrollarOffset, DataSize - scrollarOffset, writable: false);
        using var scrollarReader = new BinaryReader(scrollarStream);
        Scrollars[layerId] = new ScrollScreen(scrollarReader);

        ScrollDirX[layerId] = 0;
        ScrollDirY[layerId] = 0;

        if (Scrollars[layerId].ScrollXPeriod != 0)
        {
            ScrollDirX[layerId] = ((Scrollars[layerId].ScrollXSpeed >= 0) ^ (Scrollars[layerId].ScrollXPeriod < 0)) ? +1 : -1;
        }

        if (Scrollars[layerId].ScrollYPeriod != 0)
        {
            ScrollDirY[layerId] = ((Scrollars[layerId].ScrollYSpeed >= 0) ^ (Scrollars[layerId].ScrollYPeriod < 0)) ? +1 : -1;
        }
    }

    // JUSTIFICATION: backend GDI renderer adaptation only
    public Bitmap? GetScrollBitmap(int paletteIndex, int u, int v, int width, int height, byte shaderBlendMode = 0, bool semiTransOnly = false)
    {
        if (width <= 0 || height <= 0 || TileSheetImageData.Length == 0 || Palettes.Length == 0 || PaletteWords.Length == 0)
        {
            return null;
        }

        if ((uint)paletteIndex >= (uint)Palettes.Length)
        {
            paletteIndex = 0;
        }

        u &= 0xFF;
        v &= 0xFF;
        var blendEnabled = shaderBlendMode is >= 1 and <= 4;
        if (!blendEnabled && semiTransOnly)
        {
            return null;
        }

        var bitmapKind = blendEnabled ? (semiTransOnly ? 2 : 1) : 0;

        var cacheKey = ((long)paletteIndex << 40)
                 | ((long)(u & 0xFF) << 32)
                 | ((long)(v & 0xFF) << 24)
                 | ((long)(width & 0xFF) << 16)
                 | ((long)(height & 0xFF) << 8)
                     | (long)bitmapKind;

        if (_scrollBitmapCache.TryGetValue(cacheKey, out var bitmap))
        {
            return bitmap;
        }

        var rowsize = (32 * width + 31) / 32 * 4;
        var pixels = new byte[rowsize * Math.Abs(height)];
        var palette = Palettes[paletteIndex];
        var paletteWords = PaletteWords[paletteIndex];
        var wrotePixel = false;

        for (var y = 0; y < height; y++)
        {
            var rowBase = y * rowsize;
            var sourceY = (v + y) & 0xFF;

            for (var x = 0; x < width; x++)
            {
                var sourceX = (u + x) & 0xFF;
                var sourceIndex = sourceY * TileSheetStride + (sourceX >> 1);
                if ((uint)sourceIndex >= (uint)TileSheetImageData.Length)
                {
                    continue;
                }

                var packed = TileSheetImageData[sourceIndex];
                var paletteDex = (sourceX & 1) == 0 ? (packed & 0x0F) : ((packed >> 4) & 0x0F);
                var paletteWord = paletteWords[paletteDex];
                var stp = (paletteWord & 0x8000) != 0;
                var isTransparentBlack = (paletteWord & 0x7FFF) == 0 && !stp;

                if (isTransparentBlack)
                {
                    continue;
                }

                if (blendEnabled && stp != semiTransOnly)
                {
                    continue;
                }

                var color = palette[paletteDex];
                var pixelBase = rowBase + x * 4;

                pixels[pixelBase + 0] = color.R;
                pixels[pixelBase + 1] = color.G;
                pixels[pixelBase + 2] = color.B;
                pixels[pixelBase + 3] = 255;
                wrotePixel = true;
            }
        }

        if (!wrotePixel)
        {
            return null;
        }

        bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length);
        bitmap.UnlockBits(bitmapData);

        _scrollBitmapCache[cacheKey] = bitmap;
        return bitmap;
    }
}

public enum CellType : byte
{
    Normal = 0,
    ScriptTrack = 1,
    FallRespawn = 2,
    WaveX = 4,
}

public class LiningInfos
{
    public byte Enabled;
    public byte AnimNum;
    public byte[] ModeLayer;
    public byte BGColorR;
    public byte BGColorG;
    public byte BGColorB;
    public byte BGColorA;

    public LiningInfos()
    {
        ModeLayer = new byte[2];
    }

    public LiningInfos(BinaryReader br)
        : this()
    {
        Enabled = br.ReadByte();
        AnimNum = br.ReadByte();
        ModeLayer[0] = br.ReadByte();
        ModeLayer[1] = br.ReadByte();
        BGColorR = br.ReadByte();
        BGColorG = br.ReadByte();
        BGColorB = br.ReadByte();
        BGColorA = br.ReadByte();
    }
}

public class LayerInfos
{
    public byte Unused;
    public byte AnimTimer;
    public byte BlendMode;
    public byte Ground;

    public LayerInfos()
    {
    }

    public LayerInfos(BinaryReader br)
    {
        Unused = br.ReadByte();
        AnimTimer = br.ReadByte();
        BlendMode = br.ReadByte();
        Ground = br.ReadByte();
    }
}

public class ScrollScreen // Scrollar
{
    public sbyte FactorXNum;
    public sbyte FactorXDenom;
    public sbyte FactorYNum;
    public sbyte FactorYDenom;
    public sbyte ScrollXSpeed;
    public sbyte ScrollXPeriod;
    public sbyte ScrollYSpeed;
    public sbyte ScrollYPeriod;

    public ScrollScreen()
    {
    }

    public ScrollScreen(BinaryReader br)
    {
        FactorXNum = br.ReadSByte();
        FactorXDenom = br.ReadSByte();
        FactorYNum = br.ReadSByte();
        FactorYDenom = br.ReadSByte();
        ScrollXSpeed = br.ReadSByte();
        ScrollXPeriod = br.ReadSByte();
        ScrollYSpeed = br.ReadSByte();
        ScrollYPeriod = br.ReadSByte();
    }

    public override string ToString()
    {
        return $"{FactorXNum} {FactorXDenom} {FactorYNum} {FactorYDenom} {ScrollXSpeed} {ScrollXPeriod} {ScrollYSpeed} {ScrollYPeriod}";
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

    public Cellular()
    {
    }

    public Cellular(BinaryReader br)
    {
        CountBase = br.ReadByte();
        AWaveY = br.ReadByte();
        AWavePhase = br.ReadByte();
        AWaveAmp = br.ReadByte();
        BWaveY = br.ReadByte();
        BWavePhase = br.ReadByte();
        BWaveWeight = br.ReadByte();
        Divisions = br.ReadByte();
    }
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

    public Cell()
    {
    }

    public Cell(BinaryReader br)
    {
        PalDex = br.ReadByte();
        U0 = br.ReadByte();
        V0 = br.ReadByte();
        U1 = br.ReadByte();
        V1 = br.ReadByte();
        Type = br.ReadByte();
        X0 = br.ReadInt16();
        Y0 = br.ReadInt16();
        CamXNum = br.ReadSByte();
        CamXDen = br.ReadSByte();
        CamYNum = br.ReadSByte();
        CamYDen = br.ReadSByte();
        DX = br.ReadSByte();
        PeriodX = br.ReadSByte();
        DY = br.ReadSByte();
        PeriodY = br.ReadSByte();
        Unused0 = br.ReadByte();
        Unused1 = br.ReadByte();
    }
}

public class Overlay
{
    public byte R;
    public byte G;
    public byte B;
    public byte Hold;

    public Overlay()
    {
    }

    public Overlay(BinaryReader br)
    {
        R = br.ReadByte();
        G = br.ReadByte();
        B = br.ReadByte();
        Hold = br.ReadByte();
    }
}

public class OverlayExt
{
    public byte R00;
    public byte G00;
    public byte B00;
    public byte R10;
    public byte G10;
    public byte B10;
    public byte R01;
    public byte G01;
    public byte B01;
    public byte R11;
    public byte G11;
    public byte B11;
    public byte Hold;
    public byte Pad0;
    public byte Pad1;
    public byte Pad2;

    public OverlayExt()
    {
    }

    public OverlayExt(BinaryReader br)
    {
        R00 = br.ReadByte();
        G00 = br.ReadByte();
        B00 = br.ReadByte();
        R10 = br.ReadByte();
        G10 = br.ReadByte();
        B10 = br.ReadByte();
        R01 = br.ReadByte();
        G01 = br.ReadByte();
        B01 = br.ReadByte();
        R11 = br.ReadByte();
        G11 = br.ReadByte();
        B11 = br.ReadByte();
        Hold = br.ReadByte();
        Pad0 = br.ReadByte();
        Pad1 = br.ReadByte();
        Pad2 = br.ReadByte();
    }
}
