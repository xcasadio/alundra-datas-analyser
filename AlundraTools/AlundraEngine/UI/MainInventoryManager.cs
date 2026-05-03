using System.Diagnostics;
using AlundraEngine.Gameplay;
using AlundraEngine.Graphics;
using static AlundraEngine.Graphics.Renderer;

namespace AlundraEngine.UI;

public class MainInventoryManager
{
    private readonly GameEngine _gameEngine;

    public readonly List<Sprite> InventoryWeaponNameSprites = new();
    public readonly List<Sprite>[] InventoryWeaponDescriptionLinesSprites = [new(), new()];
    public readonly List<Sprite> InventoryItemNameSprites = new();

    public MainInventoryManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    //80054a34
    public void InitializeInventorySpriteNumberOf()
    {
        FUN_800548a4(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground);
        FUN_800548a4(_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground);
        FUN_800548a4(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground);
        FUN_800548a4(_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground);
        FUN_800548a4(_gameEngine.StaticVariables.UIBoxConfiguration_800b9a10);
        FUN_800548a4(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons);
        _gameEngine.StaticVariables.g_forbiddenWarpFlag = 0;
        _gameEngine.StaticVariables.g_inventorySelectedSlotId = 0;
        FUN_80050998(_gameEngine.StaticVariables.g_inventoryCursorAnimation);

        var i = 0;

        do
        {
            var sprite = _gameEngine.StaticVariables.g_mainInventoryNumberOfHerbsSprites[i];
            sprite.w = 8;
            sprite.h = 0x10;
            sprite.clut = 5;

            //SetSprt(sprite);
            //SetSemiTrans(sprite, 0);
            //SetShadeTex(sprite, 1);

            i++;
        } while (i < 2);

        i = 0;

        do
        {
            //SetSprt(sprite2);
            //SetSemiTrans(sprite2, 0);
            //SetShadeTex(sprite2, 1);

            var sprite = _gameEngine.StaticVariables.g_spriteInventoryMoneyAmount[i];
            sprite.w = 8;
            sprite.h = 0x10;
            sprite.clut = 5; //_gameEngine.StaticVariables.g_clutTableIndex; //_gameEngine.StaticVariables.g_clutTable[_gameEngine.StaticVariables.g_clutTableIndex];
            sprite.r0 = 0x80;
            sprite.g0 = 0x80;
            sprite.b0 = 0x80;

            i++;
        } while (i < 8);

        i = 0;

        do
        {
            //SetSprt(sprite2);
            //SetSemiTrans(sprite2, 0);
            //SetShadeTex(sprite2, 1);
            var sprite = _gameEngine.StaticVariables.g_spriteInventoryNumberOfKeys[i];
            sprite.w = 8;
            sprite.h = 0x10;
            sprite.clut = 5; //_gameEngine.StaticVariables.g_clutTableIndex; //_gameEngine.StaticVariables.g_clutTable[_gameEngine.StaticVariables.g_clutTableIndex];
            sprite.r0 = 0x80;
            sprite.g0 = 0x80;
            sprite.b0 = 0x80;

            i++;
        } while (i < 4);

        i = 0;

        do
        {
            //SetSprt(sprite2);
            //SetSemiTrans(sprite2, 0);
            //SetShadeTex(sprite2, 1);

            var sprite = _gameEngine.StaticVariables.g_spriteInventoryNumberOfFalcon[i];
            sprite.w = 8;
            sprite.h = 0x10;
            sprite.clut = 5; //_gameEngine.StaticVariables.g_clutTableIndex; //_gameEngine.StaticVariables.g_clutTable[_gameEngine.StaticVariables.g_clutTableIndex];
            sprite.r0 = 0x80;
            sprite.g0 = 0x80;
            sprite.b0 = 0x80;

            i++;
        } while (i < 4);
    }

    //80050998
    public void FUN_80050998(InventoryCursorAnimation cursorAnim)
    {
        int index;
        int iVar2;

        index = 0;
        iVar2 = 8;

        do
        {
            var sprite = cursorAnim.Sprites[index];
            sprite.w = 0x10;
            sprite.h = 0x10;
            sprite.u0 = _gameEngine.StaticVariables.g_inventoryCursorTextureUVs[0];
            sprite.v0 = _gameEngine.StaticVariables.g_inventoryCursorTextureUVs[1];
            //SetSprt(p);
            //SetSemiTrans(p, 0);
            //SetShadeTex(p, 1);
            UpdateCursorSpritePosition(cursorAnim, 0, 0, index);
            sprite.clut = 0; //_gameEngine.StaticVariables.g_clutTable[0];

            index += 1;

        } while (index < 2);
    }

    //80050908
    public void UpdateCursorSpritePosition(InventoryCursorAnimation cursorAnim, short x, short y, int index)
    {
        cursorAnim.Sprites[index].x0 = (short)(_gameEngine.StaticVariables.g_inventoryCursorAnimSpriteX[cursorAnim.FrameDelay / 10] + x);
        cursorAnim.Sprites[index].y0 = (short)(_gameEngine.StaticVariables.g_inventoryCursorAnimSpriteY[cursorAnim.FrameDelay / 10] + y);
    }

    //800548a4
    private void FUN_800548a4(UIBoxConfiguration textTilesConfig)
    {
        int index;
        int col;
        int row;
        int mode;
        short w;

        row = 0;

        if (0 < textTilesConfig.Height)
        {
            do
            {
                w = textTilesConfig.Width;
                col = 0;

                if (0 < textTilesConfig.Width)
                {
                    do
                    {
                        //SetSprt(tileConfig.spritesA + row * w + col);
                        //SetSemiTrans(tileConfig.spritesA + row * textTilesConfig.width + col, 0);
                        //SetShadeTex(tileConfig.spritesA + row * textTilesConfig.width + col, 1);

                        index = row * textTilesConfig.Width + col;
                        var clut = textTilesConfig.SpritesA[index].clut;
                        //clut = _gameEngine.StaticVariables.g_clutTable[textTilesConfig.SpritesA[index].clut];
                        textTilesConfig.SpritesA[index].clut = clut;
                        //textTilesConfig.SpritesB[index].clut = _gameEngine.StaticVariables.g_clutTable[textTilesConfig.SpritesB[index].clut];
                        col += 1;

                    } while (col < textTilesConfig.Width);
                }

                row += 1;

            } while (row < textTilesConfig.Height);
        }
    }

    //80057c84
    public void StartHudTransition(int x, int y, int z,
        int destX, int destY,
        byte uvX, byte uvY,
        short width, short height,
        Bitmap image)

    {
        _gameEngine.StaticVariables.g_hudTransitionStartX = 8;
        _gameEngine.StaticVariables.g_hudTransitionStartY = 0x74;
        InitializeHudTransitionVariables(x, y, z, destX, destY, uvX, uvY, width, height, image);
    }

    //80057c18
    public void InitializeHudTransitionVariablesAndSetStart(int srcX, int srcY, int srcZ,
        int dstX, int dstY,
        sbyte u, sbyte v,
        Bitmap image)
    {
        _gameEngine.StaticVariables.g_hudTransitionStartX = 0xf8;
        _gameEngine.StaticVariables.g_hudTransitionStartY = 0x68;
        _gameEngine.MainInventoryManager.InitializeHudTransitionVariables(srcX, srcY, srcZ,
            dstX, dstY, (byte)u, (byte)v, 0x30, 0x38, image);
    }

