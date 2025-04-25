namespace Alundra.Sound;

public class BalanceRecord
{
    public readonly byte Level;//0
    public readonly byte OffsetToNextLevel;//1
    public readonly byte Hp;//2 
    public readonly byte[] Vals = new byte[11];//supposed to be at 2
    //but i think ill put it at 3 and subtract q from the indexvals
    //3
    //4
    //5
    //6
    //7
    //8
    //9
    //a
    //b
    //c
    //d
    public readonly byte NumAnimVals;//e
    public readonly BalanceAnimValRef[] AnimVals;//targetanim+1 //f

    public int Offset;
    public readonly BalanceRecord Next;

    public BalanceRecord(BinaryReader br, int offset)
    {
        Offset = offset;
        br.BaseStream.Position = offset;
        Level = br.ReadByte();
        OffsetToNextLevel = br.ReadByte();
        Hp = br.ReadByte();
        br.Read(Vals, 0, 11);
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