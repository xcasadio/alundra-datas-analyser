namespace Alundra.DatasBin;

public class Portal
{
    public Portal(BinaryReader br)
    {
        X1 = br.ReadByte();
        Y1 = br.ReadByte();
        X2 = br.ReadByte();
        Y2 = br.ReadByte();
        DestMapId = br.ReadInt16();
        DestX = br.ReadByte();
        DestY = br.ReadByte();
        Unknown1 = br.ReadByte();
        Unknown2 = br.ReadByte();
        Unknown3 = br.ReadByte();
        Unknown4 = br.ReadByte();
    }
    public readonly byte X1;
    public readonly byte Y1;
    public readonly byte X2;
    public readonly byte Y2;
    public readonly short DestMapId;
    public readonly byte DestX;
    public readonly byte DestY;
    public readonly byte Unknown1;
    public readonly byte Unknown2;
    public readonly byte Unknown3;
    public readonly byte Unknown4;
}