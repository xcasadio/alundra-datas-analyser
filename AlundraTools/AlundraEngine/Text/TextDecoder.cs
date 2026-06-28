using AlundraEngine.Graphics;
using static AlundraEngine.Graphics.Renderer;

namespace AlundraEngine.Text;

public static class TextDecoder
{
    private static readonly Dictionary<string, string> Tokens = new()
    {
        { "}7", "Ç" },
        { "{k", "ù" },
        { "{i", "ù" },
        { "{B", "'" },
        { "}d", "ô" },
        { "}P", "à" },
        { "}R", "â" },
        { "}X", "è" }, //8 14
        { "}W", "ç" }, //7 14
        { "}Y", "é" }, //9 14
        { "}Z", "ê" }, //11 14
        { "}^", "î" },
        { "}¨", "ï" },
        //{ "\\N", Environment.NewLine },
    };

    private static readonly Dictionary<char, char> TokensWithoutSpecialCharacter = new()
    {
        { '7', 'Ç' },
        { 'k', 'ù' },
        { 'i', 'ù' },
        { 'B', '\'' },
        { 'd', 'ô' },
        { 'P', 'à' },
        { 'R', 'â' },
        { 'X', 'è' }, 
        { 'W', 'ç' }, 
        { 'Y', 'é' }, 
        { 'Z', 'ê' }, 
        { '^', 'î' },
        { '¨', 'ï' }
    };

    public static string DecodeString(string message)
    {
        foreach (var token in Tokens)
        {
            message = message.Replace(token.Key, token.Value);
        }

        return message;
    }

    // Convertion CP850 -> Latin-1 (ISO-8859-1) 
    static readonly Dictionary<byte, int> Cp850ToLatin1 = new()
    {
        {128, 199}, // Ç
        {129, 252}, // ü
        {130, 233}, // é
        {131, 226}, // â
        {132, 228}, // ä
        {133, 224}, // à
        {134, 229}, // å
        {135, 231}, // ç
        {136, 234}, // ê
        {137, 235}, // ë
        {138, 232}, // è
        {139, 239}, // ï
        {140, 238}, // î
        {141, 236}, // ì
        {142, 196}, // Ä
        {143, 197}, // Å
        {144, 201}, // É
        {145, 230}, // æ
        {146, 198}, // Æ
        {147, 244}, // ô
        {148, 246}, // ö
        {149, 242}, // ò
        {150, 251}, // û
        {151, 249}, // ù
        {152, 255}, // ÿ
        {153, 214}, // Ö
        {154, 220}, // Ü

        {160, 225}, // á
        {161, 237}, // í
        {162, 243}, // ó
        {163, 250}, // ú
        {164, 241}, // ñ
        {165, 209}, // Ñ

        {181, 193}, // Á
        {182, 194}, // Â
        {183, 192}, // À

        {155, 162}, // ¢
        {156, 163}, // £
        {157, 165}, // ¥
        {166, 170}, // ª
        {167, 186}, // º
        {168, 191}, // ¿
        {170, 172}, // ¬
        {171, 189}, // ½
        {172, 188}, // ¼
        {173, 161}, // ¡
        {174, 171}, // «
        {175, 187}, // »
        {184, 169}, // ©
    };

    public static char DecodeCharacter(char c)
    {
        var newValue = c;
        TokensWithoutSpecialCharacter.TryGetValue(c, out newValue);
        return newValue;
    }

    public static int ConvertCp850ToLatin1(char cp850)
    {
        int latin1 = cp850;

        if (AlundraConfiguration.Version == AlundraVersion.Usa)
        {
            return cp850 - 0x10;
        }

        if (cp850 >= 128 && Cp850ToLatin1.TryGetValue((byte)cp850, out int mapped))
        {
            latin1 = mapped;
        }

        return latin1;
    }

