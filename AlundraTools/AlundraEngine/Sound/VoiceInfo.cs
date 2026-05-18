using System.Runtime.InteropServices;

namespace AlundraEngine.Sound;

public class VoiceInfo
{
    public int[] VoiceSfxIds = new int[24];//0x00  the sfxids assigned to each voice
    public int[] VoiceVabIds = new int[24];//0x60   the vabids assigned to each voice
    public int[] VoiceToneNums = new int[24];//0xc0
    public int[] VoiceVolumes = new int[24];//0x120
    public int[] VoicePans = new int[24];//0x180
}


// PARTIAL: runtime sequence track layout closed by xrefs, full command semantics still unknown
[StructLayout(LayoutKind.Explicit, Size = 0xAC)]
public struct SequenceTrackState
{
    [FieldOffset(0x00)] public uint field_0x00;
    [FieldOffset(0x04)] public int SeqPosition;
    [FieldOffset(0x08)] public int SeqStartPos;
    [FieldOffset(0x0C)] public int SeqLoopPos;
    [FieldOffset(0x10)] public byte field_0x10;
    [FieldOffset(0x11)] public byte MessageType;
    [FieldOffset(0x12)] public byte CurrentChannel;
    [FieldOffset(0x13)] public byte field_0x13;
    [FieldOffset(0x14)] public ushort field_0x14;
    [FieldOffset(0x16)] public byte field_0x16;
    [FieldOffset(0x17)] public byte Orientation0;
    [FieldOffset(0x18)] public byte Orientation1;
    [FieldOffset(0x19)] public byte Orientation2;
    [FieldOffset(0x1A)] public byte Orientation3;
    [FieldOffset(0x1B)] public byte Orientation4;
    [FieldOffset(0x1C)] public byte Orientation5;
    [FieldOffset(0x1D)] public byte Orientation6;
    [FieldOffset(0x1E)] public byte Orientation7;
    [FieldOffset(0x1F)] public byte Orientation8;
    [FieldOffset(0x20)] public byte Orientation9;
    [FieldOffset(0x21)] public byte Orientation10;
    [FieldOffset(0x22)] public byte Orientation11;
    [FieldOffset(0x23)] public byte Orientation12;
    [FieldOffset(0x24)] public byte Orientation13;
    [FieldOffset(0x25)] public byte Orientation14;
    [FieldOffset(0x26)] public byte Orientation15;
    [FieldOffset(0x27)] public byte field_0x27;
    [FieldOffset(0x28)] public byte Loops;
    [FieldOffset(0x29)] public byte field_0x29;
    [FieldOffset(0x2A)] public byte field_0x2A;
    [FieldOffset(0x2B)] public byte field_0x2B;
    [FieldOffset(0x2C)] public byte Channel0;
    [FieldOffset(0x2D)] public byte Channel1;
    [FieldOffset(0x2E)] public byte Channel2;
    [FieldOffset(0x2F)] public byte Channel3;
    [FieldOffset(0x30)] public byte Channel4;
    [FieldOffset(0x31)] public byte Channel5;
    [FieldOffset(0x32)] public byte Channel6;
    [FieldOffset(0x33)] public byte Channel7;
    [FieldOffset(0x34)] public byte Channel8;
    [FieldOffset(0x35)] public byte Channel9;
    [FieldOffset(0x36)] public byte Channel10;
    [FieldOffset(0x37)] public byte Channel11;
    [FieldOffset(0x38)] public byte Channel12;
    [FieldOffset(0x39)] public byte Channel13;
    [FieldOffset(0x3A)] public byte Channel14;
    [FieldOffset(0x3B)] public byte Channel15;
    [FieldOffset(0x3C)] public byte field_0x3C;
    [FieldOffset(0x3D)] public byte field_0x3D;
    [FieldOffset(0x3E)] public short field_0x3E;
    [FieldOffset(0x40)] public short field_0x40;
    [FieldOffset(0x42)] public short field_0x42;
    [FieldOffset(0x44)] public short field_0x44;
    [FieldOffset(0x46)] public short LoopCount;
    [FieldOffset(0x48)] public ushort TimesPlayed;
    [FieldOffset(0x4A)] public ushort Tempo;
    [FieldOffset(0x4C)] public short Vab;
    [FieldOffset(0x4E)] public ushort Volume0;
    [FieldOffset(0x50)] public ushort Volume1;
    [FieldOffset(0x52)] public ushort Volume2;
    [FieldOffset(0x54)] public ushort Volume3;
    [FieldOffset(0x56)] public ushort Volume4;
    [FieldOffset(0x58)] public ushort Volume5;
    [FieldOffset(0x5A)] public ushort Volume6;
    [FieldOffset(0x5C)] public ushort Volume7;
    [FieldOffset(0x5E)] public ushort Volume8;
    [FieldOffset(0x60)] public ushort Volume9;
    [FieldOffset(0x62)] public ushort Volume10;
    [FieldOffset(0x64)] public ushort Volume11;
    [FieldOffset(0x66)] public ushort Volume12;
    [FieldOffset(0x68)] public ushort Volume13;
    [FieldOffset(0x6A)] public ushort Volume14;
    [FieldOffset(0x6C)] public ushort Volume15;
    [FieldOffset(0x6E)] public short PreDelay;
    [FieldOffset(0x70)] public short CurrentTempo;
    [FieldOffset(0x72)] public ushort field_0x72;
    [FieldOffset(0x74)] public ushort field_0x74;
    [FieldOffset(0x76)] public ushort field_0x76;
    [FieldOffset(0x78)] public ushort field_0x78;
    [FieldOffset(0x7A)] public ushort field_0x7A;
    [FieldOffset(0x7C)] public uint field_0x7C;
    [FieldOffset(0x80)] public uint Playtime;
    [FieldOffset(0x84)] public uint field_0x84;
    [FieldOffset(0x88)] public uint Delay;
    [FieldOffset(0x8C)] public uint field_0x8C;
    [FieldOffset(0x90)] public uint Flags;
    [FieldOffset(0x94)] public uint field_0x94;
    [FieldOffset(0x98)] public uint field_0x98;
    [FieldOffset(0x9C)] public uint field_0x9C;
    [FieldOffset(0xA0)] public uint field_0xA0;
    [FieldOffset(0xA4)] public uint field_0xA4;
    [FieldOffset(0xA8)] public ushort field_0xA8;
    [FieldOffset(0xAA)] public ushort field_0xAA;
}

