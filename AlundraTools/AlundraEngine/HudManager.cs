using AlundraEngine.Gameplay;
using AlundraEngine.Graphics;
using AlundraEngine.UI;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using static AlundraEngine.Renderer;

namespace AlundraEngine;

public class HudManager
{
    private readonly GameEngine _gameEngine;

    public HudManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    //8004bd9c
    public void InitializeHudPosition()
    {
        if ((_gameEngine.StaticVariables.g_drawFrameFlags & 3U) == 1)
        {
            _gameEngine.StaticVariables.g_textToDisplay_hud.mode = 2;
            _gameEngine.StaticVariables.g_textToDisplay_hud.tick = 0;
            _gameEngine.StaticVariables.g_textToDisplay_hud.speed = 0xf;
            _gameEngine.StaticVariables.g_textToDisplay_hud.x = 0;
            _gameEngine.StaticVariables.g_textToDisplay_hud.y = 0x10;
            _gameEngine.StaticVariables.g_textToDisplay_hud.startX = 0;
            _gameEngine.StaticVariables.g_textToDisplay_hud.startY = (short)~(_gameEngine.StaticVariables.UIBoxHud.Height << 3);
            _gameEngine.StaticVariables.g_drawFrameFlags |= 2;
        }
    }

    //8004be0c
    public void InitializeHudPositionBeforeHide()
    {
        if ((_gameEngine.StaticVariables.g_saveData.MapFlags[0x33] & 0x40000000U) != 0
            && _gameEngine.StaticVariables.g_drawFrameFlags == 0)
        {
            _gameEngine.GraphicManager.SetTransitionType(1);
            _gameEngine.StaticVariables.g_textToDisplay_hud.mode = 2;
            _gameEngine.StaticVariables.g_textToDisplay_hud.tick = 0;
            _gameEngine.StaticVariables.g_textToDisplay_hud.speed = 0xf;
            _gameEngine.StaticVariables.g_textToDisplay_hud.startX = 0;
            _gameEngine.StaticVariables.g_textToDisplay_hud.startY = 0x10;
            _gameEngine.StaticVariables.g_textToDisplay_hud.x = 0;
            _gameEngine.StaticVariables.g_textToDisplay_hud.y = (short)~(_gameEngine.StaticVariables.UIBoxHud.Height << 3);
            _gameEngine.StaticVariables.g_drawFrameFlags = 5;
        }
    }

    //8004b770
    //init hud
    public void FUN_8004b770(CallBackInfo callBackInfo)
    {
        var index = 0;
        var index3 = 0;
        var iVar7 = 0;

        _gameEngine.StaticVariables.INT_ARRAY_800a8284[1] = _gameEngine.PlayerManager.GetPlayerHpMax();
        _gameEngine.StaticVariables.INT_ARRAY_800a8284[3] = _gameEngine.PlayerManager.GetPlayerMpMax();

        do
        {
            var sprt = _gameEngine.StaticVariables.g_MpIconsSprites[index];
            sprt.w = 8;
            sprt.h = 0x10;
            sprt.u0 = 0;
            sprt.v0 = 0;
            sprt.x0 = (short)(_gameEngine.StaticVariables.UIBoxHud.X + 0x90 + index * 8);
            sprt.y0 = _gameEngine.StaticVariables.UIBoxHud.Y;
            sprt.clut = _gameEngine.StaticVariables.g_clutTable[_gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar7]];

            //SetSprt(pSVar5);
            //SetSemiTrans(pSVar5, 0);
            //SetShadeTex(pSVar5, 1);

            iVar7 += 2;
            index += 1;
        } while (index < 5);

        index = 0;

        //number of group of life
        do
        {
            var sprt = _gameEngine.StaticVariables.g_lifeBigIconSprites[index];
            sprt.w = 0x10;
            sprt.h = 0x10;
            //sprt.u0 = 0;
            //sprt.v0 = 0;
            sprt.x0 = (short)(_gameEngine.StaticVariables.UIBoxHud.X + 0x50 + index * 8);
            sprt.y0 = _gameEngine.StaticVariables.UIBoxHud.Y;
            sprt.clut = _gameEngine.StaticVariables.g_clutTable[_gameEngine.StaticVariables.DAT_800a0d62];

            //SetSprt(pSVar5);
            //SetSemiTrans(pSVar5, 0);
            //SetShadeTex(pSVar5, 1);

            iVar7 += 2;
            index += 1;
        } while (index < 5);

        index3 = 0;

        //life
        do
        {
            index = 0;
            iVar7 = 0;

            do
            {
                var sprt = _gameEngine.StaticVariables.g_lifeSmallIconSprites[index + 6 * index3];
                sprt.w = 0x10;
                sprt.h = 0x10;
                sprt.u0 = 0x88; //BYTE_800a206c array ?
                sprt.v0 = 0x38; //BYTE_800a206d
                sprt.x0 = (short)(_gameEngine.StaticVariables.UIBoxHud.X + 0x78 + index * 8);
                sprt.y0 = (short)(_gameEngine.StaticVariables.UIBoxHud.Y + index3 * 8);
                sprt.clut = _gameEngine.StaticVariables.g_clutTable[_gameEngine.StaticVariables.DAT_800a0d62];

                //SetSprt(pSVar5);
                //SetSemiTrans(pSVar5, 0);
                //SetShadeTex(pSVar5, 1);

                iVar7 += 2;
                index += 1;
            } while (index < 6);

            index3++;
        } while (index3 < 2);

