using AlundraEngine;
using AlundraEngine.DatasBin;

namespace AlundraTools.GameControls.CommandControls.Commands;

public class SetFlagCommand : CommandBase
{
    public SetFlagCommand(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, parameters, name, memoryAddress)
    {
    }

    public override string PrintParameters()
    {
        return (Parameters[0] | (Parameters[1] << 8)).ToString("x4").TrimStart('0');
    }

    //public override string Description(List<SiCommand> commands)
    //{
    //    var flag = (uint)(Parameters[0] + Parameters[1] * 0x100);
    //    var description = "Set the flag ";
    //    var flagName = (flag & 0x8000) == 0 ? nameof(StaticVariables.g_mapFlags) : nameof(StaticVariables.g_globalFlags);
    //    flagName += $"[{(flag >> 3) & 0xffc}]";
    //    description += flagName;
    //
    //    if (Command == 0x5)
    //    {
    //        description += $" |= {1 << (Parameters[0] & 0x1f)}";
    //    }
    //    else //0x6
    //    {
    //        description += $" = {flagName} & {~(1 << (Parameters[0] & 0x1f))}";
    //    }
    //        
    //    return description + base.Description(commands);
    //}
}