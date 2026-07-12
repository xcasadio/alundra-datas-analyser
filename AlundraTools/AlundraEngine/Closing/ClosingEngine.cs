using AlundraEngine.Graphics;

namespace AlundraEngine.Closing;

public class ClosingEngine(IRenderer renderer)
{
    // SOURCE: comment on the constructor below (pre-existing in this file).
    // formule: File Offset = RAM Address - 0x8001F800
    private const uint RamToFileOffsetDelta = 0x8001F800;

    private Inspector _inspector;
    private byte[] _exeBytes = [];
    private CreditsPictureEntry[] CreditsPictureEntry_ARRAY_8003a28c = [];
    private CreditsBlock[] g_creditsBlockTable = [];

    // GHIDRA: g_creditsPictureTablePtr @ 0x801b6b64
    // JUSTIFICATION: C# language bridge only
    // RELATION: original type is `CreditsPictureEntry *` (walks CreditsPictureEntry_ARRAY_8003a28c
    // one entry at a time via `+ 1` pointer arithmetic). Ported as an index into that same array
    // instead of a raw pointer (no unsafe in this port); the "+1" arithmetic becomes "+1" index
    // arithmetic in LoadNextCreditsPicture, preserving the exact same walk.
    private int g_creditsPictureTablePtr;

    private int g_creditsSequenceDone;
    private int g_creditsFadeSpeed;

