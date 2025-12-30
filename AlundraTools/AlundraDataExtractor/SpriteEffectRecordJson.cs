using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record SpriteEffectRecordJson
{
    public int SpriteInfoMemoryAddress { get; set; }
    public int[] AnimationOffsets { get; set; }
    public int AnimationCount { get; set; }
    public int EffectId { get; set; }

    public SpriteEffectRecordJson(SpriteEffectRecord spriteEffectRecord)
    {
        EffectId = spriteEffectRecord.EffectId;
        SpriteInfoMemoryAddress = SpriteInfoMemoryAddress;
        AnimationOffsets = spriteEffectRecord.AnimationOffsets;
    }
}