using System;
using System.IO;

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

    // GHIDRA: SetCdToAranXaMusicIndex @ 0x8005ABE0
    public void SetCdToAranXaMusicIndex(int mode)
    {
        if ((_gameEngine.StaticVariables.g_isCdResetRequested != 0) ||
            (_gameEngine.StaticVariables.g_cdIsReady != 0 && _gameEngine.StaticVariables.g_cdDataLoaded == 0))
        {
            _gameEngine.StaticVariables.g_cdDataStartPtr = _gameEngine.StaticVariables.DAT_CDAranXa_pos + _gameEngine.StaticVariables.g_mapCdDataOffsets[mode * 3];
            PlayAranXaMusicIndexDesktopAdapter(mode);
        }
    }

    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: adapter for CdlSetloc + CdlReadS XA-ADPCM playback from extracted ARAN_XA.XA data
    private void PlayAranXaMusicIndexDesktopAdapter(int mode)
    {
        var tableBase = mode * 3;
        var startBlock = _gameEngine.StaticVariables.g_mapCdDataOffsets[tableBase];
        var channel = _gameEngine.StaticVariables.g_mapCdDataOffsets[tableBase + 1];
        var blockCount = _gameEngine.StaticVariables.g_mapCdDataOffsets[tableBase + 2];
        var dataFolder = Path.GetDirectoryName(_gameEngine.DatasBin.Binfile);
        if (dataFolder == null)
        {
            return;
        }

        var xaFile = Path.GetFullPath(Path.Combine(dataFolder, "..", "ARAN_XA.XA"));
        if (!File.Exists(xaFile))
        {
            return;
        }

        var waveStream = DecodeAranXaMusicIndexToWave(xaFile, startBlock, channel, blockCount);
        if (waveStream == null)
        {
            return;
        }

        _gameEngine.SoundBin.PlayWave(waveStream, -1, 0x1000);
    }

    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: adapter for extracted raw 0x800-byte ARAN_XA.XA blocks that contain 0x80-byte XA-ADPCM groups
    private static MemoryStream? DecodeAranXaMusicIndexToWave(string xaFile, int startBlock, int channel, int blockCount)
    {
        const int sourceBlockSize = 0x800;
        const int xaInterleave = 8;
        const int xaGroupSize = 0x80;
        const int xaGroupsPerSourceBlock = sourceBlockSize / xaGroupSize;
        const int xaBlocksPerGroup = 4;
        const int xaSamplesPerBlock = 28;
        const int channelCount = 2;
        const int bitsPerSample = 16;
        const int sampleRate = 37800;

        if (startBlock < 0 || channel < 0 || blockCount <= 0)
        {
            return null;
        }

        var firstPhysicalBlock = startBlock + channel;
        var lastPhysicalBlock = firstPhysicalBlock + (blockCount - 1) * xaInterleave;
        var fileLength = new FileInfo(xaFile).Length;
        if (firstPhysicalBlock < 0 || lastPhysicalBlock < firstPhysicalBlock || (long)lastPhysicalBlock * sourceBlockSize + sourceBlockSize > fileLength)
        {
            return null;
        }

        var totalSampleFrames = blockCount * xaGroupsPerSourceBlock * xaBlocksPerGroup * xaSamplesPerBlock;
        var pcmLength = totalSampleFrames * channelCount * (bitsPerSample >> 3);
        var pcm = new byte[pcmLength];
        var pcmPosition = 0;
        var group = new byte[xaGroupSize];
        var oldLeft = 0;
        var olderLeft = 0;
        var oldRight = 0;
        var olderRight = 0;

        using var stream = File.OpenRead(xaFile);
        for (var blockIndex = 0; blockIndex < blockCount; blockIndex++)
        {
            var physicalBlock = firstPhysicalBlock + blockIndex * xaInterleave;
            stream.Position = (long)physicalBlock * sourceBlockSize;

            for (var groupIndex = 0; groupIndex < xaGroupsPerSourceBlock; groupIndex++)
            {
                var bytesRead = stream.Read(group, 0, group.Length);
                if (bytesRead != group.Length)
                {
                    return null;
                }

                DecodeXaAdpcmGroup(group, pcm, ref pcmPosition, ref oldLeft, ref olderLeft, ref oldRight, ref olderRight);
            }
        }

        return WriteStereoPcmWave(pcm, sampleRate);
    }

    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: CD-XA 4-bit stereo ADPCM sound-group decoder used by the desktop CdlReadS adapter
    private static void DecodeXaAdpcmGroup(byte[] group, byte[] pcm, ref int pcmPosition, ref int oldLeft, ref int olderLeft, ref int oldRight, ref int olderRight)
    {
        for (var block = 0; block < 4; block++)
        {
            for (var sampleIndex = 0; sampleIndex < 28; sampleIndex++)
            {
                var data = group[0x10 + block + sampleIndex * 4];
                var left = DecodeXaAdpcmNibble(group[4 + block * 2], data & 0x0F, ref oldLeft, ref olderLeft);
                var right = DecodeXaAdpcmNibble(group[5 + block * 2], data >> 4, ref oldRight, ref olderRight);

                pcm[pcmPosition++] = (byte)(left & 0xFF);
                pcm[pcmPosition++] = (byte)((left >> 8) & 0xFF);
                pcm[pcmPosition++] = (byte)(right & 0xFF);
                pcm[pcmPosition++] = (byte)((right >> 8) & 0xFF);
            }
        }
    }

    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: XA-ADPCM uses the same predictor contract as PSX ADPCM with XA nibble/header layout
    private static short DecodeXaAdpcmNibble(byte header, int nibble, ref int oldSample, ref int olderSample)
    {
        var shift = header & 0x0F;
        if (shift > 12)
        {
            shift = 9;
        }

        var filterIndex = (header >> 4) & 0x03;
        var filterPos = filterIndex switch
        {
            1 => 60,
            2 => 115,
            3 => 98,
            _ => 0
        };
        var filterNeg = filterIndex switch
        {
            2 => -52,
            3 => -55,
            _ => 0
        };
        var rawSample = (sbyte)(nibble << 4) >> 4;
        var shiftedSample = rawSample << (12 - shift);
        var filteredSample = shiftedSample + (oldSample * filterPos + olderSample * filterNeg + 32) / 64;
        var clampedSample = (short)(filteredSample < short.MinValue ? short.MinValue : filteredSample > short.MaxValue ? short.MaxValue : filteredSample);
        olderSample = oldSample;
        oldSample = clampedSample;
        return clampedSample;
    }

    // JUSTIFICATION: PSX hardware adaptation only
    // RELATION: packages decoded CD-XA stereo PCM for the existing desktop playback backend
    private static MemoryStream WriteStereoPcmWave(byte[] pcm, int sampleRate)
    {
        const short channelCount = 2;
        const short bitsPerSample = 16;
        var byteRate = sampleRate * channelCount * bitsPerSample / 8;
        var blockAlign = (short)(channelCount * bitsPerSample / 8);
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);

        writer.Write((byte)'R');
        writer.Write((byte)'I');
        writer.Write((byte)'F');
        writer.Write((byte)'F');
        writer.Write(4 + 8 + 16 + 8 + pcm.Length);
        writer.Write((byte)'W');
        writer.Write((byte)'A');
        writer.Write((byte)'V');
        writer.Write((byte)'E');
        writer.Write((byte)'f');
        writer.Write((byte)'m');
        writer.Write((byte)'t');
        writer.Write((byte)' ');
        writer.Write(16);
        writer.Write((short)1);
        writer.Write(channelCount);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write(blockAlign);
        writer.Write(bitsPerSample);
        writer.Write((byte)'d');
        writer.Write((byte)'a');
        writer.Write((byte)'t');
        writer.Write((byte)'a');
        writer.Write(pcm.Length);
        writer.Write(pcm);
        stream.Position = 0;
        return stream;
    }
}