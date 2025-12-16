using AlundraTools.GameControls.CommandControls.Commands;

namespace AlundraEngine.DatasBin.Commands;

public class GotoCommand : ContainerCommand
{
    private int _lastCommandMemoryAddress;
    public override int LastCommandMemoryAddress => _lastCommandMemoryAddress;

    public GotoCommand(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, parameters, name, memoryAddress)
    {
        RefOffset = (short)(parameters[0] | (parameters[1] << 8));
    }

    public override int Build(int i, List<SiCommand> commands)
    {
        var index = GetCommandNameByOffset(RefOffset, commands);
        Name += index == -1 ? " ?" : $" #{index:D2}";
        
        _lastCommandMemoryAddress = MemoryAddress + RefOffset;

        return base.Build(i, commands);
    }
}