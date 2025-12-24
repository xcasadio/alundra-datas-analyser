using AlundraTools.GameControls.CommandControls.Commands;

namespace AlundraEngine.DatasBin.Commands;

public class WalkCommand : CommandBase
{
    public WalkCommand(byte command, byte[] parameters, string name, int offset)
        : base(command, parameters, name, offset)
    {
        Name += $" at least {(Parameters[0] | (Parameters[1] << 8))} pixels";
    }

    protected override string PrintParameters()
    {
        return (Parameters[0] | (Parameters[1] << 8)).ToString();
    }
}