namespace AlundraEngine.Gameplay;

public class PadState
{
    public const uint R2 = 0x0001;
    public const uint L2 = 0x0002;
    public const uint R1 = 0x0004;
    public const uint L1 = 0x0008;
    public const uint Triangle = 0x0010;
    public const uint Circle = 0x0020;
    public const uint Cross = 0x0040;
    public const uint Square = 0x0080;
    public const uint Select = 0x0100;
    //public const uint unkwnown = 0x0200;
    //public const uint unkwnown = 0x0400;
    public const uint Start = 0x0800;
    public const uint Up = 0x1000;
    public const uint Right = 0x2000;
    public const uint Down = 0x4000;
    public const uint Left = 0x8000;

    public uint MaxNbFrameHeld;
    public uint RepeatInterval;
    public uint IsOverThanMaxNbFrameHeld;
    public uint NumberOfFrameHold;
    public ushort ButtonsHold;
    public ushort ButtonsJustPressed;
    public ushort ButtonReleased;
    public ushort ButtonsJustPressedByInterval;
};