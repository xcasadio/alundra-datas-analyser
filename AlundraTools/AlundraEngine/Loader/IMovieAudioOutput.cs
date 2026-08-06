namespace AlundraEngine.Loader;

/// <summary>
/// Audio sink for movie playback.
///
/// JUSTIFICATION: PSX hardware adaptation only.
/// RELATION: on the console the CD controller feeds XA sectors straight to the SPU, and the only
/// thing the game code touches is the CD input volume (GHIDRA: FUN_80027f10 @ 0x80027f10, which
/// calls SpuSetCommonAttr). There is no such path on desktop, so the decoded PCM has to be handed
/// to the host's audio device instead. Kept as an interface so AlundraEngine stays free of any
/// MonoGame reference.
/// </summary>
public interface IMovieAudioOutput
{
    /// <summary>Opens the device for a new movie. Any previously queued audio is discarded.</summary>
    void Start(int sampleRate, int channels);

    /// <summary>Queues interleaved 16-bit PCM for playback.</summary>
    void Submit(short[] samples, int offset, int count);

    /// <summary>
    /// Overall playback volume, 0..1. Mirrors the CD input volume the original ramps down at the
    /// end of a movie (<c>g_volume</c>, 0..0x7FFF).
    /// </summary>
    float Volume { get; set; }

    /// <summary>Number of buffers still queued; used to decide when playback may start.</summary>
    int PendingBufferCount { get; }

    /// <summary>Stops playback and releases the device.</summary>
    void Stop();
}
