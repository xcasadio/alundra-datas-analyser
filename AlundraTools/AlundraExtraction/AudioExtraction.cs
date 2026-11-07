using System.Text.Json.Serialization;
using AlundraEngine;
using AlundraEngine.Sound;

namespace AlundraExtraction;

// TickRate (ticks per second) is written last and only at a profile other than desktop60, so a desktop60 bgm.json is
// byte-identical to the ones written before the PAL tick; its absence means 60 Hz, 735 samples per tick (parent
// repository docs/plan-bgm-tick-pal.md, D5 and P4). Frames and LoopStartFrame count ticks of that rate.
public record BgmExportRecord(int SoundIndex, string File, int Frames, double DurationSeconds, bool LoopDetected, int PeakLeft, int PeakRight, double RmsLeft, double RmsRight, int FirstAudibleFrame, bool Looping, int? LoopStartFrame, int? LoopStartSample,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] double? TickRate);

// Volume/Pan (VagAtr) and the three record-level attributes (VabHdr.Mvol, ProgAtr.Mvol/Mpan) come from the
// record DecodeSfxTones actually decoded, after the RefSfxId chain; null when the record was not resolved.
// Appended at the end so the fields that were already there keep their place in sfx.json.
public record SfxToneExport(int ToneIndex, string File, int SampleRate, int LoopStart, int LoopEnd, bool Repeat, int? Volume, int? Pan);

public record SfxExportRecord(int Id, short VabId, short ProgramNumber, short ToneNumber, short Note, short SeqNum, short RefSfxId, short MaxVoices, short NumTones, string? SkipReason, SfxToneExport[] Tones,
    int? VabMasterVolume, int? ProgramVolume, int? ProgramPan);

/// <summary>The rendering of one music track: stereo interleaved samples and the statistics.</summary>
public sealed class BgmRender
{
    public short[] Samples = Array.Empty<short>();
    public int Frames;
    public bool LoopDetected;
    public bool Looping;
    public int LoopStartFrame = -1;
    public int LoopEndFrame = -1;
    public int EndOfTrackFrame = -1;
    public int PeakLeft;
    public int PeakRight;
    public double RmsLeft;
    public double RmsRight;
    public int FirstAudibleFrame = -1;
    public string? Failure;
}

public enum BgmTrackStatus
{
    Rendered,
    NoSequence,
    Failed,
    Silent,
}

/// <summary>Outcome of one music track; Samples and Record are set only when Status is Rendered.</summary>
public sealed class BgmTrackResult
{
    public int SoundIndex;
    public string FileName = "";
    public BgmTrackStatus Status;
    public string Message = "";
    public short[] Samples = Array.Empty<short>();
    public BgmExportRecord? Record;
}

public sealed record BgmSummary(int Rendered, int Silent, int Failed, int MaxSoundIndex);

/// <summary>Every music track result, in track order, and the summary, as computed by AudioExtraction.ComputeBgm.</summary>
public sealed record BgmComputation(IReadOnlyList<BgmTrackResult> Tracks, BgmSummary Summary);

/// <summary>PCM of one sound-effect tone, as decoded; encode it with ExtractionAudio.EncodeMonoWav.</summary>
public sealed record SfxTonePcm(string FileName, byte[] Pcm, int SampleRate);

public sealed class SfxExtraction
{
    public SortedDictionary<int, SfxExportRecord> Records { get; } = new();
    public List<SfxTonePcm> Tones { get; } = new();
    public int SiblingSourcedCount;
    public List<string> CrossVabIds { get; } = new();
    public int SoundEffectCount;
}

public static class ExtractionAudio
{
    public static byte[] EncodeStereoWav(short[] interleavedStereo, int sampleRate)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
        {
            var dataLength = interleavedStereo.Length * sizeof(short);
            writer.Write("RIFF"u8);
            writer.Write(36 + dataLength);
            writer.Write("WAVE"u8);
            writer.Write("fmt "u8);
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)2);
            writer.Write(sampleRate);
            writer.Write(sampleRate * 2 * sizeof(short));
            writer.Write((short)(2 * sizeof(short)));
            writer.Write((short)16);
            writer.Write("data"u8);
            writer.Write(dataLength);
            foreach (var sample in interleavedStereo)
            {
                writer.Write(sample);
            }
        }

        return stream.ToArray();
    }

    public static byte[] EncodeMonoWav(byte[] pcm, int sampleRate)
    {
        using var wav = SoundBin.WriteWavFile(pcm, 0, pcm.Length, sampleRate, false, 0x7F, 0x7F);
        return wav.ToArray();
    }
}

