using AlundraEngine.Gameplay;
using AlundraEngine.Graphics;
using AlundraEngine.Text;
using AlundraEngine.UI;
using System;
using System.Diagnostics;

namespace AlundraEngine;

public class SubInventoryManager
{
    private readonly GameEngine _gameEngine;

    public readonly List<Renderer.Sprite> InventoryArmoryNameSprites = new();
    public readonly List<Renderer.Sprite> InventoryBootNameSprites = new();
    public readonly List<Renderer.Sprite>[] InventoryItemDescriptionLinesSprites = [new(), new()];

    public SubInventoryManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    //800537f0
    public void InitializeSubInventory(CallBackInfo callBackInfo)
    {
        _gameEngine.StaticVariables.g_subInventoryState = 5;
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
    private void FUN_80052f24(int index)
    {
        int itemId;

        itemId = (int)_gameEngine.PlayerManager.GetItemIdFromSlotId(_gameEngine.StaticVariables.UINT_ARRAY_800b44f0[index]);

        if (itemId != -1)
        {
            SPRT[] sprites =
            [
                _gameEngine.StaticVariables.SPRT_ARRAY_8017f480[index * 2],
                _gameEngine.StaticVariables.SPRT_ARRAY_8017f480[index * 2 + 1]
            ];

            var nameSprites = index == 1 ? InventoryBootNameSprites : InventoryArmoryNameSprites;
            nameSprites.Clear();
            var itemName = _gameEngine.EtcRes.GetItemName(itemId);

            _gameEngine.UIManager.DisplayIconName(sprites,
                nameSprites,
                itemName.ToCharArray(),
                0x20,
                0, //0x140,
                0,
                index);
        }
    }

    //80053328
    public void DisplaySubInventory(CallBackInfo callBackInfo)
    {
        int iVar1;

        //DISPENV DStack_40;
        //GetDispEnv(&DStack_40);
        //SetDrawArea((DR_AREA*)(&UNK_8017f610 + g_drawModes[0x14].tag * 0xc), &DStack_40.disp);

        if ((_gameEngine.StaticVariables.g_subInventoryState & 6U) == 0)
        {
            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Right) != 0)
            {
                _gameEngine.StaticVariables.INT_8017f734 = _gameEngine.StaticVariables.INT_ARRAY_800b43d8[_gameEngine.StaticVariables.INT_8017f734];
                _gameEngine.StaticVariables.INT_8017f788 = 0;
                _gameEngine.SoundManager.PlaySoundEffect(1);
            }

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Left) != 0)
            {
                _gameEngine.StaticVariables.INT_8017f734 = _gameEngine.StaticVariables.INT_ARRAY_800b4410[_gameEngine.StaticVariables.INT_8017f734];
                _gameEngine.StaticVariables.INT_8017f788 = 0;
                _gameEngine.SoundManager.PlaySoundEffect(1);
            }

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Up) != 0)
            {
                _gameEngine.StaticVariables.INT_8017f734 = _gameEngine.StaticVariables.INT_ARRAY_800b4448[_gameEngine.StaticVariables.INT_8017f734];
                _gameEngine.StaticVariables.INT_8017f788 = 0;
                _gameEngine.SoundManager.PlaySoundEffect(1);
            }

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.Down) != 0)
            {
                _gameEngine.StaticVariables.INT_8017f734 = _gameEngine.StaticVariables.INT_ARRAY_800b4480[_gameEngine.StaticVariables.INT_8017f734];
                _gameEngine.StaticVariables.INT_8017f788 = 0;
                _gameEngine.SoundManager.PlaySoundEffect(1);
            }

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.OpenInventory) != 0)
            {
                FUN_800526cc();
                _gameEngine.MainInventoryManager.UpdateHudTransitionState();
                _gameEngine.HudManager.InitializeHudPositionBeforeHide();
            }

            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & PadState.OpenSubInventory) != 0)
            {
                FUN_800526cc();
                _gameEngine.StaticVariables.g_playerControlFlags |= 8;
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
                if ((_gameEngine.StaticVariables.g_subInventoryState & 4U) != 0)
                {
                    _gameEngine.StaticVariables.g_subInventoryState = (int)(_gameEngine.StaticVariables.g_subInventoryState & 0xfffffffb);
                }

                if ((_gameEngine.StaticVariables.g_subInventoryState & 2U) != 0)
                {
                    _gameEngine.StaticVariables.g_subInventoryState = 0;
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
                        _gameEngine.StaticVariables.g_playerControlFlags &= 0xfffffff7;
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
        FUN_80053144(0, _gameEngine.StaticVariables.UIBoxConfiguration_800b122c); // armor text
        FUN_80053144(1, _gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c); // boots text
        FUN_80052fb4(0, _gameEngine.StaticVariables.UIBoxConfiguration_800b287c); // armor icon
        FUN_80052fb4(1, _gameEngine.StaticVariables.UIBoxConfiguration_800b287c); // boots icon
        FUN_80052dd8(_gameEngine.StaticVariables.UIBoxConfiguration_800af664); // armory icon
        FUN_80052c64(_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc); // key items icon
        FUN_80053270(_gameEngine.StaticVariables.UIBoxConfiguration_800af664); // armory background
        FUN_80053270(_gameEngine.StaticVariables.UIBoxConfiguration_800b06dc); // key item background
        FUN_80053270(_gameEngine.StaticVariables.UIBoxConfiguration_800b122c); // armor text background
        FUN_80053270(_gameEngine.StaticVariables.UIBoxConfiguration_800b1d7c); // boots text background
        FUN_80053270(_gameEngine.StaticVariables.UIBoxConfiguration_800b287c); // armor/boots icon background
        FUN_80053270(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground); // description text/background
        FUN_80053270(_gameEngine.StaticVariables.g_UiBoxesInventoryMoneyFalconKeyIcons); // money/falcon/keys icon
        FUN_80053fdc(); // item description
        DisplayAmountOfMoneyFalconKeys2();
    }

    //80053fdc
    private void FUN_80053fdc()
    {
        int iVar1;
        int iVar2;
        int iVar3;
        int uVar4;
        int uVar5;
        string text;
        int iVar6;

        uVar4 = _gameEngine.StaticVariables.INT_ARRAY_800b4330[_gameEngine.StaticVariables.INT_8017f734];

        if (uVar4 < -7)
        {
            LAB_80054088:
            iVar1 = _gameEngine.StaticVariables.INT_ARRAY_800b4330[_gameEngine.StaticVariables.INT_8017f734];
            iVar6 = _gameEngine.PlayerManager.GetNumberOfItem(iVar1);

            if (iVar6 == 0)
            {
                return;
            }
        }
        else
        {
            if (uVar4 < -2)
            {
                iVar1 = _gameEngine.StaticVariables.INT_ARRAY_8017f628[-3 - uVar4];
            }
            else
            {
                if (-1 < uVar4)
                {
                    //goto LAB_80054088;
                    iVar1 = _gameEngine.StaticVariables.INT_ARRAY_800b4330[_gameEngine.StaticVariables.INT_8017f734];
                    iVar6 = _gameEngine.PlayerManager.GetNumberOfItem(iVar1);

                    if (iVar6 == 0)
                    {
                        return;
                    }
                }
                else
                {
                    iVar1 = (int)_gameEngine.PlayerManager.GetItemIdFromSlotId(_gameEngine.StaticVariables.UINT_ARRAY_800b44f0[~uVar4]);
                }
            }

            if (iVar1 == -1)
            {
                return;
            }
        }

        if (_gameEngine.StaticVariables.INT_8017f788 == 0)
        {
            InventoryItemDescriptionLinesSprites[0].Clear();
            text = _gameEngine.EtcRes.GetItemName(iVar1); //_gameEngine.StaticVariables.g_itemDropProperties[iVar1 * 2];
            iVar1 = 0x20;

            LAB_8005420c:
            //_gameEngine.UIManager.DisplayIconName(
            //    _gameEngine.StaticVariables.SPRT_ARRAY_8017f738,
            //    InventoryItemDescriptionLinesSprites[0],
            //    text.ToCharArray(),
            //    iVar1,
            //    0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X,
            //    0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y, 
            //    2);
            _gameEngine.StaticVariables.INT_8017f78c = 0;
            _gameEngine.StaticVariables.SPRT_ARRAY_8017f738[0].w = 0;
            _gameEngine.StaticVariables.SPRT_ARRAY_8017f738[1].w = 0;
            _gameEngine.StaticVariables.INT_8017f788 += 1;
        }
        else
        {
            var itemName = _gameEngine.EtcRes.GetItemName(iVar1); //_gameEngine.StaticVariables.g_itemDropProperties[iVar1 * 2];

            if (_gameEngine.StaticVariables.INT_8017f788 - 1U < 0x10)
            {
                if (itemName.Length <= _gameEngine.StaticVariables.INT_8017f788 - 1)
                {
                    _gameEngine.StaticVariables.INT_8017f788 = 0x11;
                    uVar5 = 0;
                }
                else
                {
                    InventoryItemDescriptionLinesSprites[0].Clear();
                    _gameEngine.UIManager.DisplayIconName(
                        _gameEngine.StaticVariables.SPRT_ARRAY_8017f738,
                        InventoryItemDescriptionLinesSprites[0],
                        itemName.Substring(0, _gameEngine.StaticVariables.INT_8017f788).ToCharArray(),
                        iVar1,
                        0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X,
                        0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y, 
                        2);

                    FUN_80053f3c(0,
                        _gameEngine.StaticVariables.INT_8017f788,
                        itemName[_gameEngine.StaticVariables.INT_8017f788 - 1]);
                    uVar5 = 0;
                }
            }
            else
            {
                iVar6 = 0;

                if (_gameEngine.StaticVariables.INT_8017f788 - 0x11U < 0x3c)
                {
                    do
                    {
                        iVar2 = iVar6; // + g_drawModes[0x14].tag;
                        iVar3 = itemName.Length;
                        iVar6 += 1;
                        _gameEngine.StaticVariables.SPRT_ARRAY_8017f738[iVar2].w = (short)(iVar3 << 3);
                    } while (iVar6 < 1);

                    uVar5 = 0;
                    _gameEngine.StaticVariables.INT_8017f788 += 1;
                }
                else
                {
                    var description = _gameEngine.EtcRes.GetItemDescription(iVar1); //(_gameEngine.StaticVariables.g_itemDescriptionsEtc)[iVar1 * 2];

                    if (_gameEngine.StaticVariables.INT_8017f788 == 0x4d)
                    {
                        text = description;
                        iVar1 = 0x40;
                        //goto LAB_8005420c;
                        InventoryItemDescriptionLinesSprites[0].Clear();
                        //_gameEngine.UIManager.DisplayIconName(
                        //    _gameEngine.StaticVariables.SPRT_ARRAY_8017f738,
                        //    InventoryItemDescriptionLinesSprites[0],
                        //    text.ToCharArray(),
                        //    iVar1,
                        //    0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X,
                        //    0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y,
                        //    2);
                        _gameEngine.StaticVariables.INT_8017f78c = 0;
                        _gameEngine.StaticVariables.SPRT_ARRAY_8017f738[0].w = 0;
                        _gameEngine.StaticVariables.SPRT_ARRAY_8017f738[1].w = 0;
                        _gameEngine.StaticVariables.INT_8017f788 += 1;
                        return;
                    }

                    if (_gameEngine.StaticVariables.INT_8017f788 - 0x4eU < 0x40)
                    {
                        uVar5 = 0;
                        if (description.Length <= _gameEngine.StaticVariables.INT_8017f788 - 0x4e)
                        {
                            _gameEngine.StaticVariables.INT_8017f788 = 0x8e;
                        }
                        else
                        {
                            InventoryItemDescriptionLinesSprites[0].Clear();
                            _gameEngine.UIManager.DisplayIconName(
                                _gameEngine.StaticVariables.SPRT_ARRAY_8017f738,
                                InventoryItemDescriptionLinesSprites[0],
                                description.Substring(0, _gameEngine.StaticVariables.INT_8017f788 - 0x4d).ToCharArray(),
                                iVar1,
                                0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X,
                                0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y,
                                2);

                            FUN_80053f3c(0,
                                _gameEngine.StaticVariables.INT_8017f788 + -0x4d,
                                description[_gameEngine.StaticVariables.INT_8017f788 - 0x4e]);
                            uVar5 = 0;
                        }
                    }
                    else
                    {
                        if (_gameEngine.StaticVariables.INT_8017f788 == 0x8e)
                        {
                            //var name = _gameEngine.EtcRes.GetItemDescription(iVar1);//_gameEngine.StaticVariables.g_DescriptionEtcBase[iVar1 * 2];
                            SPRT[] sprites = [_gameEngine.StaticVariables.SPRT_ARRAY_8017f738[2], _gameEngine.StaticVariables.SPRT_ARRAY_8017f738[3]];

                            InventoryItemDescriptionLinesSprites[1].Clear();
                            //_gameEngine.UIManager.DisplayIconName(
                            //    sprites,
                            //    InventoryItemDescriptionLinesSprites[1],
                            //    description.ToCharArray(),
                            //    0x40,
                            //    0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X,
                            //    0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y, 
                            //    3);
                            _gameEngine.StaticVariables.INT_8017f78c = 0;
                            _gameEngine.StaticVariables.INT_8017f788 += 1;
                            FUN_80053e54(0);
                            _gameEngine.StaticVariables.SPRT_ARRAY_8017f738[2].w = 0;
                            _gameEngine.StaticVariables.SPRT_ARRAY_8017f738[3].w = 0;
                            return;
                        }

                        if (_gameEngine.StaticVariables.INT_8017f788 - 0x8fU < 0x40)
                        {
                            if (description.Length >= _gameEngine.StaticVariables.INT_8017f788 - 0x90)
                            {
                                _gameEngine.StaticVariables.INT_8017f788 = 0xcf;
                            }
                            else
                            {
                                SPRT[] sprites = [_gameEngine.StaticVariables.SPRT_ARRAY_8017f738[2], _gameEngine.StaticVariables.SPRT_ARRAY_8017f738[3]];

                                InventoryItemDescriptionLinesSprites[1].Clear();
                                _gameEngine.UIManager.DisplayIconName(
                                    sprites,
                                    InventoryItemDescriptionLinesSprites[1],
                                    description.Substring(0, (int)(_gameEngine.StaticVariables.INT_8017f788 - 0x8fU)).ToCharArray(),
                                    0x40,
                                    0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X,
                                    0, //_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y, 
                                    3);

                                FUN_80053f3c(1,
                                    (int)(_gameEngine.StaticVariables.INT_8017f788 - 0x8fU),
                                    description[_gameEngine.StaticVariables.INT_8017f788 - 0x90]);
                            }
                        }
                        else if (_gameEngine.StaticVariables.INT_8017f788 != 0xcf)
                        {
                            return;
                        }

                        FUN_80053e54(0);
                        uVar5 = 1;
                    }
                }
            }

            FUN_80053e54(uVar5);
        }
    }

    //80053f3c
    private void FUN_80053f3c(int index, int param_2, char c)
    {
        if (_gameEngine.StaticVariables.INT_8017f78c == 0)
        {
            _gameEngine.StaticVariables.INT_8017f78c = 2;
            _gameEngine.StaticVariables.INT_8017f788 += 1;

            var width = _gameEngine.StaticVariables.g_fontCharWidthTable[(c & 0xff) * 5];
            var sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017f738[index];
            sprite.w = (short)(sprite.w + width);
            sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017f738[index + 1];
            sprite.w = (short)(sprite.w + width);

            /*
            var charValue = TextDecoder.ConvertCp850ToLatin1(c);

            var bitmap = _gameEngine.Font3.GenerateFontBitmapTim(
                (charValue % 16) * 16,
                (charValue / 16) * 16,
                16, 16, 8);

            InventoryItemDescriptionLinesSprites[index].Add(new Sprite(
                sprite.w, index * 0x10,
                16, 16, SpriteDepth.ForegroundUI, bitmap));*/
        }
        else
        {
            _gameEngine.StaticVariables.INT_8017f78c += -1;
        }
    }

    //80053e54
    private void FUN_80053e54(int index)
    {
        ulong uVar1;
        uint puVar2;
        uint uVar3;
        SPRT pSVar4;
        SPRT pSVar5;
        int iVar6;
        short sVar7;
        int i;

        uVar1 = 0; //g_drawModes[0x14].tag;
        sVar7 = 0xc;
        //pSVar5 = _gameEngine.StaticVariables.SPRT_80146f5c[0]; // + g_drawModes[0x14].tag * 2;
        //pSVar4 = _gameEngine.StaticVariables.SPRT_ARRAY_8017f738[index * 2];
        //iVar6 = 0;
        i = 0;

        //do
        //{
        var sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017f738[index * 2];
        sprite.x0 = (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X + 0x10);
        sprite.y0 = (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y + sVar7 + (short)index * 0x10);

        //TODO text to display here
        //var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
        //_gameEngine.Renderer.AddSprite(sprite, SpriteDepth., bitmap);

        foreach (var spr in InventoryItemDescriptionLinesSprites[index])
        {
            _gameEngine.Renderer.AddSprite(spr.X + sprite.x0, spr.Y + sprite.y0,
                spr.Width, spr.Height,
                SpriteDepth.ForegroundUI, spr.Bitmap, spr.Alpha);
        }

        //uVar3 = (uint)pSVar4 & 0xffffff;
        //pSVar4 = pSVar4 + 1;
        //puVar2 = (uint*)((int)&SPRT_ARRAY_8017f738[uVar1 + index * 2].tag + iVar6);
        /* Probable PsyQ macro: addPrim(). */
        //*puVar2 = *puVar2 & 0xff000000 | pSVar5->tag & 0xffffff;
        //pSVar5->tag = pSVar5->tag & 0xff000000 | uVar3;
        //iVar6 = iVar6 + 0x14;
        //sVar7 = (short)(sVar7 + -1);
        //i = i + 1;
        //} while (i < 1);
    }

    //80054388
    private void DisplayAmountOfMoneyFalconKeys2()
    {
        //same function as DisplayAmountOfMoneyFalconKeys2 but use different sprites
        //I just copy the function to avoid confusion
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
                Debugger.Break();
            }

            sprite = _gameEngine.StaticVariables.g_spriteInventoryMoney[i];

            iVar2 = value / divisor % 10 * 0x14;
            sprite.u0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar2];
            sprite.v0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar2 + 1];
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
                Debugger.Break();
            }

            sprite = _gameEngine.StaticVariables.g_spriteInventoryNumberOfKeys[i];

            iVar2 = value / divisor % 10 * 0x14;
            sprite.u0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar2];
            sprite.v0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar2 + 1];
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
                Debugger.Break();
            }

            sprite = _gameEngine.StaticVariables.g_spriteInventoryNumberOfFalcon[i];

            iVar2 = value / divisor % 10 * 0x14;
            sprite.u0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar2];
            sprite.v0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar2 + 1];
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

    //800526cc
    private void FUN_800526cc()
    {
        _gameEngine.SoundManager.PlaySoundEffect(5);
        _gameEngine.StaticVariables.TextToDisplay_8017f344.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_8017f344.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_8017f344.speed = 0xf;
        _gameEngine.StaticVariables.g_subInventoryState |= 2;

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

            var sprites = slotId == 0 ? InventoryArmoryNameSprites : InventoryBootNameSprites;

            foreach (var spr in sprites)
            {
                _gameEngine.Renderer.AddSprite(spr.X + sprite.x0, spr.Y + sprite.y0,
                    spr.Width, spr.Height,
                    SpriteDepth.ForegroundUI, spr.Bitmap, spr.Alpha);
            }

            //TODO display text here
            //var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            //_gameEngine.Renderer.AddSprite(sprite, SpriteDepth., bitmap);

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

            index = _gameEngine.GraphicManager.GetItemTextureIdByItemId((int)index);
            var image = _gameEngine.GraphicManager.GetAnimationImageByIndex(index);
            var bitmap3 = _gameEngine.AlundraMap.GetSpriteBitmap(image);
            _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap3);

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

            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

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
            var itemId = _gameEngine.StaticVariables.INT_ARRAY_800b42dc[i];
            itemCount = _gameEngine.PlayerManager.GetNumberOfItem(itemId);

            if (itemCount != 0)
            {
                var sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017f4f8[i];
                sprite.x0 = (short)(uiBoxConfig.X + _gameEngine.StaticVariables.INT_ARRAY_800b42f8[i]);
                sprite.y0 = (short)(uiBoxConfig.Y + _gameEngine.StaticVariables.INT_ARRAY_800b4314[i]);

                var index = _gameEngine.GraphicManager.GetItemTextureIdByItemId((int)itemId);
                var image = _gameEngine.GraphicManager.GetAnimationImageByIndex(index);
                var bitmap = _gameEngine.AlundraMap.GetSpriteBitmap(image);
                _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

                //puVar3 = _gameEngine.StaticVariables.SPRT_ARRAY_8017f4f8[iVar4];
                //itemCount = g_drawModes[0x14].tag * 0x28;
                //puVar2 = (uint*)(&DAT_80146f68 + itemCount);
                /* Probable PsyQ macro: addPrim(). */
                //*puVar3 = *puVar3 & 0xff000000 | *puVar2 & 0xffffff;
                //*puVar2 = *puVar2 & 0xff000000 | (int)&SPRT_ARRAY_8017f4f8[uVar1].tag + iVar4 & 0xffffffU;
            }

            i += 1;
        } while (i < 7);
    }

    //80052c64
    private void FUN_80052c64(UIBoxConfiguration uiBoxConfig)
    {
        int itemId;
        int iVar4;
        int i;

        itemId = 0;
        i = 0;
        //piVar5 = _gameEngine.StaticVariables.g_subInventoryState;
        iVar4 = 0;

        do
        {
            _gameEngine.StaticVariables.INT_ARRAY_8017f628[i] = -1;

            itemId = FUN_8004e640(itemId + 1, 0x1c);

            if (itemId != -1)
            {
                _gameEngine.StaticVariables.INT_ARRAY_8017f628[i] = itemId;

                var sprite = _gameEngine.StaticVariables.SPRT_ARRAY_8017f63c[i];
                _gameEngine.GraphicManager.InitializeSpriteWithImage(sprite, itemId, 0, 0);
                sprite.x0 = (short)(uiBoxConfig.X + i * 0x20 + 8);
                sprite.y0 = (short)(uiBoxConfig.Y + 4);

                var index = _gameEngine.GraphicManager.GetItemTextureIdByItemId(itemId);
                var image = _gameEngine.GraphicManager.GetAnimationImageByIndex(index);
                var bitmap = _gameEngine.AlundraMap.GetSpriteBitmap(image);
                _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

                //uVar1 = g_drawModes[0x14].tag;
                //_gameEngine.StaticVariables.g_subInventoryState = textureId;
                //puVar2 = (uint*)(&DAT_80146f68 + uVar1 * 0x28);
                //puVar3 = (uint*)((int)&SPRT_ARRAY_8017f63c[uVar1].tag + iVar4);
                /* Probable PsyQ macro: addPrim(). */
                //*puVar3 = *puVar3 & 0xff000000 | *puVar2 & 0xffffff;
                //*puVar2 = *puVar2 & 0xff000000 | (int)&SPRT_ARRAY_8017f63c[uVar1].tag + iVar4 & 0xffffffU;
            }

            i += 1;
        } while (i < 5);
    }

    //8004e640
    private int FUN_8004e640(int startIndex, int wantedPropertyId)
    {
        if (wantedPropertyId < 0x20)
        {
            if (startIndex < _gameEngine.StaticVariables.g_itemsCount)
            {
                do
                {
                    var property = _gameEngine.StaticVariables.g_itemsProperties[startIndex * 5];
                    var quantity = _gameEngine.StaticVariables.g_numberOfItems[startIndex * 2 + 1];

                    if ((property == wantedPropertyId) && (0 < quantity))
                    {
                        return startIndex;
                    }

                    startIndex += 1;
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
                        _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.BackgroundUI, bitmap);

                        w += 1;
                        /* Probable PsyQ macro: addPrim(). */
                        //sprite.tag = sprite.tag & 0xff000000 | *puVar2 & 0xffffff;
                        //*puVar2 = *puVar2 & 0xff000000 | (uint)sprite & 0xffffff;
                    } while (w < uiBoxConfig.Width);
                }
                h += 1;
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
            i += 1;
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
            i += 1;
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
            _gameEngine.GraphicManager.InitializeSpriteWithImage(sprite, _gameEngine.StaticVariables.INT_ARRAY_800b42dc[i], 0, 0);
            i += 1;
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
                            w += 1;
                        } while (w < uiBoxConfig.Width);
                    }

                    h += 1;

                } while (h < uiBoxConfig.Height);
            }

            passIndex += 1;

        } while (passIndex < 2);
    }

}