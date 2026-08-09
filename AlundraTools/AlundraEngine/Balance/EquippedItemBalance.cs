namespace AlundraEngine.Balance;

// Un des 3 emplacements d'équipement dont le jeu suit l'enregistrement de balance.
// GHIDRA: g_itemBalanceRecords @ 0x80146E00
public class EquippedItemBalance
{
    public BalanceRecord BalanceRecord;
    public int ItemId;
}
