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

/// <summary>The default extraction: every family, in a fixed order, from the bytes of the nine disc files.</summary>
public static class DefaultExtraction
{
    /// <summary>
    /// Builds the readers from the bytes and writes every file of the default extraction to the sink, in the order of the
    /// extractor's default mode. Every console line goes through log. notWritten receives the relative path of a music track
    /// that is silent, failed or refused, so the caller can remove a file a previous run wrote (the library never deletes).
    /// </summary>
    public static void Write(
        DefaultExtractionInputs inputs,
        TiledTilesetLayoutMode tiledTilesetLayoutMode,
        SpriteSheetLayoutMode spriteSheetLayoutMode,
        SequencerTickProfile tickProfile,
        IExtractionSink sink,
        Action<string> log,
        Action<string> notWritten)
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

        TextAndUiExtraction.WriteMemoryCardFrames(alunCdExe, sink);
        TextAndUiExtraction.WriteClosingImages(closingExe, sink);
        TextAndUiExtraction.WriteBalance(balanceBin, sink);
        TextAndUiExtraction.WriteFont3AndWind(font3, gameEngine.StaticVariables, inputs.Font3Tim, inputs.WindCl, sink);
        TextAndUiExtraction.WriteEtcStrings(etcRes, sink);
        var psxFramesPerSecond = etcRes is EtcResUsa ? 60 : 50;

        // DatasBin: loading screen, then every map. (The extractor's old per-entity sprite sheet listing loop printed nothing:
        // its dictionary was never filled, the code that filled it being commented out.)
        TextAndUiExtraction.WriteLoadingScreen(gameEngine.DatasBin, sink);
        var tileAnimDescriptors = GameInitializer.CreateTileAnimDescriptors(0);
        EntityNames.Load(EntityNames.Language.French);
        MapExtraction.WriteAllMaps(gameEngine.DatasBin, tileAnimDescriptors, psxFramesPerSecond, tiledTilesetLayoutMode, spriteSheetLayoutMode, sink, log);

        // Sound effects.
        log("Extract sound effects");
        var extraction = AudioExtraction.ComputeSfx(gameEngine.SoundBin);
        AudioExtraction.WriteSfx(extraction, sink);
        log($"Extracted {extraction.Records.Values.Count(r => r.Tones.Length > 0)}/{gameEngine.SoundBin.SfxRecords.Length - 1} sound effects ({extraction.Records.Values.Sum(r => r.Tones.Length)} WAV files)");
        log($"Sound effects exported from a sibling of their RefSfxId chain: {extraction.SiblingSourcedCount} ({extraction.CrossVabIds.Count} of them from another VAB: {string.Join(", ", extraction.CrossVabIds)})");

        // Music.
        log($"Extract BGM (tick {tickProfile.Name}, {tickProfile.TickRate.ToString(System.Globalization.CultureInfo.InvariantCulture)} Hz)");
        var summary = AudioExtraction.ExtractBgm(inputs.SoundBin, tickProfile, sink, log, notWritten);
        log($"Extracted BGM: rendered={summary.Rendered} silent={summary.Silent} failed={summary.Failed} (of {summary.MaxSoundIndex})");
        log(VoicePitchGuard.FormatReport());
    }
}
