namespace AlundraEngine.DatasBin;

public class SpriteRecord
{
    public readonly SpriteTableHeader Header;
    public readonly AnimationSet[] AnimSets;

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
}