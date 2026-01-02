using AlundraEngine.Graphics;
using System.Diagnostics;
using static AlundraEngine.Renderer;

namespace AlundraEngine.UI;

public class MemoryCardManager
{
    private readonly GameEngine _gameEngine;
    private readonly List<Sprite> MemoryFileBlocSprites = new();

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

        return ReadFromMemoryCard(slotId, gameTitle, _gameEngine.StaticVariables.g_memoryCardDataBlob);
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
        if (ReadFromMemoryCard(slotId, gameTitle, _gameEngine.StaticVariables.g_memoryCardDataBlob) == -1)
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
    private int DisplayUIMemoryCardFiles(string arg1, string arg2, ref uint payloadOffset)
    {
        _gameEngine.HudManager.InitializeHudPosition();
        _gameEngine.StaticVariables.PTR_80180128 = payloadOffset;
        _gameEngine.StaticVariables.g_memoryCardOffsetArg1 = arg1;
        _gameEngine.StaticVariables.g_memoryCardOffsetArg2 = arg2;
        payloadOffset = 0xffffffff;
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
            bVar1 = true;
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
        return;
        //Debugger.Break();
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
        val = 0; //FUN_8005eaf8();
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
        iVar1 = 0; //FUN_8005ea5c();
        param_2[slotId] = iVar1;
        //FUN_8005eb94(); //TestEvent

        if (param_2[slotId] == 3)
        {
            //FUN_8005e7c4(slotId); //_card_clear(slotId << 4);
        }

        //_card_load();
        uVar2 = 0;//FUN_8005ea5c();
        param_3[slotId] = uVar2;
        //FUN_8005eb94(); //TestEvent
    }

