using System.Text;

namespace AlundraEngine;

public static class EntityNames
{
    public enum Language : int
    {
        Japanese = 0,
        French = 1,
        English = 2
    }

    public static string[] SpriteNames { get; private set; }

    public static void Load(Language language)
    {
        var lines = new List<string>();
        using (var reader = new StreamReader("EntityNames.csv", Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line.Split(";")[(int)language]); //0:jp, 1:fr, 2:en
            }
        }

        SpriteNames = lines.Skip(1).ToArray();
    }

    public static string? GetNameWithIndex(byte spriteDirection, uint spriteTableIndex)
    {
        //if ((spriteDirection & 0x80) != 0)
        {
            spriteTableIndex += 0x100;
        }
    
        return spriteTableIndex < 512 ? $"{spriteTableIndex}_{SpriteNames[spriteTableIndex]}" : null;
    }


    public static string? GetName(byte spriteDirection, uint spriteTableIndex)
    {
        if ((spriteDirection & 0x80) != 0)
        {
            spriteTableIndex += 0x100;
        }

        return spriteTableIndex < 512 ? SpriteNames[spriteTableIndex] : null;
    }


    public static string? GetName(uint spriteTableIndex)
    {
        if (SpriteNames == null)
        {
            return null;
        }

        return spriteTableIndex < 512 ? SpriteNames[spriteTableIndex] : null;
    }
}