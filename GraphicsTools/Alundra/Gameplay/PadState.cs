namespace Alundra.Gameplay;

public class PadState {
    public uint MaxNbFrameHeld;
    public uint RepeatInterval;
    public uint IsOverThanMaxNbFrameHeld;
    public uint NumberOfFrameHold;
    public ushort ButtonsHold;
    public ushort ButtonsJustPressed;
    public ushort ButtonReleased;
    public ushort ButtonsJustPressedByInterval;
};