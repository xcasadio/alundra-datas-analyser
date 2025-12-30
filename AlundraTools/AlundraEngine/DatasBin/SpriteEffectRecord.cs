namespace AlundraEngine.DatasBin;

public class SpriteEffectRecord
{
    public readonly long BinOffset;
    public readonly int SpriteInfoMemoryAddress;
    public readonly int[] AnimationOffsets;
    public readonly int AnimationCount;
    public readonly int EffectId;
    public readonly SiEffectAnimation[] PreloadedAnims;

    public SpriteEffectRecord(BinaryReader br, long binOffset, int id, int memoryAddress, int spriteInfoMemoryAddress)
    {
        EffectId = id;
        BinOffset = binOffset;
        SpriteInfoMemoryAddress = spriteInfoMemoryAddress;

        AnimationOffsets = new int[255];
        var final = -1;

        for (var i = 0; i < AnimationOffsets.Length; i++)
        {
            if (final != -1 && i >= final)
            {
                AnimationCount = i;
                break;
            }

            AnimationOffsets[i] = br.ReadInt16();

            if (final == -1)
            {
                final = AnimationOffsets[i] / 2;
            }
        }

        //preload all of the animations here
        PreloadedAnims = new SiEffectAnimation[AnimationCount];
        for (int i = 0; i < AnimationCount; i++)
        {
            PreloadedAnims[i] = GetAnimation(br, AnimationOffsets[i]);
        }
    }

    public SiEffectAnimation GetAnimation(BinaryReader br, int animationOffset)
    {
        br.BaseStream.Position = BinOffset + animationOffset;
        var anim = new SiEffectAnimation(br, EffectId, (int)BinOffset, SpriteInfoMemoryAddress + animationOffset);
        return anim;
    }
}