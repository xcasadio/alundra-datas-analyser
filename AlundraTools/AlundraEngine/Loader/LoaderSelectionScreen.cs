using System.Text;

namespace AlundraEngine.Loader;

/// <summary>
/// A position that eases towards a target at a fixed step per axis.
///
/// GHIDRA: struct HudPositionLerp, with SetHudElement @ 0x8002202c,
/// HudLerpSetTargetPosition @ 0x80022154, HudLerpSetPosition @ 0x8002213c and
/// UpdateHubPositionLerp @ 0x80022054.
/// </summary>
public sealed class HudPositionLerp
{
    public int X;
    public int Y;
    public int StepX;
    public int StepY;
    public int TargetX;
    public int TargetY;

    /// <summary>GHIDRA: SetHudElement @ 0x8002202c.</summary>
    public void Set(int x, int y, int stepX, int stepY, int targetX, int targetY)
    {
        X = x;
        Y = y;
        StepX = stepX;
        StepY = stepY;
        TargetX = targetX;
        TargetY = targetY;
    }

    /// <summary>GHIDRA: HudLerpSetTargetPosition @ 0x80022154.</summary>
    public void SetTarget(int targetX, int targetY)
    {
        TargetX = targetX;
        TargetY = targetY;
    }

    /// <summary>GHIDRA: UpdateHubPositionLerp @ 0x80022054 — one step, clamped at the target.</summary>
    public void Update()
    {
        if (X < TargetX)
        {
            X += StepX;
            if (TargetX <= X)
            {
                X = TargetX;
            }
        }

        if (TargetX < X)
        {
            X -= StepX;
            if (X < TargetX)
            {
                X = TargetX;
            }
        }

        if (Y < TargetY)
        {
            Y += StepY;
            if (TargetY <= Y)
            {
                Y = TargetY;
            }
        }

        if (TargetY < Y)
        {
            Y -= StepY;
            if (Y < TargetY)
            {
                Y = TargetY;
            }
        }
    }
}

/// <summary>
/// The save-slot selection screen: a small map the player walks across, four houses standing for the
/// four memory-card records, and a message panel that types its text out.
///
/// GHIDRA: RunLoaderMainSequence @ 0x80024e28 and everything it drives —
/// InitSelectionBackdropTiles @ 0x80023d94, InitializeSelectionScreenGraphics @ 0x80022af4,
/// InitSaveSlotSelectionUI @ 0x80023500, InitSelectionMenu @ 0x8002471c,
/// ShowSelectionScreen @ 0x800247fc, UpdateSelectionCursor @ 0x80024888,
/// GetUserInput @ 0x80024984, ValidateSelection @ 0x80024b10, ProcessSelection @ 0x80024a54,
/// UpdateBackgroundState @ 0x800231c4, UpdateMenuGraphics @ 0x80023b14,
/// UpdateFadeState @ 0x800243d4 and FUN_800239c4 @ 0x800239c4.
///
/// JUSTIFICATION: PSX hardware adaptation only.
/// RELATION: the original is a stack of blocking loops, each iteration ended by
/// RunTransitionEffect / UpdateTransitionGraphics — the pair that clears the ordering table and then
/// hands it to the GPU. Here the host calls <see cref="Update"/> once per rendered frame, so the
/// same nesting is expressed as a phase machine; every counter, step and button mask keeps its
/// original value, and the per-frame body of each loop is reproduced call for call.
/// </summary>
public sealed class LoaderSelectionScreen
{
    // GHIDRA: PadRead(0) masks, PsyQ PADL*/PADR* values.
    private const uint ButtonUp = 0x1000;
    private const uint ButtonDown = 0x4000;
    private const uint ButtonLeft = 0x8000;
    private const uint ButtonRight = 0x2000;
    private const uint ButtonDirections = 0xF000;
    private const uint ButtonCircle = 0x0020;
    private const uint ButtonCross = 0x0040;

    /// <summary>GHIDRA: ShowSelectionScreen @ 0x800247fc plays BGM track 0x29.</summary>
    private const int SelectionBgmTrack = 0x29;

    /// <summary>The phases the original's nested loops become.</summary>
    public enum Phase
    {
        /// <summary>Not running.</summary>
        Idle,

        /// <summary>InitSelectionMenu: the card message types out, then waits on Circle.</summary>
        CardMessage,

        /// <summary>InitSelectionMenu: 0x3C frames with the panel retracting.</summary>
        CardMessageHold,

        /// <summary>ShowSelectionScreen: the screen fades up from black, 8 levels a frame.</summary>
        FadeIn,

        /// <summary>UpdateSelectionCursor: the "walk to a house" prompt, waits on Cross.</summary>
        Prompt,

        /// <summary>UpdateSelectionCursor: 0x3C frames with the panel retracting.</summary>
        PromptHold,

        /// <summary>GetUserInput: walking around until a hotspot answers.</summary>
        Walking,

        /// <summary>ValidateSelection: the confirmation panel, waits on Cross.</summary>
        Confirm,

        /// <summary>ProcessSelection: the screen fades out, 3 levels a frame.</summary>
        FadeOut,

        /// <summary>Two blank frames, then the result is available.</summary>
        Settle,

        /// <summary>Done; <see cref="Result"/> holds the slot index, or -1.</summary>
        Finished,
    }

    private readonly LoaderUiRenderer _ui;
    private readonly LoaderExeInspector _inspector;
    private readonly LoaderEtcStrings? _strings;
    private readonly Action<int> _playSoundEffect;
    private readonly Action<int> _playBgmTrack;
    private readonly Action _stopBgm;

    // ---- tile layers ---------------------------------------------------------------------------
    // GHIDRA: TileMap_8014f030 (TIM 1, the map), TileMap_8014ed40 (TIM 2, the clouds),
    // TileMap_8014ed10 (TIM 5, the sprite sheet), g_transitionGraphics (TIM 3, the panel frames).
    private readonly LoaderTileMap _mapLayer = new();
    private readonly LoaderTileMap _cloudLayer = new();
    private readonly LoaderTileMap _spriteSheetLayer = new();
    private readonly LoaderTileMap _panelLayer = new();

