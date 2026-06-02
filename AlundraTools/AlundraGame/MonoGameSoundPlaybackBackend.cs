using System;
using System.Buffers.Binary;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using AlundraEngine.Sound;

namespace AlundraGame;

public sealed class MonoGameSoundPlaybackBackend : ISoundPlaybackBackend
{
    private readonly SoundEffect?[] _voiceEffects = new SoundEffect[24];
    private readonly SoundEffectInstance?[] _voiceInstances = new SoundEffectInstance[24];
    private readonly EventHandler<EventArgs>?[] _voiceDynamicBufferNeededHandlers = new EventHandler<EventArgs>?[24];
    private readonly short[] _voicePitches = new short[24];
    private readonly short[] _voiceBasePitches = new short[24];
    private readonly bool[] _voicePitchInitialized = new bool[24];
    private SoundEffect? _singleEffect;
    private SoundEffectInstance? _singleInstance;
    private EventHandler<EventArgs>? _singleDynamicBufferNeededHandler;
    private short _singlePitch;
    private short _singleBasePitch;
    private bool _singlePitchInitialized;

    public bool Play(Stream stream, int voiceId, bool shouldLoop, int loopStartSample, int loopEndSample)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var playbackStream = new MemoryStream();
        stream.CopyTo(playbackStream);
        var waveData = playbackStream.ToArray();

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        var waveBasePitch = TryGetWaveBasePitch(waveData, out var parsedWaveBasePitch)
            ? parsedWaveBasePitch
            : (short)0;

        if (TryCreateDynamicInstance(waveData, shouldLoop, loopStartSample, loopEndSample, out var dynamicInstance, out var dynamicBufferNeededHandler) && dynamicInstance != null)
        {
            if ((uint)voiceId < (uint)_voiceInstances.Length)
            {
                Stop(voiceId);
                _voiceEffects[voiceId] = null;
                _voiceInstances[voiceId] = dynamicInstance;
                _voiceDynamicBufferNeededHandlers[voiceId] = dynamicBufferNeededHandler;
                if (_voicePitchInitialized[voiceId])
                {
                    _voiceBasePitches[voiceId] = waveBasePitch != 0 ? waveBasePitch : _voicePitches[voiceId];
                    ApplyVoicePitch(dynamicInstance, _voicePitches[voiceId], _voiceBasePitches[voiceId]);
                }
            }
            else
            {
                Stop(-1);
                _singleEffect = null;
                _singleInstance = dynamicInstance;
                _singleDynamicBufferNeededHandler = dynamicBufferNeededHandler;
                if (_singlePitchInitialized)
                {
                    _singleBasePitch = waveBasePitch != 0 ? waveBasePitch : _singlePitch;
                    ApplyVoicePitch(dynamicInstance, _singlePitch, _singleBasePitch);
                }
            }

            dynamicInstance.Play();
            return true;
        }

