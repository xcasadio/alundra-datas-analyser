using AlundraEngine.DatasBin;

namespace AlundraTools.GameControls.CommandControls.Commands;

public class WaitFlagCommand : CommandBase
{
    public WaitFlagCommand(byte code, byte[] parameters, string name, int offset) :
        base(code, parameters, name, offset)

    {
    }

    public override int Build(int i, List<SiCommand> commands)
    {
        var name = "Wait ";
        var flag = (uint)(Parameters[0] + Parameters[1] * 0x100);
        name += (flag & 0x8000) == 0 ? "MapFlags" : "GlobalFlags";
        name += $"[{((flag >> 3) & 0xffc) >> 2}]";
        name += $" & {1 << (Parameters[0] & 0x1f)} is ";
        name += $"{(Command == 0x36 ? "off" : "on")}";
        Name = name;

        return base.Build(i, commands);
    }
}