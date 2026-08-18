namespace AlundraEngine.Gameplay;

/// <summary>
/// Named bits of <see cref="Entity.Flags"/> (offset 0x6C of the original <c>Entity</c> struct).
///
/// The field is built once at spawn time in <c>EntityManager.SpawnEntity</c> from three bytes of the
/// sprite table header (<see cref="DatasBin.SpriteTableHeader"/>):
/// <code>
///   Flags = MoreFlags                       //  bits  0-7   (header byte 0x10)
///         | CanPickup                &lt;&lt; 8   //  bits  8-15  (header byte 0x11)
///         | FlagsPortraitShadowType  &lt;&lt; 16; //  bits 16-23  (header byte 0x12)
/// </code>
/// Bits 24-31 are never produced by the data and are never read by the engine.
///
/// Event scripts only reach the low 16 bits: opcode 0x62 ORs a 16 bit mask, opcode 0x63 clears a
/// 16 bit mask, opcode 0xAC rewrites the shadow size field. Every meaning documented below is backed
/// by a real read site in the transliterated engine; bits without a known reader are left unnamed
/// rather than guessed at.
/// </summary>
public static class EntityFlags
{
    public const uint None = 0x00000000;

    // ---------------------------------------------------------------------------------------------
    // Bits 0-7 - SpriteTableHeader.MoreFlags
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    /// Bit 0 - the entity belongs to collision/damage class A.
    /// <para>Terrain: tiles whose ground property byte has bit 4 set (pits/holes) become solid for this
    /// entity. <c>PhysicsEngine.GetCollisionFlags</c> @ 0x800373E4,
    /// <c>PhysicsEngine.GetCollisionFlagsWithPlayer</c> @ 0x80037488 and
    /// <c>EntityGameplayManager.ComputeEntityGroundHeight</c> all fold this bit into the tile mask as
    /// 0x1000.</para>
    /// <para>Combat: an attacker carrying <see cref="HitsClassA"/> can damage this entity
    /// (<c>EntityManager.ProcessFrameCollisions</c>).</para>
    /// <para>Toggled by script opcodes 0x2A / 0x2B.</para>
    /// </summary>
    public const uint ClassA = 0x00000001;

    /// <summary>
    /// Bit 1 - this entity's attacks can damage entities carrying <see cref="ClassB"/>.
    /// </summary>
    public const uint HitsClassB = 0x00000002;

    /// <summary>
    /// Bit 2 - this entity's attacks can damage entities carrying <see cref="ClassA"/>.
    /// </summary>
    public const uint HitsClassA = 0x00000004;

    /// <summary>
    /// Bit 3 - the entity belongs to collision/damage class B.
    /// <para>Terrain: tiles whose walkability byte has bit 0 set become solid for this entity (the mask
    /// used by <c>GetCollisionFlags</c> goes from 0x40 to 0x41).</para>
    /// <para>Combat: an attacker carrying <see cref="HitsClassB"/> can damage this entity.</para>
    /// <para>Toggled by script opcodes 0x28 / 0x29.</para>
    /// </summary>
    public const uint ClassB = 0x00000008;

    /// <summary>
    /// Bit 4 - run the deactivate program (slot E) as soon as the entity hits something: either
    /// <see cref="Entity.ForceAdjusted"/> or <see cref="Entity.IsOnGround"/> is non zero.
    /// Checked by <c>EntityManager.UpdateEntitiesEvents</c> @ 0x800386D0.
    /// </summary>
    public const uint DeactivateOnImpact = 0x00000010;

    /// <summary>
    /// Bit 5 - run the deactivate program (slot E) when <see cref="Entity.HitCounter"/> is non zero.
    /// Checked by <c>EntityManager.UpdateEntitiesEvents</c> @ 0x800386D0.
    /// </summary>
    public const uint DeactivateOnHit = 0x00000020;

    /// <summary>
    /// Bit 6 - run the deactivate program (slot E) once the animation wrapped around
    /// (<see cref="Entity.ForceResetAnimationFlag"/> is non zero). By far the most used flag of the
    /// whole field: nearly every "play this animation then continue" step of the AI scripts sets it.
    /// Checked by <c>EntityManager.UpdateEntitiesEvents</c> @ 0x800386D0.
    /// </summary>
    public const uint DeactivateOnAnimationEnd = 0x00000040;

