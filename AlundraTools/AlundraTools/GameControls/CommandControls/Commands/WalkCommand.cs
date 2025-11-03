using AlundraTools.GameControls.CommandControls.Commands;

namespace AlundraEngine.DatasBin.Commands;

public class WalkCommand : CommandBase
{
    public WalkCommand(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, parameters, name, memoryAddress)
    {
        Name += $" at least {(Parameters[0] | (Parameters[1] << 8))} pixels";
    }

    protected override string PrintParameters()
    {
        return (Parameters[0] | (Parameters[1] << 8)).ToString();
    }
}