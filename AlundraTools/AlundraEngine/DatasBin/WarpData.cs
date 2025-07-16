namespace AlundraEngine.DatasBin;

public class WarpData
{
    public WarpData(BinaryReader br)
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
}