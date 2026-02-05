using System.Diagnostics;
using AlundraEngine.Sound;

namespace AlundraEngine;

public class SoundManager
{
    private readonly GameEngine _gameEngine;

    public SoundManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    //see soundBin

    //800484e8
    public int InitializeSoundSystem()
    {
        int result;

        if (_gameEngine.StaticVariables.g_isCdResetRequested != 0)
        {
            //_gameEngine.CdManager.CdInit();
            //_gameEngine.CdManager.InitCDRom2();
            _gameEngine.StaticVariables.g_cdIsReady = 0;
        }

        _gameEngine.StaticVariables.g_cdIsReady = 1;

        _gameEngine.StaticVariables.g_resetSoundFlag = 0;
        //_gameEngine.StaticVariables.INT_80164fc0 = 0;
        _gameEngine.StaticVariables.g_currentMapSoundIndex = 0;
        _gameEngine.StaticVariables.g_currentSoundGroup = -1; //0xffffffff;
        _gameEngine.StaticVariables.g_soundEffectState = 0;
        _gameEngine.StaticVariables.g_resetSoundFlag = 0;
        _gameEngine.StaticVariables.DAT_80165024 = 0;

        Array.Clear(_gameEngine.StaticVariables.g_voiceState);

        //FUN_8008eeac(-0x7fe8a5b0, 4, 1);
        //FUN_8008e398();
        //SpuSetMute(1);
        //FUN_8008ec8c(4);
        //_gameEngine.StaticVariables.g_vabHeader = -1;
        _gameEngine.StaticVariables.g_currentVabId = -1;
        //FUN_8008f994();

        if (_gameEngine.StaticVariables.g_cdIsReady != 0)
        {
            //g_vabBaseSector = CdPosToInt((CdlLOC*)&PTR_CDFile_Sound_bin);
        }

        _gameEngine.StaticVariables.DAT_8017384c = 4;
        //_gameEngine.StaticVariables.g_spuReverbAttr.mask = 7;
        //_gameEngine.StaticVariables.g_spuReverbAttr.mode = 0x104;
        //_gameEngine.StaticVariables.g_spuReverbAttr.depth.left = 0x2a00;
        //_gameEngine.StaticVariables.g_spuReverbAttr.depth.right = 0x2a00;
        //SpuSetReverbModeParam(&g_spuReverbAttr);
        //SpuSetReverbDepth(&g_spuReverbAttr);
        //SpuSetReverbVoice(1, 0xffffff);
        //SpuSetReverb(1);
        //SpuCommonAttr_80166140.mask = 0x2ec0;
        //SpuCommonAttr_80166140.cd.volume.left = 0x7fff;
        //SpuCommonAttr_80166140.cd.volume.right = 0x7fff;
        //SpuCommonAttr_80166140.cd.mix = 1;
        //SpuCommonAttr_80166140.ext.volume.left = 0x7fff;
        //SpuCommonAttr_80166140.ext.volume.right = 0x7fff;
        //SpuCommonAttr_80166140.ext.mix = 1;
        //SpuSetCommonAttr(&SpuCommonAttr_80166140);
        //FUN_80048704();

        result = 1;

        //if (_gameEngine.StaticVariables.SfxVabHeaderOffset < 0x801)
        //{
        //    //ReadFileFromCDIntoBuffer("data\\sound.bin", (u_long*)&DAT_80173850, 0, DAT_800a7d2c);
        //    //voiceIndex = 9;
        //    //psVar1 = _gameEngine.StaticVariables.SHORT_80175d12;
        //
        //    //do
        //    //{
        //    //    *psVar1 = -1;
        //    //    voiceIndex = voiceIndex + -1;
        //    //    psVar1 = psVar1 + -1;
        //    //} while (-1 < voiceIndex);
        //
        //    //FUN_8008e398();
        //    //SpuSetMute(0);
        //    result = 1;
        //}
        //else
        //{
        //    //DoNothing();
        //    result = 0;
        //}

        return result;
    }

