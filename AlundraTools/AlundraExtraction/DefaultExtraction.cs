using AlundraEngine;
using AlundraEngine.Balance;
using AlundraEngine.Closing;
using AlundraEngine.DatasBin;
using AlundraEngine.Editor;
using AlundraEngine.Etc;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using AlundraDataExtractor;

namespace AlundraExtraction;

/// <summary>
/// The nine disc files read by the default extraction, as bytes. EtcResName and BalanceBinName are the logical names passed to
/// the readers; BalanceBinName is the path as the path entry builds it, because data/BALANCE.BIN.json serializes BalanceBin.FileName.
/// </summary>
public sealed record DefaultExtractionInputs(
    byte[] DatasBin, byte[] BalanceBin, string BalanceBinName, byte[] SoundBin, byte[] EtcRes, string EtcResName,
    byte[] AlunCdExe, byte[] ClosingExe, byte[] Font3Tim, byte[] WindCl, byte[] WindTx);

/// <summary>
/// The objects built from the nine disc files by <see cref="DefaultExtraction.Load"/>: the initialized game engine (and through
/// it DatasBin, SoundBin and StaticVariables), the readers that sit beside it, and the raw bytes some writers still need.
/// </summary>
public sealed record DefaultExtractionContext(
    DefaultExtractionInputs Inputs, GameEngine GameEngine, BalanceBin BalanceBin, Font3 Font3, EtcRes EtcRes,
    AlunCdExe AlunCdExe, ClosingExeInspector ClosingExeInspector, byte[] Font3Tim, byte[] WindCl);

/// <summary>The default extraction: every family, in a fixed order, from the bytes of the nine disc files.</summary>
public static class DefaultExtraction
{
    /// <summary>
    /// Builds the readers from the bytes and initializes the game engine, without writing anything. Write(context, ...) writes
    /// the files of the default extraction from the result.
    /// </summary>
    public static DefaultExtractionContext Load(DefaultExtractionInputs inputs)
    {
        var datasBin = new DatasBin(inputs.DatasBin);
        var balanceBin = new BalanceBin(inputs.BalanceBin, inputs.BalanceBinName);
        var soundBin = new SoundBin(inputs.SoundBin);
        var font3 = new Font3(inputs.Font3Tim, inputs.WindCl, inputs.WindTx);
        EtcRes etcRes = inputs.EtcResName.Contains("usa", StringComparison.InvariantCultureIgnoreCase)
            ? new EtcResUsa(inputs.EtcRes, inputs.EtcResName)
            : new EtcResR(inputs.EtcRes, inputs.EtcResName);

        var gameEngine = new GameEngine(datasBin, balanceBin, soundBin, etcRes, font3, null);
        gameEngine.InitializeEngine();

        var alunCdExe = new AlunCdExe(inputs.AlunCdExe);
        var closingExe = new ClosingExeInspector(inputs.ClosingExe);

        return new DefaultExtractionContext(inputs, gameEngine, balanceBin, font3, etcRes, alunCdExe, closingExe, inputs.Font3Tim, inputs.WindCl);
    }

    /// <summary>Loads the nine disc files, then writes every file of the default extraction (see the context overload).</summary>
    public static void Write(
        DefaultExtractionInputs inputs,
        TiledTilesetLayoutMode tiledTilesetLayoutMode,
        SpriteSheetLayoutMode spriteSheetLayoutMode,
        SequencerTickProfile tickProfile,
        IExtractionSink sink,
        Action<string> log,
        Action<string> notWritten)
        => Write(Load(inputs), tiledTilesetLayoutMode, spriteSheetLayoutMode, tickProfile, sink, log, notWritten);

    /// <summary>
    /// Writes every file of the default extraction to the sink, in the order of the extractor's default mode: the text and
    /// interface family, the maps, the sound effects, then the music, each through its own public step below. Every console line
    /// goes through log. notWritten receives the relative path of a music track that is silent, failed or refused, so the caller
    /// can remove a file a previous run wrote (the library never deletes).
    /// </summary>
    public static void Write(
        DefaultExtractionContext context,
        TiledTilesetLayoutMode tiledTilesetLayoutMode,
        SpriteSheetLayoutMode spriteSheetLayoutMode,
        SequencerTickProfile tickProfile,
        IExtractionSink sink,
        Action<string> log,
        Action<string> notWritten)
    {
        WriteTextAndUi(context, sink);
        WriteMaps(context, tiledTilesetLayoutMode, spriteSheetLayoutMode, sink, log);
        WriteSoundEffects(context, sink, log);
        WriteMusic(context, tickProfile, sink, log, notWritten);
    }