    //80057cf0
    public void InitializeHudTransitionVariables(
        int srcX, int srcY, int srcZ,
        int dstXPtr, int dstYPtr,
        byte uvX, byte uvY,
        short width, short height,
        Bitmap image)
    {
        int i;
        byte uvBottom;
        byte uvRight;


        if (_gameEngine.StaticVariables.g_hudTransitionState == 0)
        {
            i = 0;
            uvRight = (byte)(uvX + width);
            uvBottom = (byte)(uvY + height);
            _gameEngine.StaticVariables.g_hudTransitionState = 5;
            _gameEngine.StaticVariables.g_hudTransitionSrcX = srcX >> 16;
            _gameEngine.StaticVariables.g_hudTransitionSrcY = srcY >> 16;
            _gameEngine.StaticVariables.g_hudTransitionSrcZ = srcZ >> 16;
            _gameEngine.StaticVariables.g_hudTransitionDstXPtr = dstXPtr;
            _gameEngine.StaticVariables.g_hudTransitionDstYPtr = dstYPtr;

            _gameEngine.StaticVariables.g_spriteCharacterPortraitImage = image;

            do
            {
                var poly = _gameEngine.StaticVariables.g_spriteCharacterPortrait[i];

                //SetPolyFT4((POLY_FT4*)poly);
                poly.r0 = 0xff;
                poly.g0 = 0xff;
                poly.b0 = 0xff;

                poly.u0 = uvX;
                poly._2 = uvY;

                poly.u1 = uvRight;
                poly._3 = uvY;

                poly.v2 = uvBottom;
                poly.u2 = uvX;

                poly.u3 = uvRight;
                poly.v3 = uvBottom;

                poly.x0 = 100;
                poly.y0 = 100;

                poly.x1 = (short)(width + 100);
                poly.y1 = 100;

                poly.x2 = 100;
                poly.y2 = (short)(height + 100);

                poly.x3 = (short)(width + 100);
                poly.y3 = (short)(height + 100);

                //puVar1[9] = textureId1;
                //puVar1[0xd] = textureId2;
                //puVar1 = puVar1 + 0x14;

                i += 1;
            } while (i < 2);

            //GraphicManager.DrawPolyFt4(0xff, 0xff, 0xff,
            //    100, 100, 
            //    width + 100, 
            //    100, 100, 
            //    height + 100, 
            //    width + 100, 
            //    height + 100, image);

            _gameEngine.StaticVariables.g_hudDeltaX = _gameEngine.StaticVariables.g_hudTransitionSrcX + 2
                                          - _gameEngine.StaticVariables.g_hudTransitionDstXPtr;

            _gameEngine.StaticVariables.g_hudX = _gameEngine.StaticVariables.g_hudDeltaX - _gameEngine.StaticVariables.g_hudTransitionStartX;
            _gameEngine.StaticVariables.g_hudTransitionHalfWidth = 0x30;
            _gameEngine.StaticVariables.g_hudTransitionHalfHeight = 0x38;
            _gameEngine.StaticVariables.g_hudCurrentX = _gameEngine.StaticVariables.g_hudTransitionStartX;
            _gameEngine.StaticVariables.g_hudCurrentY = _gameEngine.StaticVariables.g_hudTransitionStartY;
            _gameEngine.StaticVariables.g_hudTransitionStepValue = 0xf;
            _gameEngine.StaticVariables.g_hudDeltaY =
                _gameEngine.StaticVariables.g_hudTransitionSrcY + 2
                - _gameEngine.StaticVariables.g_hudTransitionDstYPtr
                - (_gameEngine.StaticVariables.g_hudTransitionSrcZ + 2)
                - 0x20;
            _gameEngine.StaticVariables.g_hudY = _gameEngine.StaticVariables.g_hudDeltaY - _gameEngine.StaticVariables.g_hudTransitionStartY;
        }
    }

    //80057ebc
    public void UpdateHudTransitionVariables()
    {
        ushort uVar1;
        char uVar4;
        short cameraY;
        short offsetY;
        short offsetX;
        short cameraX;
        int divisionHalfWidth;
        int divisionHalfHeight;
        int iVar5;
        int scaledHalfHeight;
        char alphaValue;

        offsetX = 0;
        offsetY = 0;
        uVar4 = '\0';

        if (_gameEngine.StaticVariables.g_hudTransitionState == 0)
        {
            return;
        }

        cameraX = (short)_gameEngine.StaticVariables.g_hudCurrentX;
        cameraY = (short)_gameEngine.StaticVariables.g_hudCurrentY;

        if (_gameEngine.StaticVariables.g_hudTransitionStepValue == 0)
        {
            uVar1 = (ushort)(_gameEngine.StaticVariables.g_hudTransitionState & 2);
            _gameEngine.StaticVariables.g_hudTransitionState = (short)(_gameEngine.StaticVariables.g_hudTransitionState & 0xfffe);
            if (uVar1 == 0)
            {
                uVar4 = (char)0x80;
                offsetX = (short)_gameEngine.StaticVariables.g_hudTransitionHalfWidth;
                offsetY = (short)_gameEngine.StaticVariables.g_hudTransitionHalfHeight;
            }
            else
            {
                cameraX = (short)_gameEngine.StaticVariables.g_hudDeltaX;
                cameraY = (short)_gameEngine.StaticVariables.g_hudDeltaY;
                _gameEngine.StaticVariables.g_hudTransitionState = 0;
            }
            goto FinalizeTransitionUpdate;
        }

        cameraX = (short)(_gameEngine.StaticVariables.g_hudTransitionStepValue * _gameEngine.StaticVariables.g_hudX / 0xf + cameraX);
        cameraY = (short)(_gameEngine.StaticVariables.g_hudTransitionStepValue * _gameEngine.StaticVariables.g_hudY / 0xf + cameraY);

        if ((_gameEngine.StaticVariables.g_hudTransitionState & 1) == 0)
        {
            uVar4 = '\0';
            offsetX = 0;
            offsetY = 0;

            if ((_gameEngine.StaticVariables.g_hudTransitionState & 2) != 0)
            {
                iVar5 = _gameEngine.StaticVariables.g_hudTransitionHalfWidth * _gameEngine.StaticVariables.g_hudTransitionStepValue;
                divisionHalfWidth = (int)((ulong)((long)iVar5 * -0x77777777) >> 0x20);
                scaledHalfHeight = _gameEngine.StaticVariables.g_hudTransitionHalfHeight * _gameEngine.StaticVariables.g_hudTransitionStepValue;
                divisionHalfHeight = (int)((ulong)((long)scaledHalfHeight * -0x77777777) >> 0x20);
                alphaValue = (char)((0xf - _gameEngine.StaticVariables.g_hudTransitionStepValue) * 0x80 / 0xf);

                //goto ComputeOffsets;
                uVar4 = (char)(alphaValue + '\x7f');
                offsetX = (short)((short)(divisionHalfWidth + iVar5 >> 3) - (short)(iVar5 >> 0x1f));
                offsetY = (short)((short)(divisionHalfHeight + scaledHalfHeight >> 3) - (short)(scaledHalfHeight >> 0x1f));

            }
        }
        else
        {
            iVar5 = _gameEngine.StaticVariables.g_hudTransitionHalfWidth * (0xf - _gameEngine.StaticVariables.g_hudTransitionStepValue);
            divisionHalfWidth = (int)((ulong)((long)iVar5 * -0x77777777) >> 0x20);
            scaledHalfHeight = _gameEngine.StaticVariables.g_hudTransitionHalfHeight * (0xf - _gameEngine.StaticVariables.g_hudTransitionStepValue);
            divisionHalfHeight = (int)((ulong)((long)scaledHalfHeight * -0x77777777) >> 0x20);
            alphaValue = (char)(_gameEngine.StaticVariables.g_hudTransitionStepValue * 0x80 / 0xf);
            ComputeOffsets:
            uVar4 = (char)(alphaValue + '\x7f');
            offsetX = (short)((short)(divisionHalfWidth + iVar5 >> 3) - (short)(iVar5 >> 0x1f));
            offsetY = (short)((short)(divisionHalfHeight + scaledHalfHeight >> 3) - (short)(scaledHalfHeight >> 0x1f));
        }

        _gameEngine.StaticVariables.g_hudTransitionStepValue += -1;

        FinalizeTransitionUpdate:

        var polyFt4 = _gameEngine.StaticVariables.g_spriteCharacterPortrait[0];
        polyFt4.r0 = (byte)uVar4;
        polyFt4.g0 = (byte)uVar4;
        polyFt4.b0 = (byte)uVar4;
        polyFt4.x0 = cameraX;
        polyFt4.y0 = cameraY;
        polyFt4.x1 = (short)(cameraX + offsetX);
        polyFt4.y1 = cameraY;
        polyFt4.x2 = cameraX;
        polyFt4.y2 = (short)(cameraY + offsetY);
        polyFt4.x3 = (short)(cameraX + offsetX);
        polyFt4.y3 = (short)(cameraY + offsetY);
    }

    //80057b84
    public void UpdateHudTransitionState()
    {
        if (_gameEngine.StaticVariables.g_hudTransitionState != 0)
        {
            _gameEngine.StaticVariables.g_hudCurrentX = _gameEngine.StaticVariables.g_hudTransitionSrcX + 2 - _gameEngine.StaticVariables.g_hudTransitionDstXPtr;
            _gameEngine.StaticVariables.g_hudDeltaX = _gameEngine.StaticVariables.g_hudTransitionStartX;
            _gameEngine.StaticVariables.g_hudX = _gameEngine.StaticVariables.g_hudTransitionStartX - _gameEngine.StaticVariables.g_hudCurrentX;
            _gameEngine.StaticVariables.g_hudDeltaY = _gameEngine.StaticVariables.g_hudTransitionStartY;
            _gameEngine.StaticVariables.g_hudTransitionState = 2;
            _gameEngine.StaticVariables.g_hudCurrentY =
                _gameEngine.StaticVariables.g_hudTransitionSrcY + 2 - _gameEngine.StaticVariables.g_hudTransitionDstYPtr -
                (_gameEngine.StaticVariables.g_hudTransitionSrcZ + 2) + -0x20;
            _gameEngine.StaticVariables.g_hudY = _gameEngine.StaticVariables.g_hudTransitionStartY - _gameEngine.StaticVariables.g_hudCurrentY;
            _gameEngine.StaticVariables.g_hudTransitionStepValue = 0xf;
        }
    }