    // GHIDRA: g_creditsFadeActive @ 0x801b6b50
    private short g_creditsFadeActive;
    // GHIDRA: g_creditsFadeLevel @ 0x801b6b5c
    private int g_creditsFadeLevel;
    // GHIDRA: g_creditsPictureLayoutIndex @ 0x801b6b60
    private ushort g_creditsPictureLayoutIndex;
    // GHIDRA: DAT_801b6b54 @ 0x801b6b54
    // PROBABLE (per UpdateCreditsTextSequencer's plate comment): background/backdrop parameter,
    // not yet elucidated. Kept raw, only ever written by LoadNextCreditsPicture, never read.
    private uint DAT_801b6b54;

    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: original tracks the loaded picture only as a raw VRAM upload (OpenTIM/ReadTIM/
    // LoadImage into the shared picture VRAM rect); the desktop equivalent keeps the decoded
    // Bitmap directly instead of a separate VRAM buffer, since IRenderer.DrawDeformedQuad samples
    // a Bitmap rather than a VRAM tpage/clut pair.
    private Bitmap? _currentCreditsPictureBitmap;

    // GHIDRA: g_creditsBlockTimer @ 0x801beb68
    private short g_creditsBlockTimer;
    // GHIDRA: g_creditsBlockIndex @ 0x801beb6a
    private short g_creditsBlockIndex;
    // GHIDRA: g_creditsBlockPhase @ 0x801beb76
    private short g_creditsBlockPhase;
    // GHIDRA: g_creditsScrollY @ 0x801beb6e
    private short g_creditsScrollY;
    // GHIDRA: g_creditsBlockType @ 0x801beb78
    private ushort g_creditsBlockType;
    // GHIDRA: g_creditsTextX @ 0x801beb6c
    private short g_creditsTextX;
    // GHIDRA: g_creditsBlockExtraHoldFlag @ 0x801beb70
    private short g_creditsBlockExtraHoldFlag;
    // GHIDRA: g_creditsBlockTextHeight @ 0x801beb74
    private short g_creditsBlockTextHeight;
    // GHIDRA: g_creditsBlockLineCount @ 0x801beb72
    private short g_creditsBlockLineCount;

    // GHIDRA: g_creditsLineCountScrollTable @ 0x800208a4
    // SOURCE: Ghidra (ReVa read-memory), 12 x int32, closed by direct read.
    private static readonly int[] g_creditsLineCountScrollTable = [0, -48, 80, 72, 64, 56, 48, 40, 32, 24, 16, 8];

    // GHIDRA: g_creditsGlyphTable @ 0x80038e94
    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: read once from CLOSING.EXE at a fixed file offset instead of being resident at a
    // fixed RAM address, same pattern as AlunCdExe.cs's hardcoded-offset exe resource reads.
    private GlyphMetrics[] g_creditsGlyphTable = [];

    // GHIDRA: g_creditsFontBitmapPtr @ 0x801b6b48
    // JUSTIFICATION: C# language bridge only
    // RELATION: original is a u_char* into VRAM (the just-uploaded font glyph sheet, set by
    // LoadCreditsFont from TIM_IMAGE.paddr). Ported as the raw (still nibble-packed 4bpp, not
    // decoded to ARGB) pixel bytes of that same TIM resource, since DrawCreditsTextLine's blit is a
    // nibble-level operation that must stay nibble-level to remain faithful (no unsafe pointers).
    private byte[] g_creditsFontBitmapPtr = [];
    private Color[] _creditsFontPalette = [];

    // GHIDRA: g_creditsTextCanvas @ 0x801b6b68
    // JUSTIFICATION: C# language bridge only
    // RELATION: original is a VRAM-format 4bpp canvas (256x240, u_long* aligned). Ported as the
    // same nibble-packed byte[] layout (0x7800 = 256*240/2 bytes) so DrawCreditsTextLine's
    // byte/nibble arithmetic carries over unchanged; only the final "upload to VRAM" step is
    // adapted (see DrawCreditsTextQuad).
    private readonly byte[] g_creditsTextCanvas = new byte[0x7800];

    // JUSTIFICATION: C# language bridge only
    // RELATION: g_creditsTextCanvas only actually changes when UpdateCreditsTextSequencer redraws
    // it (once per CreditsBlock, via DrawCreditsTextLine), not every frame; caching the decoded
    // Bitmap and only rebuilding it when the canvas was just rewritten avoids re-decoding all
    // 256x240 nibbles through BuildBitmapFromNibbleCanvas on every single DrawCreditsTextQuad call.
    private Bitmap? _creditsTextCanvasBitmap;
    private bool _creditsTextCanvasDirty = true;

    private int textBrightness;
    private int scene2Counter;
    private ClosingState _closingState = ClosingState.PlayMovie;

    public void InitializeEngine(string gamePath)
    {
        _inspector = new Inspector(gamePath);
        _exeBytes = _inspector.ExeBytes;
        CreditsPictureEntry_ARRAY_8003a28c = ReadCreditsPictureTable();
        g_creditsBlockTable = ReadCreditsBlockTable();
        g_creditsGlyphTable = ReadCreditsGlyphTable();

        g_creditsPictureTablePtr = 0;
        g_creditsSequenceDone = 0;
        textBrightness = 0x80;

        LoadCreditsFont();
    }

    public GameState MainLoop()
    {
        var i = 0;
        ulong buttons = 0;

        switch (_closingState)
        {
            case ClosingState.PlayMovie:
                //play \\MOVIE\\ARAN_END.MOV
                _closingState = ClosingState.Scene1;
                break;

            case ClosingState.Scene1:
                UpdateAndDrawCreditsFade();
                UpdateCreditsTextSequencer();
                DrawCreditsTextQuad(0x80);
                //EndFrame(0);
                buttons = PadManager.ButtonStates; //PadRead(0);
                
                if ((buttons & 0x840) != 0)
                {
                    _closingState = ClosingState.Finished;
                }

                if (g_creditsSequenceDone != 0)
                {
                    _closingState = ClosingState.Scene2;
                }
                break;

            case ClosingState.Scene2:
                UpdateAndDrawCreditsFade();
                //EndFrame(0);
                buttons = PadManager.ButtonStates; //PadRead(0);
                if ((buttons & 0x840) == 0)
                {
                    scene2Counter++;
                }
                else
                {
                    _closingState = ClosingState.Scene3;
                }

                if (scene2Counter == 0xfa)
                {
                    _closingState = ClosingState.Scene3;
                    g_creditsFadeSpeed = -2;
                }
                break;

            case ClosingState.Scene3:
                // SOURCE: Ghidra main @ LAB_80022790: u_char textBrightness pre-decremented by 2
                // (u_char += 0xfe = -2 mod 256) every frame before drawing, guarded by != 0
                // (the outer do-while's condition). The original inner loop also decrements after
                // the draw when g_creditsFadeActive == 0, but since both paths decrement once per
                // frame the net per-frame draw value is identical; collapsing to a single
                // pre-decrement before the draw is faithful and simpler for the state machine.
                if (textBrightness != 0)
                {
                    textBrightness -= 2;
                }

                UpdateAndDrawCreditsFade();
                DrawCreditsTextQuad(textBrightness);
                //EndFrame(0);

                if (g_creditsFadeActive != 0)
                {
                    break;
                }

                if (textBrightness == 0)
                {
                    _closingState = ClosingState.Finished;
                }
                break;

            case ClosingState.Finished:
                //SetDispMask(0);
                //OpenTIM((u_long*)&DAT_8016e6e4);
                //ReadTIM(&timImage);
                //rect.x = 0;
                //rect.y = 0;
                //rect.w = 0x140;
                //rect.h = 0xf0;
                //LoadImage(&rect, timImage.paddr);
                //rect.x = 0;
                //rect.y = 0xf0;
                //rect.w = 0x140;
                //rect.h = 0xf0;
                //LoadImage(&rect, timImage.paddr);
                //DrawSync(0);
                //SetDispMask(1);
                //StopRCnt(0xf2000000);
                //StopRCnt(0xf2000002);
                //StopRCnt(0xf2000003);
                //ResetGraph(3);
                //FUN_8002e4d4();
                //FUN_8002e4b4();
                //FUN_8003690c();
                //PadStop();
                //StopCallback();
                //_96_remove();
                //_96_init();
                //FUN_800235b0();
                //LoadExec("cdrom:\\LOADER.EXE;1", &DAT_801fff00, 0);
                //exit();
                return GameState.MainMenu;
        }

        renderer.Render();
        renderer.Clear();

        return GameState.EndScene;
    }

    // GHIDRA: UpdateAndDrawCreditsFade @ 0x800222f0
    // SOURCE: Ghidra (ReVa get-decompilation), plate comment CERTAIN via PCSX-Redux runtime
    // confirmation (SLES01198 PAL-FR, 2026-07-11): fade ramp/clamp behavior and quad geometry.
    // NOTE: that same plate comment also claims "x0,x2,y2,y3 fields are never written by this
    // function"; the current (fully-expanded switch) decompilation below contradicts that - every
    // case 0-6 sets all eight x0..x3/y0..y3 fields explicitly, only the `default:` (layoutIndex
    // outside 0-6, never produced by the real CreditsPictureEntry table) skips them. Trusting the
    // current decompilation over the older comment here.
    // JUSTIFICATION: PSX hardware adaptation only (for the final draw call)
    // RELATION: original builds one POLY_FT4 primitive and links it into the OT/primitive buffer
    // (g_currentOtEntryPtr/g_nextPrimitivePtr, not ported - no desktop equivalent exists once
    // rendering goes through IRenderer). Replaced by a single IRenderer.DrawDeformedQuad call
    // sampling _currentCreditsPictureBitmap, matching the same accepted VRAM/primitive -> desktop
    // renderer adaptation already used throughout GraphicManager.cs.
    private void UpdateAndDrawCreditsFade()
    {
        if (g_creditsFadeActive == 0)
        {
            return;
        }

        if (g_creditsFadeSpeed < 1)
        {
            if (g_creditsFadeSpeed < 0)
            {
                g_creditsFadeLevel = g_creditsFadeLevel + g_creditsFadeSpeed;
                if (g_creditsFadeLevel < 1)
                {
                    g_creditsFadeActive = 0;
                    g_creditsFadeLevel = 0;
                }
            }
        }
        else
        {
            g_creditsFadeLevel = g_creditsFadeLevel + g_creditsFadeSpeed;
            if (0x7f < g_creditsFadeLevel)
            {
                g_creditsFadeSpeed = 0;
                g_creditsFadeLevel = 0x80;
            }
        }

        short x0, y0, x1, y1, x2, y2, x3, y3;
        // PARTIAL: named uSpan since the decompiled local is reused later for the true fadeLevel
        // byte (a decompiler variable-collapse artifact, same kind already flagged for
        // destKeptNibble in DrawCreditsTextLine) - kept as two separate locals here for clarity.
        // uSpan ends up in BOTH u1 and u3 in every case (0-6): the goto-cases (1,3,5) set both
        // directly, the fallthrough-cases (0,2,4,6) set u1 then reach the shared u3=fadeLevel
        // assignment below (LAB_800225b0) - same value either way.
        byte uSpan;
        byte u0, u2;
        var hasGeometry = true;

        switch (g_creditsPictureLayoutIndex)
        {
            case 0:
                y0 = 0x10; y1 = 0x10; x1 = 0x7c; x3 = 0x7c; uSpan = 0x7f; x0 = 0xc; x2 = 0xc; y2 = 0xf0; y3 = 0xf0;
                u0 = 0; u2 = 0;
                break;
            case 1:
                y0 = 0x10; y1 = 0x10; x0 = 0xc; x2 = 0xc; x1 = 0x7c; x3 = 0x7c; uSpan = 0xff; y2 = 0xf0; y3 = 0xf0;
                u0 = 0x80; u2 = 0x80;
                break;
            case 2:
                y0 = 0x10; y1 = 0x10; x1 = 0x8a; x3 = 0x8a; uSpan = 0x8d; x0 = 8; x2 = 8; y2 = 0xf0; y3 = 0xf0;
                u0 = 0; u2 = 0;
                break;
            case 3:
                y0 = 0x10; y1 = 0x10; x0 = 0xc; x2 = 0xc; x1 = 0x6e; x3 = 0x6e; uSpan = 0xff; y2 = 0xf0; y3 = 0xf0;
                u0 = 0x8e; u2 = 0x8e;
                break;
            case 4:
                y0 = 0x10; y1 = 0x10; x1 = 0x5c; x3 = 0x5c; uSpan = 0x5f; x0 = 0xc; x2 = 0xc; y2 = 0xf0; y3 = 0xf0;
                u0 = 0; u2 = 0;
                break;
            case 5:
                y0 = 0x10; y1 = 0x10; x0 = 8; x2 = 8; x1 = 0x9c; x3 = 0x9c; uSpan = 0xff; y2 = 0xf0; y3 = 0xf0;
                u0 = 0x60; u2 = 0x60;
                break;
            case 6:
                y0 = 8; y1 = 8; x1 = 0x120; x3 = 0x120; uSpan = 0xff; y2 = 0xe8; y3 = 0xe8; x0 = 0x20; x2 = 0x20;
                u0 = 0; u2 = 0;
                break;
            default:
                // BLOCKED: geometry/UV fields left untouched here in the original (never produced
                // by the real 20-entry CreditsPictureEntry table, whose layoutIndex only spans 0-5).
                x0 = y0 = x1 = y1 = x2 = y2 = x3 = y3 = 0;
                u0 = u2 = 0;
                uSpan = 0;
                hasGeometry = false;
                break;
        }

        // v0 = v1 = 0 (INCONNU: never written by this function, see plate comment above).
        const byte v0 = 0, v1 = 0;
        var u1 = uSpan;
        const byte v2 = 0xf0, v3 = 0xf0;
        var u3 = uSpan;

        // tPage = GetTPage(2,0,0x140,0) selects the shared picture VRAM rect - not needed once
        // rendering samples _currentCreditsPictureBitmap directly.
        var fadeLevel = (byte)g_creditsFadeLevel;

        if (!hasGeometry || _currentCreditsPictureBitmap == null)
        {
            return;
        }

        var texW = (float)_currentCreditsPictureBitmap.Width;
        var texH = (float)_currentCreditsPictureBitmap.Height;
        renderer.DrawDeformedQuad(
            _currentCreditsPictureBitmap,
            x0, y0, u0 / texW, v0 / texH,
            x1, y1, u1 / texW, v1 / texH,
            x2, y2, u2 / texW, v2 / texH,
            x3, y3, u3 / texW, v3 / texH,
            SpriteDepth.BackgroundUI,
            fadeLevel, fadeLevel, fadeLevel);
    }

    // GHIDRA: DrawCreditsTextQuad @ 0x80022124
    // SOURCE: Ghidra (ReVa get-decompilation).
    // JUSTIFICATION: PSX hardware adaptation only (for the final draw call)
    // RELATION: same OT/primitive-buffer -> IRenderer.DrawDeformedQuad adaptation as
    // UpdateAndDrawCreditsFade, sampling a Bitmap built from g_creditsTextCanvas instead of VRAM.
    private void DrawCreditsTextQuad(int textBrightnessParam)
    {
        var brightness = (byte)textBrightnessParam;

        short x0 = g_creditsTextX;
        short y0 = g_creditsScrollY;
        short x1 = (short)(g_creditsTextX + 0xb0);
        short y1 = g_creditsScrollY;
        short x2 = g_creditsTextX;
        short y2 = (short)(g_creditsScrollY + 0xf0);
        short x3 = (short)(g_creditsTextX + 0xb0);
        short y3 = (short)(g_creditsScrollY + 0xf0);

        // u1=u3=0xb0, u0=u2=0, v0=v1=0 (never written, same cross-frame-persistence caveat as
        // UpdateAndDrawCreditsFade), v2=v3=0xf0. Canvas is 256 wide / 240 tall.
        const float texW = 256f;
        const float texH = 240f;

        if (_creditsTextCanvasDirty || _creditsTextCanvasBitmap == null)
        {
            _creditsTextCanvasBitmap?.Dispose();
            _creditsTextCanvasBitmap = BuildBitmapFromNibbleCanvas(g_creditsTextCanvas, 256, 240, _creditsFontPalette);
            _creditsTextCanvasDirty = false;
        }

        renderer.DrawDeformedQuad(
            _creditsTextCanvasBitmap,
            x0, y0, 0f / texW, 0f / texH,
            x1, y1, 0xb0 / texW, 0f / texH,
            x2, y2, 0f / texW, 0xf0 / texH,
            x3, y3, 0xb0 / texW, 0xf0 / texH,
            SpriteDepth.BackgroundUI,
            brightness, brightness, brightness);
    }

    // GHIDRA: UpdateCreditsTextSequencer @ 0x80021d88
    // SOURCE: Ghidra (ReVa get-decompilation), plate comment CERTAIN (confirmed via main()'s call
    // site) for the phase state machine; CORRECTION comment for CreditsBlockEntry.nameType (proven
    // by DrawCreditsTextLine's signature to be a start-X indentation selector, not a font style).
    private void UpdateCreditsTextSequencer()
    {
        // JUSTIFICATION: C# language bridge only
        // RELATION: original walks g_creditsLineCountScrollTable with raw pointer arithmetic into
        // a 12-int local (`local_48`), stopping at a fixed end-of-table marker; expressed here as a
        // direct array copy of the same 12 known entries (no unsafe pointers in this port).
        var local48 = new int[12];
        Array.Copy(g_creditsLineCountScrollTable, local48, 12);

        if (g_creditsBlockTimer == 0)
        {
            var blockIndex = (int)g_creditsBlockIndex;
            var block = g_creditsBlockTable[blockIndex];
            if (block.BlockType == 0)
            {
                if (g_creditsBlockPhase != 4)
                {
                    g_creditsBlockPhase = 4;
                    g_creditsBlockTimer = 0x78;
                }
            }
            else
            {
                g_creditsBlockTimer = 0x200;
                Array.Clear(g_creditsTextCanvas);
                var entryCount = 0;
                short textHeightAccum = 0;
                for (var entryIndex = 0; entryIndex < block.Entries.Length; entryIndex++)
                {
                    var currentEntry = block.Entries[entryIndex];
                    if (currentEntry.IsEmpty)
                    {
                        break;
                    }

                    // CORRECTION (proof: DrawCreditsTextLine's proven signature): this is the
                    // start-X indentation of the text in the 4bpp canvas, not a font style -
                    // entryNameType 0 = 0px (title, full margin), 1 = 0x20px (very indented
                    // sub-line), other = 8px (standard indentation).
                    short startX;
                    var entryNameType = currentEntry.NameType;
                    if (entryNameType == 0)
                    {
                        startX = 0;
                        textHeightAccum = (short)(textHeightAccum + 0x18);
                    }
                    else
                    {
                        startX = entryNameType == 1 ? (short)0x20 : (short)8;
                        textHeightAccum = (short)(textHeightAccum + 0x10);
                    }

                    entryCount++;
                    DrawCreditsTextLine(ReadNulTerminatedString(currentEntry.NamePtrFileOffset), g_creditsTextCanvas, 0x280, 0, startX, textHeightAccum, 0x100, 0xf0);
                }

                // JUSTIFICATION: C# language bridge only
                // RELATION: see _creditsTextCanvasDirty declaration - g_creditsTextCanvas was just
                // cleared and redrawn above, so DrawCreditsTextQuad's cached Bitmap is stale.
                _creditsTextCanvasDirty = true;

                g_creditsTextX = (short)(g_creditsBlockIndex == 0 ? 100 : 0x90);
                g_creditsBlockExtraHoldFlag = block.Unknown0x04;
                g_creditsScrollY = 0xf0;
                g_creditsBlockTextHeight = (short)(textHeightAccum + 0x10);
                g_creditsBlockLineCount = (short)entryCount;
                g_creditsBlockPhase = 0;
                g_creditsBlockIndex = (short)(g_creditsBlockIndex + 1);
                g_creditsBlockType = (ushort)block.BlockType;
            }
        }
        else
        {
            g_creditsBlockTimer = (short)(g_creditsBlockTimer - 1);
            switch (g_creditsBlockPhase)
            {
                case 0:
                    g_creditsScrollY = (short)(g_creditsScrollY - 1);
                    if (g_creditsScrollY == 0xe0 && (g_creditsBlockType == 1 || g_creditsBlockType == 3))
                    {
                        LoadNextCreditsPicture();
                    }

                    if (g_creditsScrollY == local48[g_creditsBlockLineCount])
                    {
                        g_creditsBlockPhase++;
                        g_creditsBlockTimer = g_creditsBlockExtraHoldFlag == 0 ? (short)0 : (short)0x3c;
                    }
                    break;

                case 1:
                    if (g_creditsBlockTimer == 0)
                    {
                        g_creditsBlockPhase++;
                        g_creditsBlockTimer = (short)(g_creditsBlockTextHeight + local48[g_creditsBlockLineCount] + 0x1e);
                    }
                    break;

                case 2:
                    g_creditsScrollY = (short)(g_creditsScrollY - 1);
                    // (ushort)(blockType - 2) < 2 : original's unsigned-wraparound trick to test
                    // "blockType is 2 or 3" without a second comparison - kept literal.
                    if (g_creditsBlockTimer == 0x23 && (ushort)(g_creditsBlockType - 2) < 2)
                    {
                        g_creditsFadeSpeed = -4;
                    }
                    break;

                case 3:
                    if (g_creditsBlockTimer == 0 && g_creditsBlockType != 4)
                    {
                        LoadNextCreditsPicture();
                    }
                    break;

                case 4:
                    if (g_creditsBlockTimer == 0)
                    {
                        LoadNextCreditsPicture();
                        g_creditsSequenceDone = 1;
                    }
                    break;
            }
        }
    }

    // GHIDRA: LoadNextCreditsPicture @ 0x80021cd4
    // SOURCE: Ghidra (ReVa get-decompilation).
    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: OpenTIM/ReadTIM/LoadImage/DrawSync (VRAM DMA upload of the freshly-opened TIM into
    // the shared picture VRAM rect) collapse into fetching the decoded Bitmap kept in
    // _currentCreditsPictureBitmap - matches the already-established LoadImage-is-a-no-op
    // precedent (AlundraEngine.Text.Font3, UI.UIManager) since there's no separate desktop VRAM to
    // push pixel data into. Routed through Inspector.LoadImage (already caches by index) instead of
    // decoding the TIM again: the picture table's 20 entries reuse only 10 distinct TIM resources
    // (e.g. the same picture shown under two different layoutIndex presets), and Inspector already
    // decoded and cached all of them while extracting closing_00..12.png.
    private void LoadNextCreditsPicture()
    {
        var tableEntry = CreditsPictureEntry_ARRAY_8003a28c[g_creditsPictureTablePtr];
        DAT_801b6b54 = tableEntry.Unknown0x00;
        g_creditsPictureTablePtr++;

        _currentCreditsPictureBitmap = tableEntry.InspectorImageIndex >= 0
            ? _inspector.LoadImage(tableEntry.InspectorImageIndex)
            : LoadTimFromFileOffset(tableEntry.TimDataFileOffset);

        g_creditsPictureLayoutIndex = tableEntry.LayoutIndex;
        g_creditsFadeSpeed = 4;
        g_creditsFadeLevel = 0;
        g_creditsFadeActive = 1;
    }

    // GHIDRA: LoadCreditsFont @ 0x80022288
    // SOURCE: Ghidra (ReVa get-decompilation), plate comment CERTAIN.
    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: OpenTIM/ReadTIM/LoadImage/DrawSync collapse into reading the same TIM resource's
    // raw (undecoded) pixel bytes + palette via TimLoader.LoadTimRaw, matching Inspector image
    // index 10 (g_creditsFontTimData @ 0x801664bc; both resolve to the identical CLOSING.EXE file
    // offset - verified by cross-checking Inspector's independently signature-scanned offset
    // against RAM-address-minus-0x8001F800 for this address).
    private void LoadCreditsFont()
    {
        var fontTimFileOffset = _inspector.GetImageFileOffset(10);
        using var stream = new MemoryStream(_exeBytes, fontTimFileOffset, _exeBytes.Length - fontTimFileOffset, writable: false);
        using var br = new BinaryReader(stream);
        var raw = TimLoader.LoadTimRaw(br);
        g_creditsFontBitmapPtr = raw.ImgData;
        _creditsFontPalette = raw.Palettes?[0] ?? [];
    }

    // GHIDRA: DrawCreditsTextLine @ 0x80021954
    // SOURCE: Ghidra (ReVa get-decompilation), plate comment CERTAIN + RUNTIME-CONFIRMED (PCSX-Redux
    // live RAM read of g_creditsGlyphTable, 2026-07-11): ASCII 0-127 charset, row-stride 0x80.
    // JUSTIFICATION: PSX hardware adaptation only (for the LoadImage/DrawSync tail call)
    // RELATION: the original uploads destCanvas to VRAM after every line; on desktop that's a
    // no-op (destCanvas already holds the byte[] that DrawCreditsTextQuad reads directly), matching
    // the established LoadImage-is-a-no-op precedent used elsewhere in this codebase.
    private void DrawCreditsTextLine(byte[] text, byte[] destCanvas, short destVramX, short destVramY, short startXParam, short startY, short canvasWidth, short canvasHeight)
    {
        var startX = startXParam;
        var textPos = 0;

        if (text.Length > 0 && text[0] != 0)
        {
            var rowByteStride = (int)canvasWidth;
            do
            {
                var fontBitmap = g_creditsFontBitmapPtr;
                var charCode = text[textPos];
                var glyph = g_creditsGlyphTable[charCode];
                var glyphSheetX = glyph.SheetX;
                var glyphByteOffset = glyph.SheetRow * 0x80 + (int)glyphSheetX / 2;

                for (var glyphRowIdx = 0; glyphRowIdx < glyph.Height; glyphRowIdx++)
                {
                    if (glyph.Width <= 0)
                    {
                        continue;
                    }

                    var destRowY = startY + glyphRowIdx;
                    var srcRowByteOffset = glyphRowIdx * 0x80;
                    uint srcColumn = glyphSheetX & 1;
                    uint destColumn = (uint)(int)startX;

                    for (var glyphColIdx = 0; glyphColIdx < glyph.Width; glyphColIdx++)
                    {
                        if (0xff < (int)destColumn)
                        {
                            break;
                        }

                        byte destKeptNibble;
                        byte srcPixelNibble;
                        var destByteIndex = (rowByteStride * (destRowY + glyph.YOffset)) / 2 + (int)destColumn / 2;
                        if ((destColumn & 1) == 0)
                        {
                            destKeptNibble = (byte)(destCanvas[destByteIndex] & 0xf0);
                            srcPixelNibble = (srcColumn & 1) == 0
                                ? (byte)(fontBitmap[srcRowByteOffset + (int)srcColumn / 2 + glyphByteOffset] & 0xf)
                                : (byte)(fontBitmap[srcRowByteOffset + (int)srcColumn / 2 + glyphByteOffset] >> 4);
                        }
                        else
                        {
                            destKeptNibble = (byte)(destCanvas[destByteIndex] & 0xf);
                            srcPixelNibble = (srcColumn & 1) == 0
                                ? (byte)(fontBitmap[srcRowByteOffset + (int)srcColumn / 2 + glyphByteOffset] << 4)
                                : (byte)(fontBitmap[srcRowByteOffset + (int)srcColumn / 2 + glyphByteOffset] & 0xf0);
                        }

                        destCanvas[destByteIndex] = (byte)(destKeptNibble | srcPixelNibble);
                        destColumn++;
                        srcColumn++;
                    }
                }

                var justDrawnChar = text[textPos];
                textPos++;
                startX = (short)(g_creditsGlyphTable[justDrawnChar].Width + startX + 1);
            } while (textPos < text.Length && text[textPos] != 0);
        }
    }

    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: adapter for the PsyQ GetTPage() SDK macro/function (Ghidra address below); same
    // formula already ported once as GameInitializer.GetClut, duplicated here per-file rather than
    // shared since ClosingEngine's globals/helpers are required to stay local to this file.
    // GHIDRA: GetTPage @ 0x80024e50
    // SOURCE: Ghidra (ReVa get-decompilation). The GetGraphType()!=1/2 fallback branch (PRIM_OBJ_A4)
    // is PSX video-mode compatibility plumbing, not reachable in the desktop port.
    private static ushort GetTPage(int tp, int abr, int x, int y)
    {
        return (ushort)(((tp & 3) << 7) | ((abr & 3) << 5) | ((y & 0x100) >> 4) | ((x & 0x3ff) >> 6) | ((y & 0x200) << 2));
    }

    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: adapter for the PsyQ GetClut() SDK macro; same formula already ported once as
    // GameInitializer.GetClut, duplicated here per-file for the same reason as GetTPage above.
    private static ushort GetClut(int x, int y)
    {
        return (ushort)((y << 6) | ((x >> 4) & 0x3f));
    }

    // JUSTIFICATION: C# language bridge only
    // RELATION: converts the nibble-packed 4bpp VRAM-format canvas (or any equal-layout buffer,
    // e.g. the font sheet) into an ARGB Bitmap for IRenderer - the adapted replacement for a real
    // VRAM texture-page read, done once per draw instead of once per LoadImage upload.
    private static Bitmap BuildBitmapFromNibbleCanvas(byte[] nibbleData, int width, int height, Color[] palette)
    {
        var bitmap = new Bitmap(width, height);
        var bytesPerRow = width / 2;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var b = nibbleData[y * bytesPerRow + x / 2];
                var index = (x & 1) == 0 ? b & 0xf : b >> 4;
                bitmap.SetPixel(x, y, index < palette.Length ? palette[index] : Color.Black);
            }
        }

        return bitmap;
    }

