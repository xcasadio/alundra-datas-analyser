using System.Buffers.Binary;

namespace AlundraEngine.Sound;

// JUSTIFICATION: PSX hardware adaptation only
// RELATION: desktop synthesis of the raw SPU voice state staged by the transliterated runtime.
// The runtime already produces original-accurate SPU words (proven on BGM 21 voice 3 against PCSX-Redux:
// left/right volumes, pitch, ADSR1/ADSR2). This mixer consumes those words directly instead of mapping
// them onto per-voice MonoGame instances:
// - exact independent left/right SPU gains (raw 0..0x3FFF, /0x4000) instead of a host volume+pan law,
// - raw SPU pitch step (0x1000 = 1.0 at 44100 Hz) without the previous +/-1 octave host clamp,
// - sample-accurate VAG loop points instead of re-submitted byte buffers,
// - the SPU reverb unit (nocash psx-spx algorithm, 22050 Hz) fed by per-voice reverb enables, matching
//   InitializeSoundSystem @ 0x800484E8: mode 0x104 (clear work area + SPU_REV_MODE_STUDIO_C),
//   depth left/right 0x2A00, SpuSetReverbVoice(1, 0xFFFFFF), SpuSetReverb(1).
public sealed class SpuMixerSoundPlaybackBackend : ISoundPlaybackBackend
{
    public const int OutputSampleRate = 44100;
    private const int TrackedVoiceCount = 24;
    private const int VoiceSlotCount = TrackedVoiceCount + 1; // +1 untracked slot for voiceId outside 0..23

    private const int RawPitchOne = 0x1000;
    private const int VolumeShift = 14; // raw SPU direct volume 0x3FFF ~= 1.0

    private readonly object _renderLock = new();
    private readonly VoiceState[] _voices;

    private bool _reverbEnabled;
    private short _reverbDepthLeft;
    private short _reverbDepthRight;
    private short[] _reverbWork = new short[StudioLargeWorkAreaBytes / 2];
    private int _reverbPos;
    private bool _reverbTickToggle;
    private int _reverbLastLeft;
    private int _reverbLastRight;

    public SpuMixerSoundPlaybackBackend()
    {
        _voices = new VoiceState[VoiceSlotCount];
        for (var i = 0; i < _voices.Length; i++)
        {
            _voices[i] = new VoiceState();
        }
    }

    private sealed class VoiceState
    {
        public short[]? Pcm; // interleaved frames
        public int Channels;
        public int FrameCount;
        public int SourceSampleRate;
        public long PositionFp; // fixed point, RawPitchOne = one source frame
        public int StepFp;
        public bool Looping;
        public int LoopStartFrame;
        public int LoopEndFrameExclusive;
        public bool Active;
        public short VolumeLeft;
        public short VolumeRight;
        public short Pitch;
        public bool PitchInitialized;
        public bool ReverbEnabled;
    }

    private VoiceState GetVoiceSlot(int voiceId)
    {
        return (uint)voiceId < TrackedVoiceCount ? _voices[voiceId] : _voices[TrackedVoiceCount];
    }

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

        if (!TryParsePcmWaveToFrames(waveData, out var pcm, out var channels, out var sampleRate, out var frameCount))
        {
            return false;
        }

