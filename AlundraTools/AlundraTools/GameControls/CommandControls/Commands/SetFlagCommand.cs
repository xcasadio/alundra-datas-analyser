using AlundraEngine.DatasBin;

namespace AlundraTools.GameControls.CommandControls.Commands;

public class SetFlagCommand : CommandBase
{
    public SetFlagCommand(byte command, byte[] parameters, string name, int offset)
        : base(command, parameters, name, offset)
    {
    }

    public override int Build(int i, List<SiCommand> commands)
    {
        var name = $"Flag {(Command == 0x5 ? "on" : "off")} ";
        var flag = (uint)(Parameters[0] + Parameters[1] * 0x100);
        name += (flag & 0x8000) == 0 ? "MapFlags" : "GlobalFlags";
        name += $"[{((flag >> 3) & 0xffc) >> 2}]";
        name += $" with mask {1 << (Parameters[0] & 0x1f)}";
        Name = name;

        return base.Build(i, commands);
    }
}