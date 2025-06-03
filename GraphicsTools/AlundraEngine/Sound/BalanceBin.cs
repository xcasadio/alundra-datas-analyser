namespace AlundraEngine.Sound;

public class BalanceBin
{
    private readonly string _balanceFile;
    private readonly List<BalanceRecord> _balanceRecords = new();

    public BalanceBin(string balanceFile)
    {
        _balanceFile = balanceFile;
        using var br = new BinaryReader(File.OpenRead(balanceFile));
        List<int> offsets = new ();
        var firstoffset = 0;

        while (firstoffset == 0 || br.BaseStream.Position < firstoffset)
        {
            int offset = br.ReadInt16();
            if (firstoffset == 0)
            {
                firstoffset = offset;
            }

            offsets.Add(offset);
        }

        foreach (var offset in offsets)
        {
            var record = new BalanceRecord(br, offset);
            _balanceRecords.Add(record);
        }
    }

    public BalanceRecord GetBalanceRecordFromSpriteIndex(int index, int balancelevel)
    {
        var record = _balanceRecords[index];
        if (record.Level >= balancelevel)
        {
            return record;
        }

        do
        {
            record = record.Next;
        } while (record.Level < balancelevel);

        return record;
    }
}