    // JUSTIFICATION: C# language bridge only
    // RELATION: CreditsPictureEntry/CreditsBlockEntry ported their RAM pointers as CLOSING.EXE file
    // offsets (see CreditsPictureEntry.TimDataFileOffset); this decodes the TIM resource found at
    // such an offset the same way Inspector.LoadImage does for the 13 catalogued images.
    private Bitmap LoadTimFromFileOffset(int fileOffset)
    {
        using var stream = new MemoryStream(_exeBytes, fileOffset, _exeBytes.Length - fileOffset, writable: false);
        using var br = new BinaryReader(stream);
        return TimLoader.LoadTim(br);
    }

    // JUSTIFICATION: C# language bridge only
    // RELATION: resolves a CreditsBlockEntry.NamePtrFileOffset (see that type) to the actual
    // NUL-terminated byte string stored at that file offset - the desktop equivalent of
    // dereferencing the original `void *namePtr` directly.
    private byte[] ReadNulTerminatedString(int fileOffset)
    {
        var end = fileOffset;
        while (end < _exeBytes.Length && _exeBytes[end] != 0)
        {
            end++;
        }

        var result = new byte[end - fileOffset + 1];
        Array.Copy(_exeBytes, fileOffset, result, 0, end - fileOffset);
        return result;
    }

