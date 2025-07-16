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

        if ((sfxId & 0x100) != 0)
        {
            _gameEngine.SoundBin.PlayMapSfx(((int)sfxId & 0x0FF) - 43, 11025, false, out _, out _, out _);
        }
        else
        {
            _gameEngine.SoundBin.PlaySfx((int)sfxId, 11025, false, out _, out _, out _);
        }
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
}