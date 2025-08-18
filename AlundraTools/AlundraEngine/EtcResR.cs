using System.Diagnostics;
using System.IO.Compression;
using AlundraEngine.Text;

namespace AlundraEngine;

public class EtcResR
{
    private readonly string _fileName;

    private readonly short[] _indexTable;
    public readonly string[] DescriptionItems = new string[196];
    public readonly string[] IconNames = new string[196];
    public readonly string[] OtherStrings = new string[196];

    public readonly string[] StringTable = new string[256];
    public readonly string[] Strings = new string[512];
    public readonly string[] DescriptionStrings = new string[256];

    public EtcResR(string fileName)
    {
        _fileName = fileName;

        using var br = new BinaryReader(File.OpenRead(fileName));
        _indexTable = new short[1024];

        for (int i = 0; i < 1024; i++)
        {
            _indexTable[i] = br.ReadInt16();
        }

        var buffer = File.ReadAllBytes(fileName);

        for (int i = 0; i < 0x100; i++)
        {
            int offset = _indexTable[i + 0x100];
            StringTable[i] = ReadString(buffer, ref offset);
        }


        for (int i = 0; i < 0x100; i++)
        {
            int offset = _indexTable[i];
            DescriptionStrings[i] = ReadString(buffer, ref offset);
        }

        int x = 0;
        int l = 0x400 * 2;
        while (l < buffer.Length)
        {
            var str = ReadString(buffer, ref l);
            if (!string.IsNullOrEmpty(str))
            {
                Strings[x++] = str;
            }
            l++;
        }

        for (int i = 0; i < 0x62; i++)
        {
            int iconNameOffset = _indexTable[i + 0x200];
            int descriptionOffset = _indexTable[i + 0x280];
            int OtherStringOffset = _indexTable[i + 0x300];

            var offset = iconNameOffset;
            IconNames[i * 2] = ReadString(buffer, ref offset);
            //_gameEngine.StaticVariables.g_iconNameEtcBase[i * 2] = (byte)i;
            offset = descriptionOffset;
            DescriptionItems[i * 2] = ReadString(buffer, ref offset);
            offset = OtherStringOffset;
            OtherStrings[i * 2] = ReadString(buffer, ref offset);
        }

        l = _indexTable[0x3ff];
        var gameTitle = ReadString(buffer, ref l); // "BESLES-01135ALUNDRA " => BESLES-01198ALUNDRA
    }

    private static string ReadString(byte[] buffer, ref int l)
    {
        var str = "";
        var c = (char)buffer[l];

        while (c != '\0')
        {
            str += (char)buffer[l];
            l++;
            c = (char)buffer[l];
        }

        return TextDecoder.DecodeString(str);
    }

    public string GetItemName(int id)
    {
        return IconNames[id * 2];
    }

    public string GetEtcString(int id)
    {
        Debugger.Break();

        /*
        0 < id < 0x100 (256) => DescriptionStrings
        0x100 (256) < id < 0x200 (512) => StringTable
        0x400 (1024) < id < => Strings
         */

        var buffer = File.ReadAllBytes(_fileName);

        int offset = _indexTable[id];
        //var value = ReadString(buffer, ref offset);

        return Strings[id];
    }

    public string GetOtherString(int id)
    {
        return OtherStrings[id];
    }

    public string GetItemDescription(int itemId)
    {
        return DescriptionItems[itemId * 2];
    }
}