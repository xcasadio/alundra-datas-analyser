using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;
using System;
using System.Diagnostics;

namespace AlundraEngine.UI;

public class UIManager
{
    private readonly GameEngine _gameEngine;

    public UIManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    //80048304
    public void Func_80048304(CallBackInfo callBackInfo)
    {
        //TODO : same as InitializeTextSpriteTiles ? => 8005a0c8
        int tileX;
        int tileY;
        TextTilesConfiguration tilesConfiguration;
        int surfaceIndex;

        surfaceIndex = 0;
        StaticVariables.g_etcDisplayFlags = 0;
        tilesConfiguration = callBackInfo.Data;

        do
        {
            tileY = 0;

            if (0 < tilesConfiguration.Height)
            {
                do
                {
                    tileX = 0;

                    if (0 < tilesConfiguration.Width)
                    {
                        SPRT[] sprites = surfaceIndex == 0 ? tilesConfiguration.SpritesA : tilesConfiguration.SpritesB;

                        do
                        {
                            var tileIndex = tileY * tilesConfiguration.Width + tileX;
                            var sprite = sprites[tileIndex];
                            //SetSprt(sprite);
                            //SetSemiTrans(sprite, 0);
                            //SetShadeTex(sprite, 1);
                            sprite.clut = StaticVariables.g_clutTable[0]; //TODO Font3.Palettes[0];

                            tileX += 1;
                        } while (tileX < tilesConfiguration.Width);
                    }

                    tileY += 1;

                } while (tileY < tilesConfiguration.Height);
            }

            surfaceIndex += 1;

        } while (surfaceIndex < 2);
    }

    //80046ef0
    public void Func_80046ef0(CallBackInfo callBackInfo)
    {
        //Debugger.Break();
        int y;
        int height;
        //DISPENV local_28;

        //GetDispEnv(&local_28);
        y = callBackInfo.Y + callBackInfo.Data.Y + -1;

        if (0xef < y)
        {
            y = 0xef;
        }

        height = callBackInfo.Height * 8 + 2;

        if (0xef < y + height)
        {
            height = 0xf0 - y;
        }

        //var x = local_28.disp.x + callBackInfo.X + callBackInfo.Data.X;
        //var y2 = local_28.disp.y + (short)y;
        //var h = (short)height;
        //var w = callBackInfo.Width * 8 + 2;

        //SetDrawArea((DR_AREA*)(&DAT_80153010 + g_drawModes[0x14].tag * 0xc), &local_28.disp);

        if ((StaticVariables.g_warpFlags_2 & 3) == 0)
        {
            if (StaticVariables.g_textPrimitives == 0)
            {
                if (StaticVariables.g_textMessageConfirmed != 0)
                {
                    FUN_80045988(callBackInfo);
                    return; // 1;
                }

                TextInterpreter();
            }
            else
            {
                ProcessEtcTextAdvance();
            }
        }
        else
        {
            y = RenderTextTilesStep(callBackInfo.Data, StaticVariables.g_textToDisplay);

            if (y == 1)
            {
                if ((StaticVariables.g_warpFlags_2 & 1) != 0)
                {
                    StaticVariables.g_warpFlags_2 = (int)(StaticVariables.g_warpFlags_2 & 0xfffffffe);
                }

                if ((StaticVariables.g_warpFlags_2 & 2) != 0)
                {
                    StaticVariables.g_textTilesConfiguration2.X = StaticVariables.g_textToDisplay.originX;
                    StaticVariables.g_textTilesConfiguration2.Y = StaticVariables.g_textToDisplay.originY;
                    FUN_8004501c(callBackInfo);
                    return; // 0;
                }
            }
        }

        RenderText(callBackInfo);
        return; // 1;
    }

    //80045988
    private void FUN_80045988(CallBackInfo callbackInfo)
    {
        Debugger.Break();
    }

