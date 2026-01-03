using System;

namespace AlundraEngine;

public class CdManager
{
    private readonly GameEngine _gameEngine;

    public CdManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    //8005a724
    public void InitCDReading()
    {
        int result;
        byte[] setLocParams = new byte[8];

        if ((_gameEngine.StaticVariables.g_isCdResetRequested != 0 
             || (_gameEngine.StaticVariables.g_cdIsReady != 0 && _gameEngine.StaticVariables.g_cdDataLoaded == 0)) 
            && _gameEngine.StaticVariables.g_cdInitRequired != 0)
        {
            do
            {
                /* pause */
                result = CdControlB('\v', null, 0x0);
            } while (result == 0);

            do
            {
                /* stop */
                result = CdControlB('\t', null, 0x0);
            } while (result == 0);

            setLocParams[0] = 0x80;

            do
            {
                result = CdControlB('\x0e', setLocParams, 0x0);
            } while (result == 0);
            _gameEngine.StaticVariables.g_cdInitRequired = 0;
        }
    }


    // 8005a9e0
    public void StartCdStreaming(int mapIndex)
    {
        bool bVar1;
        Action previousVSyncCallback;

        if ((_gameEngine.StaticVariables.g_isCdResetRequested != 0
             || (_gameEngine.StaticVariables.g_cdIsReady != 0 && _gameEngine.StaticVariables.g_cdDataLoaded == 0))
            && _gameEngine.SoundManager.IsSoundLoading() == false)
        {
            _gameEngine.StaticVariables.g_cdDataStartPtr = _gameEngine.StaticVariables.DAT_CDAranXa_pos + _gameEngine.StaticVariables.g_mapCdDataOffsets[mapIndex * 3];
            _gameEngine.StaticVariables.g_cdDataEndPtr = _gameEngine.StaticVariables.g_cdDataStartPtr + _gameEngine.StaticVariables.g_mapCdDataOffsets[mapIndex * 3 + 2] * 8 + -1;
            _gameEngine.StaticVariables.g_cdReadPtr = _gameEngine.StaticVariables.g_cdDataStartPtr;
            previousVSyncCallback = OnCdDataStreamComplete;

            if (previousVSyncCallback != OnCdDataStreamComplete
                && previousVSyncCallback != null)
            {
                _gameEngine.StaticVariables.g_previousVSyncCallback = previousVSyncCallback;
            }
            _gameEngine.StaticVariables.g_cdControlCommand = 1;
            _gameEngine.StaticVariables.g_cdTrackIndex = (byte)_gameEngine.StaticVariables.g_mapCdDataOffsets[mapIndex * 3 + 1];
            //CdControlF('\r',&StaticVariables.g_cdControlCommand);
            _gameEngine.StaticVariables.g_cdReadComplete = 0;
            _gameEngine.StaticVariables.g_cdInitRequired = 2;
        }
    }

    private int CdControlB(char com, byte[]? param, byte result)

    {
        int iVar1;
        int iVar2;
        int iVar3;
        uint uVar4;
        int iVar5;
        /*
        iVar3 = _gameEngine.StaticVariables.INT_ARRAY_800c8238[0x27];
        iVar5 = 3;
        do
        {
            _gameEngine.StaticVariables.INT_ARRAY_800c8238[0x27] = 0;

            if ((com != 1) && (((byte)_gameEngine.StaticVariables.INT_ARRAY_800c8238[0x2b] & 0x10) != 0))
            {
                //CD_cw(1, (undefined1*)0x0, (undefined1*)0x0, 0);
            }
            if ((param == 0x0 || (_gameEngine.StaticVariables.INT_ARRAY_800c8238[com + 7] == 0) ||
                (iVar1 = CD_cw(2, param, result, 0), iVar1 == 0))
            {
                _gameEngine.StaticVariables.INT_ARRAY_800c8238[0x27] = iVar3;
                iVar1 = CD_cw(com, param, result, 0);
                iVar2 = 0;
                if (iVar1 == 0) break;
            }
            iVar5 = iVar5 + -1;
            iVar2 = -1;
            _gameEngine.StaticVariables.INT_ARRAY_800c8238[0x27] = iVar3;
        } while (iVar5 != -1);
        if (iVar2 != 0)
        {
            return 0;
        }

        iVar3 = CD_sync(0, result);
        SYS_OBJ_538();
        */
        return 1; //iVar3 == 2 ? 1 : 0;
    }

    //8005a7d4
    public bool FUN_8005a7d4()
    {
        bool bVar1;

        return false;

        if ((_gameEngine.StaticVariables.g_isCdResetRequested == 0) && ((_gameEngine.StaticVariables.g_cdIsReady == 0 || (_gameEngine.StaticVariables.g_cdDataLoaded != 0))))
        {
            bVar1 = false;
        }
        else
        {
            bVar1 = _gameEngine.StaticVariables.g_cdInitRequired != 0;
        }

        return bVar1;
    }

