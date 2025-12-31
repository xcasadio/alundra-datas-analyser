namespace AlundraEngine;

public class EtcResR : EtcRes
{
    public EtcResR(string fileName) : base(fileName)
    {
        using var br = new BinaryReader(File.OpenRead(fileName));
        IndexTable = new short[1024];

        for (int i = 0; i < 1024; i++)
        {
            IndexTable[i] = br.ReadInt16();
        }

        var buffer = File.ReadAllBytes(fileName);

        for (int i = 0; i < 0x100; i++)
        {
            int offset = IndexTable[i + 0x100];
            var j = offset;
            StringTable[i] = ReadString(buffer, ref j);
            StringByIndex.Add(offset, StringTable[i]);
        }

        for (int i = 0; i < 0x100; i++)
        {
            int offset = IndexTable[i];
            var j = offset;
            DescriptionStrings[i] = ReadString(buffer, ref j);
            StringByIndex.Add(offset, DescriptionStrings[i]);
        }

        int x = 0;
        int l = 0x400 * 2;
        while (l < buffer.Length)
        {
            var offset = l;
            var str = ReadString(buffer, ref l);
            if (!string.IsNullOrEmpty(str))
            {
                Strings[x++] = str;
            }
            l++;

            StringByIndex.TryAdd(offset, str);
        }

        for (int i = 0; i < 0x62; i++)
        {
            int iconNameOffset = IndexTable[i + 0x200];
            var offset = iconNameOffset;
            IconNames[i * 2] = ReadString(buffer, ref offset);
            StringByIndex.TryAdd(iconNameOffset, IconNames[i * 2]);

            int descriptionOffset = IndexTable[i + 0x280];
            offset = descriptionOffset;
            DescriptionItems[i * 2] = ReadString(buffer, ref offset);
            StringByIndex.TryAdd(descriptionOffset, DescriptionItems[i * 2]);

            int otherStringOffset = IndexTable[i + 0x300];
            offset = otherStringOffset;
            OtherStrings[i * 2] = ReadString(buffer, ref offset);
            StringByIndex.TryAdd(otherStringOffset, OtherStrings[i * 2]);
        }

        //l = _indexTable[0x3ff];
        //var gameTitle = ReadString(buffer, ref l); // "BESLES-01135ALUNDRA " => BESLES-01198ALUNDRA
    }
    
    public override string GetItemName(int id)
    {
        return IconNames[id * 2];
    }

    public override string GetEtcString(int id)
    {
        return StringByIndex[IndexTable[id]];
    }

    public override string GetOtherString(int id)
    {
        return OtherStrings[id];
    }

    public override string GetItemDescription(int itemId)
    {
        return DescriptionItems[itemId * 2];
    }
}