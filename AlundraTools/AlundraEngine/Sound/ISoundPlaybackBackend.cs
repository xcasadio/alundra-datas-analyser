namespace AlundraEngine.Sound;

public interface ISoundPlaybackBackend
{
    // Returning true means the backend fully consumed the stream and the caller may dispose it.
    bool Play(Stream stream, int voiceId, bool shouldLoop, int loopStartSample, int loopEndSample);
    void Stop(int voiceId);
    bool IsPlaying(int voiceId);
    void UpdateVoiceStereoVolume(int voiceId, short volumeLeft, short volumeRight);
    void UpdateVoicePitch(int voiceId, short pitch);
}