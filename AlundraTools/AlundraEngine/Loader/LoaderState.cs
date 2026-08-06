namespace AlundraEngine.Loader;

/// <summary>
/// States of the boot sequence, following the original's control flow.
/// </summary>
/// <remarks>
/// GHIDRA: main @ 0x80025668 runs <c>RunLoadingScreenIntro</c> once and then loops forever on
/// <c>LaunchGame</c> @ 0x800255f0, which alternates movie playback with the title menu
/// (<c>MainLoop</c> @ 0x8002538c).
/// </remarks>
public enum LoaderState
{
    /// <summary>GHIDRA: RunLoadingScreenIntro @ 0x80024fa8 — boot logo, shown once.</summary>
    LoadingScreen,

    /// <summary>GHIDRA: PlayMovie(0, 0x28, g_EURO_OP_MOV_fileInfo, ...) — publisher movie.</summary>
    PlayMovieEuro,

    /// <summary>GHIDRA: PromptNewGameOrContinue @ 0x80021c28 — title screen, New Game / Continue.</summary>
    TitleMenu,

    /// <summary>GHIDRA: RunLoaderMainSequence @ 0x80024e28 — save slot selection.</summary>
    SlotSelection,

    /// <summary>GHIDRA: PlayMovie(8, 0x10, g_ARAN_OP_MOV_fileInfo, ...) — attract-mode intro.</summary>
    PlayMovieIntro,

    /// <summary>GHIDRA: LoadExec("cdrom:\ALUN_CD.EXE;1") — hand over to the game.</summary>
    Finished
}
