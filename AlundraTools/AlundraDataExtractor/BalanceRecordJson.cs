using AlundraEngine.Balance;

namespace AlundraDataExtractor;

internal class BalanceRecordJson
{
    public byte Level { get; set; }
    public byte OffsetToNextLevel { get; set; }
    public byte Hp { get; set; }
    public int[] Values { get; set; }
    public byte NumAnimVals { get; set; }
    public BalanceAnimValRefJson[]? AnimVals { get; set; }
    public int Offset { get; set; }
    public int Next { get; set; }

    public BalanceRecordJson(BalanceRecord balanceRecord)
    {
        Level = balanceRecord.Level;
        OffsetToNextLevel = balanceRecord.OffsetToNextLevel;
        Hp = balanceRecord.Hp;
        Values = balanceRecord.Values.Select(b => (int)b).ToArray();
        NumAnimVals = balanceRecord.NumAnimVals;
        AnimVals = balanceRecord.AnimVals?.Select(x => new BalanceAnimValRefJson(x)).ToArray();
        Offset = balanceRecord.Offset;                  
        Next = balanceRecord.Next?.Offset ?? -1;
    }
}