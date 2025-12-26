namespace AlundraEngine.Balance;

public class BalanceRecord
{
    public readonly byte Level;//0
    public readonly byte OffsetToNextLevel;//1
    public byte Hp;//2 
    public readonly byte[] Values = new byte[11];
    public readonly byte NumAnimVals;
    public readonly BalanceAnimValRef[] AnimVals;

    public readonly int Offset;
    public readonly BalanceRecord Next;

    public BalanceRecord(BinaryReader br, int offset)
    {
        Offset = offset;
        br.BaseStream.Position = offset;
        Level = br.ReadByte();
        OffsetToNextLevel = br.ReadByte();
        Hp = br.ReadByte();
        br.Read(Values, 0, 11);
        NumAnimVals = br.ReadByte();

        if (NumAnimVals > 0)
        {
            AnimVals = new BalanceAnimValRef[NumAnimVals];

            for (var i = 0; i < NumAnimVals; i++)
            {
                AnimVals[i] = new BalanceAnimValRef(br);
            }
        }

        if (Level < 255)
        {
            Next = new BalanceRecord(br, offset + OffsetToNextLevel);
        }
    }
}