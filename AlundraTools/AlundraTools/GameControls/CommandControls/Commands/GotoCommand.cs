using AlundraTools.GameControls.CommandControls.Commands;

namespace AlundraEngine.DatasBin.Commands;

public class GotoCommand : CommandBase
{
    public string GotoCommandName { get; private set; }

    public GotoCommand(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, parameters, name, memoryAddress)
    {
        RefOffset = (short)(parameters[0] | (parameters[1] << 8));
    }

    //public override string PrintParameters()
    //{
    //    return GotoCommandName;
    //}

    public override int Build(int i, List<SiCommand> commands)
    {
        var index = GetCommandNameByOffset(RefOffset, commands);
        GotoCommandName =  index == -1 ? "?" : $"{index} ({commands[index].Name})";

        return base.Build(i, commands);
    }
}