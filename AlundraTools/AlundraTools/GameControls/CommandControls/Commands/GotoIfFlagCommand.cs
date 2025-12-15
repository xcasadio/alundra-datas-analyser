using AlundraEngine.DatasBin;

namespace AlundraTools.GameControls.CommandControls.Commands;

public class GotoIfFlagCommand : ContainerCommand
{
    private int _lastCommandMemoryAddress;

    public override int LastCommandMemoryAddress => _lastCommandMemoryAddress;

    public GotoIfFlagCommand(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, parameters, name, memoryAddress)
    {
        RefOffset = (short)(parameters[0] | (parameters[1] << 8));
    }

    protected override string PrintParameters()
    {
        return string.Join(", ", Parameters.Select(x => x.ToString("x2")));
    }

    public override int Build(int i, List<SiCommand> commands)
    {
        var name = "If ";
        var flag = (uint)(Parameters[0] + Parameters[1] * 0x100);
        name += (flag & 0x8000) == 0 ? "MapFlags" : "GlobalFlags";
        name += $"[{((flag >> 3) & 0xffc) >> 2}]";
        name += $" & {1 << (Parameters[0] & 0x1f)} is ";
        name += $"{(Command == 0x30 ? "off" : "on")}"; 
        //TODO remove this
        name += " goto ";
        var offset = ((Parameters[2] + Parameters[3] * 0x100) * 0x10000) >> 0x10;
        var index = GetCommandNameByOffset(offset, commands);
        name += $"{(index == -1 ? "?" : $"{index}")}";
        name += $" ({(index == -1 ? "out of bounds" : commands[index].Name)})";
        name += " else goto ";
        index = GetCommandNameByOffset(Size, commands);
        name += $"{(index == -1 ? "?" : $"{index}")}";
        name += $" ({(index == -1 ? "out of bounds" : commands[index].Name)})";
        Name = name;

        _lastCommandMemoryAddress = MemoryAddress + Math.Max(offset, Size);

        return base.Build(i, commands);
    }
}