    // GHIDRA: g_tileMap1/2/3 over g_tileBuffer1/2/3 — the three dynamic text surfaces.
    private readonly LoaderTileMap _tileMap1 = new();
    private readonly LoaderTileMap _tileMap2 = new();
    private readonly LoaderTileMap _tileMap3 = new();

    private readonly LoaderFont _font = new();

    // ---- UI elements ---------------------------------------------------------------------------
    /// <summary>GHIDRA: g_uiBox0[0..5] — the three message panels, two boxes each.</summary>
    private readonly UiBox[] _panelBoxes = [new(), new(), new(), new(), new(), new()];

    /// <summary>GHIDRA: g_textBox1 / g_textBox2 / g_textBox3.</summary>
    private readonly UiBox _textBox1 = new();

    private readonly UiBox _textBox2 = new();
    private readonly UiBox _textBox3 = new();

    /// <summary>GHIDRA: g_saveSlotBox — the animated pointer, and the yes/no cursor.</summary>
    private readonly UiBox _saveSlotBox = new();

    /// <summary>GHIDRA: UIBox_ARRAY_8014f490 — the map, five 64x240 columns.</summary>
    private readonly UiBox[] _mapBoxes = [new(), new(), new(), new(), new()];

    /// <summary>GHIDRA: UIBox_ARRAY_8014f6b8 — the map's foreground strip, drawn over the walker.</summary>
    private readonly UiBox[] _mapForegroundBoxes = [new(), new(), new(), new(), new()];

    /// <summary>GHIDRA: UIBox_ARRAY_8014f3d8 — the scrolling clouds, drawn twice for the wrap.</summary>
    private readonly UiBox[] _cloudBoxes = [new(), new(), new(), new(), new()];

    /// <summary>GHIDRA: UIBox_ARRAY_8014f390 — the walking sprite and its shadow.</summary>
    private readonly UiBox[] _walkerBoxes = [new(), new()];

    /// <summary>GHIDRA: UIBox_ARRAY_8014f548 — four ornaments, three of them spinning.</summary>
    private readonly UiBox[] _ornamentBoxes = [new(), new(), new(), new()];

    /// <summary>GHIDRA: UIBox_8014f2e8.packedU and the three boxes after it — one marker per slot.</summary>
    private readonly UiBox[] _slotMarkerBoxes = [new(), new(), new(), new()];

    /// <summary>GHIDRA: UIBox_8014f2e8 — the full-screen fade quad.</summary>
    private readonly UiBox _fadeQuad = new();

    // ---- text layers ---------------------------------------------------------------------------
    // GHIDRA: g_uiSlot1 (@0x8014f198), g_uiSlot1 + 0x20 and g_uiSlot3 — three 32-byte descriptors.
    private LoaderTextLayer _textLayer1 = null!;
    private LoaderTextLayer _textLayer2 = null!;
    private LoaderTextLayer _textLayer3 = null!;

    // ---- HUD lerps -----------------------------------------------------------------------------
    private readonly HudPositionLerp _hudTopLeft = new();
    private readonly HudPositionLerp _hudTopRight = new();
    private readonly HudPositionLerp _hudTitle = new();

    // ---- scalar state --------------------------------------------------------------------------
    /// <summary>
    /// GHIDRA: g_selectedSlot.
    /// CORRECTION: the name is wrong — this is the screen's fade level, not a slot index. Positive
    /// values drive an additive white quad, negative ones a subtractive black quad.
    /// </summary>
    private int _fadeLevel;

    /// <summary>GHIDRA: g_loaderState1 / g_loaderState2 — the cloud scroll, one pixel every four frames.</summary>
    private int _cloudTick;

    private int _cloudScroll;

    /// <summary>GHIDRA: g_cameraStartX / g_cameraStartY — the walker's position, 20.12 fixed point.</summary>
    private int _cameraX;

    private int _cameraY;

    /// <summary>
    /// GHIDRA: g_transitionState.
    /// CORRECTION: it is the walker's idle flag — 1 while no direction is held, which is also what
    /// stops FUN_800239c4 from moving the camera.
    /// </summary>
    private int _isIdle;

    /// <summary>
    /// GHIDRA: g_fadeInProgress.
    /// CORRECTION: it is the walker's facing — 0 down, 1 up, 2 left, 3 right — and selects both the
    /// sprite row and the direction FUN_800239c4 steps in.
    /// </summary>
    private int _facing;

    /// <summary>GHIDRA: g_fadeFrame — free-running, its bits 3..4 pick the walk frame.</summary>
    private int _walkFrame;

    /// <summary>
    /// GHIDRA: g_fadeType.
    /// CORRECTION: it is the hotspot code the walker last bumped into, -1 for none.
    /// </summary>
    private int _hotspot = -1;

    /// <summary>
    /// GHIDRA: g_isTransitionRunning — which way the message panels are parked:
    /// 0 both off-screen, 1 the left panel out, 2 the left panel out and the right one down.
    /// </summary>
    private int _panelMode;

    /// <summary>GHIDRA: DAT_8014f1f8 / DAT_8014f1fc — the pointer's four-frame animation.</summary>
    private int _pointerFrame;

    private int _pointerTick;

    /// <summary>GHIDRA: INT_8015296c / DAT_80156970 / DAT_80156974 — the ornaments' phases.</summary>
    private int _ornamentTick;

    private int _ornamentPhase2;
    private int _ornamentPhase3;

    /// <summary>
    /// GHIDRA: g_slotError4, g_slotError3, g_slotError2, g_slotError1 — declared backwards, so
    /// index 0 here is g_slotError4 and stands for slot 0. Null means "no save, draw nothing".
    /// </summary>
    private readonly byte[]?[] _slotMarkerText = new byte[4][];

    private readonly int[] _slotMarkerPosition = new int[4];

    // ---- selection state -----------------------------------------------------------------------
    private readonly LoaderSaveSlotRecord[] _records = new LoaderSaveSlotRecord[LoaderSaveSlots.SlotCount];
    private LoaderExeInspector.SelectionHotspot[] _hotspots = [];
    private byte[] _markerRestingString = [];
    private byte[] _markerAnimationString = [];
    private int _cardReadResult;
    private int _holdCounter;
    private int _confirmSlot;
    private int _confirmYesNo;
    private int _confirmDelay;
    private bool _confirmBlipPending;
    private uint _confirmButtons;

