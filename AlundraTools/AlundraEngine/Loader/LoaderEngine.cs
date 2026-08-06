using System.Diagnostics;
using AlundraEngine.Graphics;
using PsxSdk.Video;

namespace AlundraEngine.Loader;

/// <summary>
/// Boot sequence of LOADER.EXE: loading screen, publisher movie, title menu, intro movie.
///
/// GHIDRA: main @ 0x80025668, LaunchGame @ 0x800255f0, MainLoop @ 0x8002538c (LOADER.EXE).
///
/// JUSTIFICATION: PSX hardware adaptation only.
/// RELATION: the original is a tree of blocking loops, each paced by <c>VSync(0)</c> and ended by
/// a <c>return</c>. Here the host calls <see cref="MainLoop"/> once per rendered frame, so the
/// same sequence is expressed as a state machine; every counter, timeout and button test keeps its
/// original value.
/// </summary>
public class LoaderEngine(IRenderer renderer)
{
    // GHIDRA: PromptNewGameOrContinue @ 0x80021c28 reads these masks out of PadRead(0).
    // Bit values follow AlundraEngine.Gameplay.PadState.
    private const uint ButtonUp = 0x1000;
    private const uint ButtonDown = 0x4000;
    private const uint ButtonStart = 0x0800;
    private const uint ButtonCross = 0x0040;

    /// <summary>
    /// GHIDRA: LaunchGame @ 0x800255f0 passes 0x840 as <c>mode</c> on the first pass, restricting
    /// movie skipping to Start and Cross, then -1 (any button) on every pass after that.
    /// </summary>
    private const uint FirstPassSkipMask = ButtonStart | ButtonCross;

    /// <summary>GHIDRA: MainLoop @ 0x8002538c calls PromptNewGameOrContinue(0x708, ...).</summary>
    private const int TitleMenuTimeoutFrames = 0x708;

    /// <summary>
    /// GHIDRA: RunLoadingScreenIntro @ 0x80024fa8 — fade in over 0xFF/2 ticks, hold 500 ticks,
    /// fade out over 0xFF/2 ticks.
    /// </summary>
    private const int LoadingScreenHoldFrames = 500;

    /// <summary>
    /// DELIBERATE DEVIATION, default on.
    ///
    /// The original holds its boot screen for 500 ticks (about 12.6 s in total with the fades) to
    /// cover the CD access that follows — time that buys nothing on desktop, where the movie file
    /// opens instantly.
    ///
    /// It is also not yet possible to draw the right thing: RunLoadingScreenIntro shows the
    /// animated "load room" — GetEtcResource(g_loadRoomBackgroundTimPtr, "TIM", 7) composited over
    /// a scrolling tile layer (InitializeTileLayer @ 0x80026730) with a cursor sprite — none of
    /// which is transliterated yet. Showing g_loadingScreenTim instead would be the wrong image.
    ///
    /// Set to false to run the timing faithfully once Phase 3 provides the real scene.
    /// </summary>
    private const bool SkipBootLoadingScreen = true;

    private LoaderState _state = LoaderState.LoadingScreen;
    private LoaderExeInspector? _inspector;
    private string _moviePath = string.Empty;

    private StrMoviePlayer? _moviePlayer;
    private readonly MovieFrameBitmap _movieFrame = new();

    private Bitmap? _loadingScreen;
    private Bitmap? _titleScreen;

    /// <summary>
    /// GHIDRA: LaunchGame's <c>mode</c> parameter, widened to the skip mask it represents.
    /// </summary>
    private uint _skipButtonMask = FirstPassSkipMask;

    /// <summary>
    /// Which movie played last, so the title-menu timeout alternates EURO_OP and ARAN_OP the way
    /// LaunchGame's body does.
    /// </summary>
    private LoaderState _lastMoviePlayed = LoaderState.PlayMovieIntro;

    private int _loadingScreenTick;
    private int _loadingScreenFade;
    private int _titleMenuTimer;
    private int _titleMenuSelection;
    private uint _previousButtons;

    public void InitializeEngine(string gamePath)
    {
        if (!File.Exists(Path.Combine(gamePath, "LOADER.EXE")))
        {
            Debug.WriteLine($"LOADER.EXE not found in '{gamePath}'; the boot sequence will be skipped.");
            _state = LoaderState.Finished;
            return;
        }

        _inspector = new LoaderExeInspector(gamePath);
        _moviePath = Path.Combine(gamePath, "MOVIE");

        _loadingScreen = _inspector.LoadImage(LoaderExeInspector.LoadingScreenIndex);
        _titleScreen = _inspector.LoadImage(LoaderExeInspector.TitleFullIndex);

        _loadingScreenTick = 0;
        _loadingScreenFade = 0;

        if (SkipBootLoadingScreen)
        {
            BeginMovie(LoaderState.PlayMovieEuro);
            return;
        }

        _state = LoaderState.LoadingScreen;
    }

