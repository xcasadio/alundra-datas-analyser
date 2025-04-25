namespace Alundra.Text;

public class TextInterpreter
{
    public static string DecodeString(string message)
    {
        Dictionary<string, string> tokens = new ()
        {
            { "}7", "Ç" },
            { "{k", "ù" },
            { "{i", "ù" },
            { "{B", "'" },
            { "}P", "à" },
            { "}R", "â" },
            { "}X", "è" },
            { "}W", "ç" },
            { "}Y", "é" },
            { "}Z", "ê" },
            { "}^", "î" },
            { "}¨", "ï" },
            //{ "\\N", Environment.NewLine },
        };

        foreach (var token in tokens)
        {
            message = message.Replace(token.Key, token.Value);
        }

        return message;
    }
}