    //8005ad38
    public void OnCdDataStreamComplete()
    {
        //int syncResult;
        //u_char uVar1;
        //CdlLOC aCStack_30[2];
        //u_char syncStatusBuffer[5];
        //CdlLOC tempPosition[2];
        //int tempCdTimer;
        //
        //if (_gameEngine.StaticVariables.g_isCdResetRequested == 0)
        //{
        //    if (_gameEngine.StaticVariables.g_cdIsReady == 0)
        //    {
        //        return;
        //    }
        //
        //    if (_gameEngine.StaticVariables.g_cdDataLoaded != 0)
        //    {
        //        return;
        //    }
        //}
        //
        //if ((_gameEngine.StaticVariables.g_cdInitRequired & 2U) != 0)
        //{
        //    syncResult = CdSync(1, syncStatusBuffer);
        //    if (syncResult == 5)
        //    {
        //        CdControlF('\r', &_gameEngine.StaticVariables.g_cdControlCommand);
        //        tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //    }
        //    else
        //    {
        //        tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //        if (syncResult == 2)
        //        {
        //            _gameEngine.StaticVariables.g_cdInitRequired = 4;
        //            tempPosition[0].track = 0xc9;
        //            CdControlF('\x0e', &tempPosition[0].track);
        //            tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //        }
        //    }
        //    goto END;
        //}
        //
        //if ((_gameEngine.StaticVariables.g_cdInitRequired & 4U) == 0)
        //{
        //    if ((_gameEngine.StaticVariables.g_cdInitRequired & 0x40U) == 0)
        //    {
        //        if ((_gameEngine.StaticVariables.g_cdInitRequired & 0x10U) == 0)
        //        {
        //            if ((_gameEngine.StaticVariables.g_cdInitRequired & 0x20U) == 0)
        //            {
        //                if ((_gameEngine.StaticVariables.g_cdInitRequired & 1U) != 0)
        //                {
        //                    tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay + -1;
        //
        //                    if (_gameEngine.StaticVariables.g_cdStreamDelay == 0)
        //                    {
        //                        syncResult = CdSync(1, syncStatusBuffer);
        //
        //                        if (syncResult == 5)
        //                        {
        //                            CdControlF('\x06', (u_char*)0x0);
        //                            _gameEngine.StaticVariables.g_cdReadComplete = 1;
        //                            tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //                        }
        //                        else
        //                        {
        //                            tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //                            
        //                            if (syncResult == 2)
        //                            {
        //                                if (_gameEngine.StaticVariables.g_cdReadComplete == 1)
        //                                {
        //                                    CdControlF('\x11', (u_char*)0x0);
        //                                    _gameEngine.StaticVariables.g_cdReadComplete = 0;
        //                                    tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //                                }
        //                                else if (_gameEngine.StaticVariables.g_cdReadPtr < _gameEngine.StaticVariables.g_cdDataEndPtr)
        //                                {
        //                                    tempCdTimer = CdPosToInt(tempPosition);
        //                                    if (0 < tempCdTimer)
        //                                    {
        //                                        _gameEngine.StaticVariables.g_cdReadPtr = tempCdTimer;
        //                                    }
        //                                    CdControlF('\x11', (u_char*)0x0);
        //                                    _gameEngine.StaticVariables.g_cdStreamDelay = 5;
        //                                    tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //                                }
        //                                else
        //                                {
        //                                    CdControlF('\v', (u_char*)0x0);
        //                                    _gameEngine.StaticVariables.g_cdInitRequired = 0x80;
        //                                    tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //                                }
        //                            }
        //                        }
        //                    }
        //                    goto END;
        //                }
        //
        //                if ((_gameEngine.StaticVariables.g_cdInitRequired & 0x80U) != 0)
        //                {
        //                    tempCdTimer = CdSync(1, syncStatusBuffer);
        //                    if (tempCdTimer == 5)
        //                    {
        //                        CdControlF('\v', (u_char*)0x0);
        //                    }
        //                    else if (tempCdTimer == 2)
        //                    {
        //                        tempPosition[0].track = 0x80;
        //                        CdControlF('\x0e', &tempPosition[0].track);
        //                        _gameEngine.StaticVariables.g_cdInitRequired = 0x100;
        //                    }
        //                }
        //
        //                if ((_gameEngine.StaticVariables.g_cdInitRequired & 0x100U) != 0)
        //                {
        //                    tempCdTimer = CdSync(1, syncStatusBuffer);
        //                    if (tempCdTimer == 5)
        //                    {
        //                        tempPosition[0].track = 0x80;
        //                        CdControlF('\x0e', &tempPosition[0].track);
        //                    }
        //                    else if (tempCdTimer == 2)
        //                    {
        //                        CdControlF('\t', (u_char*)0x0);
        //                        _gameEngine.StaticVariables.g_cdInitRequired = 8;
        //                    }
        //                }
        //
        //                tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //
        //                if ((_gameEngine.StaticVariables.g_cdInitRequired & 8U) == 0) goto END;
        //
        //                syncResult = CdSync(1, syncStatusBuffer);
        //
        //                if (syncResult != 5)
        //                {
        //                    tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //
        //                    if (syncResult == 2)
        //                    {
        //                        if (_gameEngine.StaticVariables.g_previousVSyncCallback != null)
        //                        {
        //                            VSyncCallback(_gameEngine.StaticVariables.g_previousVSyncCallback);
        //                        }
        //
        //                        _gameEngine.StaticVariables.g_cdInitRequired = 0;
        //                        tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //                    }
        //                    goto END;
        //                }
        //                uVar1 = '\t';
        //            }
        //            else
        //            {
        //                syncResult = CdSync(1, syncStatusBuffer);
        //
        //                if (syncResult != 5)
        //                {
        //                    tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //
        //                    if (syncResult == 2)
        //                    {
        //                        _gameEngine.StaticVariables.g_cdInitRequired = 1;
        //                        _gameEngine.StaticVariables.g_cdReadPtr = _gameEngine.StaticVariables.g_cdDataStartPtr;
        //                        CdIntToPos(_gameEngine.StaticVariables.g_cdDataStartPtr, aCStack_30);
        //                        CdControlF('\x06', (u_char*)0x0);
        //                        _gameEngine.StaticVariables.g_cdStreamDelay = 0x14;
        //                        tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //                    }
        //                    goto END;
        //                }
        //
        //                _gameEngine.StaticVariables.g_cdInitRequired = 0x20;
        //                uVar1 = '\f';
        //            }
        //        }
        //        else
        //        {
        //            syncResult = CdSync(1, syncStatusBuffer);
        //
        //            if (syncResult == 5)
        //            {
        //                CdIntToPos(_gameEngine.StaticVariables.g_cdDataStartPtr, aCStack_30);
        //                uVar1 = '\x15';
        //            }
        //            else
        //            {
        //                tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //                if (syncResult != 2) goto END;
        //                _gameEngine.StaticVariables.g_cdInitRequired = 0x20;
        //                uVar1 = '\f';
        //            }
        //        }
        //    }
        //    else
        //    {
        //        syncResult = CdSync(1, syncStatusBuffer);
        //
        //        if (syncResult == 5)
        //        {
        //            uVar1 = '\x02';
        //            goto LAB_8005ae90;
        //        }
        //
        //        tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //        if (syncResult != 2) goto END;
        //        _gameEngine.StaticVariables.g_cdInitRequired = 0x10;
        //        _gameEngine.StaticVariables.g_cdReadPtr = g_cdDataStartPtr;
        //        CdIntToPos(_gameEngine.StaticVariables.g_cdDataStartPtr, aCStack_30);
        //        uVar1 = '\x15';
        //    }
        //    CdControlF(uVar1, (u_char*)0x0);
        //    tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //}
        //else
        //{
        //    syncResult = CdSync(1, syncStatusBuffer);
        //
        //    if (syncResult != 5)
        //    {
        //        tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //        if (syncResult == 2)
        //        {
        //            _gameEngine.StaticVariables.g_cdInitRequired = 0x40;
        //            _gameEngine.StaticVariables.g_cdReadPtr = _gameEngine.StaticVariables.g_cdDataStartPtr;
        //            CdIntToPos(_gameEngine.StaticVariables.g_cdDataStartPtr, aCStack_30);
        //            CdControlF('\x02', &aCStack_30[0].minute);
        //            tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //        }
        //        goto END;
        //    }
        //    uVar1 = '\x0e';
        //    LAB_8005ae90:
        //    tempPosition[0].track = 0xc9;
        //    CdControlF(uVar1, &tempPosition[0].track);
        //    tempCdTimer = _gameEngine.StaticVariables.g_cdStreamDelay;
        //}

        END:
        _gameEngine.StaticVariables.g_cdStreamDelay = 0; //tempCdTimer;

        if (_gameEngine.StaticVariables.g_previousVSyncCallback != null &&
           _gameEngine.StaticVariables.g_previousVSyncCallback != OnCdDataStreamComplete)
        {
            _gameEngine.StaticVariables.g_previousVSyncCallback();
        }
    }

    //8005abe0
    public void SetCdToAranXaMusicIndex(int mode)
    {
        /*
           CdlLOC cdlLoc [2];
           u_char buffer [8];

           if ((g_isCdResetRequested != 0) || ((g_cdIsReady != 0 && (g_cdDataLoaded == 0)))) {
             g_cdDataStartPtr = DAT_CDAranXa_pos + g_mapCdDataOffsets[mode * 3];
             CdIntToPos(g_cdDataStartPtr,cdlLoc);
             CdControl('\x02',&cdlLoc[0].minute,buffer);
             CdControl('\x15',(u_char *)0x0,buffer);
           }
         */
    }
}