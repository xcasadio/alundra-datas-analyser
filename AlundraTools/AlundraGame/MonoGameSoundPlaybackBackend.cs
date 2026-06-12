using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using AlundraEngine.Sound;

namespace AlundraGame;

// JUSTIFICATION: backend MonoGame only
// RELATION: host output adapter for SpuMixerSoundPlaybackBackend. The mixer synthesizes the SPU
// observable contract (exact left/right gains, raw pitch step, sample-accurate loops, reverb);
// this wrapper only pushes its single 44100 Hz stereo s16 stream into one DynamicSoundEffectInstance,
// matching the PsyZ/SDL SPU output contract instead of one host instance per voice.
public sealed class MonoGameSoundPlaybackBackend : ISoundPlaybackBackend
{
    private const int BufferFrames = 735; // one 60 Hz tick at 44100 Hz
    private const int TargetQueuedBuffers = 4;

    private readonly SpuMixerSoundPlaybackBackend _mixer = new();
    private readonly object _outputLock = new();
    private readonly short[] _renderBuffer = new short[BufferFrames * 2];
    private readonly byte[] _submitBuffer = new byte[BufferFrames * 2 * sizeof(short)];
    private DynamicSoundEffectInstance? _output;

    public bool Play(Stream stream, int voiceId, bool shouldLoop, int loopStartSample, int loopEndSample)
    {
        if (!_mixer.Play(stream, voiceId, shouldLoop, loopStartSample, loopEndSample))
        {
            return false;
        }

        EnsureOutputStarted();
        TopUpOutput();
        return true;
    }

    public void Stop(int voiceId)
    {
        _mixer.Stop(voiceId);
    }

    public bool IsPlaying(int voiceId)
    {
        TopUpOutput();
        return _mixer.IsPlaying(voiceId);
    }

    public void UpdateVoiceStereoVolume(int voiceId, short volumeLeft, short volumeRight)
    {
        _mixer.UpdateVoiceStereoVolume(voiceId, volumeLeft, volumeRight);
    }

    public void UpdateVoicePitch(int voiceId, short pitch)
    {
        _mixer.UpdateVoicePitch(voiceId, pitch);
    }

    public void UpdateVoiceReverb(int voiceId, bool reverbEnabled)
    {
        _mixer.UpdateVoiceReverb(voiceId, reverbEnabled);
    }

    public void SetReverbEnabled(bool enabled)
    {
        _mixer.SetReverbEnabled(enabled);
    }

    public void SetReverbState(int mode, short depthLeft, short depthRight)
    {
        _mixer.SetReverbState(mode, depthLeft, depthRight);
    }

    // JUSTIFICATION: backend MonoGame only
    private void EnsureOutputStarted()
    {
        lock (_outputLock)
        {
            if (_output != null)
            {
                return;
            }

            try
            {
                var output = new DynamicSoundEffectInstance(SpuMixerSoundPlaybackBackend.OutputSampleRate, AudioChannels.Stereo);
                output.BufferNeeded += (_, _) => TopUpOutput();
                _output = output;
            }
            catch
            {
                _output = null;
                return;
            }
        }

        TopUpOutput();

        lock (_outputLock)
        {
            try
            {
                _output?.Play();
            }
            catch
            {
            }
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private void TopUpOutput()
    {
        lock (_outputLock)
        {
            var output = _output;
            if (output == null)
            {
                return;
            }

            try
            {
                while (output.PendingBufferCount < TargetQueuedBuffers)
                {
                    _mixer.RenderSamples(_renderBuffer, BufferFrames);
                    Buffer.BlockCopy(_renderBuffer, 0, _submitBuffer, 0, _submitBuffer.Length);
                    output.SubmitBuffer(_submitBuffer);
                }
            }
            catch (ObjectDisposedException)
            {
                _output = null;
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