    // 8004b114
    public void FUN_8004b114(int soundIndex, int stopAllSound)
    {
        if (-1 < soundIndex)
        {
            if (soundIndex == 0)
            {
                _gameEngine.StaticVariables.g_currentMapSoundIndex = 0;
                InitializeBgm(_gameEngine.StaticVariables.g_requestedSeqId);
                ResetSomethingSound(_gameEngine.StaticVariables.g_requestedSeqId);
                MaybeFreeSound(_gameEngine.StaticVariables.g_currentVabId);
            }
            else if (_gameEngine.StaticVariables.g_cdIsReady == 0)
            {
                MaybeLoadSound(soundIndex, stopAllSound);

                if (stopAllSound == 0)
                {
                    _gameEngine.StaticVariables.g_resetSoundFlag = 0;
                }
            }
            else
            {
                _gameEngine.CdManager.InitCDReading();
                _gameEngine.StaticVariables.g_soundLoadState = 1;
                _gameEngine.StaticVariables.g_currentMapSoundIndex = (short)soundIndex;
                _gameEngine.StaticVariables.g_forceStopAllSound = stopAllSound;
            }
        }
    }

    //80049be0
    public void MaybeLoadSound(int soundIndex, int stopAllSound)
    {
        int segId;

        _gameEngine.StaticVariables.g_currentMapSoundIndex = (short)soundIndex;
        InitializeBgm(_gameEngine.StaticVariables.g_requestedSeqId);
        ResetSomethingSound(_gameEngine.StaticVariables.g_requestedSeqId);
        MaybeFreeSound(_gameEngine.StaticVariables.g_currentVabId);

        if (_gameEngine.StaticVariables.g_currentMapSoundIndex == -1)
        {
            _gameEngine.StaticVariables.g_currentMapSoundIndex = 1;
        }

        //FUN_8004a184(_gameEngine.StaticVariables.g_currentVabId, _gameEngine.StaticVariables.DAT_8015b1a0, _gameEngine.StaticVariables.g_wind_tx_buffer, 0x39040);

        if (_gameEngine.StaticVariables.g_currentVabId < 0)
        {
            //DoNothing();
        }

        //ReadFileFromCDIntoBuffer("data\\sound.bin", (u_long*)&g_errorMarker2,
        //    (uint)(&g_seqExtraAddrTable)[g_currentMapSoundIndex * 3],
        //    (int)(&g_seqStartAddrTable)[g_currentMapSoundIndex * 3] -
        //    (int)(&g_seqExtraAddrTable)[g_currentMapSoundIndex * 3]);

        segId = -1; //LoadSeq(_gameEngine.StaticVariables.g_errorMarker2, _gameEngine.StaticVariables.g_currentVabId);
        _gameEngine.StaticVariables.g_requestedSeqId = (short)segId;

        if ((int)(segId << 0x10) < 0)
        {
            //DoNothing();
        }

        _gameEngine.StaticVariables.g_currentMapSoundIndex = (short)soundIndex;

        if ((0 < soundIndex) && (stopAllSound != 0))
        {
            StopAllSound();
        }

        _gameEngine.StaticVariables.g_resetSoundFlag = 1;
    }

    //8008f9a4
    private void MaybeFreeSound(short vabId)
    {
        if (vabId < 0x10 && vabId > 0)
        {
            if (_gameEngine.StaticVariables.DAT_sound_801f76b8[vabId] == 1)
            {
                //SpuFree(_gameEngine.StaticVariables.DAT_sound_801f7718[vabId]);
                _gameEngine.StaticVariables.DAT_sound_801f76b8[vabId] = 0;
                _gameEngine.StaticVariables.DAT_sound_801f7710 = (short)(_gameEngine.StaticVariables.DAT_sound_801f7710 - 1);
            }
        }
    }

    //8008df04
    public void ResetSomethingSound(short vabId)
    {
        ResetSomethingSound2(vabId);
    }

    //8008dd8c
    public void ResetSomethingSound2(short vabId)
    {
        //TODO
    }

    // 80049b7c
    public void LoadBgm(int bgmIndex)
    {
        _gameEngine.StaticVariables.g_resetSoundFlag = 0;

        if (bgmIndex == 0)
        {
            InitializeBgm(_gameEngine.StaticVariables.g_requestedSeqId);
        }
        else
        {
            _gameEngine.StaticVariables.g_soundEffectState = 0x78;
        }
    }
    
    // 8008f458
    public void InitializeBgm(short seqId)
    {
        FUN_8008f2e8(seqId, 0);
    }

    // 8008f2e8
    private void FUN_8008f2e8(short seqId, short i)
    {
        //AlundraEngine.Debug.Debugger.Breakpoint();
    }