    /// <summary>
    /// Text and interface family: memory card frames, closing images, balance, font3 and wind, ETC strings, then the loading
    /// screen (which Write used to write at the start of the DatasBin part; it logs nothing and shares no state with the map
    /// step, see WriteMaps).
    /// </summary>
    public static void WriteTextAndUi(DefaultExtractionContext context, IExtractionSink sink)
    {
        var inputs = context.Inputs;
        TextAndUiExtraction.WriteMemoryCardFrames(context.AlunCdExe, sink);
        TextAndUiExtraction.WriteClosingImages(context.ClosingExeInspector, sink);
        TextAndUiExtraction.WriteBalance(context.BalanceBin, sink);
        TextAndUiExtraction.WriteFont3AndWind(context.Font3, context.GameEngine.StaticVariables, inputs.Font3Tim, inputs.WindCl, sink);
        TextAndUiExtraction.WriteEtcStrings(context.EtcRes, sink);
        TextAndUiExtraction.WriteLoadingScreen(context.GameEngine.DatasBin, sink);
    }

    /// <summary>
    /// Maps family: the French entity names, the tile animation descriptors, then every map. (The extractor's old per-entity
    /// sprite sheet listing loop printed nothing: its dictionary was never filled, the code that filled it being commented out.)
    /// </summary>
    public static void WriteMaps(
        DefaultExtractionContext context,
        TiledTilesetLayoutMode tiledTilesetLayoutMode,
        SpriteSheetLayoutMode spriteSheetLayoutMode,
        IExtractionSink sink,
        Action<string> log)
    {
        var psxFramesPerSecond = context.EtcRes is EtcResUsa ? 60 : 50;
        var tileAnimDescriptors = GameInitializer.CreateTileAnimDescriptors(0);
        EntityNames.Load(EntityNames.Language.French);
        MapExtraction.WriteAllMaps(context.GameEngine.DatasBin, tileAnimDescriptors, psxFramesPerSecond, tiledTilesetLayoutMode, spriteSheetLayoutMode, sink, log);
    }

    /// <summary>Sound effects: computed, written, then the two summary lines.</summary>
    public static void WriteSoundEffects(DefaultExtractionContext context, IExtractionSink sink, Action<string> log)
    {
        var soundBin = context.GameEngine.SoundBin;
        log("Extract sound effects");
        var extraction = AudioExtraction.ComputeSfx(soundBin);
        AudioExtraction.WriteSfx(extraction, sink);
        log($"Extracted {extraction.Records.Values.Count(r => r.Tones.Length > 0)}/{soundBin.SfxRecords.Length - 1} sound effects ({extraction.Records.Values.Sum(r => r.Tones.Length)} WAV files)");
        log($"Sound effects exported from a sibling of their RefSfxId chain: {extraction.SiblingSourcedCount} ({extraction.CrossVabIds.Count} of them from another VAB: {string.Join(", ", extraction.CrossVabIds)})");
    }

    /// <summary>Music: every track rendered from SOUND.BIN and written, then the summary and the pitch guard report.</summary>
    public static void WriteMusic(DefaultExtractionContext context, SequencerTickProfile tickProfile, IExtractionSink sink, Action<string> log, Action<string> notWritten)
    {
        log($"Extract BGM (tick {tickProfile.Name}, {tickProfile.TickRate.ToString(System.Globalization.CultureInfo.InvariantCulture)} Hz)");
        var summary = AudioExtraction.ExtractBgm(context.Inputs.SoundBin, tickProfile, sink, log, notWritten);
        log($"Extracted BGM: rendered={summary.Rendered} silent={summary.Silent} failed={summary.Failed} (of {summary.MaxSoundIndex})");
        log(VoicePitchGuard.FormatReport());
    }
}
