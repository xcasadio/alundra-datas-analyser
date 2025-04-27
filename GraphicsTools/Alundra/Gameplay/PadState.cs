namespace Alundra.Gameplay;

public class PadState {
    uint maxNbFrameHeld;
    uint repeatInterval;
    uint isOverThanMaxNbFrameHeld;
    uint numberOfFrameHold;
    ushort buttonsHold;
    ushort buttonsJustPressed;
    ushort buttonReleased;
    ushort buttonsJustPressedByInterval;
};