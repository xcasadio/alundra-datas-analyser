using System;
using AlundraEngine.Loader;
using Microsoft.Xna.Framework.Audio;

namespace AlundraGame;

// JUSTIFICATION: backend MonoGame only
// RELATION: host output adapter for movie audio. Separate from MonoGameSoundPlaybackBackend, which
// owns the SPU mixer's single 44100 Hz stream for music and sound effects: the loader's movies run
// before the game's sound driver is up and carry their own 37800 Hz XA stream, so they get their
// own DynamicSoundEffectInstance rather than being resampled through the mixer.
public sealed class MonoGameMovieAudioOutput : IMovieAudioOutput, IDisposable
{
    // Roughly 1/15 s at 37800 Hz stereo, i.e. one movie frame's worth of audio per buffer.
    private const int SubmitFrames = 2520;

    private readonly object _lock = new();
    private DynamicSoundEffectInstance? _output;
    private byte[] _submitBuffer = new byte[SubmitFrames * 2 * sizeof(short)];
    private short[] _pending = new short[SubmitFrames * 2];
    private int _pendingCount;
    private int _channels = 2;
    private float _volume = 1f;
    private bool _started;

    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0f, 1f);
            lock (_lock)
            {
                if (_output != null)
                {
                    _output.Volume = _volume;
                }
            }
        }
    }

    public int PendingBufferCount
    {
        get
        {
            lock (_lock)
            {
                return _output?.PendingBufferCount ?? 0;
            }
        }
    }

    public void Start(int sampleRate, int channels)
    {
        lock (_lock)
        {
            StopCore();

            _channels = channels;
            _pendingCount = 0;
            _started = false;

            var required = SubmitFrames * channels;
            if (_pending.Length < required)
            {
                _pending = new short[required];
                _submitBuffer = new byte[required * sizeof(short)];
            }

            try
            {
                _output = new DynamicSoundEffectInstance(
                    sampleRate,
                    channels == 1 ? AudioChannels.Mono : AudioChannels.Stereo)
                {
                    Volume = _volume,
                };
            }
            catch (Exception)
            {
                // No audio device, or an unsupported rate: play the movie silently rather than
                // taking the whole boot sequence down with it.
                _output = null;
            }
        }
    }

    public void Submit(short[] samples, int offset, int count)
    {
        lock (_lock)
        {
            if (_output == null)
            {
                return;
            }

            var chunk = SubmitFrames * _channels;
            for (var i = 0; i < count; i++)
            {
                _pending[_pendingCount++] = samples[offset + i];
                if (_pendingCount < chunk)
                {
                    continue;
                }

                Buffer.BlockCopy(_pending, 0, _submitBuffer, 0, _pendingCount * sizeof(short));
                _output.SubmitBuffer(_submitBuffer, 0, _pendingCount * sizeof(short));
                _pendingCount = 0;
            }

            // Wait for a small cushion before starting so the first buffers do not underrun while
            // the decoder is still filling the pipeline.
            if (!_started && _output.PendingBufferCount >= 2)
            {
                _output.Play();
                _started = true;
            }
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            StopCore();
        }
    }

    private void StopCore()
    {
        if (_output == null)
        {
            return;
        }

        try
        {
            _output.Stop();
        }
        catch (Exception)
        {
            // Disposing below is what matters; a device that already went away can throw here.
        }

        _output.Dispose();
        _output = null;
        _started = false;
        _pendingCount = 0;
    }

    public void Dispose() => Stop();
}
