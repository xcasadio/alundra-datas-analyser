namespace AlundraEngine.DatasBin;

public class SpriteRecord
{
    public SpriteRecord(BinaryReader br, long binOffset, int id, int memoryAddress, int spriteInfoMemoryAddress)
    {
        const int sizeofAnimationSet = 14;
        const int sizeofHeader = 32;

        Header = new SpriteTableHeader(br, binOffset, id, memoryAddress, spriteInfoMemoryAddress);
        AnimSets = new AnimationSet[(Header.AnimationsPointer - Header.AnimationOffsetsPointer) / sizeofAnimationSet];

        for (var i = 0; i < AnimSets.Length; i++)
        {
            AnimSets[i] = new AnimationSet(br, memoryAddress + sizeofHeader + i * sizeofAnimationSet);
        }

        //preload all of the animations here
        for (int i = 0; i < AnimSets.Length; i++)
        {
            for (int dirIndex = 0; dirIndex < 4; dirIndex++)
            {
                //TODO check direction
                var direction = dirIndex switch
                {
                    1 => 2,
                    2 => 1,
                    _ => dirIndex
                };

                if (AnimSets[i].AnimationOffsets[direction] != 0xffff)
                {
                    AnimSets[i].PreloadedAnims[dirIndex] = GetAnimation(br, AnimSets[i].AnimationOffsets[direction]);

                    /*DBFrame* frames = (DBFrame*)&(*spr->framesdata)[spr->animsets[animdex].diroffsets[dirdex]];
                    int framedex;
                    for (framedex = 0; framedex < 32; framedex++)
                    {
                        DBFrame* frame = &frames[framedex];
                        if ((frame->delay & 0x80) != 0x80)
                            break;

                        DBImageSet* imageset = (DBImageSet*)&(*spr->imagesetdata)[flipu16(frame->imagesetoffset) << 1];

                        int imagedex;
                        for (imagedex = 0; imagedex < imageset->numimages; imagedex++)
                        {
                            numimages++;
                            if (dex > 0)
                                cache_image(&imageset->images[imagedex], 1);
                        }

                    }*/
                }
            }
        }
    }

    public SiAnimation GetAnimation(BinaryReader br, int animationOffset)
    {
        br.BaseStream.Position = Header.BinOffset + Header.AnimationsPointer + animationOffset;
        var animation = new SiAnimation(br, Header, Header.SpriteInfoMemoryAddress + Header.AnimationsPointer + animationOffset);
        return animation;
    }

    public SiImageSet GetPortraitImageset(BinaryReader br)
    {
        var position = br.BaseStream.Position;
        var imageSetPointer = 0;//(its the first one)
        br.BaseStream.Position = Header.BinOffset + Header.FramesPointer + 0;
        var imageset = new SiImageSet(br, Header.Sector5Id << 16 | imageSetPointer, Header.SpriteInfoMemoryAddress + Header.FramesPointer + imageSetPointer, true);

        br.BaseStream.Position = position;
        return imageset;
    }

    public readonly SpriteTableHeader Header;
    public readonly AnimationSet[] AnimSets;
}