    //8005dd74
    private int ReadFromMemoryCard(int slotId, string gameName, MemoryCardDataBlob memoryCardDataBlob)
    {
        int fd;
        int result = -1;
        string localPath = "saves";//slotId == 0 ? "bu00:" : "bu10:";

        //string fileName = localPath + gameName;
        //fd = open(fileName, 3);
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


        string fileName = Path.Combine(localPath, gameName);

        if (!File.Exists(fileName))
        {
            return -1;
        }

        result = 1;

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

    private int StartMemoryCardProcess()
    {
        int result = 0;

        /* locals nécessaires à quelques appels (écriture via pointeurs) */
        int localStatus = 0;
        int fadeCounter = 0;

        switch (_gameEngine.StaticVariables.g_globalTransitionState)
        {
            /* ------------------------------------------------------------ */
            /* 0x2710 / 0x2711 : entrée menu memory card + fade -> state 1   */
            /* ------------------------------------------------------------ */

            case 0x2710:
                {
                    _gameEngine.StaticVariables.g_isMemoryCopyInProgress = 1;
                    _gameEngine.StaticVariables.g_fadeFrame = 0;
                    _gameEngine.StaticVariables.g_playerControlFlags |= 0x8;

                    string s0 = _gameEngine.EtcRes.GetEtcString(0x87);
                    string s1 = _gameEngine.EtcRes.GetEtcString(0x88);
                    TryOpenMemoryCardMenu(s0, s1, ref _gameEngine.StaticVariables.g_openMemoryCardState);

                    _gameEngine.StaticVariables.g_globalTransitionState = 0x2711;
                    return result;
                }

            case 0x2711:
                {
                    if (!AdvanceFadeOldCheck(0x13))
                    {
                        return result;
                    }
                    _gameEngine.StaticVariables.g_globalTransitionState = 1;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 1 / 2 : sélection slot / lecture de résultat INT_ARRAY_*      */
            /* ------------------------------------------------------------ */

            case 1:
                {
                    _gameEngine.StaticVariables.g_fadeSubstate = 0;
                    FUN_8005dc04(0, _gameEngine.StaticVariables.INT_ARRAY_80191088, _gameEngine.StaticVariables.INT_ARRAY_80191080);

                    localStatus = _gameEngine.StaticVariables.INT_ARRAY_80191088[0];
                    if (localStatus == 0 || localStatus == 3)
                    {
                        _gameEngine.StaticVariables.g_memorySlotId = 0;
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3ec;
                    }
                    else
                    {
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3ea;
                    }
                    return result;
                }

            case 0x3ea: /* trampoline -> 2 */
                _gameEngine.StaticVariables.g_globalTransitionState = 2;
                return result;

            case 2:
                {
                    _gameEngine.StaticVariables.g_fadeSubstate = 0;
                    FUN_8005dc04(1, _gameEngine.StaticVariables.INT_ARRAY_80191088, _gameEngine.StaticVariables.INT_ARRAY_80191080);

                    localStatus = _gameEngine.StaticVariables.INT_ARRAY_80191088[1];
                    if (localStatus == 0 || localStatus == 3)
                    {
                        _gameEngine.StaticVariables.g_memorySlotId = 1;
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3fa;
                    }
                    else
                    {
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3eb;
                    }
                    return result;
                }

            case 0x3eb: /* trampoline -> 3 */
                _gameEngine.StaticVariables.g_globalTransitionState = 3;
                return result;

            case 3:
                _gameEngine.StaticVariables.g_fadeSubstate = 0;
                _gameEngine.StaticVariables.g_globalTransitionState = 0x3f9;
                return result;

            case 0x3ec: /* trampoline -> 4 */
                _gameEngine.StaticVariables.g_globalTransitionState = 4;
                return result;

            case 4:
                _gameEngine.StaticVariables.g_fadeSubstate = 0;
                FUN_80060b1c();
                return result;

            /* ------------------------------------------------------------ */
            /* 0x3ed -> fade -> state 5                                      */
            /* ------------------------------------------------------------ */

            case 0x3ed:
                {
                    if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                    {
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                    }

                    if (!AdvanceFadeOldCheck(0x13))
                    {
                        return result;
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 5;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 5 : FUN_8005dc04(slot) puis StartAsyncOperation -> state 0x69 */
            /* ------------------------------------------------------------ */

            case 5:
                {
                    /* Ghidra: a1=sp+0x10 (local_28), a2=sp+0x14 ; on modélise 2 sorties */
                    _gameEngine.StaticVariables.g_fadeSubstate = 0;
                    var tmpA = new int[2];
                    var tmpB = new int[2];
                    FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId, tmpA, tmpB);

                    /* check: (tmpA - 1) < 2  <=> tmpA == 1 || tmpA == 2 */
                    if ((tmpA[0] - 1) >= 2u)
                    {
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3eb;
                        return result;
                    }

                    {
                        string line1 = _gameEngine.EtcRes.GetEtcString(0x8F);
                        string line2 = _gameEngine.EtcRes.GetEtcString(0x90);
                        TryOpenMemoryCardMenu(line1, line2, ref _gameEngine.StaticVariables.g_openMemoryCardState);
                    }

                    _gameEngine.StaticVariables.g_asyncOperationResult = 2;

                    {
                        string arg1 = _gameEngine.EtcRes.GetEtcString(0x81);
                        string arg2 = _gameEngine.EtcRes.GetEtcString(0x82);
                        _gameEngine.StartAsyncOperation(arg1, arg2, result => _gameEngine.StaticVariables.g_asyncOperationResult = result);
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 0x69;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x69 : poll async result -> state 0x3ee / 0x3ef + FADE_RESTART */
            /* ------------------------------------------------------------ */

            case 0x69:
                {
                    int r = _gameEngine.StaticVariables.g_asyncOperationResult;

                    if (r == 1)
                    {
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3ee;
                        _gameEngine.StaticVariables.g_asyncOperationResult = 0;
                        _gameEngine.StaticVariables.g_fadeFrame = 0;

                        /* FADE_RESTART */
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                        return result;
                    }

                    if (r == 2)
                    {
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3ef;
                        _gameEngine.StaticVariables.g_asyncOperationResult = 0;
                        _gameEngine.StaticVariables.g_fadeFrame = 0;

                        /* FADE_RESTART */
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                        return result;
                    }

                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x3ee : sous-états + fade -> state 6                          */
            /* ------------------------------------------------------------ */

            case 0x3ee:
                {
                    if (_gameEngine.StaticVariables.g_fadeSubstate == 1)
                    {
                        if (AdvanceFadeOldCheck(0x1c))
                        {
                            _gameEngine.StaticVariables.g_fadeSubstate = 2;
                        }
                    }

                    if (_gameEngine.StaticVariables.g_fadeSubstate == 2)
                    {
                        string line1 = _gameEngine.EtcRes.GetEtcString(0x91);
                        string line2 = _gameEngine.EtcRes.GetEtcString(0x92);
                        TryOpenMemoryCardMenu(line1, line2, ref _gameEngine.StaticVariables.g_openMemoryCardState);
                        _gameEngine.StaticVariables.g_fadeSubstate = 3;
                    }

                    if (_gameEngine.StaticVariables.g_fadeSubstate == 3)
                    {
                        if (!AdvanceFadeOldCheck(0x13))
                        {
                            return result;
                        }
                        _gameEngine.StaticVariables.g_globalTransitionState = 6;
                        return result;
                    }

                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 6 : check DAT_8019acc4[slot], FUN_8005df84 -> 0x3f3 ou 0x3fc   */
            /* ------------------------------------------------------------ */

            case 6:
                {
                    _gameEngine.StaticVariables.g_fadeSubstate = 0;
                    FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.DAT_8019acc4, _gameEngine.StaticVariables.DAT_8019acbc);

                    if (_gameEngine.StaticVariables.DAT_8019acc4[_gameEngine.StaticVariables.g_memorySlotId] != 0)
                    {
                        /* branche vers LAB_8005fe34 dans le dump (non-zéro) */
                        _gameEngine.StaticVariables.g_globalTransitionState = 
                            (_gameEngine.StaticVariables.DAT_8019acc4[_gameEngine.StaticVariables.g_memorySlotId] == 3) ? 0x3fb : 0x3f9;
                        return result;
                    }

                    /* DAT_8019acc4[slot] == 0 */
                    {
                        FUN_8005df84(_gameEngine.StaticVariables.g_memorySlotId);
                        int r = 0; //FUN_8005df84 if success then 0
                        _gameEngine.StaticVariables.g_fadeFrame = 0;
                        _gameEngine.StaticVariables.g_globalTransitionState = (r == 1) ? 0x3f3 : 0x3fc;
                        return result;
                    }
                }

            /* ------------------------------------------------------------ */
            /* 0x3ef : input(0x80) + fade -> state 7                         */
            /* ------------------------------------------------------------ */

            case 0x3ef:
                {
                    if (_gameEngine.StaticVariables.g_openMemoryCardState == 0x3e8)
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
                        return result;
                    }

                    if (!AdvanceFadeOldCheck(0x13))
                    {
                        return result;
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 7;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 7 : menu + InitializeAsyncOperation -> state 0x6b             */
            /* ------------------------------------------------------------ */

            case 7:
                {
                    _gameEngine.StaticVariables.g_fadeSubstate = 0;

                    {
                        string line1 = _gameEngine.EtcRes.GetEtcString(0x93);
                        string line2 = _gameEngine.EtcRes.GetEtcString(0x94);
                        TryOpenMemoryCardMenu(line1, line2, ref _gameEngine.StaticVariables.g_openMemoryCardState);
                    }

                    {
                        string a0 = _gameEngine.EtcRes.GetEtcString(0x81);
                        string a1 = _gameEngine.EtcRes.GetEtcString(0x82);
                        _gameEngine.InitializeAsyncOperation(a0, a1, result => _gameEngine.StaticVariables.g_asyncOperationResult = result);
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 0x6b;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x6b : poll async -> state 0x3f5 ou 0x3ed + FADE_RESTART       */
            /* ------------------------------------------------------------ */

            case 0x6b:
                {
                    int r = _gameEngine.StaticVariables.g_asyncOperationResult;

                    if (r == 1)
                    {
                        _gameEngine.StaticVariables.g_asyncOperationResult = 0;
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3f5;
                        /* FADE_RESTART */
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                        return result;
                    }

                    if (r == 2)
                    {
                        _gameEngine.StaticVariables.g_asyncOperationResult = 0;
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3ed;
                        /* FADE_RESTART */
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                        return result;
                    }

                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 8 / 0x3f0                                                    */
            /* ------------------------------------------------------------ */

            case 0x3f0: /* trampoline -> 8 */
                _gameEngine.StaticVariables.g_globalTransitionState = 8;
                return result;

            case 8:
                _gameEngine.StaticVariables.g_fadeSubstate = 0;
                FUN_80060bd0();
                return result;

            /* ------------------------------------------------------------ */
            /* 0x3f1 : reset menu + fade -> state 9                          */
            /* ------------------------------------------------------------ */

            case 0x3f1:
                {
                    if (_gameEngine.StaticVariables.g_openMemoryCardState == 0x3e8)
                    {
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                    }

                    ResetMemoryCardMenuState();
                    _gameEngine.StaticVariables.g_fadeSubstate = 1;

                    if (!AdvanceFadeOldCheck(0x13))
                    {
                        return result;
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 9;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 9 : FUN_8005dc04 + FindSaveFileInMemoryCard + somme -> 0x3ff / 0x3f2
             *     sinon si DAT_8019acc4 != 0 -> 0x3fb/0x3f9
             * ------------------------------------------------------------ */
            case 9:
                {
                    _gameEngine.StaticVariables.g_fadeSubstate = 0;

                    FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.DAT_8019acc4, _gameEngine.StaticVariables.DAT_8019acbc);

                    if (_gameEngine.StaticVariables.DAT_8019acc4[_gameEngine.StaticVariables.g_memorySlotId] != 0)
                    {
                        _gameEngine.StaticVariables.g_globalTransitionState = (_gameEngine.StaticVariables.DAT_8019acc4[_gameEngine.StaticVariables.g_memorySlotId] == 3) ? 0x3fb : 0x3f9;
                        return result;
                    }

                    /* DAT_8019acc4 == 0 -> recherche et somme */
                    FindSaveFileInMemoryCard(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.g_gameTitle, ref _gameEngine.StaticVariables.g_memoryCardFileIndex);

                    fadeCounter = 0;

                    if (_gameEngine.StaticVariables.g_memoryCardFileIndex > 0)
                    {
                        /* dans le dump: boucle somme sur un tableau d'entrées (stride 0x28, add 0x18) */
                        /* Ici on laisse en placeholder : tu peux recâbler sur ta structure de directory. */
                        /* TODO: remplacer par itération réelle sur tes entrées MC. */
                    }

                    /* compare avec 0x1C0000 (lui 1; ori C000) */
                    if (fadeCounter <= (int)0x001C0000)
                    {
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3ff;
                        return result;
                    }

                    {
                        string line1 = _gameEngine.EtcRes.GetEtcString(0x97);
                        string line2 = _gameEngine.EtcRes.GetEtcString(0x98);
                        TryOpenMemoryCardMenu(line1, line2, ref _gameEngine.StaticVariables.g_openMemoryCardState);
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 0x3f2;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x3f2 : input(0x20) + fade -> state 0x0A                      */
            /* ------------------------------------------------------------ */

            case 0x3f2:
                {
                    if (_gameEngine.StaticVariables.g_openMemoryCardState == 0x3e8)
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
                        return result;
                    }

                    if (!AdvanceFadeOldCheck(0x13))
                    {
                        return result;
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 0x0A;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x0A : bloc LAB_8005fecc (création/prepare payload) -> 0x3f7   */
            /* ------------------------------------------------------------ */

            case 0x0A:
                {
                    _gameEngine.StaticVariables.g_fadeSubstate = 0;

                    FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId,
                                 _gameEngine.StaticVariables.DAT_8019acc4,
                                 _gameEngine.StaticVariables.DAT_8019acbc);

                    if (_gameEngine.StaticVariables.DAT_8019acc4[_gameEngine.StaticVariables.g_memorySlotId] != 0)
                    {
                        _gameEngine.StaticVariables.g_globalTransitionState = 
                            (_gameEngine.StaticVariables.DAT_8019acc4[_gameEngine.StaticVariables.g_memorySlotId] == 3) ? 0x3fb : 0x3f9;
                        return result;
                    }

                    /* DAT_8019acc4 == 0 : appels FUN_8005e3e4 + DisplayUIMemoryCardFiles dans le dump */
                    /* TODO: recâbler précisément si tu as les types exacts */
                    {
                        /* dans le dump: FUN_8005e3e4(..., _gameEngine.StaticVariables.g_memoryCardFileIndex, PTR_8018ed68) renvoie un ptr (v0) */
                        /* on omet ici faute de prototype exact, mais on reproduit l’idée: prépare des pointeurs + payloadOffset */
                        var arg1 = _gameEngine.EtcRes.GetEtcString(0x85);
                        var arg2 = _gameEngine.EtcRes.GetEtcString(0x86);

                        /* DisplayUIMemoryCardFiles(PTR_8018ed68, PTR_8018ede8, &_gameEngine.StaticVariables.g_memoryCardPayloadOffset) */
                        /* NB: le dump stocke aussi un champ à +4, etc. */
                        DisplayUIMemoryCardFiles(arg1, arg2, ref _gameEngine.StaticVariables.g_memoryCardPayloadOffset);

                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3f7;
                        return result;
                    }
                }

            /* ------------------------------------------------------------ */
            /* 0x3f7 : check payloadOffset (-1/-2) + menu -> state 0x0F      */
            /* ------------------------------------------------------------ */

            case 0x3f7:
                {
                    if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset == 0xFFFFFFFF)
                    {
                        return result;
                    }

                    if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset == 0xFFFFFFFE)
                    {
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x0F;
                        _gameEngine.StaticVariables.g_fadeFrame = 0;
                        return result;
                    }

                    {
                        string line1 = _gameEngine.EtcRes.GetEtcString(0xA3);
                        string line2 = _gameEngine.EtcRes.GetEtcString(0xA4);
                        TryOpenMemoryCardMenu(line1, line2, ref _gameEngine.StaticVariables.g_openMemoryCardState);
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 0x0F;
                    _gameEngine.StaticVariables.g_fadeFrame = 0;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x0F : fade -> (delete file?) -> 0x3ff / 0x3fb / 0x3f9 / 0x3f6 */
            /* ------------------------------------------------------------ */

            case 0x0F:
                {
                    if (!AdvanceFadeOldCheck(0x13))
                    {
                        return result;
                    }

                    if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset == 0xFFFFFFFE)
                    {
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                        _gameEngine.StaticVariables.g_fadeFrame = 0;
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3f6;
                        return result;
                    }

                    _gameEngine.StaticVariables.g_fadeSubstate = 0;
                    FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId,
                        _gameEngine.StaticVariables.DAT_8019acc4,
                                 _gameEngine.StaticVariables.DAT_8019acbc);

                    if (_gameEngine.StaticVariables.DAT_8019acc4[_gameEngine.StaticVariables.g_memorySlotId] == 0)
                    {
                        /* delete file correspondant à payloadOffset */
                        /* dans le dump: index = payloadOffset; calc: (idx*4 + idx) * 8 = idx*40 ; + base */
                        /* TODO: remplacer la base/structure réelle (ici placeholder) */
                        DeleteMemoryCardFile(_gameEngine.StaticVariables.g_memorySlotId, null);

                        _gameEngine.StaticVariables.g_fadeSubstate = 0;
                        _gameEngine.StaticVariables.g_fadeFrame = 0;
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3ff;
                        return result;
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = (_gameEngine.StaticVariables.DAT_8019acc4[_gameEngine.StaticVariables.g_memorySlotId] == 3) ? 0x3fb : 0x3f9;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x3f3 : séquence en 3 sous-états puis fade -> state 0x0B      */
            /* ------------------------------------------------------------ */

            case 0x3f3:
                {
                    if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                    {
                        FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId,
                                     _gameEngine.StaticVariables.DAT_8019acc4,
                                     _gameEngine.StaticVariables.DAT_8019acbc);
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                    }

                    if (_gameEngine.StaticVariables.g_fadeSubstate == 1)
                    {
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 2;
                    }

                    if (_gameEngine.StaticVariables.g_fadeSubstate == 2)
                    {
                        if (!AdvanceFadeOldCheck(0x13))
                        {
                            return result;
                        }
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x0B;
                        return result;
                    }

                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x0B : check DAT_8019acc4[slot]; si 0 -> FUN_80060e20 ; -> 0x3f4
             *      sinon -> 0x3fb/0x3f9
             * ------------------------------------------------------------ */
            case 0x0B:
                {
                    _gameEngine.StaticVariables.g_fadeSubstate = 0;

                    if (_gameEngine.StaticVariables.DAT_8019acc4[_gameEngine.StaticVariables.g_memorySlotId] == 0)
                    {
                        int r = BuildDataAndSaveInMemoryCard(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.g_gameTitle);
                        if (r == -1)
                        {
                            _gameEngine.StaticVariables.g_globalTransitionState = 0x3fd;
                            return result;
                        }
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3f4;
                        return result;
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = (_gameEngine.StaticVariables.DAT_8019acc4[_gameEngine.StaticVariables.g_memorySlotId] == 3) ? 0x3fb : 0x3f9;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x3f4 : SaveInMemoryCard + reset + fade -> state 0x0C         */
            /* ------------------------------------------------------------ */

            case 0x3f4:
                {
                    if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                    {
                        ReadFromMemoryCard(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.g_gameTitle, _gameEngine.StaticVariables.g_memoryCardDataBlob);
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                    }

                    if (!AdvanceFadeOldCheck(0x13))
                    {
                        return result;
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 0x0C;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x0C : gros bloc build payload + loop(4) + FUN_80058ab4 -> 0x3f8 */
            /* ------------------------------------------------------------ */

            case 0x0C:
                {
                    _gameEngine.StaticVariables.g_fadeSubstate = 0;

                    //Debugger.Break();

                    /* loop 4 fois (strides 0x76C) – on reproduit le schéma */
                    for (int i = 0; i < 4; i++)
                    {
                        //var src = _gameEngine.StaticVariables.g_memoryCardDataBlob.SavePayload[8 + (i * 0x76c)];
                        //var resOut = FUN_800818e4(src);
                        //
                        ///* dump: écrit 2 pointeurs par entrée dans une table (PTR_8018ed68) */
                        ///* TODO: remapper PTR_8018ed68 à ta vraie table [4][2] */
                        //resOut;
                    }

                    _gameEngine.StaticVariables.INT_8018ed88 = 0;
                    _gameEngine.StaticVariables.INT_8018ed8c = 0;

                    var arg1 = _gameEngine.EtcRes.GetEtcString(0x83);
                    var arg2 = _gameEngine.EtcRes.GetEtcString(0x84);

                    DisplayUIMemoryCardFiles(arg1, arg2, ref _gameEngine.StaticVariables.g_memoryCardPayloadOffset);

                    _gameEngine.StaticVariables.g_fadeFrame = 0;
                    _gameEngine.StaticVariables.g_globalTransitionState = 0x3f8;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x3f8 : check payloadOffset (-1/-2) + fade + menu -> state 0x10 */
            /* ------------------------------------------------------------ */

            case 0x3f8:
                {
                    if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset == 0xFFFFFFFF)
                    {
                        return result;
                    }

                    if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset == 0xFFFFFFFE)
                    {
                        if (!AdvanceFadeOldCheck(0x13))
                        {
                            return result;
                        }
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x10;
                        return result;
                    }

                    if (!AdvanceFadeOldCheck(0x13))
                    {
                        return result;
                    }

                    {
                        string line1 = _gameEngine.EtcRes.GetEtcString(0xA5);
                        string line2 = _gameEngine.EtcRes.GetEtcString(0xA6);
                        TryOpenMemoryCardMenu(line1, line2, ref _gameEngine.StaticVariables.g_openMemoryCardState);
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 0x10;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x10 : fade -> FUN_8005dc04 + save/update -> 0x3fe / 0x3fd / 0x3f6 ... */
            /* ------------------------------------------------------------ */

            case 0x10:
                {
                    if (!AdvanceFadeOldCheck(0x13))
                    {
                        return result;
                    }

                    if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset == 0xFFFFFFFE)
                    {
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                        _gameEngine.StaticVariables.g_fadeFrame = 0;
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3f6;
                        return result;
                    }

                    _gameEngine.StaticVariables.g_fadeSubstate = 0;
                    FUN_8005dc04(_gameEngine.StaticVariables.g_memorySlotId, _gameEngine.StaticVariables.DAT_8019acc4, _gameEngine.StaticVariables.DAT_8019acbc);

                    if (_gameEngine.StaticVariables.DAT_8019acc4[_gameEngine.StaticVariables.g_memorySlotId] == 0)
                    {
                        var r = BuildDataAndSaveInMemoryCardAndUpdateData(_gameEngine.StaticVariables.g_memorySlotId,
                            _gameEngine.StaticVariables.g_gameTitle, _gameEngine.StaticVariables.g_memoryCardPayloadOffset);

                        if ((int)r == -1)
                        {
                            _gameEngine.StaticVariables.g_globalTransitionState = 0x3fd;
                            return result;
                        }

                        _gameEngine.StaticVariables.g_fadeSubstate = 0;
                        _gameEngine.StaticVariables.g_fadeFrame = 0;
                        _gameEngine.StaticVariables.g_globalTransitionState = 0x3fe;
                        return result;
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = (_gameEngine.StaticVariables.DAT_8019acc4[_gameEngine.StaticVariables.g_memorySlotId] == 3) ? 0x3fb : 0x3f9;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x3f6 : fade -> state 0x0E                                    */
            /* ------------------------------------------------------------ */

            case 0x3f6:
                {
                    if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                    {
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                    }

                    if (!AdvanceFadeOldCheck(0x13))
                    {
                        return result;
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 0x0E;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x0E : init HUD hide + state 0x44B                            */
            /* ------------------------------------------------------------ */

            case 0x0E:
                _gameEngine.StaticVariables.g_fadeSubstate = 0;
                _gameEngine.StaticVariables.g_fadeFrame = 0;
                _gameEngine.HudManager.InitializeHudPositionBeforeHide();
                _gameEngine.StaticVariables.g_globalTransitionState = 0x44B;
                return result;

            /* ------------------------------------------------------------ */
            /* 0x44B : reset menu once + fade (limit 0x0B) -> state 0x63     */
            /* ------------------------------------------------------------ */

            case 0x44B:
                {
                    if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                    {
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                    }

                    if (!AdvanceFadeOldCheck(0x0B))
                    {
                        return result;
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 0x63;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x63 : fin : clear flags + state=0                            */
            /* ------------------------------------------------------------ */

            case 0x63:
                _gameEngine.StaticVariables.g_fadeSubstate = 0;
                _gameEngine.StaticVariables.g_fadeFrame = 0;
                _gameEngine.StaticVariables.g_isMemoryCopyInProgress = 0;
                _gameEngine.StaticVariables.g_globalTransitionState = 0;
                _gameEngine.StaticVariables.g_playerControlFlags = (uint)(_gameEngine.StaticVariables.g_playerControlFlags & ~0x8);
                return result;

            /* ------------------------------------------------------------ */
            /* 0x3f5 : input(0x80) + fade -> state 0x0D                      */
            /* ------------------------------------------------------------ */

            case 0x3f5:
                {
                    if (_gameEngine.StaticVariables.g_openMemoryCardState == 0x3e8)
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
                        return result;
                    }

                    if (!AdvanceFadeOldCheck(0x13))
                    {
                        return result;
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 0x0D;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x0D : menu strings 0x9F/0xA0 -> state 0x3f6                  */
            /* ------------------------------------------------------------ */

            case 0x0D:
                {
                    _gameEngine.StaticVariables.g_fadeSubstate = 0;
                    _gameEngine.StaticVariables.g_fadeFrame = 0;

                    {
                        string line1 = _gameEngine.EtcRes.GetEtcString(0x9F);
                        string line2 = _gameEngine.EtcRes.GetEtcString(0xA0);
                        TryOpenMemoryCardMenu(line1, line2, ref _gameEngine.StaticVariables.g_openMemoryCardState);
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 0x3f6;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x3f9 : reset menu once + fade -> si payloadOffset != -1 -> state 0x11 */
            /* ------------------------------------------------------------ */

            case 0x3f9:
                {
                    if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                    {
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                    }

                    if (!AdvanceFadeOldCheck(0x13))
                    {
                        return result;
                    }

                    if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset == 0xFFFFFFFF)
                    {
                        return result;
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 0x11;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x3fa : reset menu once + fade -> si payloadOffset != -1 -> state 0x12 */
            /* ------------------------------------------------------------ */

            case 0x3fa:
                {
                    if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                    {
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                    }

                    if (!AdvanceFadeOldCheck(0x13))
                    {
                        return result;
                    }

                    if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset == 0xFFFFFFFF)
                    {
                        return result;
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 0x12;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x12 : menu 0xAA + 0x?? -> state 0x3f5 (selon dump)           */
            /* ------------------------------------------------------------ */

            case 0x12:
                {
                    _gameEngine.StaticVariables.g_fadeSubstate = 0;
                    _gameEngine.StaticVariables.g_fadeFrame = 0;

                    {
                        string line1 = _gameEngine.EtcRes.GetEtcString(0xAA);
                        string line2 = _gameEngine.EtcRes.GetEtcString(0xAA); /* placeholder: dans dump c’est AA puis (après) */
                        TryOpenMemoryCardMenu(line1, line2, ref _gameEngine.StaticVariables.g_openMemoryCardState);
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 0x3f5;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x3fb : reset menu once + fade -> si payloadOffset != -1 -> state 0x13 */
            /* ------------------------------------------------------------ */

            case 0x3fb:
                {
                    if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                    {
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                    }

                    if (!AdvanceFadeOldCheck(0x13))
                    {
                        return result;
                    }

                    if (_gameEngine.StaticVariables.g_memoryCardPayloadOffset == 0xFFFFFFFF)
                    {
                        return result;
                    }

                    _gameEngine.StaticVariables.g_globalTransitionState = 0x13;
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x13 : menu 0xAB/0xAC puis jump vers LAB_800609c0 (non fourni) */
            /* ------------------------------------------------------------ */

            case 0x13:
                {
                    _gameEngine.StaticVariables.g_fadeSubstate = 0;
                    _gameEngine.StaticVariables.g_fadeFrame = 0;

                    {
                        string line1 = _gameEngine.EtcRes.GetEtcString(0xAB);
                        string line2 = _gameEngine.EtcRes.GetEtcString(0xAC);
                        TryOpenMemoryCardMenu(line1, line2, ref _gameEngine.StaticVariables.g_openMemoryCardState);
                    }

                    /* TODO: ton dump continue via LAB_800609c0 (non présent dans l’extrait). */
                    return result;
                }

            /* ------------------------------------------------------------ */
            /* 0x3fc : début du bloc LAB_80060818 (dump tronqué chez toi)    */
            /* ------------------------------------------------------------ */

            case 0x3fc:
                {
                    if (_gameEngine.StaticVariables.g_fadeSubstate == 0)
                    {
                        ResetMemoryCardMenuState();
                        _gameEngine.StaticVariables.g_fadeSubstate = 1;
                    }

                    /* Ici ton extrait s’arrête au chargement de g_fadeFrame (LAB_80060840). */
                    /* TODO: compléter avec la suite du dump (après 80060840). */
                    return result;
                }

            /* ------------------------------------------------------------ */
            default:
                return result;
        }
    }

    private bool AdvanceFadeOldCheck(int limitInclusiveOld)
    {
        int old = _gameEngine.StaticVariables.g_fadeFrame;
        _gameEngine.StaticVariables.g_fadeFrame = old + 1;

        if (old < limitInclusiveOld)
        {
            return false; // pas fini
        }

        _gameEngine.StaticVariables.g_fadeFrame = 0;

        return true; // fini
    }

    //800583ec
    //Display all memory card files
    public void InitializeMemoryCardMenu(CallBackInfo callbackInfo)
    {
        Debugger.Break();

        callbackInfo.RenderFunc = DisplayMemoryCardMenu;

        //var i = 0;
        //ppUVar2 = &PTR_800c419c;
        //psVar3 = SHORT_ARRAY_800c436c;
        //do
        //{
        //    psVar3 = psVar3 + 2;
        //    i = i + 1;
        //    ppUVar2[1].Y = *psVar3;
        //    *ppUVar2 = (UIBoxConfiguration*)0x0;
        //    ppUVar2 = ppUVar2 + 0x1d;
        //} while (i < 4);

        _gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.X = -1;
        _gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.Y = -1;
        _gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.Width = 0;
        _gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.Height = 0;

        _gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] = 1;
        _gameEngine.StaticVariables.UINT_ARRAY_800c4190[1] = 0;
        _gameEngine.StaticVariables.UINT_ARRAY_800c4190[2] = 0;
        _gameEngine.StaticVariables.TextToDisplay_800c41a4.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_800c41a4.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_800c41a4.speed = 0xf;
        _gameEngine.StaticVariables.TextToDisplay_800c41a4.x = _gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.X;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_800c41a4.x =
                (short)(_gameEngine.StaticVariables.TextToDisplay_800c41a4.x + _gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.Width * -8);
        }

        _gameEngine.StaticVariables.TextToDisplay_800c41a4.y = 0xf0;
        _gameEngine.StaticVariables.TextToDisplay_800c41a4.startX = _gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.X;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_800c41a4.startX =
                (short)(_gameEngine.StaticVariables.TextToDisplay_800c41a4.startX + _gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.Width * -8);
        }

        _gameEngine.StaticVariables.TextToDisplay_800c41a4.startY = _gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.Y;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_800c41a4.startY =
                (short)(_gameEngine.StaticVariables.TextToDisplay_800c41a4.startY + _gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.Height * -8);
        }

        _gameEngine.StaticVariables.TextToDisplay_800c41a4.originX = _gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.X;
        _gameEngine.StaticVariables.TextToDisplay_800c41a4.originY = _gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.Y;
        _gameEngine.StaticVariables.TextToDisplay_800c4218.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_800c4218.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_800c4218.speed = 0xf;
        _gameEngine.StaticVariables.TextToDisplay_800c4218.x = _gameEngine.StaticVariables.UIBoxConfiguration_800bf2a0.X;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800bf2a0.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_800c4218.x =
                (short)(_gameEngine.StaticVariables.TextToDisplay_800c4218.x + _gameEngine.StaticVariables.UIBoxConfiguration_800bf2a0.Width * -8);
        }

        _gameEngine.StaticVariables.TextToDisplay_800c4218.y = 0xf0;
        _gameEngine.StaticVariables.TextToDisplay_800c4218.startX = _gameEngine.StaticVariables.UIBoxConfiguration_800bf2a0.X;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800bf2a0.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_800c4218.startX =
                (short)(_gameEngine.StaticVariables.TextToDisplay_800c4218.startX + _gameEngine.StaticVariables.UIBoxConfiguration_800bf2a0.Width * -8);
        }

        _gameEngine.StaticVariables.TextToDisplay_800c4218.startY = _gameEngine.StaticVariables.UIBoxConfiguration_800bf2a0.Y;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800bf2a0.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_800c4218.startY =
                (short)(_gameEngine.StaticVariables.TextToDisplay_800c4218.startY + _gameEngine.StaticVariables.UIBoxConfiguration_800bf2a0.Height * -8);
        }

        _gameEngine.StaticVariables.TextToDisplay_800c4218.originX = _gameEngine.StaticVariables.UIBoxConfiguration_800bf2a0.X;
        _gameEngine.StaticVariables.TextToDisplay_800c4218.originY = _gameEngine.StaticVariables.UIBoxConfiguration_800bf2a0.Y;
        _gameEngine.StaticVariables.TextToDisplay_800c428c.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_800c428c.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_800c428c.speed = 0xf;
        _gameEngine.StaticVariables.TextToDisplay_800c428c.x = _gameEngine.StaticVariables.UIBoxConfiguration_800c1a10.X;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800c1a10.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_800c428c.x =
                (short)(_gameEngine.StaticVariables.TextToDisplay_800c428c.x + _gameEngine.StaticVariables.UIBoxConfiguration_800c1a10.Width * -8);
        }

        _gameEngine.StaticVariables.TextToDisplay_800c428c.y = 0xf0;
        _gameEngine.StaticVariables.TextToDisplay_800c428c.startX = _gameEngine.StaticVariables.UIBoxConfiguration_800c1a10.X;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800c1a10.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_800c428c.startX =
                (short)(_gameEngine.StaticVariables.TextToDisplay_800c428c.startX + _gameEngine.StaticVariables.UIBoxConfiguration_800c1a10.Width * -8);
        }

        _gameEngine.StaticVariables.TextToDisplay_800c428c.startY = _gameEngine.StaticVariables.UIBoxConfiguration_800c1a10.Y;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800c1a10.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_800c428c.startY =
                (short)(_gameEngine.StaticVariables.TextToDisplay_800c428c.startY + _gameEngine.StaticVariables.UIBoxConfiguration_800c1a10.Height * -8);
        }

        _gameEngine.StaticVariables.TextToDisplay_800c428c.originX = _gameEngine.StaticVariables.UIBoxConfiguration_800c1a10.X;
        _gameEngine.StaticVariables.TextToDisplay_800c428c.originY = _gameEngine.StaticVariables.UIBoxConfiguration_800c1a10.Y;
        _gameEngine.StaticVariables.TextToDisplay_800c4300.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_800c4300.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_800c4300.speed = 0xf;
        _gameEngine.StaticVariables.TextToDisplay_800c4300.x = _gameEngine.StaticVariables.UIBoxConfiguration_800c4180.X;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800c4180.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_800c4300.x =
                (short)(_gameEngine.StaticVariables.TextToDisplay_800c4300.x + _gameEngine.StaticVariables.UIBoxConfiguration_800c4180.Width * -8);
        }

        _gameEngine.StaticVariables.TextToDisplay_800c4300.y = 0xf0;
        _gameEngine.StaticVariables.TextToDisplay_800c4300.startX = _gameEngine.StaticVariables.UIBoxConfiguration_800c4180.X;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800c4180.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_800c4300.startX =
                (short)(_gameEngine.StaticVariables.TextToDisplay_800c4300.startX + _gameEngine.StaticVariables.UIBoxConfiguration_800c4180.Width * -8);
        }

        _gameEngine.StaticVariables.TextToDisplay_800c4300.startY = _gameEngine.StaticVariables.UIBoxConfiguration_800c4180.Y;

        if (_gameEngine.StaticVariables.UIBoxConfiguration_800c4180.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_800c4300.startY =
                (short)(_gameEngine.StaticVariables.TextToDisplay_800c4300.startY + _gameEngine.StaticVariables.UIBoxConfiguration_800c4180.Height * -8);
        }

        _gameEngine.StaticVariables.TextToDisplay_800c4300.originX = _gameEngine.StaticVariables.UIBoxConfiguration_800c4180.X;
        _gameEngine.StaticVariables.TextToDisplay_800c4300.originY = _gameEngine.StaticVariables.UIBoxConfiguration_800c4180.Y;
        InitializeUIMemoryFileBox(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[0], 0, 0, 0, 0x40, 0x40, 0x40, 0xf);
        InitializeUIMemoryFileBox(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[1], 0, 0, 0, 0x80, 0x80, 0x80, 0xf);
        InitializeUIMemoryFileBox(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[2], 0, 0, 0, 0x40, 0x40, 0x40, 0xf);
        InitializeUIMemoryFileBox(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[3], 0, 0, 0, 0x40, 0x40, 0x40, 0xf);
        _gameEngine.StaticVariables.TextToDisplay_80180130.mode = 2;
        _gameEngine.StaticVariables.TextToDisplay_80180130.tick = 0;
        _gameEngine.StaticVariables.TextToDisplay_80180130.speed = 0xf;

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_80180130.x =
                 (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X +
                         _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_80180130.x = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        }

        _gameEngine.StaticVariables.TextToDisplay_80180130.y = 0xf0;

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_80180130.startX =
                 (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X +
                         _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_80180130.startX = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        }

        if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y < 0)
        {
            _gameEngine.StaticVariables.TextToDisplay_80180130.startY =
                 (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y +
                         _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Height * -8);
        }
        else
        {
            _gameEngine.StaticVariables.TextToDisplay_80180130.startY = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y;
        }

        _gameEngine.StaticVariables.TextToDisplay_80180130.originX = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        _gameEngine.StaticVariables.TextToDisplay_80180130.originY = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y;

        _gameEngine.UIManager.DisplayIconName(
            _gameEngine.StaticVariables.SPRT_ARRAY_80180210,
            MemoryFileBlocSprites,
            _gameEngine.StaticVariables.g_memoryCardOffsetArg2.ToCharArray(),
            0x40,
            _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X,
            _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y, 
            0);
        _gameEngine.StaticVariables.UINT_ARRAY_800c4190[2] |= 1;

        //Debugger.Break();
        //if (_gameEngine.StaticVariables.g_memoryCardOffsetArg1 != 0)
        //{
        //    i = 0;
        //
        //    do
        //    {
        //        pcVar1 = g_memoryCardOffsetArg1 + i;
        //        i = i + 4;
        //        DoNothing(*(char**)pcVar1);
        //        DoNothing("\r\n");
        //    } while (_gameEngine.StaticVariables.g_memoryCardOffsetArg1[i] != 0);
        //}

        var result = FUN_80058b28(_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0], _gameEngine.StaticVariables.UINT_ARRAY_800c4190[1]);

        if (result != 0)
        {
            _gameEngine.StaticVariables.UINT_ARRAY_800c4190[1] += 1;
        }

        _gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] = (_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] + 1) & 3;
        result = FUN_80058b28(_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0], _gameEngine.StaticVariables.UINT_ARRAY_800c4190[1]);

        if (result != 0)
        {
            _gameEngine.StaticVariables.UINT_ARRAY_800c4190[1] += 1;
        }

        _gameEngine.StaticVariables.INT_80180120 = 0;
        _gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] = _gameEngine.StaticVariables.UINT_ARRAY_800c4190[1] & 3;

        //return 1;
    }

    //80059dc8
    private void InitializeUIMemoryFileBox(UIMemoryFileBox uiBox, int r, int g, int b, int targetR, int targetG, int targetB, int duration)
    {
        uiBox.StartR = r;
        uiBox.StartG = g;
        uiBox.StartB = b;
        uiBox.R = r;
        uiBox.G = g;
        uiBox.B = b;
        uiBox.Tick = 0;
        uiBox.Enabled = 0;
        uiBox.TargetR = targetR;
        uiBox.TargetG = targetG;
        uiBox.TargetB = targetB;
        uiBox.Duration = duration;
    }

    //80058b28
    private int FUN_80058b28(uint param_1, uint param_2)
    {
        Debugger.Break();

        //var piVar2 = _gameEngine.StaticVariables.g_memoryCardOffsetArg1[param_2];
        //var bVar1 = piVar2 == 0;
        var bVar1 = false;

        if (bVar1)
        {
            //(&PTR_800c419c)[param_1 * 0x1d] = (UIBoxConfiguration*)0x0;
        }
        else
        {
            //(&PTR_800c419c)[param_1 * 0x1d] = (UIBoxConfiguration*)0x1;
        
            SPRT[] sprites = [_gameEngine.StaticVariables.SPRT_ARRAY_800c41c0[param_1], _gameEngine.StaticVariables.SPRT_ARRAY_800c41c0[param_1 + 1]];
        
            _gameEngine.UIManager.DisplayIconName(
                sprites,
                MemoryFileBlocSprites,
                _gameEngine.StaticVariables.g_memoryCardOffsetArg1.ToCharArray(),
                0x10,
                _gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.X,
                _gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.Y, 
                (int)(param_1 * 2 + 1));
        
            _gameEngine.UIManager.DisplayIconName(
                sprites,
                MemoryFileBlocSprites,
                _gameEngine.StaticVariables.g_memoryCardOffsetArg2.ToCharArray(), 
                0x10,
                _gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.X,
                _gameEngine.StaticVariables.UIBoxConfiguration_800bcb30.Y, 
                (int)(param_1 * 2 + 2));
        }
        
        //return !bVar1;

        return 1;
    }

    //80058f24
    private void DisplayMemoryCardMenu(CallBackInfo callbackInfo)
    {
        Debugger.Break();

        //ulong uVar1;
        //int iVar2;
        //int iVar3;
        //string arg1;
        //string arg2;
        //uint uVar4;
        //int piVar5;
        //SPRT pSVar6;
        //UIBoxConfiguration pUVar7;
        //uint puVar8;
        //
        //if ((_gameEngine.StaticVariables.UINT_ARRAY_800c4190[2] & 5) == 0)
        //{
        //    if ((_gameEngine.StaticVariables.UINT_ARRAY_800c4190[2] & 2) == 0)
        //    {
        //        if ((_gameEngine.StaticVariables.UINT_ARRAY_800c4190[2] & 8) == 0)
        //        {
        //            if ((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & 0x40) == 0)
        //            {
        //                if (((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & 0x4000) != 0) 
        //                    && (((uint)(&PTR_800c419c)[((_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] + 2 & 3) + 1 & 3) * 0x1d] & 1) != 0))
        //                {
        //                    iVar2 = 0;
        //                    uVar4 = _gameEngine.StaticVariables.UINT_ARRAY_800c4190[0];
        //                    do
        //                    {
        //                        uVar4 = uVar4 + 1 & 3;
        //                        iVar3 = uVar4 * 0x74;
        //                        (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d].Y = _gameEngine.StaticVariables.SHORT_ARRAY_800c436c[iVar2 * 2];
        //                        _gameEngine.StaticVariables.TextToDisplay_800c41a4[iVar3 + 8] = 2;
        //                        _gameEngine.StaticVariables.TextToDisplay_800c41a4[iVar3] = 0;
        //                        _gameEngine.StaticVariables.TextToDisplay_800c41a4[iVar3 + 4] = 0xf;
        //                        pUVar7 = (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d];
        //
        //                        if (pUVar7.X < 0)
        //                        {
        //                            _gameEngine.StaticVariables.TextToDisplay_800c41a4 + iVar3 + 0xc =pUVar7.X + pUVar7.Width * -8;
        //                        }
        //                        else
        //                        {
        //                            _gameEngine.StaticVariables.TextToDisplay_800c41a4 + iVar3 + 0xc = pUVar7.X;
        //                        }
        //
        //                        piVar5 = _gameEngine.StaticVariables.SHORT_ARRAY_800c436c + iVar2 * 2 + 2);
        //
        //                        if (*piVar5 < 0)
        //                        {
        //                            _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0xe) = (short)*piVar5 + (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d].Height * -8;
        //                        }
        //                        else
        //                        {
        //                            _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0xe) = (short)*piVar5;
        //                        }
        //
        //                        pUVar7 = (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d];
        //
        //                        if (pUVar7.X < 0)
        //                        {
        //                            _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0x10) = pUVar7.X + pUVar7.Width * -8;
        //                        }
        //                        else
        //                        {
        //                            _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0x10) = pUVar7.X;
        //                        }
        //
        //                        pUVar7 = (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d];
        //
        //                        if (pUVar7.Y < 0)
        //                        {
        //                            _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0x12) = pUVar7.Y + pUVar7.Height * -8;
        //                        }
        //                        else
        //                        {
        //                            _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0x12) = pUVar7.Y;
        //                        }
        //
        //                        iVar3 = uVar4 * 0x74;
        //
        //                        _gameEngine.StaticVariables.TextToDisplay_800c41a4 + iVar3 + 0x18) = (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d].X;
        //                        _gameEngine.StaticVariables.TextToDisplay_800c41a4 + iVar3 + 0x1a) = (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d].Y;
        //
        //                        iVar2 += 1;
        //
        //                        _gameEngine.StaticVariables.UpdateUiBoxesPosition((&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d], (TextToDisplay*)((int)&TextToDisplay_800c41a4 + iVar3));
        //                    } while (iVar2 < 4);
        //
        //                    InitializeUIMemoryFileBox(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0]], 0, 0, 0, 0x40, 0x40, 0x40, 0xf);
        //                    InitializeUIMemoryFileBox(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[(_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] + 1 & 3)], 0x40, 0x40, 0x40, 0, 0, 0, 0xf);
        //                    InitializeUIMemoryFileBox(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[(_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] + 2 & 3)], 0x80, 0x80, 0x80, 0x40, 0x40, 0x40, 0xf);
        //                    InitializeUIMemoryFileBox(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[(_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] + 3 & 3)], 0x40, 0x40, 0x40, 0x80, 0x80, 0x80, 0xf);
        //                    _gameEngine.StaticVariables.UINT_ARRAY_800c4190[2] |= 4;
        //                    iVar2 = FUN_80058b28(_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0], _gameEngine.StaticVariables.UINT_ARRAY_800c4190[1]);
        //                    
        //                    if (iVar2 != 0)
        //                    {
        //                        _gameEngine.StaticVariables.UINT_ARRAY_800c4190[1] += 1;
        //                    }
        //
        //                    _gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] = _gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] + 1 & 3;
        //                    _gameEngine.StaticVariables.INT_80180120 += 1;
        //                }
        //
        //                iVar2 = 0;
        //
        //                if (((_gameEngine.StaticVariables.g_padState1.ButtonsJustPressedByInterval & 0x1000) != 0) 
        //                    && _gameEngine.StaticVariables.INT_80180120 != 0)
        //                {
        //                    _gameEngine.StaticVariables.UINT_ARRAY_800c4190[2] |= 4;
        //                    uVar4 = _gameEngine.StaticVariables.UINT_ARRAY_800c4190[0];
        //
        //                    do
        //                    {
        //                        iVar3 = uVar4 * 0x74;
        //                        (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d].Y = _gameEngine.StaticVariables.SHORT_ARRAY_800c436c[iVar2 * 2 + 2];
        //                        _gameEngine.StaticVariables.TextToDisplay_800c41a4[iVar3 + 8] = 2;
        //                        _gameEngine.StaticVariables.TextToDisplay_800c41a4[iVar3] = 0;
        //                        _gameEngine.StaticVariables.TextToDisplay_800c41a4[iVar3 + 4] = 0xf;
        //                        pUVar7 = (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d];
        //
        //                        if (pUVar7.X < 0)
        //                        {
        //                            _gameEngine.StaticVariables.TextToDisplay_800c41a4 + iVar3 + 0xc = pUVar7.X + pUVar7.Width * -8;
        //                        }
        //                        else
        //                        {
        //                            _gameEngine.StaticVariables.TextToDisplay_800c41a4 + iVar3 + 0xc = pUVar7.X;
        //                        }
        //
        //                        piVar5 = _gameEngine.StaticVariables.SHORT_ARRAY_800c436c[iVar2 * 2];
        //
        //                        if (*piVar5 < 0)
        //                        {
        //                            _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0xe) = piVar5 + (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d].Height * -8;
        //                        }
        //                        else
        //                        {
        //                            _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0xe = piVar5;
        //                        }
        //
        //                        pUVar7 = (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d];
        //
        //                        if (pUVar7.X < 0)
        //                        {
        //                            _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0x10 =pUVar7.X + pUVar7.Width * -8;
        //                        }
        //                        else
        //                        {
        //                            *(short*)((int)&TextToDisplay_800c41a4 + uVar4 * 0x74 + 0x10) = pUVar7.X;
        //                        }
        //
        //                        pUVar7 = (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d];
        //
        //                        if (pUVar7.Y < 0)
        //                        {
        //                            _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0x12 = pUVar7.Y + pUVar7.Height * -8;
        //                        }
        //                        else
        //                        {
        //                            _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0x12 = pUVar7.Y;
        //                        }
        //
        //                        iVar3 = uVar4 * 0x74;
        //
        //                        _gameEngine.StaticVariables.TextToDisplay_800c41a4 + iVar3 + 0x18) = (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d].X;
        //                        _gameEngine.StaticVariables.TextToDisplay_800c41a4 + iVar3 + 0x1a) = (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d].Y;
        //                        
        //                        _gameEngine.StaticVariables.UpdateUiBoxesPosition((&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d], (TextToDisplay*)((int)&TextToDisplay_800c41a4 + iVar3));
        //
        //                        iVar2 += 1; 
        //                        uVar4 = uVar4 + 1 & 3;
        //                    } while (iVar2 < 4);
        //
        //                    if (1 < _gameEngine.StaticVariables.INT_80180120)
        //                    {
        //                        FUN_80058b28(_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0], _gameEngine.StaticVariables.INT_80180120 + -2);
        //                    }
        //
        //                    InitializeUIMemoryFileBox(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0]], 0, 0, 0, 0x40, 0x40, 0x40, 0xf);
        //                    InitializeUIMemoryFileBox(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[(_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] + 1) & 3], 0x40, 0x40, 0x40, 0x80, 0x80, 0x80, 0xf);
        //                    InitializeUIMemoryFileBox(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[(_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] + 2) & 3], 0x80, 0x80, 0x80, 0x40, 0x40, 0x40, 0xf);
        //                    InitializeUIMemoryFileBox(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[(_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] + 3) & 3], 0x40, 0x40, 0x40, 0, 0, 0, 0xf);
        //                    
        //                    if ((_gameEngine.StaticVariables.g_memoryCardOffsetArg1 + (_gameEngine.StaticVariables.INT_80180120 + -1) * 8 + 8) == 0)
        //                    {
        //                        _gameEngine.StaticVariables.UINT_ARRAY_800c4190[1] = (uint)_gameEngine.StaticVariables.INT_80180120;
        //                    }
        //                    else
        //                    {
        //                        _gameEngine.StaticVariables.UINT_ARRAY_800c4190[1] = (uint)(_gameEngine.StaticVariables.INT_80180120 + 1);
        //                    }
        //
        //                    _gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] = _gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] - 1 & 3;
        //                    _gameEngine.StaticVariables.INT_80180120 += -1;
        //                }
        //            }
        //            else
        //            {
        //                _gameEngine.StaticVariables.g_asyncOperationResult2 = 0;
        //                arg1 = GetEtcString(0x4a);
        //                arg2 = GetEtcString(0x4b);
        //                InitializeAsyncOperation(arg1, arg2, &g_asyncOperationResult2);
        //                DisplayIconName(
        //                    _gameEngine.StaticVariables.SPRT_ARRAY_80180210, 
        //                    (_gameEngine.StaticVariables.g_memoryCardOffsetArg2 + 4), 
        //                    0x40,
        //                    _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X,
        //                    _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y, 
        //                    0);
        //
        //                _gameEngine.StaticVariables.UINT_ARRAY_800c4190[2] = 8;
        //            }
        //        }
        //        else if (_gameEngine.StaticVariables.g_asyncOperationResult2 - 1U < 2)
        //        {
        //            _gameEngine.StaticVariables.UINT_ARRAY_800c4190[2] = 2;
        //            iVar2 = 0;
        //            uVar4 = _gameEngine.StaticVariables.UINT_ARRAY_800c4190[0];
        //
        //            do
        //            {
        //                uVar4 = uVar4 + 1 & 3;
        //                iVar3 = uVar4 * 0x74;
        //
        //                (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d].Y = -1;
        //                _gameEngine.StaticVariables.TextToDisplay_800c41a4 + iVar3 + 8 = 2;
        //                _gameEngine.StaticVariables.TextToDisplay_800c41a4 + iVar3 = 0;
        //                _gameEngine.StaticVariables.TextToDisplay_800c41a4 + iVar3 + 4 = 0xf;
        //                pUVar7 = (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d];
        //
        //                if (pUVar7.X < 0)
        //                {
        //                    _gameEngine.StaticVariables.TextToDisplay_800c41a4 + iVar3 + 0xc = pUVar7.X + pUVar7.Width * -8;
        //                }
        //                else
        //                {
        //                    _gameEngine.StaticVariables.TextToDisplay_800c41a4 + iVar3 + 0xc = pUVar7.X;
        //                }
        //
        //                piVar5 = _gameEngine.StaticVariables.SHORT_ARRAY_800c436c + iVar2 * 2 + 2;
        //
        //                if (*piVar5 < 0)
        //                {
        //                    _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0xe) = (short)*piVar5 + (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d].Height * -8;
        //                }
        //                else
        //                {
        //                    _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0xe) = (short)*piVar5;
        //                }
        //
        //                pUVar7 = (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d];
        //
        //                if (pUVar7.X < 0)
        //                {
        //                    _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0x10) = pUVar7.X + pUVar7.Width * -8;
        //                }
        //                else
        //                {
        //                    _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0x10) = pUVar7.X;
        //                }
        //
        //                pUVar7 = (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d];
        //
        //                if (pUVar7.Y < 0)
        //                {
        //                    _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0x12) = pUVar7.Y + pUVar7.Height * -8;
        //                }
        //                else
        //                {
        //                    _gameEngine.StaticVariables.TextToDisplay_800c41a4 + uVar4 * 0x74 + 0x12) = pUVar7.Y;
        //                }
        //
        //                iVar3 = uVar4 * 0x74;
        //
        //                _gameEngine.StaticVariables.TextToDisplay_800c41a4 + iVar3 + 0x18) = (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d].X;
        //                _gameEngine.StaticVariables.TextToDisplay_800c41a4 + iVar3 + 0x1a) = (&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d].Y;
        //                iVar2 += 1;
        //                UpdateUiBoxesPosition((&PTR_UIBoxConfiguration_800c41a0)[uVar4 * 0x1d], _gameEngine.StaticVariables.TextToDisplay_800c41a4 + iVar3));
        //            } while (iVar2 < 3);
        //
        //            InitializeUIMemoryFileBox(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] + 1 & 3], 0x40, 0x40, 0x40, 0, 0, 0, 0xf);
        //            InitializeUIMemoryFileBox(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] + 2 & 3], 0x80, 0x80, 0x80, 0, 0, 0, 0xf);
        //            InitializeUIMemoryFileBox(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[_gameEngine.StaticVariables.UINT_ARRAY_800c4190[0] + 3 & 3], 0x40, 0x40, 0x40, 0, 0, 0, 0xf);
        //            _gameEngine.StaticVariables.TextToDisplay_80180130.mode = 2;
        //            _gameEngine.StaticVariables.TextToDisplay_80180130.tick = 0;
        //            _gameEngine.StaticVariables.TextToDisplay_80180130.speed = 0xf;
        //
        //            if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
        //            {
        //                _gameEngine.StaticVariables.TextToDisplay_80180130.x = (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X + _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
        //            }
        //            else
        //            {
        //                _gameEngine.StaticVariables.TextToDisplay_80180130.x = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        //            }
        //
        //            if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y < 0)
        //            {
        //                _gameEngine.StaticVariables.TextToDisplay_80180130.y = (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y + _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Height * -8);
        //            }
        //            else
        //            {
        //                _gameEngine.StaticVariables.TextToDisplay_80180130.y = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y;
        //            }
        //            if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
        //            {
        //                _gameEngine.StaticVariables.TextToDisplay_80180130.startX = (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X + _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
        //            }
        //            else
        //            {
        //                _gameEngine.StaticVariables.TextToDisplay_80180130.startX = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
        //            }
        //
        //            _gameEngine.StaticVariables.TextToDisplay_80180130.startY = 0xf0;
        //        }
        //    }
        //    else
        //    {
        //        FUN_80059e0c(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[0]);
        //        FUN_80059e0c(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[1]);
        //        FUN_80059e0c(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[2]);
        //        FUN_80059e0c(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[3]);
        //        UpdateUiBoxesPosition(PTR_UIBoxConfiguration_800c41a0, _gameEngine.StaticVariables.TextToDisplay_800c41a4);
        //        UpdateUiBoxesPosition(PTR_UIBoxConfiguration_800c4214, _gameEngine.StaticVariables.TextToDisplay_800c4218);
        //        UpdateUiBoxesPosition(PTR_UIBoxConfiguration_800c4288, _gameEngine.StaticVariables.TextToDisplay_800c428c);
        //        UpdateUiBoxesPosition(PTR_UIBoxConfiguration_800c42fc, _gameEngine.StaticVariables.TextToDisplay_800c4300);
        //        iVar2 = UpdateUiBoxesPosition(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground, _gameEngine.StaticVariables.TextToDisplay_80180130);
        //        
        //        if (iVar2 != 0)
        //        {
        //            _gameEngine.StaticVariables.UINT_ARRAY_800c4190[2] &= 0xfffffffd;
        //            _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X = _gameEngine.StaticVariables.TextToDisplay_80180130.originX;
        //            _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y = _gameEngine.StaticVariables.TextToDisplay_80180130.originY;
        //            FUN_80047cb0(callbackInfo);
        //            
        //            if (_gameEngine.StaticVariables.g_asyncOperationResult2 == 2)
        //            {
        //                *PTR_80180128 = -2;
        //                return 1;
        //            }
        //
        //            *PTR_80180128 = INT_80180120;
        //            return 1;
        //        }
        //    }
        //}
        //else
        //{
        //    UpdateUiBoxesPosition(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground, _gameEngine.StaticVariables.TextToDisplay_80180130);
        //    FUN_80059e0c(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[0]);
        //    FUN_80059e0c(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[1]);
        //    FUN_80059e0c(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[2]);
        //    FUN_80059e0c(_gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150[3]);
        //    UpdateUiBoxesPosition(PTR_UIBoxConfiguration_800c41a0, _gameEngine.StaticVariables.TextToDisplay_800c41a4);
        //    UpdateUiBoxesPosition(PTR_UIBoxConfiguration_800c4214, _gameEngine.StaticVariables.TextToDisplay_800c4218);
        //    UpdateUiBoxesPosition(PTR_UIBoxConfiguration_800c42fc, _gameEngine.StaticVariables.TextToDisplay_800c4300);
        //    iVar2 = UpdateUiBoxesPosition(PTR_UIBoxConfiguration_800c4288, _gameEngine.StaticVariables.TextToDisplay_800c428c);
        //
        //    if (iVar2 != 0)
        //    {
        //        if ((_gameEngine.StaticVariables.UINT_ARRAY_800c4190[2] & 1) != 0)
        //        {
        //            _gameEngine.StaticVariables.UINT_ARRAY_800c4190[2] &= 0xfffffffe;
        //        }
        //
        //        if ((_gameEngine.StaticVariables.UINT_ARRAY_800c4190[2] & 4) != 0)
        //        {
        //            _gameEngine.StaticVariables.UINT_ARRAY_800c4190[2] &= 0xfffffffb;
        //            (&PTR_800c419c)[UINT_ARRAY_800c4190[0] * 0x1d] = null;
        //        }
        //    }
        //}
        //
        //FUN_80058e6c(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground);
        //
        //if (((uint)PTR_800c419c & 1) != 0)
        //{
        //    FUN_80058c44(_gameEngine.StaticVariables.PTR_800c419c, _gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150);
        //}
        //
        //if ((_gameEngine.StaticVariables.DAT_800c4210 & 1) != 0)
        //{
        //    FUN_80058c44(_gameEngine.StaticVariables.DAT_800c4210, _gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150 + 1);
        //}
        //
        //if ((_gameEngine.StaticVariables.DAT_800c4284 & 1) != 0)
        //{
        //    FUN_80058c44(_gameEngine.StaticVariables.DAT_800c4284, _gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150 + 2);
        //}
        //
        //if ((_gameEngine.StaticVariables.DAT_800c42f8 & 1) != 0)
        //{
        //    FUN_80058c44(_gameEngine.StaticVariables.DAT_800c42f8, _gameEngine.StaticVariables.UIMemoryFileBox_ARRAY_80180150 + 3);
        //}
        //
        ////uVar1 = g_drawModes[0x14].tag;
        //_gameEngine.StaticVariables.SPRT_ARRAY_80180210[0].x0 = (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X + 0x10);
        //_gameEngine.StaticVariables.SPRT_ARRAY_80180210[0].y0 = (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y + 0x10);
        //
        //foreach (var sprite in MemoryFileBlocSprites)
        //{
        //    _gameEngine.Renderer.AddSprite(sprite);
        //}

        //pSVar6 = _gameEngine.StaticVariables.SPRT_ARRAY_80180210[0];
        //uVar4 = pSVar6.tag;
        //puVar8 = &UINT_80146f70 + uVar1 * 10;
        //pSVar6.tag = uVar4 & 0xff000000 | *puVar8 & 0xffffff;
        //*puVar8 = *puVar8 & 0xff000000 | (uint)pSVar6 & 0xffffff;
    }

    //80050ec8
    //open memory card menu
    public void Fun_80050ec8(CallBackInfo callBackInfo)
    {
        SPRT pSVar1;
        ulong uVar2;
        int iVar3;
        uint uVar4;
        uint uVar5;
        uint uVar6;
        SPRT pSVar7;
        SPRT pSVar8;

        if ((_gameEngine.StaticVariables.g_memoryCardMenuState & 3U) == 0)
        {
            if ((_gameEngine.StaticVariables.g_memoryCardMenuState & 8U) != 0)
            {
                _gameEngine.StaticVariables.g_memoryCardMenuState = (_gameEngine.StaticVariables.g_memoryCardMenuState & 0xfffffff7U) | 2;
                _gameEngine.StaticVariables.TextToDisplay_8017e620.mode = 2;
                _gameEngine.StaticVariables.TextToDisplay_8017e620.tick = 0;
                _gameEngine.StaticVariables.TextToDisplay_8017e620.speed = 0xf;

                if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
                {
                    _gameEngine.StaticVariables.TextToDisplay_8017e620.x =
                        (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X +
                                _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
                }
                else
                {
                    _gameEngine.StaticVariables.TextToDisplay_8017e620.x = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
                }
                if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y < 0)
                {
                    _gameEngine.StaticVariables.TextToDisplay_8017e620.y =
                        (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y +
                                _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Height * -8);
                }
                else
                {
                    _gameEngine.StaticVariables.TextToDisplay_8017e620.y = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y;
                }
                if (_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X < 0)
                {
                    _gameEngine.StaticVariables.TextToDisplay_8017e620.startX =
                        (short)(_gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X +
                                _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Width * -8);
                }
                else
                {
                    _gameEngine.StaticVariables.TextToDisplay_8017e620.startX = _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X;
                }

                _gameEngine.StaticVariables.TextToDisplay_8017e620.startY = 0xf0;
                _gameEngine.SoundManager.PlaySoundEffect(5);
            }
        }
        else
        {
            iVar3 = _gameEngine.UIManager.UpdateUiBoxesPosition(callBackInfo.Data, _gameEngine.StaticVariables.TextToDisplay_8017e620);

            if (iVar3 == 1)
            {
                if ((_gameEngine.StaticVariables.g_memoryCardMenuState & 1U) != 0)
                {
                    _gameEngine.StaticVariables.g_memoryCardMenuState &= 0xfffffffe;
                }

                if ((_gameEngine.StaticVariables.g_memoryCardMenuState & 2U) != 0)
                {
                    _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.X = _gameEngine.StaticVariables.TextToDisplay_8017e620.originX;
                    _gameEngine.StaticVariables.g_uiBoxesInventoryDescriptionBackground.Y = _gameEngine.StaticVariables.TextToDisplay_8017e620.originY;
                    _gameEngine.StaticVariables.g_openMemoryCardState = -1;
                    _gameEngine.UIManager.FUN_80047cb0(callBackInfo);
                    return;
                }
            }
        }

        //uVar2 = _gameEngine.StaticVariables.g_drawModes[0x14].tag;
        pSVar7 = _gameEngine.StaticVariables.SPRT_ARRAY_8017e410[0];
        _gameEngine.StaticVariables.SPRT_ARRAY_8017e410[0].w = 0xff;
        _gameEngine.StaticVariables.SPRT_ARRAY_8017e410[0].h = 0x10;
        _gameEngine.StaticVariables.SPRT_ARRAY_8017e410[0].x0 = (short)(callBackInfo.Data.X + callBackInfo.Data.Width);
        _gameEngine.StaticVariables.SPRT_ARRAY_8017e410[0].y0 = (short)(callBackInfo.Data.Y + callBackInfo.Data.Height);

        pSVar8 = _gameEngine.StaticVariables.SPRT_ARRAY_8017e438[0];
        _gameEngine.StaticVariables.SPRT_ARRAY_8017e438[0].w = 0xff;
        _gameEngine.StaticVariables.SPRT_ARRAY_8017e438[0].h = 0x10;
        _gameEngine.StaticVariables.SPRT_ARRAY_8017e438[0].x0 = (short)(callBackInfo.Data.X + callBackInfo.Data.Width);
        _gameEngine.StaticVariables.SPRT_ARRAY_8017e438[0].y0 = (short)(callBackInfo.Data.Y + callBackInfo.Data.Height + 0x10);

        //pSVar1 = _gameEngine.StaticVariables.SPRT_80146f5c[0];
        //uVar5._0_1_ = pSVar1.r0;
        //uVar5._1_1_ = pSVar1.g0;
        //uVar5._2_1_ = pSVar1.b0;
        //uVar5._3_1_ = pSVar1.code;
        //pSVar7.tag = pSVar7.tag & 0xff000000 | uVar5 & 0xffffff;
        //uVar4._0_1_ = pSVar1.r0;
        //uVar4._1_1_ = pSVar1.g0;
        //uVar4._2_1_ = pSVar1.b0;
        //uVar4._3_1_ = pSVar1.code;
        //uVar5 = uVar4 & 0xff000000 | (uint)pSVar7 & 0xffffff;
        //pSVar1.r0 = (char)uVar5;
        //pSVar1.g0 = (char)(uVar5 >> 8);
        //pSVar1.b0 = (char)(uVar5 >> 0x10);
        //pSVar1.code = (char)(uVar5 >> 0x18);
        //pSVar8.tag = pSVar8.tag & 0xff000000 | (uint)pSVar7 & 0xffffff;
        //uVar6._0_1_ = pSVar1.r0;
        //uVar6._1_1_ = pSVar1.g0;
        //uVar6._2_1_ = pSVar1.b0;
        //uVar6._3_1_ = pSVar1.code;
        //uVar5 = uVar6 & 0xff000000 | (uint)pSVar8 & 0xffffff;
        //pSVar1.r0 = (char)uVar5;
        //pSVar1.g0 = (char)(uVar5 >> 8);
        //pSVar1.b0 = (char)(uVar5 >> 0x10);
        //pSVar1.code = (char)(uVar5 >> 0x18);
    }
}