        lock (_renderLock)
        {
            var voice = GetVoiceSlot(voiceId);
            voice.Pcm = pcm;
            voice.Channels = channels;
            voice.FrameCount = frameCount;
            voice.SourceSampleRate = sampleRate;
            voice.PositionFp = 0;
            voice.Looping = shouldLoop;
            if (shouldLoop && loopStartSample >= 0 && loopEndSample >= loopStartSample && loopStartSample < frameCount)
            {
                voice.LoopStartFrame = loopStartSample;
                voice.LoopEndFrameExclusive = Math.Min(loopEndSample + 1, frameCount);
            }
            else
            {
                voice.LoopStartFrame = 0;
                voice.LoopEndFrameExclusive = frameCount;
            }

            voice.StepFp = CalculateVoiceStep(voice);
            voice.Active = frameCount > 0;
            return true;
        }
    }

    public void Stop(int voiceId)
    {
        lock (_renderLock)
        {
            var voice = GetVoiceSlot(voiceId);
            voice.Active = false;
            voice.Pcm = null;
        }
    }

    public bool IsPlaying(int voiceId)
    {
        lock (_renderLock)
        {
            return GetVoiceSlot(voiceId).Active;
        }
    }

    public void UpdateVoiceStereoVolume(int voiceId, short volumeLeft, short volumeRight)
    {
        lock (_renderLock)
        {
            var voice = GetVoiceSlot(voiceId);
            voice.VolumeLeft = volumeLeft > 0 ? volumeLeft : (short)0;
            voice.VolumeRight = volumeRight > 0 ? volumeRight : (short)0;
        }
    }

    public void UpdateVoicePitch(int voiceId, short pitch)
    {
        lock (_renderLock)
        {
            var voice = GetVoiceSlot(voiceId);
            voice.Pitch = pitch;
            voice.PitchInitialized = true;
            voice.StepFp = CalculateVoiceStep(voice);
        }
    }

    public void UpdateVoiceReverb(int voiceId, bool reverbEnabled)
    {
        lock (_renderLock)
        {
            GetVoiceSlot(voiceId).ReverbEnabled = reverbEnabled;
        }
    }

    public void SetReverbEnabled(bool enabled)
    {
        lock (_renderLock)
        {
            _reverbEnabled = enabled;
        }
    }

    public void SetReverbState(int mode, short depthLeft, short depthRight)
    {
        lock (_renderLock)
        {
            _reverbDepthLeft = depthLeft;
            _reverbDepthRight = depthRight;
            if ((mode & 0x100) != 0)
            {
                Array.Clear(_reverbWork, 0, _reverbWork.Length);
                _reverbPos = 0;
                _reverbLastLeft = 0;
                _reverbLastRight = 0;
            }
            // Only SPU_REV_MODE_STUDIO_C (4) registers are implemented; Alundra's
            // InitializeSoundSystem @ 0x800484E8 only ever selects mode 0x104.
        }
    }

    // RELATION: voices keyed on with a raw SPU pitch advance at pitch/0x1000 of the 44100 Hz VAG
    // nominal rate, exactly like the SPU pitch counter; the desktop WAV sample rate (which may have
    // been clamped for host playback) is intentionally ignored in that case.
    private static int CalculateVoiceStep(VoiceState voice)
    {
        if (voice.PitchInitialized && voice.Pitch != 0)
        {
            return (ushort)voice.Pitch;
        }

        if (voice.SourceSampleRate <= 0)
        {
            return RawPitchOne;
        }

        return (int)((long)voice.SourceSampleRate * RawPitchOne / OutputSampleRate);
    }

    // RELATION: one 44100 Hz stereo s16 output stream, the SPU output contract.
    public void RenderSamples(short[] interleavedStereo, int frameCount)
    {
        lock (_renderLock)
        {
            for (var frame = 0; frame < frameCount; frame++)
            {
                var mixLeft = 0;
                var mixRight = 0;
                var reverbInLeft = 0;
                var reverbInRight = 0;

                for (var voiceIndex = 0; voiceIndex < _voices.Length; voiceIndex++)
                {
                    var voice = _voices[voiceIndex];
                    if (!voice.Active || voice.Pcm == null)
                    {
                        continue;
                    }

                    SampleVoice(voice, out var sampleLeft, out var sampleRight);

                    var left = (sampleLeft * voice.VolumeLeft) >> VolumeShift;
                    var right = (sampleRight * voice.VolumeRight) >> VolumeShift;
                    mixLeft += left;
                    mixRight += right;
                    if (voice.ReverbEnabled)
                    {
                        reverbInLeft += left;
                        reverbInRight += right;
                    }

                    AdvanceVoice(voice);
                }

                if (_reverbEnabled)
                {
                    // The SPU reverb unit runs at 22050 Hz and holds its output for two 44100 Hz ticks.
                    _reverbTickToggle = !_reverbTickToggle;
                    if (_reverbTickToggle)
                    {
                        ReverbStep(Saturate(reverbInLeft), Saturate(reverbInRight));
                    }

                    mixLeft += _reverbLastLeft;
                    mixRight += _reverbLastRight;
                }

                interleavedStereo[frame * 2] = (short)Saturate(mixLeft);
                interleavedStereo[frame * 2 + 1] = (short)Saturate(mixRight);
            }
        }
    }

    private static void SampleVoiceFrame(VoiceState voice, int frameIndex, out int left, out int right)
    {
        var pcm = voice.Pcm!;
        if (voice.Channels == 2)
        {
            left = pcm[frameIndex * 2];
            right = pcm[frameIndex * 2 + 1];
            return;
        }

        left = pcm[frameIndex];
        right = left;
    }

    private static void SampleVoice(VoiceState voice, out int left, out int right)
    {
        var position = voice.PositionFp;
        var frameIndex = (int)(position >> 12);
        if (frameIndex >= voice.FrameCount)
        {
            frameIndex = voice.FrameCount - 1;
        }

        var fraction = (int)(position & 0xFFF);
        var nextFrameIndex = frameIndex + 1;
        if (voice.Looping && nextFrameIndex >= voice.LoopEndFrameExclusive)
        {
            nextFrameIndex = voice.LoopStartFrame;
        }
        else if (nextFrameIndex >= voice.FrameCount)
        {
            nextFrameIndex = frameIndex;
        }

        SampleVoiceFrame(voice, frameIndex, out var currentLeft, out var currentRight);
        SampleVoiceFrame(voice, nextFrameIndex, out var nextLeft, out var nextRight);

        left = currentLeft + (((nextLeft - currentLeft) * fraction) >> 12);
        right = currentRight + (((nextRight - currentRight) * fraction) >> 12);
    }

    private static void AdvanceVoice(VoiceState voice)
    {
        voice.PositionFp += voice.StepFp;
        if (voice.Looping)
        {
            var loopEndFp = (long)voice.LoopEndFrameExclusive << 12;
            if (voice.PositionFp >= loopEndFp)
            {
                var loopLengthFp = loopEndFp - ((long)voice.LoopStartFrame << 12);
                if (loopLengthFp <= 0)
                {
                    voice.PositionFp = (long)voice.LoopStartFrame << 12;
                    return;
                }

                voice.PositionFp = ((long)voice.LoopStartFrame << 12) + ((voice.PositionFp - loopEndFp) % loopLengthFp);
            }

            return;
        }

        if (voice.PositionFp >= (long)voice.FrameCount << 12)
        {
            voice.Active = false;
            voice.Pcm = null;
        }
    }

    private static int Saturate(int value)
    {
        if (value < short.MinValue)
        {
            return short.MinValue;
        }

        if (value > short.MaxValue)
        {
            return short.MaxValue;
        }

        return value;
    }

    // ---------------------------------------------------------------------
    // SPU reverb unit
    // RELATION: nocash psx-spx reverb formula with the SPU_REV_MODE_STUDIO_C ("Studio Large")
    // preset register file selected by Alundra's mode 0x104. Disp registers are in 8-byte units;
    // the work area advances one halfword per 22050 Hz tick and wraps inside the preset work size.
    // ---------------------------------------------------------------------
    private const int StudioLargeWorkAreaBytes = 0x6FE0;
    private const int RevDAPF1 = 0x00E3;
    private const int RevDAPF2 = 0x00A9;
    private const short RevVIIR = 0x6F60;
    private const short RevVCOMB1 = 0x4FA8;
    private const short RevVCOMB2 = unchecked((short)0xBCE0);
    private const short RevVCOMB3 = 0x4510;
    private const short RevVCOMB4 = unchecked((short)0xBEF0);
    private const short RevVWALL = unchecked((short)0xA680);
    private const short RevVAPF1 = 0x5680;
    private const short RevVAPF2 = 0x52C0;
    private const int RevMLSAME = 0x0DFB;
    private const int RevMRSAME = 0x0B58;
    private const int RevMLCOMB1 = 0x0D09;
    private const int RevMRCOMB1 = 0x0A3C;
    private const int RevMLCOMB2 = 0x0BD9;
    private const int RevMRCOMB2 = 0x0973;
    private const int RevDLSAME = 0x0B59;
    private const int RevDRSAME = 0x08DA;
    private const int RevMLDIFF = 0x08D9;
    private const int RevMRDIFF = 0x05E9;
    private const int RevMLCOMB3 = 0x07EC;
    private const int RevMRCOMB3 = 0x04B0;
    private const int RevMLCOMB4 = 0x06EF;
    private const int RevMRCOMB4 = 0x03D2;
    private const int RevDLDIFF = 0x05EA;
    private const int RevDRDIFF = 0x031D;
    private const int RevMLAPF1 = 0x031C;
    private const int RevMRAPF1 = 0x0238;
    private const int RevMLAPF2 = 0x0154;
    private const int RevMRAPF2 = 0x00AA;
    private const short RevVLIN = unchecked((short)0x8000);
    private const short RevVRIN = unchecked((short)0x8000);

    private static int MulVol(int sample, int volume)
    {
        return (sample * volume) >> 15;
    }

    private int ReverbRead(int dispHalfwords)
    {
        var index = _reverbPos + dispHalfwords;
        var size = _reverbWork.Length;
        index %= size;
        if (index < 0)
        {
            index += size;
        }

        return _reverbWork[index];
    }

    private void ReverbWrite(int dispHalfwords, int value)
    {
        var index = (_reverbPos + dispHalfwords) % _reverbWork.Length;
        _reverbWork[index] = (short)Saturate(value);
    }

    private void ReverbStep(int inputLeft, int inputRight)
    {
        // Disp registers are in 8-byte units -> 4 halfwords.
        var lin = MulVol(inputLeft, RevVLIN);
        var rin = MulVol(inputRight, RevVRIN);

        // Same-side reflection.
        var mlSamePrev = ReverbRead(RevMLSAME * 4 - 1);
        ReverbWrite(RevMLSAME * 4, MulVol(Saturate(lin + MulVol(ReverbRead(RevDLSAME * 4), RevVWALL) - mlSamePrev), RevVIIR) + mlSamePrev);
        var mrSamePrev = ReverbRead(RevMRSAME * 4 - 1);
        ReverbWrite(RevMRSAME * 4, MulVol(Saturate(rin + MulVol(ReverbRead(RevDRSAME * 4), RevVWALL) - mrSamePrev), RevVIIR) + mrSamePrev);

        // Different-side reflection.
        var mlDiffPrev = ReverbRead(RevMLDIFF * 4 - 1);
        ReverbWrite(RevMLDIFF * 4, MulVol(Saturate(lin + MulVol(ReverbRead(RevDRDIFF * 4), RevVWALL) - mlDiffPrev), RevVIIR) + mlDiffPrev);
        var mrDiffPrev = ReverbRead(RevMRDIFF * 4 - 1);
        ReverbWrite(RevMRDIFF * 4, MulVol(Saturate(rin + MulVol(ReverbRead(RevDLDIFF * 4), RevVWALL) - mrDiffPrev), RevVIIR) + mrDiffPrev);

        // Early echo (comb filters).
        var leftOut = Saturate(
            MulVol(ReverbRead(RevMLCOMB1 * 4), RevVCOMB1)
            + MulVol(ReverbRead(RevMLCOMB2 * 4), RevVCOMB2)
            + MulVol(ReverbRead(RevMLCOMB3 * 4), RevVCOMB3)
            + MulVol(ReverbRead(RevMLCOMB4 * 4), RevVCOMB4));
        var rightOut = Saturate(
            MulVol(ReverbRead(RevMRCOMB1 * 4), RevVCOMB1)
            + MulVol(ReverbRead(RevMRCOMB2 * 4), RevVCOMB2)
            + MulVol(ReverbRead(RevMRCOMB3 * 4), RevVCOMB3)
            + MulVol(ReverbRead(RevMRCOMB4 * 4), RevVCOMB4));

        // Late reverb all-pass filter 1.
        var leftApf1 = ReverbRead((RevMLAPF1 - RevDAPF1) * 4);
        leftOut = Saturate(leftOut - MulVol(leftApf1, RevVAPF1));
        ReverbWrite(RevMLAPF1 * 4, leftOut);
        leftOut = Saturate(MulVol(leftOut, RevVAPF1) + leftApf1);

        var rightApf1 = ReverbRead((RevMRAPF1 - RevDAPF1) * 4);
        rightOut = Saturate(rightOut - MulVol(rightApf1, RevVAPF1));
        ReverbWrite(RevMRAPF1 * 4, rightOut);
        rightOut = Saturate(MulVol(rightOut, RevVAPF1) + rightApf1);

        // Late reverb all-pass filter 2.
        var leftApf2 = ReverbRead((RevMLAPF2 - RevDAPF2) * 4);
        leftOut = Saturate(leftOut - MulVol(leftApf2, RevVAPF2));
        ReverbWrite(RevMLAPF2 * 4, leftOut);
        leftOut = Saturate(MulVol(leftOut, RevVAPF2) + leftApf2);

        var rightApf2 = ReverbRead((RevMRAPF2 - RevDAPF2) * 4);
        rightOut = Saturate(rightOut - MulVol(rightApf2, RevVAPF2));
        ReverbWrite(RevMRAPF2 * 4, rightOut);
        rightOut = Saturate(MulVol(rightOut, RevVAPF2) + rightApf2);

        _reverbLastLeft = MulVol(leftOut, _reverbDepthLeft);
        _reverbLastRight = MulVol(rightOut, _reverbDepthRight);

        _reverbPos = (_reverbPos + 1) % _reverbWork.Length;
    }

    // ---------------------------------------------------------------------
    // WAV parsing
    // RELATION: adapter for SoundBin-authored RIFF/WAV payloads (PCM 8/16-bit, mono/stereo).
    // ---------------------------------------------------------------------
    private static bool TryParsePcmWaveToFrames(byte[] waveData, out short[] pcm, out int channels, out int sampleRate, out int frameCount)
    {
        pcm = [];
        channels = 1;
        sampleRate = 0;
        frameCount = 0;

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
        var bitsPerSample = 0;
        var dataOffset = 0;
        var dataLength = 0;
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

        if (!foundFmt || dataLength <= 0 || sampleRate <= 0)
        {
            return false;
        }

        if (channelCount != 1 && channelCount != 2)
        {
            return false;
        }

        channels = channelCount;
        if (bitsPerSample == 16)
        {
            var sampleCount = dataLength / 2;
            pcm = new short[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                pcm[i] = BinaryPrimitives.ReadInt16LittleEndian(waveData.AsSpan(dataOffset + i * 2, 2));
            }

            frameCount = sampleCount / channelCount;
            return frameCount > 0;
        }

        if (bitsPerSample == 8)
        {
            // 8-bit WAV is unsigned with 0x80 center.
            pcm = new short[dataLength];
            for (var i = 0; i < dataLength; i++)
            {
                pcm[i] = (short)((waveData[dataOffset + i] - 0x80) << 8);
            }

            frameCount = dataLength / channelCount;
            return frameCount > 0;
        }

        return false;
    }
}
