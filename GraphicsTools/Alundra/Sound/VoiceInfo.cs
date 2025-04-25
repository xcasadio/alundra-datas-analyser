namespace Alundra.Sound;

public class VoiceInfo
{
    public int[] VoiceSfxIds = new int[24];//0x00  the sfxids assigned to each voice
    public int[] VoiceVabIds = new int[24];//0x60   the vabids assigned to each voice
    public int[] VoiceToneNums = new int[24];//0xc0
    public int[] VoiceVolumes = new int[24];//0x120
    public int[] VoicePans = new int[24];//0x180
}