    // GHIDRA: TextInterpreter @ 0x80045FE0
    public static void TextInterpreter(GameEngine gameEngine)
    {
        uint uVar1;
        char pcVar2;
        int fontWidth;
        char[] textBuffer = new char[8];
        char[] numericString = new char[16];
        byte charCode;
        char currentChar;
        int currentLineIndex;
        int cursor;
        bool forceLineAdvance;
        bool shouldRender;

        shouldRender = false;
        forceLineAdvance = false;
        currentLineIndex = gameEngine.StaticVariables.g_textLineIndex;

        if ((gameEngine.StaticVariables.g_textFlags & 8) == 0)
        {
            if ((gameEngine.StaticVariables.g_textFlags & 2) != 0)
            {
                gameEngine.StaticVariables.g_textDelay += -1;

                if (gameEngine.StaticVariables.g_textDelay == 0)
                {
                    shouldRender = true;
                    gameEngine.StaticVariables.g_textDelay = gameEngine.StaticVariables.g_textDelayReset;
                }
            }

            if ((gameEngine.StaticVariables.g_textFlags & 1) != 0
                && (gameEngine.StaticVariables.g_padState1.ButtonsHold & 0x80) != 0)
            {
                shouldRender = true;
            }

            if ((gameEngine.StaticVariables.g_textFlags & 4) != 0
                && gameEngine.StaticVariables.g_textAutoAdvanceFlag == 1)
            {
                shouldRender = true;
                gameEngine.StaticVariables.g_textAutoAdvanceFlag = 0;
            }

            cursor = gameEngine.StaticVariables.g_textCursor;

            if (shouldRender)
            {
                switchD_80046540_RENDER_NEXT_CHARACTER:
                gameEngine.StaticVariables.g_textCursor = cursor;
                currentChar = gameEngine.StaticVariables.g_scriptBuffer[gameEngine.StaticVariables.g_textCursor];

                if (currentChar != '\0')
                {
                    if (currentChar == '\n')
                    {
                        cursor = gameEngine.StaticVariables.g_textCursor + 1;
                        goto switchD_80046540_RENDER_NEXT_CHARACTER;
                    }

                    if (currentChar == '{')
                    {
                        cursor = gameEngine.StaticVariables.g_textCursor + 1;
                        gameEngine.StaticVariables.g_textCursor += 2;
                        currentLineIndex = gameEngine.StaticVariables.g_textBufferX + gameEngine.StaticVariables.g_textLineIndex >> 0x1f;
                        fontWidth = (gameEngine.StaticVariables.g_textBufferX + gameEngine.StaticVariables.g_textLineIndex) / 3 + currentLineIndex;
                        textBuffer[0] = (char)(gameEngine.StaticVariables.g_scriptBuffer[cursor] + 0x50);

                    LAB_8004635c:
                        textBuffer[1] = '\0';

                        RenderTextBitmap(gameEngine, 
                            textBuffer,
                            gameEngine.UIManager.DialogLinesSprites[gameEngine.StaticVariables.g_textLineIndex % 3], //_gameEngine.StaticVariables.g_textBuffer,
                            0,
                            (short)(gameEngine.StaticVariables.g_textLineIndex % 3 * 0x10 + 0x120),
                            (short)gameEngine.StaticVariables.g_textLineStartX,
                            0, 0x10, 0x10);

                        gameEngine.StaticVariables.g_textLineStartX += gameEngine.StaticVariables.g_fontCharWidthTable[textBuffer[0] * 5];
                        return;
                    }

                    if (currentChar == '}')
                    {
                        cursor = gameEngine.StaticVariables.g_textCursor + 1;
                        gameEngine.StaticVariables.g_textCursor += 2;
                        currentLineIndex = gameEngine.StaticVariables.g_textBufferX + gameEngine.StaticVariables.g_textLineIndex >> 0x1f;
                        fontWidth = (gameEngine.StaticVariables.g_textBufferX + gameEngine.StaticVariables.g_textLineIndex) / 3 + currentLineIndex;
                        textBuffer[0] = (char)(gameEngine.StaticVariables.g_scriptBuffer[cursor] + 0x90);

                        //goto LAB_8004635c;
                        textBuffer[1] = '\0';

                        RenderTextBitmap(gameEngine, 
                            textBuffer,
                            gameEngine.UIManager.DialogLinesSprites[gameEngine.StaticVariables.g_textLineIndex % 3], //_gameEngine.StaticVariables.g_textBuffer,
                            0,
                            (short)(gameEngine.StaticVariables.g_textLineIndex % 3 * 0x10 + 0x120),
                            (short)gameEngine.StaticVariables.g_textLineStartX,
                            0, 0x10, 0x10);

                        gameEngine.StaticVariables.g_textLineStartX += gameEngine.StaticVariables.g_fontCharWidthTable[textBuffer[0] * 5];
                        return;
                    }

                    var currentTextCursorValue = gameEngine.StaticVariables.g_scriptBuffer[gameEngine.StaticVariables.g_textCursor + 1];

                    if (currentChar != '\\')
                    {
                        currentLineIndex = gameEngine.StaticVariables.g_textCursor;
                        //we want to go to the default case to simulate the goto 'LAB_80046ccc'
                        currentTextCursorValue = '@';

                        //goto LAB_80046ccc;
                        //AlundraEngine.Debug.Debugger.Breakpoint();
                    }
                    else
                    {
                        currentLineIndex = gameEngine.StaticVariables.g_textCursor + 1;
                    }

                    switch (currentTextCursorValue)
                    {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            numericString[0] = '\0';
                            cursor = 0;
                            charCode = (byte)gameEngine.StaticVariables.g_scriptBuffer[gameEngine.StaticVariables.g_textCursor + 1];
                            gameEngine.StaticVariables.g_textCursor = currentLineIndex;

                            while (charCode - 0x30 < 10)
                            {
                                numericString[cursor] = gameEngine.StaticVariables.g_scriptBuffer[gameEngine.StaticVariables.g_textCursor];
                                numericString[cursor + 1] = '\0';
                                cursor += 1;
                                charCode = (byte)gameEngine.StaticVariables.g_scriptBuffer[gameEngine.StaticVariables.g_textCursor + 1];
                                gameEngine.StaticVariables.g_textCursor += 1;
                            }

                            currentLineIndex = 0;

                            if (0 < cursor + -1)
                            {
                                do
                                {
                                    pcVar2 = numericString[currentLineIndex];
                                    currentLineIndex += 1;

                                    if (pcVar2 != '0')
                                    {
                                        break;
                                    }

                                    //AlundraEngine.Debug.Debugger.Breakpoint();
                                    pcVar2 = ' ';

                                } while (currentLineIndex < cursor + -1);
                            }

                            uVar1 = uint.Parse(numericString);
                            var index = ((uVar1 >> 3) & 0xffc) >> 2;
                            gameEngine.StaticVariables.g_temporaryFlags[index] |= (uint)(1 << (int)(uVar1 & 0x1f));
                            cursor = gameEngine.StaticVariables.g_textCursor;
                            goto switchD_80046540_RENDER_NEXT_CHARACTER;

                        case 'A':
                            gameEngine.StaticVariables.g_textFlags |= 8;
                            gameEngine.StaticVariables.g_textCursor += 2;
                            gameEngine.StaticVariables.g_textHoldState = 1;
                            return;

                        case 'B':
                            gameEngine.StaticVariables.g_currentVoiceSfxId = -1;
                            break;

                        case 'C':
                            gameEngine.StaticVariables.g_currentVoiceSfxId = 0;
                            cursor = gameEngine.StaticVariables.g_textCursor + 2;
                            goto switchD_80046540_RENDER_NEXT_CHARACTER;

                        case 'D':
                            gameEngine.StaticVariables.g_currentVoiceSfxId = 1;
                            break;

                        case 'E':
                            gameEngine.StaticVariables.g_currentVoiceSfxId = 2;
                            break;

                        case 'F':
                            gameEngine.StaticVariables.g_currentVoiceSfxId = 3;
                            break;

                        case 'G':
                            gameEngine.StaticVariables.g_currentVoiceSfxId = 4;
                            break;

                        case 'H':
                            currentLineIndex = gameEngine.StaticVariables.g_textCursor + 2;
                            gameEngine.StaticVariables.g_textCursor += 2;
                            var array = gameEngine.StaticVariables.g_scriptBuffer.Skip(currentLineIndex).TakeWhile(c => c != '\0').ToArray();
                            currentLineIndex = CalculateTextWidthFromScript(gameEngine, array);
                            gameEngine.StaticVariables.g_textLineWidth[(gameEngine.StaticVariables.g_textBufferX + gameEngine.StaticVariables.g_textLineIndex) % 3] = currentLineIndex;
                            cursor = gameEngine.StaticVariables.g_textCursor;
                            goto switchD_80046540_RENDER_NEXT_CHARACTER;

                        case 'M':
                            cursor = gameEngine.StaticVariables.g_textCursor + 2;
                            if (gameEngine.StaticVariables.g_scriptBuffer[gameEngine.StaticVariables.g_textCursor + 2] == 'C')
                            {
                                cursor = gameEngine.StaticVariables.g_textCursor + 3;

                                if (gameEngine.StaticVariables.g_scriptBuffer[gameEngine.StaticVariables.g_textCursor + 3] == 'E')
                                {
                                    gameEngine.StaticVariables.g_textFlags = 4;
                                    cursor = gameEngine.StaticVariables.g_textCursor + 4;
                                }
                            }

                            goto switchD_80046540_RENDER_NEXT_CHARACTER;

                        case 'N':
                            gameEngine.StaticVariables.g_textCursor += 2;
                            forceLineAdvance = true;
                            //goto LAB_80046e34;
                            currentLineIndex = gameEngine.StaticVariables.g_textLineIndex;

                            if (forceLineAdvance)
                            {
                                gameEngine.StaticVariables.g_textRenderStep = 0;
                                gameEngine.StaticVariables.g_textLineStartX = 0;
                                Array.Clear(gameEngine.StaticVariables.g_textBuffer);
                                currentLineIndex = gameEngine.StaticVariables.g_textLineIndex;

                                if (gameEngine.StaticVariables.g_textLineIndex == 2)
                                {
                                    gameEngine.StaticVariables.g_textMessageConfirmed = 1;
                                    gameEngine.StaticVariables.g_textChoiceIndex = gameEngine.StaticVariables.g_textNextChoice;

                                    if ((gameEngine.StaticVariables.g_debugFlags_2 & 1) != 0)
                                    {
                                        gameEngine.StaticVariables.g_textSelectionConfirmed = gameEngine.StaticVariables.g_textSelectionNext;
                                    }
                                }
                                else
                                {
                                    currentLineIndex = gameEngine.StaticVariables.g_textLineIndex + 1;

                                    if (gameEngine.StaticVariables.g_textLineIndex + 1 == 3)
                                    {
                                        currentLineIndex = gameEngine.StaticVariables.g_textLineIndex;
                                    }
                                }
                            }

                            gameEngine.StaticVariables.g_textLineIndex = currentLineIndex;
                            return;

                        case 'T':
                            gameEngine.StaticVariables.g_textDelay = gameEngine.StaticVariables.g_textDelayReset << 1;
                            gameEngine.StaticVariables.g_textCursor += 2;
                            return;

                        case 'V':
                            charCode = (byte)gameEngine.StaticVariables.g_scriptBuffer[gameEngine.StaticVariables.g_textCursor + 2];
                            gameEngine.StaticVariables.g_textCursor += 3;
                            currentLineIndex = gameEngine.StaticVariables.INT_ARRAY_80191908[charCode - 0x30];
                            InsertIntoScriptBuffer(gameEngine, 
                                gameEngine.StaticVariables.g_textCursor,
                                currentLineIndex.ToString());
                            cursor = gameEngine.StaticVariables.g_textCursor;
                            goto switchD_80046540_RENDER_NEXT_CHARACTER;

                        case 'W':
                            var value = gameEngine.StaticVariables.g_scriptBuffer[gameEngine.StaticVariables.g_textCursor + 2] - 48;
                            var caracterToDisplay = (char)(value + 16);

                            //var value = (int)currentChar - 0x20;
                            //
                            //if (0x40 < (byte)_gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor + 2])
                            //{
                            //    value = -0x27;
                            //}

                            //textBuffer[0] = (char)(_gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor + 2] + value);
                            textBuffer[0] = caracterToDisplay;

                            currentLineIndex = gameEngine.StaticVariables.g_textBufferX + gameEngine.StaticVariables.g_textLineIndex >> 0x1f;
                            fontWidth = (gameEngine.StaticVariables.g_textBufferX + gameEngine.StaticVariables.g_textLineIndex) / 3 + currentLineIndex;
                            gameEngine.StaticVariables.g_textCursor += 3;
                            //goto LAB_8004635c;
                            //RENDER_CHARACTER
                            var textY2 = gameEngine.StaticVariables.g_textLineIndex % 3 * 0x10; // + 0x120;
                            RenderTextBitmap(gameEngine,
                                textBuffer,
                                gameEngine.UIManager.DialogLinesSprites[gameEngine.StaticVariables.g_textLineIndex % 3], //_gameEngine.StaticVariables.g_textBuffer,
                                0,
                                (short)textY2,
                                (short)gameEngine.StaticVariables.g_textLineStartX,
                                0, 0x10, 0x10);

                            gameEngine.StaticVariables.g_textLineStartX += gameEngine.StaticVariables.g_fontCharWidthTable[(uint)textBuffer[0] * 5];


                            //currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;
                            //
                            //if (forceLineAdvance)
                            //{
                            //    _gameEngine.StaticVariables.g_textRenderStep = 0;
                            //    _gameEngine.StaticVariables.g_textLineStartX = 0;
                            //    Array.Clear(_gameEngine.StaticVariables.g_textBuffer);
                            //    currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;
                            //
                            //    if (_gameEngine.StaticVariables.g_textLineIndex == 2)
                            //    {
                            //        _gameEngine.StaticVariables.g_textMessageConfirmed = 1;
                            //        _gameEngine.StaticVariables.g_textChoiceIndex = _gameEngine.StaticVariables.g_textNextChoice;
                            //
                            //        if ((_gameEngine.StaticVariables.g_debugFlags_2 & 1) != 0)
                            //        {
                            //            _gameEngine.StaticVariables.g_textSelectionConfirmed = _gameEngine.StaticVariables.g_textSelectionNext;
                            //        }
                            //    }
                            //    else
                            //    {
                            //        currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex + 1;
                            //
                            //        if (_gameEngine.StaticVariables.g_textLineIndex + 1 == 3)
                            //        {
                            //            currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;
                            //        }
                            //    }
                            //}
                            //
                            //_gameEngine.StaticVariables.g_textLineIndex = currentLineIndex;
                            return;

                        case 'X':
                            switch (gameEngine.StaticVariables.g_scriptBuffer[gameEngine.StaticVariables.g_textCursor + 2])
                            {
                                case '0':
                                    gameEngine.StaticVariables.g_textCursor += 3;
                                    currentLineIndex = gameEngine.PlayerManager.GetNumberOfFalconTemp();
                                    InsertIntoScriptBuffer(gameEngine,
                                        gameEngine.StaticVariables.g_textCursor,
                                        BuildFalconNumberString(currentLineIndex, false));
                                    gameEngine.PlayerManager.UpdateNumberOfFalcon();
                                    UpdatePlayerProgressState(gameEngine);
                                    cursor = gameEngine.StaticVariables.g_textCursor;
                                    goto switchD_80046540_RENDER_NEXT_CHARACTER;

                                case '1':
                                    gameEngine.StaticVariables.g_textCursor += 3;
                                    gameEngine.PlayerManager.UpdateNumberOfFalcon();
                                    UpdatePlayerProgressState(gameEngine);
                                    currentLineIndex = gameEngine.PlayerManager.GetNumberOfFalcon();
                                    InsertIntoScriptBuffer(gameEngine,
                                        gameEngine.StaticVariables.g_textCursor,
                                        BuildFalconNumberString(currentLineIndex, false));
                                    cursor = gameEngine.StaticVariables.g_textCursor;
                                    goto switchD_80046540_RENDER_NEXT_CHARACTER;

                                case '2':
                                case '4':
                                    currentChar = gameEngine.StaticVariables.g_scriptBuffer[gameEngine.StaticVariables.g_textCursor + 2];
                                    gameEngine.StaticVariables.g_textCursor += 3;
                                    InsertIntoScriptBuffer(gameEngine,
                                        gameEngine.StaticVariables.g_textCursor,
                                        gameEngine.EtcRes.GetItemName(gameEngine.StaticVariables.g_textCategoryIndex) ?? string.Empty);
                                    gameEngine.PlayerManager.UpdateNumberOfFalcon();
                                    UpdatePlayerProgressState(gameEngine);
                                    cursor = gameEngine.StaticVariables.g_textCursor;
                                    goto switchD_80046540_RENDER_NEXT_CHARACTER;

                                case '3':
                                    gameEngine.StaticVariables.g_textCursor += 3;
                                    gameEngine.PlayerManager.UpdateNumberOfFalcon();
                                    UpdatePlayerProgressState(gameEngine);
                                    currentLineIndex = gameEngine.StaticVariables.g_categoryThresholdTable[
                                        gameEngine.StaticVariables.g_textCategoryIndex];
                                    InsertIntoScriptBuffer(gameEngine, 
                                        gameEngine.StaticVariables.g_textCursor,
                                        BuildFalconNumberString(currentLineIndex, true));
                                    cursor = gameEngine.StaticVariables.g_textCursor;
                                    goto switchD_80046540_RENDER_NEXT_CHARACTER;

                                case '5':
                                    gameEngine.StaticVariables.g_textCursor += 3;
                                    gameEngine.PlayerManager.UpdateNumberOfFalcon();
                                    UpdatePlayerProgressState(gameEngine);
                                    currentLineIndex =
                                        gameEngine.StaticVariables.g_categoryThresholdTable[gameEngine.StaticVariables.g_textCategoryIndex] -
                                        gameEngine.PlayerManager.GetNumberOfFalcon();
                                    InsertIntoScriptBuffer(gameEngine, 
                                        gameEngine.StaticVariables.g_textCursor,
                                        BuildFalconNumberString(currentLineIndex, false));
                                    cursor = gameEngine.StaticVariables.g_textCursor;
                                    goto switchD_80046540_RENDER_NEXT_CHARACTER;

                                default:
                                    cursor = gameEngine.StaticVariables.g_textCursor + 2;
                                    goto switchD_80046540_RENDER_NEXT_CHARACTER;
                            }

                        case 'Y':
                            gameEngine.StaticVariables.g_textCursor += 2;
                            return;

                        case ':':
                        case ';':
                        case '<':
                        case '=':
                        case '>':
                        case '?':
                        case '@':
                        case 'I':
                        case 'J':
                        case 'K':
                        case 'L':
                        case 'O':
                        case 'P':
                        case 'Q':
                        case 'R':
                        case 'S':
                        case 'U':
                        default:
                        LAB_80046ccc:
                            gameEngine.StaticVariables.g_textCursor = currentLineIndex;
                            shouldRender = ContainsSpecialTextFormatting(
                                gameEngine.StaticVariables.g_scriptBuffer,
                                gameEngine.StaticVariables.g_textCursor);

                            if (shouldRender)
                            {
                                // Original consumes the two-byte KROM sequence and renders '?' in this path.
                                textBuffer[0] = (char)0x3f;
                                gameEngine.StaticVariables.g_textCursor += 2;
                            }
                            else
                            {
                                textBuffer[0] = gameEngine.StaticVariables.g_scriptBuffer[gameEngine.StaticVariables.g_textCursor];
                                gameEngine.StaticVariables.g_textCursor += 1;
                            }

                            textBuffer[1] = '\0';

                            //RenderTextBitmap(textBuffer, _gameEngine.StaticVariables.g_textBuffer, 
                            //    0x3c0, (short)((uint)(((_gameEngine.StaticVariables.g_textBufferX + _gameEngine.StaticVariables.g_textLineIndex) % 3 * 0x10 + 0x120) * 0x10000) >> 0x10),
                            //    (short)_gameEngine.StaticVariables.g_textLineStartX, 0, 0x100, 0x10);

                        RENDER_CHARACTER:
                            var textY = gameEngine.StaticVariables.g_textLineIndex % 3 * 0x10; // + 0x120;
                            RenderTextBitmap(gameEngine, 
                                textBuffer,
                                gameEngine.UIManager.DialogLinesSprites[gameEngine.StaticVariables.g_textLineIndex % 3], //_gameEngine.StaticVariables.g_textBuffer,
                                0,
                                (short)textY,
                                (short)gameEngine.StaticVariables.g_textLineStartX,
                                0, 0x10, 0x10);

                            gameEngine.StaticVariables.g_textLineStartX += gameEngine.StaticVariables.g_fontCharWidthTable[(uint)textBuffer[0] * 5];

                            if ((gameEngine.StaticVariables.g_textRenderStep & 1) == 0
                                && gameEngine.StaticVariables.g_currentVoiceSfxId != 4
                                && -1 < gameEngine.StaticVariables.g_currentVoiceSfxId)
                            {
                                gameEngine.SoundManager.PlaySoundEffect((uint)(gameEngine.StaticVariables.g_currentVoiceSfxId + 0x4f));
                            }

                            gameEngine.StaticVariables.g_textRenderStep += 1;

                            //goto LAB_80046e34;
                            currentLineIndex = gameEngine.StaticVariables.g_textLineIndex;

                            if (forceLineAdvance)
                            {
                                gameEngine.StaticVariables.g_textRenderStep = 0;
                                gameEngine.StaticVariables.g_textLineStartX = 0;
                                Array.Clear(gameEngine.StaticVariables.g_textBuffer);
                                currentLineIndex = gameEngine.StaticVariables.g_textLineIndex;

                                if (gameEngine.StaticVariables.g_textLineIndex == 2)
                                {
                                    gameEngine.StaticVariables.g_textMessageConfirmed = 1;
                                    gameEngine.StaticVariables.g_textChoiceIndex = gameEngine.StaticVariables.g_textNextChoice;

                                    if ((gameEngine.StaticVariables.g_debugFlags_2 & 1) != 0)
                                    {
                                        gameEngine.StaticVariables.g_textSelectionConfirmed = gameEngine.StaticVariables.g_textSelectionNext;
                                    }
                                }
                                else
                                {
                                    currentLineIndex = gameEngine.StaticVariables.g_textLineIndex + 1;

                                    if (gameEngine.StaticVariables.g_textLineIndex + 1 == 3)
                                    {
                                        currentLineIndex = gameEngine.StaticVariables.g_textLineIndex;
                                    }
                                }
                            }

                            gameEngine.StaticVariables.g_textLineIndex = currentLineIndex;
                            return;
                            //LAB_80046e34
                    }
                    cursor = gameEngine.StaticVariables.g_textCursor + 2;
                    goto switchD_80046540_RENDER_NEXT_CHARACTER;

                }

                gameEngine.StaticVariables.g_textPrimitives = 1;
                currentLineIndex = gameEngine.StaticVariables.g_textLineIndex;

                if ((gameEngine.StaticVariables.g_etcAnimationMode & 1U) != 0)
                {
                    gameEngine.StaticVariables.INT_80149cc4 = gameEngine.StaticVariables.g_textBufferSize;
                }
            }
        }
        else
        {
            forceLineAdvance = true;

            if ((gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & 0x80) != 0)
            {
                gameEngine.StaticVariables.g_textHoldState = 0;
                gameEngine.StaticVariables.g_textFlags &= 0xfffffff7;
                gameEngine.StaticVariables.g_debugFlags_2 |= 8;

            LAB_80046e34:
                currentLineIndex = gameEngine.StaticVariables.g_textLineIndex;

                if (forceLineAdvance)
                {
                    gameEngine.StaticVariables.g_textRenderStep = 0;
                    gameEngine.StaticVariables.g_textLineStartX = 0;
                    Array.Clear(gameEngine.StaticVariables.g_textBuffer);
                    currentLineIndex = gameEngine.StaticVariables.g_textLineIndex;

                    if (gameEngine.StaticVariables.g_textLineIndex == 2)
                    {
                        gameEngine.StaticVariables.g_textMessageConfirmed = 1;
                        gameEngine.StaticVariables.g_textChoiceIndex = gameEngine.StaticVariables.g_textNextChoice;

                        if ((gameEngine.StaticVariables.g_debugFlags_2 & 1) != 0)
                        {
                            gameEngine.StaticVariables.g_textSelectionConfirmed = gameEngine.StaticVariables.g_textSelectionNext;
                        }
                    }
                    else
                    {
                        currentLineIndex = gameEngine.StaticVariables.g_textLineIndex + 1;

                        if (gameEngine.StaticVariables.g_textLineIndex + 1 == 3)
                        {
                            currentLineIndex = gameEngine.StaticVariables.g_textLineIndex;
                        }
                    }
                }
            }
        }

        gameEngine.StaticVariables.g_textLineIndex = currentLineIndex;
    }

