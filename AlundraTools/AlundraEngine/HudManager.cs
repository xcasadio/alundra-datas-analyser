using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay;

namespace AlundraEngine;

public class HudManager
{
    private readonly GameEngine _gameEngine;

    public HudManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
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
            StaticVariables.g_hudTransitionSrcX = srcX;
            StaticVariables.g_hudTransitionSrcY = srcY;
            StaticVariables.g_hudTransitionSrcZ = srcZ;
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

                i = i + 1;
            } while (i < 2);

            //GraphicManager.DrawPolyFt4(0xff, 0xff, 0xff,
            //    100, 100, 
            //    width + 100, 
            //    100, 100, 
            //    height + 100, 
            //    width + 100, 
            //    height + 100, image);

            StaticVariables.g_hudDeltaX = -(StaticVariables.g_hudTransitionSrcX + 2)
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

        cameraX = (short)(((StaticVariables.g_hudTransitionStepValue * StaticVariables.g_hudX) / 0xf) + cameraX);
        cameraY = (short)(((StaticVariables.g_hudTransitionStepValue * StaticVariables.g_hudY) / 0xf) + cameraY);

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
                alphaValue = (char)(((0xf - StaticVariables.g_hudTransitionStepValue) * 0x80) / 0xf);

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
            alphaValue = (char)((StaticVariables.g_hudTransitionStepValue * 0x80) / 0xf);
            ComputeOffsets:
            uVar4 = (char)(alphaValue + '\x7f');
            offsetX = (short)((short)(divisionHalfWidth + iVar5 >> 3) - (short)(iVar5 >> 0x1f));
            offsetY = (short)((short)(divisionHalfHeight + scaledHalfHeight >> 3) - (short)(scaledHalfHeight >> 0x1f));
        }

        StaticVariables.g_hudTransitionStepValue = StaticVariables.g_hudTransitionStepValue + -1;

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
}