namespace GraphicsTools.Alundra;

public class EtcResR
{
    private readonly string _fileName;

    public readonly byte[] TileTable = new byte[128];
    public readonly byte[] IconNameTable = new byte[128];
    public readonly byte[] PaletteTable = new byte[128];
    public readonly byte[] StringTable = new byte[256];
    public readonly string[] Strings = new string[1024];

    public EtcResR(string fileName)
    {
        _fileName = fileName;
        var buffer = File.ReadAllBytes(fileName);

        var stringTable = new List<byte>(256);
        var ressources = new byte[1024];

        for (int i = 0; i < 1024; i++)
        {
            ressources[i] = buffer[i];
        }

        for (int i = 0; i < 0x62; i++)
        {
            TileTable[i] = buffer[(i + 0x200) * 2];
            IconNameTable[i] = buffer[(i + 0x280) * 2];
            PaletteTable[i] = buffer[(i + 0x300) * 2];
        }

        for (int i = 0; i < 0x100; i++)
        {
            StringTable[i] = buffer[(i + 0x100) * 2];
        }

        //Test
        int x = 0;
        int l = 2049;
        while (l < buffer.Length)
        {
            var str = "";
            var c = (char)buffer[l];

            while (c != '\0')
            {
                str += (char)buffer[l];
                l++;
                c = (char)buffer[l];
            }

            Strings[x++] = TextInterpreter.DecodeString(str);
            l++;
        }
    }
}