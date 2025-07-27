namespace AlundraEngine;

public class EtcResR
{
    private readonly string _fileName;

    public readonly short[] TileTable = new short[196];
    public readonly int[] IconNameTable = new int[196];
    public readonly short[] PaletteTable = new short[196];
    public readonly short[] StringTable = new short[256];

    public readonly string[] Strings = new string[512];

    public EtcResR(string fileName)
    {
        _fileName = fileName;
        var buffer = File.ReadAllBytes(fileName);

        var ressources = new byte[128]; // TODO : what is it ??

        for (int i = 0; i < 64; i++)
        {
            ressources[i] = buffer[i * 2];
        }

        for (int i = 0; i < 0x100; i++)
        {
            StringTable[i] = buffer[(i + 0x100) * 2];
        }

        int x = 0;
        int l = 0x400 * 2;
        while (l < buffer.Length)
        {
            var str = ReadString(buffer, ref l);
            if (!string.IsNullOrEmpty(str))
            {
                Strings[x++] = str; //TextInterpreter.DecodeString(str);
            }
            l++;
        }

        for (int i = 0; i < 0x62; i++)
        {
            int iconNameOffset = (i + 0x200) * 2;
            int tileSetOffset = (i + 0x280) * 2;
            int paletteOffset = (i + 0x300) * 2;

            //IconNameTable[i * 2] = i; //iconNameOffset / 1024;//buffer[iconNameOffset];
            StaticVariables.g_iconNameEtcBase[i * 2] = (byte)i;
            TileTable[i * 2] = buffer[tileSetOffset * 2];
            PaletteTable[i * 2] = buffer[paletteOffset * 2];
        }

        //l = 0x3ff * 2;
        //var gameTitle = ReadString(buffer, ref l); // "BESLES-01135ALUNDRA "
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

        return str;
    }

    public string GetIconName(int id)
    {
        return Strings[StaticVariables.g_iconNameEtcBase[id * 2]];
    }
    /*
    public int GetValueByOffset(int offset)
    {
        return StaticVariables.g_iconNameEtcBase[offset];
    }
    */
    public string GetEtcString(int id)
    {
        return Strings[id];
    }
}