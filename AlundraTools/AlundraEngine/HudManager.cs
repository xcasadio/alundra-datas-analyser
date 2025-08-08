using System.Diagnostics;
using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;
using AlundraEngine.Graphics;
using AlundraEngine.UI;

namespace AlundraEngine;

public class HudManager
{
    private readonly GameEngine _gameEngine;

    public HudManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    //80054a34
    public void InitializeInventorySpriteNumberOf()
    {
        SPRT sprite;

        FUN_800548a4(StaticVariables.TextTilesConfiguration_800b58a8);
        FUN_800548a4(StaticVariables.TextTilesConfiguration_800b8360);
        FUN_800548a4(StaticVariables.g_textTilesConfiguration_800b8eb0);
        FUN_800548a4(StaticVariables.TextTilesConfiguration_800b9a00);
        FUN_800548a4(StaticVariables.TextTilesConfiguration_800b9a10);
        FUN_800548a4(StaticVariables.TextTilesConfiguration_800b9e58);
        StaticVariables.g_forbiddenWarpFlag = 0;
        StaticVariables.g_inventorySelectedSlotId = 0;
        FUN_80050998(StaticVariables.g_inventoryCursorAnimation);

        var i = 0;

        do
        {
            var textToDisplay = StaticVariables.TextToDisplay_ARRAY_8017f920[i];
            textToDisplay.x = 8;
            textToDisplay.y = 0x10;
            textToDisplay.mode = StaticVariables.g_clutTable[StaticVariables.g_clutTableIndex];

            //sprite = StaticVariables.SPRT_ARRAY_8017fe74[i];
            //SetSprt(sprite);
            //SetSemiTrans(sprite, 0);
            //SetShadeTex(sprite, 1);

            i++;
        } while (i < 9);

        i = 0;

        do
        {
            //SetSprt(sprite2);
            //SetSemiTrans(sprite2, 0);
            //SetShadeTex(sprite2, 1);

            sprite = StaticVariables.g_spriteInventoryMoney[i];
            sprite.w = 8;
            sprite.h = 0x10;
            sprite.clut = StaticVariables.g_clutTable[StaticVariables.g_clutTableIndex];
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
            sprite = StaticVariables.g_spriteInventoryNumberOfKeys[i];
            sprite.w = 8;
            sprite.h = 0x10;
            sprite.clut = StaticVariables.g_clutTable[StaticVariables.g_clutTableIndex];
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

            sprite = StaticVariables.g_spriteInventoryNumberOfFalcon[i];
            sprite.w = 8;
            sprite.h = 0x10;
            sprite.clut = StaticVariables.g_clutTable[StaticVariables.g_clutTableIndex];
            sprite.r0 = 0x80;
            sprite.g0 = 0x80;
            sprite.b0 = 0x80;

            i++;
        } while (i < 4);
    }

    //80050998
    private void FUN_80050998(InventoryCursorAnimation cursorAnim)
    {
        int index;
        int iVar2;

        index = 0;
        iVar2 = 8;

        do
        {
            cursorAnim.Sprites[index].w = 0x10;
            cursorAnim.Sprites[index].h = 0x10;
            cursorAnim.Sprites[index].u0 = StaticVariables.g_inventoryCursorTextureU;
            cursorAnim.Sprites[index].v0 = StaticVariables.g_inventoryCursorTextureV;
            //SetSprt(p);
            //SetSemiTrans(p, 0);
            //SetShadeTex(p, 1);
            FUN_80050908(cursorAnim, 0, 0, index);
            cursorAnim.Sprites[index].clut = StaticVariables.g_clutTable[0];

            index += 1;

        } while (index < 2);
    }

    //80050908
    private void FUN_80050908(InventoryCursorAnimation cursorAnim, short x, short y, int index)
    {
        cursorAnim.Sprites[index].x0 = (short)(StaticVariables.g_inventoryCursorAnimSpriteX[cursorAnim.FrameDelay / 10 * 4] + x);
        cursorAnim.Sprites[index].y0 = (short)(StaticVariables.g_inventoryCursorAnimSpriteY[cursorAnim.FrameDelay / 10 * 4] + y);

    }

