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
public class LoaderEngine(IRenderer renderer, IMovieAudioOutput? audioOutput = null, Sound.SoundManager? soundManager = null)
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
    /// How much wall-clock time one <see cref="MainLoop"/> call represents. The host calls it once
    /// per rendered frame and MonoGame's defaults (fixed time step, 60 Hz) are left in place, so
    /// this must stay in step with <c>AlundraGame</c>'s target elapsed time — it is what converts
    /// the engine's frame-based loop into the movie player's wall-clock pacing.
    /// </summary>
    private const double HostFrameSeconds = 1.0 / 60.0;

    /// <summary>
    /// GHIDRA: RunLoadingScreenIntro @ 0x80024fa8 — fade in over 0xFF/2 ticks, hold 500 ticks,
    /// fade out over 0xFF/2 ticks.
    /// </summary>
    private const int LoadingScreenHoldFrames = 500;

    /// <summary>
    /// Whether to skip the boot screen entirely.
    ///
    /// The original holds it for 500 ticks (about 12.6 s in total with the fades) to cover the CD
    /// access that follows. That is now drawn faithfully — the scene is
    /// <c>GetEtcResource(g_loadRoomBackgroundTimPtr, "TIM", 7)</c>, which resolves to
    /// g_licenceScreenTim, uploaded to VRAM (0x180, 0x100) and shown at (0x20, -8) under a
    /// subtractive full-screen quad that fades in and back out. An earlier pass guessed
    /// g_loadingScreenTim, which was the wrong image.
    /// </summary>
    // Deliberately a field, not a const: as a const the compiler folds the branch and reports the
    // other path as unreachable, which would hide it from any later change.
    private static readonly bool SkipBootLoadingScreen = false;

    /// <summary>
    /// GHIDRA: PromptNewGameOrContinue @ 0x80021c28 — the unselected entry sits at 0x40 and the
    /// selected one at 0xA0, around which FUN_80021a1c pulses it.
    /// </summary>
    private const byte MenuColorSelected = 0xA0;
    private const byte MenuColorUnselected = 0x40;

    /// <summary>GHIDRA: PromptNewGameOrContinue — INT_80042f6c += 0x58 per frame, wrapping at 0xFFF.</summary>
    private const int MenuPulseStep = 0x58;
    private const int MenuPulseWrap = 0xFFF;

    private LoaderState _state = LoaderState.LoadingScreen;
    private LoaderExeInspector? _inspector;
    private LoaderEtcStrings? _etcStrings;
    private string _moviePath = string.Empty;

    private LoaderUiRenderer? _ui;

    // GHIDRA: the title screen's UI elements, laid out by InitBootSequenceGraphics @ 0x800213c4.
    private readonly UiBox[] _titleTopBoxes = [new(), new(), new(), new(), new()];
    private readonly UiBox[] _menuOptionBoxes = [new(), new()];
    private readonly UiBox[] _titleFooterBoxes = [new(), new(), new(), new(), new()];
    private readonly UiBox _systemMessageBox = new();

    /// <summary>GHIDRA: INT_80042f6c — phase of the selected entry's brightness pulse.</summary>
    private int _menuPulsePhase;

    // The loader's own sound tables, read out of LOADER.EXE rather than SOUND.BIN's.
    private LoaderExeInspector.BgmTrack[] _bgmTracks = [];
    private Sound.SoundEffectRecord[] _sfxRecords = [];
    private short _sfxVabId = -1;

    /// <summary>
    /// GHIDRA: MainLoop @ 0x8002538c calls PromptNewGameOrContinue(0x708, 1) — the title screen's
    /// music is track 1.
    /// </summary>
    private const int TitleBgmTrack = 1;

    /// <summary>GHIDRA: PromptNewGameOrContinue @ 0x80021c28 — PlaySoundEffect(1) on Up and Down.</summary>
    private const int SfxCursorMove = 1;

    /// <summary>GHIDRA: PromptNewGameOrContinue @ 0x80021c28 — PlaySoundEffect(2) on confirm.</summary>
    private const int SfxConfirm = 2;

    /// <summary>True once the title BGM has been started for the current visit to the menu.</summary>
    private bool _titleBgmStarted;

    private StrMoviePlayer? _moviePlayer;
    private readonly MovieFrameBitmap _movieFrame = new();

    // Staging buffer for one MainLoop's worth of decoded PCM. A movie frame carries about
    // 1.25 XA sectors, so ~2520 stereo frames; this is comfortably above a burst of four.
    private readonly short[] _audioDrain = new short[1 << 16];

    private Bitmap? _titleScreen;

    /// <summary>GHIDRA: g_TileMapTileFrame0 — the boot screen's tile layer.</summary>
    private readonly LoaderTileMap _bootScreenLayer = new();

    /// <summary>GHIDRA: g_loadingScreenBackgroundBox @ RunLoadingScreenIntro 0x80024fa8.</summary>
    private readonly UiBox _bootScreenBox = new();

    /// <summary>GHIDRA: g_loadingScreenCursorBox — the subtractive quad that fades the screen.</summary>
    private readonly UiBox _bootScreenFadeQuad = new();

    /// <summary>GHIDRA: RunLoaderMainSequence @ 0x80024e28 and everything under it.</summary>
    private LoaderSelectionScreen? _selectionScreen;

    /// <summary>
    /// The save the player picked, or null when they started a new game.
    /// </summary>
    /// <remarks>
    /// GHIDRA: MainLoop @ 0x8002538c copies g_saveSlotRecords + index * 0x76C into g_saveDataInRam
    /// word by word, then sets SlotData = 1 and LastMapId = index before LoadExec hands over to
    /// ALUN_CD.EXE. Here the record is already a SaveData, so the copy is the reference itself.
    /// </remarks>
    public SaveData? SelectedSave { get; private set; }

    /// <summary>GHIDRA: g_saveDataInRam.SlotData — 0 for a new game, 1 for a loaded one.</summary>
    public int SelectedSlotData { get; private set; }

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
        var build = LoaderBuild.Detect(gamePath);
        if (build?.FindExeFilePath(gamePath) == null)
        {
            Debug.WriteLine($"No loader executable found in '{gamePath}'; the boot sequence will be skipped.");
            _state = LoaderState.Finished;
            return;
        }

        _inspector = new LoaderExeInspector(gamePath, build);
        _moviePath = Path.Combine(gamePath, "MOVIE");

        _titleScreen = _inspector.LoadImage(build.TitleFull);
        _etcStrings = LoaderEtcStrings.Load(gamePath);

        _ui = new LoaderUiRenderer(renderer);
        InitBootSequenceGraphics();
        InitSoundTables(gamePath);
        InitLoadingScreenGraphics();

        // GHIDRA: MainLoop @ 0x8002538c builds the selection screen before the title menu runs,
        // and it stays resident in VRAM alongside it - the two use disjoint rectangles.
        _selectionScreen = new LoaderSelectionScreen(
            _ui, _inspector, _etcStrings, PlaySoundEffect, PlayBgmTrack, StopTitleBgm);
        _selectionScreen.Initialize();

        // GHIDRA: RunLoadingScreenIntro @ 0x80024fa8 sets g_cursorFadeLevel = 0xFF before its first
        // tick, so the screen starts fully covered by the subtractive quad and fades up from there.
        _loadingScreenTick = 0;
        _loadingScreenFade = 0xFF;

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
                UpdateSlotSelection(buttons);
                break;

            case LoaderState.Finished:
                return GameState.InGame;

            default:
                throw new ArgumentOutOfRangeException();
        }

        renderer.Render();
        renderer.Clear();
        return GameState.MainMenu;
    }

    /// <summary>
    /// GHIDRA: InitBootSequenceGraphics @ 0x800213c4 — uploads the title screen to VRAM and lays
    /// out the UI elements that sample it.
    /// </summary>
    /// <remarks>
    /// PARTIAL: the original also builds eight tile layers from
    /// <c>GetEtcResource(g_loadRoomBackgroundTimPtr, "ANM", 1..8)</c>, an animation container held
    /// inside LOADER.EXE that is not parsed yet. FUN_80021a1c cycles those layers into VRAM at
    /// (0x180, 0) every eight frames, which is what animates the logo. Until that container is
    /// read, only the static g_TitleFull upload is done - see <see cref="TitleAnimationAvailable"/>.
    /// </remarks>
    private void InitBootSequenceGraphics()
    {
        if (_ui is null || _inspector is null)
        {
            return;
        }

        // InitializeTileLayer(&g_tileMapTitleFull, g_TitleFull) then
        // SetTileLayerBounds(&g_tileMapTitleFull, 0x180, 0, 0, 0x1e2, 0):
        // the 320x240 8bpp title screen lands at VRAM (0x180, 0) - 160 words wide - and its CLUT
        // at (0, 0x1e2).
        _ui.UploadTim(_inspector.ExeBytes, _inspector.GetImageFileOffset(_inspector.IndexOf(_inspector.Build.TitleFull)),
            destX: 0x180, destY: 0, clutDestX: 0, clutDestY: 0x1E2);

        // The five boxes covering the animated logo area: 64x160 each, side by side, sourced from
        // VRAM x 0x180 stepping 0x20 words (= 64 pixels at 8bpp).
        //
        // DELIBERATE INTERIM: the original gives these clutY 0x1E3, the palette of the ANM layers
        // that overwrite this VRAM region every eight frames. With those layers not yet loaded
        // that palette is empty, so they are pointed at the title screen's own CLUT instead, which
        // renders the static logo correctly. Restore 0x1E3 together with the ANM container.
        var topClutY = (short)(TitleAnimationAvailable ? 0x1E3 : 0x1E2);
        for (short i = 0; i < 5; i++)
        {
            var box = _titleTopBoxes[i];
            box.Initialize(1, -1, (short)(0x180 + i * 0x20), 0, 0x40, 0xA0, 0, topClutY);
            box.SetOffset((short)(i * 0x40), 0);
            box.SetBaseAndRotation(0, 0, 0, -1);
        }

        // The two menu entries, 128x16 each, from VRAM (0x1b0, 0xa0) and (0x1b0, 0xb0).
        for (short i = 0; i < 2; i++)
        {
            var box = _menuOptionBoxes[i];
            box.Initialize(1, -1, 0x1B0, (short)(0xA0 + i * 0x10), 0x80, 0x10, 0, 0x1E2);
            box.SetOffset(0x60, (short)(0x90 + i * 0x10));
            box.SetBaseAndRotation(0, 0, 1, -1);
        }

        // The copyright block: five 64x48 boxes along the bottom.
        for (short i = 0; i < 5; i++)
        {
            var box = _titleFooterBoxes[i];
            box.Initialize(1, -1, (short)(0x180 + i * 0x20), 0xC0, 0x40, 0x30, 0, 0x1E2);
            box.SetOffset((short)(i * 0x40), 0xC0);
            box.SetBaseAndRotation(0, 0, 0, -1);
        }

        // GHIDRA: InitBootSequenceGraphics builds one tile layer per "ANM" resource, in that order.
        // FUN_80021a1c then walks them by index, so the container's own order and count are what
        // matter - eight frames on France, nine on USA.
        _titleAnimationOffsets = [.. _inspector.ReadTitleAnimationFrames().Select(frame => frame.PayloadOffset)];
        _titleAnimationHold = 0;
        _titleAnimationLayer = 0;

        _systemMessageBox.Initialize(1, -1, 0x180, 0xA0, 0x20, 0x10, 0, 0x1E2);
        _systemMessageBox.SetOffset(0x11D, 0x1D);
        _systemMessageBox.SetBaseAndRotation(0, 0, 1, -1);

        ResetMenuColors();
    }

    /// <summary>
    /// GHIDRA: InitializeSoundDriver @ 0x80028338 — reads the loader's own BGM and sound-effect
    /// tables and opens the sound-effect VAB bank.
    /// </summary>
    private void InitSoundTables(string gamePath)
    {
        if (_inspector is null || soundManager is null)
        {
            return;
        }

        var soundBinPath = Path.Combine(gamePath, "DATA", "SOUND.BIN");
        if (!File.Exists(soundBinPath))
        {
            Debug.WriteLine($"SOUND.BIN not found at '{soundBinPath}'; the loader will run silently.");
            return;
        }

        var soundBinLength = (int)new FileInfo(soundBinPath).Length;
        _bgmTracks = _inspector.ReadBgmTrackTable(soundBinLength);
        _sfxRecords = _inspector.ReadSoundEffectTable();

        // GHIDRA: FUN_80028504 @ 0x80028504 — the global sound-effect VAB, which is the handle
        // PlaySoundEffect's direct branch uses. Not the group bank that FUN_80028650(0x25) loads.
        var sfxVab = _inspector.ReadSfxVab();
        _sfxVabId = soundManager.LoadLoaderSfxVab(sfxVab.HeaderOffset, sfxVab.BodyOffset, sfxVab.BodyEnd);
    }

    /// <summary>GHIDRA: PlayBgmTrack @ 0x80028dd8.</summary>
    private void PlayBgmTrack(int trackId)
    {
        if (soundManager is null || (uint)trackId >= (uint)_bgmTracks.Length)
        {
            return;
        }

        var track = _bgmTracks[trackId];
        soundManager.PlayLoaderBgm(track.SeqOffset, track.SeqEnd, track.VabBodyOffset, track.VabBodyEnd);
    }

    /// <summary>
    /// GHIDRA: PlaySoundEffect @ 0x80028b40.
    /// </summary>
    private void PlaySoundEffect(int sfxId)
    {
        if (soundManager is null || _sfxRecords.Length == 0 || _sfxVabId < 0)
        {
            return;
        }

        soundManager.PlayLoaderSoundEffect(_sfxRecords, sfxId, _sfxVabId);
    }

    /// <summary>
    /// False until the "ANM" animation container inside LOADER.EXE is parsed; see
    /// <see cref="InitBootSequenceGraphics"/>.
    /// </summary>
    // Field rather than const, for the same reason as SkipBootLoadingScreen above.
    private static readonly bool TitleAnimationAvailable = true;

    /// <summary>
    /// GHIDRA: the eight "ANM" resources InitBootSequenceGraphics @ 0x800213c4 turns into tile
    /// layers, in the order it asks for them. They are g_TitleFrame0..7 — the frames FUN_80021a1c
    /// cycles into VRAM to animate the logo.
    /// </summary>
    private int[] _titleAnimationOffsets = [];

    /// <summary>GHIDRA: INT_80042f68 — frames held on the current animation layer.</summary>
    private int _titleAnimationHold;

    /// <summary>GHIDRA: INT_80042f64 — index of the tile layer currently uploaded.</summary>
    private int _titleAnimationLayer;

    /// <summary>GHIDRA: PromptNewGameOrContinue @ 0x80021c28 sets both entries' base colours.</summary>
    private void ResetMenuColors()
    {
        for (var i = 0; i < _menuOptionBoxes.Length; i++)
        {
            var level = i == _titleMenuSelection ? MenuColorSelected : MenuColorUnselected;
            _menuOptionBoxes[i].R = level;
            _menuOptionBoxes[i].G = level;
            _menuOptionBoxes[i].B = level;
        }
    }

    /// <summary>
    /// GHIDRA: FUN_80021a1c @ 0x80021a1c — draws one frame of the title screen: the logo strip, the
    /// two menu entries with the selected one pulsing, the copyright block and the message box.
    /// </summary>
    private void DrawTitleScreen()
    {
        if (_ui is null)
        {
            return;
        }

        AdvanceTitleAnimation();

        _titleTopBoxes[0].SetBaseAndRotation(0, 0, 0, -1);
        _ui.RenderRun(_titleTopBoxes, 0, 5);

        // The selected entry's brightness is its base colour plus a cosine of the running phase,
        // divided by 64 (the original corrects the arithmetic shift so the division truncates
        // toward zero), clamped to a byte. It is restored right after drawing so the pulse never
        // accumulates.
        var selected = _menuOptionBoxes[_titleMenuSelection];
        var baseLevel = selected.R;

        var cosine = FixedCosine(_menuPulsePhase);
        if (cosine < 0)
        {
            cosine += 0x3F;
        }

        var pulsed = Math.Clamp(baseLevel + (cosine >> 6), 0, 0xFF);
        selected.R = (byte)pulsed;
        selected.G = (byte)pulsed;
        selected.B = (byte)pulsed;

        _menuOptionBoxes[0].SetBaseAndRotation(0, 0, 1, -1);
        _ui.RenderRun(_menuOptionBoxes, 0, 2);

        selected.R = baseLevel;
        selected.G = baseLevel;
        selected.B = baseLevel;

        _titleFooterBoxes[0].SetBaseAndRotation(0, 0, 0, -1);
        _ui.RenderRun(_titleFooterBoxes, 0, 5);

        _ui.Render(_systemMessageBox);
    }

    /// <summary>
    /// GHIDRA: FUN_80021a1c @ 0x80021a1c, opening block — holds the current animation layer for
    /// eight frames, then on the ninth uploads the next one and advances the index.
    /// </summary>
    /// <remarks>
    /// The index wraps from 8 back to <b>3</b>, not to 0: the first pass walks all eight frames as
    /// an opening flourish, then the animation settles into a five-frame loop over ANM 4 to 8.
    ///
    /// The destination is VRAM (0x180, 0) with CLUT (0, 0x1E3) — the same rectangle g_TitleFull
    /// occupies. The frames are 320x160 and g_TitleFull is 320x240, so each upload overwrites only
    /// the logo area and leaves the menu entries and the copyright block, which live below y=160,
    /// standing.
    /// </remarks>
    private void AdvanceTitleAnimation()
    {
        if (_ui is null || _inspector is null || _titleAnimationOffsets.Length == 0)
        {
            return;
        }

        if (_titleAnimationHold < 8)
        {
            _titleAnimationHold++;
            return;
        }

        _titleAnimationHold = 0;

        if (_titleAnimationLayer < _titleAnimationOffsets.Length)
        {
            _ui.UploadTim(_inspector.ExeBytes, _titleAnimationOffsets[_titleAnimationLayer],
                destX: 0x180, destY: 0, clutDestX: 0, clutDestY: 0x1E3);
        }

        _titleAnimationLayer++;
        if (_titleAnimationLayer == _titleAnimationOffsets.Length)
        {
            _titleAnimationLayer = _inspector.Build.TitleAnimationLoopStart;
        }
    }

    /// <summary>
    /// PsyQ <c>ccos</c>: cosine in 1.12 fixed point, with a full turn spanning 4096 units.
    /// </summary>
    /// <remarks>GHIDRA: ccos @ 0x80030910.</remarks>
    private static int FixedCosine(int angle) =>
        (int)Math.Round(Math.Cos(angle * 2.0 * Math.PI / 4096.0) * 4096.0);

    /// <summary>
    /// GHIDRA: RunLoadingScreenIntro @ 0x80024fa8, setup block.
    /// </summary>
    /// <remarks>
    /// CORRECTION: an earlier pass assumed the boot screen was g_loadingScreenTim. It is not — the
    /// original asks for <c>GetEtcResource(g_loadRoomBackgroundTimPtr, "TIM", 7)</c>, and the ETC
    /// container's seventh "TIM" is payload #6, g_licenceScreenTim (256x256, 4bpp). It goes to VRAM
    /// (0x180, 0x100) with its CLUT at (0, 0x1E5) and is shown at (0x20, -8), i.e. a 256x256 image
    /// centred horizontally on a 320-wide screen and pushed 8 pixels off the top.
    /// </remarks>
    private void InitLoadingScreenGraphics()
    {
        if (_ui is null || _inspector is null)
        {
            return;
        }

        var resource = _inspector.Build.BootScreen is { } bootScreen
            ? _inspector.FindEtcResource(bootScreen)
            : null;
        if (resource is not null)
        {
            _bootScreenLayer.InitializeTileLayer(_inspector.ExeBytes, resource.Value.PayloadOffset);
            _bootScreenLayer.SetTileLayerBounds(_ui.Vram, 0x180, 0x100, 0, 0x1E5, 0);
        }

        _bootScreenBox.Initialize(0, -1, 0x180, 0x100, 0x100, 0x100, 0, 0x1E5);
        _bootScreenBox.SetOffset(0, 0);
        _bootScreenBox.SetBaseAndRotation(0x20, -8, 0, -1);

        // GHIDRA: InitCursorObject(&g_loadingScreenCursorBox, 2, 0x140, 0xf0, 0xff, 0xff, 0xff) -
        // abr 2 is the GPU's subtractive rate, so the quad darkens the whole screen by its colour.
        _bootScreenFadeQuad.InitializeCursorObject(2, 0x140, 0xF0, 0xFF, 0xFF, 0xFF);
        _bootScreenFadeQuad.SetCursorColor(0, 0);
        _bootScreenFadeQuad.SetCursorPosition(0, 0, 200);

        _loadingScreenFade = 0xFF;
    }

    /// <summary>
    /// GHIDRA: RunLoadingScreenIntro @ 0x80024fa8 and TickLoadingScreenTransition @ 0x80024f4c —
    /// g_cursorFadeLevel runs 0xFF down to 0 in steps of 2, holds 500 ticks, then climbs back.
    /// </summary>
    private void UpdateLoadingScreen()
    {
        if (_ui is null)
        {
            BeginMovie(LoaderState.PlayMovieEuro);
            return;
        }

        _bootScreenFadeQuad.FlatColorR = (byte)_loadingScreenFade;
        _bootScreenFadeQuad.FlatColorG = (byte)_loadingScreenFade;
        _bootScreenFadeQuad.FlatColorB = (byte)_loadingScreenFade;

        _ui.Render(_bootScreenBox);
        _ui.RenderFlatQuad(_bootScreenFadeQuad);

        // The original's three loops in order: fade in while the level is above 0, hold 500 ticks,
        // then fade back out until the level is 0xFF again.
        const int fadeFrames = 0xFF / 2;
        _loadingScreenTick++;

        if (_loadingScreenTick <= fadeFrames)
        {
            _loadingScreenFade = Math.Max(0, _loadingScreenFade - 2);
        }
        else if (_loadingScreenTick > fadeFrames + LoadingScreenHoldFrames)
        {
            _loadingScreenFade = Math.Min(0xFF, _loadingScreenFade + 2);
        }

        if (_loadingScreenTick >= fadeFrames * 2 + LoadingScreenHoldFrames)
        {
            _loadingScreenFade = 0xFF;
            BeginMovie(LoaderState.PlayMovieEuro);
        }
    }

    /// <summary>
    /// GHIDRA: RunLoaderMainSequence @ 0x80024e28, plus the block of MainLoop @ 0x8002538c that
    /// copies the chosen record into g_saveDataInRam.
    /// </summary>
    private void UpdateSlotSelection(uint buttons)
    {
        if (_selectionScreen is null)
        {
            _state = LoaderState.TitleMenu;
            _titleMenuTimer = TitleMenuTimeoutFrames;
            return;
        }

        if (_selectionScreen.CurrentPhase == LoaderSelectionScreen.Phase.Idle)
        {
            _selectionScreen.Start();
        }

        if (!_selectionScreen.Update(buttons))
        {
            return;
        }

        var slot = _selectionScreen.Result;
        if (slot < 0)
        {
            // GHIDRA: MainLoop loops back to PromptNewGameOrContinue when the sequence returns -1.
            _state = LoaderState.TitleMenu;
            _titleMenuTimer = TitleMenuTimeoutFrames;
            _titleMenuSelection = 0;
            ResetMenuColors();
            _selectionScreen.Reset();
            return;
        }

        SelectedSave = _selectionScreen.ChosenSave;
        SelectedSlotData = 1;
        if (SelectedSave is not null)
        {
            SelectedSave.SlotData = 1;
            SelectedSave.LastMapId = (uint)slot;
        }

        _state = LoaderState.Finished;
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

        // The player converts host time into movie frames at the stream's own rate (15 fps), so at
        // 60 Hz it produces a new image exactly every fourth call; re-uploading the unchanged one
        // in between would just burn a full-frame colour-swizzled copy for nothing.
        var newFrame = _moviePlayer.Tick(HostFrameSeconds, buttons);
        PumpMovieAudio();

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
        // GHIDRA: PromptNewGameOrContinue @ 0x80021c28 advances the pulse phase every frame and
        // wraps it at 0xFFF, then calls FUN_80021a1c to draw.
        _menuPulsePhase += MenuPulseStep;
        if (_menuPulsePhase > MenuPulseWrap)
        {
            _menuPulsePhase = 0;
        }

        DrawTitleScreen();

        // GHIDRA: PromptNewGameOrContinue @ 0x80021c28 starts the BGM once on entry, before the
        // input loop.
        if (!_titleBgmStarted)
        {
            PlayBgmTrack(TitleBgmTrack);
            _titleBgmStarted = true;
        }

        if ((justPressed & ButtonUp) != 0)
        {
            _titleMenuSelection = 0;
            ResetMenuColors();
            PlaySoundEffect(SfxCursorMove);
        }

        if ((justPressed & ButtonDown) != 0)
        {
            _titleMenuSelection = 1;
            ResetMenuColors();
            PlaySoundEffect(SfxCursorMove);
        }

        if ((justPressed & (ButtonStart | ButtonCross)) != 0)
        {
            PlaySoundEffect(SfxConfirm);

            // GHIDRA: MainLoop @ 0x8002538c - result 0 starts a new game (SlotData = 0 then
            // LoadExec), result 1 goes through the save slot selection first.
            if (_titleMenuSelection == 0)
            {
                SelectedSave = null;
                SelectedSlotData = 0;
            }

            _state = _titleMenuSelection == 0 ? LoaderState.Finished : LoaderState.SlotSelection;
            StopTitleBgm();
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

    /// <summary>
    /// GHIDRA: PromptNewGameOrContinue @ 0x80021c28 ends on FUN_80028d90(1), which releases the
    /// sequence it opened; the next visit to the menu opens it again.
    /// </summary>
    private void StopTitleBgm()
    {
        if (_titleBgmStarted)
        {
            soundManager?.StopLoaderBgm();
            _titleBgmStarted = false;
        }
    }

    private void BeginMovie(LoaderState movieState)
    {
        StopTitleBgm();
        DisposeMoviePlayer();
        _state = movieState;

        var isEuro = movieState == LoaderState.PlayMovieEuro;
        var baseName = isEuro ? "EURO_OP" : "ARAN_OP";

        // A ".STR" alongside the ".MOV" is the raw 2352-byte-per-sector re-extraction, which is the
        // only form that carries complete XA audio: an extractor that writes a flat 2048 bytes per
        // sector truncates every Form 2 sector from 2324 bytes, losing 2 of its 18 ADPCM sound
        // groups. Prefer it when present, fall back to the ".MOV" (video only) otherwise.
        var fullPath = Path.Combine(_moviePath, baseName + ".STR");
        if (!File.Exists(fullPath))
        {
            fullPath = Path.Combine(_moviePath, baseName + ".MOV");
        }

        if (!File.Exists(fullPath))
        {
            Debug.WriteLine($"Movie '{baseName}' not found in '{_moviePath}'; skipping it.");
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
            return;
        }

        if (audioOutput is not null && _moviePlayer.HasAudio)
        {
            audioOutput.Volume = 1f;
            audioOutput.Start(_moviePlayer.AudioSampleRate, _moviePlayer.AudioChannels);
        }
        else if (!_moviePlayer.HasAudio)
        {
            Debug.WriteLine(
                $"'{Path.GetFileName(fullPath)}' carries no usable XA audio (2048-byte sectors). " +
                "Re-extract the MOVIE files from the CD image preserving 2352-byte sectors to get sound.");
        }
    }

    /// <summary>
    /// Drains the decoded PCM into the host's audio device and tracks the movie's CD volume ramp.
    /// </summary>
    /// <remarks>
    /// GHIDRA: FUN_80027f10 @ 0x80027f10 sets the SPU's CD input volume from <c>g_volume</c>
    /// (0..0x7FFF); PlayMovie ramps it down over the last frames and at a skip. The player keeps
    /// that value, so the desktop sink only has to follow it.
    /// </remarks>
    private void PumpMovieAudio()
    {
        if (audioOutput is null || _moviePlayer is null || !_moviePlayer.HasAudio)
        {
            return;
        }

        audioOutput.Volume = _moviePlayer.Volume / 32767f;

        int read;
        while ((read = _moviePlayer.ReadAudio(_audioDrain, 0, _audioDrain.Length)) > 0)
        {
            audioOutput.Submit(_audioDrain, 0, read);
        }
    }

    private void DisposeMoviePlayer()
    {
        audioOutput?.Stop();
        _moviePlayer?.Dispose();
        _moviePlayer = null;
    }
}
