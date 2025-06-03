namespace AlundraEngine.DatasBin;

public class SpriteEffectRecord
{
    public SpriteEffectRecord(BinaryReader br, long binOffset, int id, int memaddr, int spriteInfoMemoryAddress)
    {
        _effectId = id;
        _binOffset = binOffset;
        _spriteInfoMemoryAddress = spriteInfoMemoryAddress;

        _animationOffsets = new int[255];
        var final = -1;

        for (var i = 0; i < _animationOffsets.Length; i++)
        {
            if (final != -1 && i >= final)
            {
                _animationCount = i;
                break;
            }

            _animationOffsets[i] = br.ReadInt16();

            if (final == -1)
            {
                final = _animationOffsets[i] / 2;
            }
        }

        //preload all of the animations here
        PreloadedAnims = new SiEffectAnimation[_animationCount];
        for (int i = 0; i < _animationCount; i++)
        {
            PreloadedAnims[i] = GetAnimation(br, _animationOffsets[i]);
        }
    }

    private readonly long _binOffset;
    private readonly int _spriteInfoMemoryAddress;
    private readonly int[] _animationOffsets;
    private readonly int _animationCount;
    private readonly int _effectId;
    public readonly SiEffectAnimation[] PreloadedAnims;

    public SiEffectAnimation GetAnimation(BinaryReader br, int animationOffset)
    {
        br.BaseStream.Position = _binOffset + animationOffset;
        var anim = new SiEffectAnimation(br, _effectId, (int)_binOffset, _spriteInfoMemoryAddress + animationOffset);
        return anim;
    }
}