    // GHIDRA: CreditsPictureEntry_ARRAY_8003a28c @ 0x8003a28c
    // SOURCE: Ghidra (ReVa get-data: "CreditsPictureEntry[20]"), 20 entries x 0xC bytes, closed.
    private CreditsPictureEntry[] ReadCreditsPictureTable()
    {
        const uint ramAddress = 0x8003a28c;
        var fileOffset = (int)(ramAddress - RamToFileOffsetDelta);
        using var stream = new MemoryStream(_exeBytes, fileOffset, _exeBytes.Length - fileOffset, writable: false);
        using var br = new BinaryReader(stream);
        var entries = new CreditsPictureEntry[21];
        for (var i = 0; i < entries.Length; i++)
        {
            entries[i] = new CreditsPictureEntry(br, RamToFileOffsetDelta);
            entries[i].InspectorImageIndex = ResolveInspectorImageIndex(entries[i].TimDataFileOffset);
        }

        return entries;
    }

    // JUSTIFICATION: C# language bridge only
    // RELATION: see CreditsPictureEntry.InspectorImageIndex.
    private int ResolveInspectorImageIndex(int fileOffset)
    {
        for (var i = 0; i < Inspector.ImageCount; i++)
        {
            if (_inspector.GetImageFileOffset(i) == fileOffset)
            {
                return i;
            }
        }

        return -1;
    }

