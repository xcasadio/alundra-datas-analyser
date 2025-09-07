using System.Diagnostics;

namespace AlundraEngine;

public class EtcResR : EtcRes
{
    private readonly string _fileName;
    private readonly Dictionary<int, string> _stringByIndex = new();

    public EtcResR(string fileName)
    {
        _fileName = fileName;

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
            StringTable[i] = ReadString(buffer, ref offset);
            _stringByIndex.Add(offset, StringTable[i]);
        }


        for (int i = 0; i < 0x100; i++)
        {
            int offset = IndexTable[i];
            DescriptionStrings[i] = ReadString(buffer, ref offset);
            _stringByIndex.Add(offset, DescriptionStrings[i]);
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

            _stringByIndex.TryAdd(offset, str);
        }

        for (int i = 0; i < 0x62; i++)
        {
            int iconNameOffset = IndexTable[i + 0x200];
            int descriptionOffset = IndexTable[i + 0x280];
            int otherStringOffset = IndexTable[i + 0x300];

            var offset = iconNameOffset;
            IconNames[i * 2] = ReadString(buffer, ref offset);
            //_gameEngine.StaticVariables.g_iconNameEtcBase[i * 2] = (byte)i;
            offset = descriptionOffset;
            DescriptionItems[i * 2] = ReadString(buffer, ref offset);
            offset = otherStringOffset;
            OtherStrings[i * 2] = ReadString(buffer, ref offset);

            _stringByIndex.TryAdd(iconNameOffset, IconNames[i * 2]);
            _stringByIndex.TryAdd(descriptionOffset, DescriptionItems[i * 2]);
            _stringByIndex.TryAdd(otherStringOffset, OtherStrings[i * 2]);
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
        return _stringByIndex[IndexTable[id]];

        //Debugger.Break();

        /*
        0 < id < 0x100 (256) => DescriptionStrings
        0x100 (256) < id < 0x200 (512) => StringTable
        0x400 (1024) < id < => Strings
         */

        //var buffer = File.ReadAllBytes(_fileName);
        //int offset = _indexTable[id];
        //var value = ReadString(buffer, ref offset);

        //return Strings[id];
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