namespace AlundraEngine.Gameplay;

/// <summary>
/// Values of <see cref="Entity.Status"/> (offset 0x10 of the original <c>Entity</c> struct).
///
/// The state machine is driven by <c>EntityManager.UpdateEntitiesEvents</c> @ 0x800386D0, which picks
/// the event program slot to run from the current status, and by
/// <c>EntityManager.RebuildEntityLists</c>, which keeps the "active" and "renderable" lists made of the
/// statuses in <see cref="EntityStatus.Normal"/>..<see cref="EntityStatus.Deactivated"/>.
/// </summary>
public enum EntityStatus
{
    /// <summary>
    /// Free slot. <c>EntityManager.FindFreeEntitySlot</c> hands out the first entity in that state and
    /// no event program runs.
    /// </summary>
    Destroyed = 0,

    /// <summary>
    /// Freshly spawned. The next event pass runs the load program (slot A) and moves the entity to
    /// <see cref="Normal"/>.
    /// </summary>
    Loaded = 1,

    /// <summary>
    /// Regular gameplay state: the tick / touch / interact programs run, and the deactivate triggers of
    /// <see cref="EntityFlags.DeactivateTriggerMask"/> are evaluated every frame.
    /// </summary>
    Normal = 2,

    /// <summary>
    /// The entity keeps being simulated and drawn but only runs the deactivate program (slot E). This
    /// is the state AI scripts use to park an entity in a one-shot behaviour.
    /// </summary>
    Deactivated = 3,

    /// <summary>
    /// Marked for destruction by <c>GameEngine.DestroyEntity</c>. No event program runs any more; the
    /// slot is recycled by the next <c>EntityManager</c> pass.
    /// </summary>
    FlagToDestroy = 4,

    /// <summary>Never produced by the transliterated code; kept for parity with the original enum.</summary>
    Unknown = 5
}

/// <summary>Convenience predicates over <see cref="EntityStatus"/>.</summary>
public static class EntityStatusExtensions
{
    /// <summary>
    /// True for <see cref="EntityStatus.Normal"/> and <see cref="EntityStatus.Deactivated"/>, the two
    /// statuses that make an entity simulated, collidable and drawn. Transliterated from the original
    /// unsigned trick <c>(uint)(status - 2) &lt; 2</c>.
    /// </summary>
    public static bool IsActive(this EntityStatus status)
        => status is EntityStatus.Normal or EntityStatus.Deactivated;
}
