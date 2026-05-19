namespace AlundraEngine.Gameplay;

public class Voice {
    byte volLeft;
    byte _1;
    byte _2;
    byte _3;
    byte pitch;
    byte _5;
    byte reverbDepth;
    byte _7;
    byte adsrAttack;
    byte _9;
    byte adsrSustain;
    byte _b;
    byte status;
    byte[] _d = new byte[3];
    Voice[] nextVoice;
    byte[] _14 = new byte[359];
    short _17b;
    short _17d;
    short _17f;
    short _181;
    byte[] _183 = new byte[5];
    short _188;
    short _18a;
    byte _18c;
    byte _18d;
    short _18e;
    int _190;
    ushort _194;
    ushort _196;
    short _198;
    short _19a;
    byte[] _19c = new byte[14];
    ushort _1aa;

    public ushort field_0x194
    {
        get => _194;
        set => _194 = value;
    }

    public ushort field_0x196
    {
        get => _196;
        set => _196 = value;
    }

    public short field_0x188
    {
        get => _188;
        set => _188 = value;
    }

    public short field_0x18A
    {
        get => _18a;
        set => _18a = value;
    }

    public byte field_0x18C
    {
        get => _18c;
        set => _18c = value;
    }

    public byte field_0x18D
    {
        get => _18d;
        set => _18d = value;
    }

    public short field_0x18E
    {
        get => _18e;
        set => _18e = value;
    }

    public short field_0x198
    {
        get => _198;
        set => _198 = value;
    }

    public short field_0x19A
    {
        get => _19a;
        set => _19a = value;
    }

    public ushort field_0x1AA
    {
        get => _1aa;
        set => _1aa = value;
    }
};