    /// <summary>The phase the screen is in.</summary>
    public Phase CurrentPhase { get; private set; } = Phase.Idle;

    /// <summary>The chosen slot index once <see cref="CurrentPhase"/> is Finished, or -1.</summary>
    public int Result { get; private set; } = -1;

    /// <summary>The chosen record, or null when the player backed out.</summary>
    public SaveData? ChosenSave => Result >= 0 && Result < _records.Length ? _records[Result]?.Data : null;

    public LoaderSelectionScreen(
        LoaderUiRenderer ui,
        LoaderExeInspector inspector,
        LoaderEtcStrings? strings,
        Action<int> playSoundEffect,
        Action<int> playBgmTrack,
        Action stopBgm)
    {
        _ui = ui;
        _inspector = inspector;
        _strings = strings;
        _playSoundEffect = playSoundEffect;
        _playBgmTrack = playBgmTrack;
        _stopBgm = stopBgm;
    }

    /// <summary>
    /// GHIDRA: MainLoop @ 0x8002538c builds all three screens once, before the title menu ever
    /// runs: InitBootSequenceGraphics, InitSelectionBackdropTiles, InitializeSelectionScreenGraphics
    /// then InitSaveSlotSelectionUI.
    /// </summary>
    public void Initialize()
    {
        _hotspots = _inspector.ReadSelectionHotspots();
        _markerRestingString = _inspector.ReadSlotMarkerRestingString();
        _markerAnimationString = _inspector.ReadSlotMarkerAnimationString();

        InitSelectionBackdropTiles();
        InitializeSelectionScreenGraphics();
        InitSaveSlotSelectionUI();
    }

    /// <summary>GHIDRA: InitSelectionBackdropTiles @ 0x80023d94.</summary>
    private void InitSelectionBackdropTiles()
    {
        var map = _inspector.FindEtcResource("TIM", 1);
        var clouds = _inspector.FindEtcResource("TIM", 2);

        if (map is not null)
        {
            _mapLayer.InitializeTileLayer(_inspector.ExeBytes, map.Value.PayloadOffset);
        }

        if (clouds is not null)
        {
            _cloudLayer.InitializeTileLayer(_inspector.ExeBytes, clouds.Value.PayloadOffset);
        }

        _mapLayer.SetTileLayerBounds(_ui.Vram, 0x300, 0, 0, 0x1E0, 0);
        _cloudLayer.SetTileLayerBounds(_ui.Vram, 0x300, 0x180, 0x100, 0x1E0, 0);

        // Five 64x240 columns of the map, 8bpp, side by side across the screen.
        for (short i = 0; i < 5; i++)
        {
            var box = _mapBoxes[i];
            box.Initialize(1, -1, (short)(0x300 + i * 0x20), 0, 0x40, 0xF0, 0, 0x1E0);
            box.SetOffset((short)(i * 0x40), 0);
            box.SetBaseAndRotation((short)(i * 0x40), 0, 0, -1);
        }

        // The strip that must cover the walker, from the map's lower half, drawn at ordering slot 9.
        for (short i = 0; i < 5; i++)
        {
            var box = _mapForegroundBoxes[i];
            box.Initialize(1, -1, (short)(0x300 + i * 0x20), 0x100, 0x40, 0x80, 0, 0x1E0);
            box.SetOffset((short)(i * 0x40), 0x2A);
            box.SetBaseAndRotation((short)(i * 0x40), 0, 9, -1);
        }

        // The clouds: 4bpp, semi-transparent (abr 0 = half and half), stepping 0x10 words = 64 px.
        for (short i = 0; i < 5; i++)
        {
            var box = _cloudBoxes[i];
            box.Initialize(0, 0, (short)(0x300 + i * 0x10), 0x180, 0x40, 0x80, 0x100, 0x1E0);
            box.SetOffset((short)(i * 0x40), 0x78);
            box.SetBaseAndRotation((short)(i * 0x40), 0, 1, -1);
        }

        _fadeQuad.InitializeCursorObject(1, 0x140, 0xF0, 0xFF, 0xFF, 0xFF);
        _fadeQuad.SetCursorColor(0, 0);
        _fadeQuad.SetCursorPosition(0, 0, 200);
    }

