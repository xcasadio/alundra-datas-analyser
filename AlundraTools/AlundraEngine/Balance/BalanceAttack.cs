namespace AlundraEngine.Balance;

// Attaque infligée par une entité pendant une animation donnée (2 octets dans BALANCE.BIN).
public class BalanceAttack(BinaryReader br)
{
    public readonly byte AttackAttribute = br.ReadByte();
    public readonly byte Power = br.ReadByte();

    // Bits 0-3 : index dans g_weaponNames (0 = No Effect, 1 = Sword ... 11 = Air Magic)
    public int AttackType => AttackAttribute & 0x0f;

    // Bit 7 : ajoute la puissance des objets équipés au calcul de dégâts (8004464c)
    public bool UsesEquipmentPower => (AttackAttribute & 0x80) != 0;

    public override string ToString()
    {
        return $"{AttackAttribute} {Power}";
    }
}
