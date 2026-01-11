namespace AlundraEngine.DatasBin;

public class AnimationSet
{
    public readonly int[] AnimationOffsets;//4 of them for each direction
    public readonly ushort Speed; //0x8
    public readonly byte Sfx; // 0xa
    public readonly byte Flags; //0xb
    public readonly byte _C; // 0xc
    public readonly byte Acceleration; // 0xd

    public readonly int MemoryAddress;
    public readonly SiAnimation[] PreloadedAnims;

    public int IsZForceApplied => (short)((Flags << 8) | Sfx);

    public int DownOffset => AnimationOffsets[(int)SiAnimDir.Down];
    public int UpOffset => AnimationOffsets[(int)SiAnimDir.Up];
    public int LeftOffset => AnimationOffsets[(int)SiAnimDir.Left];
    public int RightOffset => AnimationOffsets[(int)SiAnimDir.Right];

    public AnimationSet(BinaryReader br, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        AnimationOffsets = new int[4];
        for (var i = 0; i < AnimationOffsets.Length; i++)
        {
            AnimationOffsets[i] = br.ReadInt16();
        }

        Speed = br.ReadUInt16();
        Sfx = br.ReadByte();
        Flags = br.ReadByte();
        _C = br.ReadByte();
        Acceleration = br.ReadByte();
        PreloadedAnims = new SiAnimation[4];
    }
}