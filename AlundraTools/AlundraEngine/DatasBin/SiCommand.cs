namespace AlundraEngine.DatasBin;

public class SiCommand
{
    public bool HasParameters => Parameters is { Length: > 0 } && Command != 0 && Command != 0xff;

    public SiCommand(byte command, int size, byte[] parameters, string name, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        Command = command;
        Parameters = parameters;
        Size = size;
        Name = name;
    }

    public readonly int MemoryAddress;
    public readonly byte Command;
    public readonly byte[] Parameters;
    public readonly int Size;
    public int RefOffset;

    public readonly string Name;

    public string Print(int depth, List<SiCommand> commands)
    {
        var index = commands.IndexOf(this);
        var output = index.ToString("d3") + " ";
        output += new string(' ', depth * 4);
        output += PrintName();
        output += $"({PrintCode()})";
        if (Command != 0 && Command != 0xff)
        {
            output += $" ({PrintParameters(commands)})";
        }
        return output;
    }

    public string PrintEvent(int depth, List<SiCommand> commands)
    {
        var output = new string(' ', depth * 4);
        output += PrintName();
        output += $"[{PrintCode()}]";
        if (Command != 0 && Command != 0xff)
        {
            output += $" ({PrintParameters(commands)})";
        }
        return output;
    }

    public string PrintName()
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
}