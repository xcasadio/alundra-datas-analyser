namespace AlundraEngine.Balance;

public class BalanceRecordData
{
    public byte Level;
    public byte OffsetToNextLevel;
    public byte Hp;
    public byte[] Values = new byte[11];
    public byte NumAnimVals;

    public void CopyFrom(BalanceRecord balanceRecord)
    {
        Level = balanceRecord.Level;
        OffsetToNextLevel = balanceRecord.OffsetToNextLevel;
        Hp = balanceRecord.Hp;
        Values = new byte[balanceRecord.Values.Length];
        Array.Copy(balanceRecord.Values, Values, Values.Length);
        NumAnimVals = balanceRecord.NumAnimVals;
    }
}