    //80058134
    public void DisplayInventoryCharacterPortrait()
    {
        //POLY_FT4* pPVar1;
        //DISPENV DStack_28;

        //GetDispEnv(&DStack_28);
        //SetDrawArea((DR_AREA*)(UINT_ARRAY_80180108 + g_drawModes[0x14].tag * 3), &DStack_28.disp);
        if (_gameEngine.StaticVariables.g_hudTransitionState != 0)
        {
            _gameEngine.MainInventoryManager.UpdateHudTransitionVariables();
            //var image = _gameEngine.GraphicManager.GetAnimationImageByIndex(0); // alundra portrait
            //_gameEngine.StaticVariables.g_spriteCharacterPortraitImage = image;
            var image = _gameEngine.StaticVariables.g_spriteCharacterPortraitImage;
            _gameEngine.GraphicManager.DrawPolyFt4(_gameEngine.StaticVariables.g_spriteCharacterPortrait[0], image);
            //pPVar1 = _gameEngine.StaticVariables.g_spriteCharacterPortrait + g_drawModes[0x14].tag;
            /* Probable PsyQ macro: addPrim(). */
            //pPVar1->tag = pPVar1->tag & 0xff000000 | *param_1 & 0xffffff;
            //*param_1 = *param_1 & 0xff000000 | (uint)pPVar1 & 0xffffff;
        }
    }

    //80055570
    public int DisplayInventory()
    {
        if (_gameEngine.StaticVariables.g_forbiddenWarpFlag == 0)
        {
            var isSpecialWarpTriggered = _gameEngine.CheckSpecialWarpCondition(0);

            if (isSpecialWarpTriggered != 0)
            {
                return 1;
            }

            isSpecialWarpTriggered = _gameEngine.CheckSpecialWarpCondition(0xb);
            if (isSpecialWarpTriggered != 0)
            {
                return 1;
            }

            if (_gameEngine.StaticVariables.g_cdIsReady == 0)
            {
                if ((_gameEngine.StaticVariables.g_padState1.ButtonsHold & PadState.Right) != 0)
                {
                    _gameEngine.TriggerWarpTypeA();
                    return 1;
                }
                if ((_gameEngine.StaticVariables.g_padState1.ButtonsHold & PadState.Left) != 0)
                {
                    //_gameEngine.TriggerWarpDebugZone(); //can't work
                    return 0;
                }
                if ((_gameEngine.StaticVariables.g_padState1.ButtonsHold & PadState.Up) != 0)
                {
                    _gameEngine.GraphicManager.StartFadeOut();
                    return 1;
                }
                if ((_gameEngine.StaticVariables.g_padState1.ButtonsHold & PadState.Down) != 0)
                {
                    _gameEngine.ActivateDebugSoundMenu();
                    return 1;
                }
            }

            _gameEngine.HudManager.InitializeHudPosition();
            _gameEngine.GraphicManager.SetTransitionType(6);
            var image = _gameEngine.GraphicManager.GetAnimationImageByIndex(0); //portrait alundra
            var bitmap = _gameEngine.AlundraMap.GenerateSpriteBitmap(image,
                _gameEngine.AlundraMap.SpriteInfo.Palettes[image.Palette]);

            InitializeHudTransitionVariablesAndSetStart(
                _gameEngine.StaticVariables.PlayerEntity.PosX, _gameEngine.StaticVariables.PlayerEntity.PosY, _gameEngine.StaticVariables.PlayerEntity.PosZ,
                _gameEngine.StaticVariables.g_cameraScrollingX, _gameEngine.StaticVariables.g_cameraScrollingY,
                (sbyte)image.Sx, (sbyte)image.Sy, bitmap);
            DisplayIconNames();
            _gameEngine.SoundManager.PlaySoundEffect(4);
        }

        return 1;
    }


