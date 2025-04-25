namespace Alundra.DatasBin;

public class SiAnimSet
{
    public SiAnimSet(BinaryReader br, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        AnimOffsets = new int[4];
        for (var i = 0; i < AnimOffsets.Length; i++)
        {
            AnimOffsets[i] = br.ReadInt16();
        }

        Speed = br.ReadUInt16();
        Sfx = br.ReadByte();
        Flags = br.ReadByte();
        Acceleration = br.ReadByte();
        U6 = br.ReadByte();
        PreloadedAnims = new SiAnimation[4];
    }
    public readonly int MemoryAddress;
    public readonly int[] AnimOffsets;//4 of them for each direction
    public readonly SiAnimation[] PreloadedAnims;
    public readonly ushort Speed;
    public readonly byte Sfx;
    public readonly byte Flags;//0x80 adds 0x100 to sfx, does it mean global or map sfx?
    public readonly byte Acceleration;
    public readonly byte U6;

    public int DownOffset => AnimOffsets[(int)SiAnimDir.Down];
    public int UpOffset => AnimOffsets[(int)SiAnimDir.Up];
    public int LeftOffset => AnimOffsets[(int)SiAnimDir.Left];
    public int RightOffset => AnimOffsets[(int)SiAnimDir.Right];
}