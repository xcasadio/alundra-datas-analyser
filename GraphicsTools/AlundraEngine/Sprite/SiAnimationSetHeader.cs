namespace AlundraEngine.Sprite;

public class SiAnimationSetHeader
{
    public SiAnimationSetHeader(BinaryReader br, long binoffset)
    {
        Binoffset = binoffset;
        Animationoffsetspointer = br.ReadInt32();
        Animationspointer = br.ReadInt32();
        Unknownpointer = br.ReadInt32();
        Framespointer = br.ReadInt32();
        U1 = br.ReadByte();
        U2 = br.ReadByte();
        U3 = br.ReadByte();
        U4 = br.ReadByte();
        U5 = br.ReadByte();
        U6 = br.ReadByte();
        U7 = br.ReadByte();
        U8 = br.ReadByte();
        U9 = br.ReadByte();
        U10 = br.ReadByte();
        U11 = br.ReadByte();
        U12 = br.ReadByte();
        U13 = br.ReadByte();
        U14 = br.ReadByte();
        U15 = br.ReadByte();
        U16 = br.ReadByte();
    }
    public readonly long Binoffset;

    public readonly int Animationoffsetspointer;
    public readonly int Animationspointer;
    public readonly int Unknownpointer;
    public readonly int Framespointer;
    public byte U1;
    public byte U2;
    public byte U3;
    public byte U4;
    public byte U5;
    public byte U6;
    public byte U7;
    public byte U8;
    public byte U9;
    public byte U10;
    public byte U11;
    public byte U12;
    public byte U13;
    public byte U14;
    public byte U15;
    public byte U16;
}