/// <summary>Family 2 of the extraction: background music and sound effects.</summary>
public static class AudioExtraction
{
    public const int BgmMaxSeconds = 480;

    // ---- Compute (no writing) ----

    public static int ComputeMaxSoundIndex(string soundBinPath) =>
        (new SoundBin(soundBinPath).MusicSeqVabOffsets.Length - 4) / 3;

    /// <summary>Same as the path overload, from the contents of SOUND.BIN.</summary>
    public static int ComputeMaxSoundIndex(byte[] soundBin) =>
        (new SoundBin(soundBin).MusicSeqVabOffsets.Length - 4) / 3;

    /// <summary>Renders one track from a fresh sound bank, mixer, engine and sound system. Exceptions propagate.</summary>
    public static BgmTrackResult ComputeBgmTrack(string soundBinPath, int soundIndex, SequencerTickProfile tickProfile) =>
        ComputeBgmTrack(() => new SoundBin(soundBinPath), soundIndex, tickProfile);

    /// <summary>Same as the path overload, from the contents of SOUND.BIN; the track still gets a fresh SoundBin built from the same array.</summary>
    public static BgmTrackResult ComputeBgmTrack(byte[] soundBin, int soundIndex, SequencerTickProfile tickProfile) =>
        ComputeBgmTrack(() => new SoundBin(soundBin), soundIndex, tickProfile);

    private static BgmTrackResult ComputeBgmTrack(Func<SoundBin> createSoundBin, int soundIndex, SequencerTickProfile tickProfile)
    {
        var maxFrames = tickProfile.TicksForSeconds(BgmMaxSeconds);
        var track = new BgmTrackResult { SoundIndex = soundIndex, FileName = $"bgm_{soundIndex:D3}.wav" };

        // docs/plan-e19-opcodes.md section 1.2v (X2, D-E19-69): every track is rendered from a
        // fresh sound bank, mixer, engine and sound system, like --render-bgm. One shared system
        // made each track start with what the previous one left sounding.
        var soundBin = createSoundBin();
        soundBin.TickProfile = tickProfile;
        var mixer = new SpuMixerSoundPlaybackBackend();
        soundBin.AttachPlaybackBackend(mixer);
        var gameEngine = new GameEngine(null!, null!, soundBin, null!, null!, null);
        gameEngine.StaticVariables.Initialize(gameEngine);
        gameEngine.SoundManager.InitializeSoundSystem();

        gameEngine.SoundManager.LoadMapSequence(soundIndex, 1);
        var seqId = gameEngine.StaticVariables.g_requestedSeqId;
        if (seqId < 0)
        {
            track.Status = BgmTrackStatus.NoSequence;
            track.Message = $"BGM {soundIndex}: no sequence (seqId<0) - not exported";
            return track;
        }

        var result = RenderBgmTrackUntilLoop(gameEngine, mixer, seqId, maxFrames, tickProfile);
        if (result.Failure != null)
        {
            track.Status = BgmTrackStatus.Failed;
            track.Message = $"BGM {soundIndex} failed: {result.Failure} - not exported";
            return track;
        }

        // docs/plan-extraction-bgm.md, decision D-X-4: a render that never crossed the
        // audibility threshold is a FAILURE, not a file.
        if (result.FirstAudibleFrame < 0)
        {
            track.Status = BgmTrackStatus.Silent;
            track.Message = $"BGM {soundIndex}: rendered SILENT ({result.Frames} frames, peak {result.PeakLeft}/{result.PeakRight}) - not exported";
            return track;
        }

        track.Status = BgmTrackStatus.Rendered;
        track.Samples = result.Samples;
        // DurationSeconds = start(Frames) / 44100: at desktop60 the same double as Frames / 60.0 (one exact rational,
        // correctly rounded once).
        track.Record = new BgmExportRecord(soundIndex, track.FileName, result.Frames, tickProfile.StartSample(result.Frames) / (double)SpuMixerSoundPlaybackBackend.OutputSampleRate, result.LoopDetected, result.PeakLeft, result.PeakRight, result.RmsLeft, result.RmsRight, result.FirstAudibleFrame,
            result.Looping, result.Looping ? result.LoopStartFrame : null, result.Looping ? (int)tickProfile.StartSample(result.LoopStartFrame) : null,
            ReferenceEquals(tickProfile, SequencerTickProfile.Desktop60) ? null : tickProfile.TickRate);
        return track;
    }

