using AlundraEngine.DatasBin;

namespace AlundraTools.GameControls.CommandControls.Commands;

public class CommandBase
{
    public readonly string Name;
    public readonly int MemoryAddress;
    public readonly byte Command;
    public readonly byte[] Parameters;
    public readonly int Size;
    public int RefOffset;

    public bool HasParameters => Parameters is { Length: > 0 } && Command != 0 && Command != 0xff;

    public CommandBase(byte command, byte[] parameters, string name, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        Command = command;
        Parameters = parameters;
        Size = SpriteInfoEventCodes.CommandSizeByCodes.GetValueOrDefault(command, 1);
        Name = name;
    }

    public string PrintEvent()
    {
        var output = PrintName();
        if (HasParameters)
        {
            output += $" ({PrintParameters()})";
        }
        return output;
    }

    public string PrintCode()
    {
        return $"{Command} (0x{Command:x2})";
    }

    public virtual string PrintName()
    {
        return !string.IsNullOrEmpty(Name) ? Name : "<no name>";
    }

    public virtual string PrintParameters()
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

    public virtual int Build(int i, List<SiCommand> commands)
    {
        return i;
    }

    protected int GetCommandNameByOffset(int offset, List<SiCommand> commands)
    {
        var jumpAmount = offset;
        var jumpAddress = MemoryAddress + jumpAmount;
        int i;
        for (i = 0; i < commands.Count; i++)
        {
            if (commands[i].MemoryAddress == jumpAddress)
            {
                break;
            }
        }

        return i < commands.Count ? i : -1;
    }
}