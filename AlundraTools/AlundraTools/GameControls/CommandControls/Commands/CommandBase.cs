using AlundraEngine.DatasBin;
using System.Globalization;

namespace AlundraTools.GameControls.CommandControls.Commands;

public class CommandBase
{
    public readonly int Offset;
    public readonly byte Command;
    public readonly byte[] Parameters;
    public readonly int Size;
    public int OffsetShift;

    public string Name { get; protected set; }
    public bool HasParameters => Parameters is { Length: > 0 } && Command != 0 && Command != 0xff;

    public CommandBase(byte command, byte[] parameters, string name, int offset)
    {
        Offset = offset;
        Command = command;
        Parameters = parameters;
        Size = SpriteInfoEventCodes.CommandSizeByCode.GetValueOrDefault(command, 1);
        Name = name;
    }

    public virtual int Build(int i, List<SiCommand> commands)
    {
        return i;
    }

    private string PrintCode()
    {
        return $"{Command} (0x{Command:x2})";
    }

    public string PrintName()
    {
        var output = !string.IsNullOrEmpty(Name) ? Name : "<no name>";
        
        if (HasParameters)
        {
            output += $" ({PrintParameters()})";
        }

        output += $" (addr:{Offset} size:{Size})";

        return output;
    }

    protected virtual string PrintParameters()
    {
        return string.Join(", ", Parameters.Select(x => x.ToString("x2")));
    }

    public string Description()
    {
        var description = string.Empty;

        if (SpriteInfoEventCodes.CommandPropertiesByCode.TryGetValue(Command, out var commandProperties))
        {
            description = commandProperties.Description;
        }

        if (HasParameters)
        {
            return $"Code={PrintCode()} Parameters=({PrintParameters()}) {description}";
        }

        return $"Code={PrintCode()} {description}";
    }

    protected int GetCommandNameByOffset(int offset, List<SiCommand> commands)
    {
        var jumpAmount = offset;
        var jumpAddress = Offset + jumpAmount;
        int i;
        for (i = 0; i < commands.Count; i++)
        {
            if (commands[i].Offset == jumpAddress)
            {
                break;
            }
        }

        return jumpAddress;
        //return i < commands.Count ? i : -1;
    }

    private static string FormatWithSpaces(int value)
    {
        // group thousands and replace commas with spaces
        return value.ToString("N0", CultureInfo.InvariantCulture).Replace(',', ' ');
    }
}