    // GHIDRA: g_creditsBlockTable @ 0x80039894
    // SOURCE: Ghidra (ReVa read-memory of the first 4 blocks), blockType==0 sentinel confirmed by
    // UpdateCreditsTextSequencer's own decompiled check; length not fixed in the original either
    // (walked until the sentinel), so read the same way here instead of a hardcoded count.
    private CreditsBlock[] ReadCreditsBlockTable()
    {
        const uint ramAddress = 0x80039894;
        var fileOffset = (int)(ramAddress - RamToFileOffsetDelta);
        using var stream = new MemoryStream(_exeBytes, fileOffset, _exeBytes.Length - fileOffset, writable: false);
        using var br = new BinaryReader(stream);
        var blocks = new List<CreditsBlock>();
        while (true)
        {
            var block = new CreditsBlock(br, RamToFileOffsetDelta);
            blocks.Add(block);
            if (block.BlockType == 0)
            {
                break;
            }
        }

        return blocks.ToArray();
    }

    // GHIDRA: g_creditsGlyphTable @ 0x80038e94
    // SOURCE: Ghidra (ReVa get-data), GlyphMetrics[128], closed by direct read.
    private GlyphMetrics[] ReadCreditsGlyphTable()
    {
        const uint ramAddress = 0x80038e94;
        var fileOffset = (int)(ramAddress - RamToFileOffsetDelta);
        using var stream = new MemoryStream(_exeBytes, fileOffset, _exeBytes.Length - fileOffset, writable: false);
        using var br = new BinaryReader(stream);
        var glyphs = new GlyphMetrics[128];
        for (var i = 0; i < glyphs.Length; i++)
        {
            glyphs[i] = new GlyphMetrics(br);
        }

        return glyphs;
    }
}
