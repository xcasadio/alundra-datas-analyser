using AlundraEngine.DatasBin;
using AlundraEngine.DatasBin.Commands;

namespace AlundraTools.GameControls.CommandControls.Commands;

public static class CommandsBuilder
{
    public static CommandBase[] Convert(List<SiCommand> commands)
    {
        var commandBases = new List<CommandBase>();

        for (var i = 0; i < commands.Count; i++)
        {
            CommandBase cmd = Convert(commands[i]);
            i = cmd.Build(i, commands);
            commandBases.Add(cmd);
        }

        return commandBases.ToArray();
    }

    public static CommandBase Convert(SiCommand command)
    {
        var code = command.Command;
        var parameters = command.Parameters;
        var name = command.Name;
        var memoryAddress = command.MemoryAddress;

        return code switch
        {
            0x02 => new GotoCommand(code, parameters, name, memoryAddress),
            0x03 or 0x04 => new BranchCommand(code, parameters, name, memoryAddress),
            0x05 or 0x06 => new SetFlagCommand(code, parameters, name, memoryAddress),
            0x0D => new DialogCommand(code, parameters, name, memoryAddress),
            0x30 or 0x31 => new GotoIfFlagCommand(code, parameters, name, memoryAddress),
            0x35 or 0x36 => new WaitFlagCommand(code, parameters, name, memoryAddress),
            0x37 => new WaitCommand(code, parameters, name, memoryAddress),
            0x1A => new CommandBaseDecimalParameters(code, parameters, name, memoryAddress),
            0x1C or 0x1D => new RepeatAnimationCommand(code, parameters, name, memoryAddress),
            0x1E or 0x1F => new WalkCommand(code, parameters, name, memoryAddress),
            0x58 => new DirectionBranchCommand(code, parameters, name, memoryAddress),
            0x5C => new DialogCommandWithChoice(code, parameters, name, memoryAddress),
            0x64 => new SetPositionCommand(code, parameters, name, memoryAddress),
            0x78 => new GotoCommand(code, parameters, name, memoryAddress),
            0xC4 => new DialogWithEntityAndNameCommand(code, parameters, name, memoryAddress),

            _ => new CommandBase(code, parameters, name, memoryAddress),
        };
    }
}