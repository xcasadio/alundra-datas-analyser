using System.Diagnostics;
using AlundraEngine.DatasBin;

namespace AlundraTools.GameControls.CommandControls.Commands;

public abstract class ContainerCommand : CommandBase
{
    public CommandBase[] Children { get; set; } = [];
    public abstract int LastCommandOffset { get; }

    public ContainerCommand(byte command, byte[] parameters, string name, int offset)
        : base(command, parameters, name, offset)
    {
    }

    public override int Build(int i, List<SiCommand> commands)
    {
        int j = i + 1;
        var children = new List<CommandBase>();

        for (; j < commands.Count; j++)
        {
            if (commands[j].Offset > LastCommandOffset)
            {
                //Debugger.Break();
            }

            if (commands[j].Offset >= LastCommandOffset)
            {
                break;
            }

            var commandBase = CommandsBuilder.Convert(commands[j]);
            j = commandBase.Build(j, commands);
            children.Add(commandBase);
        }

        Children = children.ToArray();

        return j - 1;
    }
}