    // A backward SeqPosition with sequence flag bit 0 still set is a loop jump; with bit 0 clear it is the end of
    // the track (docs/plan-e19-loop-annexe/data/notes.md section 1.2). The first jump J1 is kept, the second J2 is not
    // (the file is [0, J2)); a track without a loop jump keeps its release tail after the end of the track.
    // silenceGraceFrames cuts short the unused/silent placeholder slots once nothing has crossed the audible threshold
    // for a few seconds.
    private static BgmRender RenderBgmTrackUntilLoop(GameEngine gameEngine, SpuMixerSoundPlaybackBackend mixer, short seqId, int maxFrames, SequencerTickProfile tickProfile)
    {
        // The frames below are host ticks of tickProfile; the grace and the silent tail keep their length in seconds.
        var silenceGraceFrames = tickProfile.TicksForSeconds(5);
        var releaseTailSilentFrames = tickProfile.TicksForSeconds(1);
        var renderBuffer = new short[tickProfile.MaxSamplesInTick * 2];
        var allSamples = new List<short>((int)(tickProfile.StartSample(maxFrames) * 2 / 4));
        var renderedTicks = 0;
        var result = new BgmRender();
        var sequenceStates = gameEngine.StaticVariables.g_sequenceStatePointers;
        var previousSeqPosition = (uint)sequenceStates[seqId].SeqPosition;
        var silentFrameRun = 0;
        var lastAudibleFrame = -1;
        var stoppedByCap = true;

        for (var frame = 0; frame < maxFrames; frame++)
        {
            gameEngine.SoundManager.AdvanceSoundFrame();
            var currentSeqPosition = (uint)sequenceStates[seqId].SeqPosition;
            var sequencePlaying = (sequenceStates[seqId].Flags & 1u) != 0;
            var tickSamples = tickProfile.SamplesInTick(frame);
            mixer.RenderSamples(renderBuffer, tickSamples);
            var backward = currentSeqPosition < previousSeqPosition;

            if (backward && sequencePlaying && result.LoopStartFrame >= 0)
            {
                result.LoopEndFrame = frame; // J2: the first frame of the third pass, not kept
                stoppedByCap = false;
                break;
            }

            var frameAudible = false;
            for (var i = 0; i < tickSamples; i++)
            {
                if (Math.Abs((int)renderBuffer[i * 2]) > 64 || Math.Abs((int)renderBuffer[i * 2 + 1]) > 64)
                {
                    frameAudible = true;
                    break;
                }
            }

            allSamples.AddRange(new ArraySegment<short>(renderBuffer, 0, tickSamples * 2));
            renderedTicks++;
            if (frameAudible)
            {
                lastAudibleFrame = frame;
            }

            silentFrameRun = frameAudible ? 0 : silentFrameRun + 1;
            if (result.EndOfTrackFrame >= 0 && silentFrameRun >= releaseTailSilentFrames)
            {
                stoppedByCap = false;
                break;
            }

            if (silentFrameRun >= silenceGraceFrames)
            {
                stoppedByCap = false;
                break;
            }

            if (backward)
            {
                result.LoopDetected = true;
                if (sequencePlaying)
                {
                    result.LoopStartFrame = frame; // J1: the first frame of the second pass, kept
                }
                else
                {
                    result.EndOfTrackFrame = frame; // end of the track, kept; the release tail follows
                }
            }

            previousSeqPosition = currentSeqPosition;
        }

        var frames = renderedTicks;
        if (result.EndOfTrackFrame >= 0)
        {
            frames = Math.Max(result.EndOfTrackFrame + 1, lastAudibleFrame + 1);
            var keptValues = (int)(tickProfile.StartSample(frames) * 2);
            allSamples.RemoveRange(keptValues, allSamples.Count - keptValues);
        }

        if (result.LoopStartFrame >= 0 && result.LoopEndFrame < 0)
        {
            result.Failure = $"first loop jump at frame {result.LoopStartFrame} but no second one before the cap ({maxFrames} frames)";
            return result;
        }

        if (result.LoopStartFrame < 0 && result.EndOfTrackFrame < 0 && stoppedByCap)
        {
            result.Failure = $"no loop jump and no end of track before the cap ({maxFrames} frames)";
            return result;
        }

        if (result.EndOfTrackFrame >= 0 && stoppedByCap)
        {
            result.Failure = $"end of track at frame {result.EndOfTrackFrame} but its release tail did not end before the cap ({maxFrames} frames)";
            return result;
        }

        result.Frames = frames;
        result.Samples = allSamples.ToArray();
        result.Looping = result.LoopEndFrame >= 0;
        if (result.Looping)
        {
            CrossfadeLoopEnd(result.Samples, result.LoopStartFrame, result.LoopEndFrame, tickProfile);
        }

        // The statistics describe the written samples, after the crossfade.
        long sumSquaresLeft = 0;
        long sumSquaresRight = 0;
        var totalSamples = tickProfile.StartSample(frames);
        for (var i = 0; i < totalSamples; i++)
        {
            int left = result.Samples[i * 2];
            int right = result.Samples[i * 2 + 1];
            sumSquaresLeft += (long)left * left;
            sumSquaresRight += (long)right * right;
            result.PeakLeft = Math.Max(result.PeakLeft, Math.Abs(left));
            result.PeakRight = Math.Max(result.PeakRight, Math.Abs(right));
            if (result.FirstAudibleFrame < 0 && (Math.Abs(left) > 64 || Math.Abs(right) > 64))
            {
                result.FirstAudibleFrame = (int)tickProfile.TickOfSample(i);
            }
        }

        result.RmsLeft = totalSamples > 0 ? Math.Sqrt(sumSquaresLeft / (double)totalSamples) : 0;
        result.RmsRight = totalSamples > 0 ? Math.Sqrt(sumSquaresRight / (double)totalSamples) : 0;
        return result;
    }

