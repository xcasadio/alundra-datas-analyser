namespace AlundraEngine;

public static class Random
{
    public static ulong RandomSeed = 0xB017C93D;

    public static void Reset()
    {
        RandomSeed = 0xB017C93D;
    }

    public static ulong Next()
    {
        RandomSeed = RandomSeed * 0x7d2b89dd + 0xe06a02e7;
        return (uint)RandomSeed;
    }
}