        try
        {
            using var fallbackPlaybackStream = new MemoryStream(waveData, writable: false);
            var effect = SoundEffect.FromStream(fallbackPlaybackStream);
            var instance = effect.CreateInstance();
            instance.IsLooped = shouldLoop;
            if ((uint)voiceId < (uint)_voiceInstances.Length)
            {
                Stop(voiceId);
                _voiceEffects[voiceId] = effect;
                _voiceInstances[voiceId] = instance;
                _voiceDynamicBufferNeededHandlers[voiceId] = null;
                if (_voicePitchInitialized[voiceId])
                {
                    _voiceBasePitches[voiceId] = waveBasePitch != 0 ? waveBasePitch : _voicePitches[voiceId];
                    ApplyVoicePitch(instance, _voicePitches[voiceId], _voiceBasePitches[voiceId]);
                }
            }
            else
            {
                Stop(-1);
                _singleEffect = effect;
                _singleInstance = instance;
                _singleDynamicBufferNeededHandler = null;
                if (_singlePitchInitialized)
                {
                    _singleBasePitch = waveBasePitch != 0 ? waveBasePitch : _singlePitch;
                    ApplyVoicePitch(instance, _singlePitch, _singleBasePitch);
                }
            }

            instance.Play();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Stop(int voiceId)
    {
        if ((uint)voiceId < (uint)_voiceInstances.Length)
        {
            var voiceInstance = _voiceInstances[voiceId];
            if (voiceInstance != null)
            {
                if (voiceInstance is DynamicSoundEffectInstance dynamicVoiceInstance)
                {
                    var dynamicBufferNeededHandler = _voiceDynamicBufferNeededHandlers[voiceId];
                    if (dynamicBufferNeededHandler != null)
                    {
                        dynamicVoiceInstance.BufferNeeded -= dynamicBufferNeededHandler;
                        _voiceDynamicBufferNeededHandlers[voiceId] = null;
                    }
                }

                voiceInstance.Stop();
                voiceInstance.Dispose();
                _voiceInstances[voiceId] = null;
            }

            var voiceEffect = _voiceEffects[voiceId];
            if (voiceEffect != null)
            {
                voiceEffect.Dispose();
                _voiceEffects[voiceId] = null;
            }

            return;
        }

        if (_singleInstance != null)
        {
            if (_singleInstance is DynamicSoundEffectInstance dynamicSingleInstance)
            {
                if (_singleDynamicBufferNeededHandler != null)
                {
                    dynamicSingleInstance.BufferNeeded -= _singleDynamicBufferNeededHandler;
                    _singleDynamicBufferNeededHandler = null;
                }
            }

            _singleInstance.Stop();
            _singleInstance.Dispose();
            _singleInstance = null;
        }

        if (_singleEffect != null)
        {
            _singleEffect.Dispose();
            _singleEffect = null;
        }
    }

    public bool IsPlaying(int voiceId)
    {
        if ((uint)voiceId < (uint)_voiceInstances.Length)
        {
            return _voiceInstances[voiceId]?.State == SoundState.Playing;
        }

        return _singleInstance?.State == SoundState.Playing;
    }

    // JUSTIFICATION: backend MonoGame only
    // RELATION: adapter for raw SPU left/right voice volume observable contract
    public void UpdateVoiceStereoVolume(int voiceId, short volumeLeft, short volumeRight)
    {
        if ((uint)voiceId >= (uint)_voiceInstances.Length)
        {
            return;
        }

        var voiceInstance = _voiceInstances[voiceId];
        if (voiceInstance == null)
        {
            return;
        }

        ApplyStereoVolumes(voiceInstance, volumeLeft, volumeRight);
    }

    // JUSTIFICATION: backend MonoGame only
    // RELATION: adapter from SPU left/right voice registers to MonoGame volume/pan controls
     public void UpdateVoicePitch(int voiceId, short pitch)
     {
         if ((uint)voiceId < (uint)_voiceInstances.Length)
         {
             _voicePitches[voiceId] = pitch;
             _voicePitchInitialized[voiceId] = true;

             var voiceInstance = _voiceInstances[voiceId];
             if (voiceInstance != null)
             {
                 ApplyVoicePitch(voiceInstance, pitch, _voiceBasePitches[voiceId]);
             }

             return;
         }

         _singlePitch = pitch;
         _singlePitchInitialized = true;

         if (_singleInstance != null)
         {
             ApplyVoicePitch(_singleInstance, pitch, _singleBasePitch);
         }
     }

     // JUSTIFICATION: backend MonoGame only
     private static void ApplyStereoVolumes(SoundEffectInstance instance, short volumeLeft, short volumeRight)
    {
        var left = volumeLeft;
        if (left < 0)
        {
            left = 0;
        }

        var right = volumeRight;
        if (right < 0)
        {
            right = 0;
        }

        var maxVolume = left;
        if (maxVolume < right)
        {
            maxVolume = right;
        }

        if (maxVolume <= 0)
        {
            instance.Volume = 0f;
            instance.Pan = 0f;
            return;
        }

        var normalizedVolume = maxVolume / 16383f;
        if (normalizedVolume > 1f)
        {
            normalizedVolume = 1f;
        }

        var pan = 0f;
        var sum = left + right;
        if (sum > 0)
        {
            pan = (right - left) / (float)sum;
            if (pan < -1f)
            {
                pan = -1f;
            }
            else if (pan > 1f)
            {
                pan = 1f;
            }
        }

        instance.Volume = normalizedVolume;
        instance.Pan = pan;
    }

    // JUSTIFICATION: backend MonoGame only
    private static void ApplyVoicePitch(SoundEffectInstance instance, short pitch, short basePitch)
    {
        var rawPitch = unchecked((ushort)pitch);
        var rawBasePitch = unchecked((ushort)basePitch);
        if (rawPitch == 0 || rawBasePitch == 0)
        {
            instance.Pitch = 0f;
            return;
        }

        var relativePitch = rawPitch / (double)rawBasePitch;
        var monoGamePitch = (float)(Math.Log(relativePitch) / Math.Log(2.0));
        if (monoGamePitch < -1f)
        {
            monoGamePitch = -1f;
        }
        else if (monoGamePitch > 1f)
        {
            monoGamePitch = 1f;
        }

        instance.Pitch = monoGamePitch;
    }

    // JUSTIFICATION: backend MonoGame only
    // RELATION: derive the raw SPU pitch represented by the desktop WAV sample rate so clamped PCM rates keep the original pitch ratio.
    private static bool TryGetWaveBasePitch(byte[] waveData, out short basePitch)
    {
        basePitch = 0;
        if (!TryParsePcmWave(waveData, out var sampleRate, out _, out _, out _, out _) || sampleRate <= 0)
        {
            return false;
        }

        var rawPitch = ((sampleRate * 0x1000) + 22050) / 44100;
        if (rawPitch <= 0)
        {
            return false;
        }

        if (rawPitch > ushort.MaxValue)
        {
            rawPitch = ushort.MaxValue;
        }

        basePitch = unchecked((short)(ushort)rawPitch);
        return true;
    }

    // JUSTIFICATION: backend MonoGame only
    // RELATION: adapter from WAV stream generated by SoundBin to raw PCM submission for DynamicSoundEffectInstance
    private static bool TryCreateDynamicInstance(byte[] waveData, bool shouldLoop, int loopStartSample, int loopEndSample, out SoundEffectInstance? instance, out EventHandler<EventArgs>? dynamicBufferNeededHandler)
    {
        instance = null;
        dynamicBufferNeededHandler = null;

        if (!TryParsePcmWave(waveData, out var sampleRate, out var channels, out var bitsPerSample, out var dataOffset, out var dataLength))
        {
            return false;
        }

        if (bitsPerSample != 16)
        {
            return false;
        }

        DynamicSoundEffectInstance? dynamicInstance = null;
        try
        {
            dynamicInstance = new DynamicSoundEffectInstance(sampleRate, channels);
            if (shouldLoop && TryGetLoopByteRange(channels, bitsPerSample, dataOffset, dataLength, loopStartSample, loopEndSample, out var firstLength, out var loopOffset, out var loopLength))
            {
                SubmitDynamicBuffer(dynamicInstance, waveData, dataOffset, firstLength);
                SubmitDynamicBuffer(dynamicInstance, waveData, loopOffset, loopLength);

                dynamicBufferNeededHandler = (_, _) =>
                {
                    try
                    {
                        SubmitDynamicBuffer(dynamicInstance, waveData, loopOffset, loopLength);
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    catch (InvalidOperationException)
                    {
                    }
                };
                dynamicInstance.BufferNeeded += dynamicBufferNeededHandler;

                instance = dynamicInstance;
                return true;
            }

            SubmitDynamicBuffer(dynamicInstance, waveData, dataOffset, dataLength);

            if (shouldLoop)
            {
                dynamicBufferNeededHandler = (_, _) =>
                {
                    try
                    {
                        SubmitDynamicBuffer(dynamicInstance, waveData, dataOffset, dataLength);
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                    catch (InvalidOperationException)
                    {
                    }
                };
                dynamicInstance.BufferNeeded += dynamicBufferNeededHandler;
                SubmitDynamicBuffer(dynamicInstance, waveData, dataOffset, dataLength);
            }

            instance = dynamicInstance;
            return true;
        }
        catch
        {
            if (dynamicInstance != null)
            {
                if (dynamicBufferNeededHandler != null)
                {
                    dynamicInstance.BufferNeeded -= dynamicBufferNeededHandler;
                }

                dynamicInstance.Dispose();
            }

            instance = null;
            dynamicBufferNeededHandler = null;
            return false;
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private static void SubmitDynamicBuffer(DynamicSoundEffectInstance dynamicInstance, byte[] waveData, int dataOffset, int dataLength)
    {
        dynamicInstance.SubmitBuffer(waveData, dataOffset, dataLength);
    }

    // JUSTIFICATION: backend MonoGame only
    // RELATION: adapter from PSX ADPCM loop-start/end sample indices to DynamicSoundEffectInstance byte ranges
    private static bool TryGetLoopByteRange(AudioChannels channels, int bitsPerSample, int dataOffset, int dataLength, int loopStartSample, int loopEndSample, out int firstLength, out int loopOffset, out int loopLength)
    {
        firstLength = 0;
        loopOffset = 0;
        loopLength = 0;

        if (loopStartSample < 0 || loopEndSample < loopStartSample)
        {
            return false;
        }

        var channelCount = channels == AudioChannels.Stereo ? 2 : 1;
        var bytesPerFrame = channelCount * (bitsPerSample >> 3);
        if (bytesPerFrame <= 0)
        {
            return false;
        }

        var loopStartByteOffset = loopStartSample * bytesPerFrame;
        var loopEndExclusiveByteOffset = (loopEndSample + 1) * bytesPerFrame;
        if (loopStartByteOffset < 0 || loopEndExclusiveByteOffset <= loopStartByteOffset || loopStartByteOffset >= dataLength)
        {
            return false;
        }

        if (loopEndExclusiveByteOffset > dataLength)
        {
            loopEndExclusiveByteOffset = dataLength;
        }

        firstLength = loopEndExclusiveByteOffset;
        loopOffset = dataOffset + loopStartByteOffset;
        loopLength = loopEndExclusiveByteOffset - loopStartByteOffset;
        return firstLength > 0 && loopLength > 0;
    }

    // JUSTIFICATION: backend MonoGame only
    // RELATION: adapter for SoundBin-authored RIFF/WAV payloads
    private static bool TryParsePcmWave(byte[] waveData, out int sampleRate, out AudioChannels channels, out int bitsPerSample, out int dataOffset, out int dataLength)
    {
        sampleRate = 0;
        channels = AudioChannels.Mono;
        bitsPerSample = 0;
        dataOffset = 0;
        dataLength = 0;

        if (waveData.Length < 12)
        {
            return false;
        }

        const uint riffTag = 0x46464952;
        const uint waveTag = 0x45564157;
        const uint fmtTag = 0x20746d66;
        const uint dataTag = 0x61746164;

        if (BinaryPrimitives.ReadUInt32LittleEndian(waveData.AsSpan(0, 4)) != riffTag ||
            BinaryPrimitives.ReadUInt32LittleEndian(waveData.AsSpan(8, 4)) != waveTag)
        {
            return false;
        }

        ushort channelCount = 0;
        var foundFmt = false;
        var cursor = 12;
        while (cursor + 8 <= waveData.Length)
        {
            var chunkTag = BinaryPrimitives.ReadUInt32LittleEndian(waveData.AsSpan(cursor, 4));
            var chunkSize = (int)BinaryPrimitives.ReadUInt32LittleEndian(waveData.AsSpan(cursor + 4, 4));
            cursor += 8;
            if (chunkSize < 0 || cursor + chunkSize > waveData.Length)
            {
                return false;
            }

            if (chunkTag == fmtTag)
            {
                if (chunkSize < 16)
                {
                    return false;
                }

                var formatTag = BinaryPrimitives.ReadUInt16LittleEndian(waveData.AsSpan(cursor, 2));
                channelCount = BinaryPrimitives.ReadUInt16LittleEndian(waveData.AsSpan(cursor + 2, 2));
                sampleRate = (int)BinaryPrimitives.ReadUInt32LittleEndian(waveData.AsSpan(cursor + 4, 4));
                bitsPerSample = BinaryPrimitives.ReadUInt16LittleEndian(waveData.AsSpan(cursor + 14, 2));
                if (formatTag != 1)
                {
                    return false;
                }

                foundFmt = true;
            }
            else if (chunkTag == dataTag)
            {
                dataOffset = cursor;
                dataLength = chunkSize;
                break;
            }

            cursor += chunkSize;
            if ((chunkSize & 1) != 0)
            {
                cursor++;
            }
        }

        if (!foundFmt || dataLength <= 0)
        {
            return false;
        }

        channels = channelCount == 2 ? AudioChannels.Stereo : AudioChannels.Mono;
        return channelCount == 1 || channelCount == 2;
    }
}