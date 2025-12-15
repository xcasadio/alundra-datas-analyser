namespace AlundraEngine.Gameplay;

public class PlayerStats {
    public short Hp;
    public short HpMax;
    public short Mp;
    public short MpMax;
    public short MoneyAmount;
    public short WeaponId;
    public short ItemId;
    public short FalconTemp;
    public short Falcon;

    public void CopyFrom(PlayerStats source)
    {
        Hp = source.Hp;
        HpMax = source.HpMax;
        Mp = source.Mp;
        MpMax = source.MpMax;
        MoneyAmount = source.MoneyAmount;
        WeaponId = source.WeaponId;
        ItemId = source.ItemId;
        FalconTemp = source.FalconTemp;
        Falcon = source.Falcon;
    }
};