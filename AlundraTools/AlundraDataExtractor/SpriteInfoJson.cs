using AlundraEngine.DatasBin;

namespace AlundraDataExtractor;

record SpriteInfoJson
{
    public SpriteInfoEventCodesJson EventCodes { get; set; }
    public SpriteInfoEntityJson[] Entities { get; set; }
    public MapEventRecordJson[] MapEventRecord { get; set; }
    public int[] SpriteTable { get; set; }
    public SpriteRecordJson[] SpriteRecords { get; set; }
    public int[] SpriteEffectTable { get; set; }
    public SpriteEffectRecordJson[] SpriteEffectRecords { get; set; }
    public int NumSpriteEffects { get; set; }
    public MapEffectRecordJson[] MapEffectRecords { get; set; }

    public SpriteInfoJson()
    {

    }

    public SpriteInfoJson(SpriteInfo spriteInfo)
    {
        EventCodes = new SpriteInfoEventCodesJson(spriteInfo.EventCodes);
        Entities = spriteInfo.Entities.Entities.Where(x => x != null).Select(x => new SpriteInfoEntityJson(x)).ToArray();
        MapEventRecord = spriteInfo.MapEvents.Records.Where(x => x != null).Select(x => new MapEventRecordJson(x)).ToArray();
        SpriteTable = spriteInfo.SpriteTable;
        SpriteRecords = spriteInfo.SpriteRecords.Where(x => x != null).Select(sr => new SpriteRecordJson(sr)).ToArray();
        SpriteEffectTable = spriteInfo.SpriteEffectTable;
        SpriteEffectRecords = spriteInfo.SpriteEffectRecords.Where(x => x != null).Select(s => new SpriteEffectRecordJson(s)).ToArray();
        NumSpriteEffects = spriteInfo.NumSpriteEffects;
        MapEffectRecords = spriteInfo.MapEffectRecords.Where(x => x != null).Select(m => new MapEffectRecordJson(m)).ToArray();
    }
}