    public GameState MainLoop()
    {
        var buttons = (uint)PadManager.ButtonStates;
        var justPressed = buttons & ~_previousButtons;
        _previousButtons = buttons;

        switch (_state)
        {
            case LoaderState.LoadingScreen:
                UpdateLoadingScreen();
                break;

            case LoaderState.PlayMovieEuro:
            case LoaderState.PlayMovieIntro:
                UpdateMovie(buttons);
                break;

            case LoaderState.TitleMenu:
                UpdateTitleMenu(buttons, justPressed);
                break;

            case LoaderState.SlotSelection:
                // BLOCKED: RunLoaderMainSequence @ 0x80024e28 and the save-slot UI it drives
                // (InitSaveSlotSelectionUI @ 0x80023500, UpdateSelectionCursor @ 0x80024888, ...)
                // are not transliterated yet. Until they are, choosing Continue behaves like the
                // original does when the player backs out of slot selection: return to the title.
                _state = LoaderState.TitleMenu;
                _titleMenuTimer = TitleMenuTimeoutFrames;
                break;

            case LoaderState.Finished:
                return GameState.Game;

            default:
                throw new ArgumentOutOfRangeException();
        }

        renderer.Render();
        renderer.Clear();
        return GameState.MainMenu;
    }

    /// <summary>
    /// GHIDRA: RunLoadingScreenIntro @ 0x80024fa8. The original fades a cursor overlay in, holds
    /// for 500 ticks and fades back out; the fade level drives the tint of the loading screen.
    /// </summary>
    private void UpdateLoadingScreen()
    {
        if (_loadingScreen is not null)
        {
            var level = Math.Clamp(_loadingScreenFade, 0, 255) / 255f;
            renderer.AddSprite(0, 0, _loadingScreen.Width, _loadingScreen.Height,
                SpriteDepth.BackgroundUI, _loadingScreen, 1f, level, level, level);
        }

        _loadingScreenTick++;

        // g_cursorFadeLevel steps by 2 per tick in both directions (0xFF..0 then 0..0xFF).
        const int fadeFrames = 0xFF / 2;
        if (_loadingScreenTick <= fadeFrames)
        {
            _loadingScreenFade = Math.Min(255, _loadingScreenFade + 2);
        }
        else if (_loadingScreenTick > fadeFrames + LoadingScreenHoldFrames)
        {
            _loadingScreenFade = Math.Max(0, _loadingScreenFade - 2);
        }

        if (_loadingScreenTick >= fadeFrames * 2 + LoadingScreenHoldFrames)
        {
            BeginMovie(LoaderState.PlayMovieEuro);
        }
    }

    /// <summary>
    /// GHIDRA: PlayMovie @ 0x80027ff4. All the decoding lives in PsxSdk; this only supplies the
    /// per-movie parameters and draws the frame the player produced.
    /// </summary>
    private void UpdateMovie(uint buttons)
    {
        if (_moviePlayer is null)
        {
            AdvancePastMovie();
            return;
        }

        // One MainLoop call is one 60 Hz display frame, matching the original's VSync(0) pacing.
        // The player converts that into movie frames at the stream's own rate (~30 fps), so it
        // only produces a new image every other call - re-uploading the unchanged one in between
        // would just burn a full-frame colour-swizzled copy for nothing.
        var newFrame = _moviePlayer.Tick(1.0 / 60.0, buttons);

        if (newFrame && _moviePlayer.Width > 0 && _moviePlayer.FrameRgb24.Length > 0)
        {
            var updated = _movieFrame.Update(_moviePlayer.FrameRgb24, _moviePlayer.Width, _moviePlayer.Height);

            // The same Bitmap instance is rewritten every frame, so a backend that caches a GPU
            // copy keyed on that instance would keep showing the very first frame - which, both
            // movies fading in from black, means a black screen for the whole movie.
            renderer.InvalidateTexture(updated);
        }

        // The dimension check matters when moving from one movie to the next: EURO_OP is 320x160
        // and ARAN_OP is 304x224, so the previous movie's last frame must not be stretched over
        // the new one's rectangle during the frame before the first new image arrives.
        if (_movieFrame.Bitmap is { } frame &&
            frame.Width == _moviePlayer.Width &&
            frame.Height == _moviePlayer.Height)
        {
            renderer.AddSprite(
                _moviePlayer.ScreenX, _moviePlayer.ScreenY,
                _moviePlayer.Width, _moviePlayer.Height,
                SpriteDepth.BackgroundUI, frame);
        }

        if (_moviePlayer.IsFinished)
        {
            AdvancePastMovie();
        }
    }