    //800478c4
    public static void RenderTextBitmap(GameEngine _gameEngine, char[] formattedText,
        List<Sprite> sprites, //char[] buffer,
        short posX, short posY, short textWidth,
        short textLineOffset, short drawWidth, short drawHeight)
    {
        int i = 0;
        byte fontPixel;
        int bufferWidth2;
        byte bufferOffset;
        uint glyphStartBit;
        char charIndex;
        uint fontColumn;
        uint fontPixelOffset;
        int lineByteOffset;
        int drawY;
        int iVar1;
        int glyphRow;
        int bufferWidth;
        Rectangle drawRect = new Rectangle();
        short posX_;

        if (formattedText[i] != '\0' && i < formattedText.Length)
        {
            bufferWidth = (int)drawWidth;
            posX_ = posX;

            do
            {
                charIndex = formattedText[i];

                if (charIndex == '\0')
                {
                    break;
                }

                //glyphRow = 0;
                //glyphStartBit = (uint)_gameEngine.StaticVariables.g_fontCharWidthTable[charIndex * 5 + 2];
                //bufferWidth2 = _gameEngine.StaticVariables.g_fontCharWidthTable[charIndex * 5 + 3] * 0x80 + (int)glyphStartBit / 2 + -0x7feb52d8;
                //
                //if (0 < _gameEngine.StaticVariables.g_fontCharWidthTable[charIndex * 5 + 1])
                //{
                //    do
                //    {
                //        iVar1 = 0;
                //
                //        if (0 < _gameEngine.StaticVariables.g_fontCharWidthTable[charIndex * 5])
                //        {
                //            drawY = textLineOffset + glyphRow;
                //            lineByteOffset = glyphRow * 0x80;
                //            fontColumn = glyphStartBit & 1;
                //            fontPixelOffset = (uint)textWidth;
                //
                //            do
                //            {
                //                if (0xff < (int)fontPixelOffset)
                //                {
                //                    break;
                //                }
                //
                //                if ((fontPixelOffset & 1) == 0)
                //                {
                //                    bufferOffset = (byte)(buffer[bufferWidth *
                //                                                 (drawY + _gameEngine.StaticVariables.g_fontCharWidthTable
                //                                                     [formattedText[i] * 5 + 4]) / 2 +
                //                                                 (int)fontPixelOffset / 2] & 0xf0);
                //
                //                    if ((fontColumn & 1) == 0)
                //                    {
                //                        fontPixel = (byte)((bufferWidth2 + lineByteOffset + (int)fontColumn / 2) & 0xf);
                //                    }
                //                    else
                //                    {
                //                        fontPixel = (byte)((bufferWidth2 + lineByteOffset + (int)fontColumn / 2) >> 4);
                //                    }
                //                }
                //                else
                //                {
                //                    bufferOffset = (byte)(buffer[bufferWidth * (drawY + _gameEngine.StaticVariables.g_fontCharWidthTable[charIndex * 5 + 4]) / 2 + (int)fontPixelOffset / 2] & 0xf);
                //
                //                    if ((fontColumn & 1) == 0)
                //                    {
                //                        fontPixel = (byte)(((bufferWidth2 + lineByteOffset + (int)fontColumn / 2) & 0xf) << 4);
                //                    }
                //                    else
                //                    {
                //                        fontPixel = (byte)((bufferWidth2 + lineByteOffset + (int)fontColumn / 2) & 0xf0);
                //                    }
                //                }
                //
                //                buffer[bufferWidth * (drawY + _gameEngine.StaticVariables.g_fontCharWidthTable[formattedText[i] * 5 + 4]) / 2 + (int)fontPixelOffset / 2] = (char)(bufferOffset | fontPixel);
                //                fontPixelOffset += 1;
                //                charIndex = formattedText[i];
                //                iVar1 += 1;
                //                fontColumn += 1;
                //
                //            } while (iVar1 < _gameEngine.StaticVariables.g_fontCharWidthTable[charIndex * 5]);
                //        }
                //
                //        charIndex = formattedText[i];
                //        glyphRow += 1;
                //
                //    } while (glyphRow < _gameEngine.StaticVariables.g_fontCharWidthTable[charIndex * 5 + 1]);
                //}

                bufferWidth2 = bufferWidth;

                if (bufferWidth < 0)
                {
                    bufferWidth2 = bufferWidth + 3;
                }

                drawRect.X = posX_;
                drawRect.Width = (short)(bufferWidth2 >> 2);
                drawRect.Height = drawHeight;
                drawRect.Y = posY;
                //LoadImage(drawRect, buffer);
                //DrawSync(0);

                //Modify
                //create a sprite with the character
                if (charIndex != ' ')
                {
                    var charValue = TextDecoder.ConvertCp850ToLatin1(charIndex);

                    var bitmap = _gameEngine.Font3.GenerateFontBitmapTim(
                        charValue % 16 * 16,
                        charValue / 16 * 16,
                        16, 16, 8);

                    sprites.Add(new Sprite(
                        posX_ + textWidth, posY + textLineOffset * 16,
                        16, 16, SpriteDepth.ForegroundUI, bitmap));
                }

                i++;
                textWidth = (short)(textWidth + _gameEngine.StaticVariables.g_fontCharWidthTable[charIndex * 5]);
            } while (charIndex != '\0' && i < formattedText.Length);
        }
    }