    /// <summary>GHIDRA: InitializeSelectionScreenGraphics @ 0x80022af4.</summary>
    private void InitializeSelectionScreenGraphics()
    {
        // The fallback used to be image #3, which is the font only on the France build - on the USA
        // one that index is the background message. "TIM" #4 resolves on both, so a build without it
        // is a build this port does not understand, and guessing an index would draw the wrong sheet.
        var fontTim = _inspector.FindEtcResource("TIM", 4);
        if (fontTim is not null)
        {
            _font.InitFontTileMap(
                _ui.Vram,
                _inspector.ExeBytes,
                fontTim.Value.PayloadOffset,
                _inspector.ReadFontCharacterTable(),
                _inspector.Build.GlyphAdvancePadding);
        }

        var panel = _inspector.FindEtcResource("TIM", 3);
        if (panel is not null)
        {
            _panelLayer.InitializeTileLayer(_inspector.ExeBytes, panel.Value.PayloadOffset);
        }

        _panelLayer.SetTileLayerBounds(_ui.Vram, 0x3C0, 0, 0x110, 0x1E0, 0);

        _tileMap1.InitializeTileMap(0, 0x100, 0x30, new byte[0x100 / 2 * 0x30]);
        _tileMap2.InitializeTileMap(0, 0x100, 0x20, new byte[0x100 / 2 * 0x20]);
        _tileMap3.InitializeTileMap(0, 0x60, 0x10, new byte[0x60 / 2 * 0x10]);

        // The original clears a 0x100 x 0x100 square on each, well past every layer's real size;
        // the bounds test inside SetTileMapPixel is what makes that harmless.
        for (var y = 0; y < 0x100; y++)
        {
            for (var x = 0; x < 0x100; x++)
            {
                _tileMap1.SetTileMapPixel(x, y, 0);
                _tileMap2.SetTileMapPixel(x, y, 0);
                _tileMap3.SetTileMapPixel(x, y, 0);
            }
        }

        _tileMap1.SetTileLayerBounds(_ui.Vram, 0x2C0, 0x180, -1, -1, 0);
        _tileMap2.SetTileLayerBounds(_ui.Vram, 0x2C0, 0x1B0, -1, -1, 0);
        _tileMap3.SetTileLayerBounds(_ui.Vram, 0x2C0, 0x1D0, -1, -1, 0);

        // Three panels of two boxes: 0xA0x0x40 twice for the first two, 0x40x0x20 twice for the
        // title. All 4bpp out of the panel TIM at (0x3C0, 0), CLUT (0x110, 0x1E0).
        for (short i = 0; i < 4; i++)
        {
            var box = _panelBoxes[i];
            box.Initialize(0, -1, 0x3C0, (short)(i * 0x40), 0xA0, 0x40, 0x110, 0x1E0);
            box.SetOffset((short)((i & 1) * 0xA0), 0);
            box.SetBaseAndRotation(0, 0, 100, -1);
        }

        for (short i = 0; i < 2; i++)
        {
            var box = _panelBoxes[4 + i];
            box.Initialize(0, -1, 0x3F0, (short)(i * 0x20), 0x40, 0x20, 0x110, 0x1E0);
            box.SetOffset((short)(i * 0x40), 0);
            box.SetBaseAndRotation(0, 0, 100, -1);
        }

        // The three text boxes sample exactly the rectangles the tile maps upload to.
        _textBox1.Initialize(0, -1, 0x2C0, 0x180, 0x100, 0x30, 0x100, 0x1E1);
        _textBox1.SetOffset(0x20, 8);
        _textBox1.SetBaseAndRotation(0, 0, 100, -1);

        _textBox2.Initialize(0, -1, 0x2C0, 0x1B0, 0x100, 0x20, 0x100, 0x1E1);
        _textBox2.SetOffset(0x20, 0x10);
        _textBox2.SetBaseAndRotation(0, 0, 100, -1);

        _textBox3.Initialize(0, -1, 0x2C0, 0x1D0, 0x60, 0x10, 0x100, 0x1E1);
        _textBox3.SetOffset(0x10, 8);
        _textBox3.SetBaseAndRotation(0, 0, 100, -1);

        _textLayer1 = new LoaderTextLayer(_font, _ui.Vram, _playSoundEffect);
        _textLayer2 = new LoaderTextLayer(_font, _ui.Vram, _playSoundEffect);
        _textLayer3 = new LoaderTextLayer(_font, _ui.Vram, _playSoundEffect);

        _textLayer1.SetTextLayer(_tileMap1, _textBox1);
        _textLayer2.SetTextLayer(_tileMap2, _textBox2);
        _textLayer3.SetTextLayer(_tileMap3, _textBox3);

        // GHIDRA: the two labels of the confirmation prompt, drawn side by side 0x30 apart into the
        // third tile map. Entries 0xCA and 0xCB of ETC_RES.R on the France build; on the USA build
        // FUN_80022d7c passes the executable's own "Yes" and "No " literals instead.
        //
        // CORRECTION: an earlier pass read the SetSpriteImage calls here as loading cursor sprites.
        // SetSpriteImage draws text (renamed DrawTextToLayer), so these are strings.
        DrawConfirmLabel(_textLayer3, _inspector.Build.Strings.Yes, _inspector.Build.ConfirmYesStringAddress, 0, 0);
        DrawConfirmLabel(_textLayer3, _inspector.Build.Strings.No, _inspector.Build.ConfirmNoStringAddress, 0x30, 1);

        _hudTopLeft.Set(0, 0xF0, 0, 4, 0, 0xF0);
        _hudTopRight.Set(0, 0xF0, 0, 8, 0, 0xF0);
        _hudTitle.Set(0x140, 0x90, 0x10, 4, 0x140, 0x90);

        _saveSlotBox.Initialize(0, -1, 0x2C0, 0x100, 0x10, 0x10, 0x100, 0x1E1);
        _saveSlotBox.SetOffset(0, 0);
        _saveSlotBox.SetBaseAndRotation(0xAF, 0x85, 0x78, -1);

        _pointerTick = 0;
        _pointerFrame = 0;
    }

    /// <summary>GHIDRA: InitSaveSlotSelectionUI @ 0x80023500.</summary>
    private void InitSaveSlotSelectionUI()
    {
        var sheet = _inspector.FindEtcResource("TIM", 5);
        if (sheet is not null)
        {
            _spriteSheetLayer.InitializeTileLayer(_inspector.ExeBytes, sheet.Value.PayloadOffset);
        }

        _spriteSheetLayer.SetTileLayerBounds(_ui.Vram, 0x280, 0, 0, 0x1E1, 0);

        // The walker: a 24x40 body and a 24x16 shadow, both from the sheet at (0x280, 0). Their
        // offsets put the sheet cell's centre on the camera.
        _walkerBoxes[0].Initialize(1, -1, 0x280, 0, 0x18, 0x28, 0, 0x1E1);
        _walkerBoxes[0].SetOffset(unchecked((short)0xFFF4), unchecked((short)0xFFD8));
        _walkerBoxes[0].SetBaseAndRotation(0x3C, 0x3C, 0xC, 0);

        _walkerBoxes[1].Initialize(1, 0, 0x280, 0xA0, 0x18, 0x10, 0, 0x1E1);
        _walkerBoxes[1].SetOffset(unchecked((short)0xFFF4), unchecked((short)0xFFF1));
        _walkerBoxes[1].SetBaseAndRotation(0x3C, 0x3C, 0xC, 0);

        _cameraX = 0xA0000;
        _cameraY = 0xB4000;
        _isIdle = 1;
        _facing = 1;
        _walkFrame = 0;
        _hotspot = -1;

        _ornamentBoxes[0].Initialize(1, -1, 0x280, 0xB0, 0x20, 0x30, 0, 0x1E1);
        _ornamentBoxes[0].SetOffset(unchecked((short)0xFFF0), unchecked((short)0xFFE8));
        _ornamentBoxes[0].SetBaseAndRotation(0xA0, 0x60, 0xC, -1);

        _ornamentBoxes[1].Initialize(1, 1, 0x2C4, 0xB0, 0x33, 0x31, 0, 0x1E1);
        _ornamentBoxes[1].SetOffset(unchecked((short)0xFFE7), unchecked((short)0xFFEB));
        _ornamentBoxes[1].SetBaseAndRotation(0xA0, 0x60, 0xD, -1);

        _ornamentBoxes[2].Initialize(1, 1, 0x28C, 0xA0, 0x50, 8, 0, 0x1E1);
        _ornamentBoxes[2].SetOffset(unchecked((short)0xFFD8), unchecked((short)0xFFFF));
        _ornamentBoxes[2].SetBaseAndRotation(0xA0, 0x60, 0xE, -1);

        _ornamentBoxes[3].Initialize(1, 1, 0x28C, 0xA0, 0x50, 8, 0, 0x1E1);
        _ornamentBoxes[3].SetOffset(unchecked((short)0xFFD8), unchecked((short)0xFFFF));
        _ornamentBoxes[3].SetBaseAndRotation(0xA0, 0x60, 0xF, -1);

        _ornamentTick = 0;
        _ornamentPhase2 = 0;
        _ornamentPhase3 = 0x3DE;

        // Four 24x24 markers over the four houses, 0x30 apart.
        for (short i = 0; i < 4; i++)
        {
            var box = _slotMarkerBoxes[i];
            box.Initialize(1, -1, 0x280, 0xE8, 0x18, 0x18, 0, 0x1E1);
            box.SetOffset((short)(0x4C + i * 0x30), 0x6C);
            box.SetBaseAndRotation(0, 0, 0xC, -1);
        }

        for (var i = 0; i < 4; i++)
        {
            _slotMarkerText[i] = _markerRestingString;
            _slotMarkerPosition[i] = 0;
        }
    }