    // Linear crossfade of the file's last tick (its k stereo samples, before start(J2)) toward the k stereo samples before
    // start(J1), so the last sample equals the sample just before J1: the wrap J2 -> J1 is the junction the render itself
    // made at J1. k is the length of the last tick: 735 at desktop60, 886 or 887 at the PAL rate. Round to nearest; for an
    // even k a tie rounds away from zero.
    private static void CrossfadeLoopEnd(short[] samples, int loopStartFrame, int loopEndFrame, SequencerTickProfile tickProfile)
    {
        var k = tickProfile.SamplesInTick(loopEndFrame - 1);
        var a0 = (int)tickProfile.StartSample(loopEndFrame) - k;
        var b0 = (int)tickProfile.StartSample(loopStartFrame) - k;
        for (var x = 0; x < k; x++)
        {
            for (var channel = 0; channel < 2; channel++)
            {
                long end = samples[(a0 + x) * 2 + channel];
                long beforeLoopStart = samples[(b0 + x) * 2 + channel];
                var numerator = end * (k - (x + 1)) + beforeLoopStart * (x + 1);
                var value = numerator >= 0 ? (numerator + k / 2) / k : (numerator - k / 2) / k;
                samples[(a0 + x) * 2 + channel] = (short)value;
            }
        }
    }