    // JUSTIFICATION: C# language bridge only
    private static void InsertIntoScriptBuffer(GameEngine _gameEngine, int insertionIndex, string insertedText)
    {
        if (string.IsNullOrEmpty(insertedText))
        {
            return;
        }

        char[] scriptBuffer = _gameEngine.StaticVariables.g_scriptBuffer;
        char[] rebuiltBuffer = new char[scriptBuffer.Length];
        int scriptLength = Array.IndexOf(scriptBuffer, '\0');

        if (scriptLength < 0)
        {
            scriptLength = scriptBuffer.Length;
        }

        int prefixLength = Math.Min(insertionIndex, scriptLength);
        Array.Copy(scriptBuffer, rebuiltBuffer, prefixLength);

        int insertedLength = Math.Min(insertedText.Length, rebuiltBuffer.Length - 1 - prefixLength);
        insertedText.AsSpan(0, insertedLength).CopyTo(rebuiltBuffer.AsSpan(prefixLength));

        int suffixLength = Math.Min(
            scriptLength - prefixLength,
            rebuiltBuffer.Length - 1 - prefixLength - insertedLength);

        if (suffixLength > 0)
        {
            Array.Copy(
                scriptBuffer,
                prefixLength,
                rebuiltBuffer,
                prefixLength + insertedLength,
                suffixLength);
        }

        Array.Clear(scriptBuffer);
        Array.Copy(rebuiltBuffer, scriptBuffer, rebuiltBuffer.Length);
    }

