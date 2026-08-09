namespace AlundraEngine.Balance;

public class BalanceRecord
{
    public readonly byte BalanceLevel;//0
    public readonly byte RecordSize;//1
    public readonly byte MaxHp;//2 - également DamageResponses[-1], slot 0 du tableau de réponses
    public readonly byte[] DamageResponses = new byte[11];//3
    public readonly byte AttackCount;//e
    public readonly BalanceAttack[] Attacks;//f

    public readonly int FileOffset;
    public readonly BalanceRecord NextLevel;

    public BalanceRecord(BinaryReader br, int offset)
    {
        FileOffset = offset;
        br.BaseStream.Position = offset;
        BalanceLevel = br.ReadByte();
        RecordSize = br.ReadByte();
        MaxHp = br.ReadByte();
        br.Read(DamageResponses, 0, 11);
        AttackCount = br.ReadByte();

        if (AttackCount > 0)
        {
            Attacks = new BalanceAttack[AttackCount];

            for (var i = 0; i < AttackCount; i++)
            {
                Attacks[i] = new BalanceAttack(br);
            }
        }

        if (BalanceLevel < 255)
        {
            NextLevel = new BalanceRecord(br, offset + RecordSize);
        }
    }

    // 80038ab4 / 8004464c : l'attaque de l'animation N est stockée dans Attacks[N + 1],
    // l'élément 0 servant de repli quand N + 1 dépasse AttackCount.
    public BalanceAttack? GetAttackForAnimation(int animationId)
    {
        if (AttackCount == 0)
        {
            return null;
        }

        var index = animationId + 1 >= AttackCount ? 0 : animationId + 1;
        return Attacks[index];
    }
}