    /// <summary>
    /// GHIDRA: RunLoaderMainSequence @ 0x80024e28, opening block, then InitSelectionMenu
    /// @ 0x8002471c.
    /// </summary>
    public void Start()
    {
        _cloudTick = 0;
        _cloudScroll = 0;
        _fadeLevel = 0;
        _cameraX = 0xA0000;
        _cameraY = 0xF0000;
        _isIdle = 0;
        _facing = 1;
        _walkFrame = 0;
        _hotspot = -1;
        Result = -1;

        // InitSelectionMenu: clear the panel, read the card, choose the message.
        _textLayer1.ClearTextLayer(1);
        _cardReadResult = LoadSavesAndPickMessage();
        _playSoundEffect(4);
        _panelMode = 1;
        CurrentPhase = Phase.CardMessage;
    }

    /// <summary>
    /// GHIDRA: HandleResourceLoadFailureOrFallback @ 0x80024560 — renamed
    /// <c>LoadSaveSlotsAndPickMessage</c>: it reads the card and picks the message that describes
    /// what it found, it does not handle a resource load failure.
    /// </summary>
    /// <returns>0 when at least one save was found, -1 otherwise.</returns>
    private int LoadSavesAndPickMessage()
    {
        var result = LoaderSaveSlots.Read(_records);

        // The five card errors reduce to one on desktop; the message indices are the original's.
        if (result == LoaderSaveSlots.ResultNoCard)
        {
            SetLayerText(_textLayer1, _inspector.Build.Strings.InsertMemoryCard);
            return -1;
        }

        var occupied = 0;
        for (var i = 0; i < _records.Length; i++)
        {
            // GHIDRA: g_slotError4..1, declared backwards — index 0 is slot 0's marker.
            _slotMarkerText[i] = _records[i].Occupied ? _markerRestingString : null;
            _slotMarkerPosition[i] = 0;
            if (_records[i].Occupied)
            {
                occupied++;
            }
        }

        if (occupied != 0)
        {
            SetLayerText(_textLayer1, _inspector.Build.Strings.UsingMemoryCard);
            return 0;
        }

        SetLayerText(_textLayer1, _inspector.Build.Strings.NoSaveData);
        return -1;
    }

    /// <summary>
    /// Puts the screen back to Idle so a later visit re-runs <see cref="Start"/>.
    /// </summary>
    /// <remarks>
    /// GHIDRA: the original has no equivalent because RunLoaderMainSequence is a call, not a state:
    /// returning from it leaves nothing behind, and the next call runs its prologue again. This is
    /// that prologue's precondition, expressed for a phase machine.
    /// </remarks>
    public void Reset()
    {
        CurrentPhase = Phase.Idle;
        Result = -1;
        _panelMode = 0;
    }

    /// <summary>One frame. Returns true once the sequence is over.</summary>
    public bool Update(uint buttons)
    {
        switch (CurrentPhase)
        {
            case Phase.CardMessage:
                UpdateBackgroundState();
                if (!_textLayer1.IsTyping && (buttons & ButtonCircle) != 0)
                {
                    _playSoundEffect(5);
                    _panelMode = 0;
                    _holdCounter = 0;
                    CurrentPhase = Phase.CardMessageHold;
                }

                break;

            case Phase.CardMessageHold:
                _holdCounter++;
                UpdateBackgroundState();
                if (_holdCounter >= 0x3C)
                {
                    if (_cardReadResult != 0)
                    {
                        // GHIDRA: RunLoaderMainSequence skips the whole screen and returns -1 when
                        // InitSelectionMenu reports a failure.
                        Result = -1;
                        CurrentPhase = Phase.Finished;
                        break;
                    }

                    // ShowSelectionScreen @ 0x800247fc.
                    _playBgmTrack(SelectionBgmTrack);
                    _playSoundEffect(0x193);
                    _fadeLevel = -0xFF;
                    CurrentPhase = Phase.FadeIn;
                }

                break;

            case Phase.FadeIn:
                _fadeLevel += 8;
                RenderSelectionFrame();
                if (_fadeLevel >= 0)
                {
                    _fadeLevel = 0;

                    // UpdateSelectionCursor @ 0x80024888.
                    _isIdle = 1;
                    _textLayer1.ClearTextLayer(1);
                    SetLayerText(_textLayer1, 199);
                    _playSoundEffect(4);
                    _panelMode = 1;
                    CurrentPhase = Phase.Prompt;
                }

                break;

            case Phase.Prompt:
                RenderSelectionFrame();
                if (!_textLayer1.IsTyping && (buttons & ButtonCross) != 0)
                {
                    _playSoundEffect(5);
                    _panelMode = 0;
                    _holdCounter = 0;
                    CurrentPhase = Phase.PromptHold;
                }

                break;

            case Phase.PromptHold:
                _holdCounter++;
                RenderSelectionFrame();
                if (_holdCounter >= 0x3C)
                {
                    CurrentPhase = Phase.Walking;
                }

                break;

            case Phase.Walking:
                UpdateWalking(buttons);
                break;

            case Phase.Confirm:
                UpdateConfirm(buttons);
                break;

            case Phase.FadeOut:
                if (Result == 6)
                {
                    _cameraY += 0x400;
                }

                RenderSelectionFrame();
                _fadeLevel += 3;
                if (_fadeLevel >= 0xFF)
                {
                    _holdCounter = 0;
                    CurrentPhase = Phase.Settle;
                }

                break;

            case Phase.Settle:
                // GHIDRA: RunLoaderMainSequence ends on two bare RunTransitionEffect /
                // UpdateTransitionGraphics pairs — two frames drawing nothing.
                _holdCounter++;
                if (_holdCounter >= 2)
                {
                    if (Result == 6)
                    {
                        Result = -1;
                    }

                    CurrentPhase = Phase.Finished;
                }

                break;

            case Phase.Finished:
                return true;

            case Phase.Idle:
            default:
                return true;
        }

        return CurrentPhase == Phase.Finished;
    }

