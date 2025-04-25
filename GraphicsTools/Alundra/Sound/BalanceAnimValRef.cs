namespace Alundra.Sound;

public class BalanceAnimValRef
{
    public readonly byte Val;
    public byte U2;

    public BalanceAnimValRef(BinaryReader br)
    {
        Val = br.ReadByte();
        U2 = br.ReadByte();
    }
}