namespace AlundraEngine.Balance;

// Copie mutable des stats de balance effectives du joueur (enregistrement de base + arme + bouclier),
// recalculée à chaque frame par UpdateItemEffectState. À ne pas confondre avec BalanceRecord,
// qui est la donnée immuable lue dans BALANCE.BIN.
// GHIDRA: g_balanceRecord @ 0x80127008
public class EffectiveBalanceStats
{
    public byte BalanceLevel;
    public byte RecordSize;
    public byte MaxHp;
    public byte[] DamageResponses = new byte[11];
    public byte AttackCount;

    public void CopyFrom(BalanceRecord balanceRecord)
    {
        BalanceLevel = balanceRecord.BalanceLevel;
        RecordSize = balanceRecord.RecordSize;
        MaxHp = balanceRecord.MaxHp;
        DamageResponses = new byte[balanceRecord.DamageResponses.Length];
        Array.Copy(balanceRecord.DamageResponses, DamageResponses, DamageResponses.Length);
        AttackCount = balanceRecord.AttackCount;
    }
}
