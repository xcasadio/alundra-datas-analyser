using System.Diagnostics;
using System.Drawing.Imaging.Effects;

namespace AlundraEngine.Sound;

public class SoundManager
{
    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: on the original, FUN_8008A718 @ 0x8008A718 runs from the 60 Hz VSync/timer
    // interrupt and keeps the music ticking while the main loop blocks on CD reads
    // (g_voiceCommandLock @ 0x801F6CD0 is its interrupt-vs-mainline guard). On desktop the
    // tick runs on a dedicated audio-driver thread, so this gate provides the mutual
    // exclusion the PSX got from interrupt masking: the tick thread and every game-thread
    // sound entry point serialize on it.
    private readonly object _soundTickGate = new();

    // RELATION: set when a desktop audio-driver thread delivers the 60 Hz sound tick;
    // the main-loop and warp-wait call sites then stop ticking by themselves.
    public bool HasExternalSoundTickDriver { get; set; }

    private readonly GameEngine _gameEngine;
    private readonly SoundBin.VabHeader?[] _loadedVabHeaders = new SoundBin.VabHeader[16];
    private readonly byte[]?[] _loadedVabBodies = new byte[16][];
    private readonly int[] _loadedVabBodyWriteOffsets = new int[16];
    private readonly byte[]?[] _loadedSequenceData = new byte[0x20][];
    private readonly byte[,] _sequenceChannelPrograms = new byte[0x20, 16];
    private readonly ushort[,] _sequenceChannelPitchBends = new ushort[0x20, 16];
    private static readonly ushort[] s_voicePitchTable =
    {
        0x1000, 0x100E, 0x101D, 0x102C, 0x103B, 0x104A, 0x1059, 0x1068, 0x1078, 0x1087, 0x1096, 0x10A5,
        0x10B5, 0x10C4, 0x10D4, 0x10E3, 0x10F3, 0x1103, 0x1113, 0x1122, 0x1132, 0x1142, 0x1152, 0x1162,
        0x1172, 0x1182, 0x1193, 0x11A3, 0x11B3, 0x11C4, 0x11D4, 0x11E5, 0x11F5, 0x1206, 0x1216, 0x1227,
        0x1238, 0x1249, 0x125A, 0x126B, 0x127C, 0x128D, 0x129E, 0x12AF, 0x12C1, 0x12D2, 0x12E3, 0x12F5,
        0x1306, 0x1318, 0x132A, 0x133C, 0x134D, 0x135F, 0x1371, 0x1383, 0x1395, 0x13A7, 0x13BA, 0x13CC,
        0x13DE, 0x13F1, 0x1403, 0x1416, 0x1428, 0x143B, 0x144E, 0x1460, 0x1473, 0x1486, 0x1499, 0x14AC,
        0x14BF, 0x14D3, 0x14E6, 0x14F9, 0x150D, 0x1520, 0x1534, 0x1547, 0x155B, 0x156F, 0x1583, 0x1597,
        0x15AB, 0x15BF, 0x15D3, 0x15E7, 0x15FB, 0x1610, 0x1624, 0x1638, 0x164D, 0x1662, 0x1676, 0x168B,
        0x16A0, 0x16B5, 0x16CA, 0x16DF, 0x16F4, 0x170A, 0x171F, 0x1734, 0x174A, 0x175F, 0x1775, 0x178B,
        0x17A1, 0x17B6, 0x17CC, 0x17E2, 0x17F9, 0x180F, 0x1825, 0x183B, 0x1852, 0x1868, 0x187F, 0x1896,
        0x18AC, 0x18C3, 0x18DA, 0x18F1, 0x1908, 0x191F, 0x1937, 0x194E, 0x1965, 0x197D, 0x1995, 0x19AC,
        0x19C4, 0x19DC, 0x19F4, 0x1A0C, 0x1A24, 0x1A3C, 0x1A55, 0x1A6D, 0x1A85, 0x1A9E, 0x1AB7, 0x1ACF,
        0x1AE8, 0x1B01, 0x1B1A, 0x1B33, 0x1B4C, 0x1B66, 0x1B7F, 0x1B98, 0x1BB2, 0x1BCC, 0x1BE5, 0x1BFF,
        0x1C19, 0x1C33, 0x1C4D, 0x1C67, 0x1C82, 0x1C9C, 0x1CB7, 0x1CD1, 0x1CEC, 0x1D07, 0x1D22, 0x1D3D,
        0x1D58, 0x1D73, 0x1D8E, 0x1DA9, 0x1DC5, 0x1DE0, 0x1DFC, 0x1E18, 0x1E34, 0x1E50, 0x1E6C, 0x1E88,
        0x1EA4, 0x1EC1, 0x1EDD, 0x1EFA, 0x1F16, 0x1F33, 0x1F50, 0x1F6D, 0x1F8A, 0x1FA7, 0x1FC5, 0x1FE2,
    };

    public SoundManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    private void EnsureSoundEffectDataInitialized()
    {
        var sequenceOffsets = SoundBin.SeqOffsets;
        if (_gameEngine.StaticVariables.g_soundEffectSeqOffsets.Length != sequenceOffsets.Length)
        {
            _gameEngine.StaticVariables.g_soundEffectSeqOffsets = new int[sequenceOffsets.Length];
        }

        Array.Copy(sequenceOffsets, _gameEngine.StaticVariables.g_soundEffectSeqOffsets, sequenceOffsets.Length);

        var sourceRecords = _gameEngine.SoundBin.SfxRecords;
        if (_gameEngine.StaticVariables.g_soundEffectData.Length == sourceRecords.Length)
        {
            return;
        }

        _gameEngine.StaticVariables.g_soundEffectData = new SoundEffectRecord[sourceRecords.Length];
        for (var sfxId = 0; sfxId < sourceRecords.Length; sfxId++)
        {
            ref var destination = ref _gameEngine.StaticVariables.g_soundEffectData[sfxId];
            var source = sourceRecords[sfxId];
            destination.VabId = source.VabId;
            destination.ProgramNumber = source.ProgramNumber;
            destination.ToneNumber = source.ToneNumber;
            destination.Note = source.Note;
            destination.Flags = source.Flags;
            destination.SeqNum = source.SeqNum;
            destination.RefSfxId = source.RefSfxId;
            destination.field_0x0E = source.Unknown1;
            destination.MaxVoices = source.MaxVoices;
            destination.field_0x12 = source.Unknown2;
            destination.ToneCount = source.NumTones;
        }
    }

    private short LoadVabFromSoundBinRange(int headerOffset, int bodyOffset, int nextOffset, short requestedVabId)
    {
        var vabHeaderData = _gameEngine.SoundBin.ReadRange(headerOffset, bodyOffset - headerOffset);
        Array.Clear(_gameEngine.StaticVariables.DAT_8015b1a0);
        Array.Copy(vabHeaderData, _gameEngine.StaticVariables.DAT_8015b1a0, Math.Min(vabHeaderData.Length, _gameEngine.StaticVariables.DAT_8015b1a0.Length));

        var vabId = LoadVabHeader(_gameEngine.StaticVariables.DAT_8015b1a0, requestedVabId, 0x39040);
        if (vabId < 0)
        {
            return -1;
        }

        var vabBodyOffset = bodyOffset;
        var vabBodyRemainingSize = nextOffset - bodyOffset;
        if (vabBodyRemainingSize <= 0)
        {
            FreeLoadedVab(vabId);
            return -1;
        }

        _loadedVabBodies[vabId] = new byte[vabBodyRemainingSize];
        _loadedVabBodyWriteOffsets[vabId] = 0;
        _gameEngine.StaticVariables.g_loadedVabBodySizes[vabId] = vabBodyRemainingSize;

        while (vabBodyRemainingSize > 0)
        {
            var chunkSize = Math.Min(0x8000, vabBodyRemainingSize);
            var vabBodyChunk = _gameEngine.SoundBin.ReadRange(vabBodyOffset, chunkSize);
            Array.Clear(_gameEngine.StaticVariables.g_partialVabBodyBuffer);
            Array.Copy(vabBodyChunk, _gameEngine.StaticVariables.g_partialVabBodyBuffer, vabBodyChunk.Length);
            if (UploadVabBodyChunk(_gameEngine.StaticVariables.g_partialVabBodyBuffer, chunkSize, vabId) < 0)
            {
                FreeLoadedVab(vabId);
                return -1;
            }

            vabBodyOffset += chunkSize;
            vabBodyRemainingSize -= chunkSize;
        }

        if (_loadedVabBodies[vabId] == null
            || _loadedVabBodyWriteOffsets[vabId] != _gameEngine.StaticVariables.g_loadedVabBodySizes[vabId])
        {
            FreeLoadedVab(vabId);
            return -1;
        }

        return vabId;
    }

    private void LoadGlobalSoundVab()
    {
        _gameEngine.StaticVariables.g_globalSoundVabId = LoadVabFromSoundBinRange(
            SoundBin.SfxVabHeaderOffset,
            SoundBin.SfxVabBodyOffset,
            SoundBin.MapVabOffsets[0],
            _gameEngine.StaticVariables.g_globalSoundVabId);
    }

    private void LoadMapSoundVab(int soundGroup)
    {
        var offsetIndex = soundGroup * 2;
        if ((uint)(offsetIndex + 2) >= (uint)SoundBin.MapVabOffsets.Length)
        {
            _gameEngine.StaticVariables.g_mapSoundVabId = -1;
            return;
        }

        _gameEngine.StaticVariables.g_mapSoundVabId = LoadVabFromSoundBinRange(
            SoundBin.MapVabOffsets[offsetIndex],
            SoundBin.MapVabOffsets[offsetIndex + 1],
            SoundBin.MapVabOffsets[offsetIndex + 2],
            _gameEngine.StaticVariables.g_mapSoundVabId);
    }

    //see soundBin

    // GHIDRA: InitializeSoundSystem @ 0x800484E8
    public int InitializeSoundSystem()
    {
        lock (_soundTickGate)
        {
            return InitializeSoundSystemCore();
        }
    }

    private int InitializeSoundSystemCore()
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
        _gameEngine.StaticVariables.g_globalSoundVabId = -1;
        _gameEngine.StaticVariables.g_mapSoundVabId = -1;
        _gameEngine.StaticVariables.g_numberOfVoices = (byte)Math.Min(_gameEngine.StaticVariables.g_voiceRuntimeSlots.Length, 0x18);

        EnsureSoundEffectDataInitialized();
        Array.Clear(_gameEngine.StaticVariables.g_voiceState);
        Array.Clear(_gameEngine.StaticVariables.g_voiceSfxId);
        Array.Clear(_gameEngine.StaticVariables.DAT_801f6d68);
        var soundEffectSequenceBuffer = _gameEngine.SoundBin.GetSoundEffectSequenceBuffer();
        if (_gameEngine.StaticVariables.g_soundBinSequenceBuffer.Length != soundEffectSequenceBuffer.Length)
        {
            _gameEngine.StaticVariables.g_soundBinSequenceBuffer = new byte[soundEffectSequenceBuffer.Length];
        }

        Array.Clear(_gameEngine.StaticVariables.g_soundBinSequenceBuffer);
        Array.Copy(soundEffectSequenceBuffer, _gameEngine.StaticVariables.g_soundBinSequenceBuffer, soundEffectSequenceBuffer.Length);

        for (var sequenceIndex = 0; sequenceIndex < _gameEngine.StaticVariables.g_loadedSequenceHandles.Length; sequenceIndex++)
        {
            _gameEngine.StaticVariables.g_loadedSequenceHandles[sequenceIndex] = -1;
        }

        FUN_8008eeac(unchecked((int)0x80175A50), 4, 1);
        //FUN_8008e398();
        //SpuSetMute(1);
        FUN_8008ec8c(4);
        FUN_8008df4c();
        //_gameEngine.StaticVariables.g_globalSoundVabId = -1;
        _gameEngine.StaticVariables.g_currentVabId = -1;
        //FUN_8008f994();

        if (_gameEngine.StaticVariables.g_cdIsReady != 0)
        {
            //g_vabBaseSector = CdPosToInt((CdlLOC*)&PTR_CDFile_Sound_bin);
        }

