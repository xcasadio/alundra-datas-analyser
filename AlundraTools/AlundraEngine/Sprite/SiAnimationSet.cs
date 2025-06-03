namespace AlundraEngine.Sprite;

public class SiAnimationSet
{
    public SiAnimationSet(BinaryReader br, long binoffset)
    {

        Header = new SiAnimationSetHeader(br, binoffset);
        Animoffsets = new int[(Header.Unknownpointer - Header.Animationoffsetspointer) / 2];
        for (var dex = 0; dex < Animoffsets.Length; dex++)
        {
            Animoffsets[dex] = br.ReadInt16();
        }
    }

    public SiAnimation GetAnimation(BinaryReader br, int animationoffset)
    {
        br.BaseStream.Position = Header.Binoffset + Header.Animationspointer + animationoffset;

        var anim = new SiAnimation(br, Header);

        return anim;
    }


    public readonly SiAnimationSetHeader Header;
    public readonly int[] Animoffsets;

}