    /// <summary>
    /// GHIDRA: PromptNewGameOrContinue @ 0x80021c28 — Up and Down move the selection, Start and
    /// Cross confirm, and after 0x708 frames without input the function returns -1, which sends
    /// LaunchGame on to the next movie.
    /// </summary>
    private void UpdateTitleMenu(uint buttons, uint justPressed)
    {
        if (_titleScreen is not null)
        {
            renderer.AddSprite(0, 0, _titleScreen.Width, _titleScreen.Height,
                SpriteDepth.BackgroundUI, _titleScreen);
        }

        // PARTIAL: the two option labels are baked into g_TitleFull, so they are already on
        // screen, but the original also highlights the selected one through its own UIBox layer
        // (FUN_80021a1c @ 0x80021a1c, which pulses the selected entry with
        // `colour + (ccos(phase) >> 6)` around 0xA0, the other staying at 0x40). That layer is not
        // transliterated yet, so the selection is tracked but not yet shown.

        if ((justPressed & ButtonUp) != 0)
        {
            _titleMenuSelection = 0;
        }

        if ((justPressed & ButtonDown) != 0)
        {
            _titleMenuSelection = 1;
        }

        if ((justPressed & (ButtonStart | ButtonCross)) != 0)
        {
            // GHIDRA: MainLoop @ 0x8002538c - result 0 starts a new game (SlotData = 0 then
            // LoadExec), result 1 goes through the save slot selection first.
            _state = _titleMenuSelection == 0 ? LoaderState.Finished : LoaderState.SlotSelection;
            return;
        }

        // GHIDRA: PromptNewGameOrContinue @ 0x80021c28 - `iVar6 = nbLoop; if (buttons == 0)
        // iVar6 = loopIndex;` then `loopIndex = iVar6 - 1`. Any button held reloads the timeout,
        // not just the ones the menu acts on.
        if (buttons != 0)
        {
            _titleMenuTimer = TitleMenuTimeoutFrames;
        }

        if (--_titleMenuTimer > 0)
        {
            return;
        }

        // Timed out. GHIDRA: LaunchGame @ 0x800255f0 alternates EURO_OP and ARAN_OP forever,
        // one movie per title-menu timeout.
        BeginMovie(_lastMoviePlayed == LoaderState.PlayMovieEuro
            ? LoaderState.PlayMovieIntro
            : LoaderState.PlayMovieEuro);
    }

    /// <summary>
    /// GHIDRA: LaunchGame @ 0x800255f0 — EURO_OP, title menu, ARAN_OP, title menu, then the whole
    /// sequence again with every button allowed to skip.
    /// </summary>
    private void AdvancePastMovie()
    {
        DisposeMoviePlayer();

        if (_state is LoaderState.PlayMovieEuro or LoaderState.PlayMovieIntro)
        {
            _lastMoviePlayed = _state;
        }

        if (_state == LoaderState.PlayMovieIntro)
        {
            // One full LaunchGame pass is done (EURO_OP, menu, ARAN_OP, menu). The next call comes
            // in with `mode = -1`, so from here on any button skips a movie.
            _skipButtonMask = uint.MaxValue;
        }

        _state = LoaderState.TitleMenu;
        _titleMenuTimer = TitleMenuTimeoutFrames;
        _titleMenuSelection = 0;
    }

    private void BeginMovie(LoaderState movieState)
    {
        DisposeMoviePlayer();
        _state = movieState;

        var isEuro = movieState == LoaderState.PlayMovieEuro;
        var fileName = isEuro ? "EURO_OP.MOV" : "ARAN_OP.MOV";
        var fullPath = Path.Combine(_moviePath, fileName);

        if (!File.Exists(fullPath))
        {
            Debug.WriteLine($"Movie '{fullPath}' not found; skipping it.");
            AdvancePastMovie();
            return;
        }

        // GHIDRA: LaunchGame @ 0x800255f0
        //   PlayMovie(0, 0x28, &g_EURO_OP_MOV_fileInfo, 0xaf4, mode, 0x136)
        //   PlayMovie(8, 0x10, &g_ARAN_OP_MOV_fileInfo, 0xeab, mode, 0)
        //
        // DELIBERATE DEVIATION: StopAtLastFrame is left false, so the 0xAF4 / 0xEAB frame limits
        // are not enforced and both movies play to their natural end. The originals stop 16 and 10
        // frames early respectively, cutting off the tail of each movie.
        var options = new MoviePlaybackOptions
        {
            ScreenX = isEuro ? 0 : 8,
            ScreenY = isEuro ? 0x28 : 0x10,
            LastFrame = isEuro ? 0xAF4 : 0xEAB,
            StopAtLastFrame = false,
            SkipButtonMask = _skipButtonMask,
            SkipAfterFrame = isEuro ? 0x136 : 0,
        };

        try
        {
            _moviePlayer = new StrMoviePlayer(fullPath, options);
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Could not open movie '{fullPath}': {exception.Message}");
            AdvancePastMovie();
        }
    }

    private void DisposeMoviePlayer()
    {
        _moviePlayer?.Dispose();
        _moviePlayer = null;
    }
}
