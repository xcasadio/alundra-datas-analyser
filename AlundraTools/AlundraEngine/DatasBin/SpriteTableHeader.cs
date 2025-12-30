namespace AlundraEngine.DatasBin;

public class SpriteTableHeader
{
    public readonly int Sector5Id;
    public readonly long BinOffset;
    public readonly int MemoryAddress;
    public readonly int SpriteInfoMemoryAddress;

    public readonly int AnimationOffsetsPointer;
    public readonly int AnimationsPointer;
    public readonly int FrameCollisionPointer;
    public readonly int FramesPointer;

    public readonly byte MoreFlags;
    public readonly byte CanPickup;
    public readonly byte FlagsPortraitShadowType;
    public readonly byte ProgramLoad;
    public readonly byte ProgramTick;
    public readonly byte ProgramTouch;
    public readonly byte ProgramDeactivate;
    public readonly byte ProgramInteract;
    public readonly sbyte OffsetX;
    public readonly sbyte OffsetY;
    public readonly sbyte OffsetZ;
    public readonly byte SizeX;
    public readonly byte SizeY;
    public readonly byte SizeZ;
    public readonly byte BreakEffect;
    public readonly byte Contents;

    public readonly byte[] Ubuff;

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

        MoreFlags = br.ReadByte();//10
        CanPickup = br.ReadByte();//11
        FlagsPortraitShadowType = br.ReadByte();//12
        ProgramLoad = br.ReadByte();//13
        ProgramTick = br.ReadByte();//14
        ProgramTouch = br.ReadByte();//15
        ProgramDeactivate = br.ReadByte();//16
        ProgramInteract = br.ReadByte();//17
        OffsetX = br.ReadSByte();//18+0
        OffsetY = br.ReadSByte();//18+1
        OffsetZ = br.ReadSByte();//18+2
        SizeX = br.ReadByte();//18+3
        SizeY = br.ReadByte();//18+4
        SizeZ = br.ReadByte();//18+5
        BreakEffect = br.ReadByte();//18+6
        Contents = br.ReadByte();//18+7
    }
}