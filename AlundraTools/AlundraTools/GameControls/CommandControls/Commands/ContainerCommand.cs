using AlundraEngine.DatasBin;

namespace AlundraTools.GameControls.CommandControls.Commands;

public abstract class ContainerCommand : CommandBase
{
    public CommandBase[] Children { get; set; } = [];
    public abstract int LastCommandMemoryAddress { get; }

    public ContainerCommand(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, parameters, name, memoryAddress)
    {
    }

    public override int Build(int i, List<SiCommand> commands)
    {
        int j = i + 1;
        var children = new List<CommandBase>();

        for (; j < commands.Count; j++)
        {
            if (commands[j].MemoryAddress == LastCommandMemoryAddress)
            {
                break;
            }

            children.Add(CommandsBuilder.Convert(commands[j]));
        }

        Children = children.ToArray();

        return j - 1;
    }
}