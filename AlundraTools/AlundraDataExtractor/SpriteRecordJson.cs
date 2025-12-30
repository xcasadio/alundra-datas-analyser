using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record SpriteRecordJson
{
    public SpriteRecordHeaderJson Header { get; set; }
    public AnimationSetJson[] AnimationSet { get; set; }

    public SpriteRecordJson(SpriteRecord spriteRecord)
    {
        Header = new SpriteRecordHeaderJson(spriteRecord.Header);
        AnimationSet = spriteRecord.AnimSets.Select(x => new AnimationSetJson(x)).ToArray();
    }
}