        index = 0;

        do
        {
            var sprt = _gameEngine.StaticVariables.g_MpIconSprites[index];
            sprt.w = 8;
            sprt.h = 0x10;
            sprt.u0 = 0x88; //BYTE_800a206c array ?
            sprt.v0 = 0x38; //BYTE_800a206d
            sprt.x0 = (short)(_gameEngine.StaticVariables.UIBoxHud.X + 0xd8 + index * 8);
            sprt.y0 = _gameEngine.StaticVariables.UIBoxHud.Y;
            sprt.clut = _gameEngine.StaticVariables.g_clutTable[_gameEngine.StaticVariables.DAT_800a323a];

            //SetSprt(pSVar5);
            //SetSemiTrans(pSVar5, 0);
            //SetShadeTex(pSVar5, 1);

            iVar7 += 2;
            index += 1;
        } while (index < 4);

        index = 0;

        //money
        do
        {
            var sprt = _gameEngine.StaticVariables.g_HudMoneySprites[index];
            sprt.w = 8;
            sprt.h = 0x10;
            sprt.u0 = 0; //BYTE_800a206c array ?
            sprt.v0 = 0; //BYTE_800a206d
            sprt.x0 = (short)(_gameEngine.StaticVariables.UIBoxHud.X + 0x100 + index * 8);
            sprt.y0 = (short)(_gameEngine.StaticVariables.UIBoxHud.Y + index3 * 8);
            sprt.clut = _gameEngine.StaticVariables.g_clutTable[_gameEngine.StaticVariables.DAT_8009fb82];

            //SetSprt(pSVar5);
            //SetSemiTrans(pSVar5, 0);
            //SetShadeTex(pSVar5, 1);

            iVar7 += 2;
            index += 1;
        } while (index < 5);

        index = 0;