        _gameEngine.StaticVariables.DAT_8017384c = 4;
        LoadGlobalSoundVab();
        _gameEngine.StaticVariables.g_spuReverbAttr2.Mask = 7;
        _gameEngine.StaticVariables.g_spuReverbAttr2.Mode = 0x104;
        _gameEngine.StaticVariables.g_spuReverbAttr2.DepthLeft = 0x2a00;
        _gameEngine.StaticVariables.g_spuReverbAttr2.DepthRight = 0x2a00;
        _gameEngine.SoundBin.UpdateReverbAttr(_gameEngine.StaticVariables.g_spuReverbAttr2);
        _gameEngine.SoundBin.SetReverbEnabled(true);
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
        lock (_soundTickGate)
        {
            FUN_8004b114Core(soundIndex, stopAllSound);
        }
    }

    private void FUN_8004b114Core(int soundIndex, int stopAllSound)
    {
        if (!_gameEngine.StaticVariables.IsBgmActivated && soundIndex > 0)
        {
            _gameEngine.StaticVariables.g_soundLoadState = 0;
            _gameEngine.StaticVariables.g_soundEffectState = 0;
            _gameEngine.StaticVariables.g_currentMapSoundIndex = 0;
            InitializeBgm(_gameEngine.StaticVariables.g_requestedSeqId);
            ResetSomethingSound(_gameEngine.StaticVariables.g_requestedSeqId);
            FreeLoadedVab(_gameEngine.StaticVariables.g_currentVabId);
            return;
        }

        if (-1 < soundIndex)
        {
            if (soundIndex == 0)
            {
                _gameEngine.StaticVariables.g_currentMapSoundIndex = 0;
                InitializeBgm(_gameEngine.StaticVariables.g_requestedSeqId);
                ResetSomethingSound(_gameEngine.StaticVariables.g_requestedSeqId);
                FreeLoadedVab(_gameEngine.StaticVariables.g_currentVabId);
            }
            else if (_gameEngine.StaticVariables.g_cdIsReady == 0)
            {
                LoadMapSequence(soundIndex, stopAllSound);

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

    // GHIDRA: LoadMapSequenceVab @ 0x8004A184
    private void LoadMapSequenceVab()
    {
        var offsetIndex = _gameEngine.StaticVariables.g_currentMapSoundIndex * 3;
        var vabHeaderOffset = _gameEngine.SoundBin.MusicSeqVabOffsets[offsetIndex + 1];
        var vabBodyOffset = _gameEngine.SoundBin.MusicSeqVabOffsets[offsetIndex + 2];
        var nextSequenceOffset = _gameEngine.SoundBin.MusicSeqVabOffsets[offsetIndex + 3];

        var vabHeaderData = _gameEngine.SoundBin.ReadRange(vabHeaderOffset, vabBodyOffset - vabHeaderOffset);
        Array.Clear(_gameEngine.StaticVariables.DAT_8015b1a0);
        Array.Copy(vabHeaderData, _gameEngine.StaticVariables.DAT_8015b1a0, Math.Min(vabHeaderData.Length, _gameEngine.StaticVariables.DAT_8015b1a0.Length));

        _gameEngine.StaticVariables.g_currentVabId = LoadVabHeader(_gameEngine.StaticVariables.DAT_8015b1a0, _gameEngine.StaticVariables.g_currentVabId, 0x39040);

        var vabBodySize = nextSequenceOffset - vabBodyOffset;
        if (_gameEngine.StaticVariables.g_currentVabId < 0 || _gameEngine.StaticVariables.g_currentVabId >= 0x10 || vabBodySize <= 0)
        {
            return;
        }

        if ((uint)_gameEngine.StaticVariables.g_currentVabId < 0x10)
        {
            _loadedVabBodies[_gameEngine.StaticVariables.g_currentVabId] = new byte[vabBodySize];
            _loadedVabBodyWriteOffsets[_gameEngine.StaticVariables.g_currentVabId] = 0;
            _gameEngine.StaticVariables.g_loadedVabBodySizes[_gameEngine.StaticVariables.g_currentVabId] = vabBodySize;
        }

        _gameEngine.StaticVariables.g_vabBodyOffset = vabBodyOffset;
        _gameEngine.StaticVariables.g_vabBodyRemainingSize = vabBodySize;

        while (_gameEngine.StaticVariables.g_vabBodyRemainingSize > 0)
        {
            var chunkSize = Math.Min(0x8000, _gameEngine.StaticVariables.g_vabBodyRemainingSize);
            var vabBodyChunk = _gameEngine.SoundBin.ReadRange(_gameEngine.StaticVariables.g_vabBodyOffset, chunkSize);
            Array.Clear(_gameEngine.StaticVariables.g_partialVabBodyBuffer);
            Array.Copy(vabBodyChunk, _gameEngine.StaticVariables.g_partialVabBodyBuffer, vabBodyChunk.Length);
            UploadVabBodyChunk(_gameEngine.StaticVariables.g_partialVabBodyBuffer, chunkSize, _gameEngine.StaticVariables.g_currentVabId);
            _gameEngine.StaticVariables.g_vabBodyOffset += chunkSize;
            _gameEngine.StaticVariables.g_vabBodyRemainingSize -= chunkSize;
        }
    }

    // GHIDRA: LoadMapSequence @ 0x80049BE0
    public void LoadMapSequence(int soundIndex, int stopAllSound)
    {
        lock (_soundTickGate)
        {
            LoadMapSequenceCore(soundIndex, stopAllSound);
        }
    }

    private void LoadMapSequenceCore(int soundIndex, int stopAllSound)
    {
        int segId;

        _gameEngine.StaticVariables.g_currentMapSoundIndex = (short)soundIndex;
        InitializeBgm(_gameEngine.StaticVariables.g_requestedSeqId);
        ResetSomethingSound(_gameEngine.StaticVariables.g_requestedSeqId);
        FreeLoadedVab(_gameEngine.StaticVariables.g_currentVabId);

        if (_gameEngine.StaticVariables.g_currentMapSoundIndex == -1)
        {
            _gameEngine.StaticVariables.g_currentMapSoundIndex = 1;
        }

        LoadMapSequenceVab();

        if (_gameEngine.StaticVariables.g_currentVabId < 0)
        {
            //DoNothing();
        }

        var sequenceOffsetIndex = _gameEngine.StaticVariables.g_currentMapSoundIndex * 3;
        var sequenceOffset = _gameEngine.SoundBin.MusicSeqVabOffsets[sequenceOffsetIndex];
        var sequenceSize = _gameEngine.SoundBin.MusicSeqVabOffsets[sequenceOffsetIndex + 1] - sequenceOffset;
        var sequenceData = _gameEngine.SoundBin.ReadRange(sequenceOffset, sequenceSize);

        if (sequenceData.Length > _gameEngine.StaticVariables.g_soundBinSequenceBuffer.Length)
        {
            _gameEngine.StaticVariables.g_soundBinSequenceBuffer = new byte[sequenceData.Length];
        }

        Array.Clear(_gameEngine.StaticVariables.g_soundBinSequenceBuffer);
        Array.Copy(sequenceData, _gameEngine.StaticVariables.g_soundBinSequenceBuffer, sequenceData.Length);

        segId = LoadSeq(_gameEngine.StaticVariables.g_soundBinSequenceBuffer, _gameEngine.StaticVariables.g_currentVabId);
        _gameEngine.StaticVariables.g_requestedSeqId = (short)segId;

        if ((int)(segId << 0x10) < 0)
        {
            //DoNothing();
        }
        else
        {
            SetSeqVolume(_gameEngine.StaticVariables.g_requestedSeqId, 0x7F, 0x7F);

            if ((0 < soundIndex) && (stopAllSound != 0))
            {
                StopAllSound();
            }
            else if (_gameEngine.StaticVariables.IsBgmActivated)
            {
                PlaySeq(_gameEngine.StaticVariables.g_requestedSeqId, 1, 1);
            }
        }

        _gameEngine.StaticVariables.g_currentMapSoundIndex = (short)soundIndex;

        _gameEngine.StaticVariables.g_resetSoundFlag = 1;
    }

    // GHIDRA: FreeLoadedVab @ 0x8008F9A4
    private void FreeLoadedVab(short vabId)
    {
        if ((uint)vabId < 0x10)
        {
            if (_gameEngine.StaticVariables.g_loadedVabState[vabId] != 0)
            {
                //SpuFree(_gameEngine.StaticVariables.g_loadedVabSpuAllocationPointers[vabId]);
                _gameEngine.StaticVariables.g_loadedVabState[vabId] = 0;
                _loadedVabHeaders[vabId] = null;
                _loadedVabBodies[vabId] = null;
                _loadedVabBodyWriteOffsets[vabId] = 0;
                _gameEngine.StaticVariables.g_loadedVabBodySizes[vabId] = 0;
                _gameEngine.StaticVariables.g_loadedVabCount = (short)(_gameEngine.StaticVariables.g_loadedVabCount - 1);
            }
        }
    }

    // GHIDRA: ResetSomethingSound @ 0x8008DF04
    public void ResetSomethingSound(short vabId)
    {
        ResetSomethingSound2(vabId);
    }

    // GHIDRA: ResetSomethingSound2 @ 0x8008DD8C
    public void ResetSomethingSound2(short vabId)
    {
        FUN_80093c78(vabId, 0, 0, 1);
        FUN_80093ef4(vabId);
        _gameEngine.StaticVariables.g_sequenceSlotMask &= ~(1 << vabId);

        if (_gameEngine.StaticVariables.g_sequenceTrackCount <= 0 || (uint)vabId >= _gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        var trackCount = Math.Min((int)_gameEngine.StaticVariables.g_sequenceTrackCount, 1);
        for (var trackIndex = 0; trackIndex < trackCount; trackIndex++)
        {
            ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[vabId];
            sequenceState.Flags = 0;
            sequenceState.field_0x3C = 0xFF;
            sequenceState.field_0x00 = 0;
            sequenceState.field_0x3E = 0;
            sequenceState.field_0x40 = 0;
            sequenceState.field_0x94 = 0;
            var programNumber = GetSequenceChannelMapping(ref sequenceState, sequenceState.CurrentChannel);
            sequenceState.field_0xA4 = 0;
            sequenceState.field_0xA0 = 0;
            sequenceState.field_0x9C = 0;
            sequenceState.field_0x44 = 0;
            sequenceState.field_0x74 = 0x7F;
            sequenceState.field_0x76 = 0x7F;
        }
    }

    // 80049b7c
    public void LoadBgm(int bgmIndex)
    {
        lock (_soundTickGate)
        {
            LoadBgmCore(bgmIndex);
        }
    }

    private void LoadBgmCore(int bgmIndex)
    {
        _gameEngine.StaticVariables.g_resetSoundFlag = 0;

        if (!_gameEngine.StaticVariables.IsBgmActivated && bgmIndex != 0)
        {
            _gameEngine.StaticVariables.g_soundEffectState = 0;
            InitializeBgm(_gameEngine.StaticVariables.g_requestedSeqId);
            ResetSomethingSound(_gameEngine.StaticVariables.g_requestedSeqId);
            return;
        }

        if (bgmIndex == 0)
        {
            InitializeBgm(_gameEngine.StaticVariables.g_requestedSeqId);
        }
        else
        {
            _gameEngine.StaticVariables.g_soundEffectState = 0x78;
        }
    }
    
    // GHIDRA: InitializeBgm @ 0x8008F458
    public void InitializeBgm(short seqId)
    {
        FUN_8008f2e8(seqId, 0);
    }

    // GHIDRA: FUN_8008F2E8 @ 0x8008F2E8
    private void FUN_8008f2e8(short seqId, short i)
    {
        if ((uint)seqId >= _gameEngine.StaticVariables.g_sequenceStatePointers.Length || i != 0)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];

        sequenceState.Flags &= ~1u;
        sequenceState.Flags &= ~2u;
        sequenceState.Flags &= ~8u;
        sequenceState.Flags |= 4;

        sequenceState.field_0x2B = 0;
        sequenceState.Playtime = 0;
        sequenceState.field_0x27 = 0;
        sequenceState.field_0x13 = 0;
        sequenceState.field_0x14 = 0;
        sequenceState.field_0x29 = 0;
        sequenceState.field_0x16 = 0;
        sequenceState.field_0x2A = 0;
        sequenceState.CurrentChannel = 0;
        sequenceState.TimesPlayed = 0;
        sequenceState.field_0x27 = 0;
        sequenceState.Loops = 0;
        sequenceState.field_0x10 = 0;
        sequenceState.MessageType = 0;

        sequenceState.Delay = sequenceState.field_0x7C;
        sequenceState.field_0x8C = sequenceState.field_0x84;
        sequenceState.CurrentTempo = (short)sequenceState.field_0x72;
        sequenceState.SeqPosition = sequenceState.SeqStartPos;
        sequenceState.SeqLoopPos = sequenceState.SeqStartPos;

        sequenceState.Channel0 = 0;
        sequenceState.Channel1 = 1;
        sequenceState.Channel2 = 2;
        sequenceState.Channel3 = 3;
        sequenceState.Channel4 = 4;
        sequenceState.Channel5 = 5;
        sequenceState.Channel6 = 6;
        sequenceState.Channel7 = 7;
        sequenceState.Channel8 = 8;
        sequenceState.Channel9 = 9;
        sequenceState.Channel10 = 10;
        sequenceState.Channel11 = 11;
        sequenceState.Channel12 = 12;
        sequenceState.Channel13 = 13;
        sequenceState.Channel14 = 14;
        sequenceState.Channel15 = 15;
        sequenceState.Orientation0 = 0x40;
        sequenceState.Orientation1 = 0x40;
        sequenceState.Orientation2 = 0x40;
        sequenceState.Orientation3 = 0x40;
        sequenceState.Orientation4 = 0x40;
        sequenceState.Orientation5 = 0x40;
        sequenceState.Orientation6 = 0x40;
        sequenceState.Orientation7 = 0x40;
        sequenceState.Orientation8 = 0x40;
        sequenceState.Orientation9 = 0x40;
        sequenceState.Orientation10 = 0x40;
        sequenceState.Orientation11 = 0x40;
        sequenceState.Orientation12 = 0x40;
        sequenceState.Orientation13 = 0x40;
        sequenceState.Orientation14 = 0x40;
        sequenceState.Orientation15 = 0x40;
        sequenceState.Volume0 = 0x7F;
        sequenceState.Volume1 = 0x7F;
        sequenceState.Volume2 = 0x7F;
        sequenceState.Volume3 = 0x7F;
        sequenceState.Volume4 = 0x7F;
        sequenceState.Volume5 = 0x7F;
        sequenceState.Volume6 = 0x7F;
        sequenceState.Volume7 = 0x7F;
        sequenceState.Volume8 = 0x7F;
        sequenceState.Volume9 = 0x7F;
        sequenceState.Volume10 = 0x7F;
        sequenceState.Volume11 = 0x7F;
        sequenceState.Volume12 = 0x7F;
        sequenceState.Volume13 = 0x7F;
        sequenceState.Volume14 = 0x7F;
        sequenceState.Volume15 = 0x7F;

        for (var channelIndex = 0; channelIndex < 16; channelIndex++)
        {
            _sequenceChannelPrograms[seqId, channelIndex] = (byte)channelIndex;
            _sequenceChannelPitchBends[seqId, channelIndex] = 0x2000;
        }

        sequenceState.field_0x78 = 0x7F;
        sequenceState.field_0x7A = 0x7F;
        FUN_80093ef4(ComposeSequenceKey(seqId, i));
    }

    // GHIDRA: FUN_8008EEAC @ 0x8008EEAC
    private void FUN_8008eeac(int param_1, short param_2, short param_3)
    {
        // PARTIAL: original param_1 is a raw RAM base pointer; C# uses g_sequenceStatePointers as the materialized contiguous block.
        _gameEngine.StaticVariables.g_sequenceSlotCount = param_2;
        _gameEngine.StaticVariables.g_sequenceTrackCount = param_3;

        var slotCount = Math.Min(param_2, _gameEngine.StaticVariables.g_sequenceStatePointers.Length);
        for (var sequenceSlot = 0; sequenceSlot < slotCount; sequenceSlot++)
        {
            var trackCount = Math.Min((int)param_3, 1);
            for (var trackIndex = 0; trackIndex < trackCount; trackIndex++)
            {
                ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[sequenceSlot];
                sequenceState.Flags = 0;
                sequenceState.field_0x3C = 0xFF;
                sequenceState.field_0x00 = 0;
                sequenceState.field_0x3E = 0;
                sequenceState.field_0x40 = 0;
                sequenceState.field_0x94 = 0;
                sequenceState.field_0x98 = 0;
                sequenceState.field_0x42 = 0;
                sequenceState.field_0xA4 = 0;
                sequenceState.field_0xA0 = 0;
                sequenceState.field_0x9C = 0;
                sequenceState.field_0x44 = 0;
                sequenceState.field_0x74 = 0x7F;
                sequenceState.field_0x76 = 0x7F;
                sequenceState.field_0x78 = 0x7F;
                sequenceState.field_0x7A = 0x7F;
            }
        }
    }

    // GHIDRA: FUN_8008EC8C @ 0x8008EC8C
    private void FUN_8008ec8c(int param_1)
    {
        // PARTIAL: InitializeSoundSystem only calls this as FUN_8008ec8c(4).
        // The original also queries another low-level mode helper to pick 50Hz vs 60Hz.
        // The desktop runtime currently ticks at 60Hz, so keep the proven raw globals and use the 60Hz branch for this call-site.
        _gameEngine.StaticVariables.DAT_800c9778 = (param_1 & 0x1000) != 0 ? 1 : 0;
        _gameEngine.StaticVariables.DAT_800c9774 = param_1 & 0x0FFF;

        if (_gameEngine.StaticVariables.DAT_800c9774 == 4)
        {
            _gameEngine.StaticVariables.DAT_801f6ce0 = 0x3C;
            return;
        }

        // Keep the last non-zero driver rate rather than regressing to the uninitialized zero path.
        if (_gameEngine.StaticVariables.DAT_801f6ce0 == 0)
        {
            _gameEngine.StaticVariables.DAT_801f6ce0 = 0x3C;
        }
    }

    // GHIDRA: FUN_8008DF4C @ 0x8008DF4C
    private void FUN_8008df4c()
    {
        FUN_8009299c(0x18);
        Array.Clear(_gameEngine.StaticVariables.DAT_801f6d68);
        _gameEngine.StaticVariables.DAT_801f6ce0 = 0x3C;
        _gameEngine.StaticVariables.g_sequenceSlotMask = 0;
        _gameEngine.StaticVariables.g_voiceCommandLock = 0;
    }

    // GHIDRA: FUN_8009299C @ 0x8009299C
    // PARTIAL: PSX transfer allocator reset remains backend-only; the proven runtime/global reset is transliterated here.
    private void FUN_8009299c(int numberOfVoices)
    {
        var staticVariables = _gameEngine.StaticVariables;
        var voiceResetCount = Math.Min(staticVariables.g_voiceRuntimeSlots.Length, 0x18);

        staticVariables.g_audioFadeState = 0;
        staticVariables.DAT_sound_801f7610 = 0;

        Array.Clear(staticVariables.g_spuVoiceVolumeLeft);
        Array.Clear(staticVariables.g_spuVoiceVolumeRight);
        Array.Clear(staticVariables.g_spuVoicePitch);
        Array.Clear(staticVariables.g_spuVoiceReverb);
        Array.Clear(staticVariables.g_spuVoiceAdsr1);
        Array.Clear(staticVariables.g_spuVoiceAdsr2);
        Array.Clear(staticVariables.g_spuVoiceDirtyFlags);

        staticVariables.g_loadedVabCount = 0;
        Array.Clear(staticVariables.g_loadedVabState);

        staticVariables.g_numberOfVoices = (byte)Math.Min(numberOfVoices, voiceResetCount);
        staticVariables.g_voiceCommandPlayingLeft = 0;
        staticVariables.g_voiceCommandPlayingRight = 0;
        staticVariables.g_voiceCommandPendingLeft = 0;
        staticVariables.g_voiceCommandPendingRight = 0;
        staticVariables.DAT_sound_801f7ef8 = 0;
        staticVariables.DAT_sound_801f7f00 = 0;

        for (var voiceId = 0; voiceId < voiceResetCount; voiceId++)
        {
            staticVariables.g_voiceRuntimeSlots[voiceId] = new VoiceRuntimeSlot();
            ref var voiceSlot = ref staticVariables.g_voiceRuntimeSlots[voiceId];
            voiceSlot.field_0x00 = 0x00FF;
            voiceSlot.ReplacementAge = 0x18;
            voiceSlot.field_0x0A = 0x40;
            voiceSlot.SequenceKey = -1;
            voiceSlot.ToneIndex = unchecked((short)0x00FF);

            staticVariables.g_spuVoiceAdsr1[voiceId] = 0x1000;
            staticVariables.g_spuVoiceAdsr2[voiceId] = 0x0200;

            staticVariables.DAT_maybeCurrentVoiceIndex_801f76b2 = (short)voiceId;
            FUN_80091134(voiceId);
        }

        staticVariables.g_spuReverbAttr2.Mask = 0;
        staticVariables.g_spuReverbAttr2.DepthLeft = 0x3FFF;
        staticVariables.g_spuReverbAttr2.DepthRight = 0x3FFF;
        staticVariables.g_spuReverbAttr2.Mode = 0;
        staticVariables.g_voiceLockFlag = 0;
        staticVariables.DAT_sound_801f7658 = 0;
        staticVariables.DAT_sound_801f7660 = 0x80;

        UpdateSoundVoicesState();
    }

    // GHIDRA: UpdateSoundVoicesState @ 0x8009311C
    // PARTIAL: this now flushes raw per-voice stereo volume, pitch, reverb, ADSR, pending stop masks, and the raw common-block tail snapshot; the original hardware voice-status/history path still remains unported.
    private void UpdateSoundVoicesState()
    {
        // PARTIAL: raw voice shadow state is mirrored into the desktop adaptation layer here.
        // MonoGame playback still does not reproduce the audible SPU reverb/envelope behavior.
        var voiceCount = Math.Min(_gameEngine.StaticVariables.g_numberOfVoices, _gameEngine.StaticVariables.g_spuVoiceDirtyFlags.Length);
        var voiceStatusCount = Math.Min(_gameEngine.StaticVariables.g_numberOfVoices, _gameEngine.StaticVariables.g_voiceRuntimeSlots.Length);
        var activeVoiceBufferIndex = (_gameEngine.StaticVariables.g_activeVoiceBufferIndex + 1) & 0x0F;
        _gameEngine.StaticVariables.g_activeVoiceBufferIndex = activeVoiceBufferIndex;
        var inactiveVoiceFlags = 0;
        for (short voiceId = 0; voiceId < voiceStatusCount; voiceId++)
        {
            var voiceStatus = _gameEngine.SoundBin.GetTrackedVoiceStatus(voiceId);
            _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId].field_0x06 = voiceStatus;
            if (voiceStatus == 0)
            {
                inactiveVoiceFlags |= 1 << (voiceId & 0x1F);
            }
        }

        _gameEngine.StaticVariables.DAT_801f7e18[activeVoiceBufferIndex] = inactiveVoiceFlags;

        if (_gameEngine.StaticVariables.g_voiceLockFlag == 0)
        {
            var stableInactiveVoiceFlags = -1;
            for (var index = 0; index < 0x0F && index < _gameEngine.StaticVariables.DAT_801f7e18.Length; index++)
            {
                stableInactiveVoiceFlags &= _gameEngine.StaticVariables.DAT_801f7e18[index];
            }

            for (short voiceId = 0; voiceId < voiceStatusCount; voiceId++)
            {
                if ((stableInactiveVoiceFlags & (1 << (voiceId & 0x1F))) == 0)
                {
                    continue;
                }

                if (_gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId].NoiseState == 2)
                {
                    _gameEngine.SoundBin.StopTrackedVoice(voiceId);
                }

                _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId].NoiseState = 0;
            }
        }

        var transitionVoiceCount = Math.Min(_gameEngine.StaticVariables.g_numberOfVoices, _gameEngine.StaticVariables.g_voiceRuntimeSlots.Length);
        for (short voiceId = 0; voiceId < transitionVoiceCount; voiceId++)
        {
            if (_gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId].field_0x1C != 0)
            {
                FUN_800920e0(voiceId);
            }

            if (_gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId].field_0x28 != 0)
            {
                ProcessVoiceStop(voiceId);
            }
        }

        for (var voiceId = 0; voiceId < voiceCount; voiceId++)
        {
            var dirtyFlags = _gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId];
            if (dirtyFlags == 0)
            {
                continue;
            }

            if ((dirtyFlags & 0x03) != 0)
            {
                PushTrackedVoiceStereoVolume((short)voiceId);
            }

            if ((dirtyFlags & 0x04) != 0)
            {
                PushTrackedVoicePitch((short)voiceId);
            }

            if ((dirtyFlags & 0x08) != 0)
            {
                PushTrackedVoiceReverb((short)voiceId);
            }

            if ((dirtyFlags & 0x30) != 0)
            {
                PushTrackedVoiceAdsr((short)voiceId);
            }

            _gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] = 0;
        }

        var pendingLeft = unchecked((ushort)_gameEngine.StaticVariables.g_voiceCommandPendingLeft);
        var pendingRight = unchecked((ushort)_gameEngine.StaticVariables.g_voiceCommandPendingRight);
        var driverVoiceState = _gameEngine.StaticVariables.PTR_VOICE_00_LEFT_RIGHT_800c9794 ??= new AlundraEngine.Gameplay.Voice();
        driverVoiceState.field_0x188 = _gameEngine.StaticVariables.g_voiceCommandPlayingLeft;
        driverVoiceState.field_0x18A = _gameEngine.StaticVariables.g_voiceCommandPlayingRight;
        driverVoiceState.field_0x18C = (byte)pendingLeft;
        driverVoiceState.field_0x18D = (byte)(pendingLeft >> 8);
        driverVoiceState.field_0x18E = _gameEngine.StaticVariables.g_voiceCommandPendingRight;
        driverVoiceState.field_0x198 = _gameEngine.StaticVariables.DAT_sound_801f7ef8;
        driverVoiceState.field_0x19A = _gameEngine.StaticVariables.DAT_sound_801f7f00;
        ApplyPendingVoiceStopsToBackend(pendingLeft, pendingRight);
    }

    // GHIDRA: ProcessVoiceStop @ 0x8009261C
    // PARTIAL: volume-transition fields are ported; upstream writers for the transition fields remain partially closed.
    private void ProcessVoiceStop(short voiceId)
    {
        if ((uint)voiceId >= (uint)_gameEngine.StaticVariables.g_voiceRuntimeSlots.Length || !TryGetCurrentVabContext(out var vabHeader, out _, out _))
        {
            return;
        }

        ref var voiceSlot = ref _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId];
        if (voiceSlot.field_0x2C != 0)
        {
            var field_0x2e = unchecked((ushort)voiceSlot.field_0x2E);
            voiceSlot.field_0x2E = unchecked((short)(field_0x2e - 1));
            if (unchecked((int)((uint)field_0x2e << 16)) > 0)
            {
                return;
            }

            voiceSlot.field_0x2E = voiceSlot.field_0x2C;
        }

        var step = voiceSlot.field_0x2A;
        var nextValue = unchecked((short)(voiceSlot.field_0x30 + step));
        voiceSlot.field_0x30 = nextValue;
        if (step < 1)
        {
            if (step < 0 && voiceSlot.field_0x32 >= nextValue)
            {
                voiceSlot.field_0x30 = voiceSlot.field_0x32;
                voiceSlot.field_0x28 = 0;
            }
        }
        else if (nextValue >= voiceSlot.field_0x32)
        {
            voiceSlot.field_0x30 = voiceSlot.field_0x32;
            voiceSlot.field_0x28 = 0;
        }

        _gameEngine.StaticVariables.DAT_801f769d = unchecked((byte)voiceSlot.field_0x30);
        var baseVolume = (_gameEngine.StaticVariables.DAT_801f769c * vabHeader.Header.Mvol * 0x3fff) / 0x3f01;
        baseVolume = (baseVolume * _gameEngine.StaticVariables.DAT_801f76a2 * _gameEngine.StaticVariables.DAT_801f76a5) / 0x3f01;

        var tonePan = _gameEngine.StaticVariables.DAT_801f76a6;
        var leftVolume = baseVolume;
        var rightVolume = baseVolume;
        if (tonePan < 0x40)
        {
            rightVolume = (rightVolume * tonePan) >> 6;
        }
        else
        {
            leftVolume = (leftVolume * (0x7f - tonePan)) >> 6;
        }

        var programPan = _gameEngine.StaticVariables.DAT_801f76a3;
        if (programPan < 0x40)
        {
            rightVolume = (unchecked((ushort)rightVolume) * programPan) >> 6;
        }
        else
        {
            leftVolume = (unchecked((ushort)leftVolume) * (0x7f - programPan)) >> 6;
        }

        var voicePan = _gameEngine.StaticVariables.DAT_801f769d;
        if (voicePan < 0x40)
        {
            rightVolume = (unchecked((ushort)rightVolume) * voicePan) >> 6;
        }
        else
        {
            leftVolume = (unchecked((ushort)leftVolume) * (0x7f - voicePan)) >> 6;
        }

        if (_gameEngine.StaticVariables.DAT_sound_801f7658 == 1)
        {
            if (leftVolume < rightVolume)
            {
                leftVolume = rightVolume;
            }
            else
            {
                rightVolume = leftVolume;
            }
        }

        _gameEngine.StaticVariables.g_spuVoiceVolumeLeft[voiceId] = unchecked((short)leftVolume);
        _gameEngine.StaticVariables.g_spuVoiceVolumeRight[voiceId] = unchecked((short)rightVolume);
        _gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] = (byte)(_gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] | 0x03);
    }

    // GHIDRA: FUN_800920E0 @ 0x800920E0
    // PARTIAL: volume-transition fields are ported; upstream writers for the transition fields remain partially closed.
    private void FUN_800920e0(short voiceId)
    {
        if ((uint)voiceId >= (uint)_gameEngine.StaticVariables.g_voiceRuntimeSlots.Length || !TryGetCurrentVabContext(out var vabHeader, out _, out _))
        {
            return;
        }

        ref var voiceSlot = ref _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId];
        if (voiceSlot.field_0x20 != 0)
        {
            var field_0x22 = unchecked((ushort)voiceSlot.field_0x22);
            voiceSlot.field_0x22 = unchecked((short)(field_0x22 - 1));
            if (unchecked((int)((uint)field_0x22 << 16)) > 0)
            {
                return;
            }

            voiceSlot.field_0x22 = voiceSlot.field_0x20;
        }

        var step = voiceSlot.field_0x1E;
        var nextValue = unchecked((short)(voiceSlot.field_0x24 + step));
        voiceSlot.field_0x24 = nextValue;
        if (step < 1)
        {
            if (step < 0 && voiceSlot.field_0x26 >= nextValue)
            {
                voiceSlot.field_0x24 = voiceSlot.field_0x26;
                voiceSlot.field_0x1C = 0;
            }
        }
        else if (nextValue >= voiceSlot.field_0x26)
        {
            voiceSlot.field_0x24 = voiceSlot.field_0x26;
            voiceSlot.field_0x1C = 0;
        }

        _gameEngine.StaticVariables.DAT_801f769c = unchecked((byte)voiceSlot.field_0x24);
        var baseVolume = (voiceSlot.field_0x24 * vabHeader.Header.Mvol * 0x3fff) / 0x3f01;
        baseVolume = (baseVolume * _gameEngine.StaticVariables.DAT_801f76a2 * _gameEngine.StaticVariables.DAT_801f76a5) / 0x3f01;

        var tonePan = _gameEngine.StaticVariables.DAT_801f76a6;
        var leftVolume = baseVolume;
        var rightVolume = baseVolume;
        if (tonePan < 0x40)
        {
            rightVolume = (rightVolume * tonePan) >> 6;
        }
        else
        {
            leftVolume = (leftVolume * (0x7f - tonePan)) >> 6;
        }

        var programPan = _gameEngine.StaticVariables.DAT_801f76a3;
        if (programPan < 0x40)
        {
            rightVolume = (unchecked((ushort)rightVolume) * programPan) >> 6;
        }
        else
        {
            leftVolume = (unchecked((ushort)leftVolume) * (0x7f - programPan)) >> 6;
        }

        var voicePan = _gameEngine.StaticVariables.DAT_801f769d;
        if (voicePan < 0x40)
        {
            rightVolume = (unchecked((ushort)rightVolume) * voicePan) >> 6;
        }
        else
        {
            leftVolume = (unchecked((ushort)leftVolume) * (0x7f - voicePan)) >> 6;
        }

        if (_gameEngine.StaticVariables.DAT_sound_801f7658 == 1)
        {
            if (leftVolume < rightVolume)
            {
                leftVolume = rightVolume;
            }
            else
            {
                rightVolume = leftVolume;
            }
        }

        _gameEngine.StaticVariables.g_spuVoiceVolumeLeft[voiceId] = unchecked((short)leftVolume);
        _gameEngine.StaticVariables.g_spuVoiceVolumeRight[voiceId] = unchecked((short)rightVolume);
        _gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] = (byte)(_gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] | 0x03);
    }

    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: consumes staged SPU key-off masks by stopping tracked desktop voices
    private void ApplyPendingVoiceStopsToBackend(ushort pendingLeft, ushort pendingRight)
    {
        var trackedVoiceCount = Math.Min(_gameEngine.StaticVariables.g_voiceRuntimeSlots.Length, _gameEngine.SoundBin.VoicesAreActive.Length);
        for (var voiceId = 0; voiceId < trackedVoiceCount; voiceId++)
        {
            GetVoiceCommandMasks((short)voiceId, out var leftMask, out var rightMask);
            if (((pendingLeft & leftMask) != 0) || ((pendingRight & rightMask) != 0))
            {
                _gameEngine.SoundBin.KeyOffTrackedVoice(voiceId);
            }
        }

        _gameEngine.StaticVariables.g_voiceCommandPendingLeft = 0;
        _gameEngine.StaticVariables.g_voiceCommandPendingRight = 0;
        _gameEngine.StaticVariables.g_voiceCommandPlayingLeft = 0;
        _gameEngine.StaticVariables.g_voiceCommandPlayingRight = 0;
    }

    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: adapter for raw SPU per-voice left/right updates on desktop tracked voices
    private void PushTrackedVoiceStereoVolume(short voiceId)
    {
        if ((uint)voiceId >= (uint)_gameEngine.StaticVariables.g_spuVoiceVolumeLeft.Length)
        {
            return;
        }

        _gameEngine.SoundBin.UpdateTrackedVoiceStereoVolume(
            voiceId,
            _gameEngine.StaticVariables.g_spuVoiceVolumeLeft[voiceId],
            _gameEngine.StaticVariables.g_spuVoiceVolumeRight[voiceId]);
    }

    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: adapter for raw SPU per-voice pitch updates on desktop tracked voices
    private void PushTrackedVoicePitch(short voiceId)
    {
        if ((uint)voiceId >= (uint)_gameEngine.StaticVariables.g_spuVoicePitch.Length)
        {
            return;
        }

        _gameEngine.SoundBin.UpdateTrackedVoicePitch(
            voiceId,
            _gameEngine.StaticVariables.g_spuVoicePitch[voiceId]);
    }

    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: adapter for raw SPU per-voice reverb shadow updates on desktop tracked voices
    private void PushTrackedVoiceReverb(short voiceId)
    {
        if ((uint)voiceId >= (uint)_gameEngine.StaticVariables.g_spuVoiceReverb.Length)
        {
            return;
        }

        _gameEngine.SoundBin.UpdateTrackedVoiceReverb(
            voiceId,
            _gameEngine.StaticVariables.g_spuVoiceReverb[voiceId]);
    }

    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: adapter for raw SPU per-voice ADSR shadow updates on desktop tracked voices
    private void PushTrackedVoiceAdsr(short voiceId)
    {
        if ((uint)voiceId >= (uint)_gameEngine.StaticVariables.g_spuVoiceAdsr1.Length
            || (uint)voiceId >= (uint)_gameEngine.StaticVariables.g_spuVoiceAdsr2.Length)
        {
            return;
        }

        _gameEngine.SoundBin.UpdateTrackedVoiceAdsr(
            voiceId,
            _gameEngine.StaticVariables.g_spuVoiceAdsr1[voiceId],
            _gameEngine.StaticVariables.g_spuVoiceAdsr2[voiceId]);
    }

    // GHIDRA: FUN_8008E3D8 @ 0x8008E3D8
    public void FUN_8008e3d8()
    {
        if (_gameEngine.StaticVariables.g_voiceCommandLock == 1)
        {
            return;
        }

        _gameEngine.StaticVariables.g_voiceCommandLock = 1;
        UpdateSoundVoicesState();

        var sequenceSlotCount = _gameEngine.StaticVariables.g_sequenceSlotCount;
        var sequenceTrackCount = _gameEngine.StaticVariables.g_sequenceTrackCount;

        if (sequenceSlotCount > 0)
        {
            for (short sequenceSlot = 0; sequenceSlot < sequenceSlotCount && sequenceSlot < _gameEngine.StaticVariables.g_sequenceStatePointers.Length; sequenceSlot++)
            {
                if ((_gameEngine.StaticVariables.g_sequenceSlotMask & (1 << sequenceSlot)) == 0)
                {
                    continue;
                }

                for (short trackIndex = 0; trackIndex < sequenceTrackCount && trackIndex < 1; trackIndex++)
                {
                    ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[sequenceSlot];
                    var flags = sequenceState.Flags;

                    if ((flags & 0x01) != 0)
                    {
                        FUN_8008ebf8(sequenceSlot, trackIndex);

                        if ((sequenceState.Flags & 0x10) != 0)
                        {
                            FUN_8008e610(sequenceSlot, trackIndex);
                        }

                        if ((sequenceState.Flags & 0x20) != 0)
                        {
                            FUN_8008e8d0(sequenceSlot, trackIndex);
                        }

                        if ((sequenceState.Flags & 0x40) != 0)
                        {
                            FUN_8008f4ac(sequenceSlot, trackIndex);
                        }

                        if ((sequenceState.Flags & 0x80) != 0)
                        {
                            FUN_8008f4ac(sequenceSlot, trackIndex);
                        }
                    }

                    if ((sequenceState.Flags & 0x02) != 0)
                    {
                        FUN_8008eb5c(sequenceSlot, trackIndex);
                    }

                    if ((sequenceState.Flags & 0x08) != 0)
                    {
                        FUN_8008ec24(sequenceSlot, trackIndex);
                    }

                    if ((sequenceState.Flags & 0x04) != 0)
                    {
                        FUN_8008f2e8(sequenceSlot, trackIndex);
                        sequenceState.Flags = 0;
                    }
                }
            }
        }

        _gameEngine.StaticVariables.g_voiceCommandLock = 0;
    }

    // GHIDRA: FUN_8008EBF8 @ 0x8008EBF8
    private void FUN_8008ebf8(short seqId, short trackId)
    {
        FUN_8008bcc4(seqId, trackId);
    }

    // GHIDRA: FUN_8008BCC4 @ 0x8008BCC4
    private void FUN_8008bcc4(short seqId, short trackId)
    {
        if ((uint)seqId >= _gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        var sequenceData = _loadedSequenceData[seqId];
        if (sequenceData == null || sequenceData.Length == 0)
        {
            _gameEngine.StaticVariables.g_sequenceStatePointers[seqId].Flags &= ~1u;
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        var tempo = sequenceState.CurrentTempo;
        var delay = sequenceState.Delay;
        var reducedDelay = unchecked((uint)((int)delay - tempo));

        if ((int)reducedDelay < 1)
        {
            do
            {
                if (tempo < (int)delay)
                {
                    return;
                }

                FUN_8008bdd0(seqId, trackId);
                while ((sequenceState.Flags & 1u) != 0 && sequenceState.Delay == 0)
                {
                    FUN_8008bdd0(seqId, trackId);
                }

                if ((sequenceState.Flags & 1u) == 0)
                {
                    return;
                }

                delay += sequenceState.Delay;
                reducedDelay = unchecked((uint)((int)delay - sequenceState.CurrentTempo));
            } while ((int)delay < sequenceState.CurrentTempo);
        }
        else
        {
            var preDelay = sequenceState.PreDelay;
            if (preDelay > 0)
            {
                sequenceState.PreDelay = (short)(preDelay - 1);
                return;
            }

            if (preDelay != 0)
            {
                sequenceState.Delay = reducedDelay;
                return;
            }

            sequenceState.PreDelay = tempo;
            reducedDelay = unchecked(sequenceState.Delay - 1);
        }

        sequenceState.Delay = reducedDelay;
    }

    // GHIDRA: FUN_8008BDD0 @ 0x8008BDD0
    private void FUN_8008bdd0(short seqId, short trackId)
    {
        if ((uint)seqId >= _gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        var sequenceData = _loadedSequenceData[seqId];
        if (sequenceData == null || sequenceData.Length == 0)
        {
            _gameEngine.StaticVariables.g_sequenceStatePointers[seqId].Flags &= ~1u;
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        if ((sequenceState.Flags & 1u) != 1u)
        {
            return;
        }

        if ((uint)sequenceState.SeqPosition >= (uint)sequenceData.Length)
        {
            sequenceState.Flags &= ~1u;
            return;
        }

        var instruction = sequenceData[sequenceState.SeqPosition++];
        var channel = (byte)(instruction & 0x0F);
        byte messageType;

        if ((instruction & 0x80) != 0)
        {
            sequenceState.CurrentChannel = channel;
            messageType = (byte)(instruction & 0xF0);
            sequenceState.MessageType = messageType;
        }
        else
        {
            messageType = sequenceState.MessageType;
            channel = sequenceState.CurrentChannel;
            if (messageType == 0xFF)
            {
                FUN_8008d568(seqId, trackId, (char)instruction);
                return;
            }

            if (messageType != 0x90 && messageType != 0xB0 && messageType != 0xC0 && messageType != 0xE0)
            {
                return;
            }

            sequenceState.SeqPosition--;
        }

        switch (messageType)
        {
            case 0x80:
            case 0xA0:
                return;

            case 0x90:
            {
                if ((uint)(sequenceState.SeqPosition + 1) >= (uint)sequenceData.Length)
                {
                    sequenceState.Flags &= ~1u;
                    return;
                }

                var data1 = (byte)(sequenceData[sequenceState.SeqPosition++] & 0x7F);
                var data2 = (byte)(sequenceData[sequenceState.SeqPosition++] & 0x7F);
                sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
                FUN_8008c064(seqId, trackId, data1, data2);
                return;
            }

            case 0xE0:
            {
                if ((uint)(sequenceState.SeqPosition + 1) >= (uint)sequenceData.Length)
                {
                    sequenceState.Flags &= ~1u;
                    return;
                }

                var data1 = (byte)(sequenceData[sequenceState.SeqPosition++] & 0x7F);
                var data2 = (byte)(sequenceData[sequenceState.SeqPosition] & 0x7F);
                _sequenceChannelPitchBends[seqId, channel] = (ushort)(data1 + (data2 << 7));
                FUN_8008d4c0(seqId, trackId);
                return;
            }

            case 0xC0:
            {
                if ((uint)sequenceState.SeqPosition >= (uint)sequenceData.Length)
                {
                    sequenceState.Flags &= ~1u;
                    return;
                }

                var data1 = (byte)(sequenceData[sequenceState.SeqPosition++] & 0x7F);
                FUN_8008c144(seqId, trackId, data1);
                return;
            }

            case 0xD0:
                return;

            case 0xB0:
            {
                if ((uint)sequenceState.SeqPosition >= (uint)sequenceData.Length)
                {
                    sequenceState.Flags &= ~1u;
                    return;
                }

                var control = sequenceData[sequenceState.SeqPosition++];
                FUN_8008c1b8(seqId, trackId, control);
                return;
            }

            case 0xF0:
                sequenceState.MessageType = 0xFF;
                goto SystemMessageMeta;

            default:
                return;
        }

    SystemMessageMeta:
        if ((uint)sequenceState.SeqPosition >= (uint)sequenceData.Length)
        {
            sequenceState.Flags &= ~1u;
            return;
        }

        sequenceState.MessageType = 0xFF;
        FUN_8008d568(seqId, trackId, (char)sequenceData[sequenceState.SeqPosition++]);
        return;
    }

    // GHIDRA: FUN_8008D568 @ 0x8008D568
    private void FUN_8008d568(short seqId, short trackId, char metaType)
    {
        if ((uint)seqId >= (uint)_gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        var sequenceData = _loadedSequenceData[seqId];
        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];

        if (metaType == '/')
        {
            var timesPlayed = (ushort)(sequenceState.TimesPlayed + 1);
            sequenceState.TimesPlayed = timesPlayed;
            if (sequenceState.LoopCount == 0)
            {
                sequenceState.Playtime = 0;
                sequenceState.field_0x27 = 0;
                sequenceState.Delay = 0;
                sequenceState.SeqPosition = sequenceState.SeqStartPos;
            }
            else if ((short)timesPlayed < sequenceState.LoopCount)
            {
                sequenceState.Playtime = 0;
                sequenceState.field_0x27 = 0;
                sequenceState.Delay = 0;
                sequenceState.SeqPosition = sequenceState.SeqStartPos;
                sequenceState.SeqLoopPos = sequenceState.SeqStartPos;
            }
            else
            {
                sequenceState.Flags &= ~1u;
                sequenceState.Flags &= ~8u;
                sequenceState.Flags &= ~2u;
                sequenceState.Flags |= 0x200u;
                sequenceState.Flags |= 4u;
                sequenceState.field_0x2B = 0;
                sequenceState.SeqLoopPos = sequenceState.SeqStartPos;
                if ((sbyte)sequenceState.field_0x3C != -1)
                {
                    sequenceState.field_0x2B = 0;
                    FUN_8008da70((byte)sequenceState.field_0x3C, (short)(byte)sequenceState.field_0x00);
                    FUN_80093ef4(ComposeSequenceKey(seqId, trackId));
                }

                FUN_80093ef4(ComposeSequenceKey(seqId, trackId));
                sequenceState.Delay = unchecked((ushort)sequenceState.CurrentTempo);
            }

            return;
        }

        if (metaType == 'Q')
        {
            if (sequenceData == null || (uint)(sequenceState.SeqPosition + 2) >= (uint)sequenceData.Length)
            {
                sequenceState.Flags &= ~1u;
                return;
            }

            var tempoByte0 = sequenceData[sequenceState.SeqPosition++];
            var tempoByte1 = sequenceData[sequenceState.SeqPosition++];
            var tempoByte2 = sequenceData[sequenceState.SeqPosition++];
            var tempoValue = (tempoByte0 << 16) | (tempoByte1 << 8) | tempoByte2;
            if (tempoValue == 0)
            {
                sequenceState.Flags &= ~1u;
                return;
            }

            var tempoMultiplier = _gameEngine.StaticVariables.DAT_801f6ce0;
            var beatsPerMinute = 60000000u / (uint)tempoValue;
            var scaledTempo = (uint)sequenceState.Tempo * beatsPerMinute;
            var driverTempo = (uint)(tempoMultiplier * 0x3C);
            sequenceState.field_0x8C = beatsPerMinute;
            if (scaledTempo * 10 < driverTempo)
            {
                if (scaledTempo == 0)
                {
                    sequenceState.Flags &= ~1u;
                    return;
                }

                var nextTempo = (ushort)((uint)(tempoMultiplier * 600) / scaledTempo);
                sequenceState.PreDelay = unchecked((short)nextTempo);
                sequenceState.CurrentTempo = unchecked((short)nextTempo);
            }
            else
            {
                if (driverTempo == 0)
                {
                    sequenceState.Flags &= ~1u;
                    return;
                }

                sequenceState.PreDelay = -1;
                var nextTempo = (ushort)(((uint)sequenceState.Tempo * sequenceState.field_0x8C * 10) / driverTempo);
                sequenceState.CurrentTempo = unchecked((short)nextTempo);
                if ((uint)(tempoMultiplier * 0x1E) < (((uint)sequenceState.Tempo * sequenceState.field_0x8C * 10) % driverTempo))
                {
                    sequenceState.CurrentTempo = unchecked((short)(nextTempo + 1));
                }
            }

            sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
            return;
        }

        return;
    }

    // GHIDRA: FUN_8008DA70 @ 0x8008DA70
    private void FUN_8008da70(int param_1, short param_2)
    {
        if ((uint)param_1 >= (uint)_gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        if (param_2 != 0)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[param_1];
        sequenceState.LoopCount = 1;
        sequenceState.TimesPlayed = 0;
        sequenceState.Flags &= ~0x100u;
        sequenceState.Flags &= ~8u;
        sequenceState.Flags &= ~2u;
        sequenceState.Flags &= ~4u;
        sequenceState.Flags &= ~0x200u;
        sequenceState.field_0x2B = 1;
        sequenceState.SeqPosition = sequenceState.SeqStartPos;
        sequenceState.Flags |= 1u;
    }

    // GHIDRA: FUN_8008C1B8 @ 0x8008C1B8
    // PARTIAL: active-voice refresh side effects driven by FUN_8009410C and desktop SPU reverb application remain partial.
    private void FUN_8008c1b8(short seqId, short trackId, byte control)
    {
        if ((uint)seqId >= (uint)_loadedSequenceData.Length)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        var sequenceData = _loadedSequenceData[seqId];
        if (sequenceData == null)
        {
            sequenceState.Flags &= ~1u;
            return;
        }

        if ((uint)sequenceState.SeqPosition >= (uint)sequenceData.Length)
        {
            sequenceState.Flags &= ~1u;
            return;
        }

        var value = sequenceData[sequenceState.SeqPosition++];
        var currentChannel = sequenceState.CurrentChannel;
        var programNumber = (short)GetSequenceChannelMapping(ref sequenceState, currentChannel);

        if (control > 0x79)
        {
            sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
            return;
        }

        switch (control)
        {
            case 0:
                sequenceState.Vab = value;
                break;

            case 6:
                FUN_8008cc70(seqId, trackId, value);
                return;

            case 7:
                FUN_8009410c((ushort)ComposeSequenceKey(seqId, trackId), (ushort)sequenceState.Vab, programNumber,
                    value, GetSequenceChannelOrientation(ref sequenceState, currentChannel));
                SetSequenceChannelVolume(ref sequenceState, currentChannel, value);
                break;

            case 10:
                FUN_8009410c((ushort)ComposeSequenceKey(seqId, trackId), (ushort)sequenceState.Vab, programNumber,
                    GetSequenceChannelVolume(ref sequenceState, currentChannel), value);
                SetSequenceChannelOrientation(ref sequenceState, currentChannel, value);
                break;

            case 11:
                FUN_80093f8c((ushort)sequenceState.Vab, programNumber, value);
                FUN_8009410c((ushort)ComposeSequenceKey(seqId, trackId), (ushort)sequenceState.Vab, programNumber,
                    GetSequenceChannelVolume(ref sequenceState, currentChannel), GetSequenceChannelOrientation(ref sequenceState, currentChannel));
                break;

            case 0x40:
                if (value < 0x40)
                {
                    FUN_80090144();
                }
                else
                {
                    FUN_80090154();
                }
                break;

            case 0x41:
                FUN_8008c70c(seqId, trackId, value);
                return;

            case 0x5B:
                FUN_80090728(value, value);
                break;

            case 0x62:
                FUN_8008c918(seqId, trackId, value);
                return;

            case 0x63:
                FUN_8008ca40(seqId, trackId, value);
                return;

            case 0x64:
                FUN_8008cb88(seqId, trackId, value);
                return;

            case 0x65:
                FUN_8008cbfc(seqId, trackId, value);
                return;

            case 0x79:
                FUN_8008c854(seqId, trackId);
                return;

            default:
                break;
        }

        sequenceState.Delay = FUN_8008d8d0((short)seqId, trackId);
    }

    // GHIDRA: FUN_8008C70C @ 0x8008C70C
    private void FUN_8008c70c(short seqId, short trackId, byte value)
    {
        if ((uint)seqId >= (uint)_gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        var currentChannel = sequenceState.CurrentChannel;
        var programNumber = (short)GetSequenceChannelMapping(ref sequenceState, currentChannel);
        FUN_800901a8(sequenceState.Vab, programNumber, out var programAttributes);

        for (short toneIndex = 0; toneIndex < programAttributes.Tones; toneIndex++)
        {
            FUN_80090370(sequenceState.Vab, programNumber, toneIndex, out var toneAttributes);

            if (value < 0x40)
            {
                toneAttributes.Mode = 2;
            }
            else if (value < 0x80)
            {
                toneAttributes.Mode = 0;
            }

            FUN_80090824(sequenceState.Vab, programNumber, toneIndex, ref toneAttributes);
        }

        sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
    }

    // GHIDRA: FUN_8008C854 @ 0x8008C854
    // PARTIAL: FUN_800906C8 is currently a desktop no-op because SPU reverb is not mapped to the MonoGame backend yet.
    private void FUN_8008c854(short seqId, short trackId)
    {
        if ((uint)seqId >= (uint)_gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        FUN_800906c8();
        FUN_80090144();
        SetSequenceChannelMapping(ref sequenceState, sequenceState.CurrentChannel, (byte)sequenceState.CurrentChannel);
        sequenceState.field_0x13 = 0;
        sequenceState.field_0x14 = (ushort)(sequenceState.field_0x14 & 0xFF00);
        SetSequenceChannelVolume(ref sequenceState, sequenceState.CurrentChannel, 0x7F);
        SetSequenceChannelOrientation(ref sequenceState, sequenceState.CurrentChannel, 0x40);
        sequenceState.Delay = FUN_8008d8d0((short)seqId, trackId);
    }

    // GHIDRA: FUN_8008C918 @ 0x8008C918
    // PARTIAL: the 0x801F6D68 callback table now dispatches through raw registered entries, but its registration/lifecycle semantics remain unresolved.
    private void FUN_8008c918(short seqId, short trackId, byte value)
    {
        if ((uint)seqId >= (uint)_gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        if (sequenceState.field_0x27 == 1 && sequenceState.field_0x10 == 0)
        {
            sequenceState.Loops = value;
            sequenceState.field_0x10 = 1;
        }
        else
        {
            var field16 = sequenceState.field_0x16;
            if (field16 != 0x1E && field16 != 0x14)
            {
                sequenceState.field_0x14 = (ushort)((sequenceState.field_0x14 & 0x00FF) | (value << 8));
                sequenceState.field_0x2A++;
            }
        }

        if (sequenceState.field_0x16 == 0x28)
        {
            var callbackIndex = (((int)(ushort)seqId) << 4) + (ushort)trackId;
            if ((uint)callbackIndex < (uint)_gameEngine.StaticVariables.DAT_801f6d68.Length)
            {
                _gameEngine.StaticVariables.DAT_801f6d68[callbackIndex]?.Invoke(seqId, trackId, value);
            }
        }

        sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
    }

    // GHIDRA: FUN_8008CA40 @ 0x8008CA40
    private void FUN_8008ca40(short seqId, short trackId, byte value)
    {
        if ((uint)seqId >= (uint)_gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        if (value == 0x14)
        {
            sequenceState.field_0x16 = 0x14;
            sequenceState.field_0x27 = 1;
            sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
            sequenceState.SeqLoopPos = sequenceState.SeqPosition;
            return;
        }

        if (value == 0x1E)
        {
            sequenceState.field_0x16 = 0x1E;
            if (sequenceState.Loops == 0)
            {
                sequenceState.field_0x10 = 0;
                sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
                return;
            }

            if (sequenceState.Loops < 0x7F)
            {
                sequenceState.Loops--;
                sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
                if (sequenceState.Loops == 0)
                {
                    sequenceState.field_0x10 = 0;
                }
                else
                {
                    sequenceState.SeqPosition = sequenceState.SeqLoopPos;
                }

                return;
            }

            FUN_8008d8d0(seqId, trackId);
            sequenceState.Delay = 0;
            sequenceState.SeqPosition = sequenceState.SeqLoopPos;
            return;
        }

        sequenceState.field_0x16 = value;
        sequenceState.field_0x2A++;
        sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
    }

    // GHIDRA: FUN_8008CB88 @ 0x8008CB88
    private void FUN_8008cb88(short seqId, short trackId, byte value)
    {
        if ((uint)seqId >= (uint)_gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        sequenceState.field_0x13 = value;
        sequenceState.field_0x29++;
        sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
    }

    // GHIDRA: FUN_8008CBFC @ 0x8008CBFC
    private void FUN_8008cbfc(short seqId, short trackId, byte value)
    {
        if ((uint)seqId >= (uint)_gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        sequenceState.field_0x14 = (ushort)((sequenceState.field_0x14 & 0xFF00) | value);
        sequenceState.field_0x29++;
        sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
    }

    // GHIDRA: FUN_8008CC70 @ 0x8008CC70
    private void FUN_8008cc70(short seqId, short trackId, byte value)
    {
        if ((uint)seqId >= (uint)_gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        var currentChannel = sequenceState.CurrentChannel;
        var programNumber = (short)GetSequenceChannelMapping(ref sequenceState, currentChannel);
        FUN_800901a8(sequenceState.Vab, programNumber, out var programAttributes);

        if (sequenceState.field_0x27 == 1 && sequenceState.field_0x10 == 0)
        {
            sequenceState.Loops = value;
            sequenceState.field_0x10 = 1;
            sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
            return;
        }

        var registeredParameter = unchecked((byte)sequenceState.field_0x14);
        if (sequenceState.field_0x29 == 2)
        {
            if (sequenceState.field_0x13 == 0 && registeredParameter == 0)
            {
                for (short toneIndex = 0; toneIndex < programAttributes.Tones; toneIndex++)
                {
                    FUN_80090370(sequenceState.Vab, programNumber, toneIndex, out var toneAttributes);
                    toneAttributes.PitchBendMin = unchecked((byte)(value & 0x7F));
                    toneAttributes.PitchBendMax = unchecked((byte)(value & 0x7F));
                    FUN_80090824(sequenceState.Vab, programNumber, toneIndex, ref toneAttributes);
                }
            }

            if (sequenceState.field_0x13 == 1 && registeredParameter == 0)
            {
                for (short toneIndex = 0; toneIndex < programAttributes.Tones; toneIndex++)
                {
                    FUN_80090370(sequenceState.Vab, programNumber, toneIndex, out var toneAttributes);
                    FUN_80090824(sequenceState.Vab, programNumber, toneIndex, ref toneAttributes);
                }
            }

            if (sequenceState.field_0x13 == 2 && registeredParameter == 0)
            {
                for (short toneIndex = 0; toneIndex < programAttributes.Tones; toneIndex++)
                {
                    FUN_80090370(sequenceState.Vab, programNumber, toneIndex, out var toneAttributes);
                    FUN_80090824(sequenceState.Vab, programNumber, toneIndex, ref toneAttributes);
                }
            }

            sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
            sequenceState.field_0x29 = 0;
            return;
        }

        if (sequenceState.field_0x2A == 2)
        {
            var controller = unchecked((byte)(sequenceState.field_0x14 >> 8));
            if (sequenceState.field_0x16 == 0x10)
            {
                for (short toneIndex = 0; toneIndex < programAttributes.Tones; toneIndex++)
                {
                    FUN_8008d1f4(sequenceState.Vab, programNumber, toneIndex, controller, value);
                }
            }
            else
            {
                FUN_8008d1f4(sequenceState.Vab, programNumber, sequenceState.field_0x16, controller, value);
            }

            sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
            sequenceState.field_0x2A = 0;
            return;
        }

        sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
    }

    // GHIDRA: FUN_8008D1F4 @ 0x8008D1F4
    // PARTIAL: controller subcases touching SPU reverb only record the proven raw reverb fields; the desktop backend still does not apply PSX reverb.
    private void FUN_8008d1f4(short vabId, short programIndex, short toneIndex, byte controller, byte value)
    {
        FUN_80090370(vabId, programIndex, toneIndex, out var toneAttributes);

        switch (controller)
        {
            case 0:
                toneAttributes.Priority = value;
                FUN_80090824(vabId, programIndex, toneIndex, ref toneAttributes);
                break;

            case 1:
                toneAttributes.Mode = value;
                FUN_80090824(vabId, programIndex, toneIndex, ref toneAttributes);
                if (value == 0)
                {
                    FUN_800906c8();
                }
                else if (value == 4)
                {
                    FUN_800906a8();
                }

                break;

            case 2:
                toneAttributes.Min = value;
                FUN_80090824(vabId, programIndex, toneIndex, ref toneAttributes);
                break;

            case 3:
                toneAttributes.Max = value;
                FUN_80090824(vabId, programIndex, toneIndex, ref toneAttributes);
                break;

            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 0x0B:
            case 0x0C:
            case 0x0D:
            case 0x0E:
                var adsrFields = new ushort[9];
                FUN_8008d988(toneAttributes.Adsr1, toneAttributes.Adsr2, adsrFields);
                var rawValue = (ushort)value;

                switch (controller - 4)
                {
                    case 0:
                        adsrFields[5] = 0;
                        adsrFields[0] = rawValue;
                        break;

                    case 1:
                        adsrFields[5] = 1;
                        adsrFields[0] = rawValue;
                        break;

                    case 2:
                        adsrFields[1] = rawValue;
                        break;

                    case 3:
                        adsrFields[2] = rawValue;
                        break;

                    case 4:
                        adsrFields[6] = 0;
                        adsrFields[3] = rawValue;
                        break;

                    case 5:
                        adsrFields[6] = 1;
                        adsrFields[3] = rawValue;
                        break;

                    case 6:
                        adsrFields[7] = 0;
                        adsrFields[4] = rawValue;
                        break;

                    case 7:
                        adsrFields[7] = 1;
                        adsrFields[4] = rawValue;
                        break;

                    case 8:
                        if (value != 0 && value < 0x40)
                        {
                            adsrFields[8] = 0;
                        }
                        else if (unchecked((byte)(value - 0x40)) < 0x40)
                        {
                            adsrFields[8] = 1;
                        }

                        break;

                    case 9:
                        break;

                    case 10:
                        toneAttributes.PortamentoWidth = value;
                        break;
                }

                FUN_8008d9e4(adsrFields, out var adsr1, out var adsr2);
                toneAttributes.Adsr1 = adsr1;
                toneAttributes.Adsr2 = adsr2;
                FUN_80090824(vabId, programIndex, toneIndex, ref toneAttributes);
                break;

            case 0x0F:
                FUN_800905f8(value);
                break;

            case 0x10:
                FUN_80090728(value, value);
                break;

            case 0x11:
                FUN_800906e8(value);
                break;

            case 0x12:
            case 0x13:
                FUN_800907e4(value);
                break;
        }
    }

    // GHIDRA: FUN_8008D988 @ 0x8008D988
    private static void FUN_8008d988(ushort adsr1, ushort adsr2, ushort[] unpackedFields)
    {
        unpackedFields[5] = (ushort)(adsr1 & 0x8000);
        unpackedFields[6] = (ushort)(adsr2 & 0x8000);
        unpackedFields[8] = (ushort)(adsr2 & 0x4000);
        unpackedFields[7] = (ushort)(adsr2 & 0x20);
        unpackedFields[0] = (ushort)((adsr1 >> 8) & 0x7F);
        unpackedFields[1] = (ushort)((adsr1 >> 4) & 0x0F);
        unpackedFields[2] = (ushort)(adsr1 & 0x0F);
        unpackedFields[3] = (ushort)((adsr2 >> 6) & 0x7F);
        unpackedFields[4] = (ushort)(adsr2 & 0x1F);
    }

    // GHIDRA: FUN_8008D9E4 @ 0x8008D9E4
    private static void FUN_8008d9e4(ushort[] unpackedFields, out ushort adsr1, out ushort adsr2)
    {
        var highAdsr2 = unpackedFields[6] != 0 ? (ushort)0x8000 : (ushort)0;
        if (unpackedFields[8] != 0)
        {
            highAdsr2 |= 0x4000;
        }

        adsr1 = (ushort)((unpackedFields[5] != 0 ? 0x8000 : 0)
            | ((unpackedFields[0] & 0x7F) << 8)
            | ((unpackedFields[1] & 0x0F) << 4)
            | (unpackedFields[2] & 0x0F));
        adsr2 = (ushort)(highAdsr2
            | ((unpackedFields[3] & 0x7F) << 6)
            | (unpackedFields[4] & 0x1F));
    }

    // GHIDRA: FUN_80090144 @ 0x80090144
    private void FUN_80090144()
    {
        _gameEngine.StaticVariables.DAT_sound_801f7610 = 0;
    }

    // GHIDRA: FUN_80090154 @ 0x80090154
    private void FUN_80090154()
    {
        _gameEngine.StaticVariables.DAT_sound_801f7610 = 2;
    }

    // GHIDRA: FUN_800905F8 @ 0x800905F8
    // PARTIAL: records the proven g_spuReverbAttr2 writes, but the desktop backend does not apply PSX SPU reverb.
    private int FUN_800905f8(ushort value)
    {
        var signedValue = unchecked((short)value);
        var isNegative = signedValue < 0;
        var magnitude = isNegative ? (ushort)(-signedValue) : value;
        if (magnitude >= 10)
        {
            return -1;
        }

        _gameEngine.StaticVariables.g_spuReverbAttr2.Mask = 1;
        _gameEngine.StaticVariables.g_spuReverbAttr2.Mode = isNegative ? magnitude | 0x100 : magnitude;
        _gameEngine.SoundBin.UpdateReverbAttr(_gameEngine.StaticVariables.g_spuReverbAttr2);
        if (magnitude == 0)
        {
            FUN_800906c8();
        }

        return magnitude;
    }

    // GHIDRA: FUN_800906A8 @ 0x800906A8
    // PARTIAL: desktop adaptation records the reverb enable toggle, but the backend does not reproduce audible SPU reverb.
    private void FUN_800906a8()
    {
        _gameEngine.SoundBin.SetReverbEnabled(true);
    }

    // GHIDRA: FUN_800906C8 @ 0x800906C8
    // PARTIAL: desktop adaptation records the reverb disable toggle, but the backend does not reproduce audible SPU reverb.
    private void FUN_800906c8()
    {
        _gameEngine.SoundBin.SetReverbEnabled(false);
    }

    // GHIDRA: FUN_800906E8 @ 0x800906E8
    // PARTIAL: records the proven raw reverb feedback field, but the desktop backend does not apply PSX SPU reverb.
    private void FUN_800906e8(short value)
    {
        _gameEngine.StaticVariables.g_spuReverbAttr2.Mask = 0x10;
        _gameEngine.StaticVariables.g_spuReverbAttr2.Feedback = value;
        _gameEngine.SoundBin.UpdateReverbAttr(_gameEngine.StaticVariables.g_spuReverbAttr2);
    }

    // GHIDRA: FUN_80090728 @ 0x80090728
    // PARTIAL: desktop adaptation records the proven raw reverb depth fields, but the backend does not reproduce audible SPU reverb.
    private void FUN_80090728(short param_1, short param_2)
    {
        _gameEngine.StaticVariables.g_spuReverbAttr2.Mask = 6;
        _gameEngine.StaticVariables.g_spuReverbAttr2.DepthLeft = (short)((param_1 * 0x7fff) / 0x7f);
        _gameEngine.StaticVariables.g_spuReverbAttr2.DepthRight = (short)((param_2 * 0x7fff) / 0x7f);
        _gameEngine.SoundBin.UpdateReverbAttr(_gameEngine.StaticVariables.g_spuReverbAttr2);
    }

    // GHIDRA: FUN_800907E4 @ 0x800907E4
    // PARTIAL: records the proven raw reverb delay field, but the desktop backend does not apply PSX SPU reverb.
    private void FUN_800907e4(short value)
    {
        _gameEngine.StaticVariables.g_spuReverbAttr2.Mask = 8;
        _gameEngine.StaticVariables.g_spuReverbAttr2.Delay = value;
        _gameEngine.SoundBin.UpdateReverbAttr(_gameEngine.StaticVariables.g_spuReverbAttr2);
    }

    // GHIDRA: FUN_80090824 @ 0x80090824
    private int FUN_80090824(short vabId, short programIndex, short toneIndex, ref VabToneAttributesCopy toneAttributes)
    {
        var vabIndex = (ushort)vabId;
        if ((uint)vabIndex >= (uint)_gameEngine.StaticVariables.g_loadedVabState.Length
            || _gameEngine.StaticVariables.g_loadedVabState[vabIndex] != 1)
        {
            return -1;
        }

        if (SelectLoadedVabProgram(vabId, programIndex) < 0 || !TryGetLoadedVabHeader(vabId, out var vabHeader))
        {
            return -1;
        }

        if ((uint)programIndex >= (uint)vabHeader.VagAttributes.Length)
        {
            return -1;
        }

        var tones = vabHeader.VagAttributes[programIndex];
        if ((uint)toneIndex >= (uint)tones.Length)
        {
            return -1;
        }

        var toneAttr = tones[toneIndex];
        toneAttr.Prior = toneAttributes.Priority;
        toneAttr.Mode = toneAttributes.Mode;
        toneAttr.Vol = toneAttributes.Volume;
        toneAttr.Pan = toneAttributes.Pan;
        toneAttr.Center = toneAttributes.Center;
        toneAttr.Shift = toneAttributes.Shift;
        toneAttr.Min = toneAttributes.Min;
        toneAttr.Max = toneAttributes.Max;
        toneAttr.VibW = toneAttributes.VibratoWidth;
        toneAttr.VibT = toneAttributes.VibratoTime;
        toneAttr.PorW = toneAttributes.PortamentoWidth;
        toneAttr.PorT = toneAttributes.PortamentoTime;
        toneAttr.Pbmin = toneAttributes.PitchBendMin;
        toneAttr.Pbmax = toneAttributes.PitchBendMax;
        toneAttr.Adsr1 = toneAttributes.Adsr1;
        toneAttr.Adsr2 = toneAttributes.Adsr2;
        toneAttr.Prog = unchecked((short)toneAttributes.Program);
        toneAttr.Vag = unchecked((short)toneAttributes.Vag);
        return 0;
    }

    // GHIDRA: FUN_80093F8C @ 0x80093F8C
    private uint FUN_80093f8c(ushort vabId, short programNumber, byte value)
    {
        if (SelectLoadedVabProgram((short)vabId, programNumber) != 0 || !TryGetLoadedVabHeader((short)vabId, out var vabHeader))
        {
            return 0xFFFFFFFF;
        }

        if ((uint)programNumber >= (uint)vabHeader.ProgAttributes.Length)
        {
            return 0xFFFFFFFF;
        }

        vabHeader.ProgAttributes[programNumber].Mvol = value;
        return value;
    }

    // GHIDRA: FUN_8009410C @ 0x8009410C
    private int FUN_8009410c(ushort sequenceKey, ushort vabId, short programNumber, uint channelVolume, ushort orientation)
    {
        var sequenceId = sequenceKey & 0xFF;
        if ((uint)sequenceId >= (uint)_gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return 0;
        }

        if (!TryGetLoadedVabHeader((short)vabId, out var vabHeader) || (uint)programNumber >= (uint)vabHeader.ProgAttributes.Length)
        {
            return 0;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[sequenceId];
        SelectLoadedVabProgram((short)vabId, programNumber);
        var currentChannel = sequenceState.CurrentChannel;
        var currentChannelVolume = GetSequenceChannelVolume(ref sequenceState, currentChannel);
        if (currentChannelVolume != (ushort)channelVolume && currentChannelVolume == 0)
        {
            SetSequenceChannelVolume(ref sequenceState, currentChannel, 1);
        }

        var refreshedVoices = 0;
        var voiceCount = _gameEngine.StaticVariables.g_numberOfVoices;
        var programAttributes = vabHeader.ProgAttributes[programNumber];
        for (var voiceId = 0; voiceId < voiceCount; voiceId++)
        {
            ref var voiceSlot = ref _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId];
            if (voiceSlot.SequenceKey != (short)sequenceKey || voiceSlot.ProgramIndex != programNumber || voiceSlot.VabId != vabId)
            {
                continue;
            }

            var noteVelocity = unchecked((ushort)voiceSlot.field_0x08);
            var effectiveVelocity = (noteVelocity * channelVolume) / 0x7F;
            var baseVolume = (effectiveVelocity * (((int)vabHeader.Header.Mvol << 14) - vabHeader.Header.Mvol)) / 0x3F01;
            var toneAttributes = vabHeader.VagAttributes[programNumber][voiceSlot.ToneIndex];
            baseVolume = (baseVolume * programAttributes.Mvol * toneAttributes.Vol) / 0x3F01;

            var leftVolume = (baseVolume * sequenceState.field_0x74) / 0x7F;
            var rightVolume = (baseVolume * sequenceState.field_0x76) / 0x7F;

            var tonePan = toneAttributes.Pan;
            if (tonePan < 0x40)
            {
                rightVolume = (rightVolume * tonePan) / 0x3F;
            }
            else
            {
                leftVolume = (leftVolume * (0x7F - tonePan)) / 0x3F;
            }

            var programPan = vabHeader.ProgAttributes[voiceSlot.VabFirstToneIndex].Mpan;
            if (programPan < 0x40)
            {
                rightVolume = (rightVolume * programPan) / 0x3F;
            }
            else
            {
                leftVolume = (leftVolume * (0x7F - programPan)) / 0x3F;
            }

            if (orientation < 0x40)
            {
                rightVolume = (rightVolume * orientation) / 0x3F;
            }
            else
            {
                leftVolume = (leftVolume * (0x7F - orientation)) / 0x3F;
            }

            if (_gameEngine.StaticVariables.DAT_sound_801f7658 == 1)
            {
                if (leftVolume < rightVolume)
                {
                    leftVolume = rightVolume;
                }
                else
                {
                    rightVolume = leftVolume;
                }
            }

            leftVolume = (leftVolume * leftVolume) / 0x3FFF;
            rightVolume = (rightVolume * rightVolume) / 0x3FFF;
            _gameEngine.StaticVariables.g_spuVoiceVolumeLeft[voiceId] = (short)leftVolume;
            _gameEngine.StaticVariables.g_spuVoiceVolumeRight[voiceId] = (short)rightVolume;
            _gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] = (byte)(_gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] | 0x03);
            PushTrackedVoiceStereoVolume((short)voiceId);
            refreshedVoices++;
        }

        return refreshedVoices;
    }

    // JUSTIFICATION: C# language bridge only
    private static short ComposeSequenceKey(short seqId, short trackId)
    {
        return (short)(((trackId & 0xFF) << 8) | (seqId & 0xFF));
    }

    // JUSTIFICATION: C# language bridge only
    private static byte GetSequenceChannelMapping(ref SequenceTrackState sequenceState, int channel)
    {
        return channel switch
        {
            0 => sequenceState.Channel0,
            1 => sequenceState.Channel1,
            2 => sequenceState.Channel2,
            3 => sequenceState.Channel3,
            4 => sequenceState.Channel4,
            5 => sequenceState.Channel5,
            6 => sequenceState.Channel6,
            7 => sequenceState.Channel7,
            8 => sequenceState.Channel8,
            9 => sequenceState.Channel9,
            10 => sequenceState.Channel10,
            11 => sequenceState.Channel11,
            12 => sequenceState.Channel12,
            13 => sequenceState.Channel13,
            14 => sequenceState.Channel14,
            15 => sequenceState.Channel15,
            _ => 0,
        };
    }

    // JUSTIFICATION: C# language bridge only
    private static ushort GetSequenceChannelVolume(ref SequenceTrackState sequenceState, int channel)
    {
        return channel switch
        {
            0 => sequenceState.Volume0,
            1 => sequenceState.Volume1,
            2 => sequenceState.Volume2,
            3 => sequenceState.Volume3,
            4 => sequenceState.Volume4,
            5 => sequenceState.Volume5,
            6 => sequenceState.Volume6,
            7 => sequenceState.Volume7,
            8 => sequenceState.Volume8,
            9 => sequenceState.Volume9,
            10 => sequenceState.Volume10,
            11 => sequenceState.Volume11,
            12 => sequenceState.Volume12,
            13 => sequenceState.Volume13,
            14 => sequenceState.Volume14,
            15 => sequenceState.Volume15,
            _ => 0,
        };
    }

    // JUSTIFICATION: C# language bridge only
    private static byte GetSequenceChannelOrientation(ref SequenceTrackState sequenceState, int channel)
    {
        return channel switch
        {
            0 => sequenceState.Orientation0,
            1 => sequenceState.Orientation1,
            2 => sequenceState.Orientation2,
            3 => sequenceState.Orientation3,
            4 => sequenceState.Orientation4,
            5 => sequenceState.Orientation5,
            6 => sequenceState.Orientation6,
            7 => sequenceState.Orientation7,
            8 => sequenceState.Orientation8,
            9 => sequenceState.Orientation9,
            10 => sequenceState.Orientation10,
            11 => sequenceState.Orientation11,
            12 => sequenceState.Orientation12,
            13 => sequenceState.Orientation13,
            14 => sequenceState.Orientation14,
            15 => sequenceState.Orientation15,
            _ => 0x40,
        };
    }

    // JUSTIFICATION: C# language bridge only
    private static void SetSequenceChannelMapping(ref SequenceTrackState sequenceState, int channel, byte mappedChannel)
    {
        switch (channel)
        {
            case 0: sequenceState.Channel0 = mappedChannel; break;
            case 1: sequenceState.Channel1 = mappedChannel; break;
            case 2: sequenceState.Channel2 = mappedChannel; break;
            case 3: sequenceState.Channel3 = mappedChannel; break;
            case 4: sequenceState.Channel4 = mappedChannel; break;
            case 5: sequenceState.Channel5 = mappedChannel; break;
            case 6: sequenceState.Channel6 = mappedChannel; break;
            case 7: sequenceState.Channel7 = mappedChannel; break;
            case 8: sequenceState.Channel8 = mappedChannel; break;
            case 9: sequenceState.Channel9 = mappedChannel; break;
            case 10: sequenceState.Channel10 = mappedChannel; break;
            case 11: sequenceState.Channel11 = mappedChannel; break;
            case 12: sequenceState.Channel12 = mappedChannel; break;
            case 13: sequenceState.Channel13 = mappedChannel; break;
            case 14: sequenceState.Channel14 = mappedChannel; break;
            case 15: sequenceState.Channel15 = mappedChannel; break;
        }
    }

    // JUSTIFICATION: C# language bridge only
    private static void SetSequenceChannelVolume(ref SequenceTrackState sequenceState, int channel, byte volume)
    {
        switch (channel)
        {
            case 0: sequenceState.Volume0 = volume; break;
            case 1: sequenceState.Volume1 = volume; break;
            case 2: sequenceState.Volume2 = volume; break;
            case 3: sequenceState.Volume3 = volume; break;
            case 4: sequenceState.Volume4 = volume; break;
            case 5: sequenceState.Volume5 = volume; break;
            case 6: sequenceState.Volume6 = volume; break;
            case 7: sequenceState.Volume7 = volume; break;
            case 8: sequenceState.Volume8 = volume; break;
            case 9: sequenceState.Volume9 = volume; break;
            case 10: sequenceState.Volume10 = volume; break;
            case 11: sequenceState.Volume11 = volume; break;
            case 12: sequenceState.Volume12 = volume; break;
            case 13: sequenceState.Volume13 = volume; break;
            case 14: sequenceState.Volume14 = volume; break;
            case 15: sequenceState.Volume15 = volume; break;
        }
    }

    // JUSTIFICATION: C# language bridge only
    private static void SetSequenceChannelOrientation(ref SequenceTrackState sequenceState, int channel, byte orientation)
    {
        switch (channel)
        {
            case 0: sequenceState.Orientation0 = orientation; break;
            case 1: sequenceState.Orientation1 = orientation; break;
            case 2: sequenceState.Orientation2 = orientation; break;
            case 3: sequenceState.Orientation3 = orientation; break;
            case 4: sequenceState.Orientation4 = orientation; break;
            case 5: sequenceState.Orientation5 = orientation; break;
            case 6: sequenceState.Orientation6 = orientation; break;
            case 7: sequenceState.Orientation7 = orientation; break;
            case 8: sequenceState.Orientation8 = orientation; break;
            case 9: sequenceState.Orientation9 = orientation; break;
            case 10: sequenceState.Orientation10 = orientation; break;
            case 11: sequenceState.Orientation11 = orientation; break;
            case 12: sequenceState.Orientation12 = orientation; break;
            case 13: sequenceState.Orientation13 = orientation; break;
            case 14: sequenceState.Orientation14 = orientation; break;
            case 15: sequenceState.Orientation15 = orientation; break;
        }
    }

    // GHIDRA: FUN_8008EB5C @ 0x8008EB5C
    private void FUN_8008eb5c(short seqId, short trackId)
    {
        if ((uint)seqId >= _gameEngine.StaticVariables.g_sequenceStatePointers.Length || trackId != 0)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        var sequenceKey = (short)(((trackId & 0xFF) << 8) | (seqId & 0xFF));
        FUN_80093ef4(sequenceKey);
        sequenceState.field_0x2B = 0;
        sequenceState.Flags &= ~0x02u;
    }

    // GHIDRA: FUN_8008EC24 @ 0x8008EC24
    private void FUN_8008ec24(short seqId, short trackId)
    {
        if ((uint)seqId >= _gameEngine.StaticVariables.g_sequenceStatePointers.Length || trackId != 0)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        sequenceState.field_0x2B = 1;
        sequenceState.Flags &= ~0x08u;
    }

    // GHIDRA: FUN_8008E610 @ 0x8008E610
    private void FUN_8008e610(short seqId, short trackId)
    {
        // PARTIAL: control flow and mutated fields are ported; xrefs close +0x3E/+0x40/+0x42 as transition magnitude / working counter / signed step-divisor, but exact musical naming remains partial.
        if ((uint)seqId >= _gameEngine.StaticVariables.g_sequenceStatePointers.Length || trackId != 0)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        var sequenceKey = (short)(((trackId & 0xFF) << 8) | (seqId & 0xFF));
        var remainingTicks = unchecked((int)sequenceState.field_0x98) - 1;
        sequenceState.field_0x98 = unchecked((uint)remainingTicks);

        var step = sequenceState.field_0x42;
        if (step > 0)
        {
            if (remainingTicks % step == 0)
            {
                var field_0x40 = unchecked((short)(sequenceState.field_0x40 - 1));
                sequenceState.field_0x40 = field_0x40;

                if (field_0x40 < 0)
                {
                    FUN_80093c78(sequenceKey, 0x7F, 0x7F, 0);
                    sequenceState.Flags &= ~0x10u;
                }
                else
                {
                    FUN_80093de8(sequenceKey, out var volumeLeft, out var volumeRight);
                    if (field_0x40 > 0)
                    {
                        FUN_80093c78(sequenceKey, (short)(volumeLeft + 1), (short)(volumeRight + 1), 0);
                    }
                }
            }
        }
        else if (step < 0)
        {
            var field_0x40 = unchecked((short)(sequenceState.field_0x40 + step));
            sequenceState.field_0x40 = field_0x40;

            if (field_0x40 < 0)
            {
                FUN_80093c78(sequenceKey, 0x7F, 0x7F, 0);
                sequenceState.Flags &= ~0x10u;
            }
            else
            {
                FUN_80093de8(sequenceKey, out var volumeLeft, out var volumeRight);
                var nextVolumeLeft = volumeLeft - step;
                var nextVolumeRight = volumeRight - step;

                if (nextVolumeLeft >= 0x7F && nextVolumeRight >= 0x7F)
                {
                    FUN_80093c78(sequenceKey, 0x7F, 0x7F, 0);
                }
                else
                {
                    var elapsedTicks = unchecked((int)sequenceState.field_0x94) - unchecked((int)sequenceState.field_0x98);
                    var transitionDelta = elapsedTicks * -step;
                    if ((uint)transitionDelta < (uint)(ushort)sequenceState.field_0x3E)
                    {
                        FUN_80093c78(sequenceKey, (short)nextVolumeLeft, (short)nextVolumeRight, 0);
                    }
                }
            }
        }

        if (sequenceState.field_0x98 == 0 || sequenceState.field_0x40 == 0)
        {
            sequenceState.Flags &= ~0x10u;
        }

        FUN_80093de8(sequenceKey, out var currentLeft, out var currentRight);
        sequenceState.field_0x78 = currentLeft;
        sequenceState.field_0x7A = currentRight;
    }

    // GHIDRA: FUN_8008E8D0 @ 0x8008E8D0
    private void FUN_8008e8d0(short seqId, short trackId)
    {
        // PARTIAL: control flow and mutated fields are ported; xrefs close +0x3E/+0x40/+0x42 as transition magnitude / working counter / signed step-divisor, but exact musical naming remains partial.
        if ((uint)seqId >= _gameEngine.StaticVariables.g_sequenceStatePointers.Length || trackId != 0)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        var sequenceKey = (short)(((trackId & 0xFF) << 8) | (seqId & 0xFF));
        var remainingTicks = unchecked((int)sequenceState.field_0x98) - 1;
        sequenceState.field_0x98 = unchecked((uint)remainingTicks);

        var step = sequenceState.field_0x42;
        if (step > 0)
        {
            if (remainingTicks % step == 0)
            {
                var field_0x40 = unchecked((short)(sequenceState.field_0x40 - 1));
                sequenceState.field_0x40 = field_0x40;

                if (field_0x40 > 0)
                {
                    FUN_80093de8(sequenceKey, out var volumeLeft, out var volumeRight);
                    if ((volumeLeft - field_0x40) > 0 && (volumeRight - field_0x40) > 0 && volumeLeft != 1)
                    {
                        FUN_80093c78(sequenceKey, (short)(volumeLeft - 1), (short)(volumeRight - 1), 0);
                    }
                    else
                    {
                        FUN_80093c78(sequenceKey, 1, 1, 0);
                    }
                }
                else
                {
                    sequenceState.Flags &= ~0x20u;
                }
            }
        }
        else if (step < 0)
        {
            var field_0x40 = unchecked((short)(sequenceState.field_0x40 + step));
            sequenceState.field_0x40 = field_0x40;

            if (field_0x40 > 0)
            {
                FUN_80093de8(sequenceKey, out var volumeLeft, out var volumeRight);
                var elapsedTicks = unchecked((int)sequenceState.field_0x94) - unchecked((int)sequenceState.field_0x98);
                var transitionDelta = elapsedTicks * -step;

                if ((uint)transitionDelta >= (uint)(ushort)sequenceState.field_0x3E && -step < volumeLeft)
                {
                    FUN_80093c78(sequenceKey, (short)(volumeLeft + step), (short)(volumeRight + step), 0);
                }
                else
                {
                    FUN_80093c78(sequenceKey, 1, 1, 0);
                }
            }
            else
            {
                sequenceState.Flags &= ~0x20u;
            }
        }

        if (sequenceState.field_0x98 == 0 || sequenceState.field_0x40 == 0)
        {
            sequenceState.Flags &= ~0x20u;
        }

        FUN_80093de8(sequenceKey, out var currentLeft, out var currentRight);
        sequenceState.field_0x78 = currentLeft;
        sequenceState.field_0x7A = currentRight;
    }

    // GHIDRA: FUN_8008F4AC @ 0x8008F4AC
    private void FUN_8008f4ac(short seqId, short trackId)
    {
        if ((uint)seqId >= _gameEngine.StaticVariables.g_sequenceStatePointers.Length || trackId != 0)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        var remainingTicks = unchecked((int)sequenceState.field_0xA0) - 1;
        var step = sequenceState.field_0x44;
        sequenceState.field_0xA0 = unchecked((uint)remainingTicks);

        if (step > 0)
        {
            if (remainingTicks % step != 0)
            {
                return;
            }

            var currentValue = sequenceState.field_0x8C;
            var targetValue = sequenceState.field_0xA4;
            if (targetValue < currentValue)
            {
                currentValue--;
            }
            else if (currentValue < targetValue)
            {
                currentValue++;
            }

            sequenceState.field_0x8C = currentValue;
        }
        else
        {
            var currentValue = sequenceState.field_0x8C;
            var targetValue = sequenceState.field_0xA4;
            if (targetValue < currentValue)
            {
                currentValue = unchecked((uint)(currentValue + step));
                if (currentValue < targetValue)
                {
                    currentValue = targetValue;
                }
            }
            else if (currentValue < targetValue)
            {
                currentValue = unchecked((uint)(currentValue - step));
                if (targetValue < currentValue)
                {
                    currentValue = targetValue;
                }
            }

            sequenceState.field_0x8C = currentValue;
        }

        var tempoProduct = sequenceState.Tempo * unchecked((int)sequenceState.field_0x8C);
        var sequenceDriverRate = _gameEngine.StaticVariables.DAT_801f6ce0;
        var divisor = sequenceDriverRate * 60;
        if (divisor == 0)
        {
            Breakpoint.TriggerBreak();
            return;
        }

        var nextCurrentTempo = (tempoProduct * 10) / divisor;
        if (nextCurrentTempo <= 0)
        {
            nextCurrentTempo = 1;
        }

        sequenceState.CurrentTempo = (short)nextCurrentTempo;

        if (sequenceState.field_0xA0 == 0 || sequenceState.field_0x8C == sequenceState.field_0xA4)
        {
            sequenceState.Flags &= ~0x40u;
            sequenceState.Flags &= ~0x80u;
        }
    }

    // GHIDRA: FUN_80093DE8 @ 0x80093DE8
    private short FUN_80093de8(short sequenceKey, out ushort volumeLeft, out ushort volumeRight)
    {
        var seqId = sequenceKey & 0xFF;
        var trackId = (sequenceKey >> 8) & 0xFF;
        volumeLeft = 0;
        volumeRight = 0;

        if ((uint)seqId >= _gameEngine.StaticVariables.g_sequenceStatePointers.Length || trackId != 0)
        {
            return _gameEngine.StaticVariables.g_sequenceKey;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        _gameEngine.StaticVariables.g_sequenceKey = sequenceKey;
        volumeLeft = sequenceState.field_0x74;
        volumeRight = sequenceState.field_0x76;

        return _gameEngine.StaticVariables.g_sequenceKey;
    }

    // GHIDRA: FUN_80093EF4 @ 0x80093EF4
    private void FUN_80093ef4(short sequenceKey)
    {
        if (_gameEngine.StaticVariables.g_numberOfVoices == 0)
        {
            return;
        }

        var voiceCount = Math.Min(_gameEngine.StaticVariables.g_numberOfVoices, _gameEngine.StaticVariables.g_voiceRuntimeSlots.Length);
        for (var voiceIndex = 0; voiceIndex < voiceCount; voiceIndex++)
        {
            if (_gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceIndex].SequenceKey == sequenceKey)
            {
                _gameEngine.StaticVariables.DAT_maybeCurrentVoiceIndex_801f76b2 = (short)voiceIndex;
                FUN_80091134(0);
            }
        }
    }

    // GHIDRA: FUN_80091B1C @ 0x80091B1C
    // PARTIAL: raw note-off writes are closed; PTR_VOICE_00_LEFT_RIGHT_800c9794.field_0x194/0x196 now close as raw left/right voice command masks, but their downstream hardware consumption remains outside the desktop port.
    private void FUN_80091b1c(short voiceId)
    {
        if ((uint)voiceId >= (uint)_gameEngine.StaticVariables.g_voiceRuntimeSlots.Length)
        {
            return;
        }

        ref var voiceSlot = ref _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId];
        voiceSlot.NoiseState = 0;
        voiceSlot.CurrentPitch = 0;
        _gameEngine.StaticVariables.PTR_VOICE_00_LEFT_RIGHT_800c9794.field_0x194 = 0;
        _gameEngine.StaticVariables.PTR_VOICE_00_LEFT_RIGHT_800c9794.field_0x196 = 0;
    }

    private static void GetVoiceCommandMasks(short voiceId, out ushort leftMask, out ushort rightMask)
    {
        if ((ushort)voiceId < 0x10)
        {
            leftMask = (ushort)(1 << voiceId);
            rightMask = 0;
            return;
        }

        leftMask = 0;
        rightMask = (ushort)(1 << (voiceId - 0x10));
    }

    // GHIDRA: FUN_80091134 @ 0x80091134
    private void FUN_80091134(int param_1)
    {
        var voiceId = _gameEngine.StaticVariables.DAT_maybeCurrentVoiceIndex_801f76b2;
        if ((uint)voiceId >= (uint)_gameEngine.StaticVariables.g_voiceRuntimeSlots.Length)
        {
            return;
        }

        GetVoiceCommandMasks(voiceId, out var leftMask, out var rightMask);

        ref var voiceSlot = ref _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId];
        voiceSlot.NoiseState = 0;
        voiceSlot.CurrentPitch = 0;
        voiceSlot.field_0x00 = 0;

        _gameEngine.StaticVariables.g_voiceCommandPendingLeft = (short)(unchecked((ushort)_gameEngine.StaticVariables.g_voiceCommandPendingLeft) | leftMask);
        _gameEngine.StaticVariables.g_voiceCommandPendingRight = (short)(unchecked((ushort)_gameEngine.StaticVariables.g_voiceCommandPendingRight) | rightMask);
        _gameEngine.StaticVariables.g_voiceCommandPlayingLeft = (short)(unchecked((ushort)_gameEngine.StaticVariables.g_voiceCommandPlayingLeft) & unchecked((ushort)~leftMask));
        _gameEngine.StaticVariables.g_voiceCommandPlayingRight = (short)(unchecked((ushort)_gameEngine.StaticVariables.g_voiceCommandPlayingRight) & unchecked((ushort)~rightMask));
    }

    // GHIDRA: FUN_8008A718 @ 0x8008A718
    private void FUN_8008a718(int param_1)
    {
        _gameEngine.SoundBin.AdvanceTrackedVoices();

        // g_soundEffectState is decremented only by FUN_8004b674 (called from FinalizeAudioBuffers
        // each game frame). The PSX original FUN_8008A718 does not touch g_soundEffectState.

        SyncSoundEffectVoiceStates();
        FUN_8008e3d8();

        // PARTIAL: the original also pumps the low-level PsyQ/SPU driver callbacks.
    }

    public void AdvanceSoundFrame()
    {
        lock (_soundTickGate)
        {
            FUN_8008a718(0);
        }
    }

    // GHIDRA: FUN_8008F808 @ 0x8008F808
    private void FUN_8008f808(short param_1, int param_2, int param_3)
    {
        FUN_8008f760(param_1, 0, (short)param_2, param_3);
    }

    // GHIDRA: FUN_8008F690 @ 0x8008F690
    private void FUN_8008f690(short seqId, short trackId, short param_3, int param_4)
    {
        if ((uint)seqId >= _gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        if (trackId != 0)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        var flags = sequenceState.Flags;

        if ((flags & 0x04) != 0 || (flags & 0x100) != 0)
        {
            return;
        }

        if (param_3 == 0)
        {
            return;
        }

        sequenceState.field_0x3E = param_3;
        sequenceState.field_0x94 = unchecked((uint)param_4);
        sequenceState.field_0x40 = param_3;
        sequenceState.field_0x98 = unchecked((uint)param_4);

        var absoluteParam3 = param_3 < 0 ? -param_3 : param_3;
        if (param_4 == 0)
        {
            Breakpoint.TriggerBreak();
            return;
        }

        if ((uint)param_4 < (uint)absoluteParam3)
        {
            sequenceState.field_0x42 = (short)(-(absoluteParam3 / param_4));
        }
        else
        {
            sequenceState.field_0x42 = (short)(param_4 / absoluteParam3);
        }
    }

    // GHIDRA: FUN_8008F760 @ 0x8008F760
    private void FUN_8008f760(short seqId, short trackId, short param_3, int param_4)
    {
        FUN_8008f690(seqId, trackId, param_3, param_4);

        if ((uint)seqId >= _gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        if (trackId != 0)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        sequenceState.Flags |= 0x10;
        sequenceState.Flags &= ~0x20u;
    }

    // GHIDRA: FUN_8008D8D0 @ 0x8008D8D0
    private uint FUN_8008d8d0(short seqId, short trackId)
    {
        _ = trackId;

        if ((uint)seqId >= (uint)_loadedSequenceData.Length)
        {
            return 0;
        }

        var sequenceData = _loadedSequenceData[seqId];
        if (sequenceData == null)
        {
            return 0;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        if ((uint)sequenceState.SeqPosition >= (uint)sequenceData.Length)
        {
            sequenceState.Flags &= ~1u;
            return 0;
        }

        var instruction = sequenceData[sequenceState.SeqPosition++];
        uint instructionValue = instruction;
        if (instructionValue == 0)
        {
            return 0;
        }

        var delay = instructionValue << 2;
        if ((instruction & 0x80) != 0)
        {
            instructionValue &= 0x7Fu;
            do
            {
                if ((uint)sequenceState.SeqPosition >= (uint)sequenceData.Length)
                {
                    sequenceState.Flags &= ~1u;
                    return 0;
                }

                instruction = sequenceData[sequenceState.SeqPosition++];
                instructionValue = instructionValue * 0x80u + (uint)(instruction & 0x7F);
            } while ((instruction & 0x80) != 0);

            delay = instructionValue * 4;
        }

        delay = (delay + instructionValue) * 2;
        sequenceState.Playtime += delay;
        return delay;
    }

    // GHIDRA: FUN_8008C064 @ 0x8008C064
    // PARTIAL: note-off matching is closed; note-on now routes through FUN_800934B8 with a desktop playback bridge for audible output.
    private void FUN_8008c064(short seqId, short trackId, byte data1, byte data2)
    {
        if ((uint)seqId >= (uint)_gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        var channel = sequenceState.CurrentChannel;
        if ((((sequenceState.field_0xAA >> (channel & 0x1F)) & 1) != 0) || sequenceState.field_0x74 == 0)
        {
            return;
        }

        var programNumber = GetSequenceChannelMapping(ref sequenceState, channel);
        if (data2 == 0)
        {
            FUN_80093a04(ComposeSequenceKey(seqId, trackId), sequenceState.Vab, programNumber, data1);
        }
        else
        {
            FUN_800934b8((uint)(ushort)ComposeSequenceKey(seqId, trackId), (ushort)sequenceState.Vab, programNumber,
                data1, data2, GetSequenceChannelOrientation(ref sequenceState, channel));
            sequenceState.field_0xA8 = data2;
        }
    }

    // GHIDRA: FUN_8008D4C0 @ 0x8008D4C0
    private void FUN_8008d4c0(short seqId, short trackId)
    {
        if ((uint)seqId >= (uint)_gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        var sequenceData = _loadedSequenceData[seqId];
        if (sequenceData == null || (uint)sequenceState.SeqPosition >= (uint)sequenceData.Length)
        {
            sequenceState.Flags &= ~1u;
            return;
        }

        var programNumber = GetSequenceChannelMapping(ref sequenceState, sequenceState.CurrentChannel);
        var value = sequenceData[sequenceState.SeqPosition++];
        FUN_80093030(ComposeSequenceKey(seqId, trackId), sequenceState.Vab, programNumber, value);
        sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
    }

    // GHIDRA: FUN_8008C144 @ 0x8008C144
    private void FUN_8008c144(short seqId, short trackId, byte programNumber)
    {
        if ((uint)seqId >= (uint)_gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        SetSequenceChannelMapping(ref sequenceState, sequenceState.CurrentChannel, (byte)(programNumber & 0x7F));
        sequenceState.Delay = FUN_8008d8d0(seqId, trackId);
    }

    // GHIDRA: FUN_80093A04 @ 0x80093A04
    // PARTIAL: raw note-off cleanup follows FUN_80091B1C/FUN_80091134; pending SPU key-off masks are consumed by a desktop release adapter.
    private int FUN_80093a04(short sequenceKey, short vabId, short programNumber, uint note)
    {
        var stoppedVoices = 0;
        var voiceCount = _gameEngine.StaticVariables.g_numberOfVoices;
        for (var voiceId = 0; voiceId < voiceCount; voiceId++)
        {
            ref var voiceSlot = ref _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId];
            if (voiceSlot.Note != (short)(note & 0xFFFF) ||
                voiceSlot.ProgramIndex != programNumber ||
                voiceSlot.SequenceKey != sequenceKey ||
                voiceSlot.VabId != vabId)
            {
                continue;
            }

            if (voiceSlot.field_0x00 == 0xFF)
            {
                FUN_80091b1c((short)voiceId);
            }
            else
            {
                _gameEngine.StaticVariables.DAT_maybeCurrentVoiceIndex_801f76b2 = (short)voiceId;
                FUN_80091134(0);
            }

            stoppedVoices++;
        }

        return stoppedVoices;
    }

    // GHIDRA: FUN_80093030 @ 0x80093030
    private short FUN_80093030(short sequenceKey, short vabId, short programNumber, byte value)
    {
        SelectLoadedVabProgram(vabId, programNumber);
        _gameEngine.StaticVariables.g_sequenceKey = sequenceKey;

        short updatedVoices = 0;
        var voiceCount = Math.Min(_gameEngine.StaticVariables.g_numberOfVoices, _gameEngine.StaticVariables.g_voiceRuntimeSlots.Length);
        for (short voiceId = 0; voiceId < voiceCount; voiceId++)
        {
            updatedVoices = unchecked((short)(updatedVoices + FUN_80092e04(voiceId, sequenceKey, vabId, programNumber, value)));
        }

        return updatedVoices;
    }

    // GHIDRA: FUN_80092E04 @ 0x80092E04
    private short FUN_80092e04(short voiceId, short sequenceKey, short vabId, short programNumber, byte value)
    {
        if ((uint)voiceId >= (uint)_gameEngine.StaticVariables.g_voiceRuntimeSlots.Length)
        {
            return 0;
        }

        ref var voiceSlot = ref _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId];
        if (voiceSlot.SequenceKey != sequenceKey
            || voiceSlot.VabId != vabId
            || voiceSlot.ProgramIndex != programNumber)
        {
            return 0;
        }

        if (FUN_80090370(vabId, programNumber, voiceSlot.ToneIndex, out var toneAttributes) != 0)
        {
            return 0;
        }

        var note = voiceSlot.Note;
        var fine = 0;
        var pitchBendDelta = value - 0x40;
        if (pitchBendDelta > 0)
        {
            var product = pitchBendDelta * toneAttributes.PitchBendMax;
            note = unchecked((short)(note + (product / 0x3F)));
            fine = (product % 0x3F) << 1;
        }
        else if (pitchBendDelta < 0)
        {
            var product = pitchBendDelta * toneAttributes.PitchBendMin;
            var quotientSource = product;
            if (quotientSource < 0)
            {
                quotientSource += 0x3F;
            }

            var quotient = quotientSource >> 6;
            note = unchecked((short)(note + quotient - 1));

            var remainderBase = product;
            if (remainderBase < 0)
            {
                remainderBase += 0x3F;
            }

            var remainderQuotient = remainderBase >> 6;
            fine = ((product - (remainderQuotient << 6)) << 1) + 0x7F;
        }

        _gameEngine.StaticVariables.DAT_maybeCurrentVoiceIndex_801f76b2 = voiceId;
        _gameEngine.StaticVariables.DAT_801f76a4 = unchecked((byte)voiceSlot.ToneIndex);
        var pitch = CalculateVoicePitch(note, unchecked((short)fine));
        _gameEngine.StaticVariables.g_spuVoicePitch[voiceId] = pitch;
        _gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] = (byte)(_gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] | 0x04);
        return 1;
    }

    // GHIDRA: FUN_8008B8C8 @ 0x8008B8C8
    private short FUN_8008b8c8(short sequenceSlot, short vabId, byte[] sequenceData)
    {
        if ((uint)sequenceSlot >= _gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return -1;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[sequenceSlot];
        var sequenceIndex = 0;

        sequenceState.Vab = vabId;
        sequenceState.Tempo = 0;
        sequenceState.field_0x13 = 0;
        sequenceState.field_0x14 = 0;
        sequenceState.field_0x29 = 0;
        sequenceState.field_0x16 = 0;
        sequenceState.field_0x2A = 0;
        sequenceState.CurrentChannel = 0;
        sequenceState.field_0x7C = 0;
        sequenceState.Playtime = 0;
        sequenceState.field_0x84 = 0;
        sequenceState.field_0x72 = 0;
        sequenceState.TimesPlayed = 0;
        sequenceState.field_0x2B = 0;
        sequenceState.Delay = 0;
        sequenceState.field_0x27 = 0;
        sequenceState.Loops = 0;
        sequenceState.field_0x10 = 0;
        sequenceState.MessageType = 0;
        sequenceState.field_0xA8 = 0x7F;
        sequenceState.field_0xAA = 0;

        sequenceState.Channel0 = 0;
        sequenceState.Channel1 = 1;
        sequenceState.Channel2 = 2;
        sequenceState.Channel3 = 3;
        sequenceState.Channel4 = 4;
        sequenceState.Channel5 = 5;
        sequenceState.Channel6 = 6;
        sequenceState.Channel7 = 7;
        sequenceState.Channel8 = 8;
        sequenceState.Channel9 = 9;
        sequenceState.Channel10 = 10;
        sequenceState.Channel11 = 11;
        sequenceState.Channel12 = 12;
        sequenceState.Channel13 = 13;
        sequenceState.Channel14 = 14;
        sequenceState.Channel15 = 15;
        sequenceState.Orientation0 = 0x40;
        sequenceState.Orientation1 = 0x40;
        sequenceState.Orientation2 = 0x40;
        sequenceState.Orientation3 = 0x40;
        sequenceState.Orientation4 = 0x40;
        sequenceState.Orientation5 = 0x40;
        sequenceState.Orientation6 = 0x40;
        sequenceState.Orientation7 = 0x40;
        sequenceState.Orientation8 = 0x40;
        sequenceState.Orientation9 = 0x40;
        sequenceState.Orientation10 = 0x40;
        sequenceState.Orientation11 = 0x40;
        sequenceState.Orientation12 = 0x40;
        sequenceState.Orientation13 = 0x40;
        sequenceState.Orientation14 = 0x40;
        sequenceState.Orientation15 = 0x40;
        sequenceState.Volume0 = 0x7F;
        sequenceState.Volume1 = 0x7F;
        sequenceState.Volume2 = 0x7F;
        sequenceState.Volume3 = 0x7F;
        sequenceState.Volume4 = 0x7F;
        sequenceState.Volume5 = 0x7F;
        sequenceState.Volume6 = 0x7F;
        sequenceState.Volume7 = 0x7F;
        sequenceState.Volume8 = 0x7F;
        sequenceState.Volume9 = 0x7F;
        sequenceState.Volume10 = 0x7F;
        sequenceState.Volume11 = 0x7F;
        sequenceState.Volume12 = 0x7F;
        sequenceState.Volume13 = 0x7F;
        sequenceState.Volume14 = 0x7F;
        sequenceState.Volume15 = 0x7F;

        for (var channelIndex = 0; channelIndex < 16; channelIndex++)
        {
            _sequenceChannelPrograms[sequenceSlot, channelIndex] = (byte)channelIndex;
            _sequenceChannelPitchBends[sequenceSlot, channelIndex] = 0x2000;
        }

        sequenceState.PreDelay = 1;
        sequenceState.SeqPosition = sequenceIndex;

        if (sequenceData.Length == 0)
        {
            return -1;
        }

        var firstByte = sequenceData[0];
        if (firstByte == (byte)'S' || firstByte == (byte)'p')
        {
            sequenceIndex = 8;
            sequenceState.SeqPosition = sequenceIndex;

            if (sequenceData.Length <= 7 || sequenceData[7] != 1)
            {
                Debug.WriteLine("LoadSeq: unsupported sequence header version");
                return -1;
            }
        }
        else
        {
            Debug.WriteLine("LoadSeq: unsupported sequence header signature");
            return -1;
        }

        if (sequenceData.Length < 13)
        {
            return -1;
        }

        sequenceState.SeqPosition = 9;
        // The SEQ header resolution (ticks per quarter note) is stored big-endian like the
        // rest of the pQES header; reading it little-endian turned 0x01E0 (480) into 0xE001
        // (57345) and made the sequencer consume ~119x too many ticks per frame.
        sequenceState.Tempo = (ushort)((sequenceData[8] << 8) | sequenceData[9]);
        if (sequenceState.Tempo == 0)
        {
            return -1;
        }

        sequenceIndex = 13;
        sequenceState.SeqPosition = sequenceIndex;

        var timingValue = (sequenceData[10] << 16) | (sequenceData[11] << 8) | sequenceData[12];
        if (timingValue == 0)
        {
            return -1;
        }

        var roundedTempo = 0x03938700 / timingValue;
        if ((0x03938700 % timingValue) > (timingValue >> 1))
        {
            roundedTempo++;
        }

        if (roundedTempo == 0)
        {
            return -1;
        }

        sequenceState.field_0x84 = unchecked((uint)roundedTempo);
        sequenceState.field_0x8C = unchecked((uint)roundedTempo);

        sequenceState.SeqPosition = sequenceIndex + 2;

        var initialDelay = FUN_8008d8d0(sequenceSlot, 0);

        var tempoProduct = sequenceState.Tempo * roundedTempo;
        if (tempoProduct == 0)
        {
            return -1;
        }

        sequenceState.field_0x7C = initialDelay;
        sequenceState.Delay = initialDelay;
        sequenceState.SeqLoopPos = sequenceState.SeqPosition;
        sequenceState.SeqStartPos = sequenceState.SeqPosition;

        var sequenceDriverRate = _gameEngine.StaticVariables.DAT_801f6ce0;
        if (sequenceDriverRate == 0)
        {
            return -1;
        }

        var driverScaled = sequenceDriverRate * 60;
        var tempoScaled = tempoProduct * 10;

        if ((uint)tempoScaled < (uint)driverScaled)
        {
            var preDelay = (short)((sequenceDriverRate * 600) / tempoProduct);
            sequenceState.PreDelay = preDelay;
            sequenceState.CurrentTempo = preDelay;
        }
        else
        {
            var playbackStep = tempoScaled / driverScaled;
            var playbackRemainder = tempoScaled % driverScaled;
            sequenceState.PreDelay = -1;

            if (playbackRemainder > (sequenceDriverRate * 30))
            {
                playbackStep++;
            }

            sequenceState.CurrentTempo = (short)playbackStep;
        }

        sequenceState.field_0x72 = (ushort)sequenceState.CurrentTempo;

        return 0;
    }

    // GHIDRA: LoadSeq @ 0x8008BC00
    public short LoadSeq(byte[] sequenceData, short vabId)
    {
        if (_gameEngine.StaticVariables.g_sequenceSlotMask == -1)
        {
            Debug.WriteLine("LoadSeq: no free sequence slot");
            return -1;
        }

        short sequenceSlot = 0;
    while (sequenceSlot < _gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            if ((_gameEngine.StaticVariables.g_sequenceSlotMask & (1 << sequenceSlot)) == 0)
            {
                break;
            }

            sequenceSlot++;
        }

        if ((uint)sequenceSlot >= _gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return -1;
        }

        _gameEngine.StaticVariables.g_sequenceSlotMask |= 1 << sequenceSlot;

        byte[] storedSequenceData;
        if (sequenceData.Length > 0)
        {
            storedSequenceData = new byte[sequenceData.Length];
            Array.Copy(sequenceData, storedSequenceData, sequenceData.Length);
        }
        else
        {
            storedSequenceData = Array.Empty<byte>();
        }

        _loadedSequenceData[sequenceSlot] = storedSequenceData;

        var initResult = FUN_8008b8c8(sequenceSlot, vabId, storedSequenceData);
        if (initResult != -1)
        {
            return sequenceSlot;
        }

        _loadedSequenceData[sequenceSlot] = null;
        _gameEngine.StaticVariables.g_sequenceSlotMask &= ~(1 << sequenceSlot);

        return -1;
    }

    // GHIDRA: SetSeqVolume @ 0x8008F23C
    private void SetSeqVolume(short seqId, short volumeLeft, short volumeRight)
    {
        FUN_8008f1f8(seqId, 0, volumeLeft, volumeRight);
    }

    // GHIDRA: StartSequencePlayback @ 0x8008F088
    private void StartSequencePlayback(short seqId, short trackId, byte param_3, short param_4)
    {
        if ((uint)seqId >= _gameEngine.StaticVariables.g_sequenceStatePointers.Length)
        {
            return;
        }

        if (trackId != 0)
        {
            return;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
        sequenceState.Flags &= ~0x200u;
        sequenceState.Flags &= ~0x04u;
        sequenceState.LoopCount = param_4;

        if (param_3 == 1)
        {
            sequenceState.Flags |= 0x01;
            var sequenceKey = (short)(((trackId & 0xFF) << 8) | (seqId & 0xFF));
            var volumeLeft = sequenceState.field_0x74;
            var volumeRight = sequenceState.field_0x76;
            sequenceState.TimesPlayed = 0;
            sequenceState.field_0x2B = 1;
            FUN_80093c78(sequenceKey, (short)volumeLeft, (short)volumeRight, 0);
        }
        else if (param_3 == 0)
        {
            sequenceState.Flags |= 0x02;
        }
    }

    // GHIDRA: PlaySeq @ 0x8008F188
    private void PlaySeq(short seqId, byte param_2, short param_3)
    {
        StartSequencePlayback(seqId, 0, param_2, param_3);
    }

    // GHIDRA: FUN_8008F1F8 @ 0x8008F1F8
    private void FUN_8008f1f8(short seqId, short trackId, short volumeLeft, short volumeRight)
    {
        var sequenceKey = (short)(((trackId & 0xFF) << 8) | (seqId & 0xFF));
        FUN_80093c78(sequenceKey, volumeLeft, volumeRight, 0);
    }

    // GHIDRA: UpdateSequenceVolumeBalance @ 0x80093C78
    private short FUN_80093c78(short sequenceKey, short volumeLeft, short volumeRight, short updateVoiceVolumes)
    {
        var seqId = sequenceKey & 0xFF;
        var trackId = (sequenceKey >> 8) & 0xFF;

        if ((uint)seqId >= _gameEngine.StaticVariables.g_sequenceStatePointers.Length || trackId != 0)
        {
            return _gameEngine.StaticVariables.g_sequenceKey;
        }

        ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];

        _gameEngine.StaticVariables.g_sequenceKey = sequenceKey;

        var rawVolumeLeft = (ushort)volumeLeft;
        var rawVolumeRight = (ushort)volumeRight;
        sequenceState.field_0x74 = rawVolumeLeft;
        if (sequenceState.field_0x74 >= 0x80)
        {
            sequenceState.field_0x74 = 0x7F;
        }

        sequenceState.field_0x76 = rawVolumeRight;
        if (sequenceState.field_0x76 >= 0x80)
        {
            sequenceState.field_0x76 = 0x7F;
        }

        if (updateVoiceVolumes == 1 && _gameEngine.StaticVariables.g_numberOfVoices != 0)
        {
            var spuVolumeLeft = (short)(rawVolumeLeft * 0x81);
            var spuVolumeRight = (short)(rawVolumeRight * 0x81);
            var voiceCount = Math.Min(_gameEngine.StaticVariables.g_numberOfVoices, _gameEngine.StaticVariables.g_voiceRuntimeSlots.Length);

            for (var voiceIndex = 0; voiceIndex < voiceCount; voiceIndex++)
            {
                if ((ushort)_gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceIndex].SequenceKey == (ushort)sequenceKey)
                {
                    _gameEngine.StaticVariables.g_spuVoiceVolumeLeft[voiceIndex] = spuVolumeLeft;
                    _gameEngine.StaticVariables.g_spuVoiceVolumeRight[voiceIndex] = spuVolumeRight;
                    _gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceIndex] |= 0x03;
                    PushTrackedVoiceStereoVolume((short)voiceIndex);
                }
            }
        }

        return _gameEngine.StaticVariables.g_sequenceKey;
    }

    // GHIDRA: LoadVabHeader @ 0x8008FAAC
    private short LoadVabHeader(byte[] vabHeaderBuffer, short requestedVabId, int uploadBase)
    {
        // PARTIAL: desktop bridge reserves a runtime VAB slot, resets body state, parses the managed header/program/tone data, and marks it body-loading,
        // but it still does not mirror the raw PSX pointer-table population from LoadVabHeaderCore (header/program/tone/sample/SPU-allocation pointers).
        short vabId = requestedVabId;

        if (vabId < 0 || vabId >= 0x10 || _gameEngine.StaticVariables.g_loadedVabState[vabId] != 0)
        {
            vabId = 0;
            while (vabId < 0x10 && _gameEngine.StaticVariables.g_loadedVabState[vabId] != 0)
            {
                vabId++;
            }

            if (vabId >= 0x10)
            {
                return -1;
            }
        }

        if (_gameEngine.StaticVariables.g_loadedVabState[vabId] == 0)
        {
            _gameEngine.StaticVariables.g_loadedVabCount++;
        }

        _loadedVabBodies[vabId] = null;
        _loadedVabBodyWriteOffsets[vabId] = 0;
        _gameEngine.StaticVariables.g_loadedVabBodySizes[vabId] = 0;

        using (var headerReader = new BinaryReader(new MemoryStream(vabHeaderBuffer, writable: false)))
        {
            _loadedVabHeaders[vabId] = new SoundBin.VabHeader(headerReader);
        }

        var loadedHeader = _loadedVabHeaders[vabId];
        if (loadedHeader != null)
        {
            var programCount = Math.Min(loadedHeader.Header.Ps, loadedHeader.ProgAttributes.Length);
            for (var programIndex = 0; programIndex < programCount; programIndex++)
            {
                // PARTIAL: LoadVabHeaderCore materializes programAttr+0x08 as the first tone block; C# stores tones as [program][tone].
                loadedHeader.ProgAttributes[programIndex].Reserved1 = (loadedHeader.ProgAttributes[programIndex].Reserved1 & unchecked((int)0xFFFFFF00)) | programIndex;
            }
        }

        _gameEngine.StaticVariables.g_loadedVabState[vabId] = 2;
        return vabId;
    }

    // GHIDRA: UploadVabBodyChunk @ 0x8008FFC0
    private short UploadVabBodyChunk(byte[] vabBodyBuffer, int size, short vabId)
    {
        if ((uint)vabId >= 0x10)
        {
            return -1;
        }

        var loadedVabBody = _loadedVabBodies[vabId];
        var totalSize = _gameEngine.StaticVariables.g_loadedVabBodySizes[vabId];
        if (loadedVabBody == null || totalSize <= 0)
        {
            return -1;
        }

        var writeOffset = _loadedVabBodyWriteOffsets[vabId];
        if (writeOffset < 0 || writeOffset + size > totalSize || writeOffset + size > loadedVabBody.Length)
        {
            return -1;
        }

        Array.Copy(vabBodyBuffer, 0, loadedVabBody, writeOffset, size);
        _loadedVabBodyWriteOffsets[vabId] = writeOffset + size;

        if (_loadedVabBodyWriteOffsets[vabId] >= totalSize)
        {
            _gameEngine.StaticVariables.g_loadedVabState[vabId] = 1;
        }

        return 0;
    }

    // GHIDRA: StopAllSound @ 0x80049AF4
    public void StopAllSound()
    {
        lock (_soundTickGate)
        {
            StopAllSoundCore();
        }
    }

    private void StopAllSoundCore()
    {
        if (-1 < _gameEngine.StaticVariables.g_currentMapSoundIndex)
        {
            if (_gameEngine.StaticVariables.g_soundEffectState != 0)
            {
                for (short voiceId = 0; voiceId < _gameEngine.StaticVariables.g_voiceState.Length; voiceId++)
                {
                    StopVoice(voiceId);
                }

                _gameEngine.StaticVariables.g_soundEffectState = 0;
            }

            FUN_8008b878(0x7f, 0x7f);
            SetSeqVolume(_gameEngine.StaticVariables.g_requestedSeqId, 0x7f, 0x7f);
            if (_gameEngine.StaticVariables.IsBgmActivated)
            {
                PlaySeq(_gameEngine.StaticVariables.g_requestedSeqId, 1, 1);
            }
        }
    }

    // GHIDRA: FUN_8008B878 @ 0x8008B878
    private void FUN_8008b878(int volumeLeft, int volumeRight)
    {
        _gameEngine.SoundBin.SetMasterVolume(volumeLeft, volumeRight);
    }

    // GHIDRA: FUN_8004B674 @ 0x8004B674
    private void FUN_8004b674()
    {
        if (_gameEngine.StaticVariables.g_soundEffectState == 0)
        {
            return;
        }

        var iVar2 = _gameEngine.StaticVariables.g_soundEffectState - 1;

        if (iVar2 == 3)
        {
            _gameEngine.StaticVariables.g_soundEffectState = iVar2;
            FUN_8008b878(0x7f, 0x7f);
            return;
        }

        if (iVar2 == 0x3c)
        {
            _gameEngine.StaticVariables.g_soundEffectState = iVar2;
            FUN_8008b878(0, 0);
            InitializeBgm(_gameEngine.StaticVariables.g_requestedSeqId);
            // JUSTIFICATION: PSX hardware adaptation only
            // RELATION: SpuSetKey(0, 0xffffff) key-off all 24 voices
            for (short voiceId = 0; voiceId < 0x18; voiceId++)
            {
                StopVoice(voiceId);
            }
            return;
        }

        if (iVar2 < 0x3d)
        {
            _gameEngine.StaticVariables.g_soundEffectState = iVar2;
            return;
        }

        var sVar1 = (short)((_gameEngine.StaticVariables.g_soundEffectState - 0x3d) * 0x7f / 0x3c);
        _gameEngine.StaticVariables.g_soundEffectState = iVar2;
        FUN_8008b878(sVar1, sVar1);
    }

    // GHIDRA: FinalizeAudioBuffers @ 0x80048CD4
    private void FinalizeAudioBuffers()
    {
        SyncSoundEffectVoiceStates();
        FUN_8004b674();

        for (var i = 0x3f; i >= 0; i--)
        {
            _gameEngine.StaticVariables.INT_ARRAY_80165028[i] = 0;
        }

        EnsureSoundEffectDataInitialized();
        for (var sfxId = 0; sfxId < _gameEngine.StaticVariables.g_soundEffectData.Length; sfxId++)
        {
            ref var soundEffectRecord = ref _gameEngine.StaticVariables.g_soundEffectData[sfxId];

            if (soundEffectRecord.SeqNum < 0 || (soundEffectRecord.Flags & 0x0002) == 0)
            {
                continue;
            }

            if ((uint)soundEffectRecord.SeqNum >= (uint)_gameEngine.StaticVariables.g_loadedSequenceHandles.Length)
            {
                continue;
            }

            var sequenceSlot = _gameEngine.StaticVariables.g_loadedSequenceHandles[soundEffectRecord.SeqNum];
            if (sequenceSlot < 0 || FUN_8008dd1c(sequenceSlot, 0) != 0)
            {
                continue;
            }

            InitializeBgm(sequenceSlot);
            ResetSomethingSound(sequenceSlot);
            soundEffectRecord.Flags = (short)(soundEffectRecord.Flags & ~0x0002);
        }
    }

    // GHIDRA: IsSoundEffectAlreadyPlaying @ 0x80048DF4
    private int IsSoundEffectAlreadyPlaying(uint sfxId)
    {
        for (var index = 0; index < _gameEngine.StaticVariables.INT_ARRAY_80165028.Length; index++)
        {
            var currentSfxId = _gameEngine.StaticVariables.INT_ARRAY_80165028[index];
            if (currentSfxId == sfxId)
            {
                return 1;
            }

            if (currentSfxId == 0)
            {
                _gameEngine.StaticVariables.INT_ARRAY_80165028[index] = (int)sfxId;
                return 0;
            }
        }

        return 0;
    }

    // GHIDRA: PlaySoundEffect @ 0x800490FC
    public void PlaySoundEffect(uint sfxId)
    {
        lock (_soundTickGate)
        {
            PlaySoundEffectCore(sfxId);
        }
    }

    private void PlaySoundEffectCore(uint sfxId)
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

        if (IsSoundEffectAlreadyPlaying(sfxId) != 0)
        {
            return;
        }

        EnsureSoundEffectDataInitialized();

        if ((sfxId & 0x100) != 0)
        {
            //sfxId = (sfxId & 0x0FF) - 43;
        }

        if (sfxId >= (uint)_gameEngine.StaticVariables.g_soundEffectData.Length)
        {
            return;
        }

        if (!TryResolveSoundEffectRecord((int)sfxId, out var resolvedSfxId, out var loadedVabId))
        {
            return;
        }

        ref var soundEffectRecord = ref _gameEngine.StaticVariables.g_soundEffectData[resolvedSfxId];

        SyncSoundEffectVoiceStates();

        if (TryPlaySoundEffectSequence(ref soundEffectRecord, loadedVabId))
        {
            return;
        }

        if (CountActiveVoicesForSfx((int)sfxId) >= soundEffectRecord.MaxVoices)
        {
            return;
        }

        if (!TryPlayDirectSoundEffectVoices((int)sfxId, ref soundEffectRecord, loadedVabId))
        {
            return;
        }

        //if ((sfxId & 0x100) != 0)
        //{
        //    _gameEngine.SoundBin.PlayMapSfx(((int)sfxId & 0x0FF) - 43, 11025, false, out _, out _, out _);
        //}
        //else
        //{
        //    _gameEngine.SoundBin.PlaySfx((int)sfxId, 11025, false, out _, out _, out _);
        //}
    }

    private void SyncSoundEffectVoiceStates()
    {
        // PARTIAL: backend voice snapshot is SoundBin.VoicesAreActive; raw slot cleanup is mirrored here when a tracked voice ends, but the full SPU status/history snapshot is not ported.
        for (var voiceIndex = 0; voiceIndex < 0x18; voiceIndex++)
        {
            if (_gameEngine.SoundBin.VoicesAreActive[voiceIndex] == 0 &&
                _gameEngine.StaticVariables.g_voiceState[voiceIndex] != 0)
            {
                _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceIndex].NoiseState = 0;
                _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceIndex].CurrentPitch = 0;
                _gameEngine.StaticVariables.g_voiceState[voiceIndex] = 0;
                _gameEngine.StaticVariables.g_voiceSfxId[voiceIndex] = 0;
                _gameEngine.StaticVariables.g_voiceVabId[voiceIndex] = -2;
                _gameEngine.StaticVariables.g_voiceToneIndex[voiceIndex] = 0;
                _gameEngine.StaticVariables.g_voiceToneVolume[voiceIndex] = 0;
                _gameEngine.StaticVariables.g_voiceTonePan[voiceIndex] = 0;
            }
        }
    }

    private bool TryPlayDirectSoundEffectVoices(int requestedSfxId, ref SoundEffectRecord soundEffectRecord, short loadedVabId)
    {
        if (loadedVabId < 0 || soundEffectRecord.ToneCount <= 0)
        {
            return false;
        }

        var playedAnyVoice = false;
        for (var toneOffset = 0; toneOffset < soundEffectRecord.ToneCount; toneOffset++)
        {
            var toneIndex = unchecked((short)(soundEffectRecord.ToneNumber + toneOffset));
            var voiceId = TriggerVoice(loadedVabId, soundEffectRecord.ProgramNumber, toneIndex, soundEffectRecord.Note, 0, 0x7f, 0x7f);
            if (voiceId < 0)
            {
                continue;
            }

            if (!TryPlayLoadedVabToneVoice(voiceId, loadedVabId, soundEffectRecord.ProgramNumber, toneIndex, soundEffectRecord.Note))
            {
                StopVoice(voiceId);
                continue;
            }

            RegisterTriggeredSoundEffectVoice(voiceId, requestedSfxId, ref soundEffectRecord, loadedVabId, toneIndex);
            playedAnyVoice = true;
        }

        return playedAnyVoice;
    }

    private void RegisterTriggeredSoundEffectVoice(short voiceId, int requestedSfxId, ref SoundEffectRecord soundEffectRecord, short loadedVabId, short toneIndex)
    {
        soundEffectRecord.Flags = (short)(soundEffectRecord.Flags | 0x0001);
        _gameEngine.StaticVariables.g_voiceState[voiceId] = 0x80;
        _gameEngine.StaticVariables.g_voiceSfxId[voiceId] = requestedSfxId;
        _gameEngine.StaticVariables.g_voiceVabId[voiceId] = soundEffectRecord.VabId;
        _gameEngine.StaticVariables.g_voiceToneIndex[voiceId] = toneIndex;

        if (FUN_80090370(loadedVabId, soundEffectRecord.ProgramNumber, toneIndex, out var toneAttributes) >= 0)
        {
            _gameEngine.StaticVariables.g_voiceToneVolume[voiceId] = toneAttributes.Volume;
            _gameEngine.StaticVariables.g_voiceTonePan[voiceId] = toneAttributes.Pan;
        }
        else
        {
            _gameEngine.StaticVariables.g_voiceToneVolume[voiceId] = 0;
            _gameEngine.StaticVariables.g_voiceTonePan[voiceId] = 0;
        }

        if (TryGetLoadedVabHeader(loadedVabId, out var loadedVabHeader))
        {
            var loadedVabBody = _loadedVabBodies[loadedVabId];
            if (loadedVabBody != null)
            {
                _gameEngine.SoundBin.TrackVoicePlayback(soundEffectRecord.VabId != -1, voiceId, soundEffectRecord.ProgramNumber, toneIndex, soundEffectRecord.Note, loadedVabHeader, loadedVabBody);
                PushTrackedVoiceStereoVolume(voiceId);
                return;
            }
        }

        _gameEngine.SoundBin.TrackVoicePlayback(soundEffectRecord.VabId != -1, voiceId, soundEffectRecord.ProgramNumber, toneIndex, soundEffectRecord.Note);
        PushTrackedVoiceStereoVolume(voiceId);
    }

    // GHIDRA: CountActiveVoicesForSfx @ 0x80049060
    private int CountActiveVoicesForSfx(int sfxId)
    {
        EnsureSoundEffectDataInitialized();

        if ((uint)sfxId >= (uint)_gameEngine.StaticVariables.g_soundEffectData.Length)
        {
            return 0;
        }

        var soundEffectRecord = _gameEngine.StaticVariables.g_soundEffectData[sfxId];
        var activeVoiceCount = 0;

        for (var voiceIndex = 0; voiceIndex < 0x18; voiceIndex++)
        {
            if (_gameEngine.StaticVariables.g_voiceState[voiceIndex] == 0)
            {
                continue;
            }

            if (_gameEngine.StaticVariables.g_voiceSfxId[voiceIndex] != sfxId)
            {
                continue;
            }

            if (_gameEngine.StaticVariables.g_voiceVabId[voiceIndex] != soundEffectRecord.VabId)
            {
                continue;
            }

            activeVoiceCount++;
        }

        return activeVoiceCount;
    }

    // GHIDRA: AreSoundEffectsIdle @ 0x80049E10
    public bool AreSoundEffectsIdle()
    {
        for (var voiceIndex = 0; voiceIndex < 0x18; voiceIndex++)
        {
            if (_gameEngine.StaticVariables.g_voiceSfxId[voiceIndex] != 0)
            {
                return false;
            }
        }

        EnsureSoundEffectDataInitialized();

        for (var sfxId = 0; sfxId < _gameEngine.StaticVariables.g_soundEffectData.Length; sfxId++)
        {
            ref var soundEffectRecord = ref _gameEngine.StaticVariables.g_soundEffectData[sfxId];
            if (soundEffectRecord.SeqNum == -1 || (soundEffectRecord.Flags & 0x0002) == 0)
            {
                continue;
            }

            if ((uint)soundEffectRecord.SeqNum >= (uint)_gameEngine.StaticVariables.g_loadedSequenceHandles.Length)
            {
                continue;
            }

            var sequenceSlot = _gameEngine.StaticVariables.g_loadedSequenceHandles[soundEffectRecord.SeqNum];
            if (sequenceSlot >= 0 && FUN_8008dd1c(sequenceSlot, 0) == 1)
            {
                return false;
            }
        }

        return true;
    }

    public bool WaitForSoundEffectsIdleStep(ref int extraFramesRemaining)
    {
        // With an external 60 Hz tick driver, the interrupt-style tick already runs in real
        // time; ticking again here would double the music rate during warp waits.
        if (!HasExternalSoundTickDriver)
        {
            AdvanceSoundFrame();
        }

        lock (_soundTickGate)
        {
            return WaitForSoundEffectsIdleStepCore(ref extraFramesRemaining);
        }
    }

    private bool WaitForSoundEffectsIdleStepCore(ref int extraFramesRemaining)
    {
        if (extraFramesRemaining > 0)
        {
            extraFramesRemaining--;
            return false;
        }

        if (extraFramesRemaining == 0)
        {
            return true;
        }

        if (_gameEngine.StaticVariables.g_soundEffectState != 0)
        {
            return false;
        }

        if (!AreSoundEffectsIdle())
        {
            return false;
        }

        extraFramesRemaining = 2;
        return false;
    }

    // GHIDRA: StopSoundEffect @ 0x80049634
    private void StopSoundEffect(int sfxId)
    {
        if (_gameEngine.StaticVariables.g_voiceCommandLock == 1)
        {
            return;
        }

        EnsureSoundEffectDataInitialized();

        while (true)
        {
            var voiceId = FindVoiceBySfxId(sfxId);
            if (voiceId < 0)
            {
                break;
            }

            StopVoice((short)voiceId);
        }

        if ((uint)sfxId < (uint)_gameEngine.StaticVariables.g_soundEffectData.Length)
        {
            _gameEngine.StaticVariables.g_soundEffectData[sfxId].Flags = 0;
        }
    }

    // GHIDRA: FindSfxRecordForSoundGroup @ 0x80048A14
    public int FindSfxRecordForSoundGroup(int sfxId, int vabId)
    {
        EnsureSoundEffectDataInitialized();

        int refSfxId = sfxId;
        var soundEffectData = _gameEngine.StaticVariables.g_soundEffectData;

        while ((uint)refSfxId < (uint)soundEffectData.Length)
        {
            ref var soundEffectRecord = ref soundEffectData[refSfxId];

            if (soundEffectRecord.VabId == vabId)
            {
                return refSfxId;
            }

            refSfxId = soundEffectRecord.RefSfxId;

            if (refSfxId == 0)
            {
                return -1;
            }
        }

        return -1;
    }

    // GHIDRA: FindVoiceBySfxId @ 0x80049714
    public int FindVoiceBySfxId(int param_1)
    {
        var iVar1 = 0;

        do
        {
            if (_gameEngine.StaticVariables.g_voiceSfxId[iVar1] == param_1)
            {
                return iVar1;
            }

            iVar1++;
        }
        while (iVar1 < 0x18);

        return -1;
    }

            // GHIDRA: FUN_80091204 @ 0x80091204
            private byte FUN_80091204(byte[] toneIndices, byte[] voiceIds)
            {
                if (!TryGetLoadedVabHeader(_gameEngine.StaticVariables.g_currentVabIdByte, out var vabHeader))
                {
                    return 0;
                }

                var programIndex = _gameEngine.StaticVariables.g_currentVabProgramIndex;
                if ((uint)programIndex >= (uint)vabHeader.VagAttributes.Length)
                {
                    return 0;
                }

                var tones = vabHeader.VagAttributes[programIndex];
                var toneCount = Math.Min(_gameEngine.StaticVariables.g_currentVabToneCount, (byte)tones.Length);
                byte matchCount = 0;
                for (byte toneIndex = 0; toneIndex < toneCount; toneIndex++)
                {
                    var tone = tones[toneIndex];
                    if (tone.Min <= _gameEngine.StaticVariables.DAT_801f769a && _gameEngine.StaticVariables.DAT_801f769a <= tone.Max)
                    {
                        voiceIds[matchCount] = unchecked((byte)tone.Vag);
                        toneIndices[matchCount] = toneIndex;
                        matchCount++;
                    }
                }

                return matchCount;
            }

            // GHIDRA: FUN_80091B60 @ 0x80091B60
            private uint FUN_80091b60()
            {
                var noteDelta = (_gameEngine.StaticVariables.DAT_801f769a + 0x3C) - _gameEngine.StaticVariables.DAT_801f76a8;
                var octaveQuotient = noteDelta / 12;
                var semitoneRemainder = noteDelta % 12;
                var fineIndex = _gameEngine.StaticVariables.DAT_801f76a9 >> 3;
                if (fineIndex > 0x0F)
                {
                    fineIndex = 0x0F;
                }

                uint pitch = s_voicePitchTable[(semitoneRemainder << 4) + fineIndex];
                var octaveShift = octaveQuotient - 5;
                if (octaveShift < 0)
                {
                    pitch >>= -octaveShift;
                }
                else if (octaveShift > 0)
                {
                    pitch <<= octaveShift;
                }

                return pitch & 0xFFFF;
            }

            // GHIDRA: FUN_800934B8 @ 0x800934B8
            // PARTIAL: the original SPU voice start is bridged to desktop playback through TryPlayLoadedVabToneVoice and TrackVoicePlayback; raw note velocity/orientation and tone volume/pan storage are now closed.
            private uint FUN_800934b8(uint sequenceKey, ushort vabId, short programNumber, ushort note, ushort velocity, byte orientation)
            {
                var seqId = (short)(sequenceKey & 0xFF);
                if ((uint)seqId >= (uint)_gameEngine.StaticVariables.g_sequenceStatePointers.Length)
                {
                    return 0xFFFFFFFF;
                }

                ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[seqId];
                if (SelectLoadedVabProgram((short)vabId, programNumber) != 0 || !TryGetLoadedVabHeader((short)vabId, out var vabHeader))
                {
                    return 0xFFFFFFFF;
                }

                _gameEngine.StaticVariables.DAT_801f769a = unchecked((byte)note);
                _gameEngine.StaticVariables.DAT_801f769b = 0;
                if ((short)sequenceKey == 0x21)
                {
                    _gameEngine.StaticVariables.DAT_801f769c = unchecked((byte)velocity);
                }
                else
                {
                    _gameEngine.StaticVariables.DAT_801f769c = unchecked((byte)((velocity * GetSequenceChannelVolume(ref sequenceState, sequenceState.CurrentChannel)) / 0x7F));
                }

                _gameEngine.StaticVariables.DAT_801f769d = orientation;
                var programAttr = vabHeader.ProgAttributes[programNumber];
                _gameEngine.StaticVariables.DAT_801f76a2 = programAttr.Mvol;
                _gameEngine.StaticVariables.DAT_801f76a3 = programAttr.Mpan;
                _gameEngine.StaticVariables.g_currentVabToneCount = programAttr.Tones;
                _gameEngine.StaticVariables.g_sequenceKey = unchecked((short)sequenceKey);

                var playedMask = 0xFFFFFFFFu;
                if (_gameEngine.StaticVariables.g_currentVabFirstToneIndex >= vabHeader.Header.Ts)
                {
                    return playedMask;
                }

                if (velocity == 0)
                {
                    return unchecked((uint)FUN_80093a04((short)sequenceKey, (short)vabId, programNumber, note));
                }

                var toneIndices = new byte[136];
                var voiceIds = new byte[128];
                var matchCount = FUN_80091204(toneIndices, voiceIds);
                playedMask = 0;
                if (matchCount == 0)
                {
                    return playedMask;
                }

                var loadedVabBody = _loadedVabBodies[vabId];
                for (var matchIndex = 0; matchIndex < matchCount; matchIndex++)
                {
                    _gameEngine.StaticVariables.DAT_801f76b0 = voiceIds[matchIndex];
                    _gameEngine.StaticVariables.DAT_801f76a4 = toneIndices[matchIndex];

                    if (FUN_80090370((short)vabId, programNumber, _gameEngine.StaticVariables.DAT_801f76a4, out var toneAttributes) != 0)
                    {
                        continue;
                    }

                    _gameEngine.StaticVariables.DAT_801f76a7 = toneAttributes.Priority;
                    _gameEngine.StaticVariables.DAT_801f76a5 = toneAttributes.Volume;
                    _gameEngine.StaticVariables.DAT_801f76a6 = toneAttributes.Pan;
                    _gameEngine.StaticVariables.DAT_801f76a8 = toneAttributes.Center;
                    _gameEngine.StaticVariables.DAT_801f76a9 = toneAttributes.Shift;
                    _gameEngine.StaticVariables.DAT_801f76ac = toneAttributes.Mode;
                    _gameEngine.StaticVariables.DAT_801f76aa = toneAttributes.Min;
                    _gameEngine.StaticVariables.DAT_801f76ab = toneAttributes.Max;

                    var voiceId = AllocateVoiceSlot();
                    _gameEngine.StaticVariables.DAT_maybeCurrentVoiceIndex_801f76b2 = voiceId;
                    if ((uint)voiceId >= (uint)_gameEngine.StaticVariables.g_numberOfVoices || (uint)voiceId >= (uint)_gameEngine.StaticVariables.g_voiceRuntimeSlots.Length)
                    {
                        playedMask = 0xFFFFFFFF;
                        continue;
                    }

                    ref var voiceSlot = ref _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId];
                    voiceSlot.NoiseState = 1;
                    voiceSlot.ReplacementAge = 0;
                    voiceSlot.SequenceKey = unchecked((short)sequenceKey);
                    voiceSlot.VabId = unchecked((short)vabId);
                    voiceSlot.VabFirstToneIndex = unchecked((short)_gameEngine.StaticVariables.g_currentVabFirstToneIndex);
                    voiceSlot.ProgramIndex = programNumber;
                    if ((short)sequenceKey != 0x21)
                    {
                        voiceSlot.field_0x08 = unchecked((short)velocity);
                    }

                    voiceSlot.field_0x0A = orientation;
                    voiceSlot.ToneIndex = unchecked((short)_gameEngine.StaticVariables.DAT_801f76a4);
                    voiceSlot.Note = unchecked((short)note);
                    voiceSlot.Priority = unchecked((short)_gameEngine.StaticVariables.DAT_801f76a7);
                    voiceSlot.field_0x00 = unchecked((ushort)_gameEngine.StaticVariables.DAT_801f76b0);

                    FUN_800912b4();
                    if (_gameEngine.StaticVariables.DAT_801f76b0 == 0xFF)
                    {
                        FUN_800914cc(voiceId);
                    }
                    else
                    {
                        var pitch = FUN_80091b60();
                        FUN_80090c58(matchCount, unchecked((short)pitch));
                        if (loadedVabBody == null || !TryPlayLoadedVabToneVoice(voiceId, (short)vabId, programNumber, _gameEngine.StaticVariables.DAT_801f76a4, note))
                        {
                            StopVoice(voiceId);
                            playedMask = 0xFFFFFFFF;
                            continue;
                        }

                        PushTrackedVoiceAdsr(voiceId);
                        _gameEngine.SoundBin.TrackVoicePlayback(false, voiceId, programNumber, _gameEngine.StaticVariables.DAT_801f76a4,
                            note, vabHeader, loadedVabBody);
                        PushTrackedVoiceStereoVolume(voiceId);
                    }

                    _gameEngine.StaticVariables.g_voiceState[voiceId] = 0x80;
                    _gameEngine.StaticVariables.g_voiceSfxId[voiceId] = 0;
                    _gameEngine.StaticVariables.g_voiceVabId[voiceId] = vabId;
                    _gameEngine.StaticVariables.g_voiceToneIndex[voiceId] = _gameEngine.StaticVariables.DAT_801f76a4;
                    _gameEngine.StaticVariables.g_voiceToneVolume[voiceId] = _gameEngine.StaticVariables.DAT_801f76a5;
                    _gameEngine.StaticVariables.g_voiceTonePan[voiceId] = _gameEngine.StaticVariables.DAT_801f76a6;
                    playedMask |= 1u << (voiceId & 0x1F);
                }

                return playedMask;
            }

    // GHIDRA: FindVoiceBySfxIdAndToneIndex @ 0x8004974C
    public int FindVoiceBySfxIdAndToneIndex(int param_1, int param_2)
    {
        var iVar1 = 0;

        do
        {
            if (_gameEngine.StaticVariables.g_voiceSfxId[iVar1] == param_1
                && _gameEngine.StaticVariables.g_voiceToneIndex[iVar1] == param_2)
            {
                return iVar1;
            }

            iVar1++;
        }
        while (iVar1 < 0x18);

        return -1;
    }

    private bool TryResolveSoundEffectRecord(int requestedSfxId, out int resolvedSfxId, out short loadedVabId)
    {
        resolvedSfxId = requestedSfxId;
        loadedVabId = -1;

        ref var soundEffectRecord = ref _gameEngine.StaticVariables.g_soundEffectData[requestedSfxId];
        if (soundEffectRecord.VabId == -2)
        {
            return false;
        }

        if (soundEffectRecord.VabId == -1)
        {
            loadedVabId = _gameEngine.StaticVariables.g_globalSoundVabId;
            return true;
        }

        if (_gameEngine.StaticVariables.g_currentSoundGroup == soundEffectRecord.VabId)
        {
            loadedVabId = _gameEngine.StaticVariables.g_mapSoundVabId;
            return true;
        }

        resolvedSfxId = FindSfxRecordForSoundGroup(requestedSfxId, _gameEngine.StaticVariables.g_currentSoundGroup);
        if (resolvedSfxId < 0)
        {
            return false;
        }

        loadedVabId = _gameEngine.StaticVariables.g_mapSoundVabId;
        return true;
    }

    private bool TryGetSoundEffectSequenceData(short sequenceIndex, out byte[] sequenceData)
    {
        sequenceData = [];

        if ((uint)sequenceIndex >= (uint)_gameEngine.StaticVariables.g_soundEffectSeqOffsets.Length)
        {
            return false;
        }

        var sequenceStart = _gameEngine.StaticVariables.g_soundEffectSeqOffsets[sequenceIndex] & ~3;
        var sequenceEnd = sequenceIndex + 1 < _gameEngine.StaticVariables.g_soundEffectSeqOffsets.Length
            ? _gameEngine.StaticVariables.g_soundEffectSeqOffsets[sequenceIndex + 1]
            : _gameEngine.StaticVariables.g_soundBinSequenceBuffer.Length;

        sequenceEnd = Math.Min(sequenceEnd, _gameEngine.StaticVariables.g_soundBinSequenceBuffer.Length);
        if ((uint)sequenceStart >= (uint)sequenceEnd)
        {
            return false;
        }

        sequenceData = new byte[sequenceEnd - sequenceStart];
        Array.Copy(_gameEngine.StaticVariables.g_soundBinSequenceBuffer, sequenceStart, sequenceData, 0, sequenceData.Length);
        return true;
    }

    private bool TryPlaySoundEffectSequence(ref SoundEffectRecord soundEffectRecord, short vabId)
    {
        if (soundEffectRecord.SeqNum < 0)
        {
            return false;
        }

        if ((soundEffectRecord.Flags & 0x0002) != 0)
        {
            return true;
        }

        if (vabId < 0 || !TryGetSoundEffectSequenceData(soundEffectRecord.SeqNum, out var sequenceData))
        {
            return true;
        }

        var sequenceSlot = LoadSeq(sequenceData, vabId);
        if (sequenceSlot < 0)
        {
            return true;
        }

        if ((uint)soundEffectRecord.SeqNum < (uint)_gameEngine.StaticVariables.g_loadedSequenceHandles.Length)
        {
            _gameEngine.StaticVariables.g_loadedSequenceHandles[soundEffectRecord.SeqNum] = sequenceSlot;
        }

        PlaySeq(sequenceSlot, 1, 1);
        soundEffectRecord.Flags = (short)(soundEffectRecord.Flags | 0x0002);
        return true;
    }

    // GHIDRA: FUN_8008DD1C @ 0x8008DD1C
    private byte FUN_8008dd1c(short sequenceSlot, short trackId)
    {
        if ((uint)sequenceSlot >= (uint)_gameEngine.StaticVariables.g_sequenceStatePointers.Length || trackId != 0)
        {
            return 0;
        }

        return _gameEngine.StaticVariables.g_sequenceStatePointers[sequenceSlot].field_0x2B;
    }

    private bool TryGetLoadedVabHeader(short vabId, out SoundBin.VabHeader vabHeader)
    {
        if ((uint)vabId >= 0x10 || _gameEngine.StaticVariables.g_loadedVabState[vabId] != 1)
        {
            vabHeader = null!;
            return false;
        }

        var loadedVabHeader = _loadedVabHeaders[vabId];
        if (loadedVabHeader != null)
        {
            vabHeader = loadedVabHeader;
            return true;
        }

        vabHeader = null!;
        return false;
    }

    // JUSTIFICATION: PSX hardware adaptation only
    private bool TryPlayLoadedVabToneVoice(short voiceId, short vabId, int programNumber, int toneIndex, int note)
    {
        if (!TryGetLoadedVabHeader(vabId, out var vabHeader))
        {
            return false;
        }

        var loadedVabBody = _loadedVabBodies[vabId];
        if (loadedVabBody == null)
        {
            return false;
        }

        var rawPitch = (uint)voiceId < (uint)_gameEngine.StaticVariables.g_spuVoicePitch.Length
            ? _gameEngine.StaticVariables.g_spuVoicePitch[voiceId]
            : (short)0;

        return _gameEngine.SoundBin.PlayLoadedVabTone(voiceId, vabHeader, loadedVabBody, programNumber, toneIndex, note, false, out _, out _, out _, rawPitch) != null;
    }

    // GHIDRA: SelectLoadedVabProgram @ 0x800902AC
    private short SelectLoadedVabProgram(short vabId, short programIndex)
    {
        if (!TryGetLoadedVabHeader(vabId, out var vabHeader))
        {
            return -1;
        }

        if ((uint)programIndex >= (uint)vabHeader.Header.Ps)
        {
            return -1;
        }

        var firstToneIndex = unchecked((byte)vabHeader.ProgAttributes[programIndex].Reserved1);
        _gameEngine.StaticVariables.g_currentVabIdByte = (byte)vabId;
        _gameEngine.StaticVariables.g_currentVabProgramIndex = (byte)programIndex;
        _gameEngine.StaticVariables.g_currentVabFirstToneIndex = firstToneIndex;
        return 0;
    }

    // GHIDRA: CopyVabProgramAttributes @ 0x800901A8
    private short FUN_800901a8(short vabId, short programIndex, out VabProgramAttributesCopy programAttributes)
    {
        programAttributes = default;
        if (SelectLoadedVabProgram(vabId, programIndex) < 0 || !TryGetLoadedVabHeader(vabId, out var vabHeader))
        {
            return -1;
        }

        var programAttr = vabHeader.ProgAttributes[programIndex];
        programAttributes.Tones = programAttr.Tones;
        programAttributes.Volume = programAttr.Mvol;
        programAttributes.Priority = programAttr.Prior;
        programAttributes.Mode = programAttr.Mode;
        programAttributes.Pan = programAttr.Mpan;
        programAttributes.Attr = unchecked((ushort)programAttr.Attr);
        return 0;
    }

    // GHIDRA: CopyVabToneAttributes @ 0x80090370
    private short FUN_80090370(short vabId, short programIndex, short toneIndex, out VabToneAttributesCopy toneAttributes)
    {
        toneAttributes = default;
        if (SelectLoadedVabProgram(vabId, programIndex) < 0 || !TryGetLoadedVabHeader(vabId, out var vabHeader))
        {
            return -1;
        }

        if ((uint)programIndex >= (uint)vabHeader.VagAttributes.Length)
        {
            return -1;
        }

        var tones = vabHeader.VagAttributes[programIndex];
        if ((uint)toneIndex >= (uint)tones.Length)
        {
            return -1;
        }

        var toneAttr = tones[toneIndex];
        toneAttributes.Priority = toneAttr.Prior;
        toneAttributes.Mode = toneAttr.Mode;
        toneAttributes.Volume = toneAttr.Vol;
        toneAttributes.Pan = toneAttr.Pan;
        toneAttributes.Center = toneAttr.Center;
        toneAttributes.Shift = toneAttr.Shift;
        toneAttributes.Min = toneAttr.Min;
        toneAttributes.Max = toneAttr.Max;
        toneAttributes.VibratoWidth = toneAttr.VibW;
        toneAttributes.VibratoTime = toneAttr.VibT;
        toneAttributes.PortamentoWidth = toneAttr.PorW;
        toneAttributes.PortamentoTime = toneAttr.PorT;
        toneAttributes.PitchBendMin = toneAttr.Pbmin;
        toneAttributes.PitchBendMax = toneAttr.Pbmax;
        toneAttributes.Adsr1 = toneAttr.Adsr1;
        toneAttributes.Adsr2 = toneAttr.Adsr2;
        toneAttributes.Program = unchecked((ushort)toneAttr.Prog);
        toneAttributes.Vag = unchecked((ushort)toneAttr.Vag);
        return 0;
    }

    // GHIDRA: AllocateVoiceSlot @ 0x800909E8
    private short AllocateVoiceSlot()
    {
        var selectedVoiceIndex = 0x63;
        var candidateVoiceIndex = 0;
        var replacementFound = false;
        var bestPriority = (ushort)_gameEngine.StaticVariables.DAT_801f76a7;
        var bestReplacementCriterion = ushort.MaxValue;
        short bestReplacementAge = 0;
        var voiceCount = Math.Min(_gameEngine.StaticVariables.g_numberOfVoices, _gameEngine.StaticVariables.g_voiceRuntimeSlots.Length);

        for (var voiceIndex = 0; voiceIndex < voiceCount; voiceIndex++)
        {
            ref var voiceSlot = ref _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceIndex];
            if (voiceSlot.NoiseState == 0 && voiceSlot.field_0x06 == 0)
            {
                selectedVoiceIndex = voiceIndex;
                break;
            }

            var voicePriority = (ushort)voiceSlot.Priority;
            if (voicePriority < bestPriority)
            {
                bestPriority = voicePriority;
                bestReplacementCriterion = (ushort)voiceSlot.field_0x06;
                bestReplacementAge = voiceSlot.ReplacementAge;
                candidateVoiceIndex = voiceIndex;
                replacementFound = true;
                continue;
            }

            if (voicePriority != bestPriority)
            {
                continue;
            }

            var replacementCriterion = (ushort)voiceSlot.field_0x06;
            if (replacementCriterion < bestReplacementCriterion || (replacementCriterion == bestReplacementCriterion && voiceSlot.ReplacementAge > bestReplacementAge))
            {
                bestReplacementCriterion = replacementCriterion;
                bestReplacementAge = voiceSlot.ReplacementAge;
                candidateVoiceIndex = voiceIndex;
                replacementFound = true;
            }
        }

        if (selectedVoiceIndex == 0x63)
        {
            selectedVoiceIndex = replacementFound ? candidateVoiceIndex : voiceCount;
        }

        if ((uint)selectedVoiceIndex >= (uint)voiceCount)
        {
            return (short)selectedVoiceIndex;
        }

        for (var voiceIndex = 0; voiceIndex < voiceCount; voiceIndex++)
        {
            _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceIndex].ReplacementAge = unchecked((short)(_gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceIndex].ReplacementAge + 1));
        }

        ref var selectedVoice = ref _gameEngine.StaticVariables.g_voiceRuntimeSlots[selectedVoiceIndex];
        selectedVoice.ReplacementAge = 0;
        selectedVoice.Priority = _gameEngine.StaticVariables.DAT_801f76a7;

        if (selectedVoice.NoiseState == 2)
        {
            _gameEngine.SoundBin.StopTrackedVoice(selectedVoiceIndex);
        }

        return (short)selectedVoiceIndex;
    }

    private bool TryGetCurrentVabContext(out SoundBin.VabHeader vabHeader, out SoundBin.VabHeader.ProgAtr programAttr, out SoundBin.VabHeader.VagAtr toneAttr)
    {
        vabHeader = null!;
        programAttr = null!;
        toneAttr = null!;

        if (!TryGetLoadedVabHeader(_gameEngine.StaticVariables.g_currentVabIdByte, out vabHeader))
        {
            return false;
        }

        var programIndex = _gameEngine.StaticVariables.g_currentVabProgramIndex;
        var toneIndex = _gameEngine.StaticVariables.DAT_801f76a4;

        if ((uint)programIndex >= (uint)vabHeader.ProgAttributes.Length || (uint)programIndex >= (uint)vabHeader.VagAttributes.Length)
        {
            return false;
        }

        var tones = vabHeader.VagAttributes[programIndex];
        if ((uint)toneIndex >= (uint)tones.Length)
        {
            return false;
        }

        programAttr = vabHeader.ProgAttributes[programIndex];
        toneAttr = tones[toneIndex];
        return true;
    }

    // GHIDRA: CalculateVoicePitch @ 0x80091C18
    private short CalculateVoicePitch(short note, short fine)
    {
        if (!TryGetCurrentVabContext(out _, out _, out var toneAttr))
        {
            return 0;
        }

        var fineIndex = fine + toneAttr.Shift;
        if (fineIndex < 0)
        {
            fineIndex += 7;
        }

        fineIndex >>= 3;

        var octaveCarry = 0;
        if (fineIndex >= 0x10)
        {
            octaveCarry = 1;
            fineIndex -= 0x10;
        }

        var noteDelta = octaveCarry + (note + 0x3c - toneAttr.Center);
        var octaveShift = (noteDelta / 12) - 5;
        var pitch = s_voicePitchTable[((noteDelta % 12) << 4) + fineIndex];

        if (octaveShift > 0)
        {
            pitch = (ushort)(pitch << octaveShift);
        }
        else if (octaveShift < 0)
        {
            pitch = (ushort)(pitch >> -octaveShift);
        }

        return unchecked((short)pitch);
    }

    // GHIDRA: FUN_800912B4 @ 0x800912B4
    private void FUN_800912b4()
    {
        var voiceId = _gameEngine.StaticVariables.DAT_maybeCurrentVoiceIndex_801f76b2;
        if ((uint)voiceId >= (uint)_gameEngine.StaticVariables.g_voiceRuntimeSlots.Length || !TryGetCurrentVabContext(out _, out var programAttr, out var toneAttr))
        {
            return;
        }

        _gameEngine.StaticVariables.DAT_801f76b4 = (short)(voiceId << 3);
        _gameEngine.StaticVariables.DAT_801f76b6 = (short)((_gameEngine.StaticVariables.g_currentVabFirstToneIndex << 4) + _gameEngine.StaticVariables.DAT_801f76a4);
        _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId].field_0x06 = 0x7fff;

        var clearMask = ~(1 << voiceId);
        for (var index = 0; index < _gameEngine.StaticVariables.DAT_801f7e18.Length; index++)
        {
            _gameEngine.StaticVariables.DAT_801f7e18[index] &= clearMask;
        }

        var reverbWord = unchecked((uint)programAttr.Reserved2);
        var reverbValue = (_gameEngine.StaticVariables.DAT_801f76b0 & 0x0001) != 0
            ? unchecked((ushort)reverbWord)
            : unchecked((ushort)(reverbWord >> 16));

        _gameEngine.StaticVariables.g_spuVoiceReverb[voiceId] = unchecked((short)reverbValue);
        _gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] = (byte)(_gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] | 0x08);
        _gameEngine.StaticVariables.g_spuVoiceAdsr1[voiceId] = unchecked((short)toneAttr.Adsr1);
        _gameEngine.StaticVariables.g_spuVoiceAdsr2[voiceId] = unchecked((short)(ushort)(toneAttr.Adsr2 + unchecked((ushort)_gameEngine.StaticVariables.DAT_sound_801f7610)));
        _gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] = (byte)(_gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] | 0x30);
    }

    // GHIDRA: FUN_80090C58 @ 0x80090C58
    private void FUN_80090c58(short param_1, short pitch)
    {
        _ = param_1;

        var voiceId = _gameEngine.StaticVariables.DAT_maybeCurrentVoiceIndex_801f76b2;
        if ((uint)voiceId >= (uint)_gameEngine.StaticVariables.g_voiceRuntimeSlots.Length || !TryGetCurrentVabContext(out var vabHeader, out _, out _))
        {
            return;
        }

        var baseVolume = (_gameEngine.StaticVariables.DAT_801f769c * (((int)vabHeader.Header.Mvol << 14) - vabHeader.Header.Mvol)) / 0x3f01;
        baseVolume = (baseVolume * _gameEngine.StaticVariables.DAT_801f76a2 * _gameEngine.StaticVariables.DAT_801f76a5) / 0x3f01;

        var leftVolume = baseVolume;
        var rightVolume = baseVolume;
        var sequenceKey = unchecked((ushort)_gameEngine.StaticVariables.g_sequenceKey);

        if (sequenceKey != 0x0021)
        {
            var sequenceId = sequenceKey & 0x00ff;
            if ((uint)sequenceId < (uint)_gameEngine.StaticVariables.g_sequenceStatePointers.Length)
            {
                ref var sequenceState = ref _gameEngine.StaticVariables.g_sequenceStatePointers[sequenceId];
                leftVolume = (baseVolume * unchecked((ushort)sequenceState.field_0x74)) / 0x7f;
                rightVolume = (baseVolume * unchecked((ushort)sequenceState.field_0x76)) / 0x7f;
            }
        }

        var tonePan = _gameEngine.StaticVariables.DAT_801f76a6;
        if (tonePan < 0x40)
        {
            rightVolume = (rightVolume * tonePan) / 0x3f;
        }
        else
        {
            leftVolume = (leftVolume * (0x7f - tonePan)) / 0x3f;
        }

        var programPan = _gameEngine.StaticVariables.DAT_801f76a3;
        if (programPan < 0x40)
        {
            rightVolume = (rightVolume * programPan) / 0x3f;
        }
        else
        {
            leftVolume = (leftVolume * (0x7f - programPan)) / 0x3f;
        }

        var voicePan = _gameEngine.StaticVariables.DAT_801f769d;
        if (voicePan < 0x40)
        {
            rightVolume = (rightVolume * voicePan) / 0x3f;
        }
        else
        {
            leftVolume = (leftVolume * (0x7f - voicePan)) / 0x3f;
        }

        if (_gameEngine.StaticVariables.DAT_sound_801f7658 == 1)
        {
            if (leftVolume < rightVolume)
            {
                leftVolume = rightVolume;
            }
            else
            {
                rightVolume = leftVolume;
            }
        }

        leftVolume = (leftVolume * leftVolume) / 0x3fff;
        rightVolume = (rightVolume * rightVolume) / 0x3fff;

        _gameEngine.StaticVariables.g_spuVoiceVolumeLeft[voiceId] = unchecked((short)leftVolume);
        _gameEngine.StaticVariables.g_spuVoiceVolumeRight[voiceId] = unchecked((short)rightVolume);
        _gameEngine.StaticVariables.g_spuVoicePitch[voiceId] = pitch;
        _gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] = (byte)(_gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] | 0x07);
        PushTrackedVoiceAdsr(voiceId);
        PushTrackedVoiceStereoVolume(voiceId);
        PushTrackedVoicePitch(voiceId);
        _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId].CurrentPitch = pitch;
        _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId].NoiseState = 1;

        GetVoiceCommandMasks(voiceId, out var leftMask, out var rightMask);
        if ((_gameEngine.StaticVariables.DAT_801f76ac & 0x04) != 0)
        {
            _gameEngine.StaticVariables.DAT_sound_801f7ef8 = (short)(unchecked((ushort)_gameEngine.StaticVariables.DAT_sound_801f7ef8) | leftMask);
            _gameEngine.StaticVariables.DAT_sound_801f7f00 = (short)(unchecked((ushort)_gameEngine.StaticVariables.DAT_sound_801f7f00) | rightMask);
        }
        else
        {
            _gameEngine.StaticVariables.DAT_sound_801f7ef8 = (short)(unchecked((ushort)_gameEngine.StaticVariables.DAT_sound_801f7ef8) & unchecked((ushort)~leftMask));
            _gameEngine.StaticVariables.DAT_sound_801f7f00 = (short)(unchecked((ushort)_gameEngine.StaticVariables.DAT_sound_801f7f00) & unchecked((ushort)~rightMask));
        }

        _gameEngine.StaticVariables.g_voiceCommandPlayingLeft = (short)(unchecked((ushort)_gameEngine.StaticVariables.g_voiceCommandPlayingLeft) | leftMask);
        _gameEngine.StaticVariables.g_voiceCommandPlayingRight = (short)(unchecked((ushort)_gameEngine.StaticVariables.g_voiceCommandPlayingRight) | rightMask);
        _gameEngine.StaticVariables.g_voiceCommandPendingLeft = (short)(unchecked((ushort)_gameEngine.StaticVariables.g_voiceCommandPendingLeft) & unchecked((ushort)~leftMask));
        _gameEngine.StaticVariables.g_voiceCommandPendingRight = (short)(unchecked((ushort)_gameEngine.StaticVariables.g_voiceCommandPendingRight) & unchecked((ushort)~rightMask));
    }

    // GHIDRA: FUN_800914CC @ 0x800914CC
    private void FUN_800914cc(short voiceId)
    {
        if ((uint)voiceId >= (uint)_gameEngine.StaticVariables.g_voiceRuntimeSlots.Length)
        {
            return;
        }

        FUN_80093de8(_gameEngine.StaticVariables.g_sequenceKey, out var sequenceVolumeLeft, out var sequenceVolumeRight);

        var leftVolume = (((int)sequenceVolumeLeft << 7) + sequenceVolumeLeft) * _gameEngine.StaticVariables.DAT_801f76a2 / 0x7f;
        var rightVolume = (((int)sequenceVolumeRight << 7) + sequenceVolumeRight) * _gameEngine.StaticVariables.DAT_801f76a2 / 0x7f;

        leftVolume = (leftVolume * _gameEngine.StaticVariables.DAT_801f76a5) / 0x7f;
        rightVolume = (rightVolume * _gameEngine.StaticVariables.DAT_801f76a5) / 0x7f;

        var tonePan = _gameEngine.StaticVariables.DAT_801f76a6;
        if (tonePan < 0x40)
        {
            rightVolume = (leftVolume * tonePan) / 0x3f;
        }
        else
        {
            leftVolume = (leftVolume * (0x7f - tonePan)) / 0x3f;
        }

        var programPan = _gameEngine.StaticVariables.DAT_801f76a3;
        if (programPan < 0x40)
        {
            rightVolume = (leftVolume * programPan) / 0x3f;
        }
        else
        {
            leftVolume = (rightVolume * (0x7f - programPan)) / 0x3f;
        }

        var voicePan = _gameEngine.StaticVariables.DAT_801f769d;
        if (voicePan < 0x40)
        {
            rightVolume = (voicePan * rightVolume) / 0x3f;
        }
        else
        {
            leftVolume = (leftVolume * (0x7f - voicePan)) / 0x3f;
        }

        if (_gameEngine.StaticVariables.DAT_sound_801f7658 == 1)
        {
            if (leftVolume < rightVolume)
            {
                leftVolume = rightVolume;
            }
            else
            {
                rightVolume = leftVolume;
            }

            leftVolume = (leftVolume * leftVolume) / 0x3fff;
            rightVolume = (rightVolume * rightVolume) / 0x3fff;
        }

        var driverVoiceState = _gameEngine.StaticVariables.PTR_VOICE_00_LEFT_RIGHT_800c9794 ??= new AlundraEngine.Gameplay.Voice();
        var rawNoisePitch = unchecked((ushort)(driverVoiceState.field_0x1AA & 0xC0FF));
        rawNoisePitch = (ushort)(rawNoisePitch | (((_gameEngine.StaticVariables.DAT_801f769a - _gameEngine.StaticVariables.DAT_801f76a8) & 0x3f) << 8));
        driverVoiceState.field_0x1AA = rawNoisePitch;

        _gameEngine.StaticVariables.g_spuVoiceVolumeRight[voiceId] = unchecked((short)rightVolume);
        _gameEngine.StaticVariables.g_spuVoiceVolumeLeft[voiceId] = unchecked((short)leftVolume);
        _gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] = (byte)(_gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] | 0x03);
        PushTrackedVoiceStereoVolume(voiceId);
        _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId].CurrentPitch = 10;

        var voiceCount = Math.Min(_gameEngine.StaticVariables.g_numberOfVoices, _gameEngine.StaticVariables.g_voiceRuntimeSlots.Length);
        for (var index = 0; index < voiceCount; index++)
        {
            _gameEngine.StaticVariables.g_voiceRuntimeSlots[index].NoiseState = (byte)(_gameEngine.StaticVariables.g_voiceRuntimeSlots[index].NoiseState & 0x01);
        }

        _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId].NoiseState = 2;

        GetVoiceCommandMasks(voiceId, out var leftMask, out var rightMask);

        _gameEngine.StaticVariables.g_voiceCommandPlayingLeft = (short)(unchecked((ushort)_gameEngine.StaticVariables.g_voiceCommandPlayingLeft) | leftMask);
        _gameEngine.StaticVariables.g_voiceCommandPlayingRight = (short)(unchecked((ushort)_gameEngine.StaticVariables.g_voiceCommandPlayingRight) | rightMask);
        _gameEngine.StaticVariables.g_voiceCommandPendingLeft = (short)(unchecked((ushort)_gameEngine.StaticVariables.g_voiceCommandPendingLeft) & unchecked((ushort)~leftMask));
        _gameEngine.StaticVariables.g_voiceCommandPendingRight = (short)(unchecked((ushort)_gameEngine.StaticVariables.g_voiceCommandPendingRight) & unchecked((ushort)~rightMask));

        if ((_gameEngine.StaticVariables.DAT_801f76ac & 0x04) != 0)
        {
            _gameEngine.StaticVariables.DAT_sound_801f7ef8 = (short)(unchecked((ushort)_gameEngine.StaticVariables.DAT_sound_801f7ef8) | leftMask);
            _gameEngine.StaticVariables.DAT_sound_801f7f00 = (short)(unchecked((ushort)_gameEngine.StaticVariables.DAT_sound_801f7f00) | rightMask);
        }
        else
        {
            _gameEngine.StaticVariables.DAT_sound_801f7ef8 = (short)(unchecked((ushort)_gameEngine.StaticVariables.DAT_sound_801f7ef8) & unchecked((ushort)~leftMask));
            _gameEngine.StaticVariables.DAT_sound_801f7f00 = (short)(unchecked((ushort)_gameEngine.StaticVariables.DAT_sound_801f7f00) & unchecked((ushort)~rightMask));
        }

        driverVoiceState.field_0x194 = leftMask;
        driverVoiceState.field_0x196 = rightMask;
    }

    // GHIDRA: TriggerVoice @ 0x80094660
    private short TriggerVoice(short vabId, short programIndex, short toneIndex, short note, short fine, short volumeLeft, short volumeRight)
    {
        if (_gameEngine.StaticVariables.g_voiceCommandLock == 1)
        {
            return -1;
        }

        _gameEngine.StaticVariables.g_voiceCommandLock = 1;

        try
        {
            if (SelectLoadedVabProgram(vabId, programIndex) < 0)
            {
                return -1;
            }

            _gameEngine.StaticVariables.g_sequenceKey = 0x21;
            _gameEngine.StaticVariables.DAT_801f769a = unchecked((byte)note);
            _gameEngine.StaticVariables.DAT_801f769b = unchecked((byte)fine);
            _gameEngine.StaticVariables.DAT_801f76a4 = unchecked((byte)toneIndex);

            if (volumeLeft == volumeRight)
            {
                _gameEngine.StaticVariables.DAT_801f769d = 0x40;
                _gameEngine.StaticVariables.DAT_801f769c = unchecked((byte)volumeLeft);
            }
            else if (volumeRight < volumeLeft)
            {
                _gameEngine.StaticVariables.DAT_801f769c = unchecked((byte)volumeLeft);
                _gameEngine.StaticVariables.DAT_801f769d = unchecked((byte)(((int)volumeRight << 6) / volumeLeft));
            }
            else
            {
                _gameEngine.StaticVariables.DAT_801f769c = unchecked((byte)volumeRight);
                _gameEngine.StaticVariables.DAT_801f769d = unchecked((byte)(0x7f - (((int)volumeLeft << 6) / volumeRight)));
            }

            if (!TryGetCurrentVabContext(out _, out var programAttr, out var toneAttr))
            {
                return -1;
            }

            _gameEngine.StaticVariables.DAT_801f76a2 = programAttr.Mvol;
            _gameEngine.StaticVariables.DAT_801f76a3 = programAttr.Mpan;
            _gameEngine.StaticVariables.g_currentVabToneCount = programAttr.Tones;
            _gameEngine.StaticVariables.DAT_801f76a7 = toneAttr.Prior;
            _gameEngine.StaticVariables.DAT_801f76b0 = unchecked((short)(ushort)toneAttr.Vag);
            _gameEngine.StaticVariables.DAT_801f76a5 = toneAttr.Vol;
            _gameEngine.StaticVariables.DAT_801f76a6 = toneAttr.Pan;
            _gameEngine.StaticVariables.DAT_801f76a8 = toneAttr.Center;
            _gameEngine.StaticVariables.DAT_801f76a9 = toneAttr.Shift;
            _gameEngine.StaticVariables.DAT_801f76ac = toneAttr.Mode;
            _gameEngine.StaticVariables.DAT_801f76aa = toneAttr.Pbmin;
            _gameEngine.StaticVariables.DAT_801f76ab = toneAttr.Pbmax;

            if (_gameEngine.StaticVariables.g_currentVabToneCount == 0)
            {
                return -1;
            }

            var voiceId = AllocateVoiceSlot();
            if ((ushort)voiceId >= _gameEngine.StaticVariables.g_numberOfVoices || (uint)voiceId >= (uint)_gameEngine.StaticVariables.g_voiceRuntimeSlots.Length)
            {
                return -1;
            }

            _gameEngine.StaticVariables.DAT_maybeCurrentVoiceIndex_801f76b2 = voiceId;

            ref var voiceSlot = ref _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId];
            voiceSlot.SequenceKey = 0x21;
            voiceSlot.VabId = vabId;
            voiceSlot.ProgramIndex = programIndex;
            voiceSlot.VabFirstToneIndex = unchecked((short)_gameEngine.StaticVariables.g_currentVabFirstToneIndex);
            voiceSlot.Note = note;
            voiceSlot.Priority = unchecked((short)_gameEngine.StaticVariables.DAT_801f76a7);
            voiceSlot.NoiseState = 1;
            voiceSlot.ReplacementAge = 0;
            voiceSlot.field_0x00 = unchecked((ushort)_gameEngine.StaticVariables.DAT_801f76b0);
            voiceSlot.ToneIndex = unchecked((short)_gameEngine.StaticVariables.DAT_801f76a4);

            FUN_800912b4();

            if (unchecked((ushort)_gameEngine.StaticVariables.DAT_801f76b0) == 0x00ff)
            {
                FUN_800914cc(voiceId);
            }
            else
            {
                FUN_80090c58(1, CalculateVoicePitch(note, fine));
            }

            return voiceId;
        }
        finally
        {
            _gameEngine.StaticVariables.g_voiceCommandLock = 0;
        }
    }

    // JUSTIFICATION: C# language bridge only
    // RELATION: sound-side body split from LoadMapSounds @ 0x8004A09C
    public void LoadMapSounds(uint mapId)
    {
        lock (_soundTickGate)
        {
            LoadMapSoundsCore(mapId);
        }
    }

    private void LoadMapSoundsCore(uint mapId)
    {
        var soundoffset = _gameEngine.GetMapSoundIndex(mapId);

        if (soundoffset != 0)
        {
            int iVar2 = _gameEngine.StaticVariables.g_currentMapSoundIndex;
            soundoffset = _gameEngine.GetMapSoundIndex(mapId);

            if (iVar2 != soundoffset)
            {
                if (_gameEngine.StaticVariables.g_requestedSeqId >= 0)
                {
                    InitializeBgm(_gameEngine.StaticVariables.g_requestedSeqId);
                    FUN_8008a718(0);
                    ResetSomethingSound(_gameEngine.StaticVariables.g_requestedSeqId);
                }
                    
                soundoffset = _gameEngine.GetMapSoundIndex(mapId);
                if (soundoffset != 0x2d)
                {
                    soundoffset = _gameEngine.GetMapSoundIndex(mapId);
                    LoadMapSequence(soundoffset, 0);
                }

                FUN_8008f808(_gameEngine.StaticVariables.g_requestedSeqId, 0x7f, 10);
            }
        }

        var iVar1 = GetSoundGroupByMapId(mapId);

        if (_gameEngine.StaticVariables.g_currentSoundGroup != iVar1)
        {
            LoadMapSoundGroup(mapId);
        }
    }

    // GHIDRA: GetSoundGroupByMapId @ 0x80049F00
    public int GetSoundGroupByMapId(uint mapId)
    {
        return SoundBin.VabIndexByMapId[mapId];
    }

    // GHIDRA: LoadMapSoundGroup @ 0x800489C8
    private void LoadMapSoundGroup(uint mapId)
    {
        FreeLoadedVab(_gameEngine.StaticVariables.g_mapSoundVabId);
        var iVar1 = GetSoundGroupByMapId(mapId);
        _gameEngine.StaticVariables.g_currentSoundGroup = iVar1;

        LoadMapSoundVab(iVar1);

        // SoundBin keeps the desktop-decoder map buffers in sync with the runtime VAB slot above.
        _gameEngine.SoundBin.OpenMap(mapId);
    }

    // GHIDRA: PlaySoundEffectWithToneVolumeMix @ 0x80049794
    public void PlaySoundEffectWithToneVolumeMix(int param_1, int param_2, int param_3)
    {
        lock (_soundTickGate)
        {
            PlaySoundEffectWithToneVolumeMixCore(param_1, param_2, param_3);
        }
    }

    private void PlaySoundEffectWithToneVolumeMixCore(int param_1, int param_2, int param_3)
    {
        EnsureSoundEffectDataInitialized();

        if ((uint)param_1 >= (uint)_gameEngine.StaticVariables.g_soundEffectData.Length)
        {
            return;
        }

        if (!TryResolveSoundEffectRecord(param_1, out var resolvedSfxId, out var loadedVabId))
        {
            return;
        }

        ref var soundEffectRecord = ref _gameEngine.StaticVariables.g_soundEffectData[resolvedSfxId];
        if (soundEffectRecord.ToneCount <= 0 || FUN_800901a8(loadedVabId, soundEffectRecord.ProgramNumber, out var programAttributes) < 0)
        {
            return;
        }

        var programVolumeSquaredMinusOne = SquarePlusOne(programAttributes.Volume) - 1;
        var leftMixSquaredMinusOne = SquarePlusOne(param_2) - 1;
        var rightMixSquaredMinusOne = SquarePlusOne(param_3) - 1;

        for (var toneOffset = 0; toneOffset < soundEffectRecord.ToneCount; toneOffset++)
        {
            var toneIndex = soundEffectRecord.ToneNumber + toneOffset;
            var voiceId = FindVoiceBySfxIdAndToneIndex(param_1, toneIndex);
            if (voiceId < 0)
            {
                continue;
            }

            var tonePan = _gameEngine.StaticVariables.g_voiceTonePan[voiceId];
            var leftPanWeight = tonePan < 0x41 ? 0x3f : 0x7f - tonePan;
            var rightPanWeight = tonePan < 0x40 ? tonePan : 0x3f;
            var toneVolumeSquaredMinusOne = SquarePlusOne(_gameEngine.StaticVariables.g_voiceToneVolume[voiceId]) - 1;

            var leftBase = ApplyMipsVolumeScale13(ApplyMipsVolumeScale13(leftMixSquaredMinusOne * toneVolumeSquaredMinusOne) * programVolumeSquaredMinusOne);
            var rightBase = ApplyMipsVolumeScale13(ApplyMipsVolumeScale13(rightMixSquaredMinusOne * toneVolumeSquaredMinusOne) * programVolumeSquaredMinusOne);
            var volumeLeft = ApplyMipsVolumeScale11(leftBase * (leftPanWeight * leftPanWeight));
            var volumeRight = ApplyMipsVolumeScale11(rightBase * (rightPanWeight * rightPanWeight));

            SetVoiceVolume((short)voiceId, unchecked((short)volumeLeft), unchecked((short)volumeRight));
        }
    }

    private static int ApplyMipsVolumeScale13(int value)
    {
        return ApplySignedMagicScale(value, unchecked((int)0x80020009), 13);
    }

    private static int ApplyMipsVolumeScale11(int value)
    {
        return ApplySignedMagicScale(value, unchecked((int)0x8418828d), 11);
    }

    // MIPS uses signed multiply-high magic constants instead of direct division in the tone-mix path.
    private static int ApplySignedMagicScale(int value, int magic, int shift)
    {
        var product = (long)value * magic;
        var high = (int)(product >> 32);
        return ((high + value) >> shift) - (value >> 31);
    }

    private static int SquarePlusOne(int value)
    {
        var adjustedValue = value + 1;
        return adjustedValue * adjustedValue;
    }

    // GHIDRA: SetVoiceVolume @ 0x80095298
    private void SetVoiceVolume(short voiceId, short volumeLeft, short volumeRight)
    {
        if (_gameEngine.StaticVariables.g_voiceCommandLock == 1)
        {
            return;
        }

        if ((uint)voiceId >= _gameEngine.StaticVariables.g_spuVoiceVolumeLeft.Length)
        {
            return;
        }

        _gameEngine.StaticVariables.g_voiceCommandLock = 1;
        _gameEngine.StaticVariables.DAT_maybeCurrentVoiceIndex_801f76b2 = voiceId;
        _gameEngine.StaticVariables.g_spuVoiceVolumeLeft[voiceId] = volumeLeft;
        _gameEngine.StaticVariables.g_spuVoiceVolumeRight[voiceId] = volumeRight;
        _gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] = (byte)(_gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] | 0x03);
        _gameEngine.StaticVariables.g_voiceCommandLock = 0;
        PushTrackedVoiceStereoVolume(voiceId);
    }

    // GHIDRA: StopVoice @ 0x80094F20
    private void StopVoice(short voiceId)
    {
        if (_gameEngine.StaticVariables.g_voiceCommandLock == 1)
        {
            return;
        }

        if ((uint)voiceId >= _gameEngine.StaticVariables.g_voiceState.Length)
        {
            return;
        }

        _gameEngine.StaticVariables.g_voiceCommandLock = 1;
        _gameEngine.StaticVariables.DAT_maybeCurrentVoiceIndex_801f76b2 = voiceId;
        FUN_80091134(0);
        _gameEngine.StaticVariables.g_voiceState[voiceId] = 0;
        _gameEngine.StaticVariables.g_voiceSfxId[voiceId] = 0;
        _gameEngine.StaticVariables.g_voiceVabId[voiceId] = -2;
        _gameEngine.StaticVariables.g_voiceToneIndex[voiceId] = 0;
        _gameEngine.StaticVariables.g_voiceToneVolume[voiceId] = 0;
        _gameEngine.StaticVariables.g_voiceTonePan[voiceId] = 0;
        _gameEngine.StaticVariables.g_voiceRuntimeSlots[voiceId].NoiseState = 0;
        _gameEngine.SoundBin.StopTrackedVoice(voiceId);
        _gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] = (byte)(_gameEngine.StaticVariables.g_spuVoiceDirtyFlags[voiceId] | 0x03);
        _gameEngine.StaticVariables.g_voiceCommandLock = 0;
    }

    // GHIDRA: HandleMapSoundEffects @ 0x80049F1C
    public int HandleMapSoundEffects(uint mapId, uint soundEffectId)
    {
        lock (_soundTickGate)
        {
            return HandleMapSoundEffectsCore(mapId, soundEffectId);
        }
    }

    private int HandleMapSoundEffectsCore(uint mapId, uint soundEffectId)
    {
        ResetSoundEffectRuntime();

        if (SoundBin.SfxRecordsData[soundEffectId][0] == 0xFF &&
            SoundBin.SfxRecordsData[soundEffectId][6] == 0)
        {
            soundEffectId = 0;
        }

        var offset = _gameEngine.GetMapSoundIndex(mapId);

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

    // GHIDRA: ResetSoundEffectRuntime @ 0x80048E44
    private void ResetSoundEffectRuntime()
    {
        for (short voiceIndex = 0; voiceIndex < 0x18; voiceIndex++)
        {
            if (_gameEngine.StaticVariables.g_voiceState[voiceIndex] != 0 && _gameEngine.StaticVariables.g_voiceVabId[voiceIndex] != -2)
            {
                StopVoice(voiceIndex);
            }
        }

        EnsureSoundEffectDataInitialized();
        for (var sfxId = 0; sfxId < _gameEngine.StaticVariables.g_soundEffectData.Length; sfxId++)
        {
            ref var soundEffectRecord = ref _gameEngine.StaticVariables.g_soundEffectData[sfxId];
            soundEffectRecord.Flags = (short)(soundEffectRecord.Flags & ~0x0001);

            if (soundEffectRecord.SeqNum < 0 || (soundEffectRecord.Flags & 0x0002) == 0)
            {
                continue;
            }

            if ((uint)soundEffectRecord.SeqNum >= (uint)_gameEngine.StaticVariables.g_loadedSequenceHandles.Length)
            {
                continue;
            }

            var sequenceSlot = _gameEngine.StaticVariables.g_loadedSequenceHandles[soundEffectRecord.SeqNum];
            if (sequenceSlot < 0 || FUN_8008dd1c(sequenceSlot, 0) != 1)
            {
                continue;
            }

            InitializeBgm(sequenceSlot);
            ResetSomethingSound(sequenceSlot);
            soundEffectRecord.Flags = (short)(soundEffectRecord.Flags & ~0x0002);
        }
    }

    // GHIDRA: HandleMapSoundStreaming @ 0x8004B1D4
    public void HandleMapSoundStreaming()
    {
        lock (_soundTickGate)
        {
            HandleMapSoundStreamingCore();
        }
    }

    private void HandleMapSoundStreamingCore()
    {
        if (_gameEngine.StaticVariables.g_resetSoundFlag != 0)
        {
            StopAllSound();
            _gameEngine.StaticVariables.g_resetSoundFlag = 0;
        }

        FinalizeAudioBuffers();

        if (_gameEngine.StaticVariables.g_soundLoadState < 1 || _gameEngine.StaticVariables.g_soundLoadState > 5)
        {
            return;
        }

        var soundIndex = _gameEngine.StaticVariables.g_currentMapSoundIndex;
        var offsetIndex = soundIndex * 3;

        switch (_gameEngine.StaticVariables.g_soundLoadState)
        {
            case 1:
                InitializeBgm(_gameEngine.StaticVariables.g_requestedSeqId);
                ResetSomethingSound(_gameEngine.StaticVariables.g_requestedSeqId);
                FreeLoadedVab(_gameEngine.StaticVariables.g_currentVabId);

                _gameEngine.StaticVariables.g_vabBodyOffset = _gameEngine.SoundBin.MusicSeqVabOffsets[offsetIndex + 1];
                _gameEngine.StaticVariables.g_vabBodyRemainingSize = _gameEngine.SoundBin.MusicSeqVabOffsets[offsetIndex + 2] - _gameEngine.SoundBin.MusicSeqVabOffsets[offsetIndex + 1];

                var vabHeaderData = _gameEngine.SoundBin.ReadRange(_gameEngine.StaticVariables.g_vabBodyOffset, _gameEngine.StaticVariables.g_vabBodyRemainingSize);
                Array.Clear(_gameEngine.StaticVariables.DAT_8015b1a0);
                Array.Copy(vabHeaderData, _gameEngine.StaticVariables.DAT_8015b1a0, Math.Min(vabHeaderData.Length, _gameEngine.StaticVariables.DAT_8015b1a0.Length));
                _gameEngine.StaticVariables.g_soundLoadState = 2;
                break;

            case 2:
                _gameEngine.StaticVariables.g_currentVabId = LoadVabHeader(_gameEngine.StaticVariables.DAT_8015b1a0, _gameEngine.StaticVariables.g_currentVabId, 0x39040);
                if (_gameEngine.StaticVariables.g_currentVabId < 0)
                {
                    Breakpoint.TriggerBreak();
                    Debug.WriteLine("HandleMapSoundStreaming: failed to load VAB header");
                    _gameEngine.StaticVariables.g_soundLoadState = 0;
                    break;
                }

                _gameEngine.StaticVariables.g_soundLoadState = 3;
                break;

            case 3:
                _gameEngine.StaticVariables.g_vabBodyOffset = _gameEngine.SoundBin.MusicSeqVabOffsets[offsetIndex + 2];
                _gameEngine.StaticVariables.g_vabBodyRemainingSize = _gameEngine.SoundBin.MusicSeqVabOffsets[offsetIndex + 3] - _gameEngine.SoundBin.MusicSeqVabOffsets[offsetIndex + 2];

                if (_gameEngine.StaticVariables.g_vabBodyRemainingSize <= 0)
                {
                    Breakpoint.TriggerBreak();
                    Debug.WriteLine("HandleMapSoundStreaming: invalid VAB body size");
                    _gameEngine.StaticVariables.g_soundLoadState = 0;
                    break;
                }

                if ((uint)_gameEngine.StaticVariables.g_currentVabId < 0x10 && _gameEngine.StaticVariables.g_vabBodyRemainingSize > 0)
                {
                    _loadedVabBodies[_gameEngine.StaticVariables.g_currentVabId] = new byte[_gameEngine.StaticVariables.g_vabBodyRemainingSize];
                    _loadedVabBodyWriteOffsets[_gameEngine.StaticVariables.g_currentVabId] = 0;
                    _gameEngine.StaticVariables.g_loadedVabBodySizes[_gameEngine.StaticVariables.g_currentVabId] = _gameEngine.StaticVariables.g_vabBodyRemainingSize;
                }

                var initialChunkSize = Math.Min(0x8000, _gameEngine.StaticVariables.g_vabBodyRemainingSize);
                if (initialChunkSize > 0)
                {
                    var initialChunk = _gameEngine.SoundBin.ReadRange(_gameEngine.StaticVariables.g_vabBodyOffset, initialChunkSize);
                    Array.Clear(_gameEngine.StaticVariables.g_partialVabBodyBuffer);
                    Array.Copy(initialChunk, _gameEngine.StaticVariables.g_partialVabBodyBuffer, initialChunk.Length);
                }

                _gameEngine.StaticVariables.g_soundLoadState = 4;
                _gameEngine.StaticVariables.g_partialVabBodyLoadState = 0;
                _gameEngine.StaticVariables.g_vabBodyOffset += 0x8000;
                break;

            case 4:
                if (_gameEngine.StaticVariables.g_partialVabBodyLoadState == 1)
                {
                    if (_gameEngine.StaticVariables.g_vabBodyRemainingSize <= 0)
                    {
                        var sequenceOffset = _gameEngine.SoundBin.MusicSeqVabOffsets[offsetIndex];
                        var sequenceSize = _gameEngine.SoundBin.MusicSeqVabOffsets[offsetIndex + 1] - sequenceOffset;
                        var sequenceData = _gameEngine.SoundBin.ReadRange(sequenceOffset, sequenceSize);

                        if (sequenceData.Length > _gameEngine.StaticVariables.g_soundBinSequenceBuffer.Length)
                        {
                            _gameEngine.StaticVariables.g_soundBinSequenceBuffer = new byte[sequenceData.Length];
                        }

                        Array.Clear(_gameEngine.StaticVariables.g_soundBinSequenceBuffer);
                        Array.Copy(sequenceData, _gameEngine.StaticVariables.g_soundBinSequenceBuffer, sequenceData.Length);
                        _gameEngine.StaticVariables.g_soundLoadState = 5;
                    }
                    else
                    {
                        var nextChunkSize = Math.Min(0x8000, _gameEngine.StaticVariables.g_vabBodyRemainingSize);
                        var nextChunk = _gameEngine.SoundBin.ReadRange(_gameEngine.StaticVariables.g_vabBodyOffset, nextChunkSize);
                        Array.Clear(_gameEngine.StaticVariables.g_partialVabBodyBuffer);
                        Array.Copy(nextChunk, _gameEngine.StaticVariables.g_partialVabBodyBuffer, nextChunk.Length);
                        _gameEngine.StaticVariables.g_partialVabBodyLoadState = 0;
                        _gameEngine.StaticVariables.g_vabBodyOffset += 0x8000;
                    }
                }
                else
                {
                    var chunkSize = _gameEngine.StaticVariables.g_vabBodyRemainingSize > 0x7FFF
                        ? 0x8000
                        : _gameEngine.StaticVariables.g_vabBodyRemainingSize;

                    var uploadResult = UploadVabBodyChunk(_gameEngine.StaticVariables.g_partialVabBodyBuffer, chunkSize, _gameEngine.StaticVariables.g_currentVabId);
                    if (uploadResult == -1)
                    {
                        Breakpoint.TriggerBreak();
                        Debug.WriteLine("HandleMapSoundStreaming: failed to upload VAB chunk");
                    }

                    _gameEngine.StaticVariables.g_partialVabBodyLoadState = 1;
                    _gameEngine.StaticVariables.g_vabBodyRemainingSize -= 0x8000;
                }

                break;

            case 5:
                _gameEngine.StaticVariables.g_requestedSeqId = LoadSeq(_gameEngine.StaticVariables.g_soundBinSequenceBuffer, _gameEngine.StaticVariables.g_currentVabId);
                SetSeqVolume(_gameEngine.StaticVariables.g_requestedSeqId, 0x7F, 0x7F);

                if (_gameEngine.StaticVariables.g_forceStopAllSound != 0)
                {
                    StopAllSound();
                }
                else if (_gameEngine.StaticVariables.IsBgmActivated)
                {
                    PlaySeq(_gameEngine.StaticVariables.g_requestedSeqId, 1, 1);
                }

                _gameEngine.StaticVariables.g_soundLoadState = 0;
                break;
        }
    }

    //8004b104
    public bool IsSoundLoading()
    {
        lock (_soundTickGate)
        {
            return _gameEngine.StaticVariables.g_soundLoadState != 0;
        }
    }
}