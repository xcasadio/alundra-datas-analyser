namespace AlundraEngine.Loader;

/// <summary>
/// The loader's UI element: one textured sprite sampled out of VRAM, or one flat-coloured quad.
///
/// GHIDRA: struct UIBox (36 bytes), LOADER.EXE.
/// SOURCE: Ghidra structure editor; every field below carries the annotation established there.
///
/// NOTE: this is NOT the same type as <see cref="UI.UIBoxConfiguration"/> in the game engine.
/// LOADER.EXE embeds its own copy of the UI layer and the two structures do not match, so this one
/// stays local to the loader.
/// </summary>
public sealed class UiBox
{
    /// <summary>Size of the original structure, which the menu code relies on for its stride.</summary>
    public const int StructSize = 36;

    // 0x00 / 0x02 - screen position before the offset is added.
    public short BaseX;
    public short BaseY;

    /// <summary>0x04 - ordering table slot. Negative means "do not draw".</summary>
    public short OtIndex = -1;

    /// <summary>0x06 - red of the flat quad path (POLY_F4 r0).</summary>
    public byte FlatColorR;

    /// <summary>0x07 - green of the flat quad path (POLY_F4 g0).</summary>
    public byte FlatColorG;

    /// <summary>
    /// 0x08 - dual use, proven by disassembly. In the sprite path it is a Z rotation fed to the
    /// GTE, with -1 meaning "no rotation, draw as a plain sprite". In the flat quad path its low
    /// byte is read as the blue channel. The high half is added to BaseX there but is always 0 in
    /// practice, so it is not modelled separately.
    /// </summary>
    public int RotationZ = -1;

    // 0x0C..0x0E - colour modulation of the sprite; 0x80 is neutral.
    public byte R = 0x80;
    public byte G = 0x80;
    public byte B = 0x80;

    /// <summary>0x0F - padding in the sprite path; part of the abr word in the flat quad path.</summary>
    public byte Pad0F;

    // 0x10 / 0x12 - added to BaseX / BaseY at draw time.
    public short OffsetX;
    public short OffsetY;

    /// <summary>0x14 - texture page colour depth: 0 = 4bpp, 1 = 8bpp, 2 = 16bpp.</summary>
    public short BppMode;

    /// <summary>0x16 - semi-transparency rate, or -1 for opaque.</summary>
    public short AbrOrMinus1 = -1;

    // 0x18 / 0x1A - packed VRAM source: the page origin and the offset inside it.
    public short PackedU;
    public short PackedV;

    // 0x1C / 0x1E - sprite size in pixels.
    public short Width;
    public short Height;

    // 0x20 / 0x22 - CLUT address.
    public short ClutXRaw;
    public short ClutY;

    /// <summary>GHIDRA: InitializeUiBox @ 0x80025d5c.</summary>
    public void Initialize(short bppMode, short abrOrMinus1, short packedU, short packedV, short width, short height, short clutXRaw, short clutY)
    {
        RotationZ = -1;
        OtIndex = -1;
        BaseY = -1;
        BaseX = -1;
        B = 0x80;
        G = 0x80;
        R = 0x80;
        BppMode = bppMode;
        AbrOrMinus1 = abrOrMinus1;
        PackedU = packedU;
        OffsetY = 0;
        OffsetX = 0;
        PackedV = packedV;
        ClutXRaw = clutXRaw;
        ClutY = clutY;
        Width = width;
        Height = height;
    }

    /// <summary>GHIDRA: SetUiBoxOffset @ 0x80025dc0.</summary>
    public void SetOffset(short offsetX, short offsetY)
    {
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    /// <summary>GHIDRA: SetUiBoxBaseAndRotation @ 0x80025dcc.</summary>
    public void SetBaseAndRotation(short x, short y, short otIndex, int rotationZ)
    {
        BaseX = x;
        BaseY = y;
        OtIndex = otIndex;
        RotationZ = rotationZ;
    }
}
