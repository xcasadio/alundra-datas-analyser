using AlundraEngine;
using AlundraEngine.Balance;
using AlundraEngine.Closing;
using AlundraEngine.DatasBin;
using AlundraEngine.Editor;
using AlundraEngine.Etc;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using AlundraExtraction;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlundraDataExtractor;

internal class Program
{
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

        if (args.Length > 0 && string.Equals(args[0], "--extract-bgm", StringComparison.OrdinalIgnoreCase))
        {
            ExtractBgmOnly(args);
            return;
        }

        if (args.Length < 2)
        {
            Console.WriteLine("Usage: AlundraDataExtractor <gamePath> <extractionPath> [--tiled-tileset-layout original|compact] [--spritesheet-layout original|compact] [--tick pal|desktop60]");
            Console.WriteLine("       AlundraDataExtractor --trace-bgm <gamePath|soundBinPath> <outputPath> [--bgm-index N] [--frames N] [--tick pal|desktop60]");
            Console.WriteLine("       AlundraDataExtractor --render-bgm <gamePath|soundBinPath> <outputPath> [--bgm-index N] [--frames N] [--tick pal|desktop60]");
            Console.WriteLine("       AlundraDataExtractor --verify-bgm <gamePath|soundBinPath> [--frames N] [--tick pal|desktop60]");
            Console.WriteLine("       AlundraDataExtractor --probe-portraits <gamePath> <outputJsonPath>");
            Console.WriteLine("       AlundraDataExtractor --extract-sfx <gamePath> <extractionPath>");
            Console.WriteLine("       AlundraDataExtractor --extract-bgm <gamePath> <extractionPath> [--tick pal|desktop60]");
        Console.WriteLine("       AlundraDataExtractor --extract-movies <cdImage.bin> <movieOutputPath>");
            return;
        }

        var gamePath = args[0];
        var extractionPath = args[1];
        // Read before any extraction step, so a bad --tick value stops the run before it writes anything.
        var tickProfile = ReadTickProfile(args);
        Console.WriteLine($"Extract data from {gamePath}");
        Console.WriteLine($"To {extractionPath}");

        // The nine disc files of the default extraction are read once, here; the orchestration lives in AlundraExtraction.
        var discFiles = ReadDiscFiles(gamePath);
        var tiledTilesetLayoutMode = ReadEnumOption(args, "--tiled-tileset-layout", TiledTilesetLayoutMode.Compact);
        var spriteSheetLayoutMode = ReadEnumOption(args, "--spritesheet-layout", SpriteSheetLayoutMode.Compact);

        // The sink creates folders on write; these two are created up front as before.
        Directory.CreateDirectory(Path.Combine(extractionPath, "sound", "sfx"));
        Directory.CreateDirectory(Path.Combine(extractionPath, "sound", "bgm"));

