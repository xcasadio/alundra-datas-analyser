using AlundraEngine.Gameplay;

namespace AlundraEngine;

public class PadManager
{
    public static ulong ButtonStates = 0;

    public void UpdatePads()
    {
        var padState = PadRead();
        UpdatePad(StaticVariables.g_padState1, (ushort)padState);
        UpdatePad(StaticVariables.g_padState2, (ushort)(padState >> 0x10));
    }

    private ulong PadRead()
    {
        return ButtonStates;
    }

    private void UpdatePad(PadState padState, ushort buttonState)
    {
        uint numberOfFrameHold;

        padState.ButtonsJustPressed = (ushort)(buttonState & (buttonState ^ padState.ButtonsHold));
        padState.ButtonsReleased = (ushort)(padState.ButtonsHold & (buttonState ^ padState.ButtonsHold));

        if (padState.ButtonsHold != buttonState || padState.ButtonsHold == 0)
        {
            padState.IsOverThanMaxNbFrameHeld = 0;
            padState.NumberOfFrameHold = 0;
            padState.ButtonsJustPressedByInterval = padState.ButtonsJustPressed;
            padState.ButtonsHold = buttonState;
            return;
        }

        if (padState.IsOverThanMaxNbFrameHeld == 0)
        {
            numberOfFrameHold = padState.NumberOfFrameHold;

            if (numberOfFrameHold < padState.MaxNbFrameHeld)
            {
                LAB_8002e2fc:
                padState.NumberOfFrameHold = numberOfFrameHold + 1;
                padState.ButtonsJustPressedByInterval = 0;
                padState.ButtonsHold = buttonState;
                return;
            }

            padState.IsOverThanMaxNbFrameHeld = 1;
        }
        else
        {
            numberOfFrameHold = padState.NumberOfFrameHold;

            if (numberOfFrameHold < padState.RepeatInterval)
            {
                padState.NumberOfFrameHold = numberOfFrameHold + 1;
                padState.ButtonsJustPressedByInterval = 0;
                padState.ButtonsHold = buttonState;
                return;
            }
        }

        padState.NumberOfFrameHold = 0;
        padState.ButtonsJustPressedByInterval = buttonState;
        padState.ButtonsHold = buttonState;
    }
}