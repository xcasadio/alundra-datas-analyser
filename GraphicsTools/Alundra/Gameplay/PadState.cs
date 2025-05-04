namespace Alundra.Gameplay;

public class PadState
{
    const uint R2 = 0x0001;
    const uint L2 = 0x0002;
    const uint R1 = 0x0004;
    const uint L1 = 0x0008;
    const uint Triangle = 0x0010;
    const uint Circle = 0x0020;
    const uint Cross = 0x0040;
    const uint Square = 0x0080;
    const uint Select = 0x0100;
    //const uint unkwnown = 0x0200;
    //const uint unkwnown = 0x0400;
    const uint Start = 0x0800;
    const uint Up = 0x1000;
    const uint Right = 0x2000;
    const uint Down = 0x4000;
    const uint Left = 0x8000;

    public uint MaxNbFrameHeld;
    public uint RepeatInterval;
    public uint IsOverThanMaxNbFrameHeld;
    public uint NumberOfFrameHold;
    public ushort ButtonsHold;
    public ushort ButtonsJustPressed;
    public ushort ButtonReleased;
    public ushort ButtonsJustPressedByInterval;
};