    // JUSTIFICATION: C# language bridge only
    private static string BuildFalconNumberString(int value, bool alwaysTwoDigits)
    {
        value = Math.Max(value, 0);
        int tens = value / 10;
        int ones = value % 10;

        if (alwaysTwoDigits || tens != 0)
        {
            return string.Concat((char)('0' + tens), (char)('0' + ones));
        }

        return ((char)('0' + ones)).ToString();
    }

    // GHIDRA: FUN_8004F304 @ 0x8004F304
    public static bool ContainsSpecialTextFormatting(char[] text, int startIndex)
    {
        if (text == null || startIndex < 0 || startIndex + 1 >= text.Length)
        {
            return false;
        }

        char first = text[startIndex];
        char second = text[startIndex + 1];

        return (first == '{' || first == '}')
            && TextDecoder.DecodeCharacter(second) != second;
    }

    //8004f304
    public static bool ContainsSpecialTextFormatting(char c)
    {
        int result;

        result = c << 8;
        //Krom2RawAdd();
        return result != -1;
    }

    //8004771c
    public static int CalculateTextWidthFromScript(GameEngine gameEngine, char[] text)
    {
        if (text == null || text?.Length == 0)
        {
            return 0;
        }

        char pbVar1;
        int fontWidth;
        int totalWidth;
        char currentChar;
        int index = 0;

        totalWidth = 0;
        currentChar = text[0];

        while (currentChar != 0 && index < text.Length)
        {
            if (currentChar == 0x7b)
            {
                fontWidth = gameEngine.StaticVariables.g_fontCharWidthTable[(text[1] + 0x50) * 5];
                index += 2;
            LAB_800478a0:
                totalWidth += fontWidth;
            }
            else
            {
                if (currentChar == 0x7d)
                {
                    fontWidth = gameEngine.StaticVariables.g_fontCharWidthTable[(text[1] + 0x90) * 5];
                    index += 2;

                    //goto LAB_800478a0;
                    totalWidth += fontWidth;
                }
                else if (currentChar != 0x5c)
                {
                    fontWidth = gameEngine.StaticVariables.g_fontCharWidthTable[(uint)currentChar * 5];
                    index += 1;

                    //goto LAB_800478a0;
                    totalWidth += fontWidth;
                }
                else
                {
                    currentChar = text[++index];

                    switch ((int)currentChar)
                    {
                        case 0x30:
                        case 0x31:
                        case 0x32:
                        case 0x33:
                        case 0x34:
                        case 0x35:
                        case 0x36:
                        case 0x37:
                        case 0x38:
                        case 0x39:
                            if (currentChar - 0x30 < 10)
                            {
                                index += 2;
                                currentChar = text[index];

                                do
                                {
                                    currentChar = text[++index];
                                } while (currentChar - 0x30 < 10);
                            }
                            break;

                        case 0x41:
                        case 0x4e:
                            return totalWidth;

                        case 0x42:
                        case 0x43:
                        case 0x44:
                        case 0x45:
                        case 0x46:
                        case 0x47:
                        case 0x54:
                        case 0x59:
                            index += 2;
                            break;

                        case 0x57:
                            index += 2;

                            if (currentChar < 0x41)
                            {
                                fontWidth = currentChar - 0x20;
                            }
                            else
                            {
                                fontWidth = currentChar - 0x27;
                            }

                            fontWidth = gameEngine.StaticVariables.g_fontCharWidthTable[fontWidth * 5];
                            index += 3;
                            //goto LAB_800478a0;
                            totalWidth += fontWidth;
                            break;

                        case 0x58:
                            index += 3;
                            break;
                    }
                }
            }

            if (index >= text.Length)
            {
                break;
            }

            currentChar = text[index];
        }

        return totalWidth;
    }


