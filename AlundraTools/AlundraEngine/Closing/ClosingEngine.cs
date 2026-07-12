using AlundraEngine.Graphics;

namespace AlundraEngine.Closing;

public class ClosingEngine(IRenderer renderer)
{
    private Inspector _inspector;
    private CreditsPictureEntry[] g_creditsPictureTablePtr;
    private int g_creditsSequenceDone;
    private int g_creditsFadeSpeed;
    private int textBrightness;
    private int scene2Counter;
    private ClosingState _closingState = ClosingState.PlayMovie;

    public void InitializeEngine(string gamePath)
    {
        _inspector = new Inspector(gamePath);
        g_creditsPictureTablePtr = CreditsPictureEntry_ARRAY_8003a28c;
        g_creditsSequenceDone = 0;
        textBrightness = 0x80;
        g_creditsFadeSpeed = -2;
    }

    public GameState MainLoop()
    {
        var i = 0;
        uint buttons = 0;

        switch (_closingState)
        {
            case ClosingState.PlayMovie:
                _closingState = ClosingState.Scene1;
                break;

            case ClosingState.Scene1:
                UpdateAndDrawCreditsFade();
                UpdateCreditsTextSequencer();
                DrawCreditsTextQuad(0x80);
                //EndFrame(0);
                //buttons = PadRead(0);
                //
                //if ((buttons & 0x840) == 0)
                //{
                //    _closingState = ClosingState.Finished;
                //}

                if (g_creditsSequenceDone != 0)
                {
                    _closingState = ClosingState.Scene2;
                }
                break;

            case ClosingState.Scene2:
                UpdateAndDrawCreditsFade();
                //EndFrame(0);
                //buttons = PadRead(0);
                //if ((buttons & 0x840) == 0)
                //{
                //    scene2Counter++;
                //}
                //else
                //{
                //    _closingState = ClosingState.Scene3;
                //}

                if (scene2Counter == 0xfa)
                {
                    _closingState = ClosingState.Scene3;
                }
                break;

            case ClosingState.Scene3:
                break;

            case ClosingState.Finished:
                return GameState.MainMenu;
        }

        return GameState.EndScene;
    }

    private CreditsPictureEntry CreditsPictureEntry_ARRAY_8003a28c = new CreditsPictureEntry[20];
}