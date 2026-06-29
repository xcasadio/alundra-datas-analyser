using AlundraEngine.Graphics;

namespace AlundraEngine.UI;

public class UIBoxConfiguration
{
    public short X;
    public short Y;
    public short Width;
    public short Height;
    public SPRT[] SpritesA;
    public SPRT[] SpritesB;

    public override string ToString()
    {
        return $"x:{X} y:{Y} w:{Width}  h:{Height}";
    }
}

public class TextToDisplay
{
    public int tick; //0x0
    public int speed; //0x4
    public int mode; //0x8
    public short x; //0xc
    public short y; //0xe
    public short startX; //0x10
    public short startY; //0x12
    public byte _14; //0x14		
    public byte _15; //0x15
    public byte	_16; //0x16
    public byte _17; //0x17
    public short originX; //0x18
    public short originY; //0x1a

    public override string ToString()
    {
        return $"t:{tick} s:{speed} m:{mode} x:{x} y:{y} sx:{startX} sy:{startY} ox:{originX} oy:{originY}";
    }
}

public class UIMemoryFileBox
{
    public int StartR;
    public int StartG;
    public int StartB;
    public int TargetR;
    public int TargetG;
    public int TargetB;
    public int R;
    public int G;
    public int B;
    public int Tick;
    public int Duration;
    public int Enabled;

    public override string ToString()
    {
        return $"Start:({StartR},{StartG},{StartB}) Target:({TargetR},{TargetG},{TargetB}) Current:({R},{G},{B}) Tick:{Tick} Duration:{Duration} Enabled:{Enabled}";
    }
}

public class MemoryCardUiBoxRecord
{
    public uint field_0x00;
    public UIBoxConfiguration field_0x04;
    public TextToDisplay field_0x08;
    public SPRT field_0x24;
    public SPRT field_0x4c;

    public MemoryCardUiBoxRecord(UIBoxConfiguration field_0x04, TextToDisplay field_0x08, SPRT field_0x24, SPRT field_0x4c)
    {
        this.field_0x04 = field_0x04;
        this.field_0x08 = field_0x08;
        this.field_0x24 = field_0x24;
        this.field_0x4c = field_0x4c;
    }
}

public class MemoryCardPointerRecord
{
    public string? field_0x0;
    public string? field_0x4;
}
    