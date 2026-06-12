using AlundraEngine;
using AlundraEngine.Balance;
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

        if (args.Length < 2)
        {
            Console.WriteLine("Usage: AlundraDataExtractor <gamePath> <extractionPath> [--tiled-tileset-layout original|compact]");
            Console.WriteLine("       AlundraDataExtractor --trace-bgm <gamePath|soundBinPath> <outputPath> [--bgm-index N] [--frames N]");
            Console.WriteLine("       AlundraDataExtractor --render-bgm <gamePath|soundBinPath> <outputPath> [--bgm-index N] [--frames N]");
            return;
        }

        var gamePath = args[0];
        var extractionPath = args[1];
        Console.WriteLine($"Extract data from {gamePath}");
        Console.WriteLine($"To {extractionPath}");

        var gameEngine = CreateGameEngine(gamePath, out var balanceBin, out var font3, out var etcRes);
        gameEngine.InitializeEngine();

        var alunCdExe = new AlunCdExe(gamePath);

        ExtractDataFromAlunCdExe(alunCdExe, extractionPath);
        ExtractDataFromBalanceBin(balanceBin, extractionPath);
        ExtractDataFromScreenFolder(font3, gameEngine.StaticVariables, extractionPath);
        ExtractDataFromEtcRes(etcRes, gameEngine.StaticVariables, extractionPath);
        var psxFramesPerSecond = etcRes is EtcResUsa ? 60 : 50;
        var tiledTilesetLayoutMode = ReadTiledTilesetLayoutMode(args, "--tiled-tileset-layout", TiledTilesetLayoutMode.Compact);
        ExtractDataFromDatasBin(gameEngine.DatasBin, gameEngine.StaticVariables, extractionPath, psxFramesPerSecond, tiledTilesetLayoutMode);
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

    private static TiledTilesetLayoutMode ReadTiledTilesetLayoutMode(string[] args, string optionName, TiledTilesetLayoutMode defaultValue)
    {
        for (var index = 0; index + 1 < args.Length; index++)
        {
            if (!string.Equals(args[index], optionName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (Enum.TryParse<TiledTilesetLayoutMode>(args[index + 1], true, out var value))
            {
                return value;
            }

            throw new ArgumentException($"Unsupported value '{args[index + 1]}' for {optionName}. Expected 'original' or 'compact'.");
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

    private static void ExtractDataFromBalanceBin(BalanceBin balanceBin, string extractionPath)
    {
        var balanceBinPath = Path.Combine(extractionPath, "data");
        Directory.CreateDirectory(balanceBinPath);
        File.WriteAllText(Path.Combine(balanceBinPath, $"{Path.GetFileName(balanceBin.FileName)}.json"), JsonSerializer.Serialize(balanceBin, _jsonSerializerOptions));
    }

    private static void ExtractDataFromScreenFolder(Font3 font3, StaticVariables staticVariables, string extractionPath)
    {
        var screenPath = Path.Combine(extractionPath, "ui");
        Directory.CreateDirectory(screenPath);
        font3.FontBitmapTim.Save(Path.Combine(screenPath, "font3.png"), ImageFormat.Png);
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

    private static void ExtractDataFromDatasBin(DatasBin datasBin, StaticVariables staticVariables, string extractionPath, int psxFramesPerSecond, TiledTilesetLayoutMode tiledTilesetLayoutMode)
    {
        datasBin.LoadingScreen.Save(Path.Combine(extractionPath, "data", "loading_screen.png"), ImageFormat.Png);

        var tileAnimDescriptors = GameInitializer.CreateTileAnimDescriptors(0);
        EntityNames.Load(EntityNames.Language.French);

        var dataPath = Path.Combine(extractionPath, "data");
        Directory.CreateDirectory(dataPath);

        Console.WriteLine("Extract map alundra");
        using var br = datasBin.OpenBin();
        datasBin.AlundraGameMap.Load(br);
        SaveAlundraMap(datasBin.AlundraGameMap, dataPath);

        for (int i = 0; i < 483; i++)
        {
            Console.WriteLine($"Extract map {i}");
            var gameMap = datasBin.GameMaps[i];
            gameMap.Load(br);
            SaveMap(gameMap, i, dataPath, tileAnimDescriptors, psxFramesPerSecond, tiledTilesetLayoutMode);
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

    private static void SaveAlundraMap(GameMap gameMap, string extractionPath)
    {
        File.WriteAllText(Path.Combine(extractionPath, "map_alundra.json"), JsonSerializer.Serialize(gameMap, _jsonSerializerOptions));

        //var gameMapJson = ConvertGameMap(gameMap);
        //File.WriteAllText(Path.Combine(extractionPath, "map_alundra.json"), JsonSerializer.Serialize(gameMapJson, _jsonSerializerOptions));

        GameMapHelper.SaveSpriteSheet(gameMap, Path.Combine(extractionPath, "map_alundra_spritesheet.png"));
    }

    private static void SaveMap(GameMap gameMap, int id, string extractionPath, TileAnimDescriptor[] tileAnimDescriptors, int psxFramesPerSecond, TiledTilesetLayoutMode tiledTilesetLayoutMode)
    {
        //var gameMapJson = ConvertGameMap(gameMap);
        //File.WriteAllText(Path.Combine(extractionPath, $"map_{id}.json"), JsonSerializer.Serialize(gameMapJson, _jsonSerializerOptions));

        File.WriteAllText(Path.Combine(extractionPath, $"map_{id}.json"), JsonSerializer.Serialize(gameMap, _jsonSerializerOptions));

        //try to extract all entity infos from all map
        //GetEntitySpriteSheets(gameMap, id, extractionPath);

        GameMapHelper.SaveTileSheet(gameMap, Path.Combine(extractionPath, $"map_{id}_tilesheet.png"), tileAnimDescriptors);
        GameMapHelper.SaveSpriteSheet(gameMap, Path.Combine(extractionPath, $"map_{id}_spritesheet.png"));
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