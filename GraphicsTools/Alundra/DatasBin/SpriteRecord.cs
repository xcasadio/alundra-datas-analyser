namespace Alundra.DatasBin;

public class SpriteRecord
{
    public SpriteRecord(BinaryReader br, long binOffset, int id, int memoryAddress, int spriteInfoMemoryAddress)
    {
        Header = new SpriteTableHeader(br, binOffset, id, memoryAddress, spriteInfoMemoryAddress);
        AnimSets = new SiAnimSet[(Header.AnimationsPointer - Header.AnimationOffsetsPointer) / 14];

        for (var i = 0; i < AnimSets.Length; i++)
        {
            AnimSets[i] = new SiAnimSet(br, memoryAddress + 32 + i * 14);
        }

        //preload all of the animations here
        for (int i = 0; i < AnimSets.Length; i++)
        {
            for (int direction = 0; direction < 4; direction++)
            {
                if (AnimSets[i].AnimOffsets[direction] != 0xffff)
                {
                    AnimSets[i].PreloadedAnims[direction] = GetAnimation(br, AnimSets[i].AnimOffsets[direction]);

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
        var savepos = br.BaseStream.Position;
        var imageSetPointer = 0;//(its the first one)
        br.BaseStream.Position = Header.BinOffset + Header.FramesPointer + 0;
        var imageset = new SiImageSet(br, Header.Sector5Id << 16 | imageSetPointer, Header.SpriteInfoMemoryAddress + Header.FramesPointer + imageSetPointer, true);

        br.BaseStream.Position = savepos;
        return imageset;
    }

    public readonly SpriteTableHeader Header;
    public readonly SiAnimSet[] AnimSets;
}