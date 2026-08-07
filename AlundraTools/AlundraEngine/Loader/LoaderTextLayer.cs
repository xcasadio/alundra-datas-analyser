using PsxSdk.Graphics;

namespace AlundraEngine.Loader;

/// <summary>
/// One entry of LOADER.EXE's proportional font table: where a character sits in the font sheet and
/// how wide it is.
///
/// GHIDRA: struct FontCharacter (20 bytes), array g_characterPositionInSpriteSheet @ 0x80042f80,
/// 256 entries.
/// SOURCE: field roles read off FUN_800223ec @ 0x800223ec, which is the only consumer. Ghidra's
/// field names are shuffled relative to the memory order, so the mapping below is by offset:
/// +0 advance, +4 height, +8 source x, +12 source y, +16 vertical adjustment.
/// </summary>
/// <param name="Width">Advance in pixels, which is also the blit width.</param>
/// <param name="Height">Blit height in pixels.</param>
/// <param name="SourceX">Left edge inside the font sheet.</param>
/// <param name="SourceY">Top edge inside the font sheet.</param>
/// <param name="YOffset">Added to the cursor's y before blitting.</param>
public readonly record struct LoaderFontCharacter(int Width, int Height, int SourceX, int SourceY, int YOffset);

/// <summary>
/// The font sheet every text layer draws from.
///
/// GHIDRA: TileMap_8014f060 (the sheet) and g_characterPositionInSpriteSheet @ 0x80042f80 (the
/// table), both globals in LOADER.EXE.
/// </summary>
public sealed class LoaderFont
{
    /// <summary>GHIDRA: TileMap_8014f060 — the layer holding g_loadRoomFontTim.</summary>
    public LoaderTileMap Sheet { get; } = new();

    /// <summary>GHIDRA: g_characterPositionInSpriteSheet @ 0x80042f80.</summary>
    public LoaderFontCharacter[] Characters { get; private set; } = [];

    /// <summary>
    /// GHIDRA: ResetGraphicsState @ 0x80022160.
    /// </summary>
    /// <remarks>
    /// CORRECTION: the Ghidra name is wrong. The function resets nothing — it points the font layer
    /// at ETC resource "TIM" #4 (image #3, g_loadRoomFontTim, 256x256 4bpp), forces palette entry 0
    /// to 0 so it reads as transparent, and uploads the sheet to VRAM (0x2C0, 0x100) with its CLUT
    /// at (0x100, 0x1E1) — the same CLUT the three text boxes sample. Renamed
    /// <c>InitFontTileMap</c> in Ghidra.
    ///
    /// The write to palette entry 0 mutates the TIM in place. The buffer here is the loaded
    /// LOADER.EXE image, which is exactly what the original mutates too (the resource is resident
    /// in RAM), so the effect is identical and idempotent.
    /// </remarks>
    public void InitFontTileMap(PsxVram vram, byte[] exeBytes, int fontTimOffset, LoaderFontCharacter[] characters)
    {
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(exeBytes);

        Characters = characters;
        Sheet.InitializeTileLayer(exeBytes, fontTimOffset);

        if (Sheet.ClutBuffer is not null)
        {
            Sheet.ClutBuffer[Sheet.ClutOffset] = 0;
            Sheet.ClutBuffer[Sheet.ClutOffset + 1] = 0;
        }

        Sheet.SetTileLayerBounds(vram, 0x2C0, 0x100, 0x100, 0x1E1, 0);
    }
}

/// <summary>
/// A text layer: a cursor walking a string and rasterising its glyphs into a
/// <see cref="LoaderTileMap"/>, which a <see cref="UiBox"/> then displays.
///
/// GHIDRA: the 32-byte descriptor SetTextLayer @ 0x800221d4 builds, LOADER.EXE.
///
/// JUSTIFICATION: C# language bridge only.
/// RELATION: the original reuses the 36-byte UIBox structure as this descriptor, so its fields carry
/// meanings that have nothing to do with a UI box — the first four bytes hold a TileMap pointer, the
/// next four the linked UIBox, and so on. Reusing <see cref="UiBox"/> here would carry that pun into
/// C# for no benefit; the fields below are the same eight values, named for what they are, with
/// their original offsets recorded.
/// </summary>
public sealed class LoaderTextLayer
{
    /// <summary>+0x00, written by SetTextLayer as the low/high halves of its second argument.</summary>
    public LoaderTileMap? Target;