    //8004754c
    private static void UpdatePlayerProgressState(GameEngine gameEngine)
    {
        int currentValue;
        uint progressFlags;
        int piVar1;

        progressFlags = gameEngine.StaticVariables.g_saveData.GameFlags[0x2d] & 0xfffffe01;

        if (gameEngine.StaticVariables.g_saveData.GameFlags[0x2c] < 0)
        {
            gameEngine.StaticVariables.g_saveData.GameFlags[0x2d] = progressFlags | 0x100;
            gameEngine.StaticVariables.g_textCategoryIndex = 7;
        }
        else
        {
            gameEngine.StaticVariables.g_saveData.GameFlags[0x2d] = progressFlags | 0x80;

            if ((gameEngine.StaticVariables.g_saveData.GameFlags[0x2c] & 0x40000000U) == 0)
            {
                gameEngine.StaticVariables.g_saveData.GameFlags[0x2d] = progressFlags | 0x40;

                if ((gameEngine.StaticVariables.g_saveData.GameFlags[0x2c] & 0x20000000U) == 0)
                {
                    gameEngine.StaticVariables.g_saveData.GameFlags[0x2d] = progressFlags | 0x20;

                    if ((gameEngine.StaticVariables.g_saveData.GameFlags[0x2c] & 0x10000000U) == 0)
                    {
                        gameEngine.StaticVariables.g_saveData.GameFlags[0x2d] = progressFlags | 0x10;

                        if ((gameEngine.StaticVariables.g_saveData.GameFlags[0x2c] & 0x8000000U) == 0)
                        {
                            gameEngine.StaticVariables.g_saveData.GameFlags[0x2d] = progressFlags | 8;

                            if ((gameEngine.StaticVariables.g_saveData.GameFlags[0x2c] & 0x4000000U) == 0)
                            {
                                gameEngine.StaticVariables.g_saveData.GameFlags[0x2d] = progressFlags | 4;

                                if ((gameEngine.StaticVariables.g_saveData.GameFlags[0x2c] & 0x2000000U) == 0)
                                {
                                    gameEngine.StaticVariables.g_saveData.GameFlags[0x2d] = progressFlags | 2;

                                    gameEngine.StaticVariables.g_textCategoryIndex = 0;
                                }
                                else
                                {
                                    gameEngine.StaticVariables.g_textCategoryIndex = 1;
                                }
                            }
                            else
                            {
                                gameEngine.StaticVariables.g_textCategoryIndex = 2;
                            }
                        }
                        else
                        {
                            gameEngine.StaticVariables.g_textCategoryIndex = 3;
                        }
                    }
                    else
                    {
                        gameEngine.StaticVariables.g_textCategoryIndex = 4;
                    }
                }
                else
                {
                    gameEngine.StaticVariables.g_textCategoryIndex = 5;
                }
            }
            else
            {
                gameEngine.StaticVariables.g_textCategoryIndex = 6;
            }
        }

        piVar1 = gameEngine.StaticVariables.g_categoryThresholdTable[gameEngine.StaticVariables.g_textCategoryIndex];
        currentValue = gameEngine.PlayerManager.GetNumberOfFalcon();

        if (currentValue < piVar1)
        {
            gameEngine.StaticVariables.g_saveData.GameFlags[0x2d] &= 0xfffff7ff;
        }
        else
        {
            gameEngine.StaticVariables.g_saveData.GameFlags[0x2d] |= 0x800;
        }
    }
}