    //80049af4
    public void StopAllSound()
    {
        if (-1 < _gameEngine.StaticVariables.g_currentMapSoundIndex)
        {
            if (_gameEngine.StaticVariables.g_soundEffectState != 0)
            {
                //SpuSetKey(0, 0xffffff);
                //VSync(0);
                _gameEngine.StaticVariables.g_soundEffectState = 0;
            }

            FUN_8008b878(0x7f, 0x7f);
            //SetSeqVolume(_gameEngine.StaticVariables.g_requestedSeqId, 0x7f, 0x7f);
            //PlaySeq(_gameEngine.StaticVariables.g_requestedSeqId, '\x01', 1);
        }
    }

    //8008b878
    private void FUN_8008b878(int volumeLeft, int volumeRight)
    {
        //SpuCommonAttr local_30;
        //
        //local_30.mask = 3;
        //local_30.mvol.left = param_1 * 0x81;
        //local_30.mvol.right = param_2 * 0x81;
        //SpuSetCommonAttr(&local_30);
    }

    //800490fc or 80049634 ???
    public void PlaySoundEffect(uint sfxId)
    {
        if (_gameEngine.StaticVariables.g_soundEffectState != 0)
        {
            return;
        }

        if (sfxId < 1)
        {
            return;
        }

        if (0x3c2 < sfxId) //962
        {
            return;
        }

        _gameEngine.SoundBin.PlaySoundEffect((int)sfxId);

        //if ((sfxId & 0x100) != 0)
        //{
        //    _gameEngine.SoundBin.PlayMapSfx(((int)sfxId & 0x0FF) - 43, 11025, false, out _, out _, out _);
        //}
        //else
        //{
        //    _gameEngine.SoundBin.PlaySfx((int)sfxId, 11025, false, out _, out _, out _);
        //}
    }

    //8004a09c
    public void LoadMapSounds(uint mapId)
    {
        var iVar1 = _gameEngine.GetSoundOffsetByMapId(mapId);

        if (iVar1 != 0)
        {
            int iVar2 = _gameEngine.StaticVariables.g_currentMapSoundIndex;
            iVar1 = _gameEngine.GetSoundOffsetByMapId(mapId);

            if (iVar2 != iVar1)
            {
                if (_gameEngine.StaticVariables.g_requestedSeqId >= 0)
                {
                    InitializeBgm(_gameEngine.StaticVariables.g_requestedSeqId);
                    ResetSomethingSound(_gameEngine.StaticVariables.g_requestedSeqId);
                }

                iVar1 = _gameEngine.GetSoundOffsetByMapId(mapId);
                if (iVar1 != 0x2d)
                {
                    iVar1 = _gameEngine.GetSoundOffsetByMapId(mapId);
                    MaybeLoadSound((int)iVar1, 0);
                }

                //FUN_8008f808(_gameEngine.StaticVariables.g_requestedSeqId, 0x7f, 10);
            }
        }

        //iVar1 = GetSoundGroupBbyMapId(mapId);
        //
        //if (_gameEngine.StaticVariables.g_currentSoundGroup != iVar1)
        //{
        //    FUN_800489c8(mapId);
        //}
        //
        //FUN_8005ac90();
        Debug.WriteLine("!!!!!!!!!!!!!!!! Implement PlaySoundEffectWithToneVolumeMix");
    }

    //80049794
    public void PlaySoundEffectWithToneVolumeMix(int param_1, int param_2, int param_3)
    {
        Debug.WriteLine("!!!!!!!!!!!!!!!! Implement LoadMapSounds 0x8004a09c");
        //TODO
        /*
        int s4 = param_1;
        int s7 = param_2;
        int s8 = param_3;

        int s0;
        int toneCount;
        int voiceId;
        int s1 = 0;

        var soundEffectData = _gameEngine.StaticVariables.g_soundEffectData;

        if (soundEffectData[s4].Id == -1)
        {
            s0 = s4;
            param_1 = _gameEngine.StaticVariables.g_vabHeader;
            param_2 = soundEffectData[s4].Pitch;
        }
        else
        {
            param_2 = _gameEngine.StaticVariables.g_currentSoundGroup;
            s0 = _gameEngine.FUN_80048a14(s4, param_2);
            param_1 = _gameEngine.StaticVariables.g_altSoundDriver;
            param_2 = soundEffectData[s0].Pitch;
        }

        _gameEngine.FUN_800901a8((short)param_1, (short)param_2, out var localData);

        toneCount = soundEffectData[s0].ToneCount;
        if (toneCount > 0)
        {
            do
            {
                voiceId = _gameEngine.FUN_8004974c(s4, soundEffectData[s0].Pitch + s1);

                if (voiceId != -1)
                {
                    int volumeRight = _gameEngine.StaticVariables.g_voiceVolumeRight[voiceId];
                    int volumeLeft = _gameEngine.StaticVariables.g_voiceVolumeLeft[voiceId];

                    int t1 = (volumeRight < 65) ? volumeRight : (127 - volumeRight);
                    int t0 = (volumeRight < 64) ? volumeRight : 63;

                    int volCalc1 = (s7 + 1) * (s7 + 1);
                    int volCalc2 = (volumeLeft + 1) * (volumeLeft + 1);

                    int combinedVolume = (((volCalc1 - 1) * (volCalc2 - 1)) * 0x800209) >> 13;

                    int volFinal = (((combinedVolume + localData.Field) >> 13) * (localData.OtherField - 1)) * 0x800209 >> 13;
                    int volRightFinal = ((((t1 * t1 * volFinal) >> 13) * (volCalc2 - 1)) * 0x800209) >> 13;
                    int volLeftFinal = ((((t0 * t0 * volFinal) >> 13) * volCalc2) * 0x800209) >> 13;

                    SetVoiceVolume((short)voiceId, (short)(volRightFinal >> 11), (short)(volLeftFinal >> 11));
                }

                s1++;
            }
            while (s1 < toneCount);
        }*/
    }