    //8004b770
    public void Func_8004b770(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //8004bea4
    public void Func_8004bea4(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //8004f628
    public void Func_8004f628(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //800501fc
    public void Func_800501fc(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //800537f0
    public void Func_800537f0(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //80053328
    public void Func_80053328(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //8004a8a8
    public void Func_8004a8a8(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //80054f1c
    public void Func_80054f1c(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //8004afe8
    public void Func_8004afe8(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //80050ec8
    public void Func_80050ec8(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //800583ec
    public void Func_800583ec(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //80051550
    public void Func_80051550(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //8005a268
    public void Func_8005a268(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //8005a3e0
    public void Func_8005a3e0(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //80045e60
    private void ProcessEtcTextAdvance()
    {
        byte shouldAdvance = 0;

        if ((StaticVariables.g_etcAnimationMode & 2U) != 0)
        {
            shouldAdvance = (byte)(StaticVariables.g_padState1.ButtonsJustPressed >> 7);
        }

        if ((StaticVariables.g_etcAnimationMode & 1U) != 0)
        {
            StaticVariables.INT_80149cc4 += -1;

            if (StaticVariables.INT_80149cc4 == 0)
            {
                shouldAdvance = 1;
            }
        }

        if ((StaticVariables.g_etcAnimationMode & 4U) != 0 && StaticVariables.g_textHoldState_2 == 1)
        {
            shouldAdvance = 1;
            StaticVariables.g_textHoldState_2 = 0;
        }

        if (shouldAdvance != 0)
        {
            StaticVariables.g_warpFlags_2 |= 2;

            _gameEngine.SoundManager.PlaySoundEffect(7);
            ResetEtcTextAnimationState();
            UpdateCameraTransitionState();

            StaticVariables.g_textToDisplay.mode = 2;
            StaticVariables.g_textToDisplay.tick = 0;
            StaticVariables.g_textToDisplay.speed = 0xf;

            if (StaticVariables.g_textTilesConfiguration2.X < 0)
            {
                StaticVariables.g_textToDisplay.x = (short)(StaticVariables.g_textTilesConfiguration2.X + StaticVariables.g_textTilesConfiguration2.Width * -8);
            }
            else
            {
                StaticVariables.g_textToDisplay.x = StaticVariables.g_textTilesConfiguration2.X;
            }

            if (StaticVariables.g_textTilesConfiguration2.Y < 0)
            {
                StaticVariables.g_textToDisplay.y = (short)(StaticVariables.g_textTilesConfiguration2.Y + StaticVariables.g_textTilesConfiguration2.Height * -8);
            }

            else
            {
                StaticVariables.g_textToDisplay.y = StaticVariables.g_textTilesConfiguration2.Y;
            }

            if (StaticVariables.g_textTilesConfiguration2.X < 0)
            {
                StaticVariables.g_textToDisplay.startX = (short)(StaticVariables.g_textTilesConfiguration2.X + StaticVariables.g_textTilesConfiguration2.Width * -8);
            }
            else
            {
                StaticVariables.g_textToDisplay.startX = StaticVariables.g_textTilesConfiguration2.X;
            }

            StaticVariables.g_textToDisplay.startY = 0xf0;
        }
    }

    //80059fe0
    private void ResetEtcTextAnimationState()
    {
        if ((StaticVariables.g_etcDisplayFlags & 4U) != 0)
        {
            StaticVariables.g_etcDisplayFlags = 6;
        }

        StaticVariables.g_textToDisplay2.mode = 2;
        StaticVariables.g_textToDisplay2.tick = 0;
        StaticVariables.g_textToDisplay2.speed = 0xf;

        if (StaticVariables.g_textTilesConfiguration.X < 0)
        {
            StaticVariables.g_textToDisplay2.x = (short)(StaticVariables.g_textTilesConfiguration.X + StaticVariables.g_textTilesConfiguration.Width * -8);
        }
        else
        {
            StaticVariables.g_textToDisplay2.x = StaticVariables.g_textTilesConfiguration.X;
        }

        if (StaticVariables.g_textTilesConfiguration.Y < 0)
        {
            StaticVariables.g_textToDisplay2.y = (short)(StaticVariables.g_textTilesConfiguration.Y + StaticVariables.g_textTilesConfiguration.Height * -8);
        }
        else
        {
            StaticVariables.g_textToDisplay2.y = StaticVariables.g_textTilesConfiguration.Y;
        }

        StaticVariables.g_textToDisplay2.startX = 0x140;

        if (StaticVariables.g_textTilesConfiguration.Y < 0)
        {
            StaticVariables.g_textToDisplay2.startY = (short)(StaticVariables.g_textTilesConfiguration.Y + StaticVariables.g_textTilesConfiguration.Height * -8);
        }
        else
        {
            StaticVariables.g_textToDisplay2.startY = StaticVariables.g_textTilesConfiguration.Y;
        }
    }

    //80057b84
    private void UpdateCameraTransitionState()
    {
        if (StaticVariables.g_cameraTransitionState != 0)
        {
            StaticVariables.g_cameraCurrentX = StaticVariables.g_cameraTransitionSrcX + 2 - StaticVariables.g_cameraTransitionDstXPtr;
            StaticVariables.g_cameraDeltaX = StaticVariables.g_cameraTransitionStartX;
            StaticVariables.g_cameraX = StaticVariables.g_cameraTransitionStartX - StaticVariables.g_cameraCurrentX;
            StaticVariables.g_cameraDeltaY = StaticVariables.g_cameraTransitionStartY;
            StaticVariables.g_cameraTransitionState = 2;
            StaticVariables.g_cameraCurrentY = 
                StaticVariables.g_cameraTransitionSrcY + 2 - StaticVariables.g_cameraTransitionDstYPtr -
                (StaticVariables.g_cameraTransitionSrcZ + 2) + -0x20;
            StaticVariables.g_cameraY = StaticVariables.g_cameraTransitionStartY - StaticVariables.g_cameraCurrentY;
            StaticVariables.g_cameraTransitionStepValue = 0xf;
        }
    }

    //80047dd0
    private int RenderTextTilesStep(TextTilesConfiguration textTilesConfiguration, TextToDisplay textToDisplay)
    {
        int result;
        int x;
        int y;
        SPRT sprt;

        if (textToDisplay.mode == 0)
        {
            result = 1;
        }
        else
        {
            y = textToDisplay.speed;

            if (textToDisplay.tick == y)
            {
                textTilesConfiguration.X = textToDisplay.startX;
                textTilesConfiguration.Y = textToDisplay.startY;
                textToDisplay.mode += -1;
            }
            else
            {
                x = (textToDisplay.startX - textToDisplay.x) * textToDisplay.tick;

                if (y == 0)
                {
                    Debugger.Break();
                    //trap(0x1c00);
                }

                if (y == -1 && x == -0x80000000)
                {
                    Debugger.Break();
                    //trap(0x1800);
                }

                textTilesConfiguration.X = (short)(textToDisplay.x + (short)(x / y));
                x = (textToDisplay.startY - textToDisplay.y) * textToDisplay.tick;
                y = textToDisplay.speed;

                if (y == 0)
                {
                    Debugger.Break();
                    //trap(0x1c00);
                }

                if (y == -1 && x == -0x80000000)
                {
                    Debugger.Break();
                    //trap(0x1800);
                }

                textTilesConfiguration.Y = (short)(textToDisplay.y + (short)(x / y));
                textToDisplay.tick += 1;
            }

            y = textTilesConfiguration.Y;
            Debugger.Break();
            var i = 0;
            result = 0;

            if (y < textTilesConfiguration.Height * 8 + y)
            {
                do
                {
                    x = textTilesConfiguration.X;

                    if (x < textTilesConfiguration.Width * 8 + x)
                    {
                        sprt = textTilesConfiguration.SpritesA[StaticVariables.g_bufferIndex + i];

                        do
                        {
                            sprt.x0 = (short)x;
                            sprt.y0 = (short)y;

                            x += 8;
                            i++;
                        } while (x < textTilesConfiguration.Width * 8 + textTilesConfiguration.X);
                    }

                    y += 8;
                    result = 0;

                } while (y < textTilesConfiguration.Height * 8 + textTilesConfiguration.Y);
            }
        }

        return result;
    }

    //8004501c
    private void FUN_8004501c(CallBackInfo callBackInfo)
    {
        FUN_80047cb0(callBackInfo);
        StaticVariables.g_warpFlags_2 = 0;
        StaticVariables.g_playerControlFlags &= 0xffffffe7;
    }

    //80047cb0
    private int FUN_80047cb0(CallBackInfo callBackInfo)
    {
        callBackInfo.Flags = 0;
        return 1;
    }

    //80045fe0
    private void TextInterpreter()
    {
        uint uVar1;
        char pcVar2;
        uint puVar3;
        int fontWidth;
        byte[] textBuffer = new byte[8];
        char[] numericString = new char[16];
        char[] acStack_1c48 = new char[2400];
        char[] acStack_12e8 = new char[2400];
        char[] acStack_988 = new char[2400];
        byte charCode;
        char currentChar;
        int currentLineIndex;
        int cursor;
        bool forceLineAdvance;
        bool shouldRender;

        shouldRender = false;
        forceLineAdvance = false;
        currentLineIndex = StaticVariables.g_textLineIndex;

        if ((StaticVariables.g_textFlags & 8) == 0)
        {
            if ((StaticVariables.g_textFlags & 2) != 0)
            {
                StaticVariables.g_textDelay += -1;

                if (StaticVariables.g_textDelay == 0)
                {
                    shouldRender = true;
                    StaticVariables.g_textDelay = StaticVariables.g_textDelayReset;
                }
            }

            if ((StaticVariables.g_textFlags & 1) != 0 
                && (StaticVariables.g_padState1.ButtonsHold & 0x80) != 0)
            {
                shouldRender = true;
            }

            if ((StaticVariables.g_textFlags & 4) != 0 
                && StaticVariables.g_textAutoAdvanceFlag == 1)
            {
                shouldRender = true;
                StaticVariables.g_textAutoAdvanceFlag = 0;
            }

            cursor = StaticVariables.g_textCursor;

            if (shouldRender)
            {
                switchD_80046540_RENDER_NEXT_CHARACTER:
                StaticVariables.g_textCursor = cursor;
                currentChar = StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor];

                if (currentChar != '\0')
                {
                    if (currentChar == '\n')
                    {
                        cursor = StaticVariables.g_textCursor + 1;
                        goto switchD_80046540_RENDER_NEXT_CHARACTER;
                    }

                    if (currentChar == '{')
                    {
                        cursor = StaticVariables.g_textCursor + 1;
                        StaticVariables.g_textCursor += 2;
                        currentLineIndex = StaticVariables.g_textBufferX + StaticVariables.g_textLineIndex >> 0x1f;
                        fontWidth = (StaticVariables.g_textBufferX + StaticVariables.g_textLineIndex) / 3 + currentLineIndex;
                        textBuffer[0] = (byte)(StaticVariables.g_scriptBuffer[cursor] + 0x50);
                        
                        LAB_8004635c:
                        textBuffer[1] = 0;

                        Debugger.Break();
                        //RenderTextBitmap(textBuffer, StaticVariables.g_textBuffer, 0x3c0,
                        //                 (short)((uint)(((StaticVariables.g_textBufferX + StaticVariables.g_textLineIndex + (fontWidth - currentLineIndex) * -3) * 0x10 + 0x120) * 0x10000) >> 0x10), 
                        //                 (short)StaticVariables.g_textLineStartX, 0, 0x100, 0x10);

                        StaticVariables.g_textLineStartX += StaticVariables.g_fontCharWidthTable[textBuffer[0] * 5];
                        return;
                    }

                    if (currentChar == '}')
                    {
                        cursor = StaticVariables.g_textCursor + 1;
                        StaticVariables.g_textCursor += 2;
                        currentLineIndex = StaticVariables.g_textBufferX + StaticVariables.g_textLineIndex >> 0x1f;
                        fontWidth = (StaticVariables.g_textBufferX + StaticVariables.g_textLineIndex) / 3 + currentLineIndex;
                        textBuffer[0] = (byte)(StaticVariables.g_scriptBuffer[cursor] + 0x90);
                        
                        Debugger.Break();
                        return;
                        //goto LAB_8004635c;
                    }

                    var currentTextCursorValue = StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor + 1];
                    
                    if (currentChar != '\\')
                    {
                        currentLineIndex = StaticVariables.g_textCursor;
                        //we want to go to the default case to simulate the goto 'LAB_80046ccc'
                        currentTextCursorValue = '@';

                        //goto LAB_80046ccc;
                        Debugger.Break();
                    }
                    else
                    {
                        currentLineIndex = StaticVariables.g_textCursor + 1;
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
                            charCode = (byte)StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor + 1];
                            StaticVariables.g_textCursor = currentLineIndex;
                            
                            while (charCode - 0x30 < 10)
                            {
                                numericString[cursor] = StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor];
                                numericString[cursor + 1] = '\0';
                                cursor += 1;
                                charCode = (byte)StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor + 1];
                                StaticVariables.g_textCursor += 1;
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

                                    pcVar2 = ' ';

                                } while (currentLineIndex < cursor + -1);
                            }

                            uVar1 = uint.Parse(numericString);
                            StaticVariables.g_globalFlags[(uVar1 >> 3) & 0xffc] |= (uint)(1 << (int)(uVar1 & 0x1f));
                            cursor = StaticVariables.g_textCursor;
                            goto switchD_80046540_RENDER_NEXT_CHARACTER;

                        case 'A':
                            StaticVariables.g_textFlags |= 8;
                            StaticVariables.g_textCursor += 2;
                            StaticVariables.g_textHoldState = 1;
                            return;

                        case 'B':
                            StaticVariables.g_currentVoiceSfxId = -1;
                            break;

                        case 'C':
                            StaticVariables.g_currentVoiceSfxId = 0;
                            cursor = StaticVariables.g_textCursor + 2;
                            goto switchD_80046540_RENDER_NEXT_CHARACTER;

                        case 'D':
                            StaticVariables.g_currentVoiceSfxId = 1;
                            break;

                        case 'E':
                            StaticVariables.g_currentVoiceSfxId = 2;
                            break;

                        case 'F':
                            StaticVariables.g_currentVoiceSfxId = 3;
                            break;

                        case 'G':
                            StaticVariables.g_currentVoiceSfxId = 4;
                            break;

                        case 'H':
                            currentLineIndex = StaticVariables.g_textCursor + 2;
                            StaticVariables.g_textCursor += 2;
                            currentLineIndex = CalculateTextWidthFromScript([StaticVariables.g_scriptBuffer[currentLineIndex]]);
                            StaticVariables.g_textLineWidth[(StaticVariables.g_textBufferX + StaticVariables.g_textLineIndex) % 3] = currentLineIndex;
                            cursor = StaticVariables.g_textCursor;
                            goto switchD_80046540_RENDER_NEXT_CHARACTER;

                        case 'M':
                            cursor = StaticVariables.g_textCursor + 2;
                            if (StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor + 2] == 'C')
                            {
                                cursor = StaticVariables.g_textCursor + 3;

                                if ((StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor + 3] == 'E'))
                                {
                                    StaticVariables.g_textFlags = 4;
                                    cursor = StaticVariables.g_textCursor + 4;
                                }
                            }

                            goto switchD_80046540_RENDER_NEXT_CHARACTER;

                        case 'N':
                            StaticVariables.g_textCursor += 2;
                            forceLineAdvance = true;
                            //goto LAB_80046e34;
                            currentLineIndex = StaticVariables.g_textLineIndex;

                            if (forceLineAdvance)
                            {
                                StaticVariables.g_textRenderStep = 0;
                                StaticVariables.g_textLineStartX = 0;
                                Array.Clear(StaticVariables.g_textBuffer);
                                currentLineIndex = StaticVariables.g_textLineIndex;

                                if (StaticVariables.g_textLineIndex == 2)
                                {
                                    StaticVariables.g_textMessageConfirmed = 1;
                                    StaticVariables.g_textChoiceIndex = StaticVariables.g_textNextChoice;

                                    if ((StaticVariables.g_debugFlags_2 & 1) != 0)
                                    {
                                        StaticVariables.g_textSelectionConfirmed = StaticVariables.g_textSelectionNext;
                                    }
                                }
                                else
                                {
                                    currentLineIndex = StaticVariables.g_textLineIndex + 1;

                                    if (StaticVariables.g_textLineIndex + 1 == 3)
                                    {
                                        currentLineIndex = StaticVariables.g_textLineIndex;
                                    }
                                }
                            }

                            StaticVariables.g_textLineIndex = currentLineIndex;
                            return;

                        case 'T':
                            StaticVariables.g_textDelay = StaticVariables.g_textDelayReset << 1;
                            StaticVariables.g_textCursor += 2;
                            return;

                        case 'V':
                            Debugger.Break();
                            break;
                            /*
                            charCode = (byte)StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor + 2];
                            StaticVariables.g_textCursor += 3;
                            strncpy(acStack_1c48, StaticVariables.g_scriptBuffer, StaticVariables.g_textCursor);
                            acStack_1c48[StaticVariables.g_textCursor] = '\0';
                            acStack_988[0] = '\0';
                            currentLineIndex = StaticVariables.INT_ARRAY_80191908[charCode - 0x30];

                            if (currentLineIndex == 0)
                            {
                                strcpy(acStack_988, PTR_DAT_8009a7ec);
                            }
                            else
                            {
                                cursor = ((currentLineIndex * 0x66666667) >> 0x20);

                                do
                                {
                                    fontWidth = (cursor >> 2) - (currentLineIndex >> 0x1f);
                                    strcpy(acStack_12e8, PTR_DAT_8009a7ec[currentLineIndex + fontWidth * -10]);
                                    strcat(acStack_12e8, acStack_988);
                                    strcpy(acStack_988, acStack_12e8);
                                    cursor = (int)((fontWidth * 0x66666667) >> 0x20);
                                    currentLineIndex = fontWidth;

                                } while (fontWidth != 0);
                            }

                            strcat(acStack_1c48, acStack_988);
                            strcat(acStack_1c48, StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor]);
                            goto LAB_800469b4;*/

                        case 'W':
                            var value = (int)currentChar - 0x20;

                            if (0x40 < (byte)StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor + 2])
                            {
                                value = -0x27;
                            }

                            textBuffer[0] = (byte)(StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor + 2] + value);
                            currentLineIndex = StaticVariables.g_textBufferX + StaticVariables.g_textLineIndex >> 0x1f;
                            fontWidth = (StaticVariables.g_textBufferX + StaticVariables.g_textLineIndex) / 3 + currentLineIndex;
                            StaticVariables.g_textCursor += 3;
                            //goto LAB_8004635c;
                            currentLineIndex = StaticVariables.g_textLineIndex;

                            if (forceLineAdvance)
                            {
                                StaticVariables.g_textRenderStep = 0;
                                StaticVariables.g_textLineStartX = 0;
                                Array.Clear(StaticVariables.g_textBuffer);
                                currentLineIndex = StaticVariables.g_textLineIndex;

                                if (StaticVariables.g_textLineIndex == 2)
                                {
                                    StaticVariables.g_textMessageConfirmed = 1;
                                    StaticVariables.g_textChoiceIndex = StaticVariables.g_textNextChoice;

                                    if ((StaticVariables.g_debugFlags_2 & 1) != 0)
                                    {
                                        StaticVariables.g_textSelectionConfirmed = StaticVariables.g_textSelectionNext;
                                    }
                                }
                                else
                                {
                                    currentLineIndex = StaticVariables.g_textLineIndex + 1;

                                    if (StaticVariables.g_textLineIndex + 1 == 3)
                                    {
                                        currentLineIndex = StaticVariables.g_textLineIndex;
                                    }
                                }
                            }

                            StaticVariables.g_textLineIndex = currentLineIndex;
                            return;

                        case 'X':
                            cursor = StaticVariables.g_textCursor + 2;
                            Debugger.Break();
                            break;
                            /*
                            switch (StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor + 2])
                            {
                                case '0':
                                    StaticVariables.g_textCursor += 3;
                                    strncpy(acStack_1c48, StaticVariables.g_scriptBuffer, StaticVariables.g_textCursor);

                                    acStack_1c48[StaticVariables.g_textCursor] = '\0';
                                    currentLineIndex = _gameEngine.PlayerManager.GetNumberOfFalconTemp();

                                    if (currentLineIndex / 10 + (currentLineIndex >> 0x1f) != currentLineIndex >> 0x1f)
                                    {
                                        currentLineIndex = _gameEngine.PlayerManager.GetNumberOfFalconTemp();
                                        strcat(acStack_1c48, PTR_DAT_8009a7ec[currentLineIndex / 10]);
                                    }

                                    currentLineIndex = _gameEngine.PlayerManager.GetNumberOfFalconTemp();
                                    strcat(acStack_1c48, PTR_DAT_8009a7ec[currentLineIndex % 10]);
                                    strcat(acStack_1c48, StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor]);
                                    break;

                                case '1':
                                    StaticVariables.g_textCursor += 3;
                                    strncpy(acStack_1c48, StaticVariables.g_scriptBuffer, StaticVariables.g_textCursor);
                                    acStack_1c48[StaticVariables.g_textCursor] = '\0';
                                    _gameEngine.PlayerManager.UpdateNumberOfFalcon();
                                    UpdatePlayerProgressState();
                                    currentLineIndex = _gameEngine.PlayerManager.GetNumberOfFalcon();

                                    if (currentLineIndex / 10 + (currentLineIndex >> 0x1f) != currentLineIndex >> 0x1f)
                                    {
                                        currentLineIndex = _gameEngine.PlayerManager.GetNumberOfFalcon();
                                        strcat(acStack_1c48, PTR_DAT_8009a7ec[currentLineIndex / 10]);
                                    }

                                    currentLineIndex = _gameEngine.PlayerManager.GetNumberOfFalcon();
                                    goto LAB_80046984;

                                case '2':
                                case '4':
                                    StaticVariables.g_textCursor += 3;
                                    strncpy(acStack_1c48, StaticVariables.g_scriptBuffer, StaticVariables.g_textCursor);
                                    acStack_1c48[StaticVariables.g_textCursor] = '\0';
                                    strcat(acStack_1c48,
                                        PTR_g_iconNameEtcBase_8009a814[StaticVariables.g_textCategoryIndex]);
                                    strcat(acStack_1c48, StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor]);
                                    break;

                                case '3':
                                    StaticVariables.g_textCursor += 3;
                                    strncpy(acStack_1c48, StaticVariables.g_scriptBuffer, StaticVariables.g_textCursor);
                                    acStack_1c48[StaticVariables.g_textCursor] = '\0';
                                    _gameEngine.PlayerManager.UpdateNumberOfFalcon();
                                    UpdatePlayerProgressState();
                                    strcat(acStack_1c48,
                                        PTR_DAT_8009a7ec[
                                            StaticVariables.g_categoryThresholdTable[
                                                StaticVariables.g_textCategoryIndex] / 10]);
                                    strcat(acStack_1c48,
                                        PTR_DAT_8009a7ec[
                                            StaticVariables.g_categoryThresholdTable[
                                                StaticVariables.g_textCategoryIndex] % 10]);
                                    strcat(acStack_1c48, StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor]);
                                    strcpy(StaticVariables.g_scriptBuffer, acStack_1c48);
                                    cursor = StaticVariables.g_textCursor;
                                    goto switchD_80046540_RENDER_NEXT_CHARACTER;

                                case '5':
                                    StaticVariables.g_textCursor += 3;
                                    strncpy(acStack_1c48, StaticVariables.g_scriptBuffer, StaticVariables.g_textCursor);
                                    acStack_1c48[StaticVariables.g_textCursor] = '\0';
                                    _gameEngine.PlayerManager.UpdateNumberOfFalcon();
                                    UpdatePlayerProgressState();
                                    currentLineIndex = _gameEngine.PlayerManager.GetNumberOfFalcon();
                                    currentLineIndex =
                                        StaticVariables.g_categoryThresholdTable[StaticVariables.g_textCategoryIndex] -
                                        currentLineIndex;

                                    if (9 < currentLineIndex)
                                    {
                                        strcat(acStack_1c48, PTR_DAT_8009a7ec[currentLineIndex / 10]);
                                    }

                                    LAB_80046984:
                                    strcat(acStack_1c48, PTR_DAT_8009a7ec[currentLineIndex % 10]);
                                    strcat(acStack_1c48, StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor]);
                                    
                                    LAB_800469b4:
                                    strcpy(StaticVariables.g_scriptBuffer, acStack_1c48);
                                    cursor = StaticVariables.g_textCursor;
                                    goto switchD_80046540_RENDER_NEXT_CHARACTER;

                                default:
                                    goto switchD_80046540_RENDER_NEXT_CHARACTER;
                            }
                            strcpy(StaticVariables.g_scriptBuffer, acStack_1c48);
                            _gameEngine.PlayerManager.UpdateNumberOfFalcon();
                            UpdatePlayerProgressState();
                            cursor = StaticVariables.g_textCursor;
                            goto switchD_80046540_RENDER_NEXT_CHARACTER;
                            */

                        case 'Y':
                            StaticVariables.g_textCursor += 2;
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
                            StaticVariables.g_textCursor = currentLineIndex;
                            shouldRender = ContainsSpecialTextFormatting(StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor]);

                            if (shouldRender == false)
                            {
                                textBuffer[0] = (byte)StaticVariables.g_scriptBuffer[StaticVariables.g_textCursor];
                                StaticVariables.g_textCursor += 1;
                            }
                            else
                            {
                                //DoNothing();
                                //DoNothing();
                                //DoNothing();
                                textBuffer[0] = 0x3f;
                                StaticVariables.g_textCursor += 2;
                            }

                            textBuffer[1] = 0;

                            Debugger.Break();
                            //RenderTextBitmap(textBuffer, StaticVariables.g_textBuffer, 0x3c0,
                            //                 (short)((uint)(((StaticVariables.g_textBufferX + StaticVariables.g_textLineIndex) % 3 * 0x10 + 0x120) *
                            //                               0x10000) >> 0x10), (short)StaticVariables.g_textLineStartX, 0, 0x100, 0x10);

                            StaticVariables.g_textLineStartX += StaticVariables.g_fontCharWidthTable[(uint)textBuffer[0] * 5];

                            if ((StaticVariables.g_textRenderStep & 1) == 0
                                && StaticVariables.g_currentVoiceSfxId != 4
                                && -1 < StaticVariables.g_currentVoiceSfxId)
                            {
                                _gameEngine.SoundManager.PlaySoundEffect((uint)(StaticVariables.g_currentVoiceSfxId + 0x4f));
                            }

                            StaticVariables.g_textRenderStep += 1;

                            //goto LAB_80046e34;
                            currentLineIndex = StaticVariables.g_textLineIndex;

                            if (forceLineAdvance)
                            {
                                StaticVariables.g_textRenderStep = 0;
                                StaticVariables.g_textLineStartX = 0;
                                Array.Clear(StaticVariables.g_textBuffer);
                                currentLineIndex = StaticVariables.g_textLineIndex;

                                if (StaticVariables.g_textLineIndex == 2)
                                {
                                    StaticVariables.g_textMessageConfirmed = 1;
                                    StaticVariables.g_textChoiceIndex = StaticVariables.g_textNextChoice;

                                    if ((StaticVariables.g_debugFlags_2 & 1) != 0)
                                    {
                                        StaticVariables.g_textSelectionConfirmed = StaticVariables.g_textSelectionNext;
                                    }
                                }
                                else
                                {
                                    currentLineIndex = StaticVariables.g_textLineIndex + 1;

                                    if (StaticVariables.g_textLineIndex + 1 == 3)
                                    {
                                        currentLineIndex = StaticVariables.g_textLineIndex;
                                    }
                                }
                            }

                            StaticVariables.g_textLineIndex = currentLineIndex;
                            return;
                            //LAB_80046e34
                    }
                    cursor = StaticVariables.g_textCursor + 2;
                    goto switchD_80046540_RENDER_NEXT_CHARACTER;

                }

                StaticVariables.g_textPrimitives = 1;
                currentLineIndex = StaticVariables.g_textLineIndex;

                if ((StaticVariables.g_etcAnimationMode & 1U) != 0)
                {
                    StaticVariables.INT_80149cc4 = StaticVariables.g_textBufferSize;
                }
            }
        }
        else
        {
            forceLineAdvance = true;

            if ((StaticVariables.g_padState1.ButtonsJustPressed & 0x80) != 0)
            {
                StaticVariables.g_textHoldState = 0;
                StaticVariables.g_textFlags = (int)(StaticVariables.g_textFlags & 0xfffffff7);
                StaticVariables.g_debugFlags_2 |= 8;

                LAB_80046e34:
                currentLineIndex = StaticVariables.g_textLineIndex;

                if (forceLineAdvance)
                {
                    StaticVariables.g_textRenderStep = 0;
                    StaticVariables.g_textLineStartX = 0;
                    Array.Clear(StaticVariables.g_textBuffer);
                    currentLineIndex = StaticVariables.g_textLineIndex;

                    if (StaticVariables.g_textLineIndex == 2)
                    {
                        StaticVariables.g_textMessageConfirmed = 1;
                        StaticVariables.g_textChoiceIndex = StaticVariables.g_textNextChoice;

                        if ((StaticVariables.g_debugFlags_2 & 1) != 0)
                        {
                            StaticVariables.g_textSelectionConfirmed = StaticVariables.g_textSelectionNext;
                        }
                    }
                    else
                    {
                        currentLineIndex = StaticVariables.g_textLineIndex + 1;

                        if (StaticVariables.g_textLineIndex + 1 == 3)
                        {
                            currentLineIndex = StaticVariables.g_textLineIndex;
                        }
                    }
                }
            }
        }

        StaticVariables.g_textLineIndex = currentLineIndex;
    }

    //8004f304
    private bool ContainsSpecialTextFormatting(char c)
    {
        int result;

        result = c << 8;
        //Krom2RawAdd();
        return result != -1;
    }

    //8004771c
    private int CalculateTextWidthFromScript(char[] text)
    {
        char pbVar1;
        int fontWidth;
        int totalWidth;
        char currentChar;
        int index = 0;

        totalWidth = 0;
        currentChar = text[0];

        while (currentChar != 0)
        {
            if (currentChar == 0x7b)
            {
                fontWidth = StaticVariables.g_fontCharWidthTable[(text[1] + 0x50) * 5];
                index += 2;
                LAB_800478a0:
                totalWidth = totalWidth + fontWidth;
            }
            else
            {
                if (currentChar == 0x7d)
                {
                    fontWidth = StaticVariables.g_fontCharWidthTable[(text[1] + 0x90) * 5];
                    index += 2;

                    //goto LAB_800478a0;
                    totalWidth = totalWidth + fontWidth;
                }
                else if (currentChar != 0x5c)
                {
                    fontWidth = StaticVariables.g_fontCharWidthTable[(uint)currentChar * 5];
                    index += 1;

                    //goto LAB_800478a0;
                    totalWidth = totalWidth + fontWidth;
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

                            fontWidth = StaticVariables.g_fontCharWidthTable[fontWidth * 5];
                            index += 3;
                            //goto LAB_800478a0;
                            totalWidth = totalWidth + fontWidth;
                            break;

                        case 0x58:
                            index += 3;
                            break;
                    }
                }
            }

            currentChar = text[index];
        }

        return totalWidth;
    }

    //8004754c
    private void UpdatePlayerProgressState()
    {
        int currentValue;
        uint progressFlags;
        int piVar1;

        progressFlags = (uint)(StaticVariables.g_progressStateFlags & 0xfffffe01);

        if (StaticVariables.g_playerState < 0)
        {
            StaticVariables.g_progressStateFlags = (int)(progressFlags | 0x100);
            StaticVariables.g_textCategoryIndex = 7;
        }
        else
        {
            StaticVariables.g_progressStateFlags = (int)(progressFlags | 0x80);

            if ((StaticVariables.g_playerState & 0x40000000U) == 0)
            {
                StaticVariables.g_progressStateFlags = (int)(progressFlags | 0x40);

                if ((StaticVariables.g_playerState & 0x20000000U) == 0)
                {
                    StaticVariables.g_progressStateFlags = (int)(progressFlags | 0x20);

                    if ((StaticVariables.g_playerState & 0x10000000U) == 0)
                    {
                        StaticVariables.g_progressStateFlags = (int)(progressFlags | 0x10);

                        if ((StaticVariables.g_playerState & 0x8000000U) == 0)
                        {
                            StaticVariables.g_progressStateFlags = (int)(progressFlags | 8);

                            if ((StaticVariables.g_playerState & 0x4000000U) == 0)
                            {
                                StaticVariables.g_progressStateFlags = (int)(progressFlags | 4);

                                if ((StaticVariables.g_playerState & 0x2000000U) == 0)
                                {
                                    StaticVariables.g_progressStateFlags = (int)(progressFlags | 2);

                                    StaticVariables.g_textCategoryIndex = 0;
                                }
                                else
                                {
                                    StaticVariables.g_textCategoryIndex = 1;
                                }
                            }
                            else
                            {
                                StaticVariables.g_textCategoryIndex = 2;
                            }
                        }
                        else
                        {
                            StaticVariables.g_textCategoryIndex = 3;
                        }
                    }
                    else
                    {
                        StaticVariables.g_textCategoryIndex = 4;
                    }
                }
                else
                {
                    StaticVariables.g_textCategoryIndex = 5;
                }
            }
            else
            {
                StaticVariables.g_textCategoryIndex = 6;
            }
        }

        piVar1 = StaticVariables.g_categoryThresholdTable[StaticVariables.g_textCategoryIndex];
        currentValue = _gameEngine.PlayerManager.GetNumberOfFalcon();

        if (currentValue < piVar1)
        {
            StaticVariables.g_progressStateFlags = (int)(StaticVariables.g_progressStateFlags & 0xfffff7ff);
        }
        else
        {
            StaticVariables.g_progressStateFlags = StaticVariables.g_progressStateFlags | 0x800;
        }
    }

    //800455b4
    private void RenderText(CallBackInfo callBackInfo)
    {
        if (callBackInfo == null || callBackInfo.Data == null)
        {
            return;
        }

        const int MAX_LINES = 3;
        const int LINE_SPACING = 16;
        const int TILE_W = 8;
        const int TILE_H = 8;
        const int HOLD_MAX = 40;
        int bufferIndex = StaticVariables.g_bufferIndex;
        var blk = callBackInfo.Data;

        // Variables qui reproduisent la dynamique de l’ASM
        int lineYAccum = 0;  // 0, 0x10, 0x20 (LINE_SPACING)
        int primOffset = 0;  // 0x00, 0x14, 0x28 (pas mémoire dans le groupe)
        int offsetY = 0;  // décrémenté à chaque ligne (nudge vertical)

        // === Boucle sur 3 lignes « visibles » depuis g_textBufferX ===
        for (int lineIdx = 0; lineIdx < MAX_LINES; ++lineIdx)
        {
            int logical = (StaticVariables.g_textBufferX + lineIdx) % 3;

            // Largeur réelle connue ?
            var lineWidthPx = StaticVariables.g_textLineWidth[logical];
            
            // Calcul X (centrage si largeur connue)
            int startX;

            if (lineWidthPx != 0)
            {
                int targetW = blk.Width * TILE_W;       // largeur cible en pixels
                int delta = targetW - lineWidthPx;    // ce qu'il reste
                startX = blk.X + (delta >> 1);           // centré
            }
            else
            {
                // Si largeur inconnue/0, l’ASM prend la base « à gauche »
                startX = blk.X;
            }

            // Calcul Y (base + addY1 + offsetY + lineYAccum)
            var startY = blk.Y + blk.Height + offsetY + lineYAccum;

            // Écritures équivalentes aux sh @ +0x08/+0x0A dans le bloc per‑line du buffer
            StaticVariables.g_bufferTextToDisplay[bufferIndex + logical * 20] = (short)startX;
            StaticVariables.g_bufferTextToDisplay[bufferIndex + 1 + logical * 20] = (short)startY;

            // --- Chaînage des primitives de texte (équivalent addPrim dans l’ASM) ---
            // Dans le moteur original : addPrim( OrderTableSlot(buf, logical, primOffset), TextGroup(buf, logical)+primOffset )
            // Ici : on ajoute simplement la primitive correspondante dans l’OT du buffer.
            //addPrim(ot_curr(), text_group(bufferIndex, logical));

            // Prépare la suite (comme dans l’ASM)
            primOffset += 0x14;
            offsetY -= 1;
            lineYAccum += LINE_SPACING;
        }

        // Tête de groupe . OT (équivalent de l’addPrim final ASM « group head »)
        //addPrim(ot_curr(), text_head(bufferIndex));

        // === Bloc « HOLD » (indicateur) ===
        if (StaticVariables.g_textHoldState != 0)
        {
            // Compteur 0..39 (reboucle comme dans l’ASM)
            StaticVariables.g_textHoldState = (StaticVariables.g_textHoldState + 1);

            if (StaticVariables.g_textHoldState >= HOLD_MAX)
            {
                StaticVariables.g_textHoldState = 0;
            }

            //// Dans l’ASM : on quantise via 0x66666667 (division rapide) pour indexer une petite table.
            //// Ici on fait simple : un « frame >> 1 » borné à 0..15 pour piocher 2 octets (glyphes)
            //int gi = (StaticVariables.g_textHoldState >> 1) & 0x0F; // 0..15
            //uint8_t g0 = StaticVariables.g_holdGlyphLUT[gi & ~1u];  // paire alignée
            //uint8_t g1 = StaticVariables.g_holdGlyphLUT[(gi & ~1u) + 1];
            //
            //// Écrit dans un « hudTextPrimitive » (les offsets 0x0C/0x0D dans l’ASM)
            //// On le fait pour la 1re ligne « logique » par simplicité
            //const int logical0 = mod3((int)StaticVariables.g_textBufferX + 0);
            //Primitive* hud = hud_text(buf, logical0);
            //hud.bytes[0x0C] = g0;
            //hud.bytes[0x0D] = g1;
            //
            //// Positionne le petit quad de fondu (équivalents aux sh @ +0x08/+0x0A)
            //// L’ASM calcule :
            ////   Xfade = xBase + (cols*TILE_W) - 0x10
            ////   Yfade = yBase + (rows*TILE_H) - 0x18
            //const int xFade = (int)blk.xBase + ((int)blk.cols * TILE_W) - 0x10;
            //const int yFade = (int)blk.yBase + ((int)blk.rows * TILE_H) - 0x18;
            //
            //// On « encode » ces deux valeurs dans le fadePrimitive du buffer (mock)
            //Primitive* fade = fade_prim(buf);
            //// Ici, je range les coordonnées dans bytes pour montrer l’idée
            //fade.bytes[0x08] = (uint8_t)(xFade & 0xFF);
            //fade.bytes[0x09] = (uint8_t)((xFade >> 8) & 0xFF);
            //fade.bytes[0x0A] = (uint8_t)(yFade & 0xFF);
            //fade.bytes[0x0B] = (uint8_t)((yFade >> 8) & 0xFF);
            //
            //// Chaîne le HUD (texte HOLD) et le fade dans l’order table du buffer
            //addPrim(ot_curr(), hud);
            //addPrim(ot_curr(), fade);
        }
    }
}