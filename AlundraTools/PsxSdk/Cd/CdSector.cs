namespace PsxSdk.Cd;

/// <summary>
/// CD-ROM sector geometry constants.
/// </summary>
public static class CdSector
{
    /// <summary>Raw sector size on a CD (sync + header + subheader + data + EDC/ECC).</summary>
    public const int RawSize = 2352;

    /// <summary>User data of a Mode 2 Form 1 sector — the size STR video sectors carry.</summary>
    public const int Form1UserDataSize = 2048;

    /// <summary>User data of a Mode 2 Form 2 sector — the size XA-ADPCM audio sectors carry.</summary>
    public const int Form2UserDataSize = 2324;
}