    //80049f1c
    public int HandleMapSoundEffects(uint mapId, uint soundEffectId)
    {
        InitializeSoundSomething();

        if (SoundBin.SfxRecordsData[soundEffectId][0] == 0xFF &&
            SoundBin.SfxRecordsData[soundEffectId][6] == 0)
        {
            soundEffectId = 0;
        }

        var offset = _gameEngine.GetSoundOffsetByMapId(mapId);

        if (offset == 0)
        {
            offset = _gameEngine.StaticVariables.g_currentMapSoundIndex;
        }

        if (_gameEngine.StaticVariables.g_currentMapSoundIndex == offset)
        {
            if (soundEffectId == 0)
            {
                return 1;
            }
        }
        else
        {
            if (soundEffectId == 0)
            {
                _gameEngine.StaticVariables.g_soundEffectState = 0x78;
                return 1;
            }

            LoadBgm(0);
        }

        PlaySoundEffect(soundEffectId);
        return 1;
    }

    //80048e44
    private void InitializeSoundSomething()
    {
        //undefined1 uVar1;
        //undefined3 extraout_var;
        //undefined** ppuVar2;
        //int iVar3;
        //byte* pbVar4;
        //char acStack_80[104];
        //
        //iVar3 = 0;
        //pbVar4 = g_voiceState;
        //do
        //{
        //    if ((g_voiceState[iVar3] != 0) && (*(int*)(pbVar4 + 0x78) != -2))
        //    {
        //        FUN_80094f20((ushort)iVar3);
        //    }
        //    iVar3 = iVar3 + 1;
        //    pbVar4 = pbVar4 + 4;
        //} while (iVar3 < 0x18);
        //iVar3 = 0;
        //ppuVar2 = &g_soundEffectData;
        //do
        //{
        //    if (((*(short*)((int)ppuVar2 + 10) != -1) && (((uint)ppuVar2[2] & 2) != 0)) &&
        //        (uVar1 = FUN_8008dd1c((int)g_loadedSequenceHandles[*(short*)((int)ppuVar2 + 10)], 0),
        //            (short)CONCAT31(extraout_var, uVar1) == 1))
        //    {
        //        InitializeBgm(g_loadedSequenceHandles[*(short*)((int)ppuVar2 + 10)]);
        //        ResetSomethingSound(g_loadedSequenceHandles[*(short*)((int)ppuVar2 + 10)]);
        //        *(ushort*)(ppuVar2 + 2) = *(ushort*)(ppuVar2 + 2) & 0xfffd;
        //        sprintf(acStack_80, "%d %d\r\n", iVar3,
        //            (int)g_loadedSequenceHandles[*(short*)((int)ppuVar2 + 10)]);
        //        DoNothing();
        //    }
        //    iVar3 = iVar3 + 1;
        //    ppuVar2 = (undefined**)((int)ppuVar2 + 0x16);
        //} while (iVar3 < 0x3c2);
        //FUN_80090168();
    }

    //8004b1d4
    public void HandleMapSoundStreaming()
    {
        _gameEngine.StaticVariables.g_soundLoadState = 0;

        //TODO
    }

    //8004b104
    public bool IsSoundLoading()
    {
        //TODO we don't load sound yet
        //return _gameEngine.StaticVariables.g_soundLoadState != 0;
        return false;
    }
}