namespace Alundra.DatasBin;

public class MapEffectRecord
{
    public MapEffectRecord(BinaryReader br)
    {
        X1 = br.ReadByte();
        Y1 = br.ReadByte();
        X2 = br.ReadByte();
        Y2 = br.ReadByte();
        Flags = br.ReadByte();
        EffectId = br.ReadByte();
        X = br.ReadByte();
        Y = br.ReadByte();
        Z = br.ReadByte();
        AnimId = br.ReadByte();

        U1 = br.ReadByte();//probably just padding
        U2 = br.ReadByte();//padding
    }
    public readonly byte X1;//player must be within these map tiles
    public readonly byte X2;//player must be within these map tiles
    public readonly byte Y1;//player must be within these map tiles
    public readonly byte Y2;//player must be within these map tiles
    public readonly byte Flags;//0x80 ismapsprite //4
    public readonly byte EffectId;//5
    public readonly byte X;//6
    public readonly byte Y;//7
    public readonly byte Z;//8
    public readonly byte AnimId;//9

    public byte U1, U2;

}