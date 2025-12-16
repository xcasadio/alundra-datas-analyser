using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Diagnostics;
using System.Security.Cryptography;

namespace AlundraEngine;

public class MemoryCardManager
{
    private readonly GameEngine _gameEngine;

    public MemoryCardManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }


    //8005ec98
    public int UpdateMemoryCardProcess()
    {
        var result = _gameEngine.StaticVariables.g_globalTransitionState == 1;

        if (_gameEngine.StaticVariables.g_globalTransitionState != 0)
        {
            result = _gameEngine.StaticVariables.g_postProcessingState < 3;

            if (_gameEngine.StaticVariables.g_postProcessingState == 2)
            {
                StartMemoryCardProcess();
            }
            else if (_gameEngine.StaticVariables.g_postProcessingState == 0)
            {
                result = true;

                if (_gameEngine.StaticVariables.g_postProcessingState == 3)
                {
                    result = UpdateSaveGameTransition() != 0;
                }
            }
            else
            {
                result = true;

                if (_gameEngine.StaticVariables.g_postProcessingState == 1)
                {
                    StartMemoryCardProcess();
                }
            }
        }

        return result ? 1 : 0;
    }

    //8005f458
    private void StartMemoryCardProcess()
    {
        string arg1;
        string arg2;
        byte[] pbVar1;
        int etcSectionA;
        byte[] pbVar4;
        int fadeCounter;
        int iVar5;
        int[] piVar6;
        int[] local_28 = new int[2];
        bool isFadeComplete;

        fadeCounter = 0;
        if (_gameEngine.StaticVariables.g_globalTransitionState == 0x6b)
        {
            if (_gameEngine.StaticVariables.g_asyncOperationResult == 1)
            {
                _gameEngine.StaticVariables.g_globalTransitionState = 0x3f5;
            }
            else
            {
                if (_gameEngine.StaticVariables.g_asyncOperationResult < 2)
                {
                    return;
                }

                if (_gameEngine.StaticVariables.g_asyncOperationResult != 2)
                {
                    return;
                }
                _gameEngine.StaticVariables.g_globalTransitionState = 0x3ed;
            }

            //FADE_RESTART:
            _gameEngine.StaticVariables.g_asyncOperationResult = 0;
            ResetMemoryCardMenuState();
            _gameEngine.StaticVariables.g_fadeSubstate = 1;
            return;
        }

        if (0x6b < _gameEngine.StaticVariables.g_globalTransitionState)
        {
            if (_gameEngine.StaticVariables.g_globalTransitionState == 0x3f6)
            {
                if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                {
                    ResetMemoryCardMenuState();
                    _gameEngine.StaticVariables.g_fadeSubstate = 1;
                }

                if (_gameEngine.StaticVariables.g_fadeFrame < 0x13)
                {
                    _gameEngine.StaticVariables.g_fadeFrame += 1;
                    return;
                }

                etcSectionA = 0xe;
            }
            else if (_gameEngine.StaticVariables.g_globalTransitionState < 0x3f7)
            {
                if (_gameEngine.StaticVariables.g_globalTransitionState == 0x3ef)
                {
                    if (_gameEngine.StaticVariables.g_fadeControlValue == 1000)
                    {
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                    }

                    if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & 0x80) != 0)
                    {
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                    }

                    if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                    {
                        return;
                    }

                    if (_gameEngine.StaticVariables.g_fadeFrame < 0x13)
                    {
                        _gameEngine.StaticVariables.g_fadeFrame += 1;
                        return;
                    }

                    etcSectionA = 7;
                }
                else
                {
                    if (_gameEngine.StaticVariables.g_globalTransitionState < 0x3f0)
                    {
                        if (_gameEngine.StaticVariables.g_globalTransitionState == 0x3ec)
                        {
                            _gameEngine.StaticVariables.g_globalTransitionState = 4;
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_globalTransitionState < 0x3ed)
                        {
                            if (_gameEngine.StaticVariables.g_globalTransitionState == 0x3ea)
                            {
                                _gameEngine.StaticVariables.g_globalTransitionState = 2;
                                return;
                            }

                            if (_gameEngine.StaticVariables.g_globalTransitionState == 0x3eb)
                            {
                                _gameEngine.StaticVariables.g_globalTransitionState = 3;
                                return;
                            }

                            return;
                        }

                        if (_gameEngine.StaticVariables.g_globalTransitionState == 0x3ed)
                        {
                            if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                            {
                                ResetMemoryCardMenuState();
                                _gameEngine.StaticVariables.g_fadeSubstate = 1;
                            }

                            if (0x12 < _gameEngine.StaticVariables.g_fadeFrame)
                            {
                                _gameEngine.StaticVariables.g_fadeFrame = 0;
                                _gameEngine.StaticVariables.g_globalTransitionState = 5;
                                return;
                            }

                            _gameEngine.StaticVariables.g_fadeFrame += 1;
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_globalTransitionState != 0x3ee)
                        {
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_fadeSubstate == 1)
                        {
                            isFadeComplete = 0x1b < _gameEngine.StaticVariables.g_fadeFrame;
                            _gameEngine.StaticVariables.g_fadeFrame += 1;

                            if (isFadeComplete)
                            {
                                _gameEngine.StaticVariables.g_fadeFrame = 0;
                                _gameEngine.StaticVariables.g_fadeSubstate = 2;
                            }
                        }

                        if (_gameEngine.StaticVariables.g_fadeSubstate == 2)
                        {
                            arg1 = _gameEngine.EtcRes.GetEtcString(0x91);
                            arg2 = _gameEngine.EtcRes.GetEtcString(0x92);
                            TryOpenMemoryCardMenu(arg1, arg2, ref _gameEngine.StaticVariables.g_fadeControlValue);
                            _gameEngine.StaticVariables.g_fadeSubstate = 3;
                        }

                        if (_gameEngine.StaticVariables.g_fadeSubstate == 3)
                        {
                            if (0x12 < _gameEngine.StaticVariables.g_fadeFrame)
                            {
                                _gameEngine.StaticVariables.g_fadeFrame = 0;
                                _gameEngine.StaticVariables.g_globalTransitionState = 6;
                                return;
                            }

                            _gameEngine.StaticVariables.g_fadeFrame += 1;
                            return;
                        }
                        return;
                    }

                    if (_gameEngine.StaticVariables.g_globalTransitionState != 0x3f2)
                    {
                        if (_gameEngine.StaticVariables.g_globalTransitionState < 0x3f3)
                        {
                            if (_gameEngine.StaticVariables.g_globalTransitionState == 0x3f0)
                            {
                                _gameEngine.StaticVariables.g_globalTransitionState = 8;
                                return;
                            }

                            if (_gameEngine.StaticVariables.g_globalTransitionState == 0x3f1)
                            {
                                if (_gameEngine.StaticVariables.g_fadeControlValue == 1000)
                                {
                                    _gameEngine.StaticVariables.g_fadeSubstate = 1;
                                }

                                ResetMemoryCardMenuState();

                                if (0x12 < _gameEngine.StaticVariables.g_fadeFrame)
                                {
                                    _gameEngine.StaticVariables.g_fadeFrame = 0;
                                    _gameEngine.StaticVariables.g_globalTransitionState = 9;
                                    _gameEngine.StaticVariables.g_fadeSubstate = 1;
                                    return;
                                }

                                _gameEngine.StaticVariables.g_fadeFrame += 1;
                                _gameEngine.StaticVariables.g_fadeSubstate = 1;
                                return;
                            }
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_globalTransitionState == 0x3f4)
                        {
                            if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                            {
                                SaveInMemoryCard(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.g_gameTitle, _gameEngine.StaticVariables.g_memoryCardDataBlob);
                                ResetMemoryCardMenuState();
                                _gameEngine.StaticVariables.g_fadeSubstate = 1;
                            }

                            if (0x12 < _gameEngine.StaticVariables.g_fadeFrame)
                            {
                                _gameEngine.StaticVariables.g_fadeFrame = 0;
                                _gameEngine.StaticVariables.g_globalTransitionState = 0xc;
                                return;
                            }

                            _gameEngine.StaticVariables.g_fadeFrame += 1;
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_globalTransitionState < 0x3f5)
                        {
                            if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                            {
                                FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.INT_ARRAY_80191088, _gameEngine.StaticVariables.INT_ARRAY_80191080);
                                _gameEngine.StaticVariables.g_fadeSubstate = 1;
                            }

                            if (_gameEngine.StaticVariables.g_fadeSubstate == 1)
                            {
                                ResetMemoryCardMenuState();
                                _gameEngine.StaticVariables.g_fadeSubstate = 2;
                            }

                            if (_gameEngine.StaticVariables.g_fadeSubstate == 2)
                            {
                                if (0x12 < _gameEngine.StaticVariables.g_fadeFrame)
                                {
                                    _gameEngine.StaticVariables.g_fadeFrame = 0;
                                    _gameEngine.StaticVariables.g_globalTransitionState = 0xb;
                                    return;
                                }

                                _gameEngine.StaticVariables.g_fadeFrame += 1;
                                return;
                            }
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_fadeControlValue == 1000)
                        {
                            _gameEngine.StaticVariables.g_fadeSubstate = 1;
                        }

                        if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & 0x80) != 0)
                        {
                            ResetMemoryCardMenuState();
                            _gameEngine.StaticVariables.g_fadeSubstate = 1;
                        }

                        if (_gameEngine.StaticVariables.g_fadeSubstate != 0)
                        {
                            if (0x12 < _gameEngine.StaticVariables.g_fadeFrame)
                            {
                                _gameEngine.StaticVariables.g_fadeFrame = 0;
                                _gameEngine.StaticVariables.g_globalTransitionState = 0xd;
                                return;
                            }

                            _gameEngine.StaticVariables.g_fadeFrame += 1;
                            return;
                        }
                        return;
                    }

                    if (_gameEngine.StaticVariables.g_fadeControlValue == 1000)
                    {
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                    }

                    if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressed & 0x20) != 0)
                    {
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                    }

                    if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                    {
                        return;
                    }

                    if (_gameEngine.StaticVariables.g_fadeFrame < 0x13)
                    {
                        _gameEngine.StaticVariables.g_fadeFrame += 1;
                        return;
                    }
                    etcSectionA = 10;
                }
            }
            else
            {
                if (_gameEngine.StaticVariables.g_globalTransitionState == 0x3fc)
                {
                    if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                    {
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                    }

                    fadeCounter = _gameEngine.StaticVariables.g_fadeFrame + 1;

                    if (_gameEngine.StaticVariables.g_fadeFrame < 0x13)
                    {
                        _gameEngine.StaticVariables.g_fadeFrame = fadeCounter;
                        return;
                    }

                    etcSectionA = 0x14;
                }
                else
                {
                    if (0x3fc < _gameEngine.StaticVariables.g_globalTransitionState)
                    {
                        if (_gameEngine.StaticVariables.g_globalTransitionState == 0x3ff)
                        {
                            if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                            {
                                ResetMemoryCardMenuState();
                                _gameEngine.StaticVariables.g_fadeSubstate = 1;
                            }

                            if (_gameEngine.StaticVariables.g_fadeSubstate == 1)
                            {
                                fadeCounter = _gameEngine.StaticVariables.g_fadeFrame + 1;
                                _gameEngine.StaticVariables.g_fadeFrame = fadeCounter;

                                if (0x12 < _gameEngine.StaticVariables.g_fadeFrame && _gameEngine.StaticVariables.g_memoryCardPayloadOffset != 0xffffffff)
                                {
                                    _gameEngine.StaticVariables.g_fadeFrame = 0;
                                    arg1 = _gameEngine.EtcRes.GetEtcString(0xb3);
                                    arg2 = _gameEngine.EtcRes.GetEtcString(0xb4);
                                    TryOpenMemoryCardMenu(arg1, arg2, ref _gameEngine.StaticVariables.g_fadeControlValue);
                                    _gameEngine.StaticVariables.g_fadeSubstate = 2;
                                }
                            }

                            if (_gameEngine.StaticVariables.g_fadeSubstate != 2)
                            {
                                return;
                            }

                            if (_gameEngine.StaticVariables.g_fadeFrame < 0x13)
                            {
                                _gameEngine.StaticVariables.g_fadeFrame += 1;
                                return;
                            }

                            if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset != 0xffffffff)
                            {
                                _gameEngine.StaticVariables.g_fadeFrame = 0;
                                _gameEngine.StaticVariables.g_globalTransitionState = 0x17;
                                return;
                            }

                            _gameEngine.StaticVariables.g_fadeFrame += 1;
                            return;
                        }

                        if (0x3ff < _gameEngine.StaticVariables.g_globalTransitionState)
                        {
                            if (_gameEngine.StaticVariables.g_globalTransitionState == 10000)
                            {
                                _gameEngine.StaticVariables.g_isMemoryCopyInProgress = 1;
                                _gameEngine.StaticVariables.g_fadeFrame = 0;
                                _gameEngine.StaticVariables.g_playerControlFlags |= 8;
                                arg1 = _gameEngine.EtcRes.GetEtcString(0x87);
                                arg2 = _gameEngine.EtcRes.GetEtcString(0x88);
                                TryOpenMemoryCardMenu(arg1, arg2, ref _gameEngine.StaticVariables.g_fadeControlValue);
                                _gameEngine.StaticVariables.g_globalTransitionState = 0x2711;
                                return;
                            }

                            if (10000 < _gameEngine.StaticVariables.g_globalTransitionState)
                            {
                                if (_gameEngine.StaticVariables.g_globalTransitionState != 0x2711)
                                {
                                    return;
                                }
                                if (0x12 < _gameEngine.StaticVariables.g_fadeFrame)
                                {
                                    _gameEngine.StaticVariables.g_fadeFrame = 0;
                                    _gameEngine.StaticVariables.g_globalTransitionState = 1;
                                    return;
                                }
                                _gameEngine.StaticVariables.g_fadeFrame += 1;
                                return;
                            }

                            if (_gameEngine.StaticVariables.g_globalTransitionState != 1099)
                            {
                                return;
                            }

                            if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                            {
                                ResetMemoryCardMenuState();
                                _gameEngine.StaticVariables.g_fadeSubstate = 1;
                            }

                            if (10 < _gameEngine.StaticVariables.g_fadeFrame)
                            {
                                _gameEngine.StaticVariables.g_fadeFrame = 0;
                                _gameEngine.StaticVariables.g_globalTransitionState = 99;
                                return;
                            }
                            _gameEngine.StaticVariables.g_fadeFrame += 1;
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_globalTransitionState == 0x3fd)
                        {
                            if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                            {
                                ResetMemoryCardMenuState();
                                _gameEngine.StaticVariables.g_fadeSubstate = 1;
                            }

                            if (0x12 < _gameEngine.StaticVariables.g_fadeFrame)
                            {
                                if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset != 0xffffffff)
                                {
                                    _gameEngine.StaticVariables.g_fadeFrame = 0;
                                    _gameEngine.StaticVariables.g_globalTransitionState = 0x15;
                                    return;
                                }

                                _gameEngine.StaticVariables.g_fadeFrame += 1;
                                return;
                            }

                            _gameEngine.StaticVariables.g_fadeFrame += 1;
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_globalTransitionState != 0x3fe)
                        {
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                        {
                            ResetMemoryCardMenuState();
                            _gameEngine.StaticVariables.g_fadeSubstate = 1;
                        }

                        if (0x12 < _gameEngine.StaticVariables.g_fadeFrame)
                        {
                            if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset != 0xffffffff)
                            {
                                _gameEngine.StaticVariables.g_fadeFrame = 0;
                                _gameEngine.StaticVariables.g_globalTransitionState = 0x16;
                                return;
                            }
                            _gameEngine.StaticVariables.g_fadeFrame += 1;
                            return;
                        }
                        _gameEngine.StaticVariables.g_fadeFrame += 1;
                        return;
                    }

                    if (_gameEngine.StaticVariables.g_globalTransitionState != 0x3f9)
                    {
                        if (0x3f9 < _gameEngine.StaticVariables.g_globalTransitionState)
                        {
                            if (_gameEngine.StaticVariables.g_globalTransitionState == 0x3fa)
                            {
                                if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                                {
                                    ResetMemoryCardMenuState();
                                    _gameEngine.StaticVariables.g_fadeSubstate = 1;
                                }

                                if (0x12 < _gameEngine.StaticVariables.g_fadeFrame)
                                {
                                    if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset != 0xffffffff)
                                    {
                                        _gameEngine.StaticVariables.g_fadeFrame = 0;
                                        _gameEngine.StaticVariables.g_globalTransitionState = 0x12;
                                        return;
                                    }
                                    _gameEngine.StaticVariables.g_fadeFrame += 1;
                                    return;
                                }

                                _gameEngine.StaticVariables.g_fadeFrame += 1;
                                return;
                            }

                            if (_gameEngine.StaticVariables.g_globalTransitionState != 0x3fb)
                            {
                                return;
                            }

                            if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                            {
                                ResetMemoryCardMenuState();
                                _gameEngine.StaticVariables.g_fadeSubstate = 1;
                            }

                            if (0x12 < _gameEngine.StaticVariables.g_fadeFrame)
                            {
                                if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset != 0xffffffff)
                                {
                                    _gameEngine.StaticVariables.g_fadeFrame = 0;
                                    _gameEngine.StaticVariables.g_globalTransitionState = 0x13;
                                    return;
                                }
                                _gameEngine.StaticVariables.g_fadeFrame += 1;
                                return;
                            }

                            _gameEngine.StaticVariables.g_fadeFrame += 1;
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_globalTransitionState == 0x3f7)
                        {
                            if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset != 0xffffffff)
                            {
                                if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset != 0xfffffffe)
                                {
                                    _gameEngine.StaticVariables.g_globalTransitionState = 0xf;
                                }
                                else
                                {
                                    _gameEngine.StaticVariables.g_globalTransitionState = 0xf;
                                    arg1 = _gameEngine.EtcRes.GetEtcString(0xa3);
                                    arg2 = _gameEngine.EtcRes.GetEtcString(0xa4);
                                    TryOpenMemoryCardMenu(arg1, arg2, ref _gameEngine.StaticVariables.g_fadeControlValue);
                                }
                                _gameEngine.StaticVariables.g_fadeFrame = 0;
                                return;
                            }
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_globalTransitionState != 0x3f8)
                        {
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset != 0xffffffff)
                        {
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset != 0xfffffffe)
                        {
                            if (0x12 < _gameEngine.StaticVariables.g_fadeFrame)
                            {
                                _gameEngine.StaticVariables.g_fadeFrame = 0;
                                _gameEngine.StaticVariables.g_globalTransitionState = 0x10;
                                return;
                            }
                            _gameEngine.StaticVariables.g_fadeFrame += 1;
                            return;
                        }

                        if (0x12 < _gameEngine.StaticVariables.g_fadeFrame)
                        {
                            _gameEngine.StaticVariables.g_fadeFrame += 1;
                            arg1 = _gameEngine.EtcRes.GetEtcString(0xa5);
                            arg2 = _gameEngine.EtcRes.GetEtcString(0xa6);
                            TryOpenMemoryCardMenu(arg1, arg2, ref _gameEngine.StaticVariables.g_fadeControlValue);
                            _gameEngine.StaticVariables.g_fadeFrame = 0;
                            _gameEngine.StaticVariables.g_globalTransitionState = 0x10;
                            return;
                        }

                        _gameEngine.StaticVariables.g_fadeFrame += 1;
                        return;
                    }

                    if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                    {
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                    }

                    fadeCounter = _gameEngine.StaticVariables.g_fadeFrame + 1;

                    if (_gameEngine.StaticVariables.g_fadeFrame < 0x13)
                    {
                        _gameEngine.StaticVariables.g_fadeFrame = fadeCounter;
                        return;
                    }

                    etcSectionA = 0x11;
                }

                if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset != 0xffffffff)
                {
                    _gameEngine.StaticVariables.g_fadeFrame = fadeCounter;
                    return;
                }
            }

            _gameEngine.StaticVariables.g_fadeFrame = 0;
            _gameEngine.StaticVariables.g_globalTransitionState = etcSectionA;
            return;
        }

        if (_gameEngine.StaticVariables.g_globalTransitionState == 0xd)
        {
            _gameEngine.StaticVariables.g_fadeSubstate = 0;
            _gameEngine.StaticVariables.g_fadeFrame = 0;
            arg1 = _gameEngine.EtcRes.GetEtcString(0x9f);
            arg2 = _gameEngine.EtcRes.GetEtcString(0xa0);
            TryOpenMemoryCardMenu(arg1, arg2, ref _gameEngine.StaticVariables.g_fadeControlValue);
            _gameEngine.StaticVariables.g_globalTransitionState = 0x3f6;
            return;
        }

        if (_gameEngine.StaticVariables.g_globalTransitionState < 0xe)
        {
            if (_gameEngine.StaticVariables.g_globalTransitionState == 6)
            {
                _gameEngine.StaticVariables.g_fadeSubstate = 0;
                FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.INT_ARRAY_80191088, _gameEngine.StaticVariables.INT_ARRAY_80191080);

                iVar5 = _gameEngine.StaticVariables.INT_ARRAY_80191088[_gameEngine.StaticVariables.g_memorySlotId];
                fadeCounter = 3;

                if (iVar5 == 0)
                {
                    FUN_8005df84(_gameEngine.StaticVariables.g_memorySlotId);

                    if (fadeCounter != 1)
                    {
                        _gameEngine.StaticVariables.g_fadeFrame = 0;
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3fc;
                        return;
                    }

                    _gameEngine.StaticVariables.g_fadeFrame = 0;
                    _gameEngine.StaticVariables.g_globalTransitionState = 0x3f3;
                    return;
                }

                //LAB_8005fe34:
                if (iVar5 == 3)
                {
                    //goto LAB_80060640
                    _gameEngine.StaticVariables.g_globalTransitionState = 0x3fb;
                    return;
                }
            }
            else
            {
                if (6 < _gameEngine.StaticVariables.g_globalTransitionState)
                {
                    if (_gameEngine.StaticVariables.g_globalTransitionState != 9)
                    {
                        if (_gameEngine.StaticVariables.g_globalTransitionState < 10)
                        {
                            if (_gameEngine.StaticVariables.g_globalTransitionState == 7)
                            {
                                _gameEngine.StaticVariables.g_fadeSubstate = 0;
                                arg1 = _gameEngine.EtcRes.GetEtcString(0x93);
                                arg2 = _gameEngine.EtcRes.GetEtcString(0x94);
                                TryOpenMemoryCardMenu(arg1, arg2, ref _gameEngine.StaticVariables.g_fadeControlValue);
                                arg1 = _gameEngine.EtcRes.GetEtcString(0x81);
                                arg2 = _gameEngine.EtcRes.GetEtcString(0x82);
                                _gameEngine.InitializeAsyncOperation(arg1, arg2, result => _gameEngine.StaticVariables.g_asyncOperationResult = result);
                                _gameEngine.StaticVariables.g_globalTransitionState = 0x6b;
                                return;
                            }

                            if (_gameEngine.StaticVariables.g_globalTransitionState == 8)
                            {
                                _gameEngine.StaticVariables.g_fadeSubstate = 0;
                                FUN_80060bd0();
                            }

                            return;
                        }

                        if (_gameEngine.StaticVariables.g_globalTransitionState == 0xb)
                        {
                            _gameEngine.StaticVariables.g_fadeSubstate = 0;

                            if (_gameEngine.StaticVariables.INT_ARRAY_80191088[_gameEngine.StaticVariables.g_memorySlotId] != 0)
                            {
                                if (_gameEngine.StaticVariables.INT_ARRAY_80191088[_gameEngine.StaticVariables.g_memorySlotId] != 3)
                                {
                                    _gameEngine.StaticVariables.g_globalTransitionState = 0x3f9;
                                    _gameEngine.StaticVariables.g_fadeSubstate = 0;
                                    return;
                                }

                                _gameEngine.StaticVariables.g_globalTransitionState = 0x3fb;
                                _gameEngine.StaticVariables.g_fadeSubstate = 0;
                                return;
                            }

                            fadeCounter = BuildDataAndSaveInMemoryCard(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.g_gameTitle);

                            if (fadeCounter == -1)
                            {
                                _gameEngine.StaticVariables.g_globalTransitionState = 0x3fd;
                                return;
                            }

                            _gameEngine.StaticVariables.g_globalTransitionState = 0x3f4;
                            return;
                        }

                        if (0xb < _gameEngine.StaticVariables.g_globalTransitionState)
                        {
                            _gameEngine.StaticVariables.g_fadeSubstate = 0;
                            Debugger.Break();
                            //fadeCounter = 0;
                            //puVar2 = _gameEngine.StaticVariables.PTR_8018ed68;
                            //piVar6 = _gameEngine.StaticVariables.PTR_8018ed6c;
                            //pbVar4 = _gameEngine.StaticVariables.BYTE_ARRAY_8018f278[8];
                            //iVar5 = _gameEngine.StaticVariables.BYTE_ARRAY_8018f278[40];
                            //
                            //do
                            //{
                            //    pbVar1 = FUN_800818e4(pbVar4);
                            //    *puVar2 = pbVar1;
                            //    *piVar6 = iVar5;
                            //    piVar6 += 2;
                            //    iVar5 += 0x76c;
                            //    puVar2 += 2;
                            //    fadeCounter += 1;
                            //    pbVar4 += 0x76c;
                            //} while (fadeCounter < 4);

                            _gameEngine.StaticVariables.DAT_8018ed88 = 0;
                            _gameEngine.StaticVariables.DAT_8018ed8c = 0;
                            _gameEngine.StaticVariables.PTR_8018ede8 = _gameEngine.EtcRes.GetEtcString(0x83);
                            _gameEngine.StaticVariables.PTR_8018edec = _gameEngine.EtcRes.GetEtcString(0x84);
                            FUN_80058ab4(_gameEngine.StaticVariables.PTR_8018ed68, _gameEngine.StaticVariables.PTR_8018ed6c, ref _gameEngine.StaticVariables.g_memoryCardPayloadOffset);
                            _gameEngine.StaticVariables.g_fadeFrame = 0;
                            _gameEngine.StaticVariables.g_globalTransitionState = 0x3f8;
                            return;
                        }

                        _gameEngine.StaticVariables.g_fadeSubstate = 0;
                        FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.INT_ARRAY_80191088, _gameEngine.StaticVariables.INT_ARRAY_80191080);

                        if (_gameEngine.StaticVariables.INT_ARRAY_80191088[_gameEngine.StaticVariables.g_memorySlotId] != 0)
                        {
                            if (_gameEngine.StaticVariables.INT_ARRAY_80191088[_gameEngine.StaticVariables.g_memorySlotId] != 3)
                            {
                                _gameEngine.StaticVariables.g_globalTransitionState = 0x3f9;
                                return;
                            }

                            _gameEngine.StaticVariables.g_globalTransitionState = 0x3fb;
                            return;
                        }

                        FUN_8005e3e4(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.g_gameTitle, _gameEngine.StaticVariables.g_memoryCardFileIndex, _gameEngine.StaticVariables.PTR_8018ed68);
                        _gameEngine.StaticVariables.PTR_8018ede8 = _gameEngine.EtcRes.GetEtcString(0x85);
                        _gameEngine.StaticVariables.PTR_8018edec = _gameEngine.EtcRes.GetEtcString(0x86);
                        FUN_80058ab4(_gameEngine.StaticVariables.PTR_8018ed68, _gameEngine.StaticVariables.PTR_8018ed6c, ref _gameEngine.StaticVariables.g_memoryCardPayloadOffset);
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3f7;
                        return;
                    }

                    _gameEngine.StaticVariables.g_fadeSubstate = 0;
                    FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.INT_ARRAY_80191088, _gameEngine.StaticVariables.INT_ARRAY_80191080);
                    iVar5 = _gameEngine.StaticVariables.INT_ARRAY_80191088[_gameEngine.StaticVariables.g_memorySlotId];

                    if (iVar5 == 0)
                    {
                        //puVar3 = &_gameEngine.StaticVariables.g_gameTitle;
                        //FindSaveFileInMemoryCard(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.g_gameTitle, ref _gameEngine.StaticVariables.g_memoryCardFileIndex);
                        //iVar5 = 0;
                        //
                        //if (0 < _gameEngine.StaticVariables.g_memoryCardFileIndex)
                        //{
                        //    do
                        //    {
                        //        iVar5 += 1;
                        //        fadeCounter += (puVar3 + 0x18);
                        //        puVar3 += 0x28;
                        //    } while (iVar5 < _gameEngine.StaticVariables.g_memoryCardFileIndex);
                        //}

                        Debugger.Break();
                        fadeCounter = 0x1c000;

                        if (0x1c000 < fadeCounter)
                        {
                            arg1 = _gameEngine.EtcRes.GetEtcString(0x97);
                            arg2 = _gameEngine.EtcRes.GetEtcString(0x98);
                            TryOpenMemoryCardMenu(arg1, arg2, ref _gameEngine.StaticVariables.g_fadeControlValue);
                            _gameEngine.StaticVariables.g_globalTransitionState = 0x3f2;
                            return;
                        }

                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3ff;
                    }

                    //goto LAB_8005fe34;
                    if (iVar5 == 3)
                    {
                        //goto LAB_80060640
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3fb;
                        return;
                    }
                }

                if (_gameEngine.StaticVariables.g_globalTransitionState != 3)
                {
                    if (3 < _gameEngine.StaticVariables.g_globalTransitionState)
                    {
                        if (_gameEngine.StaticVariables.g_globalTransitionState == 4)
                        {
                            _gameEngine.StaticVariables.g_fadeSubstate = 0;
                            FUN_80060b1c();
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_globalTransitionState == 5)
                        {
                            _gameEngine.StaticVariables.g_fadeSubstate = 0;
                            var uStack_24 = new int[2];
                            FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId, local_28, uStack_24);

                            if (1 < local_28[_gameEngine.StaticVariables.g_memorySlotId] - 1U)
                            {
                                arg1 = _gameEngine.EtcRes.GetEtcString(0x8f);
                                arg2 = _gameEngine.EtcRes.GetEtcString(0x90);
                                TryOpenMemoryCardMenu(arg1, arg2, ref _gameEngine.StaticVariables.g_fadeControlValue);
                                _gameEngine.StaticVariables.g_asyncOperationResult = 2;
                                arg1 = _gameEngine.EtcRes.GetEtcString(0x81);
                                arg2 = _gameEngine.EtcRes.GetEtcString(0x82);
                                _gameEngine.StartAsyncOperation(arg1, arg2, result => _gameEngine.StaticVariables.g_asyncOperationResult = result);
                                _gameEngine.StaticVariables.g_globalTransitionState = 0x69;
                                return;
                            }

                            _gameEngine.StaticVariables.g_globalTransitionState = 0x3eb;
                        }
                        return;
                    }

                    if (_gameEngine.StaticVariables.g_globalTransitionState == 1)
                    {
                        _gameEngine.StaticVariables.g_fadeSubstate = 0;
                        FUN_8005dc04(0, _gameEngine.StaticVariables.INT_ARRAY_80191088, _gameEngine.StaticVariables.INT_ARRAY_80191080);

                        if (_gameEngine.StaticVariables.INT_ARRAY_80191088[0] != 0 && _gameEngine.StaticVariables.INT_ARRAY_80191088[0] != 3)
                        {
                            _gameEngine.StaticVariables.g_globalTransitionState = 0x3ea;
                            return;
                        }

                        _gameEngine.StaticVariables.g_memorySlotId = 0;
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3ec;
                        return;
                    }

                    if (_gameEngine.StaticVariables.g_globalTransitionState != 2)
                    {
                        return;
                    }

                    _gameEngine.StaticVariables.g_fadeSubstate = 0;
                    FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.INT_ARRAY_80191088, _gameEngine.StaticVariables.INT_ARRAY_80191080);

                    if (_gameEngine.StaticVariables.INT_ARRAY_80191088[1] != 0 && _gameEngine.StaticVariables.INT_ARRAY_80191088[1] != 3)
                    {
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3eb;
                        return;
                    }

                    _gameEngine.StaticVariables.g_memorySlotId = 1;
                    _gameEngine.StaticVariables.g_globalTransitionState = 0x3fa;
                    return;
                }

                _gameEngine.StaticVariables.g_fadeSubstate = 0;
            }

            _gameEngine.StaticVariables.g_globalTransitionState = 0x3f9;
        }
        else
        {
            if (_gameEngine.StaticVariables.g_globalTransitionState == 0x13)
            {
                _gameEngine.StaticVariables.g_fadeSubstate = 0;
                _gameEngine.StaticVariables.g_fadeFrame = 0;
                arg1 = _gameEngine.EtcRes.GetEtcString(0xab);
                fadeCounter = 0xac;
                //LAB_800609c0:
                arg2 = _gameEngine.EtcRes.GetEtcString(fadeCounter);
                TryOpenMemoryCardMenu(arg1, arg2, ref _gameEngine.StaticVariables.g_fadeControlValue);
                _gameEngine.StaticVariables.g_globalTransitionState = 0x3f5;
                return;
            }

            if (0x13 < _gameEngine.StaticVariables.g_globalTransitionState)
            {
                if (_gameEngine.StaticVariables.g_globalTransitionState == 0x16)
                {
                    _gameEngine.StaticVariables.g_fadeSubstate = 0;
                    _gameEngine.StaticVariables.g_fadeFrame = 0;
                    arg1 = _gameEngine.EtcRes.GetEtcString(0xb1);
                    fadeCounter = 0xb2;
                }
                else
                {
                    if (0x16 < _gameEngine.StaticVariables.g_globalTransitionState)
                    {
                        if (_gameEngine.StaticVariables.g_globalTransitionState == 99)
                        {
                            _gameEngine.StaticVariables.g_fadeFrame = 0;
                            _gameEngine.StaticVariables.g_isMemoryCopyInProgress = 0;
                            _gameEngine.StaticVariables.g_globalTransitionState = 0;
                            _gameEngine.StaticVariables.g_fadeSubstate = 0;
                            _gameEngine.StaticVariables.g_playerControlFlags &= 0xfffffff7;
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_globalTransitionState < 100)
                        {
                            if (_gameEngine.StaticVariables.g_globalTransitionState == 0x17)
                            {
                                _gameEngine.StaticVariables.g_fadeFrame = 0;
                                _gameEngine.StaticVariables.g_globalTransitionState = 0x3f3;
                                _gameEngine.StaticVariables.g_fadeSubstate = 0;
                            }
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_globalTransitionState != 0x69)
                        {
                            return;
                        }

                        if (_gameEngine.StaticVariables.g_asyncOperationResult == 1)
                        {
                            _gameEngine.StaticVariables.g_globalTransitionState = 0x3ee;
                            _gameEngine.StaticVariables.g_fadeFrame = 0;
                        }
                        else
                        {
                            if (_gameEngine.StaticVariables.g_asyncOperationResult < 2)
                            {
                                return;
                            }

                            if (_gameEngine.StaticVariables.g_asyncOperationResult != 2)
                            {
                                return;
                            }

                            _gameEngine.StaticVariables.g_globalTransitionState = 0x3ef;
                            _gameEngine.StaticVariables.g_fadeFrame = 0;
                        }

                        //goto FADE_RESTART;
                        _gameEngine.StaticVariables.g_asyncOperationResult = 0;
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                        return;
                    }

                    if (_gameEngine.StaticVariables.g_globalTransitionState != 0x14)
                    {
                        if (_gameEngine.StaticVariables.g_globalTransitionState == 0x15)
                        {
                            _gameEngine.StaticVariables.g_fadeSubstate = 0;
                            _gameEngine.StaticVariables.g_fadeFrame = 0;
                            arg1 = _gameEngine.EtcRes.GetEtcString(0xaf);
                            arg2 = _gameEngine.EtcRes.GetEtcString(0xb0);
                            TryOpenMemoryCardMenu(arg1, arg2, ref _gameEngine.StaticVariables.g_fadeControlValue);
                            _gameEngine.StaticVariables.g_globalTransitionState = 0x3f5;
                        }
                        return;
                    }

                    _gameEngine.StaticVariables.g_fadeSubstate = 0;
                    _gameEngine.StaticVariables.g_fadeFrame = 0;
                    arg1 = _gameEngine.EtcRes.GetEtcString(0xad);
                    fadeCounter = 0xae;
                }

                //goto LAB_800609c0;
                arg2 = _gameEngine.EtcRes.GetEtcString(fadeCounter);
                TryOpenMemoryCardMenu(arg1, arg2, ref _gameEngine.StaticVariables.g_fadeControlValue);
                _gameEngine.StaticVariables.g_globalTransitionState = 0x3f5;
                return;
            }

            if (_gameEngine.StaticVariables.g_globalTransitionState != 0x10)
            {
                if (_gameEngine.StaticVariables.g_globalTransitionState < 0x11)
                {
                    if (_gameEngine.StaticVariables.g_globalTransitionState == 0xe)
                    {
                        _gameEngine.StaticVariables.g_fadeSubstate = 0;
                        _gameEngine.StaticVariables.g_fadeFrame = 0;
                        _gameEngine.HudManager.InitializeHudPositionBeforeHide();
                        _gameEngine.StaticVariables.g_globalTransitionState = 1099;
                        return;
                    }

                    if (_gameEngine.StaticVariables.g_globalTransitionState != 0xf)
                    {
                        return;
                    }

                    if (_gameEngine.StaticVariables.g_fadeFrame < 0x13)
                    {
                        _gameEngine.StaticVariables.g_fadeFrame += 1;
                        return;
                    }

                    if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset != 0xfffffffe)
                    {
                        _gameEngine.StaticVariables.g_fadeFrame = 0;
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3f6;
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                        return;
                    }

                    _gameEngine.StaticVariables.g_fadeFrame += 1;
                    FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.INT_ARRAY_80191088, _gameEngine.StaticVariables.INT_ARRAY_80191080);

                    if (_gameEngine.StaticVariables.INT_ARRAY_80191088[_gameEngine.StaticVariables.g_memorySlotId] == 0)
                    {
                        DeleteMemoryCardFile(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.g_gameTitle /*+ _gameEngine.StaticVariables.g_memoryCardPayloadOffset * 0x28*/);
                        _gameEngine.StaticVariables.g_fadeFrame = 0;
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3ff;
                        _gameEngine.StaticVariables.g_fadeSubstate = 0;
                        return;
                    }

                    if (_gameEngine.StaticVariables.INT_ARRAY_80191088[_gameEngine.StaticVariables.g_memorySlotId] != 3)
                    {
                        _gameEngine.StaticVariables.g_fadeFrame = 0;
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3f9;
                        return;
                    }

                    _gameEngine.StaticVariables.g_fadeFrame = 0;
                    _gameEngine.StaticVariables.g_globalTransitionState = 0x3fb;

                    return;
                }

                if (_gameEngine.StaticVariables.g_globalTransitionState != 0x11)
                {
                    if (_gameEngine.StaticVariables.g_globalTransitionState == 0x12)
                    {
                        _gameEngine.StaticVariables.g_fadeSubstate = 0;
                        _gameEngine.StaticVariables.g_fadeFrame = 0;
                        arg1 = _gameEngine.EtcRes.GetEtcString(0xa9);
                        arg2 = _gameEngine.EtcRes.GetEtcString(0xaa);
                        TryOpenMemoryCardMenu(arg1, arg2, ref _gameEngine.StaticVariables.g_fadeControlValue);
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3f5;
                    }

                    return;
                }

                _gameEngine.StaticVariables.g_fadeSubstate = 0;
                _gameEngine.StaticVariables.g_fadeFrame = 0;
                arg1 = _gameEngine.EtcRes.GetEtcString(0xa7);
                fadeCounter = 0xa8;
                //goto LAB_800609c0;
                arg2 = _gameEngine.EtcRes.GetEtcString(fadeCounter);
                TryOpenMemoryCardMenu(arg1, arg2, ref _gameEngine.StaticVariables.g_fadeControlValue);
                _gameEngine.StaticVariables.g_globalTransitionState = 0x3f5;
                return;
            }

            if (_gameEngine.StaticVariables.g_fadeFrame < 0x13)
            {
                _gameEngine.StaticVariables.g_fadeFrame += 1;
                return;
            }

            if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset != 0xfffffffe)
            {
                _gameEngine.StaticVariables.g_fadeFrame = 0;
                _gameEngine.StaticVariables.g_globalTransitionState = 0x3f6;
                _gameEngine.StaticVariables.g_fadeSubstate = 1;
                return;
            }
            _gameEngine.StaticVariables.g_fadeFrame += 1;

            FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.INT_ARRAY_80191088, _gameEngine.StaticVariables.INT_ARRAY_80191080);

            if (_gameEngine.StaticVariables.INT_ARRAY_80191088[_gameEngine.StaticVariables.g_memorySlotId] == 0)
            {
                fadeCounter = BuildDataAndSaveInMemoryCardAndUpdateData(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.g_gameTitle, _gameEngine.StaticVariables.g_memoryCardPayloadOffset);

                if (fadeCounter == -1)
                {
                    _gameEngine.StaticVariables.g_globalTransitionState = 0x3fd;
                    return;
                }

                _gameEngine.StaticVariables.g_fadeFrame = 0;
                _gameEngine.StaticVariables.g_globalTransitionState = 0x3fe;
                _gameEngine.StaticVariables.g_fadeSubstate = 0;

                return;
            }
            _gameEngine.StaticVariables.g_fadeFrame = 0;

            if (_gameEngine.StaticVariables.INT_ARRAY_80191088[_gameEngine.StaticVariables.g_memorySlotId] != 3)
            {
                _gameEngine.StaticVariables.g_fadeFrame = 0;
                _gameEngine.StaticVariables.g_globalTransitionState = 0x3f9;
                return;
            }

            //LAB_80060640:
            _gameEngine.StaticVariables.g_globalTransitionState = 0x3fb;
        }
    }

    //8005e3e4
    private string FUN_8005e3e4(int slotId, string gameTitle, int memoryCardFileIndex, string data)
    {
        Debugger.Break();
        return "";
    }


    //80060e20
    private int BuildDataAndSaveInMemoryCard(int slotId, string gameTitle)
    {
        _gameEngine.StaticVariables.g_memoryCardDataBlob.Header[0] = 'S';
        _gameEngine.StaticVariables.g_memoryCardDataBlob.Header[1] = 'C';
        _gameEngine.StaticVariables.g_memoryCardDataBlob.IconFlags = 0x11;
        _gameEngine.StaticVariables.g_memoryCardDataBlob.BlockCount = 1;
        _gameEngine.StaticVariables.g_memoryCardDataBlob.Title = "ＡＬＵＮＤＲＡ・アランドラ";

        Array.Copy(_gameEngine.StaticVariables.g_memoryCardIconClut, _gameEngine.StaticVariables.g_memoryCardDataBlob.IconClut16, _gameEngine.StaticVariables.g_memoryCardIconClut.Length);
        Array.Copy(_gameEngine.StaticVariables.g_memoryCardIconFrame0, _gameEngine.StaticVariables.g_memoryCardDataBlob.IconFrame4bpp_0, _gameEngine.StaticVariables.g_memoryCardIconFrame0.Length);
        Array.Copy(_gameEngine.StaticVariables.g_memoryCardIconFrame1, _gameEngine.StaticVariables.g_memoryCardDataBlob.IconFrame4bpp_1, _gameEngine.StaticVariables.g_memoryCardIconFrame1.Length);
        Array.Copy(_gameEngine.StaticVariables.g_memoryCardIconFrame2, _gameEngine.StaticVariables.g_memoryCardDataBlob.IconFrame4bpp_2, _gameEngine.StaticVariables.g_memoryCardIconFrame2.Length);

        Array.Clear(_gameEngine.StaticVariables.g_memoryCardDataBlob.SavePayload,
            _gameEngine.StaticVariables.g_memoryCardDataBlob.SavePayload.Length - 0x1daf, 
            0x1daf);

        //N’analyse pas!!:::Kobayashi Toshiaki:j1494039:Matrix
        _gameEngine.StaticVariables.g_memoryCardDataBlob.DeveloperWatermark = "解析するな!!:::小林敬明:j1494039:Matrix"; //length = 64
        _gameEngine.StaticVariables.g_memoryCardDataBlob.Checksum = ComputeChecksum(0x1ffc);

        return SaveInMemoryCard(slotId, gameTitle, _gameEngine.StaticVariables.g_memoryCardDataBlob);
    }

    //8006122c
    private int BuildDataAndSaveInMemoryCardAndUpdateData(int slotId, string gameTitle, uint offset)
    {
        _gameEngine.StaticVariables.g_memoryCardDataBlob.Header[0] = 'S';
        _gameEngine.StaticVariables.g_memoryCardDataBlob.Header[1] = 'C';
        _gameEngine.StaticVariables.g_memoryCardDataBlob.IconFlags = 0x11;
        _gameEngine.StaticVariables.g_memoryCardDataBlob.BlockCount = 1;
        _gameEngine.StaticVariables.g_memoryCardDataBlob.Title = "ＡＬＵＮＤＲＡ・アランドラ";

        Array.Copy(_gameEngine.StaticVariables.g_memoryCardIconClut, _gameEngine.StaticVariables.g_memoryCardDataBlob.IconClut16, _gameEngine.StaticVariables.g_memoryCardIconClut.Length);
        Array.Copy(_gameEngine.StaticVariables.g_memoryCardIconFrame0, _gameEngine.StaticVariables.g_memoryCardDataBlob.IconFrame4bpp_0, _gameEngine.StaticVariables.g_memoryCardIconFrame0.Length);
        Array.Copy(_gameEngine.StaticVariables.g_memoryCardIconFrame1, _gameEngine.StaticVariables.g_memoryCardDataBlob.IconFrame4bpp_1, _gameEngine.StaticVariables.g_memoryCardIconFrame1.Length);
        Array.Copy(_gameEngine.StaticVariables.g_memoryCardIconFrame2, _gameEngine.StaticVariables.g_memoryCardDataBlob.IconFrame4bpp_2, _gameEngine.StaticVariables.g_memoryCardIconFrame2.Length);

        Debugger.Break();

        //Array.Copy(_gameEngine.StaticVariables.g_saveDataCopyPtr, 
        //    _gameEngine.StaticVariables.g_memoryCardDataBlob.SavePayload[offset * 0x76c], 
        //    _gameEngine.StaticVariables.g_saveDataSize);
        //_gameEngine.StaticVariables.g_memoryCardDataBlob.SavePayload[offset * 0x76c + 4] = offset;

        //N’analyse pas!!:::Kobayashi Toshiaki:j1494039:Matrix
        _gameEngine.StaticVariables.g_memoryCardDataBlob.DeveloperWatermark = "解析するな!!:::小林敬明:j1494039:Matrix"; //length = 64
        _gameEngine.StaticVariables.g_memoryCardDataBlob.Checksum = ComputeChecksum(0x1ffc);

        var result = 1;
        if (SaveInMemoryCard(slotId, gameTitle, _gameEngine.StaticVariables.g_memoryCardDataBlob) == -1)
        {
            result = -1;
        }

        //Array.Copy(
        //    _gameEngine.StaticVariables.g_memoryCardDataBlob.SavePayload[offset * 0x76c],
        //    _gameEngine.StaticVariables.g_saveDataInRam, 
        //    0x760);

        return result;
    }

    //800616d8
    private uint ComputeChecksum(uint value)
    {
        uint val;
        uint j;
        uint i;

        val = 0xffffffff;
        i = 0;

        if (value != 0)
        {
            do
            {
                j = 0;
                val = (uint)(val ^ (_gameEngine.StaticVariables.g_memoryCardDataBlob.Header[i] << 0x18));

                do
                {
                    if ((val & 0x80000000) == 0)
                    {
                        val = val << 1;
                    }
                    else
                    {
                        val = (val << 1) ^ 0x4c11db7;
                    }
                    j = j + 1;

                } while (j < 8);

                i = i + 1;
            } while (i < value);
        }

        return ~val;
    }

    //80058ab4
    private int FUN_80058ab4(string param_1, string param_2, ref uint param_3)
    {
        _gameEngine.HudManager.InitializeHudPosition();
        _gameEngine.StaticVariables.PTR_80180128 = param_3;
        _gameEngine.StaticVariables.PTR_80180238 = param_1;
        _gameEngine.StaticVariables.PTR_8018023c = param_2;
        param_3 = 0xffffffff;
        _gameEngine.GraphicManager.SetTransitionType(10);
        return 1;
    }

    //8005e12c
    private void DeleteMemoryCardFile(int slotId, string gameTitle)
    {
        string prefix = slotId == 0 ? "bu00:" : "bu10:";
        const int maxTitleChars = 0x15;
        int titleLen = Math.Min(gameTitle.Length, maxTitleChars);
        string truncatedTitle = titleLen > 0 ? gameTitle.Substring(0, titleLen) : string.Empty;
        string fullPath = prefix + truncatedTitle;
        //DeleteFileOnCard(fullPath);
    }

    //80060b1c
    private void FUN_80060b1c()
    {
        FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.INT_ARRAY_80191088, _gameEngine.StaticVariables.INT_ARRAY_80191080);
        
        if (_gameEngine.StaticVariables.INT_ARRAY_80191088[_gameEngine.StaticVariables.g_memorySlotId] == 0)
        {
            if (_gameEngine.StaticVariables.INT_ARRAY_80191080[_gameEngine.StaticVariables.g_memorySlotId] == 0)
            {
                _gameEngine.StaticVariables.g_globalTransitionState = 0x3f0;
            }
            else
            {
                _gameEngine.StaticVariables.g_globalTransitionState = 0x3ed;
            }
        }
        else if (_gameEngine.StaticVariables.INT_ARRAY_80191088[_gameEngine.StaticVariables.g_memorySlotId] == 3)
        {
            _gameEngine.StaticVariables.g_globalTransitionState = 0x3fb;
        }
        else
        {
            _gameEngine.StaticVariables.g_globalTransitionState = 0x3f9;
        }
    }

    //80060bd0
    private void FUN_80060bd0()
    {
        bool bVar1;
        int iVar2;
        int i;

        FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.INT_ARRAY_80191088, _gameEngine.StaticVariables.INT_ARRAY_80191080);

        if (_gameEngine.StaticVariables.INT_ARRAY_80191088[_gameEngine.StaticVariables.g_memorySlotId] == 0)
        {
            FindSaveFileInMemoryCard(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.g_gameTitle, ref _gameEngine.StaticVariables.g_memoryCardFileIndex);
            bVar1 = false;
            i = 0;

            if (0 < _gameEngine.StaticVariables.g_memoryCardFileIndex)
            {
                //do
                //{
                //    iVar2 = strcmp(piVar4, _gameEngine.StaticVariables.g_gameTitle);
                //
                //    if (iVar2 == 0)
                //    {
                //        bVar1 = true;
                //    }
                //
                //    i = i + 1;
                //    piVar4 = piVar4 + 0x28;
                //} while (i < _gameEngine.StaticVariables.g_memoryCardFileIndex);
            }

            if (bVar1)
            {
                _gameEngine.StaticVariables.g_globalTransitionState = 0x3f4;
            }
            else
            {
                _gameEngine.StaticVariables.g_globalTransitionState = 0x3f1;
            }
        }
        else if (_gameEngine.StaticVariables.INT_ARRAY_80191088[_gameEngine.StaticVariables.g_memorySlotId] == 3)
        {
            _gameEngine.StaticVariables.g_globalTransitionState = 0x3fb;
        }
        else
        {
            _gameEngine.StaticVariables.g_globalTransitionState = 0x3f9;
        }
    }
    
    //8005dc94
    private void FindSaveFileInMemoryCard(int slotId, string gameTitle, ref int fileIndex)
    {
        //string prefix = slotId == 0 ? "bu00:" : "bu10:";
        //string pattern = prefix + "*";
        //
        //if (!FirstFile(pattern, gameTitle))
        //{
        //    return;
        //}
        //
        //do
        //{
        //    fileIndex++;
        //}
        //while (NextFile(gameTitle));
    }

    //8005df84
    private void FUN_8005df84(int memorySlotId)
    {
        if (memorySlotId == 0 || memorySlotId == 1)
        {
            //format();
        }

        FUN_8005dfe0(memorySlotId);
    }

    //8005dfe0
    private uint FUN_8005dfe0(int memorySlotId)
    {
        int val;
        uint result;
        byte[] buffer = new byte[0x80];
        Array.Clear(buffer);

        //FUN_8005ebec(); //TestEvent();
        //_new_card();
        //_card_read(memorySlotId, 0, buffer);
        val = 3; //FUN_8005eaf8();
        result = 0xffffffff;

        if (val == 0)
        {
            result = 0;

            if (buffer[0] == 'M')
            {
                result = 1;

                if (buffer[1] != 'C')
                {
                    result = 0;
                }
            }
        }

        return result;
    }

    //8005dc04
    private void FUN_8005dc04(int slotId, int[] param_2, int[] param_3)
    {
        int iVar1;
        int uVar2;

        //_card_info();
        iVar1 = 3; //FUN_8005ea5c();
        param_2[slotId] = iVar1;
        //FUN_8005eb94(); //TestEvent

        if (param_2[slotId] == 3)
        {
            //FUN_8005e7c4(slotId); //_card_clear(slotId << 4);
        }

        //_card_load();
        uVar2 = 3;//FUN_8005ea5c();
        param_3[slotId] = uVar2;
        //FUN_8005eb94(); //TestEvent
    }

    //8005dd74
    private int SaveInMemoryCard(int slotId, string gameName, MemoryCardDataBlob memoryCardDataBlob)
    {
        int fd;
        int result = -1;
        string localPath = slotId == 0 ? "bu00:" : "bu10:";

        Debugger.Break();
        //string path = localPath + gameName;
        //fd = open(path, 3);
        //
        //if (fd == -1)
        //{
        //    return -1;
        //}
        //
        //result = read(fd, buffer, 0x2000);
        //
        //if (result == -1)
        //{
        //    close(fd);
        //    return -1;
        //}
        //
        //close(fd);
        return result;
    }

    //80050c64
    private void ResetMemoryCardMenuState()
    {
        if ((_gameEngine.StaticVariables.g_memoryCardMenuState & 4U) != 0)
        {
            _gameEngine.StaticVariables.g_memoryCardMenuState |= 8;
        }
    }

    //80060cf8
    private void TryOpenMemoryCardMenu(string arg1, string arg2, ref int val)
    {
        if (string.IsNullOrEmpty(arg1) && string.IsNullOrEmpty(arg2))
        {
            _gameEngine.StaticVariables.g_fadeFrame = 0x11;
            val = 1000;
        }
        else
        {
            _gameEngine.UIManager.FUN_80050c88(arg1, arg2, ref val);
        }
    }

    //8005ed2c
    private int UpdateSaveGameTransition()
    {
        Debugger.Break();
        return 0;
    }
}