        do
        {
            var polyG4 = _gameEngine.StaticVariables.g_hudBackgroundWeaponAndItemPolyG4s[index];
            var pbVar4 = _gameEngine.StaticVariables.g_inventoryWeaponIconX[index + 8];

            polyG4.x0 = (short)(_gameEngine.StaticVariables.UIBoxHud.X + pbVar4);
            polyG4.y0 = _gameEngine.StaticVariables.UIBoxHud.Y;
            polyG4.x1 = (short)(pbVar4 + 0x18);
            polyG4.y1 = _gameEngine.StaticVariables.UIBoxHud.Y;
            polyG4.x2 = pbVar4;
            polyG4.y2 = (short)(_gameEngine.StaticVariables.UIBoxHud.Y + 0x20);
            polyG4.x3 = (short)(pbVar4 + 0x18);
            polyG4.r0 = 0;
            polyG4.g0 = 0;
            polyG4.b0 = 0;
            polyG4.r1 = 0xff;
            polyG4.g1 = 0xff;
            polyG4.b1 = 0;
            polyG4.r2 = 0;
            polyG4.g2 = 0xff;
            polyG4.b2 = 0xff;
            polyG4.r3 = 0;
            polyG4.g3 = 0;
            polyG4.b3 = 0xff;
            polyG4.y3 = (short)(_gameEngine.StaticVariables.UIBoxHud.Y + 0x20);

            //p = (POLY_G4*)((int)&sprt->tag + local_38);
            //SetPolyG4(p);
            //SetSemiTrans(p, 1);
            //SetShadeTex(p, 1);

            index += 1;
        } while (index < 2);
    }

    //8004bea4
    //Affichage hud
    public void Fun_8004bea4(CallBackInfo callBackInfo)
    {
        int value;
        int sVar2;
        int iVar2;
        byte pbVar4;
        int index;
        int iVar6;
        
        if (_gameEngine.StaticVariables.g_drawFrameFlags != 0)
        {
            if ((_gameEngine.StaticVariables.g_drawFrameFlags & 6U) != 0)
            {
                value = _gameEngine.UIManager.UpdateUiBoxesPosition(_gameEngine.StaticVariables.UIBoxHud, _gameEngine.StaticVariables.g_textToDisplay_hud);
                
                if (value == 1)
                {
                    if ((_gameEngine.StaticVariables.g_drawFrameFlags & 2U) != 0)
                    {
                        _gameEngine.StaticVariables.g_drawFrameFlags = 0;
                    }
                    if ((_gameEngine.StaticVariables.g_drawFrameFlags & 4U) != 0)
                    {
                        _gameEngine.StaticVariables.g_drawFrameFlags &= 0xfffffffb;
                    }
                }

                index = 0;

                do
                {
                    var sprt = _gameEngine.StaticVariables.g_MpIconsSprites[index];
                    sprt.x0 = (short)(_gameEngine.StaticVariables.UIBoxHud.X + 0x90 + index * 8);
                    sprt.y0 = _gameEngine.StaticVariables.UIBoxHud.Y;
                    index += 1;
                } while (index < 5);

                index = 0;

                do
                {
                    var sprt = _gameEngine.StaticVariables.g_lifeBigIconSprites[index];
                    sprt.x0 = (short)(_gameEngine.StaticVariables.UIBoxHud.X + 0x50 + index * 8);
                    sprt.y0 = _gameEngine.StaticVariables.UIBoxHud.Y;
                    index += 1;
                } while (index < 5);

                var index2 = 0;
                index = 0;

                do
                {
                    do
                    {
                        var sprt = _gameEngine.StaticVariables.g_lifeSmallIconSprites[index];
                        sprt.x0 = (short)(_gameEngine.StaticVariables.UIBoxHud.X + 0x78 + index * 8);
                        sprt.y0 = (short)(_gameEngine.StaticVariables.UIBoxHud.Y + index2 * 8);
                        index += 1; 
                    } while (index < 6);

                    index2 += 1;
                } while (index2 < 2);

                index = 0;

                do
                {
                    var sprt = _gameEngine.StaticVariables.g_MpIconSprites[index];
                    sprt.x0 = (short)(_gameEngine.StaticVariables.UIBoxHud.X + 0xd8 + index * 8);
                    sprt.y0 = _gameEngine.StaticVariables.UIBoxHud.Y;
                    index += 1;
                } while (index < 4);

                index = 0;
                    
                do
                {
                    var sprt = _gameEngine.StaticVariables.g_HudMoneySprites[index];
                    sprt.x0 = (short)(_gameEngine.StaticVariables.UIBoxHud.X + 0x100 + index * 8);
                    sprt.y0 = _gameEngine.StaticVariables.UIBoxHud.Y;
                    index += 1;
                } while (index < 5);

                index = 0;

                do
                {
                    var polyG4 = _gameEngine.StaticVariables.g_hudBackgroundWeaponAndItemPolyG4s[index];
                    var offset = _gameEngine.StaticVariables.g_inventoryWeaponIconX[8 + index];

                    polyG4.x0 = (short)(_gameEngine.StaticVariables.UIBoxHud.X + offset);
                    polyG4.y0 = _gameEngine.StaticVariables.UIBoxHud.Y;
                    polyG4.x1 = (short)(offset + 0x18);
                    polyG4.y1 = _gameEngine.StaticVariables.UIBoxHud.Y;
                    polyG4.x2 = offset;
                    polyG4.y2 = (short)(_gameEngine.StaticVariables.UIBoxHud.Y + 0x20);
                    polyG4.x3 = (short)(offset + 0x18);
                    polyG4.y3 = (short)(_gameEngine.StaticVariables.UIBoxHud.Y + 0x20);

                    index += 1;
                } while (index < 2);
            }

            _gameEngine.StaticVariables.INT_800a827c += 1;
            DisplayMpWithIcons();
            FUN_8004c7d0();
            FUN_8004cb00();
            FUN_8004d3cc();
            FUN_8004d7fc();
            //_gameEngine.PlayerManager.GetPlayerHp();
            value = _gameEngine.PlayerManager.GetPlayerHpMax();

            if ((_gameEngine.StaticVariables.INT_800a827c & 1U) == 0)
            {
                sVar2 = _gameEngine.PlayerManager.GetPlayerHp();

                if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[1] < sVar2)
                {
                    sVar2 = _gameEngine.StaticVariables.INT_ARRAY_800a8284[1];
                }

                if (sVar2 < _gameEngine.StaticVariables.INT_ARRAY_800a8284[0] 
                    || (_gameEngine.StaticVariables.INT_ARRAY_800a8284[0] == sVar2 
                        && _gameEngine.StaticVariables.INT_ARRAY_800a8284[5] != 0))
                {
                    if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[5] == 0)
                    {
                        _gameEngine.StaticVariables.INT_ARRAY_800a8284[0] += -1;
                    }
                    
                    _gameEngine.StaticVariables.INT_ARRAY_800a8284[5] += 1;
                    
                    if (3 < _gameEngine.StaticVariables.INT_ARRAY_800a8284[5])
                    {
                        _gameEngine.StaticVariables.INT_ARRAY_800a8284[5] = 0;
                    }
                }
                else if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[0] < sVar2)
                {
                    _gameEngine.StaticVariables.INT_ARRAY_800a8284[5] += -1;
                    
                    if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[5] < 0)
                    {
                        _gameEngine.StaticVariables.INT_ARRAY_800a8284[5] = 3;
                    }
                    else if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[5] == 0)
                    {
                        _gameEngine.StaticVariables.INT_ARRAY_800a8284[0] += 1;
                        _gameEngine.SoundManager.PlaySoundEffect(8);
                    }
                }
                else if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[1] < value)
                {
                    _gameEngine.StaticVariables.INT_ARRAY_800a8284[6] += -1;
                    
                    if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[6] < 0)
                    {
                        _gameEngine.StaticVariables.INT_ARRAY_800a8284[6] = 3;
                    }
                    else if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[6] == 0)
                    {
                        _gameEngine.StaticVariables.INT_ARRAY_800a8284[0] = _gameEngine.StaticVariables.INT_ARRAY_800a8284[1] + 1;
                        _gameEngine.StaticVariables.INT_ARRAY_800a8284[1] += 1;
                    }
                }
                
                value = _gameEngine.PlayerManager.GetPlayerMp();
                
                if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[3] < value)
                {
                    value = _gameEngine.StaticVariables.INT_ARRAY_800a8284[3];
                }
                
                if (value < _gameEngine.StaticVariables.INT_ARRAY_800a8284[2] ||
                   (_gameEngine.StaticVariables.INT_ARRAY_800a8284[2] == value && _gameEngine.StaticVariables.INT_ARRAY_800a8284[7] != 0))
                {
                    if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[7] == 0)
                    {
                        _gameEngine.StaticVariables.INT_ARRAY_800a8284[2] += -1;
                    }
                    _gameEngine.StaticVariables.INT_ARRAY_800a8284[7] += 1;
                    
                    if (3 < _gameEngine.StaticVariables.INT_ARRAY_800a8284[7])
                    {
                        _gameEngine.StaticVariables.INT_ARRAY_800a8284[7] = 0;
                    }
                }
                else if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[2] < value)
                {
                    _gameEngine.StaticVariables.INT_ARRAY_800a8284[7] += -1;
                    
                    if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[7] < 0)
                    {
                        _gameEngine.StaticVariables.INT_ARRAY_800a8284[7] = 3;
                    }
                    else if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[7] == 0)
                    {
                        _gameEngine.StaticVariables.INT_ARRAY_800a8284[2] += 1;
                        _gameEngine.SoundManager.PlaySoundEffect(9);
                    }
                }
                else
                {
                    value = _gameEngine.PlayerManager.GetPlayerMpMax();
                    
                    if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[3] < value)
                    {
                        _gameEngine.StaticVariables.INT_ARRAY_800a8284[8] += -1;
                        
                        if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[8] < 0)
                        {
                            _gameEngine.StaticVariables.INT_ARRAY_800a8284[8] = 3;
                        }
                        else if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[8] == 0)
                        {
                            _gameEngine.StaticVariables.INT_ARRAY_800a8284[2] = _gameEngine.StaticVariables.INT_ARRAY_800a8284[3] + 1;
                            _gameEngine.StaticVariables.INT_ARRAY_800a8284[3] = _gameEngine.StaticVariables.INT_ARRAY_800a8284[2];
                        }
                    }
                }
            }

            value = _gameEngine.PlayerManager.GetMoney();

            if (value < _gameEngine.StaticVariables.INT_ARRAY_800a8284[4] 
                || (_gameEngine.StaticVariables.INT_ARRAY_800a8284[4] == value 
                        && _gameEngine.StaticVariables.INT_ARRAY_800a8284[9] != 0))
            {
                index = _gameEngine.StaticVariables.INT_ARRAY_800a8284[4] + -10;
                
                if (value < index)
                {
                    _gameEngine.StaticVariables.INT_ARRAY_800a8284[4] += -10;
                }
                else
                {
                    if (value < _gameEngine.StaticVariables.INT_ARRAY_800a8284[4])
                    {
                        _gameEngine.StaticVariables.INT_ARRAY_800a8284[4] += -1;
                    }
                }
            }
            else
            {
                if (value <= _gameEngine.StaticVariables.INT_ARRAY_800a8284[4])
                {
                    _gameEngine.StaticVariables.INT_ARRAY_800a8284[9] = 0;
                    return;
                }
                
                index = _gameEngine.StaticVariables.INT_ARRAY_800a8284[4] + 10;

                if (index < value)
                {
                    _gameEngine.StaticVariables.INT_ARRAY_800a8284[4] += 10;
                }
                else
                {
                    _gameEngine.StaticVariables.INT_ARRAY_800a8284[4] += 1;
                }
            }

            if (_gameEngine.StaticVariables.INT_800a827c == _gameEngine.StaticVariables.INT_800a827c / 6 * 6)
            {
                _gameEngine.StaticVariables.INT_ARRAY_800a8284[9] += 1;

                if (3 < _gameEngine.StaticVariables.INT_ARRAY_800a8284[9])
                {
                    _gameEngine.StaticVariables.INT_ARRAY_800a8284[9] = 0;
                }
            }
        }
    }

    //8004d7fc
    private void FUN_8004d7fc()
    {
        SPRT pSVar1;
        SPRT pSVar2;
        uint uVar4;
        uint uVar5;
        uint uVar6;
        uint uVar7;
        uint uVar8;
        uint puVar9;
        uint puVar10;
        uint puVar11;

        var weaponId = _gameEngine.PlayerManager.GetItemIdFromCurrentWeapon();

        if (weaponId != 0xffffffff)
        {
            var sprite = _gameEngine.StaticVariables.SPRT_ARRAY_801762b8[0];

            _gameEngine.GraphicManager.InitializeSpriteWithImage(
                sprite, 
                (int)weaponId,
                _gameEngine.StaticVariables.g_inventoryWeaponIconX[8],
                _gameEngine.StaticVariables.UIBoxHud.Y);

            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

            //pSVar1 = _gameEngine.StaticVariables.g_hudBackgroundWeaponAndItemPolyG4s[7];
            //pSVar1->r0 = (char)uVar8;
            //pSVar1->g0 = (char)(uVar8 >> 8);
            //pSVar1->b0 = (char)(uVar8 >> 0x10);
            ////pSVar1->code = (char)(uVar8 >> 0x18);

            /* Probable PsyQ macro: addPrim(). */
            //uVar8 = _gameEngine.StaticVariables.UINT_ARRAY_80146f68 + 2;
            //pSVar2->u0 = (char)uVar8;
            //pSVar2->v0 = (char)(uVar8 >> 8);
            //pSVar2->clut = (short)(uVar8 >> 0x10);
            //uVar7._0_1_ = pSVar1->r0;
            //uVar7._1_1_ = pSVar1->g0;
            //uVar7._2_1_ = pSVar1->b0;
            //uVar7._3_1_ = pSVar1->code;
            //uVar8._0_1_ = pSVar2->u0;
            //uVar8._1_1_ = pSVar2->v0;
            //uVar8._2_2_ = pSVar2->clut;
            //uVar8 = uVar7 & 0xff000000 | uVar8 & 0xffffff;
            //uVar4._0_1_ = pSVar2->u0;
            //uVar4._1_1_ = pSVar2->v0;
            //uVar4._2_2_ = pSVar2->clut;
            //uVar8 = uVar4 & 0xff000000 | (uint)&pSVar1->r0 & 0xffffff;
        }

        weaponId = _gameEngine.PlayerManager.SetItemIdFromCurrentItemId();

        if (weaponId != 0xffffffff)
        {
            var sprite = _gameEngine.StaticVariables.SPRT_ARRAY_801762e8[0];
            _gameEngine.GraphicManager.InitializeSpriteWithImage(
                sprite, 
                (int)weaponId,
                _gameEngine.StaticVariables.g_inventoryWeaponIconX[9], 
                _gameEngine.StaticVariables.UIBoxHud.Y);

            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

            //puVar9 = (uint*)(&DAT_801762e8 +  0x14);
            //pSVar1 = &SPRT_80146f5c +  2;
            //pSVar1->u0 = (char)uVar8;
            //pSVar1->v0 = (char)(uVar8 >> 8);
            //pSVar1->clut = (short)(uVar8 >> 0x10);

            /* Probable PsyQ macro: addPrim(). */
            //uVar5._0_1_ = pSVar1->u0;
            //uVar5._1_1_ = pSVar1->v0;
            //uVar5._2_2_ = pSVar1->clut;
            //uVar6._0_1_ = pSVar1->u0;
            //uVar6._1_1_ = pSVar1->v0;
            //uVar6._2_2_ = pSVar1->clut;
            //*puVar9 = *puVar9 & 0xff000000 | uVar5 & 0xffffff;
            //uVar8 = uVar6 & 0xff000000 | (uint)puVar9 & 0xffffff;
        }

        {
            var polyG4 = _gameEngine.StaticVariables.g_hudBackgroundWeaponAndItemPolyG4s[0];
            _gameEngine.Renderer.AddQuadColor(polyG4, SpriteDepth.BackgroundUI);

            polyG4 = _gameEngine.StaticVariables.g_hudBackgroundWeaponAndItemPolyG4s[1];
            _gameEngine.Renderer.AddQuadColor(polyG4, SpriteDepth.BackgroundUI);
        }

        //puVar11 = (uint*)((int)g_hudBackgroundWeaponAndItemPolyG4s +  0x48);
        //puVar9 = (uint*)((int)g_drawModes +  0x28 + 0xf8);
        //puVar10 = (uint*)((int)g_hudBackgroundWeaponAndItemPolyG4s +  0x48 + 0x24);
        ///* Probable PsyQ macro: addPrim(). */
        //*puVar11 = *puVar11 & 0xff000000 | *puVar9 & 0xffffff;
        //*puVar9 = *puVar9 & 0xff000000 | (uint)puVar11 & 0xffffff;
        //*puVar10 = *puVar10 & 0xff000000 | (uint)puVar11 & 0xffffff;
        //*puVar9 = *puVar9 & 0xff000000 | (uint)puVar10 & 0xffffff;
    }

    //8004d3cc
    private void FUN_8004d3cc()
    {
        int iVar1;
        int maxCount;
        uint puVar3;
        int iVar4;
        uint puVar5;
        SPRT pSVar6;
        int i;
        int nbNumbers;
        SPRT pSVar10;

        nbNumbers = _gameEngine.StaticVariables.INT_ARRAY_800a8284[2];
        maxCount = _gameEngine.StaticVariables.INT_ARRAY_800a8284[3];

        if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[7] != 0 
            || _gameEngine.StaticVariables.INT_ARRAY_800a8284[8] != 0)
        {
            nbNumbers = _gameEngine.StaticVariables.INT_ARRAY_800a8284[2] + 1;
        }

        i = 0;

        //if (0 < nbNumbers)
        //{
        //    do
        //    {
        //        var sprt = _gameEngine.StaticVariables.g_MpIconSprites[i];
        //
        //        iVar1 = _gameEngine.StaticVariables.INT_800a827c / 10;
        //        iVar4 = iVar1;
        //
        //        if (iVar1 < 0)
        //        {
        //            iVar4 = iVar1 + 3;
        //        }
        //
        //        var index = i * 4 + iVar1 + (iVar4 >> 2) * -4;
        //        sprt.u0 = _gameEngine.StaticVariables.DAT_800a3238[_gameEngine.StaticVariables.PTR_BYTE_ARRAY_800a8280[index]];
        //        iVar1 = _gameEngine.StaticVariables.INT_800a827c / 10;
        //        iVar4 = iVar1;
        //
        //        if (iVar1 < 0)
        //        {
        //            iVar4 = iVar1 + 3;
        //        }
        //
        //        sprt.v0 = _gameEngine.StaticVariables.DAT_800a3238[_gameEngine.StaticVariables.PTR_BYTE_ARRAY_800a8280[index + 1]];
        //        
        //        /* Probable PsyQ macro: addPrim(). */
        //        //puVar3 = _gameEngine.StaticVariables.g_MpIconSprites[4].tag + index;
        //        //puVar5 = _gameEngine.StaticVariables.g_drawModes +  0x28 + 0xf8;
        //        //*puVar3 = *puVar3 & 0xff000000 | *puVar5 & 0xffffff;
        //        //*puVar5 = *puVar5 & 0xff000000 | (uint)pSVar6 & 0xffffff;
        //
        //        i++;
        //    } while (i < nbNumbers);
        //}
        //
        //if (nbNumbers < maxCount)
        //{
        //    var index = nbNumbers * 0x14;
        //    pSVar10 = _gameEngine.StaticVariables.g_MpIconSprites + nbNumbers;
        //    i = nbNumbers;
        //    do
        //    {
        //        _gameEngine.StaticVariables.g_MpIconSprites[ 4].u0[index] = _gameEngine.StaticVariables.DAT_800a3314;
        //        _gameEngine.StaticVariables.g_MpIconSprites[ 4].v0[index] = _gameEngine.StaticVariables.DAT_800a3315;
        //        i += 1;
        //        pSVar6 = pSVar10 +  4;
        //        pSVar10 += 1;
        //        /* Probable PsyQ macro: addPrim(). */
        //        //puVar3 = _gameEngine.StaticVariables.g_MpIconSprites[4].tag + index);
        //        //puVar5 = (uint*)((int)g_drawModes + 0x28 + 0xf8);
        //        //*puVar3 = *puVar3 & 0xff000000 | *puVar5 & 0xffffff;
        //        //*puVar5 = *puVar5 & 0xff000000 | (uint)pSVar6 & 0xffffff;
        //
        //        index += 0x14;
        //    } while (i < maxCount);
        //}
        //
        //nbNumbers += -1;
        //
        //if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[7] == 0)
        //{
        //    if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[8] == 0)
        //    {
        //        return;
        //    }
        //
        //    _gameEngine.StaticVariables.g_MpIconSprites[ 4 + nbNumbers].u0 = _gameEngine.StaticVariables.DAT_800a3238[(_gameEngine.StaticVariables.INT_ARRAY_800a8284[8] + 8) * 0x14];
        //    nbNumbers = nbNumbers * 0x14 +  0x50;
        //    maxCount = _gameEngine.StaticVariables.INT_ARRAY_800a8284[8] + 8;
        //}
        //else
        //{
        //    _gameEngine.StaticVariables.g_MpIconSprites[ 4 + nbNumbers].u0 = _gameEngine.StaticVariables.DAT_800a3238[(_gameEngine.StaticVariables.INT_ARRAY_800a8284[7] + 4) * 0x14];
        //    nbNumbers = nbNumbers * 0x14 +  0x50;
        //    maxCount = _gameEngine.StaticVariables.INT_ARRAY_800a8284[7] + 4;
        //}
        //
        //_gameEngine.StaticVariables.g_MpIconSprites[0].v0)[nbNumbers] = _gameEngine.StaticVariables.DAT_800a3239[maxCount * 0x14];
    }

    //8004cb00
    private void FUN_8004cb00()
    {
        int iVar1;
        int iVar2;
        int iVar3;
        uint puVar4;
        int iVar5;
        int iVar6;
        int iVar7;
        int iVar8;

        iVar6 = _gameEngine.StaticVariables.INT_ARRAY_800a8284[0];

        if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[5] != 0 
            || _gameEngine.StaticVariables.INT_ARRAY_800a8284[6] != 0)
        {
            iVar6 = _gameEngine.StaticVariables.INT_ARRAY_800a8284[0] + 1;
        }

        iVar7 = iVar6 / 10;
        iVar8 = iVar6 % 10;

        if (9 < iVar6 && iVar8 == 0)
        {
            iVar7 += -1;
            iVar8 = 10;
        }

        iVar5 = 0;

        //life big cristal
        if (0 < iVar7)
        {
            do
            {
                iVar1 = 3 - iVar5;
                var sprite = _gameEngine.StaticVariables.g_lifeBigIconSprites[iVar1];
                sprite.u0 = _gameEngine.StaticVariables.BYTE_800a0d60;
                sprite.v0 = _gameEngine.StaticVariables.BYTE_800a0d61;

                var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

                iVar5 += 1;
                iVar2 =  5;
                //_gameEngine.StaticVariables.g_lifeBigIconSprites[iVar1 + 5].tag = _gameEngine.StaticVariables.g_lifeBigIconSprites[iVar1 + 5].tag & 0xff000000 | *puVar4 & 0xffffff;
                /* Probable PsyQ macro: addPrim(). */
                //puVar4 = (uint*)((int)g_drawModes + 0x28 + 0xf8);
                //*puVar4 = *puVar4 & 0xff000000 | (uint)(g_lifeBigIconSprites + iVar1 + iVar2) & 0xffffff;
            } while (iVar5 < iVar7);
        }

        iVar2 = _gameEngine.PlayerManager.GetPlayerHpMax();

        for (; iVar5 < (iVar2 + -1) / 10; iVar5 += 1)
        {
            iVar1 = 3 - iVar5;
            var sprite = _gameEngine.StaticVariables.g_lifeBigIconSprites[iVar1];
            sprite.u0 = _gameEngine.StaticVariables.BYTE_800a0dd8;
            sprite.v0 = _gameEngine.StaticVariables.BYTE_800a0dd9;

            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

            iVar2 =  5;
            //_gameEngine.StaticVariables.g_lifeBigIconSprites[iVar1 + 5].tag = _gameEngine.StaticVariables.g_lifeBigIconSprites[iVar1 + 5].tag & 0xff000000 | *puVar4 & 0xffffff;
            /* Probable PsyQ macro: addPrim(). */
            //puVar4 = (uint*)((int)g_drawModes + 0x28 + 0xf8);
            //*puVar4 = *puVar4 & 0xff000000 | (uint)(g_lifeBigIconSprites + iVar1 + iVar2) & 0xffffff;
        }

        iVar5 = 0;

        if (0 < iVar8)
        {
            do
            {
                iVar2 = iVar5 / 5;
                iVar3 = iVar5 % 5;
                var sprite = _gameEngine.StaticVariables.g_lifeSmallIconSprites[iVar2 * 6 + iVar3];
                sprite.u0 = 0x70; //_gameEngine.StaticVariables.BYTE_800a2030;
                sprite.v0 = 0x38; //_gameEngine.StaticVariables.BYTE_800a2031;

                var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
                _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

                iVar5 += 1;
                iVar1 =  0xc;
                //_gameEngine.StaticVariables.g_lifeSmallIconSprites[iVar2 * 6 + 0xc + iVar3].tag = _gameEngine.StaticVariables.g_lifeSmallIconSprites[iVar2 * 6 + 0xc + iVar3].tag & 0xff000000 | *puVar4 & 0xffffff;
                /* Probable PsyQ macro: addPrim(). */
                //puVar4 = (uint*)((int)g_drawModes + 0x28 + 0xf8);
                //*puVar4 = *puVar4 & 0xff000000 | (uint)(g_lifeSmallIconSprites + iVar2 * 6 + iVar1 + iVar3) & 0xffffff;
            } while (iVar5 < iVar8);
        }

        for (; iVar5 < 10; iVar5 += 1)
        {
            iVar2 = iVar5 / 5;
            iVar3 = iVar5 % 5;
            var sprite = _gameEngine.StaticVariables.g_lifeSmallIconSprites[iVar2 * 6 + iVar3];
            sprite.u0 = _gameEngine.StaticVariables.BYTE_800a206c;
            sprite.v0 = _gameEngine.StaticVariables.BYTE_800a206d;

            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

            iVar1 =  0xc;
            //_gameEngine.StaticVariables.g_lifeSmallIconSprites[iVar2 * 6 + 0xc + iVar3].tag = _gameEngine.StaticVariables.g_lifeSmallIconSprites[iVar2 * 6 + 0xc + iVar3].tag & 0xff000000 | *puVar4 & 0xffffff;
            /* Probable PsyQ macro: addPrim(). */
            //puVar4 = (uint*)((int)g_drawModes + 0x28 + 0xf8);
            //*puVar4 = *puVar4 & 0xff000000 | (uint)(g_lifeSmallIconSprites + iVar2 * 6 + iVar1 + iVar3) & 0xffffff;
        }

        //if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[5] == 0)
        //{
        //    if (_gameEngine.StaticVariables.INT_ARRAY_800a8284[6] != 0)
        //    {
        //        if (iVar8 == 1)
        //        {
        //            _gameEngine.StaticVariables.g_lifeBigIconSprites[ 5 + (4 - iVar7)].u0 = _gameEngine.StaticVariables.BYTE_800a0d60[(_gameEngine.StaticVariables.INT_ARRAY_800a8284[6] + 4) * 0x28];
        //            _gameEngine.StaticVariables.g_lifeBigIconSprites[ 5 + (4 - iVar7)].v0 = _gameEngine.StaticVariables.BYTE_800a0d61[(_gameEngine.StaticVariables.INT_ARRAY_800a8284[6] + 4) * 0x28];
        //        }
        //        iVar6 = (iVar6 + -1) % 10;
        //        _gameEngine.StaticVariables.g_lifeSmallIconSprites[ 0xc + iVar6 / 5 + iVar6].u0 = _gameEngine.StaticVariables.BYTE_800a2030[(_gameEngine.StaticVariables.INT_ARRAY_800a8284[6] + 4) * 0x14];
        //        _gameEngine.StaticVariables.g_lifeSmallIconSprites[ 0xc + iVar6 / 5 + iVar6].v0 = _gameEngine.StaticVariables.BYTE_800a2031[(_gameEngine.StaticVariables.INT_ARRAY_800a8284[6] + 4) * 0x14];
        //    }
        //}
        //else
        //{
        //    if (iVar8 == 1)
        //    {
        //        _gameEngine.StaticVariables.g_lifeBigIconSprites[ 5 + (4 - iVar7)].u0 = _gameEngine.StaticVariables.BYTE_800a0d60[_gameEngine.StaticVariables.INT_ARRAY_800a8284[5] * 0x28];
        //        _gameEngine.StaticVariables.g_lifeBigIconSprites[ 5 + (4 - iVar7)].v0 = _gameEngine.StaticVariables.BYTE_800a0d61[_gameEngine.StaticVariables.INT_ARRAY_800a8284[5] * 0x28];
        //    }
        //
        //    iVar6 = (iVar6 + -1) % 10;
        //    _gameEngine.StaticVariables.g_lifeSmallIconSprites[ 0xc + iVar6 / 5 + iVar6].u0 = _gameEngine.StaticVariables.BYTE_800a2030[_gameEngine.StaticVariables.INT_ARRAY_800a8284[5] * 0x14];
        //    _gameEngine.StaticVariables.g_lifeSmallIconSprites[ 0xc + iVar6 / 5 + iVar6].v0 = _gameEngine.StaticVariables.BYTE_800a2031[_gameEngine.StaticVariables.INT_ARRAY_800a8284[5] * 0x14];
        //}
    }

    //8004c7d0
    private void FUN_8004c7d0()
    {
        //int iVar1;
        //u_long uVar2;
        //int iVar3;
        //int iVar4;
        //SPRT* pSVar5;
        //uint* puVar6;
        //SPRT* pSVar7;
        //uint* puVar8;
        //
        //iVar1 = _gameEngine.StaticVariables.INT_ARRAY_800a8284[4];
        //pSVar7 = _gameEngine.StaticVariables.g_HudMoneySprites;
        //iVar4 = _gameEngine.StaticVariables.INT_ARRAY_800a8284[4] / 1000;
        //iVar3 = iVar4 % 10 * 0x14;
        //_gameEngine.StaticVariables.g_HudMoneySprites[ 5].u0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar3];
        //_gameEngine.StaticVariables.g_HudMoneySprites[ 5].v0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar3 + 1];
        //iVar4 = (iVar1 / 100 + iVar4 * -10) * 0x14;
        //_gameEngine.StaticVariables.g_HudMoneySprites[ 5 + 1].u0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar4];
        //_gameEngine.StaticVariables.g_HudMoneySprites[ 5 + 1].v0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar4 + 1];
        //iVar3 = 0;
        //iVar4 = (iVar1 / 10 + iVar1 / 100 * -10) * 0x14;
        //_gameEngine.StaticVariables.g_HudMoneySprites[ 5 + 2].u0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar4];
        //_gameEngine.StaticVariables.g_HudMoneySprites[ 5 + 2].v0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar4 + 1];
        //iVar4 = iVar1 % 10 * 0x14;
        //_gameEngine.StaticVariables.g_HudMoneySprites[ 5 + 3].u0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar4];
        //_gameEngine.StaticVariables.g_HudMoneySprites[ 5 + 3].v0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar4 + 1];
        //_gameEngine.StaticVariables.g_HudMoneySprites[ 5 + 4].u0 = _gameEngine.StaticVariables.DAT_8009fb80[_gameEngine.StaticVariables.INT_ARRAY_800a8284[9] * 0x14];
        //_gameEngine.StaticVariables.g_HudMoneySprites[ 5 + 4].v0 = _gameEngine.StaticVariables.DAT_8009fb81[_gameEngine.StaticVariables.INT_ARRAY_800a8284[9] * 0x14];
        //uVar2 = g_drawModes[0x14].tag;
        //puVar8 = (uint*)((int)g_drawModes +  0x28 + 0xf8);
        //iVar4 =  100;
        //
        //do
        //{
        //    //pSVar5 = pSVar7 + uVar2 * 5;
        //    //pSVar7 += 1;
        //    //puVar6 = (uint*)((int)&g_HudMoneySprites[0].tag + iVar4);
        //    //iVar3 += 1;
        //    ///* Probable PsyQ macro: addPrim(). */
        //    //*puVar6 = *puVar6 & 0xff000000 | *puVar8 & 0xffffff;
        //    //*puVar8 = *puVar8 & 0xff000000 | (uint)pSVar5 & 0xffffff;
        //    //iVar4 += 0x14;
        //} while (iVar3 < 5);
    }

    //8004c5d4
    //display magic crystals
    private void DisplayMpWithIcons()
    {
        int iVar2;
        int iVar3;
        SPRT pSVar4;
        uint puVar5;
        SPRT pSVar6;
        uint puVar7;
        int i = 0;

        iVar3 = _gameEngine.StaticVariables.INT_ARRAY_800a8284[1];
        iVar2 = iVar3 / 10 % 10;

        _gameEngine.StaticVariables.g_MpIconsSprites[3].u0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar2];
        _gameEngine.StaticVariables.g_MpIconsSprites[3].v0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar2 + 1];

        iVar3 = iVar3 % 10;

        _gameEngine.StaticVariables.g_MpIconsSprites[4].u0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar3];
        _gameEngine.StaticVariables.g_MpIconsSprites[4].v0 = _gameEngine.StaticVariables.BYTE_ARRAY_8009cfd8[iVar3 + 1];

        _gameEngine.StaticVariables.g_MpIconsSprites[2].u0 = _gameEngine.StaticVariables.DAT_8009d0a0; //0x50
        _gameEngine.StaticVariables.g_MpIconsSprites[2].v0 = _gameEngine.StaticVariables.DAT_8009d0a1; //0x28
        //uVar1 = g_drawModes[0x14].tag;
        //puVar7 = (uint*)((int)g_drawModes + 0x28 + 0xf8);
        //iVar3 = 100;

        do
        {
            var sprite = _gameEngine.StaticVariables.g_MpIconsSprites[i];
            var bitmap = _gameEngine.Font3.GenerateHudBitmapFromSprite(sprite);
            _gameEngine.Renderer.AddSprite(sprite, SpriteDepth.ForegroundUI, bitmap);

            //pSVar4 = pSVar6 + uVar1 * 5;
            //pSVar6 += 1;
            //puVar5 = (uint*)((int)&g_MpIconsSprites[0].tag + iVar3);
            ///* Probable PsyQ macro: addPrim(). */
            //*puVar5 = *puVar5 & 0xff000000 | *puVar7 & 0xffffff;
            //*puVar7 = *puVar7 & 0xff000000 | (uint)pSVar4 & 0xffffff;
            //iVar3 += 0x14;
            i += 1;
        } while (i < 5);
    }
}