// PARTIAL: runtime voice slot layout closed by xrefs, full field semantics still unknown
[StructLayout(LayoutKind.Explicit, Size = 0x34)]
public struct VoiceRuntimeSlot
{
    [FieldOffset(0x00)] public ushort field_0x00;
    [FieldOffset(0x02)] public short ReplacementAge;
    [FieldOffset(0x04)] public short CurrentPitch;
    [FieldOffset(0x06)] public short VoiceStatus;
    [FieldOffset(0x08)] public short field_0x08; // PARTIAL: DAT_sound_801f7938 via g_voiceRuntimeSlots stride 0x34
    [FieldOffset(0x0A)] public byte field_0x0A; // PARTIAL: DAT_sound_801f793A via g_voiceRuntimeSlots stride 0x34
    [FieldOffset(0x0C)] public short Note;
    [FieldOffset(0x0E)] public short SequenceKey;
    [FieldOffset(0x10)] public short VabFirstToneIndex;
    [FieldOffset(0x12)] public short ProgramIndex;
    [FieldOffset(0x14)] public short ToneIndex;
    [FieldOffset(0x16)] public short VabId;
    [FieldOffset(0x18)] public short Priority;
    [FieldOffset(0x1A)] public byte field_0x1A;
    [FieldOffset(0x1B)] public byte NoiseState;
    [FieldOffset(0x1C)] public short field_0x1C;
    [FieldOffset(0x1E)] public short field_0x1E;
    [FieldOffset(0x20)] public short field_0x20;
    [FieldOffset(0x22)] public short field_0x22;
    [FieldOffset(0x24)] public short field_0x24;
    [FieldOffset(0x26)] public short field_0x26;
    [FieldOffset(0x28)] public short field_0x28;
    [FieldOffset(0x2A)] public short field_0x2A;
    [FieldOffset(0x2C)] public short field_0x2C;
    [FieldOffset(0x2E)] public short field_0x2E;
    [FieldOffset(0x30)] public short field_0x30;
    [FieldOffset(0x32)] public short field_0x32;
}

[StructLayout(LayoutKind.Explicit, Size = 0x16)]
public struct SoundEffectRecord
{
    [FieldOffset(0x00)] public short VabId;
    [FieldOffset(0x02)] public short ProgramNumber;
    [FieldOffset(0x04)] public short ToneNumber;
    [FieldOffset(0x06)] public short Note;
    [FieldOffset(0x08)] public short Flags;
    [FieldOffset(0x0A)] public short SeqNum;
    [FieldOffset(0x0C)] public short RefSfxId;
    [FieldOffset(0x0E)] public short field_0x0E;
    [FieldOffset(0x10)] public short MaxVoices;
    [FieldOffset(0x12)] public short field_0x12;
    [FieldOffset(0x14)] public short ToneCount;
}

[StructLayout(LayoutKind.Explicit, Size = 0x08)]
public struct VabProgramAttributesCopy
{
    [FieldOffset(0x00)] public byte Tones;
    [FieldOffset(0x01)] public byte Volume;
    [FieldOffset(0x02)] public byte Priority;
    [FieldOffset(0x03)] public byte Mode;
    [FieldOffset(0x04)] public byte Pan;
    [FieldOffset(0x06)] public ushort Attr;
}

[StructLayout(LayoutKind.Explicit, Size = 0x18)]
public struct VabToneAttributesCopy
{
    [FieldOffset(0x00)] public byte Priority;
    [FieldOffset(0x01)] public byte Mode;
    [FieldOffset(0x02)] public byte Volume;
    [FieldOffset(0x03)] public byte Pan;
    [FieldOffset(0x04)] public byte Center;
    [FieldOffset(0x05)] public byte Shift;
    [FieldOffset(0x06)] public byte Min;
    [FieldOffset(0x07)] public byte Max;
    [FieldOffset(0x08)] public byte VibratoWidth;
    [FieldOffset(0x09)] public byte VibratoTime;
    [FieldOffset(0x0A)] public byte PortamentoWidth;
    [FieldOffset(0x0B)] public byte PortamentoTime;
    [FieldOffset(0x0C)] public byte PitchBendMin;
    [FieldOffset(0x0D)] public byte PitchBendMax;
    [FieldOffset(0x10)] public ushort Adsr1;
    [FieldOffset(0x12)] public ushort Adsr2;
    [FieldOffset(0x14)] public ushort Program;
    [FieldOffset(0x16)] public ushort Vag;
}

// PARTIAL: only the fields proven by FUN_800905F8 / FUN_800906E8 / FUN_800907E4 are modeled.
public struct SpuReverbAttrPartial
{
    public int Mask;
    public int Mode;
    public int Feedback;
    public int Delay;
}
