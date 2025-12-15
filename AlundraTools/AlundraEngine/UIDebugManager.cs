using AlundraEngine.Gameplay;
using AlundraEngine.Graphics;
using AlundraEngine.UI;
using System.Diagnostics;

namespace AlundraEngine;

public class UIDebugManager
{
    private readonly GameEngine _gameEngine;

    public UIDebugManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }


    //80051624
    public void DisplayFlagsDebugMenu(CallBackInfo callBackInfo)
    {
        short psVar1;
        ulong uVar2;
        int iVar3;
        uint[] flags;
        int puVar5;
        int puVar6;
        int puVar7;
        uint puVar8;
        SPRT sprite;
        char[] numbers;
        SPRT sprite2;
        int puVar10;
        int i;
        int[] unities = new int[6];
        int[] localNumbers = new int[10];
        char[] buffer = new char[80];
        //DISPENV dispEnv;

        unities[0] = 1;
        unities[1] = 10;
        unities[2] = 100;
        unities[3] = 1000;
        unities[4] = 10000;
        string[] g_numberStringArray = Enumerable.Range(0, 10).Select(i => i.ToString()).ToArray();

        //do
        //{
        //    puVar10 = localNumbers2;
        //    numbers = numberStringArray;
        //    puVar5 = numbers[1];
        //    puVar6 = numbers[2];
        //    puVar7 = numbers[3];
        //    *puVar10 = *numbers;
        //    puVar10[1] = puVar5;
        //    puVar10[2] = puVar6;
        //    puVar10[3] = puVar7;
        //    numberStringArray = numbers + 4;
        //    localNumbers2 = puVar10 + 4;
        //} while (numbers + 4 != &PTR_s_8_80026828);
        //
        //puVar5 = numbers[5];
        //puVar10[4] = "8";
        //puVar10[5] = puVar5;

        //GetDispEnv(&dispEnv);
        //dispEnv.disp.x = dispEnv.disp.x + *(short*)(callBackInfo._0 + 8) + **(short**)(callBackInfo._0 + 4);
        //dispEnv.disp.y = dispEnv.disp.y + *(short*)(callBackInfo._0 + 10) + *(short*)(*(int*)(callBackInfo._0 + 4) + 2);
        //dispEnv.disp.w = *(short*)(callBackInfo._0 + 0xc) * 8 + 2;
        //dispEnv.disp.h = *(short*)(callBackInfo._0 + 0xe) * 8 + 2;
        //SetDrawArea(DR_AREA_ARRAY_80153010 + g_drawModes[0x14].tag, &dispEnv.disp);

        if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Triangle) != 0)
        {
            _gameEngine.StaticVariables.DAT_8017e9ac = (byte)((_gameEngine.StaticVariables.DAT_8017e9ac + 1) & 1);
            _gameEngine.SoundManager.PlaySoundEffect(1);
        }

        if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Cross) == 0)
        {
            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Circle) == 0)
            {
                if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Right) != 0)
                {
                    _gameEngine.StaticVariables.SHORT_8017e8dc = (short)(_gameEngine.StaticVariables.SHORT_8017e8dc - 1);

                    if (_gameEngine.StaticVariables.SHORT_8017e8dc << 0x10 < 0)
                    {
                        _gameEngine.StaticVariables.SHORT_8017e8dc = 0;
                    }
                    else
                    {
                        _gameEngine.SoundManager.PlaySoundEffect(1);
                    }
                }

                if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Left) != 0)
                {
                    _gameEngine.StaticVariables.SHORT_8017e8dc = (short)(_gameEngine.StaticVariables.SHORT_8017e8dc + 1);

                    if (_gameEngine.StaticVariables.SHORT_8017e8dc < 5)
                    {
                        _gameEngine.SoundManager.PlaySoundEffect(1);
                    }
                    else
                    {
                        _gameEngine.StaticVariables.SHORT_8017e8dc = 4;
                    }
                }

                if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Up) != 0)
                {
                    _gameEngine.StaticVariables.SHORT_ARRAY_8017e8de[4 - _gameEngine.StaticVariables.SHORT_8017e8dc] =
                        (short)(_gameEngine.StaticVariables.SHORT_ARRAY_8017e8de[4 - _gameEngine.StaticVariables.SHORT_8017e8dc] + 1);

                    if (_gameEngine.StaticVariables.SHORT_ARRAY_8017e8de[4 - _gameEngine.StaticVariables.SHORT_8017e8dc] < 10)
                    {
                        _gameEngine.SoundManager.PlaySoundEffect(1);
                    }
                    else
                    {
                        _gameEngine.StaticVariables.SHORT_ARRAY_8017e8de[4 - _gameEngine.StaticVariables.SHORT_8017e8dc] = 0;
                    }
                }

                if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Down) != 0)
                {
                    _gameEngine.StaticVariables.SHORT_ARRAY_8017e8de[4 - _gameEngine.StaticVariables.SHORT_8017e8dc] =
                        (short)(_gameEngine.StaticVariables.SHORT_ARRAY_8017e8de[4 - _gameEngine.StaticVariables.SHORT_8017e8dc] + -1);

                    if (_gameEngine.StaticVariables.SHORT_ARRAY_8017e8de[4 - _gameEngine.StaticVariables.SHORT_8017e8dc] < 0)
                    {
                        _gameEngine.StaticVariables.SHORT_ARRAY_8017e8de[4 - _gameEngine.StaticVariables.SHORT_8017e8dc] = 9;
                    }
                    else
                    {
                        _gameEngine.SoundManager.PlaySoundEffect(1);
                    }
                }

                _gameEngine.StaticVariables.UINT_8017e8d8 = 0;
                i = 0;

                do
                {
                    iVar3 = 4 - i;
                    if (_gameEngine.StaticVariables != null)
                    {
                        _gameEngine.StaticVariables.UINT_8017e8d8 +=
                            (uint)(_gameEngine.StaticVariables.SHORT_ARRAY_8017e8de[iVar3] * unities[i]);
                    }

                    i += 1;
                } while (i < 5);

                buffer[0] = '\0';
                i = 0;
                puVar8 = _gameEngine.StaticVariables.UINT_8017e8d8;

                Debugger.Break();

                do
                {
                    //psVar1 = (short*)((int)puVar8 + 6);
                    //puVar8 = (uint*)((int)puVar8 + 2);
                    i += 1;
                    //strcat(buffer, localNumbers[*psVar1]);
                } while (i < 5);


                _gameEngine.UIManager.DisplayIconName(
                    _gameEngine.StaticVariables.SPRT_ARRAY_8017e8e8,
                    _gameEngine.SubInventoryManager.InventoryArmoryNameSprites,
                    buffer,
                    5,
                    0, //(short)(callBackInfo.Data.X + callBackInfo.Data.Width + 100),
                    0, //(short)(callBackInfo.Data.Y + callBackInfo.Data.Height + 0x10),
                    0);

                //uVar2 = g_drawModes[0x14].tag;
                sprite2 = _gameEngine.StaticVariables.SPRT_ARRAY_8017e8e8[0];
                //puVar8 = _gameEngine.StaticVariables.DAT_80146f60[0];
                sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017e8e8[1];
                //pSVar9.tag = pSVar9.tag & 0xff000000 | *puVar8 & 0xffffff;
                //*puVar8 = *puVar8 & 0xff000000 | (uint)pSVar9 & 0xffffff;
                //sprite.tag = sprite.tag & 0xff000000 | (uint)pSVar9 & 0xffffff;
                //*puVar8 = *puVar8 & 0xff000000 | (uint)sprite & 0xffffff;

                Debugger.Break();
                var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

                var name = "";

                if (_gameEngine.StaticVariables.DAT_8017e9ac == 0)
                {
                    //Array.Copy(_gameEngine.StaticVariables.CHAR_ARRAY_8017e998, "Global", 7);
                    name = "Global";
                }
                else
                {
                    //Array.Copy(_gameEngine.StaticVariables.CHAR_ARRAY_8017e998, "Local", 6);
                    name = "Local";
                }

                _gameEngine.UIManager.DisplayIconName(
                    _gameEngine.StaticVariables.SPRT_ARRAY_8017e938,
                    _gameEngine.SubInventoryManager.InventoryArmoryNameSprites,
                    name.ToCharArray(),//_gameEngine.StaticVariables.CHAR_ARRAY_8017e998, 
                    5,
                    0, //(short)(callBackInfo.Data.X + callBackInfo.Data.Width + 0x10),
                    0, //(short)(callBackInfo.Data.Y + callBackInfo.Data.Height + 0x10),
                    1);

                //uVar2 = g_drawModes[0x14].tag;
                sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017e938[0];
                sprite2 = _gameEngine.StaticVariables.SPRT_ARRAY_8017e938[1];
                //puVar8 = (uint*)(&DAT_80146f60 + g_drawModes[0x14].tag * 0x28);
                //sprite2.tag = sprite2.tag & 0xff000000 | *puVar8 & 0xffffff;
                //*puVar8 = *puVar8 & 0xff000000 | (uint)sprite2 & 0xffffff;
                //sprite.tag = sprite.tag & 0xff000000 | (uint)sprite2 & 0xffffff;
                //*puVar8 = *puVar8 & 0xff000000 | (uint)sprite & 0xffffff;

                Debugger.Break();
                bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

                puVar10 = 0; //localNumbers[0];

                if (((_gameEngine.StaticVariables.UINT_8017e8d8 & 0xffff | (uint)_gameEngine.StaticVariables.DAT_8017e9ac << 0xf) & 0x8000) == 0)
                {
                    flags = _gameEngine.StaticVariables.g_saveData.MapFlags;
                }
                else
                {
                    flags = _gameEngine.StaticVariables.g_globalFlags;
                }

                var index = (_gameEngine.StaticVariables.UINT_8017e8d8 & 0x7fe0) >> 3;
                Debugger.Break();
                //index >>= 2; ??
                var flagValue = flags[index];
                var bitMask = 1 << (int)(_gameEngine.StaticVariables.UINT_8017e8d8 & 0x1f);
                if ((flagValue & bitMask) != 0)
                {
                    puVar10 = 1; //localNumbers[1];
                }

                SPRT[] sprites = [_gameEngine.StaticVariables.SPRT_ARRAY_8017e8e8[2], _gameEngine.StaticVariables.SPRT_ARRAY_8017e8e8[3]];

                _gameEngine.UIManager.DisplayIconName(
                    sprites,
                    _gameEngine.SubInventoryManager.InventoryArmoryNameSprites,
                    puVar10.ToString().ToCharArray(),
                    5,
                    0, //(short)(callBackInfo.Data.X + callBackInfo.Data.Width + 200),
                    0, //    (short)(callBackInfo.Data.Y + callBackInfo.Data.Height + 0x10),
                    2);

                //uVar2 = g_drawModes[0x14].tag;
                sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017e8e8[2];
                //puVar8 = (uint*)(&DAT_80146f60 + g_drawModes[0x14].tag * 0x28);
                sprite2 = _gameEngine.StaticVariables.SPRT_ARRAY_8017e8e8[3];
                //sprite.tag = sprite.tag & 0xff000000 | *puVar8 & 0xffffff;
                //*puVar8 = *puVar8 & 0xff000000 | (uint)sprite & 0xffffff;
                //sprite2.tag = sprite2.tag & 0xff000000 | (uint)sprite & 0xffffff;
                //*puVar8 = *puVar8 & 0xff000000 | (uint)sprite2 & 0xffffff;

                _gameEngine.GraphicManager.ApplyFadeTransform(sprites,
                    (short)(_gameEngine.StaticVariables.SPRT_ARRAY_8017e8e8[0].x0 - (_gameEngine.StaticVariables.SHORT_8017e8dc * 0x10 + -0x40)),
                    (short)(_gameEngine.StaticVariables.SPRT_ARRAY_8017e8e8[0].y0 + -0x10),
                    0);

                _gameEngine.UIManager.FUN_800507e4(sprites);
            }
            else
            {
                _gameEngine.StaticVariables.UINT_8017e8d8 = 0;
                i = 0;

                do
                {
                    iVar3 = 4 - i;
                    _gameEngine.StaticVariables.UINT_8017e8d8 =
                        (uint)(_gameEngine.StaticVariables.UINT_8017e8d8 + _gameEngine.StaticVariables.SHORT_ARRAY_8017e8de[iVar3] * unities[i]);
                    i += 1;
                } while (i < 5);

                var value = (_gameEngine.StaticVariables.UINT_8017e8d8 & 0xffff) | (uint)(_gameEngine.StaticVariables.DAT_8017e9ac << 0xf);

                if ((value & 0x8000) == 0)
                {
                    flags = _gameEngine.StaticVariables.g_saveData.MapFlags;
                }
                else
                {
                    flags = _gameEngine.StaticVariables.g_globalFlags;
                }

                var index = (_gameEngine.StaticVariables.UINT_8017e8d8 & 0x7fe0) >> 3;
                Debugger.Break();
                //index >>= 2;
                flags[index] = (uint)(flags[index] ^ (1 << (int)(_gameEngine.StaticVariables.UINT_8017e8d8 & 0x1f)));

                if ((_gameEngine.StaticVariables.DAT_8017e990 & 1) != 0)
                {
                    _gameEngine.SoundManager.PlaySoundEffect(5);
                    _gameEngine.UIManager.FUN_80047cb0(callBackInfo);
                }
            }
        }
        else
        {
            _gameEngine.StaticVariables.UINT_8017e8d8 = 0;
            i = 0;

            do
            {
                iVar3 = 4 - i;
                _gameEngine.StaticVariables.UINT_8017e8d8 =
                    (uint)(_gameEngine.StaticVariables.UINT_8017e8d8 + _gameEngine.StaticVariables.SHORT_ARRAY_8017e8de[iVar3] * unities[i]);
                i += 1;
            } while (i < 5);

            _gameEngine.SoundManager.PlaySoundEffect(5);
            _gameEngine.StaticVariables.g_playerControlFlags &= 0xfffffff7;
            _gameEngine.UIManager.FUN_80047cb0(callBackInfo);
        }
    }
}