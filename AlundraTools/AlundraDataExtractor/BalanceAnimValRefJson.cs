using AlundraEngine.Balance;

namespace AlundraDataExtractor;

internal class BalanceAnimValRefJson
{
    public int AnimValIndex { get; set; }
    public int Value { get; set; }

    public BalanceAnimValRefJson(BalanceAnimValRef animVal)
    {
        AnimValIndex = animVal.Val;
        Value = animVal.U2;
    }
}