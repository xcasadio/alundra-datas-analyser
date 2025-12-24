using AlundraTools.GameControls.CommandControls.Commands;

namespace AlundraEngine.DatasBin.Commands;

public class GotoCommand : ContainerCommand
{
    private int _lastCommandOffset;
    public override int LastCommandOffset => _lastCommandOffset;

    public GotoCommand(byte command, byte[] parameters, string name, int offset)
        : base(command, parameters, name, offset)
    {
        OffsetShift = (short)(parameters[0] | (parameters[1] << 8));
    }

    public override int Build(int i, List<SiCommand> commands)
    {
        var index = GetCommandNameByOffset(OffsetShift, commands);
        Name += index == -1 ? " ?" : $" {index}"; //index:D2

        _lastCommandOffset = Offset + OffsetShift;

        return base.Build(i, commands);
    }
}