    /// <summary>+0x04, SetTextLayer's third argument.</summary>
    public UiBox? LinkedBox;

    /// <summary>+0x08, the 32-bit value Ghidra shows as <c>(tileMap->clut).w</c> and <c>.h</c>.</summary>
    public int CursorX;

    /// <summary>+0x0C, Ghidra's <c>clutData</c> — an int in pixels, not a pointer.</summary>
    public int CursorY;

    /// <summary>+0x10, Ghidra's <c>pMode</c> — the string being typed out.</summary>
    public byte[]? Text;

    /// <summary>Index of the next byte to consume in <see cref="Text"/>.</summary>
    public int TextPosition;

    /// <summary>+0x14, the value the per-character delay reloads from. SetTextLayer zeroes it.</summary>
    public int CharacterDelayReload;

    /// <summary>+0x18, frames left before the next character.</summary>
    public int CharacterDelay;

    /// <summary>+0x1C, rows left to scroll after the cursor ran off the bottom.</summary>
    public int ScrollCounter;

    private readonly LoaderFont _font;
    private readonly PsxVram _vram;
    private readonly Action<int>? _playSoundEffect;

    public LoaderTextLayer(LoaderFont font, PsxVram vram, Action<int>? playSoundEffect = null)
    {
        _font = font;
        _vram = vram;
        _playSoundEffect = playSoundEffect;
    }

    /// <summary>GHIDRA: SetTextLayer @ 0x800221d4.</summary>
    public void SetTextLayer(LoaderTileMap target, UiBox linkedBox)
    {
        Target = target;
        LinkedBox = linkedBox;
        CursorX = 0;
        CursorY = 0;
        Text = null;
        TextPosition = 0;
        CharacterDelayReload = 0;
        CharacterDelay = 0;
        ScrollCounter = 0;
    }

    /// <summary>
    /// GHIDRA: SetupTransitionLayer @ 0x80022a20 — blanks the target layer and puts the cursor back
    /// at its origin. Renamed <c>ClearTextLayer</c>: it has nothing to do with transitions.
    /// </summary>
    public void ClearTextLayer(int sync)
    {
        if (Target is null)
        {
            return;
        }

        for (var y = 0; y < Target.HeightPixels; y++)
        {
            for (var x = 0; x < Target.WidthPixels; x++)
            {
                Target.SetTileMapPixel(x, y, 0);
            }
        }

        if (sync == 1)
        {
            Target.SetTileLayerBounds(_vram, -1, -1, -1, -1, 0);
        }

        CursorY = 0;
        CursorX = 0;
    }

    /// <summary>
    /// GHIDRA: FUN_800223ec @ 0x800223ec — draws one glyph at the cursor and advances it.
    /// Renamed <c>DrawGlyphToTileMap</c>.
    /// </summary>
    /// <returns>The glyph's advance in pixels.</returns>
    public int DrawGlyph(int code, int sync)
    {
        if (Target is null)
        {
            return 0;
        }

        var index = code & 0xFFFF;

        if (index < 0x100)
        {
            if (index >= _font.Characters.Length)
            {
                return 0;
            }

            var character = _font.Characters[index];

            Target.BlitTransparent(
                _font.Sheet,
                CursorX,
                CursorY + character.YOffset,
                character.SourceX,
                character.SourceY,
                character.Width,
                character.Height);

            if (sync == 1)
            {
                Target.SetTileLayerBounds(_vram, -1, -1, -1, -1, 0);
            }

            CursorX += character.Width;
            return character.Width;
        }

        var advance = DrawKanjiGlyph(index, sync);
        CursorX += 1 + advance;
        return advance;
    }

