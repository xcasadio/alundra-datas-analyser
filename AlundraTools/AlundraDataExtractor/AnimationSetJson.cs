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
    public SiAnimationJson[] SiAnimationJsons { get; set; }

    public AnimationSetJson(AnimationSet animationSet)
    {
        AnimationOffsets = animationSet.AnimationOffsets;
        Speed = animationSet.Speed;
        Sfx = animationSet.Sfx;
        Flags = animationSet.Flags;
        Acceleration = animationSet._C;
        U6 = animationSet.Acceleration;

        if (animationSet.PreloadedAnims.Count(x => x != null) > 0)
        {
            SiAnimationJsons = new SiAnimationJson[4];

            for (int i = 0; i < animationSet.PreloadedAnims.Length; i++)
            {
                SiAnimationJsons[i] = new SiAnimationJson(animationSet.PreloadedAnims[i]);
            }
        }
    }
}