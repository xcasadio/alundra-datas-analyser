using System.Diagnostics;

namespace AlundraEngine;

public class SoundManager
{
    private readonly GameEngine _gameEngine;

    public SoundManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    //see soundBin

    // 8004b114
    public void FUN_8004b114(int soundIndex, int stopAllSound)
    {
        if (-1 < soundIndex)
        {
            if (soundIndex == 0)
            {
                StaticVariables.g_currentMapSoundIndex = 0;
                InitializeBgm(StaticVariables.g_requestedSeqId);
                ResetSomethingSound(StaticVariables.g_requestedSeqId);
                MaybeFreeSound(StaticVariables.g_currentVabId);
            }
            else if (StaticVariables.g_cdIsReady == 0)
            {
                MaybeLoadSound(soundIndex, stopAllSound);

                if (stopAllSound == 0)
                {
                    StaticVariables.g_resetSoundFlag = 0;
                }
            }
            else
            {
                _gameEngine.CdManager.InitCDReading();
                StaticVariables.g_soundLoadState = 1;
                StaticVariables.g_currentMapSoundIndex = (short)soundIndex;
                StaticVariables.g_forceStopAllSound = stopAllSound;
            }
        }
    }

    //80049be0
    public void MaybeLoadSound(int soundIndex, int stopAllSound)
    {
        Debugger.Break();
    }

    //8008f9a4
    private void MaybeFreeSound(short vabId)
    {
        Debugger.Break();
    }

    //8008df04
    public void ResetSomethingSound(short vabId)
    {
        Debugger.Break();
    }

    // 80049b7c
    public void LoadBgm(int bgmIndex)
    {
        StaticVariables.g_resetSoundFlag = 0;
        if (bgmIndex == 0)
        {
            InitializeBgm(StaticVariables.g_requestedSeqId);
        }
        else
        {
            StaticVariables.g_soundEffectState = 0x78;
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
        Debugger.Break();
    }

    //80049af4
    public void StopAllSound()
    {
        Debugger.Break();
    }

    //800490fc or 80049634 ???
    public void PlaySoundEffect(uint sfxId)
    {
        if (StaticVariables.g_soundEffectState != 0)
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
    public void LoadMapSounds(int mapId)
    {
        var iVar1 = _gameEngine.GetMapWarpDestination(mapId);

        if (iVar1 != 0)
        {
            int iVar2 = StaticVariables.g_currentMapSoundIndex;
            iVar1 = _gameEngine.GetMapWarpDestination(mapId);

            if (iVar2 != iVar1)
            {
                if (StaticVariables.g_requestedSeqId >= 0)
                {
                    InitializeBgm(StaticVariables.g_requestedSeqId);
                    ResetSomethingSound(StaticVariables.g_requestedSeqId);
                }

                iVar1 = _gameEngine.GetMapWarpDestination(mapId);
                if (iVar1 != 0x2d)
                {
                    iVar1 = _gameEngine.GetMapWarpDestination(mapId);
                    MaybeLoadSound((int)iVar1, 0);
                }

                //FUN_8008f808(StaticVariables.g_requestedSeqId, 0x7f, 10);
            }
        }

        //iVar1 = GetSoundGroupBbyMapId(mapId);
        //
        //if (StaticVariables.g_currentSoundGroup != iVar1)
        //{
        //    FUN_800489c8(mapId);
        //}
        //
        //FUN_8005ac90();
    }

    //80049794
    public void FUN_80049794(int param_1, int param_2, int param_3)
    {
        //TODO
        /*
        int s4 = param_1;
        int s7 = param_2;
        int s8 = param_3;

        int s0;
        int toneCount;
        int voiceId;
        int s1 = 0;

        var soundEffectData = StaticVariables.g_soundEffectData;

        if (soundEffectData[s4].Id == -1)
        {
            s0 = s4;
            param_1 = StaticVariables.g_mainSoundDriver;
            param_2 = soundEffectData[s4].Pitch;
        }
        else
        {
            param_2 = StaticVariables.g_currentSoundGroup;
            s0 = _gameEngine.FUN_80048a14(s4, param_2);
            param_1 = StaticVariables.g_altSoundDriver;
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
                    int volumeRight = StaticVariables.g_voiceVolumeRight[voiceId];
                    int volumeLeft = StaticVariables.g_voiceVolumeLeft[voiceId];

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
}