    /// <summary>GHIDRA: GetUserInput @ 0x80024984, one iteration.</summary>
    private void UpdateWalking(uint buttons)
    {
        // SetTransitionStateAndProgress(0, dir) for each direction, then
        // SetTransitionStateAndProgress((buttons & 0xf000) == 0, -1): the first argument is the
        // idle flag, the second the facing, and -1 means "leave alone".
        if ((buttons & ButtonUp) != 0)
        {
            _isIdle = 0;
            _facing = 1;
        }

        if ((buttons & ButtonDown) != 0)
        {
            _isIdle = 0;
            _facing = 0;
        }

        if ((buttons & ButtonLeft) != 0)
        {
            _isIdle = 0;
            _facing = 2;
        }

        if ((buttons & ButtonRight) != 0)
        {
            _isIdle = 0;
            _facing = 3;
        }

        _isIdle = (buttons & ButtonDirections) == 0 ? 1 : 0;

        RenderSelectionFrame();

        if (_hotspot == -1)
        {
            return;
        }

        if (_hotspot == 6)
        {
            Result = 6;
            BeginFadeOut();
            return;
        }

        // ValidateSelection @ 0x80024b10: anything but an occupied slot 0..3 is rejected and the
        // walk resumes, which is what turns the 0x64-and-up hotspots into walls.
        if (_hotspot >= 4 || _slotMarkerText[_hotspot] is null)
        {
            _hotspot = -1;
            return;
        }

        BeginConfirm(_hotspot);
    }

    /// <summary>GHIDRA: ValidateSelection @ 0x80024b10, prologue.</summary>
    private void BeginConfirm(int slot)
    {
        _confirmSlot = slot;
        _confirmYesNo = 0;
        _confirmDelay = 10;
        _confirmBlipPending = true;
        _confirmButtons = 0;

        _playSoundEffect(4);
        _panelMode = 2;
        _textLayer1.ClearTextLayer(1);
        _textLayer2.ClearTextLayer(1);
        _isIdle = 1;

        SetLayerText(_textLayer1, _inspector.Build.Strings.ConfirmLoad);

        // The chapter name comes from the four ASCII digits at the head of CurrentFlagName, read as
        // a string-table index; the save's own summary line is the 0x20 bytes at +0x28. An index of
        // -1 means the field could not supply one, and the top line is left blank rather than
        // labelled with a chapter the save is not in.
        var record = _records[slot];
        if (record.PlaceStringIndex >= 0)
        {
            DrawStringEntry(_textLayer2, record.PlaceStringIndex, 0, 0, 0);
        }

        _textLayer2.CursorX = 0;
        _textLayer2.CursorY = 0x10;
        if (record.Name.Length != 0)
        {
            _textLayer2.DrawText(record.Name, 0, 1);
        }

        _slotMarkerText[slot] = _markerAnimationString;
        _slotMarkerPosition[slot] = 0;
        _playSoundEffect(0x194);

        CurrentPhase = Phase.Confirm;
    }

    /// <summary>GHIDRA: ValidateSelection @ 0x80024b10, the wait loop.</summary>
    private void UpdateConfirm(uint buttons)
    {
        var marker = _slotMarkerText[_confirmSlot];
        if (marker is not null && _confirmBlipPending && marker[_slotMarkerPosition[_confirmSlot]] == 0)
        {
            _playSoundEffect(0xCC);
            _confirmBlipPending = false;
        }

        _saveSlotBox.BaseX = (short)(_confirmYesNo * 0x30 + 0xBF);

        RenderSelectionFrame();
        _confirmButtons = buttons;

        // The inner loop holds until the message has finished typing and the title panel has
        // finished sliding in, then ten more frames pass before input is read.
        if (_textLayer1.IsTyping || _hudTitle.X != 0xAF)
        {
            return;
        }

        _confirmDelay--;
        if (_confirmDelay != -1)
        {
            return;
        }

        _confirmDelay = 0;

        if ((_confirmButtons & ButtonLeft) != 0)
        {
            _confirmYesNo = 0;
        }

        if ((_confirmButtons & ButtonRight) != 0)
        {
            _confirmYesNo = 1;
        }

        if ((_confirmButtons & ButtonCross) == 0)
        {
            return;
        }

        _playSoundEffect(_confirmYesNo == 0 ? 2 : 3);
        _playSoundEffect(5);
        _panelMode = 0;

        // The marker is parked on the animation string's terminator, which UpdateMenuGraphics reads
        // by stepping one character back — so it settles on the last frame and stays there.
        _slotMarkerText[_confirmSlot] = _markerAnimationString;
        _slotMarkerPosition[_confirmSlot] = Math.Min(LoaderExeInspector.SlotMarkerParkedOffset, _markerAnimationString.Length - 1);

        if (_confirmYesNo == 0)
        {
            Result = _confirmSlot;
            BeginFadeOut();
            return;
        }

        _hotspot = -1;
        CurrentPhase = Phase.Walking;
    }

