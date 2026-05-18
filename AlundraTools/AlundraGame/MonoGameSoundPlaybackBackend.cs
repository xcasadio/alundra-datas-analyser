using System.IO;
using Microsoft.Xna.Framework.Audio;
using AlundraEngine.Sound;

namespace AlundraGame;

public sealed class MonoGameSoundPlaybackBackend : ISoundPlaybackBackend
{
    private readonly SoundEffect?[] _voiceEffects = new SoundEffect[24];
    private readonly SoundEffectInstance?[] _voiceInstances = new SoundEffectInstance[24];
    private SoundEffect? _singleEffect;
    private SoundEffectInstance? _singleInstance;

    public bool Play(Stream stream, int voiceId)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var playbackStream = new MemoryStream();
        stream.CopyTo(playbackStream);
        playbackStream.Position = 0;

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        try
        {
            var effect = SoundEffect.FromStream(playbackStream);
            var instance = effect.CreateInstance();
            if ((uint)voiceId < (uint)_voiceInstances.Length)
            {
                Stop(voiceId);
                _voiceEffects[voiceId] = effect;
                _voiceInstances[voiceId] = instance;
            }
            else
            {
                Stop(-1);
                _singleEffect = effect;
                _singleInstance = instance;
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
}