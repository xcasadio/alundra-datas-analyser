namespace AlundraEngine.DatasBin;

public class SiCommand
{
    public bool HasParameters => Parameters is { Length: > 0 } && Command != 0 && Command != 0xff;

    public SiCommand(byte command, byte[] parameters, string name, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        Command = command;
        Parameters = parameters;
        Size = SpriteInfoEventCodes.CommandSizeByCode.GetValueOrDefault(command, 1);
        Name = name;
    }

    public readonly string Name;
    public readonly int MemoryAddress;
    public readonly byte Command;
    public readonly byte[] Parameters;
    public readonly int Size;
    public int RefOffset;

    public string Print(int depth, List<SiCommand> commands)
    {
        var index = commands.IndexOf(this);
        var output = index.ToString("d3") + " ";
        output += new string(' ', depth * 4);
        output += PrintName(commands);
        output += $"({PrintCode()})";
        if (HasParameters)
        {
            output += $" ({PrintParameters(commands)})";
        }
        return output;
    }

    public string PrintEvent(int depth, List<SiCommand> commands)
    {
        var output = new string(' ', depth * 4);
        output += PrintName(commands);
        if (HasParameters)
        {
            output += $" ({PrintParameters(commands)})";
        }
        return output;
    }

    public virtual string PrintName(List<SiCommand> commands)
    {
        return !string.IsNullOrEmpty(Name) ? Name : "<no name>";
    }

    public string PrintCode()
    {
        return $"{Command} (0x{Command:x2})";
    }

    public virtual string PrintParameters(List<SiCommand> commands)
    {
        return string.Join(", ", Parameters.Select(x => x.ToString("x2")));
    }

    public virtual string Description(List<SiCommand> commands)
    {
        if (SpriteInfoEventCodes.CommandPropertiesByCode.TryGetValue(Command, out var commandProperties))
        {
            return commandProperties.Description;
        }

        return string.Empty;
    }

    public override string ToString()
    {
        var parameters = "-";

        if (HasParameters)
        {
            parameters = string.Join(", ", Parameters.Select(x => x.ToString()));
        }

        return $"{Name} (0x{Command}) s:{Size} p:{parameters} [{MemoryAddress}]";
    }
}