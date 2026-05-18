namespace AlundraEngine.Sound;

public interface ISoundPlaybackBackend
{
    // Returning true means the backend fully consumed the stream and the caller may dispose it.
    bool Play(Stream stream, int voiceId);
    void Stop(int voiceId);
}