        // The library never deletes: a track that is silent, failed or refused must not keep the file a previous run wrote.
        DefaultExtraction.Write(discFiles, tiledTilesetLayoutMode, spriteSheetLayoutMode, tickProfile, new DirectorySink(extractionPath), Console.WriteLine,
            relativePath => DeleteStaleBgm(Path.Combine(extractionPath, relativePath.Replace('/', Path.DirectorySeparatorChar))));
    }

    // JUSTIFICATION: C# language bridge only
    private static DefaultExtractionInputs ReadDiscFiles(string gamePath)
    {
        var dataFolder = Path.Combine(gamePath, "DATA");
        var screenFolder = Path.Combine(dataFolder, "..", "TAKI\\SCREEN");
        var etcResFileName = PathHelper.GetEtcFileName(dataFolder);

        return new DefaultExtractionInputs(
            File.ReadAllBytes(Path.Combine(dataFolder, "DATAS.BIN")),
            File.ReadAllBytes(Path.Combine(dataFolder, "BALANCE.BIN")),
            Path.Combine(dataFolder, "BALANCE.BIN"),
            File.ReadAllBytes(Path.Combine(dataFolder, "SOUND.BIN")),
            File.ReadAllBytes(etcResFileName),
            Path.GetFileName(etcResFileName),
            File.ReadAllBytes(Path.Combine(gamePath, "ALUN_CD.EXE")),
            File.ReadAllBytes(Path.Combine(gamePath, "CLOSING.EXE")),
            File.ReadAllBytes(Path.Combine(screenFolder, "FONT3.TIM")),
            File.ReadAllBytes(Path.Combine(screenFolder, "WIND.CL")),
            File.ReadAllBytes(Path.Combine(screenFolder, "WIND.TX")));
    }

    // TickRate (ticks per second) is written last and only at a profile other than desktop60, so a desktop60 bgm.json is
    // byte-identical to the ones written before the PAL tick; its absence means 60 Hz, 735 samples per tick (parent
    // repository docs/plan-bgm-tick-pal.md, D5 and P4). Frames and LoopStartFrame count ticks of that rate.

    // JUSTIFICATION: C# language bridge only
    // RELATION: --tick pal|desktop60 picks the SequencerTickProfile the music is rendered on (parent repository
    // docs/plan-bgm-tick-pal.md, P4). pal, the default, is the cadence of the French binary: rate 50 and one tick per PAL
    // VBlank, 49.761 Hz. desktop60 is AlundraGame's cadence and the one of every export before that plan, kept to prove the
    // change inert.
    private enum BgmTickOption
    {
        Pal,
        Desktop60,
    }

    // JUSTIFICATION: C# language bridge only
    private static SequencerTickProfile ReadTickProfile(string[] args) =>
        ReadEnumOption(args, "--tick", BgmTickOption.Pal) == BgmTickOption.Pal ? SequencerTickProfile.PalBinary : SequencerTickProfile.Desktop60;

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
    // LoopStartSample = start(J1) (the sample the engine seeks to when it loops); a track without a loop keeps
    // its release tail. Capped at maxSeconds: reaching the cap is a failure, not a file.
    // The ticks are those of the SequencerTickProfile: tick n covers the output samples [start(n), start(n + 1)), with
    // start(n) = floor(n * 44100 / f) (parent repository docs/plan-bgm-tick-pal.md, P1); at desktop60, start(n) = 735 n.
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

    private static void ExtractDataFromBgm(string soundBinPath, string extractionPath, SequencerTickProfile tickProfile) =>
        ExtractDataFromBgm(extractionPath, tickProfile, (sink, notWritten) => AudioExtraction.ExtractBgm(soundBinPath, tickProfile, sink, Console.WriteLine, notWritten));

    private static void ExtractDataFromBgm(string extractionPath, SequencerTickProfile tickProfile, Func<IExtractionSink, Action<string>, BgmSummary> extract)
    {
        Console.WriteLine($"Extract BGM (tick {tickProfile.Name}, {tickProfile.TickRate.ToString(System.Globalization.CultureInfo.InvariantCulture)} Hz)");

        Directory.CreateDirectory(Path.Combine(extractionPath, "sound", "bgm"));

        // The library never deletes: a track that is silent, failed or refused must not keep the file a previous run wrote.
        var summary = extract(new DirectorySink(extractionPath),
            relativePath => DeleteStaleBgm(Path.Combine(extractionPath, relativePath.Replace('/', Path.DirectorySeparatorChar))));

        Console.WriteLine($"Extracted BGM: rendered={summary.Rendered} silent={summary.Silent} failed={summary.Failed} (of {summary.MaxSoundIndex})");
        Console.WriteLine(VoicePitchGuard.FormatReport());
    }


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
    // RELATION: re-extracts the music alone (sound/bgm/*.wav and sound/bgm.json) through the very function the full
    // export calls, so a change to the music render needs no 4 GB re-extraction and touches nothing else
    // (parent repository docs/plan-bgm-tick-pal.md, P5). The engine is created as for the full export.
    private static void ExtractBgmOnly(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: AlundraDataExtractor --extract-bgm <gamePath> <extractionPath> [--tick pal|desktop60]");
            return;
        }

        var gamePath = args[1];
        var extractionPath = args[2];
        var tickProfile = ReadTickProfile(args);
        Console.WriteLine($"Extract music from {gamePath}");
        Console.WriteLine($"To {extractionPath}");

        var gameEngine = CreateGameEngine(gamePath, out _, out _, out _);
        gameEngine.InitializeEngine();
        ExtractDataFromBgm(Path.Combine(gamePath, "DATA", "SOUND.BIN"), extractionPath, tickProfile);
    }

    // JUSTIFICATION: C# language bridge only
    // RELATION: exhaustive archival export of every SfxRecord (global VabId=-1 bank plus every
    // per-map VAB reachable through VabIndexByMapId/RefSfxId chains) to mono 16-bit WAV + a
    // companion JSON with the loop/pitch/routing metadata a WAV file cannot carry on its own.
    private static void ExtractDataFromSoundBin(SoundBin soundBin, string extractionPath)
    {
        Console.WriteLine("Extract sound effects");

        Directory.CreateDirectory(Path.Combine(extractionPath, "sound", "sfx"));

        var extraction = AudioExtraction.ComputeSfx(soundBin);
        AudioExtraction.WriteSfx(extraction, new DirectorySink(extractionPath));

        Console.WriteLine($"Extracted {extraction.Records.Values.Count(r => r.Tones.Length > 0)}/{soundBin.SfxRecords.Length - 1} sound effects ({extraction.Records.Values.Sum(r => r.Tones.Length)} WAV files)");
        Console.WriteLine($"Sound effects exported from a sibling of their RefSfxId chain: {extraction.SiblingSourcedCount} ({extraction.CrossVabIds.Count} of them from another VAB: {string.Join(", ", extraction.CrossVabIds)})");
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

        File.WriteAllText(outputJsonPath, JsonSerializer.Serialize(results, ExtractionJson.Options));

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
            Console.WriteLine("Usage: AlundraDataExtractor --trace-bgm <gamePath|soundBinPath> <outputPath> [--bgm-index N] [--frames N] [--tick pal|desktop60]");
            return;
        }

        var soundBinPath = ResolveSoundBinPath(args[1]);
        var outputPath = args[2];
        var bgmIndex = ReadIntOption(args, "--bgm-index", 1);
        var frames = ReadIntOption(args, "--frames", 600);

        Directory.CreateDirectory(outputPath);

        var gameEngine = CreateSoundOnlyGameEngine(soundBinPath, ReadTickProfile(args));

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
            Console.WriteLine("Usage: AlundraDataExtractor --verify-bgm <gamePath|soundBinPath> [--frames N] [--tick pal|desktop60]");
            return;
        }

        var soundBinPath = ResolveSoundBinPath(args[1]);
        var frames = ReadIntOption(args, "--frames", 3600);
        var tickProfile = ReadTickProfile(args);

        var soundBin = new SoundBin(soundBinPath) { TickProfile = tickProfile };
        var mixer = new SpuMixerSoundPlaybackBackend();
        soundBin.AttachPlaybackBackend(mixer);
        var gameEngine = new GameEngine(null!, null!, soundBin, null!, null!, null);
        gameEngine.StaticVariables.Initialize(gameEngine);
        gameEngine.SoundManager.InitializeSoundSystem();

        // One engine for every track: the host tick keeps counting across tracks, like the sound bank's own tick count.
        var renderBuffer = new short[tickProfile.MaxSamplesInTick * 2];
        var maxSoundIndex = (soundBin.MusicSeqVabOffsets.Length - 4) / 3;
        long hostTick = 0;

        var audible = 0;
        var silent = 0;
        var failed = 0;

        Console.WriteLine($"Verify BGM ({maxSoundIndex} tracks, {frames} frames each, tick {tickProfile.Name})");
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
                    var tickSamples = tickProfile.SamplesInTick(hostTick++);
                    mixer.RenderSamples(renderBuffer, tickSamples);
                    for (var i = 0; i < tickSamples * 2; i++)
                    {
                        var magnitude = Math.Abs((int)renderBuffer[i]);
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
            Console.WriteLine("Usage: AlundraDataExtractor --render-bgm <gamePath|soundBinPath> <outputPath> [--bgm-index N] [--frames N] [--tick pal|desktop60]");
            return;
        }

        var soundBinPath = ResolveSoundBinPath(args[1]);
        var outputPath = args[2];
        var bgmIndex = ReadIntOption(args, "--bgm-index", 1);
        var frames = ReadIntOption(args, "--frames", 600);
        var tickProfile = ReadTickProfile(args);

        Directory.CreateDirectory(outputPath);

        var soundBin = new SoundBin(soundBinPath) { TickProfile = tickProfile };
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

        var renderBuffer = new short[tickProfile.MaxSamplesInTick * 2];
        var allSamples = new short[tickProfile.StartSample(frames) * 2];
        long sumSquaresLeft = 0;
        long sumSquaresRight = 0;
        var peakLeft = 0;
        var peakRight = 0;
        var firstAudibleFrame = -1;

        for (var frame = 0; frame < frames; frame++)
        {
            gameEngine.SoundManager.AdvanceSoundFrame();
            var tickSamples = tickProfile.SamplesInTick(frame);
            mixer.RenderSamples(renderBuffer, tickSamples);
            Array.Copy(renderBuffer, 0, allSamples, tickProfile.StartSample(frame) * 2, tickSamples * 2);

            for (var i = 0; i < tickSamples; i++)
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
        File.WriteAllBytes(waveFileName, ExtractionAudio.EncodeStereoWav(allSamples, SpuMixerSoundPlaybackBackend.OutputSampleRate));

        var totalSamples = tickProfile.StartSample(frames);
        var rmsLeft = Math.Sqrt(sumSquaresLeft / (double)totalSamples);
        var rmsRight = Math.Sqrt(sumSquaresRight / (double)totalSamples);
        Console.WriteLine($"Render BGM C# written to {waveFileName}");
        Console.WriteLine($"frames={frames} firstAudibleFrame={firstAudibleFrame} peakL={peakLeft} peakR={peakRight} rmsL={rmsLeft:F1} rmsR={rmsRight:F1}");
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
    private static GameEngine CreateSoundOnlyGameEngine(string soundBinPath, SequencerTickProfile tickProfile)
    {
        var soundBin = new SoundBin(soundBinPath) { TickProfile = tickProfile };
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
            tickProfile = gameEngine.SoundBin.TickProfile.Name,
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