    /// <summary>GHIDRA: ProcessSelection @ 0x80024a54, prologue.</summary>
    private void BeginFadeOut()
    {
        _stopBgm();
        _fadeLevel = 0;
        CurrentPhase = Phase.FadeOut;
    }

    /// <summary>
    /// One iteration of the selection screen's frame body, in the original's order:
    /// UpdateFadeState, UpdateBackgroundState, UpdateMenuGraphics.
    /// </summary>
    private void RenderSelectionFrame()
    {
        UpdateFadeState();
        UpdateBackgroundState();
        UpdateMenuGraphics();
    }

    /// <summary>
    /// GHIDRA: UpdateFadeState @ 0x800243d4 — draws the backdrop, scrolls the clouds and puts the
    /// fade quad on top.
    /// </summary>
    private void UpdateFadeState()
    {
        _mapBoxes[0].SetBaseAndRotation(0, 0, 0, -1);
        _ui.RenderRun(_mapBoxes, 0, 5);

        _mapForegroundBoxes[0].SetBaseAndRotation(0, 0, 9, -1);
        _ui.RenderRun(_mapForegroundBoxes, 0, 5);

        // The clouds are drawn twice, 0x140 apart, so the scroll wraps without a seam.
        _cloudBoxes[0].SetBaseAndRotation((short)_cloudScroll, 0, 1, -1);
        _ui.RenderRun(_cloudBoxes, 0, 5);
        _cloudBoxes[0].SetBaseAndRotation((short)((_cloudScroll & 0xFFFF) + 0x140), 0, 1, -1);
        _ui.RenderRun(_cloudBoxes, 0, 5);

        _cloudTick++;
        if (_cloudTick == 4)
        {
            _cloudTick = 0;
            _cloudScroll--;
        }

        if (_cloudScroll < -0x13F)
        {
            _cloudScroll = 0;
        }

        if (_fadeLevel > 0)
        {
            _fadeQuad.FlatAbr = 1;
            _fadeQuad.FlatColorB = (byte)_fadeLevel;
            _fadeQuad.FlatColorG = (byte)_fadeLevel;
            _fadeQuad.FlatColorR = (byte)_fadeLevel;
            _ui.RenderFlatQuad(_fadeQuad);
        }

        if (_fadeLevel < 0)
        {
            _fadeQuad.FlatAbr = 2;
            _fadeQuad.FlatColorR = (byte)-(sbyte)_fadeLevel;
            _fadeQuad.FlatColorG = _fadeQuad.FlatColorR;
            _fadeQuad.FlatColorB = _fadeQuad.FlatColorR;
            _ui.RenderFlatQuad(_fadeQuad);
        }
    }

    /// <summary>
    /// GHIDRA: UpdateBackgroundState @ 0x800231c4 — slides the message panels, draws them and their
    /// text, and steps all three typewriters.
    /// </summary>
    private void UpdateBackgroundState()
    {
        var titleTargetX = 0x140;
        var setTitleTarget = true;

        switch (_panelMode)
        {
            case 1:
                _hudTopLeft.SetTarget(0, 0xA8);
                _hudTopRight.SetTarget(0, 0xF0);
                break;

            case 0:
                _hudTopLeft.SetTarget(0, 0xF0);
                _hudTopRight.SetTarget(0, 0xF0);
                break;

            case 2:
                _hudTopLeft.SetTarget(0, 0xA8);
                _hudTopRight.SetTarget(0, 0x20);

                // The title panel only comes in once the message has finished typing.
                if (_textLayer1.IsTyping)
                {
                    setTitleTarget = false;
                }
                else
                {
                    titleTargetX = 0xAF;
                }

                break;

            default:
                setTitleTarget = false;
                break;
        }

        if (setTitleTarget)
        {
            _hudTitle.SetTarget(titleTargetX, 0x90);
        }

        _panelBoxes[0].SetBaseAndRotation((short)_hudTopLeft.X, (short)_hudTopLeft.Y, 100, -1);
        _ui.RenderRun(_panelBoxes, 0, 2);
        _panelBoxes[2].SetBaseAndRotation((short)_hudTopRight.X, (short)_hudTopRight.Y, 100, -1);
        _ui.RenderRun(_panelBoxes, 2, 2);
        _panelBoxes[4].SetBaseAndRotation((short)_hudTitle.X, (short)_hudTitle.Y, 100, -1);
        _ui.RenderRun(_panelBoxes, 4, 2);

        _textBox1.SetBaseAndRotation((short)_hudTopLeft.X, (short)_hudTopLeft.Y, 0x6E, -1);
        _ui.Render(_textBox1);
        _textBox2.SetBaseAndRotation((short)_hudTopRight.X, (short)_hudTopRight.Y, 0x6E, -1);
        _ui.Render(_textBox2);
        _textBox3.SetBaseAndRotation((short)_hudTitle.X, (short)_hudTitle.Y, 0x6E, -1);
        _ui.Render(_textBox3);

        _textLayer1.Advance();
        _textLayer2.Advance();
        _textLayer3.Advance();

        _hudTopLeft.Update();
        _hudTopRight.Update();
        _hudTitle.Update();

        // DisplayString2("~c0f0 cr0_anm      ", ...) is a debug print; not ported.
        _saveSlotBox.PackedU = (short)(_pointerFrame * 4 + 0x2C0);
        _pointerTick++;
        if (_pointerTick == 9)
        {
            _pointerFrame = (_pointerFrame + 1) & 3;
            _pointerTick = 0;
        }

        if (_hudTitle.X == 0xAF)
        {
            _ui.Render(_saveSlotBox);
        }
    }

