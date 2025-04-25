namespace Alundra.DatasBin;

public class SpriteTableHeader
{
    public SpriteTableHeader(BinaryReader br, long binOffset, int id, int memoryAddress, int spriteInfoMemoryAddress)
    {
        SpriteInfoMemoryAddress = spriteInfoMemoryAddress;
        MemoryAddress = memoryAddress;
        Sector5Id = id;
        BinOffset = binOffset;
        AnimationOffsetsPointer = br.ReadInt32();
        AnimationsPointer = br.ReadInt32();
        FrameCollisionPointer = br.ReadInt32();
        FramesPointer = br.ReadInt32();

        Ubuff = new byte[16];
        br.Read(Ubuff, 0, Ubuff.Length);
        br.BaseStream.Position -= 16;

        Moreflags = br.ReadByte();//10
        CanPickup = br.ReadByte();//11
        FlagsPortraitShadowtype = br.ReadByte();//12
        ProgramLoad = br.ReadByte();//13
        ProgramTick = br.ReadByte();//14
        ProgramTouch = br.ReadByte();//15
        ProgramDeactivate = br.ReadByte();//16
        ProgramInteract = br.ReadByte();//17
        Xmod = br.ReadSByte();//18+0
        Ymod = br.ReadSByte();//18+1
        Zmod = br.ReadSByte();//18+2
        Width = br.ReadByte();//18+3
        Depth = br.ReadByte();//18+4
        Height = br.ReadByte();//18+5
        Breakeffect = br.ReadByte();//18+6
        Contents = br.ReadByte();//18+7
    }
    public readonly int Sector5Id;
    public readonly long BinOffset;
    public readonly int MemoryAddress;
    public readonly int SpriteInfoMemoryAddress;

    public readonly int AnimationOffsetsPointer;
    public readonly int AnimationsPointer;
    public readonly int FrameCollisionPointer;
    public readonly int FramesPointer;
    public readonly byte[] Ubuff;

    public readonly byte Moreflags;
    public readonly byte CanPickup;
    public readonly byte FlagsPortraitShadowtype;
    public readonly byte ProgramLoad;
    public readonly byte ProgramTick;
    public readonly byte ProgramTouch;
    public readonly byte ProgramDeactivate;
    public readonly byte ProgramInteract;
    public readonly sbyte Xmod;
    public readonly sbyte Ymod;
    public readonly sbyte Zmod;
    public readonly byte Width;
    public readonly byte Depth;
    public readonly byte Height;
    public readonly byte Breakeffect;
    public readonly byte Contents;
}