    /// <summary>
    /// Bit 7 - the entity takes part in entity vs entity collision. Entities carrying it are pushed
    /// into <c>g_collideableEntities</c> every frame and are the only ones considered by
    /// <c>PhysicsEngine.CheckEntityCollisionDown/Up</c>, <c>GetCollisionOnZ</c> @ 0x80037F28 and
    /// <c>FindEntityCollisionCandidate</c> @ 0x80036F34. Animation flag 0x80 suppresses it for the
    /// current frame.
    /// </summary>
    public const uint Collidable = 0x00000080;

    // ---------------------------------------------------------------------------------------------
    // Bits 8-15 - SpriteTableHeader.CanPickup
    // ---------------------------------------------------------------------------------------------

    /// <summary>
    /// Bit 8 - the entity is simulated on the Z axis: gravity is applied, <see cref="Entity.ForceZ"/>
    /// is reset on landing, and the tile/VRAM sampling of <c>UpdateEntityTiles</c> only runs when the
    /// bit is clear. Set by script opcode 0x16 ("high gravity"), cleared by 0x17 ("low gravity").
    /// </summary>
    public const uint Gravity = 0x00000100;

    /// <summary>Shift of the 2 bit pickup kind field (see <see cref="EntityPickupKind"/>).</summary>
    public const int PickupKindShift = 9;

    /// <summary>
    /// Bits 9-10 - how the player may lift and carry this entity, decoded by
    /// <c>PlayerManager.PlayerTryInteractWithEntity</c> @ 0x8002EDBC. See <see cref="EntityPickupKind"/>.
    /// </summary>
    public const uint PickupKindMask = 0x00000600;

    /// <summary>
    /// Bit 11 - the entity belongs to collision/damage class C; an attacker carrying
    /// <see cref="HitsClassC"/> can damage it.
    /// </summary>
    public const uint ClassC = 0x00000800;

    /// <summary>
    /// Bit 12 - this entity's attacks can damage entities carrying <see cref="ClassC"/>.
    /// </summary>
    public const uint HitsClassC = 0x00001000;

    /// <summary>
    /// Bit 13 - disable the "slide along the obstacle" retry of <c>PhysicsEngine.MoveEntity</c>: the
    /// entity stops dead against a blocked tile instead of being nudged sideways.
    /// Set by script opcode 0x46, cleared by 0x45.
    /// </summary>
    public const uint NoObstacleSlide = 0x00002000;

    /// <summary>
    /// Bit 14 - the entity never carries riders. <c>PhysicsEngine.CheckRidingEntities</c> @ 0x800364C8
    /// only considers entities matching <c>(Flags &amp; (NoRiders | Gravity)) == Gravity</c>.
    /// </summary>
    public const uint NoRiders = 0x00004000;

    /// <summary>
    /// Bit 15 - door / warp style entity: walking into it does not fire the interact program by
    /// itself, the action button must be pressed. <c>PlayerManager</c> also remembers such an entity in
    /// <c>g_lastValidWarpEntity</c> when the player collides with it.
    /// </summary>
    public const uint InteractRequiresButton = 0x00008000;

    // ---------------------------------------------------------------------------------------------
    // Bits 16-23 - SpriteTableHeader.FlagsPortraitShadowType
    // ---------------------------------------------------------------------------------------------

    /// <summary>Shift of the 3 bit shadow size field.</summary>
    public const int ShadowSizeShift = 16;

    /// <summary>
    /// Bits 16-18 - size of the drop shadow. <c>EntityManager.UpdateEntitiesShadow</c> uses
    /// <c>(size - 1) - ((PosZ - FloorHeight) &gt;&gt; 20)</c> as the shadow animation id, so 0 means
    /// "no shadow" and a larger value means a wider shadow. Rewritten by script opcode 0xAC.
    /// </summary>
    public const uint ShadowSizeMask = 0x00070000;

    /// <summary>
    /// Bit 20 - destroy the entity (break effect 6) as soon as it stands on a slope of type 4.
    /// Checked by <c>EntityManager.UpdateEntitiesEvents</c> @ 0x800386D0.
    /// </summary>
    public const uint DestroyOnSlidingSlope = 0x00100000;

    /// <summary>
    /// Bit 21 - destroy the entity (no break effect) as soon as its combined tile VRAM flags match
    /// 0x8004. Checked by <c>EntityManager.UpdateEntitiesEvents</c> @ 0x800386D0.
    /// </summary>
    public const uint DestroyOnVramFlags = 0x00200000;

