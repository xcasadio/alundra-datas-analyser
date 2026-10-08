using AlundraEngine;
using AlundraEngine.Balance;
using AlundraEngine.Closing;
using AlundraEngine.DatasBin;
using AlundraEngine.Editor;
using AlundraEngine.Etc;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;

namespace AlundraDataExtractor;

internal class Program
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true, IncludeFields = true };
    private static readonly JsonSerializerOptions _jsonLineSerializerOptions = new() { IncludeFields = true };
    private static Dictionary<string, HashSet<string>> entitySpriteSheetIds = new();
    private static HashSet<string> entitySpriteSheetAlreadySaved = new();

    /// <summary>
    /// Re-extracts the MOVIE/*.MOV streams from a CD image, keeping whole 2352-byte sectors.
    ///
    /// The usual extraction writes a flat 2048 bytes per sector, which is the Form 1 user data
    /// size. That is fine for the video sectors but truncates every Form 2 (XA audio) sector from
    /// 2324 bytes, losing 2 of its 18 ADPCM sound groups - about 11% of every 53 ms of sound. The
    /// raw copy produced here keeps the subheaders and the full audio payload, and is what
    /// LoaderEngine looks for first (".STR" beside the ".MOV").
    /// </summary>
    private static void ExtractMovies(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: AlundraDataExtractor --extract-movies <cdImage.bin> <movieOutputPath>");
            return;
        }

        var imagePath = args[1];
        var outputPath = args[2];

        using var image = PsxSdk.Cd.CdImageReader.OpenFile(imagePath);
        Console.WriteLine($"Image  : {imagePath}");
        Console.WriteLine($"Layout : {image.SectorSize} bytes/sector ({(image.IsRaw ? "raw" : "iso")})");

        if (!image.IsRaw)
        {
            Console.WriteLine("This image holds 2048-byte sectors only, so its XA audio is already incomplete.");
            Console.WriteLine("Use a raw BIN/IMG rip (2352 bytes per sector) to recover movie sound.");
            return;
        }

        Directory.CreateDirectory(outputPath);
        var movies = image.ListFiles()
            .Where(file => file.Path.StartsWith("MOVIE", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (movies.Count == 0)
        {
            Console.WriteLine("No MOVIE directory found in this image.");
            return;
        }

        foreach (var movie in movies)
        {
            var target = Path.Combine(outputPath, Path.ChangeExtension(movie.Name, ".STR"));
            Console.Write($"  {movie.Name} -> {Path.GetFileName(target)} ... ");
            using (var stream = File.Create(target))
            {
                image.ExtractFileRaw(movie, stream);
            }

            Console.WriteLine($"{new FileInfo(target).Length:N0} bytes");
        }
    }

    static void Main(string[] args)
    {
        _jsonSerializerOptions.Converters.Add(new ByteArrayAsNumbersConverter());

        if (args.Length > 0 && string.Equals(args[0], "--trace-bgm", StringComparison.OrdinalIgnoreCase))
        {
            TraceBgm(args);
            return;
        }

        if (args.Length > 0 && string.Equals(args[0], "--render-bgm", StringComparison.OrdinalIgnoreCase))
        {
            RenderBgm(args);
            return;
        }

        if (args.Length > 0 && string.Equals(args[0], "--verify-bgm", StringComparison.OrdinalIgnoreCase))
        {
            VerifyBgm(args);
            return;
        }

        if (args.Length > 0 && string.Equals(args[0], "--extract-movies", StringComparison.OrdinalIgnoreCase))
        {
            ExtractMovies(args);
            return;
        }

        if (args.Length > 0 && string.Equals(args[0], "--probe-portraits", StringComparison.OrdinalIgnoreCase))
        {
            ProbePortraits(args);
            return;
        }

        if (args.Length > 0 && string.Equals(args[0], "--extract-sfx", StringComparison.OrdinalIgnoreCase))
        {
            ExtractSfxOnly(args);
            return;
        }

        if (args.Length < 2)
        {
            Console.WriteLine("Usage: AlundraDataExtractor <gamePath> <extractionPath> [--tiled-tileset-layout original|compact] [--spritesheet-layout original|compact]");
            Console.WriteLine("       AlundraDataExtractor --trace-bgm <gamePath|soundBinPath> <outputPath> [--bgm-index N] [--frames N]");
            Console.WriteLine("       AlundraDataExtractor --render-bgm <gamePath|soundBinPath> <outputPath> [--bgm-index N] [--frames N]");
            Console.WriteLine("       AlundraDataExtractor --verify-bgm <gamePath|soundBinPath> [--frames N]");
            Console.WriteLine("       AlundraDataExtractor --probe-portraits <gamePath> <outputJsonPath>");
            Console.WriteLine("       AlundraDataExtractor --extract-sfx <gamePath> <extractionPath>");
        Console.WriteLine("       AlundraDataExtractor --extract-movies <cdImage.bin> <movieOutputPath>");
            return;
        }

        var gamePath = args[0];
        var extractionPath = args[1];
        Console.WriteLine($"Extract data from {gamePath}");
        Console.WriteLine($"To {extractionPath}");

        var gameEngine = CreateGameEngine(gamePath, out var balanceBin, out var font3, out var etcRes);
        gameEngine.InitializeEngine();

        var alunCdExe = new AlunCdExe(gamePath);
        var closingExe = new ClosingExeInspector(gamePath);

        ExtractDataFromAlunCdExe(alunCdExe, extractionPath);
        ExtractDataFromClosingExe(closingExe, extractionPath);
        ExtractDataFromBalanceBin(balanceBin, extractionPath);
        ExtractDataFromScreenFolder(font3, gameEngine.StaticVariables, extractionPath, Path.Combine(gamePath, "DATA", "..", "TAKI\\SCREEN"));
        ExtractDataFromEtcRes(etcRes, gameEngine.StaticVariables, extractionPath);
        var psxFramesPerSecond = etcRes is EtcResUsa ? 60 : 50;
        var tiledTilesetLayoutMode = ReadEnumOption(args, "--tiled-tileset-layout", TiledTilesetLayoutMode.Compact);
        var spriteSheetLayoutMode = ReadEnumOption(args, "--spritesheet-layout", SpriteSheetLayoutMode.Compact);
        ExtractDataFromDatasBin(gameEngine.DatasBin, gameEngine.StaticVariables, extractionPath, psxFramesPerSecond, tiledTilesetLayoutMode, spriteSheetLayoutMode);
        ExtractDataFromSoundBin(gameEngine.SoundBin, extractionPath);
        ExtractDataFromBgm(Path.Combine(gamePath, "DATA", "SOUND.BIN"), extractionPath);
    }

    private record BgmExportRecord(int SoundIndex, string File, int Frames, double DurationSeconds, bool LoopDetected, int PeakLeft, int PeakRight, double RmsLeft, double RmsRight, int FirstAudibleFrame, bool Looping, int? LoopStartFrame, int? LoopStartSample);

    // JUSTIFICATION: C# language bridge only
    // RELATION: batch counterpart to --render-bgm; renders every LoadMapSequence-addressable track
    // (MusicSeqVabOffsets triplets) through the same SPU mixer, up to the second loop jump (E19.L-b1,
    // docs/plan-e19-opcodes.md section 1.2w.2, D-E19-73, D-E19-76).
    // SequenceTrackState.TimesPlayed (incremented on the '/' end-of-track meta-event, gated by
    // LoopCount) never fires for these tracks - PlaySeq(seqId, 1, 1) sets LoopCount=1, but the real
    // repeat mechanism used by background music is a separate loop-marker meta-event (0x1E, in
    // SoundManager.FUN_8008ca40) that jumps SeqPosition back to SeqLoopPos directly, without ever
    // touching TimesPlayed. So the loop point is detected the model-free way instead: watch
    // SequenceTrackState.SeqPosition frame to frame. A track that loops is written as [0, J2), J1 and J2 being
    // its first and second loop jumps, and bgm.json records Looping, LoopStartFrame = J1 and
    // LoopStartSample = J1 x 735 (the sample the engine seeks to when it loops); a track without a loop keeps
    // its release tail. Capped at maxSeconds: reaching the cap is a failure, not a file.
    // JUSTIFICATION: C# language bridge only
    // RELATION: docs/plan-extraction-bgm.md decision D-X-7 - this batch never cleans its output
    // directory, so a track that is refused today would otherwise keep the file a previous, broken run
    // wrote for it. Removing it is what makes "a silent render is a failure, not a file" true on disk.
    private static void DeleteStaleBgm(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private static void ExtractDataFromBgm(string soundBinPath, string extractionPath)
    {
        Console.WriteLine("Extract BGM");

        var soundPath = Path.Combine(extractionPath, "sound");
        var bgmPath = Path.Combine(soundPath, "bgm");
        Directory.CreateDirectory(bgmPath);

        const int maxSeconds = 480;
        const int maxFrames = maxSeconds * 60;
        var maxSoundIndex = (new SoundBin(soundBinPath).MusicSeqVabOffsets.Length - 4) / 3;
        var exported = new List<BgmExportRecord>();
        var silent = 0;
        var failed = 0;

        for (var soundIndex = 1; soundIndex <= maxSoundIndex; soundIndex++)
        {
            var fileName = $"bgm_{soundIndex:D3}.wav";
            var filePath = Path.Combine(bgmPath, fileName);

            try
            {
                // docs/plan-e19-opcodes.md section 1.2v (X2, D-E19-69): every track is rendered from a
                // fresh sound bank, mixer, engine and sound system, like --render-bgm. One shared system
                // made each track start with what the previous one left sounding.
                var soundBin = new SoundBin(soundBinPath);
                var mixer = new SpuMixerSoundPlaybackBackend();
                soundBin.AttachPlaybackBackend(mixer);
                var gameEngine = new GameEngine(null!, null!, soundBin, null!, null!, null);
                gameEngine.StaticVariables.Initialize(gameEngine);
                gameEngine.SoundManager.InitializeSoundSystem();

                gameEngine.SoundManager.LoadMapSequence(soundIndex, 1);
                var seqId = gameEngine.StaticVariables.g_requestedSeqId;
                if (seqId < 0)
                {
                    Console.WriteLine($"BGM {soundIndex}: no sequence (seqId<0) - not exported");
                    DeleteStaleBgm(filePath);
                    failed++;
                    continue;
                }

                var result = RenderBgmTrackUntilLoop(gameEngine, mixer, seqId, maxFrames);
                if (result.Failure != null)
                {
                    Console.WriteLine($"BGM {soundIndex} failed: {result.Failure} - not exported");
                    DeleteStaleBgm(filePath);
                    failed++;
                    continue;
                }

                // docs/plan-extraction-bgm.md, decision D-X-4: a render that never crossed the
                // audibility threshold is a FAILURE, not a file. 26 tracks were once written as five
                // seconds of silence while the run reported success - and FirstAudibleFrame, the very
                // field that proves it, was already being recorded and never read. Refusing to write
                // is not enough on its own: the batch never cleans its output directory, so the stale
                // silent file has to go too (D-X-7), or "not a file" would be false on disk.
                if (result.FirstAudibleFrame < 0)
                {
                    Console.WriteLine($"BGM {soundIndex}: rendered SILENT ({result.Frames} frames, peak {result.PeakLeft}/{result.PeakRight}) - not exported");
                    DeleteStaleBgm(filePath);
                    silent++;
                    continue;
                }

                WriteStereoWav(filePath, result.Samples, SpuMixerSoundPlaybackBackend.OutputSampleRate);
                exported.Add(new BgmExportRecord(soundIndex, fileName, result.Frames, result.Frames / 60.0, result.LoopDetected, result.PeakLeft, result.PeakRight, result.RmsLeft, result.RmsRight, result.FirstAudibleFrame,
                    result.Looping, result.Looping ? result.LoopStartFrame : null, result.Looping ? result.LoopStartFrame * SpuMixerSoundPlaybackBackend.OutputSampleRate / 60 : null));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BGM {soundIndex} failed: {ex.Message}");
                DeleteStaleBgm(filePath);
                failed++;
            }
        }

        File.WriteAllText(Path.Combine(soundPath, "bgm.json"), JsonSerializer.Serialize(exported, _jsonSerializerOptions));
        Console.WriteLine($"Extracted BGM: rendered={exported.Count} silent={silent} failed={failed} (of {maxSoundIndex})");
        Console.WriteLine(VoicePitchGuard.FormatReport());
    }

    // JUSTIFICATION: C# language bridge only
    // RELATION: a handful of MusicSeqVabOffsets slots are unused/silent placeholders (or one-shot
    // stingers with no loop marker at all) rather than real looping tracks, and would otherwise
    // burn the full maxFrames as near-silence; silenceGraceFrames cuts those short once nothing
    // has crossed the audible threshold for a few seconds.
    // A backward SeqPosition with sequence flag bit 0 still set is a loop jump; with bit 0 clear it is the end of
    // the track (docs/plan-e19-loop-annexe/data/notes.md section 1.2). The first jump J1 is kept, the second J2 is not
    // (the file is [0, J2)); a track without a loop jump keeps its release tail after the end of the track.
    private sealed class BgmRender
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

    private static BgmRender RenderBgmTrackUntilLoop(GameEngine gameEngine, SpuMixerSoundPlaybackBackend mixer, short seqId, int maxFrames)
    {
        const int samplesPerFrame = SpuMixerSoundPlaybackBackend.OutputSampleRate / 60;
        const int silenceGraceFrames = 5 * 60;
        const int releaseTailSilentFrames = 60;
        var renderBuffer = new short[samplesPerFrame * 2];
        var allSamples = new List<short>(maxFrames * samplesPerFrame * 2 / 4);
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
            mixer.RenderSamples(renderBuffer, samplesPerFrame);
            var backward = currentSeqPosition < previousSeqPosition;

            if (backward && sequencePlaying && result.LoopStartFrame >= 0)
            {
                result.LoopEndFrame = frame; // J2: the first frame of the third pass, not kept
                stoppedByCap = false;
                break;
            }

            var frameAudible = false;
            for (var i = 0; i < samplesPerFrame; i++)
            {
                if (Math.Abs((int)renderBuffer[i * 2]) > 64 || Math.Abs((int)renderBuffer[i * 2 + 1]) > 64)
                {
                    frameAudible = true;
                    break;
                }
            }

            allSamples.AddRange(renderBuffer);
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

        var frames = allSamples.Count / (samplesPerFrame * 2);
        if (result.EndOfTrackFrame >= 0)
        {
            frames = Math.Max(result.EndOfTrackFrame + 1, lastAudibleFrame + 1);
            allSamples.RemoveRange(frames * samplesPerFrame * 2, allSamples.Count - frames * samplesPerFrame * 2);
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
            CrossfadeLoopEnd(result.Samples, result.LoopStartFrame, result.LoopEndFrame, samplesPerFrame);
        }

        // The statistics describe the written samples, after the crossfade.
        long sumSquaresLeft = 0;
        long sumSquaresRight = 0;
        for (var i = 0; i < frames * samplesPerFrame; i++)
        {
            int left = result.Samples[i * 2];
            int right = result.Samples[i * 2 + 1];
            sumSquaresLeft += (long)left * left;
            sumSquaresRight += (long)right * right;
            result.PeakLeft = Math.Max(result.PeakLeft, Math.Abs(left));
            result.PeakRight = Math.Max(result.PeakRight, Math.Abs(right));
            if (result.FirstAudibleFrame < 0 && (Math.Abs(left) > 64 || Math.Abs(right) > 64))
            {
                result.FirstAudibleFrame = i / samplesPerFrame;
            }
        }

        var totalSamples = (long)frames * samplesPerFrame;
        result.RmsLeft = totalSamples > 0 ? Math.Sqrt(sumSquaresLeft / (double)totalSamples) : 0;
        result.RmsRight = totalSamples > 0 ? Math.Sqrt(sumSquaresRight / (double)totalSamples) : 0;
        return result;
    }

    // Linear crossfade of the file's last frame (k stereo samples before J2) toward the k stereo samples before J1, so the
    // last sample equals the sample just before J1: the wrap J2 -> J1 is the junction the render itself made at J1.
    // Round to nearest, k odd: no ties.
    private static void CrossfadeLoopEnd(short[] samples, int loopStartFrame, int loopEndFrame, int k)
    {
        var a0 = loopEndFrame * k - k;
        var b0 = loopStartFrame * k - k;
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

    // Volume/Pan (VagAtr) and the three record-level attributes (VabHdr.Mvol, ProgAtr.Mvol/Mpan) come from the
    // record DecodeSfxTones actually decoded, after the RefSfxId chain; null when the record was not resolved.
    // Appended at the end so the fields that were already there keep their place in sfx.json.
    private record SfxToneExport(int ToneIndex, string File, int SampleRate, int LoopStart, int LoopEnd, bool Repeat, int? Volume, int? Pan);

    private record SfxExportRecord(int Id, short VabId, short ProgramNumber, short ToneNumber, short Note, short SeqNum, short RefSfxId, short MaxVoices, short NumTones, string? SkipReason, SfxToneExport[] Tones,
        int? VabMasterVolume, int? ProgramVolume, int? ProgramPan);

    // JUSTIFICATION: C# language bridge only
    // RELATION: sound effects only (sound/sfx.json and sound/sfx/*.wav), through the very same GameEngine
    // and ExtractDataFromSoundBin as the full extraction, so the WAV files are identical to its output;
    // lets sfx.json be refreshed without re-extracting (and re-decoding) everything else.
    private static void ExtractSfxOnly(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: AlundraDataExtractor --extract-sfx <gamePath> <extractionPath>");
            return;
        }

        var gamePath = args[1];
        var extractionPath = args[2];
        Console.WriteLine($"Extract sound effects from {gamePath}");
        Console.WriteLine($"To {extractionPath}");

        var gameEngine = CreateGameEngine(gamePath, out _, out _, out _);
        gameEngine.InitializeEngine();
        ExtractDataFromSoundBin(gameEngine.SoundBin, extractionPath);
    }

    // JUSTIFICATION: C# language bridge only
    // RELATION: exhaustive archival export of every SfxRecord (global VabId=-1 bank plus every
    // per-map VAB reachable through VabIndexByMapId/RefSfxId chains) to mono 16-bit WAV + a
    // companion JSON with the loop/pitch/routing metadata a WAV file cannot carry on its own.
    private static void ExtractDataFromSoundBin(SoundBin soundBin, string extractionPath)
    {
        Console.WriteLine("Extract sound effects");

        var soundPath = Path.Combine(extractionPath, "sound");
        var sfxPath = Path.Combine(soundPath, "sfx");
        Directory.CreateDirectory(sfxPath);

        var exported = new SortedDictionary<int, SfxExportRecord>();
        var siblingSourcedCount = 0;
        var crossVabIds = new List<string>();

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
            // fire on retail data (the acceptance requires its hit count to read zero). If it ever
            // does, the dropped tone must be visible in sfx.json - so it is recorded through the
            // EXISTING record-level SkipReason rather than a new per-tone field, which would emit
            // "SkipReason": null on every tone and move the whole manifest even when untriggered.
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
                using (var wav = SoundBin.WriteWavFile(tone.Pcm, 0, tone.Pcm.Length, tone.SampleRate, false, 0x7F, 0x7F))
                {
                    File.WriteAllBytes(Path.Combine(sfxPath, fileName), wav.ToArray());
                }

                toneExports[i] = new SfxToneExport(tone.ToneIndex, fileName, tone.SampleRate, tone.LoopStart, tone.LoopEnd, tone.Repeat, tone.Volume, tone.Pan);
            }

            // The samples of a map record may come from a sibling further down its RefSfxId chain (the first one
            // whose VAB is the map VAB open right now): counted, since the attributes follow those samples.
            if (attributes.ResolvedSfxId != sfxid)
            {
                siblingSourcedCount++;
                var resolvedVabId = soundBin.SfxRecords[attributes.ResolvedSfxId].VabId;
                if (resolvedVabId != record.VabId)
                {
                    crossVabIds.Add($"{sfxid}->{attributes.ResolvedSfxId} (VAB {record.VabId}->{resolvedVabId})");
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

        File.WriteAllText(Path.Combine(soundPath, "sfx.json"), JsonSerializer.Serialize(exported.Values, _jsonSerializerOptions));
        Console.WriteLine($"Extracted {exported.Values.Count(r => r.Tones.Length > 0)}/{soundBin.SfxRecords.Length - 1} sound effects ({exported.Values.Sum(r => r.Tones.Length)} WAV files)");
        Console.WriteLine($"Sound effects exported from a sibling of their RefSfxId chain: {siblingSourcedCount} ({crossVabIds.Count} of them from another VAB: {string.Join(", ", crossVabIds)})");
    }

    private record PortraitProbeRecord(int Icon, int ItemId, int Position, string Status, int? Sector5Id, long? Signature, int? Spritesheet, int? Page, int? Palette, int? SourceX, int? SourceY, int? Swidth, int? Sheight);

    // JUSTIFICATION: read-only measurement for docs/plan-e13c-icones-hud.md, tranche S1.a (D-E13C-3).
    // RELATION: opens the .BIN exactly the way the normal extraction path does (CreateGameEngine),
    // then calls SpriteRecord.GetPortraitImageset - never GameMapHelper.EnumerateImages - for every
    // icon value D-E13C-1 already established (31..119, itemId = icon - 30; GraphicManager.cs:1911-
    // 1914 and :1791 index SpriteInfo.SpriteRecords by that same value, "position" below). It writes
    // only to the caller's outputJsonPath: no file under gamePath, data-extracted/ or
    // alundra-project/ is read for writing or touched. Icon 72 (itemId 42, §6 point 5 of the plan)
    // is expected to report "missing record": SpriteInfo.cs:93-101 leaves SpriteRecords[i] null
    // whenever SpriteTable[i] is 0 or -1, and that case is measured here, not guessed.
    private static void ProbePortraits(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: AlundraDataExtractor --probe-portraits <gamePath> <outputJsonPath>");
            return;
        }

        var gamePath = args[1];
        var outputJsonPath = args[2];

        const int firstIcon = 31;
        const int lastIcon = 119;

        var gameEngine = CreateGameEngine(gamePath, out _, out _, out _);
        gameEngine.InitializeEngine();

        var spriteRecords = gameEngine.AlundraMap.SpriteInfo.SpriteRecords;
        using var br = gameEngine.DatasBin.OpenBin();

        var results = new List<PortraitProbeRecord>();

        for (var icon = firstIcon; icon <= lastIcon; icon++)
        {
            var itemId = icon - 30;

            if (icon < 0 || icon >= spriteRecords.Length)
            {
                results.Add(new PortraitProbeRecord(icon, itemId, icon, $"out of range (SpriteRecords.Length={spriteRecords.Length})", null, null, null, null, null, null, null, null, null));
                continue;
            }

            var record = spriteRecords[icon];
            if (record == null)
            {
                results.Add(new PortraitProbeRecord(icon, itemId, icon, "missing record (SpriteTable entry is 0 or -1)", null, null, null, null, null, null, null, null, null));
                continue;
            }

            try
            {
                var imageset = record.GetPortraitImageset(br);
                var image = imageset.Images[0];
                results.Add(new PortraitProbeRecord(
                    icon, itemId, icon, "ok", record.Header.Sector5Id,
                    image.Signature, image.Spritesheet, image.Spritesheet & 7, image.Palette,
                    image.SourceX, image.SourceY, image.Swidth, image.Sheight));
            }
            catch (Exception ex)
            {
                results.Add(new PortraitProbeRecord(icon, itemId, icon, $"exception: {ex.GetType().Name}: {ex.Message}", record.Header.Sector5Id, null, null, null, null, null, null, null, null));
            }
        }

        var outputDir = Path.GetDirectoryName(Path.GetFullPath(outputJsonPath));
        if (!string.IsNullOrEmpty(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        File.WriteAllText(outputJsonPath, JsonSerializer.Serialize(results, _jsonSerializerOptions));

        Console.WriteLine($"Probed {results.Count} icons ({firstIcon}..{lastIcon}), wrote {outputJsonPath}");
        Console.WriteLine($"  ok: {results.Count(r => r.Status == "ok")}");
        Console.WriteLine($"  missing record: {results.Count(r => r.Status.StartsWith("missing"))}");
        Console.WriteLine($"  exception: {results.Count(r => r.Status.StartsWith("exception"))}");
    }

    // JUSTIFICATION: C# language bridge only
    private static GameEngine CreateGameEngine(string gamePath, out BalanceBin balanceBin, out Font3 font3, out EtcRes etcRes)
    {
        var dataFolder = Path.Combine(gamePath, "DATA");
        var datasBin = new DatasBin(Path.Combine(dataFolder, "DATAS.BIN"));
        balanceBin = new BalanceBin(Path.Combine(dataFolder, "BALANCE.BIN"));
        var soundBin = new SoundBin(Path.Combine(dataFolder, "SOUND.BIN"));
        font3 = new Font3(Path.Combine(dataFolder, "..", "TAKI\\SCREEN"));
        var etcResFileName = PathHelper.GetEtcFileName(dataFolder);
        etcRes = Path.GetFileName(etcResFileName).Contains("usa", StringComparison.InvariantCultureIgnoreCase)
            ? new EtcResUsa(etcResFileName)
            : new EtcResR(etcResFileName);

        return new GameEngine(datasBin, balanceBin, soundBin, etcRes, font3, null);
    }

    // JUSTIFICATION: C# language bridge only
    private static void TraceBgm(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: AlundraDataExtractor --trace-bgm <gamePath|soundBinPath> <outputPath> [--bgm-index N] [--frames N]");
            return;
        }

        var soundBinPath = ResolveSoundBinPath(args[1]);
        var outputPath = args[2];
        var bgmIndex = ReadIntOption(args, "--bgm-index", 1);
        var frames = ReadIntOption(args, "--frames", 600);

        Directory.CreateDirectory(outputPath);

        var gameEngine = CreateSoundOnlyGameEngine(soundBinPath);

        var traceFileName = Path.Combine(outputPath, $"bgm_{bgmIndex:D3}_csharp_trace.jsonl");
        using var writer = new StreamWriter(traceFileName) { AutoFlush = true };

        WriteBgmTraceHeader(writer, gameEngine, bgmIndex, frames);
        gameEngine.SoundManager.LoadMapSequence(bgmIndex, 1);
        WriteBgmTraceFrame(writer, gameEngine, -1, "after-load");

        for (var frame = 0; frame < frames; frame++)
        {
            gameEngine.SoundManager.AdvanceSoundFrame();
            WriteBgmTraceFrame(writer, gameEngine, frame, "frame");
        }

        Console.WriteLine($"Trace BGM C# written to {traceFileName}");
    }

    // JUSTIFICATION: C# language bridge only
    // RELATION: offline validation harness for SpuMixerSoundPlaybackBackend; renders the desktop
    // synthesis of the staged SPU voice state to a listenable 44100 Hz stereo WAV with level stats.
    // JUSTIFICATION: C# language bridge only
    // RELATION: read-only acceptance oracle for the BGM batch export (docs/plan-extraction-bgm.md,
    // slice X1). Renders every LoadMapSequence-addressable track through a SHARED GameEngine - one
    // engine, InitializeSoundSystem once - which is NOT the shape ExtractDataFromBgm uses since X2
    // (a fresh sound system per track, docs/plan-e19-opcodes.md section 1.2v) - and reports the peak
    // and an audible/silent verdict per index, writing nothing to disk. It exists because 26 of the
    // 46 tracks were exported as five seconds of silence and NOTHING reported it: the batch swallowed
    // the exception that poisoned them (Program.cs, ExtractDataFromBgm) and wrote the files anyway.
    // Run it BEFORE a fix to reproduce that profile, and after one to show the recovery.
    private static void VerifyBgm(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: AlundraDataExtractor --verify-bgm <gamePath|soundBinPath> [--frames N]");
            return;
        }

        var soundBinPath = ResolveSoundBinPath(args[1]);
        var frames = ReadIntOption(args, "--frames", 3600);

        var soundBin = new SoundBin(soundBinPath);
        var mixer = new SpuMixerSoundPlaybackBackend();
        soundBin.AttachPlaybackBackend(mixer);
        var gameEngine = new GameEngine(null!, null!, soundBin, null!, null!, null);
        gameEngine.StaticVariables.Initialize(gameEngine);
        gameEngine.SoundManager.InitializeSoundSystem();

        const int samplesPerFrame = SpuMixerSoundPlaybackBackend.OutputSampleRate / 60;
        var renderBuffer = new short[samplesPerFrame * 2];
        var maxSoundIndex = (soundBin.MusicSeqVabOffsets.Length - 4) / 3;

        var audible = 0;
        var silent = 0;
        var failed = 0;

        Console.WriteLine($"Verify BGM ({maxSoundIndex} tracks, {frames} frames each)");
        Console.WriteLine("idx  peak   verdict");

        for (var soundIndex = 1; soundIndex <= maxSoundIndex; soundIndex++)
        {
            try
            {
                gameEngine.SoundManager.LoadMapSequence(soundIndex, 1);
                var seqId = gameEngine.StaticVariables.g_requestedSeqId;
                if (seqId < 0)
                {
                    Console.WriteLine($"{soundIndex,3}  {"-",5}  SKIPPED (seqId<0)");
                    failed++;
                    continue;
                }

                var peak = 0;
                for (var frame = 0; frame < frames; frame++)
                {
                    gameEngine.SoundManager.AdvanceSoundFrame();
                    mixer.RenderSamples(renderBuffer, samplesPerFrame);
                    foreach (var sample in renderBuffer)
                    {
                        var magnitude = Math.Abs((int)sample);
                        if (magnitude > peak)
                        {
                            peak = magnitude;
                        }
                    }
                }

                // Same audibility threshold the batch export itself uses, so the two agree.
                var isAudible = peak > 64;
                Console.WriteLine($"{soundIndex,3}  {peak,5}  {(isAudible ? "AUDIBLE" : "*** SILENT ***")}");
                if (isAudible)
                {
                    audible++;
                }
                else
                {
                    silent++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{soundIndex,3}  {"-",5}  *** FAILED *** {ex.GetType().Name}: {ex.Message}");
                failed++;
            }
        }

        Console.WriteLine($"audible={audible} silent={silent} failed={failed}");
    }

    private static void RenderBgm(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: AlundraDataExtractor --render-bgm <gamePath|soundBinPath> <outputPath> [--bgm-index N] [--frames N]");
            return;
        }

        var soundBinPath = ResolveSoundBinPath(args[1]);
        var outputPath = args[2];
        var bgmIndex = ReadIntOption(args, "--bgm-index", 1);
        var frames = ReadIntOption(args, "--frames", 600);

        Directory.CreateDirectory(outputPath);

        var soundBin = new SoundBin(soundBinPath);
        var mixer = new SpuMixerSoundPlaybackBackend();
        soundBin.AttachPlaybackBackend(mixer);
        var gameEngine = new GameEngine(null!, null!, soundBin, null!, null!, null);
        gameEngine.StaticVariables.Initialize(gameEngine);
        gameEngine.SoundManager.InitializeSoundSystem();

        gameEngine.SoundManager.LoadMapSequence(bgmIndex, 1);

        if (args.Contains("--no-reverb", StringComparer.OrdinalIgnoreCase))
        {
            mixer.SetReverbEnabled(false);
        }

        const int samplesPerFrame = SpuMixerSoundPlaybackBackend.OutputSampleRate / 60;
        var renderBuffer = new short[samplesPerFrame * 2];
        var allSamples = new short[frames * samplesPerFrame * 2];
        long sumSquaresLeft = 0;
        long sumSquaresRight = 0;
        var peakLeft = 0;
        var peakRight = 0;
        var firstAudibleFrame = -1;

        for (var frame = 0; frame < frames; frame++)
        {
            gameEngine.SoundManager.AdvanceSoundFrame();
            mixer.RenderSamples(renderBuffer, samplesPerFrame);
            Array.Copy(renderBuffer, 0, allSamples, frame * samplesPerFrame * 2, samplesPerFrame * 2);

            for (var i = 0; i < samplesPerFrame; i++)
            {
                int left = renderBuffer[i * 2];
                int right = renderBuffer[i * 2 + 1];
                sumSquaresLeft += (long)left * left;
                sumSquaresRight += (long)right * right;
                if (Math.Abs(left) > peakLeft)
                {
                    peakLeft = Math.Abs(left);
                }

                if (Math.Abs(right) > peakRight)
                {
                    peakRight = Math.Abs(right);
                }

                if (firstAudibleFrame < 0 && (Math.Abs(left) > 64 || Math.Abs(right) > 64))
                {
                    firstAudibleFrame = frame;
                }
            }
        }

        var waveFileName = Path.Combine(outputPath, $"bgm_{bgmIndex:D3}_csharp_render.wav");
        WriteStereoWav(waveFileName, allSamples, SpuMixerSoundPlaybackBackend.OutputSampleRate);

        var totalSamples = (long)frames * samplesPerFrame;
        var rmsLeft = Math.Sqrt(sumSquaresLeft / (double)totalSamples);
        var rmsRight = Math.Sqrt(sumSquaresRight / (double)totalSamples);
        Console.WriteLine($"Render BGM C# written to {waveFileName}");
        Console.WriteLine($"frames={frames} firstAudibleFrame={firstAudibleFrame} peakL={peakLeft} peakR={peakRight} rmsL={rmsLeft:F1} rmsR={rmsRight:F1}");
    }

    // JUSTIFICATION: C# language bridge only
    private static void WriteStereoWav(string fileName, short[] interleavedStereo, int sampleRate)
    {
        using var writer = new BinaryWriter(File.Create(fileName));
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

    // JUSTIFICATION: C# language bridge only
    private static string ResolveSoundBinPath(string inputPath)
    {
        if (File.Exists(inputPath))
        {
            return inputPath;
        }

        var directSoundBinPath = Path.Combine(inputPath, "SOUND.BIN");
        if (File.Exists(directSoundBinPath))
        {
            return directSoundBinPath;
        }

        var dataSoundBinPath = Path.Combine(inputPath, "DATA", "SOUND.BIN");
        if (File.Exists(dataSoundBinPath))
        {
            return dataSoundBinPath;
        }

        throw new FileNotFoundException("SOUND.BIN was not found from trace input path.", inputPath);
    }

    // JUSTIFICATION: C# language bridge only
    private static GameEngine CreateSoundOnlyGameEngine(string soundBinPath)
    {
        var soundBin = new SoundBin(soundBinPath);
        soundBin.AttachPlaybackBackend(new TraceSoundPlaybackBackend());
        var gameEngine = new GameEngine(null!, null!, soundBin, null!, null!, null);
        gameEngine.StaticVariables.Initialize(gameEngine);
        gameEngine.SoundManager.InitializeSoundSystem();
        return gameEngine;
    }

    // JUSTIFICATION: C# language bridge only
    private static int ReadIntOption(string[] args, string optionName, int defaultValue)
    {
        for (var index = 0; index + 1 < args.Length; index++)
        {
            if (string.Equals(args[index], optionName, StringComparison.OrdinalIgnoreCase) && int.TryParse(args[index + 1], out var value))
            {
                return value;
            }
        }

        return defaultValue;
    }

    // JUSTIFICATION: C# language bridge only
    private static TEnum ReadEnumOption<TEnum>(string[] args, string optionName, TEnum defaultValue) where TEnum : struct, Enum
    {
        for (var index = 0; index + 1 < args.Length; index++)
        {
            if (!string.Equals(args[index], optionName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Enum.TryParse also accepts raw numbers, hence the IsDefined guard.
            if (Enum.TryParse<TEnum>(args[index + 1], true, out var value) && Enum.IsDefined(value))
            {
                return value;
            }

            var expected = string.Join(" or ", Enum.GetNames<TEnum>().Select(name => $"'{name.ToLowerInvariant()}'"));
            throw new ArgumentException($"Unsupported value '{args[index + 1]}' for {optionName}. Expected {expected}.");
        }

        return defaultValue;
    }

    // JUSTIFICATION: C# language bridge only
    private static void WriteBgmTraceHeader(TextWriter writer, GameEngine gameEngine, int bgmIndex, int frames)
    {
        var offsetIndex = bgmIndex * 3;
        var sequenceOffset = SafeRead(gameEngine.SoundBin.MusicSeqVabOffsets, offsetIndex);
        var vabHeaderOffset = SafeRead(gameEngine.SoundBin.MusicSeqVabOffsets, offsetIndex + 1);
        var vabBodyOffset = SafeRead(gameEngine.SoundBin.MusicSeqVabOffsets, offsetIndex + 2);
        var sequenceSectionIndex = Array.IndexOf(gameEngine.SoundBin.SequenceSectionOffsets, sequenceOffset);
        var vabSectionIndex = Array.IndexOf(gameEngine.SoundBin.VabSectionOffsets, vabHeaderOffset);

        WriteJsonLine(writer, new
        {
            eventName = "trace-start",
            bgmIndex,
            frames,
            sequenceOffset,
            vabHeaderOffset,
            vabBodyOffset,
            sequenceSectionIndex,
            vabSectionIndex,
            originalEntry = "DisplayDebugBgmMenu @ 0x8004A9D8 -> LoadMapSequence(index, 1)"
        });
    }

    // JUSTIFICATION: C# language bridge only
    private static void WriteBgmTraceFrame(TextWriter writer, GameEngine gameEngine, int frame, string eventName)
    {
        var staticVariables = gameEngine.StaticVariables;
        var seq = staticVariables.g_requestedSeqId >= 0 && staticVariables.g_requestedSeqId < staticVariables.g_sequenceStatePointers.Length
            ? staticVariables.g_sequenceStatePointers[staticVariables.g_requestedSeqId]
            : default;

        WriteJsonLine(writer, new
        {
            eventName,
            frame,
            currentMapSoundIndex = staticVariables.g_currentMapSoundIndex,
            currentVabId = staticVariables.g_currentVabId,
            requestedSeqId = staticVariables.g_requestedSeqId,
            sequenceSlotMask = staticVariables.g_sequenceSlotMask,
            seqFlags = seq.Flags,
            seqPosition = seq.SeqPosition,
            seqDelay = seq.Delay,
            seqTempo = seq.CurrentTempo,
            seqMessageType = seq.MessageType,
            seqChannel = seq.CurrentChannel,
            seqVolumeLeft = seq.field_0x74,
            seqVolumeRight = seq.field_0x76,
            seqChannelMapping = ReadSequenceChannelMapping(seq, seq.CurrentChannel),
            seqChannelVolume = ReadSequenceChannelVolume(seq, seq.CurrentChannel),
            seqChannelOrientation = ReadSequenceChannelOrientation(seq, seq.CurrentChannel),
            reverbMask = staticVariables.g_spuReverbAttr2.Mask,
            reverbMode = staticVariables.g_spuReverbAttr2.Mode,
            reverbDepthLeft = staticVariables.g_spuReverbAttr2.DepthLeft,
            reverbDepthRight = staticVariables.g_spuReverbAttr2.DepthRight,
            reverbDelay = staticVariables.g_spuReverbAttr2.Delay,
            reverbFeedback = staticVariables.g_spuReverbAttr2.Feedback
        });

        var voiceCount = Math.Min(staticVariables.g_numberOfVoices, staticVariables.g_voiceRuntimeSlots.Length);
        for (var voiceId = 0; voiceId < voiceCount; voiceId++)
        {
            ref var voiceSlot = ref staticVariables.g_voiceRuntimeSlots[voiceId];
            var active = gameEngine.SoundBin.VoicesAreActive[voiceId] != 0 || voiceSlot.SequenceKey >= 0 || voiceSlot.field_0x00 != 0;
            if (!active)
            {
                continue;
            }

            WriteJsonLine(writer, new
            {
                eventName = "voice",
                frame,
                voiceId,
                backendActive = gameEngine.SoundBin.VoicesAreActive[voiceId],
                voiceSlot.field_0x00,
                voiceSlot.ReplacementAge,
                voiceSlot.CurrentPitch,
                voiceSlot.field_0x06,
                voiceSlot.field_0x08,
                voiceSlot.field_0x0A,
                voiceSlot.Note,
                voiceSlot.SequenceKey,
                voiceSlot.VabFirstToneIndex,
                voiceSlot.ProgramIndex,
                voiceSlot.ToneIndex,
                voiceSlot.VabId,
                voiceSlot.Priority,
                voiceSlot.NoiseState,
                volumeLeft = staticVariables.g_spuVoiceVolumeLeft[voiceId],
                volumeRight = staticVariables.g_spuVoiceVolumeRight[voiceId],
                pitch = staticVariables.g_spuVoicePitch[voiceId],
                adsr1 = staticVariables.g_spuVoiceAdsr1[voiceId],
                adsr2 = staticVariables.g_spuVoiceAdsr2[voiceId],
                reverb = staticVariables.g_spuVoiceReverb[voiceId],
                dirtyFlags = staticVariables.g_spuVoiceDirtyFlags[voiceId]
            });
        }
    }

    // JUSTIFICATION: C# language bridge only
    private static int SafeRead(int[] values, int index)
    {
        return (uint)index < (uint)values.Length ? values[index] : -1;
    }

    // JUSTIFICATION: C# language bridge only
    private static void WriteJsonLine<T>(TextWriter writer, T value)
    {
        writer.WriteLine(JsonSerializer.Serialize(value, _jsonLineSerializerOptions));
    }

    // JUSTIFICATION: C# language bridge only
    private static byte ReadSequenceChannelMapping(SequenceTrackState seq, byte channel)
    {
        return channel switch
        {
            0 => seq.Channel0,
            1 => seq.Channel1,
            2 => seq.Channel2,
            3 => seq.Channel3,
            4 => seq.Channel4,
            5 => seq.Channel5,
            6 => seq.Channel6,
            7 => seq.Channel7,
            8 => seq.Channel8,
            9 => seq.Channel9,
            10 => seq.Channel10,
            11 => seq.Channel11,
            12 => seq.Channel12,
            13 => seq.Channel13,
            14 => seq.Channel14,
            15 => seq.Channel15,
            _ => 0
        };
    }

    // JUSTIFICATION: C# language bridge only
    private static ushort ReadSequenceChannelVolume(SequenceTrackState seq, byte channel)
    {
        return channel switch
        {
            0 => seq.Volume0,
            1 => seq.Volume1,
            2 => seq.Volume2,
            3 => seq.Volume3,
            4 => seq.Volume4,
            5 => seq.Volume5,
            6 => seq.Volume6,
            7 => seq.Volume7,
            8 => seq.Volume8,
            9 => seq.Volume9,
            10 => seq.Volume10,
            11 => seq.Volume11,
            12 => seq.Volume12,
            13 => seq.Volume13,
            14 => seq.Volume14,
            15 => seq.Volume15,
            _ => 0
        };
    }

    // JUSTIFICATION: C# language bridge only
    private static byte ReadSequenceChannelOrientation(SequenceTrackState seq, byte channel)
    {
        return channel switch
        {
            0 => seq.Orientation0,
            1 => seq.Orientation1,
            2 => seq.Orientation2,
            3 => seq.Orientation3,
            4 => seq.Orientation4,
            5 => seq.Orientation5,
            6 => seq.Orientation6,
            7 => seq.Orientation7,
            8 => seq.Orientation8,
            9 => seq.Orientation9,
            10 => seq.Orientation10,
            11 => seq.Orientation11,
            12 => seq.Orientation12,
            13 => seq.Orientation13,
            14 => seq.Orientation14,
            15 => seq.Orientation15,
            _ => 0
        };
    }

    private sealed class TraceSoundPlaybackBackend : ISoundPlaybackBackend
    {
        private readonly byte[] _playing = new byte[24];

        // JUSTIFICATION: backend desktop adaptation only
        public bool Play(Stream stream, int voiceId, bool shouldLoop, int loopStartSample, int loopEndSample)
        {
            if ((uint)voiceId < (uint)_playing.Length)
            {
                _playing[voiceId] = 1;
            }

            return true;
        }

        // JUSTIFICATION: backend desktop adaptation only
        public void Stop(int voiceId)
        {
            if ((uint)voiceId < (uint)_playing.Length)
            {
                _playing[voiceId] = 0;
            }
        }

        // JUSTIFICATION: backend desktop adaptation only
        public bool IsPlaying(int voiceId)
        {
            return (uint)voiceId < (uint)_playing.Length && _playing[voiceId] != 0;
        }

        // JUSTIFICATION: backend desktop adaptation only
        public void UpdateVoiceStereoVolume(int voiceId, short volumeLeft, short volumeRight)
        {
        }

        // JUSTIFICATION: backend desktop adaptation only
        public void UpdateVoicePitch(int voiceId, short pitch)
        {
        }

        // JUSTIFICATION: backend desktop adaptation only
        public void UpdateVoiceReverb(int voiceId, bool reverbEnabled)
        {
        }

        // JUSTIFICATION: backend desktop adaptation only
        public void SetReverbEnabled(bool enabled)
        {
        }

        // JUSTIFICATION: backend desktop adaptation only
        public void SetReverbState(int mode, short depthLeft, short depthRight)
        {
        }
    }

    private static void ExtractDataFromAlunCdExe(AlunCdExe alunCdExe, string extractionPath)
    {
        var memoryCardPath = Path.Combine(extractionPath, "memorycard");
        Directory.CreateDirectory(memoryCardPath);
        alunCdExe.MemoryCardFrame1Image.Save(Path.Combine(memoryCardPath, "memorycardframe1.png"), ImageFormat.Png);
        alunCdExe.MemoryCardFrame2Image.Save(Path.Combine(memoryCardPath, "memorycardframe2.png"), ImageFormat.Png);
        alunCdExe.MemoryCardFrame3Image.Save(Path.Combine(memoryCardPath, "memorycardframe3.png"), ImageFormat.Png);
    }

    private static void ExtractDataFromClosingExe(ClosingExeInspector closingExeInspector, string extractionPath)
    {
        closingExeInspector.SaveAllImages(Path.Combine(extractionPath, "closing"));
    }

    private static void ExtractDataFromBalanceBin(BalanceBin balanceBin, string extractionPath)
    {
        var balanceBinPath = Path.Combine(extractionPath, "data");
        Directory.CreateDirectory(balanceBinPath);
        File.WriteAllText(Path.Combine(balanceBinPath, $"{Path.GetFileName(balanceBin.FileName)}.json"), JsonSerializer.Serialize(balanceBin, _jsonSerializerOptions));
    }

    // E19.L-b1 (O-E19-66, docs/plan-e19-opcodes.md section 1.2w.2): FONT3.TIM's 4 bpp image decoded with entry 8 of
    // the binary's CLUT table, filled from WIND.CL (the loop at 0x80044B48-0x80044B9C reads it into the table, the text
    // bands and the cursor draw font3 with entry 8, read at 0x80050424). Only those two uses are proven to read entry 8
    // (docs/formats/font.md). The 15-bit colours are decoded as TimLoader does (bit replication, never the plain << 3 of
    // Font3.Palettes); colour 0x0000 stays transparent and keeps FONT3.TIM's own index-0 RGB. Lives here and calls the
    // public TimLoader API: the decompiled AlundraEngine is not modified.
    private static Bitmap DecodeFont3WithWindClut(string screenFolder, int windClutEntry)
    {
        using var stream = File.OpenRead(Path.Combine(screenFolder, "FONT3.TIM"));
        using var reader = new BinaryReader(stream);
        var raw = AlundraEngine.Graphics.TimLoader.LoadTimRaw(reader, Color.FromArgb(255, 156, 165, 132));
        var windCl = File.ReadAllBytes(Path.Combine(screenFolder, "WIND.CL"));
        var palette = new Color[16];
        for (var i = 0; i < 16; i++)
        {
            int c = BitConverter.ToUInt16(windCl, (windClutEntry * 16 + i) * 2);
            if (c == 0)
            {
                palette[i] = raw.Palettes![0][0];
                continue;
            }

            int r = ((c & 0x1F) << 3) | ((c & 0x1F) >> 2);
            int g = (((c >> 5) & 31) << 3) | (((c >> 5) & 31) >> 2);
            int b = (((c >> 10) & 31) << 3) | (((c >> 10) & 31) >> 2);
            palette[i] = Color.FromArgb(255, r, g, b);
        }

        return AlundraEngine.Graphics.TimLoader.DecodeBuffer(0, raw.Width, raw.Height, raw.Bpp, new[] { palette }, raw.ImgWWords, raw.ImgData);
    }

    private static void ExtractDataFromScreenFolder(Font3 font3, StaticVariables staticVariables, string extractionPath, string screenFolder)
    {
        var screenPath = Path.Combine(extractionPath, "ui");
        Directory.CreateDirectory(screenPath);
        using (var font3Page = DecodeFont3WithWindClut(screenFolder, 8))
        {
            font3Page.Save(Path.Combine(screenPath, "font3.png"), ImageFormat.Png);
        }

        var fontCharTiles = new List<FontCharTile>();

        for (int i = 0; i < 16 * 16; i++)
        {
            var charValue = (char)i;
            //charValue = TextDecoder.ConvertCp850ToLatin1(charIndex);
            fontCharTiles.Add(new FontCharTile { Code = i, X = charValue % 16 * 16, Y = charValue / 16 * 16, Width = 16, Height = 16, Palette = 8 });
        }

        //all tiles data
        File.WriteAllText(Path.Combine(screenPath, "font3.json"), JsonSerializer.Serialize(fontCharTiles, _jsonSerializerOptions));

        //all hud sprites
        var windBitmap = new Bitmap(256, 256);
        var windTiles = DrawAllUITiles(font3, staticVariables, windBitmap);
        windBitmap.Save(Path.Combine(screenPath, "wind.png"), ImageFormat.Png);

        //all tiles data
        var windData = windTiles.OrderBy(x => x.U0).ThenBy(x => x.V0);
        File.WriteAllText(Path.Combine(screenPath, "wind.json"), JsonSerializer.Serialize(windData, _jsonSerializerOptions));
    }

    private static HashSet<SpriteSheetTile> DrawAllUITiles(Font3 font3, StaticVariables staticVariables, Bitmap windBitmap)
    {
        var spriteSheetTiles = new HashSet<SpriteSheetTile>();

        //cursor dialog
        for (int i = 0; i < 4; i++)
        {
            spriteSheetTiles.Add(new SpriteSheetTile(
                staticVariables.g_dialogCursorTextureUV[i * 0x28],
                staticVariables.g_dialogCursorTextureUV[i * 0x28 + 1],
                0x10,
                0x10,
                8));
        }

        //message background
        foreach (var sprite in staticVariables.g_uiBoxesInventoryDescriptionBackground.SpritesA)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        //numbers
        for (int i = 0; i < 10; i++)
        {
            spriteSheetTiles.Add(new SpriteSheetTile(
                staticVariables.g_numbersSpriteSheetUVs[i * 0x14],
                staticVariables.g_numbersSpriteSheetUVs[i * 0x14 + 1],
                8,
                0x10,
                5));
        }

        AddHudTiles(staticVariables, spriteSheetTiles);
        AddInventoryTiles(staticVariables, spriteSheetTiles);

        //draw
        using var g = Graphics.FromImage(windBitmap);
        foreach (var tile in spriteSheetTiles)
        {
            var tileBitmap = font3.GenerateHudBitmap(tile.U0, tile.V0, tile.Width, tile.Height, tile.PaletteIndex);
            g.DrawImage(tileBitmap, tile.U0, tile.V0, tile.Width, tile.Height);
        }

        return spriteSheetTiles;
    }

    private static void AddHudTiles(StaticVariables staticVariables, HashSet<SpriteSheetTile> spriteSheetTiles)
    {
        //'/'
        spriteSheetTiles.Add(new SpriteSheetTile(
            staticVariables.g_numbersSpriteSheetUVs[200],  //0x50
            staticVariables.g_numbersSpriteSheetUVs[201],  //0x28
            8,
            0x10,
            5));

        //full life big icons
        spriteSheetTiles.Add(new SpriteSheetTile(
            staticVariables.g_fullLifeBigIconUVs[0],
            staticVariables.g_fullLifeBigIconUVs[1],
            0x10,
            0x10,
            7));


        //empty life big icons
        spriteSheetTiles.Add(new SpriteSheetTile(
            staticVariables.g_emptyLifeBigIconUVs[0],
            staticVariables.g_emptyLifeBigIconUVs[1],
            0x10,
            0x10,
            7));


        //full life small icons
        spriteSheetTiles.Add(new SpriteSheetTile(
            staticVariables.g_fullLifeSmallIconUVs[0],
            staticVariables.g_fullLifeSmallIconUVs[1],
            8,
            8,
            7));


        //empty life small icons
        spriteSheetTiles.Add(new SpriteSheetTile(
            staticVariables.g_emptyLifeSmallIconUVs[0],
            staticVariables.g_emptyLifeSmallIconUVs[1],
            8,
            8,
            7));

        // mp cristal
        for (int i = 0; i < 14; i++)
        {
            spriteSheetTiles.Add(new SpriteSheetTile(
                i * 8,
                56,
                8,
                0x10,
                5));
        }

        //money icons
        for (int i = 0; i < 4; i++)
        {
            spriteSheetTiles.Add(new SpriteSheetTile(
                staticVariables.g_hudMoneyIconUVs[i * 20],
                staticVariables.g_hudMoneyIconUVs[i * 20 + 1],
                8,
                0x10,
                5));
        }
    }

    private static void AddInventoryTiles(StaticVariables staticVariables, HashSet<SpriteSheetTile> spriteSheetTiles)
    {
        //cursor
        for (int i = 0; i < 4; i++)
        {
            var sprite = new SpriteSheetTile(
                staticVariables.g_inventoryCursorTextureUVs[i * 0x28],
                staticVariables.g_inventoryCursorTextureUVs[i * 0x28 + 1],
                0x10,
                0x10,
                0);
            spriteSheetTiles.Add(sprite);
        }

        //icons
        foreach (var sprite in staticVariables.g_moneyFalconKeyIconSpritesA)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        //rectangle selection
        spriteSheetTiles.Add(new SpriteSheetTile(48, 0x98, 0x18, 0x20, 0));

        //background
        foreach (var sprite in staticVariables.g_MainInventoryWeaponBackgroundSpritesA)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.g_MainInventoryItemBackgroundSpritesA)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800ad594)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800af674)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800b06ec)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800b123c)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800b1d8c)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800ba3d0)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800bcb40)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800bf2b0)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }

        foreach (var sprite in staticVariables.SPRT_ARRAY_800c1a20)
        {
            spriteSheetTiles.Add(new(sprite.u0, sprite.v0, sprite.w, sprite.h, sprite.clut));
        }
    }

    private static void ExtractDataFromEtcRes(EtcRes etcRes, StaticVariables staticVariables, string extractionPath)
    {
        var dataPath = Path.Combine(extractionPath, "data");
        Directory.CreateDirectory(dataPath);
        var path = Path.Combine(dataPath, $"{Path.GetFileName(etcRes.FileName)}.json");
        var sortedData = etcRes.StringByIndex.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        File.WriteAllText(path, JsonSerializer.Serialize(sortedData, _jsonSerializerOptions));
    }

    private static void ExtractDataFromDatasBin(DatasBin datasBin, StaticVariables staticVariables, string extractionPath, int psxFramesPerSecond, TiledTilesetLayoutMode tiledTilesetLayoutMode, SpriteSheetLayoutMode spriteSheetLayoutMode)
    {
        datasBin.LoadingScreen.Save(Path.Combine(extractionPath, "data", "loading_screen.png"), ImageFormat.Png);

        var tileAnimDescriptors = GameInitializer.CreateTileAnimDescriptors(0);
        EntityNames.Load(EntityNames.Language.French);

        var dataPath = Path.Combine(extractionPath, "data");
        Directory.CreateDirectory(dataPath);

        Console.WriteLine("Extract map alundra");
        using var br = datasBin.OpenBin();
        datasBin.AlundraGameMap.Load(br);
        // The inventory's opening portrait (sprite record 0, see GameMap.InventoryPortrait) is in no
        // animation: without this field, neither the atlas nor map_alundra.json would carry it.
        datasBin.AlundraGameMap.InventoryPortrait = datasBin.AlundraGameMap.SpriteInfo.SpriteRecords[0].GetPortraitImageset(br).Images[0];
        GameMapHelper.LoadDialoguePortraits(datasBin.AlundraGameMap, br);
        SaveAlundraMap(datasBin.AlundraGameMap, dataPath, spriteSheetLayoutMode);

        for (int i = 0; i < 483; i++)
        {
            Console.WriteLine($"Extract map {i}");
            var gameMap = datasBin.GameMaps[i];
            gameMap.Load(br);
            GameMapHelper.LoadDialoguePortraits(gameMap, br);
            SaveMap(gameMap, i, dataPath, tileAnimDescriptors, psxFramesPerSecond, tiledTilesetLayoutMode, spriteSheetLayoutMode);
        }

        foreach (var entitySpriteSheet in entitySpriteSheetIds)
        {
            Console.WriteLine($"Entity {entitySpriteSheet.Key}");

            foreach (var idName in entitySpriteSheet.Value)
            {
                Console.WriteLine($"\t{idName}");
            }
        }
    }

    private static void SaveAlundraMap(GameMap gameMap, string extractionPath, SpriteSheetLayoutMode spriteSheetLayoutMode)
    {
        // Must run before the JSON dump below: it populates SiImage.AtlasX/AtlasY, which the
        // serialized map_alundra.json needs to carry.
        GameMapHelper.SaveSpriteSheet(gameMap, Path.Combine(extractionPath, "map_alundra_spritesheet.png"), spriteSheetLayoutMode);
        GameMapHelper.SaveEffectSheet(gameMap, Path.Combine(extractionPath, "map_alundra_effectsheet.png"));

        File.WriteAllText(Path.Combine(extractionPath, "map_alundra.json"), JsonSerializer.Serialize(gameMap, _jsonSerializerOptions));

        //var gameMapJson = ConvertGameMap(gameMap);
        //File.WriteAllText(Path.Combine(extractionPath, "map_alundra.json"), JsonSerializer.Serialize(gameMapJson, _jsonSerializerOptions));
    }

    private static void SaveMap(GameMap gameMap, int id, string extractionPath, TileAnimDescriptor[] tileAnimDescriptors, int psxFramesPerSecond, TiledTilesetLayoutMode tiledTilesetLayoutMode, SpriteSheetLayoutMode spriteSheetLayoutMode)
    {
        GameMapHelper.SaveTileSheet(gameMap, Path.Combine(extractionPath, $"map_{id}_tilesheet.png"), tileAnimDescriptors);

        // Must run before the JSON dump below: it populates SiImage.AtlasX/AtlasY, which the
        // serialized map_{id}.json needs to carry.
        GameMapHelper.SaveSpriteSheet(gameMap, Path.Combine(extractionPath, $"map_{id}_spritesheet.png"), spriteSheetLayoutMode);
        GameMapHelper.SaveEffectSheet(gameMap, Path.Combine(extractionPath, $"map_{id}_effectsheet.png"));

        //var gameMapJson = ConvertGameMap(gameMap);
        //File.WriteAllText(Path.Combine(extractionPath, $"map_{id}.json"), JsonSerializer.Serialize(gameMapJson, _jsonSerializerOptions));

        File.WriteAllText(Path.Combine(extractionPath, $"map_{id}.json"), JsonSerializer.Serialize(gameMap, _jsonSerializerOptions));

        //try to extract all entity infos from all map
        //GetEntitySpriteSheets(gameMap, id, extractionPath);

        TiledMapExporter.ExportMap(gameMap, id, extractionPath, tileAnimDescriptors, psxFramesPerSecond, tiledTilesetLayoutMode);
    }

    private static void GetEntitySpriteSheets(GameMap gameMap, int id, string extractionPath)
    {
        foreach (var entityRecord in gameMap.SpriteInfo.Entities.Entities.Where(x => x != null))
        {
            if (entityRecord.SpriteTableIndex >= 255)
            {
                continue;
            }

            var spriteRecord = gameMap.SpriteInfo.SpriteRecords[entityRecord.SpriteTableIndex];

            if (spriteRecord?.AnimSets == null)
            {
                continue;
            }

            HashSet<string> spriteSheetIds = new();
            int minSpriteSheetId = int.MaxValue;

            foreach (var animationSet in spriteRecord.AnimSets.Where(x => x != null))
            {
                foreach (var animation in animationSet.PreloadedAnims)
                {
                    if (animation.Frames == null)
                    {
                        continue;
                    }

                    foreach (var frame in animation.Frames)
                    {
                        if (frame.Images?.Images == null)
                        {
                            continue;
                        }

                        foreach (var image in frame.Images?.Images)
                        {
                            spriteSheetIds.Add($"map:{id} spritesheet:{image.Spritesheet & 0x7}");
                            minSpriteSheetId = Math.Min(minSpriteSheetId, image.Spritesheet & 0x7);
                        }
                    }
                }
            }

            if (spriteSheetIds.Count == 0)
            {
                continue;
            }

            var name = EntityNames.GetNameWithIndex(entityRecord.SpriteDirection, entityRecord.SpriteTableIndex);
            //name = name.Replace("_", $"_{id}_");
            //var name = EntityNames.GetName(entityRecord.SpriteTableIndex);

            //if (!entitySpriteSheetIds.TryAdd(name, spriteSheetIds))
            //{
            //    foreach (var spriteSheetId in spriteSheetIds)
            //    {
            //        entitySpriteSheetIds[name].Add(spriteSheetId);
            //    }
            //}

            if (entitySpriteSheetAlreadySaved.Add(name))
            {
                var fileName = Path.Combine(extractionPath, name.Replace('\\', '-').Replace('/', '-') + ".png");
                //SaveEntitySpriteSheet(gameMap, fileName, spriteSheetIds, spriteRecord, minSpriteSheetId);
            }
        }
    }

    public static void SaveEntitySpriteSheet(GameMap gameMap, string fileName, HashSet<string> spriteSheetIds, SpriteRecord spriteRecord, int minSpriteSheetId)
    {
        using var bitmap = new Bitmap(256, 256 * spriteSheetIds.Count);
        using var graphics = Graphics.FromImage(bitmap);

        foreach (var animationSet in spriteRecord.AnimSets)
        {
            if (animationSet == null)
            {
                continue;
            }

            foreach (var animation in animationSet.PreloadedAnims)
            {
                if (animation?.Frames == null)
                {
                    continue;
                }

                for (int i = 0; i < animation.NumberOfFrames; i++)
                {
                    var frame = animation.Frames[i];

                    if (frame?.Images?.Images == null)
                    {
                        continue;
                    }

                    foreach (var image in frame.Images.Images)
                    {
                        var spriteBitmap = gameMap.GetSpriteBitmap(image);
                        var x = image.Sx;
                        var y = ((image.Spritesheet & 0x7) - minSpriteSheetId) * 256 + image.Sy;
                        graphics.DrawImage(spriteBitmap, x, y);
                    }
                }
            }
        }

        bitmap.Save(fileName, ImageFormat.Png);
    }
}