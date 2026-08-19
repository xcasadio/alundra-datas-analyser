namespace AlundraEngine;

/// <summary>
/// Named bits of <see cref="StaticVariables.g_playerControlFlags"/> (global @ 0x800DC4B8).
///
/// While any bit is set the player is not under normal pad control; each bit records why. The
/// meanings below are derived from the transliterated call sites only (Ghidra evidence not yet
/// collected): the setter and every reader of each bit are listed so the inference can be rechecked.
/// </summary>
public static class PlayerControlFlags
{
    /// <summary>
    /// Bit 2 - the player is locked out of control by a script: event opcode 0x10 ("player lose
    /// control") sets it, opcode 0x11 ("player gain control") clears it, and the interact event
    /// handlers set it while the player talks to an entity. Enemy AI scripts test this bit to leave
    /// the player alone during the interaction.
    /// </summary>
    public const uint ControlLocked = 0x04;

    /// <summary>
    /// Bit 3 - a full-screen UI owns the game: main inventory, sub inventory, memory card screen and
    /// the debug menu all set it while open. Part of <see cref="GameplayBlockedMask"/>, so map events
    /// stop running.
    /// </summary>
    public const uint MenuOpen = 0x08;

    /// <summary>
    /// Bit 4 - the "message with background" box (item pickup text) is open in its keep-control
    /// variant: <c>UIManager</c> sets this bit instead of <see cref="MenuOpen"/> when the caller asks
    /// for the player to keep control, and both bits are cleared together when the box closes.
    /// </summary>
    public const uint MessageBox = 0x10;

    /// <summary>
    /// Bit 5 - the player is inside a forced sequence (warp departure, sand-cape ride, boss
    /// choreography). <c>PlayerManager.MovePlayer</c> skips normal input while it is set, and when it
    /// is the only bit set it drives the warp facing-direction search.
    /// </summary>
    public const uint ForcedSequence = 0x20;

    /// <summary>
    /// Bit 6 - dead bit: a Ghidra sweep of all 77 cross-references to the global @ 0x800DC4B8 found
    /// no instruction that sets it in the retail binary. It only appears inside the test masks
    /// <see cref="GameplayBlockedMask"/> (0x48) and <see cref="HpRegenBlockedMask"/> (0x7C).
    /// </summary>
    public const uint Unused40 = 0x40;

    /// <summary>
    /// Bit 7 - set by event opcode 0xC0 right after it forces the player's weapon, cleared by opcode
    /// 0xC1. It has no dedicated bit test: it acts through the whole-value <c>!= 0</c> gates
    /// (inventory opening @ 0x8002BB68/0x8002BC70, item use @ 0x8002ED80, warp @ 0x8002F1C4), so
    /// while it is set the player cannot open the menus or use items - the scripted weapon stays
    /// locked in place. Verified in Ghidra against the retail binary.
    /// </summary>
    public const uint ForcedWeapon = 0x80;

    /// <summary>Mask 0x48 - map events and world updates pause while any of these bits is set.</summary>
    public const uint GameplayBlockedMask = MenuOpen | Unused40;

    /// <summary>Mask 0x34 - normal player input processing is skipped while any of these bits is set.</summary>
    public const uint InputBlockedMask = ControlLocked | MessageBox | ForcedSequence;

    /// <summary>
    /// Mask 0x7C - passive HP regeneration from equipped healing items pauses while any of these
    /// bits is set (<c>UpdateItemEffectState</c> @ 0x800307E8, test at 0x80030C24). Note that
    /// <see cref="ForcedWeapon"/> is deliberately absent: regeneration keeps running with a scripted
    /// weapon.
    /// </summary>
    public const uint HpRegenBlockedMask =
        ControlLocked | MenuOpen | MessageBox | ForcedSequence | Unused40;
}
