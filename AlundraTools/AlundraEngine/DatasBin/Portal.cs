namespace AlundraEngine.DatasBin;

public class Portal
{
    public Portal(BinaryReader br)
    {
        X1 = br.ReadByte();
        Y1 = br.ReadByte();
        X2 = br.ReadByte();
        Y2 = br.ReadByte();
        DestMapId = br.ReadInt16();
        DestTileX = br.ReadByte();
        DestTileY = br.ReadByte();
        ZLevel = br.ReadInt16();
        Flags = br.ReadUInt16();
    }

    public readonly byte X1;
    public readonly byte Y1;
    public readonly byte X2;
    public readonly byte Y2;
    public readonly short DestMapId;
    public readonly byte DestTileX;
    public readonly byte DestTileY;
    public readonly short ZLevel;
    public readonly ushort Flags;

    /// <summary>
    /// Bits 14-15 of <see cref="Flags"/> - direction (0..3) the player must face, and hold on the
    /// pad, for the warp to trigger; index into <c>SHORT_ARRAY_80022776</c>.
    /// </summary>
    public uint RequiredFacingDirection => (uint)(Flags >> 14);

    /// <summary>
    /// Bits 12-13 of <see cref="Flags"/> - direction (0..3) the player faces on arrival; index into
    /// <c>g_cardinalDirectionTable</c>.
    /// </summary>
    public int ArrivalDirectionIndex => (Flags & 0x3000) >> 12;

    /// <summary>Bits 4-6 of <see cref="Flags"/> - map transition effect id (fade style).</summary>
    public int TransitionEffectId => (Flags & 0x70) >> 4;

    /// <summary>Bits 0-3 of <see cref="Flags"/> - index into <c>g_warpBehaviorTable</c> (warp sound).</summary>
    public int WarpBehaviorId => Flags & 0xF;

    public override string ToString()
    {
        return $"x1:{X1} y1:{Y1} x2:{X2} y2:{Y2} map:{DestMapId} ->x:{DestTileX} ->y:{DestTileY} z:{ZLevel} f:{Flags}";
    }
}