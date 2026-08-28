using AlundraEngine.Text;

namespace AlundraEngine.Etc;

public abstract class EtcRes
{
    public readonly string[] DescriptionItems = new string[196];
    public readonly string[] IconNames = new string[196];
    public readonly string[] OtherStrings = new string[196];

    public readonly string[] StringTable = new string[256];
    public readonly string[] Strings = new string[512];
    public readonly string[] DescriptionStrings = new string[256];

    public readonly Dictionary<int, string> StringByIndex = new();
    public short[] IndexTable;

    public readonly string FileName;

    protected EtcRes(string fileName)
    {
        FileName = fileName;
    }

    public abstract string GetItemName(int id);
    public abstract string GetEtcString(int id);
    public abstract string GetOtherString(int id);
    public abstract string GetItemDescription(int itemId);

    // The inventory description has two lines: IndexTable[itemId + 0x280] and IndexTable[itemId + 0x300].
    // The original keeps them in two pointer tables (0x800C628C and 0x800C6290, stride 8).
    public abstract string GetItemDescriptionSecondLine(int itemId);

    protected string ReadString(byte[] buffer, ref int l)
    {
        string str = null;
        var c = (char)buffer[l];

        while (c != '\0')
        {
            str += (char)buffer[l];
            l++;
            c = (char)buffer[l];
        }

        return str != null ? TextDecoder.DecodeString(str) : null;
    }
}