    /// <summary>
    /// GHIDRA: FUN_800221fc @ 0x800221fc — draws a two-byte character from the console's kanji ROM.
    /// Renamed <c>DrawKanjiGlyphToTileMap</c>.
    /// </summary>
    /// <remarks>
    /// BLOCKED. The original calls <c>Krom2RawAdd2</c>, the PsyQ helper that resolves a Shift-JIS
    /// code to a 16x15 one-bit pattern inside the PlayStation BIOS's kanji ROM. That ROM is part of
    /// the console, not of the game, so there is nothing in LOADER.EXE, the disc or the emulator to
    /// read it from here.
    ///
    /// The rest of the function is transliterated below and runs as soon as a pattern source is
    /// supplied through <see cref="KanjiPatternProvider"/>: the pattern is expanded into a 16x16
    /// grid (row 15 is blanked), then every set bit writes palette index 6 at (x, y) and index 3 at
    /// the three neighbours (x+1, y), (x, y+1) and (x+1, y+1) — a one-pixel drop shadow, drawn in
    /// that order so each glyph pixel overwrites the shadow its left and upper neighbours laid down.
    ///
    /// This path is unreachable on the France build: its strings escape every accented character as
    /// <c>{</c> or <c>}</c> plus a byte, which lands back in the 256-entry sheet, so no byte ever
    /// reaches 0x80. See §6.5 of the plan.
    /// </remarks>
    public int DrawKanjiGlyph(int code, int sync)
    {
        if (Target is null)
        {
            return 0;
        }

        var pattern = KanjiPatternProvider?.Invoke(code);
        if (pattern is null)
        {
            return 0;
        }

        var baseX = CursorX;
        var baseY = CursorY;
        var bits = new byte[256];
        var write = 0;

        for (var row = 0; row < 15; row++)
        {
            var word = (ushort)((pattern[row * 2] << 8) | pattern[row * 2 + 1]);
            for (var column = 0; column < 16; column++)
            {
                bits[write++] = (word & 0x8000) == 0 ? (byte)0 : (byte)1;
                word <<= 1;
            }
        }

        // The original blanks 17 cells, not 16: its loop counts down from 15 to -1 inclusive. Only
        // the first 16 fall inside the grid, the seventeenth writes one past it into a 256-byte
        // stack buffer that is 256 cells long, so it is harmless there and dropped here.
        for (var column = 0; column < 16; column++)
        {
            bits[write++] = 0;
        }

        for (var row = 0; row < 16; row++)
        {
            for (var column = 0; column < 16; column++)
            {
                if (bits[row * 16 + column] != 1)
                {
                    continue;
                }

                var x = baseX + column;
                var y = baseY + row;
                Target.SetTileMapPixel(x, y, 6);
                Target.SetTileMapPixel(x + 1, y, 3);
                Target.SetTileMapPixel(x, y + 1, 3);
                Target.SetTileMapPixel(x + 1, y + 1, 3);
            }
        }

        if (sync == 1)
        {
            Target.SetTileLayerBounds(_vram, -1, -1, -1, -1, 0);
        }

        return 0x11;
    }

    /// <summary>
    /// Supplies the 30-byte, 16x15 one-bit pattern for a two-byte character code, standing in for
    /// <c>Krom2RawAdd2</c>. Null — the default — leaves <see cref="DrawKanjiGlyph"/> blocked.
    /// </summary>
    public static Func<int, byte[]?>? KanjiPatternProvider { get; set; }

    /// <summary>
    /// GHIDRA: SetSpriteImage @ 0x80022518 — draws a whole string in one call.
    /// </summary>
    /// <remarks>
    /// CORRECTION: the Ghidra name is wrong — the function draws text, not a sprite image. Renamed
    /// <c>DrawTextToLayer</c>.
    ///
    /// The markup it understands, shared with <see cref="Advance"/>:
    /// <c>\N</c> and <c>\A</c> start a new line, <c>\W</c> plus a hex digit selects glyph 0x10..0x1F,
    /// any other <c>\</c> pair is consumed and ignored, <c>{</c> plus a byte selects that byte plus
    /// 0x50, <c>}</c> plus a byte selects that byte plus 0x90, a byte of 0x80 or more starts a
    /// two-byte big-endian code, and anything else is its own code.
    /// </remarks>
    public void DrawText(byte[] text, int offset, int sync)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (Target is null)
        {
            return;
        }

        var position = offset;

