using System.Text;

namespace AlundraTools.Decompiler.LibModule;

public static class Helper
{
    public static string ReadString(this BinaryReader br, int length)
    {
        if (length == -1)
        {
            length = br.ReadByte();
        }

        var buff = new byte[length];
        br.Read(buff, 0, length);
        var sb = new StringBuilder();
        foreach(var b in buff)
        {
            if (b == 0)
            {
                break;
            }

            sb.Append((char)b);
        }
        return sb.ToString();
    }
    /*public static string ReadString(BinaryReader br)
        {
            int length = br.ReadByte();
            return ReadString(br, length);
        }*/
}