    //80054f1c Inventory display
    public void FUN_80054f1c(CallBackInfo callBackInfo)
    {
        _gameEngine.StaticVariables.g_forbiddenWarpFlag = 5;
        _gameEngine.StaticVariables.g_inventoryCursorText = 0;
        _gameEngine.StaticVariables.g_playerControlFlags |= 8;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].speed = 0xf;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].x = (short)~(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Width << 3);

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].y =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].y = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].startX =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.X + _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].startX = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.X;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].startY =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].startY = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].originX = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.X;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].originY = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y;

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].speed = 0xf;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].x = (short)~(_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Width << 3);

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].y =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].y = _gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Y;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].startX =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.X + _gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].startX = _gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.X;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].startY =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].startY = _gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].originX = _gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.X;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].originY = _gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Y;

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].speed = 0xf;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].x = 0x140;

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].y =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].y = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Y;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].startX =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.X + _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].startX = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.X;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].startY =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].startY = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].originX = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.X;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].originY = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Y;

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].speed = 0xf;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].x = 0x140;

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].y =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].y = _gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Y;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].startX =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.X + _gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].startX = _gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.X;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].startY =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].startY = _gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].originX = _gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.X;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].originY = _gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Y;

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].speed = 0xf;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].x = 0x140;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].y =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].y = _gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Y;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].startX =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.X + _gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].startX = _gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.X;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].startY =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].startY = _gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].originX = _gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.X;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].originY = _gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Y;

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].speed = 0xf;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].x = 0x140;

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].y =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].y = _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].startX =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X + _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].startX = _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].startY =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].startY = _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].originX = _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].originY = _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y;

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].speed = 0xf;

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].x =
                 (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X + _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].x = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].y = 0xf0;

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].startX =
                 (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X + _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].startX = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        }

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].startY =
                 (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y + _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].startY = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].originX = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].originY = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y;

        callBackInfo.RenderFunc = FUN_80056598;
    }

    //80056598
    private void FUN_80056598(CallBackInfo callbackInfo)
    {
        int slot;

        if ((_gameEngine.StaticVariables.g_forbiddenWarpFlag & 6U) == 0)
        {
            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Down) != 0)
            {
                slot = _gameEngine.StaticVariables.g_inventorySelectedSlotId + 6;
                var iVar2 = _gameEngine.StaticVariables.g_inventorySelectedSlotId - 0x12;
                _gameEngine.StaticVariables.g_inventorySelectedSlotId = slot;
                if (0x17 < slot)
                {
                    _gameEngine.StaticVariables.g_inventorySelectedSlotId = iVar2;
                }
                _gameEngine.SoundManager.PlaySoundEffect(1);
                _gameEngine.StaticVariables.g_inventoryCursorText = 0;
            }

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Up) != 0)
            {
                slot = _gameEngine.StaticVariables.g_inventorySelectedSlotId - 6;
                if (_gameEngine.StaticVariables.g_inventorySelectedSlotId - 6 < 0)
                {
                    slot = _gameEngine.StaticVariables.g_inventorySelectedSlotId + 0x12;
                }
                _gameEngine.StaticVariables.g_inventorySelectedSlotId = slot;
                _gameEngine.SoundManager.PlaySoundEffect(1);
                _gameEngine.StaticVariables.g_inventoryCursorText = 0;
            }

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Right) != 0)
            {
                slot = _gameEngine.StaticVariables.g_inventorySelectedSlotId + 1;
                if (slot == slot / 6 * 6)
                {
                    slot = _gameEngine.StaticVariables.g_inventorySelectedSlotId - 5;
                }
                _gameEngine.StaticVariables.g_inventorySelectedSlotId = slot;
                _gameEngine.SoundManager.PlaySoundEffect(1);
                _gameEngine.StaticVariables.g_inventoryCursorText = 0;
            }

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Left) != 0)
            {
                slot = _gameEngine.StaticVariables.g_inventorySelectedSlotId - 1;
                if (_gameEngine.StaticVariables.g_inventorySelectedSlotId == _gameEngine.StaticVariables.g_inventorySelectedSlotId / 6 * 6)
                {
                    slot = _gameEngine.StaticVariables.g_inventorySelectedSlotId + 5;
                }
                _gameEngine.StaticVariables.g_inventorySelectedSlotId = slot;
                _gameEngine.SoundManager.PlaySoundEffect(1);
                _gameEngine.StaticVariables.g_inventoryCursorText = 0;
            }

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Cross) != 0)
            {
                if (_gameEngine.StaticVariables.g_inventorySelectedSlotId < 6)
                {
                    FUN_8005795c();
                }
                else
                {
                    FUN_80057854();
                }
            }

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.OpenInventory) != 0)
            {
                FUN_800556dc();
                _gameEngine.MainInventoryManager.UpdateHudTransitionState();
                _gameEngine.HudManager.InitializeHudPositionBeforeHide();
            }

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.OpenSubInventory) != 0)
            {
                FUN_800556dc();
                _gameEngine.StaticVariables.g_playerControlFlags |= 8;
                _gameEngine.MainInventoryManager.UpdateHudTransitionState();
                _gameEngine.StaticVariables.g_postProcessState = 1;
            }
        }
        else
        {
            _gameEngine.UIManager.UpdateUiBoxesPosition(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground, _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0]);
            _gameEngine.UIManager.UpdateUiBoxesPosition(_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground, _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1]);
            _gameEngine.UIManager.UpdateUiBoxesPosition(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground, _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2]);
            _gameEngine.UIManager.UpdateUiBoxesPosition(_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground, _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3]);
            _gameEngine.UIManager.UpdateUiBoxesPosition(_gameEngine.StaticVariables.UIBoxConfiguration_800b9a10, _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4]);
            _gameEngine.UIManager.UpdateUiBoxesPosition(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground, _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6]);
            slot = _gameEngine.UIManager.UpdateUiBoxesPosition(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons, _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5]);

            if (slot == 1)
            {
                if ((_gameEngine.StaticVariables.g_forbiddenWarpFlag & 4U) != 0)
                {
                    _gameEngine.StaticVariables.g_forbiddenWarpFlag &= 0xfffffffb;
                }

                if ((_gameEngine.StaticVariables.g_forbiddenWarpFlag & 2U) != 0)
                {
                    _gameEngine.StaticVariables.g_forbiddenWarpFlag = 0;
                    _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.X = _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].originX;
                    _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y = _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].originY;
                    _gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.X = _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].originX;
                    _gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Y = _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].originY;
                    _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.X = _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].originX;
                    _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Y = _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].originY;
                    _gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.X = _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].originX;
                    _gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Y = _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].originY;
                    _gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.X = _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].originX;
                    _gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Y = _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].originY;
                    _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X = _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].originX;
                    _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y = _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].originY;
                    _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X = _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].originX;
                    _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y = _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].originY;

                    if ((_gameEngine.StaticVariables.g_postProcessState & 1U) == 0)
                    {
                        _gameEngine.StaticVariables.g_playerControlFlags &= 0xfffffff7;
                    }

                    _gameEngine.UIManager.FUN_80047cb0(callbackInfo);
                    return;
                }
            }
        }

        UpdateCursorSpritePosition(_gameEngine.StaticVariables.g_inventoryCursorAnimation,
            (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.X + _gameEngine.StaticVariables.g_uiBoxesInventoryAnimationOffsetX[_gameEngine.StaticVariables.g_inventorySelectedSlotId] + 0x12),
            (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y + _gameEngine.StaticVariables.g_uiBoxesInventoryAnimationOffsetY[_gameEngine.StaticVariables.g_inventorySelectedSlotId] - 8),
            0);
        DisplayInventoryCursor(_gameEngine.StaticVariables.g_inventoryCursorAnimation);
        DisplayAmountOfMoneyFalconKeys();
        DisplayWeaponAndItemIcons();
        DisplayUiBoxes(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground); //weapon background
        DisplayUiBoxes(_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground); //item background
        DisplayUiBoxes(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground); //weapon name background
        DisplayUiBoxes(_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground); //item name background
        DisplayUiBoxes(_gameEngine.StaticVariables.UIBoxConfiguration_800b9a10);
        DisplayUiBoxes(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons); // money falcon key icon
        DisplayUiBoxes(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground); // display description background
        FUN_800562dc();
        DisplayInventoryTexts();
    }

    //80055fe8
    private void DisplayInventoryTexts()
    {
        int uVar1;
        int iVar2;

        var itemId = _gameEngine.StaticVariables.g_ItemIdBySlotIndex[_gameEngine.StaticVariables.g_inventorySelectedSlotId];

        if (itemId == 0)
        {
            return;
        }

        if (itemId == -1)
        {
            var slotIndex = _gameEngine.StaticVariables.g_inventorySelectedSlotId;
            var value = (uint)GetItemIdFromSlotIndex(slotIndex);

            if (slotIndex < 6)
            {
                value = _gameEngine.PlayerManager.GetWeaponIdBySlotId((int)value);
            }
            else
            {
                value = _gameEngine.PlayerManager.GetItemIdFromSlotId(value);
            }

            if (value == 0xffffffff)
            {
                return;
            }

            itemId = (int)value;
        }
        else
        {
            iVar2 = _gameEngine.PlayerManager.GetNumberOfItem(itemId);

            if (iVar2 == 0)
            {
                return;
            }
        }

        //
        //InventoryWeaponDescriptionLinesSprites[1].Clear();

        if (_gameEngine.StaticVariables.g_inventoryCursorText == 0)
        {
            var text = _gameEngine.EtcRes.GetItemName(itemId);//_gameEngine.StaticVariables.g_itemDropProperties[itemId * 2];
            //text = text.PadRight(0x20);
            text = text.Substring(0, _gameEngine.StaticVariables.g_inventoryCursorText);
            iVar2 = 0x20;

            //LAB_8005616c:
            InventoryWeaponDescriptionLinesSprites[0].Clear();
            //_gameEngine.UIManager.DisplayIconName(
            //    _gameEngine.StaticVariables.g_spriteInventoryText,
            //    InventoryWeaponDescriptionLinesSprites[0],
            //    text.ToCharArray(),
            //    iVar2,
            //    0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X,
            //    0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y, 
            //    2);
            _gameEngine.StaticVariables.INT_8017fef0 = 0;
            _gameEngine.StaticVariables.g_spriteInventoryText[0].w = 0;
            _gameEngine.StaticVariables.g_spriteInventoryText[1].w = 0;
            //InventoryWeaponDescriptionLinesSprites[0].Clear();
            _gameEngine.StaticVariables.g_inventoryCursorText += 1;
        }
        else
        {
            if (_gameEngine.StaticVariables.g_inventoryCursorText - 1U < 0x10)
            {
                var text = _gameEngine.EtcRes.GetItemName(itemId);//_gameEngine.StaticVariables.g_itemDropProperties[itemId * 2];
                //text = text.PadRight(0x11);
                var initialLength = text.Length;
                var length = Math.Min(text.Length, _gameEngine.StaticVariables.g_inventoryCursorText);
                text = text.Substring(0, length);
                //text.Length == _gameEngine.StaticVariables.INT_8017feec - 1

                if (length > initialLength)//text[length] == '\0')
                {
                    _gameEngine.StaticVariables.g_inventoryCursorText = 0x11;
                    uVar1 = 0;
                }
                else
                {
                    //var text = _gameEngine.EtcRes.GetOtherString((int)itemId); //_gameEngine.StaticVariables.g_itemDropProperties[itemId * 2];
                    FUN_80055f48(0, 0, ' ');
                        //_gameEngine.StaticVariables.g_inventoryCursorText,
                        //text[_gameEngine.StaticVariables.g_inventoryCursorText - 1]);

                    _gameEngine.UIManager.DisplayIconName(
                        _gameEngine.StaticVariables.g_spriteInventoryText,
                        InventoryWeaponDescriptionLinesSprites[0],
                        text.ToCharArray(),
                        0,
                        0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X,
                        0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y, 
                        2);

                    uVar1 = 0;
                }
            }
            else if (_gameEngine.StaticVariables.g_inventoryCursorText - 0x11U < 0x3c)
            {
                uVar1 = 0;
                _gameEngine.StaticVariables.g_inventoryCursorText += 1;
            }
            else
            {
                if (_gameEngine.StaticVariables.g_inventoryCursorText == 0x4d)
                {
                    var text = _gameEngine.EtcRes.GetItemDescription(itemId);//_gameEngine.StaticVariables.g_itemDropProperties[itemId * 2];
                    //text = _gameEngine.StaticVariables.g_tileSetEtcBase[itemId * 2];
                    //text = text.PadRight(0x40);var length = Math.Min(text.Length, _gameEngine.StaticVariables.g_inventoryCursorText);
                    var length = Math.Min(text.Length, _gameEngine.StaticVariables.g_inventoryCursorText);
                    text = text.Substring(0, length);
                    iVar2 = 0x40;
                    //goto LAB_8005616c;

                    InventoryWeaponDescriptionLinesSprites[0].Clear();
                    //_gameEngine.UIManager.DisplayIconName(
                    //    _gameEngine.StaticVariables.g_spriteInventoryText,
                    //    InventoryWeaponDescriptionLinesSprites[0],
                    //    text.ToCharArray(),
                    //    iVar2,
                    //    0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X,
                    //    0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y,
                    //    2);
                    _gameEngine.StaticVariables.INT_8017fef0 = 0;
                    _gameEngine.StaticVariables.g_spriteInventoryText[0].w = 0;
                    _gameEngine.StaticVariables.g_spriteInventoryText[1].w = 0;
                    _gameEngine.StaticVariables.g_inventoryCursorText += 1;
                    return;
                }

                if (_gameEngine.StaticVariables.g_inventoryCursorText - 0x4eU < 0x40)
                {
                    uVar1 = 0;
                    var text = _gameEngine.EtcRes.GetItemDescription(itemId);//_gameEngine.StaticVariables.g_tileSetEtcBase[itemId * 2];
                    var initialLength = text.Length;
                    var length = Math.Min(text.Length, _gameEngine.StaticVariables.g_inventoryCursorText - 0x04d);
                    text = text.Substring(0, length);

                    if (_gameEngine.StaticVariables.g_inventoryCursorText - 0x4e > initialLength)
                    {
                        _gameEngine.StaticVariables.g_inventoryCursorText = 0x8e;
                    }
                    else
                    {
                        FUN_80055f48(0, 0, ' ');
                            //_gameEngine.StaticVariables.g_inventoryCursorText - 0x4d,
                            //text[_gameEngine.StaticVariables.g_inventoryCursorText - 0x4e]);

                        _gameEngine.UIManager.DisplayIconName(
                            _gameEngine.StaticVariables.g_spriteInventoryText,
                            InventoryWeaponDescriptionLinesSprites[0],
                            text.ToCharArray(),
                            0,
                            0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X,
                            0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y, 
                            2);

                        uVar1 = 0;
                    }
                }
                else
                {
                    if (_gameEngine.StaticVariables.g_inventoryCursorText == 0x8e)
                    {
                        SPRT[] sprites =
                        [
                            _gameEngine.StaticVariables.g_spriteInventoryText[2],
                            _gameEngine.StaticVariables.g_spriteInventoryText[3]
                        ];

                        InventoryWeaponDescriptionLinesSprites[1].Clear();

                        //var text3 = _gameEngine.EtcRes.GetItemDescription((int)itemId);//_gameEngine.StaticVariables.g_paletteSetEtcBase
                        ////text3 = text3.PadRight(0x40);
                        ////var initialLength = text3.Length;
                        //var length = Math.Min(text3.Length - 1, _gameEngine.StaticVariables.g_inventoryCursorText);
                        //text3 = text3.Substring(0, length);
                        //
                        //_gameEngine.UIManager.DisplayIconName(sprites,
                        //    InventoryWeaponDescriptionLinesSprites[1],
                        //    text3.ToCharArray(),
                        //    0x40,
                        //    0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X,
                        //    0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y,
                        //    3);

                        _gameEngine.StaticVariables.INT_8017fef0 = 0;
                        _gameEngine.StaticVariables.g_inventoryCursorText += 1;

                        DisplayInventoryDescription(0);

                        _gameEngine.StaticVariables.g_spriteInventoryText[2].w = 0;
                        _gameEngine.StaticVariables.g_spriteInventoryText[3].w = 0;
                        return;
                    }

                    if (_gameEngine.StaticVariables.g_inventoryCursorText - 0x8fU < 0x40)
                    {
                        var text = _gameEngine.EtcRes.GetItemDescription(itemId);
                        var initialLength = text.Length;
                        var length = Math.Min(text.Length, _gameEngine.StaticVariables.g_inventoryCursorText);
                        text = text.Substring(0, length);

                        if (_gameEngine.StaticVariables.g_inventoryCursorText - 0x8f < 0x40)
                        {
                            _gameEngine.StaticVariables.g_inventoryCursorText = 0xcf;
                        }
                        else
                        {
                            FUN_80055f48(1, 0, ' ');
                                //_gameEngine.StaticVariables.g_inventoryCursorText - 0x90,
                                //text[_gameEngine.StaticVariables.g_inventoryCursorText - 0x8f]);

                            _gameEngine.UIManager.DisplayIconName(
                                _gameEngine.StaticVariables.g_spriteInventoryText,
                                InventoryWeaponDescriptionLinesSprites[1],
                                text.ToCharArray(),
                                0,
                                0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X,
                                0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y, 
                                2);
                        }
                    }
                    else if (_gameEngine.StaticVariables.g_inventoryCursorText != 0xcf)
                    {
                        return;
                    }

                    DisplayInventoryDescription(0);
                    uVar1 = 1;
                }
            }

            DisplayInventoryDescription(uVar1);
        }
    }

    //80055e30
    private void DisplayInventoryDescription(int lineIndex)
    {
        var sprite = _gameEngine.StaticVariables.g_spriteInventoryText[lineIndex];
        var inventoryDescriptionBackground = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground;
        sprite.x0 = (short)(inventoryDescriptionBackground.X + 0x10);
        sprite.y0 = (short)(inventoryDescriptionBackground.Y + 0xc + lineIndex * 0x10);

        //Display inventory item description
        //TODO save the text and display it
        //var sprite = _gameEngine.StaticVariables.g_spriteInventoryText[index];
        //var bitmap = _gameEngine.Font3.GenerateFontBitmapFromSprite(sprite);
        //_gameEngine.Renderer.AddSprite(sprite, SpriteDepth., bitmap);

        //uVar2 = _gameEngine.StaticVariables.g_drawModes[0x14].tag;
        //puVar9 = _gameEngine.StaticVariables.DAT_80146f5c + _gameEngine.StaticVariables.g_drawModes[0x14].tag * 0x28);

        //uVar5 = (uint)pSVar7 & 0xffffff;
        //pSVar7 = pSVar7 + 1;
        //puVar3 = _gameEngine.StaticVariables.g_spriteInventoryText[uVar2 + index * 2].tag + iVar4);

        /* Probable PsyQ macro: addPrim(). */
        //*puVar3 = *puVar3 & 0xff000000 | *puVar9 & 0xffffff;
        //*puVar9 = *puVar9 & 0xff000000 | uVar5;

        //sprite = _gameEngine.StaticVariables.g_spriteInventoryText[index * 2];
        //bitmap = _gameEngine.Font3.GenerateFontBitmapFromSprite(sprite);
        //_gameEngine.Renderer.AddSprite(sprite, SpriteDepth., bitmap);

        //_gameEngine.UIManager.DisplayTexts(0, 0); //sprite.x0, sprite.y0);

        foreach (var spr in InventoryWeaponDescriptionLinesSprites[lineIndex])
        {
            _gameEngine.Renderer.AddSprite(spr.X + sprite.x0, spr.Y + sprite.y0,
                spr.Width, spr.Height,
                SpriteDepth.ForegroundUI, spr.Bitmap, spr.Alpha);
        }
    }

    //80055f48
    private void FUN_80055f48(int index, int offset, char c)
    {
        int iVar1;
        int i;

        if (_gameEngine.StaticVariables.INT_8017fef0 == 0)
        {
            i = 0;
            _gameEngine.StaticVariables.INT_8017fef0 = 2;
            _gameEngine.StaticVariables.g_inventoryCursorText += 1;
            iVar1 = index * 0x28;

            do
            {
                _gameEngine.StaticVariables.g_spriteInventoryText[i].w = (short)(_gameEngine.StaticVariables.g_spriteInventoryText[i].w + _gameEngine.StaticVariables.g_fontCharWidthTable[(c & 0xff) * 5]);
                _gameEngine.StaticVariables.g_spriteInventoryText[i + 1].w = (short)(_gameEngine.StaticVariables.g_spriteInventoryText[i + 1].w + _gameEngine.StaticVariables.g_fontCharWidthTable[(c & 0xff) * 5]);
                i += 1;

            } while (i < 1);
        }
        else
        {
            _gameEngine.StaticVariables.INT_8017fef0 -= 1;
        }
    }

    //800562dc
    private void FUN_800562dc()
    {
        ulong uVar2;
        int x;
        SPRT sprite;
        //DISPENV dispEnv;

        //GetDispEnv(&dispEnv);
        x = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.X;

        if (0x13f < x)
        {
            x = 0x13f;
        }

        var w = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Width * 8;

        if (0x13f < x + w)
        {
            w = 0x140 - x;
        }

        //dispEnv.disp.w = (short)w;
        //dispEnv.disp.y = dispEnv.disp.y + g_UiBoxesInventoryWeaponNameBackground.y;
        //dispEnv.disp.x = dispEnv.disp.x + (short)iVar9;
        //dispEnv.disp.h =(g_UiBoxesInventoryItemNameBackground.y - g_UiBoxesInventoryWeaponNameBackground.y) + g_UiBoxesInventoryItemNameBackground.height * 8;
        //SetDrawArea((DR_AREA*)(&UNK_8017fa84 + g_drawModes[0x14].tag * 0xc), &dispEnv.disp);
        //SetDrawArea((DR_AREA*)(&DAT_8017fa9c + g_drawModes[0x14].tag * 0xc), &dispEnv.disp);
        sprite = _gameEngine.StaticVariables.g_ItemNameSprites[2];
        sprite.x0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.X + 0x10);
        sprite.y0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Y + 8);

        foreach (var spr in InventoryItemNameSprites)
        {
            _gameEngine.Renderer.AddSprite(spr.X + sprite.x0, spr.Y + sprite.y0,
                spr.Width, spr.Height,
                SpriteDepth.ForegroundUI, spr.Bitmap, spr.Alpha);
        }

        sprite = _gameEngine.StaticVariables.g_ItemNameSprites[0];
        sprite.x0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.X + 0x10);
        sprite.y0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Y + 8);

        foreach (var spr in InventoryWeaponNameSprites)
        {
            _gameEngine.Renderer.AddSprite(spr.X + sprite.x0, spr.Y + sprite.y0,
                spr.Width, spr.Height,
                SpriteDepth.ForegroundUI, spr.Bitmap, spr.Alpha);
        }

        //iVar10 = -0x7fe805a4; _gameEngine.StaticVariables.g_ItemNameSprites[2]
        //puVar8 = _gameEngine.StaticVariables.DAT_80146f60 + _gameEngine.StaticVariables.g_drawModes[0x14].tag * 0x28);

        //puVar7 = _gameEngine.StaticVariables.g_ItemNameSprites[0]. + i;
        /* Probable PsyQ macro: addPrim(). */
        //pSVar4 = pSVar12 + uVar3;
        //*puVar7 = *puVar7 & 0xff000000 | *puVar8 & 0xffffff;

        //puVar7 = _gameEngine.StaticVariables.g_ItemNameSprites[2]. + i;
        //*puVar8 = *puVar8 & 0xff000000 | (uint)pSVar4 & 0xffffff;
        //*puVar7 = *puVar7 & 0xff000000 | (uint)pSVar4 & 0xffffff;
        //*puVar8 = *puVar8 & 0xff000000 | uVar6 & 0xffffff;

        //puVar7 = _gameEngine.StaticVariables.DAT_8017fa9c + g_drawModes[0x14].tag * 0xc);
        //puVar8 = _gameEngine.StaticVariables.DAT_80146f60 + g_drawModes[0x14].tag * 0x28);
        /* Probable PsyQ macro: addPrim(). */
        //*puVar7 = *puVar7 & 0xff000000 | *puVar8 & 0xffffff;
        //*puVar8 = *puVar8 & 0xff000000 | (uint)puVar7 & 0xffffff;
    }

    //80055d78
    //background
    private void DisplayUiBoxes(UIBoxConfiguration textTileConfig)
    {
        ulong uVar1;
        SPRT sprite;
        uint puVar3;
        int w;
        int h = 0;

        //uVar1 = _gameEngine.StaticVariables.g_drawModes[0x14].tag;

        if (0 < textTileConfig.Height)
        {
            do
            {
                w = 0;

                if (0 < textTileConfig.Width)
                {
                    //puVar3 = _gameEngine.StaticVariables.g_drawModes + uVar1 * 0x28 + 0xf8;
                    do
                    {
                        /* Probable PsyQ macro: addPrim(). */
                        //pSVar2->tag = pSVar2->tag & 0xff000000 | *puVar3 & 0xffffff;
                        //*puVar3 = *puVar3 & 0xff000000 | (uint)pSVar2 & 0xffffff;
                        //pSVar2 = pSVar2 + 1;
                        sprite = textTileConfig.SpritesA[h * textTileConfig.Width + w];
                        var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                        _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.BackgroundUI, bitmap);

                        w += 1;
                    } while (w < textTileConfig.Width);
                }

                h += 1;

            } while (h < textTileConfig.Height);
        }
    }

    //80056fb4
    private void DisplayWeaponAndItemIcons()
    {
        uint uVar2;
        int index;
        int offset;
        int textureId;
        int i;
        int row;
        int col;

        var rectangleSprite = _gameEngine.StaticVariables.g_itemSelectedRectangle[0];
        //SetSprt(rectangleSprite + g_drawModes[0x14].tag * 2);
        rectangleSprite.r0 = 0x80;
        rectangleSprite.g0 = 0x80;
        rectangleSprite.b0 = 0x80;
        rectangleSprite.u0 = 48;
        rectangleSprite.v0 = 0x98;
        rectangleSprite.w = 0x18;
        rectangleSprite.h = 0x20;
        rectangleSprite.clut = 0; //_gameEngine.StaticVariables.g_clutTable[(ushort)g_numbersSpriteSheetUVs._2_2_];
        //SetSemiTrans(rectangleSprite + iVar2, 0);
        //SetShadeTex(rectangleSprite + g_draSetShadeTex(rectangleSprite + g_dra    SetShadeTex(rectangleSprite + g_dra

        row = 0;
        offset = 0;

        do
        {
            col = 0;

            do
            {
                var itemId = _gameEngine.StaticVariables.g_ItemIdBySlotIndex[offset];

                if (itemId != 0)
                {
                    if (row == 1 && col == 0)
                    {
                        //special case herbs : number of item is displayed
                        var numOfItem = _gameEngine.PlayerManager.GetNumberOfItem(itemId);

                        if (numOfItem != 0)
                        {
                            var sprite = _gameEngine.StaticVariables.g_spriteInventoryItems[6];

                            _gameEngine.GraphicManager.InitializeSpriteWithImage(sprite, itemId,
                                       (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.X + _gameEngine.StaticVariables.g_uiBoxesInventoryAnimationOffsetX[6]),
                                       (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y + _gameEngine.StaticVariables.g_uiBoxesInventoryAnimationOffsetY[6]));

                            i = (int)_gameEngine.PlayerManager.SetItemIdFromCurrentItemId();

                            if (i == 0x24) //herbs
                            {
                                rectangleSprite.x0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.X + _gameEngine.StaticVariables.g_uiBoxesInventoryAnimationOffsetX[6]);
                                rectangleSprite.y0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y + _gameEngine.StaticVariables.g_uiBoxesInventoryAnimationOffsetY[6]);
                                //puVar3 = (uint*)((int)g_drawModes + i + 0xf8);
                                /* Probable PsyQ macro: addPrim(). */
                                //*puVar5 = *puVar5 & 0xff000000 | *puVar3 & 0xffffff;
                                //*puVar3 = *puVar3 & 0xff000000 | (uint)puVar5 & 0xffffff;

                                //index = _gameEngine.GraphicManager.GetItemTextureIdByItemId((int)itemId);
                                //var image = _gameEngine.GraphicManager.GetAnimationImageByIndex(index);
                                //var bitmap = _gameEngine.AlundraMap.GetSpriteBitmap(image);
                                var bitmap3 = _gameEngine.Font3.GenerateHudBitmapFromSprite(rectangleSprite);
                                _gameEngine.Renderer.AddSprite(rectangleSprite, SpriteDepth.ForegroundUI, bitmap3);
                            }

                            sprite.r0 = 0x90;
                            sprite.g0 = 0x90;
                            sprite.b0 = 0x90;

                            index = _gameEngine.GraphicManager.GetItemTextureIdByItemId(itemId);
                            var image = _gameEngine.GraphicManager.GetAnimationImageByIndex(index);
                            var bitmap = _gameEngine.AlundraMap.GetSpriteBitmap(image);
                            _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

                            var numberSprite = _gameEngine.StaticVariables.g_mainInventoryNumberOfHerbsSprites[0];
                            //SetSprt(sprite);
                            numberSprite.x0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.X + _gameEngine.StaticVariables.g_uiBoxesInventoryAnimationOffsetX[6] + 0x10);
                            numberSprite.y0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y + _gameEngine.StaticVariables.g_uiBoxesInventoryAnimationOffsetY[6] + 0x10);
                            
                            numberSprite.r0 = 0x90;
                            numberSprite.g0 = 0x90;
                            numberSprite.b0 = 0x90;
                            i = (numOfItem % 10) * 0x14;
                            numberSprite.u0 = _gameEngine.StaticVariables.g_numbersSpriteSheetUVs[i];
                            numberSprite.v0 = _gameEngine.StaticVariables.g_numbersSpriteSheetUVs[i + 1];

                            var bitmap2 = _gameEngine.Font3.GenerateHudBitmapFromSprite(numberSprite);
                            _gameEngine.Renderer.AddSprite(numberSprite, SpriteDepth.ForegroundUI, bitmap2);

                            //uVar1 = g_drawModes[0x14].tag;
                            //pSVar4 = _gameEngine.StaticVariables.g_mainInventoryNumberOfHerbsSprites + g_drawModes[0x14].tag;
                            //puVar3 = _gameEngine.StaticVariables.DAT_80146f6c + g_drawModes[0x14].tag * 0x28);
                            //puVar5 = _gameEngine.StaticVariables.DAT_80146f68 + g_drawModes[0x14].tag * 0x28);
                            /* Probable PsyQ macro: addPrim(). */
                            //pSVar4->tag = pSVar4->tag & 0xff000000 | *puVar3 & 0xffffff;
                            //*puVar3 = *puVar3 & 0xff000000 | (uint)pSVar4 & 0xffffff;
                            /* Probable PsyQ macro: addPrim(). */
                            //*(uint*)(&g_spriteInventoryItems + uVar1 * 0x1e0) = *(uint*)(&g_spriteInventoryItems + uVar1 * 0x1e0) & 0xff000000 | *puVar5 & 0xffffff;
                            //*puVar5 = *puVar5 & 0xff000000 | (uint)(&g_spriteInventoryItems + uVar1 * 0x1e0) & 0xffffff;
                        }
                    }
                    else
                    {
                        textureId = itemId;

                        if (textureId == -1)
                        {
                            itemId = GetItemIdFromSlotIndex(offset);

                            var value = _gameEngine.PlayerManager.GetItemIdFromSlotId((uint)itemId);

                            if (value == 0xffffffff)
                            {
                                textureId = -1;
                            }
                            else
                            {
                                textureId = (int)value;
                            }
                        }
                        else if (_gameEngine.PlayerManager.GetNumberOfItem(textureId) == 0)
                        {
                            textureId = -1;
                        }

                        if (textureId != -1)
                        {
                            var sprite = _gameEngine.StaticVariables.g_spriteInventoryItems[offset];
                            
                            _gameEngine.GraphicManager.InitializeSpriteWithImage(sprite,
                                       textureId,
                                       (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.X + _gameEngine.StaticVariables.g_uiBoxesInventoryAnimationOffsetX[offset]),
                                       (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y + _gameEngine.StaticVariables.g_uiBoxesInventoryAnimationOffsetY[offset]));
                            
                            //display selection item
                            if (row == 0 && textureId == _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon())
                            {
                                rectangleSprite.x0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.X + _gameEngine.StaticVariables.g_uiBoxesInventoryAnimationOffsetX[col]);
                                rectangleSprite.y0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y + _gameEngine.StaticVariables.g_uiBoxesInventoryAnimationOffsetY[col]);
                                //puVar3 = (uint*)((int)g_drawModes + i + 0xf8);
                                /* Probable PsyQ macro: addPrim(). */
                                //*puVar5 = *puVar5 & 0xff000000 | *puVar3 & 0xffffff;
                                //*puVar3 = *puVar3 & 0xff000000 | (uint)puVar5 & 0xffffff;
                            
                                var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(rectangleSprite);
                                _gameEngine.Renderer.AddSprite(rectangleSprite, SpriteDepth.ForegroundUICursor, bitmap);
                            }
                            else if (0 < row && textureId == _gameEngine.PlayerManager.SetItemIdFromCurrentItemId())
                            {
                                rectangleSprite.x0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.X + _gameEngine.StaticVariables.g_uiBoxesInventoryAnimationOffsetX[offset]);
                                rectangleSprite.y0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y + _gameEngine.StaticVariables.g_uiBoxesInventoryAnimationOffsetY[offset]);
                                //puVar5 = _gameEngine.StaticVariables.g_drawModes + i + 0xf8);
                                /* Probable PsyQ macro: addPrim(). */
                                //*puVar3 = *puVar3 & 0xff000000 | *puVar5 & 0xffffff;
                                //*puVar5 = *puVar5 & 0xff000000 | (uint)puVar3 & 0xffffff;

                                var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(rectangleSprite);
                                _gameEngine.Renderer.AddSprite(rectangleSprite, SpriteDepth.ForegroundUICursor, bitmap);
                            }

                            sprite.r0 = 0x90;
                            sprite.g0 = 0x90;
                            sprite.b0 = 0x90;

                            index = _gameEngine.GraphicManager.GetItemTextureIdByItemId(textureId);
                            var image = _gameEngine.GraphicManager.GetAnimationImageByIndex(index);
                            var bitmap3 = _gameEngine.AlundraMap.GetSpriteBitmap(image);
                            _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap3);

                            //i = _gameEngine.StaticVariables.g_drawModes[0x14].tag * 0x1e0;
                            //puVar3 = _gameEngine.StaticVariables.DAT_80146f68 + g_drawModes[0x14].tag * 0x28);
                            /* Probable PsyQ macro: addPrim(). */
                            //*(uint*)(&DAT_8017fab4 + i + i) = *(uint*)(&DAT_8017fab4 + textTioDisplay + i) & 0xff000000 | *puVar3 & 0xffffff;
                            //*puVar3 = *puVar3 & 0xff000000 | (uint)(&DAT_8017fab4 + i + i) & 0xffffff;
                        }
                    }
                }

                offset += 1;
                col += 1;

            } while (col < 6);

            row += 1;

        } while (row < 4);
    }

    private static int GetItemIdFromSlotIndex(int slotIndex)
    {
        int itemId;
        itemId = slotIndex < 6 ? slotIndex + 1 : slotIndex;

        itemId = itemId switch
        {
            12 => 16,
            13 => 17,
            14 => 18,
            15 => 19,
            21 => 20,
            22 => 21,
            23 => 22,
            _ => itemId
        };
        return itemId;
    }

    //80056a98
    private void DisplayAmountOfMoneyFalconKeys()
    {
        int value;
        int iVar2;
        int iVar8;
        int i;
        short offsetX;
        int divisor;
        SPRT sprite;

        divisor = 1000;
        value = _gameEngine.PlayerManager.GetMoney();
        i = 0;
        offsetX = 0x18;

        do
        {
            if (divisor == 0 || (divisor == -1 && value == -0x80000000))
            {
                Breakpoint.TriggerBreak();
            }

            sprite = _gameEngine.StaticVariables.g_spriteInventoryMoneyAmount[i];

            iVar2 = value / divisor % 10 * 0x14;
            sprite.u0 = _gameEngine.StaticVariables.g_numbersSpriteSheetUVs[iVar2];
            sprite.v0 = _gameEngine.StaticVariables.g_numbersSpriteSheetUVs[iVar2 + 1];
            sprite.x0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X + offsetX);
            sprite.y0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y + 4);

            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

            //puVar5 = (_gameEngine.StaticVariables.sprite.tag + iVar8);;
            //puVar6 = _gameEngine.StaticVariables.DAT_80146f6c[i];
            /* Probable PsyQ macro: addPrim(). */
            //*puVar5 = uVar3 & 0xff000000 | *puVar6 & 0xffffff;
            //*puVar6 = *puVar6 & 0xff000000 | (uint)sprite & 0xffffff;
            divisor /= 10;
            offsetX = (short)(offsetX + 8);
            i += 1;
        } while (i < 4);

        divisor = 10;
        value = _gameEngine.PlayerManager.GetNumberOfItem(0x3d);
        i = 0;
        offsetX = 0x18;

        do
        {
            if (divisor == 0 || (divisor == -1 && value == -0x80000000))
            {
                Breakpoint.TriggerBreak();
            }

            sprite = _gameEngine.StaticVariables.g_spriteInventoryNumberOfKeys[i];

            iVar2 = value / divisor % 10 * 0x14;
            sprite.u0 = _gameEngine.StaticVariables.g_numbersSpriteSheetUVs[iVar2];
            sprite.v0 = _gameEngine.StaticVariables.g_numbersSpriteSheetUVs[iVar2 + 1];
            sprite.x0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X + offsetX + 0x10);
            sprite.y0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y + 0x34);

            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

            //puVar5 = (_gameEngine.StaticVariables.sprite.tag + iVar8);
            //uVar3 = *puVar5;
            //puVar6 = _gameEngine.StaticVariables.DAT_80146f6c[i];
            /* Probable PsyQ macro: addPrim(). */
            //*puVar5 = uVar3 & 0xff000000 | *puVar6 & 0xffffff;
            //*puVar6 = *puVar6 & 0xff000000 | (uint)sprite & 0xffffff;
            divisor /= 10;
            offsetX = (short)(offsetX + 8);
            i += 1;
        } while (i < 2);

        divisor = 10;
        value = _gameEngine.PlayerManager.GetNumberOfFalcon() + _gameEngine.PlayerManager.GetNumberOfFalconTemp();
        offsetX = 0x18;
        i = 0;

        do
        {
            if (divisor == 0 || (divisor == -1 && value == -0x80000000))
            {
                Breakpoint.TriggerBreak();
            }

            sprite = _gameEngine.StaticVariables.g_spriteInventoryNumberOfFalcon[i];

            iVar2 = value / divisor % 10 * 0x14;
            sprite.u0 = _gameEngine.StaticVariables.g_numbersSpriteSheetUVs[iVar2];
            sprite.v0 = _gameEngine.StaticVariables.g_numbersSpriteSheetUVs[iVar2 + 1];
            sprite.x0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X + offsetX + 0x10);
            sprite.y0 = (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y + 0x1c);

            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

            //puVar5 = (_gameEngine.StaticVariables.sprite.tag + iVar8);
            //uVar3 = *puVar5;
            //puVar6 = _gameEngine.StaticVariables.DAT_80146f6c[i];
            /* Probable PsyQ macro: addPrim(). */
            //*puVar5 = uVar3 & 0xff000000 | *puVar6 & 0xffffff;
            //*puVar6 = *puVar6 & 0xff000000 | (uint)sprite & 0xffffff;
            divisor /= 10;
            offsetX = (short)(offsetX + 8);
            i += 1;
        } while (i < 2);
    }

    //80050a74
    public void DisplayInventoryCursor(InventoryCursorAnimation cursorAnim)
    {
        cursorAnim.FrameDelay += 1;

        if (cursorAnim.FrameDelay == 0x28)
        {
            cursorAnim.FrameDelay = 0;
        }

        cursorAnim.Sprites[0].u0 = _gameEngine.StaticVariables.g_inventoryCursorTextureUVs[cursorAnim.FrameDelay / 10 * 0x28];
        cursorAnim.Sprites[0].v0 = _gameEngine.StaticVariables.g_inventoryCursorTextureUVs[cursorAnim.FrameDelay / 10 * 0x28 + 1];

        //cursorAnim.SpriteRecords[1].u0 = (byte)(_gameEngine.StaticVariables.g_inventoryCursorTextureU + cursorAnim.FrameDelay / 10 * 0x28);
        //cursorAnim.SpriteRecords[1].v0 = (byte)(_gameEngine.StaticVariables.g_inventoryCursorTextureV + cursorAnim.FrameDelay / 10 * 0x28);

        //puVar2 = _gameEngine.StaticVariables.DAT_80146f6c[g_drawModes[0x14].tag * 0x28];
        //pSVar3 = cursorAnim.SpriteRecords[_gameEngine.StaticVariables.g_drawModes[0x14].tag];
        /* Probable PsyQ macro: addPrim(). */
        //cursorAnim.sprites[g_drawModes[0x14].tag].tag = cursorAnim.sprites[g_drawModes[0x14].tag].tag & 0xff000000 | *puVar2 & 0xffffff;
        //*puVar2 = *puVar2 & 0xff000000 | (uint)pSVar3 & 0xffffff;

        var sprite = cursorAnim.Sprites[0];
        var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
        _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUICursor2, bitmap);
    }

    //8005795c
    private void FUN_8005795c()
    {
        uint iVar1;
        uint iVar2;
        ushort uVar3;

        switch (_gameEngine.StaticVariables.g_inventorySelectedSlotId)
        {
            case 0:
                iVar2 = _gameEngine.PlayerManager.GetWeaponIdFromSlot1();
                iVar1 = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();

                if (iVar1 == iVar2)
                {
                    DisplayIconNames();
                    return;
                }

                uVar3 = 1;
                goto joined_r0x800579e0;

            case 1:
                iVar1 = _gameEngine.PlayerManager.GetWeaponIdFromSlot3();
                iVar2 = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();

                if (iVar2 == iVar1)
                {
                    DisplayIconNames();
                    return;
                }

                if (iVar1 != -1)
                {
                    _gameEngine.SoundManager.PlaySoundEffect(2);
                    uVar3 = 3;
                    //LAB_80057b04:
                    _gameEngine.PlayerManager.SetPlayerWeaponId(uVar3);
                    DisplayIconNames();
                    return;
                }
                break;

            case 2:
                iVar2 = _gameEngine.PlayerManager.GetWeaponIdFromSlot2();
                iVar1 = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();

                if (iVar1 == iVar2)
                {
                    DisplayIconNames();
                    return;
                }

                uVar3 = 2;
                joined_r0x800579e0:
                if (iVar2 != -1)
                {
                    _gameEngine.PlayerManager.SetPlayerWeaponId(uVar3);
                    _gameEngine.SoundManager.PlaySoundEffect(2);
                    DisplayIconNames();
                    return;
                }
                break;

            case 3:
                iVar1 = _gameEngine.PlayerManager.GetWeaponIdFromSlot4();
                iVar2 = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();

                if (iVar2 == iVar1)
                {
                    DisplayIconNames();
                    return;
                }

                if (iVar1 != -1)
                {
                    _gameEngine.SoundManager.PlaySoundEffect(2);
                    uVar3 = 4;
                    _gameEngine.PlayerManager.SetPlayerWeaponId(uVar3);
                    DisplayIconNames();
                    return;
                }
                break;

            case 4:
                iVar1 = _gameEngine.PlayerManager.GetWeaponIdFromSlot5();
                iVar2 = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();

                if (iVar2 == iVar1)
                {
                    DisplayIconNames();
                    return;
                }

                if (iVar1 != -1)
                {
                    _gameEngine.SoundManager.PlaySoundEffect(2);
                    uVar3 = 5;
                    _gameEngine.PlayerManager.SetPlayerWeaponId(uVar3);
                    DisplayIconNames();
                    return;
                }
                break;

            case 5:
                iVar1 = (uint)_gameEngine.PlayerManager.GetNumberOfItem(_gameEngine.StaticVariables.g_ItemIdBySlotIndex[_gameEngine.StaticVariables.g_inventorySelectedSlotId]);
                iVar2 = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();

                if (iVar2 == _gameEngine.StaticVariables.g_ItemIdBySlotIndex[_gameEngine.StaticVariables.g_inventorySelectedSlotId])
                {
                    DisplayIconNames();
                    return;
                }

                if (iVar1 != 0)
                {
                    _gameEngine.SoundManager.PlaySoundEffect(2);
                    uVar3 = 6;
                    _gameEngine.PlayerManager.SetPlayerWeaponId(uVar3);
                    DisplayIconNames();
                    return;
                }
                break;

            default:
                break;
        }

        _gameEngine.SoundManager.PlaySoundEffect(3);
        DisplayIconNames();
    }

    //80055c84
    private void DisplayIconNames()
    {
        uint currentTileIndex;
        string sourceWarpName;

        currentTileIndex = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon(); //_gameEngine.StaticVariables.g_itemDropProperties[currentTileIndex * 2]

        if (currentTileIndex != 0xffffffff)
        {
            sourceWarpName = _gameEngine.EtcRes.GetItemName((int)currentTileIndex);

            InventoryWeaponNameSprites.Clear();
            _gameEngine.UIManager.DisplayIconName(
                _gameEngine.StaticVariables.g_ItemNameSprites,
                InventoryWeaponNameSprites,
                sourceWarpName.ToCharArray(),
                0x20,
                0, //_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.X,
                0, //(short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Y + 8),
                0);
        }

        currentTileIndex = _gameEngine.PlayerManager.SetItemIdFromCurrentItemId();

        if (currentTileIndex == 0xffffffff)
        {
            sourceWarpName = "       ";
        }
        else
        {
            sourceWarpName = _gameEngine.EtcRes.GetItemName((int)currentTileIndex);// _gameEngine.StaticVariables.g_itemDropProperties[currentTileIndex * 2];
        }

        SPRT[] sprites = [_gameEngine.StaticVariables.g_ItemNameSprites[2], _gameEngine.StaticVariables.g_ItemNameSprites[3], _gameEngine.StaticVariables.g_ItemNameSprites[4], _gameEngine.StaticVariables.g_ItemNameSprites[5]];

        InventoryItemNameSprites.Clear();
        _gameEngine.UIManager.DisplayIconName(sprites,
            InventoryItemNameSprites,
            sourceWarpName.ToCharArray(),
            0x20,
            0, //_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.X,
            0, //_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Y,
            1);
    }

    //80057854
    private void FUN_80057854()
    {
        uint currentItemId;
        int iVar2;

        var slotId = _gameEngine.StaticVariables.g_ItemIdBySlotIndex[_gameEngine.StaticVariables.g_inventorySelectedSlotId];

        if (slotId != 0)
        {
            if (slotId == -1)
            {
                var slotIndex = _gameEngine.StaticVariables.g_inventorySelectedSlotId;
                var value = (uint)GetItemIdFromSlotIndex(slotIndex);

                if (slotIndex < 6)
                {
                    value = _gameEngine.PlayerManager.GetWeaponIdBySlotId((int)value);
                }
                else
                {
                    value = _gameEngine.PlayerManager.GetItemIdFromSlotId(value);
                }

                slotId = (int)value;

                if (value == 0xffffffff)
                {
                    LAB_80057938:
                    _gameEngine.SoundManager.PlaySoundEffect(3);
                    return;
                }

                currentItemId = _gameEngine.PlayerManager.SetItemIdFromCurrentItemId();

                if (currentItemId == slotId)
                {
                    return;
                }
            }
            else
            {
                iVar2 = _gameEngine.PlayerManager.GetNumberOfItem(slotId);

                if (iVar2 == 0)
                {
                    //goto LAB_80057938;
                    _gameEngine.SoundManager.PlaySoundEffect(3);
                    return;
                }

                currentItemId = _gameEngine.PlayerManager.SetItemIdFromCurrentItemId();
                slotId = _gameEngine.StaticVariables.g_ItemIdBySlotIndex[_gameEngine.StaticVariables.g_inventorySelectedSlotId];

                if (currentItemId == slotId)
                {
                    return;
                }
            }

            _gameEngine.PlayerManager.SetCurrentItemId((uint) slotId);
            _gameEngine.SoundManager.PlaySoundEffect(2);
        }

        DisplayIconNames();
        FUN_8005ac90();
    }

    //8005ac90
    private void FUN_8005ac90()
    {
        uint iVar1;
        int iVar2;

        if (_gameEngine.StaticVariables.g_isCdResetRequested != 0
            || (_gameEngine.StaticVariables.g_cdIsReady != 0 && _gameEngine.StaticVariables.g_cdDataLoaded == 0))
        {
            iVar1 = _gameEngine.PlayerManager.SetItemIdFromCurrentItemId();

            if (iVar1 == 0x2f)
            {
                iVar2 = 2;
            }
            else if (iVar1 < 0x30)
            {
                if (iVar1 == 0x2b)
                {
                    iVar2 = 0;
                }
                else
                {
                    iVar2 = 1;
                    if (iVar1 != 0x2c)
                    {
                        return;
                    }
                }
            }
            else
            {
                iVar2 = 3;
                if (iVar1 != 0x30)
                {
                    return;
                }
            }
            //SetCdReadPosition(iVar2);
        }
    }

    //800556dc
    private void FUN_800556dc()
    {
        _gameEngine.StaticVariables.g_forbiddenWarpFlag |= 2;
        _gameEngine.SoundManager.PlaySoundEffect(5);
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].speed = 0xf;

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].x =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.X + _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].x = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.X;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].y =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].y = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].startX =
             (short)~(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Width << 3);

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].startY =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[0].startY = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponBackground.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].speed = 0xf;

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].x =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.X + _gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].x = _gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.X;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].y =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].y = _gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].startX =
             (short)~(ushort)(_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Width << 3);

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].startY =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[1].startY = _gameEngine.StaticVariables.g_UiBoxesInventoryItemBackground.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].speed = 0xf;

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].x =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.X + _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].x = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.X;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].y =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].y = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].startX = 0x140;

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].startY =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[2].startY = _gameEngine.StaticVariables.g_UiBoxesInventoryWeaponNameBackground.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].speed = 0xf;

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].x =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.X + _gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].x = _gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.X;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].y =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].y = _gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].startX = 0x140;

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].startY =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[3].startY = _gameEngine.StaticVariables.g_UiBoxesInventoryItemNameBackground.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].speed = 0xf;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].x =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.X + _gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].x = _gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.X;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].y =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].y = _gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].startX = 0x140;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].startY =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[4].startY = _gameEngine.StaticVariables.UIBoxConfiguration_800b9a10.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].speed = 0xf;

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].x =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X + _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].x = _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].y =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].y = _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].startX = 0x140;

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].startY =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[5].startY = _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].speed = 0xf;

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].x =
                 (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X + _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].x = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        }

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].y =
                 (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y + _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].y = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y;
        }

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].startX =
                 (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X + _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].startX = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        }

        _gameEngine.StaticVariables.TextToDisplay_ARRAY_8017f920[6].startY = 0xf0;
    }

}