    //800548a4
    private void FUN_800548a4(TextTilesConfiguration textTilesConfig)
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
                        textTilesConfig.SpritesA[index].clut = StaticVariables.g_clutTable[textTilesConfig.SpritesA[index].clut];
                        textTilesConfig.SpritesB[index].clut = StaticVariables.g_clutTable[textTilesConfig.SpritesB[index].clut];
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
        SiImage image)

    {
        StaticVariables.g_hudTransitionStartX = 8;
        StaticVariables.g_hudTransitionStartY = 0x74;
        InitializeHudTransitionVariables(x, y, z, destX, destY, uvX, uvY, width, height, image);
    }

    //80057c18
    public void InitializeHudTransitionVariablesAndSetStart(int srcX, int srcY, int srcZ,
        int dstX, int dstY,
        sbyte u, sbyte v,
        SiImage image)
    {
        StaticVariables.g_hudTransitionStartX = 0xf8;
        StaticVariables.g_hudTransitionStartY = 0x68;
        _gameEngine.HudManager.InitializeHudTransitionVariables(srcX, srcY, srcZ,
            dstX, dstY, (byte)u, (byte)v, 0x30, 0x38, image);
    }

    //80057cf0
    public void InitializeHudTransitionVariables(
        int srcX, int srcY, int srcZ,
        int dstXPtr, int dstYPtr,
        byte uvX, byte uvY,
        short width, short height,
        SiImage image)
    {
        short puVar1;
        int i;
        byte uvBottom;
        byte uvRight;

        puVar1 = StaticVariables.g_hudTransitionState;

        if (StaticVariables.g_hudTransitionState == 0)
        {
            i = 0;
            uvRight = (byte)(uvX + width);
            uvBottom = (byte)(uvY + height);
            StaticVariables.g_hudTransitionState = 5;
            StaticVariables.g_hudTransitionSrcX = srcX >> 16;
            StaticVariables.g_hudTransitionSrcY = srcY >> 16;
            StaticVariables.g_hudTransitionSrcZ = srcZ >> 16;
            StaticVariables.g_hudTransitionDstXPtr = dstXPtr;
            StaticVariables.g_hudTransitionDstYPtr = dstYPtr;

            //Debugger.Break();

            do
            {
                var poly = StaticVariables.g_spriteInventoryAlundraPotrait[i];

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

            StaticVariables.g_hudDeltaX = StaticVariables.g_hudTransitionSrcX + 2
                                          - StaticVariables.g_hudTransitionDstXPtr;

            StaticVariables.g_hudX = StaticVariables.g_hudDeltaX - StaticVariables.g_hudTransitionStartX;
            StaticVariables.g_hudTransitionHalfWidth = 0x30;
            StaticVariables.g_hudTransitionHalfHeight = 0x38;
            StaticVariables.g_hudCurrentX = StaticVariables.g_hudTransitionStartX;
            StaticVariables.g_hudCurrentY = StaticVariables.g_hudTransitionStartY;
            StaticVariables.g_hudTransitionStepValue = 0xf;
            StaticVariables.g_hudDeltaY =
                StaticVariables.g_hudTransitionSrcY + 2
                - StaticVariables.g_hudTransitionDstYPtr
                - (StaticVariables.g_hudTransitionSrcZ + 2)
                - 0x20;
            StaticVariables.g_hudY = StaticVariables.g_hudDeltaY - StaticVariables.g_hudTransitionStartY;
        }
    }

    //80057ebc
    public void UpdateHudTransitionVariables()
    {
        ushort uVar1;
        ulong uVar2;
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

        uVar2 = 0; //StaticVariables.g_drawModes[0x14].tag;
        offsetX = 0;
        offsetY = 0;
        uVar4 = '\0';

        if (StaticVariables.g_hudTransitionState == 0)
        {
            return;
        }

        cameraX = (short)StaticVariables.g_hudCurrentX;
        cameraY = (short)StaticVariables.g_hudCurrentY;

        if (StaticVariables.g_hudTransitionStepValue == 0)
        {
            uVar1 = (ushort)(StaticVariables.g_hudTransitionState & 2);
            StaticVariables.g_hudTransitionState = (short)(StaticVariables.g_hudTransitionState & 0xfffe);
            if (uVar1 == 0)
            {
                uVar4 = (char)0x80;
                offsetX = (short)StaticVariables.g_hudTransitionHalfWidth;
                offsetY = (short)StaticVariables.g_hudTransitionHalfHeight;
            }
            else
            {
                cameraX = (short)StaticVariables.g_hudDeltaX;
                cameraY = (short)StaticVariables.g_hudDeltaY;
                StaticVariables.g_hudTransitionState = 0;
            }
            goto FinalizeTransitionUpdate;
        }

        cameraX = (short)(StaticVariables.g_hudTransitionStepValue * StaticVariables.g_hudX / 0xf + cameraX);
        cameraY = (short)(StaticVariables.g_hudTransitionStepValue * StaticVariables.g_hudY / 0xf + cameraY);

        if ((StaticVariables.g_hudTransitionState & 1) == 0)
        {
            uVar4 = '\0';
            offsetX = 0;
            offsetY = 0;

            if ((StaticVariables.g_hudTransitionState & 2) != 0)
            {
                iVar5 = StaticVariables.g_hudTransitionHalfWidth * StaticVariables.g_hudTransitionStepValue;
                divisionHalfWidth = (int)((ulong)((long)iVar5 * -0x77777777) >> 0x20);
                scaledHalfHeight = StaticVariables.g_hudTransitionHalfHeight * StaticVariables.g_hudTransitionStepValue;
                divisionHalfHeight = (int)((ulong)((long)scaledHalfHeight * -0x77777777) >> 0x20);
                alphaValue = (char)((0xf - StaticVariables.g_hudTransitionStepValue) * 0x80 / 0xf);

                //goto ComputeOffsets;
                uVar4 = (char)(alphaValue + '\x7f');
                offsetX = (short)((short)(divisionHalfWidth + iVar5 >> 3) - (short)(iVar5 >> 0x1f));
                offsetY = (short)((short)(divisionHalfHeight + scaledHalfHeight >> 3) - (short)(scaledHalfHeight >> 0x1f));

            }
        }
        else
        {
            iVar5 = StaticVariables.g_hudTransitionHalfWidth * (0xf - StaticVariables.g_hudTransitionStepValue);
            divisionHalfWidth = (int)((ulong)((long)iVar5 * -0x77777777) >> 0x20);
            scaledHalfHeight = StaticVariables.g_hudTransitionHalfHeight * (0xf - StaticVariables.g_hudTransitionStepValue);
            divisionHalfHeight = (int)((ulong)((long)scaledHalfHeight * -0x77777777) >> 0x20);
            alphaValue = (char)(StaticVariables.g_hudTransitionStepValue * 0x80 / 0xf);
            ComputeOffsets:
            uVar4 = (char)(alphaValue + '\x7f');
            offsetX = (short)((short)(divisionHalfWidth + iVar5 >> 3) - (short)(iVar5 >> 0x1f));
            offsetY = (short)((short)(divisionHalfHeight + scaledHalfHeight >> 3) - (short)(scaledHalfHeight >> 0x1f));
        }

        StaticVariables.g_hudTransitionStepValue += -1;

        FinalizeTransitionUpdate:

        StaticVariables.g_spriteInventoryAlundraPotrait[uVar2].r0 = (byte)uVar4;
        StaticVariables.g_spriteInventoryAlundraPotrait[uVar2].g0 = (byte)uVar4;
        StaticVariables.g_spriteInventoryAlundraPotrait[uVar2].b0 = (byte)uVar4;
        StaticVariables.g_spriteInventoryAlundraPotrait[uVar2].x0 = cameraX;
        StaticVariables.g_spriteInventoryAlundraPotrait[uVar2].y0 = cameraY;
        StaticVariables.g_spriteInventoryAlundraPotrait[uVar2].x1 = (short)(cameraX + offsetX);
        StaticVariables.g_spriteInventoryAlundraPotrait[uVar2].y1 = cameraY;
        StaticVariables.g_spriteInventoryAlundraPotrait[uVar2].x2 = cameraX;
        StaticVariables.g_spriteInventoryAlundraPotrait[uVar2].y2 = (short)(cameraY + offsetY);
        StaticVariables.g_spriteInventoryAlundraPotrait[uVar2].x3 = (short)(cameraX + offsetX);
        StaticVariables.g_spriteInventoryAlundraPotrait[uVar2].y3 = (short)(cameraY + offsetY);
    }

    //80057b84
    public void UpdateHudTransitionState()
    {
        if (StaticVariables.g_hudTransitionState != 0)
        {
            StaticVariables.g_hudCurrentX = StaticVariables.g_hudTransitionSrcX + 2 - StaticVariables.g_hudTransitionDstXPtr;
            StaticVariables.g_hudDeltaX = StaticVariables.g_hudTransitionStartX;
            StaticVariables.g_hudX = StaticVariables.g_hudTransitionStartX - StaticVariables.g_hudCurrentX;
            StaticVariables.g_hudDeltaY = StaticVariables.g_hudTransitionStartY;
            StaticVariables.g_hudTransitionState = 2;
            StaticVariables.g_hudCurrentY =
                StaticVariables.g_hudTransitionSrcY + 2 - StaticVariables.g_hudTransitionDstYPtr -
                (StaticVariables.g_hudTransitionSrcZ + 2) + -0x20;
            StaticVariables.g_hudY = StaticVariables.g_hudTransitionStartY - StaticVariables.g_hudCurrentY;
            StaticVariables.g_hudTransitionStepValue = 0xf;
        }
    }

    //80058134
    public void FUN_80058134()
    {
        //POLY_FT4* pPVar1;
        //DISPENV DStack_28;

        //GetDispEnv(&DStack_28);
        //SetDrawArea((DR_AREA*)(UINT_ARRAY_80180108 + g_drawModes[0x14].tag * 3), &DStack_28.disp);
        if (StaticVariables.g_hudTransitionState != 0)
        {
            _gameEngine.HudManager.UpdateHudTransitionVariables();
            var image = _gameEngine.GraphicManager.GetAnimationImageByIndex(0); // alundra portrait
            _gameEngine.GraphicManager.DrawPolyFt4(StaticVariables.g_spriteInventoryAlundraPotrait[0], image);
            //DrawPolyFt4(StaticVariables.g_spriteInventoryAlundraPotrait[1], image);
            //pPVar1 = StaticVariables.g_spriteInventoryAlundraPotrait + g_drawModes[0x14].tag;
            /* Probable PsyQ macro: addPrim(). */
            //pPVar1->tag = pPVar1->tag & 0xff000000 | *param_1 & 0xffffff;
            //*param_1 = *param_1 & 0xff000000 | (uint)pPVar1 & 0xffffff;
        }
    }

    //80055570
    public int DisplayInventory()
    {
        if (StaticVariables.g_forbiddenWarpFlag == 0)
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

            if (StaticVariables.g_cdIsReady == 0)
            {
                if ((StaticVariables.g_padState1.ButtonsHold & PadState.Right) != 0)
                {
                    _gameEngine.TriggerWarpTypeA();
                    return 1;
                }
                if ((StaticVariables.g_padState1.ButtonsHold & PadState.Left) != 0)
                {
                    //TriggerWarpTypeB();
                    return 0;
                }
                if ((StaticVariables.g_padState1.ButtonsHold & PadState.Up) != 0)
                {
                    //StartFadeOut();
                    return 1;
                }
                if ((StaticVariables.g_padState1.ButtonsHold & PadState.Down) != 0)
                {
                    //TriggerWarpTypeC();
                    return 1;
                }
            }

            _gameEngine.GraphicManager.InitializeFrame();
            _gameEngine.GraphicManager.SetTransitionType(6);
            var image = _gameEngine.GraphicManager.GetAnimationImageByIndex(0); //portrait alundra
            InitializeHudTransitionVariablesAndSetStart(
                StaticVariables.PlayerEntity.PosX, StaticVariables.PlayerEntity.PosY, StaticVariables.PlayerEntity.PosZ,
                StaticVariables.g_cameraScrollingX, StaticVariables.g_cameraScrollingY,
                (sbyte)image.Sx, (sbyte)image.Sy, image);
            //DisplayWarpNames();
            _gameEngine.SoundManager.PlaySoundEffect(4);
        }

        return 1;
    }


    //80054f1c Inventory display
    public void FUN_80054f1c(CallBackInfo callBackInfo)
    {
        StaticVariables.g_forbiddenWarpFlag = 5;
        StaticVariables.TextToDisplay_ARRAY_8017f920[0].mode = 2;
        StaticVariables.DAT_8017feec = 0;
        StaticVariables.TextToDisplay_ARRAY_8017f920[0].tick = 0;
        StaticVariables.TextToDisplay_ARRAY_8017f920[0].speed = 0xf;
        StaticVariables.g_playerControlFlags |= 8;
        StaticVariables.TextToDisplay_ARRAY_8017f920[0].x = (short)~(StaticVariables.TextTilesConfiguration_800b58a8.Width << 3);

        if (StaticVariables.TextTilesConfiguration_800b58a8.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[0].y =
                 (short)(StaticVariables.TextTilesConfiguration_800b58a8.Y + StaticVariables.TextTilesConfiguration_800b58a8.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[0].y = StaticVariables.TextTilesConfiguration_800b58a8.Y;
        }

        if (StaticVariables.TextTilesConfiguration_800b58a8.X < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[0].startX =
                 (short)(StaticVariables.TextTilesConfiguration_800b58a8.X + StaticVariables.TextTilesConfiguration_800b58a8.Width * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[0].startX = StaticVariables.TextTilesConfiguration_800b58a8.X;
        }

        if (StaticVariables.TextTilesConfiguration_800b58a8.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[0].startY =
                 (short)(StaticVariables.TextTilesConfiguration_800b58a8.Y + StaticVariables.TextTilesConfiguration_800b58a8.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[0].startY = StaticVariables.TextTilesConfiguration_800b58a8.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[0].originX = StaticVariables.TextTilesConfiguration_800b58a8.X;
        StaticVariables.TextToDisplay_ARRAY_8017f920[0].originY = StaticVariables.TextTilesConfiguration_800b58a8.Y;

        StaticVariables.TextToDisplay_ARRAY_8017f920[1].mode = 2;
        StaticVariables.TextToDisplay_ARRAY_8017f920[1].tick = 0;
        StaticVariables.TextToDisplay_ARRAY_8017f920[1].speed = 0xf;
        StaticVariables.TextToDisplay_ARRAY_8017f920[1].x = (short)~(StaticVariables.TextTilesConfiguration_800b8360.Width << 3);

        if (StaticVariables.TextTilesConfiguration_800b8360.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[1].y =
                 (short)(StaticVariables.TextTilesConfiguration_800b8360.Y + StaticVariables.TextTilesConfiguration_800b8360.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[1].y = StaticVariables.TextTilesConfiguration_800b8360.Y;
        }

        if (StaticVariables.TextTilesConfiguration_800b8360.X < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[1].startX =
                 (short)(StaticVariables.TextTilesConfiguration_800b8360.X + StaticVariables.TextTilesConfiguration_800b8360.Width * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[1].startX = StaticVariables.TextTilesConfiguration_800b8360.X;
        }

        if (StaticVariables.TextTilesConfiguration_800b8360.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[1].startY =
                 (short)(StaticVariables.TextTilesConfiguration_800b8360.Y + StaticVariables.TextTilesConfiguration_800b8360.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[1].startY = StaticVariables.TextTilesConfiguration_800b8360.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[1].originX = StaticVariables.TextTilesConfiguration_800b8360.X;
        StaticVariables.TextToDisplay_ARRAY_8017f920[1].originY = StaticVariables.TextTilesConfiguration_800b8360.Y;

        StaticVariables.TextToDisplay_ARRAY_8017f920[2].mode = 2;
        StaticVariables.TextToDisplay_ARRAY_8017f920[2].tick = 0;
        StaticVariables.TextToDisplay_ARRAY_8017f920[2].speed = 0xf;
        StaticVariables.TextToDisplay_ARRAY_8017f920[2].x = 0x140;

        if (StaticVariables.g_textTilesConfiguration_800b8eb0.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[2].y =
                 (short)(StaticVariables.g_textTilesConfiguration_800b8eb0.Y + StaticVariables.g_textTilesConfiguration_800b8eb0.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[2].y = StaticVariables.g_textTilesConfiguration_800b8eb0.Y;
        }

        if (StaticVariables.g_textTilesConfiguration_800b8eb0.X < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[2].startX =
                 (short)(StaticVariables.g_textTilesConfiguration_800b8eb0.X + StaticVariables.g_textTilesConfiguration_800b8eb0.Width * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[2].startX = StaticVariables.g_textTilesConfiguration_800b8eb0.X;
        }

        if (StaticVariables.g_textTilesConfiguration_800b8eb0.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[2].startY =
                 (short)(StaticVariables.g_textTilesConfiguration_800b8eb0.Y + StaticVariables.g_textTilesConfiguration_800b8eb0.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[2].startY = StaticVariables.g_textTilesConfiguration_800b8eb0.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[2].originX = StaticVariables.g_textTilesConfiguration_800b8eb0.X;
        StaticVariables.TextToDisplay_ARRAY_8017f920[2].originY = StaticVariables.g_textTilesConfiguration_800b8eb0.Y;

        StaticVariables.TextToDisplay_ARRAY_8017f920[3].mode = 2;
        StaticVariables.TextToDisplay_ARRAY_8017f920[3].tick = 0;
        StaticVariables.TextToDisplay_ARRAY_8017f920[3].speed = 0xf;
        StaticVariables.TextToDisplay_ARRAY_8017f920[3].x = 0x140;

        if (StaticVariables.TextTilesConfiguration_800b9a00.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[3].y =
                 (short)(StaticVariables.TextTilesConfiguration_800b9a00.Y + StaticVariables.TextTilesConfiguration_800b9a00.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[3].y = StaticVariables.TextTilesConfiguration_800b9a00.Y;
        }

        if (StaticVariables.TextTilesConfiguration_800b9a00.X < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[3].startX =
                 (short)(StaticVariables.TextTilesConfiguration_800b9a00.X + StaticVariables.TextTilesConfiguration_800b9a00.Width * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[3].startX = StaticVariables.TextTilesConfiguration_800b9a00.X;
        }

        if (StaticVariables.TextTilesConfiguration_800b9a00.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[3].startY =
                 (short)(StaticVariables.TextTilesConfiguration_800b9a00.Y + StaticVariables.TextTilesConfiguration_800b9a00.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[3].startY = StaticVariables.TextTilesConfiguration_800b9a00.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[3].originX = StaticVariables.TextTilesConfiguration_800b9a00.X;
        StaticVariables.TextToDisplay_ARRAY_8017f920[3].originY = StaticVariables.TextTilesConfiguration_800b9a00.Y;

        StaticVariables.TextToDisplay_ARRAY_8017f920[4].mode = 2;
        StaticVariables.TextToDisplay_ARRAY_8017f920[4].tick = 0;
        StaticVariables.TextToDisplay_ARRAY_8017f920[4].speed = 0xf;
        StaticVariables.TextToDisplay_ARRAY_8017f920[4].x = 0x140;

        if (StaticVariables.TextTilesConfiguration_800b9a10.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[4].y =
                 (short)(StaticVariables.TextTilesConfiguration_800b9a10.Y + StaticVariables.TextTilesConfiguration_800b9a10.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[4].y = StaticVariables.TextTilesConfiguration_800b9a10.Y;
        }

        if (StaticVariables.TextTilesConfiguration_800b9a10.X < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[4].startX =
                 (short)(StaticVariables.TextTilesConfiguration_800b9a10.X + StaticVariables.TextTilesConfiguration_800b9a10.Width * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[4].startX = StaticVariables.TextTilesConfiguration_800b9a10.X;
        }

        if (StaticVariables.TextTilesConfiguration_800b9a10.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[4].startY =
                 (short)(StaticVariables.TextTilesConfiguration_800b9a10.Y + StaticVariables.TextTilesConfiguration_800b9a10.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[4].startY = StaticVariables.TextTilesConfiguration_800b9a10.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[4].originX = StaticVariables.TextTilesConfiguration_800b9a10.X;
        StaticVariables.TextToDisplay_ARRAY_8017f920[4].originY = StaticVariables.TextTilesConfiguration_800b9a10.Y;

        StaticVariables.TextToDisplay_ARRAY_8017f920[5].mode = 2;
        StaticVariables.TextToDisplay_ARRAY_8017f920[5].tick = 0;
        StaticVariables.TextToDisplay_ARRAY_8017f920[5].speed = 0xf;
        StaticVariables.TextToDisplay_ARRAY_8017f920[5].x = 0x140;

        if (StaticVariables.TextTilesConfiguration_800b9e58.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[5].y =
                 (short)(StaticVariables.TextTilesConfiguration_800b9e58.Y + StaticVariables.TextTilesConfiguration_800b9e58.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[5].y = StaticVariables.TextTilesConfiguration_800b9e58.Y;
        }

        if (StaticVariables.TextTilesConfiguration_800b9e58.X < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[5].startX =
                 (short)(StaticVariables.TextTilesConfiguration_800b9e58.X + StaticVariables.TextTilesConfiguration_800b9e58.Width * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[5].startX = StaticVariables.TextTilesConfiguration_800b9e58.X;
        }

        if (StaticVariables.TextTilesConfiguration_800b9e58.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[5].startY =
                 (short)(StaticVariables.TextTilesConfiguration_800b9e58.Y + StaticVariables.TextTilesConfiguration_800b9e58.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[5].startY = StaticVariables.TextTilesConfiguration_800b9e58.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[5].originX = StaticVariables.TextTilesConfiguration_800b9e58.X;
        StaticVariables.TextToDisplay_ARRAY_8017f920[5].originY = StaticVariables.TextTilesConfiguration_800b9e58.Y;

        StaticVariables.TextToDisplay_ARRAY_8017f920[6].mode = 2;
        StaticVariables.TextToDisplay_ARRAY_8017f920[6].tick = 0;
        StaticVariables.TextToDisplay_ARRAY_8017f920[6].speed = 0xf;

        if (StaticVariables.g_textTilesConfiguration2.X < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[6].x =
                 (short)(StaticVariables.g_textTilesConfiguration2.X + StaticVariables.g_textTilesConfiguration2.Width * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[6].x = StaticVariables.g_textTilesConfiguration2.X;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[6].y = 0xf0;

        if (StaticVariables.g_textTilesConfiguration2.X < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[6].startX =
                 (short)(StaticVariables.g_textTilesConfiguration2.X + StaticVariables.g_textTilesConfiguration2.Width * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[6].startX = StaticVariables.g_textTilesConfiguration2.X;
        }

        if (StaticVariables.g_textTilesConfiguration2.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[6].startY =
                 (short)(StaticVariables.g_textTilesConfiguration2.Y + StaticVariables.g_textTilesConfiguration2.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[6].startY = StaticVariables.g_textTilesConfiguration2.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[6].originX = StaticVariables.g_textTilesConfiguration2.X;
        StaticVariables.TextToDisplay_ARRAY_8017f920[6].originY = StaticVariables.g_textTilesConfiguration2.Y;

        callBackInfo.RenderFunc = FUN_80056598;
    }

    //80056598
    private void FUN_80056598(CallBackInfo callbackInfo)
    {
        int iVar1;
        int iVar2;

        if ((StaticVariables.g_forbiddenWarpFlag & 6U) == 0)
        {
            if ((StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Down) != 0)
            {
                iVar1 = StaticVariables.g_inventorySelectedSlotId + 6;
                iVar2 = StaticVariables.g_inventorySelectedSlotId - 0x12;
                StaticVariables.g_inventorySelectedSlotId = iVar1;
                if (0x17 < iVar1)
                {
                    StaticVariables.g_inventorySelectedSlotId = iVar2;
                }
                _gameEngine.SoundManager.PlaySoundEffect(1);
                StaticVariables.DAT_8017feec = 0;
            }

            if ((StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Up) != 0)
            {
                iVar1 = StaticVariables.g_inventorySelectedSlotId - 6;
                if (StaticVariables.g_inventorySelectedSlotId - 6 < 0)
                {
                    iVar1 = StaticVariables.g_inventorySelectedSlotId + 0x12;
                }
                StaticVariables.g_inventorySelectedSlotId = iVar1;
                _gameEngine.SoundManager.PlaySoundEffect(1);
                StaticVariables.DAT_8017feec = 0;
            }

            if ((StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Right) != 0)
            {
                iVar1 = StaticVariables.g_inventorySelectedSlotId + 1;
                if (iVar1 == iVar1 / 6 * 6)
                {
                    iVar1 = StaticVariables.g_inventorySelectedSlotId - 5;
                }
                StaticVariables.g_inventorySelectedSlotId = iVar1;
                _gameEngine.SoundManager.PlaySoundEffect(1);
                StaticVariables.DAT_8017feec = 0;
            }

            if ((StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Left) != 0)
            {
                iVar1 = StaticVariables.g_inventorySelectedSlotId - 1;
                if (StaticVariables.g_inventorySelectedSlotId == StaticVariables.g_inventorySelectedSlotId / 6 * 6)
                {
                    iVar1 = StaticVariables.g_inventorySelectedSlotId + 5;
                }
                StaticVariables.g_inventorySelectedSlotId = iVar1;
                _gameEngine.SoundManager.PlaySoundEffect(1);
                StaticVariables.DAT_8017feec = 0;
            }

            if ((StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Cross) != 0)
            {
                if (StaticVariables.g_inventorySelectedSlotId < 6)
                {
                    FUN_8005795c();
                }
                else
                {
                    FUN_80057854();
                }
            }

            if ((StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.OpenInventory) != 0)
            {
                FUN_800556dc();
                _gameEngine.HudManager.UpdateHudTransitionState();
                _gameEngine.GraphicManager.PrepareBufferFlip();
            }

            if ((StaticVariables.g_padState1.ButtonsJustPressedByInterval & (PadState.R1 | PadState.L1)) != 0)
            {
                FUN_800556dc();
                StaticVariables.g_playerControlFlags |= 8;
                _gameEngine.HudManager.UpdateHudTransitionState();
                StaticVariables.g_postProcessState = 1;
            }
        }
        else
        {
            _gameEngine.UIManager.RenderTextTilesStep(StaticVariables.TextTilesConfiguration_800b58a8, StaticVariables.TextToDisplay_ARRAY_8017f920[0]);
            _gameEngine.UIManager.RenderTextTilesStep(StaticVariables.TextTilesConfiguration_800b8360, StaticVariables.TextToDisplay_ARRAY_8017f920[1]);
            _gameEngine.UIManager.RenderTextTilesStep(StaticVariables.g_textTilesConfiguration_800b8eb0, StaticVariables.TextToDisplay_ARRAY_8017f920[2]);
            _gameEngine.UIManager.RenderTextTilesStep(StaticVariables.TextTilesConfiguration_800b9a00, StaticVariables.TextToDisplay_ARRAY_8017f920[3]);
            _gameEngine.UIManager.RenderTextTilesStep(StaticVariables.TextTilesConfiguration_800b9a10, StaticVariables.TextToDisplay_ARRAY_8017f920[4]);
            _gameEngine.UIManager.RenderTextTilesStep(StaticVariables.g_textTilesConfiguration2, StaticVariables.TextToDisplay_ARRAY_8017f920[6]);
            iVar1 = _gameEngine.UIManager.RenderTextTilesStep(StaticVariables.TextTilesConfiguration_800b9e58, StaticVariables.TextToDisplay_ARRAY_8017f920[5]);

            if (iVar1 == 1)
            {
                if ((StaticVariables.g_forbiddenWarpFlag & 4U) != 0)
                {
                    StaticVariables.g_forbiddenWarpFlag = (int)(StaticVariables.g_forbiddenWarpFlag & 0xfffffffb);
                }

                if ((StaticVariables.g_forbiddenWarpFlag & 2U) != 0)
                {
                    StaticVariables.g_forbiddenWarpFlag = 0;
                    StaticVariables.TextTilesConfiguration_800b58a8.X = StaticVariables.TextToDisplay_ARRAY_8017f920[0].originX;
                    StaticVariables.TextTilesConfiguration_800b58a8.Y = StaticVariables.TextToDisplay_ARRAY_8017f920[0].originY;
                    StaticVariables.TextTilesConfiguration_800b8360.X = StaticVariables.TextToDisplay_ARRAY_8017f920[1].originX;
                    StaticVariables.TextTilesConfiguration_800b8360.Y = StaticVariables.TextToDisplay_ARRAY_8017f920[1].originY;
                    StaticVariables.g_textTilesConfiguration_800b8eb0.X = StaticVariables.TextToDisplay_ARRAY_8017f920[2].originX;
                    StaticVariables.g_textTilesConfiguration_800b8eb0.Y = StaticVariables.TextToDisplay_ARRAY_8017f920[2].originY;
                    StaticVariables.TextTilesConfiguration_800b9a00.X = StaticVariables.TextToDisplay_ARRAY_8017f920[3].originX;
                    StaticVariables.TextTilesConfiguration_800b9a00.Y = StaticVariables.TextToDisplay_ARRAY_8017f920[3].originY;
                    StaticVariables.TextTilesConfiguration_800b9a10.X = StaticVariables.TextToDisplay_ARRAY_8017f920[4].originX;
                    StaticVariables.TextTilesConfiguration_800b9a10.Y = StaticVariables.TextToDisplay_ARRAY_8017f920[4].originY;
                    StaticVariables.TextTilesConfiguration_800b9e58.X = StaticVariables.TextToDisplay_ARRAY_8017f920[5].originX;
                    StaticVariables.TextTilesConfiguration_800b9e58.Y = StaticVariables.TextToDisplay_ARRAY_8017f920[5].originY;
                    StaticVariables.g_textTilesConfiguration2.X = StaticVariables.TextToDisplay_ARRAY_8017f920[6].originX;
                    StaticVariables.g_textTilesConfiguration2.Y = StaticVariables.TextToDisplay_ARRAY_8017f920[6].originY;

                    if ((StaticVariables.g_postProcessState & 1U) == 0)
                    {
                        StaticVariables.g_playerControlFlags &= 0xfffffff7;
                    }

                    FUN_80047cb0(callbackInfo);
                    return;
                }
            }
        }

        FUN_80050908(StaticVariables.g_inventoryCursorAnimation,
            (short)(StaticVariables.TextTilesConfiguration_800b58a8.X +
                    StaticVariables.UINT_ARRAY_800b9f28[StaticVariables.g_inventorySelectedSlotId] + 0x12),
            (short)(StaticVariables.TextTilesConfiguration_800b58a8.Y +
                    StaticVariables.UINT_ARRAY_800b9f28[StaticVariables.g_inventorySelectedSlotId] + -8), 
            /*StaticVariables.g_drawModes[0x14].tag*/0);
        FUN_80050a74(StaticVariables.g_inventoryCursorAnimation);
        FUN_80056a98();
        FUN_80056fb4();
        FUN_80055d78(StaticVariables.TextTilesConfiguration_800b58a8);
        FUN_80055d78(StaticVariables.TextTilesConfiguration_800b8360);
        FUN_80055d78(StaticVariables.g_textTilesConfiguration_800b8eb0);
        FUN_80055d78(StaticVariables.TextTilesConfiguration_800b9a00);
        FUN_80055d78(StaticVariables.TextTilesConfiguration_800b9a10);
        FUN_80055d78(StaticVariables.TextTilesConfiguration_800b9e58);
        FUN_80055d78(StaticVariables.g_textTilesConfiguration2);
        FUN_800562dc();
        FUN_80055fe8();
    }

    //80056a98
    private void FUN_80056a98()
    {
        ulong uVar1;
        int value;
        int iVar2;
        uint uVar3;
        int iVar4;
        uint puVar5;
        uint puVar6;
        int iVar8;
        int i;
        short sVar10;
        int divisor;
        SPRT sprite;

        divisor = 1000;
        value = _gameEngine.PlayerManager.GetMoney();
        i = 0;
        sVar10 = 0x18;
        iVar8 = 0;

        do
        {
            if (divisor == 0)
            {
                Debugger.Break();
                //trap(0x1c00);
            }

            if (divisor == -1 && value == -0x80000000)
            {
                Debugger.Break();
                //trap(0x1800);
            }

            sprite = StaticVariables.g_spriteInventoryMoney[i];

            iVar2 = ((value / divisor) % 10) * 0x14;
            sprite.u0 = StaticVariables.BYTE_ARRAY_8009cfd8[iVar2];
            sprite.v0 = StaticVariables.BYTE_ARRAY_8009cfd8[iVar2 + 1];
            sprite.x0 = (short)(StaticVariables.TextTilesConfiguration_800b9e58.X + sVar10);
            sprite.y0 = (short)(StaticVariables.TextTilesConfiguration_800b9e58.Y + 4);

            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

            //puVar5 = (StaticVariables.sprite.tag + iVar8);
            //uVar3 = *puVar5;
            //puVar6 = StaticVariables.DAT_80146f6c[i];
            /* Probable PsyQ macro: addPrim(). */
            //*puVar5 = uVar3 & 0xff000000 | *puVar6 & 0xffffff;
            //*puVar6 = *puVar6 & 0xff000000 | (uint)sprite & 0xffffff;
            divisor = divisor / 10;
            i = i + 1;
        } while (i < 4);

        divisor = 10;
        value = _gameEngine.PlayerManager.GetNumberOfItem(0x3d);
        i = 0;

        do
        {
            if (divisor == 0)
            {
                Debugger.Break();
                //trap(0x1c00);
            }

            if (divisor == -1 && value == -0x80000000)
            {
                Debugger.Break();
                //trap(0x1800);
            }

            sprite = StaticVariables.g_spriteInventoryNumberOfKeys[i];

            iVar2 = ((value / divisor) % 10) * 0x14;
            sprite.u0 = StaticVariables.BYTE_ARRAY_8009cfd8[iVar2];
            sprite.v0 = StaticVariables.BYTE_ARRAY_8009cfd8[iVar2 + 1];
            sprite.x0 = (short)(StaticVariables.TextTilesConfiguration_800b9e58.X + sVar10 + 0x10);
            sprite.y0 = (short)(StaticVariables.TextTilesConfiguration_800b9e58.Y + 0x34);

            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

            //puVar5 = (StaticVariables.sprite.tag + iVar8);
            //uVar3 = *puVar5;
            //puVar6 = StaticVariables.DAT_80146f6c[i];
            /* Probable PsyQ macro: addPrim(). */
            //*puVar5 = uVar3 & 0xff000000 | *puVar6 & 0xffffff;
            //*puVar6 = *puVar6 & 0xff000000 | (uint)sprite & 0xffffff;
            divisor = divisor / 10;
            i = i + 1;
        } while (i < 2);

        divisor = 10;
        value = _gameEngine.PlayerManager.GetNumberOfFalcon();
        i = _gameEngine.PlayerManager.GetNumberOfFalconTemp();

        do
        {

            if (divisor == 0)
            {
                Debugger.Break();
                //trap(0x1c00);
            }

            if (divisor == -1 && value == -0x80000000)
            {
                Debugger.Break();
                //trap(0x1800);
            }

            sprite = StaticVariables.g_spriteInventoryNumberOfFalcon[i];

            iVar2 = ((value / divisor) % 10) * 0x14;
            sprite.u0 = StaticVariables.BYTE_ARRAY_8009cfd8[iVar2];
            sprite.v0 = StaticVariables.BYTE_ARRAY_8009cfd8[iVar2 + 1];
            sprite.x0 = (short)(StaticVariables.TextTilesConfiguration_800b9e58.X + sVar10 + 0x10);
            sprite.y0 = (short)(StaticVariables.TextTilesConfiguration_800b9e58.Y + 0x34);

            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

            //puVar5 = (StaticVariables.sprite.tag + iVar8);
            //uVar3 = *puVar5;
            //puVar6 = StaticVariables.DAT_80146f6c[i];
            /* Probable PsyQ macro: addPrim(). */
            //*puVar5 = uVar3 & 0xff000000 | *puVar6 & 0xffffff;
            //*puVar6 = *puVar6 & 0xff000000 | (uint)sprite & 0xffffff;
            divisor = divisor / 10;
            i = i + 1;
        } while (iVar2 < 2);
    }

    //80050a74
    private void FUN_80050a74(InventoryCursorAnimation cursorAnim)
    {
        cursorAnim.FrameDelay += 1;

        if (cursorAnim.FrameDelay == 0x28)
        {
            cursorAnim.FrameDelay = 0;
        }

        cursorAnim.Sprites[0].u0 = (byte)(StaticVariables.g_inventoryCursorTextureU + cursorAnim.FrameDelay / 10 * 0x28);
        cursorAnim.Sprites[0].v0 = (byte)(StaticVariables.g_inventoryCursorTextureV + cursorAnim.FrameDelay / 10 * 0x28);

        cursorAnim.Sprites[1].u0 = (byte)(StaticVariables.g_inventoryCursorTextureU + cursorAnim.FrameDelay / 10 * 0x28);
        cursorAnim.Sprites[1].v0 = (byte)(StaticVariables.g_inventoryCursorTextureV + cursorAnim.FrameDelay / 10 * 0x28);

        //puVar2 = StaticVariables.DAT_80146f6c[g_drawModes[0x14].tag * 0x28];
        //pSVar3 = cursorAnim.Sprites[StaticVariables.g_drawModes[0x14].tag];
        /* Probable PsyQ macro: addPrim(). */
        //cursorAnim.sprites[g_drawModes[0x14].tag].tag = cursorAnim.sprites[g_drawModes[0x14].tag].tag & 0xff000000 | *puVar2 & 0xffffff;
        //*puVar2 = *puVar2 & 0xff000000 | (uint)pSVar3 & 0xffffff;

        var bitmap = _gameEngine.Font3.GenerateFontBitmapFromSprite(cursorAnim.Sprites[0]);
        Debugger.Break();
        _gameEngine.Renderer.AddSprite(cursorAnim.Sprites[0], int.MaxValue, bitmap);
    }

    //8005795c
    private void FUN_8005795c()
    {
        uint iVar1;
        uint iVar2;
        ushort uVar3;

        switch (StaticVariables.g_inventorySelectedSlotId)
        {
            case 0:
                iVar2 = _gameEngine.GetWeaponIdFromSlot1();
                iVar1 = _gameEngine.GetItemIdFromCurrentWeapon();

                if (iVar1 == iVar2)
                {
                    DisplayWarpNames();
                    return;
                }

                uVar3 = 1;
                goto joined_r0x800579e0;

            case 1:
                iVar1 = _gameEngine.GetWeaponIdFromSlot3();
                iVar2 = _gameEngine.GetItemIdFromCurrentWeapon();

                if (iVar2 == iVar1)
                {
                    DisplayWarpNames();
                    return;
                }

                if (iVar1 != -1)
                {
                    _gameEngine.SoundManager.PlaySoundEffect(2);
                    uVar3 = 3;
                    //LAB_80057b04:
                    _gameEngine.PlayerManager.SetPlayerWeaponId(uVar3);
                    DisplayWarpNames();
                    return;
                }
                break;

            case 2:
                iVar2 = _gameEngine.GetWeaponIdFromSlot2();
                iVar1 = _gameEngine.GetItemIdFromCurrentWeapon();

                if (iVar1 == iVar2)
                {
                    DisplayWarpNames();
                    return;
                }

                uVar3 = 2;
                joined_r0x800579e0:
                if (iVar2 != -1)
                {
                    _gameEngine.PlayerManager.SetPlayerWeaponId(uVar3);
                    _gameEngine.SoundManager.PlaySoundEffect(2);
                    DisplayWarpNames();
                    return;
                }
                break;

            case 3:
                iVar1 = _gameEngine.GetWeaponIdFromSlot4();
                iVar2 = _gameEngine.GetItemIdFromCurrentWeapon();

                if (iVar2 == iVar1)
                {
                    DisplayWarpNames();
                    return;
                }

                if (iVar1 != -1)
                {
                    _gameEngine.SoundManager.PlaySoundEffect(2);
                    uVar3 = 4;
                    _gameEngine.PlayerManager.SetPlayerWeaponId(uVar3);
                    DisplayWarpNames();
                    return;
                }
                break;

            case 4:
                iVar1 = _gameEngine.GetWeaponIdFromSlot5();
                iVar2 = _gameEngine.GetItemIdFromCurrentWeapon();

                if (iVar2 == iVar1)
                {
                    DisplayWarpNames();
                    return;
                }

                if (iVar1 != -1)
                {
                    _gameEngine.SoundManager.PlaySoundEffect(2);
                    uVar3 = 5;
                    _gameEngine.PlayerManager.SetPlayerWeaponId(uVar3);
                    DisplayWarpNames();
                    return;
                }
                break;

            case 5:
                iVar1 = (uint)_gameEngine.PlayerManager.GetNumberOfItem((int)StaticVariables.UINT_ARRAY_800b9ec8[StaticVariables.g_inventorySelectedSlotId]);
                iVar2 = _gameEngine.GetItemIdFromCurrentWeapon();

                if (iVar2 == StaticVariables.UINT_ARRAY_800b9ec8[StaticVariables.g_inventorySelectedSlotId])
                {
                    DisplayWarpNames();
                    return;
                }

                if (iVar1 != 0)
                {
                    _gameEngine.SoundManager.PlaySoundEffect(2);
                    uVar3 = 6;
                    _gameEngine.PlayerManager.SetPlayerWeaponId(uVar3);
                    DisplayWarpNames();
                    return;
                }
                break;

            default:
                break;
        }

        _gameEngine.SoundManager.PlaySoundEffect(3);
        DisplayWarpNames();
    }

    //80055c84
    private void DisplayWarpNames()
    {
        uint currentTileIndex;
        string sourceWarpName;

        currentTileIndex = _gameEngine.GetItemIdFromCurrentWeapon(); //StaticVariables.g_iconNameEtcBase[currentTileIndex * 2]

        if (currentTileIndex != -1)
        {
            sourceWarpName = _gameEngine.EtcResR.GetIconName((int)currentTileIndex);

            _gameEngine.GraphicManager.DisplayIconName(
                StaticVariables.g_ItemNameSprites,
                sourceWarpName.ToCharArray(),
                0x20,
                StaticVariables.g_textTilesConfiguration_800b8eb0.X,
                (short)(StaticVariables.g_textTilesConfiguration_800b8eb0.Y + 8),
                0);
        }
        currentTileIndex = (uint)_gameEngine.SetItemIdFromCurrentItemId();

        if (currentTileIndex == -1)
        {
            sourceWarpName = "       ";
        }
        else
        {
            sourceWarpName = _gameEngine.EtcResR.GetIconName((int)currentTileIndex);// StaticVariables.g_iconNameEtcBase[currentTileIndex * 2];
        }

        _gameEngine.GraphicManager.DisplayIconName([StaticVariables.g_ItemNameSprites[2]],
            sourceWarpName.ToCharArray(),
            0x20,
            StaticVariables.TextTilesConfiguration_800b9a00.X,
            StaticVariables.TextTilesConfiguration_800b9a00.Y,
            1);
    }

    //80047cb0
    public int FUN_80047cb0(CallBackInfo callBackInfo)
    {
        callBackInfo.Flags = 0;
        return 1;
    }



    //80057854
    private void FUN_80057854()
    {
        uint currentItemId;
        int iVar2;
        uint slotId;

        slotId = StaticVariables.UINT_ARRAY_800b9ec8[StaticVariables.g_inventorySelectedSlotId];
        if (slotId != 0)
        {
            if (slotId == 0xffffffff)
            {
                //PTR_GetWeaponIdFromSlot1_800b9e68
                slotId = StaticVariables.g_inventorySelectedSlotId switch
                {
                    1 => _gameEngine.GetWeaponIdFromSlot1(),
                    2 => _gameEngine.GetWeaponIdFromSlot3(),
                    3 => _gameEngine.GetWeaponIdFromSlot2(),
                    4 => _gameEngine.GetWeaponIdFromSlot4(),
                    5 => _gameEngine.GetWeaponIdFromSlot5(),
                    _ => slotId
                };

                if (slotId == 0xffffffff)
                {
                    LAB_80057938:
                    _gameEngine.SoundManager.PlaySoundEffect(3);
                    return;
                }

                currentItemId = (uint)_gameEngine.SetItemIdFromCurrentItemId();

                if (currentItemId == slotId)
                {
                    return;
                }
            }
            else
            {
                iVar2 = _gameEngine.PlayerManager.GetNumberOfItem((int)slotId);

                if (iVar2 == 0)
                {
                    //goto LAB_80057938;
                    _gameEngine.SoundManager.PlaySoundEffect(3);
                    return;
                }

                currentItemId = (uint)_gameEngine.SetItemIdFromCurrentItemId();
                slotId = StaticVariables.UINT_ARRAY_800b9ec8[StaticVariables.g_inventorySelectedSlotId];

                if (currentItemId == slotId)
                {
                    return;
                }
            }

            _gameEngine.SetCurrentItemId(slotId);
            _gameEngine.SoundManager.PlaySoundEffect(2);
        }

        DisplayWarpNames();
        FUN_8005ac90();
    }

    //8005ac90
    private void FUN_8005ac90()
    {
        int iVar1;
        int iVar2;

        if (StaticVariables.g_isCdResetRequested != 0
            || (StaticVariables.g_cdIsReady != 0 && StaticVariables.g_cdDataLoaded == 0))
        {
            iVar1 = _gameEngine.SetItemIdFromCurrentItemId();
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
        StaticVariables.g_forbiddenWarpFlag |= 2;
        _gameEngine.SoundManager.PlaySoundEffect(5);
        StaticVariables.TextToDisplay_ARRAY_8017f920[0].mode = 2;
        StaticVariables.TextToDisplay_ARRAY_8017f920[0].tick = 0;
        StaticVariables.TextToDisplay_ARRAY_8017f920[0].speed = 0xf;

        if (StaticVariables.TextTilesConfiguration_800b58a8.X < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[0].x =
                 (short)(StaticVariables.TextTilesConfiguration_800b58a8.X + StaticVariables.TextTilesConfiguration_800b58a8.Width * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[0].x = StaticVariables.TextTilesConfiguration_800b58a8.X;
        }

        if (StaticVariables.TextTilesConfiguration_800b58a8.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[0].y =
                 (short)(StaticVariables.TextTilesConfiguration_800b58a8.Y + StaticVariables.TextTilesConfiguration_800b58a8.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[0].y = StaticVariables.TextTilesConfiguration_800b58a8.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[0].startX =
             (short)~(StaticVariables.TextTilesConfiguration_800b58a8.Width << 3);

        if (StaticVariables.TextTilesConfiguration_800b58a8.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[0].startY =
                 (short)(StaticVariables.TextTilesConfiguration_800b58a8.Y + StaticVariables.TextTilesConfiguration_800b58a8.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[0].startY = StaticVariables.TextTilesConfiguration_800b58a8.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[1].mode = 2;
        StaticVariables.TextToDisplay_ARRAY_8017f920[1].tick = 0;
        StaticVariables.TextToDisplay_ARRAY_8017f920[1].speed = 0xf;

        if (StaticVariables.TextTilesConfiguration_800b8360.X < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[1].x =
                 (short)(StaticVariables.TextTilesConfiguration_800b8360.X + StaticVariables.TextTilesConfiguration_800b8360.Width * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[1].x = StaticVariables.TextTilesConfiguration_800b8360.X;
        }

        if (StaticVariables.TextTilesConfiguration_800b8360.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[1].y =
                 (short)(StaticVariables.TextTilesConfiguration_800b8360.Y + StaticVariables.TextTilesConfiguration_800b8360.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[1].y = StaticVariables.TextTilesConfiguration_800b8360.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[1].startX =
             (short)~(ushort)((int)StaticVariables.TextTilesConfiguration_800b8360.Width << 3);

        if (StaticVariables.TextTilesConfiguration_800b8360.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[1].startY =
                 (short)(StaticVariables.TextTilesConfiguration_800b8360.Y + StaticVariables.TextTilesConfiguration_800b8360.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[1].startY = StaticVariables.TextTilesConfiguration_800b8360.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[2].mode = 2;
        StaticVariables.TextToDisplay_ARRAY_8017f920[2].tick = 0;
        StaticVariables.TextToDisplay_ARRAY_8017f920[2].speed = 0xf;

        if (StaticVariables.g_textTilesConfiguration_800b8eb0.X < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[2].x =
                 (short)(StaticVariables.g_textTilesConfiguration_800b8eb0.X + StaticVariables.g_textTilesConfiguration_800b8eb0.Width * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[2].x = StaticVariables.g_textTilesConfiguration_800b8eb0.X;
        }

        if (StaticVariables.g_textTilesConfiguration_800b8eb0.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[2].y =
                 (short)(StaticVariables.g_textTilesConfiguration_800b8eb0.Y + StaticVariables.g_textTilesConfiguration_800b8eb0.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[2].y = StaticVariables.g_textTilesConfiguration_800b8eb0.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[2].startX = 0x140;

        if (StaticVariables.g_textTilesConfiguration_800b8eb0.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[2].startY =
                 (short)(StaticVariables.g_textTilesConfiguration_800b8eb0.Y + StaticVariables.g_textTilesConfiguration_800b8eb0.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[2].startY = StaticVariables.g_textTilesConfiguration_800b8eb0.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[3].mode = 2;
        StaticVariables.TextToDisplay_ARRAY_8017f920[3].tick = 0;
        StaticVariables.TextToDisplay_ARRAY_8017f920[3].speed = 0xf;

        if (StaticVariables.TextTilesConfiguration_800b9a00.X < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[3].x =
                 (short)(StaticVariables.TextTilesConfiguration_800b9a00.X + StaticVariables.TextTilesConfiguration_800b9a00.Width * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[3].x = StaticVariables.TextTilesConfiguration_800b9a00.X;
        }

        if (StaticVariables.TextTilesConfiguration_800b9a00.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[3].y =
                 (short)(StaticVariables.TextTilesConfiguration_800b9a00.Y + StaticVariables.TextTilesConfiguration_800b9a00.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[3].y = StaticVariables.TextTilesConfiguration_800b9a00.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[3].startX = 0x140;

        if (StaticVariables.TextTilesConfiguration_800b9a00.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[3].startY =
                 (short)(StaticVariables.TextTilesConfiguration_800b9a00.Y + StaticVariables.TextTilesConfiguration_800b9a00.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[3].startY = StaticVariables.TextTilesConfiguration_800b9a00.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[4].mode = 2;
        StaticVariables.TextToDisplay_ARRAY_8017f920[4].tick = 0;
        StaticVariables.TextToDisplay_ARRAY_8017f920[4].speed = 0xf;

        if (StaticVariables.TextTilesConfiguration_800b9a10.X < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[4].x =
                 (short)(StaticVariables.TextTilesConfiguration_800b9a10.X + StaticVariables.TextTilesConfiguration_800b9a10.Width * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[4].x = StaticVariables.TextTilesConfiguration_800b9a10.X;
        }

        if (StaticVariables.TextTilesConfiguration_800b9a10.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[4].y =
                 (short)(StaticVariables.TextTilesConfiguration_800b9a10.Y + StaticVariables.TextTilesConfiguration_800b9a10.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[4].y = StaticVariables.TextTilesConfiguration_800b9a10.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[4].startX = 0x140;

        if (StaticVariables.TextTilesConfiguration_800b9a10.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[4].startY =
                 (short)(StaticVariables.TextTilesConfiguration_800b9a10.Y + StaticVariables.TextTilesConfiguration_800b9a10.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[4].startY = StaticVariables.TextTilesConfiguration_800b9a10.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[5].mode = 2;
        StaticVariables.TextToDisplay_ARRAY_8017f920[5].tick = 0;
        StaticVariables.TextToDisplay_ARRAY_8017f920[5].speed = 0xf;

        if (StaticVariables.TextTilesConfiguration_800b9e58.X < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[5].x =
                 (short)(StaticVariables.TextTilesConfiguration_800b9e58.X + StaticVariables.TextTilesConfiguration_800b9e58.Width * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[5].x = StaticVariables.TextTilesConfiguration_800b9e58.X;
        }

        if (StaticVariables.TextTilesConfiguration_800b9e58.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[5].y =
                 (short)(StaticVariables.TextTilesConfiguration_800b9e58.Y + StaticVariables.TextTilesConfiguration_800b9e58.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[5].y = StaticVariables.TextTilesConfiguration_800b9e58.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[5].startX = 0x140;

        if (StaticVariables.TextTilesConfiguration_800b9e58.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[5].startY =
                 (short)(StaticVariables.TextTilesConfiguration_800b9e58.Y + StaticVariables.TextTilesConfiguration_800b9e58.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[5].startY = StaticVariables.TextTilesConfiguration_800b9e58.Y;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[6].mode = 2;
        StaticVariables.TextToDisplay_ARRAY_8017f920[6].tick = 0;
        StaticVariables.TextToDisplay_ARRAY_8017f920[6].speed = 0xf;

        if (StaticVariables.g_textTilesConfiguration2.X < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[6].x =
                 (short)(StaticVariables.g_textTilesConfiguration2.X + StaticVariables.g_textTilesConfiguration2.Width * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[6].x = StaticVariables.g_textTilesConfiguration2.X;
        }

        if (StaticVariables.g_textTilesConfiguration2.Y < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[6].y =
                 (short)(StaticVariables.g_textTilesConfiguration2.Y + StaticVariables.g_textTilesConfiguration2.Height * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[6].y = StaticVariables.g_textTilesConfiguration2.Y;
        }

        if (StaticVariables.g_textTilesConfiguration2.X < 0)
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[6].startX =
                 (short)(StaticVariables.g_textTilesConfiguration2.X + StaticVariables.g_textTilesConfiguration2.Width * -8);
        }
        else
        {
            StaticVariables.TextToDisplay_ARRAY_8017f920[6].startX = StaticVariables.g_textTilesConfiguration2.X;
        }

        StaticVariables.TextToDisplay_ARRAY_8017f920[6].startY = 0xf0;
    }
}