        while (true)
        {
            if (position >= text.Length)
            {
                return;
            }

            var current = text[position];
            if (current == 0)
            {
                if (sync == 1)
                {
                    Target.SetTileLayerBounds(_vram, -1, -1, -1, -1, 0);
                }

                return;
            }

            int code;
            int next;

            if (current == 0x5C)
            {
                var escape = text[position + 1];
                next = position + 2;

                if (escape is 0x4E or 0x41)
                {
                    CursorX = 0;
                    CursorY += 16;
                    position = next;
                    continue;
                }

                if (escape < 0x4F)
                {
                    position = next;
                    continue;
                }

                if (escape != 0x57)
                {
                    // The original computes code 0x20 here and then falls through without drawing:
                    // only the \W branch reaches the draw. Kept as the same no-op.
                    position = next;
                    continue;
                }

                code = HexDigitToGlyph(text[next]);
                next = position + 3;
            }
            else if (current < 0x80)
            {
                code = current;
                next = position + 1;

                if (code == 0x7B)
                {
                    code = text[next] + 0x50;
                    next = position + 2;
                }
                else if (code == 0x7D)
                {
                    code = text[next] + 0x90;
                    next = position + 2;
                }
            }
            else
            {
                code = ((current << 8) | text[position + 1]) & 0xFFFF;
                next = position + 2;
            }

            DrawGlyph(code, 0);
            position = next;
        }
    }

    /// <summary>
    /// GHIDRA: FUN_800227ac @ 0x800227ac — one frame of the typewriter: scroll a row, wait out the
    /// per-character delay, or consume and draw the next character. Renamed
    /// <c>AdvanceTextLayerTypewriter</c>.
    /// </summary>
    public void Advance()
    {
        if (Target is null)
        {
            return;
        }

        if (ScrollCounter != 0)
        {
            ScrollCounter--;
            CursorY--;
            Target.ScrollUpOneLine();
            Target.SetTileLayerBounds(_vram, -1, -1, -1, -1, 0);
            return;
        }

        CharacterDelay--;
        if (CharacterDelay != -1)
        {
            return;
        }

        CharacterDelay = CharacterDelayReload;

        if (Text is null || TextPosition >= Text.Length)
        {
            return;
        }

        var current = Text[TextPosition];
        if (current == 0)
        {
            return;
        }

        // GHIDRA: `if (((uint)pbVar5 & 1) != 0) PlaySoundEffect(0x4f);` - the typing blip is gated on
        // the *address* of the character being odd, which for a string in a buffer loaded at an even
        // address is the parity of its offset. Text and TextPosition index the whole ETC_RES.R
        // buffer for exactly that reason, so this reproduces the original's alternation.
        if ((TextPosition & 1) != 0)
        {
            _playSoundEffect?.Invoke(0x4F);
        }

        int code;
        int next;

        if (current == 0x5C)
        {
            var escape = Text[TextPosition + 1];
            next = TextPosition + 2;

            if (escape is 0x4E or 0x41)
            {
                CursorX = 0;
                CursorY += 16;

                if (Target.HeightPixels <= CursorY)
                {
                    ScrollCounter = 0x10;
                }

                TextPosition = next;
                return;
            }

            if (escape < 0x4F || escape != 0x57)
            {
                TextPosition = next;
                return;
            }

            code = HexDigitToGlyph(Text[next]);
            next = TextPosition + 3;
        }
        else if (current < 0x80)
        {
            code = current;
            next = TextPosition + 1;

            if (code == 0x7B)
            {
                code = Text[next] + 0x50;
                next = TextPosition + 2;
            }
            else if (code == 0x7D)
            {
                code = Text[next] + 0x90;
                next = TextPosition + 2;
            }
        }
        else
        {
            code = ((current << 8) | Text[TextPosition + 1]) & 0xFFFF;
            next = TextPosition + 2;
        }

        DrawGlyph(code, 1);
        TextPosition = next;
    }

    /// <summary>True while the layer still has characters left to type out.</summary>
    /// <remarks>
    /// GHIDRA: every wait loop in the selection screen spells this
    /// <c>while (*(char *)g_uiSlot1._16_4_ != '\0')</c> — the text pointer walks forward as
    /// characters are consumed, so it lands on the terminator when the message is complete.
    /// </remarks>
    public bool IsTyping => Text is not null && TextPosition < Text.Length && Text[TextPosition] != 0;

    /// <summary>
    /// GHIDRA: the <c>\W</c> switch, identical in FUN_800227ac and SetSpriteImage — '0'..'9' map to
    /// 0x10..0x19 and 'A'..'F' to 0x1A..0x1F. Anything else leaves the code at 0x20.
    /// </summary>
    private static int HexDigitToGlyph(byte digit) => digit switch
    {
        >= 0x30 and <= 0x39 => 0x10 + (digit - 0x30),
        >= 0x41 and <= 0x46 => 0x1A + (digit - 0x41),
        _ => 0x20,
    };
}
