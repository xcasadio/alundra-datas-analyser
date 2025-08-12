using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;
using AlundraEngine.Graphics;
using System;
using System.Diagnostics;
using static AlundraEngine.Renderer;

namespace AlundraEngine.UI;

public class UIManager
{
    private readonly GameEngine _gameEngine;

    public UIManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    //80048304
    //Display background message text
    public void DisplayDialogBackgroundText(CallBackInfo callBackInfo)
    {
        //TODO : same as InitializeTextSpriteTiles ? => 8005a0c8
        int tileX;
        int tileY;
        UIBoxConfiguration tilesConfiguration;
        int surfaceIndex;

        surfaceIndex = 0;
        _gameEngine.StaticVariables.g_etcDisplayFlags = 0;
        tilesConfiguration = callBackInfo.Data;

        if (callBackInfo.Data == null)
        {
            return;
        }

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
                        SPRT[] sprites = tilesConfiguration.SpritesA;

                        do
                        {
                            var tileIndex = tileY * tilesConfiguration.Width + tileX;
                            var sprite = sprites[tileIndex];
                            //SetSprt(sprite);
                            //SetSemiTrans(sprite, 0);
                            //SetShadeTex(sprite, 1);
                            sprite.clut = 0; //_gameEngine.StaticVariables.g_clutTable[0];
                            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                            _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

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

        if ((_gameEngine.StaticVariables.g_warpFlags_2 & 3) == 0)
        {
            if (_gameEngine.StaticVariables.g_textPrimitives == 0)
            {
                if (_gameEngine.StaticVariables.g_textMessageConfirmed != 0)
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
            y = RenderTextTilesStep(callBackInfo.Data, _gameEngine.StaticVariables.g_textToDisplay);

            if (y == 1)
            {
                if ((_gameEngine.StaticVariables.g_warpFlags_2 & 1) != 0)
                {
                    _gameEngine.StaticVariables.g_warpFlags_2 = (int)(_gameEngine.StaticVariables.g_warpFlags_2 & 0xfffffffe);
                }

                if ((_gameEngine.StaticVariables.g_warpFlags_2 & 2) != 0)
                {
                    _gameEngine.StaticVariables.g_uiBoxDialogMessageBackground.X = _gameEngine.StaticVariables.g_textToDisplay.originX;
                    _gameEngine.StaticVariables.g_uiBoxDialogMessageBackground.Y = _gameEngine.StaticVariables.g_textToDisplay.originY;
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
        bool doAdavance;
        ulong uVar2;
        int iVar3;
        short[] psVar4;
        int index;
        int iVar5;
        uint[] puVar6;
        uint[] puVar7;
        int iVar8;
        int iVar9;
        short sVar10;
        int iVar11;
        short sVar12;
        int line;
        int iVar14;
        //RECT rect;

        iVar5 = _gameEngine.StaticVariables.g_textBufferX;

        if (_gameEngine.StaticVariables.g_textChoiceIndex == _gameEngine.StaticVariables.g_textNextChoice)
        {
            doAdavance = (_gameEngine.StaticVariables.g_debugFlags_2 & 8) != 0;

            if (doAdavance)
            {
                _gameEngine.StaticVariables.g_debugFlags_2 &= 0xfffffff7;
            }

            if ((_gameEngine.StaticVariables.g_debugFlags_2 & 2) != 0
                && (_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & 0x80) != 0)
            {
                doAdavance = true;
            }

            if ((_gameEngine.StaticVariables.g_debugFlags_2 & 1) != 0)
            {
                _gameEngine.StaticVariables.g_textSelectionConfirmed += -1;

                if (_gameEngine.StaticVariables.g_textSelectionConfirmed == 0)
                {
                    doAdavance = true;
                }
            }

            if ((_gameEngine.StaticVariables.g_debugFlags_2 & 4) != 0
                && _gameEngine.StaticVariables.g_textAutoAdvanceFlag_2 == 1)
            {
                doAdavance = true;
                _gameEngine.StaticVariables.g_textAutoAdvanceFlag_2 = 0;
            }

            if (!doAdavance)
            {
                _gameEngine.StaticVariables.g_textHoldState = 0;
                goto LAB_80045aa0;
            }
        }

        _gameEngine.StaticVariables.g_textChoiceIndex += -1;

        LAB_80045aa0:
        line = 0;
        sVar12 = 0;

        do
        {
            //uVar2 = _gameEngine.StaticVariables.g_drawModes[0x14].tag;
            iVar11 = 0;
            iVar8 = 0;
            sVar10 = 0;
            iVar3 = (iVar5 + line) % 3;
            iVar9 = iVar3/* * 0x28*/;

            do
            {
                if (_gameEngine.StaticVariables.g_textNextChoice == 0)
                {
                    Debugger.Break();
                    //trap(0x1c00);
                }

                index = (_gameEngine.StaticVariables.g_textNextChoice - _gameEngine.StaticVariables.g_textChoiceIndex) * 0x10;

                if (_gameEngine.StaticVariables.g_textNextChoice == -1 && index == -0x80000000)
                {
                    Debugger.Break();
                    //trap(0x1800);
                }

                iVar14 = index / _gameEngine.StaticVariables.g_textNextChoice;
                index = iVar8 + iVar9 /*+ uVar2 * 0x14*/;
                var sprite = _gameEngine.StaticVariables.g_textFullLinesSprites[index];

                if (_gameEngine.StaticVariables.g_textLineWidth[iVar3] == 0)
                {
                    sprite.x0 = (short)(callbackInfo.Data.X + callbackInfo.Data.Width);
                    sprite.y0 = (short)(callbackInfo.Data.Y + callbackInfo.Data.Height + sVar10 + sVar12 - iVar14);
                }
                else
                {
                    sprite.x0 = (short)(callbackInfo.Data.X + (callbackInfo.Data.Width * 8 - _gameEngine.StaticVariables.g_textLineWidth[iVar3]) / 2);
                    sprite.y0 = (short)(callbackInfo.Data.Y + callbackInfo.Data.Height + sVar10 + sVar12 - iVar14);
                }

                sprite.u0 = (byte)(line * 0x40);
                sprite.v0 = 0xD0;
                sprite.w = 0x10;
                sprite.h = 0x10;
                sprite.clut = 8; //_gameEngine.StaticVariables.g_clutTable[8];

                iVar8 += 0x14;
                sVar10 += -1;
                //puVar7 = (uint*)((int)&_gameEngine.StaticVariables.g_textFullLinesSprites + iVar14);
                //puVar6 = (uint*)(&DAT_80146f60 + uVar2 * 0x28);
                iVar11 += 1;
                /* Probable PsyQ macro: addPrim(). */
                //*puVar7 = *puVar7 & 0xff000000 | *puVar6 & 0xffffff;
                //*puVar6 = *puVar6 & 0xff000000 | (int)_gameEngine.StaticVariables.g_textFullLinesSprites[index] & 0xffffffU;

                var bitmap2 = _gameEngine.Font3.GenerateFontBitmapFromSprite(sprite);
                _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap2);

            } while (iVar11 < 1);

            line += 1;
            sVar12 += 0x10;
        } while (line < 3);

        //puVar6 = (uint*)(&DAT_80153010 + _gameEngine.StaticVariables.g_drawModes[0x14].tag * 0xc);
        //puVar7 = (uint*)(&DAT_80146f60 + _gameEngine.StaticVariables.g_drawModes[0x14].tag * 0x28);
        /* Probable PsyQ macro: addPrim(). */
        //*puVar6 = *puVar6 & 0xff000000 | *puVar7 & 0xffffff;
        //*puVar7 = *puVar7 & 0xff000000 | (uint)puVar6 & 0xffffff; //addPrim()


        //TODO find the sprite
        //var bitmap = _gameEngine.Font3.GenerateFontBitmapFromSprite(sprite);
        //_gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

        doAdavance = _gameEngine.StaticVariables.g_textChoiceIndex == 0;

        if (doAdavance)
        {
            _gameEngine.StaticVariables.g_textBufferX = (_gameEngine.StaticVariables.g_textBufferX + 1) % 3;
            _gameEngine.StaticVariables.g_textMessageConfirmed = 0;
            iVar5 = _gameEngine.StaticVariables.g_textBufferX + 2;
            _gameEngine.StaticVariables.g_textLineWidth[(_gameEngine.StaticVariables.g_textBufferX + _gameEngine.StaticVariables.g_textLineIndex) % 3] = 0;
            //rect.x = 0x3c0;
            //rect.y = ((short)iVar5 + ((short)((ulong)((long)iVar5 * 0x55555556) >> 0x20) - (short)(iVar5 >> 0x1f)) * -3) * 0x10 + 0x120;
            //rect.w = 0x40;
            //rect.h = 0x10;
            //ClearImage(&rect, '\0', '\0', '\0');
        }
    }

    //8004b770
    public void FUN_8004b770(CallBackInfo callBackInfo)
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
    //display yes or no dialog box
    public void FUN_800501fc(CallBackInfo callBackInfo)
    {
        long puVar1;
        int witdh;
        char pcVar2;
        short psVar3;
        int tileConfig;
        int i;
        int index;
        string[] buffer6;
        int iVar5;
        int iVar6;
        int k;
        int iVar7;
        char[] buffer = new char[256];
        int j;
        int local_34;
        int local_30;
        int local_2c;
        char c;

        tileConfig = 0;
        //sprite = _gameEngine.StaticVariables.g_sprites;
        buffer6 = _gameEngine.StaticVariables.g_asyncCallbackArgs2;
        i = 0;
        j = 0;

        while (tileConfig < 2)
        {
            var scan = _gameEngine.StaticVariables.g_asyncCallbackArgs[tileConfig];
            var w = buffer[j];

            for (; ; )
            {
                c = scan[i++];

                if (c == '{')
                {
                    buffer[j++] = (char)(scan[i++] + 0x50);
                }
                else if (c == '}')
                {
                    buffer[j++] = (char)(scan[i++] + 0x90);
                }
                else if (c == 0x00)
                {
                    buffer[j] = (char)0;
                    _gameEngine.StaticVariables.g_asyncCallbackArgs[tileConfig] = new string(buffer);
                    ++tileConfig;
                    break;
                }
                else
                {
                    buffer[j++] = c;
                }
            }
        }

        Array.Clear(_gameEngine.StaticVariables.CHAR_ARRAY_8014a4e8);
        RenderTextBitmap(_gameEngine.StaticVariables.g_asyncCallbackArgs2[0].ToCharArray(), _gameEngine.StaticVariables.CHAR_ARRAY_8014a4e8, 0x3c0, 0x1d0, 0, 0, 0x80, 0x10);
        Array.Clear(_gameEngine.StaticVariables.CHAR_ARRAY_8014a4e8);
        RenderTextBitmap(_gameEngine.StaticVariables.g_asyncCallbackArgs2[1].ToCharArray(), _gameEngine.StaticVariables.CHAR_ARRAY_8014a4e8, 0x3e0, 0x1d0, 0, 0, 0x80, 0x10);

        j = 0;
        local_2c = 0;

        do
        {
            i = 0;
            tileConfig = 0;

            do
            {
                k = 0;
                iVar6 = 0;
                iVar5 = 0;

                do
                {
                    witdh = CalculateTextWidthFromScript(_gameEngine.StaticVariables.g_asyncCallbackArgs2[i].ToCharArray());
                    index = iVar6 + tileConfig + local_2c;
                    var sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017e674[index];
                    sprite.w = (short)witdh;
                    sprite.h = 0x10;
                    sprite.u0 = (byte)(i << 7);
                    sprite.v0 = 0xd0;
                    sprite.clut = 8;//_gameEngine.StaticVariables.g_clutTable[8];
                    //sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017e674[iVar5 + local_2c + tileConfig];
                    //SetSprt(sprites);
                    //SetSemiTrans(sprites, 0);
                    //SetShadeTex(sprites, 1);

                    var bitmap = _gameEngine.Font3.GenerateFontBitmapFromSprite(sprite);
                    _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

                    iVar6 += 0x3c;
                    iVar5 += 0x3c;
                    k += 1;

                } while (k < 1);

                tileConfig += 0x3c;
                i += 1;

            } while (i < 2);

            local_2c += 0x78;
            j += 1;

        } while (j < 2);

        _gameEngine.StaticVariables.g_textToDisplay3.mode = 2;
        _gameEngine.StaticVariables.g_textToDisplay3.tick = 0;
        _gameEngine.StaticVariables.g_textToDisplay3.speed = 0xf;
        _gameEngine.StaticVariables.g_textToDisplay3.x = 0x140;
        _gameEngine.StaticVariables.g_textToDisplay3.y = callBackInfo.Data.Y;

        if (callBackInfo.Data.Y < 0)
        {
            _gameEngine.StaticVariables.g_textToDisplay3.y = (short)(_gameEngine.StaticVariables.g_textToDisplay3.y + (callBackInfo.Height + 6) * -8);
        }

        _gameEngine.StaticVariables.g_textToDisplay3.startX = callBackInfo.Data.X;

        if (callBackInfo.Data.X < 0)
        {
            _gameEngine.StaticVariables.g_textToDisplay3.startX = (short)(_gameEngine.StaticVariables.g_textToDisplay3.startX + callBackInfo.Width * -8);
        }

        _gameEngine.StaticVariables.g_textToDisplay3.startY = callBackInfo.Data.Y;

        if (callBackInfo.Data.Y < 0)
        {
            _gameEngine.StaticVariables.g_textToDisplay3.startY = (short)(_gameEngine.StaticVariables.g_textToDisplay3.startY + callBackInfo.Data.Height * -8);
        }

        _gameEngine.StaticVariables.g_textToDisplay3.originX = callBackInfo.Data.X;
        _gameEngine.StaticVariables.g_textToDisplay3.originY = callBackInfo.Data.Y;

        RenderTextTilesStep(callBackInfo.Data, _gameEngine.StaticVariables.g_textToDisplay3);

        callBackInfo.RenderFunc = FUN_800501a4;
        DisplayDialogBackgroundText(callBackInfo);
    }

    //800537f0
    public void FUN_800537f0(CallBackInfo callBackInfo)
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

    //8004afe8
    public void Fun_8004afe8(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //80050ec8
    public void Fun_80050ec8(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //800583ec
    public void FUN_800583ec(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //80051550
    public void Func_80051550(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //8005a268
    public void FUN_8005a268(CallBackInfo callBackInfo)
    {
        Debugger.Break();

        var textTileConfig = callBackInfo.Data;

        _gameEngine.StaticVariables.g_textToDisplay2.mode = 2;
        _gameEngine.StaticVariables.g_textToDisplay2.tick = 0;
        _gameEngine.StaticVariables.g_textToDisplay2.speed = 0xf;
        _gameEngine.StaticVariables.g_textToDisplay2.x = 0x140;
        _gameEngine.StaticVariables.g_textToDisplay2.y = textTileConfig.Y;
        _gameEngine.StaticVariables.g_textToDisplay2.startX = textTileConfig.X;
        _gameEngine.StaticVariables.g_textToDisplay2.startY = textTileConfig.Y;

        if (textTileConfig.X < 0)
        {
            _gameEngine.StaticVariables.g_textToDisplay2.startX = (short)(_gameEngine.StaticVariables.g_textToDisplay2.startX + textTileConfig.Width * -8);
        }

        if (textTileConfig.Y < 0)
        {
            _gameEngine.StaticVariables.g_textToDisplay2.y = (short)(_gameEngine.StaticVariables.g_textToDisplay2.y + textTileConfig.Height * -8);
            _gameEngine.StaticVariables.g_textToDisplay2.startY = (short)(_gameEngine.StaticVariables.g_textToDisplay2.startY + textTileConfig.Height * -8);
        }

        _gameEngine.StaticVariables.g_textToDisplay2.originX = textTileConfig.X;
        _gameEngine.StaticVariables.g_textToDisplay2.originY = textTileConfig.Y;
        _gameEngine.StaticVariables.g_etcDisplayFlags = 5;

        var text = _gameEngine.StaticVariables.g_entitySpriteNamesTable[_gameEngine.StaticVariables.g_entitySpriteNameTableIndex];

        _gameEngine.GraphicManager.DisplayIconName(
            _gameEngine.StaticVariables.SPRT_80180260,
            text.ToCharArray(),
            6,
            0,
            (short)(textTileConfig.Height + textTileConfig.Y),
            3);

        //return 1;
    }

    //8005a3e0
    public void Fun_8005a3e0(CallBackInfo callBackInfo)
    {
        Debugger.Break();
    }

    //800501a4
    private void FUN_800501a4(CallBackInfo callBackInfo)
    {
        int res = RenderTextTilesStep(callBackInfo.Data, _gameEngine.StaticVariables.g_textToDisplay3);
        if (res != 0)
        {
            callBackInfo.RenderFunc = FUN_8004ffa8;
        }

        FUN_8004fce8(callBackInfo);
    }

    //8004ffa8
    private void FUN_8004ffa8(CallBackInfo callBackInfo)
    {
        RenderTextTilesStep(callBackInfo.Data, _gameEngine.StaticVariables.g_textToDisplay3);

        if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & 0x40) != 0)
        {
            _gameEngine.StaticVariables.g_textToDisplay3.mode = 2;
            _gameEngine.StaticVariables.g_textToDisplay3.tick = 0;
            _gameEngine.StaticVariables.g_textToDisplay3.speed = 0xf;
            _gameEngine.StaticVariables.g_textToDisplay3.x = callBackInfo.Data.X;
            //_gameEngine.StaticVariables.g_sprites[0].tag = _gameEngine.StaticVariables.g_asyncOperationCountdown + 1;

            if (callBackInfo.Data.X < 0)
            {
                _gameEngine.StaticVariables.g_textToDisplay3.x = (short)(_gameEngine.StaticVariables.g_textToDisplay3.x + callBackInfo.Data.Width * -8);
            }

            _gameEngine.StaticVariables.g_textToDisplay3.y = callBackInfo.Data.Y;

            if (callBackInfo.Data.Y < 0)
            {
                _gameEngine.StaticVariables.g_textToDisplay3.y = (short)(_gameEngine.StaticVariables.g_textToDisplay3.y + callBackInfo.Data.Height * -8);
            }

            _gameEngine.StaticVariables.g_textToDisplay3.startX = 0x140;
            _gameEngine.StaticVariables.g_textToDisplay3.startY = callBackInfo.Data.Y;

            if (callBackInfo.Data.Y < 0)
            {
                _gameEngine.StaticVariables.g_textToDisplay3.startY = (short)(_gameEngine.StaticVariables.g_textToDisplay3.startY + callBackInfo.Data.Height * -8);
            }

            _gameEngine.SoundManager.PlaySoundEffect(5);
            uint iVar1 = 3;

            if (_gameEngine.StaticVariables.g_sprites[0].tag == 1)
            {
                iVar1 = 2;
            }

            _gameEngine.SoundManager.PlaySoundEffect(iVar1);
            callBackInfo.RenderFunc = FUN_8004fefc;
        }

        if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & 0x8000) != 0)
        {
            if (_gameEngine.StaticVariables.g_asyncOperationCountdown == 1)
            {
                _gameEngine.SoundManager.PlaySoundEffect(1);
            }

            _gameEngine.StaticVariables.g_asyncOperationCountdown = 0;
        }

        if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & 0x2000) != 0)
        {
            if (_gameEngine.StaticVariables.g_asyncOperationCountdown == 0)
            {
                _gameEngine.SoundManager.PlaySoundEffect(1);
            }

            _gameEngine.StaticVariables.g_asyncOperationCountdown = 1;
        }

        FUN_8004fce8(callBackInfo);
    }

    //8004fefc
    private void FUN_8004fefc(CallBackInfo callBackInfo)
    {
        int iVar1;

        iVar1 = RenderTextTilesStep(callBackInfo.Data, _gameEngine.StaticVariables.g_textToDisplay3);

        if (iVar1 == 1)
        {
            callBackInfo.Data.X = _gameEngine.StaticVariables.g_textToDisplay3.originX;
            callBackInfo.Data.Y = _gameEngine.StaticVariables.g_textToDisplay3.originY;
            _gameEngine.HudManager.FUN_80047cb0(callBackInfo);
            Debugger.Break();
            //_gameEngine.StaticVariables.g_asyncCallback(_gameEngine.StaticVariables.g_sprites[0].tag);
        }
        else
        {
            FUN_8004fce8(callBackInfo);
        }
    }

    //8004fce8
    private void FUN_8004fce8(CallBackInfo callBackInfo)
    {
        short sVar1;
        short sVar2;
        ulong uVar3;
        int iVar4;
        uint puVar5;
        int iVar6;
        uint puVar7;
        int j;
        int iVar8;
        SPRT pSVar9;
        SPRT sprite;
        int i;
        short sVar10;

        sVar2 = (short)_gameEngine.StaticVariables.g_asyncOperationCountdown;
        iVar4 = _gameEngine.StaticVariables.g_asyncCallbackArgs2[_gameEngine.StaticVariables.g_asyncOperationCountdown].Length;

        Debugger.Break();
        //ApplyFadeTransform(_gameEngine.StaticVariables.g_sprites,
        //    (short)(callBackInfo.Data.X + callBackInfo.Data.Width + sVar2 * 0x30 + iVar4 * 4 + -8),
        //    (short)(callBackInfo.Data.Height + callBackInfo.Data.Y + -0x10),
        //    _gameEngine.StaticVariables.g_drawModes[0x14].tag);

        FUN_800507e4(_gameEngine.StaticVariables.g_sprites);

        Debugger.Break();
        //uVar3 = _gameEngine.StaticVariables.g_drawModes[0x14].tag;
        //i = 0;
        //sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017e674;
        //iVar4 = 0;
        //sVar10 = 0;
        //
        //do
        //{
        //    j = 0;
        //    iVar6 = uVar3 * 0x78 + iVar4;
        //
        //    do
        //    {
        //        _gameEngine.StaticVariables.SPRT_ARRAY_8017e674[iVar6].x0 = (short)(callBackInfo.Data.Width + callBackInfo.Data.X + sVar10);
        //        _gameEngine.StaticVariables.SPRT_ARRAY_8017e674[iVar6].y0 = (short)(callBackInfo.Data.Height + callBackInfo.Data.Y - j);
        //        j = j + 1;
        //        iVar6 = iVar6 + 1;
        //    } while (j < 1);
        //
        //    j = 0;
        //    iVar8 = uVar3 * 0x28;
        //    iVar6 = 0;
        //    pSVar9 = sprite + uVar3 * 6;
        //
        //    do
        //    {
        //        puVar7 = (_gameEngine.StaticVariables.DAT_80146f70 + iVar8);
        //        iVar8 = iVar8 + 4;
        //        puVar5 = (uint*)((int)_gameEngine.StaticVariables.SPRT_ARRAY_8017e674[uVar3 * 6].tag + iVar6 + iVar4);
        //        j = j + 1;
        //        /* Probable PsyQ macro: addPrim(). */
        //        //*puVar5 = *puVar5 & 0xff000000 | *puVar7 & 0xffffff;
        //        //*puVar7 = *puVar7 & 0xff000000 | (uint)pSVar9 & 0xffffff;
        //        iVar6 = iVar6 + 0x3c;
        //        pSVar9 = pSVar9 + 3;
        //    } while (j < 1);
        //
        //    sprite = sprite + 3;
        //    iVar4 = iVar4 + 0x3c;
        //    i = i + 1;
        //    sVar10 = sVar10 + 0x30;
        //
        //} while (i < 2);
    }

    //800507e4
    private void FUN_800507e4(SPRT[] sprites)
    {
        SPRT pSVar1;
        SPRT pSVar2;
        int value;
        uint uVar3;
        uint puVar4;

        value = sprites[0].r0 + 1;
        sprites[0].r0 = (byte)value;

        if (value == 0x28)
        {
            sprites[0].r0 = 0;
            sprites[0].g0 = 0;
            sprites[0].b0 = 0;
            sprites[0].code = 0;
        }

        Debugger.Break();
        //sprites[_gameEngine.StaticVariables.g_drawModes[0x14].tag + 1].tag = _gameEngine.StaticVariables.BYTE_800a58d8[(sprites[0].r0 / 10) * 0x28];
        //sprites[_gameEngine.StaticVariables.g_drawModes[0x14].tag + 1].tag + 1 = _gameEngine.StaticVariables.DAT_800a58d9[(sprites[0].r0 / 10) * 0x28];
        //puVar4 = _gameEngine.StaticVariables.DAT_80146f6c[_gameEngine.StaticVariables.g_drawModes[0x14].tag * 0x28];
        //pSVar1 = sprites[_gameEngine.StaticVariables.g_drawModes[0x14].tag];
        //uVar3._0_2_ = sprites[_gameEngine.StaticVariables.g_drawModes[0x14].tag].x0;
        //uVar3._2_2_ = sprites[_gameEngine.StaticVariables.g_drawModes[0x14].tag].y0;
        ///* Probable PsyQ macro: addPrim(). */
        ////uVar3 = uVar3 & 0xff000000 | *puVar4 & 0xffffff;
        //pSVar2 = sprites + _gameEngine.StaticVariables.g_drawModes[0x14].tag;
        //pSVar2.x0 = (short)uVar3;
        //pSVar2.y0 = (short)(uVar3 >> 0x10);
        ////*puVar4 = *puVar4 & 0xff000000 | (uint)&pSVar1.x0 & 0xffffff;
    }

    //80045e60
    private void ProcessEtcTextAdvance()
    {
        byte shouldAdvance = 0;

        if ((_gameEngine.StaticVariables.g_etcAnimationMode & 2U) != 0)
        {
            shouldAdvance = (byte)(_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed >> 7);
        }

        if ((_gameEngine.StaticVariables.g_etcAnimationMode & 1U) != 0)
        {
            _gameEngine.StaticVariables.INT_80149cc4 += -1;

            if (_gameEngine.StaticVariables.INT_80149cc4 == 0)
            {
                shouldAdvance = 1;
            }
        }

        if ((_gameEngine.StaticVariables.g_etcAnimationMode & 4U) != 0 && _gameEngine.StaticVariables.g_textHoldState_2 == 1)
        {
            shouldAdvance = 1;
            _gameEngine.StaticVariables.g_textHoldState_2 = 0;
        }

        if (shouldAdvance != 0)
        {
            _gameEngine.StaticVariables.g_warpFlags_2 |= 2;

            _gameEngine.SoundManager.PlaySoundEffect(7);
            ResetHudTransitionState();
            _gameEngine.HudManager.UpdateHudTransitionState();

            _gameEngine.StaticVariables.g_textToDisplay.mode = 2;
            _gameEngine.StaticVariables.g_textToDisplay.tick = 0;
            _gameEngine.StaticVariables.g_textToDisplay.speed = 0xf;

            if (_gameEngine.StaticVariables.g_uiBoxDialogMessageBackground.X < 0)
            {
                _gameEngine.StaticVariables.g_textToDisplay.x = (short)(_gameEngine.StaticVariables.g_uiBoxDialogMessageBackground.X + _gameEngine.StaticVariables.g_uiBoxDialogMessageBackground.Width * -8);
            }
            else
            {
                _gameEngine.StaticVariables.g_textToDisplay.x = _gameEngine.StaticVariables.g_uiBoxDialogMessageBackground.X;
            }

            if (_gameEngine.StaticVariables.g_uiBoxDialogMessageBackground.Y < 0)
            {
                _gameEngine.StaticVariables.g_textToDisplay.y = (short)(_gameEngine.StaticVariables.g_uiBoxDialogMessageBackground.Y + _gameEngine.StaticVariables.g_uiBoxDialogMessageBackground.Height * -8);
            }

            else
            {
                _gameEngine.StaticVariables.g_textToDisplay.y = _gameEngine.StaticVariables.g_uiBoxDialogMessageBackground.Y;
            }

            if (_gameEngine.StaticVariables.g_uiBoxDialogMessageBackground.X < 0)
            {
                _gameEngine.StaticVariables.g_textToDisplay.startX = (short)(_gameEngine.StaticVariables.g_uiBoxDialogMessageBackground.X + _gameEngine.StaticVariables.g_uiBoxDialogMessageBackground.Width * -8);
            }
            else
            {
                _gameEngine.StaticVariables.g_textToDisplay.startX = _gameEngine.StaticVariables.g_uiBoxDialogMessageBackground.X;
            }

            _gameEngine.StaticVariables.g_textToDisplay.startY = 0xf0;
        }
    }

    //80059fe0
    private void ResetHudTransitionState()
    {
        if ((_gameEngine.StaticVariables.g_etcDisplayFlags & 4U) != 0)
        {
            _gameEngine.StaticVariables.g_etcDisplayFlags = 6;
        }

        _gameEngine.StaticVariables.g_textToDisplay2.mode = 2;
        _gameEngine.StaticVariables.g_textToDisplay2.tick = 0;
        _gameEngine.StaticVariables.g_textToDisplay2.speed = 0xf;

        if (_gameEngine.StaticVariables.g_textTilesConfiguration.X < 0)
        {
            _gameEngine.StaticVariables.g_textToDisplay2.x = (short)(_gameEngine.StaticVariables.g_textTilesConfiguration.X + _gameEngine.StaticVariables.g_textTilesConfiguration.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.g_textToDisplay2.x = _gameEngine.StaticVariables.g_textTilesConfiguration.X;
        }

        if (_gameEngine.StaticVariables.g_textTilesConfiguration.Y < 0)
        {
            _gameEngine.StaticVariables.g_textToDisplay2.y = (short)(_gameEngine.StaticVariables.g_textTilesConfiguration.Y + _gameEngine.StaticVariables.g_textTilesConfiguration.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.g_textToDisplay2.y = _gameEngine.StaticVariables.g_textTilesConfiguration.Y;
        }

        _gameEngine.StaticVariables.g_textToDisplay2.startX = 0x140;

        if (_gameEngine.StaticVariables.g_textTilesConfiguration.Y < 0)
        {
            _gameEngine.StaticVariables.g_textToDisplay2.startY = (short)(_gameEngine.StaticVariables.g_textTilesConfiguration.Y + _gameEngine.StaticVariables.g_textTilesConfiguration.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.g_textToDisplay2.startY = _gameEngine.StaticVariables.g_textTilesConfiguration.Y;
        }
    }

    //80047dd0
    public int RenderTextTilesStep(UIBoxConfiguration uiBoxConfiguration, TextToDisplay textToDisplay)
    {
        int result;
        int x;
        int y;
        SPRT sprt;

        if (textToDisplay.mode == 0)
        {
            return 1;
        }

        if (textToDisplay.tick == textToDisplay.speed)
        {
            uiBoxConfiguration.X = textToDisplay.startX;
            uiBoxConfiguration.Y = textToDisplay.startY;
            textToDisplay.mode -= 1;

            if (uiBoxConfiguration == _gameEngine.StaticVariables.UIBoxConfiguration_800b9e58)
            {
                Debug.WriteLine($"end {uiBoxConfiguration} - {textToDisplay}");
            }
        }
        else
        {
            int xInterp = textToDisplay.x + ((textToDisplay.startX - textToDisplay.x) * textToDisplay.tick) / textToDisplay.speed;
            int yInterp = textToDisplay.y + ((textToDisplay.startY - textToDisplay.y) * textToDisplay.tick) / textToDisplay.speed;
            uiBoxConfiguration.X = (short)xInterp;
            uiBoxConfiguration.Y = (short)yInterp;

            if (uiBoxConfiguration == _gameEngine.StaticVariables.UIBoxConfiguration_800b9e58)
            {
                Debug.WriteLine($"move {uiBoxConfiguration} - {textToDisplay}");
            }

            textToDisplay.tick += 1;
        }

        y = uiBoxConfiguration.Y;
        var i = 0;
        result = 0;

        if (y < uiBoxConfiguration.Height * 8 + y)
        {
            do
            {
                x = uiBoxConfiguration.X;

                if (x < uiBoxConfiguration.Width * 8 + x)
                {
                    do
                    {
                        sprt = uiBoxConfiguration.SpritesA[i];
                        sprt.x0 = (short)x;
                        sprt.y0 = (short)y;

                        x += 8;
                        i++;
                    } while (x < uiBoxConfiguration.Width * 8 + uiBoxConfiguration.X);
                }

                y += 8;
                result = 0;

            } while (y < uiBoxConfiguration.Height * 8 + uiBoxConfiguration.Y);
        }

        return result;
    }

    //8004501c
    private void FUN_8004501c(CallBackInfo callBackInfo)
    {
        _gameEngine.HudManager.FUN_80047cb0(callBackInfo);
        _gameEngine.StaticVariables.g_warpFlags_2 = 0;
        _gameEngine.StaticVariables.g_playerControlFlags &= 0xffffffe7;
    }

    //80045fe0
    private void TextInterpreter()
    {
        uint uVar1;
        char pcVar2;
        uint puVar3;
        int fontWidth;
        char[] textBuffer = new char[8];
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
        currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;

        if ((_gameEngine.StaticVariables.g_textFlags & 8) == 0)
        {
            if ((_gameEngine.StaticVariables.g_textFlags & 2) != 0)
            {
                _gameEngine.StaticVariables.g_textDelay += -1;

                if (_gameEngine.StaticVariables.g_textDelay == 0)
                {
                    shouldRender = true;
                    _gameEngine.StaticVariables.g_textDelay = _gameEngine.StaticVariables.g_textDelayReset;
                }
            }

            if ((_gameEngine.StaticVariables.g_textFlags & 1) != 0
                && (_gameEngine.StaticVariables.g_padState1.ButtonsHold & 0x80) != 0)
            {
                shouldRender = true;
            }

            if ((_gameEngine.StaticVariables.g_textFlags & 4) != 0
                && _gameEngine.StaticVariables.g_textAutoAdvanceFlag == 1)
            {
                shouldRender = true;
                _gameEngine.StaticVariables.g_textAutoAdvanceFlag = 0;
            }

            cursor = _gameEngine.StaticVariables.g_textCursor;

            if (shouldRender)
            {
                switchD_80046540_RENDER_NEXT_CHARACTER:
                _gameEngine.StaticVariables.g_textCursor = cursor;
                currentChar = _gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor];

                if (currentChar != '\0')
                {
                    if (currentChar == '\n')
                    {
                        cursor = _gameEngine.StaticVariables.g_textCursor + 1;
                        goto switchD_80046540_RENDER_NEXT_CHARACTER;
                    }

                    if (currentChar == '{')
                    {
                        cursor = _gameEngine.StaticVariables.g_textCursor + 1;
                        _gameEngine.StaticVariables.g_textCursor += 2;
                        currentLineIndex = _gameEngine.StaticVariables.g_textBufferX + _gameEngine.StaticVariables.g_textLineIndex >> 0x1f;
                        fontWidth = (_gameEngine.StaticVariables.g_textBufferX + _gameEngine.StaticVariables.g_textLineIndex) / 3 + currentLineIndex;
                        textBuffer[0] = (char)(_gameEngine.StaticVariables.g_scriptBuffer[cursor] + 0x50);

                        LAB_8004635c:
                        textBuffer[1] = '\0';

                        RenderTextBitmap(textBuffer, _gameEngine.StaticVariables.g_textBuffer, 0x3c0,
                                         (short)((uint)(((_gameEngine.StaticVariables.g_textBufferX + _gameEngine.StaticVariables.g_textLineIndex + (fontWidth - currentLineIndex) * -3) * 0x10 + 0x120) * 0x10000) >> 0x10),
                                         (short)_gameEngine.StaticVariables.g_textLineStartX, 0, 0x100, 0x10);

                        _gameEngine.StaticVariables.g_textLineStartX += _gameEngine.StaticVariables.g_fontCharWidthTable[textBuffer[0] * 5];
                        return;
                    }

                    if (currentChar == '}')
                    {
                        cursor = _gameEngine.StaticVariables.g_textCursor + 1;
                        _gameEngine.StaticVariables.g_textCursor += 2;
                        currentLineIndex = _gameEngine.StaticVariables.g_textBufferX + _gameEngine.StaticVariables.g_textLineIndex >> 0x1f;
                        fontWidth = (_gameEngine.StaticVariables.g_textBufferX + _gameEngine.StaticVariables.g_textLineIndex) / 3 + currentLineIndex;
                        textBuffer[0] = (char)(_gameEngine.StaticVariables.g_scriptBuffer[cursor] + 0x90);

                        Debugger.Break();
                        return;
                        //goto LAB_8004635c;
                    }

                    var currentTextCursorValue = _gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor + 1];

                    if (currentChar != '\\')
                    {
                        currentLineIndex = _gameEngine.StaticVariables.g_textCursor;
                        //we want to go to the default case to simulate the goto 'LAB_80046ccc'
                        currentTextCursorValue = '@';

                        //goto LAB_80046ccc;
                        //Debugger.Break();
                    }
                    else
                    {
                        currentLineIndex = _gameEngine.StaticVariables.g_textCursor + 1;
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
                            charCode = (byte)_gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor + 1];
                            _gameEngine.StaticVariables.g_textCursor = currentLineIndex;

                            while (charCode - 0x30 < 10)
                            {
                                numericString[cursor] = _gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor];
                                numericString[cursor + 1] = '\0';
                                cursor += 1;
                                charCode = (byte)_gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor + 1];
                                _gameEngine.StaticVariables.g_textCursor += 1;
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
                            _gameEngine.StaticVariables.g_globalFlags[(uVar1 >> 3) & 0xffc] |= (uint)(1 << (int)(uVar1 & 0x1f));
                            cursor = _gameEngine.StaticVariables.g_textCursor;
                            goto switchD_80046540_RENDER_NEXT_CHARACTER;

                        case 'A':
                            _gameEngine.StaticVariables.g_textFlags |= 8;
                            _gameEngine.StaticVariables.g_textCursor += 2;
                            _gameEngine.StaticVariables.g_textHoldState = 1;
                            return;

                        case 'B':
                            _gameEngine.StaticVariables.g_currentVoiceSfxId = -1;
                            break;

                        case 'C':
                            _gameEngine.StaticVariables.g_currentVoiceSfxId = 0;
                            cursor = _gameEngine.StaticVariables.g_textCursor + 2;
                            goto switchD_80046540_RENDER_NEXT_CHARACTER;

                        case 'D':
                            _gameEngine.StaticVariables.g_currentVoiceSfxId = 1;
                            break;

                        case 'E':
                            _gameEngine.StaticVariables.g_currentVoiceSfxId = 2;
                            break;

                        case 'F':
                            _gameEngine.StaticVariables.g_currentVoiceSfxId = 3;
                            break;

                        case 'G':
                            _gameEngine.StaticVariables.g_currentVoiceSfxId = 4;
                            break;

                        case 'H':
                            currentLineIndex = _gameEngine.StaticVariables.g_textCursor + 2;
                            _gameEngine.StaticVariables.g_textCursor += 2;
                            currentLineIndex = CalculateTextWidthFromScript([_gameEngine.StaticVariables.g_scriptBuffer[currentLineIndex]]);
                            _gameEngine.StaticVariables.g_textLineWidth[(_gameEngine.StaticVariables.g_textBufferX + _gameEngine.StaticVariables.g_textLineIndex) % 3] = currentLineIndex;
                            cursor = _gameEngine.StaticVariables.g_textCursor;
                            goto switchD_80046540_RENDER_NEXT_CHARACTER;

                        case 'M':
                            cursor = _gameEngine.StaticVariables.g_textCursor + 2;
                            if (_gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor + 2] == 'C')
                            {
                                cursor = _gameEngine.StaticVariables.g_textCursor + 3;

                                if (_gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor + 3] == 'E')
                                {
                                    _gameEngine.StaticVariables.g_textFlags = 4;
                                    cursor = _gameEngine.StaticVariables.g_textCursor + 4;
                                }
                            }

                            goto switchD_80046540_RENDER_NEXT_CHARACTER;

                        case 'N':
                            _gameEngine.StaticVariables.g_textCursor += 2;
                            forceLineAdvance = true;
                            //goto LAB_80046e34;
                            currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;

                            if (forceLineAdvance)
                            {
                                _gameEngine.StaticVariables.g_textRenderStep = 0;
                                _gameEngine.StaticVariables.g_textLineStartX = 0;
                                Array.Clear(_gameEngine.StaticVariables.g_textBuffer);
                                currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;

                                if (_gameEngine.StaticVariables.g_textLineIndex == 2)
                                {
                                    _gameEngine.StaticVariables.g_textMessageConfirmed = 1;
                                    _gameEngine.StaticVariables.g_textChoiceIndex = _gameEngine.StaticVariables.g_textNextChoice;

                                    if ((_gameEngine.StaticVariables.g_debugFlags_2 & 1) != 0)
                                    {
                                        _gameEngine.StaticVariables.g_textSelectionConfirmed = _gameEngine.StaticVariables.g_textSelectionNext;
                                    }
                                }
                                else
                                {
                                    currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex + 1;

                                    if (_gameEngine.StaticVariables.g_textLineIndex + 1 == 3)
                                    {
                                        currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;
                                    }
                                }
                            }

                            _gameEngine.StaticVariables.g_textLineIndex = currentLineIndex;
                            return;

                        case 'T':
                            _gameEngine.StaticVariables.g_textDelay = _gameEngine.StaticVariables.g_textDelayReset << 1;
                            _gameEngine.StaticVariables.g_textCursor += 2;
                            return;

                        case 'V':
                            Debugger.Break();
                            break;
                        /*
                        charCode = (byte)_gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor + 2];
                        _gameEngine.StaticVariables.g_textCursor += 3;
                        strncpy(acStack_1c48, _gameEngine.StaticVariables.g_scriptBuffer, _gameEngine.StaticVariables.g_textCursor);
                        acStack_1c48[_gameEngine.StaticVariables.g_textCursor] = '\0';
                        acStack_988[0] = '\0';
                        currentLineIndex = _gameEngine.StaticVariables.INT_ARRAY_80191908[charCode - 0x30];

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
                        strcat(acStack_1c48, _gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor]);
                        goto LAB_800469b4;*/

                        case 'W':
                            var value = (int)currentChar - 0x20;

                            if (0x40 < (byte)_gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor + 2])
                            {
                                value = -0x27;
                            }

                            textBuffer[0] = (char)(_gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor + 2] + value);
                            currentLineIndex = _gameEngine.StaticVariables.g_textBufferX + _gameEngine.StaticVariables.g_textLineIndex >> 0x1f;
                            fontWidth = (_gameEngine.StaticVariables.g_textBufferX + _gameEngine.StaticVariables.g_textLineIndex) / 3 + currentLineIndex;
                            _gameEngine.StaticVariables.g_textCursor += 3;
                            //goto LAB_8004635c;
                            currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;

                            if (forceLineAdvance)
                            {
                                _gameEngine.StaticVariables.g_textRenderStep = 0;
                                _gameEngine.StaticVariables.g_textLineStartX = 0;
                                Array.Clear(_gameEngine.StaticVariables.g_textBuffer);
                                currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;

                                if (_gameEngine.StaticVariables.g_textLineIndex == 2)
                                {
                                    _gameEngine.StaticVariables.g_textMessageConfirmed = 1;
                                    _gameEngine.StaticVariables.g_textChoiceIndex = _gameEngine.StaticVariables.g_textNextChoice;

                                    if ((_gameEngine.StaticVariables.g_debugFlags_2 & 1) != 0)
                                    {
                                        _gameEngine.StaticVariables.g_textSelectionConfirmed = _gameEngine.StaticVariables.g_textSelectionNext;
                                    }
                                }
                                else
                                {
                                    currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex + 1;

                                    if (_gameEngine.StaticVariables.g_textLineIndex + 1 == 3)
                                    {
                                        currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;
                                    }
                                }
                            }

                            _gameEngine.StaticVariables.g_textLineIndex = currentLineIndex;
                            return;

                        case 'X':
                            cursor = _gameEngine.StaticVariables.g_textCursor + 2;
                            Debugger.Break();
                            break;
                        /*
                        switch (_gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor + 2])
                        {
                            case '0':
                                _gameEngine.StaticVariables.g_textCursor += 3;
                                strncpy(acStack_1c48, _gameEngine.StaticVariables.g_scriptBuffer, _gameEngine.StaticVariables.g_textCursor);

                                acStack_1c48[_gameEngine.StaticVariables.g_textCursor] = '\0';
                                currentLineIndex = _gameEngine.PlayerManager.GetNumberOfFalconTemp();

                                if (currentLineIndex / 10 + (currentLineIndex >> 0x1f) != currentLineIndex >> 0x1f)
                                {
                                    currentLineIndex = _gameEngine.PlayerManager.GetNumberOfFalconTemp();
                                    strcat(acStack_1c48, PTR_DAT_8009a7ec[currentLineIndex / 10]);
                                }

                                currentLineIndex = _gameEngine.PlayerManager.GetNumberOfFalconTemp();
                                strcat(acStack_1c48, PTR_DAT_8009a7ec[currentLineIndex % 10]);
                                strcat(acStack_1c48, _gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor]);
                                break;

                            case '1':
                                _gameEngine.StaticVariables.g_textCursor += 3;
                                strncpy(acStack_1c48, _gameEngine.StaticVariables.g_scriptBuffer, _gameEngine.StaticVariables.g_textCursor);
                                acStack_1c48[_gameEngine.StaticVariables.g_textCursor] = '\0';
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
                                _gameEngine.StaticVariables.g_textCursor += 3;
                                strncpy(acStack_1c48, _gameEngine.StaticVariables.g_scriptBuffer, _gameEngine.StaticVariables.g_textCursor);
                                acStack_1c48[_gameEngine.StaticVariables.g_textCursor] = '\0';
                                strcat(acStack_1c48,
                                    PTR_g_iconNameEtcBase_8009a814[_gameEngine.StaticVariables.g_textCategoryIndex]);
                                strcat(acStack_1c48, _gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor]);
                                break;

                            case '3':
                                _gameEngine.StaticVariables.g_textCursor += 3;
                                strncpy(acStack_1c48, _gameEngine.StaticVariables.g_scriptBuffer, _gameEngine.StaticVariables.g_textCursor);
                                acStack_1c48[_gameEngine.StaticVariables.g_textCursor] = '\0';
                                _gameEngine.PlayerManager.UpdateNumberOfFalcon();
                                UpdatePlayerProgressState();
                                strcat(acStack_1c48,
                                    PTR_DAT_8009a7ec[
                                        _gameEngine.StaticVariables.g_categoryThresholdTable[
                                            _gameEngine.StaticVariables.g_textCategoryIndex] / 10]);
                                strcat(acStack_1c48,
                                    PTR_DAT_8009a7ec[
                                        _gameEngine.StaticVariables.g_categoryThresholdTable[
                                            _gameEngine.StaticVariables.g_textCategoryIndex] % 10]);
                                strcat(acStack_1c48, _gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor]);
                                strcpy(_gameEngine.StaticVariables.g_scriptBuffer, acStack_1c48);
                                cursor = _gameEngine.StaticVariables.g_textCursor;
                                goto switchD_80046540_RENDER_NEXT_CHARACTER;

                            case '5':
                                _gameEngine.StaticVariables.g_textCursor += 3;
                                strncpy(acStack_1c48, _gameEngine.StaticVariables.g_scriptBuffer, _gameEngine.StaticVariables.g_textCursor);
                                acStack_1c48[_gameEngine.StaticVariables.g_textCursor] = '\0';
                                _gameEngine.PlayerManager.UpdateNumberOfFalcon();
                                UpdatePlayerProgressState();
                                currentLineIndex = _gameEngine.PlayerManager.GetNumberOfFalcon();
                                currentLineIndex =
                                    _gameEngine.StaticVariables.g_categoryThresholdTable[_gameEngine.StaticVariables.g_textCategoryIndex] -
                                    currentLineIndex;

                                if (9 < currentLineIndex)
                                {
                                    strcat(acStack_1c48, PTR_DAT_8009a7ec[currentLineIndex / 10]);
                                }

                                LAB_80046984:
                                strcat(acStack_1c48, PTR_DAT_8009a7ec[currentLineIndex % 10]);
                                strcat(acStack_1c48, _gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor]);

                                LAB_800469b4:
                                strcpy(_gameEngine.StaticVariables.g_scriptBuffer, acStack_1c48);
                                cursor = _gameEngine.StaticVariables.g_textCursor;
                                goto switchD_80046540_RENDER_NEXT_CHARACTER;

                            default:
                                goto switchD_80046540_RENDER_NEXT_CHARACTER;
                        }
                        strcpy(_gameEngine.StaticVariables.g_scriptBuffer, acStack_1c48);
                        _gameEngine.PlayerManager.UpdateNumberOfFalcon();
                        UpdatePlayerProgressState();
                        cursor = _gameEngine.StaticVariables.g_textCursor;
                        goto switchD_80046540_RENDER_NEXT_CHARACTER;
                        */

                        case 'Y':
                            _gameEngine.StaticVariables.g_textCursor += 2;
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
                            _gameEngine.StaticVariables.g_textCursor = currentLineIndex;
                            shouldRender = ContainsSpecialTextFormatting(_gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor]);

                            if (shouldRender == false)
                            {
                                textBuffer[0] = (char)_gameEngine.StaticVariables.g_scriptBuffer[_gameEngine.StaticVariables.g_textCursor];
                                _gameEngine.StaticVariables.g_textCursor += 1;
                            }
                            else
                            {
                                //DoNothing();
                                //DoNothing();
                                //DoNothing();
                                textBuffer[0] = (char)0x3f;
                                _gameEngine.StaticVariables.g_textCursor += 2;
                            }

                            textBuffer[1] = '\0';

                            RenderTextBitmap(textBuffer, _gameEngine.StaticVariables.g_textBuffer, 0x3c0,
                                (short)((uint)(((_gameEngine.StaticVariables.g_textBufferX + _gameEngine.StaticVariables.g_textLineIndex) % 3 * 0x10 + 0x120) * 0x10000) >> 0x10),
                                (short)_gameEngine.StaticVariables.g_textLineStartX, 0, 0x100, 0x10);

                            _gameEngine.StaticVariables.g_textLineStartX += _gameEngine.StaticVariables.g_fontCharWidthTable[(uint)textBuffer[0] * 5];

                            if ((_gameEngine.StaticVariables.g_textRenderStep & 1) == 0
                                && _gameEngine.StaticVariables.g_currentVoiceSfxId != 4
                                && -1 < _gameEngine.StaticVariables.g_currentVoiceSfxId)
                            {
                                _gameEngine.SoundManager.PlaySoundEffect((uint)(_gameEngine.StaticVariables.g_currentVoiceSfxId + 0x4f));
                            }

                            _gameEngine.StaticVariables.g_textRenderStep += 1;

                            //goto LAB_80046e34;
                            currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;

                            if (forceLineAdvance)
                            {
                                _gameEngine.StaticVariables.g_textRenderStep = 0;
                                _gameEngine.StaticVariables.g_textLineStartX = 0;
                                Array.Clear(_gameEngine.StaticVariables.g_textBuffer);
                                currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;

                                if (_gameEngine.StaticVariables.g_textLineIndex == 2)
                                {
                                    _gameEngine.StaticVariables.g_textMessageConfirmed = 1;
                                    _gameEngine.StaticVariables.g_textChoiceIndex = _gameEngine.StaticVariables.g_textNextChoice;

                                    if ((_gameEngine.StaticVariables.g_debugFlags_2 & 1) != 0)
                                    {
                                        _gameEngine.StaticVariables.g_textSelectionConfirmed = _gameEngine.StaticVariables.g_textSelectionNext;
                                    }
                                }
                                else
                                {
                                    currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex + 1;

                                    if (_gameEngine.StaticVariables.g_textLineIndex + 1 == 3)
                                    {
                                        currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;
                                    }
                                }
                            }

                            _gameEngine.StaticVariables.g_textLineIndex = currentLineIndex;
                            return;
                            //LAB_80046e34
                    }
                    cursor = _gameEngine.StaticVariables.g_textCursor + 2;
                    goto switchD_80046540_RENDER_NEXT_CHARACTER;

                }

                _gameEngine.StaticVariables.g_textPrimitives = 1;
                currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;

                if ((_gameEngine.StaticVariables.g_etcAnimationMode & 1U) != 0)
                {
                    _gameEngine.StaticVariables.INT_80149cc4 = _gameEngine.StaticVariables.g_textBufferSize;
                }
            }
        }
        else
        {
            forceLineAdvance = true;

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & 0x80) != 0)
            {
                _gameEngine.StaticVariables.g_textHoldState = 0;
                _gameEngine.StaticVariables.g_textFlags = (int)(_gameEngine.StaticVariables.g_textFlags & 0xfffffff7);
                _gameEngine.StaticVariables.g_debugFlags_2 |= 8;

                LAB_80046e34:
                currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;

                if (forceLineAdvance)
                {
                    _gameEngine.StaticVariables.g_textRenderStep = 0;
                    _gameEngine.StaticVariables.g_textLineStartX = 0;
                    Array.Clear(_gameEngine.StaticVariables.g_textBuffer);
                    currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;

                    if (_gameEngine.StaticVariables.g_textLineIndex == 2)
                    {
                        _gameEngine.StaticVariables.g_textMessageConfirmed = 1;
                        _gameEngine.StaticVariables.g_textChoiceIndex = _gameEngine.StaticVariables.g_textNextChoice;

                        if ((_gameEngine.StaticVariables.g_debugFlags_2 & 1) != 0)
                        {
                            _gameEngine.StaticVariables.g_textSelectionConfirmed = _gameEngine.StaticVariables.g_textSelectionNext;
                        }
                    }
                    else
                    {
                        currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex + 1;

                        if (_gameEngine.StaticVariables.g_textLineIndex + 1 == 3)
                        {
                            currentLineIndex = _gameEngine.StaticVariables.g_textLineIndex;
                        }
                    }
                }
            }
        }

        _gameEngine.StaticVariables.g_textLineIndex = currentLineIndex;
    }

    //800478c4
    public void RenderTextBitmap(char[] formattedText, char[] buffer,
        short posX, short posY, short textWidth,
        short textLineOffset, short drawWidth, short drawHeight)
    {
        int i = 0;
        byte fontPixel;
        int bufferWidth2;
        byte bufferOffset;
        uint glyphStartBit;
        uint charIndex;
        uint fontColumn;
        uint fontPixelOffset;
        int lineByteOffset;
        int drawY;
        int iVar1;
        int glyphRow;
        int bufferWidth;
        Rectangle drawRect = new Rectangle();
        short posX_;

        //Debugger.Break();

        if (formattedText[i] != '\0')
        {
            bufferWidth = (int)drawWidth;
            posX_ = posX;

            do
            {
                charIndex = formattedText[i];
                glyphRow = 0;
                glyphStartBit = (uint)_gameEngine.StaticVariables.g_fontCharWidthTable[charIndex * 5 + 2];
                bufferWidth2 = _gameEngine.StaticVariables.g_fontCharWidthTable[charIndex * 5 + 3] * 0x80 + (int)glyphStartBit / 2 + -0x7feb52d8;

                if (0 < _gameEngine.StaticVariables.g_fontCharWidthTable[charIndex * 5 + 1])
                {
                    do
                    {
                        iVar1 = 0;

                        if (0 < _gameEngine.StaticVariables.g_fontCharWidthTable[charIndex * 5])
                        {
                            drawY = textLineOffset + glyphRow;
                            lineByteOffset = glyphRow * 0x80;
                            fontColumn = glyphStartBit & 1;
                            fontPixelOffset = (uint)textWidth;

                            do
                            {
                                if (0xff < (int)fontPixelOffset)
                                {
                                    break;
                                }

                                if ((fontPixelOffset & 1) == 0)
                                {
                                    bufferOffset = (byte)(buffer[bufferWidth *
                                                                 (drawY + _gameEngine.StaticVariables.g_fontCharWidthTable
                                                                     [formattedText[i] * 5 + 4]) / 2 +
                                                                 (int)fontPixelOffset / 2] & 0xf0);

                                    if ((fontColumn & 1) == 0)
                                    {
                                        fontPixel = (byte)((bufferWidth2 + lineByteOffset + (int)fontColumn / 2) & 0xf);
                                    }
                                    else
                                    {
                                        fontPixel = (byte)((bufferWidth2 + lineByteOffset + (int)fontColumn / 2) >> 4);
                                    }
                                }
                                else
                                {
                                    bufferOffset = (byte)(buffer[bufferWidth * (drawY + _gameEngine.StaticVariables.g_fontCharWidthTable[charIndex * 5 + 4]) / 2 + (int)fontPixelOffset / 2] & 0xf);

                                    if ((fontColumn & 1) == 0)
                                    {
                                        fontPixel = (byte)(((bufferWidth2 + lineByteOffset + (int)fontColumn / 2) & 0xf) << 4);
                                    }
                                    else
                                    {
                                        fontPixel = (byte)((bufferWidth2 + lineByteOffset + (int)fontColumn / 2) & 0xf0);
                                    }
                                }

                                buffer[bufferWidth * (drawY + _gameEngine.StaticVariables.g_fontCharWidthTable[formattedText[i] * 5 + 4]) / 2 + (int)fontPixelOffset / 2] = (char)(bufferOffset | fontPixel);
                                fontPixelOffset += 1;
                                charIndex = formattedText[i];
                                iVar1 += 1;
                                fontColumn += 1;
                            } while (iVar1 < _gameEngine.StaticVariables.g_fontCharWidthTable[charIndex * 5]);
                        }

                        charIndex = formattedText[i];
                        glyphRow += 1;

                    } while (glyphRow < _gameEngine.StaticVariables.g_fontCharWidthTable[charIndex * 5 + 1]);
                }

                bufferWidth2 = bufferWidth;

                if (bufferWidth < 0)
                {
                    bufferWidth2 = bufferWidth + 3;
                }

                drawRect.X = posX_;
                drawRect.Width = (short)(bufferWidth2 >> 2);
                drawRect.Height = drawHeight;
                drawRect.Y = posY;
                LoadImage(drawRect, buffer);
                //DrawSync(0);
                bufferOffset = (byte)formattedText[i];
                i++;
                textWidth = (short)(textWidth + _gameEngine.StaticVariables.g_fontCharWidthTable[(uint)bufferOffset * 5]);
            } while (formattedText[i] != 0);
        }
    }

    //80084f90
    private void LoadImage(Rectangle rectangle, char[] buffer)
    {
        //Debug.WriteLine("LoadImage: " + rectangle);
        //Debugger.Break();
    }

    //8004f304
    public bool ContainsSpecialTextFormatting(char c)
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
                fontWidth = _gameEngine.StaticVariables.g_fontCharWidthTable[(text[1] + 0x50) * 5];
                index += 2;
                LAB_800478a0:
                totalWidth += fontWidth;
            }
            else
            {
                if (currentChar == 0x7d)
                {
                    fontWidth = _gameEngine.StaticVariables.g_fontCharWidthTable[(text[1] + 0x90) * 5];
                    index += 2;

                    //goto LAB_800478a0;
                    totalWidth += fontWidth;
                }
                else if (currentChar != 0x5c)
                {
                    fontWidth = _gameEngine.StaticVariables.g_fontCharWidthTable[(uint)currentChar * 5];
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

                            fontWidth = _gameEngine.StaticVariables.g_fontCharWidthTable[fontWidth * 5];
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

        progressFlags = (uint)(_gameEngine.StaticVariables.g_progressStateFlags & 0xfffffe01);

        if (_gameEngine.StaticVariables.g_playerState < 0)
        {
            _gameEngine.StaticVariables.g_progressStateFlags = (int)(progressFlags | 0x100);
            _gameEngine.StaticVariables.g_textCategoryIndex = 7;
        }
        else
        {
            _gameEngine.StaticVariables.g_progressStateFlags = (int)(progressFlags | 0x80);

            if ((_gameEngine.StaticVariables.g_playerState & 0x40000000U) == 0)
            {
                _gameEngine.StaticVariables.g_progressStateFlags = (int)(progressFlags | 0x40);

                if ((_gameEngine.StaticVariables.g_playerState & 0x20000000U) == 0)
                {
                    _gameEngine.StaticVariables.g_progressStateFlags = (int)(progressFlags | 0x20);

                    if ((_gameEngine.StaticVariables.g_playerState & 0x10000000U) == 0)
                    {
                        _gameEngine.StaticVariables.g_progressStateFlags = (int)(progressFlags | 0x10);

                        if ((_gameEngine.StaticVariables.g_playerState & 0x8000000U) == 0)
                        {
                            _gameEngine.StaticVariables.g_progressStateFlags = (int)(progressFlags | 8);

                            if ((_gameEngine.StaticVariables.g_playerState & 0x4000000U) == 0)
                            {
                                _gameEngine.StaticVariables.g_progressStateFlags = (int)(progressFlags | 4);

                                if ((_gameEngine.StaticVariables.g_playerState & 0x2000000U) == 0)
                                {
                                    _gameEngine.StaticVariables.g_progressStateFlags = (int)(progressFlags | 2);

                                    _gameEngine.StaticVariables.g_textCategoryIndex = 0;
                                }
                                else
                                {
                                    _gameEngine.StaticVariables.g_textCategoryIndex = 1;
                                }
                            }
                            else
                            {
                                _gameEngine.StaticVariables.g_textCategoryIndex = 2;
                            }
                        }
                        else
                        {
                            _gameEngine.StaticVariables.g_textCategoryIndex = 3;
                        }
                    }
                    else
                    {
                        _gameEngine.StaticVariables.g_textCategoryIndex = 4;
                    }
                }
                else
                {
                    _gameEngine.StaticVariables.g_textCategoryIndex = 5;
                }
            }
            else
            {
                _gameEngine.StaticVariables.g_textCategoryIndex = 6;
            }
        }

        piVar1 = _gameEngine.StaticVariables.g_categoryThresholdTable[_gameEngine.StaticVariables.g_textCategoryIndex];
        currentValue = _gameEngine.PlayerManager.GetNumberOfFalcon();

        if (currentValue < piVar1)
        {
            _gameEngine.StaticVariables.g_progressStateFlags = (int)(_gameEngine.StaticVariables.g_progressStateFlags & 0xfffff7ff);
        }
        else
        {
            _gameEngine.StaticVariables.g_progressStateFlags |= 0x800;
        }
    }

    //800455b4
    private void RenderText(CallBackInfo callbackInfo)
    {
        if (callbackInfo == null || callbackInfo.Data == null)
        {
            return;
        }

        ulong uVar1;
        int currentLine;
        int iVar5;
        short offsetY;
        int lineAdvance;
        short sVar6;
        int lineIndex;
        int bufferX;
        bool holdActive;

        bufferX = _gameEngine.StaticVariables.g_textBufferX;
        lineIndex = 0;
        sVar6 = 0;
        do
        {
            uVar1 = 0; //_gameEngine.StaticVariables.g_drawModes[0x14].tag;
            lineAdvance = 0;
            offsetY = 0;
            currentLine = (bufferX + lineIndex) % 3;
            iVar5 = 0;

            if (_gameEngine.StaticVariables.g_textLineWidth[currentLine] == 0)
            {
                _gameEngine.StaticVariables.g_textFullLinesSprites[currentLine * 2].x0 = (short)(callbackInfo.X + callbackInfo.Width);
                _gameEngine.StaticVariables.g_textFullLinesSprites[currentLine * 2].y0 = (short)(callbackInfo.Height + callbackInfo.Y + 2 + offsetY + sVar6);
            }
            else
            {
                _gameEngine.StaticVariables.g_textFullLinesSprites[currentLine * 2].x0 = (short)(callbackInfo.X + (callbackInfo.Width * 8 - _gameEngine.StaticVariables.g_textLineWidth[currentLine]) / 2);
                _gameEngine.StaticVariables.g_textFullLinesSprites[currentLine * 2].y0 = (short)(callbackInfo.Height + callbackInfo.Y + 2 + offsetY + sVar6);
            }

            //offsetY = offsetY + -1;
            //pPrimitiveEntry = _gameEngine.StaticVariables.g_textFullLinesSprites[currentLine * 2].tag;
            //pOrderTable = _gameEngine.StaticVariables.DAT_80146f60 + uVar1 * 0x28);
            lineAdvance = lineAdvance + 1;

            /* Probable PsyQ macro: addPrim(). */
            //*pPrimitiveEntry = *pPrimitiveEntry & 0xff000000 | *pOrderTable & 0xffffff;
            //*pOrderTable = *pOrderTable & 0xff000000 | _gameEngine.StaticVariables.g_textFullLinesSprites[uVar1 + currentLine * 2].tag + iVar5 & 0xffffffU;
            //uVar2 = _gameEngine.StaticVariables.g_drawModes[0x14].tag;
            iVar5 = iVar5 + 1;

            lineIndex = lineIndex + 1;
            //sVar6 = sVar6 + 0x10;

            //TODO create a buffer to draw the text
            //var sprite = _gameEngine.StaticVariables.g_textFullLinesSprites[currentLine * 2];
            //var bitmap = _gameEngine.Font3.GenerateFontBitmapFromSprite(sprite);
            //_gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

        } while (lineIndex < 3);

        //pOrderTable = _gameEngine.StaticVariables.DAT_80153010 + _gameEngine.StaticVariables.g_drawModes[0x14].tag * 0xc);
        //pPrimitiveEntry = _gameEngine.StaticVariables.DAT_80146f60 + _gameEngine.StaticVariables.g_drawModes[0x14].tag * 0x28);
        /* Probable PsyQ macro: addPrim(). */
        //*pOrderTable = *pOrderTable & 0xff000000 | *pPrimitiveEntry & 0xffffff;
        //*pPrimitiveEntry = *pPrimitiveEntry & 0xff000000 | (uint)pOrderTable & 0xffffff;

        holdActive = _gameEngine.StaticVariables.g_textHoldState != 0;

        if (holdActive)
        {
            _gameEngine.StaticVariables.INT_80149cd4 = _gameEngine.StaticVariables.INT_80149cd4 + 1;

            if (0x27 < _gameEngine.StaticVariables.INT_80149cd4)
            {
                _gameEngine.StaticVariables.INT_80149cd4 = 0;
            }

            _gameEngine.StaticVariables.g_cursorTextSprites[0].u0 = _gameEngine.StaticVariables.g_dialogCursorTextureUV[_gameEngine.StaticVariables.INT_80149cd4 / 10];
            _gameEngine.StaticVariables.g_cursorTextSprites[0].v0 = _gameEngine.StaticVariables.g_dialogCursorTextureUV[_gameEngine.StaticVariables.INT_80149cd4 / 10 + 1];
            _gameEngine.StaticVariables.g_cursorTextSprites[0].x0 = (short)(callbackInfo.X + callbackInfo.Width * 8 + -0x10);
            //_gameEngine.StaticVariables.g_cursorTextSprites[uVar1].y0 = (callbackInfo.X + 2) + (callbackInfo.Width + 6) * 8 + -0x18;
            _gameEngine.StaticVariables.g_cursorTextSprites[0].y0 = (short)(callbackInfo.Y + 2 + (callbackInfo.Height + 6) * 8 + -0x18);

            //uVar1 = g_drawModes[0x14].tag;
            //psVar3 = (callbackInfo.X);
            //pSVar4 = _gameEngine.StaticVariables.g_cursorTextSprites[0];
            //pOrderTable = _gameEngine.StaticVariables.g_drawModes + uVar1 * 0x28 + 0xf8);
            /* Probable PsyQ macro: addPrim(). */
            //pSVar4->tag = pSVar4->tag & 0xff000000 | *pOrderTable & 0xffffff;
            //*pOrderTable = *pOrderTable & 0xff000000 | (uint)pSVar4 & 0xffffff;

            var sprite = _gameEngine.StaticVariables.g_cursorTextSprites[0];
            var bitmap = _gameEngine.Font3.GenerateFontBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);
        }
    }

}