using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using AlundraEngine.Sound;

namespace AlundraGame;

// JUSTIFICATION: backend MonoGame only
// RELATION: host output adapter for SpuMixerSoundPlaybackBackend. The mixer synthesizes the SPU
// observable contract (exact left/right gains, raw pitch step, sample-accurate loops, reverb);
// this wrapper only pushes its single 44100 Hz stereo s16 stream into one DynamicSoundEffectInstance,
// matching the PsyZ/SDL SPU output contract instead of one host instance per voice.
// StartTickDriver additionally adapts the original 60 Hz VSync/timer sound interrupt: a dedicated
// thread paced by the audio queue (one tick per 735-sample buffer) keeps the sequencer and the
// output running in real time even while the game thread blocks on map loading.
public sealed class MonoGameSoundPlaybackBackend : ISoundPlaybackBackend
{
    private const int BufferFrames = 735; // one 60 Hz tick at 44100 Hz
    private const int TargetQueuedBuffers = 4;

    private readonly SpuMixerSoundPlaybackBackend _mixer = new();
    private readonly object _outputLock = new();
    private readonly short[] _renderBuffer = new short[BufferFrames * 2];
    private readonly byte[] _submitBuffer = new byte[BufferFrames * 2 * sizeof(short)];
    private DynamicSoundEffectInstance? _output;
    private Thread? _tickThread;
    private Action? _soundTick;
    private volatile bool _tickThreadStopRequested;

    // JUSTIFICATION: backend MonoGame only
    // RELATION: desktop adaptation of the original 60 Hz sound interrupt. The callback is the
    // SoundManager tick (FUN_8008A718 @ 0x8008A718); pacing is locked to the audio clock by
    // submitting exactly one 1/60 s buffer per tick, with a wall-clock fallback when no audio
    // device is available.
    public void StartTickDriver(Action soundTick)
    {
        if (_tickThread != null)
        {
            return;
        }

        _soundTick = soundTick;
        _tickThreadStopRequested = false;
        _tickThread = new Thread(TickDriverLoop)
        {
            IsBackground = true,
            Name = "AlundraSoundTick",
            Priority = ThreadPriority.AboveNormal,
        };
        _tickThread.Start();
    }

    // JUSTIFICATION: backend MonoGame only
    public void StopTickDriver()
    {
        _tickThreadStopRequested = true;
        _tickThread?.Join(500);
        _tickThread = null;
    }

    // JUSTIFICATION: backend MonoGame only
    private void TickDriverLoop()
    {
        var fallbackPacer = Stopwatch.StartNew();
        var nextFallbackTick = 0.0;
        const double fallbackTickSeconds = 1.0 / 60.0;

        while (!_tickThreadStopRequested)
        {
            EnsureOutputStarted();

            DynamicSoundEffectInstance? output;
            lock (_outputLock)
            {
                output = _output;
            }

            if (output != null)
            {
                try
                {
                    if (output.PendingBufferCount >= TargetQueuedBuffers)
                    {
                        Thread.Sleep(2);
                        continue;
                    }
                }
                catch (ObjectDisposedException)
                {
                    lock (_outputLock)
                    {
                        _output = null;
                    }

                    continue;
                }

                _soundTick?.Invoke();
                SubmitOneBuffer();
                nextFallbackTick = fallbackPacer.Elapsed.TotalSeconds;
                continue;
            }

            // No audio device: keep the sound state ticking at wall-clock 60 Hz.
            _soundTick?.Invoke();
            nextFallbackTick += fallbackTickSeconds;
            var sleepSeconds = nextFallbackTick - fallbackPacer.Elapsed.TotalSeconds;
            if (sleepSeconds > 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(sleepSeconds));
            }
            else if (sleepSeconds < -1.0)
            {
                nextFallbackTick = fallbackPacer.Elapsed.TotalSeconds;
            }
        }
    }

    // JUSTIFICATION: backend MonoGame only
    private void SubmitOneBuffer()
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
                _mixer.RenderSamples(_renderBuffer, BufferFrames);
                Buffer.BlockCopy(_renderBuffer, 0, _submitBuffer, 0, _submitBuffer.Length);
                output.SubmitBuffer(_submitBuffer);
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

    public bool Play(Stream stream, int voiceId, bool shouldLoop, int loopStartSample, int loopEndSample)
    {
        if (!_mixer.Play(stream, voiceId, shouldLoop, loopStartSample, loopEndSample))
        {
            return false;
        }

        if (_tickThread == null)
        {
            EnsureOutputStarted();
            TopUpOutput();
        }

        return true;
    }

    public void Stop(int voiceId)
    {
        _mixer.Stop(voiceId);
    }

    public bool IsPlaying(int voiceId)
    {
        if (_tickThread == null)
        {
            TopUpOutput();
        }

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
                if (_tickThread == null)
                {
                    // Without a tick driver the main thread keeps the queue filled; with the
                    // driver, ticking and submitting are paired on the driver thread and the
                    // BufferNeeded pump must not refill the queue tick-free.
                    output.BufferNeeded += (_, _) => TopUpOutput();
                }

                _output = output;
            }
            catch
            {
                _output = null;
                return;
            }
        }

        if (_tickThread == null)
        {
            TopUpOutput();
        }

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