    /// <summary>
    /// Resolves every sound-effect record and decodes the PCM of its tones (global bank, then every map VAB).
    /// Pitch-guard hits are counted through VoicePitchGuard as before.
    /// </summary>
    public static SfxExtraction ComputeSfx(SoundBin soundBin)
    {
        var extraction = new SfxExtraction();
        var exported = extraction.Records;

        // Only records a result on successful resolution; unresolved ids are retried on every
        // OpenMapVab pass below since a given map-specific VabId is only reachable once that
        // particular map VAB is the one currently loaded.
        bool TryExtractSfx(int sfxid)
        {
            if (exported.ContainsKey(sfxid))
            {
                return true;
            }

            // docs/plan-extraction-bgm.md, D-X-3: the exporter-side pitch guard is expected NEVER to
            // fire on retail data. If it ever does, the dropped tone must be visible in sfx.json - so it is
            // recorded through the EXISTING record-level SkipReason.
            var guardHitsBefore = VoicePitchGuard.Hits(VoicePitchSite.Exporter);
            var tones = soundBin.DecodeSfxTones(sfxid, out var attributes);
            if (tones == null)
            {
                return false;
            }

            var pitchRefusals = VoicePitchGuard.Hits(VoicePitchSite.Exporter) - guardHitsBefore;

            var record = soundBin.SfxRecords[sfxid];
            var toneExports = new SfxToneExport[tones.Count];
            for (var i = 0; i < tones.Count; i++)
            {
                var tone = tones[i];
                var fileName = tones.Count == 1 ? $"sfx_{sfxid:D4}.wav" : $"sfx_{sfxid:D4}_{tone.ToneIndex}.wav";
                extraction.Tones.Add(new SfxTonePcm(fileName, tone.Pcm, tone.SampleRate));
                toneExports[i] = new SfxToneExport(tone.ToneIndex, fileName, tone.SampleRate, tone.LoopStart, tone.LoopEnd, tone.Repeat, tone.Volume, tone.Pan);
            }

            // The samples of a map record may come from a sibling further down its RefSfxId chain (the first one
            // whose VAB is the map VAB open right now): counted, since the attributes follow those samples.
            if (attributes.ResolvedSfxId != sfxid)
            {
                extraction.SiblingSourcedCount++;
                var resolvedVabId = soundBin.SfxRecords[attributes.ResolvedSfxId].VabId;
                if (resolvedVabId != record.VabId)
                {
                    extraction.CrossVabIds.Add($"{sfxid}->{attributes.ResolvedSfxId} (VAB {record.VabId}->{resolvedVabId})");
                }
            }

            var skipReason = pitchRefusals > 0
                ? $"pitch out of table ({pitchRefusals} tone(s) refused)"
                : tones.Count == 0 ? "no tones (NumTones=0)" : null;
            exported[sfxid] = new SfxExportRecord(sfxid, record.VabId, record.ProgramNumber, record.ToneNumber, record.Note, record.SeqNum, record.RefSfxId, record.MaxVoices, record.NumTones, skipReason, toneExports,
                attributes.VabMasterVolume, attributes.ProgramVolume, attributes.ProgramPan);
            return true;
        }

        for (var sfxid = 1; sfxid < soundBin.SfxRecords.Length; sfxid++)
        {
            if (soundBin.SfxRecords[sfxid].VabId == -1)
            {
                TryExtractSfx(sfxid);
            }
        }

        foreach (var vabIndex in SoundBin.VabIndexByMapId.Distinct().OrderBy(x => x))
        {
            soundBin.OpenMapVab(vabIndex);
            for (var sfxid = 1; sfxid < soundBin.SfxRecords.Length; sfxid++)
            {
                if (soundBin.SfxRecords[sfxid].VabId >= 0)
                {
                    TryExtractSfx(sfxid);
                }
            }
        }

        // Anything left unresolved after every known map VAB has been tried is genuinely
        // undecodable (invalid record, sequence-triggered, or an unreachable VabId) - document why.
        for (var sfxid = 1; sfxid < soundBin.SfxRecords.Length; sfxid++)
        {
            if (exported.ContainsKey(sfxid))
            {
                continue;
            }

            var record = soundBin.SfxRecords[sfxid];
            var reason = record.VabId == -2 ? "invalid (VabId=-2)"
                : record.SeqNum != -1 ? "sequence-triggered, not a decodable sample"
                : "map VAB not resolvable";
            exported[sfxid] = new SfxExportRecord(sfxid, record.VabId, record.ProgramNumber, record.ToneNumber, record.Note, record.SeqNum, record.RefSfxId, record.MaxVoices, record.NumTones, reason, [],
                null, null, null);
        }

        extraction.SoundEffectCount = soundBin.SfxRecords.Length - 1;
        return extraction;
    }

    // ---- Write (through an IExtractionSink) ----

    public static void WriteBgmTrack(BgmTrackResult track, IExtractionSink sink) =>
        sink.WriteBytes($"sound/bgm/{track.FileName}", ExtractionAudio.EncodeStereoWav(track.Samples, SpuMixerSoundPlaybackBackend.OutputSampleRate));

    public static void WriteBgmManifest(IReadOnlyList<BgmExportRecord> records, IExtractionSink sink) =>
        sink.WriteBytes("sound/bgm.json", ExtractionJson.Serialize(records));

    /// <summary>
    /// Renders every track without writing anything: every track result in order (a rendered one carries its record and
    /// samples; a silent, refused or failed one carries its status and message, an exception being a Failed result) and the
    /// summary. log receives the per-track messages of the tracks that are not rendered, in track order. All the samples
    /// are held at once (about 1.3 GB on the French disc).
    /// </summary>
    public static BgmComputation ComputeBgm(string soundBinPath, SequencerTickProfile tickProfile, Action<string> log) =>
        ComputeBgm(ComputeMaxSoundIndex(soundBinPath), soundIndex => ComputeBgmTrack(soundBinPath, soundIndex, tickProfile), log);

