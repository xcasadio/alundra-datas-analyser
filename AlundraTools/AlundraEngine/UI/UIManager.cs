using AlundraEngine.Graphics;
using AlundraEngine.Text;
using System;
using System.Diagnostics;
using static AlundraEngine.Graphics.Renderer;

namespace AlundraEngine.UI;

public class UIManager
{
    private readonly GameEngine _gameEngine;

    public readonly List<Sprite>[] DialogLinesSprites = [new(), new(), new()];
    public readonly List<Sprite> DialogCharacterNameSprites = new();
    public readonly List<Sprite>[] DialogChoiceSprites = [new(), new()];
    public readonly List<Sprite> DialogSaveGameSprites = new();

    public UIManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    //80048304
    public void InitializeDialogBackgroundSprites(CallBackInfo callBackInfo)
    {
        //TODO : same as InitializeTextSpriteTiles ? => 8005a0c8
        int tileX;
        int tileY;
        UIBoxConfiguration tilesConfiguration;
        int surfaceIndex;

        surfaceIndex = 0;
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
    public void Fun_80046ef0(CallBackInfo callBackInfo)
    {
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

        if ((_gameEngine.StaticVariables.g_dialog_flags & 3) == 0)
        {
            if (_gameEngine.StaticVariables.g_textPrimitives == 0)
            {
                if (_gameEngine.StaticVariables.g_textMessageConfirmed != 0)
                {
                    FUN_80045988(callBackInfo);
                    return; // 1;
                }

                TextDecoder.TextInterpreter(_gameEngine);
            }
            else
            {
                ProcessEtcTextAdvance();
            }
        }
        else
        {
            y = UpdateUiBoxesPosition(callBackInfo.Data, _gameEngine.StaticVariables.g_backgroundMessageAnimation);

            if (y == 1)
            {
                if ((_gameEngine.StaticVariables.g_dialog_flags & 1) != 0)
                {
                    _gameEngine.StaticVariables.g_dialog_flags &= 0xfffffffe;
                }

                if ((_gameEngine.StaticVariables.g_dialog_flags & 2) != 0)
                {
                    _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X = _gameEngine.StaticVariables.g_backgroundMessageAnimation.originX;
                    _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y = _gameEngine.StaticVariables.g_backgroundMessageAnimation.originY;
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
        int iVar3;
        int index;
        int iVar5;
        int iVar8;
        int iVar9;
        short sVar10;
        short heightOffset;
        int line;
        int iVar14;

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
        heightOffset = 0;

        do
        {
            //uVar2 = _gameEngine.StaticVariables.g_drawModes[0x14].tag;
            iVar8 = 0;
            sVar10 = 0;
            iVar3 = (iVar5 + line) % 3;
            iVar9 = iVar3/* * 0x28*/;

            if (_gameEngine.StaticVariables.g_textNextChoice == 0)
            {
                Breakpoint.TriggerBreak();
                //trap(0x1c00);
            }

            index = (_gameEngine.StaticVariables.g_textNextChoice - _gameEngine.StaticVariables.g_textChoiceIndex) * 0x10;

            if (_gameEngine.StaticVariables.g_textNextChoice == -1 && index == -0x80000000)
            {
                Breakpoint.TriggerBreak();
                //trap(0x1800);
            }

            iVar14 = index / _gameEngine.StaticVariables.g_textNextChoice;
            index = iVar8 + iVar9 /*+ uVar2 * 0x14*/;
            var sprite = _gameEngine.StaticVariables.g_textFullLinesSprites[index];

            if (_gameEngine.StaticVariables.g_textLineWidth[iVar3] == 0)
            {
                sprite.x0 = (short)(callbackInfo.Data.X + callbackInfo.Data.Width);
                sprite.y0 = (short)(callbackInfo.Data.Y + callbackInfo.Data.Height + sVar10 + heightOffset - iVar14);
            }
            else
            {
                sprite.x0 = (short)(callbackInfo.Data.X + (callbackInfo.Data.Width * 8 - _gameEngine.StaticVariables.g_textLineWidth[iVar3]) / 2);
                sprite.y0 = (short)(callbackInfo.Data.Y + callbackInfo.Data.Height + sVar10 + heightOffset - iVar14);
            }

            sprite.u0 = (byte)(line * 0x40);
            sprite.v0 = 0xD0;
            sprite.w = 0x10;
            sprite.h = 0x10;
            sprite.clut = 8; //_gameEngine.StaticVariables.g_clutTable[8];

            foreach (var spr in DialogLinesSprites[index])
            {
                _gameEngine.Renderer.AddSprite(
                    spr.X + callbackInfo.Data.Width,
                    /*spr.Y +*/index * 0x10 + sprite.y0,
                    spr.Width, spr.Height,
                    SpriteDepth.ForegroundUI, spr.Bitmap, spr.Alpha);
            }

            //puVar7 = (uint*)((int)&_gameEngine.StaticVariables.g_textFullLinesSprites + iVar14);
            //puVar6 = (uint*)(&DAT_80146f60 + uVar2 * 0x28);
            /* Probable PsyQ macro: addPrim(). */
            //*puVar7 = *puVar7 & 0xff000000 | *puVar6 & 0xffffff;
            //*puVar6 = *puVar6 & 0xff000000 | (int)_gameEngine.StaticVariables.g_textFullLinesSprites[index] & 0xffffffU;

            line += 1;
            //heightOffset += 0x10; //already added somewhere...
        } while (line < 3);

        //puVar6 = (uint*)(&DAT_80153010 + _gameEngine.StaticVariables.g_drawModes[0x14].tag * 0xc);
        //puVar7 = (uint*)(&DAT_80146f60 + _gameEngine.StaticVariables.g_drawModes[0x14].tag * 0x28);
        /* Probable PsyQ macro: addPrim(). */
        //*puVar6 = *puVar6 & 0xff000000 | *puVar7 & 0xffffff;
        //*puVar7 = *puVar7 & 0xff000000 | (uint)puVar6 & 0xffffff; //addPrim()

        doAdavance = _gameEngine.StaticVariables.g_textChoiceIndex == 0;

        if (doAdavance)
        {
            _gameEngine.StaticVariables.g_textMessageConfirmed = 0;
            _gameEngine.StaticVariables.g_textBufferX = (_gameEngine.StaticVariables.g_textBufferX + 1) % 3;
            index = (_gameEngine.StaticVariables.g_textBufferX + _gameEngine.StaticVariables.g_textLineIndex) % 3;
            _gameEngine.StaticVariables.g_textLineWidth[index] = 0;
            //DialogLinesSprites[index].Clear();

            DialogLinesSprites[0].Clear();
            DialogLinesSprites[0].AddRange(DialogLinesSprites[1]);
            DialogLinesSprites[1].Clear();
            DialogLinesSprites[1].AddRange(DialogLinesSprites[2]);
            DialogLinesSprites[2].Clear();

            //iVar5 = _gameEngine.StaticVariables.g_textBufferX + 2;
            //rect.x = 0x3c0;
            //rect.y = ((short)iVar5 + ((short)((ulong)((long)iVar5 * 0x55555556) >> 0x20) - (short)(iVar5 >> 0x1f)) * -3) * 0x10 + 0x120;
            //rect.w = 0x40;
            //rect.h = 0x10;
            //ClearImage(&rect, '\0', '\0', '\0');
        }
    }
    
    //8004f628
    //display InitializeDialogBackgroundSprites
    public void Fun_8004f628(CallBackInfo callBackInfo)
    {
        Breakpoint.TriggerBreak();
    }

    //800501fc
    //display yes or no dialog box
    public void DisplayMessageChoice(CallBackInfo callBackInfo)
    {
        long puVar1;
        int witdh;
        char pcVar2;
        short psVar3;
        int tileConfig;
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
        j = 0;

        while (tileConfig < 2)
        {
            var word = _gameEngine.StaticVariables.g_asyncCallbackArgs[tileConfig];
            _gameEngine.StaticVariables.g_asyncCallbackArgs[tileConfig] = word; //TextDecoder.DecodeString(scan);
            tileConfig++;
            //for (int i = 0; i < scan.Length; i++)
            //{
            //    c = scan[i];
            //
            //    if (c == '{')
            //    {
            //        buffer[j++] = (char)TextDecoder.ConvertCp850ToLatin1(scan[i++]); //(char)(scan[i++] + 0x50);
            //    }
            //    else if (c == '}')
            //    {
            //        buffer[j++] = (char)TextDecoder.ConvertCp850ToLatin1(scan[i++]); //(scan[i++] + 0x90);
            //    }
            //    else if (c == '\0')
            //    {
            //        buffer[j] = '\0';
            //        _gameEngine.StaticVariables.g_asyncCallbackArgs[tileConfig] = new string(buffer);
            //        ++tileConfig;
            //        break;
            //    }
            //    else
            //    {
            //        buffer[j++] = c;
            //    }
            //}
        }

        //AlundraEngine.Debug.Debugger.Breakpoint();

        //DialogChoice = _gameEngine.StaticVariables.g_asyncCallbackArgs2[0] + _gameEngine.StaticVariables.g_asyncCallbackArgs2[1];
        DialogChoiceSprites[0].Clear();
        DialogChoiceSprites[1].Clear();

        Array.Clear(_gameEngine.StaticVariables.CHAR_ARRAY_8014a4e8);
        TextDecoder.RenderTextBitmap(_gameEngine, _gameEngine.StaticVariables.g_asyncCallbackArgs2[0].ToCharArray(),
            DialogChoiceSprites[0],
            0, //0x3c0, 
            0, //0x1d0,
            0, 0, 0x80, 0x10);

        Array.Clear(_gameEngine.StaticVariables.CHAR_ARRAY_8014a4e8);
        TextDecoder.RenderTextBitmap(_gameEngine, _gameEngine.StaticVariables.g_asyncCallbackArgs2[1].ToCharArray(),
            DialogChoiceSprites[1],
            0, //0x3e0, 
            0,//0x1d0,
            0, 0, 0x80, 0x10);

        j = 0;
        local_2c = 0;

        do
        {
            var i = 0;
            tileConfig = 0;

            do
            {
                k = 0;
                iVar6 = 0;
                iVar5 = 0;

                do
                {
                    witdh = TextDecoder.CalculateTextWidthFromScript(_gameEngine, _gameEngine.StaticVariables.g_asyncCallbackArgs2[i].ToCharArray());
                    index = k + i + j;// + tileConfig + local_2c;
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

        UpdateUiBoxesPosition(callBackInfo.Data, _gameEngine.StaticVariables.g_textToDisplay3);

        callBackInfo.RenderFunc = FUN_800501a4;
        InitializeDialogBackgroundSprites(callBackInfo);
    }
    //8004afe8
    public void Fun_8004afe8(CallBackInfo callBackInfo)
    {
        Breakpoint.TriggerBreak();
    }

    //8005a268
    public void FUN_8005a268(CallBackInfo callBackInfo)
    {
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
        _gameEngine.StaticVariables.g_UIDisplayFlags = 5;

        var text = _gameEngine.EtcRes.GetEtcString(_gameEngine.StaticVariables.g_entitySpriteNameTableIndex);

        DialogCharacterNameSprites.Clear();
        _gameEngine.UIManager.DisplayIconName(
            _gameEngine.StaticVariables.g_spriteMessageCharacterPortrait,
            DialogCharacterNameSprites,
            text != null ? text.ToCharArray() : null,
            6,
            0,
            (short)(textTileConfig.Height + textTileConfig.Y),
            3);

        //return 1;
    }

    //8005a3e0
    public void Fun_8005a3e0(CallBackInfo callbackInfo)
    {
        short sVar1;
        ulong uVar2;
        int i;
        int iVar4;
        int iVar5;
        short psVar6;
        SPRT pSVar7;
        SPRT puVar8;
        uint puVar9;
        SPRT pSVar10;
        //DISPENV local_30;

        if ((_gameEngine.StaticVariables.g_UIDisplayFlags & 3U) != 0)
        {
            i = UpdateUiBoxesPosition(callbackInfo.Data, _gameEngine.StaticVariables.g_textToDisplay2);

            if (i == 1)
            {
                if ((_gameEngine.StaticVariables.g_UIDisplayFlags & 1U) != 0)
                {
                    _gameEngine.StaticVariables.g_UIDisplayFlags &= 0xfffe;
                }

                if ((_gameEngine.StaticVariables.g_UIDisplayFlags & 2U) != 0)
                {
                    callbackInfo.Data.X = _gameEngine.StaticVariables.g_textToDisplay2.originX;
                    callbackInfo.Data.Y = _gameEngine.StaticVariables.g_textToDisplay2.originY;
                    FUN_8005a244(callbackInfo);
                    return; // 0;
                }
            }
        }

        i = 0;

        do
        {
            var text = _gameEngine.EtcRes.GetEtcString(_gameEngine.StaticVariables.g_entitySpriteNameTableIndex);

            var textWidth = TextDecoder.CalculateTextWidthFromScript(_gameEngine, text != null ? text.ToCharArray() : null);
            //uVar2 = g_drawModes[0x14].tag;
            //iVar5 = iVar3 + g_drawModes[0x14].tag;
            //psVar6 = callbackInfo.Data.X;
            _gameEngine.StaticVariables.g_spriteMessageCharacterPortrait[i].x0 = (short)(callbackInfo.Data.X + (callbackInfo.Data.Width * 8 - textWidth) / 2);
            _gameEngine.StaticVariables.g_spriteMessageCharacterPortrait[i].y0 = (short)(callbackInfo.Data.Y + callbackInfo.Data.Height - i);

            i += 1;
        } while (i < 1);

        iVar4 = 0;
        //puVar9 = _gameEngine.StaticVariables.DAT_80146f70[uVar2 * 0x28];
        //pSVar10 = _gameEngine.StaticVariables.g_spriteMessageCharacterPortrait;
        i = 0; //uVar2 * 0x14;

        do
        {
            //pSVar7 = pSVar10 + uVar2;
            //pSVar10 = pSVar10 + 1;
            puVar8 = _gameEngine.StaticVariables.g_spriteMessageCharacterPortrait[i];

            var sprite = _gameEngine.StaticVariables.g_spriteMessageCharacterPortrait[i];

            foreach (var spr in DialogCharacterNameSprites)
            {
                _gameEngine.Renderer.AddSprite(
                    spr.X + sprite.x0,
                    spr.Y /*+ sprite.y0*/ + 4,
                    spr.Width, spr.Height,
                    SpriteDepth.ForegroundUI, spr.Bitmap, spr.Alpha);
            }

            i += 1;
            iVar4 += 1;
            //*puVar8 = *puVar8 & 0xff000000 | *puVar9 & 0xffffff;
            //*puVar9 = *puVar9 & 0xff000000 | (uint)pSVar7 & 0xffffff;
        } while (iVar4 < 1);

        //GetDispEnv(&local_30);
        //local_30.disp.x = local_30.disp.x + *(short*)(callbackInfo->_0 + 8) + **(short**)(callbackInfo->_0 + 4);
        //local_30.disp.y = local_30.disp.y + *(short*)(callbackInfo->_0 + 10) + *(short*)(*(int*)(callbackInfo->_0 + 4) + 2);
        //local_30.disp.w = *(short*)(callbackInfo->_0 + 0xc) * 8 + 2;
        //local_30.disp.h = *(short*)(callbackInfo->_0 + 0xe) * 8 + 2;
        //
        //if (0x13f < local_30.disp.x)
        //{
        //    local_30.disp.x = 0x140;
        //}
        //
        //if (0x13f < (int)local_30.disp.x + (int)local_30.disp.w)
        //{
        //    local_30.disp.w = 0x140 - local_30.disp.x;
        //}
        //
        //SetDrawArea((DR_AREA*)(&DAT_80180290 + g_drawModes[0x14].tag * 0xc), &local_30.disp);

        //puVar8 = (uint*)(&DAT_80180290 + g_drawModes[0x14].tag * 0xc);
        //puVar9 = (uint*)(&DAT_80146f70 + g_drawModes[0x14].tag * 0x28);
        //*puVar8 = *puVar8 & 0xff000000 | *puVar9 & 0xffffff;
        //*puVar9 = *puVar9 & 0xff000000 | (uint)puVar8 & 0xffffff;
    }

    //8005a244
    private void FUN_8005a244(CallBackInfo callBackInfo)
    {
        FUN_80047cb0(callBackInfo);
        _gameEngine.StaticVariables.g_UIDisplayFlags = 0;
    }

    //80047cb0
    public void FUN_80047cb0(CallBackInfo callBackInfo)
    {
        callBackInfo.Flags = 0;
    }

    //800501a4
    private void FUN_800501a4(CallBackInfo callBackInfo)
    {
        int res = UpdateUiBoxesPosition(callBackInfo.Data, _gameEngine.StaticVariables.g_textToDisplay3);
        if (res != 0)
        {
            callBackInfo.RenderFunc = FUN_8004ffa8;
        }

        FUN_8004fce8(callBackInfo);
    }

    //8004ffa8
    private void FUN_8004ffa8(CallBackInfo callBackInfo)
    {
        UpdateUiBoxesPosition(callBackInfo.Data, _gameEngine.StaticVariables.g_textToDisplay3);

        if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & 0x40) != 0)
        {
            _gameEngine.StaticVariables.g_textToDisplay3.mode = 2;
            _gameEngine.StaticVariables.g_textToDisplay3.tick = 0;
            _gameEngine.StaticVariables.g_textToDisplay3.speed = 0xf;
            _gameEngine.StaticVariables.g_textToDisplay3.x = callBackInfo.Data.X;
            _gameEngine.StaticVariables.g_sprites[0].tag = (ulong)_gameEngine.StaticVariables.g_asyncOperationCountdown + 1;

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
            uint soundSfxId = 3;

            if (_gameEngine.StaticVariables.g_sprites[0].tag == 1)
            {
                soundSfxId = 2;
            }

            _gameEngine.SoundManager.PlaySoundEffect(soundSfxId);
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

        iVar1 = UpdateUiBoxesPosition(callBackInfo.Data, _gameEngine.StaticVariables.g_textToDisplay3);

        if (iVar1 == 1)
        {
            callBackInfo.Data.X = _gameEngine.StaticVariables.g_textToDisplay3.originX;
            callBackInfo.Data.Y = _gameEngine.StaticVariables.g_textToDisplay3.originY;
            _gameEngine.UIManager.FUN_80047cb0(callBackInfo);
            _gameEngine.StaticVariables.g_asyncCallback((int)_gameEngine.StaticVariables.g_sprites[0].tag);
        }
        else
        {
            FUN_8004fce8(callBackInfo);
        }
    }

    //8004fce8
    private void FUN_8004fce8(CallBackInfo callBackInfo)
    {
        short sVar2;
        int iVar4;
        int j;
        int i;
        short sVar10;

        sVar2 = (short)_gameEngine.StaticVariables.g_asyncOperationCountdown;
        iVar4 = _gameEngine.StaticVariables.g_asyncCallbackArgs2[_gameEngine.StaticVariables.g_asyncOperationCountdown].Length;

        _gameEngine.GraphicManager.ApplyFadeTransform(
            _gameEngine.StaticVariables.g_sprites,
            (short)(callBackInfo.Data.X + callBackInfo.Data.Width + sVar2 * 0x30 + iVar4 * 4 - 8),
            (short)(callBackInfo.Data.Y + callBackInfo.Data.Height - 0x10),
            0);

        FUN_800507e4(_gameEngine.StaticVariables.g_sprites);

        i = 0;
        j = 0;
        iVar4 = 0;
        sVar10 = 0;

        do
        {
            var sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017e674[i];
            sprite.x0 = (short)(callBackInfo.Data.Width + callBackInfo.Data.X + sVar10);
            sprite.y0 = (short)(callBackInfo.Data.Height + callBackInfo.Data.Y - j);

            //puVar7 = (_gameEngine.StaticVariables.DAT_80146f70 + iVar8);
            //puVar5 = (uint*)((int)_gameEngine.StaticVariables.SPRT_ARRAY_8017e674[0].tag + iVar6 + iVar4);
            /* Probable PsyQ macro: addPrim(). */
            //*puVar5 = *puVar5 & 0xff000000 | *puVar7 & 0xffffff;
            //*puVar7 = *puVar7 & 0xff000000 | (uint)pSVar9 & 0xffffff;

            iVar4 += 0x3c;
            i += 1;
            //sVar10 = sVar10 + 0x30;

        } while (i < 2);
    }



    //800507e4
    public void FUN_800507e4(SPRT[] sprites)
    {
        SPRT pSVar1;
        byte value;
        SPRT spriteTemp;
        SPRT sprite;
        SPRT spriteSource;

        sprite = sprites[0];
        value = (byte)(sprite.r0 + 1);
        sprite.r0 = value;

        if (value == 0x28)
        {
            sprite.r0 = 0;
            sprite.g0 = 0;
            sprite.b0 = 0;
            sprite.code = 0;
        }

        sprites[0].u0 = _gameEngine.StaticVariables.g_dialogCursorTextureUV[sprite.r0 / 10 * 0x28];
        sprites[0].v0 = _gameEngine.StaticVariables.g_dialogCursorTextureUV[sprite.r0 / 10 * 0x28 + 1];

        //AlundraEngine.Debug.Debugger.Breakpoint();
        //TODO remove this
        sprite.x0 = sprite.w;
        sprite.y0 = sprite.h;
        sprite.w = 16;
        sprite.h = 16;

        //spriteSource = _gameEngine.StaticVariables.SPRT_80146f5c[0];
        //
        //sprite.x0 = spriteSource.x0;
        //sprite.y0 = spriteSource.y0;
        //sprite.u0 = spriteSource.u0;
        //sprite.v0 = spriteSource.v0;
        //sprite.clut = spriteSource.clut;
        //
        //pSVar1 = sprites[0];
        //spriteTemp.x0 = sprite.x0;
        //spriteTemp.y0 = sprite.y0;
        //spriteTemp.u0 = sprite.u0;
        //spriteTemp.v0 = sprite.v0;
        //spriteTemp.clut = sprite.clut;
        ///* Probable PsyQ macro: addPrim(). */
        ////uVar4 = uVar3 & 0xff000000 | uVar4 & 0xffffff;
        //sprite = sprites[0];
        //sprite.x0 = spriteTemp.x0;
        //sprite.y0 = spriteTemp.y0;
        //spriteTemp.u0 = sprite.u0;
        //spriteTemp.v0 = sprite.v0;
        //spriteTemp.clut = sprite.clut;
        ////uVar4 = uVar2 & 0xff000000 | (uint)&pSVar1.x0 & 0xffffff;
        //sprite.u0 = (char)uVar4;
        //sprite.v0 = (char)(uVar4 >> 8);
        //sprite.clut = (short)(uVar4 >> 0x10);

        var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
        _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);
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
            _gameEngine.StaticVariables.g_dialog_flags |= 2;

            _gameEngine.SoundManager.PlaySoundEffect(7);
            ResetHudTransitionState();
            _gameEngine.MainInventoryManager.UpdateHudTransitionState();

            _gameEngine.StaticVariables.g_backgroundMessageAnimation.mode = 2;
            _gameEngine.StaticVariables.g_backgroundMessageAnimation.tick = 0;
            _gameEngine.StaticVariables.g_backgroundMessageAnimation.speed = 0xf;

            if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
            {
                _gameEngine.StaticVariables.g_backgroundMessageAnimation.x = (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X + _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
            }
            else
            {
                _gameEngine.StaticVariables.g_backgroundMessageAnimation.x = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
            }

            if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y < 0)
            {
                _gameEngine.StaticVariables.g_backgroundMessageAnimation.y = (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y + _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Height * -8);
            }
            else
            {
                _gameEngine.StaticVariables.g_backgroundMessageAnimation.y = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y;
            }

            if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
            {
                _gameEngine.StaticVariables.g_backgroundMessageAnimation.startX = (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X + _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
            }
            else
            {
                _gameEngine.StaticVariables.g_backgroundMessageAnimation.startX = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
            }

            _gameEngine.StaticVariables.g_backgroundMessageAnimation.startY = 0xf0;
        }
    }

    //80059fe0
    private void ResetHudTransitionState()
    {
        if ((_gameEngine.StaticVariables.g_UIDisplayFlags & 4U) != 0)
        {
            _gameEngine.StaticVariables.g_UIDisplayFlags = 6;
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
    public int UpdateUiBoxesPosition(UIBoxConfiguration uiBoxConfiguration, TextToDisplay textToDisplay)
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
        }
        else
        {
            int xInterp = textToDisplay.x + (textToDisplay.startX - textToDisplay.x) * textToDisplay.tick / textToDisplay.speed;
            int yInterp = textToDisplay.y + (textToDisplay.startY - textToDisplay.y) * textToDisplay.tick / textToDisplay.speed;
            uiBoxConfiguration.X = (short)xInterp;
            uiBoxConfiguration.Y = (short)yInterp;
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


                        sprt = uiBoxConfiguration.SpritesB[i];
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
        _gameEngine.UIManager.FUN_80047cb0(callBackInfo);
        _gameEngine.StaticVariables.g_dialog_flags = 0;
        _gameEngine.StaticVariables.g_playerControlFlags &= 0xffffffe7;
    }

    

    //80084f90
    private void LoadImage(Rectangle rectangle, char[] buffer)
    {
        //Debug.WriteLine("LoadImage: " + rectangle);
        //AlundraEngine.Debug.Debugger.Breakpoint();
    }

    //800455b4
    private void RenderText(CallBackInfo callbackInfo)
    {
        if (callbackInfo == null || callbackInfo.Data == null)
        {
            return;
        }

        int currentLine;
        short offsetY;
        short sVar6;
        int lineIndex;
        int bufferX;
        bool holdActive;

        bufferX = _gameEngine.StaticVariables.g_textBufferX;
        lineIndex = 0;
        sVar6 = 0;
        do
        {
            offsetY = 0;
            currentLine = (bufferX + lineIndex) % 3;

            var sprite = _gameEngine.StaticVariables.g_textFullLinesSprites[currentLine];

            if (_gameEngine.StaticVariables.g_textLineWidth[currentLine] == 0)
            {
                sprite.x0 = (short)(callbackInfo.Data.X + callbackInfo.Data.Width);
                sprite.y0 = (short)(callbackInfo.Data.Y + callbackInfo.Height + offsetY + sVar6);
            }
            else
            {
                sprite.x0 = (short)(callbackInfo.Data.X + (callbackInfo.Data.Width * 8 - _gameEngine.StaticVariables.g_textLineWidth[currentLine]) / 2);
                sprite.y0 = (short)(callbackInfo.Data.Y + callbackInfo.Data.Height + offsetY + sVar6);
            }

            //offsetY = offsetY + -1;
            //pPrimitiveEntry = _gameEngine.StaticVariables.g_textFullLinesSprites[currentLine * 2].tag;
            //pOrderTable = _gameEngine.StaticVariables.DAT_80146f60 + uVar1 * 0x28);

            /* Probable PsyQ macro: addPrim(). */
            //*pPrimitiveEntry = *pPrimitiveEntry & 0xff000000 | *pOrderTable & 0xffffff;
            //*pOrderTable = *pOrderTable & 0xff000000 | _gameEngine.StaticVariables.g_textFullLinesSprites[uVar1 + currentLine * 2].tag + iVar5 & 0xffffffU;
            //uVar2 = _gameEngine.StaticVariables.g_drawModes[0x14].tag;


            foreach (var spr in DialogLinesSprites[lineIndex])
            {
                _gameEngine.Renderer.AddSprite(
                    spr.X + callbackInfo.Data.Width,
                    /*spr.Y*/lineIndex * 0x10 + sprite.y0,
                    spr.Width, spr.Height,
                    SpriteDepth.ForegroundUI, spr.Bitmap, spr.Alpha);
            }

            lineIndex += 1;
        } while (lineIndex < 3);

        //pOrderTable = _gameEngine.StaticVariables.DAT_80153010 + _gameEngine.StaticVariables.g_drawModes[0x14].tag * 0xc);
        //pPrimitiveEntry = _gameEngine.StaticVariables.DAT_80146f60 + _gameEngine.StaticVariables.g_drawModes[0x14].tag * 0x28);
        /* Probable PsyQ macro: addPrim(). */
        //*pOrderTable = *pOrderTable & 0xff000000 | *pPrimitiveEntry & 0xffffff;
        //*pPrimitiveEntry = *pPrimitiveEntry & 0xff000000 | (uint)pOrderTable & 0xffffff;

        holdActive = _gameEngine.StaticVariables.g_textHoldState != 0;

        //display waiting cursor
        if (holdActive)
        {
            _gameEngine.StaticVariables.INT_80149cd4 += 1;

            if (0x27 < _gameEngine.StaticVariables.INT_80149cd4)
            {
                _gameEngine.StaticVariables.INT_80149cd4 = 0;
            }

            _gameEngine.StaticVariables.g_cursorTextSprites[0].u0 = _gameEngine.StaticVariables.g_dialogCursorTextureUV[_gameEngine.StaticVariables.INT_80149cd4 / 10 * 0x28];
            _gameEngine.StaticVariables.g_cursorTextSprites[0].v0 = _gameEngine.StaticVariables.g_dialogCursorTextureUV[_gameEngine.StaticVariables.INT_80149cd4 / 10 * 0x28 + 1];
            _gameEngine.StaticVariables.g_cursorTextSprites[0].x0 = (short)(callbackInfo.Data.X + callbackInfo.Data.Width * 8 - 0x10);
            _gameEngine.StaticVariables.g_cursorTextSprites[0].y0 = (short)(callbackInfo.Data.Y + callbackInfo.Data.Height * 8 - 0x18);

            //uVar1 = g_drawModes[0x14].tag;
            //psVar3 = (callbackInfo.X);
            //pSVar4 = _gameEngine.StaticVariables.g_cursorTextSprites[0];
            //pOrderTable = _gameEngine.StaticVariables.g_drawModes + uVar1 * 0x28 + 0xf8);
            /* Probable PsyQ macro: addPrim(). */
            //pSVar4.tag = pSVar4.tag & 0xff000000 | *pOrderTable & 0xffffff;
            //*pOrderTable = *pOrderTable & 0xff000000 | (uint)pSVar4 & 0xffffff;

            var sprite = _gameEngine.StaticVariables.g_cursorTextSprites[0];
            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);
        }
    }

    //800450f0
    public int InitializeDialogMessage(string scriptText, int controlPlayer)
    {
        if (_gameEngine.GraphicManager.SetTransitionType(0) == 0)
        {
            return 0;
        }

        if (scriptText.Length >= 0x960)
        {
            Breakpoint.TriggerBreak();
            var message = "the sub text is too long"; //サブテキストが長すぎます!
            Array.Copy(message.ToCharArray(), _gameEngine.StaticVariables.g_scriptBuffer, message.Length);
        }
        else
        {
            Array.Clear(_gameEngine.StaticVariables.g_scriptBuffer);
            Array.Copy(scriptText.ToCharArray(), _gameEngine.StaticVariables.g_scriptBuffer, scriptText.Length);
        }

        _gameEngine.StaticVariables.g_backgroundMessageAnimation.mode = 2;
        _gameEngine.StaticVariables.g_backgroundMessageAnimation.tick = 0;
        _gameEngine.StaticVariables.g_backgroundMessageAnimation.speed = 0xF;

        var baseX = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        if (baseX < 0)
        {
            var offset = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width;
            baseX = (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X - offset * 8);
        }

        _gameEngine.StaticVariables.g_backgroundMessageAnimation.x = baseX;
        _gameEngine.StaticVariables.g_backgroundMessageAnimation.y = 0xF0;

        var startX = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;

        if (startX < 0)
        {
            var width = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width;
            startX = (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X - width * 8);
        }

        _gameEngine.StaticVariables.g_backgroundMessageAnimation.startX = startX;

        var startY = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y;

        if (startY < 0)
        {
            var height = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Height;
            startY = (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y - height * 8);
        }

        _gameEngine.StaticVariables.g_backgroundMessageAnimation.startY = startY;
        _gameEngine.StaticVariables.g_backgroundMessageAnimation.originX = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        _gameEngine.StaticVariables.g_backgroundMessageAnimation.originY = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y;
        
        _gameEngine.StaticVariables.g_dialog_flags = 5;
        _gameEngine.StaticVariables.g_playerControlFlags |= (uint)(controlPlayer == 1 ? 0x10 : 0x8);

        _gameEngine.StaticVariables.g_textPrimitives = 0;
        _gameEngine.StaticVariables.g_textHoldState_2 = 0;
        _gameEngine.StaticVariables.g_textMessageConfirmed = 0;
        _gameEngine.StaticVariables.g_textAutoAdvanceFlag_2 = 0;
        _gameEngine.StaticVariables.g_textAutoAdvanceFlag = 0;
        _gameEngine.StaticVariables.g_currentVoiceSfxId = -1;
        _gameEngine.StaticVariables.g_textBufferX = 0;
        _gameEngine.StaticVariables.g_textLineIndex = 0;
        _gameEngine.StaticVariables.g_textCursor = 0;
        _gameEngine.StaticVariables.g_textRenderStep = 0;

        byte yOffset = 0x20;

        // 3 lines to display the text in the dialog box
        DialogChoiceSprites[0].Clear();
        DialogChoiceSprites[1].Clear();
        //DialogCharacterNameSprites.Clear();
        foreach (var dialogLinesSprite in DialogLinesSprites)
        {
            dialogLinesSprite.Clear();
        }

        for (int group = 0; group < 3; ++group)
        {
            for (int y = 0; y < 2; ++y)
            {
                for (int x = 0; x < 1; ++x)
                {
                    var index = x + y + group;

                    var sprite = _gameEngine.StaticVariables.g_textFullLinesSprites[index];
                    sprite.w = 0xff;
                    sprite.h = 0x10;
                    sprite.u0 = 0;
                    sprite.v0 = yOffset;
                    sprite.clut = (ushort)(8 - x); // StaticVariables.g_clutTable[8 - x]);

                    //SetSprt(sprt);
                    //SetSemiTrans(sprt, 0);
                    //SetShadeTex(sprt, 1);

                    //display a buffer created with text
                    //var bitmap = Font3.GenerateFontBitmapFromSprite(sprite);
                    //Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

                    x += 1;
                }
            }

            yOffset += 0x10;
        }

        //dialog cursor
        var i = 0;

        do
        {
            var sprite = _gameEngine.StaticVariables.g_cursorTextSprites[i];
            sprite.w = 0x10;
            sprite.h = 0x10;
            sprite.u0 = _gameEngine.StaticVariables.g_dialogCursorTextureUV[0];
            sprite.v0 = _gameEngine.StaticVariables.g_dialogCursorTextureUV[1];
            sprite.x0 = 0;
            sprite.y0 = 0;
            sprite.clut = 8; //StaticVariables.g_clutTable[8];

            //SetSprt(sprite);
            //SetSemiTrans(sprite, 0);
            //SetShadeTex(sprite, 1);

            //var bitmap = Font3.GenerateHudBitmapFromSprite(sprite);
            //Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

            i += 1;
        } while (i < 2);

        // Efface une zone d’écran pour préparer l’affichage
        //RECT clearRect;
        //clearRect.x = 0x3C0;
        //clearRect.y = 0x120;
        //clearRect.w = 0x40;
        //clearRect.h = 0x30;
        //ClearImage(&clearRect, 0, 0, 0);

        _gameEngine.StaticVariables.g_textFlags = 3;
        _gameEngine.StaticVariables.g_textDelayReset = 4;
        _gameEngine.StaticVariables.g_textDelay = 1;
        _gameEngine.StaticVariables.g_debugFlags_2 = 3;
        _gameEngine.StaticVariables.g_etcAnimationMode = 3;
        _gameEngine.StaticVariables.g_textLineStartX = 0;
        _gameEngine.StaticVariables.g_textHoldState = 0;
        Array.Clear(_gameEngine.StaticVariables.g_textBuffer);
        _gameEngine.SoundManager.PlaySoundEffect(6);

        return 1;
    }

    //800472d0
    public void DisplayIconName(SPRT[] sprites,
        List<Sprite> spritesToDisplay,
        char[]? text, int textLength,
        short textCoordDstX, short textCoordDstY,
        int displayMode)
    {
        if (text == null || text?.Length == 0)
        {
            return;
        }

        int i;
        short y;
        int j;
        int k = 0;
        char[] formattedText = new char[128];
        ushort clut;
        char currentChar;
        bool useStyledText;

        Array.Fill(formattedText, ' ');
        formattedText[^1] = '\0';
        Array.Clear(_gameEngine.StaticVariables.g_textBuffer);
        formattedText[0] = '\0';

        if (3 < displayMode)
        {
            displayMode += 1;
        }

        if (displayMode < 0xe)
        {
            useStyledText = TextDecoder.ContainsSpecialTextFormatting(text[k]);

            if (useStyledText)
            {
                useStyledText = false;
                j = 0;

                while (k < text.Length)
                {
                    currentChar = text[k];

                    if (currentChar == '\0')
                    {
                        break;
                    }

                    if (currentChar == '{')
                    {
                        k++;
                        //formattedText[j] = (char)(text[k] + 'P');
                        formattedText[j] = TextDecoder.DecodeCharacter(text[k]);
                    }
                    else if (currentChar == '}')
                    {
                        //é
                        //9 14
                        k++;
                        //formattedText[j] = (char)(text[k] - 0x70);
                        formattedText[j] = TextDecoder.DecodeCharacter(text[k]);
                    }
                    else
                    {
                        formattedText[j] = currentChar;
                    }

                    k++;
                    j += 1;
                }

                /*
                formattedText[j] = '\0';

                while (true)
                {
                    //j = strlen(formattedText);
                    if (0x7f < j)
                    {
                        break;
                    }

                    strcat(formattedText, " ");
                }*/
            }
            else
            {
                Array.Copy(text, formattedText, text.Length);
                useStyledText = true;
            }

            if (useStyledText)
            {
                RenderStyledText(formattedText, 0x3c0,
                                 (short)((uint)((displayMode * 0x10 + 0x120) * 0x10000) >> 0x10));
            }
            else
            {
                //RenderTextBitmap(
                //    formattedText, _gameEngine.StaticVariables.g_textBuffer, 0x3c0,
                //    (short)((uint)((displayMode * 0x10 + 0x120) * 0x10000) >> 0x10),
                //    0, 0, 0x100, 0x10);
                TextDecoder.RenderTextBitmap(_gameEngine,
                    formattedText, spritesToDisplay, //_gameEngine.StaticVariables.g_textBuffer,
                    textCoordDstX, textCoordDstY,
                    0, 0, 0x10, 0x10);
            }

            j = 0;

            do
            {
                i = 0;
                y = textCoordDstY;

                do
                {
                    var sprite = sprites[j];
                    sprite.w = 0xff;
                    sprite.h = 0x10;
                    sprite.u0 = (byte)'\0';
                    sprite.v0 = (byte)(displayMode * '\x10' + ' ');
                    sprite.x0 = textCoordDstX;
                    sprite.y0 = y;
                    sprite.clut = (ushort)(8 - i); //_gameEngine.StaticVariables.g_clutTable[8 - i]

                    //SetSprt(spritePtr);
                    //SetSemiTrans(spritePtr, 0);
                    //SetShadeTex(spritePtr, 1);

                    //y = (short)(y - 1);
                    i += 1;
                } while (i < 1);

                j += 1;
            } while (j < 2);
        }
        else
        {
            Breakpoint.TriggerBreak();
            //DoNothing();
        }
    }

    //8004f374
    private void RenderStyledText(char[] text, short posX, short posY)
    {
        Breakpoint.TriggerBreak();
    }

    //80045054
    public void TryActivateTextHoldState()
    {
        if ((_gameEngine.StaticVariables.g_etcAnimationMode & 4U) != 0)
        {
            _gameEngine.StaticVariables.g_textHoldState_2 = 1;
        }
    }


    //80050c88
    public int FUN_80050c88(string arg1, string arg2, ref int val)
    {
        val = 0;

        int i = 0;
        int maxLen = 0x80;

        if (arg1 == null)
        {
            arg1 = string.Empty;
        }

        if (arg2 == null)
        {
            arg2 = string.Empty;
        }

        Array.Clear(_gameEngine.StaticVariables.CHAR_ARRAY_8017e490);

        while (i < maxLen && i < arg1.Length && arg1[i] != '\0')
        {
            _gameEngine.StaticVariables.CHAR_ARRAY_8017e490[i] = arg1[i];
            i++;
        }

        i = 0;
        int base2 = 0x81;

        Array.Clear(_gameEngine.StaticVariables.CHAR_ARRAY_8017e511);

        while (i < maxLen && i < arg2.Length && arg2[i] != '\0')
        {
            int index = base2 + i;
            if (index < _gameEngine.StaticVariables.CHAR_ARRAY_8017e511.Length)
            {
                _gameEngine.StaticVariables.CHAR_ARRAY_8017e511[index] = arg2[i];
            }

            i++;
        }

        int setTransResult = _gameEngine.GraphicManager.SetTransitionType(9);

        if (setTransResult == 0)
        {
            return 0;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017e620.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_8017e620.speed = 0xF;

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017e620.x =
                (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X +
                        _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017e620.x = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017e620.y = 0xf0;

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017e620.startX =
                (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X +
                        _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017e620.startX = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        }

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017e620.startY =
                (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y +
                        _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017e620.startY = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017e620.originX = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        _gameEngine.StaticVariables.TextToDisplay_8017e620.originY = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y;
        _gameEngine.StaticVariables.g_memoryCardMenuState = 5;

        _gameEngine.UIManager.DisplayIconName(
            _gameEngine.StaticVariables.SPRT_ARRAY_8017e410, 
            DialogSaveGameSprites,
            _gameEngine.StaticVariables.CHAR_ARRAY_8017e490, 
            0x40,
            (short)(_gameEngine.StaticVariables.g_activeTransitionCallback.Data.X + _gameEngine.StaticVariables.g_activeTransitionCallback.X),
            (short)(_gameEngine.StaticVariables.g_activeTransitionCallback.Data.Y + _gameEngine.StaticVariables.g_activeTransitionCallback.Y), 1);

        _gameEngine.UIManager.DisplayIconName(
            _gameEngine.StaticVariables.SPRT_ARRAY_8017e438, 
            DialogSaveGameSprites,
            _gameEngine.StaticVariables.CHAR_ARRAY_8017e511, 
            0x40,
            (short)(_gameEngine.StaticVariables.g_activeTransitionCallback.Data.X + _gameEngine.StaticVariables.g_activeTransitionCallback.X),
            (short)(_gameEngine.StaticVariables.g_activeTransitionCallback.Data.Y + _gameEngine.StaticVariables.g_activeTransitionCallback.Y + 0x10), 2);

        return 1;
    }

}