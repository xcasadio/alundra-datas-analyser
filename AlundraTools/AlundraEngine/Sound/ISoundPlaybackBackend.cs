namespace AlundraEngine.Sound;

public interface ISoundPlaybackBackend
{
    // Returning true means the backend fully consumed the stream and the caller may dispose it.
    bool Play(Stream stream, int voiceId, bool shouldLoop, int loopStartSample, int loopEndSample);
    void Stop(int voiceId);
    bool IsPlaying(int voiceId);
    void UpdateVoiceStereoVolume(int voiceId, short volumeLeft, short volumeRight);
    void UpdateVoicePitch(int voiceId, short pitch);
    // RELATION: adapter for the SPU per-voice reverb enable bits (SpuSetReverbVoice observable contract).
    void UpdateVoiceReverb(int voiceId, bool reverbEnabled);
    // RELATION: adapter for the SpuSetReverb on/off observable contract.
    void SetReverbEnabled(bool enabled);
    // RELATION: adapter for SpuSetReverbModeParam/SpuSetReverbDepth (mode word + vLOUT/vROUT).
    void SetReverbState(int mode, short depthLeft, short depthRight);
}