    /// <summary>
    /// GHIDRA: UpdateMenuGraphics @ 0x80023b14 — animates the walker, spins the ornaments, moves
    /// the camera and draws the four save markers.
    /// </summary>
    private void UpdateMenuGraphics()
    {
        var frame = _walkFrame >> 3;
        _walkFrame++;

        // The sheet row is the facing, the column the walk frame; idle shifts a whole 0x30-word set.
        _walkerBoxes[0].PackedU = (short)(_isIdle * 0x30 + (frame & 3) * 0xC + 0x280);
        _walkerBoxes[0].PackedV = (short)(_facing * 0x28);

        var cameraX = _cameraX < 0 ? _cameraX + 0xFFF : _cameraX;
        var cameraY = _cameraY < 0 ? _cameraY + 0xFFF : _cameraY;

        _walkerBoxes[0].SetBaseAndRotation((short)(cameraX >> 12), (short)(cameraY >> 12), 0xC, -1);
        _ui.RenderRun(_walkerBoxes, 0, 2);

        _hotspot = MoveCameraAndTestHotspots();

        _ornamentBoxes[0].PackedU = (short)((_ornamentTick & 0x18) * 2 + 0x280);

        _ornamentBoxes[1].RotationZ -= 0x16;
        if (_ornamentBoxes[1].RotationZ == 0x1000)
        {
            _ornamentBoxes[1].RotationZ = 0;
        }

        _ornamentBoxes[2].RotationZ = _ornamentPhase2;
        _ornamentPhase2 += 0xB;
        _ornamentBoxes[3].RotationZ = _ornamentPhase3;
        if (_ornamentPhase2 == 0x1000)
        {
            _ornamentPhase2 = 0;
        }

        _ornamentPhase3 += 0xB;
        if (_ornamentPhase3 == 0x1000)
        {
            _ornamentPhase3 = 0;
        }

        // Every other frame only the first ornament is drawn, which is what makes the three spinning
        // ones flicker at half rate.
        var count = ((_ornamentTick + 1) & 1) == 0 ? 1 : 4;
        _ornamentTick++;
        _ui.RenderRun(_ornamentBoxes, 0, count);

        for (var slot = 0; slot < 4; slot++)
        {
            var text = _slotMarkerText[slot];
            if (text is null)
            {
                continue;
            }

            // Reaching the terminator steps the cursor one character back, so a one-character string
            // holds a single frame and the animation string parks on its last one.
            if (text[_slotMarkerPosition[slot]] == 0 && _slotMarkerPosition[slot] > 0)
            {
                _slotMarkerPosition[slot]--;
            }

            var digit = text[_slotMarkerPosition[slot]];
            _slotMarkerBoxes[slot].PackedU = (short)((digit - 0x30) * 0xC + 0x280);
            if (_slotMarkerPosition[slot] + 1 < text.Length)
            {
                _slotMarkerPosition[slot]++;
            }

            _ui.Render(_slotMarkerBoxes[slot]);
        }
    }

    /// <summary>
    /// GHIDRA: FUN_800239c4 @ 0x800239c4 — renamed <c>MoveCameraAndTestHotspots</c>.
    /// </summary>
    /// <remarks>
    /// The step is applied to a copy first and the hotspot table tested against that copy; on a hit
    /// the code is returned and the copy is thrown away. That single detail is the screen's whole
    /// collision system: the four save houses (codes 0..3) and the exit strip (6) stop the walker
    /// just as the six wall rectangles (0x64 and up) do, and it is ValidateSelection that decides
    /// which of them mean anything.
    ///
    /// The comparison is on the pixel position, so the fixed-point value is shifted down by 12 with
    /// the compiler's rounding correction for negatives kept.
    /// </remarks>
    private int MoveCameraAndTestHotspots()
    {
        var x = _cameraX;
        var y = _cameraY;

        if (_isIdle == 0)
        {
            switch (_facing)
            {
                case 1:
                    y = _cameraY - 0x1800;
                    break;
                case 0:
                    y = _cameraY + 0x1800;
                    break;
                case 2:
                    x = _cameraX - 0x2000;
                    break;
                case 3:
                    x = _cameraX + 0x2000;
                    break;
            }

            var pixelX = (x < 0 ? x + 0xFFF : x) >> 12;
            var pixelY = (y < 0 ? y + 0xFFF : y) >> 12;

            foreach (var hotspot in _hotspots)
            {
                if (hotspot.X < pixelX &&
                    hotspot.Y < pixelY &&
                    pixelX <= hotspot.X + hotspot.Width &&
                    pixelY <= hotspot.Y + hotspot.Height)
                {
                    return hotspot.Code;
                }
            }
        }

        _cameraY = y;
        _cameraX = x;
        return -1;
    }

    /// <summary>Points a layer at a string-table entry so its typewriter can walk it.</summary>
    private void SetLayerText(LoaderTextLayer layer, int entryIndex)
    {
        var offset = _strings?.GetEntryOffset(entryIndex) ?? -1;
        if (offset < 0 || _strings is null)
        {
            layer.Text = Encoding.Latin1.GetBytes("\0");
            layer.TextPosition = 0;
            return;
        }

        layer.Text = _strings.Buffer;
        layer.TextPosition = offset;
    }

    /// <summary>
    /// Draws one confirmation label, from wherever this build keeps it.
    /// </summary>
    /// <remarks>
    /// France reads it out of ETC_RES.R like every other string; the USA build has it as a literal
    /// in the executable, so the bytes come from there and the cursor work is the same either way.
    /// </remarks>
    private void DrawConfirmLabel(LoaderTextLayer layer, int entryIndex, uint? exeAddress, int cursorX, int sync)
    {
        if (exeAddress is not { } address)
        {
            DrawStringEntry(layer, entryIndex, cursorX, 0, sync);
            return;
        }

        layer.CursorX = cursorX;
        layer.CursorY = 0;
        layer.DrawText(_inspector.ExeBytes, _inspector.RamToFileOffset(address), sync);
    }

    /// <summary>Draws a string-table entry into a layer in one call, at a given cursor position.</summary>
    private void DrawStringEntry(LoaderTextLayer layer, int entryIndex, int cursorX, int cursorY, int sync)
    {
        layer.CursorX = cursorX;
        layer.CursorY = cursorY;

        var offset = _strings?.GetEntryOffset(entryIndex) ?? -1;
        if (offset < 0 || _strings is null)
        {
            return;
        }

        layer.DrawText(_strings.Buffer, offset, sync);
    }
}
