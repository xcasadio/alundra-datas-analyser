using AlundraEngine.Graphics;
using AlundraEngine.UI;
using OfficeOpenXml.DataValidation.Exceptions;
using System;
using System.Diagnostics;

namespace AlundraEngine;

public class SubInventoryManager
{
    private readonly GameEngine _gameEngine;

    public readonly List<Renderer.Sprite> InventoryWeaponNameSprites = new();
    public readonly List<Renderer.Sprite>[] InventoryWeaponDescriptionLinesSprites = [new(), new()];
    public readonly List<Renderer.Sprite> InventoryItemNameSprites = new();

    public SubInventoryManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    //800537f0
    public void InitializeSubInventory(CallBackInfo callBackInfo)
    {
        _gameEngine.StaticVariables.INT_8017f340 = 5;
        _gameEngine.StaticVariables.INT_8017f788 = 0;
        _gameEngine.StaticVariables.TextToDisplay_8017f344.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_8017f344.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_8017f344.speed = 0xf;
        _gameEngine.StaticVariables.g_playerControlFlags |= 8;
        _gameEngine.StaticVariables.TextToDisplay_8017f344.x = (short)~(_gameEngine.StaticVariables.UIBoxConfiguration_800af664.Width << 3);

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800af664.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f344.y =
                (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800af664.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800af664.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f344.y = _gameEngine.StaticVariables.UIBoxConfiguration_800af664.Y;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800af664.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f344.startX =
                (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800af664.X + _gameEngine.StaticVariables.UIBoxConfiguration_800af664.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f344.startX = _gameEngine.StaticVariables.UIBoxConfiguration_800af664.X;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800af664.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f344.startY =
                (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800af664.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800af664.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f344.startY = _gameEngine.StaticVariables.UIBoxConfiguration_800af664.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f360.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_8017f360.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_8017f360.speed = 0xf;
        _gameEngine.StaticVariables.TextToDisplay_8017f360.x = (short)~(ushort)(_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Width << 3);
        _gameEngine.StaticVariables.TextToDisplay_8017f344.originX = _gameEngine.StaticVariables.UIBoxConfiguration_800af664.X;
        _gameEngine.StaticVariables.TextToDisplay_8017f344.originY = _gameEngine.StaticVariables.UIBoxConfiguration_800af664.Y;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f360.y =
                (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f360.y = _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Y;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f360.startX =
                (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.X + _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f360.startX = _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.X;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f360.startY =
                (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f360.startY = _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f37c.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_8017f37c.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_8017f37c.speed = 0xf;
        _gameEngine.StaticVariables.TextToDisplay_8017f37c.x = 0x140;
        _gameEngine.StaticVariables.TextToDisplay_8017f360.originX = _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.X;
        _gameEngine.StaticVariables.TextToDisplay_8017f360.originY = _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Y;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f37c.y =
                (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f37c.y = _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Y;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b122c.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f37c.startX =
                (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b122c.X + _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f37c.startX = _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.X;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f37c.startY =
                (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f37c.startY = _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f398.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_8017f398.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_8017f398.speed = 0xf;
        _gameEngine.StaticVariables.TextToDisplay_8017f398.x = 0x140;
        _gameEngine.StaticVariables.TextToDisplay_8017f37c.originX = _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.X;
        _gameEngine.StaticVariables.TextToDisplay_8017f37c.originY = _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Y;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f398.y =
                (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f398.y = _gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Y;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f398.startX =
                (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.X + _gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f398.startX = _gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.X;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f398.startY =
                (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f398.startY = _gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f3b4.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_8017f3b4.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_8017f3b4.speed = 0xf;
        _gameEngine.StaticVariables.TextToDisplay_8017f3b4.x = 0x140;
        _gameEngine.StaticVariables.TextToDisplay_8017f398.originX = _gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.X;
        _gameEngine.StaticVariables.TextToDisplay_8017f398.originY = _gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Y;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3b4.y =
                (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3b4.y = _gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Y;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b287c.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3b4.startX =
                (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b287c.X + _gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3b4.startX = _gameEngine.StaticVariables.UIBoxConfiguration_800b287c.X;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3b4.startY =
                (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3b4.startY = _gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f3ec.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_8017f3ec.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_8017f3ec.speed = 0xf;
        _gameEngine.StaticVariables.TextToDisplay_8017f3ec.x = 0x140;
        _gameEngine.StaticVariables.TextToDisplay_8017f3b4.originX = _gameEngine.StaticVariables.UIBoxConfiguration_800b287c.X;
        _gameEngine.StaticVariables.TextToDisplay_8017f3b4.originY = _gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Y;

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3ec.y =
                (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Height * -8)
                ;
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3ec.y = _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3ec.startX =
                (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X + _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3ec.startX = _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3ec.startY =
                (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Height * -8)
                ;
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3ec.startY = _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f3d0.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_8017f3d0.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_8017f3d0.speed = 0xf;
        _gameEngine.StaticVariables.TextToDisplay_8017f3ec.originX = _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X;
        _gameEngine.StaticVariables.TextToDisplay_8017f3ec.originY = _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y;

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3d0.x =
                (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X +
                        _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3d0.x = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f3d0.y = 0xf0;

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3d0.startX =
                (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X +
                        _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3d0.startX = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        }

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3d0.startY =
                (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y +
                        _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3d0.startY = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f3d0.originX = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        _gameEngine.StaticVariables.TextToDisplay_8017f3d0.originY = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y;

        FUN_80052f24(0);
        FUN_80052f24(1);
    }

    //80052f24
    private void FUN_80052f24(int param_1)
    {
        int itemId;

        itemId = (int)_gameEngine.PlayerManager.GetItemIdFromSlotId(_gameEngine.StaticVariables.UINT_ARRAY_800b44f0[param_1]);

        if (itemId != -1)
        {
            SPRT[] sprites =
            [
                _gameEngine.StaticVariables.SPRT_ARRAY_8017f480[param_1 * 2],
                _gameEngine.StaticVariables.SPRT_ARRAY_8017f480[param_1 * 2 + 1]
            ];

            var itemName = _gameEngine.EtcResR.GetItemName(itemId);

            _gameEngine.UIManager.DisplayIconName(sprites,
                InventoryItemNameSprites,
                itemName.ToCharArray(),
                0x20, 0x140, 0,
                param_1);
        }
    }

    //80053328
    public void DisplaySubInventory(CallBackInfo callBackInfo)
    {
        int iVar1;

        //DISPENV DStack_40;
        //GetDispEnv(&DStack_40);
        //SetDrawArea((DR_AREA*)(&UNK_8017f610 + g_drawModes[0x14].tag * 0xc), &DStack_40.disp);

        if ((_gameEngine.StaticVariables.INT_8017f340 & 6U) == 0)
        {
            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & 0x2000) != 0)
            {
                _gameEngine.StaticVariables.INT_8017f734 = _gameEngine.StaticVariables.INT_ARRAY_800b43d8[_gameEngine.StaticVariables.INT_8017f734];
                _gameEngine.StaticVariables.INT_8017f788 = 0;
                _gameEngine.SoundManager.PlaySoundEffect(1);
            }

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & 0x8000) != 0)
            {
                _gameEngine.StaticVariables.INT_8017f734 = _gameEngine.StaticVariables.INT_ARRAY_800b4410[_gameEngine.StaticVariables.INT_8017f734];
                _gameEngine.StaticVariables.INT_8017f788 = 0;
                _gameEngine.SoundManager.PlaySoundEffect(1);
            }

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & 0x1000) != 0)
            {
                _gameEngine.StaticVariables.INT_8017f734 = _gameEngine.StaticVariables.INT_ARRAY_800b4448[_gameEngine.StaticVariables.INT_8017f734];
                _gameEngine.StaticVariables.INT_8017f788 = 0;
                _gameEngine.SoundManager.PlaySoundEffect(1);
            }

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & 0x4000) != 0)
            {
                _gameEngine.StaticVariables.INT_8017f734 = _gameEngine.StaticVariables.INT_ARRAY_800b4480[_gameEngine.StaticVariables.INT_8017f734];
                _gameEngine.StaticVariables.INT_8017f788 = 0;
                _gameEngine.SoundManager.PlaySoundEffect(1);
            }

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & 0x813) != 0)
            {
                FUN_800526cc();
                _gameEngine.MainInventoryManager.UpdateHudTransitionState();
                _gameEngine.GraphicManager.PrepareBufferFlip();
            }

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & 0xc) != 0)
            {
                FUN_800526cc();
                _gameEngine.StaticVariables.g_playerControlFlags = _gameEngine.StaticVariables.g_playerControlFlags | 8;
                _gameEngine.MainInventoryManager.UpdateHudTransitionState();
                _gameEngine.StaticVariables.g_postProcessState = 2;
            }
        }
        else
        {
            _gameEngine.UIManager.UpdateUiBoxesPosition(_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc, _gameEngine.StaticVariables.TextToDisplay_8017f360);
            _gameEngine.UIManager.UpdateUiBoxesPosition(_gameEngine.StaticVariables.UIBoxConfiguration_800b122c, _gameEngine.StaticVariables.TextToDisplay_8017f37c);
            _gameEngine.UIManager.UpdateUiBoxesPosition(_gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c, _gameEngine.StaticVariables.TextToDisplay_8017f398);
            _gameEngine.UIManager.UpdateUiBoxesPosition(_gameEngine.StaticVariables.UIBoxConfiguration_800b287c, _gameEngine.StaticVariables.TextToDisplay_8017f3b4);
            _gameEngine.UIManager.UpdateUiBoxesPosition(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground, _gameEngine.StaticVariables.TextToDisplay_8017f3d0);
            _gameEngine.UIManager.UpdateUiBoxesPosition(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons, _gameEngine.StaticVariables.TextToDisplay_8017f3ec);
            iVar1 = _gameEngine.UIManager.UpdateUiBoxesPosition(_gameEngine.StaticVariables.UIBoxConfiguration_800af664, _gameEngine.StaticVariables.TextToDisplay_8017f344);

            if (iVar1 == 1)
            {
                if ((_gameEngine.StaticVariables.INT_8017f340 & 4U) != 0)
                {
                    _gameEngine.StaticVariables.INT_8017f340 = (int)(_gameEngine.StaticVariables.INT_8017f340 & 0xfffffffb);
                }
                if ((_gameEngine.StaticVariables.INT_8017f340 & 2U) != 0)
                {
                    _gameEngine.StaticVariables.INT_8017f340 = 0;
                    _gameEngine.StaticVariables.UIBoxConfiguration_800af664.X = _gameEngine.StaticVariables.TextToDisplay_8017f344.originX;
                    _gameEngine.StaticVariables.UIBoxConfiguration_800af664.Y = _gameEngine.StaticVariables.TextToDisplay_8017f344.originY;
                    _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.X = _gameEngine.StaticVariables.TextToDisplay_8017f360.originX;
                    _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Y = _gameEngine.StaticVariables.TextToDisplay_8017f360.originY;
                    _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.X = _gameEngine.StaticVariables.TextToDisplay_8017f37c.originX;
                    _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Y = _gameEngine.StaticVariables.TextToDisplay_8017f37c.originY;
                    _gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.X = _gameEngine.StaticVariables.TextToDisplay_8017f398.originX;
                    _gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Y = _gameEngine.StaticVariables.TextToDisplay_8017f398.originY;
                    _gameEngine.StaticVariables.UIBoxConfiguration_800b287c.X = _gameEngine.StaticVariables.TextToDisplay_8017f3b4.originX;
                    _gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Y = _gameEngine.StaticVariables.TextToDisplay_8017f3b4.originY;
                    _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X = _gameEngine.StaticVariables.TextToDisplay_8017f3d0.originX;
                    _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y = _gameEngine.StaticVariables.TextToDisplay_8017f3d0.originY;
                    _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X = _gameEngine.StaticVariables.TextToDisplay_8017f3ec.originX;
                    _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y = _gameEngine.StaticVariables.TextToDisplay_8017f3ec.originY;

                    if ((_gameEngine.StaticVariables.g_postProcessState & 2U) == 0)
                    {
                        _gameEngine.StaticVariables.g_playerControlFlags = _gameEngine.StaticVariables.g_playerControlFlags & 0xfffffff7;
                    }

                    _gameEngine.MainInventoryManager.FUN_80047cb0(callBackInfo);
                    return; // 1;
                }
            }
        }

        //PTR_ARRAY_800b44b8
        var uiBoxConfig = _gameEngine.StaticVariables.INT_8017f734 switch
        {
            0 => _gameEngine.StaticVariables.UIBoxConfiguration_800af664,
            1 => _gameEngine.StaticVariables.UIBoxConfiguration_800af664,
            2 => _gameEngine.StaticVariables.UIBoxConfiguration_800af664,
            3 => _gameEngine.StaticVariables.UIBoxConfiguration_800af664,
            4 => _gameEngine.StaticVariables.UIBoxConfiguration_800af664,
            5 => _gameEngine.StaticVariables.UIBoxConfiguration_800af664,
            6 => _gameEngine.StaticVariables.UIBoxConfiguration_800af664,
            7 => _gameEngine.StaticVariables.UIBoxConfiguration_800b287c,
            8 => _gameEngine.StaticVariables.UIBoxConfiguration_800b287c,
            9 => _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc,
            10 => _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc,
            11 => _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc,
            12 => _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc,
            13 => _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc
        };

        _gameEngine.MainInventoryManager.UpdateCursorSpritePosition(_gameEngine.StaticVariables.InventoryCursorAnimation_8017f704,
            (short)(uiBoxConfig.X + _gameEngine.StaticVariables.INT_ARRAY_800b4368[_gameEngine.StaticVariables.INT_8017f734]),
            (short)(uiBoxConfig.Y + _gameEngine.StaticVariables.INT_ARRAY_800b43a0[_gameEngine.StaticVariables.INT_8017f734]),
            0); //_gameEngine.StaticVariables.g_drawModes[0x14].tag);

        _gameEngine.MainInventoryManager.DisplayInventoryCursor(_gameEngine.StaticVariables.InventoryCursorAnimation_8017f704);
        FUN_80053144(0, _gameEngine.StaticVariables.UIBoxConfiguration_800b122c);
        FUN_80053144(1, _gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c);
        FUN_80052fb4(0, _gameEngine.StaticVariables.UIBoxConfiguration_800b287c);
        FUN_80052fb4(1, _gameEngine.StaticVariables.UIBoxConfiguration_800b287c);
        FUN_80052dd8(_gameEngine.StaticVariables.UIBoxConfiguration_800af664);
        FUN_80052c64(_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc);
        FUN_80053270(_gameEngine.StaticVariables.UIBoxConfiguration_800af664);
        FUN_80053270(_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc);
        FUN_80053270(_gameEngine.StaticVariables.UIBoxConfiguration_800b122c);
        FUN_80053270(_gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c);
        FUN_80053270(_gameEngine.StaticVariables.UIBoxConfiguration_800b287c);
        FUN_80053270(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground);
        FUN_80053270(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons);
        //FUN_80053fdc();
        //FUN_80054388();
    }

    //800526cc
    private void FUN_800526cc()
    {
        _gameEngine.SoundManager.PlaySoundEffect(5);
        _gameEngine.StaticVariables.TextToDisplay_8017f344.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_8017f344.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_8017f344.speed = 0xf;
        _gameEngine.StaticVariables.INT_8017f340 = _gameEngine.StaticVariables.INT_8017f340 | 2;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800af664.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f344.x =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800af664.X + _gameEngine.StaticVariables.UIBoxConfiguration_800af664.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f344.x = _gameEngine.StaticVariables.UIBoxConfiguration_800af664.X;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800af664.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f344.y =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800af664.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800af664.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f344.y = _gameEngine.StaticVariables.UIBoxConfiguration_800af664.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f344.startX = (short)~(_gameEngine.StaticVariables.UIBoxConfiguration_800af664.Width << 3);

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800af664.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f344.startY =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800af664.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800af664.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f344.startY = _gameEngine.StaticVariables.UIBoxConfiguration_800af664.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f360.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_8017f360.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_8017f360.speed = 0xf;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f360.x =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.X + _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f360.x = _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.X;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f360.y =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f360.y = _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f360.startX = (short)~(_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Width << 3);

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f360.startY =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f360.startY = _gameEngine.StaticVariables.UIBoxConfiguration_800b06dc.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f37c.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_8017f37c.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_8017f37c.speed = 0xf;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b122c.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f37c.x =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b122c.X + _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f37c.x = _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.X;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f37c.y =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f37c.y = _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f37c.startX = 0x140;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f37c.startY =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f37c.startY = _gameEngine.StaticVariables.UIBoxConfiguration_800b122c.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f398.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_8017f398.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_8017f398.speed = 0xf;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f398.x =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.X + _gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f398.x = _gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.X;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f398.y =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f398.y = _gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f398.startX = 0x140;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f398.startY =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f398.startY = _gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f3b4.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_8017f3b4.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_8017f3b4.speed = 0xf;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b287c.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3b4.x =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b287c.X + _gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3b4.x = _gameEngine.StaticVariables.UIBoxConfiguration_800b287c.X;
        }

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3b4.y =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3b4.y = _gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f3b4.startX = 0x140;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3b4.startY =
                 (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Y + _gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3b4.startY = _gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f3d0.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_8017f3d0.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_8017f3d0.speed = 0xf;

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3d0.x =
                 (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X +
                         _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3d0.x = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        }

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3d0.y =
                 (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y +
                         _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3d0.y = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y;
        }

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3d0.startX =
                 (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X +
                         _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3d0.startX = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f3d0.startY = 0xf0;
        _gameEngine.StaticVariables.TextToDisplay_8017f3ec.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_8017f3ec.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_8017f3ec.speed = 0xf;

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3ec.x =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X + _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3ec.x = _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.X;
        }

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3ec.y =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Height * -8)
            ;
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3ec.y = _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_8017f3ec.startX = 0x140;

        if (_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3ec.startY =
                 (short)(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y + _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Height * -8)
            ;
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_8017f3ec.startY = _gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons.Y;
        }
    }

    //80053144
    private void FUN_80053144(int slotId, UIBoxConfiguration uiBoxConfig)
    {
        var index = (int)_gameEngine.PlayerManager.GetItemIdFromSlotId(_gameEngine.StaticVariables.UINT_ARRAY_800b44f0[slotId]);

        if (index != -1)
        {
            var sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017f480[slotId];
            sprite.x0 = (short)(uiBoxConfig.X + 0x10);
            sprite.y0 = (short)(uiBoxConfig.Y + 8);

            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, int.MaxValue - 1, bitmap);

            //uVar5 = (uint)pSVar7 & 0xffffff;
            //pSVar7 = pSVar7 + 1;
            //iVar3 = iVar6 + uVar1;
            //iVar6 = iVar6 + 1;
            //puVar4 = _gameEngine.StaticVariables.SPRT_ARRAY_8017f480[uVar1 + slotId * 2].tag + iVar2);

            /* Probable PsyQ macro: addPrim(). */
            //*puVar4 = *puVar4 & 0xff000000 | *puVar8 & 0xffffff;
            //sVar9 = sVar9 + -1;
            //*puVar8 = *puVar8 & 0xff000000 | uVar5;
            //iVar2 = iVar2 + 0x14;
        }
    }

    //80052fb4
    private void FUN_80052fb4(int slotId, UIBoxConfiguration uIBoxConfig)
    {
        short sVar1;

        var index = (int)_gameEngine.PlayerManager.GetItemIdFromSlotId(_gameEngine.StaticVariables.UINT_ARRAY_800b44f0[slotId]);

        if (index != -1)
        {
            sVar1 = (short)(slotId * 0x28 + 0x18);

            var sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017f408[slotId];
            _gameEngine.GraphicManager.InitializeSpriteWithImage(
                sprite,
                index,
                (short)(uIBoxConfig.X + 8),
                (short)(uIBoxConfig.Y + sVar1));

            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, int.MaxValue - 1, bitmap);

            //iVar2 = g_drawModes[0x14].tag * 0x3c;
            //iVar4 = g_drawModes[0x14].tag * 0x28;
            //puVar6 = (uint*)(&DAT_80146f68 + iVar4);
            //puVar5 = (uint*)((int)g_drawModes + iVar4 + 0xf8);

            /* Probable PsyQ macro: addPrim(). */
            //*(uint*)(&DAT_8017f408 + iVar2 + iVar7) = *(uint*)(&DAT_8017f408 + iVar2 + iVar7) & 0xff000000 | *puVar6 & 0xffffff;
            //*puVar6 = *puVar6 & 0xff000000 | (uint)(&DAT_8017f408 + iVar2 + iVar7) & 0xffffff;

            sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017f790[0];
            sprite.x0 = (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b287c.X + 8);
            sprite.y0 = (short)(_gameEngine.StaticVariables.UIBoxConfiguration_800b287c.Y + sVar1);

            bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, int.MaxValue - 1, bitmap);

            /* Probable PsyQ macro: addPrim(). */
            //*(uint*)(&DAT_8017f790 + iVar3) = *(uint*)(&DAT_8017f790 + iVar3) & 0xff000000 | *puVar5 & 0xffffff;
            //*puVar5 = *puVar5 & 0xff000000 | (uint)(&DAT_8017f790 + iVar4 + iVar7) & 0xffffff;
            //iVar3 = iVar4 + iVar7;
        }
    }

    //80052dd8
    private void FUN_80052dd8(UIBoxConfiguration uiBoxConfig)
    {
        int itemCount;
        int i;

        i = 0;

        do
        {
            itemCount = _gameEngine.PlayerManager.GetNumberOfItem(_gameEngine.StaticVariables.INT_ARRAY_800b42dc[i]);

            if (itemCount != 0)
            {
                var sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017f4f8[i];
                sprite.x0 = (short)(uiBoxConfig.X + _gameEngine.StaticVariables.INT_ARRAY_800b42f8[i]);
                sprite.y0 = (short)(uiBoxConfig.Y + _gameEngine.StaticVariables.INT_ARRAY_800b4314[i]);

                var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

                //puVar3 = _gameEngine.StaticVariables.SPRT_ARRAY_8017f4f8[iVar4];
                //itemCount = g_drawModes[0x14].tag * 0x28;
                //puVar2 = (uint*)(&DAT_80146f68 + itemCount);
                /* Probable PsyQ macro: addPrim(). */
                //*puVar3 = *puVar3 & 0xff000000 | *puVar2 & 0xffffff;
                //*puVar2 = *puVar2 & 0xff000000 | (int)&SPRT_ARRAY_8017f4f8[uVar1].tag + iVar4 & 0xffffffU;
            }

            i = i + 1;
        } while (i < 7);
    }

    //80052c64
    private void FUN_80052c64(UIBoxConfiguration uiBoxConfig)
    {
        int textureId;
        int iVar4;
        int i;

        textureId = 0;
        i = 0;
        //piVar5 = _gameEngine.StaticVariables.INT_8017f340;
        iVar4 = 0;

        do
        {
            _gameEngine.StaticVariables.INT_8017f340 = -1;

            textureId = FUN_8004e640(textureId + 1, 0x1c);

            if (textureId != -1)
            {
                var sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017f63c[i];
                _gameEngine.GraphicManager.InitializeSpriteWithImage(sprite, textureId, 0, 0);
                sprite.x0 = (short)(uiBoxConfig.X + i * 0x20 + 8);
                sprite.y0 = (short)(uiBoxConfig.Y + 4);

                var index = _gameEngine.GraphicManager.GetItemTextureIdByItemId((int)textureId);
                var image = _gameEngine.GraphicManager.GetAnimationImageByIndex(index);
                var bitmap = _gameEngine.AlundraMap.GetSpriteBitmap(image);
                _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

                //uVar1 = g_drawModes[0x14].tag;
                //_gameEngine.StaticVariables.INT_8017f340 = textureId;
                //puVar2 = (uint*)(&DAT_80146f68 + uVar1 * 0x28);
                //puVar3 = (uint*)((int)&SPRT_ARRAY_8017f63c[uVar1].tag + iVar4);
                /* Probable PsyQ macro: addPrim(). */
                //*puVar3 = *puVar3 & 0xff000000 | *puVar2 & 0xffffff;
                //*puVar2 = *puVar2 & 0xff000000 | (int)&SPRT_ARRAY_8017f63c[uVar1].tag + iVar4 & 0xffffffU;
            }

            i = i + 1;
        } while (i < 5);
    }

    //8004e640
    private int FUN_8004e640(int startIndex, int wantedPropertyId)
    {
        if (wantedPropertyId < 0x20)
        {
            if (startIndex < _gameEngine.StaticVariables.g_itemsCount)
            {
                var property = _gameEngine.StaticVariables.g_itemsProperties[startIndex * 5];

                do
                {
                    var quantity = _gameEngine.StaticVariables.g_numberOfItems[startIndex * 2 + 1];

                    if ((property == wantedPropertyId) && (0 < quantity))
                    {
                        return startIndex;
                    }

                    startIndex = startIndex + 1;
                } while (startIndex < _gameEngine.StaticVariables.g_itemsCount);
            }
        }
        else
        {
            Debugger.Break();
        }

        return -1;
    }

    //80053270
    private void FUN_80053270(UIBoxConfiguration uiBoxConfig)
    {
        SPRT sprite;
        uint puVar2;
        int w;
        int h;

        if (0 < uiBoxConfig.Height)
        {
            h = 0;

            do
            {
                w = 0;

                if (0 < uiBoxConfig.Width)
                {
                    //puVar2 = (uint*)((int)g_drawModes + uVar1 * 0x28 + 0xf8);

                    do
                    {
                        sprite = uiBoxConfig.SpritesA[h * uiBoxConfig.Width + w];

                        var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                        _gameEngine.Renderer.AddSprite(sprite, int.MaxValue, bitmap);

                        w = w + 1;
                        /* Probable PsyQ macro: addPrim(). */
                        //sprite.tag = sprite.tag & 0xff000000 | *puVar2 & 0xffffff;
                        //*puVar2 = *puVar2 & 0xff000000 | (uint)sprite & 0xffffff;
                    } while (w < uiBoxConfig.Width);
                }
                h = h + 1;
            } while (h < uiBoxConfig.Height);
        }
    }

    //80052100
    public void InitializeSubInventorySprite()
    {
        SPRT sprite;
        int piVar3;
        int i;
        int iVar7;
        int j;

        j = 0;

        //do
        //{
            i = 0;

            do
            {
                //SetSprt(spr);
                //SetSemiTrans(spr, 0);
                //SetShadeTex(spr, 1);
                sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017f7e0[i];
                sprite.w = 8;
                sprite.h = 0x10;
                sprite.clut = 0; //g_clutTable[(ushort)BYTE_ARRAY_8009cfd8._2_2_];
                sprite.r0 = 0x80;
                sprite.g0 = 0x80;
                sprite.b0 = 0x80;
                i = i + 1;
            } while (i < 4);

            i = 0;

            do
            {
                //SetSprt(sprite);
                //SetSemiTrans(sprite, 0);
                //SetShadeTex(sprite, 1);
                sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017f880[i];
                sprite.w = 8;
                sprite.h = 0x10;
                sprite.clut = 0; //g_clutTable[(ushort)BYTE_ARRAY_8009cfd8._2_2_];
                sprite.r0 = 0x80;
                sprite.g0 = 0x80;
                sprite.b0 = 0x80;
                i = i + 1;
            } while (i < 2);

            i = 0;

            do
            {
                //SetSprt(sprite);
                //SetSemiTrans(sprite, 0);
                //SetShadeTex(sprite, 1);
                sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017f8d0[i];
                sprite.w = 8;
                sprite.h = 0x10;
                sprite.clut = 0; //g_clutTable[(ushort)BYTE_ARRAY_8009cfd8._2_2_];
                sprite.r0 = 0x80;
                sprite.g0 = 0x80;
                sprite.b0 = 0x80;
                i++;
            } while (i < 2);

            i = 0;

            do
            {
                //SetSprt(sprite);
                //SetSemiTrans(sprite, 0);
                //SetShadeTex(sprite, 1);
                sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017f790[i];
                sprite.w = 0x18;
                sprite.h = 0x20;
                sprite.u0 = (byte)'0';
                sprite.v0 = 0x98;
                sprite.clut = 0; //g_clutTable[(ushort)BYTE_ARRAY_8009cfd8._2_2_];
                sprite.r0 = 0x80;
                sprite.g0 = 0x80;
                sprite.b0 = 0x80;
                i++;
        } while (i < 2);

        //} while (j < 2);

        InitializeSpriteGrid(_gameEngine.StaticVariables.UIBoxConfiguration_800af664);
        InitializeSpriteGrid(_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc);
        InitializeSpriteGrid(_gameEngine.StaticVariables.UIBoxConfiguration_800b122c);
        InitializeSpriteGrid(_gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c);
        InitializeSpriteGrid(_gameEngine.StaticVariables.UIBoxConfiguration_800b287c);
        InitializeSpriteGrid(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground);

        i = 0;

        do
        {
            sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017f4f8[i];
            _gameEngine.GraphicManager.InitializeSpriteWithImage(
                sprite, _gameEngine.StaticVariables.INT_ARRAY_800b42dc[i], 0, 0);
            i = i + 1;
        } while (i < 7);

        _gameEngine.MainInventoryManager.FUN_80050998(_gameEngine.StaticVariables.InventoryCursorAnimation_8017f704);
    }

    //80051f70
    private void InitializeSpriteGrid(UIBoxConfiguration uiBoxConfig)
    {
        int index;
        int w;
        int h;
        int passIndex;

        passIndex = 0;

        do
        {
            h = 0;

            if (0 < uiBoxConfig.Height)
            {
                do
                {
                    w = 0;

                    if (0 < uiBoxConfig.Width)
                    {
                        do
                        {
                            //SetSprt(uiBoxConfig.SpritesA + h * gridWidth + w);
                            //SetSemiTrans(uiBoxConfig.SpritesA + h * uiBoxConfig.Width + w, 0);
                            //SetShadeTex(uiBoxConfig.SpritesA + h * uiBoxConfig.Width + w, 1);
                            index = h * uiBoxConfig.Width + w;
                            uiBoxConfig.SpritesA[index].clut = 0; //g_clutTable[uiBoxConfig.SpritesA[index].clut];
                            w = w + 1;
                        } while (w < uiBoxConfig.Width);
                    }

                    h = h + 1;

                } while (h < uiBoxConfig.Height);
            }

            passIndex = passIndex + 1;

        } while (passIndex < 2);
    }
}