    /// <summary>
    /// Bit 23 - the entity owns a dialogue portrait; the dialogue scripts show it instead of a plain
    /// text box.
    /// </summary>
    public const uint HasPortrait = 0x00800000;

    // ---------------------------------------------------------------------------------------------
    // Composite masks
    // ---------------------------------------------------------------------------------------------

    /// <summary>All "run the deactivate program when ..." triggers.</summary>
    public const uint DeactivateTriggerMask =
        DeactivateOnImpact | DeactivateOnHit | DeactivateOnAnimationEnd;

    /// <summary>All "I belong to that class" bits.</summary>
    public const uint ClassMask = ClassA | ClassB | ClassC;

    /// <summary>All "my attacks reach that class" bits.</summary>
    public const uint HitsClassMask = HitsClassA | HitsClassB | HitsClassC;

    /// <summary>Bits no known code path reads, kept so <see cref="Describe"/> can report them.</summary>
    public const uint UnknownMask = 0xFF480000;

    // ---------------------------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------------------------

    public static bool Has(uint flags, uint mask) => (flags & mask) != 0;

    public static bool HasAll(uint flags, uint mask) => (flags & mask) == mask;

    public static EntityPickupKind GetPickupKind(uint flags)
        => (EntityPickupKind)((flags & PickupKindMask) >> PickupKindShift);

    public static uint GetShadowSize(uint flags) => (flags & ShadowSizeMask) >> ShadowSizeShift;

    public static uint WithShadowSize(uint flags, uint shadowSize)
        => (flags & ~ShadowSizeMask) | ((shadowSize << ShadowSizeShift) & ShadowSizeMask);

    /// <summary>Readable dump, for the logs and the runtime inspector.</summary>
    public static string Describe(uint flags)
    {
        if (flags == 0)
        {
            return nameof(None);
        }

        var parts = new List<string>();

        void Add(uint mask, string name)
        {
            if ((flags & mask) != 0)
            {
                parts.Add(name);
            }
        }

        Add(ClassA, nameof(ClassA));
        Add(HitsClassB, nameof(HitsClassB));
        Add(HitsClassA, nameof(HitsClassA));
        Add(ClassB, nameof(ClassB));
        Add(DeactivateOnImpact, nameof(DeactivateOnImpact));
        Add(DeactivateOnHit, nameof(DeactivateOnHit));
        Add(DeactivateOnAnimationEnd, nameof(DeactivateOnAnimationEnd));
        Add(Collidable, nameof(Collidable));
        Add(Gravity, nameof(Gravity));

        var pickupKind = GetPickupKind(flags);
        if (pickupKind != EntityPickupKind.NotLiftable)
        {
            parts.Add($"Pickup={pickupKind}");
        }

        Add(ClassC, nameof(ClassC));
        Add(HitsClassC, nameof(HitsClassC));
        Add(NoObstacleSlide, nameof(NoObstacleSlide));
        Add(NoRiders, nameof(NoRiders));
        Add(InteractRequiresButton, nameof(InteractRequiresButton));

        var shadowSize = GetShadowSize(flags);
        if (shadowSize != 0)
        {
            parts.Add($"ShadowSize={shadowSize}");
        }

        Add(DestroyOnSlidingSlope, nameof(DestroyOnSlidingSlope));
        Add(DestroyOnVramFlags, nameof(DestroyOnVramFlags));
        Add(HasPortrait, nameof(HasPortrait));

        var unknown = flags & UnknownMask;
        if (unknown != 0)
        {
            parts.Add($"Unknown(0x{unknown:x8})");
        }

        return string.Join('|', parts);
    }
}

/// <summary>
/// Value of the 2 bit <see cref="EntityFlags.PickupKindMask"/> field, as decoded by
/// <c>PlayerManager.PlayerTryInteractWithEntity</c> @ 0x8002EDBC.
/// </summary>
public enum EntityPickupKind
{
    /// <summary>The player cannot lift this entity at all.</summary>
    NotLiftable = 0,

    /// <summary>The player can always lift this entity.</summary>
    Liftable = 1,

    /// <summary>Liftable only while the player does not own item 0x3B.</summary>
    LiftableUnlessItem3B = 2,

    /// <summary>
    /// Same gate as <see cref="LiftableUnlessItem3B"/>, but the player switches to the hold /
    /// jump-with-object animations while carrying it.
    /// </summary>
    LiftableHeavy = 3
}
