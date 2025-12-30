using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record AnimationSetJson
{
    public int[] AnimationOffsets { get; set; }
    public ushort Speed { get; set; }
    public byte Sfx { get; set; }
    public byte Flags { get; set; }
    public byte Acceleration { get; set; }
    public byte U6 { get; set; }

    public AnimationSetJson(AnimationSet animationSet)
    {
        AnimationOffsets = animationSet.AnimationOffsets;
        Speed = animationSet.Speed;
        Sfx = animationSet.Sfx;
        Flags = animationSet.Flags;
        Acceleration = animationSet.Acceleration;
        U6 = animationSet.U6;
    }
}