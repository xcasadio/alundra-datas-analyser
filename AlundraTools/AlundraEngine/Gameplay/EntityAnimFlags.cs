namespace AlundraEngine.Gameplay;

/// <summary>
/// Named bits of <see cref="Entity.AnimFlags"/>.
///
/// The field is reloaded on every animation change from the per-animation byte
/// <see cref="DatasBin.AnimationSet.Acceleration"/> (offset 0xD of the animation set header), in
/// <c>EntityManager.UpdateAnimation</c>. Each bit therefore describes a property of the animation
/// currently playing, not of the entity itself. Only the three bits below are read by the engine;
/// scripts can also clear <see cref="Invulnerable"/> directly on the player.
/// </summary>
public static class EntityAnimFlags
{
    /// <summary>
    /// Bit 4 - hide the drop shadow while this animation plays.
    /// Read by <c>EntityManager.UpdateEntitiesShadow</c>, which parks the shadow effect when set.
    /// </summary>
    public const int NoShadow = 0x10;

    /// <summary>
    /// Bit 6 - the entity cannot be damaged while this animation plays: every frame-collision and
    /// nearby-target search (<c>EntityManager.ProcessFrameCollisions</c>,
    /// <c>GameEngine.FUN_8003AF70</c>, projectile hit tests in the AI scripts) skips entities whose
    /// current animation carries this bit.
    /// </summary>
    public const int Invulnerable = 0x40;

    /// <summary>
    /// Bit 7 - the entity does not take part in entity-vs-entity collision while this animation
    /// plays. Checked everywhere alongside <see cref="EntityFlags.Collidable"/>: the entity is kept
    /// out of <c>g_collideableEntities</c> and ignored by the Z-collision and platform tests.
    /// </summary>
    public const int NoEntityCollision = 0x80;
}
