using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;
using AlundraEngine.Graphics;
using AlundraEngine.UI;
using System.Diagnostics;
using static AlundraEngine.Renderer;

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
            DisplayWarpNames();
            _gameEngine.SoundManager.PlaySoundEffect(4);
        }

        return 1;
    }


    //80054f1c Inventory display
    public void FUN_80054f1c(CallBackInfo callBackInfo)
    {
        StaticVariables.g_forbiddenWarpFlag = 5;
        StaticVariables.TextToDisplay_ARRAY_8017f920[0].mode = 2;
        StaticVariables.INT_8017feec = 0;
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
                StaticVariables.INT_8017feec = 0;
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
                StaticVariables.INT_8017feec = 0;
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
                StaticVariables.INT_8017feec = 0;
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
                StaticVariables.INT_8017feec = 0;
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

    //80055fe8
    private void FUN_80055fe8()
    {
        int uVar1;
        int iVar2;
        uint itemId;

        itemId = StaticVariables.UINT_ARRAY_800b9ec8[StaticVariables.g_inventorySelectedSlotId];

        if (itemId == 0)
        {
            return;
        }

        if (itemId == 0xffffffff)
        {
            var weaponId = StaticVariables.g_inventorySelectedSlotId;

            weaponId = weaponId switch
            {
                1 => 2,
                2 => 1,
                _ => weaponId
            };

            itemId = _gameEngine.PlayerManager.GetItemIdFromSlotId((uint)weaponId);

            if (itemId == 0xffffffff)
            {
                return;
            }
        }
        else
        {
            iVar2 = _gameEngine.PlayerManager.GetNumberOfItem((int)itemId);

            if (iVar2 == 0)
            {
                return;
            }
        }

        if (StaticVariables.INT_8017feec == 0)
        {
            var text = _gameEngine.EtcResR.GetEtcString((int)itemId);//StaticVariables.g_iconNameEtcBase[itemId * 2];
            iVar2 = 0x20;

            LAB_8005616c:
            _gameEngine.GraphicManager.DisplayIconName(StaticVariables.g_spriteInventoryText, 
                text.ToCharArray(), 
                iVar2, 
                StaticVariables.g_textTilesConfiguration2.X,
                StaticVariables.g_textTilesConfiguration2.Y, 2);
            StaticVariables.INT_8017fef0 = 0;
            StaticVariables.g_spriteInventoryText[0].w = 0;
            StaticVariables.g_spriteInventoryText[1].w = 0;
            StaticVariables.INT_8017feec = StaticVariables.INT_8017feec + 1;
        }
        else
        {
            if (StaticVariables.INT_8017feec - 1U < 0x10)
            {
                var text = _gameEngine.EtcResR.GetEtcString((int)itemId);//StaticVariables.g_iconNameEtcBase[itemId * 2];

                Debugger.Break(); //text.Length == StaticVariables.INT_8017feec - 1

                if (text[StaticVariables.INT_8017feec - 1] == '\0')
                {
                    StaticVariables.INT_8017feec = 0x11;
                    uVar1 = 0;
                }
                else
                {
                    //var text = _gameEngine.EtcResR.GetDescriptionString((int)itemId); //StaticVariables.g_iconNameEtcBase[itemId * 2];
                    FUN_80055f48(0, 
                        StaticVariables.INT_8017feec,
                        text[StaticVariables.INT_8017feec]);
                    uVar1 = 0;
                }
            }
            else if (StaticVariables.INT_8017feec - 0x11U < 0x3c)
            {
                uVar1 = 0;
                StaticVariables.INT_8017feec = StaticVariables.INT_8017feec + 1;
            }
            else
            {
                if (StaticVariables.INT_8017feec == 0x4d)
                {
                    var text = _gameEngine.EtcResR.GetEtcString((int)itemId);//StaticVariables.g_iconNameEtcBase[itemId * 2];
                    //text = StaticVariables.g_tileSetEtcBase[itemId * 2];
                    iVar2 = 0x40;
                    //goto LAB_8005616c;

                    _gameEngine.GraphicManager.DisplayIconName(StaticVariables.g_spriteInventoryText,
                        text.ToCharArray(),
                        iVar2,
                        StaticVariables.g_textTilesConfiguration2.X,
                        StaticVariables.g_textTilesConfiguration2.Y, 2);
                    StaticVariables.INT_8017fef0 = 0;
                    StaticVariables.g_spriteInventoryText[0].w = 0;
                    StaticVariables.g_spriteInventoryText[1].w = 0;
                    StaticVariables.INT_8017feec = StaticVariables.INT_8017feec + 1;
                    return;
                }

                if (StaticVariables.INT_8017feec - 0x4eU < 0x40)
                {
                    Debugger.Break();
                    uVar1 = 0;
                    var text = _gameEngine.EtcResR.GetEtcString((int)itemId);//StaticVariables.g_tileSetEtcBase[itemId * 2];
                    /*
                    if (StaticVariables.g_tileSetEtcBase)[itemId * 2] + StaticVariables.INT_8017feec + -0x4e) == '\0')
                    {
                        StaticVariables.INT_8017feec = 0x8e;
                    }
                    else
                    {
                        FUN_80055f48(0, 
                            StaticVariables.INT_8017feec + -0x4d,
                            StaticVariables.g_tileSetEtcBase[itemId * 2] + StaticVariables.INT_8017feec + -0x4e);
                        uVar1 = 0;
                    }*/
                }
                else
                {
                    if (StaticVariables.INT_8017feec == 0x8e)
                    {
                        SPRT[] sprites =
                        [
                            StaticVariables.g_spriteInventoryText[2],
                            StaticVariables.g_spriteInventoryText[3],
                            StaticVariables.g_spriteInventoryText[4],
                            StaticVariables.g_spriteInventoryText[5]
                        ];

                        var text3 = _gameEngine.EtcResR.GetIconName((int)itemId);//StaticVariables.g_paletteSetEtcBase

                        _gameEngine.GraphicManager.DisplayIconName(sprites,
                            text3.ToCharArray(), 
                            0x40,
                            StaticVariables.g_textTilesConfiguration2.X, 
                            StaticVariables.g_textTilesConfiguration2.Y, 
                            3);
                        StaticVariables.INT_8017fef0 = 0;
                        StaticVariables.INT_8017feec = StaticVariables.INT_8017feec + 1;
                        FUN_80055e30(0);
                        StaticVariables.g_spriteInventoryText[2].w = 0;
                        StaticVariables.g_spriteInventoryText[3].w = 0;
                        return;
                    }

                    if (StaticVariables.INT_8017feec - 0x8fU < 0x40)
                    {
                        Debugger.Break();
                        //if (StaticVariables.g_paletteSetEtcBase[itemId * 2] + StaticVariables.INT_8017feec + -0x8f == '\0')
                        //{
                        //    StaticVariables.INT_8017feec = 0xcf;
                        //}
                        //else
                        //{
                        //    FUN_80055f48(1, StaticVariables.INT_8017feec + -0x90,
                        //        StaticVariables.g_paletteSetEtcBase)[itemId * 2] + StaticVariables.INT_8017feec + -0x8f));
                        //}
                    }
                    else if (StaticVariables.INT_8017feec != 0xcf)
                    {
                        return;
                    }

                    FUN_80055e30(0);
                    uVar1 = 1;
                }
            }

            FUN_80055e30(uVar1);
        }
    }

    //80055e30
    private void FUN_80055e30(int index)
    {
        short sVar1;
        ulong uVar2;
        uint puVar3;
        int iVar4;
        uint uVar5;
        short sVar6;
        SPRT sprite;
        int iVar8;
        uint puVar9;

        iVar8 = 0;
        sVar6 = 0xc;
        iVar4 = index; // * 0x28 + StaticVariables.g_drawModes[0x14].tag * 0x14;

        do
        {
            iVar8 = iVar8 + 1;
            StaticVariables.g_spriteInventoryText[iVar4].x0 = (short)(StaticVariables.g_textTilesConfiguration2.X + 0x10);
            sVar1 = (short)(StaticVariables.g_textTilesConfiguration2.Y + sVar6 + index * 0x10);
            StaticVariables.g_spriteInventoryText[iVar4].y0 = sVar1;
            //uVar2 = StaticVariables.g_drawModes[0x14].tag;
            iVar4 = iVar4 + 1;
            sVar6 = (short)(sVar6 - 1);
        } while (iVar8 < 1);

        iVar8 = 0;
        //puVar9 = StaticVariables.DAT_80146f5c + StaticVariables.g_drawModes[0x14].tag * 0x28);
        iVar4 = 0;

        do
        {
            sprite = StaticVariables.g_spriteInventoryText[index * 2 + iVar8];

            //uVar5 = (uint)pSVar7 & 0xffffff;
            //pSVar7 = pSVar7 + 1;
            //puVar3 = StaticVariables.g_spriteInventoryText[uVar2 + index * 2].tag + iVar4);

            /* Probable PsyQ macro: addPrim(). */
            //*puVar3 = *puVar3 & 0xff000000 | *puVar9 & 0xffffff;
            //*puVar9 = *puVar9 & 0xff000000 | uVar5;

            var bitmap = _gameEngine.Font3.GenerateFontBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

            iVar4 = iVar4 + 1;
            iVar8 = iVar8 + 1;
        } while (iVar8 < 1);
    }

    //80055f48
    private void FUN_80055f48(int index, int offset, char c)
    {
        int iVar1;
        int i;

        if (StaticVariables.INT_8017fef0 == 0)
        {
            i = 0;
            StaticVariables.INT_8017fef0 = 2;
            StaticVariables.INT_8017feec = StaticVariables.INT_8017feec + 1;
            iVar1 = index * 0x28;

            do
            {
                StaticVariables.g_spriteInventoryText[i].w = 
                    (short)(StaticVariables.g_spriteInventoryText[i].w + StaticVariables.g_fontCharWidthTable[(c & 0xff) * 5]);
                StaticVariables.g_spriteInventoryText[i + 1].w =
                    (short)(StaticVariables.g_spriteInventoryText[i + 1].w + StaticVariables.g_fontCharWidthTable[(c & 0xff) * 5]);
                i = i + 1;

            } while (i < 1);
        }
        else
        {
            StaticVariables.INT_8017fef0 = StaticVariables.INT_8017fef0 - 1;
        }
    }

    //800562dc
    private void FUN_800562dc()
    {
        short sVar1;
        ulong uVar2;
        ulong uVar3;
        SPRT pSVar4;
        int iVar5;
        uint uVar6;
        uint puVar7;
        uint puVar8;
        int x;
        int iVar10;
        int iVar11;
        SPRT sprite;
        //DISPENV dispEnv;

        //GetDispEnv(&dispEnv);
        x = (int)StaticVariables.g_textTilesConfiguration_800b8eb0.X;

        if (0x13f < x)
        {
            x = 0x13f;
        }

        var w = StaticVariables.g_textTilesConfiguration_800b8eb0.Width * 8;

        if (0x13f < x + w)
        {
            w = 0x140 - x;
        }

        //dispEnv.disp.w = (short)w;
        //dispEnv.disp.y = dispEnv.disp.y + g_textTilesConfiguration_800b8eb0.y;
        //dispEnv.disp.x = dispEnv.disp.x + (short)iVar9;
        //dispEnv.disp.h =(TextTilesConfiguration_800b9a00.y - g_textTilesConfiguration_800b8eb0.y) + TextTilesConfiguration_800b9a00.height * 8;
        //SetDrawArea((DR_AREA*)(&UNK_8017fa84 + g_drawModes[0x14].tag * 0xc), &dispEnv.disp);
        //SetDrawArea((DR_AREA*)(&DAT_8017fa9c + g_drawModes[0x14].tag * 0xc), &dispEnv.disp);
        uVar2 = 0; //StaticVariables.g_drawModes[0x14].tag;
        var i = 0;

        do
        {
            StaticVariables.g_ItemNameSprites[i].x0 = (short)(StaticVariables.g_textTilesConfiguration_800b8eb0.X + 0x10);
            StaticVariables.g_ItemNameSprites[i].y0 = (short)(StaticVariables.g_textTilesConfiguration_800b8eb0.Y + -i + 8);
            StaticVariables.g_ItemNameSprites[i + 2].x0 = (short)(StaticVariables.TextTilesConfiguration_800b9a00.X + 0x10);
            StaticVariables.g_ItemNameSprites[i + 2].y0 = (short)(StaticVariables.TextTilesConfiguration_800b9a00.Y + -i + 8);
            //uVar3 = StaticVariables.g_drawModes[0x14].tag;
            i = i + 1;
        } while (i < 1);

        i = 0;
        //iVar10 = -0x7fe805a4; StaticVariables.g_ItemNameSprites[2]
        //puVar8 = StaticVariables.DAT_80146f60 + StaticVariables.g_drawModes[0x14].tag * 0x28);

        do
        {
            //sprite = StaticVariables.g_ItemNameSprites[i];
            //puVar7 = StaticVariables.g_ItemNameSprites[0]. + i;
            /* Probable PsyQ macro: addPrim(). */
            //pSVar4 = pSVar12 + uVar3;
            //*puVar7 = *puVar7 & 0xff000000 | *puVar8 & 0xffffff;

            //puVar7 = StaticVariables.g_ItemNameSprites[2]. + i;
            //*puVar8 = *puVar8 & 0xff000000 | (uint)pSVar4 & 0xffffff;
            //*puVar7 = *puVar7 & 0xff000000 | (uint)pSVar4 & 0xffffff;
            //*puVar8 = *puVar8 & 0xff000000 | uVar6 & 0xffffff;

            i = i + 1;

        } while (i < 1);

        for (i = 0; i < 2; i++)
        {
            sprite = StaticVariables.g_ItemNameSprites[i];
            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);
        }

        //TODO : DAT_8017fa9c sprite ??

        //puVar7 = StaticVariables.DAT_8017fa9c + g_drawModes[0x14].tag * 0xc);
        //puVar8 = StaticVariables.DAT_80146f60 + g_drawModes[0x14].tag * 0x28);
        /* Probable PsyQ macro: addPrim(). */
        //*puVar7 = *puVar7 & 0xff000000 | *puVar8 & 0xffffff;
        //*puVar8 = *puVar8 & 0xff000000 | (uint)puVar7 & 0xffffff;
    }

    //80055d78
    private void FUN_80055d78(TextTilesConfiguration textTileConfig)
    {
        ulong uVar1;
        SPRT sprite;
        uint puVar3;
        int w;
        int h = 0;

        //uVar1 = StaticVariables.g_drawModes[0x14].tag;

        if (0 < textTileConfig.Height)
        {
            do
            {
                w = 0;

                if (0 < textTileConfig.Width)
                {
                    //puVar3 = StaticVariables.g_drawModes + uVar1 * 0x28 + 0xf8;
                    do
                    {
                        sprite = textTileConfig.SpritesA[h * textTileConfig.Width + w];
                        w = w + 1;

                        /* Probable PsyQ macro: addPrim(). */
                        //pSVar2->tag = pSVar2->tag & 0xff000000 | *puVar3 & 0xffffff;
                        //*puVar3 = *puVar3 & 0xff000000 | (uint)pSVar2 & 0xffffff;
                        //pSVar2 = pSVar2 + 1;

                        //TODO : display something ?
                        var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                        _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

                    } while (w < textTileConfig.Width);
                }

                h = h + 1;

            } while (h < textTileConfig.Height);
        }
    }

    //80056fb4
    private void FUN_80056fb4()
    {
        ulong uVar1;
        uint uVar2;
        uint puVar3;
        SPRT pSVar4;
        uint puVar5;
        int iVar6;
        int index;
        int offset;
        uint textureId;
        TextToDisplay textToDisplay;
        int i;
        int local_40;
        int local_3c;
        int local_38;

        i = 0;
        textToDisplay = StaticVariables.TextToDisplay_ARRAY_8017f920[7];
        offset = 7;

        do
        {
            index = offset + i;

            //StaticVariables.TextToDisplay_ARRAY_8017f920[index]. = 0x80;
            //StaticVariables.TextToDisplay_ARRAY_8017f920[index]. = 0x80;
            //StaticVariables.TextToDisplay_ARRAY_8017f920[index]. = 0x80;
            //StaticVariables.TextToDisplay_ARRAY_8017f920[index].0xd0 = 0x30;
            //StaticVariables.TextToDisplay_ARRAY_8017f920[index].0xd1 = 0x98;
                                                           
            StaticVariables.TextToDisplay_ARRAY_8017f920[index].startX = 0x18; //0xd4
            StaticVariables.TextToDisplay_ARRAY_8017f920[index].startY = 0x20; //0xd6

            var clutIndexFromTable = StaticVariables.BYTE_ARRAY_8009cfd8[2] | (StaticVariables.BYTE_ARRAY_8009cfd8[3] << 8);
            var clutValue = StaticVariables.g_clutTable[clutIndexFromTable];
            StaticVariables.TextToDisplay_ARRAY_8017f920[i].mode = clutValue;

            //SetSprt((SPRT*)(g_drawModes[0x14].tag * 0x28 + iVar9));
            //SetSemiTrans((void*)(iVar6 + iVar9), 0);
            //SetShadeTex((void*)(g_drawModes[0x14].tag * 0x28 + iVar9), 1);

            i = i + 1;
        } while (i < 2);

        local_40 = 0;
        local_38 = 0;

        do
        {
            local_3c = 0;
            offset = local_38;

            do
            {
                if (StaticVariables.UINT_ARRAY_800b9ec8[offset] != 0)
                {
                    if (local_40 == 1 && local_3c == 0)
                    {
                        //special case herbs : number of item is displayed
                        var numOfItem = _gameEngine.PlayerManager.GetNumberOfItem(0x24); 

                        if (numOfItem != 0)
                        {
                            var sprite = StaticVariables.g_spriteInventoryItems[6];

                            InitializeSpriteWithImage(sprite, 0x24,
                                       (short)(StaticVariables.TextTilesConfiguration_800b58a8.X + StaticVariables.UINT_ARRAY_800b9f28[6]),
                                       (short)(StaticVariables.TextTilesConfiguration_800b58a8.Y + StaticVariables.UINT_ARRAY_800b9f88[6]));

                            i = (int)_gameEngine.PlayerManager.SetItemIdFromCurrentItemId();

                            sprite.r0 = 0x90;
                            sprite.g0 = 0x90;
                            sprite.b0 = 0x90;

                            if (i == 0x24) //herbs
                            {
                                //puVar5 = StaticVariables.TextToDisplay_ARRAY_8017f920[7];
                                StaticVariables.TextToDisplay_ARRAY_8017f920[7].x = (short)(StaticVariables.TextTilesConfiguration_800b58a8.X + StaticVariables.UINT_ARRAY_800b9f28[6]);
                                StaticVariables.TextToDisplay_ARRAY_8017f920[7].y = (short)(StaticVariables.TextTilesConfiguration_800b58a8.Y + StaticVariables.UINT_ARRAY_800b9f88[6]);
                                //puVar3 = (uint*)((int)g_drawModes + i + 0xf8);
                                /* Probable PsyQ macro: addPrim(). */
                                //*puVar5 = *puVar5 & 0xff000000 | *puVar3 & 0xffffff;
                                //*puVar3 = *puVar3 & 0xff000000 | (uint)puVar5 & 0xffffff;

                                var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                                _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);
                            }

                            //uVar1 = g_drawModes[0x14].tag;
                            sprite = StaticVariables.SPRT_ARRAY_8017fe74[0];
                            StaticVariables.SPRT_ARRAY_8017fe74[0].x0 = (short)(StaticVariables.TextTilesConfiguration_800b58a8.X + StaticVariables.UINT_ARRAY_800b9f28[6] + 0x10);
                            StaticVariables.SPRT_ARRAY_8017fe74[0].y0 = (short)(StaticVariables.TextTilesConfiguration_800b58a8.Y + StaticVariables.UINT_ARRAY_800b9f88[6] + 0x10);
                            //SetSprt(pSVar4);
                            StaticVariables.SPRT_ARRAY_8017fe74[0].r0 = 0x90;
                            StaticVariables.SPRT_ARRAY_8017fe74[0].g0 = 0x90;
                            StaticVariables.SPRT_ARRAY_8017fe74[0].b0 = 0x90;
                            i = i % 10 * 0x14;
                            StaticVariables.SPRT_ARRAY_8017fe74[0].u0 = StaticVariables.BYTE_ARRAY_8009cfd8[i];
                            StaticVariables.SPRT_ARRAY_8017fe74[0].v0 = StaticVariables.BYTE_ARRAY_8009cfd8[i + 1];

                            var bitmap2 = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                            _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap2);

                            //uVar1 = g_drawModes[0x14].tag;
                            //pSVar4 = StaticVariables.SPRT_ARRAY_8017fe74 + g_drawModes[0x14].tag;
                            //puVar3 = StaticVariables.DAT_80146f6c + g_drawModes[0x14].tag * 0x28);
                            //puVar5 = StaticVariables.DAT_80146f68 + g_drawModes[0x14].tag * 0x28);
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
                        textureId = StaticVariables.UINT_ARRAY_800b9ec8[offset];

                        if (textureId == 0xffffffff)
                        {
                            textureId = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();
                        }
                        else
                        {
                            i = _gameEngine.PlayerManager.GetNumberOfItem(offset);

                            if (i == 0)
                            {
                                textureId = 0xffffffff;
                            }
                        }

                        if (textureId != 0xffffffff)
                        {
                            var sprite = StaticVariables.g_spriteInventoryItems[offset];

                            InitializeSpriteWithImage(sprite,
                                       (int)textureId,
                                       (short)(StaticVariables.TextTilesConfiguration_800b58a8.X + StaticVariables.UINT_ARRAY_800b9f28[offset]),
                                       (short)(StaticVariables.TextTilesConfiguration_800b58a8.Y + StaticVariables.UINT_ARRAY_800b9f88[offset]));

                            uVar2 = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();

                            if (local_40 == 0 && textureId == uVar2)
                            {
                                //textToDisplay = StaticVariables.TextToDisplay_ARRAY_8017f920[7];
                                StaticVariables.TextToDisplay_ARRAY_8017f920[7].x = (short)(StaticVariables.TextTilesConfiguration_800b58a8.X + StaticVariables.UINT_ARRAY_800b9f28[local_3c]);
                                StaticVariables.TextToDisplay_ARRAY_8017f920[7].y = (short)(StaticVariables.TextTilesConfiguration_800b58a8.Y + StaticVariables.UINT_ARRAY_800b9f88[local_3c]);
                                //puVar3 = (uint*)((int)g_drawModes + i + 0xf8);
                                /* Probable PsyQ macro: addPrim(). */
                                //*puVar5 = *puVar5 & 0xff000000 | *puVar3 & 0xffffff;
                                //*puVar3 = *puVar3 & 0xff000000 | (uint)puVar5 & 0xffffff;

                                var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                                _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);
                            }

                            i = offset << 2;

                            if (0 < local_40)
                            {
                                uVar2 = (uint)_gameEngine.PlayerManager.SetItemIdFromCurrentItemId();
                                i = offset; // * 4;

                                if (textureId == uVar2)
                                {
                                    //i = g_drawModes[0x14].tag * 0x28;
                                    //textToDisplay = StaticVariables.TextToDisplay_ARRAY_8017f920[7];
                                    StaticVariables.TextToDisplay_ARRAY_8017f920[7].x = (short)(StaticVariables.TextTilesConfiguration_800b58a8.X + StaticVariables.UINT_ARRAY_800b9f28[offset]);
                                    StaticVariables.TextToDisplay_ARRAY_8017f920[7].y = (short)(StaticVariables.TextTilesConfiguration_800b58a8.Y + StaticVariables.UINT_ARRAY_800b9f88[offset]);
                                    //puVar5 = StaticVariables.g_drawModes + i + 0xf8);
                                    /* Probable PsyQ macro: addPrim(). */
                                    //*puVar3 = *puVar3 & 0xff000000 | *puVar5 & 0xffffff;
                                    //*puVar5 = *puVar5 & 0xff000000 | (uint)puVar3 & 0xffffff;

                                    var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                                    _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

                                    i = offset << 2;
                                }
                            }

                            i = (i + offset); // * 4;

                            sprite.r0 = 0x90;
                            sprite.g0 = 0x90;
                            sprite.b0 = 0x90;

                            var bitmap3 = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                            _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap3);

                            //i = StaticVariables.g_drawModes[0x14].tag * 0x1e0;
                            //puVar3 = StaticVariables.DAT_80146f68 + g_drawModes[0x14].tag * 0x28);
                            /* Probable PsyQ macro: addPrim(). */
                            //*(uint*)(&DAT_8017fab4 + i + i) = *(uint*)(&DAT_8017fab4 + textTioDisplay + i) & 0xff000000 | *puVar3 & 0xffffff;
                            //*puVar3 = *puVar3 & 0xff000000 | (uint)(&DAT_8017fab4 + i + i) & 0xffffff;
                        }
                    }
                }

                offset = offset + 1;
                local_3c = local_3c + 1;

            } while (local_3c < 6);

            local_38 = local_38 + 6;
            local_40 = local_40 + 1;

        } while (local_40 < 4);
    }

    //8004da0c
    private void InitializeSpriteWithImage(SPRT sprt, int textureId, short x, short y)
    {
        int index;
        SiImage image;

        if (textureId != -1)
        {
            sprt.x0 = x;
            sprt.y0 = y;
            index = _gameEngine.GraphicManager.GetItemTextureIdByItemId(textureId);
            image = _gameEngine.GraphicManager.GetAnimationImageByIndex(index);
            sprt.r0 = 0x80;
            sprt.g0 = 0x80;
            sprt.b0 = 0x80;
            sprt.u0 = image.Sx;
            sprt.v0 = image.Sy;
            sprt.w = image.Swidth;
            sprt.h = image.Sheight;
            //sprt.clut = StaticVariables.g_clutTableBase[image.Palette]; //why ? => number bigger than 30000
            //SetSprt(sprt);
        }
    }

    //80056a98
    private void FUN_80056a98()
    {
        int value;
        int iVar2;
        int iVar8;
        int i;
        short sVar10;
        int divisor;
        SPRT sprite;

        divisor = 1000;
        value = _gameEngine.PlayerManager.GetMoney();
        i = 0;
        sVar10 = 0x18;

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

            iVar2 = value / divisor % 10 * 0x14;
            sprite.u0 = StaticVariables.BYTE_ARRAY_8009cfd8[iVar2];
            sprite.v0 = StaticVariables.BYTE_ARRAY_8009cfd8[iVar2 + 1];
            sprite.x0 = (short)(StaticVariables.TextTilesConfiguration_800b9e58.X + sVar10);
            sprite.y0 = (short)(StaticVariables.TextTilesConfiguration_800b9e58.Y + 4);

            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

            //puVar5 = (StaticVariables.sprite.tag + iVar8);;
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

            iVar2 = value / divisor % 10 * 0x14;
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

            iVar2 = value / divisor % 10 * 0x14;
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

        var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(cursorAnim.Sprites[0]);
        //Debugger.Break();
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
                iVar2 = _gameEngine.PlayerManager.GetWeaponIdFromSlot1();
                iVar1 = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();

                if (iVar1 == iVar2)
                {
                    DisplayWarpNames();
                    return;
                }

                uVar3 = 1;
                goto joined_r0x800579e0;

            case 1:
                iVar1 = _gameEngine.PlayerManager.GetWeaponIdFromSlot3();
                iVar2 = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();

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
                iVar2 = _gameEngine.PlayerManager.GetWeaponIdFromSlot2();
                iVar1 = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();

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
                iVar1 = _gameEngine.PlayerManager.GetWeaponIdFromSlot4();
                iVar2 = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();

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
                iVar1 = _gameEngine.PlayerManager.GetWeaponIdFromSlot5();
                iVar2 = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();

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
                iVar2 = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();

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

        currentTileIndex = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon(); //StaticVariables.g_iconNameEtcBase[currentTileIndex * 2]

        if (currentTileIndex != 0xffffffff)
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

        currentTileIndex = _gameEngine.PlayerManager.SetItemIdFromCurrentItemId();

        if (currentTileIndex == 0xffffffff)
        {
            sourceWarpName = "       ";
        }
        else
        {
            sourceWarpName = _gameEngine.EtcResR.GetIconName((int)currentTileIndex);// StaticVariables.g_iconNameEtcBase[currentTileIndex * 2];
        }

        SPRT[] sprites = [StaticVariables.g_ItemNameSprites[2], StaticVariables.g_ItemNameSprites[3], StaticVariables.g_ItemNameSprites[4], StaticVariables.g_ItemNameSprites[5]];
        _gameEngine.GraphicManager.DisplayIconName(sprites,
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
                    1 => _gameEngine.PlayerManager.GetWeaponIdFromSlot1(),
                    2 => _gameEngine.PlayerManager.GetWeaponIdFromSlot3(),
                    3 => _gameEngine.PlayerManager.GetWeaponIdFromSlot2(),
                    4 => _gameEngine.PlayerManager.GetWeaponIdFromSlot4(),
                    5 => _gameEngine.PlayerManager.GetWeaponIdFromSlot5(),
                    _ => slotId
                };

                if (slotId == 0xffffffff)
                {
                    LAB_80057938:
                    _gameEngine.SoundManager.PlaySoundEffect(3);
                    return;
                }

                currentItemId = (uint)_gameEngine.PlayerManager.SetItemIdFromCurrentItemId();

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

                currentItemId = (uint)_gameEngine.PlayerManager.SetItemIdFromCurrentItemId();
                slotId = StaticVariables.UINT_ARRAY_800b9ec8[StaticVariables.g_inventorySelectedSlotId];

                if (currentItemId == slotId)
                {
                    return;
                }
            }

            _gameEngine.PlayerManager.SetCurrentItemId(slotId);
            _gameEngine.SoundManager.PlaySoundEffect(2);
        }

        DisplayWarpNames();
        FUN_8005ac90();
    }

    //8005ac90
    private void FUN_8005ac90()
    {
        uint iVar1;
        int iVar2;

        if (StaticVariables.g_isCdResetRequested != 0
            || (StaticVariables.g_cdIsReady != 0 && StaticVariables.g_cdDataLoaded == 0))
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