    /// <summary>Same as the path overload, from the contents of SOUND.BIN; each track gets a fresh SoundBin built from the array.</summary>
    public static BgmComputation ComputeBgm(byte[] soundBin, SequencerTickProfile tickProfile, Action<string> log) =>
        ComputeBgm(ComputeMaxSoundIndex(soundBin), soundIndex => ComputeBgmTrack(soundBin, soundIndex, tickProfile), log);

    private static BgmComputation ComputeBgm(int maxSoundIndex, Func<int, BgmTrackResult> computeTrack, Action<string> log)
    {
        var tracks = new List<BgmTrackResult>();
        var summary = ComputeBgmTracks(maxSoundIndex, computeTrack, log, tracks.Add);
        return new BgmComputation(tracks, summary);
    }

    // The loop of ComputeBgm and ExtractBgm. consume receives every track result, in order, once its message is logged; a
    // rendered result whose consume throws is turned into a Failed one (logged, consumed again) and counted as failed.
    private static BgmSummary ComputeBgmTracks(int maxSoundIndex, Func<int, BgmTrackResult> computeTrack, Action<string> log, Action<BgmTrackResult> consume)
    {
        var rendered = 0;
        var silent = 0;
        var failed = 0;

        for (var soundIndex = 1; soundIndex <= maxSoundIndex; soundIndex++)
        {
            try
            {
                var track = computeTrack(soundIndex);
                if (track.Status != BgmTrackStatus.Rendered)
                {
                    log(track.Message);
                    consume(track);
                    if (track.Status == BgmTrackStatus.Silent)
                    {
                        silent++;
                    }
                    else
                    {
                        failed++;
                    }

                    continue;
                }

                consume(track);
                rendered++;
            }
            catch (Exception ex)
            {
                var message = $"BGM {soundIndex} failed: {ex.Message}";
                log(message);
                consume(new BgmTrackResult { SoundIndex = soundIndex, FileName = $"bgm_{soundIndex:D3}.wav", Status = BgmTrackStatus.Failed, Message = message });
                failed++;
            }
        }

        return new BgmSummary(rendered, silent, failed, maxSoundIndex);
    }

    /// <summary>
    /// Renders every track, writes the WAV of each rendered one and the manifest. Track by track, so only one
    /// rendering is held (the same decisions as ComputeBgm, consumed one at a time). notWritten receives the sink-relative
    /// path of every track that is silent, failed or refused, so the caller may remove a stale file (the library never
    /// deletes). log receives the per-track messages.
    /// </summary>
    public static BgmSummary ExtractBgm(string soundBinPath, SequencerTickProfile tickProfile, IExtractionSink sink, Action<string> log, Action<string> notWritten) =>
        ExtractBgm(ComputeMaxSoundIndex(soundBinPath), soundIndex => ComputeBgmTrack(soundBinPath, soundIndex, tickProfile), sink, log, notWritten);

    /// <summary>Same as the path overload, from the contents of SOUND.BIN; each track gets a fresh SoundBin built from the array.</summary>
    public static BgmSummary ExtractBgm(byte[] soundBin, SequencerTickProfile tickProfile, IExtractionSink sink, Action<string> log, Action<string> notWritten) =>
        ExtractBgm(ComputeMaxSoundIndex(soundBin), soundIndex => ComputeBgmTrack(soundBin, soundIndex, tickProfile), sink, log, notWritten);

    private static BgmSummary ExtractBgm(int maxSoundIndex, Func<int, BgmTrackResult> computeTrack, IExtractionSink sink, Action<string> log, Action<string> notWritten)
    {
        var exported = new List<BgmExportRecord>();
        var summary = ComputeBgmTracks(maxSoundIndex, computeTrack, log, track =>
        {
            if (track.Status != BgmTrackStatus.Rendered)
            {
                notWritten($"sound/bgm/bgm_{track.SoundIndex:D3}.wav");
                return;
            }

            WriteBgmTrack(track, sink);
            exported.Add(track.Record!);
        });

        WriteBgmManifest(exported, sink);
        return summary;
    }

    public static void WriteSfx(SfxExtraction extraction, IExtractionSink sink)
    {
        foreach (var tone in extraction.Tones)
        {
            sink.WriteBytes($"sound/sfx/{tone.FileName}", ExtractionAudio.EncodeMonoWav(tone.Pcm, tone.SampleRate));
        }

        sink.WriteBytes("sound/sfx.json", ExtractionJson.Serialize(extraction.Records.Values));
    }
}
