namespace AlundraEngine;

/// <summary>
/// PSX GPU blending modes (ABR values)
/// </summary>
public enum BlendMode
{
    /// <summary>No blending (opaque)</summary>
    None = -1,
    /// <summary>50% Background + 50% Foreground (average)</summary>
    Average = 0,
    /// <summary>Background + Foreground (additive)</summary>
    Additive = 1,
    /// <summary>Background - Foreground (subtractive)</summary>
    Subtractive = 2,
    /// <summary>Background + 25% Foreground (additive dimmed)</summary>
    AdditiveDim = 3
}