namespace AlundraEngine.Text;

public class TextDecoder
{
    private static readonly Dictionary<string, string> Tokens = new()
    {
        { "}7", "Ç" },
        { "{k", "ù" },
        { "{i", "ù" },
        { "{B", "'" },
        { "}d", "ô" },
        { "}P", "à" },
        { "}R", "â" },
        { "}X", "è" }, //8 14
        { "}W", "ç" }, //7 14
        { "}Y", "é" }, //9 14
        { "}Z", "ê" }, //11 14
        { "}^", "î" },
        { "}¨", "ï" },
        //{ "\\N", Environment.NewLine },
    };

    private static readonly Dictionary<char, char> TokensWithoutSpecialCharacter = new()
    {
        { '7', 'Ç' },
        { 'k', 'ù' },
        { 'i', 'ù' },
        { 'B', '\'' },
        { 'd', 'ô' },
        { 'P', 'à' },
        { 'R', 'â' },
        { 'X', 'è' }, 
        { 'W', 'ç' }, 
        { 'Y', 'é' }, 
        { 'Z', 'ê' }, 
        { '^', 'î' },
        { '¨', 'ï' }
    };

    public static string DecodeString(string message)
    {
        foreach (var token in Tokens)
        {
            message = message.Replace(token.Key, token.Value);
        }

        return message;
    }

    // Convertion CP850 -> Latin-1 (ISO-8859-1) 
    static readonly Dictionary<byte, int> Cp850ToLatin1 = new()
    {
        {128, 199}, // Ç
        {129, 252}, // ü
        {130, 233}, // é
        {131, 226}, // â
        {132, 228}, // ä
        {133, 224}, // à
        {134, 229}, // å
        {135, 231}, // ç
        {136, 234}, // ê
        {137, 235}, // ë
        {138, 232}, // è
        {139, 239}, // ï
        {140, 238}, // î
        {141, 236}, // ì
        {142, 196}, // Ä
        {143, 197}, // Å
        {144, 201}, // É
        {145, 230}, // æ
        {146, 198}, // Æ
        {147, 244}, // ô
        {148, 246}, // ö
        {149, 242}, // ò
        {150, 251}, // û
        {151, 249}, // ù
        {152, 255}, // ÿ
        {153, 214}, // Ö
        {154, 220}, // Ü

        {160, 225}, // á
        {161, 237}, // í
        {162, 243}, // ó
        {163, 250}, // ú
        {164, 241}, // ñ
        {165, 209}, // Ñ

        {181, 193}, // Á
        {182, 194}, // Â
        {183, 192}, // À

        {155, 162}, // ¢
        {156, 163}, // £
        {157, 165}, // ¥
        {166, 170}, // ª
        {167, 186}, // º
        {168, 191}, // ¿
        {170, 172}, // ¬
        {171, 189}, // ½
        {172, 188}, // ¼
        {173, 161}, // ¡
        {174, 171}, // «
        {175, 187}, // »
        {184, 169}, // ©
    };

    public static char DecodeCharacter(char c)
    {
        var newValue = c;
        TokensWithoutSpecialCharacter.TryGetValue(c, out newValue);

        return newValue;
    }

    public static int ConvertCp850ToLatin1(char cp850)
    {
        int latin1 = cp850;

        if (cp850 >= 128 && Cp850ToLatin1.TryGetValue((byte)cp850, out int mapped))
            latin1 = mapped;

        return latin1;
    }
}